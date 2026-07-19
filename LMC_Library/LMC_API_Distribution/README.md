# LASAL Motion Control API 배포 패키지

적용 버전: `LasalMotionControlLib 0.9.1-preview`
대상 환경: Windows, .NET Framework 4.8

이 배포 패키지는 다음 세 항목으로만 구성된다.

> **Preview 경고:** 이 패키지는 개발·통합 시험용이며 production 승인본이
> 아니다. current source의 PC 시험과 LASAL 정적 계약은 통과했지만 실제 PLC
> command E2E와 packet 재캡처는 `0/25`다. `Close`, `Dispose`, cancellation은
> motion Stop이 아니다. 장비에서 E-stop, HW/SW limit, UNIT, Home/Reference와
> 이동 범위를 별도로 승인한 뒤 사용한다. DLL은 strong-name/AuthentiCode 서명이 없다.

> **Manual 출판 상태:** 포함된 DOCX/PDF는 문서 버전 `1.0`이다. 이 README의
> preview, `0/25`, safe-stop과 group read 제한이 아직 manual 본문에 모두
> 재출판되지 않았으므로 상충하면 이 README의 제한을 우선한다.

| 번호 | 폴더 | 내용 |
|---:|---|---|
| 1 | `01_API` | PC 응용프로그램에서 참조할 `LasalMotionControlLib.dll` |
| 2 | `02_Example_Program` | DLL 상대경로 참조 WPF 예제 source, solution과 실행 파일 |
| 3 | `03_API_User_Manual` | API 사용법을 정리한 한국어 PDF와 편집 가능한 Word 원본 |

처음 사용하는 경우 [API 사용설명서 PDF](03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf)를
먼저 읽고, [예제프로그램 안내](02_Example_Program/README.md)의 순서로 실행한다.
내용을 수정할 때는 [편집 가능한 Word 원본](03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.docx)을
사용한다.

DLL은 단위를 자동 변환하지 않는다. 호출자가 물리값을 PLC application UNIT으로
변환해 DINT로 전달한다. 연결 종료는 motion Stop이 아니므로 상태 확인과 정지 절차는
사용설명서의 호출 순서를 따른다.

## 전달 폴더 청결성

Visual Studio build 뒤 예제 source 아래에 ignored `bin/`, `obj/`, `.vs/`가 생길 수
있다. working tree 폴더를 그대로 압축하지 않는다. 내부
`Build-LmcApiDistribution.ps1`가 cleanup을 완료한 뒤 `git ls-files`로 확인되는
세 번호 폴더와 이 README만 전달한다. build script가 출력한 DLL/manual SHA-256과
source commit은 distribution 외부의 승인 기록에 보존한다.
