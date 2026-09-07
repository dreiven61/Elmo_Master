README(1).md
파일
SET_OPERATION_MODE_START_EXECUTION_REFACTOR_PLAN_20260831.md
파일
@
GitHub
 설계문서대로 구현을 진행해보자.
붙여넣은 텍스트 (1)(2).txt
문서
@
GitHub
 동작시켰는데 돌기만 하고 실제로 모드가 바뀌지 않네[10:36:39.904] Executable identity: Path=C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Debug\LasalMotionControlApiExample.exe, Version=0.9.1.0, BuildUtc=2026-08-31 01:36:04 UTC, Feature=CREVIS_TOPOLOGY_AXIS1_UI24_SDO_WRITE_LIVE_AXIS_QUAL_V5, ReconnectPolicy=RPC_INIT_FRESH_TCP_ONCE_V2, SdkPath=C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Debug\LasalMotionControlLib.dll, SdkBuildUtc=2026-08-31 01:36:02 UTC.
[10:36:39.905] Example ready. Connect, load _LMCAxis1, and start with Read Status. Connect automatically attempts only the read-only CREVIS topology load; no motion or mutation command is sent automatically.
[10:36:42.738] Connect started.
[10:36:42.768] Connection state Disconnected -> Connecting
[10:36:42.787] Connection state Connecting -> Connected
[10:36:42.789] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
[10:36:42.855] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=INITIAL, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
[10:36:42.861] Connect PASS.
[10:36:49.375] Refresh SetOperationMode Capabilities started.
[10:36:49.388] Refresh SetOperationMode Capabilities PASS.
[10:36:50.161] SetOperationMode Start UI handler entered.
[10:37:03.829] Set Operation Mode Selected Mode Once started.
[10:37:03.917] D5 terminal wake ignored: no exact current retained ticket, EventId=0x00000001, BootId=0x00000066
[10:37:03.972] D5 terminal wake ignored: no exact current retained ticket, EventId=0x00000002, BootId=0x00000066
[10:37:03.983] SetOperationMode cross-mode preflight passed: axis=1, currentMode=8, requestedMode=1, StatusWord=0x02D0.
[10:37:03.996] SetOperationMode final Diagnostics refreshed: Build=1, BootId=0x00000066, MapRevision=0x957F101E.
[10:37:03.999] SetOperationMode prepared: RequestId=3, ClientIntentId=DC6A7F6B-6A552839-A1EFEF11-790A7015.
[10:37:04.008] SetOperationMode journal armed before dispatch: Identity=47361a128afd437baf03f1347af6d508, RequestId=3.
[10:37:04.080] SetOperationMode durable journal promoted to RecoveryRequired after definitive Start rejection. No automatic Start replay is permitted.
[10:37:04.081] SetOperationMode definitive Start rejection | Requested=ProfilePosition(1), Axis=1, RequestId=3, Status=1, ErrorId=-31000, Detail=SetOperationModeOutcomeStorageUnavailable(49), Build/Boot/Map=0x00000001/0x00000066/0x957F101E
[10:37:04.130] SetOperationMode definitive Start rejection archived durably; no retained PLC outcome exists and the recovery interlock was cleared. Evidence=C:\Users\dreiv\AppData\Local\Elmo\LasalMotionControlApiExample\AxisSetOperationModeRecoveryJournal\v1\axis-set-operation-mode-rejected-47361a128afd437baf03f1347af6d508.evidence.
[10:37:04.169] Set Operation Mode Selected Mode Once FAILED: StartAxisSetOperationMode was rejected. This is not mode-change completion evidence.
[10:37:23.820] Submit SDO Read started.
[10:37:23.827] QTEST|utc=2026-08-31T01:37:23.827Z|elapsedMs=0|run=6dde648d508b4439aa45a49e3a20fe93|scenario=D5ExternalTracking:manual-sdo-read-submit|step=1|event=BEGIN|stage=manual-sdo-read-submit
[10:37:23.859] QTEST|utc=2026-08-31T01:37:23.859Z|elapsedMs=31|run=6dde648d508b4439aa45a49e3a20fe93|scenario=D5ExternalTracking:manual-sdo-read-submit|step=2|event=D5_EXTERNAL_PREFLIGHT|stage=manual-sdo-read-submit|bootId=0x00000066|mapRevision=0x957F101E|requiredDataBytes=1|generalInlineRequired=true|verdict=PASS
[10:37:23.863] QTEST|utc=2026-08-31T01:37:23.863Z|elapsedMs=36|run=6dde648d508b4439aa45a49e3a20fe93|scenario=D5ExternalTracking:manual-sdo-read-submit|step=3|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=aa027231bd82410f864613c87851c9f5|slave=1|bootId=0x00000066|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|state=ARMED_BEFORE_SUBMIT
[10:37:23.875] QTEST|utc=2026-08-31T01:37:23.875Z|elapsedMs=47|run=6dde648d508b4439aa45a49e3a20fe93|scenario=D5ExternalTracking:manual-sdo-read-submit|step=4|event=D5_EXTERNAL_TICKET_PRESERVED|stage=manual-sdo-read-submit|ticket=3|slave=1|bootId=0x00000066|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|terminalWaitMs=6000
[10:37:23.876] QTEST|utc=2026-08-31T01:37:23.876Z|elapsedMs=48|run=6dde648d508b4439aa45a49e3a20fe93|scenario=D5ExternalTracking:manual-sdo-read-submit|step=5|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=aa027231bd82410f864613c87851c9f5|ticket=3|bootId=0x00000066|mapRevision=0x957F101E|state=ACCEPTED_TICKET|detail=3|quarantine=false
[10:37:23.877] Submit SDO Read PASS.
[10:37:23.915] D5 terminal wake matched retained ticket; authoritative TCP status query started. TicketId=0x00000003
[10:37:23.926] QTEST|utc=2026-08-31T01:37:23.926Z|elapsedMs=98|run=6dde648d508b4439aa45a49e3a20fe93|scenario=D5ExternalTracking:manual-sdo-read-submit|step=6|event=D5_EXTERNAL_TICKET_TERMINAL|stage=callback-d5-terminal-wake|ticket=3|state=Completed|outcome=Success|verdict=CLEARED
[10:37:23.926] QTEST|utc=2026-08-31T01:37:23.926Z|elapsedMs=98|run=6dde648d508b4439aa45a49e3a20fe93|scenario=D5ExternalTracking:manual-sdo-read-submit|step=7|event=END|resolution=KNOWN_TICKET_TERMINAL|verdict=RESOLVED
[10:37:23.929] Callback D5 authoritative TCP status processed. TicketId=0x00000003
[10:37:25.460] Read SDO Inline started.
[10:37:25.463] QTEST|utc=2026-08-31T01:37:25.463Z|elapsedMs=0|run=b9a2002e4d7c4a44b3c3ffdf359d6d39|scenario=D5ExternalTracking:manual-sdo-inline-read|step=1|event=BEGIN|stage=manual-sdo-inline-read
[10:37:25.474] QTEST|utc=2026-08-31T01:37:25.474Z|elapsedMs=11|run=b9a2002e4d7c4a44b3c3ffdf359d6d39|scenario=D5ExternalTracking:manual-sdo-inline-read|step=2|event=D5_EXTERNAL_PREFLIGHT|stage=manual-sdo-inline-read|bootId=0x00000066|mapRevision=0x957F101E|requiredDataBytes=1|generalInlineRequired=true|verdict=PASS
[10:37:25.475] QTEST|utc=2026-08-31T01:37:25.475Z|elapsedMs=12|run=b9a2002e4d7c4a44b3c3ffdf359d6d39|scenario=D5ExternalTracking:manual-sdo-inline-read|step=3|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-inline-read|evidence=3a480c0b3420471aa17055f9181c1528|slave=1|bootId=0x00000066|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|state=ARMED_BEFORE_SUBMIT
[10:37:25.517] QTEST|utc=2026-08-31T01:37:25.517Z|elapsedMs=54|run=b9a2002e4d7c4a44b3c3ffdf359d6d39|scenario=D5ExternalTracking:manual-sdo-inline-read|step=4|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-inline-read|evidence=3a480c0b3420471aa17055f9181c1528|ticket=4|bootId=0x00000066|mapRevision=0x957F101E|state=TERMINAL_SUCCESS|detail=Completed/Success|quarantine=false
[10:37:25.517] QTEST|utc=2026-08-31T01:37:25.517Z|elapsedMs=54|run=b9a2002e4d7c4a44b3c3ffdf359d6d39|scenario=D5ExternalTracking:manual-sdo-inline-read|step=5|event=END|resolution=TERMINAL_SUCCESS|verdict=RESOLVED
[10:37:25.518] Read SDO Inline PASS.
[10:37:25.537] D5 terminal wake matched retained ticket; authoritative TCP status query started. TicketId=0x00000004
[10:37:25.547] Callback D5 authoritative TCP status processed. TicketId=0x00000004
[10:37:32.457] SetOperationMode Start UI handler entered.
[10:37:33.828] Set Operation Mode Selected Mode Once started.
[10:37:33.882] D5 terminal wake ignored: no exact current retained ticket, EventId=0x00000005, BootId=0x00000066
[10:37:33.932] D5 terminal wake ignored: no exact current retained ticket, EventId=0x00000006, BootId=0x00000066
[10:37:33.933] SetOperationMode cross-mode preflight passed: axis=1, currentMode=8, requestedMode=1, StatusWord=0x02D0.
[10:37:33.936] SetOperationMode final Diagnostics refreshed: Build=1, BootId=0x00000066, MapRevision=0x957F101E.
[10:37:33.936] SetOperationMode prepared: RequestId=5, ClientIntentId=4EF05AD4-620C2306-0C30547E-86E516CE.
[10:37:33.939] SetOperationMode journal armed before dispatch: Identity=82f16a8005e347ec9cc1c5fc18af8c45, RequestId=5.
[10:37:33.989] SetOperationMode durable journal promoted to RecoveryRequired after definitive Start rejection. No automatic Start replay is permitted.
[10:37:33.989] SetOperationMode definitive Start rejection | Requested=ProfilePosition(1), Axis=1, RequestId=5, Status=1, ErrorId=-31000, Detail=SetOperationModeOutcomeStorageUnavailable(49), Build/Boot/Map=0x00000001/0x00000066/0x957F101E
[10:37:34.033] SetOperationMode definitive Start rejection archived durably; no retained PLC outcome exists and the recovery interlock was cleared. Evidence=C:\Users\dreiv\AppData\Local\Elmo\LasalMotionControlApiExample\AxisSetOperationModeRecoveryJournal\v1\axis-set-operation-mode-rejected-82f16a8005e347ec9cc1c5fc18af8c45.evidence.
[10:37:34.070] Set Operation Mode Selected Mode Once FAILED: StartAxisSetOperationMode was rejected. This is not mode-change completion evidence.
[10:37:41.806] SetOperationMode Start UI handler entered.
[10:37:41.807] Set Operation Mode Selected Mode Once started.
[10:37:41.893] D5 terminal wake ignored: no exact current retained ticket, EventId=0x00000007, BootId=0x00000066
[10:37:41.938] SetOperationMode cross-mode preflight passed: axis=1, currentMode=8, requestedMode=1, StatusWord=0x02D0.
[10:37:41.941] SetOperationMode final Diagnostics refreshed: Build=1, BootId=0x00000066, MapRevision=0x957F101E.
[10:37:41.941] SetOperationMode prepared: RequestId=7, ClientIntentId=FAF246DA-54A69BA0-A9B8B762-F87F3F66.
[10:37:41.944] SetOperationMode journal armed before dispatch: Identity=c2cc7026711d4a64936d8806274c28d2, RequestId=7.
[10:37:41.993] SetOperationMode durable journal promoted to RecoveryRequired after definitive Start rejection. No automatic Start replay is permitted.
[10:37:41.993] SetOperationMode definitive Start rejection | Requested=ProfilePosition(1), Axis=1, RequestId=7, Status=1, ErrorId=-31000, Detail=SetOperationModeOutcomeStorageUnavailable(49), Build/Boot/Map=0x00000001/0x00000066/0x957F101E
[10:37:42.034] SetOperationMode definitive Start rejection archived durably; no retained PLC outcome exists and the recovery interlock was cleared. Evidence=C:\Users\dreiv\AppData\Local\Elmo\LasalMotionControlApiExample\AxisSetOperationModeRecoveryJournal\v1\axis-set-operation-mode-rejected-c2cc7026711d4a64936d8806274c28d2.evidence.
[10:37:42.073] Set Operation Mode Selected Mode Once FAILED: StartAxisSetOperationMode was rejected. This is not mode-change completion evidence.
[10:37:42.075] D5 terminal wake ignored: no exact current retained ticket, EventId=0x00000008, BootId=0x00000066
[10:37:52.989] SetOperationMode Start UI handler entered.
[10:37:52.991] Set Operation Mode Selected Mode Once started.
[10:37:53.062] D5 terminal wake ignored: no exact current retained ticket, EventId=0x00000009, BootId=0x00000066
[10:37:53.111] SetOperationMode cross-mode preflight passed: axis=1, currentMode=8, requestedMode=1, StatusWord=0x02D0.
[10:37:53.113] D5 terminal wake ignored: no exact current retained ticket, EventId=0x0000000A, BootId=0x00000066
[10:37:53.115] SetOperationMode final Diagnostics refreshed: Build=1, BootId=0x00000066, MapRevision=0x957F101E.
[10:37:53.116] SetOperationMode prepared: RequestId=9, ClientIntentId=955F9489-6A72434E-D37FBB28-A791D3BF.
[10:37:53.120] SetOperationMode journal armed before dispatch: Identity=d3a63d5ee61e435caa7bdf01283a5ba0, RequestId=9.
[10:37:53.169] SetOperationMode durable journal promoted to RecoveryRequired after definitive Start rejection. No automatic Start replay is permitted.
[10:37:53.170] SetOperationMode definitive Start rejection | Requested=ProfilePosition(1), Axis=1, RequestId=9, Status=1, ErrorId=-31000, Detail=SetOperationModeOutcomeStorageUnavailable(49), Build/Boot/Map=0x00000001/0x00000066/0x957F101E
[10:37:53.213] SetOperationMode definitive Start rejection archived durably; no retained PLC outcome exists and the recovery interlock was cleared. Evidence=C:\Users\dreiv\AppData\Local\Elmo\LasalMotionControlApiExample\AxisSetOperationModeRecoveryJournal\v1\axis-set-operation-mode-rejected-d3a63d5ee61e435caa7bdf01283a5ba0.evidence.
[10:37:53.256] Set Operation Mode Selected Mode Once FAILED: StartAxisSetOperationMode was rejected. This is not mode-change completion evidence.
[10:37:58.179] Read SDO Inline started.
[10:37:58.188] QTEST|utc=2026-08-31T01:37:58.188Z|elapsedMs=0|run=c9e4e931971d4611ba646080c4bfd62c|scenario=D5ExternalTracking:manual-sdo-inline-read|step=1|event=BEGIN|stage=manual-sdo-inline-read
[10:37:58.193] QTEST|utc=2026-08-31T01:37:58.193Z|elapsedMs=4|run=c9e4e931971d4611ba646080c4bfd62c|scenario=D5ExternalTracking:manual-sdo-inline-read|step=2|event=D5_EXTERNAL_PREFLIGHT|stage=manual-sdo-inline-read|bootId=0x00000066|mapRevision=0x957F101E|requiredDataBytes=1|generalInlineRequired=true|verdict=PASS
[10:37:58.194] QTEST|utc=2026-08-31T01:37:58.194Z|elapsedMs=5|run=c9e4e931971d4611ba646080c4bfd62c|scenario=D5ExternalTracking:manual-sdo-inline-read|step=3|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-inline-read|evidence=148a037bda4a4b2a8254441b36f5c9c0|slave=1|bootId=0x00000066|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|state=ARMED_BEFORE_SUBMIT
[10:37:58.235] QTEST|utc=2026-08-31T01:37:58.235Z|elapsedMs=47|run=c9e4e931971d4611ba646080c4bfd62c|scenario=D5ExternalTracking:manual-sdo-inline-read|step=4|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-inline-read|evidence=148a037bda4a4b2a8254441b36f5c9c0|ticket=11|bootId=0x00000066|mapRevision=0x957F101E|state=TERMINAL_SUCCESS|detail=Completed/Success|quarantine=false
[10:37:58.237] QTEST|utc=2026-08-31T01:37:58.237Z|elapsedMs=48|run=c9e4e931971d4611ba646080c4bfd62c|scenario=D5ExternalTracking:manual-sdo-inline-read|step=5|event=END|resolution=TERMINAL_SUCCESS|verdict=RESOLVED
[10:37:58.239] Read SDO Inline PASS.
[10:37:58.245] D5 terminal wake matched retained ticket; authoritative TCP status query started. TicketId=0x0000000B
[10:37:58.252] Callback D5 authoritative TCP status processed. TicketId=0x0000000B

