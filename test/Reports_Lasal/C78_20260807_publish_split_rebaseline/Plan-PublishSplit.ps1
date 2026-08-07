param(
    [string]$RepositoryRoot = (Join-Path $PSScriptRoot '..\..\..'),
    [string]$SnapshotPath = '',
    [string]$EmitApplyPatchPath = '',
    [string]$EmitCompactApplyPatchPrefix = '',
    [switch]$RequirePreSplitTarget,
    [switch]$RequireLfPreSplitTarget,
    [switch]$RunSelfTest
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# Default planning and self-test are intentionally read-only. Optional emission
# may CreateNew the logical proof or three compact apply_patch documents in this
# exact evidence directory; it never writes LASAL source or generated data.
$ExpectedSourceCanonicalLfSha256 =
    'F923D5F5A2649B33911072537BFF4B9CB597FAB1C3C8E1D956C8AB5F3C80B2DC'
$ExpectedSourceIdeCrlfSha256 =
    'C636265238F44D73FDC483309BFB1FF48384EFCD7AF44EE487071CB467281AE5'
$ExpectedSourceCanonicalLfBytes = 593113
$ExpectedSourceIdeCrlfBytes = 609947
$ExpectedMethodCanonicalLfSha256 =
    '688241F3FD3DE43DC9B95B7A4AB0E7160C2F31D7FCFD4529AC18E8946E034F18'
$ExpectedHomeExtractionCanonicalLfSha256 =
    '84A8FE035018CC10F595EEF8024357E0EDD75035BAE9C51F11B85D49547FBFF1'
$ExpectedHomeExtractionIdeCrlfSha256 =
    '3B9C74787829FDF51B1F7E3EF2F7DB4FE1519AC17A7C146C60B56E0785507E2D'
$ExpectedDecisionExtractionCanonicalLfSha256 =
    '90B7E61AA6F4F85896835C7F0EE05855930FE73A891546E8832A46196213DB8E'
$ExpectedDecisionExtractionIdeCrlfSha256 =
    'E2E06E5ADBF2F526C765E893512D365C008A6AE9BA1C1494500BB1133D5D58A3'
$ExpectedHomeDeclarationCanonicalLfSha256 =
    '9646C411E51D4EC072C3A5BD1AF888CF6861FD548F6856F3B2E17FD9BB15EE43'
$ExpectedDecisionDeclarationCanonicalLfSha256 =
    '514A99A5A4BE8D8C6B23E2373B6D0F836054E7F2C62CA1C4033E51F606DAE1F8'
$ExpectedHomeEmptyStubCanonicalLfSha256 =
    '10A87A7F2C280A65379E74AACFBB95C238B8182549E7FF65A48EFC68E43F2D0B'
$ExpectedDecisionEmptyStubCanonicalLfSha256 =
    '577932A085940A8B79260724A4C31BB057CCDEB6478A78620A600EE151B66ACD'
$ExpectedAdapterCanonicalLfBytes = 26265
$ExpectedAdapterCanonicalLfSha256 =
    '355A0EA77E13D0CA612BDBD9FA0A55FCA5233B33D3C4DEAC91F5BAEED2B108BE'
$ExpectedHomeHelperCanonicalLfBytes = 15035
$ExpectedHomeHelperCanonicalLfSha256 =
    'EF68864255B888F8E579AE066BB65C1313349B8BE44E0FCEB402FE2DF4DCC849'
$ExpectedDecisionHelperCanonicalLfBytes = 24708
$ExpectedDecisionHelperCanonicalLfSha256 =
    '75804F7C0681D51416E75C55D54038162E71768EAFF00C4057F8200D138FC377'
$ExpectedCandidateCanonicalLfBytes = 594938
$ExpectedCandidateCanonicalLfSha256 =
    '8715896406D3B99185C40FBE9C2F0E29170C2D57E1E58792515172EBDDC81E65'
$ExpectedCandidateIdeCrlfBytes = 611837
$ExpectedCandidateIdeCrlfSha256 =
    'B6A3D9368AA5A81ADD58B002A8504607443ACDAA6AD176E8193FFEBEC9552636'
$ExpectedAdapterStateCanonicalLfBytes = 555935
$ExpectedAdapterStateCanonicalLfLines = 15891
$ExpectedAdapterStateCanonicalLfSha256 =
    'D50F50C9D172D27F1909AFD088A98E6CA4E1B033BCB1A88C28DEF83140C7AE74'
$ExpectedHomeStateCanonicalLfBytes = 570648
$ExpectedHomeStateCanonicalLfLines = 16244
$ExpectedHomeStateCanonicalLfSha256 =
    '5518AB8335A0AAE5E57C13C0B8BB946A2B8CFAF948FBC182E797502DCEF985B5'
$ExpectedCompactAdapterPatchBytes = 41640
$ExpectedCompactAdapterPatchSha256 =
    'DD243404411966E88BAFCFDB0C43CA538961EE9F163778606EE0DC71B47A89C0'
$ExpectedCompactHomePatchBytes = 15270
$ExpectedCompactHomePatchSha256 =
    '846C064C999D1A4AA3290B09DF26E3EFE72DE8E2721F929C06589575150B8031'
$ExpectedCompactDecisionPatchBytes = 25147
$ExpectedCompactDecisionPatchSha256 =
    '646A1BC3FD88B0BAEF6E7022D3737258CDD6FB05A8B38E9FED48C397CB5746AE'
$ExpectedLogicalProofPatchBytes = 133828
$ExpectedLogicalProofPatchSha256 =
    '7F2F3B8FE70423037F39A958006BF11919F7518974CE79779430C0FF14FE39A1'
$MethodSizeLimitBytes = 32768
$Utf8 = [Text.UTF8Encoding]::new($false, $true)
$CanonicalTargetRelativePath = (
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/' +
    'LMCControlCommandService/LMCControlCommandService.st')

function ConvertTo-CanonicalLf {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    return $Text.Replace("`r`n", "`n").Replace("`r", "`n")
}

function ConvertTo-IdeCrlf {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    return (ConvertTo-CanonicalLf -Text $Text).Replace("`n", "`r`n")
}

function Get-TextSha256 {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    return [Convert]::ToHexString(
        [Security.Cryptography.SHA256]::HashData($Utf8.GetBytes($Text)))
}

function Get-BytesSha256 {
    param([Parameter(Mandatory = $true)][byte[]]$Bytes)

    return [Convert]::ToHexString(
        [Security.Cryptography.SHA256]::HashData($Bytes))
}

function ConvertFrom-StrictAsciiUtf8Bytes {
    param(
        [Parameter(Mandatory = $true)][byte[]]$RawBytes,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    if (($RawBytes.Count -ge 3) -and
        ($RawBytes[0] -eq 0xEF) -and
        ($RawBytes[1] -eq 0xBB) -and
        ($RawBytes[2] -eq 0xBF)) {
        throw "$Owner has a forbidden UTF-8 BOM."
    }
    foreach ($value in $RawBytes) {
        if ($value -gt 0x7F) {
            throw "$Owner contains a non-ASCII byte (0x$('{0:X2}' -f $value))."
        }
    }
    $text = $Utf8.GetString($RawBytes)
    $canonicalLf = ConvertTo-CanonicalLf -Text $text
    $ideCrlf = ConvertTo-IdeCrlf -Text $canonicalLf
    if (($text -cne $canonicalLf) -and ($text -cne $ideCrlf)) {
        throw "$Owner uses mixed or unsupported line endings."
    }
    return $text
}

function Assert-CanonicalTargetRatchet {
    param(
        [Parameter(Mandatory = $true)][byte[]]$RawBytes,
        [Parameter(Mandatory = $true)][string]$Owner,
        [switch]$RequirePreSplit
    )

    $null = ConvertFrom-StrictAsciiUtf8Bytes `
        -RawBytes $RawBytes `
        -Owner $Owner
    $rawSha256 = Get-BytesSha256 -Bytes $RawBytes
    $approvedState = switch ("$($RawBytes.Count)/$rawSha256") {
        "$ExpectedSourceCanonicalLfBytes/$ExpectedSourceCanonicalLfSha256" {
            [pscustomobject]@{
                State = 'PostIdePreSplit'
                LineEnding = 'LF'
                CanonicalLfSha256 = $ExpectedSourceCanonicalLfSha256
                IdeCrlfSha256 = $ExpectedSourceIdeCrlfSha256
            }
            break
        }
        "$ExpectedSourceIdeCrlfBytes/$ExpectedSourceIdeCrlfSha256" {
            [pscustomobject]@{
                State = 'PostIdePreSplit'
                LineEnding = 'CRLF'
                CanonicalLfSha256 = $ExpectedSourceCanonicalLfSha256
                IdeCrlfSha256 = $ExpectedSourceIdeCrlfSha256
            }
            break
        }
        "$ExpectedCandidateCanonicalLfBytes/$ExpectedCandidateCanonicalLfSha256" {
            [pscustomobject]@{
                State = 'PostSplit'
                LineEnding = 'LF'
                CanonicalLfSha256 = $ExpectedCandidateCanonicalLfSha256
                IdeCrlfSha256 = $ExpectedCandidateIdeCrlfSha256
            }
            break
        }
        "$ExpectedCandidateIdeCrlfBytes/$ExpectedCandidateIdeCrlfSha256" {
            [pscustomobject]@{
                State = 'PostSplit'
                LineEnding = 'CRLF'
                CanonicalLfSha256 = $ExpectedCandidateCanonicalLfSha256
                IdeCrlfSha256 = $ExpectedCandidateIdeCrlfSha256
            }
            break
        }
        default { $null }
    }
    if ($null -eq $approvedState) {
        throw (
            "$Owner is neither exact post-IDE pre-split nor exact post-split " +
            "state ($($RawBytes.Count)/$rawSha256).")
    }
    if ($RequirePreSplit -and
        ($approvedState.State -cne 'PostIdePreSplit')) {
        throw "$Owner apply preflight requires exact PostIdePreSplit target state."
    }
    return [pscustomobject]@{
        State = $approvedState.State
        LineEnding = $approvedState.LineEnding
        PhysicalBytes = $RawBytes.Count
        PhysicalSha256 = $rawSha256
        CanonicalLfSha256 = $approvedState.CanonicalLfSha256
        IdeCrlfSha256 = $approvedState.IdeCrlfSha256
    }
}

function Assert-LfPreSplitApplyTarget {
    param(
        [Parameter(Mandatory = $true)][object]$TargetState,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    if (($TargetState.State -cne 'PostIdePreSplit') -or
        ($TargetState.LineEnding -cne 'LF') -or
        ($TargetState.PhysicalBytes -ne $ExpectedSourceCanonicalLfBytes) -or
        ($TargetState.PhysicalSha256 -cne
            $ExpectedSourceCanonicalLfSha256)) {
        throw "$Owner requires exact LF/F923 pre-split apply target."
    }
}

function Get-ByteDimensions {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    $lfText = ConvertTo-CanonicalLf -Text $Text
    $crlfText = ConvertTo-IdeCrlf -Text $lfText
    return [ordered]@{
        raw = $Utf8.GetByteCount($Text)
        lf = $Utf8.GetByteCount($lfText)
        crlf = $Utf8.GetByteCount($crlfText)
    }
}

function Get-MethodContent {
    param([Parameter(Mandatory = $true)][string]$Method)

    $lfMethod = ConvertTo-CanonicalLf -Text $Method
    if (-not $lfMethod.EndsWith("`n", [StringComparison]::Ordinal)) {
        throw 'Method block does not end with one canonical LF.'
    }
    return $lfMethod.Substring(0, $lfMethod.Length - 1)
}

function Replace-ExactOne {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$Old,
        [AllowEmptyString()][string]$New,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    $first = $Text.IndexOf($Old, [StringComparison]::Ordinal)
    $last = $Text.LastIndexOf($Old, [StringComparison]::Ordinal)
    if (($first -lt 0) -or ($first -ne $last)) {
        throw "$Owner exact replacement count is not one."
    }
    return $Text.Substring(0, $first) + $New +
        $Text.Substring($first + $Old.Length)
}

function Assert-SourceRatchet {
    param(
        [Parameter(Mandatory = $true)][string]$InputText,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    $canonicalLf = ConvertTo-CanonicalLf -Text $InputText
    $ideCrlf = ConvertTo-IdeCrlf -Text $canonicalLf
    $lineEnding = if ($InputText -ceq $canonicalLf) {
        'LF'
    }
    elseif ($InputText -ceq $ideCrlf) {
        'CRLF'
    }
    else {
        throw "$Owner uses mixed or unsupported line endings."
    }
    $canonicalSha = Get-TextSha256 -Text $canonicalLf
    $ideSha = Get-TextSha256 -Text $ideCrlf
    if (($canonicalSha -cne $ExpectedSourceCanonicalLfSha256) -or
        ($ideSha -cne $ExpectedSourceIdeCrlfSha256) -or
        ($Utf8.GetByteCount($canonicalLf) -ne
            $ExpectedSourceCanonicalLfBytes) -or
        ($Utf8.GetByteCount($ideCrlf) -ne
            $ExpectedSourceIdeCrlfBytes)) {
        throw (
            "$Owner post-IDE F923/C636 source ratchet drifted " +
            "($canonicalSha/$ideSha).")
    }
    return [pscustomobject]@{
        CanonicalLf = $canonicalLf
        IdeCrlf = $ideCrlf
        LineEnding = $lineEnding
        PhysicalSha256 = Get-TextSha256 -Text $InputText
        PhysicalBytes = $Utf8.GetByteCount($InputText)
    }
}

function Assert-PreSplitSnapshotRatchet {
    param(
        [Parameter(Mandatory = $true)][byte[]]$RawBytes,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    $inputText = ConvertFrom-StrictAsciiUtf8Bytes `
        -RawBytes $RawBytes `
        -Owner $Owner
    $rawState = Assert-CanonicalTargetRatchet `
        -RawBytes $RawBytes `
        -Owner $Owner `
        -RequirePreSplit
    $textState = Assert-SourceRatchet `
        -InputText $inputText `
        -Owner $Owner
    if (($textState.LineEnding -cne $rawState.LineEnding) -or
        ($textState.PhysicalBytes -ne $rawState.PhysicalBytes) -or
        ($textState.PhysicalSha256 -cne $rawState.PhysicalSha256)) {
        throw "$Owner raw-byte and decoded-text checkpoints diverged."
    }
    return $textState
}

function ConvertTo-ApplyPatchPrefixedLines {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][ValidateSet('-', '+')][string]$Prefix,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    if ((ConvertTo-CanonicalLf -Text $Text) -cne $Text) {
        throw "$Owner apply_patch fragment is not canonical LF."
    }
    if (-not $Text.EndsWith("`n", [StringComparison]::Ordinal)) {
        throw "$Owner apply_patch fragment has no terminal LF."
    }
    $body = $Text.Substring(0, $Text.Length - 1)
    $lines = $body.Split([char]10, [StringSplitOptions]::None)
    return [string]::Join("`n", @(
            foreach ($line in $lines) {
                $Prefix + $line
            })) + "`n"
}

function New-ApplyPatchDocument {
    param(
        [Parameter(Mandatory = $true)][object[]]$Replacements,
        [Parameter(Mandatory = $true)][string]$RelativeTargetPath
    )

    if ($Replacements.Count -lt 1) {
        throw 'Publish apply_patch document requires at least one replacement.'
    }
    if (($RelativeTargetPath -match '\\') -or
        [IO.Path]::IsPathRooted($RelativeTargetPath) -or
        ($RelativeTargetPath -match '(^|/)\.\.(/|$)')) {
        throw 'Publish apply_patch target must be one safe repository-relative path.'
    }
    $builder = [Text.StringBuilder]::new()
    $null = $builder.Append("*** Begin Patch`n")
    $null = $builder.Append("*** Update File: $RelativeTargetPath`n")
    foreach ($replacement in $Replacements) {
        $null = $builder.Append("@@`n")
        $null = $builder.Append((ConvertTo-ApplyPatchPrefixedLines `
            -Text ([string]$replacement.Old) `
            -Prefix '-' `
            -Owner ([string]$replacement.Name)))
        $null = $builder.Append((ConvertTo-ApplyPatchPrefixedLines `
            -Text ([string]$replacement.New) `
            -Prefix '+' `
            -Owner ([string]$replacement.Name)))
    }
    $null = $builder.Append("*** End Patch`n")
    return $builder.ToString()
}

function Get-CanonicalLfLineArray {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    if ((ConvertTo-CanonicalLf -Text $Text) -cne $Text) {
        throw "$Owner state is not canonical LF."
    }
    if (-not $Text.EndsWith("`n", [StringComparison]::Ordinal)) {
        throw "$Owner state has no terminal LF."
    }
    return @($Text.Substring(0, $Text.Length - 1).Split(
            [char]10,
            [StringSplitOptions]::None))
}

function New-ApplyPatchOpcode {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('equal', 'delete', 'insert', 'replace')]
        [string]$Tag,
        [Parameter(Mandatory = $true)][int]$I1,
        [Parameter(Mandatory = $true)][int]$I2,
        [Parameter(Mandatory = $true)][int]$J1,
        [Parameter(Mandatory = $true)][int]$J2
    )

    return [pscustomobject]@{
        Tag = $Tag
        I1 = $I1
        I2 = $I2
        J1 = $J1
        J2 = $J2
    }
}

function New-GroupedOpcodeApplyPatchDocument {
    param(
        [Parameter(Mandatory = $true)][string]$InputText,
        [Parameter(Mandatory = $true)][string]$OutputText,
        [Parameter(Mandatory = $true)][object[]]$Groups,
        [Parameter(Mandatory = $true)][string]$RelativeTargetPath,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    if (($RelativeTargetPath -match '\\') -or
        [IO.Path]::IsPathRooted($RelativeTargetPath) -or
        ($RelativeTargetPath -match '(^|/)\.\.(/|$)')) {
        throw "$Owner target must be one safe repository-relative path."
    }
    if ($Groups.Count -lt 1) {
        throw "$Owner has no grouped opcodes."
    }
    $inputLines = @(Get-CanonicalLfLineArray `
        -Text $InputText `
        -Owner "$Owner input")
    $outputLines = @(Get-CanonicalLfLineArray `
        -Text $OutputText `
        -Owner "$Owner output")
    $builder = [Text.StringBuilder]::new()
    $null = $builder.Append("*** Begin Patch`n")
    $null = $builder.Append("*** Update File: $RelativeTargetPath`n")
    $expected = [Collections.Generic.List[object]]::new()
    $previousInputEnd = -1
    foreach ($group in $Groups) {
        $ops = @($group.Ops)
        if ($ops.Count -lt 1) {
            throw "$Owner group '$($group.Name)' has no opcodes."
        }
        $null = $builder.Append("@@`n")
        $oldBuilder = [Text.StringBuilder]::new()
        $newBuilder = [Text.StringBuilder]::new()
        $groupInputEnd = $null
        $groupOutputEnd = $null
        $changeCount = 0
        foreach ($op in $ops) {
            $tag = [string]$op.Tag
            $i1 = [int]$op.I1
            $i2 = [int]$op.I2
            $j1 = [int]$op.J1
            $j2 = [int]$op.J2
            if (($i1 -lt 0) -or ($i2 -lt $i1) -or
                ($i2 -gt $inputLines.Count) -or
                ($j1 -lt 0) -or ($j2 -lt $j1) -or
                ($j2 -gt $outputLines.Count)) {
                throw "$Owner group '$($group.Name)' has an out-of-range opcode."
            }
            if (($null -ne $groupInputEnd) -and
                (($i1 -ne $groupInputEnd) -or ($j1 -ne $groupOutputEnd))) {
                throw "$Owner group '$($group.Name)' opcodes are not contiguous."
            }
            $groupInputEnd = $i2
            $groupOutputEnd = $j2
            if (($tag -ceq 'equal') -or
                ($tag -ceq 'delete') -or
                ($tag -ceq 'replace')) {
                for ($lineIndex = $i1; $lineIndex -lt $i2; $lineIndex++) {
                    $prefix = if ($tag -ceq 'equal') { ' ' } else { '-' }
                    $line = $inputLines[$lineIndex]
                    $null = $builder.Append($prefix + $line + "`n")
                    $null = $oldBuilder.Append($line + "`n")
                }
            }
            if (($tag -ceq 'equal') -or
                ($tag -ceq 'insert') -or
                ($tag -ceq 'replace')) {
                for ($lineIndex = $j1; $lineIndex -lt $j2; $lineIndex++) {
                    $line = $outputLines[$lineIndex]
                    if ($tag -ceq 'equal') {
                        $inputOffset = $i1 + ($lineIndex - $j1)
                        if (($i2 - $i1) -ne ($j2 - $j1) -or
                            ($inputLines[$inputOffset] -cne $line)) {
                            throw "$Owner group '$($group.Name)' equal opcode drifted."
                        }
                    }
                    else {
                        $null = $builder.Append('+' + $line + "`n")
                    }
                    $null = $newBuilder.Append($line + "`n")
                }
            }
            if ($tag -cne 'equal') {
                if ($tag -notin @('delete', 'insert', 'replace')) {
                    throw "$Owner group '$($group.Name)' has unsupported tag '$tag'."
                }
                $changeCount++
            }
        }
        $firstOp = $ops[0]
        $lastOp = $ops[-1]
        if (([int]$firstOp.I1 -lt $previousInputEnd) -or
            ($oldBuilder.Length -eq 0) -or
            ($newBuilder.Length -eq 0) -or
            ($changeCount -eq 0)) {
            throw "$Owner group '$($group.Name)' is not ordered bounded context."
        }
        $previousInputEnd = [int]$lastOp.I2
        $expected.Add([pscustomobject]@{
                Name = [string]$group.Name
                Old = $oldBuilder.ToString()
                New = $newBuilder.ToString()
            })
    }
    $null = $builder.Append("*** End Patch`n")
    return [pscustomobject]@{
        Text = $builder.ToString()
        ExpectedReplacements = $expected.ToArray()
        HunkCount = $expected.Count
        InputLines = $inputLines.Count
        OutputLines = $outputLines.Count
    }
}

function Read-ApplyPatchReplacementHunks {
    param(
        [Parameter(Mandatory = $true)][string]$PatchText,
        [Parameter(Mandatory = $true)][string]$RelativeTargetPath,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    if ($PatchText.Contains("`r")) {
        throw "$Owner apply_patch document must use LF only."
    }
    $prefix = "*** Begin Patch`n*** Update File: $RelativeTargetPath`n"
    $suffix = "*** End Patch`n"
    if ((-not $PatchText.StartsWith($prefix, [StringComparison]::Ordinal)) -or
        (-not $PatchText.EndsWith($suffix, [StringComparison]::Ordinal))) {
        throw "$Owner apply_patch envelope or target path drifted."
    }
    $body = $PatchText.Substring(
        $prefix.Length,
        $PatchText.Length - $prefix.Length - $suffix.Length)
    $segments = [regex]::Split($body, '(?m)^@@\n')
    if (($segments.Count -lt 2) -or ($segments[0] -cne '')) {
        throw "$Owner apply_patch document has no bounded hunks."
    }
    $hunks = [Collections.Generic.List[object]]::new()
    for ($index = 1; $index -lt $segments.Count; $index++) {
        $segment = $segments[$index]
        $matches = [regex]::Matches(
            $segment,
            '(?m)^(?<Prefix>[ +\-])(?<Content>[^\n]*)\n')
        if (($matches.Count -eq 0) -or
            ([string]::Concat(@($matches | ForEach-Object Value)) -cne
                $segment)) {
            throw "$Owner apply_patch hunk $index has unsupported context."
        }
        $oldBuilder = [Text.StringBuilder]::new()
        $newBuilder = [Text.StringBuilder]::new()
        $changeLineCount = 0
        foreach ($match in $matches) {
            $line = $match.Groups['Content'].Value + "`n"
            $linePrefix = $match.Groups['Prefix'].Value
            if (($linePrefix -ceq '-') -or ($linePrefix -ceq ' ')) {
                $null = $oldBuilder.Append($line)
            }
            if (($linePrefix -ceq '+') -or ($linePrefix -ceq ' ')) {
                $null = $newBuilder.Append($line)
            }
            if ($linePrefix -cne ' ') {
                $changeLineCount++
            }
        }
        if (($oldBuilder.Length -eq 0) -or
            ($newBuilder.Length -eq 0) -or
            ($changeLineCount -eq 0)) {
            throw "$Owner apply_patch hunk $index is not a bounded change."
        }
        $hunks.Add([pscustomobject]@{
                Old = $oldBuilder.ToString()
                New = $newBuilder.ToString()
                EncodedBytes = $Utf8.GetByteCount("@@`n" + $segment)
                OldLines = @($oldBuilder.ToString().Split(
                        [char]10,
                        [StringSplitOptions]::None)).Count - 1
                NewLines = @($newBuilder.ToString().Split(
                        [char]10,
                        [StringSplitOptions]::None)).Count - 1
            })
    }
    return $hunks.ToArray()
}

function Assert-ApplyPatchPlan {
    param(
        [Parameter(Mandatory = $true)][string]$PatchText,
        [Parameter(Mandatory = $true)][object[]]$ExpectedReplacements,
        [Parameter(Mandatory = $true)][string]$PreSplitSource,
        [Parameter(Mandatory = $true)][string]$PostSplitSource,
        [Parameter(Mandatory = $true)][string]$ProtectedDeclarations,
        [Parameter(Mandatory = $true)][string]$RelativeTargetPath,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    $hunks = @(Read-ApplyPatchReplacementHunks `
        -PatchText $PatchText `
        -RelativeTargetPath $RelativeTargetPath `
        -Owner $Owner)
    if (($hunks.Count -lt 1) -or
        ($hunks.Count -ne $ExpectedReplacements.Count)) {
        throw "$Owner replacement inventory count diverged."
    }
    for ($index = 0; $index -lt $hunks.Count; $index++) {
        if (($hunks[$index].Old -cne
                [string]$ExpectedReplacements[$index].Old) -or
            ($hunks[$index].New -cne
                [string]$ExpectedReplacements[$index].New)) {
            throw "$Owner hunk $($index + 1) does not match its exact replacement."
        }
        if (($hunks[$index].Old.Contains($ProtectedDeclarations)) -or
            ($hunks[$index].New.Contains($ProtectedDeclarations))) {
            throw "$Owner hunk $($index + 1) includes protected declarations."
        }
    }

    $forward = $PreSplitSource
    for ($index = 0; $index -lt $hunks.Count; $index++) {
        $forward = Replace-ExactOne `
            -Text $forward `
            -Old $hunks[$index].Old `
            -New $hunks[$index].New `
            -Owner "$Owner forward hunk $($index + 1)"
    }
    if ($forward -cne $PostSplitSource) {
        throw "$Owner forward hunks did not produce the exact expected state."
    }
    if (([regex]::Matches(
                $forward,
                [regex]::Escape($ProtectedDeclarations))).Count -ne 1) {
        throw "$Owner forward replacements changed protected declarations."
    }

    $reverse = $PostSplitSource
    for ($index = $hunks.Count - 1; $index -ge 0; $index--) {
        $reverse = Replace-ExactOne `
            -Text $reverse `
            -Old $hunks[$index].New `
            -New $hunks[$index].Old `
            -Owner "$Owner reverse hunk $($index + 1)"
    }
    if ($reverse -cne $PreSplitSource) {
        throw "$Owner reverse hunks did not restore the exact phase input."
    }
    return $hunks.Count
}

function Resolve-EvidenceEmitPath {
    param([Parameter(Mandatory = $true)][string]$RequestedPath)

    $candidatePath = if ([IO.Path]::IsPathRooted($RequestedPath)) {
        $RequestedPath
    }
    else {
        Join-Path $PSScriptRoot $RequestedPath
    }
    $fullPath = [IO.Path]::GetFullPath($candidatePath)
    $evidenceDirectory = [IO.Path]::GetFullPath($PSScriptRoot).TrimEnd(
        [IO.Path]::DirectorySeparatorChar,
        [IO.Path]::AltDirectorySeparatorChar)
    $parentDirectory = [IO.Path]::GetFullPath(
        [IO.Path]::GetDirectoryName($fullPath)).TrimEnd(
        [IO.Path]::DirectorySeparatorChar,
        [IO.Path]::AltDirectorySeparatorChar)
    if (-not $parentDirectory.Equals(
            $evidenceDirectory,
            [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Publish apply_patch output must be directly inside the exact evidence directory.'
    }
    if ([IO.Path]::GetExtension($fullPath) -cne '.patch') {
        throw 'Publish apply_patch output must use the .patch extension.'
    }
    return $fullPath
}

function Write-CreateNewUtf8File {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][byte[]]$Bytes
    )

    $stream = [IO.FileStream]::new(
        $Path,
        [IO.FileMode]::CreateNew,
        [IO.FileAccess]::Write,
        [IO.FileShare]::None)
    try {
        $stream.Write($Bytes, 0, $Bytes.Count)
        $stream.Flush($true)
    }
    finally {
        $stream.Dispose()
    }
}

function Get-NormalizedInventory {
    param([Parameter(Mandatory = $true)][object[]]$Matches)

    return [string]::Join('|', @(
            foreach ($match in $Matches) {
                [regex]::Replace(
                    $match.Value, '\s+', '').ToLowerInvariant()
            }))
}

function Get-PersistentMutationMatches {
    param([Parameter(Mandatory = $true)][string]$Text)

    $pattern = (
        '(?is)(?:' +
        '\b(?:Ownership[A-Za-z0-9_]*State|ZeroHomeState)\s*\[[^;]+?' +
        '\]\s*(?:\$[A-Za-z_][A-Za-z0-9_]*\s*)?' +
        '(?::|\+|-|\*|/|and|or|xor)\s*=\s*[^;]+;' +
        '|' +
        '\b_memset\s*\(\s*dest\s*:=\s*#' +
        '(?:Ownership[A-Za-z0-9_]*State|ZeroHomeState)\s*\[.*?\)\s*;' +
        '|' +
        '\b_memcpy\s*\(\s*ptr1\s*:=\s*#' +
        '(?:Ownership[A-Za-z0-9_]*State|ZeroHomeState)\s*\[.*?\)\s*;' +
        '|' +
        '\bUpdateAxisRebaseRequiredState\s*\(.*?\)\s*;' +
        ')')
    return @([regex]::Matches($Text, $pattern))
}

function Get-CallHistogram {
    param([Parameter(Mandatory = $true)][string]$Text)

    $histogram = [ordered]@{}
    $controlWords = @(
        'if', 'elsif', 'while', 'case', 'for', 'and', 'or', 'not')
    foreach ($match in [regex]::Matches(
            $Text,
            '(?i)(?<![A-Za-z0-9_])(?<Name>[A-Za-z_][A-Za-z0-9_]*)\s*\(')) {
        $name = $match.Groups['Name'].Value
        if ($controlWords -contains $name.ToLowerInvariant()) {
            continue
        }
        $key = $name.ToLowerInvariant()
        if (-not $histogram.Contains($key)) {
            $histogram[$key] = 0
        }
        $histogram[$key]++
    }
    return $histogram
}

function Get-HistogramInventory {
    param([Parameter(Mandatory = $true)][Collections.IDictionary]$Histogram)

    return [string]::Join('|', @(
            foreach ($key in @($Histogram.Keys | Sort-Object)) {
                "$key=$($Histogram[$key])"
            }))
}

$root = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$emitLogicalProofRequested =
    -not [string]::IsNullOrWhiteSpace($EmitApplyPatchPath)
$emitCompactSetRequested =
    -not [string]::IsNullOrWhiteSpace($EmitCompactApplyPatchPrefix)
if ($RunSelfTest -and
    ($emitLogicalProofRequested -or $emitCompactSetRequested)) {
    throw 'Publish self-test is no-write and cannot emit an apply_patch artifact.'
}
if ($emitLogicalProofRequested -and $emitCompactSetRequested) {
    throw 'Publish logical-proof and compact patch emission are mutually exclusive.'
}
if (($emitLogicalProofRequested -or $emitCompactSetRequested) -and
    (-not $RequirePreSplitTarget)) {
    throw 'Publish apply_patch emission requires -RequirePreSplitTarget.'
}
if ($emitCompactSetRequested -and (-not $RequireLfPreSplitTarget)) {
    throw (
        'Publish compact apply_patch emission requires ' +
        '-RequireLfPreSplitTarget.')
}
$sourcePath = Join-Path $root (
    $CanonicalTargetRelativePath.Replace('/', [IO.Path]::DirectorySeparatorChar))
$canonicalTargetBytes = [IO.File]::ReadAllBytes($sourcePath)
$canonicalTargetState = Assert-CanonicalTargetRatchet `
    -RawBytes $canonicalTargetBytes `
    -Owner 'Publish split canonical target' `
    -RequirePreSplit:$RequirePreSplitTarget
if ($RequireLfPreSplitTarget) {
    Assert-LfPreSplitApplyTarget `
        -TargetState $canonicalTargetState `
        -Owner 'Publish compact apply preflight'
}
$snapshotInputPath = if ([string]::IsNullOrWhiteSpace($SnapshotPath)) {
    Join-Path $PSScriptRoot 'post_ide_pre_split_LMCControlCommandService.st'
}
else {
    $SnapshotPath
}
$snapshotInputPath = (Resolve-Path -LiteralPath $snapshotInputPath).Path
$snapshotInputBytes = [IO.File]::ReadAllBytes($snapshotInputPath)
$sourceCheckpoint = Assert-PreSplitSnapshotRatchet `
    -RawBytes $snapshotInputBytes `
    -Owner 'Publish split post-IDE snapshot input'
$source = $sourceCheckpoint.CanonicalLf

$methodPattern = (
    '(?ms)^FUNCTION GLOBAL ' +
    'LMCControlCommandService::PublishAxisOwnership\n' +
    '.*?^END_FUNCTION\n')
$methodMatches = [regex]::Matches($source, $methodPattern)
if ($methodMatches.Count -ne 1) {
    throw "PublishAxisOwnership method count is $($methodMatches.Count), expected one."
}
$method = $methodMatches[0].Value
$methodSha256 = Get-TextSha256 -Text $method
if ($methodSha256 -cne $ExpectedMethodCanonicalLfSha256) {
    throw "PublishAxisOwnership canonical LF method drifted ($methodSha256)."
}

$publicInterface = [string]::Join("`n", @(
        'FUNCTION GLOBAL LMCControlCommandService::PublishAxisOwnership',
        "`tVAR_INPUT",
        "`t`tAxisMask : UDINT;",
        "`t`tAdmissionToken : UDINT;",
        "`t`tOwnerGeneration : UDINT;",
        "`t`tReportKind : UINT;",
        "`t`tReportValue0 : UDINT;",
        "`t`tReportValue1 : UDINT;",
        "`t`tObservationCycle : UDINT;",
        "`tEND_VAR",
        "`tVAR_OUTPUT",
        "`t`tResult : DINT;",
        "`tEND_VAR",
        "`tVAR"
    )) + "`n"
if (-not $method.StartsWith($publicInterface, [StringComparison]::Ordinal)) {
    throw 'PublishAxisOwnership public implementation ABI/order drifted.'
}

$localVarMatches = [regex]::Matches(
    $method, '(?ms)^\tVAR\n.*?^\tEND_VAR\n')
if ($localVarMatches.Count -ne 1) {
    throw 'PublishAxisOwnership local VAR block count is not one.'
}
$originalLocalVarBlock = $localVarMatches[0].Value
$localDeclarationMatches = [regex]::Matches(
    $originalLocalVarBlock,
    '(?m)^\t\t(?<Name>[A-Za-z_][A-Za-z0-9_]*)\s*:(?!=)[^;\n]+;\n')
$localDeclarationByName = @{}
foreach ($match in $localDeclarationMatches) {
    $name = $match.Groups['Name'].Value
    if ($localDeclarationByName.ContainsKey($name)) {
        throw "Publish local '$name' is duplicated."
    }
    $localDeclarationByName[$name] = $match.Value
}
if ($localDeclarationByName.Count -ne 93) {
    throw "Publish local declaration count is $($localDeclarationByName.Count), expected 93."
}

$homeExtractionPattern = (
    '(?ms)(^\tif ZeroHomeState\[60\]\$UDINT = ' +
    'LMC_HOME_OWNER_RECEIPT_MAGIC then\n.*?^\tend_if;\n)' +
    '(?=\texpectedResourceKind := 0;)')
$homeExtractionMatches = [regex]::Matches($method, $homeExtractionPattern)
if ($homeExtractionMatches.Count -ne 1) {
    throw 'Publish Home extraction count is not one.'
}
$homeExtraction = $homeExtractionMatches[0].Groups[1].Value
$homeExtractionContent = Get-MethodContent -Method $homeExtraction
$homeExtractionLfSha256 = Get-TextSha256 -Text $homeExtractionContent
$homeExtractionCrlfSha256 = Get-TextSha256 -Text (
    ConvertTo-IdeCrlf -Text $homeExtractionContent)
if (($homeExtractionLfSha256 -cne
        $ExpectedHomeExtractionCanonicalLfSha256) -or
    ($homeExtractionCrlfSha256 -cne
        $ExpectedHomeExtractionIdeCrlfSha256)) {
    throw 'Publish Home extraction LF/CRLF ratchet drifted.'
}

$decisionExtractionPattern = (
    '(?ms)(^\tpreemptRootValid :=\n.*?^\tend_if;\n)' +
    '(?=\tdestroyBanks :=)')
$decisionExtractionMatches = [regex]::Matches(
    $method, $decisionExtractionPattern)
if ($decisionExtractionMatches.Count -ne 1) {
    throw 'Publish decision extraction count is not one.'
}
$decisionExtraction = $decisionExtractionMatches[0].Groups[1].Value
$decisionExtractionContent = Get-MethodContent -Method $decisionExtraction
$decisionExtractionLfSha256 = Get-TextSha256 -Text $decisionExtractionContent
$decisionExtractionCrlfSha256 = Get-TextSha256 -Text (
    ConvertTo-IdeCrlf -Text $decisionExtractionContent)
if (($decisionExtractionLfSha256 -cne
        $ExpectedDecisionExtractionCanonicalLfSha256) -or
    ($decisionExtractionCrlfSha256 -cne
        $ExpectedDecisionExtractionIdeCrlfSha256)) {
    throw 'Publish decision extraction LF/CRLF ratchet drifted.'
}

$homeExtractionOnlyLocals = @(
    'homeReceiptExact',
    'homeReceiptCallMatches',
    'homeReceiptTerminalComplete',
    'homeReceiptIdentityPre',
    'homeReceiptIdentityPost',
    'homeReceiptObserverPre',
    'homeReceiptObserverPost',
    'homeReceiptRecordPre',
    'homeReceiptRecordBodyPost',
    'homeReceiptRecordPost',
    'homeReceiptSingletonPre',
    'homeReceiptSingletonPost',
    'homeReceiptSurfaceValid',
    'homeReceiptPhase',
    'homeReceiptIndex',
    'rebaseUpdateResult'
)
$decisionExtractionOnlyLocals = @(
    'leaseRecordBase',
    'leaseFirstRecordBase',
    'preemptRecordBase',
    'probeRecordBase',
    'probeAxisIndex',
    'probeAxisBit',
    'preemptFlags',
    'cleanupRequiredMask',
    'cleanupCompleteMask',
    'preemptToken',
    'preemptGeneration',
    'preemptSession',
    'preemptSequence',
    'preemptMask',
    'preemptIdentitySize',
    'preemptState',
    'preemptOwnerKind',
    'preemptResourceKind',
    'preemptCommand',
    'preemptReference',
    'preemptAdmission',
    'replacementRecordBase',
    'replacementHeaderBase',
    'singletonToken',
    'singletonGeneration',
    'singletonMask',
    'replacementIdentitySize',
    'replacementTailSize',
    'replacementTailOffset',
    'replacementPackedCommand',
    'replacementPackedOwner',
    'cleanupReady',
    'leaseValid',
    'preemptBankValid',
    'preemptSpecialValid',
    'replacementFound',
    'replacementValid',
    'replacementStateValid',
    'preemptLmcFound',
    'preemptDs402Found',
    'preemptEncoderFound'
)
$homeHelperLocals = @(
    'axisIndex',
    'recordBase',
    'observerBase',
    'identityHeaderBase',
    'identitySize',
    'identityTailSize',
    'identityTailOffset',
    'identityPackedCommand',
    'identityPackedOwner',
    'homeReceiptAxisMaskValid'
) + $homeExtractionOnlyLocals
$decisionHelperLocals = @(
    'axisIndex',
    'axisBit',
    'observerBase',
    'identityIndex',
    'identityCompareResult',
    'forceQuarantine',
    'preemptRootValid',
    'restoreLease'
) + $decisionExtractionOnlyLocals

$methodBodyWithoutLocals = Replace-ExactOne `
    -Text $method `
    -Old $originalLocalVarBlock `
    -New '' `
    -Owner 'Publish local-only analysis'
$outsideExtractions = Replace-ExactOne `
    -Text $methodBodyWithoutLocals `
    -Old $homeExtraction `
    -New '' `
    -Owner 'Publish Home local-only analysis'
$outsideExtractions = Replace-ExactOne `
    -Text $outsideExtractions `
    -Old $decisionExtraction `
    -New '' `
    -Owner 'Publish decision local-only analysis'
foreach ($name in ($homeExtractionOnlyLocals + $decisionExtractionOnlyLocals)) {
    if (-not $localDeclarationByName.ContainsKey($name)) {
        throw "Extraction-only local '$name' is not declared."
    }
    $tokenPattern = '(?i)(?<![A-Za-z0-9_])' +
        [regex]::Escape($name) + '(?![A-Za-z0-9_])'
    if ([regex]::IsMatch($outsideExtractions, $tokenPattern)) {
        throw "Extraction-only local '$name' leaks outside both extraction blocks."
    }
}
foreach ($name in $homeHelperLocals) {
    if (-not $localDeclarationByName.ContainsKey($name)) {
        throw "Home helper local '$name' is not declared in the monolith."
    }
}
foreach ($name in $decisionHelperLocals) {
    if (-not $localDeclarationByName.ContainsKey($name)) {
        throw "Decision helper local '$name' is not declared in the monolith."
    }
}

$adapter = $method
foreach ($name in ($homeExtractionOnlyLocals + $decisionExtractionOnlyLocals)) {
    $adapter = Replace-ExactOne `
        -Text $adapter `
        -Old $localDeclarationByName[$name] `
        -New '' `
        -Owner "Publish adapter moved local $name"
}
$adapterLocalAnchor = "`t`tgroupHeaderPublished : BOOL;`n"
$adapterAddedLocals = [string]::Join("`n", @(
        "`t`thomeReceiptResult : DINT;",
        "`t`tpublishDecisionResult : DINT;"
    )) + "`n"
$adapter = Replace-ExactOne `
    -Text $adapter `
    -Old $adapterLocalAnchor `
    -New ($adapterLocalAnchor + $adapterAddedLocals) `
    -Owner 'Publish adapter result locals'

$homeCallMap = [string]::Join("`n", @(
        "`thomeReceiptResult := HandleAxisOwnershipPublishHomeReceipt(",
        "`t`tAxisMask:=AxisMask, AdmissionToken:=AdmissionToken,",
        "`t`tOwnerGeneration:=OwnerGeneration, ReportKind:=ReportKind,",
        "`t`tReportValue0:=ReportValue0, ReportValue1:=ReportValue1,",
        "`t`tObservationCycle:=ObservationCycle);",
        "`tif homeReceiptResult <> 2 then",
        "`t`tResult := homeReceiptResult;",
        "`t`tRETURN;",
        "`tend_if;"
    )) + "`n"
$adapter = Replace-ExactOne `
    -Text $adapter `
    -Old $homeExtraction `
    -New $homeCallMap `
    -Owner 'Publish adapter Home call/fence'

$decisionCallMap = [string]::Join("`n", @(
        "`tpublishDecisionResult := PrepareAxisOwnershipPublishDecision(",
        "`t`tAxisMask:=AxisMask, AdmissionToken:=AdmissionToken,",
        "`t`tOwnerGeneration:=OwnerGeneration, ReportKind:=ReportKind,",
        "`t`tExpectedSession:=expectedSession,",
        "`t`tExpectedSequence:=expectedSequence,",
        "`t`tExpectedCommandId:=expectedCommandId,",
        "`t`tExpectedReference:=expectedReference,",
        "`t`tExpectedAdmissionMode:=expectedAdmissionMode,",
        "`t`tExpectedOwnerKind:=expectedOwnerKind);",
        "`tif publishDecisionResult < 0 then",
        "`t`tResult := publishDecisionResult;",
        "`t`tRETURN;",
        "`telsif publishDecisionResult > 7 then",
        "`t`tResult := -3;",
        "`t`tRETURN;",
        "`tend_if;",
        "`tpreemptRootValid := (publishDecisionResult and 1) <> 0;",
        "`tforceQuarantine := (publishDecisionResult and 2) <> 0;",
        "`trestoreLease := (publishDecisionResult and 4) <> 0;"
    )) + "`n"
$adapter = Replace-ExactOne `
    -Text $adapter `
    -Old $decisionExtraction `
    -New $decisionCallMap `
    -Owner 'Publish adapter decision call/map'
$adapterLocalVarMatches = [regex]::Matches(
    $adapter, '(?ms)^\tVAR\n.*?^\tEND_VAR\n')
if ($adapterLocalVarMatches.Count -ne 1) {
    throw 'Publish planned adapter local VAR block count is not one.'
}
$adapterLocalVarBlock = $adapterLocalVarMatches[0].Value

$homeClassDeclarationMatches = [regex]::Matches(
    $source,
    ('(?ms)^\tFUNCTION HandleAxisOwnershipPublishHomeReceipt\n' +
     '.*?^\t\tEND_VAR;\n'))
$decisionClassDeclarationMatches = [regex]::Matches(
    $source,
    ('(?ms)^\tFUNCTION PrepareAxisOwnershipPublishDecision\n' +
     '.*?^\t\tEND_VAR;\n'))
if (($homeClassDeclarationMatches.Count -ne 1) -or
    ($decisionClassDeclarationMatches.Count -ne 1)) {
    throw 'Publish actual IDE private declaration inventory is not 1/1.'
}
$homeClassDeclaration = $homeClassDeclarationMatches[0].Value
$decisionClassDeclaration = $decisionClassDeclarationMatches[0].Value
if (($Utf8.GetByteCount($homeClassDeclaration) -ne 297) -or
    ((Get-TextSha256 -Text $homeClassDeclaration) -cne
        $ExpectedHomeDeclarationCanonicalLfSha256) -or
    ($Utf8.GetByteCount($decisionClassDeclaration) -ne 396) -or
    ((Get-TextSha256 -Text $decisionClassDeclaration) -cne
        $ExpectedDecisionDeclarationCanonicalLfSha256)) {
    throw 'Publish actual IDE private declaration byte/hash ratchet drifted.'
}
$privateDeclarationSeparator = "`t`n"
$privateClassDeclarations = $homeClassDeclaration +
    $privateDeclarationSeparator + $decisionClassDeclaration
if (($privateClassDeclarations -match '(?i)\b(?:GLOBAL|VIRTUAL)\b') -or
    ([regex]::Matches(
        $source,
        [regex]::Escape($privateClassDeclarations)).Count -ne 1)) {
    throw 'Publish actual IDE private declarations are not exact and private.'
}
$classTableAnchor = "  //Tables:`n"
$classTableIndex = $source.IndexOf(
    $classTableAnchor,
    [StringComparison]::Ordinal)
if (($classTableIndex -lt 0) -or
    (($decisionClassDeclarationMatches[0].Index +
        $decisionClassDeclarationMatches[0].Length) -ne $classTableIndex)) {
    throw 'Publish actual IDE declarations are not immediately before //Tables:.'
}

$homeEmptyStubMatches = [regex]::Matches(
    $source,
    ('(?ms)^FUNCTION LMCControlCommandService::' +
     'HandleAxisOwnershipPublishHomeReceipt\n.*?^END_FUNCTION\n'))
$decisionEmptyStubMatches = [regex]::Matches(
    $source,
    ('(?ms)^FUNCTION LMCControlCommandService::' +
     'PrepareAxisOwnershipPublishDecision\n.*?^END_FUNCTION\n'))
if (($homeEmptyStubMatches.Count -ne 1) -or
    ($decisionEmptyStubMatches.Count -ne 1)) {
    throw 'Publish actual IDE qualified empty-stub inventory is not 1/1.'
}
$homeEmptyStub = $homeEmptyStubMatches[0].Value
$decisionEmptyStub = $decisionEmptyStubMatches[0].Value
if (($Utf8.GetByteCount($homeEmptyStub) -ne 323) -or
    ((Get-TextSha256 -Text $homeEmptyStub) -cne
        $ExpectedHomeEmptyStubCanonicalLfSha256) -or
    ($Utf8.GetByteCount($decisionEmptyStub) -ne 419) -or
    ((Get-TextSha256 -Text $decisionEmptyStub) -cne
        $ExpectedDecisionEmptyStubCanonicalLfSha256)) {
    throw 'Publish actual IDE empty-stub byte/hash ratchet drifted.'
}
$emptyStubBodyAnchor = "`nEND_FUNCTION`n"
foreach ($emptyStub in @($homeEmptyStub, $decisionEmptyStub)) {
    if (([regex]::Matches(
            $emptyStub,
            [regex]::Escape($emptyStubBodyAnchor)).Count -ne 1) -or
        ($emptyStub -notmatch '(?s)\tEND_VAR\n\nEND_FUNCTION\n\z') -or
        ($emptyStub -match '(?i)^FUNCTION\s+(?:GLOBAL|VIRTUAL\s+GLOBAL)\s+')) {
        throw 'Publish actual IDE stub is not one exact private empty body.'
    }
}
$homeStubEnd = $homeEmptyStubMatches[0].Index +
    $homeEmptyStubMatches[0].Length
$decisionStubEnd = $decisionEmptyStubMatches[0].Index +
    $decisionEmptyStubMatches[0].Length
if (($homeStubEnd -ge $decisionEmptyStubMatches[0].Index) -or
    ($source.Substring(
            $homeStubEnd,
            $decisionEmptyStubMatches[0].Index - $homeStubEnd) -cne
        "`n`n") -or
    ($decisionStubEnd -ne $source.Length)) {
    throw 'Publish actual IDE Home/decision EOF stub order/separator drifted.'
}

$homeStubInterface = $homeEmptyStub.Substring(
    0,
    $homeEmptyStub.Length - $emptyStubBodyAnchor.Length)
$homeLocalBlock = "`tVAR`n"
foreach ($name in $homeHelperLocals) {
    $homeLocalBlock += $localDeclarationByName[$name]
}
$homeLocalBlock += "`tEND_VAR`n`n"
$homeHelperPrefix = $homeStubInterface + $homeLocalBlock +
    ([string]::Join("`n", @(
        "`tResult := 2;"
    )) + "`n")
$homeHelper = $homeHelperPrefix + $homeExtraction +
    "`nEND_FUNCTION`n"

$decisionStubInterface = $decisionEmptyStub.Substring(
    0,
    $decisionEmptyStub.Length - $emptyStubBodyAnchor.Length)
$decisionLocalBlock = "`tVAR`n"
foreach ($name in $decisionHelperLocals) {
    $decisionLocalBlock += $localDeclarationByName[$name]
}
$decisionLocalBlock += "`tEND_VAR`n`n"
$decisionHelperPrefix = $decisionStubInterface + $decisionLocalBlock
$decisionHelperSuffix = [string]::Join("`n", @(
        "`tResult := 0;",
        "`tif preemptRootValid then",
        "`t`tResult += 1;",
        "`tend_if;",
        "`tif forceQuarantine then",
        "`t`tResult += 2;",
        "`tend_if;",
        "`tif restoreLease then",
        "`t`tResult += 4;",
        "`tend_if;",
        '',
        'END_FUNCTION'
    )) + "`n"
$decisionHelper = $decisionHelperPrefix + $decisionExtraction +
    $decisionHelperSuffix

# The captured source already owns the CodeGenerator declarations and exact
# qualified empty-stub headers. Planning changes only the public implementation
# and the two empty implementation bodies; no declaration or EOF append is
# synthesized here.
$adapterStateSource = Replace-ExactOne `
    -Text $source `
    -Old $method `
    -New $adapter `
    -Owner 'Publish planned adapter'
$homeStateSource = Replace-ExactOne `
    -Text $adapterStateSource `
    -Old $homeEmptyStub `
    -New $homeHelper `
    -Owner 'Publish planned Home empty-stub body'
$plannedSource = Replace-ExactOne `
    -Text $homeStateSource `
    -Old $decisionEmptyStub `
    -New $decisionHelper `
    -Owner 'Publish planned decision empty-stub body'
$logicalProofReplacements = @(
    [pscustomobject]@{
        Name = 'PublishAxisOwnership monolith adapter'
        Old = $method
        New = $adapter
    },
    [pscustomobject]@{
        Name = 'HandleAxisOwnershipPublishHomeReceipt empty stub body'
        Old = $homeEmptyStub
        New = $homeHelper
    },
    [pscustomobject]@{
        Name = 'PrepareAxisOwnershipPublishDecision empty stub body'
        Old = $decisionEmptyStub
        New = $decisionHelper
    })
$logicalProofPatchDocument = New-ApplyPatchDocument `
    -Replacements $logicalProofReplacements `
    -RelativeTargetPath $CanonicalTargetRelativePath

$adapterStateLines = @(Get-CanonicalLfLineArray `
    -Text $adapterStateSource `
    -Owner 'Publish compact adapter state')
$homeStateLines = @(Get-CanonicalLfLineArray `
    -Text $homeStateSource `
    -Owner 'Publish compact Home state')
if (($Utf8.GetByteCount($adapterStateSource) -ne
        $ExpectedAdapterStateCanonicalLfBytes) -or
    ($adapterStateLines.Count -ne $ExpectedAdapterStateCanonicalLfLines) -or
    ((Get-TextSha256 -Text $adapterStateSource) -cne
        $ExpectedAdapterStateCanonicalLfSha256) -or
    ($Utf8.GetByteCount($homeStateSource) -ne
        $ExpectedHomeStateCanonicalLfBytes) -or
    ($homeStateLines.Count -ne $ExpectedHomeStateCanonicalLfLines) -or
    ((Get-TextSha256 -Text $homeStateSource) -cne
        $ExpectedHomeStateCanonicalLfSha256)) {
    throw 'Publish compact intermediate state byte/line/hash ratchet drifted.'
}

$compactAdapterGroups = @(
    [pscustomobject]@{
        Name = 'Adapter locals group 1'
        Ops = @(
            (New-ApplyPatchOpcode equal 5877 5880 5877 5880)
            (New-ApplyPatchOpcode delete 5880 5886 5880 5880)
            (New-ApplyPatchOpcode equal 5886 5889 5880 5883)
        )
    },
    [pscustomobject]@{
        Name = 'Adapter locals group 2'
        Ops = @(
            (New-ApplyPatchOpcode equal 5897 5900 5891 5894)
            (New-ApplyPatchOpcode delete 5900 5903 5894 5894)
            (New-ApplyPatchOpcode equal 5903 5909 5894 5900)
            (New-ApplyPatchOpcode delete 5909 5931 5900 5900)
            (New-ApplyPatchOpcode equal 5931 5937 5900 5906)
            (New-ApplyPatchOpcode delete 5937 5938 5906 5906)
            (New-ApplyPatchOpcode equal 5938 5939 5906 5907)
            (New-ApplyPatchOpcode delete 5939 5940 5907 5907)
            (New-ApplyPatchOpcode equal 5940 5941 5907 5908)
            (New-ApplyPatchOpcode delete 5941 5949 5908 5908)
            (New-ApplyPatchOpcode equal 5949 5952 5908 5911)
            (New-ApplyPatchOpcode replace 5952 5955 5911 5913)
            (New-ApplyPatchOpcode equal 5955 5956 5913 5914)
            (New-ApplyPatchOpcode delete 5956 5969 5914 5914)
            (New-ApplyPatchOpcode equal 5969 5972 5914 5917)
        )
    },
    [pscustomobject]@{
        Name = 'Adapter Home extraction group'
        Ops = @(
            (New-ApplyPatchOpcode equal 5975 5978 5920 5923)
            (New-ApplyPatchOpcode replace 5978 6294 5923 5930)
            (New-ApplyPatchOpcode equal 6294 6295 5930 5931)
            (New-ApplyPatchOpcode delete 6295 6300 5931 5931)
            (New-ApplyPatchOpcode equal 6300 6303 5931 5934)
        )
    },
    [pscustomobject]@{
        Name = 'Adapter decision extraction group'
        Ops = @(
            (New-ApplyPatchOpcode equal 6565 6568 6196 6199)
            (New-ApplyPatchOpcode replace 6568 6597 6199 6212)
            (New-ApplyPatchOpcode equal 6597 6600 6212 6215)
            (New-ApplyPatchOpcode replace 6600 7161 6215 6218)
            (New-ApplyPatchOpcode equal 7161 7164 6218 6221)
        )
    })
$compactHomeGroups = @(
    [pscustomobject]@{
        Name = 'Home empty-stub body insertion'
        Ops = @(
            (New-ApplyPatchOpcode equal 15866 15869 15866 15869)
            (New-ApplyPatchOpcode insert 15869 15869 15869 16222)
            (New-ApplyPatchOpcode equal 15869 15872 16222 16225)
        )
    })
$compactDecisionGroups = @(
    [pscustomobject]@{
        Name = 'Decision empty-stub body insertion'
        Ops = @(
            (New-ApplyPatchOpcode equal 16239 16242 16239 16242)
            (New-ApplyPatchOpcode insert 16242 16242 16242 16897)
            (New-ApplyPatchOpcode equal 16242 16244 16897 16899)
        )
    })

$compactAdapterPatch = New-GroupedOpcodeApplyPatchDocument `
    -InputText $source `
    -OutputText $adapterStateSource `
    -Groups $compactAdapterGroups `
    -RelativeTargetPath $CanonicalTargetRelativePath `
    -Owner 'Publish compact patch 1 adapter'
$compactHomePatch = New-GroupedOpcodeApplyPatchDocument `
    -InputText $adapterStateSource `
    -OutputText $homeStateSource `
    -Groups $compactHomeGroups `
    -RelativeTargetPath $CanonicalTargetRelativePath `
    -Owner 'Publish compact patch 2 Home'
$compactDecisionPatch = New-GroupedOpcodeApplyPatchDocument `
    -InputText $homeStateSource `
    -OutputText $plannedSource `
    -Groups $compactDecisionGroups `
    -RelativeTargetPath $CanonicalTargetRelativePath `
    -Owner 'Publish compact patch 3 decision'
$compactPatchPlans = @(
    [pscustomobject]@{
        Step = 1
        Name = 'adapter'
        Document = $compactAdapterPatch
        Input = $source
        Output = $adapterStateSource
        ExpectedBytes = $ExpectedCompactAdapterPatchBytes
        ExpectedSha256 = $ExpectedCompactAdapterPatchSha256
        ExpectedHunks = 4
        FileSuffix = '01_adapter.patch'
    },
    [pscustomobject]@{
        Step = 2
        Name = 'home'
        Document = $compactHomePatch
        Input = $adapterStateSource
        Output = $homeStateSource
        ExpectedBytes = $ExpectedCompactHomePatchBytes
        ExpectedSha256 = $ExpectedCompactHomePatchSha256
        ExpectedHunks = 1
        FileSuffix = '02_home.patch'
    },
    [pscustomobject]@{
        Step = 3
        Name = 'decision'
        Document = $compactDecisionPatch
        Input = $homeStateSource
        Output = $plannedSource
        ExpectedBytes = $ExpectedCompactDecisionPatchBytes
        ExpectedSha256 = $ExpectedCompactDecisionPatchSha256
        ExpectedHunks = 1
        FileSuffix = '03_decision.patch'
    })

function New-PlannedSourceFromFragments {
    param(
        [Parameter(Mandatory = $true)][string]$CandidateAdapter,
        [Parameter(Mandatory = $true)][string]$CandidateHomeHelper,
        [Parameter(Mandatory = $true)][string]$CandidateDecisionHelper,
        [Parameter(Mandatory = $true)][string]$CandidateDeclarations
    )

    $candidateSource = Replace-ExactOne `
        -Text $source `
        -Old $privateClassDeclarations `
        -New $CandidateDeclarations `
        -Owner 'Publish candidate existing private declarations'
    $candidateSource = Replace-ExactOne `
        -Text $candidateSource `
        -Old $method `
        -New $CandidateAdapter `
        -Owner 'Publish candidate adapter'
    $candidateSource = Replace-ExactOne `
        -Text $candidateSource `
        -Old $homeEmptyStub `
        -New $CandidateHomeHelper `
        -Owner 'Publish candidate Home empty-stub body'
    return Replace-ExactOne `
        -Text $candidateSource `
        -Old $decisionEmptyStub `
        -New $CandidateDecisionHelper `
        -Owner 'Publish candidate decision empty-stub body'
}

function Get-ReversedAdapter {
    param(
        [Parameter(Mandatory = $true)][string]$CandidateAdapter,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    $candidateLocalMatches = [regex]::Matches(
        $CandidateAdapter, '(?ms)^\tVAR\n.*?^\tEND_VAR\n')
    if ($candidateLocalMatches.Count -ne 1) {
        throw "$Owner adapter local VAR block count is not one."
    }
    $reversed = Replace-ExactOne `
        -Text $CandidateAdapter `
        -Old $candidateLocalMatches[0].Value `
        -New $originalLocalVarBlock `
        -Owner "$Owner reverse local movement"
    $reversed = Replace-ExactOne `
        -Text $reversed `
        -Old $homeCallMap `
        -New $homeExtraction `
        -Owner "$Owner reverse Home inline"
    return Replace-ExactOne `
        -Text $reversed `
        -Old $decisionCallMap `
        -New $decisionExtraction `
        -Owner "$Owner reverse decision inline"
}

function Assert-PublishSplitCandidate {
    param(
        [Parameter(Mandatory = $true)][string]$CandidateAdapter,
        [Parameter(Mandatory = $true)][string]$CandidateHomeHelper,
        [Parameter(Mandatory = $true)][string]$CandidateDecisionHelper,
        [Parameter(Mandatory = $true)][string]$CandidateDeclarations,
        [Parameter(Mandatory = $true)][string]$CandidateSource,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    if ($CandidateDeclarations -cne $privateClassDeclarations) {
        throw "$Owner private ABI/declaration order changed."
    }
    if (($CandidateDeclarations -match '(?i)\b(?:GLOBAL|VIRTUAL)\b') -or
        ($CandidateHomeHelper -match
            '(?i)^FUNCTION\s+(?:GLOBAL|VIRTUAL\s+GLOBAL)\s+') -or
        ($CandidateDecisionHelper -match
            '(?i)^FUNCTION\s+(?:GLOBAL|VIRTUAL\s+GLOBAL)\s+')) {
        throw "$Owner helper private visibility changed."
    }
    if (-not $CandidateAdapter.StartsWith(
            $publicInterface, [StringComparison]::Ordinal)) {
        throw "$Owner public ABI changed."
    }
    $candidateAdapterLocalMatches = [regex]::Matches(
        $CandidateAdapter, '(?ms)^\tVAR\n.*?^\tEND_VAR\n')
    if (($candidateAdapterLocalMatches.Count -ne 1) -or
        ($candidateAdapterLocalMatches[0].Value -cne $adapterLocalVarBlock)) {
        throw "$Owner extraction-only local movement changed."
    }
    if (-not $CandidateHomeHelper.StartsWith(
            $homeHelperPrefix, [StringComparison]::Ordinal)) {
        throw "$Owner Home helper ABI/local contract changed."
    }
    if (-not $CandidateDecisionHelper.StartsWith(
            $decisionHelperPrefix, [StringComparison]::Ordinal)) {
        throw "$Owner decision helper ABI/local contract changed."
    }

    $adapterDimensions = Get-ByteDimensions -Text (
        Get-MethodContent -Method $CandidateAdapter)
    $homeDimensions = Get-ByteDimensions -Text (
        Get-MethodContent -Method $CandidateHomeHelper)
    $decisionDimensions = Get-ByteDimensions -Text (
        Get-MethodContent -Method $CandidateDecisionHelper)
    foreach ($entry in @(
            [pscustomobject]@{ Name = 'adapter'; Value = $adapterDimensions },
            [pscustomobject]@{ Name = 'Home helper'; Value = $homeDimensions },
            [pscustomobject]@{ Name = 'decision helper'; Value = $decisionDimensions })) {
        if (($entry.Value.lf -ge $MethodSizeLimitBytes) -or
            ($entry.Value.crlf -ge $MethodSizeLimitBytes)) {
            throw "$Owner $($entry.Name) violates the method-size hard limit."
        }
    }

    if ([regex]::Matches(
            $CandidateAdapter,
            '(?i)(?<![A-Za-z0-9_])HandleAxisOwnershipPublishHomeReceipt\s*\(').Count -ne 1) {
        throw "$Owner Home helper call count/dominance changed."
    }
    if ([regex]::Matches(
            $CandidateAdapter,
            '(?i)(?<![A-Za-z0-9_])PrepareAxisOwnershipPublishDecision\s*\(').Count -ne 1) {
        throw "$Owner decision helper call count/dominance changed."
    }
    if (($CandidateAdapter.IndexOf(
                $homeCallMap, [StringComparison]::Ordinal) -lt 0) -or
        ($CandidateAdapter.IndexOf(
                $decisionCallMap, [StringComparison]::Ordinal) -lt 0)) {
        throw "$Owner exact adapter Result fence/bit map changed."
    }
    $homeBlockFirst = $CandidateHomeHelper.IndexOf(
        $homeExtraction, [StringComparison]::Ordinal)
    $homeBlockLast = $CandidateHomeHelper.LastIndexOf(
        $homeExtraction, [StringComparison]::Ordinal)
    if (($homeBlockFirst -lt 0) -or ($homeBlockFirst -ne $homeBlockLast) -or
        ((Get-TextSha256 -Text $homeExtractionContent) -cne
            $ExpectedHomeExtractionCanonicalLfSha256)) {
        throw "$Owner exact Home extraction changed."
    }
    $decisionBlockFirst = $CandidateDecisionHelper.IndexOf(
        $decisionExtraction, [StringComparison]::Ordinal)
    $decisionBlockLast = $CandidateDecisionHelper.LastIndexOf(
        $decisionExtraction, [StringComparison]::Ordinal)
    if (($decisionBlockFirst -lt 0) -or
        ($decisionBlockFirst -ne $decisionBlockLast) -or
        ((Get-TextSha256 -Text $decisionExtractionContent) -cne
            $ExpectedDecisionExtractionCanonicalLfSha256)) {
        throw "$Owner exact decision extraction changed."
    }
    if (($CandidateHomeHelper -cne $homeHelper) -or
        ($CandidateDecisionHelper -cne $decisionHelper)) {
        throw "$Owner extraction-only helper body changed."
    }

    $candidateSemanticText = $CandidateAdapter +
        $CandidateHomeHelper + $CandidateDecisionHelper
    $originalMutations = Get-PersistentMutationMatches -Text $method
    $candidateMutations = Get-PersistentMutationMatches -Text $candidateSemanticText
    $normalizedOriginalMutations = @(
        foreach ($match in $originalMutations) {
            [regex]::Replace(
                $match.Value, '\s+', '').ToLowerInvariant()
        })
    $normalizedCandidateMutations = @(
        foreach ($match in $candidateMutations) {
            [regex]::Replace(
                $match.Value, '\s+', '').ToLowerInvariant()
        })
    $originalMutationInventory = [string]::Join(
        '|', @($normalizedOriginalMutations | Sort-Object))
    $candidateMutationInventory = [string]::Join(
        '|', @($normalizedCandidateMutations | Sort-Object))
    if (($candidateMutations.Count -ne $originalMutations.Count) -or
        ($candidateMutationInventory -cne $originalMutationInventory)) {
        throw "$Owner persistent-write inventory changed."
    }

    $originalCalls = Get-CallHistogram -Text $method
    $candidateCalls = Get-CallHistogram -Text $candidateSemanticText
    foreach ($key in $originalCalls.Keys) {
        if ((-not $candidateCalls.Contains($key)) -or
            ($candidateCalls[$key] -ne $originalCalls[$key])) {
            throw "$Owner original call inventory changed at '$key'."
        }
    }
    $allowedExtraCalls = @(
        'handleaxisownershippublishhomereceipt',
        'prepareaxisownershippublishdecision'
    )
    foreach ($key in $candidateCalls.Keys) {
        if ($originalCalls.Contains($key)) {
            continue
        }
        if (($allowedExtraCalls -notcontains $key) -or
            ($candidateCalls[$key] -ne 1)) {
            throw "$Owner added unexpected call '$key'."
        }
    }
    foreach ($key in $allowedExtraCalls) {
        if ((-not $candidateCalls.Contains($key)) -or
            ($candidateCalls[$key] -ne 1)) {
            throw "$Owner helper call inventory changed at '$key'."
        }
    }

    $decisionCallIndex = $CandidateAdapter.IndexOf(
        $decisionCallMap, [StringComparison]::Ordinal)
    $postDecisionText = $CandidateAdapter.Substring(
        $decisionCallIndex + $decisionCallMap.Length)
    $firstPostDecisionMutation = Get-PersistentMutationMatches -Text $postDecisionText |
        Select-Object -First 1
    if ($null -eq $firstPostDecisionMutation) {
        throw "$Owner lost all post-decision persistent commits."
    }
    if (($CandidateHomeHelper -notmatch '(?m)^\tResult := 2;$') -or
        ([regex]::Matches(
            $CandidateHomeHelper, '(?m)^\tResult := 2;$').Count -ne 1)) {
        throw "$Owner Home Result=2 containment changed."
    }
    if (($CandidateDecisionHelper -notmatch '(?m)^\t\tResult \+= 1;$') -or
        ($CandidateDecisionHelper -notmatch '(?m)^\t\tResult \+= 2;$') -or
        ($CandidateDecisionHelper -notmatch '(?m)^\t\tResult \+= 4;$')) {
        throw "$Owner decision result bit domain changed."
    }

    $reversedAdapter = Get-ReversedAdapter `
        -CandidateAdapter $CandidateAdapter `
        -Owner $Owner
    if ($reversedAdapter -cne $method) {
        throw "$Owner adapter extraction-only reverse-inline changed the monolith."
    }

    $expectedCandidateSource = New-PlannedSourceFromFragments `
        -CandidateAdapter $CandidateAdapter `
        -CandidateHomeHelper $CandidateHomeHelper `
        -CandidateDecisionHelper $CandidateDecisionHelper `
        -CandidateDeclarations $CandidateDeclarations
    if ($CandidateSource -cne $expectedCandidateSource) {
        throw "$Owner candidate source/fragments diverged."
    }
    $reverseSource = Replace-ExactOne `
        -Text $CandidateSource `
        -Old $CandidateAdapter `
        -New $reversedAdapter `
        -Owner "$Owner reverse adapter"
    $reverseSource = Replace-ExactOne `
        -Text $reverseSource `
        -Old $CandidateHomeHelper `
        -New $homeEmptyStub `
        -Owner "$Owner reverse Home body to exact IDE empty stub"
    $reverseSource = Replace-ExactOne `
        -Text $reverseSource `
        -Old $CandidateDecisionHelper `
        -New $decisionEmptyStub `
        -Owner "$Owner reverse decision body to exact IDE empty stub"
    $reverseSource = Replace-ExactOne `
        -Text $reverseSource `
        -Old $CandidateDeclarations `
        -New $privateClassDeclarations `
        -Owner "$Owner reverse exact existing private declarations"
    if (($reverseSource -cne $source) -or
        ((Get-TextSha256 -Text $reverseSource) -cne
            $ExpectedSourceCanonicalLfSha256) -or
        ((Get-TextSha256 -Text (
            ConvertTo-IdeCrlf -Text $reverseSource)) -cne
            $ExpectedSourceIdeCrlfSha256)) {
        throw "$Owner whole-source reverse did not restore exact F923/C636 post-IDE snapshot."
    }
}

function Assert-PublishSplitPositiveRatchet {
    $adapterContent = Get-MethodContent -Method $adapter
    $homeContent = Get-MethodContent -Method $homeHelper
    $decisionContent = Get-MethodContent -Method $decisionHelper
    $candidateIdeCrlf = ConvertTo-IdeCrlf -Text $plannedSource
    if (($Utf8.GetByteCount($adapterContent) -ne
            $ExpectedAdapterCanonicalLfBytes) -or
        ((Get-TextSha256 -Text $adapterContent) -cne
            $ExpectedAdapterCanonicalLfSha256) -or
        ($Utf8.GetByteCount($homeContent) -ne
            $ExpectedHomeHelperCanonicalLfBytes) -or
        ((Get-TextSha256 -Text $homeContent) -cne
            $ExpectedHomeHelperCanonicalLfSha256) -or
        ($Utf8.GetByteCount($decisionContent) -ne
            $ExpectedDecisionHelperCanonicalLfBytes) -or
        ((Get-TextSha256 -Text $decisionContent) -cne
            $ExpectedDecisionHelperCanonicalLfSha256) -or
        ($Utf8.GetByteCount($plannedSource) -ne
            $ExpectedCandidateCanonicalLfBytes) -or
        ((Get-TextSha256 -Text $plannedSource) -cne
            $ExpectedCandidateCanonicalLfSha256) -or
        ($Utf8.GetByteCount($candidateIdeCrlf) -ne
            $ExpectedCandidateIdeCrlfBytes) -or
        ((Get-TextSha256 -Text $candidateIdeCrlf) -cne
            $ExpectedCandidateIdeCrlfSha256)) {
        throw (
            'Publish exact post-IDE-based adapter/helper/whole-candidate ' +
            'byte or hash ratchet drifted.')
    }
}

Assert-PublishSplitCandidate `
    -CandidateAdapter $adapter `
    -CandidateHomeHelper $homeHelper `
    -CandidateDecisionHelper $decisionHelper `
    -CandidateDeclarations $privateClassDeclarations `
    -CandidateSource $plannedSource `
    -Owner 'Publish planned positive candidate'
Assert-PublishSplitPositiveRatchet
$logicalProofReplacementCount = Assert-ApplyPatchPlan `
    -PatchText $logicalProofPatchDocument `
    -ExpectedReplacements $logicalProofReplacements `
    -PreSplitSource $source `
    -PostSplitSource $plannedSource `
    -ProtectedDeclarations $privateClassDeclarations `
    -RelativeTargetPath $CanonicalTargetRelativePath `
    -Owner 'Publish whole logical-proof patch plan'
if ($logicalProofReplacementCount -ne 3) {
    throw 'Publish whole logical-proof patch lost its three logical replacements.'
}

$compactPatchMetrics = @(
    foreach ($plan in $compactPatchPlans) {
        $documentText = [string]$plan.Document.Text
        $documentBytes = $Utf8.GetBytes($documentText)
        $documentSha256 = Get-BytesSha256 -Bytes $documentBytes
        if (($documentBytes.Count -ne [int]$plan.ExpectedBytes) -or
            ($documentSha256 -cne [string]$plan.ExpectedSha256) -or
            ([int]$plan.Document.HunkCount -ne [int]$plan.ExpectedHunks)) {
            throw "Publish compact patch $($plan.Step) byte/hash/hunk ratchet drifted."
        }
        $validatedHunkCount = Assert-ApplyPatchPlan `
            -PatchText $documentText `
            -ExpectedReplacements $plan.Document.ExpectedReplacements `
            -PreSplitSource ([string]$plan.Input) `
            -PostSplitSource ([string]$plan.Output) `
            -ProtectedDeclarations $privateClassDeclarations `
            -RelativeTargetPath $CanonicalTargetRelativePath `
            -Owner "Publish compact patch $($plan.Step) $($plan.Name)"
        if ($validatedHunkCount -ne [int]$plan.ExpectedHunks) {
            throw "Publish compact patch $($plan.Step) validated hunk count drifted."
        }
        $parsedHunks = @(Read-ApplyPatchReplacementHunks `
            -PatchText $documentText `
            -RelativeTargetPath $CanonicalTargetRelativePath `
            -Owner "Publish compact patch $($plan.Step) metrics")
        $maxOldLines = ($parsedHunks.OldLines | Measure-Object -Maximum).Maximum
        $maxNewLines = ($parsedHunks.NewLines | Measure-Object -Maximum).Maximum
        $maxOldBytes = (@($parsedHunks | ForEach-Object {
                    $Utf8.GetByteCount($_.Old)
                }) | Measure-Object -Maximum).Maximum
        $maxNewBytes = (@($parsedHunks | ForEach-Object {
                    $Utf8.GetByteCount($_.New)
                }) | Measure-Object -Maximum).Maximum
        $maxEncodedBytes = ($parsedHunks.EncodedBytes |
            Measure-Object -Maximum).Maximum
        if (($maxOldLines -gt 599) -or
            ($maxOldBytes -gt 22925) -or
            ($maxEncodedBytes -gt $ExpectedCompactDecisionPatchBytes) -or
            ([regex]::Matches(
                [string]$plan.Output,
                [regex]::Escape($privateClassDeclarations)).Count -ne 1)) {
            throw "Publish compact patch $($plan.Step) bounded/declaration contract drifted."
        }
        [pscustomobject]@{
            Step = [int]$plan.Step
            Name = [string]$plan.Name
            FileSuffix = [string]$plan.FileSuffix
            Bytes = $documentBytes.Count
            Sha256 = $documentSha256
            HunkCount = $validatedHunkCount
            InputBytes = $Utf8.GetByteCount([string]$plan.Input)
            InputSha256 = Get-TextSha256 -Text ([string]$plan.Input)
            OutputBytes = $Utf8.GetByteCount([string]$plan.Output)
            OutputSha256 = Get-TextSha256 -Text ([string]$plan.Output)
            MaxOldLines = $maxOldLines
            MaxNewLines = $maxNewLines
            MaxOldBytes = $maxOldBytes
            MaxNewBytes = $maxNewBytes
            MaxEncodedHunkBytes = $maxEncodedBytes
        }
    })
if (($compactPatchMetrics.Count -ne 3) -or
    (($compactPatchMetrics.HunkCount | Measure-Object -Sum).Sum -ne 6) -or
    ($compactPatchMetrics[0].InputSha256 -cne
        $ExpectedSourceCanonicalLfSha256) -or
    ($compactPatchMetrics[0].OutputSha256 -cne
        $compactPatchMetrics[1].InputSha256) -or
    ($compactPatchMetrics[1].OutputSha256 -cne
        $compactPatchMetrics[2].InputSha256) -or
    ($compactPatchMetrics[2].OutputSha256 -cne
        $ExpectedCandidateCanonicalLfSha256)) {
    throw 'Publish compact three-step state chain ratchet drifted.'
}

function Invoke-PublishSplitPlannerSelfTest {
    $lfCheckpoint = Assert-PreSplitSnapshotRatchet `
        -RawBytes ($Utf8.GetBytes($sourceCheckpoint.CanonicalLf)) `
        -Owner 'Publish LF positive fixture'
    $crlfCheckpoint = Assert-PreSplitSnapshotRatchet `
        -RawBytes ($Utf8.GetBytes($sourceCheckpoint.IdeCrlf)) `
        -Owner 'Publish CRLF positive fixture'
    if (($lfCheckpoint.LineEnding -cne 'LF') -or
        ($crlfCheckpoint.LineEnding -cne 'CRLF') -or
        ($lfCheckpoint.CanonicalLf -cne $crlfCheckpoint.CanonicalLf)) {
        throw 'Publish LF/CRLF positive fixtures did not converge.'
    }

    $snapshotNegativeCount = 0
    try {
        [byte[]]$bomSnapshot = @(
            0xEF,
            0xBB,
            0xBF) + $Utf8.GetBytes($sourceCheckpoint.CanonicalLf)
        $null = Assert-PreSplitSnapshotRatchet `
            -RawBytes $bomSnapshot `
            -Owner 'Publish BOM-prefixed snapshot negative fixture'
    }
    catch {
        if ($_.Exception.Message -notmatch 'forbidden UTF-8 BOM') {
            throw
        }
        $snapshotNegativeCount++
    }
    if ($snapshotNegativeCount -ne 1) {
        throw 'Publish snapshot raw-byte ratchet did not reject its BOM fixture.'
    }

    $selfTestPatchReplacementCount = Assert-ApplyPatchPlan `
        -PatchText $logicalProofPatchDocument `
        -ExpectedReplacements $logicalProofReplacements `
        -PreSplitSource $source `
        -PostSplitSource $plannedSource `
        -ProtectedDeclarations $privateClassDeclarations `
        -RelativeTargetPath $CanonicalTargetRelativePath `
        -Owner 'Publish self-test logical-proof patch plan'
    if ($selfTestPatchReplacementCount -ne 3) {
        throw 'Publish self-test logical proof did not prove three replacements.'
    }
    foreach ($plan in $compactPatchPlans) {
        $compactSelfTestCount = Assert-ApplyPatchPlan `
            -PatchText ([string]$plan.Document.Text) `
            -ExpectedReplacements $plan.Document.ExpectedReplacements `
            -PreSplitSource ([string]$plan.Input) `
            -PostSplitSource ([string]$plan.Output) `
            -ProtectedDeclarations $privateClassDeclarations `
            -RelativeTargetPath $CanonicalTargetRelativePath `
            -Owner "Publish self-test compact patch $($plan.Step)"
        if ($compactSelfTestCount -ne [int]$plan.ExpectedHunks) {
            throw "Publish self-test compact patch $($plan.Step) hunk count drifted."
        }
    }

    foreach ($targetFixture in @(
            @{
                Name = 'pre-split LF'
                Text = $sourceCheckpoint.CanonicalLf
                State = 'PostIdePreSplit'
            },
            @{
                Name = 'pre-split CRLF'
                Text = $sourceCheckpoint.IdeCrlf
                State = 'PostIdePreSplit'
            },
            @{
                Name = 'post-split LF'
                Text = $plannedSource
                State = 'PostSplit'
            },
            @{
                Name = 'post-split CRLF'
                Text = ConvertTo-IdeCrlf -Text $plannedSource
                State = 'PostSplit'
            })) {
        $targetState = Assert-CanonicalTargetRatchet `
            -RawBytes ($Utf8.GetBytes([string]$targetFixture.Text)) `
            -Owner "Publish target $($targetFixture.Name) positive fixture"
        if ($targetState.State -cne $targetFixture.State) {
            throw (
                "Publish target $($targetFixture.Name) positive fixture " +
                "resolved $($targetState.State), expected $($targetFixture.State).")
        }
    }
    $targetNegativeCount = 0
    try {
        $null = Assert-CanonicalTargetRatchet `
            -RawBytes ($Utf8.GetBytes(
                $sourceCheckpoint.CanonicalLf + "// target drift`n")) `
            -Owner 'Publish target drift negative fixture'
    }
    catch {
        if ($_.Exception.Message -notmatch 'neither exact post-IDE pre-split') {
            throw
        }
        $targetNegativeCount++
    }
    try {
        $null = Assert-CanonicalTargetRatchet `
            -RawBytes ($Utf8.GetBytes($plannedSource)) `
            -Owner 'Publish post-split apply-preflight negative fixture' `
            -RequirePreSplit
    }
    catch {
        if ($_.Exception.Message -notmatch
                'apply preflight requires exact PostIdePreSplit') {
            throw
        }
        $targetNegativeCount++
    }
    $lfApplyTarget = Assert-CanonicalTargetRatchet `
        -RawBytes ($Utf8.GetBytes($sourceCheckpoint.CanonicalLf)) `
        -Owner 'Publish LF apply-preflight positive fixture' `
        -RequirePreSplit
    Assert-LfPreSplitApplyTarget `
        -TargetState $lfApplyTarget `
        -Owner 'Publish LF apply-preflight positive fixture'
    try {
        $crlfApplyTarget = Assert-CanonicalTargetRatchet `
            -RawBytes ($Utf8.GetBytes($sourceCheckpoint.IdeCrlf)) `
            -Owner 'Publish CRLF direct-apply negative fixture' `
            -RequirePreSplit
        Assert-LfPreSplitApplyTarget `
            -TargetState $crlfApplyTarget `
            -Owner 'Publish CRLF direct-apply negative fixture'
    }
    catch {
        if ($_.Exception.Message -notmatch
                'requires exact LF/F923 pre-split apply target') {
            throw
        }
        $targetNegativeCount++
    }
    if ($targetNegativeCount -ne 3) {
        throw (
            'Publish canonical target ratchet rejected ' +
            "$targetNegativeCount/3 negative fixtures.")
    }

    $fixtures = @()
    $fixtures += [pscustomobject]@{
        Name = 'PrivateDeclarationGlobal'
        Adapter = $adapter
        Home = $homeHelper
        Decision = $decisionHelper
        Declarations = $privateClassDeclarations.Replace(
            "`tFUNCTION HandleAxisOwnershipPublishHomeReceipt",
            "`tFUNCTION GLOBAL HandleAxisOwnershipPublishHomeReceipt")
        Expected = 'private ABI/declaration'
    }
    $fixtures += [pscustomobject]@{
        Name = 'PrivateInputOrderSwapped'
        Adapter = $adapter
        Home = $homeHelper
        Decision = $decisionHelper
        Declarations = (Replace-ExactOne `
            -Text $homeClassDeclaration `
            -Old ("`t`t`tAxisMask `t: UDINT;`n" +
                "`t`t`tAdmissionToken `t: UDINT;`n") `
            -New ("`t`t`tAdmissionToken `t: UDINT;`n" +
                "`t`t`tAxisMask `t: UDINT;`n") `
            -Owner 'PrivateInputOrderSwapped mutation') +
            $privateDeclarationSeparator + $decisionClassDeclaration
        Expected = 'private ABI/declaration'
    }
    $fixtures += [pscustomobject]@{
        Name = 'PublicAbiChanged'
        Adapter = $adapter.Replace(
            "`t`tReportKind : UINT;", "`t`tReportKind : UDINT;")
        Home = $homeHelper
        Decision = $decisionHelper
        Declarations = $privateClassDeclarations
        Expected = 'public ABI'
    }
    $fixtures += [pscustomobject]@{
        Name = 'HomeHelperGlobal'
        Adapter = $adapter
        Home = $homeHelper.Replace(
            'FUNCTION LMCControlCommandService::HandleAxisOwnershipPublishHomeReceipt',
            'FUNCTION GLOBAL LMCControlCommandService::HandleAxisOwnershipPublishHomeReceipt')
        Decision = $decisionHelper
        Declarations = $privateClassDeclarations
        Expected = 'helper private visibility|Home helper ABI'
    }
    $fixtures += [pscustomobject]@{
        Name = 'HomeActualIdeStubStillEmpty'
        Adapter = $adapter
        Home = $homeEmptyStub
        Decision = $decisionHelper
        Declarations = $privateClassDeclarations
        Expected = 'Home helper ABI/local'
    }
    $fixtures += [pscustomobject]@{
        Name = 'DecisionActualIdeStubStillEmpty'
        Adapter = $adapter
        Home = $homeHelper
        Decision = $decisionEmptyStub
        Declarations = $privateClassDeclarations
        Expected = 'decision helper ABI/local'
    }
    $fixtures += [pscustomobject]@{
        Name = 'HomeActualIdeHeaderTabDrift'
        Adapter = $adapter
        Home = Replace-ExactOne `
            -Text $homeHelper `
            -Old "`t`tAxisMask `t: UDINT;`n" `
            -New "`t`tAxisMask : UDINT;`n" `
            -Owner 'HomeActualIdeHeaderTabDrift mutation'
        Decision = $decisionHelper
        Declarations = $privateClassDeclarations
        Expected = 'Home helper ABI/local'
    }
    $fixtures += [pscustomobject]@{
        Name = 'HomeResultSeedChanged'
        Adapter = $adapter
        Home = Replace-ExactOne `
            -Text $homeHelper `
            -Old "`tResult := 2;`n" `
            -New "`tResult := 0;`n" `
            -Owner 'HomeResultSeedChanged mutation'
        Decision = $decisionHelper
        Declarations = $privateClassDeclarations
        Expected = 'Home helper ABI/local|extraction-only helper|Home Result=2'
    }
    $fixtures += [pscustomobject]@{
        Name = 'HomeReceiptMagicChanged'
        Adapter = $adapter
        Home = Replace-ExactOne `
            -Text $homeHelper `
            -Old 'LMC_HOME_OWNER_RECEIPT_MAGIC then' `
            -New 'LMC_HOME_RECORD_MAGIC then' `
            -Owner 'HomeReceiptMagicChanged mutation'
        Decision = $decisionHelper
        Declarations = $privateClassDeclarations
        Expected = 'exact Home extraction'
    }
    $fixtures += [pscustomobject]@{
        Name = 'HomeUpdateCallDropped'
        Adapter = $adapter
        Home = Replace-ExactOne `
            -Text $homeHelper `
            -Old ("`t`t`trebaseUpdateResult := UpdateAxisRebaseRequiredState(`n" +
                "`t`t`t`tSetAxisMask:=0, ClearAxisMask:=AxisMask);`n") `
            -New "`t`t`trebaseUpdateResult := 0;`n" `
            -Owner 'HomeUpdateCallDropped mutation'
        Decision = $decisionHelper
        Declarations = $privateClassDeclarations
        Expected = 'exact Home extraction'
    }
    $fixtures += [pscustomobject]@{
        Name = 'HomeAdapterFenceInverted'
        Adapter = Replace-ExactOne `
            -Text $adapter `
            -Old 'if homeReceiptResult <> 2 then' `
            -New 'if homeReceiptResult = 2 then' `
            -Owner 'HomeAdapterFenceInverted mutation'
        Home = $homeHelper
        Decision = $decisionHelper
        Declarations = $privateClassDeclarations
        Expected = 'exact adapter Result fence'
    }
    $fixtures += [pscustomobject]@{
        Name = 'DecisionHelperGlobal'
        Adapter = $adapter
        Home = $homeHelper
        Decision = $decisionHelper.Replace(
            'FUNCTION LMCControlCommandService::PrepareAxisOwnershipPublishDecision',
            'FUNCTION GLOBAL LMCControlCommandService::PrepareAxisOwnershipPublishDecision')
        Declarations = $privateClassDeclarations
        Expected = 'helper private visibility|decision helper ABI'
    }
    $fixtures += [pscustomobject]@{
        Name = 'DecisionPreemptFlagMaskChanged'
        Adapter = $adapter
        Home = $homeHelper
        Decision = Replace-ExactOne `
            -Text $decisionHelper `
            -Old '0xFFFE0000) = 0);' `
            -New '0xFFFF0000) = 0);' `
            -Owner 'DecisionPreemptFlagMaskChanged mutation'
        Declarations = $privateClassDeclarations
        Expected = 'exact decision extraction'
    }
    $fixtures += [pscustomobject]@{
        Name = 'DecisionResultBitDropped'
        Adapter = $adapter
        Home = $homeHelper
        Decision = Replace-ExactOne `
            -Text $decisionHelper `
            -Old "`t`tResult += 4;`n" `
            -New "`t`tResult += 0;`n" `
            -Owner 'DecisionResultBitDropped mutation'
        Declarations = $privateClassDeclarations
        Expected = 'extraction-only helper|decision result bit'
    }
    $fixtures += [pscustomobject]@{
        Name = 'DecisionAdapterBitSwap'
        Adapter = Replace-ExactOne `
            -Text $adapter `
            -Old ("`tforceQuarantine := (publishDecisionResult and 2) <> 0;`n" +
                "`trestoreLease := (publishDecisionResult and 4) <> 0;`n") `
            -New ("`tforceQuarantine := (publishDecisionResult and 4) <> 0;`n" +
                "`trestoreLease := (publishDecisionResult and 2) <> 0;`n") `
            -Owner 'DecisionAdapterBitSwap mutation'
        Home = $homeHelper
        Decision = $decisionHelper
        Declarations = $privateClassDeclarations
        Expected = 'exact adapter Result fence'
    }
    $fixtures += [pscustomobject]@{
        Name = 'DecisionNegativeGateRemoved'
        Adapter = Replace-ExactOne `
            -Text $adapter `
            -Old 'if publishDecisionResult < 0 then' `
            -New 'if publishDecisionResult = -999 then' `
            -Owner 'DecisionNegativeGateRemoved mutation'
        Home = $homeHelper
        Decision = $decisionHelper
        Declarations = $privateClassDeclarations
        Expected = 'exact adapter Result fence'
    }
    $fixtures += [pscustomobject]@{
        Name = 'ExpectedSessionResampled'
        Adapter = Replace-ExactOne `
            -Text $adapter `
            -Old "`t`tExpectedSession:=expectedSession,`n" `
            -New ("`t`tExpectedSession:=" +
                "OwnershipState[firstRecordBase + 6]`$UDINT,`n") `
            -Owner 'ExpectedSessionResampled mutation'
        Home = $homeHelper
        Decision = $decisionHelper
        Declarations = $privateClassDeclarations
        Expected = 'exact adapter Result fence'
    }
    $fixtures += [pscustomobject]@{
        Name = 'PersistentWriteAdded'
        Adapter = $adapter
        Home = $homeHelper
        Decision = Replace-ExactOne `
            -Text $decisionHelper `
            -Old "`tResult := 0;`n" `
            -New ("`tOwnershipState[0] := 1;`n" + "`tResult := 0;`n") `
            -Owner 'PersistentWriteAdded mutation'
        Declarations = $privateClassDeclarations
        Expected = 'extraction-only helper|persistent-write'
    }
    $fixtures += [pscustomobject]@{
        Name = 'OriginalCallChanged'
        Adapter = $adapter
        Home = $homeHelper
        Decision = $decisionHelper.Replace('_memcmp(', '_memcpy(')
        Declarations = $privateClassDeclarations
        Expected = 'exact decision extraction'
    }
    $fixtures += [pscustomobject]@{
        Name = 'ExtractionOnlyLocalRetained'
        Adapter = Replace-ExactOne `
            -Text $adapter `
            -Old $adapterAddedLocals `
            -New ($localDeclarationByName['preemptToken'] + $adapterAddedLocals) `
            -Owner 'ExtractionOnlyLocalRetained mutation'
        Home = $homeHelper
        Decision = $decisionHelper
        Declarations = $privateClassDeclarations
        Expected = 'extraction-only local movement'
    }
    $fixtures += [pscustomobject]@{
        Name = 'AdapterOversized'
        Adapter = Replace-ExactOne `
            -Text $adapter `
            -Old "`nEND_FUNCTION`n" `
            -New ("`n// " + ('X' * 8000) + "`nEND_FUNCTION`n") `
            -Owner 'AdapterOversized mutation'
        Home = $homeHelper
        Decision = $decisionHelper
        Declarations = $privateClassDeclarations
        Expected = 'method-size hard limit'
    }
    $fixtures += [pscustomobject]@{
        Name = 'CandidateSourceDrift'
        Adapter = $adapter
        Home = $homeHelper
        Decision = $decisionHelper
        Declarations = $privateClassDeclarations
        Expected = 'candidate source/fragments diverged'
        SourceOverride = $plannedSource + "// drift`n"
    }

    $expectedFixtureCount = 22
    if ($fixtures.Count -ne $expectedFixtureCount) {
        throw (
            "Publish negative fixture count is $($fixtures.Count), " +
            "expected $expectedFixtureCount.")
    }
    $rejected = 0
    foreach ($fixture in $fixtures) {
        $candidateSource = if (
            $fixture.PSObject.Properties.Name -contains 'SourceOverride') {
            $fixture.SourceOverride
        }
        else {
            New-PlannedSourceFromFragments `
                -CandidateAdapter $fixture.Adapter `
                -CandidateHomeHelper $fixture.Home `
                -CandidateDecisionHelper $fixture.Decision `
                -CandidateDeclarations $fixture.Declarations
        }
        $didReject = $false
        try {
            Assert-PublishSplitCandidate `
                -CandidateAdapter $fixture.Adapter `
                -CandidateHomeHelper $fixture.Home `
                -CandidateDecisionHelper $fixture.Decision `
                -CandidateDeclarations $fixture.Declarations `
                -CandidateSource $candidateSource `
                -Owner "Publish fixture $($fixture.Name)"
        }
        catch {
            $didReject = $true
            if ($_.Exception.Message -notmatch $fixture.Expected) {
                throw (
                    "Publish fixture '$($fixture.Name)' rejected by the " +
                    "wrong contract: $($_.Exception.Message)")
            }
        }
        if (-not $didReject) {
            throw "Publish negative fixture '$($fixture.Name)' was accepted."
        }
        $rejected++
    }
    return $rejected + $targetNegativeCount + $snapshotNegativeCount
}

if ($RunSelfTest) {
    $negativeCount = Invoke-PublishSplitPlannerSelfTest
    Write-Host (
        'PASS LASAL.AxisOwnershipPublishSplitPlan.SelfTest (' +
        "$negativeCount/$negativeCount negative fixtures rejected; " +
        'LF/CRLF state positives, LF-only apply preflight, exact candidate, ' +
        'and reversible logical/compact patch plans accepted)')
    return
}

$adapterContent = Get-MethodContent -Method $adapter
$homeHelperContent = Get-MethodContent -Method $homeHelper
$decisionHelperContent = Get-MethodContent -Method $decisionHelper
$originalMutations = Get-PersistentMutationMatches -Text $method
$originalMutationInventory = Get-NormalizedInventory -Matches $originalMutations
$originalCalls = Get-CallHistogram -Text $method
$plannedCalls = Get-CallHistogram -Text (
    $adapter + $homeHelper + $decisionHelper)
$reverseAdapter = Get-ReversedAdapter `
    -CandidateAdapter $adapter `
    -Owner 'Publish result reverse'
$logicalProofPatchBytes = $Utf8.GetBytes($logicalProofPatchDocument)
$logicalProofPatchSha256 = Get-BytesSha256 -Bytes $logicalProofPatchBytes
if (($logicalProofPatchBytes.Count -ne $ExpectedLogicalProofPatchBytes) -or
    ($logicalProofPatchSha256 -cne $ExpectedLogicalProofPatchSha256)) {
    throw 'Publish whole logical-proof patch byte/hash ratchet drifted.'
}
$emittedApplyPatchPath = $null
$emittedCompactPatchPaths = @()
if ($emitLogicalProofRequested) {
    if (($canonicalTargetState.State -cne 'PostIdePreSplit') -or
        (-not $RequirePreSplitTarget)) {
        throw 'Publish apply_patch emission lost its exact pre-split target preflight.'
    }
    $emittedApplyPatchPath = Resolve-EvidenceEmitPath `
        -RequestedPath $EmitApplyPatchPath
    Write-CreateNewUtf8File `
        -Path $emittedApplyPatchPath `
        -Bytes $logicalProofPatchBytes
    $emittedApplyPatchBytes = [IO.File]::ReadAllBytes($emittedApplyPatchPath)
    if (($emittedApplyPatchBytes.Count -ne $logicalProofPatchBytes.Count) -or
        ((Get-BytesSha256 -Bytes $emittedApplyPatchBytes) -cne
            $logicalProofPatchSha256)) {
        throw 'Published apply_patch artifact differs from the validated document.'
    }
}
if ($emitCompactSetRequested) {
    Assert-LfPreSplitApplyTarget `
        -TargetState $canonicalTargetState `
        -Owner 'Publish compact patch-set emission'
    if (($EmitCompactApplyPatchPrefix -notmatch
            '^[A-Za-z0-9][A-Za-z0-9._-]*$') -or
        $EmitCompactApplyPatchPrefix.EndsWith(
            '.patch',
            [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Publish compact patch-set prefix is not one safe extensionless leaf.'
    }
    $compactEmitPlans = @(
        foreach ($plan in $compactPatchPlans) {
            $requestedLeaf = $EmitCompactApplyPatchPrefix + '_' +
                [string]$plan.FileSuffix
            [pscustomobject]@{
                Plan = $plan
                Path = Resolve-EvidenceEmitPath -RequestedPath $requestedLeaf
                Bytes = $Utf8.GetBytes([string]$plan.Document.Text)
            }
        })
    foreach ($emitPlan in $compactEmitPlans) {
        if (Test-Path -LiteralPath $emitPlan.Path) {
            throw "Publish compact patch output already exists: $($emitPlan.Path)"
        }
    }
    foreach ($emitPlan in $compactEmitPlans) {
        Write-CreateNewUtf8File `
            -Path $emitPlan.Path `
            -Bytes $emitPlan.Bytes
        $readBack = [IO.File]::ReadAllBytes($emitPlan.Path)
        if (($readBack.Count -ne $emitPlan.Bytes.Count) -or
            ((Get-BytesSha256 -Bytes $readBack) -cne
                [string]$emitPlan.Plan.ExpectedSha256)) {
            throw "Publish compact patch $($emitPlan.Plan.Step) read-back drifted."
        }
        $emittedCompactPatchPaths += $emitPlan.Path
    }
}

$result = [ordered]@{
    status = 'PASS'
    mode = if (($null -eq $emittedApplyPatchPath) -and
        ($emittedCompactPatchPaths.Count -eq 0)) {
        'captured post-IDE in-memory planning only; no repository write'
    }
    elseif ($emittedCompactPatchPaths.Count -eq 3) {
        'emit-only compact 3-step apply_patch set; canonical source remains untouched'
    }
    else {
        'emit-only logical-proof patch; canonical source remains untouched'
    }
    source = [ordered]@{
        snapshotInputPath = $snapshotInputPath
        canonicalTargetPath = $sourcePath
        targetState = $canonicalTargetState.State
        targetLineEnding = $canonicalTargetState.LineEnding
        targetPhysicalBytes = $canonicalTargetState.PhysicalBytes
        targetPhysicalSha256 = $canonicalTargetState.PhysicalSha256
        targetCanonicalLfSha256 =
            $canonicalTargetState.CanonicalLfSha256
        targetIdeCrlfSha256 = $canonicalTargetState.IdeCrlfSha256
        requirePreSplitTarget = [bool]$RequirePreSplitTarget
        requireLfPreSplitTarget = [bool]$RequireLfPreSplitTarget
        snapshotInputLineEnding = $sourceCheckpoint.LineEnding
        snapshotPhysicalBytes = $sourceCheckpoint.PhysicalBytes
        snapshotPhysicalSha256 = $sourceCheckpoint.PhysicalSha256
        snapshotCanonicalLfBytes = $Utf8.GetByteCount($source)
        snapshotCanonicalLfSha256 = Get-TextSha256 -Text $source
        snapshotIdeCrlfBytes = $Utf8.GetByteCount(
            $sourceCheckpoint.IdeCrlf)
        snapshotIdeCrlfSha256 = Get-TextSha256 -Text (
            $sourceCheckpoint.IdeCrlf)
    }
    originalMethod = [ordered]@{
        dimensionsWithTerminalEol = Get-ByteDimensions -Text $method
        canonicalLfSha256 = $methodSha256
        localDeclarations = $localDeclarationByName.Count
        persistentMutationEvents = $originalMutations.Count
        persistentMutationInventorySha256 = Get-TextSha256 -Text (
            $originalMutationInventory)
        callHistogram = Get-HistogramInventory -Histogram $originalCalls
        resultAssignments = [regex]::Matches(
            $method, '(?i)\bResult\s*:=').Count
        returns = [regex]::Matches($method, '(?i)\bRETURN\s*;').Count
    }
    extraction = [ordered]@{
        home = [ordered]@{
            dimensions = Get-ByteDimensions -Text $homeExtractionContent
            canonicalLfSha256 = $homeExtractionLfSha256
            ideCrlfSha256 = $homeExtractionCrlfSha256
            extractionOnlyLocals = $homeExtractionOnlyLocals.Count
        }
        decision = [ordered]@{
            dimensions = Get-ByteDimensions -Text $decisionExtractionContent
            canonicalLfSha256 = $decisionExtractionLfSha256
            ideCrlfSha256 = $decisionExtractionCrlfSha256
            extractionOnlyLocals = $decisionExtractionOnlyLocals.Count
        }
    }
    postIdeGeneratedBaseline = [ordered]@{
        declarations = [ordered]@{
            home = [ordered]@{
                canonicalLfBytes = $Utf8.GetByteCount(
                    $homeClassDeclaration)
                canonicalLfSha256 = Get-TextSha256 -Text (
                    $homeClassDeclaration)
            }
            decision = [ordered]@{
                canonicalLfBytes = $Utf8.GetByteCount(
                    $decisionClassDeclaration)
                canonicalLfSha256 = Get-TextSha256 -Text (
                    $decisionClassDeclaration)
            }
        }
        emptyStubs = [ordered]@{
            home = [ordered]@{
                canonicalLfBytes = $Utf8.GetByteCount($homeEmptyStub)
                canonicalLfSha256 = Get-TextSha256 -Text $homeEmptyStub
            }
            decision = [ordered]@{
                canonicalLfBytes = $Utf8.GetByteCount($decisionEmptyStub)
                canonicalLfSha256 = Get-TextSha256 -Text $decisionEmptyStub
            }
        }
    }
    candidate = [ordered]@{
        adapter = [ordered]@{
            dimensions = Get-ByteDimensions -Text $adapterContent
            canonicalLfSha256 = Get-TextSha256 -Text $adapterContent
            ideCrlfSha256 = Get-TextSha256 -Text (
                ConvertTo-IdeCrlf -Text $adapterContent)
        }
        homeHelper = [ordered]@{
            private = $true
            dimensions = Get-ByteDimensions -Text $homeHelperContent
            canonicalLfSha256 = Get-TextSha256 -Text $homeHelperContent
            ideCrlfSha256 = Get-TextSha256 -Text (
                ConvertTo-IdeCrlf -Text $homeHelperContent)
        }
        decisionHelper = [ordered]@{
            private = $true
            dimensions = Get-ByteDimensions -Text $decisionHelperContent
            canonicalLfSha256 = Get-TextSha256 -Text $decisionHelperContent
            ideCrlfSha256 = Get-TextSha256 -Text (
                ConvertTo-IdeCrlf -Text $decisionHelperContent)
            successDomain = '0..7; bits 0/1/2 map preemptRootValid/forceQuarantine/restoreLease'
        }
        wholeSource = [ordered]@{
            canonicalLfBytes = $Utf8.GetByteCount($plannedSource)
            canonicalLfSha256 = Get-TextSha256 -Text $plannedSource
            ideCrlfBytes = $Utf8.GetByteCount(
                (ConvertTo-IdeCrlf -Text $plannedSource))
            ideCrlfSha256 = Get-TextSha256 -Text (
                ConvertTo-IdeCrlf -Text $plannedSource)
        }
        callHistogram = Get-HistogramInventory -Histogram $plannedCalls
    }
    applyPatch = [ordered]@{
        officialWorkflow = 'compact 3-step LF-only sequence'
        format = 'Codex apply_patch'
        targetRelativePath = $CanonicalTargetRelativePath
        canonicalTargetWritten = $false
        preflightIsPointInTime = $true
        requiredInputState = 'exact canonical LF pre-split F923'
        applyReadyNow = (
            ($canonicalTargetState.State -ceq 'PostIdePreSplit') -and
            ($canonicalTargetState.LineEnding -ceq 'LF') -and
            ($canonicalTargetState.PhysicalSha256 -ceq
                $ExpectedSourceCanonicalLfSha256))
        crlfDirectApplyRejected = $true
        eolBoundary =
            'apply_patch changes only touched lines; CRLF direct apply can create mixed EOL'
        requiredPreApplyAction =
            'normalize the whole file as a separate phase if needed, verify F923, then rerun -RequireLfPreSplitTarget immediately before step 1'
        logicalProof = [ordered]@{
            applyable = $false
            purpose = 'whole-plan 3 logical replacements and exact reverse proof only'
            emitted = ($null -ne $emittedApplyPatchPath)
            emittedPath = $emittedApplyPatchPath
            bytes = $logicalProofPatchBytes.Count
            sha256 = $logicalProofPatchSha256
            logicalReplacementCount = $logicalProofReplacementCount
        }
        compactSequence = [ordered]@{
            emitted = ($emittedCompactPatchPaths.Count -eq 3)
            emittedPaths = @($emittedCompactPatchPaths)
            prefix = $EmitCompactApplyPatchPrefix
            artifactCount = 3
            totalHunkCount =
                ($compactPatchMetrics.HunkCount | Measure-Object -Sum).Sum
            maxHunksPerArtifact =
                ($compactPatchMetrics.HunkCount | Measure-Object -Maximum).Maximum
            maxOldMatchLines =
                ($compactPatchMetrics.MaxOldLines | Measure-Object -Maximum).Maximum
            maxNewLines =
                ($compactPatchMetrics.MaxNewLines | Measure-Object -Maximum).Maximum
            maxOldMatchBytes =
                ($compactPatchMetrics.MaxOldBytes | Measure-Object -Maximum).Maximum
            maxNewBytes =
                ($compactPatchMetrics.MaxNewBytes | Measure-Object -Maximum).Maximum
            maxEncodedHunkBytes =
                ($compactPatchMetrics.MaxEncodedHunkBytes |
                    Measure-Object -Maximum).Maximum
            steps = @(
                foreach ($metric in $compactPatchMetrics) {
                    [ordered]@{
                        step = $metric.Step
                        name = $metric.Name
                        emittedPath = if (
                            $emittedCompactPatchPaths.Count -eq 3) {
                            $emittedCompactPatchPaths[$metric.Step - 1]
                        }
                        else {
                            $null
                        }
                        patchBytes = $metric.Bytes
                        patchSha256 = $metric.Sha256
                        hunkCount = $metric.HunkCount
                        inputBytes = $metric.InputBytes
                        inputSha256 = $metric.InputSha256
                        outputBytes = $metric.OutputBytes
                        outputSha256 = $metric.OutputSha256
                        maxOldLines = $metric.MaxOldLines
                        maxNewLines = $metric.MaxNewLines
                        maxOldBytes = $metric.MaxOldBytes
                        maxNewBytes = $metric.MaxNewBytes
                        maxEncodedHunkBytes = $metric.MaxEncodedHunkBytes
                    }
                })
            finalLfBytes = $ExpectedCandidateCanonicalLfBytes
            finalLfSha256 = $ExpectedCandidateCanonicalLfSha256
            reverseValidatedInMemory = $true
            reverseRestoresLfBytes = $ExpectedSourceCanonicalLfBytes
            reverseRestoresLfSha256 = $ExpectedSourceCanonicalLfSha256
        }
    }
    preservation = [ordered]@{
        publicAbiExact = $adapter.StartsWith(
            $publicInterface, [StringComparison]::Ordinal)
        privateHelpersHaveNoGlobal =
            (($privateClassDeclarations -notmatch
                '(?i)\b(?:GLOBAL|VIRTUAL)\b') -and
             ($homeHelper -notmatch
                '(?i)^FUNCTION\s+(?:GLOBAL|VIRTUAL\s+GLOBAL)\s+') -and
             ($decisionHelper -notmatch
                '(?i)^FUNCTION\s+(?:GLOBAL|VIRTUAL\s+GLOBAL)\s+'))
        homeResult2Contained =
            ([regex]::Matches(
                $homeHelper,
                '(?m)^\tResult := 2;$').Count -eq 1)
        decisionBitDomainChecked = $true
        actualIdeDeclarationsPreserved =
            ([regex]::Matches(
                $plannedSource,
                [regex]::Escape($privateClassDeclarations)).Count -eq 1)
        actualIdeStubHeaderCapturedAndPreserved =
            ($homeHelper.StartsWith(
                $homeStubInterface,
                [StringComparison]::Ordinal) -and
             $decisionHelper.StartsWith(
                $decisionStubInterface,
                [StringComparison]::Ordinal))
        codeGeneratorAppendLayoutCapturedAndVerified =
            (($classTableIndex -gt 0) -and
             ($homeEmptyStubMatches[0].Index -lt
                $decisionEmptyStubMatches[0].Index) -and
             ($decisionStubEnd -eq $source.Length))
        onlyPublicMethodAndTwoStubBodiesPlanned = $true
        wholeSourceHashIsPostIdeSnapshotBasedPlan = $true
        requiresPostIdeSnapshotRebaseline = $false
        extractionOnlyLocalMovement = '16 Home + 41 decision locals'
        persistentMutationInventoryExact = $true
        originalCallInventoryExact = $true
        reverseAdapterRestoredMonolithExact = ($reverseAdapter -ceq $method)
        reversePlanRestoredSourceCanonicalLfSha256 =
            $ExpectedSourceCanonicalLfSha256
        reversePlanRestoredSourceIdeCrlfSha256 =
            $ExpectedSourceIdeCrlfSha256
    }
    artifactScope = [ordered]@{
        projectLcb =
            'out-of-scope; not read or written by this source-only planner'
        classesLcb =
            'out-of-scope; captured manifest and generated gate own evidence'
        network =
            'out-of-scope; not read or written by this source-only planner'
    }
    expectedPostSplitMethodInventory = [ordered]@{
        classes = 6
        methods = 98
        underLimit = 95
        baselineDebt = 3
    }
}

$result | ConvertTo-Json -Depth 9
