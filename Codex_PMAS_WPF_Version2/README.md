# Elmo PMAS Packet Capture Test - Version2

## 목적

`Codex_PMAS_WPF`를 별도 Version2로 복제하고, 시작 화면을
`LMC_Library/LasalApiWpfTestApp`과 같은 6개 탭 구조로 구성했다.

- Single Axis
- Group Motion
- EtherCAT / PI
- Bulk Snapshot
- Recorder
- SDO / Write Policy

backend는 LASAL SDK가 아니라 Elmo `MMCLibDotNET v3.0.0.7`을 직접 호출한다.
원본 PMAS의 Cycle Test와 전체 API 화면은 `Open Advanced PMAS UI` 버튼으로 열 수 있다.

이 프로그램은 Wireshark를 제어하거나 `.pcapng`를 생성하지 않는다. controller API
traffic을 발생시키고, 각 호출을 실행 로그의 `CAPTURE #`와 연결하는 시험 프로그램이다.

## 빌드와 실행

```powershell
& 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe' `
  'C:\work\Elmo\Elmo_Master\Codex_PMAS_WPF_Version2\PmasApiWpfTestApp.Version2.sln' `
  /t:Rebuild /p:Configuration=Debug /p:Platform='Any CPU'
```

실행 파일:

```text
C:\work\Elmo\Elmo_Master\Codex_PMAS_WPF_Version2\PmasApiWpfTestApp.Version2\bin\Debug\PmasApiWpfTestApp.Version2.exe
```

프로젝트 표시는 Any CPU지만 실제 `PlatformTarget`은 x64다. MMCLib와 native 종속 DLL
때문에 x64를 유지해야 한다.

## 패킷 캡처 순서

1. 실제 장비의 E-stop, software/hardware limit, Home 상태와 작은 이동 범위를 먼저 확인한다.
2. Wireshark에서 controller와 연결된 PC NIC의 캡처를 시작한다.
3. 예시 display filter를 적용한다.

   ```text
   ip.addr == 192.168.99.20 && (tcp.port == 4000 || udp.port == 5000)
   ```

4. Version2에서 controller IP, TCP port, PC local IPv4, callback UDP port를 입력하고 Connect한다.
5. `a01` 같은 실제 PMAS axis name을 Load한 뒤 Read Status/Read Position부터 실행한다. Group은 Load Group 후 반드시 Get Members로 실제 X/Y/Z/U를 검증한다.
6. motion command는 작은 값으로 시험하고, Move Velocity 뒤에는 반드시 Stop 또는 Power Off를 확인한다.
7. 하단 Execution Log를 펼쳐 `CAPTURE #NNNN START/PASS/FAILED`와 API 이름, elapsed를 pcap 시각과 대조한다.
8. `LOCAL (no controller packet)` 로그는 의도적으로 controller packet을 보내지 않은 동작이다.

## 중요한 차이

- 화면과 기능 목적을 LASAL 앱에 맞췄지만 binary packet은 같지 않다.
  LASAL `0x20xx/0x7Exx` DINT protocol과 Elmo MMCLib RPC를 각각 캡처해 비교해야 한다.
- Version2 motion 입력은 MMCLib의 `double` controller/user unit을 그대로 전달한다.
  LASAL 앱의 `engineering value x 10000` 변환을 적용하지 않는다.
- Group Power On/Off는 PMAS에 1:1 group wrapper가 없어 X/Y/Z/U 멤버별 command를 보낸다.
- Get Members는 controller가 반환한 4개 group member name/ref를 검증하고 axis wrapper를 cache한다.
  Group Power On/Off는 각 member command 뒤 `ReadStatus`도 실행하므로 버튼 1회에 8개 호출이 발생한다.
- PMAS에는 LASAL diagnostics capability bit, Bulk lease/status/release, Recorder adopt/identity/CRC,
  SDO ticket/status/cancel 계약이 없다. 해당 화면은 PMAS native API로 재구성하거나
  `LOCAL (no controller packet)`으로 명확히 표시한다.
- Bulk Configure의 `AddEntry`는 내부적으로 PI metadata를 조회하고, 첫 Read Snapshot의
  `Upload`는 bulk buffer configure와 perform을 연속 실행한다.
- PI Read Selected는 순차 read다. same-cycle 비교에는 Bulk Snapshot을 사용한다. MMCLib의
  direct `VAR_TYPE` overload가 없는 PI metadata type은 read/write/bulk를 차단한다.
- Recorder는 internal/PI variable용 `BeginRecordingEx`와 native `uiRv/uiRp` 값을 사용한다.
  `Use Selected PI`는 checked catalog row를 native `uiRv`와 signal bit mask로 변환하는
  local helper이며 raw 입력도 유지한다. Status에서 ready buffer를 확인하고 Header의
  `Rl` 범위 안에서만 Download한다. SDO 결과의 저장 byte는
  typed value를 PC host endian으로 재인코딩한 값이며 raw packet payload가 아니다.
- Connect/Close 시 이전 handle에 묶인 axis/group/PI bulk wrapper를 폐기하므로 다시 Load해야 한다.
- Close와 창 닫기는 Stop이 아니다.

세부 매핑은 [API_MAPPING.md](API_MAPPING.md)를 참고한다.

## 현재 검증 범위

- Version2 원본 복제 Debug/Release build
- 새 PacketCaptureWindow Debug/Release build
- XAML handler 연결과 application startup smoke
- MMCLib public wrapper 시그니처 기반 정적 연결
- 2026-07-21 제공 capture 23개의 native packet 분석
  - PI catalog/read/snapshot, Recorder stop/status/header/download와 `0x1000:0` SDO Read
  - 이 capture는 선택 operation의 native 호출 근거이며 Version2 전체 실기 승인은 아님

실제 축/그룹 motion과 모든 PI/Bulk/Recorder/SDO 조합의 end-to-end 동작, Recorder
ready 성공 flow와 전체 packet 재캡처는 아직 검증하지 않았다. Native capture는 custom
LASAL `0x7Exx` PLC runtime 증거로도 사용하지 않는다.
