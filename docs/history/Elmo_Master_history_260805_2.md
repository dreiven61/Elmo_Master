# 테스트 항목 정리

> 이제 어떤걸 테스트 해야되는지 한번 정리해봐

<details><summary>3 previous messages</summary>

> 최근 캡처 결과와 현재 소스 상태를 다시 대조해서, 완료·차단·미검증으로 나눈 실제 장비 테스트 순서를 정리하겠습니다. 먼저 최신 이력과 작업 트리, Test3/Test4 근거를 병렬로 확인하겠습니다.
>
> 현재 작업트리는 7월 31일 캡처 이후 Home/encoder-maintenance 구현이 더 진행된 상태입니다. 다만 생성 선언과 Comm Network가 아직 소스 본문을 따라오지 않아, 지금은 실축 명령보다 LASAL IDE 동기화 검증이 먼저라는 정황이 확인됩니다. 이 경계를 정확히 잡고 있습니다.
>
> 현재 검증기를 실제로 돌려보니 SourceOnly와 IDE ABI 검사가 모두 같은 지점에서 실패했습니다: `LMCDiagnosticsService ... AxisOwnership typed client is missing.` 따라서 F9/다운로드를 바로 할 단계가 아닙니다. IDE 선언과 Comm Network 저장이 끝난 뒤 이 검사를 먼저 통과시켜야 합니다.
</details>

결론부터 말하면, 지금 바로 Group·Reset·Home·TW19/TW20을 실축에서 시험하면 안 됩니다. 다음 순서는 `LASAL IDE 동기화 → 빌드/다운로드 정합성 → 4축 read-only → 단일축 재검증 → Group`입니다.

### 현재 판정

| 항목 | 판정 |
|---|---|
| 축 1~4 기본 상대이동 | 기능 PASS |
| EtherCAT OP / AL=0 / 4축 Referenced | PASS |
| 동일 PLC 빌드의 4축 qualification | 미완료 — Axis1은 BootId `0x0C`, Axis2~4는 `0x0D` |
| 드라이브 준비 상태 | FAIL — 전 축 `0x6041=0x02B3`, Warning=1 |
| 실제 이동 중 Stop | 미검증 — 기존 Stop은 이동 완료 후 실행됨 |
| Group Power/Enable/Move/Reset | 미검증 |
| 신규 LMC_Home/DS402 Home/TW19/TW20 | 구현·IDE·gate 차단 상태 |
| CREVIS live DI/DO | 미검증 |

기존 결과의 자세한 판정은 [260803 요약](/C:/work/Elmo/Elmo_Master/docs/history/260803/99_analysis_summary.md:158)에 정리돼 있습니다.

### 1순위: LASAL IDE 구조 동기화

현재 검증기를 다시 실행한 결과, 다음 오류로 실패했습니다.

```text
LMCDiagnosticsService Home/maintenance IDE ABI
AxisOwnership typed client is missing.
```

현재 `LMCControlCommandService` 선언에는 아직 `ReferenceState`, `ProcessAxisReference`가 남아 있고, 새 `AxisOwnership` 선언과 Comm Network 연결도 없습니다.

먼저 IDE에서 다음을 저장해야 합니다.

- `LMCControlCommandService`: `InputLatch`, `ZeroHomeState`, `OwnershipState`, 새 ownership/Home 함수 선언
- `LMCDiagnosticsService`: `AxisOwnership`, encoder-maintenance 변수와 함수 선언
- Comm Network 연결 2개
  - `LMCControlCommandService1.InputLatch → LMCEcatInputLatch1.ClassSvr`
  - `LMCDiagnosticsService1.AxisOwnership → LMCControlCommandService1.ClassSvr`

정확한 입력값은 [IDE handoff](/C:/work/Elmo/Elmo_Master/docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md:182)에 있습니다.

Save All 후 순서는 다음입니다.

1. generated declaration/Network diff 확인
2. ABI current-only 및 SourceOnly/full static PASS
3. F9 → Rebuild/Link
4. 변경 클래스 `Find in Implementation`
5. smoke 시작 이후 `Lasal2.log` 신규 `CInvalidArgException=0`

현재 상태에서는 F9부터 실행하지 않는 게 맞습니다.

### 2순위: 최신 PLC 배포 후 read-only 재시험

canonical 프로젝트를 cold download한 뒤, 같은 연결·같은 BootId에서 먼저 확인합니다.

- `DiagnosticsBuild=1`
- `CapabilityBits=0x0000633F`
- `MapRevision=0x957F101E`
- Catalog 24개
- `BootId != 0`, 시험 중 고정
- `0x7E10`: Master/축 1~4 OP, AL=0, invalid cycle=0
- 축별 실제 D5:
  - `0x6061=8`
  - `0x6041`: Fault=0, Warning=0, InternalLimit=0
  - `0x603F=0`
  - `AxisError=0`

`0x2028`의 `StatusWord=0`은 실제 DS402 `0x6041`이 아닙니다.

여기서 Warning=1이 다시 나오면 시험 중단입니다. EAS/드라이브 경고 원인을 read-only로 확인해야 하며 Reset, SDO Write, TW19/TW20으로 억지로 지우면 안 됩니다.

### 그다음 시험 순서

1. 같은 새 BootId에서 축 1→4 단일축 기본 시험 재실행
2. 별도 이동 중 Stop 시험
   - 저속·소거리 이동
   - non-Standstill 확인 중 `0x2022` 정확히 1회
   - Standstill 3회, 최종 위치 3회, Power Off 확인
3. Group 무이동 lifecycle
   - Group Power On
   - Power stable
   - Set Identity
   - Enable ACK 1회
   - Locked/Standby 3회
   - Disable
   - Power Off 3회
4. Group 소거리 이동과 실제 이동 중 Group Stop
5. 실제 Fault가 있을 때만 Group Reset
   - ACK 후 GroupError, 멤버 AxisError, `0x6041`, `0x603F`가 3회 안정적으로 clear되는지 확인
6. 신규 Home 기능은 common-owner startup proof가 `0x0F`로 완성된 뒤 별도 시험
7. TW19/TW20은 마지막

현재 common-owner startup proof는 필요한 `0x0F`가 아니라 BootId만 있는 `0x01`이라 Home reserve가 의도적으로 거부됩니다. TW19/TW20도 축별 encoder family/socket live 정보가 없어 현재 활성화 가능한 축은 0개입니다.

각 시험은 캡처를 버튼 조작 전에 시작하고, 최종 상태 후 최소 3초 유지하며 `pcapng + QTEST TXT + PLC log`를 같은 이름으로 남기면 됩니다. ACK는 완료가 아니므로 반드시 polling, 3회 안정 상태, 최종 readback까지 확인해야 합니다.

즉, 지금 바로 할 일은 LASAL IDE declaration과 Network 두 연결을 Save All하는 것입니다. 그 저장본을 확인하기 전에는 F9도, PLC 다운로드도 진행하지 않는 것이 맞습니다.
