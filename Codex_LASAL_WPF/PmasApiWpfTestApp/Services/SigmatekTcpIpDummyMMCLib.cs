using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

// Dummy replacement for Elmo MMCLibDotNET.
// This keeps the same public type names/signatures used by the app,
// but executes a local simulation intended for SIGMATEK TCP/IP migration scaffolding.
namespace ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET
{
    public enum LibraryErrors
    {
        NoError = 0,
        InvalidHandle = -4,
        InternalError = -100
    }

    public enum MMCErrors
    {
        NoError = 0,
        NC_NODE_NOT_FOUND = 1,
        NC_UNSUITABLE_NODE_STATE = 2,
        NC_DIRECTION_TYPE_OUT_OF_RANGE = 3
    }

    public enum MC_BUFFERED_MODE_ENUM
    {
        MC_ABORTING = 0,
        MC_BUFFERED = 1,
        MC_BLENDING_PREVIOUS = 2
    }

    public enum MC_DIRECTION_ENUM
    {
        MC_SHORTEST_WAY = 0,
        MC_POSITIVE_DIRECTION = 1,
        MC_NEGATIVE_DIRECTION = 2,
        MC_CURRENT_DIRECTION = 3,
        MC_NONE_DIRECTION = 4
    }

    public enum OPM402
    {
        OPM402_NONE = 0,
        OPM402_CSP = 8,
        OPM402_CSV = 9,
        OPM402_CST = 10,
        OPM402_HOMING = 6
    }

    public enum MC_EXECUTION_MODE
    {
        MC_IMMEDIATELY = 0,
        MC_QUEUED = 1
    }

    public enum MMC_PARAMETER_LIST_ENUM
    {
        PARAM_0 = 0,
        PARAM_1 = 1,
        PARAM_2 = 2
    }

    public enum MMC_BOOLEAN_PARAMETER_LIST_ENUM
    {
        BOOL_PARAM_0 = 0,
        BOOL_PARAM_1 = 1
    }

    public enum PIVarDirection
    {
        INPUT = 0,
        OUTPUT = 1,
        INOUT = 2
    }

    public enum NC_BULKREAD_CONFIG_ENUM
    {
        DEFAULT = 0,
        FAST = 1
    }

    public enum NC_BULKREAD_PRESET_ENUM
    {
        PRESET_0 = 0,
        PRESET_1 = 1
    }

    public enum NC_TRANSITION_MODE_ENUM
    {
        NONE = 0,
        CORNER = 1
    }

    public enum MC_COORD_SYSTEM_ENUM
    {
        MCS = 0,
        ACS = 1
    }

    public enum MC_CONDITIONFB_OPERATION_TYPE
    {
        EQUAL = 0,
        GREATER = 1,
        LESS = 2
    }

    public enum VAR_TYPE
    {
        BYTE = 0,
        SHORT = 1,
        USHORT = 2,
        INT = 3,
        UINT = 4,
        FLOAT = 5
    }

    public enum NC_AXIS_IN_GROUP_TYPE_ENUM_EX
    {
        NC_PROFILER_X_AXIS_TYPE = 0,
        NC_PROFILER_Y_AXIS_TYPE = 1,
        NC_PROFILER_Z_AXIS_TYPE = 2
    }

    public enum NC_TR_FUNC_ID_ENUM
    {
        NC_TR_SHIFT_FUNC = 0
    }

    public delegate void cbFunc(object sender, MMC_CAN_REPLY_DATA_OUT data);
    public delegate void MotionEndEvent(ushort axisRef, bool result);
    public delegate void HomingEndEvent(ushort axisRef, short errId);
    public delegate void ErrorStateEventCallback(ushort axisRef, short state, ushort emergencyCode);

    public struct MMC_CAN_REPLY_DATA_OUT
    {
        public byte btEventType;
        public ushort usAxisRef;
        public ushort usFunctionID;
        public ushort usErrorID;
        public ushort usStatus;
    }

    public struct PI_VAR_UNION
    {
        public byte _byte;
        public short _int16;
        public ushort _uint16;
        public int _int32;
        public uint _uint32;
        public float _float;
    }

    public struct NC_PI_INFO_BY_ALIAS
    {
        public string Alias;
        public ushort Index;
        public PIVarDirection Direction;
    }

    public struct UploadRecorderHeaderParam
    {
        public uint Gap;
        public uint DataLength;
        public uint SignalCount;
    }

    public sealed class MMCConnectionObject
    {
        public int Handle { get; internal set; }
        public bool IsUDPChannelOpened { get; internal set; }
        public int CbUdpPort { get; internal set; }
        public string RemoteIp { get; internal set; }
        public string LocalIp { get; internal set; }
    }

    public sealed class MMCException : Exception
    {
        public MMCException(
            string message,
            ushort commandId,
            LibraryErrors libraryError,
            MMCErrors mmcError,
            ushort status,
            ushort axisRef,
            string axisName)
            : base(message)
        {
            CommandID = commandId;
            LibraryError = libraryError;
            MMCError = mmcError;
            Status = status;
            AxisRef = axisRef;
            AxisName = axisName;
        }

        public ushort CommandID { get; private set; }
        public LibraryErrors LibraryError { get; private set; }
        public MMCErrors MMCError { get; private set; }
        public ushort Status { get; private set; }
        public ushort AxisRef { get; private set; }
        public string AxisName { get; private set; }
    }

    public sealed class MC_KIN_NODE
    {
        public NC_AXIS_IN_GROUP_TYPE_ENUM_EX eType;
        public ushort hNode;
        public NC_TR_FUNC_ID_ENUM iMcsToAcsFuncID;
        public double[] ulTrCoef;

        public MC_KIN_NODE()
        {
            ulTrCoef = new double[3];
        }
    }

    public sealed class MC_KIN_REF_CARTESIAN
    {
        public int iNumAxes;
        public MC_KIN_NODE[] sNode;

