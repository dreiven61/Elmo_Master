from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def replace_exact(path, old, new, expected=1):
    file_path = ROOT / path
    raw = file_path.read_bytes()
    text = raw.decode("utf-8")
    newline = "\r\n" if "\r\n" in text else "\n"
    normalized = text.replace("\r\n", "\n").replace("\r", "\n")
    old_n = old.replace("\r\n", "\n").replace("\r", "\n")
    new_n = new.replace("\r\n", "\n").replace("\r", "\n")
    actual = normalized.count(old_n)
    if actual != expected:
        raise RuntimeError(
            f"replacement count mismatch: {path}: expected={expected}, actual={actual}, old={old!r}"
        )
    normalized = normalized.replace(old_n, new_n)
    if newline == "\r\n":
        normalized = normalized.replace("\n", "\r\n")
    file_path.write_bytes(normalized.encode("utf-8"))


diag = "Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st"
sdk = "LMC_Library/LMC_API_Delivery/src/LmcAdminSetOperationModeProtocol.cs"
wpf = "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.AxisSetOperationModeRecovery.cs"
readonly = "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.ReadOnlyApi.cs"
sdk_tests = "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/AdminSetOperationModeContractTests.cs"
wpf_tests = "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfAxisSetOperationModeRecoveryIntegrationTests.cs"
prepare = "tools/Prepare-SetOperationModeMode11Bench.ps1"
candidate = "tools/Verify-SetOperationModeMode11Candidate.ps1"

# ---------------------------------------------------------------------------
# PLC qualification branch: explicit PP/PV/IP/CSP allow-list.
# Production dev never receives this define or these broader guards.
# ---------------------------------------------------------------------------
replace_exact(
    diag,
    "#define LMC_DIAG_SET_OPERATION_MODE_ENABLED TRUE",
    "#define LMC_DIAG_SET_OPERATION_MODE_ENABLED TRUE\n"
    "#define LMC_DIAG_SET_OPERATION_MODE_BENCH_PRECONDITION_MODES TRUE",
)

replace_exact(
    diag,
    "\telsif requestedMode <> 8 then\n"
    "\t\tdetailCode := LMC_DIAG_MODE_DETAIL_UNSUPPORTED;",
    "\telsif (requestedMode <> 8) &\n"
    "\t      ((LMC_DIAG_SET_OPERATION_MODE_BENCH_PRECONDITION_MODES = FALSE) |\n"
    "\t       ((requestedMode <> 1) & (requestedMode <> 3) & (requestedMode <> 7))) then\n"
    "\t\tdetailCode := LMC_DIAG_MODE_DETAIL_UNSUPPORTED;",
)
replace_exact(
    diag,
    "\telsif requestedMode <> 8 then\n"
    "\t\tdetailCode := LMC_DIAG_MODE_DETAIL_KEY_MISMATCH;",
    "\telsif (requestedMode <> 8) &\n"
    "\t      ((LMC_DIAG_SET_OPERATION_MODE_BENCH_PRECONDITION_MODES = FALSE) |\n"
    "\t       ((requestedMode <> 1) & (requestedMode <> 3) & (requestedMode <> 7))) then\n"
    "\t\tdetailCode := LMC_DIAG_MODE_DETAIL_KEY_MISMATCH;",
)

replace_exact(
    diag,
    "\tAxisOperationModeState[LMC_DIAG_MODE_RUNTIME_TIMEOUT_MS]$UDINT :=\n"
    "\t\ttimeoutMilliseconds;\n"
    "\tAxisOperationModeState[LMC_DIAG_MODE_RUNTIME_WRITE_DATA] := 8;",
    "\tAxisOperationModeState[LMC_DIAG_MODE_RUNTIME_TIMEOUT_MS]$UDINT :=\n"
    "\t\ttimeoutMilliseconds;\n"
    "\tAxisOperationModeState[LMC_DIAG_MODE_RUNTIME_WRITE_DATA] := TO_DINT(requestedMode);",
)
replace_exact(
    diag,
    "\t\t\t\tAxisOperationModeState[LMC_DIAG_MODE_RUNTIME_TIMEOUT_MS]$UDINT :=\n"
    "\t\t\t\t\tLMC_DIAG_MODE_RECOVERY_READ_TIMEOUT_MS;\n"
    "\t\t\t\tAxisOperationModeState[LMC_DIAG_MODE_RUNTIME_WRITE_DATA] := 8;",
    "\t\t\t\tAxisOperationModeState[LMC_DIAG_MODE_RUNTIME_TIMEOUT_MS]$UDINT :=\n"
    "\t\t\t\t\tLMC_DIAG_MODE_RECOVERY_READ_TIMEOUT_MS;\n"
    "\t\t\t\tAxisOperationModeState[LMC_DIAG_MODE_RUNTIME_WRITE_DATA] :=\n"
    "\t\t\t\t\tAxisOperationModeState[recoveryScanBase + 10];",
)
replace_exact(
    diag,
    "\t\t\t\tif observedMode = 8 then",
    "\t\t\t\tif observedMode = AxisOperationModeState[recordBase + 10]$SINT then",
)

