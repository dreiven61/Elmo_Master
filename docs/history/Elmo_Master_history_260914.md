금요일 오후 4:23
붙여넣은 텍스트 (1)(4).txt
문서

@
GitHub
 이번 결과는 앞서 수정한 SDO 길이 오염 문제는 해결됐지만, DS402 Home이 완료 조건을 만들지 못하고 60초 timeout된 상황으로 보입니다.

근거는 시간입니다.

Start 승인: 16:02:40.193
1.2초 후 Outcome 조회: PASS
14초 후 Outcome 조회: PASS
약 76초 후 조회: DetailCode=26

초기 두 번의 Read Home Status PASS는 Home 성공이 아니라, Running 상태를 정상적으로 읽었다는 뜻입니다. WPF도 Running이면 완료로 처리하지 않습니다. [MainWindow.MaintenanceActions.cs (line 1057)](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MaintenanceActions.cs:1057)

DS402 Home 성공에는 다음 조건이 3회 연속 필요합니다.

StatusWord bit 12 = Homing attained
StatusWord bit 10 = Target reached
StatusWord bit 13 = Homing error clear
ActualPosition = 0

[LMCDiagnosticsService.st (line 6618)](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:6618)

이번에는 위치가 0으로 변경됐지만, Homing attained/Target reached 조건이 충족되지 않아 Running 상태가 계속된 것입니다. 위치 변경만으로는 terminal success가 아닙니다.

가장 유력한 원인은 PowerOn/Operation Enabled 없이 DS402 Home을 실행한 것입니다. 이번 로그에는 다음 순서만 있고 Power On이 없습니다.

Connect
Load Axis
Read Position
Capability refresh
DS402 Home

현재 PLC Start 검사는 DS402 base state 0x40, 0x21, 0x23, 0x27을 모두 허용합니다. 즉 PowerOff/Ready/SwitchedOn 상태에서도 Start가 접수될 수 있습니다. 하지만 실제 드라이브가 method 37의 bit 12를 완료 상태로 올리려면 Operation Enabled(0x27)가 필요한 것으로 이번 물리 결과가 나타냅니다.

따라서 현재 현상은 다음 흐름입니다.

Start accepted
→ 0x607C/0x6098/0x6060 처리
→ 위치 0 변경
→ Homing attained가 올라오지 않음
→ 60초 timeout
→ cleanup
→ unresolved/quarantine stage 101
→ DetailCode 26

정확히 확정하려면 현재 Watch에서 다음 값만 추가로 확인하면 됩니다.

Ds402HomeState[92]
[106], [107], [108]
[114] — 마지막 StatusWord
[17] — retained StatusWord

예상값은 다음과 같습니다.

[92]=101
[107]=30
[108]=0x05040000에 해당하는 timeout 값
[114]의 0x006F base state가 0x27이 아님
[114]의 bit 12 또는 bit 10이 미설정

현재 설계의 PowerOff + Standstill에서 DS402 Home 정책은 실제 드라이브 동작과 맞지 않는 것으로 보입니다. 다음 수정 방향은 DS402 Home Start를 stable PowerOn + DS402 Operation Enabled 상태에서만 허용하도록 PLC admission과 WPF 안내를 함께 바꾸는 것입니다. 현재 unresolved record는 위치가 0이 됐다는 이유만으로 수동 resolve하거나 Start를 재전송하면 안 됩니다.

오후 4:05

사용자 첨부 파일

3분 36초 동안 작업함

원인은 확인됐습니다. 이전 $DINT 변환 문제는 정상적으로 수정·반영됐습니다.

이번 실패는 DS402 Home 자체가 완전히 실패한 것이 아니라, 현재 PLC의 완료 판정 조건이 실제 드라이브 결과보다 지나치게 엄격해서 발생했습니다.

[96]=24673 = 0x6061, [97]=0, [98]=1
이전 SDO 인덱스 손상 문제는 해결됐습니다.
[114]=4816 = 0x12D0
Homing attained(bit12)=1
Homing error(bit13)=0
Target reached(bit10)=0
DS402 기본 상태는 0x0040, 즉 Switch On Disabled입니다.
[115]=1
실제 위치가 목표 HomePosition 0이 아니라 +1 count입니다.

현재 구현은 다음 조건을 전부 만족해야 완료 처리합니다.

Homing attained
Target reached
ActualPosition이 HomePosition과 정확히 동일
같은 위치가 3회 연속 유지

해당 조건은 [LMCDiagnosticsService.st (line 6618)](C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:6618)에 있습니다.

이번 결과는 Homing attained까지 올라왔지만:

Target reached=0
ActualPosition=1, 목표값은 0

이므로 완료 단계로 넘어가지 못하고 계속 대기했습니다. [118]=39495, [119]=99495 차이가 정확히 60000이므로 설정된 60초 후 timeout이 발생한 것도 확인됩니다. Timeout 판정은 [LMCDiagnosticsService.st (line 6516)](C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:6516)입니다.

그 뒤 복구도 완료되지 않았습니다.

[92]=101: 미해결/격리 상태
[106]=3, [107]=30: 일반 실행 실패
[116]=1: Homing mode 0x6060=6에서 CSP 0x6060=8 복귀가 증명되지 않음
[117]=0: RT Home control-word 소유권은 LOW로 내려감
[99]=6: CSP 복귀용 다음 SDO가 시작되지 않음
[108]=6: 원래 timeout 코드가 복구 과정의 하위 반환값 -6에 의해 덮어써진 상태

따라서 위치값은 변경됐지만 시스템은 정상화되지 않은 것이 맞습니다. 핵심 수정 대상은 다음 두 가지입니다.

Method 37 완료 판정을 Target reached 필수 + 위치 정확히 0으로 고정하지 말고, Homing attained + Homing error 없음 + 허용 오차 내 위치 안정 조건으로 조정
실패 복구 중 원래 timeout 원인을 덮어쓰지 않고 별도 cleanup 오류로 보존하며, CSP mode 8 복귀가 실행되도록 복구 경로 수정

