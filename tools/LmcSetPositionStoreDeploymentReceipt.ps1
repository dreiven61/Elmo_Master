Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$script:LmcSpReceiptSchema = 1
$script:LmcSpImageSchema = 1
$script:LmcSpImageBytes = 2048
$script:LmcSpZeroSha256 = '0' * 64
$script:LmcSpAllowedStates = @(
    'FactoryNew',
    'FactoryInstallStarted',
    'VerifiedFactoryEmpty',
    'ActivationAuthorized',
    'Activated')
$script:LmcSpReceiptPropertyNames = @(
    'ReceiptSchema',
    'ControllerSerial',
    'State',
    'SourceRevision',
    'ImageSchema',
    'ImageASha256',
    'ImageBSha256',
    'StopEvidenceSha256',
    'PreviousReceiptSha256',
    'Utc',
    'OperatorId')
$script:LmcSpManifestPropertyNames = @(
    'ManifestSchema',
    'ControllerSerial',
    'SourceRevision',
    'ImageSchema',
    'ImageAFileName',
    'ImageABytes',
    'ImageASha256',
    'ImageBFileName',
    'ImageBBytes',
    'ImageBSha256')
$script:LmcSpUtf8 = New-Object System.Text.UTF8Encoding($false)
$script:LmcSpUtf8Strict = New-Object System.Text.UTF8Encoding($false, $true)

function Assert-LmcSpCondition {
    param(
        [Parameter(Mandatory = $true)][bool]$Condition,
        [Parameter(Mandatory = $true)][string]$Message
    )

    if (-not $Condition) {
        throw [System.IO.InvalidDataException]::new($Message)
    }
}

function Assert-LmcSpSafeSerial {
    param([Parameter(Mandatory = $true)][string]$ControllerSerial)

    Assert-LmcSpCondition `
        (-not [string]::IsNullOrWhiteSpace($ControllerSerial)) `
        'ControllerSerial must be non-empty.'
    Assert-LmcSpCondition `
        ($ControllerSerial -match '^[A-Za-z0-9._-]{1,128}$') `
        'ControllerSerial may contain only A-Z, a-z, 0-9, dot, underscore, and hyphen.'
}

function Assert-LmcSpOperatorId {
    param([Parameter(Mandatory = $true)][string]$OperatorId)

    Assert-LmcSpCondition `
        (-not [string]::IsNullOrWhiteSpace($OperatorId)) `
        'OperatorId must be non-empty.'
    Assert-LmcSpCondition `
        ($OperatorId.Length -le 128) `
        'OperatorId must be at most 128 characters.'
    Assert-LmcSpCondition `
        ($OperatorId.IndexOfAny([char[]]@("`r", "`n")) -lt 0) `
        'OperatorId must not contain line breaks.'
}

function Test-LmcSpSha256Text {
    param([string]$Value)
    return $null -ne $Value -and $Value -match '^[0-9A-Fa-f]{64}$'
}

function Get-LmcSpFileSha256 {
    param([Parameter(Mandatory = $true)][string]$Path)
    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToUpperInvariant()
}

function Get-LmcSpTextSha256 {
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$Text)

    $algorithm = [System.Security.Cryptography.SHA256]::Create()
    try {
        return ([BitConverter]::ToString(
            $algorithm.ComputeHash($script:LmcSpUtf8.GetBytes($Text)))).Replace('-', '')
    }
    finally {
        $algorithm.Dispose()
    }
}

function ConvertTo-LmcSpJsonString {
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$Value)
    return ConvertTo-Json -InputObject $Value -Compress
}

function ConvertTo-LmcSpCanonicalReceiptLine {
    param([Parameter(Mandatory = $true)]$Record)

    return ('{' +
        '"ReceiptSchema":' + [string]([int]$Record.ReceiptSchema) + ',' +
        '"ControllerSerial":' + (ConvertTo-LmcSpJsonString ([string]$Record.ControllerSerial)) + ',' +
        '"State":' + (ConvertTo-LmcSpJsonString ([string]$Record.State)) + ',' +
        '"SourceRevision":' + (ConvertTo-LmcSpJsonString ([string]$Record.SourceRevision)) + ',' +
        '"ImageSchema":' + [string]([int]$Record.ImageSchema) + ',' +
        '"ImageASha256":' + (ConvertTo-LmcSpJsonString ([string]$Record.ImageASha256)) + ',' +
        '"ImageBSha256":' + (ConvertTo-LmcSpJsonString ([string]$Record.ImageBSha256)) + ',' +
        '"StopEvidenceSha256":' + (ConvertTo-LmcSpJsonString ([string]$Record.StopEvidenceSha256)) + ',' +
        '"PreviousReceiptSha256":' + (ConvertTo-LmcSpJsonString ([string]$Record.PreviousReceiptSha256)) + ',' +
        '"Utc":' + (ConvertTo-LmcSpJsonString ([string]$Record.Utc)) + ',' +
        '"OperatorId":' + (ConvertTo-LmcSpJsonString ([string]$Record.OperatorId)) +
        '}')
}

