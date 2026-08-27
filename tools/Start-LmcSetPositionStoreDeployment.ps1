[CmdletBinding(DefaultParameterSetName = 'Start')]
param(
    [Parameter(Mandatory = $true, ParameterSetName = 'Start')]
    [string]$Manifest,

    [Parameter(Mandatory = $true, ParameterSetName = 'Start')]
    [string]$ControllerSerial,

    [Parameter(Mandatory = $true, ParameterSetName = 'Start')]
    [string]$StopEvidence,

    [Parameter(Mandatory = $true, ParameterSetName = 'Start')]
    [string]$ReceiptRoot,

    [Parameter(Mandatory = $true, ParameterSetName = 'Start')]
    [string]$OperatorId,

    [Parameter(Mandatory = $true, ParameterSetName = 'SelfTest')]
    [switch]$SelfTest
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'LmcSetPositionStoreDeploymentReceipt.ps1')

if ($SelfTest) {
    Invoke-LmcSpDeploymentSelfTest
    exit 0
}

$result = Invoke-LmcSpDeploymentStart `
    -ManifestPath ([IO.Path]::GetFullPath($Manifest)) `
    -ControllerSerial $ControllerSerial `
    -StopEvidencePath ([IO.Path]::GetFullPath($StopEvidence)) `
    -ReceiptRoot ([IO.Path]::GetFullPath($ReceiptRoot)) `
    -OperatorId $OperatorId

Write-Host "PASS receipt state: $($result.State) / $($result.Result)"
Write-Host "PASS receipt path: $($result.ReceiptPath)"
Write-Host "PASS factory bundle A: $($result.ImageAPath)"
Write-Host "PASS factory bundle B: $($result.ImageBPath)"
Write-Host "PASS STOP/unload evidence SHA-256: $($result.StopEvidenceSha256)"
Write-Host 'NEXT MANUAL STEP: with the PLC application STOPPED and project unloaded, use LASAL CLASS 2 Debug -> File Transfer to upload LMCSP_A.BIN and LMCSP_B.BIN. Then download both files back to the PC and run Verify-LmcSetPositionStoreDeployment.ps1.'
Write-Host 'NOT AUTHORIZED: project start, SetPosition Store gate change, Admin bits 3/5/7, native execution, or production activation.'
exit 0