현재 상태에서는 Record Resolve로 강제 해제하거나 DS402 Home을 재실행하지 않는 것이 맞습니다. 먼저 PLC를 정상 상태로 복구한 뒤 위 완료·복구 로직을 수정해야 합니다.

오후 4:11

IDE 종료했다. 코드 수정 진행해봐

오후 4:12

확인된 두 문제를 함께 수정하겠습니다. Method 37의 완료 판정을 실제 드라이브 상태에 맞추고, timeout 후 CSP 복귀가 1초 복구창 안에서 중단되면서 원인 코드까지 덮어쓰는 경로를 바로잡겠습니다. LASAL 코딩 규칙과 현재 변경 상태부터 확인한 뒤 구현만 진행하겠습니다.

명령을 실행했습니다

2분 4초 동안 작업함

수정 방향을 좁혔습니다. 기본 DS402 Method 37에만 Homing attained + no error를 완료 근거로 인정하고, Switch On Disabled에서는 Target reached를 강제하지 않겠습니다. 위치는 실측된 양자화 오차만 반영해 0±1 count로 제한하며, 일반 이동형 Home에는 이 완화를 적용하지 않습니다. 실패 복구창은 5초로 늘리고 원래 timeout 코드가 cleanup 오류로 덮이지 않게 보존합니다.

파일 수정함 명령을 실행함

사용 한도에 도달했습니다. 계속하려면 요금제를 업그레이드하거나 크레딧을 추가하세요. 또는 오후 6:03에 다시 시도하세요.

파일 4개를 편집했습니다+83-46변경 사항 검토

실행 취소리뷰

Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.stLasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st+62-32

docs/api/design/HOME_DS402_COMPLETION_IMPLEMENTATION_DESIGN_20260902.mddocs/api/design/HOME_DS402_COMPLETION_IMPLEMENTATION_DESIGN_20260902.md+5-4

docs/api/design/HOME_DS402_DESIGN.mddocs/api/design/HOME_DS402_DESIGN.md+5-4

1개 파일 더 보기

ds402 HOME 구현중인데 다음과 같이 로그가 발생하면서 안돼고 있어. 내용확인해서 어디가 문제인지 알려줘

확인해보니 이번 로그는 이전의 “60초 동안 Homing 완료조건을 못 만들어 timeout” 문제와 다른 실패입니다.

이번에는 DS402 Home 시작 후 약 2초 만에 PLC 내부 DS402 Home runtime이 stage 101 quarantine/unresolved 상태로 들어간 것이 핵심입니다.

로그 순서는 이렇습니다. 직전에 Power Off가 정상 완료됐고, 약 6.7초 뒤 DS402 Home Start가 접수됐습니다. 그런데 Start 후 2.1초 뒤 첫 Outcome 조회에서 바로 DetailCode=26이 발생했습니다.

16:19:28.202  Power Off safety verification PASS

16:19:34.910  DS402 Home started
16:19:34.966  DS402 Home PASS       ← Start ACK

16:19:36.957  Read Home Status
16:19:37.049  FAILED
               ErrorId=-31000
               DetailCode=26

여기서 DS402 Home PASS는 Home 완료가 아니라 0x7D15 Start 요청이 접수됐다는 뜻입니다. WPF도 실제 terminal 여부는 별도 Outcome으로 판정합니다.

현재 DetailCode=26이 의미하는 것이 명확합니다

현재 dev의 HandleAxisDs402HomeOutcome()을 보면 26은 그냥 “아직 Running”이 아닙니다.

elsif (baseOutcomeState = 1) &
      (Ds402HomeState[92] = 101) &
      (Ds402HomeState[93] = TO_DINT(Reference - 1)) then

    detailCode := 26;

즉 이번 로그가 26을 반환했다는 것 자체로 다음 세 가지는 사실상 확정됩니다.

Durable record state = 1 (Running)
Ds402HomeState[92] = 101
Ds402HomeState[93] = 현재 조회 Axis

따라서 현재 문제 위치는 Homing attained / Target reached / ActualPosition 완료판정 쪽보다 앞단인 ProcessAxisDs402Home()의 ownership / preemption / cleanup fail-closed 경로입니다.

가장 의심되는 곳은 PENDING_FREEZE → 1초 후 stage 101입니다

현재 GitHub dev 코드에는 아래 경로가 있습니다.

preemptionResult :=
    AxisOwnership.CopyAxisOwnershipPreemption(...);

if preemptionResult = LMC_DIAG_PREEMPT_PENDING_FREEZE then

    serviceStart := Ds402HomeState[118]$UDINT;

    cleanupExpired :=
        (serviceStart <> 0) &
        ((ops.tAbsolute - serviceStart) >= 1000);

    if cleanupExpired then
        Ds402HomeState[92] := 101;
    end_if;

    RETURN;
end_if;

즉 Safety owner가 RESERVED 상태로 남아 PENDING_FREEZE가 반환되면 DS402 Home은 진행을 전부 멈추고, Home service 시작 후 1초가 지나면 바로 quarantine 101로 갑니다.

이번 실제 시간과 굉장히 잘 맞습니다.

Home Start       16:19:34.910
Outcome 실패     16:19:37.049

약 2.14초

특히 Home 직전에 Power Off를 실행했습니다. PowerOff는 현재 ownership 설계상 safety-preemption 명령입니다. 기존 owner를 SafetyPreempting으로 전환하고 cleanup 증명 후에만 release하는 구조입니다.

따라서 현재 가장 강한 가설은:

Power Off
→ physical PowerOff는 정상 완료
→ ownership/preemption 쪽 safety reservation 또는 overlay가 완전히 정리되지 않음
→ DS402 Home Start는 일단 Accepted
→ ProcessAxisDs402Home()에서
   CopyAxisOwnershipPreemption() = PENDING_FREEZE
→ DS402 state machine 진행 정지
→ 1초 fail-closed
→ Ds402HomeState[92] = 101
→ ReadOutcome DetailCode 26

입니다.

다만 PowerOff가 원인이라고 아직 100% 확정할 단계는 아닙니다. 같은 101로 들어가는 다른 경로도 있습니다.

