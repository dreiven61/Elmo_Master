param(
    [string]$RepositoryRoot = ""
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $RepositoryRoot = [System.IO.Path]::GetFullPath(
        (Join-Path $PSScriptRoot '..\..\..\..'))
}

$sourcePath = Join-Path $RepositoryRoot (
    'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\' +
    'LMCSdoExecutor\LMCSdoExecutor.st')
if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) {
    throw "LMCSdoExecutor source was not found: $sourcePath"
}

$source = Get-Content -LiteralPath $sourcePath -Raw

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

function Assert-NotMatch {
    param(
        [string]$Text,
        [string]$Pattern,
        [string]$Message
    )

    if ($Text -match $Pattern) {
        throw $Message
    }
}

function Get-MethodBlock {
    param([string]$MethodName)

    $escaped = [regex]::Escape($MethodName)
    $match = [regex]::Match(
        $source,
        ('(?s)FUNCTION (?:VIRTUAL )?(?:GLOBAL )?' + $escaped +
            '.*?END_FUNCTION'))
    if (-not $match.Success) {
        throw "LASAL method was not found: $MethodName"
    }

    return $match.Value
}

Assert-Match $source 'RequestSource\s*:\s*UDINT\s*;' `
    'LMCSdoExecutor RequestSource declaration is missing.'
Assert-Match $source `
    '(?s)#define LMC_SDO_SOURCE_NONE\s+0.*?#define LMC_SDO_SOURCE_MANUAL_SERVER\s+1.*?#define LMC_SDO_SOURCE_PROGRAMMATIC\s+2' `
    'LMCSdoExecutor request-source constants are incomplete or reordered.'

$manual = Get-MethodBlock `
    'LMCSdoExecutor::ParaReadWrite::Write'
Assert-NotMatch $manual `
    'production executor cannot be started through the manual channel' `
    'LMCSdoExecutor still documents the manual Server entry as disabled.'
Assert-Match $manual 'ParaReadWrite\s*:=\s*input\s*;' `
    'ParaReadWrite.Write does not accept the manual trigger value.'
Assert-Match $manual `
    'RequestSource\s*:=\s*LMC_SDO_SOURCE_MANUAL_SERVER\s*;' `
    'ParaReadWrite.Write does not reserve the manual request source.'
Assert-Match $manual `
    '(?s)sigclib_atomic_cmpxchgU32\(.*?LMC_SDO_EXEC_IDLE.*?LMC_SDO_EXEC_ARMING.*?LMC_SDO_EXEC_RUNNING' `
    'ParaReadWrite.Write does not reserve the shared executor state.'
Assert-Match $manual 'toSlave\.StartReadSDO\(' `
    'ParaReadWrite.Write does not dispatch manual SDO Read.'
Assert-Match $manual 'toSlave\.StartWriteSDO\(' `
    'ParaReadWrite.Write does not dispatch manual SDO Write.'
Assert-Match $manual `
    'ParaLength\s*>\s*sizeof\(ParaValue\)' `
    'Manual numeric Write does not reject an oversized ParaLength.'
Assert-Match $manual `
    '(?s)ClassState\s*:=\s*BUSY\s*;.*?startResult\s*<>\s*READY\s*then\s*ClassState\s*:=\s*ERROR\s*;' `
    'Manual start result is not exposed through ClassState.'
Assert-Match $manual `
    'RequestSource\s*:=\s*LMC_SDO_SOURCE_NONE\s*;' `
    'Manual start rejection does not release its request source.'

$paraType = Get-MethodBlock 'LMCSdoExecutor::ParaType::Write'
Assert-Match $paraType `
    '(?s)if input = 0 then.*?ParaType\s*:=\s*0\s*;.*?else.*?ParaType\s*:=\s*1\s*;.*?result\s*:=\s*ParaType\s*;' `
    'ParaType.Write does not preserve EtherCAT_SDOBase semantics.'

$paraString = Get-MethodBlock 'LMCSdoExecutor::ParaString::Write'
Assert-Match $paraString 'ParaString\s*:=\s*input\s*;' `
    'ParaString.Write does not accept the manual string handle.'
Assert-Match $paraString `
    'strSDOParaString\.Data\.Write\(ParaString\)' `
    'ParaString.Write does not forward to the inherited String client.'

foreach ($methodName in @(
        'LMCSdoExecutor::TryStartRead',
        'LMCSdoExecutor::TryStartWrite')) {
    $block = Get-MethodBlock $methodName
    Assert-Match $block `
        'RequestSource\s*<>\s*LMC_SDO_SOURCE_NONE' `
        "$methodName does not reject a competing manual/programmatic owner."
    Assert-Match $block `
        'RequestSource\s*:=\s*LMC_SDO_SOURCE_PROGRAMMATIC\s*;' `
        "$methodName does not identify the programmatic owner."
    Assert-Match $block `
        'RequestSource\s*:=\s*LMC_SDO_SOURCE_NONE\s*;' `
        "$methodName does not release the source on pre-callback failure."
}

$callback = Get-MethodBlock `
    'LMCSdoExecutor::ClassState::NewInst'
Assert-Match $callback `
    '(?s)RequestSource\s*=\s*LMC_SDO_SOURCE_MANUAL_SERVER.*?RequestSource\s*<>\s*LMC_SDO_SOURCE_PROGRAMMATIC' `
    'The callback does not dispatch manual before programmatic completion.'
Assert-Match $callback `
    '(?s)ParaLength\s*:=\s*actualLength\s*;.*?strSDOParaString\.TxtSet\(' `
    'Manual Read completion does not publish length/string results.'
Assert-Match $callback `
    '(?s)osResult\s*<>\s*0.*?ErrorCode\s*:=\s*abortCode\s*;.*?ClassState\s*:=\s*READY' `
    'Manual callback status does not preserve abort and success semantics.'
Assert-Match $callback `
    'RequestSource\s*:=\s*LMC_SDO_SOURCE_NONE\s*;' `
    'Manual/programmatic callback release does not clear the source.'

$copyCompletion = Get-MethodBlock `
    'LMCSdoExecutor::CopyCompletion'
Assert-Match $copyCompletion `
    'RequestSource\s*<>\s*LMC_SDO_SOURCE_PROGRAMMATIC' `
    'CopyCompletion does not reject non-programmatic ownership.'
Assert-Match $copyCompletion `
    'RequestSource\s*:=\s*LMC_SDO_SOURCE_NONE\s*;' `
    'CopyCompletion does not release programmatic ownership.'

$markOrphan = Get-MethodBlock 'LMCSdoExecutor::MarkOrphan'
Assert-Match $markOrphan `
    'RequestSource\s*<>\s*LMC_SDO_SOURCE_PROGRAMMATIC' `
    'MarkOrphan can affect a manual Server request.'

$isReusable = Get-MethodBlock 'LMCSdoExecutor::IsReusable'
Assert-Match $isReusable `
    '(?s)RequestSource\s*=\s*LMC_SDO_SOURCE_NONE.*?AdapterState.*?LMC_SDO_EXEC_IDLE' `
    'IsReusable does not require both source-none and adapter-idle.'

Write-Output 'PASS LMCSdoExecutor dual-entry source contract.'
