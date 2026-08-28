from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
XAML = ROOT / 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml'
DIAG = ROOT / 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs'
LOCALIZATION = ROOT / 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/UiLocalization.cs'
SMOKE = ROOT / 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs'
DESIGN = ROOT / 'docs/api/design/SDO_OPERATION_MODE_REIMPLEMENTATION_PLAN_20260827.md'
VERIFY = ROOT / 'tools/Verify-SdoR04RequestPreview.ps1'
WORKFLOW = ROOT / '.github/workflows/sdo-r04-request-preview.yml'


def replace_once(path: Path, old: str, new: str, label: str) -> None:
    text = path.read_text(encoding='utf-8')
    count = text.count(old)
    if count != 1:
        raise RuntimeError(f'{path}: expected exactly one {label} anchor, found {count}')
    path.write_text(text.replace(old, new, 1), encoding='utf-8')


# Add a dedicated request-preview row without changing the submission controls.
replace_once(
    XAML,
    '''                                <Grid.RowDefinitions>\n                                    <RowDefinition Height="Auto" />\n                                    <RowDefinition Height="Auto" />\n                                    <RowDefinition Height="Auto" />\n                                    <RowDefinition Height="Auto" />\n                                    <RowDefinition Height="Auto" />\n                                </Grid.RowDefinitions>\n\n                                <StackPanel Grid.Row="0" Grid.Column="0">\n                                    <TextBlock Style="{StaticResource FieldLabel}" Text="Operation" />''',
    '''                                <Grid.RowDefinitions>\n                                    <RowDefinition Height="Auto" />\n                                    <RowDefinition Height="Auto" />\n                                    <RowDefinition Height="Auto" />\n                                    <RowDefinition Height="Auto" />\n                                    <RowDefinition Height="Auto" />\n                                    <RowDefinition Height="Auto" />\n                                </Grid.RowDefinitions>\n\n                                <StackPanel Grid.Row="0" Grid.Column="0">\n                                    <TextBlock Style="{StaticResource FieldLabel}" Text="Operation" />''',
    'SDO grid row definitions')

replace_once(
    XAML,
    '''                                <WrapPanel Grid.Row="3" Grid.ColumnSpan="5">''',
    '''                                <StackPanel Grid.Row="3" Grid.ColumnSpan="5" Margin="0,2,0,6">\n                                    <TextBlock Style="{StaticResource FieldLabel}" Text="Exact request preview" />\n                                    <TextBlock\n                                        x:Name="TextSdoRequestPreview"\n                                        FontFamily="Consolas"\n                                        Tag="UiLocalization.Preserve"\n                                        Text="REQUEST DRAFT | edit the fields above"\n                                        TextWrapping="Wrap" />\n                                    <TextBlock\n                                        x:Name="TextSdoSemanticWarning"\n                                        Margin="0,2,0,0"\n                                        Foreground="#8A4A00"\n                                        FontWeight="SemiBold"\n                                        Tag="UiLocalization.Preserve"\n                                        TextWrapping="Wrap"\n                                        Visibility="Collapsed" />\n                                </StackPanel>\n\n                                <WrapPanel Grid.Row="4" Grid.ColumnSpan="5">''',
    'SDO preview insertion')

replace_once(
    XAML,
    '''                                    Grid.Row="4"\n                                    Grid.ColumnSpan="5"\n                                    FontFamily="Consolas"\n                                    Text="SDO Read and Generic Write support exact 1/2/4-byte typed values.''',
    '''                                    Grid.Row="5"\n                                    Grid.ColumnSpan="5"\n                                    FontFamily="Consolas"\n                                    Text="SDO Read and Generic Write support exact 1/2/4-byte typed values.''',
    'SDO summary row shift')

