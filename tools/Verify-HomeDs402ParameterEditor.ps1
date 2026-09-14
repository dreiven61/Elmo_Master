param(
    [string]$RepositoryRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'
$failures = [System.Collections.Generic.List[string]]::new()
$passes = 0

function Assert-Contains {
    param([string]$Text, [string]$Pattern, [string]$Description)
    if ($Text -match $Pattern) {
        $script:passes++
        Write-Host "PASS $Description"
    }
    else {
        $script:failures.Add($Description)
        Write-Host "FAIL $Description"
    }
}

function Read-Source {
    param([string]$RelativePath)
    return [IO.File]::ReadAllText((Join-Path $RepositoryRoot $RelativePath))
}

$xaml = Read-Source 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml'
$wpf = Read-Source 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MaintenanceActions.cs'
$models = Read-Source 'LMC_Library/LMC_API_Delivery/src/LmcAdminDs402HomeModels.cs'
$protocol = Read-Source 'LMC_Library/LMC_API_Delivery/src/LmcAdminDs402HomeProtocol.cs'
$outcome = Read-Source 'LMC_Library/LMC_API_Delivery/src/LmcAdminDs402HomeOutcomeModels.cs'
$plc = Read-Source 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st'

foreach ($control in @(
    'ComboDs402HomeMethod',
    'TextDs402HomeOffset',
    'TextDs402HomeVelocity1',
    'TextDs402HomeVelocity2',
    'TextDs402HomeAcceleration')) {
    Assert-Contains $xaml ('x:Name="' + $control + '"') "UI exposes $control"
}

Assert-Contains $wpf '1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14,[\s\S]*33, 34, 37' 'UI lists supported standard methods and method 37'
Assert-Contains $wpf 'HomeVelocity1=' 'recovery journal records HomeVelocity1'
Assert-Contains $wpf 'HomeVelocity2=' 'recovery journal records HomeVelocity2'
Assert-Contains $wpf 'ContainsKey\("HomeVelocity1"\)[\s\S]*ReadParameterInt\(values, "Velocity"\)' 'recovery accepts legacy Velocity journal key'
Assert-Contains $wpf 'ContainsKey\("HomeVelocity2"\)[\s\S]*ReadParameterInt\(values, "DistanceLimit"\)' 'recovery accepts legacy DistanceLimit journal key'

Assert-Contains $models 'HomeVelocity1 \{ get \{ return Velocity; \} \}' 'API aliases Velocity as HomeVelocity1'
Assert-Contains $models 'HomeVelocity2 \{ get \{ return DistanceLimit; \} \}' 'API aliases DistanceLimit as HomeVelocity2'
Assert-Contains $protocol 'homingMethod >= 1 && homingMethod <= 14' 'API accepts method range 1 through 14'
Assert-Contains $protocol 'homingMethod >= 17 && homingMethod <= 30' 'API accepts method range 17 through 30'
Assert-Contains $protocol 'homingMethod == 33[\s\S]*homingMethod == 34' 'API accepts methods 33 and 34'
Assert-Contains $protocol 'currentPositionMethod[\s\S]*velocity != 0 \|\| distanceLimit != 0 \|\| acceleration != 0' 'API enforces non-moving method 37 parameters'
Assert-Contains $protocol 'velocity <= 0 \|\| distanceLimit <= 0 \|\| acceleration <= 0' 'API enforces positive moving parameters'

Assert-Contains $plc 'homeSdoSubIndex := 0;[\s\S]*sdoIndex := 0x607C;' 'PLC writes Home Offset 0x607C:00'
Assert-Contains $plc 'homeSdoSubIndex := 0;[\s\S]*sdoIndex := 0x6098;' 'PLC writes Homing Method 0x6098:00'
Assert-Contains $plc 'sdoIndex := 0x6099;[\s\S]*sdoSubIndex := 1;' 'PLC writes Home Velocity1 0x6099:01'
Assert-Contains $plc 'sdoIndex := 0x6099;[\s\S]*sdoSubIndex := 2;' 'PLC writes Home Velocity2 0x6099:02'
Assert-Contains $plc 'homeSdoSubIndex := 0;[\s\S]*sdoIndex := 0x609A;' 'PLC writes Home Acceleration 0x609A:00'
Assert-Contains $plc 'Ds402HomeState\[105\]' 'PLC retains and restores the saved operating mode'
Assert-Contains $plc 'statusWord and 0x1000' 'PLC observes HomingAttained bit 12'
Assert-Contains $plc 'statusWord and 0x2000' 'PLC rejects HomingError bit 13'

Assert-Contains $outcome 'homingMethod == 37[\s\S]*\? homeOffset[\s\S]*: -\(long\)homeOffset' 'PC outcome uses method-specific expected position'

if ($failures.Count -gt 0) {
    Write-Host "DS402 Home parameter editor verification FAILED: $($failures.Count) failure(s), $passes pass(es)."
    foreach ($failure in $failures) { Write-Host " - $failure" }
    exit 1
}

Write-Host "DS402 Home parameter editor verification PASSED: $passes checks."
