using System;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public partial class LMCSingleAxis
    {
        public const uint DefaultDriveReadTimeoutCycles = 1000;

        public LMCDriveOperationModeResult GetDriveOperationMode()
        {
            return GetDriveOperationMode(DefaultDriveReadTimeoutCycles);
        }

        /// <summary>
        /// Reads CiA 402 object 0x6061:0 through the adapter's asynchronous D5
        /// SDO ticket and waits for a bounded terminal result.
        /// </summary>
        public LMCDriveOperationModeResult GetDriveOperationMode(
            uint timeoutCycles)
        {
            EnsureDriveReadAxis();
            var completion = connection.Diagnostics.ReadInlineSdoToTerminal(
                CreateOperationModeRequest(timeoutCycles),
                sessionGeneration);
            EnsureCurrentSessionForUse();
            return new LMCDriveOperationModeResult(
                AxisReference,
                completion);
        }

        public Task<LMCDriveOperationModeResult> GetDriveOperationModeAsync(
            CancellationToken cancellationToken)
        {
            return GetDriveOperationModeAsync(
                DefaultDriveReadTimeoutCycles,
                cancellationToken);
        }

        /// <summary>
        /// Reads CiA 402 object 0x6061:0 and waits for its D5 ticket. Cancelling
        /// the token stops the PC wait; it does not cancel an already submitted
        /// PLC ticket.
        /// </summary>
        public async Task<LMCDriveOperationModeResult>
            GetDriveOperationModeAsync(
                uint timeoutCycles,
                CancellationToken cancellationToken)
        {
            EnsureDriveReadAxis();
            var completion = await connection.Diagnostics
                .ReadInlineSdoToTerminalAsync(
                    CreateOperationModeRequest(timeoutCycles),
                    sessionGeneration,
                    cancellationToken).ConfigureAwait(false);
            EnsureCurrentSessionForUse();
            return new LMCDriveOperationModeResult(
                AxisReference,
                completion);
        }

        public LMCDriveStatus ReadDriveStatus()
        {
            return ReadDriveStatus(DefaultDriveReadTimeoutCycles);
        }

        /// <summary>
        /// Sequentially reads LASAL axis status, CiA 402 0x6041:0, and CiA 402
        /// 0x6061:0. The returned composite is not an atomic same-cycle snapshot.
        /// </summary>
        public LMCDriveStatus ReadDriveStatus(uint sdoTimeoutCycles)
        {
            EnsureDriveReadAxis();

            var axisStatus = ReadStatusResult();
            EnsureSuccess(
                "ReadDriveStatus axis ReadStatus",
                axisStatus.IsReadSuccessful,
                axisStatus.Response);

            var statusWordCompletion = connection.Diagnostics
                .ReadInlineSdoToTerminal(
                    CreateStatusWordRequest(sdoTimeoutCycles),
                    sessionGeneration);
            var operationModeCompletion = connection.Diagnostics
                .ReadInlineSdoToTerminal(
                    CreateOperationModeRequest(sdoTimeoutCycles),
                    sessionGeneration);

            EnsureCurrentSessionForUse();
            var operationMode = new LMCDriveOperationModeResult(
                AxisReference,
                operationModeCompletion);
            return new LMCDriveStatus(
                AxisReference,
                axisStatus,
                statusWordCompletion,
                operationMode);
        }

        public Task<LMCDriveStatus> ReadDriveStatusAsync(
            CancellationToken cancellationToken)
        {
            return ReadDriveStatusAsync(
                DefaultDriveReadTimeoutCycles,
                cancellationToken);
        }

        /// <summary>
        /// Sequentially reads LASAL axis status, CiA 402 0x6041:0, and CiA 402
        /// 0x6061:0. The result is not atomic. Cancellation stops the PC wait but
        /// does not cancel an already submitted PLC SDO ticket.
        /// </summary>
        public async Task<LMCDriveStatus> ReadDriveStatusAsync(
            uint sdoTimeoutCycles,
            CancellationToken cancellationToken)
        {
            EnsureDriveReadAxis();

            var axisStatus = await ReadStatusResultAsync(cancellationToken)
                .ConfigureAwait(false);
            EnsureSuccess(
                "ReadDriveStatus axis ReadStatus",
                axisStatus.IsReadSuccessful,
                axisStatus.Response);

            var statusWordCompletion = await connection.Diagnostics
                .ReadInlineSdoToTerminalAsync(
                    CreateStatusWordRequest(sdoTimeoutCycles),
                    sessionGeneration,
                    cancellationToken).ConfigureAwait(false);
            var operationModeCompletion = await connection.Diagnostics
                .ReadInlineSdoToTerminalAsync(
                    CreateOperationModeRequest(sdoTimeoutCycles),
                    sessionGeneration,
                    cancellationToken).ConfigureAwait(false);

            EnsureCurrentSessionForUse();
            var operationMode = new LMCDriveOperationModeResult(
                AxisReference,
                operationModeCompletion);
            return new LMCDriveStatus(
                AxisReference,
                axisStatus,
                statusWordCompletion,
                operationMode);
        }

        private void EnsureDriveReadAxis()
        {
            EnsureCurrentSessionForUse();
            if (AxisReference < 1 || AxisReference > 4)
            {
                throw new NotSupportedException(
                    "Drive SDO reads require the adapter's physical axis/slave mapping 1 through 4.");
            }
        }

        private LMCSdoRequest CreateOperationModeRequest(
            uint timeoutCycles)
        {
            return LMCSdoRequest.CreateRead(
                AxisReference,
                0x6061,
                0,
                LMCSignalValueType.Int8,
                1,
                timeoutCycles);
        }

        private LMCSdoRequest CreateStatusWordRequest(
            uint timeoutCycles)
        {
            return LMCSdoRequest.CreateRead(
                AxisReference,
                0x6041,
                0,
                LMCSignalValueType.BitField16,
                2,
                timeoutCycles);
        }
    }
}
