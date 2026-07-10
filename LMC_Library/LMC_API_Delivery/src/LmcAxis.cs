using System;
using System.IO;

namespace LasalMotionControlLib
{
    public class LMCSingleAxis
    {
        private readonly LMCConnection connection;

        public string AxisName { get; private set; }
        public ushort AxisReference { get; private set; }
        public LMC_Response AxisInfoResponse { get; private set; }

        public LMCSingleAxis(LMCConnection connection, string axisName)
        {
            this.connection = connection ?? throw new ArgumentNullException(nameof(connection));

            AxisName = axisName;
            AxisReference = ResolveAxisReference(axisName);

            AxisInfoResponse = LMCConnection.ParseAcknowledgement(
                connection.Exchange(LMC_Frame.LMCAxisInfo(AxisReference)));

            if (!AxisInfoResponse.IsFrameValid
                || AxisInfoResponse.PayloadLength != 8
                || !AxisInfoResponse.HasCommandResult)
            {
                throw new InvalidDataException(
                    "AxisInfo response must contain an 8-byte acknowledgement payload.");
            }

            EnsureSuccess("AxisInfo", AxisInfoResponse);
        }

        public LMC_Response PowerOn()
        {
            return SendPower(true);
        }

        public LMC_Response PowerOff()
        {
            return SendPower(false);
        }

        public LMC_Response Reset()
        {
            return SendReset();
        }

        public LMC_Response Stop(int deceleration, int jerk)
        {
            return SendStop(deceleration, jerk);
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

        public LMC_Response MoveVelocityEx(
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMC_DIRECTION direction)
        {
            return SendMoveVelocity(velocity, acceleration, deceleration, jerk, direction);
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
            return LMCConnection.ParseReadStatusResult(
                connection.Exchange(LMC_Frame.LMCAxisReadStatus(AxisReference)));
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
            return LMCConnection.ParseReadActualPositionResult(
                connection.Exchange(LMC_Frame.LMCAxisReadPosition(AxisReference)));
        }

        private ushort ResolveAxisReference(string axisName)
        {
            ushort axisReference;

            if (!LMCConnection.TryParseLookupReference(
                connection.Exchange(LMC_Frame.LMCAxisGetByName(axisName)),
                out _,
                out axisReference))
            {
                throw new InvalidOperationException("Invalid axis lookup response.");
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
            return LMCConnection.ParseAcknowledgement(connection.Exchange(request));
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
