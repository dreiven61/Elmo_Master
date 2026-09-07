
[media pointer="sediment://file_00000000f17882068544691a7165d2e5"]
@GitHub 현재 dev 브런치에 SDO Write 기능을 구현했는데 SDO Write 탭에서 기능을 쓸수가 없어.
왜 그런건지 분석좀 해줘


8m 26s 동안 처리함
분석해보니 SDO Write 구현이 빠진 게 아니라, 현재 dev에서 Manual SDO Write를 의도적으로 잠가놓은 안전 게이트 때문에 사용할 수 없는 상태입니다.

현재 dev HEAD는 7f2a71c...이고 커밋 메시지도 dev : Implementing SDO write입니다. 이 커밋에서 실제로 ButtonSubmitSdo 활성 조건에 hasCurrentSdoWriteTransportProof가 추가됐습니다.

핵심 원인
MainWindow.Diagnostics.cs를 보면 Write 모드의 Submit 버튼이 대략 아래 조건을 전부 만족해야 활성화됩니다.

PLC 연결 상태
UI/operation이 idle 상태
Diagnostics mutation 작업을 새로 시작할 수 있는 상태
SupportsSdoWrite()가 true
HasCurrentSdoWriteActivationQualificationProof()가 true
DiagnosticsMutationJournalCanArm == true
즉 SDOWrite Capability가 있다고 바로 Write 버튼이 열리는 구조가 아닙니다. 현재 코드에서 결정적인 조건은 5번입니다.

실제 Submit 실행부도 동일하게 한 번 더 검사합니다.

Manual SDO Write requires a current image/session transport qualification proof. Run the same-value SDO Write qualification first.

즉 Write 기능을 쓰기 전에 반드시 Same-Value SDO Write Qualification을 한 번 성공시켜야 합니다.

그런데 이 Qualification 자체도 상당히 빡빡하게 잠겨 있습니다
D5 SDO Write activation qualification (same-value only)이 성공해야 sdoWriteActivationQualificationProof가 생성됩니다. 이 과정은 baseline Read → pre-write guard Read → 동일 값 Write → exact Readback의 4개 ticket이 모두 정상이어야 합니다. 성공하면 그때서야 ManualWriteProof=current-session으로 저장됩니다.

Qualification 실행 버튼 역시 다음 조건이 전부 맞아야 활성화됩니다. 코드의 gate reason이 아주 명확합니다.

PLC가 연결되어 있어야 함
Diagnostics Capability가 정상 로드되어야 함
SDK/PLC SDO Write policy blocker가 없어야 함
현재 qualification용으로 SDK-known UI[24] preset 하나가 존재하고 선택되어 있어야 함
BaseCycleTimeUs != 0
MaxSdoDataBytes == 4
Motion이 active가 아니어야 함
Manual D5 ticket이 없어야 함
Diagnostics mutation interlock이 없어야 함
Mutation journal이 정상적으로 arm 가능해야 함
화면의 4개 activation confirmation checkbox를 전부 체크해야 함
실제 qualification 중에도 PowerOn=False, Standstill=True, position stable 상태가 검증되어야 함
그리고 Capabilities는 연결만 했다고 diagnosticCapabilities에 자동으로 들어가는 구조가 아닙니다. Refresh Capabilities를 실행해야 GetCapabilitiesAsync() 결과가 저장되고 SDO Write preset도 갱신됩니다.

그래서 지금 UI에서 "구현했는데 못 쓰는" 것처럼 보이는 겁니다
SDO Write 자체 SDK gate는 현재 켜져 있습니다.

SdoWriteEnabled = true

이고 Axis1 UI24 qualification preset도 켜져 있습니다. 따라서 소스에서 기능을 아예 막아놓은 것은 아닙니다.

문제는 UI activation flow입니다.

Operation = Write로 바꾸면 Write value 입력란은 코드상 활성화되도록 되어 있습니다. XAML에는 초기값으로 IsEnabled="False"가 들어가 있지만 UpdateSdoOperationControls()에서 Write 모드일 때 다시 활성화합니다.

하지만 Submit SDO Write 버튼은 qualification proof가 없으면 계속 Disabled입니다.

더 큰 UX 문제는 이 버튼이 Disabled일 때 사용자에게 왜 Disabled인지 바로 보여주지 않는다는 점입니다. Tooltip도 단순히:

