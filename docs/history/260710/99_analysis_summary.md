# Elmo Master History 260710 Analysis Summary

## 분석 범위

- 원본: `docs/history/Elmo_Master_history_260710.md`
- 원본 크기: 218,876 bytes, 3,512 lines
- 분할본: `docs/history/260710/`의 15개 청크
- 무결성: 원본과 청크 재결합 SHA-256 일치
- 분석 기준일: 2026-07-10

이 문서는 히스토리에 기록된 주장과 현재 저장소에서 다시 확인한 사실을
구분해, 다음 작업을 바로 재개할 수 있도록 정리한다.

## 2026-07-10 최신 구현 업데이트

아래의 기존 분석 스냅샷 이후 `996686d`를 baseline으로 다음 source 변경을
목적별 checkpoint commit으로 반영했다. 아래 오래된 "Live LASAL server와의 계약 불일치"
절은 변경 전 근거이므로 현재 상태 판단에는 이 업데이트와
`LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md`를
우선한다.

- tracked `Elmo_EtherCAT_Test_4Axis`를 canonical source로 확정
- `_LMCAxis1..4`/`_LMCRobotBase1` 실제 이름을 `_GetObjName`으로 읽고
  opaque descriptor `1..4`/`0x0100`을 발급하는 LASAL registry 구현
- `TCPMotionInterface`에 4개 typed axis client를 연결하고 generated client
  table count를 6으로 교정
- single-axis DINT Power/Reset/Stop/Read/Move와 group lookup/members/
  enable/disable/status/linear handler 반영
- legacy `0x2081..0x2084` 차단, 32-bit axis error 상위 비트 truncation 방지,
  지원하지 않는 direction/velocity-decel 조합 deterministic 거부
- C# exact ACK/lookup/typed read/1350-byte group-members parser, short error
  envelope 보존, strict RPC init/callback shape 검증 구현
- WPF의 `8,388,608`을 caller-side 23-bit encoder dummy profile로 명시하고
  DLL에는 UNIT/CPR 변환을 넣지 않음
- WPF response 실패 판정과 native LASAL PowerOn bit 0/Standstill bit 25 교정
- 이전 checkpoint에서 Debug/Release 30개 C# test와 LASAL static contract suite 통과
- 이후 PC API에 `GroupReadActualPosition(0x2051)` LASAL-DINT request와 exact
  68-byte `DINT[16]+status/error` typed result를 추가했다. PMAS legacy
  136-byte LREAL response는 거부한다.
- PC API에 `SetKinTransformCartesian4Axis(0x20E7)` exact 1320-byte serializer를
  추가했다. 공개 범위는 캡처된 X/Y/Z/U identity-shift, Cartesian,
  `Buffered(2)` profile로 제한한다.
- group coordinate/transition/buffer/execute 옵션, 배열/enum validation,
  connection timeout/state/async/cancellation, callback source-address 검증과
  raw payload 방어 복사를 반영했다.
- connection lifecycle은 timeout/전송 오류와 in-flight 취소 시 transport를
  폐기하고 `Faulted`로 전환한다. queued cancellation은 active request를
  보존하며 invalid reconnect는 기존 session을 유지한다. reconnect 뒤 stale
  axis/group object는 거부하고, generation 검증과 frame write를 같은 exchange
  gate에서 수행한다. 취소 가능한 axis/group factory와
  initialization/transport/close 오류 분리도 반영했다.
- PC runner 42/42 PASS로 신규 packet, lifecycle/async와 stale-handle 경로를
  검증했다.
- test target을 `RunPcTests`, `RunLasalContract`, combined `RunTests`로 나눠
  PC 실패와 LASAL static contract 실패를 구분했다. 현재 PC 42/42와 LASAL
  static contract(6 clients, 4 links, offsets/error guards/legacy block)가
  모두 PASS다.
- callback typed parser는 실제 datagram 캡처가 없어 만들지 않았다.
  다중 PC의 읽기 공유와 motion/control ownership은 LASAL server 정책으로
  남겼다.
- WPF에 async/cancel, connection/callback 상태와 raw datagram log,
  `GroupReadActualPosition`, `SetKinTransformCartesian4Axis`와 group option UI를
  반영했다. lookup도 async factory를 사용하고 종료 시 RPC cleanup을 기다리며,
  Debug/Release solution build를 확인했다.
- callback listener는 listener 세대를 캡처하고 stop 시 shared field를 먼저
  원자적으로 분리해, join timeout 뒤 이전 thread가 재연결 listener를 소비하거나
  새 endpoint/thread 추적값을 지우지 못하게 했다.
