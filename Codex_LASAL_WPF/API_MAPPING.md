# LASAL TCP/IP WPF Test App (Dummy)

- Solution: `C:\work\Elmo\Elmo_Master\Codex_LASAL_WPF\PmasApiWpfTestApp.sln`
- Project: `C:\work\Elmo\Elmo_Master\Codex_LASAL_WPF\PmasApiWpfTestApp\PmasApiWpfTestApp.csproj`
- Target: `.NET Framework 4.8`, `WPF`, `Visual Studio 2019`
- Output: `C:\work\Elmo\Elmo_Master\Codex_LASAL_WPF\PmasApiWpfTestApp\bin\Debug\PmasApiWpfTestApp.exe`
- Backend: `SIGMATEK TCP/IP` simulation dummy (no Elmo MMCLibDotNET external DLL dependency)

## Implemented Areas

- Connectivity
  - RPC connection open/close
  - axis/group load by name
  - error description lookup
  - UDP callback channel status inspection
- Single axis
  - power on/off, reset, stop
  - read/write parameter
  - read bool parameter
  - change DS402 operation mode
  - move absolute/relative/velocity
  - override
  - read actual position, status, status register
  - SDO read/write
  - Home DS402 Ex
- Group
  - read status, enable, disable, reset, stop
  - read group status register
  - get group members info
  - move linear absolute / relative / absolute ex
  - set Cartesian kinematic transform
  - wait-until-condition
- PI / Bulk
  - PI info by alias
  - PI USHORT read/write
  - bulk read configure / execute
- Recorder
  - begin recording
  - recorder status
  - stop recording
  - upload header
  - upload data

## Notes

- `MMC_SetPositionCmd`
  - This command remains UI-visible for parity, but is not executed in the dummy backend.
  - The WPF app keeps this item visible and logs the limitation instead of pretending to call it.

- `MMC_OpenUdpChannelCmdEx`
  - Dummy backend assigns callback UDP info during `ConnectRPC(...)`.
  - The app exposes this item as a diagnostic check of current UDP callback state and listener port.

## Verification

- `devenv.com C:\work\Elmo\Elmo_Master\Codex_LASAL_WPF\PmasApiWpfTestApp.sln /Build "Debug|Any CPU"`
