using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    internal enum ConfiguredTopologyComparisonKind
    {
        Initial,
        Unchanged,
        Changed
    }

    internal sealed class ConfiguredTopologySnapshot
    {
        private readonly ReadOnlyCollection<string> entryLines;

        private ConfiguredTopologySnapshot(
            string endpoint,
            long sessionGeneration,
            string loadOrigin,
            DateTime capturedUtc,
            string headerLine,
            IList<string> entryLines,
            string canonicalText,
            string sha256,
            uint topologyRevision)
        {
            Endpoint = endpoint;
            SessionGeneration = sessionGeneration;
            LoadOrigin = loadOrigin;
            CapturedUtc = capturedUtc;
            HeaderLine = headerLine;
            this.entryLines = new ReadOnlyCollection<string>(
                new List<string>(entryLines));
            CanonicalText = canonicalText;
            Sha256 = sha256;
            TopologyRevision = topologyRevision;
        }

        internal string Endpoint { get; private set; }
        internal long SessionGeneration { get; private set; }
        internal string LoadOrigin { get; private set; }
        internal DateTime CapturedUtc { get; private set; }
        internal string HeaderLine { get; private set; }
        internal IReadOnlyList<string> EntryLines { get { return entryLines; } }
        internal string CanonicalText { get; private set; }
        internal string Sha256 { get; private set; }
        internal uint TopologyRevision { get; private set; }
        internal int EntryCount { get { return entryLines.Count; } }

        internal static ConfiguredTopologySnapshot Capture(
            LMCEtherCATTopology topology,
            string endpoint,
            long sessionGeneration,
            string loadOrigin,
            DateTime capturedUtc)
        {
            if (topology == null)
            {
                throw new ArgumentNullException("topology");
            }

            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentException(
                    "A normalized PLC endpoint is required.",
                    "endpoint");
            }

            if (sessionGeneration <= 0)
            {
                throw new ArgumentOutOfRangeException("sessionGeneration");
            }

            var normalizedCapturedUtc = capturedUtc.Kind == DateTimeKind.Utc
                ? capturedUtc
                : capturedUtc.ToUniversalTime();
            var info = topology.Info;
            var headerLine = FormatHeader(info);
            var lines = new List<string>(topology.Entries.Count);
            for (var index = 0; index < topology.Entries.Count; index++)
            {
                lines.Add(FormatEntry(index, topology.Entries[index]));
            }

            var canonical = new StringBuilder();
            canonical.Append(headerLine);
            foreach (var line in lines)
            {
                canonical.Append('\n');
                canonical.Append(line);
            }

            var canonicalText = canonical.ToString();
            return new ConfiguredTopologySnapshot(
                endpoint.Trim(),
                sessionGeneration,
                string.IsNullOrWhiteSpace(loadOrigin)
                    ? "unknown"
                    : loadOrigin.Trim(),
                normalizedCapturedUtc,
                headerLine,
                lines,
                canonicalText,
                ComputeSha256(canonicalText),
                topology.TopologyRevision);
        }

        private static string FormatHeader(LMCEtherCATTopologyInfo info)
        {
            return "HEADER"
                + "|TopologyRevision=" + FormatHex32(info.TopologyRevision)
                + "|TotalNodeCount=" + FormatUInt16(info.TotalNodeCount)
                + "|EntryStride=" + FormatUInt16(info.EntryStride)
                + "|MaxEntriesPerChunk="
                + FormatUInt16(info.MaxEntriesPerChunk)
                + "|ConfiguredSlaveCount="
                + FormatUInt16(info.ConfiguredSlaveCount)
                + "|SlotModuleCount=" + FormatUInt16(info.SlotModuleCount)
                + "|PhysicalAxisCount="
                + FormatUInt16(info.PhysicalAxisCount)
                + "|TopologyFlags=" + FormatHex32(info.TopologyFlagsValue)
                + "|CrcKind=" + FormatHex32(info.CrcKindValue);
        }

        private static string FormatEntry(
            int orderIndex,
            LMCEtherCATTopologyEntry entry)
        {
            var name = entry.Name ?? string.Empty;
            return "ENTRY["
                + orderIndex.ToString("D4", CultureInfo.InvariantCulture)
                + "]"
                + "|NodeId=" + FormatHex32(entry.NodeId)
                + "|ParentNodeId=" + FormatHex32(entry.ParentNodeId)
                + "|TopologyIndex=" + FormatUInt16(entry.TopologyIndex)
                + "|MasterSlaveIndex="
                + FormatUInt16(entry.MasterSlaveIndex)
                + "|NodeKind="
                + ((byte)entry.NodeKind).ToString(
                    CultureInfo.InvariantCulture)
                + "|NodeFlags=" + FormatHex16((ushort)entry.NodeFlags)
                + "|SdoSlaveReference="
                + FormatUInt16(entry.SdoSlaveReference)
                + "|PhysicalAxisReference="
                + FormatUInt16(entry.PhysicalAxisReference)
                + "|SlotIndex=" + FormatUInt16(entry.SlotIndex)
                + "|VendorId=" + FormatHex32(entry.VendorId)
                + "|ProductCode=" + FormatHex32(entry.ProductCode)
                + "|RevisionNumber=" + FormatHex32(entry.RevisionNumber)
                + "|SerialNumber=" + FormatHex32(entry.SerialNumber)
                + "|InputBytes=" + FormatUInt16(entry.InputBytes)
                + "|OutputBytes=" + FormatUInt16(entry.OutputBytes)
                + "|NameLength="
                + name.Length.ToString(CultureInfo.InvariantCulture)
                + "|Name=" + Escape(name)
                + "|IOReference=" + FormatHex32(entry.IOReference);
        }

        private static string Escape(string value)
        {
            return value
                .Replace("\\", "\\\\")
                .Replace("|", "\\|")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");
        }

        private static string ComputeSha256(string value)
        {
            byte[] hash;
            using (var algorithm = SHA256.Create())
            {
                hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(value));
            }

            var result = new StringBuilder(hash.Length * 2);
            foreach (var octet in hash)
            {
                result.Append(octet.ToString("X2", CultureInfo.InvariantCulture));
            }

            return result.ToString();
        }

        private static string FormatUInt16(ushort value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static string FormatHex16(ushort value)
        {
            return "0x" + value.ToString("X4", CultureInfo.InvariantCulture);
        }

        private static string FormatHex32(uint value)
        {
            return "0x" + value.ToString("X8", CultureInfo.InvariantCulture);
        }
    }

    internal sealed class ConfiguredTopologyComparison
    {
        private const string BoundaryLine =
            "BOUNDARY=CONFIGURED SCHEMA ONLY (capability bit 14; commands 0x7E11/0x7E12)";
        private const string NotProofLine =
            "NOT PROOF=runtime EtherCAT discovery, physical cable order, live Online/AL/DS402, DI, or physical DO feedback";

        private readonly ReadOnlyCollection<string> differences;

        private ConfiguredTopologyComparison(
            ConfiguredTopologyComparisonKind kind,
            string reason,
            ConfiguredTopologySnapshot previous,
            ConfiguredTopologySnapshot current,
            IList<string> differences)
        {
            Kind = kind;
            Reason = reason;
            Previous = previous;
            Current = current;
            this.differences = new ReadOnlyCollection<string>(
                new List<string>(differences));
        }

        internal ConfiguredTopologyComparisonKind Kind { get; private set; }
        internal string Reason { get; private set; }
        internal ConfiguredTopologySnapshot Previous { get; private set; }
        internal ConfiguredTopologySnapshot Current { get; private set; }
        internal IReadOnlyList<string> Differences { get { return differences; } }

        internal static ConfiguredTopologyComparison Compare(
            ConfiguredTopologySnapshot previous,
            ConfiguredTopologySnapshot current)
        {
            if (current == null)
            {
                throw new ArgumentNullException("current");
            }

            if (previous == null)
            {
                return new ConfiguredTopologyComparison(
                    ConfiguredTopologyComparisonKind.Initial,
                    "No earlier successful snapshot exists in this WPF process.",
                    null,
                    current,
                    new[] { "INITIAL BASELINE; no prior same-endpoint snapshot." });
            }

            if (!string.Equals(
                    previous.Endpoint,
                    current.Endpoint,
                    StringComparison.OrdinalIgnoreCase))
            {
                return new ConfiguredTopologyComparison(
                    ConfiguredTopologyComparisonKind.Initial,
                    "The PLC endpoint changed; the comparison baseline was reset.",
                    previous,
                    current,
                    new[]
                    {
                        "ENDPOINT CHANGED: "
                            + previous.Endpoint
                            + " -> "
                            + current.Endpoint
                    });
            }

            if (string.Equals(
                    previous.CanonicalText,
                    current.CanonicalText,
                    StringComparison.Ordinal))
            {
                return new ConfiguredTopologyComparison(
                    ConfiguredTopologyComparisonKind.Unchanged,
                    "The complete ordered configured schema is identical.",
                    previous,
                    current,
                    new[]
                    {
                        "UNCHANGED CONFIGURED SCHEMA; physical bus order was not verified."
                    });
            }

            var differences = BuildDifferences(previous, current);
            return new ConfiguredTopologyComparison(
                ConfiguredTopologyComparisonKind.Changed,
                "The complete ordered configured schema changed.",
                previous,
                current,
                differences);
        }

        internal string BuildDisplayText()
        {
            var text = new StringBuilder();
            text.Append("Configured comparison=");
            text.Append(Kind.ToString().ToUpperInvariant());
            text.Append("; ");
            text.Append(Reason);
            text.AppendLine();
            text.Append("Current Endpoint=");
            text.Append(Current.Endpoint);
            text.Append(", Revision=");
            text.Append(FormatHex32(Current.TopologyRevision));
            text.Append(", Nodes=");
            text.Append(Current.EntryCount.ToString(CultureInfo.InvariantCulture));
            text.Append(", SHA256=");
            text.AppendLine(Current.Sha256);
            if (Previous != null)
            {
                text.Append("Previous Endpoint=");
                text.Append(Previous.Endpoint);
                text.Append(", Revision=");
                text.Append(FormatHex32(Previous.TopologyRevision));
                text.Append(", Nodes=");
                text.Append(Previous.EntryCount.ToString(
                    CultureInfo.InvariantCulture));
                text.Append(", SHA256=");
                text.AppendLine(Previous.Sha256);
            }

            foreach (var difference in differences)
            {
                text.AppendLine(difference);
            }

            text.AppendLine(BoundaryLine);
            text.Append(NotProofLine);
            return text.ToString();
        }

        internal string BuildEvidenceText(string capabilityEvidence)
        {
            var text = new StringBuilder();
            text.AppendLine("ELMO WPF CONFIGURED ETHERCAT TOPOLOGY EVIDENCE");
            text.AppendLine(BoundaryLine);
            text.AppendLine(NotProofLine);
            text.AppendLine(
                "EVIDENCE_STATE=last successful configured-topology response retained in this WPF process");
            text.Append("CapturedUtc=");
            text.AppendLine(Current.CapturedUtc.ToString(
                "O",
                CultureInfo.InvariantCulture));
            text.Append("Endpoint=");
            text.AppendLine(Current.Endpoint);
            text.Append("SessionGeneration=");
            text.AppendLine(Current.SessionGeneration.ToString(
                CultureInfo.InvariantCulture));
            text.Append("LoadOrigin=");
            text.AppendLine(Current.LoadOrigin);
            text.Append("CapabilityEvidence=");
            text.AppendLine(string.IsNullOrWhiteSpace(capabilityEvidence)
                ? "unavailable"
                : capabilityEvidence.Trim());
            text.Append("Comparison=");
            text.AppendLine(Kind.ToString().ToUpperInvariant());
            text.Append("ComparisonReason=");
            text.AppendLine(Reason);
            AppendSnapshotSummary(text, "Current", Current);
            if (Previous != null)
            {
                AppendSnapshotSummary(text, "Previous", Previous);
            }

            text.AppendLine();
            text.AppendLine("[ORDERED DIFF]");
            foreach (var difference in differences)
            {
                text.AppendLine(difference);
            }

            text.AppendLine();
            text.AppendLine("[CURRENT TOPOLOGY HEADER]");
            text.AppendLine(Current.HeaderLine);
            text.AppendLine();
            text.AppendLine("[CURRENT ORDERED CONFIGURED ENTRIES]");
            foreach (var entryLine in Current.EntryLines)
            {
                text.AppendLine(entryLine);
            }

            return text.ToString();
        }

        internal static void SaveEvidence(string path, string evidenceText)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("An evidence path is required.", "path");
            }

            if (string.IsNullOrWhiteSpace(evidenceText))
            {
                throw new ArgumentException(
                    "Configured-topology evidence is empty.",
                    "evidenceText");
            }

            File.WriteAllText(path, evidenceText, new UTF8Encoding(false));
        }

        private static IList<string> BuildDifferences(
            ConfiguredTopologySnapshot previous,
            ConfiguredTopologySnapshot current)
        {
            var result = new List<string>();
            if (!string.Equals(
                    previous.HeaderLine,
                    current.HeaderLine,
                    StringComparison.Ordinal))
            {
                result.Add("MODIFIED HEADER");
                result.Add("- " + previous.HeaderLine);
                result.Add("+ " + current.HeaderLine);
            }

            var maximumCount = Math.Max(
                previous.EntryLines.Count,
                current.EntryLines.Count);
            for (var index = 0; index < maximumCount; index++)
            {
                if (index >= previous.EntryLines.Count)
                {
                    result.Add("ADDED ORDER["
                        + index.ToString("D4", CultureInfo.InvariantCulture)
                        + "] "
                        + current.EntryLines[index]);
                    continue;
                }

                if (index >= current.EntryLines.Count)
                {
                    result.Add("REMOVED ORDER["
                        + index.ToString("D4", CultureInfo.InvariantCulture)
                        + "] "
                        + previous.EntryLines[index]);
                    continue;
                }

                if (!string.Equals(
                        previous.EntryLines[index],
                        current.EntryLines[index],
                        StringComparison.Ordinal))
                {
                    result.Add("MODIFIED ORDER["
                        + index.ToString("D4", CultureInfo.InvariantCulture)
                        + "]");
                    result.Add("- " + previous.EntryLines[index]);
                    result.Add("+ " + current.EntryLines[index]);
                }
            }

            if (result.Count == 0)
            {
                result.Add(
                    "CHANGED was detected by canonical equality, but no printable ordered difference was produced.");
            }

            return result;
        }

        private static void AppendSnapshotSummary(
            StringBuilder text,
            string prefix,
            ConfiguredTopologySnapshot snapshot)
        {
            text.Append(prefix);
            text.Append("Endpoint=");
            text.AppendLine(snapshot.Endpoint);
            text.Append(prefix);
            text.Append("Revision=");
            text.AppendLine(FormatHex32(snapshot.TopologyRevision));
            text.Append(prefix);
            text.Append("NodeCount=");
            text.AppendLine(snapshot.EntryCount.ToString(
                CultureInfo.InvariantCulture));
            text.Append(prefix);
            text.Append("SHA256=");
            text.AppendLine(snapshot.Sha256);
        }

        private static string FormatHex32(uint value)
        {
            return "0x" + value.ToString("X8", CultureInfo.InvariantCulture);
        }
    }
}
