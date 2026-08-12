# Axis DS402 Home Ex typed input 계약

## 1. 목적

개발 진행표의 우선순위 `상`, No.22 `MMC_HomeDS402ExCmd`를 대상으로 한다.
이번 단계는 확장 DS402 Homing을 실행하는 기능이 아니라, 향후 전용 command가 사용할
입력 의미와 송신 전 검증을 C# SDK에 먼저 고정하는 단계다.

기존 `0x7D15/0x7D16/0x7D17`은 method 37, Home offset 0의 비이동
`LMC_HomeDS402` 계약이다. 이 계약과 durable outcome identity를 변경하거나
`HomeDS402Ex`로 확장하지 않는다.

## 2. 근거와 현재 경계

Maestro API reference의 `MMC_HomeDS402Ex`는 Distributed mode와 Standstill에서만
동작하며 Position, DetectionVelocityLimit, Acceleration, VelocityHigh,
VelocityLow, DistanceLimit, TorqueLimit, BufferMode, HomingMethod, TimeLimit,
DetectionTimeLimit과 32-byte spare를 입력으로 가진다. Position은 검출 위치에 더하는
일반 목표가 아니라 완료 위치를 `-Position`으로 만드는 offset 의미다.

현재 LASAL 프로젝트에는 다음 조건이 없다.

- 위 engineering-unit 값을 Elmo drive object 값으로 변환하는 승인된 scale/range
- 축별 Home/limit/index/block 검출 입력과 active level/debounce 증거
- method별 travel, torque, detection timeout 안전 정책
- 확장 Homing의 exact-once start/outcome/retirement를 보존할 독립 retained store
- 기존 DS402 Home과 공유하는 mode/SDO executor ownership의 승인된 전이 계약

따라서 이번 단계에서는 command ID, request/response wire, Admin feature bit,
LASAL route/handler, WPF control을 추가하지 않는다. Capability 광고, SDO write,
native Homing 호출과 축 이동은 모두 0이다.

## 3. 구현 범위

C# SDK에는 `LMCAxisDs402HomeExParameters`를 추가한다. 이 형식은 Maestro 입력을
engineering-unit 값으로 보존하고 다음을 송신 전에 차단한다.

- NaN 또는 Infinity
- 0 이하 acceleration, high/low velocity
- 음수 torque limit
- 0인 overall/detection timeout
- 정의되지 않은 buffer enum. 문서는 같은 절에서 `Aborting` 미지원이라고 쓰면서
  바로 뒤 설명에는 이를 default mode로 적어 서로 모순된다. typed model은 정의된
  `1..6` 값을 보존만 하고 실제 허용 조합은 activation 전에 별도로 확정한다.
- standard candidate `1..14`, `17..30`, `33..34` 밖의 homing method와
  reserved/obsolete method
- standard candidate에서 0이 아닌 distance limit. `-1..-4` Gold Home-on-Block은
  별도 drive/object/wiring qualification 대상으로 분류만 하고 아직 생성하지 못한다.
- 정확히 32 bytes가 아니거나 하나라도 0이 아닌 spare

이 검증은 PLC가 해당 method를 지원한다는 뜻이 아니다. 입력 model을 생성할 수 있어도
전용 wire와 capability가 없으므로 현재 SDK에서 실행할 수 없다.

## 4. 다음 구현 게이트

다음 단계는 아래 순서로 진행한다.

1. 실제 장비/배선 기준 method allowlist와 축별 unit scale/range를 승인한다.
2. 기존 method 37 record와 분리된 source-bound start/outcome/retire wire 및 retained
   record를 설계한다.
3. DS402 mode 전이와 D5 SDO executor의 단일 owner를 유지하는 LASAL state machine을
   구현한다.
4. Capability OFF 상태의 parser/admission/fail-closed static test를 먼저 통과한다.
5. LASAL IDE Build 뒤 generated artifact를 승인하고 같은 image를 Download한다.
6. 축별 normal, limit/index/block, timeout, torque, disconnect, abort, recovery matrix와
   packet/readback을 확인한 뒤에만 capability와 WPF control을 활성화한다.

PC model/unit test PASS는 입력 계약 증거일 뿐 LASAL build, PLC runtime 또는 물리 Homing
증거가 아니다. 2026-08-12 VS2019 SDK 전체 회귀는 Debug/Release 각각
`1138/1138` PASS했다.
