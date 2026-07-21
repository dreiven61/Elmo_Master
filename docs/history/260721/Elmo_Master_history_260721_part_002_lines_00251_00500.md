
## 현재 PDO의 제한

현재 각 Elmo 축에서 실제 활성화된 PDO는 다음 6개뿐입니다.

입력:

- Actual Position
- Digital Inputs
- Status Word

출력:

- Target Position
- Digital Outputs
- Control Word

근거: [Elmo_1.st](</C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/Elmo_1/Elmo_1.st:159>)

속도, 토크, 전류, DC Link, Following Error 등은 정의는 있지만 현재 PDO에서 비활성화되어 있습니다. 따라서 Recorder를 만들어도 이 값들이 자동으로 기록되지는 않습니다.

필요한 신호는 다음 중 하나가 필요합니다.

- PDO map에 추가
- 느린 진단값이면 비동기 SDO로 조회
- EtherCAT 주기보다 빠른 드라이브 내부 신호이면 Elmo 자체 Recorder 사용

## 권장 구조

```text
EtherCAT RT cycle
  ├─ EtherCAT Health
  ├─ Signal Catalog / PI
  ├─ 동일 cycle의 Bulk Snapshot
  └─ RT Recorder → 고정 Ring/Double Buffer
                         ↓
                  Non-RT TCP chunk upload
                         ↓
                   WPF Plot / CSV Export
```

핵심 원칙은 다음과 같습니다.

- RT Recorder는 PDO 갱신 직후 전용 RT 경로에서 샘플링
- 동적 메모리, 문자열, TCP 송신, 파일 저장, SDO를 RT 경로에 넣지 않음
- Signal ID, 타입, 단위, scale, PDO index/subindex, read/write 권한을 정적 Catalog로 관리
- Bulk 결과에 `cycleCounter`, timestamp, map revision, entry별 상태 포함
- PI Write는 기본 비활성화하고 allowlist 적용
- `ControlWord`와 `TargetPosition` 직접 Write는 기존 DS402 모션 경로와 충돌하므로 기본 금지
- SDO는 요청 ticket을 반환하고 PLC 주기에서 비동기로 처리

현재 TCP request payload는 1320 bytes, send staging buffer는 2048 bytes입니다. Recorder 데이터는 한 응답에 담을 수 없으므로 `recordId + bufferId + offset + count + sequence` 기반 chunk upload가 필수입니다.

근거: [TCPMotionInterface.st](</C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:113>)

## 시간 해상도와 메모리

현재 EtherCAT bus와 축 RT 주기는 1 ms입니다. 따라서 LASAL Recorder의 현실적인 최고 해상도도 1 ms입니다.

Elmo와 비슷하게 구성하면:

```text
32채널 × 4 bytes × 31,250 samples
= 4,000,000 bytes
≈ 3.81 MiB
```

