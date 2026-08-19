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
            Name = 'MANUAL_RECONNECT_SCOPE'
            Statement = 'Both external manuals describe the bounded RPC_INIT_FRESH_TCP_ONCE_V2 two-cause candidate-1 policy, its no-retry exclusions and evidence fields, the actual-EXE X-close/process/mutex/wire gate, the current build/download versus unverified same-window PLC-runtime boundary, and standalone-versus-full-Distribution result.'
        },
        [pscustomobject]@{
            Name = 'MANUAL_RELEASE_WARNING_SCOPE'
            Statement = 'Both external manuals retain the unfinished motion/diagnostics matrices, explicit safe-stop and machine-safety prerequisites, and unsigned strong-name/AuthentiCode warnings.'
        },
        [pscustomobject]@{
            Name = 'MANUAL_VERSION_SCOPE'
            Statement = 'Both external manuals identify the current document revision as 2.3-candidate.'
        },
        [pscustomobject]@{
            Name = 'PI_WRITE_DISABLED'
            Statement = 'PI Write remains disabled and the SDK PI Write allowlist remains empty.'
        },
        [pscustomobject]@{
            Name = 'PLC_LIVE_UNVERIFIED'
            Statement = 'The current reconnect PLC image build/download is recorded, but same-window live reconnect and live SDO Write remain unverified and must not be inferred from image transfer, static checks, or PC fake tests.'
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
            $encodedOutput = & $PythonPath -B -c $pythonCode $Path 2>&1
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

function Get-LmcDistributionPolicyMatchingPatterns {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Text,

        [Parameter(Mandatory = $true)]
        [string[]]$Patterns
    )

    $matching = New-Object 'System.Collections.Generic.List[string]'
    foreach ($pattern in $Patterns) {
        if ([System.Text.RegularExpressions.Regex]::IsMatch(
            $Text,
            $pattern,
            [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor
                [System.Text.RegularExpressions.RegexOptions]::Singleline)) {
            [void]$matching.Add($pattern)
        }
    }
    return $matching.ToArray()
}

function Test-LmcDistributionManualReleasePolicy {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$DocxText,

        [Parameter(Mandatory = $true)]
        [string]$PdfText
    )

    $checkCount = 0

    $manualVersionPatterns = @(
        '(Manual\s+revision|Document\s+revision|\uBB38\uC11C\s+\uBC84\uC804)\s*[:\uFF1A]?\s*2\.3-candidate(?![0-9])'
    )
    $docxMissingVersionPatterns = @(
        Get-LmcDistributionPolicyMissingPatterns `
            -Text $DocxText `
            -Patterns $manualVersionPatterns)
    $pdfMissingVersionPatterns = @(
        Get-LmcDistributionPolicyMissingPatterns `
            -Text $PdfText `
            -Patterns $manualVersionPatterns)
    Assert-LmcDistributionSemanticPolicy `
        -Condition (($docxMissingVersionPatterns.Count -eq 0) -and
            ($pdfMissingVersionPatterns.Count -eq 0)) `
        -Blocker 'MANUAL_VERSION_SCOPE' `
        -Message ('Both candidate DOCX and PDF must identify document revision 2.3-candidate. DOCX missing: {0}; PDF missing: {1}' -f
            ($docxMissingVersionPatterns -join ', '),
            ($pdfMissingVersionPatterns -join ', '))
    $checkCount++

    $manualReconnectPatterns = @(
        'RPC_INIT_FRESH_TCP_ONCE_V2',
        '(exact\s+(persistent\s+)?canonical.{0,80}(?:ErrorId\s*=?\s*)?-1.{0,120}(AttemptCount\s*=?\s*2|two\s+same[- ]socket)|exact\s+canonical.{0,120}AttemptCount\s*=?\s*2)',
        '(AttemptCount\s*=?\s*2.{0,160}100\s*ms|100\s*ms.{0,160}AttemptCount\s*=?\s*2)',
        '(actual\s+0x8080\s+request.{0,80}(started|start)|\uC2E4\uC81C\s*`?0x8080`?\s*request.{0,80}\uC2DC\uC791)',
        'AttemptCount\s*=?\s*1',
        '(no\s+(received\s+)?response|response.{0,30}(none|\uC5C6)|\uC751\uB2F5.{0,30}\uC5C6)',
        'EndOfStreamException',
        'SocketException',
        'TimeoutException',
        'IOException',
        '((direct|the\s+exceptions?\s+themselves|\uC9C1\uC811|\uC790\uCCB4).{0,80}EndOfStreamException.{0,100}SocketException.{0,100}TimeoutException)',
        '(IOException.{0,120}inner.{0,120}(one\s+of\s+those|EndOfStreamException|SocketException|TimeoutException)|(EndOfStreamException|SocketException|TimeoutException).{0,160}inner.{0,80}IOException)',
        '(1000\s*ms|1\s*second)',
        '((Cause\s*B|\(\s*B\s*\)|pre-response\s+transport).{0,500}(actual\s+0x8080\s+request|\uC2E4\uC81C\s*`?0x8080`?\s*request).{0,180}(started|start|\uC2DC\uC791).{0,220}AttemptCount\s*=?\s*1.{0,220}(no\s+(received\s+)?response|response.{0,30}(none|\uC5C6)|\uC751\uB2F5.{0,30}\uC5C6).{0,220}(direct|the\s+exceptions?\s+themselves|\uC9C1\uC811|\uC790\uCCB4).{0,80}EndOfStreamException.{0,160}SocketException.{0,160}TimeoutException.{0,260}(IOException.{0,160}(inner|InnerException)|(inner|InnerException).{0,160}IOException).{0,300}(1000\s*ms|1\s*second))',
        '(candidate\s*2|second.{0,30}candidate|\uB450\s*\uBC88\uC9F8\s*candidate).{0,100}terminal',
        '(Connect-before-init|connect.{0,30}before.{0,30}init).{0,80}AttemptCount\s*=?\s*0.{0,80}(no[- ]?retry|does\s+not\s+retry|retry.{0,20}(none|\uC5C6)|\uC7AC\uC2DC\uB3C4.{0,30}\uC54A)',
        'cancellation.{0,240}(no[- ]?retry|does\s+not\s+retry|do\s+not\s+retry|retry.{0,20}(none|\uC5C6)|\uC7AC\uC2DC\uB3C4.{0,30}\uC54A)',
        'ObjectDisposedException.{0,240}(no[- ]?retry|does\s+not\s+retry|do\s+not\s+retry|retry.{0,20}(none|\uC5C6)|\uC7AC\uC2DC\uB3C4.{0,30}\uC54A)',
        'InvalidDataException.{0,200}(inner|InnerException).{0,240}(no[- ]?retry|does\s+not\s+retry|do\s+not\s+retry|retry.{0,20}(none|\uC5C6)|\uC7AC\uC2DC\uB3C4.{0,30}\uC54A)',
        'malformed.{0,240}(no[- ]?retry|does\s+not\s+retry|do\s+not\s+retry|retry.{0,20}(none|\uC5C6)|\uC7AC\uC2DC\uB3C4.{0,30}\uC54A)',
        '(valid\s+non-?`?-1`?|non-?`?-1`?\s+response).{0,240}(no[- ]?retry|does\s+not\s+retry|do\s+not\s+retry|retry.{0,20}(none|\uC5C6)|\uC7AC\uC2DC\uB3C4.{0,30}\uC54A)',
        '(response.{0,40}(after|\uC774\uD6C4)|after.{0,30}response).{0,240}(no[- ]?retry|does\s+not\s+retry|do\s+not\s+retry|retry.{0,20}(none|\uC5C6)|\uC7AC\uC2DC\uB3C4.{0,30}\uC54A)',
        'callback[- ]stage.{0,240}(no[- ]?retry|does\s+not\s+retry|do\s+not\s+retry|retry.{0,20}(none|\uC5C6)|\uC7AC\uC2DC\uB3C4.{0,30}\uC54A)',
        'CandidateOrdinal',
        'FreshSessionRetryReason',
        'FreshSessionRetryDelayMs',
        'FreshSessionRetryFromCandidate',
        'FreshSessionRetryNextCandidate',
        'FreshSessionFirstFailure',
        '((one\s+UI\s+operation|UI\s+operation\s+one).{0,100}(TCP\s*(2|two)|two\s+TCP).{0,100}0x8080.{0,40}(4|four)|UI\s+operation\s+\uD558\uB098.{0,80}\uC0C1\uD55C\uC740\s+TCP\s*2\s*\uAC1C.{0,80}0x8080\s*4\s*\uD68C)',
        '(15\s*:\s*58.{0,160}(build|\uBE4C\uB4DC).{0,80}(download|\uB2E4\uC6B4\uB85C\uB4DC)|(build|\uBE4C\uB4DC).{0,80}(download|\uB2E4\uC6B4\uB85C\uB4DC).{0,160}15\s*:\s*58)',
        '(same[- ]window|\uAC19\uC740\s+\uCC3D).{0,160}(live\s+reconnect|Close.{0,30}Connect).{0,120}(not.{0,30}(observed|verified|proven)|\uD655\uC778\uB418\uC9C0\s+\uC54A|\uBBF8\uD655\uC778)',
        '(PC.{0,40}(fake|loopback).{0,160}(not.{0,30}(PLC\s+runtime\s+proof|prove\s+PLC)|PLC\s+runtime.{0,30}\uC544\uB2C8)|PLC\s+runtime.{0,120}(not.{0,30}proven|\uC99D\uBA85.{0,20}\uC544\uB2C8))',
        '((current\s+PLC\s+)?live\s+SDO\s+Write.{0,120}(not.{0,30}(proven|verified)|unverified|remain.{0,30}unverified|\uBBF8\uAC80\uC99D)|SDO\s+Write.{0,50}(live|runtime|\uC2E4\uAE30).{0,100}(proof.{0,30}(not|\uC5C6|\uC544\uC9C1)|\uBBF8\uAC80\uC99D|\uAC80\uC99D.{0,30}(\uC54A|\uC548|\uBABB)))',
        'actual[- ]?EXE',
        'SC_CLOSE',
        'actual[- ]?EXE.{0,120}(\bX\b|window.{0,20}close|\uCC3D.{0,12}X|X.{0,12}(\uC885\uB8CC|\uB2EB\uAE30))',
        'process.{0,30}(exit|\uC885\uB8CC)',
        '(default.{0,40}(named\s+)?mutex.{0,120}(reacquir|\uC7AC\uD68D\uB4DD)|mutex.{0,120}(reacquir|\uC7AC\uD68D\uB4DD)|(reacquir|\uC7AC\uD68D\uB4DD).{0,120}default.{0,40}(named\s+)?mutex)',
        '3\s*/\s*28\s*\(\s*13\s*,\s*2\s*,\s*13\s*\)',
        '(PC[- ]?loopback[- ]?only|PC\s+loopback.{0,40}(only|local)|PC.{0,40}\uB8E8\uD504\uBC31)',
        '(PLC.{0,240}(cleanup|disarm|readiness).{0,160}(is\s+not\s+proven|are\s+not\s+proven|does\s+not\s+prove|\uC99D\uBA85\uD558\uC9C0\s+\uC54A\uB294\uB2E4|\uC99D\uAC70\uAC00\s+\uC544\uB2C8\uB2E4)|does\s+not\s+prove.{0,120}PLC.{0,120}(cleanup|disarm|readiness))',
        '(standalone|binary[- ]reference|\uBCC4\uB3C4.{0,80}(candidate|gate|\uD6C4\uBCF4)).{0,240}PASS',
        'full\s+Distribution.{0,600}(STOP|is\s+not\s+PASS|did\s+not.{0,40}PASS|PASS.{0,20}(\uAC00\s+\uC544\uB2C8|\uAC00\s+\uC544\uB2D8)|\uB3C4\uB2EC\uD558\uC9C0)'
    )
    $docxMissingReconnectPatterns = @(
        Get-LmcDistributionPolicyMissingPatterns `
            -Text $DocxText `
            -Patterns $manualReconnectPatterns)
    $pdfMissingReconnectPatterns = @(
        Get-LmcDistributionPolicyMissingPatterns `
            -Text $PdfText `
            -Patterns $manualReconnectPatterns)
    $manualReconnectConflictPatterns = @(
        'actual[- ]?EXE.{0,240}(?<!does not )(?<!do not )(?<!not )(?:prove[sd]?|verif(?:y|ies|ied)|demonstrat(?:e|es|ed)).{0,120}\bPLC\b',
        '\bPLC\b.{0,160}(cleanup|readiness).{0,120}(is|are|was|were).{0,30}(?<!not )(?<!un)(proven|verified).{0,160}actual[- ]?EXE',
        'actual[- ]?EXE.{0,240}\bPLC\b.{0,120}(\uC99D\uBA85\uD55C\uB2E4|\uC785\uC99D\uD55C\uB2E4|\uAC80\uC99D\uD55C\uB2E4)',
        '((V2.{0,40}(fake|loopback)|(fake|loopback).{0,40}V2).{0,180}(?<!does not )(?<!do not )(?<!not )(?:prove[sd]?|verif(?:y|ies|ied)|demonstrat(?:e|es|ed)).{0,160}\bPLC\b.{0,160}(same[- ]window|runtime|readiness|cleanup|reconnect))',
        '((1000\s*ms|1\s*second).{0,80}(backoff|delay|wait).{0,120}(?<!does not )(?<!do not )(?<!not )(?:prove[sd]?|verif(?:y|ies|ied)|demonstrat(?:e|es|ed)).{0,120}\bPLC\b.{0,120}(runtime|readiness|cleanup|reconnect))',
        '(15\s*:\s*58.{0,200}(build|\uBE4C\uB4DC).{0,100}(download|\uB2E4\uC6B4\uB85C\uB4DC).{0,160}(?<!does not )(?<!do not )(?<!not )(?:prove[sd]?|verif(?:y|ies|ied)|demonstrat(?:e|es|ed)).{0,160}(same[- ]window|live\s+reconnect|Close.{0,30}Connect))',
        '((Cause\s*A|persistent\s+same[- ]socket|AttemptCount\s*=?\s*2).{0,160}(exact\s+canonical.{0,80}(?:ErrorId\s*=?\s*)?-1|(?:ErrorId\s*=?\s*)?-1.{0,80}exact\s+canonical).{0,120}(not.{0,30}required|no\s+longer\s+required|optional|unnecessary|may\s+be\s+ignored|need\s+not)|(exact\s+canonical.{0,80}(?:ErrorId\s*=?\s*)?-1|(?:ErrorId\s*=?\s*)?-1.{0,80}exact\s+canonical).{0,120}(not.{0,30}required|no\s+longer\s+required|optional|unnecessary|may\s+be\s+ignored|need\s+not).{0,160}(Cause\s*A|persistent\s+same[- ]socket|AttemptCount\s*=?\s*2))',
        '(fresh[- ]?TCP.{0,140}(more\s+than\s+one|multiple|unbounded|any\s+number|two\s+or\s+more|2\s+or\s+more|\uBCF5\uC218|\uC5EC\uB7EC|\uB450\s*\uAC1C\s*\uC774\uC0C1|2\uD68C\s*\uC774\uC0C1)|(more\s+than\s+one|multiple|unbounded|any\s+number|two\s+or\s+more|\uBCF5\uC218|\uC5EC\uB7EC).{0,100}fresh[- ]?TCP)',
        '((same[- ]socket|same.{0,24}TCP|\uAC19\uC740.{0,24}(TCP|socket)|\uB3D9\uC77C.{0,24}(TCP|socket)).{0,140}(more\s+than\s+two|three\s+or\s+more|unbounded|any\s+number|\uC138\s*\uBC88\s*\uC774\uC0C1|\uBB34\uC81C\uD55C).{0,80}(attempt|retry|\uC2DC\uB3C4))',
        '(same[- ]socket|same.{0,24}TCP).{0,100}(two|2).{0,60}attempts?.{0,80}(not\s+a\s+limit|no\s+limit)',
        '(SC_CLOSE|process.{0,30}exit|default.{0,40}(named\s+)?mutex.{0,100}reacquir).{0,120}(not.{0,30}required|optional|unnecessary|may\s+be\s+skipped|need\s+not)',
        '(candidate\s*2|second.{0,30}candidate).{0,100}(\bmay\b|\bcan\b|\bwill\b|is\s+allowed\s+to).{0,40}(retry|open.{0,20}(another|third|fresh))',
        '(Connect-before-init|AttemptCount\s*=?\s*0).{0,100}(\bmay\b|\bcan\b|\bwill\b|is\s+allowed\s+to).{0,40}(retry|open.{0,20}fresh)',
        '(malformed|valid\s+non-?`?-1`?|callback[- ]stage|ObjectDisposedException|cancellation).{0,120}(\bmay\b|\bcan\b|\bwill\b|is\s+allowed\s+to).{0,40}(retry|open.{0,20}fresh)',
        '(failure\s+after\s+(a\s+)?response|after.{0,30}response|response.{0,30}(after|\uC774\uD6C4)).{0,120}(\bmay\b|\bcan\b|\bwill\b|is\s+allowed\s+to).{0,80}(retry|open.{0,20}fresh)',
        '((plain|generic|unclassified|any|all)\s+IOException|IOException.{0,80}(without|no|does\s+not\s+require).{0,50}inner).{0,160}(eligible|may|can|will|is\s+allowed\s+to).{0,80}(retry|fresh[- ]?TCP)',
        'InvalidDataException.{0,180}(inner|InnerException).{0,180}(\b(?:eligible|may|can|will)\b|is\s+allowed\s+to).{0,100}(retry|fresh[- ]?TCP)',
        '((current\s+PLC\s+)?live\s+SDO\s+Write|SDO\s+Write.{0,50}(live|runtime|\uC2E4\uAE30)).{0,100}((is|was|has\s+been)\s+(?!not\b)(?!un)(verified|proven|production[- ]?ready|available|supported)|\uAC80\uC99D\uB410|production[- ]?ready|\uC0AC\uC6A9\s*\uAC00\uB2A5)',
        'full\s+Distribution\s+(is|was|has\s+been|returned)\s+(a\s+)?PASS(?:ED)?\b',
        'full\s+Distribution.{0,240}PASS(\uC774\uB2E4|\uD588\uB2E4|\uC600\uB2E4|\uB85C\s+\uD655\uC778)'
    )
    $docxReconnectConflicts = @(
        Get-LmcDistributionPolicyMatchingPatterns `
            -Text $DocxText `
            -Patterns $manualReconnectConflictPatterns)
    $pdfReconnectConflicts = @(
        Get-LmcDistributionPolicyMatchingPatterns `
            -Text $PdfText `
            -Patterns $manualReconnectConflictPatterns)
    Assert-LmcDistributionSemanticPolicy `
        -Condition (($docxMissingReconnectPatterns.Count -eq 0) -and
            ($pdfMissingReconnectPatterns.Count -eq 0) -and
            ($docxReconnectConflicts.Count -eq 0) -and
            ($pdfReconnectConflicts.Count -eq 0)) `
        -Blocker 'MANUAL_RECONNECT_SCOPE' `
        -Message ('Both candidate DOCX and PDF must retain the bounded reconnect/actual-EXE contract without contradictory scope or PLC-proof claims. DOCX missing: {0}; PDF missing: {1}; DOCX conflicts: {2}; PDF conflicts: {3}' -f
            ($docxMissingReconnectPatterns -join ', '),
            ($pdfMissingReconnectPatterns -join ', '),
            ($docxReconnectConflicts -join ', '),
            ($pdfReconnectConflicts -join ', '))
    $checkCount++

    $manualReleaseWarningPatterns = @(
        '25[- ]?command.{0,50}(matrix|\uB9E4\uD2B8\uB9AD\uC2A4).{0,60}(remains\s+(unfinished|incomplete)|is\s+not\s+(complete|finished)|\uC544\uC9C1\s+\uC644\uB8CC\uB418\uC9C0\s+\uC54A\uC558\uB2E4|\uBBF8\uC644\uB8CC)',
        'D1\s*/\s*D2\s*/\s*D5.{0,180}fault\s*/?\s*soak.{0,180}D3\s*/\s*D4.{0,180}(remain(s)?\s+(unfinished|incomplete)|are\s+not\s+(complete|finished)|\uC644\uB8CC\uB85C\s+\uD655\uB300\s+\uD574\uC11D\uD558\uC9C0\s+\uC54A\uB294\uB2E4)',
        '(Close(Connection)?|Close).{0,120}Dispose.{0,120}(cancel(?:lation)?|\uCDE8\uC18C).{0,140}(do\s+not\s+send.{0,50}(motion\s+)?Stop|(motion\s+)?Stop.{0,40}\uBCF4\uB0B4\uC9C0\s+\uC54A\uB294\uB2E4)',
        '(use\s+an?\s+explicit\s+safe[- ]?stop|explicit\s+safe[- ]?stop.{0,80}(required|procedure|must)|explicit\s+safe[- ]?stop.{0,80}\uBCC4\uB3C4\s+\uC2B9\uC778)',
        '(Before\s+motion.{0,80}verify.{0,120}E[- ]?stop.{0,80}(hardware|HW).{0,30}(software|SW).{0,30}limits?.{0,80}UNIT.{0,80}Home|E[- ]?stop.{0,100}HW\s*/\s*SW\s+limit.{0,100}UNIT.{0,80}Home(?:/Reference)?.{0,120}\uBCC4\uB3C4\s+\uC2B9\uC778)',
        '(DLL\s+is\s+unsigned.{0,120}(neither|no).{0,80}strong[- ]?name.{0,80}AuthentiCode|DLL.{0,120}strong[- ]?name.{0,80}AuthentiCode.{0,80}\uC11C\uBA85\uC774\s+\uC5C6)'
    )
    $docxMissingReleaseWarningPatterns = @(
        Get-LmcDistributionPolicyMissingPatterns `
            -Text $DocxText `
            -Patterns $manualReleaseWarningPatterns)
    $pdfMissingReleaseWarningPatterns = @(
        Get-LmcDistributionPolicyMissingPatterns `
            -Text $PdfText `
            -Patterns $manualReleaseWarningPatterns)
    $manualReleaseWarningConflictPatterns = @(
        '25[- ]?command.{0,140}(matrix.{0,40})?(is|was|has\s+been|remains|no\s+longer\s+unfinished)?\s*\b(complete|finished|validated|qualified|fully\s+covered)\b',
        '25.{0,50}(command|\uBA85\uB839).{0,140}\uC644\uB8CC(\uB410\uB2E4|\uB418\uC5C8\uB2E4|\uC774\uB2E4|\uD568)',
        'D1\s*/\s*D2\s*/\s*D5.{0,140}fault\s*/?\s*soak.{0,180}\b(complete|finished|validated|qualified|fully\s+covered)\b',
        'D1\s*/\s*D2\s*/\s*D5.{0,140}fault\s*/?\s*soak.{0,120}\uC644\uB8CC(\uB410\uB2E4|\uB418\uC5C8\uB2E4|\uC774\uB2E4|\uD568)',
        'D3\s*/\s*D4.{0,140}runtime.{0,180}\b(complete|finished|validated|qualified|fully\s+covered)\b',
        'D3\s*/\s*D4.{0,140}runtime.{0,120}\uC644\uB8CC(\uB410\uB2E4|\uB418\uC5C8\uB2E4|\uC774\uB2E4|\uD568)',
        '\bClose\b.{0,100}(is|acts\s+as|provides|performs).{0,50}(a\s+)?safe[- ]?stop',
        '\bClose\b.{0,100}(constitutes|equals|becomes).{0,50}(the\s+|a\s+)?safe[- ]?stop',
        '\bClose\b.{0,100}\uC548\uC804.{0,30}(Stop|\uC815\uC9C0)(\uC774\uB2E4|\uC784|\uB85C\s+\uB3D9\uC791)',
        '(Close|Dispose|cancel(?:lation)?).{0,120}(automatically|always).{0,60}(sends?\s+(a\s+)?(?:motion\s+)?Stop|stops?\s+motion)',
        '(E[- ]?stop|emergency\s+stop|(HW|hardware)\s*/?\s*(SW|software).{0,20}limits?|\bUNIT\b|\bHome\b).{0,180}(not.{0,30}required|unnecessary|optional|may\s+be\s+skipped|need\s+not|\uBD88\uD544\uC694|\uC120\uD0DD)',
        '(not.{0,30}required|unnecessary|may\s+be\s+skipped|need\s+not|\uBD88\uD544\uC694).{0,120}(E[- ]?stop|limits?|\bUNIT\b|\bHome\b)',
        '(E[- ]?stop|limits?|\bUNIT\b|\bHome\b).{0,160}(can|may)\s+be\s+omitted',
        '\bDLL\b.{0,140}\b(is|was|has\s+been)\b.{0,70}\bsigned\b',
        '\bDLL\b.{0,140}(valid|approved).{0,40}(strong[- ]?name|AuthentiCode).{0,40}(signature|signing)',
        '\bDLL\b.{0,140}(no\s+longer\s+unsigned|strong[- ]?name.{0,60}AuthentiCode.{0,60}(enabled|present))',
        '\bDLL\b.{0,140}(strong[- ]?name|AuthentiCode).{0,50}(\uC11C\uBA85\uB428|\uC11C\uBA85\uB418\uC5C8|\uC11C\uBA85\s*\uC644\uB8CC)'
    )
    $docxReleaseWarningConflicts = @(
        Get-LmcDistributionPolicyMatchingPatterns `
            -Text $DocxText `
            -Patterns $manualReleaseWarningConflictPatterns)
    $pdfReleaseWarningConflicts = @(
        Get-LmcDistributionPolicyMatchingPatterns `
            -Text $PdfText `
            -Patterns $manualReleaseWarningConflictPatterns)
    Assert-LmcDistributionSemanticPolicy `
        -Condition (($docxMissingReleaseWarningPatterns.Count -eq 0) -and
            ($pdfMissingReleaseWarningPatterns.Count -eq 0) -and
            ($docxReleaseWarningConflicts.Count -eq 0) -and
            ($pdfReleaseWarningConflicts.Count -eq 0)) `
        -Blocker 'MANUAL_RELEASE_WARNING_SCOPE' `
        -Message ('Both candidate DOCX and PDF must retain the complete release-warning scope without contradictory approval claims. DOCX missing: {0}; PDF missing: {1}; DOCX conflicts: {2}; PDF conflicts: {3}' -f
            ($docxMissingReleaseWarningPatterns -join ', '),
            ($pdfMissingReleaseWarningPatterns -join ', '),
            ($docxReleaseWarningConflicts -join ', '),
            ($pdfReleaseWarningConflicts -join ', '))
    $checkCount++

    return [pscustomobject]@{
        Result = 'PASS'
        CheckCount = $checkCount
    }
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

    $manualReleasePolicyResult = Test-LmcDistributionManualReleasePolicy `
        -DocxText $docxText `
        -PdfText $pdfText
    $checkCount += [int]$manualReleasePolicyResult.CheckCount

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
