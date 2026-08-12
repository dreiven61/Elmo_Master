>
> - Ran `rg --files LMC_Library | rg "(?i)(user.*manual.*\.(docx|pdf)|api.*manual.*\.(docx|pdf))$"`
> - Ran `$py = Join-Path $env:USERPROFILE '.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; $docx = (Resolve-Path 'LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.docx').Path; $pdf = (Resolve-Path 'LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf').Path; & $py -c "import sys,importlib.metadata as m; baseline=set(sys.modules); from docx import Document; from pypdf import PdfReader; d=Document(sys.argv[1]); r=PdfReader(sys.argv[2]); _=(len(d.paragraphs),len(d.tables),len(r.pages)); mp=m.packages_distributions(); roots={'python-docx','pypdf'}; print('\n'.join(sorted({x for n in set(sys.modules)-baseline for x in (mp.get(n.split('.')[0]) or [])}-roots,key=str.casefold)))" $docx $pdf`
> - Ran `$py = Join-Path $env:USERPROFILE '.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; & $py -c "import importlib.metadata as m; names=['cffi','cryptography','lxml','pillow','typing_extensions']; print('\n'.join(f'{n}|{m.version(n)}|{len(m.distribution(n).files)}' for n in names))"`
>
> </details>
>
> 실제 배포 DOCX/PDF를 열어 본 workload에서도 같은 5개만 새로 적재됐습니다. DOCX 경로는 `lxml`·`typing_extensions`, PDF 경로는 `cryptography`·`Pillow`·`cffi`입니다. `pycparser`는 설치 의존성이지만 이 실행 경로에서는 로드되지 않아 이번 활성 closure에서 제외합니다.
>
> 문서 동결 직후 한 가지를 바로잡고 있습니다. 다음 공백 목록에 `cffi`가 빠져 있었으므로 실제 workload 결과에 맞춰 5개(`lxml`, `typing_extensions`, `cryptography`, `Pillow`, `cffi`)로 보정하고, 로드되지 않은 `pycparser`는 제외한다고 명시합니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$docs=@('LMC_Library/LMC_API/README.md','docs/architecture/LMC_API_TRANSACTIONAL_DISTRIBUTION_CANDIDATE_2026-07-31.md','LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md','docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md','docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md'); Get-FileHash -Algorithm SHA256 $docs | ForEach-Object { '{0}  {1}' -f $_.Hash,$_.Path }; git diff --check -- $docs; git diff --stat -- $docs; rg -n "1b9be6a|14/14|831331|F2B6DE0|CE3D330|C3219FED|cffi|pycparser|39c3e6f|12/12" $docs`
>
> </details>
>
> 다음 구현 범위도 확정했습니다. 더 높은 PC-only blocker는 없고, 현재 8-role을 13-role로 늘려 활성 5개 배포판을 각각 전체 파일 inventory로 묶습니다. 동시에 발견된 fail-closed 결함—Python·Roslyn 역할에 full inventory가 없어도 단일 파일 hash로 통과하는 경로—도 같은 tranche에서 막겠습니다.
>
> 문서 독립 검토는 현재까지 finding 없이 진행 중이며, 14/14 계약과 정확한 5개 Python closure 경계를 마지막으로 대조하고 있습니다. 코드 쪽은 문서와 겹치지 않는 5개 파일에서 13-role 구현을 시작했습니다.
>
> 문서 독립 리뷰도 CLEAN입니다. 이제 정확한 5개 Markdown만 커밋합니다. 이 커밋은 14/14 PC/tooling 증거를 기록할 뿐이며, full Distribution·현재 schema3 후보 manifest·actual EXE·PLC 검증으로 확대해서 쓰지 않습니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `$docs=@('LMC_Library/LMC_API/README.md','LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md','docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md','docs/architecture/LMC_API_TRANSACTIONAL_DISTRIBUTION_CANDIDATE_2026-07-31.md','docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md'); git diff --check -- $docs; git add -- $docs; git diff --cached --name-only; git diff --cached --check`
> - Ran `git commit -m "docs(api): record mandatory provenance suite"; git show --stat --oneline --summary HEAD; git diff-tree --no-commit-id --name-only -r HEAD; git status --short`
>
> </details>
>
> 코드 `1b9be6a`와 문서 `4867096`까지 각각 분리 커밋했습니다. 현재 작업트리에는 사용자 `Classes.lcb`와 기존 미추적 자료만 남아 있고, 다음 13-role Python 활성 의존성 구현을 진행 중입니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short | Select-Object -First 20`
>
> </details>
>
> 13-role 구현은 핵심 resolver부터 시작됐고 현재 `DistributionToolchainProvenance.ps1` 한 파일만 편집된 초기 단계입니다. 아직 테스트 수치나 PASS를 주장할 단계는 아니며, exact 5파일 범위를 유지합니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `git diff --stat -- LMC_Library/LMC_API/DistributionToolchainProvenance.ps1; git diff -- LMC_Library/LMC_API/DistributionToolchainProvenance.ps1 | Select-Object -First 260`
> - Ran `git diff -- LMC_Library/LMC_API/DistributionToolchainProvenance.ps1 | Select-Object -Skip 250 -First 310`
>
> </details>
>
> 핵심 구현은 AST 오류 없이 들어갔습니다. Python probe는 기준 모듈 집합을 먼저 잡고 실제 `Document`/`PdfReader` import 이후 delta를 계산하며, 정확한 owner 7개와 각 imported module의 배포판 소유권을 확인합니다. 현재는 누락·추가·변조가 실제로 차단되는 테스트 fixture를 채우는 단계입니다.
>
> 독립 조기 리뷰에서 실제 결함을 잡았습니다. `cffi` 배포 inventory에 `site-packages` 밖의 `Scripts/cffi-gen-src.exe`가 포함되어 기존 상대경로 방식이면 정상 번들 Python까지 거부됩니다. 전체 53파일을 유지하되 공통 Python root 기준 경로로 정규화하고, root 밖 escape만 거부하도록 수정합니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `rg -n "GetRelativePath|MakeRelativeUri|RelativePath|repository prefix|StartsWith\(" LMC_Library/LMC_API/DistributionToolchainProvenance.ps1 | Select-Object -First 80`
> - Ran `Get-Content LMC_Library/LMC_API/DistributionToolchainProvenance.ps1 | Select-Object -Skip 190 -First 190`
>
> </details>
>
> 조기 결함 수정 방향은 명확합니다. 모든 Python package 파일을 `sys.base_prefix` 아래의 물리 경로로 재해석해 `Lib/site-packages/...`와 `Scripts/...`를 한 inventory에 포함시키고, 그 공통 root 밖으로 나가는 항목만 fail-closed합니다.
>
> 두 번째 정확성 문제도 잡았습니다. 기본 Python runtime이 `Scripts` 전체를 해시하면 무관한 패키지의 console script 변경까지 provenance가 흔들립니다. 기본 role에서는 `Lib/site-packages`와 `Scripts`를 제외하고, 활성 배포판이 실제 소유한 script만 해당 role inventory로 다시 포함하도록 수정합니다.
>
> 호환성도 보강합니다. PATH의 Python 3.8에는 `packages_distributions()`가 없어 새 probe만으로는 기존 fallback이 깨집니다. Python 버전을 새로 고정하지 않고, 배포 metadata 파일과 실제 로드 모듈 경로의 소유권을 대조하는 호환 방식으로 3.8/3.12 모두 유지합니다.
>
> 세 finding은 모두 코드에 반영됐습니다. 특히 active owner 판정은 이제 설치 배포판의 물리 파일 목록으로 수행하므로 Python 3.8에도 동작하고, `cffi-gen-src.exe`는 cffi role에만 포함되며 무관한 `Scripts` 변경은 기본 Python hash에 영향을 주지 않습니다. 이제 fixture와 첫 집중 실행을 마무리합니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$py=Join-Path $env:USERPROFILE '.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; & $py -B -c "import importlib.metadata as m; d=m.distribution('cffi'); print('\n'.join(str(x) for x in d.files))"`
>
> </details>
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `rg -n "Invoke-LmcPython|PythonPath|python|Arguments @\('-c'|'-c'," LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/DistributionSemanticPolicy.ps1`
> - Ran `Get-Content LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 | Select-Object -Skip 1395 -First 70; Get-Content LMC_Library/LMC_API/DistributionSemanticPolicy.ps1 | Select-Object -Skip 155 -First 55`
>
> </details>
>
> 활성 배포판 inventory에는 `.pyc`도 포함돼 있어 자체 실행이 hash를 바꾸지 않도록 처리합니다. provenance probe와 실제 PDF/DOCX 검사·semantic extraction 모두 `-B`로 실행하고, `.pyc` 파일 자체는 inventory에서 빼지 않습니다. 이 변경 때문에 코드 범위는 관련 실행/테스트 파일까지 좁게 확장됩니다.
>
> 소유권 우회 가능성도 막습니다. 새로 로드된 namespace/ownerless 모듈을 조용히 무시하지 않고, 표준 라이브러리·built-in/frozen인지 검증합니다. 외부 `site-packages`/PYTHONPATH namespace는 거부하고, cryptography가 만드는 무경로 runtime 모듈만 이름 패턴을 좁게 허용합니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short | Where-Object { $_ -match 'LMC_Library/LMC_API' } | Select-Object -First 20; git diff --stat -- LMC_Library/LMC_API/DistributionToolchainProvenance.ps1 LMC_Library/LMC_API/Test-LmcDistributionToolchainProvenance.ps1 LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1 LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/DistributionSemanticPolicy.ps1 LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1`
>
> </details>
>
> 현재 4개 핵심/테스트 파일에 약 1천 줄 규모의 fail-closed fixture가 들어갔고, `-B` 실행 경로 3개는 아직 반영 중입니다. 시스템 Python 3.8은 API 호환 probe는 되지만 실제로 추가 `numpy`를 로드하므로 exact 7-owner 정책상 의도적으로 후보 거부됩니다.
>
> production 코드에는 13-role, cffi script 정규화, 기본 Scripts 제외, Python 3.8-compatible 소유권 판정, 세 실행 경로 `-B`, ownerless 모듈 검증까지 모두 반영됐습니다. 지금은 이 경계들이 실제로 빠지면 테스트가 실패하도록 non-vacuous fixture를 완성 중입니다.
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `rg -n "ownerless|namespace|synthetic|_openssl|cython_runtime|_cython|Scripts|ExcludedRelativePrefixes|Normalized|base_prefix|BasePrefix|metadata.distributions|packages_distributions" LMC_Library/LMC_API/DistributionToolchainProvenance.ps1 LMC_Library/LMC_API/Test-LmcDistributionToolchainProvenance.ps1`
> - Ran `Get-Content LMC_Library/LMC_API/DistributionToolchainProvenance.ps1 | Select-Object -Skip 1040 -First 175; Get-Content LMC_Library/LMC_API/DistributionToolchainProvenance.ps1 | Select-Object -Skip 1350 -First 170`
> - Ran `Get-Content LMC_Library/LMC_API/DistributionToolchainProvenance.ps1 | Select-Object -Skip 1200 -First 155`
> - Ran `Get-Content LMC_Library/LMC_API/DistributionToolchainProvenance.ps1 | Select-Object -Skip 990 -First 80`
>
> </details>
>
> 실제 번들 Python으로 resolver를 직접 통과시켜 Python 쪽 8개 descriptor(기본 Python + root 2 + 활성 dependency 5)가 생성되는 것까지 확인됐습니다. 테스트 fixture에는 built-in/frozen·표준 Lib/DLL 허용과 site-packages namespace·외부 경로 거부를 각각 독립 고정하고 있습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short | Where-Object { $_ -match 'LMC_Library/LMC_API' }`
>
> </details>
>
> 테스트 자체의 이식성도 다듬고 있습니다. PATH에 Python 3.8이 없다고 실패하지 않게 하고, 있을 때만 “추가 numpy 때문에 controlled reject”를 확인합니다. 또 활성 package fixture에 실제 `__pycache__/*.pyc`를 넣어 bytecode 변조가 hash를 바꾸는지 비공허하게 검증합니다.
>
> 또 하나의 실제 문제를 잡았습니다. ownerless 검증이 cryptography 경로를 `Lib\site-packages\cryptography`로 하드코딩해 유효한 다른 설치 레이아웃과 fixture를 깨뜨렸습니다. 이 하드코딩은 제거하고, 이미 존재하는 “PythonCryptography role + imported module이 full inventory에 포함” 계약으로 소유권을 고정합니다.
>
> 첫 PS5.1 provenance 집중 검사는 83 assertions로 PASS했습니다. 다만 방금 발견한 이식성/pyc fixture 보정을 반영하면 수치가 바뀌므로 이 결과는 중간 체크포인트일 뿐입니다. 수정 후 PS5·PS7 전체 focused matrix를 다시 실행해 최종값만 기록합니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git diff --stat -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/DistributionSemanticPolicy.ps1 LMC_Library/LMC_API/DistributionToolchainProvenance.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1 LMC_Library/LMC_API/Test-LmcDistributionToolchainProvenance.ps1 LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1 LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1`
>
> </details>
>
> 현재 diff는 production 3파일과 테스트 4파일, 총 7파일입니다. 약 1,380줄의 대부분은 13-role/active-owner fail-closed fixture이며, HostParity의 최종 evidence count는 focused 수치가 동결된 뒤 마지막으로 갱신합니다.
>
> 독립 reviewer가 현재 바이트로 PS7 provenance `84/84 PASS`를 확인했습니다. cffi root 정규화, ownerless 거부, pyc 변조, optional legacy 거부, 실제 13-role resolution과 `-B` 이후 no-drift까지 포함한 결과입니다. 나머지 manifest·pipeline·semantic과 PS5 재실행이 남았습니다.
>
> 추가 독립 실행도 통과했습니다: ReleaseManifest `108/108`, SemanticPolicy `52 tests / 18 policy checks`, provenance는 양 호스트 `84/84`입니다. 이제 pipeline transaction fixture와 HostParity count를 맞춘 뒤 전체 matrix를 다시 동결합니다.
>
> pipeline fixture에서 즉시 실패하는 테스트 버그를 찾았습니다. 정규식 문자열 안의 `$probeCode`가 StrictMode에서 변수로 확장된 문제이며 production 코드 결함은 아닙니다. 단일 인용 literal로 고치고 양 호스트 pipeline을 다시 돌립니다.
>
> pipeline regex를 고친 뒤 PS7과 PS5.1 모두 `291/291 PASS`했습니다. 현재 동결 후보 수치는 Pipeline 291, SemanticPolicy 52/18, Manifest 108, Toolchain 84이며 HostParity가 이 정확한 증거를 요구하도록 갱신 중입니다.
>
> HostParity 내부에 SemanticPolicy의 예전 `50` 조건이 한 곳 남아 있어 전체 gate가 실패할 상태였습니다. 이를 `52`로 맞추고, 이전 수치 `49/50/100/286` 잔여를 전수 검색한 뒤 각 worker를 독립 실행합니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `rg -n "(49|50|100|286) (distribution toolchain|distribution pipeline|release manifest)|TestCount -ne (49|50|100|286)|PASS: (49|50|100|286)|Expected (49|50|100|286)" LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 LMC_Library/LMC_API/Test-LmcDistributionToolchainProvenance.ps1 LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1 LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1`
> - Ran `rg -n "ToolchainProvenance|Pipeline|SemanticPolicy|ReleaseManifest|TestCount -ne" LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1 | Select-Object -First 120`
> - Ran `Get-Content LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1 | Select-Object -Skip 600 -First 110; Get-Content LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1 | Select-Object -Skip 1048 -First 36`
>
> </details>
>
> 양 호스트 focused matrix는 모두 통과했습니다: provenance 84, manifest 108, pipeline 291, semantic 52/check 18. 현재는 장시간 aggregate 전에 7개 worker를 PS5/PS7 각각 독립 실행해 evidence 문자열과 worker 종료 계약까지 확인 중입니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$files=@('LMC_Library/LMC_API/Build-LmcApiDistribution.ps1','LMC_Library/LMC_API/DistributionSemanticPolicy.ps1','LMC_Library/LMC_API/DistributionToolchainProvenance.ps1','LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1','LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1','LMC_Library/LMC_API/Test-LmcDistributionToolchainProvenance.ps1','LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1','LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1'); git diff --check -- $files; git diff --stat -- $files`
>
> </details>
>
> 현재 변경 범위는 정확히 8개 파일이며 scoped `git diff --check`를 통과했습니다. 독립 리뷰의 stale-count/AST 검사도 깨끗하고, 마지막으로 owner mapping·inventory 누락 우회가 없는지 core bypass scan을 진행 중입니다.
>
> 격리 worker에서 PS5.1은 7/7 모두 통과했습니다. PS7은 테스트 시작 전, `Get-Command pwsh`가 실제 실행파일과 alias 두 경로를 반환해 임시 실행 스크립트가 문자열을 합친 문제였습니다. 제품 코드 실패는 아니며, 검증된 물리 pwsh 경로 하나로 PS7 7개만 재실행합니다.
>
> PS7 재실행은 물리 실행파일 경로를 하나로 고정해 진행 중입니다. 이 재실행도 Pipeline→Semantic→Manifest→Provenance→MethodSize→UDP→Control 순서이며, UDP 구간 때문에 수분이 걸립니다.
>
> PS7 worker는 오류 없이 실행 중이고, 경과시간상 앞의 5개 suite를 지나 긴 UDP self-test 구간으로 보입니다. 이후 ControlHandleRequest 하나만 남습니다. 파일은 동결 상태입니다.
>
> PS7 UDP 검증은 계속 계산 중이며 타임아웃 범위 안입니다. 이 단계는 이전 실측상 약 8분 걸리므로 현재 무출력은 정상입니다. 완료되기 전에는 최종 PASS로 고정하지 않습니다.
>
> 같은 PS7 UDP 구간이 진행 중입니다. 현재까지 테스트 프로세스 종료나 오류 신호는 없고, full Distribution·PLC·IDE는 여전히 실행하지 않습니다.
>
> PS7 UDP PID를 확인했고 `Responding=True`, CPU 누적 246초로 실제 연산 중입니다. hang이 아니며, 완료 후 ControlHandleRequest만 실행됩니다.
>
> PS7 UDP 검사는 계속 정상 진행 중입니다. 900초 per-suite 제한과 프로세스 트리 종료 계약이 적용돼 있으므로 실제 정지라면 PASS가 아니라 fail-closed로 끝납니다.
>
> 현재도 마지막 장시간 UDP 검증 구간입니다. 동결 바이트와 사용자 파일 상태는 그대로이며, 결과가 나오면 바로 7/7 및 잔류 프로세스 0을 확인합니다.
>
> UDP worker는 아직 전환 전이지만 `Responding=True`, CPU 467초로 정상 계산 중입니다. 이전 정상 PS7 소요와 거의 같은 구간이라 완료가 임박한 것으로 보입니다.
>
> 격리 matrix가 최종 통과했습니다: PS5 7/7 + PS7 7/7. PS7 시간은 Pipeline 24.2s, Semantic 9.3s, Manifest 2.5s, Provenance 45.9s, Method 1.0s, UDP 503.9s, Control 5.2s입니다. 이제 같은 동결 바이트로 mandatory PS5-parent 14/14 aggregate를 실행합니다.
>
> 최종 aggregate는 PS5의 7개 suite부터 순차 실행 중입니다. 동일한 suite를 방금 격리 실행으로 통과했지만, 실제 mandatory orchestrator의 host attestation·digest·evidence 결합까지 다시 확인하는 별도 검증입니다.
>
> mandatory aggregate는 PS5의 Pipeline·Semantic·Manifest·Provenance·MethodSize를 지나 현재 PS5 UDP callback을 실행 중입니다. 프로세스는 `Responding=True`이며, 이후 PS5 Control과 PS7 7개가 남습니다.
>
> PS5 UDP 검사는 정상 계산 중입니다. 이 self-test는 PC 정적 계약 296개를 검사하며 PLC나 외부 네트워크에는 접근하지 않습니다. 결과가 나오기 전에는 다음 host로 넘어가지 않습니다.
>
> 현재도 PS5 UDP 구간이며 예상 소요 범위 안입니다. 파일은 동결됐고 이번 aggregate 중 새 temp·candidate·manifest를 만들지 않습니다.
>
> PS5 7/7이 완료됐고 PS7로 전환했습니다. 현재 PS7 Pipeline 내부 ReleaseManifest fixture가 실행 중입니다. PS7의 나머지 6개 suite까지 완료돼야 최종 14/14입니다.
>
> PS7 초반 suite가 계속 진행 중입니다. Pipeline은 여러 transaction/manifest negative fixture를 포함하므로 단순 스크립트 한 번보다 길지만, 격리 실행에서 291/291을 이미 통과한 동일 바이트입니다.
>
> PS7도 Pipeline·SemanticPolicy·ReleaseManifest·ToolchainProvenance·MethodSize를 모두 마쳤고, 현재 UDP callback만 실행 중입니다. 이후 ControlHandleRequest 하나만 남습니다.
>
> PS7 UDP worker는 계속 정상 계산 중입니다. 이 단계가 가장 길며 이전 격리 실측 503.9초와 비교해 아직 정상 범위입니다.
>
> 아직 PS7 UDP 구간입니다. 900초 제한과 프로세스 트리 정리 계약이 적용돼 있어 결과가 명확히 PASS 또는 FAIL로 끝나도록 되어 있습니다.
>
> 동결 상태와 테스트 프로세스가 유지되고 있습니다. 완료 즉시 마지막 Control 검사와 attestation/toolchain digest를 검증합니다.
>
> PS7 UDP는 `Responding=True`, CPU 343초로 계속 연산 중입니다. 정지 징후 없이 이전 정상 소요 범위 안입니다.
>
> PS7 UDP 검증이 계속 진행 중입니다. 이번 tranche의 Python provenance 변경과 무관한 기존 296개 LASAL callback 정적 계약도 함께 회귀 확인하는 단계입니다.
>
> 아직 같은 구간이며 오류는 없습니다. 완료 뒤 ControlHandleRequest가 약 수초 소요되고 최종 14/14 terminal line이 나옵니다.
>
> mandatory aggregate가 성공했습니다: Windows PowerShell 5.1 부모에서 `14/14`, PS5=7/7·PS7=7/7, files=94, tooling digest `F687FDE9…CDB4CA`입니다. 이제 검출된 worker 1개와 Temp pipeline 디렉터리 2개가 현재 실행 잔류인지 기존 잔류인지 구분한 뒤 최종 증거를 고정합니다.
>
> 최종 증거가 모두 고정됐습니다. Toolchain은 13 records, SHA `91E56793…F7EB`, 활성 dependency inventory는 cffi53·cryptography195·lxml208·Pillow219·typing_extensions7입니다. worker·현재-run temp·stage·lock 잔류는 0입니다. 제가 8파일 SHA를 독립 재확인한 뒤 정확히 그 8개만 커밋합니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `$files=@('LMC_Library/LMC_API/Build-LmcApiDistribution.ps1','LMC_Library/LMC_API/DistributionSemanticPolicy.ps1','LMC_Library/LMC_API/DistributionToolchainProvenance.ps1','LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1','LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1','LMC_Library/LMC_API/Test-LmcDistributionToolchainProvenance.ps1','LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1','LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1'); Get-FileHash -Algorithm SHA256 $files | ForEach-Object { '{0}  {1}' -f $_.Hash,$_.Path }; git diff --check -- $files; git diff --cached --check -- $files; git diff --stat -- $files; git diff --name-only -- $files`
> - Ran `git add -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/DistributionSemanticPolicy.ps1 LMC_Library/LMC_API/DistributionToolchainProvenance.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1 LMC_Library/LMC_API/Test-LmcDistributionToolchainProvenance.ps1 LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1 LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1; git diff --cached --name-only; git diff --cached --check`
> - Ran `git commit -m "feat(distribution): bind active Python dependencies"; git show --stat --oneline --summary HEAD; git diff-tree --no-commit-id --name-only -r HEAD; git status --short`
>
> </details>
>
> 13-role 구현을 `3c63dea`로 정확히 8파일 커밋했습니다. 사용자 LASAL 파일은 그대로입니다. 이제 5개 설계/상태 문서를 새 digest와 13-role 기준으로 갱신하는 동시에, Gate D 전에 남은 더 높은 PC-only 공백이 있는지 다시 감사합니다.
>
> 문서 쪽은 8-role `1b9be6a`를 역사적 선행으로 남기고, 13-role `3c63dea`를 current로 교체 중입니다. 다음-gap 감사에서는 full Distribution을 다시 돌리기 전에 추가로 닫아야 할 PC-only fail-open이 있는지 우선 확인합니다.
>
> 다음 감사에서 중요한 운영 경계를 확인 중입니다. Gate D만 승인해도 clean release가 바로 되지는 않습니다. 현재 Git에 추적된 기본 manual은 1.9이고, 검토 완료된 2.3 DOCX/PDF는 ignored 비정규 입력이라 `-AllowDirty`/`dirty-preview`로만 빌드됩니다. 어떤 경로를 canonical로 승격할지 먼저 닫아야 합니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `git ls-files | rg "(?i)\.(docx|pdf)$" | rg "(?i)(manual|user|api)"; Get-FileHash -Algorithm SHA256 LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.docx,LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf | Format-Table Hash,Path -AutoSize; rg -n "ManualInput|Manual.*Path|1\.9|2\.3|A23211|9E82A4|FF77|9069" LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/DistributionPipeline.ps1 LMC_Library/LMC_API/README.md`
> - Ran `$paths=@('output\doc\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx','output\pdf\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf'); foreach($p in $paths){ if(Test-Path -LiteralPath $p -PathType Leaf){$h=Get-FileHash -Algorithm SHA256 -LiteralPath $p; '{0}|{1}|{2}' -f $h.Hash,(Get-Item -LiteralPath $p).Length,$h.Path}else{'MISSING|'+$p}}; git check-ignore -v $paths`
> - Ran `rg -n "FF77F1|90698E|A23211|9E82A4|1\.9|2\.3-candidate|canonical manual|Canonical Distribution" LMC_Library/LMC_API LMC_Library/LMC_API_Delivery/tests docs --glob '*.ps1' --glob '*.md' --glob '*.txt' | Select-Object -First 260`
>
> </details>
>
> 정규 manual 상태를 직접 확인했습니다. 추적된 canonical 파일은 여전히 1.9(SHA `FF77…`/`9069…`), 검토 완료 2.3은 ignored 출력(SHA `A232…`/`9E82…`)입니다. 다음 단계는 이 두 검토본을 canonical tracked 입력으로 승격할지에 대한 정적 승인 계약을 닫는 것입니다.
>
> 이제 실제 DOCX/PDF 승격을 다루므로 `doc`와 `pdf` 검증 지침을 적용하겠습니다. 두 skill의 렌더·검증 절차를 먼저 읽고, 이미 승인된 artifact를 그대로 복사해도 최종 canonical 바이트를 다시 확인합니다.
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `Get-Content -Raw 'C:\Users\dreiv\.codex\skills\doc\SKILL.md'`
> - Ran `Get-Content -Raw 'C:\Users\dreiv\.codex\skills\pdf\SKILL.md'`
> - Ran `Get-Command pdftoppm,soffice -ErrorAction SilentlyContinue | Select-Object Name,Source; Get-ChildItem -Path C:\Users\dreiv\.codex\skills\doc\scripts -ErrorAction SilentlyContinue | Select-Object Name,FullName`
> - Ran `Get-Content -Raw 'C:\Users\dreiv\.codex\skills\doc\scripts\render_docx.py'`
>
> </details>
>
> 감사 결론은 명확합니다. 새 pipeline 코드 결함은 없지만 clean release의 기본 manual이 1.9라 현재 semantic policy를 통과할 수 없습니다. 검토 완료된 2.3 두 파일을 dedicated artifact transition으로 canonical 경로에 승격하겠습니다. 기존 1.9는 Git 이력에서 복구 가능하며, 정확히 두 tracked binary만 바꿉니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$root=(Resolve-Path '.').Path
> $srcDoc=(Resolve-Path 'output\doc\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx').Path
> $srcPdf=(Resolve-Path 'output\pdf\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf').Path
> $dstDoc=(Resolve-Path 'LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.docx').Path
> $dstPdf=(Resolve-Path 'LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf').Path
> foreach($p in @($srcDoc,$srcPdf,$dstDoc,$dstPdf)){if(-not $p.StartsWith($root+'\',[StringComparison]::OrdinalIgnoreCase)){throw "Path escaped workspace: $p"}}
> $expected=@{$srcDoc='A23211A5F530736E6BDC8746DCA1DF4556C47E08524828A7ADB70DC8C91C3182';$srcPdf='9E82A467C1BEC2FC3FE20AF1EE8D1332C66D07617CAB2D512C744357C5C28E70'}
> foreach($p in @($srcDoc,$srcPdf)){if((Get-FileHash -LiteralPath $p -Algorithm SHA256).Hash -cne $expected[$p]){throw "Source hash drift: $p"}}
> Copy-Item -LiteralPath $srcDoc -Destination $dstDoc -Force
