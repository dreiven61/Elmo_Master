using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using LasalMotionControlLib.Tests;

namespace LasalMotionControlApiExample
{
    internal static class UiLocalizationTests
    {
        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.UiLocalization.PreferenceDefaultsAndInvalidToken",
                PreferenceDefaultsAndInvalidToken);
            tests.Add(
                "Wpf.UiLocalization.PreferenceSaveLoadRoundTrip",
                PreferenceSaveLoadRoundTrip);
            tests.Add(
                "Wpf.UiLocalization.CatalogAndDynamicRecoveryDetail",
                CatalogAndDynamicRecoveryDetail);
            tests.Add(
                "Wpf.UiLocalization.DynamicSafetyActionsAndGuidance",
                DynamicSafetyActionsAndGuidance);
            tests.Add(
                "Wpf.UiLocalization.StaticChromeHasExplicitCatalogEntries",
                StaticChromeHasExplicitCatalogEntries);
            tests.Add(
                "Wpf.UiLocalization.DialogAndTechnicalCatalog",
                DialogAndTechnicalCatalog);
            tests.Add(
                "Wpf.UiLocalization.ServiceEnglishKoreanEnglishPreservesTextBox",
                ServiceEnglishKoreanEnglishPreservesTextBox);
            tests.Add(
                "Wpf.UiLocalization.MainWindowLoadsAndPersistsSelectorLanguage",
                MainWindowLoadsAndPersistsSelectorLanguage);
        }

        private static void PreferenceDefaultsAndInvalidToken()
        {
            WithTemporaryDirectory(delegate(string directory)
            {
                var missingPath = Path.Combine(
                    directory,
                    "missing-language.txt");
                AssertEx.Equal(
                    UiLanguage.English,
                    UiLanguagePreferenceStore.Load(null));
                AssertEx.Equal(
                    UiLanguage.English,
                    UiLanguagePreferenceStore.Load(" "));
                AssertEx.Equal(
                    UiLanguage.English,
                    UiLanguagePreferenceStore.Load(missingPath));

                var invalidPath = Path.Combine(
                    directory,
                    "invalid-language.txt");
                File.WriteAllText(
                    invalidPath,
                    "not-a-supported-language",
                    new UTF8Encoding(false));
                AssertEx.Equal(
                    UiLanguage.English,
                    UiLanguagePreferenceStore.Load(invalidPath));
            });
        }

        private static void PreferenceSaveLoadRoundTrip()
        {
            WithTemporaryDirectory(delegate(string directory)
            {
                var preferencePath = Path.Combine(
                    directory,
                    "nested",
                    "ui-language.txt");

                UiLanguagePreferenceStore.Save(
                    preferencePath,
                    UiLanguage.Korean);
                AssertEx.Equal(
                    "ko-KR",
                    File.ReadAllText(preferencePath, Encoding.UTF8));
                AssertEx.Equal(
                    UiLanguage.Korean,
                    UiLanguagePreferenceStore.Load(preferencePath));

                UiLanguagePreferenceStore.Save(
                    preferencePath,
                    UiLanguage.English);
                AssertEx.Equal(
                    "en-US",
                    File.ReadAllText(preferencePath, Encoding.UTF8));
                AssertEx.Equal(
                    UiLanguage.English,
                    UiLanguagePreferenceStore.Load(preferencePath));
            });
        }

        private static void CatalogAndDynamicRecoveryDetail()
        {
            AssertEx.True(
                UiLocalizationCatalog.KoreanTranslationCount >= 418,
                "The Korean UI catalog lost broad tab/action coverage.");
            AssertEx.Equal(
                "Connect",
                UiLocalizationCatalog.Translate(
                    "Connect",
                    UiLanguage.English));
            AssertEx.Equal(
                "연결",
                UiLocalizationCatalog.Translate(
                    "Connect",
                    UiLanguage.Korean));
            AssertEx.Equal(
                "실제 축 Qualification 실행",
                UiLocalizationCatalog.Translate(
                    "Run LIVE Axis Qualification",
                    UiLanguage.Korean));
            AssertEx.Equal(
                "그룹 모션",
                UiLocalizationCatalog.Translate(
                    "Group Motion",
                    UiLanguage.Korean));
            AssertEx.Equal(
                "CREVIS / Topology 불러오기",
                UiLocalizationCatalog.Translate(
                    "Load CREVIS / Topology",
                    UiLanguage.Korean));
            AssertEx.Equal(
                "Bulk qualification 자동화",
                UiLocalizationCatalog.Translate(
                    "Bulk qualification automation",
                    UiLanguage.Korean));
            AssertEx.Equal(
                "레코더",
                UiLocalizationCatalog.Translate(
                    "Recorder",
                    UiLanguage.Korean));
            AssertEx.Equal(
                "SDO / Write 정책",
                UiLocalizationCatalog.Translate(
                    "SDO / Write Policy",
                    UiLanguage.Korean));
            AssertEx.Equal(
                "읽기 전용 API",
                UiLocalizationCatalog.Translate(
                    "Read-only API",
                    UiLanguage.Korean));
            AssertEx.Equal(
                "Unknown runtime text 42",
                UiLocalizationCatalog.Translate(
                    "Unknown runtime text 42",
                    UiLanguage.Korean));

            const string dynamicIdentityDetail =
                "Stored BootId=0x00000006, current BootId=0x0000000C, "
                + "stored MapRevision=0x957F101E, current MapRevision=0x957F101E.";
            const string warning =
                "SAFETY: RECOVERY IDENTITY READ-ONLY QUARANTINE. "
                + "The stored recovery identity does not match the current PLC. "
                + "Only ordinary non-D5 read-only inspection, local draft editing, and Close/Exit are allowed. "
                + "Do not infer the old command result from the current PLC state; the durable recovery record remains unchanged. "
                + "Reconnect Axis Power On recovery identity is blocked because DiagnosticsBootId or MapRevision does not match the durable "
                + "Axis Power On recovery record. "
                + dynamicIdentityDetail;

            var translated = UiLocalizationCatalog.Translate(
                warning,
                UiLanguage.Korean);
            AssertEx.Contains(
                "안전: 복구 ID 읽기 전용 격리.",
                translated);
            AssertEx.Contains(
                "저장된 복구 ID가 현재 PLC와 일치하지 않습니다.",
                translated);
            AssertEx.Contains(
                "일반 non-D5 읽기 전용 확인, 로컬 초안 편집, Close/Exit만 허용됩니다.",
                translated);
            AssertEx.Contains(
                "현재 PLC 상태로 이전 명령 결과를 추정하지 마십시오.",
                translated);
            AssertEx.Contains(
                "Reconnect Axis Power On recovery identity",
                translated);
            AssertEx.Contains(dynamicIdentityDetail, translated);
        }

        private static void DynamicSafetyActionsAndGuidance()
        {
            var expectedActions = new Dictionary<string, string>(
                StringComparer.Ordinal)
            {
                {
                    "Power Off Safety Takeover",
                    "안전 인수 Power Off"
                },
                {
                    "Power Off (Durability Degraded)",
                    "Power Off (durable 기록 저하)"
                },
                {
                    "Resume Reset Verification (No 0x2024 Replay)",
                    "Reset 확인 계속 (0x2024 재전송 없음)"
                },
                {
                    "Retry Stop (Outcome Uncertain)",
                    "Stop 재시도 (결과 미확정)"
                },
                {
                    "Observe Pending Reset (Single Group Status)",
                    "대기 중 Reset 확인 (단일 그룹 상태)"
                },
                {
                    "Resume Power On Verification (No 0x204A Replay)",
                    "Power On 확인 계속 (0x204A 재전송 없음)"
                },
                {
                    "Resume Unlock Verification (No 0x2048 Replay)",
                    "Unlock 확인 계속 (0x2048 재전송 없음)"
                },
                {
                    "Disable (Reset Safety Recovery)",
                    "Disable (Reset 안전 복구)"
                },
                {
                    "Resume Reset Verification (No 0x2049 Replay)",
                    "Reset 확인 계속 (0x2049 재전송 없음)"
                }
            };

            foreach (var expected in expectedActions)
            {
                AssertEx.Equal(
                    expected.Value,
                    UiLocalizationCatalog.Translate(
                        expected.Key,
                        UiLanguage.Korean));
            }

            var allDynamicActionSources = new[]
            {
                "Power On Replay Blocked - Send Power Off",
                "Resume Power On Verification (No 0x2023 Replay)",
                "Power On",
                "Power Off Again (Confirmed Interference)",
                "Resume Power Off Verification (No 0x2023 Replay)",
                "Power Off Safety Takeover",
                "Power Off (Durability Degraded)",
                "Power Off",
                "Resume Reset Verification (No 0x2024 Replay)",
                "Reset Again (Confirmed Interference)",
                "Retry Reset (Outcome Uncertain)",
                "Reset Blocked by Stop Recovery",
                "Reset",
                "Resume Stop Verification (No 0x2022 Replay)",
                "Retry Stop (Outcome Uncertain)",
                "Stop Safety Takeover",
                "Stop",
                "Read Status (Inspection Only)",
                "Observe Pending Reset (Single Group Status)",
                "Observe Pending Power Off (Single Status)",
                "Observe Pending Power On (Single Status)",
                "Observe Lock State (Safe Recovery Required)",
                "Verify Pending Lock State (Read Status)",
                "2 / 5 Read Status (Power Ready / Lock Ready)",
                "Resume Power On Verification (No 0x204A Replay)",
                "1 Power On",
                "Send Power Off Safety Takeover",
                "Resume Power Off Verification (No 0x204B Replay)",
                "7 Power Off",
                "Lock State Uncertain - Safe Recovery Required",
                "Resume Lock Verification (No 0x2047 Replay)",
                "4 Enable (Lock Profile)",
                "Resume Unlock Verification (No 0x2048 Replay)",
                "Retry Disable Explicitly (0x2048)",
                "Disable (Lock-to-Unlock Takeover)",
                "Disable Replay Blocked",
                "Disable (Reset Safety Recovery)",
                "Disable (Observed Reset LockedStandby)",
                "Disable (Unlock Profile)",
                "Resume Reset Verification (No 0x2049 Replay)",
                "Reset Replay Blocked - Safety Recovery Required",
                "Group Reset",
                "Arm SDO Write",
                "Run Same-Value Qualification First",
                "Submit Required Exact Readback",
                "Readback Session Mismatch",
                "Confirm & Submit SDO Write",
                "Submit SDO Read",
                "Verify Recovered SDO Readback",
                "Acknowledge Stale SDO Write",
                "Acknowledge Recovered Mutation",
                "Clean Active D5 Ticket (Write quarantine remains)",
                "SDO Write Quarantine (Read proof unavailable)",
                "Resolve D5 Ticket (Readback remains)",
                "Exact SDO Write Readback Required",
                "Resolve D5 Quarantine",
                "Cleanup Retained Double",
                "Cleanup Retained Double (gates closed)",
                "Continue Double Recovery",
                "Recover Double Journal",
                "Recover Double Journal (gate closed)",
                "Resume: Slave Is Offline",
                "Resume: Slave Restored",
                "Resume External Step"
            };
            foreach (var source in allDynamicActionSources)
            {
                AssertEx.True(
                    !string.Equals(
                        source,
                        UiLocalizationCatalog.Translate(
                            source,
                            UiLanguage.Korean),
                        StringComparison.Ordinal),
                    "Dynamic action has no Korean translation: " + source);
            }

            AssertEx.Equal(
                "안전: 축 Power On 결과가 미확정입니다. Power On을 재전송하지 말고 Power Off를 명시적으로 전송한 뒤 안전 상태 sample 3회를 안정적으로 확인하십시오.",
                UiLocalizationCatalog.Translate(
                    "SAFETY: Axis Power On outcome is uncertain. Do not replay Power On; send Power Off explicitly and verify three stable safe samples.",
                    UiLanguage.Korean));

            var preparation = UiLocalizationCatalog.Translate(
                "Preparation: Power Ready/ACTIVE verified | identity axes referenced | identity configured | profile locked/standby verified. Ready: Move Linear or Disable (Unlock Profile).",
                UiLanguage.Korean);
            AssertEx.Contains("준비: Power Ready/ACTIVE 확인됨", preparation);
            AssertEx.Contains("identity 축 reference 완료", preparation);
            AssertEx.Contains("identity 설정됨", preparation);
            AssertEx.Contains("profile locked/standby 확인됨", preparation);
            AssertEx.Contains(
                "준비됨: Move Linear 또는 Disable(Profile Unlock)을 실행할 수 있습니다.",
                preparation);

            const string target =
                "Slave=1, Index=0x2101, SubIndex=0x00, Type=UInt32";
            var sdoGuidance = UiLocalizationCatalog.Translate(
                "SAFETY: SDO Write transport completed but exact manual readback is pending for "
                    + target
                    + ". Only that exact SDO Read under the original BootId/MapRevision, Stop, PowerOff, and existing-resource cleanup are allowed; mutation and Close remain blocked.",
                UiLanguage.Korean);
            AssertEx.Contains(
                "안전: SDO Write 전송은 완료됐지만",
                sdoGuidance);
            AssertEx.Contains(target, sdoGuidance);
            AssertEx.Contains(
                "Mutation과 Close는 계속 차단됩니다.",
                sdoGuidance);

            AssertEx.Contains(
                "4-ticket 동일 값 SDO Write qualification",
                UiLocalizationCatalog.Translate(
                    "Manual SDO Write is fail-closed until this exact connection session, DiagnosticsBuild, BootId, MapRevision, and approved target pass the four-ticket Same-Value SDO Write qualification below.",
                    UiLanguage.Korean));
            AssertEx.Equal(
                "대기 중인 checkpoint: INJECT_ONE_SLAVE_OFFLINE",
                UiLocalizationCatalog.Translate(
                    "Waiting checkpoint: INJECT_ONE_SLAVE_OFFLINE",
                    UiLanguage.Korean));
            AssertEx.Equal(
                "같은 session 정리가 차단됐습니다: raw denial detail",
                UiLocalizationCatalog.Translate(
                    "Same-session cleanup is blocked: raw denial detail",
                    UiLanguage.Korean));
        }

        private static void StaticChromeHasExplicitCatalogEntries()
        {
            WithTemporaryDirectory(delegate(string root)
            {
                var preferencePath = Path.Combine(
                    root,
                    "UiLanguage",
                    "ui-language.txt");
                UiLanguagePreferenceStore.Save(
                    preferencePath,
                    UiLanguage.English);

                MainWindow window = null;
                try
                {
                    window = new MainWindow(root)
                    {
                        ShowActivated = false,
                        ShowInTaskbar = false,
                        WindowStartupLocation = WindowStartupLocation.Manual,
                        Left = -10000,
                        Top = -10000
                    };
                    window.Show();
                    WaitForUiCondition(
                        () => window.IsLoaded,
                        "The English MainWindow did not load for localization coverage.");

                    var sources = new HashSet<string>(StringComparer.Ordinal);
                    CollectUiChromeStrings(window, sources);
                    var missing = sources
                        .Where(value =>
                            !UiLocalizationCatalog.HasKoreanTranslation(value)
                            && string.Equals(
                                value,
                                UiLocalizationCatalog.Translate(
                                    value,
                                    UiLanguage.Korean),
                                StringComparison.Ordinal))
                        .OrderBy(value => value, StringComparer.Ordinal)
                        .ToArray();
                    AssertEx.True(
                        sources.Count >= 350,
                        "The static UI chrome coverage unexpectedly shrank.");
                    AssertEx.True(
                        missing.Length == 0,
                        "Static UI chrome has no explicit Korean catalog entry: "
                            + string.Join(" | ", missing));

                    CloseLocalizedMainWindow(window);
                    window = null;
                }
                finally
                {
                    CloseLocalizedMainWindow(window);
                }
            });
        }

        private static void DialogAndTechnicalCatalog()
        {
            AssertEx.Equal(
                "LASAL 모션 제어 API 예제 - 이미 실행 중",
                UiLocalizationCatalog.Translate(
                    "LASAL Motion Control API Example - Already Running",
                    UiLanguage.Korean));
            AssertEx.Equal(
                "복구된 Mutation 승인",
                UiLocalizationCatalog.Translate(
                    "Acknowledge Recovered Mutation",
                    UiLanguage.Korean));
            AssertEx.Equal(
                "보호된 digital output write를 전송하시겠습니까?",
                UiLocalizationCatalog.Translate(
                    "Submit guarded digital output write?",
                    UiLanguage.Korean));
            AssertEx.Equal(
                "CSV 파일 (*.csv)|*.csv|모든 파일 (*.*)|*.*",
                UiLocalizationCatalog.Translate(
                    "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    UiLanguage.Korean));
            AssertEx.Equal(
                "Int8",
                UiLocalizationCatalog.Translate(
                    "Int8",
                    UiLanguage.Korean));
            AssertEx.Equal(
                "DS402",
                UiLocalizationCatalog.Translate(
                    "DS402",
                    UiLanguage.Korean));
            AssertEx.Equal(
                "정지",
                UiLocalizationCatalog.Translate(
                    "Stop",
                    UiLanguage.Korean));
        }

        private static void
            ServiceEnglishKoreanEnglishPreservesTextBox()
        {
            var root = new StackPanel();
            var button = new Button { Content = "Connect" };
            var status = new TextBlock { Text = "Connection state" };
            var boundStatus = new TextBlock();
            BindingOperations.SetBinding(
                boundStatus,
                TextBlock.TextProperty,
                new Binding("Text")
                {
                    Source = status
                });
            var group = new GroupBox { Header = "Axis object" };
            var userInput = new TextBox { Text = "Connect" };
            var grid = new DataGrid();
            var column = new DataGridTextColumn
            {
                Header = "Latest axis result"
            };
            grid.Columns.Add(column);
            root.Children.Add(button);
            root.Children.Add(status);
            root.Children.Add(boundStatus);
            root.Children.Add(group);
            root.Children.Add(userInput);
            root.Children.Add(grid);

            UiLocalizationService.Apply(root, UiLanguage.English);
            AssertEx.Equal("Connect", (string)button.Content);
            AssertEx.Equal("Connection state", status.Text);
            AssertEx.Equal("Connection state", boundStatus.Text);
            AssertEx.True(
                BindingOperations.IsDataBound(
                    boundStatus,
                    TextBlock.TextProperty),
                "The English localization pass removed a Text binding.");
            AssertEx.Equal("Axis object", (string)group.Header);
            AssertEx.Equal("Latest axis result", (string)column.Header);
            AssertEx.Equal(
                "Connect",
                userInput.Text,
                "The English localization pass changed operator input.");

            UiLocalizationService.Apply(root, UiLanguage.Korean);
            AssertEx.Equal("연결", (string)button.Content);
            AssertEx.Equal("연결 상태", status.Text);
            AssertEx.Equal("연결 상태", boundStatus.Text);
            AssertEx.True(
                BindingOperations.IsDataBound(
                    boundStatus,
                    TextBlock.TextProperty),
                "The Korean localization pass removed a Text binding.");
            AssertEx.Equal("축 객체", (string)group.Header);
            AssertEx.Equal("최근 축 결과", (string)column.Header);
            AssertEx.Equal(
                "Connect",
                userInput.Text,
                "The Korean localization pass translated operator input.");

            button.Content = "Close";
            status.Text = "Close";
            UiLocalizationService.Apply(root, UiLanguage.Korean);
            AssertEx.Equal(
                "닫기",
                (string)button.Content,
                "A dynamic English button state was not translated.");
            AssertEx.Equal(
                "닫기",
                boundStatus.Text,
                "The bound mirror did not follow translated source text.");
            AssertEx.True(
                BindingOperations.IsDataBound(
                    boundStatus,
                    TextBlock.TextProperty),
                "A dynamic Korean pass removed a Text binding.");

            UiLocalizationService.Apply(root, UiLanguage.English);
            AssertEx.Equal("Close", (string)button.Content);
            AssertEx.Equal("Close", status.Text);
            AssertEx.Equal("Close", boundStatus.Text);
            AssertEx.True(
                BindingOperations.IsDataBound(
                    boundStatus,
                    TextBlock.TextProperty),
                "The English restore pass removed a Text binding.");
            AssertEx.Equal("Axis object", (string)group.Header);
            AssertEx.Equal("Latest axis result", (string)column.Header);
            AssertEx.Equal(
                "Connect",
                userInput.Text,
                "The English restore pass changed operator input.");
        }

        private static void CollectUiChromeStrings(
            DependencyObject target,
            ISet<string> values)
        {
            AddUiChromeString((target as Window)?.Title, values);

            var headeredContent = target as HeaderedContentControl;
            if (headeredContent != null)
            {
                AddUiChromeString(headeredContent.Header, values);
            }

            var headeredItems = target as HeaderedItemsControl;
            if (headeredItems != null)
            {
                AddUiChromeString(headeredItems.Header, values);
            }

            var content = target as ContentControl;
            if (content != null)
            {
                AddUiChromeString(content.Content, values);
            }

            var textBlock = target as TextBlock;
            if (textBlock != null
                && !string.Equals(
                    textBlock.Tag as string,
                    UiLocalizationService.PreserveTextTag,
                    StringComparison.Ordinal))
            {
                AddUiChromeString(textBlock.Text, values);
            }

            var frameworkElement = target as FrameworkElement;
            if (frameworkElement != null)
            {
                AddUiChromeString(frameworkElement.ToolTip, values);
            }

            var dataGrid = target as DataGrid;
            if (dataGrid != null)
            {
                foreach (var column in dataGrid.Columns)
                {
                    AddUiChromeString(column.Header, values);
                }
            }

            foreach (var child in LogicalTreeHelper.GetChildren(target))
            {
                var childObject = child as DependencyObject;
                if (childObject != null)
                {
                    CollectUiChromeStrings(childObject, values);
                }
            }
        }

        private static void AddUiChromeString(
            object candidate,
            ISet<string> values)
        {
            var text = candidate as string;
            if (!string.IsNullOrEmpty(text))
            {
                values.Add(text);
            }
        }

        private static void MainWindowLoadsAndPersistsSelectorLanguage()
        {
            WithTemporaryDirectory(delegate(string root)
            {
                var preferencePath = Path.Combine(
                    root,
                    "UiLanguage",
                    "ui-language.txt");
                UiLanguagePreferenceStore.Save(
                    preferencePath,
                    UiLanguage.Korean);

                MainWindow window = null;
                try
                {
                    window = new MainWindow(root)
                    {
                        ShowActivated = false,
                        ShowInTaskbar = false,
                        WindowStartupLocation = WindowStartupLocation.Manual,
                        Left = -10000,
                        Top = -10000
                    };
                    window.Show();
                    WaitForUiCondition(
                        () => window.IsLoaded,
                        "The localized MainWindow did not load.");

                    var selected =
                        window.ComboUiLanguage.SelectedItem
                            as UiLanguageOption;
                    AssertEx.NotNull(selected);
                    AssertEx.Equal(UiLanguage.Korean, selected.Language);
                    AssertEx.Equal(1, window.ComboUiLanguage.SelectedIndex);
                    AssertEx.Equal("연결", (string)window.ButtonConnect.Content);
                    AssertEx.Equal("연결 끊김", window.TextConnectionState.Text);
                    AssertEx.Equal(
                        "10.10.150.1",
                        window.TextRemoteIp.Text,
                        "Initial Korean localization changed the PLC IP input.");
                    AssertEx.True(
                        BindingOperations.IsDataBound(
                            window.TextCrevisQuickStatus,
                            TextBlock.TextProperty),
                        "Korean startup removed the CREVIS quick-status binding.");
                    window.TextEtherCATTopologySummary.Text =
                        "Connection state";
                    UiLocalizationService.Apply(
                        window,
                        UiLanguage.Korean);
                    PumpUiOnce();
                    AssertEx.Equal(
                        "연결 상태",
                        window.TextEtherCATTopologySummary.Text);
                    AssertEx.Equal(
                        "연결 상태",
                        window.TextCrevisQuickStatus.Text,
                        "The CREVIS quick status stopped following its source after Korean localization.");
                    AssertEx.True(
                        BindingOperations.IsDataBound(
                            window.TextCrevisQuickStatus,
                            TextBlock.TextProperty),
                        "Korean refresh removed the CREVIS quick-status binding.");

                    window.ComboSdoOperation.SelectedItem =
                        window.ComboSdoOperation.Items
                            .Cast<object>()
                            .Single(item => string.Equals(
                                item.ToString(),
                                "Write",
                                StringComparison.Ordinal));
                    PumpUiOnce();
                    AssertEx.Equal(
                        "먼저 동일 값 Qualification 실행",
                        (string)window.ButtonSubmitSdo.Content,
                        "Korean SDO Write mode did not localize its action.");
                    window.TextSdoWriteData.Text = "1";
                    PumpUiOnce();
                    AssertEx.Equal(
                        "먼저 동일 값 Qualification 실행",
                        (string)window.ButtonSubmitSdo.Content,
                        "Editing the Korean SDO request restored an English action caption.");

                    const string operatorInput = "192.0.2.55";
                    window.TextRemoteIp.Text = operatorInput;
                    window.ComboUiLanguage.SelectedIndex = 0;
                    PumpUiOnce();

                    selected = window.ComboUiLanguage.SelectedItem
                        as UiLanguageOption;
                    AssertEx.NotNull(selected);
                    AssertEx.Equal(UiLanguage.English, selected.Language);
                    AssertEx.Equal("Connect", (string)window.ButtonConnect.Content);
                    AssertEx.Equal(
                        "Run Same-Value Qualification First",
                        (string)window.ButtonSubmitSdo.Content,
                        "English restore retained the Korean SDO action caption.");
                    AssertEx.Equal(
                        "Disconnected",
                        window.TextConnectionState.Text);
                    AssertEx.Equal(
                        operatorInput,
                        window.TextRemoteIp.Text,
                        "Changing the MainWindow language changed operator input.");
                    AssertEx.Equal(
                        "en-US",
                        File.ReadAllText(preferencePath, Encoding.UTF8));
                    AssertEx.Equal(
                        UiLanguage.English,
                        UiLanguagePreferenceStore.Load(preferencePath));

                    CloseLocalizedMainWindow(window);
                    window = null;
                }
                finally
                {
                    CloseLocalizedMainWindow(window);
                }
            });
        }

        private static void CloseLocalizedMainWindow(MainWindow window)
        {
            if (window == null || !window.IsLoaded)
            {
                return;
            }

            window.Close();
            WaitForUiCondition(
                () => !window.IsLoaded,
                "The localized MainWindow did not close.");
        }

        private static void WaitForUiCondition(
            Func<bool> condition,
            string failureMessage)
        {
            var deadlineUtc = DateTime.UtcNow.AddSeconds(3);
            while (!condition() && DateTime.UtcNow < deadlineUtc)
            {
                PumpUiOnce();
            }

            AssertEx.True(condition(), failureMessage);
        }

        private static void PumpUiOnce()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() => frame.Continue = false));
            Dispatcher.PushFrame(frame);
        }

        private static void WithTemporaryDirectory(Action<string> body)
        {
            var directory = Path.Combine(
                Path.GetTempPath(),
                "ElmoUiLocalizationTests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                body(directory);
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }
    }
}