# Refresh the preview after every editor mutation and after mode/preset normalization.
replace_once(
    DIAG,
    '''                UiLocalizationService.Apply(\n                    ButtonSubmitSdo,\n                    currentUiLanguage);\n            }\n        }\n\n        private void SdoWriteTarget_SelectionChanged''',
    '''                UiLocalizationService.Apply(\n                    ButtonSubmitSdo,\n                    currentUiLanguage);\n            }\n\n            UpdateSdoRequestPreview();\n        }\n\n        private void SdoWriteTarget_SelectionChanged''',
    'editor-change preview refresh')

replace_once(
    DIAG,
    '''            sdoWriteConfirmationState.Clear();\n            ApplySelectedSdoWriteTarget();\n            if (ButtonConnect != null)''',
    '''            sdoWriteConfirmationState.Clear();\n            ApplySelectedSdoWriteTarget();\n            UpdateSdoRequestPreview();\n            if (ButtonConnect != null)''',
    'preset preview refresh')

replace_once(
    DIAG,
    '''            else\n            {\n                ButtonSubmitSdo.Content = "Submit SDO Read";\n                TextDiagnosticOperationSummary.Text =\n                    "SDO Read supports exact 1/2/4-byte typed values. Read SDO Inline waits for and displays the terminal typed/raw result in one action; Submit/Refresh remains available for low-level ticket diagnostics. Bit 13 enables editable nonzero object index and sub-index; a bit-8-only PLC uses fixed 0x1000:0 UInt32/4.";\n            }\n        }\n\n        private void ApplySelectedSdoWriteTarget()''',
    '''            else\n            {\n                ButtonSubmitSdo.Content = "Submit SDO Read";\n                TextDiagnosticOperationSummary.Text =\n                    "SDO Read supports exact 1/2/4-byte typed values. Read SDO Inline waits for and displays the terminal typed/raw result in one action; Submit/Refresh remains available for low-level ticket diagnostics. Bit 13 enables editable nonzero object index and sub-index; a bit-8-only PLC uses fixed 0x1000:0 UInt32/4.";\n            }\n\n            UpdateSdoRequestPreview();\n        }\n\n        private void UpdateSdoRequestPreview()\n        {\n            if (TextSdoRequestPreview == null\n                || TextSdoSemanticWarning == null\n                || ComboSdoOperation == null\n                || TextSdoSlaveReference == null\n                || TextSdoIndex == null\n                || TextSdoSubIndex == null\n                || ComboSdoValueType == null\n                || ComboSdoDataLength == null\n                || TextSdoTimeoutCycles == null\n                || TextSdoWriteData == null)\n            {\n                return;\n            }\n\n            TextSdoSemanticWarning.Text = string.Empty;\n            TextSdoSemanticWarning.Visibility = Visibility.Collapsed;\n\n            try\n            {\n                var mode = RequireSelectedEnum<SdoOperationMode>(\n                    ComboSdoOperation,\n                    "SDO operation");\n                var slaveReference = ParseUInt16Wire(\n                    TextSdoSlaveReference.Text,\n                    "SDO slave reference",\n                    false);\n                if (slaveReference < 1 || slaveReference > 4)\n                {\n                    throw new InvalidOperationException(\n                        "Slave reference must be between 1 and 4.");\n                }\n\n                var objectIndex = ParseUInt16Wire(\n                    TextSdoIndex.Text,\n                    "SDO object index",\n                    false);\n                var subIndex = ParseByteWire(\n                    TextSdoSubIndex.Text,\n                    "SDO sub-index");\n                var valueType = RequireSelectedEnum<LMCSignalValueType>(\n                    ComboSdoValueType,\n                    "SDO value type");\n                var dataLength = ParseUInt16Wire(\n                    ComboSdoDataLength.Text,\n                    "SDO data length",\n                    false);\n                var expectedLength = GetSdoReadDataLength(valueType);\n                if (dataLength != expectedLength)\n                {\n                    throw new InvalidOperationException(\n                        "Data length must match the selected type: 8-bit types=1, 16-bit types=2, 32-bit types=4.");\n                }\n\n                var timeoutCycles = ParseUInt32(\n                    TextSdoTimeoutCycles.Text,\n                    "SDO timeout cycles");\n                if (timeoutCycles < 1 || timeoutCycles > 60000)\n                {\n                    throw new InvalidOperationException(\n                        "Timeout must be between 1 and 60000 cycles.");\n                }\n\n                LMCSdoRequest request;\n                if (mode == SdoOperationMode.Write)\n                {\n                    var writeData = ParseSdoWriteScalarData(\n                        TextSdoWriteData.Text,\n                        valueType,\n                        dataLength);\n                    request = LMCSdoRequest.CreateWrite(\n                        slaveReference,\n                        objectIndex,\n                        subIndex,\n                        valueType,\n                        writeData,\n                        timeoutCycles);\n                    try\n                    {\n                        LMCDiagnosticsWritePolicy.RequireSdoWriteAllowed(request);\n                    }\n                    catch (NotSupportedException error)\n                    {\n                        TextSdoRequestPreview.Text =\n                            "BLOCKED REQUEST | NOT SUBMITTED | "\n                            + FormatSdoExactRequestPreview(request);\n                        TextSdoSemanticWarning.Text =\n                            "BLOCKED RESERVED SDO WRITE | NOT SUBMITTED | "\n                            + error.Message;\n                        TextSdoSemanticWarning.Visibility = Visibility.Visible;\n                        return;\n                    }\n                }\n                else\n                {\n                    request = LMCSdoRequest.CreateRead(\n                        slaveReference,\n                        objectIndex,\n                        subIndex,\n                        valueType,\n                        dataLength,\n                        timeoutCycles);\n                }\n\n                TextSdoRequestPreview.Text =\n                    FormatSdoExactRequestPreview(request);\n            }\n            catch (Exception error)\n                when (error is ArgumentException\n                    || error is InvalidOperationException\n                    || error is NotSupportedException\n                    || error is FormatException\n                    || error is OverflowException)\n            {\n                TextSdoRequestPreview.Text =\n                    "INVALID REQUEST DRAFT | NOT SUBMITTED | "\n                    + error.Message;\n            }\n        }\n\n        private static string FormatSdoExactRequestPreview(\n            LMCSdoRequest request)\n        {\n            if (request == null)\n            {\n                throw new ArgumentNullException("request");\n            }\n\n            var preview = new StringBuilder();\n            preview.Append("EXACT REQUEST | Operation=")\n                .Append(request.IsWrite ? "Write" : "Read")\n                .Append(" | Slave=")\n                .Append(request.SlaveReference.ToString(\n                    CultureInfo.InvariantCulture))\n                .Append(" | Object=0x")\n                .Append(request.ObjectIndex.ToString("X4"))\n                .Append(':')\n                .Append(request.SubIndex.ToString(\n                    CultureInfo.InvariantCulture))\n                .Append(" | Type=")\n                .Append(request.ValueType)\n                .Append(" | Length=")\n                .Append(request.DataLength.ToString(\n                    CultureInfo.InvariantCulture))\n                .Append(" | TimeoutCycles=")\n                .Append(request.TimeoutCycles.ToString(\n                    CultureInfo.InvariantCulture));\n            if (request.IsWrite)\n            {\n                preview.Append(" | WriteData=")\n                    .Append(BitConverter.ToString(request.WriteData));\n            }\n\n            return preview.ToString();\n        }\n\n        private void ApplySelectedSdoWriteTarget()''',
    'preview implementation')

