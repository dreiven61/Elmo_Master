> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+9 -6)
> - Wrote `./docs/architecture/AXIS_STOP_STABLE_STANDSTILL_2026-07-29.md` (+4 -3)
>
> </details>
>
> 코드와 현재 문서 수치를 모두 맞췄습니다. 마지막으로 LASAL SourceOnly 계약, 관련 파일의 공백 오류, 외부 시험본과의 해시 동일성, 그리고 기존 스테이지가 건드려지지 않았는지를 한 번에 확인하겠습니다. LASAL IDE/PLC는 열지 않습니다.
</details>

반영 완료했습니다.

- 외부 시험에서 통과한 동일 IP 소켓 takeover 소스 4개를 마스터에 반영했고, 시험 폴더와 SHA-256이 모두 동일합니다.
  - [TCPIPServer.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPIPServer/TCPIPServer.st)
  - [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st)
  - Comm Network `.lcn`/table
- 같은 IP 재접속 시 기존 소켓 shutdown, 새 RPC owner 교체, 이전 소켓 데이터·늦은 disconnect 격리를 적용했습니다.
- SDK/WPF의 Stop·Reset durable 복구와 실제 프로세스 강제종료 복구도 반영했습니다.
- Reset 완료 직후 Stop NACK 경로는 최종 D0 identity가 일치할 때만 해제하며, mismatch면 `RecoveryRequired`로 유지하도록 수정했습니다.

검증 결과:

- SDK Debug/Release: 각각 `974/974 PASS`
- WPF Release: warning/error `0/0`, `206/206 PASS`
- LASAL SourceOnly: `Phase5TransportClean / StaticTopologyOnly PASS`
- 독립 검토: 남은 P1/P2 없음
- working-tree `git diff --check`: PASS
- cached 검사는 기존 staged [Elmo_Master_history_260721.md](./docs/history/Elmo_Master_history_260721.md)의 trailing whitespace 때문에만 실패했습니다. 해당 파일과 stage는 건드리지 않았습니다.

현재 중요한 제한은 다음입니다.

- 마스터의 `Classes.lcb`, `Networks.lcb`, `.lba` 등은 아직 이전 `_TCPIPServer_RT` 기준 생성물입니다.
- 마스터 프로젝트에서 LASAL Save/Generate → Rebuild/Link → `Find in Implementation` smoke → PLC 다운로드가 필요합니다.
- 그 후 비정상 종료→동일 IP 재접속→첫 명령 응답을 확인하면 됩니다.
- 다른 IP 거절, peer 조회 실패, 반복 reconnect/soak는 아직 실기 미검증입니다.

상세 현황은 [ELMO_TCP_SAME_PEER_TAKEOVER_TEST_2026-07-29.md](./docs/architecture/ELMO_TCP_SAME_PEER_TAKEOVER_TEST_2026-07-29.md)와 [API_DEVELOPMENT_PROGRESS_2026-07-30.md](./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.md)에 동기화했습니다. 커밋·추가 스테이징·테스트 폴더 복사는 하지 않았습니다.