preemptionResult < 0, invalid preemption snapshot, owner token/generation/session 불일치, Ds402HomeState[125] <> 2, cleanup/receipt publication 실패 등도 즉시 101을 만듭니다.

그리고 중요한 문제가 하나 더 있습니다: GitHub dev에는 방금 수정한 코드가 없습니다

현재 원격 dev HEAD는 아직:

420c20f75c6a4ac8b946a7834588e173a39ec170
2026-09-10
dev : update hw

입니다.

즉 앞에서 로컬에서 수정했다는:

Target reached 완화
ActualPosition ±1
cleanup 1초 → 5초
원래 timeout 원인 보존

변경은 현재 GitHub의 dev에서는 확인되지 않습니다.

실제로 원격 dev의 Home stage 23은 아직도 다음을 전부 요구합니다.

Homing attained
AND Target reached
AND ActualPosition == HomePosition
AND 3 stable samples

cleanup 쪽도 여전히 1000 ms입니다.

cleanupExpired :=
    ...
    ((serviceNow - cleanupStart) >= 1000);

따라서 현재 PLC에 실제로 어느 소스가 Build/Download되어 있는지가 중요합니다. 만약 16:12에 수정한 로컬 소스를 LASAL에서 빌드해서 다운로드했다면 GitHub source와 PLC runtime이 서로 다릅니다.

지금 Watch에서 보면 바로 원인을 분기할 수 있습니다

이번에는 아래 값들을 DetailCode 26이 뜬 직후 그대로 보면 됩니다.

Ds402HomeState[92], [93] — [92]는 현재 사실상 101 예상
[94], [113] — SDO pending/orphan drain 상태
[106], [107], [108] — 실행 결과 / failure detail / native failure
[114], [115] — 마지막 StatusWord / ActualPosition
[116] — CSP mode 8 복귀 미완료 여부
[117] — RT Home control-word owner 보유 여부
[118], [119] — 특히 중요
[120]~[124] — Session / RequestSequence / AdmissionToken / OwnerGeneration / AxisMask
[125], [126], [127] — ownership receipt phase / dispatch 여부 / uncertainty

여기서 진단이 꽤 명확하게 갈립니다.

[118] = 0x50424631이면 PBF1 magic입니다. 이 경우는 AxisOwnership에 terminal/quarantine 결과를 publish하는 과정 자체가 실패한 것이고 [119]에 실제 ownerResult가 남습니다. 이 magic의 정의와 저장 계약이 코드에 명시돼 있습니다.

반대로 [118]이 PBF1이 아니고 [116]/[117]/[94]/[113]/[127]도 특별한 이상이 없는데 약 1초 만에 101로 갔다면, CopyAxisOwnershipPreemption()의 PENDING_FREEZE 또는 owner tuple 검증 경로가 1순위입니다.

현재 결론

이 실행에서는 이전의 TargetReached=0 / Position=+1 문제가 첫 번째 장애물이 아닙니다.

현재 실패 지점은 이쪽입니다.

0x7D15 Start Accepted
        ↓
DS402 durable record = Running
        ↓
ProcessAxisDs402Home
        ↓
Ownership / Preemption 처리
        ↓
fail-closed
        ↓
Ds402HomeState[92] = 101
        ↓
0x7D16 ReadOutcome
        ↓
DetailCode = 26

따라서 지금 Home 완료조건을 또 수정하면 안 됩니다. 먼저 왜 [92]가 101로 들어갔는지를 잡아야 합니다. 또한 이 상태에서 Home을 반복 실행하거나 record를 강제 Resolve하는 것도 하지 않는 게 맞습니다.

위 Watch 값 중 특히 **[92], [94], [106]~[127]**을 그대로 캡처해서 보내주시면, 다음 답변에서 101로 진입한 if 분기를 코드 기준으로 거의 정확히 하나까지 좁힐 수 있습니다.

@
GitHub
 

원인 확정됐습니다. 이번 캡처는 PowerOff 잔여 preemption 문제가 아니라 stage 20의 사전 판정이 잘못돼 Home start bit를 올리기도 전에 실패한 경우입니다.

캡처 값을 코드와 맞춰보면 증거가 정확합니다.

Slot	값	의미
[92]	101	quarantine
[104]	0	Home start bit HIGH가 아직 dispatch되지 않음
[111]	0	stage 20 사전검사가 통과하지 못함
[106]	3	일반 실행 실패
[107]	30	DS402 Home execution failure
[108]	4787 = 0x12B3	실패 당시 StatusWord
[114]	691 = 0x02B3	cleanup 후 현재 StatusWord
[116]	0	CSP mode 8 복귀 완료
[117]	0	RT Home control owner release 완료
[94]/[113]/[127]	0/0/0	SDO pending/orphan uncertainty 없음
[126]	1	이미 SDO dispatch가 발생했음

핵심은 0x12B3입니다.

0x12B3
  bit12 = 1   Homing attained
  bit13 = 0   Homing error 없음
  bit10 = 0   Target reached 없음
  base state = 0x23 = Switched On

현재 dev의 stage 20은 Mode 6 진입을 확인한 다음 Home start bit 4를 올리기 전에 bit12와 bit13이 둘 다 0이어야 한다고 검사합니다.

20:
    if newCycle then
        if (statusWord and 0x3000) = 0 then
            Ds402HomeState[111] := 1;
            Ds402HomeState[92] := 21;
        else
            failure := TRUE;
            failureNative := statusWord;
        end_if;
    end_if;

즉 0x3000 = bit12 | bit13입니다. 이번 드라이브는 Mode 6으로 전환된 시점에 이미 Homing attained(bit12)=1을 반환했고, 코드가 그것을 오류로 판단했습니다. 캡처의 [108]=0x12B3은 바로 이 failureNative := statusWord에 저장된 값입니다.

