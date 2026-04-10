using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET;

namespace PmasApiWpfTestApp.Services
{
    public sealed class PmasControllerContext : INotifyPropertyChanged
    {
        private readonly cbFunc _userCallback;
        private readonly MotionEndEvent _motionEndCallback;
        private readonly HomingEndEvent _homingEndCallback;
        private readonly ErrorStateEventCallback _errorStateCallback;

        private int _handle;
        private string _remoteIp;
        private string _localIp;
        private string _axisName;
        private string _groupName;
        private string _groupAxisNames;
        private MMCSingleAxis _singleAxis;
        private MMCGroupAxis _groupAxis;

        public PmasControllerContext()
        {
            Logs = new ObservableCollection<string>();
            Logs.CollectionChanged += OnLogsCollectionChanged;
            _userCallback = new cbFunc(UserCallback);
            _motionEndCallback = new MotionEndEvent(OnEndMotionEvent);
            _homingEndCallback = new HomingEndEvent(OnEndHomingEvent);
            _errorStateCallback = new ErrorStateEventCallback(OnErrorStateEvent);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> Logs { get; private set; }

        public string LogText
        {
            get { return string.Join(Environment.NewLine, Logs); }
        }

        public int Handle
        {
            get { return _handle; }
            private set
            {
                if (_handle == value)
                {
                    return;
                }

                _handle = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsConnected));
                OnPropertyChanged(nameof(StatusSummary));
            }
        }

