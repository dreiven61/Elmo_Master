# Elmo Master 2026-08-13 historical continuation summary

작성일: 2026-08-13 (KST)

정리일: 2026-08-19 (KST)

## 문서 성격

이 문서는 2026-08-13 세 작업 기록의 핵심 결론만 보존한 역사 스냅샷이다.
원본 대화 로그, 250줄 단위 청크, digest와 manifest는 로컬 생성물이라 2026-08-19
정리에서 제거했다. 아래 내용은 당시 상태이며 현재 source, build, PLC 상태로 읽지 않는다.

현재 상태는 다음 문서를 우선한다.

- `docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md`
- `docs/architecture/AXIS_SET_POSITION_ASYNC_RT_EXECUTOR_AND_RECOVERY_DESIGN_2026-08-19.md`
- `docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.md`

## 당시 결론

1. PC reconnect V2는 commit `dde24aa`에 구현·회귀됐다. 첫 candidate의 persistent
   `0x8080/-1` 또는 응답 전 제한된 transport failure에만 fresh TCP 1회를 허용하고,
   candidate 2 실패는 terminal로 처리했다.
2. 실제 PLC에서 두 candidate가 모두 `0x8080/-1`로 거부된 원인은 LASAL
   `IsClientConnected()`의 DINT 반환값에 bitwise `NOT`을 사용한 한 줄 결함으로
   좁혀졌다. 당시 작업본은 명시적 0 비교로 수정됐다.
3. 2026-08-13 09:16 LASAL 로그에는 282-file Download와 `Download Ok`, project load
   성공이 기록됐다. 수정 이미지에서 첫 RPC init과 callback `0x405C` 등록은
   Candidate 1에서 성공한 것으로 기록됐다.
4. 정상 `Close -> Disconnected -> Connect`는 완료되지 않았다. 오래된
   `SdoWrite / OutcomeUnverified` journal과 BootId 불일치가 GUI close/reconnect를
   막는 별도 정책 문제로 남아 있었다.
5. 따라서 당시 완료 범위는 결함 원인 확인, LASAL 한 줄 수정, 시험 이미지 Download,
   첫 연결·등록 성공까지였다. process owner-loss 후 재실행, callback terminal,
   clean Close, 같은 창 second Connect, pcap과 production 승인 증거는 남아 있었다.

## 당시 병행 개발

- owner-loss retirement와 callback tuple/queue 정리 계약을 source/static으로 강화했다.
- `HomeDS402Ex`는 typed input과 PC fail-fast 계약까지만 추가됐고 LASAL wire/runtime
  기능은 없었다.
- SetPosition retirement는 `0x7D1A`, capability bit 7, detail `16..24`, strict parser의
  PC 계약까지 진행됐으며 PLC retained store와 runtime route는 아직 없었다.
- API 진행도 workbook은 PC/정적 진척과 PLC/패킷/실기 진척을 분리해 산출했지만,
  그 로컬 출력 파일은 저장소 증거로 채택하지 않았다.

## 당시 증거 수준

- PC 회귀, SourceOnly와 C78 결과는 PC/static/IDE 증거였다.
- test-image Download 기록은 production provenance나 실축 E2E를 뜻하지 않았다.
- callback 등록 성공은 UDP wake, authoritative TCP terminal, queue drain과 정상
  reconnect 전체를 증명하지 않았다.
- 당시 process 상태에 의존하던 즉시 실행 절차는 이후 작업으로 폐기돼 보존하지 않았다.

## 이후 상태

이 스냅샷의 SetPosition·recovery 상태는 이후 commit `e03352b`, `1b3011d`,
`e2a4328`, `d254f0e`에서 크게 갱신됐다. 현재 판단에는 2026-08-19 source, verifier,
C78 결과와 current architecture/status 문서를 사용한다. PLC download/runtime/hardware
증거는 별도로 확인해야 한다.
