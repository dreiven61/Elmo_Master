ip.addr == 10.10.150.1 &&
((tcp.port == 4000 && tcp.len > 0) || udp.port == 5000)
```

각 시험은 별도 캡처로 저장하고, 버튼 실행 전부터 시작해서 `PASS/FAIL` 및 cleanup 완료 후 최소 2초 뒤 종료하십시오. 새 runner 실행 시 이전 QTEST 로그가 지워지므로 매번 즉시 `Save QTEST Log`를 해야 합니다. 전체 실행 조건은 [WPF qualification 기준](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/README.md:115)에 정리돼 있습니다.

현재 실행하면 안 되는 항목:

- Recorder Double Bank
- PI Write / SDO Write
- 8/12-byte extended SDO
- `Read Selected Health`
- Digital Input/Output Shadow
- Digital Output Write
- `0x7E13`, `0x7E22`, `0x7E23`

오늘 바로 시작한다면 `P0 → Bulk 2종 → Recorder Single/Ring → Group Enable → 0x2045 RPC`까지만 먼저 진행하는 것이 안전합니다.
