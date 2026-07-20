> ```
>
> ```text
> window is minimized; call activate_window, refresh with get_window, then retry get_window_state
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.activate_window({window:baseWindow});globalThis.baseWindow=await sky.get_window({id:baseWindow.id,app:baseWindow.app});globalThis.baseFresh2=await sky.get_window_state({window:baseWindow});globalThis.baseWindow=baseFresh2.window;",
>   "title": "기준 프로젝트 화면 복원"
> }
> ```
>
> ```text
> user input was detected in this window; call get_window_state before continuing
> ```
>
> </details>
>
> 원인을 거의 특정했습니다. `_Edit`에서 검색이 단순히 “0건”으로 끝나는 것이 아니라 LASAL 내부 로그에 매번 `CInvalidArgException`이 발생합니다. 그리고 실패 대상 `TCPMotionInterface.st`에는 기준 프로젝트에 없는 UTF-8 한글 주석이 들어 있습니다. 검색기가 문자열 위치를 색칠할 때 바이트 위치와 문자 위치가 어긋나는 패턴과 정확히 맞는지 추가 검증하겠습니다.
</details>

결론부터 말하면, 이 스레드에서 수정·커밋한 LASAL 프로젝트는 `Elmo_EtherCAT_Test_4Axis`입니다. `_Edit`가 아닙니다.

### 수정 대상

- `996686d`: [TCPMotionInterface.st](</C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st>)
  - RPC 초기화/Callback/Close
  - TCP 수신 누적 및 세션 처리
- `da4a912`
  - `TCPMotionInterface.st`: Object Registry, Dispatcher, DINT handler, `LMCAxis2~4`
  - `Motion_Network.lcn`: `_LMCAxis2~4.Control` 연결

`Elmo_EtherCAT_Test_4Axis_Edit`는 폴더 전체가 Git 미추적 상태라서 커밋 이력이 전혀 없습니다. 따라서 그 폴더의 기존 수정 작성자는 Git으로 특정할 수 없습니다. 이후 PC API 작업에서는 비교 대상으로만 사용하고 수정하지 않았습니다.

### Find in Implementation 장애

직접 재현했고, `_Edit`에서 실제로 IDE 내부 예외가 발생합니다.

- 원본 프로젝트: `Power`, `pos`, `velo` 검색 정상
- `_Edit`: `acc`, `dec`, `LMCAxis` 검색이 모두 `CInvalidArgException`
- `_Edit`를 Rebuild/Link한 뒤에도 동일하게 실패

증거는 [Lasal2.log](</C:/Users/dreiv/AppData/Local/Temp/Lasal2.log:12332>)에 남아 있습니다.

가장 유력한 원인은 [_Edit의 TCPMotionInterface.st 806행](</C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis_Edit/Class/TCPMotionInterface/TCPMotionInterface.st:806>)입니다.

```st
// 확정된 LMC 패킷 순서
```

이 파일은 BOM 없는 UTF-8이고, 한글 7자가 21바이트를 차지합니다. 이후 검색 위치는 문자 기준과 바이트 기준이 14만큼 어긋납니다. `Find in Implementation`은 검색 결과를 코드 편집기에서 색으로 표시하므로 잘못된 범위를 전달하면서 `CInvalidArgException`이 발생하는 정황과 정확히 맞습니다.

다만 한글 주석을 ASCII로 바꾸고 재시험하지 않았으므로 아직은 “매우 강한 원인 추정”입니다.

다음 항목들은 원인 가능성이 낮거나 배제됐습니다.

- 두 `.lcp` 설정은 프로젝트 이름 한 줄을 제외하고 동일
- Class/Header/Network 등록 누락 없음
- GUID 중복 없음
- `BrowserInfo_arm.bin`, `LobInfo_arm.bin`에 소스 경로 누락 없음
- Read-only/권한/경로 길이 문제 없음
- `_Edit` 빌드 자체는 0 error

### 두 프로젝트의 실질 차이

- 원본: `LMCAxis`부터 `LMCAxis4`까지 있고 RPC/Dispatcher/DINT 구현이 들어감
- `_Edit`: `LMCAxis` 하나만 있으며 이전 패킷 파서 구현
- `.lcb`, `.lba`, `.lcn`, `ProjectInternal` 차이 대부분은 위 소스 차이와 IDE 재생성 결과

따라서 앞으로 LASAL 개발은 Git 추적 대상인 `Elmo_EtherCAT_Test_4Axis`에서 진행하는 것이 맞습니다. `_Edit`는 먼저 한글 주석을 ASCII로 바꿔 검색을 재시험하고, 그래도 실패하면 두 IDE를 종료한 뒤 `_Edit`만 단독 실행하여 캐시 문제를 확인하면 됩니다.

이번 조사에서는 파일을 수정하지 않았습니다. 현재 원본 프로젝트에는 IDE 저장/빌드 과정에서 생긴 것으로 보이는 미커밋 변경 10개가 남아 있으므로, 다음 코드 작업 전 이 변경부터 보존 여부를 확인해야 합니다.

> 분석된 내용을 정리해서 Lasal 프로그래밍 지침서를 만들어서 에러가 발생 안되도록 해.

<details><summary>16 previous messages</summary>

> 이번에는 코드가 아니라 재사용 가능한 LASAL 프로그래밍 지침서를 저장소에 만들겠습니다. 기존 코딩 규칙과 중복되지 않게 연결하고, 방금 확인한 UTF-8 다중바이트 문자·생성영역·프로젝트 복제·IDE 캐시·Git 추적 기준을 실제 체크리스트와 복구 절차로 정리하겠습니다.
>
> 지침서는 기존 문서와 분리된 고립 문서로 두지 않겠습니다. `docs/architecture`에 오류 예방·복구 전용 지침서를 만들고, 기존 코딩 규칙·스터디·README·AGENTS에서 필수 문서로 연결해 이후 LASAL 작업 때 자동으로 확인되도록 하겠습니다.
>
> <details><summary>Created a file, edited 5 files</summary>
>
> - Wrote `./docs/architecture/SIGMATEK_LASAL_programming_error_prevention_guide.md` (+372 -0)
> - Wrote `./AGENTS.md` (+10 -5)
> - Wrote `./README.md` (+3 -2)
> - Wrote `./docs/architecture/SIGMATEK_LASAL_coding_rules.md` (+21 -8)
> - Wrote `./docs/architecture/SIGMATEK_LASAL_programming_method_study.md` (+12 -3)
> - Wrote `./docs/architecture/SIGMATEK_LASAL_programming_error_prevention_guide.md` (+2 -2)
>
> </details>
>
> 지침서 초안과 필수 연결은 작성했습니다. 이제 명령 예제가 실제 PowerShell에서 동작하는지, Markdown 링크가 존재하는지, 기존 사용자/IDE 변경과 섞이지 않았는지 문서 범위만 검증하겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./docs/architecture/SIGMATEK_LASAL_coding_rules.md` (+2 -1)
> - Wrote `./docs/architecture/SIGMATEK_LASAL_programming_method_study.md` (+2 -1)
> - Wrote `./docs/architecture/SIGMATEK_LASAL_programming_error_prevention_guide.md` (+10 -8)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/architecture/SIGMATEK_LASAL_programming_error_prevention_guide.md` (+7 -0)
> - Wrote `./docs/architecture/SIGMATEK_LASAL_programming_error_prevention_guide.md` (+5 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_COMMAND_QUEUE_RTWORK_DESIGN_2026-07-10.md` (+6 -1)
>
> </details>
>
> 최종 리뷰에서 예방 절차의 허점을 찾았습니다. 새 미추적 LASAL 소스는 일반 `git diff`에 안 잡히고, 전체 `Lasal2.log` 검색은 과거 오류까지 다시 잡습니다. 이 두 검사를 보완하고, 아직 승인 전인 queue/RtWork 설계를 확정 구현처럼 쓰지 않도록 상태 표현과 실제 PLC 완료 기준도 바로잡겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_COMMAND_QUEUE_RTWORK_DESIGN_2026-07-10.md` (+4 -3)
> - Wrote `./docs/architecture/SIGMATEK_LASAL_coding_rules.md` (+3 -3)
> - Wrote `./docs/architecture/SIGMATEK_LASAL_programming_error_prevention_guide.md` (+60 -13)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 최종 리뷰에서 중요한 누락이 확인됐습니다. 특히 현재 callback이 아직 motion 함수를 직접 실행하는 상태를 “미완료 위험”으로 명시하고, IDE 검색 인덱스·ASCII 범위·staged/untracked 검사·CodeGenerator 재생성 조건을 더 정확하게 고친 뒤 검증하겠습니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./AGENTS.md` (+3 -3)
> - Wrote `./docs/architecture/SIGMATEK_LASAL_coding_rules.md` (+15 -4)
> - Wrote `./docs/architecture/SIGMATEK_LASAL_programming_error_prevention_guide.md` (+58 -21)
> - Wrote `./docs/architecture/SIGMATEK_LASAL_programming_method_study.md` (+2 -1)
>
> </details>
>
> 핵심 수정은 끝났습니다. 이제 문서 링크가 실제 파일을 가리키는지, 코드 블록이 닫혔는지, PowerShell 검사 예제가 현재 저장소에서 실행되는지까지 확인하고 있습니다. LASAL 소스 자체는 건드리지 않았습니다.
>
> 원인 표현도 한 단계 더 보수적으로 다듬겠습니다. 한글 주석은 유력한 후보지만 아직 A/B 확정 전이고, 동일 GUID 복제본·복사된 `ProjectInternal`·동시 IDE 실행 조건도 분리 시험하지 않았으므로 단일 원인으로 못 박지 않겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/SIGMATEK_LASAL_programming_error_prevention_guide.md` (+19 -5)
>
> </details>
>
> 마지막 기술 검토에서 실제로 놓칠 수 있는 검사 결함을 잡았습니다. `rg`는 잘못된 UTF-8/legacy high-byte를 못 찾을 수 있으므로 PowerShell 기반 검사로 바꾸고, 기존 연구 문서의 queue 흐름은 “현재 구현”이 아니라 “목표 구조”라고 바로잡겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./docs/architecture/SIGMATEK_LASAL_coding_rules.md` (+6 -2)
> - Wrote `./docs/architecture/SIGMATEK_LASAL_programming_error_prevention_guide.md` (+17 -12)
> - Wrote `./docs/architecture/SIGMATEK_LASAL_programming_method_study.md` (+6 -2)
>
> </details>
</details>

