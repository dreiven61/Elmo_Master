from pathlib import Path


def load(path):
    raw = Path(path).read_bytes().decode("utf-8")
    newline = "\r\n" if "\r\n" in raw else "\n"
    return raw.replace("\r\n", "\n"), newline


def save(path, text, newline):
    Path(path).write_bytes(text.replace("\n", newline).encode("utf-8"))


def replace_once(path, old, new):
    text, nl = load(path)
    count = text.count(old)
    if count != 1:
        raise RuntimeError(
            f"{path}: expected one match, got {count}: {old[:100]!r}")
    text = text.replace(old, new, 1)
    save(path, text, nl)


def insert_after_once(path, marker, addition):
    text, nl = load(path)
    count = text.count(marker)
    if count != 1:
        raise RuntimeError(
            f"{path}: expected one marker, got {count}: {marker!r}")
    text = text.replace(marker, marker + addition, 1)
    save(path, text, nl)


def replace_between_once(path, start_marker, end_marker, replacement):
    text, nl = load(path)
    start = text.find(start_marker)
    if start < 0:
        raise RuntimeError(f"{path}: start marker missing: {start_marker!r}")
    end = text.find(end_marker, start + len(start_marker))
    if end < 0:
        raise RuntimeError(f"{path}: end marker missing: {end_marker!r}")
    if text.find(start_marker, start + len(start_marker)) >= 0:
        raise RuntimeError(f"{path}: start marker is not unique")
    text = text[:start] + replacement + text[end:]
    save(path, text, nl)


latch = (
    "Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/"
    "LMCEcatInputLatch/LMCEcatInputLatch.st"
)
control = (
    "Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/"
    "LMCControlCommandService/LMCControlCommandService.st"
)
diag = (
    "Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/"
    "LMCDiagnosticsService/LMCDiagnosticsService.st"
)
tcp = (
    "Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/"
    "TCPMotionInterface/TCPMotionInterface.st"
)
diag_models = "LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsModels.cs"
enc_models = (
    "LMC_Library/LMC_API_Delivery/src/"
    "LmcDiagnosticsEncoderMaintenanceModels.cs"
)

# Current hardware profile: Axis1/2 are physical Elmo drives; Axis3/4 are
# retained as logical simulation axes. Keep this explicit and shared by the
# startup admission layers rather than treating every logical axis as a
# mandatory physical EtherCAT drive.
insert_after_once(
    latch,
    "#define LMC_OWNER_STARTUP_SNAPSHOT_MAGIC  0x4C4D4353\n",
    "#define LMC_CONFIGURED_PHYSICAL_DRIVE_MASK 0x00000003\n",
)

latch_block = """\townershipLatchDrainFlags := 0;
\tif IsClientConnected(#EcatMaster) &
\t\t(masterState = 8) & (consecutiveInvalidCycles = 0) &
\t\t(((LMC_CONFIGURED_PHYSICAL_DRIVE_MASK and 0x00000001) = 0) |
\t\t (IsClientConnected(#Drive1) & IsClientConnected(#LMCAxis1) &
\t\t  (SnapshotBytes[64]$DINT <> 0) &
\t\t  (SnapshotBytes[68]$UDINT = 8) &
\t\t  (SnapshotBytes[76]$UDINT = 0) &
\t\t  (SnapshotBytes[92]$UDINT = cycleCounter))) &
\t\t(((LMC_CONFIGURED_PHYSICAL_DRIVE_MASK and 0x00000002) = 0) |
\t\t (IsClientConnected(#Drive2) & IsClientConnected(#LMCAxis2) &
\t\t  (SnapshotBytes[100]$DINT <> 0) &
\t\t  (SnapshotBytes[104]$UDINT = 8) &
\t\t  (SnapshotBytes[112]$UDINT = 0) &
\t\t  (SnapshotBytes[128]$UDINT = cycleCounter))) &
\t\t(((LMC_CONFIGURED_PHYSICAL_DRIVE_MASK and 0x00000004) = 0) |
\t\t (IsClientConnected(#Drive3) & IsClientConnected(#LMCAxis3) &
\t\t  (SnapshotBytes[136]$DINT <> 0) &
\t\t  (SnapshotBytes[140]$UDINT = 8) &
\t\t  (SnapshotBytes[148]$UDINT = 0) &
\t\t  (SnapshotBytes[164]$UDINT = cycleCounter))) &
\t\t(((LMC_CONFIGURED_PHYSICAL_DRIVE_MASK and 0x00000008) = 0) |
\t\t (IsClientConnected(#Drive4) & IsClientConnected(#LMCAxis4) &
\t\t  (SnapshotBytes[172]$DINT <> 0) &
\t\t  (SnapshotBytes[176]$UDINT = 8) &
\t\t  (SnapshotBytes[184]$UDINT = 0) &
\t\t  (SnapshotBytes[200]$UDINT = cycleCounter))) then
\t\townershipLatchDrainFlags := ownershipLatchDrainFlags or
\t\t\tLMC_OWNER_STARTUP_LATCH_PHYSICAL;
\tend_if;
"""
replace_between_once(
    latch,
    "\townershipLatchDrainFlags := 0;",
    "\n\tif zeroHomeRequestSequence = zeroHomeAppliedSequence then",
    latch_block,
)