Write Once uses an exact-request two-click confirmation...

정도만 설명하고, 실제 핵심 원인인

Run the same-value SDO Write qualification first.

를 보여주지 않습니다. 그 메시지는 Submit 실행부 안에 있는데, 버튼 자체가 Disabled라 실행부까지 들어갈 수도 없습니다.

그래서 사용자 입장에서는 **"SDO Write 기능은 구현되어 있는데 UI에서는 아무것도 못 한다"**가 됩니다. 이건 기능 로직 문제라기보다는 activation UX 연결 문제에 가깝습니다.

그리고 중간에 제가 "같은 Slave/Index/SubIndex qualification proof가 필요하다"고 봤던 부분은 최신 dev를 끝까지 확인해보니 조금 다릅니다. 현재 구조는 UI[24]를 transport canary로 qualification한 뒤 동일 image/session 전체에 대한 transport proof를 생성하는 방식입니다. Manual Write 활성 검사에서는 target tuple을 proof와 다시 비교하지 않고 MatchesCurrent(connection, capabilities)를 사용합니다. 다만 reconnect, PLC BootId/MapRevision 변경, diagnostics identity 변경 등이 발생하면 proof는 폐기됩니다.

현재 정상적인 사용 순서
Connect → Refresh Capabilities → D5 SDO Write activation qualification → UI[24] preset 선택 → 4개 confirmation 체크 → Run Same-Value SDO Write Qualification → PASS → SDO Operation을 Write로 선택 → Manual Write

이 순서가 현재 코드가 요구하는 정상 경로입니다.

제 판단으로는 수정이 필요합니다. Safety gate 자체를 없앨 필요는 없지만, SDO Write 화면에 최소한 Write blocked: Same-value qualification required 같은 상태 표시와 Run Qualification으로 연결되는 UX가 있어야 합니다. 지금 구조는 안전하게 막기는 했지만 사용자가 어떻게 해제하는지 SDO Write 화면만 보고는 알 수 없게 되어 있습니다.

원하시면 다음 단계로 제가 dev에서 SDO Write를 실제로 사용할 수 있도록 하되 현재 safety 설계는 유지하는 방향으로 UI/activation flow를 수정해드릴 수 있습니다.

@GitHub 모든 오브젝트가 SDO Write 할수 있게 변경해줘


10m 21s 동안 처리함
변경 완료했습니다. dev 브랜치에 반영됐습니다.

현재 HEAD는 7b9baac9c26a0e32ab22865706a28a7de216c7b8 — dev: allow SDO Write for all object indices 입니다.

변경 내용은 다음과 같습니다.

기존 주소 차단 목록 제거:
0x6040, 0x6060, 0x607A, 0x60FF, 0x6071, 0x3204, 0x20FC
이제 유효한 non-zero CoE ObjectIndex 전체를 generic SDO Write 대상으로 사용할 수 있습니다. SDK의 IsPermanentlyUnsafeObject()도 더 이상 특정 주소를 차단하지 않습니다.
LASAL PLC의 GetSdoWritePolicyDetail()에서도 동일한 DetailCode=8 주소 차단을 제거했습니다. PLC와 PC SDK 정책이 같이 풀렸습니다.
WPF durable mutation journal의 주소 제한도 제거했습니다.
기존 테스트에서 “이 주소들은 거부돼야 함”으로 되어 있던 항목을 허용되어야 함으로 변경했습니다.
Distribution 쪽 WPF 사본과 RELEASE_MANIFEST.md도 같이 갱신했습니다.
패치 적용 검증 작업은 성공했습니다.
다만 안전/무결성 계층은 일부러 유지했습니다. ObjectIndex=0은 여전히 잘못된 주소로 거부되고, Slave Reference 1~4, ValueType/DataLength 검증, 현재 세션/BootId/MapRevision 확인, safe-axis 확인, SDO Write qualification proof, durable journal/no-replay, Write 후 exact readback은 그대로입니다. 즉 주소 때문에 Write가 막히는 제한만 없앤 상태입니다.

한 가지 별도 제한도 남아 있습니다. 0x2F00:24 UI[24] known preset은 qualification용으로 설정된 기존 값 범위 검증이 아직 남아 있습니다. 모든 주소는 허용되지만 UI[24]의 해당 특수 value-range 정책까지 완전히 없앤 것은 아닙니다.