LASAL 오류 예방 지침서를 만들고 기존 개발 문서 체계에 연결했습니다.

핵심 문서:

- [SIGMATEK LASAL 프로그래밍 및 IDE 오류 예방 지침서](./docs/architecture/SIGMATEK_LASAL_programming_error_prevention_guide.md)
- [LASAL 코딩 규칙](./docs/architecture/SIGMATEK_LASAL_coding_rules.md)
- [LASAL 프로그래밍 방법 연구](./docs/architecture/SIGMATEK_LASAL_programming_method_study.md)

반영한 핵심 기준은 다음과 같습니다.

- 개발 기준 프로젝트를 `Lasal_PRG/Elmo_EtherCAT_Test_4Axis`로 고정
- `_Edit` 복제본은 비교·장애 재현용으로만 사용
- LASAL custom source와 IDE metadata는 7-bit ASCII만 입력
- invalid UTF-8까지 검출하도록 `Select-String` 기반 검사 적용
- CodeGenerator 영역과 사용자 구현 영역 분리
- `LMCAxis2..4`, registry, client count 6의 IDE 재생성 검증을 선행 조건으로 지정
- `Find in Implementation` smoke test와 신규 로그만 검사하는 절차 추가
- `CInvalidArgException` 발생 시 A/B 시험, cache 격리, lock 확인 복구 절차 추가
- 현재 `Response → MsgPaser → motion method` 직접 호출을 known gap으로 명시
- queue 자체 저장소와 `RtWork()` 실행 구조가 검증되기 전에는 production PLC motion 승인 금지
- 실제 PLC 다운로드, 안전 장비 시험, 패킷 재캡처까지 완료해야 production 완료로 판정

