from pathlib import Path

root = Path(__file__).resolve().parents[1]
path = root / 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/AdminSetOperationModeContractTests.cs'
text = path.read_text(encoding='utf-8')
old = '''                    CapabilitiesPayload(\n                        OriginalRequestId,\n                        CapabilityTriad,\n                        6)),\n'''
new = '''                    CapabilitiesPayload(\n                        OriginalRequestId,\n                        CapabilityTriad,\n                        6,\n                        0x018A)),\n'''
count = text.count(old)
if count != 1:
    raise RuntimeError(f'expected one full-triad success fixture, found {count}')
path.write_text(text.replace(old, new, 1), encoding='utf-8', newline='')
print('SetOperationMode full-triad fixture now carries SupportedModeMask 0x018A.')
