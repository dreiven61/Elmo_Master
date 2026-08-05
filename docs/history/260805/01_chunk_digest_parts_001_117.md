# Elmo Master history chunk digest: parts 001-117

## 범위와 읽기 기준

- 대상: `Elmo_Master_history_260805_1_part_001`부터 `part_117`까지, 원본 줄 `00001-29250`.
- 확인률: **117/117 파트 개별 확인 완료**.
- 이 문서는 히스토리 대화·도구 출력의 인덱스다. 아래의 빌드·테스트·소스 상태는 당시 기록이며, 2026-08-05 현재 작업 트리나 PLC 상태를 이번 작성 과정에서 재검증한 결과가 아니다.
- `part_117`은 `LMCEcatInputLatch.CopyAxisOwnershipStartupSnapshot` 선언을 편집하던 도중 끝난다. 따라서 마지막 작업은 완료가 아니라 **중단된 진행 상태**다.

## 파트별 digest

| Part | 원본 줄 | 주제 / 결정 단서 |
|---:|---:|---|
| 001 | 00001-00250 | 이전 260803 분할·무결성 요약과 `0x7D17` 미구현 체크포인트. SDK 1077/1077 등 과거 PASS가 있으나 PLC/WPF 경로와 실축 qualification은 미완료로 시작한다. |
| 002 | 00251-00500 | `0x7D17` 계약 확정: 23-DINT axis record 재사용, terminal record를 tombstone으로 전환, 같은 key/generation 재시도는 동일 92-byte snapshot을 반환하는 idempotent retire. |
| 003 | 00501-00750 | Windows 제어 지침과 앱 탐색. 구현 변경은 없고 LASAL Class 2 대상 찾기 단계다. |
| 004 | 00751-01000 | canonical LASAL Class 2를 찾아 실행. 아직 offline이며 feature gate나 PLC 조작은 하지 않았다. |
| 005 | 01001-01250 | 빈 LASAL workspace에서 Open Project 접근을 시도했으나 stale accessibility index로 실패; 상태 재관찰 단계다. |
| 006 | 01251-01500 | `Ctrl+O`로 프로젝트 대화상자를 열고 canonical `.lcp`를 선택해 로드를 시작한다. |
| 007 | 01501-01750 | 프로젝트 loading/compiling UI를 관찰. 코드·프로젝트 저장은 없다. |
| 008 | 01751-02000 | canonical `Elmo_EtherCAT_Test_4Axis`가 offline으로 로드되고 `LMCDiagnosticsService`를 연다. |
| 009 | 02001-02250 | Diagnostics class 선택·이동만 계속한다. 소스 변경 없음. |
| 010 | 02251-02500 | `0x7D17` declaration→source route→WPF 순서의 재개점. computer-use 상태 호출 오류가 발생한다. |
| 011 | 02501-02750 | LASAL 창/process 식별을 복구하는 단계. 구현 변경 없음. |
| 012 | 02751-03000 | Windows 제어 API를 다시 확인하고 올바른 창 상태 취득을 시도한다. |
| 013 | 03001-03250 | `LMCDiagnosticsService` 구현과 gate를 관찰: D1-D3/제한 SDO 경로는 보이지만 DS402 Home/TW20/ECAT IO 계열은 비활성 상태다. |
| 014 | 03251-03500 | Diagnostics class tree를 펼치려는 UI 탐색. 변경 없음. |
| 015 | 03501-03750 | `Methods` 노드에 접근하기 위한 class tree 탐색. 변경 없음. |
| 016 | 03751-04000 | `Methods`의 Global/Private 분류를 확인한다. |
| 017 | 04001-04250 | Private method 목록을 펼친다. 아직 선언 변경은 없다. |
| 018 | 04251-04500 | 기존 `HandleAxisDs402HomeStart/Outcome`, `ProcessAxisDs402Home`를 확인하고 Outcome 복제 준비. |
| 019 | 04501-04750 | Outcome method 복사 UI 작업. 아직 새 선언이 확인되지는 않는다. |
| 020 | 04751-05000 | Private 그룹에 붙여넣기/복제 시도. |
| 021 | 05001-05250 | Outcome signature가 복제되어 새 method skeleton 생성 단계로 진입한다. |
| 022 | 05251-05500 | `HandleAxisDs402HomeOutcome1` 복제본과 class modified 상태를 확인한다. |
| 023 | 05501-05750 | 수정된 class를 재관찰하며 저장 전 상태를 유지한다. |
| 024 | 05751-06000 | 새 `HandleAxisDs402HomeOutcome1` 노드 존재가 tree에서 확인된다. |
| 025 | 06001-06250 | 복제 method를 `HandleAxisDs402HomeRetire`로 rename하려 했으나 `set_value`가 실패한다. |
| 026 | 06251-06500 | rename 실패 상태를 다시 확인하고 context menu를 닫는다. |
| 027 | 06501-06750 | method 이름 편집창에서 전체 선택. |
| 028 | 06751-07000 | `HandleAxisDs402HomeRetire`를 입력하되 아직 확정 전이다. |
| 029 | 07001-07250 | Return으로 rename을 확정하고 modified class 상태를 확인한다. |
| 030 | 07251-07500 | 선언 저장 후 IDE를 닫고 tracked `.st` 구현은 외부 편집으로 처리한다는 방향. `Ctrl+S` 실행. |
| 031 | 07501-07750 | IDE 저장이 `LMCDiagnosticsService.st`를 129,272→90,877 bytes로 축소해 구현을 덮어쓴 사고 발견. dangling blob `ee99ca...`를 복구 원본으로 선정; WPF는 `0x7D16→0x7D17→Resolve`, 당시 smoke 323/323 주장. |
| 032 | 07751-08000 | LASAL 종료 시 미사용 library 제거 질문에 No를 선택해 기존 library 구성을 보존한다. |
| 033 | 08001-08250 | blob 복구 후 `0x7D17` handler declaration, Diagnostics/TCP route, terminal tombstone read/detail32/idempotent retire 구현을 적용했다는 기록. 관련 gates/capability는 계속 OFF. |
| 034 | 08251-08500 | LASAL을 재실행해 canonical 프로젝트를 다시 로드한다. |
| 035 | 08501-08750 | 프로젝트 auto-build 후 Retire method가 Private tree에 있고 AWL implementation이 열리는 것을 smoke로 확인한다. |
| 036 | 08751-09000 | `LMCDiagnosticsService.InputLatch` client와 Comm Network 연결을 탐색한다. |
| 037 | 09001-09250 | `Comm_Network.lcn`에서 InputLatch 검색/연결 위치를 확인한다. |
| 038 | 09251-09500 | InputLatch network/source occurrence를 열어 읽기만 한다. |
| 039 | 09501-09750 | `Find in Implementation` 경로를 찾기 위해 메뉴를 조사한다. |
| 040 | 09751-10000 | 네트워크/툴바를 탐색한 뒤 Diagnostics 구현에서 InputLatch 검색을 연다. |
| 041 | 10001-10250 | 중간 checkpoint: `0x7D17` F9 0 errors/24 warnings, PC 1077/1077 주장. 독립 리뷰가 WPF의 `0x7D16/0x7D17` 전체 snapshot equality 누락을 발견해 회귀 보강 중. |
| 042 | 10251-10500 | LASAL search dialog 위치·API 오류를 복구하는 UI 단계. |
| 043 | 10501-10750 | 검색/goto 시도 중 금지된 Windows-key 계열 조작 기록이 있으며, line 861 이동을 시도한다. 구현 상태 변화는 없다. |
| 044 | 10751-11000 | Windows 제어 API 재확인. 소스 변경 없음. |
| 045 | 11001-11250 | computer-use guidance가 대부분을 차지하며 프로젝트 변경 없음. |
| 046 | 11251-11500 | goto-line 입력을 계속 시도하다 사용자 개입 직전까지 진행한다. |
| 047 | 11501-11750 | 사용자 요청으로 느린 직접 IDE 제어를 중단하고 사람에게 search를 요청. 이후 `0x7D17`은 SourceOnly/SDK/WPF/F9/search smoke 완료로 정리되지만, 10:02 저장이 `LMCEcatInputLatch` DS402 methods를 stub으로 만든 사고를 발견하고 복구를 지시한다. |
| 048 | 11751-12000 | test clone임을 확인해 production 손상이 아니라고 판정하고 `LMCEcatInputLatch`를 blob `39e25c...`에서 복구. 사용자는 LMC Home/DS402 Home/TW20/TW19 모두 사용 가능하게 할 것을 명시; Home은 무이동 current-position-zero, LMC 명칭, DS402 method 37/`0x607C=0`로 구체화. |
| 049 | 12001-12250 | SDK 1069 PASS 주장, TW19/TW20 전용 lifecycle과 generic SDO fallback 차단. `SubmitAxisZeroHome`/`CopyAxisZeroHomeResult` IDE signature, Save All 후 F9 금지 요청. DS402 timeout을 absolute service time으로 보정. |
| 050 | 12251-12500 | SDK 1074/1074 및 WPF focused tests 주장. 사용자 Save/IDE 종료 후 `^pVoid`가 잘못된 double-pointer 의미임을 교정해 `^void` ABI로 재입력. Zero Home RT SetPosition-once + raw unchanged + 3 stable cycles 구현 주장; duplicate stubs 제거. `HandleAdminCommands` 40,832 bytes로 관리 한도 초과 발견. |
| 051 | 12501-12750 | TW19/TW20 object를 `0x20FC:01/02`, `UINT16`, feedback socket value로 교정하고 `0x3204:13/14` 전송/fallback을 금지. 70개 IDE declaration/network 오류를 exact handoff로 복구, `|` 정수 OR 오류 교정 후 사용자 Rebuild가 C78 0 errors/38 warnings로 성공했다는 기록. |
| 052 | 12751-13000 | 사용자가 평일 직접 IDE 제어 허용 시간을 17:30~다음 날 08:30로 부여; Windows 제어 지침 확인. |
| 053 | 13001-13250 | 사용자가 주말·일요일·공휴일은 종일 직접 제어 허용. 최신 historical 권한 규칙이 평일 야간/익일 아침 + 휴일 종일로 확정된다. |
| 054 | 13251-13500 | 열린 앱/창을 열거하고 LASAL canonical 여부를 판별하는 단계. |
| 055 | 13501-13750 | 열린 프로젝트가 canonical이 아니라 `Elmo_Master_test` clone임을 확인. gates와 축별 encoder compatibility manifest가 0인 상태를 관찰하고 저장 없이 전환하기로 한다. |
| 056 | 13751-14000 | clone 전환 시도 중 UI 오류 후 LASAL이 닫혔음을 확인한다. |
| 057 | 14001-14250 | canonical Lasal2를 다시 실행하고 blank workspace에서 Open Project를 시작한다. |
| 058 | 14251-14500 | 파일 대화상자에서 canonical 프로젝트를 선택해 loading/compiling을 시작한다. |
| 059 | 14501-14750 | canonical 프로젝트가 로드되고 Diagnostics gates가 0인 상태에서 Edit/search 메뉴를 연다. |
| 060 | 14751-15000 | `LMCDiagnosticsService`에서 feature gates FALSE와 encoder manifests 0을 다시 관찰; `HandleEncoderMaintenanceStart` 부근에서 메뉴 탐색. |
| 061 | 15001-15250 | Class View로 이동하고 `LMCDiagnosticsService` context menu를 여는 UI 단계. |
| 062 | 15251-15500 | canonical 전환 확인. 표시된 `1 error`는 vendor `DriveComL2.h` 일시 읽기 오류라는 historical 해석이며, 대형 handler 분리를 위해 `HandleDiagnosticsBulkRequest` private method 생성을 시작한다. |
| 063 | 15501-15750 | 새 Diagnostics method 이름을 `HandleDiagnosticsBulkRequest`로 입력·확정한다. |
| 064 | 15751-16000 | Private tree에 `HandleDiagnosticsBulkRequest` 노드가 생성된 것을 확인한다. |
| 065 | 16001-16250 | 새 Diagnostics helper의 method variable context menu를 연다. |
| 066 | 16251-16500 | 첫 input variable 생성을 시작한다. |
| 067 | 16501-16750 | 첫 input 이름을 `CommandId`로 편집한다. |
| 068 | 16751-17000 | `CommandId` 이름을 확정한다. |
| 069 | 17001-17250 | `CommandId : UINT` 타입을 지정·확정한다. |
| 070 | 17251-17500 | helper 상세 편집기 진입을 준비한다. |
| 071 | 17501-17750 | `HandleDiagnosticsBulkRequest` 상세 편집기를 연다. |
| 072 | 17751-18000 | Diagnostics helper의 나머지 ABI를 `pRequest`, `RequestSize`, `pResponse`, `ResponseCapacity`, `CallerSessionEpoch`, output `ResponseSize` 순서로 만들기로 명시; `pRequest` 생성을 시작한다. |
| 073 | 18001-18250 | `pRequest : ^USINT` 이름/타입/포인터 속성을 설정하는 UI 작업. |
| 074 | 18251-18500 | `pRequest` pointer=true 선택을 여러 방식으로 재시도; API 문서를 확인한다. |
| 075 | 18501-18750 | pointer=true 토글/확정 재시도 후 다음 input variable 메뉴로 이동한다. |
| 076 | 18751-19000 | `RequestSize : UDINT`를 만들고 `pResponse` input 생성을 시작한다. |
| 077 | 19001-19250 | `pResponse : ^USINT`와 pointer=true를 설정. 이 시점 narrative는 앞 4개 인자를 정확히 넣었다고 주장한다. |
| 078 | 19251-19500 | `ResponseCapacity : UDINT`를 만들고 `CallerSessionEpoch` input 생성을 시작한다. |
| 079 | 19501-19750 | `CallerSessionEpoch : UDINT`를 확정하고 output `ResponseSize`를 생성한다. |
| 080 | 19751-20000 | Control helper 생성을 위해 source/class tree에서 `LMCControlCommandService`로 이동한다. |
| 081 | 20001-20250 | Control class 전환 경로를 찾기 위해 File/Library/Project/View 메뉴를 탐색한다. |
| 082 | 20251-20500 | Diagnostics helper 7개 인자 완료를 주장. source file open과 current Class View 전환을 구분하며, Comm Network에서 Control object를 찾는다. |
| 083 | 20501-20750 | Network에서 Control object를 열어 Private method `HandleAxisZeroHomeCommands`를 생성하고 이름 입력까지 진행한다. |
| 084 | 20751-21000 | `HandleAxisZeroHomeCommands` 이름 확정 후 `CommandId : UINT`를 추가하고 `Reference` input을 준비한다. |
| 085 | 21001-21250 | `Reference : UINT`를 만들고 `pRequestFrame` input 생성을 시작한다. |
| 086 | 21251-21500 | `pRequestFrame : ^USINT` pointer=true를 설정하고 `RequestFrameSize`를 추가한다. |
| 087 | 21501-21750 | `RequestFrameSize : UDINT` 확정. 남은 ABI를 `pResponseFrame`, `ResponseCapacity`, output `ResponseSize`로 명시하고 `pResponseFrame` 생성을 시작한다. |
| 088 | 21751-22000 | `pResponseFrame : ^USINT` pointer=true와 `ResponseCapacity : UDINT`를 설정한다. |
| 089 | 22001-22250 | Control helper output `ResponseSize`를 추가하고 Save All. LASAL 종료 시 library 보존을 위해 No를 선택한다. |
| 090 | 22251-22500 | 두 대형 handler를 helper로 분리: Control `18,211 + 22,608`, Diagnostics `23,490 + 10,824` bytes 주장. verifier를 새 ABI/ownership/size/RT latch 기준으로 대폭 보정하고 SourceOnly/full static 통과 후 C# 1075/1075를 보고, Rebuild를 위해 IDE 재실행한다. |
| 091 | 22501-22750 | computer-use guidance/confirmation 전문이 대부분. 프로젝트 상태 변화 없음. |
| 092 | 22751-23000 | LASAL 실행 후 canonical project Open을 시도하나 stale element 오류가 발생한다. |
| 093 | 23001-23250 | `Ctrl+O`와 파일 대화상자로 canonical LCP 선택을 반복; 좌표 범위 오류 후 재관찰한다. |
| 094 | 23251-23500 | canonical LCP를 실제 선택·로드하고 `Elmo_EtherCAT_Test_4Axis - Comm_Network` 창을 확인한다. generated declaration에 Control `InputLatch` client가 보인다. |
| 095 | 23501-23750 | project load 시 표시된 오류 행을 찾으려는 단계. 정확한 오류 결론은 이 파트만으로 확정되지 않는다. |
| 096 | 23751-24000 | Build menu에서 `Rebuild All`을 실행한다. |
| 097 | 24001-24250 | LASAL C78 ARM Rebuild가 `0 error(s), 38 warning(s)`로 끝났다는 UI 관찰. 이어 두 helper search smoke 준비. |
| 098 | 24251-24500 | Class View로 돌아가 대상 class/method를 찾는다; API 문서가 길게 포함된다. |
| 099 | 24501-24750 | Control class context/search 경로를 연다. |
| 100 | 24751-25000 | Control Methods/Private tree를 펼친다. |
| 101 | 25001-25250 | Private tree에서 `HandleAxisZeroHomeCommands` 노드를 확인하고 implementation 접근을 준비한다. |
| 102 | 25251-25500 | `HandleAxisZeroHomeCommands`를 double-click해 implementation 위치 smoke를 수행한다. |
| 103 | 25501-25750 | Diagnostics class의 Methods/Private tree로 전환한다. |
| 104 | 25751-26000 | `HandleDiagnosticsBulkRequest` 위치를 찾아 implementation smoke를 시도한다. 창 title이 여전히 Control로 보이는 순간이 있어 최종 성공 판단은 후속 기록에 의존한다. |
| 105 | 26001-26250 | tree에 두 helper 이름이 함께 보임. compaction 뒤 Save All·종료 및 새 `CInvalidArgException` 검사 단계로 넘어간다. |
| 106 | 26251-26500 | IDE Save All/종료, library 보존 No, 신규 `CInvalidArgException` 0건 주장. parser 2/2, negative fixture 131/131, SourceOnly/full static, C# 1075/1075, diff checks PASS를 최종 보고하지만 네 feature gates FALSE·startup proof BootId-only·encoder manifests 0이므로 PLC download/실축 사용 불가라고 명시한다. 이어 startup ownership 결함 분석을 새 작업으로 시작한다. |
| 107 | 26501-26750 | computer-use guidance 전문. startup ownership 구현 자체 변화는 없다. |
| 108 | 26751-27000 | 새 `LMC_AXIS_OWNERSHIP_STARTUP_RECONCILER_IDE_HANDOFF_2026-08-03.md` 작성 기록 후 LASAL 앱을 찾는다. |
| 109 | 27001-27250 | LASAL IDE를 실행하고 canonical project Open을 시작한다. |
| 110 | 27251-27500 | 파일 대화상자에서 프로젝트 선택 후 load를 기다린다. |
| 111 | 27501-27750 | canonical 프로젝트 로드 완료. current gates FALSE, ownership constants, `LMCEcatInputLatch` class를 관찰. 설계는 BootId-only 보고를 폐기하고 InputLatch seqlock 안의 48-byte startup snapshot + Control의 서로 다른 RT cycle 3회 판정으로 확정한다. |
| 112 | 27751-28000 | startup reconciler handoff 문서를 수정하고 InputLatch class 선언 편집을 위해 Windows 제어 API 오류를 복구한다. |
| 113 | 28001-28250 | `LMCEcatInputLatch` context menu 접근을 여러 번 재시도; 아직 method 생성 전이다. |
| 114 | 28251-28500 | InputLatch menu에서 New Method를 누르려 하나 좌표/서브윈도우 범위 오류가 발생해 재관찰한다. |
| 115 | 28501-28750 | New Method action이 열리고 기본 method node가 생성된 정황. 이름 편집 포커스는 아직 확보하지 못한다. |
| 116 | 28751-29000 | F2로 이름 편집을 열고 `CopyAxisOwnershipStartupSnapshot` 입력·확정을 시도. `set_value` timeout 후 type_text로 재시도하며 method tree를 다시 연다. |
| 117 | 29001-29250 | 최초 확인은 `hasNew:false`; 중간 Save All 후 Private tree에서 `CopyAxisOwnershipStartupSnapshot` node를 확인한다. global 접근/입출력 ABI/구현은 아직 맞추지 못한 채 method 속성 편집 준비에서 히스토리가 종료된다. |

