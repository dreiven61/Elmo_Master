using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;

namespace LasalMotionControlApiExample
{
    internal enum TopologyIoLiveEvidenceOrigin
    {
        Auto = 1,
        Manual = 2
    }

    internal enum TopologyIoLiveEvidenceKind
    {
        Health = 1,
        DI = 2
    }

    internal enum TopologyIoLiveEvidenceOutcome
    {
        Success = 1,
        Failure = 2
    }

    internal sealed class TopologyIoLiveEvidenceContext
    {
        internal TopologyIoLiveEvidenceContext(
            TopologyIoLiveEvidenceOrigin origin,
            TopologyIoLiveEvidenceKind kind,
            string endpoint,
            long sessionGeneration,
            uint diagnosticsBootId,
            uint mapRevision,
            uint capabilityBits,
            string topologyLoadOrigin,
            uint topologyRevision,
            uint nodeId,
            string nodeName,
            ushort? topologyIndex,
            ushort? masterSlaveIndex,
            ushort? slotIndex,
            uint? ioReference,
            string request,
            uint? requestId,
            string requestedDirection,
            byte? requestedBitWidth)
        {
            ValidateOrigin(origin);
            ValidateKind(kind);
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentException(
                    "A normalized PLC endpoint is required.",
                    "endpoint");
            }

            if (sessionGeneration <= 0)
            {
                throw new ArgumentOutOfRangeException("sessionGeneration");
            }

            if (diagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException("diagnosticsBootId");
            }

            if (mapRevision == 0)
            {
                throw new ArgumentOutOfRangeException("mapRevision");
            }

            if (capabilityBits == 0)
            {
                throw new ArgumentOutOfRangeException("capabilityBits");
            }

            if (string.IsNullOrWhiteSpace(topologyLoadOrigin))
            {
                throw new ArgumentException(
                    "The configured-topology load origin is required.",
                    "topologyLoadOrigin");
            }

            if (topologyRevision == 0)
            {
                throw new ArgumentOutOfRangeException("topologyRevision");
            }

            if (nodeId == 0)
            {
                throw new ArgumentOutOfRangeException("nodeId");
            }

            if (nodeName == null)
            {
                throw new ArgumentNullException("nodeName");
            }

            if (string.IsNullOrWhiteSpace(request))
            {
                throw new ArgumentException(
                    "A live topology or I/O request description is required.",
                    "request");
            }

            if (kind == TopologyIoLiveEvidenceKind.DI)
            {
                if (!ioReference.HasValue || ioReference.Value == 0)
                {
                    throw new ArgumentOutOfRangeException("ioReference");
                }

                if (string.IsNullOrWhiteSpace(requestedDirection))
                {
                    throw new ArgumentException(
                        "A digital-I/O request direction is required.",
                        "requestedDirection");
                }

                if (!requestedBitWidth.HasValue
                    || requestedBitWidth.Value == 0
                    || requestedBitWidth.Value > 64)
                {
                    throw new ArgumentOutOfRangeException(
                        "requestedBitWidth");
                }
            }

            Origin = origin;
            Kind = kind;
            Endpoint = endpoint.Trim();
            SessionGeneration = sessionGeneration;
            DiagnosticsBootId = diagnosticsBootId;
            MapRevision = mapRevision;
            CapabilityBits = capabilityBits;
            TopologyLoadOrigin = topologyLoadOrigin.Trim();
            TopologyRevision = topologyRevision;
            NodeId = nodeId;
            NodeName = nodeName;
            TopologyIndex = topologyIndex;
            MasterSlaveIndex = masterSlaveIndex;
            SlotIndex = slotIndex;
            IOReference = ioReference;
            Request = request.Trim();
            RequestId = requestId;
            RequestedDirection = NormalizeOptional(requestedDirection);
            RequestedBitWidth = requestedBitWidth;
        }

        internal TopologyIoLiveEvidenceOrigin Origin { get; private set; }
        internal TopologyIoLiveEvidenceKind Kind { get; private set; }
        internal string Endpoint { get; private set; }
        internal long SessionGeneration { get; private set; }
        internal uint DiagnosticsBootId { get; private set; }
        internal uint MapRevision { get; private set; }
        internal uint CapabilityBits { get; private set; }
        internal string TopologyLoadOrigin { get; private set; }
        internal uint TopologyRevision { get; private set; }
        internal uint NodeId { get; private set; }
        internal string NodeName { get; private set; }
        internal ushort? TopologyIndex { get; private set; }
        internal ushort? MasterSlaveIndex { get; private set; }
        internal ushort? SlotIndex { get; private set; }
        internal uint? IOReference { get; private set; }
        internal string Request { get; private set; }
        internal uint? RequestId { get; private set; }
        internal string RequestedDirection { get; private set; }
        internal byte? RequestedBitWidth { get; private set; }

