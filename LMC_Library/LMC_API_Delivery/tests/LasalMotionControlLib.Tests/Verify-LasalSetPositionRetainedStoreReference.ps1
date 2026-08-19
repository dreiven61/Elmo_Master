param(
    [switch]$SelfTestOnly
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 2.0

if (-not $SelfTestOnly) {
    throw (
        'REFERENCE MODEL ONLY: pass -SelfTestOnly explicitly. ' +
        'This script is not a production source verifier.')
}

# This file is an executable, test-only reference model. Its CRC32 routine is
# an IEEE reflected CRC32 oracle used to make the fixtures deterministic. It
# does not claim numerical equivalence with the PLC vendor function
# LDR_CRC32_BufferEx. Production activation still requires a vendor-produced
# golden vector and PLC retained-memory evidence.

$script:AxisCount = 4
$script:RecordCountPerAxis = 4
$script:RecordSize = 84
$script:RecordWordCount = 21
$script:AxisSize = 336
$script:AxisWordCount = 84
$script:StoreSize = 1344
$script:StoreWordCount = 336
$script:RecoveryKeySize = 48
$script:TerminalSnapshotSize = 68
$script:TerminalResponseTotalSize = 92
$script:RecordCrcOffset = 76
$script:RecordCrcLength = 76
$script:CommitMarkerOffset = 80
$script:StoreSchema = 1
$script:SemanticMode = 1
$script:StateArmed = 1
$script:StateSucceeded = 2
$script:StateRejected = 3
$script:TombstoneFlag = 1
$script:CommitMarker = [Convert]::ToUInt32('7D12C0DE', 16)
$script:CrcPolynomial = [Convert]::ToUInt32('EDB88320', 16)
$script:UInt32Max = [uint32]::MaxValue
$script:DetailNotFound = 19
$script:DetailIndeterminate = 20
$script:DetailStoreCorrupt = 21
$script:DetailKeyMismatch = 22
$script:DetailSlotOccupied = 23
$script:DetailStorageUnavailable = 24
$script:FixtureCount = 0
$script:AssertionCount = 0

function Assert-ReferenceTrue {
    param(
        [bool]$Condition,
        [string]$Message
    )

    $script:AssertionCount++
    if (-not $Condition) {
        throw $Message
    }
}

function Assert-ReferenceEqual {
    param(
        $Expected,
        $Actual,
        [string]$Message
    )

    $script:AssertionCount++
    if ($Expected -ne $Actual) {
        throw (
            $Message + ' Expected=' + [string]$Expected +
            ', Actual=' + [string]$Actual + '.')
    }
}

function Test-ReferenceBytesEqual {
    param(
        [byte[]]$Left,
        [byte[]]$Right
    )

    if ($null -eq $Left -or $null -eq $Right -or
        $Left.Length -ne $Right.Length) {
        return $false
    }
    for ($index = 0; $index -lt $Left.Length; $index++) {
        if ($Left[$index] -ne $Right[$index]) {
            return $false
        }
    }
    return $true
}

function Assert-ReferenceBytesEqual {
    param(
        [byte[]]$Expected,
        [byte[]]$Actual,
        [string]$Message
    )

    $script:AssertionCount++
    if (-not (Test-ReferenceBytesEqual -Left $Expected -Right $Actual)) {
        throw $Message
    }
}

function Assert-ReferenceThrows {
    param(
        [string]$ExpectedType,
        [scriptblock]$Body,
        [string]$Message
    )

    $script:AssertionCount++
    try {
        $null = & $Body
    }
    catch {
        $exception = $_.Exception
        while ($null -ne $exception -and
            $exception.GetType().FullName -cne $ExpectedType -and
            $null -ne $exception.InnerException) {
            $exception = $exception.InnerException
        }
        if ($null -ne $exception -and
            $exception.GetType().FullName -ceq $ExpectedType) {
            return
        }
        throw (
            $Message + ' Expected exception=' + $ExpectedType +
            ', Actual=' + $_.Exception.GetType().FullName + '.')
    }
    throw $Message + ' Expected exception was not thrown: ' + $ExpectedType
}

function Invoke-ReferenceFixture {
    param(
        [string]$Name,
        [scriptblock]$Body
    )

    try {
        $null = & $Body
    }
    catch {
        throw "SetPosition retained-store fixture '$Name' failed: $($_.Exception.Message)"
    }
    $script:FixtureCount++
}

function Copy-ReferenceBytes {
    param([byte[]]$Bytes)

    $copy = New-Object byte[] $Bytes.Length
    [Array]::Copy($Bytes, 0, $copy, 0, $Bytes.Length)
    return ,$copy
}

function Convert-ReferenceHexToBytes {
    param([string]$Hex)

    if ($null -eq $Hex -or ($Hex.Length % 2) -ne 0 -or
        $Hex -notmatch '^[0-9A-Fa-f]+$') {
        throw [ArgumentException]::new(
            'Hex must contain an even number of hexadecimal characters.')
    }
    $bytes = New-Object byte[] ($Hex.Length / 2)
    for ($index = 0; $index -lt $bytes.Length; $index++) {
        $bytes[$index] = [Convert]::ToByte(
            $Hex.Substring($index * 2, 2), 16)
    }
    return ,$bytes
}

function Get-ReferenceRegion {
    param(
        [byte[]]$Bytes,
        [int]$Offset,
        [int]$Length
    )

    if ($Offset -lt 0 -or $Length -lt 0 -or
        $Offset + $Length -gt $Bytes.Length) {
        throw [ArgumentOutOfRangeException]::new('Offset')
    }
    $copy = New-Object byte[] $Length
    [Array]::Copy($Bytes, $Offset, $copy, 0, $Length)
    return ,$copy
}

function Set-ReferenceUInt16LE {
    param(
        [byte[]]$Bytes,
        [int]$Offset,
        [uint16]$Value
    )

    $Bytes[$Offset] = [byte]($Value -band 0xFF)
    $Bytes[$Offset + 1] = [byte](($Value -shr 8) -band 0xFF)
}

function Get-ReferenceUInt16LE {
    param(
        [byte[]]$Bytes,
        [int]$Offset
    )

    return [uint16](
        [uint16]$Bytes[$Offset] -bor
        ([uint16]$Bytes[$Offset + 1] -shl 8))
}

function Set-ReferenceUInt32LE {
    param(
        [byte[]]$Bytes,
        [int]$Offset,
        [uint32]$Value
    )

    $Bytes[$Offset] = [byte]($Value -band 0xFF)
    $Bytes[$Offset + 1] = [byte](($Value -shr 8) -band 0xFF)
    $Bytes[$Offset + 2] = [byte](($Value -shr 16) -band 0xFF)
    $Bytes[$Offset + 3] = [byte](($Value -shr 24) -band 0xFF)
}

function Get-ReferenceUInt32LE {
    param(
        [byte[]]$Bytes,
        [int]$Offset
    )

    [uint64]$value = [uint64]$Bytes[$Offset]
    $value = $value -bor ([uint64]$Bytes[$Offset + 1] -shl 8)
    $value = $value -bor ([uint64]$Bytes[$Offset + 2] -shl 16)
    $value = $value -bor ([uint64]$Bytes[$Offset + 3] -shl 24)
    return [uint32]$value
}

function Set-ReferenceInt16LE {
    param(
        [byte[]]$Bytes,
        [int]$Offset,
        [int16]$Value
    )

    $encoded = [BitConverter]::GetBytes($Value)
    if (-not [BitConverter]::IsLittleEndian) {
        [Array]::Reverse($encoded)
    }
    [Array]::Copy($encoded, 0, $Bytes, $Offset, 2)
}

function Get-ReferenceInt16LE {
    param(
        [byte[]]$Bytes,
        [int]$Offset
    )

    $encoded = Get-ReferenceRegion -Bytes $Bytes -Offset $Offset -Length 2
    if (-not [BitConverter]::IsLittleEndian) {
        [Array]::Reverse($encoded)
    }
    return [BitConverter]::ToInt16($encoded, 0)
}

function Set-ReferenceInt32LE {
    param(
        [byte[]]$Bytes,
        [int]$Offset,
        [int32]$Value
    )

    $encoded = [BitConverter]::GetBytes($Value)
    if (-not [BitConverter]::IsLittleEndian) {
        [Array]::Reverse($encoded)
    }
    [Array]::Copy($encoded, 0, $Bytes, $Offset, 4)
}

function Get-ReferenceInt32LE {
    param(
        [byte[]]$Bytes,
        [int]$Offset
    )

    $encoded = Get-ReferenceRegion -Bytes $Bytes -Offset $Offset -Length 4
    if (-not [BitConverter]::IsLittleEndian) {
        [Array]::Reverse($encoded)
    }
    return [BitConverter]::ToInt32($encoded, 0)
}

function Get-TestOnlyOracleCrc32 {
    param(
        [byte[]]$Bytes,
        [int]$Offset = 0,
        [int]$Length = -1,
        [uint32]$Seed = 0
    )

    if ($Length -eq -1) {
        $Length = $Bytes.Length - $Offset
    }
    if ($Offset -lt 0 -or $Length -lt 0 -or
        $Offset + $Length -gt $Bytes.Length) {
        throw [ArgumentOutOfRangeException]::new('Length')
    }

    [uint32]$crc = $Seed -bxor $script:UInt32Max
    for ($index = 0; $index -lt $Length; $index++) {
        $crc = [uint32]($crc -bxor [uint32]$Bytes[$Offset + $index])
        for ($bit = 0; $bit -lt 8; $bit++) {
            if (($crc -band 1) -ne 0) {
                $crc = [uint32](
                    [uint32]($crc -shr 1) -bxor $script:CrcPolynomial)
            }
            else {
                $crc = [uint32]($crc -shr 1)
            }
        }
    }
    return [uint32]($crc -bxor $script:UInt32Max)
}

function New-ReferenceRecoveryKey {
    param(
        [uint16]$AxisReference = 1,
        [uint32]$DiagnosticsBuild = 0x01020304,
        [uint32]$DiagnosticsBootId = 0x11223344,
        [uint32]$MapRevision = 0x55667788,
        [uint32]$OriginalRequestId = 0x10203040,
        [uint32]$Intent0 = 0x11111111,
        [uint32]$Intent1 = 0x22222222,
        [uint32]$Intent2 = 0x33333333,
        [uint32]$Intent3 = 0x44444444,
        [int32]$TargetPosition = 123456,
        [int32]$ExpectedActualPosition = -654321,
        [uint16]$SemanticMode = 1,
        [uint16]$SchemaVersion = 1
    )

    return [pscustomobject][ordered]@{
        SchemaVersion = $SchemaVersion
        DiagnosticsBuild = $DiagnosticsBuild
        DiagnosticsBootId = $DiagnosticsBootId
        MapRevision = $MapRevision
        OriginalRequestId = $OriginalRequestId
        Intent0 = $Intent0
        Intent1 = $Intent1
        Intent2 = $Intent2
        Intent3 = $Intent3
        AxisReference = $AxisReference
        TargetPosition = $TargetPosition
        ExpectedActualPosition = $ExpectedActualPosition
        SemanticMode = $SemanticMode
    }
}

function ConvertTo-ReferenceRecoveryKeyBytes {
    param(
        $Key,
        [uint16]$Reserved = 0
    )

    if ($null -eq $Key) {
        throw [ArgumentNullException]::new('Key')
    }
    $bytes = New-Object byte[] $script:RecoveryKeySize
    Set-ReferenceUInt16LE $bytes 0 $Key.SchemaVersion
    Set-ReferenceUInt16LE $bytes 2 $Key.SemanticMode
    Set-ReferenceUInt32LE $bytes 4 $Key.DiagnosticsBuild
    Set-ReferenceUInt32LE $bytes 8 $Key.DiagnosticsBootId
    Set-ReferenceUInt32LE $bytes 12 $Key.MapRevision
    Set-ReferenceUInt32LE $bytes 16 $Key.OriginalRequestId
    Set-ReferenceUInt32LE $bytes 20 $Key.Intent0
    Set-ReferenceUInt32LE $bytes 24 $Key.Intent1
    Set-ReferenceUInt32LE $bytes 28 $Key.Intent2
    Set-ReferenceUInt32LE $bytes 32 $Key.Intent3
    Set-ReferenceUInt16LE $bytes 36 $Key.AxisReference
    Set-ReferenceUInt16LE $bytes 38 $Reserved
    Set-ReferenceInt32LE $bytes 40 $Key.TargetPosition
    Set-ReferenceInt32LE $bytes 44 $Key.ExpectedActualPosition
    return ,$bytes
}

function New-ReferenceRecoveryKeyBoundaryResult {
    param(
        [bool]$IsValid,
        [string]$Reason,
        $Key,
        [byte[]]$Bytes
    )

    return [pscustomobject][ordered]@{
        IsValid = $IsValid
        Reason = $Reason
        Key = $Key
        Bytes = $Bytes
    }
}

function Get-ReferenceRecoveryKeyBoundary {
    param(
        $Key,
        [byte[]]$KeyBytes,
        [int]$KeySize = 48
    )

    if ($null -eq $KeyBytes) {
        if ($null -eq $Key) {
            return New-ReferenceRecoveryKeyBoundaryResult `
                -IsValid $false -Reason 'NilKey' -Key $null -Bytes $null
        }
        $KeyBytes = ConvertTo-ReferenceRecoveryKeyBytes -Key $Key
    }
    if ($KeySize -ne $script:RecoveryKeySize -or
        $KeyBytes.Length -ne $script:RecoveryKeySize) {
        return New-ReferenceRecoveryKeyBoundaryResult `
            -IsValid $false -Reason 'KeySize' -Key $null `
            -Bytes (Copy-ReferenceBytes $KeyBytes)
    }
    if ((Get-ReferenceUInt16LE $KeyBytes 38) -ne 0) {
        return New-ReferenceRecoveryKeyBoundaryResult `
            -IsValid $false -Reason 'Reserved' -Key $null `
            -Bytes (Copy-ReferenceBytes $KeyBytes)
    }

    $parsed = New-ReferenceRecoveryKey `
        -SchemaVersion (Get-ReferenceUInt16LE $KeyBytes 0) `
        -SemanticMode (Get-ReferenceUInt16LE $KeyBytes 2) `
        -DiagnosticsBuild (Get-ReferenceUInt32LE $KeyBytes 4) `
        -DiagnosticsBootId (Get-ReferenceUInt32LE $KeyBytes 8) `
        -MapRevision (Get-ReferenceUInt32LE $KeyBytes 12) `
        -OriginalRequestId (Get-ReferenceUInt32LE $KeyBytes 16) `
        -Intent0 (Get-ReferenceUInt32LE $KeyBytes 20) `
        -Intent1 (Get-ReferenceUInt32LE $KeyBytes 24) `
        -Intent2 (Get-ReferenceUInt32LE $KeyBytes 28) `
        -Intent3 (Get-ReferenceUInt32LE $KeyBytes 32) `
        -AxisReference (Get-ReferenceUInt16LE $KeyBytes 36) `
        -TargetPosition (Get-ReferenceInt32LE $KeyBytes 40) `
        -ExpectedActualPosition (Get-ReferenceInt32LE $KeyBytes 44)
    if ($null -ne $Key -and
        -not (Test-ReferenceRecoveryKeysEqual $Key $parsed)) {
        return New-ReferenceRecoveryKeyBoundaryResult `
            -IsValid $false -Reason 'KeyBytesDrift' -Key $null `
            -Bytes (Copy-ReferenceBytes $KeyBytes)
    }
    return New-ReferenceRecoveryKeyBoundaryResult `
        -IsValid $true -Reason 'Valid' -Key $parsed `
        -Bytes (Copy-ReferenceBytes $KeyBytes)
}

function Copy-ReferenceRecoveryKey {
    param($Key)

    return New-ReferenceRecoveryKey `
        -AxisReference $Key.AxisReference `
        -DiagnosticsBuild $Key.DiagnosticsBuild `
        -DiagnosticsBootId $Key.DiagnosticsBootId `
        -MapRevision $Key.MapRevision `
        -OriginalRequestId $Key.OriginalRequestId `
        -Intent0 $Key.Intent0 `
        -Intent1 $Key.Intent1 `
        -Intent2 $Key.Intent2 `
        -Intent3 $Key.Intent3 `
        -TargetPosition $Key.TargetPosition `
        -ExpectedActualPosition $Key.ExpectedActualPosition `
        -SemanticMode $Key.SemanticMode `
        -SchemaVersion $Key.SchemaVersion
}

function Test-ReferenceRecoveryKeysEqual {
    param(
        $Left,
        $Right
    )

    if ($null -eq $Left -or $null -eq $Right) {
        return $false
    }
    return $Left.SchemaVersion -eq $Right.SchemaVersion -and
        $Left.SemanticMode -eq $Right.SemanticMode -and
        $Left.DiagnosticsBuild -eq $Right.DiagnosticsBuild -and
        $Left.DiagnosticsBootId -eq $Right.DiagnosticsBootId -and
        $Left.MapRevision -eq $Right.MapRevision -and
        $Left.OriginalRequestId -eq $Right.OriginalRequestId -and
        $Left.Intent0 -eq $Right.Intent0 -and
        $Left.Intent1 -eq $Right.Intent1 -and
        $Left.Intent2 -eq $Right.Intent2 -and
        $Left.Intent3 -eq $Right.Intent3 -and
        $Left.AxisReference -eq $Right.AxisReference -and
        $Left.TargetPosition -eq $Right.TargetPosition -and
        $Left.ExpectedActualPosition -eq $Right.ExpectedActualPosition
}

function New-ReferenceSetPositionTransaction {
    return [pscustomobject][ordered]@{
        Active = $false
        AxisReference = 0
        Key = $null
        RecordGeneration = [uint32]0
        IntentStoreGeneration = [uint32]0
        TerminalTargetSlot = -1
        ReservedTerminalTargetSlot = -1
        TerminalStoreGeneration = [uint32]0
        ReservedTerminalStoreGeneration = [uint32]0
        TerminalTargetBeforeBytes = $null
    }
}

function Clear-ReferenceSetPositionTransaction {
    param($Transaction)

    $Transaction.Active = $false
    $Transaction.AxisReference = 0
    $Transaction.Key = $null
    $Transaction.RecordGeneration = [uint32]0
    $Transaction.IntentStoreGeneration = [uint32]0
    $Transaction.TerminalTargetSlot = -1
    $Transaction.ReservedTerminalTargetSlot = -1
    $Transaction.TerminalStoreGeneration = [uint32]0
    $Transaction.ReservedTerminalStoreGeneration = [uint32]0
    $Transaction.TerminalTargetBeforeBytes = $null
}

function Get-ReferenceRecordOffset {
    param(
        [int]$AxisReference,
        [int]$Slot
    )

    if ($AxisReference -lt 1 -or $AxisReference -gt $script:AxisCount) {
        throw [ArgumentOutOfRangeException]::new('AxisReference')
    }
    if ($Slot -lt 0 -or $Slot -ge $script:RecordCountPerAxis) {
        throw [ArgumentOutOfRangeException]::new('Slot')
    }
    return (($AxisReference - 1) * $script:AxisSize) +
        ($Slot * $script:RecordSize)
}

function Get-ReferenceRecordWordOffset {
    param(
        [int]$AxisReference,
        [int]$Slot
    )

    return [int]((Get-ReferenceRecordOffset `
        -AxisReference $AxisReference -Slot $Slot) / 4)
}

function New-ReferenceStore {
    return ,(New-Object byte[] $script:StoreSize)
}

function Update-ReferenceRecordOracleCrc {
    param([byte[]]$RecordBytes)

    if ($RecordBytes.Length -ne $script:RecordSize) {
        throw [ArgumentException]::new('RecordBytes must contain 84 bytes.')
    }
    $crc = Get-TestOnlyOracleCrc32 `
        -Bytes $RecordBytes `
        -Offset 0 `
        -Length $script:RecordCrcLength `
        -Seed 0
    Set-ReferenceUInt32LE `
        -Bytes $RecordBytes `
        -Offset $script:RecordCrcOffset `
        -Value $crc
}

function New-ReferenceRecordBytes {
    param(
        $Key,
        [uint32]$StoreGeneration,
        [uint16]$RecordState,
        [uint32]$RecordGeneration,
        [int32]$AppliedPosition = 0,
        [uint16]$OriginalCommandStatus = 0,
        [int16]$OriginalErrorId = 0,
        [uint32]$OriginalDetailCode = 0,
        [uint32]$NativeCommandState = 0,
        [switch]$Tombstone,
        [uint32]$CommitMarker = 0x7D12C0DE
    )

    $bytes = New-Object byte[] $script:RecordSize
    Set-ReferenceUInt16LE $bytes 0 $Key.SchemaVersion
    Set-ReferenceUInt16LE $bytes 2 $(if ($Tombstone) { 1 } else { 0 })
    Set-ReferenceUInt32LE $bytes 4 $StoreGeneration
    Set-ReferenceUInt16LE $bytes 8 $RecordState
    Set-ReferenceUInt16LE $bytes 10 $Key.SemanticMode
    Set-ReferenceUInt32LE $bytes 12 $Key.DiagnosticsBuild
    Set-ReferenceUInt32LE $bytes 16 $Key.DiagnosticsBootId
    Set-ReferenceUInt32LE $bytes 20 $Key.MapRevision
    Set-ReferenceUInt32LE $bytes 24 $Key.OriginalRequestId
    Set-ReferenceUInt32LE $bytes 28 $Key.Intent0
    Set-ReferenceUInt32LE $bytes 32 $Key.Intent1
    Set-ReferenceUInt32LE $bytes 36 $Key.Intent2
    Set-ReferenceUInt32LE $bytes 40 $Key.Intent3
    Set-ReferenceUInt16LE $bytes 44 $Key.AxisReference
    Set-ReferenceUInt16LE $bytes 46 0
    Set-ReferenceInt32LE $bytes 48 $Key.TargetPosition
    Set-ReferenceInt32LE $bytes 52 $Key.ExpectedActualPosition
    Set-ReferenceInt32LE $bytes 56 $AppliedPosition
    Set-ReferenceUInt16LE $bytes 60 $OriginalCommandStatus
    Set-ReferenceInt16LE $bytes 62 $OriginalErrorId
    Set-ReferenceUInt32LE $bytes 64 $OriginalDetailCode
    Set-ReferenceUInt32LE $bytes 68 $NativeCommandState
    Set-ReferenceUInt32LE $bytes 72 $RecordGeneration
    Update-ReferenceRecordOracleCrc -RecordBytes $bytes
    Set-ReferenceUInt32LE $bytes $script:CommitMarkerOffset $CommitMarker
    return ,$bytes
}

function New-ReferenceArmedRecordBytes {
    param(
        $Key,
        [uint32]$StoreGeneration,
        [uint32]$RecordGeneration
    )

    return ,(New-ReferenceRecordBytes `
        -Key $Key `
        -StoreGeneration $StoreGeneration `
        -RecordState $script:StateArmed `
        -RecordGeneration $RecordGeneration)
}

function New-ReferenceSucceededRecordBytes {
    param(
        $Key,
        [uint32]$StoreGeneration,
        [uint32]$RecordGeneration,
        [switch]$Tombstone
    )

    return ,(New-ReferenceRecordBytes `
        -Key $Key `
        -StoreGeneration $StoreGeneration `
        -RecordState $script:StateSucceeded `
        -RecordGeneration $RecordGeneration `
        -AppliedPosition $Key.TargetPosition `
        -Tombstone:$Tombstone)
}

function New-ReferenceRejectedRecordBytes {
    param(
        $Key,
        [uint32]$StoreGeneration,
        [uint32]$RecordGeneration,
        [uint32]$DetailCode = 10,
        [int16]$ErrorId = -31000,
        [uint32]$NativeCommandState = 0,
        [switch]$Tombstone
    )

    return ,(New-ReferenceRecordBytes `
        -Key $Key `
        -StoreGeneration $StoreGeneration `
        -RecordState $script:StateRejected `
        -RecordGeneration $RecordGeneration `
        -OriginalCommandStatus 1 `
        -OriginalErrorId $ErrorId `
        -OriginalDetailCode $DetailCode `
        -NativeCommandState $NativeCommandState `
        -Tombstone:$Tombstone)
}

function New-ReferenceReadResult {
    param(
        [string]$Classification,
        [string]$Reason,
        $Record,
        [int]$Slot,
        [byte[]]$Bytes
    )

    return [pscustomobject][ordered]@{
        Classification = $Classification
        Reason = $Reason
        Record = $Record
        Slot = $Slot
        Bytes = $Bytes
    }
}

function Test-ReferenceAllZero {
    param([byte[]]$Bytes)

    foreach ($value in $Bytes) {
        if ($value -ne 0) {
            return $false
        }
    }
    return $true
}

function Read-ReferenceRecord {
    param(
        [byte[]]$Bytes,
        [int]$Offset = 0,
        [int]$Slot = -1
    )

    $recordBytes = Get-ReferenceRegion `
        -Bytes $Bytes -Offset $Offset -Length $script:RecordSize
    $marker = Get-ReferenceUInt32LE `
        -Bytes $recordBytes -Offset $script:CommitMarkerOffset
    if (Test-ReferenceAllZero -Bytes $recordBytes) {
        return New-ReferenceReadResult `
            -Classification 'Blank' -Reason 'AllZero' -Record $null `
            -Slot $Slot -Bytes $recordBytes
    }
    if ($marker -eq 0) {
        return New-ReferenceReadResult `
            -Classification 'Incomplete' -Reason 'MarkerZero' -Record $null `
            -Slot $Slot -Bytes $recordBytes
    }
    if ($marker -ne $script:CommitMarker) {
        return New-ReferenceReadResult `
            -Classification 'Corrupt' -Reason 'UnknownMarker' -Record $null `
            -Slot $Slot -Bytes $recordBytes
    }

    $record = [pscustomobject][ordered]@{
        StoreSchema = Get-ReferenceUInt16LE $recordBytes 0
        StoreFlags = Get-ReferenceUInt16LE $recordBytes 2
        StoreGeneration = Get-ReferenceUInt32LE $recordBytes 4
        RecordState = Get-ReferenceUInt16LE $recordBytes 8
        SemanticMode = Get-ReferenceUInt16LE $recordBytes 10
        DiagnosticsBuild = Get-ReferenceUInt32LE $recordBytes 12
        DiagnosticsBootId = Get-ReferenceUInt32LE $recordBytes 16
        MapRevision = Get-ReferenceUInt32LE $recordBytes 20
        OriginalRequestId = Get-ReferenceUInt32LE $recordBytes 24
        Intent0 = Get-ReferenceUInt32LE $recordBytes 28
        Intent1 = Get-ReferenceUInt32LE $recordBytes 32
        Intent2 = Get-ReferenceUInt32LE $recordBytes 36
        Intent3 = Get-ReferenceUInt32LE $recordBytes 40
        AxisReference = Get-ReferenceUInt16LE $recordBytes 44
        Reserved = Get-ReferenceUInt16LE $recordBytes 46
        TargetPosition = Get-ReferenceInt32LE $recordBytes 48
        ExpectedActualPosition = Get-ReferenceInt32LE $recordBytes 52
        AppliedPosition = Get-ReferenceInt32LE $recordBytes 56
        OriginalCommandStatus = Get-ReferenceUInt16LE $recordBytes 60
        OriginalErrorId = Get-ReferenceInt16LE $recordBytes 62
        OriginalDetailCode = Get-ReferenceUInt32LE $recordBytes 64
        NativeCommandState = Get-ReferenceUInt32LE $recordBytes 68
        RecordGeneration = Get-ReferenceUInt32LE $recordBytes 72
        RecordCrc32 = Get-ReferenceUInt32LE $recordBytes 76
        CommitMarker = $marker
        IsTombstone = ((Get-ReferenceUInt16LE $recordBytes 2) -band 1) -ne 0
        Slot = $Slot
        Bytes = $recordBytes
    }

    $computedCrc = Get-TestOnlyOracleCrc32 `
        -Bytes $recordBytes -Offset 0 -Length 76 -Seed 0
    $reason = $null
    if ($record.RecordCrc32 -ne $computedCrc) {
        $reason = 'CrcMismatch'
    }
    elseif ($record.StoreSchema -ne $script:StoreSchema) {
        $reason = 'StoreSchema'
    }
    elseif (($record.StoreFlags -band 0xFFFE) -ne 0) {
        $reason = 'StoreFlags'
    }
    elseif ($record.StoreGeneration -eq 0) {
        $reason = 'StoreGeneration'
    }
    elseif ($record.RecordState -ne $script:StateArmed -and
        $record.RecordState -ne $script:StateSucceeded -and
        $record.RecordState -ne $script:StateRejected) {
        $reason = 'RecordState'
    }
    elseif ($record.SemanticMode -ne $script:SemanticMode) {
        $reason = 'SemanticMode'
    }
    elseif ($record.DiagnosticsBuild -eq 0 -or
        $record.DiagnosticsBootId -eq 0 -or
        $record.MapRevision -eq 0 -or
        $record.OriginalRequestId -eq 0) {
        $reason = 'RecoveryIdentity'
    }
    elseif ($record.Intent0 -eq 0 -and $record.Intent1 -eq 0 -and
        $record.Intent2 -eq 0 -and $record.Intent3 -eq 0) {
        $reason = 'ClientIntent'
    }
    elseif ($record.AxisReference -lt 1 -or
        $record.AxisReference -gt $script:AxisCount) {
        $reason = 'AxisReference'
    }
    elseif ($record.Reserved -ne 0) {
        $reason = 'Reserved'
    }
    elseif ($record.RecordGeneration -eq 0) {
        $reason = 'RecordGeneration'
    }
    elseif ($record.RecordState -eq $script:StateArmed -and
        ($record.StoreFlags -ne 0 -or
         $record.AppliedPosition -ne 0 -or
         $record.OriginalCommandStatus -ne 0 -or
         $record.OriginalErrorId -ne 0 -or
         $record.OriginalDetailCode -ne 0 -or
         $record.NativeCommandState -ne 0)) {
        $reason = 'ArmedPayload'
    }
    elseif ($record.RecordState -eq $script:StateSucceeded -and
        ($record.AppliedPosition -ne $record.TargetPosition -or
         $record.OriginalCommandStatus -ne 0 -or
         $record.OriginalErrorId -ne 0 -or
         $record.OriginalDetailCode -ne 0 -or
         $record.NativeCommandState -ne 0)) {
        $reason = 'SucceededPayload'
    }
    elseif ($record.RecordState -eq $script:StateRejected -and
        ($record.AppliedPosition -ne 0 -or
         $record.OriginalCommandStatus -ne 1 -or
         ($record.OriginalDetailCode -eq 11 -and
            ($record.OriginalErrorId -ne -6 -or
             $record.NativeCommandState -eq 0)) -or
         ($record.OriginalDetailCode -ne 11 -and
            (($record.OriginalDetailCode -lt 10) -or
             ($record.OriginalDetailCode -gt 15) -or
             $record.OriginalErrorId -ne -31000 -or
             $record.NativeCommandState -ne 0)))) {
        $reason = 'RejectedPayload'
    }
    elseif ($record.IsTombstone -and
        $record.RecordState -eq $script:StateArmed) {
        $reason = 'ArmedTombstone'
    }

    if ($null -ne $reason) {
        return New-ReferenceReadResult `
            -Classification 'Corrupt' -Reason $reason -Record $record `
            -Slot $Slot -Bytes $recordBytes
    }
    return New-ReferenceReadResult `
        -Classification 'Valid' -Reason 'Valid' -Record $record `
        -Slot $Slot -Bytes $recordBytes
}

function Set-ReferenceStoreRecordDirect {
    param(
        [byte[]]$Store,
        [int]$AxisReference,
        [int]$Slot,
        [byte[]]$RecordBytes
    )

    if ($Store.Length -ne $script:StoreSize -or
        $RecordBytes.Length -ne $script:RecordSize) {
        throw [ArgumentException]::new('Store or record size is invalid.')
    }
    $offset = Get-ReferenceRecordOffset `
        -AxisReference $AxisReference -Slot $Slot
    [Array]::Copy($RecordBytes, 0, $Store, $offset, $script:RecordSize)
}

