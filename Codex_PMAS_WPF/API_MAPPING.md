# P-MAS API WPF Test App

- Solution: `C:\work\Elmo\Elmo_Master\Codex_PMAS_WPF\PmasApiWpfTestApp.sln`
- Project: `C:\work\Elmo\Elmo_Master\Codex_PMAS_WPF\PmasApiWpfTestApp\PmasApiWpfTestApp.csproj`
- Target: `.NET Framework 4.8`, `WPF`, `Visual Studio 2019`
- Output: `C:\work\Elmo\Elmo_Master\Codex_PMAS_WPF\PmasApiWpfTestApp\bin\Debug\net48\PmasApiWpfTestApp.exe`

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

## Honest Gaps

- `MMC_SetPositionCmd`
  - `MMCLibDotNET v3.0.0.7` public wrapper does not expose a single-axis `SetPosition(...)` API.
  - The WPF app keeps this item visible and logs the limitation instead of pretending to call it.

- `MMC_OpenUdpChannelCmdEx`
  - In this .NET wrapper, the UDP callback port is effectively assigned during `MMCConnection.ConnectRPC(...)`.
  - The app exposes this item as a diagnostic check of current UDP callback state and listener port.

## Verification

- `dotnet build C:\work\Elmo\Elmo_Master\Codex_PMAS_WPF\PmasApiWpfTestApp.sln -c Debug`
- build result: success
- startup smoke test: success
