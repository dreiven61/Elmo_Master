using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace LasalMotionControlLib
{
    public enum LMCDriveReadOperationKind
    {
        DriveOperationMode = 0,
        DriveStatus = 1
    }

    public enum LMCDriveReadAttemptPhase
    {
        FacadePreflight = 0,
        AxisStatusRead = 1,
        CapabilityPreflight = 2,
        Submission = 3,
        StatusPolling = 4,
        ResultMaterialization = 5
    }

    public enum LMCSdoReadSubmissionOutcome
    {
        NotAttempted = 0,
        Rejected = 1,
        OutcomeUncertain = 2,
        Accepted = 3
    }

    /// <summary>
    /// Immutable state of one SDO Read inside a drive-read facade call.
    /// SubmissionOutcome describes only whether the 0x7E50 command could have
    /// created a PLC ticket. Terminal state is reported independently through
    /// LastOperationStatus.
    /// </summary>
    public sealed class LMCSdoReadAttemptSnapshot
    {
        internal LMCSdoReadAttemptSnapshot(
            int attemptNumber,
            LMCSdoRequest request,
            LMCSdoSubmissionOutcome submissionOutcome,
            LMCOperationTicket ticket,
            LMCOperationStatus lastOperationStatus,
            uint diagnosticsBootId = 0,
            uint mapRevision = 0)
        {
            if (attemptNumber < 1)
            {
                throw new ArgumentOutOfRangeException("attemptNumber");
            }

            Request = request ?? throw new ArgumentNullException("request");
            if (request.IsWrite)
            {
                throw new ArgumentException(
                    "Drive-read attempt snapshots require an SDO Read request.",
                    "request");
            }

            if (!Enum.IsDefined(
                typeof(LMCSdoSubmissionOutcome),
                submissionOutcome))
            {
                throw new ArgumentOutOfRangeException("submissionOutcome");
            }

            if (submissionOutcome == LMCSdoSubmissionOutcome.Accepted)
            {
                if (ticket == null)
                {
                    throw new ArgumentNullException(
                        "ticket",
                        "An accepted SDO Read submission requires its ticket.");
                }
            }
            else if (ticket != null || lastOperationStatus != null)
            {
                throw new ArgumentException(
                    "Only an accepted SDO Read submission can have a ticket or status.");
            }

            if (submissionOutcome
                    != LMCSdoSubmissionOutcome.NotAttempted
                && (diagnosticsBootId == 0 || mapRevision == 0))
            {
                throw new ArgumentException(
                    "A dispatched SDO Read requires its capability BootId and MapRevision.");
            }

            if (ticket != null
                && ticket.DiagnosticsBootId != diagnosticsBootId)
            {
                throw new ArgumentException(
                    "The accepted ticket does not match the SDO Read capability BootId.",
                    "ticket");
            }

            if (lastOperationStatus != null
                && (lastOperationStatus.TicketId != ticket.TicketId
                    || lastOperationStatus.OperationKind
                        != ticket.OperationKind
                    || lastOperationStatus.DiagnosticsBootId
                        != ticket.DiagnosticsBootId))
            {
                throw new ArgumentException(
                    "The SDO Read status does not belong to the accepted ticket.",
                    "lastOperationStatus");
            }

            AttemptNumber = attemptNumber;
            GenericSubmissionOutcome = submissionOutcome;
            SubmissionOutcome = (LMCSdoReadSubmissionOutcome)submissionOutcome;
            Ticket = ticket;
            LastOperationStatus = lastOperationStatus;
            DiagnosticsBootId = diagnosticsBootId;
            MapRevision = mapRevision;
        }

        public int AttemptNumber { get; private set; }
        public LMCSdoRequest Request { get; private set; }
        /// <summary>
        /// Legacy drive-read projection retained for source and binary
        /// compatibility. GenericSubmissionOutcome exposes the same state
        /// through the shared SDO submission enum.
        /// </summary>
        public LMCSdoReadSubmissionOutcome SubmissionOutcome
        {
            get;
            private set;
        }

        public LMCSdoSubmissionOutcome GenericSubmissionOutcome
        {
            get;
            private set;
        }

        public LMCOperationTicket Ticket { get; private set; }
        public LMCOperationStatus LastOperationStatus { get; private set; }
        public uint DiagnosticsBootId { get; private set; }
        public uint MapRevision { get; private set; }

        public bool IsTerminal
        {
            get
            {
                return LastOperationStatus != null
                    && LastOperationStatus.IsTerminal;
            }
        }
    }

    /// <summary>
    /// Typed failure context for GetDriveOperationMode and ReadDriveStatus.
    /// The original exception type is preserved. Call TryGet with the caught
    /// exception to inspect whether an SDO was never attempted, explicitly
    /// rejected, outcome-uncertain, or accepted with a known ticket.
    /// </summary>
    public sealed class LMCDriveReadFailureContext
    {
        private static readonly object FailureContextSync = new object();
        private static readonly ConditionalWeakTable<
            Exception,
            LMCDriveReadFailureContext> FailureContexts =
                new ConditionalWeakTable<
                    Exception,
                    LMCDriveReadFailureContext>();

        private readonly ReadOnlyCollection<LMCSdoReadAttemptSnapshot>
            sdoAttempts;

        internal LMCDriveReadFailureContext(
            LMCDriveReadOperationKind operationKind,
            ushort axisReference,
            LMCDriveReadAttemptPhase phase,
            bool axisStatusReadCompleted,
            IList<LMCSdoReadAttemptSnapshot> attempts)
        {
            if (!Enum.IsDefined(typeof(LMCDriveReadOperationKind), operationKind))
            {
                throw new ArgumentOutOfRangeException("operationKind");
            }

            if (axisReference == 0)
            {
                throw new ArgumentOutOfRangeException("axisReference");
            }

            if (!Enum.IsDefined(typeof(LMCDriveReadAttemptPhase), phase))
            {
                throw new ArgumentOutOfRangeException("phase");
            }

            if (attempts == null)
            {
                throw new ArgumentNullException("attempts");
            }

            var copiedAttempts = new List<LMCSdoReadAttemptSnapshot>(attempts);
            for (var index = 0; index < copiedAttempts.Count; index++)
            {
                if (copiedAttempts[index] == null
                    || copiedAttempts[index].AttemptNumber != index + 1)
                {
                    throw new ArgumentException(
                        "SDO Read attempts must be non-null and sequentially numbered.",
                        "attempts");
                }

                if (index + 1 < copiedAttempts.Count
                    && !copiedAttempts[index].IsTerminal)
                {
                    throw new ArgumentException(
                        "A later SDO Read cannot start before the previous ticket is terminal.",
                        "attempts");
                }
            }

            OperationKind = operationKind;
            AxisReference = axisReference;
            Phase = phase;
            AxisStatusReadCompleted = axisStatusReadCompleted;
            sdoAttempts = copiedAttempts.AsReadOnly();
        }

        public LMCDriveReadOperationKind OperationKind { get; private set; }
        public ushort AxisReference { get; private set; }
        public LMCDriveReadAttemptPhase Phase { get; private set; }
        public bool AxisStatusReadCompleted { get; private set; }

        public IReadOnlyList<LMCSdoReadAttemptSnapshot> SdoAttempts
        {
            get { return sdoAttempts; }
        }

        public LMCSdoReadAttemptSnapshot CurrentSdoAttempt
        {
            get
            {
                return sdoAttempts.Count == 0
                    ? null
                    : sdoAttempts[sdoAttempts.Count - 1];
            }
        }

        public static bool TryGet(
            Exception exception,
            out LMCDriveReadFailureContext context)
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
            LMCDriveReadFailureContext context)
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

    internal sealed class LMCDriveReadAttemptTracker
        : ILMCSdoSubmissionAttemptTracker
    {
        private sealed class MutableSdoReadAttempt
        {
            internal MutableSdoReadAttempt(int attemptNumber, LMCSdoRequest request)
            {
                AttemptNumber = attemptNumber;
                Request = request;
                SubmissionOutcome =
                    LMCSdoSubmissionOutcome.NotAttempted;
            }

            internal int AttemptNumber { get; private set; }
            internal LMCSdoRequest Request { get; private set; }
            internal LMCSdoSubmissionOutcome SubmissionOutcome
            {
                get;
                set;
            }

            internal LMCOperationTicket Ticket { get; set; }
            internal LMCOperationStatus LastOperationStatus { get; set; }
            internal uint DiagnosticsBootId { get; set; }
            internal uint MapRevision { get; set; }
        }

        private readonly object sync = new object();
        private readonly LMCDriveReadOperationKind operationKind;
        private readonly ushort axisReference;
        private readonly List<MutableSdoReadAttempt> attempts =
            new List<MutableSdoReadAttempt>();
        private LMCDriveReadAttemptPhase phase =
            LMCDriveReadAttemptPhase.FacadePreflight;
        private bool axisStatusReadCompleted;

        internal LMCDriveReadAttemptTracker(
            LMCDriveReadOperationKind operationKind,
            ushort axisReference)
        {
            if (!Enum.IsDefined(typeof(LMCDriveReadOperationKind), operationKind))
            {
                throw new ArgumentOutOfRangeException("operationKind");
            }

            if (axisReference == 0)
            {
                throw new ArgumentOutOfRangeException("axisReference");
            }

            this.operationKind = operationKind;
            this.axisReference = axisReference;
        }

        internal void BeginAxisStatusRead()
        {
            lock (sync)
            {
                phase = LMCDriveReadAttemptPhase.AxisStatusRead;
            }
        }

        internal void MarkAxisStatusReadCompleted()
        {
            lock (sync)
            {
                if (phase != LMCDriveReadAttemptPhase.AxisStatusRead)
                {
                    throw new InvalidOperationException(
                        "Axis status completion requires the AxisStatusRead phase.");
                }

                axisStatusReadCompleted = true;
            }
        }

        internal void BeginSdoRead(LMCSdoRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (request.IsWrite)
            {
                throw new ArgumentException(
                    "Drive-read tracking accepts SDO Read requests only.",
                    "request");
            }

            lock (sync)
            {
                if (attempts.Count != 0)
                {
                    var previous = attempts[attempts.Count - 1];
                    if (previous.SubmissionOutcome
                            != LMCSdoSubmissionOutcome.Accepted
                        || previous.LastOperationStatus == null
                        || !previous.LastOperationStatus.IsTerminal)
                    {
                        throw new InvalidOperationException(
                            "A new SDO Read cannot start before the previous ticket is terminal.");
                    }
                }

                attempts.Add(new MutableSdoReadAttempt(
                    attempts.Count + 1,
                    request));
                phase = LMCDriveReadAttemptPhase.CapabilityPreflight;
            }
        }

        public void BeginSubmission()
        {
            lock (sync)
            {
                RequireCurrentOutcome(
                    LMCSdoSubmissionOutcome.NotAttempted);
                if (phase != LMCDriveReadAttemptPhase.CapabilityPreflight
                    || CurrentAttempt.DiagnosticsBootId == 0
                    || CurrentAttempt.MapRevision == 0)
                {
                    throw new InvalidOperationException(
                        "SDO Read submission requires a validated capability identity.");
                }

                phase = LMCDriveReadAttemptPhase.Submission;
            }
        }

        public void RecordCapabilityIdentity(
            uint diagnosticsBootId,
            uint mapRevision)
        {
            lock (sync)
            {
                RequireCurrentOutcome(
                    LMCSdoSubmissionOutcome.NotAttempted);
                if (phase != LMCDriveReadAttemptPhase.CapabilityPreflight)
                {
                    throw new InvalidOperationException(
                        "Capability identity must be recorded during capability preflight.");
                }

                CurrentAttempt.DiagnosticsBootId = diagnosticsBootId;
                CurrentAttempt.MapRevision = mapRevision;
            }
        }

        public void MarkSubmissionOutcomeUncertain()
        {
            lock (sync)
            {
                RequireCurrentOutcome(
                    LMCSdoSubmissionOutcome.NotAttempted);
                phase = LMCDriveReadAttemptPhase.Submission;
                CurrentAttempt.SubmissionOutcome =
                    LMCSdoSubmissionOutcome.OutcomeUncertain;
            }
        }

        public void MarkSubmissionRejected()
        {
            lock (sync)
            {
                RequireCurrentOutcome(
                    LMCSdoSubmissionOutcome.OutcomeUncertain);
                CurrentAttempt.SubmissionOutcome =
                    LMCSdoSubmissionOutcome.Rejected;
            }
        }

        public void MarkSubmissionAccepted(LMCOperationTicket ticket)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException("ticket");
            }

            lock (sync)
            {
                RequireCurrentOutcome(
                    LMCSdoSubmissionOutcome.OutcomeUncertain);
                CurrentAttempt.SubmissionOutcome =
                    LMCSdoSubmissionOutcome.Accepted;
                CurrentAttempt.Ticket = ticket;
            }
        }

        internal void BeginStatusPolling()
        {
            lock (sync)
            {
                RequireCurrentOutcome(
                    LMCSdoSubmissionOutcome.Accepted);
                phase = LMCDriveReadAttemptPhase.StatusPolling;
            }
        }

        internal void RecordOperationStatus(LMCOperationStatus status)
        {
            if (status == null)
            {
                throw new ArgumentNullException("status");
            }

            lock (sync)
            {
                RequireCurrentOutcome(
                    LMCSdoSubmissionOutcome.Accepted);
                var ticket = CurrentAttempt.Ticket;
                if (status.TicketId != ticket.TicketId
                    || status.OperationKind != ticket.OperationKind
                    || status.DiagnosticsBootId != ticket.DiagnosticsBootId)
                {
                    throw new ArgumentException(
                        "The operation status does not belong to the tracked SDO Read ticket.",
                        "status");
                }

                CurrentAttempt.LastOperationStatus = status;
            }
        }

        internal void BeginResultMaterialization()
        {
            lock (sync)
            {
                for (var index = 0; index < attempts.Count; index++)
                {
                    if (attempts[index].SubmissionOutcome
                            != LMCSdoSubmissionOutcome.Accepted
                        || attempts[index].LastOperationStatus == null
                        || !attempts[index].LastOperationStatus.IsTerminal)
                    {
                        throw new InvalidOperationException(
                            "Result materialization requires every SDO Read ticket to be terminal.");
                    }
                }

                phase = LMCDriveReadAttemptPhase.ResultMaterialization;
            }
        }

        internal LMCDriveReadFailureContext CreateFailureContext()
        {
            lock (sync)
            {
                var snapshots = new List<LMCSdoReadAttemptSnapshot>(
                    attempts.Count);
                foreach (var attempt in attempts)
                {
                    snapshots.Add(new LMCSdoReadAttemptSnapshot(
                        attempt.AttemptNumber,
                        attempt.Request,
                        attempt.SubmissionOutcome,
                        attempt.Ticket,
                        attempt.LastOperationStatus,
                        attempt.DiagnosticsBootId,
                        attempt.MapRevision));
                }

                return new LMCDriveReadFailureContext(
                    operationKind,
                    axisReference,
                    phase,
                    axisStatusReadCompleted,
                    snapshots);
            }
        }

        private MutableSdoReadAttempt CurrentAttempt
        {
            get
            {
                if (attempts.Count == 0)
                {
                    throw new InvalidOperationException(
                        "No SDO Read attempt is active.");
                }

                return attempts[attempts.Count - 1];
            }
        }

        private void RequireCurrentOutcome(
            LMCSdoSubmissionOutcome expectedOutcome)
        {
            if (CurrentAttempt.SubmissionOutcome != expectedOutcome)
            {
                throw new InvalidOperationException(
                    "The SDO Read attempt state transition is invalid.");
            }
        }
    }

    /// <summary>
    /// CiA 402 modes of operation read from object 0x6061:0.
    /// Unknown and manufacturer-specific signed values remain available through
    /// LMCDriveOperationModeResult.RawValue.
    /// </summary>
    public enum LMCDriveOperationMode : sbyte
    {
        NoModeAssigned = 0,
        ProfilePosition = 1,
        Velocity = 2,
        ProfileVelocity = 3,
        ProfileTorque = 4,
        Homing = 6,
        InterpolatedPosition = 7,
        CyclicSynchronousPosition = 8,
        CyclicSynchronousVelocity = 9,
        CyclicSynchronousTorque = 10
    }

    public sealed class LMCDriveOperationModeResult
    {
        internal LMCDriveOperationModeResult(
            ushort axisReference,
            LMCInlineSdoReadCompletion completion)
        {
            if (axisReference == 0)
            {
                throw new ArgumentOutOfRangeException("axisReference");
            }

            if (completion == null)
            {
                throw new ArgumentNullException("completion");
            }

            var data = completion.Status.ResultData;
            if (!completion.Status.IsSuccessful
                || completion.Status.ResultValueType != LMCSignalValueType.Int8
                || completion.Status.ResultLength != 1
                || data.Length != 1)
            {
                throw new InvalidDataException(
                    "Drive operation mode requires a successful Int8 one-byte SDO result.");
            }

            AxisReference = axisReference;
            Ticket = completion.Ticket;
            OperationStatus = completion.Status;
            RawValue = unchecked((sbyte)data[0]);
            Mode = (LMCDriveOperationMode)RawValue;
        }

        public ushort AxisReference { get; private set; }
        public sbyte RawValue { get; private set; }
        public LMCDriveOperationMode Mode { get; private set; }
        public LMCOperationTicket Ticket { get; private set; }
        public LMCOperationStatus OperationStatus { get; private set; }

        public bool IsSuccessful
        {
            get { return OperationStatus.IsSuccessful; }
        }

        public bool IsKnownMode
        {
            get
            {
                return Enum.IsDefined(
                    typeof(LMCDriveOperationMode),
                    Mode);
            }
        }

        public bool IsDefined
        {
            get { return IsKnownMode; }
        }
    }

    /// <summary>
    /// Sequential composite of LASAL axis status, DS402 0x6041:0, and
    /// DS402 0x6061:0. It is deliberately not an atomic same-cycle snapshot.
    /// </summary>
    public sealed class LMCDriveStatus
    {
        private const uint LasalPositionLimitActiveMask = 0x00000020u;
        private const ushort LasalSoftwareMinimumErrorMask = 0x0002;
        private const ushort LasalSoftwareMaximumErrorMask = 0x0004;
        private const ushort LasalHardwareMinimumErrorMask = 0x0008;
        private const ushort LasalHardwareMaximumErrorMask = 0x0010;
        private const ushort Ds402InternalLimitActiveMask = 0x0800;

        internal LMCDriveStatus(
            ushort axisReference,
            LMCReadStatusResult axisStatus,
            LMCInlineSdoReadCompletion statusWordCompletion,
            LMCDriveOperationModeResult operationMode)
        {
            if (axisReference == 0)
            {
                throw new ArgumentOutOfRangeException("axisReference");
            }

            if (axisStatus == null)
            {
                throw new ArgumentNullException("axisStatus");
            }

            if (!axisStatus.IsReadSuccessful)
            {
                throw new ArgumentException(
                    "The axis ReadStatus operation was not successful.",
                    "axisStatus");
            }

            if (statusWordCompletion == null)
            {
                throw new ArgumentNullException("statusWordCompletion");
            }

            var statusWordData = statusWordCompletion.Status.ResultData;
            if (!statusWordCompletion.Status.IsSuccessful
                || statusWordCompletion.Status.ResultValueType
                    != LMCSignalValueType.BitField16
                || statusWordCompletion.Status.ResultLength != 2
                || statusWordData.Length != 2)
            {
                throw new InvalidDataException(
                    "Drive statusword requires a successful BitField16 two-byte SDO result.");
            }

            if (operationMode == null)
            {
                throw new ArgumentNullException("operationMode");
            }

            if (operationMode.AxisReference != axisReference)
            {
                throw new InvalidDataException(
                    "Drive operation mode belongs to a different axis reference.");
            }

            AxisReference = axisReference;
            AxisStatus = axisStatus;
            StatusWordTicket = statusWordCompletion.Ticket;
            StatusWordOperationStatus = statusWordCompletion.Status;
            Ds402StatusWord = (ushort)(
                statusWordData[0]
                | (statusWordData[1] << 8));
            OperationModeResult = operationMode;
        }

        public ushort AxisReference { get; private set; }
        public LMCReadStatusResult AxisStatus { get; private set; }
        public ushort Ds402StatusWord { get; private set; }
        public LMCOperationTicket StatusWordTicket { get; private set; }
        public LMCOperationStatus StatusWordOperationStatus { get; private set; }
        public LMCDriveOperationModeResult OperationModeResult
        {
            get;
            private set;
        }

        public LMCDriveOperationMode OperationMode
        {
            get { return OperationModeResult.Mode; }
        }

        public sbyte OperationModeRaw
        {
            get { return OperationModeResult.RawValue; }
        }

        /// <summary>
        /// Always false. The component reads are intentionally sequential and
        /// can represent different PLC and EtherCAT cycles.
        /// </summary>
        public bool IsAtomicSnapshot
        {
            get { return false; }
        }

        public bool IsReadSuccessful
        {
            get
            {
                return AxisStatus.IsReadSuccessful
                    && StatusWordOperationStatus.IsSuccessful
                    && OperationModeResult.OperationStatus.IsSuccessful;
            }
        }

        public ushort AxisErrorFlags
        {
            get { return AxisStatus.AxisErrorFlags; }
        }

        public bool HasAxisError
        {
            get { return AxisStatus.HasAxisError; }
        }

        public bool IsLasalPositionLimitActive
        {
            get
            {
                return (AxisStatus.State & LasalPositionLimitActiveMask) != 0;
            }
        }

        public bool HasSoftwareMinimumLimitError
        {
            get
            {
                return (AxisErrorFlags & LasalSoftwareMinimumErrorMask) != 0;
            }
        }

        public bool HasSoftwareMaximumLimitError
        {
            get
            {
                return (AxisErrorFlags & LasalSoftwareMaximumErrorMask) != 0;
            }
        }

        public bool HasHardwareMinimumLimitError
        {
            get
            {
                return (AxisErrorFlags & LasalHardwareMinimumErrorMask) != 0;
            }
        }

        public bool HasHardwareMaximumLimitError
        {
            get
            {
                return (AxisErrorFlags & LasalHardwareMaximumErrorMask) != 0;
            }
        }

        public bool IsDs402InternalLimitActive
        {
            get
            {
                return (Ds402StatusWord & Ds402InternalLimitActiveMask) != 0;
            }
        }

        public bool HasAnyLimitIndication
        {
            get
            {
                return IsLasalPositionLimitActive
                    || HasSoftwareMinimumLimitError
                    || HasSoftwareMaximumLimitError
                    || HasHardwareMinimumLimitError
                    || HasHardwareMaximumLimitError
                    || IsDs402InternalLimitActive;
            }
        }
    }

    public enum LMCSdoReadCommandStage
    {
        CapabilityPreflight = 0,
        Submission = 1,
        StatusPolling = 2
    }

    /// <summary>
    /// Reports a diagnostics command rejection raised by the bounded SDO Read
    /// facade. CapabilityPreflight and Submission failures have no accepted
    /// ticket. StatusPolling failures preserve the accepted PLC ticket for
    /// explicit recovery.
    /// </summary>
    public sealed class LMCSdoReadCommandException
        : LMCDiagnosticsCommandException
    {
        internal LMCSdoReadCommandException(
            LMCSdoReadCommandStage stage,
            LMCOperationTicket ticket,
            LMCDiagnosticsCommandException innerException)
            : base(
                CreateMessage(stage, ticket, innerException),
                innerException == null ? null : innerException.Response,
                innerException)
        {
            if (innerException == null)
            {
                throw new ArgumentNullException("innerException");
            }

            if (stage == LMCSdoReadCommandStage.CapabilityPreflight
                || stage == LMCSdoReadCommandStage.Submission)
            {
                if (ticket != null)
                {
                    throw new ArgumentException(
                        "Pre-ticket SDO Read command failures cannot have an accepted ticket.",
                        "ticket");
                }
            }
            else if (stage == LMCSdoReadCommandStage.StatusPolling)
            {
                if (ticket == null)
                {
                    throw new ArgumentNullException(
                        "ticket",
                        "Status-polling SDO Read failures require the accepted ticket.");
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException("stage");
            }

            Stage = stage;
            Ticket = ticket;
        }

        public LMCSdoReadCommandStage Stage { get; private set; }

        /// <summary>
        /// Gets the accepted PLC ticket for StatusPolling failures, or null when
        /// the diagnostics command failed before a ticket was accepted.
        /// </summary>
        public LMCOperationTicket Ticket { get; private set; }

        private static string CreateMessage(
            LMCSdoReadCommandStage stage,
            LMCOperationTicket ticket,
            LMCDiagnosticsCommandException innerException)
        {
            var commandMessage = innerException == null
                ? "The diagnostics command failed."
                : innerException.Message;

            if (stage == LMCSdoReadCommandStage.CapabilityPreflight)
            {
                return "SDO Read diagnostics command failed during capability preflight. Stage=CapabilityPreflight. "
                    + commandMessage;
            }

            if (stage == LMCSdoReadCommandStage.Submission)
            {
                return "SDO Read diagnostics command failed before a PLC ticket was accepted. Stage=Submission. "
                    + commandMessage;
            }

            if (stage == LMCSdoReadCommandStage.StatusPolling)
            {
                return "SDO Read diagnostics command failed while polling PLC ticket status. Stage=StatusPolling, TicketId="
                    + (ticket == null ? 0u : ticket.TicketId)
                    + ". "
                    + commandMessage;
            }

            return "SDO Read diagnostics command failed at an unknown facade stage. "
                + commandMessage;
        }
    }

    /// <summary>
    /// Reports a terminal D5 SDO Read failure while preserving the PLC ticket
    /// and terminal status fields.
    /// </summary>
    public sealed class LMCSdoReadOperationException : InvalidOperationException
    {
        internal LMCSdoReadOperationException(
            LMCOperationTicket ticket,
            LMCOperationStatus status)
            : base(CreateMessage(ticket, status))
        {
            Ticket = ticket ?? throw new ArgumentNullException("ticket");
            OperationStatus = status
                ?? throw new ArgumentNullException("status");
        }

        public LMCOperationTicket Ticket { get; private set; }
        public LMCOperationStatus OperationStatus { get; private set; }

        private static string CreateMessage(
            LMCOperationTicket ticket,
            LMCOperationStatus status)
        {
            if (ticket == null || status == null)
            {
                return "SDO Read reached an invalid terminal result.";
            }

            return "SDO Read failed. TicketId="
                + ticket.TicketId
                + ", State="
                + status.State
                + ", Outcome="
                + status.Outcome
                + ", ErrorId="
                + status.OperationErrorId
                + ", Detail=0x"
                + status.OperationDetail.ToString("X8")
                + ".";
        }
    }

    /// <summary>
    /// Reports that the PC-side bounded status polling limit was reached. The
    /// PLC ticket is preserved because it was not cancelled and can still be
    /// inspected through LMCDiagnostics.GetOperationStatus.
    /// </summary>
    public sealed class LMCSdoReadPollingTimeoutException : TimeoutException
    {
        internal LMCSdoReadPollingTimeoutException(
            LMCOperationTicket ticket,
            int pollCount)
            : base(CreateMessage(ticket, pollCount))
        {
            Ticket = ticket ?? throw new ArgumentNullException("ticket");
            PollCount = pollCount;
        }

        public LMCOperationTicket Ticket { get; private set; }
        public int PollCount { get; private set; }

        private static string CreateMessage(
            LMCOperationTicket ticket,
            int pollCount)
        {
            return "SDO Read did not reach a terminal state after "
                + pollCount
                + " status polls. TicketId="
                + (ticket == null ? 0u : ticket.TicketId)
                + ". The PLC ticket was not cancelled and may still be active.";
        }
    }

    /// <summary>
    /// Reports cancellation of the PC-side wait after an SDO ticket was
    /// submitted. The ticket is not cancelled on the PLC and is preserved for
    /// explicit status inspection or queued-only cancellation by the caller.
    /// </summary>
    public sealed class LMCSdoReadWaitCanceledException
        : OperationCanceledException
    {
        internal LMCSdoReadWaitCanceledException(
            LMCOperationTicket ticket,
            OperationCanceledException innerException,
            System.Threading.CancellationToken cancellationToken)
            : base(
                CreateMessage(ticket),
                innerException,
                cancellationToken)
        {
            Ticket = ticket ?? throw new ArgumentNullException("ticket");
        }

        public LMCOperationTicket Ticket { get; private set; }

        private static string CreateMessage(LMCOperationTicket ticket)
        {
            return "The PC-side SDO Read wait was cancelled. TicketId="
                + (ticket == null ? 0u : ticket.TicketId)
                + ". The PLC ticket was not cancelled and may still be active.";
        }
    }

    internal sealed class LMCInlineSdoReadCompletion
    {
        internal LMCInlineSdoReadCompletion(
            LMCOperationTicket ticket,
            LMCOperationStatus status)
        {
            Ticket = ticket ?? throw new ArgumentNullException("ticket");
            Status = status ?? throw new ArgumentNullException("status");
        }

        internal LMCOperationTicket Ticket { get; private set; }
        internal LMCOperationStatus Status { get; private set; }
    }

    internal sealed class LMCInlineSdoReadSubmission
    {
        internal LMCInlineSdoReadSubmission(
            LMCOperationTicket ticket,
            uint baseCycleTimeUs)
        {
            if (baseCycleTimeUs == 0)
            {
                throw new ArgumentOutOfRangeException("baseCycleTimeUs");
            }

            Ticket = ticket ?? throw new ArgumentNullException("ticket");
            BaseCycleTimeUs = baseCycleTimeUs;
        }

        internal LMCOperationTicket Ticket { get; private set; }
        internal uint BaseCycleTimeUs { get; private set; }
    }
}