남은 전체 구현 설계를 dev에 반영했습니다. 새 정본은 **docs/api/design/REMAINING_IMPLEMENTATION_DESIGN_20260901.md**이고, 설계 인덱스에서도 이 문서를 SetOperationMode 이후 구현 master plan으로 지정했습니다.

핵심은 남은 기능을 단순 나열하지 않고 실제로 코드를 더 짜야 하는 기능과 qualification만 남은 기능을 분리한 것입니다. 현재 분류는 Generic SDO → HomeDS402 → HomeDS402Ex → SetPosition → EtherCAT/Diagnostics 후순위 기능 → Release 순입니다. Generic SDO와 HomeDS402는 runtime을 새로 설계하는 게 아니라 실기·artifact·activation을 닫는 쪽이고, HomeDS402Ex와 SetPosition은 아직 실제 PLC/RT runtime 구현이 남아 있습니다.

1. Generic SDO — 현재 P0

새 엔진을 다시 만들지 않습니다. 현재 LMCSdoExecutor의 Manual Server/programmatic dual-entry와 axis1~4 generic 1/2/4-byte 정책을 유지하고, 남은 것을 SDO-Q01~Q03으로 나눴습니다.

Axis1 1/2/4-byte safe object exact Write/readback → Manual/programmatic BUSY contention → timeout/disconnect/abort/readback mismatch no-replay → Axis2~4 확대 순서입니다. 특히 0x6060 등 semantic-owner object는 계속 Generic SDO에서 막습니다. 현재 issue #46의 완료 조건과 정확히 맞춘 구조입니다.

