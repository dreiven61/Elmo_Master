# 확인 actual position 단위 변환

> 매뉴얼 1.2 unit 변환에서 actual position 값은 plc 값이 넘어오는거지? 1mm가 넘어와야되면 10000 값이 넘어오는거지?
>
> User context
>
> Attachments:
> - LASAL_Motion_Control_API_User_Manual_KO.docx: `./LMC_Library/LMC_API_Distribution_candidate_20260731_manual_2_0_provenance/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.docx`

<details><summary>3 previous messages</summary>

> 문서의 1.2 표기와 실제 PLC→C# 전송 코드를 함께 대조하겠습니다. `.docx` 확인에는 `doc` 스킬을 사용하고, 이 스킬이 요구하는 방식대로 문서 내용부터 정확히 읽겠습니다.
>
> 질문의 핵심은 “PLC 내부 원시값을 그대로 보내는가”와 “매뉴얼의 공학 단위 환산을 누가 적용하는가”입니다. 최신 추적 소스, 패킷 맵, 매뉴얼 예제를 각각 분리해서 확인하고 있습니다.
>
> 확인 결과, 여기서 말하는 `Actual position`은 엔코더 count 자체가 아니라 PLC 모션 축이 `APPUNIT` 모드로 반환한 값입니다. 이제 저장된 축 UNIT 설정까지 대조해 “현재 프로젝트 기준”과 “실제 다운로드된 PLC 기준”을 구분해 답하겠습니다.
</details>

네, 맞습니다. 정확히는 PLC의 **encoder raw count가 아니라 MotionLib의 application UNIT 값**이 넘어옵니다.

현재 설정이 `1 mm = 10000 DINT`이므로:

- `ActualPosition = 10000` → `1.0000 mm`
- `ActualPosition = 1` → `0.0001 mm`
- mm 변환: `PositionRaw / 10000.0`

PLC는 [`ReadPosition(LMCAXIS_ACTPOS_APPUNIT)`](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st:1904) 값을 DINT로 응답하고, C#은 [`PositionRaw`를 변환 없이 반환](./LMC_Library/LMC_API_Delivery/src/LmcAxis.cs:452)합니다. 매뉴얼 1.2 내용과 일치합니다.

다만 두 가지는 구분해야 합니다.

- 실제 피드백은 위치 오차 때문에 `9998`, `10002`처럼 나올 수 있습니다.
- 실제 PLC에 다운로드된 UNIT 설정이 현재 소스의 `1 mm = 10000`과 같은지는 별도 확인이 필요합니다.
