using System;
using System.Collections.Generic;

namespace LasalMotionControlLib.Tests
{
    internal static class Program
    {
        private static int Main()
        {
            var tests = new List<TestCase>();

            AdminContractTests.Register(tests);
            ErrorCatalogTests.Register(tests);
            RequestGoldenTests.Register(tests);
            ResponseParserTests.Register(tests);
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
