using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace LasalMotionControlApiExample
{
    internal enum UiLanguage
    {
        English = 0,
        Korean = 1
    }

    internal sealed class UiLanguageOption
    {
        internal UiLanguageOption(UiLanguage language, string displayName)
        {
            Language = language;
            DisplayName = displayName;
        }

        internal UiLanguage Language { get; private set; }
        public string DisplayName { get; private set; }

        internal static UiLanguageOption[] CreateDefaultOptions()
        {
            return new[]
            {
                new UiLanguageOption(UiLanguage.English, "English"),
                new UiLanguageOption(UiLanguage.Korean, "한국어")
            };
        }
    }

    internal static class UiLanguagePreferenceStore
    {
        private const string KoreanToken = "ko-KR";
        private const string EnglishToken = "en-US";

        internal static string GetDefaultFilePath()
        {
            var localApplicationData = Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrWhiteSpace(localApplicationData))
            {
                throw new InvalidOperationException(
                    "Windows LocalApplicationData is unavailable.");
            }

            return Path.Combine(
                localApplicationData,
                "Elmo",
                "LasalMotionControlApiExample",
                "ui-language.txt");
        }

        internal static UiLanguage Load(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath)
                    || !File.Exists(filePath))
                {
                    return UiLanguage.English;
                }

                var value = File.ReadAllText(filePath, Encoding.UTF8).Trim();
                return string.Equals(
                        value,
                        KoreanToken,
                        StringComparison.OrdinalIgnoreCase)
                    || string.Equals(
                        value,
                        "Korean",
                        StringComparison.OrdinalIgnoreCase)
                    ? UiLanguage.Korean
                    : UiLanguage.English;
            }
            catch
            {
                return UiLanguage.English;
            }
        }

        internal static void Save(string filePath, UiLanguage language)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(
                    "A UI language preference path is required.",
                    "filePath");
            }

            var fullPath = Path.GetFullPath(filePath);
            var directoryPath = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new InvalidOperationException(
                    "The UI language preference directory is unavailable.");
            }

            Directory.CreateDirectory(directoryPath);
            var temporaryPath = fullPath + ".tmp";
            try
            {
                File.WriteAllText(
                    temporaryPath,
                    language == UiLanguage.Korean
                        ? KoreanToken
                        : EnglishToken,
                    new UTF8Encoding(false));
                if (File.Exists(fullPath))
                {
                    File.Replace(temporaryPath, fullPath, null);
                }
                else
                {
                    File.Move(temporaryPath, fullPath);
                }
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
        }
    }

    internal static class UiLocalizationCatalog
    {
        private static readonly Dictionary<string, string>
            KoreanTranslations = CreateKoreanTranslations();

        private static readonly KeyValuePair<string, string>[]
            KoreanPrefixes =
            {
                Pair(
                    "LASAL Motion Control API Example",
                    "LASAL 모션 제어 API 예제"),
                Pair(
                    "SAFETY: RECOVERY IDENTITY READ-ONLY QUARANTINE. ",
                    "안전: 복구 ID 읽기 전용 격리. "),
                Pair("SAFETY: ", "안전: "),
                Pair("RECOVERY: ", "복구: "),
                Pair("Listening ", "수신 대기 "),
                Pair("Stopped, rejected=", "중지됨, 거부="),
                Pair("Estimate: ", "예상: "),
                Pair("Preparation: ", "준비: "),
                Pair("Resume Power On Verification", "Power On 확인 계속"),
                Pair("Resume Power Off Verification", "Power Off 확인 계속"),
                Pair("Resume Reset Verification", "Reset 확인 계속"),
                Pair("Resume Stop Verification", "Stop 확인 계속"),
                Pair("Resume Lock Verification", "Lock 확인 계속"),
                Pair("Resume Unlock Verification", "Unlock 확인 계속")
            };

        private static readonly KeyValuePair<string, string>[]
            KoreanSegments =
            {
                Pair(
                    "Group Reset may have been sent, but there is no accepted status-only continuation. Fresh 0x2049, reconnect, mutation, and Close are blocked. Use Group Stop, Power Off, safe Disable, or disconnect. Readiness is invalid.",
                    "그룹 Reset이 전송됐을 수 있지만 승인된 상태 읽기 전용 continuation이 없습니다. 새 0x2049, 재연결, mutation, Close는 차단됩니다. Group Stop, Power Off, 안전 Disable 또는 연결 해제를 사용하십시오. 준비 상태는 무효입니다."),
                Pair(
                    "outcome-uncertain Group Reset recovery is attached; current group/member stable error-clearance proof is pending. The prior 0x2049 outcome remains unknown and will not be replayed. Power, identity/Home, and profile-lock readiness are invalid.",
                    "결과 미확정 그룹 Reset 복구가 연결됐으며 현재 그룹/member의 안정적인 오류 해제 증명을 기다리고 있습니다. 이전 0x2049 결과는 계속 미확정이며 재전송하지 않습니다. Power, identity/Home, profile-lock 준비 상태는 무효입니다."),
                Pair(
                    "Group Reset ACK accepted; stable group/member error-clearance proof is pending",
                    "그룹 Reset ACK가 승인됐으며 안정적인 그룹/member 오류 해제 증명을 기다리는 중"),
                Pair(
                    ". Power, identity/Home, and profile-lock readiness are invalid. Next: Resume Reset Verification (status reads only; no 0x2049 replay), or use Stop, Power Off, or safe Disable.",
                    ". Power, identity/Home, profile-lock 준비 상태는 무효입니다. 다음: Reset 확인 계속(상태 읽기만, 0x2049 재전송 없음) 또는 Stop, Power Off, 안전 Disable을 사용하십시오."),
                Pair(
                    "Power Off proof interfered, explicit replacement allowed",
                    "Power Off 증거에 간섭 발생, 명시적 대체 전송 허용"),
                Pair(
                    "Power Off outcome uncertain, status-only proof required",
                    "Power Off 결과 미확정, 상태 읽기 전용 증명 필요"),
                Pair(
                    "Power Off accepted/start only, Power Off pending",
                    "Power Off 승인/시작만 확인, Power Off 완료 대기"),
                Pair(
                    "Power On outcome uncertain, replay blocked",
                    "Power On 결과 미확정, 재전송 차단"),
                Pair(
                    "Power On accepted/start only, Power Ready pending",
                    "Power On 승인/시작만 확인, Power Ready 대기"),
                Pair(
                    "Read Status failed, Power/Lock state unknown",
                    "상태 읽기 실패, Power/Lock 상태 미확정"),
                Pair(
                    "Power Ready/ACTIVE verified",
                    "Power Ready/ACTIVE 확인됨"),
                Pair("Power On required", "Power On 필요"),
                Pair("identity configured", "identity 설정됨"),
                Pair("identity not configured", "identity 설정 안 됨"),
                Pair("identity axes referenced", "identity 축 reference 완료"),
                Pair("identity axis Home required", "identity 축 Home 필요"),
                Pair("identity Home not checked", "identity Home 미확인"),
                Pair(
                    "profile unlock accepted, Disabled proof pending",
                    "profile unlock 승인, Disabled 증명 대기"),
                Pair(
                    "profile locked/standby verified",
                    "profile locked/standby 확인됨"),
                Pair(
                    "profile lock result stale, Disable or stable Power Off required",
                    "profile lock 결과가 오래됨, Disable 또는 안정적인 Power Off 필요"),
                Pair(
                    "profile lock accepted, Lock Ready pending",
                    "profile lock 승인, Lock Ready 대기"),
                Pair("profile unlocked", "profile unlock 상태"),
                Pair(
                    "Next: Send Power Off Safety Takeover; do not replay 0x204A.",
                    "다음: 안전 인수 Power Off를 전송하십시오. 0x204A를 재전송하지 마십시오."),
                Pair(
                    "Next: Power Off Again is allowed after confirmed interference.",
                    "다음: 간섭이 확인됐으므로 Power Off 재전송이 허용됩니다."),
                Pair(
                    "Next: Resume Power Off Verification (status reads only; no 0x204B replay).",
                    "다음: Power Off 확인을 계속하십시오(상태 읽기만, 0x204B 재전송 없음)."),
                Pair(
                    "Next: Read Status to refresh the group state.",
                    "다음: 상태 읽기로 그룹 상태를 새로고침하십시오."),
                Pair(
                    "Next: Resume Unlock Verification (status reads only; no 0x2048 replay).",
                    "다음: Unlock 확인을 계속하십시오(상태 읽기만, 0x2048 재전송 없음)."),
                Pair(
                    "Next: Resume Power On Verification (status reads only; no 0x204A replay).",
                    "다음: Power On 확인을 계속하십시오(상태 읽기만, 0x204A 재전송 없음)."),
                Pair("Next: Power On.", "다음: Power On."),
                Pair(
                    "Next: Home the failed axes, then Set Identity.",
                    "다음: 실패한 축을 Home한 뒤 Identity를 설정하십시오."),
                Pair(
                    "Next: Set Identity (automatic Home Check).",
                    "다음: Identity를 설정하십시오(자동 Home 확인)."),
                Pair(
                    "Next: Disable or complete stable Power Off verification; do not replay Enable.",
                    "다음: Disable을 수행하거나 안정적인 Power Off 확인을 완료하십시오. Enable을 재전송하지 마십시오."),
                Pair(
                    "Next: Resume Lock Verification (status reads only; no Enable replay).",
                    "다음: Lock 확인을 계속하십시오(상태 읽기만, Enable 재전송 없음)."),
                Pair(
                    "Next: Enable (Lock Profile).",
                    "다음: Enable(Profile Lock)을 실행하십시오."),
                Pair(
                    "Ready: Move Linear or Disable (Unlock Profile).",
                    "준비됨: Move Linear 또는 Disable(Profile Unlock)을 실행할 수 있습니다."),
                Pair(
                    "Group Reset is unresolved. ",
                    "그룹 Reset이 해결되지 않았습니다. "),
                Pair(
                    "diagnostics mutation or durable recovery evidence is unresolved. ",
                    "진단 mutation 또는 durable 복구 증거가 해결되지 않았습니다. "),
                Pair(
                    "Durable evidence remains: ",
                    "Durable 증거가 남아 있습니다: "),
                Pair(
                    " Physically verify the target, then use Persisted Mutation Recovery acknowledgement. No command will be replayed.",
                    " Target을 물리적으로 확인한 뒤 저장된 Mutation 복구 승인을 사용하십시오. 명령은 재전송하지 않습니다."),
                Pair(
                    " The exact SDO readback cannot run in the current connection session. Physically verify the target and PLC state, then use Persisted Mutation Recovery acknowledgement. No command will be replayed.",
                    " 현재 연결 session에서는 정확한 SDO readback을 실행할 수 없습니다. Target과 PLC 상태를 물리적으로 확인한 뒤 저장된 Mutation 복구 승인을 사용하십시오. 명령은 재전송하지 않습니다."),
                Pair(
                    " Complete the current ticket/readback workflow. No command will be replayed.",
                    " 현재 ticket/readback 절차를 완료하십시오. 명령은 재전송하지 않습니다."),
                Pair(
                    "New Move commands are disabled because the durable motion journal is unavailable: ",
                    "Durable motion journal을 사용할 수 없어 새 Move 명령이 차단됩니다: "),
                Pair(
                    "SDO Write transport completed but exact manual readback is pending for ",
                    "SDO Write 전송은 완료됐지만 다음 target의 정확한 수동 readback을 기다리고 있습니다: "),
                Pair(
                    ". Only that exact SDO Read under the original BootId/MapRevision, Stop, PowerOff, and existing-resource cleanup are allowed; mutation and Close remain blocked.",
                    ". 원래 BootId/MapRevision에서 수행하는 해당 exact SDO Read, Stop, PowerOff 및 기존 resource 정리만 허용됩니다. Mutation과 Close는 계속 차단됩니다."),
                Pair(
                    " The digital output write also requires a terminal ticket plus exact shadow reread, or physical verification and explicit acknowledgement.",
                    " Digital output write도 terminal ticket과 정확한 shadow 재읽기 또는 물리 확인 및 명시적 승인이 필요합니다."),
                Pair(
                    "Durable Double-bank recovery evidence remains: ",
                    "Durable Double-bank 복구 증거가 남아 있습니다: "),
                Pair(
                    " It was recovered at startup.",
                    " 시작 시 복구됐습니다."),
                Pair(
                    " Automatic Configure, Start, inventory, adoption, and Release replay are disabled. Exact recovery remains behind the closed ReconnectRecovery proof gate.",
                    " Configure, Start, inventory, 인계 및 Release 자동 재전송은 차단됩니다. 정확한 복구는 닫힌 ReconnectRecovery 증명 gate 뒤에 유지됩니다."),
                Pair(
                    " Active durable evidence remains, so connection/window close stays blocked until that evidence is resolved.",
                    " 활성 durable 증거가 남아 있으므로 해당 증거를 해결할 때까지 연결/창 닫기는 차단됩니다."),
                Pair(
                    " No active durable evidence remains, so normal connection/window exit is available.",
                    " 활성 durable 증거가 없으므로 연결/창을 정상적으로 닫을 수 있습니다."),
                Pair(
                    "Same-session cleanup is blocked: ",
                    "같은 session 정리가 차단됐습니다: "),
                Pair(
                    "Same-session cleanup is unsafe: ",
                    "같은 session 정리는 안전하지 않습니다: "),
                Pair(
                    "Waiting checkpoint: ",
                    "대기 중인 checkpoint: "),
                Pair(
                    "SDO Write evidence is quarantined. Resolve D5 Quarantine cannot clear it with the current Read recovery proof; the quarantine must remain active. Stop, PowerOff, and existing-resource cleanup remain available.",
                    "SDO Write 증거가 격리됐습니다. 현재 Read 복구 증거로는 D5 격리 해결이 이를 해제할 수 없으므로 격리를 유지해야 합니다. Stop, PowerOff 및 기존 resource 정리는 계속 사용할 수 있습니다."),
                Pair(
                    "The SDO Write readback belongs to a different or stale LMCConnection session, so this session cannot submit the exact readback. Mutation and Close remain blocked until the physical target and PLC state are independently verified and Persisted Mutation Recovery is explicitly acknowledged. No command will be replayed.",
                    "SDO Write readback이 다른 session 또는 오래된 LMCConnection session에 속하므로 현재 session에서는 정확한 readback을 전송할 수 없습니다. 물리 target과 PLC 상태를 독립적으로 확인하고 저장된 Mutation 복구를 명시적으로 승인할 때까지 Mutation과 Close는 차단됩니다. 명령은 재전송하지 않습니다."),
                Pair(
                    "Use Resolve D5 Quarantine; Stop, PowerOff, and existing-resource cleanup remain available.",
                    "D5 격리 해결을 사용하십시오. Stop, PowerOff 및 기존 resource 정리는 계속 사용할 수 있습니다."),
                Pair(
                    "Refresh or cancel the digital output ticket. A successful terminal must be followed by an exact output-shadow reread. If the session was lost, physically verify the output before using Acknowledge Unverified Outcome; never replay automatically.",
                    "Digital output ticket을 새로고침하거나 취소하십시오. 성공 terminal 뒤에는 정확한 output-shadow 재읽기가 필요합니다. Session이 끊겼다면 미확인 결과 승인을 사용하기 전에 물리 output을 확인하십시오. 자동 재전송하지 마십시오."),
                Pair(
                    "The durable mutation journal is unavailable. New live/mutation commands and tracked D5 reads are disabled; ordinary non-D5 read-only inspection, Stop, PowerOff, and Group Stop remain available.",
                    "Durable mutation journal을 사용할 수 없습니다. 새 live/mutation 명령과 추적 D5 read는 차단되며 일반 non-D5 읽기 전용 확인, Stop, PowerOff, Group Stop은 계속 사용할 수 있습니다."),
                Pair(
                    "The Double-bank recovery journal is unavailable. New live/mutation commands are disabled; ordinary non-D5 read-only inspection, Stop, PowerOff, and Group Stop remain available. No Double-bank recovery command will be replayed. ",
                    "Double-bank 복구 journal을 사용할 수 없습니다. 새 live/mutation 명령은 차단되며 일반 non-D5 읽기 전용 확인, Stop, PowerOff, Group Stop은 계속 사용할 수 있습니다. Double-bank 복구 명령은 재전송하지 않습니다. "),
                Pair(
                    "The stored recovery identity does not match the current PLC. ",
                    "저장된 복구 ID가 현재 PLC와 일치하지 않습니다. "),
                Pair(
                    "Only ordinary non-D5 read-only inspection, local draft editing, and Close/Exit are allowed. ",
                    "일반 non-D5 읽기 전용 확인, 로컬 초안 편집, Close/Exit만 허용됩니다. "),
                Pair(
                    "Do not infer the old command result from the current PLC state; the durable recovery record remains unchanged. ",
                    "현재 PLC 상태로 이전 명령 결과를 추정하지 마십시오. durable 복구 레코드는 변경되지 않습니다. "),
                Pair(
                    "is blocked because DiagnosticsBootId or MapRevision does not match the durable ",
                    "은(는) DiagnosticsBootId 또는 MapRevision이 durable 복구 레코드와 달라 차단됐습니다: "),
                Pair(
                    "may still be active on ",
                    "이(가) 아직 동작 중일 수 있습니다: "),
                Pair(
                    ". Use Stop or PowerOff and verify standstill.",
                    ". Stop 또는 PowerOff 후 정지를 확인하십시오."),
                Pair(
                    ". Use Group Stop and verify InPosition.",
                    ". Group Stop 후 InPosition을 확인하십시오."),
                Pair(
                    "New motion/diagnostic mutation and Close are blocked. ",
                    "새 motion/diagnostic mutation과 Close가 차단됩니다. ")
            };

        internal static int KoreanTranslationCount
        {
            get { return KoreanTranslations.Count; }
        }

        internal static bool HasKoreanTranslation(string source)
        {
            return source != null && KoreanTranslations.ContainsKey(source);
        }

        internal static string Translate(string source, UiLanguage language)
        {
            if (language != UiLanguage.Korean || string.IsNullOrEmpty(source))
            {
                return source;
            }

            string exact;
            if (KoreanTranslations.TryGetValue(source, out exact))
            {
                return exact;
            }

            for (var index = 0; index < KoreanPrefixes.Length; index++)
            {
                var prefix = KoreanPrefixes[index];
                if (source.StartsWith(prefix.Key, StringComparison.Ordinal))
                {
                    return prefix.Value
                        + TranslateBody(
                            source.Substring(prefix.Key.Length));
                }
            }

            return TranslateKnownSegments(source);
        }

        private static string TranslateBody(string source)
        {
            string exact;
            return KoreanTranslations.TryGetValue(source, out exact)
                ? exact
                : TranslateKnownSegments(source);
        }

        private static string TranslateKnownSegments(string source)
        {
            var translated = source;
            for (var index = 0; index < KoreanSegments.Length; index++)
            {
                translated = translated.Replace(
                    KoreanSegments[index].Key,
                    KoreanSegments[index].Value);
            }
            return translated;
        }

        private static KeyValuePair<string, string> Pair(
            string english,
            string korean)
        {
            return new KeyValuePair<string, string>(english, korean);
        }

        private static Dictionary<string, string> CreateKoreanTranslations()
        {
            var values = new Dictionary<string, string>(StringComparer.Ordinal);

            values["Language"] = "언어";
            values["LASAL Motion Control API Example"] =
                "LASAL 모션 제어 API 예제";
            values["Motion controls remain PLC-active. Diagnostics controls are enabled only after the PLC advertises the required capability. Values use the selected application UNIT or pass through as raw DINT."] =
                "모션 제어는 PLC에서 활성 상태입니다. 진단 제어는 PLC가 필요한 기능을 광고한 뒤에만 활성화됩니다. 값은 선택한 application UNIT을 사용하거나 raw DINT로 전달됩니다.";
            values["Connection / RPC callback"] = "연결 / RPC 콜백";
            values["PLC IP"] = "PLC IP";
            values["TCP port"] = "TCP 포트";
            values["PC local IPv4"] = "PC 로컬 IPv4";
            values["Callback UDP port"] = "콜백 UDP 포트";
            values["Connect"] = "연결";
            values["Close"] = "닫기";
            values["Connection state"] = "연결 상태";
            values["Callback listener"] = "콜백 수신기";
            values["Disconnected"] = "연결 끊김";
            values["Connected"] = "연결됨";
            values["Stopped"] = "중지됨";
            values["RPC initialization / callback evidence"] =
                "RPC 초기화 / 콜백 증거";
            values["Safety / recovery details"] =
                "안전 / 복구 상세 정보";
            values["RPC initialization evidence"] = "RPC 초기화 증거";
            values["Callback v2 registration"] = "콜백 v2 등록";
            values["PC callback receiver evidence"] = "PC 콜백 수신 증거";
            values["Connect performs RPC session initialization and callback registration automatically. Callback payloads are logged as raw diagnostic data."] =
                "연결 시 RPC 세션 초기화와 콜백 등록을 자동으로 수행합니다. 콜백 payload는 raw 진단 데이터로 로그에 기록됩니다.";
            values["Connect performs RPC session initialization and callback registration automatically. UDP wake hints are non-authoritative; operation state changes only after the matching TCP status response."] =
                "연결 시 RPC 세션 초기화와 콜백 등록을 자동으로 수행합니다. UDP wake hint는 상태 판단 근거가 아니며, 동작 상태는 일치하는 TCP status 응답을 받은 뒤에만 변경됩니다.";
            values["Power On, Reset, motion, and group preparation commands are sent immediately without a confirmation dialog. Stop and axis Power Off remain available while connected. Closing the connection does not stop motion."] =
                "Power On, Reset, motion, group 준비 명령은 확인 창 없이 즉시 전송됩니다. 연결 중에는 Stop과 축 Power Off를 사용할 수 있습니다. 연결을 닫아도 motion은 정지하지 않습니다.";
            values["Stop, PowerOff, and Group Stop remain available while connected. Closing the connection does not stop motion."] =
                "연결 중에는 Stop, PowerOff, Group Stop을 사용할 수 있습니다. 연결을 닫아도 motion은 정지하지 않습니다.";
            values["Archive and retire stale recovery evidence"] =
                "오래된 복구 증거 보관 및 폐기";
            values["No stale recovery snapshot is available."] =
                "오래된 복구 snapshot이 없습니다.";
            values["Archive and Retire Stale Recovery"] =
                "오래된 복구 레코드 보관 및 폐기";
            values["I independently verified the machine and drive physical state. I accept that every listed old-PLC command outcome remains unknown, and I want to archive and retire only the listed stale recovery records."] =
                "장비와 드라이브의 물리 상태를 독립적으로 확인했습니다. 표시된 이전 PLC 명령 결과가 여전히 미확정임을 인정하며, 목록의 오래된 복구 레코드만 보관 후 폐기합니다.";
            values["No PLC command is sent. The current RPC connection must close, and a fresh reconnect must validate the current PLC identity before control or write is permitted."] =
                "PLC 명령은 전송되지 않습니다. 현재 RPC 연결을 닫고 새로 연결해 현재 PLC ID를 확인한 뒤에만 제어 또는 Write가 허용됩니다.";

            values["Single Axis"] = "단축";
            values["Axis object"] = "축 객체";
            values["LASAL object name"] = "LASAL 객체 이름";
            values["Load Axis"] = "축 불러오기";
            values["Axis reference:"] = "축 reference:";
            values["not loaded"] = "불러오지 않음";
            values["Read / control"] = "읽기 / 제어";
            values["Read Status"] = "상태 읽기";
            values["Read Position"] = "위치 읽기";
            values["Power On"] = "전원 켜기";
            values["Power Off"] = "전원 끄기";
            values["Reset"] = "리셋";
            values["Stop"] = "정지";
            values["Latest axis result"] = "최근 축 결과";
            values["Load an axis object, then start with Read Status."] =
                "축 객체를 불러온 다음 상태 읽기부터 시작하십시오.";
            values["Engineering values"] = "엔지니어링 값";
            values["PLC application UNIT"] = "PLC 애플리케이션 UNIT";
            values["Position / distance"] = "위치 / 거리";
            values["Velocity"] = "속도";
            values["Acceleration"] = "가속도";
            values["Deceleration / Stop"] = "감속도 / Stop";
            values["Jerk (axis unit/s^3/1000)"] = "Jerk (축 unit/s^3/1000)";
            values["Velocity direction"] = "속도 방향";
            values["Motion"] = "모션";
            values["Move Absolute"] = "절대 이동";
            values["Move Relative"] = "상대 이동";
            values["Move Velocity"] = "속도 이동";
            values["Absolute/Relative use Shortest. Relative direction comes from the distance sign. Velocity runs until Stop or PowerOff is verified."] =
                "Absolute/Relative는 Shortest를 사용합니다. 상대 방향은 거리 부호로 결정됩니다. Velocity는 Stop 또는 PowerOff가 확인될 때까지 동작합니다.";
            values["Single Axis live qualification (Power / relative Move / Stop / Power Off)"] =
                "단축 실기 qualification (Power / 상대 Move / Stop / Power Off)";
            values["LIVE AXIS MOTION. This runner sends real 0x2023, 0x20A0, 0x2022, and 0x2023 commands. Stop is sent after the planned move reaches standstill; it proves accepted-once/stable Stop handling, not an in-motion halt."] =
                "실제 축이 움직입니다. 이 runner는 실제 0x2023, 0x20A0, 0x2022, 0x2023 명령을 전송합니다. Stop은 계획 이동이 정지한 뒤 전송되며, accepted-once/stable 처리를 검증할 뿐 이동 중 정지 성능을 증명하지 않습니다.";
            values["Relative delta (raw DINT, nonzero, |value| <= 1000000)"] =
                "상대 delta (raw DINT, 0 제외, |값| <= 1000000)";
            values["Velocity (raw DINT, positive)"] = "속도 (raw DINT, 양수)";
            values["Acceleration (raw DINT, positive)"] = "가속도 (raw DINT, 양수)";
            values["Deceleration / Stop (raw DINT, positive)"] =
                "감속도 / Stop (raw DINT, 양수)";
            values["Jerk (raw DINT; first live slice requires 0)"] =
                "Jerk (raw DINT, 첫 실기 단계는 0 필요)";
            values["Final position tolerance (raw DINT, positive)"] =
                "최종 위치 허용오차 (raw DINT, 양수)";
            values["I verified E-stop/STO, software limits, and clear travel for this exact nonzero relative delta."] =
                "이 nonzero 상대 delta에 대해 E-stop/STO, software limit, 이동 공간을 확인했습니다.";
            values["I independently verified the loaded axis name/reference, direction, raw units, target, and tolerance."] =
                "불러온 축 이름/reference, 방향, raw unit, 목표값, 허용오차를 독립적으로 확인했습니다.";
            values["I have exclusive motion ownership; people/tooling are clear and PLC/packet evidence capture is running. Keep the capture running for at least two seconds after PASS."] =
                "모션 제어 권한을 단독으로 보유하고 있으며 사람과 공구가 안전 구역 밖에 있고 PLC/packet 증거 capture가 실행 중입니다. PASS 후 최소 2초 동안 capture를 유지하십시오.";
            values["Run LIVE Axis Qualification"] = "실제 축 Qualification 실행";
            values["Cancel / Safe Cleanup"] = "취소 / 안전 정리";
            values["Save Axis QTEST Log"] = "축 QTEST 로그 저장";
            values["No Single Axis qualification has run yet."] =
                "아직 단축 qualification을 실행하지 않았습니다.";

            values["Group Motion"] = "그룹 모션";
            values["Group object"] = "그룹 객체";
            values["Load Group"] = "그룹 불러오기";
            values["Group reference:"] = "그룹 reference:";
            values["Group commands"] = "그룹 명령";
            values["Get Members"] = "멤버 읽기";
            values["1 Power On"] = "1 전원 켜기";
            values["2 / 5 Read Status (Power Ready / Lock Ready)"] =
                "2 / 5 상태 읽기 (Power Ready / Lock Ready)";
            values["3 Set Identity (Auto Home Check + Configure)"] =
                "3 Identity 설정 (자동 Home Check + Configure)";
            values["4 Enable (Lock Profile)"] = "4 Enable (Profile Lock)";
            values["Disable (Unlock Profile)"] = "Disable (Profile Unlock)";
            values["7 Power Off"] = "7 전원 끄기";
            values["Latest group result"] = "최근 그룹 결과";
            values["Linear motion values"] = "선형 모션 값";
            values["Cartesian 4-axis identity kinematics"] =
                "Cartesian 4축 identity kinematics";
            values["Qualification automation (live PLC motion)"] =
                "Qualification 자동화 (실제 PLC motion)";
            values["6 Move Linear Absolute"] = "6 선형 절대 이동";
            values["6 Move Linear Relative"] = "6 선형 상대 이동";
            values["Home Check (X/Y/Z/U)"] = "Home 확인 (X/Y/Z/U)";

            values["EtherCAT / CREVIS / PI"] = "EtherCAT / CREVIS / PI";
            values["Diagnostics capabilities"] = "진단 capability";
            values["Refresh Capabilities"] = "Capability 새로고침";
            values["Legacy Elmo 4-axis health (fixed drive slots; CREVIS excluded)"] =
                "기존 Elmo 4축 상태 (고정 drive slot, CREVIS 제외)";
            values["Read Legacy Elmo Health"] = "기존 Elmo 상태 읽기";
            values["Configured EtherCAT schema / CREVIS live I/O"] =
                "설정된 EtherCAT schema / CREVIS live I/O";
            values["Load CREVIS / Topology"] = "CREVIS / Topology 불러오기";
            values["Read Selected Health"] = "선택 항목 상태 읽기";
            values["Read Digital Input"] = "Digital Input 읽기";
            values["Read Output Shadow"] = "Output Shadow 읽기";
            values["Submit Output Write"] = "Output Write 전송";
            values["Save Configured Evidence"] = "설정 증거 저장";
            values["Save Live Evidence"] = "실시간 증거 저장";
            values["Auto refresh live state"] = "실시간 상태 자동 새로고침";
            values["Signal Catalog / PI image"] = "신호 카탈로그 / PI 이미지";
            values["Load PI Catalog"] = "PI Catalog 불러오기";
            values["Read Selected PI"] = "선택 PI 읽기";

            values["Bulk Snapshot"] = "Bulk 스냅샷";
            values["Same-cycle Bulk configuration"] = "동일 cycle Bulk 설정";
            values["1 Configure Selected"] = "1 선택 항목 Configure";
            values["2 Refresh Status"] = "2 상태 새로고침";
            values["3 Read Snapshot"] = "3 Snapshot 읽기";
            values["4 Release"] = "4 해제";
            values["Latest same-cycle snapshot"] = "최근 동일 cycle snapshot";
            values["Bulk qualification automation"] = "Bulk qualification 자동화";
            values["Run 24-entry Snapshot Soak"] = "24-entry Snapshot Soak 실행";
            values["Run Configure/Read/Release Soak"] =
                "Configure/Read/Release Soak 실행";
            values["Run One-Slave-Offline Partial"] =
                "단일 Slave Offline Partial 실행";

            values["Recorder"] = "레코더";
            values["Recorder configuration"] = "레코더 설정";
            values["Configure"] = "설정";
            values["Start"] = "시작";
            values["Trigger Now"] = "지금 Trigger";
            values["Refresh Status"] = "상태 새로고침";
            values["Read Header"] = "Header 읽기";
            values["Download"] = "다운로드";
            values["Export CSV"] = "CSV 내보내기";
            values["Release"] = "해제";
            values["Cancel Download"] = "다운로드 취소";
            values["Reconnect / adopt existing Recorder"] =
                "기존 Recorder 재연결 / adopt";
            values["Adopt"] = "인계받기";
            values["Downloaded raw sample plot"] = "다운로드한 raw sample plot";
            values["Recorder qualification automation"] =
                "Recorder qualification 자동화";
            values["Run Single Manual"] = "Single Manual 실행";
            values["Run Ring Forced Trigger"] = "Ring Forced Trigger 실행";
            values["Run Trigger Lifecycle Soak"] = "Trigger Lifecycle Soak 실행";
            values["Run Reconnect Exact Adopt"] = "정확한 Reconnect Adopt 실행";
            values["Run Reconnect 0/0 Discovery"] = "Reconnect 0/0 Discovery 실행";
            values["Cancel Test"] = "테스트 취소";
            values["Save QTEST Log"] = "QTEST 로그 저장";

            values["SDO / Write Policy"] = "SDO / Write 정책";
            values["Persisted Mutation Recovery"] = "저장된 Mutation 복구";
            values["Acknowledge Mutation Recovery"] = "Mutation 복구 확인";
            values["Asynchronous SDO Read / Write ticket"] =
                "비동기 SDO Read / Write ticket";
            values["Submit SDO Read"] = "SDO Read 전송";
            values["Read SDO Inline (wait terminal)"] =
                "SDO Inline 읽기 (terminal 대기)";
            values["Refresh Ticket"] = "Ticket 새로고침";
            values["Cancel Ticket"] = "Ticket 취소";
            values["Load Required Exact Readback"] = "필수 정확 Readback 불러오기";
            values["D5 SDO fault/recovery qualification (read-only)"] =
                "D5 SDO fault/recovery qualification (읽기 전용)";
            values["D5 SDO Write activation qualification (same-value only)"] =
                "D5 SDO Write 활성화 qualification (동일 값 전용)";
            values["Run Same-Value SDO Write Qualification"] =
                "동일 값 SDO Write Qualification 실행";
            values["PI Write policy gate"] = "PI Write 정책 gate";
            values["Submit PI Write"] = "PI Write 전송";
            values["I independently verified the physical target and PLC state; do not replay the previous command."] =
                "물리 target과 PLC 상태를 독립적으로 확인했습니다. 이전 명령을 재전송하지 않습니다.";
            values["I independently verified the physical output and PLC output shadow; do not replay the previous command."] =
                "물리 output과 PLC output shadow를 독립적으로 확인했습니다. 이전 명령을 재전송하지 않습니다.";
            values["I verified the selected output, mask, value, and current shadow revision."] =
                "선택한 output, mask, value, 현재 shadow revision을 확인했습니다.";
            values["I confirmed the selected drive program does not use UI[24] for another purpose."] =
                "선택한 drive program이 UI[24]를 다른 용도로 사용하지 않음을 확인했습니다.";
            values["I established a single-writer window: no drive logic, PLC task, GUI, or other client will change this target until the exact readback completes."] =
                "single-writer 구간을 확보했습니다. 정확한 readback이 끝날 때까지 drive logic, PLC task, GUI, 다른 client가 이 target을 변경하지 않습니다.";
            values["I will record the baseline/original value and independently verify the physical target if the run is interrupted."] =
                "baseline/original 값을 기록하고 실행이 중단되면 물리 target을 독립적으로 확인하겠습니다.";
            values["PLC mailbox evidence and packet capture are running for this activation test."] =
                "이 활성화 시험을 위해 PLC mailbox 증거와 packet capture가 실행 중입니다.";

            values["Read-only API"] = "읽기 전용 API";
            values["Admin capabilities"] = "Admin capability";
            values["Refresh Admin Capabilities"] = "Admin capability 새로고침";
            values["Semantic axis parameter"] = "축 semantic parameter";
            values["Read Axis Parameter"] = "축 parameter 읽기";
            values["Semantic group parameters (group reference fixed at 0x0100)"] =
                "그룹 semantic parameter (group reference 0x0100 고정)";
            values["Read Group Parameters"] = "그룹 parameter 읽기";
            values["Physical drive reads"] = "물리 drive 읽기";
            values["1 Get Drive Operation Mode"] = "1 Drive Operation Mode 읽기";
            values["2 Read Drive Status"] = "2 Drive 상태 읽기";
            values["3 Get Drive Error Code"] = "3 Drive Error Code 읽기";

            values["Copy Log"] = "로그 복사";
            values["Clear Log"] = "로그 지우기";
            values["Execution log / raw callback diagnostics"] =
                "실행 로그 / raw callback 진단";
            values["Execution log / callback diagnostics"] =
                "실행 로그 / 콜백 진단";
            values["Save Result"] = "결과 저장";
            values["Download Result"] = "결과 다운로드";
            values["Save RPC CSV"] = "RPC CSV 저장";
            values["Cancel Runner (not PLC Stop)"] =
                "Runner 취소 (PLC Stop 아님)";
            values["Cancel Inline Wait (PC only)"] =
                "Inline 대기 취소 (PC만)";
            values["Acknowledge Unverified Outcome"] =
                "미확인 결과 승인";
            values["Power On Replay Blocked - Send Power Off"] =
                "Power On 재전송 차단 - Power Off 전송";
            values["Resume Power On Verification (No 0x2023 Replay)"] =
                "Power On 확인 계속 (0x2023 재전송 없음)";
            values["Resume Power Off Verification (No 0x2023 Replay)"] =
                "Power Off 확인 계속 (0x2023 재전송 없음)";
            values["Power Off Again (Confirmed Interference)"] =
                "Power Off 다시 전송 (간섭 확인됨)";
            values["Send Power Off Safety Takeover"] =
                "안전 인수 Power Off 전송";
            values["Reset Replay Blocked - Safety Recovery Required"] =
                "Reset 재전송 차단 - 안전 복구 필요";
            values["Lock State Uncertain - Safe Recovery Required"] =
                "Lock 상태 미확정 - 안전 복구 필요";
            values["Disable Replay Blocked"] = "Disable 재전송 차단";

            AddDynamicRecoveryTranslations(values);

            // Common actionable fields and status text. Protocol identifiers,
            // value-type tokens, and raw numeric values intentionally remain
            // unchanged.
            values["LASAL Motion Control API Example [LIVE Axis qualification / qualified Axis1 UI24 SDO Write]"] =
                "LASAL 모션 제어 API 예제 [실제 축 qualification / 검증된 Axis1 UI24 SDO Write]";
            values["Axis"] = "축";
            values["Axis Error"] = "축 오류";
            values["Operation"] = "작업";
            values["Status"] = "상태";
            values["Detail"] = "상세";
            values["Selection"] = "선택";
            values["Direction"] = "방향";
            values["Value"] = "값";
            values["Value type"] = "값 형식";
            values["Data length"] = "데이터 길이";
            values["Data length (1, 2, or 4)"] = "데이터 길이 (1, 2 또는 4)";
            values["Expected revision"] = "예상 revision";
            values["Diagnostics Boot ID"] = "진단 Boot ID";
            values["Interval (ms)"] = "간격 (ms)";
            values["Iterations"] = "반복 횟수";
            values["Soak iterations"] = "Soak 반복 횟수";
            values["Measured requests (min 10000)"] =
                "측정 request 수 (최소 10000)";
            values["Warm-up requests"] = "준비 request 수";
            values["Timeout (1..60000 cycles)"] =
                "Timeout (1..60000 cycle)";
            values["Normal/recovery timeout cycles"] =
                "일반/복구 timeout cycle";
            values["Tolerance (raw DINT)"] = "허용오차 (raw DINT)";
            values["Velocity (raw DINT)"] = "속도 (raw DINT)";
            values["Acceleration (raw DINT)"] = "가속도 (raw DINT)";
            values["Deceleration / Stop (raw DINT)"] =
                "감속도 / Stop (raw DINT)";
            values["Jerk (raw DINT)"] = "저크 (raw DINT)";
            values["Delta A (raw DINT)"] = "Delta A (raw DINT)";
            values["Delta B (raw DINT)"] = "Delta B (raw DINT)";

            values["Coordinate (Read: None/ACS; Motion: None)"] =
                "좌표계 (읽기: None/ACS, 모션: None)";
            values["Transition"] = "전환 모드";
            values["Buffer ID"] = "버퍼 ID";
            values["Buffer mode"] = "버퍼 모드";
            values["Identity axis Home status"] = "Identity 축 Home 상태";
            values["X axis object"] = "X축 객체";
            values["Y axis object"] = "Y축 객체";
            values["Z axis object"] = "Z축 객체";
            values["U axis object"] = "U축 객체";
            values["X target / delta"] = "X 목표값 / delta";
            values["Y target / delta"] = "Y 목표값 / delta";
            values["Z target / delta"] = "Z 목표값 / delta";
            values["U target / delta"] = "U 목표값 / delta";
            values["Preparation: load the group first."] =
                "준비: 먼저 그룹을 불러오십시오.";
            values["Run Read-only 0x2045 RPC"] =
                "읽기 전용 0x2045 RPC 실행";
            values["Run Enable ACK -> Locked"] =
                "Enable ACK -> Locked 실행";
            values["Run Buffered A -> B"] = "Buffered A -> B 실행";
            values["Run Deterministic Stop-First"] =
                "결정론적 Stop-First 실행";

            values["Digital output masked write"] =
                "Digital output mask Write";
            values["Mask"] = "마스크";
            values["Read a valid selected output shadow before writing."] =
                "Write 전에 선택한 output의 유효한 shadow를 읽으십시오.";
            values["Select a topology row to inspect health or digital I/O."] =
                "상태 또는 digital I/O를 확인할 topology 행을 선택하십시오.";
            values["Auto live monitor: waiting. Configured columns remain static."] =
                "자동 live monitor: 대기 중. 설정된 열은 고정 상태로 유지됩니다.";
            values["Refresh Capabilities and load the PI Catalog."] =
                "Capability를 새로고침하고 PI Catalog를 불러오십시오.";
            values["Connect, then refresh diagnostics capabilities."] =
                "연결한 다음 진단 capability를 새로고침하십시오.";
            values["Load the PI Catalog and check Bulk-readable signals first."] =
                "먼저 PI Catalog를 불러오고 Bulk-readable signal을 선택하십시오.";
            values["Load the PI Catalog and check Recordable signals first."] =
                "먼저 PI Catalog를 불러오고 Recordable signal을 선택하십시오.";
            values["Estimate unavailable until the PI Catalog is loaded."] =
                "PI Catalog를 불러오기 전에는 예상값을 계산할 수 없습니다.";

            values["Sample capacity"] = "샘플 용량";
            values["Sample period (cycles)"] = "샘플 주기 (cycle)";
            values["Pre-trigger samples"] = "Trigger 전 샘플";
            values["Post-trigger samples"] = "Trigger 후 샘플";
            values["Trigger type"] = "Trigger 형식";
            values["Trigger signal"] = "트리거 신호";
            values["Trigger operator"] = "Trigger 연산자";
            values["Trigger value (ignored in Manual mode)"] =
                "Trigger 값 (Manual mode에서는 무시)";
            values["Trigger mask (ignored in Manual mode)"] =
                "Trigger mask (Manual mode에서는 무시)";
            values["Record ID"] = "레코드 ID";
            values["No downloaded data."] = "다운로드한 데이터가 없습니다.";
            values["Double recovery journal is initializing."] =
                "Double 복구 journal을 초기화하는 중입니다.";
            values["Resume External Step"] = "외부 단계 계속";

            values["Object index (0x0001..0xFFFF)"] =
                "객체 인덱스 (0x0001..0xFFFF)";
            values["Sub-index (0..255)"] = "서브인덱스 (0..255)";
            values["Slave reference (1..4)"] = "슬레이브 참조 (1..4)";
            values["Target slave (1..4)"] = "대상 slave (1..4)";
            values["Abort object index"] = "Abort 객체 인덱스";
            values["Abort sub-index"] = "Abort 서브인덱스";
            values["Abort value type"] = "Abort 값 형식";
            values["SDK-approved SDO Write target"] =
                "SDK 승인 SDO Write target";
            values["Single SDK-approved target"] = "SDK 승인 단일 target";
            values["Readiness matrix / next attempt (cached, no wire)"] =
                "준비 상태 matrix / 다음 시도 (cache, wire 전송 없음)";
            values["Last attempt / current run"] = "마지막 시도 / 현재 실행";
            values["Phase 5 read-only transport qualification"] =
                "Phase 5 읽기 전용 transport qualification";
            values["PLC verification boundary"] = "PLC 검증 범위";
            values["Physical axis reference (1..4)"] =
                "물리 축 번호 (1..4)";
            values["Semantic parameter key"] = "Semantic 파라미터 키";
            values["Admin capabilities have not been read."] =
                "Admin capability를 아직 읽지 않았습니다.";
            values["No axis parameter result."] = "축 parameter 결과가 없습니다.";
            values["No group parameter result."] =
                "그룹 parameter 결과가 없습니다.";
            values["No drive read result."] = "Drive 읽기 결과가 없습니다.";
            values["Ready"] = "준비됨";

            values["Set Operation Mode - software target / durable no-replay recovery"] =
                "Operation Mode 설정 - 소프트웨어 target / durable 재전송 방지 복구";
            values["PP(1)/PV(3)/IP(7)/CSP(8) software targets are implemented. Production Start remains disabled until PLC capability and hardware qualification are complete. Homing(6) remains unavailable here."] =
                "PP(1)/PV(3)/IP(7)/CSP(8) 소프트웨어 target이 구현되어 있습니다. PLC capability와 hardware qualification이 완료될 때까지 Production Start는 비활성화됩니다. Homing(6)은 여기서 사용할 수 없습니다.";
            values["I verified the exact drive/axis and understand that this may write DS402 0x6060:0 to the selected mode once only. If the response or completion is uncertain I will use the durable recovery query and will not send Start again."] =
                "정확한 drive/축을 확인했으며 선택한 mode를 DS402 0x6060:0에 한 번만 쓸 수 있음을 이해했습니다. 응답 또는 완료가 불확실하면 durable 복구 조회만 사용하고 Start를 다시 전송하지 않습니다.";
            values["Start Selected Mode Once (0x7D23)"] =
                "선택 Mode 1회 시작 (0x7D23)";
            values["Set Operation Mode Selected Mode Once"] =
                "Operation Mode 선택 Mode 1회 설정";

            AddStaticChromeTranslations(values);

            return values;
        }

        private static void AddDynamicRecoveryTranslations(
            IDictionary<string, string> values)
        {
            // Button captions below are assigned at runtime by UpdateUiState.
            // Keep every safety/recovery branch explicit so Korean mode never
            // falls back to an English command label.
            values["Power Off Safety Takeover"] =
                "안전 인수 Power Off";
            values["Power Off (Durability Degraded)"] =
                "Power Off (durable 기록 저하)";
            values["Resume Reset Verification (No 0x2024 Replay)"] =
                "Reset 확인 계속 (0x2024 재전송 없음)";
            values["Reset Again (Confirmed Interference)"] =
                "Reset 다시 전송 (간섭 확인됨)";
            values["Retry Reset (Outcome Uncertain)"] =
                "Reset 재시도 (결과 미확정)";
            values["Reset Blocked by Stop Recovery"] =
                "Stop 복구로 Reset 차단";
            values["Resume Stop Verification (No 0x2022 Replay)"] =
                "Stop 확인 계속 (0x2022 재전송 없음)";
            values["Retry Stop (Outcome Uncertain)"] =
                "Stop 재시도 (결과 미확정)";
            values["Stop Safety Takeover"] =
                "안전 인수 Stop";
            values["Read Status (Inspection Only)"] =
                "상태 읽기 (확인 전용)";
            values["Observe Pending Reset (Single Group Status)"] =
                "대기 중 Reset 확인 (단일 그룹 상태)";
            values["Observe Pending Power Off (Single Status)"] =
                "대기 중 Power Off 확인 (단일 상태)";
            values["Observe Pending Power On (Single Status)"] =
                "대기 중 Power On 확인 (단일 상태)";
            values["Observe Lock State (Safe Recovery Required)"] =
                "Lock 상태 확인 (안전 복구 필요)";
            values["Verify Pending Lock State (Read Status)"] =
                "대기 중 Lock 상태 확인 (상태 읽기)";
            values["Resume Power On Verification (No 0x204A Replay)"] =
                "Power On 확인 계속 (0x204A 재전송 없음)";
            values["Resume Power Off Verification (No 0x204B Replay)"] =
                "Power Off 확인 계속 (0x204B 재전송 없음)";
            values["Resume Lock Verification (No 0x2047 Replay)"] =
                "Lock 확인 계속 (0x2047 재전송 없음)";
            values["Resume Unlock Verification (No 0x2048 Replay)"] =
                "Unlock 확인 계속 (0x2048 재전송 없음)";
            values["Retry Disable Explicitly (0x2048)"] =
                "Disable 명시적 재시도 (0x2048)";
            values["Disable (Lock-to-Unlock Takeover)"] =
                "Disable (Lock에서 Unlock으로 안전 인수)";
            values["Disable (Reset Safety Recovery)"] =
                "Disable (Reset 안전 복구)";
            values["Disable (Observed Reset LockedStandby)"] =
                "Disable (Reset LockedStandby 확인됨)";
            values["Resume Reset Verification (No 0x2049 Replay)"] =
                "Reset 확인 계속 (0x2049 재전송 없음)";

            values["Arm SDO Write"] = "SDO Write 준비";
            values["Run Same-Value Qualification First"] =
                "먼저 동일 값 Qualification 실행";
            values["Submit Required Exact Readback"] =
                "필수 정확 Readback 전송";
            values["Readback Session Mismatch"] =
                "Readback session 불일치";
            values["Confirm & Submit SDO Write"] =
                "SDO Write 확인 후 전송";
            values["Verify Recovered SDO Readback"] =
                "복구된 SDO Readback 확인";
            values["Acknowledge Stale SDO Write"] =
                "오래된 SDO Write 승인";
            values["Clean Active D5 Ticket (Write quarantine remains)"] =
                "활성 D5 Ticket 정리 (Write 격리 유지)";
            values["SDO Write Quarantine (Read proof unavailable)"] =
                "SDO Write 격리 (Read 증거 없음)";
            values["Resolve D5 Ticket (Readback remains)"] =
                "D5 Ticket 해결 (Readback 유지)";
            values["Exact SDO Write Readback Required"] =
                "정확한 SDO Write Readback 필요";
            values["Resolve D5 Quarantine"] =
                "D5 격리 해결";
            values["Cleanup Retained Double"] =
                "유지된 Double 정리";
            values["Continue Double Recovery"] =
                "Double 복구 계속";
            values["Recover Double Journal"] =
                "Double Journal 복구";
            values["Resume: Slave Is Offline"] =
                "계속: Slave Offline 확인됨";
            values["Resume: Slave Restored"] =
                "계속: Slave 복원됨";
            values["Use the approved external method to make exactly one EtherCAT slave Online=False (not merely non-OP), then click Resume: Slave Is Offline."] =
                "승인된 외부 방법으로 EtherCAT slave 하나만 Online=False 상태로 만드십시오(non-OP만으로는 부족). 그런 다음 계속: Slave Offline 확인됨을 누르십시오.";
            values["Restore the same EtherCAT slave to OP with the approved external method, then click Resume: Slave Restored."] =
                "승인된 외부 방법으로 같은 EtherCAT slave를 OP로 복원한 뒤 계속: Slave 복원됨을 누르십시오.";
            values["Manual SDO Write is fail-closed until this exact connection session, DiagnosticsBuild, BootId, MapRevision, and approved target pass the four-ticket Same-Value SDO Write qualification below."] =
                "현재의 정확한 connection session, DiagnosticsBuild, BootId, MapRevision 및 승인 target이 아래 4-ticket 동일 값 SDO Write qualification을 통과할 때까지 수동 SDO Write는 fail-closed입니다.";
            values["Manual Double configuration-only cleanup is blocked before wire because its route gates are closed."] =
                "Route gate가 닫혀 있어 wire 전 Manual Double configuration-only 정리가 차단됩니다.";
            values["Double-bank retained cleanup is blocked before wire because its proof gates are closed."] =
                "증명 gate가 닫혀 있어 wire 전 Double-bank 유지 항목 정리가 차단됩니다.";
            values["Confirm the exact journal identity and Release order first."] =
                "먼저 정확한 journal identity와 Release 순서를 확인하십시오.";
            values["Release the exact retained configuration only."] =
                "정확히 유지된 configuration만 Release하십시오.";
            values["Release Bank B, Bank A, then the exact configuration."] =
                "Bank B, Bank A, 정확한 configuration 순서로 Release하십시오.";
            values["Double-bank recovery is blocked before wire: ReconnectRecovery proof gate is CLOSED. No inventory, adoption, or Release command will be sent."] =
                "Wire 전 Double-bank 복구가 차단됩니다. ReconnectRecovery 증명 gate가 닫혀 있으며 inventory, 인계 또는 Release 명령을 전송하지 않습니다.";
            values["Double-bank recovery requires the exact advertised two-bank contract."] =
                "Double-bank 복구에는 광고된 정확한 two-bank 계약이 필요합니다.";
            values["Current capability BootId/MapRevision does not match the durable Double-bank record."] =
                "현재 capability의 BootId/MapRevision이 durable Double-bank record와 일치하지 않습니다.";
            values["Use Cleanup Retained Double for the exact current-session handles."] =
                "현재 session의 정확한 handle에는 유지된 Double 정리를 사용하십시오.";
            values["Recover the exact durable Double-bank record without automatic replay."] =
                "자동 재전송 없이 정확한 durable Double-bank record를 복구하십시오.";

            values["No Single Axis qualification recovery journal is available."] =
                "단축 qualification 복구 journal을 사용할 수 없습니다.";
            values["No unresolved Single Axis qualification sequence exists."] =
                "해결되지 않은 단축 qualification sequence가 없습니다.";
            values["Reconnect and load the exact Axis, then resume status-only Power Off verification. Never replay Power On or Move."] =
                "다시 연결해 정확한 축을 불러온 뒤 상태 읽기 전용 Power Off 확인을 계속하십시오. Power On 또는 Move를 재전송하지 마십시오.";
            values["Reconnect and load the exact Axis. Use only explicit Stop and Power Off safety controls with stable proof; no command is replayed automatically."] =
                "다시 연결해 정확한 축을 불러오십시오. 안정 상태를 증명하는 명시적 Stop 및 Power Off 안전 제어만 사용하십시오. 어떤 명령도 자동 재전송하지 않습니다.";
            values["Reconnect and load the exact Axis, then use explicit Power Off with stable PowerOff plus Standstill proof. Power On and Move replay are blocked."] =
                "다시 연결해 정확한 축을 불러온 뒤 명시적 Power Off를 사용하고 안정적인 PowerOff 및 Standstill을 증명하십시오. Power On과 Move 재전송은 차단됩니다.";

            values["Axis Power journal durability is degraded. Explicit safety Power Off remains available through process-local tracking, but status proof cannot claim durable recovery resolution."] =
                "축 Power journal의 durable 기록 기능이 저하됐습니다. Process-local 추적을 통한 명시적 안전 Power Off는 사용할 수 있지만 상태 증거를 durable 복구 완료로 인정할 수 없습니다.";
            values["The Axis Power recovery journal is unavailable; new Power On is disabled. Explicit safety Power Off remains available, but its proof cannot claim a durable journal resolution."] =
                "축 Power 복구 journal을 사용할 수 없어 새 Power On이 차단됩니다. 명시적 안전 Power Off는 사용할 수 있지만 그 증거를 durable journal 복구 완료로 인정할 수 없습니다.";
            values["No durable Axis Power recovery record is active."] =
                "활성 durable 축 Power 복구 record가 없습니다.";
            values["Axis Power On outcome is uncertain. Do not replay Power On; send Power Off explicitly and verify three stable safe samples."] =
                "축 Power On 결과가 미확정입니다. Power On을 재전송하지 말고 Power Off를 명시적으로 전송한 뒤 안전 상태 sample 3회를 안정적으로 확인하십시오.";
            values["Axis Power Off outcome is uncertain. Run exact-identity status-only PowerOn=false plus Standstill verification before considering 0x2023 again."] =
                "축 Power Off 결과가 미확정입니다. 0x2023을 다시 고려하기 전에 exact-identity 상태 읽기만으로 PowerOn=false 및 Standstill을 확인하십시오.";
            values["An accepted Axis Power transition is awaiting exact-identity status-only verification; do not replay 0x2023."] =
                "승인된 축 Power 전환이 exact-identity 상태 읽기 전용 확인을 기다리고 있습니다. 0x2023을 재전송하지 마십시오.";

            values["The Group Power recovery journal is unavailable; new Group Power commands are disabled."] =
                "그룹 Power 복구 journal을 사용할 수 없어 새 그룹 Power 명령이 차단됩니다.";
            values["No durable Group Power recovery record is active."] =
                "활성 durable 그룹 Power 복구 record가 없습니다.";
            values["Group Power On outcome is uncertain. Do not replay Power On; send Power Off explicitly and verify stable PowerOn=false."] =
                "그룹 Power On 결과가 미확정입니다. Power On을 재전송하지 말고 Power Off를 명시적으로 전송한 뒤 안정적인 PowerOn=false를 확인하십시오.";
            values["Group Power Off outcome is uncertain. Run status-only PowerOn=false verification before considering 0x204B again."] =
                "그룹 Power Off 결과가 미확정입니다. 0x204B를 다시 고려하기 전에 상태 읽기만으로 PowerOn=false를 확인하십시오.";
            values["An accepted Group Power transition is awaiting exact-identity status-only verification; do not replay its power command."] =
                "승인된 그룹 Power 전환이 exact-identity 상태 읽기 전용 확인을 기다리고 있습니다. 해당 Power 명령을 재전송하지 마십시오.";

            values["The Group Reset recovery journal is unavailable or corrupt. New mutations are fail-closed; no 0x2049 is sent."] =
                "그룹 Reset 복구 journal을 사용할 수 없거나 손상됐습니다. 새 mutation은 fail-closed이며 0x2049를 전송하지 않습니다.";
            values["A durable Group Reset recovery record is active. Reconnect with the exact PLC/local callback endpoint, DiagnosticsBuild, BootId, and MapRevision; then load the exact group. Recovery refreshes 0x20D2 once and sends only 0x2045/0x2028 status reads. It never replays 0x2049."] =
                "Durable 그룹 Reset 복구 record가 활성 상태입니다. 정확한 PLC/local callback endpoint, DiagnosticsBuild, BootId, MapRevision으로 다시 연결한 뒤 정확한 그룹을 불러오십시오. 복구는 0x20D2를 한 번 새로고침하고 0x2045/0x2028 상태 읽기만 전송합니다. 0x2049를 재전송하지 않습니다.";
            values["Outcome-uncertain Group Reset recovery is attached. Resume status-only group/member clearance proof; the prior 0x2049 outcome remains unknown and is never replayed."] =
                "결과 미확정 그룹 Reset 복구가 연결됐습니다. 상태 읽기 전용 그룹/member 오류 해제 증명을 계속하십시오. 이전 0x2049 결과는 계속 미확정이며 재전송하지 않습니다.";
            values["Group Reset may have been sent, but no accepted continuation exists. Fresh 0x2049 and reconnect are blocked. Use Group Stop, Power Off, safe Disable, or disconnect; status reads are inspection only."] =
                "그룹 Reset이 전송됐을 수 있지만 승인된 continuation이 없습니다. 새 0x2049와 재연결은 차단됩니다. Group Stop, Power Off, 안전 Disable 또는 연결 해제를 사용하십시오. 상태 읽기는 확인 전용입니다.";
            values["Resume Reset Verification in the same live session to send 0x2045/0x2028 reads only, or use Group Stop, Power Off, or safe Disable. Close and new mutations remain blocked while the accepted Reset is pending. Its exact identity and member snapshot are durably retained across disconnect or restart."] =
                "같은 live session에서 Reset 확인을 계속해 0x2045/0x2028 읽기만 전송하거나 Group Stop, Power Off 또는 안전 Disable을 사용하십시오. 승인된 Reset이 대기 중인 동안 Close와 새 mutation은 계속 차단됩니다. 정확한 identity와 member snapshot은 연결 해제나 재시작 후에도 durable하게 유지됩니다.";

            values["Reconnect to the exact recorded endpoint and identity, load only the recorded target, then use Stop or PowerOff and wait for stable safe-state proof. No Move is replayed."] =
                "기록된 정확한 endpoint와 identity로 다시 연결하고 기록된 target만 불러온 뒤 Stop 또는 PowerOff를 사용해 안정적인 안전 상태 증거를 기다리십시오. Move는 재전송하지 않습니다.";

            values["SDO Write evidence is quarantined. Resolve D5 Quarantine cannot clear it with the current Read recovery proof; the quarantine must remain active. Stop, PowerOff, and existing-resource cleanup remain available."] =
                "SDO Write 증거가 격리됐습니다. 현재 Read 복구 증거로는 D5 격리 해결이 이를 해제할 수 없으므로 격리를 유지해야 합니다. Stop, PowerOff 및 기존 resource 정리는 계속 사용할 수 있습니다.";
            values["The SDO Write readback belongs to a different or stale LMCConnection session, so this session cannot submit the exact readback. Mutation and Close remain blocked until the physical target and PLC state are independently verified and Persisted Mutation Recovery is explicitly acknowledged. No command will be replayed."] =
                "SDO Write readback이 다른 session 또는 오래된 LMCConnection session에 속하므로 현재 session에서는 정확한 readback을 전송할 수 없습니다. 물리 target과 PLC 상태를 독립적으로 확인하고 저장된 Mutation 복구를 명시적으로 승인할 때까지 Mutation과 Close는 차단됩니다. 명령은 재전송하지 않습니다.";
            values["Use Resolve D5 Quarantine; Stop, PowerOff, and existing-resource cleanup remain available."] =
                "D5 격리 해결을 사용하십시오. Stop, PowerOff 및 기존 resource 정리는 계속 사용할 수 있습니다.";
            values["Refresh or cancel the digital output ticket. A successful terminal must be followed by an exact output-shadow reread. If the session was lost, physically verify the output before using Acknowledge Unverified Outcome; never replay automatically."] =
                "Digital output ticket을 새로고침하거나 취소하십시오. 성공 terminal 뒤에는 정확한 output-shadow 재읽기가 필요합니다. Session이 끊겼다면 미확인 결과 승인을 사용하기 전에 물리 output을 확인하십시오. 자동 재전송하지 마십시오.";
            values["The durable mutation journal is unavailable. New live/mutation commands and tracked D5 reads are disabled; ordinary non-D5 read-only inspection, Stop, PowerOff, and Group Stop remain available."] =
                "Durable mutation journal을 사용할 수 없습니다. 새 live/mutation 명령과 추적 D5 read는 차단되며 일반 non-D5 읽기 전용 확인, Stop, PowerOff, Group Stop은 계속 사용할 수 있습니다.";
            values["The Double-bank recovery journal is unavailable. New live/mutation commands are disabled; ordinary non-D5 read-only inspection, Stop, PowerOff, and Group Stop remain available. No Double-bank recovery command will be replayed. "] =
                "Double-bank 복구 journal을 사용할 수 없습니다. 새 live/mutation 명령은 차단되며 일반 non-D5 읽기 전용 확인, Stop, PowerOff, Group Stop은 계속 사용할 수 있습니다. Double-bank 복구 명령은 재전송하지 않습니다. ";
        }

        private static void AddStaticChromeTranslations(
            IDictionary<string, string> values)
        {
            // Every static string reached by UiLocalizationService has an
            // explicit catalog entry. Protocol/type literals are deliberately
            // registered with the same value so a new untranslated chrome
            // string cannot hide behind the English fallback.
            values["Bool"] = "Bool";
            values["Int8"] = "Int8";
            values["UInt8"] = "UInt8";
            values["BitField8"] = "BitField8";
            values["Int16"] = "Int16";
            values["UInt16"] = "UInt16";
            values["BitField16"] = "BitField16";
            values["Int32"] = "Int32";
            values["UInt32"] = "UInt32";
            values["BitField32"] = "BitField32";
            values["Real32"] = "Real32";
            values["DS402"] = "DS402";
            values["PDO"] = "PDO";
            values["CFG SDO"] = "CFG SDO";
            values["LIVE AL"] = "LIVE AL";
            values["LIVE EC"] = "LIVE EC";

            values["Jerk"] = "저크";
            values["AL Code"] = "AL 코드";
            values["Alias"] = "별칭";
            values["CFG #"] = "CFG 번호";
            values["CFG Axis"] = "CFG 축";
            values["CFG In bits"] = "CFG 입력 비트";
            values["CFG IO Ref"] = "CFG I/O 참조";
            values["CFG Kind"] = "CFG 유형";
            values["CFG Name"] = "CFG 이름";
            values["CFG Node ID"] = "CFG 노드 ID";
            values["CFG Out bits"] = "CFG 출력 비트";
            values["CFG Parent"] = "CFG 상위 노드";
            values["CFG slave"] = "CFG 슬레이브";
            values["CFG Slave"] = "CFG 슬레이브";
            values["CFG Slot"] = "CFG 슬롯";
            values["CFG Vendor / Product"] = "CFG 제조사 / 제품";
            values["Change Cycle"] = "변경 사이클";
            values["Cycle"] = "사이클";
            values["EC State"] = "EC 상태";
            values["Legacy slot"] = "레거시 슬롯";
            values["LIVE Cycles (H / DI)"] = "LIVE 사이클 (H / DI)";
            values["LIVE Online"] = "LIVE 온라인";
            values["LIVE Quality"] = "LIVE 품질";
            values["LIVE Selected DI"] = "LIVE 선택 DI";
            values["Online"] = "온라인";
            values["Raw Value"] = "Raw 값";
            values["Raw value (decimal or 0x...)"] =
                "Raw 값 (10진수 또는 0x...)";
            values["Signal"] = "신호";
            values["Signal ID"] = "신호 ID";
            values["Type"] = "형식";
            values["Use"] = "사용";
            values["Valid Cycle"] = "유효 사이클";

            values[
                "PC values use mm (x10000); the saved axis transmission is 8388608 counts per 10 mm. "
                + "Re-reference after downloading the new scale. None / raw DINT sends an already converted integer. "
                + "Velocity/acceleration/deceleration must be positive. Enter physical jerk / 1000; the UI applies the selected UNIT."] =
                "PC 값은 mm (x10000)를 사용하며 저장된 축 transmission은 10 mm당 8388608 count입니다. "
                + "새 scale을 다운로드한 뒤 다시 reference 하십시오. None / raw DINT는 이미 변환된 정수를 전송합니다. "
                + "속도/가속도/감속도는 양수여야 합니다. 물리 jerk / 1000 값을 입력하면 UI가 선택한 UNIT을 적용합니다.";
            values[
                "Absolute interprets X/Y/Z/U as targets; Relative interprets them as deltas. "
                + "Positions and dynamics use the selected group application UNIT. The current PLC group uses mm (x10000). "
                + "Read Position accepts None/ACS, but motion remains Coordinate=None only and Move buttons are disabled while ACS is selected. "
                + "For axis-mapping captures, keep three deltas at 0 and move one X/Y/Z/U axis at a time. "
                + "Completion timeout is calculated from distance, velocity, acceleration, and deceleration (15 to 600 seconds)."] =
                "Absolute는 X/Y/Z/U를 목표값으로, Relative는 delta로 해석합니다. "
                + "위치와 동역학 값은 선택한 그룹 application UNIT을 사용하며 현재 PLC 그룹은 mm (x10000)을 사용합니다. "
                + "Read Position은 None/ACS를 허용하지만 motion은 Coordinate=None에서만 가능하고 ACS 선택 중에는 Move 버튼이 비활성화됩니다. "
                + "축 매핑 capture에서는 세 delta를 0으로 두고 X/Y/Z/U 축 하나씩 이동하십시오. "
                + "완료 timeout은 거리, 속도, 가속도, 감속도로 계산되며 범위는 15~600초입니다.";
            values[
                "Home Check reads _LMCAXIS_STATUS.IsReferenced for the four identity axes. "
                + "Set Identity repeats the check automatically and is blocked when any selected axis is not referenced."] =
                "Home Check는 네 identity 축의 _LMCAXIS_STATUS.IsReferenced를 읽습니다. "
                + "Set Identity는 이 확인을 자동으로 반복하며 선택한 축 중 reference되지 않은 축이 있으면 차단됩니다.";
            values[
                "These tests send real group commands. Group Enable qualification starts powered with identity configured but unlocked/disabled. "
                + "Buffered and Stop-first qualifications start powered, identity configured, and locked. Keep people and tooling clear. "
                + "Group Stop and Power Off remain available. Buffered A/B returns to the captured start position only after a verified PASS; "
                + "any uncertain motion is stopped and verified instead."] =
                "이 테스트는 실제 그룹 명령을 전송합니다. Group Enable qualification은 전원이 켜지고 identity가 설정됐지만 unlock/disabled인 상태에서 시작합니다. "
                + "Buffered와 Stop-first qualification은 전원이 켜지고 identity가 설정되고 lock된 상태에서 시작합니다. 사람과 공구를 안전 구역 밖으로 이동하십시오. "
                + "Group Stop과 Power Off는 계속 사용할 수 있습니다. Buffered A/B는 PASS가 확인된 뒤에만 capture한 시작 위치로 복귀하며, "
                + "motion이 불확실하면 대신 정지 후 확인합니다.";
            values[
                "Required order: 1 Power On (automatic three-sample PowerOn=True verification) -> 3 Set Identity (automatic Home Check) -> "
                + "4 Enable (automatic three-sample Locked Standby verification) -> 6 Move -> Disable (Unlock Profile) -> "
                + "7 Power Off (automatic three-sample PowerOn=False verification). If a verification is interrupted, press the same Power or Enable button "
                + "to resume status reads without command replay. Read Status observes one sample and does not complete a pending Power On/Off transition."] =
                "필수 순서: 1 전원 켜기(자동 3회 PowerOn=True 확인) -> 3 Identity 설정(자동 Home Check) -> "
                + "4 Enable(자동 3회 Locked Standby 확인) -> 6 이동 -> Disable(Profile Unlock) -> "
                + "7 전원 끄기(자동 3회 PowerOn=False 확인). 확인이 중단되면 같은 Power 또는 Enable 버튼을 눌러 명령을 재전송하지 않고 상태 읽기만 계속하십시오. "
                + "Read Status는 한 sample만 확인하며 보류 중인 Power On/Off 전환을 완료하지 않습니다.";
            values[
                "Sends Group Power On once, then requires three consecutive PowerOn=True status samples. "
                + "A pending verification resumes with status reads only."] =
                "Group Power On을 한 번 전송한 뒤 PowerOn=True 상태 sample이 3회 연속이어야 합니다. "
                + "보류 중인 확인은 상태 읽기만으로 계속됩니다.";
            values[
                "Sends Group Enable once, then requires three consecutive powered Locked Standby status samples. "
                + "A pending verification resumes with status reads only."] =
                "Group Enable을 한 번 전송한 뒤 전원이 켜진 Locked Standby 상태 sample이 3회 연속이어야 합니다. "
                + "보류 중인 확인은 상태 읽기만으로 계속됩니다.";
            values[
                "Unlocks the profile. A successful Disable also resolves a pending accepted Group Enable."] =
                "Profile lock을 해제합니다. Disable이 성공하면 승인 후 확인 중이던 Group Enable도 해소됩니다.";
            values[
                "Sends Group Power Off once, then requires three consecutive PowerOn=False status samples. "
                + "A pending verification resumes with status reads only."] =
                "Group Power Off를 한 번 전송한 뒤 PowerOn=False 상태 sample이 3회 연속이어야 합니다. "
                + "보류 중인 확인은 상태 읽기만으로 계속됩니다.";
            values[
                "Sequentially reads Group Status (0x2045) and requires stable InPosition throughout. Run only with an exclusive PLC session. "
                + "Reported latency is the PC API RPC elapsed time (UI dispatch and command-gate wait excluded); it is not PLC dispatch time, task jitter, or an overrun measurement. "
                + "Cancel takes effect before the next RPC; the current RPC is allowed to finish."] =
                "Group Status(0x2045)를 순차적으로 읽으며 전체 과정에서 InPosition이 안정적으로 유지돼야 합니다. PLC session을 단독 점유한 상태에서만 실행하십시오. "
                + "표시 latency는 UI dispatch와 command-gate 대기를 제외한 PC API RPC 경과 시간이며 PLC dispatch 시간, task jitter 또는 overrun 측정값이 아닙니다. "
                + "Cancel은 다음 RPC 전에 적용되고 현재 RPC는 완료될 수 있습니다.";
            values["No qualification has run yet."] =
                "아직 qualification을 실행하지 않았습니다.";

            values[
                "PI/Bulk capture order: Refresh Capabilities -> Load PI Catalog -> check Use signals -> Read Selected PI -> "
                + "switch to Bulk Snapshot -> Configure Selected -> Refresh Status -> Read Snapshot -> Release."] =
                "PI/Bulk capture 순서: Capability 새로고침 -> PI Catalog 불러오기 -> 사용할 signal 선택 -> 선택 PI 읽기 -> "
                + "Bulk Snapshot 탭으로 이동 -> 선택 항목 Configure -> 상태 새로고침 -> Snapshot 읽기 -> 해제.";
            values[
                "CONFIGURED SCHEMA ONLY: bit 14 (0x7E11/0x7E12) reports the PLC serializer/configuration; it is not runtime EtherCAT discovery. "
                + "LIVE health, DI/output shadow, and DO write require bits 15/16/17 and separate requests; output shadow is not physical feedback."] =
                "설정 schema 전용: bit 14(0x7E11/0x7E12)는 PLC serializer/configuration을 보고하며 runtime EtherCAT 검색이 아닙니다. "
                + "LIVE 상태, DI/output shadow, DO write에는 bit 15/16/17과 별도 request가 필요합니다. Output shadow는 물리 feedback이 아닙니다.";
            values[
                "Legacy command 0x7E10 reports the four Elmo drives only. CREVIS configured topology and I/O are shown in the separate section below."] =
                "Legacy 명령 0x7E10은 Elmo drive 4개만 보고합니다. CREVIS 설정 topology와 I/O는 아래의 별도 영역에 표시됩니다.";
            values["Health has not been read."] =
                "상태를 아직 읽지 않았습니다.";
            values[
                "Connect to auto-load CREVIS / Topology. Reload refreshes capabilities and retries; configured CREVIS rows require PLC bit 14."] =
                "연결하면 CREVIS / Topology를 자동으로 불러옵니다. 다시 불러오기는 capability를 새로고침하고 재시도하며, 설정된 CREVIS 행에는 PLC bit 14가 필요합니다.";
            values[
                "Live evidence: retained=0, dropped=0. Parsed current-session PLC responses only; not physical wiring or I/O proof."] =
                "Live 증거: retained=0, dropped=0. 현재 session의 PLC response만 parse하며 물리 배선 또는 I/O 동작 증거가 아닙니다.";
            values[
                "GUARDED WRITE: Output values are PLC RT-owner shadows, not physical feedback. Submit remains disabled unless capability bit 17, "
                + "the SDK allowlist, a valid output-shadow revision, and explicit confirmation all match. A failed readback remains blocked for exact reread; "
                + "connection loss before terminal proof requires the separate physical/PLC-shadow verification checkbox and explicit operator acknowledgement."] =
                "보호된 WRITE: Output 값은 PLC RT-owner shadow이며 물리 feedback이 아닙니다. Capability bit 17, SDK allowlist, 유효한 output-shadow revision, "
                + "명시적 확인이 모두 일치해야 Submit이 활성화됩니다. Readback 실패 시 정확한 재읽기 전까지 차단되며, terminal 증명 전에 연결이 끊기면 "
                + "별도의 물리/PLC-shadow 확인 checkbox와 작업자 승인이 필요합니다.";
            values[
                "Check signals to use for PI reads, Bulk snapshots, and Recorder capture. Only catalog flags permitted by the PLC are used."] =
                "PI 읽기, Bulk snapshot, Recorder capture에 사용할 signal을 선택하십시오. PLC가 허용한 catalog flag만 사용됩니다.";

            values[
                "The runner reloads Capabilities and the PI Catalog, selects exactly all 24 BulkReadable entries in Catalog order, "
                + "uses the revision-bound builder facade, and always releases the reader. The Partial workflow never injects a fault: "
                + "with Group PowerOff and Disabled, the operator makes exactly one slave Online=False (not merely non-OP), resumes, "
                + "restores that same slave to OP, and resumes again."] =
                "Runner는 Capability와 PI Catalog를 다시 불러오고 Catalog 순서대로 BulkReadable 항목 24개를 정확히 모두 선택하며, "
                + "revision에 묶인 builder facade를 사용하고 항상 reader를 해제합니다. Partial workflow는 fault를 주입하지 않습니다. "
                + "Group PowerOff/Disabled 상태에서 작업자가 정확히 한 slave를 Online=False(단순 non-OP가 아님)로 만든 뒤 계속하고, "
                + "같은 slave를 OP로 복구한 다음 다시 계속합니다.";

            values[
                "Manual ignores all trigger fields. Edge uses TriggerValue as its threshold and forces TriggerMask to zero. "
                + "Window maps lower bound to TriggerValue and upper bound to TriggerMask; signed Int16/Int32 bounds accept signed decimal input. "
                + "Mask requires a BitField16/32 signal and a non-zero TriggerMask."] =
                "Manual은 모든 trigger field를 무시합니다. Edge는 TriggerValue를 threshold로 사용하고 TriggerMask를 0으로 강제합니다. "
                + "Window는 하한을 TriggerValue, 상한을 TriggerMask에 매핑하며 signed Int16/Int32 범위에는 부호 있는 10진수를 입력할 수 있습니다. "
                + "Mask에는 BitField16/32 signal과 0이 아닌 TriggerMask가 필요합니다.";
            values[
                "Uses checked Recordable signals from the PI Catalog. Trigger modes require PLC capability. Double mode stays hidden until the ManualActions proof gate "
                + "and durable recoverable manual route are both open. Adopt is separately blocked on a Double-capable target while the ReconnectRecovery proof gate "
                + "is closed because the target mode is unknown before wire."] =
                "PI Catalog에서 선택한 Recordable signal을 사용합니다. Trigger mode에는 PLC capability가 필요합니다. ManualActions 증명 gate와 durable 복구 가능한 manual route가 "
                + "모두 열릴 때까지 Double mode는 숨겨집니다. Double-capable target에서는 wire 전 target mode를 알 수 없으므로 ReconnectRecovery 증명 gate가 닫혀 있는 동안 Adopt도 별도로 차단됩니다.";
            values[
                "Download copies a frozen PLC recording into this app's PC memory; it does not create a file. Export CSV opens a Save dialog and writes the downloaded PC data to the file you choose."] =
                "Download는 고정된 PLC recording을 이 앱의 PC 메모리로 복사하며 파일을 만들지 않습니다. Export CSV는 저장 창을 열고 다운로드된 PC 데이터를 선택한 파일에 기록합니다.";
            values[
                "Start fills these IDs automatically and they remain visible after disconnect. Reconnect to the same PLC boot, refresh Capabilities, then Adopt. "
                + "Record ID=0 and Buffer ID=0 discovers and adopts the current single-bank Recorder; nonzero Record ID keeps exact adoption. "
                + "Read Status for authoritative terminal metadata or Header before Download. Trigger Now explicitly fires a locally configured non-Manual D4 recorder "
                + "and is enabled only when RecorderTrigger is advertised."] =
                "Start가 ID를 자동으로 채우며 연결이 끊긴 뒤에도 표시됩니다. 같은 PLC boot에 다시 연결하고 Capability를 새로고친 뒤 인계받으십시오. "
                + "Record ID=0, Buffer ID=0은 현재 single-bank Recorder를 검색해 인계받고, 0이 아닌 Record ID는 정확한 대상을 유지합니다. "
                + "권위 있는 terminal metadata는 Read Status로, Download 전에는 Header로 확인하십시오. Trigger Now는 로컬 설정된 non-Manual D4 recorder를 명시적으로 trigger하며 "
                + "RecorderTrigger가 광고될 때만 활성화됩니다.";
            values[
                "Single validates two identical downloads. Ring uses the advertised local forced-trigger path. Double-bank qualification has separate QualificationExecution "
                + "and ReconnectRecovery proof gates; both remain closed. Trigger Soak repeats a compact 32-sample lifecycle. Reconnect Exact and Discovery intentionally "
                + "close and reopen this app's RPC connection, adopt the preserved Ring Recorder, download it, and release it. External RT evidence remains a separate operator workflow."] =
                "Single은 동일한 download 두 개를 검증합니다. Ring은 광고된 로컬 forced-trigger 경로를 사용합니다. Double-bank qualification에는 QualificationExecution과 "
                + "ReconnectRecovery 증명 gate가 각각 있으며 둘 다 닫혀 있습니다. Trigger Soak는 32-sample lifecycle을 반복합니다. Reconnect Exact와 Discovery는 의도적으로 "
                + "앱의 RPC 연결을 닫고 다시 연 뒤 유지된 Ring Recorder를 인계받아 다운로드하고 해제합니다. 외부 RT 증거는 별도의 작업자 workflow입니다.";
            values["Double Bank (proof gates closed)"] =
                "Double Bank (증명 gate 닫힘)";
            values[
                "Double-bank QualificationExecution and ReconnectRecovery proof gates are closed."] =
                "Double-bank QualificationExecution 및 ReconnectRecovery 증명 gate가 닫혀 있습니다.";
            values[
                "I verified the exact Double journal identity and full displayed Release plan."] =
                "정확한 Double journal identity와 표시된 전체 해제 계획을 확인했습니다.";
            values["Cleanup Retained Double (gates closed)"] =
                "유지된 Double 정리 (gate 닫힘)";
            values[
                "No exact same-session Double-bank qualification handles are retained."] =
                "같은 session의 정확한 Double-bank qualification handle이 유지되어 있지 않습니다.";
            values["Recover Double Journal (gate closed)"] =
                "Double Journal 복구 (gate 닫힘)";
            values[
                "Double-bank ReconnectRecovery proof gate is closed. No inventory, adoption, or Release command will be sent."] =
                "Double-bank ReconnectRecovery 증명 gate가 닫혀 있습니다. Inventory, 인계 또는 Release 명령을 전송하지 않습니다.";

            values["Opening the durable mutation journal..."] =
                "Durable mutation journal을 여는 중...";
            values["Write value (decimal or raw 0x hex)"] =
                "Write 값 (10진수 또는 raw 0x hex)";
            values[
                "Write mode remains fail-closed until the exact current session passes the four-ticket Same-Value SDO Write qualification, "
                + "then changes this button to Arm SDO Write."] =
                "현재 정확한 session이 4-ticket Same-Value SDO Write qualification을 통과할 때까지 Write mode는 fail-closed이며, "
                + "통과하면 이 버튼이 SDO Write Arm으로 바뀝니다.";
            values[
                "Submit one ordinary 1/2/4-byte SDO Read and wait for its terminal result. This does not run SDO Write, required Write readback, or CREVIS I/O."] =
                "일반 1/2/4-byte SDO Read 하나를 전송하고 terminal 결과를 기다립니다. SDO Write, 필수 Write readback 또는 CREVIS I/O는 실행하지 않습니다.";
            values[
                "Cancel only the PC-side Inline Read wait. This never sends CancelOperation, never cancels the PLC ticket, and never retries the SDO request."] =
                "PC 측 Inline Read 대기만 취소합니다. CancelOperation을 전송하거나 PLC ticket을 취소하거나 SDO request를 재시도하지 않습니다.";
            values[
                "Restore the exact read request required to verify the last successful SDO Write. This does not send a command."] =
                "마지막으로 성공한 SDO Write를 확인하는 데 필요한 정확한 read request를 복원합니다. 명령은 전송하지 않습니다.";
            values[
                "SDO Read supports exact 1/2/4-byte typed values. Manual SDO Write also requires a four-ticket Same-Value qualification PASS bound to the exact current session "
                + "and PLC identity; arbitrary object writes remain blocked."] =
                "SDO Read는 정확한 1/2/4-byte typed 값을 지원합니다. Manual SDO Write에는 현재 정확한 session과 PLC identity에 묶인 4-ticket Same-Value qualification PASS도 필요하며, "
                + "임의 object write는 계속 차단됩니다.";
            values[
                "These runners never write an SDO and never inject an EtherCAT fault. Abort qualification uses a manufacturer-approved nonexistent read-only object/sub-index. "
                + "Contention, timeout, and queued-cancel qualification use only 0x6061:0 Int8/1. Abrupt Disconnect intentionally drops only the API TCP transport without sending RPC Close (0x405D), "
                + "reconnects, and performs two exact 0x6061:0 reads. The selected Slave 1..4 is mapped to _LMCAxis1..4; every runner requires three stable PowerOn=False, "
                + "Standstill=True position samples before submitting any D5 ticket."] =
                "이 runner는 SDO를 쓰거나 EtherCAT fault를 주입하지 않습니다. Abort qualification은 제조사 승인된 존재하지 않는 읽기 전용 object/sub-index를 사용합니다. "
                + "Contention, timeout, queued-cancel qualification은 0x6061:0 Int8/1만 사용합니다. Abrupt Disconnect는 RPC Close(0x405D)를 보내지 않고 API TCP transport만 의도적으로 끊은 뒤 "
                + "재연결하여 0x6061:0을 정확히 두 번 읽습니다. 선택한 Slave 1..4는 _LMCAxis1..4에 매핑되며 모든 runner는 D5 ticket 전송 전에 "
                + "PowerOn=False, Standstill=True 위치 sample 3회가 안정적으로 일치해야 합니다.";
            values[
                "Abort PASS: stable BootId/MapRevision -> baseline -> exact SDO abort -> same-value recovery. Contention PASS: first 0x6061 ticket accepted -> "
                + "second Submit exact Rejected/ResourceBusy before first terminal -> first Completed/Success -> third distinct ticket Completed/Success with the exact first value. "
                + "Timeout PASS: 0x6061 TimeoutCycles=1 -> exact Expired/TimedOut -> bounded exact ResourceBusy drain wait -> distinct same-value recovery. "
                + "Queued Cancel PASS: submit 0x6061 -> immediate one-shot CancelOperation -> exact Cancelled/Cancelled -> distinct same-value recovery. "
                + "Abrupt Disconnect can PASS only application-visible new-connection recovery; orphanQualified remains false until a PLC MarkOrphan/late-callback witness exists. "
                + "A terminal-before-loss or Running race is INCONCLUSIVE for PLC orphan proof. Unexpected acceptance or uncertain outcome is preserved and blocks automatic retry. "
                + "Cancel Runner does not send PLC Stop."] =
                "Abort PASS: 안정적인 BootId/MapRevision -> baseline -> 정확한 SDO abort -> 동일 값 복구. Contention PASS: 첫 0x6061 ticket 승인 -> "
                + "첫 terminal 전에 두 번째 Submit이 정확히 Rejected/ResourceBusy -> 첫 ticket Completed/Success -> 정확히 같은 첫 값으로 세 번째 별도 ticket Completed/Success. "
                + "Timeout PASS: 0x6061 TimeoutCycles=1 -> 정확한 Expired/TimedOut -> 제한된 ResourceBusy drain 대기 -> 별도 동일 값 복구. "
                + "Queued Cancel PASS: 0x6061 전송 -> 즉시 one-shot CancelOperation -> 정확한 Cancelled/Cancelled -> 별도 동일 값 복구. "
                + "Abrupt Disconnect는 앱에서 확인 가능한 새 연결 복구만 PASS할 수 있으며 PLC MarkOrphan/late-callback 증거가 생길 때까지 orphanQualified는 false입니다. "
                + "연결 손실 전 terminal 완료 또는 Running race는 PLC orphan 증명에 대해 INCONCLUSIVE입니다. 예상 밖 승인 또는 불확실한 결과는 보존되어 자동 재시도를 차단합니다. "
                + "Cancel Runner는 PLC Stop을 전송하지 않습니다.";
            values["Run D5 Abort -> Recovery"] =
                "D5 Abort -> 복구 실행";
            values["Run D5 Contention -> Recovery"] =
                "D5 Contention -> 복구 실행";
            values["Run D5 Timeout -> Recovery"] =
                "D5 Timeout -> 복구 실행";
            values["Run D5 Queued Cancel -> Recovery"] =
                "D5 Queued Cancel -> 복구 실행";
            values["Run D5 Abrupt Disconnect -> App Recovery"] =
                "D5 Abrupt Disconnect -> 앱 복구 실행";
            values["No D5 SDO qualification has run yet."] =
                "아직 D5 SDO qualification을 실행하지 않았습니다.";
            values[
                "LIVE WRITE: this runner reads the approved target, writes the exact same four bytes, and requires an exact readback. "
                + "It never writes a sentinel and never performs an automatic restore or replay."] =
                "실제 WRITE: 이 runner는 승인된 target을 읽고 정확히 같은 4 byte를 쓴 뒤 정확한 readback을 요구합니다. "
                + "Sentinel을 쓰지 않으며 자동 복원이나 재전송을 수행하지 않습니다.";
            values[
                "The button stays fail-closed unless the PLC advertises bits 8/9/13, this SDK build exposes exactly one approved target, "
                + "the durable mutation journal is available, and all four operator checks are selected. After confirmation, the selected axis is checked again for "
                + "PowerOn=False, Standstill=True, and stable position; the target is also read again and must still match the baseline before Write submission."] =
                "PLC가 bit 8/9/13을 광고하고, 이 SDK build가 승인 target을 정확히 하나만 노출하고, durable mutation journal을 사용할 수 있고, "
                + "작업자 확인 4개가 모두 선택돼야 버튼이 fail-closed 상태에서 풀립니다. 확인 후 선택 축의 PowerOn=False, Standstill=True, 안정 위치를 다시 검사하고 "
                + "target도 다시 읽어 baseline과 계속 일치해야 Write를 전송합니다.";
            values["OVERALL CLOSED | EVALUATION_WIRE=NONE"] =
                "전체 차단 | EVALUATION_WIRE=NONE";
            values["No same-value SDO Write qualification has run yet."] =
                "아직 동일 값 SDO Write qualification을 실행하지 않았습니다.";
            values[
                "Select exactly one WritableByPolicy Catalog entry on the EtherCAT / PI tab. The call remains fail-closed until schema v1 defines the payload "
                + "and a compile-time allowlist is approved. DS402 ControlWord and target objects are permanently blocked."] =
                "EtherCAT / PI 탭에서 WritableByPolicy Catalog 항목을 정확히 하나 선택하십시오. Schema v1이 payload를 정의하고 compile-time allowlist가 승인될 때까지 호출은 fail-closed입니다. "
                + "DS402 ControlWord와 target object는 영구 차단됩니다.";

            values[
                "Admin commands 0x7D00/0x7D10/0x7D20 require the matching LASAL source on the PLC. The 2026-07-23 axis 1..4/group happy-path capture passed; "
                + "invalid-input, stale-session, and fault cases remain separate tests. Every command on this tab is read-only."] =
                "Admin 명령 0x7D00/0x7D10/0x7D20에는 PLC의 일치하는 LASAL source가 필요합니다. 2026-07-23 축 1..4/그룹 happy-path capture는 통과했으며 "
                + "invalid-input, stale-session, fault case는 별도 테스트입니다. 이 탭의 모든 명령은 읽기 전용입니다.";
            values[
                "Drive capture: scroll to Physical drive reads at the bottom of this tab, select axes 1..4, then run Get Drive Operation Mode and Read Drive Status."] =
                "Drive capture: 이 탭 아래의 물리 drive 읽기로 이동하여 축 1..4를 선택한 뒤 Get Drive Operation Mode와 Read Drive Status를 실행하십시오.";
            values[
                "The key is semantic; native LASAL MotionLib enum values are not exposed on the PC wire."] =
                "키는 semantic 값이며 native LASAL MotionLib enum 값은 PC wire에 노출되지 않습니다.";
            values[
                "Read Drive Status is explicitly non-atomic: LASAL axis status, DS402 0x6041:0, and DS402 0x6061:0 are read sequentially and may come from different PLC/EtherCAT cycles. "
                + "Get Drive Error Code is a separate one-ticket 0x603F:0 read. LASAL 0x2028 StatusWord is reserved zero and is not DS402 0x6041 evidence."] =
                "Read Drive Status는 명시적으로 non-atomic입니다. LASAL 축 상태, DS402 0x6041:0, DS402 0x6061:0을 순차적으로 읽으므로 서로 다른 PLC/EtherCAT cycle의 값일 수 있습니다. "
                + "Get Drive Error Code는 별도의 one-ticket 0x603F:0 읽기입니다. LASAL 0x2028 StatusWord는 예약된 0이며 DS402 0x6041 증거가 아닙니다.";

            values[
                "LASAL Motion Control API Example is already running in this Windows session.\n\n"
                + "Close the existing instance before starting another. This second instance will exit before opening recovery journals or network ports."] =
                "이 Windows session에서 LASAL 모션 제어 API 예제가 이미 실행 중입니다.\n\n"
                + "다른 instance를 시작하기 전에 기존 프로그램을 닫으십시오. 두 번째 instance는 recovery journal 또는 network port를 열기 전에 종료됩니다.";
            values["LASAL Motion Control API Example - Already Running"] =
                "LASAL 모션 제어 API 예제 - 이미 실행 중";
            values[
                "Startup was blocked because the single-instance guard could not be acquired. "
                + "No recovery journal or network port was opened.\n\n"] =
                "Single-instance guard를 확보하지 못해 시작이 차단됐습니다. "
                + "Recovery journal 또는 network port를 열지 않았습니다.\n\n";
            values["LASAL Motion Control API Example - Startup Blocked"] =
                "LASAL 모션 제어 API 예제 - 시작 차단";

            values[
                "The exact SDO readback cannot run in the current connection session. Confirm that the SDO target and PLC state were checked independently."] =
                "현재 연결 session에서는 정확한 SDO readback을 실행할 수 없습니다. SDO target과 PLC 상태를 독립적으로 확인했는지 확인하십시오.";
            values[
                "Confirm that the physical target and PLC state were checked independently."] =
                "물리 target과 PLC 상태를 독립적으로 확인했는지 확인하십시오.";
            values[
                "This writes a durable Resolved tombstone. It does not replay the command or prove the previous outcome."] =
                "Durable Resolved tombstone을 기록합니다. 명령을 재전송하지 않으며 이전 결과를 증명하지 않습니다.";
            values["Acknowledge Recovered Mutation"] =
                "복구된 Mutation 승인";
            values[
                "The durable Resolved tombstone could not be written. The interlock remains active."] =
                "Durable Resolved tombstone을 기록하지 못했습니다. Interlock은 계속 활성 상태입니다.";
            values["Mutation Recovery Failed"] =
                "Mutation 복구 실패";

            values[
                "This action archives the exact original recovery journal bytes, then marks only the listed old-PLC records Resolved."] =
                "이 작업은 원본 recovery journal byte를 정확히 보관한 뒤 목록에 표시된 이전 PLC record만 Resolved로 표시합니다.";
            values[
                "It does NOT prove whether any old command succeeded or failed. Every listed outcome remains UNKNOWN."] =
                "이 작업은 이전 명령의 성공 또는 실패를 증명하지 않습니다. 목록의 모든 결과는 계속 UNKNOWN입니다.";
            values[
                "No Motion, Power, SDO, Write, replay, or cleanup command is sent. A second read-only Capabilities query will recheck the same TCP session and PLC identity after confirmation."] =
                "Motion, Power, SDO, Write, replay 또는 cleanup 명령은 전송하지 않습니다. 확인 후 두 번째 읽기 전용 Capability query로 같은 TCP session과 PLC identity를 다시 확인합니다.";
            values[
                "After success, this quarantined connection closes and the application must restart."] =
                "성공 후 격리된 연결을 닫으며 애플리케이션을 다시 시작해야 합니다.";
            values["Current: "] = "현재: ";
            values["RETIRE STALE - records to archive and resolve:"] =
                "오래된 record 폐기 - 보관 후 resolve할 record:";
            values[
                "KEEP EXACT CURRENT - records left active for exact recovery:"] =
                "현재 identity 일치 record 유지 - 정확한 복구를 위해 활성 상태로 둘 record:";
            values[
                "KEEP OTHER ENDPOINT - records left active for their recorded endpoint:"] =
                "다른 endpoint record 유지 - 기록된 endpoint에서 처리하도록 활성 상태로 둘 record:";
            values["- none"] = "- 없음";
            values[
                "Proceed only if you independently verified the physical machine and drive state."] =
                "장비와 drive의 물리 상태를 독립적으로 확인한 경우에만 계속하십시오.";
            values[
                "The exact stale recovery records were archived and retired. Their command outcomes remain unknown. "
                + "The quarantined TCP session is closed; the application will now exit. Start it again and address kept records at their recorded endpoints. "
                + "Any kept exact-current record must finish its exact status-only recovery before Motion, Power, or the approved SDO Write can open."] =
                "정확한 stale recovery record를 보관하고 폐기했습니다. 해당 명령의 결과는 계속 미확정입니다. "
                + "격리된 TCP session을 닫았으며 애플리케이션을 종료합니다. 다시 시작한 뒤 유지된 record를 각 기록 endpoint에서 처리하십시오. "
                + "유지된 exact-current record는 Motion, Power 또는 승인된 SDO Write가 열리기 전에 정확한 status-only 복구를 완료해야 합니다.";
            values["Restart Required"] = "다시 시작 필요";

            values["Submit guarded digital output write?"] =
                "보호된 digital output write를 전송하시겠습니까?";
            values["Confirm Digital Output Write"] =
                "Digital Output Write 확인";
            values[
                "This clears only the GUI output-write interlock. It does not prove whether the PLC applied the write."] =
                "GUI output-write interlock만 해제합니다. PLC가 write를 적용했는지는 증명하지 않습니다.";
            values[
                "Confirm the physical output and PLC output shadow independently before continuing."] =
                "계속하기 전에 물리 output과 PLC output shadow를 독립적으로 확인하십시오.";
            values["Clear the unverified-outcome interlock now?"] =
                "미확인 결과 interlock을 지금 해제하시겠습니까?";
            values["Acknowledge Unverified Digital Output Outcome"] =
                "미확인 Digital Output 결과 승인";
            values[
                "The durable Resolved tombstone could not be written. The output interlock remains active."] =
                "Durable Resolved tombstone을 기록하지 못했습니다. Output interlock은 계속 활성 상태입니다.";
            values["Output Recovery Failed"] =
                "Output 복구 실패";

            values["Export LASAL Recorder CSV"] =
                "LASAL Recorder CSV 저장";
            values["Save recorder CSV"] =
                "Recorder CSV 저장";
            values["Save LASAL SDO Result"] =
                "LASAL SDO 결과 저장";
            values["Save SDO result"] =
                "SDO 결과 저장";
            values["Save qualification log"] =
                "Qualification 로그 저장";
            values["Save Phase 5 read-only API RPC samples"] =
                "Phase 5 읽기 전용 API RPC sample 저장";
            values["Save Configured EtherCAT Topology Evidence"] =
                "설정된 EtherCAT Topology 증거 저장";
            values["Save configured EtherCAT topology evidence"] =
                "설정된 EtherCAT topology 증거 저장";
            values["Save Live EtherCAT Topology / I/O Evidence"] =
                "Live EtherCAT Topology / I/O 증거 저장";
            values["Save live EtherCAT topology and I/O evidence"] =
                "Live EtherCAT topology 및 I/O 증거 저장";
            values["CSV files (*.csv)|*.csv|All files (*.*)|*.*"] =
                "CSV 파일 (*.csv)|*.csv|모든 파일 (*.*)|*.*";
            values["Binary files (*.bin)|*.bin|All files (*.*)|*.*"] =
                "Binary 파일 (*.bin)|*.bin|모든 파일 (*.*)|*.*";
            values[
                "Text file (*.txt)|*.txt|Qualification log (*.log)|*.log|All files (*.*)|*.*"] =
                "Text 파일 (*.txt)|*.txt|Qualification 로그 (*.log)|*.log|모든 파일 (*.*)|*.*";
            values["Text files (*.txt)|*.txt|All files (*.*)|*.*"] =
                "Text 파일 (*.txt)|*.txt|모든 파일 (*.*)|*.*";
            values[
                "Text evidence (*.txt)|*.txt|CSV evidence (*.csv)|*.csv"] =
                "Text 증거 (*.txt)|*.txt|CSV 증거 (*.csv)|*.csv";

            values[
                "Auto live monitor: waiting for a connected topology and live-read capabilities. Configured columns remain static."] =
                "자동 live monitor: 연결된 topology와 live-read capability를 기다리는 중입니다. 설정 column은 정적으로 유지됩니다.";
            values["CLOSED: connect to the PLC first."] =
                "차단: 먼저 PLC에 연결하십시오.";
            values[
                "Double recovery journal ready. No unresolved durable Double-bank record."] =
                "Double 복구 journal 준비됨. 해결되지 않은 durable Double-bank record가 없습니다.";
            values[
                "Double-bank contract gate is closed: requires RecorderSingleBank, RecorderDoubleBank, exactly two buffers, four Recordable signals, and the existing Recorder capacity limits."] =
                "Double-bank contract gate가 닫혀 있습니다. RecorderSingleBank, RecorderDoubleBank, 정확히 2개 buffer, 4개 Recordable signal과 기존 Recorder capacity 제한이 필요합니다.";
            values["Group Reset"] = "그룹 리셋";
            values[
                "Mutation journal ready. No unresolved durable write record."] =
                "Mutation journal 준비됨. 해결되지 않은 durable write record가 없습니다.";
            values["No external Bulk checkpoint is waiting."] =
                "대기 중인 외부 Bulk checkpoint가 없습니다.";
            values[
                "No unresolved Double-bank recovery record exists."] =
                "해결되지 않은 Double-bank 복구 record가 없습니다.";
            values[
                "SDO Read supports exact 1/2/4-byte typed values. Read SDO Inline waits for and displays the terminal typed/raw result in one action; Submit/Refresh remains available for low-level ticket diagnostics. Bit 13 enables editable nonzero object index and sub-index; a bit-8-only PLC uses fixed 0x1000:0 UInt32/4."] =
                "SDO Read는 정확한 1/2/4-byte typed 값을 지원합니다. Read SDO Inline은 한 번의 동작으로 terminal typed/raw 결과를 기다려 표시하며, 저수준 ticket 진단에는 Submit/Refresh를 계속 사용할 수 있습니다. Bit 13은 0이 아닌 object index/sub-index 편집을 허용하고 bit 8만 지원하는 PLC는 고정 0x1000:0 UInt32/4를 사용합니다.";
            values[
                "Write mode uses two-click confirmation after the current-session same-value activation proof. Read mode submits one tracked SDO Read."] =
                "Write mode는 현재 session의 same-value 활성화 증명 뒤 2단계 확인을 사용합니다. Read mode는 추적되는 SDO Read 하나를 전송합니다.";

            values["Home (explicit one-shot actions)"] =
                "Home (명시적 1회 실행)";
            values[
                "Both Home routes set the current coordinate to zero without physical motion, a Home switch, or a limit switch. Refresh capabilities and verify the exact stationary axis before the one-shot confirmation."] =
                "두 Home 방식 모두 실제 이동, Home switch, limit switch 없이 현재 좌표를 0으로 설정합니다. Capability를 새로고침하고 정확히 정지한 축을 확인한 뒤 1회 실행을 확인하십시오.";
            values["Refresh Home Capabilities"] =
                "Home Capability 새로고침";
            values["Read Home Status (no replay)"] =
                "Home 상태 읽기 (재전송 없음)";
            values["LMC Home - Current Position Zero (0x7D13)"] =
                "LMC Home - 현재 위치 0 설정 (0x7D13)";
            values[
                "The app reads the current actual position as an exact stale-read guard, then requests TargetPosition=0. Start acceptance is not completion; Read Home Status queries the retained 0x7D18 outcome and retires a terminal record with 0x7D19."] =
                "앱은 정확한 stale-read guard로 현재 actual position을 읽은 뒤 TargetPosition=0을 요청합니다. 시작 수락은 완료가 아니며 Home 상태 읽기는 보존된 0x7D18 결과를 조회하고 terminal record를 0x7D19로 폐기합니다.";
            values["Timeout (ms)"] = "Timeout (ms)";
            values["DS402 Home (0x7D15)"] = "DS402 Home (0x7D15)";
            values[
                "This route is fixed to DS402 method 37 and Home offset 0. Method 37 takes the current position as Home without enabling motion or seeking a switch; offset 0 makes the completed actual position zero."] =
                "이 방식은 DS402 method 37과 Home offset 0으로 고정됩니다. Method 37은 motion enable이나 switch 탐색 없이 현재 위치를 Home으로 사용하며 offset 0은 완료 actual position을 0으로 만듭니다.";
            values["Fixed semantic"] = "고정 의미";
            values["Method=37; HomeOffset=0; no motion"] =
                "Method=37; HomeOffset=0; 실제 이동 없음";
            values[
                "I verified the exact stationary axis, its current actual position, raw units, and the coordinate-zero effect. Send the selected Home route once only and never replay it after an uncertain result."] =
                "정확히 정지한 축, 현재 actual position, raw unit과 좌표 0 설정 효과를 확인했습니다. 선택한 Home 방식을 한 번만 전송하고 결과가 미확정이면 재전송하지 않습니다.";
            values["Execute LMC Home Once"] = "LMC Home 1회 실행";
            values["Execute DS402 Home Once"] = "DS402 Home 1회 실행";
            values["No Home action has been sent."] =
                "전송된 Home 동작이 없습니다.";
            values[
                "Opening the durable Home/encoder-maintenance recovery journal..."] =
                "Durable Home/encoder 유지보수 복구 journal을 여는 중입니다...";
            values[
                "No unresolved Home/encoder-maintenance recovery record. Commands still require live capability and explicit confirmation."] =
                "해결되지 않은 Home/encoder 유지보수 복구 record가 없습니다. 명령에는 현재 capability와 명시적 확인이 계속 필요합니다.";
            values[
                "I independently verified the physical axis/drive state and understand that resolving this record sends no command and does not prove the previous action result."] =
                "실제 축/drive 상태를 독립적으로 확인했으며 이 record를 resolve해도 명령은 전송되지 않고 이전 동작의 결과를 증명하지 않음을 이해했습니다.";
            values["Resolve Recovery Record (no command)"] =
                "복구 Record Resolve (명령 없음)";

            values[
                "TEST ONLY - Encoder Maintenance (TW[20] / TW[19])"] =
                "테스트 전용 - Encoder 유지보수 (TW[20] / TW[19])";
            values[
                "DESTRUCTIVE TEST ONLY: TW[20] writes UInt16 1 to 0x20FC:0x02. TW[19] writes UInt16 1 to 0x20FC:0x01, resets the absolute multi-turn position, and requires LMC Home before motion. Select the exact drive axis."] =
                "파괴적 테스트 전용: TW[20]은 0x20FC:0x02에 UInt16 1을 씁니다. TW[19]는 0x20FC:0x01에 UInt16 1을 써 absolute multi-turn position을 reset하며 motion 전에 LMC Home이 필요합니다. 정확한 drive 축을 선택하십시오.";
            values[
                "Before arming: power off the selected axis, verify stable standstill and physical position independently, and confirm support for the exact 0x20FC command. This dedicated 0x7E53/0x7E54/0x7E55 path is separate from generic SDO Write."] =
                "Arm 전: 선택한 축의 Power를 끄고 안정적인 standstill과 실제 위치를 독립적으로 확인한 뒤 정확한 0x20FC 명령 지원 여부를 확인하십시오. 전용 0x7E53/0x7E54/0x7E55 경로는 일반 SDO Write와 분리됩니다.";
            values["Operation"] = "작업";
            values["Wire schema profile (fixed 1)"] =
                "Wire schema profile (고정 1)";
            values["Drive / axis (1..4)"] = "Drive / 축 (1..4)";
            values["Wire schema socket (fixed 1)"] =
                "Wire schema socket (고정 1)";
            values["Timeout (ms, 1..60000)"] =
                "Timeout (ms, 1..60000)";
            values["Refresh Encoder Maintenance Capabilities"] =
                "Encoder 유지보수 Capability 새로고침";
            values["Read Encoder Maintenance Outcome"] =
                "Encoder 유지보수 결과 읽기";
            values[
                "Step 1A: I verified PowerOn=False and stable standstill for the selected physical axis."] =
                "1단계 A: 선택한 실제 축의 PowerOn=False와 안정적인 standstill을 확인했습니다.";
            values[
                "Step 1B: I recorded the physical position and will run the required Home procedure before motion."] =
                "1단계 B: 실제 위치를 기록했으며 motion 전에 필요한 Home 절차를 실행합니다.";
            values[
                "Step 1C: I verified the exact drive axis and the selected TW[20] or TW[19] destructive effect."] =
                "1단계 C: 정확한 drive 축과 선택한 TW[20] 또는 TW[19]의 파괴적 효과를 확인했습니다.";
            values[
                "Step 1D: I independently verified that the selected drive supports this exact 0x20FC maintenance command."] =
                "1단계 D: 선택한 drive가 이 정확한 0x20FC 유지보수 명령을 지원함을 독립적으로 확인했습니다.";
            values["Step 1 - Arm Encoder Maintenance"] =
                "1단계 - Encoder 유지보수 Arm";
            values[
                "Step 2: Execute this unchanged armed request once only."] =
                "2단계: 변경되지 않은 arm 요청을 한 번만 실행합니다.";
            values["Step 2 - Execute Encoder Maintenance Once"] =
                "2단계 - Encoder 유지보수 1회 실행";
            values[
                "No encoder maintenance operation is armed or pending."] =
                "Arm되거나 대기 중인 encoder 유지보수 작업이 없습니다.";
            values[
                "Requires live mutation admission, a current TW[20]/TW[19] capability, an available durable maintenance journal, and all four Step 1 confirmations. Arm prepares the exact request in PC memory only and sends no 0x7E53."] =
                "Live mutation 승인, 현재 TW[20]/TW[19] capability, 사용 가능한 durable 유지보수 journal 및 1단계의 네 가지 확인이 필요합니다. Arm은 정확한 요청을 PC 메모리에만 준비하며 0x7E53을 전송하지 않습니다.";
            values[
                "Requires the unchanged PC-only armed request, live mutation admission, current capability, an available durable maintenance journal, and Step 2 confirmation. Execute sends 0x7E53 once."] =
                "변경되지 않은 PC 전용 arm 요청, live mutation 승인, 현재 capability, 사용 가능한 durable 유지보수 journal 및 2단계 확인이 필요합니다. Execute는 0x7E53을 한 번 전송합니다.";
            values[
                "BLOCKED: connect to the PLC before arming encoder maintenance. No encoder-maintenance RPC was sent."] =
                "차단: Encoder 유지보수를 arm하기 전에 PLC에 연결하십시오. Encoder 유지보수 RPC를 전송하지 않았습니다.";
            values[
                "BLOCKED: wait for the current operation, safety action, monitor, or qualification to finish before arming encoder maintenance. No encoder-maintenance RPC was sent."] =
                "차단: Encoder 유지보수를 arm하기 전에 현재 작업, 안전 동작, monitor 또는 qualification이 끝날 때까지 기다리십시오. Encoder 유지보수 RPC를 전송하지 않았습니다.";
            values[
                "BLOCKED: recovery-identity read-only quarantine permits inspection only; encoder maintenance cannot be armed. Open Safety / Recovery Details, independently verify the machine and drive state, then archive and retire the stale record. No encoder-maintenance RPC was sent."] =
                "차단: 복구 ID 읽기 전용 격리에서는 확인만 허용되며 Encoder 유지보수를 arm할 수 없습니다. 안전 / 복구 상세 정보를 열고 기계와 drive 상태를 독립 확인한 뒤 오래된 record를 보관 및 폐기하십시오. Encoder 유지보수 RPC를 전송하지 않았습니다.";
            values["Open Safety / Recovery Details"] =
                "안전 / 복구 상세 정보 열기";
            values[
                "Expands the local safety/recovery panel only. This button sends no PLC command and does not retire any record."] =
                "로컬 안전/복구 panel만 펼칩니다. 이 버튼은 PLC 명령을 전송하지 않으며 어떤 record도 폐기하지 않습니다.";
            values[
                "BLOCKED: the durable Home/encoder-maintenance recovery journal is unavailable. Encoder maintenance remains fail-closed; no RPC was sent."] =
                "차단: Durable Home/Encoder 유지보수 복구 journal을 사용할 수 없습니다. Encoder 유지보수는 계속 fail-closed이며 RPC를 전송하지 않았습니다.";
            values[
                "BLOCKED: an unresolved Home/encoder-maintenance recovery record is active. Use its exact outcome/status recovery path; do not replay it. No encoder-maintenance RPC was sent."] =
                "차단: 미해결 Home/Encoder 유지보수 복구 record가 활성 상태입니다. 정확한 outcome/status 복구 경로를 사용하고 재전송하지 마십시오. Encoder 유지보수 RPC를 전송하지 않았습니다.";
            values[
                "BLOCKED: another unresolved mutation, recovery, unavailable journal, or possible-motion state prevents encoder-maintenance arming. Resolve it first; no encoder-maintenance RPC was sent."] =
                "차단: 다른 미해결 mutation, recovery, 사용할 수 없는 journal 또는 motion 가능 상태로 인해 Encoder 유지보수를 arm할 수 없습니다. 먼저 해당 상태를 해결하십시오. Encoder 유지보수 RPC를 전송하지 않았습니다.";
            values[
                "BLOCKED: refresh current-session Diagnostics capabilities and identity before arming encoder maintenance. No encoder-maintenance RPC was sent."] =
                "차단: Encoder 유지보수를 arm하기 전에 현재 session의 Diagnostics capability와 identity를 새로고침하십시오. Encoder 유지보수 RPC를 전송하지 않았습니다.";
            values[
                "BLOCKED: the current PLC does not advertise the selected TW[20]/TW[19] encoder-maintenance capability. No encoder-maintenance RPC was sent."] =
                "차단: 현재 PLC가 선택된 TW[20]/TW[19] Encoder 유지보수 capability를 advertise하지 않습니다. Encoder 유지보수 RPC를 전송하지 않았습니다.";
            values[
                "ARMED: the exact encoder-maintenance request is held in PC memory only. Verify it and select Step 2 to permit one 0x7E53 submission."] =
                "ARMED: 정확한 Encoder 유지보수 요청은 PC 메모리에만 보관됩니다. 요청을 확인하고 0x7E53 1회 전송을 허용하려면 2단계를 선택하십시오.";
            values[
                "READY: current-session capability, recovery, and all Step 1 gates are open. Arm performs fresh read-only checks and prepares the request in PC memory; it does not send 0x7E53/0x7E54/0x7E55."] =
                "준비됨: 현재 session의 capability, recovery 및 모든 1단계 gate가 열렸습니다. Arm은 최신 읽기 전용 확인 후 요청을 PC 메모리에 준비하며 0x7E53/0x7E54/0x7E55를 전송하지 않습니다.";
            values[
                "BLOCKED: an exact encoder-maintenance request is already armed in PC memory. Execute or change the input to clear it; no RPC was sent."] =
                "차단: 정확한 Encoder 유지보수 요청이 이미 PC 메모리에 arm되어 있습니다. Execute하거나 입력을 변경해 지우십시오. RPC를 전송하지 않았습니다.";
            values["Capabilities refreshed."] =
                "Capability를 새로고침했습니다.";
            values[
                "BLOCKED: explicit one-shot Home confirmation is required. No RPC was sent."] =
                "차단: 명시적 Home 1회 실행 확인이 필요합니다. RPC를 전송하지 않았습니다.";
            values["LMC Home Start=Accepted; Outcome=NotQueried"] =
                "LMC Home 시작=수락됨; 결과=조회 전";
            values[
                "Read Home Status performs the exact 0x7D18 outcome query; the start acknowledgement is not completion proof."] =
                "Home 상태 읽기는 정확한 0x7D18 결과 조회를 실행합니다. 시작 ACK는 완료 증거가 아닙니다.";
            values["DS402 Home Start=Accepted; Outcome=NotQueried"] =
                "DS402 Home 시작=수락됨; 결과=조회 전";
            values[
                "Read Home Status performs the exact 0x7D16 outcome query; IsReferenced alone is not completion proof."] =
                "Home 상태 읽기는 정확한 0x7D16 결과 조회를 실행합니다. IsReferenced만으로는 완료 증거가 아닙니다.";
            values["Home status read-only sample"] =
                "Home 상태 읽기 전용 sample";
            values[
                "This sample does not replay Home and does not by itself prove LMC or DS402 Home completion."] =
                "이 sample은 Home을 재전송하지 않으며 그 자체로 LMC 또는 DS402 Home 완료를 증명하지 않습니다.";
            values["LMC Home Start=Accepted; Outcome="] =
                "LMC Home 시작=수락됨; 결과=";
            values["DS402 Home Start=Accepted; Outcome="] =
                "DS402 Home 시작=수락됨; 결과=";
            values[
                "Running is not completion evidence. The durable record remains active and no Home is replayed."] =
                "Running은 완료 증거가 아닙니다. Durable record를 활성 상태로 유지하며 Home을 재전송하지 않습니다.";
            values[
                "Exact terminal 0x7D16 outcome and matching 0x7D17 retirement verified. The non-moving Home no-replay record was resolved."] =
                "정확한 terminal 0x7D16 결과와 일치하는 0x7D17 retirement를 확인했습니다. 이동 없는 Home 재전송 금지 record를 resolve했습니다.";
            values[
                "Exact terminal 0x7D18 outcome and matching 0x7D19 retirement verified. The LMC Home no-replay record was resolved."] =
                "정확한 terminal 0x7D18 결과와 일치하는 0x7D19 retirement를 확인했습니다. LMC Home 재전송 금지 record를 resolve했습니다.";
            values[
                "BLOCKED: all Step 1 physical and encoder compatibility checks are required. No encoder-maintenance operation was armed or sent."] =
                "차단: 1단계의 모든 물리 확인과 encoder 호환성 확인이 필요합니다. Encoder 유지보수 작업을 arm하거나 전송하지 않았습니다.";
            values[
                "Step 1 armed in PC memory only; no encoder-maintenance RPC was sent."] =
                "1단계는 PC 메모리에만 arm했습니다. Encoder 유지보수 RPC를 전송하지 않았습니다.";
            values[
                "Select Step 2 confirmation to execute this exact request once."] =
                "이 정확한 요청을 한 번 실행하려면 2단계 확인을 선택하십시오.";
            values[
                "BLOCKED: complete every Step 1 check and the final Step 2 confirmation. No encoder-maintenance RPC was sent."] =
                "차단: 모든 1단계 확인과 최종 2단계 확인을 완료하십시오. Encoder 유지보수 RPC를 전송하지 않았습니다.";
            values[
                "Encoder Maintenance Start=Accepted; Outcome=NotQueried"] =
                "Encoder 유지보수 시작=수락됨; 결과=조회 전";
            values[
                "Use Read Encoder Maintenance Outcome. Start acceptance is not completion proof and the prepared command must never be replayed."] =
                "Encoder 유지보수 결과 읽기를 사용하십시오. 시작 수락은 완료 증거가 아니며 준비된 명령을 절대 재전송하면 안 됩니다.";
            values["Encoder Maintenance Outcome"] =
                "Encoder 유지보수 결과";
            values[
                "Running is not completion evidence. The durable encoder-maintenance record remains active and no command is replayed."] =
                "Running은 완료 증거가 아닙니다. Durable encoder 유지보수 record를 활성 상태로 유지하고 명령을 재전송하지 않습니다.";
            values[
                "Exact terminal encoder-maintenance outcome and matching retirement verified. The no-replay record was resolved."] =
                "정확한 terminal encoder 유지보수 결과와 일치하는 retirement를 확인했습니다. 재전송 금지 record를 resolve했습니다.";
            values[
                "PLC terminal proves the exact SDO write completion and cleanup, not the physical encoder effect. Verify the effect independently before further operation."] =
                "PLC terminal은 정확한 SDO write 완료와 cleanup만 증명하며 실제 encoder 효과는 증명하지 않습니다. 추가 동작 전에 효과를 독립적으로 확인하십시오.";
            values[
                "TW[19] position reset requires successful LMC Home current-position-zero before any subsequent motion."] =
                "TW[19] position reset 뒤에는 다음 motion 전에 LMC Home 현재 위치 0 설정이 성공해야 합니다.";
            values[
                "Recovery record resolved without sending a command. Last read-only status: IsReferenced="] =
                "명령을 전송하지 않고 복구 record를 resolve했습니다. 마지막 읽기 전용 상태: IsReferenced=";
            values[
                "Recovery record resolved without command replay. Physical state and Home remain separate checks."] =
                "명령 재전송 없이 복구 record를 resolve했습니다. 실제 상태와 Home은 별도 확인으로 남습니다.";
            values[
                "SAFETY: Home/encoder-maintenance recovery journal unavailable. No Home or encoder-maintenance command is permitted. "] =
                "안전: Home/encoder 유지보수 복구 journal을 사용할 수 없습니다. Home 또는 encoder 유지보수 명령을 허용하지 않습니다. ";
            values["SAFETY: NO-REPLAY RECOVERY ACTIVE. Action="] =
                "안전: 재전송 금지 복구 활성. 동작=";
            values[
                "The encoder-maintenance input changed; repeat every Step 1 check and arm the exact request again."] =
                "Encoder 유지보수 입력이 변경되었습니다. 모든 1단계 확인을 반복하고 정확한 요청을 다시 arm하십시오.";
            values[
                "SAFETY: HOME/ENCODER-MAINTENANCE NO-REPLAY QUARANTINE. "] =
                "안전: HOME/ENCODER-MAINTENANCE 재전송 금지 격리. ";
            values[
                "The durable recovery journal is unavailable; Home and encoder-maintenance commands remain blocked."] =
                "Durable 복구 journal을 사용할 수 없어 Home과 encoder 유지보수 명령을 계속 차단합니다.";
            values["Unresolved action="] = "미해결 동작=";
            values["axis="] = "축=";
            values[
                "Do not replay it. Use exact outcome/status evidence or explicit physical operator retirement as permitted by the action contract."] =
                "재전송하지 마십시오. 동작 contract가 허용하는 정확한 결과/상태 증거 또는 명시적 물리 작업자 폐기를 사용하십시오.";
            values[
                "DS402 Home requires Read Home Status to obtain the exact terminal 0x7D16 outcome; manual record resolution is disabled."] =
                "DS402 Home은 정확한 terminal 0x7D16 결과를 얻기 위해 Home 상태 읽기가 필요하며 수동 record resolve는 비활성화됩니다.";
            values[
                "LMC Home requires Read Home Status to obtain the exact terminal 0x7D18 outcome; manual record resolution is disabled."] =
                "LMC Home은 정확한 terminal 0x7D18 결과를 얻기 위해 Home 상태 읽기가 필요하며 수동 record resolve는 비활성화됩니다.";
            values[
                "Encoder maintenance requires Read Encoder Maintenance Outcome and exact terminal retirement; manual record resolution is disabled."] =
                "Encoder 유지보수는 Encoder 유지보수 결과 읽기와 정확한 terminal retirement가 필요하며 수동 record resolve는 비활성화됩니다.";
        }
    }

    internal static class UiLocalizationService
    {
        internal const string PreserveTextTag = "UiLocalization.Preserve";

        private static readonly ConditionalWeakTable<DependencyObject,
            ElementTranslationState> States =
                new ConditionalWeakTable<DependencyObject,
                    ElementTranslationState>();

        internal static void Apply(DependencyObject root, UiLanguage language)
        {
            if (root == null)
            {
                return;
            }

            ApplyNode(root, language);
            foreach (var child in LogicalTreeHelper.GetChildren(root))
            {
                var childObject = child as DependencyObject;
                if (childObject != null)
                {
                    Apply(childObject, language);
                }
            }
        }

        private static void ApplyNode(
            DependencyObject target,
            UiLanguage language)
        {
            var state = States.GetOrCreateValue(target);
            var window = target as Window;
            if (window != null)
            {
                ApplyString(
                    () => window.Title,
                    value => window.Title = value,
                    state.Title,
                    language);
            }

            var headeredContent = target as HeaderedContentControl;
            if (headeredContent != null)
            {
                ApplyString(
                    () => headeredContent.Header,
                    value => headeredContent.Header = value,
                    state.Header,
                    language);
            }

            var headeredItems = target as HeaderedItemsControl;
            if (headeredItems != null)
            {
                ApplyString(
                    () => headeredItems.Header,
                    value => headeredItems.Header = value,
                    state.Header,
                    language);
            }

            var content = target as ContentControl;
            if (content != null)
            {
                ApplyString(
                    () => content.Content,
                    value => content.Content = value,
                    state.Content,
                    language);
            }

            var textBlock = target as TextBlock;
            if (textBlock != null
                && !System.Windows.Data.BindingOperations.IsDataBound(
                    textBlock,
                    TextBlock.TextProperty)
                && !string.Equals(
                    textBlock.Tag as string,
                    PreserveTextTag,
                    StringComparison.Ordinal))
            {
                ApplyString(
                    () => textBlock.Text,
                    value => textBlock.Text = value,
                    state.Text,
                    language);
            }

            var frameworkElement = target as FrameworkElement;
            if (frameworkElement != null)
            {
                ApplyString(
                    () => frameworkElement.ToolTip,
                    value => frameworkElement.ToolTip = value,
                    state.ToolTip,
                    language);
            }

            var dataGrid = target as DataGrid;
            if (dataGrid != null)
            {
                foreach (var column in dataGrid.Columns)
                {
                    var columnState = States.GetOrCreateValue(column);
                    ApplyString(
                        () => column.Header,
                        value => column.Header = value,
                        columnState.Header,
                        language);
                }
            }
        }

        private static void ApplyString(
            Func<object> read,
            Action<string> write,
            PropertyTranslationState state,
            UiLanguage language)
        {
            var current = read() as string;
            if (current == null)
            {
                return;
            }

            if (state.Source == null)
            {
                state.Source = current;
            }
            else if (!string.Equals(
                    current,
                    state.LastApplied,
                    StringComparison.Ordinal)
                && !string.Equals(
                    current,
                    state.Source,
                    StringComparison.Ordinal))
            {
                state.Source = current;
            }

            var translated = UiLocalizationCatalog.Translate(
                state.Source,
                language);
            if (!string.Equals(current, translated, StringComparison.Ordinal))
            {
                write(translated);
            }
            state.LastApplied = translated;
        }

        private sealed class ElementTranslationState
        {
            internal readonly PropertyTranslationState Title =
                new PropertyTranslationState();
            internal readonly PropertyTranslationState Header =
                new PropertyTranslationState();
            internal readonly PropertyTranslationState Content =
                new PropertyTranslationState();
            internal readonly PropertyTranslationState Text =
                new PropertyTranslationState();
            internal readonly PropertyTranslationState ToolTip =
                new PropertyTranslationState();
        }

        private sealed class PropertyTranslationState
        {
            internal string Source;
            internal string LastApplied;
        }
    }
}