function Test-ReferenceRecordMatchesKey {
    param(
        $Record,
        $Key
    )

    return $null -ne $Record -and $null -ne $Key -and
        $Record.StoreSchema -eq $Key.SchemaVersion -and
        $Record.SemanticMode -eq $Key.SemanticMode -and
        $Record.DiagnosticsBuild -eq $Key.DiagnosticsBuild -and
        $Record.DiagnosticsBootId -eq $Key.DiagnosticsBootId -and
        $Record.MapRevision -eq $Key.MapRevision -and
        $Record.OriginalRequestId -eq $Key.OriginalRequestId -and
        $Record.Intent0 -eq $Key.Intent0 -and
        $Record.Intent1 -eq $Key.Intent1 -and
        $Record.Intent2 -eq $Key.Intent2 -and
        $Record.Intent3 -eq $Key.Intent3 -and
        $Record.AxisReference -eq $Key.AxisReference -and
        $Record.TargetPosition -eq $Key.TargetPosition -and
        $Record.ExpectedActualPosition -eq $Key.ExpectedActualPosition
}

function Test-ReferenceRecordsHaveSameKey {
    param(
        $Left,
        $Right
    )

    if ($null -eq $Left -or $null -eq $Right) {
        return $false
    }
    $key = [pscustomobject]@{
        SchemaVersion = $Right.StoreSchema
        SemanticMode = $Right.SemanticMode
        DiagnosticsBuild = $Right.DiagnosticsBuild
        DiagnosticsBootId = $Right.DiagnosticsBootId
        MapRevision = $Right.MapRevision
        OriginalRequestId = $Right.OriginalRequestId
        Intent0 = $Right.Intent0
        Intent1 = $Right.Intent1
        Intent2 = $Right.Intent2
        Intent3 = $Right.Intent3
        AxisReference = $Right.AxisReference
        TargetPosition = $Right.TargetPosition
        ExpectedActualPosition = $Right.ExpectedActualPosition
    }
    return Test-ReferenceRecordMatchesKey -Record $Left -Key $key
}

function Test-ReferenceTerminalSnapshotsEqual {
    param(
        $Left,
        $Right
    )

    return $Left.RecordState -eq $Right.RecordState -and
        $Left.AppliedPosition -eq $Right.AppliedPosition -and
        $Left.OriginalCommandStatus -eq $Right.OriginalCommandStatus -and
        $Left.OriginalErrorId -eq $Right.OriginalErrorId -and
        $Left.OriginalDetailCode -eq $Right.OriginalDetailCode -and
        $Left.NativeCommandState -eq $Right.NativeCommandState -and
        $Left.RecordGeneration -eq $Right.RecordGeneration
}

function Get-ReferenceAxisScan {
    param(
        [byte[]]$Store,
        [int]$AxisReference
    )

    if ($Store.Length -ne $script:StoreSize) {
        throw [ArgumentException]::new('Store must contain exactly 1344 bytes.')
    }
    $entries = @()
    for ($slot = 0; $slot -lt $script:RecordCountPerAxis; $slot++) {
        $offset = Get-ReferenceRecordOffset `
            -AxisReference $AxisReference -Slot $slot
        $entry = Read-ReferenceRecord `
            -Bytes $Store -Offset $offset -Slot $slot
        if ($entry.Classification -eq 'Valid' -and
            $entry.Record.AxisReference -ne $AxisReference) {
            $entry = New-ReferenceReadResult `
                -Classification 'Corrupt' `
                -Reason 'AxisSlotMismatch' `
                -Record $entry.Record `
                -Slot $slot `
                -Bytes $entry.Bytes
        }
        elseif ($entry.Classification -eq 'Valid' -and $slot -eq 0 -and
            ($entry.Record.RecordState -ne $script:StateArmed -or
             $entry.Record.StoreFlags -ne 0)) {
            $entry = New-ReferenceReadResult `
                -Classification 'Corrupt' `
                -Reason 'IntentSlotRole' `
                -Record $entry.Record `
                -Slot $slot `
                -Bytes $entry.Bytes
        }
        elseif ($entry.Classification -eq 'Valid' -and $slot -ne 0 -and
            ($entry.Record.RecordState -ne $script:StateSucceeded -and
             $entry.Record.RecordState -ne $script:StateRejected)) {
            $entry = New-ReferenceReadResult `
                -Classification 'Corrupt' `
                -Reason 'TerminalSlotRole' `
                -Record $entry.Record `
                -Slot $slot `
                -Bytes $entry.Bytes
        }
        $entries += $entry
    }

    $valid = @($entries | Where-Object {
            $_.Classification -eq 'Valid'
        } | ForEach-Object { $_.Record })
    $corruptReasons = @($entries | Where-Object {
            $_.Classification -eq 'Corrupt'
        } | ForEach-Object { $_.Reason })

    for ($leftIndex = 0; $leftIndex -lt $valid.Count; $leftIndex++) {
        for ($rightIndex = $leftIndex + 1;
             $rightIndex -lt $valid.Count;
             $rightIndex++) {
            $left = $valid[$leftIndex]
            $right = $valid[$rightIndex]
            # A committed StoreGeneration is a per-axis physical commit ID.
            # It may never be reused, even when the bytes happen to match.
            if ($left.StoreGeneration -eq $right.StoreGeneration) {
                $corruptReasons += 'DuplicateCommittedStoreGeneration'
            }
            $sameKey = Test-ReferenceRecordsHaveSameKey $left $right
            if ($left.RecordGeneration -eq $right.RecordGeneration -and
                -not $sameKey) {
                $corruptReasons += 'DifferentKeyRecordGenerationReuse'
            }
            if ($sameKey -and
                $left.RecordGeneration -ne $right.RecordGeneration) {
                $corruptReasons += 'ExactKeyRecordGenerationMismatch'
            }
            $leftTerminal = $left.RecordState -eq $script:StateSucceeded -or
                $left.RecordState -eq $script:StateRejected
            $rightTerminal = $right.RecordState -eq $script:StateSucceeded -or
                $right.RecordState -eq $script:StateRejected
            if ($leftTerminal -and $rightTerminal -and
                $sameKey -and
                $left.RecordGeneration -eq $right.RecordGeneration -and
                -not (Test-ReferenceTerminalSnapshotsEqual $left $right)) {
                $corruptReasons += 'DivergentTerminalSnapshot'
            }
            if ($leftTerminal -and $rightTerminal -and
                $sameKey -and
                $left.RecordGeneration -eq $right.RecordGeneration -and
                $left.IsTombstone -ne $right.IsTombstone) {
                $tombstone = if ($left.IsTombstone) { $left } else { $right }
                $terminal = if ($left.IsTombstone) { $right } else { $left }
                if ($tombstone.StoreGeneration -le
                    $terminal.StoreGeneration) {
                    $corruptReasons += 'TombstoneNotAfterMatchingTerminal'
                }
            }
            if ($sameKey -and
                $left.RecordGeneration -eq $right.RecordGeneration) {
                if ($left.RecordState -eq $script:StateArmed -and
                    $rightTerminal -and
                    $right.StoreGeneration -le $left.StoreGeneration) {
                    $corruptReasons += 'TerminalNotAfterMatchingArmed'
                }
                elseif ($right.RecordState -eq $script:StateArmed -and
                    $leftTerminal -and
                    $left.StoreGeneration -le $right.StoreGeneration) {
                    $corruptReasons += 'TerminalNotAfterMatchingArmed'
                }
            }
        }
    }

    $unsupersededTerminals = @($valid | Where-Object {
            -not $_.IsTombstone -and
            ($_.RecordState -eq $script:StateSucceeded -or
             $_.RecordState -eq $script:StateRejected) -and
            -not (Test-ReferenceRecordIsRetiredShadow `
                -Record $_ -ValidRecords $valid)
        })
    if ($unsupersededTerminals.Count -gt 1) {
        $corruptReasons += 'MultipleUnsupersededTerminals'
    }
    $armedRecords = @($valid | Where-Object {
            $_.RecordState -eq $script:StateArmed
        })
    foreach ($terminal in $unsupersededTerminals) {
        $matchingLowerArmed = @($armedRecords | Where-Object {
                $_.RecordGeneration -eq $terminal.RecordGeneration -and
                $_.StoreGeneration -lt $terminal.StoreGeneration -and
                (Test-ReferenceRecordsHaveSameKey $_ $terminal)
            })
        if ($matchingLowerArmed.Count -ne 1) {
            $corruptReasons += 'ActiveTerminalWithoutExactlyOneMatchingArmed'
        }
    }
    foreach ($armed in $armedRecords) {
        foreach ($terminal in $unsupersededTerminals) {
            if ($armed.RecordGeneration -ne $terminal.RecordGeneration -or
                -not (Test-ReferenceRecordsHaveSameKey $armed $terminal)) {
                $corruptReasons += 'ArmedWithDifferentActiveTerminal'
            }
        }
    }

    return [pscustomobject][ordered]@{
        AxisReference = $AxisReference
        Entries = $entries
        ValidRecords = $valid
        IsCorrupt = $corruptReasons.Count -ne 0
        CorruptReasons = $corruptReasons
    }
}

function Test-ReferenceRecordIsRetiredShadow {
    param(
        $Record,
        [object[]]$ValidRecords
    )

    if ($Record.IsTombstone -or
        ($Record.RecordState -ne $script:StateSucceeded -and
         $Record.RecordState -ne $script:StateRejected)) {
        return $false
    }
    foreach ($candidate in $ValidRecords) {
        if ($candidate.IsTombstone -and
            $candidate.StoreGeneration -gt $Record.StoreGeneration -and
            $candidate.RecordGeneration -eq $Record.RecordGeneration -and
            (Test-ReferenceRecordsHaveSameKey $Record $candidate) -and
            (Test-ReferenceTerminalSnapshotsEqual $Record $candidate)) {
            return $true
        }
    }
    return $false
}

function Test-ReferenceArmedIsInactiveShadow {
    param(
        $Record,
        [object[]]$ValidRecords
    )

    if ($Record.RecordState -ne $script:StateArmed) {
        return $false
    }
    foreach ($candidate in $ValidRecords) {
        $candidateIsTerminal =
            $candidate.RecordState -eq $script:StateSucceeded -or
            $candidate.RecordState -eq $script:StateRejected
        if ($candidateIsTerminal -and
            $candidate.StoreGeneration -gt $Record.StoreGeneration -and
            $candidate.RecordGeneration -eq $Record.RecordGeneration -and
            (Test-ReferenceRecordsHaveSameKey $Record $candidate)) {
            return $true
        }
    }
    return $false
}

function New-ReferenceOperationResult {
    param(
        [string]$Operation,
        [bool]$Success,
        [uint32]$DetailCode,
        $Record,
        [int]$MutationCount = 0,
        [int]$NativeCount = 0,
        [bool]$NoResponse = $false,
        [bool]$IsDuplicate = $false,
        [int]$ResultCode = [int]::MinValue,
        [byte[]]$SnapshotBytes
    )

    if ($ResultCode -eq [int]::MinValue) {
        if ($Success) {
            $ResultCode = 1
        }
        else {
            $ResultCode = 0
        }
    }

    if ($null -eq $SnapshotBytes) {
        $SnapshotBytes = New-Object byte[] $script:TerminalSnapshotSize
    }
    elseif ($SnapshotBytes.Length -ne $script:TerminalSnapshotSize) {
        throw [ArgumentException]::new(
            'SnapshotBytes must contain exactly 68 bytes.')
    }
    else {
        $SnapshotBytes = Copy-ReferenceBytes $SnapshotBytes
    }
    $hasTerminalRecord = $Success -and $null -ne $Record -and
        $null -ne $Record.PSObject.Properties['RecordState'] -and
        ($Record.RecordState -eq $script:StateSucceeded -or
         $Record.RecordState -eq $script:StateRejected) -and
        $null -ne $Record.PSObject.Properties['Bytes'] -and
        $Record.Bytes.Length -eq $script:RecordSize
    if ($hasTerminalRecord) {
        $SnapshotBytes = Get-ReferenceRegion `
            -Bytes $Record.Bytes -Offset 8 `
            -Length $script:TerminalSnapshotSize
    }

    return [pscustomobject][ordered]@{
        Operation = $Operation
        Success = $Success
        DetailCode = $DetailCode
        Record = $Record
        MutationCount = $MutationCount
        NativeCount = $NativeCount
        NoResponse = $NoResponse
        IsDuplicate = $IsDuplicate
        ResultCode = $ResultCode
        SnapshotBytes = $SnapshotBytes
    }
}

function Invoke-ReferenceQuery {
    param(
        [byte[]]$Store,
        $Key,
        [bool]$StorageAvailable = $true,
        [int]$TotalResponseCapacity = 92
    )

    if (-not $StorageAvailable) {
        return New-ReferenceOperationResult `
            -Operation 'Query' -Success $false `
            -DetailCode $script:DetailStorageUnavailable -Record $null
    }
    $scan = Get-ReferenceAxisScan `
        -Store $Store -AxisReference $Key.AxisReference
    if ($scan.IsCorrupt) {
        return New-ReferenceOperationResult `
            -Operation 'Query' -Success $false `
            -DetailCode $script:DetailStoreCorrupt -Record $null
    }

    $exactTerminals = @($scan.ValidRecords | Where-Object {
            ($_.RecordState -eq $script:StateSucceeded -or
             $_.RecordState -eq $script:StateRejected) -and
            (Test-ReferenceRecordMatchesKey $_ $Key)
        } | Sort-Object StoreGeneration -Descending)
    if ($exactTerminals.Count -ne 0) {
        if ($TotalResponseCapacity -lt $script:TerminalResponseTotalSize) {
            return New-ReferenceOperationResult `
                -Operation 'Query' -Success $false `
                -DetailCode 0 -Record $null
        }
        return New-ReferenceOperationResult `
            -Operation 'Query' -Success $true -DetailCode 0 `
            -Record $exactTerminals[0]
    }

    $exactArmed = @($scan.ValidRecords | Where-Object {
            $_.RecordState -eq $script:StateArmed -and
            (Test-ReferenceRecordMatchesKey $_ $Key)
        })
    if ($exactArmed.Count -ne 0) {
        return New-ReferenceOperationResult `
            -Operation 'Query' -Success $false `
            -DetailCode $script:DetailIndeterminate -Record $null
    }

    if ($scan.ValidRecords.Count -ne 0) {
        return New-ReferenceOperationResult `
            -Operation 'Query' -Success $false `
            -DetailCode $script:DetailKeyMismatch -Record $null
    }
    return New-ReferenceOperationResult `
        -Operation 'Query' -Success $false `
        -DetailCode $script:DetailNotFound -Record $null
}

function Get-ReferenceNextNonZeroGeneration {
    param([uint32]$Current)

    if ($Current -eq $script:UInt32Max) {
        throw [OverflowException]::new('Generation cannot wrap to zero.')
    }
    return [uint32]([uint64]$Current + 1)
}

function Get-ReferenceMaximumGeneration {
    param(
        [object[]]$ValidRecords,
        [string]$PropertyName
    )

    [uint32]$maximum = 0
    foreach ($record in $ValidRecords) {
        $value = [uint32]$record.$PropertyName
        if ($value -gt $maximum) {
            $maximum = $value
        }
    }
    return $maximum
}

function New-ReferenceCommitResult {
    param(
        [bool]$Completed,
        [int]$MutationCount,
        [string]$CrashAt,
        [string]$FailureAt
    )

    return [pscustomobject][ordered]@{
        Completed = $Completed
        MutationCount = $MutationCount
        CrashAt = $CrashAt
        FailureAt = $FailureAt
    }
}

function Invoke-ReferenceRecordCommit {
    param(
        [byte[]]$Store,
        [int]$AxisReference,
        [int]$Slot,
        [byte[]]$RecordBytes,
        [ValidateSet(
            'None',
            'AfterMarkerClear',
            'AfterBodyWrite',
            'DuringMarkerWrite',
            'AfterMarkerWrite')]
        [string]$CrashAt = 'None',
        [ValidateSet(
            'None',
            'MarkerClearWriteFailure',
            'MarkerClearReadbackFailure',
            'MarkerClearReadbackMismatch',
            'BodyWriteFailure',
            'BodyReadbackFailure',
            'BodyReadbackMismatch',
            'FinalMarkerWriteFailure',
            'FinalMarkerReadbackFailure',
            'FinalMarkerReadbackMismatch',
            'FinalRecordReadbackFailure',
            'FinalRecordReadbackMismatch')]
        [string]$FailureAt = 'None'
    )

    if ($CrashAt -ne 'None' -and $FailureAt -ne 'None') {
        throw [ArgumentException]::new(
            'CrashAt and FailureAt are mutually exclusive.')
    }
    $candidate = Read-ReferenceRecord -Bytes $RecordBytes
    if ($candidate.Classification -ne 'Valid') {
        throw [ArgumentException]::new(
            'Commit candidate must be a valid committed record image.')
    }
    $candidateRoleIsValid =
        ($Slot -eq 0 -and
         $candidate.Record.RecordState -eq $script:StateArmed -and
         $candidate.Record.StoreFlags -eq 0) -or
        ($Slot -ne 0 -and
         ($candidate.Record.RecordState -eq $script:StateSucceeded -or
          $candidate.Record.RecordState -eq $script:StateRejected))
    if ($candidate.Record.AxisReference -ne $AxisReference -or
        -not $candidateRoleIsValid) {
        throw [ArgumentException]::new(
            'Commit candidate axis or physical slot role is invalid.')
    }
    $offset = Get-ReferenceRecordOffset `
        -AxisReference $AxisReference -Slot $Slot
    $mutationCount = 0

    if ($FailureAt -eq 'MarkerClearWriteFailure') {
        return New-ReferenceCommitResult `
            -Completed $false -MutationCount $mutationCount `
            -CrashAt $CrashAt -FailureAt $FailureAt
    }
    Set-ReferenceUInt32LE `
        -Bytes $Store `
        -Offset ($offset + $script:CommitMarkerOffset) `
        -Value 0
    $mutationCount++
    if ($FailureAt -eq 'MarkerClearReadbackMismatch') {
        $Store[$offset + $script:CommitMarkerOffset] = 1
        return New-ReferenceCommitResult `
            -Completed $false -MutationCount $mutationCount `
            -CrashAt $CrashAt -FailureAt $FailureAt
    }
    if ($FailureAt -eq 'MarkerClearReadbackFailure') {
        return New-ReferenceCommitResult `
            -Completed $false -MutationCount $mutationCount `
            -CrashAt $CrashAt -FailureAt $FailureAt
    }
    if ((Get-ReferenceUInt32LE `
            -Bytes $Store `
            -Offset ($offset + $script:CommitMarkerOffset)) -ne 0) {
        throw [InvalidOperationException]::new(
            'Marker-clear readback validation failed.')
    }
    if ($CrashAt -eq 'AfterMarkerClear') {
        return New-ReferenceCommitResult `
            -Completed $false -MutationCount $mutationCount `
            -CrashAt $CrashAt -FailureAt $FailureAt
    }

    if ($FailureAt -eq 'BodyWriteFailure') {
        return New-ReferenceCommitResult `
            -Completed $false -MutationCount $mutationCount `
            -CrashAt $CrashAt -FailureAt $FailureAt
    }
    [Array]::Copy($RecordBytes, 0, $Store, $offset, 80)
    $mutationCount++
    if ($FailureAt -eq 'BodyReadbackMismatch') {
        $Store[$offset] = [byte]($Store[$offset] -bxor 1)
        return New-ReferenceCommitResult `
            -Completed $false -MutationCount $mutationCount `
            -CrashAt $CrashAt -FailureAt $FailureAt
    }
    if ($FailureAt -eq 'BodyReadbackFailure') {
        return New-ReferenceCommitResult `
            -Completed $false -MutationCount $mutationCount `
            -CrashAt $CrashAt -FailureAt $FailureAt
    }
    $stagedBody = Get-ReferenceRegion `
        -Bytes $RecordBytes -Offset 0 -Length 80
    $bodyReadback = Get-ReferenceRegion `
        -Bytes $Store -Offset $offset -Length 80
    $bodyCrc = Get-TestOnlyOracleCrc32 `
        -Bytes $bodyReadback -Offset 0 -Length 76 -Seed 0
    if (-not (Test-ReferenceBytesEqual $stagedBody $bodyReadback) -or
        $bodyCrc -ne (Get-ReferenceUInt32LE $bodyReadback 76)) {
        throw [InvalidOperationException]::new(
            'Body readback or CRC validation failed.')
    }
    if ($CrashAt -eq 'AfterBodyWrite') {
        return New-ReferenceCommitResult `
            -Completed $false -MutationCount $mutationCount `
            -CrashAt $CrashAt -FailureAt $FailureAt
    }

    if ($FailureAt -eq 'FinalMarkerWriteFailure') {
        return New-ReferenceCommitResult `
            -Completed $false -MutationCount $mutationCount `
            -CrashAt $CrashAt -FailureAt $FailureAt
    }
    if ($CrashAt -eq 'DuringMarkerWrite') {
        $Store[$offset + $script:CommitMarkerOffset] =
            $RecordBytes[$script:CommitMarkerOffset]
        $mutationCount++
        return New-ReferenceCommitResult `
            -Completed $false -MutationCount $mutationCount `
            -CrashAt $CrashAt -FailureAt $FailureAt
    }

    [Array]::Copy(
        $RecordBytes,
        $script:CommitMarkerOffset,
        $Store,
        $offset + $script:CommitMarkerOffset,
        4)
    $mutationCount++
    if ($FailureAt -eq 'FinalMarkerReadbackMismatch') {
        $Store[$offset + $script:CommitMarkerOffset + 3] = [byte](
            $Store[$offset + $script:CommitMarkerOffset + 3] -bxor 1)
        return New-ReferenceCommitResult `
            -Completed $false -MutationCount $mutationCount `
            -CrashAt $CrashAt -FailureAt $FailureAt
    }
    if ($FailureAt -eq 'FinalMarkerReadbackFailure') {
        return New-ReferenceCommitResult `
            -Completed $false -MutationCount $mutationCount `
            -CrashAt $CrashAt -FailureAt $FailureAt
    }
    if ((Get-ReferenceUInt32LE `
            -Bytes $Store `
            -Offset ($offset + $script:CommitMarkerOffset)) -ne
        $script:CommitMarker) {
        throw [InvalidOperationException]::new(
            'Final-marker readback validation failed.')
    }
    if ($CrashAt -eq 'AfterMarkerWrite') {
        return New-ReferenceCommitResult `
            -Completed $false -MutationCount $mutationCount `
            -CrashAt $CrashAt -FailureAt $FailureAt
    }
    if ($FailureAt -eq 'FinalRecordReadbackMismatch') {
        $Store[$offset + $script:RecordCrcOffset] = [byte](
            $Store[$offset + $script:RecordCrcOffset] -bxor 1)
        return New-ReferenceCommitResult `
            -Completed $false -MutationCount $mutationCount `
            -CrashAt $CrashAt -FailureAt $FailureAt
    }
    if ($FailureAt -eq 'FinalRecordReadbackFailure') {
        return New-ReferenceCommitResult `
            -Completed $false -MutationCount $mutationCount `
            -CrashAt $CrashAt -FailureAt $FailureAt
    }

    $readbackBytes = Get-ReferenceRegion `
        -Bytes $Store -Offset $offset -Length $script:RecordSize
    $readback = Read-ReferenceRecord `
        -Bytes $readbackBytes -Slot $Slot
    if ($readback.Classification -ne 'Valid') {
        throw [InvalidOperationException]::new(
            'Final retained record full validation failed.')
    }
    $slotRoleIsValid =
        ($Slot -eq 0 -and
         $readback.Record.RecordState -eq $script:StateArmed -and
         $readback.Record.StoreFlags -eq 0) -or
        ($Slot -ne 0 -and
         ($readback.Record.RecordState -eq $script:StateSucceeded -or
          $readback.Record.RecordState -eq $script:StateRejected))
    if (-not (Test-ReferenceBytesEqual $RecordBytes $readbackBytes) -or
        $readback.Record.AxisReference -ne $AxisReference -or
        -not $slotRoleIsValid) {
        throw [InvalidOperationException]::new(
            'Final retained record readback or full validation failed.')
    }
    return New-ReferenceCommitResult `
        -Completed $true -MutationCount $mutationCount `
        -CrashAt $CrashAt -FailureAt $FailureAt
}