insert_after_once(
    control,
    "#define LMC_OWNER_STARTUP_SNAPSHOT_MAGIC 0x4C4D4353\n",
    "#define LMC_OWNER_CONFIGURED_PHYSICAL_AXIS_MASK 0x00000003\n",
)

physical_idle = """\tphysicalIdle :=
\t\t((startupSnapshot[10] and LMC_OWNER_STARTUP_LATCH_PHYSICAL) <> 0) &
\t\t(((LMC_OWNER_CONFIGURED_PHYSICAL_AXIS_MASK and 0x00000001) = 0) |
\t\t (((startupSnapshot[2] and LMC_AXIS_STATUS_STANDSTILL) <> 0) &
\t\t  ((startupSnapshot[2] and LMC_OWNER_STARTUP_AXIS_CLEAR_MASK) = 0))) &
\t\t(((LMC_OWNER_CONFIGURED_PHYSICAL_AXIS_MASK and 0x00000002) = 0) |
\t\t (((startupSnapshot[3] and LMC_AXIS_STATUS_STANDSTILL) <> 0) &
\t\t  ((startupSnapshot[3] and LMC_OWNER_STARTUP_AXIS_CLEAR_MASK) = 0))) &
\t\t(((LMC_OWNER_CONFIGURED_PHYSICAL_AXIS_MASK and 0x00000004) = 0) |
\t\t (((startupSnapshot[4] and LMC_AXIS_STATUS_STANDSTILL) <> 0) &
\t\t  ((startupSnapshot[4] and LMC_OWNER_STARTUP_AXIS_CLEAR_MASK) = 0))) &
\t\t(((LMC_OWNER_CONFIGURED_PHYSICAL_AXIS_MASK and 0x00000008) = 0) |
\t\t (((startupSnapshot[5] and LMC_AXIS_STATUS_STANDSTILL) <> 0) &
\t\t  ((startupSnapshot[5] and LMC_OWNER_STARTUP_AXIS_CLEAR_MASK) = 0)));
"""
replace_between_once(
    control,
    "\tphysicalIdle :=",
    "\n\n\tgroupIdle := FALSE;",
    physical_idle,
)

insert_after_once(
    diag,
    "#define LMC_OWNER_STARTUP_DIAG_EXECUTOR 0x00000010\n",
    "#define LMC_DIAG_CONFIGURED_PHYSICAL_DRIVE_MASK 0x00000003\n",
)

