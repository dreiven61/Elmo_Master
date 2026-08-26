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
                    "Operation Mode 설정 - CSP=8 / durable 재전송 방지 복구",
                    setOperationModeGroup.Header as string);
                var setOperationModeText = CollectText(setOperationModeGroup);
                AssertEx.True(
                    setOperationModeText.Contains("물리 축 번호 (1..4)"),
                    "The dynamically created SetOperationMode axis label remained English in Korean UI.");
                AssertEx.True(
                    setOperationModeText.Contains("CSP 위치 동기 모드 (8)"),
                    "The dynamically created SetOperationMode CSP label remained English in Korean UI.");

                var homeExGroup = window.AxisDs402HomeExRecoveryGroupForTests;
                AssertEx.NotNull(homeExGroup);
                AssertEx.Equal(
                    "HomeDS402Ex - durable 재전송 방지 복구 (Start UI 닫힘)",
                    homeExGroup.Header as string);

                window.ComboUiLanguage.SelectedIndex = 0;
                PumpUiTwice();

                selected = window.ComboUiLanguage.SelectedItem
                    as UiLanguageOption;
                AssertEx.NotNull(selected);
                AssertEx.Equal(UiLanguage.English, selected.Language);
                AssertEx.Equal(
                    "Set Operation Mode - CSP=8 / durable no-replay recovery",
                    setOperationModeGroup.Header as string);
                setOperationModeText = CollectText(setOperationModeGroup);
                AssertEx.True(
                    setOperationModeText.Contains("Physical axis reference (1..4)"),
                    "English restore did not recover the SetOperationMode axis label.");
                AssertEx.True(
                    setOperationModeText.Contains("CyclicSynchronousPosition (8)"),
                    "English restore did not recover the SetOperationMode CSP label.");
                AssertEx.Equal(
                    "HomeDS402Ex - durable no-replay recovery (Start UI closed)",
                    homeExGroup.Header as string);

                window.ComboUiLanguage.SelectedIndex = 1;
                PumpUiTwice();
                setOperationModeText = CollectText(setOperationModeGroup);
                AssertEx.True(
                    setOperationModeText.Contains("물리 축 번호 (1..4)"),
                    "The second Korean pass did not restore the SetOperationMode axis translation.");
                AssertEx.True(
                    setOperationModeText.Contains("CSP 위치 동기 모드 (8)"),
                    "The second Korean pass did not restore the SetOperationMode CSP translation.");
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

        private static HashSet<string> CollectText(DependencyObject root)
        {
            var result = new HashSet<string>(StringComparer.Ordinal);
            CollectText(root, result);
            return result;
        }

        private static void CollectText(
            DependencyObject current,
            ISet<string> result)
        {
            if (current == null)
            {
                return;
            }

            var textBlock = current as TextBlock;
            if (textBlock != null && !string.IsNullOrEmpty(textBlock.Text))
            {
                result.Add(textBlock.Text);
            }

            foreach (var child in LogicalTreeHelper.GetChildren(current))
            {
                var dependencyChild = child as DependencyObject;
                if (dependencyChild != null)
                {
                    CollectText(dependencyChild, result);
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
