using System;
using System.IO;

namespace LasalMotionControlLib
{
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
