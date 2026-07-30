using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public partial class LMCSingleAxis
    {
        private readonly LMCConnection connection;
        private readonly long sessionGeneration;
        private readonly LMCAxisPowerOnWaitCoordinator powerOnWaitCoordinator;

        public string AxisName { get; private set; }
        public ushort AxisReference { get; private set; }
        public LMCLookupResult LookupResult { get; private set; }
        public LMC_Response AxisInfoResponse { get; private set; }

        internal LMCConnection Connection
        {
            get { return connection; }
        }

        internal long SessionGeneration
        {
            get { return sessionGeneration; }
        }

        public LMCSingleAxis(LMCConnection connection, string axisName)
        {
            this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
            sessionGeneration = connection.SessionGeneration;
            EnsureCurrentSessionForUse();

            AxisName = axisName;
            LookupResult = ResolveAxisLookup(axisName);
            AxisReference = LookupResult.Reference;
            powerOnWaitCoordinator = connection.GetAxisPowerOnWaitCoordinator(
                sessionGeneration,
                AxisReference);

            EnsureCurrentSessionForUse();
            AxisInfoResponse = LMCConnection.ParseCommandAcknowledgement(
                connection.Exchange(
                    LMC_Frame.LMCAxisInfo(AxisReference),
                    sessionGeneration),
                "AxisInfo");

            EnsureSuccess("AxisInfo", AxisInfoResponse);
            ValidateAxisInfoResponse(AxisInfoResponse, AxisReference);
        }

        private LMCSingleAxis(
            LMCConnection connection,
            string axisName,
            long sessionGeneration,
            LMCLookupResult lookupResult,
            LMC_Response axisInfoResponse)
        {
            this.connection = connection;
            this.sessionGeneration = sessionGeneration;
            AxisName = axisName;
            LookupResult = lookupResult;
            AxisReference = lookupResult.Reference;
            AxisInfoResponse = axisInfoResponse;
            powerOnWaitCoordinator = connection.GetAxisPowerOnWaitCoordinator(
                sessionGeneration,
                AxisReference);
        }

        public static async Task<LMCSingleAxis> CreateAsync(
            LMCConnection connection,
            string axisName,
            CancellationToken cancellationToken)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            var generation = connection.SessionGeneration;
            connection.EnsureSessionGeneration(generation);

            var lookupRaw = await connection.ExchangeAsync(
                LMC_Frame.LMCAxisGetByName(axisName),
                generation,
                cancellationToken).ConfigureAwait(false);
            var lookupResult = LMCConnection.ParseLookupResult(
                LMCLookupTargetKind.Axis,
                axisName,
                lookupRaw);

            var axisInfoResponse = LMCConnection.ParseCommandAcknowledgement(
                await connection.ExchangeAsync(
                    LMC_Frame.LMCAxisInfo(lookupResult.Reference),
                    generation,
                    cancellationToken).ConfigureAwait(false),
                "AxisInfo");

            EnsureSuccess("AxisInfo", axisInfoResponse);
            ValidateAxisInfoResponse(
                axisInfoResponse,
                lookupResult.Reference);
            connection.EnsureSessionGeneration(generation);

            return new LMCSingleAxis(
                connection,
                axisName,
                generation,
                lookupResult,
                axisInfoResponse);
        }

        private static void ValidateAxisInfoResponse(
            LMC_Response response,
            ushort expectedAxisReference)
        {
            if (response == null
                || !response.IsFrameValid
                || response.PayloadLength != 8
                || !response.HasCommandResult)
            {
                throw new InvalidDataException(
                    "AxisInfo response must contain an 8-byte acknowledgement payload.");
            }

            var actualAxisReference = LMC_Frame.ReadUInt32(response.Payload, 0);
            if (actualAxisReference != expectedAxisReference)
            {
                throw new InvalidDataException(
                    "AxisInfo response descriptor 0x"
                    + actualAxisReference.ToString("X8")
                    + " does not match expected axis reference 0x"
                    + expectedAxisReference.ToString("X4")
                    + ".");
            }
        }

        /// <summary>
        /// Sends the legacy raw Axis Power On request. If an accepted-once
        /// Power On continuation is pending, this method throws
        /// LMCAxisPowerOnPendingException instead of replaying 0x2023. Use
        /// PendingPowerOnWaitContinuation and the status-only resume or safe
        /// Power Off resolution APIs first.
        /// </summary>
        public LMC_Response PowerOn()
        {
            return SendPowerOnWithPendingGuard();
        }

        /// <summary>
        /// Asynchronous legacy raw Axis Power On with the same pending-
        /// continuation replay guard as PowerOn().
        /// </summary>
        public Task<LMC_Response> PowerOnAsync(CancellationToken cancellationToken)
        {
            return SendPowerOnWithPendingGuardAsync(cancellationToken);
        }

        public LMC_Response PowerOff()
        {
            return SendPowerOffWithAcceptanceObserverGuard();
        }

        public Task<LMC_Response> PowerOffAsync(CancellationToken cancellationToken)
        {
            return SendPowerOffWithAcceptanceObserverGuardAsync(
                cancellationToken);
        }

        public LMC_Response Reset()
        {
            return SendReset();
        }

        public Task<LMC_Response> ResetAsync(CancellationToken cancellationToken)
        {
            return SendAsync(
                LMC_Frame.LMCAxisReset(AxisReference),
                cancellationToken);
        }

        public LMC_Response Stop(int deceleration, int jerk)
        {
            return SendStop(deceleration, jerk);
        }

        public Task<LMC_Response> StopAsync(
            int deceleration,
            int jerk,
            CancellationToken cancellationToken)
        {
            return SendAsync(
                LMC_Frame.LMCAxisStop(AxisReference, deceleration, jerk),
                cancellationToken);
        }

        public LMC_Response MoveAbsoluteEx(
            int position,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMC_DIRECTION direction = LMC_DIRECTION.Shortest)
        {
            return SendMoveAbsolute(position, velocity, acceleration, deceleration, jerk, direction);
        }

        public Task<LMC_Response> MoveAbsoluteExAsync(
            int position,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMC_DIRECTION direction,
            CancellationToken cancellationToken)
        {
            return SendAsync(
                LMC_Frame.LMCAxisMoveAbsolute(
                    AxisReference,
                    position,
                    velocity,
                    acceleration,
                    deceleration,
                    jerk,
                    direction),
                cancellationToken);
        }

        public LMC_Response MoveRelativeEx(
            int distance,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMC_DIRECTION direction = LMC_DIRECTION.Shortest)
        {
            return SendMoveRelative(distance, velocity, acceleration, deceleration, jerk, direction);
        }

        public Task<LMC_Response> MoveRelativeExAsync(
            int distance,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMC_DIRECTION direction,
            CancellationToken cancellationToken)
        {
            return SendAsync(
                LMC_Frame.LMCAxisMoveRelative(
                    AxisReference,
                    distance,
                    velocity,
                    acceleration,
                    deceleration,
                    jerk,
                    direction),
                cancellationToken);
        }

        public LMC_Response MoveVelocityEx(
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMC_DIRECTION direction)
        {
            return SendMoveVelocity(velocity, acceleration, deceleration, jerk, direction);
        }

        public Task<LMC_Response> MoveVelocityExAsync(
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMC_DIRECTION direction,
            CancellationToken cancellationToken)
        {
            return SendAsync(
                LMC_Frame.LMCAxisMoveVelocity(
                    AxisReference,
                    velocity,
                    acceleration,
                    deceleration,
                    jerk,
                    direction),
                cancellationToken);
        }

        public uint ReadStatus()
        {
            LMC_Response response;
            return ReadStatusValue(out response);
        }

        public uint ReadStatus(out LMC_Response response)
        {
            return ReadStatusValue(out response);
        }

        public LMCReadStatusResult ReadStatusResult()
        {
            EnsureCurrentSessionForUse();
            var observationTarget =
                CapturePendingPowerOnStatusObservation();
            var result = LMCConnection.ParseReadStatusResult(
                connection.Exchange(
                    LMC_Frame.LMCAxisReadStatus(AxisReference),
                    sessionGeneration));
            ObservePendingPowerOnStatus(observationTarget, result);
            return result;
        }

        public async Task<LMCReadStatusResult> ReadStatusResultAsync(
            CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            var observationTarget =
                CapturePendingPowerOnStatusObservation();
            var raw = await connection.ExchangeAsync(
                LMC_Frame.LMCAxisReadStatus(AxisReference),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            var result = LMCConnection.ParseReadStatusResult(raw);
            ObservePendingPowerOnStatus(observationTarget, result);
            return result;
        }

        public int GetActualPosition()
        {
            LMC_Response response;
            return ReadActualPositionValue(out response);
        }

        public int GetActualPosition(out LMC_Response response)
        {
            return ReadActualPositionValue(out response);
        }

        public LMCReadActualPositionResult GetActualPositionResult()
        {
            EnsureCurrentSessionForUse();
            return LMCConnection.ParseReadActualPositionResult(
                connection.Exchange(
                    LMC_Frame.LMCAxisReadPosition(AxisReference),
                    sessionGeneration));
        }

        public async Task<LMCReadActualPositionResult> GetActualPositionResultAsync(
            CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            var raw = await connection.ExchangeAsync(
                LMC_Frame.LMCAxisReadPosition(AxisReference),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            return LMCConnection.ParseReadActualPositionResult(raw);
        }

        private LMCLookupResult ResolveAxisLookup(string axisName)
        {
            EnsureCurrentSessionForUse();
            var lookupRaw = connection.Exchange(
                LMC_Frame.LMCAxisGetByName(axisName),
                sessionGeneration);
            return LMCConnection.ParseLookupResult(
                LMCLookupTargetKind.Axis,
                axisName,
                lookupRaw);
        }

        private LMC_Response SendPower(bool enable)
        {
            return SendWhileAxisMutationGateHeld(
                LMC_Frame.LMCAxisPower(AxisReference, enable));
        }

        private LMC_Response SendReset()
        {
            return Send(LMC_Frame.LMCAxisReset(AxisReference));
        }

        private LMC_Response SendStop(int deceleration, int jerk)
        {
            return Send(LMC_Frame.LMCAxisStop(AxisReference, deceleration, jerk));
        }

        private LMC_Response SendMoveAbsolute(
            int position,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMC_DIRECTION direction)
        {
            return Send(
                LMC_Frame.LMCAxisMoveAbsolute(
                    AxisReference,
                    position,
                    velocity,
                    acceleration,
                    deceleration,
                    jerk,
                    direction));
        }

        private LMC_Response SendMoveRelative(
            int distance,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMC_DIRECTION direction)
        {
            return Send(
                LMC_Frame.LMCAxisMoveRelative(
                    AxisReference,
                    distance,
                    velocity,
                    acceleration,
                    deceleration,
                    jerk,
                    direction));
        }

        private LMC_Response SendMoveVelocity(
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMC_DIRECTION direction)
        {
            return Send(
                LMC_Frame.LMCAxisMoveVelocity(
                    AxisReference,
                    velocity,
                    acceleration,
                    deceleration,
                    jerk,
                    direction));
        }

        private uint ReadStatusValue(out LMC_Response response)
        {
            var result = ReadStatusResult();
            response = result.Response;
            EnsureSuccess("ReadStatus", result.IsSuccess, response);
            return result.State;
        }

        private int ReadActualPositionValue(out LMC_Response response)
        {
            var result = GetActualPositionResult();
            response = result.Response;
            EnsureSuccess("GetActualPosition", result.IsSuccess, response);
            return result.PositionRaw;
        }

        private LMC_Response Send(byte[] request)
        {
            EnsureCurrentSessionForUse();
            powerOnWaitCoordinator.MutationGate.Wait();
            try
            {
                EnsureCurrentSessionForUse();
                return SendWhileAxisMutationGateHeld(request);
            }
            finally
            {
                powerOnWaitCoordinator.MutationGate.Release();
            }
        }

        private LMC_Response SendWhileAxisMutationGateHeld(byte[] request)
        {
            EnsureAxisMutationAdmission(request);
            var reservedMutationGeneration = 0L;
            // Exchange invokes this callback after connection/session/priority
            // admission and immediately before Stream.Write may start.
            var raw = connection.Exchange(
                request,
                sessionGeneration,
                () =>
                {
                    EnsureAxisMutationAdmission(request);
                    reservedMutationGeneration = powerOnWaitCoordinator
                        .MarkMutationMayHaveBeenSent();
                });
            var response = LMCConnection.ParseCommandAcknowledgement(
                raw,
                "Axis command");
            if (!response.IsSuccess)
            {
                powerOnWaitCoordinator.TryRollbackRejectedMutation(
                    reservedMutationGeneration);
            }
            LMC_Response publishedResponse = null;
            connection.PublishSessionBoundSendPriorityResult(
                sessionGeneration,
                LMC_Frame.GetRequestCommand(request),
                () => publishedResponse = response);
            return publishedResponse;
        }

        private async Task<LMC_Response> SendAsync(
            byte[] request,
            CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            cancellationToken.ThrowIfCancellationRequested();
            await powerOnWaitCoordinator.MutationGate.WaitAsync(
                cancellationToken).ConfigureAwait(false);
            try
            {
                EnsureCurrentSessionForUse();
                return await SendAsyncWhileAxisMutationGateHeld(
                    request,
                    cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                powerOnWaitCoordinator.MutationGate.Release();
            }
        }

        private async Task<LMC_Response>
            SendAsyncWhileAxisMutationGateHeld(
            byte[] request,
            CancellationToken cancellationToken)
        {
            EnsureAxisMutationAdmission(request);
            var reservedMutationGeneration = 0L;
            // The callback is the raw async path's final write boundary; frame
            // validation and queued/pre-write cancellation happen before it.
            var raw = await connection.ExchangeAsync(
                request,
                sessionGeneration,
                cancellationToken,
                () =>
                {
                    EnsureAxisMutationAdmission(request);
                    reservedMutationGeneration = powerOnWaitCoordinator
                        .MarkMutationMayHaveBeenSent();
                }).ConfigureAwait(false);
            var response = LMCConnection.ParseCommandAcknowledgement(
                raw,
                "Axis command");
            if (!response.IsSuccess)
            {
                powerOnWaitCoordinator.TryRollbackRejectedMutation(
                    reservedMutationGeneration);
            }
            LMC_Response publishedResponse = null;
            connection.PublishSessionBoundSendPriorityResult(
                sessionGeneration,
                LMC_Frame.GetRequestCommand(request),
                () => publishedResponse = response);
            return publishedResponse;
        }

        private void EnsureNoAxisAcceptedMutationObserverInProgress()
        {
            lock (powerOnWaitCoordinator.Sync)
            {
                EnsureNoAxisAcceptedMutationObserverInProgressCore();
            }
        }

        private void EnsureAxisMutationAdmission(byte[] request)
        {
            lock (powerOnWaitCoordinator.Sync)
            {
                EnsureNoAxisAcceptedMutationObserverInProgressCore();
                var command = LMC_Frame.GetRequestCommand(request);
                var pendingStop = powerOnWaitCoordinator
                    .PendingStopContinuation;
                if ((command == LMC_CommandId.Reset
                        || command == LMC_CommandId.Stop)
                    && pendingStop != null
                    && pendingStop.IsPending)
                {
                    throw new LMCAxisStopWaitPendingException(pendingStop);
                }

                var pendingReset = powerOnWaitCoordinator
                    .PendingResetContinuation;
                if ((command == LMC_CommandId.Stop
                        || command == LMC_CommandId.Reset)
                    && pendingReset != null
                    && pendingReset.IsPending)
                {
                    throw new LMCAxisResetWaitPendingException(pendingReset);
                }
            }
        }

        private void EnsureNoAxisAcceptedMutationObserverInProgressCore()
        {
            if (powerOnWaitCoordinator.StopAcceptanceObserverInProgress)
            {
                throw new LMCAxisAcceptedObserverInProgressException(
                    powerOnWaitCoordinator.PendingStopContinuation,
                    null);
            }
            if (powerOnWaitCoordinator.ResetAcceptanceObserverInProgress)
            {
                throw new LMCAxisAcceptedObserverInProgressException(
                    null,
                    powerOnWaitCoordinator.PendingResetContinuation);
            }
        }

        internal void EnsureCurrentSessionForUse()
        {
            connection.EnsureSessionGeneration(sessionGeneration);
        }

        private static void EnsureSuccess(string operation, LMC_Response response)
        {
            EnsureSuccess(operation, response != null && response.IsSuccess, response);
        }

        private static void EnsureSuccess(
            string operation,
            bool isSuccess,
            LMC_Response response)
        {
            if (isSuccess)
            {
                return;
            }

            if (response == null)
            {
                throw new InvalidOperationException(
                    operation + " failed without a response.");
            }

            throw new InvalidOperationException(
                operation
                + " failed. Status="
                + response.Status
                + ", ErrorId="
                + response.ErrorId
                + ".");
        }
    }

    public sealed class LMCAxis : LMCSingleAxis
    {
        public LMCAxis(LMCConnection connection, string axisName)
            : base(connection, axisName)
        {
        }
    }
}
