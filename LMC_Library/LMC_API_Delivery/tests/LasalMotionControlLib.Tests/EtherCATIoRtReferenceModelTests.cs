using System;
using System.Collections.Generic;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    // This is an executable reference for the PLC RT-owner contract.  It does
    // not prove that the LASAL implementation was built, downloaded, or run.
    internal static class EtherCATIoRtReferenceModelTests
    {
        private const byte EtherCATStateInit = 1;
        private const byte EtherCATStatePreOperational = 2;
        private const byte EtherCATStateSafeOperational = 4;
        private const byte EtherCATStateOperational = 8;
        private const uint ClassStateOk = 0;
        private const uint ClassStateNoHardware = 5;
        private const uint SlaveStateIdentityError = 0x0020;
        private const uint FullMask = 0xFFFFFFFFu;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "ReferenceModel.EtherCATIo.Byte0IsLeastSignificant",
                Byte0IsLeastSignificant);
            tests.Add(
                "ReferenceModel.EtherCATIo.NoHardwareNormalizesPresence",
                NoHardwareNormalizesPresence);
            tests.Add(
                "ReferenceModel.EtherCATIo.PreOpAndSafeOpRemainDetected",
                PreOpAndSafeOpRemainDetected);
            tests.Add(
                "ReferenceModel.EtherCATIo.OperationalSnapshotIsValid",
                OperationalSnapshotIsValid);
            tests.Add(
                "ReferenceModel.EtherCATIo.MissedFrameDefaultsSnapshot",
                MissedFrameDefaultsSnapshot);
            tests.Add(
                "ReferenceModel.EtherCATIo.ExactStatusCauseMatrix",
                ExactStatusCauseMatrix);
            tests.Add(
                "ReferenceModel.EtherCATIo.SlotUsesParentBusHealth",
                SlotUsesParentBusHealth);
            tests.Add(
                "ReferenceModel.EtherCATIo.SlotRequiresParentPhysicalPresence",
                SlotRequiresParentPhysicalPresence);
            tests.Add(
                "ReferenceModel.EtherCATIo.OutputRevisionTransitions",
                OutputRevisionTransitions);
            tests.Add(
                "ReferenceModel.EtherCATIo.MaskedWritePreservesBits",
                MaskedWritePreservesBits);
            tests.Add(
                "ReferenceModel.EtherCATIo.RejectedWriteDoesNotMutate",
                RejectedWriteDoesNotMutate);
            tests.Add(
                "ReferenceModel.EtherCATIo.SingleMailboxNoReplay",
                SingleMailboxNoReplay);
        }

        private static void Byte0IsLeastSignificant()
        {
            var bytes = new byte[] { 0x78, 0x56, 0x34, 0x12 };
            var packed = RtReferenceModel.Pack32(bytes);

            AssertEx.Equal(0x12345678u, packed);
            AssertEx.SequenceEqual(bytes, RtReferenceModel.Unpack32(packed));
        }

        private static void NoHardwareNormalizesPresence()
        {
            var native = NativeNode(
                EtherCATStateInit,
                ClassStateNoHardware,
                nativeOnline: false);
            var wire = RtReferenceModel.BuildHealth(native, ClassStateNoHardware);

            AssertEx.False(wire.Online);
            AssertEx.Equal((byte)0, wire.EtherCATState);
            AssertEx.Equal(
                LMCEtherCATNodeHealthFlags.Configured
                    | LMCEtherCATNodeHealthFlags.DataDefaulted,
                wire.Flags);

            var disconnectedSource = RtReferenceModel.BuildHealth(
                NativeNode(
                    EtherCATStateOperational,
                    ClassStateOk,
                    nativeOnline: true),
                ClassStateOk,
                sourceConnected: false);
            AssertEx.False(disconnectedSource.Online);
            AssertEx.Equal((byte)0, disconnectedSource.EtherCATState);
            AssertEx.Equal(uint.MaxValue, disconnectedSource.ClassState);
            AssertEx.Equal(
                LMCEtherCATNodeHealthFlags.Configured
                    | LMCEtherCATNodeHealthFlags.DataDefaulted,
                disconnectedSource.Flags);

            var disconnectedIo = RtReferenceModel.CaptureIo(
                NativeNode(
                    EtherCATStateOperational,
                    ClassStateOk,
                    nativeOnline: true),
                ClassStateOk,
                masterOperational: true,
                missedFrameCounter: 0,
                rawValue: FullMask,
                sourceConnected: false);
            AssertEx.True(
                (disconnectedIo.Status
                    & LMCDigitalIOStatusFlags.SourceUnavailable) != 0);
            AssertEx.True(
                (disconnectedIo.Status
                    & LMCDigitalIOStatusFlags.DataDefaulted) != 0);
            AssertEx.Equal(0u, disconnectedIo.Value);
            AssertEx.Equal(0u, disconnectedIo.ValidMask);
        }

        private static void PreOpAndSafeOpRemainDetected()
        {
            foreach (var state in new[]
            {
                EtherCATStatePreOperational,
                EtherCATStateSafeOperational
            })
            {
                var native = NativeNode(
                    state,
                    classState: 0,
                    nativeOnline: false);
                var wire = RtReferenceModel.BuildHealth(native, 0);

                AssertEx.True(wire.Online);
                AssertEx.Equal(state, wire.EtherCATState);
                AssertEx.True(
                    (wire.Flags & LMCEtherCATNodeHealthFlags.Detected) != 0);
                AssertEx.True(
                    (wire.Flags & LMCEtherCATNodeHealthFlags.DataDefaulted) != 0);
                AssertEx.False(
                    (wire.Flags & LMCEtherCATNodeHealthFlags.DataValid) != 0);
            }
        }

        private static void OperationalSnapshotIsValid()
        {
            var parent = NativeNode(
                EtherCATStateOperational,
                ClassStateOk,
                nativeOnline: true);
            var health = RtReferenceModel.BuildHealth(
                parent,
                ClassStateOk,
                sourceConnected: true,
                masterOperational: true,
                missedFrameCounter: 0);
            var snapshot = RtReferenceModel.CaptureIo(
                parent,
                ClassStateOk,
                masterOperational: true,
                missedFrameCounter: 0,
                rawValue: 0x89ABCDEFu);

            AssertEx.True(
                (health.Flags & LMCEtherCATNodeHealthFlags.DataValid) != 0);
            AssertEx.Equal(LMCDigitalIOStatusFlags.Valid, snapshot.Status);
            AssertEx.Equal(0x89ABCDEFu, snapshot.Value);
            AssertEx.Equal(FullMask, snapshot.ValidMask);
        }

        private static void MissedFrameDefaultsSnapshot()
        {
            var parent = NativeNode(
                EtherCATStateOperational,
                ClassStateOk,
                nativeOnline: true);
            var snapshot = RtReferenceModel.CaptureIo(
                parent,
                ClassStateOk,
                masterOperational: true,
                missedFrameCounter: 1,
                rawValue: 0xFFFFFFFFu);
            var health = RtReferenceModel.BuildHealth(
                parent,
                ClassStateOk,
                sourceConnected: true,
                masterOperational: true,
                missedFrameCounter: 1);

            AssertEx.False(
                (health.Flags & LMCEtherCATNodeHealthFlags.DataValid) != 0);
            AssertEx.True(
                (health.Flags & LMCEtherCATNodeHealthFlags.DataDefaulted) != 0);
            AssertEx.Equal(
                LMCDigitalIOStatusFlags.StaleFrame
                    | LMCDigitalIOStatusFlags.DataDefaulted,
                snapshot.Status);
            AssertEx.Equal(0u, snapshot.Value);
            AssertEx.Equal(0u, snapshot.ValidMask);
        }

        private static void ExactStatusCauseMatrix()
        {
            const uint rawValue = 0xA5A55A5Au;
            var healthyFlags = LMCEtherCATNodeHealthFlags.Configured
                | LMCEtherCATNodeHealthFlags.Detected
                | LMCEtherCATNodeHealthFlags.IdentityMatched
                | LMCEtherCATNodeHealthFlags.DataValid;
            var detectedDefaultedFlags =
                LMCEtherCATNodeHealthFlags.Configured
                | LMCEtherCATNodeHealthFlags.Detected
                | LMCEtherCATNodeHealthFlags.IdentityMatched
                | LMCEtherCATNodeHealthFlags.DataDefaulted;
            var cases = new[]
            {
                new StatusCauseCase
                {
                    Name = "normal baseline",
                    Native = NativeNode(
                        EtherCATStateOperational,
                        ClassStateOk,
                        nativeOnline: true),
                    SlotClassState = ClassStateOk,
                    MasterOperational = true,
                    ExpectedOnline = true,
                    ExpectedEtherCATState = EtherCATStateOperational,
                    ExpectedALStatusCode = 0,
                    ExpectedClassState = ClassStateOk,
                    ExpectedHealthFlags = healthyFlags,
                    ExpectedIoStatus = LMCDigitalIOStatusFlags.Valid,
                    ExpectedValue = rawValue,
                    ExpectedValidMask = FullMask
                },
                new StatusCauseCase
                {
                    Name = "master non-operational",
                    Native = NativeNode(
                        EtherCATStateOperational,
                        ClassStateOk,
                        nativeOnline: true),
                    SlotClassState = ClassStateOk,
                    MasterOperational = false,
                    ExpectedOnline = true,
                    ExpectedEtherCATState = EtherCATStateOperational,
                    ExpectedALStatusCode = 0,
                    ExpectedClassState = ClassStateOk,
                    ExpectedHealthFlags = detectedDefaultedFlags,
                    ExpectedIoStatus =
                        LMCDigitalIOStatusFlags.MasterNotOperational
                        | LMCDigitalIOStatusFlags.DataDefaulted,
                    ExpectedValue = 0,
                    ExpectedValidMask = 0
                },
                new StatusCauseCase
                {
                    Name = "native Online false while master operational",
                    Native = NativeNode(
                        EtherCATStateOperational,
                        ClassStateOk,
                        nativeOnline: false),
                    SlotClassState = ClassStateOk,
                    MasterOperational = true,
                    ExpectedOnline = true,
                    ExpectedEtherCATState = EtherCATStateOperational,
                    ExpectedALStatusCode = 0,
                    ExpectedClassState = ClassStateOk,
                    ExpectedHealthFlags = detectedDefaultedFlags,
                    ExpectedIoStatus =
                        LMCDigitalIOStatusFlags.NodeNotOperational
                        | LMCDigitalIOStatusFlags.DataDefaulted,
                    ExpectedValue = 0,
                    ExpectedValidMask = 0
                },
                new StatusCauseCase
                {
                    Name = "AL error",
                    Native = NativeNode(
                        EtherCATStateOperational,
                        ClassStateOk,
                        nativeOnline: true,
                        alStatusCode: 0x0011),
                    SlotClassState = ClassStateOk,
                    MasterOperational = true,
                    ExpectedOnline = true,
                    ExpectedEtherCATState = EtherCATStateOperational,
                    ExpectedALStatusCode = 0x0011,
                    ExpectedClassState = ClassStateOk,
                    ExpectedHealthFlags = detectedDefaultedFlags,
                    ExpectedIoStatus = LMCDigitalIOStatusFlags.AlError
                        | LMCDigitalIOStatusFlags.DataDefaulted,
                    ExpectedValue = 0,
                    ExpectedValidMask = 0
                },
                new StatusCauseCase
                {
                    Name = "node offline",
                    Native = NativeNode(
                        EtherCATStateOperational,
                        ClassStateOk,
                        nativeOnline: true),
                    SlotClassState = ClassStateNoHardware,
                    MasterOperational = true,
                    ExpectedOnline = false,
                    ExpectedEtherCATState = 0,
                    ExpectedALStatusCode = 0,
                    ExpectedClassState = ClassStateNoHardware,
                    ExpectedHealthFlags =
                        LMCEtherCATNodeHealthFlags.Configured
                        | LMCEtherCATNodeHealthFlags.DataDefaulted,
                    ExpectedIoStatus = LMCDigitalIOStatusFlags.NodeOffline
                        | LMCDigitalIOStatusFlags.IdentityMismatch
                        | LMCDigitalIOStatusFlags.DataDefaulted,
                    ExpectedValue = 0,
                    ExpectedValidMask = 0
                }
            };

            foreach (var testCase in cases)
            {
                var health = RtReferenceModel.BuildHealth(
                    testCase.Native,
                    testCase.SlotClassState,
                    sourceConnected: true,
                    masterOperational: testCase.MasterOperational,
                    missedFrameCounter: 0);
                var snapshot = RtReferenceModel.CaptureIo(
                    testCase.Native,
                    testCase.SlotClassState,
                    testCase.MasterOperational,
                    missedFrameCounter: 0,
                    rawValue: rawValue,
                    sourceConnected: true);
                var messagePrefix = testCase.Name + ": ";

                AssertEx.Equal(
                    testCase.ExpectedOnline,
                    health.Online,
                    messagePrefix + "health Online mismatch.");
                AssertEx.Equal(
                    testCase.ExpectedEtherCATState,
                    health.EtherCATState,
                    messagePrefix + "health EtherCAT state mismatch.");
                AssertEx.Equal(
                    0u,
                    health.SlaveState,
                    messagePrefix + "health SlaveState mismatch.");
                AssertEx.Equal(
                    testCase.ExpectedALStatusCode,
                    health.ALStatusCode,
                    messagePrefix + "health AL status mismatch.");
                AssertEx.Equal(
                    testCase.ExpectedClassState,
                    health.ClassState,
                    messagePrefix + "health ClassState mismatch.");
                AssertEx.Equal(
                    testCase.ExpectedHealthFlags,
                    health.Flags,
                    messagePrefix + "health quality flags mismatch.");
                AssertEx.Equal(
                    testCase.ExpectedIoStatus,
                    snapshot.Status,
                    messagePrefix + "I/O status flags mismatch.");
                AssertEx.Equal(
                    testCase.ExpectedValue,
                    snapshot.Value,
                    messagePrefix + "I/O value mismatch.");
                AssertEx.Equal(
                    testCase.ExpectedValidMask,
                    snapshot.ValidMask,
                    messagePrefix + "I/O valid mask mismatch.");
            }
        }

        private static void SlotUsesParentBusHealth()
        {
            var parent = NativeNode(
                EtherCATStateSafeOperational,
                ClassStateOk,
                nativeOnline: false,
                slaveState: 0x0042,
                alStatusCode: 0x0011);
            var slot = RtReferenceModel.BuildHealth(parent, nodeClassState: 7);

            AssertEx.True(slot.Online);
            AssertEx.Equal(EtherCATStateSafeOperational, slot.EtherCATState);
            AssertEx.Equal(0x0042u, slot.SlaveState);
            AssertEx.Equal(0x0011u, slot.ALStatusCode);
            AssertEx.Equal(7u, slot.ClassState);
            AssertEx.False(
                (slot.Flags & LMCEtherCATNodeHealthFlags.IdentityMatched) != 0);
            AssertEx.True(
                (slot.Flags & LMCEtherCATNodeHealthFlags.DataDefaulted) != 0);

            var missingSlot = RtReferenceModel.BuildHealth(
                NativeNode(
                    EtherCATStateOperational,
                    ClassStateOk,
                    nativeOnline: true),
                nodeClassState: ClassStateNoHardware);
            AssertEx.False(missingSlot.Online);
            AssertEx.Equal((byte)0, missingSlot.EtherCATState);
            AssertEx.Equal(
                LMCEtherCATNodeHealthFlags.Configured
                    | LMCEtherCATNodeHealthFlags.DataDefaulted,
                missingSlot.Flags);

            var identityErrorParent = NativeNode(
                EtherCATStateOperational,
                ClassStateOk,
                nativeOnline: true,
                slaveState: EtherCATStateOperational
                    | SlaveStateIdentityError);
            var identityError = RtReferenceModel.BuildHealth(
                identityErrorParent,
                ClassStateOk);
            AssertEx.False(
                (identityError.Flags
                    & LMCEtherCATNodeHealthFlags.IdentityMatched) != 0);
            AssertEx.True(
                (identityError.Flags
                    & LMCEtherCATNodeHealthFlags.DataDefaulted) != 0);

            var classFaultParent = NativeNode(
                EtherCATStateOperational,
                classState: 7,
                nativeOnline: true);
            var classFaultSlot = RtReferenceModel.BuildHealth(
                classFaultParent,
                ClassStateOk);
            var classFaultIo = RtReferenceModel.CaptureIo(
                classFaultParent,
                ClassStateOk,
                masterOperational: true,
                missedFrameCounter: 0,
                rawValue: FullMask);

            AssertEx.True(
                (classFaultSlot.Flags
                    & LMCEtherCATNodeHealthFlags.Detected) != 0);
            AssertEx.False(
                (classFaultSlot.Flags
                    & LMCEtherCATNodeHealthFlags.IdentityMatched) != 0);
            AssertEx.True(
                (classFaultIo.Status
                    & LMCDigitalIOStatusFlags.IdentityMismatch) != 0);
            AssertEx.True(
                (classFaultIo.Status
                    & LMCDigitalIOStatusFlags.DataDefaulted) != 0);
        }

        private static void SlotRequiresParentPhysicalPresence()
        {
            var staleSlotBehindMissingCoupler = RtReferenceModel.BuildHealth(
                NativeNode(
                    EtherCATStateOperational,
                    ClassStateNoHardware,
                    nativeOnline: true),
                nodeClassState: ClassStateOk);

            AssertEx.False(staleSlotBehindMissingCoupler.Online);
            AssertEx.Equal((byte)0, staleSlotBehindMissingCoupler.EtherCATState);
            AssertEx.False(
                (staleSlotBehindMissingCoupler.Flags
                    & LMCEtherCATNodeHealthFlags.Detected) != 0);
            AssertEx.Equal(
                LMCEtherCATNodeHealthFlags.Configured
                    | LMCEtherCATNodeHealthFlags.DataDefaulted,
                staleSlotBehindMissingCoupler.Flags);

            var staleSlotBehindUnavailableSource = RtReferenceModel.BuildHealth(
                NativeNode(
                    EtherCATStateOperational,
                    ClassStateOk,
                    nativeOnline: true),
                nodeClassState: ClassStateOk,
                sourceConnected: false);

            AssertEx.False(staleSlotBehindUnavailableSource.Online);
            AssertEx.Equal((byte)0, staleSlotBehindUnavailableSource.EtherCATState);
            AssertEx.False(
                (staleSlotBehindUnavailableSource.Flags
                    & LMCEtherCATNodeHealthFlags.Detected) != 0);
            AssertEx.Equal(uint.MaxValue, staleSlotBehindUnavailableSource.ClassState);
        }

        private static void OutputRevisionTransitions()
        {
            var owner = new OutputOwnerReferenceModel();

            AssertEx.Equal(1u, owner.Revision);
            owner.Observe(0, isValid: false);
            AssertEx.Equal(1u, owner.Revision);

            owner.Observe(0x11223344u, isValid: true);
            AssertEx.Equal(2u, owner.Revision);

            owner.Observe(0x55667788u, isValid: true);
            AssertEx.Equal(3u, owner.Revision);

            owner.Observe(0, isValid: false);
            AssertEx.Equal(4u, owner.Revision);

            owner.ForceRevisionForWrapTest(uint.MaxValue);
            owner.Observe(0x55667788u, isValid: true);
            AssertEx.Equal(1u, owner.Revision);
        }

        private static void MaskedWritePreservesBits()
        {
            var owner = ValidOwner(0xA5A55A5Au);
            var expectedRevision = owner.Revision;

            var result = owner.Apply(
                value: 0x00001200u,
                mask: 0x0000FF00u,
                expectedRevision: expectedRevision,
                healthValid: true);

            AssertEx.Equal(OutputApplyResult.Applied, result);
            AssertEx.Equal(0xA5A5125Au, owner.Shadow);
            AssertEx.Equal(expectedRevision + 1, owner.Revision);

            expectedRevision = owner.Revision;
            result = owner.Apply(
                value: owner.Shadow,
                mask: FullMask,
                expectedRevision: expectedRevision,
                healthValid: true);
            AssertEx.Equal(OutputApplyResult.Applied, result);
            AssertEx.Equal(expectedRevision + 1, owner.Revision);
        }

        private static void RejectedWriteDoesNotMutate()
        {
            AssertRejectedWithoutMutation(
                value: 0,
                mask: 0,
                revisionOffset: 0,
                healthValid: true,
                expected: OutputApplyResult.MaskInvalid);
            AssertRejectedWithoutMutation(
                value: 0x00010000u,
                mask: 0x000000FFu,
                revisionOffset: 0,
                healthValid: true,
                expected: OutputApplyResult.MaskInvalid);
            AssertRejectedWithoutMutation(
                value: 0x000000AAu,
                mask: 0x000000FFu,
                revisionOffset: 1,
                healthValid: true,
                expected: OutputApplyResult.RevisionMismatch);
            AssertRejectedWithoutMutation(
                value: 0x000000AAu,
                mask: 0x000000FFu,
                revisionOffset: 0,
                healthValid: false,
                expected: OutputApplyResult.HealthInvalid);
        }

        private static void SingleMailboxNoReplay()
        {
            var owner = ValidOwner(0x01020304u);
            var initialRevision = owner.Revision;

            AssertEx.True(owner.TryQueue(
                token: 1,
                value: 0x000000AAu,
                mask: 0x000000FFu,
                expectedRevision: initialRevision));
            AssertEx.False(owner.TryQueue(
                token: 2,
                value: 0,
                mask: FullMask,
                expectedRevision: initialRevision));
            AssertEx.True(owner.CancelQueued(token: 1));
            AssertEx.False(owner.ConsumeQueued(healthValid: true));
            AssertEx.Equal(0x01020304u, owner.Shadow);
            AssertEx.Equal(initialRevision, owner.Revision);

            AssertEx.True(owner.TryQueue(
                token: 3,
                value: 0x000000BBu,
                mask: 0x000000FFu,
                expectedRevision: initialRevision));
            AssertEx.True(owner.ConsumeQueued(healthValid: true));
            AssertEx.Equal(0x010203BBu, owner.Shadow);
            AssertEx.False(owner.TryQueue(
                token: 4,
                value: 0,
                mask: FullMask,
                expectedRevision: owner.Revision));
            AssertEx.False(owner.ConsumeQueued(healthValid: true));
            AssertEx.Equal(initialRevision + 1, owner.Revision);

            OutputCompletion completion;
            AssertEx.True(owner.TryCopyCompletion(token: 3, out completion));
            AssertEx.Equal(OutputApplyResult.Applied, completion.Result);
            AssertEx.Equal(0x010203BBu, completion.Shadow);
            AssertEx.Equal(initialRevision + 1, completion.Revision);

            AssertEx.True(owner.TryQueue(
                token: 5,
                value: 0x000000CCu,
                mask: 0x000000FFu,
                expectedRevision: initialRevision));
            AssertEx.True(owner.ConsumeQueued(healthValid: true));
            AssertEx.True(owner.TryCopyCompletion(token: 5, out completion));
            AssertEx.Equal(OutputApplyResult.RevisionMismatch, completion.Result);
            AssertEx.Equal(0x010203BBu, owner.Shadow);
            AssertEx.Equal(initialRevision + 1, owner.Revision);
            AssertEx.False(owner.ConsumeQueued(healthValid: true));
        }

        private static NativeNodeSample NativeNode(
            byte etherCATState,
            uint classState,
            bool nativeOnline,
            uint slaveState = 0,
            uint alStatusCode = 0)
        {
            return new NativeNodeSample(
                etherCATState,
                slaveState,
                alStatusCode,
                classState,
                nativeOnline);
        }

        private static OutputOwnerReferenceModel ValidOwner(uint value)
        {
            var owner = new OutputOwnerReferenceModel();
            owner.Observe(value, isValid: true);
            return owner;
        }

        private static void AssertRejectedWithoutMutation(
            uint value,
            uint mask,
            uint revisionOffset,
            bool healthValid,
            OutputApplyResult expected)
        {
            var owner = ValidOwner(0x12345678u);
            var beforeValue = owner.Shadow;
            var beforeRevision = owner.Revision;
            var result = owner.Apply(
                value,
                mask,
                beforeRevision + revisionOffset,
                healthValid);

            AssertEx.Equal(expected, result);
            AssertEx.Equal(beforeValue, owner.Shadow);
            AssertEx.Equal(beforeRevision, owner.Revision);
        }

        private sealed class NativeNodeSample
        {
            internal NativeNodeSample(
                byte etherCATState,
                uint slaveState,
                uint alStatusCode,
                uint classState,
                bool nativeOnline)
            {
                EtherCATState = etherCATState;
                SlaveState = slaveState;
                ALStatusCode = alStatusCode;
                ClassState = classState;
                NativeOnline = nativeOnline;
            }

            internal byte EtherCATState { get; private set; }
            internal uint SlaveState { get; private set; }
            internal uint ALStatusCode { get; private set; }
            internal uint ClassState { get; private set; }
            internal bool NativeOnline { get; private set; }
        }

        private sealed class StatusCauseCase
        {
            internal string Name { get; set; }
            internal NativeNodeSample Native { get; set; }
            internal uint SlotClassState { get; set; }
            internal bool MasterOperational { get; set; }
            internal bool ExpectedOnline { get; set; }
            internal byte ExpectedEtherCATState { get; set; }
            internal uint ExpectedALStatusCode { get; set; }
            internal uint ExpectedClassState { get; set; }
            internal LMCEtherCATNodeHealthFlags ExpectedHealthFlags
            {
                get;
                set;
            }
            internal LMCDigitalIOStatusFlags ExpectedIoStatus { get; set; }
            internal uint ExpectedValue { get; set; }
            internal uint ExpectedValidMask { get; set; }
        }

        private sealed class WireHealth
        {
            internal bool Online { get; set; }
            internal byte EtherCATState { get; set; }
            internal uint SlaveState { get; set; }
            internal uint ALStatusCode { get; set; }
            internal uint ClassState { get; set; }
            internal LMCEtherCATNodeHealthFlags Flags { get; set; }
        }

        private sealed class IoSnapshot
        {
            internal LMCDigitalIOStatusFlags Status { get; set; }
            internal uint Value { get; set; }
            internal uint ValidMask { get; set; }
        }

        private static class RtReferenceModel
        {
            internal static uint Pack32(byte[] bytes)
            {
                if (bytes == null || bytes.Length != 4)
                {
                    throw new ArgumentException("Exactly four bytes are required.");
                }

                return bytes[0]
                    | ((uint)bytes[1] << 8)
                    | ((uint)bytes[2] << 16)
                    | ((uint)bytes[3] << 24);
            }

            internal static byte[] Unpack32(uint value)
            {
                return new[]
                {
                    (byte)value,
                    (byte)(value >> 8),
                    (byte)(value >> 16),
                    (byte)(value >> 24)
                };
            }

            internal static WireHealth BuildHealth(
                NativeNodeSample parent,
                uint nodeClassState,
                bool sourceConnected = true,
                bool masterOperational = true,
                uint missedFrameCounter = 0)
            {
                var parentPhysicallyPresent = sourceConnected
                    && parent.ClassState != ClassStateNoHardware
                    && parent.ClassState != uint.MaxValue
                    && parent.EtherCATState != 0;
                var detected = parentPhysicallyPresent
                    && nodeClassState != ClassStateNoHardware
                    && nodeClassState != uint.MaxValue;
                var identityMatched = detected
                    && parent.ClassState == ClassStateOk
                    && nodeClassState == ClassStateOk
                    && (parent.SlaveState & SlaveStateIdentityError) == 0;
                var dataValid = detected
                    && identityMatched
                    && masterOperational
                    && missedFrameCounter == 0
                    && parent.NativeOnline
                    && parent.EtherCATState == EtherCATStateOperational
                    && parent.ALStatusCode == 0;
                var flags = LMCEtherCATNodeHealthFlags.Configured;

                if (detected)
                {
                    flags |= LMCEtherCATNodeHealthFlags.Detected;
                }
                if (identityMatched)
                {
                    flags |= LMCEtherCATNodeHealthFlags.IdentityMatched;
                }
                flags |= dataValid
                    ? LMCEtherCATNodeHealthFlags.DataValid
                    : LMCEtherCATNodeHealthFlags.DataDefaulted;

                return new WireHealth
                {
                    Online = detected,
                    EtherCATState = detected ? parent.EtherCATState : (byte)0,
                    SlaveState = detected ? parent.SlaveState : 0,
                    ALStatusCode = detected ? parent.ALStatusCode : 0,
                    ClassState = sourceConnected
                        ? nodeClassState
                        : uint.MaxValue,
                    Flags = flags
                };
            }

            internal static IoSnapshot CaptureIo(
                NativeNodeSample parent,
                uint slotClassState,
                bool masterOperational,
                uint missedFrameCounter,
                uint rawValue,
                bool sourceConnected = true)
            {
                var health = BuildHealth(
                    parent,
                    slotClassState,
                    sourceConnected,
                    masterOperational,
                    missedFrameCounter);
                var status = LMCDigitalIOStatusFlags.None;

                if (!sourceConnected)
                {
                    status |= LMCDigitalIOStatusFlags.SourceUnavailable;
                }
                if (!masterOperational)
                {
                    status |= LMCDigitalIOStatusFlags.MasterNotOperational;
                }
                if (missedFrameCounter != 0)
                {
                    status |= LMCDigitalIOStatusFlags.StaleFrame;
                }
                if (!health.Online)
                {
                    status |= LMCDigitalIOStatusFlags.NodeOffline;
                }
                else if (health.EtherCATState != EtherCATStateOperational
                    || !parent.NativeOnline)
                {
                    status |= LMCDigitalIOStatusFlags.NodeNotOperational;
                }
                if (health.ALStatusCode != 0)
                {
                    status |= LMCDigitalIOStatusFlags.AlError;
                }
                if ((health.Flags
                    & LMCEtherCATNodeHealthFlags.IdentityMatched) == 0)
                {
                    status |= LMCDigitalIOStatusFlags.IdentityMismatch;
                }

                if (status == LMCDigitalIOStatusFlags.None)
                {
                    return new IoSnapshot
                    {
                        Status = LMCDigitalIOStatusFlags.Valid,
                        Value = rawValue,
                        ValidMask = FullMask
                    };
                }

                return new IoSnapshot
                {
                    Status = status | LMCDigitalIOStatusFlags.DataDefaulted,
                    Value = 0,
                    ValidMask = 0
                };
            }
        }

        private enum OutputApplyResult
        {
            Applied,
            MaskInvalid,
            RevisionMismatch,
            HealthInvalid
        }

        private sealed class OutputRequest
        {
            internal uint Token { get; set; }
            internal uint Value { get; set; }
            internal uint Mask { get; set; }
            internal uint ExpectedRevision { get; set; }
        }

        private sealed class OutputCompletion
        {
            internal uint Token { get; set; }
            internal OutputApplyResult Result { get; set; }
            internal uint Revision { get; set; }
            internal uint Shadow { get; set; }
        }

        private enum OutputMailboxState
        {
            Idle,
            WritingRequest,
            Ready,
            Running,
            WritingCompletion,
            CompletionReady
        }

        private sealed class OutputOwnerReferenceModel
        {
            private bool hasObservation;
            private bool isValid;
            private OutputRequest queued;
            private OutputCompletion completion;
            private OutputMailboxState mailboxState;

            internal OutputOwnerReferenceModel()
            {
                Revision = 1;
            }

            internal uint Revision { get; private set; }
            internal uint Shadow { get; private set; }

            internal void Observe(uint value, bool isValid)
            {
                if (!hasObservation)
                {
                    hasObservation = true;
                    this.isValid = isValid;
                    Shadow = value;
                    return;
                }

                if (this.isValid != isValid
                    || (isValid && Shadow != value))
                {
                    AdvanceRevision();
                }

                this.isValid = isValid;
                Shadow = value;
            }

            internal OutputApplyResult Apply(
                uint value,
                uint mask,
                uint expectedRevision,
                bool healthValid)
            {
                if (mask == 0 || (value & ~mask) != 0)
                {
                    return OutputApplyResult.MaskInvalid;
                }
                if (expectedRevision != Revision)
                {
                    return OutputApplyResult.RevisionMismatch;
                }
                if (!healthValid || !isValid)
                {
                    return OutputApplyResult.HealthInvalid;
                }

                Shadow = (Shadow & ~mask) | (value & mask);
                AdvanceRevision();
                return OutputApplyResult.Applied;
            }

            internal bool TryQueue(
                uint token,
                uint value,
                uint mask,
                uint expectedRevision)
            {
                if (mailboxState != OutputMailboxState.Idle || token == 0)
                {
                    return false;
                }

                mailboxState = OutputMailboxState.WritingRequest;
                queued = new OutputRequest
                {
                    Token = token,
                    Value = value,
                    Mask = mask,
                    ExpectedRevision = expectedRevision
                };
                mailboxState = OutputMailboxState.Ready;
                return true;
            }

            internal bool CancelQueued(uint token)
            {
                if (mailboxState != OutputMailboxState.Ready
                    || queued == null
                    || queued.Token != token)
                {
                    return false;
                }

                mailboxState = OutputMailboxState.WritingRequest;
                queued = null;
                mailboxState = OutputMailboxState.Idle;
                return true;
            }

            internal bool ConsumeQueued(bool healthValid)
            {
                if (mailboxState != OutputMailboxState.Ready
                    || queued == null)
                {
                    return false;
                }

                mailboxState = OutputMailboxState.Running;
                var request = queued;
                queued = null;
                var result = Apply(
                    request.Value,
                    request.Mask,
                    request.ExpectedRevision,
                    healthValid);
                mailboxState = OutputMailboxState.WritingCompletion;
                completion = new OutputCompletion
                {
                    Token = request.Token,
                    Result = result,
                    Revision = Revision,
                    Shadow = Shadow
                };
                mailboxState = OutputMailboxState.CompletionReady;
                return true;
            }

            internal bool TryCopyCompletion(
                uint token,
                out OutputCompletion result)
            {
                result = null;
                if (mailboxState != OutputMailboxState.CompletionReady
                    || completion == null
                    || completion.Token != token)
                {
                    return false;
                }

                result = completion;
                completion = null;
                mailboxState = OutputMailboxState.Idle;
                return true;
            }

            internal void ForceRevisionForWrapTest(uint value)
            {
                Revision = value;
            }

            private void AdvanceRevision()
            {
                Revision = Revision == uint.MaxValue ? 1 : Revision + 1;
            }
        }
    }
}
