[CmdletBinding()]
param(
    [string]$RepositoryRoot,

    [ValidateSet('Pending', 'Approved')]
    [string]$ExpectedState = 'Pending'
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
    $RepositoryRoot = Split-Path -Parent $scriptDirectory
}

$script:PassCount = 0
$profilePath = Join-Path $RepositoryRoot 'docs\api\design\HOME_DS402_EX_AXIS_PROFILE.json'

function Pass([string]$Message) {
    Write-Host "PASS $Message"
    $script:PassCount++
}

function Require-True([bool]$Condition, [string]$Message) {
    if (-not $Condition) {
        throw "HomeDS402Ex axis-profile verification failed: $Message"
    }
    Pass $Message
}

function Require-Null($Value, [string]$Message) {
    Require-True ($null -eq $Value) $Message
}

function Require-NonEmpty($Value, [string]$Message) {
    Require-True (($null -ne $Value) -and (-not [string]::IsNullOrWhiteSpace([string]$Value))) $Message
}

Require-True (Test-Path -LiteralPath $profilePath) 'axis profile manifest exists'
$raw = Get-Content -LiteralPath $profilePath -Raw
Require-True (-not [string]::IsNullOrWhiteSpace($raw)) 'axis profile manifest is non-empty'
$profile = $raw | ConvertFrom-Json

Require-True ($profile.schemaVersion -eq 1) 'profile schema version is 1'
Require-True ($profile.currentMapRevision -eq 1) 'current diagnostics MapRevision baseline is 1'
Require-True ($profile.axes.Count -eq 4) 'profile contains exactly four physical axes'

$axisNumbers = @($profile.axes | ForEach-Object { [int]$_.axis })
Require-True (([string]::Join(',', $axisNumbers)) -ceq '1,2,3,4') 'profile axis order is exactly 1,2,3,4'
Require-True ((@($axisNumbers | Sort-Object -Unique).Count) -eq 4) 'profile axes are unique'

$candidateMethods = New-Object 'System.Collections.Generic.HashSet[int]'
foreach ($method in 1..14) { [void]$candidateMethods.Add($method) }
foreach ($method in 17..30) { [void]$candidateMethods.Add($method) }
foreach ($method in 33..34) { [void]$candidateMethods.Add($method) }

if ($ExpectedState -ceq 'Pending') {
    Require-True ($profile.approvalState -ceq 'pending') 'global profile approval remains pending'
    Require-Null $profile.approvedMapRevision 'pending profile has no approved MapRevision'

    foreach ($axis in $profile.axes) {
        $label = "axis $($axis.axis)"
        Require-True ($axis.approved -eq $false) "$label remains unapproved"
        Require-True ($axis.methodAllowlist.Count -eq 0) "$label method allowlist remains empty until approval"

        foreach ($name in @(
            'homeSwitchSource',
            'positiveLimitSource',
            'negativeLimitSource',
            'indexSource',
            'blockSource',
            'activeLevel',
            'debounceMilliseconds',
            'travelDirection',
            'maxTravel')) {
            Require-Null $axis.wiring.$name "$label wiring.$name is not guessed"
        }

        foreach ($name in @(
            'position',
            'velocity',
            'acceleration',
            'torque',
            'rounding',
            'dintMin',
            'dintMax')) {
            Require-Null $axis.scale.$name "$label scale.$name is not guessed"
        }

        foreach ($name in @('detectionVelocityLimit', 'distanceLimit', 'torqueLimit')) {
            $vendor = $axis.vendorSpecific.$name
            Require-True ($vendor.approved -eq $false) "$label vendor-specific $name remains disabled"
            Require-Null $vendor.objectMapping "$label vendor-specific $name mapping is absent"
        }
    }
}
else {
    Require-True ($profile.approvalState -ceq 'approved') 'global profile approval is approved'
    Require-True (($null -ne $profile.approvedMapRevision) -and
        ([int64]$profile.approvedMapRevision -gt [int64]$profile.currentMapRevision)) `
        'approved profile carries a paired MapRevision increase'

    foreach ($axis in $profile.axes) {
        $label = "axis $($axis.axis)"
        Require-True ($axis.approved -eq $true) "$label is explicitly approved"
        Require-True ($axis.methodAllowlist.Count -gt 0) "$label has a non-empty method allowlist"

        $methodSet = New-Object 'System.Collections.Generic.HashSet[int]'
        foreach ($methodValue in $axis.methodAllowlist) {
            $method = [int]$methodValue
            Require-True ($candidateMethods.Contains($method)) "$label method $method is inside the v1 candidate set"
            Require-True ($methodSet.Add($method)) "$label method $method is not duplicated"
        }

        foreach ($name in @(
            'homeSwitchSource',
            'positiveLimitSource',
            'negativeLimitSource',
            'indexSource',
            'blockSource',
            'activeLevel',
            'debounceMilliseconds',
            'travelDirection',
            'maxTravel')) {
            Require-NonEmpty $axis.wiring.$name "$label wiring.$name is explicitly resolved"
        }

        foreach ($name in @(
            'position',
            'velocity',
            'acceleration',
            'torque',
            'rounding',
            'dintMin',
            'dintMax')) {
            Require-NonEmpty $axis.scale.$name "$label scale.$name is explicitly resolved"
        }

        foreach ($name in @('detectionVelocityLimit', 'distanceLimit', 'torqueLimit')) {
            $vendor = $axis.vendorSpecific.$name
            if ($vendor.approved -eq $true) {
                Require-NonEmpty $vendor.objectMapping "$label approved vendor-specific $name has an object mapping"
            }
            else {
                Require-Null $vendor.objectMapping "$label disabled vendor-specific $name has no object mapping"
            }
        }
    }
}

Write-Host ("HomeDS402Ex axis-profile verification PASS: state={0}; checks={1}" -f $ExpectedState, $script:PassCount)