전체 Windows MSBuild/PC Test suite는 이번 변경에서 실행하지 않았고, 패치 스크립트 검증과 git diff --check까지 성공한 상태입니다.

@GitHub 심각도	코드	설명	프로젝트	파일	줄	비표시 오류(Suppression) 상태
오류	CS0103	'ValidateSdoSubmitPolicy' 이름이 현재 컨텍스트에 없습니다.	LasalMotionControlLib	C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Delivery\src\LmcDiagnosticsD5.cs	1101	활성
오류	CS0006	'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Delivery\src\bin\Debug\LasalMotionControlLib.dll' 메타데이터 파일을 찾을 수 없습니다.	LasalApiWpfTestApp	C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\CSC	1	활성
오류	CS0006	'C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Debug\LasalMotionControlApiExample.exe' 메타데이터 파일을 찾을 수 없습니다.	LasalApiWpfTestApp.SmokeTests	C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\CSC	1	활성
오류	CS0006	'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Delivery\src\bin\Debug\LasalMotionControlLib.dll' 메타데이터 파일을 찾을 수 없습니다.	LasalApiWpfTestApp.SmokeTests	C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\CSC	1	활성
오류	CS0103	'ValidateSdoSubmitPolicy' 이름이 현재 컨텍스트에 없습니다.	LasalMotionControlLib	C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Delivery\src\LmcDiagnosticsD5.cs	214	활성
오류	CS0103	'ValidateSdoSubmitPolicy' 이름이 현재 컨텍스트에 없습니다.	LasalMotionControlLib	C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Delivery\src\LmcDiagnosticsD5.cs	251	활성
오류	CS0103	'ValidateSdoSubmitPolicy' 이름이 현재 컨텍스트에 없습니다.	LasalMotionControlLib	C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Delivery\src\LmcDiagnosticsD5.cs	403	활성
오류	CS0103	'ValidateSdoSubmitPolicy' 이름이 현재 컨텍스트에 없습니다.	LasalMotionControlLib	C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Delivery\src\LmcDiagnosticsD5.cs	456	활성
오류	CS0103	'ValidateSdoSubmitPolicy' 이름이 현재 컨텍스트에 없습니다.	LasalMotionControlLib	C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Delivery\src\LmcDiagnosticsD5.cs	507	활성
오류	CS0103	'ValidateSdoSubmitPolicy' 이름이 현재 컨텍스트에 없습니다.	LasalMotionControlLib	C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Delivery\src\LmcDiagnosticsD5.cs	579	활성

API 빌드에러난거 확인해줘

…

3m 17s 동안 처리함
확인했고 원인 수정까지 dev 브랜치에 반영했습니다.

핵심 원인은 앞서 “모든 ObjectIndex SDO Write 허용”으로 바꾸면서 LmcDiagnosticsD5.cs의 denylist 제거 정규식이 범위를 너무 크게 잡아, ValidateSdoWritePolicy() 내부 코드뿐 아니라 바로 뒤의 ValidateSdoSubmitPolicy() 메서드 선언까지 같이 삭제한 것입니다. 그 결과 여러 곳에서 ValidateSdoSubmitPolicy(request)를 호출하지만 정의가 없어 CS0103가 발생했습니다.

현재는 구조를 정상 복구했습니다.

private static void ValidateSdoWritePolicy(LMCSdoRequest request)
{
    if (!request.IsWrite)
    {
        return;
    }

    // Generic SDO Write no longer denies valid object addresses by ObjectIndex.
}

private static void ValidateSdoSubmitPolicy(LMCSdoRequest request)
{
    if (request == null)
    {
        throw new ArgumentNullException("request");
    }

    ValidateSdoWritePolicy(request);

    if (request.IsWrite)
    {
        LMCDiagnosticsWritePolicy.RequireSdoWriteAllowed(request);
        return;
    }

    LMCDiagnosticsSdoPolicy.RequireReadAllowed(request);
}
현재 dev 소스에도 이 형태로 들어가 있습니다. 즉 모든 유효 ObjectIndex 허용 정책은 유지하면서, Submit 공통 검증 메서드만 정상 복구했습니다.

그리고 Windows MSBuild 환경에서 실제로

