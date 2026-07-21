>
> 최종 판단
> LASAL에서 Elmo PI/Bulk/Recorder에 대응하는 기능을 만드는 것은 충분히 가능합니다.
> 다만 정확한 경계는 다음과 같습니다.
> PDO와 PLC 내부 신호의 1 ms 진단·기록: 가능
> EtherCAT 상태·누락 frame 진단: 가능
> Elmo와 비슷한 PI/Bulk/Recorder PC API: 신규 개발로 가능
> 드라이브 내부 servo-loop 고속 신호: LASAL만으로 완전 대체 불가
> Raw EtherCAT datagram/Wireshark급 캡처: 현재 공개 API만으로 확인되지 않으므로 외부 TAP/pcap 또는 SIGMATEK vendor hook이 필요
> 이번 요청은 가능성 검토만 수행했으며 파일은 수정하지 않았습니다.
>
> 아까 검토한 내용을 토대로 구현 설계를 진행해봐

<details><summary>16 previous messages</summary>

> 기존 가능성 검토를 실제 구현 가능한 설계로 내리겠습니다. 새 설계 문서에는 RT/Non-RT 경계, LASAL 클래스 통합 지점, PI/Bulk/Recorder 상태기계, TCP 명령·청크 형식, 메모리 예산, WPF API, 검증 기준을 확정하고, Elmo식 static 호환 계층은 “후속 구현”으로 명시하겠습니다. 우선 현재 변경 상태와 기존 설계·코드의 실제 확장 지점을 다시 확인하겠습니다.
>
> 필수 LASAL 규칙을 확인했습니다. 이번 설계는 기존 `TCPMotionInterface`의 non-RT 명령 경로에 RT 샘플링을 억지로 넣지 않고, EtherCAT/PDO 갱신 직후의 별도 RT producer와 TCP 업로드 consumer를 분리하는 방향으로 잡겠습니다. 현재 작업 트리의 API 가이드 변경은 건드리지 않겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 설계 문서는 기존 인스턴스 기반 `LMCConnection`을 코어로 유지하고, Elmo식 static/handle 계층은 마지막 후속 단계로 명시하겠습니다. 지금은 실제 클래스·프로토콜·버퍼 상태기계·검증 게이트까지 구현자가 바로 착수할 수 있는 수준으로 구체화하고 있습니다.
>
> 조사 결과 한 가지를 설계에 바로 반영합니다. 현재 4개 Elmo 축에 실제 활성화된 PDO는 축당 6개뿐이며, `ActSpeed/ActTorque/FollowingError`는 클래스에 있어도 현재 PDO에는 없습니다. 따라서 Catalog는 “클래스에 존재”와 “현재 PDO 활성”을 분리하고, 비활성 신호를 실시간 PI처럼 노출하지 않도록 하겠습니다.
>
> <details><summary>Created a file, edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+31 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_STRUCTURE_DECISION_2026-07-09.md` (+18 -0)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+1205 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+0 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_STRUCTURE_DECISION_2026-07-09.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+236 -92)
>
> </details>
>
> 프로토콜은 `0x7E00~0x7EFF` 로컬 확장으로 묶고 capability negotiation을 앞에 두었습니다. Recorder chunk는 이론상 1,920 bytes까지 가능하지만, 현재 direct-send 실패 시 세션 전체가 fault되는 구조라 1차 기본값을 1,280 bytes로 낮췄습니다. 32채널이면 chunk당 10 sample이며 실기 재캡처 후에만 상향하도록 설계했습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+75 -0)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+122 -8)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+5 -5)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+22 -0)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+8 -2)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+2 -1)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+2 -1)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+12 -0)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+4 -4)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+3 -2)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+4 -0)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+3 -0)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+16 -4)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+19 -8)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+63 -24)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+18 -8)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+54 -6)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+1 -0)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+68 -14)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+7 -0)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+20 -3)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+16 -0)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+47 -10)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+5 -3)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+3 -0)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+11 -11)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 설계 문서의 2차 교차 검토와 프로토콜 크기·링크·Git diff 검증만 마무리하고 있습니다. 검토에서 발견되는 모순은 문서에 바로 반영한 뒤, 구현 착수 기준까지 정리해서 드리겠습니다.
>
> 2차 검토에서 실제 구현 시 문제가 될 계약 모순이 6건 확인됐습니다. 특히 Recorder ID의 PLC 재부팅 식별자, SDO Read 결과 반환, Bulk wire header 누락은 그대로 두면 양쪽 구현이 달라질 수 있어 지금 설계에서 확정하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+16 -7)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+6 -1)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+9 -1)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+8 -3)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+44 -30)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+13 -6)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+13 -3)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+10 -6)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+4 -3)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+4 -3)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+29 -3)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+8 -0)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+4 -2)
>
> </details>
>
> 최종 재검토에서는 critical/major 이슈가 더 나오지 않았습니다. 마지막으로 Recorder v1에서 사용하지 않는 trigger 필드를 모두 0으로 강제하는 규칙까지 명시해, C#과 PLC의 golden-packet 구현이 애매하지 않도록 닫겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+4 -2)
>
> </details>
</details>

구현 가능한 수준의 통합 설계를 작성했습니다.

- [EtherCAT PI/Bulk/Recorder 통합 구현 설계](/C:/work/Elmo/Elmo_Master/docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md)
- [API 구조 결정 문서](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/docs/API_STRUCTURE_DECISION_2026-07-09.md)
- [개발 백로그](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md)

설계에 확정한 주요 내용은 다음과 같습니다.

- EtherCAT input 갱신 직후 RT sample 경로와 Non-RT TCP 경로 분리
- Health, 정적 Signal Catalog, Read-only PI, 동일-cycle Bulk 구조
- Recorder fixed bank, 상태 머신, reconnect와 buffer 소유권
- `DiagnosticsBootId + RecordId + BufferId` 기반 stale 데이터 차단
- `0x7E00~0x7EFF` LASAL 전용 diagnostics wire와 전체 byte offset
- 기본 Recorder chunk `1,280 B`, 전체 응답 `1,340 B`
- 32채널 최대 Recorder bank `4,000,000 B`
- SDO ticket, polling, cancel, 최대 12-byte inline 결과
- WPF Plot down-sampling과 CSV metadata
- D0~D6 구현 순서 및 단계별 완료 기준
- Elmo식 static/handle API는 D6 후속 facade로 연기하고 현재 instance core 유지

검증 결과:

- 로컬 문서 링크 누락 0개
- wire 크기 모두 현재 `1,320 B` request 및 `2,048 B` staging 제한 이내
- 독립 3차 설계 검토에서 critical/major 잔여 문제 없음
- `git diff --check`, `git diff --cached --check` 통과

이번에는 설계 문서만 작성했으며 PLC/C#/WPF 기능은 아직 구현하지 않았습니다. 기존의 별도 `API_DEVELOPMENT_GUIDE.md/html` 변경도 건드리지 않았고 커밋은 생성하지 않았습니다. 다음 구현 시작점은 D0 protocol skeleton과 `DiagnosticsBootId` 초기화입니다.

> 구현문서 기준으로 구현을 진행해보자
>
> User context
>
> Attachments:
> - LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md: `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md`

<details><summary>14 previous messages</summary>

> 설계의 단계 순서를 그대로 따르겠습니다. 먼저 D0의 공통 프로토콜·capability·모델·golden packet 기반을 실제 코드로 만들고 회귀 검증한 뒤, 같은 기반 위에 D1 Health/Catalog/PI까지 연결하겠습니다. 기존에 남아 있는 API 가이드 변경은 작업 범위에서 제외해 보존합니다.
>
> 구현 현실을 확인해 보니 retained boot counter는 LASAL IDE에서 채널/초기화까지 등록해야 하므로, 텍스트 소스에서 세션 번호를 BootId처럼 가장하면 reconnect 의미가 틀어집니다. D0/D1의 stateless 기능에서는 `BootId=0`을 명시적 sentinel로 허용하고, D2 이상 stateful capability를 켜기 전에 retained BootId를 필수 gate로 두겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+15 -7)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+44 -3)
>
> </details>
>
> D0 wire는 현재 PC/PLC 양쪽에 들어가고 있습니다. PLC 쪽은 `0x7E00`에 68-byte capability payload를 반환하며, 아직 없는 기능을 성공처럼 보이지 않도록 모든 capability bit를 0으로 유지했습니다. retained BootId가 구현되기 전에는 Bulk/Recorder/ticket 기능을 켜지 않도록 코드와 설계에 함께 고정했습니다.
>
> D1 조사 결과도 나왔습니다. 현재 프로젝트에는 TCP interface에서 EtherCAT master/slave를 읽을 연결이 없고, master의 “모든 input PDO callback 직후” 공식 hook도 노출되어 있지 않습니다. 따라서 D1을 텍스트 파일만으로 억지로 붙이면 동일-cycle 보장을 거짓으로 만들게 됩니다. 이번 단계에서는 D0를 완결하고, D1은 LASAL IDE에서 class/network/RT 실행 순서를 실제로 등록·증명하는 작업부터 시작하도록 gate를 남기겠습니다.
>
> D0 구현은 병렬 검증에서 53개 PC 테스트와 두 WPF 예제 빌드까지 통과했습니다. 이제 제가 동일한 VS2019 전체 테스트를 다시 실행하고, 변경된 모든 wire offset과 문서 상태를 독립적으로 대조하겠습니다.
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+18 -2)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+32 -3)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+7 -5)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+40 -0)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+10 -8)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+13 -8)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+15 -8)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -2)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> D0 구현은 완료된 상태입니다. 지금 독립 리뷰 결과를 회수한 뒤 전체 diff, 프로토콜 오프셋, 공백·ASCII·네트워크 계약을 한 번 더 확인하고, D1을 소스만으로 진행할 수 없는 정확한 경계까지 정리하겠습니다.
>
> 프로토콜 대조상 D0 프레임은 요청 16바이트, 응답 76바이트로 C#과 PLC 오프셋이 일치합니다. 기능 비트는 0이고 `DiagnosticsBootId=0`은 상태 보유 기능을 하나도 광고하지 않을 때만 허용되도록 클라이언트에서 차단했습니다. 이제 동일한 VS2019 검증 파이프라인을 최종 재실행합니다.
>
> 독립 리뷰에서 실제 계약 불일치 2건이 발견됐습니다. 길이가 8바이트보다 큰 잘못된 요청도 공통 헤더의 `RequestId`는 echo해야 하고, PC가 예약한 `RequestId=0`을 PLC도 거부해야 합니다. 두 항목을 바로 수정하고 해당 정적·패킷 테스트를 추가하겠습니다.
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+15 -9)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+2 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+4 -1)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+5 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsContractTests.cs` (+21 -0)
>
> </details>
>
> 리뷰 수정 과정에서 `TCPMotionInterface.st`에 15개의 단독 LF가 섞인 것을 확인했습니다. 파일의 기존 CRLF 형식을 유지하도록 줄바꿈 바이트만 정규화한 뒤 다시 diff와 테스트를 확인하겠습니다.
>
> 독립 재검토에서 두 오류 모두 해소됐고 새 회귀는 없었습니다. 자동 검증도 다시 통과했습니다. 마지막으로 작업 트리를 분류해 이번 구현 파일과 기존 사용자 문서 변경을 분리해서 보고하겠습니다.
</details>