        public MC_KIN_REF_CARTESIAN()
        {
            sNode = new MC_KIN_NODE[16];
            for (var i = 0; i < sNode.Length; i++)
            {
                sNode[i] = new MC_KIN_NODE();
            }
        }
    }

    public sealed class MMCGroupMemberInfo
    {
        public string Name { get; set; }
        public ushort AxisRef { get; set; }
    }

    internal sealed class DummyAxisState
    {
        private readonly object _sync = new object();
        private readonly Dictionary<string, object> _sdo = new Dictionary<string, object>(StringComparer.Ordinal);
        private readonly Dictionary<string, double> _parameters = new Dictionary<string, double>(StringComparer.Ordinal);
        private double _currentPosition;
        private double _moveStartPosition;
        private double _moveTargetPosition;
        private DateTime _moveStartUtc;
        private double _moveDurationMs;
        private bool _moving;

        public DummyAxisState(string name, int handle)
        {
            Name = name;
            Handle = handle;
            AxisReference = (ushort)(Math.Abs((name + "_" + handle.ToString(CultureInfo.InvariantCulture)).GetHashCode()) % 60000 + 1);
            DriveId = AxisReference;
            Powered = true;
            CurrentOpMode = OPM402.OPM402_CSP;
        }

        public string Name { get; private set; }
        public int Handle { get; private set; }
        public ushort AxisReference { get; private set; }
        public ushort DriveId { get; private set; }
        public bool Powered { get; set; }
        public OPM402 CurrentOpMode { get; set; }

        public double GetActualPosition()
        {
            lock (_sync)
            {
                return GetActualPositionLocked(DateTime.UtcNow);
            }
        }

        public void Stop()
        {
            lock (_sync)
            {
                _currentPosition = GetActualPositionLocked(DateTime.UtcNow);
                _moving = false;
                _moveDurationMs = 0.0;
            }
        }

        public void IssueAbsoluteMove(double targetPosition, double velocity)
        {
            lock (_sync)
            {
                if (!Powered)
                {
                    throw new MMCException(
                        "Axis is not powered.",
                        0x209F,
                        LibraryErrors.NoError,
                        MMCErrors.NC_UNSUITABLE_NODE_STATE,
                        16,
                        AxisReference,
                        Name);
                }

                var now = DateTime.UtcNow;
                _currentPosition = GetActualPositionLocked(now);
                _moveStartPosition = _currentPosition;
                _moveTargetPosition = targetPosition;
                var speed = Math.Max(Math.Abs(velocity), 1.0);
                var distance = Math.Abs(_moveTargetPosition - _moveStartPosition);
                _moveDurationMs = Math.Max(5.0, distance / speed * 1000.0);
                _moveStartUtc = now;
                _moving = true;
            }
        }

        public void IssueRelativeMove(double distance, double velocity)
        {
            var actual = GetActualPosition();
            IssueAbsoluteMove(actual + distance, velocity);
        }

        public void SetParameter(MMC_PARAMETER_LIST_ENUM parameter, int index, double value)
        {
            lock (_sync)
            {
                _parameters[BuildParamKey(parameter, index)] = value;
            }
        }

        public double GetParameter(MMC_PARAMETER_LIST_ENUM parameter, int index)
        {
            lock (_sync)
            {
                double value;
                if (_parameters.TryGetValue(BuildParamKey(parameter, index), out value))
                {
                    return value;
                }

                return 0.0;
            }
        }

        public void SetSdo(ushort objIndex, byte objSubIndex, object value)
        {
            lock (_sync)
            {
                _sdo[BuildSdoKey(objIndex, objSubIndex)] = value;
            }
        }

        public T GetSdo<T>(ushort objIndex, byte objSubIndex)
        {
            lock (_sync)
            {
                object value;
                if (_sdo.TryGetValue(BuildSdoKey(objIndex, objSubIndex), out value))
                {
                    if (value is T)
                    {
                        return (T)value;
                    }

                    return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
                }

                return default(T);
            }
        }

        private static string BuildSdoKey(ushort objIndex, byte objSubIndex)
        {
            return objIndex.ToString("X4", CultureInfo.InvariantCulture) + ":" + objSubIndex.ToString("X2", CultureInfo.InvariantCulture);
        }

        private static string BuildParamKey(MMC_PARAMETER_LIST_ENUM parameter, int index)
        {
            return parameter.ToString() + ":" + index.ToString(CultureInfo.InvariantCulture);
        }

        private double GetActualPositionLocked(DateTime nowUtc)
        {
            if (!_moving)
            {
                return _currentPosition;
            }

            var elapsedMs = (nowUtc - _moveStartUtc).TotalMilliseconds;
            if (elapsedMs >= _moveDurationMs)
            {
                _currentPosition = _moveTargetPosition;
                _moving = false;
                return _currentPosition;
            }

            var ratio = _moveDurationMs <= 0.0 ? 1.0 : elapsedMs / _moveDurationMs;
            ratio = Math.Max(0.0, Math.Min(1.0, ratio));
            _currentPosition = _moveStartPosition + ((_moveTargetPosition - _moveStartPosition) * ratio);
            return _currentPosition;
        }
    }

    internal sealed class DummyConnectionState
    {
        public int Handle { get; set; }
        public MMCConnectionObject ConnectionObject { get; set; }
        public TcpClient RpcClient { get; set; }
        public readonly object IoSync = new object();
        public cbFunc UserCallback { get; set; }
        public MotionEndEvent EndMotionCallback { get; set; }
        public HomingEndEvent EndHomingCallback { get; set; }
        public ErrorStateEventCallback ErrorStateCallback { get; set; }
        public readonly Dictionary<string, DummyAxisState> AxisByName = new Dictionary<string, DummyAxisState>(StringComparer.OrdinalIgnoreCase);
        public readonly Random Random = new Random();
        public uint RecorderGap { get; set; }
        public uint RecorderDataLength { get; set; }
        public uint[] RecorderParams { get; set; }
        public uint[] RecorderSignalIds { get; set; }
        public bool RecorderRunning { get; set; }
    }