## 교차 파트 chronology

1. **`0x7D17` retire 도입 (001-047)**
   idempotent tombstone 계약을 세우고 LASAL/WPF/검증기를 맞췄다. 그러나 LASAL IDE 저장이 implementation을 축소·stub화하는 사고가 두 차례 발생해 dangling blob 복구가 필요했다. 최종 historical checkpoint는 `0x7D17` 정적·PC·IDE smoke 완료지만 PLC/runtime 완료는 아니다.

2. **네 기능 요구와 ABI/객체 교정 (048-051)**
   사용자는 LMC Home, DS402 Home, TW20, TW19를 모두 요구했다. Home은 축을 움직이지 않고 현재 actual position을 zero로 만드는 동작이며, DS402 variant는 method 37/`0x607C=0`로 정의됐다. TW19/TW20 객체는 중간의 `0x3204` 주장에서 `0x20FC:01/02 UINT16` 전용 경로로 교정됐다.

3. **IDE 선언·Network 복구와 직접 제어 권한 (050-060)**
   declaration/network 누락, `^pVoid`, duplicate skeleton, 정수 OR 문제를 순차 교정하고 C78 Rebuild 0/38을 얻었다는 기록이다. 사용자는 최신 기준으로 평일 17:30~다음 날 08:30, 주말·공휴일 종일 직접 IDE 제어를 허용했다. 그 외 평일 시간대는 사용자에게 요청해야 한다.

