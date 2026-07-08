# SNET-ECAT Library vs Maestro Administrative/Motion API 비교 분석 (2026-06-23)

## 전제

- 비교 원본 1: `Chapter6_Library_(250508).pdf`, 435 pages, SNET-P/SNET-ECAT Library, 2025-05-09
- 비교 원본 2: `Maestro Administrative and Motion API_2022_12_v2.012.pdf`, 2435 pages, Maestro Administrative and Motion API v2.012
- 이 표의 “성능”은 실측 ms/us 수치가 아니다. 두 문서는 동일 조건의 benchmark를 제공하지 않는다. 따라서 controller-side execution, PC 왕복 호출 감소, group/path 처리, trigger/capture 전용성 같은 구조적 성능 요소로 비교했다.

## 한줄 결론

Maestro는 Group Motion, kinematics, PVT/ECAM, PI/Bulk/Event/Recording, 통신/관리 기능이 강하다. SNET은 SNET 장치 계열의 단축/보간 이송, IO, 위치 동기 Trigger/Capture/Latch, ADC/DAC, SNET-ECAT 노드 접근이 강하다. 현재 Group Motion P1-P4-P1, transition mode, group InPosition 테스트는 Maestro API 모델이 원본 기준이고, SNET API에는 직접 대응 기능이 없다.

## 기능/성능 비교표