    internal static class DummyBackend
    {
        private static readonly object Sync = new object();
        private static int _nextHandle = 1;
        private static readonly Dictionary<int, DummyConnectionState> Connections = new Dictionary<int, DummyConnectionState>();

        public static DummyConnectionState Connect(
            IPAddress remoteAddress,
            int remotePort,
            IPAddress localAddress,
            int localPort,
            cbFunc callback)
        {
            if (remoteAddress == null)
            {
                throw new ArgumentNullException(nameof(remoteAddress));
            }

            if (remotePort <= 0 || remotePort > 65535)
            {
                throw new ArgumentOutOfRangeException(nameof(remotePort));
            }

            var bindAddress = localAddress;
            if (bindAddress == null || IPAddress.Any.Equals(bindAddress))
            {
                bindAddress = IPAddress.Any;
            }

            if (localPort < 0 || localPort > 65535)
            {
                throw new ArgumentOutOfRangeException(nameof(localPort));
            }

            var rpcClient = new TcpClient(AddressFamily.InterNetwork);
            try
            {
                rpcClient.Client.NoDelay = true;
                rpcClient.Client.Bind(new IPEndPoint(bindAddress, localPort));
                rpcClient.Connect(remoteAddress, remotePort);

                lock (Sync)
                {
                    var handle = _nextHandle++;
                    var connection = new DummyConnectionState
                    {
                        Handle = handle,
                        UserCallback = callback,
                        RpcClient = rpcClient,
                        ConnectionObject = new MMCConnectionObject
                        {
                            Handle = handle,
                            IsUDPChannelOpened = true,
                            CbUdpPort = localPort > 0 ? localPort : 5001,
                            RemoteIp = remoteAddress.ToString(),
                            LocalIp = bindAddress.ToString()
                        }
                    };

                    Connections[handle] = connection;
                    return connection;
                }
            }
            catch
            {
                rpcClient.Close();
                throw;
            }
        }

        public static bool TryGetConnection(int handle, out DummyConnectionState state)
        {
            lock (Sync)
            {
                return Connections.TryGetValue(handle, out state);
            }
        }

        public static DummyConnectionState GetConnectionOrThrow(int handle)
        {
            DummyConnectionState state;
            if (!TryGetConnection(handle, out state))
            {
                throw new MMCException(
                    "Invalid connection handle.",
                    0,
                    LibraryErrors.InvalidHandle,
                    MMCErrors.NC_NODE_NOT_FOUND,
                    0,
                    0,
                    string.Empty);
            }

            return state;
        }

        public static void Disconnect(int handle)
        {
            DummyConnectionState state = null;
            lock (Sync)
            {
                if (Connections.TryGetValue(handle, out state))
                {
                    Connections.Remove(handle);
                }
            }

            if (state != null && state.RpcClient != null)
            {
                try
                {
                    state.RpcClient.Close();
                }
                catch
                {
                    // Ignore cleanup exceptions during disconnect.
                }

                state.RpcClient = null;
            }
        }

        public static DummyAxisState GetOrCreateAxis(int handle, string axisName)
        {
            var connection = GetConnectionOrThrow(handle);
            var name = string.IsNullOrWhiteSpace(axisName) ? "Axis1" : axisName.Trim();

            DummyAxisState axis;
            if (!connection.AxisByName.TryGetValue(name, out axis))
            {
                axis = new DummyAxisState(name, handle);
                connection.AxisByName[name] = axis;
            }

            return axis;
        }
    }

    public static class MMCConnection
    {
        public static int ConnectRPC(
            IPAddress destination,
            int remotePort,
            IPAddress localIp,
            int localPort,
            cbFunc callback,
            uint eventMask,
            out int handle)
        {
            handle = 0;
            try
            {
                var state = DummyBackend.Connect(destination, remotePort, localIp, localPort, callback);
                handle = state.Handle;
                return (int)LibraryErrors.NoError;
            }
            catch
            {
                return (int)LibraryErrors.InternalError;
            }
        }

        public static void RegisterEndMotionEventCallback(int handle, MotionEndEvent callback)
        {
            var state = DummyBackend.GetConnectionOrThrow(handle);
            state.EndMotionCallback = callback;
        }

        public static void RegisterEndHomingEventCallback(int handle, HomingEndEvent callback)
        {
            var state = DummyBackend.GetConnectionOrThrow(handle);
            state.EndHomingCallback = callback;
        }

        public static void RegisterErrorStateCallback(int handle, ErrorStateEventCallback callback)
        {
            var state = DummyBackend.GetConnectionOrThrow(handle);
            state.ErrorStateCallback = callback;
        }

        public static int CloseConnection(MMCConnectionObject connection)
        {
            if (connection != null)
            {
                DummyBackend.Disconnect(connection.Handle);
            }

            return (int)LibraryErrors.NoError;
        }

        public static MMCConnectionObject GetConnection(int handle)
        {
            return DummyBackend.GetConnectionOrThrow(handle).ConnectionObject;
        }

        public static int GetUDPListenerPortNumber(int handle)
        {
            return DummyBackend.GetConnectionOrThrow(handle).ConnectionObject.CbUdpPort;
        }

        public static void GetErrorCodeDescriptionByID(int handle, int errorCode, byte errorType, out string resolution, out string description)
        {
            DummyBackend.GetConnectionOrThrow(handle);

            if (errorCode == 0)
            {
                description = "No error.";
                resolution = "No action required.";
                return;
            }

            description = "Dummy SIGMATEK TCP/IP backend error code " + errorCode.ToString(CultureInfo.InvariantCulture);
            resolution = "Check simulated connection state and requested command sequence.";
        }

