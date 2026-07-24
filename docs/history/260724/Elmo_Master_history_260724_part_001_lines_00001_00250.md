# 히스토리 파일 분할 분석

> 이 파일을 분석해서 이 쓰레드에서 작업을 이어 진행하도록하자. 히스토리 파일이 너무 크니까 날짜폴더 만들어서 니가 읽어들이기 편한 사이즈로 잘라서 나눠서 저장하고 개별 파일을 읽어 들여서 내용 분석해.
>
> User context
>
> Attachments:
> - Elmo_Master_history_260721.md: `./docs/history/Elmo_Master_history_260721.md`

<details><summary>11 previous messages</summary>

> 먼저 기존 히스토리 분할·인계 절차와 현재 파일 상태를 확인하겠습니다. 원본은 보존하고, `docs/history/260721/` 아래에 재결합 가능한 날짜별 조각과 이어서 작업할 요약을 만든 뒤 각 조각을 실제로 읽어 현재 작업 상태를 정리하겠습니다.
>
> 파일은 약 42.0MB, 53,548줄이며 현재 미추적 원본입니다. 기존 `history-thread-handoff` 절차를 적용해 고정 크기 조각·인덱스·재결합 검증·분석 요약을 남기겠습니다. 현재 LASAL `.lcb` 2개 수정은 사용자 작업으로 보고 건드리지 않습니다.
>
> 원본은 한 줄이 매우 큰 데이터가 섞였을 가능성까지 점검 중입니다. 단순 줄 수만 기준으로 자르면 일부 조각이 지나치게 커질 수 있어, 조각 복사본에서만 대형 페이로드를 표시문으로 치환할지 먼저 판단하겠습니다.
>
> 대형 행 39개가 모두 스크린샷/base64·computer-use 상태 출력임을 확인했습니다. 원본은 그대로 두고 해당 행만 해시가 포함된 placeholder로 바꾸며, 나머지 53,509개 행은 250줄 기준으로 나누겠습니다. 원본에 포함된 후행 공백도 읽기용 복사본에서 정규화하고 그 행 번호를 manifest에 기록합니다.
>
> 분할이 끝났습니다. 215개 조각, 원본 53,548줄이며 원본 SHA-256은 분할 전후 동일합니다. 읽기용 재결합도 치환·공백 정규화 기준본과 정확히 일치합니다. 이제 조각 001–072, 073–144, 145–215를 병렬로 전수 분석하고, 저는 최신 Git/소스 상태와 마지막 커밋·미완료 범위를 직접 대조하겠습니다.
>
> 현재 저장소는 히스토리 마지막 시점과 달라졌습니다. `HEAD`는 기록대로 `29b5512`지만 `origin/main`보다 3커밋 앞서 있고, 처음 확인했던 `.lcb` 2개 외에 LASAL `.st/.lcn` 변경 5개가 작업 중 새로 나타났습니다. 제 분할 작업은 `docs/history/260721/`만 생성했으므로 이 LASAL 변경은 건드리지 않고, 현재 diff를 읽기 전용으로 확인해 재개 문서에 별도 표시하겠습니다.
>
> 현재 `HEAD` 기준 Release 테스트 실행 파일은 실제로 `101/101 PASS`입니다. 다만 중앙 상태 문서와 README에는 아직 `100/100`이 남아 있어 문서가 한 단계 뒤처졌습니다. 새로 나타난 LASAL 변경까지 포함한 현재 작업트리에서 PC 테스트와 source/full-network 정적 계약을 다시 실행해, 히스토리의 완료 보고와 지금 상태를 분리하겠습니다.
>
> 전반 001–072와 후반 145–215 분석이 끝났고, 중간 073–144를 마무리 중입니다. 개별 조각을 다시 찾기 쉽도록 세 구간별 `chunk_digest`도 남기고 있습니다. 히스토리의 최종 보고와 달리 현재 full-network 계약은 `LMCEcatInputLatch1`에 독립 `RealTime="1 ms"` task가 추가되어 실패하므로, 이 차이를 최종 재개점의 첫 blocker로 기록합니다.
>
> <details><summary>Created a file</summary>
>
> - Wrote `./docs/history/260721/99_analysis_summary.md` (+217 -0)
>
> </details>
>
> 전 215개 조각 분석이 모두 끝났고 구간별 digest 3개와 통합 재개 문서를 작성했습니다. 이제 인덱스에 모든 조각의 원본 행 범위·링크·주제 힌트를 연결하고, 링크/해시/재결합/공백/Git 상태를 한 번에 최종 검증하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/history/260721/index.md` (+5 -0)
>
> </details>
</details>

분할·전수 분석을 완료했고, 이 쓰레드에서 바로 이어갈 재개점까지 정리했습니다.

- [분할 인덱스](/C:/work/Elmo/Elmo_Master/docs/history/260721/index.md)
- [통합 분석·재개 문서](/C:/work/Elmo/Elmo_Master/docs/history/260721/99_analysis_summary.md)
- [무결성 manifest](/C:/work/Elmo/Elmo_Master/docs/history/260721/split_manifest.json)
- [Parts 001–072 digest](/C:/work/Elmo/Elmo_Master/docs/history/260721/01_chunk_digest_parts_001_072.md)
- [Parts 073–144 digest](/C:/work/Elmo/Elmo_Master/docs/history/260721/02_chunk_digest_parts_073_144.md)
- [Parts 145–215 digest](/C:/work/Elmo/Elmo_Master/docs/history/260721/03_chunk_digest_parts_145_215.md)

분할 결과:

- 원본 42,025,939바이트, 53,548줄
- 약 250줄씩 215개 조각
- 대형 image/tool-state 행 39개는 분할본에서만 해시 placeholder 처리
- 원본 SHA-256 불변
- 분할본 재결합 검증 통과
- 215개 파일 모두 읽고 part별 주제 힌트 작성

현재 실제 재개점은 히스토리 마지막 보고와 다릅니다.

- `HEAD`: `29b5512`
- `origin/main`보다 3커밋 앞섬
- PC API: `101/101 PASS`
- LASAL source-only 계약: PASS
- LASAL full-network 계약: **FAIL**
- 원인: `LMCEcatInputLatch1`에 독립 `RealTime="1 ms"` task가 추가됨
- 설계상 이 객체는 별도 task 없이 `_LMCAxis1.LMCPreRtWorkTrigger`로만 실행돼야 함

따라서 다음 작업은 LASAL IDE에서 독립 task 설정을 제거하고 trigger 연결을 유지한 뒤 Rebuild/Link와 full-network 계약을 재검증하는 것입니다. 이후 D4 Double Buffer, D5 PLC PI/SDO 실행부, D6 static facade와 실제 PLC 시험으로 넘어가면 됩니다.

기존 LASAL 변경 7개는 보존했고, 이번 작업에서는 `docs/history/260721/`만 생성했습니다. 커밋은 하지 않았습니다.

> EtherCAT / PI에서 나머지는 정상적으로 다 동작됐는데 Read EtherCAT Health 동작시키면 다음과 같이 익셉션 발생된다. 이거 코드를 디버깅 모드로 동작시켜야 어디서 뻣는지 알 수 있을꺼 같아. 그리고 Recorder는 솔직히 어떻게 써야되는지 모르겠어. Lasal에 Data Analize를 이용하는거야? 아님 니가 직접 레코딩 하는거야?
> 그리고 SDO / Write Policy 탭은 구현하다 말은건가? 전반적으로 점검해줘
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-5d544852-e30c-47a8-af06-453191abc14a.png`
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-15cf8786-e867-4148-aaf0-3a6e9f48546a.png`

<details><summary>4 previous messages</summary>

> 스크린샷의 예외만으로도 Health 실패 지점은 거의 특정됩니다. `HealthSlaveRow.Online`이 읽기 전용인데 DataGrid 바인딩이 기본 TwoWay로 생성되어 UI 행을 표시하는 순간 중단된 형태입니다. 실제 XAML/C#을 대조하고, Recorder가 자체 PLC 레코더인지 LASAL Data Analyzer 연동인지, SDO/Write Policy가 어디까지 활성인지 세 갈래로 나눠 현재 소스와 PLC capability를 점검하겠습니다. 코드 수정은 우선 하지 않고 원인·구현 경계·수정 범위를 확정하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> Health 예외는 디버거 화면에서 원인이 확정됐습니다. PLC 응답이 아니라 WPF `Online` 체크박스 열의 양방향 바인딩이 읽기 전용 속성과 충돌한 것입니다. 지금은 Recorder의 실제 데이터 경로와 SDO/Write의 PC·PLC 구현 경계를 소스/테스트 기준으로 끝까지 대조하고 있습니다.
>
> Recorder도 경계가 확인됐습니다. LASAL Data Analyzer를 호출하는 기능이 아니라, PLC의 자체 고정 메모리 뱅크에 PDO 스냅샷을 저장하고 PC 앱이 내려받아 그래프/CSV를 만드는 별도 구현입니다. 다만 현재 LASAL Network 저장 상태에 Recorder 샘플링을 두 번 호출할 수 있는 스케줄 설정이 보여서, 이것은 실제 기록 신뢰도와 관련된 별도 우선 점검 항목입니다.
</details>

결론부터 말하면 세 기능의 상태가 서로 다릅니다.

- `Read EtherCAT Health`: PLC 문제가 아니라 WPF 바인딩 버그입니다.
- `Recorder`: LASAL Data Analyzer가 아니라 우리가 구현한 PLC Recorder입니다. 다만 사용성이 나쁘고 현재 Network 설정에 위험 요소가 있습니다.
- `SDO / Write Policy`: PC API와 화면만 구현됐고 PLC 실행부는 아직 미구현입니다. 실제 기능 기준으로는 미완성입니다.

### 1. EtherCAT Health 예외

디버그 화면만으로 원인이 확정됩니다.

[MainWindow.xaml:648](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml:648)의 체크박스가 기본 `TwoWay`로 바인딩되지만, 대상인 [HealthSlaveRow.Online](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs:3176)은 `get; private set;`입니다.

최소 수정은 다음 한 줄입니다.

```xml
Binding="{Binding Online, Mode=OneWay}"
```

`Grid.IsReadOnly="True"`는 사용자 편집만 막으며 기본 `TwoWay` 바인딩을 바꾸지 않습니다.

Health 응답과 파싱은 이미 성공했을 가능성이 매우 높습니다. [ItemsSource 지정](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs:133) 후 WPF가 행을 그리면서 죽었기 때문입니다. 바인딩 엔진의 Dispatcher 단계에서 발생해서 일반 작업 예외 처리에도 잡히지 않은 것입니다.

Debug 빌드는 정상 통과했습니다. 이 결함은 XAML 컴파일 시에는 검출되지 않아 WPF 화면 smoke test가 추가로 필요합니다.

### 2. Recorder는 어떻게 사용하는가

LASAL Data Analyzer를 호출하지 않습니다. 데이터 흐름은 다음과 같습니다.

`EtherCAT PDO → LMCEcatInputLatch → LMCRecorderStore PLC 메모리 → TCP chunk 다운로드 → WPF plot/CSV`

- PDO snapshot 기록 호출: [LMCEcatInputLatch.st:418](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st:418)
- PLC 고정 bank 1,280,000 bytes: [LMCRecorderStore.st:183](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCRecorderStore/LMCRecorderStore.st:183)
- PC chunk 다운로드: [LmcRecorderDownload.cs:27](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/src/LmcRecorderDownload.cs:27)

가장 단순한 첫 시험은 다음 순서입니다.

1. `Refresh Capabilities`
2. EtherCAT / PI 탭에서 `Load PI Catalog`
3. 기록할 신호의 `Use` 체크
4. Recorder 탭에서:
   - Sample period: `1`
   - Capacity: `1000`
   - Buffer: `Single`
   - Trigger: `Manual`
5. `Configure`
6. `Start`
7. 약 1초 후 `Refresh Status`
8. `State=Ready`, `StopReason=SampleCountComplete`, `Samples=1000` 확인
9. `Download`
10. plot 확인 또는 `Export CSV`
11. 마지막에 `Release`

현재 Base Cycle이 1,000µs라 정상 스케줄에서는 `Period=1`, `Capacity=1000`이 약 1초입니다. 전체 설명은 [README:49](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/README.md:49)에 있습니다.

이벤트 전후를 기록하려면 `Ring + Edge/Window/Mask`를 사용합니다. Pre/Post 값을 설정하고 Start한 후 조건 발생 또는 `Trigger Now`를 사용합니다. 반드시 `Refresh Status`에서 최종 `Ready`, `StopReason`, `TriggerIndex`를 확인해야 합니다.

LASAL Data Analyzer는 Recorder 동작에 필수는 아닙니다. 다만 첫 실기 검증 때 `LMCEcatInputLatch1.PublishSequence`, `LMCRecorderStore1.SampleCount`, `StateValue`를 함께 trace해서 1ms당 한 번만 기록되는지 교차 확인하는 용도로 쓰는 것이 좋습니다.

### 3. 현재 Recorder의 중요한 LASAL Network 문제

현재 Network 파일에는 `LMCEcatInputLatch1`이 독립 `RealTime="1 ms"` task를 갖고 있습니다.

- 독립 RT 설정: [Motion_Network.lcn:5332](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Motion_Network/Motion_Network.lcn:5332)
- 동시에 Pre-RT trigger 연결도 존재: [Motion_Network.lcn:5512](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Motion_Network/Motion_Network.lcn:5512)

현재 검증 결과는 다음과 같습니다.

```text
PC tests                         101/101 PASS
WPF Debug build                 PASS
LASAL source-only contract      PASS
LASAL full-network contract     FAIL
LMCEcatInputLatch1 must not own an independent scheduled task.
```

이 상태에서는 latch가 독립 task와 Pre-RT trigger 양쪽에서 실행될 가능성이 있어 Recorder 중복 sample, cycle/timestamp 순서 이탈 위험이 있습니다. PI Read는 최신값만 보므로 정상처럼 보일 수 있습니다.

Recorder 실기 시험 전 LASAL IDE에서 `LMCEcatInputLatch1`의 독립 RealTime assignment를 제거하고, `_LMCAxis1.LMCPreRtWorkTrigger` 연결만 유지해야 합니다.

### 4. SDO / Write Policy 상태

스크린샷의 값이 현재 구현 상태를 정확히 보여줍니다.

```text
Bits=0x0000003F
MaxSDO=0 bytes
```

`0x3F`에는 Health, Catalog, PI Read, Bulk, Recorder Single Bank, Recorder Trigger만 포함됩니다. PI Write, SDO Read/Write, Extended SDO result bit는 모두 꺼져 있습니다. PLC 광고 코드는 [TCPMotionInterface.st:1039](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:1039)와 [TCPMotionInterface.st:1050](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:1050)에 있습니다.

구현 경계는 다음과 같습니다.

- C# API, frame, parser, ticket/status 모델: 구현됨
- WPF Submit/Refresh/Download/Cancel 화면: 구현됨
- SDK PI/SDO Write allowlist: 빈 배열, 전부 차단
- PLC PI Write `0x7E21`: 요청 길이만 검사하고 `UnsupportedFeature`
- PLC SDO submit/status/cancel: 실행부 없음
- PLC extended result `0x7E51`: 예약 골격만 존재
- 현재 Catalog도 `WritableByPolicy` 항목이 하나도 없음

근거는 [LmcDiagnosticsD5Models.cs:5](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5Models.cs:5), [LMCDiagnosticsService.st:678](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:678), [LMCDiagnosticsService.st:990](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:990)입니다.

따라서 “구현하다 만 것인가?”에 대한 답은:

> 실제 PLC 기능 기준으로는 맞습니다. PC 계약과 UI를 먼저 만들어 놓고 PLC ticket queue, SDO adapter, write allowlist를 구현하지 않은 상태입니다. 현재는 의도적으로 fail-closed되어 있습니다.

입력 필드는 편집 가능하지만 Submit 버튼만 비활성화되므로 사용자가 오해하기 쉽습니다. 탭 전체에 `현재 PLC build에서는 사용할 수 없음` 표시가 필요합니다.

### 종합 우선순위

1. Health `Online` 바인딩을 `OneWay`로 수정하고 WPF smoke test 추가
2. LASAL IDE에서 `LMCEcatInputLatch1` 독립 RT task 제거 후 Full Network 계약 재검증
3. `Single + Manual` Recorder 실기 시험 및 Data Analyzer 교차 trace
4. Recorder 단계 안내와 자동 Status polling 개선
5. 오래된 문서의 `0x1F/D4 Unsupported` 설명을 현재 `0x3F` 상태로 갱신
6. D5는 SDO Read 4/8/12-byte부터 별도 단계로 구현

이번 점검에서는 파일을 수정하지 않았고 기존 LASAL IDE 변경사항도 건드리지 않았습니다.

> 최소 수정은 다음 한 줄입니다.
> Binding="{Binding Online, Mode=OneWay}"
> Grid.IsReadOnly="True"는 사용자 편집만 막으며 기본 TwoWay 바인딩을 바꾸지 않습니다.
> Health 응답과 파싱은 이미 성공했을 가능성이 매우 높습니다. [ItemsSource 지정 (line 133)](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs:133) 후 WPF가 행을 그리면서 죽었기 때문입니다. 바인딩 엔진의 Dispatcher 단계에서 발생해서 일반 작업 예외 처리에도 잡히지 않은 것입니다.
> Debug 빌드는 정상 통과했습니다. 이 결함은 XAML 컴파일 시에는 검출되지 않아 WPF 화면 smoke test가 추가로 필요합니다.
>
> 이거 수정해줘

<details><summary>3 previous messages</summary>

> `Online` 체크박스 바인딩만 `OneWay`로 수정하고, Debug 빌드와 기존 101개 테스트로 회귀 여부를 확인하겠습니다. 기존 LASAL 작업 트리 변경은 건드리지 않습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+1 -1)
>
> </details>
>
> 수정은 적용됐고 Debug 빌드와 기존 PC 테스트 101개가 모두 통과했습니다. 마지막으로 변경 범위가 XAML 한 줄뿐인지 확인하고 마무리하겠습니다.
</details>

수정 완료했습니다.

- 변경: [MainWindow.xaml:648](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml:648)
- `Online` 바인딩을 `Mode=OneWay`로 변경
- Debug 빌드 성공
- PC 테스트 `101/101 PASS`
- `git diff --check` 통과
- 기존 LASAL 변경사항은 건드리지 않았습니다.

이제 PLC에 연결해 `Read EtherCAT Health`를 다시 누르면 실제 화면 동작까지 확인할 수 있습니다.

> 레코드 하다가 Throw났어. 미구현중인건가?
>