executor_block = """\texecutorsReusable := TRUE;
\tif (LMC_DIAG_CONFIGURED_PHYSICAL_DRIVE_MASK and 0x00000001) <> 0 then
\t\texecutorsReusable := IsClientConnected(#SdoAxis1) <> 0;
\t\tif executorsReusable then
\t\t\texecutorsReusable := SdoAxis1.IsReusable();
\t\tend_if;
\tend_if;
\tif executorsReusable &
\t   ((LMC_DIAG_CONFIGURED_PHYSICAL_DRIVE_MASK and 0x00000002) <> 0) then
\t\texecutorsReusable := IsClientConnected(#SdoAxis2) <> 0;
\t\tif executorsReusable then
\t\t\texecutorsReusable := SdoAxis2.IsReusable();
\t\tend_if;
\tend_if;
\tif executorsReusable &
\t   ((LMC_DIAG_CONFIGURED_PHYSICAL_DRIVE_MASK and 0x00000004) <> 0) then
\t\texecutorsReusable := IsClientConnected(#SdoAxis3) <> 0;
\t\tif executorsReusable then
\t\t\texecutorsReusable := SdoAxis3.IsReusable();
\t\tend_if;
\tend_if;
\tif executorsReusable &
\t   ((LMC_DIAG_CONFIGURED_PHYSICAL_DRIVE_MASK and 0x00000008) <> 0) then
\t\texecutorsReusable := IsClientConnected(#SdoAxis4) <> 0;
\t\tif executorsReusable then
\t\t\texecutorsReusable := SdoAxis4.IsReusable();
\t\tend_if;
\tend_if;
"""
replace_between_once(
    diag,
    "\texecutorsReusable :=",
    "\n\tif executorsReusable then\n"
    "\t\tdrainFlags := drainFlags or LMC_OWNER_STARTUP_DIAG_EXECUTOR;",
    executor_block,
)

insert_after_once(
    diag,
    "#define LMC_DIAG_ENCODER_DETAIL_SEMANTIC 42\n",
    "#define LMC_DIAG_ENCODER_DETAIL_OWNERSHIP_ADMISSION 43\n"
    "#define LMC_DIAG_ENCODER_DETAIL_PHYSICAL_DRIVE_UNAVAILABLE 44\n",
)

replace_once(
    diag,
    "\telsif diagnosticsBuild <> 1 then\n"
    "\t\tdetailCode := LMC_DIAG_ENCODER_DETAIL_COMPATIBILITY;\n",
    "\telsif ((TO_UDINT(1) shl TO_UDINT(driveReference - 1)) and\n"
    "\t       LMC_DIAG_CONFIGURED_PHYSICAL_DRIVE_MASK) = 0 then\n"
    "\t\tdetailCode := "
    "LMC_DIAG_ENCODER_DETAIL_PHYSICAL_DRIVE_UNAVAILABLE;\n"
    "\telsif diagnosticsBuild <> 1 then\n"
    "\t\tdetailCode := LMC_DIAG_ENCODER_DETAIL_COMPATIBILITY;\n",
)

encoder_admission_reject = """
    if (CommandID = 0x7E53) &
       (diagnosticsAdmissionResult <> 0) then
      _memset(dest:=#Sendbuf[8], usByte:=0, cntr:=16);
      Sendbuf[8]$UINT := 1;
      Sendbuf[12]$UINT := 1;
      Sendbuf[14]$INT := -32000;
      Sendbuf[16]$UDINT := RequestBuf[12]$UDINT;
      if diagnosticsAdmissionResult = -2 then
        Sendbuf[20]$UDINT := 41;
      else
        Sendbuf[20]$UDINT := 43;
      end_if;
      diagnosticsResponseSize := 16;
    end_if;

"""
tcp_marker = """    if (diagnosticsDs402StartValid | diagnosticsHomeExStartValid |
        diagnosticsOperationModeStartValid) &
"""
text, nl = load(tcp)
if text.count(tcp_marker) != 1:
    raise RuntimeError("TCP admission failure marker not unique")
text = text.replace(tcp_marker, encoder_admission_reject + tcp_marker, 1)
save(tcp, text, nl)

replace_once(
    tcp,
    "      (diagnosticsOperationModeStartValid = FALSE)) |\n",
    "      (diagnosticsOperationModeStartValid = FALSE) &\n"
    "       ((CommandID <> 0x7E53) | "
    "(diagnosticsAdmissionResult = 0))) |\n",
)

replace_once(
    diag_models,
    "        EncoderMaintenanceSemanticVerificationFailed = 42\n",
    "        EncoderMaintenanceSemanticVerificationFailed = 42,\n"
    "        EncoderMaintenanceOwnershipAdmissionUnavailable = 43,\n"
    "        EncoderMaintenancePhysicalDriveUnavailable = 44\n",
)

