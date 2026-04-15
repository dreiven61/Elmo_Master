using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET;
using PmasApiWpfTestApp.Services;

namespace PmasApiWpfTestApp
{
    public partial class MainWindow
    {
        private const int MaxPositionReadSamplesToSave = 300000;
        private const int DefaultWarmupCycles = 10;
        private CancellationTokenSource _cycleTestCancellation;
        private bool _isCycleTestRunning;
        private CycleTestSnapshot _lastCycleTestSnapshot;
        private CancellationTokenSource _cycleTest2Cancellation;
        private bool _isCycleTest2Running;
        private CycleTestSnapshot _lastCycleTest2Snapshot;

        private sealed class CycleTestOptions
        {
            public int RequestedCycles { get; set; }
            public int WarmupCycles { get; set; }
            public double BasePosition { get; set; }
            public double MoveDistanceMm { get; set; }
            public int MoveTimeoutMs { get; set; }
            public int PollIntervalMs { get; set; }
            public int StableSamplesRequired { get; set; }
            public int DropThresholdMs { get; set; }
            public double InPositionTolerance { get; set; }
            public bool StopOnTimeout { get; set; }
            public bool StopOnAxisError { get; set; }
            public double Jerk { get; set; }
            public MC_DIRECTION_ENUM Direction { get; set; }
            public MC_BUFFERED_MODE_ENUM BufferedMode { get; set; }
            public double Velocity { get; set; }
            public double Acceleration { get; set; }
            public double Deceleration { get; set; }
            public bool UseHighPriorityWorkerThread { get; set; }
            public bool UseHighPrecisionWait { get; set; }
            public bool Request1msTimerResolution { get; set; }

            public double ForwardPosition
            {
                get { return BasePosition + MoveDistanceMm; }
            }

            public double ReturnPosition
            {
                get { return BasePosition; }
            }
        }

        private sealed class RunningMetric
        {
            private long _count;
            private double _sum;
            private double _max;

            public void Add(double value)
            {
                _count++;
                _sum += value;
                if (_count == 1 || value > _max)
                {
                    _max = value;
                }
            }

            public long Count
            {
                get { return _count; }
            }

            public double Average
            {
                get { return _count == 0 ? 0.0 : _sum / _count; }
            }

            public double Max
            {
                get { return _count == 0 ? 0.0 : _max; }
            }
        }

        private sealed class WaitPhaseResult
        {
            public WaitPhaseResult(bool success, double settleMilliseconds, double positionError)
            {
                Success = success;
                SettleMilliseconds = settleMilliseconds;
                PositionError = positionError;
            }

            public bool Success { get; private set; }
            public double SettleMilliseconds { get; private set; }
            public double PositionError { get; private set; }
        }

        private sealed class PositionReadSample
        {
            public long SampleIndex { get; set; }
            public int CycleIndex { get; set; }
            public string Phase { get; set; }
            public double TargetPosition { get; set; }
            public double ActualPosition { get; set; }
            public double PositionError { get; set; }
            public bool InTolerance { get; set; }
            public double ReadStartFromTestMs { get; set; }
            public double ReadEndFromTestMs { get; set; }
            public double ReadLatencyMs { get; set; }
        }

        private sealed class CycleTestMetrics
        {
            public CycleTestMetrics()
            {
                CycleTimeMs = new RunningMetric();
                ResponseLatencyMs = new RunningMetric();
                PollPeriodMs = new RunningMetric();
                ForwardSettleMs = new RunningMetric();
                ReturnSettleMs = new RunningMetric();
                PositionReadSamples = new List<PositionReadSample>();
                StopReason = "Completed";
            }

            public int AttemptedCycles { get; set; }
            public int SuccessfulCycles { get; set; }
            public int ForwardTimeouts { get; set; }
            public int ReturnTimeouts { get; set; }
            public int AxisErrorCount { get; set; }
            public int ExceptionCount { get; set; }
            public int DropCount { get; set; }
            public double MaxInPositionError { get; set; }
            public double TotalElapsedMs { get; set; }
            public string LastError { get; set; }
            public string StopReason { get; set; }
            public DateTime TestStartedAt { get; set; }
            public long TestStartedTick { get; set; }
            public long PositionReadSampleCounter { get; set; }
            public int PositionReadSamplesDropped { get; set; }
            public RunningMetric CycleTimeMs { get; private set; }
            public RunningMetric ResponseLatencyMs { get; private set; }
            public RunningMetric PollPeriodMs { get; private set; }
            public RunningMetric ForwardSettleMs { get; private set; }
            public RunningMetric ReturnSettleMs { get; private set; }
            public IList<PositionReadSample> PositionReadSamples { get; private set; }
        }

        private sealed class CycleTestSnapshot
        {
            public DateTime CompletedAt { get; set; }
            public string AxisName { get; set; }
            public string RemoteIp { get; set; }
            public CycleTestOptions Options { get; set; }
            public CycleTestMetrics Metrics { get; set; }
            public string SummaryText { get; set; }
        }

        private async void ButtonStartCycleTest_Click(object sender, RoutedEventArgs e)
        {
            if (_isCycleTestRunning)
            {
                return;
            }

            if (_isCycleTest2Running)
            {
                MessageBox.Show("Cycle Test2 is running. Stop it first.", "Cycle Test", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                Context.EnsureAxis();

                var options = BuildCycleTestOptions();

                _cycleTestCancellation = new CancellationTokenSource();
                _isCycleTestRunning = true;
                ToggleCycleTestControls(true);
                ResetCycleTestOutput();
                SetCycleTestStatus(string.Format(
                    CultureInfo.InvariantCulture,
                    "Running... Base={0}, Forward={1}, Distance={2}mm, Cycles={3}, Warmup={4}, Vel={5:F3}, Acc={6:F3}, Dec={7:F3}, Jerk={8:F3}",
                    options.BasePosition,
                    options.ForwardPosition,
                    options.MoveDistanceMm,
                    options.RequestedCycles,
                    options.WarmupCycles,
                    options.Velocity,
                    options.Acceleration,
                    options.Deceleration,
                    options.Jerk));

                Context.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "CycleTest started: base={0}, forward={1}, distanceMm={2}, vel={3}, acc={4}, dec={5}, jerk={6}, cycles={7}, warmupCycles={8}, tol={9}, timeoutMs={10}, pollMs={11}, highPriority={12}, highPrecisionWait={13}, timer1ms={14}",
                    options.BasePosition,
                    options.ForwardPosition,
                    options.MoveDistanceMm,
                    options.Velocity,
                    options.Acceleration,
                    options.Deceleration,
                    options.Jerk,
                    options.RequestedCycles,
                    options.WarmupCycles,
                    options.InPositionTolerance,
                    options.MoveTimeoutMs,
                    options.PollIntervalMs,
                    options.UseHighPriorityWorkerThread,
                    options.UseHighPrecisionWait,
                    options.Request1msTimerResolution));

                var metrics = await Task.Factory.StartNew(
                    () => ExecuteCycleTest(options, _cycleTestCancellation.Token),
                    _cycleTestCancellation.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);
                UpdateCycleTestUi(options, metrics);
                Context.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "CycleTest finished: attempted={0}, success={1}, forwardTimeout={2}, returnTimeout={3}, axisError={4}, exceptions={5}, drop={6}",
                    metrics.AttemptedCycles,
                    metrics.SuccessfulCycles,
                    metrics.ForwardTimeouts,
                    metrics.ReturnTimeouts,
                    metrics.AxisErrorCount,
                    metrics.ExceptionCount,
                    metrics.DropCount));

                StoreLastCycleTestSnapshot(options, metrics);
                ButtonSaveCycleResult.IsEnabled = true;