2. HomeDS402 — core 구현은 건드리지 않음

HomeDS402는 현재 method37 lifecycle 자체가 이미 구현돼 있으므로 새 state machine을 만들지 않습니다. 남은 것은:

H37-05/06 fresh C78/generated artifact → H37-07 Axis1 hardware matrix → H37-08 Axis2~4 → H37-09 atomic activation

입니다.

마지막 activation은 기존 설계대로 ordinary ownership 2곳, Diagnostics gate, InputLatch startup sweep, Admin capability bit6의 5개 값을 한 changeset으로 켜게 설계했습니다. issue #32의 generated artifact ratchet을 완화하지 않고 실제 fresh C78 evidence로 닫도록 유지했습니다.

3. HomeDS402Ex — 가장 큰 실제 PLC runtime 작업

여기는 실제 구현이 많이 남았습니다. 먼저 issue #28의 축별 wiring/method/scale profile과 issue #35의 fresh generated artifact가 선행 조건입니다.

그 다음 runtime을 다음처럼 쪼갰습니다.

approved profile
→ parameter snapshot
→ approved parameter SDO program
→ RT owner
→ 6060=6 / 6061=6
→ controlword bit4
→ homing observer
→ bit4 low
→ parameter restore
→ CSP8 restore
→ setpoint alignment
→ RT owner release
→ ActualPosition == -Position
→ terminal outcome

