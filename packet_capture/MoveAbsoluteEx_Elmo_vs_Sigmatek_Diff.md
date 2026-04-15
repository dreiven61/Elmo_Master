# MoveAbsoluteEx Packet Diff (Elmo vs Sigmatek)

## Capture Scope
- Elmo: PC `192.168.1.13` <-> Controller `192.168.1.3`
- Sigmatek: PC `10.10.150.13` <-> Controller `10.10.150.1`
- Filter: `tcp && ip.addr==PC && ip.addr==Controller`

## Matched Frames
- Elmo frames: command #287, response #288, ack #289
- Sigmatek frames: command #39, response #40, ack #41

## Protocol Stack
- Elmo command/response protocols: `eth:ethertype:ip:tcp:data` / `eth:ethertype:ip:tcp:data`
- Sigmatek command/response protocols: `eth:ethertype:ip:tcp:gsm_ipa:gsm_abis_rsl` / `eth:ethertype:ip:tcp:gsm_ipa:gsm_abis_rsl`
- Sigmatek only: Wireshark heuristic dissector applied `gsm_ipa:gsm_abis_rsl`

## Command Frame Compare (64B payload)
- Elmo cmd id: `0x20A0`
- Sigmatek cmd id: `0x209F`
- AxisRef / PayloadLen: `0` / `56` vs `0` / `56`
- Elmo (double decode) d1..d5: `8388608`, `48933546`, `4893354600`, `4893354600`, `48933546000`
- Sigmatek (int64 decode) i1..i5: `0`, `500000`, `500000`, `500000`, `500000`
- dir/buf/exe: Elmo `2/1/1` vs Sigmatek `0/0/1`

## Response Frame Compare (16B payload)
- Elmo: len `8`, handle `1182672`, status `0`, err `0`
- Sigmatek: len `8`, handle `759984`, status `0`, err `0`

## Timing
- Elmo command->response: `0.1494 ms`
- Sigmatek command->response: `1.8936 ms`

## Generated Diff Files
- `MoveAbsoluteEx_Elmo_vs_Sigmatek_FrameFields.csv`
- `MoveAbsoluteEx_Elmo_vs_Sigmatek_CommandByteDiff.csv`
- `MoveAbsoluteEx_Elmo_vs_Sigmatek_ResponseByteDiff.csv`
