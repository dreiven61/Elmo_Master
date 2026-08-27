using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LasalMotionControlLib.Tests;

namespace LasalMotionControlApiExample
{
    internal static class RecentRecoveryPanelLocalizationTests
    {
        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.UiLocalization.SetOperationMode.HomeDS402ExRecentRecoveryLabelsRoundTrip",
                RecentRecoveryLabelsRoundTrip);
        }

        private static void RecentRecoveryLabelsRoundTrip()
        {
            var root = Path.Combine(
                Path.GetTempPath(),
                "ElmoRecentRecoveryLocalization",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);

            MainWindow window = null;
            try
            {
                var preferencePath = Path.Combine(
                    root,
                    "UiLanguage",
                    "ui-language.txt");
                UiLanguagePreferenceStore.Save(
                    preferencePath,
                    UiLanguage.Korean);

                window = new MainWindow(root)
                {
                    ShowActivated = false,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Left = -10000,
                    Top = -10000
                };
                RecentRecoveryPanelLocalization.Attach(window);
                window.Show();
                PumpUiTwice();

                var selected = window.ComboUiLanguage.SelectedItem
                    as UiLanguageOption;
                AssertEx.NotNull(selected);
                AssertEx.Equal(UiLanguage.Korean, selected.Language);

                var setOperationModeGroup =
                    window.AxisSetOperationModeRecoveryGroupForTests;
                AssertEx.NotNull(setOperationModeGroup);
                AssertEx.Equal(
                    "Operation Mode 설정 - 소프트웨어 target / durable 재전송 방지 복구",
                    setOperationModeGroup.Header as string);
                var setOperationModeText = CollectText(setOperationModeGroup);
                AssertContains(
                    setOperationModeText,
                    "물리 축 번호 (1..4)",
                    "The dynamically created SetOperationMode axis label was not found in Korean UI.");
                AssertContains(
                    setOperationModeText,
                    "PP(1)/PV(3)/IP(7)/CSP(8) 소프트웨어 target이 구현되어 있습니다. PLC capability와 hardware qualification이 완료될 때까지 Production Start는 비활성화됩니다. Homing(6)은 여기서 사용할 수 없습니다.",
                    "The dynamically created SetOperationMode multi-mode warning was not found in Korean UI.");
                AssertContains(
                    setOperationModeText,
                    "Mode capability 새로고침",
                    "The dynamically created SetOperationMode capability button was not localized in Korean UI.");
                AssertContains(
                    setOperationModeText,
                    "복구 조회 / 폐기 (Start 재전송 없음)",
                    "The dynamically created SetOperationMode recovery button was not localized in Korean UI.");

                var homeExGroup = window.AxisDs402HomeExRecoveryGroupForTests;
                AssertEx.NotNull(homeExGroup);
                AssertEx.Equal(
                    "HomeDS402Ex - durable 재전송 방지 복구 (Start UI 닫힘)",
                    homeExGroup.Header as string);
                var homeExText = CollectText(homeExGroup);
                AssertContains(
                    homeExText,
                    "HOMEDS402EX START UI는 제공하지 않습니다. Engineering scale/profile과 LASAL runtime activation은 아직 qualification되지 않았습니다. 이 패널은 이미 durable하게 저장된 정확한 intent만 0x7D1C/0x7D1D로 복구합니다.",
                    "The dynamically created HomeDS402Ex warning text was not localized in Korean UI.");
                AssertContains(
                    homeExText,
                    "HomeEx capability 새로고침",
                    "The dynamically created HomeDS402Ex capability button was not localized in Korean UI.");
                AssertContains(
                    homeExText,
                    "HomeEx 복구 조회 / 폐기 (Start 재전송 없음)",
                    "The dynamically created HomeDS402Ex recovery button was not localized in Korean UI.");

                window.ComboUiLanguage.SelectedIndex = 0;
                PumpUiTwice();

                selected = window.ComboUiLanguage.SelectedItem
                    as UiLanguageOption;
                AssertEx.NotNull(selected);
                AssertEx.Equal(UiLanguage.English, selected.Language);
                AssertEx.Equal(
                    "Set Operation Mode - software target / durable no-replay recovery",
                    setOperationModeGroup.Header as string);
                setOperationModeText = CollectText(setOperationModeGroup);
                AssertContains(
                    setOperationModeText,
                    "Physical axis reference (1..4)",
                    "English restore did not recover the SetOperationMode axis label.");
                AssertContains(
                    setOperationModeText,
                    "PP(1)/PV(3)/IP(7)/CSP(8) software targets are implemented. Production Start remains disabled until PLC capability and hardware qualification are complete. Homing(6) remains unavailable here.",
                    "English restore did not recover the SetOperationMode multi-mode warning.");
                AssertContains(
                    setOperationModeText,
                    "Refresh Mode Capabilities",
                    "English restore did not recover the SetOperationMode capability button.");
                AssertContains(
                    setOperationModeText,
                    "Query / Retire Recovery (No Start Replay)",
                    "English restore did not recover the SetOperationMode recovery button.");

                AssertEx.Equal(
                    "HomeDS402Ex - durable no-replay recovery (Start UI closed)",
                    homeExGroup.Header as string);
                homeExText = CollectText(homeExGroup);
                AssertContains(
                    homeExText,
                    "NO HOMEDS402EX START UI. Engineering scale/profile and LASAL runtime activation are not qualified. This panel only recovers an already durable exact intent through 0x7D1C/0x7D1D.",
                    "English restore did not recover the HomeDS402Ex warning text.");
                AssertContains(
                    homeExText,
                    "Refresh HomeEx Capabilities",
                    "English restore did not recover the HomeDS402Ex capability button.");
                AssertContains(
                    homeExText,
                    "Query / Retire HomeEx Recovery (No Start Replay)",
                    "English restore did not recover the HomeDS402Ex recovery button.");

                window.ComboUiLanguage.SelectedIndex = 1;
                PumpUiTwice();
                setOperationModeText = CollectText(setOperationModeGroup);
                AssertContains(
                    setOperationModeText,
                    "물리 축 번호 (1..4)",
                    "The second Korean pass did not restore the SetOperationMode axis translation.");
                AssertContains(
                    setOperationModeText,
                    "PP(1)/PV(3)/IP(7)/CSP(8) 소프트웨어 target이 구현되어 있습니다. PLC capability와 hardware qualification이 완료될 때까지 Production Start는 비활성화됩니다. Homing(6)은 여기서 사용할 수 없습니다.",
                    "The second Korean pass did not restore the SetOperationMode multi-mode warning translation.");
                AssertContains(
                    setOperationModeText,
                    "Mode capability 새로고침",
                    "The second Korean pass did not restore the SetOperationMode capability-button translation.");

                homeExText = CollectText(homeExGroup);
                AssertContains(
                    homeExText,
                    "HOMEDS402EX START UI는 제공하지 않습니다. Engineering scale/profile과 LASAL runtime activation은 아직 qualification되지 않았습니다. 이 패널은 이미 durable하게 저장된 정확한 intent만 0x7D1C/0x7D1D로 복구합니다.",
                    "The second Korean pass did not restore the HomeDS402Ex warning translation.");
                AssertContains(
                    homeExText,
                    "HomeEx capability 새로고침",
                    "The second Korean pass did not restore the HomeDS402Ex capability-button translation.");
                AssertContains(
                    homeExText,
                    "HomeEx 복구 조회 / 폐기 (Start 재전송 없음)",
                    "The second Korean pass did not restore the HomeDS402Ex recovery-button translation.");
            }
            finally
            {
                if (window != null)
                {
                    try
                    {
                        window.Close();
                        PumpUiTwice();
                    }
                    catch
                    {
                        // The temporary directory cleanup below is best-effort if
                        // a WPF close path reports an unrelated shutdown failure.
                    }
                }

                try
                {
                    if (Directory.Exists(root))
                    {
                        Directory.Delete(root, true);
                    }
                }
                catch
                {
                    // Do not hide the localization assertion result behind a
                    // temporary-directory cleanup failure.
                }
            }
        }

        private static void AssertContains(
            ISet<string> values,
            string expected,
            string message)
        {
            AssertEx.True(values.Contains(expected), message);
        }

        private static HashSet<string> CollectText(DependencyObject root)
        {
            var result = new HashSet<string>(StringComparer.Ordinal);
            var visited = new HashSet<DependencyObject>();
            CollectText(root, result, visited);
            return result;
        }

        private static void CollectText(
            DependencyObject current,
            ISet<string> result,
            ISet<DependencyObject> visited)
        {
            if (current == null || !visited.Add(current))
            {
                return;
            }

            var textBlock = current as TextBlock;
            if (textBlock != null && !string.IsNullOrEmpty(textBlock.Text))
            {
                result.Add(textBlock.Text);
            }

            var contentControl = current as ContentControl;
            if (contentControl != null)
            {
                var stringContent = contentControl.Content as string;
                if (!string.IsNullOrEmpty(stringContent))
                {
                    result.Add(stringContent);
                }

                var dependencyContent =
                    contentControl.Content as DependencyObject;
                if (dependencyContent != null)
                {
                    CollectText(dependencyContent, result, visited);
                }
            }

            var panel = current as Panel;
            if (panel != null)
            {
                foreach (UIElement child in panel.Children)
                {
                    CollectText(child, result, visited);
                }
            }

            foreach (var child in LogicalTreeHelper.GetChildren(current))
            {
                var dependencyChild = child as DependencyObject;
                if (dependencyChild != null)
                {
                    CollectText(dependencyChild, result, visited);
                }
            }
        }

        private static void PumpUiTwice()
        {
            PumpUiOnce();
            PumpUiOnce();
        }

        private static void PumpUiOnce()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.ContextIdle,
                new Action(() => frame.Continue = false));
            Dispatcher.PushFrame(frame);
        }
    }
}