function Assert-LmcSpExactProperties {
    param(
        [Parameter(Mandatory = $true)]$Object,
        [Parameter(Mandatory = $true)][string[]]$Expected,
        [Parameter(Mandatory = $true)][string]$Scope
    )

    $actual = @($Object.PSObject.Properties | ForEach-Object { $_.Name })
    Assert-LmcSpCondition `
        ($actual.Count -eq $Expected.Count) `
        "$Scope contains an unexpected property count."
    for ($index = 0; $index -lt $Expected.Count; $index++) {
        Assert-LmcSpCondition `
            ($actual[$index] -ceq $Expected[$index]) `
            "$Scope property order/name mismatch at index $index."
    }
}

function Assert-LmcSpReceiptRecordShape {
    param(
        [Parameter(Mandatory = $true)]$Record,
        [Parameter(Mandatory = $true)][string]$ExpectedSerial,
        [Parameter(Mandatory = $true)][int]$Index
    )

    Assert-LmcSpExactProperties $Record $script:LmcSpReceiptPropertyNames "Receipt[$Index]"
    Assert-LmcSpCondition ([int]$Record.ReceiptSchema -eq $script:LmcSpReceiptSchema) "Receipt[$Index] schema must be 1."
    Assert-LmcSpCondition ([string]$Record.ControllerSerial -ceq $ExpectedSerial) "Receipt[$Index] ControllerSerial mismatch."
    Assert-LmcSpCondition ($script:LmcSpAllowedStates -ccontains [string]$Record.State) "Receipt[$Index] state is unsupported."
    Assert-LmcSpCondition ([string]$Record.SourceRevision -match '^[0-9A-Fa-f]{40}$') "Receipt[$Index] SourceRevision must be a 40-hex Git revision."
    Assert-LmcSpCondition ([int]$Record.ImageSchema -eq $script:LmcSpImageSchema) "Receipt[$Index] ImageSchema must be 1."
    Assert-LmcSpCondition (Test-LmcSpSha256Text ([string]$Record.ImageASha256)) "Receipt[$Index] ImageASha256 is invalid."
    Assert-LmcSpCondition (Test-LmcSpSha256Text ([string]$Record.ImageBSha256)) "Receipt[$Index] ImageBSha256 is invalid."
    Assert-LmcSpCondition (Test-LmcSpSha256Text ([string]$Record.StopEvidenceSha256)) "Receipt[$Index] StopEvidenceSha256 is invalid."
    Assert-LmcSpCondition (Test-LmcSpSha256Text ([string]$Record.PreviousReceiptSha256)) "Receipt[$Index] PreviousReceiptSha256 is invalid."
    [datetime]$utc = [datetime]::MinValue
    Assert-LmcSpCondition `
        ([datetime]::TryParseExact(
            [string]$Record.Utc,
            'yyyy-MM-ddTHH:mm:ss.fffffffZ',
            [Globalization.CultureInfo]::InvariantCulture,
            [Globalization.DateTimeStyles]::AssumeUniversal -bor [Globalization.DateTimeStyles]::AdjustToUniversal,
            [ref]$utc)) `
        "Receipt[$Index] Utc must be canonical UTC with seven fractional digits."
    Assert-LmcSpOperatorId ([string]$Record.OperatorId)
}

