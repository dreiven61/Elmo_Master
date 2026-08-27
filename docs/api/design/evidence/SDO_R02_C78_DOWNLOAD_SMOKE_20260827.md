# SDO-R02 C78 다운로드/기본 구동 smoke — 2026-08-27

## 1. 판정

- C78/ARM Rebuild/Link: **PASS**
- PLC 다운로드: **PASS (사용자 확인)**
- PLC 기본 정상 구동: **PASS (사용자 확인)**
- SDO-R02 전체 bench qualification: **미완료**
- production activation: **변경 없음 / NO-GO**

이 판정은 빌드, 다운로드 및 기본 실행 smoke까지만 의미한다. manual SDO Read/Write,
programmatic D5 regression, 실제 packet/readback 및 MODE 변경을 통과한 것으로 확대하지 않는다.

## 2. 기준 소스

- implementation commit: `4ff371599d01eae55ce9d246fdbf9e6ec08e8385` (`dev : Edit SdoExecutor`)
- audit head after generated/syntax/evidence updates: `4909200ba45e9e5d4f87334e92f6190599f471e2`
- branch: `codex/sdo-mode-redesign-docs-20260827`

다운로드 시점 working tree에는 LASAL IDE 재생성물과 사용자가 수정한
`LMCDiagnosticsService.st`가 미커밋 상태로 존재했다. 따라서 implementation commit 하나만으로
다운로드 image의 물리 identity를 재현했다고 주장하지 않는다.

## 3. C78 build evidence

사용자가 제공한 rebuild transcript에서 다음을 확인했다.

- `Rebuild project with compiler version C78 (target architecture: ARM)`
- `LMCDiagnosticsService.st` compile 수행
- `LMCSdoExecutor.st` compile 수행
- compiler done 2회
- `Linker: [INFO] Done`
- 최종 결과: `Done - 0 error(s), 101 warning(s).`

원본 transcript:

- byte length: `36,838`
- SHA-256: `E9EA6D44601E68A1319834F10E0357F36C2EABC45B1C7ABB19FFC0850BCFB0E8`

101개 warning에는 기존 source warning과 프로젝트 C78 / library C82 version mismatch warning이
포함된다. error나 linker failure는 없다.

## 4. C78 syntax 수정

다음 두 implementation의 `VAR_OUTPUT` 종료부에서 `END_VAR;` 뒤에 `VAR`가 이어지던 문법을
`END_VAR`로 수정했다.

- `LMCDiagnosticsService::HandleAxisDs402HomeExOutcome`
- `LMCDiagnosticsService::HandleAxisDs402HomeExRetire`

수정 후 두 함수와 전체 프로젝트가 C78 compile/link를 통과했다.

## 5. 집중 정적 확인

- `Verify-LasalSdoExecutorDualEntry.ps1`: **PASS**
- generated `Classes.lcb`에서 `LMCSdoExecutor.st` record 1개 확인
- 해당 record에서 `RequestSource`, `ParaReadWrite`, `ParaType`, `ParaString` 등록 확인

전체 `Verify-LasalContract.ps1 -SourceOnly`는 기존 strict physical-identity gate에서 중단됐다.

```text
LASAL.UdpCallbackContract blocker: SetPosition-augmented Classes.lcb physical identity drifted.
```

이 결과를 SDO source 실패로 분류하지 않으며, 기준 hash를 자동 재설정하지도 않는다.

## 6. 다운로드 후 관찰 artifact

다운로드/기본 구동 확인 뒤 working tree에서 관찰한 값이다. 이 값이 PLC에 다운로드된 image의
checksum과 동일하다는 별도 PLC-side 증거는 아직 없다.

| artifact | bytes | SHA-256 |
|---|---:|---|
| `Class/Classes.lcb` | 8,651,165 | `AA389B28918A4237B918E692180EC2ACB578C4D64E9F2F3A568F689CB2EA7152` |
| `Elmo_EtherCAT_Test_4Axis.lcb` | 634,865 | `5890D44409866ED0F821D20AF04F2024826C7D8431D615AE350408338737CD61` |
| `Network/Networks.lcb` | 243,125 | `10ECCEE8A7B0BEE8F820D7495F07E3FB2C648EA2AB4A122DA7CAADE95B2ECE46` |

`Classes.lcb`는 빌드 직후 관찰 hash와 다운로드 후 관찰 hash가 같은 byte length에서 달랐다.
원인이 IDE의 후속 저장/생성인지 확인되지 않았으므로 strict artifact identity는 미종결이다.

## 7. 남은 SDO-R02 bench gate

- [ ] Axis1..4 executor network direct-open 확인
- [ ] Axis1..4 manual `0x6061:0` Read PASS
- [ ] 승인된 safe object manual Write + exact readback PASS
- [ ] manual/programmatic 동시 진입 BUSY/no-wire arbitration PASS
- [ ] completion 후 반대 entry 재사용 PASS
- [ ] late/source mismatch quarantine PASS
- [ ] programmatic D5 Read/Write regression PASS
- [ ] 위 시험과 동일한 PLC image identity 고정

이 gate가 닫히기 전에는 `LMCSdoExecutor` manual Server 기능을 hardware-qualified 완료로 표시하지 않는다.
