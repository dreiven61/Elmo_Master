<!--
Source: docs/history/Elmo_Master_history_260624.md
Original lines: 2001-2250
Part: 9 of 10
-->

# Elmo_Master_history_260624 part 09

Source lines: 2001-2250

>         maestro_entries.append(e)
> maestro_entries=sorted(maestro_entries, key=lambda e:[int(x) for x in e['section'].split('.')])
> maestro_groups=defaultdict(list)
> for e in maestro_entries:
>     major=e['section'].split('.')[0]
>     maestro_groups[major].append(e)
> maestro_group_rows=[]
> for major in sorted(maestro_groups, key=lambda x:int(x)):
>     maestro_group_rows.append((int(major), f'{major}. {chapter_titles.get(major, "")}'.strip(), maestro_groups[major]))
>
> # comparison matrix data
> feature_rows=[
>  ('제어기 연결/세션', '지원: UDP/TCP 계열 연결 `eSnetConnect*`, 통신 설정/응답시간 확인', '지원: RPC/IPC/TCP/UDP 연결, 축/그룹 리소스 로딩, 네트워크 설정', '둘 다 있음. Maestro는 리소스/그룹 객체 기반, SNET은 net/axis 번호 기반이 더 단순함.', 'Maestro는 초기화가 복잡하지만 리소스 관리가 세밀함. SNET은 단순 호출/응답 확인에 유리함.'),
>  ('단축 Servo/Home/Move/Status', '지원: Servo On/Off, STEP/Method Homing, 단축 Move, 상태/에러/위치', '지원: Power, Home/DS402 Home, MoveAbs/Rel/Add/Vel/Torque/Continuous, ReadStatus/Position/Torque/Velocity', '둘 다 있음. Maestro는 DS402/PLCopen 형태가 더 풍부함.', '단축 기본 성능 비교는 문서만으로 정량화 불가. API 폭은 Maestro가 넓음.'),
>  ('축 파라미터', '지원: `Param002`, `Param010` 등 축 파라미터별 전용 Set/Get가 많음', '지원: generic/global parameter, bool parameter, group parameter, resource import/export/save/load', '둘 다 있음. SNET은 전용 함수형, Maestro는 범용 파라미터/리소스형.', '대량/범용 관리와 자동화는 Maestro 쪽이 유리함. 특정 SNET 파라미터 접근은 SNET이 명확함.'),
>  ('Group 객체/Group 상태', '문서상 전용 group object와 `GroupReadStatus` 등은 없음', '지원: `MMC_AddAxisToGroup`, `MMC_GroupEnable/Disable/Reset`, `MMC_GroupReadStatus`, `MMC_GroupReadActualPosition/Velocity/Error`', 'Maestro에 있음, SNET에는 직접 대응 기능 없음.', '다축 InPosition/standby/error를 그룹 단위로 판정하는 테스트는 Maestro API가 직접 적합함.'),
>  ('좌표계/기구학 변환', '문서상 Maestro식 kinematic transform API 없음', '지원: `MMC_SetKinTransform*`, Cartesian/SCARA/ThreeLink/Hxpd, conveyor/rotary tracking', 'Maestro에 있음. SNET은 보간 축 지정 중심.', '로봇/기구 좌표계 기반 다축 테스트는 Maestro가 훨씬 강함.'),
>  ('선형/원호/스플라인 보간', '지원: `eSnetMoveLine`, `MoveLineMultiAxis`, `Arc*`, `Helical`, `Spline`', '지원: `MMC_MoveLinear*`, `MoveCircular*`, `MovePolynomAbsolute`, `PathSelect/MovePath`', '둘 다 있음. Maestro는 group/path 모션 체계, SNET은 보간 이송 함수 체계.', '컨트롤러 측 보간 실행은 양쪽 모두 가능. 블렌딩/전이 제어는 Maestro 쪽이 명시적임.'),
>  ('Transition/Buffer/Blending', '문서상 Maestro식 `BufferMode`, `TransitionMode`, transition parameter 직접 대응 없음', '지원: Group Motion의 buffer/transition 개념, path/repetitive motion, kinematic group 명령', 'Maestro에 있음. SNET은 연속 보간 job/trigger는 있으나 동일 개념은 아님.', 'P1-P4-P1 블렌딩/InPosition 경계 시험은 Maestro가 직접 대상임. SNET은 별도 경로/연속 보간 방식으로 재설계 필요.'),
>  ('연속 경로/큐', '지원: Conti channel/job 생성/시작, job index/result, dwell/output trigger', '지원: repetitive motion, path/table/PVT, function-block depth/status', '둘 다 있음. 기능 모델이 다름.', 'PC 반복 호출 대신 컨트롤러에 경로를 올리는 구조는 양쪽 모두 가능함.'),
>  ('PVT', '문서상 PVT 전용 장 없음', '지원: Chapter 8 PVT, table init/load/move/append/index', 'Maestro에 있음.', '고밀도 궤적/시간 기반 프로파일은 Maestro가 우세함.'),
>  ('Electronic CAM/Gearing', '문서상 ECAM/Gear 전용 API 없음', '지원: `MMC_Cam*`, `MMC_GearIn/GearInPos/GearOut`', 'Maestro에 있음.', '마스터-슬레이브 CAM/gear 동기 응용은 Maestro가 직접 지원함.'),
>  ('Gantry', '지원: `eSnetEnableGantrySync`, Gantry homing 상태', '전용 Gantry 장/함수는 문서상 제한적. group/coupling/kinematic으로 구현 여지', 'SNET에는 명시적 gantry API가 있음.', '단순 gantry sync/homing은 SNET 함수가 바로 보임. Maestro는 프로젝트 구성 방식 확인 필요.'),
>  ('Trigger 출력', '강함: 거리/위치/보간 중 trigger, SNET-P/RTEX/ECAT/RTEX-2LAN 별 장 제공', 'TouchProbe/Event/Data Recording/IO는 있으나 SNET식 multi-trigger API 묶음은 없음', 'SNET에 전용 기능이 훨씬 많음.', '고속 위치 기반 출력은 SNET이 API상 더 직접적임. Maestro는 대체 구현 검토 필요.'),
>  ('Capture/Latch', '강함: Latch, Position Capture 1/2/3, RTEX/ECAT fieldbus capture', 'TouchProbe, Data Recording, PI/event 기반 접근은 있음. SNET식 capture API 묶음은 없음', 'SNET에 명시적 capture/latch가 많음.', '외부 신호 위치 캡처/계측은 SNET이 문서상 더 직접적임.'),
>  ('I/O', '지원: SNET-P, SNET-P-AD, RTEX Option, RTEX IO Slave, Remote IO, SNET-ECAT IO Node, ADC/DAC', '지원: Digital IO, Process Image, DS-401 CAN I/O, EtherCAT IO analog/digital, Modbus/EtherNet/IP', '둘 다 강하지만 대상 버스/장치 모델이 다름.', 'SNET 보드/노드 I/O는 SNET이 직접적. Maestro는 산업통신/PI와 연동 폭이 큼.'),
>  ('EtherCAT/Drive 통신', '지원: SNET-ECAT CoE PDO/SDO, ESC register, state machine, brake', '지원: EtherCAT config/statistics/diagnostics/IO, CANbus PDO/SDO, interpreter command', '둘 다 있음. Maestro는 drive/network admin 폭이 넓음.', '진단/통신 상태/드라이브 직접 명령은 Maestro가 더 넓고, SNET-ECAT 노드 함수는 SNET 쪽이 단순함.'),
>  ('Process Image/Bulk Read', '전용 PI/Bulk Read 장은 문서상 없음', '지원: PI read/write, PI bulk read, bulk parameter read', 'Maestro에 있음.', '고주기 상태 수집/대량 파라미터 읽기는 Maestro 구조가 유리함.'),
>  ('Recording/Event', '사용자 로그, interrupt event, trigger/capture 상태 확인', '지원: data recording, API events/callback, events mask, notifications, error policy', '둘 다 있으나 Maestro는 callback/event/recording 계층이 더 큼.', '장시간 진단/비동기 이벤트 처리는 Maestro가 유리함.'),
>  ('Firmware/Admin', 'API/OS/FPGA version 확인, 통신 설정 중심', 'FoE download/status, version path/download, resource import/export/save/load, reset system', 'Maestro에 admin 기능이 많음.', '장비 운영/배포/복구 자동화는 Maestro가 우세함.'),
>  ('언어/래퍼', 'C/C#/VB 선언 예제가 포함됨', 'C API, C++ wrapper, IEC 61131-3 special functions, Python functions가 별도 장으로 제공됨', 'Maestro 래퍼 문서가 더 방대함.', 'PC 자동화/테스트 언어 선택 폭은 Maestro가 넓음.'),
> ]
>
> perf_rows=[
>  ('PC 왕복 호출 지연', '전용 `eSnetCheckCommunicationTime`로 응답 시간 확인 가능', '개별 Cmd/Status 구조와 `CmdStatus`, PI/Bulk Read로 관리', '동일 조건 실측값은 문서에 없음. 호출 구조만 보면 반복 폴링 최소화는 Maestro의 PI/Bulk, SNET의 controller-side trigger/capture가 각각 유리함.'),
>  ('컨트롤러 측 경로 실행', '연속 보간 job/conti channel 지원', 'Path/PVT/Repetitive/Group motion 지원', '둘 다 PC 루프 의존도를 낮출 수 있음. 복잡 경로/블렌딩은 Maestro가 더 강함.'),
>  ('고속 출력/캡처', 'Trigger/Capture/Latch 전용 장이 많음', 'TouchProbe/Event/Recording은 있으나 SNET식 trigger/capture 전용 폭은 작음', '위치 동기 출력/캡처 계측은 SNET 우세.'),
>  ('다축 동기/그룹 제어', '보간 다축 이동과 gantry 중심', 'Group object, status, kinematics, transition/blending 중심', 'GroupReadStatus/InPosition/Transition Mode 테스트는 Maestro 우세.'),
>  ('대량 상태 수집', '개별 상태/좌표/파라미터 함수 중심', 'Process Image, PI Bulk Read, Bulk Parameter Read 제공', '다수 변수/고주기 모니터링은 Maestro 구조가 우세.'),
>  ('통신/드라이브 진단', 'SNET-ECAT CoE/ESC/state/brake 확인', 'EtherCAT/CANbus/DS401/EtherNetIP/Modbus, diagnostics/statistics', '네트워크/드라이브 진단 폭은 Maestro 우세.'),
> ]
>
> # SNET doc
> snet_lines=[]
> snet_lines.append('# SNET-ECAT Chapter6 Library API 분석 (2026-06-23)')
> snet_lines.append('')
> snet_lines.append('## 분석 대상')
> snet_lines.append('')
> snet_lines.append('- 원본: `C:/work/자료/EMotion/SNET-ECAT-User-Manual-25.05.08-ko/SNET-ECAT User Manual 25.05.08 ko/Chapter6_Library_(250508).pdf`')
> snet_lines.append('- PDF 정보: 435 pages, title `SNET-P`, 생성/수정일 2025-05-09')
> snet_lines.append('- 추출 기준: PDF 목차/섹션명에서 `eSnet*` API를 추출했다. 자동 추출 기준 목차 API 섹션은 300개, 원문 전체의 unique `eSnet*` 심볼은 371개다. 차이는 예제/enum/중복 참조가 포함되기 때문이다.')
> snet_lines.append('')
> snet_lines.append('## 결론')
> snet_lines.append('')
> snet_lines.append('SNET API는 `net + axis` 중심의 장치 제어 라이브러리다. 단축 이송, 보간 이송, 연속 보간, 위치/속도 override, gantry, IO, trigger, capture, EtherCAT CoE/ESC 접근을 폭넓게 제공한다. 특히 위치 동기 trigger/capture, RTEX/ECAT fieldbus capture, ADC/DAC 같은 장비 I/O 계측 기능이 강하다.')
> snet_lines.append('')
> snet_lines.append('반대로 Maestro의 Group Motion처럼 group object를 만들고 `GroupReadStatus`, transition mode, kinematic transform으로 다축을 관리하는 구조는 문서상 직접 대응되지 않는다. SNET은 다축 보간과 연속 보간이 중심이고, 그룹 상태/블렌딩 의미는 별도 설계가 필요하다.')
> snet_lines.append('')
> snet_lines.append('## API 영역별 정리')
> snet_lines.append('')
> snet_lines.append('| 영역 | API 수 | 대표 API | 판단 |')
> snet_lines.append('|---|---:|---|---|')
> for _,title,entries in snet_groups:
>     names=[e['name'] for e in entries]
>     if 'Trigger' in title: judgement='위치/거리 기반 출력 기능. SNET의 강점 영역.'
>     elif 'Capture' in title or 'Latch' in title: judgement='외부 신호 기반 위치 계측 기능. SNET의 강점 영역.'
>     elif '보간' in title: judgement='컨트롤러 보간 실행. Group object는 아니지만 다축 경로 구현 가능.'
>     elif '이더켓' in title or 'ECAT' in title: judgement='SNET-ECAT 노드/CoE/ESC 제어 영역.'
>     elif '입/출력' in title or 'ADC' in title or 'DAC' in title: judgement='장비 I/O 제어 기능이 세분화되어 있음.'
>     elif '단축' in title or '원점' in title or '서보' in title: judgement='기본 축 제어 기능.'
>     elif '겐트리' in title: judgement='SNET 전용 명시 기능.'
>     else: judgement='기본 관리/상태/파라미터 기능.'
>     snet_lines.append(f'| {md_escape(title)} | {len(names)} | {code_list(names, 8)} | {judgement} |')
> snet_lines.append('')
> snet_lines.append('## 성능 관점')
> snet_lines.append('')
> snet_lines.append('- 정량 latency/throughput benchmark는 이 PDF에 공통 조건으로 제시되어 있지 않다.')
> snet_lines.append('- `eSnetCheckCommunicationTime`이 제공되므로 PC-제어기 응답 시간 자체는 API 레벨에서 측정할 수 있다.')
> snet_lines.append('- 고속 위치 동기 동작은 PC polling보다 controller/fieldbus trigger/capture API를 쓰는 구조가 성능상 유리하다.')
> snet_lines.append('- 다축 경로는 `eSnetMoveLine*`, `eSnetMoveArc*`, `eSnetMoveSpline`, `eSnetBeginContiMakeJob`/`eSnetStartConti` 같은 컨트롤러 실행형 API를 우선 고려해야 한다.')
> snet_lines.append('')
> snet_lines.append('## Maestro 대비 SNET에 강하게 보이는 기능')
> snet_lines.append('')
> snet_lines.append('- 거리/위치/보간 중 Trigger 출력: Chapter 34-41')
> snet_lines.append('- Latch 및 Position Capture: Chapter 43-46')
> snet_lines.append('- SNET-P/RTEX/Remote/SNET-ECAT IO, ADC/DAC: Chapter 27-33, 50')
> snet_lines.append('- 명시적 Gantry sync/homing: Chapter 24-25')
> snet_lines.append('- SNET-ECAT CoE/ESC/state/brake 함수: Chapter 51')
> snet_lines.append('')
> snet_lines.append('## Maestro 대비 부족하거나 직접 대응이 없는 기능')
> snet_lines.append('')
> snet_lines.append('- `MMC_GroupReadStatus` 같은 group object status API')
> snet_lines.append('- `MMC_SetKinTransform*` 같은 기구학/좌표계 transform API')
> snet_lines.append('- Group Motion의 `BufferMode`, `TransitionMode`, transition parameter 중심 블렌딩')
> snet_lines.append('- PVT table motion, Electronic CAM/Gear 기능')
> snet_lines.append('- Process Image, PI Bulk Read, Bulk Parameter Read 계층')
> snet_lines.append('- FoE download/resource import/export 등 Maestro administrative 기능')
> write(out/'SNET_ECAT_Library_API_Analysis_2026-06-23.md','\n'.join(snet_lines))
>
> # Maestro doc
> mae_lines=[]
> mae_lines.append('# Maestro Administrative and Motion API 분석 (2026-06-23)')
> mae_lines.append('')
> mae_lines.append('## 분석 대상')
> mae_lines.append('')
> mae_lines.append('- 원본: `C:/work/Elmo/Elmo_Master/Maestro Administrative and Motion API_2022_12_v2.012.pdf`')
> mae_lines.append('- PDF 정보: 2435 pages, title `Maestro Administrative and Motion`, API version 2.012, 문서 release 2022-12')
> mae_lines.append('- 추출 기준: PDF 목차/섹션명에서 `MMC_*`, `MC_*`, `Eip*` 계열 기능을 추출했다. 원문 전체의 unique `MMC_*` 심볼은 2501개지만 구조체/상수/래퍼/예제 참조가 섞여 있으므로 기능 비교는 목차 섹션 단위로 판단했다.')
> mae_lines.append('')
> mae_lines.append('## 결론')
> mae_lines.append('')
> mae_lines.append('Maestro API는 단순 축 제어 라이브러리가 아니라 motion controller 전체를 관리하는 API다. Single Axis, Multi-Axis/Group Motion, kinematic transform, PVT, ECAM/Gear, Process Image, Data Recording, Bulk Read, Event/Callback, EtherCAT/CANbus/DS401/EtherNetIP/Modbus, firmware/resource 관리까지 포함한다.')
> mae_lines.append('')
> mae_lines.append('현재 `Codex_PMAS_WPF`와 `Codex_LASAL_WPF`에서 다루는 `MMC_MoveLinearAbsoluteCmd`, `MMC_GroupReadStatusCmd`, transition mode, group InPosition 판정은 이 문서의 Chapter 7 Multi-Axis/Group Motion 영역에 직접 해당한다. SNET 문서의 보간 이송과 유사한 축 이동은 가능하지만, group status/transition/blending의 원래 모델은 Maestro 쪽이 기준이다.')
> mae_lines.append('')
> mae_lines.append('## API 영역별 정리')
> mae_lines.append('')
> mae_lines.append('| 장 | API/섹션 수 | 대표 기능 | 판단 |')
> mae_lines.append('|---|---:|---|---|')
> for _,title,entries in maestro_group_rows:
>     major=title.split('.')[0]
>     names=[e['name'] for e in entries]
>     if major=='6': judgement='단축 모션/상태/파라미터. PLCopen/DS402 축 제어가 강함.'
>     elif major=='7': judgement='Group Motion, path, kinematics. 다축 테스트 핵심 영역.'
>     elif major=='8': judgement='PVT table motion. 시간 기반 고밀도 궤적용.'
>     elif major=='9': judgement='ECAM/Gear. master-slave 동기 응용용.'
>     elif major=='10': judgement='연결, 리소스, FoE, 조건 대기, 메모리/파라미터 관리.'
>     elif major=='11': judgement='Process Image read/write와 PI bulk. 대량 상태 수집에 유리.'
>     elif major=='12': judgement='Recorder. 장시간 진단/파형 수집용.'
>     elif major=='13': judgement='Bulk parameter read. 통신 왕복 최소화에 유리.'
>     elif major=='14': judgement='이벤트/callback. 비동기 진단과 motion ended 처리.'
>     elif major in ['19','20','21','22','23']: judgement='드라이브/필드버스 통신과 진단 영역.'
>     elif major in ['24','25','26']: judgement='언어/IEC 래퍼 또는 특수 함수. 기능 중복 포함.'
>     else: judgement='관리/진단 보조 기능.'
>     mae_lines.append(f'| {md_escape(title)} | {len(names)} | {code_list(names, 10)} | {judgement} |')
> mae_lines.append('')
> mae_lines.append('## 성능 관점')
> mae_lines.append('')
> mae_lines.append('- 정량 latency/throughput benchmark는 이 PDF만으로 비교할 수 없다. 문서가 제공하는 것은 기능 구조와 호출 모델이다.')
> mae_lines.append('- Group Motion은 PC가 각 축을 순차 polling/명령하는 구조보다 controller 내부의 group/path/transition 계산을 쓰는 구조다. 다축 동기, kinematics, blending 테스트에는 이 구조가 유리하다.')
> mae_lines.append('- `Process Image`, `PI Bulk Read`, `Bulk Parameters Reading`은 여러 변수를 한 번에 수집하는 구조라 반복 개별 호출보다 통신 왕복을 줄일 수 있다.')
> mae_lines.append('- `Data Recording`과 `API Events`는 PC polling 대신 controller/event 기반 진단을 구성할 수 있게 한다.')
> mae_lines.append('- EtherCAT/CANbus/DS401/Interpreter/EtherNetIP 관련 API가 넓어 장비 통신 진단과 드라이브 직접 명령 자동화에 유리하다.')
> mae_lines.append('')
> mae_lines.append('## SNET 대비 Maestro에 강하게 보이는 기능')
> mae_lines.append('')
> mae_lines.append('- Group object, group status, group parameter, group position/velocity/error read')
> mae_lines.append('- `MMC_SetKinTransform*`, Cartesian/SCARA/ThreeLink/Hxpd, conveyor/rotary tracking')
> mae_lines.append('- Buffer/transition/blending을 포함한 Group Motion과 path motion')
> mae_lines.append('- PVT table motion, Electronic CAM, Gear')
> mae_lines.append('- Process Image, PI Bulk Read, Bulk Parameter Read')
> mae_lines.append('- Data Recording, API Events/callback, error policy')
> mae_lines.append('- FoE download, resource import/export, version path, system reset 등 administrative 기능')
> mae_lines.append('- EtherCAT/CANbus/DS401/EtherNetIP/Modbus 등 통신 기능 폭')
> mae_lines.append('')
> mae_lines.append('## SNET 대비 부족하거나 직접 대응이 약한 기능')
> mae_lines.append('')
> mae_lines.append('- SNET-P/RTEX/SNET-ECAT 별 거리/위치 trigger 전용 API 묶음')
> mae_lines.append('- RTEX/ECAT fieldbus position capture, latch, 사용자 기계좌표 output처럼 문서상 SNET에 특화된 계측 API')
> mae_lines.append('- SNET-P-AD/RTEX Option/Remote IO 같은 SNET 보드/노드 단위 전용 IO API')
> mae_lines.append('- SNET 문서의 명시적 gantry sync/homing 함수명과 동일한 직접 대응 API')
> write(out/'Maestro_Administrative_Motion_API_Analysis_2026-06-23.md','\n'.join(mae_lines))
>
> # Comparison doc
> cmp=[]
> cmp.append('# SNET-ECAT Library vs Maestro Administrative/Motion API 비교 분석 (2026-06-23)')
> cmp.append('')
> cmp.append('## 전제')
> cmp.append('')
> cmp.append('- 비교 원본 1: `Chapter6_Library_(250508).pdf`, 435 pages, SNET-P/SNET-ECAT Library, 2025-05-09')
> cmp.append('- 비교 원본 2: `Maestro Administrative and Motion API_2022_12_v2.012.pdf`, 2435 pages, Maestro Administrative and Motion API v2.012')
> cmp.append('- 이 표의 “성능”은 실측 ms/us 수치가 아니다. 두 문서는 동일 조건의 benchmark를 제공하지 않는다. 따라서 controller-side execution, PC 왕복 호출 감소, group/path 처리, trigger/capture 전용성 같은 구조적 성능 요소로 비교했다.')
> cmp.append('')
> cmp.append('## 한줄 결론')
> cmp.append('')
> cmp.append('Maestro는 Group Motion, kinematics, PVT/ECAM, PI/Bulk/Event/Recording, 통신/관리 기능이 강하다. SNET은 SNET 장치 계열의 단축/보간 이송, IO, 위치 동기 Trigger/Capture/Latch, ADC/DAC, SNET-ECAT 노드 접근이 강하다. 현재 Group Motion P1-P4-P1, transition mode, group InPosition 테스트는 Maestro API 모델이 원본 기준이고, SNET API에는 직접 대응 기능이 없다.')
> cmp.append('')
> cmp.append('## 기능/성능 비교표')
> cmp.append('')
> cmp.append('| 영역 | SNET-ECAT Library | Maestro API | 없는 기능/차이 | 성능/구현 영향 |')
> cmp.append('|---|---|---|---|---|')
> for row in feature_rows:
>     cmp.append('| ' + ' | '.join(md_escape(x) for x in row) + ' |')
> cmp.append('')
> cmp.append('## 성능 구조 비교')
> cmp.append('')
> cmp.append('| 성능 관점 | SNET | Maestro | 판단 |')
> cmp.append('|---|---|---|---|')
> for row in perf_rows:
>     cmp.append('| ' + ' | '.join(md_escape(x) for x in row) + ' |')
> cmp.append('')
> cmp.append('## 어디에는 있고 어디에는 없는가')
> cmp.append('')
> cmp.append('| 분류 | SNET에 있고 Maestro에 약하거나 직접 없음 | Maestro에 있고 SNET에 직접 없음 | 둘 다 있음 |')
> cmp.append('|---|---|---|---|')
> cmp.append('| Motion | 명시적 Gantry sync/homing | Group object/status, kinematics, transition/blending, PVT, ECAM/Gear | 단축 이동, homing, status, linear/circular 계열 보간 |')
> cmp.append('| I/O/계측 | Trigger 34-41장, Latch, Position Capture, RTEX/ECAT fieldbus capture, ADC/DAC | PI/PI bulk, Data Recording, Event/callback | Digital IO, EtherCAT/fieldbus 연동 |')
> cmp.append('| 통신/관리 | SNET-ECAT CoE/ESC/state/brake 전용 함수 | FoE download, resource import/export, EtherCAT/CANbus/DS401/EtherNetIP/Modbus, interpreter command | 연결, 버전, 에러, 파라미터 관리 |')
> cmp.append('| 개발/래퍼 | C/C#/VB 선언 예제 | C/C++/IEC/Python wrapper와 class 문서가 큼 | PC 앱에서 DLL/API 호출 가능 |')
> cmp.append('')
> cmp.append('## 프로젝트 적용 판단')
> cmp.append('')
> cmp.append('- `Codex_PMAS_WPF`의 `MMC_MoveLinearAbsoluteCmd`/`MMC_GroupReadStatusCmd` 테스트는 Maestro Chapter 7의 Group Motion 모델을 그대로 따른다.')
> cmp.append('- `Codex_LASAL_WPF`에서 같은 테스트를 흉내 내려면 SNET 방식이 아니라 LASAL 내부 `_LMCRobotBase`/Motion Network의 group 상태를 TCP 프레임으로 매핑해야 한다.')
> cmp.append('- InPosition 조건, transition mode, blending은 SNET API에서 동일 명령을 찾는 방식으로는 해결되지 않는다. Maestro group status와 LASAL robot/group status 사이의 의미 대응표가 필요하다.')
> cmp.append('- 반대로 trigger/capture/latch/ADC/DAC 계측 기능을 테스트할 때는 SNET 문서가 더 직접적인 기준이다. Maestro에서는 TouchProbe, Recording, PI, EtherCAT IO/event 기반으로 대체 설계를 해야 한다.')
> write(out/'SNET_ECAT_vs_Maestro_API_Comparison_2026-06-23.md','\n'.join(cmp))
>
> print('written')
> for path in sorted(out.glob('*2026-06-23.md')):
>     print(path, path.stat().st_size)
> '@ | python -``
>
> </details>
>
> 문서 생성 중 Python 버전의 `Path.write_text(newline=...)` 인자 지원 문제로 실패했습니다. 파일은 아직 생성되지 않았고, 같은 내용으로 writer만 바꿔서 바로 다시 실행합니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran ``$env:PYTHONIOENCODING='utf-8'; @'
> from pathlib import Path
> import json, re
> from collections import defaultdict
>
> out=Path('docs/api_analysis')
> base=Path('tmp/api_pdf_extract')
> parsed=json.loads((base/'api_parsed.json').read_text(encoding='utf-8'))
> mae_eip=json.loads((base/'maestro_toc_function_entries.json').read_text(encoding='utf-8'))
>
> def md_escape(s):
>     return str(s).replace('|','\\|')
>
> def code_list(names, limit=None):
>     if limit is not None and len(names)>limit:
>         show=names[:limit]
>         return ', '.join(f'`{md_escape(n)}`' for n in show)+f', ... 외 {len(names)-limit}개'
>     return ', '.join(f'`{md_escape(n)}`' for n in names)
>
> def write(path, text):
>     with open(path, 'w', encoding='utf-8', newline='\n') as f:
>         f.write(text.strip()+"\n")
>
> snet_title_map={'6':'로그 정보 남기기','8':'축 파라미터 설정','22':'위치/속도 Override','24':'겐트리 동기 구동','25':'겐트리 원점 검색','31':'입/출력 제어 (SNET-RTEX-IO Slave)','39':'Trigger 출력 (SNET-ECAT) - 특정 위치 트리거'}
> snet_groups=[]
> for cat, entries in parsed['snet']['categories'].items():
>     major=cat.split('.')[0]
>     title=cat
>     if cat.strip().isdigit(): title=f'{major}. {snet_title_map.get(major, cat)}'
>     snet_groups.append((int(major), title, entries))
> snet_groups=sorted(snet_groups, key=lambda x:x[0])
