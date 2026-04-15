# ReadActualPosition Latency Comparison (XLSX vs PCAP)

## Data Sources
- `CycleTestResult_20260415_141925.xlsx` and `CycleTestResult_20260415_141925.pcapng`
- `CycleTestResult_20260415_142121.xlsx` and `CycleTestResult_20260415_142121.pcapng`

## Method
- Outlier rule: keep only samples with latency `< 2.0 ms`.
- XLSX source: `PositionSamples` sheet, `ReadLatency(ms)` column.
- PCAP source: request/response round-trip time (RTT) for `ReadActualPosition` (`2E 20`), matched by packet order.

## Result 1: Independent <2 ms Averages
### 141925
- XLSX avg: `1.092706 ms` (n=22041/25313)
- PCAP avg: `1.053200 ms` (n=22542/25313)
- Difference (XLSX - PCAP): `0.039506 ms`

### 142121
- XLSX avg: `1.106631 ms` (n=21728/25062)
- PCAP avg: `1.066541 ms` (n=22237/25062)
- Difference (XLSX - PCAP): `0.040089 ms`

### Overall
- XLSX avg: `1.099618 ms` (n=43769)
- PCAP avg: `1.059825 ms` (n=44779)
- Difference (XLSX - PCAP): `0.039793 ms`

## Result 2: 1:1 Aligned Pair Comparison (Both <2 ms)
### 141925
- Total aligned pairs: `25313`, both <2ms: `22041`
- XLSX avg: `1.092706 ms`
- PCAP avg: `1.032852 ms`
- Difference (XLSX - PCAP): `0.059854 ms`

### 142121
- Total aligned pairs: `25062`, both <2ms: `21728`
- XLSX avg: `1.106631 ms`
- PCAP avg: `1.045941 ms`
- Difference (XLSX - PCAP): `0.060689 ms`

## Conclusion
- LASAL app-side measured latency is higher than wire RTT by about `0.04 to 0.06 ms`.
- This is expected because app timing includes software overhead (thread scheduling, API call and parsing), while PCAP RTT reflects network timing only.
