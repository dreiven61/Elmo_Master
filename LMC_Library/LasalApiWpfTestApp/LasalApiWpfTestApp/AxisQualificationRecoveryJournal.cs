using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace LasalMotionControlApiExample
{
    internal enum AxisQualificationRecoveryStage
    {
        ArmedBeforePowerOn = 1,
        PowerOnAccepted = 2,
        PowerOnStable = 3,
        MovePrepared = 4,
        MoveAccepted = 5,
        MoveStable = 6,
        StopAccepted = 7,
        StopStable = 8,
        PowerOffAccepted = 9,
        SafeResolved = 10
    }

    internal sealed class AxisQualificationRecoveryRecord
    {
        private const int MaximumAxisNameLength = 256;

        private readonly Guid identity;
        private readonly long recordRevision;
        private readonly AxisQualificationRecoveryStage stage;
        private readonly string endpointIp;
        private readonly int endpointPort;
        private readonly long ownerSessionGeneration;
        private readonly string axisName;
        private readonly ushort axisReference;
        private readonly uint diagnosticsBuild;
        private readonly uint diagnosticsBootId;
        private readonly uint mapRevision;
        private readonly int deltaRaw;
        private readonly int velocityRaw;
        private readonly int accelerationRaw;
        private readonly int decelerationRaw;
        private readonly int jerkRaw;
        private readonly int toleranceRaw;
        private readonly bool hasTarget;
        private readonly int startPositionRaw;
        private readonly int targetPositionRaw;
        private readonly long safetyGeneration;
        private readonly bool wasCrashPromoted;
        private readonly DateTime createdUtc;
        private readonly DateTime updatedUtc;
        private readonly byte[] checksum;

        internal AxisQualificationRecoveryRecord(
            Guid identity,
            long recordRevision,
            AxisQualificationRecoveryStage stage,
            string endpointIp,
            int endpointPort,
            long ownerSessionGeneration,
            string axisName,
            ushort axisReference,
            uint diagnosticsBuild,
            uint diagnosticsBootId,
            uint mapRevision,
            int deltaRaw,
            int velocityRaw,
            int accelerationRaw,
            int decelerationRaw,
            int jerkRaw,
            int toleranceRaw,
            bool hasTarget,
            int startPositionRaw,
            int targetPositionRaw,
            long safetyGeneration,
            bool wasCrashPromoted,
            DateTime createdUtc,
            DateTime updatedUtc,
            byte[] checksum)
        {
            if (identity == Guid.Empty)
            {
                throw new ArgumentException(
                    "Axis qualification recovery identity cannot be empty.",
                    "identity");
            }
            if (recordRevision < 1)
            {
                throw new ArgumentOutOfRangeException(
                    "recordRevision",
                    "Axis qualification recovery revision must be positive.");
            }
            ValidateStage(stage);

            var normalizedEndpointIp = NormalizeIpv4(
                endpointIp,
                "endpointIp");
            ValidatePort(endpointPort, "endpointPort");
            if (ownerSessionGeneration < 1)
            {
                throw new ArgumentOutOfRangeException(
                    "ownerSessionGeneration",
                    "The owner session generation must be positive.");
            }
            ValidateAsciiName(
                axisName,
                "axisName",
                MaximumAxisNameLength);
            if (axisReference == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "axisReference",
                    "The qualification Axis reference must be non-zero.");
            }
            if (diagnosticsBuild == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "diagnosticsBuild",
                    "Qualification recovery requires DiagnosticsBuild.");
            }
            if (diagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "diagnosticsBootId",
                    "Qualification recovery requires DiagnosticsBootId.");
            }
            if (mapRevision == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "mapRevision",
                    "Qualification recovery requires MapRevision.");
            }
            ValidateInput(
                deltaRaw,
                velocityRaw,
                accelerationRaw,
                decelerationRaw,
                jerkRaw,
                toleranceRaw);
            ValidateTarget(
                stage,
                deltaRaw,
                hasTarget,
                startPositionRaw,
                targetPositionRaw);
            if (safetyGeneration < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "safetyGeneration",
                    "The qualification safety generation cannot be negative.");
            }
            ValidateTimestamps(createdUtc, updatedUtc);
            if (checksum != null
                && checksum.Length
                    != AxisQualificationRecoveryJournal.ChecksumLength)
            {
                throw new ArgumentException(
                    "An Axis qualification recovery checksum must contain 32 bytes.",
                    "checksum");
            }

            this.identity = identity;
            this.recordRevision = recordRevision;
            this.stage = stage;
            this.endpointIp = normalizedEndpointIp;
            this.endpointPort = endpointPort;
            this.ownerSessionGeneration = ownerSessionGeneration;
            this.axisName = axisName;
            this.axisReference = axisReference;
            this.diagnosticsBuild = diagnosticsBuild;
            this.diagnosticsBootId = diagnosticsBootId;
            this.mapRevision = mapRevision;
            this.deltaRaw = deltaRaw;
            this.velocityRaw = velocityRaw;
            this.accelerationRaw = accelerationRaw;
            this.decelerationRaw = decelerationRaw;
            this.jerkRaw = jerkRaw;
            this.toleranceRaw = toleranceRaw;
            this.hasTarget = hasTarget;
            this.startPositionRaw = startPositionRaw;
            this.targetPositionRaw = targetPositionRaw;
            this.safetyGeneration = safetyGeneration;
            this.wasCrashPromoted = wasCrashPromoted;
            this.createdUtc = createdUtc;
            this.updatedUtc = updatedUtc;
            this.checksum = CloneBytes(checksum);
        }

        internal Guid Identity { get { return identity; } }
        internal long RecordRevision { get { return recordRevision; } }
        internal AxisQualificationRecoveryStage Stage { get { return stage; } }
        internal string EndpointIp { get { return endpointIp; } }
        internal int EndpointPort { get { return endpointPort; } }
        internal long OwnerSessionGeneration
        {
            get { return ownerSessionGeneration; }
        }
        internal string AxisName { get { return axisName; } }
        internal ushort AxisReference { get { return axisReference; } }
        internal uint DiagnosticsBuild { get { return diagnosticsBuild; } }
        internal uint DiagnosticsBootId { get { return diagnosticsBootId; } }
        internal uint MapRevision { get { return mapRevision; } }
        internal int DeltaRaw { get { return deltaRaw; } }
        internal int VelocityRaw { get { return velocityRaw; } }
        internal int AccelerationRaw { get { return accelerationRaw; } }
        internal int DecelerationRaw { get { return decelerationRaw; } }
        internal int JerkRaw { get { return jerkRaw; } }
        internal int ToleranceRaw { get { return toleranceRaw; } }
        internal bool HasTarget { get { return hasTarget; } }
        internal int StartPositionRaw { get { return startPositionRaw; } }
        internal int TargetPositionRaw { get { return targetPositionRaw; } }
        internal long SafetyGeneration { get { return safetyGeneration; } }
        internal bool WasCrashPromoted { get { return wasCrashPromoted; } }
        internal DateTime CreatedUtc { get { return createdUtc; } }
        internal DateTime UpdatedUtc { get { return updatedUtc; } }
        internal bool IsActive
        {
            get { return stage != AxisQualificationRecoveryStage.SafeResolved; }
        }
        internal byte[] Checksum { get { return CloneBytes(checksum); } }

        internal bool MatchesEndpoint(
            string candidateEndpointIp,
            int candidateEndpointPort)
        {
            string normalizedEndpointIp;
            return TryNormalizeIpv4(
                    candidateEndpointIp,
                    out normalizedEndpointIp)
                && string.Equals(
                    endpointIp,
                    normalizedEndpointIp,
                    StringComparison.Ordinal)
                && endpointPort == candidateEndpointPort;
        }

        internal bool MatchesRecoveryIdentity(
            string candidateEndpointIp,
            int candidateEndpointPort,
            long candidateOwnerSessionGeneration,
            string candidateAxisName,
            ushort candidateAxisReference,
            uint candidateDiagnosticsBuild,
            uint candidateDiagnosticsBootId,
            uint candidateMapRevision)
        {
            return MatchesEndpoint(
                    candidateEndpointIp,
                    candidateEndpointPort)
                && ownerSessionGeneration
                    == candidateOwnerSessionGeneration
                && string.Equals(
                    axisName,
                    candidateAxisName,
                    StringComparison.Ordinal)
                && axisReference == candidateAxisReference
                && diagnosticsBuild == candidateDiagnosticsBuild
                && diagnosticsBootId == candidateDiagnosticsBootId
                && mapRevision == candidateMapRevision;
        }

        internal bool MatchesInput(
            int candidateDeltaRaw,
            int candidateVelocityRaw,
            int candidateAccelerationRaw,
            int candidateDecelerationRaw,
            int candidateJerkRaw,
            int candidateToleranceRaw)
        {
            return deltaRaw == candidateDeltaRaw
                && velocityRaw == candidateVelocityRaw
                && accelerationRaw == candidateAccelerationRaw
                && decelerationRaw == candidateDecelerationRaw
                && jerkRaw == candidateJerkRaw
                && toleranceRaw == candidateToleranceRaw;
        }

        internal bool MatchesTarget(
            int candidateStartPositionRaw,
            int candidateTargetPositionRaw)
        {
            return hasTarget
                && startPositionRaw == candidateStartPositionRaw
                && targetPositionRaw == candidateTargetPositionRaw;
        }

        internal AxisQualificationRecoveryRecord Copy()
        {
            return CreateCopy(checksum);
        }

        internal AxisQualificationRecoveryRecord WithChecksum(
            byte[] nextChecksum)
        {
            return CreateCopy(nextChecksum);
        }

        internal AxisQualificationRecoveryRecord TransitionTo(
            AxisQualificationRecoveryStage nextStage,
            DateTime nextUpdatedUtc)
        {
            ValidateTransition(stage, nextStage);
            return CreateNext(
                nextStage,
                hasTarget,
                startPositionRaw,
                targetPositionRaw,
                safetyGeneration,
                wasCrashPromoted,
                nextUpdatedUtc);
        }

        internal AxisQualificationRecoveryRecord PrepareMove(
            int nextStartPositionRaw,
            int nextTargetPositionRaw,
            DateTime nextUpdatedUtc)
        {
            if (hasTarget)
            {
                throw new InvalidOperationException(
                    "An Axis qualification target cannot be replaced.");
            }
            ValidateTransition(
                stage,
                AxisQualificationRecoveryStage.MovePrepared);
            return CreateNext(
                AxisQualificationRecoveryStage.MovePrepared,
                true,
                nextStartPositionRaw,
                nextTargetPositionRaw,
                safetyGeneration,
                wasCrashPromoted,
                nextUpdatedUtc);
        }

        internal AxisQualificationRecoveryRecord TransitionSafety(
            AxisQualificationRecoveryStage nextStage,
            long nextSafetyGeneration,
            DateTime nextUpdatedUtc)
        {
            if (nextSafetyGeneration < safetyGeneration)
            {
                throw new InvalidOperationException(
                    "The Axis qualification safety generation cannot move backwards.");
            }
            ValidateTransition(stage, nextStage);
            return CreateNext(
                nextStage,
                hasTarget,
                startPositionRaw,
                targetPositionRaw,
                nextSafetyGeneration,
                wasCrashPromoted,
                nextUpdatedUtc);
        }

        internal AxisQualificationRecoveryRecord PromoteCrash(
            AxisQualificationRecoveryStage nextStage,
            DateTime nextUpdatedUtc)
        {
            if ((stage
                        != AxisQualificationRecoveryStage.ArmedBeforePowerOn
                    || nextStage
                        != AxisQualificationRecoveryStage.PowerOnAccepted)
                && (stage
                        != AxisQualificationRecoveryStage.MovePrepared
                    || nextStage
                        != AxisQualificationRecoveryStage.MoveAccepted))
            {
                throw new InvalidOperationException(
                    "Only volatile pre-acceptance qualification stages can be crash-promoted.");
            }
            ValidateTransition(stage, nextStage);
            return CreateNext(
                nextStage,
                hasTarget,
                startPositionRaw,
                targetPositionRaw,
                safetyGeneration,
                true,
                nextUpdatedUtc);
        }

        internal AxisQualificationRecoveryRecord ResolveOperatorRetirement(
            DateTime nextUpdatedUtc)
        {
            if (!IsActive)
            {
                throw new InvalidOperationException(
                    "A resolved Axis qualification record cannot be retired again.");
            }
            return CreateNext(
                AxisQualificationRecoveryStage.SafeResolved,
                hasTarget,
                startPositionRaw,
                targetPositionRaw,
                safetyGeneration,
                wasCrashPromoted,
                nextUpdatedUtc);
        }

        internal bool ExactEquals(
            AxisQualificationRecoveryRecord candidate)
        {
            return candidate != null
                && identity == candidate.identity
                && recordRevision == candidate.recordRevision
                && stage == candidate.stage
                && string.Equals(
                    endpointIp,
                    candidate.endpointIp,
                    StringComparison.Ordinal)
                && endpointPort == candidate.endpointPort
                && ownerSessionGeneration
                    == candidate.ownerSessionGeneration
                && string.Equals(
                    axisName,
                    candidate.axisName,
                    StringComparison.Ordinal)
                && axisReference == candidate.axisReference
                && diagnosticsBuild == candidate.diagnosticsBuild
                && diagnosticsBootId == candidate.diagnosticsBootId
                && mapRevision == candidate.mapRevision
                && deltaRaw == candidate.deltaRaw
                && velocityRaw == candidate.velocityRaw
                && accelerationRaw == candidate.accelerationRaw
                && decelerationRaw == candidate.decelerationRaw
                && jerkRaw == candidate.jerkRaw
                && toleranceRaw == candidate.toleranceRaw
                && hasTarget == candidate.hasTarget
                && startPositionRaw == candidate.startPositionRaw
                && targetPositionRaw == candidate.targetPositionRaw
                && safetyGeneration == candidate.safetyGeneration
                && wasCrashPromoted == candidate.wasCrashPromoted
                && createdUtc == candidate.createdUtc
                && updatedUtc == candidate.updatedUtc
                && ByteArraysEqual(checksum, candidate.checksum);
        }

        private AxisQualificationRecoveryRecord CreateCopy(
            byte[] nextChecksum)
        {
            return new AxisQualificationRecoveryRecord(
                identity,
                recordRevision,
                stage,
                endpointIp,
                endpointPort,
                ownerSessionGeneration,
                axisName,
                axisReference,
                diagnosticsBuild,
                diagnosticsBootId,
                mapRevision,
                deltaRaw,
                velocityRaw,
                accelerationRaw,
                decelerationRaw,
                jerkRaw,
                toleranceRaw,
                hasTarget,
                startPositionRaw,
                targetPositionRaw,
                safetyGeneration,
                wasCrashPromoted,
                createdUtc,
                updatedUtc,
                nextChecksum);
        }

        private AxisQualificationRecoveryRecord CreateNext(
            AxisQualificationRecoveryStage nextStage,
            bool nextHasTarget,
            int nextStartPositionRaw,
            int nextTargetPositionRaw,
            long nextSafetyGeneration,
            bool nextWasCrashPromoted,
            DateTime nextUpdatedUtc)
        {
            if (nextUpdatedUtc.Kind != DateTimeKind.Utc
                || nextUpdatedUtc < updatedUtc)
            {
                throw new ArgumentOutOfRangeException(
                    "nextUpdatedUtc",
                    "Qualification recovery time must be UTC and cannot move backwards.");
            }
            return new AxisQualificationRecoveryRecord(
                identity,
                checked(recordRevision + 1),
                nextStage,
                endpointIp,
                endpointPort,
                ownerSessionGeneration,
                axisName,
                axisReference,
                diagnosticsBuild,
                diagnosticsBootId,
                mapRevision,
                deltaRaw,
                velocityRaw,
                accelerationRaw,
                decelerationRaw,
                jerkRaw,
                toleranceRaw,
                nextHasTarget,
                nextStartPositionRaw,
                nextTargetPositionRaw,
                nextSafetyGeneration,
                nextWasCrashPromoted,
                createdUtc,
                nextUpdatedUtc,
                null);
        }

        private static void ValidateTransition(
            AxisQualificationRecoveryStage currentStage,
            AxisQualificationRecoveryStage nextStage)
        {
            if (currentStage == AxisQualificationRecoveryStage.SafeResolved
                || currentStage == nextStage
                || (int)nextStage <= (int)currentStage)
            {
                throw new InvalidOperationException(
                    "Axis qualification recovery stages must advance monotonically.");
            }

            var valid = false;
            if (currentStage
                    == AxisQualificationRecoveryStage.ArmedBeforePowerOn)
            {
                valid = nextStage
                        == AxisQualificationRecoveryStage.PowerOnAccepted
                    || nextStage
                        == AxisQualificationRecoveryStage.PowerOffAccepted
                    || nextStage
                        == AxisQualificationRecoveryStage.SafeResolved;
            }
            else if (currentStage
                == AxisQualificationRecoveryStage.PowerOnAccepted)
            {
                valid = nextStage
                        == AxisQualificationRecoveryStage.PowerOnStable
                    || nextStage
                        == AxisQualificationRecoveryStage.StopAccepted
                    || nextStage
                        == AxisQualificationRecoveryStage.PowerOffAccepted;
            }
            else if (currentStage
                == AxisQualificationRecoveryStage.PowerOnStable)
            {
                valid = nextStage
                        == AxisQualificationRecoveryStage.MovePrepared
                    || nextStage
                        == AxisQualificationRecoveryStage.StopAccepted
                    || nextStage
                        == AxisQualificationRecoveryStage.PowerOffAccepted;
            }
            else if (currentStage
                    == AxisQualificationRecoveryStage.MovePrepared
                || currentStage
                    == AxisQualificationRecoveryStage.MoveAccepted
                || currentStage
                    == AxisQualificationRecoveryStage.MoveStable)
            {
                valid = nextStage
                        == (AxisQualificationRecoveryStage)
                            ((int)currentStage + 1)
                    || nextStage
                        == AxisQualificationRecoveryStage.StopAccepted
                    || nextStage
                        == AxisQualificationRecoveryStage.PowerOffAccepted;
            }
            else if (currentStage
                == AxisQualificationRecoveryStage.StopAccepted)
            {
                valid = nextStage
                        == AxisQualificationRecoveryStage.StopStable
                    || nextStage
                        == AxisQualificationRecoveryStage.PowerOffAccepted;
            }
            else if (currentStage
                == AxisQualificationRecoveryStage.StopStable)
            {
                valid = nextStage
                    == AxisQualificationRecoveryStage.PowerOffAccepted;
            }
            else if (currentStage
                == AxisQualificationRecoveryStage.PowerOffAccepted)
            {
                valid = nextStage
                    == AxisQualificationRecoveryStage.SafeResolved;
            }

            if (!valid)
            {
                throw new InvalidOperationException(
                    "The requested Axis qualification recovery transition is invalid.");
            }
        }

        private static void ValidateStage(
            AxisQualificationRecoveryStage candidate)
        {
            if ((int)candidate
                    < (int)AxisQualificationRecoveryStage.ArmedBeforePowerOn
                || (int)candidate
                    > (int)AxisQualificationRecoveryStage.SafeResolved)
            {
                throw new ArgumentOutOfRangeException("stage");
            }
        }

        private static void ValidateInput(
            int candidateDeltaRaw,
            int candidateVelocityRaw,
            int candidateAccelerationRaw,
            int candidateDecelerationRaw,
            int candidateJerkRaw,
            int candidateToleranceRaw)
        {
            if (candidateDeltaRaw == 0)
            {
                throw new ArgumentOutOfRangeException("deltaRaw");
            }
            if (candidateVelocityRaw <= 0)
            {
                throw new ArgumentOutOfRangeException("velocityRaw");
            }
            if (candidateAccelerationRaw <= 0)
            {
                throw new ArgumentOutOfRangeException("accelerationRaw");
            }
            if (candidateDecelerationRaw <= 0)
            {
                throw new ArgumentOutOfRangeException("decelerationRaw");
            }
            if (candidateJerkRaw < 0)
            {
                throw new ArgumentOutOfRangeException("jerkRaw");
            }
            if (candidateToleranceRaw <= 0)
            {
                throw new ArgumentOutOfRangeException("toleranceRaw");
            }
        }

        private static void ValidateTarget(
            AxisQualificationRecoveryStage candidateStage,
            int candidateDeltaRaw,
            bool candidateHasTarget,
            int candidateStartPositionRaw,
            int candidateTargetPositionRaw)
        {
            if (!candidateHasTarget)
            {
                if (candidateStartPositionRaw != 0
                    || candidateTargetPositionRaw != 0)
                {
                    throw new ArgumentException(
                        "A qualification record without a target must use zero target placeholders.",
                        "hasTarget");
                }
                if (candidateStage
                        == AxisQualificationRecoveryStage.MoveAccepted
                    || candidateStage
                        == AxisQualificationRecoveryStage.MoveStable)
                {
                    throw new ArgumentException(
                        "Accepted or stable qualification motion requires an exact target.",
                        "hasTarget");
                }
                return;
            }

            if ((int)candidateStage
                < (int)AxisQualificationRecoveryStage.MovePrepared)
            {
                throw new ArgumentException(
                    "A target cannot precede the MovePrepared stage.",
                    "hasTarget");
            }
            int expectedTarget;
            try
            {
                expectedTarget = checked(
                    candidateStartPositionRaw + candidateDeltaRaw);
            }
            catch (OverflowException error)
            {
                throw new ArgumentOutOfRangeException(
                    "candidateTargetPositionRaw",
                    error.Message);
            }
            if (candidateTargetPositionRaw != expectedTarget)
            {
                throw new ArgumentException(
                    "The qualification target must equal start plus delta.",
                    "targetPositionRaw");
            }
        }

        private static void ValidateTimestamps(
            DateTime candidateCreatedUtc,
            DateTime candidateUpdatedUtc)
        {
            if (candidateCreatedUtc.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException(
                    "Qualification recovery creation time must be UTC.",
                    "createdUtc");
            }
            if (candidateUpdatedUtc.Kind != DateTimeKind.Utc
                || candidateUpdatedUtc < candidateCreatedUtc)
            {
                throw new ArgumentException(
                    "Qualification recovery update time must be UTC and monotonic.",
                    "updatedUtc");
            }
        }

        internal static void ValidateAsciiName(
            string value,
            string parameterName,
            int maximumLength)
        {
            if (string.IsNullOrWhiteSpace(value)
                || value.Length > maximumLength
                || !string.Equals(
                    value,
                    value.Trim(),
                    StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Qualification recovery names are missing or invalid.",
                    parameterName);
            }
            for (var index = 0; index < value.Length; index++)
            {
                if (value[index] < 0x20 || value[index] > 0x7e)
                {
                    throw new ArgumentException(
                        "Qualification recovery names must use printable 7-bit ASCII.",
                        parameterName);
                }
            }
        }

        internal static string NormalizeIpv4(
            string value,
            string parameterName)
        {
            string normalized;
            if (!TryNormalizeIpv4(value, out normalized))
            {
                throw new ArgumentException(
                    "Qualification recovery endpoints must be IPv4 literals.",
                    parameterName);
            }
            return normalized;
        }

        private static bool TryNormalizeIpv4(
            string value,
            out string normalized)
        {
            normalized = null;
            if (string.IsNullOrWhiteSpace(value)
                || !string.Equals(
                    value,
                    value.Trim(),
                    StringComparison.Ordinal))
            {
                return false;
            }
            IPAddress parsed;
            if (!IPAddress.TryParse(value, out parsed)
                || parsed.AddressFamily != AddressFamily.InterNetwork)
            {
                return false;
            }
            normalized = parsed.ToString();
            return true;
        }

        private static void ValidatePort(int value, string parameterName)
        {
            if (value < 1 || value > 65535)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        private static byte[] CloneBytes(byte[] source)
        {
            return source == null ? null : (byte[])source.Clone();
        }

        private static bool ByteArraysEqual(byte[] left, byte[] right)
        {
            if (left == null || right == null)
            {
                return left == null && right == null;
            }
            if (left.Length != right.Length)
            {
                return false;
            }
            var difference = 0;
            for (var index = 0; index < left.Length; index++)
            {
                difference |= left[index] ^ right[index];
            }
            return difference == 0;
        }
    }

    // This is the whole-qualification orchestration ledger above the
    // command-level Axis Power, Motion, and Axis Command recovery journals.
    // It records evidence only and intentionally exposes no mutation replay.
    internal sealed class AxisQualificationRecoveryJournal : IDisposable
    {
        internal const string JournalFileName = "journal.dat";
        internal const string LockFileName = "journal.lock";
        internal const int ChecksumLength = 32;

        private const int FormatVersion = 1;
        private const int MaximumFileLength = 32768;
        private const int MaximumTextByteLength = 1024;
        private static readonly byte[] Magic =
            Encoding.ASCII.GetBytes("ELMOAQJ1");

        private readonly object sync = new object();
        private readonly string directoryPath;
        private readonly string journalFilePath;
        private FileStream lockStream;
        private AxisQualificationRecoveryRecord currentRecord;
        private bool disposed;

        private AxisQualificationRecoveryJournal(
            string requestedDirectoryPath,
            bool deferCrashPromotion)
        {
            if (string.IsNullOrWhiteSpace(requestedDirectoryPath))
            {
                throw new ArgumentException(
                    "An Axis qualification recovery directory is required.",
                    "requestedDirectoryPath");
            }

            directoryPath = Path.GetFullPath(requestedDirectoryPath);
            journalFilePath = Path.Combine(
                directoryPath,
                JournalFileName);
            Directory.CreateDirectory(directoryPath);
            try
            {
                lockStream = new FileStream(
                    Path.Combine(directoryPath, LockFileName),
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    1,
                    FileOptions.WriteThrough);
                currentRecord = LoadRecord(journalFilePath);
                if (!deferCrashPromotion)
                {
                    PromoteVolatileStageAfterRestartCore();
                }
            }
            catch
            {
                if (lockStream != null)
                {
                    lockStream.Dispose();
                    lockStream = null;
                }
                throw;
            }
        }

        internal string DirectoryPath { get { return directoryPath; } }
        internal string JournalFilePath { get { return journalFilePath; } }

        internal AxisQualificationRecoveryRecord Current
        {
            get
            {
                lock (sync)
                {
                    ThrowIfDisposed();
                    return currentRecord == null
                        ? null
                        : currentRecord.Copy();
                }
            }
        }

        internal AxisQualificationRecoveryRecord CurrentRecord
        {
            get { return Current; }
        }

        internal bool HasActiveRecord
        {
            get
            {
                lock (sync)
                {
                    ThrowIfDisposed();
                    return currentRecord != null && currentRecord.IsActive;
                }
            }
        }

        internal static AxisQualificationRecoveryJournal Open(
            string directoryPath)
        {
            return new AxisQualificationRecoveryJournal(
                directoryPath,
                false);
        }

        internal static AxisQualificationRecoveryJournal Open(
            string directoryPath,
            bool deferCrashPromotion)
        {
            return new AxisQualificationRecoveryJournal(
                directoryPath,
                deferCrashPromotion);
        }

        internal static AxisQualificationRecoveryJournal OpenDefault()
        {
            return Open(GetDefaultDirectoryPath());
        }

        internal static AxisQualificationRecoveryJournal OpenDefault(
            bool deferCrashPromotion)
        {
            return Open(
                GetDefaultDirectoryPath(),
                deferCrashPromotion);
        }

        internal void PromoteRecoveredVolatileStage()
        {
            lock (sync)
            {
                ThrowIfDisposed();
                PromoteVolatileStageAfterRestartCore();
            }
        }

        internal static string GetDefaultDirectoryPath()
        {
            var localApplicationData = Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrWhiteSpace(localApplicationData))
            {
                throw new InvalidOperationException(
                    "Windows LocalApplicationData is unavailable.");
            }
            return Path.Combine(
                localApplicationData,
                "Elmo",
                "LasalMotionControlApiExample",
                "AxisQualificationRecoveryJournal",
                "v1");
        }

        internal AxisQualificationRecoveryRecord ArmBeforePowerOn(
            string endpointIp,
            int endpointPort,
            long ownerSessionGeneration,
            string axisName,
            ushort axisReference,
            uint diagnosticsBuild,
            uint diagnosticsBootId,
            uint mapRevision,
            int deltaRaw,
            int velocityRaw,
            int accelerationRaw,
            int decelerationRaw,
            int jerkRaw,
            int toleranceRaw,
            long safetyGeneration,
            DateTime createdUtc)
        {
            return ArmBeforePowerOn(
                Guid.NewGuid(),
                endpointIp,
                endpointPort,
                ownerSessionGeneration,
                axisName,
                axisReference,
                diagnosticsBuild,
                diagnosticsBootId,
                mapRevision,
                deltaRaw,
                velocityRaw,
                accelerationRaw,
                decelerationRaw,
                jerkRaw,
                toleranceRaw,
                safetyGeneration,
                createdUtc);
        }

        internal AxisQualificationRecoveryRecord ArmBeforePowerOn(
            Guid identity,
            string endpointIp,
            int endpointPort,
            long ownerSessionGeneration,
            string axisName,
            ushort axisReference,
            uint diagnosticsBuild,
            uint diagnosticsBootId,
            uint mapRevision,
            int deltaRaw,
            int velocityRaw,
            int accelerationRaw,
            int decelerationRaw,
            int jerkRaw,
            int toleranceRaw,
            long safetyGeneration,
            DateTime createdUtc)
        {
            lock (sync)
            {
                ThrowIfDisposed();
                if (currentRecord != null && currentRecord.IsActive)
                {
                    throw new InvalidOperationException(
                        "An unresolved Axis qualification recovery record already exists.");
                }
                if (currentRecord != null
                    && currentRecord.Identity == identity)
                {
                    throw new InvalidOperationException(
                        "A resolved Axis qualification recovery identity cannot be reused.");
                }

                var armed = new AxisQualificationRecoveryRecord(
                    identity,
                    1,
                    AxisQualificationRecoveryStage.ArmedBeforePowerOn,
                    endpointIp,
                    endpointPort,
                    ownerSessionGeneration,
                    axisName,
                    axisReference,
                    diagnosticsBuild,
                    diagnosticsBootId,
                    mapRevision,
                    deltaRaw,
                    velocityRaw,
                    accelerationRaw,
                    decelerationRaw,
                    jerkRaw,
                    toleranceRaw,
                    false,
                    0,
                    0,
                    safetyGeneration,
                    false,
                    createdUtc,
                    createdUtc,
                    null);
                currentRecord = PersistRecord(armed);
                return currentRecord.Copy();
            }
        }

        internal AxisQualificationRecoveryRecord MarkPowerOnAccepted(
            AxisQualificationRecoveryRecord expectedCurrent,
            DateTime updatedUtc)
        {
            return Transition(
                expectedCurrent,
                AxisQualificationRecoveryStage.PowerOnAccepted,
                updatedUtc);
        }

        internal AxisQualificationRecoveryRecord MarkPowerOnStable(
            AxisQualificationRecoveryRecord expectedCurrent,
            DateTime updatedUtc)
        {
            return Transition(
                expectedCurrent,
                AxisQualificationRecoveryStage.PowerOnStable,
                updatedUtc);
        }

        internal AxisQualificationRecoveryRecord PrepareMove(
            AxisQualificationRecoveryRecord expectedCurrent,
            int startPositionRaw,
            int targetPositionRaw,
            DateTime updatedUtc)
        {
            if (expectedCurrent == null)
            {
                throw new ArgumentNullException("expectedCurrent");
            }
            lock (sync)
            {
                ThrowIfDisposed();
                RequireExactCurrent(expectedCurrent);
                var prepared = currentRecord.PrepareMove(
                    startPositionRaw,
                    targetPositionRaw,
                    updatedUtc);
                currentRecord = PersistRecord(prepared);
                return currentRecord.Copy();
            }
        }

        internal AxisQualificationRecoveryRecord MarkMoveAccepted(
            AxisQualificationRecoveryRecord expectedCurrent,
            DateTime updatedUtc)
        {
            return Transition(
                expectedCurrent,
                AxisQualificationRecoveryStage.MoveAccepted,
                updatedUtc);
        }

        internal AxisQualificationRecoveryRecord MarkMoveStable(
            AxisQualificationRecoveryRecord expectedCurrent,
            DateTime updatedUtc)
        {
            return Transition(
                expectedCurrent,
                AxisQualificationRecoveryStage.MoveStable,
                updatedUtc);
        }

        internal AxisQualificationRecoveryRecord MarkStopAccepted(
            AxisQualificationRecoveryRecord expectedCurrent,
            long safetyGeneration,
            DateTime updatedUtc)
        {
            return TransitionSafety(
                expectedCurrent,
                AxisQualificationRecoveryStage.StopAccepted,
                safetyGeneration,
                updatedUtc);
        }

        internal AxisQualificationRecoveryRecord MarkStopStable(
            AxisQualificationRecoveryRecord expectedCurrent,
            DateTime updatedUtc)
        {
            return Transition(
                expectedCurrent,
                AxisQualificationRecoveryStage.StopStable,
                updatedUtc);
        }

        internal AxisQualificationRecoveryRecord MarkPowerOffAccepted(
            AxisQualificationRecoveryRecord expectedCurrent,
            long safetyGeneration,
            DateTime updatedUtc)
        {
            return TransitionSafety(
                expectedCurrent,
                AxisQualificationRecoveryStage.PowerOffAccepted,
                safetyGeneration,
                updatedUtc);
        }

        internal AxisQualificationRecoveryRecord ResolveSafe(
            AxisQualificationRecoveryRecord expectedCurrent,
            DateTime updatedUtc)
        {
            return Transition(
                expectedCurrent,
                AxisQualificationRecoveryStage.SafeResolved,
                updatedUtc);
        }

        internal RecoveryJournalSourceEvidence
            CaptureActiveRetirementEvidence()
        {
            lock (sync)
            {
                ThrowIfDisposed();
                return CaptureActiveRetirementEvidenceCore();
            }
        }

        internal AxisQualificationRecoveryRecord ResolveOperatorRetirement(
            RecoveryJournalSourceEvidence expectedEvidence,
            RecoveryRecordRetirementDecision committedDecision,
            DateTime updatedUtc)
        {
            if (expectedEvidence == null)
            {
                throw new ArgumentNullException("expectedEvidence");
            }
            if (committedDecision == null)
            {
                throw new ArgumentNullException("committedDecision");
            }
            if (!committedDecision.IsDurablyCommitted)
            {
                throw new InvalidOperationException(
                    "Axis qualification retirement requires a durably committed ledger decision.");
            }

            lock (sync)
            {
                ThrowIfDisposed();
                if (!committedDecision.MatchesSourceEvidence(
                        expectedEvidence))
                {
                    throw new InvalidOperationException(
                        "The retirement decision does not match the expected Axis qualification evidence.");
                }
                var currentEvidence =
                    CaptureActiveRetirementEvidenceCore();
                if (!expectedEvidence.ExactSourceEquals(currentEvidence)
                    || !committedDecision.MatchesSourceEvidence(
                        currentEvidence))
                {
                    throw new InvalidOperationException(
                        "Axis qualification recovery changed after operator confirmation.");
                }

                var resolved = currentRecord.ResolveOperatorRetirement(
                    updatedUtc);
                currentRecord = PersistRecord(resolved);
                return currentRecord.Copy();
            }
        }

        public void Dispose()
        {
            lock (sync)
            {
                if (disposed)
                {
                    return;
                }
                disposed = true;
                if (lockStream != null)
                {
                    lockStream.Dispose();
                    lockStream = null;
                }
            }
        }

        private AxisQualificationRecoveryRecord Transition(
            AxisQualificationRecoveryRecord expectedCurrent,
            AxisQualificationRecoveryStage nextStage,
            DateTime updatedUtc)
        {
            if (expectedCurrent == null)
            {
                throw new ArgumentNullException("expectedCurrent");
            }
            lock (sync)
            {
                ThrowIfDisposed();
                RequireExactCurrent(expectedCurrent);
                var transitioned = currentRecord.TransitionTo(
                    nextStage,
                    updatedUtc);
                currentRecord = PersistRecord(transitioned);
                return currentRecord.Copy();
            }
        }

        private AxisQualificationRecoveryRecord TransitionSafety(
            AxisQualificationRecoveryRecord expectedCurrent,
            AxisQualificationRecoveryStage nextStage,
            long safetyGeneration,
            DateTime updatedUtc)
        {
            if (expectedCurrent == null)
            {
                throw new ArgumentNullException("expectedCurrent");
            }
            lock (sync)
            {
                ThrowIfDisposed();
                RequireExactCurrent(expectedCurrent);
                var transitioned = currentRecord.TransitionSafety(
                    nextStage,
                    safetyGeneration,
                    updatedUtc);
                currentRecord = PersistRecord(transitioned);
                return currentRecord.Copy();
            }
        }

        private void RequireExactCurrent(
            AxisQualificationRecoveryRecord expectedCurrent)
        {
            if (currentRecord == null
                || !currentRecord.ExactEquals(expectedCurrent))
            {
                throw new InvalidOperationException(
                    "The expected Axis qualification recovery record is not the exact current durable revision.");
            }
        }

        private void PromoteVolatileStageAfterRestartCore()
        {
            if (currentRecord == null || !currentRecord.IsActive)
            {
                return;
            }
            AxisQualificationRecoveryStage nextStage;
            if (currentRecord.Stage
                == AxisQualificationRecoveryStage.ArmedBeforePowerOn)
            {
                nextStage =
                    AxisQualificationRecoveryStage.PowerOnAccepted;
            }
            else if (currentRecord.Stage
                == AxisQualificationRecoveryStage.MovePrepared)
            {
                nextStage = AxisQualificationRecoveryStage.MoveAccepted;
            }
            else
            {
                return;
            }

            var promotedUtc = DateTime.UtcNow;
            if (promotedUtc < currentRecord.UpdatedUtc)
            {
                promotedUtc = currentRecord.UpdatedUtc;
            }
            currentRecord = PersistRecord(
                currentRecord.PromoteCrash(nextStage, promotedUtc));
        }

        private RecoveryJournalSourceEvidence
            CaptureActiveRetirementEvidenceCore()
        {
            if (currentRecord == null || !currentRecord.IsActive)
            {
                throw new InvalidOperationException(
                    "No active Axis qualification recovery record exists for retirement.");
            }
            var originalBytes = ReadAllJournalBytes();
            var diskRecord = DeserializeRecord(originalBytes);
            if (!currentRecord.ExactEquals(diskRecord))
            {
                throw new InvalidDataException(
                    "Axis qualification recovery memory state does not match durable source bytes.");
            }

            return new RecoveryJournalSourceEvidence(
                RecoveryRecordOwner.AxisQualification,
                diskRecord.Identity,
                (int)diskRecord.Stage,
                diskRecord.CreatedUtc,
                diskRecord.UpdatedUtc,
                diskRecord.EndpointIp,
                diskRecord.EndpointPort,
                diskRecord.DiagnosticsBuild,
                diskRecord.DiagnosticsBootId,
                diskRecord.MapRevision,
                "Axis",
                diskRecord.AxisName,
                diskRecord.AxisReference,
                "Qualification",
                BuildRetirementFingerprint(diskRecord),
                originalBytes);
        }

        private static string BuildRetirementFingerprint(
            AxisQualificationRecoveryRecord record)
        {
            var builder = new StringBuilder();
            builder.Append("Revision=");
            builder.Append(record.RecordRevision.ToString(
                CultureInfo.InvariantCulture));
            builder.Append(";SessionGeneration=");
            builder.Append(record.OwnerSessionGeneration.ToString(
                CultureInfo.InvariantCulture));
            builder.Append(";Delta=");
            builder.Append(record.DeltaRaw.ToString(
                CultureInfo.InvariantCulture));
            builder.Append(";Velocity=");
            builder.Append(record.VelocityRaw.ToString(
                CultureInfo.InvariantCulture));
            builder.Append(";Acceleration=");
            builder.Append(record.AccelerationRaw.ToString(
                CultureInfo.InvariantCulture));
            builder.Append(";Deceleration=");
            builder.Append(record.DecelerationRaw.ToString(
                CultureInfo.InvariantCulture));
            builder.Append(";Jerk=");
            builder.Append(record.JerkRaw.ToString(
                CultureInfo.InvariantCulture));
            builder.Append(";Tolerance=");
            builder.Append(record.ToleranceRaw.ToString(
                CultureInfo.InvariantCulture));
            builder.Append(";HasTarget=");
            builder.Append(record.HasTarget ? "true" : "false");
            builder.Append(";Start=");
            builder.Append(record.StartPositionRaw.ToString(
                CultureInfo.InvariantCulture));
            builder.Append(";Target=");
            builder.Append(record.TargetPositionRaw.ToString(
                CultureInfo.InvariantCulture));
            builder.Append(";SafetyGeneration=");
            builder.Append(record.SafetyGeneration.ToString(
                CultureInfo.InvariantCulture));
            builder.Append(";CrashPromoted=");
            builder.Append(record.WasCrashPromoted ? "true" : "false");
            return builder.ToString();
        }

        private byte[] ReadAllJournalBytes()
        {
            using (var stream = new FileStream(
                journalFilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read))
            {
                return ReadBoundedBytes(stream);
            }
        }

        private AxisQualificationRecoveryRecord PersistRecord(
            AxisQualificationRecoveryRecord record)
        {
            var bytes = SerializeRecord(record);
            var persisted = record.WithChecksum(CopyChecksum(bytes));
            var temporaryPath = Path.Combine(
                directoryPath,
                JournalFileName
                    + "."
                    + Guid.NewGuid().ToString("N")
                    + ".tmp");
            var temporaryExists = false;
            try
            {
                using (var stream = new FileStream(
                    temporaryPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    4096,
                    FileOptions.WriteThrough))
                {
                    temporaryExists = true;
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }
                if (File.Exists(journalFilePath))
                {
                    File.Replace(
                        temporaryPath,
                        journalFilePath,
                        null,
                        true);
                }
                else
                {
                    File.Move(temporaryPath, journalFilePath);
                }
                temporaryExists = false;
                return persisted;
            }
            finally
            {
                if (temporaryExists && File.Exists(temporaryPath))
                {
                    try
                    {
                        File.Delete(temporaryPath);
                    }
                    catch
                    {
                        // Preserve the primary persistence failure.
                    }
                }
            }
        }

        private static AxisQualificationRecoveryRecord LoadRecord(
            string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }
            using (var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                4096,
                FileOptions.SequentialScan))
            {
                return DeserializeRecord(ReadBoundedBytes(stream));
            }
        }

        private static byte[] ReadBoundedBytes(Stream stream)
        {
            if (stream.Length < Magic.Length + 8 + ChecksumLength
                || stream.Length > MaximumFileLength)
            {
                throw new InvalidDataException(
                    "Axis qualification recovery journal length is invalid.");
            }
            var bytes = new byte[checked((int)stream.Length)];
            var offset = 0;
            while (offset < bytes.Length)
            {
                var read = stream.Read(bytes, offset, bytes.Length - offset);
                if (read <= 0)
                {
                    throw new InvalidDataException(
                        "Axis qualification recovery journal is incomplete.");
                }
                offset += read;
            }
            return bytes;
        }

        private static byte[] SerializeRecord(
            AxisQualificationRecoveryRecord record)
        {
            byte[] payload;
            using (var payloadStream = new MemoryStream())
            using (var writer = new BinaryWriter(
                payloadStream,
                Encoding.ASCII,
                true))
            {
                writer.Write(record.Identity.ToByteArray());
                writer.Write(record.RecordRevision);
                writer.Write((int)record.Stage);
                writer.Write(record.EndpointPort);
                writer.Write(record.OwnerSessionGeneration);
                writer.Write(record.AxisReference);
                writer.Write(record.DiagnosticsBuild);
                writer.Write(record.DiagnosticsBootId);
                writer.Write(record.MapRevision);
                writer.Write(record.DeltaRaw);
                writer.Write(record.VelocityRaw);
                writer.Write(record.AccelerationRaw);
                writer.Write(record.DecelerationRaw);
                writer.Write(record.JerkRaw);
                writer.Write(record.ToleranceRaw);
                writer.Write(record.HasTarget ? (byte)1 : (byte)0);
                writer.Write(record.StartPositionRaw);
                writer.Write(record.TargetPositionRaw);
                writer.Write(record.SafetyGeneration);
                writer.Write(record.WasCrashPromoted ? (byte)1 : (byte)0);
                writer.Write(record.CreatedUtc.Ticks);
                writer.Write(record.UpdatedUtc.Ticks);
                WriteText(writer, record.EndpointIp);
                WriteText(writer, record.AxisName);
                writer.Flush();
                payload = payloadStream.ToArray();
            }

            byte[] prefix;
            using (var fileStream = new MemoryStream())
            using (var writer = new BinaryWriter(
                fileStream,
                Encoding.ASCII,
                true))
            {
                writer.Write(Magic);
                writer.Write(FormatVersion);
                writer.Write(payload.Length);
                writer.Write(payload);
                writer.Flush();
                prefix = fileStream.ToArray();
            }

            byte[] checksum;
            using (var sha256 = SHA256.Create())
            {
                checksum = sha256.ComputeHash(prefix);
            }
            var result = new byte[prefix.Length + checksum.Length];
            Buffer.BlockCopy(prefix, 0, result, 0, prefix.Length);
            Buffer.BlockCopy(
                checksum,
                0,
                result,
                prefix.Length,
                checksum.Length);
            return result;
        }

        private static AxisQualificationRecoveryRecord DeserializeRecord(
            byte[] bytes)
        {
            try
            {
                return DeserializeRecordCore(bytes);
            }
            catch (InvalidDataException)
            {
                throw;
            }
            catch (EndOfStreamException error)
            {
                throw InvalidRecord(error);
            }
            catch (ArgumentException error)
            {
                throw InvalidRecord(error);
            }
            catch (OverflowException error)
            {
                throw InvalidRecord(error);
            }
        }

        private static AxisQualificationRecoveryRecord DeserializeRecordCore(
            byte[] bytes)
        {
            if (bytes == null
                || bytes.Length < Magic.Length + 8 + ChecksumLength
                || bytes.Length > MaximumFileLength)
            {
                throw new InvalidDataException(
                    "Axis qualification recovery journal length is invalid.");
            }
            var checksumOffset = bytes.Length - ChecksumLength;
            byte[] computed;
            using (var sha256 = SHA256.Create())
            {
                computed = sha256.ComputeHash(bytes, 0, checksumOffset);
            }
            if (!ChecksumEquals(computed, bytes, checksumOffset))
            {
                throw new InvalidDataException(
                    "Axis qualification recovery journal checksum is invalid.");
            }

            byte[] payload;
            using (var fileStream = new MemoryStream(
                bytes,
                0,
                checksumOffset,
                false))
            using (var reader = new BinaryReader(
                fileStream,
                Encoding.ASCII,
                true))
            {
                if (!ByteArraysEqual(Magic, reader.ReadBytes(Magic.Length)))
                {
                    throw new InvalidDataException(
                        "Axis qualification recovery magic is invalid.");
                }
                if (reader.ReadInt32() != FormatVersion)
                {
                    throw new InvalidDataException(
                        "Axis qualification recovery version is unsupported.");
                }
                var payloadLength = reader.ReadInt32();
                if (payloadLength <= 0
                    || payloadLength
                        != checksumOffset - (Magic.Length + 8))
                {
                    throw new InvalidDataException(
                        "Axis qualification recovery payload length is invalid.");
                }
                payload = reader.ReadBytes(payloadLength);
                if (payload.Length != payloadLength
                    || fileStream.Position != checksumOffset)
                {
                    throw new InvalidDataException(
                        "Axis qualification recovery payload is incomplete.");
                }
            }

            var checksum = CopyChecksum(bytes);
            using (var stream = new MemoryStream(payload, false))
            using (var reader = new BinaryReader(
                stream,
                Encoding.ASCII,
                true))
            {
                var identityBytes = reader.ReadBytes(16);
                if (identityBytes.Length != 16)
                {
                    throw new InvalidDataException(
                        "Axis qualification recovery identity is incomplete.");
                }
                var identity = new Guid(identityBytes);
                var revision = reader.ReadInt64();
                var stage =
                    (AxisQualificationRecoveryStage)reader.ReadInt32();
                var endpointPort = reader.ReadInt32();
                var ownerSessionGeneration = reader.ReadInt64();
                var axisReference = reader.ReadUInt16();
                var diagnosticsBuild = reader.ReadUInt32();
                var diagnosticsBootId = reader.ReadUInt32();
                var mapRevision = reader.ReadUInt32();
                var deltaRaw = reader.ReadInt32();
                var velocityRaw = reader.ReadInt32();
                var accelerationRaw = reader.ReadInt32();
                var decelerationRaw = reader.ReadInt32();
                var jerkRaw = reader.ReadInt32();
                var toleranceRaw = reader.ReadInt32();
                var hasTargetValue = reader.ReadByte();
                if (hasTargetValue > 1)
                {
                    throw new InvalidDataException(
                        "Axis qualification target flag is invalid.");
                }
                var startPositionRaw = reader.ReadInt32();
                var targetPositionRaw = reader.ReadInt32();
                var safetyGeneration = reader.ReadInt64();
                var wasCrashPromotedValue = reader.ReadByte();
                if (wasCrashPromotedValue > 1)
                {
                    throw new InvalidDataException(
                        "Axis qualification crash flag is invalid.");
                }
                var createdUtc = new DateTime(
                    reader.ReadInt64(),
                    DateTimeKind.Utc);
                var updatedUtc = new DateTime(
                    reader.ReadInt64(),
                    DateTimeKind.Utc);
                var endpointIp = ReadText(reader);
                var axisName = ReadText(reader);
                if (stream.Position != stream.Length)
                {
                    throw new InvalidDataException(
                        "Axis qualification recovery has trailing payload data.");
                }

                var record = new AxisQualificationRecoveryRecord(
                    identity,
                    revision,
                    stage,
                    endpointIp,
                    endpointPort,
                    ownerSessionGeneration,
                    axisName,
                    axisReference,
                    diagnosticsBuild,
                    diagnosticsBootId,
                    mapRevision,
                    deltaRaw,
                    velocityRaw,
                    accelerationRaw,
                    decelerationRaw,
                    jerkRaw,
                    toleranceRaw,
                    hasTargetValue == 1,
                    startPositionRaw,
                    targetPositionRaw,
                    safetyGeneration,
                    wasCrashPromotedValue == 1,
                    createdUtc,
                    updatedUtc,
                    checksum);
                if (!string.Equals(
                        endpointIp,
                        record.EndpointIp,
                        StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        "Axis qualification endpoint is not canonical.");
                }
                return record;
            }
        }

        private static void WriteText(BinaryWriter writer, string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            if (bytes.Length < 1
                || bytes.Length > MaximumTextByteLength)
            {
                throw new InvalidOperationException(
                    "Axis qualification recovery text encoding is invalid.");
            }
            writer.Write(bytes.Length);
            writer.Write(bytes);
        }

        private static string ReadText(BinaryReader reader)
        {
            var length = reader.ReadInt32();
            if (length < 1 || length > MaximumTextByteLength)
            {
                throw new InvalidDataException(
                    "Axis qualification recovery text length is invalid.");
            }
            var bytes = reader.ReadBytes(length);
            if (bytes.Length != length)
            {
                throw new InvalidDataException(
                    "Axis qualification recovery text is incomplete.");
            }
            for (var index = 0; index < bytes.Length; index++)
            {
                if (bytes[index] < 0x20 || bytes[index] > 0x7e)
                {
                    throw new InvalidDataException(
                        "Axis qualification recovery text is not printable ASCII.");
                }
            }
            return Encoding.ASCII.GetString(bytes);
        }

        private static byte[] CopyChecksum(byte[] bytes)
        {
            var result = new byte[ChecksumLength];
            Buffer.BlockCopy(
                bytes,
                bytes.Length - ChecksumLength,
                result,
                0,
                ChecksumLength);
            return result;
        }

        private static bool ChecksumEquals(
            byte[] expected,
            byte[] actual,
            int actualOffset)
        {
            if (expected == null
                || expected.Length != ChecksumLength
                || actual == null
                || actualOffset < 0
                || actual.Length - actualOffset != ChecksumLength)
            {
                return false;
            }
            var difference = 0;
            for (var index = 0; index < ChecksumLength; index++)
            {
                difference |= expected[index] ^ actual[actualOffset + index];
            }
            return difference == 0;
        }

        private static bool ByteArraysEqual(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }
            var difference = 0;
            for (var index = 0; index < left.Length; index++)
            {
                difference |= left[index] ^ right[index];
            }
            return difference == 0;
        }

        private static InvalidDataException InvalidRecord(Exception error)
        {
            return new InvalidDataException(
                "Axis qualification recovery journal record is invalid.",
                error);
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(
                    "AxisQualificationRecoveryJournal");
            }
        }
    }
}