        public static void BeginRecording(
            int handle,
            uint gap,
            uint dataLength,
            uint signalBitMask,
            uint[] recorderParams,
            uint[] signalIds)
        {
            var state = DummyBackend.GetConnectionOrThrow(handle);
            state.RecorderGap = gap;
            state.RecorderDataLength = dataLength;
            state.RecorderParams = recorderParams ?? new uint[0];
            state.RecorderSignalIds = signalIds ?? new uint[0];
            state.RecorderRunning = true;
        }

        public static void GetRecordingStatus(int handle, out uint recordingIndex, out uint triggerStatus)
        {
            var state = DummyBackend.GetConnectionOrThrow(handle);
            recordingIndex = state.RecorderRunning ? 1u : 0u;
            triggerStatus = state.RecorderRunning ? 1u : 0u;
        }

        public static void StopRecording(int handle)
        {
            var state = DummyBackend.GetConnectionOrThrow(handle);
            state.RecorderRunning = false;
        }

        public static void GetRecordingDataHeader(int handle, out UploadRecorderHeaderParam header)
        {
            var state = DummyBackend.GetConnectionOrThrow(handle);
            header = new UploadRecorderHeaderParam
            {
                Gap = state.RecorderGap,
                DataLength = state.RecorderDataLength,
                SignalCount = (uint)(state.RecorderSignalIds == null ? 0 : state.RecorderSignalIds.Length)
            };
        }

        public static void GetRecordingData(int handle, uint from, uint to, uint bufferIndex, out int[] data)
        {
            var state = DummyBackend.GetConnectionOrThrow(handle);
            if (to < from)
            {
                data = new int[0];
                return;
            }

            var length = checked((int)(to - from + 1));
            var seed = state.Random.Next(1, 100);
            data = new int[length];
            for (var i = 0; i < length; i++)
            {
                data[i] = seed + i;
            }
        }
    }

    public sealed class MMCSingleAxis
    {
        // Temporary transport-only mode:
        // Send commands with AxisRef=0 even when no real axis reference is available.
        private static readonly bool ForceZeroAxisRefForCommands = true;
        private const ushort PowerOnCommandId = 0x2081;
        private const ushort PowerOffCommandId = 0x2082;
        private const ushort ResetCommandId = 0x2083;
        private const ushort StopCommandId = 0x2084;
        private const ushort GetActualPositionCommandId = 0x202E;
        private const ushort MoveAbsoluteExCommandId = 0x209F;
        private readonly DummyAxisState _state;
        private readonly byte[] _getActualPositionRequestFrame;
        private readonly byte[] _getActualPositionResponseBuffer = new byte[64];
        private double _lastReadActualPositionAuxiliary;

        public MMCSingleAxis(string axisName, int handle)
        {
            _state = DummyBackend.GetOrCreateAxis(handle, axisName);
            AxisName = _state.Name;
            AxisReference = _state.AxisReference;
            DriveID = _state.DriveId;
            _getActualPositionRequestFrame = BuildGetActualPositionRequestFrame(AxisReference);
        }

        public string AxisName { get; private set; }
        public ushort AxisReference { get; private set; }
        public ushort DriveID { get; private set; }

        public void PowerOn(MC_BUFFERED_MODE_ENUM bufferedMode)
        {
            var frame = BuildPowerFrame(PowerOnCommandId, AxisReference, true, bufferedMode);
            SendFrame(PowerOnCommandId, frame, "PowerOn");
            _state.Powered = true;
        }

        public void PowerOff(MC_BUFFERED_MODE_ENUM bufferedMode)
        {
            var frame = BuildPowerFrame(PowerOffCommandId, AxisReference, false, bufferedMode);
            SendFrame(PowerOffCommandId, frame, "PowerOff");
            _state.Powered = false;
            _state.Stop();
        }

        public void Reset()
        {
            var frame = BuildResetFrame(AxisReference);
            SendFrame(ResetCommandId, frame, "Reset");
            _state.Powered = true;
        }

        public void Stop(MC_BUFFERED_MODE_ENUM bufferedMode)
        {
            var frame = BuildStopFrame(AxisReference, bufferedMode);
            SendFrame(StopCommandId, frame, "Stop");
            _state.Stop();
        }

        public double GetParameter(MMC_PARAMETER_LIST_ENUM parameter, int index)
        {
            return _state.GetParameter(parameter, index);
        }

        public void SetParameter(double value, MMC_PARAMETER_LIST_ENUM parameter, int index)
        {
            _state.SetParameter(parameter, index, value);
        }

        public bool GetBoolParameter(MMC_BOOLEAN_PARAMETER_LIST_ENUM parameter, int index)
        {
            return false;
        }

        public void SetOpMode(OPM402 opMode, MC_EXECUTION_MODE executionMode, double initialValue)
        {
            _state.CurrentOpMode = opMode;
        }

        public double GetActualPosition()
        {
            var responseLength = SendFrameAndTryRead(
                GetActualPositionCommandId,
                _getActualPositionRequestFrame,
                "GetActualPosition",
                _getActualPositionResponseBuffer,
                30);

            double positionFromResponse;
            double auxiliaryFromResponse;
            if (TryParseGetActualPositionResponse(
                _getActualPositionResponseBuffer,
                responseLength,
                out positionFromResponse,
                out auxiliaryFromResponse))
            {
                _lastReadActualPositionAuxiliary = auxiliaryFromResponse;
                return positionFromResponse;
            }

            return _state.GetActualPosition();
        }

        public uint ReadStatus()
        {
            return _state.Powered ? 0x0007u : 0x0000u;
        }

        public void GetStatusRegister(ref uint statusRegister, ref uint mcsLimitRegister, ref byte endMotionReason)
        {
            statusRegister = ReadStatus();
            mcsLimitRegister = 0;
            endMotionReason = 0;
        }

