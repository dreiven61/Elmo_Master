using System.Collections.Generic;

namespace LasalMotionControlLib
{
    /// <summary>
    /// Identifies the source contract used to interpret an error value.
    /// Numeric values are not interchangeable between domains.
    /// </summary>
    public enum LMCErrorDomain
    {
        AdapterCommand = 1,
        DiagnosticsDetail = 2,
        GroupProfile = 3,
        AdminDetail = 4
    }

    /// <summary>
    /// Immutable, versioned description of a known project-local error value.
    /// </summary>
    public sealed class LMCErrorDescription
    {
        internal LMCErrorDescription(
            LMCErrorDomain domain,
            long code,
            string symbol,
            string description,
            string resolution,
            uint catalogVersion,
            string sourceVersion)
        {
            Domain = domain;
            Code = code;
            Symbol = symbol;
            Description = description;
            Resolution = resolution;
            CatalogVersion = catalogVersion;
            SourceVersion = sourceVersion;
        }

        public LMCErrorDomain Domain { get; private set; }
        public long Code { get; private set; }
        public string Symbol { get; private set; }
        public string Description { get; private set; }
        public string Resolution { get; private set; }
        public uint CatalogVersion { get; private set; }
        public string SourceVersion { get; private set; }
    }

    /// <summary>
    /// Describes only error contracts shipped with this LASAL adapter.
    /// This is not an Elmo Maestro Personality database.
    /// </summary>
    public static class LMCErrorCatalog
    {
        public const uint CurrentCatalogVersion = 2;

        public const string AdapterSourceVersion =
            "Elmo_Master TCPMotionInterface local errors v2";

        public const string DiagnosticsSourceVersion =
            "Elmo_Master diagnostics schema v1";

        public const string GroupProfileSourceVersion =
            "SIGMATEK MotionLib _LMCProfile 3.00 / _LMCRobotBase 2.22";

        public const string AdminSourceVersion =
            "Elmo_Master LASAL-local admin schema v1";

        private static readonly Dictionary<long, LMCErrorDescription>
            AdapterEntries = CreateAdapterEntries();

        private static readonly Dictionary<long, LMCErrorDescription>
            DiagnosticsEntries = CreateDiagnosticsEntries();

        private static readonly Dictionary<long, LMCErrorDescription>
            GroupProfileEntries = CreateGroupProfileEntries();

        private static readonly Dictionary<long, LMCErrorDescription>
            AdminEntries = CreateAdminEntries();

        /// <summary>
        /// Looks up a description in an explicit error domain. Unknown domains
        /// and values return false and set description to null.
        /// </summary>
        public static bool TryDescribe(
            LMCErrorDomain domain,
            long code,
            out LMCErrorDescription description)
        {
            description = null;

            switch (domain)
            {
                case LMCErrorDomain.AdapterCommand:
                    return AdapterEntries.TryGetValue(code, out description);

                case LMCErrorDomain.DiagnosticsDetail:
                    return DiagnosticsEntries.TryGetValue(code, out description);

                case LMCErrorDomain.GroupProfile:
                    return GroupProfileEntries.TryGetValue(code, out description);

                case LMCErrorDomain.AdminDetail:
                    return AdminEntries.TryGetValue(code, out description);

                default:
                    return false;
            }
        }

        private static Dictionary<long, LMCErrorDescription>
            CreateAdapterEntries()
        {
            var entries = new Dictionary<long, LMCErrorDescription>();

            Add(
                entries,
                LMCErrorDomain.AdapterCommand,
                -1,
                "RpcSessionStateInvalid",
                "The RPC session or connection state is not valid for the command.",
                "Initialize a new RPC session or reconnect before retrying.",
                AdapterSourceVersion);
            Add(
                entries,
                LMCErrorDomain.AdapterCommand,
                -2,
                "ObjectOrClientUnavailable",
                "A name lookup failed or a required LASAL client is not connected.",
                "Verify the object name, axis or group reference, and LASAL client wiring.",
                AdapterSourceVersion);
            Add(
                entries,
                LMCErrorDomain.AdapterCommand,
                -3,
                "MalformedRequest",
                "The command descriptor, payload length, or request shape is invalid.",
                "Use the exact request schema and clear all reserved fields.",
                AdapterSourceVersion);
            Add(
                entries,
                LMCErrorDomain.AdapterCommand,
                -4,
                "UnknownCommand",
                "The connected adapter does not implement the requested command identifier.",
                "Check the connected PLC build and its advertised capabilities before retrying.",
                AdapterSourceVersion);
            Add(
                entries,
                LMCErrorDomain.AdapterCommand,
                -5,
                "PayloadTooLarge",
                "The ingress payload exceeds the adapter request limit.",
                "Reduce the payload to the documented maximum or use a bounded chunk command.",
                AdapterSourceVersion);
            Add(
                entries,
                LMCErrorDomain.AdapterCommand,
                -6,
                "NativeErrorNotRepresentable",
                "A native MotionLib error cannot be represented on the wire or the Robot state is inconsistent.",
                "Read the axis or group status and the PLC-side profile error for the native cause.",
                AdapterSourceVersion);
            Add(
                entries,
                LMCErrorDomain.AdapterCommand,
                -7,
                "UnsupportedArgumentCombination",
                "The request is well formed but its option combination is not supported by this adapter.",
                "Use only the command options documented for the connected adapter build.",
                AdapterSourceVersion);
            Add(
                entries,
                LMCErrorDomain.AdapterCommand,
                -8,
                "QueueOrFramingError",
                "The request queue or transport framing contract was violated.",
                "Reconnect if the session faulted, then retry with one exact framed request.",
                AdapterSourceVersion);
            Add(
                entries,
                LMCErrorDomain.AdapterCommand,
                -9,
                "AxisOwnershipConflict",
                "The requested axes are reserved by another active or retained operation.",
                "Read the current operation outcome, wait for its ownership to retire, then retry once.",
                AdapterSourceVersion);

            return entries;
        }