그리고 LASAL method 32 KiB 제한 때문에 ProcessAxisDs402HomeEx 한 함수에 몰아넣지 않고 Preflight / ParameterStages / RtStages / CleanupStages / RecoveryStages로 분할하도록 설계했습니다. Cleanup proof가 불완전하면 일반 Failed로 축소하지 않고 Indeterminate/Quarantine으로 남깁니다.

4. SetPosition — 외부 prerequisite 이후 구현

SetPosition은 issue #44의 두 조건을 그대로 hard blocker로 유지했습니다.

실제 vendor CheckSum.CRC32 golden vector
LASAL IDE가 생성한 _FileSys class/client ABI

이 둘을 확보하기 전에는 CRC나 generated ABI를 추측 구현하지 않습니다.

확보 후 구현 순서는:

SP-01B durable 2x2048 A/B backend
→ SP-02 RT claim-before-native exactly-once executor
→ SP-03 durable terminal-before-owner-release
→ SP-04 WPF durable recovery integration
→ cold-cycle / response-loss / hardware
→ SP-07 paired activation

으로 고정했습니다.

특히 native .SetPosition()은 exact tuple당 논리 call site 1개 / NativeCount 0→1만 허용하고, terminal 순서는 반드시:

RT proof
→ durable terminal commit
→ full storage readback
→ owner release
→ TCP response