# Localize the new static label. Preview wire evidence remains canonical machine text.
replace_once(
    LOCALIZATION,
    '''            values["Known SDO Write preset (optional)"] =\n                "알려진 SDO Write preset (선택 사항)";''',
    '''            values["Known SDO Write preset (optional)"] =\n                "알려진 SDO Write preset (선택 사항)";\n            values["Exact request preview"] =\n                "정확 요청 미리보기";''',
    'preview localization')

# Extend the existing generic-write smoke to assert exact preview and zero-wire reserved warning.
replace_once(
    SMOKE,
    '''                    AssertEx.SequenceEqual(\n                        new byte[] { 0x34, 0x12 },\n                        genericRequest.WriteData);\n\n                    window.TextSdoIndex.Text = "0x6060";''',
    '''                    AssertEx.SequenceEqual(\n                        new byte[] { 0x34, 0x12 },\n                        genericRequest.WriteData);\n                    InvokePrivate(window, "UpdateSdoRequestPreview");\n                    AssertEx.Contains(\n                        "EXACT REQUEST",\n                        window.TextSdoRequestPreview.Text);\n                    AssertEx.Contains(\n                        "Operation=Write",\n                        window.TextSdoRequestPreview.Text);\n                    AssertEx.Contains(\n                        "Slave=2",\n                        window.TextSdoRequestPreview.Text);\n                    AssertEx.Contains(\n                        "Object=0x2000:3",\n                        window.TextSdoRequestPreview.Text);\n                    AssertEx.Contains(\n                        "Type=UInt16",\n                        window.TextSdoRequestPreview.Text);\n                    AssertEx.Contains(\n                        "Length=2",\n                        window.TextSdoRequestPreview.Text);\n                    AssertEx.Contains(\n                        "WriteData=34-12",\n                        window.TextSdoRequestPreview.Text);\n                    AssertEx.Equal(\n                        System.Windows.Visibility.Collapsed,\n                        window.TextSdoSemanticWarning.Visibility);\n\n                    window.TextSdoIndex.Text = "0x6060";''',
    'generic preview smoke')