- assembly는 `0.9.0.0`으로 versioning하고 current Release DLL/EXE와 SHA-256을
  package `RELEASE_MANIFEST.md`에 기록하고 release artifact로 추적했다.

현재 구현 완료도는 다음과 같이 판정한다.

- 대상 command 23개 중 C# request/public path는 23개다.
- tracked LASAL에는 21개 `case` handler가 있지만, `0x2049`와 `0x2085`는
  정상 기능이 아니라 unsupported error `-5`만 반환한다.
- 따라서 정상 기능 후보 source path는 19개이며, 이 수치는 실제 PLC 완료
  개수가 아니다.
- LASAL IDE compile, PLC smoke test와 Wireshark 재캡처를 통과한 command는
  현재 0개다.
- PC packet API는 23개 source path를 갖췄지만, single-PC P0 MVP에서는 LASAL
  command queue/RtWork와 PLC 검증이 가장 큰 잔여다.
- `0x2051`과 `0x20E7`은 PC 구현을 완료했으나 LASAL handler, coordinate
  mapping/large-command staging과 E2E 검증이 남았다.
- callback은 raw-only가 현재 완료 범위이며 typed payload는 근거 확보 뒤
  추가한다. multi-session/ownership은 LASAL 구현 범위다. PC preview binary와
  hash manifest는 만들었지만 LASAL/PLC 검증 전 production 승인본은 아니다.

관련 commit:

- `16e94c8`: PC API packet, lifecycle와 42개 test
- `b2da80e`: WPF async/cancel/callback/group UI
- `b67a96c`: packet/API/backlog/migration 문서
- `da4a912`: LASAL object dispatcher/DINT handler prototype
- `adeb631`: typed response, packet validation과 30개 자동 테스트
- `6dd7eab`: WPF dummy UNIT profile과 response 안전 판정
- `bcbde89`: obsolete `LmcMotionApiTestApp` 제거

현재 즉시 재개할 작업은 LASAL 추가 구현이 아니라 설계 승인이다.

1. `LASAL_COMMAND_QUEUE_RTWORK_DESIGN_2026-07-10.md`의 D0~D15를 함께 확정한다.
2. 특히 AP task/CyWork/RtWork 배치(D10)와 SIGMATEK RT-safe atomic/barrier(D4)가
   확인되기 전에는 queue/RtWork 구현을 시작하지 않는다.
3. 승인 후 `ReadActualPosition` one-slot부터 IDE model, compile, PLC 검증을
   단계적으로 진행한다.

그 뒤 남은 기능은 승인된 GroupReset/GroupStop semantics, `0x2051` LASAL
coordinate mapping/68-byte response, `0x20E7` large-command staging/apply,
실제 UDP callback sender/payload, multi-PC ownership 순서다. 현재 LASAL
prototype은 IDE compile/PLC download를 하지 않았고 production 승인 상태가
아니다.

## 분석 당시 결론 (위 최신 업데이트로 대체됨)

히스토리의 최신 장기 목표는 PMAS/Maestro API 전체 복제가 아니다.
`LMC_Library/LMC_API_Delivery`를 LASAL 전용 DINT 클라이언트로 유지하고,
Wireshark에서 확인된 패킷 중 실제 필요한 기능만 하나의 public API로
구현하는 것이다.

히스토리의 마지막 결론과 현재 C# 소스는 일치한다. 캡처 목록 중 아래 두
명령은 아직 public API/frame/parser가 없다.

- `0x2051 GroupReadActualPosition`
- `0x20E7 SetKinTransform`

그러나 live LASAL 소스 대조 결과, 이 두 API보다 먼저 해결해야 할 계약
불일치가 있다. 현재 C# DLL의 DINT/RPC 계약은 Git 추적 중인
`Lasal_PRG/Elmo_EtherCAT_Test_4Axis/.../TCPMotionInterface.st`와 호환되지
않는다. 따라서 다음 작업은 곧바로 `0x2051`을 추가하는 것이 아니라, 실제
배포할 LASAL 프로젝트를 확정하고 PC/PLC 프레임 계약을 먼저 맞추는 것이다.

## 히스토리에서 확정된 설계 결정

1. 개발 기준 소스는 `Codex_LASAL_WPF` 더미가 아니라
   `LMC_Library/LMC_API_Delivery`이다.
2. API 입력은 호출자가 이미 변환한 LASAL internal `DINT`이며, DLL 내부에서
   단위 변환하지 않는다.
3. `LMC_Units`는 `unit.h` 기반 상수 선언만 제공하고 실행 경로에서는 참조하지
   않는다.
4. Axis/Group 객체는 이름 lookup으로 얻은 reference를 보관하고 이후 명령
   frame에 자동 삽입한다.