| 영역 | SNET-ECAT Library | Maestro API | 없는 기능/차이 | 성능/구현 영향 |
|---|---|---|---|---|
| 제어기 연결/세션 | 지원: UDP/TCP 계열 연결 `eSnetConnect*`, 통신 설정/응답시간 확인 | 지원: RPC/IPC/TCP/UDP 연결, 축/그룹 리소스 로딩, 네트워크 설정 | 둘 다 있음. Maestro는 리소스/그룹 객체 기반, SNET은 net/axis 번호 기반이 더 단순함. | Maestro는 초기화가 복잡하지만 리소스 관리가 세밀함. SNET은 단순 호출/응답 확인에 유리함. |
| 단축 Servo/Home/Move/Status | 지원: Servo On/Off, STEP/Method Homing, 단축 Move, 상태/에러/위치 | 지원: Power, Home/DS402 Home, MoveAbs/Rel/Add/Vel/Torque/Continuous, ReadStatus/Position/Torque/Velocity | 둘 다 있음. Maestro는 DS402/PLCopen 형태가 더 풍부함. | 단축 기본 성능 비교는 문서만으로 정량화 불가. API 폭은 Maestro가 넓음. |
| 축 파라미터 | 지원: `Param002`, `Param010` 등 축 파라미터별 전용 Set/Get가 많음 | 지원: generic/global parameter, bool parameter, group parameter, resource import/export/save/load | 둘 다 있음. SNET은 전용 함수형, Maestro는 범용 파라미터/리소스형. | 대량/범용 관리와 자동화는 Maestro 쪽이 유리함. 특정 SNET 파라미터 접근은 SNET이 명확함. |
| Group 객체/Group 상태 | 문서상 전용 group object와 `GroupReadStatus` 등은 없음 | 지원: `MMC_AddAxisToGroup`, `MMC_GroupEnable/Disable/Reset`, `MMC_GroupReadStatus`, `MMC_GroupReadActualPosition/Velocity/Error` | Maestro에 있음, SNET에는 직접 대응 기능 없음. | 다축 InPosition/standby/error를 그룹 단위로 판정하는 테스트는 Maestro API가 직접 적합함. |
| 좌표계/기구학 변환 | 문서상 Maestro식 kinematic transform API 없음 | 지원: `MMC_SetKinTransform*`, Cartesian/SCARA/ThreeLink/Hxpd, conveyor/rotary tracking | Maestro에 있음. SNET은 보간 축 지정 중심. | 로봇/기구 좌표계 기반 다축 테스트는 Maestro가 훨씬 강함. |
| 선형/원호/스플라인 보간 | 지원: `eSnetMoveLine`, `MoveLineMultiAxis`, `Arc*`, `Helical`, `Spline` | 지원: `MMC_MoveLinear*`, `MoveCircular*`, `MovePolynomAbsolute`, `PathSelect/MovePath` | 둘 다 있음. Maestro는 group/path 모션 체계, SNET은 보간 이송 함수 체계. | 컨트롤러 측 보간 실행은 양쪽 모두 가능. 블렌딩/전이 제어는 Maestro 쪽이 명시적임. |
| Transition/Buffer/Blending | 문서상 Maestro식 `BufferMode`, `TransitionMode`, transition parameter 직접 대응 없음 | 지원: Group Motion의 buffer/transition 개념, path/repetitive motion, kinematic group 명령 | Maestro에 있음. SNET은 연속 보간 job/trigger는 있으나 동일 개념은 아님. | P1-P4-P1 블렌딩/InPosition 경계 시험은 Maestro가 직접 대상임. SNET은 별도 경로/연속 보간 방식으로 재설계 필요. |
| 연속 경로/큐 | 지원: Conti channel/job 생성/시작, job index/result, dwell/output trigger | 지원: repetitive motion, path/table/PVT, function-block depth/status | 둘 다 있음. 기능 모델이 다름. | PC 반복 호출 대신 컨트롤러에 경로를 올리는 구조는 양쪽 모두 가능함. |
| PVT | 문서상 PVT 전용 장 없음 | 지원: Chapter 8 PVT, table init/load/move/append/index | Maestro에 있음. | 고밀도 궤적/시간 기반 프로파일은 Maestro가 우세함. |
| Electronic CAM/Gearing | 문서상 ECAM/Gear 전용 API 없음 | 지원: `MMC_Cam*`, `MMC_GearIn/GearInPos/GearOut` | Maestro에 있음. | 마스터-슬레이브 CAM/gear 동기 응용은 Maestro가 직접 지원함. |
| Gantry | 지원: `eSnetEnableGantrySync`, Gantry homing 상태 | 전용 Gantry 장/함수는 문서상 제한적. group/coupling/kinematic으로 구현 여지 | SNET에는 명시적 gantry API가 있음. | 단순 gantry sync/homing은 SNET 함수가 바로 보임. Maestro는 프로젝트 구성 방식 확인 필요. |
| Trigger 출력 | 강함: 거리/위치/보간 중 trigger, SNET-P/RTEX/ECAT/RTEX-2LAN 별 장 제공 | TouchProbe/Event/Data Recording/IO는 있으나 SNET식 multi-trigger API 묶음은 없음 | SNET에 전용 기능이 훨씬 많음. | 고속 위치 기반 출력은 SNET이 API상 더 직접적임. Maestro는 대체 구현 검토 필요. |
| Capture/Latch | 강함: Latch, Position Capture 1/2/3, RTEX/ECAT fieldbus capture | TouchProbe, Data Recording, PI/event 기반 접근은 있음. SNET식 capture API 묶음은 없음 | SNET에 명시적 capture/latch가 많음. | 외부 신호 위치 캡처/계측은 SNET이 문서상 더 직접적임. |
| I/O | 지원: SNET-P, SNET-P-AD, RTEX Option, RTEX IO Slave, Remote IO, SNET-ECAT IO Node, ADC/DAC | 지원: Digital IO, Process Image, DS-401 CAN I/O, EtherCAT IO analog/digital, Modbus/EtherNet/IP | 둘 다 강하지만 대상 버스/장치 모델이 다름. | SNET 보드/노드 I/O는 SNET이 직접적. Maestro는 산업통신/PI와 연동 폭이 큼. |
| EtherCAT/Drive 통신 | 지원: SNET-ECAT CoE PDO/SDO, ESC register, state machine, brake | 지원: EtherCAT config/statistics/diagnostics/IO, CANbus PDO/SDO, interpreter command | 둘 다 있음. Maestro는 drive/network admin 폭이 넓음. | 진단/통신 상태/드라이브 직접 명령은 Maestro가 더 넓고, SNET-ECAT 노드 함수는 SNET 쪽이 단순함. |
| Process Image/Bulk Read | 전용 PI/Bulk Read 장은 문서상 없음 | 지원: PI read/write, PI bulk read, bulk parameter read | Maestro에 있음. | 고주기 상태 수집/대량 파라미터 읽기는 Maestro 구조가 유리함. |
| Recording/Event | 사용자 로그, interrupt event, trigger/capture 상태 확인 | 지원: data recording, API events/callback, events mask, notifications, error policy | 둘 다 있으나 Maestro는 callback/event/recording 계층이 더 큼. | 장시간 진단/비동기 이벤트 처리는 Maestro가 유리함. |
| Firmware/Admin | API/OS/FPGA version 확인, 통신 설정 중심 | FoE download/status, version path/download, resource import/export/save/load, reset system | Maestro에 admin 기능이 많음. | 장비 운영/배포/복구 자동화는 Maestro가 우세함. |
| 언어/래퍼 | C/C#/VB 선언 예제가 포함됨 | C API, C++ wrapper, IEC 61131-3 special functions, Python functions가 별도 장으로 제공됨 | Maestro 래퍼 문서가 더 방대함. | PC 자동화/테스트 언어 선택 폭은 Maestro가 넓음. |

