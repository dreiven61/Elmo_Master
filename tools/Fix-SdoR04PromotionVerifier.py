from pathlib import Path

path = Path(__file__).with_name('Verify-SdoR04RequestPreview.ps1')
text = path.read_text(encoding='utf-8')
old = 'throw "Missing $Label: $Needle"'
new = 'throw "Missing ${Label}: $Needle"'
if text.count(old) != 1:
    raise RuntimeError('expected one PowerShell interpolation anchor')
path.write_text(text.replace(old, new, 1), encoding='utf-8')
print('SDO-R04 verifier normalized for Windows PowerShell 5.1.')
