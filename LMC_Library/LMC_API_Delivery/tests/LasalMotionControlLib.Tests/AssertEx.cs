using System;
using System.Collections.Generic;

namespace LasalMotionControlLib.Tests
{
    internal sealed class TestCase
    {
        internal TestCase(string name, Action body)
        {
            Name = name;
            Body = body;
        }

        internal string Name { get; private set; }
        internal Action Body { get; private set; }
    }

    internal static class TestRegistration
    {
        internal static void Add(
            this ICollection<TestCase> tests,
            string name,
            Action body)
        {
            tests.Add(new TestCase(name, body));
        }
    }

    internal static class AssertEx
    {
        internal static void True(bool actual, string message = null)
        {
            if (!actual)
            {
                throw new InvalidOperationException(message ?? "Expected true.");
            }
        }

        internal static void False(bool actual, string message = null)
        {
            if (actual)
            {
                throw new InvalidOperationException(message ?? "Expected false.");
            }
        }

        internal static void NotNull(object actual, string message = null)
        {
            if (actual == null)
            {
                throw new InvalidOperationException(message ?? "Expected a non-null value.");
            }
        }

        internal static void Equal<T>(T expected, T actual, string message = null)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new InvalidOperationException(
                    (message == null ? string.Empty : message + " ")
                    + "Expected <" + expected + ">, actual <" + actual + ">.");
            }
        }

        internal static void SequenceEqual(
            byte[] expected,
            byte[] actual,
            string message = null)
        {
            if (ReferenceEquals(expected, actual))
            {
                return;
            }

            if (expected == null || actual == null)
            {
                throw new InvalidOperationException(
                    (message ?? "Byte sequence mismatch.")
                    + " One sequence is null.");
            }

            if (expected.Length != actual.Length)
            {
                throw new InvalidOperationException(
                    (message ?? "Byte sequence length mismatch.")
                    + " Expected " + expected.Length
                    + " bytes, actual " + actual.Length
                    + " bytes. Expected=" + TestFrame.ToHex(expected)
                    + ", Actual=" + TestFrame.ToHex(actual));
            }

            for (var index = 0; index < expected.Length; index++)
            {
                if (expected[index] != actual[index])
                {
                    throw new InvalidOperationException(
                        (message ?? "Byte sequence mismatch.")
                        + " Offset " + index
                        + ": expected 0x" + expected[index].ToString("X2")
                        + ", actual 0x" + actual[index].ToString("X2")
                        + ". Expected=" + TestFrame.ToHex(expected)
                        + ", Actual=" + TestFrame.ToHex(actual));
                }
            }
        }

        internal static TException Throws<TException>(
            Action action,
            string message = null)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException ex)
            {
                return ex;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    (message ?? "Unexpected exception type.")
                    + " Expected " + typeof(TException).FullName
                    + ", actual " + ex.GetType().FullName + ".",
                    ex);
            }

            throw new InvalidOperationException(
                (message ?? "Expected an exception.")
                + " Expected " + typeof(TException).FullName + ".");
        }

        internal static void Contains(string expectedPart, string actual)
        {
            if (actual == null
                || actual.IndexOf(expectedPart, StringComparison.Ordinal) < 0)
            {
                throw new InvalidOperationException(
                    "Expected string containing <" + expectedPart
                    + ">, actual <" + actual + ">.");
            }
        }
    }
}