function Get-ReferenceTerminalTargetSlot {
    param(
        $Scan,
        [int]$ExcludedSlot = -1
    )

    # Empty priority is global across A/B/C: every Blank beats every
    # Incomplete, with physical slot order breaking ties.
    foreach ($entry in $Scan.Entries) {
        if ($entry.Slot -eq 0 -or $entry.Slot -eq $ExcludedSlot) {
            continue
        }
        if ($entry.Classification -eq 'Blank') {
            return $entry.Slot
        }
    }
    foreach ($entry in $Scan.Entries) {
        if ($entry.Slot -eq 0 -or $entry.Slot -eq $ExcludedSlot) {
            continue
        }
        if ($entry.Classification -eq 'Incomplete') {
            return $entry.Slot
        }
    }

    $protectedTombstone = @($Scan.ValidRecords | Where-Object {
            $_.IsTombstone
        } | Sort-Object StoreGeneration -Descending | Select-Object -First 1)
    $protectedSlot = if ($protectedTombstone.Count -eq 0) {
        -1
    }
    else {
        [int]$protectedTombstone[0].Slot
    }
    $replaceable = @($Scan.ValidRecords | Where-Object {
            $_.Slot -ne 0 -and
            $_.Slot -ne $ExcludedSlot -and
            $_.Slot -ne $protectedSlot -and
            ($_.IsTombstone -or
             (Test-ReferenceRecordIsRetiredShadow `
                -Record $_ -ValidRecords $Scan.ValidRecords))
        } | Sort-Object StoreGeneration, Slot)
    if ($replaceable.Count -ne 0) {
        return [int]$replaceable[0].Slot
    }
    return -1
}

function ConvertTo-ReferenceTombstoneBytes {
    param(
        $Record,
        [uint32]$StoreGeneration
    )

    $key = New-ReferenceRecoveryKey `
        -AxisReference $Record.AxisReference `
        -DiagnosticsBuild $Record.DiagnosticsBuild `
        -DiagnosticsBootId $Record.DiagnosticsBootId `
        -MapRevision $Record.MapRevision `
        -OriginalRequestId $Record.OriginalRequestId `
        -Intent0 $Record.Intent0 `
        -Intent1 $Record.Intent1 `
        -Intent2 $Record.Intent2 `
        -Intent3 $Record.Intent3 `
        -TargetPosition $Record.TargetPosition `
        -ExpectedActualPosition $Record.ExpectedActualPosition `
        -SemanticMode $Record.SemanticMode `
        -SchemaVersion $Record.StoreSchema
    return ,(New-ReferenceRecordBytes `
        -Key $key `
        -StoreGeneration $StoreGeneration `
        -RecordState $Record.RecordState `
        -RecordGeneration $Record.RecordGeneration `
        -AppliedPosition $Record.AppliedPosition `
        -OriginalCommandStatus $Record.OriginalCommandStatus `
        -OriginalErrorId $Record.OriginalErrorId `
        -OriginalDetailCode $Record.OriginalDetailCode `
        -NativeCommandState $Record.NativeCommandState `
        -Tombstone)
}

function Invoke-ReferenceRetirementStoreCore {
    param(
        [byte[]]$Store,
        $Key,
        [uint32]$ExpectedRecordGeneration,
        [bool]$StorageAvailable = $true,
        [int]$SnapshotCapacity = 68,
        [ValidateSet(
            'None',
            'AfterMarkerClear',
            'AfterBodyWrite',
            'DuringMarkerWrite',
            'AfterMarkerWrite')]
        [string]$CrashAt = 'None',
        [ValidateSet(
            'None',
            'MarkerClearWriteFailure',
            'MarkerClearReadbackFailure',
            'MarkerClearReadbackMismatch',
            'BodyWriteFailure',
            'BodyReadbackFailure',
            'BodyReadbackMismatch',
            'FinalMarkerWriteFailure',
            'FinalMarkerReadbackFailure',
            'FinalMarkerReadbackMismatch',
            'FinalRecordReadbackFailure',
            'FinalRecordReadbackMismatch')]
        [string]$CommitFailureAt = 'None'
    )

    if ($SnapshotCapacity -lt $script:TerminalSnapshotSize) {
        # Store public methods validate their own 68-byte snapshot boundary.
        # This is independent of the service's 92-byte total wire response.
        return New-ReferenceOperationResult `
            -Operation 'RetirementStoreCore' -Success $false `
            -DetailCode $script:DetailStorageUnavailable -Record $null
    }
    $query = Invoke-ReferenceQuery `
        -Store $Store -Key $Key -StorageAvailable $StorageAvailable
    if (-not $query.Success) {
        return New-ReferenceOperationResult `
            -Operation 'Retirement' -Success $false `
            -DetailCode $query.DetailCode -Record $null
    }
    if ($query.Record.RecordGeneration -ne $ExpectedRecordGeneration) {
        return New-ReferenceOperationResult `
            -Operation 'Retirement' -Success $false `
            -DetailCode $script:DetailKeyMismatch -Record $null
    }
    if ($query.Record.IsTombstone) {
        return New-ReferenceOperationResult `
            -Operation 'Retirement' -Success $true -DetailCode 0 `
            -Record $query.Record -IsDuplicate $true
    }

    $scan = Get-ReferenceAxisScan `
        -Store $Store -AxisReference $Key.AxisReference
    try {
        $maximum = Get-ReferenceMaximumGeneration `
            -ValidRecords $scan.ValidRecords -PropertyName 'StoreGeneration'
        $nextStoreGeneration = Get-ReferenceNextNonZeroGeneration $maximum
    }
    catch [OverflowException] {
        return New-ReferenceOperationResult `
            -Operation 'Retirement' -Success $false `
            -DetailCode $script:DetailStorageUnavailable -Record $null
    }

    $targetSlot = Get-ReferenceTerminalTargetSlot `
        -Scan $scan -ExcludedSlot $query.Record.Slot
    if ($targetSlot -lt 0) {
        return New-ReferenceOperationResult `
            -Operation 'Retirement' -Success $false `
            -DetailCode $script:DetailStoreCorrupt -Record $null
    }
    $tombstone = ConvertTo-ReferenceTombstoneBytes `
        -Record $query.Record -StoreGeneration $nextStoreGeneration
    $commit = Invoke-ReferenceRecordCommit `
        -Store $Store `
        -AxisReference $Key.AxisReference `
        -Slot $targetSlot `
        -RecordBytes $tombstone `
        -CrashAt $CrashAt `
        -FailureAt $CommitFailureAt
    if (-not $commit.Completed) {
        # The current transport close-without-response sentinel is scoped to
        # 0x7D12 terminal commit only. 0x7D1A immediately rescans: an exact
        # durable tombstone proves success, corruption is detail 21, and a
        # preserved source without commit proof is detail 24.
        $rescan = Get-ReferenceAxisScan `
            -Store $Store -AxisReference $Key.AxisReference
        if ($rescan.IsCorrupt) {
            return New-ReferenceOperationResult `
                -Operation 'Retirement' -Success $false `
                -DetailCode $script:DetailStoreCorrupt `
                -Record $null -MutationCount $commit.MutationCount
        }
        $durableTombstone = @($rescan.ValidRecords | Where-Object {
                $_.Slot -eq $targetSlot -and
                $_.IsTombstone -and
                $_.StoreGeneration -eq $nextStoreGeneration -and
                $_.RecordGeneration -eq $ExpectedRecordGeneration -and
                (Test-ReferenceRecordMatchesKey $_ $Key) -and
                (Test-ReferenceTerminalSnapshotsEqual $_ $query.Record)
            })
        if ($durableTombstone.Count -eq 1) {
            return New-ReferenceOperationResult `
                -Operation 'Retirement' -Success $true -DetailCode 0 `
                -Record $durableTombstone[0] `
                -MutationCount $commit.MutationCount
        }
        $preservedSource = @($rescan.ValidRecords | Where-Object {
                $_.Slot -eq $query.Record.Slot -and
                -not $_.IsTombstone -and
                $_.StoreGeneration -eq $query.Record.StoreGeneration -and
                $_.RecordGeneration -eq $ExpectedRecordGeneration -and
                (Test-ReferenceRecordMatchesKey $_ $Key) -and
                (Test-ReferenceTerminalSnapshotsEqual $_ $query.Record)
            })
        if ($preservedSource.Count -ne 1) {
            return New-ReferenceOperationResult `
                -Operation 'Retirement' -Success $false `
                -DetailCode $script:DetailStoreCorrupt `
                -Record $null -MutationCount $commit.MutationCount
        }
        return New-ReferenceOperationResult `
            -Operation 'Retirement' -Success $false `
            -DetailCode $script:DetailStorageUnavailable `
            -Record $null -MutationCount $commit.MutationCount
    }

    $committed = Read-ReferenceRecord `
        -Bytes $Store `
        -Offset (Get-ReferenceRecordOffset `
            -AxisReference $Key.AxisReference -Slot $targetSlot) `
        -Slot $targetSlot
    return New-ReferenceOperationResult `
        -Operation 'Retirement' -Success $true -DetailCode 0 `
        -Record $committed.Record `
        -MutationCount $commit.MutationCount
}

function Invoke-ReferenceRetirement {
    param(
        [byte[]]$Store,
        $Key,
        [uint32]$ExpectedRecordGeneration,
        [bool]$StorageAvailable = $true,
        [int]$TotalResponseCapacity = 92,
        [int]$StoreSnapshotCapacity = 68,
        [ValidateSet(
            'None',
            'AfterMarkerClear',
            'AfterBodyWrite',
            'DuringMarkerWrite',
            'AfterMarkerWrite')]
        [string]$CrashAt = 'None',
        [ValidateSet(
            'None',
            'MarkerClearWriteFailure',
            'MarkerClearReadbackFailure',
            'MarkerClearReadbackMismatch',
            'BodyWriteFailure',
            'BodyReadbackFailure',
            'BodyReadbackMismatch',
            'FinalMarkerWriteFailure',
            'FinalMarkerReadbackFailure',
            'FinalMarkerReadbackMismatch',
            'FinalRecordReadbackFailure',
            'FinalRecordReadbackMismatch')]
        [string]$CommitFailureAt = 'None'
    )

    if ($TotalResponseCapacity -lt $script:TerminalResponseTotalSize) {
        # The service owns the 92-byte wire response and rejects insufficient
        # capacity before entering the retained-store core.
        return New-ReferenceOperationResult `
            -Operation 'Retirement' -Success $false `
            -DetailCode 0 -Record $null
    }
    return Invoke-ReferenceRetirementStoreCore `
        -Store $Store `
        -Key $Key `
        -ExpectedRecordGeneration $ExpectedRecordGeneration `
        -StorageAvailable $StorageAvailable `
        -SnapshotCapacity $StoreSnapshotCapacity `
        -CrashAt $CrashAt `
        -CommitFailureAt $CommitFailureAt
}

function Invoke-ReferenceBeginSetPosition {
    param(
        [byte[]]$Store,
        $Transaction,
        $Key,
        [byte[]]$KeyBytes,
        [int]$KeySize = 48,
        [bool]$StorageAvailable = $true,
        [int]$SnapshotCapacity = 68,
        [uint32]$PreArmedDetailCode = 0,
        [ValidateSet(
            'None',
            'AfterMarkerClear',
            'AfterBodyWrite',
            'DuringMarkerWrite',
            'AfterMarkerWrite')]
        [string]$IntentCrashAt = 'None',
        [ValidateSet(
            'None',
            'MarkerClearWriteFailure',
            'MarkerClearReadbackFailure',
            'MarkerClearReadbackMismatch',
            'BodyWriteFailure',
            'BodyReadbackFailure',
            'BodyReadbackMismatch',
            'FinalMarkerWriteFailure',
            'FinalMarkerReadbackFailure',
            'FinalMarkerReadbackMismatch',
            'FinalRecordReadbackFailure',
            'FinalRecordReadbackMismatch')]
        [string]$IntentFailureAt = 'None'
    )

    if ($null -eq $Transaction) {
        throw [ArgumentNullException]::new('Transaction')
    }
    $keyBoundary = Get-ReferenceRecoveryKeyBoundary `
        -Key $Key -KeyBytes $KeyBytes -KeySize $KeySize
    if (-not $keyBoundary.IsValid) {
        return New-ReferenceOperationResult `
            -Operation 'BeginSetPosition' -Success $false `
            -DetailCode 0 -Record $null -ResultCode -1
    }
    $Key = $keyBoundary.Key
    if ($Transaction.Active) {
        return New-ReferenceOperationResult `
            -Operation 'BeginSetPosition' -Success $false `
            -DetailCode $script:DetailStorageUnavailable -Record $null
    }
    Clear-ReferenceSetPositionTransaction -Transaction $Transaction

    $allowedPreArmedDetails = @(
        1, 2, 3, 4, 5, 6, 7, 8, 9,
        16, 17, 18, 24)
    if ($PreArmedDetailCode -ne 0) {
        # This injected result models Control-service gates that dominate the
        # store Begin call. No retained-store scan or mutation occurs here.
        if ($allowedPreArmedDetails -notcontains $PreArmedDetailCode) {
            throw [ArgumentOutOfRangeException]::new('PreArmedDetailCode')
        }
        return New-ReferenceOperationResult `
            -Operation 'BeginSetPosition' -Success $false `
            -DetailCode $PreArmedDetailCode -Record $null
    }
    if (-not $StorageAvailable -or
        $SnapshotCapacity -lt $script:TerminalSnapshotSize) {
        return New-ReferenceOperationResult `
            -Operation 'BeginSetPosition' -Success $false `
            -DetailCode $script:DetailStorageUnavailable -Record $null
    }

    $scan = Get-ReferenceAxisScan `
        -Store $Store -AxisReference $Key.AxisReference
    if ($scan.IsCorrupt) {
        return New-ReferenceOperationResult `
            -Operation 'BeginSetPosition' -Success $false `
            -DetailCode $script:DetailStoreCorrupt -Record $null
    }
    $exactTerminal = @($scan.ValidRecords | Where-Object {
            ($_.RecordState -eq $script:StateSucceeded -or
             $_.RecordState -eq $script:StateRejected) -and
            (Test-ReferenceRecordMatchesKey $_ $Key)
        } | Sort-Object StoreGeneration -Descending)
    if ($exactTerminal.Count -ne 0) {
        return New-ReferenceOperationResult `
            -Operation 'BeginSetPosition' -Success $true -DetailCode 0 `
            -Record $exactTerminal[0] -IsDuplicate $true -ResultCode 2
    }
    $exactArmed = @($scan.ValidRecords | Where-Object {
            $_.RecordState -eq $script:StateArmed -and
            (Test-ReferenceRecordMatchesKey $_ $Key)
        })
    if ($exactArmed.Count -ne 0) {
        return New-ReferenceOperationResult `
            -Operation 'BeginSetPosition' -Success $false `
            -DetailCode $script:DetailIndeterminate -Record $null
    }
    $activeDifferent = @($scan.ValidRecords | Where-Object {
            if ($_.RecordState -eq $script:StateArmed) {
                return -not (Test-ReferenceArmedIsInactiveShadow `
                    -Record $_ -ValidRecords $scan.ValidRecords)
            }
            if ($_.IsTombstone) {
                return $false
            }
            return -not (Test-ReferenceRecordIsRetiredShadow `
                -Record $_ -ValidRecords $scan.ValidRecords)
        })
    if ($activeDifferent.Count -ne 0) {
        return New-ReferenceOperationResult `
            -Operation 'BeginSetPosition' -Success $false `
            -DetailCode $script:DetailSlotOccupied -Record $null
    }

    try {
        $maximumStoreGeneration = Get-ReferenceMaximumGeneration `
            -ValidRecords $scan.ValidRecords -PropertyName 'StoreGeneration'
        $intentStoreGeneration = Get-ReferenceNextNonZeroGeneration `
            $maximumStoreGeneration
        $terminalStoreGeneration = Get-ReferenceNextNonZeroGeneration `
            $intentStoreGeneration
        $maximumRecordGeneration = Get-ReferenceMaximumGeneration `
            -ValidRecords $scan.ValidRecords -PropertyName 'RecordGeneration'
        $recordGeneration = Get-ReferenceNextNonZeroGeneration `
            $maximumRecordGeneration
    }
    catch [OverflowException] {
        return New-ReferenceOperationResult `
            -Operation 'BeginSetPosition' -Success $false `
            -DetailCode $script:DetailStorageUnavailable -Record $null
    }

    $targetSlot = Get-ReferenceTerminalTargetSlot -Scan $scan
    if ($targetSlot -lt 0) {
        return New-ReferenceOperationResult `
            -Operation 'BeginSetPosition' -Success $false `
            -DetailCode $script:DetailSlotOccupied -Record $null
    }
    $targetBeforeBytes = Get-ReferenceRegion `
        -Bytes $Store `
        -Offset (Get-ReferenceRecordOffset `
            -AxisReference $Key.AxisReference -Slot $targetSlot) `
        -Length $script:RecordSize
    $armed = New-ReferenceArmedRecordBytes `
        -Key $Key `
        -StoreGeneration $intentStoreGeneration `
        -RecordGeneration $recordGeneration
    $intentCommit = Invoke-ReferenceRecordCommit `
        -Store $Store `
        -AxisReference $Key.AxisReference `
        -Slot 0 `
        -RecordBytes $armed `
        -CrashAt $IntentCrashAt `
        -FailureAt $IntentFailureAt
    if (-not $intentCommit.Completed) {
        return New-ReferenceOperationResult `
            -Operation 'BeginSetPosition' -Success $false `
            -DetailCode $script:DetailStorageUnavailable -Record $null `
            -MutationCount $intentCommit.MutationCount
    }

    $committedArmed = Read-ReferenceRecord `
        -Bytes $Store `
        -Offset (Get-ReferenceRecordOffset `
            -AxisReference $Key.AxisReference -Slot 0) `
        -Slot 0
    $Transaction.AxisReference = [int]$Key.AxisReference
    $Transaction.Key = Copy-ReferenceRecoveryKey -Key $Key
    $Transaction.RecordGeneration = [uint32]$recordGeneration
    $Transaction.IntentStoreGeneration = [uint32]$intentStoreGeneration
    $Transaction.TerminalTargetSlot = [int]$targetSlot
    $Transaction.ReservedTerminalTargetSlot = [int]$targetSlot
    $Transaction.TerminalStoreGeneration = [uint32]$terminalStoreGeneration
    $Transaction.ReservedTerminalStoreGeneration =
        [uint32]$terminalStoreGeneration
    $Transaction.TerminalTargetBeforeBytes =
        Copy-ReferenceBytes $targetBeforeBytes
    $Transaction.Active = $true
    return New-ReferenceOperationResult `
        -Operation 'BeginSetPosition' -Success $true -DetailCode 0 `
        -Record $committedArmed.Record `
        -MutationCount $intentCommit.MutationCount -ResultCode 1
}

function Invoke-ReferenceCommitSetPositionTerminal {
    param(
        [byte[]]$Store,
        $Transaction,
        $Key,
        [byte[]]$KeyBytes,
        [int]$KeySize = 48,
        [uint32]$RecordGeneration,
        [uint16]$RecordState,
        [int32]$AppliedPosition = 0,
        [uint16]$OriginalCommandStatus = 1,
        [int16]$OriginalErrorId = -31000,
        [uint32]$OriginalDetailCode = 10,
        [uint32]$NativeCommandState = 0,
        [int]$SnapshotCapacity = 68,
        [ValidateSet(
            'None',
            'AfterMarkerClear',
            'AfterBodyWrite',
            'DuringMarkerWrite',
            'AfterMarkerWrite')]
        [string]$TerminalCrashAt = 'None',
        [ValidateSet(
            'None',
            'MarkerClearWriteFailure',
            'MarkerClearReadbackFailure',
            'MarkerClearReadbackMismatch',
            'BodyWriteFailure',
            'BodyReadbackFailure',
            'BodyReadbackMismatch',
            'FinalMarkerWriteFailure',
            'FinalMarkerReadbackFailure',
            'FinalMarkerReadbackMismatch',
            'FinalRecordReadbackFailure',
            'FinalRecordReadbackMismatch')]
        [string]$TerminalFailureAt = 'None'
    )

    if ($null -eq $Transaction) {
        throw [ArgumentNullException]::new('Transaction')
    }
    $keyBoundary = Get-ReferenceRecoveryKeyBoundary `
        -Key $Key -KeyBytes $KeyBytes -KeySize $KeySize
    if (-not $Transaction.Active) {
        if (-not $keyBoundary.IsValid) {
            return New-ReferenceOperationResult `
                -Operation 'CommitSetPositionTerminal' -Success $false `
                -DetailCode 0 -Record $null -ResultCode -1
        }
        return New-ReferenceOperationResult `
            -Operation 'CommitSetPositionTerminal' -Success $false `
            -DetailCode $script:DetailStorageUnavailable -Record $null
    }

    try {
        if (-not $keyBoundary.IsValid) {
            return New-ReferenceOperationResult `
                -Operation 'CommitSetPositionTerminal' -Success $false `
                -DetailCode 0 -Record $null -NoResponse $true `
                -ResultCode -12
        }
        $Key = $keyBoundary.Key
        $reservationValid =
            $SnapshotCapacity -ge $script:TerminalSnapshotSize -and
            (Test-ReferenceRecoveryKeysEqual $Transaction.Key $Key) -and
            $RecordGeneration -eq $Transaction.RecordGeneration -and
            $Transaction.AxisReference -eq $Key.AxisReference -and
            $Transaction.TerminalTargetSlot -eq
                $Transaction.ReservedTerminalTargetSlot -and
            $Transaction.TerminalStoreGeneration -eq
                $Transaction.ReservedTerminalStoreGeneration
        if (-not $reservationValid) {
            return New-ReferenceOperationResult `
                -Operation 'CommitSetPositionTerminal' -Success $false `
                -DetailCode 0 -Record $null -NoResponse $true `
                -ResultCode -12
        }

        $scan = Get-ReferenceAxisScan `
            -Store $Store -AxisReference $Transaction.AxisReference
        $matchingArmed = @($scan.ValidRecords | Where-Object {
                $_.Slot -eq 0 -and
                $_.RecordState -eq $script:StateArmed -and
                $_.StoreGeneration -eq $Transaction.IntentStoreGeneration -and
                $_.RecordGeneration -eq $Transaction.RecordGeneration -and
                (Test-ReferenceRecordMatchesKey $_ $Transaction.Key)
            })
        $maximumStoreGeneration = Get-ReferenceMaximumGeneration `
            -ValidRecords $scan.ValidRecords -PropertyName 'StoreGeneration'
        $targetCurrentBytes = Get-ReferenceRegion `
            -Bytes $Store `
            -Offset (Get-ReferenceRecordOffset `
                -AxisReference $Transaction.AxisReference `
                -Slot $Transaction.TerminalTargetSlot) `
            -Length $script:RecordSize
        if ($scan.IsCorrupt -or
            $matchingArmed.Count -ne 1 -or
            $maximumStoreGeneration -ne
                $Transaction.IntentStoreGeneration -or
            $Transaction.TerminalStoreGeneration -ne
                ($Transaction.IntentStoreGeneration + 1) -or
            -not (Test-ReferenceBytesEqual `
                $Transaction.TerminalTargetBeforeBytes `
                $targetCurrentBytes)) {
            return New-ReferenceOperationResult `
                -Operation 'CommitSetPositionTerminal' -Success $false `
                -DetailCode 0 -Record $null -NoResponse $true `
                -ResultCode -12
        }

        $terminal = New-ReferenceRecordBytes `
            -Key $Transaction.Key `
            -StoreGeneration $Transaction.TerminalStoreGeneration `
            -RecordState $RecordState `
            -RecordGeneration $Transaction.RecordGeneration `
            -AppliedPosition $AppliedPosition `
            -OriginalCommandStatus $OriginalCommandStatus `
            -OriginalErrorId $OriginalErrorId `
            -OriginalDetailCode $OriginalDetailCode `
            -NativeCommandState $NativeCommandState
        $candidate = Read-ReferenceRecord `
            -Bytes $terminal -Slot $Transaction.TerminalTargetSlot
        if ($candidate.Classification -ne 'Valid') {
            return New-ReferenceOperationResult `
                -Operation 'CommitSetPositionTerminal' -Success $false `
                -DetailCode 0 -Record $null -NoResponse $true `
                -ResultCode -12
        }

        $terminalCommit = Invoke-ReferenceRecordCommit `
            -Store $Store `
            -AxisReference $Transaction.AxisReference `
            -Slot $Transaction.TerminalTargetSlot `
            -RecordBytes $terminal `
            -CrashAt $TerminalCrashAt `
            -FailureAt $TerminalFailureAt
        if (-not $terminalCommit.Completed) {
            return New-ReferenceOperationResult `
                -Operation 'CommitSetPositionTerminal' -Success $false `
                -DetailCode 0 -Record $null `
                -MutationCount $terminalCommit.MutationCount `
                -NoResponse $true -ResultCode -12
        }
        $committed = Read-ReferenceRecord `
            -Bytes $Store `
            -Offset (Get-ReferenceRecordOffset `
                -AxisReference $Transaction.AxisReference `
                -Slot $Transaction.TerminalTargetSlot) `
            -Slot $Transaction.TerminalTargetSlot
        return New-ReferenceOperationResult `
            -Operation 'CommitSetPositionTerminal' `
            -Success $true -DetailCode 0 `
            -Record $committed.Record `
            -MutationCount $terminalCommit.MutationCount -ResultCode 1
    }
    finally {
        Clear-ReferenceSetPositionTransaction -Transaction $Transaction
    }
}

function Invoke-ReferenceReadSetPositionOutcome {
    param(
        [byte[]]$Store,
        $Transaction,
        $Key,
        [byte[]]$KeyBytes,
        [int]$KeySize = 48,
        [bool]$StorageAvailable = $true,
        [int]$SnapshotCapacity = 68
    )

    $keyBoundary = Get-ReferenceRecoveryKeyBoundary `
        -Key $Key -KeyBytes $KeyBytes -KeySize $KeySize
    if (-not $keyBoundary.IsValid) {
        return New-ReferenceOperationResult `
            -Operation 'ReadSetPositionOutcome' -Success $false `
            -DetailCode 0 -Record $null -ResultCode -1
    }
    $Key = $keyBoundary.Key
    if ($null -ne $Transaction -and $Transaction.Active -and
        $Transaction.AxisReference -eq $Key.AxisReference) {
        return New-ReferenceOperationResult `
            -Operation 'ReadSetPositionOutcome' -Success $false `
            -DetailCode $script:DetailStorageUnavailable -Record $null
    }
    if ($SnapshotCapacity -lt $script:TerminalSnapshotSize) {
        return New-ReferenceOperationResult `
            -Operation 'ReadSetPositionOutcome' -Success $false `
            -DetailCode $script:DetailStorageUnavailable -Record $null
    }
    return Invoke-ReferenceQuery `
        -Store $Store -Key $Key -StorageAvailable $StorageAvailable
}

function Invoke-ReferenceRetireSetPositionOutcome {
    param(
        [byte[]]$Store,
        $Transaction,
        $Key,
        [byte[]]$KeyBytes,
        [int]$KeySize = 48,
        [uint32]$ExpectedRecordGeneration,
        [bool]$StorageAvailable = $true,
        [int]$SnapshotCapacity = 68
    )

    $keyBoundary = Get-ReferenceRecoveryKeyBoundary `
        -Key $Key -KeyBytes $KeyBytes -KeySize $KeySize
    if (-not $keyBoundary.IsValid -or
        $ExpectedRecordGeneration -eq 0) {
        return New-ReferenceOperationResult `
            -Operation 'RetireSetPositionOutcome' -Success $false `
            -DetailCode 0 -Record $null -ResultCode -1
    }
    $Key = $keyBoundary.Key
    if ($null -ne $Transaction -and $Transaction.Active -and
        $Transaction.AxisReference -eq $Key.AxisReference) {
        return New-ReferenceOperationResult `
            -Operation 'RetireSetPositionOutcome' -Success $false `
            -DetailCode $script:DetailStorageUnavailable -Record $null
    }
    return Invoke-ReferenceRetirementStoreCore `
        -Store $Store `
        -Key $Key `
        -ExpectedRecordGeneration $ExpectedRecordGeneration `
        -StorageAvailable $StorageAvailable `
        -SnapshotCapacity $SnapshotCapacity
}

function Invoke-ReferenceStart {
    param(
        [byte[]]$Store,
        $Key,
        [bool]$StorageAvailable = $true,
        [ValidateSet('Succeeded', 'PreNativeRejected', 'NativeRejected')]
        [string]$Outcome = 'Succeeded',
        [ValidateSet(
            'None',
            'AfterMarkerClear',
            'AfterBodyWrite',
            'DuringMarkerWrite',
            'AfterMarkerWrite')]
        [string]$IntentCrashAt = 'None',
        [ValidateSet(
            'None',
            'AfterMarkerClear',
            'AfterBodyWrite',
            'DuringMarkerWrite',
            'AfterMarkerWrite')]
        [string]$TerminalCrashAt = 'None',
        [ValidateSet(
            'None',
            'MarkerClearWriteFailure',
            'MarkerClearReadbackFailure',
            'MarkerClearReadbackMismatch',
            'BodyWriteFailure',
            'BodyReadbackFailure',
            'BodyReadbackMismatch',
            'FinalMarkerWriteFailure',
            'FinalMarkerReadbackFailure',
            'FinalMarkerReadbackMismatch',
            'FinalRecordReadbackFailure',
            'FinalRecordReadbackMismatch')]
        [string]$IntentFailureAt = 'None',
        [ValidateSet(
            'None',
            'MarkerClearWriteFailure',
            'MarkerClearReadbackFailure',
            'MarkerClearReadbackMismatch',
            'BodyWriteFailure',
            'BodyReadbackFailure',
            'BodyReadbackMismatch',
            'FinalMarkerWriteFailure',
            'FinalMarkerReadbackFailure',
            'FinalMarkerReadbackMismatch',
            'FinalRecordReadbackFailure',
            'FinalRecordReadbackMismatch')]
        [string]$TerminalFailureAt = 'None'
    )

    if (-not $StorageAvailable) {
        return New-ReferenceOperationResult `
            -Operation 'Start' -Success $false `
            -DetailCode $script:DetailStorageUnavailable -Record $null
    }
    $scan = Get-ReferenceAxisScan `
        -Store $Store -AxisReference $Key.AxisReference
    if ($scan.IsCorrupt) {
        return New-ReferenceOperationResult `
            -Operation 'Start' -Success $false `
            -DetailCode $script:DetailStoreCorrupt -Record $null
    }

    $exactTerminal = @($scan.ValidRecords | Where-Object {
            ($_.RecordState -eq $script:StateSucceeded -or
             $_.RecordState -eq $script:StateRejected) -and
            (Test-ReferenceRecordMatchesKey $_ $Key)
        } | Sort-Object StoreGeneration -Descending)
    if ($exactTerminal.Count -ne 0) {
        return New-ReferenceOperationResult `
            -Operation 'Start' -Success $true -DetailCode 0 `
            -Record $exactTerminal[0] -IsDuplicate $true
    }
    $exactArmed = @($scan.ValidRecords | Where-Object {
            $_.RecordState -eq $script:StateArmed -and
            (Test-ReferenceRecordMatchesKey $_ $Key)
        })
    if ($exactArmed.Count -ne 0) {
        return New-ReferenceOperationResult `
            -Operation 'Start' -Success $false `
            -DetailCode $script:DetailIndeterminate -Record $null
    }

    $activeDifferent = @($scan.ValidRecords | Where-Object {
            if ($_.RecordState -eq $script:StateArmed) {
                return -not (Test-ReferenceArmedIsInactiveShadow `
                    -Record $_ -ValidRecords $scan.ValidRecords)
            }
            if ($_.IsTombstone) {
                return $false
            }
            return -not (Test-ReferenceRecordIsRetiredShadow `
                -Record $_ -ValidRecords $scan.ValidRecords)
        })
    if ($activeDifferent.Count -ne 0) {
        return New-ReferenceOperationResult `
            -Operation 'Start' -Success $false `
            -DetailCode $script:DetailSlotOccupied -Record $null
    }

    try {
        $maximumStoreGeneration = Get-ReferenceMaximumGeneration `
            -ValidRecords $scan.ValidRecords -PropertyName 'StoreGeneration'
        $intentStoreGeneration = Get-ReferenceNextNonZeroGeneration `
            $maximumStoreGeneration
        # Reserve both physical commits before writing Armed so a generation
        # exhaustion cannot strand a newly written intent by construction.
        $terminalStoreGeneration = Get-ReferenceNextNonZeroGeneration `
            $intentStoreGeneration
        $maximumRecordGeneration = Get-ReferenceMaximumGeneration `
            -ValidRecords $scan.ValidRecords -PropertyName 'RecordGeneration'
        $recordGeneration = Get-ReferenceNextNonZeroGeneration `
            $maximumRecordGeneration
    }
    catch [OverflowException] {
        return New-ReferenceOperationResult `
            -Operation 'Start' -Success $false `
            -DetailCode $script:DetailStorageUnavailable -Record $null
    }

    $targetSlot = Get-ReferenceTerminalTargetSlot -Scan $scan
    if ($targetSlot -lt 0) {
        return New-ReferenceOperationResult `
            -Operation 'Start' -Success $false `
            -DetailCode $script:DetailSlotOccupied -Record $null
    }
    $armed = New-ReferenceArmedRecordBytes `
        -Key $Key `
        -StoreGeneration $intentStoreGeneration `
        -RecordGeneration $recordGeneration
    $intentCommit = Invoke-ReferenceRecordCommit `
        -Store $Store `
        -AxisReference $Key.AxisReference `
        -Slot 0 `
        -RecordBytes $armed `
        -CrashAt $IntentCrashAt `
        -FailureAt $IntentFailureAt
    if (-not $intentCommit.Completed) {
        return New-ReferenceOperationResult `
            -Operation 'Start' -Success $false `
            -DetailCode $script:DetailStorageUnavailable -Record $null `
            -MutationCount $intentCommit.MutationCount
    }

    $nativeCount = 0
    if ($Outcome -eq 'Succeeded') {
        $nativeCount = 1
        $terminal = New-ReferenceSucceededRecordBytes `
            -Key $Key `
            -StoreGeneration $terminalStoreGeneration `
            -RecordGeneration $recordGeneration
    }
    elseif ($Outcome -eq 'NativeRejected') {
        $nativeCount = 1
        $terminal = New-ReferenceRejectedRecordBytes `
            -Key $Key `
            -StoreGeneration $terminalStoreGeneration `
            -RecordGeneration $recordGeneration `
            -DetailCode 11 `
            -ErrorId -6 `
            -NativeCommandState ([Convert]::ToUInt32('A5A5A5A5', 16))
    }
    else {
        $terminal = New-ReferenceRejectedRecordBytes `
            -Key $Key `
            -StoreGeneration $terminalStoreGeneration `
            -RecordGeneration $recordGeneration `
            -DetailCode 10 `
            -ErrorId -31000
    }
    if ($nativeCount -gt 1) {
        throw [InvalidOperationException]::new(
            'Reference model attempted more than one native call.')
    }

    $terminalCommit = Invoke-ReferenceRecordCommit `
        -Store $Store `
        -AxisReference $Key.AxisReference `
        -Slot $targetSlot `
        -RecordBytes $terminal `
        -CrashAt $TerminalCrashAt `
        -FailureAt $TerminalFailureAt
    $mutationCount = $intentCommit.MutationCount +
        $terminalCommit.MutationCount
    if (-not $terminalCommit.Completed) {
        return New-ReferenceOperationResult `
            -Operation 'Start' -Success $false -DetailCode 0 `
            -Record $null -MutationCount $mutationCount `
            -NativeCount $nativeCount -NoResponse $true -ResultCode -12
    }

    $committed = Read-ReferenceRecord `
        -Bytes $Store `
        -Offset (Get-ReferenceRecordOffset `
            -AxisReference $Key.AxisReference -Slot $targetSlot) `
        -Slot $targetSlot
    return New-ReferenceOperationResult `
        -Operation 'Start' -Success $true -DetailCode 0 `
        -Record $committed.Record -MutationCount $mutationCount `
        -NativeCount $nativeCount
}

function New-ReferenceStoreBeforeThirdRetirement {
    $store = New-ReferenceStore
    $key1 = New-ReferenceRecoveryKey `
        -OriginalRequestId 1001 -Intent0 101
    $key2 = New-ReferenceRecoveryKey `
        -OriginalRequestId 1002 -Intent0 102
    $key3 = New-ReferenceRecoveryKey `
        -OriginalRequestId 1003 -Intent0 103

    $start1 = Invoke-ReferenceStart -Store $store -Key $key1
    if (-not $start1.Success) {
        throw [InvalidOperationException]::new('Cycle setup start1 failed.')
    }
    $retire1 = Invoke-ReferenceRetirement `
        -Store $store -Key $key1 `
        -ExpectedRecordGeneration $start1.Record.RecordGeneration
    if (-not $retire1.Success) {
        throw [InvalidOperationException]::new('Cycle setup retire1 failed.')
    }

    $start2 = Invoke-ReferenceStart -Store $store -Key $key2
    if (-not $start2.Success) {
        throw [InvalidOperationException]::new('Cycle setup start2 failed.')
    }
    $retire2 = Invoke-ReferenceRetirement `
        -Store $store -Key $key2 `
        -ExpectedRecordGeneration $start2.Record.RecordGeneration
    if (-not $retire2.Success) {
        throw [InvalidOperationException]::new('Cycle setup retire2 failed.')
    }

    $start3 = Invoke-ReferenceStart -Store $store -Key $key3
    if (-not $start3.Success) {
        throw [InvalidOperationException]::new('Cycle setup start3 failed.')
    }
    return [pscustomobject][ordered]@{
        Store = $store
        Key1 = $key1
        Key2 = $key2
        Key3 = $key3
        Start3 = $start3
    }
}

Invoke-ReferenceFixture -Name 'LayoutBounds' -Body {
    Assert-ReferenceEqual 84 $script:RecordSize 'Record byte size changed.'
    Assert-ReferenceEqual 21 $script:RecordWordCount 'Record word size changed.'
    Assert-ReferenceEqual 336 $script:AxisSize 'Axis byte size changed.'
    Assert-ReferenceEqual 84 $script:AxisWordCount 'Axis word size changed.'
    Assert-ReferenceEqual 1344 $script:StoreSize 'Store byte size changed.'
    Assert-ReferenceEqual 336 $script:StoreWordCount 'Store word size changed.'
    Assert-ReferenceEqual 68 $script:TerminalSnapshotSize `
        'Store-core terminal snapshot size changed.'
    Assert-ReferenceEqual 92 $script:TerminalResponseTotalSize `
        'Service total terminal response size changed.'

    $expectedAxisWordBases = @(0, 84, 168, 252)
    $expectedSlotWordOffsets = @(0, 21, 42, 63)
    for ($axis = 1; $axis -le 4; $axis++) {
        for ($slot = 0; $slot -lt 4; $slot++) {
            $expected = $expectedAxisWordBases[$axis - 1] +
                $expectedSlotWordOffsets[$slot]
            Assert-ReferenceEqual `
                $expected `
                (Get-ReferenceRecordWordOffset `
                    -AxisReference $axis -Slot $slot) `
                'Axis-major record word offset changed.'
        }
    }
    $lastStart = Get-ReferenceRecordWordOffset -AxisReference 4 -Slot 3
    Assert-ReferenceEqual 315 $lastStart 'Last record word base changed.'
    Assert-ReferenceEqual 335 ($lastStart + 20) 'Last store word changed.'
    Assert-ReferenceThrows `
        'System.ArgumentOutOfRangeException' `
        { Get-ReferenceRecordOffset -AxisReference 0 -Slot 0 } `
        'Axis lower bound must be enforced.'
    Assert-ReferenceThrows `
        'System.ArgumentOutOfRangeException' `
        { Get-ReferenceRecordOffset -AxisReference 5 -Slot 0 } `
        'Axis upper bound must be enforced.'
    Assert-ReferenceThrows `
        'System.ArgumentOutOfRangeException' `
        { Get-ReferenceRecordOffset -AxisReference 1 -Slot 4 } `
        'Slot upper bound must be enforced.'
}

Invoke-ReferenceFixture -Name 'RawKeyAndSnapshotPublicBoundary' -Body {
    $key = New-ReferenceRecoveryKey
    $keyBytes = ConvertTo-ReferenceRecoveryKeyBytes -Key $key
    Assert-ReferenceEqual 48 $keyBytes.Length `
        'Normalized recovery key must contain exactly 48 bytes.'
    Assert-ReferenceEqual $key.SchemaVersion `
        (Get-ReferenceUInt16LE $keyBytes 0) `
        'Recovery key schema offset changed.'
    Assert-ReferenceEqual $key.SemanticMode `
        (Get-ReferenceUInt16LE $keyBytes 2) `
        'Recovery key semantic offset changed.'
    Assert-ReferenceEqual $key.AxisReference `
        (Get-ReferenceUInt16LE $keyBytes 36) `
        'Recovery key axis offset changed.'
    Assert-ReferenceEqual 0 (Get-ReferenceUInt16LE $keyBytes 38) `
        'Recovery key Reserved must be zero.'
    Assert-ReferenceEqual $key.TargetPosition `
        (Get-ReferenceInt32LE $keyBytes 40) `
        'Recovery key target offset changed.'
    Assert-ReferenceEqual $key.ExpectedActualPosition `
        (Get-ReferenceInt32LE $keyBytes 44) `
        'Recovery key expected-actual offset changed.'
    $decoded = Get-ReferenceRecoveryKeyBoundary `
        -Key $null -KeyBytes $keyBytes -KeySize 48
    Assert-ReferenceTrue $decoded.IsValid `
        'Canonical 48-byte recovery key must decode.'
    Assert-ReferenceTrue `
        (Test-ReferenceRecoveryKeysEqual $key $decoded.Key) `
        'Raw 48-byte recovery key round trip changed.'

    foreach ($invalidSize in @(47, 49)) {
        $store = New-ReferenceStore
        $before = Copy-ReferenceBytes $store
        $transaction = New-ReferenceSetPositionTransaction
        $result = Invoke-ReferenceBeginSetPosition `
            -Store $store -Transaction $transaction -Key $key `
            -KeyBytes $keyBytes -KeySize $invalidSize
        Assert-ReferenceEqual -1 $result.ResultCode `
            ('KeySize ' + $invalidSize + ' must fail at the boundary.')
        Assert-ReferenceEqual 0 $result.DetailCode `
            ('KeySize ' + $invalidSize + ' failure must remain internal.')
        Assert-ReferenceEqual 0 $result.MutationCount `
            ('KeySize ' + $invalidSize + ' failure must be zero-write.')
        Assert-ReferenceTrue (-not $transaction.Active) `
            ('KeySize ' + $invalidSize + ' may not arm a transaction.')
        Assert-ReferenceTrue (Test-ReferenceAllZero $result.SnapshotBytes) `
            ('KeySize ' + $invalidSize + ' must zero the snapshot output.')
        Assert-ReferenceBytesEqual $before $store `
            ('KeySize ' + $invalidSize + ' must preserve retained bytes.')
    }

    foreach ($bufferLength in @(47, 49)) {
        $driftedBuffer = New-Object byte[] $bufferLength
        [Array]::Copy(
            $keyBytes, 0, $driftedBuffer, 0,
            [Math]::Min($keyBytes.Length, $driftedBuffer.Length))
        $store = New-ReferenceStore
        $transaction = New-ReferenceSetPositionTransaction
        $result = Invoke-ReferenceBeginSetPosition `
            -Store $store -Transaction $transaction -Key $null `
            -KeyBytes $driftedBuffer -KeySize 48
        Assert-ReferenceEqual -1 $result.ResultCode `
            ('Raw key buffer length ' + $bufferLength +
             ' must fail at the boundary.')
        Assert-ReferenceEqual 0 $result.MutationCount `
            ('Raw key buffer length ' + $bufferLength +
             ' failure must be zero-write.')
    }

    $reservedKey = Copy-ReferenceBytes $keyBytes
    Set-ReferenceUInt16LE $reservedKey 38 1
    $reservedStore = New-ReferenceStore
    $reservedTransaction = New-ReferenceSetPositionTransaction
    $reservedResult = Invoke-ReferenceBeginSetPosition `
        -Store $reservedStore -Transaction $reservedTransaction -Key $null `
        -KeyBytes $reservedKey -KeySize 48
    Assert-ReferenceEqual -1 $reservedResult.ResultCode `
        'Nonzero raw-key Reserved must fail at the boundary.'
    Assert-ReferenceEqual 0 $reservedResult.MutationCount `
        'Nonzero raw-key Reserved must be zero-write.'
    Assert-ReferenceTrue (-not $reservedTransaction.Active) `
        'Nonzero raw-key Reserved may not arm a transaction.'

    $store = New-ReferenceStore
    $transaction = New-ReferenceSetPositionTransaction
    $begin = Invoke-ReferenceBeginSetPosition `
        -Store $store -Transaction $transaction -Key $key `
        -KeyBytes $keyBytes -KeySize 48
    Assert-ReferenceEqual 1 $begin.ResultCode `
        'Valid raw key must admit staged Begin.'
    Assert-ReferenceEqual 68 $begin.SnapshotBytes.Length `
        'Every public snapshot output must contain exactly 68 bytes.'
    Assert-ReferenceTrue (Test-ReferenceAllZero $begin.SnapshotBytes) `
        'New Armed Begin must leave the terminal snapshot zeroed.'
    $commit = Invoke-ReferenceCommitSetPositionTerminal `
        -Store $store -Transaction $transaction -Key $key `
        -KeyBytes $keyBytes -KeySize 48 `
        -RecordGeneration $begin.Record.RecordGeneration `
        -RecordState $script:StateSucceeded `
        -AppliedPosition $key.TargetPosition `
        -OriginalCommandStatus 0 -OriginalErrorId 0 `
        -OriginalDetailCode 0 -NativeCommandState 0
    Assert-ReferenceEqual 1 $commit.ResultCode `
        'Valid staged terminal Commit must succeed.'
    Assert-ReferenceBytesEqual `
        (Get-ReferenceRegion `
            -Bytes $commit.Record.Bytes -Offset 8 -Length 68) `
        $commit.SnapshotBytes `
        'Commit snapshot must be exact retained bytes 8..75.'
    $read = Invoke-ReferenceReadSetPositionOutcome `
        -Store $store -Transaction $transaction -Key $key `
        -KeyBytes $keyBytes -KeySize 48
    Assert-ReferenceBytesEqual $commit.SnapshotBytes $read.SnapshotBytes `
        'Read snapshot must match the committed terminal bytes.'
    $retireBefore = Copy-ReferenceBytes $store
    $zeroGeneration = Invoke-ReferenceRetireSetPositionOutcome `
        -Store $store -Transaction $transaction -Key $key `
        -KeyBytes $keyBytes -KeySize 48 `
        -ExpectedRecordGeneration 0
    Assert-ReferenceEqual -1 $zeroGeneration.ResultCode `
        'Zero retirement generation must fail before scan.'
    Assert-ReferenceEqual 0 $zeroGeneration.DetailCode `
        'Zero retirement generation must remain an internal boundary failure.'
    Assert-ReferenceEqual 0 $zeroGeneration.MutationCount `
        'Zero retirement generation must be zero-write.'
    Assert-ReferenceBytesEqual $retireBefore $store `
        'Zero retirement generation must preserve retained bytes.'
    Assert-ReferenceTrue (Test-ReferenceAllZero $zeroGeneration.SnapshotBytes) `
        'Zero retirement generation must leave the snapshot zeroed.'
    $retire = Invoke-ReferenceRetireSetPositionOutcome `
        -Store $store -Transaction $transaction -Key $key `
        -KeyBytes $keyBytes -KeySize 48 `
        -ExpectedRecordGeneration $begin.Record.RecordGeneration
    Assert-ReferenceEqual 1 $retire.ResultCode `
        'Valid staged retirement must succeed.'
    Assert-ReferenceBytesEqual `
        (Get-ReferenceRegion `
            -Bytes $retire.Record.Bytes -Offset 8 -Length 68) `
        $retire.SnapshotBytes `
        'Retirement snapshot must be exact retained bytes 8..75.'

    $failureStore = New-ReferenceStore
    $failureRead = Invoke-ReferenceReadSetPositionOutcome `
        -Store $failureStore `
        -Transaction (New-ReferenceSetPositionTransaction) `
        -Key $key -KeyBytes $keyBytes -KeySize 48
    Assert-ReferenceEqual 19 $failureRead.DetailCode `
        'Blank staged Read must return detail 19.'
    Assert-ReferenceTrue (Test-ReferenceAllZero $failureRead.SnapshotBytes) `
        'Domain failure must keep the 68-byte snapshot zeroed.'

    $driftStore = New-ReferenceStore
    $driftTransaction = New-ReferenceSetPositionTransaction
    $driftBegin = Invoke-ReferenceBeginSetPosition `
        -Store $driftStore -Transaction $driftTransaction -Key $key `
        -KeyBytes $keyBytes -KeySize 48
    $driftedKey = Copy-ReferenceBytes $keyBytes
    $driftedKey[20] = [byte]($driftedKey[20] -bxor 1)
    $driftCommit = Invoke-ReferenceCommitSetPositionTerminal `
        -Store $driftStore -Transaction $driftTransaction -Key $key `
        -KeyBytes $driftedKey -KeySize 48 `
        -RecordGeneration $driftBegin.Record.RecordGeneration `
        -RecordState $script:StateSucceeded `
        -AppliedPosition $key.TargetPosition `
        -OriginalCommandStatus 0 -OriginalErrorId 0 `
        -OriginalDetailCode 0 -NativeCommandState 0
    Assert-ReferenceEqual -12 $driftCommit.ResultCode `
        'Raw key drift after durable Begin must return exact -12.'
    Assert-ReferenceTrue $driftCommit.NoResponse `
        'Raw key drift after durable Begin must close without response.'
    Assert-ReferenceTrue (-not $driftTransaction.Active) `
        'Raw key drift Commit must consume the transaction.'
    Assert-ReferenceTrue (Test-ReferenceAllZero $driftCommit.SnapshotBytes) `
        'Failed terminal Commit must keep the snapshot zeroed.'
}

Invoke-ReferenceFixture -Name 'LittleEndianCodec' -Body {
    $bytes = New-Object byte[] 12
    Set-ReferenceUInt16LE $bytes 0 0x1234
    Set-ReferenceUInt32LE `
        $bytes 2 ([Convert]::ToUInt32('89ABCDEF', 16))
    Set-ReferenceInt16LE $bytes 6 ([int16]-6)
    Set-ReferenceInt32LE $bytes 8 ([int32]-654321)

    $expectedPrefix = @(0x34, 0x12, 0xEF, 0xCD, 0xAB, 0x89)
    for ($index = 0; $index -lt $expectedPrefix.Count; $index++) {
        Assert-ReferenceEqual `
            $expectedPrefix[$index] $bytes[$index] `
            'Little-endian byte order changed.'
    }
    Assert-ReferenceEqual 0x1234 `
        (Get-ReferenceUInt16LE $bytes 0) `
        'UInt16 little-endian round trip failed.'
    Assert-ReferenceEqual `
        ([Convert]::ToUInt32('89ABCDEF', 16)) `
        (Get-ReferenceUInt32LE $bytes 2) `
        'UInt32 little-endian round trip failed.'
    Assert-ReferenceEqual -6 `
        (Get-ReferenceInt16LE $bytes 6) `
        'Int16 little-endian round trip failed.'
    Assert-ReferenceEqual -654321 `
        (Get-ReferenceInt32LE $bytes 8) `
        'Int32 little-endian round trip failed.'
}

Invoke-ReferenceFixture -Name 'TestOnlyCrcOracle' -Body {
    $vector = [Text.Encoding]::ASCII.GetBytes('123456789')
    $actual = Get-TestOnlyOracleCrc32 `
        -Bytes $vector -Offset 0 -Length $vector.Length -Seed 0
    Assert-ReferenceEqual `
        ([Convert]::ToUInt32('CBF43926', 16)) $actual `
        'Test-only IEEE CRC32 oracle vector changed.'

    $record = New-ReferenceSucceededRecordBytes `
        -Key (New-ReferenceRecoveryKey) `
        -StoreGeneration 1 -RecordGeneration 1
    $stored = Get-ReferenceUInt32LE $record 76
    $computed = Get-TestOnlyOracleCrc32 `
        -Bytes $record -Offset 0 -Length 76 -Seed 0
    Assert-ReferenceEqual $computed $stored `
        'Record CRC must cover exactly bytes 0..75 with seed zero.'
}

Invoke-ReferenceFixture -Name 'ValidRecordCodec' -Body {
    $key = New-ReferenceRecoveryKey
    $successBytes = New-ReferenceSucceededRecordBytes `
        -Key $key -StoreGeneration 7 -RecordGeneration 9
    $success = Read-ReferenceRecord -Bytes $successBytes -Slot 2
    Assert-ReferenceEqual 'Valid' $success.Classification `
        'Succeeded record must decode as valid.'
    Assert-ReferenceEqual 7 $success.Record.StoreGeneration `
        'Store generation did not round trip.'
    Assert-ReferenceEqual 9 $success.Record.RecordGeneration `
        'Record generation did not round trip.'
    Assert-ReferenceEqual $key.TargetPosition `
        $success.Record.AppliedPosition `
        'Succeeded snapshot did not round trip.'
    Assert-ReferenceEqual 2 $success.Record.Slot `
        'Decoded physical slot was not retained.'

    $nativeState = [Convert]::ToUInt32('A5A5A5A5', 16)
    $rejectedBytes = New-ReferenceRejectedRecordBytes `
        -Key $key -StoreGeneration 8 -RecordGeneration 10 `
        -DetailCode 11 -ErrorId -6 -NativeCommandState $nativeState
    $rejected = Read-ReferenceRecord -Bytes $rejectedBytes
    Assert-ReferenceEqual 'Valid' $rejected.Classification `
        'Native-rejected record must decode as valid.'
    Assert-ReferenceEqual 1 $rejected.Record.OriginalCommandStatus `
        'Rejected command status did not round trip.'
    Assert-ReferenceEqual -6 $rejected.Record.OriginalErrorId `
        'Rejected error did not round trip.'
    Assert-ReferenceEqual 11 $rejected.Record.OriginalDetailCode `
        'Rejected detail did not round trip.'
    Assert-ReferenceEqual $nativeState $rejected.Record.NativeCommandState `
        'Rejected native state did not round trip.'
}

Invoke-ReferenceFixture -Name 'LiteralTestOracleGoldenRecord' -Body {
    # Literal ABI oracle for this executable reference model only. The CRC
    # bytes are not claimed to be a vendor LDR_CRC32_BufferEx golden vector.
    $literalHex =
        '01000000070000000200010004030201443322118877665540302010' +
        '111111112222222233333333444444440100000040E201000F04F6FF' +
        '40E2010000000000000000000000000009000000F9532558DEC0127D'
    $literal = Convert-ReferenceHexToBytes -Hex $literalHex
    $encoded = New-ReferenceSucceededRecordBytes `
        -Key (New-ReferenceRecoveryKey) `
        -StoreGeneration 7 -RecordGeneration 9

    Assert-ReferenceEqual 84 $literal.Length `
        'Literal ABI record must contain exactly 84 bytes.'
    Assert-ReferenceBytesEqual $literal $encoded `
        'Canonical record bytes changed from the literal ABI oracle.'
    Assert-ReferenceEqual `
        ([Convert]::ToUInt32('582553F9', 16)) `
        (Get-ReferenceUInt32LE $literal 76) `
        'Literal test-only CRC bytes changed.'
    Assert-ReferenceEqual $script:CommitMarker `
        (Get-ReferenceUInt32LE $literal 80) `
        'Literal commit marker bytes changed.'
    Assert-ReferenceEqual 'Valid' `
        (Read-ReferenceRecord -Bytes $literal).Classification `
        'Literal ABI record must pass full record validation.'
}

Invoke-ReferenceFixture -Name 'BlankStore' -Body {
    $store = New-ReferenceStore
    Assert-ReferenceEqual 1344 $store.Length 'Blank store length changed.'
    $scan = Get-ReferenceAxisScan -Store $store -AxisReference 1
    Assert-ReferenceEqual 4 $scan.Entries.Count `
        'Blank scan must return four records.'
    foreach ($entry in $scan.Entries) {
        Assert-ReferenceEqual 'Blank' $entry.Classification `
            'All-zero record must classify as blank.'
    }
    $query = Invoke-ReferenceQuery `
        -Store $store -Key (New-ReferenceRecoveryKey)
    Assert-ReferenceTrue (-not $query.Success) `
        'Blank store query must not succeed.'
    Assert-ReferenceEqual 19 $query.DetailCode `
        'Blank store must return detail 19.'
    Assert-ReferenceEqual 0 $query.MutationCount `
        'Blank query must not write.'
    Assert-ReferenceEqual 0 $query.NativeCount `
        'Blank query must not call native motion.'
}

Invoke-ReferenceFixture -Name 'IncompleteMarkerIgnored' -Body {
    $key = New-ReferenceRecoveryKey
    $incomplete = New-ReferenceSucceededRecordBytes `
        -Key $key -StoreGeneration $script:UInt32Max `
        -RecordGeneration $script:UInt32Max
    Set-ReferenceUInt32LE $incomplete $script:CommitMarkerOffset 0
    $store = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $store -AxisReference 1 -Slot 1 `
        -RecordBytes $incomplete

    $read = Read-ReferenceRecord -Bytes $incomplete
    Assert-ReferenceEqual 'Incomplete' $read.Classification `
        'Marker-zero record must classify as incomplete.'
    $scan = Get-ReferenceAxisScan -Store $store -AxisReference 1
    Assert-ReferenceEqual 0 $scan.ValidRecords.Count `
        'Incomplete record must not enter the committed set.'
    Assert-ReferenceEqual 0 `
        (Get-ReferenceMaximumGeneration `
            -ValidRecords $scan.ValidRecords `
            -PropertyName 'StoreGeneration') `
        'Incomplete generation must be ignored.'
    Assert-ReferenceEqual 1 `
        (Get-ReferenceNextNonZeroGeneration 0) `
        'Ignored incomplete generation must remain reusable.'
    $query = Invoke-ReferenceQuery -Store $store -Key $key
    Assert-ReferenceEqual 19 $query.DetailCode `
        'Incomplete-only store must return detail 19.'
}

Invoke-ReferenceFixture -Name 'UnknownMarkerCorrupt' -Body {
    $key = New-ReferenceRecoveryKey
    $record = New-ReferenceSucceededRecordBytes `
        -Key $key -StoreGeneration 1 -RecordGeneration 1
    Set-ReferenceUInt32LE $record $script:CommitMarkerOffset 1
    $store = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $store -AxisReference 1 -Slot 1 -RecordBytes $record

    $read = Read-ReferenceRecord -Bytes $record
    Assert-ReferenceEqual 'Corrupt' $read.Classification `
        'Unknown nonzero marker must classify as corrupt.'
    Assert-ReferenceEqual 'UnknownMarker' $read.Reason `
        'Unknown marker reason changed.'
    $query = Invoke-ReferenceQuery -Store $store -Key $key
    Assert-ReferenceEqual 21 $query.DetailCode `
        'Unknown marker must return detail 21.'
}

Invoke-ReferenceFixture -Name 'BadCrcCorrupt' -Body {
    $key = New-ReferenceRecoveryKey
    $record = New-ReferenceSucceededRecordBytes `
        -Key $key -StoreGeneration 1 -RecordGeneration 1
    $record[12] = [byte]($record[12] -bxor 1)
    $store = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $store -AxisReference 1 -Slot 1 -RecordBytes $record

    $read = Read-ReferenceRecord -Bytes $record
    Assert-ReferenceEqual 'Corrupt' $read.Classification `
        'Tampered body must classify as corrupt.'
    Assert-ReferenceEqual 'CrcMismatch' $read.Reason `
        'Tampered body must fail CRC before schema interpretation.'
    $query = Invoke-ReferenceQuery -Store $store -Key $key
    Assert-ReferenceEqual 21 $query.DetailCode `
        'Bad CRC must return detail 21.'
}

Invoke-ReferenceFixture -Name 'InvalidCommittedFieldMatrix' -Body {
    $key = New-ReferenceRecoveryKey
    $cases = @(
        [pscustomobject]@{ Name = 'StoreSchema'; Base = 'Success';
            Reason = 'StoreSchema'; Mutate = {
                param([byte[]]$b) Set-ReferenceUInt16LE $b 0 2 } },
        [pscustomobject]@{ Name = 'StoreFlags'; Base = 'Success';
            Reason = 'StoreFlags'; Mutate = {
                param([byte[]]$b) Set-ReferenceUInt16LE $b 2 2 } },
        [pscustomobject]@{ Name = 'StoreGeneration'; Base = 'Success';
            Reason = 'StoreGeneration'; Mutate = {
                param([byte[]]$b) Set-ReferenceUInt32LE $b 4 0 } },
        [pscustomobject]@{ Name = 'RecordState'; Base = 'Success';
            Reason = 'RecordState'; Mutate = {
                param([byte[]]$b) Set-ReferenceUInt16LE $b 8 9 } },
        [pscustomobject]@{ Name = 'SemanticMode'; Base = 'Success';
            Reason = 'SemanticMode'; Mutate = {
                param([byte[]]$b) Set-ReferenceUInt16LE $b 10 2 } },
        [pscustomobject]@{ Name = 'RecoveryIdentity'; Base = 'Success';
            Reason = 'RecoveryIdentity'; Mutate = {
                param([byte[]]$b) Set-ReferenceUInt32LE $b 12 0 } },
        [pscustomobject]@{ Name = 'ClientIntent'; Base = 'Success';
            Reason = 'ClientIntent'; Mutate = {
                param([byte[]]$b)
                foreach ($offset in @(28, 32, 36, 40)) {
                    Set-ReferenceUInt32LE $b $offset 0
                }
            } },
        [pscustomobject]@{ Name = 'AxisReference'; Base = 'Success';
            Reason = 'AxisReference'; Mutate = {
                param([byte[]]$b) Set-ReferenceUInt16LE $b 44 5 } },
        [pscustomobject]@{ Name = 'Reserved'; Base = 'Success';
            Reason = 'Reserved'; Mutate = {
                param([byte[]]$b) Set-ReferenceUInt16LE $b 46 1 } },
        [pscustomobject]@{ Name = 'RecordGeneration'; Base = 'Success';
            Reason = 'RecordGeneration'; Mutate = {
                param([byte[]]$b) Set-ReferenceUInt32LE $b 72 0 } },
        [pscustomobject]@{ Name = 'SucceededPayload'; Base = 'Success';
            Reason = 'SucceededPayload'; Mutate = {
                param([byte[]]$b) Set-ReferenceInt32LE $b 56 123457 } },
        [pscustomobject]@{ Name = 'ArmedPayload'; Base = 'Armed';
            Reason = 'ArmedPayload'; Mutate = {
                param([byte[]]$b) Set-ReferenceInt32LE $b 56 1 } },
        [pscustomobject]@{ Name = 'ArmedTombstone'; Base = 'Armed';
            Reason = 'ArmedPayload'; Mutate = {
                param([byte[]]$b) Set-ReferenceUInt16LE $b 2 1 } },
        [pscustomobject]@{ Name = 'RejectedPayload'; Base = 'Rejected';
            Reason = 'RejectedPayload'; Mutate = {
                param([byte[]]$b) Set-ReferenceUInt32LE $b 64 9 } }
    )
    Assert-ReferenceEqual 14 $cases.Count `
        'Invalid-field matrix case count changed.'
    foreach ($case in $cases) {
        if ($case.Base -eq 'Armed') {
            $candidate = New-ReferenceArmedRecordBytes `
                -Key $key -StoreGeneration 1 -RecordGeneration 1
        }
        elseif ($case.Base -eq 'Rejected') {
            $candidate = New-ReferenceRejectedRecordBytes `
                -Key $key -StoreGeneration 1 -RecordGeneration 1
        }
        else {
            $candidate = New-ReferenceSucceededRecordBytes `
                -Key $key -StoreGeneration 1 -RecordGeneration 1
        }
        $mutator = $case.Mutate
        & $mutator $candidate
        Update-ReferenceRecordOracleCrc -RecordBytes $candidate
        $read = Read-ReferenceRecord -Bytes $candidate
        Assert-ReferenceEqual 'Corrupt' $read.Classification `
            ($case.Name + ' must classify as corrupt.')
        Assert-ReferenceEqual $case.Reason $read.Reason `
            ($case.Name + ' corruption reason changed.')
    }
}

Invoke-ReferenceFixture -Name 'SlotRoleInvariant' -Body {
    $key = New-ReferenceRecoveryKey

    $intentStore = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $intentStore -AxisReference 1 -Slot 0 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key -StoreGeneration 1 -RecordGeneration 1)
    $intentScan = Get-ReferenceAxisScan `
        -Store $intentStore -AxisReference 1
    Assert-ReferenceTrue $intentScan.IsCorrupt `
        'Intent slot may contain only Armed.'
    Assert-ReferenceTrue `
        ($intentScan.CorruptReasons -contains 'IntentSlotRole') `
        'Intent slot role corruption reason is missing.'
    Assert-ReferenceEqual 21 `
        (Invoke-ReferenceQuery -Store $intentStore -Key $key).DetailCode `
        'Terminal state in Intent slot must return detail 21.'

    $terminalStore = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $terminalStore -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceArmedRecordBytes `
            -Key $key -StoreGeneration 1 -RecordGeneration 1)
    $terminalScan = Get-ReferenceAxisScan `
        -Store $terminalStore -AxisReference 1
    Assert-ReferenceTrue $terminalScan.IsCorrupt `
        'Terminal A/B/C slots may not contain Armed.'
    Assert-ReferenceTrue `
        ($terminalScan.CorruptReasons -contains 'TerminalSlotRole') `
        'Terminal slot role corruption reason is missing.'
    Assert-ReferenceEqual 21 `
        (Invoke-ReferenceQuery -Store $terminalStore -Key $key).DetailCode `
        'Armed state in terminal bank must return detail 21.'
}

Invoke-ReferenceFixture -Name 'DifferentKeyRecordGenerationInvariant' -Body {
    $key1 = New-ReferenceRecoveryKey `
        -OriginalRequestId 6101 -Intent0 611
    $key2 = New-ReferenceRecoveryKey `
        -OriginalRequestId 6102 -Intent0 612
    $store = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $store -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key1 -StoreGeneration 1 -RecordGeneration 9 `
            -Tombstone)
    Set-ReferenceStoreRecordDirect `
        -Store $store -AxisReference 1 -Slot 2 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key2 -StoreGeneration 2 -RecordGeneration 9 `
            -Tombstone)

    $scan = Get-ReferenceAxisScan -Store $store -AxisReference 1
    Assert-ReferenceTrue $scan.IsCorrupt `
        'Different keys may not reuse one RecordGeneration.'
    Assert-ReferenceTrue `
        ($scan.CorruptReasons -contains 'DifferentKeyRecordGenerationReuse') `
        'Different-key RecordGeneration reuse reason is missing.'
    Assert-ReferenceEqual 21 `
        (Invoke-ReferenceQuery -Store $store -Key $key1).DetailCode `
        'Different-key RecordGeneration reuse must return detail 21.'
}

Invoke-ReferenceFixture -Name 'ExactKeyRecordGenerationInvariant' -Body {
    $key = New-ReferenceRecoveryKey

    $armedTerminalStore = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $armedTerminalStore -AxisReference 1 -Slot 0 `
        -RecordBytes (New-ReferenceArmedRecordBytes `
            -Key $key -StoreGeneration 1 -RecordGeneration 7)
    Set-ReferenceStoreRecordDirect `
        -Store $armedTerminalStore -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key -StoreGeneration 2 -RecordGeneration 8)
    $armedTerminalScan = Get-ReferenceAxisScan `
        -Store $armedTerminalStore -AxisReference 1
    Assert-ReferenceTrue $armedTerminalScan.IsCorrupt `
        'Exact Armed and terminal must share RecordGeneration.'
    Assert-ReferenceTrue `
        ($armedTerminalScan.CorruptReasons -contains `
            'ExactKeyRecordGenerationMismatch') `
        'Exact Armed/terminal generation mismatch reason is missing.'
    Assert-ReferenceEqual 21 `
        (Invoke-ReferenceQuery `
            -Store $armedTerminalStore -Key $key).DetailCode `
        'Exact Armed/terminal generation mismatch must return detail 21.'

    $terminalTombstoneStore = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $terminalTombstoneStore -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key -StoreGeneration 1 -RecordGeneration 7)
    Set-ReferenceStoreRecordDirect `
        -Store $terminalTombstoneStore -AxisReference 1 -Slot 2 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key -StoreGeneration 2 -RecordGeneration 8 `
            -Tombstone)
    $terminalTombstoneScan = Get-ReferenceAxisScan `
        -Store $terminalTombstoneStore -AxisReference 1
    Assert-ReferenceTrue $terminalTombstoneScan.IsCorrupt `
        'Exact terminal and tombstone must share RecordGeneration.'
    Assert-ReferenceTrue `
        ($terminalTombstoneScan.CorruptReasons -contains `
            'ExactKeyRecordGenerationMismatch') `
        'Exact terminal/tombstone generation mismatch reason is missing.'
}

Invoke-ReferenceFixture -Name 'TerminalAfterMatchingArmedInvariant' -Body {
    $key = New-ReferenceRecoveryKey
    $store = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $store -AxisReference 1 -Slot 0 `
        -RecordBytes (New-ReferenceArmedRecordBytes `
            -Key $key -StoreGeneration 5 -RecordGeneration 7)
    Set-ReferenceStoreRecordDirect `
        -Store $store -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key -StoreGeneration 4 -RecordGeneration 7)

    $scan = Get-ReferenceAxisScan -Store $store -AxisReference 1
    Assert-ReferenceTrue $scan.IsCorrupt `
        'Matching terminal must physically follow Armed.'
    Assert-ReferenceTrue `
        ($scan.CorruptReasons -contains 'TerminalNotAfterMatchingArmed') `
        'Terminal/Armed ordering reason is missing.'
    Assert-ReferenceEqual 21 `
        (Invoke-ReferenceQuery -Store $store -Key $key).DetailCode `
        'Reversed terminal/Armed order must return detail 21.'

    $validStore = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $validStore -AxisReference 1 -Slot 0 `
        -RecordBytes (New-ReferenceArmedRecordBytes `
            -Key $key -StoreGeneration 5 -RecordGeneration 7)
    Set-ReferenceStoreRecordDirect `
        -Store $validStore -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key -StoreGeneration 6 -RecordGeneration 7)
    $validScan = Get-ReferenceAxisScan `
        -Store $validStore -AxisReference 1
    Assert-ReferenceTrue (-not $validScan.IsCorrupt) `
        'Matching terminal after Armed must remain valid.'
    Assert-ReferenceTrue `
        (Invoke-ReferenceQuery -Store $validStore -Key $key).Success `
        'Matching terminal must win over its older Armed.'
}

Invoke-ReferenceFixture -Name 'OlderTombstoneNewerTerminalInvariant' -Body {
    $key = New-ReferenceRecoveryKey
    $store = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $store -AxisReference 1 -Slot 0 `
        -RecordBytes (New-ReferenceArmedRecordBytes `
            -Key $key -StoreGeneration 5 -RecordGeneration 7)
    Set-ReferenceStoreRecordDirect `
        -Store $store -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key -StoreGeneration 4 -RecordGeneration 7 `
            -Tombstone)
    Set-ReferenceStoreRecordDirect `
        -Store $store -AxisReference 1 -Slot 2 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key -StoreGeneration 6 -RecordGeneration 7)

    $scan = Get-ReferenceAxisScan -Store $store -AxisReference 1
    Assert-ReferenceTrue $scan.IsCorrupt `
        'Older matching tombstone may not precede Armed.'
    Assert-ReferenceTrue `
        ($scan.CorruptReasons -contains 'TerminalNotAfterMatchingArmed') `
        'Older-tombstone/newer-terminal ordering reason is missing.'
    Assert-ReferenceEqual 21 `
        (Invoke-ReferenceQuery -Store $store -Key $key).DetailCode `
        'Newer terminal may not hide an older-than-Armed tombstone.'
}

Invoke-ReferenceFixture -Name 'TombstoneMustFollowMatchingTerminal' -Body {
    $key = New-ReferenceRecoveryKey
    $store = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $store -AxisReference 1 -Slot 0 `
        -RecordBytes (New-ReferenceArmedRecordBytes `
            -Key $key -StoreGeneration 1 -RecordGeneration 7)
    Set-ReferenceStoreRecordDirect `
        -Store $store -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key -StoreGeneration 2 -RecordGeneration 7 `
            -Tombstone)
    Set-ReferenceStoreRecordDirect `
        -Store $store -AxisReference 1 -Slot 2 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key -StoreGeneration 3 -RecordGeneration 7)

    $scan = Get-ReferenceAxisScan -Store $store -AxisReference 1
    Assert-ReferenceTrue $scan.IsCorrupt `
        'A newer non-tombstone terminal may not undo retirement.'
    Assert-ReferenceTrue `
        ($scan.CorruptReasons -contains `
            'TombstoneNotAfterMatchingTerminal') `
        'Reverse terminal/tombstone ordering reason is missing.'
    Assert-ReferenceEqual 21 `
        (Invoke-ReferenceQuery -Store $store -Key $key).DetailCode `
        'A terminal newer than its tombstone must return detail 21.'
}

Invoke-ReferenceFixture -Name 'ActiveTerminalRequiresMatchingArmed' -Body {
    $key = New-ReferenceRecoveryKey
    $terminalOnlyStore = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $terminalOnlyStore -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key -StoreGeneration 2 -RecordGeneration 1)
    $terminalOnlyScan = Get-ReferenceAxisScan `
        -Store $terminalOnlyStore -AxisReference 1
    Assert-ReferenceTrue $terminalOnlyScan.IsCorrupt `
        'An unsuperseded active terminal without Armed must be corrupt.'
    Assert-ReferenceTrue `
        ($terminalOnlyScan.CorruptReasons -contains `
            'ActiveTerminalWithoutExactlyOneMatchingArmed') `
        'Active-terminal/matching-Armed invariant reason is missing.'
    Assert-ReferenceEqual 21 `
        (Invoke-ReferenceQuery `
            -Store $terminalOnlyStore -Key $key).DetailCode `
        'Terminal-only active state must return detail 21.'

    $activeStore = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $activeStore -AxisReference 1 -Slot 0 `
        -RecordBytes (New-ReferenceArmedRecordBytes `
            -Key $key -StoreGeneration 1 -RecordGeneration 1)
    Set-ReferenceStoreRecordDirect `
        -Store $activeStore -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key -StoreGeneration 2 -RecordGeneration 1)
    Assert-ReferenceTrue `
        (-not (Get-ReferenceAxisScan `
            -Store $activeStore -AxisReference 1).IsCorrupt) `
        'One exact lower-generation Armed must validate an active terminal.'
    Assert-ReferenceTrue `
        (Invoke-ReferenceQuery -Store $activeStore -Key $key).Success `
        'Validated active terminal must remain queryable.'

    $retiredHistoryStore = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $retiredHistoryStore -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key -StoreGeneration 1 -RecordGeneration 1)
    Set-ReferenceStoreRecordDirect `
        -Store $retiredHistoryStore -AxisReference 1 -Slot 2 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key -StoreGeneration 2 -RecordGeneration 1 `
            -Tombstone)
    Assert-ReferenceTrue `
        (-not (Get-ReferenceAxisScan `
            -Store $retiredHistoryStore -AxisReference 1).IsCorrupt) `
        'A terminal retired by a matching tombstone needs no Armed.'
    Assert-ReferenceTrue `
        (Invoke-ReferenceQuery `
            -Store $retiredHistoryStore -Key $key).Record.IsTombstone `
        'Retired history must select its matching tombstone.'

    $tombstoneOnlyStore = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $tombstoneOnlyStore -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key -StoreGeneration 1 -RecordGeneration 1 `
            -Tombstone)
    Assert-ReferenceTrue `
        (-not (Get-ReferenceAxisScan `
            -Store $tombstoneOnlyStore -AxisReference 1).IsCorrupt) `
        'Tombstone-only history must remain valid without Armed.'
    Assert-ReferenceTrue `
        (Invoke-ReferenceQuery `
            -Store $tombstoneOnlyStore -Key $key).Success `
        'Tombstone-only history must remain queryable.'
}

Invoke-ReferenceFixture -Name 'ActiveStateGraphInvariant' -Body {
    $key1 = New-ReferenceRecoveryKey `
        -OriginalRequestId 6201 -Intent0 621
    $key2 = New-ReferenceRecoveryKey `
        -OriginalRequestId 6202 -Intent0 622

    $twoTerminalStore = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $twoTerminalStore -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key1 -StoreGeneration 1 -RecordGeneration 1)
    Set-ReferenceStoreRecordDirect `
        -Store $twoTerminalStore -AxisReference 1 -Slot 2 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key2 -StoreGeneration 2 -RecordGeneration 2)
    $twoTerminalScan = Get-ReferenceAxisScan `
        -Store $twoTerminalStore -AxisReference 1
    Assert-ReferenceTrue $twoTerminalScan.IsCorrupt `
        'Two unsuperseded terminals must corrupt the axis store.'
    Assert-ReferenceTrue `
        ($twoTerminalScan.CorruptReasons -contains `
            'MultipleUnsupersededTerminals') `
        'Multiple-unsuperseded-terminal reason is missing.'

    $armedDifferentStore = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $armedDifferentStore -AxisReference 1 -Slot 0 `
        -RecordBytes (New-ReferenceArmedRecordBytes `
            -Key $key2 -StoreGeneration 3 -RecordGeneration 2)
    Set-ReferenceStoreRecordDirect `
        -Store $armedDifferentStore -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key1 -StoreGeneration 2 -RecordGeneration 1)
    $armedDifferentScan = Get-ReferenceAxisScan `
        -Store $armedDifferentStore -AxisReference 1
    Assert-ReferenceTrue $armedDifferentScan.IsCorrupt `
        'Armed plus different active terminal must corrupt the store.'
    Assert-ReferenceTrue `
        ($armedDifferentScan.CorruptReasons -contains `
            'ArmedWithDifferentActiveTerminal') `
        'Armed/different-active-terminal reason is missing.'
    Assert-ReferenceEqual 21 `
        (Invoke-ReferenceQuery `
            -Store $armedDifferentStore -Key $key2).DetailCode `
        'Invalid active graph must beat exact Armed detail 20.'

    $retiredHistoryStore = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $retiredHistoryStore -AxisReference 1 -Slot 0 `
        -RecordBytes (New-ReferenceArmedRecordBytes `
            -Key $key2 -StoreGeneration 3 -RecordGeneration 2)
    Set-ReferenceStoreRecordDirect `
        -Store $retiredHistoryStore -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key1 -StoreGeneration 1 -RecordGeneration 1)
    Set-ReferenceStoreRecordDirect `
        -Store $retiredHistoryStore -AxisReference 1 -Slot 2 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key1 -StoreGeneration 2 -RecordGeneration 1 `
            -Tombstone)
    $retiredHistoryScan = Get-ReferenceAxisScan `
        -Store $retiredHistoryStore -AxisReference 1
    Assert-ReferenceTrue (-not $retiredHistoryScan.IsCorrupt) `
        'Matching newer tombstone must supersede older terminal history.'
    Assert-ReferenceEqual 20 `
        (Invoke-ReferenceQuery `
            -Store $retiredHistoryStore -Key $key2).DetailCode `
        'Current exact Armed with retired history must remain detail 20.'
}

Invoke-ReferenceFixture -Name 'StartReplaceableSlotSelection' -Body {
    $newKey = New-ReferenceRecoveryKey `
        -OriginalRequestId 6304 -Intent0 634

    $blankPriorityStore = New-ReferenceStore
    $incomplete = New-ReferenceSucceededRecordBytes `
        -Key (New-ReferenceRecoveryKey `
            -OriginalRequestId 6301 -Intent0 631) `
        -StoreGeneration 99 -RecordGeneration 1
    Set-ReferenceUInt32LE $incomplete $script:CommitMarkerOffset 0
    Set-ReferenceStoreRecordDirect `
        -Store $blankPriorityStore -AxisReference 1 -Slot 1 `
        -RecordBytes $incomplete
    Set-ReferenceStoreRecordDirect `
        -Store $blankPriorityStore -AxisReference 1 -Slot 3 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key (New-ReferenceRecoveryKey `
                -OriginalRequestId 6302 -Intent0 632) `
            -StoreGeneration 5 -RecordGeneration 2 -Tombstone)
    $incompleteBefore = Get-ReferenceRegion `
        -Bytes $blankPriorityStore `
        -Offset (Get-ReferenceRecordOffset -AxisReference 1 -Slot 1) `
        -Length $script:RecordSize
    $blankPriority = Invoke-ReferenceStart `
        -Store $blankPriorityStore -Key $newKey
    Assert-ReferenceTrue $blankPriority.Success `
        'Blank bank must admit Start when another bank is Incomplete.'
    Assert-ReferenceEqual 2 $blankPriority.Record.Slot `
        'Every Blank bank must precede every Incomplete bank.'
    Assert-ReferenceBytesEqual $incompleteBefore `
        (Get-ReferenceRegion `
            -Bytes $blankPriorityStore `
            -Offset (Get-ReferenceRecordOffset -AxisReference 1 -Slot 1) `
            -Length $script:RecordSize) `
        'Blank selection must leave the Incomplete bank unchanged.'

    $threeTombstoneStore = New-ReferenceStore
    $tombstoneKeys = @(
        (New-ReferenceRecoveryKey `
            -OriginalRequestId 6311 -Intent0 641),
        (New-ReferenceRecoveryKey `
            -OriginalRequestId 6312 -Intent0 642),
        (New-ReferenceRecoveryKey `
            -OriginalRequestId 6313 -Intent0 643))
    $storeGenerations = @(7, 3, 9)
    for ($slot = 1; $slot -le 3; $slot++) {
        Set-ReferenceStoreRecordDirect `
            -Store $threeTombstoneStore -AxisReference 1 -Slot $slot `
            -RecordBytes (New-ReferenceSucceededRecordBytes `
                -Key $tombstoneKeys[$slot - 1] `
                -StoreGeneration $storeGenerations[$slot - 1] `
                -RecordGeneration $slot -Tombstone)
    }
    $threeTombstoneStart = Invoke-ReferenceStart `
        -Store $threeTombstoneStore -Key $newKey
    Assert-ReferenceTrue $threeTombstoneStart.Success `
        'Three valid tombstones must not incorrectly return detail 23.'
    Assert-ReferenceEqual 2 $threeTombstoneStart.Record.Slot `
        'Start must replace the minimum-generation unprotected tombstone.'
    Assert-ReferenceEqual 1 $threeTombstoneStart.NativeCount `
        'Admitted three-tombstone Start must call native exactly once.'

    $mixedReplaceableStore = New-ReferenceStore
    $retiredKey = New-ReferenceRecoveryKey `
        -OriginalRequestId 6321 -Intent0 651
    $olderTombstoneKey = New-ReferenceRecoveryKey `
        -OriginalRequestId 6322 -Intent0 652
    Set-ReferenceStoreRecordDirect `
        -Store $mixedReplaceableStore -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $retiredKey -StoreGeneration 8 -RecordGeneration 1)
    Set-ReferenceStoreRecordDirect `
        -Store $mixedReplaceableStore -AxisReference 1 -Slot 2 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $retiredKey -StoreGeneration 10 -RecordGeneration 1 `
            -Tombstone)
    Set-ReferenceStoreRecordDirect `
        -Store $mixedReplaceableStore -AxisReference 1 -Slot 3 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $olderTombstoneKey -StoreGeneration 2 `
            -RecordGeneration 2 -Tombstone)
    $mixedStart = Invoke-ReferenceStart `
        -Store $mixedReplaceableStore -Key $newKey
    Assert-ReferenceTrue $mixedStart.Success `
        'Mixed replaceable history must admit Start.'
    Assert-ReferenceEqual 3 $mixedStart.Record.Slot `
        'Minimum generation must beat retired-shadow record type priority.'
}

Invoke-ReferenceFixture -Name 'GenerationNoWrap' -Body {
    Assert-ReferenceEqual 1 `
        (Get-ReferenceNextNonZeroGeneration 0) `
        'Generation zero must advance to one.'
    $beforeMaximum = [uint32]([uint64]$script:UInt32Max - 1)
    Assert-ReferenceEqual $script:UInt32Max `
        (Get-ReferenceNextNonZeroGeneration $beforeMaximum) `
        'Maximum nonzero generation must be reachable.'
    Assert-ReferenceThrows `
        'System.OverflowException' `
        { Get-ReferenceNextNonZeroGeneration $script:UInt32Max } `
        'Generation must never wrap to zero.'
}

Invoke-ReferenceFixture -Name 'DuplicateCommittedStoreGeneration' -Body {
    $key = New-ReferenceRecoveryKey
    $identical = New-ReferenceSucceededRecordBytes `
        -Key $key -StoreGeneration 7 -RecordGeneration 4
    $store = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $store -AxisReference 1 -Slot 1 -RecordBytes $identical
    Set-ReferenceStoreRecordDirect `
        -Store $store -AxisReference 1 -Slot 2 `
        -RecordBytes (Copy-ReferenceBytes $identical)
    $scan = Get-ReferenceAxisScan -Store $store -AxisReference 1
    Assert-ReferenceTrue $scan.IsCorrupt `
        'Identical bytes may not reuse a committed StoreGeneration.'
    Assert-ReferenceTrue `
        ($scan.CorruptReasons -contains 'DuplicateCommittedStoreGeneration') `
        'Duplicate committed generation reason is missing.'
    Assert-ReferenceEqual 21 `
        (Invoke-ReferenceQuery -Store $store -Key $key).DetailCode `
        'Identical duplicate generation must return detail 21.'

    $otherKey = New-ReferenceRecoveryKey `
        -OriginalRequestId 2002 -Intent0 202
    $store = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $store -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key -StoreGeneration 9 -RecordGeneration 5)
    Set-ReferenceStoreRecordDirect `
        -Store $store -AxisReference 1 -Slot 2 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $otherKey -StoreGeneration 9 -RecordGeneration 6)
    $scan = Get-ReferenceAxisScan -Store $store -AxisReference 1
    Assert-ReferenceTrue $scan.IsCorrupt `
        'Divergent bytes may not reuse a committed StoreGeneration.'
    Assert-ReferenceEqual 21 `
        (Invoke-ReferenceQuery -Store $store -Key $key).DetailCode `
        'Divergent duplicate generation must return detail 21.'
}

Invoke-ReferenceFixture -Name 'IncompleteGenerationReusable' -Body {
    $key = New-ReferenceRecoveryKey
    $store = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $store -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key -StoreGeneration 5 -RecordGeneration 1 `
            -Tombstone)
    $incomplete = New-ReferenceSucceededRecordBytes `
        -Key $key -StoreGeneration $script:UInt32Max `
        -RecordGeneration 2
    Set-ReferenceUInt32LE $incomplete $script:CommitMarkerOffset 0
    Set-ReferenceStoreRecordDirect `
        -Store $store -AxisReference 1 -Slot 2 `
        -RecordBytes $incomplete

    $scan = Get-ReferenceAxisScan -Store $store -AxisReference 1
    Assert-ReferenceTrue (-not $scan.IsCorrupt) `
        'Marker-zero generation must not corrupt the axis.'
    Assert-ReferenceEqual 5 `
        (Get-ReferenceMaximumGeneration `
            -ValidRecords $scan.ValidRecords `
            -PropertyName 'StoreGeneration') `
        'Marker-zero maximum must be ignored.'
    $reused = Get-ReferenceNextNonZeroGeneration 5
    Assert-ReferenceEqual 6 $reused `
        'Generation after committed five must reuse six.'
    Set-ReferenceStoreRecordDirect `
        -Store $store -AxisReference 1 -Slot 3 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key -StoreGeneration $reused -RecordGeneration 1 `
            -Tombstone)
    $scan = Get-ReferenceAxisScan -Store $store -AxisReference 1
    Assert-ReferenceTrue (-not $scan.IsCorrupt) `
        'Reusing a marker-zero generation must remain valid.'
    Assert-ReferenceEqual 6 `
        (Get-ReferenceMaximumGeneration `
            -ValidRecords $scan.ValidRecords `
            -PropertyName 'StoreGeneration') `
        'Reused committed generation was not observed.'
}

Invoke-ReferenceFixture -Name 'LatestExactTerminalSelection' -Body {
    $key = New-ReferenceRecoveryKey
    $store = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $store -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key -StoreGeneration 10 -RecordGeneration 4)
    Set-ReferenceStoreRecordDirect `
        -Store $store -AxisReference 1 -Slot 2 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key -StoreGeneration 20 -RecordGeneration 4 -Tombstone)

    $query = Invoke-ReferenceQuery -Store $store -Key $key
    Assert-ReferenceTrue $query.Success `
        'Exact terminal query must succeed.'
    Assert-ReferenceEqual 20 $query.Record.StoreGeneration `
        'Exact query must select the newest physical commit.'
    Assert-ReferenceTrue $query.Record.IsTombstone `
        'Newest exact terminal must be the durable tombstone.'
    Assert-ReferenceEqual 0 $query.MutationCount `
        'Query must not write retained storage.'
    Assert-ReferenceEqual 0 $query.NativeCount `
        'Query must not replay motion.'
}

Invoke-ReferenceFixture -Name 'DivergentSnapshotRegardlessStoreGeneration' -Body {
    $key = New-ReferenceRecoveryKey
    $store = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $store -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key -StoreGeneration 1 -RecordGeneration 7)
    Set-ReferenceStoreRecordDirect `
        -Store $store -AxisReference 1 -Slot 2 `
        -RecordBytes (New-ReferenceRejectedRecordBytes `
            -Key $key -StoreGeneration 99 -RecordGeneration 7)

    $scan = Get-ReferenceAxisScan -Store $store -AxisReference 1
    Assert-ReferenceTrue $scan.IsCorrupt `
        'Same key and RecordGeneration with divergent snapshots is corrupt.'
    Assert-ReferenceTrue `
        ($scan.CorruptReasons -contains 'DivergentTerminalSnapshot') `
        'Divergent terminal snapshot reason is missing.'
    $query = Invoke-ReferenceQuery -Store $store -Key $key
    Assert-ReferenceEqual 21 $query.DetailCode `
        'Divergence must return detail 21 regardless of StoreGeneration.'
    Assert-ReferenceEqual 0 $query.NativeCount `
        'Divergence query must not replay motion.'
}

Invoke-ReferenceFixture -Name 'QueryDetailPrecedence19To24' -Body {
    $key = New-ReferenceRecoveryKey
    $otherKey = New-ReferenceRecoveryKey `
        -OriginalRequestId 3002 -Intent0 302

    $blank = New-ReferenceStore
    $result19 = Invoke-ReferenceQuery -Store $blank -Key $key

    $exactArmedStore = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $exactArmedStore -AxisReference 1 -Slot 0 `
        -RecordBytes (New-ReferenceArmedRecordBytes `
            -Key $key -StoreGeneration 1 -RecordGeneration 1)
    $result20 = Invoke-ReferenceQuery -Store $exactArmedStore -Key $key
    Assert-ReferenceEqual 20 `
        (Invoke-ReferenceRetirement `
            -Store $exactArmedStore -Key $key `
            -ExpectedRecordGeneration 1).DetailCode `
        'Only exact Armed may return retirement detail 20.'

    $corruptStore = New-ReferenceStore
    $corruptRecord = New-ReferenceSucceededRecordBytes `
        -Key $key -StoreGeneration 1 -RecordGeneration 1
    Set-ReferenceUInt32LE $corruptRecord $script:CommitMarkerOffset 1
    Set-ReferenceStoreRecordDirect `
        -Store $corruptStore -AxisReference 1 -Slot 1 `
        -RecordBytes $corruptRecord
    $result21 = Invoke-ReferenceQuery -Store $corruptStore -Key $key

    $mismatchStore = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $mismatchStore -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $otherKey -StoreGeneration 1 -RecordGeneration 1 `
            -Tombstone)
    $result22 = Invoke-ReferenceQuery -Store $mismatchStore -Key $key

    $occupiedStore = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $occupiedStore -AxisReference 1 -Slot 0 `
        -RecordBytes (New-ReferenceArmedRecordBytes `
            -Key $otherKey -StoreGeneration 1 -RecordGeneration 1)
    $differentArmedQuery = Invoke-ReferenceQuery `
        -Store $occupiedStore -Key $key

    $result24 = Invoke-ReferenceQuery `
        -Store $corruptStore -Key $key -StorageAvailable $false
    $results = @($result19, $result20, $result21,
        $result22, $differentArmedQuery, $result24)
    $expectedDetails = @(19, 20, 21, 22, 22, 24)
    for ($index = 0; $index -lt $results.Count; $index++) {
        $expectedDetail = $expectedDetails[$index]
        Assert-ReferenceTrue (-not $results[$index].Success) `
            ('Detail ' + $expectedDetail + ' case must fail.')
        Assert-ReferenceEqual $expectedDetail `
            $results[$index].DetailCode `
            'Query result precedence changed.'
        Assert-ReferenceEqual 0 $results[$index].MutationCount `
            'Query precedence case must not write.'
        Assert-ReferenceEqual 0 $results[$index].NativeCount `
            'Query precedence case must not call native motion.'
    }
    Assert-ReferenceEqual 23 `
        (Invoke-ReferenceStart `
            -Store $occupiedStore -Key $key).DetailCode `
        'Detail 23 must be reserved for Start admission.'
    Assert-ReferenceEqual 22 `
        (Invoke-ReferenceRetirement `
            -Store $occupiedStore -Key $key `
            -ExpectedRecordGeneration 1).DetailCode `
        'Retirement of a different valid key must return detail 22.'

    $exactWins = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $exactWins -AxisReference 1 -Slot 2 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $otherKey -StoreGeneration 1 -RecordGeneration 1 `
            -Tombstone)
    Set-ReferenceStoreRecordDirect `
        -Store $exactWins -AxisReference 1 -Slot 0 `
        -RecordBytes (New-ReferenceArmedRecordBytes `
            -Key $key -StoreGeneration 2 -RecordGeneration 2)
    Set-ReferenceStoreRecordDirect `
        -Store $exactWins -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key -StoreGeneration 3 -RecordGeneration 2)
    $exactResult = Invoke-ReferenceQuery -Store $exactWins -Key $key
    Assert-ReferenceTrue $exactResult.Success `
        'Exact terminal must coexist with unrelated retired history.'
}

Invoke-ReferenceFixture -Name 'QueryResponseCapacityGate' -Body {
    $key = New-ReferenceRecoveryKey
    $terminalStore = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $terminalStore -AxisReference 1 -Slot 0 `
        -RecordBytes (New-ReferenceArmedRecordBytes `
            -Key $key -StoreGeneration 1 -RecordGeneration 1)
    Set-ReferenceStoreRecordDirect `
        -Store $terminalStore -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key -StoreGeneration 2 -RecordGeneration 1)
    $before = Copy-ReferenceBytes $terminalStore

    $tooSmall = Invoke-ReferenceQuery `
        -Store $terminalStore -Key $key -TotalResponseCapacity 91
    Assert-ReferenceTrue (-not $tooSmall.Success) `
        '91-byte query response capacity must fail.'
    Assert-ReferenceEqual 0 $tooSmall.DetailCode `
        'Query capacity failure must be an internal failure.'
    Assert-ReferenceEqual 0 $tooSmall.MutationCount `
        'Query capacity failure must remain zero-write.'
    Assert-ReferenceEqual 0 $tooSmall.NativeCount `
        'Query capacity failure must remain zero-native.'
    Assert-ReferenceBytesEqual $before $terminalStore `
        'Query capacity failure must preserve the store.'

    $exact = Invoke-ReferenceQuery `
        -Store $terminalStore -Key $key -TotalResponseCapacity 92
    Assert-ReferenceTrue $exact.Success `
        'Exactly 92 bytes must admit terminal query success.'
    Assert-ReferenceEqual 1 $exact.Record.RecordGeneration `
        '92-byte query must return the exact terminal snapshot.'

    $tombstoneStore = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $tombstoneStore -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key -StoreGeneration 2 -RecordGeneration 1 `
            -Tombstone)
    $tombstoneBefore = Copy-ReferenceBytes $tombstoneStore
    $tombstoneTooSmall = Invoke-ReferenceQuery `
        -Store $tombstoneStore -Key $key `
        -TotalResponseCapacity 91
    Assert-ReferenceTrue (-not $tombstoneTooSmall.Success) `
        'Tombstone query success must also require 92 bytes.'
    Assert-ReferenceEqual 0 $tombstoneTooSmall.DetailCode `
        'Tombstone query capacity failure must be internal.'
    Assert-ReferenceEqual 0 $tombstoneTooSmall.MutationCount `
        'Tombstone query capacity failure must remain zero-write.'
    Assert-ReferenceEqual 0 $tombstoneTooSmall.NativeCount `
        'Tombstone query capacity failure must remain zero-native.'
    Assert-ReferenceBytesEqual $tombstoneBefore $tombstoneStore `
        'Tombstone query capacity failure must preserve the store.'

    $armedStore = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $armedStore -AxisReference 1 -Slot 0 `
        -RecordBytes (New-ReferenceArmedRecordBytes `
            -Key $key -StoreGeneration 1 -RecordGeneration 1)
    Assert-ReferenceEqual 20 `
        (Invoke-ReferenceQuery `
            -Store $armedStore -Key $key `
            -TotalResponseCapacity 0).DetailCode `
        'Capacity gate must apply only to query success paths.'
}

Invoke-ReferenceFixture -Name 'RejectedTerminalQuery' -Body {
    $key = New-ReferenceRecoveryKey
    $preNativeStore = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $preNativeStore -AxisReference 1 -Slot 0 `
        -RecordBytes (New-ReferenceArmedRecordBytes `
            -Key $key -StoreGeneration 1 -RecordGeneration 1)
    Set-ReferenceStoreRecordDirect `
        -Store $preNativeStore -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceRejectedRecordBytes `
            -Key $key -StoreGeneration 2 -RecordGeneration 1 `
            -DetailCode 10 -ErrorId -31000)
    $preNative = Invoke-ReferenceQuery -Store $preNativeStore -Key $key
    Assert-ReferenceTrue $preNative.Success `
        'Pre-native rejection is an exact terminal result.'
    Assert-ReferenceEqual 10 $preNative.Record.OriginalDetailCode `
        'Pre-native rejection detail changed.'
    Assert-ReferenceEqual 0 $preNative.Record.NativeCommandState `
        'Pre-native rejection must not contain native state.'

    $nativeState = [Convert]::ToUInt32('A5A5A5A5', 16)
    $nativeStore = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $nativeStore -AxisReference 1 -Slot 0 `
        -RecordBytes (New-ReferenceArmedRecordBytes `
            -Key $key -StoreGeneration 1 -RecordGeneration 2)
    Set-ReferenceStoreRecordDirect `
        -Store $nativeStore -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceRejectedRecordBytes `
            -Key $key -StoreGeneration 2 -RecordGeneration 2 `
            -DetailCode 11 -ErrorId -6 `
            -NativeCommandState $nativeState)
    $native = Invoke-ReferenceQuery -Store $nativeStore -Key $key
    Assert-ReferenceTrue $native.Success `
        'Native rejection is an exact terminal result.'
    Assert-ReferenceEqual 11 $native.Record.OriginalDetailCode `
        'Native rejection detail changed.'
    Assert-ReferenceEqual -6 $native.Record.OriginalErrorId `
        'Native rejection error changed.'
    Assert-ReferenceEqual $nativeState $native.Record.NativeCommandState `
        'Native rejection state changed.'
    Assert-ReferenceEqual 0 $native.NativeCount `
        'Rejected query must never replay native motion.'
}

Invoke-ReferenceFixture -Name 'OccupiedAndIndeterminateStart' -Body {
    $key = New-ReferenceRecoveryKey
    $otherKey = New-ReferenceRecoveryKey `
        -OriginalRequestId 4002 -Intent0 402

    $exactArmedStore = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $exactArmedStore -AxisReference 1 -Slot 0 `
        -RecordBytes (New-ReferenceArmedRecordBytes `
            -Key $key -StoreGeneration 1 -RecordGeneration 1)
    $indeterminate = Invoke-ReferenceStart `
        -Store $exactArmedStore -Key $key
    Assert-ReferenceEqual 20 $indeterminate.DetailCode `
        'Exact Armed start must be indeterminate.'
    Assert-ReferenceEqual 0 $indeterminate.NativeCount `
        'Indeterminate start must not call native motion.'

    $otherArmedStore = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $otherArmedStore -AxisReference 1 -Slot 0 `
        -RecordBytes (New-ReferenceArmedRecordBytes `
            -Key $otherKey -StoreGeneration 1 -RecordGeneration 1)
    $occupiedArmed = Invoke-ReferenceStart `
        -Store $otherArmedStore -Key $key
    Assert-ReferenceEqual 23 $occupiedArmed.DetailCode `
        'Different Armed start must be occupied.'
    Assert-ReferenceEqual 0 $occupiedArmed.NativeCount `
        'Occupied Armed start must not call native motion.'

    $otherTerminalStore = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $otherTerminalStore -AxisReference 1 -Slot 0 `
        -RecordBytes (New-ReferenceArmedRecordBytes `
            -Key $otherKey -StoreGeneration 1 -RecordGeneration 1)
    Set-ReferenceStoreRecordDirect `
        -Store $otherTerminalStore -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $otherKey -StoreGeneration 2 -RecordGeneration 1)
    $occupiedTerminal = Invoke-ReferenceStart `
        -Store $otherTerminalStore -Key $key
    Assert-ReferenceEqual 23 $occupiedTerminal.DetailCode `
        'Different active terminal must be occupied.'
    Assert-ReferenceEqual 0 $occupiedTerminal.NativeCount `
        'Occupied terminal start must not call native motion.'

    $retiredStore = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $retiredStore -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $otherKey -StoreGeneration 1 -RecordGeneration 1 `
            -Tombstone)
    $afterRetired = Invoke-ReferenceStart -Store $retiredStore -Key $key
    Assert-ReferenceTrue $afterRetired.Success `
        'Different tombstone must not occupy the axis.'
    Assert-ReferenceEqual 1 $afterRetired.NativeCount `
        'Fresh start after tombstone must call native exactly once.'
}

Invoke-ReferenceFixture -Name 'CommitWriteReadbackFailureMatrix' -Body {
    $key = New-ReferenceRecoveryKey
    $candidate = New-ReferenceSucceededRecordBytes `
        -Key $key -StoreGeneration 1 -RecordGeneration 1
    $cases = @(
        [pscustomobject]@{ Name = 'MarkerClearWriteFailure';
            Mutations = 0; Classification = 'Blank' },
        [pscustomobject]@{ Name = 'MarkerClearReadbackFailure';
            Mutations = 1; Classification = 'Blank' },
        [pscustomobject]@{ Name = 'MarkerClearReadbackMismatch';
            Mutations = 1; Classification = 'Corrupt' },
        [pscustomobject]@{ Name = 'BodyWriteFailure';
            Mutations = 1; Classification = 'Blank' },
        [pscustomobject]@{ Name = 'BodyReadbackFailure';
            Mutations = 2; Classification = 'Incomplete' },
        [pscustomobject]@{ Name = 'BodyReadbackMismatch';
            Mutations = 2; Classification = 'Incomplete' },
        [pscustomobject]@{ Name = 'FinalMarkerWriteFailure';
            Mutations = 2; Classification = 'Incomplete' },
        [pscustomobject]@{ Name = 'FinalMarkerReadbackFailure';
            Mutations = 3; Classification = 'Valid' },
        [pscustomobject]@{ Name = 'FinalMarkerReadbackMismatch';
            Mutations = 3; Classification = 'Corrupt' },
        [pscustomobject]@{ Name = 'FinalRecordReadbackFailure';
            Mutations = 3; Classification = 'Valid' },
        [pscustomobject]@{ Name = 'FinalRecordReadbackMismatch';
            Mutations = 3; Classification = 'Corrupt' }
    )
    Assert-ReferenceEqual 11 $cases.Count `
        'Commit write/readback failure case count changed.'
    foreach ($case in $cases) {
        $store = New-ReferenceStore
        $commit = Invoke-ReferenceRecordCommit `
            -Store $store -AxisReference 1 -Slot 1 `
            -RecordBytes $candidate -FailureAt $case.Name
        Assert-ReferenceTrue (-not $commit.Completed) `
            ($case.Name + ' must not complete the commit.')
        Assert-ReferenceEqual $case.Name $commit.FailureAt `
            ($case.Name + ' failure identity changed.')
        Assert-ReferenceEqual $case.Mutations $commit.MutationCount `
            ($case.Name + ' retained mutation count changed.')
        $read = Read-ReferenceRecord `
            -Bytes $store `
            -Offset (Get-ReferenceRecordOffset `
                -AxisReference 1 -Slot 1) `
            -Slot 1
        Assert-ReferenceEqual $case.Classification $read.Classification `
            ($case.Name + ' physical classification changed.')
    }

    Assert-ReferenceThrows `
        'System.ArgumentException' `
        { Invoke-ReferenceRecordCommit `
            -Store (New-ReferenceStore) -AxisReference 1 -Slot 1 `
            -RecordBytes $candidate -CrashAt 'AfterBodyWrite' `
            -FailureAt 'BodyReadbackFailure' } `
        'CrashAt and FailureAt must not be combined.'

    $completeStore = New-ReferenceStore
    $complete = Invoke-ReferenceRecordCommit `
        -Store $completeStore -AxisReference 1 -Slot 1 `
        -RecordBytes $candidate
    Assert-ReferenceTrue $complete.Completed `
        'No injected failure must complete full readback validation.'
    Assert-ReferenceEqual 'Valid' `
        (Read-ReferenceRecord `
            -Bytes $completeStore `
            -Offset (Get-ReferenceRecordOffset `
                -AxisReference 1 -Slot 1)).Classification `
        'Completed commit must be physically valid.'
}

Invoke-ReferenceFixture -Name 'RetirementDurableLifecycle' -Body {
    $setup = New-ReferenceStoreBeforeThirdRetirement
    $previous = Invoke-ReferenceQuery `
        -Store $setup.Store -Key $setup.Key2
    Assert-ReferenceTrue $previous.Success `
        'Previous tombstone must exist before third retirement.'
    $previousBytes = Copy-ReferenceBytes $previous.Record.Bytes
    $previousSlot = $previous.Record.Slot
    $expectedTargetSlot = 3
    $intentBeforeBytes = Get-ReferenceRegion `
        -Bytes $setup.Store `
        -Offset (Get-ReferenceRecordOffset `
            -AxisReference 1 -Slot 0) `
        -Length $script:RecordSize

    $retired = Invoke-ReferenceRetirement `
        -Store $setup.Store -Key $setup.Key3 `
        -ExpectedRecordGeneration $setup.Start3.Record.RecordGeneration
    Assert-ReferenceTrue $retired.Success `
        'Third distinct retirement must commit durably.'
    Assert-ReferenceTrue $retired.Record.IsTombstone `
        'Retirement must produce a tombstone.'
    Assert-ReferenceEqual $expectedTargetSlot $retired.Record.Slot `
        'Third retirement must reuse the minimum-generation replaceable slot.'
    Assert-ReferenceEqual 3 $retired.MutationCount `
        'Durable retirement must use only the three tombstone commit writes.'

    $previousAfterBytes = Get-ReferenceRegion `
        -Bytes $setup.Store `
        -Offset (Get-ReferenceRecordOffset `
            -AxisReference 1 -Slot $previousSlot) `
        -Length $script:RecordSize
    Assert-ReferenceBytesEqual $previousBytes $previousAfterBytes `
        'Previous tombstone must survive the next distinct durable commit.'

    Assert-ReferenceEqual $setup.Start3.Record.RecordState `
        $retired.Record.RecordState `
        'Retirement must preserve terminal state.'
    Assert-ReferenceEqual $setup.Start3.Record.AppliedPosition `
        $retired.Record.AppliedPosition `
        'Retirement must preserve applied position.'
    Assert-ReferenceEqual $setup.Start3.Record.OriginalCommandStatus `
        $retired.Record.OriginalCommandStatus `
        'Retirement must preserve original status.'
    Assert-ReferenceEqual $setup.Start3.Record.OriginalErrorId `
        $retired.Record.OriginalErrorId `
        'Retirement must preserve original error.'
    Assert-ReferenceEqual $setup.Start3.Record.OriginalDetailCode `
        $retired.Record.OriginalDetailCode `
        'Retirement must preserve original detail.'
    Assert-ReferenceEqual $setup.Start3.Record.NativeCommandState `
        $retired.Record.NativeCommandState `
        'Retirement must preserve native state.'
    Assert-ReferenceEqual $setup.Start3.Record.RecordGeneration `
        $retired.Record.RecordGeneration `
        'Retirement must preserve RecordGeneration.'

    $scan = Get-ReferenceAxisScan -Store $setup.Store -AxisReference 1
    Assert-ReferenceTrue (-not $scan.IsCorrupt) `
        'Durable retirement lifecycle must remain valid.'
    Assert-ReferenceEqual 'Valid' $scan.Entries[0].Classification `
        'Retirement must never physically clear the Intent slot.'
    Assert-ReferenceEqual $script:StateArmed `
        $scan.Entries[0].Record.RecordState `
        'Retirement must retain the matching Armed record as a shadow.'
    Assert-ReferenceBytesEqual $intentBeforeBytes `
        $scan.Entries[0].Bytes `
        'Retirement must preserve every Intent byte.'
    Assert-ReferenceEqual 4 $scan.ValidRecords.Count `
        'Intent plus terminal A/B/C must remain four committed records.'
    Assert-ReferenceTrue `
        (Test-ReferenceArmedIsInactiveShadow `
            -Record $scan.Entries[0].Record `
            -ValidRecords $scan.ValidRecords) `
        'A matching newer tombstone must make Armed an inactive shadow.'
    Assert-ReferenceTrue `
        (Invoke-ReferenceQuery `
            -Store $setup.Store -Key $setup.Key2).Success `
        'Previous tombstone must still be queryable.'

    $nextKey = New-ReferenceRecoveryKey `
        -OriginalRequestId 1004 -Intent0 104
    $next = Invoke-ReferenceStart -Store $setup.Store -Key $nextKey
    Assert-ReferenceTrue $next.Success `
        'A distinct Start must be admitted after durable retirement.'
    Assert-ReferenceEqual 6 $next.MutationCount `
        'Distinct Start must use full three-write Intent and terminal commits.'
    Assert-ReferenceEqual 1 $next.NativeCount `
        'Distinct Start must call native exactly once.'
    $nextIntent = Read-ReferenceRecord `
        -Bytes $setup.Store `
        -Offset (Get-ReferenceRecordOffset `
            -AxisReference 1 -Slot 0) `
        -Slot 0
    Assert-ReferenceEqual 'Valid' $nextIntent.Classification `
        'Distinct Start must leave a fully committed new Intent.'
    Assert-ReferenceTrue `
        (Test-ReferenceRecordMatchesKey $nextIntent.Record $nextKey) `
        'Distinct Start must overwrite slot zero with its exact key.'
    Assert-ReferenceTrue `
        (-not (Test-ReferenceBytesEqual `
            $intentBeforeBytes $nextIntent.Bytes)) `
        'Distinct Start must replace the retired Intent shadow.'
}

Invoke-ReferenceFixture -Name 'RetirementCrashMatrix' -Body {
    $crashCases = @(
        [pscustomobject]@{ Name = 'AfterMarkerClear'; Mutations = 1;
            Classification = 'Incomplete'; Success = $false; Detail = 24;
            DurableNew = $false },
        [pscustomobject]@{ Name = 'AfterBodyWrite'; Mutations = 2;
            Classification = 'Incomplete'; Success = $false; Detail = 24;
            DurableNew = $false },
        [pscustomobject]@{ Name = 'DuringMarkerWrite'; Mutations = 3;
            Classification = 'Corrupt'; Success = $false; Detail = 21;
            DurableNew = $false },
        [pscustomobject]@{ Name = 'AfterMarkerWrite'; Mutations = 3;
            Classification = 'Valid'; Success = $true; Detail = 0;
            DurableNew = $true })
    $failureCases = @(
        [pscustomobject]@{ Name = 'MarkerClearWriteFailure';
            Mutations = 0; Classification = 'Valid'; Success = $false;
            Detail = 24; DurableNew = $false },
        [pscustomobject]@{ Name = 'MarkerClearReadbackFailure';
            Mutations = 1; Classification = 'Incomplete'; Success = $false;
            Detail = 24; DurableNew = $false },
        [pscustomobject]@{ Name = 'MarkerClearReadbackMismatch';
            Mutations = 1; Classification = 'Corrupt'; Success = $false;
            Detail = 21; DurableNew = $false },
        [pscustomobject]@{ Name = 'BodyWriteFailure';
            Mutations = 1; Classification = 'Incomplete'; Success = $false;
            Detail = 24; DurableNew = $false },
        [pscustomobject]@{ Name = 'BodyReadbackFailure';
            Mutations = 2; Classification = 'Incomplete'; Success = $false;
            Detail = 24; DurableNew = $false },
        [pscustomobject]@{ Name = 'BodyReadbackMismatch';
            Mutations = 2; Classification = 'Incomplete'; Success = $false;
            Detail = 24; DurableNew = $false },
        [pscustomobject]@{ Name = 'FinalMarkerWriteFailure';
            Mutations = 2; Classification = 'Incomplete'; Success = $false;
            Detail = 24; DurableNew = $false },
        [pscustomobject]@{ Name = 'FinalMarkerReadbackFailure';
            Mutations = 3; Classification = 'Valid'; Success = $true;
            Detail = 0; DurableNew = $true },
        [pscustomobject]@{ Name = 'FinalMarkerReadbackMismatch';
            Mutations = 3; Classification = 'Corrupt'; Success = $false;
            Detail = 21; DurableNew = $false },
        [pscustomobject]@{ Name = 'FinalRecordReadbackFailure';
            Mutations = 3; Classification = 'Valid'; Success = $true;
            Detail = 0; DurableNew = $true },
        [pscustomobject]@{ Name = 'FinalRecordReadbackMismatch';
            Mutations = 3; Classification = 'Corrupt'; Success = $false;
            Detail = 21; DurableNew = $false }
    )
    Assert-ReferenceEqual 4 $crashCases.Count `
        'Retirement crash matrix count changed.'
    Assert-ReferenceEqual 11 $failureCases.Count `
        'Retirement commit failure case count changed.'
    foreach ($crashCase in $crashCases) {
        $setup = New-ReferenceStoreBeforeThirdRetirement
        $previous = Invoke-ReferenceQuery `
            -Store $setup.Store -Key $setup.Key2
        $source = Invoke-ReferenceQuery `
            -Store $setup.Store -Key $setup.Key3
        $targetSlot = 3
        $previousBytes = Copy-ReferenceBytes $previous.Record.Bytes
        $sourceBytes = Copy-ReferenceBytes $source.Record.Bytes
        $intentBytes = Get-ReferenceRegion `
            -Bytes $setup.Store `
            -Offset (Get-ReferenceRecordOffset `
                -AxisReference 1 -Slot 0) `
            -Length $script:RecordSize
        $result = Invoke-ReferenceRetirement `
            -Store $setup.Store -Key $setup.Key3 `
            -ExpectedRecordGeneration $setup.Start3.Record.RecordGeneration `
            -CrashAt $crashCase.Name

        Assert-ReferenceEqual $crashCase.Success $result.Success `
            ($crashCase.Name + ' retirement rescan result changed.')
        Assert-ReferenceTrue (-not $result.NoResponse) `
            ($crashCase.Name + ' retirement must never use no-response.')
        Assert-ReferenceTrue `
            ($result.Success -or
             $result.DetailCode -eq $script:DetailStoreCorrupt -or
             $result.DetailCode -eq $script:DetailStorageUnavailable) `
            ($crashCase.Name +
             ' post-write result must be success or detail 21/24.')
        Assert-ReferenceEqual `
            $(if ($result.Success) { 1 } else { 0 }) `
            $result.ResultCode `
            ($crashCase.Name + ' retirement result code changed.')
        Assert-ReferenceTrue ($result.ResultCode -ge 0) `
            ($crashCase.Name + ' retirement result may not be negative.')
        Assert-ReferenceTrue ($result.ResultCode -ne -12) `
            ($crashCase.Name + ' retirement may not expose -12 sentinel.')
        Assert-ReferenceEqual $crashCase.Detail $result.DetailCode `
            ($crashCase.Name + ' retirement rescan detail changed.')
        Assert-ReferenceEqual $crashCase.Mutations `
            $result.MutationCount `
            ($crashCase.Name + ' retirement mutation count changed.')
        Assert-ReferenceEqual 0 $result.NativeCount `
            ($crashCase.Name + ' retirement must not call native motion.')
        $previousAfter = Get-ReferenceRegion `
            -Bytes $setup.Store `
            -Offset (Get-ReferenceRecordOffset `
                -AxisReference 1 -Slot $previous.Record.Slot) `
            -Length $script:RecordSize
        Assert-ReferenceBytesEqual $previousBytes $previousAfter `
            ($crashCase.Name + ' must preserve the previous tombstone.')
        Assert-ReferenceBytesEqual $sourceBytes `
            (Get-ReferenceRegion `
                -Bytes $setup.Store `
                -Offset (Get-ReferenceRecordOffset `
                    -AxisReference 1 -Slot $source.Record.Slot) `
                -Length $script:RecordSize) `
            ($crashCase.Name + ' must preserve the terminal source.')
        Assert-ReferenceBytesEqual $intentBytes `
            (Get-ReferenceRegion `
                -Bytes $setup.Store `
                -Offset (Get-ReferenceRecordOffset `
                    -AxisReference 1 -Slot 0) `
                -Length $script:RecordSize) `
            ($crashCase.Name + ' must preserve every Intent byte.')

        $targetRead = Read-ReferenceRecord `
            -Bytes $setup.Store `
            -Offset (Get-ReferenceRecordOffset `
                -AxisReference 1 -Slot $targetSlot) `
            -Slot $targetSlot
        Assert-ReferenceEqual $crashCase.Classification `
            $targetRead.Classification `
            ($crashCase.Name + ' target classification changed.')
        if ($crashCase.DurableNew) {
            Assert-ReferenceTrue $targetRead.Record.IsTombstone `
                ($crashCase.Name + ' must expose the exact tombstone.')
            Assert-ReferenceTrue $result.Record.IsTombstone `
                ($crashCase.Name + ' rescan must return the tombstone proof.')
        }
        $queryAfter = Invoke-ReferenceQuery `
            -Store $setup.Store -Key $setup.Key3
        if ($crashCase.Classification -eq 'Corrupt') {
            Assert-ReferenceEqual 21 $queryAfter.DetailCode `
                ($crashCase.Name + ' corrupt target must close query.')
        }
        else {
            Assert-ReferenceTrue $queryAfter.Success `
                ($crashCase.Name + ' must leave an exact query winner.')
            Assert-ReferenceEqual $crashCase.DurableNew `
                $queryAfter.Record.IsTombstone `
                ($crashCase.Name + ' query winner identity changed.')
        }
    }

    foreach ($failureCase in $failureCases) {
        $setup = New-ReferenceStoreBeforeThirdRetirement
        $previous = Invoke-ReferenceQuery `
            -Store $setup.Store -Key $setup.Key2
        $targetSlot = 3
        $source = Invoke-ReferenceQuery `
            -Store $setup.Store -Key $setup.Key3
        $previousBytes = Copy-ReferenceBytes $previous.Record.Bytes
        $sourceBytes = Copy-ReferenceBytes $source.Record.Bytes
        $intentBytes = Get-ReferenceRegion `
            -Bytes $setup.Store `
            -Offset (Get-ReferenceRecordOffset `
                -AxisReference 1 -Slot 0) `
            -Length $script:RecordSize
        $result = Invoke-ReferenceRetirement `
            -Store $setup.Store -Key $setup.Key3 `
            -ExpectedRecordGeneration $setup.Start3.Record.RecordGeneration `
            -CommitFailureAt $failureCase.Name

        Assert-ReferenceEqual $failureCase.Success $result.Success `
            ($failureCase.Name + ' retirement rescan result changed.')
        Assert-ReferenceTrue (-not $result.NoResponse) `
            ($failureCase.Name + ' retirement must never use no-response.')
        Assert-ReferenceEqual `
            $(if ($result.Success) { 1 } else { 0 }) `
            $result.ResultCode `
            ($failureCase.Name + ' retirement result code changed.')
        Assert-ReferenceTrue ($result.ResultCode -ge 0) `
            ($failureCase.Name + ' retirement result may not be negative.')
        Assert-ReferenceTrue ($result.ResultCode -ne -12) `
            ($failureCase.Name + ' retirement may not expose -12 sentinel.')
        if ($result.MutationCount -gt 0) {
            Assert-ReferenceTrue `
                ($result.Success -or
                 $result.DetailCode -eq $script:DetailStoreCorrupt -or
                 $result.DetailCode -eq $script:DetailStorageUnavailable) `
                ($failureCase.Name +
                 ' post-write result must be success or detail 21/24.')
        }
        Assert-ReferenceEqual $failureCase.Detail $result.DetailCode `
            ($failureCase.Name + ' retirement rescan detail changed.')
        Assert-ReferenceEqual $failureCase.Mutations `
            $result.MutationCount `
            ($failureCase.Name + ' retirement mutation count changed.')
        Assert-ReferenceEqual 0 $result.NativeCount `
            ($failureCase.Name + ' retirement must remain zero-native.')
        Assert-ReferenceBytesEqual $previousBytes `
            (Get-ReferenceRegion `
                -Bytes $setup.Store `
                -Offset (Get-ReferenceRecordOffset `
                    -AxisReference 1 -Slot $previous.Record.Slot) `
                -Length $script:RecordSize) `
            ($failureCase.Name + ' must preserve ProtectedTombstone.')
        Assert-ReferenceBytesEqual $sourceBytes `
            (Get-ReferenceRegion `
                -Bytes $setup.Store `
                -Offset (Get-ReferenceRecordOffset `
                    -AxisReference 1 -Slot $source.Record.Slot) `
                -Length $script:RecordSize) `
            ($failureCase.Name + ' must preserve active terminal source.')
        Assert-ReferenceBytesEqual $intentBytes `
            (Get-ReferenceRegion `
                -Bytes $setup.Store `
                -Offset (Get-ReferenceRecordOffset `
                    -AxisReference 1 -Slot 0) `
                -Length $script:RecordSize) `
            ($failureCase.Name + ' must preserve every Intent byte.')

        $targetRead = Read-ReferenceRecord `
            -Bytes $setup.Store `
            -Offset (Get-ReferenceRecordOffset `
                -AxisReference 1 -Slot $targetSlot) `
            -Slot $targetSlot
        Assert-ReferenceEqual $failureCase.Classification `
            $targetRead.Classification `
            ($failureCase.Name + ' retirement target classification changed.')
        if ($failureCase.DurableNew) {
            Assert-ReferenceTrue $targetRead.Record.IsTombstone `
                ($failureCase.Name + ' must expose a durable tombstone.')
            Assert-ReferenceTrue $result.Record.IsTombstone `
                ($failureCase.Name + ' rescan must return tombstone proof.')
        }
        $query = Invoke-ReferenceQuery `
            -Store $setup.Store -Key $setup.Key3
        if ($failureCase.Classification -eq 'Corrupt') {
            Assert-ReferenceEqual 21 $query.DetailCode `
                ($failureCase.Name + ' corrupt target must close query.')
        }
        else {
            Assert-ReferenceTrue $query.Success `
                ($failureCase.Name + ' must retain an exact query winner.')
            Assert-ReferenceEqual $failureCase.DurableNew `
                $query.Record.IsTombstone `
                ($failureCase.Name + ' query winner identity changed.')
        }
    }
}

Invoke-ReferenceFixture -Name 'DuplicateRetirementZeroWrite' -Body {
    $setup = New-ReferenceStoreBeforeThirdRetirement
    $first = Invoke-ReferenceRetirement `
        -Store $setup.Store -Key $setup.Key3 `
        -ExpectedRecordGeneration $setup.Start3.Record.RecordGeneration
    Assert-ReferenceTrue $first.Success `
        'Initial retirement setup must succeed.'
    $before = Copy-ReferenceBytes $setup.Store
    $duplicate = Invoke-ReferenceRetirement `
        -Store $setup.Store -Key $setup.Key3 `
        -ExpectedRecordGeneration $setup.Start3.Record.RecordGeneration
    Assert-ReferenceTrue $duplicate.Success `
        'Duplicate retirement must succeed idempotently.'
    Assert-ReferenceTrue $duplicate.IsDuplicate `
        'Duplicate retirement must be labeled duplicate.'
    Assert-ReferenceEqual 0 $duplicate.MutationCount `
        'Duplicate retirement must perform zero writes.'
    Assert-ReferenceEqual 0 $duplicate.NativeCount `
        'Duplicate retirement must call no native motion.'
    Assert-ReferenceBytesEqual $before $setup.Store `
        'Duplicate retirement must preserve every store byte.'
}

Invoke-ReferenceFixture -Name 'RetirementResponseCapacityGate' -Body {
    $key = New-ReferenceRecoveryKey
    $activeStore = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $activeStore -AxisReference 1 -Slot 0 `
        -RecordBytes (New-ReferenceArmedRecordBytes `
            -Key $key -StoreGeneration 1 -RecordGeneration 1)
    Set-ReferenceStoreRecordDirect `
        -Store $activeStore -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key -StoreGeneration 2 -RecordGeneration 1)
    $activeBefore = Copy-ReferenceBytes $activeStore
    $tooSmall = Invoke-ReferenceRetirement `
        -Store $activeStore -Key $key `
        -ExpectedRecordGeneration 1 `
        -TotalResponseCapacity 91
    Assert-ReferenceTrue (-not $tooSmall.Success) `
        '91-byte retirement response capacity must fail.'
    Assert-ReferenceEqual 0 $tooSmall.DetailCode `
        'Response-capacity failure is an internal failure, not domain detail.'
    Assert-ReferenceEqual 0 $tooSmall.MutationCount `
        'Capacity gate must run before tombstone marker-clear.'
    Assert-ReferenceEqual 0 $tooSmall.NativeCount `
        'Retirement capacity failure must not call native motion.'
    Assert-ReferenceEqual 0 $tooSmall.ResultCode `
        'Service capacity gate must return nonnegative failure code zero.'
    Assert-ReferenceTrue (-not $tooSmall.NoResponse) `
        'Retirement service capacity failure may not use -12/no-response.'
    Assert-ReferenceBytesEqual $activeBefore $activeStore `
        'Capacity failure must preserve the complete active store.'

    $coreTooSmall = Invoke-ReferenceRetirementStoreCore `
        -Store $activeStore -Key $key `
        -ExpectedRecordGeneration 1 `
        -SnapshotCapacity 67
    Assert-ReferenceTrue (-not $coreTooSmall.Success) `
        '67-byte store-core snapshot capacity must fail.'
    Assert-ReferenceEqual 24 $coreTooSmall.DetailCode `
        'Store-core snapshot boundary failure must return detail 24.'
    Assert-ReferenceEqual 0 $coreTooSmall.MutationCount `
        'Store-core snapshot gate must precede retained mutation.'
    Assert-ReferenceEqual 0 $coreTooSmall.ResultCode `
        'Store-core snapshot failure must return code zero, never negative.'
    Assert-ReferenceTrue (-not $coreTooSmall.NoResponse) `
        'Store-core snapshot failure may not use -12/no-response.'
    Assert-ReferenceBytesEqual $activeBefore $activeStore `
        'Store-core snapshot failure must preserve active store bytes.'

    $wrapperCoreTooSmall = Invoke-ReferenceRetirement `
        -Store $activeStore -Key $key `
        -ExpectedRecordGeneration 1 `
        -TotalResponseCapacity 92 `
        -StoreSnapshotCapacity 67
    Assert-ReferenceTrue (-not $wrapperCoreTooSmall.Success) `
        '92-byte service capacity must not bypass the 68-byte core gate.'
    Assert-ReferenceEqual 24 $wrapperCoreTooSmall.DetailCode `
        'Wrapper/core boundary separation must preserve detail 24.'
    Assert-ReferenceEqual 0 $wrapperCoreTooSmall.MutationCount `
        'Wrapper/core capacity failure must remain zero-write.'
    Assert-ReferenceEqual 0 $wrapperCoreTooSmall.ResultCode `
        'Wrapper/core capacity failure may not expose a negative result.'
    Assert-ReferenceTrue (-not $wrapperCoreTooSmall.NoResponse) `
        'Wrapper/core capacity failure may not use -12/no-response.'
    Assert-ReferenceBytesEqual $activeBefore $activeStore `
        'Wrapper/core capacity failure must preserve active store bytes.'

    $bothExact = Invoke-ReferenceRetirement `
        -Store $activeStore -Key $key `
        -ExpectedRecordGeneration 1 `
        -TotalResponseCapacity 92 `
        -StoreSnapshotCapacity 68
    Assert-ReferenceTrue $bothExact.Success `
        'Total 92 and core snapshot 68 must admit retirement success.'
    Assert-ReferenceEqual 3 $bothExact.MutationCount `
        'Exact service/core capacities must perform one full commit.'
    Assert-ReferenceEqual 1 $bothExact.ResultCode `
        'Successful store-core retirement must return code one.'
    Assert-ReferenceTrue (-not $bothExact.NoResponse) `
        'Successful retirement may not use -12/no-response.'

    $duplicateStore = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $duplicateStore -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key -StoreGeneration 2 -RecordGeneration 1 `
            -Tombstone)
    $duplicateBefore = Copy-ReferenceBytes $duplicateStore
    $duplicateTooSmall = Invoke-ReferenceRetirement `
        -Store $duplicateStore -Key $key `
        -ExpectedRecordGeneration 1 `
        -TotalResponseCapacity 91
    Assert-ReferenceTrue (-not $duplicateTooSmall.Success) `
        'Duplicate success also requires 92-byte capacity.'
    Assert-ReferenceEqual 0 $duplicateTooSmall.DetailCode `
        'Duplicate service capacity failure must remain internal.'
    Assert-ReferenceEqual 0 $duplicateTooSmall.MutationCount `
        'Small duplicate response must remain zero-write.'
    Assert-ReferenceEqual 0 $duplicateTooSmall.ResultCode `
        'Duplicate service capacity failure may not be negative.'
    Assert-ReferenceTrue (-not $duplicateTooSmall.NoResponse) `
        'Duplicate service capacity failure may not use -12/no-response.'
    Assert-ReferenceBytesEqual $duplicateBefore $duplicateStore `
        'Small duplicate response must preserve store bytes.'

    $duplicateCoreTooSmall = Invoke-ReferenceRetirement `
        -Store $duplicateStore -Key $key `
        -ExpectedRecordGeneration 1 `
        -TotalResponseCapacity 92 `
        -StoreSnapshotCapacity 67
    Assert-ReferenceTrue (-not $duplicateCoreTooSmall.Success) `
        'Duplicate path must also enforce the 68-byte store-core gate.'
    Assert-ReferenceEqual 24 $duplicateCoreTooSmall.DetailCode `
        'Duplicate store-core capacity failure must return detail 24.'
    Assert-ReferenceEqual 0 $duplicateCoreTooSmall.MutationCount `
        'Duplicate store-core capacity failure must remain zero-write.'
    Assert-ReferenceEqual 0 $duplicateCoreTooSmall.ResultCode `
        'Duplicate core capacity failure may not return a negative code.'
    Assert-ReferenceTrue (-not $duplicateCoreTooSmall.NoResponse) `
        'Duplicate core capacity failure may not use -12/no-response.'
    Assert-ReferenceBytesEqual $duplicateBefore $duplicateStore `
        'Duplicate core capacity failure must preserve store bytes.'

    $duplicateExact = Invoke-ReferenceRetirement `
        -Store $duplicateStore -Key $key `
        -ExpectedRecordGeneration 1 `
        -TotalResponseCapacity 92
    Assert-ReferenceTrue $duplicateExact.Success `
        'Exactly 92 bytes must admit duplicate retirement success.'
    Assert-ReferenceTrue $duplicateExact.IsDuplicate `
        '92-byte duplicate response must remain idempotent.'
    Assert-ReferenceEqual 0 $duplicateExact.MutationCount `
        '92-byte duplicate response must remain zero-write.'
    Assert-ReferenceEqual 1 $duplicateExact.ResultCode `
        'Exact duplicate store-core result must be positive one.'
    Assert-ReferenceTrue (-not $duplicateExact.NoResponse) `
        'Exact duplicate retirement may not use -12/no-response.'
}