가 되게 설계했습니다.

5. EtherCAT / Diagnostics 후순위도 구현 설계 포함

기존에 한 줄 backlog로만 있던 부분도 구체화했습니다.

먼저 read-only인 **NodeHealth 0x7E13 / DigitalIO Read 0x7E22**를 실기 qualification한 뒤 bits 15/16을 엽니다.

그 다음 **Digital Output Write 0x7E23**는 실제 신규 runtime으로 설계했습니다. C# surface만 있고 LASAL route가 없기 때문에:

TCPMotionInterface route
→ LMCDiagnosticsService ticket owner
→ LMCEcatInputLatch RT CAS mailbox
→ ExpectedOutputRevision CAS
→ masked atomic write
→ completion ticket

구조로 구현합니다. PC-side read-modify-write는 허용하지 않습니다.

PI Write는 현재 SDK AllowedPIWriteSignalIds가 empty인 상태를 유지하면서 writable semantic catalog를 먼저 승인한 뒤 SDK와 PLC allowlist를 동시에 여는 구조입니다. Recorder DoubleBank는 protocol을 다시 설계하지 않고 two-bank identity, overwrite, adopt/release, trigger race, RAM/jitter/soak를 닫은 후 bit6을 엽니다. Extended SDO Result bit12는 실제 요구사항이 있을 때만 구현하도록 했습니다.

