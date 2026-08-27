[CmdletBinding(DefaultParameterSetName = 'Verify')]
param(
    [Parameter(Mandatory = $true, ParameterSetName = 'Verify')]
    [string]$Manifest,

    [Parameter(Mandatory = $true, ParameterSetName = 'Verify')]
    [string]$ReadbackA,

    [Parameter(Mandatory = $true, ParameterSetName = 'Verify')]
    [string]$ReadbackB,

    [Parameter(Mandatory = $true, ParameterSetName = 'Verify')]
    [string]$ControllerSerial,

    [Parameter(Mandatory = $true, ParameterSetName = 'Verify')]
    [string]$StopEvidence,

    [Parameter(Mandatory = $true, ParameterSetName = 'Verify')]
    [string]$ReceiptRoot,

    [Parameter(Mandatory = $true, ParameterSetName = 'Verify')]
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

$result = Invoke-LmcSpDeploymentVerify `
    -ManifestPath ([IO.Path]::GetFullPath($Manifest)) `
    -ReadbackA ([IO.Path]::GetFullPath($ReadbackA)) `
    -ReadbackB ([IO.Path]::GetFullPath($ReadbackB)) `
    -ControllerSerial $ControllerSerial `
    -StopEvidencePath ([IO.Path]::GetFullPath($StopEvidence)) `
    -ReceiptRoot ([IO.Path]::GetFullPath($ReceiptRoot)) `
    -OperatorId $OperatorId

Write-Host "PASS receipt state: $($result.State)"
Write-Host "PASS receipt path: $($result.ReceiptPath)"
Write-Host "PASS readback A SHA-256: $($result.ReadbackASha256)"
Write-Host "PASS readback B SHA-256: $($result.ReadbackBSha256)"
Write-Host "BOUNDARY vendor CRC semantic validation: $($result.CrcSemanticStatus)"
Write-Host "BOUNDARY project start: $($result.ProjectStart)"
Write-Host "BOUNDARY capability activation: $($result.CapabilityActivation)"
Write-Host 'This PASS proves factory bundle/readback identity and receipt-chain integrity only. It does not prove internal vendor CRC semantics, PLC runtime durability, cold restart, native SetPosition execution, or production activation.'
exit 0
