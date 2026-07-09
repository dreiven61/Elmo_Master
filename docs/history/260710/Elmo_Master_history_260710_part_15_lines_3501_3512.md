  - 현재 소스에는 command 상수만 있음: [LmcProtocol.cs](./LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs:98)
  - public API, frame builder, 16축 position response parser 없음
- `0x20E7 SetKinTransform`
  - 캡처 있음: [PACKET_ANALYSIS.md](./LMC_Library/LMC_API/Elmo_API_Packet2/PACKET_ANALYSIS.md:323)
  - 현재 `LmcProtocol.cs`에 command 상수도 없고 frame builder도 없음
  - 분석 문서에서도 payload 1320 bytes이고 구조 확정 전이라고 적혀 있음

정리하면:

**현재 LASAL motion 핵심 API는 구현되어 있지만, 캡처된 패킷 전체를 기준으로 하면 `GroupReadActualPosition(0x2051)`과 `SetKinTransform(0x20E7)`이 남아 있다.**

`PowerMembers`, `PrepareGroupMCS`는 단일 command가 아니라 여러 패킷을 순서대로 호출하는 helper 성격이라 라이브러리 필수 packet API로 보긴 어렵다. 다만 `PrepareGroupMCS` 안에 `SetKinTransform`이 들어가면, 그 부분은 아직 미구현이다.