더 결정적인 것이 [104]=0, [111]=0입니다. stage 20을 통과했으면 [111]=1이 되고 stage 21에서 SubmitDs402HomeControl(... Command:=2)를 실행하면서 [104]에 cycle을 넣습니다. 현재 둘 다 0이므로 stage 21까지 가지 못했습니다. 즉 실제 Homing start bit HIGH 자체가 실행되지 않았습니다.

따라서 실행 흐름은 정확히 이겁니다.

0x6061 baseline 확인
→ 0x607C = 0
→ 0x6098 = 37
→ RT Home owner 획득
→ 0x6060 = 6
→ 0x6061 = 6 확인
→ StatusWord = 0x12B3
→ stage 20에서 (0x12B3 & 0x3000) != 0
→ bit12=1 때문에 failure
→ Home start bit4 HIGH는 실행되지 않음
→ cleanup
→ quarantine 101

그리고 이번 캡처는 cleanup 자체는 상당히 정상적으로 끝났다는 것도 보여줍니다. [116]=0이라 CSP mode 복귀가 증명됐고, [117]=0이라 RT owner도 내려갔고, [94]/[113]/[127]=0이라 SDO retirement uncertainty도 없습니다. 마지막 SDO 정보 [96]=24673=0x6061, [98]=1도 CSP mode readback까지 수행된 흔적과 일치합니다.

그런데 [126]=1 때문에 최종적으로 무조건 quarantine됩니다. 현재 cleanup stage 100은 SDO dispatch가 한번이라도 보고된 경우 실패를 safe failure로 terminal 처리하지 않고 quarantine하도록 명시돼 있습니다.

// Once an SDO dispatch was reported, a non-success path
// remains quarantined instead of claiming safe release.
...
if Ds402HomeState[126] <> 0 then
    Ds402HomeState[92] := 101;
end_if;

따라서 101은 원인이 아니라 stage 20의 false failure 이후 보수적 cleanup 정책에 의해 만들어진 2차 결과입니다.

수정해야 할 핵심

앞서 수정하려던 Target reached와 ActualPosition ±1보다 한 단계 앞을 먼저 고쳐야 합니다.

현재:

if (statusWord and 0x3000) = 0 then

이 조건은 Method 37에 맞지 않습니다. Homing attained(bit12)가 이미 1인 것을 오류로 취급해서는 안 됩니다. 여기서 실제 fatal condition은 Homing error(bit13)입니다.

따라서 Method 37에서는 최소한 다음 방향이어야 합니다.

if (statusWord and 0x2000) = 0 then
    // Homing error 없음
    Ds402HomeState[111] := 1;
    Ds402HomeState[92] := 21;
else
    failure := TRUE;
    failureNative := statusWord;
end_if;

다만 이것만 바꾸고 끝내면 안 됩니다. bit12가 Home 시작 전부터 이미 1이므로, 완료 판정에서 “bit12=1” 자체를 새 Home 완료 증거로 사용하면 stale attained를 오인할 수 있습니다.

Method 37 완료 판정은 다음 식으로 묶는 것이 맞습니다.

Home start bit HIGH가 실제 dispatch됨
+ 그 이후 fresh latch cycle
+ Homing error(bit13) = 0
+ Homing attained(bit12) = 1
+ ActualPosition가 HomePosition 허용오차 내
+ 연속 3 fresh sample 안정

Target reached(bit10)은 이번 실제 Elmo 동작처럼 0으로 남을 수 있으므로 Method 37에서는 필수 조건에서 제외하는 방향이 맞습니다. 현재 stage 23은 여전히 bit12 + bit10 + exact position을 모두 요구하고 있습니다.

그리고 이번 캡처의 ActualPosition=756,586,606은 별도 좌표 오염으로 볼 필요가 없습니다. [102]도 같은 값이고 [103]=3이므로 Home 시작 전 현재 위치가 세 fresh sample 동안 안정적이었다는 의미입니다. stage 21까지 도달했다면 [102]를 다시 설정하고 [103]=0으로 초기화하는데, 현재 그러지 않았습니다. 이것도 stage 20에서 막혔다는 추가 증거입니다.

결론적으로 이번 문제는 stage 20에서 Homing attained=1을 불법 상태로 취급하는 로직이 1차 결함입니다. 이전에 봤던 Target reached=0 / Position=+1은 이걸 통과한 다음 단계에서 만나는 2차 결함입니다.

수정 순서는 stage20 bit12 사전조건 제거 → stage23 Method37 완료조건 완화 → cleanup 원인 보존/시간창 수정이 맞습니다. 현재 캡처만으로 이 순서는 확정할 수 있습니다.

확인했습니다. 지금 dev HEAD는 **23fc465450d85bacd1ed0e282f10384d947ab6c7 (dev : source code edit)**이고, 이전 커밋 대비 실제로 수정된 런타임 파일은 LMCDiagnosticsService.st와 LmcAdminDs402HomeOutcomeModels.cs입니다.

결론부터 말하면 말씀하신 1~3번 구현은 방향과 내용 모두 맞습니다. 지금 남은 필수 작업은 거의 전부 PC Contract Test 정합입니다. 4번을 못 찾으신 이유도 4번이 PLC/LASAL 코드가 아니라 LMC_API_Delivery/tests 쪽 테스트 코드이기 때문입니다.

현재 1~3번 구현 검토 결과

먼저 Stage 20은 정상적으로 수정됐습니다. 기존 0x3000 검사에서 0x2000 검사로 변경돼서 Homing attained(bit12)가 이미 1이어도 통과하고, Homing error(bit13)만 거부합니다.

현재 구현:

20:
    if newCycle then
        // Method 37 may enter Homing mode with HomingAttained(bit12)
        // already set from the previous drive state.
        // Pre-start validation rejects HomingError only.
        // Fresh completion evidence is evaluated after bit4 HIGH.
        if (statusWord and 0x2000) = 0 then
            Ds402HomeState[111] := 1;
            Ds402HomeState[92] := 21;
        else
            failure := TRUE;
            failureNative := statusWord;
        end_if;
    end_if;

실제 커밋에도 정확히 이 변경이 들어갔습니다.

