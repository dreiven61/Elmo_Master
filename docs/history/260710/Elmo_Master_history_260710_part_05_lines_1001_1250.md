> - Searched the web for `SIGMATEK LASAL PLCopen BufferMode MC_Buffered MC_Aborting single axis manual`
>
> </details>
>
> </details>
>
> 웹에서 SIGMATEK 제품 페이지는 LASAL Motion이 motion library를 제공한다는 정도만 공개하고, PLCopen BufferMode의 세부 동작은 PLCopen 규격과 벤더별 MC 문서가 더 직접적입니다. 그래서 비교 기준은 `PLCopen 규격`, `Maestro API 매뉴얼 추출본`, `현재 LASAL 클래스/프로젝트 코드` 세 축으로 잡겠습니다.
>
> 현재 LASAL 프로젝트 쪽은 `_LMCAxis.MoveAbsolute()` 자체에 BufferMode 입력이 없습니다. 반대로 SIGMATEK 내부 profile 쪽에는 `MoveImmediately`라는 플래그가 있어서, PLCopen의 `Aborting/Buffered`와 유사한 결정을 그 레벨에서 하는 구조로 보입니다. 관련 라인을 더 좁혀서 확인하겠습니다.
>
> 현재 `TCPMotionInterface`는 `bufMode`를 파싱은 하지만 `MoveAbs()`에서 쓰지 않습니다. 즉 지금 프로젝트 기준으로는 PC에서 BufferMode를 보내도 Single Axis에는 전달되지 않습니다. `_LMCAxis.MoveAbsolute()`의 실제 기본 동작을 더 보겠습니다.
>
> Maestro 쪽은 `MoveAbsolute/Relative/Velocity/Stop/Halt` 모두 `eBufferMode`를 직접 입력으로 갖고, 규격상 `Aborting`이면 즉시 중단, `Buffered`면 이전 이동 완료 후 실행입니다. LASAL `_LMCAxis` 단축 명령은 이 입력이 없다는 점이 핵심 차이입니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
</details>

결론부터 말하면, **현재 우리 LASAL Single Axis 경로는 PLCopen의 Buffered Mode를 제대로 처리하고 있지 않다.** 패킷에서 `bufMode`는 읽지만, 실제 `MoveAbsolute()` 호출에는 반영하지 않는다.

**PLCopen/PMAS 기준**
PLCopen의 일반 동작은 명확하다.

- `Aborting`: 새 명령이 현재 모션을 중단하고 즉시 실행, 기존 버퍼 삭제
- `Buffered`: 현재 명령이 끝난 뒤 다음 명령 실행
- `Blending*`: 현재 명령 끝점에서 정지하지 않고 다음 명령과 속도 연결