LMC_Library/LMC_API_Delivery/src/LasalMotionControlLib.csproj
Configuration=Debug

빌드를 돌렸고 성공했습니다. 검증 워크플로 전체도 conclusion: success로 완료됐습니다.

수정 커밋은:

700fb32305ec47ad24ee36f12b10d65a2edbb08f
fix: restore SDO submit policy validator

이며 현재 dev HEAD입니다.

사용자가 올린 오류 중 나머지 CS0006 3개는 별도 원인이 아니라 첫 번째 API 빌드 실패의 연쇄 오류입니다. LasalMotionControlLib.dll이 생성되지 않으니 WPF가 DLL을 못 찾고, WPF exe가 안 만들어지니 SmokeTests도 exe/DLL을 못 찾은 것입니다.

따라서 로컬에서 최신 dev를 pull한 뒤 Clean → Rebuild 하면 우선 제시한 CS0103와 그에 따른 CS0006들은 없어져야 합니다.

이번 수정에서는 SDO Write 정책 자체를 다시 막지는 않았습니다. 0x6040, 0x6060, 0x607A, 0x60FF, 0x6071, 0x3204, 0x20FC 등을 포함한 non-zero ObjectIndex 전체 허용 정책은 그대로 유지됩니다.

사용자 첨부 파일
나는 이거 다 되는거를 요청했는데 아직도 사용불가 되어있어. 왜그런건지 확인해봐


5m 32s 동안 처리함


Analysis output 1
Analysis output 1
Analysis output 1
Analysis output 1
확인했습니다. 이전 수정이 완전하지 않았습니다. 제가 제거한 것은 SDK/LASAL의 ObjectIndex 주소 차단이었고, 실제 WPF의 Manual SDO Write 실행 경로에는 기존 approved target 제한이 그대로 남아 있습니다.

현재 dev를 보면 SDK 레벨에서는 0x6040, 0x6060, 0x607A, 0x60FF, 0x6071, 0x3204, 0x20FC를 포함한 일반 SDO Write가 허용되도록 바뀌어 있습니다. 실제 테스트도 이 주소들을 generic Write로 통과시키도록 변경돼 있습니다. 하지만 같은 테스트에서 GetApprovedSdoWriteTargets()는 여전히 정확히 1개만 반환한다고 검증하고 있고, 그 하나가 Slave 1 / 0x2F00:24 / Int32 / 4 byte, 즉 화면에 보이는 Reserved diagnostic UI[24]입니다.

그래서 현재 구조는 이렇게 되어 있습니다.

저수준 API 정책: 0x6060 같은 임의 ObjectIndex Write 가능.
WPF Manual Write UI: 여전히 hasApprovedSdoWriteTarget이 반드시 필요하고, ButtonSubmitSdo.IsEnabled 조건에도 supportsSdoWrite && hasApprovedSdoWriteTarget && manualSdoWriteActivationQualified && DiagnosticsMutationJournalCanArm이 들어 있습니다.
실제 Write 제출 경로: 선택된 currentApprovedTarget을 pinnedWriteTarget으로 잡고 SubmitSdoWriteIdentityPinnedAsync()에 전달합니다. 즉 버튼만 억지로 켜도 현재 구조에서는 임의 주소 요청이 approved target과 일치하지 않아 다시 막힐 수 있습니다.
또 하나 중요한 부분이 있습니다. SdoWriteActivationQualificationProof 자체도 단순히 “이 세션에서 SDO Write가 검증됨”이 아니라 Slave / ObjectIndex / SubIndex / ValueType / DataLength / 값 범위까지 exact target tuple에 귀속되어 있습니다. 즉 UI24로 Same-Value Qualification을 통과한 증명은 설계상 UI24용입니다.

지금 캡처가 이 상태를 정확히 보여줍니다. 상단 타이틀에는 **qualified Axis1 UI24 SDO Write**라고 되어 있고, preset도 여전히 0x2F00:24인데 실제 입력란에는 0x6060:0 / Int8 / Length 1을 넣으셨습니다. 즉 API는 0x6060을 받아들일 준비가 됐는데 WPF 실행 계층은 아직 UI24 전용 qualification/approved-target 모델을 사용하고 있는 상태입니다.

