Set-StrictMode -Version Latest

function New-LmcDistributionSemanticPolicyException {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Blocker,

        [Parameter(Mandatory = $true)]
        [string]$Message
    )

    $exception = New-Object System.InvalidOperationException(
        ('[{0}] {1}' -f $Blocker, $Message))
    $exception.Data['Blocker'] = $Blocker
    return $exception
}

function Assert-LmcDistributionSemanticPolicy {
    param(
        [Parameter(Mandatory = $true)]
        [bool]$Condition,

        [Parameter(Mandatory = $true)]
        [string]$Blocker,

        [Parameter(Mandatory = $true)]
        [string]$Message
    )

    if (-not $Condition) {
        throw (New-LmcDistributionSemanticPolicyException `
            -Blocker $Blocker `
            -Message $Message)
    }
}

function Get-LmcDistributionSemanticPolicyText {
    $definitions = @(
        [pscustomobject]@{
            Name = 'ACK_NOT_COMPLETION'
            Statement = 'A success ACK proves request acceptance only; terminal state/status polling proves completion.'
        },
        [pscustomobject]@{
            Name = 'CLOSE_CANCEL_NOT_STOP'
            Statement = 'Connection Close, Dispose, timeout, cancellation, and qualification-runner Cancel do not send motion Stop.'
        },
        [pscustomobject]@{
            Name = 'D4_DOUBLE_DISABLED'
            Statement = 'D4 Double-bank operation and capability bit 6 remain disabled.'
        },
        [pscustomobject]@{
            Name = 'PI_WRITE_DISABLED'
            Statement = 'PI Write remains disabled and the SDK PI Write allowlist remains empty.'
        },
        [pscustomobject]@{
            Name = 'PLC_LIVE_UNVERIFIED'
            Statement = 'Current PLC download/runtime/live SDO Write remains unverified and must not be inferred from static or PC tests.'
        },
        [pscustomobject]@{
            Name = 'PREVIEW_PRODUCTION_NO_GO'
            Statement = 'The package remains preview and is not production approved.'
        },
        [pscustomobject]@{
            Name = 'SDO_WRITE_IDENTITY_PIN'
            Statement = 'Manual SDO Write requires a current-session DiagnosticsBuild, BootId, MapRevision, exact-target identity pin and a four-ticket same-value proof.'
        },
        [pscustomobject]@{
            Name = 'SDO_WRITE_SCOPE'
            Statement = 'The only enabled SDO Write target is Axis 1 0x2F00:24 Int32 with exactly four data bytes; Axis 2 through 4 and all other targets remain blocked.'
        },
        [pscustomobject]@{
            Name = 'TOPOLOGY_7E23_ABSENT'
            Statement = 'Command 0x7E23 has no PLC dispatcher route and remains unsupported.'
        },
        [pscustomobject]@{
            Name = 'TOPOLOGY_DORMANT_BITS'
            Statement = 'Capability bits 15, 16, and 17 remain zero; node health and digital I/O read owners are dormant.'
        },
        [pscustomobject]@{
            Name = 'UNIT_CALLER_CONVERSION'
            Statement = 'Motion values are raw DINT values and UNIT conversion is performed by caller code.'
        }
    )

    $lines = New-Object 'System.Collections.Generic.List[string]'
    foreach ($definition in $definitions) {
        [void]$lines.Add(('{0}={1}' -f $definition.Name, $definition.Statement))
    }
    $lines.Sort([System.StringComparer]::Ordinal)
    return [string]::Join("`n", $lines.ToArray())
}

function Get-LmcDistributionSemanticPolicySha256 {
    $canonicalText = Get-LmcDistributionSemanticPolicyText
    $utf8 = New-Object System.Text.UTF8Encoding($false)
    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    try {
        $bytes = $utf8.GetBytes($canonicalText)
        return ([System.BitConverter]::ToString(
            $sha256.ComputeHash($bytes))).Replace('-', '')
    }
    finally {
        $sha256.Dispose()
    }
}