검증 결과:

- 문서 범위 `git diff --check`: 통과
- Markdown 링크와 코드 블록 구조: 통과
- legacy high-byte 검출 명령: 실제 파일에서 동작 확인
- LASAL 소스는 이번 작업에서 수정하지 않음
- 아직 커밋하지 않음

단, 전체 작업 트리의 `git diff --check`는 기존 LASAL 변경의 trailing whitespace 때문에 실패합니다.

- `ECAT_Master_Base.st`: 5050~5051행
- `TCPMotionInterface.st`: 286, 312~314행

따라서 현재 전체 작업 트리는 아직 LASAL 소스 커밋 허용 조건을 충족하지 않습니다.

> 이제 _Edit은 무시하고 Elmo_EtherCAT_Test_4Axis로 개발 진행할 수 있도록하자. 지금 설계단계 어디까지 진행됐는지 브리핑 해주고 추가로 어떻게 더 설계해야되는지 브리핑해봐

<details><summary>2 previous messages</summary>

> 앞으로는 `_Edit`을 분석·개발 대상에서 제외하고, Git 추적 프로젝트 `Lasal_PRG/Elmo_EtherCAT_Test_4Axis`만 기준으로 보겠습니다. 현재 설계 문서와 실제 소스를 교차 확인해서 `확정됨 / 설계됐지만 미승인 / 아직 설계 필요 / 구현됨`을 섞지 않고 브리핑하겠습니다.
>
> 현재 단계는 “설계가 없는 상태”는 아닙니다. 패킷·UNIT·단일 세션 RPC·object dispatcher는 상당히 구체화됐고 일부 prototype도 들어가 있습니다. 하지만 핵심 실행 구조인 command queue/RtWork는 D0~D15가 전부 미승인이라, 공식 개발 단계로 보면 아직 `Gate 0 → Gate 1` 경계입니다.
</details>