# ---------------------------------------------------------------------------
# SDK wire identity: keep exact allow-list and keep Homing under Home APIs.
# ---------------------------------------------------------------------------
replace_exact(
    sdk,
    "            if (requestedMode\n"
    "                != LMCDriveOperationMode.CyclicSynchronousPosition)\n"
    "            {\n"
    "                throw new NotSupportedException(\n"
    "                    \"SetOperationMode schema version 1 supports CSP mode 8 recovery only.\");\n"
    "            }",
    "            var qualificationModeAllowed = requestedMode\n"
    "                    == LMCDriveOperationMode.CyclicSynchronousPosition\n"
    "                || requestedMode == LMCDriveOperationMode.ProfilePosition\n"
    "                || requestedMode == LMCDriveOperationMode.ProfileVelocity\n"
    "                || requestedMode == LMCDriveOperationMode.InterpolatedPosition;\n"
    "            if (!qualificationModeAllowed)\n"
    "            {\n"
    "                throw new NotSupportedException(\n"
    "                    \"This MODE-11 qualification branch supports only PP(1), PV(3), IP(7), and CSP(8). \"\n"
    "                    + \"Homing(6) remains owned by HomeDS402/HomeDS402Ex.\");\n"
    "            }",
)

# ---------------------------------------------------------------------------
# WPF: selectable qualification target, still one-shot/no-replay.
# ---------------------------------------------------------------------------
replace_exact(
    wpf,
    "        private ComboBox comboAxisSetOperationModeReference;",
    "        private ComboBox comboAxisSetOperationModeReference;\n"
    "        private ComboBox comboAxisSetOperationModeRequestedMode;",
)
replace_exact(
    wpf,
    "        internal CheckBox AxisSetOperationModeConfirmationForTests\n"
    "        {\n"
    "            get { return checkAxisSetOperationModeOneShotConfirmed; }\n"
    "        }",
    "        internal CheckBox AxisSetOperationModeConfirmationForTests\n"
    "        {\n"
    "            get { return checkAxisSetOperationModeOneShotConfirmed; }\n"
    "        }\n\n"
    "        internal ComboBox AxisSetOperationModeRequestedModeForTests\n"
    "        {\n"
    "            get { return comboAxisSetOperationModeRequestedMode; }\n"
    "        }",
)
replace_exact(
    wpf,
    "                    comboAxisSetOperationModeReference.SelectedItem =\n"
    "                        record.AxisReference;\n"
    "                    textAxisSetOperationModeTimeout.Text =",
    "                    comboAxisSetOperationModeReference.SelectedItem =\n"
    "                        record.AxisReference;\n"
    "                    comboAxisSetOperationModeRequestedMode.SelectedItem =\n"
    "                        (LMCDriveOperationMode)record.RequestedModeRaw;\n"
    "                    textAxisSetOperationModeTimeout.Text =",
)
replace_exact(
    wpf,
    '                Header = "Set Operation Mode - CSP=8 / durable no-replay recovery"',
    '                Header = "Set Operation Mode - bench target / durable no-replay recovery"',
)
replace_exact(
    wpf,
    '                    + "128-bit ClientIntentId + RequestId + axis + requested CSP mode. "',
    '                    + "128-bit ClientIntentId + RequestId + axis + requested mode. "',
)
replace_exact(
    wpf,
    "            modePanel.Children.Add(new TextBlock\n"
    "            {\n"
    "                Margin = new Thickness(0, 7, 12, 8),\n"
    "                FontWeight = FontWeights.SemiBold,\n"
    "                Text = \"CyclicSynchronousPosition (8)\"\n"
    "            });\n"
    "            inputs.Children.Add(modePanel);",
    "            comboAxisSetOperationModeRequestedMode = new ComboBox\n"
    "            {\n"
    "                Width = 205,\n"
    "                ItemsSource = new[]\n"
    "                {\n"
    "                    LMCDriveOperationMode.ProfilePosition,\n"
    "                    LMCDriveOperationMode.ProfileVelocity,\n"
    "                    LMCDriveOperationMode.InterpolatedPosition,\n"
    "                    LMCDriveOperationMode.CyclicSynchronousPosition\n"
    "                },\n"
    "                SelectedItem = LMCDriveOperationMode.CyclicSynchronousPosition\n"
    "            };\n"
    "            comboAxisSetOperationModeRequestedMode.SelectionChanged +=\n"
    "                AxisSetOperationModeInputChanged;\n"
    "            modePanel.Children.Add(comboAxisSetOperationModeRequestedMode);\n"
    "            modePanel.Children.Add(new TextBlock\n"
    "            {\n"
    "                Margin = new Thickness(0, 4, 12, 4),\n"
    "                Foreground = Brushes.DarkOrange,\n"
    "                TextWrapping = TextWrapping.Wrap,\n"
    "                Text = \"PP(1)/PV(3)/IP(7): BENCH PRECONDITION ONLY. \"\n"
    "                    + \"Keep the axis operation-disabled and do not run ordinary motion; \"\n"
    "                    + \"return to CSP(8) before motion. Homing(6) remains unavailable here.\"\n"
    "            });\n"
    "            inputs.Children.Add(modePanel);",
)
replace_exact(
    wpf,
    '                    Text = "I verified the exact powered drive/axis and understand that this writes DS402 0x6060:0 to CSP=8 once only. "\n'
    '                        + "If the response or completion is uncertain I will use the durable recovery query and will not send Start again."',
    '                    Text = "I verified the exact drive/axis and understand that this may write DS402 0x6060:0 to the selected qualification mode once only. "\n'
    '                        + "PP/PV/IP are precondition states only; I will keep the axis operation-disabled and return to CSP(8) before motion. "\n'
    '                        + "If the response or completion is uncertain I will use the durable recovery query and will not send Start again."',
)
replace_exact(
    wpf,
    '                Content = "Start CSP Once (0x7D23)",',
    '                Content = "Start Selected Mode Once (0x7D23)",',
)
replace_exact(
    wpf,
    "            var axisSelected = comboAxisSetOperationModeReference != null\n"
    "                && comboAxisSetOperationModeReference.SelectedItem is ushort;",
    "            var axisSelected = comboAxisSetOperationModeReference != null\n"
    "                && comboAxisSetOperationModeReference.SelectedItem is ushort;\n"
    "            var modeSelected = comboAxisSetOperationModeRequestedMode != null\n"
    "                && comboAxisSetOperationModeRequestedMode.SelectedItem\n"
    "                    is LMCDriveOperationMode;",
)
replace_exact(
    wpf,
    "                && timeoutValid\n"
    "                && axisSelected;",
    "                && timeoutValid\n"
    "                && axisSelected\n"
    "                && modeSelected;",
)
replace_exact(
    wpf,
    "            comboAxisSetOperationModeReference.IsEnabled = idle && !active;",
    "            comboAxisSetOperationModeReference.IsEnabled = idle && !active;\n"
    "            comboAxisSetOperationModeRequestedMode.IsEnabled = idle && !active;",
)
replace_exact(
    wpf,
    '                "Set Operation Mode CSP Once",',
    '                "Set Operation Mode Selected Mode Once",',
)
replace_exact(
    wpf,
    "            var timeoutMilliseconds = RequireAxisSetOperationModeTimeout();\n"
    "            var axisReference = RequireAxisSetOperationModeAxisReference();\n"
    "            var currentConnection = RequireConnection();",
    "            var timeoutMilliseconds = RequireAxisSetOperationModeTimeout();\n"
    "            var axisReference = RequireAxisSetOperationModeAxisReference();\n"
    "            if (comboAxisSetOperationModeRequestedMode == null\n"
    "                || !(comboAxisSetOperationModeRequestedMode.SelectedItem\n"
    "                    is LMCDriveOperationMode))\n"
    "            {\n"
    "                throw new InvalidOperationException(\n"
    "                    \"A supported SetOperationMode qualification target is required.\");\n"
    "            }\n"
    "            var requestedMode = (LMCDriveOperationMode)\n"
    "                comboAxisSetOperationModeRequestedMode.SelectedItem;\n"
    "            var currentConnection = RequireConnection();",
)
replace_exact(
    wpf,
    "                LMCDriveOperationMode.CyclicSynchronousPosition,\n"
    "                timeoutMilliseconds,",
    "                requestedMode,\n"
    "                timeoutMilliseconds,",
)
replace_exact(
    readonly,
    '                "Set Operation Mode CSP Once",',
    '                "Set Operation Mode Selected Mode Once",',
)

