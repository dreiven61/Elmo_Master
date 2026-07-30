ParaValue       = 1           // socket 1..4
Timeout         = 1000
ParaReadWrite   = 1           // 마지막에 1회
```

`EtherCAT_SDOBase`는 설정한 `ParaLength` 그대로 `StartWriteSDO`를 실행하므로 2바이트와 4바이트 모두 가능합니다. [Write 구현](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/EtherCAT_SDOBase/EtherCAT_SDOBase.st:278)

### 4. 결과 확인

```text
ClassState: BUSY -> READY   성공
ClassState: ERROR           실패, ErrorCode 확인
```

`READY`는 반드시 callback 완료 후의 상태를 확인해야 합니다. Write 완료 전에는 Index, Length, Value를 바꾸면 안 됩니다.

성공 후 DS402 Fault Reset을 실행합니다.

- LASAL 로직: `LMCAxis1.QuitError()`
- Online 수동 조작: `Elmo_11.AxErrorQuit = 1` 한 번 Write

이 동작은 `0x6040` bit 7 Fault Reset을 발생시킵니다. [ECAT_DS402Base.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/ECAT_DS402Base/ECAT_DS402Base.st:650)

마지막으로 확인할 값은 다음입니다.

```text
Elmo_11.StateWord.Fault = 0
Elmo_11.AxError         = 0
절대 위치가 실제 기계 위치와 일치
```

주의할 점:

- `ParaReadWrite=0`으로 되돌리면 Write-only 객체에 Read를 실행하므로 되돌리지 않습니다.
- `ParaReadWrite=1`을 Force하거나 cyclic하게 계속 쓰면 안 됩니다.
- Startup SDO로 등록하면 재부팅마다 실행되므로 사용하면 안 됩니다.
- 같은 드라이브의 `LMCSdoExecutor`와 동시에 SDO를 실행하면 BUSY가 발생할 수 있습니다.
- 상시 기능으로 사용할 경우에는 디버그 객체를 두기보다 기존 `LMCSdoExecutor`에 `TryStartEncoderReset` 전용 메서드를 추가하는 것이 맞습니다.
