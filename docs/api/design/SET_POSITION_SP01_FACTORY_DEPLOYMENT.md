# SetPosition SP-01 Factory Deployment Receipt Gate

- scope: SP-01B host-side factory provenance / upload-readback identity gate
- transport: LASAL CLASS 2 `Debug -> File Transfer`
- target files: `C:\LMCSP_A.BIN`, `C:\LMCSP_B.BIN`
- exact file size: 2,048 bytes each
- production SetPosition activation: **OFF**
- vendor `CheckSum.CRC32` semantic qualification: **external prerequisite / issue #44**
- LASAL `_FileSys` generated class/client ABI: **external prerequisite / issue #44**

This tooling does not create a production-valid SetPosition image. It starts only after an approved factory-empty
bundle and inventory `FactoryNew` receipt already exist. `Generate-LmcSetPositionStoreImages.ps1` remains blocked
until the vendor CRC golden fixture is captured and reviewed.

## 1. Manifest contract

The approved bundle directory contains `manifest.json`, `LMCSP_A.BIN`, and `LMCSP_B.BIN`.
The host tooling freezes the manifest JSON fields and order as:

```json
{
  "ManifestSchema": 1,
  "ControllerSerial": "<inventory serial>",
  "SourceRevision": "<40-hex git revision>",
  "ImageSchema": 1,
  "ImageAFileName": "LMCSP_A.BIN",
  "ImageABytes": 2048,
  "ImageASha256": "<64-hex>",
  "ImageBFileName": "LMCSP_B.BIN",
  "ImageBBytes": 2048,
  "ImageBSha256": "<64-hex>"
}
```

For the generation-1 empty factory image, A and B must be byte-for-byte identical and therefore have the same
SHA-256. These checks prove bundle identity only; the host gate intentionally does not guess or reimplement the
vendor CRC algorithm.

## 2. Receipt chain

Receipts are stored outside the controller at:

`<ReceiptRoot>/<ControllerSerial>/deployment_receipts.jsonl`

Each line is canonical UTF-8 without BOM, LF-terminated, with the exact field order:

`ReceiptSchema, ControllerSerial, State, SourceRevision, ImageSchema, ImageASha256, ImageBSha256, StopEvidenceSha256, PreviousReceiptSha256, Utc, OperatorId`

`PreviousReceiptSha256` is SHA-256 of the previous canonical JSON record bytes, excluding the LF. The first
`FactoryNew` record uses 64 zero characters. The supported monotonic chain is:

`FactoryNew -> FactoryInstallStarted -> VerifiedFactoryEmpty -> ActivationAuthorized -> Activated`

The SP-01 factory tools implemented here only append through `VerifiedFactoryEmpty`. They refuse to overwrite,
truncate, reorder, duplicate, or skip states. Existing successful deployment/activation history blocks a new empty
factory install.

The manufacturing inventory system must issue the first `FactoryNew` record. The application and these deployment
tools must not infer factory-new state from missing controller files.

## 3. Start gate

With the PLC application **STOPPED** and the project **unloaded**, preserve a non-empty screenshot/export/file as
STOP evidence and run:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Start-LmcSetPositionStoreDeployment.ps1 `
  -Manifest '<bundle>\manifest.json' `
  -ControllerSerial '<serial>' `
  -StopEvidence '<stopped-unloaded evidence>' `
  -ReceiptRoot '<manufacturing evidence root>' `
  -OperatorId '<operator>'
```

The start gate verifies:

- exact manifest schema/serial/source revision;
- exact 2,048-byte A/B bundle files and SHA-256;
- byte-for-byte identical generation-1 empty bundle images;
- existing canonical receipt hash chain;
- exact `FactoryNew` predecessor and no prior successful deployment/activation;
- non-empty STOP/unload evidence and its SHA-256.

It then appends one `FactoryInstallStarted` record. Re-running with the exact same manifest and STOP evidence while
the chain is exactly `FactoryNew -> FactoryInstallStarted` is idempotent and does not append another record.

The tool performs **no PLC upload**. Use LASAL CLASS 2 `Debug -> File Transfer` manually to place the two files on
the controller. Until File Transfer automation is separately qualified, this process must not be described as
one-click or atomic deployment.

## 4. Readback verification gate

After upload, while the application remains stopped/unloaded, download both controller files back to the PC and
run:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Verify-LmcSetPositionStoreDeployment.ps1 `
  -Manifest '<bundle>\manifest.json' `
  -ReadbackA '<downloaded LMCSP_A.BIN>' `
  -ReadbackB '<downloaded LMCSP_B.BIN>' `
  -ControllerSerial '<serial>' `
  -StopEvidence '<same stopped-unloaded evidence>' `
  -ReceiptRoot '<manufacturing evidence root>' `
  -OperatorId '<operator>'
```

Verification requires the exact two-record predecessor chain and proves:

- readback A/B are each exactly 2,048 bytes;
- each SHA-256 equals the manifest;
- A and B are byte-for-byte identical;
- manifest/controller/source/image identity matches both prior receipts;
- STOP evidence matches `FactoryInstallStarted`;
- receipt chain hashes and canonical bytes are intact.

Only then is `VerifiedFactoryEmpty` appended. A duplicate verification attempt is rejected rather than creating a
second success record.

## 5. Deliberate non-claims

A green host receipt workflow or a `VerifiedFactoryEmpty` record does **not** prove:

- `CheckSum.CRC32` internal header/body compatibility;
- generation/marker parsing by real `_FileSys` runtime;
- C78/generated artifact freshness;
- cold power-cycle durability;
- Store async request-completion ABI;
- RT claim/native exactly-once execution;
- SetPosition hardware effect;
- Admin bits 3/5/7 or any production activation.

Issue #44 remains mandatory for vendor CRC semantics and LASAL IDE-generated `_FileSys` ABI. After those are
available, SP-01B/01C can attach the runtime backend to this already-frozen manufacturing provenance chain.

## 6. CI self-test boundary

The workflow uses synthetic 2,048-byte identical files only to test manifest/receipt/hash-chain behavior. Those
fixtures are deliberately **not** called valid SetPosition store images and are not CRC qualification evidence.
Negative self-tests reject duplicate verification and canonical receipt tampering.
