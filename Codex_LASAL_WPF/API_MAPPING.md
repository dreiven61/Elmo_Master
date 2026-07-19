# LASAL TCP/IP WPF Test App - Legacy Hybrid Map

- Solution: `C:\work\Elmo\Elmo_Master\Codex_LASAL_WPF\PmasApiWpfTestApp.sln`
- Target: `.NET Framework 4.8`, WPF, Visual Studio 2019
- Backend: `SigmatekTcpIpDummyMMCLib.cs`
- 현재 판정: **legacy reference only; canonical E2E client가 아님**

이 앱은 이름과 화면은 PMAS/LASAL 전체 기능을 구현한 것처럼 보이지만 실제로는
초기 이식 실험이다. 실제 TCP 전송, local simulation과 no-op이 같은 backend에
섞여 있으므로 화면에 보이는 기능을 PLC 구현 완료로 해석하면 안 된다.

현재 API 개발·실기 앱은 `LMC_Library/LasalApiWpfTestApp`, 외부 전달 앱은
`LMC_Library/LMC_API_Distribution/02_Example_Program`이다. 최신 전체 판정은
[`docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md`](../docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md)를
따른다.

## 실제 동작 분류

| 영역 | 현재 동작 | 판정 |
|---|---|---|
| RPC connect/close | 실제 `TcpClient` connect와 socket 정리 | transport 실험 |
| Axis Power/Reset/Stop | legacy local ID `0x2081..0x2084`, AxisRef 0 강제 가능 | current DINT 계약과 불일치 |
| Axis GetActualPosition | `0x202E`를 전송하고 응답 실패 시 local state fallback | hybrid |
| Axis MoveAbsolute | legacy Int64 frame 전송 후 local simulation도 갱신 | hybrid |
| Axis ReadStatus/StatusRegister | PLC `0x2028` 없이 local power state 반환 | simulation |
| MoveRelative/MoveVelocity | TCP frame 없이 local state 갱신 | simulation |
| Override | 빈 method | no-op |
| Group ReadStatus | `0x2045` 전송, 실패 시 local success-like fallback | hybrid |
| Group MoveLinearAbsoluteEx | legacy/LREAL 계열 `0x20A4` frame 전송 | current DINT 계약과 불일치 |
| Group Enable/Disable/Reset/Stop | 빈 method | no-op |
| Group Members | fabricated X/Y/Z 3축 배열 | simulation |
| SetKinTransform/WaitUntilCondition | connection 확인 또는 짧은 sleep만 수행 | no-op/simulation |
| PI/Bulk/Recorder/SDO 일부 | local dictionary/random/generated data | simulation |

## 사용 허용 범위

- PMAS 화면 구조와 초기 TCP 실험 기록 비교
- 과거 cycle benchmark UI와 결과 형식 참고
- legacy frame과 current `LASAL-DINT v1` 차이 분석

다음 용도로는 사용하지 않는다.

- 현재 PLC 지원 기능 판정
- command ID, payload offset 또는 UNIT의 기준
- 실제 장비 acceptance test
- 외부 사용자 배포

## 빌드 확인

2026-07-16 기준 VS2019 Debug build는 통과했다. build 성공은 위 simulation/no-op
경로를 실제 PLC 기능으로 바꾸지 않는다.