4. **대형 handler 분리 (061-106)**
   IDE에서 두 private helper ABI를 생성하고 외부 편집으로 Control/Diagnostics 대형 handler를 각각 32 KiB 미만으로 분리했다. verifier negative fixtures를 여러 차례 강화한 뒤 Rebuild/static/C#/search smoke PASS를 보고했다. 동시에 gates와 manifests는 계속 닫혀 있어 runtime usable 선언을 명시적으로 금지했다.

5. **startup ownership 다음 단계 (106-117)**
   기존 첫 BootId-only proof가 owner table을 영구 quarantine시키고 일반 Axis/Group 명령도 owner를 획득하지 않는 결함을 새 핵심 문제로 잡았다. 최소 설계는 `InputLatch` seqlock에 48-byte startup snapshot을 publish하고 Control에서 서로 다른 RT cycle 3개를 판정하는 것이다. 실제 history 끝점은 `CopyAxisOwnershipStartupSnapshot` private skeleton만 생성된 중간 상태다.

## 확인 사실과 historical claim의 구분

### 히스토리 자체에서 직접 확인되는 사실

- 사용자는 느린 IDE 직접 제어를 한때 중단시키고 사람에게 요청하라고 했으며(047), 이후 시간대별 직접 제어 권한을 다시 명시했다(051-053). 최신 지시는 후자다.
- 사용자는 네 기능 전부, 무이동 current-position-zero Home, LMC 명칭, build 우선 해결을 명시했다(048-051).
- UI 기록에는 canonical/test clone 혼동, IDE 저장에 의한 source 축소/stub, `^pVoid` 교정, C78 `0 errors, 38 warnings`, gates FALSE와 encoder manifest 0이 나타난다.
- 마지막 파트에서 `CopyAxisOwnershipStartupSnapshot`은 중간 Save All 뒤 **Private method node**로 보이지만 global/ABI/implementation 완료 증거는 없다.
- 이 117개 파트에는 PLC download, cold boot, 실제 EtherCAT axis, in-motion, encoder maintenance 실행 성공 증거가 없다.