                if (CheckCycleAutoSaveResult.IsChecked == true)
                {
                    var savedPath = SaveLastCycleResultToExcel();
                    Context.Log("CycleTest result saved: " + savedPath);
                    SetCycleTestStatus("Completed. Result saved: " + savedPath);
                }
            }
            catch (OperationCanceledException)
            {
                SetCycleTestStatus("Canceled by user.");
                Context.Log("CycleTest canceled by user.");
            }
            catch (Exception ex)
            {
                SetCycleTestStatus("Failed: " + ex.Message);
                Context.Log("CycleTest failed: " + ex.Message);
                MessageBox.Show(ex.ToString(), "Cycle Test", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isCycleTestRunning = false;
                ToggleCycleTestControls(false);
                if (_cycleTestCancellation != null)
                {
                    _cycleTestCancellation.Dispose();
                    _cycleTestCancellation = null;
                }
            }
        }

        private void ButtonStopCycleTest_Click(object sender, RoutedEventArgs e)
        {
            if (!_isCycleTestRunning || _cycleTestCancellation == null)
            {
                return;
            }

            _cycleTestCancellation.Cancel();
            SetCycleTestStatus("Stop requested...");
            Context.Log("CycleTest stop requested.");

            try
            {
                Context.EnsureAxis();
                Context.SingleAxis.Stop((MC_BUFFERED_MODE_ENUM)ComboBufferedMode.SelectedItem);
            }
            catch (Exception ex)
            {
                Context.Log("CycleTest stop command failed: " + ex.Message);
            }
        }

        private void ButtonSaveCycleResult_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var savedPath = SaveLastCycleResultToExcel();
                Context.Log("CycleTest result saved: " + savedPath);
                SetCycleTestStatus("Result saved: " + savedPath);
            }
            catch (Exception ex)
            {
                Context.Log("CycleTest result save failed: " + ex.Message);
                MessageBox.Show(ex.Message, "Save Cycle Result", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ButtonStartCycleTest2_Click(object sender, RoutedEventArgs e)
        {
            if (_isCycleTest2Running)
            {
                return;
            }

            if (_isCycleTestRunning)
            {
                MessageBox.Show("Cycle Test is running. Stop it first.", "Cycle Test2", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                Context.EnsureAxis();

                var options = BuildCycleTest2Options();

                _cycleTest2Cancellation = new CancellationTokenSource();
                _isCycleTest2Running = true;
                ToggleCycleTest2Controls(true);
                ResetCycleTest2Output();
                SetCycleTest2Status(string.Format(
                    CultureInfo.InvariantCulture,
                    "Running... Base={0}, Forward={1}, Distance={2}mm, Cycles={3}, Warmup={4}, Vel={5:F3}, Acc={6:F3}, Dec={7:F3}, Jerk={8:F3}",
                    options.BasePosition,
                    options.ForwardPosition,
                    options.MoveDistanceMm,
                    options.RequestedCycles,
                    options.WarmupCycles,
                    options.Velocity,
                    options.Acceleration,
                    options.Deceleration,
                    options.Jerk));

                Context.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "CycleTest2 started(no in-position wait): base={0}, forward={1}, distanceMm={2}, vel={3}, acc={4}, dec={5}, jerk={6}, cycles={7}, warmupCycles={8}, commandIntervalMs={9}, highPriority={10}, highPrecisionWait={11}, timer1ms={12}",
                    options.BasePosition,
                    options.ForwardPosition,
                    options.MoveDistanceMm,
                    options.Velocity,
                    options.Acceleration,
                    options.Deceleration,
                    options.Jerk,
                    options.RequestedCycles,
                    options.WarmupCycles,
                    options.PollIntervalMs,
                    options.UseHighPriorityWorkerThread,
                    options.UseHighPrecisionWait,
                    options.Request1msTimerResolution));

                var metrics = await Task.Factory.StartNew(
                    () => ExecuteCycleTest2(options, _cycleTest2Cancellation.Token),
                    _cycleTest2Cancellation.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);
                UpdateCycleTest2Ui(options, metrics);
                Context.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "CycleTest2 finished: attempted={0}, success={1}, axisError={2}, exceptions={3}, drop={4}",
                    metrics.AttemptedCycles,
                    metrics.SuccessfulCycles,
                    metrics.AxisErrorCount,
                    metrics.ExceptionCount,
                    metrics.DropCount));

                StoreLastCycleTest2Snapshot(options, metrics);
                ButtonSaveCycleResult2.IsEnabled = true;

                if (CheckCycle2AutoSaveResult.IsChecked == true)
                {
                    var savedPath = SaveLastCycleResult2ToExcel();
                    Context.Log("CycleTest2 result saved: " + savedPath);
                    SetCycleTest2Status("Completed. Result saved: " + savedPath);
                }
            }
            catch (OperationCanceledException)
            {
                SetCycleTest2Status("Canceled by user.");
                Context.Log("CycleTest2 canceled by user.");
            }
            catch (Exception ex)
            {
                SetCycleTest2Status("Failed: " + ex.Message);
                Context.Log("CycleTest2 failed: " + ex.Message);
                MessageBox.Show(ex.ToString(), "Cycle Test2", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isCycleTest2Running = false;
                ToggleCycleTest2Controls(false);
                if (_cycleTest2Cancellation != null)
                {
                    _cycleTest2Cancellation.Dispose();
                    _cycleTest2Cancellation = null;
                }
            }
        }

        private void ButtonStopCycleTest2_Click(object sender, RoutedEventArgs e)
        {
            if (!_isCycleTest2Running || _cycleTest2Cancellation == null)
            {
                return;
            }

            _cycleTest2Cancellation.Cancel();
            SetCycleTest2Status("Stop requested...");
            Context.Log("CycleTest2 stop requested.");

            try
            {
                Context.EnsureAxis();
                Context.SingleAxis.Stop((MC_BUFFERED_MODE_ENUM)ComboBufferedMode.SelectedItem);
            }
            catch (Exception ex)
            {
                Context.Log("CycleTest2 stop command failed: " + ex.Message);
            }
        }

        private void ButtonSaveCycleResult2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var savedPath = SaveLastCycleResult2ToExcel();
                Context.Log("CycleTest2 result saved: " + savedPath);
                SetCycleTest2Status("Result saved: " + savedPath);
            }
            catch (Exception ex)
            {
                Context.Log("CycleTest2 result save failed: " + ex.Message);
                MessageBox.Show(ex.Message, "Save Cycle Test2 Result", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private CycleTestOptions BuildCycleTest2Options()
        {
            var options = new CycleTestOptions();
            options.WarmupCycles = DefaultWarmupCycles;
            options.BasePosition = ParseDouble(TextCycle2BasePosition.Text);
            options.MoveDistanceMm = ParseDouble(TextCycle2MoveDistanceMm.Text);
            options.Velocity = ParseDouble(TextCycle2Velocity.Text);
            options.Acceleration = ParseDouble(TextCycle2Acceleration.Text);
            options.Deceleration = ParseDouble(TextCycle2Deceleration.Text);
            options.RequestedCycles = ParseInt32(TextCycle2Count.Text);
            options.Jerk = ParseDouble(TextCycle2Jerk.Text);
            options.PollIntervalMs = ParseInt32(TextCycle2PollIntervalMs.Text);
            options.DropThresholdMs = ParseInt32(TextCycle2DropThresholdMs.Text);
            options.StopOnAxisError = CheckCycle2StopOnError.IsChecked == true;
            options.StopOnTimeout = false;
            options.MoveTimeoutMs = 0;
            options.StableSamplesRequired = 0;
            options.InPositionTolerance = 0.0;
            options.Direction = NormalizeDirectionForAbsoluteMove((MC_DIRECTION_ENUM)ComboDirection.SelectedItem);
            options.BufferedMode = (MC_BUFFERED_MODE_ENUM)ComboBufferedMode.SelectedItem;
            options.UseHighPriorityWorkerThread = CheckCycle2HighPriorityThread.IsChecked == true;
            options.UseHighPrecisionWait = CheckCycle2HighPrecisionWait.IsChecked == true;
            options.Request1msTimerResolution = CheckCycle2Use1msTimerResolution.IsChecked == true;

            if (options.RequestedCycles <= 0)
            {
                throw new InvalidOperationException("Cycle Count must be > 0.");
            }

            if (Math.Abs(options.MoveDistanceMm) <= 0.0)
            {
                throw new InvalidOperationException("Move Distance(mm) must be non-zero.");
            }

            if (options.Velocity <= 0.0)
            {
                throw new InvalidOperationException("Velocity must be > 0.");
            }

            if (options.Acceleration <= 0.0)
            {
                throw new InvalidOperationException("Acceleration must be > 0.");
            }

            if (options.Deceleration <= 0.0)
            {
                throw new InvalidOperationException("Deceleration must be > 0.");
            }

            if (options.Jerk <= 0.0)
            {
                throw new InvalidOperationException("Jerk must be > 0.");
            }

            if (options.PollIntervalMs <= 0)
            {
                throw new InvalidOperationException("Command Interval must be > 0.");
            }

            if (options.DropThresholdMs <= 0)
            {
                throw new InvalidOperationException("Drop Threshold must be > 0.");
            }

            return options;
        }

        private CycleTestOptions BuildCycleTestOptions()
        {
            var options = new CycleTestOptions();
            options.WarmupCycles = DefaultWarmupCycles;
            options.BasePosition = ParseDouble(TextCycleBasePosition.Text);
            options.MoveDistanceMm = ParseDouble(TextCycleMoveDistanceMm.Text);
            options.Velocity = ParseDouble(TextCycleVelocity.Text);
            options.Acceleration = ParseDouble(TextCycleAcceleration.Text);
            options.Deceleration = ParseDouble(TextCycleDeceleration.Text);
            options.RequestedCycles = ParseInt32(TextCycleCount.Text);
            options.InPositionTolerance = ParseDouble(TextCycleTolerance.Text);
            options.Jerk = ParseDouble(TextCycleJerk.Text);
            options.MoveTimeoutMs = ParseInt32(TextCycleMoveTimeoutMs.Text);
            options.PollIntervalMs = ParseInt32(TextCyclePollIntervalMs.Text);
            options.StableSamplesRequired = ParseInt32(TextCycleStableSamples.Text);
            options.DropThresholdMs = ParseInt32(TextCycleDropThresholdMs.Text);
            options.StopOnTimeout = CheckCycleStopOnTimeout.IsChecked == true;
            options.StopOnAxisError = CheckCycleStopOnError.IsChecked == true;
            options.Direction = NormalizeDirectionForAbsoluteMove((MC_DIRECTION_ENUM)ComboDirection.SelectedItem);
            options.BufferedMode = (MC_BUFFERED_MODE_ENUM)ComboBufferedMode.SelectedItem;
            options.UseHighPriorityWorkerThread = CheckCycleHighPriorityThread.IsChecked == true;
            options.UseHighPrecisionWait = CheckCycleHighPrecisionWait.IsChecked == true;
            options.Request1msTimerResolution = CheckCycleUse1msTimerResolution.IsChecked == true;

            if (options.RequestedCycles <= 0)
            {
                throw new InvalidOperationException("Cycle Count must be > 0.");
            }

            if (Math.Abs(options.MoveDistanceMm) <= 0.0)
            {
                throw new InvalidOperationException("Move Distance(mm) must be non-zero.");
            }

            if (options.Velocity <= 0.0)
            {
                throw new InvalidOperationException("Velocity must be > 0.");
            }

            if (options.Acceleration <= 0.0)
            {
                throw new InvalidOperationException("Acceleration must be > 0.");
            }

            if (options.Deceleration <= 0.0)
            {
                throw new InvalidOperationException("Deceleration must be > 0.");
            }

            if (options.Jerk <= 0.0)
            {
                throw new InvalidOperationException("Jerk must be > 0.");
            }

            if (options.MoveTimeoutMs <= 0)
            {
                throw new InvalidOperationException("Move Timeout must be > 0.");
            }

            if (options.PollIntervalMs <= 0)
            {
                throw new InvalidOperationException("Poll Interval must be > 0.");
            }

            if (options.StableSamplesRequired <= 0)
            {
                throw new InvalidOperationException("Stable Samples must be > 0.");
            }

            if (options.DropThresholdMs <= 0)
            {
                throw new InvalidOperationException("Drop Threshold must be > 0.");
            }

            if (options.InPositionTolerance <= 0.0)
            {
                throw new InvalidOperationException("In-position Tolerance must be > 0.");
            }

            return options;
        }

        private CycleTestMetrics ExecuteCycleTest(CycleTestOptions options, CancellationToken token)
        {
            CycleTestMetrics metrics = new CycleTestMetrics();
            var totalStopwatch = Stopwatch.StartNew();
            var currentThread = Thread.CurrentThread;
            var previousPriority = currentThread.Priority;
            metrics.TestStartedAt = DateTime.Now;
            metrics.TestStartedTick = Stopwatch.GetTimestamp();
            var warmupCycles = Math.Max(0, options.WarmupCycles);
            var totalCycles = options.RequestedCycles + warmupCycles;
            var measurementStarted = warmupCycles == 0;

            if (measurementStarted)
            {
                metrics.TestStartedAt = DateTime.Now;
                metrics.TestStartedTick = Stopwatch.GetTimestamp();
            }
            else
            {
                Context.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "CycleTest warm-up started: cycles={0} (excluded from recording)",
                    warmupCycles));
            }

            try
            {
                if (options.UseHighPriorityWorkerThread)
                {
                    currentThread.Priority = ThreadPriority.Highest;
                }

                using (var timerScope = options.Request1msTimerResolution
                    ? HighResolutionTimerScope.TryCreate(1, Context)
                    : null)
                {
                    for (int cycleIndex = 1; cycleIndex <= totalCycles; cycleIndex++)
                    {
                        var isWarmup = cycleIndex <= warmupCycles;
                        var recordedCycleIndex = isWarmup ? cycleIndex : cycleIndex - warmupCycles;
                        var phaseLabel = isWarmup ? "Warm-up" : "Cycle";

                        if (!measurementStarted && !isWarmup)
                        {
                            metrics = new CycleTestMetrics();
                            metrics.TestStartedAt = DateTime.Now;
                            metrics.TestStartedTick = Stopwatch.GetTimestamp();
                            totalStopwatch.Restart();
                            measurementStarted = true;
                            Context.Log("CycleTest measurement recording started after warm-up.");
                        }

                        token.ThrowIfCancellationRequested();
                        metrics.AttemptedCycles = recordedCycleIndex;

                        var cycleStopwatch = Stopwatch.StartNew();
                        bool cyclePassed = true;

                        try
                        {
                            IssueCycleMove(options.ForwardPosition, options);
                            var forwardResult = WaitForInPosition(options.ForwardPosition, options, metrics, recordedCycleIndex, "Forward", token);
                            metrics.ForwardSettleMs.Add(forwardResult.SettleMilliseconds);
                            if (forwardResult.PositionError > metrics.MaxInPositionError)
                            {
                                metrics.MaxInPositionError = forwardResult.PositionError;
                            }

                            if (!forwardResult.Success)
                            {
                                metrics.ForwardTimeouts++;
                                cyclePassed = false;
                                Context.Log(string.Format(
                                    CultureInfo.InvariantCulture,
                                    "{0} {1}: forward timeout (target={2}, error={3:F6})",
                                    phaseLabel,
                                    recordedCycleIndex,
                                    options.ForwardPosition,
                                    forwardResult.PositionError));
                                if (options.StopOnTimeout)
                                {
                                    metrics.StopReason = isWarmup
                                        ? "Stopped during warm-up: forward timeout"
                                        : "Stopped: forward timeout";
                                    break;
                                }
                            }

                            IssueCycleMove(options.ReturnPosition, options);
                            var returnResult = WaitForInPosition(options.ReturnPosition, options, metrics, recordedCycleIndex, "Return", token);
                            metrics.ReturnSettleMs.Add(returnResult.SettleMilliseconds);
                            if (returnResult.PositionError > metrics.MaxInPositionError)
                            {
                                metrics.MaxInPositionError = returnResult.PositionError;
                            }

                            if (!returnResult.Success)
                            {
                                metrics.ReturnTimeouts++;
                                cyclePassed = false;
                                Context.Log(string.Format(
                                    CultureInfo.InvariantCulture,
                                    "{0} {1}: return timeout (target={2}, error={3:F6})",
                                    phaseLabel,
                                    recordedCycleIndex,
                                    options.ReturnPosition,
                                    returnResult.PositionError));
                                if (options.StopOnTimeout)
                                {
                                    metrics.StopReason = isWarmup
                                        ? "Stopped during warm-up: return timeout"
                                        : "Stopped: return timeout";
                                    break;
                                }
                            }

                            ushort emergencyCode = 0;
                            var axisError = Context.SingleAxis.GetAxisError(ref emergencyCode);
                            if (axisError != 0 || emergencyCode != 0)
                            {
                                metrics.AxisErrorCount++;
                                cyclePassed = false;
                                Context.Log(string.Format(
                                    CultureInfo.InvariantCulture,
                                    "{0} {1}: axis error detected (AxisError={2}, EmergencyCode=0x{3:X})",
                                    phaseLabel,
                                    recordedCycleIndex,
                                    axisError,
                                    emergencyCode));
                                if (options.StopOnAxisError)
                                {
                                    metrics.StopReason = isWarmup
                                        ? "Stopped during warm-up: axis error"
                                        : "Stopped: axis error";
                                    break;
                                }
                            }
                        }
                        catch (MMCException ex)
                        {
                            cyclePassed = false;
                            metrics.ExceptionCount++;
                            metrics.LastError = ex.Message;
                            Context.Log(string.Format(
                                CultureInfo.InvariantCulture,
                                "{0} {1}: MMCException Command={2}, LibraryError={3}, MMCError={4}, Status={5}, AxisRef={6}",
                                phaseLabel,
                                recordedCycleIndex,
                                ex.CommandID,
                                ex.LibraryError,
                                ex.MMCError,
                                ex.Status,
                                ex.AxisRef));

                            if (options.StopOnAxisError)
                            {
                                metrics.StopReason = isWarmup
                                    ? "Stopped during warm-up: MMCException"
                                    : "Stopped: MMCException";
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            cyclePassed = false;
                            metrics.ExceptionCount++;
                            metrics.LastError = ex.Message;
                            Context.Log(string.Format(
                                CultureInfo.InvariantCulture,
                                "{0} {1}: Exception {2}",
                                phaseLabel,
                                recordedCycleIndex,
                                ex.Message));

                            if (options.StopOnAxisError)
                            {
                                metrics.StopReason = isWarmup
                                    ? "Stopped during warm-up: exception"
                                    : "Stopped: exception";
                                break;
                            }
                        }
                        finally
                        {
                            cycleStopwatch.Stop();
                            if (!isWarmup)
                            {
                                metrics.CycleTimeMs.Add(cycleStopwatch.Elapsed.TotalMilliseconds);
                                if (cyclePassed)
                                {
                                    metrics.SuccessfulCycles++;
                                }
                            }
                        }

                        if (!isWarmup && (recordedCycleIndex % 10 == 0 || recordedCycleIndex == options.RequestedCycles))
                        {
                            UpdateCycleTestUi(options, metrics);
                        }
                        else if (isWarmup && (recordedCycleIndex % 10 == 0 || recordedCycleIndex == warmupCycles))
                        {
                            SetCycleTestStatus(string.Format(
                                CultureInfo.InvariantCulture,
                                "Warm-up... {0}/{1} cycles (recording not started)",
                                recordedCycleIndex,
                                warmupCycles));
                        }
                    }
                }
            }
            finally
            {
                currentThread.Priority = previousPriority;
            }

            if (!measurementStarted)
            {
                metrics.AttemptedCycles = 0;
                metrics.SuccessfulCycles = 0;
            }

            totalStopwatch.Stop();
            metrics.TotalElapsedMs = totalStopwatch.Elapsed.TotalMilliseconds;
            UpdateCycleTestUi(options, metrics);
            return metrics;
        }

        private CycleTestMetrics ExecuteCycleTest2(CycleTestOptions options, CancellationToken token)
        {
            CycleTestMetrics metrics = new CycleTestMetrics();
            var totalStopwatch = Stopwatch.StartNew();
            var currentThread = Thread.CurrentThread;
            var previousPriority = currentThread.Priority;
            long previousCommandTick = 0;
            var warmupCycles = Math.Max(0, options.WarmupCycles);
            var totalCycles = options.RequestedCycles + warmupCycles;
            var measurementStarted = warmupCycles == 0;

            if (!measurementStarted)
            {
                Context.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "CycleTest2 warm-up started: cycles={0} (excluded from recording)",
                    warmupCycles));
            }

            try
            {
                if (options.UseHighPriorityWorkerThread)
                {
                    currentThread.Priority = ThreadPriority.Highest;
                }

                using (var timerScope = options.Request1msTimerResolution
                    ? HighResolutionTimerScope.TryCreate(1, Context)
                    : null)
                {
                    for (int cycleIndex = 1; cycleIndex <= totalCycles; cycleIndex++)
                    {
                        var isWarmup = cycleIndex <= warmupCycles;
                        var recordedCycleIndex = isWarmup ? cycleIndex : cycleIndex - warmupCycles;
                        var phaseLabel = isWarmup ? "Warm-up" : "Cycle2";

                        if (!measurementStarted && !isWarmup)
                        {
                            metrics = new CycleTestMetrics();
                            totalStopwatch.Restart();
                            previousCommandTick = 0;
                            measurementStarted = true;
                            Context.Log("CycleTest2 measurement recording started after warm-up.");
                        }

                        token.ThrowIfCancellationRequested();
                        metrics.AttemptedCycles = recordedCycleIndex;

                        var cycleStopwatch = Stopwatch.StartNew();
                        bool cyclePassed = true;

                        try
                        {
                            IssueCycleMoveWithoutInPositionWait(options.ForwardPosition, options, metrics, ref previousCommandTick);
                            WaitForPollInterval(options.PollIntervalMs, options.UseHighPrecisionWait, options.Request1msTimerResolution, token);
                            IssueCycleMoveWithoutInPositionWait(options.ReturnPosition, options, metrics, ref previousCommandTick);
                            WaitForPollInterval(options.PollIntervalMs, options.UseHighPrecisionWait, options.Request1msTimerResolution, token);

                            ushort emergencyCode = 0;
                            var axisError = Context.SingleAxis.GetAxisError(ref emergencyCode);
                            if (axisError != 0 || emergencyCode != 0)
                            {
                                metrics.AxisErrorCount++;
                                cyclePassed = false;
                                Context.Log(string.Format(
                                    CultureInfo.InvariantCulture,
                                    "{0} {1}: axis error detected (AxisError={2}, EmergencyCode=0x{3:X})",
                                    phaseLabel,
                                    recordedCycleIndex,
                                    axisError,
                                    emergencyCode));
                                if (options.StopOnAxisError)
                                {
                                    metrics.StopReason = isWarmup
                                        ? "Stopped during warm-up: axis error"
                                        : "Stopped: axis error";
                                    break;
                                }
                            }
                        }
                        catch (MMCException ex)
                        {
                            cyclePassed = false;
                            metrics.ExceptionCount++;
                            metrics.LastError = ex.Message;
                            Context.Log(string.Format(
                                CultureInfo.InvariantCulture,
                                "{0} {1}: MMCException Command={2}, LibraryError={3}, MMCError={4}, Status={5}, AxisRef={6}",
                                phaseLabel,
                                recordedCycleIndex,
                                ex.CommandID,
                                ex.LibraryError,
                                ex.MMCError,
                                ex.Status,
                                ex.AxisRef));

                            if (options.StopOnAxisError)
                            {
                                metrics.StopReason = isWarmup
                                    ? "Stopped during warm-up: MMCException"
                                    : "Stopped: MMCException";
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            cyclePassed = false;
                            metrics.ExceptionCount++;
                            metrics.LastError = ex.Message;
                            Context.Log(string.Format(
                                CultureInfo.InvariantCulture,
                                "{0} {1}: Exception {2}",
                                phaseLabel,
                                recordedCycleIndex,
                                ex.Message));

                            if (options.StopOnAxisError)
                            {
                                metrics.StopReason = isWarmup
                                    ? "Stopped during warm-up: exception"
                                    : "Stopped: exception";
                                break;
                            }
                        }
                        finally
                        {
                            cycleStopwatch.Stop();
                            if (!isWarmup)
                            {
                                metrics.CycleTimeMs.Add(cycleStopwatch.Elapsed.TotalMilliseconds);
                                if (cyclePassed)
                                {
                                    metrics.SuccessfulCycles++;
                                }
                            }
                        }

                        if (!isWarmup && (recordedCycleIndex % 10 == 0 || recordedCycleIndex == options.RequestedCycles))
                        {
                            UpdateCycleTest2Ui(options, metrics);
                        }
                        else if (isWarmup && (recordedCycleIndex % 10 == 0 || recordedCycleIndex == warmupCycles))
                        {
                            SetCycleTest2Status(string.Format(
                                CultureInfo.InvariantCulture,
                                "Warm-up... {0}/{1} cycles (recording not started)",
                                recordedCycleIndex,
                                warmupCycles));
                        }
                    }
                }
            }
            finally
            {
                currentThread.Priority = previousPriority;
            }

            if (!measurementStarted)
            {
                metrics.AttemptedCycles = 0;
                metrics.SuccessfulCycles = 0;
            }

            totalStopwatch.Stop();
            metrics.TotalElapsedMs = totalStopwatch.Elapsed.TotalMilliseconds;
            UpdateCycleTest2Ui(options, metrics);
            return metrics;
        }

        private void IssueCycleMoveWithoutInPositionWait(
            double targetPosition,
            CycleTestOptions options,
            CycleTestMetrics metrics,
            ref long previousCommandTick)
        {
            var nowTick = Stopwatch.GetTimestamp();
            if (previousCommandTick != 0)
            {
                var commandPeriodMs = (nowTick - previousCommandTick) * 1000.0 / Stopwatch.Frequency;
                metrics.PollPeriodMs.Add(commandPeriodMs);
                if (commandPeriodMs > options.DropThresholdMs)
                {
                    metrics.DropCount++;
                }
            }

            previousCommandTick = nowTick;

            var commandStopwatch = Stopwatch.StartNew();
            IssueCycleMove(targetPosition, options);
            commandStopwatch.Stop();
            metrics.ResponseLatencyMs.Add(commandStopwatch.Elapsed.TotalMilliseconds);
        }

        private int IssueCycleMove(double targetPosition, CycleTestOptions options)
        {
            var result = Context.SingleAxis.MoveAbsolute(
                ParseScaledLongForMove(targetPosition),
                ParseScaledLongForMove(options.Velocity),
                ParseScaledLongForMove(options.Acceleration),
                ParseScaledLongForMove(options.Deceleration),
                ParseScaledLongForMove(options.Jerk),
                options.Direction,
                options.BufferedMode);

            if (result != 0)
            {
                Context.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "MoveAbsolute returned non-zero code: {0}",
                    result));
            }

            return result;
        }

        private static MC_DIRECTION_ENUM NormalizeDirectionForAbsoluteMove(MC_DIRECTION_ENUM direction)
        {
            if (direction == MC_DIRECTION_ENUM.MC_NONE_DIRECTION || direction == MC_DIRECTION_ENUM.MC_CURRENT_DIRECTION)
            {
                return MC_DIRECTION_ENUM.MC_SHORTEST_WAY;
            }

            return direction;
        }

        private WaitPhaseResult WaitForInPosition(
            double targetPosition,
            CycleTestOptions options,
            CycleTestMetrics metrics,
            int cycleIndex,
            string phase,
            CancellationToken token)
        {
            var waitStopwatch = Stopwatch.StartNew();
            long previousTick = 0;
            int stableCounter = 0;
            double lastError = double.MaxValue;
            var scaledTargetPosition = ParseScaledLongForMove(targetPosition);
            var scaledTolerance = Math.Abs(options.InPositionTolerance) * 10000.0;

            while (waitStopwatch.ElapsedMilliseconds <= options.MoveTimeoutMs)
            {
                token.ThrowIfCancellationRequested();

                var nowTick = Stopwatch.GetTimestamp();
                if (previousTick != 0)
                {
                    var pollPeriod = (nowTick - previousTick) * 1000.0 / Stopwatch.Frequency;
                    metrics.PollPeriodMs.Add(pollPeriod);
                    if (pollPeriod > options.DropThresholdMs)
                    {
                        metrics.DropCount++;
                    }
                }
                previousTick = nowTick;

                var readStartTick = Stopwatch.GetTimestamp();
                var actualPosition = Context.SingleAxis.GetActualPosition();
                var readEndTick = Stopwatch.GetTimestamp();
                var readLatencyMs = (readEndTick - readStartTick) * 1000.0 / Stopwatch.Frequency;
                metrics.ResponseLatencyMs.Add(readLatencyMs);

                lastError = Math.Abs(scaledTargetPosition - actualPosition);
                var inTolerance = lastError <= scaledTolerance;
                AppendPositionReadSample(
                    metrics,
                    cycleIndex,
                    phase,
                    scaledTargetPosition,
                    actualPosition,
                    lastError,
                    inTolerance,
                    readStartTick,
                    readEndTick,
                    readLatencyMs);

                if (inTolerance)
                {
                    stableCounter++;
                    if (stableCounter >= options.StableSamplesRequired)
                    {
                        return new WaitPhaseResult(true, waitStopwatch.Elapsed.TotalMilliseconds, lastError);
                    }
                }
                else
                {
                    stableCounter = 0;
                }

                WaitForPollInterval(options, token);
            }

            return new WaitPhaseResult(false, waitStopwatch.Elapsed.TotalMilliseconds, lastError);
        }

        private static void AppendPositionReadSample(
            CycleTestMetrics metrics,
            int cycleIndex,
            string phase,
            double targetPosition,
            double actualPosition,
            double positionError,
            bool inTolerance,
            long readStartTick,
            long readEndTick,
            double readLatencyMs)
        {
            metrics.PositionReadSampleCounter++;

            if (metrics.PositionReadSamples.Count >= MaxPositionReadSamplesToSave)
            {
                metrics.PositionReadSamplesDropped++;
                return;
            }

            var sample = new PositionReadSample
            {
                SampleIndex = metrics.PositionReadSampleCounter,
                CycleIndex = cycleIndex,
                Phase = phase ?? "-",
                TargetPosition = targetPosition,
                ActualPosition = actualPosition,
                PositionError = positionError,
                InTolerance = inTolerance,
                ReadStartFromTestMs = (readStartTick - metrics.TestStartedTick) * 1000.0 / Stopwatch.Frequency,
                ReadEndFromTestMs = (readEndTick - metrics.TestStartedTick) * 1000.0 / Stopwatch.Frequency,
                ReadLatencyMs = readLatencyMs
            };

            metrics.PositionReadSamples.Add(sample);
        }

        private static void WaitForPollInterval(CycleTestOptions options, CancellationToken token)
        {
            WaitForPollInterval(options.PollIntervalMs, options.UseHighPrecisionWait, options.Request1msTimerResolution, token);
        }

        private static void WaitForPollInterval(
            int pollIntervalMs,
            bool useHighPrecisionWait,
            bool request1msTimerResolution,
            CancellationToken token)
        {
            if (!useHighPrecisionWait)
            {
                Thread.Sleep(pollIntervalMs);
                return;
            }

            var targetTick = Stopwatch.GetTimestamp() + (long)(pollIntervalMs * Stopwatch.Frequency / 1000.0);
            while (true)
            {
                token.ThrowIfCancellationRequested();

                var now = Stopwatch.GetTimestamp();
                var remainingTicks = targetTick - now;
                if (remainingTicks <= 0)
                {
                    break;
                }

                var remainingMs = remainingTicks * 1000.0 / Stopwatch.Frequency;
                if (remainingMs > 2.0 && request1msTimerResolution)
                {
                    var sleepMs = Math.Max(1, (int)(remainingMs - 1.0));
                    Thread.Sleep(sleepMs);
                    continue;
                }

                if (remainingMs > 0.5)
                {
                    Thread.Sleep(0);
                    continue;
                }

                Thread.SpinWait(200);
            }
        }

        private void ToggleCycleTestControls(bool running)
        {
            ButtonStartCycleTest.IsEnabled = !running;
            ButtonStopCycleTest.IsEnabled = running;
            ButtonSaveCycleResult.IsEnabled = !running && _lastCycleTestSnapshot != null;
        }

        private void ResetCycleTestOutput()
        {
            UpdateCycleTestOutput("Idle", "No result yet.", 0.0);
        }

        private void SetCycleTestStatus(string statusText)
        {
            Action update = delegate
            {
                TextCycleRunStatus.Text = statusText;
            };

            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(update);
                return;
            }

            update();
        }

        private void UpdateCycleTestUi(CycleTestOptions options, CycleTestMetrics metrics)
        {
            var attemptedCycles = Math.Max(metrics.AttemptedCycles, 0);
            var progress = options.RequestedCycles <= 0
                ? 0.0
                : attemptedCycles * 100.0 / options.RequestedCycles;

            var status = string.Format(
                CultureInfo.InvariantCulture,
                "{0} / {1} cycles attempted, {2} successful, stop reason: {3}",
                attemptedCycles,
                options.RequestedCycles,
                metrics.SuccessfulCycles,
                metrics.StopReason);

            var summary = BuildCycleTestSummary(options, metrics);
            UpdateCycleTestOutput(status, summary, progress);
        }

        private void UpdateCycleTestOutput(string status, string summary, double progress)
        {
            Action update = delegate
            {
                TextCycleRunStatus.Text = status;
                TextCycleSummary.Text = summary;
                ProgressCycleTest.Value = Math.Max(0.0, Math.Min(100.0, progress));
            };

            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(update);
                return;
            }

            update();
        }

        private static string BuildCycleTestSummary(CycleTestOptions options, CycleTestMetrics metrics)
        {
            var builder = new StringBuilder();
            builder.AppendLine("=== Motion Profile Cycle Test Summary ===");
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "Profile: Base={0} -> Forward={1} -> Return={2} (MoveDistance={3} mm)",
                options.BasePosition,
                options.ForwardPosition,
                options.ReturnPosition,
                options.MoveDistanceMm));
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "Dynamics(manual): Velocity={0:F3}, Acc={1:F3}, Dec={2:F3}, Jerk={3:F3}",
                options.Velocity,
                options.Acceleration,
                options.Deceleration,
                options.Jerk));
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "Timing mode: highPriority={0}, highPrecisionWait={1}, timer1ms={2}",
                options.UseHighPriorityWorkerThread,
                options.UseHighPrecisionWait,
                options.Request1msTimerResolution));
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "Warm-up cycles (excluded): {0}",
                options.WarmupCycles));
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "Cycles: attempted={0}, successful={1}, target={2}",
                metrics.AttemptedCycles,
                metrics.SuccessfulCycles,
                options.RequestedCycles));
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "Elapsed: total={0:F1} ms",
                metrics.TotalElapsedMs));
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "Cycle time: avg={0:F3} ms, max={1:F3} ms",
                metrics.CycleTimeMs.Average,
                metrics.CycleTimeMs.Max));
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "Response latency(GetActualPosition): avg={0:F3} ms, max={1:F3} ms, samples={2}",
                metrics.ResponseLatencyMs.Average,
                metrics.ResponseLatencyMs.Max,
                metrics.ResponseLatencyMs.Count));
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "GetActualPosition samples: captured={0}, droppedByLimit={1}, limit={2}",
                metrics.PositionReadSamples.Count,
                metrics.PositionReadSamplesDropped,
                MaxPositionReadSamplesToSave));
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "Poll period: avg={0:F3} ms, max={1:F3} ms, drop(th>{2}ms)={3}",
                metrics.PollPeriodMs.Average,
                metrics.PollPeriodMs.Max,
                options.DropThresholdMs,
                metrics.DropCount));
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "In-position settle: forward avg={0:F3} ms, return avg={1:F3} ms, max error={2:F6}",
                metrics.ForwardSettleMs.Average,
                metrics.ReturnSettleMs.Average,
                metrics.MaxInPositionError));
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "Timeouts/Error: forwardTimeout={0}, returnTimeout={1}, axisError={2}, exception={3}",
                metrics.ForwardTimeouts,
                metrics.ReturnTimeouts,
                metrics.AxisErrorCount,
                metrics.ExceptionCount));
            if (!string.IsNullOrWhiteSpace(metrics.LastError))
            {
                builder.AppendLine("Last error: " + metrics.LastError);
            }

            return builder.ToString().TrimEnd();
        }

        private void ToggleCycleTest2Controls(bool running)
        {
            ButtonStartCycleTest2.IsEnabled = !running;
            ButtonStopCycleTest2.IsEnabled = running;
            ButtonSaveCycleResult2.IsEnabled = !running && _lastCycleTest2Snapshot != null;
        }

        private void ResetCycleTest2Output()
        {
            UpdateCycleTest2Output("Idle", "No result yet.", 0.0);
        }

        private void SetCycleTest2Status(string statusText)
        {
            Action update = delegate
            {
                TextCycle2RunStatus.Text = statusText;
            };

            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(update);
                return;
            }

            update();
        }

        private void UpdateCycleTest2Ui(CycleTestOptions options, CycleTestMetrics metrics)
        {
            var attemptedCycles = Math.Max(metrics.AttemptedCycles, 0);
            var progress = options.RequestedCycles <= 0
                ? 0.0
                : attemptedCycles * 100.0 / options.RequestedCycles;

            var status = string.Format(
                CultureInfo.InvariantCulture,
                "{0} / {1} cycles attempted, {2} successful, stop reason: {3}",
                attemptedCycles,
                options.RequestedCycles,
                metrics.SuccessfulCycles,
                metrics.StopReason);

            var summary = BuildCycleTest2Summary(options, metrics);
            UpdateCycleTest2Output(status, summary, progress);
        }

        private void UpdateCycleTest2Output(string status, string summary, double progress)
        {
            Action update = delegate
            {
                TextCycle2RunStatus.Text = status;
                TextCycle2Summary.Text = summary;
                ProgressCycleTest2.Value = Math.Max(0.0, Math.Min(100.0, progress));
            };

            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(update);
                return;
            }

            update();
        }

        private static string BuildCycleTest2Summary(CycleTestOptions options, CycleTestMetrics metrics)
        {
            var builder = new StringBuilder();
            builder.AppendLine("=== Motion Profile Cycle Test2 Summary ===");
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "Profile: Base={0} <-> Forward={1} (MoveDistance={2} mm)",
                options.BasePosition,
                options.ForwardPosition,
                options.MoveDistanceMm));
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "Mode: No in-position wait, continuous MoveAbsoluteEx issue (interval={0} ms)",
                options.PollIntervalMs));
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "Dynamics(manual): Velocity={0:F3}, Acc={1:F3}, Dec={2:F3}, Jerk={3:F3}",
                options.Velocity,
                options.Acceleration,
                options.Deceleration,
                options.Jerk));
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "Timing mode: highPriority={0}, highPrecisionWait={1}, timer1ms={2}",
                options.UseHighPriorityWorkerThread,
                options.UseHighPrecisionWait,
                options.Request1msTimerResolution));
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "Warm-up cycles (excluded): {0}",
                options.WarmupCycles));
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "Cycles: attempted={0}, successful={1}, target={2}",
                metrics.AttemptedCycles,
                metrics.SuccessfulCycles,
                options.RequestedCycles));
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "Elapsed: total={0:F1} ms",
                metrics.TotalElapsedMs));
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "Cycle time: avg={0:F3} ms, max={1:F3} ms",
                metrics.CycleTimeMs.Average,
                metrics.CycleTimeMs.Max));
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "Command latency(MoveAbsoluteEx): avg={0:F3} ms, max={1:F3} ms, samples={2}",
                metrics.ResponseLatencyMs.Average,
                metrics.ResponseLatencyMs.Max,
                metrics.ResponseLatencyMs.Count));
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "Command period: avg={0:F3} ms, max={1:F3} ms, drop(th>{2}ms)={3}",
                metrics.PollPeriodMs.Average,
                metrics.PollPeriodMs.Max,
                options.DropThresholdMs,
                metrics.DropCount));
            builder.AppendLine("In-position check: skipped by design.");
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "Error: axisError={0}, exception={1}",
                metrics.AxisErrorCount,
                metrics.ExceptionCount));
            if (!string.IsNullOrWhiteSpace(metrics.LastError))
            {
                builder.AppendLine("Last error: " + metrics.LastError);
            }

            return builder.ToString().TrimEnd();
        }

        private void StoreLastCycleTestSnapshot(CycleTestOptions options, CycleTestMetrics metrics)
        {
            _lastCycleTestSnapshot = new CycleTestSnapshot
            {
                CompletedAt = DateTime.Now,
                AxisName = Context.AxisName ?? "-",
                RemoteIp = Context.RemoteIp ?? "-",
                Options = options,
                Metrics = metrics,
                SummaryText = BuildCycleTestSummary(options, metrics)
            };
        }

        private void StoreLastCycleTest2Snapshot(CycleTestOptions options, CycleTestMetrics metrics)
        {
            _lastCycleTest2Snapshot = new CycleTestSnapshot
            {
                CompletedAt = DateTime.Now,
                AxisName = Context.AxisName ?? "-",
                RemoteIp = Context.RemoteIp ?? "-",
                Options = options,
                Metrics = metrics,
                SummaryText = BuildCycleTest2Summary(options, metrics)
            };
        }

        private string SaveLastCycleResultToExcel()
        {
            if (_lastCycleTestSnapshot == null)
            {
                throw new InvalidOperationException("No completed cycle test result to save.");
            }

            var folderPath = NormalizeNumeric(TextCycleSaveFolder.Text);
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                throw new InvalidOperationException("Save folder path is empty.");
            }

            Directory.CreateDirectory(folderPath);

            var fileName = string.Format(
                CultureInfo.InvariantCulture,
                "CycleTestResult_{0:yyyyMMdd_HHmmss}.xlsx",
                _lastCycleTestSnapshot.CompletedAt);
            var filePath = Path.Combine(folderPath, fileName);

            var sheets = new List<XlsxSheetData>
            {
                new XlsxSheetData("Result", BuildCycleResultSheetRows(_lastCycleTestSnapshot)),
                new XlsxSheetData("PositionSamples", BuildPositionReadSampleRows(_lastCycleTestSnapshot)),
                new XlsxSheetData("ExecutionLog", BuildExecutionLogSheetRows(Context.Logs.ToList()))
            };

            SimpleXlsxExporter.Save(filePath, sheets);
            return filePath;
        }

        private string SaveLastCycleResult2ToExcel()
        {
            if (_lastCycleTest2Snapshot == null)
            {
                throw new InvalidOperationException("No completed cycle test2 result to save.");
            }

            var folderPath = NormalizeNumeric(TextCycle2SaveFolder.Text);
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                throw new InvalidOperationException("Save folder path is empty.");
            }

            Directory.CreateDirectory(folderPath);

            var fileName = string.Format(
                CultureInfo.InvariantCulture,
                "CycleTest2Result_{0:yyyyMMdd_HHmmss}.xlsx",
                _lastCycleTest2Snapshot.CompletedAt);
            var filePath = Path.Combine(folderPath, fileName);

            var sheets = new List<XlsxSheetData>
            {
                new XlsxSheetData("Result", BuildCycleResult2SheetRows(_lastCycleTest2Snapshot)),
                new XlsxSheetData("ExecutionLog", BuildExecutionLogSheetRows(Context.Logs.ToList()))
            };

            SimpleXlsxExporter.Save(filePath, sheets);
            return filePath;
        }

        private static IList<IList<string>> BuildCycleResultSheetRows(CycleTestSnapshot snapshot)
        {
            var rows = new List<IList<string>>();
            rows.Add(new List<string> { "Field", "Value" });
            rows.Add(new List<string> { "CompletedAt", snapshot.CompletedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "AxisName", snapshot.AxisName });
            rows.Add(new List<string> { "RemoteIp", snapshot.RemoteIp });
            rows.Add(new List<string> { "RequestedCycles", snapshot.Options.RequestedCycles.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "WarmupCycles", snapshot.Options.WarmupCycles.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "AttemptedCycles", snapshot.Metrics.AttemptedCycles.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "SuccessfulCycles", snapshot.Metrics.SuccessfulCycles.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "StopReason", snapshot.Metrics.StopReason ?? "-" });
            rows.Add(new List<string> { "BasePosition(mm)", snapshot.Options.BasePosition.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "MoveDistance(mm)", snapshot.Options.MoveDistanceMm.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "ForwardPosition(mm)", snapshot.Options.ForwardPosition.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "ReturnPosition(mm)", snapshot.Options.ReturnPosition.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "Velocity(mm/s)", snapshot.Options.Velocity.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "Acceleration(mm/s^2)", snapshot.Options.Acceleration.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "Deceleration(mm/s^2)", snapshot.Options.Deceleration.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "Jerk(mm/s^3)", snapshot.Options.Jerk.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "InPositionTolerance(mm)", snapshot.Options.InPositionTolerance.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "MoveTimeout(ms)", snapshot.Options.MoveTimeoutMs.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "PollInterval(ms)", snapshot.Options.PollIntervalMs.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "StableSamples", snapshot.Options.StableSamplesRequired.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "DropThreshold(ms)", snapshot.Options.DropThresholdMs.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "HighPriorityWorker", snapshot.Options.UseHighPriorityWorkerThread.ToString() });
            rows.Add(new List<string> { "HighPrecisionWait", snapshot.Options.UseHighPrecisionWait.ToString() });
            rows.Add(new List<string> { "TimerResolution1ms", snapshot.Options.Request1msTimerResolution.ToString() });
            rows.Add(new List<string> { "CycleTimeAvg(ms)", snapshot.Metrics.CycleTimeMs.Average.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "CycleTimeMax(ms)", snapshot.Metrics.CycleTimeMs.Max.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "ResponseLatencyAvg(ms)", snapshot.Metrics.ResponseLatencyMs.Average.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "ResponseLatencyMax(ms)", snapshot.Metrics.ResponseLatencyMs.Max.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "PollPeriodAvg(ms)", snapshot.Metrics.PollPeriodMs.Average.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "PollPeriodMax(ms)", snapshot.Metrics.PollPeriodMs.Max.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "DropCount", snapshot.Metrics.DropCount.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "ForwardSettleAvg(ms)", snapshot.Metrics.ForwardSettleMs.Average.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "ReturnSettleAvg(ms)", snapshot.Metrics.ReturnSettleMs.Average.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "MaxInPositionError(mm)", snapshot.Metrics.MaxInPositionError.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "ForwardTimeouts", snapshot.Metrics.ForwardTimeouts.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "ReturnTimeouts", snapshot.Metrics.ReturnTimeouts.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "AxisErrorCount", snapshot.Metrics.AxisErrorCount.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "ExceptionCount", snapshot.Metrics.ExceptionCount.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "LastError", snapshot.Metrics.LastError ?? "-" });
            rows.Add(new List<string> { "SummaryText", snapshot.SummaryText ?? "-" });
            return rows;
        }

        private static IList<IList<string>> BuildCycleResult2SheetRows(CycleTestSnapshot snapshot)
        {
            var rows = new List<IList<string>>();
            rows.Add(new List<string> { "Field", "Value" });
            rows.Add(new List<string> { "CompletedAt", snapshot.CompletedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "AxisName", snapshot.AxisName });
            rows.Add(new List<string> { "RemoteIp", snapshot.RemoteIp });
            rows.Add(new List<string> { "RequestedCycles", snapshot.Options.RequestedCycles.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "WarmupCycles", snapshot.Options.WarmupCycles.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "AttemptedCycles", snapshot.Metrics.AttemptedCycles.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "SuccessfulCycles", snapshot.Metrics.SuccessfulCycles.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "StopReason", snapshot.Metrics.StopReason ?? "-" });
            rows.Add(new List<string> { "BasePosition(mm)", snapshot.Options.BasePosition.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "MoveDistance(mm)", snapshot.Options.MoveDistanceMm.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "ForwardPosition(mm)", snapshot.Options.ForwardPosition.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "ReturnPosition(mm)", snapshot.Options.ReturnPosition.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "Velocity(mm/s)", snapshot.Options.Velocity.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "Acceleration(mm/s^2)", snapshot.Options.Acceleration.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "Deceleration(mm/s^2)", snapshot.Options.Deceleration.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "Jerk(mm/s^3)", snapshot.Options.Jerk.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "CommandInterval(ms)", snapshot.Options.PollIntervalMs.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "DropThreshold(ms)", snapshot.Options.DropThresholdMs.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "HighPriorityWorker", snapshot.Options.UseHighPriorityWorkerThread.ToString() });
            rows.Add(new List<string> { "HighPrecisionWait", snapshot.Options.UseHighPrecisionWait.ToString() });
            rows.Add(new List<string> { "TimerResolution1ms", snapshot.Options.Request1msTimerResolution.ToString() });
            rows.Add(new List<string> { "CycleTimeAvg(ms)", snapshot.Metrics.CycleTimeMs.Average.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "CycleTimeMax(ms)", snapshot.Metrics.CycleTimeMs.Max.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "CommandLatencyAvg(ms)", snapshot.Metrics.ResponseLatencyMs.Average.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "CommandLatencyMax(ms)", snapshot.Metrics.ResponseLatencyMs.Max.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "CommandPeriodAvg(ms)", snapshot.Metrics.PollPeriodMs.Average.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "CommandPeriodMax(ms)", snapshot.Metrics.PollPeriodMs.Max.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "DropCount", snapshot.Metrics.DropCount.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "AxisErrorCount", snapshot.Metrics.AxisErrorCount.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "ExceptionCount", snapshot.Metrics.ExceptionCount.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "LastError", snapshot.Metrics.LastError ?? "-" });
            rows.Add(new List<string> { "SummaryText", snapshot.SummaryText ?? "-" });
            return rows;
        }

        private static IList<IList<string>> BuildPositionReadSampleRows(CycleTestSnapshot snapshot)
        {
            var rows = new List<IList<string>>();
            rows.Add(new List<string> { "Field", "Value" });
            rows.Add(new List<string> { "CapturedSamples", snapshot.Metrics.PositionReadSamples.Count.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "DroppedByLimit", snapshot.Metrics.PositionReadSamplesDropped.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "CaptureLimit", MaxPositionReadSamplesToSave.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "TestStartedAt", snapshot.Metrics.TestStartedAt.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { string.Empty, string.Empty });

            rows.Add(new List<string>
            {
                "SampleIndex",
                "CycleIndex",
                "Phase",
                "TargetPosition(mm)",
                "ActualPosition(mm)",
                "PositionError(mm)",
                "InTolerance",
                "ReadStartFromTest(ms)",
                "ReadEndFromTest(ms)",
                "ReadLatency(ms)"
            });

            foreach (var sample in snapshot.Metrics.PositionReadSamples)
            {
                rows.Add(new List<string>
                {
                    sample.SampleIndex.ToString(CultureInfo.InvariantCulture),
                    sample.CycleIndex.ToString(CultureInfo.InvariantCulture),
                    sample.Phase ?? "-",
                    sample.TargetPosition.ToString("F6", CultureInfo.InvariantCulture),
                    sample.ActualPosition.ToString("F6", CultureInfo.InvariantCulture),
                    sample.PositionError.ToString("F6", CultureInfo.InvariantCulture),
                    sample.InTolerance.ToString(),
                    sample.ReadStartFromTestMs.ToString("F6", CultureInfo.InvariantCulture),
                    sample.ReadEndFromTestMs.ToString("F6", CultureInfo.InvariantCulture),
                    sample.ReadLatencyMs.ToString("F6", CultureInfo.InvariantCulture)
                });
            }

            return rows;
        }

        private static IList<IList<string>> BuildExecutionLogSheetRows(IList<string> logs)
        {
            var rows = new List<IList<string>>();
            rows.Add(new List<string> { "No", "Log" });
            if (logs == null)
            {
                return rows;
            }

            for (var i = 0; i < logs.Count; i++)
            {
                rows.Add(new List<string>
                {
                    (i + 1).ToString(CultureInfo.InvariantCulture),
                    logs[i] ?? string.Empty
                });
            }

            return rows;
        }

        private sealed class HighResolutionTimerScope : IDisposable
        {
            private readonly uint _periodMs;
            private readonly bool _enabled;
            private readonly PmasControllerContext _context;

            private HighResolutionTimerScope(uint periodMs, bool enabled, PmasControllerContext context)
            {
                _periodMs = periodMs;
                _enabled = enabled;
                _context = context;
            }

            public static HighResolutionTimerScope TryCreate(uint periodMs, PmasControllerContext context)
            {
                var result = timeBeginPeriod(periodMs);
                if (result == 0)
                {
                    if (context != null)
                    {
                        context.Log("CycleTest timer resolution request applied: " + periodMs.ToString(CultureInfo.InvariantCulture) + "ms");
                    }

                    return new HighResolutionTimerScope(periodMs, true, context);
                }

                if (context != null)
                {
                    context.Log("CycleTest timer resolution request failed: code=" + result.ToString(CultureInfo.InvariantCulture));
                }

                return new HighResolutionTimerScope(periodMs, false, context);
            }

            public void Dispose()
            {
                if (!_enabled)
                {
                    return;
                }

                var result = timeEndPeriod(_periodMs);
                if (result != 0 && _context != null)
                {
                    _context.Log("CycleTest timer resolution release failed: code=" + result.ToString(CultureInfo.InvariantCulture));
                }
            }

            [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
            private static extern uint timeBeginPeriod(uint uPeriod);

            [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
            private static extern uint timeEndPeriod(uint uPeriod);
        }
    }
}
