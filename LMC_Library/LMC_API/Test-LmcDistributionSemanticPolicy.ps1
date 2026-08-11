[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'DistributionSemanticPolicy.ps1')

$script:DistributionSemanticPolicyTestCount = 0
$script:DistributionSemanticPolicyUtf8 = New-Object System.Text.UTF8Encoding($false)

function Assert-DistributionSemanticPolicyTest {
    param(
        [Parameter(Mandatory = $true)]
        [bool]$Condition,

        [Parameter(Mandatory = $true)]
        [string]$Message
    )

    if (-not $Condition) {
        throw ('TEST FAILURE: {0}' -f $Message)
    }
    $script:DistributionSemanticPolicyTestCount++
}

function Assert-DistributionSemanticPolicyBlocker {
    param(
        [Parameter(Mandatory = $true)]
        [scriptblock]$Action,

        [Parameter(Mandatory = $true)]
        [string]$ExpectedBlocker
    )

    try {
        $null = & $Action
    }
    catch {
        $actualBlocker = [string]$_.Exception.Data['Blocker']
        if ($actualBlocker -ne $ExpectedBlocker) {
            throw ('TEST FAILURE: expected blocker {0}, got {1}: {2}' -f
                $ExpectedBlocker,
                $actualBlocker,
                $_.Exception.Message)
        }
        $script:DistributionSemanticPolicyTestCount++
        return
    }

    throw ('TEST FAILURE: expected blocker {0}, but the policy returned PASS.' -f
        $ExpectedBlocker)
}

function Write-DistributionSemanticPolicyFixtureFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    $directory = Split-Path -Parent $Path
    if (-not (Test-Path -LiteralPath $directory -PathType Container)) {
        $null = New-Item -ItemType Directory -Path $directory -Force
    }
    [System.IO.File]::WriteAllText(
        $Path,
        $Text,
        $script:DistributionSemanticPolicyUtf8)
}

function Set-DistributionSemanticPolicyFixtureText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [string]$OldText,

        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$NewText
    )

    $text = [System.IO.File]::ReadAllText($Path)
    if (-not $text.Contains($OldText)) {
        throw ('Fixture mutation target was not found in {0}: {1}' -f $Path, $OldText)
    }
    [System.IO.File]::WriteAllText(
        $Path,
        $text.Replace($OldText, $NewText),
        $script:DistributionSemanticPolicyUtf8)
}

function Get-DistributionSemanticPolicyFixtureManualText {
    return @'
LASAL Motion Control API 0.9.1-preview is not production approved and is a production NO-GO.
Manual revision: 2.3-candidate.
The only manual SDO Write target is Axis 1 exact 0x2F00:24, Gold UI[24], Int32, DataLength=4, four-byte data.
Axis 2 through 4 and every other target remain blocked.
Manual SDO Write is identity-pinned to the current session, DiagnosticsBuild, BootId, MapRevision, and exact target.
The four-ticket same-value qualification must pass before manual SDO Write.
A success ACK is request acceptance, not completion; poll terminal state and status.
Close, Dispose, and cancellation do not send a PLC motion Stop; use an explicit safe-stop procedure.
Raw DINT UNIT conversion is performed by caller code.
Current PLC live SDO Write is not proven and remains unverified.
RPC_INIT_FRESH_TCP_ONCE_V1 accepts only the exact canonical ErrorId=-1 result for two same-socket attempts, then exactly one fresh TCP connection.
The second candidate failure is terminal; one UI operation is bounded to TCP 2 and 0x8080 4 requests.
The actual-EXE gate sends external WM_SYSCOMMAND/SC_CLOSE for the window X close, waits for process exit, and requires the successor to reacquire the default named mutex.
The fake-RPC wire total is 3/28 (13,2,13).
The actual-EXE gate is PC-loopback-only; PLC cleanup, disarm, and readiness are not proven.
The standalone binary-reference candidate gate is PASS; full Distribution did not reach the gate and is not PASS.
The motion/group 25-command matrix remains unfinished.
D1/D2/D5 fault/soak and D3/D4 runtime remain unfinished.
Before motion, verify the E-stop, hardware/software limits, UNIT, and Home.
The DLL is unsigned and has neither strong-name nor Authenticode signing.
'@
}

function New-DistributionSemanticPolicyDocumentProvider {
    param(
        [string]$Text,

        [string]$DocxText,

        [string]$PdfText
    )

    if (-not $PSBoundParameters.ContainsKey('DocxText')) {
        $DocxText = $Text
    }
    if (-not $PSBoundParameters.ContainsKey('PdfText')) {
        $PdfText = $Text
    }
    if ([string]::IsNullOrWhiteSpace($DocxText) -or
        [string]::IsNullOrWhiteSpace($PdfText)) {
        throw 'Both DOCX and PDF provider text are required.'
    }

    $capturedDocxText = $DocxText
    $capturedPdfText = $PdfText
    return ({
        param($Path)
        if ([string]::IsNullOrWhiteSpace([string]$Path)) {
            throw 'Document path was not supplied.'
        }
        $extension = [System.IO.Path]::GetExtension([string]$Path)
        if ($extension.Equals(
            '.docx',
            [System.StringComparison]::OrdinalIgnoreCase)) {
            return $capturedDocxText
        }
        if ($extension.Equals(
            '.pdf',
            [System.StringComparison]::OrdinalIgnoreCase)) {
            return $capturedPdfText
        }
        throw ('Unexpected document extension: {0}' -f $extension)
    }).GetNewClosure()
}