# ---------------------------------------------------------------------------
# SDK tests: positive exact allow-list + explicit blocked modes.
# ---------------------------------------------------------------------------
replace_exact(
    sdk_tests,
    '                "Contract.Admin.SetOperationMode.CspOnlyImmediate",\n'
    "                CspOnlyImmediate);",
    '                "Contract.Admin.SetOperationMode.BenchAllowListImmediate",\n'
    "                BenchAllowListImmediate);",
)
replace_exact(
    sdk_tests,
    "        private static void CspOnlyImmediate()\n"
    "        {\n"
    "            AssertEx.Throws<NotSupportedException>(\n"
    "                () => new LMCAxisSetOperationModeRecoveryKey(\n"
    "                    1,\n"
    "                    OriginalRequestId,\n"
    "                    DiagnosticsBuild,\n"
    "                    DiagnosticsBootId,\n"
    "                    MapRevision,\n"
    "                    Intent0,\n"
    "                    Intent1,\n"
    "                    Intent2,\n"
    "                    Intent3,\n"
    "                    2,\n"
    "                    LMCDriveOperationMode.Homing,\n"
    "                    TimeoutMilliseconds));\n"
    "            AssertEx.Throws<ArgumentOutOfRangeException>(\n"
    "                () => new LMCAxisSetOperationModeRecoveryKey(\n"
    "                    1,\n"
    "                    OriginalRequestId,\n"
    "                    DiagnosticsBuild,\n"
    "                    DiagnosticsBootId,\n"
    "                    MapRevision,\n"
    "                    Intent0,\n"
    "                    Intent1,\n"
    "                    Intent2,\n"
    "                    Intent3,\n"
    "                    2,\n"
    "                    LMCDriveOperationMode.CyclicSynchronousPosition,\n"
    "                    0));\n"
    "            AssertEx.Throws<ArgumentException>(\n"
    "                () => new LMCAxisSetOperationModeClientIntentId(\n"
    "                    0,\n"
    "                    0,\n"
    "                    0,\n"
    "                    0));\n"
    "        }",
    "        private static void BenchAllowListImmediate()\n"
    "        {\n"
    "            foreach (var allowed in new[]\n"
    "            {\n"
    "                LMCDriveOperationMode.ProfilePosition,\n"
    "                LMCDriveOperationMode.ProfileVelocity,\n"
    "                LMCDriveOperationMode.InterpolatedPosition,\n"
    "                LMCDriveOperationMode.CyclicSynchronousPosition\n"
    "            })\n"
    "            {\n"
    "                var key = new LMCAxisSetOperationModeRecoveryKey(\n"
    "                    1, OriginalRequestId, DiagnosticsBuild, DiagnosticsBootId,\n"
    "                    MapRevision, Intent0, Intent1, Intent2, Intent3, 2,\n"
    "                    allowed, TimeoutMilliseconds);\n"
    "                AssertEx.Equal(allowed, key.RequestedMode);\n"
    "            }\n\n"
    "            foreach (var blocked in new[]\n"
    "            {\n"
    "                LMCDriveOperationMode.NoModeAssigned,\n"
    "                LMCDriveOperationMode.Velocity,\n"
    "                LMCDriveOperationMode.ProfileTorque,\n"
    "                LMCDriveOperationMode.Homing,\n"
    "                LMCDriveOperationMode.CyclicSynchronousVelocity,\n"
    "                LMCDriveOperationMode.CyclicSynchronousTorque\n"
    "            })\n"
    "            {\n"
    "                AssertEx.Throws<NotSupportedException>(\n"
    "                    () => new LMCAxisSetOperationModeRecoveryKey(\n"
    "                        1, OriginalRequestId, DiagnosticsBuild, DiagnosticsBootId,\n"
    "                        MapRevision, Intent0, Intent1, Intent2, Intent3, 2,\n"
    "                        blocked, TimeoutMilliseconds));\n"
    "            }\n\n"
    "            AssertEx.Throws<ArgumentOutOfRangeException>(\n"
    "                () => new LMCAxisSetOperationModeRecoveryKey(\n"
    "                    1, OriginalRequestId, DiagnosticsBuild, DiagnosticsBootId,\n"
    "                    MapRevision, Intent0, Intent1, Intent2, Intent3, 2,\n"
    "                    LMCDriveOperationMode.CyclicSynchronousPosition, 0));\n"
    "            AssertEx.Throws<ArgumentException>(\n"
    "                () => new LMCAxisSetOperationModeClientIntentId(0, 0, 0, 0));\n"
    "        }",
)

