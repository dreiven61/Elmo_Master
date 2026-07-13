# LASAL 최신 Library 동기화 및 Git 정리 결과

작성일: 2026-07-13

## 결론

이번 LASAL IDE 저장 결과는 단순한 API 수정만이 아니라 설치된 최신 Hardware,
MotionLib 관련 source와 CodeGenerator 결과가 canonical 프로젝트에 다시 동기화된
변경이다. 이 파일들은 `.gitignore`로 숨길 대상이 아니다. 소스와 network/project
등록 파일을 함께 커밋해야 clean clone에서도 같은 프로젝트를 재현할 수 있다.

canonical 대상은 `Lasal_PRG/Elmo_EtherCAT_Test_4Axis`다. `_Edit`과 로컬 복사본은
개발 대상이 아니므로 정확한 폴더 경로만 ignore한다.

## 변경 분류

IDE 저장 직후 실제 내용 차이가 확인된 LASAL 파일은 82개였다.

- 설치된 vendor library 파일과 hash까지 동일: 58개
- 현재 프로젝트 상태로 다시 생성된 table/include/project 파일: 13개
- Elmo API, object/network 등 프로젝트 고유 파일: 11개
- 별도로 status에 나타났지만 내용 차이가 없던 stat/line-ending 항목: 45개

`Class/SafetyRoutingTables/SafetyRoutingTables.st`는 최신 library 동기화로 새로
연결된 실제 source다. `SafetyManager`, HW network, class 등록 정보가 이 class를
참조하므로 이 파일만 ignore하면 프로젝트 등록과 source가 불일치한다. 따라서
추적한다.

`*.lcp`, `*.lcb`, `*.lcn`, `Network/**/*.st`, `ConfigObjects.st`, `Drive/*.st`는
프로젝트 구성과 실제 실행 topology에 필요하다. WTR 프로젝트의 `.gitignore`에
있는 다음 규칙은 Elmo 저장소에 복사하지 않았다.

- `*.lcb`
- `**/Network/*/*.st`
- `**/ConfigObjects.st`
- `**/Drive/*.st`
- 전역 `Internal`, `Language`
- `MaeExp.txt`, `MaeExp.xml`, `MultiMasterExp.mme`

반대로 `ProjectInternal`, LASAL build output, preview/publish/cache/upload output,
로컬 `_Edit`/복사본은 ignore한다. 현재 Git에 추적된 LASAL build cache나
`ProjectInternal` 파일은 0개이므로 `git rm --cached`는 수행하지 않는다.

## IDE 검증

LASAL Class 2 `02.03.001`에서 canonical 프로젝트에 `Rebuild All`을 실행했다.

- 실행 시각: 2026-07-13 14:22 KST
- Linker: `Done`
- 최종 결과: `0 error(s), 0 warning(s)`

변경 class의 Object Network server에서 공식 `Find in Implementation` smoke를
실행했다.

- `Power`: 성공
- `pos`: 성공
- `velo`: 성공
- smoke 시작 이후 `%TEMP%/Lasal2.log`의 새 `CInvalidArgException`: 0건

즉 최신 저장 결과에서 기존 `_Edit` 프로젝트에서 보였던 implementation 검색
오류는 canonical 프로젝트에 재현되지 않았다.

## LASAL 설치 자체의 남은 문제

프로젝트를 처음 열 때 발생하는 아래 오류는 저장소 source 오류가 아니라 현재
PC의 LASAL library 설치 조합 문제다.

```text
MotionLib/Include/global.h(15): Error reading file
Hardware/Class/_DriveMngBase/DriveComL2.h
```

확인 결과 현재 MotionLib의 `global.h`는 `DriveComL2.h`를 include하지만 활성
Hardware library에는 그 파일이 없다. 권한 문제가 아니며, 이전 설치에서 header
하나만 복사해 넣는 방식은 ABI/library 조합을 보장하지 못하므로 금지한다.

또한 현재 프로젝트 compiler는 C78이고 설치된 Hardware, MotionLib, OS Interface,
System, Tools library는 C81이라 시작 시 version warning이 발생한다. 이번 Rebuild는
성공했지만 이 경고가 사라진 것은 아니다.

정식 해결은 같은 LASAL release에 맞는 Hardware와 MotionLib 전체 세트를 함께
설치하거나 LASAL 설치 복구를 수행하는 것이다. 그 뒤 프로젝트 compiler/library
조합을 C81 기준으로 올릴지 별도 승인하고 clean open/Rebuild를 다시 확인한다.

## Git 정책

`.gitignore`는 untracked 파일에만 기본 적용된다. 이미 추적된 library source가
최신 버전으로 바뀐 것은 ignore 규칙으로 사라지지 않으며, 사라져서도 안 된다.

LASAL Class 2가 생성한 source에는 IDE 소유 trailing space가 포함된다. 이를
기계적으로 제거하면 vendor/CodeGenerator 파일을 다시 쓰게 되므로 canonical
LASAL 경로에는 Git whitespace error 판정만 끄고 기존 EOL 정책은 유지한다.
사람이 작성하는 C#/Markdown과 다른 경로는 기존 whitespace 검사를 그대로 받는다.

전체 LASAL diff의 added-line ASCII 검사는 최신 vendor source에 원래 포함된 독일어
문자와 단위 기호를 검출한다. 이 vendor 문자를 일괄 재인코딩하지 않는다. 별도로
확인한 custom `TCPMotionInterface.st`의 추가 non-ASCII line은 0개이고, 새 vendor
`SafetyRoutingTables.st` 전체의 non-ASCII line도 0개다.
