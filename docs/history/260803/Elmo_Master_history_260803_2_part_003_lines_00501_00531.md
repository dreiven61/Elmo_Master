  - InternalLimit=0
  - Remote=1

[Home Check 로그](/C:/work/Elmo/Elmo_Master/test/packet_capture/SIGMATEK_API_Analyze/Test4/Home%20Check_Read%20Legacy%20Elmo%20Health.txt:4)와 PCAP이 동일하게 4축 Referenced를 증명합니다. 하지만 [Health PASS](/C:/work/Elmo/Elmo_Master/test/packet_capture/SIGMATEK_API_Analyze/Test4/Home%20Check_Read%20Legacy%20Elmo%20Health.txt:7)는 RPC 성공이라는 뜻이며, Warning 해제를 뜻하지 않습니다.

또한 이번에는 Home Check만 수행됐습니다.

- Set Identity `0x20E7`: 0회
- Group Power/Enable `0x2047`: 0회
- Group Move: 0회

따라서 아직 그룹 테스트로 넘어가면 안 됩니다.

현재 소스를 다시 확인한 결과 TW20 Test Reset은 `FALSE`로 변경돼 있으므로, **현재 예상 Capability는 `0x0000633F`**입니다. PLC는 계속 `0x00000001`, `BootId=0x0D`를 반환합니다.

다음 작업은 테스트가 아니라 PLC 배포 정합성 수정입니다.

1. 현재 소스 상태를 고정합니다.
2. 정확한 `Elmo_EtherCAT_Test_4Axis` 프로젝트를 Rebuild → Link → Download → Restart합니다.
3. 재접속 후 다음을 확인합니다.
   - Capability=`0x0000633F`
   - Map=`0x957F101E`
   - BootId가 `0x0D`와 다른 새 값
4. Axis1~4에서 Move 없이 `0x6061`, `0x6041`, `0x603F`를 읽습니다.
   - Warning=0
   - Fault=0
   - InternalLimit=0
   - `0x603F=0`
5. Elmo EAS에서도 각 드라이브의 활성 Warning 원인을 확인합니다. 원인이 확인되기 전 Reset, TW20, SDO Write는 하지 마십시오.

이 조건을 통과한 다음에만 `Home Check → Group Power On → Set Identity → Enable ACK→Locked → Disable → Power Off` 순서로 진행하면 됩니다.
