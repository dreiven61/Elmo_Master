
> 현재 `LMC_Response`는 실제 응답 모델이라기보다 “응답 꼬리 status/error를 대충 뽑는 공통 wrapper”에 가깝다. 정확히 판단하려면 현재 parser가 어떤 offset을 읽는지와 패킷 분석 문서의 response header를 다시 대조하겠다.
>
> 확인 결과 `LMC_Response.Parse()`가 raw 끝 4바이트를 status/error로 읽고 있다. 그런데 패킷 분석 문서상 response header는 offset 0 status/reserved, offset 2 payload length, offset 8부터 payload다. 즉 지금 구조는 일부 응답에서는 우연히 맞고, 값이 들어있는 read 응답에서는 오해를 만들 수 있다. 이 판단을 설계 문서로 남기겠다.
>
> `LMC_Response`는 “지금 당장 command ack용으로는 쓸 수 있지만, 전체 응답 모델로는 부족하다”가 결론이다. 그래서 깨야 할 부분과 유지할 부분을 분리한 응답 모델 설계 문서를 추가한다.
>
> <details><summary>Created a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/RESPONSE_MODEL_DESIGN_2026-07-09.md` (+239 -0)
>
> </details>
>
> 응답 모델 설계 문서를 추가했다. README에 문서 링크만 추가하고, 코드 변경은 하지 않는다. 지금 질문은 설계 판단이 핵심이고, 코드 변경은 별도 단계로 나누는 게 안전하다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+1 -0)
>
> </details>
>
> 문서 변경만 들어갔다. 공백 검사와 변경 범위를 확인한 뒤 설계 문서 커밋을 만든다.
>
> `git diff --stat`은 untracked 문서를 표시하지 않아서 README만 보였지만, 새 설계 문서는 실제로 추가되어 있다. 이제 두 파일만 스테이징한다.
>
> 스테이징 완료했다. 스테이징된 범위만 확인하고 커밋한다.
>
> 커밋 대상은 응답 모델 설계 문서와 README 링크만이다. 기존 다른 변경은 제외한다.
>
> 설계 문서 커밋까지 완료했다. 마지막으로 커밋 해시와 상태를 확인한다.
</details>

현재 `LMC_Response`는 **임시방편에 가깝다**. command ack 응답 일부에는 맞지만, 전체 응답 모델로 쓰기에는 부정확하다.

문제는 [LmcConnection.cs](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/src/LmcConnection.cs:160)에서 `Status/ErrorId`를 raw packet의 마지막 4바이트에서 읽는다는 점이다. 그런데 패킷 분석 기준 response header는 offset `0`이 header status/reserved, offset `2`가 payload length, offset `8`부터 payload다. 즉 모든 응답이 “끝 4바이트 = status/error” 구조가 아니다. `ReadStatus`, `ReadPosition`, lookup 응답 같은 값 응답에서는 이 방식이 오해를 만든다.

장점:
- 단순하다.
- `Power/Reset/Move` 같은 ack성 응답에는 지금 LASAL 응답 구조상 어느 정도 맞을 수 있다.
- `Raw`를 보존해서 디버깅은 가능하다.

단점:
- response header를 제대로 표현하지 않는다.
- 값 응답과 ack 응답을 구분하지 못한다.
- `IsSuccess`가 실제 command 성공 여부인지, 그냥 기본값 0인지 애매하다.
- RPC init/callback 응답처럼 payload 구조가 아직 덜 분석된 응답을 표현하기 어렵다.

맞는 방향은 `LMC_Response`를 **공통 response envelope**로 바꾸고, command별 payload parser를 따로 두는 구조다. 설계 문서로 정리해서 추가했다: [RESPONSE_MODEL_DESIGN_2026-07-09.md](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/docs/RESPONSE_MODEL_DESIGN_2026-07-09.md:1)

권장 방향:
- `LMC_Response`에 `HeaderStatus`, `PayloadLength`, `HeaderReserved`, `Payload`, `IsFrameValid`, `HasCommandResult`, `CommandStatus`, `ErrorId`를 명확히 둔다.
- `Power/Move/Reset`은 ack parser 사용.
- `GetAxisByName/GetGroupByName`은 lookup parser 사용.
- `ReadStatus/GetActualPosition`은 value parser 사용.
- 기존 `Status/ErrorId/IsSuccess`는 호환용으로 유지하되 새 코드에서는 명확한 필드를 쓰게 한다.

이번에는 설계 문서만 커밋했다. 코드 변경은 아직 안 했다.
커밋: `4cc4421 Document LMC response model direction`

> 설계문서를 기반으로 소스코드 구현 진행해봐

<details><summary>25 previous messages</summary>

> 응답 모델 설계를 코드로 옮기겠다. 우선 현재 `LMC_Response` 사용 지점을 확인하고, public API는 깨지지 않게 유지하면서 내부 parser를 envelope/ack/value/lookup으로 분리한다.
>
> 구현 방향은 호환성을 유지하는 쪽으로 잡았다. `Status` 속성은 남기되, 내부적으로는 `HeaderStatus`와 `CommandStatus`를 구분하고 axis/group read와 lookup은 command-specific parser를 쓰게 바꾼다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs` (+18 -2)
>
> </details>
>
> `LMC_Response`에 envelope 필드를 추가했다. 이제 `LMCConnection.Parse`를 tail parser에서 envelope parser로 바꾸고, ack/value/lookup helper를 추가한다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+95 -6)
>
> </details>
>
> 공통 parser가 envelope 기반으로 바뀌었다. 이제 axis API에서 lookup/value/ack 응답을 각각 맞는 helper로 읽도록 바꾼다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAxis.cs` (+14 -28)
>
> </details>
>
> axis 쪽은 적용했다. lookup에서 out response가 필요 없어서 불필요한 local을 없애고, group 쪽에도 같은 parser를 적용한다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAxis.cs` (+1 -2)
>
> </details>
>
> 이제 group API도 동일하게 value/lookup/ack parser로 교체한다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcGroup.cs` (+11 -21)
>
> </details>
>
> axis/group 적용도 끝났다. 이제 설계 문서에 Phase 1 구현 상태를 표시하고 빌드로 검증한다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/RESPONSE_MODEL_DESIGN_2026-07-09.md` (+9 -1)
>
> </details>
>
> 문서에 구현 상태까지 반영했다. 이제 빌드와 정적 검사를 돌린다.
>
> 빌드가 통과했다. 배포 DLL을 갱신하고, synthetic 응답으로 ack/value/lookup parser가 의도대로 동작하는지 reflection으로 확인한다.
>
> value/lookup synthetic 검증은 통과했다. ack 검증은 PowerShell reflection에서 byte[]가 인자 배열로 펼쳐진 문제라 호출 방식만 고쳐 다시 확인한다.
>
> 방금 PowerShell 실행이 OS access denied로 실패했다. 빌드 산출물 잠금 가능성이 있어서 같은 검증을 간단한 재시도로 처리한다.
>
> 재시도도 같은 OS access denied다. 검증용 PowerShell 프로세스 쪽 문제인지 확인하기 위해 단순 명령으로 확인한다.
>
> DLL 파일 자체는 접근된다. reflection으로 파일을 직접 로드하는 과정에서 잠금이 걸린 것으로 보인다. 임시 복사본으로 ack parser 검증을 진행한다.
>
> 임시 복사 방식도 PowerShell 실행 단계에서 막혔다. 처음 성공했던 reflection 방식으로 돌아가서, byte[] 단일 인자 전달만 PowerShell unary comma로 고쳐본다.
>
> ack/value/lookup parser 검증까지 끝났다. 이제 최종 변경 범위와 배포 DLL 상태를 확인하고 커밋한다.
>
> 변경 범위는 `LMC_API_Delivery`의 response parser 관련 5개 파일이다. 다른 dirty 파일은 그대로 두고 이 범위만 스테이징한다.
>
> 스테이징도 끝났다. 마지막으로 staged 범위와 빌드 검증 상태를 확인하고 커밋한다.
>
> 커밋 대상은 응답 모델 구현 관련 5개 파일로 제한되어 있다. 이제 커밋한다.
>
> 커밋 완료했다. 마지막 상태와 커밋 해시만 확인한다.
</details>

