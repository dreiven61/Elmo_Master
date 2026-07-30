using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    internal sealed class RecorderQualificationCleanupOperations
    {
        internal Func<Task<LMCRecorderStatus>> ReadStatusAsync { get; set; }
        internal Func<Task> StopAsync { get; set; }
        internal Action<LMCRecorderStatus> ValidateStatus { get; set; }
        internal Func<bool> IsBufferReleasePending { get; set; }
        internal Func<bool> IsConfigurationReleasePending { get; set; }
        internal Func<Task> ReleaseBufferAsync { get; set; }
        internal Func<Task> ReleaseConfigurationAsync { get; set; }
        internal Func<int, Task> DelayAsync { get; set; }
        internal Action<LMCRecorderStatus> StopRaceResolved { get; set; }
        internal Action<LMCRecorderStatus> RecoveryRequired { get; set; }
    }

    internal sealed class RecorderReleasableStateResult
    {
        internal RecorderReleasableStateResult(
            LMCRecorderStatus status,
            bool stopAttempted,
            bool stopRaceResolved,
            int statusReadCount)
        {
            Status = status;
            StopAttempted = stopAttempted;
            StopRaceResolved = stopRaceResolved;
            StatusReadCount = statusReadCount;
        }

        internal LMCRecorderStatus Status { get; private set; }
        internal bool StopAttempted { get; private set; }
        internal bool StopRaceResolved { get; private set; }
        internal int StatusReadCount { get; private set; }
    }

    internal static class RecorderQualificationCleanupOrchestrator
    {
        internal static void ThrowIfCancellationRequestedAfterRpc(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
        }

        internal static void ValidateReconnectAdoption(
            LMCRecorderIdentity identity,
            uint diagnosticsBootId,
            uint recordId,
            uint bufferId,
            uint mapRevision,
            uint previousOwnerSessionEpoch)
        {
            if (identity == null
                || identity.Response == null
                || !identity.Response.IsSuccess
                || identity.DiagnosticsBootId != diagnosticsBootId
                || identity.RecordId != recordId
                || identity.BufferId != bufferId
                || identity.MapRevision != mapRevision
                || identity.OwnerSessionEpoch == 0
                || identity.OwnerSessionEpoch == previousOwnerSessionEpoch
                || identity.InitialState < LMCRecorderState.Armed
                || identity.InitialState > LMCRecorderState.Uploading)
            {
                throw new InvalidOperationException(
                    "Adopt did not return the preserved Recorder identity with a new owner session epoch.");
            }
        }

        internal static void ValidateReconnectStatus(
            LMCRecorderStatus status,
            LMCRecorderIdentity identity,
            uint diagnosticsBootId,
            uint recordId,
            uint bufferId,
            uint configId,
            uint configRevision,
            uint mapRevision,
            uint capacity)
        {
            if (status == null
                || status.Response == null
                || !status.Response.IsSuccess
                || identity == null
                || status.DiagnosticsBootId != diagnosticsBootId
                || status.RecordId != recordId
                || status.BufferId != bufferId
                || status.ConfigId != configId
                || status.ConfigRevision != configRevision
                || status.MapRevision != mapRevision
                || status.Capacity != capacity
                || status.OwnerSessionEpoch != identity.OwnerSessionEpoch)
            {
                throw new InvalidOperationException(
                    "Adopted Recorder status does not match the preserved identity and configuration revisions.");
            }
        }

        internal static async Task<RecorderReleasableStateResult>
            EnsureReleasableStateAsync(
                RecorderQualificationCleanupOperations operations,
                int timeoutMilliseconds,
                int pollMilliseconds)
        {
            ValidateStateOperations(
                operations,
                timeoutMilliseconds,
                pollMilliseconds);

            var statusReadCount = 0;
            var stopAttempted = false;
            var stopRaceResolved = false;
            var status = await ReadAndValidateStatusAsync(operations);
            statusReadCount++;
            var action = RecorderReconnectQualificationPolicy
                .SelectCleanupAction(status.State);
            if (action == RecorderQualificationCleanupAction.Preserve)
            {
                ThrowRecoveryRequired(operations, status);
            }

            if (action == RecorderQualificationCleanupAction.Release)
            {
                return new RecorderReleasableStateResult(
                    status,
                    false,
                    false,
                    statusReadCount);
            }

            stopAttempted = true;
            try
            {
                await operations.StopAsync();
            }
            catch (LMCDiagnosticsCommandException error)
                when (error.Response != null
                    && error.Response.Detail
                        == LMCDiagnosticsDetailCode.InvalidState)
            {
                status = await ReadAndValidateStatusAsync(operations);
                statusReadCount++;
                if (!RecorderReconnectQualificationPolicy
                    .CanContinueAfterRejectedStop(
                        error.Response.Detail,
                        status.State))
                {
                    if (RecorderReconnectQualificationPolicy
                            .SelectCleanupAction(status.State)
                        == RecorderQualificationCleanupAction.Preserve
                        && operations.RecoveryRequired != null)
                    {
                        operations.RecoveryRequired(status);
                    }

                    throw;
                }

                stopRaceResolved = true;
                if (operations.StopRaceResolved != null)
                {
                    operations.StopRaceResolved(status);
                }

                return new RecorderReleasableStateResult(
                    status,
                    stopAttempted,
                    stopRaceResolved,
                    statusReadCount);
            }

            var timeout = Stopwatch.StartNew();
            while (timeout.ElapsedMilliseconds <= timeoutMilliseconds)
            {
                status = await ReadAndValidateStatusAsync(operations);
                statusReadCount++;
                action = RecorderReconnectQualificationPolicy
                    .SelectCleanupAction(status.State);
                if (action == RecorderQualificationCleanupAction.Preserve)
                {
                    ThrowRecoveryRequired(operations, status);
                }

                if (action == RecorderQualificationCleanupAction.Release)
                {
                    return new RecorderReleasableStateResult(
                        status,
                        stopAttempted,
                        stopRaceResolved,
                        statusReadCount);
                }

                await operations.DelayAsync(pollMilliseconds);
            }

            throw new TimeoutException(
                "Recorder cleanup did not reach releasable Ready/Uploading state within "
                + timeoutMilliseconds
                + " ms.");
        }

        internal static async Task<RecorderReleasableStateResult>
            CleanupOwnedResourcesAsync(
                RecorderQualificationCleanupOperations operations,
                int timeoutMilliseconds,
                int pollMilliseconds)
        {
            if (operations == null)
            {
                throw new ArgumentNullException("operations");
            }

            if (operations.IsBufferReleasePending == null
                || operations.IsConfigurationReleasePending == null)
            {
                throw new ArgumentException(
                    "Recorder cleanup release-state delegates are required.",
                    "operations");
            }

            RecorderReleasableStateResult result = null;
            if (operations.IsBufferReleasePending())
            {
                if (operations.ReleaseBufferAsync == null)
                {
                    throw new ArgumentException(
                        "Recorder buffer release delegate is required.",
                        "operations");
                }

                result = await EnsureReleasableStateAsync(
                    operations,
                    timeoutMilliseconds,
                    pollMilliseconds);
                await operations.ReleaseBufferAsync();
                if (operations.IsBufferReleasePending())
                {
                    throw new InvalidOperationException(
                        "Recorder buffer Release ACK did not update the local release state.");
                }
            }

            if (operations.IsConfigurationReleasePending())
            {
                if (operations.IsBufferReleasePending())
                {
                    throw new InvalidOperationException(
                        "Recorder configuration release is blocked until its buffer is released.");
                }

                if (operations.ReleaseConfigurationAsync == null)
                {
                    throw new ArgumentException(
                        "Recorder configuration release delegate is required.",
                        "operations");
                }

                await operations.ReleaseConfigurationAsync();
                if (operations.IsConfigurationReleasePending())
                {
                    throw new InvalidOperationException(
                        "Recorder configuration Release ACK did not update the local release state.");
                }
            }

            return result;
        }

        internal static async Task RunWithCleanupAsync(
            Func<Task> primaryAsync,
            Func<Task> cleanupAsync,
            Func<Exception, Exception, Exception> aggregateFactory)
        {
            if (primaryAsync == null)
            {
                throw new ArgumentNullException("primaryAsync");
            }

            if (cleanupAsync == null)
            {
                throw new ArgumentNullException("cleanupAsync");
            }

            if (aggregateFactory == null)
            {
                throw new ArgumentNullException("aggregateFactory");
            }

            Exception primaryError = null;
            try
            {
                await primaryAsync();
            }
            catch (Exception error)
            {
                primaryError = error;
            }

            await CleanupAndRethrowPrimaryAsync(
                primaryError,
                cleanupAsync,
                aggregateFactory);
        }

        internal static async Task CleanupAndRethrowPrimaryAsync(
            Exception primaryError,
            Func<Task> cleanupAsync,
            Func<Exception, Exception, Exception> aggregateFactory)
        {
            if (cleanupAsync == null)
            {
                throw new ArgumentNullException("cleanupAsync");
            }

            if (aggregateFactory == null)
            {
                throw new ArgumentNullException("aggregateFactory");
            }

            try
            {
                await cleanupAsync();
            }
            catch (Exception cleanupError)
            {
                if (primaryError != null)
                {
                    throw aggregateFactory(primaryError, cleanupError);
                }

                ExceptionDispatchInfo.Capture(cleanupError).Throw();
                throw new InvalidOperationException(
                    "Recorder cleanup failure was not rethrown.");
            }

            if (primaryError != null)
            {
                ExceptionDispatchInfo.Capture(primaryError).Throw();
                throw new InvalidOperationException(
                    "Recorder primary failure was not rethrown.");
            }
        }

        private static async Task<LMCRecorderStatus>
            ReadAndValidateStatusAsync(
                RecorderQualificationCleanupOperations operations)
        {
            var status = await operations.ReadStatusAsync();
            operations.ValidateStatus(status);
            return status;
        }

        private static void ThrowRecoveryRequired(
            RecorderQualificationCleanupOperations operations,
            LMCRecorderStatus status)
        {
            if (operations.RecoveryRequired != null)
            {
                operations.RecoveryRequired(status);
            }

            throw new InvalidOperationException(
                "Recorder cleanup found non-releasable State="
                + status.State
                + "; automatic Stop/Release is forbidden.");
        }

        private static void ValidateStateOperations(
            RecorderQualificationCleanupOperations operations,
            int timeoutMilliseconds,
            int pollMilliseconds)
        {
            if (operations == null)
            {
                throw new ArgumentNullException("operations");
            }

            if (operations.ReadStatusAsync == null
                || operations.StopAsync == null
                || operations.ValidateStatus == null
                || operations.DelayAsync == null)
            {
                throw new ArgumentException(
                    "Recorder status, Stop, validation, and delay delegates are required.",
                    "operations");
            }

            if (timeoutMilliseconds <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "timeoutMilliseconds");
            }

            if (pollMilliseconds < 0)
            {
                throw new ArgumentOutOfRangeException("pollMilliseconds");
            }
        }
    }
}
