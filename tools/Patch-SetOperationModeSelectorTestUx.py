from pathlib import Path

source_path = Path('LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.AxisSetOperationModeRecovery.cs')
test_path = Path('LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfAxisSetOperationModeRecoveryIntegrationTests.cs')

source = source_path.read_text(encoding='utf-8')

old_create = '''            comboAxisSetOperationModeRequestedMode = new ComboBox\n            {\n                Width = 220,\n                IsEnabled = false\n            };\n'''
new_create = '''            comboAxisSetOperationModeRequestedMode = new ComboBox\n            {\n                Width = 220\n            };\n            foreach (var mode in new[]\n            {\n                LMCDriveOperationMode.ProfilePosition,\n                LMCDriveOperationMode.ProfileVelocity,\n                LMCDriveOperationMode.InterpolatedPosition,\n                LMCDriveOperationMode.CyclicSynchronousPosition\n            })\n            {\n                comboAxisSetOperationModeRequestedMode.Items.Add(mode);\n            }\n            comboAxisSetOperationModeRequestedMode.SelectedItem =\n                LMCDriveOperationMode.CyclicSynchronousPosition;\n'''
if source.count(old_create) != 1:
    raise SystemExit(f'expected one selector create block, found {source.count(old_create)}')
source = source.replace(old_create, new_create, 1)

old_refresh = '''            comboAxisSetOperationModeRequestedMode.Items.Clear();\n            if (adminCapabilities != null\n                && adminCapabilities.Response != null\n                && adminCapabilities.Response.IsSuccess\n                && adminCapabilities.Supports(\n                    AxisSetOperationModeCapabilityTriad))\n            {\n                foreach (var mode in new[]\n                {\n                    LMCDriveOperationMode.ProfilePosition,\n                    LMCDriveOperationMode.ProfileVelocity,\n                    LMCDriveOperationMode.InterpolatedPosition,\n                    LMCDriveOperationMode.CyclicSynchronousPosition\n                })\n                {\n                    if (adminCapabilities.SupportsSetOperationMode(mode))\n                    {\n                        comboAxisSetOperationModeRequestedMode.Items.Add(mode);\n                    }\n                }\n            }\n'''
new_refresh = '''            comboAxisSetOperationModeRequestedMode.Items.Clear();\n            foreach (var mode in new[]\n            {\n                LMCDriveOperationMode.ProfilePosition,\n                LMCDriveOperationMode.ProfileVelocity,\n                LMCDriveOperationMode.InterpolatedPosition,\n                LMCDriveOperationMode.CyclicSynchronousPosition\n            })\n            {\n                comboAxisSetOperationModeRequestedMode.Items.Add(mode);\n            }\n'''
if source.count(old_refresh) != 1:
    raise SystemExit(f'expected one capability-filtered selector refresh block, found {source.count(old_refresh)}')
source = source.replace(old_refresh, new_refresh, 1)

old_enable = '''            comboAxisSetOperationModeRequestedMode.IsEnabled = idle\n                && !active\n                && triadReady\n                && comboAxisSetOperationModeRequestedMode.Items.Count > 0;\n'''
new_enable = '''            comboAxisSetOperationModeRequestedMode.IsEnabled = idle\n                && !active\n                && comboAxisSetOperationModeRequestedMode.Items.Count > 0;\n'''
if source.count(old_enable) != 1:
    raise SystemExit(f'expected one selector enable block, found {source.count(old_enable)}')
source = source.replace(old_enable, new_enable, 1)

source_path.write_text(source, encoding='utf-8', newline='')

tests = test_path.read_text(encoding='utf-8')
tests = tests.replace(
    'Wpf.SetOperationModeRecovery.SelectorStartsFailClosedWithoutPlcMask',
    'Wpf.SetOperationModeRecovery.SelectorRemainsUsableWithoutPlcMask')
tests = tests.replace(
    'SetOperationModeSelectorStartsFailClosedWithoutPlcMask',
    'SetOperationModeSelectorRemainsUsableWithoutPlcMask')
old_assert = '''                AssertEx.Equal(0, selector.Items.Count);\n                AssertEx.True(selector.SelectedItem == null);\n                AssertEx.False(selector.IsEnabled);\n                AssertEx.False(window.AxisSetOperationModeStartButtonForTests.IsEnabled);\n'''
new_assert = '''                AssertEx.Equal(4, selector.Items.Count);\n                AssertEx.Equal(\n                    LMCDriveOperationMode.CyclicSynchronousPosition,\n                    (LMCDriveOperationMode)selector.SelectedItem);\n                AssertEx.True(selector.IsEnabled);\n                AssertEx.False(window.AxisSetOperationModeStartButtonForTests.IsEnabled);\n'''
if tests.count(old_assert) != 1:
    raise SystemExit(f'expected one selector fail-closed assertion block, found {tests.count(old_assert)}')
tests = tests.replace(old_assert, new_assert, 1)
test_path.write_text(tests, encoding='utf-8', newline='')

print('SetOperationMode selector UX patch applied: selector remains usable, Start remains capability-gated.')