# ---------------------------------------------------------------------------
# WPF smoke: selector must expose only the qualification allow-list.
# ---------------------------------------------------------------------------
replace_exact(
    wpf_tests,
    "            tests.Add(\n"
    "                \"Wpf.SetOperationModeRecovery.DefinitiveRejectArchivesAndClearsInterlock\",\n"
    "                SetOperationModeDefinitiveRejectArchivesAndClearsInterlock);",
    "            tests.Add(\n"
    "                \"Wpf.SetOperationModeRecovery.BenchModeSelectorIsExplicitAllowList\",\n"
    "                SetOperationModeBenchModeSelectorIsExplicitAllowList);\n"
    "            tests.Add(\n"
    "                \"Wpf.SetOperationModeRecovery.DefinitiveRejectArchivesAndClearsInterlock\",\n"
    "                SetOperationModeDefinitiveRejectArchivesAndClearsInterlock);",
)
replace_exact(
    wpf_tests,
    "        private static void\n"
    "            SetOperationModeDefinitiveRejectArchivesAndClearsInterlock()",
    "        private static void SetOperationModeBenchModeSelectorIsExplicitAllowList()\n"
    "        {\n"
    "            var root = CreateSetOperationModeTemporaryDirectory();\n"
    "            MainWindow window = null;\n"
    "            try\n"
    "            {\n"
    "                window = new MainWindow(root);\n"
    "                var selector = window.AxisSetOperationModeRequestedModeForTests;\n"
    "                AssertEx.NotNull(selector);\n"
    "                AssertEx.Equal(4, selector.Items.Count);\n"
    "                AssertEx.True(selector.Items.Contains(LMCDriveOperationMode.ProfilePosition));\n"
    "                AssertEx.True(selector.Items.Contains(LMCDriveOperationMode.ProfileVelocity));\n"
    "                AssertEx.True(selector.Items.Contains(LMCDriveOperationMode.InterpolatedPosition));\n"
    "                AssertEx.True(selector.Items.Contains(LMCDriveOperationMode.CyclicSynchronousPosition));\n"
    "                AssertEx.False(selector.Items.Contains(LMCDriveOperationMode.Homing));\n"
    "                AssertEx.Equal(\n"
    "                    LMCDriveOperationMode.CyclicSynchronousPosition,\n"
    "                    (LMCDriveOperationMode)selector.SelectedItem);\n"
    "                selector.SelectedItem = LMCDriveOperationMode.ProfilePosition;\n"
    "                AssertEx.Equal(\n"
    "                    LMCDriveOperationMode.ProfilePosition,\n"
    "                    (LMCDriveOperationMode)selector.SelectedItem);\n"
    "                AssertEx.False(window.AxisSetOperationModeStartButtonForTests.IsEnabled);\n"
    "            }\n"
    "            finally\n"
    "            {\n"
    "                if (window != null)\n"
    "                {\n"
    "                    window.Close();\n"
    "                }\n"
    "                DeleteSetOperationModeTemporaryDirectory(root);\n"
    "            }\n"
    "        }\n\n"
    "        private static void\n"
    "            SetOperationModeDefinitiveRejectArchivesAndClearsInterlock()",
)