그리고 Fault bit3과 허용 DS402 base state 검사는 stage 20 진입 전에 공통 precondition에서 이미 하고 있으므로 여기서 bit3를 다시 검사하지 않은 것도 문제없습니다. 현재 공통 검사는 Fault와 0x40/0x21/0x23/0x27를 별도로 확인합니다.

두 번째, PLC HandleAxisDs402HomeOutcome()의 ActualPosition 검증도 제대로 0 ±1 count로 변경됐습니다.

// Method 37 successful zero position is 0 +/- 1 count.
(Ds402HomeState[recordBase + 18] >=
    (0 - LMC_DIAG_DS402_HOME_POSITION_TOLERANCE)) &
(Ds402HomeState[recordBase + 18] <=
    LMC_DIAG_DS402_HOME_POSITION_TOLERANCE) &

커밋 diff에도 정확히 반영돼 있습니다.

세 번째, SDK 쪽 LMCAxisDs402HomeOutcomeSemantics.IsSucceeded()도 맞습니다.

private const int HomePositionToleranceCounts = 1;

...

&& actualPosition >= -HomePositionToleranceCounts
&& actualPosition <= HomePositionToleranceCounts

이것 역시 현재 dev에 반영돼 있습니다.

그리고 이것은 실제 parser에서 사용됩니다. Succeeded 결과를 받으면 IsValidDs402HomeOutcome()이 결국 LMCAxisDs402HomeOutcomeSemantics.IsSucceeded()를 호출하므로 SDK/parser까지 동일한 ±1 계약으로 동작합니다.

즉 1~3은 다시 건드릴 필요 없습니다.

4번은 여기 있습니다 — 기존 테스트가 지금 새 구현과 충돌합니다

수정할 파일은 정확히 이것입니다.

LMC_Library/
  LMC_API_Delivery/
    tests/
      LasalMotionControlLib.Tests/
        AdminDs402HomeOutcomeRetirementContractTests.cs

이 파일은 이미 테스트 프로젝트에 포함되어 있고, Program.cs에서도 Register()가 호출됩니다. 따라서 새 테스트 파일이나 별도 프로젝트를 만들 필요가 없습니다.

현재 이 파일에 새 구현과 모순되는 코드가 2개 남아 있습니다.

첫 번째는 SucceededEvidenceFailsClosed()의 mutation 목록입니다.

현재:

var mutations = new Action<byte[]>[]
{
    payload => TestFrame.WriteUInt16(payload, 60, 1),
    payload => TestFrame.WriteInt16(payload, 62, -31000),
    payload => TestFrame.WriteUInt32(
        payload,
        64,
        (uint)LMCAdminDetailCode.Ds402HomeExecutionFailed),
    payload => TestFrame.WriteUInt16(payload, 68, 0),
    payload => TestFrame.WriteUInt16(payload, 68, 0x0068),
    payload => TestFrame.WriteUInt16(payload, 68, 0x2027),

    payload => TestFrame.WriteInt32(payload, 72, 1),

    payload => TestFrame.WriteUInt32(payload, 76, 0),
    payload => TestFrame.WriteUInt32(payload, 80, 99),
    payload => TestFrame.WriteUInt32(payload, 84, 1),
    payload => TestFrame.WriteUInt32(payload, 88, 0)
};

현재 테스트는 ActualPosition = +1을 넣고 parser가 InvalidDataException을 던져야 한다고 기대합니다.

하지만 방금 SDK를 ±1 허용으로 고쳤으므로 이제 +1은 정상 값입니다. 따라서 이 테스트는 반드시 실패하게 됩니다.

여기만:

payload => TestFrame.WriteInt32(payload, 72, 1),

에서:

payload => TestFrame.WriteInt32(payload, 72, 2),

로 변경하세요.

+2부터는 허용범위 밖이므로 FailsClosed 테스트 의미도 그대로 유지됩니다.

두 번째도 같은 메서드 아래쪽입니다.

현재:

var invalidPublicResult = new LMCAxisDs402HomeOutcomeResult(
    validParsed.Response,
    validKey,
    validParsed.RecordState,
    validParsed.OriginalCommandStatus,
    validParsed.OriginalErrorId,
    validParsed.OriginalDetailCode,
    validParsed.Ds402StatusWord,
    1,
    validParsed.StartCycle,
    validParsed.CompletionCycle,
    validParsed.NativeCommandState,
    validParsed.RecordGeneration);

AssertEx.False(invalidPublicResult.HomingSucceeded);

현재도 ActualPosition=1을 넣고 HomingSucceeded == false를 기대하고 있습니다.

이것도:

1,

을:

2,

로 바꾸면 됩니다.

즉 4번의 필수 수정은 사실 이 두 줄입니다.

하지만 테스트는 여기서 한 단계 더 보강하는 게 좋습니다

단순히 1 → 2만 바꾸면 기존 테스트는 다시 통과하겠지만, 다음 사람이 ±1 계약을 모르고 actualPosition == 0으로 되돌려도 바로 탐지할 수 있는 명시적인 positive test가 없습니다.

같은 파일의 Register()에 하나 추가하세요. 현재 이 클래스는 각 테스트를 이 방식으로 등록합니다.

tests.Add(
    "Response.Admin.Ds402HomeOutcome.PositionTolerance",
    SucceededPositionTolerance);

그리고 SucceededEvidenceFailsClosed() 바로 다음 정도에 아래 메서드를 추가하는 것을 권장합니다.