        public int MoveAbsoluteEx(
            double position,
            double velocity,
            double acceleration,
            double deceleration,
            double jerk,
            MC_DIRECTION_ENUM direction,
            MC_BUFFERED_MODE_ENUM bufferMode)
        {
            var positionLong = ToInt64Parameter(position, nameof(position));
            var velocityLong = ToInt64Parameter(velocity, nameof(velocity));
            var accelerationLong = ToInt64Parameter(acceleration, nameof(acceleration));
            var decelerationLong = ToInt64Parameter(deceleration, nameof(deceleration));
            var jerkLong = ToInt64Parameter(jerk, nameof(jerk));

            return MoveAbsolute(positionLong, velocityLong, accelerationLong, decelerationLong, jerkLong, direction, bufferMode);
        }

        public int MoveAbsolute(
            long position,
            long velocity,
            long acceleration,
            long deceleration,
            long jerk,
            MC_DIRECTION_ENUM direction,
            MC_BUFFERED_MODE_ENUM bufferMode)
        {
            var frame = BuildMoveAbsoluteFrame(
                AxisReference,
                position,
                velocity,
                acceleration,
                deceleration,
                jerk,
                direction,
                bufferMode);

            SendFrame(MoveAbsoluteExCommandId, frame, "MoveAbsolute");

            var speed = ToSimulationSpeed(velocity);
            _state.IssueAbsoluteMove(position, speed);
            return 0;
        }

        public void MoveRelativeEx(
            double distance,
            double velocity,
            double acceleration,
            double deceleration,
            double jerk,
            MC_DIRECTION_ENUM direction,
            MC_BUFFERED_MODE_ENUM bufferMode)
        {
            _state.IssueRelativeMove(distance, velocity);
        }

        public void MoveVelocityEx(
            double velocity,
            double acceleration,
            double deceleration,
            double jerk,
            MC_DIRECTION_ENUM direction,
            MC_BUFFERED_MODE_ENUM bufferMode)
        {
            var sign = direction == MC_DIRECTION_ENUM.MC_NEGATIVE_DIRECTION ? -1.0 : 1.0;
            _state.IssueRelativeMove(sign * Math.Max(1.0, Math.Abs(velocity)) * 0.05, Math.Max(1.0, Math.Abs(velocity)));
        }

        public void SetOverride(float acceleration, float jerk, float velocity, ushort index)
        {
        }

        public void UploadSDO(ushort objectIndex, byte objectSubIndex, out byte value, int timeout)
        {
            value = _state.GetSdo<byte>(objectIndex, objectSubIndex);
        }

        public void UploadSDO(ushort objectIndex, byte objectSubIndex, out short value, int timeout)
        {
            value = _state.GetSdo<short>(objectIndex, objectSubIndex);
        }

        public void UploadSDO(ushort objectIndex, byte objectSubIndex, out ushort value, int timeout)
        {
            value = _state.GetSdo<ushort>(objectIndex, objectSubIndex);
        }

        public void UploadSDO(ushort objectIndex, byte objectSubIndex, out int value, int timeout)
        {
            value = _state.GetSdo<int>(objectIndex, objectSubIndex);
        }

        public void UploadSDO(ushort objectIndex, byte objectSubIndex, out uint value, int timeout)
        {
            value = _state.GetSdo<uint>(objectIndex, objectSubIndex);
        }

        public void UploadSDO(ushort objectIndex, byte objectSubIndex, out float value, int timeout)
        {
            value = _state.GetSdo<float>(objectIndex, objectSubIndex);
        }

        public void DownloadSDO(ushort objectIndex, byte objectSubIndex, byte value, int timeout)
        {
            _state.SetSdo(objectIndex, objectSubIndex, value);
        }

        public void DownloadSDO(ushort objectIndex, byte objectSubIndex, short value, int timeout)
        {
            _state.SetSdo(objectIndex, objectSubIndex, value);
        }

        public void DownloadSDO(ushort objectIndex, byte objectSubIndex, ushort value, int timeout)
        {
            _state.SetSdo(objectIndex, objectSubIndex, value);
        }

        public void DownloadSDO(ushort objectIndex, byte objectSubIndex, int value, int timeout)
        {
            _state.SetSdo(objectIndex, objectSubIndex, value);
        }

        public void DownloadSDO(ushort objectIndex, byte objectSubIndex, uint value, int timeout)
        {
            _state.SetSdo(objectIndex, objectSubIndex, value);
        }

        public void DownloadSDO(ushort objectIndex, byte objectSubIndex, float value, int timeout)
        {
            _state.SetSdo(objectIndex, objectSubIndex, value);
        }

        public void HomeDS402Ex(
            double homePosition,
            double homeDetectionVelocityLimit,
            float homeAccel,
            float homeVelocityHigh,
            float homeVelocityLow,
            float homeDistanceLimit,
            float homeTorqueLimit,
            MC_BUFFERED_MODE_ENUM bufferedMode,
            int homeMethod,
            uint homeTimeLimit,
            uint homeDetectionTimeLimit,
            byte execute,
            byte[] reserved)
        {
            _state.IssueAbsoluteMove(homePosition, Math.Max(1.0, Math.Abs(homeVelocityHigh)));
        }

        public void GetPIVarInfoByAlias(string alias, out NC_PI_INFO_BY_ALIAS info)
        {
            info = new NC_PI_INFO_BY_ALIAS
            {
                Alias = alias,
                Index = 1,
                Direction = PIVarDirection.INOUT
            };
        }

        public void ReadPIVar(ushort index, PIVarDirection direction, VAR_TYPE varType, ref PI_VAR_UNION value)
        {
            value._uint16 = index;
        }

        public void WritePIVar(ushort index, PI_VAR_UNION value, VAR_TYPE varType)
        {
        }

        public int GetAxisError(ref ushort emergencyCode)
        {
            emergencyCode = 0;
            return 0;
        }

