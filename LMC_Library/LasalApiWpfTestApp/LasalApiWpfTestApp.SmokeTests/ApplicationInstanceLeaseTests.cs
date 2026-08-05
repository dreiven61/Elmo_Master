using System;
using System.Collections.Generic;
using System.Threading;
using LasalMotionControlLib.Tests;

namespace LasalMotionControlApiExample
{
    internal static class ApplicationInstanceLeaseTests
    {
        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.ApplicationInstanceLease.OwnerDisposeAllowsReacquire",
                OwnerDisposeAllowsReacquire);
            tests.Add(
                "Wpf.ApplicationInstanceLease.RejectedContenderCannotRelease",
                RejectedContenderCannotReleaseOwner);
        }

        private static void OwnerDisposeAllowsReacquire()
        {
            var mutexName = CreateMutexName();
            ApplicationInstanceLease first = null;
            ApplicationInstanceLease replacement = null;
            try
            {
                AssertEx.True(
                    ApplicationInstanceLease.TryAcquire(
                        mutexName,
                        out first));
                AssertEx.NotNull(first);

                first.Dispose();
                first = null;

                AssertEx.True(
                    ApplicationInstanceLease.TryAcquire(
                        mutexName,
                        out replacement));
                AssertEx.NotNull(replacement);
            }
            finally
            {
                if (replacement != null)
                {
                    replacement.Dispose();
                }

                if (first != null)
                {
                    first.Dispose();
                }
            }
        }

        private static void RejectedContenderCannotReleaseOwner()
        {
            var mutexName = CreateMutexName();
            ApplicationInstanceLease owner = null;
            ApplicationInstanceLease replacement = null;
            try
            {
                AssertEx.True(
                    ApplicationInstanceLease.TryAcquire(
                        mutexName,
                        out owner));
                AssertEx.NotNull(owner);

                Exception contenderError = null;
                var contenderAcquired = true;
                ApplicationInstanceLease contenderLease = null;
                var contender = new Thread(() =>
                {
                    try
                    {
                        contenderAcquired =
                            ApplicationInstanceLease.TryAcquire(
                                mutexName,
                                out contenderLease);
                    }
                    catch (Exception error)
                    {
                        contenderError = error;
                    }
                    finally
                    {
                        if (contenderLease != null)
                        {
                            contenderLease.Dispose();
                        }
                    }
                });
                contender.Start();
                AssertEx.True(contender.Join(5000));
                if (contenderError != null)
                {
                    throw new InvalidOperationException(
                        "The contender failed unexpectedly.",
                        contenderError);
                }

                AssertEx.False(contenderAcquired);
                AssertEx.Equal<ApplicationInstanceLease>(
                    null,
                    contenderLease);

                ApplicationInstanceLease secondContender;
                AssertEx.False(
                    ApplicationInstanceLease.TryAcquire(
                        mutexName,
                        out secondContender));
                AssertEx.Equal<ApplicationInstanceLease>(
                    null,
                    secondContender);

                owner.Dispose();
                owner = null;

                AssertEx.True(
                    ApplicationInstanceLease.TryAcquire(
                        mutexName,
                        out replacement));
                AssertEx.NotNull(replacement);
            }
            finally
            {
                if (replacement != null)
                {
                    replacement.Dispose();
                }

                if (owner != null)
                {
                    owner.Dispose();
                }
            }
        }

        private static string CreateMutexName()
        {
            return ApplicationInstanceLease.DefaultMutexName
                + ".SmokeTests."
                + Guid.NewGuid().ToString("N");
        }
    }
}
