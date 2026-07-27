using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace LasalMotionControlLib
{
    public enum LMCSdoSubmissionPhase
    {
        RequestValidation = 0,
        SessionPreflight = 1,
        CapabilityPreflight = 2,
        Submission = 3,
        PostSubmissionValidation = 4
    }

    public enum LMCSdoSubmissionOutcome
    {
        NotAttempted = 0,
        Rejected = 1,
        OutcomeUncertain = 2,
        Accepted = 3
    }

    /// <summary>
    /// Immutable failure context for LMCDiagnostics.SubmitSdo and
    /// SubmitSdoAsync. The original exception object and type are preserved;
    /// call TryGet with the caught exception to distinguish local preflight,
    /// explicit PLC rejection, uncertain wire outcome, and an accepted ticket.
    /// </summary>
    public sealed class LMCSdoSubmissionFailureContext
    {
        private static readonly object FailureContextSync = new object();
        private static readonly ConditionalWeakTable<
            Exception,
            LMCSdoSubmissionFailureContext> FailureContexts =
                new ConditionalWeakTable<
                    Exception,
                    LMCSdoSubmissionFailureContext>();

        internal LMCSdoSubmissionFailureContext(
            LMCSdoRequest request,
            LMCSdoSubmissionPhase phase,
            LMCSdoSubmissionOutcome submissionOutcome,
            uint diagnosticsBootId,
            uint mapRevision,
            LMCOperationTicket ticket)
        {
            if (!Enum.IsDefined(typeof(LMCSdoSubmissionPhase), phase))
            {
                throw new ArgumentOutOfRangeException("phase");
            }

            if (!Enum.IsDefined(
                typeof(LMCSdoSubmissionOutcome),
                submissionOutcome))
            {
                throw new ArgumentOutOfRangeException("submissionOutcome");
            }

            if (submissionOutcome != LMCSdoSubmissionOutcome.NotAttempted
                && request == null)
            {
                throw new ArgumentNullException(
                    "request",
                    "A dispatched SDO submission requires its request.");
            }

            if (request == null
                && phase != LMCSdoSubmissionPhase.RequestValidation)
            {
                throw new ArgumentException(
                    "A null SDO request can fail only during request validation.",
                    "phase");
            }

            if (submissionOutcome == LMCSdoSubmissionOutcome.Accepted)
            {
                if (ticket == null)
                {
                    throw new ArgumentNullException(
                        "ticket",
                        "An accepted SDO submission requires its ticket.");
                }

                if (phase
                    != LMCSdoSubmissionPhase.PostSubmissionValidation)
                {
                    throw new ArgumentException(
                        "An accepted ticket failure must occur during post-submission validation.",
                        "phase");
                }

                if (ticket.DiagnosticsBootId != diagnosticsBootId
                    || ticket.SubmissionMapRevision != mapRevision)
                {
                    throw new ArgumentException(
                        "The accepted SDO ticket does not match the submission identity.",
                        "ticket");
                }
            }
            else if (ticket != null)
            {
                throw new ArgumentException(
                    "Only an accepted SDO submission can have a ticket.",
                    "ticket");
            }

            if ((submissionOutcome == LMCSdoSubmissionOutcome.Rejected
                    || submissionOutcome
                        == LMCSdoSubmissionOutcome.OutcomeUncertain)
                && phase != LMCSdoSubmissionPhase.Submission)
            {
                throw new ArgumentException(
                    "Rejected and outcome-uncertain SDO submissions require the Submission phase.",
                    "phase");
            }

            if (submissionOutcome != LMCSdoSubmissionOutcome.NotAttempted
                && (diagnosticsBootId == 0 || mapRevision == 0))
            {
                throw new ArgumentException(
                    "A dispatched SDO submission requires its capability BootId and MapRevision.");
            }

            if (ticket != null)
            {
                var expectedKind = request.IsWrite
                    ? LMCOperationKind.SDOWrite
                    : LMCOperationKind.SDORead;
                if (ticket.OperationKind != expectedKind
                    || ticket.DiagnosticsBootId != diagnosticsBootId)
                {
                    throw new ArgumentException(
                        "The accepted ticket does not match the SDO request and capability identity.",
                        "ticket");
                }
            }

            Request = request;
            Phase = phase;
            SubmissionOutcome = submissionOutcome;
            DiagnosticsBootId = diagnosticsBootId;
            MapRevision = mapRevision;
            Ticket = ticket;
        }

        /// <summary>
        /// Gets the submitted request, or null when a null argument failed
        /// during RequestValidation.
        /// </summary>
        public LMCSdoRequest Request { get; private set; }
        public LMCSdoSubmissionPhase Phase { get; private set; }
        public LMCSdoSubmissionOutcome SubmissionOutcome { get; private set; }
        public uint DiagnosticsBootId { get; private set; }
        public uint MapRevision { get; private set; }
        public LMCOperationTicket Ticket { get; private set; }

        public static bool TryGet(
            Exception exception,
            out LMCSdoSubmissionFailureContext context)
        {
            if (exception == null)
            {
                context = null;
                return false;
            }

            return FailureContexts.TryGetValue(exception, out context);
        }

        internal static void Attach(
            Exception exception,
            LMCSdoSubmissionFailureContext context)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            lock (FailureContextSync)
            {
                FailureContexts.Remove(exception);
                FailureContexts.Add(exception, context);
            }
        }
    }

    internal interface ILMCSdoSubmissionAttemptTracker
    {
        void RecordCapabilityIdentity(uint diagnosticsBootId, uint mapRevision);
        void BeginSubmission();
        void MarkSubmissionOutcomeUncertain();
        void MarkSubmissionRejected();
        void MarkSubmissionAccepted(LMCOperationTicket ticket);
    }

    internal sealed class LMCSdoSubmissionAttemptTracker
        : ILMCSdoSubmissionAttemptTracker
    {
        private readonly object sync = new object();
        private readonly LMCSdoRequest request;
        private LMCSdoSubmissionPhase phase =
            LMCSdoSubmissionPhase.RequestValidation;
        private LMCSdoSubmissionOutcome submissionOutcome =
            LMCSdoSubmissionOutcome.NotAttempted;
        private uint diagnosticsBootId;
        private uint mapRevision;
        private LMCOperationTicket ticket;

        internal LMCSdoSubmissionAttemptTracker(LMCSdoRequest request)
        {
            this.request = request;
        }

        internal void BeginSessionPreflight()
        {
            lock (sync)
            {
                RequireState(
                    LMCSdoSubmissionPhase.RequestValidation,
                    LMCSdoSubmissionOutcome.NotAttempted);
                phase = LMCSdoSubmissionPhase.SessionPreflight;
            }
        }

        internal void BeginCapabilityPreflight()
        {
            lock (sync)
            {
                RequireState(
                    LMCSdoSubmissionPhase.SessionPreflight,
                    LMCSdoSubmissionOutcome.NotAttempted);
                phase = LMCSdoSubmissionPhase.CapabilityPreflight;
            }
        }

        public void RecordCapabilityIdentity(
            uint actualDiagnosticsBootId,
            uint actualMapRevision)
        {
            lock (sync)
            {
                RequireState(
                    LMCSdoSubmissionPhase.CapabilityPreflight,
                    LMCSdoSubmissionOutcome.NotAttempted);
                diagnosticsBootId = actualDiagnosticsBootId;
                mapRevision = actualMapRevision;
            }
        }

        public void BeginSubmission()
        {
            lock (sync)
            {
                RequireState(
                    LMCSdoSubmissionPhase.CapabilityPreflight,
                    LMCSdoSubmissionOutcome.NotAttempted);
                if (diagnosticsBootId == 0 || mapRevision == 0)
                {
                    throw new InvalidOperationException(
                        "SDO submission requires a validated capability identity.");
                }

                phase = LMCSdoSubmissionPhase.Submission;
            }
        }

        public void MarkSubmissionOutcomeUncertain()
        {
            lock (sync)
            {
                RequireState(
                    LMCSdoSubmissionPhase.Submission,
                    LMCSdoSubmissionOutcome.NotAttempted);
                submissionOutcome =
                    LMCSdoSubmissionOutcome.OutcomeUncertain;
            }
        }

        public void MarkSubmissionRejected()
        {
            lock (sync)
            {
                RequireState(
                    LMCSdoSubmissionPhase.Submission,
                    LMCSdoSubmissionOutcome.OutcomeUncertain);
                submissionOutcome = LMCSdoSubmissionOutcome.Rejected;
            }
        }

        public void MarkSubmissionAccepted(LMCOperationTicket acceptedTicket)
        {
            if (acceptedTicket == null)
            {
                throw new ArgumentNullException("acceptedTicket");
            }

            lock (sync)
            {
                RequireState(
                    LMCSdoSubmissionPhase.Submission,
                    LMCSdoSubmissionOutcome.OutcomeUncertain);
                var expectedKind = request.IsWrite
                    ? LMCOperationKind.SDOWrite
                    : LMCOperationKind.SDORead;
                if (acceptedTicket.OperationKind != expectedKind
                    || acceptedTicket.DiagnosticsBootId
                        != diagnosticsBootId
                    || acceptedTicket.SubmissionMapRevision != mapRevision)
                {
                    throw new ArgumentException(
                        "The accepted ticket does not match the tracked SDO submission.",
                        "acceptedTicket");
                }

                ticket = acceptedTicket;
                submissionOutcome = LMCSdoSubmissionOutcome.Accepted;
                phase = LMCSdoSubmissionPhase.PostSubmissionValidation;
            }
        }

        internal LMCSdoSubmissionFailureContext CreateFailureContext()
        {
            lock (sync)
            {
                return new LMCSdoSubmissionFailureContext(
                    request,
                    phase,
                    submissionOutcome,
                    diagnosticsBootId,
                    mapRevision,
                    ticket);
            }
        }

        private void RequireState(
            LMCSdoSubmissionPhase expectedPhase,
            LMCSdoSubmissionOutcome expectedOutcome)
        {
            if (phase != expectedPhase
                || submissionOutcome != expectedOutcome)
            {
                throw new InvalidOperationException(
                    "The SDO submission attempt state transition is invalid.");
            }
        }
    }

    internal static class LMCDiagnosticsSdoPolicy
    {
        internal const uint MaximumReadTimeoutCycles = 60000;

        internal static void RequireReadAllowed(
            LMCSdoRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (request.IsWrite)
            {
                throw new NotSupportedException(
                    "The active D5 SDO policy supports Read operations only.");
            }

            if (request.SlaveReference < 1 || request.SlaveReference > 4)
            {
                throw new NotSupportedException(
                    "The active D5 SDO policy supports SlaveReference 1 through 4 only.");
            }

            var expectedLength = ExpectedReadLength(request.ValueType);
            if (request.DataLength != expectedLength)
            {
                throw new NotSupportedException(
                    "The active D5 SDO policy requires 8-bit types=1 byte, 16-bit types=2 bytes, and 32-bit types=4 bytes.");
            }

            if (request.TimeoutCycles < 1
                || request.TimeoutCycles > MaximumReadTimeoutCycles)
            {
                throw new NotSupportedException(
                    "The active D5 SDO policy requires TimeoutCycles from 1 through 60000.");
            }
        }

        internal static ushort ExpectedReadLength(
            LMCSignalValueType valueType)
        {
            switch (valueType)
            {
                case LMCSignalValueType.Bool:
                case LMCSignalValueType.Int8:
                case LMCSignalValueType.UInt8:
                case LMCSignalValueType.BitField8:
                    return 1;
                case LMCSignalValueType.Int16:
                case LMCSignalValueType.UInt16:
                case LMCSignalValueType.BitField16:
                    return 2;
                case LMCSignalValueType.Int32:
                case LMCSignalValueType.UInt32:
                case LMCSignalValueType.Real32:
                case LMCSignalValueType.BitField32:
                    return 4;
                default:
                    throw new NotSupportedException(
                        "The active D5 SDO policy does not support this ValueType.");
            }
        }

        internal static bool IsLegacyFirstSliceRead(
            LMCSdoRequest request)
        {
            return request != null
                && !request.IsWrite
                && request.SlaveReference >= 1
                && request.SlaveReference <= 4
                && request.ObjectIndex == 0x1000
                && request.SubIndex == 0
                && request.ValueType == LMCSignalValueType.UInt32
                && request.DataLength == 4
                && request.TimeoutCycles >= 1
                && request.TimeoutCycles <= MaximumReadTimeoutCycles;
        }
    }

    /// <summary>
    /// Immutable compile-time SDO Write target intended to be mirrored by the
    /// PLC policy. Applications must not treat an arbitrary SDO address as a
    /// writable target, and submission still verifies both policies.
    /// </summary>
    public sealed class LMCSdoWriteTarget
    {
        internal LMCSdoWriteTarget(
            string displayName,
            ushort slaveReference,
            ushort objectIndex,
            byte subIndex,
            LMCSignalValueType valueType,
            ushort dataLength,
            long minimumIntegerValue,
            long maximumIntegerValue)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException(
                    "An SDO Write target display name is required.",
                    "displayName");
            }

            if (slaveReference == 0)
            {
                throw new ArgumentOutOfRangeException("slaveReference");
            }

            if (objectIndex == 0
                || LMCSdoRequest.IsPermanentlyUnsafeObject(objectIndex))
            {
                throw new ArgumentOutOfRangeException(
                    "objectIndex",
                    "Direct motion/control objects cannot be approved for SDO Write.");
            }

            if (valueType != LMCSignalValueType.Int32
                && valueType != LMCSignalValueType.UInt32)
            {
                throw new NotSupportedException(
                    "The active SDO Write policy supports only 32-bit integer targets.");
            }

            if (dataLength != 4)
            {
                throw new ArgumentOutOfRangeException(
                    "dataLength",
                    "The active SDO Write policy supports exactly four data bytes.");
            }

            if (minimumIntegerValue > maximumIntegerValue)
            {
                throw new ArgumentOutOfRangeException("minimumIntegerValue");
            }

            if (valueType == LMCSignalValueType.Int32
                && (minimumIntegerValue < int.MinValue
                    || maximumIntegerValue > int.MaxValue))
            {
                throw new ArgumentOutOfRangeException(
                    "maximumIntegerValue",
                    "The target range does not fit Int32.");
            }

            if (valueType == LMCSignalValueType.UInt32
                && (minimumIntegerValue < uint.MinValue
                    || maximumIntegerValue > uint.MaxValue))
            {
                throw new ArgumentOutOfRangeException(
                    "maximumIntegerValue",
                    "The target range does not fit UInt32.");
            }

            DisplayName = displayName;
            SlaveReference = slaveReference;
            ObjectIndex = objectIndex;
            SubIndex = subIndex;
            ValueType = valueType;
            DataLength = dataLength;
            MinimumIntegerValue = minimumIntegerValue;
            MaximumIntegerValue = maximumIntegerValue;
        }

        public string DisplayName { get; private set; }
        public ushort SlaveReference { get; private set; }
        public ushort ObjectIndex { get; private set; }
        public byte SubIndex { get; private set; }
        public LMCSignalValueType ValueType { get; private set; }
        public ushort DataLength { get; private set; }
        public long MinimumIntegerValue { get; private set; }
        public long MaximumIntegerValue { get; private set; }

        /// <summary>
        /// Creates a canonical four-byte little-endian request for this
        /// approved target. Submission still rechecks the central allowlist.
        /// </summary>
        public LMCSdoRequest CreateRequest(
            long integerValue,
            uint timeoutCycles)
        {
            if (timeoutCycles < 1
                || timeoutCycles
                    > LMCDiagnosticsSdoPolicy.MaximumReadTimeoutCycles)
            {
                throw new ArgumentOutOfRangeException(
                    "timeoutCycles",
                    "The active D5 SDO policy requires TimeoutCycles from 1 through 60000.");
            }

            if (integerValue < MinimumIntegerValue
                || integerValue > MaximumIntegerValue)
            {
                throw new ArgumentOutOfRangeException(
                    "integerValue",
                    "The SDO Write value is outside the approved target range.");
            }

            uint raw;
            if (ValueType == LMCSignalValueType.Int32)
            {
                raw = unchecked((uint)(int)integerValue);
            }
            else
            {
                raw = checked((uint)integerValue);
            }

            return LMCSdoRequest.CreateWrite(
                SlaveReference,
                ObjectIndex,
                SubIndex,
                ValueType,
                new[]
                {
                    (byte)raw,
                    (byte)(raw >> 8),
                    (byte)(raw >> 16),
                    (byte)(raw >> 24)
                },
                timeoutCycles);
        }

        public override string ToString()
        {
            return DisplayName
                + " | Slave " + SlaveReference.ToString(
                    CultureInfo.InvariantCulture)
                + " | 0x" + ObjectIndex.ToString(
                    "X4",
                    CultureInfo.InvariantCulture)
                + ":" + SubIndex.ToString(CultureInfo.InvariantCulture)
                + " | " + ValueType
                + " | " + MinimumIntegerValue.ToString(
                    CultureInfo.InvariantCulture)
                + ".." + MaximumIntegerValue.ToString(
                    CultureInfo.InvariantCulture);
        }

        internal bool Matches(LMCSdoRequest request)
        {
            if (request == null
                || request.SlaveReference != SlaveReference
                || request.ObjectIndex != ObjectIndex
                || request.SubIndex != SubIndex
                || request.ValueType != ValueType
                || request.DataLength != DataLength
                || request.TimeoutCycles < 1
                || request.TimeoutCycles
                    > LMCDiagnosticsSdoPolicy.MaximumReadTimeoutCycles)
            {
                return false;
            }

            var data = request.WriteDataUnsafe;
            if (data == null || data.Length != 4)
            {
                return false;
            }

            long value;
            if (ValueType == LMCSignalValueType.Int32)
            {
                value = unchecked((int)(
                    (uint)data[0]
                    | ((uint)data[1] << 8)
                    | ((uint)data[2] << 16)
                    | ((uint)data[3] << 24)));
            }
            else
            {
                value = (uint)data[0]
                    | ((uint)data[1] << 8)
                    | ((uint)data[2] << 16)
                    | ((uint)data[3] << 24);
            }

            return value >= MinimumIntegerValue
                && value <= MaximumIntegerValue;
        }
    }

    internal static class LMCDiagnosticsWritePolicy
    {
        // Enable a target only after its PLC mapping, drive-program ownership,
        // and hardware behavior are verified. The global and per-axis gates
        // deliberately require two source changes before any target is exposed.
        private static readonly bool SdoWriteEnabled = false;
        private static readonly bool SdoWriteUi24Axis1Enabled = false;
        private static readonly bool SdoWriteUi24Axis2Enabled = false;
        private static readonly bool SdoWriteUi24Axis3Enabled = false;
        private static readonly bool SdoWriteUi24Axis4Enabled = false;

        private static readonly uint[] AllowedPIWriteSignalIds = new uint[0];
        private static readonly LMCSdoWriteTarget[] AllowedSdoWrites =
            CreateAllowedSdoWriteTargets(
                SdoWriteEnabled,
                SdoWriteUi24Axis1Enabled,
                SdoWriteUi24Axis2Enabled,
                SdoWriteUi24Axis3Enabled,
                SdoWriteUi24Axis4Enabled);
        private static readonly ReadOnlyCollection<LMCSdoWriteTarget>
            ApprovedSdoWriteTargets = Array.AsReadOnly(AllowedSdoWrites);

        internal static IReadOnlyList<LMCSdoWriteTarget> GetApprovedSdoWriteTargets()
        {
            return ApprovedSdoWriteTargets;
        }

        internal static void RequirePIWriteAllowed(LMCPIWriteRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            for (var index = 0;
                index < AllowedPIWriteSignalIds.Length;
                index++)
            {
                if (AllowedPIWriteSignalIds[index] == request.SignalId)
                {
                    return;
                }
            }

            throw new NotSupportedException(
                "PI Write is blocked because the signal is not in the SDK compile-time allowlist.");
        }

        internal static void RequireSdoWriteAllowed(LMCSdoRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (!request.IsWrite)
            {
                return;
            }

            for (var index = 0; index < AllowedSdoWrites.Length; index++)
            {
                if (AllowedSdoWrites[index].Matches(request))
                {
                    return;
                }
            }

            throw new NotSupportedException(
                "SDO Write is blocked because the target is not in the SDK compile-time allowlist.");
        }

        internal static void RequireSdoWriteVerificationCapabilities(
            LMCDiagnosticCapabilities capabilities)
        {
            if (capabilities == null)
            {
                throw new ArgumentNullException("capabilities");
            }

            if (!capabilities.Supports(LMCDiagnosticCapability.SDORead)
                || !capabilities.Supports(
                    LMCDiagnosticCapability.SDOReadGeneralInline))
            {
                throw new NotSupportedException(
                    "SDO Write requires SDO Read and general-inline Read support for exact-target verification and recovery evidence.");
            }
        }

        internal static LMCSdoWriteTarget[] CreateAllowedSdoWriteTargets(
            bool globalEnabled,
            bool axis1Enabled,
            bool axis2Enabled,
            bool axis3Enabled,
            bool axis4Enabled)
        {
            if (!globalEnabled)
            {
                return new LMCSdoWriteTarget[0];
            }

            var targets = new List<LMCSdoWriteTarget>(4);
            AddUi24TargetIfEnabled(
                targets,
                1,
                axis1Enabled);
            AddUi24TargetIfEnabled(
                targets,
                2,
                axis2Enabled);
            AddUi24TargetIfEnabled(
                targets,
                3,
                axis3Enabled);
            AddUi24TargetIfEnabled(
                targets,
                4,
                axis4Enabled);
            return targets.ToArray();
        }

        private static void AddUi24TargetIfEnabled(
            ICollection<LMCSdoWriteTarget> targets,
            ushort slaveReference,
            bool enabled)
        {
            if (!enabled)
            {
                return;
            }

            targets.Add(
                new LMCSdoWriteTarget(
                    "Reserved diagnostic UI[24]",
                    slaveReference,
                    0x2F00,
                    24,
                    LMCSignalValueType.Int32,
                    4,
                    -1073741823,
                    1073741823));
        }

    }

    [Flags]
    public enum LMCOperationFlags : ushort
    {
        None = 0,
        Write = 1 << 0
    }

    public enum LMCOperationState : ushort
    {
        Free = 0,
        Queued = 1,
        Running = 2,
        Completed = 3,
        Failed = 4,
        Cancelled = 5,
        Expired = 6
    }

    public enum LMCOperationKind : ushort
    {
        None = 0,
        PIWrite = 1,
        SDORead = 2,
        SDOWrite = 3
    }

    public enum LMCOperationOutcome : ushort
    {
        NoneOrPending = 0,
        Success = 1,
        Failed = 2,
        Cancelled = 3,
        TimedOut = 4
    }

    public sealed class LMCPIWriteRequest
    {
        public LMCPIWriteRequest(
            LMCSignalCatalog catalog,
            LMCSignalCatalogEntry signal,
            LMCSignalValueType valueType,
            uint rawValue32)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException("catalog");
            }

            if (signal == null)
            {
                throw new ArgumentNullException("signal");
            }

            if (catalog.MapRevision == 0)
            {
                throw new ArgumentException(
                    "PI Write requires a Catalog with a non-zero MapRevision.",
                    "catalog");
            }

            var found = false;
            for (var index = 0; index < catalog.Entries.Count; index++)
            {
                if (ReferenceEquals(catalog.Entries[index], signal))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                throw new ArgumentException(
                    "The PI Write signal does not belong to the supplied Catalog.",
                    "signal");
            }

            if (signal.SignalId == 0)
            {
                throw new ArgumentException(
                    "PI Write requires a non-zero SignalId.",
                    "signal");
            }

            if ((signal.AccessFlags & LMCSignalAccessFlags.WritableByPolicy) == 0)
            {
                throw new InvalidOperationException(
                    "The Catalog does not mark this signal WritableByPolicy.");
            }

            if (valueType == LMCSignalValueType.Invalid
                || valueType != signal.DataType)
            {
                throw new ArgumentException(
                    "The PI Write ValueType must exactly match the Catalog entry.",
                    "valueType");
            }

            if (IsPermanentlyUnsafeTarget(signal))
            {
                throw new InvalidOperationException(
                    "Direct PI Write is permanently blocked for DS402 control and target objects.");
            }

            ValidateRawValue(signal, rawValue32);

            Catalog = catalog;
            Signal = signal;
            ValueType = valueType;
            RawValue32 = rawValue32;
        }

        public LMCSignalCatalog Catalog { get; private set; }
        public LMCSignalCatalogEntry Signal { get; private set; }
        public uint MapRevision { get { return Catalog.MapRevision; } }
        public uint SignalId { get { return Signal.SignalId; } }
        public LMCSignalValueType ValueType { get; private set; }
        public uint RawValue32 { get; private set; }

        internal static bool IsPermanentlyUnsafeTarget(
            LMCSignalCatalogEntry signal)
        {
            if (signal == null)
            {
                return false;
            }

            switch (signal.PdoIndex)
            {
                case 0x6040:
                case 0x607A:
                case 0x60FF:
                case 0x6071:
                    return true;
                default:
                    return false;
            }
        }

        private static void ValidateRawValue(
            LMCSignalCatalogEntry signal,
            uint rawValue32)
        {
            switch (signal.DataType)
            {
                case LMCSignalValueType.Bool:
                    if (rawValue32 > 1)
                    {
                        throw new ArgumentOutOfRangeException(
                            "rawValue32",
                            "Bool PI values must be canonical 0 or 1.");
                    }

                    ValidateUnsignedRange(signal, rawValue32);

                    break;

                case LMCSignalValueType.Int16:
                    var int16Value = unchecked((short)(ushort)rawValue32);
                    var canonicalInt16 = unchecked((uint)(int)int16Value);
                    if (rawValue32 != canonicalInt16)
                    {
                        throw new ArgumentOutOfRangeException(
                            "rawValue32",
                            "Int16 PI values must be sign-extended to 32 bits.");
                    }

                    ValidateSignedRange(signal, int16Value);
                    break;

                case LMCSignalValueType.UInt16:
                case LMCSignalValueType.BitField16:
                    if ((rawValue32 & 0xFFFF0000u) != 0)
                    {
                        throw new ArgumentOutOfRangeException(
                            "rawValue32",
                            "Unsigned 16-bit PI values must be zero-extended to 32 bits.");
                    }

                    ValidateUnsignedRange(signal, rawValue32);
                    break;

                case LMCSignalValueType.Int32:
                    ValidateSignedRange(signal, unchecked((int)rawValue32));
                    break;

                case LMCSignalValueType.UInt32:
                case LMCSignalValueType.BitField32:
                    ValidateUnsignedRange(signal, rawValue32);
                    break;

                case LMCSignalValueType.Real32:
                    throw new NotSupportedException(
                        "REAL PI Write is fail-closed because schema v1 does not define how DINT MinimumRaw and MaximumRaw encode REAL bounds.");

                default:
                    throw new ArgumentOutOfRangeException(
                        "signal",
                        "The Catalog entry has no writable schema-v1 ValueType.");
            }
        }

        private static void ValidateSignedRange(
            LMCSignalCatalogEntry signal,
            int value)
        {
            if (value < signal.MinimumRaw || value > signal.MaximumRaw)
            {
                throw new ArgumentOutOfRangeException(
                    "rawValue32",
                    "The PI value is outside the Catalog raw range.");
            }
        }

        private static void ValidateUnsignedRange(
            LMCSignalCatalogEntry signal,
            uint value)
        {
            var minimum = unchecked((uint)signal.MinimumRaw);
            var maximum = unchecked((uint)signal.MaximumRaw);
            if (value < minimum || value > maximum)
            {
                throw new ArgumentOutOfRangeException(
                    "rawValue32",
                    "The PI value is outside the Catalog raw range.");
            }
        }
    }

    public sealed class LMCSdoRequest
    {
        private readonly byte[] writeData;

        private LMCSdoRequest(
            ushort slaveReference,
            LMCOperationFlags operationFlags,
            ushort objectIndex,
            byte subIndex,
            LMCSignalValueType valueType,
            uint timeoutCycles,
            ushort dataLength,
            byte[] writeData)
        {
            ValidateIdentity(
                slaveReference,
                operationFlags,
                objectIndex,
                valueType,
                timeoutCycles,
                dataLength);

            if (operationFlags == LMCOperationFlags.Write)
            {
                if (writeData == null || writeData.Length != dataLength)
                {
                    throw new ArgumentException(
                        "SDO WriteData length must exactly match DataLength.",
                        "writeData");
                }

                ValidateCanonicalWriteData(valueType, writeData);
                this.writeData = (byte[])writeData.Clone();
            }
            else
            {
                if (operationFlags != LMCOperationFlags.None)
                {
                    throw new ArgumentOutOfRangeException("operationFlags");
                }

                if (writeData != null && writeData.Length != 0)
                {
                    throw new ArgumentException(
                        "SDO Read must not contain WriteData.",
                        "writeData");
                }

                this.writeData = new byte[0];
            }

            SlaveReference = slaveReference;
            OperationFlags = operationFlags;
            ObjectIndex = objectIndex;
            SubIndex = subIndex;
            ValueType = valueType;
            TimeoutCycles = timeoutCycles;
            DataLength = dataLength;
        }

        public ushort SlaveReference { get; private set; }
        public LMCOperationFlags OperationFlags { get; private set; }
        public ushort ObjectIndex { get; private set; }
        public byte SubIndex { get; private set; }
        public LMCSignalValueType ValueType { get; private set; }
        public uint TimeoutCycles { get; private set; }
        public ushort DataLength { get; private set; }
        public bool IsWrite { get { return OperationFlags == LMCOperationFlags.Write; } }
        public byte[] WriteData { get { return (byte[])writeData.Clone(); } }

        internal byte[] WriteDataUnsafe { get { return writeData; } }

        public static LMCSdoRequest CreateRead(
            ushort slaveReference,
            ushort objectIndex,
            byte subIndex,
            LMCSignalValueType valueType,
            ushort dataLength,
            uint timeoutCycles)
        {
            return new LMCSdoRequest(
                slaveReference,
                LMCOperationFlags.None,
                objectIndex,
                subIndex,
                valueType,
                timeoutCycles,
                dataLength,
                null);
        }

        public static LMCSdoRequest CreateWrite(
            ushort slaveReference,
            ushort objectIndex,
            byte subIndex,
            LMCSignalValueType valueType,
            byte[] writeData,
            uint timeoutCycles)
        {
            if (writeData == null)
            {
                throw new ArgumentNullException("writeData");
            }

            if (writeData.Length != 4
                && writeData.Length != 8
                && writeData.Length != 12)
            {
                throw new ArgumentOutOfRangeException(
                    "writeData",
                    "D5 SDO WriteData must contain exactly 4, 8, or 12 bytes.");
            }

            return new LMCSdoRequest(
                slaveReference,
                LMCOperationFlags.Write,
                objectIndex,
                subIndex,
                valueType,
                timeoutCycles,
                (ushort)writeData.Length,
                writeData);
        }

        internal static bool IsPermanentlyUnsafeObject(ushort objectIndex)
        {
            switch (objectIndex)
            {
                case 0x6040:
                case 0x607A:
                case 0x60FF:
                case 0x6071:
                    return true;
                default:
                    return false;
            }
        }

        private static void ValidateIdentity(
            ushort slaveReference,
            LMCOperationFlags operationFlags,
            ushort objectIndex,
            LMCSignalValueType valueType,
            uint timeoutCycles,
            ushort dataLength)
        {
            if (slaveReference == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "slaveReference",
                    "SlaveReference must be non-zero.");
            }

            if (objectIndex == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "objectIndex",
                    "ObjectIndex must be non-zero.");
            }

            if (valueType <= LMCSignalValueType.Invalid
                || valueType > LMCSignalValueType.BitField8)
            {
                throw new ArgumentOutOfRangeException("valueType");
            }

            if (timeoutCycles == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "timeoutCycles",
                    "TimeoutCycles must be non-zero.");
            }

            var isReadInlineLength = dataLength == 1
                || dataLength == 2
                || dataLength == 4
                || dataLength == 8
                || dataLength == 12;
            var isWriteInlineLength = dataLength == 4
                || dataLength == 8
                || dataLength == 12;
            if (operationFlags == LMCOperationFlags.Write
                && !isWriteInlineLength)
            {
                throw new ArgumentOutOfRangeException(
                    "dataLength",
                    "SDO Write DataLength must be exactly 4, 8, or 12 bytes.");
            }

            if (operationFlags == LMCOperationFlags.None
                && !isReadInlineLength
                && dataLength <= LMC_DiagnosticsFrame.MaxD5InlineSdoDataBytes)
            {
                throw new ArgumentOutOfRangeException(
                    "dataLength",
                    "Inline SDO Read DataLength must be 1, 2, 4, 8, or 12 bytes; larger reads require result chunks.");
            }

            if (operationFlags == LMCOperationFlags.None
                && dataLength < 4
                && dataLength
                    != LMCDiagnosticsSdoPolicy.ExpectedReadLength(valueType))
            {
                throw new ArgumentOutOfRangeException(
                    "dataLength",
                    "One- and two-byte SDO reads must exactly match the selected SDO ValueType width.");
            }
        }

        private static void ValidateCanonicalWriteData(
            LMCSignalValueType valueType,
            byte[] data)
        {
            if (valueType == LMCSignalValueType.Bool)
            {
                if (data[0] > 1)
                {
                    throw new ArgumentException(
                        "Bool SDO data must begin with canonical 0 or 1.",
                        "writeData");
                }

                RequireTail(data, 1, 0);
            }
            else if (valueType == LMCSignalValueType.Int8)
            {
                var fill = (data[0] & 0x80) == 0 ? (byte)0 : (byte)0xFF;
                RequireTail(data, 1, fill);
            }
            else if (valueType == LMCSignalValueType.UInt8
                || valueType == LMCSignalValueType.BitField8)
            {
                RequireTail(data, 1, 0);
            }
            else if (valueType == LMCSignalValueType.Int16)
            {
                var fill = (data[1] & 0x80) == 0 ? (byte)0 : (byte)0xFF;
                RequireTail(data, 2, fill);
            }
            else if (valueType == LMCSignalValueType.UInt16
                || valueType == LMCSignalValueType.BitField16)
            {
                RequireTail(data, 2, 0);
            }
        }

        private static void RequireTail(
            byte[] data,
            int startIndex,
            byte expected)
        {
            for (var index = startIndex; index < data.Length; index++)
            {
                if (data[index] != expected)
                {
                    throw new ArgumentException(
                        "Narrow SDO values must use canonical sign or zero extension.",
                        "writeData");
                }
            }
        }
    }

    public sealed class LMCOperationTicket
    {
        internal LMCOperationTicket(
            uint ticketId,
            LMCOperationKind operationKind,
            uint queuedCycle,
            uint diagnosticsBootId,
            uint submissionMapRevision,
            long connectionSessionGeneration,
            LMCDiagnostics owner,
            bool expectsResultData,
            ushort expectedResultLength,
            LMCSignalValueType expectedResultValueType,
            bool usesExtendedResultChunks = false,
            ushort maxResultChunkDataBytes = 0)
        {
            if (ticketId == 0)
            {
                throw new ArgumentOutOfRangeException("ticketId");
            }

            if (operationKind <= LMCOperationKind.None
                || operationKind > LMCOperationKind.SDOWrite)
            {
                throw new ArgumentOutOfRangeException("operationKind");
            }

            if (diagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException("diagnosticsBootId");
            }

            if (submissionMapRevision == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "submissionMapRevision");
            }

            var isSdoRead = operationKind == LMCOperationKind.SDORead;
            if (expectsResultData != isSdoRead
                || (isSdoRead
                    && (expectedResultLength == 0
                        || expectedResultValueType
                            == LMCSignalValueType.Invalid))
                || (!isSdoRead
                    && (expectedResultLength != 0
                        || expectedResultValueType
                            != LMCSignalValueType.Invalid)))
            {
                throw new ArgumentException(
                    "Operation ticket result metadata does not match its operation kind.");
            }

            if (usesExtendedResultChunks
                && (!isSdoRead
                    || expectedResultLength
                        <= LMC_DiagnosticsFrame.MaxD5InlineSdoDataBytes
                    || maxResultChunkDataBytes == 0))
            {
                throw new ArgumentException(
                    "Extended SDO result tickets require a result larger than 12 bytes and a non-zero chunk limit.");
            }

            if (!usesExtendedResultChunks && maxResultChunkDataBytes != 0)
            {
                throw new ArgumentException(
                    "Inline operation tickets must not contain an SDO result chunk limit.");
            }

            Owner = owner ?? throw new ArgumentNullException("owner");
            TicketId = ticketId;
            OperationKind = operationKind;
            QueuedCycle = queuedCycle;
            DiagnosticsBootId = diagnosticsBootId;
            SubmissionMapRevision = submissionMapRevision;
            ConnectionSessionGeneration = connectionSessionGeneration;
            ExpectsResultData = expectsResultData;
            ExpectedResultLength = expectedResultLength;
            ExpectedResultValueType = expectedResultValueType;
            UsesExtendedResultChunks = usesExtendedResultChunks;
            MaxResultChunkDataBytes = maxResultChunkDataBytes;
        }

        public uint TicketId { get; private set; }
        public LMCOperationKind OperationKind { get; private set; }
        public uint QueuedCycle { get; private set; }
        public uint DiagnosticsBootId { get; private set; }
        public uint SubmissionMapRevision { get; private set; }
        public bool UsesExtendedResultChunks { get; private set; }
        public ushort RequestedResultLength { get { return ExpectedResultLength; } }
        public LMCSignalValueType ResultValueType
        {
            get { return ExpectedResultValueType; }
        }

        public bool BelongsTo(LMCConnection connection)
        {
            return connection != null
                && ReferenceEquals(Owner, connection.Diagnostics);
        }

        public bool BelongsToCurrentSession(LMCConnection connection)
        {
            return BelongsTo(connection)
                && ConnectionSessionGeneration
                    == connection.SessionGeneration;
        }

        internal long ConnectionSessionGeneration { get; private set; }
        internal LMCDiagnostics Owner { get; private set; }
        internal bool ExpectsResultData { get; private set; }
        internal ushort ExpectedResultLength { get; private set; }
        internal LMCSignalValueType ExpectedResultValueType { get; private set; }
        internal ushort MaxResultChunkDataBytes { get; private set; }
    }

    public sealed class LMCOperationStatus
    {
        private readonly byte[] resultData;

        internal LMCOperationStatus(
            LMCDiagnosticsResponse response,
            uint ticketId,
            LMCOperationKind operationKind,
            LMCOperationState state,
            uint submitCycle,
            uint completionCycle,
            LMCOperationOutcome outcome,
            short operationErrorId,
            uint operationDetail,
            uint resultLength,
            LMCSignalValueType resultValueType,
            byte[] resultData,
            uint diagnosticsBootId)
        {
            Response = response;
            TicketId = ticketId;
            OperationKind = operationKind;
            State = state;
            SubmitCycle = submitCycle;
            CompletionCycle = completionCycle;
            Outcome = outcome;
            OperationErrorId = operationErrorId;
            OperationDetail = operationDetail;
            ResultLength = resultLength;
            ResultValueType = resultValueType;
            this.resultData = resultData == null
                ? new byte[0]
                : (byte[])resultData.Clone();
            DiagnosticsBootId = diagnosticsBootId;
        }

        public LMCDiagnosticsResponse Response { get; private set; }
        public uint TicketId { get; private set; }
        public LMCOperationKind OperationKind { get; private set; }
        public LMCOperationState State { get; private set; }
        public uint SubmitCycle { get; private set; }
        public uint CompletionCycle { get; private set; }
        public LMCOperationOutcome Outcome { get; private set; }
        public short OperationErrorId { get; private set; }
        public uint OperationDetail { get; private set; }
        public uint ResultLength { get; private set; }
        public LMCSignalValueType ResultValueType { get; private set; }
        public byte[] ResultData { get { return (byte[])resultData.Clone(); } }
        public uint DiagnosticsBootId { get; private set; }

        public bool IsTerminal
        {
            get
            {
                return State == LMCOperationState.Completed
                    || State == LMCOperationState.Failed
                    || State == LMCOperationState.Cancelled
                    || State == LMCOperationState.Expired;
            }
        }

        public bool IsSuccessful
        {
            get
            {
                return State == LMCOperationState.Completed
                    && Outcome == LMCOperationOutcome.Success;
            }
        }
    }

    public sealed class LMCSdoResultChunkRequest
    {
        public LMCSdoResultChunkRequest(
            LMCOperationTicket ticket,
            uint offsetBytes,
            ushort requestedByteCount,
            uint sequence)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException("ticket");
            }

            if (ticket.OperationKind != LMCOperationKind.SDORead
                || !ticket.UsesExtendedResultChunks)
            {
                throw new ArgumentException(
                    "SDO result chunks require an extended SDO Read ticket.",
                    "ticket");
            }

            if (requestedByteCount == 0
                || requestedByteCount > ticket.MaxResultChunkDataBytes)
            {
                throw new ArgumentOutOfRangeException(
                    "requestedByteCount",
                    "RequestedByteCount must fit the negotiated SDO result chunk limit.");
            }

            if (offsetBytes >= ticket.ExpectedResultLength)
            {
                throw new ArgumentOutOfRangeException(
                    "offsetBytes",
                    "OffsetBytes must be inside the requested SDO result length.");
            }

            Ticket = ticket;
            OffsetBytes = offsetBytes;
            RequestedByteCount = requestedByteCount;
            Sequence = sequence;
        }

        public LMCOperationTicket Ticket { get; private set; }
        public uint OffsetBytes { get; private set; }
        public ushort RequestedByteCount { get; private set; }
        public uint Sequence { get; private set; }
    }

    public sealed class LMCSdoResultChunk
    {
        private readonly byte[] data;

        internal LMCSdoResultChunk(
            LMCDiagnosticsResponse response,
            uint ticketId,
            uint offsetBytes,
            ushort returnedByteCount,
            uint sequence,
            uint totalResultLength,
            uint dataCrc32,
            uint diagnosticsBootId,
            LMCSignalValueType valueType,
            byte[] data)
        {
            Response = response;
            TicketId = ticketId;
            OffsetBytes = offsetBytes;
            ReturnedByteCount = returnedByteCount;
            Sequence = sequence;
            TotalResultLength = totalResultLength;
            DataCrc32 = dataCrc32;
            DiagnosticsBootId = diagnosticsBootId;
            ValueType = valueType;
            this.data = data == null ? new byte[0] : (byte[])data.Clone();
        }

        public LMCDiagnosticsResponse Response { get; private set; }
        public uint TicketId { get; private set; }
        public uint OffsetBytes { get; private set; }
        public ushort ReturnedByteCount { get; private set; }
        public uint Sequence { get; private set; }
        public uint TotalResultLength { get; private set; }
        public uint DataCrc32 { get; private set; }
        public uint DiagnosticsBootId { get; private set; }
        public LMCSignalValueType ValueType { get; private set; }
        public byte[] Data { get { return (byte[])data.Clone(); } }
        public bool IsLastChunk
        {
            get
            {
                return (Response.ResponseFlags
                    & LMCDiagnosticsResponseFlags.LastChunk) != 0;
            }
        }
    }

    internal sealed class LMCOperationSubmission
    {
        internal LMCOperationSubmission(
            LMCDiagnosticsResponse response,
            uint ticketId,
            LMCOperationKind operationKind,
            uint queuedCycle,
            uint diagnosticsBootId)
        {
            Response = response;
            TicketId = ticketId;
            OperationKind = operationKind;
            QueuedCycle = queuedCycle;
            DiagnosticsBootId = diagnosticsBootId;
        }

        internal LMCDiagnosticsResponse Response { get; private set; }
        internal uint TicketId { get; private set; }
        internal LMCOperationKind OperationKind { get; private set; }
        internal uint QueuedCycle { get; private set; }
        internal uint DiagnosticsBootId { get; private set; }
    }

    internal sealed class LMCCancelOperationResult
    {
        internal LMCCancelOperationResult(
            LMCDiagnosticsResponse response,
            uint ticketId,
            LMCOperationState state,
            LMCOperationOutcome outcome,
            uint diagnosticsBootId)
        {
            Response = response;
            TicketId = ticketId;
            State = state;
            Outcome = outcome;
            DiagnosticsBootId = diagnosticsBootId;
        }

        internal LMCDiagnosticsResponse Response { get; private set; }
        internal uint TicketId { get; private set; }
        internal LMCOperationState State { get; private set; }
        internal LMCOperationOutcome Outcome { get; private set; }
        internal uint DiagnosticsBootId { get; private set; }
    }
}
