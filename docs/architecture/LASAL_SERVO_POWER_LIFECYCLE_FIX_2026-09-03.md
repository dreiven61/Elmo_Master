# 단축 Servo Power: Home 선행조건 및 소유권 종료 분류 수정

기준: `dev@ceb14b3f45f608aaac03e316781574559c2a29b5`의 작업 트리.
범위: LASAL 이식 API 단축 Power, SDK 오류 설명, 관련 검증. PMAS 원본 변경 아님.

## 추가 원인 확정: BootId 137의 Power On 예약 잔류

앞선 Home/rebase 및 Power Off 종료 분류 수정만으로 Servo On은 해결되지 않았다.
사용자 후속 시험과 현재 소스를 대조해 별도의 dispatch 결함을 확인했다.

| 사용자 Watch | 해석 |
|---|---|
| `OwnershipState[3]=137`, `[4]=4982`, `[5]=15`, `[24]=0` | 새 boot, 초기화 proof 게시, 전역 격리 없음 |
| `[29]=1`, `[30]=1`, `[31]=8227` | Axis1 DIRECT Power 요청이 RESERVED에 남음 |
| `[32]=7`, `[33]=7`, `[34]=4`, `[35]=58` | token/generation 7/7, session 4, sequence 58 |
| `[36]=76426`, `[37]=76426`, `[38]=0` | 예약 후 관측 진행 기록 없음 |
| `[42]=1`, `[43]=8`, `[44]=1`, `[45]=16777473` | ORDINARY, 8-byte payload; 마지막 DWORD `0x01000101`, Enable=1 |
| `[60..62]=0`, `[63]=0x4F574E01` | terminal 결과 없음, Axis1 record magic 정상 |
| `OwnershipObserverState[0]=1329746689`, `[1..11]=0` | observer magic만 생성, commit token 및 관측 evidence 미게시 |

`HandleRequest`는 `HandleAxisOwnershipSafetyRepeat`를 먼저 호출한다.
결함 코드에서 `0x2023`은 ON/OFF 모두 `repeatEligible=TRUE`지만,
`repeatShapeValid`는 Enable=0만 허용했다. 따라서 새 Power On 예약도
이 helper에서 `-9` 응답을 만들고 정상 `HandleRequest` 본문을 건너뛰었다.
또한 기존 fresh identity 검증은 SAFETY mode로 고정되어 있어, ON 형식만
허용하도록 넓히는 수정으로는 ORDINARY Power On이 통과할 수 없다.

이 경로는 native PowerOn 호출 전이며 commit/rollback을 실행하지 않는다.
따라서 RESERVED + terminal 없음 + observer token 0이라는 캡처와 일치한다.
`ProcessAxisOwnership`는 DIRECT_ACTIVE/GROUP_ACTIVE/SAFETY_PREEMPTING을
처리하고 RESERVED는 완료 감시 대상이 아니므로 기다린다고 이 예약이 해제되지 않는다.
이 결과를 Home 미완료 또는 전역 quarantine 문제로 설명하지 않는다.

추가 수정:

- first-dispatch 형식과 safety-repeat 형식을 분리한다.
- exact Power On은 ORDINARY mode로 기존 RESERVED identity 검증을 통과한 경우에만
  sentinel -11을 반환하고 정상 dispatch/commit/rollback 경로로 넘긴다.
- token/generation/session/sequence/target/payload 검증은 기존 validator를 그대로 사용한다.
- first-dispatch가 아닌 Power On은 safety coalescing/escalation에 진입하지 못한다.
- Power Off 및 다른 안전 명령의 repeat 처리, 기존 격리와 no-replay 정책은 유지한다.
- 이미 PLC에 남은 token 7 예약을 파일 수정으로 지우거나 자동 재실행하지 않는다.

검증 범위:

- 추가 검사 적용 후 수정 전 작업 트리: **4/133 FAIL**, 그중 Boot137 Power On
  first-dispatch 실패를 재현했다. 기존 94개 조건 검사에는 이 helper 경로가 빠져 있었다.