function New-LmcSpReceiptRecord {
    param(
        [Parameter(Mandatory = $true)][string]$ControllerSerial,
        [Parameter(Mandatory = $true)][string]$State,
        [Parameter(Mandatory = $true)][string]$SourceRevision,
        [Parameter(Mandatory = $true)][string]$ImageASha256,
        [Parameter(Mandatory = $true)][string]$ImageBSha256,
        [Parameter(Mandatory = $true)][string]$StopEvidenceSha256,
        [Parameter(Mandatory = $true)][string]$PreviousReceiptSha256,
        [Parameter(Mandatory = $true)][string]$OperatorId,
        [datetime]$Utc = [datetime]::UtcNow
    )

    return [pscustomobject][ordered]@{
        ReceiptSchema = $script:LmcSpReceiptSchema
        ControllerSerial = $ControllerSerial
        State = $State
        SourceRevision = $SourceRevision.ToLowerInvariant()
        ImageSchema = $script:LmcSpImageSchema
        ImageASha256 = $ImageASha256.ToUpperInvariant()
        ImageBSha256 = $ImageBSha256.ToUpperInvariant()
        StopEvidenceSha256 = $StopEvidenceSha256.ToUpperInvariant()
        PreviousReceiptSha256 = $PreviousReceiptSha256.ToUpperInvariant()
        Utc = $Utc.ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ss.fffffffZ', [Globalization.CultureInfo]::InvariantCulture)
        OperatorId = $OperatorId
    }
}

function Get-LmcSpReceiptPath {
    param(
        [Parameter(Mandatory = $true)][string]$ReceiptRoot,
        [Parameter(Mandatory = $true)][string]$ControllerSerial
    )

    Assert-LmcSpSafeSerial $ControllerSerial
    $root = [IO.Path]::GetFullPath($ReceiptRoot)
    return Join-Path (Join-Path $root $ControllerSerial) 'deployment_receipts.jsonl'
}

function Read-LmcSpReceiptChain {
    param(
        [Parameter(Mandatory = $true)][string]$ReceiptPath,
        [Parameter(Mandatory = $true)][string]$ControllerSerial
    )

    if (-not (Test-Path -LiteralPath $ReceiptPath -PathType Leaf)) {
        return [pscustomobject]@{ Records = @(); Lines = @() }
    }

    $bytes = [IO.File]::ReadAllBytes($ReceiptPath)
    Assert-LmcSpCondition ($bytes.Length -gt 0) 'Receipt file must not be empty.'
    Assert-LmcSpCondition `
        (-not ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF)) `
        'Receipt file must be UTF-8 without BOM.'
    $text = $script:LmcSpUtf8Strict.GetString($bytes)
    Assert-LmcSpCondition ($text.IndexOf("`r", [StringComparison]::Ordinal) -lt 0) 'Receipt file must use canonical LF line endings.'
    Assert-LmcSpCondition ($text.EndsWith("`n", [StringComparison]::Ordinal)) 'Receipt file must end with LF.'

    $rawLines = $text.Split([char]10)
    Assert-LmcSpCondition ($rawLines[$rawLines.Length - 1].Length -eq 0) 'Receipt framing is invalid.'
    $lines = @($rawLines[0..($rawLines.Length - 2)])
    Assert-LmcSpCondition ($lines.Count -ge 1 -and $lines.Count -le $script:LmcSpAllowedStates.Count) 'Receipt record count is outside the supported state chain.'

    # Materialize through ArrayList instead of expanding List[object] with
    # @(...). Windows PowerShell 5.1 and PowerShell 7 can otherwise throw
    # "Argument types do not match" while constructing the return object.
    $records = New-Object System.Collections.ArrayList
    $previousLine = $null
    for ($index = 0; $index -lt $lines.Count; $index++) {
        $line = [string]$lines[$index]
        Assert-LmcSpCondition (-not [string]::IsNullOrWhiteSpace($line)) "Receipt[$index] must not be blank."
        try {
            $record = $line | ConvertFrom-Json
        }
        catch {
            throw [IO.InvalidDataException]::new("Receipt[$index] is not valid JSON.", $_.Exception)
        }
        Assert-LmcSpReceiptRecordShape $record $ControllerSerial $index
        $canonical = ConvertTo-LmcSpCanonicalReceiptLine $record
        Assert-LmcSpCondition ($canonical -ceq $line) "Receipt[$index] is not in canonical byte form."
        Assert-LmcSpCondition ([string]$record.State -ceq $script:LmcSpAllowedStates[$index]) "Receipt[$index] violates the monotonic factory/activation state chain."
        if ($index -eq 0) {
            Assert-LmcSpCondition ([string]$record.PreviousReceiptSha256 -ceq $script:LmcSpZeroSha256) 'FactoryNew must use an all-zero PreviousReceiptSha256.'
        }
        else {
            $expectedPrevious = Get-LmcSpTextSha256 $previousLine
            Assert-LmcSpCondition ([string]$record.PreviousReceiptSha256 -ceq $expectedPrevious) "Receipt[$index] PreviousReceiptSha256 does not match the previous canonical record."
        }
        [void]$records.Add($record)
        $previousLine = $line
    }

    return [pscustomobject]@{
        Records = [object[]]$records.ToArray()
        Lines = @($lines)
    }
}