function Get-LmcDistributionPolicyFileText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [string]$Blocker
    )

    Assert-LmcDistributionSemanticPolicy `
        -Condition (Test-Path -LiteralPath $Path -PathType Leaf) `
        -Blocker $Blocker `
        -Message ('Required file is missing: {0}' -f $Path)
    return [System.IO.File]::ReadAllText($Path)
}

function Get-LmcDistributionPolicyDocumentText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [string]$PythonPath,

        [scriptblock]$DocumentTextProvider
    )

    Assert-LmcDistributionSemanticPolicy `
        -Condition (Test-Path -LiteralPath $Path -PathType Leaf) `
        -Blocker 'DOCUMENT_TEXT_EXTRACTION' `
        -Message ('Required document is missing: {0}' -f $Path)

    if ($null -ne $DocumentTextProvider) {
        try {
            $providerResult = & $DocumentTextProvider $Path
            $text = (($providerResult | ForEach-Object { [string]$_ }) -join "`n")
        }
        catch {
            throw (New-LmcDistributionSemanticPolicyException `
                -Blocker 'DOCUMENT_TEXT_EXTRACTION' `
                -Message ('DocumentTextProvider failed for {0}: {1}' -f $Path, $_.Exception.Message))
        }
    }
    else {
        Assert-LmcDistributionSemanticPolicy `
            -Condition (-not [string]::IsNullOrWhiteSpace($PythonPath)) `
            -Blocker 'DOCUMENT_TEXT_EXTRACTION' `
            -Message 'PythonPath is required when DocumentTextProvider is not supplied.'

        $pythonCode = @'
import base64
import pathlib
import sys

path = pathlib.Path(sys.argv[1])
suffix = path.suffix.lower()
parts = []
if suffix == '.docx':
    from docx import Document
    document = Document(str(path))
    parts.extend(paragraph.text for paragraph in document.paragraphs)
    for table in document.tables:
        for row in table.rows:
            for cell in row.cells:
                parts.append(cell.text)
elif suffix == '.pdf':
    from pypdf import PdfReader
    reader = PdfReader(str(path))
    parts.extend((page.extract_text() or '') for page in reader.pages)
else:
    raise ValueError('Unsupported document extension: ' + suffix)

text = '\n'.join(parts)
sys.stdout.write(base64.b64encode(text.encode('utf-8')).decode('ascii'))
'@
        try {
            $encodedOutput = & $PythonPath -c $pythonCode $Path 2>&1
            $exitCode = $LASTEXITCODE
        }
        catch {
            throw (New-LmcDistributionSemanticPolicyException `
                -Blocker 'DOCUMENT_TEXT_EXTRACTION' `
                -Message ('Python document extraction could not start for {0}: {1}' -f $Path, $_.Exception.Message))
        }
        Assert-LmcDistributionSemanticPolicy `
            -Condition ($exitCode -eq 0) `
            -Blocker 'DOCUMENT_TEXT_EXTRACTION' `
            -Message ('Python document extraction failed for {0}: {1}' -f $Path, (($encodedOutput | ForEach-Object { [string]$_ }) -join ' '))
        try {
            $encodedText = (($encodedOutput | ForEach-Object { [string]$_ }) -join '').Trim()
            $bytes = [System.Convert]::FromBase64String($encodedText)
            $text = [System.Text.Encoding]::UTF8.GetString($bytes)
        }
        catch {
            throw (New-LmcDistributionSemanticPolicyException `
                -Blocker 'DOCUMENT_TEXT_EXTRACTION' `
                -Message ('Python returned invalid document text for {0}: {1}' -f $Path, $_.Exception.Message))
        }
    }

    Assert-LmcDistributionSemanticPolicy `
        -Condition (-not [string]::IsNullOrWhiteSpace($text)) `
        -Blocker 'DOCUMENT_TEXT_EXTRACTION' `
        -Message ('Extracted document text is empty: {0}' -f $Path)
    return $text
}

function Test-LmcDistributionPolicyPatterns {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Text,

        [Parameter(Mandatory = $true)]
        [string[]]$Patterns
    )

    foreach ($pattern in $Patterns) {
        if (-not [System.Text.RegularExpressions.Regex]::IsMatch(
            $Text,
            $pattern,
            [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor
                [System.Text.RegularExpressions.RegexOptions]::Singleline)) {
            return $false
        }
    }
    return $true
}

function Get-LmcDistributionPolicyMissingPatterns {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Text,

        [Parameter(Mandatory = $true)]
        [string[]]$Patterns
    )

    $missing = New-Object 'System.Collections.Generic.List[string]'
    foreach ($pattern in $Patterns) {
        if (-not [System.Text.RegularExpressions.Regex]::IsMatch(
            $Text,
            $pattern,
            [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor
                [System.Text.RegularExpressions.RegexOptions]::Singleline)) {
            [void]$missing.Add($pattern)
        }
    }
    return $missing.ToArray()
}

function Get-LmcDistributionPolicyProjectItems {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ProjectPath,

        [Parameter(Mandatory = $true)]
        [string]$Blocker
    )

    $projectText = Get-LmcDistributionPolicyFileText `
        -Path $ProjectPath `
        -Blocker $Blocker
    try {
        $projectXml = New-Object System.Xml.XmlDocument
        $projectXml.PreserveWhitespace = $true
        $projectXml.LoadXml($projectText)
    }
    catch {
        throw (New-LmcDistributionSemanticPolicyException `
            -Blocker $Blocker `
            -Message ('Invalid project XML in {0}: {1}' -f $ProjectPath, $_.Exception.Message))
    }

    $items = New-Object 'System.Collections.Generic.List[string]'
    $nodes = $projectXml.SelectNodes(
        "//*[local-name()='ApplicationDefinition' or local-name()='Page' or local-name()='Compile']")
    foreach ($node in $nodes) {
        if ($null -ne $node.Attributes['Include']) {
            $include = $node.Attributes['Include'].Value.Replace('/', '\')
            [void]$items.Add(('{0}|{1}' -f $node.LocalName, $include))
        }
    }
    $items.Sort([System.StringComparer]::OrdinalIgnoreCase)
    return $items.ToArray()
}

function Get-LmcDistributionPolicyProjectItemPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ProjectPath,

        [Parameter(Mandatory = $true)]
        [string]$ProjectItem
    )

    $separatorIndex = $ProjectItem.IndexOf('|')
    $include = $ProjectItem.Substring($separatorIndex + 1)
    return [System.IO.Path]::GetFullPath(
        (Join-Path (Split-Path -Parent $ProjectPath) $include))
}

function Get-LmcDistributionPolicyFileSha256 {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    try {
        $stream = [System.IO.File]::OpenRead($Path)
        try {
            return ([System.BitConverter]::ToString(
                $sha256.ComputeHash($stream))).Replace('-', '')
        }
        finally {
            $stream.Dispose()
        }
    }
    finally {
        $sha256.Dispose()
    }
}

function Test-LmcDistributionSemanticPolicy {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryRoot,

        [Parameter(Mandatory = $true)]
        [string]$CandidateRoot,

        [Parameter(Mandatory = $true)]
        [string]$PythonPath,

        [scriptblock]$DocumentTextProvider
    )

    $repositoryPath = [System.IO.Path]::GetFullPath($RepositoryRoot)
    $candidatePath = [System.IO.Path]::GetFullPath($CandidateRoot)
    Assert-LmcDistributionSemanticPolicy `
        -Condition (Test-Path -LiteralPath $repositoryPath -PathType Container) `
        -Blocker 'POLICY_INPUT' `
        -Message ('RepositoryRoot does not exist: {0}' -f $repositoryPath)
    Assert-LmcDistributionSemanticPolicy `
        -Condition (Test-Path -LiteralPath $candidatePath -PathType Container) `
        -Blocker 'POLICY_INPUT' `
        -Message ('CandidateRoot does not exist: {0}' -f $candidatePath)

    $policySha256 = Get-LmcDistributionSemanticPolicySha256
    $checkCount = 0

    $sdkModelsPath = Join-Path $repositoryPath 'LMC_Library\LMC_API_Delivery\src\LmcDiagnosticsD5Models.cs'
    $sdkDiagnosticsPath = Join-Path $repositoryPath 'LMC_Library\LMC_API_Delivery\src\LmcDiagnosticsD5.cs'
    $lasalServicePath = Join-Path $repositoryPath 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCDiagnosticsService\LMCDiagnosticsService.st'
    $lasalDispatcherPath = Join-Path $repositoryPath 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\TCPMotionInterface\TCPMotionInterface.st'
    $dintMapPath = Join-Path $repositoryPath 'LMC_Library\LMC_API_Delivery\docs\DINT_PACKET_MAP.txt'
    $currentWpfRoot = Join-Path $repositoryPath 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp'
    $currentWpfProjectPath = Join-Path $currentWpfRoot 'LasalApiWpfTestApp.csproj'
    $candidateWpfRoot = Join-Path $candidatePath '02_Example_Program\LasalApiWpfTestApp'
    $candidateWpfProjectPath = Join-Path $candidateWpfRoot 'LasalApiWpfTestApp.csproj'
    $candidateRootReadmePath = Join-Path $candidatePath 'README.md'
    $candidateExampleReadmePath = Join-Path $candidatePath '02_Example_Program\README.md'
    $candidateDocxPath = Join-Path $candidatePath '03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.docx'
    $candidatePdfPath = Join-Path $candidatePath '03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf'

    $currentMainWindowPath = Join-Path $currentWpfRoot 'MainWindow.xaml.cs'
    $currentQualificationMainPath = Join-Path $currentWpfRoot 'MainWindow.Qualification.cs'

    $sdkModels = Get-LmcDistributionPolicyFileText -Path $sdkModelsPath -Blocker 'SDK_SOURCE'
    $sdkDiagnostics = Get-LmcDistributionPolicyFileText -Path $sdkDiagnosticsPath -Blocker 'SDK_SOURCE'
    $lasalService = Get-LmcDistributionPolicyFileText -Path $lasalServicePath -Blocker 'LASAL_SOURCE'
    $lasalDispatcher = Get-LmcDistributionPolicyFileText -Path $lasalDispatcherPath -Blocker 'LASAL_SOURCE'
    $dintMap = Get-LmcDistributionPolicyFileText -Path $dintMapPath -Blocker 'DINT_MAP'
    $candidateRootReadme = Get-LmcDistributionPolicyFileText -Path $candidateRootReadmePath -Blocker 'CANDIDATE_DOCUMENTATION'
    $candidateExampleReadme = Get-LmcDistributionPolicyFileText -Path $candidateExampleReadmePath -Blocker 'CANDIDATE_DOCUMENTATION'
    $currentMainWindow = Get-LmcDistributionPolicyFileText -Path $currentMainWindowPath -Blocker 'CURRENT_WPF_SEMANTICS'
    $currentQualificationMain = Get-LmcDistributionPolicyFileText -Path $currentQualificationMainPath -Blocker 'CURRENT_WPF_SEMANTICS'
    $docxText = Get-LmcDistributionPolicyDocumentText -Path $candidateDocxPath -PythonPath $PythonPath -DocumentTextProvider $DocumentTextProvider
    $pdfText = Get-LmcDistributionPolicyDocumentText -Path $candidatePdfPath -PythonPath $PythonPath -DocumentTextProvider $DocumentTextProvider

    $manualSdoPatterns = @(
        'sdo\s+write',
        '0x2f00',
        'ui\s*\[\s*24\s*\]|ui24',
        'int32',
        '4[- ]?byte|4\s*\uBC14\uC774\uD2B8|datalength\s*=?\s*4',
        'axis\s*1|axis1|\uCD95\s*1',
        'identity[- ]pinned|identity.{0,24}(pin|\uACE0\uC815)',
        'four[- ]ticket|4[- ]ticket|4\s*(\uAC1C|\uAC1C\uC758)?.{0,12}ticket',
        'same[- ]value',
        'only.{0,80}(target|axis\s*1)|axis\s*1.{0,80}only|\uC720\uC77C',
        'axis\s*2.{0,60}(through|to|\.\.|-|~)?\s*4.{0,100}(blocked|off|disabled)|\uCD95\s*2.{0,60}4.{0,100}(\uCC28\uB2E8|\uBE44\uC2B9\uC778)',
        'current[- ]session|current.{0,30}session|\uD604\uC7AC.{0,30}session',
        'diagnosticsbuild',
        'bootid',
        'maprevision',
        'exact.{0,40}target|target.{0,40}exact'
    )
    $docxMissingSdoPatterns = @(Get-LmcDistributionPolicyMissingPatterns `
        -Text $docxText `
        -Patterns $manualSdoPatterns)
    $pdfMissingSdoPatterns = @(Get-LmcDistributionPolicyMissingPatterns `
        -Text $pdfText `
        -Patterns $manualSdoPatterns)
    $manualConflictingSdoPattern =
        'axis\s*(2|3|4).{0,100}(?<!not\s)(enabled|allowed|approved|supported)|' +
        'arbitrary.{0,60}sdo.{0,40}(enabled|allowed|approved|supported)|' +
        'additional.{0,60}(sdo\s+write\s+)?target.{0,40}(enabled|allowed|approved|supported)'
    $docxHasConflictingSdoScope = [regex]::IsMatch(
        $docxText,
        $manualConflictingSdoPattern,
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor
            [System.Text.RegularExpressions.RegexOptions]::Singleline)
    $pdfHasConflictingSdoScope = [regex]::IsMatch(
        $pdfText,
        $manualConflictingSdoPattern,
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor
            [System.Text.RegularExpressions.RegexOptions]::Singleline)
    Assert-LmcDistributionSemanticPolicy `
        -Condition (($docxMissingSdoPatterns.Count -eq 0) -and
            ($pdfMissingSdoPatterns.Count -eq 0) -and
            (-not $docxHasConflictingSdoScope) -and
            (-not $pdfHasConflictingSdoScope)) `
        -Blocker 'MANUAL_SDO_WRITE_SCOPE' `
        -Message ('Both candidate DOCX and PDF must describe Axis 1 exact 0x2F00:24 Int32/4 SDO Write and its identity-pinned four-ticket same-value gate. DOCX missing: {0}; PDF missing: {1}' -f
            ($docxMissingSdoPatterns -join ', '),
            ($pdfMissingSdoPatterns -join ', '))
    $checkCount++

    $previewPatterns = @(
        'preview',
        'production',
        'not.{0,30}(approved|ready)|no[- ]go|\uC2B9\uC778.{0,20}(\uC544\uB2C8|\uB418\uC9C0)|\uC544\uC9C1.{0,30}(\uC544\uB2C8|\uBABB)'
    )
    Assert-LmcDistributionSemanticPolicy `
        -Condition ((Test-LmcDistributionPolicyPatterns -Text $candidateRootReadme -Patterns $previewPatterns) -and
            (Test-LmcDistributionPolicyPatterns -Text $candidateExampleReadme -Patterns $previewPatterns) -and
            (Test-LmcDistributionPolicyPatterns -Text $docxText -Patterns $previewPatterns) -and
            (Test-LmcDistributionPolicyPatterns -Text $pdfText -Patterns $previewPatterns)) `
        -Blocker 'PREVIEW_PRODUCTION_NO_GO' `
        -Message 'Candidate READMEs, DOCX, and PDF must all state that preview is not production approved.'
    $checkCount++

    $ackPatterns = @(
        '\back\b',
        'not.{0,30}complet|\uC644\uB8CC.{0,20}(\uC544\uB2C8|\uC544\uB2D8)|\uC218\uB77D.{0,40}\uC644\uB8CC.{0,20}(\uC544\uB2C8|\uC544\uB2D8)'
    )
    Assert-LmcDistributionSemanticPolicy `
        -Condition ((Test-LmcDistributionPolicyPatterns -Text $dintMap -Patterns $ackPatterns) -and
            (Test-LmcDistributionPolicyPatterns -Text $docxText -Patterns $ackPatterns) -and
            (Test-LmcDistributionPolicyPatterns -Text $pdfText -Patterns $ackPatterns) -and
            (Test-LmcDistributionPolicyPatterns -Text $currentMainWindow -Patterns @(
                'PowerOnAndWaitForStableStateAsync',
                'WaitForStandstillAsync',
                'ReadStatusResultAsync'))) `
        -Blocker 'ACK_NOT_COMPLETION' `
        -Message 'The DINT map and both manuals must say that ACK is acceptance, not completion.'
    $checkCount++

    $closeCancelPatterns = @(
        'stop',
        'close.{0,240}(does not|do not|not.{0,30}stop|stop.{0,30}(\uC544\uB2C8|\uC544\uB2D8|\uBCF4\uB0B4\uC9C0))',
        'cancel.{0,240}(does not|do not|not.{0,30}stop|stop.{0,30}(\uC544\uB2C8|\uC544\uB2D8|\uBCF4\uB0B4\uC9C0))'
    )
    Assert-LmcDistributionSemanticPolicy `
        -Condition ((Test-LmcDistributionPolicyPatterns -Text $docxText -Patterns $closeCancelPatterns) -and
            (Test-LmcDistributionPolicyPatterns -Text $pdfText -Patterns $closeCancelPatterns) -and
            (Test-LmcDistributionPolicyPatterns -Text ($currentMainWindow + "`n" + $currentQualificationMain) -Patterns @(
                'Cancel Runner \(not PLC Stop\)',
                'No Stop command is sent automatically',
                'CloseCurrentConnectionAsync'))) `
        -Blocker 'CLOSE_CANCEL_NOT_STOP' `
        -Message 'Both manuals must state that Close and Cancel are not PLC motion Stop.'
    $checkCount++

    $unitPatterns = @(
        '\bunit\b',
        'caller|\uD638\uCD9C\uC790|\uC0AC\uC6A9\uC790|application',
        'convert|conversion|\uBCC0\uD658|\uACF1|\uB098\uB204'
    )
    Assert-LmcDistributionSemanticPolicy `
        -Condition ((Test-LmcDistributionPolicyPatterns -Text $dintMap -Patterns $unitPatterns) -and
            (Test-LmcDistributionPolicyPatterns -Text $docxText -Patterns $unitPatterns) -and
            (Test-LmcDistributionPolicyPatterns -Text $pdfText -Patterns $unitPatterns) -and
            (Test-LmcDistributionPolicyPatterns -Text $currentMainWindow -Patterns @(
                'ToLasalDint',
                'engineeringValue\s*\*\s*unitMultiplier'))) `
        -Blocker 'UNIT_CALLER_CONVERSION' `
        -Message 'The DINT map and both manuals must assign UNIT conversion to caller code.'
    $checkCount++

    $livePatterns = @(
        '\bplc\b',
        'live|runtime|hardware|\uC2E4\uBB3C',
        'not.{0,40}(proven|verified)|unverified|\uBBF8\uAC80\uC99D|\uAC80\uC99D.{0,30}(\uC54A|\uC548|\uB418\uC9C0|\uBABB)|proof.{0,20}(\uC5C6|not)'
    )
    Assert-LmcDistributionSemanticPolicy `
        -Condition ((Test-LmcDistributionPolicyPatterns -Text $dintMap -Patterns $livePatterns) -and
            (Test-LmcDistributionPolicyPatterns -Text $docxText -Patterns $livePatterns) -and
            (Test-LmcDistributionPolicyPatterns -Text $pdfText -Patterns $livePatterns)) `
        -Blocker 'PLC_LIVE_UNVERIFIED' `
        -Message 'The DINT map and both manuals must keep current PLC live proof explicitly unverified.'
    $checkCount++

    $sdkSdoPatterns = @(
        'SdoWriteEnabled\s*=\s*true',
        'SdoWriteUi24Axis1Enabled\s*=\s*true',
        'SdoWriteUi24Axis2Enabled\s*=\s*false',
        'SdoWriteUi24Axis3Enabled\s*=\s*false',
        'SdoWriteUi24Axis4Enabled\s*=\s*false',
        'new\s+LMCSdoWriteTarget\s*\(.{0,300}(?:1|slaveReference)\s*,.{0,80}0x2F00\s*,.{0,80}24\s*,.{0,80}LMCSignalValueType\.Int32\s*,.{0,80}4\s*,'
    )
    $lasalSdoPatterns = @(
        'LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED\s+TRUE',
        'LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED\s+TRUE',
        'LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED\s+FALSE',
        'LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED\s+FALSE',
        'LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED\s+FALSE',
        'ObjectIndex\s*<>\s*0x2F00.{0,80}SubIndex\s*<>\s*24.{0,80}ValueType\s*<>\s*4.{0,80}DataLength\s*<>\s*4'
    )
    $dintSdoPatterns = @(
        'Axis\s*1\s+exact\s+0x2F00:24\s+Int32/4',
        'Axis\s*2\.\.4\s+remain\s+blocked',
        'identity-pinned',
        'four\s+distinct\s+tickets|four-ticket'
    )
    $dintHasRetiredGateOffText = [regex]::IsMatch(
        $dintMap,
        'not approved yet|all four.{0,100}SDO_WRITE.{0,100}gates are FALSE|SdoWriteEnabled.{0,160}allowlist.{0,40}(closed|empty)',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor
            [System.Text.RegularExpressions.RegexOptions]::Singleline)
    $sdkApprovedTargetCount = [regex]::Matches(
        $sdkModels,
        'new\s+LMCSdoWriteTarget\s*\(',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase).Count
    Assert-LmcDistributionSemanticPolicy `
        -Condition ((Test-LmcDistributionPolicyPatterns -Text $sdkModels -Patterns $sdkSdoPatterns) -and
            ($sdkApprovedTargetCount -eq 1) -and
            (Test-LmcDistributionPolicyPatterns -Text $lasalService -Patterns $lasalSdoPatterns) -and
            (Test-LmcDistributionPolicyPatterns -Text $dintMap -Patterns $dintSdoPatterns) -and
            (-not $dintHasRetiredGateOffText)) `
        -Blocker 'SDO_WRITE_SCOPE' `
        -Message 'SDK, LASAL, and DINT map must agree on only Axis 1 exact 0x2F00:24 Int32/4 enabled.'
    $checkCount++

    $identityPatterns = @(
        'SubmitSdoWriteIdentityPinnedAsync',
        'freshCapabilities\s*=\s*GetCapabilities\s*\(',
        'ValidateRequiredSdoWriteSubmissionIdentity'
    )
    Assert-LmcDistributionSemanticPolicy `
        -Condition (Test-LmcDistributionPolicyPatterns -Text $sdkDiagnostics -Patterns $identityPatterns) `
        -Blocker 'SDO_WRITE_IDENTITY_PIN' `
        -Message 'SDK identity-pinned submit must refresh and validate capabilities inside the mutation path.'
    $checkCount++

    $currentMarkerPath = Join-Path $currentWpfRoot 'MainWindow.xaml.cs'
    $currentDiagnosticsPath = Join-Path $currentWpfRoot 'MainWindow.Diagnostics.cs'
    $currentQualificationPath = Join-Path $currentWpfRoot 'MainWindow.Qualification.SdoWrite.cs'
    $currentProofPath = Join-Path $currentWpfRoot 'SdoWriteActivationQualificationProof.cs'
    $currentWpfText = (Get-LmcDistributionPolicyFileText -Path $currentMarkerPath -Blocker 'CURRENT_WPF_IDENTITY_PIN') + "`n" +
        (Get-LmcDistributionPolicyFileText -Path $currentDiagnosticsPath -Blocker 'CURRENT_WPF_IDENTITY_PIN') + "`n" +
        (Get-LmcDistributionPolicyFileText -Path $currentQualificationPath -Blocker 'CURRENT_WPF_IDENTITY_PIN') + "`n" +
        (Get-LmcDistributionPolicyFileText -Path $currentProofPath -Blocker 'CURRENT_WPF_IDENTITY_PIN')
    $wpfIdentityPatterns = @(
        'CREVIS_TOPOLOGY_AXIS1_UI24_SDO_WRITE_LIVE_AXIS_QUAL_V5',
        'SubmitSdoWriteIdentityPinnedAsync',
        'four-ticket',
        'SdoWriteActivationQualificationProof'
    )
    Assert-LmcDistributionSemanticPolicy `
        -Condition (Test-LmcDistributionPolicyPatterns -Text $currentWpfText -Patterns $wpfIdentityPatterns) `
        -Blocker 'CURRENT_WPF_IDENTITY_PIN' `
        -Message 'Current WPF must retain its V4 marker, identity-pinned submit, four-ticket gate, and proof type.'
    $checkCount++

    $capabilityMatch = [regex]::Match(
        $lasalService,
        'FUNCTION\s+LMCDiagnosticsService::HandleDiagnosticsCapabilities(?<body>.*?)END_FUNCTION',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor
            [System.Text.RegularExpressions.RegexOptions]::Singleline)
    if (-not $capabilityMatch.Success) {
        $capabilityMatch = [regex]::Match(
            $lasalService,
            'if\s+CommandId\s*=\s*0x7E00\s+then(?<body>.*?)ResponseSize\s*:=\s*68',
            [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor
                [System.Text.RegularExpressions.RegexOptions]::Singleline)
    }
    $capabilityBody = $capabilityMatch.Groups['body'].Value
    $dormantPatterns = @(
        'bits?\s+15.{0,30}(remain\s+zero|zero)',
        'bits?\s+(15.{0,20}and\s+)?16.{0,40}(remain\s+zero|zero)',
        'bit\s+17.{0,30}(remain\s+zero|zero)'
    )
    Assert-LmcDistributionSemanticPolicy `
        -Condition ($capabilityMatch.Success -and
            ($capabilityBody -match '0x0000613F') -and
            ($capabilityBody -match '0x00000200') -and
            ($capabilityBody -notmatch '0x00008000|0x00010000|0x00020000') -and
            (Test-LmcDistributionPolicyPatterns -Text $dintMap -Patterns $dormantPatterns)) `
        -Blocker 'TOPOLOGY_DORMANT_BITS' `
        -Message 'LASAL capabilities and DINT map must keep bits 15, 16, and 17 zero.'
    $checkCount++

    Assert-LmcDistributionSemanticPolicy `
        -Condition (($lasalDispatcher -notmatch '(?i)0x7E23|16#7E23') -and
            [regex]::IsMatch($dintMap, '0x7E23.{0,80}(absent|no\s+PLC\s+route|without\s+a\s+LASAL\s+route)', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor [System.Text.RegularExpressions.RegexOptions]::Singleline)) `
        -Blocker 'TOPOLOGY_7E23_ABSENT' `
        -Message '0x7E23 must remain absent from the PLC dispatcher and documented unsupported.'
    $checkCount++

    $piPatterns = @(
        'AllowedPIWriteSignalIds\s*=\s*new\s+uint\s*\[\s*0\s*\]',
        'PI Write.{0,80}(remain|is).{0,20}(off|disabled)'
    )
    $lasalPiDisabled = [regex]::IsMatch(
        $lasalService,
        '0x7E21\s*:.{0,240}detailCode\s*:=\s*2',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor
            [System.Text.RegularExpressions.RegexOptions]::Singleline)
    Assert-LmcDistributionSemanticPolicy `
        -Condition ((Test-LmcDistributionPolicyPatterns -Text ($sdkModels + "`n" + $dintMap) -Patterns $piPatterns) -and $lasalPiDisabled) `
        -Blocker 'PI_WRITE_DISABLED' `
        -Message 'SDK allowlist, LASAL handler, and DINT map must keep PI Write disabled.'
    $checkCount++

    Assert-LmcDistributionSemanticPolicy `
        -Condition (($capabilityBody -notmatch '0x00000040') -and
            [regex]::IsMatch($dintMap, 'D4\s+Double.{0,160}(remain|is).{0,20}(off|disabled)', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor [System.Text.RegularExpressions.RegexOptions]::Singleline) -and
            [regex]::IsMatch($dintMap, 'Double-bank\s+bit\s+6\s+remains\s+zero', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)) `
        -Blocker 'D4_DOUBLE_DISABLED' `
        -Message 'LASAL capability bit 6 and the DINT map must keep D4 Double disabled.'
    $checkCount++

    $currentItems = @(Get-LmcDistributionPolicyProjectItems -ProjectPath $currentWpfProjectPath -Blocker 'CANDIDATE_WPF_SOURCE_SET')
    $candidateItems = @(Get-LmcDistributionPolicyProjectItems -ProjectPath $candidateWpfProjectPath -Blocker 'CANDIDATE_WPF_SOURCE_SET')
    Assert-LmcDistributionSemanticPolicy `
        -Condition (($currentItems -join "`n") -ceq ($candidateItems -join "`n")) `
        -Blocker 'CANDIDATE_WPF_SOURCE_SET' `
        -Message 'Candidate WPF ApplicationDefinition/Page/Compile source set differs from current WPF.'
    $checkCount++

    foreach ($item in $currentItems) {
        $currentItemPath = Get-LmcDistributionPolicyProjectItemPath -ProjectPath $currentWpfProjectPath -ProjectItem $item
        $candidateItemPath = Get-LmcDistributionPolicyProjectItemPath -ProjectPath $candidateWpfProjectPath -ProjectItem $item
        Assert-LmcDistributionSemanticPolicy `
            -Condition ((Test-Path -LiteralPath $currentItemPath -PathType Leaf) -and
                (Test-Path -LiteralPath $candidateItemPath -PathType Leaf)) `
            -Blocker 'CANDIDATE_WPF_SOURCE_CONTENT' `
            -Message ('A current or candidate WPF source item is missing: {0}' -f $item)
        Assert-LmcDistributionSemanticPolicy `
            -Condition ((Get-LmcDistributionPolicyFileSha256 -Path $currentItemPath) -ceq
                (Get-LmcDistributionPolicyFileSha256 -Path $candidateItemPath)) `
            -Blocker 'CANDIDATE_WPF_SOURCE_CONTENT' `
            -Message ('Candidate WPF source item is not byte-identical to current: {0}' -f $item)
    }
    $checkCount++

    return [pscustomobject]@{
        PolicySha256 = $policySha256
        Result = 'PASS'
        CheckCount = $checkCount
    }
}