### 현재 상태로 사용하기 전에 재검증해야 하는 historical claim

- dangling blob 복구가 현재 tracked source와 byte/line-ending까지 일치한다는 주장.
- SourceOnly/full static, parser/negative fixture, C# `1075/1075`, `git diff --check` PASS 숫자.
- 두 helper의 실제 current function byte size와 LASAL generated declaration/Network table 상태.
- search smoke 후 신규 `CInvalidArgException` 0건.
- `0x20FC:01/02`가 현재 장착된 각 축의 실제 encoder family/socket에서 지원된다는 것. ESI object 존재만으로는 충분하지 않다.
- 휴일 여부와 현재 시간이 자동화 허용 범위인지 여부. 실제 IDE 조작 직전에 한국 시간·휴일을 다시 판단해야 한다.

## 모순·교정 이력

- **`0x7D17` 상태:** 001의 “미구현”은 시작 snapshot이고, 047 이후의 “구현/정적 완료”가 더 최신이다. 어느 쪽도 PLC runtime 완료를 뜻하지 않는다.
- **WPF PASS 숫자:** 323→326→329 등으로 변하며, 초기 PASS 뒤 full snapshot equality 누락이 발견됐다. 숫자는 시점별 결과이지 누적된 현재 보증이 아니다.
- **TW19/TW20 object:** 048의 `0x3204:13/14` 주장은 051에서 `0x20FC:01/02 UINT16`로 명시 교정됐다. 최신 계약은 generic `0x3204` fallback 금지다.
- **pointer ABI:** `^pVoid`를 정상으로 본 중간 판단이 곧 double-pointer 오류로 교정됐다. 최신 ABI는 `^void`다.
- **IDE 제어 권한:** 047의 “직접 제어 중단/사용자에게 요청” 뒤 051-053에서 평일 야간·주말/공휴일 권한이 새로 부여됐다. 평일 주간은 여전히 사용자에게 요청한다.
- **정수 OR 수정 표현:** 한 요약에는 addition으로 바꾼다는 설명이 있고 더 최신 결과에는 `OR`로 수정했다고 기록된다. 현재 소스를 읽어 실제 표현을 확인해야 한다.
- **search smoke:** 104의 창 title은 target method와 어긋나 보이지만 106은 두 helper smoke 성공을 주장한다. `%TEMP%\Lasal2.log`와 현재 IDE search로 재확인하는 편이 안전하다.
- **build vs usable:** C78 0 errors는 정적 build 증거다. gates FALSE, startup quarantine, zero manifests 때문에 다운로드/실축 usable PASS와 모순되지 않는다.