        private void SendFrame(ushort commandId, byte[] frame, string operationName)
        {
            var connection = GetConnectionOrThrow(commandId);
            var stream = connection.RpcClient.GetStream();

            try
            {
                lock (connection.IoSync)
                {
                    stream.Write(frame, 0, frame.Length);
                }
            }
            catch (Exception ex)
            {
                throw new MMCException(
                    "Failed to send " + operationName + " frame over TCP/IP: " + ex.Message,
                    commandId,
                    LibraryErrors.InternalError,
                    MMCErrors.NC_UNSUITABLE_NODE_STATE,
                    16,
                    AxisReference,
                    AxisName);
            }
        }

        private int SendFrameAndTryRead(
            ushort commandId,
            byte[] frame,
            string operationName,
            byte[] responseBuffer,
            int readTimeoutMs)
        {
            if (responseBuffer == null || responseBuffer.Length == 0)
            {
                return 0;
            }

            var connection = GetConnectionOrThrow(commandId);
            var stream = connection.RpcClient.GetStream();

            lock (connection.IoSync)
            {
                try
                {
                    stream.Write(frame, 0, frame.Length);
                }
                catch (Exception ex)
                {
                    throw new MMCException(
                        "Failed to send " + operationName + " request over TCP/IP: " + ex.Message,
                        commandId,
                        LibraryErrors.InternalError,
                        MMCErrors.NC_UNSUITABLE_NODE_STATE,
                        16,
                        AxisReference,
                        AxisName);
                }

                var previousReadTimeout = stream.ReadTimeout;
                try
                {
                    stream.ReadTimeout = readTimeoutMs;

                    var totalRead = 0;
                    while (totalRead < responseBuffer.Length)
                    {
                        var read = stream.Read(responseBuffer, totalRead, responseBuffer.Length - totalRead);
                        if (read <= 0)
                        {
                            break;
                        }

                        totalRead += read;

                        if (!stream.DataAvailable)
                        {
                            break;
                        }
                    }

                    return totalRead;
                }
                catch (IOException)
                {
                    return 0;
                }
                catch (SocketException)
                {
                    return 0;
                }
                finally
                {
                    try
                    {
                        stream.ReadTimeout = previousReadTimeout;
                    }
                    catch
                    {
                        // Ignore timeout restore failures.
                    }
                }
            }
        }

        private DummyConnectionState GetConnectionOrThrow(ushort commandId)
        {
            var connection = DummyBackend.GetConnectionOrThrow(_state.Handle);
            if (connection.RpcClient == null || !connection.RpcClient.Connected)
            {
                throw new MMCException(
                    "RPC socket is not connected.",
                    commandId,
                    LibraryErrors.InvalidHandle,
                    MMCErrors.NC_NODE_NOT_FOUND,
                    16,
                    AxisReference,
                    AxisName);
            }

            return connection;
        }

        private static byte[] BuildPowerFrame(ushort commandId, ushort axisRef, bool powerOn, MC_BUFFERED_MODE_ENUM bufferedMode)
        {
            // 16-byte fixed frame:
            // [0..1]=Command(PowerOn/PowerOff), [2..3]=AxisRef, [4..7]=PayloadLength(8),
            // [8..11]=Power(1/0), [12..15]=BufferedMode.
            var frame = new byte[16];
            axisRef = ResolveCommandAxisRef(axisRef);
            WriteUInt16LE(frame, 0, commandId);
            WriteUInt16LE(frame, 2, axisRef);
            WriteUInt32LE(frame, 4, 8u);
            WriteInt32LE(frame, 8, powerOn ? 1 : 0);
            WriteInt32LE(frame, 12, (int)bufferedMode);
            return frame;
        }

        private static byte[] BuildResetFrame(ushort axisRef)
        {
            // 12-byte fixed frame:
            // [0..1]=Command(Reset), [2..3]=AxisRef, [4..7]=PayloadLength(4), [8..11]=Execute(1).
            var frame = new byte[12];
            axisRef = ResolveCommandAxisRef(axisRef);
            WriteUInt16LE(frame, 0, ResetCommandId);
            WriteUInt16LE(frame, 2, axisRef);
            WriteUInt32LE(frame, 4, 4u);
            WriteInt32LE(frame, 8, 1);
            return frame;
        }

        private static byte[] BuildStopFrame(ushort axisRef, MC_BUFFERED_MODE_ENUM bufferedMode)
        {
            // 16-byte fixed frame:
            // [0..1]=Command(Stop), [2..3]=AxisRef, [4..7]=PayloadLength(8),
            // [8..11]=BufferedMode, [12..15]=Execute(1).
            var frame = new byte[16];
            axisRef = ResolveCommandAxisRef(axisRef);
            WriteUInt16LE(frame, 0, StopCommandId);
            WriteUInt16LE(frame, 2, axisRef);
            WriteUInt32LE(frame, 4, 8u);
            WriteInt32LE(frame, 8, (int)bufferedMode);
            WriteInt32LE(frame, 12, 1);
            return frame;
        }

        private static byte[] BuildGetActualPositionRequestFrame(ushort axisRef)
        {
            // 9-byte fixed frame observed on wire:
            // [0..1]=Command(0x202E), [2..3]=AxisRef, [4..7]=1, [8]=0.
            var frame = new byte[9];
            axisRef = ResolveCommandAxisRef(axisRef);
            WriteUInt16LE(frame, 0, GetActualPositionCommandId);
            WriteUInt16LE(frame, 2, axisRef);
            WriteUInt32LE(frame, 4, 1u);
            frame[8] = 0;
            return frame;
        }