## 성능 구조 비교

| 성능 관점 | SNET | Maestro | 판단 |
|---|---|---|---|
| PC 왕복 호출 지연 | 전용 `eSnetCheckCommunicationTime`로 응답 시간 확인 가능 | 개별 Cmd/Status 구조와 `CmdStatus`, PI/Bulk Read로 관리 | 동일 조건 실측값은 문서에 없음. 호출 구조만 보면 반복 폴링 최소화는 Maestro의 PI/Bulk, SNET의 controller-side trigger/capture가 각각 유리함. |
| 컨트롤러 측 경로 실행 | 연속 보간 job/conti channel 지원 | Path/PVT/Repetitive/Group motion 지원 | 둘 다 PC 루프 의존도를 낮출 수 있음. 복잡 경로/블렌딩은 Maestro가 더 강함. |
| 고속 출력/캡처 | Trigger/Capture/Latch 전용 장이 많음 | TouchProbe/Event/Recording은 있으나 SNET식 trigger/capture 전용 폭은 작음 | 위치 동기 출력/캡처 계측은 SNET 우세. |
| 다축 동기/그룹 제어 | 보간 다축 이동과 gantry 중심 | Group object, status, kinematics, transition/blending 중심 | GroupReadStatus/InPosition/Transition Mode 테스트는 Maestro 우세. |
| 대량 상태 수집 | 개별 상태/좌표/파라미터 함수 중심 | Process Image, PI Bulk Read, Bulk Parameter Read 제공 | 다수 변수/고주기 모니터링은 Maestro 구조가 우세. |
| 통신/드라이브 진단 | SNET-ECAT CoE/ESC/state/brake 확인 | EtherCAT/CANbus/DS401/EtherNetIP/Modbus, diagnostics/statistics | 네트워크/드라이브 진단 폭은 Maestro 우세. |

## 어디에는 있고 어디에는 없는가

| 분류 | SNET에 있고 Maestro에 약하거나 직접 없음 | Maestro에 있고 SNET에 직접 없음 | 둘 다 있음 |
|---|---|---|---|
| Motion | 명시적 Gantry sync/homing | Group object/status, kinematics, transition/blending, PVT, ECAM/Gear | 단축 이동, homing, status, linear/circular 계열 보간 |
| I/O/계측 | Trigger 34-41장, Latch, Position Capture, RTEX/ECAT fieldbus capture, ADC/DAC | PI/PI bulk, Data Recording, Event/callback | Digital IO, EtherCAT/fieldbus 연동 |
| 통신/관리 | SNET-ECAT CoE/ESC/state/brake 전용 함수 | FoE download, resource import/export, EtherCAT/CANbus/DS401/EtherNetIP/Modbus, interpreter command | 연결, 버전, 에러, 파라미터 관리 |
| 개발/래퍼 | C/C#/VB 선언 예제 | C/C++/IEC/Python wrapper와 class 문서가 큼 | PC 앱에서 DLL/API 호출 가능 |

## 프로젝트 적용 판단

- `Codex_PMAS_WPF`의 `MMC_MoveLinearAbsoluteCmd`/`MMC_GroupReadStatusCmd` 테스트는 Maestro Chapter 7의 Group Motion 모델을 그대로 따른다.
- `Codex_LASAL_WPF`에서 같은 테스트를 흉내 내려면 SNET 방식이 아니라 LASAL 내부 `_LMCRobotBase`/Motion Network의 group 상태를 TCP 프레임으로 매핑해야 한다.
- InPosition 조건, transition mode, blending은 SNET API에서 동일 명령을 찾는 방식으로는 해결되지 않는다. Maestro group status와 LASAL robot/group status 사이의 의미 대응표가 필요하다.
- 반대로 trigger/capture/latch/ADC/DAC 계측 기능을 테스트할 때는 SNET 문서가 더 직접적인 기준이다. Maestro에서는 TouchProbe, Recording, PI, EtherCAT IO/event 기반으로 대체 설계를 해야 한다.
