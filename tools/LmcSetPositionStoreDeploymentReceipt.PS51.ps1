# Windows PowerShell 5.1 compatibility override.
#
# PowerShell 5.1 can throw "Argument types do not match" when a generic
# List[object] is expanded through @(...). Keep the receipt parser contract
# identical while materializing records through ArrayList.ToArray().

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
