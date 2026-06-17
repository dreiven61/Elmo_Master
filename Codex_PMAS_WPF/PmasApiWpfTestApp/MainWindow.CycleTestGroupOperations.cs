using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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
        private const uint DefaultGroupInPositionMask = 0x00020000;
        private CancellationTokenSource _cycleTestGroup1Cancellation;
        private bool _isCycleTestGroup1Running;
        private CycleGroupTestSnapshot _lastCycleTestGroup1Snapshot;

        private sealed class CycleGroupTestOptions
        {
            public int RequestedCycles { get; set; }
            public int WarmupCycles { get; set; }
            public double[][] Points { get; set; }
            public string[] PointNames { get; set; }
            public double Velocity { get; set; }
            public double Acceleration { get; set; }
            public double Deceleration { get; set; }
            public double Jerk { get; set; }
            public MC_BUFFERED_MODE_ENUM BufferedMode { get; set; }
            public MC_COORD_SYSTEM_ENUM CoordSystem { get; set; }
            public NC_TRANSITION_MODE_ENUM TransitionMode { get; set; }
            public double[] TransitionParams { get; set; }
            public byte Superimposed { get; set; }
            public int MoveTimeoutMs { get; set; }
            public int PollIntervalMs { get; set; }
            public int StableSamplesRequired { get; set; }
            public int DropThresholdMs { get; set; }
            public uint InPositionMask { get; set; }
            public bool StopOnTimeout { get; set; }
            public bool StopOnGroupError { get; set; }
            public bool UseHighPriorityWorkerThread { get; set; }
            public bool UseHighPrecisionWait { get; set; }
            public bool Request1msTimerResolution { get; set; }
            public bool QueueBlendCommands { get; set; }
        }

        private sealed class GroupStatusReadSample
        {
            public long SampleIndex { get; set; }
            public int CycleIndex { get; set; }
            public string Phase { get; set; }
            public ushort GroupErrorId { get; set; }
            public uint GroupStatus { get; set; }
            public bool InPosition { get; set; }
            public int StableCounter { get; set; }
            public double ReadStartFromTestMs { get; set; }
            public double ReadEndFromTestMs { get; set; }
            public double ReadLatencyMs { get; set; }
        }

        private sealed class CycleGroupTestMetrics
        {
            public CycleGroupTestMetrics()
            {
                CycleTimeMs = new RunningMetric();
                CommandLatencyMs = new RunningMetric();
                ResponseLatencyMs = new RunningMetric();
                PollPeriodMs = new RunningMetric();
                PointSettleMs = new RunningMetric();
                GroupStatusReadSamples = new List<GroupStatusReadSample>(MaxStatusReadSamplesToSave);
                StopReason = "Completed";
            }

            public int AttemptedCycles { get; set; }
            public int SuccessfulCycles { get; set; }
            public int TimeoutCount { get; set; }
            public int GroupErrorCount { get; set; }
            public int ExceptionCount { get; set; }
            public int DropCount { get; set; }
            public double TotalElapsedMs { get; set; }
            public string LastError { get; set; }
            public string StopReason { get; set; }
            public DateTime TestStartedAt { get; set; }
            public long TestStartedTick { get; set; }
            public long GroupStatusReadSampleCounter { get; set; }
            public int GroupStatusReadSamplesDropped { get; set; }
            public RunningMetric CycleTimeMs { get; private set; }
            public RunningMetric CommandLatencyMs { get; private set; }
            public RunningMetric ResponseLatencyMs { get; private set; }
            public RunningMetric PollPeriodMs { get; private set; }
            public RunningMetric PointSettleMs { get; private set; }
            public IList<GroupStatusReadSample> GroupStatusReadSamples { get; private set; }
        }

        private sealed class CycleGroupTestSnapshot
        {
            public DateTime CompletedAt { get; set; }
            public string GroupName { get; set; }
            public string RemoteIp { get; set; }
            public CycleGroupTestOptions Options { get; set; }
            public CycleGroupTestMetrics Metrics { get; set; }
            public string SummaryText { get; set; }
        }

        private async void ButtonStartCycleTestGroup1_Click(object sender, RoutedEventArgs e)
        {
            if (_isCycleTestGroup1Running)
            {
                return;
            }

            if (_isCycleTestRunning || _isCycleTest2Running || _isCycleTest3Running || _isCycleTest4Running)
            {
                MessageBox.Show("Another Cycle Test is running. Stop it first.", "Cycle Test Group1", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                EnsureGroupLoadedFromText();

                var options = BuildCycleGroup1Options();

                _cycleTestGroup1Cancellation = new CancellationTokenSource();
                _isCycleTestGroup1Running = true;
                ToggleCycleTestGroup1Controls(true);
                ResetCycleTestGroup1Output();
                SetCycleTestGroup1Status(string.Format(
                    CultureInfo.InvariantCulture,
                    "Running... Cycles={0}, Warmup={1}, Transition={2}, Buffered={3}, Mask=0x{4:X8}",
                    options.RequestedCycles,
                    options.WarmupCycles,
                    options.TransitionMode,
                    options.BufferedMode,
                    options.InPositionMask));

                Context.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "CycleTestGroup1 started: group={0}, cycles={1}, warmup={2}, vel={3}, acc={4}, dec={5}, jerk={6}, buffered={7}, coord={8}, transition={9}, transitionParams={10}, superimposed={11}, inPosMask=0x{12:X8}, timeoutMs={13}, pollMs={14}, stable={15}, queueBlend={16}",
                    Context.GroupName,
                    options.RequestedCycles,
                    options.WarmupCycles,
                    options.Velocity,
                    options.Acceleration,
                    options.Deceleration,
                    options.Jerk,
                    options.BufferedMode,
                    options.CoordSystem,
                    options.TransitionMode,
                    FormatVector(options.TransitionParams),
                    options.Superimposed,
                    options.InPositionMask,
                    options.MoveTimeoutMs,
                    options.PollIntervalMs,
                    options.StableSamplesRequired,
                    options.QueueBlendCommands));

                var metrics = await Task.Factory.StartNew(
                    () => ExecuteCycleGroup1Test(options, _cycleTestGroup1Cancellation.Token),
                    _cycleTestGroup1Cancellation.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);
                UpdateCycleTestGroup1Ui(options, metrics);
                Context.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "CycleTestGroup1 finished: attempted={0}, success={1}, timeouts={2}, groupErrors={3}, exceptions={4}, drops={5}, statusReads={6}",
                    metrics.AttemptedCycles,
                    metrics.SuccessfulCycles,
                    metrics.TimeoutCount,
                    metrics.GroupErrorCount,
                    metrics.ExceptionCount,
                    metrics.DropCount,
                    metrics.GroupStatusReadSampleCounter));

                StoreLastCycleGroup1Snapshot(options, metrics);
                ButtonSaveCycleGroup1Result.IsEnabled = true;

                if (CheckCycleGroup1AutoSaveResult.IsChecked == true)
                {
                    var savedPath = SaveLastCycleGroup1ResultToExcel();
                    Context.Log("CycleTestGroup1 result saved: " + savedPath);
                    SetCycleTestGroup1Status("Completed. Result saved: " + savedPath);
                }
            }
            catch (OperationCanceledException)
            {
                SetCycleTestGroup1Status("Canceled by user.");
                Context.Log("CycleTestGroup1 canceled by user.");
            }
            catch (Exception ex)
            {
                SetCycleTestGroup1Status("Failed: " + ex.Message);
                Context.Log("CycleTestGroup1 failed: " + ex.Message);
                MessageBox.Show(ex.ToString(), "Cycle Test Group1", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isCycleTestGroup1Running = false;
                ToggleCycleTestGroup1Controls(false);
                if (_cycleTestGroup1Cancellation != null)
                {
                    _cycleTestGroup1Cancellation.Dispose();
                    _cycleTestGroup1Cancellation = null;
                }
            }
        }

        private void ButtonStopCycleTestGroup1_Click(object sender, RoutedEventArgs e)
        {
            if (!_isCycleTestGroup1Running || _cycleTestGroup1Cancellation == null)
            {
                return;
            }

            _cycleTestGroup1Cancellation.Cancel();
            SetCycleTestGroup1Status("Stop requested...");
            Context.Log("CycleTestGroup1 stop requested.");

            try
            {
                Context.EnsureGroup();
                Context.GroupAxis.GroupStop(
                    ParseSingle(TextCycleGroup1Deceleration.Text),
                    ParseSingle(TextCycleGroup1Jerk.Text),
                    (MC_BUFFERED_MODE_ENUM)ComboCycleGroup1BufferedMode.SelectedItem);
            }
            catch (Exception ex)
            {
                Context.Log("CycleTestGroup1 stop command failed: " + ex.Message);
            }
        }

        private void ButtonSaveCycleGroup1Result_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var savedPath = SaveLastCycleGroup1ResultToExcel();
                Context.Log("CycleTestGroup1 result saved: " + savedPath);
                SetCycleTestGroup1Status("Result saved: " + savedPath);
            }
            catch (Exception ex)
            {
                Context.Log("CycleTestGroup1 result save failed: " + ex.Message);
                MessageBox.Show(ex.Message, "Save Cycle Test Group1 Result", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private CycleGroupTestOptions BuildCycleGroup1Options()
        {
            var options = new CycleGroupTestOptions();
            options.PointNames = new[] { "P1", "P2", "P3", "P4", "P1" };
            var p1 = ParseDoubleArray(TextCycleGroup1P1.Text, 16);
            var p2 = ParseDoubleArray(TextCycleGroup1P2.Text, 16);
            var p3 = ParseDoubleArray(TextCycleGroup1P3.Text, 16);
            var p4 = ParseDoubleArray(TextCycleGroup1P4.Text, 16);
            options.Points = new[] { p1, p2, p3, p4, p1 };
            options.Velocity = ParseDouble(TextCycleGroup1Velocity.Text);
            options.Acceleration = ParseDouble(TextCycleGroup1Acceleration.Text);
            options.Deceleration = ParseDouble(TextCycleGroup1Deceleration.Text);
            options.Jerk = ParseDouble(TextCycleGroup1Jerk.Text);
            options.BufferedMode = (MC_BUFFERED_MODE_ENUM)ComboCycleGroup1BufferedMode.SelectedItem;
            options.CoordSystem = (MC_COORD_SYSTEM_ENUM)ComboCycleGroup1CoordSystem.SelectedItem;
            options.TransitionMode = (NC_TRANSITION_MODE_ENUM)ComboCycleGroup1TransitionMode.SelectedItem;
            options.TransitionParams = ParseDoubleArray(TextCycleGroup1TransitionParams.Text, 16);
            options.Superimposed = ParseByte(TextCycleGroup1Superimposed.Text);
            options.RequestedCycles = ParseInt32(TextCycleGroup1Count.Text);
            options.WarmupCycles = ParseInt32(TextCycleGroup1WarmupCycles.Text);
            options.MoveTimeoutMs = ParseInt32(TextCycleGroup1MoveTimeoutMs.Text);
            options.PollIntervalMs = ParseInt32(TextCycleGroup1PollIntervalMs.Text);
            options.StableSamplesRequired = ParseInt32(TextCycleGroup1StableSamples.Text);
            options.DropThresholdMs = ParseInt32(TextCycleGroup1DropThresholdMs.Text);
            var inPositionMaskText = NormalizeNumeric(TextCycleGroup1InPositionMask.Text);
            options.InPositionMask = string.IsNullOrWhiteSpace(inPositionMaskText)
                ? DefaultGroupInPositionMask
                : ParseUInt32(inPositionMaskText);
            options.StopOnTimeout = CheckCycleGroup1StopOnTimeout.IsChecked == true;
            options.StopOnGroupError = CheckCycleGroup1StopOnError.IsChecked == true;
            options.UseHighPriorityWorkerThread = CheckCycleGroup1HighPriorityThread.IsChecked == true;
            options.UseHighPrecisionWait = CheckCycleGroup1HighPrecisionWait.IsChecked == true;
            options.Request1msTimerResolution = CheckCycleGroup1Use1msTimerResolution.IsChecked == true;
            options.QueueBlendCommands = CheckCycleGroup1QueueBlend.IsChecked == true;

            if (options.RequestedCycles <= 0)
            {
                throw new InvalidOperationException("Cycle Count must be > 0.");
            }

            if (options.WarmupCycles < 0)
            {
                throw new InvalidOperationException("Warmup Cycles must be >= 0.");
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

            if (options.InPositionMask == 0)
            {
                throw new InvalidOperationException("Group In-position Mask must be non-zero.");
            }

            return options;
        }

        private CycleGroupTestMetrics ExecuteCycleGroup1Test(CycleGroupTestOptions options, CancellationToken token)
        {
            var metrics = new CycleGroupTestMetrics();
            metrics.TestStartedAt = DateTime.Now;
            metrics.TestStartedTick = Stopwatch.GetTimestamp();

            var currentThread = Thread.CurrentThread;
            var previousPriority = currentThread.Priority;
            if (options.UseHighPriorityWorkerThread)
            {
                currentThread.Priority = ThreadPriority.Highest;
            }

            var totalStopwatch = Stopwatch.StartNew();
            var totalCycles = options.WarmupCycles + options.RequestedCycles;

            try
            {
                using (var timerScope = options.Request1msTimerResolution
                    ? HighResolutionTimerScope.TryCreate(1, Context)
                    : null)
                {
                    for (var cycle = 1; cycle <= totalCycles; cycle++)
                    {
                        token.ThrowIfCancellationRequested();

                        var measuring = cycle > options.WarmupCycles;
                        var measuredCycleIndex = measuring ? cycle - options.WarmupCycles : 0;
                        var cyclePassed = true;
                        var cycleStopwatch = Stopwatch.StartNew();

                        if (measuring)
                        {
                            metrics.AttemptedCycles++;
                        }

                        try
                        {
                            if (options.QueueBlendCommands)
                            {
                                ExecuteQueuedBlendCycle(options, metrics, measuredCycleIndex, measuring, token);
                            }
                            else
                            {
                                ExecuteWaitEachPointCycle(options, metrics, measuredCycleIndex, measuring, token);
                            }
                        }
                        catch (TimeoutException ex)
                        {
                            cyclePassed = false;
                            if (measuring)
                            {
                                metrics.TimeoutCount++;
                            }

                            metrics.LastError = ex.Message;
                            if (string.IsNullOrWhiteSpace(metrics.StopReason) || metrics.StopReason == "Completed")
                            {
                                metrics.StopReason = "Stopped: timeout";
                            }

                            Context.Log("CycleTestGroup1 timeout: " + ex.Message);
                            if (options.StopOnTimeout)
                            {
                                break;
                            }
                        }
                        catch (MMCException ex)
                        {
                            cyclePassed = false;
                            if (measuring)
                            {
                                metrics.ExceptionCount++;
                            }

                            metrics.LastError = ex.Message;
                            if (string.IsNullOrWhiteSpace(metrics.StopReason) || metrics.StopReason == "Completed")
                            {
                                metrics.StopReason = "Stopped: MMCException";
                            }

                            Context.Log(string.Format(
                                CultureInfo.InvariantCulture,
                                "CycleTestGroup1: MMCException Command={0}, LibraryError={1}, MMCError={2}, Status={3}, AxisRef={4}",
                                ex.CommandID,
                                ex.LibraryError,
                                ex.MMCError,
                                ex.Status,
                                ex.AxisRef));

                            if (options.StopOnGroupError)
                            {
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            cyclePassed = false;
                            if (measuring)
                            {
                                metrics.ExceptionCount++;
                            }

                            metrics.LastError = ex.Message;
                            if (string.IsNullOrWhiteSpace(metrics.StopReason) || metrics.StopReason == "Completed")
                            {
                                metrics.StopReason = "Stopped: exception";
                            }

                            Context.Log("CycleTestGroup1: Exception " + ex.Message);
                            if (options.StopOnGroupError)
                            {
                                break;
                            }
                        }
                        finally
                        {
                            cycleStopwatch.Stop();
                            if (measuring)
                            {
                                metrics.CycleTimeMs.Add(cycleStopwatch.Elapsed.TotalMilliseconds);
                                if (cyclePassed)
                                {
                                    metrics.SuccessfulCycles++;
                                }
                            }
                        }

                        if (measuring && metrics.AttemptedCycles % 10 == 0)
                        {
                            UpdateCycleTestGroup1Ui(options, metrics);
                            SetCycleTestGroup1Status(string.Format(
                                CultureInfo.InvariantCulture,
                                "Running... {0}/{1} cycles, success={2}, statusReads={3}",
                                metrics.AttemptedCycles,
                                options.RequestedCycles,
                                metrics.SuccessfulCycles,
                                metrics.GroupStatusReadSampleCounter));
                        }
                    }
                }
            }
            finally
            {
                currentThread.Priority = previousPriority;
            }

            totalStopwatch.Stop();
            metrics.TotalElapsedMs = totalStopwatch.Elapsed.TotalMilliseconds;
            UpdateCycleTestGroup1Ui(options, metrics);
            return metrics;
        }

        private void ExecuteWaitEachPointCycle(
            CycleGroupTestOptions options,
            CycleGroupTestMetrics metrics,
            int cycleIndex,
            bool measuring,
            CancellationToken token)
        {
            for (var pointIndex = 0; pointIndex < options.Points.Length; pointIndex++)
            {
                token.ThrowIfCancellationRequested();

                var phase = options.PointNames[pointIndex];
                IssueGroupMoveLinearAbsolute(options, options.Points[pointIndex], metrics, measuring);
                var waitResult = WaitForGroupInPosition(options, metrics, cycleIndex, phase, measuring, token);
                if (!waitResult.Success)
                {
                    throw new TimeoutException(string.Format(
                        CultureInfo.InvariantCulture,
                        "Cycle={0}, phase={1}, elapsed={2:F3} ms did not reach GroupReadStatus mask 0x{3:X8}.",
                        cycleIndex,
                        phase,
                        waitResult.SettleMilliseconds,
                        options.InPositionMask));
                }

                if (measuring)
                {
                    metrics.PointSettleMs.Add(waitResult.SettleMilliseconds);
                }
            }
        }

        private void ExecuteQueuedBlendCycle(
            CycleGroupTestOptions options,
            CycleGroupTestMetrics metrics,
            int cycleIndex,
            bool measuring,
            CancellationToken token)
        {
            for (var pointIndex = 0; pointIndex < options.Points.Length; pointIndex++)
            {
                token.ThrowIfCancellationRequested();
                IssueGroupMoveLinearAbsolute(options, options.Points[pointIndex], metrics, measuring);
            }

            var waitResult = WaitForGroupInPosition(options, metrics, cycleIndex, "P1(final)", measuring, token);
            if (!waitResult.Success)
            {
                throw new TimeoutException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Cycle={0}, queued blend final P1 elapsed={1:F3} ms did not reach GroupReadStatus mask 0x{2:X8}.",
                    cycleIndex,
                    waitResult.SettleMilliseconds,
                    options.InPositionMask));
            }

            if (measuring)
            {
                metrics.PointSettleMs.Add(waitResult.SettleMilliseconds);
            }
        }

        private void IssueGroupMoveLinearAbsolute(
            CycleGroupTestOptions options,
            double[] point,
            CycleGroupTestMetrics metrics,
            bool measuring)
        {
            var commandStartTick = Stopwatch.GetTimestamp();
            Context.GroupAxis.MoveLinearAbsoluteEx(
                options.Velocity,
                options.Acceleration,
                options.Deceleration,
                options.Jerk,
                point,
                options.BufferedMode,
                options.CoordSystem,
                options.TransitionMode,
                options.TransitionParams,
                options.Superimposed,
                1);
            var commandEndTick = Stopwatch.GetTimestamp();

            if (measuring)
            {
                metrics.CommandLatencyMs.Add((commandEndTick - commandStartTick) * 1000.0 / Stopwatch.Frequency);
            }
        }

        private WaitPhaseResult WaitForGroupInPosition(
            CycleGroupTestOptions options,
            CycleGroupTestMetrics metrics,
            int cycleIndex,
            string phase,
            bool measuring,
            CancellationToken token)
        {
            var waitStopwatch = Stopwatch.StartNew();
            long previousTick = 0;
            int stableCounter = 0;

            while (waitStopwatch.ElapsedMilliseconds <= options.MoveTimeoutMs)
            {
                token.ThrowIfCancellationRequested();

                var nowTick = Stopwatch.GetTimestamp();
                if (previousTick != 0 && measuring)
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
                ushort groupErrorId = 0;
                var groupStatus = Context.GroupAxis.GroupReadStatus(ref groupErrorId);
                var readEndTick = Stopwatch.GetTimestamp();
                var readLatencyMs = (readEndTick - readStartTick) * 1000.0 / Stopwatch.Frequency;

                var inPosition = groupErrorId == 0 && (groupStatus & options.InPositionMask) == options.InPositionMask;
                stableCounter = inPosition ? stableCounter + 1 : 0;

                if (measuring)
                {
                    metrics.ResponseLatencyMs.Add(readLatencyMs);
                    AppendGroupStatusReadSample(
                        metrics,
                        cycleIndex,
                        phase,
                        groupErrorId,
                        groupStatus,
                        inPosition,
                        stableCounter,
                        readStartTick,
                        readEndTick,
                        readLatencyMs);
                }

                if (groupErrorId != 0)
                {
                    if (measuring)
                    {
                        metrics.GroupErrorCount++;
                    }

                    Context.Log(string.Format(
                        CultureInfo.InvariantCulture,
                        "CycleTestGroup1 {0} {1}: GroupReadStatus groupErrorId={2}, status=0x{3:X8}",
                        cycleIndex,
                        phase,
                        groupErrorId,
                        groupStatus));

                    if (options.StopOnGroupError)
                    {
                        throw new InvalidOperationException(string.Format(
                            CultureInfo.InvariantCulture,
                            "GroupReadStatus groupErrorId={0}, status=0x{1:X8}",
                            groupErrorId,
                            groupStatus));
                    }
                }

                if (inPosition)
                {
                    if (stableCounter >= options.StableSamplesRequired)
                    {
                        return new WaitPhaseResult(true, waitStopwatch.Elapsed.TotalMilliseconds, 0.0);
                    }
                }

                WaitForPollInterval(
                    options.PollIntervalMs,
                    options.UseHighPrecisionWait,
                    options.Request1msTimerResolution,
                    token);
            }

            return new WaitPhaseResult(false, waitStopwatch.Elapsed.TotalMilliseconds, 0.0);
        }

        private static void AppendGroupStatusReadSample(
            CycleGroupTestMetrics metrics,
            int cycleIndex,
            string phase,
            ushort groupErrorId,
            uint groupStatus,
            bool inPosition,
            int stableCounter,
            long readStartTick,
            long readEndTick,
            double readLatencyMs)
        {
            metrics.GroupStatusReadSampleCounter++;

            if (metrics.GroupStatusReadSamples.Count >= MaxStatusReadSamplesToSave)
            {
                metrics.GroupStatusReadSamplesDropped++;
                return;
            }

            var sample = new GroupStatusReadSample
            {
                SampleIndex = metrics.GroupStatusReadSampleCounter,
                CycleIndex = cycleIndex,
                Phase = phase ?? "-",
                GroupErrorId = groupErrorId,
                GroupStatus = groupStatus,
                InPosition = inPosition,
                StableCounter = stableCounter,
                ReadStartFromTestMs = (readStartTick - metrics.TestStartedTick) * 1000.0 / Stopwatch.Frequency,
                ReadEndFromTestMs = (readEndTick - metrics.TestStartedTick) * 1000.0 / Stopwatch.Frequency,
                ReadLatencyMs = readLatencyMs
            };

            metrics.GroupStatusReadSamples.Add(sample);
        }

        private void ToggleCycleTestGroup1Controls(bool running)
        {
            ButtonStartCycleTestGroup1.IsEnabled = !running;
            ButtonStopCycleTestGroup1.IsEnabled = running;
            ButtonSaveCycleGroup1Result.IsEnabled = !running && _lastCycleTestGroup1Snapshot != null;
        }

        private void ResetCycleTestGroup1Output()
        {
            UpdateCycleTestGroup1Output("Idle", "No result yet.", 0.0);
        }

        private void SetCycleTestGroup1Status(string statusText)
        {
            Action update = delegate
            {
                TextCycleGroup1RunStatus.Text = statusText;
            };

            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(update);
                return;
            }

            update();
        }

        private void UpdateCycleTestGroup1Ui(CycleGroupTestOptions options, CycleGroupTestMetrics metrics)
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

            var summary = BuildCycleTestGroup1Summary(options, metrics);
            UpdateCycleTestGroup1Output(status, summary, progress);
        }

        private void UpdateCycleTestGroup1Output(string status, string summary, double progress)
        {
            Action update = delegate
            {
                TextCycleGroup1RunStatus.Text = status;
                TextCycleGroup1Summary.Text = summary;
                ProgressCycleTestGroup1.Value = Math.Max(0.0, Math.Min(100.0, progress));
            };

            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(update);
                return;
            }

            update();
        }

        private static string BuildCycleTestGroup1Summary(CycleGroupTestOptions options, CycleGroupTestMetrics metrics)
        {
            var builder = new StringBuilder();
            builder.AppendLine("=== Group Motion Cycle Test Group1 Summary ===");
            builder.AppendLine("Profile: P1 -> P2 -> P3 -> P4 -> P1");
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "In-position source: GroupReadStatus mask=0x{0:X8}, stable samples={1}",
                options.InPositionMask,
                options.StableSamplesRequired));
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "Motion: Velocity={0:F3}, Acc={1:F3}, Dec={2:F3}, Jerk={3:F3}",
                options.Velocity,
                options.Acceleration,
                options.Deceleration,
                options.Jerk));
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "Group mode: buffered={0}, coord={1}, transition={2}, transitionParams={3}, superimposed={4}, queuedBlend={5}",
                options.BufferedMode,
                options.CoordSystem,
                options.TransitionMode,
                FormatVector(options.TransitionParams),
                options.Superimposed,
                options.QueueBlendCommands));
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
                "Command latency(MoveLinearAbsoluteEx): avg={0:F3} ms, max={1:F3} ms, samples={2}",
                metrics.CommandLatencyMs.Average,
                metrics.CommandLatencyMs.Max,
                metrics.CommandLatencyMs.Count));
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "Response latency(GroupReadStatus): avg={0:F3} ms, max={1:F3} ms, samples={2}",
                metrics.ResponseLatencyMs.Average,
                metrics.ResponseLatencyMs.Max,
                metrics.ResponseLatencyMs.Count));
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "GroupReadStatus samples: captured={0}, droppedByLimit={1}, limit={2}",
                metrics.GroupStatusReadSamples.Count,
                metrics.GroupStatusReadSamplesDropped,
                MaxStatusReadSamplesToSave));
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "Poll period: avg={0:F3} ms, max={1:F3} ms, drop(th>{2}ms)={3}",
                metrics.PollPeriodMs.Average,
                metrics.PollPeriodMs.Max,
                options.DropThresholdMs,
                metrics.DropCount));
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "Settle wait: avg={0:F3} ms, max={1:F3} ms",
                metrics.PointSettleMs.Average,
                metrics.PointSettleMs.Max));
            builder.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "Timeouts/Error: timeout={0}, groupError={1}, exception={2}",
                metrics.TimeoutCount,
                metrics.GroupErrorCount,
                metrics.ExceptionCount));
            if (!string.IsNullOrWhiteSpace(metrics.LastError))
            {
                builder.AppendLine("Last error: " + metrics.LastError);
            }

            return builder.ToString().TrimEnd();
        }

        private void StoreLastCycleGroup1Snapshot(CycleGroupTestOptions options, CycleGroupTestMetrics metrics)
        {
            _lastCycleTestGroup1Snapshot = new CycleGroupTestSnapshot
            {
                CompletedAt = DateTime.Now,
                GroupName = Context.GroupName ?? "-",
                RemoteIp = Context.RemoteIp ?? "-",
                Options = options,
                Metrics = metrics,
                SummaryText = BuildCycleTestGroup1Summary(options, metrics)
            };
        }

        private string SaveLastCycleGroup1ResultToExcel()
        {
            if (_lastCycleTestGroup1Snapshot == null)
            {
                throw new InvalidOperationException("No completed cycle test group1 result to save.");
            }

            var folderPath = NormalizeNumeric(TextCycleGroup1SaveFolder.Text);
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                throw new InvalidOperationException("Save folder path is empty.");
            }

            Directory.CreateDirectory(folderPath);

            var fileName = string.Format(
                CultureInfo.InvariantCulture,
                "CycleTestGroup1Result_{0:yyyyMMdd_HHmmss}.xlsx",
                _lastCycleTestGroup1Snapshot.CompletedAt);
            var filePath = Path.Combine(folderPath, fileName);

            var sheets = new List<XlsxSheetData>
            {
                new XlsxSheetData("Result", BuildCycleGroup1ResultSheetRows(_lastCycleTestGroup1Snapshot)),
                new XlsxSheetData("GroupStatusReadSamples", BuildGroupStatusReadSampleRows(_lastCycleTestGroup1Snapshot)),
                new XlsxSheetData("ExecutionLog", BuildExecutionLogSheetRows(Context.Logs.ToList()))
            };

            SimpleXlsxExporter.Save(filePath, sheets);
            return filePath;
        }

        private static IList<IList<string>> BuildCycleGroup1ResultSheetRows(CycleGroupTestSnapshot snapshot)
        {
            var rows = new List<IList<string>>();
            rows.Add(new List<string> { "Field", "Value" });
            rows.Add(new List<string> { "CompletedAt", snapshot.CompletedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "GroupName", snapshot.GroupName });
            rows.Add(new List<string> { "RemoteIp", snapshot.RemoteIp });
            rows.Add(new List<string> { "RequestedCycles", snapshot.Options.RequestedCycles.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "WarmupCycles", snapshot.Options.WarmupCycles.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "AttemptedCycles", snapshot.Metrics.AttemptedCycles.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "SuccessfulCycles", snapshot.Metrics.SuccessfulCycles.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "StopReason", snapshot.Metrics.StopReason ?? "-" });
            rows.Add(new List<string> { "P1", FormatVector(snapshot.Options.Points[0]) });
            rows.Add(new List<string> { "P2", FormatVector(snapshot.Options.Points[1]) });
            rows.Add(new List<string> { "P3", FormatVector(snapshot.Options.Points[2]) });
            rows.Add(new List<string> { "P4", FormatVector(snapshot.Options.Points[3]) });
            rows.Add(new List<string> { "Velocity", snapshot.Options.Velocity.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "Acceleration", snapshot.Options.Acceleration.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "Deceleration", snapshot.Options.Deceleration.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "Jerk", snapshot.Options.Jerk.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "BufferedMode", snapshot.Options.BufferedMode.ToString() });
            rows.Add(new List<string> { "CoordSystem", snapshot.Options.CoordSystem.ToString() });
            rows.Add(new List<string> { "TransitionMode", snapshot.Options.TransitionMode.ToString() });
            rows.Add(new List<string> { "TransitionParams", FormatVector(snapshot.Options.TransitionParams) });
            rows.Add(new List<string> { "Superimposed", snapshot.Options.Superimposed.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "QueueBlendCommands", snapshot.Options.QueueBlendCommands.ToString() });
            rows.Add(new List<string> { "MoveTimeout(ms)", snapshot.Options.MoveTimeoutMs.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "PollInterval(ms)", snapshot.Options.PollIntervalMs.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "StableSamples", snapshot.Options.StableSamplesRequired.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "InPositionMask(hex)", "0x" + snapshot.Options.InPositionMask.ToString("X8", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "DropThreshold(ms)", snapshot.Options.DropThresholdMs.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "HighPriorityWorker", snapshot.Options.UseHighPriorityWorkerThread.ToString() });
            rows.Add(new List<string> { "HighPrecisionWait", snapshot.Options.UseHighPrecisionWait.ToString() });
            rows.Add(new List<string> { "TimerResolution1ms", snapshot.Options.Request1msTimerResolution.ToString() });
            rows.Add(new List<string> { "CycleTimeAvg(ms)", snapshot.Metrics.CycleTimeMs.Average.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "CycleTimeMax(ms)", snapshot.Metrics.CycleTimeMs.Max.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "MoveLinearAbsoluteExLatencyAvg(ms)", snapshot.Metrics.CommandLatencyMs.Average.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "MoveLinearAbsoluteExLatencyMax(ms)", snapshot.Metrics.CommandLatencyMs.Max.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "GroupReadStatusLatencyAvg(ms)", snapshot.Metrics.ResponseLatencyMs.Average.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "GroupReadStatusLatencyMax(ms)", snapshot.Metrics.ResponseLatencyMs.Max.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "GroupReadStatusSamplesCaptured", snapshot.Metrics.GroupStatusReadSamples.Count.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "GroupReadStatusSamplesDroppedByLimit", snapshot.Metrics.GroupStatusReadSamplesDropped.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "GroupReadStatusSampleLimit", MaxStatusReadSamplesToSave.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "PollPeriodAvg(ms)", snapshot.Metrics.PollPeriodMs.Average.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "PollPeriodMax(ms)", snapshot.Metrics.PollPeriodMs.Max.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "DropCount", snapshot.Metrics.DropCount.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "SettleWaitAvg(ms)", snapshot.Metrics.PointSettleMs.Average.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "SettleWaitMax(ms)", snapshot.Metrics.PointSettleMs.Max.ToString("F6", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "TimeoutCount", snapshot.Metrics.TimeoutCount.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "GroupErrorCount", snapshot.Metrics.GroupErrorCount.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "ExceptionCount", snapshot.Metrics.ExceptionCount.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "LastError", snapshot.Metrics.LastError ?? "-" });
            rows.Add(new List<string> { "SummaryText", snapshot.SummaryText ?? "-" });
            return rows;
        }

        private static IList<IList<string>> BuildGroupStatusReadSampleRows(CycleGroupTestSnapshot snapshot)
        {
            var rows = new List<IList<string>>();
            rows.Add(new List<string> { "Field", "Value" });
            rows.Add(new List<string> { "CapturedSamples", snapshot.Metrics.GroupStatusReadSamples.Count.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "DroppedByLimit", snapshot.Metrics.GroupStatusReadSamplesDropped.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "CaptureLimit", MaxStatusReadSamplesToSave.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "TestStartedAt", snapshot.Metrics.TestStartedAt.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { string.Empty, string.Empty });

            rows.Add(new List<string>
            {
                "SampleIndex",
                "CycleIndex",
                "Phase",
                "GroupErrorId",
                "GroupStatus(hex)",
                "InPosition",
                "StableCounter",
                "ReadStartFromTest(ms)",
                "ReadEndFromTest(ms)",
                "ReadLatency(ms)"
            });

            foreach (var sample in snapshot.Metrics.GroupStatusReadSamples)
            {
                rows.Add(new List<string>
                {
                    sample.SampleIndex.ToString(CultureInfo.InvariantCulture),
                    sample.CycleIndex.ToString(CultureInfo.InvariantCulture),
                    sample.Phase ?? "-",
                    sample.GroupErrorId.ToString(CultureInfo.InvariantCulture),
                    "0x" + sample.GroupStatus.ToString("X8", CultureInfo.InvariantCulture),
                    sample.InPosition.ToString(),
                    sample.StableCounter.ToString(CultureInfo.InvariantCulture),
                    sample.ReadStartFromTestMs.ToString("F6", CultureInfo.InvariantCulture),
                    sample.ReadEndFromTestMs.ToString("F6", CultureInfo.InvariantCulture),
                    sample.ReadLatencyMs.ToString("F6", CultureInfo.InvariantCulture)
                });
            }

            return rows;
        }
    }
}
