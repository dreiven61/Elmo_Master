# ReadActualPosition Latency Comparison (XLSX vs PCAP)

## Data Sources
- `C:\work\Elmo\Elmo_Master\Codex_LASAL_WPF\Reports\CycleResults\CycleTestResult_20260416_084834.xlsx`
- `C:\work\Elmo\Elmo_Master\Codex_LASAL_WPF\Reports\CycleResults\CycleTestResult_20260416_084834.pcapng`

## Method
- XLSX source: `PositionSamples` sheet, `ReadLatency(ms)` column.
- PCAP source: packets between `192.168.1.13` and `192.168.1.3`.
  - Request signature: `2E 20` (`ReadActualPosition`)
  - Response signature: `E0 00`
- Pairing rule used for this capture: **adjacent `req -> rsp`** in packet order.
  - Reason: this pcap includes repeated `RR` bursts (`req, req, rsp`), so FIFO index matching drifts and produces false RTT inflation.
- Warm-up handling: skip first `2337` PCAP pairs so PCAP pair count matches XLSX sample count (`11547`).
- Outlier filter for comparison: latency `< 2.0 ms`.

## Packet Pattern Check
- Stream transitions: `RS=13884`, `SR=13884`, `RR=119`
- Extracted adjacent req->rsp pairs: `13884`
- Trimmed pairs used for comparison: `11547`

## Result 1: Independent Averages
- XLSX avg (all): `1.554167 ms`
- PCAP avg (all, trimmed): `1.482244 ms`
- Difference (XLSX - PCAP, all): `0.071923 ms`

- XLSX avg (<2ms): `1.018326 ms` (n=10524/11547)
- PCAP avg (<2ms): `0.961102 ms` (n=10585/11547)
- Difference (XLSX - PCAP, <2ms): `0.057224 ms`

## Result 2: 1:1 Aligned Pair Comparison (Both <2 ms)
- Aligned pairs: `11547`
- Both <2ms pairs: `9667`
- XLSX avg (both<2): `1.018846 ms`
- PCAP avg (both<2): `0.960909 ms`
- Difference (XLSX - PCAP, both<2): `0.057937 ms`

## Conclusion
- With robust packet pairing, app-side latency is consistently higher than wire RTT by about **`0.058 ms`** in the `<2ms` region.
- This is in the same direction as previous analyses (app includes software overhead).
- Previous FIFO result (`~152 ms`) is invalid for this file due to `RR` insertion and resulting index drift.
