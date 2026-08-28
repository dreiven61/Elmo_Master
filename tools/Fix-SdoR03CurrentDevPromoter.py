from pathlib import Path

path = Path(__file__).with_name('Apply-SdoR03CurrentDev.py')
text = path.read_text(encoding='utf-8')
old = r"r'\\(ObjectIndex = 0x6040\\) \\| \\(ObjectIndex = 0x6060\\) \\|\\n\\t\\t\\t\\(ObjectIndex = 0x607A\\) \\| \\(ObjectIndex = 0x60FF\\) \\|\\n\\t\\t\\t\\(ObjectIndex = 0x6071\\)'"
new = r"r'\\(ObjectIndex = 0x6040\\)\\s*\\|\\s*\\(ObjectIndex = 0x6060\\)\\s*\\|\\s*\\(ObjectIndex = 0x607A\\)\\s*\\|\\s*\\(ObjectIndex = 0x60FF\\)\\s*\\|\\s*\\(ObjectIndex = 0x6071\\)'"
if text.count(old) != 1:
    raise RuntimeError('expected one stale PLC semantic matcher')
path.write_text(text.replace(old, new, 1), encoding='utf-8')
print('R03 promoter semantic matcher normalized.')
