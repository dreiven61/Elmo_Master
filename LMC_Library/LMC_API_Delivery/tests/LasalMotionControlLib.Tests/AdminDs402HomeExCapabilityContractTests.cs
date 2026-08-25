using System;
using System.Collections.Generic;
using System.IO;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class AdminDs402HomeExCapabilityContractTests
    {
        private const uint RequestId = 0x4E455848u;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Response.Admin.Ds402HomeEx.CapabilityBit11CatalogV7",
                CapabilityBit11CatalogV7);
            tests.Add(
                "Response.Admin.Ds402HomeEx.CapabilityBit11RequiresCatalogV7",
                CapabilityBit11RequiresCatalogV7);
            tests.Add(
                "Response.Admin.Ds402HomeEx.CapabilityBit11RequiresPhysicalAxisRange",
                CapabilityBit11RequiresPhysicalAxisRange);
            tests.Add(
                "Response.Admin.Ds402HomeEx.UnknownFeatureBit12Rejected",
                UnknownFeatureBit12Rejected);
        }

        private static void CapabilityBit11CatalogV7()
        {
            var parsed = LMC_AdminParser.ParseCapabilities(
                TestFrame.Response(
                    0,
                    CapabilitiesPayload(
                        LMCAdminFeature.AxisDs402HomeEx,
                        4,
                        7)),
                RequestId,
                1);

            AssertEx.True(
                parsed.Supports(LMCAdminFeature.AxisDs402HomeEx));
            AssertEx.Equal((ushort)4, parsed.PhysicalAxisCount);
            AssertEx.Equal((ushort)7, parsed.ErrorCatalogVersion);
        }

        private static void CapabilityBit11RequiresCatalogV7()
        {
            AssertEx.Throws<InvalidDataException>(() =>
                LMC_AdminParser.ParseCapabilities(
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            LMCAdminFeature.AxisDs402HomeEx,
                            4,
                            6)),
                    RequestId,
                    1));
        }

        private static void CapabilityBit11RequiresPhysicalAxisRange()
        {
            AssertEx.Throws<InvalidDataException>(() =>
                LMC_AdminParser.ParseCapabilities(
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            LMCAdminFeature.AxisDs402HomeEx,
                            0,
                            7)),
                    RequestId,
                    1));

            AssertEx.Throws<InvalidDataException>(() =>
                LMC_AdminParser.ParseCapabilities(
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            LMCAdminFeature.AxisDs402HomeEx,
                            5,
                            7)),
                    RequestId,
                    1));
        }

        private static void UnknownFeatureBit12Rejected()
        {
            AssertEx.Throws<InvalidDataException>(() =>
                LMC_AdminParser.ParseCapabilities(
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            (LMCAdminFeature)(1u << 12),
                            4,
                            7)),
                    RequestId,
                    1));
        }

        private static byte[] CapabilitiesPayload(
            LMCAdminFeature feature,
            ushort physicalAxisCount,
            ushort errorCatalogVersion)
        {
            var payload = new byte[40];
            TestFrame.WriteUInt16(payload, 0, LMCAdmin.ProtocolSchemaVersion);
            TestFrame.WriteUInt16(payload, 2, 0);
            TestFrame.WriteUInt16(payload, 4, 0);
            TestFrame.WriteInt16(payload, 6, 0);
            TestFrame.WriteUInt32(payload, 8, RequestId);
            TestFrame.WriteUInt32(payload, 12, 0);
            TestFrame.WriteUInt32(payload, 16, (uint)feature);
            TestFrame.WriteUInt32(payload, 20, 0);
            TestFrame.WriteUInt32(payload, 24, 0);
            TestFrame.WriteUInt16(payload, 28, physicalAxisCount);
            TestFrame.WriteUInt16(payload, 30, 0);
            TestFrame.WriteUInt16(payload, 32, 0);
            TestFrame.WriteUInt16(payload, 34, 0);
            TestFrame.WriteUInt16(payload, 36, errorCatalogVersion);
            TestFrame.WriteUInt16(payload, 38, 0);
            return payload;
        }
    }
}
