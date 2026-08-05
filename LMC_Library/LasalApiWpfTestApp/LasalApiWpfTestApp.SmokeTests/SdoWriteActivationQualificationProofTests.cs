using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using LasalMotionControlLib;
using LasalMotionControlLib.Tests;

namespace LasalMotionControlApiExample
{
    internal static class SdoWriteActivationQualificationProofTests
    {
        private const long SessionGeneration = 41;
        private const uint DiagnosticsBuild = 0x20260730;
        private const uint DiagnosticsBootId = 0x10203040;
        private const uint MapRevision = 0x957F101E;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.SdoWriteActivationProof.ExactCurrentStateMatches",
                ExactCurrentStateMatches);
            tests.Add(
                "Wpf.SdoWriteActivationProof.InvalidCaptureFailsClosed",
                InvalidCaptureFailsClosed);
            tests.Add(
                "Wpf.SdoWriteActivationProof.ConnectionAndIdentityMismatch",
                ConnectionAndIdentityMismatchFailsClosed);
            tests.Add(
                "Wpf.SdoWriteActivationProof.TargetTupleMismatch",
                TargetTupleMismatchFailsClosed);
            tests.Add(
                "Wpf.SdoWriteActivationProof.IdentityMismatchCannotRevive",
                IdentityMismatchCannotRevive);
            tests.Add(
                "Wpf.SdoWriteActivationProof.DisconnectCannotRevive",
                DisconnectCannotRevive);
        }

        private static void ExactCurrentStateMatches()
        {
            using (var connection = CreateConnection(SessionGeneration))
            {
                var capabilities = CreateBoundCapabilities(
                    connection,
                    SessionGeneration,
                    DiagnosticsBuild,
                    DiagnosticsBootId,
                    MapRevision);
                var target = GetApprovedTarget(connection);
                SdoWriteActivationQualificationProof proof;
                AssertEx.True(
                    SdoWriteActivationQualificationProof.TryCapture(
                        connection,
                        capabilities,
                        target,
                        out proof));
                AssertEx.NotNull(proof);
                AssertEx.True(
                    proof.MatchesCurrent(
                        connection,
                        capabilities,
                        target));

                var equivalentTarget = CreateTarget(
                    "Equivalent display text",
                    target.SlaveReference,
                    target.ObjectIndex,
                    target.SubIndex,
                    target.ValueType,
                    target.DataLength,
                    target.MinimumIntegerValue,
                    target.MaximumIntegerValue);
                AssertEx.False(ReferenceEquals(target, equivalentTarget));
                AssertEx.True(
                    proof.MatchesCurrent(
                        connection,
                        capabilities,
                        equivalentTarget));
                AssertEx.Equal(SessionGeneration, proof.SessionGeneration);
                AssertEx.Equal(DiagnosticsBuild, proof.DiagnosticsBuild);
                AssertEx.Equal(DiagnosticsBootId, proof.DiagnosticsBootId);
                AssertEx.Equal(MapRevision, proof.MapRevision);
                AssertEx.Equal(target.SlaveReference, proof.SlaveReference);
                AssertEx.Equal(target.ObjectIndex, proof.ObjectIndex);
                AssertEx.Equal(target.SubIndex, proof.SubIndex);
                AssertEx.Equal(target.ValueType, proof.ValueType);
                AssertEx.Equal(target.DataLength, proof.DataLength);
                AssertEx.Equal(
                    target.MinimumIntegerValue,
                    proof.MinimumIntegerValue);
                AssertEx.Equal(
                    target.MaximumIntegerValue,
                    proof.MaximumIntegerValue);
            }
        }

        private static void InvalidCaptureFailsClosed()
        {
            using (var connection = CreateConnection(SessionGeneration))
            using (var zeroSessionConnection = CreateConnection(0))
            using (var otherConnection = CreateConnection(SessionGeneration))
            {
                var target = GetApprovedTarget(connection);
                var valid = CreateBoundCapabilities(
                    connection,
                    SessionGeneration,
                    DiagnosticsBuild,
                    DiagnosticsBootId,
                    MapRevision);
                SdoWriteActivationQualificationProof proof;

                AssertCaptureRejected(null, valid, target);
                AssertCaptureRejected(connection, null, target);
                AssertCaptureRejected(connection, valid, null);
                AssertCaptureRejected(
                    zeroSessionConnection,
                    CreateUnboundCapabilities(
                        0,
                        DiagnosticsBuild,
                        DiagnosticsBootId,
                        MapRevision),
                    GetApprovedTarget(zeroSessionConnection));
                AssertCaptureRejected(
                    connection,
                    CreateBoundCapabilities(
                        connection,
                        SessionGeneration,
                        0,
                        DiagnosticsBootId,
                        MapRevision),
                    target);
                AssertCaptureRejected(
                    connection,
                    CreateBoundCapabilities(
                        connection,
                        SessionGeneration,
                        DiagnosticsBuild,
                        0,
                        MapRevision),
                    target);
                AssertCaptureRejected(
                    connection,
                    CreateBoundCapabilities(
                        connection,
                        SessionGeneration,
                        DiagnosticsBuild,
                        DiagnosticsBootId,
                        0),
                    target);
                AssertCaptureRejected(
                    connection,
                    CreateUnboundCapabilities(
                        SessionGeneration,
                        DiagnosticsBuild,
                        DiagnosticsBootId,
                        MapRevision),
                    target);
                AssertCaptureRejected(
                    connection,
                    CreateBoundCapabilities(
                        otherConnection,
                        SessionGeneration,
                        DiagnosticsBuild,
                        DiagnosticsBootId,
                        MapRevision),
                    target);

                var unapprovedTarget = CreateTarget(
                    "Unapproved slave",
                    (ushort)(target.SlaveReference + 1),
                    target.ObjectIndex,
                    target.SubIndex,
                    target.ValueType,
                    target.DataLength,
                    target.MinimumIntegerValue,
                    target.MaximumIntegerValue);
                AssertCaptureRejected(connection, valid, unapprovedTarget);

                AssertEx.False(
                    SdoWriteActivationQualificationProof.TryCapture(
                        null,
                        null,
                        null,
                        out proof));
                AssertEx.Equal<SdoWriteActivationQualificationProof>(
                    null,
                    proof);
            }
        }

        private static void ConnectionAndIdentityMismatchFailsClosed()
        {
            using (var connection = CreateConnection(SessionGeneration))
            using (var otherConnection = CreateConnection(SessionGeneration))
            {
                var target = GetApprovedTarget(connection);
                var capabilities = CreateBoundCapabilities(
                    connection,
                    SessionGeneration,
                    DiagnosticsBuild,
                    DiagnosticsBootId,
                    MapRevision);
                SdoWriteActivationQualificationProof proof;
                AssertEx.True(
                    SdoWriteActivationQualificationProof.TryCapture(
                        connection,
                        capabilities,
                        target,
                        out proof));

                AssertEx.False(proof.MatchesCurrent(null, capabilities, target));
                AssertEx.False(proof.MatchesCurrent(connection, null, target));
                AssertEx.False(
                    proof.MatchesCurrent(connection, capabilities, null));
                AssertEx.False(
                    proof.MatchesCurrent(
                        otherConnection,
                        CreateBoundCapabilities(
                            otherConnection,
                            SessionGeneration,
                            DiagnosticsBuild,
                            DiagnosticsBootId,
                            MapRevision),
                        GetApprovedTarget(otherConnection)));
                AssertEx.False(
                    proof.MatchesCurrent(
                        connection,
                        CreateBoundCapabilities(
                            connection,
                            SessionGeneration,
                            DiagnosticsBuild + 1,
                            DiagnosticsBootId,
                            MapRevision),
                        target));
                AssertEx.False(
                    proof.MatchesCurrent(
                        connection,
                        CreateBoundCapabilities(
                            connection,
                            SessionGeneration,
                            DiagnosticsBuild,
                            DiagnosticsBootId + 1,
                            MapRevision),
                        target));
                AssertEx.False(
                    proof.MatchesCurrent(
                        connection,
                        CreateBoundCapabilities(
                            connection,
                            SessionGeneration,
                            DiagnosticsBuild,
                            DiagnosticsBootId,
                            MapRevision + 1),
                        target));
                AssertEx.False(
                    proof.MatchesCurrent(
                        connection,
                        CreateBoundCapabilities(
                            connection,
                            SessionGeneration,
                            0,
                            DiagnosticsBootId,
                            MapRevision),
                        target));
                AssertEx.False(
                    proof.MatchesCurrent(
                        connection,
                        CreateBoundCapabilities(
                            connection,
                            SessionGeneration,
                            DiagnosticsBuild,
                            0,
                            MapRevision),
                        target));
                AssertEx.False(
                    proof.MatchesCurrent(
                        connection,
                        CreateBoundCapabilities(
                            connection,
                            SessionGeneration,
                            DiagnosticsBuild,
                            DiagnosticsBootId,
                            0),
                        target));
                AssertEx.False(
                    proof.MatchesCurrent(
                        connection,
                        CreateUnboundCapabilities(
                            SessionGeneration,
                            DiagnosticsBuild,
                            DiagnosticsBootId,
                            MapRevision),
                        target));

                SetConnectionSessionGeneration(
                    connection,
                    SessionGeneration + 1);
                AssertEx.False(
                    proof.MatchesCurrent(
                        connection,
                        CreateBoundCapabilities(
                            connection,
                            SessionGeneration + 1,
                            DiagnosticsBuild,
                            DiagnosticsBootId,
                            MapRevision),
                        target));
            }
        }

        private static void TargetTupleMismatchFailsClosed()
        {
            using (var connection = CreateConnection(SessionGeneration))
            {
                var capabilities = CreateBoundCapabilities(
                    connection,
                    SessionGeneration,
                    DiagnosticsBuild,
                    DiagnosticsBootId,
                    MapRevision);
                var target = GetApprovedTarget(connection);
                SdoWriteActivationQualificationProof proof;
                AssertEx.True(
                    SdoWriteActivationQualificationProof.TryCapture(
                        connection,
                        capabilities,
                        target,
                        out proof));

                var mismatches = new[]
                {
                    CreateTarget(
                        "Slave mismatch",
                        (ushort)(target.SlaveReference + 1),
                        target.ObjectIndex,
                        target.SubIndex,
                        target.ValueType,
                        target.DataLength,
                        target.MinimumIntegerValue,
                        target.MaximumIntegerValue),
                    CreateTarget(
                        "Index mismatch",
                        target.SlaveReference,
                        (ushort)(target.ObjectIndex + 1),
                        target.SubIndex,
                        target.ValueType,
                        target.DataLength,
                        target.MinimumIntegerValue,
                        target.MaximumIntegerValue),
                    CreateTarget(
                        "SubIndex mismatch",
                        target.SlaveReference,
                        target.ObjectIndex,
                        (byte)(target.SubIndex + 1),
                        target.ValueType,
                        target.DataLength,
                        target.MinimumIntegerValue,
                        target.MaximumIntegerValue),
                    CreateTarget(
                        "ValueType mismatch",
                        target.SlaveReference,
                        target.ObjectIndex,
                        target.SubIndex,
                        LMCSignalValueType.UInt32,
                        target.DataLength,
                        0,
                        target.MaximumIntegerValue),
                    CreateTarget(
                        "Minimum mismatch",
                        target.SlaveReference,
                        target.ObjectIndex,
                        target.SubIndex,
                        target.ValueType,
                        target.DataLength,
                        target.MinimumIntegerValue + 1,
                        target.MaximumIntegerValue),
                    CreateTarget(
                        "Maximum mismatch",
                        target.SlaveReference,
                        target.ObjectIndex,
                        target.SubIndex,
                        target.ValueType,
                        target.DataLength,
                        target.MinimumIntegerValue,
                        target.MaximumIntegerValue - 1)
                };

                foreach (var mismatch in mismatches)
                {
                    AssertEx.False(
                        proof.MatchesCurrent(
                            connection,
                            capabilities,
                            mismatch));
                    AssertCaptureRejected(
                        connection,
                        capabilities,
                        mismatch);
                }

                var dataLengthMismatch = CreateTarget(
                    "DataLength mismatch",
                    target.SlaveReference,
                    target.ObjectIndex,
                    target.SubIndex,
                    target.ValueType,
                    target.DataLength,
                    target.MinimumIntegerValue,
                    target.MaximumIntegerValue);
                SetAutoPropertyBackingField(
                    dataLengthMismatch,
                    "DataLength",
                    (ushort)2);
                AssertEx.False(
                    proof.MatchesCurrent(
                        connection,
                        capabilities,
                        dataLengthMismatch));
                AssertCaptureRejected(
                    connection,
                    capabilities,
                    dataLengthMismatch);
            }
        }

        private static void IdentityMismatchCannotRevive()
        {
            using (var connection = CreateConnection(SessionGeneration))
            {
                var target = GetApprovedTarget(connection);
                var identityA = CreateBoundCapabilities(
                    connection,
                    SessionGeneration,
                    DiagnosticsBuild,
                    DiagnosticsBootId,
                    MapRevision);
                SdoWriteActivationQualificationProof proof;
                AssertEx.True(
                    SdoWriteActivationQualificationProof.TryCapture(
                        connection,
                        identityA,
                        target,
                        out proof));

                var identityB = CreateBoundCapabilities(
                    connection,
                    SessionGeneration,
                    DiagnosticsBuild + 1,
                    DiagnosticsBootId,
                    MapRevision);
                AssertEx.False(
                    proof.MatchesCurrent(connection, identityB, target));

                var identityAReturned = CreateBoundCapabilities(
                    connection,
                    SessionGeneration,
                    DiagnosticsBuild,
                    DiagnosticsBootId,
                    MapRevision);
                AssertEx.False(
                    proof.MatchesCurrent(
                        connection,
                        identityAReturned,
                        target));
            }
        }

        private static void DisconnectCannotRevive()
        {
            using (var disconnected = new LMCConnection())
            {
                SetConnectionSessionGeneration(
                    disconnected,
                    SessionGeneration);
                var disconnectedCapabilities = CreateBoundCapabilities(
                    disconnected,
                    SessionGeneration,
                    DiagnosticsBuild,
                    DiagnosticsBootId,
                    MapRevision);
                AssertCaptureRejected(
                    disconnected,
                    disconnectedCapabilities,
                    GetApprovedTarget(disconnected));
            }

            using (var connection = CreateConnection(SessionGeneration))
            {
                var capabilities = CreateBoundCapabilities(
                    connection,
                    SessionGeneration,
                    DiagnosticsBuild,
                    DiagnosticsBootId,
                    MapRevision);
                var target = GetApprovedTarget(connection);
                SdoWriteActivationQualificationProof proof;
                AssertEx.True(
                    SdoWriteActivationQualificationProof.TryCapture(
                        connection,
                        capabilities,
                        target,
                        out proof));

                SetConnectionState(
                    connection,
                    LMCConnectionState.Disconnected);
                AssertEx.False(
                    proof.MatchesCurrent(connection, capabilities, target));
                SetConnectionState(
                    connection,
                    LMCConnectionState.Connected);
                AssertEx.False(
                    proof.MatchesCurrent(connection, capabilities, target));
            }
        }

        private static void AssertCaptureRejected(
            LMCConnection connection,
            LMCDiagnosticCapabilities capabilities,
            LMCSdoWriteTarget target)
        {
            SdoWriteActivationQualificationProof proof;
            AssertEx.False(
                SdoWriteActivationQualificationProof.TryCapture(
                    connection,
                    capabilities,
                    target,
                    out proof));
            AssertEx.Equal<SdoWriteActivationQualificationProof>(null, proof);
        }

        private static LMCConnection CreateConnection(long sessionGeneration)
        {
            var connection = new LMCConnection();
            SetConnectionSessionGeneration(connection, sessionGeneration);
            SetConnectionState(connection, LMCConnectionState.Connected);
            return connection;
        }

        private static void SetConnectionState(
            LMCConnection connection,
            LMCConnectionState state)
        {
            var field = typeof(LMCConnection).GetField(
                "connectionState",
                BindingFlags.Instance | BindingFlags.NonPublic);
            AssertEx.NotNull(field);
            field.SetValue(connection, (int)state);
        }

        private static void SetConnectionSessionGeneration(
            LMCConnection connection,
            long sessionGeneration)
        {
            var field = typeof(LMCConnection).GetField(
                "sessionGeneration",
                BindingFlags.Instance | BindingFlags.NonPublic);
            AssertEx.NotNull(field);
            field.SetValue(connection, sessionGeneration);
        }

        private static LMCDiagnosticCapabilities CreateBoundCapabilities(
            LMCConnection connection,
            long sessionGeneration,
            uint diagnosticsBuild,
            uint diagnosticsBootId,
            uint mapRevision)
        {
            var capabilities = CreateUnboundCapabilities(
                sessionGeneration,
                diagnosticsBuild,
                diagnosticsBootId,
                mapRevision);
            var bindMethod = typeof(LMCDiagnosticCapabilities).GetMethod(
                "BindProvenance",
                BindingFlags.Instance | BindingFlags.NonPublic);
            AssertEx.NotNull(bindMethod);
            bindMethod.Invoke(
                capabilities,
                new object[]
                {
                    connection.Diagnostics,
                    sessionGeneration,
                    1L
                });
            return capabilities;
        }

        private static LMCDiagnosticCapabilities CreateUnboundCapabilities(
            long sessionGeneration,
            uint diagnosticsBuild,
            uint diagnosticsBootId,
            uint mapRevision)
        {
            var capabilityBits =
                LMCDiagnosticCapability.SDORead
                | LMCDiagnosticCapability.SDOWrite
                | LMCDiagnosticCapability.SDOReadGeneralInline;
            return (LMCDiagnosticCapabilities)Activator.CreateInstance(
                typeof(LMCDiagnosticCapabilities),
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new object[]
                {
                    null,
                    sessionGeneration,
                    diagnosticsBuild,
                    (uint)capabilityBits,
                    mapRevision,
                    (ushort)0,
                    (ushort)0,
                    (ushort)0,
                    (ushort)0,
                    0u,
                    1000u,
                    (ushort)256,
                    (ushort)512,
                    (ushort)128,
                    (ushort)0,
                    (ushort)0,
                    0u,
                    (ushort)4,
                    diagnosticsBootId
                },
                CultureInfo.InvariantCulture);
        }

        private static LMCSdoWriteTarget GetApprovedTarget(
            LMCConnection connection)
        {
            var targets = connection.Diagnostics.GetApprovedSdoWriteTargets();
            AssertEx.Equal(1, targets.Count);
            AssertEx.NotNull(targets[0]);
            return targets[0];
        }

        private static LMCSdoWriteTarget CreateTarget(
            string displayName,
            ushort slaveReference,
            ushort objectIndex,
            byte subIndex,
            LMCSignalValueType valueType,
            ushort dataLength,
            long minimumIntegerValue,
            long maximumIntegerValue)
        {
            return (LMCSdoWriteTarget)Activator.CreateInstance(
                typeof(LMCSdoWriteTarget),
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new object[]
                {
                    displayName,
                    slaveReference,
                    objectIndex,
                    subIndex,
                    valueType,
                    dataLength,
                    minimumIntegerValue,
                    maximumIntegerValue
                },
                CultureInfo.InvariantCulture);
        }

        private static void SetAutoPropertyBackingField(
            object target,
            string propertyName,
            object value)
        {
            var field = target.GetType().GetField(
                "<" + propertyName + ">k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic);
            AssertEx.NotNull(field);
            field.SetValue(target, value);
        }
    }
}
