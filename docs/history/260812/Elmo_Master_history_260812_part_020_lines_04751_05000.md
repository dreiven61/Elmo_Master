>
> distribution 순서도 연결됐습니다. 후보 Run EXE/DLL/config를 복사한 직후 실제 EXE gate를 실행하고, 그 뒤 semantic 검사와 manifest inventory를 생성합니다. 현재는 테스트 시점 EXE 해시와 최종 manifest 직전 EXE 해시의 equality assertion, 그리고 barrier scheduling race를 마무리하고 있습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-Process MSBuild,LasalApiWpfTestApp.SmokeTests,LasalMotionControlApiExample -ErrorAction SilentlyContinue | Select-Object Id,ProcessName,StartTime,CPU,Path; git status --short -- LMC_Library/LasalApiWpfTestApp LMC_Library/LMC_API/Build-LmcApiDistribution.ps1`
>
> </details>
>
> 핵심 targeted 결과가 나왔습니다. Debug와 Release의 실제 `LasalMotionControlApiExample.exe`를 각각 두 번 띄우는 gate가 모두 PASS했습니다. 첫 EXE의 X-close NACK 뒤 프로세스가 종료됐고, live contender는 mutex로 차단됐으며, 두 번째 EXE는 동일 endpoint에서 실패 TCP 1개를 폐기한 뒤 fresh TCP 1개로 복구했습니다. 지금 기존 339개 전수 회귀를 Debug/Release로 다시 돌리는 중입니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `Get-Process MSBuild,LasalApiWpfTestApp.SmokeTests,LasalMotionControlApiExample -ErrorAction SilentlyContinue | Select-Object Id,ProcessName,StartTime,CPU,Path`
> - Ran `Get-Process MSBuild,LasalApiWpfTestApp.SmokeTests,LasalMotionControlApiExample -ErrorAction SilentlyContinue | Select-Object Id,ProcessName,StartTime,CPU`
> - Ran `Get-Process MSBuild,LasalApiWpfTestApp.SmokeTests,LasalMotionControlApiExample -ErrorAction SilentlyContinue | Select-Object Id,ProcessName,StartTime,CPU`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> Tool call
>
> Tool: read_thread_terminal
> Status: completed
>
> - Ran `Get-Content 'LMC_Library\LMC_API\Build-LmcApiDistribution.ps1' -TotalCount 120`
>
> </details>
>
> 기존 전수 회귀는 Debug `339/339`, Release `339/339` 모두 첫 실패 없이 통과했습니다. 실제 EXE gate도 최종본에서 각각 약 3초로 PASS했고 temp/process 잔여는 0입니다. 지금 별도 candidate 경로에서 전체 distribution 파이프라인을 실행해 “복사된 Run EXE 자체”가 gate를 통과하고 그 동일 해시가 최종 manifest에 들어가는지 확인 중입니다. canonical distribution은 변경하지 않습니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `Get-Process MSBuild,LasalApiWpfTestApp.SmokeTests,LasalMotionControlApiExample,pwsh,powershell -ErrorAction SilentlyContinue | Where-Object { $_.Id -ne $PID } | Select-Object Id,ProcessName,StartTime,CPU,Path | Sort-Object StartTime`
> - Ran `rg -n "function Copy-LmcDevelopmentExample|Copy-LmcDevelopmentExample" LMC_Library/LMC_API/Build-LmcApiDistribution.ps1; Get-Content 'LMC_Library\LMC_API\Build-LmcApiDistribution.ps1' | Select-Object -Skip 310 -First 220`
> - Ran `Get-Content 'LMC_Library\LMC_API\Build-LmcApiDistribution.ps1' | Select-Object -Skip 180 -First 115`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-Process MSBuild,LasalApiWpfTestApp.SmokeTests,LasalMotionControlApiExample,pwsh -ErrorAction SilentlyContinue | Where-Object { $_.Id -ne $PID } | Select-Object Id,ProcessName,StartTime,CPU,Path; git status --short -- LMC_Library/LasalApiWpfTestApp LMC_Library/LMC_API/Build-LmcApiDistribution.ps1`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `git diff --check -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/App.xaml.cs LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/ExecutableRelaunchProbe.cs LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/Program.cs LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfExecutableRelaunchIntegrationTests.cs LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/LasalApiWpfTestApp.SmokeTests.csproj; git diff --cached --check`
>
> </details>
>
> 전체 distribution 실행은 새 gate 전에 기존 LASAL 정적 계약이 현재 dirty 생성물 때문에 정상 차단했습니다. 이 실패를 우회해 “전체 pipeline PASS”로 꾸미지는 않겠습니다. 대신 같은 `Copy-LmcDevelopmentExample` 변환 함수를 그대로 사용해 `%TEMP%`에 binary-reference 후보를 만들고, 그 후보 Run EXE만 독립적으로 빌드·실행하여 새 gate 자체의 package 경계를 확인하겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `rg -n '^function (Get-LmcProjectSourceEntries|Resolve-LmcSafeProjectFile|Get-LmcProjectEntryKey|ConvertTo-LmcDistributionExampleProject|Copy-LmcDevelopmentExample)' LMC_Library/LMC_API/Build-LmcApiDistribution.ps1`
>
> </details>
>
> 첫 임시 candidate 명령은 실행 전 JavaScript 문자열의 Windows 경로 이스케이프에서 막혔고 파일이나 프로세스는 만들어지지 않았습니다. 경로를 raw 문자열로 고쳐 동일 검증을 다시 실행합니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$ErrorActionPreference = 'Stop'
> $repo = [System.IO.Path]::GetFullPath('C:\work\Elmo\Elmo_Master')
> $buildScript = Join-Path $repo 'LMC_Library\LMC_API\Build-LmcApiDistribution.ps1'
> $parseTokens = $null
> $parseErrors = $null
> $buildAst = [System.Management.Automation.Language.Parser]::ParseFile($buildScript, [ref]$parseTokens, [ref]$parseErrors)
> if ($parseErrors.Count -ne 0) { throw 'Build script AST parse failed.' }
> $requiredFunctions = @('Get-LmcProjectSourceEntries','Get-LmcProjectEntryKey','Resolve-LmcSafeProjectFile','ConvertTo-LmcDistributionExampleProject','Copy-LmcDevelopmentExample')
> foreach ($functionName in $requiredFunctions) {
>     $matches = @($buildAst.FindAll({
>         param($node)
>         $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -ceq $functionName
>     }, $true))
>     if ($matches.Count -ne 1) { throw "Expected one function definition: $functionName" }
>     Invoke-Expression $matches[0].Extent.Text
> }
> $RepositoryRoot = $repo
> $developmentExampleRoot = Join-Path $repo 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp'
> $developmentExampleProject = Join-Path $developmentExampleRoot 'LasalApiWpfTestApp.csproj'
> $sourceDll = Join-Path $repo 'LMC_Library\LMC_API_Delivery\src\bin\Release\LasalMotionControlLib.dll'
> $smokeProject = Join-Path $repo 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\LasalApiWpfTestApp.SmokeTests.csproj'
> $msbuild = 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe'
> foreach ($required in @($sourceDll,$smokeProject,$msbuild,$developmentExampleProject)) {
>     if (-not (Test-Path -LiteralPath $required -PathType Leaf)) { throw "Required candidate input missing: $required" }
> }
> $probeToken = [Guid]::NewGuid().ToString('N')
> $tempParent = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath()).TrimEnd('\')
> $candidateRoot = [System.IO.Path]::GetFullPath((Join-Path $tempParent ('Elmo.WpfBinaryCandidate.' + $probeToken)))
> $requiredPrefix = $tempParent + '\Elmo.WpfBinaryCandidate.'
> if (-not $candidateRoot.StartsWith($requiredPrefix,[System.StringComparison]::Ordinal)) {
>     throw "Candidate root escaped the intended temp prefix: $candidateRoot"
> }
> try {
>     if (Test-Path -LiteralPath $candidateRoot) { throw "Fresh candidate root already exists: $candidateRoot" }
>     $apiDirectory = Join-Path $candidateRoot '01_API'
>     $exampleProgram = Join-Path $candidateRoot '02_Example_Program'
>     $exampleProjectRoot = Join-Path $exampleProgram 'LasalApiWpfTestApp'
>     $runDirectory = Join-Path $exampleProgram 'Run'
>     New-Item -ItemType Directory -Path @($apiDirectory,$runDirectory) -Force | Out-Null
>     if ((Get-Item -LiteralPath $candidateRoot -Force).Attributes -band [System.IO.FileAttributes]::ReparsePoint) {
>         throw 'Candidate root is a reparse point.'
>     }
>     Copy-Item -LiteralPath $sourceDll -Destination (Join-Path $apiDirectory 'LasalMotionControlLib.dll')
>     $candidateProject = Copy-LmcDevelopmentExample -DestinationRoot $exampleProjectRoot
>     $candidateBuildArgs = @($candidateProject,'/t:Rebuild','/p:Configuration=Release','/p:Platform=AnyCPU','/nologo','/verbosity:minimal')
>     & $msbuild @candidateBuildArgs
>     if ($LASTEXITCODE -ne 0) { throw 'Binary-reference candidate Release build failed.' }
>     $releaseOutput = Join-Path $exampleProjectRoot 'bin\Release'
>     $candidateExe = Join-Path $releaseOutput 'LasalMotionControlApiExample.exe'
>     $candidateDll = Join-Path $releaseOutput 'LasalMotionControlLib.dll'
>     foreach ($artifact in @($candidateExe,$candidateDll)) {
>         if (-not (Test-Path -LiteralPath $artifact -PathType Leaf)) { throw "Candidate output missing: $artifact" }
>         Copy-Item -LiteralPath $artifact -Destination $runDirectory
>     }
>     $candidateConfig = $candidateExe + '.config'
>     if (Test-Path -LiteralPath $candidateConfig -PathType Leaf) {
>         Copy-Item -LiteralPath $candidateConfig -Destination $runDirectory
>     }
>     $runExe = Join-Path $runDirectory 'LasalMotionControlApiExample.exe'
>     $runDll = Join-Path $runDirectory 'LasalMotionControlLib.dll'
>     $preExeHash = (Get-FileHash -LiteralPath $runExe -Algorithm SHA256).Hash
>     $preDllHash = (Get-FileHash -LiteralPath $runDll -Algorithm SHA256).Hash
>     $gateArgs = @($smokeProject,'/t:RunWpfExecutableRelaunchTest','/p:Configuration=Release','/p:Platform=AnyCPU',("/p:WpfExecutableRelaunchExe=" + $runExe),'/nologo','/verbosity:minimal')
>     & $msbuild @gateArgs
>     if ($LASTEXITCODE -ne 0) { throw 'Binary-reference Run EXE relaunch gate failed.' }
>     $postExeHash = (Get-FileHash -LiteralPath $runExe -Algorithm SHA256).Hash
>     $postDllHash = (Get-FileHash -LiteralPath $runDll -Algorithm SHA256).Hash
>     if ($preExeHash -cne $postExeHash -or $preDllHash -cne $postDllHash) {
>         throw 'Binary-reference Run artifacts changed during the relaunch gate.'
>     }
>     $sourceDllHash = (Get-FileHash -LiteralPath $sourceDll -Algorithm SHA256).Hash
>     if ($postDllHash -cne $sourceDllHash) {
>         throw 'Binary-reference Run SDK is not byte-identical to the canonical Release SDK.'
>     }
>     $candidateXml = [xml](Get-Content -LiteralPath $candidateProject -Raw)
>     [pscustomobject]@{
>         Result = 'PASS'
>         CandidateProjectReferenceCount = @($candidateXml.Project.ItemGroup.ProjectReference).Count
>         RunExeBytes = (Get-Item -LiteralPath $runExe).Length
>         RunExeSha256 = $postExeHash
>         RunDllBytes = (Get-Item -LiteralPath $runDll).Length
>         RunDllSha256 = $postDllHash
>         ConfigState = if (Test-Path -LiteralPath ($runExe + '.config')) { 'PRESENT' } else { 'ABSENT_TO_ABSENT_PASS' }
>     } | Format-List
> }
> finally {
>     if (Test-Path -LiteralPath $candidateRoot) {
>         $resolvedRoot = [System.IO.Path]::GetFullPath($candidateRoot).TrimEnd('\')
>         if (-not $resolvedRoot.StartsWith($requiredPrefix,[System.StringComparison]::Ordinal) -or ((Get-Item -LiteralPath $resolvedRoot -Force).Attributes -band [System.IO.FileAttributes]::ReparsePoint)) {
>             throw "Refusing candidate cleanup outside verified temp root: $resolvedRoot"
>         }
>         foreach ($entry in Get-ChildItem -LiteralPath $resolvedRoot -Recurse -Force) {
>             $entryFull = [System.IO.Path]::GetFullPath($entry.FullName)
>             if (-not $entryFull.StartsWith(($resolvedRoot + '\'),[System.StringComparison]::OrdinalIgnoreCase) -or ($entry.Attributes -band [System.IO.FileAttributes]::ReparsePoint)) {
>                 throw "Refusing candidate cleanup with escaped/reparse entry: $entryFull"
>             }
>         }
>         Remove-Item -LiteralPath $resolvedRoot -Recurse -Force
>     }
>     if (Test-Path -LiteralPath $candidateRoot) { throw "Candidate temp residue remains: $candidateRoot" }
> }`
>
> </details>
>
> 전체 candidate 검증을 보완하는 첫 시도는 안전 정책이 동적 함수 로딩과 재귀 정리를 한 명령에 묶은 형태를 거부해 실제 작업을 시작하지 않았습니다. 더 단순하게 고정된 workspace 임시 경로에 현재 WPF source 목록만 복사하고, distribution과 동일한 binary reference 변환을 적용해 빌드하겠습니다. 정리는 별도 명령에서 절대 경로를 다시 확인한 뒤 수행합니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$ErrorActionPreference = 'Stop'
> $repo = 'C:\work\Elmo\Elmo_Master'
> $tempRoot = 'C:\work\Elmo\Elmo_Master\.codex_wpf_binary_candidate_20260811'
> if (Test-Path -LiteralPath $tempRoot) { throw "Fixed candidate root already exists: $tempRoot" }
> $sourceRoot = Join-Path $repo 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp'
> $sourceProject = Join-Path $sourceRoot 'LasalApiWpfTestApp.csproj'
> $sourceDll = Join-Path $repo 'LMC_Library\LMC_API_Delivery\src\bin\Release\LasalMotionControlLib.dll'
> $smokeProject = Join-Path $repo 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\LasalApiWpfTestApp.SmokeTests.csproj'
> $msbuild = 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe'
> $apiDirectory = Join-Path $tempRoot '01_API'
> $exampleProgram = Join-Path $tempRoot '02_Example_Program'
> $exampleRoot = Join-Path $exampleProgram 'LasalApiWpfTestApp'
> $runDirectory = Join-Path $exampleProgram 'Run'
> New-Item -ItemType Directory -Path @($apiDirectory,$exampleRoot,$runDirectory) -Force | Out-Null
> Copy-Item -LiteralPath $sourceDll -Destination (Join-Path $apiDirectory 'LasalMotionControlLib.dll')
> [xml]$projectXml = Get-Content -LiteralPath $sourceProject -Raw
> $ns = New-Object System.Xml.XmlNamespaceManager($projectXml.NameTable)
> $ns.AddNamespace('m',$projectXml.Project.NamespaceURI)
> $sourceNodes = $projectXml.SelectNodes('/m:Project/m:ItemGroup/m:ApplicationDefinition | /m:Project/m:ItemGroup/m:Page | /m:Project/m:ItemGroup/m:Compile',$ns)
> foreach ($node in @($sourceNodes)) {
>     $relative = [string]$node.GetAttribute('Include')
>     if ([string]::IsNullOrWhiteSpace($relative) -or [System.IO.Path]::IsPathRooted($relative) -or $relative.Contains('..')) {
>         throw "Unsafe project source path: $relative"
>     }
>     $source = [System.IO.Path]::GetFullPath((Join-Path $sourceRoot $relative))
>     $destination = [System.IO.Path]::GetFullPath((Join-Path $exampleRoot $relative))
>     if (-not $source.StartsWith(($sourceRoot.TrimEnd('\') + '\'),[System.StringComparison]::OrdinalIgnoreCase) -or -not $destination.StartsWith(($exampleRoot.TrimEnd('\') + '\'),[System.StringComparison]::OrdinalIgnoreCase)) {
>         throw "Project source escaped its root: $relative"
>     }
>     New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
>     Copy-Item -LiteralPath $source -Destination $destination
> }
> $projectReference = $projectXml.SelectSingleNode('//m:ProjectReference[@Include="..\..\LMC_API_Delivery\src\LasalMotionControlLib.csproj"]',$ns)
> if ($null -eq $projectReference) { throw 'Expected development ProjectReference not found.' }
> $referenceGroup = $projectXml.SelectSingleNode('/m:Project/m:ItemGroup[m:Reference]',$ns)
> if ($null -eq $referenceGroup) { throw 'Reference ItemGroup not found.' }
> $null = $projectReference.ParentNode.RemoveChild($projectReference)
> $namespaceUri = $projectXml.Project.NamespaceURI
> $reference = $projectXml.CreateElement('Reference',$namespaceUri)
> $reference.SetAttribute('Include','LasalMotionControlLib')
> $hintPath = $projectXml.CreateElement('HintPath',$namespaceUri)
> $hintPath.InnerText = '..\..\01_API\LasalMotionControlLib.dll'
> $private = $projectXml.CreateElement('Private',$namespaceUri)
> $private.InnerText = 'True'
> $null = $reference.AppendChild($hintPath)
> $null = $reference.AppendChild($private)
> $null = $referenceGroup.AppendChild($reference)
> $candidateProject = Join-Path $exampleRoot 'LasalApiWpfTestApp.csproj'
> $settings = New-Object System.Xml.XmlWriterSettings
> $settings.Encoding = New-Object System.Text.UTF8Encoding($false)
> $settings.Indent = $true
> $settings.NewLineChars = [Environment]::NewLine
> $settings.NewLineHandling = [System.Xml.NewLineHandling]::Replace
> $writer = [System.Xml.XmlWriter]::Create($candidateProject,$settings)
> try { $projectXml.Save($writer) } finally { $writer.Dispose() }
> [xml]$candidateXml = Get-Content -LiteralPath $candidateProject -Raw
> $candidateNs = New-Object System.Xml.XmlNamespaceManager($candidateXml.NameTable)
> $candidateNs.AddNamespace('m',$candidateXml.Project.NamespaceURI)
> if ($candidateXml.SelectNodes('//m:ProjectReference',$candidateNs).Count -ne 0) { throw 'Candidate retained ProjectReference.' }
> $binaryReference = $candidateXml.SelectSingleNode('//m:Reference[@Include="LasalMotionControlLib"]',$candidateNs)
> if ($null -eq $binaryReference -or $binaryReference.HintPath -ne '..\..\01_API\LasalMotionControlLib.dll' -or $binaryReference.Private -ne 'True') {
>     throw 'Candidate binary reference contract is invalid.'
> }
> $candidateBuildArgs = @($candidateProject,'/t:Rebuild','/p:Configuration=Release','/p:Platform=AnyCPU','/nologo','/verbosity:minimal')
> & $msbuild @candidateBuildArgs
> if ($LASTEXITCODE -ne 0) { throw 'Binary-reference candidate Release build failed.' }
> $releaseOutput = Join-Path $exampleRoot 'bin\Release'
> $candidateExe = Join-Path $releaseOutput 'LasalMotionControlApiExample.exe'
> $candidateDll = Join-Path $releaseOutput 'LasalMotionControlLib.dll'
> foreach ($artifact in @($candidateExe,$candidateDll)) {
>     if (-not (Test-Path -LiteralPath $artifact -PathType Leaf)) { throw "Candidate output missing: $artifact" }
>     Copy-Item -LiteralPath $artifact -Destination $runDirectory
> }
> $candidateConfig = $candidateExe + '.config'
> if (Test-Path -LiteralPath $candidateConfig -PathType Leaf) { Copy-Item -LiteralPath $candidateConfig -Destination $runDirectory }
> $runExe = Join-Path $runDirectory 'LasalMotionControlApiExample.exe'
> $runDll = Join-Path $runDirectory 'LasalMotionControlLib.dll'
> $preExeHash = (Get-FileHash -LiteralPath $runExe -Algorithm SHA256).Hash