## 명시된 사용자 요구

- LMC Home, LMC Home DS402, TW20, TW19를 모두 구현 대상으로 유지한다.
- Home은 home/limit switch를 찾거나 축을 움직이는 동작이 아니라, 현재 actual position을 zero로 만드는 동작이다.
- 명칭은 MMC가 아니라 LMC를 사용한다.
- build 오류를 먼저 해결하고, 선언/Network는 LASAL IDE에서, 기존 implementation은 tracked `.st` 외부 편집으로 처리한다.
- 직접 IDE 제어 허용 시간은 한국 시간 기준 평일 17:30~다음 날 08:30, 토·일·공휴일 종일이다. 그 외 평일 시간에는 사용자에게 IDE 작업을 요청한다.
- 실기 근거 없이 feature gate나 per-axis manifest를 켜거나 PLC에 다운로드하지 않는다.

## 미해결 작업과 안전한 재개점

### 즉시 해야 할 read-only 확인

1. `git status`, 관련 `.st`, generated declaration/Network, handoff 문서를 다시 읽어 이 history 이후 변경 여부를 확인한다.
2. LASAL이 아직 열려 있거나 unsaved 상태인지 확인한다. `part_117`의 stale UI handle/좌표를 재사용하면 안 된다.
3. `LMCEcatInputLatch`에 `CopyAxisOwnershipStartupSnapshot`가 private skeleton/duplicate stub로 실제 남았는지, declaration과 implementation을 각각 확인한다.
4. 기존 네 gate, axis profile/socket/evidence manifest, startup quarantine 조건을 현재 source에서 재확인한다.