5. public API는 한 기능당 하나만 둔다. 기존 `LMC_*Cmd` 중복 alias와
   public-to-public wrapper chain은 제거한다.
6. `PowerMembers`와 `PrepareGroupMCS`는 단일 packet API가 아니라 사용자
   프로그램 또는 테스트 앱 helper로 본다.
7. 연결은 TCP socket만 여는 것으로 끝내지 않고 캡처 기반
   `0x8080 -> 0x405C -> commands -> 0x405D` 흐름을 사용한다.
8. callback은 현재 UDP listener가 raw payload를 전달하는 1차 구현이다.
9. 응답은 공통 envelope를 먼저 파싱한 뒤 acknowledgement, lookup, value
   parser로 나눈다.
10. 과거의 고정 `8,388,608 count -> 3,600,000` 변환과 PMAS LREAL packet
    크기는 현재 DLL의 완료 기준이 아니다.

## 주요 작업 흐름

히스토리는 아래 순서로 진행됐다.

1. 대규모 폴더 재배치 커밋을 docs/WPF/LMC/test/LASAL 목적별 커밋으로
   분리하고 원격 재배치 브랜치를 갱신했다.
2. legacy DLL IL patch와 고정 회전 스케일을 시험했지만 패킷과 `unit.h`
   분석 뒤 폐기했다.
3. 캡처를 `PACKET_ANALYSIS.md`로 정리하고 source-backed DINT delivery
   library를 만들었다.
4. 객체/reference 구조, 단위 책임, `MMC`에서 `LMC`로의 이름 변경,
   assembly/test-app 이름을 차례로 정리했다.
5. session/RPC 설계, callback listener, envelope 기반 응답 parser를
   구현했다.
6. 중복 public alias를 제거하고 테스트 앱을 현재 API와 project reference에
   맞췄다.
7. 마지막으로 API 문서를 현재 LASAL DINT 구현과 캡처 범위 기준으로 다시
   작성했다.

현재 HEAD까지 이어지는 핵심 커밋은 다음과 같다.

- `48f76ef` packet capture 분석
- `09ff75c` `unit.h` scale 적용 단계
- `c0fab9d` API object 구조 개편
- `c583807` unit 상수를 선언으로 복원
- `7207d08` `MMC` delivery symbols를 `LMC`로 변경
- `4a044b0` RPC handshake 구현
- `5d7f176` callback listener 수명주기 구현
- `6869441` envelope 기반 응답 파싱
- `82e106f` LASAL 대상 기준 frame builder rename
- `e649614`, `02ea0ba` wrapper chain과 중복 alias 제거
- `7adead4` 테스트 앱 API/project reference 갱신
- `eb49db6`, `3c90a68` line-ending 정책과 테스트 앱 포맷 정리
- `cbdfdac` 현재 LASAL API/capture 범위 문서화

## 초기 분석 당시 저장소에서 재검증한 사실

이 절은 위 최신 PC 구현 전 스냅샷이다. 현재 판단에는 "2026-07-10 최신
구현 업데이트"와 backlog를 우선한다.

### Git과 C# library

- 현재 branch는 `main`, HEAD는 `cbdfdac Document current LASAL API packet
  scope`이다.
- `main`은 `origin/main`보다 23 commits 앞서 있다.
- `LMC_Library/LMC_API_Delivery/**`, `API_LIST.md`, `LMC_PACKET_MAP.md`에는
  현재 working-tree diff가 없다.
- `LmcProtocol.cs`에는 `0x2051` 상수만 있고 request builder가 없다.
- `LmcGroup.cs`에는 group actual-position public API와 parser가 없다.
- `0x20E7`은 command 상수, frame builder, public API가 모두 없다.
- `API_LIST.md`와 `LMC_PACKET_MAP.md`도 두 packet을 미구현으로 기록한다.

현재 소스 빌드는 성공했다.

- `LasalMotionControlLib.sln`, Release rebuild: 성공
- `LasalMotionControlLibTestApp.sln`, Debug rebuild: 성공

이는 C# compile 검증일 뿐 PLC와의 실제 왕복 검증은 아니다.

### 캡처 자료

`LMC_Library/LMC_API/Elmo_API_Packet2/PACKET_ANALYSIS.md`에는 다음이
기록되어 있다.

- `0x2051`: request payload 8 bytes, 캡처값 coordinate system `2`, execute
  `1`; response payload 136 bytes이며 위치는 LREAL vector로 해석
- `0x20E7`: request payload 1,320 bytes; 전체 kinematic structure는 아직
  확정하지 않음