        private static byte[] BuildMoveAbsoluteFrame(
            ushort axisRef,
            long position,
            long velocity,
            long acceleration,
            long deceleration,
            long jerk,
            MC_DIRECTION_ENUM direction,
            MC_BUFFERED_MODE_ENUM bufferMode)
        {
            // 64-byte fixed frame:
            // [0..1]=Command(0x209F), [2..3]=AxisRef, [4..7]=PayloadLength(56),
            // [8..47]=int64 motion params, [48..63]=int32 enums/flags.
            var frame = new byte[64];
            axisRef = ResolveCommandAxisRef(axisRef);

            WriteUInt16LE(frame, 0, MoveAbsoluteExCommandId);
            WriteUInt16LE(frame, 2, axisRef);
            WriteUInt32LE(frame, 4, 56u);

            WriteInt64LE(frame, 8, position);
            WriteInt64LE(frame, 16, velocity);
            WriteInt64LE(frame, 24, acceleration);
            WriteInt64LE(frame, 32, deceleration);
            WriteInt64LE(frame, 40, jerk);

            WriteInt32LE(frame, 48, (int)direction);
            WriteInt32LE(frame, 52, (int)bufferMode);
            WriteInt32LE(frame, 56, 1); // Execute
            WriteInt32LE(frame, 60, 0); // Reserved

            return frame;
        }

        private static ushort ResolveCommandAxisRef(ushort axisRef)
        {
            if (ForceZeroAxisRefForCommands)
            {
                return 0;
            }

            return axisRef;
        }

        private static bool TryParseGetActualPositionResponse(byte[] responseFrame, int responseLength, out double position, out double auxiliaryValue)
        {
            position = 0.0;
            auxiliaryValue = 0.0;
            if (responseFrame == null || responseLength < 12)
            {
                return false;
            }

            // Common envelope:
            // [0..1]=command/marker, [2..3]=payloadLength, [4..7]=reserved
            // Payload starts at offset 8.
            // We support all observed variants:
            //  - payload=8  : position only
            //  - payload=12 : position + status/error (Elmo API style)
            //  - payload=16 : position + auxiliary value
            ushort payloadLength = BitConverter.ToUInt16(responseFrame, 2);
            int requiredLength = payloadLength + 8;
            if (payloadLength > 0 && responseLength >= requiredLength)
            {
                position = ReadPositionValue(responseFrame, 8, payloadLength);

                if (payloadLength >= 16 && responseLength >= 24)
                {
                    auxiliaryValue = ReadPositionValue(responseFrame, 16, payloadLength - 8);
                }

                return true;
            }

            // Fallback for short/invalid headers.
            if (responseLength >= 16)
            {
                position = ReadPositionValue(responseFrame, 8, responseLength - 8);
                return true;
            }

            if (responseLength >= 12)
            {
                position = ReadPositionValue(responseFrame, 8, 4);
                return true;
            }

            return false;
        }

        private static double ReadPositionValue(byte[] source, int offset, int availableBytes)
        {
            if (availableBytes < 4 || source == null || offset < 0 || offset + 4 > source.Length)
            {
                return 0.0;
            }

            // Prefer int32 first for LASAL response compatibility.
            int asInt32 = ReadInt32LE(source, offset);
            if (availableBytes < 8 || offset + 8 > source.Length)
            {
                return asInt32;
            }

            // If upper 32-bit is sign extension of int32, treat as int32 payload and skip extra parsing.
            var upperDword = ReadUInt32LE(source, offset + 4);
            if (upperDword == 0u || upperDword == 0xFFFFFFFFu)
            {
                return asInt32;
            }

            // Some integrations encode position as int64 counts while others use double.
            // Keep both as fallback when payload provides 8 bytes.
            var asDouble = ReadDoubleLE(source, offset);
            var asInt64 = ReadInt64LE(source, offset);
            if (Math.Abs(asDouble) < 1e-200 && Math.Abs(asInt64) > 1000L)
            {
                return asInt64;
            }

            return asDouble;
        }

        private static long ToInt64Parameter(double value, string parameterName)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be a finite number.");
            }