결론부터 말하면, 앞으로 `_Edit`은 완전히 제외하고 [Elmo_EtherCAT_Test_4Axis](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis)만 개발 기준으로 삼으면 됩니다.

현재 단계는 정확히 말하면:

> 기능·패킷 설계와 prototype은 상당히 진행됐지만, production용 실행 구조는 아직 `Gate 0 → Gate 1` 경계다.

즉 지금 바로 모션 기능을 더 붙일 단계가 아니라, 기존 변경을 기준점으로 고정하고 `Queue/CyWork/RtWork` 구조를 확정해야 하는 단계입니다.

## 현재까지 진행된 상태

| 영역 | 현재 상태 | 판정 |
|---|---|---|
| 개발 프로젝트 | canonical 프로젝트 확정 | 이번 결정으로 D0의 대상 선택 완료 |
| PC 패킷 API | 23/23 command 경로 구현 | PC 측 완료 |
| LASAL handler | 21/23 case 존재 | 19개 기능 후보, 2개 `-5` |
| UNIT | caller가 UNIT을 곱하고 DLL/LASAL은 재변환하지 않음 | 확정 |
| RPC lifecycle | `0x8080 → 0x405C → command → 0x405D` | 단일-session prototype 구현 |
| Object dispatcher | 실제 LASAL 이름 → descriptor 1~4/`0x0100` | source/network prototype 구현 |
| Queue/RtWork | 상세 설계안만 존재 | 구현 미승인 |
| Callback | UDP endpoint 등록까지 | event payload/sender 미설계 |
| Multi-PC | 개념 설계만 존재 | P0 범위 제외 권장 |
| 실제 PLC 검증 | 0/23 | production 완료 항목 없음 |

현재 정적 contract 검사는 직접 다시 실행했고 통과했습니다.

```text
PASS LASAL.StaticContract
(6 clients, 4 links, offsets, error guards, legacy block)
```

하지만 자동 검사는 LASAL IDE compile, PLC download, 실제 모션을 대신하지 못합니다. 현재 완료도 근거는 [API 개발 백로그](./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md:40)에 정리돼 있습니다.

## 지금 남아 있는 핵심 위험

1. TCP callback이 아직 모션을 직접 실행합니다.

현재 [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:586)는 아래 흐름입니다.

```text
Response
  → MsgPaser
  → LMCAxis/LMCRobot method