따라서 `0x2051`도 current LASAL DINT response 계약을 먼저 확정해야 하며,
`0x20E7`은 캡처와 Maestro 구조체를 더 분석하기 전 추측 구현하면 안 된다.

### Live LASAL server와의 계약 불일치

Git 추적 중인
`Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/`는 현재 C#
library와 다음이 다르다.

- tracked parser는 reference를 offset 2에서 읽지만 C# request header는
  reference를 offset 6에 쓴다.
- tracked parser의 Power/Reset/Stop IDs는 `0x2081`~`0x2084`이고 C#
  library는 `0x2023`, `0x2024`, `0x2022`를 쓴다.
- tracked Move/Group Move parser는 LREAL offsets를 읽지만 C# library는
  DINT payload를 쓴다.
- tracked parser에는 `0x8080`, `0x405C`, `0x405D`, `0x2051`, `0x20E7`
  case가 없다.

untracked `Lasal_PRG/Elmo_EtherCAT_Test_4Axis_Edit/`는 header의 payload
length/reference offset과 일부 command ID를 개선했지만, Move 계열은 여전히
LREAL이고 RPC 및 `0x2051`/`0x20E7` case가 없다.

`LMC_API_Delivery/docs/DINT_PACKET_MAP.txt`의 parser replacement는 설계
지침이며, 두 LASAL 폴더의 live `.st`에 적용된 상태가 아니다. 따라서 현재
repo의 C# DLL과 LASAL server source는 있는 그대로는 end-to-end 호환된다고
볼 수 없다.

## 기존 working tree 변경 주의

이번 히스토리 분할 전부터 working tree에는 사용자 변경이 있었다. 이 분석
작업에서는 해당 파일을 수정하거나 stage하지 않았다.

관련성이 큰 기존 변경은 다음과 같다.

- delivery package의 `README.md`, `sample/BasicUsage.cs`,
  `LMC_API_함수명_커맨드ID_인자.txt`가 수정 상태
- legacy `LmcMotionApi` DLL/EXE와 old test-app source가 삭제 상태
- `LasalMotionControlLib.dll`과 `LasalMotionControlLibTestApp.exe`가
  package 아래 untracked 상태
- package sample은 namespace만 새 이름이지만 아직 제거된 legacy
  `LMC_*Cmd` 메서드를 호출함
- 함수/Command ID TXT도 current public API가 아니라 legacy API 목록을
  유지함
- package의 untracked DLL/EXE는 이번 live rebuild 산출물과 byte hash가
  일치하지 않음

그 밖에도 `docs/history/260708/**`, reorganization inventory, generated HTML,
untracked LASAL Edit 폴더가 남아 있다. 다음 구현 커밋에 이 변경들을 섞지
말아야 한다.

## 작업 재개 지점

권장 순서는 다음과 같다.

1. 실제 배포 대상 LASAL source가 tracked
   `Lasal_PRG/Elmo_EtherCAT_Test_4Axis`인지 untracked `..._Edit`인지 먼저
   확정한다.
2. `LmcProtocol.cs`, 선택한 `TCPMotionInterface.st`,
   `SigmatekTcpIpDummyMMCLib.cs`를 command ID, payload length, reference,
   response offset 단위로 대조한다.
3. 선택한 LASAL source에 DINT header/motion contract와 RPC
   `0x8080`/`0x405C`/`0x405D` 처리 및 응답을 맞추고 실제 PLC handshake를
   검증한다.
4. 그 기준 위에서 `0x2051 GroupReadActualPosition`을 PC와 PLC 양쪽에
   구현한다. response가 DINT vector인지 captured LREAL vector인지 계약을
   먼저 확정한다.
5. `0x20E7 SetKinTransform`은 1,320-byte payload 구조를 캡처와 Maestro
   구조체로 확정한 뒤 구현한다.
6. 각 단계에서 library/test app, `API_LIST.md`, `LMC_PACKET_MAP.md`,
   packet test를 함께 갱신하고 `git diff --check`를 실행한다.

즉시 다음 기술 작업은 `0x2051` 코딩 자체가 아니라 PC/PLC 계약과 canonical
LASAL source 확정이다. 이 정합성을 건너뛰면 C# build는 성공해도 실제 PLC
통신은 실패한다.

## 현재 검증의 한계

- C# library와 WPF test app compile은 확인했다.
- 실제 SIGMATEK PLC 다운로드/실행은 이 환경에서 확인하지 않았다.
- callback transport가 실제 장비에서도 UDP인지와 callback payload 구조는
  캡처로 확정되지 않았다.
- current response parser 검증은 히스토리상 synthetic frame/loopback
  수준이며 실제 controller wire response 전 범위를 증명하지 않는다.