전체 작업도 R1~R23 tranche로 쪼갰습니다. 큰 기능을 한 번에 활성화하지 않고 Generic SDO → HomeDS402 → HomeDS402Ex → SetPosition → Node/DI → DO Write → PI/Recorder/Extended SDO → Distribution 순으로 커밋/검증할 수 있게 했습니다. 최종 production gate도 source/PC/C78/generated artifact/PLC/hardware/WPF/distribution을 분리해 두었습니다.

GitHub 반영 커밋은 두 개입니다.

3460e905ea05162fbbe486371aca2c63ba42160c — 남은 구현 통합 설계 추가
08bd07e43f274e6c150c7348c51dc084070733a0 — design README에서 이 문서를 current master plan으로 연결

현재 dev HEAD도 08bd07e43f274e6c150c7348c51dc084070733a0입니다.

이제 다음 구현은 이 설계 기준으로 R1, Generic SDO completion부터 바로 시작하면 됩니다.

@
GitHub
 SDO Write부터 진행하는게 좋을꺼 같아. SDO Write 상세설계해줘

SDO Write를 첫 P0 구현 대상으로 잡고 상세설계까지 dev에 반영했습니다. 새 정본은 docs/api/design/SDO_WRITE_DETAILED_DESIGN_20260901.md이며, 현재 design index에서도 Generic SDO Write의 상세 구현 정본으로 연결했습니다. 현재 dev HEAD는 6293d6e980e3487cdd286a7577e0edd94c68e738입니다.

이번에 source를 다시 대조하면서 핵심 문제가 명확해졌습니다. PLC 쪽은 이미 generic SDO Write 구조입니다. GetSdoWritePolicyDetail()은 Axis 1~4, 1/2/4-byte canonical scalar를 허용하고 0x6040/6060/607A/60FF/6071/3204/20FC는 semantic owner 보호를 위해 차단합니다. 실행 직전에도 현재 304-byte snapshot을 다시 읽어서 Standstill, Fault=False, OperationEnabled=False와 DS402 base 0x40/0x21/0x23을 확인합니다. 이후 ProcessOperations()가 해당 축의 TryStartWrite()를 한 번 호출합니다.

반면 WPF qualification 계층은 아직 과거 Axis1 UI[24] 전용 구조가 남아 있습니다. approvedTargets.Count == 1을 요구하고, SdoWriteActivationQualificationProof도 특정 UI24 target의 Slave/Object/SubIndex/Type/Length/range에 bind돼 있습니다. 이 때문에 PLC/SDK generic policy와 WPF activation policy가 현재 완전히 같은 의미가 아닙니다.

그래서 구현을 다음 8개 tranche로 고정했습니다.

