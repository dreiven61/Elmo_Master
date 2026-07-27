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

        public string AxisName { get; private set; }
        public ushort AxisReference { get; private set; }
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
            AxisReference = ResolveAxisReference(axisName);

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
            ushort axisReference,
            LMC_Response axisInfoResponse)
        {
            this.connection = connection;
            this.sessionGeneration = sessionGeneration;
            AxisName = axisName;
            AxisReference = axisReference;
            AxisInfoResponse = axisInfoResponse;
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

            LMC_Response lookupResponse;
            ushort axisReference;
            var lookupRaw = await connection.ExchangeAsync(
                LMC_Frame.LMCAxisGetByName(axisName),
                generation,
                cancellationToken).ConfigureAwait(false);

            if (!LMCConnection.TryParseLookupReference(
                lookupRaw,
                out lookupResponse,
                out axisReference))
            {
                throw LMCConnection.CreateLookupFailureException(
                    "Axis",
                    axisName,
                    lookupRaw);
            }

            var axisInfoResponse = LMCConnection.ParseCommandAcknowledgement(
                await connection.ExchangeAsync(
                    LMC_Frame.LMCAxisInfo(axisReference),
                    generation,
                    cancellationToken).ConfigureAwait(false),
                "AxisInfo");

            EnsureSuccess("AxisInfo", axisInfoResponse);
            ValidateAxisInfoResponse(axisInfoResponse, axisReference);
            connection.EnsureSessionGeneration(generation);

            return new LMCSingleAxis(
                connection,
                axisName,
                generation,
                axisReference,
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

        public LMC_Response PowerOn()
        {
            return SendPower(true);
        }

        public Task<LMC_Response> PowerOnAsync(CancellationToken cancellationToken)
        {
            return SendAsync(
                LMC_Frame.LMCAxisPower(AxisReference, true),
                cancellationToken);
        }

        public LMC_Response PowerOff()
        {
            return SendPower(false);
        }

        public Task<LMC_Response> PowerOffAsync(CancellationToken cancellationToken)
        {
            return SendAsync(
                LMC_Frame.LMCAxisPower(AxisReference, false),
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
            return LMCConnection.ParseReadStatusResult(
                connection.Exchange(
                    LMC_Frame.LMCAxisReadStatus(AxisReference),
                    sessionGeneration));
        }

        public async Task<LMCReadStatusResult> ReadStatusResultAsync(
            CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            var raw = await connection.ExchangeAsync(
                LMC_Frame.LMCAxisReadStatus(AxisReference),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            return LMCConnection.ParseReadStatusResult(raw);
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

        private ushort ResolveAxisReference(string axisName)
        {
            EnsureCurrentSessionForUse();
            ushort axisReference;
            var lookupRaw = connection.Exchange(
                LMC_Frame.LMCAxisGetByName(axisName),
                sessionGeneration);

            if (!LMCConnection.TryParseLookupReference(
                lookupRaw,
                out _,
                out axisReference))
            {
                throw LMCConnection.CreateLookupFailureException(
                    "Axis",
                    axisName,
                    lookupRaw);
            }

            return axisReference;
        }

        private LMC_Response SendPower(bool enable)
        {
            return Send(LMC_Frame.LMCAxisPower(AxisReference, enable));
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
            return LMCConnection.ParseCommandAcknowledgement(
                connection.Exchange(request, sessionGeneration),
                "Axis command");
        }

        private async Task<LMC_Response> SendAsync(
            byte[] request,
            CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            var raw = await connection.ExchangeAsync(
                request,
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            return LMCConnection.ParseCommandAcknowledgement(raw, "Axis command");
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
