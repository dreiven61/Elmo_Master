param(
    [switch]$RunSelfTest,
    [string]$RepositoryRoot
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    throw 'RepositoryRoot is required.'
}
if ($RunSelfTest) {
    $verifierPath = Join-Path $PSScriptRoot 'Verify-LasalContract.ps1'
    & $verifierPath `
        -RepositoryRoot $RepositoryRoot `
        -AxisOwnershipPreemptionCleanupVerifierSelfTestOnly
    return
}

$controlPath = Join-Path $RepositoryRoot (
    'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\' +
    'LMCControlCommandService\LMCControlCommandService.st')
$control = Get-Content -Raw -LiteralPath $controlPath

[pscustomobject]@{
    Control = $control
}