- 추가 수정 후: **133/133 source-predicate checks PASS**. helper 경로 검사는
  실제 소스 조건식과 명시적인 identity-validator stub을 사용하며 전체 ST 실행 시험이 아니다.
- safety-repeat contract: **31/31 negative fixture 거부**, 새 결함 회귀 4개 포함.
- rebase contract: **39/39 negative fixture 거부**.
- helper 크기: LF 29836 / CRLF 30707 bytes, 제한 32768 이내.
- 10:28 C78 rebuild와 10:29 클래스 다운로드/PLC link 성공은 **앞선 버전**의 증거다.
  이번 추가 수정의 IDE build/download 및 실제 Servo On 성공은 아직 확인하지 않았다.
- 이번 추가 수정은 C# 및 wire layout을 바꾸지 않는다. 아래 SDK/WPF 결과는
  앞선 수정 때의 PC 검사 결과이며 이번 PLC 결함의 해결 증거가 아니다.

## 관측 사실과 한계

- 사용자 Power On ACK: HeaderStatus=0, CommandStatus=1, ErrorId=-9.
- Watch: BootId=136, startup proof=15, stable samples=3, startup blocker=0.
- Axis1 `OwnershipState[29]=11`, `[30]=1`, `[31]=8227(0x2023)`.
- 추가 Watch: `[60]=3`(TERMINAL_SAFE_FAILURE), `[61]=8227`, `[62]=0`,
  `[63]=1331121665`(0x4F574E01, 정상 Axis1 record magic).
- 따라서 마지막 기록은 Power 명령의 safe-failure 종료이며 세션 종료 -20 또는
  tuple 불일치 -24 기록이 아니다. 0x2023만으로 ON/OFF 방향은 식별되지 않는다.
  현재 소스에서 정상 DIRECT Power 관측 경로가 이 결과를 게시할 수 있는 경우는
  Power Off 상태 도달 뒤 오류가 남은 경우다. Power On 완료 후보는 error-clear를 요구한다.
- 전역 `OwnershipState[24]=1`도 관측됐다. safe-failure 분류 자체는 이 전역 latch를
  설정하지 않으므로, 전역 latch의 최초 설정 원인까지 이 화면으로 확정하지 않는다.
  수정 후에도 동일 문제가 나면 owner/observer 전체, 명령 방향 및 시간순 기록이 필요하다.
- vendor `Include/types.h`의 `_LMCAXIS_STATUS.EmergStop` 정의는 오류뿐 아니라
  축 비활성화 시에도 설정됨을 명시한다. 기존 observer는 이 bit(0x200)도
  `allErrorClear=FALSE`로 처리하므로 정상 Power Off가 safe-failure가 될 수 있다.
  따라서 이 기록만으로 실제 드라이브 알람이 있었다고 단정하지 않는다.

## 소스 변경

1. `ReserveAxisOwnership`의 rebase 예외에 exact DIRECT / AXIS / ORDINARY
   단축 0x2023만 추가했다. 요청 형식 검증은 예외보다 먼저 수행된다.
2. `HandleAxisOwnershipSafetyRepeat`의 별도 rebase 사전 차단에서도 0x2023을
   제거했다. 한쪽만 수정하면 두 번째 차단이 계속 -9를 반환하므로 둘 다 수정한다.
3. `ProcessAxisOwnership`의 단축 Power Off 완료 후보는 `allPowerOff & allStandstill`.
   기존 fresh/coherent snapshot, 3 samples, 100 ms 및 identity 검증을 유지한다.
   이 exact Power Off 완료는 drive alarm이 남았다는 이유만으로 safe-failure가 되지 않는다.
   TERMINAL_SUCCESS는 Power Off 목표 완료를 뜻하며 drive alarm 해제를 뜻하지 않는다.
   drive alarm은 별도의 상태 읽기에 그대로 남는다.
4. 정상 성공은 기존 publisher를 통해 owner/observer/identity를 같이 정리한다.
   FORCE_QUARANTINE, preemption cleanup, 불명확한 실행 결과, timeout의 차단을 유지한다.
   기존 `[24]` 또는 `[29]`를 0으로 바꾸는 코드와 자동 Power On 재전송은 추가하지 않았다.
