from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def replace_exact(path, old, new, expected=1):
    file_path = ROOT / path
    raw = file_path.read_bytes()
    text = raw.decode("utf-8")
    newline = "\r\n" if "\r\n" in text else "\n"
    normalized = text.replace("\r\n", "\n").replace("\r", "\n")
    old_n = old.replace("\r\n", "\n").replace("\r", "\n")
    new_n = new.replace("\r\n", "\n").replace("\r", "\n")
    actual = normalized.count(old_n)
    if actual != expected:
        raise RuntimeError(
            f"replacement count mismatch: {path}: expected={expected}, actual={actual}, old={old!r}"
        )
    normalized = normalized.replace(old_n, new_n)
    if newline == "\r\n":
        normalized = normalized.replace("\n", "\r\n")
    file_path.write_bytes(normalized.encode("utf-8"))


catalog = "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/UiLocalization.cs"
test = "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/RecentRecoveryPanelLocalizationTests.cs"

# Add translations for every operator-facing string introduced by the
# SetOperationMode multi-mode software promotion. Keep historical CSP entries
# intact because old durable UI states may still be translated during recovery.
replace_exact(
    catalog,
    "            AddStaticChromeTranslations(values);",
    "            values[\"Set Operation Mode - software target / durable no-replay recovery\"] =\n"
    "                \"Operation Mode 설정 - 소프트웨어 target / durable 재전송 방지 복구\";\n"
    "            values[\"PP(1)/PV(3)/IP(7)/CSP(8) software targets are implemented. Production Start remains disabled until PLC capability and hardware qualification are complete. Homing(6) remains unavailable here.\"] =\n"
    "                \"PP(1)/PV(3)/IP(7)/CSP(8) 소프트웨어 target이 구현되어 있습니다. PLC capability와 hardware qualification이 완료될 때까지 Production Start는 비활성화됩니다. Homing(6)은 여기서 사용할 수 없습니다.\";\n"
    "            values[\"I verified the exact drive/axis and understand that this may write DS402 0x6060:0 to the selected mode once only. If the response or completion is uncertain I will use the durable recovery query and will not send Start again.\"] =\n"
    "                \"정확한 drive/축을 확인했으며 선택한 mode를 DS402 0x6060:0에 한 번만 쓸 수 있음을 이해했습니다. 응답 또는 완료가 불확실하면 durable 복구 조회만 사용하고 Start를 다시 전송하지 않습니다.\";\n"
    "            values[\"Start Selected Mode Once (0x7D23)\"] =\n"
    "                \"선택 Mode 1회 시작 (0x7D23)\";\n"
    "            values[\"Set Operation Mode Selected Mode Once\"] =\n"
    "                \"Operation Mode 선택 Mode 1회 설정\";\n\n"
    "            AddStaticChromeTranslations(values);",
)

# The localization integration test must follow the new multi-mode operator
# semantics instead of requiring the removed CSP-only TextBlock.
replace_exact(
    test,
    '                    "Operation Mode 설정 - CSP=8 / durable 재전송 방지 복구",',
    '                    "Operation Mode 설정 - 소프트웨어 target / durable 재전송 방지 복구",',
)
replace_exact(
    test,
    "                AssertContains(\n"
    "                    setOperationModeText,\n"
    "                    \"CSP 위치 동기 모드 (8)\",\n"
    "                    \"The dynamically created SetOperationMode CSP label was not found in Korean UI.\");",
    "                AssertContains(\n"
    "                    setOperationModeText,\n"
    "                    \"PP(1)/PV(3)/IP(7)/CSP(8) 소프트웨어 target이 구현되어 있습니다.\",\n"
    "                    \"The dynamically created SetOperationMode multi-mode warning was not found in Korean UI.\");",
    expected=2,
)
replace_exact(
    test,
    '                    "Set Operation Mode - CSP=8 / durable no-replay recovery",',
    '                    "Set Operation Mode - software target / durable no-replay recovery",',
)
replace_exact(
    test,
    "                AssertContains(\n"
    "                    setOperationModeText,\n"
    "                    \"CyclicSynchronousPosition (8)\",\n"
    "                    \"English restore did not recover the SetOperationMode CSP label.\");",
    "                AssertContains(\n"
    "                    setOperationModeText,\n"
    "                    \"PP(1)/PV(3)/IP(7)/CSP(8) software targets are implemented.\",\n"
    "                    \"English restore did not recover the SetOperationMode multi-mode warning.\");",
)

print("SetOperationMode multi-mode localization contract updated.")
