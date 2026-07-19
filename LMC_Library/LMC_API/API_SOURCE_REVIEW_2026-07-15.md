# LMC_Library 배포 준비 소스 리뷰

검토일: 2026-07-15, 현재 tree 재확인 2026-07-16

검토 대상: `LMC_API_Delivery`, `LasalApiWpfTestApp`, `LMC_API`, LASAL adapter

## 결론

현재 C# source에서 serializer/parser의 즉시 수정해야 할 P0 wire 계약 불일치는
발견하지 않았다. 다만 command별 payload 상한 적용 시점, callback 신뢰 경계,
cancellation 이후 command 결과 불명 같은 P1 hardening 항목은 남아 있다. 기존
배포 바이너리와 문서는 현재 source보다 오래되어 그대로 배포하면 안 된다. 새
배포는 `LMC_API_Distribution`에서 fresh Release DLL과 독립 예제를 사용하도록
재구성했다.

배포 등급은 production이 아니라 `0.9.1-preview`다. PC 자동/정적 검증은
통과 대상으로 구성했지만 actual PLC E2E는 아직 0/25다.

## 발견 및 조치

| 우선순위 | 발견 | 영향 | 조치 |
|---|---|---|---|
| P1 | 기존 `LMC_API_Delivery/bin` 및 `LMC_API/LMC_API` DLL이 최신 public API보다 오래됨 | 새 WPF 예제에서 missing method 또는 compile failure | Delivery의 중복 DLL 제거, fresh Release DLL만 Distribution에 복사; legacy package는 경고 후 보관 |
| P1 | old/new DLL이 모두 AssemblyVersion 0.9.0.0 | 잘못된 DLL 교체를 binding이 검출하지 못함 | API/예제를 0.9.1.0, product 0.9.1-preview로 올림 |
| P1 | 기존 예제 solution이 API source project를 포함 | 저장소 밖 독립 build 불가 | 배포 solution은 예제 프로젝트만 포함, `..\..\01_API` DLL 상대 참조 |
| P2 | 구버전 package 문서가 42/42, 18/23, 5 unsupported를 current처럼 표기 | 기능/검증 상태 오해 | legacy archive 경고 추가, 신규 문서 분리 |
| P2 | packet map이 axis descriptor 1..4라고 표기 | 현재 9축 dispatcher와 불일치 | single-axis 1..9, group 4축 제한으로 정정 |
| P2 | unit 문서 일부가 IntUnits=10 mm라고 표기 | 현재 Git network의 1 mm 설정과 불일치 | Git 기준 1 mm/10000으로 정정, live PLC 확인 요구 |
| P2 | 자동 test가 개발 예제만 build | binary-reference package 회귀를 놓침 | 배포 예제 build target을 `RunTests`에 추가 |
| P2 | `LMC_Response.Raw/Payload`가 내부 배열을 그대로 노출 | 소비자 변경이 response 진단값을 훼손 | defensive clone getter/setter와 회귀 검증 추가 |
| P2 | 수동 manifest가 rebuild 후 stale될 수 있음 | hash/version/commit 오표기 | current distribution 내부 manifest를 제거하고 build script가 세 DLL hash 동일성을 검사해 console에 출력; 내부 경로를 scan |

## 확인한 설계 강점

- request/response를 command별 exact shape로 검증한다.
- response byte array는 defensive copy로 반환한다.
- malformed response를 정상값 0으로 바꾸지 않는다.
- connection exchange가 직렬화되어 TCP response 순서를 보존한다.
- reconnect generation으로 stale axis/group handle을 거부한다.
- async lookup은 generation 확인과 request 전송을 같은 gate에서 처리한다.
- caller-side UNIT 책임과 encoder `ExUnits`를 분리한다.
- group Power, profile Lock/Unlock과 motion 명령을 별도 API로 노출한다.
- 9축 single-axis와 4축 Cartesian group 범위를 코드/문서에서 구분한다.
- callback은 확인되지 않은 schema를 추정하지 않고 raw datagram만 노출한다.

## 남은 위험

1. LASAL IDE 최신 Rebuild/Link와 Find in Implementation smoke 기록이 없다.
2. PLC 실제 다운로드본이 Git의 `IntUnits=1 mm`, SW limit, 9축 wiring과 같은지
   확인되지 않았다.
3. PLC E2E/pcap 재캡처가 0/25이므로 source-active와 장비 검증을 혼동하면 안 된다.
4. callback typed payload, motion completion callback과 multi-PC ownership은 없다.
5. `CloseConnection`, `Dispose`, cancellation은 안전 정지가 아니다.
6. DLL은 strong-name/AuthentiCode 서명이 없다.
7. response reader는 command별 payload 상한을 확인하기 전에 header의 `UInt16`
   길이만큼 읽으므로 비정상 peer가 최대 65,535-byte 대기/할당을 유발할 수 있다.
8. async API는 blocking socket을 `Task.Run`으로 감싸고 connection별 exchange를
   직렬화한다. pipelining이 없고 송신 뒤 취소되면 PLC 적용 여부를 확정할 수 없다.
9. callback은 controller IP만 검증하며 source port, 인증, 무결성, typed schema가 없다.

## 배포 포함/제외 기준

포함:

- `01_API/LasalMotionControlLib.dll`
- `02_Example_Program`의 API source와 연결되지 않은 WPF example solution/source와
  prebuilt `Run` EXE/DLL
- `03_API_User_Manual`의 canonical DOCX/PDF
- package README

현재 package 안에는 manifest/checksum 파일을 두지 않는다. build script는 DLL과
manual hash를 console에 출력하며 `RELEASE_MANIFEST`/`BUILD_METADATA` 문자열을
distribution text에서 금지한다. release provenance를 보존하려면 distribution
외부의 승인 기록에 build console, source commit과 hash를 저장한다. 현재 snapshot은
[`BUILD_METADATA_2026-07-16.md`](../LMC_API_Delivery/docs/BUILD_METADATA_2026-07-16.md)에
기록했다.

제외:

- API source/tests, LASAL source, pcap/TXT packet evidence
- `bin`, `obj`, `.vs`, PDB
- legacy `LasalMotionControlLibTestApp`
- 내부 backlog/design/history
- old `0.9.0-pc-api` DLL과 manifest

## 배포 전 승인 체크

- [ ] Git commit과 dirty 여부 기록
- [ ] VS2019 Release rebuild
- [ ] PC 46 tests PASS
- [ ] LASAL source-only/full-network static contract PASS
- [ ] 배포 예제 Debug/Release 독립 build PASS
- [ ] `01_API` DLL과 Run DLL SHA-256 동일
- [ ] assembly/file/product version 확인
- [ ] `ProjectReference`/absolute path/internal path 없음
- [ ] LASAL IDE/PLC 검증 상태와 build console hash를 외부 승인 기록에 사실대로 기록
- [ ] 실제 장비 UNIT, home, limit, E-stop 확인
