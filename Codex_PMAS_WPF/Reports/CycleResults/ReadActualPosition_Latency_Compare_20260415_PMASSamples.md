# ReadActualPosition Latency Comparison (PMAS Samples)

## Data Sources
- `CycleTestResult_20260415_144509.xlsx` and `CycleTestResult_20260415_144509.pcapng`
- `CycleTestResult_20260415_144710.xlsx` and `CycleTestResult_20260415_144710.pcapng`

## Method
- Outlier rule: keep only samples with latency `< 2.0 ms`.
- XLSX source: `PositionSamples` sheet, `ReadLatency(ms)` column.
- PCAP source: request/response round-trip time (RTT) for `ReadActualPosition` (`2E 20`), matched by packet order.

## Result 1: Independent <2 ms Averages
### 144509
- XLSX avg (<2ms): `0.401538 ms` (n=37304/38415)
- PCAP avg (<2ms): `0.338650 ms` (n=37432/38415)
- Difference (XLSX - PCAP): `+0.062887 ms`

### 144710
- XLSX avg (<2ms): `0.456163 ms` (n=35481/36728)
- PCAP avg (<2ms): `0.396262 ms` (n=35626/36728)
- Difference (XLSX - PCAP): `+0.059902 ms`

### Overall
- XLSX avg (<2ms): `0.428166 ms` (n=72785)
- PCAP avg (<2ms): `0.366744 ms` (n=73058)
- Difference (XLSX - PCAP): `+0.061422 ms`

## Result 2: 1:1 Aligned Pair Comparison (Both <2 ms)
### 144509
- Aligned pairs: `38415`, both <2ms: `37304`
- XLSX avg: `0.401538 ms`
- PCAP avg: `0.333338 ms`
- Difference avg: `+0.068200 ms`

### 144710
- Aligned pairs: `36728`, both <2ms: `35481`
- XLSX avg: `0.456163 ms`
- PCAP avg: `0.390260 ms`
- Difference avg: `+0.065903 ms`

## Conclusion
- PMAS app-side measured latency is consistently higher than wire RTT by about `0.06 to 0.07 ms`.
- This gap is expected because app timing includes software overhead (thread scheduling, API call and parsing), while PCAP RTT is pure network-level timing.