            if (value > long.MaxValue || value < long.MinValue)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value is out of Int64 range.");
            }

            return Convert.ToInt64(Math.Round(value, MidpointRounding.AwayFromZero), CultureInfo.InvariantCulture);
        }

        private static double ToSimulationSpeed(long velocity)
        {
            if (velocity == long.MinValue)
            {
                return long.MaxValue;
            }

            var speed = Math.Abs(velocity);
            if (speed < 1L)
            {
                speed = 1L;
            }

            return speed;
        }

        private static void WriteUInt16LE(byte[] target, int offset, ushort value)
        {
            target[offset] = (byte)(value & 0xFF);
            target[offset + 1] = (byte)((value >> 8) & 0xFF);
        }

        private static void WriteUInt32LE(byte[] target, int offset, uint value)
        {
            target[offset] = (byte)(value & 0xFF);
            target[offset + 1] = (byte)((value >> 8) & 0xFF);
            target[offset + 2] = (byte)((value >> 16) & 0xFF);
            target[offset + 3] = (byte)((value >> 24) & 0xFF);
        }

        private static void WriteInt32LE(byte[] target, int offset, int value)
        {
            unchecked
            {
                target[offset] = (byte)(value & 0xFF);
                target[offset + 1] = (byte)((value >> 8) & 0xFF);
                target[offset + 2] = (byte)((value >> 16) & 0xFF);
                target[offset + 3] = (byte)((value >> 24) & 0xFF);
            }
        }

        private static void WriteInt64LE(byte[] target, int offset, long value)
        {
            unchecked
            {
                target[offset] = (byte)(value & 0xFF);
                target[offset + 1] = (byte)((value >> 8) & 0xFF);
                target[offset + 2] = (byte)((value >> 16) & 0xFF);
                target[offset + 3] = (byte)((value >> 24) & 0xFF);
                target[offset + 4] = (byte)((value >> 32) & 0xFF);
                target[offset + 5] = (byte)((value >> 40) & 0xFF);
                target[offset + 6] = (byte)((value >> 48) & 0xFF);
                target[offset + 7] = (byte)((value >> 56) & 0xFF);
            }
        }

        private static ushort ReadUInt16LE(byte[] source, int offset)
        {
            return (ushort)(source[offset] | (source[offset + 1] << 8));
        }

        private static uint ReadUInt32LE(byte[] source, int offset)
        {
            return
                (uint)source[offset] |
                ((uint)source[offset + 1] << 8) |
                ((uint)source[offset + 2] << 16) |
                ((uint)source[offset + 3] << 24);
        }

        private static int ReadInt32LE(byte[] source, int offset)
        {
            unchecked
            {
                return (int)ReadUInt32LE(source, offset);
            }
        }

        private static long ReadInt64LE(byte[] source, int offset)
        {
            unchecked
            {
                uint low = ReadUInt32LE(source, offset);
                uint high = ReadUInt32LE(source, offset + 4);
                return ((long)high << 32) | low;
            }
        }

        private static double ReadDoubleLE(byte[] source, int offset)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToDouble(source, offset);
            }

            var bytes = new byte[8];
            Buffer.BlockCopy(source, offset, bytes, 0, 8);
            Array.Reverse(bytes);
            return BitConverter.ToDouble(bytes, 0);
        }
    }

    public sealed class MMCGroupAxis
    {
        private readonly int _handle;
        private readonly string _groupName;

        public MMCGroupAxis(string groupName, int handle)
        {
            _handle = handle;
            _groupName = string.IsNullOrWhiteSpace(groupName) ? "Group1" : groupName.Trim();
            DummyBackend.GetConnectionOrThrow(handle);

            AxisName = _groupName;
            AxisReference = (ushort)(Math.Abs((_groupName + "_" + handle.ToString(CultureInfo.InvariantCulture)).GetHashCode()) % 60000 + 1);
        }

        public string AxisName { get; private set; }
        public ushort AxisReference { get; private set; }

        public uint GroupReadStatus(ref ushort errorId)
        {
            errorId = 0;
            return 0x0007u;
        }

        public void GroupEnable()
        {
        }

        public void GroupDisable()
        {
        }

        public void GroupReset()
        {
        }

        public void GroupStop(float deceleration, float jerk, MC_BUFFERED_MODE_ENUM bufferedMode)
        {
        }

        public object GetGroupMembersInfo()
        {
            return new[]
            {
                new MMCGroupMemberInfo { Name = _groupName + "_X", AxisRef = (ushort)(AxisReference + 1) },
                new MMCGroupMemberInfo { Name = _groupName + "_Y", AxisRef = (ushort)(AxisReference + 2) },
                new MMCGroupMemberInfo { Name = _groupName + "_Z", AxisRef = (ushort)(AxisReference + 3) }
            };
        }

        public void GetStatusRegister(ref uint statusRegister, ref uint mcsLimitRegister, ref byte endMotionReason)
        {
            statusRegister = 0x0007u;
            mcsLimitRegister = 0;
            endMotionReason = 0;
        }

        public void MoveLinearAbsolute(float velocity, double[] position, MC_BUFFERED_MODE_ENUM bufferedMode)
        {
            DummyBackend.GetConnectionOrThrow(_handle);
        }

        public void MoveLinearRelative(float velocity, double[] distance, MC_BUFFERED_MODE_ENUM bufferedMode)
        {
            DummyBackend.GetConnectionOrThrow(_handle);
        }

        public void MoveLinearAbsoluteEx(
            double velocity,
            double acceleration,
            double deceleration,
            double jerk,
            double[] position,
            MC_BUFFERED_MODE_ENUM bufferedMode,
            MC_COORD_SYSTEM_ENUM coordSystem,
            NC_TRANSITION_MODE_ENUM transitionMode,
            double[] transitionParams,
            byte superimposed,
            byte execute)
        {
            DummyBackend.GetConnectionOrThrow(_handle);
        }

        public void SetKinTransformCartesian(MC_KIN_REF_CARTESIAN kin)
        {
            DummyBackend.GetConnectionOrThrow(_handle);
        }

        public void WaitUntilConditionFB(
            double reference,
            int paramId,
            int paramIndex,
            MC_CONDITIONFB_OPERATION_TYPE operationType,
            ushort sourceAxisRef,
            byte execute)
        {
            Thread.Sleep(1);
        }
    }

    public sealed class MMCBulkRead
    {
        private readonly int _handle;
        private ushort[] _nodeRefs = new ushort[0];
        private uint[] _customValues = new uint[0];

        public MMCBulkRead(int handle)
        {
            DummyBackend.GetConnectionOrThrow(handle);
            _handle = handle;
            ReadResult = new uint[0];
        }

        public bool IsConfigured { get; private set; }
        public uint[] ReadResult { get; private set; }

        public void Init(
            NC_BULKREAD_PRESET_ENUM preset,
            NC_BULKREAD_CONFIG_ENUM config,
            ushort[] nodeRefs,
            ushort nodeCount)
        {
            _nodeRefs = (nodeRefs ?? new ushort[0]).Take(nodeCount).ToArray();
            _customValues = new uint[0];
            IsConfigured = false;
        }

        public void Init(
            uint[] customValues,
            NC_BULKREAD_CONFIG_ENUM config,
            ushort[] nodeRefs,
            ushort nodeCount)
        {
            _customValues = customValues ?? new uint[0];
            _nodeRefs = (nodeRefs ?? new ushort[0]).Take(nodeCount).ToArray();
            IsConfigured = false;
        }

        public void Config()
        {
            DummyBackend.GetConnectionOrThrow(_handle);
            IsConfigured = true;
        }

        public void Perform()
        {
            if (!IsConfigured)
            {
                throw new InvalidOperationException("BulkRead is not configured.");
            }

            DummyBackend.GetConnectionOrThrow(_handle);
            if (_customValues.Length > 0)
            {
                ReadResult = _customValues.ToArray();
                return;
            }

            var result = new uint[_nodeRefs.Length];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = _nodeRefs[i];
            }

            ReadResult = result;
        }
    }
}