구현 완료했다. `LMC_Response`를 tail-based 임시 parser에서 response envelope 기반으로 바꿨다.

변경 핵심:
- [LmcProtocol.cs](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs:16)
  - `HeaderStatus`, `PayloadLength`, `HeaderReserved`, `Payload`, `IsFrameValid`, `HasCommandResult`, `CommandStatus` 추가
  - 기존 `Status`는 호환용 alias로 유지
- [LmcConnection.cs](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/src/LmcConnection.cs:168)
  - `Parse()`는 공통 response header/payload envelope만 파싱
  - `ParseAcknowledgement()`, `TryParseLookupReference()`, `ParseUInt32Value()`, `ParseInt32Value()` 추가
- [LmcAxis.cs](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/src/LmcAxis.cs:164), [LmcGroup.cs](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/src/LmcGroup.cs:75)
  - lookup/read/ack 응답을 command별 parser로 분리
- [RESPONSE_MODEL_DESIGN_2026-07-09.md](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/docs/RESPONSE_MODEL_DESIGN_2026-07-09.md:172)
  - Phase 1 구현 상태 반영

검증:
- `dotnet build LMC_Library\LMC_API_Delivery\src\LasalMotionControlLib.sln -c Release` 성공, warning/error 0
- `git diff --check` 통과
- synthetic response로 ack/value/lookup parser 검증 완료
- 배포 DLL 갱신: [LasalMotionControlLib.dll](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/bin/LasalMotionControlLib.dll)