private static void SucceededPositionTolerance()
{
    foreach (var actualPosition in new[] { -1, 0, 1 })
    {
        var key = RecoveryKey();

        var payload = OutcomePayload(
            17,
            key,
            LMCAxisDs402HomeOutcomeRecordState.Succeeded,
            0,
            0,
            LMCAdminDetailCode.None,
            PostCleanupStatusWord,
            100,
            200,
            0,
            RecordGeneration);

        TestFrame.WriteInt32(payload, 72, actualPosition);

        var parsed = LMC_AdminParser.ParseAxisDs402HomeOutcome(
            TestFrame.Response(0, payload),
            17,
            key);

        var result = new LMCAxisDs402HomeOutcomeResult(
            parsed.Response,
            key,
            parsed.RecordState,
            parsed.OriginalCommandStatus,
            parsed.OriginalErrorId,
            parsed.OriginalDetailCode,
            parsed.Ds402StatusWord,
            parsed.ActualPosition,
            parsed.StartCycle,
            parsed.CompletionCycle,
            parsed.NativeCommandState,
            parsed.RecordGeneration);

        AssertEx.True(result.HomingSucceeded);
        AssertEx.Equal(actualPosition, result.ActualPosition);
    }

    foreach (var actualPosition in new[] { -2, 2 })
    {
        var key = RecoveryKey();

        var payload = OutcomePayload(
            18,
            key,
            LMCAxisDs402HomeOutcomeRecordState.Succeeded,
            0,
            0,
            LMCAdminDetailCode.None,
            PostCleanupStatusWord,
            100,
            200,
            0,
            RecordGeneration);

        TestFrame.WriteInt32(payload, 72, actualPosition);

        AssertEx.Throws<InvalidDataException>(
            () => LMC_AdminParser.ParseAxisDs402HomeOutcome(
                TestFrame.Response(0, payload),
                18,
                key));
    }
}

OutcomePayload()는 원래 P72 ActualPosition을 0으로 작성하고 있기 때문에 위처럼 생성 후 P72만 override하면 됩니다. 기존 helper 구조와도 맞습니다.

이렇게 하면 계약이 명확해집니다.

-1 → PASS
 0 → PASS
+1 → PASS

-2 → InvalidDataException
+2 → InvalidDataException

이 테스트가 이번 변경의 핵심 regression guard입니다.

추가로 수정해야 하는 1곳: parser 주석

실행에는 영향을 안 주지만 지금 주석이 실제 구현과 달라졌습니다.

현재 LmcAdminDs402HomeOutcomeProtocol.cs에는:

// Succeeded is committed only after the PLC observed fresh
// homing-attained and target-reached samples. P68 is the last
// StatusWord after CSP cleanup, where bits 10/12 no longer
// carry the homing-mode meaning...

라고 되어 있습니다.

그런데 지금 계약은 Switch On Disabled에서는 Target reached를 필수로 하지 않습니다.

주석을 다음 정도로 바꾸세요.

// Succeeded is committed only after the PLC observed fresh
// homing-attained, no homing-error, method-37 completion status,
// and ActualPosition within the accepted zero-position tolerance.
// For method 37 in Switch On Disabled, TargetReached is not required.
// P68 is the last StatusWord after CSP cleanup, where bits 10/12
// no longer carry the homing-mode meaning, so validate only
// invariants that remain valid after the mode-8 restore.

기능 수정은 아니지만 안 바꾸면 다음 유지보수 때 다시 bit10 필수 조건을 넣을 가능성이 높습니다.

AdminDs402HomeH37QualificationTests.cs는 필수 수정은 아닙니다

여기도 현재 성공 terminal 샘플이:

statusWord = 0x0027
actualPosition = 0

으로 되어 있습니다.

이 자체는 틀리지 않습니다. 0 역시 허용범위 안이니까요.

다만 qualification test가 실제 실축에서 확인한 +1 case도 포함하도록 하고 싶다면 별도의 test를 추가하는 것이 좋습니다. 기존 RunningTerminalRetireSequence()의 0을 1로 그냥 바꾸는 것보다는 기존 zero case는 유지하고 별도 tolerance test로 검증하는 것이 낫습니다.

즉 이 파일은 선택 사항이고, 핵심 테스트는 앞에서 말한 AdminDs402HomeOutcomeRetirementContractTests.cs입니다.

정적 검증 스크립트도 보강하는 것을 권장합니다

현재 Verify-HomeDs402H37CurrentDevRegression.ps1은 0x6061/0x607C/0x6098/0x6060 등의 H37 SDO sequence와 activation/ownership을 확인하지만, 이번에 변경한 stage20 0x2000 계약이나 ±1 tolerance를 직접 검사하지는 않습니다.

여기에 다음 정도를 추가하면 좋습니다.

Assert-Match `
    $homeSourceBlock `
    '(?s)20:\s*if\s+newCycle\s+then.*?\(statusWord\s+and\s+0x2000\)\s*=\s*0' `
    'H37 pre-start rejects HomingError without requiring HomingAttained low'

Assert-NoMatch `
    $homeSourceBlock `
    '(?s)20:.*?\(statusWord\s+and\s+0x3000\)\s*=\s*0' `
    'H37 pre-start no longer requires HomingAttained bit12 to be low'

Assert-Match `
    $diagnostics `
    '#define\s+LMC_DIAG_DS402_HOME_POSITION_TOLERANCE\s+1' `
    'H37 method37 zero-position tolerance remains +/-1 count'

Assert-Match `
    $homeSourceBlock `
    'homePositionValid\s*:=' `
    'H37 runtime uses shared home-position validity'