function Append-LmcSpReceiptRecord {
    param(
        [Parameter(Mandatory = $true)][string]$ReceiptPath,
        [Parameter(Mandatory = $true)]$Record
    )

    $directory = Split-Path -Parent $ReceiptPath
    if (-not (Test-Path -LiteralPath $directory -PathType Container)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }
    $line = ConvertTo-LmcSpCanonicalReceiptLine $Record
    [IO.File]::AppendAllText($ReceiptPath, $line + "`n", $script:LmcSpUtf8)
    return $line
}

function Invoke-LmcSpReceiptLocked {
    param(
        [Parameter(Mandatory = $true)][string]$ReceiptPath,
        [Parameter(Mandatory = $true)][scriptblock]$Action
    )

    $directory = Split-Path -Parent $ReceiptPath
    if (-not (Test-Path -LiteralPath $directory -PathType Container)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }
    $lockPath = $ReceiptPath + '.lock'
    $stream = $null
    try {
        $stream = New-Object IO.FileStream(
            $lockPath,
            [IO.FileMode]::OpenOrCreate,
            [IO.FileAccess]::ReadWrite,
            [IO.FileShare]::None)
        return & $Action
    }
    finally {
        if ($null -ne $stream) {
            $stream.Dispose()
        }
        Remove-Item -LiteralPath $lockPath -Force -ErrorAction SilentlyContinue
    }
}

function Read-LmcSpDeploymentManifest {
    param(
        [Parameter(Mandatory = $true)][string]$ManifestPath,
        [Parameter(Mandatory = $true)][string]$ControllerSerial
    )

    Assert-LmcSpSafeSerial $ControllerSerial
    Assert-LmcSpCondition (Test-Path -LiteralPath $ManifestPath -PathType Leaf) 'Manifest file is missing.'
    try {
        $manifest = Get-Content -LiteralPath $ManifestPath -Raw | ConvertFrom-Json
    }
    catch {
        throw [IO.InvalidDataException]::new('Manifest is not valid JSON.', $_.Exception)
    }
    Assert-LmcSpExactProperties $manifest $script:LmcSpManifestPropertyNames 'Manifest'
    Assert-LmcSpCondition ([int]$manifest.ManifestSchema -eq 1) 'ManifestSchema must be 1.'
    Assert-LmcSpCondition ([string]$manifest.ControllerSerial -ceq $ControllerSerial) 'Manifest ControllerSerial does not match the requested controller.'
    Assert-LmcSpCondition ([string]$manifest.SourceRevision -match '^[0-9A-Fa-f]{40}$') 'Manifest SourceRevision must be a 40-hex Git revision.'
    Assert-LmcSpCondition ([int]$manifest.ImageSchema -eq $script:LmcSpImageSchema) 'Manifest ImageSchema must be 1.'
    Assert-LmcSpCondition ([string]$manifest.ImageAFileName -ceq 'LMCSP_A.BIN') 'Manifest ImageAFileName must be LMCSP_A.BIN.'
    Assert-LmcSpCondition ([string]$manifest.ImageBFileName -ceq 'LMCSP_B.BIN') 'Manifest ImageBFileName must be LMCSP_B.BIN.'
    Assert-LmcSpCondition ([int64]$manifest.ImageABytes -eq $script:LmcSpImageBytes) 'Manifest ImageABytes must be exactly 2048.'
    Assert-LmcSpCondition ([int64]$manifest.ImageBBytes -eq $script:LmcSpImageBytes) 'Manifest ImageBBytes must be exactly 2048.'
    Assert-LmcSpCondition (Test-LmcSpSha256Text ([string]$manifest.ImageASha256)) 'Manifest ImageASha256 is invalid.'
    Assert-LmcSpCondition (Test-LmcSpSha256Text ([string]$manifest.ImageBSha256)) 'Manifest ImageBSha256 is invalid.'
    Assert-LmcSpCondition ([string]$manifest.ImageASha256 -ceq [string]$manifest.ImageBSha256) 'Factory-empty A/B images must have identical full-image SHA-256 values.'
    return $manifest
}