# ---------------------------------------------------------------------------
# Qualification transform: pair bench-mode gate with primary/Admin activation.
# ---------------------------------------------------------------------------
replace_exact(
    prepare,
    "$DiagnosticsOn = '#define LMC_DIAG_SET_OPERATION_MODE_ENABLED TRUE'\n"
    "$AdminOff = '(pResponseFrame + 24)^$UDINT := 0x00000017;'",
    "$DiagnosticsOn = '#define LMC_DIAG_SET_OPERATION_MODE_ENABLED TRUE'\n"
    "$BenchModesOff = '#define LMC_DIAG_SET_OPERATION_MODE_BENCH_PRECONDITION_MODES FALSE'\n"
    "$BenchModesOn = '#define LMC_DIAG_SET_OPERATION_MODE_BENCH_PRECONDITION_MODES TRUE'\n"
    "$AdminOff = '(pResponseFrame + 24)^$UDINT := 0x00000017;'",
)
replace_exact(
    prepare,
    "    Assert-ExactCount $Diagnostics $DiagnosticsOff 1 'Diagnostics OFF gate'\n"
    "    Assert-ExactCount $Diagnostics $DiagnosticsOn 0 'Diagnostics ON gate'\n"
    "    Assert-ExactCount $Control $AdminOff 1 'Admin baseline feature mask'",
    "    Assert-ExactCount $Diagnostics $DiagnosticsOff 1 'Diagnostics OFF gate'\n"
    "    Assert-ExactCount $Diagnostics $DiagnosticsOn 0 'Diagnostics ON gate'\n"
    "    Assert-ExactCount $Diagnostics $BenchModesOff 1 'bench-mode OFF gate'\n"
    "    Assert-ExactCount $Diagnostics $BenchModesOn 0 'bench-mode ON gate'\n"
    "    Assert-ExactCount $Control $AdminOff 1 'Admin baseline feature mask'",
)
replace_exact(
    prepare,
    "    Assert-ExactCount $Diagnostics $DiagnosticsOff 0 'Diagnostics OFF gate'\n"
    "    Assert-ExactCount $Diagnostics $DiagnosticsOn 1 'Diagnostics ON gate'\n"
    "    Assert-ExactCount $Control $AdminOff 0 'Admin baseline feature mask'",
    "    Assert-ExactCount $Diagnostics $DiagnosticsOff 0 'Diagnostics OFF gate'\n"
    "    Assert-ExactCount $Diagnostics $DiagnosticsOn 1 'Diagnostics ON gate'\n"
    "    Assert-ExactCount $Diagnostics $BenchModesOff 0 'bench-mode OFF gate'\n"
    "    Assert-ExactCount $Diagnostics $BenchModesOn 1 'bench-mode ON gate'\n"
    "    Assert-ExactCount $Control $AdminOff 0 'Admin baseline feature mask'",
)
replace_exact(
    prepare,
    "    Assert-ExactCount $Diagnostics 'elsif requestedMode <> 8 then' 2 'CSP=8-only request guards'",
    "    Assert-MinimumCount $Diagnostics 'LMC_DIAG_SET_OPERATION_MODE_BENCH_PRECONDITION_MODES = FALSE' 2 'bench precondition allow-list guards'",
)
replace_exact(
    prepare,
    "        $newDiagnostics = $Diagnostics.Replace($DiagnosticsOff, $DiagnosticsOn)\n"
    "        $newControl = $Control.Replace($AdminOff, $AdminOn)",
    "        $newDiagnostics = $Diagnostics.Replace($DiagnosticsOff, $DiagnosticsOn)\n"
    "        $newDiagnostics = $newDiagnostics.Replace($BenchModesOff, $BenchModesOn)\n"
    "        $newControl = $Control.Replace($AdminOff, $AdminOn)",
)
replace_exact(
    prepare,
    "        $newDiagnostics = $Diagnostics.Replace($DiagnosticsOn, $DiagnosticsOff)\n"
    "        $newControl = $Control.Replace($AdminOn, $AdminOff)",
    "        $newDiagnostics = $Diagnostics.Replace($DiagnosticsOn, $DiagnosticsOff)\n"
    "        $newDiagnostics = $newDiagnostics.Replace($BenchModesOn, $BenchModesOff)\n"
    "        $newControl = $Control.Replace($AdminOn, $AdminOff)",
)
replace_exact(
    prepare,
    "    $diag = \"prefix`r`n$DiagnosticsOff`r`nelsif requestedMode <> 8 then`r`nelsif requestedMode <> 8 then`r`nLMC_DIAG_MODE_EVIDENCE_WRITE_DISPATCHED`r`n\"",
    "    $diag = \"prefix`r`n$DiagnosticsOff`r`n$BenchModesOff`r`nelsif (requestedMode <> 8) & (LMC_DIAG_SET_OPERATION_MODE_BENCH_PRECONDITION_MODES = FALSE) then`r`nelsif (requestedMode <> 8) & (LMC_DIAG_SET_OPERATION_MODE_BENCH_PRECONDITION_MODES = FALSE) then`r`nLMC_DIAG_MODE_EVIDENCE_WRITE_DISPATCHED`r`n\"",
)
replace_exact(
    prepare,
    "    Write-Host 'PASS MODE-11 source state: BENCH_ACTIVE (Diagnostics TRUE, Admin mask 0x00000717).'",
    "    Write-Host 'PASS MODE-11 source state: BENCH_ACTIVE (Diagnostics TRUE, Admin mask 0x00000717, bench targets PP/PV/IP/CSP).'",
)