Assert-Match `
    $diagnostics `
    '(?s)baseOutcomeState\s*=\s*2.*?LMC_DIAG_DS402_HOME_POSITION_TOLERANCE' `
    'H37 retained succeeded outcome uses the same +/-1 tolerance'

이건 반드시 런타임을 작동시키기 위해 필요한 수정은 아니지만, 이번에 고친 핵심 조건을 나중에 되돌리는 regression을 막아줍니다.

건드리지 않아도 되는 부분

이번 수정 때문에 Stage 23, Stage 34, cleanup 로직을 다시 수정할 필요는 없습니다.

현재 Stage 23은 이미:

Homing attained
+ no Homing error
+ method37 completion status
+ ActualPosition 0 ±1
+ stable samples

형태로 변경돼 있고, Switch On Disabled(0x40)에서는 TargetReached를 강제하지 않도록 homeCompletionStatusValid가 구성돼 있습니다.

최종 Stage 34도 이미 homePositionValid을 이용하므로 ±1 정책이 그대로 적용됩니다.

cleanup도 이미:

LMC_DIAG_DS402_CLEANUP_TIMEOUT_MS = 5000

이고, 후속 cleanup error가 들어와도 [108]이 0인 경우에만 덮어쓰도록 변경돼 있습니다.

또 RetirementResult.HomingSucceeded는 별도로 ±1 로직을 넣지 않아도 됩니다. Retire 응답도 먼저 공통 ParseAxisDs402HomeOutcomeSuccess()를 통과하므로 허용범위 밖의 성공 레코드는 결과 객체가 만들어지기 전에 거부됩니다.

지금 수정 순서는 이렇게 잡으면 됩니다
AdminDs402HomeOutcomeRetirementContractTests.cs
P72=1 failure mutation → P72=2
invalidPublicResult ActualPosition=1 → 2
같은 파일에 SucceededPositionTolerance() 추가
-1,0,+1 PASS
-2,+2 FAIL
LmcAdminDs402HomeOutcomeProtocol.cs
오래된 target-reached samples 주석 수정
가능하면 Verify-HomeDs402H37CurrentDevRegression.ps1
stage20 0x2000
0x3000 금지
±1 tolerance 고정 검사 추가

그 뒤 PC contract test를 돌리면 됩니다. 이 테스트 프로젝트는 .NET Framework 4.8 executable이고 RunPcTests target이 정의돼 있습니다.

예를 들면 저장소 루트에서:

msbuild LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\LasalMotionControlLib.Tests.csproj /t:RunPcTests /p:Configuration=Debug

그리고 정적 H37 검증:

powershell -ExecutionPolicy Bypass -File tools\Verify-HomeDs402H37CurrentDevRegression.ps1

까지 PASS하면 다시 LASAL Rebuild/Download 후 실축 시험으로 넘어가면 됩니다.

현재 상태를 요약하면 런타임 1~3번 수정은 잘 됐고, 지금 가장 명확하게 남아 있는 오류는 기존 테스트가 ActualPosition=+1을 아직 실패로 간주한다는 점입니다. 이 테스트 정합부터 고치면 됩니다.

내가 테스트 할꺼도 아닌데 테스트 코드를  바꿀 필요가 있냐?

네. 직접 실축에서 동작 확인할 목적이라면 테스트 코드는 지금 안 바꿔도 됩니다.

제가 앞에서 4번을 “필수”처럼 말한 건 과했습니다. 지금 목적을 기준으로 구분하면 이렇습니다.

실제 동작에 필요한 수정: 이미 한 1~3번
Stage 20의 0x3000 → 0x2000
PLC Outcome의 ActualPosition 0±1
C# Outcome semantics의 ActualPosition 0±1
실제 동작에 영향 없는 것: 테스트 코드 수정, 정적 검증 스크립트 수정, 주석 수정

AdminDs402HomeOutcomeRetirementContractTests.cs 같은 파일은 말 그대로 별도의 PC 테스트 프로젝트용입니다. PLC에 다운로드되는 코드도 아니고 WPF 런타임 동작 코드도 아닙니다. 테스트 실행 시에만 현재 새 계약과 충돌합니다.

그래서 지금은 테스트 코드는 건드리지 말고 LASAL 빌드/다운로드해서 실축 확인부터 진행해도 됩니다.

현재 실제 실행 경로에서 중요한 변경은 이미 들어가 있습니다. Stage 20은 이제 HomingAttained(bit12)가 기존에 1이어도 허용하고 HomingError(bit13)만 차단하도록 바뀌었습니다. PLC terminal outcome도 위치를 정확히 0이 아니라 0±1 count로 인정하도록 변경됐고, SDK의 HomingSucceeded 판정도 동일하게 맞춰졌습니다.

따라서 지금 추가 코드 수정 없이 바로 다음 실축 시험으로 가는 게 맞습니다.

이번 시험에서 Watch는 다음 값만 보면 충분합니다.

Ds402HomeState[92]   // 현재 stage
[103]                // stable sample count
[104]                // bit4 HIGH dispatch cycle
[106]                // runtime result
[107]                // failure detail
[108]                // native failure
[111]                // stage20 pre-start validation 통과 여부
[114]                // StatusWord
[115]                // ActualPosition
[116]                // CSP restore pending
[117]                // RT Home control owner

특히 이번 수정이 제대로 작동하면 이전처럼:

[111] = 0
[104] = 0
→ stage20 실패

가 아니라 최소한:

[111] = 1
[104] != 0

까지 가야 합니다.

즉 이번 시험의 첫 번째 체크포인트는 “실제 bit4 HIGH까지 진행했는가”입니다.

그 이후 예상 흐름은:

20
→ 21
→ 22/23
→ Homing attained 확인
→ ActualPosition 0±1 안정
→ 24
→ bit4 LOW
→ 26~31
→ CSP 8 복귀
→ 32~34
→ terminal success

입니다.

테스트 코드는 나중에 기능이 실축에서 정상 동작하는 것이 확인된 뒤, 코드 정리/회귀 방지 단계에서 Codex에 한 번에 정리시키면 됩니다. 지금은 PLC Rebuild → Download → DS402 Home 재시험이 우선입니다.

@
GitHub
 빌드해서 테스트해봐도 동일하네

이번 캡처를 보면 겉으로는 동일하게 멈춘 것 같지만 내부적으로는 이전 문제는 해결됐습니다. 지금은 다음 단계인 stage 23에서 막힌 상태입니다.

현재 Watch 값이 결정적입니다.

[92]  = 23          ← 현재 stage 23
[103] = 0           ← 완료 안정 샘플이 하나도 카운트되지 않음
[104] = 37629       ← bit4 HIGH dispatch 완료
[111] = 1           ← 우리가 수정한 stage20 통과
[114] = 4787        ← StatusWord = 0x12B3
[115] = 1           ← ActualPosition = +1
[116] = 1           ← CSP 복귀 아직 필요
[117] = 1           ← Home RT owner 보유 중
[106] = 0
[107] = 0
[108] = 0           ← 아직 failure 자체는 발생하지 않음

즉 지난번의 [111]=0, [104]=0과 완전히 다릅니다. Stage 20 수정은 제대로 먹었고 bit4 HIGH까지 실제 실행됐습니다. 현재 소스도 stage20에서 bit13만 검사한 뒤 21로 넘어가고, stage21/22에서 bit4 HIGH를 적용한 뒤 stage23으로 진입하도록 되어 있습니다.

문제는 이제 정확히 homeCompletionStatusValid입니다.

4787 = 0x12B3를 해석하면:

DS402 base state = 0x23  (Switched On)
bit12 Homing attained = 1
bit13 Homing error    = 0
bit10 Target reached  = 0
Fault bit3            = 0
ActualPosition        = +1

즉 Home 자체의 결과는 상당히 좋습니다. bit12가 올라왔고, error가 없고, 실제 위치도 우리가 허용한 0±1 안의 +1까지 왔습니다.

그런데 현재 코드가 이렇게 되어 있습니다.

homeCompletionStatusValid := (statusWord and 0x0400) <> 0;

if (Ds402HomeState[recordBase + 9] = 37) &
   (baseState = 0x0040) then
    homeCompletionStatusValid := TRUE;
end_if;

이번 값을 넣어보면:

bit10 = 0
→ 첫 줄 FALSE

baseState = 0x23
→ 0x40 예외에도 해당 안 됨

결과:
homeCompletionStatusValid = FALSE

그래서 stage23에서:

elsif (Ds402HomeState[111] <> 0) &
      ((statusWord and 0x1000) <> 0) &
      homeCompletionStatusValid then

앞의 두 조건은 TRUE인데 마지막 하나만 FALSE입니다. 결국 매 cycle마다 아래로 떨어집니다.

else
    Ds402HomeState[103] := 0;
end_if;

그래서 지금 정확히 [103]=0, [92]=23에서 계속 멈춰 있는 겁니다.

이번에 고쳐야 할 곳

현재 설계가 잘못 가정한 것은 Power Off 상태이면 DS402 base state가 0x40 Switch On Disabled일 것이라는 부분입니다.

그런데 프로젝트의 Power Off 계약 자체는 실제로 PowerOn=false + Standstill=true + AxisError=0만 요구하지, DS402가 반드시 0x40이어야 한다고 정의하지 않습니다.

실축에서는 지금 명확하게:

Power Off + Standstill
→ DS402 0x23 Switched On

으로 관찰됐습니다.

따라서 Target reached 면제를 0x40에만 적용한 것이 현재 두 번째 버그입니다.

제가 권장하는 수정은 이겁니다.

homeCompletionStatusValid := (statusWord and 0x0400) <> 0;

// Method 37 current-position homing may complete without TargetReached
// while the drive is not Operation Enabled.
// PowerOff/Standstill has been physically observed as 0x23 on this Elmo.
if (Ds402HomeState[recordBase + 9] = 37) &
   ((baseState = 0x0040) |
    (baseState = 0x0021) |
    (baseState = 0x0023)) then

    homeCompletionStatusValid := TRUE;
end_if;

왜 0x40과 0x23만이 아니라 0x21까지 넣었냐면 DS402에서 셋 다 Operation Enabled가 아닌 상태이기 때문입니다.

0x40 = Switch On Disabled
0x21 = Ready to Switch On
0x23 = Switched On
0x27 = Operation Enabled

반대로 0x27 Operation Enabled에서는 기존처럼 bit10 Target reached를 요구하도록 남겨둡니다. 즉 안전 조건을 무작정 없애는 것이 아니라 Servo가 실제 Operation Enabled가 아닌 Method 37에 한해서 bit10 의미를 강제하지 않는 것입니다.

현재 물리 결과만 최소 수정하고 싶으면 이것도 가능합니다.

if (Ds402HomeState[recordBase + 9] = 37) &
   ((baseState = 0x0040) |
    (baseState = 0x0023)) then

    homeCompletionStatusValid := TRUE;
end_if;

하지만 저는 위의 0x40/0x21/0x23 세 상태를 권장합니다. Power Off 계약이 애초 특정 DS402 base state를 고정하지 않기 때문입니다.

이 수정은 현재 데이터 기준으로 바로 효과가 납니다

수정 후 지금 캡처의 상태를 그대로 대입하면:

StatusWord = 0x12B3

bit13 HomingError = 0         PASS
bit12 HomingAttained = 1      PASS
baseState = 0x23              PASS (새 조건)
ActualPosition = +1           PASS (±1)

그러면 stage23의 조건이 전부 TRUE가 되고 [103]이:

0 → 1 → 2 → 3

으로 증가한 뒤:

if Ds402HomeState[103] >= 3 then
    Ds402HomeState[92] := 24;
end_if;

로 넘어갑니다.

그 다음은 이미 구현되어 있습니다. stage24에서 bit4를 LOW로 내리고, 이후 setpoint alignment, CSP 복귀, RT owner release를 거쳐 stage34에서 최종적으로 Fault/HomingError 없음과 homePositionValid를 확인하고 success를 publish합니다. Stage34도 이미 homePositionValid을 사용하기 때문에 +1은 허용됩니다.

그리고 bit4 HIGH 자체는 걱정할 필요 없습니다. SubmitDs402HomeControl()은 요청을 mailbox에 넣고, GetDs402HomeControlState()가 applied sequence와 실제 결과를 확인한 뒤에야 0을 반환하는 구조입니다. 실제 RT 측에서는 OperationModeSpec1을 쓰고 다시 ControlWord를 읽어 원하는 값과 일치하는지도 검사합니다.

따라서 현재는 Stage20, ±1 tolerance 수정은 그대로 유지하고 homeCompletionStatusValid 한 군데를 수정하는 게 다음 조치입니다.

이번 수정 후 Watch에서 가장 먼저 볼 것은 [103]입니다. 1 → 2 → 3으로 올라가고 [92]가 23 → 24로 바뀌면 현재 병목은 정확히 해결된 것입니다. 그다음 새로 막히는 stage가 있다면 그 위치부터 이어서 보면 됩니다.
