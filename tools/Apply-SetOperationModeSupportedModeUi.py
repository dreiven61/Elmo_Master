from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def read(rel):
    return (ROOT / rel).read_text(encoding="utf-8")


def write(rel, text):
    (ROOT / rel).write_text(text, encoding="utf-8", newline="")


def replace_once(rel, old, new):
    text = read(rel)
    count = text.count(old)
    if count != 1:
        raise RuntimeError(f"{rel}: expected one match, got {count}: {old[:140]!r}")
    write(rel, text.replace(old, new, 1))


# SDK: a new Start must be supported by the exact current capability observation.
admin = "LMC_Library/LMC_API_Delivery/src/LmcAdminSetOperationMode.cs"
replace_once(
    admin,
    "            ValidateAxisSetOperationModeCapabilities(\n                verifiedCapabilities,\n                sessionGeneration,\n                axis.AxisReference,\n                true);\n            ValidateAxisSetPositionDiagnosticCapabilities(",
    "            ValidateAxisSetOperationModeCapabilities(\n                verifiedCapabilities,\n                sessionGeneration,\n                axis.AxisReference,\n                true);\n            if (!verifiedCapabilities.SupportsSetOperationMode(requestedMode))\n            {\n                throw new NotSupportedException(\n                    \"The connected PLC does not advertise the requested SetOperationMode target in SetOperationModeSupportedMask.\");\n            }\n            ValidateAxisSetPositionDiagnosticCapabilities(")

wpf = "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.AxisSetOperationModeRecovery.cs"
replace_once(
    wpf,
    "        private ComboBox comboAxisSetOperationModeReference;\n        private TextBox textAxisSetOperationModeTimeout;",
    "        private ComboBox comboAxisSetOperationModeReference;\n        private ComboBox comboAxisSetOperationModeRequestedMode;\n        private TextBox textAxisSetOperationModeTimeout;")
replace_once(
    wpf,
    "        internal CheckBox AxisSetOperationModeConfirmationForTests\n        {\n            get { return checkAxisSetOperationModeOneShotConfirmed; }\n        }\n\n        internal void RefreshAxisSetOperationModeRecoveryUiForTests()",
    "        internal CheckBox AxisSetOperationModeConfirmationForTests\n        {\n            get { return checkAxisSetOperationModeOneShotConfirmed; }\n        }\n\n        internal ComboBox AxisSetOperationModeRequestedModeForTests\n        {\n            get { return comboAxisSetOperationModeRequestedMode; }\n        }\n\n        internal void RefreshAxisSetOperationModeRecoveryUiForTests()")
replace_once(
    wpf,
    "                    comboAxisSetOperationModeReference.SelectedItem =\n                        record.AxisReference;\n                    textAxisSetOperationModeTimeout.Text =",
    "                    comboAxisSetOperationModeReference.SelectedItem =\n                        record.AxisReference;\n                    var recoveredMode =\n                        (LMCDriveOperationMode)record.RequestedModeRaw;\n                    if (!comboAxisSetOperationModeRequestedMode.Items.Contains(\n                            recoveredMode))\n                    {\n                        comboAxisSetOperationModeRequestedMode.Items.Add(\n                            recoveredMode);\n                    }\n                    comboAxisSetOperationModeRequestedMode.SelectedItem =\n                        recoveredMode;\n                    textAxisSetOperationModeTimeout.Text =")
replace_once(
    wpf,
    "                Header = \"Set Operation Mode - CSP=8 / durable no-replay recovery\"",
    "                Header = \"Set Operation Mode - PLC-supported target / durable no-replay recovery\"")
replace_once(
    wpf,
    "                    + \"128-bit ClientIntentId + RequestId + axis + requested CSP mode. \"",
    "                    + \"128-bit ClientIntentId + RequestId + axis + requested mode. \"")
