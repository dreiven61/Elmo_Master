from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
LOCALIZATION = ROOT / "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/UiLocalization.cs"
TEST = ROOT / "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/RecentRecoveryPanelLocalizationTests.cs"

OLD_EN = (
    "Software targets are limited to PP(1), PV(3), IP(7), and CSP(8). "
    "The selector stays empty until the connected PLC advertises a supported-mode mask. "
    "Homing(6) remains owned by HomeDS402/HomeDS402Ex."
)
NEW_EN = (
    "Software targets are limited to PP(1), PV(3), IP(7), and CSP(8). "
    "The selector remains usable for qualification, but Start stays disabled unless the connected PLC advertises the selected mode. "
    "Homing(6) remains owned by HomeDS402/HomeDS402Ex."
)
OLD_KO = (
    "소프트웨어 target은 PP(1), PV(3), IP(7), CSP(8)로 제한됩니다. "
    "연결된 PLC가 supported-mode mask를 광고하기 전까지 selector는 비어 있습니다. "
    "Homing(6)은 HomeDS402/HomeDS402Ex가 계속 소유합니다."
)
NEW_KO = (
    "소프트웨어 target은 PP(1), PV(3), IP(7), CSP(8)로 제한됩니다. "
    "qualification 중에는 selector를 계속 사용할 수 있지만, 연결된 PLC가 선택한 mode를 광고하지 않으면 Start는 비활성 상태를 유지합니다. "
    "Homing(6)은 HomeDS402/HomeDS402Ex가 계속 소유합니다."
)


def read_normalized(path: Path):
    raw = path.read_bytes().decode("utf-8")
    newline = "\r\n" if "\r\n" in raw else "\n"
    return raw.replace("\r\n", "\n").replace("\r", "\n"), newline


def write_preserving(path: Path, text: str, newline: str):
    path.write_bytes(text.replace("\n", newline).encode("utf-8"))


localization, localization_nl = read_normalized(LOCALIZATION)
test, test_nl = read_normalized(TEST)

if localization.count(OLD_EN) != 1:
    raise SystemExit(
        f"expected old SetOperationMode localization key once, found {localization.count(OLD_EN)}"
    )
if localization.count(OLD_KO) != 1:
    raise SystemExit(
        f"expected old SetOperationMode Korean translation once, found {localization.count(OLD_KO)}"
    )
localization = localization.replace(OLD_EN, NEW_EN, 1)
localization = localization.replace(OLD_KO, NEW_KO, 1)

if test.count(OLD_EN) != 1:
    raise SystemExit(
        f"expected old English round-trip assertion once, found {test.count(OLD_EN)}"
    )
if test.count(OLD_KO) != 2:
    raise SystemExit(
        f"expected old Korean round-trip assertion twice, found {test.count(OLD_KO)}"
    )
test = test.replace(OLD_EN, NEW_EN, 1)
test = test.replace(OLD_KO, NEW_KO, 2)

if localization.count(NEW_EN) != 1 or localization.count(NEW_KO) != 1:
    raise SystemExit("new SetOperationMode localization mapping is not exact")
if test.count(NEW_EN) != 1 or test.count(NEW_KO) != 2:
    raise SystemExit("new SetOperationMode localization round-trip assertions are not exact")

write_preserving(LOCALIZATION, localization, localization_nl)
write_preserving(TEST, test, test_nl)
print("MODE-11F localization promotion patch applied")
