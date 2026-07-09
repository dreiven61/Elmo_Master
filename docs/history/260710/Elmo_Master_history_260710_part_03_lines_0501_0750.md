>
> `git diff --check --cached`는 통과했습니다. 소스 쪽에서 헤더/응답 처리와 변환 함수가 의도대로 들어갔는지 한 번 더 확인하고 커밋하겠습니다.
>
> 최종 DLL 해시와 staged 내용이 확인됐습니다. `src/bin`, `obj`, 기존 untracked LASAL 비교 폴더, 테스트앱 `.sln`은 커밋 대상에서 제외하고 새 LASAL DLL 배포 폴더만 커밋합니다.
>
> 커밋 완료했습니다. 마지막으로 HEAD와 남은 작업트리를 확인해서 의도하지 않은 파일이 섞이지 않았는지 정리하겠습니다.
</details>

확인했고, 새 배포 폴더 기준으로 수정해서 커밋했습니다.

커밋:
- `549a838 Add LASAL DINT motion API delivery`

반영 내용:
- `C:\work\Elmo\Elmo_Master\LMC_Library\LMC_LASAL_API_Delivery`
- 최종 DLL 포함: `bin/LmcLasalMotionApi.dll`
- DLL 소스 포함: `src/*.cs`, `src/LmcLasalMotionApi.csproj`
- 문서/샘플 포함

내가 추가로 고친 점:
- 기존 `rev * 8,388,608 count` 변환을 `rev * 360 * 10000`으로 수정했습니다.
- 즉 `1 rev -> 3,600,000 LASAL internal DINT`.
- 수신부 `Exchange()`가 payload length를 header offset `2`에서 읽던 버그를 offset `4`로 고쳤습니다.

검증:
- Release 빌드 성공
- DLL 리플렉션 검증:
  - `1 rev -> 3600000`
  - `0.5 rev -> 1800000`
  - `MoveAbsoluteEx` offset `[8] = 3600000`
  - `MoveLinearAbsoluteEx` offset `[8] = 3600000`, `[12] = 1800000`
- `git diff --check --cached` 통과

주의:
현재 이 DLL은 DINT 패킷 전용입니다. 그런데 로컬의 `Lasal_PRG/Elmo_EtherCAT_Test_4Axis_Edit/Class/TCPMotionInterface/TCPMotionInterface.st`는 아직 `0x209F`, `0x20A4` 일부가 LREAL 8바이트 offset 기준으로 남아 있습니다. PLC 쪽은 `LMC_LASAL_API_Delivery/docs/LASAL_DINT_PACKET_MAP.txt` 기준으로 바꿔야 이 DLL과 맞습니다.

> 이거 지금 기준이 안맞는다. Lasal이랑 PMAS랑 구조가 다르기 때문에 이렇게 쓰면 안될꺼 같다.
> Axis Interface를 받아올때 External Unit이랑 Internal 유닛을 받아서 처리를 해야 될꺼 같다. 단순히 실수, 정수 변환만 진행해서 될께 아닌거 같아.
>
> 일단 C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API\Elmo_API_Packet2에서 패킷 분석된 자료를 검토해서 패킷 분석한 자료를 md 파일로 만들어.

<details><summary>13 previous messages</summary>