replace_once(
    enc_models,
    "        OutcomeSlotOccupied = 41,\n"
    "        SemanticVerificationFailed = 42\n",
    "        OutcomeSlotOccupied = 41,\n"
    "        SemanticVerificationFailed = 42,\n"
    "        OwnershipAdmissionUnavailable = 43,\n"
    "        PhysicalDriveUnavailable = 44\n",
)

replace_once(
    enc_models,
    """            : base(
                "The PLC explicitly rejected the encoder maintenance start command. The one-shot prepared command remains consumed.",
                innerException)
        {
            PreparedCommand = preparedCommand;
            RecoveryKey = preparedCommand.RecoveryKey;
        }
""",
    """            : base(
                BuildRejectedMessage(innerException),
                innerException)
        {
            PreparedCommand = preparedCommand;
            RecoveryKey = preparedCommand.RecoveryKey;
            var diagnosticsException =
                innerException as LMCDiagnosticsCommandException;
            DetailCode = diagnosticsException != null
                && diagnosticsException.Response != null
                    ? diagnosticsException.Response.DetailCode
                    : 0;
        }

        private static string BuildRejectedMessage(Exception innerException)
        {
            var diagnosticsException =
                innerException as LMCDiagnosticsCommandException;
            if (diagnosticsException != null
                && diagnosticsException.Response != null)
            {
                return "The PLC explicitly rejected the encoder maintenance start command. DetailCode="
                    + diagnosticsException.Response.DetailCode
                    + " ("
                    + diagnosticsException.Response.Detail
                    + "). The one-shot prepared command remains consumed.";
            }

            return "The PLC explicitly rejected the encoder maintenance start command. The one-shot prepared command remains consumed.";
        }
""",
)

replace_once(
    enc_models,
    """        public LMCEncoderMaintenanceRecoveryKey RecoveryKey
        {
            get;
            private set;
        }
    }

    public sealed class LMCEncoderMaintenanceOutcomeUncertainException
""",
    """        public LMCEncoderMaintenanceRecoveryKey RecoveryKey
        {
            get;
            private set;
        }
        public uint DetailCode { get; private set; }
        public LMCEncoderMaintenanceDetailCode Detail
        {
            get { return (LMCEncoderMaintenanceDetailCode)DetailCode; }
        }
    }

    public sealed class LMCEncoderMaintenanceOutcomeUncertainException
""",
)

checks = {
    latch: [
        "LMC_CONFIGURED_PHYSICAL_DRIVE_MASK 0x00000003",
        "((LMC_CONFIGURED_PHYSICAL_DRIVE_MASK and 0x00000004) = 0)",
        "((LMC_CONFIGURED_PHYSICAL_DRIVE_MASK and 0x00000008) = 0)",
    ],
    control: [
        "LMC_OWNER_CONFIGURED_PHYSICAL_AXIS_MASK 0x00000003",
        "LMC_OWNER_STARTUP_LATCH_PHYSICAL",
    ],
    diag: [
        "LMC_DIAG_CONFIGURED_PHYSICAL_DRIVE_MASK 0x00000003",
        "LMC_DIAG_ENCODER_DETAIL_OWNERSHIP_ADMISSION 43",
        "LMC_DIAG_ENCODER_DETAIL_PHYSICAL_DRIVE_UNAVAILABLE 44",
    ],
    tcp: [
        "(CommandID = 0x7E53) &",
        "Sendbuf[20]$UDINT := 43;",
        "((CommandID <> 0x7E53) | (diagnosticsAdmissionResult = 0))",
    ],
    diag_models: [
        "EncoderMaintenanceOwnershipAdmissionUnavailable = 43",
        "EncoderMaintenancePhysicalDriveUnavailable = 44",
    ],
    enc_models: [
        "OwnershipAdmissionUnavailable = 43",
        "PhysicalDriveUnavailable = 44",
        "public uint DetailCode { get; private set; }",
    ],
}
for path, needles in checks.items():
    text, _ = load(path)
    for needle in needles:
        if needle not in text:
            raise RuntimeError(
                f"{path}: missing post-patch invariant: {needle}")

print("2-drive encoder maintenance patch assertions passed.")
