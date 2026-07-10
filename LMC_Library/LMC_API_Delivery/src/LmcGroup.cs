using System;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public class LMCGroupAxis
    {
        private readonly LMCConnection connection;
        private readonly long sessionGeneration;

        public string GroupName { get; private set; }
        public ushort GroupReference { get; private set; }

        public LMCGroupAxis(LMCConnection connection, string groupName)
        {
            this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
            sessionGeneration = connection.SessionGeneration;
            EnsureCurrentSessionForUse();

            GroupName = groupName;
            GroupReference = ResolveGroupReference(groupName);
        }

        private LMCGroupAxis(
            LMCConnection connection,
            string groupName,
            long sessionGeneration,
            ushort groupReference)
        {
            this.connection = connection;
            this.sessionGeneration = sessionGeneration;
            GroupName = groupName;
            GroupReference = groupReference;
        }

        public static async Task<LMCGroupAxis> CreateAsync(
            LMCConnection connection,
            string groupName,
            CancellationToken cancellationToken)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            var generation = connection.SessionGeneration;
            connection.EnsureSessionGeneration(generation);

            LMC_Response lookupResponse;
            ushort groupReference;
            var raw = await connection.ExchangeAsync(
                LMC_Frame.LMCGroupGetByName(groupName),
                generation,
                cancellationToken).ConfigureAwait(false);

            if (!LMCConnection.TryParseLookupReference(
                raw,
                out lookupResponse,
                out groupReference))
            {
                throw new InvalidOperationException("Invalid group lookup response.");
            }

            connection.EnsureSessionGeneration(generation);
            return new LMCGroupAxis(
                connection,
                groupName,
                generation,
                groupReference);
        }

        public LMC_Response GetGroupMembersInfo()
        {
            return GetGroupMembersInfoResult().Response;
        }

        public LMCGroupMembersInfoResult GetGroupMembersInfoResult()
        {
            EnsureCurrentSessionForUse();
            return LMCConnection.ParseGroupMembersInfoResult(
                connection.Exchange(
                    LMC_Frame.LMCGroupGetMembersInfo(GroupReference),
                    sessionGeneration));
        }

        public async Task<LMCGroupMembersInfoResult> GetGroupMembersInfoResultAsync(
            CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            var raw = await connection.ExchangeAsync(
                LMC_Frame.LMCGroupGetMembersInfo(GroupReference),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            return LMCConnection.ParseGroupMembersInfoResult(raw);
        }

        public LMC_Response GroupEnable()
        {
            return SendGroupEnable();
        }

        public Task<LMC_Response> GroupEnableAsync(
            CancellationToken cancellationToken)
        {
            return SendAsync(
                LMC_Frame.LMCGroupEnable(GroupReference),
                cancellationToken);
        }

        public LMC_Response GroupDisable()
        {
            return SendGroupDisable();
        }

        public Task<LMC_Response> GroupDisableAsync(
            CancellationToken cancellationToken)
        {
            return SendAsync(
                LMC_Frame.LMCGroupDisable(GroupReference),
                cancellationToken);
        }

        public LMC_Response GroupReset()
        {
            return SendGroupReset();
        }

        public Task<LMC_Response> GroupResetAsync(
            CancellationToken cancellationToken)
        {
            return SendAsync(
                LMC_Frame.LMCGroupReset(GroupReference),
                cancellationToken);
        }

        public LMC_Response GroupStop(int deceleration, int jerk)
        {
            return SendGroupStop(deceleration, jerk);
        }

        public Task<LMC_Response> GroupStopAsync(
            int deceleration,
            int jerk,
            CancellationToken cancellationToken)
        {
            return SendAsync(
                LMC_Frame.LMCGroupStop(GroupReference, deceleration, jerk),
                cancellationToken);
        }

        public uint GroupReadStatus()
        {
            LMC_Response response;
            return ReadGroupStatusValue(out response);
        }

        public uint GroupReadStatus(out LMC_Response response)
        {
            return ReadGroupStatusValue(out response);
        }

        public LMCGroupReadStatusResult GroupReadStatusResult()
        {
            EnsureCurrentSessionForUse();
            return LMCConnection.ParseGroupReadStatusResult(
                connection.Exchange(
                    LMC_Frame.LMCGroupReadStatus(GroupReference),
                    sessionGeneration));
        }

        public async Task<LMCGroupReadStatusResult> GroupReadStatusResultAsync(
            CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            var raw = await connection.ExchangeAsync(
                LMC_Frame.LMCGroupReadStatus(GroupReference),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            return LMCConnection.ParseGroupReadStatusResult(raw);
        }

        public LMCGroupReadActualPositionResult GroupReadActualPosition(
            LMC_COORD_SYSTEM coordinateSystem)
        {
            EnsureCurrentSessionForUse();
            return LMCConnection.ParseGroupReadActualPositionResult(
                connection.Exchange(
                    LMC_Frame.LMCGroupReadActualPosition(
                        GroupReference,
                        coordinateSystem),
                    sessionGeneration),
                coordinateSystem);
        }

        public async Task<LMCGroupReadActualPositionResult>
            GroupReadActualPositionAsync(
                LMC_COORD_SYSTEM coordinateSystem,
                CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            var raw = await connection.ExchangeAsync(
                LMC_Frame.LMCGroupReadActualPosition(
                    GroupReference,
                    coordinateSystem),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            return LMCConnection.ParseGroupReadActualPositionResult(
                raw,
                coordinateSystem);
        }

        public LMC_Response MoveLinearAbsoluteEx(
            int[] position,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk)
        {
            return SendMoveLinearAbsolute(position, velocity, acceleration, deceleration, jerk);
        }

        public Task<LMC_Response> MoveLinearAbsoluteExAsync(
            int[] position,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            CancellationToken cancellationToken)
        {
            return MoveLinearAbsoluteExAsync(
                position,
                velocity,
                acceleration,
                deceleration,
                jerk,
                new LMCGroupMotionOptions(),
                cancellationToken);
        }

        public LMC_Response MoveLinearAbsoluteEx(
            int[] position,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMCGroupMotionOptions options)
        {
            return Send(
                LMC_Frame.LMCGroupMoveLinearAbsolute(
                    GroupReference,
                    position,
                    velocity,
                    acceleration,
                    deceleration,
                    jerk,
                    options));
        }

        public Task<LMC_Response> MoveLinearAbsoluteExAsync(
            int[] position,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMCGroupMotionOptions options,
            CancellationToken cancellationToken)
        {
            return SendAsync(
                LMC_Frame.LMCGroupMoveLinearAbsolute(
                    GroupReference,
                    position,
                    velocity,
                    acceleration,
                    deceleration,
                    jerk,
                    options),
                cancellationToken);
        }

        public LMC_Response SetKinTransformCartesian4Axis(
            LMCSingleAxis axisX,
            LMCSingleAxis axisY,
            LMCSingleAxis axisZ,
            LMCSingleAxis axisU)
        {
            ValidateKinematicAxes(axisX, axisY, axisZ, axisU);

            return SendShortAcknowledgement(
                LMC_Frame.LMCGroupSetKinTransformCartesian(
                    GroupReference,
                    LMCCartesianKinematicTransform.CreateFourAxis(
                        axisX.AxisReference,
                        axisY.AxisReference,
                        axisZ.AxisReference,
                        axisU.AxisReference)));
        }

        public Task<LMC_Response> SetKinTransformCartesian4AxisAsync(
            LMCSingleAxis axisX,
            LMCSingleAxis axisY,
            LMCSingleAxis axisZ,
            LMCSingleAxis axisU,
            CancellationToken cancellationToken)
        {
            ValidateKinematicAxes(axisX, axisY, axisZ, axisU);

            return SendShortAcknowledgementAsync(
                LMC_Frame.LMCGroupSetKinTransformCartesian(
                    GroupReference,
                    LMCCartesianKinematicTransform.CreateFourAxis(
                        axisX.AxisReference,
                        axisY.AxisReference,
                        axisZ.AxisReference,
                        axisU.AxisReference)),
                cancellationToken);
        }

        private ushort ResolveGroupReference(string groupName)
        {
            EnsureCurrentSessionForUse();
            ushort groupReference;

            if (!LMCConnection.TryParseLookupReference(
                connection.Exchange(
                    LMC_Frame.LMCGroupGetByName(groupName),
                    sessionGeneration),
                out _,
                out groupReference))
            {
                throw new InvalidOperationException("Invalid group lookup response.");
            }

            return groupReference;
        }

        private LMC_Response SendGroupEnable()
        {
            return Send(LMC_Frame.LMCGroupEnable(GroupReference));
        }

        private LMC_Response SendGroupDisable()
        {
            return Send(LMC_Frame.LMCGroupDisable(GroupReference));
        }

        private LMC_Response SendGroupReset()
        {
            return Send(LMC_Frame.LMCGroupReset(GroupReference));
        }

        private LMC_Response SendGroupStop(int deceleration, int jerk)
        {
            return Send(LMC_Frame.LMCGroupStop(GroupReference, deceleration, jerk));
        }

        private uint ReadGroupStatusValue(out LMC_Response response)
        {
            var result = GroupReadStatusResult();
            response = result.Response;

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(
                    "GroupReadStatus failed. Status="
                    + response.Status
                    + ", ErrorId="
                    + response.ErrorId
                    + ".");
            }

            return result.State;
        }

        private LMC_Response SendMoveLinearAbsolute(
            int[] position,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk)
        {
            return Send(
                LMC_Frame.LMCGroupMoveLinearAbsolute(
                    GroupReference,
                    position,
                    velocity,
                    acceleration,
                    deceleration,
                    jerk));
        }

        private LMC_Response Send(byte[] request)
        {
            EnsureCurrentSessionForUse();
            return LMCConnection.ParseCommandAcknowledgement(
                connection.Exchange(request, sessionGeneration),
                "Group command");
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
            return LMCConnection.ParseCommandAcknowledgement(raw, "Group command");
        }

        private LMC_Response SendShortAcknowledgement(byte[] request)
        {
            EnsureCurrentSessionForUse();
            return LMCConnection.ParseShortAcknowledgement(
                connection.Exchange(request, sessionGeneration),
                "SetKinTransformCartesian4Axis");
        }

        private async Task<LMC_Response> SendShortAcknowledgementAsync(
            byte[] request,
            CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            var raw = await connection.ExchangeAsync(
                request,
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            return LMCConnection.ParseShortAcknowledgement(
                raw,
                "SetKinTransformCartesian4Axis");
        }

        private void EnsureCurrentSessionForUse()
        {
            connection.EnsureSessionGeneration(sessionGeneration);
        }

        private void ValidateKinematicAxes(
            LMCSingleAxis axisX,
            LMCSingleAxis axisY,
            LMCSingleAxis axisZ,
            LMCSingleAxis axisU)
        {
            var axes = new[] { axisX, axisY, axisZ, axisU };
            var usedReferences = new System.Collections.Generic.HashSet<ushort>();

            for (var index = 0; index < axes.Length; index++)
            {
                var axis = axes[index];
                if (axis == null)
                {
                    throw new ArgumentNullException("axis" + index);
                }

                if (!ReferenceEquals(connection, axis.Connection))
                {
                    throw new ArgumentException(
                        "All kinematic axes must belong to the same LMCConnection as the group.");
                }

                axis.EnsureCurrentSessionForUse();

                if (!usedReferences.Add(axis.AxisReference))
                {
                    throw new ArgumentException(
                        "Each kinematic axis reference must be unique.");
                }
            }
        }
    }

    public sealed class LMCGroup : LMCGroupAxis
    {
        public LMCGroup(LMCConnection connection, string groupName)
            : base(connection, groupName)
        {
        }
    }
}
