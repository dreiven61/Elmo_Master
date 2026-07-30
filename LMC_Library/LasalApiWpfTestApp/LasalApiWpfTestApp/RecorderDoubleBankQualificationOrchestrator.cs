using System;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    internal sealed class RecorderDoubleBankQualificationRequest
    {
        internal RecorderDoubleBankQualificationRequest(
            LMCDiagnosticCapabilities capabilities,
            LMCRecorderConfiguration configuration,
            object ownerToken,
            object sessionToken)
        {
            Capabilities = capabilities
                ?? throw new ArgumentNullException("capabilities");
            Configuration = configuration
                ?? throw new ArgumentNullException("configuration");
            OwnerToken = ownerToken
                ?? throw new ArgumentNullException("ownerToken");
            SessionToken = sessionToken
                ?? throw new ArgumentNullException("sessionToken");
        }

        internal LMCDiagnosticCapabilities Capabilities { get; private set; }
        internal LMCRecorderConfiguration Configuration { get; private set; }
        internal object OwnerToken { get; private set; }
        internal object SessionToken { get; private set; }
    }

    internal sealed class RecorderDoubleBankConfigurationLease
    {
        private int releaseState;

        internal RecorderDoubleBankConfigurationLease(
            object nativeHandle,
            uint diagnosticsBootId,
            uint configId,
            uint configRevision,
            object ownerToken,
            object sessionToken,
            bool usedZeroIdDiscovery)
        {
            NativeHandle = nativeHandle
                ?? throw new ArgumentNullException("nativeHandle");
            DiagnosticsBootId = diagnosticsBootId;
            ConfigId = configId;
            ConfigRevision = configRevision;
            OwnerToken = ownerToken
                ?? throw new ArgumentNullException("ownerToken");
            SessionToken = sessionToken
                ?? throw new ArgumentNullException("sessionToken");
            UsedZeroIdDiscovery = usedZeroIdDiscovery;
        }

        internal object NativeHandle { get; private set; }
        internal uint DiagnosticsBootId { get; private set; }
        internal uint ConfigId { get; private set; }
        internal uint ConfigRevision { get; private set; }
        internal object OwnerToken { get; private set; }
        internal object SessionToken { get; private set; }
        internal bool UsedZeroIdDiscovery { get; private set; }
        internal bool IsReleased
        {
            get { return Volatile.Read(ref releaseState) == 2; }
        }

        internal bool IsReleaseOutcomeUnverified
        {
            get { return Volatile.Read(ref releaseState) == 3; }
        }

        internal void BeginRelease()
        {
            if (Interlocked.CompareExchange(ref releaseState, 1, 0) != 0)
            {
                throw new InvalidOperationException(
                    "The Double-bank Recorder configuration is already released or releasing.");
            }
        }

        internal void CompleteRelease()
        {
            Volatile.Write(ref releaseState, 2);
        }

        internal void CancelRelease()
        {
            Interlocked.CompareExchange(ref releaseState, 0, 1);
        }

        internal void MarkReleaseOutcomeUnverified()
        {
            Volatile.Write(ref releaseState, 3);
        }
    }

    internal sealed class RecorderDoubleBankCaptureLease
    {
        private int releaseState;

        internal RecorderDoubleBankCaptureLease(
            object nativeIdentity,
            uint diagnosticsBootId,
            uint configId,
            uint configRevision,
            uint recordId,
            uint bufferId,
            object ownerToken,
            object sessionToken,
            bool usedZeroIdDiscovery)
        {
            NativeIdentity = nativeIdentity
                ?? throw new ArgumentNullException("nativeIdentity");
            DiagnosticsBootId = diagnosticsBootId;
            ConfigId = configId;
            ConfigRevision = configRevision;
            RecordId = recordId;
            BufferId = bufferId;
            OwnerToken = ownerToken
                ?? throw new ArgumentNullException("ownerToken");
            SessionToken = sessionToken
                ?? throw new ArgumentNullException("sessionToken");
            UsedZeroIdDiscovery = usedZeroIdDiscovery;
        }

        internal object NativeIdentity { get; private set; }
        internal uint DiagnosticsBootId { get; private set; }
        internal uint ConfigId { get; private set; }
        internal uint ConfigRevision { get; private set; }
        internal uint RecordId { get; private set; }
        internal uint BufferId { get; private set; }
        internal object OwnerToken { get; private set; }
        internal object SessionToken { get; private set; }
        internal bool UsedZeroIdDiscovery { get; private set; }
        internal bool IsReleased
        {
            get { return Volatile.Read(ref releaseState) == 2; }
        }

        internal bool IsReleaseOutcomeUnverified
        {
            get { return Volatile.Read(ref releaseState) == 3; }
        }

        internal void BeginRelease()
        {
            if (Interlocked.CompareExchange(ref releaseState, 1, 0) != 0)
            {
                throw new InvalidOperationException(
                    "The Double-bank Recorder capture is already released or releasing.");
            }
        }

        internal void CompleteRelease()
        {
            Volatile.Write(ref releaseState, 2);
        }

        internal void CancelRelease()
        {
            Interlocked.CompareExchange(ref releaseState, 0, 1);
        }

        internal void MarkReleaseOutcomeUnverified()
        {
            Volatile.Write(ref releaseState, 3);
        }
    }

    internal sealed class RecorderDoubleBankFrozenStatus
    {
        internal RecorderDoubleBankFrozenStatus(
            RecorderDoubleBankCaptureLease capture,
            bool isFrozen)
        {
            Capture = capture ?? throw new ArgumentNullException("capture");
            IsFrozen = isFrozen;
        }

        internal RecorderDoubleBankCaptureLease Capture { get; private set; }
        internal bool IsFrozen { get; private set; }
    }

    internal sealed class RecorderDoubleBankCaptureEvidence
    {
        private readonly byte[] headerCanonicalBytes;
        private readonly byte[] dataBytes;

        internal RecorderDoubleBankCaptureEvidence(
            RecorderDoubleBankCaptureLease capture,
            byte[] headerCanonicalBytes,
            byte[] dataBytes)
        {
            Capture = capture ?? throw new ArgumentNullException("capture");
            if (headerCanonicalBytes == null || headerCanonicalBytes.Length == 0)
            {
                throw new ArgumentException(
                    "Canonical Recorder header bytes are required.",
                    "headerCanonicalBytes");
            }

            if (dataBytes == null)
            {
                throw new ArgumentNullException("dataBytes");
            }

            this.headerCanonicalBytes = (byte[])headerCanonicalBytes.Clone();
            this.dataBytes = (byte[])dataBytes.Clone();
            HeaderSha256 = ComputeSha256(this.headerCanonicalBytes);
            DataSha256 = ComputeSha256(this.dataBytes);
        }

        internal RecorderDoubleBankCaptureEvidence(
            RecorderDoubleBankCaptureLease capture,
            LMCRecorderHeader header,
            byte[] dataBytes)
            : this(
                capture,
                RecorderHeaderSemanticCanonicalizer.Serialize(header),
                dataBytes)
        {
        }

        internal RecorderDoubleBankCaptureLease Capture { get; private set; }
        internal string HeaderSha256 { get; private set; }
        internal string DataSha256 { get; private set; }
        internal byte[] CopyHeaderCanonicalBytes()
        {
            return (byte[])headerCanonicalBytes.Clone();
        }

        internal byte[] CopyDataBytes()
        {
            return (byte[])dataBytes.Clone();
        }

        private static string ComputeSha256(byte[] bytes)
        {
            using (var sha256 = SHA256.Create())
            {
                return BitConverter.ToString(sha256.ComputeHash(bytes))
                    .Replace("-", string.Empty);
            }
        }
    }

    internal enum RecorderDoubleBankRecoveryScopeKind
    {
        FullQualification = 0,
        ConfigurationOnly = 1
    }

    internal sealed class RecorderDoubleBankRecoveryScope
    {
        internal RecorderDoubleBankRecoveryScope(
            RecorderDoubleBankQualificationRequest request,
            RecorderDoubleBankRecoveryScopeKind kind =
                RecorderDoubleBankRecoveryScopeKind.FullQualification)
        {
            Request = request ?? throw new ArgumentNullException("request");
            if (!Enum.IsDefined(typeof(RecorderDoubleBankRecoveryScopeKind), kind))
            {
                throw new ArgumentOutOfRangeException("kind");
            }

            Kind = kind;
            Stage = "PREFLIGHT_COMPLETE";
        }

        internal RecorderDoubleBankQualificationRequest Request
        {
            get;
            private set;
        }

        internal Guid RecoveryToken { get; private set; }

        internal void BindRecoveryToken(Guid recoveryToken)
        {
            if (recoveryToken == Guid.Empty)
            {
                throw new ArgumentException(
                    "A nonempty Double-bank recovery token is required.",
                    "recoveryToken");
            }

            if (RecoveryToken != Guid.Empty
                && RecoveryToken != recoveryToken)
            {
                throw new InvalidOperationException(
                    "A qualification recovery scope cannot be rebound to another token.");
            }

            RecoveryToken = recoveryToken;
        }

        internal RecorderDoubleBankConfigurationLease Configuration
        {
            get;
            set;
        }

        internal RecorderDoubleBankCaptureLease BankA { get; set; }
        internal RecorderDoubleBankCaptureLease BankB { get; set; }
        internal RecorderDoubleBankCaptureLease UnexpectedThird { get; set; }
        internal string Stage { get; set; }
        internal RecorderDoubleBankRecoveryScopeKind Kind { get; private set; }
        internal bool ConfigurationOnlyRetention
        {
            get
            {
                return Kind
                    == RecorderDoubleBankRecoveryScopeKind.ConfigurationOnly;
            }
        }

        internal bool HasValidConfigurationOnlyRetentionShape
        {
            get
            {
                return ConfigurationOnlyRetention
                    && BankA == null
                    && BankB == null
                    && UnexpectedThird == null
                    && !BankAStartAttempted
                    && !BankBStartAttempted
                    && !ThirdStartAttempted
                    && !ThirdStartExactBusyConfirmed;
            }
        }

        internal bool ConfigurationAttempted { get; set; }
        internal bool BankAStartAttempted { get; set; }
        internal bool BankBStartAttempted { get; set; }
        internal bool ThirdStartAttempted { get; set; }
        internal bool ThirdStartExactBusyConfirmed { get; set; }
        internal bool HasAnyPossibleResource
        {
            get
            {
                return (ConfigurationAttempted
                        && (Configuration == null
                            || !Configuration.IsReleased))
                    || (BankA != null && !BankA.IsReleased)
                    || (BankB != null && !BankB.IsReleased)
                    || (UnexpectedThird != null
                        && !UnexpectedThird.IsReleased);
            }
        }
    }

    internal sealed class RecorderDoubleBankQualificationResult
    {
        internal RecorderDoubleBankQualificationResult(
            RecorderDoubleBankRecoveryScope recoveryScope,
            RecorderDoubleBankCaptureEvidence bankAInitial,
            RecorderDoubleBankCaptureEvidence bankB,
            RecorderDoubleBankCaptureEvidence bankAReread,
            Exception thirdStartBusyException)
        {
            RecoveryScope = recoveryScope
                ?? throw new ArgumentNullException("recoveryScope");
            BankAInitial = bankAInitial
                ?? throw new ArgumentNullException("bankAInitial");
            BankB = bankB ?? throw new ArgumentNullException("bankB");
            BankAReread = bankAReread
                ?? throw new ArgumentNullException("bankAReread");
            ThirdStartBusyException = thirdStartBusyException
                ?? throw new ArgumentNullException("thirdStartBusyException");
        }

        internal RecorderDoubleBankRecoveryScope RecoveryScope
        {
            get;
            private set;
        }

        internal RecorderDoubleBankCaptureEvidence BankAInitial
        {
            get;
            private set;
        }

        internal RecorderDoubleBankCaptureEvidence BankB
        {
            get;
            private set;
        }

        internal RecorderDoubleBankCaptureEvidence BankAReread
        {
            get;
            private set;
        }

        internal Exception ThirdStartBusyException { get; private set; }
    }

    internal sealed class RecorderDoubleBankQualificationOperations
    {
        internal Func<RecorderDoubleBankRecoveryScope, Task>
            ArmRecoveryBeforeConfigureAsync { get; set; }

        internal Func<RecorderDoubleBankRecoveryScope, Task>
            PersistRecoveryCheckpointAsync { get; set; }

        internal Func<LMCRecorderConfiguration, Guid,
            Task<RecorderDoubleBankConfigurationLease>> ConfigureAsync
        {
            get;
            set;
        }

        internal Func<RecorderDoubleBankConfigurationLease,
            Task<RecorderDoubleBankCaptureLease>> StartAsync
        {
            get;
            set;
        }

        internal Func<RecorderDoubleBankCaptureLease,
            Task<RecorderDoubleBankFrozenStatus>> WaitForFrozenAsync
        {
            get;
            set;
        }

        internal Func<RecorderDoubleBankCaptureLease,
            Task<RecorderDoubleBankCaptureEvidence>> DownloadAsync
        {
            get;
            set;
        }

        internal Func<Exception, bool> IsExactResourceBusy { get; set; }
        internal Func<Exception, bool> IsReleaseConfirmedNotApplied { get; set; }
        internal Action<RecorderDoubleBankRecoveryScope, Exception>
            RecoveryRequired { get; set; }
        internal Func<RecorderDoubleBankCaptureLease, Task> ReleaseBankAsync
        {
            get;
            set;
        }

        internal Func<RecorderDoubleBankConfigurationLease, Task>
            ReleaseConfigurationAsync { get; set; }

        internal void Validate()
        {
            if (ArmRecoveryBeforeConfigureAsync == null
                || PersistRecoveryCheckpointAsync == null
                || ConfigureAsync == null
                || StartAsync == null
                || WaitForFrozenAsync == null
                || DownloadAsync == null
                || IsExactResourceBusy == null
                || IsReleaseConfirmedNotApplied == null
                || RecoveryRequired == null
                || ReleaseBankAsync == null
                || ReleaseConfigurationAsync == null)
            {
                throw new ArgumentException(
                    "All Double-bank qualification operations are required.");
            }
        }
    }

    internal static class RecorderDoubleBankQualificationOrchestrator
    {
        internal static async Task<RecorderDoubleBankRecoveryScope>
            ConfigureAndRetainAsync(
                RecorderDoubleBankQualificationRequest request,
                RecorderDoubleBankQualificationOperations operations,
                CancellationToken cancellationToken)
        {
            ValidatePreflight(request);
            if (operations == null)
            {
                throw new ArgumentNullException("operations");
            }

            operations.Validate();
            cancellationToken.ThrowIfCancellationRequested();

            var scope = new RecorderDoubleBankRecoveryScope(
                request,
                RecorderDoubleBankRecoveryScopeKind.ConfigurationOnly);
            var operationStarted = false;
            try
            {
                scope.Stage = "ARM_RECOVERY";
                await operations.ArmRecoveryBeforeConfigureAsync(scope);
                if (scope.RecoveryToken == Guid.Empty)
                {
                    throw new InvalidOperationException(
                        "Recovery arming must bind one exact nonempty token before Configure dispatch.");
                }

                cancellationToken.ThrowIfCancellationRequested();

                operationStarted = true;
                scope.Stage = "CONFIGURE";
                scope.ConfigurationAttempted = true;
                scope.Configuration = await operations.ConfigureAsync(
                    request.Configuration,
                    scope.RecoveryToken);
                ValidateConfigurationLease(scope.Configuration, request);

                scope.Stage = "PERSIST_CONFIGURATION";
                await operations.PersistRecoveryCheckpointAsync(scope);
                cancellationToken.ThrowIfCancellationRequested();

                scope.Stage = "CONFIGURATION_RETAINED";
                return scope;
            }
            catch (Exception primaryError)
            {
                if (operationStarted)
                {
                    try
                    {
                        operations.RecoveryRequired(scope, primaryError);
                    }
                    catch (Exception recoveryError)
                    {
                        throw new InvalidOperationException(
                            "Double-bank Recorder recovery scope publication failed.",
                            new AggregateException(
                                primaryError,
                                recoveryError));
                    }
                }

                ExceptionDispatchInfo.Capture(primaryError).Throw();
                throw;
            }
        }

        internal static async Task<RecorderDoubleBankQualificationResult>
            RunAsync(
                RecorderDoubleBankQualificationRequest request,
                RecorderDoubleBankQualificationOperations operations,
                CancellationToken cancellationToken)
        {
            ValidatePreflight(request);
            if (operations == null)
            {
                throw new ArgumentNullException("operations");
            }

            operations.Validate();
            cancellationToken.ThrowIfCancellationRequested();

            var scope = new RecorderDoubleBankRecoveryScope(request);
            var operationStarted = false;
            try
            {
                scope.Stage = "ARM_RECOVERY";
                await operations.ArmRecoveryBeforeConfigureAsync(scope);
                if (scope.RecoveryToken == Guid.Empty)
                {
                    throw new InvalidOperationException(
                        "Recovery arming must bind one exact nonempty token before Configure dispatch.");
                }

                cancellationToken.ThrowIfCancellationRequested();

                operationStarted = true;
                scope.Stage = "CONFIGURE";
                scope.ConfigurationAttempted = true;
                scope.Configuration = await operations.ConfigureAsync(
                    request.Configuration,
                    scope.RecoveryToken);
                ValidateConfigurationLease(scope.Configuration, request);
                scope.Stage = "PERSIST_CONFIGURATION";
                await operations.PersistRecoveryCheckpointAsync(scope);
                cancellationToken.ThrowIfCancellationRequested();

                scope.Stage = "START_A";
                scope.BankAStartAttempted = true;
                scope.BankA = await operations.StartAsync(
                    scope.Configuration);
                ValidateCaptureLease(scope.BankA, scope.Configuration, request, 0);
                scope.Stage = "PERSIST_BANK_A";
                await operations.PersistRecoveryCheckpointAsync(scope);
                cancellationToken.ThrowIfCancellationRequested();

                scope.Stage = "FREEZE_A";
                await ValidateFrozenAsync(
                    operations,
                    scope.BankA,
                    cancellationToken);

                scope.Stage = "DOWNLOAD_A";
                var bankAInitial = await operations.DownloadAsync(scope.BankA);
                ValidateEvidence(bankAInitial, scope.BankA);
                cancellationToken.ThrowIfCancellationRequested();

                scope.Stage = "START_B";
                scope.BankBStartAttempted = true;
                scope.BankB = await operations.StartAsync(scope.Configuration);
                ValidateCaptureLease(scope.BankB, scope.Configuration, request, 1);
                if (scope.BankB.RecordId == scope.BankA.RecordId)
                {
                    throw new InvalidOperationException(
                        "Double-bank Recorder captures must have distinct RecordId values.");
                }

                scope.Stage = "PERSIST_BANK_B";
                await operations.PersistRecoveryCheckpointAsync(scope);
                cancellationToken.ThrowIfCancellationRequested();
                scope.Stage = "FREEZE_B";
                await ValidateFrozenAsync(
                    operations,
                    scope.BankB,
                    cancellationToken);

                scope.Stage = "DOWNLOAD_B";
                var bankB = await operations.DownloadAsync(scope.BankB);
                ValidateEvidence(bankB, scope.BankB);
                cancellationToken.ThrowIfCancellationRequested();

                scope.Stage = "THIRD_START_BUSY";
                Exception busyException = null;
                try
                {
                    scope.ThirdStartAttempted = true;
                    scope.UnexpectedThird = await operations.StartAsync(
                        scope.Configuration);
                }
                catch (Exception error)
                {
                    if (!operations.IsExactResourceBusy(error))
                    {
                        throw;
                    }

                    busyException = error;
                    scope.ThirdStartExactBusyConfirmed = true;
                }

                if (scope.UnexpectedThird != null)
                {
                    ValidateCaptureLease(
                        scope.UnexpectedThird,
                        scope.Configuration,
                        request,
                        scope.UnexpectedThird.BufferId);
                    scope.Stage = "PERSIST_UNEXPECTED_THIRD";
                    await operations.PersistRecoveryCheckpointAsync(scope);
                    throw new InvalidOperationException(
                        "A third Double-bank Recorder start succeeded while both banks were retained.");
                }

                if (busyException == null)
                {
                    throw new InvalidOperationException(
                        "The third Double-bank Recorder start did not return exact ResourceBusy.");
                }

                cancellationToken.ThrowIfCancellationRequested();
                scope.Stage = "REREAD_A";
                var bankAReread = await operations.DownloadAsync(scope.BankA);
                ValidateEvidence(bankAReread, scope.BankA);
                if (!string.Equals(
                        bankAInitial.HeaderSha256,
                        bankAReread.HeaderSha256,
                        StringComparison.Ordinal)
                    || !string.Equals(
                        bankAInitial.DataSha256,
                        bankAReread.DataSha256,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "Bank A header or data changed after the Bank B capture.");
                }

                cancellationToken.ThrowIfCancellationRequested();
                scope.Stage = "COMPLETE_RETAINED";
                return new RecorderDoubleBankQualificationResult(
                    scope,
                    bankAInitial,
                    bankB,
                    bankAReread,
                    busyException);
            }
            catch (Exception primaryError)
            {
                if (operationStarted)
                {
                    try
                    {
                        operations.RecoveryRequired(scope, primaryError);
                    }
                    catch (Exception recoveryError)
                    {
                        throw new InvalidOperationException(
                            "Double-bank Recorder recovery scope publication failed.",
                            new AggregateException(
                                primaryError,
                                recoveryError));
                    }
                }

                ExceptionDispatchInfo.Capture(primaryError).Throw();
                throw;
            }
        }

        internal static async Task ReleaseBankAsync(
            RecorderDoubleBankRecoveryScope scope,
            RecorderDoubleBankCaptureLease capture,
            RecorderDoubleBankQualificationOperations operations,
            bool explicitSafetyConfirmation,
            CancellationToken cancellationToken)
        {
            await ReleaseBankCoreAsync(
                scope,
                capture,
                operations,
                explicitSafetyConfirmation,
                null,
                cancellationToken);
        }

        internal static async Task ReleaseBankWithDurableIntentAsync(
            RecorderDoubleBankRecoveryScope scope,
            RecorderDoubleBankCaptureLease capture,
            RecorderDoubleBankQualificationOperations operations,
            bool explicitSafetyConfirmation,
            Action persistIntentBeforeDispatch,
            CancellationToken cancellationToken)
        {
            if (persistIntentBeforeDispatch == null)
            {
                throw new ArgumentNullException(
                    "persistIntentBeforeDispatch");
            }

            await ReleaseBankCoreAsync(
                scope,
                capture,
                operations,
                explicitSafetyConfirmation,
                persistIntentBeforeDispatch,
                cancellationToken);
        }

        private static async Task ReleaseBankCoreAsync(
            RecorderDoubleBankRecoveryScope scope,
            RecorderDoubleBankCaptureLease capture,
            RecorderDoubleBankQualificationOperations operations,
            bool explicitSafetyConfirmation,
            Action persistIntentBeforeDispatch,
            CancellationToken cancellationToken)
        {
            ValidateReleaseArguments(
                scope,
                operations,
                explicitSafetyConfirmation);
            if (!ReferenceEquals(scope.BankA, capture)
                && !ReferenceEquals(scope.BankB, capture)
                && !ReferenceEquals(scope.UnexpectedThird, capture))
            {
                throw new InvalidOperationException(
                    "The capture is not part of this Double-bank recovery scope.");
            }

            EnsureReleaseOrder(scope, capture);
            ValidateCaptureLease(
                capture,
                scope.Configuration,
                scope.Request,
                capture.BufferId);
            cancellationToken.ThrowIfCancellationRequested();
            capture.BeginRelease();
            if (persistIntentBeforeDispatch != null)
            {
                try
                {
                    persistIntentBeforeDispatch();
                }
                catch
                {
                    capture.CancelRelease();
                    throw;
                }
            }

            try
            {
                await operations.ReleaseBankAsync(capture);
                capture.CompleteRelease();
            }
            catch (Exception releaseError)
            {
                HandleReleaseFailure(
                    releaseError,
                    operations,
                    capture.CancelRelease,
                    capture.MarkReleaseOutcomeUnverified,
                    "Double-bank Recorder capture");
                throw;
            }
        }

        internal static async Task ReleaseConfigurationAsync(
            RecorderDoubleBankRecoveryScope scope,
            RecorderDoubleBankQualificationOperations operations,
            bool explicitSafetyConfirmation,
            CancellationToken cancellationToken)
        {
            await ReleaseConfigurationCoreAsync(
                scope,
                operations,
                explicitSafetyConfirmation,
                null,
                cancellationToken);
        }

        internal static async Task
            ReleaseConfigurationWithDurableIntentAsync(
                RecorderDoubleBankRecoveryScope scope,
                RecorderDoubleBankQualificationOperations operations,
                bool explicitSafetyConfirmation,
                Action persistIntentBeforeDispatch,
                CancellationToken cancellationToken)
        {
            if (persistIntentBeforeDispatch == null)
            {
                throw new ArgumentNullException(
                    "persistIntentBeforeDispatch");
            }

            await ReleaseConfigurationCoreAsync(
                scope,
                operations,
                explicitSafetyConfirmation,
                persistIntentBeforeDispatch,
                cancellationToken);
        }

        private static async Task ReleaseConfigurationCoreAsync(
            RecorderDoubleBankRecoveryScope scope,
            RecorderDoubleBankQualificationOperations operations,
            bool explicitSafetyConfirmation,
            Action persistIntentBeforeDispatch,
            CancellationToken cancellationToken)
        {
            ValidateReleaseArguments(
                scope,
                operations,
                explicitSafetyConfirmation);
            if (scope.Configuration == null)
            {
                throw new InvalidOperationException(
                    "No exact Double-bank configuration lease is available.");
            }

            EnsureCaptureReleased(scope.BankA, "Bank A");
            EnsureCaptureReleased(scope.BankB, "Bank B");
            EnsureCaptureReleased(scope.UnexpectedThird, "unexpected third bank");
            EnsureNoUnknownStartedCapture(scope);
            ValidateConfigurationLease(scope.Configuration, scope.Request);
            cancellationToken.ThrowIfCancellationRequested();
            scope.Configuration.BeginRelease();
            if (persistIntentBeforeDispatch != null)
            {
                try
                {
                    persistIntentBeforeDispatch();
                }
                catch
                {
                    scope.Configuration.CancelRelease();
                    throw;
                }
            }

            try
            {
                await operations.ReleaseConfigurationAsync(
                    scope.Configuration);
                scope.Configuration.CompleteRelease();
            }
            catch (Exception releaseError)
            {
                HandleReleaseFailure(
                    releaseError,
                    operations,
                    scope.Configuration.CancelRelease,
                    scope.Configuration.MarkReleaseOutcomeUnverified,
                    "Double-bank Recorder configuration");
                throw;
            }
        }

        private static void ValidatePreflight(
            RecorderDoubleBankQualificationRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var capabilities = request.Capabilities;
            if (!capabilities.Supports(
                    LMCDiagnosticCapability.RecorderSingleBank)
                || !capabilities.Supports(
                    LMCDiagnosticCapability.RecorderDoubleBank))
            {
                throw new NotSupportedException(
                    "Recorder Single-bank and Double-bank capabilities are both required.");
            }

            if (capabilities.RecorderBufferCount != 2)
            {
                throw new InvalidOperationException(
                    "Double-bank qualification requires exactly two Recorder buffers.");
            }

            if (capabilities.DiagnosticsBootId == 0)
            {
                throw new InvalidOperationException(
                    "Double-bank qualification requires a nonzero diagnostics BootId.");
            }

            if (request.Configuration.BufferMode
                != LMCRecorderBufferMode.Double)
            {
                throw new ArgumentException(
                    "Double-bank qualification requires one Double Recorder configuration.",
                    "request");
            }

            if (request.Configuration.RequestedConfigId == 0)
            {
                throw new ArgumentException(
                    "Double-bank qualification requires a caller-selected nonzero RequestedConfigId before Configure dispatch.",
                    "request");
            }
        }

        private static void ValidateConfigurationLease(
            RecorderDoubleBankConfigurationLease lease,
            RecorderDoubleBankQualificationRequest request)
        {
            if (lease == null)
            {
                throw new InvalidOperationException(
                    "Configure did not return an exact lease.");
            }

            if (lease.DiagnosticsBootId
                    != request.Capabilities.DiagnosticsBootId
                || lease.ConfigId == 0
                || lease.ConfigId
                    != request.Configuration.RequestedConfigId
                || lease.ConfigRevision == 0
                || lease.UsedZeroIdDiscovery
                || !ReferenceEquals(lease.OwnerToken, request.OwnerToken)
                || !ReferenceEquals(lease.SessionToken, request.SessionToken))
            {
                throw new InvalidOperationException(
                    "Double-bank configuration lease identity or provenance is invalid.");
            }
        }

        private static void ValidateCaptureLease(
            RecorderDoubleBankCaptureLease capture,
            RecorderDoubleBankConfigurationLease configuration,
            RecorderDoubleBankQualificationRequest request,
            uint expectedBufferId)
        {
            if (capture == null || configuration == null)
            {
                throw new InvalidOperationException(
                    "An exact Double-bank capture and configuration lease are required.");
            }

            if (capture.DiagnosticsBootId
                    != request.Capabilities.DiagnosticsBootId
                || capture.ConfigId != configuration.ConfigId
                || capture.ConfigRevision != configuration.ConfigRevision
                || capture.RecordId == 0
                || capture.BufferId != expectedBufferId
                || capture.UsedZeroIdDiscovery
                || !ReferenceEquals(capture.OwnerToken, request.OwnerToken)
                || !ReferenceEquals(capture.SessionToken, request.SessionToken))
            {
                throw new InvalidOperationException(
                    "Double-bank capture identity, bank, or provenance is invalid.");
            }
        }

        private static async Task ValidateFrozenAsync(
            RecorderDoubleBankQualificationOperations operations,
            RecorderDoubleBankCaptureLease capture,
            CancellationToken cancellationToken)
        {
            var status = await operations.WaitForFrozenAsync(capture);
            if (status == null
                || !ReferenceEquals(status.Capture, capture)
                || !status.IsFrozen)
            {
                throw new InvalidOperationException(
                    "Double-bank capture did not reach an exact frozen state.");
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        private static void ValidateEvidence(
            RecorderDoubleBankCaptureEvidence evidence,
            RecorderDoubleBankCaptureLease capture)
        {
            if (evidence == null
                || !ReferenceEquals(evidence.Capture, capture))
            {
                throw new InvalidOperationException(
                    "Recorder download evidence is not bound to the exact capture lease.");
            }
        }

        private static void ValidateReleaseArguments(
            RecorderDoubleBankRecoveryScope scope,
            RecorderDoubleBankQualificationOperations operations,
            bool explicitSafetyConfirmation)
        {
            if (scope == null)
            {
                throw new ArgumentNullException("scope");
            }

            if (operations == null)
            {
                throw new ArgumentNullException("operations");
            }

            operations.Validate();
            if (!explicitSafetyConfirmation)
            {
                throw new InvalidOperationException(
                    "Explicit safety confirmation is required before Double-bank release.");
            }
        }

        private static void EnsureCaptureReleased(
            RecorderDoubleBankCaptureLease capture,
            string name)
        {
            if (capture != null && !capture.IsReleased)
            {
                throw new InvalidOperationException(
                    name + " must be released before the Recorder configuration.");
            }
        }

        private static void EnsureReleaseOrder(
            RecorderDoubleBankRecoveryScope scope,
            RecorderDoubleBankCaptureLease capture)
        {
            if (!ReferenceEquals(scope.UnexpectedThird, capture))
            {
                EnsureNoUnknownThirdCapture(scope);
            }

            if (ReferenceEquals(scope.BankA, capture))
            {
                if (scope.BankBStartAttempted && scope.BankB == null)
                {
                    throw new InvalidOperationException(
                        "Bank B Start outcome is unverified; Bank A Release is blocked.");
                }

                EnsureCaptureReleased(
                    scope.UnexpectedThird,
                    "The unexpected third bank");
                EnsureCaptureReleased(scope.BankB, "Bank B");
                return;
            }

            if (ReferenceEquals(scope.BankB, capture))
            {
                EnsureCaptureReleased(
                    scope.UnexpectedThird,
                    "The unexpected third bank");
            }
        }

        private static void EnsureNoUnknownStartedCapture(
            RecorderDoubleBankRecoveryScope scope)
        {
            if (scope.BankAStartAttempted && scope.BankA == null)
            {
                throw new InvalidOperationException(
                    "Bank A Start outcome is unverified; configuration Release is blocked.");
            }

            if (scope.BankBStartAttempted && scope.BankB == null)
            {
                throw new InvalidOperationException(
                    "Bank B Start outcome is unverified; configuration Release is blocked.");
            }

            EnsureNoUnknownThirdCapture(scope);
        }

        private static void EnsureNoUnknownThirdCapture(
            RecorderDoubleBankRecoveryScope scope)
        {
            if (scope.ThirdStartAttempted
                && !scope.ThirdStartExactBusyConfirmed
                && scope.UnexpectedThird == null)
            {
                throw new InvalidOperationException(
                    "The third Start outcome is unverified; bank and configuration Release are blocked.");
            }
        }

        private static void HandleReleaseFailure(
            Exception releaseError,
            RecorderDoubleBankQualificationOperations operations,
            Action markConfirmedNotApplied,
            Action markOutcomeUnverified,
            string resourceName)
        {
            bool confirmedNotApplied;
            try
            {
                confirmedNotApplied = operations.IsReleaseConfirmedNotApplied(
                    releaseError);
            }
            catch (Exception classificationError)
            {
                markOutcomeUnverified();
                throw new InvalidOperationException(
                    resourceName
                    + " Release outcome classification failed; automatic retry is blocked.",
                    new AggregateException(
                        releaseError,
                        classificationError));
            }

            if (confirmedNotApplied)
            {
                markConfirmedNotApplied();
                return;
            }

            markOutcomeUnverified();
        }
    }
}