        private static string NormalizeOptional(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static void ValidateOrigin(
            TopologyIoLiveEvidenceOrigin value)
        {
            if (value != TopologyIoLiveEvidenceOrigin.Auto
                && value != TopologyIoLiveEvidenceOrigin.Manual)
            {
                throw new ArgumentOutOfRangeException("origin");
            }
        }

        private static void ValidateKind(TopologyIoLiveEvidenceKind value)
        {
            if (value != TopologyIoLiveEvidenceKind.Health
                && value != TopologyIoLiveEvidenceKind.DI)
            {
                throw new ArgumentOutOfRangeException("kind");
            }
        }
    }

    internal sealed class TopologyIoLiveEvidenceRecord
    {
        private TopologyIoLiveEvidenceRecord(
            ulong journalSequence,
            DateTime recordedUtc,
            TopologyIoLiveEvidenceContext context,
            TopologyIoLiveEvidenceOutcome outcome,
            uint? cycleCounter,
            uint? plcSnapshotSequence,
            ulong? plcTimestampMicroseconds,
            string quality,
            bool? dataValid,
            ulong? value,
            ulong? validMask,
            string direction,
            byte? bitWidth,
            uint? outputRevision,
            bool? online,
            byte? etherCATState,
            ushort? alStatusCode,
            uint? slaveState,
            uint? classState,
            uint? ds402StatusWord,
            uint? axisError,
            uint? lastValidCycle,
            uint? lastStateChangeCycle,
            string errorType,
            string errorMessage)
        {
            if (recordedUtc.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException(
                    "Live evidence time must be UTC.",
                    "recordedUtc");
            }

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            Context = context;
            JournalSequence = journalSequence;
            RecordedUtc = recordedUtc;
            Outcome = outcome;
            CycleCounter = cycleCounter;
            PlcSnapshotSequence = plcSnapshotSequence;
            PlcTimestampMicroseconds = plcTimestampMicroseconds;
            Quality = quality;
            DataValid = dataValid;
            Value = value;
            ValidMask = validMask;
            Direction = direction;
            BitWidth = bitWidth;
            OutputRevision = outputRevision;
            Online = online;
            EtherCATState = etherCATState;
            ALStatusCode = alStatusCode;
            SlaveState = slaveState;
            ClassState = classState;
            DS402StatusWord = ds402StatusWord;
            AxisError = axisError;
            LastValidCycle = lastValidCycle;
            LastStateChangeCycle = lastStateChangeCycle;
            ErrorType = errorType;
            ErrorMessage = errorMessage;
        }

        internal ulong JournalSequence { get; private set; }
        internal DateTime RecordedUtc { get; private set; }
        internal TopologyIoLiveEvidenceContext Context { get; private set; }
        internal TopologyIoLiveEvidenceOutcome Outcome { get; private set; }
        internal uint? CycleCounter { get; private set; }
        internal uint? PlcSnapshotSequence { get; private set; }
        internal ulong? PlcTimestampMicroseconds { get; private set; }
        internal string Quality { get; private set; }
        internal bool? DataValid { get; private set; }
        internal ulong? Value { get; private set; }
        internal ulong? ValidMask { get; private set; }
        internal string Direction { get; private set; }
        internal byte? BitWidth { get; private set; }
        internal uint? OutputRevision { get; private set; }
        internal bool? Online { get; private set; }
        internal byte? EtherCATState { get; private set; }
        internal ushort? ALStatusCode { get; private set; }
        internal uint? SlaveState { get; private set; }
        internal uint? ClassState { get; private set; }
        internal uint? DS402StatusWord { get; private set; }
        internal uint? AxisError { get; private set; }
        internal uint? LastValidCycle { get; private set; }
        internal uint? LastStateChangeCycle { get; private set; }
        internal string ErrorType { get; private set; }
        internal string ErrorMessage { get; private set; }

