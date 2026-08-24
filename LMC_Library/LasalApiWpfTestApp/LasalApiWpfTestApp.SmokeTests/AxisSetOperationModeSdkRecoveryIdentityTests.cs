using System;
using System.Collections.Generic;
using LasalMotionControlLib;
using LasalMotionControlLib.Tests;

namespace LasalApiWpfTestApp.SmokeTests
{
    internal static class AxisSetOperationModeSdkRecoveryIdentityTests
    {
        private const uint DiagnosticsBuild = 0x11223344u;
        private const uint DiagnosticsBootId = 0x55667788u;
        private const uint MapRevision = 0x99AABBCCu;
        private const uint TimeoutMilliseconds = 1000u;
        private const uint RecordGeneration = 7u;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.SetOperationModeSdk.RecoveryBootIdMismatchZeroWire",
                RecoveryBootIdMismatchIsRejectedBeforeWire);
        }

        private static void RecoveryBootIdMismatchIsRejectedBeforeWire()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(1),
                AxisInfoStep(1),
                AdminCapabilitiesStep(),
                DiagnosticsCapabilitiesStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var axis = new LMCSingleAxis(connection, "_LMCAxis1");
                var adminCapabilities = connection.Admin.GetCapabilities();
                var diagnosticCapabilities =
                    connection.Diagnostics.GetCapabilities();
                var wrongBootKey = new LMCAxisSetOperationModeRecoveryKey(
                    1,
                    0x10203040u,
                    DiagnosticsBuild,
                    DiagnosticsBootId + 1u,
                    MapRevision,
                    0x01020304u,
                    0x11121314u,
                    0x21222324u,
                    0x31323334u,
                    1,
                    LMCDriveOperationMode.CyclicSynchronousPosition,
                    TimeoutMilliseconds);

                AssertEx.Throws<InvalidOperationException>(
                    () => axis.ReadSetOperationModeOutcome(
                        wrongBootKey,
                        adminCapabilities,
                        diagnosticCapabilities));
                AssertEx.Throws<InvalidOperationException>(
                    () => axis.RetireSetOperationModeOutcome(
                        wrongBootKey,
                        RecordGeneration,
                        adminCapabilities,
                        diagnosticCapabilities));

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(0, CountCommand(server, 0x7D24));
                AssertEx.Equal(0, CountCommand(server, 0x7D25));
            }
        }

        private static FakeRpcStep InitStep()
        {
            var payload = new byte[24];
            TestFrame.WriteUInt32(payload, 0, 64);
            return new FakeRpcStep(
                0x8080,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep CallbackStep()
        {
            return new FakeRpcStep(
                0x405C,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")));
        }

        private static FakeRpcStep AxisLookupStep(ushort axisReference)
        {
            var payload = new byte[6];
            TestFrame.WriteUInt16(payload, 4, axisReference);
            return new FakeRpcStep(
                0x103C,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep AxisInfoStep(ushort axisReference)
        {
            var payload = new byte[8];
            TestFrame.WriteUInt32(payload, 0, axisReference);
            return new FakeRpcStep(
                0x202B,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep AdminCapabilitiesStep()
        {
            var payload = new byte[40];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, 1);
            TestFrame.WriteUInt32(payload, 16, 0x00000700u);
            TestFrame.WriteUInt16(payload, 28, 4);
            TestFrame.WriteUInt16(payload, 36, 6);
            return new FakeRpcStep(
                0x7D00,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep DiagnosticsCapabilitiesStep()
        {
            var payload = new byte[68];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, 1);
            TestFrame.WriteUInt32(payload, 16, DiagnosticsBuild);
            TestFrame.WriteUInt32(payload, 24, MapRevision);
            TestFrame.WriteUInt32(payload, 40, 1000);
            TestFrame.WriteUInt16(payload, 44, 1320);
            TestFrame.WriteUInt16(payload, 46, 2040);
            TestFrame.WriteUInt16(payload, 48, 1280);
            TestFrame.WriteUInt32(payload, 64, DiagnosticsBootId);
            return new FakeRpcStep(
                0x7E00,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep CloseStep()
        {
            return new FakeRpcStep(
                0x405D,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")));
        }

        private static int CountCommand(
            FakeRpcServer server,
            ushort command)
        {
            var count = 0;
            foreach (var request in server.ReceivedRequests)
            {
                if (TestFrame.ReadUInt16(request, 0) == command)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
