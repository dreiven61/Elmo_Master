using System;
using System.Collections.Generic;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class AdminDs402HomeH37QualificationTests
    {
        private const uint OriginalRequestId = 0x11223344u;
        private const uint DiagnosticsBuild = 0x55667788u;
        private const uint DiagnosticsBootId = 0x99AABBCCu;
        private const uint MapRevision = 0xDDEEFF00u;
        private const uint QueryRequestId = 0x01020304u;
        private const uint RetireRequestId = 0x05060708u;
        private const uint RecordGeneration = 0xA1B2C3D4u;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Qualification.H37.Method37ExactPacketSequence",
                Method37ExactPacketSequence);
            tests.Add(
                "Qualification.H37.RunningTerminalRetireSequence",
                RunningTerminalRetireSequence);
        }

        private static void Method37ExactPacketSequence()
        {
            var key = RecoveryKey();
            var start = LMC_AdminFrame.StartAxisDs402Home(key);
            var query = LMC_AdminFrame.ReadAxisDs402HomeOutcome(
                QueryRequestId,
                key);
            var retire = LMC_AdminFrame.RetireAxisDs402HomeOutcome(
                RetireRequestId,
                key,
                RecordGeneration);

            AssertEx.Equal((ushort)0x7D15, TestFrame.ReadUInt16(start, 0));
            AssertEx.Equal((ushort)72, TestFrame.ReadUInt16(start, 4));
            AssertEx.Equal(OriginalRequestId, TestFrame.ReadUInt32(start, 12));
            AssertEx.Equal(37, TestFrame.ReadInt32(start, 44));
            AssertEx.Equal(0, TestFrame.ReadInt32(start, 48));
            AssertEx.Equal(0, TestFrame.ReadInt32(start, 52));
            AssertEx.Equal(0, TestFrame.ReadInt32(start, 56));
            AssertEx.Equal(0, TestFrame.ReadInt32(start, 60));
            AssertEx.Equal(0, TestFrame.ReadInt32(start, 64));

            AssertEx.Equal((ushort)0x7D16, TestFrame.ReadUInt16(query, 0));
            AssertEx.Equal((ushort)44, TestFrame.ReadUInt16(query, 4));
            AssertEx.Equal(QueryRequestId, TestFrame.ReadUInt32(query, 12));
            AssertEx.Equal(OriginalRequestId, TestFrame.ReadUInt32(query, 28));
            AssertEx.Equal(37, TestFrame.ReadInt32(query, 48));

            AssertEx.Equal((ushort)0x7D17, TestFrame.ReadUInt16(retire, 0));
            AssertEx.Equal((ushort)48, TestFrame.ReadUInt16(retire, 4));
            AssertEx.Equal(RetireRequestId, TestFrame.ReadUInt32(retire, 12));
            AssertEx.Equal(OriginalRequestId, TestFrame.ReadUInt32(retire, 28));
            AssertEx.Equal(37, TestFrame.ReadInt32(retire, 48));
            AssertEx.Equal(RecordGeneration, TestFrame.ReadUInt32(retire, 52));
        }

        private static void RunningTerminalRetireSequence()
        {
            var key = RecoveryKey();

            var running = LMC_AdminParser.ParseAxisDs402HomeOutcome(
                TestFrame.Response(
                    0,
                    OutcomePayload(
                        QueryRequestId,
                        key,
                        LMCAxisDs402HomeOutcomeRecordState.Running,
                        100,
                        0,
                        0,
                        0,
                        0)),
                QueryRequestId,
                key);
            AssertEx.Equal(
                LMCAxisDs402HomeOutcomeRecordState.Running,
                running.RecordState);
            AssertEx.Equal(100u, running.StartCycle);
            AssertEx.Equal(0u, running.CompletionCycle);
            AssertEx.Equal(RecordGeneration, running.RecordGeneration);

            var terminal = LMC_AdminParser.ParseAxisDs402HomeOutcome(
                TestFrame.Response(
                    0,
                    OutcomePayload(
                        QueryRequestId + 1,
                        key,
                        LMCAxisDs402HomeOutcomeRecordState.Succeeded,
                        100,
                        110,
                        0x0027,
                        0,
                        0)),
                QueryRequestId + 1,
                key);
            AssertEx.Equal(
                LMCAxisDs402HomeOutcomeRecordState.Succeeded,
                terminal.RecordState);
            AssertEx.Equal(0, terminal.ActualPosition);
            AssertEx.Equal(100u, terminal.StartCycle);
            AssertEx.Equal(110u, terminal.CompletionCycle);
            AssertEx.Equal(RecordGeneration, terminal.RecordGeneration);

            var retire = LMC_AdminFrame.RetireAxisDs402HomeOutcome(
                RetireRequestId,
                key,
                terminal.RecordGeneration);
            AssertEx.Equal((ushort)0x7D17, TestFrame.ReadUInt16(retire, 0));
            AssertEx.Equal(RecordGeneration, TestFrame.ReadUInt32(retire, 52));
            AssertEx.False(TestFrame.ReadUInt16(retire, 0) == 0x7D15);
        }

        private static LMCAxisDs402HomeRecoveryKey RecoveryKey()
        {
            return new LMCAxisDs402HomeRecoveryKey(
                1,
                OriginalRequestId,
                DiagnosticsBuild,
                DiagnosticsBootId,
                MapRevision,
                new LMCAxisDs402HomeClientIntentId(
                    0x01234567u,
                    0x89ABCDEFu,
                    0x10203040u,
                    0x50607080u),
                2,
                new LMCAxisDs402HomeParameters(60000));
        }

        private static byte[] OutcomePayload(
            uint queryRequestId,
            LMCAxisDs402HomeRecoveryKey key,
            LMCAxisDs402HomeOutcomeRecordState state,
            uint startCycle,
            uint completionCycle,
            ushort statusWord,
            int actualPosition,
            uint nativeCommandState)
        {
            var payload = new byte[92];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt16(payload, 4, 0);
            TestFrame.WriteInt16(payload, 6, 0);
            TestFrame.WriteUInt32(payload, 8, queryRequestId);
            TestFrame.WriteUInt32(payload, 12, 0);
            TestFrame.WriteUInt16(payload, 16, (ushort)state);
            TestFrame.WriteUInt16(payload, 18, 0);
            TestFrame.WriteUInt32(payload, 20, key.DiagnosticsBuild);
            TestFrame.WriteUInt32(payload, 24, key.DiagnosticsBootId);
            TestFrame.WriteUInt32(payload, 28, key.MapRevision);
            TestFrame.WriteUInt32(payload, 32, key.RequestId);
            TestFrame.WriteUInt32(payload, 36, key.ClientIntentId.Word0);
            TestFrame.WriteUInt32(payload, 40, key.ClientIntentId.Word1);
            TestFrame.WriteUInt32(payload, 44, key.ClientIntentId.Word2);
            TestFrame.WriteUInt32(payload, 48, key.ClientIntentId.Word3);
            TestFrame.WriteUInt16(payload, 52, key.AxisReference);
            TestFrame.WriteUInt16(payload, 54, 0);
            TestFrame.WriteInt32(payload, 56, key.Parameters.HomingMethod);
            TestFrame.WriteUInt16(payload, 60, 0);
            TestFrame.WriteInt16(payload, 62, 0);
            TestFrame.WriteUInt32(payload, 64, 0);
            TestFrame.WriteUInt16(payload, 68, statusWord);
            TestFrame.WriteUInt16(payload, 70, 0);
            TestFrame.WriteInt32(payload, 72, actualPosition);
            TestFrame.WriteUInt32(payload, 76, startCycle);
            TestFrame.WriteUInt32(payload, 80, completionCycle);
            TestFrame.WriteUInt32(payload, 84, nativeCommandState);
            TestFrame.WriteUInt32(payload, 88, RecordGeneration);
            return payload;
        }
    }
}