커밋 완료: `6869441 Implement envelope based LMC response parsing`

> LmcProtocol.cs에서 패킷을 생성하는 api가 Power, Simple 이런식으로 간단하게 명명되어 있는데, 정확이 Lasal의 LMCAxis랑 LMCGroup의 API의 명명을 붙이는게 안 햇깔릴꺼 같은데.
> 그렇게 수정해줘

<details><summary>25 previous messages</summary>

> 요청대로 `LmcProtocol.cs`의 frame builder 이름을 더 명확하게 바꾸겠다. 먼저 현재 호출 지점을 확인하고, generic builder는 축/그룹 API 단위의 이름으로 분리해서 업데이트한다.
>
> 변경 방향은 이렇게 잡는다. `CreateRequest`, `ReadUInt16` 같은 저수준 helper는 그대로 두고, 실제 command frame builder는 `LMCAxis...`, `LMCGroup...`, `GetAxisByName`, `GetGroupByName` 형태로 바꾼다. `Simple`, `Power`, `Velocity`, `AxisMove`, `MoveLinear` 같은 모호한 internal API는 외부 호출 지점에서 사라지게 한다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs` (+119 -38)
>
> </details>
>
> `LmcProtocol.cs`는 축/그룹별 builder 이름으로 바꿨다. 이제 `LmcAxis.cs`와 `LmcGroup.cs` 호출부를 새 이름으로 맞춘다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAxis.cs` (+41 -11)
>
> </details>
>
> 축 호출부는 새 builder 이름으로 맞췄다. 그룹 호출부도 `LMCGroup...` 이름으로 바꾼다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcGroup.cs` (+8 -8)
>
> </details>
>
> 이제 이전 모호한 builder 이름이 남아 있는지 검색하고, 남아 있으면 정리하겠다.
>
> 남아 있는 `LMC_Frame.Power/Simple/...` 호출은 없다. 축 이동 쪽에는 아직 `Move(command, ...)` 내부 분기가 있어서, 이왕 정리하는 김에 `MoveAbsoluteEx`와 `MoveRelativeEx`가 각각 명확한 frame builder를 직접 호출하도록 더 단순하게 바꾸겠다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAxis.cs` (+18 -67)
>
> </details>
>
> 코드 이름 정리는 완료됐다. 설계 문서에도 frame builder naming rule을 남겨서 다시 `Simple` 같은 이름으로 돌아가지 않게 기록한다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 이어서 진행합니다. 이미 `LmcProtocol.cs`의 내부 패킷 생성 함수명을 `LMCAxis*`, `LMCGroup*` 기준으로 바꾼 상태라, 남은 건 문서 반영과 빌드/검증/커밋입니다.
>
> 검색 결과 기존 `LMC_Frame.Power`, `Simple`, `AxisMove` 같은 호출은 `src` 안에서 더 이상 안 잡힙니다. 이제 설계 문서에 이 명명 기준을 남긴 뒤 빌드로 깨진 참조가 없는지 확인하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_STRUCTURE_DECISION_2026-07-09.md` (+6 -0)
>
> </details>
>
> 문서에는 “공개 API는 기존 wrapper 스타일, 내부 패킷 빌더는 LASAL 대상 명시”로 기준을 추가했습니다. 이제 실제 컴파일러가 모든 rename을 따라가는지 확인합니다.
>
> `dotnet build`는 경고 없이 통과했습니다. 빌드 결과 DLL도 배포용 `bin` 위치에 맞춰 복사하고, 그 다음 diff 검사를 하겠습니다.
>
> 검증상 변경 범위는 의도한 네 파일뿐입니다. `git diff --check`도 통과했고, 표시된 CRLF 경고는 Git의 줄끝 변환 안내라 diff 오류는 아닙니다. 커밋 전 마지막으로 실제 diff를 한 번 더 확인합니다.
>
> diff 기준으로 동작 변경은 명명 변경에 한정되어 있습니다. 다만 lookup 쪽 `GetAxisByName`/`GetGroupByName`도 “패킷 생성 API”라서, 요청 취지에 맞게 `LMCAxisGetByName`/`LMCGroupGetByName`까지 맞추는 게 더 일관됩니다.
>
> lookup 패킷 빌더까지 `LMCAxis*`, `LMCGroup*` 접두로 맞춰서 마무리하겠습니다. 이렇게 해야 `LmcProtocol.cs` 안의 command-facing 함수가 전부 LASAL 대상 기준으로 읽힙니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAxis.cs` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcGroup.cs` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_STRUCTURE_DECISION_2026-07-09.md` (+5 -4)
>
> </details>
>
> lookup까지 접두를 맞췄습니다. 이제 한 번 더 검색과 빌드를 돌려서 남은 옛 이름이나 깨진 참조가 없는지 확인합니다.
>
> 검색상 옛 빌더 이름은 남아 있지 않고, Release 빌드도 다시 통과했습니다. 방금 생성된 DLL을 배포 폴더에 다시 맞춰 놓고 커밋 준비를 하겠습니다.
>
> 변경 대상은 `LMC_API_Delivery`의 설계 문서와 세 소스 파일뿐입니다. 이제 이 범위만 선택해서 stage하고 커밋합니다.
>
> stage 완료했습니다. staged diff가 의도한 네 파일만인지 확인한 뒤 커밋을 만들겠습니다.
</details>

수정 완료했고 커밋까지 만들었습니다.

`LmcProtocol.cs`의 패킷 생성 함수명을 `Power`, `Simple`, `Velocity`, `MoveLinear` 같은 모호한 이름에서 `LMCAxisPower`, `LMCAxisMoveAbsolute`, `LMCGroupEnable`, `LMCGroupMoveLinearAbsolute`처럼 LASAL 대상이 드러나는 이름으로 정리했습니다. lookup도 `LMCAxisGetByName`, `LMCGroupGetByName`으로 맞췄고, 호출부인 `LmcAxis.cs`, `LmcGroup.cs`도 전부 반영했습니다.
