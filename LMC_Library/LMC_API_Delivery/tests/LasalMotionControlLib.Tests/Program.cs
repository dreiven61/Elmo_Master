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
                if (NegativeWireTool.IsInvocation(args))
                {
                    return NegativeWireTool.Run(args);
                }

                Console.Error.WriteLine(
                    "ERROR arguments are accepted only for the exact negative-wire tool mode.");
                NegativeWireTool.WriteUsage(Console.Error);
                return NegativeWireTool.UsageExitCode;
            }

            return RunTests();
        }

        private static int RunTests()
        {
            var tests = new List<TestCase>();

            AdminContractTests.Register(tests);
            AdminMotionContractTests.Register(tests);
            ErrorCatalogTests.Register(tests);
            RequestGoldenTests.Register(tests);
            ResponseParserTests.Register(tests);
            NegativeWireToolTests.Register(tests);
            ResponsePayloadLimitTests.Register(tests);
            TransportQualificationAnalysisTests.Register(tests);
            RecorderReconnectQualificationPolicyTests.Register(tests);
            GroupStopQualificationOrchestratorTests.Register(tests);
            BulkQualificationCleanupOrchestratorTests.Register(tests);
            BulkPartialQualificationAnalysisTests.Register(tests);
            D5SdoQualificationAnalysisTests.Register(tests);
            D5ExternalReadFailureOrchestratorTests.Register(tests);
            D5SdoQuarantineLedgerTests.Register(tests);
            D5SdoRecoveryScopePolicyTests.Register(tests);
            RpcIntegrationTests.Register(tests);
            DriveReadFacadeTests.Register(tests);
            DiagnosticsContractTests.Register(tests);
            DiagnosticsD1ContractTests.Register(tests);
            DiagnosticsD2ContractTests.Register(tests);
            DiagnosticsPIBulkFacadeContractTests.Register(tests);
            DiagnosticsD5ContractTests.Register(tests);
            DiagnosticsD45CompletionContractTests.Register(tests);
            DiagnosticsRecorderContractTests.Register(tests);

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