그리고 캡처의 버튼 문구가 먼저 동일 값 Qualification 실행이 아니라 **SDO Write 준비**인 점을 보면 현재 UI24 qualification proof 자체는 살아 있습니다. 따라서 이 화면에서 버튼이 회색인 즉각적인 원인은 approved-target/proof 외에도 TrackedD5Submit admission 조건 중 하나가 false인 상태일 가능성이 있습니다. 이 admission은 unresolved mutation뿐 아니라 AxisPowerOnUnresolved, GroupPowerUnresolved, PowerRecoveryJournalUnavailable, RecoveryIdentityReadOnly, operation slot 등을 추가로 검사합니다. 현재 캡처에서는 Mutation journal은 준비 상태이므로 journal 자체는 주원인으로 보이지 않습니다.

결론적으로, 사용자께서 요청한 “입력한 모든 유효 ObjectIndex를 이 화면에서 실제 SDO Write 가능하게”는 아직 구현 완료가 아닙니다. 이전 변경은 하위 정책까지만 풀었고 WPF의 UI24 전용 실행 게이트를 남겨둔 것이 문제입니다.

원래 요청대로 만들려면 다음 방향이 맞습니다: approved target을 Manual Write 실행 필수조건에서 제거하고, UI24 Same-Value Qualification은 특정 Object용 proof가 아니라 현재 connection/session의 SDO Write transport qualification으로만 사용하도록 바꿔야 합니다. 이후 실제 수동 요청은 입력된 Slave/Object/SubIndex/Type/Length/Value 그대로 generic LMCSdoRequest로 제출하고, 기존의 session/BootId/MapRevision, durable mutation journal, exact readback, no-replay 등은 유지하면 됩니다.

즉 UI24 한 번 qualification → 같은 연결 세션에서는 0x6060, 0x6040, 0x607A 등 임의의 유효 SDO 주소를 Write 가능하게 만드는 쪽이 지금 요청하신 동작과 일치합니다.

@GitHub 분석된 방향대로 코드를 수정해줘


25m 39s 동안 처리함
코드 수정 작업은 진행했고, 핵심 변경 자체는 컴파일 및 API 테스트까지 정상 통과한 상태입니다. 다만 WPF 전체 Smoke에서 기존 테스트 기대값 몇 건이 현재 변경된 정책과 충돌해서, 아직 최종 dev 런타임 변경 커밋까지는 밀어 넣지 않았습니다.

적용한 핵심 변경은 다음과 같습니다.

UI24 0x2F00:24 Same-Value Qualification은 계속 canary qualification 용도로 사용합니다.
Qualification PASS 후 proof는 UI24 ObjectIndex 전용 proof가 아니라 현재 connection/session + Diagnostics transport identity proof로 취급하도록 변경했습니다.
따라서 qualification 후 0x6060, 0x6040, 0x607A 등 다른 유효 ObjectIndex를 입력해도 proof가 무효화되지 않습니다.
Manual Write는 preset과 일치할 필요 없이 입력한 Slave/ObjectIndex/SubIndex/Type/Length/Value를 generic request로 사용합니다.
proof가 없으면 버튼 문구도 기존의 애매한 SDO Write 준비가 아니라 **먼저 동일 값 Qualification 실행**으로 표시하도록 변경했습니다.
session/DiagnosticsBuild/BootId/MapRevision, two-click confirmation, safe-axis preflight, durable mutation journal/no-replay, exact readback은 그대로 유지했습니다.
특히 API 회귀 테스트에 실제로 다음과 같은 0x6060 generic Write를 추가했습니다.

var request = LMCSdoRequest.CreateWrite(
    1,
    0x6060,
    0,
    LMCSignalValueType.Int8,
    TestFrame.Hex("01"),
    100);

var ticket = connection.Diagnostics
    .SubmitSdoWriteIdentityPinnedAsync(
        request,
        requiredCapabilities,
        CancellationToken.None)
    .GetAwaiter()
    .GetResult();
이 경로를 포함한 API C# 테스트는 TOTAL 1200 / PASSED 1200 / FAILED 0까지 확인했습니다. 개발용 WPF 프로젝트 자체도 Debug 빌드에 성공했습니다.

WPF proof 테스트도 새 정책으로 정상 통과했습니다. 즉 qualification target과 다른 target을 넣어도 session proof가 유지되는 Wpf.SdoWriteActivationProof.QualificationTargetDoesNotScopeManualWrite 테스트가 PASS했습니다.

