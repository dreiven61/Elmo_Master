# LASAL Motion Control API 배포 패키지

적용 버전: `LasalMotionControlLib 0.9.1-preview`
대상 환경: Windows, .NET Framework 4.8

이 배포 패키지는 다음 세 번호 폴더와 최상위 검증 manifest로 구성된다.

> **Preview 경고:** 이 패키지는 개발·통합 시험용 preview이며 production 승인본이
> 아니다. current PC 시험과 LASAL source/정적 계약은 current PLC 또는 hardware
> 검증이 아니다. 새 PLC download, safety chain 승인, command capture와 최종 physical
> readback을 완료하기 전에는 production에 사용하지 않는다. 성공 ACK는 command
> 수락일 뿐 완료가 아니므로 typed status와 최종 위치/상태를 확인한다. `Close`,
> `Dispose`, timeout과 cancellation은 motion Stop이 아니다. 장비에서 E-stop,
> HW/SW limit, UNIT, Home/Reference와 이동 범위를 별도로 승인한 뒤 사용한다.
> DLL은 strong-name/AuthentiCode 서명이 없다.

> **Manual 출판 상태:** 포함된 DOCX/PDF는 current source 계약과 맞춘 문서 버전
> `2.3-candidate`다. 이 tracked canonical release-input 승격은 full Distribution
> PASS 또는 production 승인을 뜻하지 않는다.

| 번호 | 폴더 | 내용 |
|---:|---|---|
| 1 | `01_API` | PC 응용프로그램에서 참조할 `LasalMotionControlLib.dll` |
| 2 | `02_Example_Program` | DLL 상대경로 참조 WPF 예제 source, solution과 실행 파일 |
| 3 | `03_API_User_Manual` | API 사용법을 정리한 한국어 PDF와 편집 가능한 Word 원본 |
| 검증 | `RELEASE_MANIFEST.md` | source commit, clean/dirty-preview, DLL version/3복제 identity와 모든 배포 파일의 상대경로·크기·SHA-256 |

처음 사용하는 경우 [API 사용설명서 PDF](03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf)를
먼저 읽고, [예제프로그램 안내](02_Example_Program/README.md)의 순서로 실행한다.
내용을 수정할 때는 [편집 가능한 Word 원본](03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.docx)을
사용한다.

DLL은 단위를 자동 변환하지 않는다. 호출자가 물리값을 PLC application UNIT으로
변환해 DINT로 전달한다. 연결 종료는 motion Stop이 아니므로 상태 확인과 정지 절차는
사용설명서의 호출 순서를 따른다.

## Preview 기능 범위

- 유일하게 source 승인된 SDO Write target은 Axis 1 Gold UI[24], exact
  `Slave 1 / 0x2F00:24 / Int32 / 4 bytes`다. Axis 2..4와 모든 비승인 target은
  차단된다.
- 수동 Write는 같은 current connection/session의 `DiagnosticsBuild`, `BootId`,
  `MapRevision`과 exact target을 고정하고 baseline, pre-write guard, Write,
  guarded readback의 서로 다른 four-ticket same-value qualification을 통과해야 한다.
  identity drift나 disconnect는 proof를 폐기하며 결과가 불명확한 Write를 자동
  재전송하지 않는다.
- Axis 1 UI[24]의 실제 미사용 여부, current PLC capability bit 9, EtherCAT mailbox
  mutation과 physical readback은 아직 검증되지 않았다.
- PI Write, D4 Double Recorder, dynamic capability bits 15..17과 digital output
  command `0x7E23`은 이 preview에서 활성화되지 않는다.

## 전달 폴더 청결성

Visual Studio build 뒤 예제 source 아래에 ignored `bin/`, `obj/`, `.vs/`가 생길 수
있다. working tree 폴더를 그대로 압축하지 않는다. 내부
`Build-LmcApiDistribution.ps1`가 cleanup을 완료한 뒤 같은 폴더에서 원자적으로
`RELEASE_MANIFEST.md`를 생성하고 즉시 현재 파일과 다시 대조한다. 세 번호 폴더,
이 README와 manifest만 전달한다. `dirty-preview` manifest는 미커밋 통합 시험본을
식별할 뿐 production 승인을 뜻하지 않는다. 다음 build의 입력 청결성 판정에서는
정확한 manifest와 script가 사용하는 GUID 형식 `.tmp`/`.bak`만 제외한다. 같은 입력의
두 번째 clean build는 byte-identical manifest를 만들며, 그 밖의 Distribution 변경은
계속 dirty 입력으로 처리한다.