> 현재 요청은 코드 수정이 아니라 패킷 분석 자료를 근거 문서로 고정하는 작업으로 처리하겠습니다. 먼저 `Elmo_API_Packet2` 안의 실제 파일 구성을 보고, 기존 메모리에 남은 패킷/스케일링 관련 주의점도 빠르게 대조하겠습니다.
>
> 작업트리는 추적 파일 기준으로 깨끗합니다. 메모리에는 예전 패킷 분석에서 `tshark` 대신 `scapy`를 썼고, ACK-only와 실제 TCP payload를 분리해야 한다는 주의점이 남아 있습니다. 이번에는 실제 `Elmo_API_Packet2` 파일을 다시 계산해서 문서화하겠습니다.
>
> 파일 구성은 `TXT` 추출본과 `WireShark/*.pcapng` 원본이 1:1로 있습니다. 이제 텍스트 추출본 몇 개와 pcap payload를 같이 읽어서, 캡처에서 확인된 command id, payload length, offset, 값 타입을 표로 만들겠습니다.
>
> 텍스트만 보면 Ethernet 전체 hex라 사람이 읽기 어렵습니다. `pcapng`에서 TCP payload만 추출해 command id와 길이 필드를 계산해서 표로 만들겠습니다. Ethernet padding은 TCP payload로 세지 않게 IP/TCP header 기준으로 계산합니다.
>
> 분석상 중요한 차이가 나왔습니다. 요청 프레임은 length가 offset `4`에 있지만, 응답 프레임은 length가 offset `2`에 있습니다. 그리고 motion 계열도 전부 같은 타입이 아니라 `MoveAbsolute/Relative/Linear`은 LREAL, `Stop/GroupStop`은 REAL, `MoveVelocity`는 길이 필드와 실제 TCP payload 길이가 7바이트 어긋납니다. 이 내용을 md에 명시하겠습니다.
>
> <details><summary>Created a file</summary>
>
> - Wrote `./LMC_Library/LMC_API/Elmo_API_Packet2/PACKET_ANALYSIS.md` (+481 -0)
>
> </details>
>
> `PACKET_ANALYSIS.md`를 새로 만들었습니다. 이제 문서 안의 핵심 문구와 git diff를 확인해서 과장된 추정이 섞이지 않았는지 점검하겠습니다.
>
> 문서는 새 파일 하나만 추가된 상태입니다. 기존 대량 untracked LASAL 비교 폴더와 테스트앱 `.sln`은 건드리지 않고, 패킷 분석 md만 별도 커밋으로 묶겠습니다.
>
> `git diff --check`에서 EOF 빈 줄 경고가 나왔습니다. 문서 마지막 빈 줄을 정리한 뒤 다시 검사하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API/Elmo_API_Packet2/PACKET_ANALYSIS.md` (+0 -1)
>
> </details>
>
> EOF 경고를 정리했습니다. 다시 스테이징하고 검사한 뒤 커밋합니다.
>
> 검사는 통과했고, staged 파일은 `PACKET_ANALYSIS.md` 하나뿐입니다. 이 상태로 패킷 분석 문서 커밋을 만들겠습니다.
>
> 커밋 완료했습니다. 마지막으로 최신 커밋과 남은 작업트리를 확인해서 이번 작업 외 파일이 포함되지 않았는지 확인합니다.
</details>

패킷 분석 문서 만들고 커밋했습니다.

문서:
[PACKET_ANALYSIS.md](./LMC_Library/LMC_API/Elmo_API_Packet2/PACKET_ANALYSIS.md)

커밋:
`48f76ef Document Elmo API Packet2 captures`

핵심으로 정리한 내용:
- Request header와 Response header의 length 위치가 다릅니다.
  - request length: offset `[4]`
  - response length: offset `[2]`
- `MoveAbsoluteEx`, `MoveRelativeEx`, `MoveLinearAbsoluteEx`는 LREAL/double 기반입니다.
- `Stop`, `GroupStop`은 REAL/float 기반입니다.
- `MoveVelocityEx`는 캡처상 header length `48`인데 실제 TCP payload는 `49` bytes라 예외 처리가 필요합니다.
- `0x202B AxisInfo` 캡처만으로는 External/Internal unit 변환 계수를 확인할 수 없습니다.
- 결론적으로 고정 비율 변환 DLL은 기준이 부족하고, 축별 Axis Interface 단위 정보를 받아서 처리해야 합니다.

검증:
- `pcapng`는 `scapy`로 TCP payload 기준 재계산했습니다.
- Ethernet padding은 payload로 세지 않았습니다.
- `git diff --check` 통과했습니다.

참고로 이 문서 기준이면 앞에서 만든 LASAL 전용 DLL의 response length offset `[4]` 처리는 맞지 않습니다. 후속 수정 때 `response`는 offset `[2]` 기준으로 되돌려야 합니다.

남은 untracked 파일은 기존 `Lasal_PRG/Elmo_EtherCAT_Test_4Axis_Edit/**`와 `LmcMotionApiTestApp.sln`이고, 이번 커밋에는 포함하지 않았습니다.

> axis interface의 external unit이랑 internal unit을 받을께 아니라 lasal에서는 단위변환을 unit.h에 담아두고 있는데 그게 필요할꺼같네. unit.h를 참고해서 LMC_MM 이런식으로 define된 단위변환을 추가해줘

<details><summary>36 previous messages</summary>

> `unit.h` 기준으로 단위 상수를 먼저 확인하고, 그 값이 현재 DLL 변환 코드 어디에 들어가야 하는지 보겠습니다. 이전 고정 `360*10000` 방식은 기준이 약하니, LASAL 쪽 define 이름/값을 그대로 반영하는 방향으로 정리하겠습니다.
>
> 검색 결과 `unit.h`가 여러 프로젝트에 있지만, 작업 기준은 `Elmo_EtherCAT_Test_4Axis/Include/unit.h`가 우선입니다. 그 파일과 비교용 edit 폴더의 `unit.h`가 같은지 확인한 뒤 LMC API 쪽에 동일한 단위 enum/상수를 넣겠습니다.
>
> `unit.h` 값은 확인했습니다. `mm=*10000`, `mmpsec=*10000`, `mmPsec2=*1`, `deg=*10000`, `RPM=*1000`처럼 LASAL 프로젝트 단위가 섞여 있으므로, DLL 쪽도 “회전당 내부값” 하나가 아니라 위치/속도/가속도/저크별 scale profile을 갖도록 바꾸겠습니다.
>
> 구현 범위는 `LMC_LASAL_API_Delivery`의 소스 DLL과 문서로 잡겠습니다. 이전 `LMC_API/LMC_API`의 바이너리 패치까지 다시 손대면 원본 DLL/배포본 경계가 흐려지므로, 소스가 있는 LASAL 전용 DLL에 `unit.h` 기준 단위 상수를 먼저 넣습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_LASAL_API_Delivery/src/LmcLasalProtocol.cs` (+76 -9)
>
> </details>
>
> `LMC_Units` 상수와 scale profile을 넣었습니다. 이제 Axis/Group 메서드가 위치/속도/가속도/저크 각각의 단위 scale을 쓰도록 호출부를 바꿉니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_LASAL_API_Delivery/src/LmcLasalAxis.cs` (+6 -6)
>
> </details>
>
> Axis 쪽은 파라미터 이름도 회전 기준 표현에서 일반 application unit 표현으로 바꿨습니다. Group 쪽도 같은 profile을 사용하도록 맞춥니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_LASAL_API_Delivery/src/LmcLasalGroup.cs` (+2 -2)
>
> </details>
>
> 같은 DLL에서 응답 length offset도 기존 패킷 분석 결과와 맞춰 바로 고치겠습니다. request는 `[4]`, response는 `[2]`라서 `Exchange()` 응답 수신부는 `[2]`를 써야 합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_LASAL_API_Delivery/src/LmcLasalConnection.cs` (+1 -1)
>
> </details>
>
> 코드 기준이 바뀌었으니 README, 샘플, 패킷 맵 문서도 `rev/rps`가 아니라 LASAL application unit과 `unit.h` define 기준으로 바꿉니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_LASAL_API_Delivery/README.md` (+5 -4)
> - Wrote `./LMC_Library/LMC_LASAL_API_Delivery/docs/LASAL_DINT_PACKET_MAP.txt` (+7 -2)
> - Wrote `./LMC_Library/LMC_LASAL_API_Delivery/sample/BasicUsage.cs` (+7 -2)
> - Wrote `./LMC_Library/LMC_API/Elmo_API_Packet2/PACKET_ANALYSIS.md` (+10 -9)
>
> </details>
>
> 문서까지 1차 반영했습니다. 이제 빌드해서 DLL을 다시 생성하고, 리플렉션으로 기본 profile이 실제로 `mm/mmpsec/mmPsec2` scale을 쓰는지 확인하겠습니다.
>
> 빌드는 통과했습니다. 다만 소스에 호환용으로 남겨둔 `RevolutionToInternal` 이름은 지금 기준에서는 오해를 부릅니다. 잘못된 회전 기준 API를 계속 노출하지 않도록 제거하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_LASAL_API_Delivery/src/LmcLasalProtocol.cs` (+0 -5)
>
> </details>
>
> 회전 기준 메서드를 제거했습니다. 다시 빌드하고 최종 DLL을 배포 `bin`에 복사한 뒤 해시와 변환값을 검증하겠습니다.
>
> 빌드 산출물을 배포 `bin` DLL로 복사하고, 기본 단위 profile이 실제 프레임에 들어가는 값을 확인하겠습니다.
>
> 해시가 동일합니다. 이제 DLL을 직접 로드해서 `LMC_MM`, `LMC_MMPSEC`, `LMC_MMPSEC2` 값과 `MoveAbsolute/MoveLinear` 프레임 offset 값을 확인합니다.
>
> 프레임 변환값은 맞습니다. 응답 length offset도 로컬 TCP 더미로 확인해서 offset `[2]` 기준으로 payload 4바이트를 읽는지 검증하겠습니다.
>
> PowerShell 스크립트블록 기반 로컬 서버는 스레드 실행 쪽에서 안정적으로 응답을 못 줬습니다. 같은 검증을 작은 C# 헬퍼 서버로 다시 실행하겠습니다.
>
> 응답 length offset도 C# 헬퍼 서버로 검증됐습니다. header `[2]=4`일 때 전체 12바이트를 읽었으므로 response parser가 이제 분석 문서와 맞습니다. 남은 stale 문구가 있는지 확인하고 diff check를 돌리겠습니다.
>
> 검색 결과 기존 `LMC_API/LMC_API` 문서에는 과거 `8388608 -> 3600000` 고정 변환 설명이 아직 남아 있습니다. 지금 기준과 충돌하므로, 문서/샘플 주석만 “구 기준 사용 금지, LASAL 전용 DLL의 unit.h profile 사용”으로 바꾸겠습니다. 바이너리 자체는 소스 없는 이전 실험본이라 이번 범위에서는 건드리지 않습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API/LMC_API/README.md` (+2 -5)
>
> </details>
>
> README의 고정 변환 문구를 제거했습니다. 패킷맵과 함수 목록 문서도 같은 기준으로 정리합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API/LMC_API/docs/LMC_PACKET_MAP.md` (+2 -17)
> - Wrote `./LMC_Library/LMC_API/LMC_API/docs/LMC_PACKET_MAP.md` (+3 -14)
>
> </details>
>
> 패킷맵의 남은 “역변환” 문구도 제거했습니다. 함수 목록과 샘플 주석까지 정리합니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/LMC_API/docs/LMC_API_함수명_커맨드ID_인자.txt` (+2 -6)
> - Wrote `./LMC_Library/LMC_API/LMC_API/sample/BasicUsage.cs` (+3 -4)
>
> </details>
>
> 이제 저장소 내 고정 변환 문구를 다시 검색해서 남은 것이 “잘못된 예”로 표시된 분석 문서뿐인지 확인합니다.
>
> 고정 변환 문구는 이제 “근거 부족한 잘못된 예”로 쓰인 분석 문서 한 줄만 남았습니다. 최종 diff를 확인한 뒤 스테이징/커밋하겠습니다.
>
> 최종 diff는 의도한 범위입니다. 이제 `LMC_Library` 변경만 스테이징하고 커밋합니다. untracked LASAL 비교 폴더는 계속 제외합니다.
>
> 스테이징은 됐고, `bin` 경고는 `.gitignore`가 `bin/`을 기본 무시해서 뜬 것입니다. 이 DLL은 이미 추적 중인 배포 파일이라 상태에는 staged로 잡혔습니다. cached stat만 확인하고 커밋합니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 정리된 변경이 이미 스테이징된 상태라서, 먼저 현재 staged 범위를 확인한 뒤 목적 단위로 커밋하겠습니다. 커밋 후에는 작업트리에 남은 미추적 파일이 섞이지 않았는지도 확인합니다.
>
> 스테이징 범위는 의도한 13개 파일만 잡혀 있고 `git diff --cached --check`도 통과했습니다. 이제 `unit.h` 스케일 적용과 문서 정리를 한 커밋으로 묶겠습니다.
>
> 커밋은 성공했습니다. 마지막으로 커밋 내용과 남은 작업트리 상태를 확인해서, 이번 커밋에 포함된 것과 제외된 것을 분리해서 보고하겠습니다.
</details>

완료. `main` 브랜치에 커밋했습니다.

커밋: `09ff75c Use LASAL unit.h scales in LMC API`

반영한 핵심 내용:
- [LmcLasalProtocol.cs](./LMC_Library/LMC_LASAL_API_Delivery/src/LmcLasalProtocol.cs)에 LASAL `unit.h` 기준 `LMC_Units` 추가
  - `LMC_MM = 10000`
  - `LMC_MMPSEC = 10000`
  - `LMC_MMPSEC2 = 1`
  - `LMC_RPM`, `LMC_DEG`, `LMC_BAR` 등 `unit.h` 단위 상수 반영
- 기존 `3600000/rev` 고정 변환 기준 제거
- 위치/속도/가속/감속/저크를 각각 다른 단위 스케일로 변환하도록 `LMC_UnitConverter` 수정
- Axis/Group API와 샘플/문서도 `unit.h` 기준으로 정리
- `LmcLasalConnection.Exchange()`의 response length offset 오류도 같이 수정함

검증:
- `MSBuild Release` 성공
- 생성 DLL을 `LMC_LASAL_API_Delivery/bin/LmcLasalMotionApi.dll`에 반영
- reflection 테스트로 `1.0 mm -> 10000`, `1.0 mm/s -> 10000`, `1.0 mm/s2 -> 1` 확인
- `git diff --cached --check` 통과

제외한 미추적 파일:
- `Lasal_PRG/Elmo_EtherCAT_Test_4Axis_Edit/**`
- `LMC_Library/LmcMotionApiTestApp/LmcMotionApiTestApp.sln`