        internal static TopologyIoLiveEvidenceRecord CreateHealthSuccess(
            TopologyIoLiveEvidenceContext context,
            DateTime recordedUtc,
            uint cycleCounter,
            uint plcSnapshotSequence,
            ulong plcTimestampMicroseconds,
            string quality,
            bool dataValid,
            bool? online,
            byte? etherCATState,
            ushort? alStatusCode,
            uint? slaveState,
            uint? classState,
            uint? ds402StatusWord,
            uint? axisError,
            uint? lastValidCycle,
            uint? lastStateChangeCycle)
        {
            ValidateContextKind(context, TopologyIoLiveEvidenceKind.Health);
            ValidateQuality(quality);
            return new TopologyIoLiveEvidenceRecord(
                0,
                recordedUtc,
                context,
                TopologyIoLiveEvidenceOutcome.Success,
                cycleCounter,
                plcSnapshotSequence,
                plcTimestampMicroseconds,
                quality.Trim(),
                dataValid,
                null,
                null,
                null,
                null,
                null,
                online,
                etherCATState,
                alStatusCode,
                slaveState,
                classState,
                ds402StatusWord,
                axisError,
                lastValidCycle,
                lastStateChangeCycle,
                null,
                null);
        }

        internal static TopologyIoLiveEvidenceRecord CreateDigitalInputSuccess(
            TopologyIoLiveEvidenceContext context,
            DateTime recordedUtc,
            uint cycleCounter,
            uint? plcSnapshotSequence,
            ulong? plcTimestampMicroseconds,
            string quality,
            bool dataValid,
            ulong value,
            ulong validMask,
            string direction,
            byte bitWidth,
            uint? outputRevision)
        {
            ValidateContextKind(context, TopologyIoLiveEvidenceKind.DI);
            ValidateQuality(quality);
            if (string.IsNullOrWhiteSpace(direction))
            {
                throw new ArgumentException(
                    "The returned digital-I/O direction is required.",
                    "direction");
            }

            if (bitWidth == 0 || bitWidth > 64)
            {
                throw new ArgumentOutOfRangeException("bitWidth");
            }

            return new TopologyIoLiveEvidenceRecord(
                0,
                recordedUtc,
                context,
                TopologyIoLiveEvidenceOutcome.Success,
                cycleCounter,
                plcSnapshotSequence,
                plcTimestampMicroseconds,
                quality.Trim(),
                dataValid,
                value,
                validMask,
                direction.Trim(),
                bitWidth,
                outputRevision,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);
        }

        internal static TopologyIoLiveEvidenceRecord CreateFailure(
            TopologyIoLiveEvidenceContext context,
            DateTime recordedUtc,
            string errorType,
            string errorMessage)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (string.IsNullOrWhiteSpace(errorType))
            {
                throw new ArgumentException(
                    "A failure type is required.",
                    "errorType");
            }

            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                throw new ArgumentException(
                    "A failure message is required.",
                    "errorMessage");
            }