### 이어서 구현해야 할 항목

- `CopyAxisOwnershipStartupSnapshot`를 요구된 global ABI로 완성하고, InputLatch의 기존 seqlock 안에서 48-byte startup evidence snapshot을 일관되게 복사하도록 구현한다.
- Control 쪽 startup reconciler가 서로 다른 RT cycle 3개의 full proof `0xF`를 판정하도록 구현하고, 첫 BootId-only 호출이 영구 quarantine을 만드는 경로를 제거한다.
- 일반 Axis/Group command도 공통 owner reserve/validate/commit/release 흐름에 실제로 편입한다. Robot 1..9 mask와 4-axis profile mask를 혼동하지 않는다.
- 기존 TCP BootId-only reporter의 제거/교체, verifier negative fixtures, ABI handoff 문서, generated declaration/Network 검증을 함께 맞춘다.
- IDE Rebuild, 두 신규/분리 helper `Find in Implementation`, smoke 시작 이후 `Lasal2.log`의 신규 `CInvalidArgException`을 확인한다.

### 아직 실행하면 안 되는 항목

- 네 feature gate enable, axis encoder/socket manifest provisioning, capability 광고.
- PLC download, Home/DS402 Home/TW19/TW20 실제 write, Group/mutation 시험.
- startup full-proof와 동일 BootId/capability/map provenance, DS402 warning 해소, 실제 encoder identity/socket 증거가 없는 상태의 실축 qualification.

## 다음 thread용 한 줄 상태

`0x7D17`과 네 기능의 source/IDE/build checkpoint는 historical PASS지만 runtime은 닫혀 있다. 현재 실질적인 다음 작업은 **중단된 `CopyAxisOwnershipStartupSnapshot` ABI/구현을 live source에서 재확인해 완성하고 startup ownership full-proof `0xF` 및 일반 Axis/Group ownership을 정적 검증하는 것**이며, 그 전에는 PLC download나 feature gate enable을 하면 안 된다.