function Assert-LmcSpManifestIdentityMatchesReceipt {
    param(
        [Parameter(Mandatory = $true)]$Manifest,
        [Parameter(Mandatory = $true)]$Record,
        [Parameter(Mandatory = $true)][string]$Scope
    )

    Assert-LmcSpCondition ([string]$Record.ControllerSerial -ceq [string]$Manifest.ControllerSerial) "$Scope controller identity mismatch."
    Assert-LmcSpCondition ([string]$Record.SourceRevision -ceq ([string]$Manifest.SourceRevision).ToLowerInvariant()) "$Scope source revision mismatch."
    Assert-LmcSpCondition ([int]$Record.ImageSchema -eq [int]$Manifest.ImageSchema) "$Scope image schema mismatch."
    Assert-LmcSpCondition ([string]$Record.ImageASha256 -ceq ([string]$Manifest.ImageASha256).ToUpperInvariant()) "$Scope image A identity mismatch."
    Assert-LmcSpCondition ([string]$Record.ImageBSha256 -ceq ([string]$Manifest.ImageBSha256).ToUpperInvariant()) "$Scope image B identity mismatch."
}

function Assert-LmcSpFactoryBundleFiles {
    param(
        [Parameter(Mandatory = $true)][string]$ManifestPath,
        [Parameter(Mandatory = $true)]$Manifest
    )

    $manifestDirectory = Split-Path -Parent ([IO.Path]::GetFullPath($ManifestPath))
    $imageAPath = Join-Path $manifestDirectory ([string]$Manifest.ImageAFileName)
    $imageBPath = Join-Path $manifestDirectory ([string]$Manifest.ImageBFileName)
    foreach ($path in @($imageAPath, $imageBPath)) {
        Assert-LmcSpCondition (Test-Path -LiteralPath $path -PathType Leaf) "Factory bundle image is missing: $path"
        Assert-LmcSpCondition ((Get-Item -LiteralPath $path).Length -eq $script:LmcSpImageBytes) "Factory bundle image must be exactly 2048 bytes: $path"
    }
    Assert-LmcSpCondition ((Get-LmcSpFileSha256 $imageAPath) -ceq ([string]$Manifest.ImageASha256).ToUpperInvariant()) 'Factory bundle image A SHA-256 does not match manifest.'
    Assert-LmcSpCondition ((Get-LmcSpFileSha256 $imageBPath) -ceq ([string]$Manifest.ImageBSha256).ToUpperInvariant()) 'Factory bundle image B SHA-256 does not match manifest.'
    $bytesA = [IO.File]::ReadAllBytes($imageAPath)
    $bytesB = [IO.File]::ReadAllBytes($imageBPath)
    Assert-LmcSpCondition ([Linq.Enumerable]::SequenceEqual([byte[]]$bytesA, [byte[]]$bytesB)) 'Factory-empty A/B images must be byte-for-byte identical.'
    return [pscustomobject]@{ ImageAPath = $imageAPath; ImageBPath = $imageBPath }
}

function Assert-LmcSpReadbackFiles {
    param(
        [Parameter(Mandatory = $true)]$Manifest,
        [Parameter(Mandatory = $true)][string]$ReadbackA,
        [Parameter(Mandatory = $true)][string]$ReadbackB
    )

    foreach ($path in @($ReadbackA, $ReadbackB)) {
        Assert-LmcSpCondition (Test-Path -LiteralPath $path -PathType Leaf) "Readback file is missing: $path"
        Assert-LmcSpCondition ((Get-Item -LiteralPath $path).Length -eq $script:LmcSpImageBytes) "Readback file must be exactly 2048 bytes: $path"
    }
    Assert-LmcSpCondition ((Get-LmcSpFileSha256 $ReadbackA) -ceq ([string]$Manifest.ImageASha256).ToUpperInvariant()) 'Readback A SHA-256 does not match manifest.'
    Assert-LmcSpCondition ((Get-LmcSpFileSha256 $ReadbackB) -ceq ([string]$Manifest.ImageBSha256).ToUpperInvariant()) 'Readback B SHA-256 does not match manifest.'
    $bytesA = [IO.File]::ReadAllBytes($ReadbackA)
    $bytesB = [IO.File]::ReadAllBytes($ReadbackB)
    Assert-LmcSpCondition ([Linq.Enumerable]::SequenceEqual([byte[]]$bytesA, [byte[]]$bytesB)) 'Readback A/B images are not byte-for-byte identical.'
}