Beckhoff PLCopen 문서도 같은 구조다. `BufferMode`는 축이 이미 다른 명령을 수행 중일 때 새 명령을 어떻게 처리할지 결정하고, 같은 FB 인스턴스를 active 상태에서 다시 트리거하는 방식이 아니라 보통 두 번째 FB 인스턴스를 써야 한다고 설명한다.
출처: [Beckhoff MC_BufferMode](https://infosys.beckhoff.com/content/1033/tcplclib_tc2_mc2/70147595.html), [Beckhoff MC_MoveAbsolute](https://infosys.beckhoff.com/content/1033/tcplclib_tc2_mc2/70094731.html)

Elmo/Maestro 매뉴얼도 PMAS 쪽은 Single Axis 명령에 `eBufferMode`를 직접 가진다. 예를 들어 `MMC_MoveAbsolute`, `MMC_MoveAdditive`, `MMC_Stop` 계열에서 `MC_ABORTING_MODE = 1`, `MC_BUFFERED_MODE = 2`, `MC_BLENDING_LOW_MODE = 3` 등이 정의되어 있다.
로컬 근거: [016_p0313-p0350_6.1-Single-Axis-Motion-Control.md](./output/pdf/maestro_api_md/chunks/016_p0313-p0350_6.1-Single-Axis-Motion-Control.md:1621), [017_p0351-p0387_6.1.6-MMC_MoveAdditive.md](./output/pdf/maestro_api_md/chunks/017_p0351-p0387_6.1.6-MMC_MoveAdditive.md:103)

**LASAL 현재 코드 기준**
현재 `TCPMotionInterface`는 `0x209F` Single Axis Move 패킷에서 `bufMode`를 파싱한다.

- `bufMode` 파싱: [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:470)
- 하지만 실제 실행은 `LMCAxis.MoveAbsolute(Position:=pos, Speed:=velo, Accel:=acc, Decel:=dec, Jerk:=jer);` 만 호출한다: [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:594)
- `_LMCAxis.MoveAbsolute` 인터페이스 자체에도 `BufferMode` 입력이 없다: [_LMCAxis.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/_LMCAxis/_LMCAxis.st:388)

즉, PC에서 연속 명령이 들어와도 현재 Single Axis TCP 경로는 `Buffered`, `Aborting`, `Blending`을 구분해서 처리하지 않는다. `bufMode` 값은 사실상 버려진다.

**LASAL 내부에 비슷한 개념은 있음**
LASAL 쪽에 버퍼 개념이 아예 없는 건 아니다. `_LMCProfile` 계열에는 `CmdConfig.MoveImmediately`가 있다.

- `MoveImmediately = FALSE`: 마지막 버퍼 명령 뒤에 삽입
- `MoveImmediately = TRUE`: 버퍼 삭제, 현재 모션 인터럽트, 즉시 실행

근거: [types.h](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Include/types.h:708)

이건 PLCopen 기준으로 보면 대략 `Buffered` 대 `Aborting`에 해당한다. 하지만 이 구조는 `_LMCProfile.MoveLinear/MoveRelative` 같은 profile/buffer 경로에 붙어 있고, 현재 우리가 쓰는 `_LMCAxis.MoveAbsolute` Single Axis 직접 호출 경로에는 연결되어 있지 않다.

따라서 비교 결과는 이거다.

| 구분 | PMAS/Maestro/PLCopen | 현재 LASAL Single Axis 구현 |
|---|---|---|
| BufferMode 입력 | 있음 | 패킷에는 있으나 실행에 미반영 |
| Buffered 처리 | 이전 모션 완료 후 실행 | 구현 없음 |
| Aborting 처리 | 현재 모션 중단 후 즉시 실행 | 명시 구현 없음 |
| Blending 처리 | 모드별 속도 연결 | 구현 없음 |
| LASAL 대체 구조 | 해당 없음 | `_LMCProfile.CmdConfig.MoveImmediately` 사용 가능 |

내 판단은 명확하다. **현재 코드를 PLCopen Buffered Mode 호환이라고 보면 안 된다.**
호환시키려면 `bufMode`를 실제 실행 로직에 연결해야 하고, 단순 `_LMCAxis.MoveAbsolute` 직접 호출로는 부족하다. 최소한 `Buffered`는 TCPMotionInterface 내부 큐를 만들어 현재 모션 완료 후 다음 명령을 실행해야 하고, `Aborting`은 현재 버퍼/모션을 정리하고 즉시 새 명령을 넣는 구조가 필요하다. `Blending`까지 맞추려면 `_LMCProfile` 버퍼 기반으로 옮기는 쪽이 맞다.

> C:\Program Files (x86)\Sigmatek\Lasal 여거서 매뉴얼 찾아서 확인해줄수 있냐 LMC Library 관련

<details><summary>12 previous messages</summary>

> 설치된 LASAL 쪽 매뉴얼과 도움말 파일을 먼저 찾고, LMC/버퍼 관련 키워드가 실제 문서에 있는지 확인하겠습니다. Program Files 경로라 파일 수가 많을 수 있어서 이름 검색 후 필요한 문서만 좁혀서 보겠습니다.
>
> `Class2\Runtime\Doku\HelpInfoData\HelpInfo_MotionLib.xml`가 가장 유력합니다. 설치 폴더 안의 일반 PDF보다 LASAL Motion/LMC 라이브러리 도움말 인덱스 쪽이라, 여기서 실제 LMC 클래스 문서 위치를 따라가겠습니다.
>
> 도움말은 PDF가 아니라 CHM입니다. `LASAL_CLASS_2_MotionLib_EN.chm`와 `LASAL_CLASS_2_Library_Motion_en.chm` 두 개가 핵심으로 보이므로, 임시 폴더에 풀어서 `_LMCAxis`, `_LMCProfile`, `MoveImmediately` 항목을 직접 검색하겠습니다.
>
> `hh.exe -decompile`은 이 환경에서 파일을 풀지 못했습니다. 압축 해제 도구나 CHM 내부 인덱스를 다른 방식으로 접근 가능한지 확인해서 계속 보겠습니다.
>
> 로컬에 `pdftotext`는 있지만 CHM용 `7z`는 없습니다. CHM은 압축된 HTML Help 파일이라, Python 라이브러리나 내부 바이너리 검색으로 접근 가능한 부분부터 확인하겠습니다.
>
> CHM 안에 페이지 목록은 보이지만 본문은 LZX 압축이라 단순 문자열 검색으로는 부족합니다. 시스템에 숨겨진 7-Zip 계열 실행 파일이 있는지 먼저 찾고, 없으면 Python 쪽 CHM 추출 라이브러리를 임시로 써보겠습니다.
>
> `C:\Program Files\7-Zip\7z.exe`가 있습니다. 이제 CHM을 임시 폴더에 실제로 풀어서 본문 HTML을 검색하겠습니다.
>
> 압축 해제는 성공했습니다. 본문 HTML 기준으로 LMC Axis와 Profile의 연속 명령 처리 관련 문장을 검색합니다.
>
> 전체 검색은 HTML 수가 많아서 시간 초과가 났습니다. 필요한 페이지는 확인됐으니 `_LMCAxis.MoveAbsolute`, `_LMCProfile.MoveLinear/MoveRelative`, `_LMCPROFCMDCONFIG` 문서만 직접 열어서 보겠습니다.
>
> 핵심 문서에서 확인한 내용이 꽤 명확합니다. `_LMCAxis` 설명에는 “commands cannot be buffered, one command per cycle”라고 되어 있고, `_LMCProfile`에는 별도 motion buffer가 있다고 되어 있습니다. `MoveImmediately`의 반대 동작까지 문서에서 더 확인하겠습니다.
>
> 문서상 핵심은 세 갈래입니다. `_LMCAxis`는 버퍼링 없음, `_LMCProfile`은 버퍼 있음, `MoveImmediately`는 기존 입력 movement를 버리고 즉시 새 목표로 간다고 되어 있습니다. 마지막으로 원문 주변 문맥을 더 정리해서 결론을 내겠습니다.
>
> 설치 매뉴얼과 현재 프로젝트 코드가 같은 결론을 가리킵니다. `_LMCAxis`는 새 명령이 들어오면 현재 조건에 맞춰 프로파일을 조정하지만, PLCopen식 대기열 버퍼로 쌓는 구조는 아니라고 되어 있습니다.
</details>

가능하다. 확인했다.

찾은 LMC 관련 매뉴얼은 여기다.

- `C:\Program Files (x86)\Sigmatek\Lasal\Class2\Runtime\Doku\LASAL_CLASS_2_MotionLib_EN.chm`
- `C:\Program Files (x86)\Sigmatek\Lasal\Class2\Runtime\Doku\LASAL_CLASS_2_Library_Motion_en.chm`
- 인덱스: [HelpInfo_MotionLib.xml](C:/Program%20Files%20(x86)/Sigmatek/Lasal/Class2/Runtime/Doku/HelpInfoData/HelpInfo_MotionLib.xml:3)

확인 결과는 명확하다.

`_LMCAxis` Single Axis는 PLCopen식 Buffered Mode 큐 구조가 아니다. 매뉴얼에 “명령은 버퍼링될 수 없고, 사이클당 하나의 명령만 accepted”라고 되어 있다. 대신 축이 움직이는 중 새 motion command 또는 같은 command의 다른 parameter가 들어오면, 현재 모션 프로파일을 새 조건에 맞춰 조정한다. 방향이 반대로 바뀌면 정지까지 감속한 뒤 새 방향으로 가속한다고 설명되어 있다.

근거:
- [funktionsbeschreibung_lmcaxis.htm](C:/Users/dreiv/AppData/Local/Temp/sigmatek_library_motion_7z/funktionsbeschreibung_lmcaxis.htm:142)
- [funktionsbeschreibung_lmcaxis.htm](C:/Users/dreiv/AppData/Local/Temp/sigmatek_library_motion_7z/funktionsbeschreibung_lmcaxis.htm:158)

`_LMCAxis.MoveAbsolute` 자체도 `BufferMode`나 `CmdConfig` 입력이 없다. 입력은 `Position`, `Speed`, `Accel`, `Decel`, `Jerk`뿐이다. 현재 프로젝트의 `_LMCAxis.st`도 동일하다: [_LMCAxis.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/_LMCAxis/_LMCAxis.st:388)

반대로 `_LMCProfile`에는 motion buffer가 있다. 매뉴얼상 기본/최소 201개 profile set, 최대 2000개까지 설정 가능하다. 그리고 `CmdConfig.MoveImmediately`가 있다.

- `MoveImmediately = FALSE`: movement buffer 뒤에 삽입
- `MoveImmediately = TRUE`: 즉시 실행, buffer 삭제, 현재 movement interrupt

근거:
- [funktionsbeschreibung_lmcprofile.htm](C:/Users/dreiv/AppData/Local/Temp/sigmatek_library_motion_7z/funktionsbeschreibung_lmcprofile.htm:149)
- [types.h](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Include/types.h:708)
- [change-of-direction.htm](C:/Users/dreiv/AppData/Local/Temp/sigmatek_library_motion_7z/change-of-direction.htm:143)

현재 우리 코드도 매뉴얼과 맞게 보면 `bufMode` 처리가 안 되는 게 아니라, **애초에 `_LMCAxis` 경로에서는 처리할 수 있는 API가 없다**가 더 정확하다. `TCPMotionInterface`는 `bufMode`를 파싱하지만, 실제 실행은 그냥 `LMCAxis.MoveAbsolute(...)`로 끝난다.

- `bufMode` 파싱: [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:470)
- 실제 호출: [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:594)

정리하면, PLCopen `Buffered Mode`를 LASAL에서 제대로 맞추려면 `_LMCAxis`가 아니라 `_LMCProfile` 쪽 buffer 구조를 쓰거나, Single Axis용 큐를 우리가 따로 구현해야 한다. 지금 Single Axis TCP 경로에서 `bufMode`를 PLCopen처럼 해석하면 틀린 구현이 된다.

> ConnectData.SingleAxis.MoveAbsolute(pos, ConnectData.SingleAxis.HomeBufferedMode);
>
> elmo 예제를 보면 ConnectData 랑 SingleAxis 이렇게 붙는데 왜 그런거야?

<details><summary>4 previous messages</summary>

> Elmo 예제의 `ConnectData`가 단순 변수명인지, MMCLib가 의도한 연결 컨텍스트 구조인지 확인하겠습니다. 로컬 예제와 API 문서에서 같은 패턴을 찾아서 설명하겠습니다.
>
> 검색 결과가 문서/히스토리까지 섞여서 많습니다. 이제 실제 예제 코드와 MMCLib 래퍼 정의만 좁혀서 보겠습니다.
>
> 현재 저장소 안에는 `HomeBufferedMode`라는 심볼은 안 잡힙니다. 그래서 이건 Elmo 공식 래퍼 자체라기보다 예제/샘플 앱에서 만든 `ConnectData` 모델일 가능성이 큽니다. 공식 문서의 `MMCSingleAxis` 구조와 우리 WPF의 `PmasControllerContext`를 기준으로 해석하겠습니다.
>
> 공식 문서상 핵심은 “C 함수들을 C++/클래스 메서드로 묶어서, connection handle과 axis ref를 객체가 들고 다니게 만든 구조”입니다. `ConnectData.SingleAxis`는 그 구조를 샘플 앱에서 한 번 더 감싼 이름으로 보면 됩니다.
</details>

`ConnectData.SingleAxis`는 Elmo API 자체의 필수 문법이 아니라, 예제/앱에서 만든 **연결 컨텍스트 객체 안에 Single Axis 객체를 넣어둔 구조**로 보면 된다.

구조는 대략 이거다.

```csharp
ConnectData          // 연결 상태: IP, handle, callback, axis 객체들
  .SingleAxis        // 특정 축을 나타내는 MMCSingleAxis 객체
    .MoveAbsolute()  // 그 축에 명령 실행
```

Elmo의 `MMCSingleAxis`는 생성/초기화할 때 이미 `connection handle`과 `axis name/ref`를 내부에 가진다. 그래서 원래 C 함수처럼 매번 `hConn`, `hAxisRef`를 넘기지 않고:

```csharp
SingleAxis.MoveAbsolute(pos, bufferMode);
```

처럼 축 객체에 바로 명령을 건다.

공식 문서도 `MoveAbsolute` 시그니처가 이런 식이다.

```c
int MoveAbsolute(
  double dPos,
  MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
)
```

근거: [ch24_24_3_The-MMCSingleAxis-class.md](./output/pdf/maestro_api_md/api_parts_ko/ch24_24_3_The-MMCSingleAxis-class.md:374)

우리 PMAS WPF도 같은 패턴이다. `PmasControllerContext`가 연결 컨텍스트 역할을 하고, `LoadAxis()`에서 `SingleAxis = new MMCSingleAxis(axisName, Handle);`로 축 객체를 만든다: [PmasControllerContext.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/Services/PmasControllerContext.cs:232)

그래서 우리 쪽 표현은:

```csharp
Context.SingleAxis.MoveAbsoluteEx(...);
```

Elmo 예제 표현은:

```csharp
ConnectData.SingleAxis.MoveAbsolute(...);
```

둘 다 같은 의도다. 이름만 `Context`냐 `ConnectData`냐 차이다.

다만 `ConnectData.SingleAxis.HomeBufferedMode`를 `MoveAbsolute`에 넘기는 건 이름이 좀 이상하다. 타입이 `MC_BUFFERED_MODE_ENUM`이면 동작은 가능하지만, 의미상 `HomeBufferedMode`는 홈 명령용 설정처럼 보인다. `MoveAbsolute`용이면 `MoveBufferedMode`, `MotionBufferedMode`, `DefaultBufferMode` 같은 이름이 더 맞다.

> LmcMotionApi라는 이름이 잘 안맞는거 같다 LasalMotionControlLib라고 LmcMotionApi를 모두 바꿔

<details><summary>18 previous messages</summary>

> 이름이 실제로 어디에 박혀 있는지 먼저 전부 찾겠습니다. 단순 문자열뿐 아니라 프로젝트/솔루션/네임스페이스/어셈블리명/폴더명까지 같이 봐야 빌드 산출물 이름도 따라갑니다.
>
> 현재 이름은 두 갈래입니다. 라이브러리 본체 `LMC_API_Delivery/src`와 테스트 앱 `LMC_Library/LmcMotionApiTestApp` 둘 다 이름에 박혀 있습니다. 과거 히스토리 문서는 사실 기록이라 일단 작업 대상에서 빼고, 현재 소스/패키지/README/프로젝트 산출물 기준으로 바꾸겠습니다.
>
> 수정 범위는 `namespace`, `using`, 프로젝트/솔루션 파일명, 어셈블리명, 테스트 앱 네임스페이스/타이틀, 전달 패키지 DLL/EXE 이름입니다. 먼저 프로젝트 설정을 읽고 빌드 도구가 무엇인지 확인한 뒤 일괄 변경하겠습니다.
>
> MSBuild는 PATH에는 없고 `dotnet`만 잡힙니다. .NET Framework 4.8 프로젝트라 Visual Studio MSBuild 위치도 찾아두고, 변경 후 가능한 방식으로 빌드 확인하겠습니다.
>
> 이제 실제 변경을 넣습니다. 이름은 `LasalMotionControlLib`로 통일하고, 테스트 앱은 `LasalMotionControlLibTestApp`으로 맞추겠습니다. 과거 기록인 `docs/history/**`는 작업명 변경 대상에서 제외합니다.
>
> 기본 치환과 파일/폴더명 변경은 끝났습니다. 이제 남은 `LmcMotionApi`가 어디에 있는지 확인하고, 빌드가 새 이름으로 제대로 나오는지 확인하겠습니다.
>
> 잔여 `LmcMotionApi`는 `docs/history`의 과거 로그 두 군데뿐입니다. 현재 소스/패키지 파일명에서는 빠졌습니다. 이제 라이브러리와 테스트 앱을 새 프로젝트명으로 빌드합니다.
>
> 라이브러리 빌드는 새 이름 `LasalMotionControlLib.dll`로 성공했습니다. 이어서 테스트 앱을 새 참조명으로 빌드하고, 빌드 산출물을 전달 패키지 쪽에 복사해 맞추겠습니다.
>
> 테스트 앱 빌드는 이름 변경 때문이 아니라 기존 테스트 앱이 현재 라이브러리 API와 안 맞아서 실패합니다. `LMC_RpcInitConnection` 인자 수와 그룹 메서드명이 현재 라이브러리와 다릅니다. 이름 변경 작업을 끝내려면 테스트 앱도 현재 API에 맞춰 같이 고쳐야 합니다.
>
> 테스트 앱은 현재 라이브러리에서 빠진 그룹 API를 호출하고 있었습니다. 이름 변경만으로는 빌드가 안 되므로, 테스트 앱의 그룹 메뉴를 현재 `LMCGroup`에 실제 존재하는 메서드만 호출하도록 줄이고 연결 호출 인자도 현재 시그니처에 맞추겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalMotionControlLibTestApp/MainWindow.xaml.cs` (+5 -18)
>
> </details>
>
> 테스트 앱 API 호출을 현재 라이브러리에 맞췄습니다. 다시 빌드해서 새 EXE와 DLL이 실제로 생성되는지 확인하겠습니다.
>
> 빌드는 성공했습니다. 이제 새로 생성된 DLL/EXE를 전달 패키지의 `bin`과 `test-app` 위치에 복사해서 패키지 산출물 이름도 새 이름으로 맞추겠습니다.
>
> 산출물 복사까지 끝났습니다. 이제 파일명과 본문에 이전 이름이 남아 있는지, 그리고 diff/check 상태를 확인하겠습니다.
>
> 검사해보니 이전 이름은 소스가 아니라 `.vs`, `bin`, `obj` 같은 생성물에도 남아 있었습니다. 사용자가 “모두”라고 했으니 현재 작업 영역 기준으로 이전 이름 생성물도 지우고, 히스토리 문서의 오래된 패키지 경로 표기도 새 이름으로 맞추겠습니다.
>
> 이전 이름 생성물까지 정리했습니다. 마지막으로 전체 검색, 빌드, diff check를 다시 돌려서 현재 상태를 정리하겠습니다.
>
> 검증까지 끝났습니다. `LmcMotionApi` 문자열/파일명은 현재 전체 작업 폴더 검색에서 더 이상 잡히지 않습니다. 최종 답변 전에 변경 요약과 검증 결과만 정리하겠습니다.
>
> `git diff --stat`은 아직 Git에 새 이름 파일을 add하지 않은 상태라 삭제 위주로 보입니다. 실제 새 파일은 존재하고, 커밋할 때는 새 이름 파일들을 함께 add해야 합니다. 요청이 커밋은 아니었기 때문에 여기서는 staging은 하지 않았습니다.
</details>

`LmcMotionApi` 이름을 `LasalMotionControlLib`로 바꿨습니다.

변경한 범위:
- 라이브러리 네임스페이스: `LmcMotionApi` -> `LasalMotionControlLib`
- 프로젝트/솔루션: [LasalMotionControlLib.csproj](./LMC_Library/LMC_API_Delivery/src/LasalMotionControlLib.csproj), [LasalMotionControlLib.sln](./LMC_Library/LMC_API_Delivery/src/LasalMotionControlLib.sln)
- 테스트 앱: `LmcMotionApiTestApp` -> [LasalMotionControlLibTestApp](./LMC_Library/LasalMotionControlLibTestApp/LasalMotionControlLibTestApp.csproj)
- README/sample/docs의 `using LmcMotionApi;`, DLL명, EXE명 표기
- 전달 산출물:
  - `LMC_API_Delivery/bin/LasalMotionControlLib.dll`
  - `LMC_API/LMC_API/bin/LasalMotionControlLib.dll`
  - `LMC_API/LMC_API/test-app/LasalMotionControlLib.dll`
  - `LMC_API/LMC_API/test-app/LasalMotionControlLibTestApp.exe`
