from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
PATH = ROOT / 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs'
text = PATH.read_text(encoding='utf-8')

old_server = '''            var journalDirectory = CreateJournalDirectory();
            var identity = Guid.NewGuid();
            var createdUtc = DateTime.UtcNow;
            using (var journal =
                DiagnosticsMutationJournal.Open(journalDirectory))'''
new_server = '''            var server = new FakeRpcServer(steps.ToArray());
            var journalDirectory = CreateJournalDirectory();
            var identity = Guid.NewGuid();
            var createdUtc = DateTime.UtcNow;
            using (var journal =
                DiagnosticsMutationJournal.Open(journalDirectory))'''
# Scope the replacement to the target method only by checking the unique method marker
marker = 'RecoveredTypedWriteNonAllowlistedAxisForcedAttemptIsZeroWire()'
start = text.index(marker)
end = text.index('        private static void DoubleContractAdvertisedRemainsDormantAndZeroWire()', start)
segment = text[start:end]
if segment.count(old_server) != 1:
    raise RuntimeError('expected one recovered-SDO journal fixture anchor')
segment = segment.replace(old_server, new_server, 1)

old_metadata = '''                        LMCSignalValueType.Int32,
                        4,
                        1000,
                        new byte[] { 0x2A, 0, 0, 0 }));'''
new_metadata = '''                        LMCSignalValueType.Int32,
                        4,
                        1000,
                        "127.0.0.1",
                        server.Port,
                        1u,
                        new byte[] { 0x2A, 0, 0, 0 }));'''
if segment.count(old_metadata) != 1:
    raise RuntimeError('expected one recovered-SDO metadata constructor anchor')
segment = segment.replace(old_metadata, new_metadata, 1)

old_using = '''                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(journalDirectory, server.Port);'''
new_using = '''                using (server)
                {
                    window = CreateWindow(journalDirectory, server.Port);'''
if segment.count(old_using) != 1:
    raise RuntimeError('expected one recovered-SDO fake server using anchor')
segment = segment.replace(old_using, new_using, 1)

text = text[:start] + segment + text[end:]
PATH.write_text(text, encoding='utf-8')
print('SDO-R05-B WPF full-identity recovery fixture applied.')
