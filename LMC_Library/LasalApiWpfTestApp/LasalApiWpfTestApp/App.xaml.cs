using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace LasalMotionControlApiExample
{
    public partial class App : Application
    {
        private static ApplicationInstanceLease applicationInstanceLease;

        protected override void OnStartup(StartupEventArgs e)
        {
            var probeRequested = e.Args != null
                && e.Args.Length != 0
                && string.Equals(
                    e.Args[0],
                    ExecutableRelaunchProbeArgument,
                    StringComparison.Ordinal);
            if (probeRequested)
            {
                try
                {
                    executableRelaunchProbe =
                        ExecutableRelaunchProbeOptions.Parse(e.Args);
                }
                catch
                {
                    Shutdown(64);
                    return;
                }
            }

            var startupLanguage = UiLanguage.English;
            if (executableRelaunchProbe == null)
            {
                try
                {
                    startupLanguage = UiLanguagePreferenceStore.Load(
                        UiLanguagePreferenceStore.GetDefaultFilePath());
                }
                catch
                {
                    // Startup safety dialogs remain available in English even if
                    // the local preference location cannot be resolved.
                }
            }

            try
            {
                if (!ApplicationInstanceLease.TryAcquireDefault(
                        out applicationInstanceLease))
                {
                    if (executableRelaunchProbe != null)
                    {
                        WriteExecutableRelaunchProbeReport(
                            executableRelaunchProbe,
                            "MUTEX_BUSY",
                            null,
                            "The default application-instance mutex is already owned.");
                        Shutdown(2);
                        return;
                    }

                    MessageBox.Show(
                        UiLocalizationCatalog.Translate(
                            "LASAL Motion Control API Example is already running "
                                + "in this Windows session.\n\nClose the existing "
                                + "instance before starting another. This second "
                                + "instance will exit before opening recovery journals "
                                + "or network ports.",
                            startupLanguage),
                        UiLocalizationCatalog.Translate(
                            "LASAL Motion Control API Example - Already Running",
                            startupLanguage),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    Shutdown(2);
                    return;
                }
            }
            catch (Exception error)
            {
                if (executableRelaunchProbe != null)
                {
                    WriteExecutableRelaunchProbeReport(
                        executableRelaunchProbe,
                        "STARTUP_FAILED",
                        null,
                        error.GetType().Name + ": " + error.Message);
                    Shutdown(3);
                    return;
                }

                MessageBox.Show(
                    UiLocalizationCatalog.Translate(
                        "Startup was blocked because the single-instance guard "
                            + "could not be acquired. No recovery journal or network "
                            + "port was opened.\n\n",
                        startupLanguage)
                    + error.GetType().Name
                    + ": "
                    + error.Message,
                    UiLocalizationCatalog.Translate(
                        "LASAL Motion Control API Example - Startup Blocked",
                        startupLanguage),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown(3);
                return;
            }

            base.OnStartup(e);
            MainWindow window;
            try
            {
                window = executableRelaunchProbe == null
                    ? new MainWindow()
                    : new MainWindow(executableRelaunchProbe.JournalRootPath);
            }
            catch (Exception error)
            {
                if (executableRelaunchProbe == null)
                {
                    throw;
                }

                WriteExecutableRelaunchProbeReport(
                    executableRelaunchProbe,
                    "STARTUP_FAILED",
                    null,
                    error.GetType().Name + ": " + error.Message);
                Shutdown(3);
                return;
            }

            MainWindow = window;
            RecentRecoveryPanelLocalization.Attach(window);
            if (executableRelaunchProbe != null)
            {
                window.Closed += ExecutableRelaunchProbeWindow_Closed;
            }
            window.Show();

            if (executableRelaunchProbe != null)
            {
                Dispatcher.BeginInvoke(
                    new Action(
                        () => BeginExecutableRelaunchProbeConnect(window)));
            }

            // Intentionally retain the lease until OS process teardown. Releasing
            // it from a startup-failure or WPF OnExit path could admit another
            // process while this process still owns partially opened journals.
        }
    }

    internal sealed class RecentRecoveryPanelLocalization
    {
        private static readonly IDictionary<string, string> KoreanExact =
            CreateKoreanExact();

        private static readonly KeyValuePair<string, string>[] KoreanPrefixes =
        {
            Pair(
                "JOURNAL UNAVAILABLE - START FAIL-CLOSED. ",
                "JOURNAL 사용 불가 - START FAIL-CLOSED. "),
            Pair(
                "UNRESOLVED / START REPLAY BLOCKED | ",
                "미해결 / START 재전송 차단 | "),
            Pair(
                "Journal ready; no unresolved record. AdminTriad=",
                "Journal 준비됨; 미해결 record 없음. AdminTriad="),
            Pair("START REJECTED: ", "START 거부됨: "),
            Pair("START OUTCOME UNCERTAIN. ", "START 결과 미확정. "),
            Pair("OUTCOME RUNNING. QueryRequestId=", "OUTCOME 진행 중. QueryRequestId="),
            Pair(
                "TERMINAL OUTCOME STORED DURABLY. State=",
                "TERMINAL OUTCOME durable 저장 완료. State="),
            Pair(
                "RECOVERY RESOLVED. Terminal outcome=",
                "복구 해결됨. Terminal outcome="),
            Pair("ARMED BEFORE DISPATCH. ", "전송 전 ARMED. "),
            Pair(
                "JOURNAL UNAVAILABLE - future HomeDS402Ex Start remains fail-closed. ",
                "JOURNAL 사용 불가 - 향후 HomeDS402Ex Start는 fail-closed 유지. "),
            Pair("HomeDS402ExRecovery State=", "HomeDS402Ex 복구 State=")
        };

        private static readonly DependencyPropertyDescriptor TextDescriptor =
            DependencyPropertyDescriptor.FromProperty(
                TextBlock.TextProperty,
                typeof(TextBlock));

        private readonly MainWindow window;
        private readonly Dictionary<DependencyObject, string> englishText =
            new Dictionary<DependencyObject, string>();
        private readonly HashSet<TextBlock> watchedTextBlocks =
            new HashSet<TextBlock>();
        private bool applying;

        private RecentRecoveryPanelLocalization(MainWindow window)
        {
            this.window = window ?? throw new ArgumentNullException("window");
        }

        internal static void Attach(MainWindow window)
        {
            var localizer = new RecentRecoveryPanelLocalization(window);
            window.Loaded += localizer.Window_Loaded;

            var languageSelector = window.FindName("ComboUiLanguage") as ComboBox;
            if (languageSelector != null)
            {
                languageSelector.SelectionChanged +=
                    localizer.LanguageSelector_SelectionChanged;
            }

            localizer.ApplyCurrentLanguage();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            QueueApply();
        }

        private void LanguageSelector_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            QueueApply();
        }

        private void QueueApply()
        {
            window.Dispatcher.BeginInvoke(
                DispatcherPriority.ContextIdle,
                new Action(ApplyCurrentLanguage));
        }

        private UiLanguage CurrentLanguage
        {
            get
            {
                var languageSelector =
                    window.FindName("ComboUiLanguage") as ComboBox;
                var selected = languageSelector == null
                    ? null
                    : languageSelector.SelectedItem as UiLanguageOption;
                return selected == null
                    ? UiLanguage.English
                    : selected.Language;
            }
        }

        private void ApplyCurrentLanguage()
        {
            if (applying)
            {
                return;
            }

            applying = true;
            try
            {
                var visited = new HashSet<DependencyObject>();
                ApplyRecursive(window, visited);
                // These recovery panels are composed dynamically. They are not
                // guaranteed to be reachable from the window tree before their
                // containing tab is first realized, so localize their roots
                // explicitly as well.
                ApplyRecursive(
                    window.AxisSetOperationModeRecoveryGroupForTests,
                    visited);
                ApplyRecursive(
                    window.AxisDs402HomeExRecoveryGroupForTests,
                    visited);
            }
            finally
            {
                applying = false;
            }
        }

        private void ApplyRecursive(
            DependencyObject current,
            ISet<DependencyObject> visited)
        {
            if (current == null || !visited.Add(current))
            {
                return;
            }

            var textBlock = current as TextBlock;
            if (textBlock != null)
            {
                ApplyTextBlock(textBlock);
            }

            var contentControl = current as ContentControl;
            if (contentControl != null)
            {
                var stringContent = contentControl.Content as string;
                if (stringContent != null)
                {
                    ApplyStringValue(
                        contentControl,
                        stringContent,
                        value => contentControl.Content = value);
                }

                var dependencyContent =
                    contentControl.Content as DependencyObject;
                if (dependencyContent != null)
                {
                    ApplyRecursive(dependencyContent, visited);
                }
            }

            var groupBox = current as GroupBox;
            if (groupBox != null && groupBox.Header is string)
            {
                ApplyStringValue(
                    groupBox,
                    (string)groupBox.Header,
                    value => groupBox.Header = value);
            }

            var panel = current as Panel;
            if (panel != null)
            {
                foreach (UIElement child in panel.Children)
                {
                    ApplyRecursive(child, visited);
                }
            }

            foreach (var child in LogicalTreeHelper.GetChildren(current))
            {
                var dependencyChild = child as DependencyObject;
                if (dependencyChild != null)
                {
                    ApplyRecursive(dependencyChild, visited);
                }
            }

            var visual = current as Visual;
            if (visual == null)
            {
                return;
            }

            var visualChildCount = VisualTreeHelper.GetChildrenCount(visual);
            for (var index = 0; index < visualChildCount; index++)
            {
                ApplyRecursive(
                    VisualTreeHelper.GetChild(visual, index),
                    visited);
            }
        }

        private void ApplyTextBlock(TextBlock textBlock)
        {
            var current = textBlock.Text ?? string.Empty;
            string original;
            if (CurrentLanguage == UiLanguage.Korean)
            {
                original = GetEnglishText(textBlock, current);
                var translated = TranslateRecent(original);
                if (!string.Equals(
                        translated,
                        original,
                        StringComparison.Ordinal))
                {
                    englishText[textBlock] = original;
                    Watch(textBlock);
                    textBlock.Text = translated;
                }
                return;
            }

            if (englishText.TryGetValue(textBlock, out original)
                && !string.Equals(
                    current,
                    original,
                    StringComparison.Ordinal))
            {
                textBlock.Text = original;
            }
            else if (IsRecentEnglish(current))
            {
                englishText[textBlock] = current;
                Watch(textBlock);
            }
        }

        private void ApplyStringValue(
            DependencyObject owner,
            string current,
            Action<string> assign)
        {
            string original;
            if (CurrentLanguage == UiLanguage.Korean)
            {
                original = GetEnglishText(owner, current);
                var translated = TranslateRecent(original);
                if (!string.Equals(
                        translated,
                        original,
                        StringComparison.Ordinal))
                {
                    englishText[owner] = original;
                    assign(translated);
                }
                return;
            }

            if (englishText.TryGetValue(owner, out original)
                && !string.Equals(
                    current,
                    original,
                    StringComparison.Ordinal))
            {
                assign(original);
            }
        }

        private string GetEnglishText(
            DependencyObject owner,
            string current)
        {
            string original;
            if (englishText.TryGetValue(owner, out original))
            {
                return original;
            }
            return current ?? string.Empty;
        }

        private void Watch(TextBlock textBlock)
        {
            if (watchedTextBlocks.Add(textBlock) && TextDescriptor != null)
            {
                TextDescriptor.AddValueChanged(
                    textBlock,
                    TextBlock_TextChanged);
            }
        }

        private void TextBlock_TextChanged(object sender, EventArgs e)
        {
            if (applying)
            {
                return;
            }

            var textBlock = sender as TextBlock;
            if (textBlock == null)
            {
                return;
            }

            var current = textBlock.Text ?? string.Empty;
            if (CurrentLanguage != UiLanguage.Korean)
            {
                if (IsRecentEnglish(current))
                {
                    englishText[textBlock] = current;
                }
                return;
            }

            var translated = TranslateRecent(current);
            if (string.Equals(
                    translated,
                    current,
                    StringComparison.Ordinal))
            {
                return;
            }

            englishText[textBlock] = current;
            applying = true;
            try
            {
                textBlock.Text = translated;
            }
            finally
            {
                applying = false;
            }
        }

        private static bool IsRecentEnglish(string value)
        {
            return !string.Equals(
                TranslateRecent(value),
                value,
                StringComparison.Ordinal);
        }

        private static string TranslateRecent(string english)
        {
            if (string.IsNullOrEmpty(english))
            {
                return english;
            }

            string exact;
            if (KoreanExact.TryGetValue(english, out exact))
            {
                return exact;
            }

            foreach (var prefix in KoreanPrefixes)
            {
                if (english.StartsWith(prefix.Key, StringComparison.Ordinal))
                {
                    return prefix.Value + english.Substring(prefix.Key.Length);
                }
            }

            return english;
        }

        private static IDictionary<string, string> CreateKoreanExact()
        {
            var values = new Dictionary<string, string>(StringComparer.Ordinal);

            values["Set Operation Mode - CSP=8 / durable no-replay recovery"] =
                "Operation Mode 설정 - CSP=8 / durable 재전송 방지 복구";
            values["LIVE DRIVE MODE WRITE. Start sends 0x7D23 once only. A successful Start response is acceptance only, not mode-change completion. After any uncertain or accepted Start, automatic 0x7D23/0x6060 replay is forbidden."] =
                "실제 드라이브 모드 Write입니다. Start는 0x7D23을 한 번만 전송합니다. Start 성공 응답은 접수만 의미하며 모드 변경 완료를 의미하지 않습니다. Start 결과가 미확정이거나 접수된 뒤에는 0x7D23/0x6060 자동 재전송을 금지합니다.";
            values["Recovery is bound to endpoint + DiagnosticsBuild + BootId + MapRevision + 128-bit ClientIntentId + RequestId + axis + requested CSP mode. Recovery queries 0x7D24 only; terminal proof is persisted before exact-generation 0x7D25 retirement."] =
                "복구는 endpoint + DiagnosticsBuild + BootId + MapRevision + 128-bit ClientIntentId + RequestId + axis + 요청 CSP mode에 정확히 결합됩니다. 복구 시 0x7D24만 조회하며 terminal 증거를 durable 저장한 뒤 exact-generation 0x7D25 retirement를 수행합니다.";
            values["Physical axis reference (1..4)"] = "물리 축 번호 (1..4)";
            values["Requested mode"] = "요청 모드";
            values["CyclicSynchronousPosition (8)"] = "CSP 위치 동기 모드 (8)";
            values["Timeout (ms, nonzero)"] = "Timeout (ms, 0 제외)";
            values["I verified the exact powered drive/axis and understand that this writes DS402 0x6060:0 to CSP=8 once only. If the response or completion is uncertain I will use the durable recovery query and will not send Start again."] =
                "정확한 powered drive/axis를 확인했으며 DS402 0x6060:0에 CSP=8을 한 번만 Write한다는 점을 이해했습니다. 응답 또는 완료 여부가 미확정이면 durable 복구 조회를 사용하고 Start를 다시 전송하지 않겠습니다.";
            values["Refresh Mode Capabilities"] = "Mode capability 새로고침";
            values["Start CSP Once (0x7D23)"] = "CSP 1회 시작 (0x7D23)";
            values["Query / Retire Recovery (No Start Replay)"] =
                "복구 조회 / 폐기 (Start 재전송 없음)";
            values["SetOperationMode recovery journal is initializing."] =
                "SetOperationMode 복구 journal을 초기화하는 중입니다.";

            values["HomeDS402Ex - durable no-replay recovery (Start UI closed)"] =
                "HomeDS402Ex - durable 재전송 방지 복구 (Start UI 닫힘)";
            values["NO HOMEDS402EX START UI. Engineering scale/profile and LASAL runtime activation are not qualified. This panel only recovers an already durable exact intent through 0x7D1C/0x7D1D."] =
                "HOMEDS402EX START UI는 제공하지 않습니다. Engineering scale/profile과 LASAL runtime activation은 아직 qualification되지 않았습니다. 이 패널은 이미 durable하게 저장된 정확한 intent만 0x7D1C/0x7D1D로 복구합니다.";
            values["Recovery is bound to endpoint + DiagnosticsBuild + BootId + MapRevision + RequestId + 128-bit ClientIntentId + axis + every frozen HomeDS402Ex plan field. Terminal proof is persisted before exact-generation retirement; Start is never reconstructed from the journal."] =
                "복구는 endpoint + DiagnosticsBuild + BootId + MapRevision + RequestId + 128-bit ClientIntentId + axis + 모든 frozen HomeDS402Ex plan field에 정확히 결합됩니다. Terminal 증거를 durable 저장한 뒤 exact-generation retirement를 수행하며 journal로부터 Start를 재구성하지 않습니다.";
            values["Refresh HomeEx Capabilities"] =
                "HomeEx capability 새로고침";
            values["Query / Retire HomeEx Recovery (No Start Replay)"] =
                "HomeEx 복구 조회 / 폐기 (Start 재전송 없음)";
            values["No unresolved HomeDS402Ex recovery record. Start UI intentionally remains closed until HOMEEX-01/02/06..11 are qualified."] =
                "미해결 HomeDS402Ex 복구 record가 없습니다. HOMEEX-01/02/06..11 qualification이 완료될 때까지 Start UI는 의도적으로 닫힌 상태를 유지합니다.";
            values["HomeDS402Ex recovery record: none"] =
                "HomeDS402Ex 복구 record: 없음";

            return values;
        }

        private static KeyValuePair<string, string> Pair(
            string english,
            string korean)
        {
            return new KeyValuePair<string, string>(english, korean);
        }
    }
}