CP313은 공식 사양상 dual-core와 256 MB RAM을 가지고 있어 이 정도 정적 버퍼는 구조적으로 가능해 보입니다. 다만 실제 PLC free RAM과 RT jitter를 측정한 뒤 상한을 확정해야 합니다. [CP313 공식 사양](https://www.sigmatek-automation.com/en/products/control-systems/cpu-units/cp-313/)

## SIGMATEK 기존 기능 활용

SIGMATEK 자체도 LASAL Data Analyzer와 PLC Trace View를 제공합니다. LVD DataAnalyzer는 빠른 채널 기록, 채널별 주기·시간, 트리거, 커서, 장기 기록 기능을 제공합니다. 따라서 개발·시운전 진단은 이것을 먼저 사용하는 것이 가장 빠릅니다. [LASAL 공식 소개](https://www.sigmatek-automation.com/en/products/engineering-tool-lasal/), [LVD DataAnalyzer 공식 자료](https://www.sigmatek-automation.com/fileadmin/user_upload/downloads/LVD_DataAnalyzer_infosheet_en.pdf)

다만 이것은:

- 현재 WPF API와 직접 호환되지 않음
- Elmo PI/Bulk API 대체물이 아님
- 배포 라이선스와 PLC 런타임 포함 여부를 SIGMATEK에 확인해야 함

## 권장 구현 순서

1. **EtherCAT Health + Read-only PI Catalog**
   - Master/Slave 상태, AL code, 누락 frame, frame time
   - 현재 활성 PDO 목록과 typed value

2. **Bulk Snapshot**
   - 동일 cycle의 shadow snapshot
   - 물리축 1~4와 software축 5~9 명확히 구분

3. **Recorder 1차**
   - 고정 채널
   - manual/no-trigger
   - 단일 정적 버퍼
   - status/header/chunk upload

4. **Recorder 2차**
   - pre-trigger
   - edge/window/mask trigger
   - ring/double buffer
   - PC 그래프 및 CSV export

5. **제한적 PI/SDO Write**
   - 타입·범위·축 상태·소유권 검사 후 허용

## 최종 판단

**LASAL에서 Elmo PI/Bulk/Recorder에 대응하는 기능을 만드는 것은 충분히 가능합니다.**

다만 정확한 경계는 다음과 같습니다.

- PDO와 PLC 내부 신호의 1 ms 진단·기록: 가능
- EtherCAT 상태·누락 frame 진단: 가능
- Elmo와 비슷한 PI/Bulk/Recorder PC API: 신규 개발로 가능
- 드라이브 내부 servo-loop 고속 신호: LASAL만으로 완전 대체 불가
- Raw EtherCAT datagram/Wireshark급 캡처: 현재 공개 API만으로 확인되지 않으므로 외부 TAP/pcap 또는 SIGMATEK vendor hook이 필요

이번 요청은 가능성 검토만 수행했으며 파일은 수정하지 않았습니다.

> 작성된 api 설명서를 검토중인데, 이거 지금 그냥 api랑 비동기 api로 나눠놨네? 이거 api들이 내부적으로 어떤식으로 동작하는지 구현된 내용에 대해 설명하는 내부 코드 설명서를 만들어줘. md파일이랑 html이나 pdf로 만들면돼.
>
> User context
>
> Attachments:
> - LASAL_Motion_Control_API_User_Manual_KO.docx: `./LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.docx`

<details><summary>23 previous messages</summary>

> `doc` 스킬을 사용해 기존 DOCX의 구성과 용어를 먼저 확인한 뒤, 실제 C#·LASAL 구현을 교차 추적해서 별도의 내부 코드 설명서를 만들겠습니다. 결과물은 저장소 문서 체계에 맞춘 Markdown과 단독 열람 가능한 HTML로 제공하겠습니다.
>
> 스킬 파일을 여는 과정에서 Windows 실행 세션 오류가 한 번 발생했습니다. 작업 내용과 무관한 환경 오류라 동일한 읽기 작업을 다시 시도합니다.
>
> 이제 문서 목차 검토와 실제 구현 추적을 병렬로 진행합니다. 한쪽은 DOCX가 왜 단순히 동기/비동기로만 보이는지 확인하고, 다른 쪽은 PC 라이브러리와 PLC 내부 실행 경로를 각각 추적합니다.
>
> 소스 추출 한 건이 PowerShell 출력 처리에서 예상보다 오래 걸렸습니다. 작업은 계속 진행 중이며, 해당 출력은 중단 여부를 확인한 뒤 더 작은 범위로 나눠 읽겠습니다. 문서 작성 자체는 병렬 분석 덕분에 계속 진행되고 있습니다.
>
> 확인해 보니 저장소에 `API_DEVELOPMENT_GUIDE.md`가 이미 있지만, 현재 사용자 매뉴얼에서 바로 이어 읽을 수 있는 수준의 “호출 1건이 PC 코드 → TCP frame → PLC queue → MotionLib로 어떻게 흘러가는가” 설명은 부족합니다. 기존 가이드는 유지하고, 중복 목록이 아닌 실행 흐름 중심의 보완 설명서로 만들겠습니다.
>
> 문서 방향을 확정했습니다. 기존 `API_DEVELOPMENT_GUIDE.md`를 폐기하거나 별도 문서를 난립시키지 않고, 이 파일을 실제 구현 설명서로 확장합니다. 핵심은 동기/비동기 메서드 나열이 아니라 세션, 직렬 TCP exchange, PLC 8-slot queue, CyWork dispatcher, MotionLib/RT 경계, ACK와 완료 판정을 한 흐름으로 연결하는 것입니다.
>
> <details><summary>Created a file</summary>
>
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+864 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 초안 패치가 적용 중입니다. 완료되는 즉시 문서 내용과 링크를 검증하고, 배포용 Markdown·HTML을 생성하겠습니다.
>
> Windows 샌드박스의 로그인 세션 오류가 한 번 발생했습니다. 파일 자체 문제는 아니며, 동일 검사를 비로그인 셸로 재시도합니다.
>
> 핵심 내용은 초안에 반영됐습니다. 현재 문서는 “동기/비동기 목록”이 아니라 호출 경로, TCP 프레임, LASAL 큐/dispatcher, 실제 MotionLib 호출, ACK/완료 판정까지 이어지는 내부 실행 모델로 재구성되어 있습니다. 이제 배포용 HTML을 자급형 파일로 만들고 있습니다.
>
> <details><summary>Created 2 files</summary>
>
> - Wrote `./tmp/docs/lmc_internal_guide.css` (+163 -0)
> - Wrote `./tmp/docs/lmc_internal_guide_header.html` (+20 -0)
>
> </details>
>
> HTML 생성은 완료됐습니다. 지금은 최종 산출물 위치가 원본 Markdown과 다르기 때문에, 상대 링크가 엉뚱한 경로를 가리키지 않는지 별도로 확인하고 있습니다. 이 부분까지 맞춰야 실제 내부 설명서로 쓸 수 있습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./output/doc/LASAL_Motion_Control_API_Internal_Code_Guide_KO.md` (+3 -4)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+0 -1)
> - Wrote `./output/doc/LASAL_Motion_Control_API_Internal_Code_Guide_KO.md` (+0 -0)
>
> </details>
>
> 최종 산출물의 링크·목차 앵커·표·Mermaid fallback을 검사했고 모두 통과했습니다. `git diff --check`도 정리했습니다. 지금 마지막으로 제3자 코드 대조 리뷰를 받아, 기술적으로 과장되거나 빠진 표현이 없는지 확인 중입니다.
>
> 코드 대조 리뷰에서 실제 수정이 필요한 표현이 나왔습니다. 특히 cancellation 경계가 “gate 획득 전/후”로 너무 단순화돼 있었고, `0x202B` 검증 범위와 `-5` 오류 설명도 소스보다 강하게 적혀 있었습니다. 이 네 군데를 실제 코드 기준으로 바로잡겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+19 -11)
> - Wrote `./output/doc/LASAL_Motion_Control_API_Internal_Code_Guide_KO.md` (+19 -11)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+5 -1)
> - Wrote `./output/doc/LASAL_Motion_Control_API_Internal_Code_Guide_KO.md` (+5 -1)
> - Wrote `./tmp/docs/lmc_internal_guide_header.html` (+6 -0)
>
> </details>
>
> HTML 본문 생성은 끝났지만 headless Edge가 외부 Mermaid 모듈 로딩을 기다리며 멈췄습니다. 렌더러를 종료하고, HTML을 완전 오프라인에서도 즉시 열리도록 외부 스크립트 의존성을 제거하겠습니다.
>
> <details><summary>Created 6 files, edited 4 files</summary>
>
> - Wrote `./tmp/docs/lmc_internal_diagram_1.dot` (+31 -0)
> - Wrote `./tmp/docs/lmc_internal_diagram_2.dot` (+17 -0)
> - Wrote `./tmp/docs/lmc_internal_diagram_3.dot` (+14 -0)
> - Wrote `./tmp/docs/lmc_internal_diagram_4.dot` (+22 -0)
> - Wrote `./tmp/docs/lmc_internal_diagram_5.dot` (+18 -0)
> - Wrote `./tmp/docs/lmc_internal_guide.css` (+12 -1)
> - Wrote `./tmp/docs/lmc_internal_guide_header.html` (+7 -23)
> - Wrote `./tmp/docs/mermaid_to_svg.lua` (+17 -0)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+1 -1)
> - Wrote `./output/doc/LASAL_Motion_Control_API_Internal_Code_Guide_KO.md` (+1 -1)
>
> </details>
>
> 외부 Mermaid 의존성은 제거했습니다. 다이어그램 5개를 로컬 Graphviz로 SVG로 렌더링해 HTML 안에 직접 포함하므로, 인터넷 없이도 제목·목차·도식까지 그대로 열립니다. 코드 대조 리뷰의 마지막 callback 복사 범위 표현도 수정했습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./tmp/docs/lmc_internal_diagram_1.dot` (+4 -1)
> - Wrote `./tmp/docs/lmc_internal_diagram_1.dot` (+0 -3)
>
> </details>
>
> 레이아웃까지 확인했습니다. 제목이 목차보다 앞에 나오고, 실행 구조·cancellation 도식이 확대 없이 읽히도록 수정했습니다. 마지막 검사는 원본/배포본 동등성, 링크·앵커, UTF-8, 오프라인 SVG 5개, whitespace와 현재 Git 변경 범위입니다.
>
> 정적 계약 자체는 `PASS`였습니다. 다만 검증 래퍼가 PowerShell 스크립트 호출 뒤의 오래된 `$LASTEXITCODE`를 읽어 실패로 오판했습니다. `$?` 기준으로 다시 확인하고 최종 상태만 남기겠습니다.
</details>

완료했습니다. 기존 내부 개발 문서를 실제 C#/LASAL 구현 기준의 v2.0 설명서로 전면 보강했습니다.

산출물:

- [내부 코드 설명서 원본 Markdown](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md)
- [배포용 Markdown](/C:/work/Elmo/Elmo_Master/output/doc/LASAL_Motion_Control_API_Internal_Code_Guide_KO.md)
- [오프라인 HTML](/C:/work/Elmo/Elmo_Master/output/doc/LASAL_Motion_Control_API_Internal_Code_Guide_KO.html)

주요 내용:

- sync/async가 동일 command·builder·parser를 사용하며 async는 `Task.Run` wrapper라는 점
- 단일 exchange gate, request ID가 없는 TCP 직렬 처리 구조
- C# API → TCP frame → LASAL depth-8 queue → `CyWork()` → MotionLib 호출 경로
- Connection lifecycle, session generation, reconnect와 cancellation 동작
- 25개 API command ID, payload 크기와 실제 Axis/Group handler 매핑
- ACK와 실제 모션 완료의 차이
- GroupStop, SetKin, callback 등 현재 구현의 제한
- UNIT·safety 책임 경계와 실제 PLC E2E `0/25` 상태

검증 결과:

- 실제 C#/LASAL 소스와 독립 대조 리뷰 완료
- Markdown/HTML 링크 및 앵커 정상
- HTML에 SVG 다이어그램 5개 내장, 외부 CDN 의존성 없음
- `PASS LASAL.StaticContract`
- `git diff --check`, `git diff --cached --check` 통과
- HTML 시각 렌더링 확인 완료

Git 추적 변경은 원본 Markdown 1개입니다. `output/doc`의 배포 산출물은 현재 `.gitignore` 대상이며, 아직 커밋하지 않았습니다.

> **sync/async는 호출자 대기 방식의 차이일 뿐이며, 내부 wire와 PLC 실행 경로는 같다.**
>
