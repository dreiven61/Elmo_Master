using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlApiExample;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class D5SdoQuarantineLedgerConcurrencyTests
    {
        private const uint BootId = 0x12345678u;
        private const uint MapRevision = 0x10203040u;
        private const int IterationCount = 50;
        private const int WaitTimeoutMilliseconds = 5000;

        private sealed class DisarmAttemptResult
        {
            internal D5SdoQuarantineEvidence Evidence;
            internal Exception Error;
        }

        private sealed class TaskObservation<TResult>
        {
            internal TResult Result;
            internal Exception Error;
        }

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Qualification.D5QuarantineConcurrency.PreClearMutationRejects",
                () => Repeat(PreClearMutationRejectsOnce));
            tests.Add(
                "Qualification.D5QuarantineConcurrency.PostCommitArmSurvives",
                () => Repeat(PostCommitArmSurvivesOnce));
            tests.Add(
                "Qualification.D5QuarantineConcurrency.CallbackFailureReleasesWaiter",
                () => Repeat(CallbackFailureReleasesWaiterOnce));
            tests.Add(
                "Qualification.D5QuarantineConcurrency.DisarmIsExactOnce",
                () => Repeat(DisarmIsExactOnce));
        }

        private static void PreClearMutationRejectsOnce()
        {
            using (var connection = new LMCConnection())
            using (var armReady = new ManualResetEventSlim(false))
            using (var allowArm = new ManualResetEventSlim(false))
            {
                var ledger = new D5SdoQuarantineLedger();
                var original = ArmUnknown(
                    ledger,
                    connection,
                    "original");
                var baseline = ledger.CaptureSnapshot();
                var candidate = ledger.CaptureSnapshot();
                var competingArm = Task.Run(
                    () =>
                    {
                        armReady.Set();
                        WaitSignal(allowArm, "allow competing Arm");
                        return ArmUnknown(
                            ledger,
                            connection,
                            "pre-clear-arm");
                    });

                WaitSignal(armReady, "competing Arm ready");
                allowArm.Set();
                var competing = AwaitTask(
                    competingArm,
                    "competing Arm commit");
                var callbackCount = 0;
                var cleared = ledger.TryClearAfterProof(
                    baseline,
                    candidate,
                    () => Interlocked.Increment(ref callbackCount));

                AssertEx.False(cleared);
                AssertEx.Equal(0, callbackCount);
                AssertEx.Equal(2, ledger.Count);
                AssertEx.Equal(
                    "original",
                    ledger.GetEvidence(original).EvidenceId);
                AssertEx.Equal(
                    "pre-clear-arm",
                    ledger.GetEvidence(competing).EvidenceId);
            }
        }

        private static void PostCommitArmSurvivesOnce()
        {
            using (var connection = new LMCConnection())
            using (var callbackEntered = new ManualResetEventSlim(false))
            using (var armObservedProofLock = new ManualResetEventSlim(false))
            {
                var ledger = new D5SdoQuarantineLedger();
                var syncRoot = GetLedgerSyncRoot(ledger);
                var original = ArmUnknown(
                    ledger,
                    connection,
                    "original");
                var baseline = ledger.CaptureSnapshot();
                var candidate = ledger.CaptureSnapshot();
                var callbackCount = 0;
                var callbackHeldLedgerLock = false;
                var clearTask = Task.Run(
                    () => ledger.TryClearAfterProof(
                        baseline,
                        candidate,
                        () =>
                        {
                            Interlocked.Increment(ref callbackCount);
                            callbackHeldLedgerLock =
                                Monitor.IsEntered(syncRoot);
                            callbackEntered.Set();
                            WaitSignal(
                                armObservedProofLock,
                                "competing Arm proof-lock observation");
                        }));

                WaitSignal(callbackEntered, "proof callback entered");
                var competingArm = Task.Run(
                    () => ArmAfterObservingProofLock(
                        ledger,
                        connection,
                        syncRoot,
                        armObservedProofLock,
                        "post-commit-arm"));

                var clearObservation = ObserveTask(clearTask, "proof clear");
                var armObservation = ObserveTask(
                    competingArm,
                    "post-commit Arm");
                AssertEx.True(RequireTaskSuccess(clearObservation));
                var competing = RequireTaskSuccess(armObservation);
                AssertEx.True(
                    callbackHeldLedgerLock,
                    "Proof callback did not run inside the ledger transaction.");
                AssertEx.Equal(1, callbackCount);
                AssertEx.Equal(1, ledger.Count);
                AssertEx.Equal(
                    "post-commit-arm",
                    ledger.GetEvidence(competing).EvidenceId);
                AssertEx.Throws<InvalidOperationException>(
                    () => ledger.GetEvidence(original));
            }
        }

        private static void CallbackFailureReleasesWaiterOnce()
        {
            using (var connection = new LMCConnection())
            using (var callbackEntered = new ManualResetEventSlim(false))
            using (var armObservedProofLock = new ManualResetEventSlim(false))
            {
                var ledger = new D5SdoQuarantineLedger();
                var syncRoot = GetLedgerSyncRoot(ledger);
                var original = ArmUnknown(
                    ledger,
                    connection,
                    "original");
                var baseline = ledger.CaptureSnapshot();
                var candidate = ledger.CaptureSnapshot();
                var callbackHeldLedgerLock = false;
                var clearTask = Task.Run(
                    () => ledger.TryClearAfterProof(
                        baseline,
                        candidate,
                        () =>
                        {
                            callbackHeldLedgerLock =
                                Monitor.IsEntered(syncRoot);
                            callbackEntered.Set();
                            WaitSignal(
                                armObservedProofLock,
                                "competing Arm proof-lock observation");
                            throw new InvalidOperationException(
                                "proof log failure");
                        }));

                WaitSignal(callbackEntered, "proof callback entered");
                var competingArm = Task.Run(
                    () => ArmAfterObservingProofLock(
                        ledger,
                        connection,
                        syncRoot,
                        armObservedProofLock,
                        "after-callback-failure"));

                var clearObservation = ObserveTask(
                    clearTask,
                    "proof clear callback failure");
                var armObservation = ObserveTask(
                    competingArm,
                    "Arm after callback failure");
                var callbackError = RequireTaskFailure<
                    InvalidOperationException,
                    bool>(
                    clearObservation);
                AssertEx.True(
                    callbackHeldLedgerLock,
                    "Proof callback did not run inside the ledger transaction.");
                AssertEx.Equal("proof log failure", callbackError.Message);
                var competing = RequireTaskSuccess(armObservation);
                AssertEx.Equal(2, ledger.Count);
                AssertEx.Equal(
                    "original",
                    ledger.GetEvidence(original).EvidenceId);
                AssertEx.Equal(
                    "after-callback-failure",
                    ledger.GetEvidence(competing).EvidenceId);

                var reusable = ArmUnknown(
                    ledger,
                    connection,
                    "ledger-reusable");
                AssertEx.Equal(
                    "ledger-reusable",
                    ledger.Disarm(reusable).EvidenceId);
                AssertEx.Equal(2, ledger.Count);
            }
        }

        private static void DisarmIsExactOnce()
        {
            using (var connection = new LMCConnection())
            using (var start = new Barrier(3))
            {
                var ledger = new D5SdoQuarantineLedger();
                var handle = ArmUnknown(
                    ledger,
                    connection,
                    "exact-once");
                var firstTask = Task.Run(
                    () => AttemptDisarm(ledger, handle, start));
                var secondTask = Task.Run(
                    () => AttemptDisarm(ledger, handle, start));

                var mainReleased = start.SignalAndWait(
                    WaitTimeoutMilliseconds);
                var firstObservation = ObserveTask(
                    firstTask,
                    "first Disarm attempt");
                var secondObservation = ObserveTask(
                    secondTask,
                    "second Disarm attempt");
                AssertEx.True(
                    mainReleased,
                    "Concurrent Disarm start barrier timed out.");
                var first = RequireTaskSuccess(firstObservation);
                var second = RequireTaskSuccess(secondObservation);

                var successCount = 0;
                var staleCount = 0;
                CountDisarmResult(first, ref successCount, ref staleCount);
                CountDisarmResult(second, ref successCount, ref staleCount);
                AssertEx.Equal(1, successCount);
                AssertEx.Equal(1, staleCount);
                AssertEx.Equal(0, ledger.Count);
                AssertEx.False(ledger.HasEntries);
            }
        }

        private static DisarmAttemptResult AttemptDisarm(
            D5SdoQuarantineLedger ledger,
            D5SdoQuarantineHandle handle,
            Barrier start)
        {
            try
            {
                if (!start.SignalAndWait(WaitTimeoutMilliseconds))
                {
                    throw new TimeoutException(
                        "Concurrent Disarm start barrier timed out.");
                }

                return new DisarmAttemptResult
                {
                    Evidence = ledger.Disarm(handle)
                };
            }
            catch (Exception error)
            {
                return new DisarmAttemptResult
                {
                    Error = error
                };
            }
        }

        private static void CountDisarmResult(
            DisarmAttemptResult result,
            ref int successCount,
            ref int staleCount)
        {
            AssertEx.NotNull(result);
            if (result.Evidence != null)
            {
                AssertEx.Equal("exact-once", result.Evidence.EvidenceId);
                AssertEx.True(result.Error == null);
                successCount++;
                return;
            }

            AssertEx.True(
                result.Error is InvalidOperationException,
                result.Error == null
                    ? "Concurrent Disarm did not report a result."
                    : "Unexpected concurrent Disarm error: "
                        + result.Error.GetType().FullName);
            staleCount++;
        }

        private static void Repeat(Action scenario)
        {
            for (var iteration = 0; iteration < IterationCount; iteration++)
            {
                scenario();
            }
        }

        private static D5SdoQuarantineHandle ArmUnknown(
            D5SdoQuarantineLedger ledger,
            LMCConnection ownerConnection,
            string evidenceId)
        {
            return ledger.ArmUnknown(
                ownerConnection,
                BootId,
                MapRevision,
                1,
                100,
                "concurrency-test",
                "test-reason",
                evidenceId);
        }

        private static D5SdoQuarantineHandle ArmAfterObservingProofLock(
            D5SdoQuarantineLedger ledger,
            LMCConnection ownerConnection,
            object syncRoot,
            ManualResetEventSlim proofLockObserved,
            string evidenceId)
        {
            var entered = Monitor.TryEnter(syncRoot);
            try
            {
                if (entered)
                {
                    throw new InvalidOperationException(
                        "The competing Arm did not observe the proof transaction lock.");
                }
            }
            finally
            {
                if (entered)
                {
                    Monitor.Exit(syncRoot);
                }

                proofLockObserved.Set();
            }

            return ArmUnknown(
                ledger,
                ownerConnection,
                evidenceId);
        }

        private static object GetLedgerSyncRoot(
            D5SdoQuarantineLedger ledger)
        {
            var field = typeof(D5SdoQuarantineLedger).GetField(
                "sync",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null || field.FieldType != typeof(object))
            {
                throw new InvalidOperationException(
                    "The D5 quarantine ledger sync root was not found.");
            }

            var syncRoot = field.GetValue(ledger);
            if (syncRoot == null)
            {
                throw new InvalidOperationException(
                    "The D5 quarantine ledger sync root is null.");
            }

            return syncRoot;
        }

        private static void WaitSignal(
            ManualResetEventSlim signal,
            string stage)
        {
            if (!signal.Wait(WaitTimeoutMilliseconds))
            {
                throw new TimeoutException(
                    "Timed out waiting for " + stage + ".");
            }
        }

        private static TResult AwaitTask<TResult>(
            Task<TResult> task,
            string stage)
        {
            if (task == null)
            {
                throw new ArgumentNullException("task");
            }

            var completed = false;
            try
            {
                completed = task.Wait(WaitTimeoutMilliseconds);
            }
            catch (AggregateException)
            {
                completed = true;
            }

            if (!completed)
            {
                throw new TimeoutException(
                    "Timed out waiting for " + stage + ".");
            }

            return task.GetAwaiter().GetResult();
        }

        private static TaskObservation<TResult> ObserveTask<TResult>(
            Task<TResult> task,
            string stage)
        {
            var observation = new TaskObservation<TResult>();
            try
            {
                observation.Result = AwaitTask(task, stage);
            }
            catch (Exception error)
            {
                observation.Error = error;
            }

            return observation;
        }

        private static TResult RequireTaskSuccess<TResult>(
            TaskObservation<TResult> observation)
        {
            if (observation == null)
            {
                throw new ArgumentNullException("observation");
            }

            if (observation.Error != null)
            {
                throw new InvalidOperationException(
                    "Expected task success but observed "
                    + observation.Error.GetType().FullName
                    + ".",
                    observation.Error);
            }

            return observation.Result;
        }

        private static TException RequireTaskFailure<TException, TResult>(
            TaskObservation<TResult> observation)
            where TException : Exception
        {
            if (observation == null)
            {
                throw new ArgumentNullException("observation");
            }

            var expected = observation.Error as TException;
            if (expected == null)
            {
                throw new InvalidOperationException(
                    observation.Error == null
                        ? "Expected task failure "
                            + typeof(TException).FullName
                            + " but the task succeeded."
                        : "Expected task failure "
                            + typeof(TException).FullName
                            + " but observed "
                            + observation.Error.GetType().FullName
                            + ".",
                    observation.Error);
            }

            return expected;
        }
    }
}