function New-DistributionSemanticPolicyFixture {
    param(
        [Parameter(Mandatory = $true)]
        [string]$BasePath,

        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    $repositoryRoot = Join-Path $BasePath $Name
    $candidateRoot = Join-Path $repositoryRoot 'Candidate'
    $sdkModelsPath = Join-Path $repositoryRoot 'LMC_Library\LMC_API_Delivery\src\LmcDiagnosticsD5Models.cs'
    $sdkDiagnosticsPath = Join-Path $repositoryRoot 'LMC_Library\LMC_API_Delivery\src\LmcDiagnosticsD5.cs'
    $lasalServicePath = Join-Path $repositoryRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCDiagnosticsService\LMCDiagnosticsService.st'
    $lasalDispatcherPath = Join-Path $repositoryRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\TCPMotionInterface\TCPMotionInterface.st'
    $dintMapPath = Join-Path $repositoryRoot 'LMC_Library\LMC_API_Delivery\docs\DINT_PACKET_MAP.txt'
    $currentWpfRoot = Join-Path $repositoryRoot 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp'
    $candidateWpfRoot = Join-Path $candidateRoot '02_Example_Program\LasalApiWpfTestApp'

    $sdkModels = @'
internal static class LMCDiagnosticsWritePolicy
{
    private static readonly bool SdoWriteEnabled = true;
    private static readonly bool SdoWriteUi24Axis1Enabled = true;
    private static readonly bool SdoWriteUi24Axis2Enabled = false;
    private static readonly bool SdoWriteUi24Axis3Enabled = false;
    private static readonly bool SdoWriteUi24Axis4Enabled = false;
    private static readonly uint[] AllowedPIWriteSignalIds = new uint[0];
    private static readonly object Target = new LMCSdoWriteTarget(
        "Reserved diagnostic UI[24]",
        1,
        0x2F00,
        24,
        LMCSignalValueType.Int32,
        4,
        -1073741823,
        1073741823);
}
'@
    Write-DistributionSemanticPolicyFixtureFile -Path $sdkModelsPath -Text $sdkModels

    $sdkDiagnostics = @'
internal async Task SubmitSdoWriteIdentityPinnedAsync()
{
    var freshCapabilities = GetCapabilities();
    ValidateRequiredSdoWriteSubmissionIdentity(freshCapabilities);
}
'@
    Write-DistributionSemanticPolicyFixtureFile -Path $sdkDiagnosticsPath -Text $sdkDiagnostics

    $lasalService = @'
#define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE
#define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE
#define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED FALSE
#define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED FALSE
#define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED FALSE
if (ObjectIndex <> 0x2F00) | (SubIndex <> 24) |
    (ValueType <> 4) | (DataLength <> 4) then
    DetailCode := 7;
end_if;
if CommandId = 0x7E00 then
    (pResponse + 20)^$UDINT := 0x0000613F;
    (pResponse + 20)^$UDINT := (pResponse + 20)^$UDINT | 0x00000200;
    ResponseSize := 68;
end_if;
case CommandId of
    0x7E21:
        if RequestSize <> 28 then
            detailCode := 12;
        else
            detailCode := 2;
        end_if;
end_case;
'@
    Write-DistributionSemanticPolicyFixtureFile -Path $lasalServicePath -Text $lasalService

    $lasalDispatcher = @'
case CommandID of
    0x7E00, 0x7E21, 0x7E22, 0x7E50:
        DiagnosticsService.Handle();
end_case;
'@
    Write-DistributionSemanticPolicyFixtureFile -Path $lasalDispatcherPath -Text $lasalDispatcher

    $dintMap = @'
Axis 1 exact 0x2F00:24 Int32/4 SDO Write is enabled. Axis 2..4 remain blocked.
Manual Write uses an identity-pinned gate and a four distinct tickets same-value proof.
Bits 15 remain zero. Bit 16 remains zero. Bit 17 remains zero; the read owners are dormant.
0x7E23 is absent and has no PLC route.
PI Write remains disabled.
D4 Double remains off. Double-bank bit 6 remains zero.
A success ACK is request acceptance, not completion.
Raw DINT UNIT values require caller conversion.
Current PLC live SDO Write is not proven and remains unverified.
'@
    Write-DistributionSemanticPolicyFixtureFile -Path $dintMapPath -Text $dintMap

    $projectText = @'
<?xml version="1.0" encoding="utf-8"?>
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <ItemGroup>
    <ApplicationDefinition Include="App.xaml" />
    <Page Include="MainWindow.xaml" />
    <Compile Include="MainWindow.xaml.cs" />
    <Compile Include="MainWindow.Diagnostics.cs" />
    <Compile Include="MainWindow.Qualification.cs" />
    <Compile Include="MainWindow.Qualification.SdoWrite.cs" />
    <Compile Include="SdoWriteActivationQualificationProof.cs" />
  </ItemGroup>
</Project>
'@
    $wpfFiles = [ordered]@{
        'App.xaml' = '<Application />'
        'MainWindow.xaml' = '<Window />'
        'MainWindow.xaml.cs' = @'
const string Marker = "CREVIS_TOPOLOGY_AXIS1_UI24_SDO_WRITE_LIVE_AXIS_QUAL_V5";
PowerOnAndWaitForStableStateAsync();
WaitForStandstillAsync();
ReadStatusResultAsync();
ToLasalDint();
var scaled = engineeringValue * unitMultiplier;
CloseCurrentConnectionAsync();
var closeSafety = "No Stop command is sent automatically";
'@
        'MainWindow.Diagnostics.cs' = 'await SubmitSdoWriteIdentityPinnedAsync(); // four-ticket'
        'MainWindow.Qualification.cs' = 'var cancelText = "Cancel Runner (not PLC Stop)";'
        'MainWindow.Qualification.SdoWrite.cs' = '// four-ticket current session qualification'
        'SdoWriteActivationQualificationProof.cs' = 'internal sealed class SdoWriteActivationQualificationProof { }'
    }
    foreach ($wpfRoot in @($currentWpfRoot, $candidateWpfRoot)) {
        Write-DistributionSemanticPolicyFixtureFile `
            -Path (Join-Path $wpfRoot 'LasalApiWpfTestApp.csproj') `
            -Text $projectText
        foreach ($entry in $wpfFiles.GetEnumerator()) {
            Write-DistributionSemanticPolicyFixtureFile `
                -Path (Join-Path $wpfRoot ([string]$entry.Key)) `
                -Text ([string]$entry.Value)
        }
    }

    $readme = 'LASAL Motion Control API 0.9.1-preview is not production approved; production NO-GO.'
    Write-DistributionSemanticPolicyFixtureFile -Path (Join-Path $candidateRoot 'README.md') -Text $readme
    Write-DistributionSemanticPolicyFixtureFile -Path (Join-Path $candidateRoot '02_Example_Program\README.md') -Text $readme
    Write-DistributionSemanticPolicyFixtureFile -Path (Join-Path $candidateRoot '03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.docx') -Text 'provider fixture'
    Write-DistributionSemanticPolicyFixtureFile -Path (Join-Path $candidateRoot '03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf') -Text 'provider fixture'

    return [pscustomobject]@{
        RepositoryRoot = $repositoryRoot
        CandidateRoot = $candidateRoot
        SdkModelsPath = $sdkModelsPath
        SdkDiagnosticsPath = $sdkDiagnosticsPath
        LasalServicePath = $lasalServicePath
        LasalDispatcherPath = $lasalDispatcherPath
        DintMapPath = $dintMapPath
        CurrentWpfRoot = $currentWpfRoot
        CandidateWpfRoot = $candidateWpfRoot
        ManualText = Get-DistributionSemanticPolicyFixtureManualText
    }
}

function Invoke-DistributionSemanticPolicyFixture {
    param(
        [Parameter(Mandatory = $true)]
        [psobject]$Fixture,

        [string]$ManualText,

        [string]$DocxText,

        [string]$PdfText
    )

    if (-not $PSBoundParameters.ContainsKey('ManualText')) {
        $ManualText = [string]$Fixture.ManualText
    }
    if (-not $PSBoundParameters.ContainsKey('DocxText')) {
        $DocxText = $ManualText
    }
    if (-not $PSBoundParameters.ContainsKey('PdfText')) {
        $PdfText = $ManualText
    }
    $provider = New-DistributionSemanticPolicyDocumentProvider `
        -DocxText $DocxText `
        -PdfText $PdfText
    return Test-LmcDistributionSemanticPolicy `
        -RepositoryRoot $Fixture.RepositoryRoot `
        -CandidateRoot $Fixture.CandidateRoot `
        -PythonPath 'provider-not-used' `
        -DocumentTextProvider $provider
}

$temporaryBase = Join-Path ([System.IO.Path]::GetTempPath()) (
    'LmcDistributionSemanticPolicyTests_' + [guid]::NewGuid().ToString('N'))

try {
    $null = New-Item -ItemType Directory -Path $temporaryBase

    $passFixture = New-DistributionSemanticPolicyFixture -BasePath $temporaryBase -Name 'pass'
    $passResult = Invoke-DistributionSemanticPolicyFixture -Fixture $passFixture
    Assert-DistributionSemanticPolicyTest `
        -Condition ($passResult.Result -ceq 'PASS') `
        -Message 'PASS fixture did not return Result=PASS.'
    Assert-DistributionSemanticPolicyTest `
        -Condition ([regex]::IsMatch($passResult.PolicySha256, '^[0-9A-F]{64}$')) `
        -Message 'PolicySha256 is not 64 uppercase hexadecimal characters.'
    Assert-DistributionSemanticPolicyTest `
        -Condition ($passResult.CheckCount -eq 18) `
        -Message 'PASS fixture did not return the exact 18 policy checks.'

    $secondPassResult = Invoke-DistributionSemanticPolicyFixture -Fixture $passFixture
    Assert-DistributionSemanticPolicyTest `
        -Condition ($passResult.PolicySha256 -ceq $secondPassResult.PolicySha256) `
        -Message 'Policy hash is not deterministic across identical invocations.'
    Assert-DistributionSemanticPolicyTest `
        -Condition ((Get-LmcDistributionSemanticPolicySha256) -ceq
            (Get-LmcDistributionSemanticPolicySha256)) `
        -Message 'Canonical policy hash is not deterministic.'
    $canonicalPolicyText = Get-LmcDistributionSemanticPolicyText
    Assert-DistributionSemanticPolicyTest `
        -Condition (Test-LmcDistributionPolicyPatterns `
            -Text $canonicalPolicyText `
            -Patterns @(
                '(?:^|\n)MANUAL_RECONNECT_SCOPE=',
                '(?:^|\n)MANUAL_RELEASE_WARNING_SCOPE=',
                '(?:^|\n)MANUAL_VERSION_SCOPE=')) `
        -Message 'Canonical policy text is missing a new manual policy definition.'
    $manualReleasePassResult = Test-LmcDistributionManualReleasePolicy `
        -DocxText $passFixture.ManualText `
        -PdfText $passFixture.ManualText
    Assert-DistributionSemanticPolicyTest `
        -Condition (($manualReleasePassResult.Result -ceq 'PASS') -and
            ($manualReleasePassResult.CheckCount -eq 3)) `
        -Message 'Direct manual release-policy helper did not return exact 3/3 PASS.'
    $manualReleaseNonPassText = $passFixture.ManualText +
        "`nFull Distribution is not PASS."
    $manualReleaseNonPassResult = Test-LmcDistributionManualReleasePolicy `
        -DocxText $manualReleaseNonPassText `
        -PdfText $manualReleaseNonPassText
    Assert-DistributionSemanticPolicyTest `
        -Condition (($manualReleaseNonPassResult.Result -ceq 'PASS') -and
            ($manualReleaseNonPassResult.CheckCount -eq 3)) `
        -Message 'Explicit full Distribution non-PASS wording was rejected.'

    $actualRepositoryRoot = [System.IO.Path]::GetFullPath(
        (Join-Path $PSScriptRoot '..\..'))
    $actualCurrentWpfRoot = Join-Path $actualRepositoryRoot 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp'
    $actualCurrentProject = Join-Path $actualCurrentWpfRoot 'LasalApiWpfTestApp.csproj'
    $actualCandidateRoot = Join-Path $temporaryBase 'actual_current_candidate'
    $actualCandidateWpfRoot = Join-Path $actualCandidateRoot '02_Example_Program\LasalApiWpfTestApp'
    $actualCandidateProject = Join-Path $actualCandidateWpfRoot 'LasalApiWpfTestApp.csproj'
    $actualItems = @(Get-LmcDistributionPolicyProjectItems `
        -ProjectPath $actualCurrentProject `
        -Blocker 'TEST_ACTUAL_CURRENT_SOURCE')
    Write-DistributionSemanticPolicyFixtureFile `
        -Path $actualCandidateProject `
        -Text ([System.IO.File]::ReadAllText($actualCurrentProject))
    foreach ($item in $actualItems) {
        $sourcePath = Get-LmcDistributionPolicyProjectItemPath `
            -ProjectPath $actualCurrentProject `
            -ProjectItem $item
        $destinationPath = Get-LmcDistributionPolicyProjectItemPath `
            -ProjectPath $actualCandidateProject `
            -ProjectItem $item
        $destinationDirectory = Split-Path -Parent $destinationPath
        if (-not (Test-Path -LiteralPath $destinationDirectory -PathType Container)) {
            $null = New-Item -ItemType Directory -Path $destinationDirectory -Force
        }
        [System.IO.File]::Copy($sourcePath, $destinationPath, $true)
    }
    $actualReadme = 'LASAL Motion Control API 0.9.1-preview is not production approved; production NO-GO.'
    Write-DistributionSemanticPolicyFixtureFile `
        -Path (Join-Path $actualCandidateRoot 'README.md') `
        -Text $actualReadme
    Write-DistributionSemanticPolicyFixtureFile `
        -Path (Join-Path $actualCandidateRoot '02_Example_Program\README.md') `
        -Text $actualReadme
    Write-DistributionSemanticPolicyFixtureFile `
        -Path (Join-Path $actualCandidateRoot '03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.docx') `
        -Text 'provider fixture'
    Write-DistributionSemanticPolicyFixtureFile `
        -Path (Join-Path $actualCandidateRoot '03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf') `
        -Text 'provider fixture'
    $actualProvider = New-DistributionSemanticPolicyDocumentProvider `
        -Text (Get-DistributionSemanticPolicyFixtureManualText)
    $actualSourceResult = Test-LmcDistributionSemanticPolicy `
        -RepositoryRoot $actualRepositoryRoot `
        -CandidateRoot $actualCandidateRoot `
        -PythonPath 'provider-not-used' `
        -DocumentTextProvider $actualProvider
    Assert-DistributionSemanticPolicyTest `
        -Condition (($actualSourceResult.Result -ceq 'PASS') -and
            ($actualSourceResult.CheckCount -eq $passResult.CheckCount)) `
        -Message 'Actual current SDK/LASAL/WPF/DINT source contract did not pass with a synchronized candidate and canonical manual.'

    $fixture = New-DistributionSemanticPolicyFixture `
        -BasePath $temporaryBase `
        -Name 'manual_version_docx'
    $driftText = $fixture.ManualText.Replace(
        '2.3-candidate',
        '2.2-candidate')
    Assert-DistributionSemanticPolicyBlocker `
        -ExpectedBlocker 'MANUAL_VERSION_SCOPE' `
        -Action {
            Invoke-DistributionSemanticPolicyFixture `
                -Fixture $fixture `
                -DocxText $driftText
        }

    $fixture = New-DistributionSemanticPolicyFixture `
        -BasePath $temporaryBase `
        -Name 'manual_version_pdf'
    $driftText = $fixture.ManualText.Replace(
        '2.3-candidate',
        '2.2-candidate')
    Assert-DistributionSemanticPolicyBlocker `
        -ExpectedBlocker 'MANUAL_VERSION_SCOPE' `
        -Action {
            Invoke-DistributionSemanticPolicyFixture `
                -Fixture $fixture `
                -PdfText $driftText
        }

    $fixture = New-DistributionSemanticPolicyFixture `
        -BasePath $temporaryBase `
        -Name 'manual_version_history_only'
    $driftText = $fixture.ManualText.Replace(
        'Manual revision: 2.3-candidate.',
        "Manual revision: 2.4.`nRevision history includes 2.3-candidate.")
    Assert-DistributionSemanticPolicyBlocker `
        -ExpectedBlocker 'MANUAL_VERSION_SCOPE' `
        -Action {
            Invoke-DistributionSemanticPolicyFixture `
                -Fixture $fixture `
                -DocxText $driftText
        }

    $fixture = New-DistributionSemanticPolicyFixture `
        -BasePath $temporaryBase `
        -Name 'manual_reconnect_docx'
    $driftText = $fixture.ManualText.Replace(
        'RPC_INIT_FRESH_TCP_ONCE_V1',
        'LEGACY_RECONNECT_POLICY')
    Assert-DistributionSemanticPolicyBlocker `
        -ExpectedBlocker 'MANUAL_RECONNECT_SCOPE' `
        -Action {
            Invoke-DistributionSemanticPolicyFixture `
                -Fixture $fixture `
                -DocxText $driftText
        }

    $fixture = New-DistributionSemanticPolicyFixture `
        -BasePath $temporaryBase `
        -Name 'manual_reconnect_docx_semantic_reversal'
    $driftText = $fixture.ManualText +
        "`nThe exact canonical ErrorId=-1 is no longer required; two same-socket attempts are not a limit."
    Assert-DistributionSemanticPolicyBlocker `
        -ExpectedBlocker 'MANUAL_RECONNECT_SCOPE' `
        -Action {
            Invoke-DistributionSemanticPolicyFixture `
                -Fixture $fixture `
                -DocxText $driftText
        }

    $fixture = New-DistributionSemanticPolicyFixture `
        -BasePath $temporaryBase `
        -Name 'manual_reconnect_pdf_overclaim'
    $driftText = $fixture.ManualText +
        "`nThe actual-EXE gate proves PLC cleanup and readiness."
    Assert-DistributionSemanticPolicyBlocker `
        -ExpectedBlocker 'MANUAL_RECONNECT_SCOPE' `
        -Action {
            Invoke-DistributionSemanticPolicyFixture `
                -Fixture $fixture `
                -PdfText $driftText
        }

    $fixture = New-DistributionSemanticPolicyFixture `
        -BasePath $temporaryBase `
        -Name 'manual_reconnect_docx_minus_one_optional'
    $driftText = $fixture.ManualText +
        "`nThe exact canonical ErrorId=-1 result is not required."
    Assert-DistributionSemanticPolicyBlocker `
        -ExpectedBlocker 'MANUAL_RECONNECT_SCOPE' `
        -Action {
            Invoke-DistributionSemanticPolicyFixture `
                -Fixture $fixture `
                -DocxText $driftText
        }

    $fixture = New-DistributionSemanticPolicyFixture `
        -BasePath $temporaryBase `
        -Name 'manual_reconnect_pdf_multiple_fresh_tcp'
    $driftText = $fixture.ManualText +
        "`nThe fresh TCP policy permits more than one fresh TCP connection."
    Assert-DistributionSemanticPolicyBlocker `
        -ExpectedBlocker 'MANUAL_RECONNECT_SCOPE' `
        -Action {
            Invoke-DistributionSemanticPolicyFixture `
                -Fixture $fixture `
                -PdfText $driftText
        }

    $fixture = New-DistributionSemanticPolicyFixture `
        -BasePath $temporaryBase `
        -Name 'manual_reconnect_docx_full_distribution_pass'
    $driftText = $fixture.ManualText +
        "`nFull Distribution is PASS."
    Assert-DistributionSemanticPolicyBlocker `
        -ExpectedBlocker 'MANUAL_RECONNECT_SCOPE' `
        -Action {
            Invoke-DistributionSemanticPolicyFixture `
                -Fixture $fixture `
                -DocxText $driftText
        }

    $fixture = New-DistributionSemanticPolicyFixture `
        -BasePath $temporaryBase `
        -Name 'manual_release_docx_matrix'
    $driftText = $fixture.ManualText.Replace(
        'The motion/group 25-command matrix remains unfinished.',
        'Motion APIs are documented.')
    Assert-DistributionSemanticPolicyBlocker `
        -ExpectedBlocker 'MANUAL_RELEASE_WARNING_SCOPE' `
        -Action {
            Invoke-DistributionSemanticPolicyFixture `
                -Fixture $fixture `
                -DocxText $driftText
        }

    $fixture = New-DistributionSemanticPolicyFixture `
        -BasePath $temporaryBase `
        -Name 'manual_release_pdf_semantic_reversal'
    $driftText = $fixture.ManualText +
        "`nThe 25-command matrix is no longer unfinished and is qualified. D1/D2/D5 fault/soak and D3/D4 runtime are qualified. Close constitutes the safe-stop. E-stop, limits, UNIT, and Home can be omitted. The DLL is no longer unsigned; strong-name and Authenticode are enabled."
    Assert-DistributionSemanticPolicyBlocker `
        -ExpectedBlocker 'MANUAL_RELEASE_WARNING_SCOPE' `
        -Action {
            Invoke-DistributionSemanticPolicyFixture `
                -Fixture $fixture `
                -PdfText $driftText
        }

    $fixture = New-DistributionSemanticPolicyFixture `
        -BasePath $temporaryBase `
        -Name 'manual_release_pdf_matrix_complete'
    $driftText = $fixture.ManualText +
        "`nThe motion/group 25-command matrix is complete; D1/D2/D5 fault/soak and D3/D4 runtime are complete."
    Assert-DistributionSemanticPolicyBlocker `
        -ExpectedBlocker 'MANUAL_RELEASE_WARNING_SCOPE' `
        -Action {
            Invoke-DistributionSemanticPolicyFixture `
                -Fixture $fixture `
                -PdfText $driftText
        }

    $fixture = New-DistributionSemanticPolicyFixture `
        -BasePath $temporaryBase `
        -Name 'manual_release_docx_close_safe_stop_overclaim'
    $driftText = $fixture.ManualText +
        "`nClose is a safe-stop."
    Assert-DistributionSemanticPolicyBlocker `
        -ExpectedBlocker 'MANUAL_RELEASE_WARNING_SCOPE' `
        -Action {
            Invoke-DistributionSemanticPolicyFixture `
                -Fixture $fixture `
                -DocxText $driftText
        }

    $fixture = New-DistributionSemanticPolicyFixture `
        -BasePath $temporaryBase `
        -Name 'manual_release_pdf_safety_unnecessary'
    $driftText = $fixture.ManualText +
        "`nE-stop, hardware/software limits, UNIT, and Home checks are unnecessary."
    Assert-DistributionSemanticPolicyBlocker `
        -ExpectedBlocker 'MANUAL_RELEASE_WARNING_SCOPE' `
        -Action {
            Invoke-DistributionSemanticPolicyFixture `
                -Fixture $fixture `
                -PdfText $driftText
        }

    $fixture = New-DistributionSemanticPolicyFixture `
        -BasePath $temporaryBase `
        -Name 'manual_release_docx_dll_signed'
    $driftText = $fixture.ManualText +
        "`nThe DLL is strong-name and Authenticode signed."
    Assert-DistributionSemanticPolicyBlocker `
        -ExpectedBlocker 'MANUAL_RELEASE_WARNING_SCOPE' `
        -Action {
            Invoke-DistributionSemanticPolicyFixture `
                -Fixture $fixture `
                -DocxText $driftText
        }

    $fixture = New-DistributionSemanticPolicyFixture `
        -BasePath $temporaryBase `
        -Name 'manual_release_pdf_diagnostics'
    $driftText = $fixture.ManualText.Replace(
        'D1/D2/D5 fault/soak and D3/D4 runtime remain unfinished.',
        'Diagnostics are documented.')
    Assert-DistributionSemanticPolicyBlocker `
        -ExpectedBlocker 'MANUAL_RELEASE_WARNING_SCOPE' `
        -Action {
            Invoke-DistributionSemanticPolicyFixture `
                -Fixture $fixture `
                -PdfText $driftText
        }

    $fixture = New-DistributionSemanticPolicyFixture `
        -BasePath $temporaryBase `
        -Name 'manual_release_docx_safe_stop'
    $driftText = $fixture.ManualText.Replace(
        'Close, Dispose, and cancellation do not send a PLC motion Stop; use an explicit safe-stop procedure.',
        'Close and cancellation do not send a PLC motion Stop.')
    Assert-DistributionSemanticPolicyBlocker `
        -ExpectedBlocker 'MANUAL_RELEASE_WARNING_SCOPE' `
        -Action {
            Invoke-DistributionSemanticPolicyFixture `
                -Fixture $fixture `
                -DocxText $driftText
        }

    $fixture = New-DistributionSemanticPolicyFixture `
        -BasePath $temporaryBase `
        -Name 'manual_release_pdf_machine_safety'
    $driftText = $fixture.ManualText.Replace(
        'Before motion, verify the E-stop, hardware/software limits, UNIT, and Home.',
        'Before motion, review the machine instructions.')
    Assert-DistributionSemanticPolicyBlocker `
        -ExpectedBlocker 'MANUAL_RELEASE_WARNING_SCOPE' `
        -Action {
            Invoke-DistributionSemanticPolicyFixture `
                -Fixture $fixture `
                -PdfText $driftText
        }

    $fixture = New-DistributionSemanticPolicyFixture `
        -BasePath $temporaryBase `
        -Name 'manual_release_docx_signing'
    $driftText = $fixture.ManualText.Replace(
        'The DLL is unsigned and has neither strong-name nor Authenticode signing.',
        'DLL signing information is omitted.')
    Assert-DistributionSemanticPolicyBlocker `
        -ExpectedBlocker 'MANUAL_RELEASE_WARNING_SCOPE' `
        -Action {
            Invoke-DistributionSemanticPolicyFixture `
                -Fixture $fixture `
                -DocxText $driftText
        }

    $fixture = New-DistributionSemanticPolicyFixture -BasePath $temporaryBase -Name 'manual_scope'
    $staleManual = @'
LASAL Motion Control API 0.9.1-preview is not production approved.
SDO Write gate is OFF and the approved target count is zero.
'@
    Assert-DistributionSemanticPolicyBlocker -ExpectedBlocker 'MANUAL_SDO_WRITE_SCOPE' -Action {
        Invoke-DistributionSemanticPolicyFixture -Fixture $fixture -ManualText $staleManual
    }

    $fixture = New-DistributionSemanticPolicyFixture -BasePath $temporaryBase -Name 'manual_axis_block'
    $driftText = $fixture.ManualText.Replace(
        'Axis 2 through 4 and every other target remain blocked.',
        'Other target details are omitted.')
    Assert-DistributionSemanticPolicyBlocker -ExpectedBlocker 'MANUAL_SDO_WRITE_SCOPE' -Action {
        Invoke-DistributionSemanticPolicyFixture -Fixture $fixture -ManualText $driftText
    }

    $fixture = New-DistributionSemanticPolicyFixture -BasePath $temporaryBase -Name 'manual_conflicting_target'
    $driftText = $fixture.ManualText + "`nAxis 2 SDO Write is enabled and supported."
    Assert-DistributionSemanticPolicyBlocker -ExpectedBlocker 'MANUAL_SDO_WRITE_SCOPE' -Action {
        Invoke-DistributionSemanticPolicyFixture -Fixture $fixture -ManualText $driftText
    }

    $fixture = New-DistributionSemanticPolicyFixture -BasePath $temporaryBase -Name 'preview'
    $driftText = $fixture.ManualText.Replace(
        'LASAL Motion Control API 0.9.1-preview is not production approved and is a production NO-GO.',
        'LASAL Motion Control API release documentation.')
    Assert-DistributionSemanticPolicyBlocker -ExpectedBlocker 'PREVIEW_PRODUCTION_NO_GO' -Action {
        Invoke-DistributionSemanticPolicyFixture -Fixture $fixture -ManualText $driftText
    }

    $fixture = New-DistributionSemanticPolicyFixture -BasePath $temporaryBase -Name 'ack'
    $driftText = $fixture.ManualText.Replace(
        'A success ACK is request acceptance, not completion; poll terminal state and status.',
        'A success response is returned.')
    Assert-DistributionSemanticPolicyBlocker -ExpectedBlocker 'ACK_NOT_COMPLETION' -Action {
        Invoke-DistributionSemanticPolicyFixture -Fixture $fixture -ManualText $driftText
    }

    $fixture = New-DistributionSemanticPolicyFixture -BasePath $temporaryBase -Name 'ack_wpf'
    Set-DistributionSemanticPolicyFixtureText `
        -Path (Join-Path $fixture.CurrentWpfRoot 'MainWindow.xaml.cs') `
        -OldText 'PowerOnAndWaitForStableStateAsync()' `
        -NewText 'PowerOnAsync()'
    Assert-DistributionSemanticPolicyBlocker -ExpectedBlocker 'ACK_NOT_COMPLETION' -Action {
        Invoke-DistributionSemanticPolicyFixture -Fixture $fixture
    }

    $fixture = New-DistributionSemanticPolicyFixture -BasePath $temporaryBase -Name 'close_cancel'
    $driftText = $fixture.ManualText.Replace(
        'Close, Dispose, and cancellation do not send a PLC motion Stop; use an explicit safe-stop procedure.',
        'Connection lifecycle behavior is documented elsewhere.')
    Assert-DistributionSemanticPolicyBlocker -ExpectedBlocker 'CLOSE_CANCEL_NOT_STOP' -Action {
        Invoke-DistributionSemanticPolicyFixture -Fixture $fixture -ManualText $driftText
    }

    $fixture = New-DistributionSemanticPolicyFixture -BasePath $temporaryBase -Name 'close_wpf'
    Set-DistributionSemanticPolicyFixtureText `
        -Path (Join-Path $fixture.CurrentWpfRoot 'MainWindow.xaml.cs') `
        -OldText 'No Stop command is sent automatically' `
        -NewText 'Closing connection'
    Assert-DistributionSemanticPolicyBlocker -ExpectedBlocker 'CLOSE_CANCEL_NOT_STOP' -Action {
        Invoke-DistributionSemanticPolicyFixture -Fixture $fixture
    }

    $fixture = New-DistributionSemanticPolicyFixture -BasePath $temporaryBase -Name 'unit'
    $driftText = $fixture.ManualText.Replace(
        'Raw DINT UNIT conversion is performed by caller code.',
        'Values are transmitted.')
    Assert-DistributionSemanticPolicyBlocker -ExpectedBlocker 'UNIT_CALLER_CONVERSION' -Action {
        Invoke-DistributionSemanticPolicyFixture -Fixture $fixture -ManualText $driftText
    }

    $fixture = New-DistributionSemanticPolicyFixture -BasePath $temporaryBase -Name 'unit_wpf'
    Set-DistributionSemanticPolicyFixtureText `
        -Path (Join-Path $fixture.CurrentWpfRoot 'MainWindow.xaml.cs') `
        -OldText 'engineeringValue * unitMultiplier' `
        -NewText 'engineeringValue'
    Assert-DistributionSemanticPolicyBlocker -ExpectedBlocker 'UNIT_CALLER_CONVERSION' -Action {
        Invoke-DistributionSemanticPolicyFixture -Fixture $fixture
    }

    $fixture = New-DistributionSemanticPolicyFixture -BasePath $temporaryBase -Name 'plc_live'
    $driftText = $fixture.ManualText.Replace(
        'Current PLC live SDO Write is not proven and remains unverified.',
        'Current PLC SDO Write is available.')
    $driftText = $driftText.Replace(
        'The actual-EXE gate is PC-loopback-only; PLC cleanup, disarm, and readiness are not proven.',
        'The actual-EXE gate is PC-loopback-only.')
    Assert-DistributionSemanticPolicyBlocker -ExpectedBlocker 'PLC_LIVE_UNVERIFIED' -Action {
        Invoke-DistributionSemanticPolicyFixture -Fixture $fixture -ManualText $driftText
    }

    $fixture = New-DistributionSemanticPolicyFixture -BasePath $temporaryBase -Name 'sdk_sdo_scope'
    Set-DistributionSemanticPolicyFixtureText `
        -Path $fixture.SdkModelsPath `
        -OldText 'SdoWriteUi24Axis1Enabled = true' `
        -NewText 'SdoWriteUi24Axis1Enabled = false'
    Assert-DistributionSemanticPolicyBlocker -ExpectedBlocker 'SDO_WRITE_SCOPE' -Action {
        Invoke-DistributionSemanticPolicyFixture -Fixture $fixture
    }

    $fixture = New-DistributionSemanticPolicyFixture -BasePath $temporaryBase -Name 'sdk_extra_target'
    $sdkText = [System.IO.File]::ReadAllText($fixture.SdkModelsPath) + @'

private static readonly object ExtraTarget = new LMCSdoWriteTarget(
    "Unexpected target", 1, 0x3000, 1, LMCSignalValueType.Int32, 4, 0, 1);
'@
    [System.IO.File]::WriteAllText(
        $fixture.SdkModelsPath,
        $sdkText,
        $script:DistributionSemanticPolicyUtf8)
    Assert-DistributionSemanticPolicyBlocker -ExpectedBlocker 'SDO_WRITE_SCOPE' -Action {
        Invoke-DistributionSemanticPolicyFixture -Fixture $fixture
    }

    $fixture = New-DistributionSemanticPolicyFixture -BasePath $temporaryBase -Name 'dint_sdo_scope'
    $dintText = [System.IO.File]::ReadAllText($fixture.DintMapPath) + "`nThe target is not approved yet; the allowlist is empty."
    [System.IO.File]::WriteAllText($fixture.DintMapPath, $dintText, $script:DistributionSemanticPolicyUtf8)
    Assert-DistributionSemanticPolicyBlocker -ExpectedBlocker 'SDO_WRITE_SCOPE' -Action {
        Invoke-DistributionSemanticPolicyFixture -Fixture $fixture
    }

    $fixture = New-DistributionSemanticPolicyFixture -BasePath $temporaryBase -Name 'sdk_identity'
    Set-DistributionSemanticPolicyFixtureText `
        -Path $fixture.SdkDiagnosticsPath `
        -OldText 'freshCapabilities = GetCapabilities()' `
        -NewText 'freshCapabilities = cachedCapabilities'
    Assert-DistributionSemanticPolicyBlocker -ExpectedBlocker 'SDO_WRITE_IDENTITY_PIN' -Action {
        Invoke-DistributionSemanticPolicyFixture -Fixture $fixture
    }

    $fixture = New-DistributionSemanticPolicyFixture -BasePath $temporaryBase -Name 'wpf_identity'
    Set-DistributionSemanticPolicyFixtureText `
        -Path (Join-Path $fixture.CurrentWpfRoot 'MainWindow.xaml.cs') `
        -OldText 'CREVIS_TOPOLOGY_AXIS1_UI24_SDO_WRITE_LIVE_AXIS_QUAL_V5' `
        -NewText 'CREVIS_TOPOLOGY_EDITABLE_SDO_DRAFT_V2'
    Assert-DistributionSemanticPolicyBlocker -ExpectedBlocker 'CURRENT_WPF_IDENTITY_PIN' -Action {
        Invoke-DistributionSemanticPolicyFixture -Fixture $fixture
    }

    $fixture = New-DistributionSemanticPolicyFixture -BasePath $temporaryBase -Name 'dormant_bits'
    Set-DistributionSemanticPolicyFixtureText `
        -Path $fixture.LasalServicePath `
        -OldText '| 0x00000200;' `
        -NewText '| 0x00000200 | 0x00008000;'
    Assert-DistributionSemanticPolicyBlocker -ExpectedBlocker 'TOPOLOGY_DORMANT_BITS' -Action {
        Invoke-DistributionSemanticPolicyFixture -Fixture $fixture
    }

    $fixture = New-DistributionSemanticPolicyFixture -BasePath $temporaryBase -Name 'route_7e23'
    Set-DistributionSemanticPolicyFixtureText `
        -Path $fixture.LasalDispatcherPath `
        -OldText '0x7E00, 0x7E21, 0x7E22, 0x7E50' `
        -NewText '0x7E00, 0x7E21, 0x7E22, 0x7E23, 0x7E50'
    Assert-DistributionSemanticPolicyBlocker -ExpectedBlocker 'TOPOLOGY_7E23_ABSENT' -Action {
        Invoke-DistributionSemanticPolicyFixture -Fixture $fixture
    }

    $fixture = New-DistributionSemanticPolicyFixture -BasePath $temporaryBase -Name 'pi_write'
    Set-DistributionSemanticPolicyFixtureText `
        -Path $fixture.SdkModelsPath `
        -OldText 'AllowedPIWriteSignalIds = new uint[0]' `
        -NewText 'AllowedPIWriteSignalIds = new uint[1]'
    Assert-DistributionSemanticPolicyBlocker -ExpectedBlocker 'PI_WRITE_DISABLED' -Action {
        Invoke-DistributionSemanticPolicyFixture -Fixture $fixture
    }

    $fixture = New-DistributionSemanticPolicyFixture -BasePath $temporaryBase -Name 'd4_double'
    Set-DistributionSemanticPolicyFixtureText `
        -Path $fixture.LasalServicePath `
        -OldText '| 0x00000200;' `
        -NewText '| 0x00000200 | 0x00000040;'
    Assert-DistributionSemanticPolicyBlocker -ExpectedBlocker 'D4_DOUBLE_DISABLED' -Action {
        Invoke-DistributionSemanticPolicyFixture -Fixture $fixture
    }

    $fixture = New-DistributionSemanticPolicyFixture -BasePath $temporaryBase -Name 'candidate_set'
    Set-DistributionSemanticPolicyFixtureText `
        -Path (Join-Path $fixture.CandidateWpfRoot 'LasalApiWpfTestApp.csproj') `
        -OldText '    <Compile Include="SdoWriteActivationQualificationProof.cs" />' `
        -NewText ''
    Assert-DistributionSemanticPolicyBlocker -ExpectedBlocker 'CANDIDATE_WPF_SOURCE_SET' -Action {
        Invoke-DistributionSemanticPolicyFixture -Fixture $fixture
    }

    $fixture = New-DistributionSemanticPolicyFixture -BasePath $temporaryBase -Name 'candidate_content'
    Set-DistributionSemanticPolicyFixtureText `
        -Path (Join-Path $fixture.CandidateWpfRoot 'MainWindow.Diagnostics.cs') `
        -OldText 'four-ticket' `
        -NewText 'editable-draft'
    Assert-DistributionSemanticPolicyBlocker -ExpectedBlocker 'CANDIDATE_WPF_SOURCE_CONTENT' -Action {
        Invoke-DistributionSemanticPolicyFixture -Fixture $fixture
    }

    [pscustomobject]@{
        Result = 'PASS'
        TestCount = $script:DistributionSemanticPolicyTestCount
        PolicySha256 = $passResult.PolicySha256
        PolicyCheckCount = $passResult.CheckCount
    }
}
finally {
    $resolvedTempRoot = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
    $resolvedTestRoot = [System.IO.Path]::GetFullPath($temporaryBase)
    if ($resolvedTestRoot.StartsWith(
        $resolvedTempRoot,
        [System.StringComparison]::OrdinalIgnoreCase) -and
        (Split-Path -Leaf $resolvedTestRoot).StartsWith(
            'LmcDistributionSemanticPolicyTests_',
            [System.StringComparison]::Ordinal)) {
        Remove-Item -LiteralPath $resolvedTestRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
}
