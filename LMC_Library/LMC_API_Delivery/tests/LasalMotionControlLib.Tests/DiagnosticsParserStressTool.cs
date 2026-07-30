using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal sealed class DiagnosticsParserStressOptions
    {
        internal const int MinimumIterations = 8;
        internal const int MaximumIterations = 1000000;

        internal uint Seed { get; private set; }
        internal int Iterations { get; private set; }
        internal bool ShowHelp { get; private set; }

        internal static DiagnosticsParserStressOptions Parse(string[] args)
        {
            if (args == null
                || args.Length == 0
                || !string.Equals(
                    args[0],
                    "parser-stress",
                    StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "The first argument must be the exact token 'parser-stress'.");
            }

            var options = new DiagnosticsParserStressOptions();
            var sawSeed = false;
            var sawIterations = false;
            var sawHelp = false;
            for (var index = 1; index < args.Length; index++)
            {
                var argument = args[index];
                if (string.Equals(argument, "--help", StringComparison.Ordinal))
                {
                    if (sawHelp)
                    {
                        throw new ArgumentException(
                            "--help may be specified exactly once.");
                    }

                    sawHelp = true;
                    options.ShowHelp = true;
                }
                else if (string.Equals(argument, "--seed", StringComparison.Ordinal))
                {
                    if (sawSeed)
                    {
                        throw new ArgumentException(
                            "--seed may be specified exactly once.");
                    }

                    options.Seed = ParseUInt32(
                        ReadValue(args, ref index, argument),
                        argument);
                    sawSeed = true;
                }
                else if (string.Equals(
                    argument,
                    "--iterations",
                    StringComparison.Ordinal))
                {
                    if (sawIterations)
                    {
                        throw new ArgumentException(
                            "--iterations may be specified exactly once.");
                    }

                    options.Iterations = ParseIterations(
                        ReadValue(args, ref index, argument));
                    sawIterations = true;
                }
                else
                {
                    throw new ArgumentException(
                        "Unknown parser-stress argument '" + argument + "'.");
                }
            }

            if (options.ShowHelp)
            {
                if (args.Length != 2)
                {
                    throw new ArgumentException(
                        "--help must be the only parser-stress option.");
                }

                return options;
            }

            if (!sawSeed || !sawIterations)
            {
                throw new ArgumentException(
                    "parser-stress requires exactly one --seed and one --iterations value.");
            }

            return options;
        }

        private static string ReadValue(
            string[] args,
            ref int index,
            string option)
        {
            if (index + 1 >= args.Length)
            {
                throw new ArgumentException(option + " requires a value.");
            }

            index++;
            if (string.IsNullOrWhiteSpace(args[index]))
            {
                throw new ArgumentException(
                    option + " requires a non-empty value.");
            }

            return args[index];
        }

        private static uint ParseUInt32(string value, string option)
        {
            uint parsed;
            var isHex = value.StartsWith(
                "0x",
                StringComparison.OrdinalIgnoreCase);
            var digits = isHex ? value.Substring(2) : value;
            var style = isHex
                ? NumberStyles.AllowHexSpecifier
                : NumberStyles.None;
            if (digits.Length == 0
                || !uint.TryParse(
                    digits,
                    style,
                    CultureInfo.InvariantCulture,
                    out parsed))
            {
                throw new ArgumentException(
                    option + " requires an unsigned 32-bit decimal or 0x hexadecimal value.");
            }

            return parsed;
        }

        private static int ParseIterations(string value)
        {
            int parsed;
            if (!int.TryParse(
                    value,
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out parsed)
                || parsed < MinimumIterations
                || parsed > MaximumIterations)
            {
                throw new ArgumentException(
                    "--iterations must be between "
                    + MinimumIterations
                    + " and "
                    + MaximumIterations
                    + ".");
            }

            return parsed;
        }
    }

    internal sealed class DiagnosticsParserStressFailure
    {
        internal DiagnosticsParserStressFailure(
            uint seed,
            int iteration,
            string family,
            byte[] payload,
            Exception error)
        {
            Seed = seed;
            Iteration = iteration;
            Family = family;
            Payload = payload == null ? new byte[0] : (byte[])payload.Clone();
            Error = error;
        }

        internal uint Seed { get; private set; }
        internal int Iteration { get; private set; }
        internal string Family { get; private set; }
        internal byte[] Payload { get; private set; }
        internal Exception Error { get; private set; }
    }

    internal sealed class DiagnosticsParserStressResult
    {
        internal DiagnosticsParserStressResult(int iterations, int familyCount)
        {
            Iterations = iterations;
            FamilyIterations = new int[familyCount];
        }

        internal int Iterations { get; private set; }
        internal int Accepted { get; set; }
        internal int RejectedInvalidData { get; set; }
        internal int MaximumObservedRawBytes { get; set; }
        internal int[] FamilyIterations { get; private set; }
        internal DiagnosticsParserStressFailure Failure { get; set; }
        internal bool Passed { get { return Failure == null; } }
    }

    internal static class DiagnosticsParserStressTool
    {
        internal const int SuccessExitCode = 0;
        internal const int UsageExitCode = 2;
        internal const int VerificationFailureExitCode = 3;
        internal const int FamilyCount = 8;
        internal const int MaximumRawBytes = LMC_Frame.HeaderSize
            + LMC_DiagnosticsParser.TopologyChunkHeaderPayloadLength
            + (LMC_DiagnosticsFrame.MaxTopologyEntriesPerChunk
                * LMC_DiagnosticsParser.TopologyEntryStride);

        internal static bool IsInvocation(string[] args)
        {
            return args != null
                && args.Length > 0
                && string.Equals(
                    args[0],
                    "parser-stress",
                    StringComparison.Ordinal);
        }

        internal static int Run(string[] args)
        {
            DiagnosticsParserStressOptions options;
            try
            {
                options = DiagnosticsParserStressOptions.Parse(args);
            }
            catch (Exception error)
            {
                Console.Error.WriteLine("ERROR " + error.Message);
                WriteUsage(Console.Error);
                return UsageExitCode;
            }

            if (options.ShowHelp)
            {
                WriteUsage(Console.Out);
                return SuccessExitCode;
            }

            var progress = new DiagnosticsParserStressProgress();
            var worker = Task.Run(() => Execute(options, progress));
            var timeoutMilliseconds = GetTimeoutMilliseconds(
                options.Iterations);
            DiagnosticsParserStressResult result;
            try
            {
                if (!worker.Wait(timeoutMilliseconds))
                {
                    var timeoutFailure = progress.Snapshot(
                        options.Seed,
                        new TimeoutException(
                            "A parser invocation exceeded the bounded stress-run timeout."));
                    WriteFailure(timeoutFailure);
                    return VerificationFailureExitCode;
                }

                result = worker.Result;
            }
            catch (Exception error)
            {
                var aggregate = error as AggregateException;
                var actualError = aggregate == null
                    ? error
                    : aggregate.Flatten().InnerExceptions[0];
                WriteFailure(progress.Snapshot(options.Seed, actualError));
                return VerificationFailureExitCode;
            }

            if (!result.Passed)
            {
                WriteFailure(result.Failure);
                return VerificationFailureExitCode;
            }

            Console.WriteLine("PASS parser-stress");
            Console.WriteLine("SEED=" + options.Seed
                + " (0x" + options.Seed.ToString("X8") + ")");
            Console.WriteLine("ITERATIONS_TOTAL=" + result.Iterations);
            Console.WriteLine("FAMILIES=" + FamilyCount);
            Console.WriteLine("ACCEPTED=" + result.Accepted);
            Console.WriteLine(
                "REJECTED_INVALID_DATA=" + result.RejectedInvalidData);
            Console.WriteLine(
                "MAX_RAW_BYTES=" + result.MaximumObservedRawBytes);
            return SuccessExitCode;
        }

        internal static DiagnosticsParserStressResult Execute(
            DiagnosticsParserStressOptions options)
        {
            return Execute(options, null);
        }

        internal static void WriteUsage(TextWriter writer)
        {
            writer.WriteLine("Usage:");
            writer.WriteLine(
                "  LasalMotionControlLib.Tests.exe parser-stress --seed <u32 decimal or 0x hex> --iterations <8..1000000>");
            writer.WriteLine(
                "Iterations is the total mutation count across eight round-robin parser families; every run includes each family.");
            writer.WriteLine(
                "The runner performs no file or network I/O and stops at the first unexpected parser result.");
        }

        private static DiagnosticsParserStressResult Execute(
            DiagnosticsParserStressOptions options,
            DiagnosticsParserStressProgress progress)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            var result = new DiagnosticsParserStressResult(
                options.Iterations,
                FamilyCount);
            try
            {
                using (var corpus = new DiagnosticsParserStressCorpus())
                {
                    var families = corpus.Families;
                    for (var familyIndex = 0;
                        familyIndex < families.Count;
                        familyIndex++)
                    {
                        var family = families[familyIndex];
                        var golden = (byte[])family.GoldenRaw.Clone();
                        var failure = ExecuteCase(
                            options.Seed,
                            -1,
                            family,
                            golden,
                            progress,
                            result);
                        if (failure != null)
                        {
                            result.Failure = failure;
                            return result;
                        }
                    }

                    result.Accepted = 0;
                    result.RejectedInvalidData = 0;
                    result.MaximumObservedRawBytes = 0;

                    var random = new DiagnosticsParserStressRandom(
                        options.Seed);
                    for (var iteration = 0;
                        iteration < options.Iterations;
                        iteration++)
                    {
                        var familyIndex = iteration % families.Count;
                        var family = families[familyIndex];
                        byte[] raw;
                        try
                        {
                            raw = Mutate(family.GoldenRaw, random);
                        }
                        catch (Exception error)
                        {
                            result.Failure = new DiagnosticsParserStressFailure(
                                options.Seed,
                                iteration,
                                family.Name,
                                family.GoldenRaw,
                                error);
                            return result;
                        }

                        result.FamilyIterations[familyIndex]++;
                        var failure = ExecuteCase(
                            options.Seed,
                            iteration,
                            family,
                            raw,
                            progress,
                            result);
                        if (failure != null)
                        {
                            result.Failure = failure;
                            return result;
                        }
                    }
                }
            }
            catch (Exception error)
            {
                result.Failure = new DiagnosticsParserStressFailure(
                    options.Seed,
                    -1,
                    "setup",
                    new byte[0],
                    error);
            }

            return result;
        }

        private static DiagnosticsParserStressFailure ExecuteCase(
            uint seed,
            int iteration,
            DiagnosticsParserStressFamily family,
            byte[] raw,
            DiagnosticsParserStressProgress progress,
            DiagnosticsParserStressResult result)
        {
            if (raw.Length > MaximumRawBytes)
            {
                return new DiagnosticsParserStressFailure(
                    seed,
                    iteration,
                    family.Name,
                    raw,
                    new InvalidOperationException(
                        "The mutator exceeded the hard raw-frame limit."));
            }

            result.MaximumObservedRawBytes = Math.Max(
                result.MaximumObservedRawBytes,
                raw.Length);
            if (progress != null)
            {
                progress.Update(iteration, family.Name, raw);
            }

            try
            {
                var parsed = family.Parse(raw);
                family.Validate(parsed);
                result.Accepted++;
                return null;
            }
            catch (Exception error)
            {
                if (iteration >= 0
                    && error.GetType() == typeof(InvalidDataException))
                {
                    result.RejectedInvalidData++;
                    return null;
                }

                return new DiagnosticsParserStressFailure(
                    seed,
                    iteration,
                    family.Name,
                    raw,
                    error);
            }
        }

        private static byte[] Mutate(
            byte[] golden,
            DiagnosticsParserStressRandom random)
        {
            switch (random.NextInt(5))
            {
                case 0:
                    return FlipRandomBytes(golden, random, 1 + random.NextInt(4));
                case 1:
                    return ResizeAndFill(golden, random);
                case 2:
                    return MutateDeclaredLength(golden, random);
                case 3:
                    return FlipRandomBytes(golden, random, 5 + random.NextInt(12));
                default:
                    return ResizeAndDeclare(golden, random);
            }
        }

        private static byte[] FlipRandomBytes(
            byte[] golden,
            DiagnosticsParserStressRandom random,
            int count)
        {
            var raw = (byte[])golden.Clone();
            for (var index = 0; index < count; index++)
            {
                var offset = 2 + random.NextInt(raw.Length - 2);
                raw[offset] ^= checked((byte)(1 + random.NextInt(255)));
            }

            return raw;
        }

        private static byte[] ResizeAndFill(
            byte[] golden,
            DiagnosticsParserStressRandom random)
        {
            var length = random.NextInt(MaximumRawBytes + 1);
            if (length == golden.Length)
            {
                length = length == MaximumRawBytes ? length - 1 : length + 1;
            }

            var raw = new byte[length];
            var copied = Math.Min(golden.Length, length);
            Buffer.BlockCopy(golden, 0, raw, 0, copied);
            for (var index = copied; index < raw.Length; index++)
            {
                raw[index] = random.NextByte();
            }

            return raw;
        }

        private static byte[] MutateDeclaredLength(
            byte[] golden,
            DiagnosticsParserStressRandom random)
        {
            var raw = (byte[])golden.Clone();
            var original = TestFrame.ReadUInt16(raw, 2);
            var replacement = checked((ushort)random.NextInt(
                MaximumRawBytes - LMC_Frame.HeaderSize + 1));
            if (replacement == original)
            {
                replacement = replacement == 0
                    ? (ushort)1
                    : (ushort)(replacement - 1);
            }

            TestFrame.WriteUInt16(raw, 2, replacement);
            return raw;
        }

        private static byte[] ResizeAndDeclare(
            byte[] golden,
            DiagnosticsParserStressRandom random)
        {
            var raw = ResizeAndFill(golden, random);
            if (raw.Length == LMC_Frame.HeaderSize + 4)
            {
                Array.Resize(ref raw, raw.Length + 1);
                raw[raw.Length - 1] = random.NextByte();
            }

            if (raw.Length >= LMC_Frame.HeaderSize)
            {
                TestFrame.WriteUInt16(
                    raw,
                    2,
                    checked((ushort)(raw.Length - LMC_Frame.HeaderSize)));
            }

            return raw;
        }

        private static int GetTimeoutMilliseconds(int iterations)
        {
            var calculated = 5000L + iterations * 5L;
            return checked((int)Math.Min(300000L, calculated));
        }

        private static void WriteFailure(
            DiagnosticsParserStressFailure failure)
        {
            Console.Error.WriteLine("FAIL parser-stress");
            Console.Error.WriteLine("SEED=" + failure.Seed
                + " (0x" + failure.Seed.ToString("X8") + ")");
            Console.Error.WriteLine("ITERATION=" + failure.Iteration);
            Console.Error.WriteLine("FAMILY=" + failure.Family);
            Console.Error.WriteLine(
                "PAYLOAD_HEX=" + TestFrame.ToHex(failure.Payload));
            Console.Error.WriteLine(
                "EXCEPTION_TYPE=" + failure.Error.GetType().FullName);
            Console.Error.WriteLine(
                "EXCEPTION_MESSAGE=" + failure.Error.Message);
        }
    }

    internal sealed class DiagnosticsParserStressFamily
    {
        internal DiagnosticsParserStressFamily(
            string name,
            byte[] goldenRaw,
            Func<byte[], object> parse,
            Action<object> validate)
        {
            Name = name;
            GoldenRaw = goldenRaw;
            Parse = parse;
            Validate = validate;
        }

        internal string Name { get; private set; }
        internal byte[] GoldenRaw { get; private set; }
        internal Func<byte[], object> Parse { get; private set; }
        internal Action<object> Validate { get; private set; }
    }

    internal sealed class DiagnosticsParserStressCorpus : IDisposable
    {
        private readonly LMCConnection connection = new LMCConnection();

        internal DiagnosticsParserStressCorpus()
        {
            var requestId = DiagnosticsParserDeterministicFuzzTests.RequestId;
            var topologyRevision =
                DiagnosticsParserDeterministicFuzzTests.TopologyRevision;
            var nodeId = DiagnosticsParserDeterministicFuzzTests.NodeId;
            var ioReference = DiagnosticsParserDeterministicFuzzTests.IOReference;
            var inputRequest = new LMCDigitalIOReadRequest(
                topologyRevision,
                ioReference,
                LMCDigitalIODirection.Input,
                16);
            var outputRequest = new LMCDigitalIOReadRequest(
                topologyRevision,
                ioReference,
                LMCDigitalIODirection.Output,
                64);
            var ticket = new LMCOperationTicket(
                DiagnosticsParserDeterministicFuzzTests.TicketId,
                LMCOperationKind.SDORead,
                100,
                DiagnosticsParserDeterministicFuzzTests.DiagnosticsBootId,
                DiagnosticsParserDeterministicFuzzTests.SubmissionRevision,
                0,
                connection.Diagnostics,
                true,
                4,
                LMCSignalValueType.UInt32);
            var recorderConfiguration =
                DiagnosticsParserDeterministicFuzzTests
                    .CreateRecoverableDoubleConfiguration();
            var recorderCapabilities =
                DiagnosticsParserDeterministicFuzzTests
                    .CreateRecoverableCapabilities();

            Families = new List<DiagnosticsParserStressFamily>
            {
                new DiagnosticsParserStressFamily(
                    "topology-info",
                    TestFrame.Response(
                        0,
                        DiagnosticsParserDeterministicFuzzTests
                            .CreateTopologyInfoPayload()),
                    raw => LMC_DiagnosticsParser.ParseEtherCATTopologyInfo(
                        raw,
                        requestId),
                    value => DiagnosticsParserDeterministicFuzzTests
                        .ValidateTopologyInfo(
                            (LMCEtherCATTopologyInfo)value)),
                new DiagnosticsParserStressFamily(
                    "topology-chunk",
                    TestFrame.Response(
                        0,
                        DiagnosticsParserDeterministicFuzzTests
                            .CreateTopologyChunkPayload()),
                    raw => LMC_DiagnosticsParser.ParseEtherCATTopologyChunk(
                        raw,
                        requestId,
                        topologyRevision,
                        0,
                        1),
                    value => DiagnosticsParserDeterministicFuzzTests
                        .ValidateTopologyChunk(
                            (LMCEtherCATTopologyChunk)value,
                            0,
                            1)),
                new DiagnosticsParserStressFamily(
                    "node-health",
                    TestFrame.Response(
                        0,
                        DiagnosticsParserDeterministicFuzzTests
                            .CreateNodeHealthPayload()),
                    raw => LMC_DiagnosticsParser.ParseEtherCATNodeHealth(
                        raw,
                        requestId,
                        topologyRevision,
                        nodeId),
                    value => DiagnosticsParserDeterministicFuzzTests
                        .ValidateNodeHealth(
                            (LMCEtherCATNodeHealth)value)),
                new DiagnosticsParserStressFamily(
                    "digital-input",
                    TestFrame.Response(
                        0,
                        DiagnosticsParserDeterministicFuzzTests
                            .CreateDigitalIoPayload(
                                LMCDigitalIODirection.Input,
                                16,
                                LMCDigitalIOStatusFlags.Valid,
                                0xA55Au,
                                0xFFFFu,
                                0)),
                    raw => LMC_DiagnosticsParser.ParseDigitalIO(
                        raw,
                        requestId,
                        inputRequest),
                    value => DiagnosticsParserDeterministicFuzzTests
                        .ValidateDigitalIo(
                            (LMCDigitalIOValue)value,
                            inputRequest)),
                new DiagnosticsParserStressFamily(
                    "digital-output",
                    TestFrame.Response(
                        0,
                        DiagnosticsParserDeterministicFuzzTests
                            .CreateDigitalIoPayload(
                                LMCDigitalIODirection.Output,
                                64,
                                LMCDigitalIOStatusFlags.Valid,
                                0x1122334455667788UL,
                                ulong.MaxValue,
                                1)),
                    raw => LMC_DiagnosticsParser.ParseDigitalIO(
                        raw,
                        requestId,
                        outputRequest),
                    value => DiagnosticsParserDeterministicFuzzTests
                        .ValidateDigitalIo(
                            (LMCDigitalIOValue)value,
                            outputRequest)),
                new DiagnosticsParserStressFamily(
                    "d5-variable-inline",
                    TestFrame.Response(
                        0,
                        DiagnosticsParserDeterministicFuzzTests
                            .CreateD5VariableInlineStatusPayload(ticket)),
                    raw => LMC_DiagnosticsParser.ParseOperationStatus(
                        raw,
                        requestId,
                        ticket),
                    value => DiagnosticsParserDeterministicFuzzTests
                        .ValidateD5Status(
                            (LMCOperationStatus)value,
                            ticket)),
                new DiagnosticsParserStressFamily(
                    "recorder-recoverable-configure",
                    TestFrame.Response(
                        0,
                        DiagnosticsParserDeterministicFuzzTests
                            .CreateRecoverableConfigurePayload()),
                    raw => LMC_DiagnosticsParser
                        .ParseConfigureRecoverableDoubleRecorder(
                            raw,
                            requestId,
                            recorderConfiguration,
                            DiagnosticsParserDeterministicFuzzTests
                                .RecorderRecoveryToken,
                            recorderCapabilities,
                            7,
                            null),
                    value => DiagnosticsParserDeterministicFuzzTests
                        .ValidateRecoverableConfigure(
                            (LMCRecorderConfigurationHandle)value)),
                new DiagnosticsParserStressFamily(
                    "recorder-recoverable-inventory",
                    TestFrame.Response(
                        0,
                        DiagnosticsParserDeterministicFuzzTests
                            .CreateRecoverableRecorderBankInventoryPayload()),
                    raw => LMC_DiagnosticsParser
                        .ParseRecoverableRecorderBankInventory(
                            raw,
                            requestId,
                            DiagnosticsParserDeterministicFuzzTests
                                .DiagnosticsBootId,
                            DiagnosticsParserDeterministicFuzzTests
                                .RecorderConfigId,
                            topologyRevision,
                            DiagnosticsParserDeterministicFuzzTests
                                .RecorderRecoveryToken),
                    value => DiagnosticsParserDeterministicFuzzTests
                        .ValidateRecoverableInventory(
                            (LMCRecorderBankInventory)value))
            };
        }

        internal IList<DiagnosticsParserStressFamily> Families
        {
            get;
            private set;
        }

        public void Dispose()
        {
            connection.Dispose();
        }
    }

    internal sealed class DiagnosticsParserStressRandom
    {
        private uint state;

        internal DiagnosticsParserStressRandom(uint seed)
        {
            state = seed == 0 ? 0x6D2B79F5u : seed;
        }

        internal uint NextUInt32()
        {
            var value = state;
            value ^= value << 13;
            value ^= value >> 17;
            value ^= value << 5;
            state = value;
            return value;
        }

        internal int NextInt(int exclusiveMaximum)
        {
            if (exclusiveMaximum <= 0)
            {
                throw new ArgumentOutOfRangeException("exclusiveMaximum");
            }

            return checked((int)(NextUInt32()
                % checked((uint)exclusiveMaximum)));
        }

        internal byte NextByte()
        {
            return unchecked((byte)NextUInt32());
        }
    }

    internal sealed class DiagnosticsParserStressProgress
    {
        private readonly object sync = new object();
        private int iteration = -1;
        private string family = "setup";
        private byte[] payload = new byte[0];

        internal void Update(
            int currentIteration,
            string currentFamily,
            byte[] currentPayload)
        {
            lock (sync)
            {
                iteration = currentIteration;
                family = currentFamily;
                payload = currentPayload;
            }
        }

        internal DiagnosticsParserStressFailure Snapshot(
            uint seed,
            Exception error)
        {
            lock (sync)
            {
                return new DiagnosticsParserStressFailure(
                    seed,
                    iteration,
                    family,
                    payload,
                    error);
            }
        }
    }
}
