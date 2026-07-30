# 0.9.1-preview 내부 빌드 메타데이터

- 기록일: 2026-07-16
- source baseline: `f8f99a299f72c118c9a243d0165368d666d0cd0f`
- branch: `main`
- Assembly/File version: `0.9.1.0`
- Product version: `0.9.1-preview`
- 용도: 내부 감사와 release provenance

이 문서는 2026-07-16 내부 snapshot이며 Distribution manifest가 아니다.
2026-07-29부터 `Build-LmcApiDistribution.ps1`는 package 내부
`RELEASE_MANIFEST.md`를 원자 생성한 뒤 source commit, clean/dirty-preview,
DLL version/3복제 identity와 모든 배포 파일의 상대경로·크기·SHA-256을 즉시
재검증한다. 아래 당시 hash와 검증 결과는 소급 변경하지 않는다.

> **2026-07-23 갱신:** 아래 `0/25` 표기는 이 빌드가 생성된 2026-07-16의
> provenance snapshot이다. 이후 실제 PLC 검증 결과는
> [SIGMATEK Phase 1/2 Live Packet Capture Analysis](../../../docs/architecture/SIGMATEK_PHASE1_PHASE2_LIVE_CAPTURE_ANALYSIS_2026-07-23.md)를
> 우선한다. 이 파일의 hash와 당시 미검증 목록은 소급 변경하지 않는다.

## 산출물 snapshot

| 파일 | Bytes | SHA-256 |
|---|---:|---|
| `LMC_API_Delivery/src/bin/Release/LasalMotionControlLib.dll` | 72,192 | `4603E663A8BA34674BDD68C1DBB293C9FF676F180558EB8BCBE563B3DA878FCE` |
| `LMC_API_Distribution/01_API/LasalMotionControlLib.dll` | 72,192 | `4603E663A8BA34674BDD68C1DBB293C9FF676F180558EB8BCBE563B3DA878FCE` |
| `LMC_API_Distribution/02_Example_Program/Run/LasalMotionControlLib.dll` | 72,192 | `4603E663A8BA34674BDD68C1DBB293C9FF676F180558EB8BCBE563B3DA878FCE` |
| `LMC_API_Distribution/02_Example_Program/Run/LasalMotionControlApiExample.exe` | 132,096 | `F7B0DB343A14F386E652C0A1028E029AA43C2C1D4F12CBBB38D74FC9CED1CEB3` |
| `LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf` | 662,127 | `B2F83357DAA0420099B3CA792C3F7DB0E4BC3DA003451312394BB33559870B0D` |
| `LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.docx` | 49,435 | `426ED81E57CD44D9D492B2757C8EB3A02FEB0EAB1108FBA151C6FE2977B1D3E4` |

세 API DLL은 byte-identical하다.

## 확인 결과

- PC automated runner: 46/46 PASS
- LASAL source-only static contract: PASS
- LASAL full-network static contract: PASS
- `Codex_PMAS_WPF` Debug build: PASS
- `Codex_LASAL_WPF` Debug build: PASS
- development `LasalApiWpfTestApp` Debug build: PASS
- binary-reference Distribution example Debug build: PASS
- `Build-LmcApiDistribution.ps1 -AllowDirty` preview pipeline: PASS
  - source/Distribution/Run DLL byte identity: PASS
  - temporary standalone example Debug/Release build: PASS
  - Distribution forbidden-reference scan and cleanup: PASS
  - external manual shape check: 21 PDF pages, DOCX structure PASS

`-AllowDirty` 실행은 현재 문서 작업 트리에서 pipeline을 재현하기 위한 preview
검증이다. clean release commit 또는 production 승인을 뜻하지 않는다.

## 승인 제한

- LASAL IDE current Rebuild/Link: 미검증
- Find in Implementation와 IDE log smoke: 미검증
- PLC download/current network readback: 미검증
- 실제 command E2E와 packet recapture: 0/25
- DLL strong-name/AuthentiCode: 없음
- 외부 DOCX/PDF: 적용 API는 `0.9.1-preview`지만 문서 버전 `1.0`; 내부 Markdown
  `1.4`의 preview/0-of-25/safe-stop/group-read 보완을 재출판해야 함

따라서 이 hash snapshot은 current PC build의 동일성을 증명할 뿐 production
승인을 증명하지 않는다.