        private static Dictionary<long, LMCErrorDescription>
            CreateAdminEntries()
        {
            var entries = new Dictionary<long, LMCErrorDescription>();

            AddAdmin(entries, LMCAdminDetailCode.None,
                "No LASAL-local admin detail error is present.",
                "No action is required.");
            AddAdmin(entries, LMCAdminDetailCode.UnsupportedSchema,
                "The requested admin schema version is not supported.",
                "Use the schema version advertised by GetAdminCapabilities.");
            AddAdmin(entries, LMCAdminDetailCode.UnsupportedFlags,
                "The request contains unsupported admin flags.",
                "Clear all reserved flags for admin schema version 1.");
            AddAdmin(entries, LMCAdminDetailCode.InvalidRequestId,
                "The admin request identifier is invalid.",
                "Use a nonzero request identifier and verify its response echo.");
            AddAdmin(entries, LMCAdminDetailCode.InvalidReference,
                "The axis or group reference is outside the admin command contract.",
                "Use physical axis 1 through 4 or main group reference 0x0100 as documented.");
            AddAdmin(entries, LMCAdminDetailCode.InvalidPayloadLength,
                "The admin payload length or reserved field is invalid.",
                "Use the exact command payload shape and clear all reserved fields.");
            AddAdmin(entries, LMCAdminDetailCode.UnsupportedParameter,
                "The requested semantic parameter key is not supported.",
                "Select only a parameter key advertised by the current capabilities.");
            AddAdmin(entries, LMCAdminDetailCode.MissingClient,
                "A required LASAL axis or Robot client is not connected.",
                "Verify the TCPMotionInterface client wiring and object availability.");
            AddAdmin(entries, LMCAdminDetailCode.InvalidSelection,
                "The group parameter selection is empty or contains unsupported bits.",
                "Use a nonzero subset of the advertised group parameter selection mask.");
            AddAdmin(entries, LMCAdminDetailCode.InvalidMotionParameters,
                "The LASAL-local motion request contains an unsupported parameter, range, or execution guard.",
                "Use the command's documented payload, execute confirmation, dynamics, topology, and supported options.");
            AddAdmin(entries, LMCAdminDetailCode.InvalidState,
                "The LASAL motion target is not ready to accept the requested command.",
                "Verify the advertised capability and the applicable axis or group wiring, power, readiness, and ownership state.");
            AddAdmin(entries, LMCAdminDetailCode.NativeCommandRejected,
                "The native LASAL motion command rejected the request.",
                "For group motion, interpret a positive ErrorId in the GroupProfile domain. SetAxisPosition uses ErrorId -6 and preserves the full axis command bitfield in NativeCommandState.");
            AddAdmin(entries, LMCAdminDetailCode.NonZeroVelocity,
                "The axis velocity is not zero, so its application coordinate cannot be reassigned safely.",
                "Stop the axis and verify zero velocity before preparing a new SetAxisPosition intent.");
            AddAdmin(entries, LMCAdminDetailCode.ActiveAxisError,
                "The axis has an active error that blocks application-coordinate reassignment.",
                "Read and clear the axis error, then verify the axis state before preparing a new intent.");
            AddAdmin(entries, LMCAdminDetailCode.InvalidSetPositionSafetyConfiguration,
                "The PLC SetAxisPosition safety configuration is invalid or not enabled.",
                "Correct the PLC-side safety configuration; do not bypass this admission failure in the SDK.");
            AddAdmin(entries, LMCAdminDetailCode.CoordinatePreconditionFailed,
                "The current actual position does not match the expected value, or the requested correction exceeds the approved jump limit.",
                "Read the current position, verify the approved SetPositionMaxJump policy, and prepare a new one-shot intent without bypassing either check.");
            AddAdmin(entries, LMCAdminDetailCode.DiagnosticsBuildMismatch,
                "The SetAxisPosition diagnostics build identity does not match the current PLC.",
                "Do not replay the mutation; obtain fresh capabilities, query the exact retained terminal outcome, and resolve the durable journal only after exact nonzero-generation retirement succeeds.");
            AddAdmin(entries, LMCAdminDetailCode.BootIdMismatch,
                "The SetAxisPosition diagnostics BootId does not match the current PLC boot.",
                "Treat the old outcome as unresolved and follow the durable recovery policy without inferring from current position.");
            AddAdmin(entries, LMCAdminDetailCode.MapRevisionMismatch,
                "The SetAxisPosition map revision does not match the current PLC diagnostics map.",
                "Obtain fresh capabilities and do not reuse the stale mutation identity.");
            AddAdmin(entries, LMCAdminDetailCode.SetPositionOutcomeNotFound,
                "No exact retained SetAxisPosition terminal record was found.",
                "Keep the durable recovery record unresolved; absence is not proof that the mutation was not dispatched.");
            AddAdmin(entries, LMCAdminDetailCode.SetPositionOutcomeIndeterminate,
                "The retained SetAxisPosition record is armed or indeterminate rather than terminal.",
                "Keep the durable recovery record unresolved and do not replay the mutation.");
            AddAdmin(entries, LMCAdminDetailCode.SetPositionOutcomeStoreCorrupt,
                "The retained SetAxisPosition outcome store failed its integrity checks.",
                "Keep the durable recovery record unresolved and service the PLC retained store.");
            AddAdmin(entries, LMCAdminDetailCode.SetPositionOutcomeKeyMismatch,
                "The retained SetAxisPosition record does not exactly match the requested recovery key.",
                "Keep the durable recovery record unresolved and verify every persisted identity field.");
            AddAdmin(entries, LMCAdminDetailCode.SetPositionOutcomeSlotOccupied,
                "The SetAxisPosition retained intent slot is occupied by another unresolved request.",
                "Query the exact terminal outcome, retire that generation successfully, resolve the matching durable journal record, and only then prepare another mutation.");
            AddAdmin(entries, LMCAdminDetailCode.SetPositionOutcomeStorageUnavailable,
                "The SetAxisPosition retained outcome storage is unavailable.",
                "Do not execute or replay SetAxisPosition until retained storage is healthy and advertised.");
            AddAdmin(entries, LMCAdminDetailCode.Ds402HomeOutcomeNotFound,
                "No exact retained DS402 Home record was found.",
                "Keep the durable recovery record unresolved; absence is not proof that Home was not dispatched.");
            AddAdmin(entries, LMCAdminDetailCode.Ds402HomeOutcomeIndeterminate,
                "The retained DS402 Home record is transitional or indeterminate.",
                "Keep the durable recovery record unresolved and query the exact identity again without replaying Home.");
            AddAdmin(entries, LMCAdminDetailCode.Ds402HomeOutcomeStoreCorrupt,
                "The retained DS402 Home outcome store failed its integrity checks.",
                "Keep recovery unresolved and service the PLC retained store before any new Home attempt.");
            AddAdmin(entries, LMCAdminDetailCode.Ds402HomeOutcomeKeyMismatch,
                "The retained DS402 Home record does not exactly match the requested recovery key.",
                "Keep recovery unresolved and verify every persisted identity field.");
            AddAdmin(entries, LMCAdminDetailCode.Ds402HomeOutcomeStorageUnavailable,
                "The DS402 Home outcome storage is unavailable.",
                "Do not execute or replay DS402 Home until outcome storage is healthy and advertised.");
            AddAdmin(entries, LMCAdminDetailCode.Ds402HomeExecutionFailed,
                "The accepted DS402 Home operation failed during PLC-side execution.",
                "Inspect the retained DS402 status word, native state, axis error, and drive error before deciding recovery.");
            AddAdmin(entries, LMCAdminDetailCode.Ds402HomeAborted,
                "The accepted DS402 Home operation was aborted.",
                "Inspect the retained result and current machine safety state; do not infer completion or replay automatically.");
            AddAdmin(entries, LMCAdminDetailCode.Ds402HomeOutcomeSlotOccupied,
                "The axis already has an unretired terminal DS402 Home outcome.",
                "Read the exact terminal outcome, retire it with its record generation, and only then prepare a new Home intent.");
            AddAdmin(entries, LMCAdminDetailCode.LmcHomeOutcomeNotFound,
                "No exact retained LMC_Home terminal record was found.",
                "Keep recovery unresolved; absence is not proof that LMC_Home was not dispatched.");
            AddAdmin(entries, LMCAdminDetailCode.LmcHomeOutcomeIndeterminate,
                "The retained LMC_Home record is transitional or indeterminate.",
                "Keep recovery unresolved and query the exact identity again without replaying LMC_Home.");
            AddAdmin(entries, LMCAdminDetailCode.LmcHomeOutcomeStoreCorrupt,
                "The retained LMC_Home outcome store failed its integrity checks.",
                "Keep recovery unresolved and service the PLC retained store before another LMC_Home attempt.");
            AddAdmin(entries, LMCAdminDetailCode.LmcHomeOutcomeKeyMismatch,
                "The retained LMC_Home record does not exactly match the requested recovery key.",
                "Keep recovery unresolved and verify every persisted identity field.");
            AddAdmin(entries, LMCAdminDetailCode.LmcHomeOutcomeStorageUnavailable,
                "The LMC_Home retained outcome storage is unavailable.",
                "Do not execute or replay LMC_Home until retained storage is healthy and advertised.");
            AddAdmin(entries, LMCAdminDetailCode.LmcHomeExecutionFailed,
                "The accepted LMC_Home operation failed during PLC-side execution.",
                "Inspect the retained native state, axis error, and position evidence before deciding recovery.");
            AddAdmin(entries, LMCAdminDetailCode.LmcHomeAborted,
                "The accepted LMC_Home operation was aborted.",
                "Inspect the retained result and current machine safety state; do not infer completion or replay automatically.");
            AddAdmin(entries, LMCAdminDetailCode.LmcHomeOutcomeSlotOccupied,
                "The axis already has an unretired terminal LMC_Home outcome.",
                "Read and retire the exact terminal record before preparing a new LMC_Home intent.");
            AddAdmin(entries, LMCAdminDetailCode.AxisOwnershipConflict,
                "Another admitted operation currently owns the selected axis.",
                "Wait for the current owner to publish and release its terminal outcome before submitting another mutation.");
            AddAdmin(entries, LMCAdminDetailCode.AxisOwnershipQuarantined,
                "The selected axis ownership record is quarantined after an indeterminate operation.",
                "Resolve the exact retained operation outcome and clear the quarantine through the documented recovery path.");
            AddAdmin(entries, LMCAdminDetailCode.SetOperationModeUnsupportedMode,
                "The requested DS402 operation mode is outside the activated SetOperationMode allowlist.",
                "Use CSP mode 8 only; other modes require separate PDO, controller, and hardware qualification.");
            AddAdmin(entries, LMCAdminDetailCode.SetOperationModeUnsafeState,
                "The axis is moving, faulted, or owned by an incompatible mutation, so its operation mode cannot be changed safely.",
                "Stop and make the axis safe, resolve active mutation ownership, then prepare a new intent.");
            AddAdmin(entries, LMCAdminDetailCode.SetOperationModeOutcomeNotFound,
                "No exact retained SetOperationMode outcome was found.",
                "Keep recovery unresolved; absence is not proof that the mode write was not dispatched.");
            AddAdmin(entries, LMCAdminDetailCode.SetOperationModeOutcomeIndeterminate,
                "The retained SetOperationMode outcome is indeterminate or quarantined.",
                "Do not replay the write; use read-only 0x6061 evidence and the operator recovery procedure.");
            AddAdmin(entries, LMCAdminDetailCode.SetOperationModeOutcomeStoreCorrupt,
                "The retained SetOperationMode outcome store failed its integrity checks.",
                "Keep recovery unresolved and service the outcome store before another mode mutation.");
            AddAdmin(entries, LMCAdminDetailCode.SetOperationModeOutcomeKeyMismatch,
                "The retained SetOperationMode record does not exactly match the requested recovery key.",
                "Verify every persisted identity, axis, mode, timeout, and flag field without replaying Start.");
            AddAdmin(entries, LMCAdminDetailCode.SetOperationModeOutcomeStorageUnavailable,
                "The SetOperationMode retained outcome storage is unavailable.",
                "Do not submit SetOperationMode until storage is healthy and the complete capability triad is advertised.");
            AddAdmin(entries, LMCAdminDetailCode.SetOperationModeOwnershipChannelUnavailable,
                "The LMCDiagnosticsService AxisOwnership client is not connected to LMCControlCommandService at runtime.",
                "Do not bypass ownership. Rebuild/link the LASAL communication network and verify the AxisOwnership client connection before retrying SetOperationMode.");
            AddAdmin(entries, LMCAdminDetailCode.SetOperationModeAdmissionIdentityUnavailable,
                "The SetOperationMode Start request reached the PLC without a complete nonzero ownership admission identity.",
                "Do not synthesize or replay an admission token. Inspect the TCP reservation to Diagnostics forwarding path, then submit a new intent only after the identity path is repaired.");
            AddAdmin(entries, LMCAdminDetailCode.SetOperationModeFeatureDisabled,
                "The loaded PLC runtime has SetOperationMode disabled at its feature gate.",
                "Verify the exact generated PLC artifact and loaded image feature activation before submitting a new Start request.");
            AddAdmin(entries, LMCAdminDetailCode.SetOperationModeExecutionFailed,
                "The accepted SetOperationMode operation failed during the bounded 0x6061/0x6060/0x6061 lifecycle.",
                "Inspect the retained evidence flags, observed mode, status word, and quarantine reason before recovery.");
            AddAdmin(entries, LMCAdminDetailCode.SetOperationModeOutcomeSlotOccupied,
                "The axis already has an unretired SetOperationMode outcome.",
                "Read and retire the exact terminal generation before preparing another mode mutation.");
            AddAdmin(entries, LMCAdminDetailCode.Ds402HomeExOutcomeNotFound,
                "No exact retained HomeDS402Ex outcome was found.",
                "Keep recovery unresolved; absence is not proof that the original Start was not dispatched.");
            AddAdmin(entries, LMCAdminDetailCode.Ds402HomeExOutcomeIndeterminate,
                "The retained HomeDS402Ex outcome is transitional, indeterminate, or quarantined.",
                "Do not replay Start; use only the exact recovery-key outcome path until the record becomes safely terminal.");
            AddAdmin(entries, LMCAdminDetailCode.Ds402HomeExOutcomeStoreCorrupt,
                "The retained HomeDS402Ex outcome store failed its integrity checks.",
                "Keep recovery unresolved and service the retained store before another HomeDS402Ex attempt.");
            AddAdmin(entries, LMCAdminDetailCode.Ds402HomeExOutcomeKeyMismatch,
                "The retained HomeDS402Ex record does not exactly match the full recovery key.",
                "Verify build, BootId, map revision, original request, client intent, axis, and every converted execution parameter.");
            AddAdmin(entries, LMCAdminDetailCode.Ds402HomeExOutcomeStorageUnavailable,
                "The HomeDS402Ex retained outcome storage is unavailable.",
                "Do not submit HomeDS402Ex until storage is healthy and capability bit 11 is advertised with error catalog version 7 or later.");
            AddAdmin(entries, LMCAdminDetailCode.Ds402HomeExExecutionFailed,
                "The accepted HomeDS402Ex operation failed during PLC-side execution.",
                "Inspect retained DS402 status, position readback, runtime evidence, and cleanup proof before deciding recovery.");
            AddAdmin(entries, LMCAdminDetailCode.Ds402HomeExAborted,
                "The accepted HomeDS402Ex operation was aborted.",
                "Inspect the retained result and current machine safety state; do not replay the original Start automatically.");
            AddAdmin(entries, LMCAdminDetailCode.Ds402HomeExOutcomeSlotOccupied,
                "The axis already has an unretired HomeDS402Ex outcome.",
                "Read the exact terminal outcome and retire its exact nonzero record generation before preparing another HomeDS402Ex intent.");
            AddAdmin(entries, LMCAdminDetailCode.Ds402HomeExInvalidProfile,
                "The HomeDS402Ex method, scale, range, or axis homing profile is not approved for the requested execution plan.",
                "Refresh the approved axis profile and paired MapRevision; do not bypass method, scale, range, or overflow validation.");
            AddAdmin(entries, LMCAdminDetailCode.Ds402HomeExCleanupIncomplete,
                "HomeDS402Ex cleanup did not prove every required safe terminal condition.",
                "Keep the axis quarantined and the recovery record unresolved until the exact outcome proves parameter restoration, CSP restoration, setpoint alignment, owner release, and SDO drain.");

            return entries;
        }

