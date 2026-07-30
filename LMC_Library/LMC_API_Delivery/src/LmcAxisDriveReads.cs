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
            return RunTrackedDriveRead(
                LMCDriveReadOperationKind.DriveOperationMode,
                attemptTracker =>
                {
                    EnsureDriveReadAxis();
                    var completion = connection.Diagnostics
                        .ReadInlineSdoToTerminal(
                            CreateOperationModeRequest(timeoutCycles),
                            sessionGeneration,
                            attemptTracker);
                    attemptTracker.BeginResultMaterialization();
                    EnsureCurrentSessionForUse();
                    return new LMCDriveOperationModeResult(
                        AxisReference,
                        completion);
                });
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
            return await RunTrackedDriveReadAsync(
                LMCDriveReadOperationKind.DriveOperationMode,
                async attemptTracker =>
                {
                    EnsureDriveReadAxis();
                    var completion = await connection.Diagnostics
                        .ReadInlineSdoToTerminalAsync(
                            CreateOperationModeRequest(timeoutCycles),
                            sessionGeneration,
                            attemptTracker,
                            cancellationToken).ConfigureAwait(false);
                    attemptTracker.BeginResultMaterialization();
                    EnsureCurrentSessionForUse();
                    return new LMCDriveOperationModeResult(
                        AxisReference,
                        completion);
                }).ConfigureAwait(false);
        }

        public LMCDriveErrorCodeResult GetDriveErrorCode()
        {
            return GetDriveErrorCode(DefaultDriveReadTimeoutCycles);
        }

        /// <summary>
        /// Reads CiA 402 object 0x603F:0 exactly once through the adapter's D5
        /// general-inline SDO path and returns the immutable ticket evidence.
        /// </summary>
        public LMCDriveErrorCodeResult GetDriveErrorCode(
            uint timeoutCycles)
        {
            return RunTrackedDriveRead(
                LMCDriveReadOperationKind.DriveErrorCode,
                attemptTracker =>
                {
                    EnsureDriveReadAxis();
                    var completion = connection.Diagnostics
                        .ReadInlineSdoToTerminal(
                            CreateDriveErrorCodeRequest(timeoutCycles),
                            sessionGeneration,
                            attemptTracker);
                    attemptTracker.BeginResultMaterialization();
                    EnsureCurrentSessionForUse();
                    return new LMCDriveErrorCodeResult(
                        AxisReference,
                        completion);
                });
        }

        public Task<LMCDriveErrorCodeResult> GetDriveErrorCodeAsync(
            CancellationToken cancellationToken)
        {
            return GetDriveErrorCodeAsync(
                DefaultDriveReadTimeoutCycles,
                cancellationToken);
        }

        /// <summary>
        /// Reads CiA 402 object 0x603F:0 exactly once through the adapter's D5
        /// general-inline SDO path. Cancellation stops only the PC-side wait.
        /// </summary>
        public async Task<LMCDriveErrorCodeResult> GetDriveErrorCodeAsync(
            uint timeoutCycles,
            CancellationToken cancellationToken)
        {
            return await RunTrackedDriveReadAsync(
                LMCDriveReadOperationKind.DriveErrorCode,
                async attemptTracker =>
                {
                    EnsureDriveReadAxis();
                    var completion = await connection.Diagnostics
                        .ReadInlineSdoToTerminalAsync(
                            CreateDriveErrorCodeRequest(timeoutCycles),
                            sessionGeneration,
                            attemptTracker,
                            cancellationToken).ConfigureAwait(false);
                    attemptTracker.BeginResultMaterialization();
                    EnsureCurrentSessionForUse();
                    return new LMCDriveErrorCodeResult(
                        AxisReference,
                        completion);
                }).ConfigureAwait(false);
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
            return RunTrackedDriveRead(
                LMCDriveReadOperationKind.DriveStatus,
                attemptTracker =>
                {
                    EnsureDriveReadAxis();

                    attemptTracker.BeginAxisStatusRead();
                    var axisStatus = ReadStatusResult();
                    EnsureSuccess(
                        "ReadDriveStatus axis ReadStatus",
                        axisStatus.IsReadSuccessful,
                        axisStatus.Response);
                    attemptTracker.MarkAxisStatusReadCompleted();

                    var statusWordCompletion = connection.Diagnostics
                        .ReadInlineSdoToTerminal(
                            CreateStatusWordRequest(sdoTimeoutCycles),
                            sessionGeneration,
                            attemptTracker);
                    var operationModeCompletion = connection.Diagnostics
                        .ReadInlineSdoToTerminal(
                            CreateOperationModeRequest(sdoTimeoutCycles),
                            sessionGeneration,
                            attemptTracker);

                    attemptTracker.BeginResultMaterialization();
                    EnsureCurrentSessionForUse();
                    var operationMode = new LMCDriveOperationModeResult(
                        AxisReference,
                        operationModeCompletion);
                    return new LMCDriveStatus(
                        AxisReference,
                        axisStatus,
                        statusWordCompletion,
                        operationMode);
                });
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
            return await RunTrackedDriveReadAsync(
                LMCDriveReadOperationKind.DriveStatus,
                async attemptTracker =>
                {
                    EnsureDriveReadAxis();

                    attemptTracker.BeginAxisStatusRead();
                    var axisStatus = await ReadStatusResultAsync(
                        cancellationToken).ConfigureAwait(false);
                    EnsureSuccess(
                        "ReadDriveStatus axis ReadStatus",
                        axisStatus.IsReadSuccessful,
                        axisStatus.Response);
                    attemptTracker.MarkAxisStatusReadCompleted();

                    var statusWordCompletion = await connection.Diagnostics
                        .ReadInlineSdoToTerminalAsync(
                            CreateStatusWordRequest(sdoTimeoutCycles),
                            sessionGeneration,
                            attemptTracker,
                            cancellationToken).ConfigureAwait(false);
                    var operationModeCompletion = await connection.Diagnostics
                        .ReadInlineSdoToTerminalAsync(
                            CreateOperationModeRequest(sdoTimeoutCycles),
                            sessionGeneration,
                            attemptTracker,
                            cancellationToken).ConfigureAwait(false);

                    attemptTracker.BeginResultMaterialization();
                    EnsureCurrentSessionForUse();
                    var operationMode = new LMCDriveOperationModeResult(
                        AxisReference,
                        operationModeCompletion);
                    return new LMCDriveStatus(
                        AxisReference,
                        axisStatus,
                        statusWordCompletion,
                        operationMode);
                }).ConfigureAwait(false);
        }

        private T RunTrackedDriveRead<T>(
            LMCDriveReadOperationKind operationKind,
            Func<LMCDriveReadAttemptTracker, T> read)
        {
            if (read == null)
            {
                throw new ArgumentNullException("read");
            }

            var attemptTracker = new LMCDriveReadAttemptTracker(
                operationKind,
                AxisReference);
            try
            {
                return read(attemptTracker);
            }
            catch (Exception exception)
            {
                LMCDriveReadFailureContext.Attach(
                    exception,
                    attemptTracker.CreateFailureContext());
                throw;
            }
        }

        private async Task<T> RunTrackedDriveReadAsync<T>(
            LMCDriveReadOperationKind operationKind,
            Func<LMCDriveReadAttemptTracker, Task<T>> read)
        {
            if (read == null)
            {
                throw new ArgumentNullException("read");
            }

            var attemptTracker = new LMCDriveReadAttemptTracker(
                operationKind,
                AxisReference);
            try
            {
                return await read(attemptTracker).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                LMCDriveReadFailureContext.Attach(
                    exception,
                    attemptTracker.CreateFailureContext());
                throw;
            }
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

        private LMCSdoRequest CreateDriveErrorCodeRequest(
            uint timeoutCycles)
        {
            return LMCSdoRequest.CreateRead(
                AxisReference,
                0x603F,
                0,
                LMCSignalValueType.UInt16,
                2,
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
