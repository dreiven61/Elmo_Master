param(
    [Parameter(Mandatory = $true)]
    [string]$RepositoryRoot
)

$ErrorActionPreference = 'Stop'

$root = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$utf8 = [Text.UTF8Encoding]::new($false, $true)
$expectedCurrentCanonicalLfBytes = 591670
$expectedCurrentCanonicalLfSha256 =
    '7EAB9F0E71A85C1459FD01A381859D9EC5095949D536E78B056A67BE91C2D1BE'
$controlPath = Join-Path $root (
    'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\' +
    'LMCControlCommandService\LMCControlCommandService.st')
if (-not (Test-Path -LiteralPath $controlPath -PathType Leaf)) {
    throw "Rollback split current source is missing: $controlPath"
}
$controlText = [IO.File]::ReadAllText($controlPath, $utf8)
$controlCanonicalLf =
    $controlText.Replace("`r`n", "`n").Replace("`r", "`n")
$controlCanonicalLfBytes = $utf8.GetByteCount($controlCanonicalLf)
$controlCanonicalLfSha256 = [Convert]::ToHexString(
    [Security.Cryptography.SHA256]::HashData(
        $utf8.GetBytes($controlCanonicalLf)))
if (($controlCanonicalLfBytes -ne $expectedCurrentCanonicalLfBytes) -or
    ($controlCanonicalLfSha256 -cne $expectedCurrentCanonicalLfSha256)) {
    throw (
        'Rollback split fixture requires the exact current A51E post-state; ' +
        "canonical LF is $controlCanonicalLfBytes/$controlCanonicalLfSha256.")
}

$plannerPath = Join-Path $root (
    'test\Reports_Lasal\C78_20260807_rollback_split_rebaseline\' +
    'Plan-RollbackSplit.ps1')
if (-not (Test-Path -LiteralPath $plannerPath -PathType Leaf)) {
    throw "Rollback split planner is missing: $plannerPath"
}

$plannerOutput = @(
    & $plannerPath -RunSelfTest -RepositoryRoot $root 6>&1 |
        ForEach-Object { $_.ToString() })
$expectedTerminalPass = (
    'PASS LASAL.AxisOwnershipRollbackSplitPlan.SelfTest ' +
    '(18/18 negative fixtures rejected; positive candidate accepted)')
$terminalPass = @(
    $plannerOutput | Where-Object {
        $_ -ceq $expectedTerminalPass
    })
if (($plannerOutput.Count -ne 1) -or ($terminalPass.Count -ne 1)) {
    throw (
        'Rollback split planner did not publish the exact single 18/18 ' +
        "terminal PASS line; output/pass counts are " +
        "$($plannerOutput.Count)/$($terminalPass.Count).")
}

foreach ($line in $plannerOutput) {
    Write-Output $line
}