old_mode = '''            var modePanel = new StackPanel { Width = 220 };\n            modePanel.Children.Add(new TextBlock\n            {\n                Text = "Requested mode",\n                Foreground = Brushes.DimGray\n            });\n            modePanel.Children.Add(new TextBlock\n            {\n                Margin = new Thickness(0, 7, 12, 8),\n                FontWeight = FontWeights.SemiBold,\n                Text = "CyclicSynchronousPosition (8)"\n            });\n            inputs.Children.Add(modePanel);\n'''
new_mode = '''            var modePanel = new StackPanel { Width = 240 };\n            modePanel.Children.Add(new TextBlock\n            {\n                Text = "Requested mode (PLC-advertised only)",\n                Foreground = Brushes.DimGray\n            });\n            comboAxisSetOperationModeRequestedMode = new ComboBox\n            {\n                Width = 220,\n                IsEnabled = false\n            };\n            comboAxisSetOperationModeRequestedMode.SelectionChanged +=\n                AxisSetOperationModeInputChanged;\n            modePanel.Children.Add(comboAxisSetOperationModeRequestedMode);\n            modePanel.Children.Add(new TextBlock\n            {\n                Margin = new Thickness(0, 4, 12, 4),\n                Foreground = Brushes.DarkOrange,\n                TextWrapping = TextWrapping.Wrap,\n                Text = "Software targets are limited to PP(1), PV(3), IP(7), and CSP(8). "\n                    + "The selector stays empty until the connected PLC advertises a supported-mode mask. "\n                    + "Homing(6) remains owned by HomeDS402/HomeDS402Ex."\n            });\n            inputs.Children.Add(modePanel);\n'''
replace_once(wpf, old_mode, new_mode)
replace_once(
    wpf,
    "                    Text = \"I verified the exact powered drive/axis and understand that this writes DS402 0x6060:0 to CSP=8 once only. \"\n                        + \"If the response or completion is uncertain I will use the durable recovery query and will not send Start again.\"",
    "                    Text = \"I verified the exact drive/axis and understand that this may write DS402 0x6060:0 to the selected PLC-advertised mode once only. \"\n                        + \"If the response or completion is uncertain I will use the durable recovery query and will not send Start again.\"")
replace_once(
    wpf,
    "                Content = \"Start CSP Once (0x7D23)\",",
    "                Content = \"Start Selected Mode Once (0x7D23)\",")

insert_before_update = '''        private void RefreshAxisSetOperationModeSupportedModeSelector(\n            LMCDriveOperationMode? preferredMode = null)\n        {\n            if (comboAxisSetOperationModeRequestedMode == null\n                || HasActiveAxisSetOperationModeRecoveryRecord)\n            {\n                return;\n            }\n\n            LMCDriveOperationMode? previous = preferredMode;\n            if (!previous.HasValue\n                && comboAxisSetOperationModeRequestedMode.SelectedItem\n                    is LMCDriveOperationMode)\n            {\n                previous = (LMCDriveOperationMode)\n                    comboAxisSetOperationModeRequestedMode.SelectedItem;\n            }\n\n            comboAxisSetOperationModeRequestedMode.Items.Clear();\n            if (adminCapabilities != null\n                && adminCapabilities.Response != null\n                && adminCapabilities.Response.IsSuccess\n                && adminCapabilities.Supports(\n                    AxisSetOperationModeCapabilityTriad))\n            {\n                foreach (var mode in new[]\n                {\n                    LMCDriveOperationMode.ProfilePosition,\n                    LMCDriveOperationMode.ProfileVelocity,\n                    LMCDriveOperationMode.InterpolatedPosition,\n                    LMCDriveOperationMode.CyclicSynchronousPosition\n                })\n                {\n                    if (adminCapabilities.SupportsSetOperationMode(mode))\n                    {\n                        comboAxisSetOperationModeRequestedMode.Items.Add(mode);\n                    }\n                }\n            }\n\n            if (previous.HasValue\n                && comboAxisSetOperationModeRequestedMode.Items.Contains(\n                    previous.Value))\n            {\n                comboAxisSetOperationModeRequestedMode.SelectedItem =\n                    previous.Value;\n            }\n            else if (comboAxisSetOperationModeRequestedMode.Items.Contains(\n                LMCDriveOperationMode.CyclicSynchronousPosition))\n            {\n                comboAxisSetOperationModeRequestedMode.SelectedItem =\n                    LMCDriveOperationMode.CyclicSynchronousPosition;\n            }\n            else if (comboAxisSetOperationModeRequestedMode.Items.Count > 0)\n            {\n                comboAxisSetOperationModeRequestedMode.SelectedIndex = 0;\n            }\n        }\n\n'''
replace_once(
    wpf,
    "        private void UpdateAxisSetOperationModeRecoveryUiState(\n",
    insert_before_update + "        private void UpdateAxisSetOperationModeRecoveryUiState(\n")