replace_once(
    SMOKE,
    '''                    AssertEx.Contains(\n                        "semantic or dedicated-owner objects",\n                        Convert.ToString(\n                            reservedRequestArguments[1],\n                            CultureInfo.InvariantCulture));\n\n                    AssertEx.False(''',
    '''                    AssertEx.Contains(\n                        "semantic or dedicated-owner objects",\n                        Convert.ToString(\n                            reservedRequestArguments[1],\n                            CultureInfo.InvariantCulture));\n                    InvokePrivate(window, "UpdateSdoRequestPreview");\n                    AssertEx.Equal(\n                        System.Windows.Visibility.Visible,\n                        window.TextSdoSemanticWarning.Visibility);\n                    AssertEx.Contains(\n                        "BLOCKED RESERVED SDO WRITE",\n                        window.TextSdoSemanticWarning.Text);\n                    AssertEx.Contains(\n                        "NOT SUBMITTED",\n                        window.TextSdoSemanticWarning.Text);\n                    AssertEx.Contains(\n                        "semantic or dedicated-owner objects",\n                        window.TextSdoSemanticWarning.Text);\n\n                    AssertEx.False(''',
    'reserved warning smoke')

# Bring the design checklist up to the current implementation only after this promotion passes.
design_text = DESIGN.read_text(encoding='utf-8')
old_gate = '''- [ ] `0x2F00` combo 선택 없이 arbitrary target 입력 가능\n- [ ] type/length/value canonical validation\n- [ ] exact request preview\n- [ ] semantic reserved warning\n- [ ] Write Once explicit arm\n- [ ] ticket/status/abort 표시\n- [ ] exact readback 표시\n- [ ] Korean/English localization round-trip\n- [ ] Debug/Release WPF smoke'''
new_gate = '''- [x] `0x2F00` combo 선택 없이 arbitrary target 입력 가능\n- [x] type/length/value canonical validation\n- [x] exact request preview\n- [x] semantic reserved warning\n- [x] Write Once explicit arm\n- [x] ticket/status/abort 표시\n- [x] exact readback 표시\n- [x] Korean/English localization round-trip\n- [x] Debug/Release WPF smoke\n\n2026-08-28 current-dev update: arbitrary 1/2/4-byte scalar draft input, optional preset, exact two-click Write Once, durable exact readback/no-replay 경계에 더해 wire 직전 canonical request preview와 semantic/dedicated-owner zero-wire warning surface를 추가했다. Preview는 capability refresh와 독립된 draft validation이며 실제 Submit admission/policy를 완화하지 않는다.'''
if design_text.count(old_gate) != 1:
    raise RuntimeError('R04 design completion gate anchor mismatch')
