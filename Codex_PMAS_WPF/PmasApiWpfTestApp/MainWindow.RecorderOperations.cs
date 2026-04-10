using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET;

namespace PmasApiWpfTestApp
{
    public partial class MainWindow
    {
        private void ButtonBeginRecording_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_BeginRecordingCmd", delegate
            {
                Context.EnsureConnected();
                var gap = ParseUInt32(TextRecorderGap.Text);
                var dataLength = ParseUInt32(TextRecorderLength.Text);
                var signalBitMask = ParseUInt32(TextRecorderMask.Text);
                var recorderParams = ParseUInt32Array(TextRecorderParams.Text);
                var signalIds = ParseUInt32Array(TextRecorderSignalIds.Text);

                MMCConnection.BeginRecording(
                    Context.Handle,
                    gap,
                    dataLength,
                    signalBitMask,
                    recorderParams,
                    signalIds);
            });
        }

        private void ButtonRecStatus_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_RecStatusCmd", delegate
            {
                Context.EnsureConnected();
                uint recordingIndex;
                uint triggerStatus;
                MMCConnection.GetRecordingStatus(Context.Handle, out recordingIndex, out triggerStatus);
                Context.Log(string.Format(CultureInfo.InvariantCulture, "RecordingIndex={0}, TriggerStatus={1}", recordingIndex, triggerStatus));
            });
        }

        private void ButtonStopRecording_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_StopRecordingCmd", delegate
            {
                Context.EnsureConnected();
                MMCConnection.StopRecording(Context.Handle);
            });
        }

        private void ButtonUploadDataHeader_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_UploadDataHeaderCmd", delegate
            {
                Context.EnsureConnected();
                UploadRecorderHeaderParam header;
                MMCConnection.GetRecordingDataHeader(Context.Handle, out header);
                Context.Log(DumpObject(header));
            });
        }

        private void ButtonUploadData_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_UploadDataCmd", delegate
            {
                Context.EnsureConnected();
                var from = ParseUInt32(TextRecorderFrom.Text);
                var to = ParseUInt32(TextRecorderTo.Text);
                var bufferIndex = ParseUInt32(TextRecorderBufferIndex.Text);
                if (to < from)
                {
                    throw new InvalidOperationException("Recorder 'To' must be >= 'From'.");
                }

                var length = checked((int)(to - from + 1));
                var data = new int[length];
                MMCConnection.GetRecordingData(Context.Handle, from, to, bufferIndex, out data);
                Context.Log("UploadData count = " + data.Length.ToString(CultureInfo.InvariantCulture));
                Context.Log("UploadData values = " + string.Join(",", data.Select(v => v.ToString(CultureInfo.InvariantCulture))));
            });
        }
    }
}