Invoke-ReferenceFixture -Name 'RetirementCasAndGenerationExhaustion' -Body {
    $key = New-ReferenceRecoveryKey
    $store = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $store -AxisReference 1 -Slot 0 `
        -RecordBytes (New-ReferenceArmedRecordBytes `
            -Key $key -StoreGeneration 1 -RecordGeneration 5)
    Set-ReferenceStoreRecordDirect `
        -Store $store -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key -StoreGeneration 2 -RecordGeneration 5)
    $before = Copy-ReferenceBytes $store
    $stale = Invoke-ReferenceRetirement `
        -Store $store -Key $key -ExpectedRecordGeneration 4
    Assert-ReferenceEqual 22 $stale.DetailCode `
        'Stale retirement CAS must return detail 22.'
    Assert-ReferenceEqual 0 $stale.MutationCount `
        'Stale retirement CAS must not write.'
    Assert-ReferenceBytesEqual $before $store `
        'Stale retirement CAS must preserve store bytes.'

    $exhaustedStore = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $exhaustedStore -AxisReference 1 -Slot 0 `
        -RecordBytes (New-ReferenceArmedRecordBytes `
            -Key $key -StoreGeneration 1 -RecordGeneration 5)
    Set-ReferenceStoreRecordDirect `
        -Store $exhaustedStore -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $key -StoreGeneration $script:UInt32Max `
            -RecordGeneration 5)
    $exhaustedBefore = Copy-ReferenceBytes $exhaustedStore
    $exhausted = Invoke-ReferenceRetirement `
        -Store $exhaustedStore -Key $key -ExpectedRecordGeneration 5
    Assert-ReferenceEqual 24 $exhausted.DetailCode `
        'Retirement generation exhaustion must return detail 24.'
    Assert-ReferenceEqual 0 $exhausted.MutationCount `
        'Generation exhaustion must fail before any write.'
    Assert-ReferenceBytesEqual $exhaustedBefore $exhaustedStore `
        'Generation exhaustion must preserve store bytes.'

    $otherKey = New-ReferenceRecoveryKey `
        -OriginalRequestId 5002 -Intent0 502
    $reserveStore = New-ReferenceStore
    $beforeMaximum = [uint32]([uint64]$script:UInt32Max - 1)
    Set-ReferenceStoreRecordDirect `
        -Store $reserveStore -AxisReference 1 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $otherKey -StoreGeneration $beforeMaximum `
            -RecordGeneration 1 -Tombstone)
    $reserveBefore = Copy-ReferenceBytes $reserveStore
    $reserveFailure = Invoke-ReferenceStart `
        -Store $reserveStore -Key $key
    Assert-ReferenceEqual 24 $reserveFailure.DetailCode `
        'Start must reserve two generations without wrap.'
    Assert-ReferenceEqual 0 $reserveFailure.MutationCount `
        'Failed two-generation reservation must not write Armed.'
    Assert-ReferenceEqual 0 $reserveFailure.NativeCount `
        'Failed two-generation reservation must not call native motion.'
    Assert-ReferenceBytesEqual $reserveBefore $reserveStore `
        'Failed reservation must preserve store bytes.'
}

Invoke-ReferenceFixture -Name 'StartCrashAndNativeAtMostOnce' -Body {
    $key = New-ReferenceRecoveryKey
    $subcaseCount = 0
    $maximumNativeCount = 0

    $commitCrashPoints = @(
        'AfterMarkerClear',
        'AfterBodyWrite',
        'DuringMarkerWrite',
        'AfterMarkerWrite')
    $failureCases = @(
        [pscustomobject]@{ Name = 'MarkerClearWriteFailure';
            Mutations = 0; Query = 'NotFound' },
        [pscustomobject]@{ Name = 'MarkerClearReadbackFailure';
            Mutations = 1; Query = 'NotFound' },
        [pscustomobject]@{ Name = 'MarkerClearReadbackMismatch';
            Mutations = 1; Query = 'Corrupt' },
        [pscustomobject]@{ Name = 'BodyWriteFailure';
            Mutations = 1; Query = 'NotFound' },
        [pscustomobject]@{ Name = 'BodyReadbackFailure';
            Mutations = 2; Query = 'NotFound' },
        [pscustomobject]@{ Name = 'BodyReadbackMismatch';
            Mutations = 2; Query = 'NotFound' },
        [pscustomobject]@{ Name = 'FinalMarkerWriteFailure';
            Mutations = 2; Query = 'NotFound' },
        [pscustomobject]@{ Name = 'FinalMarkerReadbackFailure';
            Mutations = 3; Query = 'Valid' },
        [pscustomobject]@{ Name = 'FinalMarkerReadbackMismatch';
            Mutations = 3; Query = 'Corrupt' },
        [pscustomobject]@{ Name = 'FinalRecordReadbackFailure';
            Mutations = 3; Query = 'Valid' },
        [pscustomobject]@{ Name = 'FinalRecordReadbackMismatch';
            Mutations = 3; Query = 'Corrupt' }
    )
    Assert-ReferenceEqual 11 $failureCases.Count `
        'Start commit failure case count changed.'
    foreach ($intentCrash in $commitCrashPoints) {
        $store = New-ReferenceStore
        $result = Invoke-ReferenceStart `
            -Store $store -Key $key -IntentCrashAt $intentCrash
        $subcaseCount++
        $maximumNativeCount = [Math]::Max(
            $maximumNativeCount, $result.NativeCount)
        Assert-ReferenceEqual 24 $result.DetailCode `
            ($intentCrash + ' intent failure must return detail 24.')
        Assert-ReferenceEqual 0 $result.NativeCount `
            ($intentCrash + ' intent failure must precede native motion.')
        $intentQuery = Invoke-ReferenceQuery -Store $store -Key $key
        if ($intentCrash -eq 'DuringMarkerWrite') {
            Assert-ReferenceEqual 21 $intentQuery.DetailCode `
                'Partial intent marker must return detail 21.'
        }
        elseif ($intentCrash -eq 'AfterMarkerWrite') {
            Assert-ReferenceEqual 20 $intentQuery.DetailCode `
                'Final intent marker before readback must expose Armed.'
        }
        else {
            Assert-ReferenceEqual 19 $intentQuery.DetailCode `
                ($intentCrash + ' incomplete intent must remain ignored.')
        }
    }

    foreach ($terminalCrash in $commitCrashPoints) {
        $store = New-ReferenceStore
        $result = Invoke-ReferenceStart `
            -Store $store -Key $key -TerminalCrashAt $terminalCrash
        $subcaseCount++
        $maximumNativeCount = [Math]::Max(
            $maximumNativeCount, $result.NativeCount)
        Assert-ReferenceTrue $result.NoResponse `
            ($terminalCrash + ' terminal failure must signal no response.')
        Assert-ReferenceEqual -12 $result.ResultCode `
            ($terminalCrash + ' terminal failure must return exact -12.')
        Assert-ReferenceEqual 1 $result.NativeCount `
            ($terminalCrash + ' terminal failure must call native once.')
        $intentRead = Read-ReferenceRecord `
            -Bytes $store `
            -Offset (Get-ReferenceRecordOffset `
                -AxisReference 1 -Slot 0) `
            -Slot 0
        Assert-ReferenceEqual 'Valid' $intentRead.Classification `
            ($terminalCrash + ' terminal failure must retain intent.')
        Assert-ReferenceEqual 1 $intentRead.Record.RecordState `
            ($terminalCrash + ' terminal failure must retain Armed state.')
        $terminalQuery = Invoke-ReferenceQuery -Store $store -Key $key
        if ($terminalCrash -eq 'DuringMarkerWrite') {
            Assert-ReferenceEqual 21 $terminalQuery.DetailCode `
                'Partial terminal marker must return detail 21.'
        }
        elseif ($terminalCrash -eq 'AfterMarkerWrite') {
            Assert-ReferenceTrue $terminalQuery.Success `
                'Final terminal marker before readback must be boot-valid.'
            Assert-ReferenceEqual `
                $intentRead.Record.RecordGeneration `
                $terminalQuery.Record.RecordGeneration `
                'Final-marker terminal must match the retained Armed generation.'
        }
        else {
            Assert-ReferenceEqual 20 `
                $terminalQuery.DetailCode `
                'Incomplete terminal plus Armed must return detail 20.'
        }
    }

    foreach ($failureCase in $failureCases) {
        $store = New-ReferenceStore
        $result = Invoke-ReferenceStart `
            -Store $store -Key $key `
            -IntentFailureAt $failureCase.Name
        $subcaseCount++
        $maximumNativeCount = [Math]::Max(
            $maximumNativeCount, $result.NativeCount)
        Assert-ReferenceEqual 24 $result.DetailCode `
            ($failureCase.Name + ' intent fault must return detail 24.')
        Assert-ReferenceEqual 0 $result.NativeCount `
            ($failureCase.Name + ' intent fault must be pre-native.')
        Assert-ReferenceEqual $failureCase.Mutations `
            $result.MutationCount `
            ($failureCase.Name + ' intent mutation count changed.')
        $query = Invoke-ReferenceQuery -Store $store -Key $key
        if ($failureCase.Query -eq 'Corrupt') {
            Assert-ReferenceEqual 21 $query.DetailCode `
                ($failureCase.Name + ' intent fault must be corrupt.')
        }
        elseif ($failureCase.Query -eq 'Valid') {
            Assert-ReferenceEqual 20 $query.DetailCode `
                ($failureCase.Name + ' intent fault must expose Armed.')
        }
        else {
            Assert-ReferenceEqual 19 $query.DetailCode `
                ($failureCase.Name + ' intent fault must be uncommitted.')
        }
    }

    foreach ($failureCase in $failureCases) {
        $store = New-ReferenceStore
        $result = Invoke-ReferenceStart `
            -Store $store -Key $key `
            -TerminalFailureAt $failureCase.Name
        $subcaseCount++
        $maximumNativeCount = [Math]::Max(
            $maximumNativeCount, $result.NativeCount)
        Assert-ReferenceTrue $result.NoResponse `
            ($failureCase.Name + ' terminal fault must signal no response.')
        Assert-ReferenceEqual -12 $result.ResultCode `
            ($failureCase.Name + ' terminal fault must return exact -12.')
        Assert-ReferenceEqual 1 $result.NativeCount `
            ($failureCase.Name + ' terminal fault must call native once.')
        Assert-ReferenceEqual (3 + $failureCase.Mutations) `
            $result.MutationCount `
            ($failureCase.Name + ' terminal mutation count changed.')
        $intent = Read-ReferenceRecord `
            -Bytes $store `
            -Offset (Get-ReferenceRecordOffset `
                -AxisReference 1 -Slot 0) `
            -Slot 0
        Assert-ReferenceEqual 'Valid' $intent.Classification `
            ($failureCase.Name + ' terminal fault must retain Armed.')
        $query = Invoke-ReferenceQuery -Store $store -Key $key
        if ($failureCase.Query -eq 'Corrupt') {
            Assert-ReferenceEqual 21 $query.DetailCode `
                ($failureCase.Name + ' terminal fault must be corrupt.')
        }
        elseif ($failureCase.Query -eq 'Valid') {
            Assert-ReferenceTrue $query.Success `
                ($failureCase.Name + ' terminal fault must expose outcome.')
        }
        else {
            Assert-ReferenceEqual 20 $query.DetailCode `
                ($failureCase.Name + ' terminal fault must leave Armed-only.')
        }
    }

    $successStore = New-ReferenceStore
    $success = Invoke-ReferenceStart -Store $successStore -Key $key
    $subcaseCount++
    $maximumNativeCount = [Math]::Max(
        $maximumNativeCount, $success.NativeCount)
    Assert-ReferenceTrue $success.Success `
        'Durable success start must succeed.'
    Assert-ReferenceEqual 1 $success.NativeCount `
        'Durable success must call native exactly once.'
    $successBytes = Copy-ReferenceBytes $successStore
    $replay = Invoke-ReferenceStart -Store $successStore -Key $key
    Assert-ReferenceTrue $replay.Success `
        'Exact replay must return the durable result.'
    Assert-ReferenceTrue $replay.IsDuplicate `
        'Exact replay must be labeled duplicate.'
    Assert-ReferenceEqual 0 $replay.NativeCount `
        'Exact replay must perform no native motion.'
    Assert-ReferenceEqual 0 $replay.MutationCount `
        'Exact replay must perform no retained write.'
    Assert-ReferenceBytesEqual $successBytes $successStore `
        'Exact replay must preserve every store byte.'

    $preNativeStore = New-ReferenceStore
    $preNative = Invoke-ReferenceStart `
        -Store $preNativeStore -Key $key -Outcome 'PreNativeRejected'
    $subcaseCount++
    $maximumNativeCount = [Math]::Max(
        $maximumNativeCount, $preNative.NativeCount)
    Assert-ReferenceTrue $preNative.Success `
        'Pre-native rejection must commit a terminal result.'
    Assert-ReferenceEqual 0 $preNative.NativeCount `
        'Pre-native rejection must call no native motion.'
    Assert-ReferenceEqual 3 $preNative.Record.RecordState `
        'Pre-native rejection must persist Rejected.'
    Assert-ReferenceEqual 10 $preNative.Record.OriginalDetailCode `
        'Pre-native rejection detail changed.'

    $nativeStore = New-ReferenceStore
    $native = Invoke-ReferenceStart `
        -Store $nativeStore -Key $key -Outcome 'NativeRejected'
    $subcaseCount++
    $maximumNativeCount = [Math]::Max(
        $maximumNativeCount, $native.NativeCount)
    Assert-ReferenceTrue $native.Success `
        'Native rejection must commit a terminal result.'
    Assert-ReferenceEqual 1 $native.NativeCount `
        'Native rejection must call native exactly once.'
    Assert-ReferenceEqual 3 $native.Record.RecordState `
        'Native rejection must persist Rejected.'
    Assert-ReferenceEqual 11 $native.Record.OriginalDetailCode `
        'Native rejection detail changed.'

    Assert-ReferenceEqual 33 $subcaseCount `
        'Start crash/native subcase count changed.'
    Assert-ReferenceEqual 1 $maximumNativeCount `
        'Reference model may call native at most once per request.'
}

Invoke-ReferenceFixture -Name 'PreNativeRejectedTerminalFailureFence' -Body {
    $key = New-ReferenceRecoveryKey
    $crashCases = @(
        [pscustomobject]@{ Name = 'AfterMarkerClear'; Mutations = 1;
            Query = 'Armed' },
        [pscustomobject]@{ Name = 'AfterBodyWrite'; Mutations = 2;
            Query = 'Armed' },
        [pscustomobject]@{ Name = 'DuringMarkerWrite'; Mutations = 3;
            Query = 'Corrupt' },
        [pscustomobject]@{ Name = 'AfterMarkerWrite'; Mutations = 3;
            Query = 'Valid' })
    $failureCases = @(
        [pscustomobject]@{ Name = 'MarkerClearWriteFailure';
            Mutations = 0; Query = 'Armed' },
        [pscustomobject]@{ Name = 'MarkerClearReadbackFailure';
            Mutations = 1; Query = 'Armed' },
        [pscustomobject]@{ Name = 'MarkerClearReadbackMismatch';
            Mutations = 1; Query = 'Corrupt' },
        [pscustomobject]@{ Name = 'BodyWriteFailure';
            Mutations = 1; Query = 'Armed' },
        [pscustomobject]@{ Name = 'BodyReadbackFailure';
            Mutations = 2; Query = 'Armed' },
        [pscustomobject]@{ Name = 'BodyReadbackMismatch';
            Mutations = 2; Query = 'Armed' },
        [pscustomobject]@{ Name = 'FinalMarkerWriteFailure';
            Mutations = 2; Query = 'Armed' },
        [pscustomobject]@{ Name = 'FinalMarkerReadbackFailure';
            Mutations = 3; Query = 'Valid' },
        [pscustomobject]@{ Name = 'FinalMarkerReadbackMismatch';
            Mutations = 3; Query = 'Corrupt' },
        [pscustomobject]@{ Name = 'FinalRecordReadbackFailure';
            Mutations = 3; Query = 'Valid' },
        [pscustomobject]@{ Name = 'FinalRecordReadbackMismatch';
            Mutations = 3; Query = 'Corrupt' })
    Assert-ReferenceEqual 4 $crashCases.Count `
        'Pre-native terminal crash case count changed.'
    Assert-ReferenceEqual 11 $failureCases.Count `
        'Pre-native terminal failure case count changed.'

    $subcaseCount = 0
    foreach ($case in $crashCases) {
        $store = New-ReferenceStore
        $result = Invoke-ReferenceStart `
            -Store $store -Key $key `
            -Outcome 'PreNativeRejected' `
            -TerminalCrashAt $case.Name
        $subcaseCount++
        Assert-ReferenceTrue $result.NoResponse `
            ($case.Name +
             ' pre-native terminal crash must signal no-response.')
        Assert-ReferenceEqual -12 $result.ResultCode `
            ($case.Name +
             ' pre-native terminal crash must return exact -12.')
        Assert-ReferenceEqual 0 $result.NativeCount `
            ($case.Name +
             ' pre-native terminal crash must remain zero-native.')
        Assert-ReferenceEqual (3 + $case.Mutations) `
            $result.MutationCount `
            ($case.Name +
             ' pre-native terminal crash mutation count changed.')
        $intent = Read-ReferenceRecord `
            -Bytes $store `
            -Offset (Get-ReferenceRecordOffset `
                -AxisReference 1 -Slot 0) `
            -Slot 0
        Assert-ReferenceEqual 'Valid' $intent.Classification `
            ($case.Name + ' must retain durable Armed.')
        $query = Invoke-ReferenceQuery -Store $store -Key $key
        if ($case.Query -eq 'Corrupt') {
            Assert-ReferenceEqual 21 $query.DetailCode `
                ($case.Name + ' must expose corrupt terminal marker.')
        }
        elseif ($case.Query -eq 'Valid') {
            Assert-ReferenceTrue $query.Success `
                ($case.Name + ' must expose durable rejected terminal.')
            Assert-ReferenceEqual $script:StateRejected `
                $query.Record.RecordState `
                ($case.Name + ' must retain Rejected state.')
            Assert-ReferenceEqual 10 $query.Record.OriginalDetailCode `
                ($case.Name + ' must retain pre-native detail.')
            Assert-ReferenceEqual -31000 $query.Record.OriginalErrorId `
                ($case.Name + ' must retain pre-native error.')
            Assert-ReferenceEqual 0 $query.Record.NativeCommandState `
                ($case.Name + ' must retain zero native state.')
        }
        else {
            Assert-ReferenceEqual 20 $query.DetailCode `
                ($case.Name + ' must leave Armed-only indeterminate state.')
        }
    }

    foreach ($case in $failureCases) {
        $store = New-ReferenceStore
        $result = Invoke-ReferenceStart `
            -Store $store -Key $key `
            -Outcome 'PreNativeRejected' `
            -TerminalFailureAt $case.Name
        $subcaseCount++
        Assert-ReferenceTrue $result.NoResponse `
            ($case.Name +
             ' pre-native terminal failure must signal no-response.')
        Assert-ReferenceEqual -12 $result.ResultCode `
            ($case.Name +
             ' pre-native terminal failure must return exact -12.')
        Assert-ReferenceEqual 0 $result.NativeCount `
            ($case.Name +
             ' pre-native terminal failure must remain zero-native.')
        Assert-ReferenceEqual (3 + $case.Mutations) `
            $result.MutationCount `
            ($case.Name +
             ' pre-native terminal failure mutation count changed.')
        $intent = Read-ReferenceRecord `
            -Bytes $store `
            -Offset (Get-ReferenceRecordOffset `
                -AxisReference 1 -Slot 0) `
            -Slot 0
        Assert-ReferenceEqual 'Valid' $intent.Classification `
            ($case.Name + ' must retain durable Armed.')
        $query = Invoke-ReferenceQuery -Store $store -Key $key
        if ($case.Query -eq 'Corrupt') {
            Assert-ReferenceEqual 21 $query.DetailCode `
                ($case.Name + ' must expose corrupt terminal bytes.')
        }
        elseif ($case.Query -eq 'Valid') {
            Assert-ReferenceTrue $query.Success `
                ($case.Name + ' must expose durable rejected terminal.')
            Assert-ReferenceEqual $script:StateRejected `
                $query.Record.RecordState `
                ($case.Name + ' must retain Rejected state.')
            Assert-ReferenceEqual 10 $query.Record.OriginalDetailCode `
                ($case.Name + ' must retain pre-native detail.')
            Assert-ReferenceEqual -31000 $query.Record.OriginalErrorId `
                ($case.Name + ' must retain pre-native error.')
            Assert-ReferenceEqual 0 $query.Record.NativeCommandState `
                ($case.Name + ' must retain zero native state.')
        }
        else {
            Assert-ReferenceEqual 20 $query.DetailCode `
                ($case.Name + ' must leave Armed-only indeterminate state.')
        }
    }
    Assert-ReferenceEqual 15 $subcaseCount `
        'Pre-native rejected terminal fence subcase count changed.'
}

Invoke-ReferenceFixture -Name 'StagedBeginPreArmedGateMatrix' -Body {
    $key = New-ReferenceRecoveryKey
    $preArmedDetails = @(
        1, 2, 3, 4, 5, 6, 7, 8, 9,
        16, 17, 18, 24)
    Assert-ReferenceEqual 13 $preArmedDetails.Count `
        'Pre-Armed gate matrix count changed.'
    foreach ($detail in $preArmedDetails) {
        $store = New-ReferenceStore
        $before = Copy-ReferenceBytes $store
        $transaction = New-ReferenceSetPositionTransaction
        $result = Invoke-ReferenceBeginSetPosition `
            -Store $store -Transaction $transaction -Key $key `
            -PreArmedDetailCode $detail
        Assert-ReferenceTrue (-not $result.Success) `
            ('Pre-Armed detail ' + $detail + ' must fail Begin.')
        Assert-ReferenceEqual $detail $result.DetailCode `
            ('Pre-Armed detail ' + $detail + ' changed.')
        Assert-ReferenceEqual 0 $result.ResultCode `
            ('Pre-Armed detail ' + $detail + ' must return zero.')
        Assert-ReferenceEqual 0 $result.MutationCount `
            ('Pre-Armed detail ' + $detail + ' must be zero-write.')
        Assert-ReferenceEqual 0 $result.NativeCount `
            ('Pre-Armed detail ' + $detail + ' must be zero-native.')
        Assert-ReferenceTrue (-not $transaction.Active) `
            ('Pre-Armed detail ' + $detail +
             ' may not create a transaction.')
        Assert-ReferenceBytesEqual $before $store `
            ('Pre-Armed detail ' + $detail +
             ' must preserve retained bytes.')
    }


    $otherKey = New-ReferenceRecoveryKey `
        -OriginalRequestId 9002 -Intent0 902
    $occupiedStore = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $occupiedStore -AxisReference 1 -Slot 0 `
        -RecordBytes (New-ReferenceArmedRecordBytes `
            -Key $otherKey -StoreGeneration 1 -RecordGeneration 1)
    $occupiedTransaction = New-ReferenceSetPositionTransaction
    $occupied = Invoke-ReferenceBeginSetPosition `
        -Store $occupiedStore -Transaction $occupiedTransaction -Key $key
    Assert-ReferenceEqual 23 $occupied.DetailCode `
        'Detail 23 must come from store admission, not a pre-store gate.'
    Assert-ReferenceEqual 0 $occupied.MutationCount `
        'Occupied admission must be zero-write.'

    $corruptOccupiedStore = Copy-ReferenceBytes $occupiedStore
    $corruptTerminal = New-ReferenceSucceededRecordBytes `
        -Key $otherKey -StoreGeneration 2 -RecordGeneration 1
    Set-ReferenceUInt32LE $corruptTerminal $script:CommitMarkerOffset 1
    Set-ReferenceStoreRecordDirect `
        -Store $corruptOccupiedStore -AxisReference 1 -Slot 1 `
        -RecordBytes $corruptTerminal
    $corruptTransaction = New-ReferenceSetPositionTransaction
    $corruptBefore = Copy-ReferenceBytes $corruptOccupiedStore
    $corrupt = Invoke-ReferenceBeginSetPosition `
        -Store $corruptOccupiedStore `
        -Transaction $corruptTransaction -Key $key
    Assert-ReferenceEqual 21 $corrupt.DetailCode `
        'Store corruption must precede occupied detail 23.'
    Assert-ReferenceEqual 0 $corrupt.MutationCount `
        'Corrupt-before-occupied admission must be zero-write.'
    Assert-ReferenceBytesEqual $corruptBefore $corruptOccupiedStore `
        'Corrupt-before-occupied admission must preserve retained bytes.'
}

Invoke-ReferenceFixture -Name 'StagedCommitTransactionContract' -Body {
    $key = New-ReferenceRecoveryKey

    $withoutBeginStore = New-ReferenceStore
    $withoutBeginTransaction = New-ReferenceSetPositionTransaction
    $withoutBegin = Invoke-ReferenceCommitSetPositionTerminal `
        -Store $withoutBeginStore `
        -Transaction $withoutBeginTransaction `
        -Key $key -RecordGeneration 1 `
        -RecordState $script:StateSucceeded `
        -AppliedPosition $key.TargetPosition `
        -OriginalCommandStatus 0 -OriginalErrorId 0 `
        -OriginalDetailCode 0 -NativeCommandState 0
    Assert-ReferenceTrue (-not $withoutBegin.Success) `
        'Commit without Begin must fail.'
    Assert-ReferenceEqual 0 $withoutBegin.ResultCode `
        'Commit without Begin must return domain failure zero.'
    Assert-ReferenceEqual 24 $withoutBegin.DetailCode `
        'Commit without Begin must return detail 24.'
    Assert-ReferenceTrue (-not $withoutBegin.NoResponse) `
        'Commit without durable Begin may not emit -12.'
    Assert-ReferenceEqual 0 $withoutBegin.MutationCount `
        'Commit without Begin must be zero-write.'

    $keyMismatchStore = New-ReferenceStore
    $keyMismatchTransaction = New-ReferenceSetPositionTransaction
    $keyMismatchBegin = Invoke-ReferenceBeginSetPosition `
        -Store $keyMismatchStore `
        -Transaction $keyMismatchTransaction -Key $key
    Assert-ReferenceTrue $keyMismatchBegin.Success `
        'Key-mismatch setup Begin must succeed.'
    $otherKey = New-ReferenceRecoveryKey `
        -OriginalRequestId 9002 -Intent0 902
    $keyMismatch = Invoke-ReferenceCommitSetPositionTerminal `
        -Store $keyMismatchStore `
        -Transaction $keyMismatchTransaction `
        -Key $otherKey `
        -RecordGeneration $keyMismatchBegin.Record.RecordGeneration `
        -RecordState $script:StateSucceeded `
        -AppliedPosition $otherKey.TargetPosition `
        -OriginalCommandStatus 0 -OriginalErrorId 0 `
        -OriginalDetailCode 0 -NativeCommandState 0
    Assert-ReferenceEqual -12 $keyMismatch.ResultCode `
        'Post-Begin key mismatch must return exact -12.'
    Assert-ReferenceTrue $keyMismatch.NoResponse `
        'Post-Begin key mismatch must close without response.'
    Assert-ReferenceTrue (-not $keyMismatchTransaction.Active) `
        'Key-mismatch Commit must consume the transaction.'
    Assert-ReferenceEqual 0 $keyMismatch.MutationCount `
        'Key-mismatch Commit must not write a terminal.'
    Assert-ReferenceEqual 20 `
        (Invoke-ReferenceQuery `
            -Store $keyMismatchStore -Key $key).DetailCode `
        'Key-mismatch Commit must leave durable Armed only.'

    $generationStore = New-ReferenceStore
    $generationTransaction = New-ReferenceSetPositionTransaction
    $generationBegin = Invoke-ReferenceBeginSetPosition `
        -Store $generationStore `
        -Transaction $generationTransaction -Key $key
    $wrongGeneration = [uint32]($generationBegin.Record.RecordGeneration + 1)
    $generationMismatch = Invoke-ReferenceCommitSetPositionTerminal `
        -Store $generationStore `
        -Transaction $generationTransaction `
        -Key $key -RecordGeneration $wrongGeneration `
        -RecordState $script:StateSucceeded `
        -AppliedPosition $key.TargetPosition `
        -OriginalCommandStatus 0 -OriginalErrorId 0 `
        -OriginalDetailCode 0 -NativeCommandState 0
    Assert-ReferenceEqual -12 $generationMismatch.ResultCode `
        'Post-Begin generation mismatch must return exact -12.'
    Assert-ReferenceTrue $generationMismatch.NoResponse `
        'Post-Begin generation mismatch must close without response.'
    Assert-ReferenceTrue (-not $generationTransaction.Active) `
        'Generation-mismatch Commit must consume the transaction.'
    Assert-ReferenceEqual 0 $generationMismatch.MutationCount `
        'Generation-mismatch Commit must be zero-terminal-write.'

    $capacityStore = New-ReferenceStore
    $capacityTransaction = New-ReferenceSetPositionTransaction
    $capacityBegin = Invoke-ReferenceBeginSetPosition `
        -Store $capacityStore -Transaction $capacityTransaction -Key $key
    $capacityFailure = Invoke-ReferenceCommitSetPositionTerminal `
        -Store $capacityStore -Transaction $capacityTransaction `
        -Key $key `
        -RecordGeneration $capacityBegin.Record.RecordGeneration `
        -RecordState $script:StateSucceeded `
        -AppliedPosition $key.TargetPosition `
        -OriginalCommandStatus 0 -OriginalErrorId 0 `
        -OriginalDetailCode 0 -NativeCommandState 0 `
        -SnapshotCapacity 67
    Assert-ReferenceEqual -12 $capacityFailure.ResultCode `
        'Post-Begin Commit capacity failure must return exact -12.'
    Assert-ReferenceTrue $capacityFailure.NoResponse `
        'Post-Begin Commit capacity failure must close without response.'
    Assert-ReferenceTrue (-not $capacityTransaction.Active) `
        'Commit capacity failure must consume the transaction.'
    Assert-ReferenceEqual 0 $capacityFailure.MutationCount `
        'Commit capacity failure must be zero-terminal-write.'

    $targetStore = New-ReferenceStore
    $targetTransaction = New-ReferenceSetPositionTransaction
    $targetBegin = Invoke-ReferenceBeginSetPosition `
        -Store $targetStore -Transaction $targetTransaction -Key $key
    $targetTransaction.TerminalTargetSlot =
        if ($targetTransaction.ReservedTerminalTargetSlot -eq 1) { 2 }
        else { 1 }
    $targetDrift = Invoke-ReferenceCommitSetPositionTerminal `
        -Store $targetStore -Transaction $targetTransaction `
        -Key $key `
        -RecordGeneration $targetBegin.Record.RecordGeneration `
        -RecordState $script:StateSucceeded `
        -AppliedPosition $key.TargetPosition `
        -OriginalCommandStatus 0 -OriginalErrorId 0 `
        -OriginalDetailCode 0 -NativeCommandState 0
    Assert-ReferenceEqual -12 $targetDrift.ResultCode `
        'Reserved terminal target drift must return exact -12.'
    Assert-ReferenceTrue $targetDrift.NoResponse `
        'Reserved terminal target drift must close without response.'
    Assert-ReferenceTrue (-not $targetTransaction.Active) `
        'Target-drift Commit must consume the transaction.'
    Assert-ReferenceEqual 0 $targetDrift.MutationCount `
        'Target-drift Commit must be zero-terminal-write.'

    $storeGenerationStore = New-ReferenceStore
    $storeGenerationTransaction = New-ReferenceSetPositionTransaction
    $storeGenerationBegin = Invoke-ReferenceBeginSetPosition `
        -Store $storeGenerationStore `
        -Transaction $storeGenerationTransaction -Key $key
    $storeGenerationTransaction.TerminalStoreGeneration = [uint32](
        $storeGenerationTransaction.ReservedTerminalStoreGeneration + 1)
    $storeGenerationDrift = Invoke-ReferenceCommitSetPositionTerminal `
        -Store $storeGenerationStore `
        -Transaction $storeGenerationTransaction `
        -Key $key `
        -RecordGeneration $storeGenerationBegin.Record.RecordGeneration `
        -RecordState $script:StateSucceeded `
        -AppliedPosition $key.TargetPosition `
        -OriginalCommandStatus 0 -OriginalErrorId 0 `
        -OriginalDetailCode 0 -NativeCommandState 0
    Assert-ReferenceEqual -12 $storeGenerationDrift.ResultCode `
        'Reserved terminal StoreGeneration drift must return exact -12.'
    Assert-ReferenceTrue $storeGenerationDrift.NoResponse `
        'Reserved StoreGeneration drift must close without response.'
    Assert-ReferenceTrue (-not $storeGenerationTransaction.Active) `
        'StoreGeneration-drift Commit must consume the transaction.'
    Assert-ReferenceEqual 0 $storeGenerationDrift.MutationCount `
        'StoreGeneration-drift Commit must be zero-terminal-write.'

    $targetBytesStore = New-ReferenceStore
    $targetBytesTransaction = New-ReferenceSetPositionTransaction
    $targetBytesBegin = Invoke-ReferenceBeginSetPosition `
        -Store $targetBytesStore `
        -Transaction $targetBytesTransaction -Key $key
    $targetOffset = Get-ReferenceRecordOffset `
        -AxisReference 1 `
        -Slot $targetBytesTransaction.TerminalTargetSlot
    $targetBytesStore[$targetOffset] = 1
    $targetBytesDrift = Invoke-ReferenceCommitSetPositionTerminal `
        -Store $targetBytesStore `
        -Transaction $targetBytesTransaction `
        -Key $key `
        -RecordGeneration $targetBytesBegin.Record.RecordGeneration `
        -RecordState $script:StateSucceeded `
        -AppliedPosition $key.TargetPosition `
        -OriginalCommandStatus 0 -OriginalErrorId 0 `
        -OriginalDetailCode 0 -NativeCommandState 0
    Assert-ReferenceEqual -12 $targetBytesDrift.ResultCode `
        'Reserved target byte drift must return exact -12.'
    Assert-ReferenceTrue $targetBytesDrift.NoResponse `
        'Reserved target byte drift must close without response.'
    Assert-ReferenceTrue (-not $targetBytesTransaction.Active) `
        'Target-byte-drift Commit must consume the transaction.'

    $failureStore = New-ReferenceStore
    $failureTransaction = New-ReferenceSetPositionTransaction
    $failureBegin = Invoke-ReferenceBeginSetPosition `
        -Store $failureStore `
        -Transaction $failureTransaction -Key $key
    $commitFailure = Invoke-ReferenceCommitSetPositionTerminal `
        -Store $failureStore -Transaction $failureTransaction `
        -Key $key `
        -RecordGeneration $failureBegin.Record.RecordGeneration `
        -RecordState $script:StateSucceeded `
        -AppliedPosition $key.TargetPosition `
        -OriginalCommandStatus 0 -OriginalErrorId 0 `
        -OriginalDetailCode 0 -NativeCommandState 0 `
        -TerminalFailureAt 'BodyReadbackFailure'
    Assert-ReferenceEqual -12 $commitFailure.ResultCode `
        'Terminal readback failure must return exact -12.'
    Assert-ReferenceTrue $commitFailure.NoResponse `
        'Terminal readback failure must close without response.'
    Assert-ReferenceEqual 2 $commitFailure.MutationCount `
        'Terminal body readback failure mutation count changed.'
    Assert-ReferenceTrue (-not $failureTransaction.Active) `
        'Commit readback failure must consume the transaction.'

    $successStore = New-ReferenceStore
    $successTransaction = New-ReferenceSetPositionTransaction
    $successBegin = Invoke-ReferenceBeginSetPosition `
        -Store $successStore -Transaction $successTransaction -Key $key
    $success = Invoke-ReferenceCommitSetPositionTerminal `
        -Store $successStore -Transaction $successTransaction `
        -Key $key `
        -RecordGeneration $successBegin.Record.RecordGeneration `
        -RecordState $script:StateSucceeded `
        -AppliedPosition $key.TargetPosition `
        -OriginalCommandStatus 0 -OriginalErrorId 0 `
        -OriginalDetailCode 0 -NativeCommandState 0
    Assert-ReferenceTrue $success.Success `
        'Valid staged Commit must succeed.'
    Assert-ReferenceEqual 1 $success.ResultCode `
        'Valid staged Commit must return one.'
    Assert-ReferenceTrue (-not $successTransaction.Active) `
        'Successful Commit must consume the transaction.'
    $afterSuccessBytes = Copy-ReferenceBytes $successStore
    $doubleCommit = Invoke-ReferenceCommitSetPositionTerminal `
        -Store $successStore -Transaction $successTransaction `
        -Key $key `
        -RecordGeneration $successBegin.Record.RecordGeneration `
        -RecordState $script:StateSucceeded `
        -AppliedPosition $key.TargetPosition `
        -OriginalCommandStatus 0 -OriginalErrorId 0 `
        -OriginalDetailCode 0 -NativeCommandState 0
    Assert-ReferenceEqual 0 $doubleCommit.ResultCode `
        'Double Commit must return domain failure zero.'
    Assert-ReferenceEqual 24 $doubleCommit.DetailCode `
        'Double Commit must return detail 24.'
    Assert-ReferenceTrue (-not $doubleCommit.NoResponse) `
        'Double Commit without an active transaction may not emit -12.'
    Assert-ReferenceEqual 0 $doubleCommit.MutationCount `
        'Double Commit must be zero-write.'
    Assert-ReferenceBytesEqual $afterSuccessBytes $successStore `
        'Double Commit must preserve the durable terminal.'

    $replayStore = New-ReferenceStore
    $combined = Invoke-ReferenceStart -Store $replayStore -Key $key
    Assert-ReferenceTrue $combined.Success `
        'Staged replay setup must succeed.'
    $replayBefore = Copy-ReferenceBytes $replayStore
    $replayTransaction = New-ReferenceSetPositionTransaction
    $replay = Invoke-ReferenceBeginSetPosition `
        -Store $replayStore -Transaction $replayTransaction -Key $key
    Assert-ReferenceTrue $replay.Success `
        'Begin must replay an exact stored terminal.'
    Assert-ReferenceEqual 2 $replay.ResultCode `
        'Stored terminal replay must return Begin result two.'
    Assert-ReferenceTrue $replay.IsDuplicate `
        'Stored terminal replay must be labeled duplicate.'
    Assert-ReferenceTrue (-not $replayTransaction.Active) `
        'Stored terminal replay may not create a transaction.'
    Assert-ReferenceEqual 0 $replay.MutationCount `
        'Stored terminal replay must be zero-write.'
    Assert-ReferenceBytesEqual $replayBefore $replayStore `
        'Stored terminal replay must preserve retained bytes.'
}

Invoke-ReferenceFixture -Name 'StagedActiveTransactionAxisFence' -Body {
    $axis1Key = New-ReferenceRecoveryKey -AxisReference 1
    $axis2Key = New-ReferenceRecoveryKey `
        -AxisReference 2 -OriginalRequestId 9202 -Intent0 922
    $store = New-ReferenceStore
    Set-ReferenceStoreRecordDirect `
        -Store $store -AxisReference 2 -Slot 1 `
        -RecordBytes (New-ReferenceSucceededRecordBytes `
            -Key $axis2Key -StoreGeneration 1 -RecordGeneration 1 `
            -Tombstone)
    $transaction = New-ReferenceSetPositionTransaction
    $begin = Invoke-ReferenceBeginSetPosition `
        -Store $store -Transaction $transaction -Key $axis1Key
    Assert-ReferenceTrue $begin.Success `
        'Axis-fence Begin setup must succeed.'
    Assert-ReferenceTrue $transaction.Active `
        'Successful Begin must activate its transaction.'
    Assert-ReferenceEqual 1 $transaction.AxisReference `
        'Active transaction axis changed.'
    $afterBeginBytes = Copy-ReferenceBytes $store

    $sameAxisRead = Invoke-ReferenceReadSetPositionOutcome `
        -Store $store -Transaction $transaction -Key $axis1Key
    Assert-ReferenceTrue (-not $sameAxisRead.Success) `
        'Same-axis staged Read must fail while transaction is active.'
    Assert-ReferenceEqual 24 $sameAxisRead.DetailCode `
        'Same-axis active Read must fail closed with detail 24.'
    Assert-ReferenceEqual 0 $sameAxisRead.MutationCount `
        'Same-axis active Read must be zero-write.'
    Assert-ReferenceTrue $transaction.Active `
        'Same-axis Read must not consume the transaction.'

    $sameAxisRetire = Invoke-ReferenceRetireSetPositionOutcome `
        -Store $store -Transaction $transaction -Key $axis1Key `
        -ExpectedRecordGeneration $begin.Record.RecordGeneration
    Assert-ReferenceTrue (-not $sameAxisRetire.Success) `
        'Same-axis staged Retire must fail while transaction is active.'
    Assert-ReferenceEqual 24 $sameAxisRetire.DetailCode `
        'Same-axis active Retire must fail closed with detail 24.'
    Assert-ReferenceEqual 0 $sameAxisRetire.MutationCount `
        'Same-axis active Retire must be zero-write.'
    Assert-ReferenceTrue $transaction.Active `
        'Same-axis Retire must not consume the transaction.'
    Assert-ReferenceBytesEqual $afterBeginBytes $store `
        'Same-axis Read/Retire fence must preserve retained bytes.'

    $otherAxisRead = Invoke-ReferenceReadSetPositionOutcome `
        -Store $store -Transaction $transaction -Key $axis2Key
    Assert-ReferenceTrue $otherAxisRead.Success `
        'A different-axis staged Read must remain allowed.'
    Assert-ReferenceTrue $otherAxisRead.Record.IsTombstone `
        'Different-axis Read must return its exact tombstone.'
    Assert-ReferenceTrue $transaction.Active `
        'Different-axis Read must not consume another axis transaction.'

    $otherAxisRetire = Invoke-ReferenceRetireSetPositionOutcome `
        -Store $store -Transaction $transaction -Key $axis2Key `
        -ExpectedRecordGeneration 1
    Assert-ReferenceTrue $otherAxisRetire.Success `
        'A different-axis staged Retire must remain allowed.'
    Assert-ReferenceTrue $otherAxisRetire.IsDuplicate `
        'Different-axis tombstone Retire must be idempotent.'
    Assert-ReferenceEqual 0 $otherAxisRetire.MutationCount `
        'Different-axis duplicate Retire must be zero-write.'
    Assert-ReferenceTrue $transaction.Active `
        'Different-axis Retire must not consume another axis transaction.'

    $commit = Invoke-ReferenceCommitSetPositionTerminal `
        -Store $store -Transaction $transaction -Key $axis1Key `
        -RecordGeneration $begin.Record.RecordGeneration `
        -RecordState $script:StateSucceeded `
        -AppliedPosition $axis1Key.TargetPosition `
        -OriginalCommandStatus 0 -OriginalErrorId 0 `
        -OriginalDetailCode 0 -NativeCommandState 0
    Assert-ReferenceTrue $commit.Success `
        'Axis1 Commit must still succeed after different-axis operations.'
    Assert-ReferenceTrue (-not $transaction.Active) `
        'Final Commit must consume the axis1 transaction.'
}

Invoke-ReferenceFixture -Name 'StagedRejectedDetailDomain' -Body {
    $key = New-ReferenceRecoveryKey
    $allowedDetails = @(10, 11, 12, 13, 14, 15)
    Assert-ReferenceEqual 6 $allowedDetails.Count `
        'Stored Rejected detail allow-list count changed.'
    foreach ($detail in $allowedDetails) {
        $store = New-ReferenceStore
        $transaction = New-ReferenceSetPositionTransaction
        $begin = Invoke-ReferenceBeginSetPosition `
            -Store $store -Transaction $transaction -Key $key
        Assert-ReferenceTrue $begin.Success `
            ('Rejected detail ' + $detail + ' Begin must succeed.')
        if ($detail -eq 11) {
            $errorId = -6
            $nativeState = [Convert]::ToUInt32('A5A5A5A5', 16)
        }
        else {
            $errorId = -31000
            $nativeState = [uint32]0
        }
        $commit = Invoke-ReferenceCommitSetPositionTerminal `
            -Store $store -Transaction $transaction -Key $key `
            -RecordGeneration $begin.Record.RecordGeneration `
            -RecordState $script:StateRejected `
            -AppliedPosition 0 -OriginalCommandStatus 1 `
            -OriginalErrorId $errorId `
            -OriginalDetailCode $detail `
            -NativeCommandState $nativeState
        Assert-ReferenceTrue $commit.Success `
            ('Rejected detail ' + $detail + ' must commit durably.')
        Assert-ReferenceEqual 1 $commit.ResultCode `
            ('Rejected detail ' + $detail + ' Commit must return one.')
        Assert-ReferenceTrue (-not $transaction.Active) `
            ('Rejected detail ' + $detail +
             ' Commit must consume the transaction.')
        $read = Invoke-ReferenceReadSetPositionOutcome `
            -Store $store -Transaction $transaction -Key $key
        Assert-ReferenceTrue $read.Success `
            ('Rejected detail ' + $detail + ' must be queryable.')
        Assert-ReferenceEqual $script:StateRejected `
            $read.Record.RecordState `
            ('Rejected detail ' + $detail +
             ' must retain Rejected state.')
        Assert-ReferenceEqual $detail $read.Record.OriginalDetailCode `
            ('Rejected detail ' + $detail + ' snapshot changed.')
    }

    $forbiddenDetails = @(
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
        16, 17, 18, 19, 20, 21, 22, 23, 24)
    Assert-ReferenceEqual 19 $forbiddenDetails.Count `
        'Stored Rejected detail deny-list count changed.'
    foreach ($detail in $forbiddenDetails) {
        $store = New-ReferenceStore
        $transaction = New-ReferenceSetPositionTransaction
        $begin = Invoke-ReferenceBeginSetPosition `
            -Store $store -Transaction $transaction -Key $key
        Assert-ReferenceTrue $begin.Success `
            ('Forbidden detail ' + $detail + ' Begin must succeed.')
        $commit = Invoke-ReferenceCommitSetPositionTerminal `
            -Store $store -Transaction $transaction -Key $key `
            -RecordGeneration $begin.Record.RecordGeneration `
            -RecordState $script:StateRejected `
            -AppliedPosition 0 -OriginalCommandStatus 1 `
            -OriginalErrorId -31000 `
            -OriginalDetailCode $detail -NativeCommandState 0
        Assert-ReferenceTrue (-not $commit.Success) `
            ('Forbidden detail ' + $detail + ' may not commit.')
        Assert-ReferenceEqual -12 $commit.ResultCode `
            ('Forbidden detail ' + $detail +
             ' after Begin must return exact -12.')
        Assert-ReferenceTrue $commit.NoResponse `
            ('Forbidden detail ' + $detail +
             ' after Begin must close without response.')
        Assert-ReferenceEqual 0 $commit.MutationCount `
            ('Forbidden detail ' + $detail +
             ' must fail before terminal write.')
        Assert-ReferenceTrue (-not $transaction.Active) `
            ('Forbidden detail ' + $detail +
             ' Commit must consume the transaction.')
        $read = Invoke-ReferenceReadSetPositionOutcome `
            -Store $store -Transaction $transaction -Key $key
        Assert-ReferenceEqual 20 $read.DetailCode `
            ('Forbidden detail ' + $detail +
             ' must leave Armed-only state.')
    }
}

$expectedFixtureCount = 41
if ($script:FixtureCount -ne $expectedFixtureCount) {
    throw (
        'SetPosition retained-store fixture count changed. Expected=' +
        $expectedFixtureCount + ', Actual=' + $script:FixtureCount + '.')
}

Write-Host (
    'PASS REFERENCE MODEL ONLY ' +
    'Verify-LasalSetPositionRetainedStoreReference.ps1 ' +
    '(' + $script:FixtureCount + '/' + $expectedFixtureCount +
    ' fixtures, ' + $script:AssertionCount +
    ' assertions, mode=SelfTestOnly).')
