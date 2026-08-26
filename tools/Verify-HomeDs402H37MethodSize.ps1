[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [int]$LimitBytes = 32768
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$script:PassCount = 0

function Assert-True {
    param(
        [Parameter(Mandatory = $true)][bool]$Condition,
        [Parameter(Mandatory = $true)][string]$Message
    )

    if (-not $Condition) {
        throw "FAIL H37 method-size verifier: $Message"
    }

    $script:PassCount++
    Write-Host "PASS $Message"
}

function ConvertTo-LfText {
    param([Parameter(Mandatory = $true)][string]$Text)
    return $Text.Replace("`r`n", "`n").Replace("`r", "`n")
}

function Get-LasalFunctionBody {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$QualifiedName
    )

    $escapedName = [regex]::Escape($QualifiedName)
    $pattern = "(?ms)^[\t ]*FUNCTION(?:[\t ]+(?:GLOBAL|VIRTUAL))*[\t ]+$escapedName\b.*?^[\t ]*END_FUNCTION[\t ]*$"
    $matches = [regex]::Matches($Text, $pattern)
    Assert-True ($matches.Count -eq 1) "exact LASAL function body $QualifiedName exists once"
    return $matches[0].Value
}

$diagnosticsPath = Join-Path $RepositoryRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCDiagnosticsService\LMCDiagnosticsService.st'
Assert-True (Test-Path -LiteralPath $diagnosticsPath) 'LMCDiagnosticsService.st exists'

$diagnostics = ConvertTo-LfText ([System.IO.File]::ReadAllText($diagnosticsPath))
$methods = @(
    'LMCDiagnosticsService::HandleAxisDs402HomeStart',
    'LMCDiagnosticsService::HandleAxisDs402HomeOutcome',
    'LMCDiagnosticsService::HandleAxisDs402HomeRetire',
    'LMCDiagnosticsService::ProcessAxisDs402Home'
)

$largestName = ''
$largestBytes = 0
foreach ($method in $methods) {
    $body = Get-LasalFunctionBody -Text $diagnostics -QualifiedName $method
    $bytes = [System.Text.Encoding]::UTF8.GetByteCount($body)
    Assert-True ($bytes -lt $LimitBytes) "$method method budget $bytes < $LimitBytes bytes"
    if ($bytes -gt $largestBytes) {
        $largestBytes = $bytes
        $largestName = $method
    }
}

Assert-True ($largestBytes -gt 0) 'HomeDS402 method-size inventory is non-empty'
Write-Host ("H37 method-size verifier PASS: {0} checks; largest={1} ({2} bytes)" -f $script:PassCount, $largestName, $largestBytes)