replace_once(
    wpf,
    "            var axisSelected = comboAxisSetOperationModeReference != null\n                && comboAxisSetOperationModeReference.SelectedItem is ushort;\n            var admissionAllowed = !active",
    "            var axisSelected = comboAxisSetOperationModeReference != null\n                && comboAxisSetOperationModeReference.SelectedItem is ushort;\n            var modeSelected = comboAxisSetOperationModeRequestedMode != null\n                && comboAxisSetOperationModeRequestedMode.SelectedItem\n                    is LMCDriveOperationMode\n                && adminCapabilities != null\n                && adminCapabilities.SupportsSetOperationMode(\n                    (LMCDriveOperationMode)\n                        comboAxisSetOperationModeRequestedMode.SelectedItem);\n            var admissionAllowed = !active")
replace_once(
    wpf,
    "                && timeoutValid\n                && axisSelected;",
    "                && timeoutValid\n                && axisSelected\n                && modeSelected;")
replace_once(
    wpf,
    "            comboAxisSetOperationModeReference.IsEnabled = idle && !active;\n            textAxisSetOperationModeTimeout.IsEnabled = idle && !active;",
    "            comboAxisSetOperationModeReference.IsEnabled = idle && !active;\n            comboAxisSetOperationModeRequestedMode.IsEnabled = idle\n                && !active\n                && triadReady\n                && comboAxisSetOperationModeRequestedMode.Items.Count > 0;\n            textAxisSetOperationModeTimeout.IsEnabled = idle && !active;")
replace_once(
    wpf,
    "                    adminCapabilities = await currentConnection.Admin\n                        .GetCapabilitiesAsync(CancellationToken.None);\n                    await RefreshDiagnosticsCapabilitiesAsync(\n                        currentConnection);\n                    RefreshAxisSetOperationModeRecoveryUi();",
    "                    adminCapabilities = await currentConnection.Admin\n                        .GetCapabilitiesAsync(CancellationToken.None);\n                    RefreshAxisSetOperationModeSupportedModeSelector();\n                    await RefreshDiagnosticsCapabilitiesAsync(\n                        currentConnection);\n                    RefreshAxisSetOperationModeRecoveryUi();")
replace_once(
    wpf,
    "                \"Set Operation Mode CSP Once\",\n                StartAxisSetOperationModeOnceAsync);",
    "                \"Set Operation Mode Selected Mode Once\",\n                StartAxisSetOperationModeOnceAsync);")
replace_once(
    wpf,
    "            var timeoutMilliseconds = RequireAxisSetOperationModeTimeout();\n            var axisReference = RequireAxisSetOperationModeAxisReference();\n            var currentConnection = RequireConnection();",
    "            var timeoutMilliseconds = RequireAxisSetOperationModeTimeout();\n            var axisReference = RequireAxisSetOperationModeAxisReference();\n            if (comboAxisSetOperationModeRequestedMode == null\n                || !(comboAxisSetOperationModeRequestedMode.SelectedItem\n                    is LMCDriveOperationMode))\n            {\n                throw new InvalidOperationException(\n                    \"Refresh capabilities and select a PLC-advertised SetOperationMode target first.\");\n            }\n            var requestedMode = (LMCDriveOperationMode)\n                comboAxisSetOperationModeRequestedMode.SelectedItem;\n            var currentConnection = RequireConnection();")
