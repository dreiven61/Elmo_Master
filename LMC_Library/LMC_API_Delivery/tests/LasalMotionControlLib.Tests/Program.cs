using System;
using System.Collections.Generic;

namespace LasalMotionControlLib.Tests
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (args != null && args.Length != 0)
            {
                if (DiagnosticsMutationJournalTests
                    .IsCrashChildInvocation(args))
                {
                    return DiagnosticsMutationJournalTests
                        .RunCrashChild(args);
                }

                if (NegativeWireTool.IsInvocation(args))
                {
                    return NegativeWireTool.Run(args);
                }

                if (CallbackOwnershipWireTool.IsInvocation(args))
                {
                    return CallbackOwnershipWireTool.Run(args);
                }

                if (TopologyIoQualificationTool.IsInvocation(args))
                {
                    return TopologyIoQualificationTool.Run(args);
                }

                if (DiagnosticsParserStressTool.IsInvocation(args))
                {
                    return DiagnosticsParserStressTool.Run(args);
                }

                Console.Error.WriteLine(
                    "ERROR arguments are accepted only for an exact internal tool mode.");
                NegativeWireTool.WriteUsage(Console.Error);
                CallbackOwnershipWireTool.WriteUsage(Console.Error);
                TopologyIoQualificationTool.WriteUsage(Console.Error);
                DiagnosticsParserStressTool.WriteUsage(Console.Error);
                return NegativeWireTool.UsageExitCode;
            }

            return RunTests();
        }

        private static int RunTests()
        {
            var tests = new List<TestCase>();

            AdminContractTests.Register(tests);
            AdminMotionContractTests.Register(tests);
            AdminSetAxisPositionContractTests.Register(tests);
            AdminSetAxisPositionOutcomeContractTests.Register(tests);
            AdminSetAxisPositionOutcomeRetirementContractTests.Register(tests);
            AdminSetOperationModeContractTests.Register(tests);
            AdminLmcHomeContractTests.Register(tests);
            AdminDs402HomeCurrentPositionZeroContractTests.Register(tests);
            AxisDs402HomeExParameterContractTests.Register(tests);
            EncoderMaintenanceProtocolContractTests.Register(tests);
            AdminDs402HomeOutcomeRetirementContractTests.Register(tests);
            ErrorCatalogTests.Register(tests);
            RequestGoldenTests.Register(tests);
            CallbackProtocolTests.Register(tests);
            CallbackSessionFencingTests.Register(tests);
            CallbackV2ConnectionTests.Register(tests);
            ResponseParserTests.Register(tests);
            NegativeWireToolTests.Register(tests);
            CallbackOwnershipWireToolTests.Register(tests);
            TopologyIoQualificationToolTests.Register(tests);
            ResponsePayloadLimitTests.Register(tests);
            TransportQualificationAnalysisTests.Register(tests);
            RecorderReconnectQualificationPolicyTests.Register(tests);
            GroupStopQualificationOrchestratorTests.Register(tests);
            GroupStopWaitContractTests.Register(tests);
            GroupDisableWaitContractTests.Register(tests);
            GroupEnableWaitContractTests.Register(tests);
            GroupPowerStateWaitContractTests.Register(tests);
            GroupResetWaitContractTests.Register(tests);
            AxisPowerStateWaitContractTests.Register(tests);
            AxisResetWaitContractTests.Register(tests);
            AxisStopWaitContractTests.Register(tests);
            BulkQualificationCleanupOrchestratorTests.Register(tests);
            BulkPartialQualificationAnalysisTests.Register(tests);
            D5SdoQualificationAnalysisTests.Register(tests);
            D5ExternalReadFailureOrchestratorTests.Register(tests);
            D5SdoQuarantineLedgerTests.Register(tests);
            D5SdoQuarantineLedgerConcurrencyTests.Register(tests);
            D5SdoRecoveryScopePolicyTests.Register(tests);
            D5SdoPendingCleanupOrchestratorTests.Register(tests);
            D5SdoContentionQualificationOrchestratorTests.Register(tests);
            D5SdoTimeoutQualificationOrchestratorTests.Register(tests);
            D5SdoQueuedCancelQualificationOrchestratorTests.Register(tests);
            D5SdoDisconnectOrphanQualificationOrchestratorTests.Register(tests);
            D5SdoWriteSameValueQualificationOrchestratorTests.Register(tests);
            DiagnosticsMutationJournalTests.Register(tests);
            DiagnosticsOperationAdmissionPolicyTests.Register(tests);
            DigitalOutputUncertainAcknowledgementPolicyTests.Register(tests);
            SdoEditorAvailabilityPolicyTests.Register(tests);
            StaleSdoWriteReadbackRecoveryPolicyTests.Register(tests);
            TopologyIoLiveMonitorPolicyTests.Register(tests);
            RpcSendPriorityCoordinatorTests.Register(tests);
            RpcLifecycleConcurrencyTests.Register(tests);
            RpcIntegrationTests.Register(tests);
            DriveReadFacadeTests.Register(tests);
            DriveErrorCodeContractTests.Register(tests);
            DiagnosticsContractTests.Register(tests);
            DiagnosticsD1ContractTests.Register(tests);
            DiagnosticsD2ContractTests.Register(tests);
            DiagnosticsPIBulkFacadeContractTests.Register(tests);
            DiagnosticsD5ContractTests.Register(tests);
            DiagnosticsSdoWritePolicyEvaluationTests.Register(tests);
            DiagnosticsSdoReadFacadeTests.Register(tests);
            DiagnosticsSdoWriteVerificationTests.Register(tests);
            DiagnosticsTopologyIoContractTests.Register(tests);
            DiagnosticsParserDeterministicFuzzTests.Register(tests);
            DiagnosticsParserStressToolTests.Register(tests);
            TopologyBindingContractTests.Register(tests);
            EtherCATIoRtReferenceModelTests.Register(tests);
            DiagnosticsD45CompletionContractTests.Register(tests);
            DiagnosticsRecorderContractTests.Register(tests);
            RecorderDoubleBankQualificationOrchestratorTests.Register(tests);
            RecorderHeaderSemanticCanonicalizerTests.Register(tests);
            RecorderDoubleRecoveryJournalTests.Register(tests);
            RecorderDoubleRecoveryPlannerTests.Register(tests);
            RecorderDoubleRecoveryOrchestratorTests.Register(tests);
            RecorderDoubleQualificationJournalBridgeTests.Register(tests);

            var failed = 0;

            foreach (var test in tests)
            {
                try
                {
                    test.Body();
                    Console.WriteLine("PASS " + test.Name);
                }
                catch (Exception ex)
                {
                    failed++;
                    Console.Error.WriteLine("FAIL " + test.Name);
                    Console.Error.WriteLine(ex);
                }
            }

            Console.WriteLine(
                "TOTAL " + tests.Count
                + ", PASSED " + (tests.Count - failed)
                + ", FAILED " + failed);

            return failed == 0 ? 0 : 1;
        }
    }
}
