# Elmo vs Sigmatek Motion Profile Analysis

- GeneratedAt: 2026-04-14 17:43:34
- Elmo CSV: `elmo_8388608_48933546_4893354600_4833546000_1165.csv`
- Sigmatek CSV: `Sigmatek_360_2100_210000_210000_2100_0.05.csv`
- SetPosition mapping: Elmo=Signals Data Group 1 column 2, Sigmatek=SetPos
- Target: 8388608 cnt
- Reach window: SetPos <= 8 cnt to SetPos >= 8388608 cnt
- Act reach threshold: 8380219 cnt (99.9% of target)

## Summary
- SetPos reach time: ELMO avg=0.233000s, SIGMATEK avg=0.233000s
- ActPos reach time: ELMO avg=0.271750s, SIGMATEK avg=0.269500s
- Peak velocity (cnt/s): ELMO avg=48933500, SIGMATEK avg=48934000
- Peak acceleration (cnt/s^2): ELMO avg=1511500000, SIGMATEK avg=1511250000
- Peak deceleration (cnt/s^2): ELMO avg=1514500000, SIGMATEK avg=1514750000
- Peak jerk (cnt/s^3): ELMO avg=61250000000, SIGMATEK avg=61250000000

## Interpretation
- Both datasets show almost identical SetPosition profile timing and shape.
- The 0->8388608 SetPosition reach time is consistently about 233 ms for both.
- Velocity/acceleration/deceleration/jerk inferred from SetPosition derivatives are nearly the same.
- Remaining differences are at the level of 1 ms sampling quantization and minor cycle alignment offsets.

## Files
- `Elmo_vs_Sigmatek_Profile_Analysis.csv` (detailed numeric comparison + per-cycle data)
- `Elmo_vs_Sigmatek_Profile_Analysis.md` (this summary)