        private static Dictionary<long, LMCErrorDescription>
            CreateDiagnosticsEntries()
        {
            var entries = new Dictionary<long, LMCErrorDescription>();

            AddDiagnostic(entries, LMCDiagnosticsDetailCode.None,
                "No diagnostics detail error is present.",
                "No action is required.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.UnsupportedSchema,
                "The requested diagnostics schema version is not supported.",
                "Use a schema version advertised by GetDiagnosticsCapabilities.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.UnsupportedFeature,
                "The requested diagnostics feature is not enabled.",
                "Check the capability bits and use only advertised features.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.MapRevisionMismatch,
                "The supplied signal map revision does not match the PLC catalog.",
                "Reload the signal catalog and retry with its current map revision.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.SignalNotFound,
                "The requested signal identifier is not in the active catalog.",
                "Reload the catalog and select an advertised signal identifier.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.TypeMismatch,
                "The requested value type does not match the catalog or operation result.",
                "Use the exact value type and byte length advertised for the signal or SDO object.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.ReadDenied,
                "The requested diagnostics value is not readable under the active policy.",
                "Select an entry marked readable by the PLC catalog.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.WriteDenied,
                "The requested diagnostics value is not writable under the active policy.",
                "Use only a target approved by both the SDK and PLC write allowlists.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.UnsafeWriteBlocked,
                "The write was blocked by a safety policy.",
                "Do not bypass the policy; use an explicitly approved target and machine state.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.ResourceBusy,
                "The diagnostics resource is already owned or busy.",
                "Finish, release, or adopt the current operation before retrying.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.HandleOrGenerationStale,
                "The supplied handle or connection generation is stale.",
                "Refresh capabilities and recreate or adopt the resource in the current session.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.NotReady,
                "The requested diagnostics resource is not ready.",
                "Refresh its status and retry only after it reaches the required state.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.BoundsInvalid,
                "An offset, count, capacity, or other bounded input is invalid.",
                "Use values within the limits advertised by the PLC capabilities.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.MixedCapturePhase,
                "The selected signals cannot be captured in one application phase.",
                "Select signals from one compatible capture phase.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.BufferNotFrozen,
                "The recorder buffer is not frozen for a stable upload.",
                "Stop or trigger the recorder and wait for a terminal frozen state.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.BufferOverwritten,
                "The recorder data was overwritten before or during upload.",
                "Start a new recording and download it before the buffer is reused.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.RtMailboxFull,
                "The PLC real-time diagnostics mailbox is full.",
                "Wait for the active operation to drain before submitting another request.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.SdoAbort,
                "The EtherCAT slave returned an SDO abort code.",
                "Inspect the operation detail code and the slave object dictionary.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.SlaveOffline,
                "The selected EtherCAT slave is offline.",
                "Restore the slave to an online state before retrying.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.InvalidState,
                "The operation is not allowed in the current resource state.",
                "Refresh status and perform the operation only from a documented source state.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.CapacityExceeded,
                "The requested operation exceeds a configured capacity.",
                "Reduce the channel, signal, sample, or payload count to the advertised limit.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.RecordNotFound,
                "The requested recorder identity does not exist.",
                "Use the current record identity or configure a new recording.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.BufferIdentityMismatch,
                "The supplied recorder buffer identity does not match the active record.",
                "Refresh recorder metadata and use the returned buffer identity.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.TicketNotFound,
                "The requested asynchronous operation ticket does not exist.",
                "Use a ticket from the current PLC boot and connection workflow.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.InternalError,
                "The PLC diagnostics service reported an internal error.",
                "Collect the request, response, and PLC logs before retrying or escalating.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.BootIdMismatch,
                "The operation belongs to a different diagnostics PLC boot.",
                "Refresh capabilities and discard handles or tickets from the previous boot.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.TopologyRevisionMismatch,
                "The supplied EtherCAT topology revision does not match the PLC topology.",
                "Reload the EtherCAT topology and retry with its current topology revision.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.NodeNotFound,
                "The requested EtherCAT topology node does not exist.",
                "Reload the EtherCAT topology and select an advertised node identifier.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.IOReferenceNotFound,
                "The requested digital IO reference does not exist.",
                "Use a non-zero IOReference advertised by the current EtherCAT topology.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.OutputRevisionMismatch,
                "The expected digital output revision is stale.",
                "Read the output again and retry only after validating its current revision.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.OutputMaskInvalid,
                "The digital output write mask is invalid for the selected IO target.",
                "Use a non-zero mask limited to writable bits advertised for the target.");
            AddDiagnostic(entries, LMCDiagnosticsDetailCode.RTOwnerUnavailable,
                "The real-time owner required for the diagnostics operation is unavailable.",
                "Restore the owning cyclic task or service before retrying.");
            AddDiagnostic(entries,
                LMCDiagnosticsDetailCode.RecorderConfigurationAbsent,
                "The exact Recorder configuration is absent and the Recorder store is canonically empty.",
                "For 0x7E4A, require a previously persisted exact configuration Release intent. For token-qualified 0x7E4D, resolve only the matching pre-dispatch recovery token without issuing Release.");
            AddDiagnostic(entries,
                LMCDiagnosticsDetailCode.EncoderMaintenanceCompatibilityMismatch,
                "The requested encoder maintenance operation is incompatible with the selected axis configuration.",
                "Verify the encoder family, feedback socket, axis identity, PLC build, BootId, and diagnostics map revision.");
            AddDiagnostic(entries,
                LMCDiagnosticsDetailCode.EncoderMaintenanceOutcomeNotFound,
                "No exact retained encoder maintenance terminal record was found.",
                "Keep recovery unresolved; absence is not proof that the maintenance write was not dispatched.");
            AddDiagnostic(entries,
                LMCDiagnosticsDetailCode.EncoderMaintenanceOutcomeIndeterminate,
                "The retained encoder maintenance record is transitional or indeterminate.",
                "Keep recovery unresolved and query the exact identity again without replaying the write.");
            AddDiagnostic(entries,
                LMCDiagnosticsDetailCode.EncoderMaintenanceOutcomeStoreCorrupt,
                "The retained encoder maintenance outcome store failed its integrity checks.",
                "Keep recovery unresolved and service the PLC retained store before another maintenance attempt.");
            AddDiagnostic(entries,
                LMCDiagnosticsDetailCode.EncoderMaintenanceOutcomeKeyMismatch,
                "The retained encoder maintenance record does not exactly match the requested recovery key.",
                "Keep recovery unresolved and verify every persisted identity field.");
            AddDiagnostic(entries,
                LMCDiagnosticsDetailCode.EncoderMaintenanceOutcomeStorageUnavailable,
                "The encoder maintenance retained outcome storage is unavailable.",
                "Do not execute or replay the write until retained storage is healthy and advertised.");
            AddDiagnostic(entries,
                LMCDiagnosticsDetailCode.EncoderMaintenanceExecutionFailed,
                "The accepted encoder maintenance operation failed during PLC-side execution.",
                "Inspect the retained SDO abort, axis state, and drive error evidence before deciding recovery.");
            AddDiagnostic(entries,
                LMCDiagnosticsDetailCode.EncoderMaintenanceAborted,
                "The accepted encoder maintenance operation was aborted.",
                "Inspect the retained result and machine safety state; do not replay automatically.");
            AddDiagnostic(entries,
                LMCDiagnosticsDetailCode.EncoderMaintenanceOutcomeSlotOccupied,
                "The selected axis already has an unretired terminal encoder maintenance outcome.",
                "Read and retire the exact terminal record before preparing another encoder maintenance intent.");
            AddDiagnostic(entries,
                LMCDiagnosticsDetailCode.EncoderMaintenanceSemanticVerificationFailed,
                "The encoder maintenance write completed but its required semantic verification did not pass.",
                "Keep the outcome retained and inspect the supported drive evidence before any further maintenance action.");

            return entries;
        }