5. 오류 catalog v4: -9에 초기화/격리 가능성을 명시하고 무조건 retry 안내를 제거했다.
   -15가 Power On에서 나오면 배포 PLC 버전을 확인하도록 안내한다.
   서보 ON을 위해 무조건 현재 위치를 원점으로 덮어쓰라는 안내는 제거했다.

Power On은 Home/Referenced나 retained rebase bit를 설정/삭제하지 않는다.
단축 이동, Group Power/Enable, SetKin과 SetPosition의 기존 좌표 보호는 유지한다.
이 변경은 이동형 Home 기능 추가 또는 기존 Home capability 활성화가 아니다.
TCPMotionInterface, LmcProtocol 및 0x2023 request/ACK의 byte layout은 변경하지 않았다.

## 최초 수정 시점의 검증 (후속 결과는 BootId 137 절 참고)

- `Verify-LasalServoPowerLifecycle.ps1`: actual source predicate 94/94 PASS.
- 같은 검사를 `-Revision HEAD`로 수정 전 소스에 적용: 7/94 FAIL을 재현.
- `Assert-LasalAxisRebaseBarrierContract`/기존 self-test를 독립 호출:
  current source accepted, 39개 negative fixture 모두 거부.
- SDK Release build 및 PC suite: 1201/1201 PASS.
- WPF Release build 및 smoke suite: 398/398 PASS.
- 최초 수정 완료 시점에는 LASAL IDE compile/link, PLC download, physical servo 시험 미수행.
  이후 사용자 build/download 및 실패 결과는 위 BootId 137 절을 따른다.
- 기존 전체 method-size gate는 미수정 `TCPMotionInterface.MsgPaser`에서 실패
  (LF 37347 / CRLF 38357 bytes, limit 32768). 기존 전체 SourceOnly gate의
  reserve repeat/mutation baseline 불일치도 별개이며 focused PASS를 전체 PASS로 사용하지 않는다.

집중 검사 재현:

```powershell
& LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalServoPowerLifecycle.ps1
& LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalServoPowerLifecycle.ps1 -IncludeRebaseContractSelfTest
& LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalServoPowerLifecycle.ps1 -IncludeSafetyRepeatContractSelfTest
# 수정 전 baseline에서 위 결함을 검출하는지 확인; 의도적으로 nonzero 종료
& LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalServoPowerLifecycle.ps1 -Revision ceb14b3f45f608aaac03e316781574559c2a29b5
```

## 현장 적용 및 남은 확인

- 이번 변경은 기존 class implementation과 SDK 설명만 변경한다. 사용자 변경
  `Class/Classes.lcb`와 Network/generated 선언은 건드리지 않는다.
- 현재 실행 중인 Debug EXE를 교체하거나 종료하지 않았다. Release EXE/DLL은 별도 빌드한다.
- PLC에 기존 격리 record/global latch가 이미 남아 있으면 PC 재연결만으로 해제되지 않는다.
  새 소스의 IDE Rebuild와 안전한 정지 상태에서 승인된 download/runtime 재초기화가 필요하다.
  원인 자료를 저장하고 진행하며 Watch flag 강제 덮어쓰기로 대체하지 않는다.
- 새 BootId, startup proof, owner IDLE을 확인한 뒤 안전 조건하에서 사용자 시험:
  Home 미완료 + rebase bit set에서도 단축 Power On 접수 및 실제 ready 확인.
  Power Off 이후 owner IDLE 확인. alarm 잔류 상황은 안전한 fixture/승인된 절차에서만 재현한다.
- Home/rebase 미완료 이동 거절, 오류/미정지 Power On 조건, 진행 중 Home 소유권 충돌,
  통신 단절/timeout 시 no-replay, 기존 quarantine 유지도 확인한다.
- 정상 초기화 뒤에도 `[24]=1`이면 이번 수정으로 해결됐다고 간주하지 않는다.
  첫 설정 시점과 전체 owner/observer/identity snapshot을 추가 확보한다.
