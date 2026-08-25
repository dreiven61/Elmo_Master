using System;
using System.Collections.Generic;
using System.IO;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class AdminDs402HomeExOutcomeContractTests
    {
        private const uint OriginalRequestId = 0x11223344u;
        private const uint QueryRequestId = 0xA1B2C3D4u;
        private const uint RetireRequestId = 0x01020304u;
        private const uint DiagnosticsBuild = 0x55667788u;
        private const uint DiagnosticsBootId = 0x99AABBCCu;
        private const uint MapRevision = 0xDDEEFF00u;
        private const uint RecordGeneration = 7u;
        private const uint CleanupAll = 0x3Fu;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Response.Admin.Ds402HomeEx.OutcomeSuccessExact176",
                OutcomeSuccessExact176);
            tests.Add(
                "Response.Admin.Ds402HomeEx.RunningSemantics",
                RunningSemantics);
            tests.Add(
                "Response.Admin.Ds402HomeEx.FullKeyMismatchRejected",
                FullKeyMismatchRejected);
            tests.Add(
                "Response.Admin.Ds402HomeEx.CleanupAndReadbackStrict",
                CleanupAndReadbackStrict);
            tests.Add(
                "Response.Admin.Ds402HomeEx.QueryFailureEnvelope",
                QueryFailureEnvelope);
            tests.Add(
                "Response.Admin.Ds402HomeEx.RetireExactGeneration",
                RetireExactGeneration);
        }

        private static void OutcomeSuccessExact176()
        {
            var parsed = LMC_AdminParser.ParseAxisDs402HomeExOutcome(
                TestFrame.Response(
                    0,
                    OutcomePayload(
                        QueryRequestId,
                        LMCAxisDs402HomeExOutcomeRecordState.Succeeded,
                        100,
                        100,
                        10,
                        20,
                        RecordGeneration,
                        CleanupAll,
                        0x1234u)),
                QueryRequestId,
                RecoveryKey());

            AssertEx.True(parsed.Response.IsSuccess);
            AssertEx.Equal(
                LMCAxisDs402HomeExOutcomeRecordState.Succeeded,
                parsed.RecordState);
            AssertEx.Equal((ushort)0, parsed.OriginalCommandStatus);
            AssertEx.Equal((short)0, parsed.OriginalErrorId);
            AssertEx.Equal(0u, parsed.OriginalDetailCode);
            AssertEx.Equal(100, parsed.ActualPosition);
            AssertEx.Equal(100, parsed.ExpectedFinalPosition);
            AssertEx.Equal(10u, parsed.StartCycle);
            AssertEx.Equal(20u, parsed.CompletionCycle);
            AssertEx.Equal(0u, parsed.NativeCommandState);
            AssertEx.Equal(RecordGeneration, parsed.RecordGeneration);
            AssertEx.Equal(
                LMCAxisDs402HomeExCleanupProofFlags.RequiredForSafeTerminal,
                parsed.CleanupProofFlags);
            AssertEx.Equal(0x1234u, parsed.SdoExecutorToken);
        }

        private static void RunningSemantics()
        {
            var parsed = LMC_AdminParser.ParseAxisDs402HomeExOutcome(
                TestFrame.Response(
                    0,
                    OutcomePayload(
                        QueryRequestId,
                        LMCAxisDs402HomeExOutcomeRecordState.Running,
                        0,
                        0,
                        10,
                        0,
                        RecordGeneration,
                        0,
                        0x1234u)),
                QueryRequestId,
                RecoveryKey());

            AssertEx.Equal(
                LMCAxisDs402HomeExOutcomeRecordState.Running,
                parsed.RecordState);
            AssertEx.Equal(0u, parsed.CompletionCycle);
            AssertEx.Equal(
                LMCAxisDs402HomeExCleanupProofFlags.None,
                parsed.CleanupProofFlags);

            AssertEx.Throws<InvalidDataException>(() =>
                LMC_AdminParser.ParseAxisDs402HomeExOutcome(
                    TestFrame.Response(
                        0,
                        OutcomePayload(
                            QueryRequestId,
                            LMCAxisDs402HomeExOutcomeRecordState.Running,
                            0,
                            0,
                            10,
                            11,
                            RecordGeneration,
                            0,
                            0x1234u)),
                    QueryRequestId,
                    RecoveryKey()));
        }

        private static void FullKeyMismatchRejected()
        {
            var bootMismatch = OutcomePayload(
                QueryRequestId,
                LMCAxisDs402HomeExOutcomeRecordState.Succeeded,
                100,
                100,
                10,
                20,
                RecordGeneration,
                CleanupAll,
                0x1234u);
            TestFrame.WriteUInt32(bootMismatch, 24, DiagnosticsBootId + 1u);
            AssertMalformedOutcome(bootMismatch);

            var planMismatch = OutcomePayload(
                QueryRequestId,
                LMCAxisDs402HomeExOutcomeRecordState.Succeeded,
                100,
                100,
                10,
                20,
                RecordGeneration,
                CleanupAll,
                0x1234u);
            TestFrame.WriteInt32(planMismatch, 68, 251);
            AssertMalformedOutcome(planMismatch);

            var nonzeroSpare = OutcomePayload(
                QueryRequestId,
                LMCAxisDs402HomeExOutcomeRecordState.Succeeded,
                100,
                100,
                10,
                20,
                RecordGeneration,
                CleanupAll,
                0x1234u);
            nonzeroSpare[117] = 1;
            AssertMalformedOutcome(nonzeroSpare);
        }

        private static void CleanupAndReadbackStrict()
        {
            AssertMalformedOutcome(OutcomePayload(
                QueryRequestId,
                LMCAxisDs402HomeExOutcomeRecordState.Succeeded,
                100,
                100,
                10,
                20,
                RecordGeneration,
                CleanupAll & ~1u,
                0x1234u));

            AssertMalformedOutcome(OutcomePayload(
                QueryRequestId,
                LMCAxisDs402HomeExOutcomeRecordState.Succeeded,
                99,
                100,
                10,
                20,
                RecordGeneration,
                CleanupAll,
                0x1234u));

            AssertMalformedOutcome(OutcomePayload(
                QueryRequestId,
                LMCAxisDs402HomeExOutcomeRecordState.Succeeded,
                100,
                100,
                10,
                20,
                0,
                CleanupAll,
                0x1234u));

            AssertMalformedOutcome(OutcomePayload(
                QueryRequestId,
                LMCAxisDs402HomeExOutcomeRecordState.Succeeded,
                100,
                100,
                10,
                20,
                RecordGeneration,
                CleanupAll | 0x40u,
                0x1234u));

            AssertMalformedOutcome(OutcomePayload(
                QueryRequestId,
                LMCAxisDs402HomeExOutcomeRecordState.Succeeded,
                100,
                100,
                10,
                20,
                RecordGeneration,
                CleanupAll,
                0));
        }

        private static void QueryFailureEnvelope()
        {
            var failure = CommonFailurePayload(QueryRequestId, 53u);
            var error = AssertEx.Throws<LMCAxisDs402HomeExOutcomeQueryException>(
                () => LMC_AdminParser.ParseAxisDs402HomeExOutcome(
                    TestFrame.Response(0, failure),
                    QueryRequestId,
                    RecoveryKey()));
            AssertEx.Equal(53u, error.Response.DetailCodeValue);
            AssertEx.Equal(OriginalRequestId, error.RecoveryKey.OriginalRequestId);
        }

        private static void RetireExactGeneration()
        {
            var parsed = LMC_AdminParser.ParseAxisDs402HomeExOutcomeRetirement(
                TestFrame.Response(
                    0,
                    OutcomePayload(
                        RetireRequestId,
                        LMCAxisDs402HomeExOutcomeRecordState.Succeeded,
                        100,
                        100,
                        10,
                        20,
                        RecordGeneration,
                        CleanupAll,
                        0x1234u)),
                RetireRequestId,
                RecoveryKey(),
                RecordGeneration);
            AssertEx.Equal(RecordGeneration, parsed.RecordGeneration);

            AssertEx.Throws<InvalidDataException>(() =>
                LMC_AdminParser.ParseAxisDs402HomeExOutcomeRetirement(
                    TestFrame.Response(
                        0,
                        OutcomePayload(
                            RetireRequestId,
                            LMCAxisDs402HomeExOutcomeRecordState.Succeeded,
                            100,
                            100,
                            10,
                            20,
                            RecordGeneration + 1u,
                            CleanupAll,
                            0x1234u)),
                    RetireRequestId,
                    RecoveryKey(),
                    RecordGeneration));

            var failure = CommonFailurePayload(RetireRequestId, 56u);
            var error = AssertEx.Throws<LMCAxisDs402HomeExOutcomeRetirementException>(
                () => LMC_AdminParser.ParseAxisDs402HomeExOutcomeRetirement(
                    TestFrame.Response(0, failure),
                    RetireRequestId,
                    RecoveryKey(),
                    RecordGeneration));
            AssertEx.Equal(56u, error.Response.DetailCodeValue);
            AssertEx.Equal(RecordGeneration, error.ExpectedRecordGeneration);
        }

        private static void AssertMalformedOutcome(byte[] payload)
        {
            AssertEx.Throws<InvalidDataException>(() =>
                LMC_AdminParser.ParseAxisDs402HomeExOutcome(
                    TestFrame.Response(0, payload),
                    QueryRequestId,
                    RecoveryKey()));
        }

        private static byte[] CommonFailurePayload(
            uint requestId,
            uint detailCode)
        {
            var payload = new byte[16];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt16(payload, 2, 0);
            TestFrame.WriteUInt16(payload, 4, 1);
            TestFrame.WriteInt16(payload, 6, -31000);
            TestFrame.WriteUInt32(payload, 8, requestId);
            TestFrame.WriteUInt32(payload, 12, detailCode);
            return payload;
        }

        private static byte[] OutcomePayload(
            uint requestId,
            LMCAxisDs402HomeExOutcomeRecordState recordState,
            int actualPosition,
            int expectedFinalPosition,
            uint startCycle,
            uint completionCycle,
            uint recordGeneration,
            uint cleanupProofFlags,
            uint sdoExecutorToken)
        {
            var payload = new byte[176];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt16(payload, 2, 0);
            TestFrame.WriteUInt16(payload, 4, 0);
            TestFrame.WriteInt16(payload, 6, 0);
            TestFrame.WriteUInt32(payload, 8, requestId);
            TestFrame.WriteUInt32(payload, 12, 0);
            TestFrame.WriteUInt16(payload, 16, (ushort)recordState);
            TestFrame.WriteUInt16(payload, 18, 0);
            TestFrame.WriteUInt32(payload, 20, DiagnosticsBuild);
            TestFrame.WriteUInt32(payload, 24, DiagnosticsBootId);
            TestFrame.WriteUInt32(payload, 28, MapRevision);
            TestFrame.WriteUInt32(payload, 32, OriginalRequestId);
            TestFrame.WriteUInt32(payload, 36, 0x01234567u);
            TestFrame.WriteUInt32(payload, 40, 0x89ABCDEFu);
            TestFrame.WriteUInt32(payload, 44, 0x10203040u);
            TestFrame.WriteUInt32(payload, 48, 0x50607080u);
            TestFrame.WriteUInt16(payload, 52, 2);
            TestFrame.WriteUInt16(payload, 54, 0);
            TestFrame.WriteInt32(payload, 56, 1);
            TestFrame.WriteInt32(payload, 60, -100);
            TestFrame.WriteInt32(payload, 64, -4);
            TestFrame.WriteInt32(payload, 68, 250);
            TestFrame.WriteInt32(payload, 72, 500);
            TestFrame.WriteInt32(payload, 76, 25);
            TestFrame.WriteInt32(payload, 80, 0);
            TestFrame.WriteInt32(payload, 84, 0);
            TestFrame.WriteUInt16(
                payload,
                88,
                (ushort)LMCDs402HomeBufferMode.Aborting);
            TestFrame.WriteUInt16(payload, 90, 0);
            TestFrame.WriteUInt32(payload, 92, 60000u);
            TestFrame.WriteUInt32(payload, 96, 5000u);
            TestFrame.WriteUInt16(payload, 132, 0);
            TestFrame.WriteInt16(payload, 134, 0);
            TestFrame.WriteUInt32(payload, 136, 0);
            TestFrame.WriteUInt16(payload, 140, 0x1234);
            TestFrame.WriteUInt16(payload, 142, 0);
            TestFrame.WriteInt32(payload, 144, actualPosition);
            TestFrame.WriteInt32(payload, 148, expectedFinalPosition);
            TestFrame.WriteUInt32(payload, 152, startCycle);
            TestFrame.WriteUInt32(payload, 156, completionCycle);
            TestFrame.WriteUInt32(payload, 160, 0);
            TestFrame.WriteUInt32(payload, 164, recordGeneration);
            TestFrame.WriteUInt32(payload, 168, cleanupProofFlags);
            TestFrame.WriteUInt32(payload, 172, sdoExecutorToken);
            return payload;
        }

        private static LMCAxisDs402HomeExRecoveryKey RecoveryKey()
        {
            return new LMCAxisDs402HomeExRecoveryKey(
                1,
                OriginalRequestId,
                DiagnosticsBuild,
                DiagnosticsBootId,
                MapRevision,
                new LMCAxisDs402HomeExClientIntentId(
                    0x01234567u,
                    0x89ABCDEFu,
                    0x10203040u,
                    0x50607080u),
                2,
                new LMCAxisDs402HomeExExecutionPlan(
                    1,
                    -100,
                    -4,
                    250,
                    500,
                    25,
                    0,
                    0,
                    LMCDs402HomeBufferMode.Aborting,
                    60000,
                    5000,
                    new byte[32]));
        }
    }
}