SWR-01 — Generic policy 분리: LMCSdoWriteTarget/GetApprovedSdoWriteTargets()는 authorization이 아니라 preset metadata로 재정의하고, NoApprovedTarget이 generic Write를 막지 않게 합니다. SDOWrite bit9도 UI24 preset 개수와 분리합니다.
SWR-02 — Transport qualification proof: UI24 target-bound proof를 Connection + SessionGeneration + DiagnosticsBuild + BootId + MapRevision + SDO capabilities에 bind되는 current-image/session proof로 변경합니다. UI24 same-value는 generic 주소 승인 수단이 아니라 transport canary가 됩니다.
SWR-03 — Ordinary Generic Write 강화: 실제 Write는 Baseline Read → 1차 확인 → fresh identity/safety → Pre-write Guard Read → baseline equality → durable journal → Write 1회 → terminal → exact Readback → Resolve 순서로 고정합니다.
SWR-04 — Manual/programmatic arbitration: LASAL Class View ParaReadWrite=0/1과 API TryStartRead/Write가 같은 executor를 경쟁할 때 한쪽만 owner가 되고 다른 쪽은 BUSY인지 검증합니다. timeout/orphan/cancel/disconnect도 같이 고정합니다.
SWR-05 — Axis1 1/2/4-byte 실기: 실제 장비 문서로 안전성이 확인된 non-semantic object만 사용합니다. same-value부터 시작합니다.
SWR-06 — Axis1 failure/recovery: abort, timeout, orphan, disconnect, response loss, readback mismatch에서 automatic replay 0회를 확인합니다.
SWR-07 — Axis2~4 확대: Axis1 결과를 복사하지 않고 축별 동일 matrix를 수행합니다.
SWR-08 — 마무리: API Manual, development progress, DINT packet map, distribution example, Debug/Release qualification과 issue #46을 동기화합니다.

Wire는 새로 만들지 않습니다. 기존 0x7E50 SubmitSDO를 유지하며 Write payload는 32-byte header + WriteData라서 1-byte=33, 2-byte=34, 4-byte=36 bytes로 고정했습니다. Submit ACK는 기존 operation ticket이고 OperationKind=SDOWrite(3)입니다. Write terminal이 Completed+Success여도 ResultLength=0이므로 그 시점에는 아직 SDO Write PASS가 아닙니다. exact Readback이 requested bytes와 일치해야 최종 VERIFIED입니다. 현재 SDK의 LMCSdoWriteVerificationContext는 이미 generic 1/2/4-byte exact readback을 지원하므로 이 부분은 재작성보다 활용하는 방향입니다.

Recovery 쪽도 명확하게 정리했습니다. Submit 이후 결과가 불명확하면 OutcomeUnverified, accepted nonterminal이면 AcceptedPendingTerminal, 성공 terminal이면 TerminalSuccessPendingReadback, readback이 다르면 ReadbackMismatch입니다. Failed/TimedOut/Orphan이라고 해서 “값이 안 써졌다”고 가정하지 않습니다. 가능한 경우 Readback만 수행하고 Write를 다시 보내지 않습니다. reconnect 후에도 원 Write 재생성은 금지하고, same identity에서 exact Readback만 허용합니다. BootId가 바뀌면 자동 해제하지 않습니다.

한 가지 중요한 설계 판단도 넣었습니다. 1-byte/2-byte 테스트용 OD 주소를 문서에서 임의로 정하지 않았습니다. 실제 드라이브/project 근거로 “non-semantic이고 쓰기 안전하다”는 것이 확인된 target만 사용하게 했습니다. 그런 대상이 확인되지 않으면 해당 width는 BLOCKED_BY_TARGET_EVIDENCE로 남깁니다. changed-value 시험 후 복구가 필요해도 자동 restore하지 않고 별도의 새 Write transaction으로 수행합니다.

상세설계 커밋은 7630e3ca5dee8278954c8d879c6dfa77fca6b546, index 반영은 6293d6e980e3487cdd286a7577e0edd94c68e738입니다. issue #46에도 SWR-01~08 구현 순서와 safety boundary를 코멘트로 남겨 추적 기준을 맞췄습니다.

다음 실제 코드 구현은 설계대로 SWR-01, 즉 UI24 preset authorization과 generic Write policy의 결합을 끊는 작업부터 시작하는 게 맞습니다.

화요일 오후 1:49
붙여넣은 텍스트 (1)(3).txt
문서
@
GitHub
 현재 dev 브런치에 SDO Write 기능을 구현했는데 SDO Write 탭에서 기능을 쓸수가 없어. 
왜 그런건지 분석좀 해줘