# ---------------------------------------------------------------------------
# Candidate verifier: enforce the widened branch-only contract, not CSP literals.
# ---------------------------------------------------------------------------
replace_exact(
    candidate,
    "$tcpPath = Join-Path $repoRoot 'Lasal_PRG\\Elmo_EtherCAT_Test_4Axis\\Class\\TCPMotionInterface\\TCPMotionInterface.st'",
    "$tcpPath = Join-Path $repoRoot 'Lasal_PRG\\Elmo_EtherCAT_Test_4Axis\\Class\\TCPMotionInterface\\TCPMotionInterface.st'\n"
    "$sdkPath = Join-Path $repoRoot 'LMC_Library\\LMC_API_Delivery\\src\\LmcAdminSetOperationModeProtocol.cs'",
)
replace_exact(
    candidate,
    "foreach ($path in @($diagnosticsPath, $controlPath, $tcpPath)) {",
    "foreach ($path in @($diagnosticsPath, $controlPath, $tcpPath, $sdkPath)) {",
)
replace_exact(
    candidate,
    "$tcp = To-Lf ([IO.File]::ReadAllText($tcpPath))",
    "$tcp = To-Lf ([IO.File]::ReadAllText($tcpPath))\n"
    "$sdk = To-Lf ([IO.File]::ReadAllText($sdkPath))",
)
replace_exact(
    candidate,
    "Assert-Regex $diagnostics 'elsif[\\t ]+requestedMode[\\t ]*<>[\\t ]*8[\\t ]+then' 'CSP=8-only validation exists at both Start and recovery-key boundaries' -ExpectedCount 2\n"
    "Assert-Regex $diagnostics 'LMC_DIAG_SET_OPERATION_MODE_ENABLED[\\t ]*=[\\t ]*FALSE' 'OFF-transition recovery guards remain compiled into source' -MinimumCount 3",
    "Assert-Regex $diagnostics '(?m)^#define[\\t ]+LMC_DIAG_SET_OPERATION_MODE_BENCH_PRECONDITION_MODES[\\t ]+TRUE[\\t ]*$' 'qualification branch bench-precondition mode gate is TRUE' -ExpectedCount 1\n"
    "Assert-Regex $diagnostics 'requestedMode[\\t ]*<>[\\t ]*1[\\s\\S]{0,120}requestedMode[\\t ]*<>[\\t ]*3[\\s\\S]{0,120}requestedMode[\\t ]*<>[\\t ]*7' 'PLC allow-list is exactly PP/PV/IP in addition to CSP' -ExpectedCount 2\n"
    "Assert-Regex $sdk 'ProfilePosition[\\s\\S]{0,260}ProfileVelocity[\\s\\S]{0,260}InterpolatedPosition' 'SDK qualification allow-list contains PP/PV/IP/CSP' -MinimumCount 1\n"
    "Assert-Regex $sdk 'Homing\\(6\\) remains owned by HomeDS402/HomeDS402Ex' 'SDK explicitly keeps Homing out of SetOperationMode qualification path' -ExpectedCount 1\n"
    "Assert-Regex $diagnostics 'LMC_DIAG_SET_OPERATION_MODE_ENABLED[\\t ]*=[\\t ]*FALSE' 'OFF-transition recovery guards remain compiled into source' -MinimumCount 3",
)
replace_exact(
    candidate,
    "    Assert-Regex $mutation 'if[\\t ]+observedMode[\\t ]*=[\\t ]*8[\\t ]+then[\\s\\S]{0,900}LMC_DIAG_MODE_STAGE_TERMINAL_SUCCESS;[\\s\\S]{0,120}RETURN;[\\s\\S]{0,900}_memset\\(dest:=#startupSnapshot' 'same-mode CSP=8 path terminates before write-safety branch' -ExpectedCount 1\n\n"
    "    $sameModePattern = '(?ms)if[\\t ]+observedMode[\\t ]*=[\\t ]*8[\\t ]+then(?<body>.*?)^[\\t ]*RETURN;'",
    "    Assert-Regex $mutation 'if[\\t ]+observedMode[\\t ]*=[\\t ]*AxisOperationModeState\\[recordBase \\+ 10\\]\\$SINT[\\t ]+then[\\s\\S]{0,900}LMC_DIAG_MODE_STAGE_TERMINAL_SUCCESS;[\\s\\S]{0,120}RETURN;[\\s\\S]{0,900}_memset\\(dest:=#startupSnapshot' 'same-requested-mode path terminates before write-safety branch' -ExpectedCount 1\n\n"
    "    $sameModePattern = '(?ms)if[\\t ]+observedMode[\\t ]*=[\\t ]*AxisOperationModeState\\[recordBase \\+ 10\\]\\$SINT[\\t ]+then(?<body>.*?)^[\\t ]*RETURN;'",
)
replace_exact(
    candidate,
    '        Fail "exact same-mode CSP=8 branch (count=$($sameModeMatches.Count), expected=1)"',
    '        Fail "exact same-requested-mode branch (count=$($sameModeMatches.Count), expected=1)"',
)
replace_exact(
    candidate,
    "        Pass 'exact same-mode CSP=8 branch'",
    "        Pass 'exact same-requested-mode branch'",
)
replace_exact(
    candidate,
    "    Assert-Regex $mutation 'LMC_DIAG_MODE_EVIDENCE_WRITE_DISPATCHED' 'mutation helper persists write-dispatch evidence' -MinimumCount 1",
    "    Assert-Regex $mutation 'LMC_DIAG_MODE_EVIDENCE_WRITE_DISPATCHED' 'mutation helper persists write-dispatch evidence' -MinimumCount 1\n"
    "    Assert-Regex $diagnostics 'LMC_DIAG_MODE_RUNTIME_WRITE_DATA\\][\\t ]*:=[\\t ]*8;' 'runtime write data is never hard-coded to CSP=8' -ExpectedCount 0\n"
    "    Assert-Regex $diagnostics 'LMC_DIAG_MODE_RUNTIME_WRITE_DATA\\][\\t ]*:=[\\t ]*TO_DINT\\(requestedMode\\)' 'new Start binds 0x6060 write data to requested mode' -ExpectedCount 1\n"
    "    Assert-Regex $diagnostics 'LMC_DIAG_MODE_RUNTIME_WRITE_DATA\\][\\t ]*:=[\\t ]*[\\r\\n\\t ]*AxisOperationModeState\\[recoveryScanBase \\+ 10\\]' 'warm recovery restores retained requested-mode identity' -ExpectedCount 1",
)

print("PASS deterministic SetOperationMode qualification multimode patch applied")