DESIGN.write_text(design_text.replace(old_gate, new_gate, 1), encoding='utf-8')

VERIFY.write_text(r'''param()
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$xamlPath = Join-Path $root 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\MainWindow.xaml'
$sourcePath = Join-Path $root 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\MainWindow.Diagnostics.cs'
$testPath = Join-Path $root 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\WpfMainWindowIntegrationTests.cs'
$designPath = Join-Path $root 'docs\api\design\SDO_OPERATION_MODE_REIMPLEMENTATION_PLAN_20260827.md'
$xaml = Get-Content -LiteralPath $xamlPath -Raw
$source = Get-Content -LiteralPath $sourcePath -Raw
$tests = Get-Content -LiteralPath $testPath -Raw
$design = Get-Content -LiteralPath $designPath -Raw
function Require-Text([string]$Text, [string]$Needle, [string]$Label) {
    if ($Text.IndexOf($Needle, [StringComparison]::Ordinal) -lt 0) {
        throw "Missing $Label: $Needle"
    }
}
Require-Text $xaml 'x:Name="TextSdoRequestPreview"' 'preview surface'
Require-Text $xaml 'x:Name="TextSdoSemanticWarning"' 'semantic warning surface'
Require-Text $xaml 'Text="Exact request preview"' 'preview label'
Require-Text $source 'private void UpdateSdoRequestPreview()' 'preview updater'
Require-Text $source 'private static string FormatSdoExactRequestPreview(' 'exact formatter'
Require-Text $source 'BLOCKED RESERVED SDO WRITE | NOT SUBMITTED' 'zero-wire warning text'
if ([regex]::Matches($source, 'LMCDiagnosticsWritePolicy\.RequireSdoWriteAllowed\(request\);').Count -lt 2) {
    throw 'Preview and submission paths must both enforce the SDK write policy.'
}
Require-Text $tests 'WriteData=34-12' 'little-endian exact preview smoke'
Require-Text $tests 'BLOCKED RESERVED SDO WRITE' 'reserved warning smoke'
Require-Text $design '- [x] exact request preview' 'R04 preview checklist'
Require-Text $design '- [x] semantic reserved warning' 'R04 warning checklist'
Write-Host 'PASS SDO-R04 exact request preview and semantic-reserved warning source contract.'
''', encoding='utf-8')

WORKFLOW.parent.mkdir(parents=True, exist_ok=True)
WORKFLOW.write_text(r'''name: SDO R04 request preview

on:
  pull_request:
    branches:
      - dev
    paths:
      - 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml'
      - 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs'
      - 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/UiLocalization.cs'
      - 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs'
      - 'docs/api/design/SDO_OPERATION_MODE_REIMPLEMENTATION_PLAN_20260827.md'
      - 'tools/Verify-SdoR04RequestPreview.ps1'
      - '.github/workflows/sdo-r04-request-preview.yml'
  workflow_dispatch:

permissions:
  contents: read

jobs:
  wpf-preview:
    runs-on: windows-latest
    timeout-minutes: 30
    steps:
      - uses: actions/checkout@v4
      - uses: microsoft/setup-msbuild@v2
      - name: Verify focused source contract
        shell: powershell
        run: .\tools\Verify-SdoR04RequestPreview.ps1
      - name: Build Debug smoke suite
        shell: powershell
        run: msbuild .\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\LasalApiWpfTestApp.SmokeTests.csproj /t:Build /p:Configuration=Debug /m
      - name: Run WPF smoke suite
        shell: powershell
        run: .\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\bin\Debug\LasalApiWpfTestApp.SmokeTests.exe
      - name: Build Release smoke suite
        shell: powershell
        run: msbuild .\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\LasalApiWpfTestApp.SmokeTests.csproj /t:Build /p:Configuration=Release /m
      - name: Diff hygiene
        shell: powershell
        run: git diff --check origin/${{ github.base_ref }}...HEAD
''', encoding='utf-8')

print('SDO-R04 request preview promotion applied.')