            // A failure deliberately carries no sample fields. In particular,
            // it must never inherit values from the last successful response.
            return new TopologyIoLiveEvidenceRecord(
                0,
                recordedUtc,
                context,
                TopologyIoLiveEvidenceOutcome.Failure,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                errorType.Trim(),
                errorMessage.Trim());
        }

        internal TopologyIoLiveEvidenceRecord WithJournalSequence(
            ulong sequence)
        {
            if (sequence == 0)
            {
                throw new ArgumentOutOfRangeException("sequence");
            }

            return new TopologyIoLiveEvidenceRecord(
                sequence,
                RecordedUtc,
                Context,
                Outcome,
                CycleCounter,
                PlcSnapshotSequence,
                PlcTimestampMicroseconds,
                Quality,
                DataValid,
                Value,
                ValidMask,
                Direction,
                BitWidth,
                OutputRevision,
                Online,
                EtherCATState,
                ALStatusCode,
                SlaveState,
                ClassState,
                DS402StatusWord,
                AxisError,
                LastValidCycle,
                LastStateChangeCycle,
                ErrorType,
                ErrorMessage);
        }

        private static void ValidateContextKind(
            TopologyIoLiveEvidenceContext context,
            TopologyIoLiveEvidenceKind expected)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (context.Kind != expected)
            {
                throw new ArgumentException(
                    "The live evidence context kind does not match the sample.",
                    "context");
            }
        }

        private static void ValidateQuality(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "Parsed PLC sample quality is required.",
                    "quality");
            }
        }
    }

    internal sealed class TopologyIoLiveEvidenceJournal
    {
        internal const int MaximumCapacity = 4096;

        private readonly object sync = new object();
        private readonly int capacity;
        private readonly Queue<TopologyIoLiveEvidenceRecord> records;
        private ulong lastSequence;
        private ulong droppedOldestCount;

        internal TopologyIoLiveEvidenceJournal()
            : this(MaximumCapacity)
        {
        }

        internal TopologyIoLiveEvidenceJournal(int capacity)
        {
            if (capacity <= 0 || capacity > MaximumCapacity)
            {
                throw new ArgumentOutOfRangeException("capacity");
            }

            this.capacity = capacity;
            records = new Queue<TopologyIoLiveEvidenceRecord>(capacity);
        }

        internal TopologyIoLiveEvidenceRecord Append(
            TopologyIoLiveEvidenceRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            lock (sync)
            {
                lastSequence = checked(lastSequence + 1);
                var sequenced = record.WithJournalSequence(lastSequence);
                if (records.Count == capacity)
                {
                    records.Dequeue();
                    droppedOldestCount = checked(droppedOldestCount + 1);
                }

                records.Enqueue(sequenced);
                return sequenced;
            }
        }

        internal TopologyIoLiveEvidenceSnapshot CaptureSnapshot()
        {
            lock (sync)
            {
                return new TopologyIoLiveEvidenceSnapshot(
                    capacity,
                    droppedOldestCount,
                    lastSequence,
                    new List<TopologyIoLiveEvidenceRecord>(records));
            }
        }
    }

    internal sealed class TopologyIoLiveEvidenceSnapshot
    {
        internal const string BoundaryLine =
            "BOUNDARY=current-session gate passed before commit; successes are PLC responses parsed by the PC; failures are read-attempt evidence with no copied sample fields";
        internal const string NotProofLine =
            "NOT PROOF=physical cable order, actual DI voltage or contact, physical DO feedback, or PLC implementation completeness";

        private readonly ReadOnlyCollection<TopologyIoLiveEvidenceRecord>
            records;

        internal TopologyIoLiveEvidenceSnapshot(
            int capacity,
            ulong droppedOldestCount,
            ulong lastSequence,
            IList<TopologyIoLiveEvidenceRecord> records)
        {
            Capacity = capacity;
            DroppedOldestCount = droppedOldestCount;
            LastSequence = lastSequence;
            this.records = new ReadOnlyCollection<TopologyIoLiveEvidenceRecord>(
                new List<TopologyIoLiveEvidenceRecord>(records));
        }

        internal int Capacity { get; private set; }
        internal ulong DroppedOldestCount { get; private set; }
        internal ulong LastSequence { get; private set; }
        internal IReadOnlyList<TopologyIoLiveEvidenceRecord> Records
        {
            get { return records; }
        }

        internal string BuildTextExport()
        {
            var text = new StringBuilder();
            text.AppendLine("ELMO WPF LIVE ETHERCAT TOPOLOGY/IO EVIDENCE");
            text.AppendLine(BoundaryLine);
            text.AppendLine(NotProofLine);
            text.Append("Capacity=");
            text.AppendLine(Capacity.ToString(CultureInfo.InvariantCulture));
            text.Append("RetainedCount=");
            text.AppendLine(records.Count.ToString(CultureInfo.InvariantCulture));
            text.Append("DroppedOldestCount=");
            text.AppendLine(DroppedOldestCount.ToString(
                CultureInfo.InvariantCulture));
            text.Append("LastSequence=");
            text.AppendLine(LastSequence.ToString(CultureInfo.InvariantCulture));

            foreach (var record in records)
            {
                text.AppendLine();
                text.Append("[RECORD ");
                text.Append(record.JournalSequence.ToString(
                    CultureInfo.InvariantCulture));
                text.AppendLine("]");
                AppendTextRecord(text, record);
            }

            return text.ToString();
        }

        internal string BuildCsvExport()
        {
            var text = new StringBuilder();
            text.AppendLine(
                "JournalSequence,RecordedUtc,Origin,Kind,Outcome,Endpoint,SessionGeneration,DiagnosticsBootId,MapRevision,CapabilityBits,TopologyLoadOrigin,TopologyRevision,NodeId,NodeName,TopologyIndex,MasterSlaveIndex,SlotIndex,IOReference,Request,RequestId,RequestedDirection,RequestedBitWidth,CycleCounter,PlcSnapshotSequence,PlcTimestampMicroseconds,Quality,DataValid,Value,ValidMask,Direction,BitWidth,OutputRevision,Online,EtherCATState,ALStatusCode,SlaveState,ClassState,DS402StatusWord,AxisError,LastValidCycle,LastStateChangeCycle,ErrorType,ErrorMessage,JournalCapacity,DroppedOldestCount,SnapshotLastSequence,EvidenceBoundary,NotProof");
            foreach (var record in records)
            {
                AppendCsvRecord(text, record);
            }

            return text.ToString();
        }

        private void AppendCsvRecord(
            StringBuilder text,
            TopologyIoLiveEvidenceRecord record)
        {
            var context = record.Context;
            var fields = new[]
            {
                Format(record.JournalSequence),
                record.RecordedUtc.ToString("O", CultureInfo.InvariantCulture),
                context.Origin.ToString(),
                context.Kind.ToString(),
                record.Outcome.ToString(),
                context.Endpoint,
                Format(context.SessionGeneration),
                FormatHex32(context.DiagnosticsBootId),
                FormatHex32(context.MapRevision),
                FormatHex32(context.CapabilityBits),
                context.TopologyLoadOrigin,
                FormatHex32(context.TopologyRevision),
                FormatHex32(context.NodeId),
                context.NodeName,
                Format(context.TopologyIndex),
                Format(context.MasterSlaveIndex),
                Format(context.SlotIndex),
                FormatHex32(context.IOReference),
                context.Request,
                Format(context.RequestId),
                context.RequestedDirection,
                Format(context.RequestedBitWidth),
                Format(record.CycleCounter),
                Format(record.PlcSnapshotSequence),
                Format(record.PlcTimestampMicroseconds),
                record.Quality,
                Format(record.DataValid),
                FormatHex64(record.Value),
                FormatHex64(record.ValidMask),
                record.Direction,
                Format(record.BitWidth),
                Format(record.OutputRevision),
                Format(record.Online),
                FormatHex8(record.EtherCATState),
                FormatHex16(record.ALStatusCode),
                FormatHex32(record.SlaveState),
                FormatHex32(record.ClassState),
                FormatHex32(record.DS402StatusWord),
                FormatHex32(record.AxisError),
                Format(record.LastValidCycle),
                Format(record.LastStateChangeCycle),
                record.ErrorType,
                record.ErrorMessage,
                Capacity.ToString(CultureInfo.InvariantCulture),
                DroppedOldestCount.ToString(CultureInfo.InvariantCulture),
                LastSequence.ToString(CultureInfo.InvariantCulture),
                BoundaryLine,
                NotProofLine
            };

            for (var index = 0; index < fields.Length; index++)
            {
                if (index != 0)
                {
                    text.Append(',');
                }

                text.Append(EscapeCsv(fields[index]));
            }

            text.AppendLine();
        }

        private static void AppendTextRecord(
            StringBuilder text,
            TopologyIoLiveEvidenceRecord record)
        {
            var context = record.Context;
            AppendText(text, "JournalSequence", Format(record.JournalSequence));
            AppendText(
                text,
                "RecordedUtc",
                record.RecordedUtc.ToString("O", CultureInfo.InvariantCulture));
            AppendText(text, "Origin", context.Origin.ToString());
            AppendText(text, "Kind", context.Kind.ToString());
            AppendText(text, "Outcome", record.Outcome.ToString());
            AppendText(text, "Endpoint", context.Endpoint);
            AppendText(
                text,
                "SessionGeneration",
                Format(context.SessionGeneration));
            AppendText(
                text,
                "DiagnosticsBootId",
                FormatHex32(context.DiagnosticsBootId));
            AppendText(text, "MapRevision", FormatHex32(context.MapRevision));
            AppendText(
                text,
                "CapabilityBits",
                FormatHex32(context.CapabilityBits));
            AppendText(text, "TopologyLoadOrigin", context.TopologyLoadOrigin);
            AppendText(
                text,
                "TopologyRevision",
                FormatHex32(context.TopologyRevision));
            AppendText(text, "NodeId", FormatHex32(context.NodeId));
            AppendText(text, "NodeName", context.NodeName);
            AppendText(text, "TopologyIndex", Format(context.TopologyIndex));
            AppendText(
                text,
                "MasterSlaveIndex",
                Format(context.MasterSlaveIndex));
            AppendText(text, "SlotIndex", Format(context.SlotIndex));
            AppendText(
                text,
                "IOReference",
                FormatHex32(context.IOReference));
            AppendText(text, "Request", context.Request);
            AppendText(text, "RequestId", Format(context.RequestId));
            AppendText(
                text,
                "RequestedDirection",
                context.RequestedDirection);
            AppendText(
                text,
                "RequestedBitWidth",
                Format(context.RequestedBitWidth));
            AppendText(text, "CycleCounter", Format(record.CycleCounter));
            AppendText(
                text,
                "PlcSnapshotSequence",
                Format(record.PlcSnapshotSequence));
            AppendText(
                text,
                "PlcTimestampMicroseconds",
                Format(record.PlcTimestampMicroseconds));
            AppendText(text, "Quality", record.Quality);
            AppendText(text, "DataValid", Format(record.DataValid));
            AppendText(text, "Value", FormatHex64(record.Value));
            AppendText(text, "ValidMask", FormatHex64(record.ValidMask));
            AppendText(text, "Direction", record.Direction);
            AppendText(text, "BitWidth", Format(record.BitWidth));
            AppendText(text, "OutputRevision", Format(record.OutputRevision));
            AppendText(text, "Online", Format(record.Online));
            AppendText(
                text,
                "EtherCATState",
                FormatHex8(record.EtherCATState));
            AppendText(
                text,
                "ALStatusCode",
                FormatHex16(record.ALStatusCode));
            AppendText(text, "SlaveState", FormatHex32(record.SlaveState));
            AppendText(text, "ClassState", FormatHex32(record.ClassState));
            AppendText(
                text,
                "DS402StatusWord",
                FormatHex32(record.DS402StatusWord));
            AppendText(text, "AxisError", FormatHex32(record.AxisError));
            AppendText(
                text,
                "LastValidCycle",
                Format(record.LastValidCycle));
            AppendText(
                text,
                "LastStateChangeCycle",
                Format(record.LastStateChangeCycle));
            AppendText(text, "ErrorType", record.ErrorType);
            AppendText(text, "ErrorMessage", record.ErrorMessage);
        }

        private static void AppendText(
            StringBuilder text,
            string name,
            string value)
        {
            text.Append(name);
            text.Append('=');
            text.AppendLine(EscapeText(value));
        }

        private static string EscapeText(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return value
                .Replace("\\", "\\\\")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");
        }

        private static string EscapeCsv(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0)
            {
                return value;
            }

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private static string Format(bool? value)
        {
            return value.HasValue
                ? (value.Value ? "true" : "false")
                : string.Empty;
        }

        private static string Format(byte? value)
        {
            return value.HasValue
                ? value.Value.ToString(CultureInfo.InvariantCulture)
                : string.Empty;
        }

        private static string Format(ushort? value)
        {
            return value.HasValue
                ? value.Value.ToString(CultureInfo.InvariantCulture)
                : string.Empty;
        }

        private static string Format(uint? value)
        {
            return value.HasValue
                ? value.Value.ToString(CultureInfo.InvariantCulture)
                : string.Empty;
        }

        private static string Format(ulong? value)
        {
            return value.HasValue
                ? value.Value.ToString(CultureInfo.InvariantCulture)
                : string.Empty;
        }

        private static string Format(long value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static string Format(ulong value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static string FormatHex8(byte? value)
        {
            return value.HasValue
                ? "0x" + value.Value.ToString("X2", CultureInfo.InvariantCulture)
                : string.Empty;
        }

        private static string FormatHex16(ushort? value)
        {
            return value.HasValue
                ? "0x" + value.Value.ToString("X4", CultureInfo.InvariantCulture)
                : string.Empty;
        }

        private static string FormatHex32(uint value)
        {
            return "0x" + value.ToString("X8", CultureInfo.InvariantCulture);
        }

        private static string FormatHex32(uint? value)
        {
            return value.HasValue
                ? FormatHex32(value.Value)
                : string.Empty;
        }

        private static string FormatHex64(ulong? value)
        {
            return value.HasValue
                ? "0x" + value.Value.ToString("X16", CultureInfo.InvariantCulture)
                : string.Empty;
        }
    }

    internal static class TopologyIoLiveEvidenceFile
    {
        internal static void SaveUtf8NoBom(string path, string content)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(
                    "A live evidence path is required.",
                    "path");
            }

            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            File.WriteAllText(path, content, new UTF8Encoding(false));
        }
    }
}
