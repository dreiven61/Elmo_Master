from pathlib import Path
import importlib.util
import re

PATCHER = Path(__file__).with_name("apply_dynamic_group_connected_axis.py")
spec = importlib.util.spec_from_file_location("dynamic_group_patcher", PATCHER)
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)


def scoped_replace_block(text: str, start_hex: str, next_hex: str, replacement: str, label: str) -> str:
    function_anchor = "FUNCTION LMCControlCommandService::HandleGroupCommands\n"
    function_start = text.find(function_anchor)
    if function_start < 0:
        raise SystemExit("HandleGroupCommands function not found")
    function_end = text.find("END_FUNCTION", function_start)
    if function_end < 0:
        raise SystemExit("HandleGroupCommands END_FUNCTION not found")
    function_end += len("END_FUNCTION")

    prefix = text[:function_start]
    body = text[function_start:function_end]
    suffix = text[function_end:]

    pattern = re.compile(rf"\n\t\t0x{start_hex}:.*?\n\t\t0x{next_hex}:", re.S)
    body, count = pattern.subn(replacement, body, count=1)
    if count != 1:
        raise SystemExit(f"{label}: expected 1 HandleGroupCommands block, found {count}")
    return prefix + body + suffix


module.replace_block = scoped_replace_block
module.main()