구현 문서 기준으로 첫 단계인 D0 capability vertical slice를 완료했습니다.

- C#: [`LMCConnection.Diagnostics.GetCapabilities()`](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/src/LmcDiagnostics.cs)와 Async API 구현
- PLC: [`0x7E00 GetDiagnosticsCapabilities`](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:1074) 처리 구현
- Wire: 요청 16 bytes, 응답 76 bytes, `RequestId` echo
- 잘못된 길이에서도 ID가 있으면 echo하며, `RequestId=0`은 `BoundsInvalid`로 거부
- D0는 의도적으로 `CapabilityBits=0`, `MapRevision=0`, `DiagnosticsBootId=0`
- stateful 기능을 BootId 없이 광고하면 C#에서 malformed response로 차단

관련 계약과 테스트도 반영했습니다.

- [DiagnosticsContractTests.cs](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsContractTests.cs)
- [DINT_PACKET_MAP.txt](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt:43)
- [구현 설계 문서](/C:/work/Elmo/Elmo_Master/docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md)

검증 결과:

- C# 테스트 53/53 PASS
- LASAL source-only 계약 PASS
- LASAL full-network 정적 계약 PASS
- WPF 예제 2개 VS2019 Debug 빌드 PASS
- `git diff --check` PASS
- LASAL 추가 코드 7-bit ASCII PASS
- 독립 재검토 PASS

현재 중요한 경계는 명확합니다. D1 EtherCAT Health/Catalog/PI는 아직 구현하지 않았습니다. 모든 PDO input callback 이후이면서 motion 계산 전이라는 실행 순서를 현재 소스만으로 보장할 수 없기 때문입니다. 다음 단계는 LASAL IDE에서 diagnostics class/channel/network를 등록하고 System Trace로 RT 순서를 증명한 뒤 capability bit 0~2를 활성화하는 것입니다.

LASAL IDE Rebuild/Link, PLC 다운로드, 실제 `0x7E00` 패킷 캡처는 아직 수행하지 않았습니다. 커밋도 하지 않았으며, 기존 `API_DEVELOPMENT_GUIDE.md/html` 변경은 건드리지 않았습니다.

> 계속 진행하자

<details><summary>57 previous messages</summary>

> 다음 단계는 D1입니다. 우선 병렬로 C# Health/Catalog/PI 클라이언트 구현, D1 wire 계약 감사, LASAL RT/IDE 연결 지점을 나눠 확인하겠습니다. 소스만으로 보장할 수 있는 부분은 바로 구현하고, 실제 RT 순서가 필요한 부분은 IDE 등록까지 이어가겠습니다.
