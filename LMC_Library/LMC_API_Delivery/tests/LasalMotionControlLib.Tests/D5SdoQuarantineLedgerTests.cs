using System;
using System.Collections.Generic;
using LasalMotionControlApiExample;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class D5SdoQuarantineLedgerTests
    {
        private const uint BootId1 = 0x12345678u;
        private const uint BootId2 = 0x89ABCDEFu;
        private const uint MapRevision1 = 0x10203040u;
        private const uint MapRevision2 = 0x50607080u;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Qualification.D5QuarantineLedger.ArmDisarmExactAndOrdered",
                ArmDisarmExactAndOrdered);
            tests.Add(
                "Qualification.D5QuarantineLedger.ValidationAndHandlesFailClosed",
                ValidationAndHandlesFailClosed);
            tests.Add(
                "Qualification.D5QuarantineLedger.ReconcileSnapshotIsImmutable",
                ReconcileSnapshotIsImmutable);
            tests.Add(
                "Qualification.D5QuarantineLedger.AcceptedIdentityIsExact",
                AcceptedIdentityIsExact);
            tests.Add(
                "Qualification.D5QuarantineLedger.OperationKindsAreExact",
                OperationKindsAreExact);
            tests.Add(
                "Qualification.D5QuarantineLedger.RecoveryClearIsConditional",
                RecoveryClearIsConditional);
        }

        private static void ArmDisarmExactAndOrdered()
        {
            using (var connection = new LMCConnection())
            {
                var ledger = new D5SdoQuarantineLedger();
                AssertEx.False(ledger.HasEntries);
                AssertEx.Equal(0, ledger.Count);

                var first = ArmUnknown(ledger, connection, "first");
                var second = ArmUnknown(ledger, connection, "second");
                var snapshot = ledger.CaptureSnapshot();

                AssertEx.True(ledger.HasEntries);
                AssertEx.Equal(2, ledger.Count);
                AssertEx.Equal(2, snapshot.Entries.Count);
                AssertEvidence(
                    snapshot.Entries[0],
                    0,
                    BootId1,
                    MapRevision1,
                    1,
                    connection,
                    "first");
                AssertEvidence(
                    snapshot.Entries[1],
                    0,
                    BootId1,
                    MapRevision1,
                    1,
                    connection,
                    "second");
                AssertEx.True(
                    snapshot.Entries[0].EntryId
                        < snapshot.Entries[1].EntryId);

                var removedFirst = ledger.Disarm(first);
                AssertEx.Equal("first", removedFirst.EvidenceId);
                AssertEx.Equal(1, ledger.Count);
                var removedSecond = ledger.Disarm(second);
                AssertEx.Equal("second", removedSecond.EvidenceId);
                AssertEx.False(ledger.HasEntries);
                AssertEx.Equal(0, ledger.Count);
                AssertEx.Throws<InvalidOperationException>(
                    () => ledger.Disarm(first));
            }
        }

        private static void ValidationAndHandlesFailClosed()
        {
            using (var connection = new LMCConnection())
            {
                var ledger = new D5SdoQuarantineLedger();
                AssertEx.Throws<ArgumentOutOfRangeException>(
                    () => ledger.ArmUnknown(
                        connection,
                        0,
                        MapRevision1,
                        1,
                        100,
                        "stage",
                        "reason",
                        "zero-boot"));
                AssertEx.Throws<ArgumentOutOfRangeException>(
                    () => ledger.ArmUnknown(
                        connection,
                        BootId1,
                        0,
                        1,
                        100,
                        "stage",
                        "reason",
                        "zero-map"));
                AssertEx.Throws<ArgumentOutOfRangeException>(
                    () => ledger.ArmUnknown(
                        connection,
                        BootId1,
                        MapRevision1,
                        0,
                        100,
                        "stage",
                        "reason",
                        "bad-slave"));
                AssertEx.Throws<ArgumentOutOfRangeException>(
                    () => ledger.ArmUnknown(
                        connection,
                        BootId1,
                        MapRevision1,
                        1,
                        60001,
                        "stage",
                        "reason",
                        "bad-timeout"));
                AssertEx.Throws<ArgumentNullException>(
                    () => ledger.ArmUnknown(
                        null,
                        BootId1,
                        MapRevision1,
                        1,
                        100,
                        "stage",
                        "reason",
                        "null-owner"));
                AssertEx.Throws<ArgumentException>(
                    () => ledger.ArmUnknown(
                        connection,
                        BootId1,
                        MapRevision1,
                        1,
                        100,
                        " ",
                        "reason",
                        "blank-stage"));
                AssertEx.Throws<ArgumentException>(
                    () => ledger.ArmUnknown(
                        connection,
                        BootId1,
                        MapRevision1,
                        1,
                        100,
                        "stage",
                        " ",
                        "blank-reason"));
                AssertEx.Throws<ArgumentException>(
                    () => ledger.ArmUnknown(
                        connection,
                        BootId1,
                        MapRevision1,
                        1,
                        100,
                        "stage",
                        "reason",
                        " "));

                var handle = ArmUnknown(ledger, connection, "reusable");
                AssertEx.Throws<InvalidOperationException>(
                    () => ArmUnknown(ledger, connection, "reusable"));

                var foreignLedger = new D5SdoQuarantineLedger();
                AssertEx.Throws<InvalidOperationException>(
                    () => foreignLedger.GetEvidence(handle));
                AssertEx.Throws<InvalidOperationException>(
                    () => foreignLedger.Disarm(handle));
                AssertEx.Equal(1, ledger.Count);
                AssertEx.Equal(0, foreignLedger.Count);

                var oldEntryId = ledger.GetEvidence(handle).EntryId;
                ledger.Disarm(handle);
                var replacement = ArmUnknown(
                    ledger,
                    connection,
                    "reusable");
                AssertEx.True(
                    oldEntryId
                        != ledger.GetEvidence(replacement).EntryId);
                AssertEx.Throws<InvalidOperationException>(
                    () => ledger.GetEvidence(handle));

                var foreignSnapshot = foreignLedger.CaptureSnapshot();
                var localSnapshot = ledger.CaptureSnapshot();
                AssertEx.Throws<InvalidOperationException>(
                    () => ledger.TryClearAfterProof(
                        localSnapshot,
                        foreignSnapshot,
                        () => { }));
                AssertEx.Equal(1, ledger.Count);
            }
        }

        private static void ReconcileSnapshotIsImmutable()
        {
            using (var connection = new LMCConnection())
            {
                var ledger = new D5SdoQuarantineLedger();
                var handle = ArmUnknown(ledger, connection, "reconcile");
                var baseline = ledger.CaptureSnapshot();
                var reconciled = ledger.ReconcileUnknown(
                    handle,
                    BootId2,
                    MapRevision2);

                AssertEvidence(
                    reconciled,
                    0,
                    BootId2,
                    MapRevision2,
                    2,
                    connection,
                    "reconcile");
                AssertEvidence(
                    baseline.Entries[0],
                    0,
                    BootId1,
                    MapRevision1,
                    1,
                    connection,
                    "reconcile");

                var versionAfterChange = ledger.CaptureSnapshot().Version;
                var idempotent = ledger.ReconcileUnknown(
                    handle,
                    BootId2,
                    MapRevision2);
                AssertEx.Equal(2L, idempotent.EntryRevision);
                AssertEx.Equal(
                    versionAfterChange,
                    ledger.CaptureSnapshot().Version);
                AssertEx.Throws<ArgumentOutOfRangeException>(
                    () => ledger.ReconcileUnknown(handle, 0, MapRevision2));
                AssertEx.Throws<ArgumentOutOfRangeException>(
                    () => ledger.ReconcileUnknown(handle, BootId2, 0));
                AssertEx.Equal(1, ledger.Count);
            }
        }

        private static void AcceptedIdentityIsExact()
        {
            using (var connection = new LMCConnection())
            {
                var ledger = new D5SdoQuarantineLedger();
                var handle = ArmUnknown(ledger, connection, "accepted");
                var ticket = SdoReadTicket(
                    connection,
                    0x01020304u,
                    BootId2,
                    MapRevision2);
                AssertEx.Equal(
                    MapRevision2,
                    ticket.SubmissionMapRevision);
                var accepted = ledger.TransitionToAccepted(
                    handle,
                    ticket,
                    BootId2,
                    MapRevision2);

                AssertEvidence(
                    accepted,
                    ticket.TicketId,
                    BootId2,
                    MapRevision2,
                    2,
                    connection,
                    "accepted");
                AssertEx.Throws<InvalidOperationException>(
                    () => ledger.TransitionToAccepted(
                        handle,
                        ticket,
                        BootId2,
                        MapRevision2));

                var mismatch = ArmUnknown(
                    ledger,
                    connection,
                    "boot-mismatch");
                var boot1Ticket = SdoReadTicket(
                    connection,
                    0x02030405u,
                    BootId1);
                AssertEx.Throws<InvalidOperationException>(
                    () => ledger.TransitionToAccepted(
                        mismatch,
                        boot1Ticket,
                        BootId2,
                        MapRevision2));
                AssertEx.Equal(
                    0u,
                    ledger.GetEvidence(mismatch).TicketId);
                AssertEx.Throws<ArgumentOutOfRangeException>(
                    () => ledger.TransitionToAccepted(
                        mismatch,
                        boot1Ticket,
                        BootId1,
                        0));
                var duplicateLedger = new D5SdoQuarantineLedger();
                var knownHandle = duplicateLedger.QuarantineKnownTicket(
                    ticket,
                    connection,
                    1,
                    100,
                    "known",
                    "stale",
                    "known-ticket",
                    MapRevision2);
                var knownEvidence =
                    duplicateLedger.GetEvidence(knownHandle);
                AssertEx.Equal(ticket.TicketId, knownEvidence.TicketId);
                AssertEx.Equal(BootId2, knownEvidence.DiagnosticsBootId);
                AssertEx.Equal(MapRevision2, knownEvidence.MapRevision);
                AssertEx.Equal(1L, knownEvidence.EntryRevision);
                AssertEx.True(
                    ReferenceEquals(connection, knownEvidence.OwnerConnection));
                AssertEx.Equal("known", knownEvidence.Stage);
                AssertEx.Equal("stale", knownEvidence.Reason);
                var knownVersion = duplicateLedger.CaptureSnapshot().Version;
                AssertEx.Throws<InvalidOperationException>(
                    () => duplicateLedger.ReconcileUnknown(
                        knownHandle,
                        BootId1,
                        MapRevision1));
                AssertEx.Equal(
                    knownVersion,
                    duplicateLedger.CaptureSnapshot().Version);
                AssertEx.Throws<InvalidOperationException>(
                    () => duplicateLedger.QuarantineKnownTicket(
                        ticket,
                        connection,
                        1,
                        100,
                        "known-copy",
                        "stale-copy",
                        "known-ticket-copy",
                        MapRevision2));
                var duplicateGuard = ArmUnknown(
                    duplicateLedger,
                    connection,
                    "duplicate-guard",
                    BootId2,
                    MapRevision2);
                AssertEx.Throws<InvalidOperationException>(
                    () => duplicateLedger.TransitionToAccepted(
                        duplicateGuard,
                        ticket,
                        BootId2,
                        MapRevision2));
                AssertEx.Equal(
                    0u,
                    duplicateLedger.GetEvidence(duplicateGuard).TicketId);

                using (var foreignConnection = new LMCConnection())
                {
                    AssertEx.True(ticket.BelongsTo(connection));
                    AssertEx.False(ticket.BelongsTo(foreignConnection));
                    AssertEx.False(ticket.BelongsTo(null));
                    var ownerMismatchBaseline =
                        duplicateLedger.CaptureSnapshot();
                    AssertEx.Throws<InvalidOperationException>(
                        () => duplicateLedger.QuarantineKnownTicket(
                            ticket,
                            foreignConnection,
                            1,
                            100,
                            "foreign-known",
                            "wrong-owner",
                            "foreign-known-ticket",
                            MapRevision2));
                    AssertSnapshotUnchanged(
                        ownerMismatchBaseline,
                        duplicateLedger.CaptureSnapshot());
                    var foreignGuard = ArmUnknown(
                        duplicateLedger,
                        foreignConnection,
                        "foreign-guard",
                        BootId2,
                        MapRevision2);
                    var transitionMismatchBaseline =
                        duplicateLedger.CaptureSnapshot();
                    AssertEx.Throws<InvalidOperationException>(
                        () => duplicateLedger.TransitionToAccepted(
                            foreignGuard,
                            ticket,
                            BootId2,
                            MapRevision2));
                    AssertSnapshotUnchanged(
                        transitionMismatchBaseline,
                        duplicateLedger.CaptureSnapshot());

                    var foreignTicket = SdoReadTicket(
                        foreignConnection,
                        ticket.TicketId,
                        BootId2,
                        MapRevision2);
                    var foreignKnown =
                        duplicateLedger.QuarantineKnownTicket(
                            foreignTicket,
                            foreignConnection,
                            1,
                            100,
                            "foreign-known",
                            "separate-owner",
                            "foreign-known-ticket",
                            MapRevision2);
                    var foreignEvidence =
                        duplicateLedger.GetEvidence(foreignKnown);
                    AssertEx.Equal(ticket.TicketId, foreignEvidence.TicketId);
                    AssertEx.True(
                        ReferenceEquals(
                            foreignConnection,
                            foreignEvidence.OwnerConnection));
                }
            }
        }

        private static void OperationKindsAreExact()
        {
            using (var connection = new LMCConnection())
            {
                var ledger = new D5SdoQuarantineLedger();
                var writeRequest = SdoWriteRequest();
                var defaultRead = ledger.ArmUnknown(
                    connection,
                    BootId1,
                    MapRevision1,
                    1,
                    100,
                    "test-stage",
                    "test-reason",
                    "default-read");
                AssertEx.Equal(
                    LMCOperationKind.SDORead,
                    ledger.GetEvidence(defaultRead).OperationKind);

                var writeGuard = ArmUnknown(
                    ledger,
                    connection,
                    "write-accepted",
                    BootId1,
                    MapRevision1,
                    LMCOperationKind.SDOWrite,
                    writeRequest);
                AssertWriteAudit(
                    ledger.GetEvidence(writeGuard),
                    writeRequest);
                var writeTicket = SdoWriteTicket(
                    connection,
                    0x03040506u,
                    BootId1);
                var writeAccepted = ledger.TransitionToAccepted(
                    writeGuard,
                    writeTicket,
                    BootId1,
                    MapRevision1);
                AssertEvidence(
                    writeAccepted,
                    writeTicket.TicketId,
                    BootId1,
                    MapRevision1,
                    2,
                    connection,
                    "write-accepted",
                    LMCOperationKind.SDOWrite);
                AssertWriteAudit(writeAccepted, writeRequest);
                var alteredWriteEvidence = new D5SdoQuarantineEvidence(
                    writeAccepted.EntryId,
                    writeAccepted.EntryRevision,
                    writeAccepted.TicketId,
                    writeAccepted.DiagnosticsBootId,
                    writeAccepted.MapRevision,
                    writeAccepted.OperationKind,
                    LMCSdoRequest.CreateWrite(
                        1,
                        0x2000,
                        1,
                        LMCSignalValueType.UInt32,
                        new byte[] { 0x79, 0x56, 0x34, 0x12 },
                        100),
                    writeAccepted.SlaveReference,
                    writeAccepted.TimeoutCycles,
                    writeAccepted.OwnerConnection,
                    writeAccepted.Stage,
                    writeAccepted.Reason,
                    writeAccepted.EvidenceId);
                AssertEx.False(
                    writeAccepted.ContentEquals(alteredWriteEvidence));

                var knownWriteLedger = new D5SdoQuarantineLedger();
                var knownWrite = knownWriteLedger.QuarantineKnownTicket(
                    writeTicket,
                    writeRequest,
                    connection,
                    1,
                    100,
                    "test-stage",
                    "test-reason",
                    "known-write",
                    MapRevision1);
                AssertWriteAudit(
                    knownWriteLedger.GetEvidence(knownWrite),
                    writeRequest);

                var missingRequestLedger = new D5SdoQuarantineLedger();
                AssertEx.Throws<ArgumentNullException>(
                    () => missingRequestLedger.ArmUnknown(
                        LMCOperationKind.SDOWrite,
                        connection,
                        BootId1,
                        MapRevision1,
                        1,
                        100,
                        "test-stage",
                        "test-reason",
                        "missing-write-request"));
                AssertEx.Throws<ArgumentNullException>(
                    () => missingRequestLedger.QuarantineKnownTicket(
                        writeTicket,
                        connection,
                        1,
                        100,
                        "test-stage",
                        "test-reason",
                        "missing-known-write-request",
                        MapRevision1));
                AssertEx.Equal(0, missingRequestLedger.Count);

                var readGuard = ArmUnknown(
                    ledger,
                    connection,
                    "read-write-mismatch");
                var mismatch = AssertEx.Throws<InvalidOperationException>(
                    () => ledger.TransitionToAccepted(
                        readGuard,
                        SdoWriteTicket(
                            connection,
                            0x04050607u,
                            BootId1),
                        BootId1,
                        MapRevision1));
                AssertEx.Contains("operation kind", mismatch.Message);
                AssertEx.Equal(
                    LMCOperationKind.SDORead,
                    ledger.GetEvidence(readGuard).OperationKind);
                AssertEx.Equal(0u, ledger.GetEvidence(readGuard).TicketId);

                AssertEx.Throws<ArgumentOutOfRangeException>(
                    () => ledger.ArmUnknown(
                        LMCOperationKind.PIWrite,
                        connection,
                        BootId1,
                        MapRevision1,
                        1,
                        100,
                        "test-stage",
                        "test-reason",
                        "invalid-kind"));
                AssertEx.Throws<ArgumentException>(
                    () => ledger.QuarantineKnownTicket(
                        PiWriteTicket(
                            connection,
                            0x05060708u,
                            BootId1),
                        connection,
                        1,
                        100,
                        "test-stage",
                        "test-reason",
                        "pi-write",
                        MapRevision1));
            }
        }

        private static void RecoveryClearIsConditional()
        {
            using (var connection = new LMCConnection())
            {
                var ledger = new D5SdoQuarantineLedger();
                var persistent = ArmUnknown(
                    ledger,
                    connection,
                    "persistent");
                var baseline = ledger.CaptureSnapshot();
                var temporary1 = ArmUnknown(
                    ledger,
                    connection,
                    "proof-temporary-1");
                ledger.TransitionToAccepted(
                    temporary1,
                    SdoReadTicket(connection, 0x11111111u, BootId1),
                    BootId1,
                    MapRevision1);
                ledger.Disarm(temporary1);
                var temporary2 = ArmUnknown(
                    ledger,
                    connection,
                    "proof-temporary-2");
                ledger.TransitionToAccepted(
                    temporary2,
                    SdoReadTicket(connection, 0x22222222u, BootId1),
                    BootId1,
                    MapRevision1);
                ledger.Disarm(temporary2);
                var candidate = ledger.CaptureSnapshot();

                AssertEx.True(baseline.Version != candidate.Version);
                var commitCallbackCount = 0;
                AssertEx.True(
                    ledger.TryClearAfterProof(
                        baseline,
                        candidate,
                        () =>
                        {
                            commitCallbackCount++;
                            AssertEx.True(ledger.HasEntries);
                        }));
                AssertEx.Equal(1, commitCallbackCount);
                AssertEx.False(ledger.HasEntries);
                AssertEx.Throws<InvalidOperationException>(
                    () => ledger.GetEvidence(persistent));

                var changedLedger = new D5SdoQuarantineLedger();
                var changed = ArmUnknown(
                    changedLedger,
                    connection,
                    "changed");
                var changedBaseline = changedLedger.CaptureSnapshot();
                changedLedger.ReconcileUnknown(
                    changed,
                    BootId2,
                    MapRevision2);
                var changedCandidate = changedLedger.CaptureSnapshot();
                var changedCallbackCount = 0;
                AssertEx.False(
                    changedLedger.TryClearAfterProof(
                        changedBaseline,
                        changedCandidate,
                        () => changedCallbackCount++));
                AssertEx.Equal(0, changedCallbackCount);
                AssertEx.Equal(1, changedLedger.Count);

                var transitionedLedger = new D5SdoQuarantineLedger();
                var transitioned = ArmUnknown(
                    transitionedLedger,
                    connection,
                    "transitioned-persistent");
                var transitionedBaseline =
                    transitionedLedger.CaptureSnapshot();
                transitionedLedger.TransitionToAccepted(
                    transitioned,
                    SdoReadTicket(connection, 0x33333333u, BootId1),
                    BootId1,
                    MapRevision1);
                var transitionedCandidate =
                    transitionedLedger.CaptureSnapshot();
                var transitionedCallbackCount = 0;
                AssertEx.False(
                    transitionedLedger.TryClearAfterProof(
                        transitionedBaseline,
                        transitionedCandidate,
                        () => transitionedCallbackCount++));
                AssertEx.Equal(0, transitionedCallbackCount);
                AssertEx.Equal(
                    0x33333333u,
                    transitionedLedger.GetEvidence(transitioned).TicketId);

                var staleCandidateLedger = new D5SdoQuarantineLedger();
                ArmUnknown(
                    staleCandidateLedger,
                    connection,
                    "stale-candidate-base");
                var staleBaseline = staleCandidateLedger.CaptureSnapshot();
                var staleCandidate = staleCandidateLedger.CaptureSnapshot();
                var postCandidate = ArmUnknown(
                    staleCandidateLedger,
                    connection,
                    "post-candidate-add");
                staleCandidateLedger.TransitionToAccepted(
                    postCandidate,
                    SdoReadTicket(connection, 0x44444444u, BootId1),
                    BootId1,
                    MapRevision1);
                staleCandidateLedger.Disarm(postCandidate);
                var staleCallbackCount = 0;
                AssertEx.False(
                    staleCandidateLedger.TryClearAfterProof(
                        staleBaseline,
                        staleCandidate,
                        () => staleCallbackCount++));
                AssertEx.Equal(0, staleCallbackCount);
                AssertEx.Equal(1, staleCandidateLedger.Count);

                var replacementLedger = new D5SdoQuarantineLedger();
                var original = ArmUnknown(
                    replacementLedger,
                    connection,
                    "same-evidence");
                var replacementBaseline =
                    replacementLedger.CaptureSnapshot();
                replacementLedger.Disarm(original);
                ArmUnknown(
                    replacementLedger,
                    connection,
                    "same-evidence");
                var replacementCandidate =
                    replacementLedger.CaptureSnapshot();
                var replacementCallbackCount = 0;
                AssertEx.False(
                    replacementLedger.TryClearAfterProof(
                        replacementBaseline,
                        replacementCandidate,
                        () => replacementCallbackCount++));
                AssertEx.Equal(0, replacementCallbackCount);
                AssertEx.Equal(1, replacementLedger.Count);

                var callbackLedger = new D5SdoQuarantineLedger();
                ArmUnknown(
                    callbackLedger,
                    connection,
                    "callback-failure");
                var callbackBaseline = callbackLedger.CaptureSnapshot();
                var callbackCandidate = callbackLedger.CaptureSnapshot();
                AssertEx.Throws<InvalidOperationException>(
                    () => callbackLedger.TryClearAfterProof(
                        callbackBaseline,
                        callbackCandidate,
                        () =>
                        {
                            throw new InvalidOperationException(
                                "log failure");
                        }));
                AssertEx.Equal(1, callbackLedger.Count);
                var callbackProbe = ArmUnknown(
                    callbackLedger,
                    connection,
                    "callback-failure-probe");
                callbackLedger.Disarm(callbackProbe);
                var callbackRetry = callbackLedger.CaptureSnapshot();
                AssertEx.True(
                    callbackLedger.TryClearAfterProof(
                        callbackRetry,
                        callbackRetry,
                        () => { }));

                var callbackMutationLedger = new D5SdoQuarantineLedger();
                ArmUnknown(
                    callbackMutationLedger,
                    connection,
                    "callback-mutation-base");
                var callbackMutationBaseline =
                    callbackMutationLedger.CaptureSnapshot();
                var callbackMutationCandidate =
                    callbackMutationLedger.CaptureSnapshot();
                AssertEx.Throws<InvalidOperationException>(
                    () => callbackMutationLedger.TryClearAfterProof(
                        callbackMutationBaseline,
                        callbackMutationCandidate,
                        () => ArmUnknown(
                            callbackMutationLedger,
                            connection,
                            "illegal-callback-mutation")));
                AssertEx.Equal(1, callbackMutationLedger.Count);
                var mutationProbe = ArmUnknown(
                    callbackMutationLedger,
                    connection,
                    "callback-mutation-probe");
                callbackMutationLedger.Disarm(mutationProbe);
                var mutationRetry =
                    callbackMutationLedger.CaptureSnapshot();
                AssertEx.True(
                    callbackMutationLedger.TryClearAfterProof(
                        mutationRetry,
                        mutationRetry,
                        () => { }));

                var writeLedger = new D5SdoQuarantineLedger();
                ArmUnknown(
                    writeLedger,
                    connection,
                    "write-uncertain",
                    BootId1,
                    MapRevision1,
                    LMCOperationKind.SDOWrite);
                var writeBaseline = writeLedger.CaptureSnapshot();
                var writeCandidate = writeLedger.CaptureSnapshot();
                var writeCallbackCount = 0;
                var writeError = AssertEx.Throws<InvalidOperationException>(
                    () => writeLedger.TryClearAfterProof(
                        writeBaseline,
                        writeCandidate,
                        () => writeCallbackCount++));
                AssertEx.Contains("write-uncertain", writeError.Message);
                AssertEx.Contains("SDOWrite", writeError.Message);
                AssertEx.Contains(
                    "Automatic recovery is unavailable",
                    writeError.Message);
                AssertEx.Contains(
                    "quarantine must remain active",
                    writeError.Message);
                AssertEx.Equal(0, writeCallbackCount);
                AssertEx.Equal(1, writeLedger.Count);

                var staleWriteLedger = new D5SdoQuarantineLedger();
                var staleWrite = ArmUnknown(
                    staleWriteLedger,
                    connection,
                    "stale-write",
                    BootId1,
                    MapRevision1,
                    LMCOperationKind.SDOWrite);
                var staleWriteBaseline =
                    staleWriteLedger.CaptureSnapshot();
                var staleWriteCandidate =
                    staleWriteLedger.CaptureSnapshot();
                staleWriteLedger.Disarm(staleWrite);
                var liveAfterDisarm = staleWriteLedger.CaptureSnapshot();
                var staleWriteCallbackCount = 0;
                AssertEx.False(
                    staleWriteLedger.TryClearAfterProof(
                        staleWriteBaseline,
                        staleWriteCandidate,
                        () => staleWriteCallbackCount++));
                AssertEx.Equal(0, staleWriteCallbackCount);
                AssertSnapshotUnchanged(
                    liveAfterDisarm,
                    staleWriteLedger.CaptureSnapshot());
            }
        }

        private static D5SdoQuarantineHandle ArmUnknown(
            D5SdoQuarantineLedger ledger,
            LMCConnection ownerConnection,
            string evidenceId,
            uint diagnosticsBootId = BootId1,
            uint mapRevision = MapRevision1,
            LMCOperationKind operationKind = LMCOperationKind.SDORead,
            LMCSdoRequest request = null)
        {
            if (operationKind == LMCOperationKind.SDOWrite
                && request == null)
            {
                request = SdoWriteRequest();
            }

            return ledger.ArmUnknown(
                operationKind,
                request,
                ownerConnection,
                diagnosticsBootId,
                mapRevision,
                1,
                100,
                "test-stage",
                "test-reason",
                evidenceId);
        }

        private static LMCSdoRequest SdoWriteRequest()
        {
            return LMCSdoRequest.CreateWrite(
                1,
                0x2000,
                1,
                LMCSignalValueType.UInt32,
                new byte[] { 0x78, 0x56, 0x34, 0x12 },
                100);
        }

        private static LMCOperationTicket SdoReadTicket(
            LMCConnection connection,
            uint ticketId,
            uint diagnosticsBootId,
            uint mapRevision = MapRevision1)
        {
            return new LMCOperationTicket(
                ticketId,
                LMCOperationKind.SDORead,
                10,
                diagnosticsBootId,
                mapRevision,
                1,
                connection.Diagnostics,
                true,
                1,
                LMCSignalValueType.Int8);
        }

        private static LMCOperationTicket SdoWriteTicket(
            LMCConnection connection,
            uint ticketId,
            uint diagnosticsBootId,
            uint mapRevision = MapRevision1)
        {
            return new LMCOperationTicket(
                ticketId,
                LMCOperationKind.SDOWrite,
                10,
                diagnosticsBootId,
                mapRevision,
                1,
                connection.Diagnostics,
                false,
                0,
                LMCSignalValueType.Invalid);
        }

        private static LMCOperationTicket PiWriteTicket(
            LMCConnection connection,
            uint ticketId,
            uint diagnosticsBootId,
            uint mapRevision = MapRevision1)
        {
            return new LMCOperationTicket(
                ticketId,
                LMCOperationKind.PIWrite,
                10,
                diagnosticsBootId,
                mapRevision,
                1,
                connection.Diagnostics,
                false,
                0,
                LMCSignalValueType.Invalid);
        }

        private static void AssertEvidence(
            D5SdoQuarantineEvidence evidence,
            uint ticketId,
            uint diagnosticsBootId,
            uint mapRevision,
            long entryRevision,
            LMCConnection ownerConnection,
            string evidenceId,
            LMCOperationKind operationKind = LMCOperationKind.SDORead)
        {
            AssertEx.NotNull(evidence);
            AssertEx.Equal(ticketId, evidence.TicketId);
            AssertEx.Equal(diagnosticsBootId, evidence.DiagnosticsBootId);
            AssertEx.Equal(mapRevision, evidence.MapRevision);
            AssertEx.Equal(operationKind, evidence.OperationKind);
            AssertEx.Equal(entryRevision, evidence.EntryRevision);
            AssertEx.Equal((ushort)1, evidence.SlaveReference);
            AssertEx.Equal(100u, evidence.TimeoutCycles);
            AssertEx.True(
                ReferenceEquals(ownerConnection, evidence.OwnerConnection));
            AssertEx.Equal("test-stage", evidence.Stage);
            AssertEx.Equal("test-reason", evidence.Reason);
            AssertEx.Equal(evidenceId, evidence.EvidenceId);
        }

        private static void AssertWriteAudit(
            D5SdoQuarantineEvidence evidence,
            LMCSdoRequest request)
        {
            AssertEx.NotNull(evidence);
            AssertEx.True(evidence.HasRequestMetadata);
            AssertEx.Equal(LMCOperationKind.SDOWrite, evidence.OperationKind);
            AssertEx.Equal(request.ObjectIndex, evidence.ObjectIndex);
            AssertEx.Equal(request.SubIndex, evidence.SubIndex);
            AssertEx.Equal(request.ValueType, evidence.ValueType);
            AssertEx.Equal(request.DataLength, evidence.DataLength);
            AssertEx.SequenceEqual(request.WriteData, evidence.WriteData);

            var returnedWriteData = evidence.WriteData;
            returnedWriteData[0] ^= 0xFF;
            AssertEx.SequenceEqual(request.WriteData, evidence.WriteData);
        }

        private static void AssertSnapshotUnchanged(
            D5SdoQuarantineSnapshot expected,
            D5SdoQuarantineSnapshot actual)
        {
            AssertEx.Equal(expected.Version, actual.Version);
            AssertEx.Equal(expected.Entries.Count, actual.Entries.Count);
            for (var index = 0; index < expected.Entries.Count; index++)
            {
                AssertEx.True(
                    expected.Entries[index].ContentEquals(
                        actual.Entries[index]));
            }
        }
    }
}
