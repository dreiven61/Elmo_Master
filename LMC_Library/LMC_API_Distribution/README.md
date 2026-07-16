# LASAL Motion Control API 배포 패키지

적용 버전: `LasalMotionControlLib 0.9.1-preview`
대상 환경: Windows, .NET Framework 4.8

이 배포 패키지는 다음 세 항목으로만 구성된다.

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
