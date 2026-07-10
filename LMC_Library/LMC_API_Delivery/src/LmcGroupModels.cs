using System;
using System.Collections.Generic;

namespace LasalMotionControlLib
{
    public enum LMC_COORD_SYSTEM : int
    {
        None = 0,
        Acs = 1,
        Mcs = 2,
        Pcs = 3
    }

    public enum LMC_BUFFER_MODE : int
    {
        Aborting = 1,
        Buffered = 2,
        BlendingLow = 3,
        BlendingPrevious = 4,
        BlendingNext = 5,
        BlendingHigh = 6
    }

    public enum LMC_GROUP_TRANSITION_MODE : int
    {
        ExactStop = 0,
        ContinuousDirect = 2,
        SmoothParabolic = 3,
        SmoothCubic = 4,
        SmoothQuintic = 5
    }

    internal enum LMC_KINEMATIC_AXIS_TYPE : int
    {
        X = 0,
        Y = 1,
        Z = 2,
        U = 3,
        V = 4,
        W = 5,
        N1 = 6,
        N2 = 7,
        N3 = 8,
        N4 = 9,
        N5 = 10,
        N6 = 11,
        N7 = 12,
        N8 = 13,
        N9 = 14,
        S = 15
    }

    internal enum LMC_KINEMATIC_TRANSFORM_FUNCTION : int
    {
        None = 0,
        Shift = 1
    }

    public sealed class LMCGroupMotionOptions
    {
        public LMCGroupMotionOptions()
        {
            CoordinateSystem = LMC_COORD_SYSTEM.None;
            TransitionMode = LMC_GROUP_TRANSITION_MODE.ExactStop;
            BufferMode = LMC_BUFFER_MODE.Aborting;
            Execute = true;
        }

        public LMC_COORD_SYSTEM CoordinateSystem { get; set; }
        public LMC_GROUP_TRANSITION_MODE TransitionMode { get; set; }
        public LMC_BUFFER_MODE BufferMode { get; set; }
        public bool Execute { get; set; }
    }

    internal sealed class LMCKinematicNode
    {
        internal LMCKinematicNode(
            ushort nodeReference,
            LMC_KINEMATIC_AXIS_TYPE axisType,
            LMC_KINEMATIC_TRANSFORM_FUNCTION transformFunction,
            double backwardRatio,
            double forwardRatio,
            double backwardShift)
        {
            ValidateFinite(backwardRatio, "backwardRatio");
            ValidateFinite(forwardRatio, "forwardRatio");
            ValidateFinite(backwardShift, "backwardShift");

            if (!Enum.IsDefined(typeof(LMC_KINEMATIC_AXIS_TYPE), axisType))
            {
                throw new ArgumentOutOfRangeException("axisType");
            }

            if (!Enum.IsDefined(
                typeof(LMC_KINEMATIC_TRANSFORM_FUNCTION),
                transformFunction))
            {
                throw new ArgumentOutOfRangeException("transformFunction");
            }

            if (transformFunction == LMC_KINEMATIC_TRANSFORM_FUNCTION.Shift
                && Math.Abs(backwardRatio * forwardRatio - 1.0) > 1e-9)
            {
                throw new ArgumentException(
                    "A shift transform requires backwardRatio * forwardRatio == 1.");
            }

            NodeReference = nodeReference;
            AxisType = axisType;
            TransformFunction = transformFunction;
            BackwardRatio = backwardRatio;
            ForwardRatio = forwardRatio;
            BackwardShift = backwardShift;
        }

        internal ushort NodeReference { get; private set; }
        internal LMC_KINEMATIC_AXIS_TYPE AxisType { get; private set; }
        internal LMC_KINEMATIC_TRANSFORM_FUNCTION TransformFunction { get; private set; }
        internal double BackwardRatio { get; private set; }
        internal double ForwardRatio { get; private set; }
        internal double BackwardShift { get; private set; }

        internal static LMCKinematicNode CreateIdentityShift(
            ushort nodeReference,
            LMC_KINEMATIC_AXIS_TYPE axisType)
        {
            return new LMCKinematicNode(
                nodeReference,
                axisType,
                LMC_KINEMATIC_TRANSFORM_FUNCTION.Shift,
                1.0,
                1.0,
                0.0);
        }

        private static void ValidateFinite(double value, string parameterName)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "Kinematic coefficients must be finite values.");
            }
        }
    }

    internal sealed class LMCCartesianKinematicTransform
    {
        private const int MaximumNodeCount = 16;
        private readonly LMCKinematicNode[] nodes;

        internal LMCCartesianKinematicTransform(
            IEnumerable<LMCKinematicNode> nodes,
            LMC_BUFFER_MODE bufferMode = LMC_BUFFER_MODE.Buffered)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException("nodes");
            }

            var nodeList = new List<LMCKinematicNode>(nodes);

            if (nodeList.Count < 1 || nodeList.Count > MaximumNodeCount)
            {
                throw new ArgumentOutOfRangeException(
                    "nodes",
                    "A Cartesian kinematic transform requires 1 to 16 nodes.");
            }

            if (!Enum.IsDefined(typeof(LMC_BUFFER_MODE), bufferMode))
            {
                throw new ArgumentOutOfRangeException("bufferMode");
            }

            var usedAxisTypes = new HashSet<LMC_KINEMATIC_AXIS_TYPE>();
            for (var index = 0; index < nodeList.Count; index++)
            {
                var node = nodeList[index];
                if (node == null)
                {
                    throw new ArgumentException(
                        "Kinematic nodes must not contain null entries.",
                        "nodes");
                }

                if (!usedAxisTypes.Add(node.AxisType))
                {
                    throw new ArgumentException(
                        "Each kinematic axis type may be used only once.",
                        "nodes");
                }
            }

            this.nodes = nodeList.ToArray();
            BufferMode = bufferMode;
        }

        internal LMCKinematicNode[] Nodes
        {
            get { return (LMCKinematicNode[])nodes.Clone(); }
        }

        internal int NodeCount
        {
            get { return nodes.Length; }
        }

        internal LMC_BUFFER_MODE BufferMode { get; private set; }

        internal static LMCCartesianKinematicTransform CreateFourAxis(
            ushort xReference,
            ushort yReference,
            ushort zReference,
            ushort uReference)
        {
            return new LMCCartesianKinematicTransform(
                new[]
                {
                    LMCKinematicNode.CreateIdentityShift(
                        xReference,
                        LMC_KINEMATIC_AXIS_TYPE.X),
                    LMCKinematicNode.CreateIdentityShift(
                        yReference,
                        LMC_KINEMATIC_AXIS_TYPE.Y),
                    LMCKinematicNode.CreateIdentityShift(
                        zReference,
                        LMC_KINEMATIC_AXIS_TYPE.Z),
                    LMCKinematicNode.CreateIdentityShift(
                        uReference,
                        LMC_KINEMATIC_AXIS_TYPE.U)
                });
        }
    }
}