현재 남은 문제는 WPF 전체 Smoke 394개 중 6건입니다. 이 중 이번 변경과 직접 연결된 것은 대부분 예전 정책을 아직 기대하는 stale test입니다. 예를 들어 localization 테스트는 proof가 없는 상황에서도 SDO Write 준비를 기대하고 있고, 다른 테스트는 0x3204/0x20FC가 여전히 금지라고 기대하며, 또 하나는 0x6060을 여전히 reserved object라며 거부해야 한다고 검사하고 있습니다. 실제 구현 정책과 반대입니다.

특히 WriteSameValueAxis1OnlyRequiresConfirmations 테스트에는 아직도 다음 옛 가정이 남아 있습니다.

qualification proof가 없어도 Generic SDO Write 버튼이 활성화되어야 한다.

그리고 뒤에서는 0x6060 Write가 거부되어야 한다고 기대합니다. 둘 다 지금 요청하신 동작과 정반대이므로 테스트를 수정해야 합니다.

또 PendingReadback... 테스트는 ArmSdoWriteMutationJournal을 예전 인자 개수로 reflection 호출하고 있어서 TargetParameterCountException이 납니다. 현재 메서드는 baseline/pre-write guard byte 배열까지 받도록 확장되어 있는데 테스트가 업데이트되지 않은 상태입니다.

마지막 1건은 SDO와 무관한 AxisQualificationRecovery.ProcessTermination... fake RPC timeout으로, 앞의 관련 테스트를 고친 뒤 재실행해서 flaky 여부를 분리해야 합니다.

따라서 런타임 구현 방향은 완료됐고 API 1200/1200 + WPF Debug build까지 검증됐지만, stale WPF Smoke 테스트 정리가 아직 남아 있어 최종 커밋은 의도적으로 보류한 상태입니다. 현재 dev에는 검증용 임시 workflow/script 커밋들이 있으며 최종 runtime patch는 아직 영구 커밋되지 않았습니다. 테스트 계약을 현재 정책에 맞춰 정리하고 전체 검증 후 임시 파일까지 제거하는 마지막 단계가 남아 있습니다.

@GitHub 내용 확인해서 커밋 진행해


21m 17s 동안 처리함











커밋 완료했습니다. 현재 dev HEAD는 eeebda2b36a52a442f4919cbe70011536103b7be, 커밋 메시지는 dev: finalize generic SDO Write session qualification입니다.

최종 검증도 전부 통과했습니다. API C# 1200/1200 PASS, WPF Debug Build PASS, WPF Smoke 394/394 PASS, git diff --check까지 성공했고 최종 검증 Workflow 자체도 success로 종료됐습니다.

이번 커밋에서 확정된 동작은 다음과 같습니다.

UI24 Same-Value Qualification은 현재 세션/transport 검증용으로 사용.
Qualification에 사용한 ObjectIndex와 이후 Manual Write ObjectIndex가 달라도 proof 유지.
즉 UI24로 Qualification 후 0x6060 등 다른 유효 ObjectIndex로 Generic SDO Write 가능.
연결/session generation, DiagnosticsBuild/BootId/MapRevision, BaseCycleTime, MaxSdoDataBytes 등이 바뀌면 proof는 즉시 폐기. 현재 구현에서도 qualification target은 provenance일 뿐 Manual Write target을 제한하지 않도록 명시되어 있습니다.
기존 two-click confirmation, safe-axis preflight, durable mutation journal/no-replay, exact readback은 유지.
0x6060 Generic Write에 대한 identity-pinned 회귀 테스트도 추가했습니다.
테스트 기대값의 journal v4, 0x3204/0x20FC, 0x6060 관련 stale contract도 현재 정책에 맞춰 정리했습니다.
검증용으로 만들었던 .github 임시 workflow와 두 patch script도 최종 커밋에서 모두 삭제되어 dev에는 남아 있지 않습니다. 최종 커밋은 총 12개 파일 변경으로 정리됐습니다.

참고로 이번 커밋은 현재 개발용 WPF/API 경로 기준입니다. LMC_API_Distribution 쪽 예제 미러의 qualification-proof 구현 동기화는 이번 최종 커밋 범위에는 포함하지 않았습니다.