        private static Dictionary<long, LMCErrorDescription>
            CreateGroupProfileEntries()
        {
            var entries = new Dictionary<long, LMCErrorDescription>();

            AddProfile(entries, 0, "_LMCPROF_NoError",
                "No profile error is present.", "No action is required.");
            AddProfile(entries, 1, "_LMCPROF_INIT_ERROR",
                "The profile buffer could not be initialized because memory is unavailable.",
                "Check the profile configuration and available PLC memory.");
            AddProfile(entries, 2, "_LMCPROF_MOVECMD_ERROR",
                "The profile received an unknown or invalid move command.",
                "Check the command type and the profile error detail.");
            AddProfile(entries, 3, "_LMCPROF_INT_POINTER_ERROR",
                "The profile detected an internal pointer error.",
                "Stop using the profile and inspect the PLC-side profile error and buffer state.");
            AddProfile(entries, 4, "_LMCPROF_JUMP_CMD_ERROR",
                "A jump command has no following active buffer entry.",
                "Correct the profile buffer sequence before restarting it.");
            AddProfile(entries, 5, "_LMCPROF_POS_OVERRUN_ERROR",
                "A set-position operation exceeded the supported position range.",
                "Correct the position, offset, and resolution configuration.");
            AddProfile(entries, 6, "_LMCPROF_AXIS_ERROR",
                "One of the axes coupled to the profile has an error.",
                "Read each member axis status and clear the originating axis error.");
            AddProfile(entries, 7, "_LMCPROF_SWE_ERROR",
                "A move endpoint violates a software end position.",
                "Correct the target or the validated software-limit configuration.");
            AddProfile(entries, 8, "_LMCPROF_CIRDEF_ERROR",
                "The circle definition is invalid.",
                "Correct the circle points, plane, direction, and radius inputs.");
            AddProfile(entries, 9, "_LMCPROF_NO_POS_CHANGE",
                "The move command does not change any axis position.",
                "Remove the zero-distance move or use the intended no-position-change handling.");
            AddProfile(entries, 10, "_LMCPROF_GROUP_ERROR",
                "The selected axis group has no valid distance or cannot be changed.",
                "Check the group number, member assignment, and movement distances.");
            AddProfile(entries, 11, "_LMCPROF_KOFAC_ERROR",
                "A coupling-factor calculation failed.",
                "Check the axis identified by the sub-error and its coupling ratios.");
            AddProfile(entries, 12, "_LMCPROF_VEL_CMD_ERROR",
                "The commanded movement velocity is below the valid minimum.",
                "Use a positive velocity within the configured profile limits.");
            AddProfile(entries, 13, "_LMCPROF_ACC_CMD_ERROR",
                "The commanded acceleration or deceleration is below the valid minimum.",
                "Use positive acceleration and deceleration within the configured limits.");
            AddProfile(entries, 14, "_LMCPROF_JERK_CMD_ERROR",
                "The commanded jerk value is invalid.",
                "Use the jerk semantics and range documented for the selected profile mode.");
            AddProfile(entries, 15, "_LMCPROF_RESOLUTION_ERROR",
                "A value multiplied by the resolution does not fit in a signed 32-bit position.",
                "Reduce the value or correct the axis resolution and unit configuration.");
            AddProfile(entries, 16, "_LMCPROF_SAFETY_ZONES_REACHED",
                "The requested path reaches a configured safety zone.",
                "Do not bypass the safety zone; correct the path or approved zone configuration.");
            AddProfile(entries, 17, "_LMCPROF_INSERT_ERROR",
                "The profile buffer is full.",
                "Wait for buffered commands to complete or reduce the queued command count.");
            AddProfile(entries, 18, "_LMCPROF_AXIS_NOT_CONNECTED",
                "A selected profile axis is missing or not connected.",
                "Verify the profile member wiring and selected axis number.");
            AddProfile(entries, 19, "_LMCPROF_GROUP_NOT_FOUND",
                "The selected group is not configured.",
                "Use a configured profile group number.");
            AddProfile(entries, 20, "_LMCPROF_PARAMETER_NOT_FOUND",
                "The selected profile parameter does not exist.",
                "Use a parameter supported by this MotionLib version.");
            AddProfile(entries, 21, "_LMCPROF_PARAMETER_ERROR",
                "The selected profile feature cannot be enabled or disabled.",
                "Check the parameter semantics and current profile configuration.");
            AddProfile(entries, 22, "_LMCPROF_ERROR_BUSY",
                "The profile cannot change the parameter in its current state.",
                "Wait for a state in which the parameter is documented as changeable.");
            AddProfile(entries, 23, "_LMCPROF_OUT_OF_RANGE",
                "A profile parameter is outside its valid range.",
                "Use a value within the parameter's documented limits.");
            AddProfile(entries, 24, "_LMCPROF_RADIUS_ERROR",
                "The calculated circle radius exceeds the supported range.",
                "Reduce the geometry or correct the unit and resolution configuration.");
            AddProfile(entries, 25, "_LMCPROF_ARCLEN_ERROR",
                "The calculated circle arc length exceeds the supported range.",
                "Reduce the geometry or correct the unit and resolution configuration.");
            AddProfile(entries, 26, "_LMCPROF_RES_PATHLEN_ERROR",
                "The residual path length exceeds the supported range.",
                "Reduce the path length or correct the unit and resolution configuration.");
            AddProfile(entries, 27, "_LMCPROF_ENDPOS_ERROR",
                "A move endpoint exceeds the signed 32-bit internal range.",
                "Check the endpoint, profile position offset, and resolution.");
            AddProfile(entries, 28, "_LMCPROF_AXTRAVEL_ERROR",
                "An axis travel distance exceeds the signed 32-bit internal range.",
                "Reduce the travel or correct the position and resolution configuration.");
            AddProfile(entries, 29, "_LMCPROF_AX_COUPLE_ERROR",
                "An axis coupling command returned an error.",
                "Read the member axis error and verify the coupling configuration.");
            AddProfile(entries, 30, "_LMCPROF_PLAINEDEF_ERROR",
                "The plane of a circle is not defined.",
                "Provide a valid non-degenerate circle plane.");
            AddProfile(entries, 31, "_LMCPROF_VEC_ERROR",
                "A vector used for circle calculation is outside the valid range.",
                "Correct the circle vectors and their unit scaling.");
            AddProfile(entries, 32, "_LMCPROF_LOCKGROUP_ERROR",
                "An axis is assigned to an undefined lock group.",
                "Correct the profile lock-group assignment.");
            AddProfile(entries, 33, "_LMCPROF_LINEDEF_ERROR",
                "The linear movement endpoint definition is invalid.",
                "Correct the endpoint vector and active-axis definition.");
            AddProfile(entries, 34, "_LMCPROF_CHORDLEN_ERROR",
                "A circle start or endpoint is identical to its center.",
                "Correct the circle geometry or explicitly use a linear move.");
            AddProfile(entries, 35, "_LMCPROF_LOCKGROUP_DIFF_RESTART",
                "The restart lock-group assignment differs from the original movement.",
                "Restore the original axis group assignment before restarting.");
            AddProfile(entries, 36, "_LMCPROF_REFPOINT_DIFF_RESTART",
                "A geometric reference point differs from the original movement at restart.",
                "Restore the original reference-point configuration before restarting.");
            AddProfile(entries, 37, "_LMCPROF_AXIS_POS_ERROR",
                "An axis is not at the required start or restart position.",
                "Move the axis into the permitted restart window before continuing.");
            AddProfile(entries, 38, "_LMCPROF_INPOSITION_TIMEOUT_ERROR",
                "An exact stop did not reach the axis in-position window before timeout.",
                "Inspect following error, mechanics, controller tuning, and the in-position window.");
            AddProfile(entries, 39, "_LMCPROF_MATH_ERROR",
                "A profile calculation failed.",
                "Inspect the command geometry, dynamics, units, and PLC-side sub-error.");
            AddProfile(entries, 40, "_LMCPROF_CALC_ITERATION_ERROR",
                "A profile calculation exceeded its iteration or accuracy limit.",
                "Simplify or correct the geometry and inspect the PLC-side sub-error.");
            AddProfile(entries, 41, "_LMCPROF_REVERSE_ONGOING",
                "The command is not allowed while reverse drive is active.",
                "Wait for reverse processing to finish before submitting the command.");
            AddProfile(entries, 42, "_LMCPROF_REVERSE_INSERT_ERROR",
                "Reverse movement calculation failed while inserting a command.",
                "Inspect the buffered path and reverse-drive configuration.");
            AddProfile(entries, 43, "_LMCPROF_REVERSE_RESET_ERROR",
                "Reverse-drive state or movement-buffer reset failed.",
                "Stop the profile and inspect its buffer and reverse-drive state before retrying.");
            AddProfile(entries, 44, "_LMCPROF_TANGAXIS_INVALIDDEFINITION",
                "The tangential-axis configuration or target is invalid.",
                "Correct the tangential-axis assignment and target geometry.");
            AddProfile(entries, 45, "_LMCPROF_TANGAXIS_ANGLEERROR",
                "Tangential-axis angle calculation failed.",
                "Correct the path geometry and tangential-axis configuration.");
            AddProfile(entries, 46, "_LMCPROF_TANGAXIS_MOVEIMM_ERROR",
                "Immediate movement calculation failed for the tangential axis.",
                "Inspect the active path, dynamics, and tangential-axis configuration.");
            AddProfile(entries, 47, "_LMCPROF_TANGAXIS_NOTCONFIGURED",
                "No tangential axis is configured.",
                "Configure a valid tangential axis before using the feature.");
            AddProfile(entries, 48, "_LMCPROF_TANGAXIS_PROFILEINERROR",
                "The tangential-axis command was rejected because the profile is in error.",
                "Read and clear the originating profile error before retrying.");
            AddProfile(entries, 49, "_LMCPROF_TANGAXIS_PROFILENOTLOCKED",
                "The tangential axis cannot be activated while the profile is unlocked.",
                "Lock the configured profile before activating the tangential axis.");
            AddProfile(entries, 50, "_LMCPROF_TANGAXIS_ALREADYACTIVE",
                "A tangential axis is already active.",
                "Deactivate the current tangential axis before configuring another one.");
            AddProfile(entries, 51, "_LMCPROF_TANGAXIS_AXMULTIPLEDEFERROR",
                "A tangential-axis definition uses an axis number more than once.",
                "Assign each tangential and transversal role to a distinct axis.");
            AddProfile(entries, 52, "_LMCPROF_TANGAXIS_INVALIDAXNUMBER",
                "A tangential-axis definition contains an invalid axis number.",
                "Use only connected axes within the profile's valid member range.");
            AddProfile(entries, 53, "_LMCPROF_TANGAXIS_TANGAXISNOTCONNECTED",
                "The selected tangential axis is not connected.",
                "Correct the profile member wiring for the tangential axis.");
            AddProfile(entries, 54, "_LMCPROF_TANGAXIS_FIRSTAXISNOTCONNECTED",
                "The first transversal axis is not connected.",
                "Correct the profile member wiring for the first transversal axis.");
            AddProfile(entries, 55, "_LMCPROF_TANGAXIS_SECONDAXISNOTCONNECTED",
                "The second transversal axis is not connected.",
                "Correct the profile member wiring for the second transversal axis.");
            AddProfile(entries, 56, "_LMCPROF_NR_MAINLOCKAXES_INVALID",
                "The number of main lock-group axes is invalid for a spline.",
                "Configure the documented number of main axes for the spline command.");
            AddProfile(entries, 57, "_LMCPROF_NR_SPLINEPOINTS_INVALID",
                "The spline point count or movement table is invalid.",
                "Provide a valid positive point count and enough valid spline points.");
            AddProfile(entries, 58, "_LMCPROF_SPLINE_ACTIVATION_FAILED",
                "The generated spline movements could not be activated.",
                "Inspect the spline table, profile state, and preceding profile error.");
            AddProfile(entries, 59, "_LMCPROF_SPLINE_CALC_ONGOING",
                "The command is not allowed while spline calculation is still active.",
                "Wait for spline calculation to finish before retrying.");

            AddProfile(entries, 1000, "_LMCROBOT_REF_ERROR",
                "At least one Robot axis is not referenced or referencing at power-on failed.",
                "Read the member axis reference and error states and complete referencing.");
            AddProfile(entries, 1001, "_LMCROBOT_AX_COUPLING_ERROR",
                "A Robot axis coupling command failed or was outside its coupling window.",
                "Check the member axis state, coupling window, and kinematic configuration.");
            AddProfile(entries, 1002, "_LMCROBOT_BACKUP_POS_ERROR",
                "The positions stored at the last Robot power-off are invalid.",
                "Re-establish and validate the Robot positions before enabling motion.");
            AddProfile(entries, 1003, "_LMCROBOT_POWERON_ERROR",
                "A Robot power-on command failed.",
                "Read all member axis status and resolve the axis that rejected power-on.");
            AddProfile(entries, 1004, "_LMCROBOT_POS_CHECK_ERROR",
                "Robot position validation returned an error.",
                "Inspect the position check detail and correct the member positions.");
            AddProfile(entries, 1005, "_LMCROBOT_LINEUP_AX_ERROR",
                "An axis error occurred during Robot position lineup.",
                "Read and clear the originating member axis error before retrying lineup.");
            AddProfile(entries, 1006, "_LMCROBOT_COUPLE_TIMEOUT",
                "Robot motor-axis coupling did not complete before timeout.",
                "Inspect member readiness, coupling windows, and the real-time task state.");
            AddProfile(entries, 1007, "_LMCROBOT_AX_COUPLING_LOST",
                "Robot motor-axis coupling was lost.",
                "Stop motion and inspect the member axis and kinematic coupling state.");
            AddProfile(entries, 1008, "_LMCROBOT_AXIS_ERROR",
                "A coupled Robot motor axis has an error.",
                "Read each motor-axis status and clear the originating error.");
            AddProfile(entries, 1009, "_LMCROBOT_BELTCOUPLING_ERROR",
                "A conveyor belt-coupling command or movement failed.",
                "Inspect the conveyor coupling parameters and active profile error.");
            AddProfile(entries, 1010, "_LMCROBOT_COORDSYSTEM_ERROR",
                "The coordinate system could not be activated or transformed.",
                "Check the requested coordinate system and kinematic configuration.");
            AddProfile(entries, 1011, "_LMCROBOT_AX_CMD_ERROR",
                "A Robot command operating on one axis was rejected.",
                "Read the selected axis state and validate the command parameters.");
            AddProfile(entries, 1012, "_LMCROBOT_KINEMATIC_ERROR",
                "Direct or inverse kinematics returned an error.",
                "Inspect the kinematic model, target range, and PLC-side sub-error.");
            AddProfile(entries, 1013, "_LMCROBOT_BELTCOUPLING_INVALIDTRANSMODE",
                "The selected transition mode is not valid with belt coupling.",
                "Use a transition mode supported by the conveyor coupling contract.");

            return entries;
        }

