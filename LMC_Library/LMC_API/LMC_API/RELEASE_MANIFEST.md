# PC API Build Manifest

> **LEGACY SNAPSHOT / 배포 금지:** 이 파일은 2026-07-13의
> `0.9.0-pc-api` 보관 기록이다. 현재 `0.9.1-preview` 배포는
> `../../LMC_API_Distribution`을 사용하며, build script가 별도 schema의
> `RELEASE_MANIFEST.md`를 자동 생성·검증한다. 이 legacy 파일을 복사하지 않는다.

- Build time: 2026-07-13 15:30 KST
- Product version: `0.9.0-pc-api`
- Assembly version: `0.9.0.0`
- Runtime: .NET Framework 4.8
- Test application platform: x64
- Configuration: Release
- PC/TestApp source commit: `ab3236c906f5abf71453cf82566de60dd8462240`
- LASAL source/library commit: `5dfddc35b57f03b2cf9dbc24287a4c0de7658e53`
- Git policy commit: `40fe3920f57ec01d4a19ebea195dc344a72a3f5c`
- Build checkout: `5dfddc35b57f03b2cf9dbc24287a4c0de7658e53`

이 manifest는 PLC 실기 시험에 사용할 PC API와 WPF test app 산출물을
식별하기 위한 것이다. PC/LASAL 정적 계약과 LASAL IDE build는 통과했지만
실제 PLC end-to-end 시험은 아직 수행하지 않았다. 따라서
`0.9.0-pc-api`는 현장 배포 승인 버전이 아니다.

## Artifacts

| File | Bytes | SHA-256 |
|---|---:|---|
| `bin/LasalMotionControlLib.dll` | 69,632 | `735BB9CC4F4453DF439A29342B0D524CADE0424CB374AA9850AF48FE26CEF7C7` |
| `test-app/LasalMotionControlLib.dll` | 69,632 | `735BB9CC4F4453DF439A29342B0D524CADE0424CB374AA9850AF48FE26CEF7C7` |
| `test-app/LasalMotionControlLibTestApp.exe` | 64,000 | `EF63FAACA5D067928DDAF5E03A426C6229C2B11D7812029F80E0ACF113FC40CD` |
| `../../LMC_API_Delivery/bin/LasalMotionControlLib.dll` | 69,632 | `735BB9CC4F4453DF439A29342B0D524CADE0424CB374AA9850AF48FE26CEF7C7` |

세 DLL 복제본의 hash가 다르면 package가 잘못 조립된 것이다.

## Verification

- PC C# runner: `42/42 PASS`
- LASAL source-only static contract: `PASS`
- LASAL strict source/network contract: `PASS`
- WPF Debug rebuild: `PASS`
- WPF Release rebuild: `PASS`
- WPF hidden-launch smoke test: `PASS`
- LASAL IDE Rebuild All: `0 error(s), 0 warning(s)`
- LASAL Find in Implementation smoke: `Power`, `pos`, `velo` 검색 성공
- smoke 이후 `%TEMP%\Lasal2.log`의 새 `CInvalidArgException`: 없음
- 실제 PLC end-to-end command test: `0/23`, 미수행

## Known Environment Warnings

- 프로젝트 compiler는 C78이고 현재 설치 library는 C81이라 시작 시 version
  mismatch warning이 표시된다.
- 현재 설치된 MotionLib `Include/global.h`는 설치 폴더에 없는 Hardware header
  `_DriveMngBase/DriveComL2.h`를 참조한다. 프로젝트 Rebuild는 성공했지만
  SIGMATEK library 설치 세트 자체는 동일 compiler 세대로 복구/재설치해야 한다.
