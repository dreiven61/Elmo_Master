# Elmo Master History 260609 Part 02

- Source: `C:\work\Elmo\Elmo_Master\docs\history\Elmo_Master_history_260609.md`
- Lines: 201-400

- C# 라이브러리: `Gold Maestro MMCLibDotNET Libs V3.0.0.7`
- Codesys 패키지: `Elmo_1.0.0.6`
- 문서: `Platinum Maestro 2.2.1.1 Release Note`, `Titanium Maestro Initial Release notes`, `Gold Maestro MMCLibDotNET_Test_App_V3.0.0.7`
근거: [Elmo Resource Center - Maestro Links](https://www.elmomc.com/products/application-studio/download-resource-center/)의 Maestro 항목에 위 내용이 올라와 있습니다. 공개 페이지상 Maestro 펌웨어/라이브러리/문서 목록은 보이지만 API 매뉴얼 PDF는 없습니다.

추가로 제품 페이지 기준:
- Gold Maestro 쪽은 설치 매뉴얼이 `V.1.501_April 2024`로 올라와 있음
- Platinum Maestro 쪽은 설치 가이드가 `V.2.001_April 2024`, Integrated I/O 설치 가이드가 `V.2.002_December 2024`로 올라와 있음
근거: [Gold Maestro](https://www.elmomc.com/product/gold-maestro/), [Platinum Maestro](https://www.elmomc.com/product/platinum-maestro/)

정리하면, **API 함수 정의/구조체 기준 문서로는 지금 가진 `v2.012`를 계속 기준으로 쓰면 됩니다.** 다만 PMAS/TMAS 최신 펌웨어 기준 변경사항은 별도 릴리즈 노트에 있을 가능성이 있으니, 최신 펌웨어 기능까지 반영하려면 `Platinum Maestro 2.2.1.1 Release Note`를 추가로 확보해서 비교해야 합니다. 현재 공개 링크는 locked 다운로드라 실제 PDF 내용은 로그인 없이 바로 열리지 않았습니다.

> 로그인 자료실 링크보내줘봐

<details><summary>1 previous message</summary>

> <details><summary>Searched web 1 time</summary>
>
> <details><summary>Searched web 1 time</summary>
>
> - Searched web for `site:elmomc.my.site.com Elmo service portal login ...`
>
> </details>
>
> </details>
</details>

여기입니다.

- Resource Center 로그인: [https://www.elmomc.com/login/](https://www.elmomc.com/login/)
- 한국어 로그인 페이지: [https://www.elmomc.com/kr/login/](https://www.elmomc.com/kr/login/)
- 현재 Maestro 다운로드 목록 페이지: [https://www.elmomc.com/products/application-studio/download-resource-center/](https://www.elmomc.com/products/application-studio/download-resource-center/)
- Elmo Support-Service Portal: [https://elmomc.my.site.com/login?locale=us](https://elmomc.my.site.com/login?locale=us)

참고로 Resource Center에서 locked 파일 누르면 로그인 페이지로 넘어갑니다. API 매뉴얼 최신본을 찾으려면 로그인 후 Resource Center에서 `Maestro Administrative and Motion API`, `MAN-MAESTRO-API`, `Maestro API`, `v2.012`로 검색하는 게 제일 빠릅니다.

> API 다운로드 항목이 없는데? 잘 찾아서 보내줘

<details><summary>5 previous messages</summary>

> 맞습니다. 내가 앞서 말한 Resource Center 현재 Maestro 목록에는 API 매뉴얼 항목이 안 보입니다. 그래서 지금은 “현재 목록” 말고 숨겨진/구버전 접힌 영역, 제품 페이지, 사이트 검색 인덱스까지 다시 뒤져서 실제 다운로드 가능한 위치가 있는지 확인하겠습니다.
>
> 현재 Resource Center HTML 전체를 긁어봐도 `Maestro Administrative`, `MAN-MAESTRO-API`, `v2.012`, `2022_12` 문자열은 0건입니다. 즉 네 말대로 공개 다운로드 목록에는 API 매뉴얼이 없습니다. 그래도 WordPress 검색/첨부 인덱스 쪽에 숨어 있는지 더 확인하겠습니다.
>
> 제품 페이지도 확인했습니다. Gold/Platinum/Titanium Maestro 제품 페이지에는 설치 매뉴얼/가이드만 있고 API 매뉴얼 링크는 없습니다. 이제 Download Monitor 쪽 공개 API/검색 라우트를 확인해서 다운로드 ID로 숨어 있는 항목이 있는지 보겠습니다.
>
> <details><summary>Searched web 3 times</summary>
>
> <details><summary>Searched web 3 times</summary>
>
> - Searched web for `site:elmomc.my.site.com "Maestro Administrative" ...`
> - Searched web for `Elmo Motion Control service portal documentation download API manual`
> - Searched web for `https://www.elmomc.com/capabilities/motion-control/host-programming-environment/g-mas-api-for-net/`
>
> </details>
>
> </details>
>
> 공개 검색과 공식 페이지 기준으로 확인한 결과를 정리하면, “API 매뉴얼 다운로드 링크”는 현재 공식 사이트에서 노출되지 않습니다. 대신 API 설명 페이지는 따로 있고, 실제 자료는 Service Portal/문의 경로로 돌린 상태로 보입니다.
</details>

찾아봤는데, 결론부터 말하면 **공식 사이트에서 `Maestro Administrative and Motion API` PDF 다운로드 항목은 현재 안 보입니다.** 내가 전에 Resource Center라고 한 건 부정확했습니다.

확인한 공식 링크는 이겁니다.

- G-MAS API 설명 페이지: [G-MAS API for .NET](https://www.elmomc.com/capabilities/motion-control/host-programming-environment/g-mas-api-for-net/)
- Technical Resource Center: [Download Resource Center](https://www.elmomc.com/products/application-studio/download-resource-center/)
- Service Portal 로그인: [Elmo Service Portal](https://elmomc.my.site.com/login?locale=us)
- 일반 로그인: [Elmo Login](https://www.elmomc.com/login/)

공식 Resource Center에서 실제로 보이는 Maestro 관련 항목은 API 매뉴얼이 아니라 아래 쪽입니다.

- PMAS firmware `v2.2.1.1`: [download/22423](https://www.elmomc.com/download/22423/?tmstv=1779769936)
- TMAS firmware `v4.2.1.1`: [download/22456](https://www.elmomc.com/download/22456/?tmstv=1779769936)
- `Gold Maestro MMCLibDotNET Libs V3.0.0.7`: [download/22446](https://www.elmomc.com/download/22446/?tmstv=1779769936)
- `Platinum Maestro 2.2.1.1 Release Note`: [download/22425](https://www.elmomc.com/download/22425/?tmstv=1779769936)
- `Gold Maestro MMCLibDotNET_Test_App_V3.0.0.7`: [download/22439](https://www.elmomc.com/download/22439/?tmstv=1779769936)

내가 공식 사이트에서 직접 확인한 검색어:
- `Maestro Administrative`
- `Maestro Administrative Motion API`
- `MAN-MAESTRO-API`
- `v2.012`
- `2022_12`

결과:
- Resource Center HTML 내 API 매뉴얼 문자열: **0건**
- 제품 페이지 Gold/Platinum/Titanium Maestro 내 API 매뉴얼 링크: **0건**
- WordPress 공개 검색/REST 검색: API PDF 다운로드 항목 없음
- media/download API 쪽은 인증 필요 또는 차단됨

따라서 지금 정확한 판단은 이겁니다.

**`Maestro Administrative and Motion API_2022_12_v2.012.pdf`는 공개 다운로드 목록에는 현재 없는 문서입니다.**
새 버전이 공개로 올라온 것도 못 찾았습니다. 최신본 확인은 Service Portal 로그인 후 검색하거나 Elmo/대리점에 직접 요청해야 합니다.

Service Portal에서 검색할 키워드는 이 순서로 넣으세요.

```text
Maestro Administrative and Motion API
MAN-MAESTRO-API
Maestro API
Administrative Motion API
v2.012
```

공개 사이트 기준으로는 네가 가진 `v2.012`가 우리가 확인 가능한 최신 API 문서입니다.

> 이 파일들을 분석해서 시그마텍에 블렌딩이 어떤기술들이 있는지 설명해
>
> User context
>
> Attachments:
> - Rounding Mode (blending or tolerance sphere).pdf: `./docs/sigmatek_blending/Rounding Mode (blending or tolerance sphere).pdf`
> - Smooth Rounding Mode with Cubic Spline.pdf: `./docs/sigmatek_blending/Smooth Rounding Mode with Cubic Spline.pdf`
> - Synchronization of Axis Groups.pdf: `./docs/sigmatek_blending/Synchronization of Axis Groups.pdf`

<details><summary>4 previous messages</summary>

> PDF 3개를 직접 읽어서 Sigmatek 문서 기준으로 블렌딩/라운딩/축그룹 동기화 기술을 분리해 정리하겠습니다. 우선 페이지 수와 텍스트 추출 상태부터 확인합니다.
>
> PDF 작업이므로 `pdf` 스킬 기준으로 진행하겠습니다. 이번 요청은 문서 내용 분석이 목적이라 렌더링보다 텍스트 추출과 용어/동작 구조 대조에 집중합니다.
>
> 첫 텍스트 추출 명령은 PowerShell 문법 때문에 실패했습니다. 파일에는 손대지 않았고, 같은 작업을 PowerShell 방식으로 다시 실행합니다.
>
> 세 PDF 모두 텍스트 추출이 됩니다. 이제 페이지별 본문을 뽑아 기술 요소와 조건/제약을 정리하겠습니다.
</details>

분석한 3개 문서 기준으로 보면, Sigmatek의 블렌딩은 단순히 “다음 명령을 끊지 않고 이어붙임” 수준이 아니라 **경로 전환부를 tolerance/blending sphere 안에서 재보간해서 속도·가속도·저크 불연속을 줄이는 기술**입니다.

참조 파일:
- [Rounding Mode](<./docs/sigmatek_blending/Rounding Mode (blending or tolerance sphere).pdf>)
- [Smooth Rounding Mode with Cubic Spline](<./docs/sigmatek_blending/Smooth Rounding Mode with Cubic Spline.pdf>)
- [Synchronization of Axis Groups](<./docs/sigmatek_blending/Synchronization of Axis Groups.pdf>)

**1. Rounding Mode / Blending Sphere**
`Radius`로 정의한 구 안에서 현재 path segment에서 다음 path segment 방향으로 미리 꺾어 들어가는 방식입니다.

핵심:
- `MoveLinear()` 쪽 `Radius` 파라미터로 blending sphere를 정의
- sphere 진입 후에는 다음 경로 방향으로 연속적으로 유도
- sphere 내부에서는 정확한 path accuracy가 보장되지 않음
- sphere를 빠져나오면 다시 path accuracy와 요구 속도를 회복
- `Radius`가 실제 path 길이보다 크면 실제 길이로 제한
- `Radius = 0`이면 내부적으로 smooth rounding mode가 활성화됨

제약:
- Linear-Linear 전환은 축별 속도 jump를 줄이기 위해 rounding 가능
- Linear-Circular, Circular-Linear은 tangential error 기준으로 판단
- 비접선 전환에서 tangential error가 tolerance보다 크면 rounding 대신 exact stop으로 전환
- Circular-Circular 전환에는 일반 rounding mode를 사용할 수 없음

**2. Smooth Rounding Mode**
비접선 path 전환을 tolerance sphere 안에서 spline으로 재보간하는 방식입니다.

문서에 나온 대표 모드:
- `_LMCPROF_SMOOTH_CUBIC`
- `_LMCPROF_SMOOTH_QUINT`

그리고 조건 판단에 언급된 smooth 계열:
- `_LMCPROF_SMOOTH_PARAB`
- `_LMCPROF_SMOOTH_CUBIC`
- `_LMCPROF_SMOOTH_QUINT`
- `_LMCPROF_SMOOTH_CLOTH`
- `_LMCPROF_CONT_DIRECT`

설정:
- `TransRadius` 사용
- 적용 메서드: `MoveLinear()`, `MoveRelative()`, `MoveCircle()`

Cubic vs Quint 차이:
- Cubic spline: 경로는 부드럽게 이어지지만 spline 시작/끝에서 acceleration jump가 생길 수 있음
- 5th order spline / Quint: rounding 시작/끝에서 acceleration jump가 없고, 허용 axis jerk를 유지하는 쪽

즉 품질 기준으로 보면:
- Cubic = 위치/속도 연속성 중심
- Quint = 가속도/저크까지 더 부드럽게 관리

**3. Exact Stop 자동 전환**
Sigmatek은 블렌딩이 불가능하거나 조건이 맞지 않으면 무조건 블렌딩하려고 하지 않고 exact stop으로 떨어집니다.

조건 예:
- 마지막 motion command까지 추가 sequence가 없으면 exact stop
- 이미 braking 중 exact stop 상태면 정지 완료 후 다음 motion은 standstill에서 재시작
- tolerance sphere 밖에 있을 때, 진입 전 허용 속도까지 감속 가능하면 지정 interpolation mode 실행
- 감속이 불가능하면 smooth rounding mode로 전환

이 부분이 중요합니다. Sigmatek 블렌딩은 “명령만 미리 넣으면 무조건 섞임”이 아니라 **현재 축 상태, tolerance sphere 진입 여부, 감속 가능 여부**를 보고 모드가 바뀝니다.

**4. Axis Group Synchronization**
축 그룹 동기화는 블렌딩 품질과 직접 관련 있습니다. 최대 6개 그룹을 정의할 수 있습니다.

구조:
- Group 1: Main group
- Group 2~6: Secondary group
- 그룹 index가 높을수록 path length 계산 우선순위 낮음

동작:
- Main group의 delta distance를 기준으로 path length 결정
- 보조축은 path에 coupling되어 모든 endpoint를 같은 시간에 도달하도록 맞춤