        private static void AddDiagnostic(
            Dictionary<long, LMCErrorDescription> entries,
            LMCDiagnosticsDetailCode code,
            string description,
            string resolution)
        {
            Add(
                entries,
                LMCErrorDomain.DiagnosticsDetail,
                (long)code,
                code.ToString(),
                description,
                resolution,
                DiagnosticsSourceVersion);
        }

        private static void AddAdmin(
            Dictionary<long, LMCErrorDescription> entries,
            LMCAdminDetailCode code,
            string description,
            string resolution)
        {
            Add(
                entries,
                LMCErrorDomain.AdminDetail,
                (long)code,
                code.ToString(),
                description,
                resolution,
                AdminSourceVersion);
        }

        private static void AddProfile(
            Dictionary<long, LMCErrorDescription> entries,
            long code,
            string symbol,
            string description,
            string resolution)
        {
            Add(
                entries,
                LMCErrorDomain.GroupProfile,
                code,
                symbol,
                description,
                resolution,
                GroupProfileSourceVersion);
        }

        private static void Add(
            Dictionary<long, LMCErrorDescription> entries,
            LMCErrorDomain domain,
            long code,
            string symbol,
            string description,
            string resolution,
            string sourceVersion)
        {
            entries.Add(
                code,
                new LMCErrorDescription(
                    domain,
                    code,
                    symbol,
                    description,
                    resolution,
                    CurrentCatalogVersion,
                    sourceVersion));
        }
    }
}
