# PC API Build Manifest

- Build time: 2026-07-10 15:11 KST
- Product version: `0.9.0-pc-api`
- Assembly version: `0.9.0.0`
- Runtime: .NET Framework 4.8
- Test application platform: x64
- Configuration: Release
- API source commit: `16e94c8f70fb95611ff0792f79ac34dc046dbc88`
- WPF source commit: `b2da80e376c1501c0d751f1aa2dd2c44b578e1ba`
- Build checkout: `b67a96c2292ff3dda06ca0bc516d29c91813287a`

이 manifest는 PC API 구현 산출물을 식별하기 위한 것이다. LASAL handler,
LASAL IDE build, PLC download와 실기 packet 재캡처가 끝나지 않았으므로
`0.9.0-pc-api`는 현장 배포 승인 버전이 아니다.

## Artifacts

| File | Bytes | SHA-256 |
|---|---:|---|
| `bin/LasalMotionControlLib.dll` | 69,632 | `9324F0BC49C5575E1057F96E3E6C9B8033F387D55A06AA304848A8604379EDC3` |
| `test-app/LasalMotionControlLib.dll` | 69,632 | `9324F0BC49C5575E1057F96E3E6C9B8033F387D55A06AA304848A8604379EDC3` |
| `test-app/LasalMotionControlLibTestApp.exe` | 47,104 | `39E23156BC975B22E85C20C7F425532F34A82D953189121D6696648C731A97CF` |
| `../../LMC_API_Delivery/bin/LasalMotionControlLib.dll` | 69,632 | `9324F0BC49C5575E1057F96E3E6C9B8033F387D55A06AA304848A8604379EDC3` |

기존 `LmcMotionApi.dll`과 `LmcMotionApiTestApp.exe` 이름은 제거했다. 세 DLL
복제본의 hash가 다르면 package가 잘못 조립된 것이다.

## Verification

- PC C# runner: `42/42 PASS`
- LASAL static source contract: `PASS`
- WPF Release rebuild: `PASS`
- 두 `BasicUsage.cs` sample compile: `PASS`

위 검증은 PC serializer/parser/transport와 tracked source 정합성 검증이다.
PLC end-to-end 동작 승인을 의미하지 않는다.
