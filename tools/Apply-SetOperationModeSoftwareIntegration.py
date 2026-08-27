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
mode_design = "docs/api/design/SET_OPERATION_MODE_DESIGN.md"
status_doc = "docs/api/design/DEVELOPMENT_STATUS_20260827.md"
design_readme = "docs/api/design/README.md"
progress_doc = "docs/api/API_DEVELOPMENT_PROGRESS.md"

# ---------------------------------------------------------------------------
# PLC software implementation: retain production activation OFF, but make the
# requested mode (PP/PV/IP/CSP) the runtime truth behind that dormant gate.
# ---------------------------------------------------------------------------
replace_exact(
    diag,
    "#define LMC_DIAG_SET_OPERATION_MODE_ENABLED FALSE",
    "#define LMC_DIAG_SET_OPERATION_MODE_ENABLED FALSE\n"
    "#define LMC_DIAG_SET_OPERATION_MODE_SOFTWARE_MODES TRUE",
)
replace_exact(
    diag,
    "\telsif requestedMode <> 8 then\n"
    "\t\tdetailCode := LMC_DIAG_MODE_DETAIL_UNSUPPORTED;",
    "\telsif (requestedMode <> 8) &\n"
    "\t      ((LMC_DIAG_SET_OPERATION_MODE_SOFTWARE_MODES = FALSE) |\n"
    "\t       ((requestedMode <> 1) & (requestedMode <> 3) & (requestedMode <> 7))) then\n"
    "\t\tdetailCode := LMC_DIAG_MODE_DETAIL_UNSUPPORTED;",
)
replace_exact(
    diag,
    "\telsif requestedMode <> 8 then\n"
    "\t\tdetailCode := LMC_DIAG_MODE_DETAIL_KEY_MISMATCH;",
    "\telsif (requestedMode <> 8) &\n"
    "\t      ((LMC_DIAG_SET_OPERATION_MODE_SOFTWARE_MODES = FALSE) |\n"
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
# SDK exact software allow-list. Homing remains owned by HomeDS402 APIs.
# ---------------------------------------------------------------------------
replace_exact(
    sdk,
    "            if (requestedMode\n"
    "                != LMCDriveOperationMode.CyclicSynchronousPosition)\n"
    "            {\n"
    "                throw new NotSupportedException(\n"
    "                    \"SetOperationMode schema version 1 supports CSP mode 8 recovery only.\");\n"
    "            }",
    "            var softwareModeAllowed = requestedMode\n"
    "                    == LMCDriveOperationMode.CyclicSynchronousPosition\n"
    "                || requestedMode == LMCDriveOperationMode.ProfilePosition\n"
    "                || requestedMode == LMCDriveOperationMode.ProfileVelocity\n"
    "                || requestedMode == LMCDriveOperationMode.InterpolatedPosition;\n"
    "            if (!softwareModeAllowed)\n"
    "            {\n"
    "                throw new NotSupportedException(\n"
    "                    \"SetOperationMode software implementation supports PP(1), PV(3), IP(7), and CSP(8). \"\n"
    "                    + \"Homing(6) remains owned by HomeDS402/HomeDS402Ex.\");\n"
    "            }",
)

# ---------------------------------------------------------------------------
# WPF software target selector. Start remains fail-closed because production
# Admin capability bits 8/9/10 and the PLC compile gate remain OFF.
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
    '                Header = "Set Operation Mode - software target / durable no-replay recovery"',
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
    "                Text = \"PP(1)/PV(3)/IP(7)/CSP(8) software targets are implemented. \"\n"
    "                    + \"Production Start remains disabled until PLC capability and hardware qualification are complete. \"\n"
    "                    + \"Homing(6) remains unavailable here.\"\n"
    "            });\n"
    "            inputs.Children.Add(modePanel);",
)
replace_exact(
    wpf,
    '                    Text = "I verified the exact powered drive/axis and understand that this writes DS402 0x6060:0 to CSP=8 once only. "\n'
    '                        + "If the response or completion is uncertain I will use the durable recovery query and will not send Start again."',
    '                    Text = "I verified the exact drive/axis and understand that this may write DS402 0x6060:0 to the selected mode once only. "\n'
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
    "                    \"A supported SetOperationMode software target is required.\");\n"
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
# Contract tests.
# ---------------------------------------------------------------------------
replace_exact(
    sdk_tests,
    '                "Contract.Admin.SetOperationMode.CspOnlyImmediate",\n'
    "                CspOnlyImmediate);",
    '                "Contract.Admin.SetOperationMode.SoftwareAllowListImmediate",\n'
    "                SoftwareAllowListImmediate);",
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
    "        private static void SoftwareAllowListImmediate()\n"
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
replace_exact(
    wpf_tests,
    "            tests.Add(\n"
    "                \"Wpf.SetOperationModeRecovery.DefinitiveRejectArchivesAndClearsInterlock\",\n"
    "                SetOperationModeDefinitiveRejectArchivesAndClearsInterlock);",
    "            tests.Add(\n"
    "                \"Wpf.SetOperationModeRecovery.SoftwareModeSelectorIsExplicitAllowList\",\n"
    "                SetOperationModeSoftwareModeSelectorIsExplicitAllowList);\n"
    "            tests.Add(\n"
    "                \"Wpf.SetOperationModeRecovery.DefinitiveRejectArchivesAndClearsInterlock\",\n"
    "                SetOperationModeDefinitiveRejectArchivesAndClearsInterlock);",
)
replace_exact(
    wpf_tests,
    "        private static void\n"
    "            SetOperationModeDefinitiveRejectArchivesAndClearsInterlock()",
    "        private static void SetOperationModeSoftwareModeSelectorIsExplicitAllowList()\n"
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
# Design/progress docs: software implementation is merged, physical/release
# qualification remains open. Release-oriented progress moves 60 -> 65.
# ---------------------------------------------------------------------------
replace_exact(mode_design, "- 현재 진행도: 60%", "- 현재 진행도: 65%")
replace_exact(
    mode_design,
    "- current 상태: `Dormant runtime source`; PC/SDK contract, owner/runtime, no-replay recovery, safety preemption, generic D5 0x6060 차단과 MODE-13 WPF durable recovery 구현",
    "- current 상태: `Dormant multi-mode software implementation`; PP/PV/IP/CSP SDK/PLC/WPF target path, owner/runtime, no-replay recovery, safety preemption, generic D5 0x6060 차단과 MODE-13 WPF durable recovery 구현",
)
replace_exact(
    mode_design,
    "- 1차 activation 범위: physical axis 1..4, Immediate, CSP mode 8만",
    "- software 구현 범위: physical axis 1..4, Immediate, PP(1)/PV(3)/IP(7)/CSP(8); production activation은 아직 없음",
)
replace_exact(
    mode_design,
    "- 진행 판정: MODE-02/06/07/08/09 source 완료, MODE-10 source/static PASS, MODE-13 PC/WPF PASS; current exact-image C78/PLC/hardware는 미완료",
    "- 진행 판정: 기존 lifecycle/no-replay tranche + MODE-11 software multi-mode path 통합. production capability/mask, current exact-image C78/PLC/hardware는 미완료",
)
replace_exact(
    mode_design,
    "- open qualification branch: PR #18 `codex/setopmode-mode11-bench-activation` — `DO NOT MERGE`, physical bench evidence 전용",
    "- PR #18 software implementation lineage는 current integration에 흡수하되 qualification-only activation은 제외. physical evidence는 별도 gate로 유지",
)
replace_exact(
    mode_design,
    "## 2. 1차 지원 범위\n\n"
    "첫 구현은 lifecycle과 recovery를 완성하되 public activation은 CSP 8만 허용한다.\n\n"
    "- physical axis 1..4\n"
    "- Immediate-only\n"
    "- requested mode `8`만\n"
    "- 이미 `0x6061=8`이면 terminal `SucceededNoWrite`\n"
    "- 다른 mode에서 8로 복구할 때만 exact one-byte `0x6060:0=8` Write\n"
    "- 움직임, Fault, pending motion, 다른 mutation owner 또는 SDO owner가 있으면 거부\n"
    "- Homing mode 6은 `HomeDS402/HomeDS402Ex` 내부 owner만 사용하고 public SetOpMode에서 거부\n\n"
    "Mode 1/3/7은 해당 mode의 setpoint PDO/controller, `_LMCAxis` output 정지·인계와 physical\n"
    "proof가 마련될 때 각각 별도 activation한다. 초기 구현에서 이를 광고하지 않는다.",
    "## 2. software 구현 범위\n\n"
    "current software path는 lifecycle/recovery identity를 유지하면서 요청 mode를 runtime truth로 일반화한다.\n"
    "이 단계는 구현 완료를 의미하지만 production activation 또는 ordinary motion support를 의미하지 않는다.\n\n"
    "- physical axis 1..4\n"
    "- Immediate-only\n"
    "- software allow-list: PP(1), PV(3), IP(7), CSP(8)\n"
    "- Homing(6)은 `HomeDS402/HomeDS402Ex` 전용이며 SetOperationMode에서 계속 거부\n"
    "- 이미 `0x6061=requestedMode`이면 terminal `SucceededNoWrite`\n"
    "- mode 변경이 필요하면 exact one-byte `0x6060:0=requestedMode` Write 후 `0x6061` exact verify\n"
    "- write-dispatch 이후 original Start/0x6060 자동 replay 금지\n"
    "- 움직임, Fault, pending motion, 다른 mutation owner 또는 SDO owner가 있으면 거부\n\n"
    "production에서는 PLC-advertised SupportedModeMask와 mode별 physical qualification이 추가되기 전까지\n"
    "compile gate와 Admin bits 8/9/10을 OFF로 유지한다. PP/PV/IP 선택 가능 소스가 존재하는 것만으로\n"
    "해당 mode의 ordinary motion/control producer가 지원된다고 해석하지 않는다.",
)
replace_exact(
    mode_design,
    "- [ ] `MODE-11` CSP same-mode no-write와 exact one-write/readback packet 검증 — PR #18 software bench tooling 준비, physical evidence 미완료",
    "- [x] `MODE-11S` PP/PV/IP/CSP software target prepare/start/recovery + WPF selector source 통합, activation OFF\n"
    "- [ ] `MODE-11` same-mode zero-write와 cross-mode exact one-write/readback packet 검증 — physical evidence 미완료",
)
replace_exact(
    mode_design,
    "## 9. 비-CSP 후속 gate\n\n"
    "Mode 1/3/7을 열려면 mode별 PDO, setpoint producer, controlword owner, `_LMCAxis` output 인계,\n"
    "Stop/Power/fault/restart와 mode 8 복귀 계약을 별도로 구현한다. 이 결정 전에는 진행도가 올라가도\n"
    "`CSP=8 only` 제한을 특이사항에서 제거하지 않는다. CSP recovery-only tranche 완료를\n"
    "`MMC_ChngOpMode` 전체 구현 또는 75% 완료로 기록하지 않는다.",
    "## 9. 비-CSP 후속 gate\n\n"
    "PP(1)/PV(3)/IP(7)의 mode-change software mutation path 자체는 current source에 구현됐다.\n"
    "다만 production support를 열려면 PLC SupportedModeMask, mode별 PDO/setpoint producer/controlword owner,\n"
    "`_LMCAxis` output 인계, Stop/Power/fault/restart와 CSP 복귀 계약을 physical evidence로 닫아야 한다.\n"
    "따라서 software implementation 통합은 65% 진행으로 기록하되 production activation은 계속 OFF다.",
)

replace_exact(
    status_doc,
    "| `SetOperationMode` | 60% | MODE-02/06/07/08/09 source, MODE-10 source/static, MODE-13 WPF durable recovery + dynamic localization CI 완료 | Dormant / compile gate FALSE / bits 8..10 OFF | current exact-image C78, MODE-11/12 physical evidence, MODE-14 activation |",
    "| `SetOperationMode` | 65% | 기존 lifecycle/no-replay + PP/PV/IP/CSP software target SDK/PLC/WPF path 통합; MODE-10 static, MODE-13 durable recovery 유지 | Dormant / compile gate FALSE / bits 8..10 OFF | SupportedModeMask, current exact-image C78, MODE-11/12 physical evidence, MODE-14 activation |",
)
replace_exact(
    status_doc,
    "- exact 56-byte Start identity\n"
    "- `6061 -> 6060 -> 6061` runtime\n"
    "- same-mode zero-write path",
    "- exact 56-byte Start identity\n"
    "- SDK/WPF software target allow-list PP(1)/PV(3)/IP(7)/CSP(8)\n"
    "- PLC requested-mode-driven `6061 -> 6060 -> 6061` dormant runtime\n"
    "- requested-mode same-mode zero-write path",
)
replace_exact(
    status_doc,
    "남은 gate:\n\n"
    "1. current exact source C78/ARM rebuild/link + artifact identity review\n"
    "2. MODE-11A CSP same-mode zero-write packet/hardware evidence\n"
    "3. MODE-11B independently approved non-CSP exact-one-write/readback evidence\n"
    "4. MODE-12 axis 1..4 timeout/disconnect/mismatch/quarantine/retire matrix\n"
    "5. MODE-14 paired capability activation",
    "남은 gate:\n\n"
    "1. PLC-advertised SupportedModeMask + WPF selector intersection/fail-closed\n"
    "2. current exact source C78/ARM rebuild/link + artifact identity review\n"
    "3. MODE-11 same-mode zero-write / cross-mode exact-one-write packet-hardware evidence\n"
    "4. MODE-12 axis 1..4 timeout/disconnect/mismatch/quarantine/retire matrix\n"
    "5. MODE-14 paired capability activation",
)

replace_exact(
    design_readme,
    "| 2 | `SetOpMode` | 60% | owner/SDO/no-replay/preemption/D5 deny source, MODE-10 static, MODE-13 PC/WPF recovery 완료; current exact-image C78/PLC/hardware 남음 | [SET_OPERATION_MODE_DESIGN.md](SET_OPERATION_MODE_DESIGN.md) |",
    "| 2 | `SetOpMode` | 65% | PP/PV/IP/CSP software target SDK/PLC/WPF path까지 통합; activation OFF, SupportedModeMask/C78/PLC/hardware 후속 | [SET_OPERATION_MODE_DESIGN.md](SET_OPERATION_MODE_DESIGN.md) |",
)
replace_exact(
    design_readme,
    "| SetOpMode | `0x7D23` | `0x7D24` | `0x7D25` | C#/LASAL lifecycle + WPF durable recovery 구현, compile gate/capability OFF |",
    "| SetOpMode | `0x7D23` | `0x7D24` | `0x7D25` | PP/PV/IP/CSP software mutation/recovery + WPF selector 구현, compile gate/capability OFF |",
)
replace_exact(
    design_readme,
    "- **SetOpMode**: MODE-10/13 software evidence는 완료. current exact source tree의 C78/PLC artifact와 MODE-11/12 hardware evidence가 다음 gate다.",
    "- **SetOpMode**: PP/PV/IP/CSP software implementation까지 통합. 다음은 PLC SupportedModeMask, current exact-image C78/PLC artifact와 MODE-11/12 hardware evidence다.",
)

replace_exact(
    progress_doc,
    "- SetOperationMode는 owner/SDO/no-replay/preemption/outcome/D5 deny + MODE-10 source/static + MODE-13\n"
    "  WPF recovery가 구현됐다. compile gate와 bits 8/9/10은 OFF다.",
    "- SetOperationMode는 owner/SDO/no-replay/preemption/outcome/D5 deny + MODE-10/13에 더해\n"
    "  PP/PV/IP/CSP software target SDK/PLC/WPF path가 구현됐다. compile gate와 bits 8/9/10은 OFF다.",
)
replace_exact(
    progress_doc,
    "| SetOperationMode | `0x7D23/7D24/7D25` | Dormant | owner/SDO/no-replay/preemption/outcome/D5 deny/WPF recovery 존재; compile gate/bits 8..10 OFF |",
    "| SetOperationMode | `0x7D23/7D24/7D25` | Dormant | PP/PV/IP/CSP software target + owner/SDO/no-replay/preemption/outcome/WPF recovery 존재; compile gate/bits 8..10 OFF |",
)
replace_exact(
    progress_doc,
    "- `6061` preflight -> 필요 시 one-byte `6060=8` -> `6061` verify\n"
    "- same-mode no-write",
    "- software target allow-list PP(1)/PV(3)/IP(7)/CSP(8)\n"
    "- `6061` preflight -> 필요 시 one-byte `6060=requestedMode` -> `6061` exact verify\n"
    "- requested-mode same-mode no-write",
)
replace_exact(
    progress_doc,
    "남은 gate:\n\n"
    "- current exact source fresh C78/generated artifact + PLC load\n"
    "- MODE-11 same-mode zero-write / exact-one-write packet evidence\n"
    "- MODE-12 axis 1..4 fault/disconnect/quarantine matrix\n"
    "- MODE-14 bits 8/9/10 paired activation",
    "남은 gate:\n\n"
    "- PLC SupportedModeMask + WPF capability intersection\n"
    "- current exact source fresh C78/generated artifact + PLC load\n"
    "- MODE-11 same-mode zero-write / cross-mode exact-one-write packet evidence\n"
    "- MODE-12 axis 1..4 fault/disconnect/quarantine matrix\n"
    "- MODE-14 bits 8/9/10 paired activation",
)

print("SetOperationMode software integration transform applied successfully.")