function Invoke-LmcSpDeploymentStart {
    param(
        [Parameter(Mandatory = $true)][string]$ManifestPath,
        [Parameter(Mandatory = $true)][string]$ControllerSerial,
        [Parameter(Mandatory = $true)][string]$StopEvidencePath,
        [Parameter(Mandatory = $true)][string]$ReceiptRoot,
        [Parameter(Mandatory = $true)][string]$OperatorId
    )

    Assert-LmcSpOperatorId $OperatorId
    $manifest = Read-LmcSpDeploymentManifest $ManifestPath $ControllerSerial
    $bundle = Assert-LmcSpFactoryBundleFiles $ManifestPath $manifest
    Assert-LmcSpCondition (Test-Path -LiteralPath $StopEvidencePath -PathType Leaf) 'STOP/unload evidence file is missing.'
    Assert-LmcSpCondition ((Get-Item -LiteralPath $StopEvidencePath).Length -gt 0) 'STOP/unload evidence file must be non-empty.'
    $stopSha = Get-LmcSpFileSha256 $StopEvidencePath
    $receiptPath = Get-LmcSpReceiptPath $ReceiptRoot $ControllerSerial

    return Invoke-LmcSpReceiptLocked $receiptPath {
        $chain = Read-LmcSpReceiptChain $receiptPath $ControllerSerial
        Assert-LmcSpCondition ($chain.Records.Count -ge 1) 'FactoryNew receipt is required before deployment can start.'
        Assert-LmcSpCondition ($chain.Records.Count -le 2) 'Factory deployment start is forbidden after a successful deployment or activation record exists.'
        Assert-LmcSpManifestIdentityMatchesReceipt $manifest $chain.Records[0] 'FactoryNew receipt'

        if ($chain.Records.Count -eq 2) {
            Assert-LmcSpManifestIdentityMatchesReceipt $manifest $chain.Records[1] 'FactoryInstallStarted receipt'
            Assert-LmcSpCondition ([string]$chain.Records[1].StopEvidenceSha256 -ceq $stopSha) 'Resume requires the exact STOP/unload evidence used by the existing FactoryInstallStarted receipt.'
            return [pscustomobject]@{
                State = 'FactoryInstallStarted'
                Result = 'RESUME_ALLOWED_NO_APPEND'
                ReceiptPath = $receiptPath
                ImageAPath = $bundle.ImageAPath
                ImageBPath = $bundle.ImageBPath
                StopEvidenceSha256 = $stopSha
                CapabilityActivation = 'KEEP_OFF'
            }
        }

        $previousHash = Get-LmcSpTextSha256 ([string]$chain.Lines[0])
        $record = New-LmcSpReceiptRecord `
            -ControllerSerial $ControllerSerial `
            -State 'FactoryInstallStarted' `
            -SourceRevision ([string]$manifest.SourceRevision) `
            -ImageASha256 ([string]$manifest.ImageASha256) `
            -ImageBSha256 ([string]$manifest.ImageBSha256) `
            -StopEvidenceSha256 $stopSha `
            -PreviousReceiptSha256 $previousHash `
            -OperatorId $OperatorId
        [void](Append-LmcSpReceiptRecord $receiptPath $record)
        [void](Read-LmcSpReceiptChain $receiptPath $ControllerSerial)
        return [pscustomobject]@{
            State = 'FactoryInstallStarted'
            Result = 'APPENDED'
            ReceiptPath = $receiptPath
            ImageAPath = $bundle.ImageAPath
            ImageBPath = $bundle.ImageBPath
            StopEvidenceSha256 = $stopSha
            CapabilityActivation = 'KEEP_OFF'
        }
    }
}

