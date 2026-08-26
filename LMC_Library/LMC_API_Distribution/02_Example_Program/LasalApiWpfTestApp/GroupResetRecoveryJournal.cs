using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace LasalMotionControlApiExample
{
    internal enum GroupResetRecoveryState
    {
        ArmedBeforeDispatch = 1,
        AcceptedAwaitingProof = 2,
        RecoveryRequired = 3,
        Resolved = 4
    }

    internal enum GroupResetRecoveryPriorOutcome
    {
        NotAttempted = 0,
        Accepted = 1,
        OutcomeUncertain = 2
    }

    internal sealed class GroupResetRecoveryMember
    {
        private const int MaximumNameLength = 256;

        private readonly string axisName;
        private readonly ushort axisReference;
        private readonly ushort deviceId;

        internal GroupResetRecoveryMember(
            string axisName,
            ushort axisReference,
            ushort deviceId)
        {
            GroupResetRecoveryRecord.ValidateName(
                axisName,
                "axisName",
                MaximumNameLength);
            if (axisReference == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "axisReference",
                    "A Group Reset member reference must be non-zero.");
            }

            this.axisName = axisName;
            this.axisReference = axisReference;
            this.deviceId = deviceId;
        }

        internal string AxisName
        {
            get { return axisName; }
        }

        internal ushort AxisReference
        {
            get { return axisReference; }
        }

        internal ushort DeviceId
        {
            get { return deviceId; }
        }

        internal GroupResetRecoveryMember Copy()
        {
            return new GroupResetRecoveryMember(
                axisName,
                axisReference,
                deviceId);
        }

        internal bool ExactEquals(GroupResetRecoveryMember candidate)
        {
            return candidate != null
                && string.Equals(
                    axisName,
                    candidate.axisName,
                    StringComparison.Ordinal)
                && axisReference == candidate.axisReference
                && deviceId == candidate.deviceId;
        }
    }

    internal sealed class GroupResetRecoveryRecord
    {
        private const int MaximumGroupNameLength = 256;
        private const int MaximumStableSampleCount = 100;

        private readonly Guid identity;
        private readonly long recordRevision;
        private readonly GroupResetRecoveryState state;
        private readonly GroupResetRecoveryPriorOutcome priorOutcome;
        private readonly string plcIp;
        private readonly int plcTcpPort;
        private readonly string localIpv4;
        private readonly int callbackUdpPort;
        private readonly uint diagnosticsBuild;
        private readonly uint diagnosticsBootId;
        private readonly uint mapRevision;
        private readonly string groupName;
        private readonly ushort groupReference;
        private readonly long ownerSessionGeneration;
        private readonly GroupResetRecoveryMember[] members;
        private readonly int requiredStableSampleCount;
        private readonly DateTime createdUtc;
        private readonly DateTime updatedUtc;
        private readonly byte[] checksum;

        internal GroupResetRecoveryRecord(
            Guid identity,
            long recordRevision,
            GroupResetRecoveryState state,
            GroupResetRecoveryPriorOutcome priorOutcome,
            string plcIp,
            int plcTcpPort,
            string localIpv4,
            int callbackUdpPort,
            uint diagnosticsBuild,
            uint diagnosticsBootId,
            uint mapRevision,
            string groupName,
            ushort groupReference,
            long ownerSessionGeneration,
            GroupResetRecoveryMember[] members,
            int requiredStableSampleCount,
            DateTime createdUtc,
            DateTime updatedUtc,
            byte[] checksum)
        {
            if (identity == Guid.Empty)
            {
                throw new ArgumentException(
                    "Group Reset recovery identity cannot be empty.",
                    "identity");
            }
            if (recordRevision < 1)
            {
                throw new ArgumentOutOfRangeException(
                    "recordRevision",
                    "Group Reset recovery revision must be positive.");
            }

            ValidateStateAndOutcome(state, priorOutcome);
            var normalizedPlcIp = NormalizeIpv4(plcIp, "plcIp");
            ValidatePort(plcTcpPort, "plcTcpPort");
            var normalizedLocalIpv4 = NormalizeIpv4(
                localIpv4,
                "localIpv4");
            ValidatePort(callbackUdpPort, "callbackUdpPort");
            if (diagnosticsBuild == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "diagnosticsBuild",
                    "Recovery identity requires a non-zero diagnostics build.");
            }
            if (diagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "diagnosticsBootId",
                    "Recovery identity requires a non-zero diagnostics BootId.");
            }
            if (mapRevision == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "mapRevision",
                    "Recovery identity requires a non-zero map revision.");
            }

            ValidateName(groupName, "groupName", MaximumGroupNameLength);
            if (groupReference == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "groupReference",
                    "The Group Reset group reference must be non-zero.");
            }
            if (ownerSessionGeneration < 1)
            {
                throw new ArgumentOutOfRangeException(
                    "ownerSessionGeneration",
                    "The owner session generation must be positive.");
            }

            var copiedMembers = CopyAndValidateMembers(members);
            if (requiredStableSampleCount < 1
                || requiredStableSampleCount > MaximumStableSampleCount)
            {
                throw new ArgumentOutOfRangeException(
                    "requiredStableSampleCount",
                    "Required stable sample count must be between 1 and 100.");
            }
            ValidateTimestamps(createdUtc, updatedUtc);

            if (checksum != null
                && checksum.Length != GroupResetRecoveryJournal.ChecksumLength)
            {
                throw new ArgumentException(
                    "A Group Reset recovery checksum must contain 32 bytes.",
                    "checksum");
            }

            this.identity = identity;
            this.recordRevision = recordRevision;
            this.state = state;
            this.priorOutcome = priorOutcome;
            this.plcIp = normalizedPlcIp;
            this.plcTcpPort = plcTcpPort;
            this.localIpv4 = normalizedLocalIpv4;
            this.callbackUdpPort = callbackUdpPort;
            this.diagnosticsBuild = diagnosticsBuild;
            this.diagnosticsBootId = diagnosticsBootId;
            this.mapRevision = mapRevision;
            this.groupName = groupName;
            this.groupReference = groupReference;
            this.ownerSessionGeneration = ownerSessionGeneration;
            this.members = copiedMembers;
            this.requiredStableSampleCount = requiredStableSampleCount;
            this.createdUtc = createdUtc;
            this.updatedUtc = updatedUtc;
            this.checksum = CloneBytes(checksum);
        }

        internal Guid Identity { get { return identity; } }
        internal long RecordRevision { get { return recordRevision; } }
        internal GroupResetRecoveryState State { get { return state; } }
        internal GroupResetRecoveryPriorOutcome PriorOutcome
        {
            get { return priorOutcome; }
        }
        internal string PlcIp { get { return plcIp; } }
        internal int PlcTcpPort { get { return plcTcpPort; } }
        internal string LocalIpv4 { get { return localIpv4; } }
        internal int CallbackUdpPort { get { return callbackUdpPort; } }
        internal uint DiagnosticsBuild { get { return diagnosticsBuild; } }
        internal uint DiagnosticsBootId { get { return diagnosticsBootId; } }
        internal uint MapRevision { get { return mapRevision; } }
        internal string GroupName { get { return groupName; } }
        internal ushort GroupReference { get { return groupReference; } }
        internal long OwnerSessionGeneration
        {
            get { return ownerSessionGeneration; }
        }
        internal int RequiredStableSampleCount
        {
            get { return requiredStableSampleCount; }
        }
        internal DateTime CreatedUtc { get { return createdUtc; } }
        internal DateTime UpdatedUtc { get { return updatedUtc; } }
        internal bool IsActive
        {
            get { return state != GroupResetRecoveryState.Resolved; }
        }
        internal GroupResetRecoveryMember[] Members
        {
            get { return CopyMembers(members); }
        }
        internal byte[] Checksum
        {
            get { return CloneBytes(checksum); }
        }

        internal bool MatchesEndpoint(
            string candidatePlcIp,
            int candidatePlcTcpPort,
            string candidateLocalIpv4,
            int candidateCallbackUdpPort)
        {
            string normalizedPlcIp;
            string normalizedLocalIpv4;
            return TryNormalizeIpv4(candidatePlcIp, out normalizedPlcIp)
                && TryNormalizeIpv4(
                    candidateLocalIpv4,
                    out normalizedLocalIpv4)
                && string.Equals(
                    plcIp,
                    normalizedPlcIp,
                    StringComparison.Ordinal)
                && plcTcpPort == candidatePlcTcpPort
                && string.Equals(
                    localIpv4,
                    normalizedLocalIpv4,
                    StringComparison.Ordinal)
                && callbackUdpPort == candidateCallbackUdpPort;
        }

        internal bool MatchesRecoveryIdentity(
            string candidatePlcIp,
            int candidatePlcTcpPort,
            string candidateLocalIpv4,
            int candidateCallbackUdpPort,
            uint candidateDiagnosticsBuild,
            uint candidateDiagnosticsBootId,
            uint candidateMapRevision,
            string candidateGroupName,
            ushort candidateGroupReference,
            long candidateOwnerSessionGeneration,
            GroupResetRecoveryMember[] candidateMembers,
            int candidateRequiredStableSampleCount)
        {
            return MatchesEndpoint(
                    candidatePlcIp,
                    candidatePlcTcpPort,
                    candidateLocalIpv4,
                    candidateCallbackUdpPort)
                && diagnosticsBuild == candidateDiagnosticsBuild
                && diagnosticsBootId == candidateDiagnosticsBootId
                && mapRevision == candidateMapRevision
                && string.Equals(
                    groupName,
                    candidateGroupName,
                    StringComparison.Ordinal)
                && groupReference == candidateGroupReference
                && ownerSessionGeneration
                    == candidateOwnerSessionGeneration
                && requiredStableSampleCount
                    == candidateRequiredStableSampleCount
                && MembersExactEqual(members, candidateMembers);
        }

        internal GroupResetRecoveryRecord Copy()
        {
            return new GroupResetRecoveryRecord(
                identity,
                recordRevision,
                state,
                priorOutcome,
                plcIp,
                plcTcpPort,
                localIpv4,
                callbackUdpPort,
                diagnosticsBuild,
                diagnosticsBootId,
                mapRevision,
                groupName,
                groupReference,
                ownerSessionGeneration,
                members,
                requiredStableSampleCount,
                createdUtc,
                updatedUtc,
                checksum);
        }

        internal GroupResetRecoveryRecord WithChecksum(byte[] nextChecksum)
        {
            return new GroupResetRecoveryRecord(
                identity,
                recordRevision,
                state,
                priorOutcome,
                plcIp,
                plcTcpPort,
                localIpv4,
                callbackUdpPort,
                diagnosticsBuild,
                diagnosticsBootId,
                mapRevision,
                groupName,
                groupReference,
                ownerSessionGeneration,
                members,
                requiredStableSampleCount,
                createdUtc,
                updatedUtc,
                nextChecksum);
        }

        internal GroupResetRecoveryRecord TransitionTo(
            GroupResetRecoveryState nextState,
            GroupResetRecoveryPriorOutcome nextOutcome,
            DateTime nextUpdatedUtc)
        {
            ValidateTransition(state, priorOutcome, nextState, nextOutcome);
            if (nextUpdatedUtc.Kind != DateTimeKind.Utc
                || nextUpdatedUtc < updatedUtc)
            {
                throw new ArgumentOutOfRangeException(
                    "nextUpdatedUtc",
                    "Recovery transition time must be UTC and cannot move backwards.");
            }

            return new GroupResetRecoveryRecord(
                identity,
                checked(recordRevision + 1),
                nextState,
                nextOutcome,
                plcIp,
                plcTcpPort,
                localIpv4,
                callbackUdpPort,
                diagnosticsBuild,
                diagnosticsBootId,
                mapRevision,
                groupName,
                groupReference,
                ownerSessionGeneration,
                members,
                requiredStableSampleCount,
                createdUtc,
                nextUpdatedUtc,
                null);
        }

        internal bool ExactEquals(GroupResetRecoveryRecord candidate)
        {
            return candidate != null
                && identity == candidate.identity
                && recordRevision == candidate.recordRevision
                && state == candidate.state
                && priorOutcome == candidate.priorOutcome
                && string.Equals(
                    plcIp,
                    candidate.plcIp,
                    StringComparison.Ordinal)
                && plcTcpPort == candidate.plcTcpPort
                && string.Equals(
                    localIpv4,
                    candidate.localIpv4,
                    StringComparison.Ordinal)
                && callbackUdpPort == candidate.callbackUdpPort
                && diagnosticsBuild == candidate.diagnosticsBuild
                && diagnosticsBootId == candidate.diagnosticsBootId
                && mapRevision == candidate.mapRevision
                && string.Equals(
                    groupName,
                    candidate.groupName,
                    StringComparison.Ordinal)
                && groupReference == candidate.groupReference
                && ownerSessionGeneration
                    == candidate.ownerSessionGeneration
                && requiredStableSampleCount
                    == candidate.requiredStableSampleCount
                && createdUtc == candidate.createdUtc
                && updatedUtc == candidate.updatedUtc
                && MembersExactEqual(members, candidate.members)
                && ByteArraysEqual(checksum, candidate.checksum);
        }

        internal static void ValidateName(
            string value,
            string parameterName,
            int maximumLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "Recovery identity names cannot be empty.",
                    parameterName);
            }
            if (!string.Equals(
                    value,
                    value.Trim(),
                    StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Recovery identity names cannot have surrounding whitespace.",
                    parameterName);
            }
            if (value.Length > maximumLength)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "Recovery identity name is too long.");
            }

            for (var index = 0; index < value.Length; index++)
            {
                var character = value[index];
                if (character < 0x20 || character > 0x7e)
                {
                    throw new ArgumentException(
                        "Recovery identity names must use printable 7-bit ASCII.",
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
                    "Recovery endpoint addresses must be valid IPv4 values.",
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
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "Recovery endpoint ports must be from 1 through 65535.");
            }
        }

        private static void ValidateStateAndOutcome(
            GroupResetRecoveryState state,
            GroupResetRecoveryPriorOutcome outcome)
        {
            if (state != GroupResetRecoveryState.ArmedBeforeDispatch
                && state
                    != GroupResetRecoveryState.AcceptedAwaitingProof
                && state != GroupResetRecoveryState.RecoveryRequired
                && state != GroupResetRecoveryState.Resolved)
            {
                throw new ArgumentOutOfRangeException("state");
            }
            if (outcome != GroupResetRecoveryPriorOutcome.NotAttempted
                && outcome != GroupResetRecoveryPriorOutcome.Accepted
                && outcome
                    != GroupResetRecoveryPriorOutcome.OutcomeUncertain)
            {
                throw new ArgumentOutOfRangeException("priorOutcome");
            }

            if (state == GroupResetRecoveryState.ArmedBeforeDispatch
                && outcome != GroupResetRecoveryPriorOutcome.NotAttempted)
            {
                throw new ArgumentException(
                    "An armed Group Reset must remain NotAttempted.",
                    "priorOutcome");
            }
            if (state == GroupResetRecoveryState.AcceptedAwaitingProof
                && outcome != GroupResetRecoveryPriorOutcome.Accepted)
            {
                throw new ArgumentException(
                    "AcceptedAwaitingProof requires an Accepted prior outcome.",
                    "priorOutcome");
            }
            if (state == GroupResetRecoveryState.RecoveryRequired
                && outcome == GroupResetRecoveryPriorOutcome.NotAttempted)
            {
                throw new ArgumentException(
                    "RecoveryRequired requires Accepted or OutcomeUncertain.",
                    "priorOutcome");
            }
        }

        private static void ValidateTransition(
            GroupResetRecoveryState currentState,
            GroupResetRecoveryPriorOutcome currentOutcome,
            GroupResetRecoveryState nextState,
            GroupResetRecoveryPriorOutcome nextOutcome)
        {
            if (currentState == GroupResetRecoveryState.Resolved
                || currentState == nextState)
            {
                throw new InvalidOperationException(
                    "A Group Reset recovery transition cannot be repeated.");
            }

            var valid = false;
            if (currentState == GroupResetRecoveryState.ArmedBeforeDispatch)
            {
                valid = (nextState
                            == GroupResetRecoveryState.AcceptedAwaitingProof
                        && nextOutcome
                            == GroupResetRecoveryPriorOutcome.Accepted)
                    || (nextState
                            == GroupResetRecoveryState.RecoveryRequired
                        && nextOutcome
                            == GroupResetRecoveryPriorOutcome
                                .OutcomeUncertain)
                    || (nextState == GroupResetRecoveryState.Resolved
                        && nextOutcome
                            == GroupResetRecoveryPriorOutcome.NotAttempted);
            }
            else if (currentState
                == GroupResetRecoveryState.AcceptedAwaitingProof)
            {
                valid = (nextState
                            == GroupResetRecoveryState.RecoveryRequired
                        || nextState == GroupResetRecoveryState.Resolved)
                    && currentOutcome
                        == GroupResetRecoveryPriorOutcome.Accepted
                    && nextOutcome == currentOutcome;
            }
            else if (currentState
                == GroupResetRecoveryState.RecoveryRequired)
            {
                valid = nextState == GroupResetRecoveryState.Resolved
                    && nextOutcome == currentOutcome;
            }

            if (!valid)
            {
                throw new InvalidOperationException(
                    "The requested Group Reset recovery state/outcome transition is invalid.");
            }
        }

        private static void ValidateTimestamps(
            DateTime createdUtc,
            DateTime updatedUtc)
        {
            if (createdUtc.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException(
                    "Recovery creation time must be UTC.",
                    "createdUtc");
            }
            if (updatedUtc.Kind != DateTimeKind.Utc
                || updatedUtc < createdUtc)
            {
                throw new ArgumentException(
                    "Recovery update time must be UTC and cannot precede creation.",
                    "updatedUtc");
            }
        }

        private static GroupResetRecoveryMember[] CopyAndValidateMembers(
            GroupResetRecoveryMember[] source)
        {
            if (source == null || source.Length < 1 || source.Length > 16)
            {
                throw new ArgumentException(
                    "A Group Reset recovery snapshot must contain 1 through 16 members.",
                    "members");
            }

            var result = new GroupResetRecoveryMember[source.Length];
            var references = new HashSet<ushort>();
            for (var index = 0; index < source.Length; index++)
            {
                if (source[index] == null
                    || source[index].AxisReference == 0
                    || !references.Add(source[index].AxisReference))
                {
                    throw new ArgumentException(
                        "Group Reset recovery members must be non-null with unique non-zero references.",
                        "members");
                }
                result[index] = source[index].Copy();
            }
            return result;
        }

        private static GroupResetRecoveryMember[] CopyMembers(
            GroupResetRecoveryMember[] source)
        {
            if (source == null)
            {
                return null;
            }
            var result = new GroupResetRecoveryMember[source.Length];
            for (var index = 0; index < source.Length; index++)
            {
                result[index] = source[index].Copy();
            }
            return result;
        }

        private static bool MembersExactEqual(
            GroupResetRecoveryMember[] left,
            GroupResetRecoveryMember[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }
            for (var index = 0; index < left.Length; index++)
            {
                if (left[index] == null
                    || !left[index].ExactEquals(right[index]))
                {
                    return false;
                }
            }
            return true;
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

    internal sealed class GroupResetRecoveryJournal : IDisposable
    {
        internal const string JournalFileName = "journal.dat";
        internal const string LockFileName = "journal.lock";
        internal const int ChecksumLength = 32;

        private const int FormatVersion = 1;
        private const int MaximumFileLength = 65536;
        private const int MaximumTextByteLength = 1024;
        private static readonly byte[] Magic =
            Encoding.ASCII.GetBytes("ELMOGRJ1");

        private readonly object sync = new object();
        private readonly string directoryPath;
        private readonly string journalFilePath;
        private FileStream lockStream;
        private GroupResetRecoveryRecord currentRecord;
        private bool disposed;

        private GroupResetRecoveryJournal(string requestedDirectoryPath)
        {
            if (string.IsNullOrWhiteSpace(requestedDirectoryPath))
            {
                throw new ArgumentException(
                    "A Group Reset recovery journal directory is required.",
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

        internal GroupResetRecoveryRecord Current
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

        internal GroupResetRecoveryRecord CurrentRecord
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

        internal static GroupResetRecoveryJournal Open(string directoryPath)
        {
            return new GroupResetRecoveryJournal(directoryPath);
        }

        internal static GroupResetRecoveryJournal OpenDefault()
        {
            return Open(GetDefaultDirectoryPath());
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
                "GroupResetRecoveryJournal",
                "v1");
        }

        internal GroupResetRecoveryRecord ArmBeforeDispatch(
            string plcIp,
            int plcTcpPort,
            string localIpv4,
            int callbackUdpPort,
            uint diagnosticsBuild,
            uint diagnosticsBootId,
            uint mapRevision,
            string groupName,
            ushort groupReference,
            long ownerSessionGeneration,
            GroupResetRecoveryMember[] members,
            int requiredStableSampleCount,
            DateTime createdUtc)
        {
            return ArmBeforeDispatch(
                Guid.NewGuid(),
                plcIp,
                plcTcpPort,
                localIpv4,
                callbackUdpPort,
                diagnosticsBuild,
                diagnosticsBootId,
                mapRevision,
                groupName,
                groupReference,
                ownerSessionGeneration,
                members,
                requiredStableSampleCount,
                createdUtc);
        }

        internal GroupResetRecoveryRecord ArmBeforeDispatch(
            Guid identity,
            string plcIp,
            int plcTcpPort,
            string localIpv4,
            int callbackUdpPort,
            uint diagnosticsBuild,
            uint diagnosticsBootId,
            uint mapRevision,
            string groupName,
            ushort groupReference,
            long ownerSessionGeneration,
            GroupResetRecoveryMember[] members,
            int requiredStableSampleCount,
            DateTime createdUtc)
        {
            lock (sync)
            {
                ThrowIfDisposed();
                if (currentRecord != null && currentRecord.IsActive)
                {
                    throw new InvalidOperationException(
                        "An unresolved Group Reset recovery record already exists.");
                }
                if (currentRecord != null
                    && currentRecord.Identity == identity)
                {
                    throw new InvalidOperationException(
                        "A resolved Group Reset recovery identity cannot be reused.");
                }

                var armed = new GroupResetRecoveryRecord(
                    identity,
                    1,
                    GroupResetRecoveryState.ArmedBeforeDispatch,
                    GroupResetRecoveryPriorOutcome.NotAttempted,
                    plcIp,
                    plcTcpPort,
                    localIpv4,
                    callbackUdpPort,
                    diagnosticsBuild,
                    diagnosticsBootId,
                    mapRevision,
                    groupName,
                    groupReference,
                    ownerSessionGeneration,
                    members,
                    requiredStableSampleCount,
                    createdUtc,
                    createdUtc,
                    null);
                currentRecord = PersistRecord(armed);
                return currentRecord.Copy();
            }
        }

        internal GroupResetRecoveryRecord MarkAccepted(
            GroupResetRecoveryRecord expectedCurrent,
            DateTime updatedUtc)
        {
            return Transition(
                expectedCurrent,
                GroupResetRecoveryState.AcceptedAwaitingProof,
                GroupResetRecoveryPriorOutcome.Accepted,
                updatedUtc);
        }

        internal GroupResetRecoveryRecord PromoteRecoveryRequired(
            GroupResetRecoveryRecord expectedCurrent,
            DateTime updatedUtc)
        {
            if (expectedCurrent == null)
            {
                throw new ArgumentNullException("expectedCurrent");
            }
            var nextOutcome = expectedCurrent.State
                    == GroupResetRecoveryState.ArmedBeforeDispatch
                ? GroupResetRecoveryPriorOutcome.OutcomeUncertain
                : expectedCurrent.PriorOutcome;
            return Transition(
                expectedCurrent,
                GroupResetRecoveryState.RecoveryRequired,
                nextOutcome,
                updatedUtc);
        }

        internal GroupResetRecoveryRecord PromoteToRecoveryRequired(
            GroupResetRecoveryRecord expectedCurrent,
            DateTime updatedUtc)
        {
            return PromoteRecoveryRequired(expectedCurrent, updatedUtc);
        }

        internal GroupResetRecoveryRecord Resolve(
            GroupResetRecoveryRecord expectedCurrent,
            DateTime updatedUtc)
        {
            if (expectedCurrent == null)
            {
                throw new ArgumentNullException("expectedCurrent");
            }
            return Transition(
                expectedCurrent,
                GroupResetRecoveryState.Resolved,
                expectedCurrent.PriorOutcome,
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

        internal GroupResetRecoveryRecord ResolveOperatorRetirement(
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
                    "Group Reset retirement requires a durably committed ledger decision.");
            }

            lock (sync)
            {
                ThrowIfDisposed();
                if (!committedDecision.MatchesSourceEvidence(
                        expectedEvidence))
                {
                    throw new InvalidOperationException(
                        "The retirement decision does not match the expected Group Reset evidence.");
                }
                var currentEvidence =
                    CaptureActiveRetirementEvidenceCore();
                if (!expectedEvidence.ExactSourceEquals(currentEvidence)
                    || !committedDecision.MatchesSourceEvidence(
                        currentEvidence))
                {
                    throw new InvalidOperationException(
                        "Group Reset recovery changed after operator confirmation.");
                }

                var resolved = currentRecord.TransitionTo(
                    GroupResetRecoveryState.Resolved,
                    currentRecord.PriorOutcome,
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

        private GroupResetRecoveryRecord Transition(
            GroupResetRecoveryRecord expectedCurrent,
            GroupResetRecoveryState nextState,
            GroupResetRecoveryPriorOutcome nextOutcome,
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
                    nextState,
                    nextOutcome,
                    updatedUtc);
                currentRecord = PersistRecord(transitioned);
                return currentRecord.Copy();
            }
        }

        private void RequireExactCurrent(
            GroupResetRecoveryRecord expectedCurrent)
        {
            if (currentRecord == null
                || !currentRecord.ExactEquals(expectedCurrent))
            {
                throw new InvalidOperationException(
                    "The expected Group Reset recovery record is not the exact current durable revision.");
            }
        }

        private RecoveryJournalSourceEvidence
            CaptureActiveRetirementEvidenceCore()
        {
            if (currentRecord == null || !currentRecord.IsActive)
            {
                throw new InvalidOperationException(
                    "No active Group Reset recovery record exists for retirement.");
            }
            var originalBytes = ReadAllJournalBytes();
            var diskRecord = DeserializeRecord(originalBytes);
            if (!currentRecord.ExactEquals(diskRecord))
            {
                throw new InvalidDataException(
                    "Group Reset recovery memory state does not match durable source bytes.");
            }

            return new RecoveryJournalSourceEvidence(
                RecoveryRecordOwner.GroupReset,
                diskRecord.Identity,
                (int)diskRecord.State,
                diskRecord.CreatedUtc,
                diskRecord.UpdatedUtc,
                diskRecord.PlcIp,
                diskRecord.PlcTcpPort,
                diskRecord.DiagnosticsBuild,
                diskRecord.DiagnosticsBootId,
                diskRecord.MapRevision,
                "Group",
                diskRecord.GroupName,
                diskRecord.GroupReference,
                "Reset",
                BuildRetirementFingerprint(diskRecord),
                originalBytes);
        }

        private static string BuildRetirementFingerprint(
            GroupResetRecoveryRecord record)
        {
            var builder = new StringBuilder();
            builder.Append("Revision=");
            builder.Append(record.RecordRevision.ToString(
                CultureInfo.InvariantCulture));
            builder.Append(";PriorOutcome=");
            builder.Append(record.PriorOutcome);
            builder.Append(";LocalIpv4=");
            builder.Append(record.LocalIpv4);
            builder.Append(";CallbackUdpPort=");
            builder.Append(record.CallbackUdpPort.ToString(
                CultureInfo.InvariantCulture));
            builder.Append(";DiagnosticsBuild=0x");
            builder.Append(record.DiagnosticsBuild.ToString("X8"));
            builder.Append(";OwnerSessionGeneration=");
            builder.Append(record.OwnerSessionGeneration.ToString(
                CultureInfo.InvariantCulture));
            builder.Append(";StableSamples=");
            builder.Append(record.RequiredStableSampleCount.ToString(
                CultureInfo.InvariantCulture));
            builder.Append(";MembersSha256=");
            builder.Append(ComputeMemberFingerprint(record.Members));
            return builder.ToString();
        }

        private static string ComputeMemberFingerprint(
            GroupResetRecoveryMember[] members)
        {
            byte[] canonical;
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(
                stream,
                Encoding.ASCII,
                true))
            {
                writer.Write(members.Length);
                for (var index = 0; index < members.Length; index++)
                {
                    writer.Write(members[index].AxisReference);
                    writer.Write(members[index].DeviceId);
                    WriteText(writer, members[index].AxisName);
                }
                writer.Flush();
                canonical = stream.ToArray();
            }

            byte[] digest;
            using (var sha256 = SHA256.Create())
            {
                digest = sha256.ComputeHash(canonical);
            }
            var result = new StringBuilder(digest.Length * 2);
            for (var index = 0; index < digest.Length; index++)
            {
                result.Append(digest[index].ToString("X2"));
            }
            return result.ToString();
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

        private GroupResetRecoveryRecord PersistRecord(
            GroupResetRecoveryRecord record)
        {
            var bytes = SerializeRecord(record);
            var persisted = record.WithChecksum(
                CopyChecksum(bytes));
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

        private static GroupResetRecoveryRecord LoadRecord(string path)
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
                    "Group Reset recovery journal length is invalid.");
            }
            var bytes = new byte[checked((int)stream.Length)];
            var offset = 0;
            while (offset < bytes.Length)
            {
                var read = stream.Read(bytes, offset, bytes.Length - offset);
                if (read <= 0)
                {
                    throw new InvalidDataException(
                        "Group Reset recovery journal is incomplete.");
                }
                offset += read;
            }
            return bytes;
        }

        private static byte[] SerializeRecord(
            GroupResetRecoveryRecord record)
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
                writer.Write((int)record.State);
                writer.Write((int)record.PriorOutcome);
                writer.Write(record.PlcTcpPort);
                writer.Write(record.CallbackUdpPort);
                writer.Write(record.DiagnosticsBuild);
                writer.Write(record.DiagnosticsBootId);
                writer.Write(record.MapRevision);
                writer.Write(record.GroupReference);
                writer.Write(record.OwnerSessionGeneration);
                writer.Write(record.RequiredStableSampleCount);
                writer.Write(record.CreatedUtc.Ticks);
                writer.Write(record.UpdatedUtc.Ticks);
                WriteText(writer, record.PlcIp);
                WriteText(writer, record.LocalIpv4);
                WriteText(writer, record.GroupName);
                var members = record.Members;
                writer.Write(members.Length);
                for (var index = 0; index < members.Length; index++)
                {
                    writer.Write(members[index].AxisReference);
                    writer.Write(members[index].DeviceId);
                    WriteText(writer, members[index].AxisName);
                }
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

        private static GroupResetRecoveryRecord DeserializeRecord(
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

        private static GroupResetRecoveryRecord DeserializeRecordCore(
            byte[] bytes)
        {
            if (bytes == null
                || bytes.Length < Magic.Length + 8 + ChecksumLength
                || bytes.Length > MaximumFileLength)
            {
                throw new InvalidDataException(
                    "Group Reset recovery journal length is invalid.");
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
                    "Group Reset recovery journal checksum is invalid.");
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
                        "Group Reset recovery journal magic is invalid.");
                }
                if (reader.ReadInt32() != FormatVersion)
                {
                    throw new InvalidDataException(
                        "Group Reset recovery journal version is unsupported.");
                }
                var payloadLength = reader.ReadInt32();
                if (payloadLength <= 0
                    || payloadLength
                        != checksumOffset - (Magic.Length + 8))
                {
                    throw new InvalidDataException(
                        "Group Reset recovery payload length is invalid.");
                }
                payload = reader.ReadBytes(payloadLength);
                if (payload.Length != payloadLength
                    || fileStream.Position != checksumOffset)
                {
                    throw new InvalidDataException(
                        "Group Reset recovery payload is incomplete.");
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
                        "Group Reset recovery identity is incomplete.");
                }
                var identity = new Guid(identityBytes);
                var revision = reader.ReadInt64();
                var state = (GroupResetRecoveryState)reader.ReadInt32();
                var priorOutcome =
                    (GroupResetRecoveryPriorOutcome)reader.ReadInt32();
                var plcTcpPort = reader.ReadInt32();
                var callbackUdpPort = reader.ReadInt32();
                var diagnosticsBuild = reader.ReadUInt32();
                var diagnosticsBootId = reader.ReadUInt32();
                var mapRevision = reader.ReadUInt32();
                var groupReference = reader.ReadUInt16();
                var ownerSessionGeneration = reader.ReadInt64();
                var requiredStableSampleCount = reader.ReadInt32();
                var createdUtc = new DateTime(
                    reader.ReadInt64(),
                    DateTimeKind.Utc);
                var updatedUtc = new DateTime(
                    reader.ReadInt64(),
                    DateTimeKind.Utc);
                var plcIp = ReadText(reader);
                var localIpv4 = ReadText(reader);
                var groupName = ReadText(reader);
                var memberCount = reader.ReadInt32();
                if (memberCount < 1 || memberCount > 16)
                {
                    throw new InvalidDataException(
                        "Group Reset recovery member count is invalid.");
                }
                var members = new GroupResetRecoveryMember[memberCount];
                for (var index = 0; index < memberCount; index++)
                {
                    var axisReference = reader.ReadUInt16();
                    var deviceId = reader.ReadUInt16();
                    var axisName = ReadText(reader);
                    members[index] = new GroupResetRecoveryMember(
                        axisName,
                        axisReference,
                        deviceId);
                }
                if (stream.Position != stream.Length)
                {
                    throw new InvalidDataException(
                        "Group Reset recovery journal has trailing payload data.");
                }
                return new GroupResetRecoveryRecord(
                    identity,
                    revision,
                    state,
                    priorOutcome,
                    plcIp,
                    plcTcpPort,
                    localIpv4,
                    callbackUdpPort,
                    diagnosticsBuild,
                    diagnosticsBootId,
                    mapRevision,
                    groupName,
                    groupReference,
                    ownerSessionGeneration,
                    members,
                    requiredStableSampleCount,
                    createdUtc,
                    updatedUtc,
                    checksum);
            }
        }

        private static void WriteText(BinaryWriter writer, string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            if (bytes.Length < 1 || bytes.Length > MaximumTextByteLength)
            {
                throw new InvalidOperationException(
                    "Group Reset recovery text encoding is invalid.");
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
                    "Group Reset recovery text length is invalid.");
            }
            var bytes = reader.ReadBytes(length);
            if (bytes.Length != length)
            {
                throw new InvalidDataException(
                    "Group Reset recovery text is incomplete.");
            }
            for (var index = 0; index < bytes.Length; index++)
            {
                if (bytes[index] < 0x20 || bytes[index] > 0x7e)
                {
                    throw new InvalidDataException(
                        "Group Reset recovery text is not printable 7-bit ASCII.");
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
                "Group Reset recovery journal record is invalid.",
                error);
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(
                    "GroupResetRecoveryJournal");
            }
        }
    }
}
