param(
    [Parameter(Mandatory = $true)]
    [string]$RepositoryRoot
)

$ErrorActionPreference = 'Stop'

function Assert-Match {
    param(
        [string]$Text,
        [string]$Pattern,
        [string]$Message
    )

    if ($Text -notmatch $Pattern) {
        throw $Message
    }
}

$root = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$stPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\TCPMotionInterface\TCPMotionInterface.st'
$networkPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\Motion_Network\Motion_Network.lcn'
$protocolPath = Join-Path $root 'LMC_Library\LMC_API_Delivery\src\LmcProtocol.cs'

$st = Get-Content -Raw -LiteralPath $stPath
$network = Get-Content -Raw -LiteralPath $networkPath
$protocol = Get-Content -Raw -LiteralPath $protocolPath

[xml]$network | Out-Null

Assert-Match $st '20\$UINT,\s*6\$UINT,\s*0\$UINT' 'TCPMotionInterface generated client count is not 6.'

$clientEntries = [regex]::Matches(
    $st,
    '\(::TCPMotionInterface\.(LMCAxis|LMCAxis2|LMCAxis3|LMCAxis4|LMCRobot|_StdLib)\.pCh\)\$UINT').Count
if ($clientEntries -ne 6) {
    throw "TCPMotionInterface generated client entry count is $clientEntries, expected 6."
}

foreach ($axisNumber in 1..4) {
    $clientName = if ($axisNumber -eq 1) { 'LMCAxis' } else { "LMCAxis$axisNumber" }
    $linkPattern = [regex]::Escape("TCPMotionInterface1.$clientName") +
        '.*' +
        [regex]::Escape("_LMCAxis$axisNumber.Control")
    Assert-Match $network $linkPattern "Missing $clientName -> _LMCAxis$axisNumber.Control link."
}

if ($st -match '(?m)^\s*0x208[1-4]\s*:') {
    throw 'Legacy 0x2081..0x2084 handler is active.'
}

$upperBitMappings = [regex]::Matches($st, 'AxisCommandState\$UDINT\s+and\s+0xFFFF0000').Count
if ($upperBitMappings -ne 5) {
    throw "32-bit axis error truncation guards=$upperBitMappings, expected 5."
}

Assert-Match $st 'AxisObjectName1\s*:\s*ARRAY \[0\.\.255\] OF CHAR' 'LASAL object-name buffer is not 256 bytes.'
Assert-Match $st 'AxisCommandInputValid\s*:=\s*\(dir = 2\)' 'Shortest-only axis direction validation is missing.'
Assert-Match $st '\(dec = 0\).*\(Exec <> 0\)' 'MoveVelocity deceleration/execute validation is missing.'
Assert-Match $protocol 'WriteInt32\(buffer, HeaderSize, reference\);\s*WriteInt32\(buffer, HeaderSize \+ 4, 1\);' 'C# read-status descriptor payload is missing.'
Assert-Match $protocol 'WriteInt32\(buffer, HeaderSize \+ 64, velocity\);' 'C# group velocity offset is not 64 bytes into payload.'
Assert-Match $protocol 'WriteInt32\(buffer, HeaderSize \+ 92, 1\);' 'C# group execute offset is not 92 bytes into payload.'

Write-Host 'PASS LASAL.StaticContract (6 clients, 4 links, offsets, error guards, legacy block)'