replace_once(
    wpf,
    "            adminCapabilities = await currentConnection.Admin\n                .GetCapabilitiesAsync(CancellationToken.None);\n            await RefreshDiagnosticsCapabilitiesAsync(currentConnection);\n            EnsureAxisSetOperationModeCapabilitiesReady(\n                \"SetOperationMode Start\");",
    "            adminCapabilities = await currentConnection.Admin\n                .GetCapabilitiesAsync(CancellationToken.None);\n            RefreshAxisSetOperationModeSupportedModeSelector(requestedMode);\n            if (!adminCapabilities.SupportsSetOperationMode(requestedMode))\n            {\n                throw new NotSupportedException(\n                    \"The connected PLC no longer advertises the selected SetOperationMode target. No Start was sent.\");\n            }\n            await RefreshDiagnosticsCapabilitiesAsync(currentConnection);\n            EnsureAxisSetOperationModeCapabilitiesReady(\n                \"SetOperationMode Start\");")
replace_once(
    wpf,
    "            var prepared = currentAxis.PrepareSetOperationMode(\n                LMCDriveOperationMode.CyclicSynchronousPosition,",
    "            var prepared = currentAxis.PrepareSetOperationMode(\n                requestedMode,")
replace_once(
    wpf,
    "                \"Journal ready; no unresolved record. AdminTriad=\"\n                + triad\n                + \", DiagnosticsIdentity=\"",
    "                \"Journal ready; no unresolved record. AdminTriad=\"\n                + triad\n                + \", SupportedModeMask=0x\"\n                + (adminCapabilities == null\n                    ? 0\n                    : adminCapabilities.SetOperationModeSupportedMask)\n                    .ToString(\"X4\")\n                + \", DiagnosticsIdentity=\"")

# The secondary read-only API path uses the same operation name.
read_only = "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.ReadOnlyApi.cs"
replace_once(
    read_only,
    "                \"Set Operation Mode CSP Once\",",
    "                \"Set Operation Mode Selected Mode Once\",")

# Localization for the new dynamic UI strings.
localization = "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/UiLocalization.cs"
replace_once(
    localization,
    "            values[\"Ready\"] = \"준비됨\";\n\n            AddStaticChromeTranslations(values);",
    "            values[\"Ready\"] = \"준비됨\";\n\n            values[\"Set Operation Mode - PLC-supported target / durable no-replay recovery\"] =\n                \"Operation Mode 설정 - PLC 지원 target / durable 재전송 방지 복구\";\n            values[\"Requested mode (PLC-advertised only)\"] =\n                \"요청 mode (PLC 광고 항목만)\";\n            values[\"Software targets are limited to PP(1), PV(3), IP(7), and CSP(8). The selector stays empty until the connected PLC advertises a supported-mode mask. Homing(6) remains owned by HomeDS402/HomeDS402Ex.\"] =\n                \"소프트웨어 target은 PP(1), PV(3), IP(7), CSP(8)로 제한됩니다. 연결된 PLC가 supported-mode mask를 광고하기 전까지 selector는 비어 있습니다. Homing(6)은 HomeDS402/HomeDS402Ex가 계속 소유합니다.\";\n            values[\"I verified the exact drive/axis and understand that this may write DS402 0x6060:0 to the selected PLC-advertised mode once only. If the response or completion is uncertain I will use the durable recovery query and will not send Start again.\"] =\n                \"정확한 drive/축을 확인했으며 PLC가 광고한 선택 mode를 DS402 0x6060:0에 한 번만 쓸 수 있음을 이해했습니다. 응답 또는 완료가 불확실하면 durable 복구 조회만 사용하고 Start를 다시 전송하지 않습니다.\";\n            values[\"Start Selected Mode Once (0x7D23)\"] =\n                \"선택 Mode 1회 시작 (0x7D23)\";\n            values[\"Set Operation Mode Selected Mode Once\"] =\n                \"Operation Mode 선택 Mode 1회 설정\";\n\n            AddStaticChromeTranslations(values);")