        public string RemoteIp
        {
            get { return _remoteIp; }
            private set
            {
                _remoteIp = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusSummary));
            }
        }

        public string LocalIp
        {
            get { return _localIp; }
            private set
            {
                _localIp = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusSummary));
            }
        }

        public string AxisName
        {
            get { return _axisName; }
            private set
            {
                _axisName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusSummary));
            }
        }

        public string GroupName
        {
            get { return _groupName; }
            private set
            {
                _groupName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusSummary));
            }
        }

        public string GroupAxisNames
        {
            get { return _groupAxisNames; }
            private set
            {
                _groupAxisNames = value;
                OnPropertyChanged();
            }
        }

        public MMCSingleAxis SingleAxis
        {
            get { return _singleAxis; }
            private set
            {
                _singleAxis = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusSummary));
            }
        }

        public MMCGroupAxis GroupAxis
        {
            get { return _groupAxis; }
            private set
            {
                _groupAxis = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusSummary));
            }
        }

        public bool IsConnected
        {
            get { return Handle != 0; }
        }

        public string StatusSummary
        {
            get
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "Connected={0} Handle={1} Remote={2} Local={3} Axis={4} Group={5}",
                    IsConnected,
                    Handle,
                    string.IsNullOrWhiteSpace(RemoteIp) ? "-" : RemoteIp,
                    string.IsNullOrWhiteSpace(LocalIp) ? "-" : LocalIp,
                    string.IsNullOrWhiteSpace(AxisName) ? "-" : AxisName,
                    string.IsNullOrWhiteSpace(GroupName) ? "-" : GroupName);
            }
        }

        public void Connect(string remoteIp, int remotePort, string localIp, int localPort, uint eventMask)
        {
            if (IsConnected)
            {
                Disconnect();
            }

            IPAddress remoteAddress;
            IPAddress localAddress;
            if (!IPAddress.TryParse(remoteIp, out remoteAddress))
            {
                throw new ArgumentException("Invalid remote IP address.", nameof(remoteIp));
            }

            if (!IPAddress.TryParse(localIp, out localAddress))
            {
                throw new ArgumentException("Invalid local IP address.", nameof(localIp));
            }

            int handle;
            var result = MMCConnection.ConnectRPC(remoteAddress, remotePort, localAddress, localPort, _userCallback, eventMask, out handle);
            if (result != 0)
            {
                var libraryError = Enum.IsDefined(typeof(LibraryErrors), result)
                    ? ((LibraryErrors)result).ToString()
                    : "Unknown";
                var mmcError = Enum.IsDefined(typeof(MMCErrors), result)
                    ? ((MMCErrors)result).ToString()
                    : "Unknown";
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "MMCConnection.ConnectRPC returned {0} (LibraryErrors={1}, MMCErrors={2}). Check Remote IP/Port, Local IP/NIC binding, and controller RPC service.",
                        result,
                        libraryError,
                        mmcError));
            }

            Handle = handle;
            RemoteIp = remoteIp;
            LocalIp = localIp;

            MMCConnection.RegisterEndMotionEventCallback(handle, _motionEndCallback);
            MMCConnection.RegisterEndHomingEventCallback(handle, _homingEndCallback);
            MMCConnection.RegisterErrorStateCallback(handle, _errorStateCallback);

            Log("RPC connection established.");
        }

        public void Disconnect()
        {
            if (!IsConnected)
            {
                return;
            }

            MMCConnection.CloseConnection(MMCConnection.GetConnection(Handle));

            Handle = 0;
            SingleAxis = null;
            GroupAxis = null;
            AxisName = null;
            GroupName = null;
            GroupAxisNames = null;

            Log("Connection closed.");
        }

        public void LoadAxis(string axisName)
        {
            EnsureConnected();
            if (string.IsNullOrWhiteSpace(axisName))
            {
                throw new ArgumentException("Axis name is empty.", nameof(axisName));
            }

            SingleAxis = new MMCSingleAxis(axisName, Handle);
            AxisName = axisName;

            Log(string.Format(
                CultureInfo.InvariantCulture,
                "Axis loaded. Name={0}, AxisRef={1}, DriveID={2}",
                SingleAxis.AxisName,
                SingleAxis.AxisReference,
                SingleAxis.DriveID));
        }

        public void LoadGroup(string groupName, string groupAxisNames)
        {
            EnsureConnected();
            if (string.IsNullOrWhiteSpace(groupName))
            {
                throw new ArgumentException("Group name is empty.", nameof(groupName));
            }

            GroupAxis = new MMCGroupAxis(groupName, Handle);
            GroupName = groupName;
            GroupAxisNames = groupAxisNames;

            Log(string.Format(
                CultureInfo.InvariantCulture,
                "Group loaded. Name={0}, AxisRef={1}, Members={2}",
                GroupAxis.AxisName,
                GroupAxis.AxisReference,
                string.IsNullOrWhiteSpace(groupAxisNames) ? "-" : groupAxisNames));
        }

        public MMCConnectionObject GetConnectionObject()
        {
            EnsureConnected();
            return MMCConnection.GetConnection(Handle);
        }

        public MMCSingleAxis[] GetConfiguredGroupAxes()
        {
            EnsureConnected();

            if (string.IsNullOrWhiteSpace(GroupAxisNames))
            {
                return new MMCSingleAxis[0];
            }

            var names = GroupAxisNames
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Trim())
                .Where(item => item.Length > 0)
                .ToArray();

            var result = new List<MMCSingleAxis>();
            foreach (var name in names)
            {
                result.Add(new MMCSingleAxis(name, Handle));
            }

            return result.ToArray();
        }

        public void EnsureConnected()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Not connected.");
            }
        }

        public void EnsureAxis()
        {
            EnsureConnected();
            if (SingleAxis == null)
            {
                throw new InvalidOperationException("Axis is not loaded.");
            }
        }

        public void EnsureGroup()
        {
            EnsureConnected();
            if (GroupAxis == null)
            {
                throw new InvalidOperationException("Group is not loaded.");
            }
        }

        public void Log(string message)
        {
            var line = string.Format(
                CultureInfo.InvariantCulture,
                "[{0:HH:mm:ss}] {1}",
                DateTime.Now,
                message);

            var dispatcher = Application.Current == null ? null : Application.Current.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.BeginInvoke(new Action(() => Logs.Add(line)));
                return;
            }

            Logs.Add(line);
        }

        private void UserCallback(object sender, MMC_CAN_REPLY_DATA_OUT data)
        {
            Log(string.Format(
                CultureInfo.InvariantCulture,
                "User callback: EventType={0}, AxisRef={1}, FunctionId={2}, ErrorId={3}, Status={4}",
                data.btEventType,
                data.usAxisRef,
                data.usFunctionID,
                data.usErrorID,
                data.usStatus));
        }

        private void OnEndMotionEvent(ushort axisRef, bool result)
        {
            Log(string.Format(CultureInfo.InvariantCulture, "EndMotion: AxisRef={0}, Result={1}", axisRef, result));
        }

        private void OnEndHomingEvent(ushort axisRef, short errId)
        {
            Log(string.Format(CultureInfo.InvariantCulture, "EndHoming: AxisRef={0}, ErrorId={1}", axisRef, errId));
        }

        private void OnErrorStateEvent(ushort axisRef, short state, ushort emergencyCode)
        {
            Log(string.Format(
                CultureInfo.InvariantCulture,
                "ErrorState: AxisRef={0}, State=0x{1:X}, Emergency=0x{2:X}",
                axisRef,
                state,
                emergencyCode));
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OnLogsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(LogText));
        }
    }
}