function Invoke-LmcSpDeploymentVerify {
    param(
        [Parameter(Mandatory = $true)][string]$ManifestPath,
        [Parameter(Mandatory = $true)][string]$ReadbackA,
        [Parameter(Mandatory = $true)][string]$ReadbackB,
        [Parameter(Mandatory = $true)][string]$ControllerSerial,
        [Parameter(Mandatory = $true)][string]$StopEvidencePath,
        [Parameter(Mandatory = $true)][string]$ReceiptRoot,
        [Parameter(Mandatory = $true)][string]$OperatorId
    )

    Assert-LmcSpOperatorId $OperatorId
    $manifest = Read-LmcSpDeploymentManifest $ManifestPath $ControllerSerial
    [void](Assert-LmcSpFactoryBundleFiles $ManifestPath $manifest)
    Assert-LmcSpReadbackFiles $manifest $ReadbackA $ReadbackB
    Assert-LmcSpCondition (Test-Path -LiteralPath $StopEvidencePath -PathType Leaf) 'STOP/unload evidence file is missing.'
    $stopSha = Get-LmcSpFileSha256 $StopEvidencePath
    $receiptPath = Get-LmcSpReceiptPath $ReceiptRoot $ControllerSerial

    return Invoke-LmcSpReceiptLocked $receiptPath {
        $chain = Read-LmcSpReceiptChain $receiptPath $ControllerSerial
        Assert-LmcSpCondition ($chain.Records.Count -eq 2) 'Verification requires the exact FactoryNew -> FactoryInstallStarted chain and refuses duplicate/successful deployment history.'
        Assert-LmcSpManifestIdentityMatchesReceipt $manifest $chain.Records[0] 'FactoryNew receipt'
        Assert-LmcSpManifestIdentityMatchesReceipt $manifest $chain.Records[1] 'FactoryInstallStarted receipt'
        Assert-LmcSpCondition ([string]$chain.Records[1].StopEvidenceSha256 -ceq $stopSha) 'Verification STOP/unload evidence does not match FactoryInstallStarted.'

        $previousHash = Get-LmcSpTextSha256 ([string]$chain.Lines[1])
        $record = New-LmcSpReceiptRecord `
            -ControllerSerial $ControllerSerial `
            -State 'VerifiedFactoryEmpty' `
            -SourceRevision ([string]$manifest.SourceRevision) `
            -ImageASha256 ([string]$manifest.ImageASha256) `
            -ImageBSha256 ([string]$manifest.ImageBSha256) `
            -StopEvidenceSha256 $stopSha `
            -PreviousReceiptSha256 $previousHash `
            -OperatorId $OperatorId
        [void](Append-LmcSpReceiptRecord $receiptPath $record)
        $verifiedChain = Read-LmcSpReceiptChain $receiptPath $ControllerSerial
        Assert-LmcSpCondition ($verifiedChain.Records.Count -eq 3) 'VerifiedFactoryEmpty receipt append did not produce the exact three-record chain.'
        return [pscustomobject]@{
            State = 'VerifiedFactoryEmpty'
            Result = 'APPENDED'
            ReceiptPath = $receiptPath
            ReadbackASha256 = Get-LmcSpFileSha256 $ReadbackA
            ReadbackBSha256 = Get-LmcSpFileSha256 $ReadbackB
            CrcSemanticStatus = 'EXTERNAL_PENDING_ISSUE_44'
            ProjectStart = 'KEEP_STOPPED_UNTIL_OPERATOR_REVIEW'
            CapabilityActivation = 'KEEP_OFF'
        }
    }
}