# Localization test expectations.
loc_test = "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/RecentRecoveryPanelLocalizationTests.cs"
replace_once(
    loc_test,
    "                    \"Operation Mode 설정 - CSP=8 / durable 재전송 방지 복구\",",
    "                    \"Operation Mode 설정 - PLC 지원 target / durable 재전송 방지 복구\",")
replace_once(
    loc_test,
    "                    \"CSP 위치 동기 모드 (8)\",\n                    \"The dynamically created SetOperationMode CSP label was not found in Korean UI.\");",
    "                    \"소프트웨어 target은 PP(1), PV(3), IP(7), CSP(8)로 제한됩니다. 연결된 PLC가 supported-mode mask를 광고하기 전까지 selector는 비어 있습니다. Homing(6)은 HomeDS402/HomeDS402Ex가 계속 소유합니다.\",\n                    \"The dynamically created SetOperationMode supported-mode warning was not found in Korean UI.\");")
replace_once(
    loc_test,
    "                    \"Set Operation Mode - CSP=8 / durable no-replay recovery\",",
    "                    \"Set Operation Mode - PLC-supported target / durable no-replay recovery\",")
replace_once(
    loc_test,
    "                    \"CyclicSynchronousPosition (8)\",\n                    \"English restore did not recover the SetOperationMode CSP label.\");",
    "                    \"Software targets are limited to PP(1), PV(3), IP(7), and CSP(8). The selector stays empty until the connected PLC advertises a supported-mode mask. Homing(6) remains owned by HomeDS402/HomeDS402Ex.\",\n                    \"English restore did not recover the SetOperationMode supported-mode warning.\");")
replace_once(
    loc_test,
    "                    \"CSP 위치 동기 모드 (8)\",\n                    \"The second Korean pass did not restore the SetOperationMode CSP translation.\");",
    "                    \"소프트웨어 target은 PP(1), PV(3), IP(7), CSP(8)로 제한됩니다. 연결된 PLC가 supported-mode mask를 광고하기 전까지 selector는 비어 있습니다. Homing(6)은 HomeDS402/HomeDS402Ex가 계속 소유합니다.\",\n                    \"The second Korean pass did not restore the SetOperationMode supported-mode warning translation.\");")

# WPF smoke: selector must start fail-closed before a current PLC capability observation.
wpf_test = "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfAxisSetOperationModeRecoveryIntegrationTests.cs"
replace_once(
    wpf_test,
    "            tests.Add(\n                \"Wpf.SetOperationModeRecovery.DynamicUiRequiresExplicitConfirmation\",\n                SetOperationModeDynamicUiRequiresExplicitConfirmation);",
    "            tests.Add(\n                \"Wpf.SetOperationModeRecovery.DynamicUiRequiresExplicitConfirmation\",\n                SetOperationModeDynamicUiRequiresExplicitConfirmation);\n            tests.Add(\n                \"Wpf.SetOperationModeRecovery.SelectorStartsFailClosedWithoutPlcMask\",\n                SetOperationModeSelectorStartsFailClosedWithoutPlcMask);")
replace_once(
    wpf_test,
    "        private static void\n            SetOperationModeDefinitiveRejectArchivesAndClearsInterlock()",
    "        private static void SetOperationModeSelectorStartsFailClosedWithoutPlcMask()\n        {\n            var root = CreateSetOperationModeTemporaryDirectory();\n            MainWindow window = null;\n            try\n            {\n                window = new MainWindow(root);\n                var selector = window.AxisSetOperationModeRequestedModeForTests;\n                AssertEx.NotNull(selector);\n                AssertEx.Equal(0, selector.Items.Count);\n                AssertEx.True(selector.SelectedItem == null);\n                AssertEx.False(selector.IsEnabled);\n                AssertEx.False(window.AxisSetOperationModeStartButtonForTests.IsEnabled);\n            }\n            finally\n            {\n                if (window != null)\n                {\n                    window.Close();\n                }\n                DeleteSetOperationModeTemporaryDirectory(root);\n            }\n        }\n\n        private static void\n            SetOperationModeDefinitiveRejectArchivesAndClearsInterlock()")

print("SetOperationMode SupportedModeMask SDK/WPF promotion applied.")
