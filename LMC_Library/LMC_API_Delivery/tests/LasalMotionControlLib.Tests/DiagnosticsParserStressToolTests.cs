using System;
using System.Collections.Generic;
using System.IO;

namespace LasalMotionControlLib.Tests
{
    internal static class DiagnosticsParserStressToolTests
    {
        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Property.DiagnosticsParserStress.OptionsAreStrictAndBounded",
                OptionsAreStrictAndBounded);
            tests.Add(
                "Property.DiagnosticsParserStress.RoundRobinRunIsDeterministic",
                RoundRobinRunIsDeterministic);
            tests.Add(
                "Property.DiagnosticsParserStress.CliReportsBoundedResult",
                CliReportsBoundedResult);
        }

        private static void OptionsAreStrictAndBounded()
        {
            var decimalOptions = DiagnosticsParserStressOptions.Parse(new[]
            {
                "parser-stress",
                "--seed",
                uint.MaxValue.ToString(),
                "--iterations",
                DiagnosticsParserStressOptions.MaximumIterations.ToString()
            });
            AssertEx.Equal(uint.MaxValue, decimalOptions.Seed);
            AssertEx.Equal(
                DiagnosticsParserStressOptions.MaximumIterations,
                decimalOptions.Iterations);

            var hexOptions = DiagnosticsParserStressOptions.Parse(new[]
            {
                "parser-stress",
                "--iterations",
                DiagnosticsParserStressOptions.MinimumIterations.ToString(),
                "--seed",
                "0x89ABCDEF"
            });
            AssertEx.Equal(0x89ABCDEFu, hexOptions.Seed);
            AssertEx.Equal(
                DiagnosticsParserStressOptions.MinimumIterations,
                hexOptions.Iterations);

            var help = DiagnosticsParserStressOptions.Parse(new[]
            {
                "parser-stress",
                "--help"
            });
            AssertEx.True(help.ShowHelp);
            AssertEx.True(DiagnosticsParserStressTool.IsInvocation(new[]
            {
                "parser-stress"
            }));
            AssertEx.False(DiagnosticsParserStressTool.IsInvocation(new[]
            {
                "Parser-Stress"
            }));

            AssertInvalidOptions("parser-stress");
            AssertInvalidOptions(
                "parser-stress", "--seed", "1");
            AssertInvalidOptions(
                "parser-stress", "--iterations", "6");
            AssertInvalidOptions(
                "parser-stress", "--seed", "1",
                "--seed", "2", "--iterations", "6");
            AssertInvalidOptions(
                "parser-stress", "--seed", "1",
                "--iterations", "6", "--iterations", "12");
            AssertInvalidOptions(
                "parser-stress", "--seed", "1",
                "--iterations", "6", "--unknown");
            AssertInvalidOptions(
                "parser-stress", "--help", "--seed", "1",
                "--iterations", "6");
            AssertInvalidOptions(
                "parser-stress", "--seed");
            AssertInvalidOptions(
                "parser-stress", "--seed", "-1",
                "--iterations", "6");
            AssertInvalidOptions(
                "parser-stress", "--seed", "4294967296",
                "--iterations", "6");
            AssertInvalidOptions(
                "parser-stress", "--seed", "0x100000000",
                "--iterations", "6");
            AssertInvalidOptions(
                "parser-stress", "--seed", "0x",
                "--iterations", "6");
            AssertInvalidOptions(
                "parser-stress", "--seed", "1",
                "--iterations", "5");
            AssertInvalidOptions(
                "parser-stress", "--seed", "1",
                "--iterations", "1000001");
            AssertInvalidOptions(
                "parser-stress", "--seed", "1",
                "--iterations", "+6");
        }

        private static void RoundRobinRunIsDeterministic()
        {
            var options = DiagnosticsParserStressOptions.Parse(new[]
            {
                "parser-stress",
                "--seed",
                "0x13572468",
                "--iterations",
                "80"
            });
            var first = DiagnosticsParserStressTool.Execute(options);
            var second = DiagnosticsParserStressTool.Execute(options);

            AssertEx.True(first.Passed);
            AssertEx.True(second.Passed);
            AssertEx.Equal(first.Accepted, second.Accepted);
            AssertEx.Equal(
                first.RejectedInvalidData,
                second.RejectedInvalidData);
            AssertEx.Equal(
                options.Iterations,
                first.Accepted + first.RejectedInvalidData);
            AssertEx.True(first.MaximumObservedRawBytes > 0);
            AssertEx.True(
                first.MaximumObservedRawBytes
                    <= DiagnosticsParserStressTool.MaximumRawBytes);
            AssertEx.Equal(
                DiagnosticsParserStressTool.FamilyCount,
                first.FamilyIterations.Length);
            for (var index = 0;
                index < first.FamilyIterations.Length;
                index++)
            {
                AssertEx.Equal(10, first.FamilyIterations[index]);
                AssertEx.Equal(
                    first.FamilyIterations[index],
                    second.FamilyIterations[index]);
            }
        }

        private static void CliReportsBoundedResult()
        {
            var originalOut = Console.Out;
            var originalError = Console.Error;
            var output = new StringWriter();
            var error = new StringWriter();
            int exitCode;
            try
            {
                Console.SetOut(output);
                Console.SetError(error);
                exitCode = DiagnosticsParserStressTool.Run(new[]
                {
                    "parser-stress",
                    "--seed",
                    "0",
                    "--iterations",
                    "8"
                });
            }
            finally
            {
                Console.SetOut(originalOut);
                Console.SetError(originalError);
            }

            AssertEx.Equal(
                DiagnosticsParserStressTool.SuccessExitCode,
                exitCode);
            AssertEx.Equal(string.Empty, error.ToString());
            AssertEx.Contains("PASS parser-stress", output.ToString());
            AssertEx.Contains("SEED=0 (0x00000000)", output.ToString());
            AssertEx.Contains("ITERATIONS_TOTAL=8", output.ToString());
            AssertEx.Contains("FAMILIES=8", output.ToString());

            originalError = Console.Error;
            error = new StringWriter();
            try
            {
                Console.SetError(error);
                exitCode = DiagnosticsParserStressTool.Run(new[]
                {
                    "parser-stress",
                    "--seed",
                    "1",
                    "--iterations",
                    "7"
                });
            }
            finally
            {
                Console.SetError(originalError);
            }

            AssertEx.Equal(
                DiagnosticsParserStressTool.UsageExitCode,
                exitCode);
            AssertEx.Contains(
                "--iterations must be between 8 and 1000000",
                error.ToString());
        }

        private static void AssertInvalidOptions(params string[] args)
        {
            AssertEx.Throws<ArgumentException>(() =>
                DiagnosticsParserStressOptions.Parse(args));
        }
    }
}