function Invoke-LmcSpDeploymentSelfTest {
    $root = Join-Path ([IO.Path]::GetTempPath()) ('ElmoSetPositionReceiptSelfTest-' + [guid]::NewGuid().ToString('N'))
    try {
        $bundleRoot = Join-Path $root 'bundle'
        $receiptRoot = Join-Path $root 'receipts'
        New-Item -ItemType Directory -Path $bundleRoot -Force | Out-Null
        New-Item -ItemType Directory -Path $receiptRoot -Force | Out-Null
        $serial = 'SELFTEST-0001'
        $source = '0123456789abcdef0123456789abcdef01234567'
        $operator = 'self-test'
        $imageA = Join-Path $bundleRoot 'LMCSP_A.BIN'
        $imageB = Join-Path $bundleRoot 'LMCSP_B.BIN'
        $synthetic = New-Object byte[] $script:LmcSpImageBytes
        [IO.File]::WriteAllBytes($imageA, $synthetic)
        [IO.File]::WriteAllBytes($imageB, $synthetic)
        $imageSha = Get-LmcSpFileSha256 $imageA
        $manifestPath = Join-Path $bundleRoot 'manifest.json'
        $manifest = [pscustomobject][ordered]@{
            ManifestSchema = 1
            ControllerSerial = $serial
            SourceRevision = $source
            ImageSchema = 1
            ImageAFileName = 'LMCSP_A.BIN'
            ImageABytes = 2048
            ImageASha256 = $imageSha
            ImageBFileName = 'LMCSP_B.BIN'
            ImageBBytes = 2048
            ImageBSha256 = $imageSha
        }
        [IO.File]::WriteAllText($manifestPath, ($manifest | ConvertTo-Json -Depth 4), $script:LmcSpUtf8)

        $factoryEvidence = Join-Path $root 'factory-new-evidence.txt'
        $stopEvidence = Join-Path $root 'stopped-unloaded-evidence.txt'
        [IO.File]::WriteAllText($factoryEvidence, 'synthetic inventory evidence', $script:LmcSpUtf8)
        [IO.File]::WriteAllText($stopEvidence, 'synthetic stopped/unloaded evidence', $script:LmcSpUtf8)
        $receiptPath = Get-LmcSpReceiptPath $receiptRoot $serial
        $factoryRecord = New-LmcSpReceiptRecord `
            -ControllerSerial $serial `
            -State 'FactoryNew' `
            -SourceRevision $source `
            -ImageASha256 $imageSha `
            -ImageBSha256 $imageSha `
            -StopEvidenceSha256 (Get-LmcSpFileSha256 $factoryEvidence) `
            -PreviousReceiptSha256 $script:LmcSpZeroSha256 `
            -OperatorId 'inventory-self-test'
        [void](Append-LmcSpReceiptRecord $receiptPath $factoryRecord)
        [void](Read-LmcSpReceiptChain $receiptPath $serial)

        $started = Invoke-LmcSpDeploymentStart $manifestPath $serial $stopEvidence $receiptRoot $operator
        Assert-LmcSpCondition ($started.Result -ceq 'APPENDED') 'Self-test initial start did not append FactoryInstallStarted.'
        $resumed = Invoke-LmcSpDeploymentStart $manifestPath $serial $stopEvidence $receiptRoot $operator
        Assert-LmcSpCondition ($resumed.Result -ceq 'RESUME_ALLOWED_NO_APPEND') 'Self-test interrupted deployment resume was not idempotent.'

        $readbackA = Join-Path $root 'readback-A.bin'
        $readbackB = Join-Path $root 'readback-B.bin'
        Copy-Item -LiteralPath $imageA -Destination $readbackA
        Copy-Item -LiteralPath $imageB -Destination $readbackB
        $verified = Invoke-LmcSpDeploymentVerify $manifestPath $readbackA $readbackB $serial $stopEvidence $receiptRoot $operator
        Assert-LmcSpCondition ($verified.State -ceq 'VerifiedFactoryEmpty') 'Self-test did not append VerifiedFactoryEmpty.'

        $duplicateRejected = $false
        try {
            [void](Invoke-LmcSpDeploymentVerify $manifestPath $readbackA $readbackB $serial $stopEvidence $receiptRoot $operator)
        }
        catch {
            $duplicateRejected = $true
        }
        Assert-LmcSpCondition $duplicateRejected 'Self-test duplicate verification was not rejected.'

        $corruptReceiptPath = Join-Path $root 'corrupt-receipt.jsonl'
        Copy-Item -LiteralPath $receiptPath -Destination $corruptReceiptPath
        $receiptText = [IO.File]::ReadAllText($corruptReceiptPath)
        $receiptText = $receiptText.Replace('VerifiedFactoryEmpty', 'VerifiedFactoryEmptx')
        [IO.File]::WriteAllText($corruptReceiptPath, $receiptText, $script:LmcSpUtf8)
        $tamperRejected = $false
        try {
            [void](Read-LmcSpReceiptChain $corruptReceiptPath $serial)
        }
        catch {
            $tamperRejected = $true
        }
        Assert-LmcSpCondition $tamperRejected 'Self-test tampered receipt was not rejected.'

        Write-Host 'PASS SP-01 factory receipt self-test: FactoryNew -> FactoryInstallStarted -> VerifiedFactoryEmpty'
        Write-Host 'PASS interrupted start is idempotent without adding a second FactoryInstallStarted record'
        Write-Host 'PASS duplicate verification and canonical receipt tamper are rejected fail-closed'
        Write-Host 'NOTE synthetic 2048-byte self-test images do not claim vendor CheckSum.CRC32 validity; issue #44 remains required.'
    }
    finally {
        if (Test-Path -LiteralPath $root) {
            Remove-Item -LiteralPath $root -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
}
