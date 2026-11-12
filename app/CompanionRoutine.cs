using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace CapriDesktop
{
    // Cheap attention gates. No polling service or model inference is needed.
    internal sealed class CompanionRoutine
    {
        private readonly string path;
        private readonly object gate = new object();
        private RoutineData data = new RoutineData { Schema = 1 };
        private bool unreadable;
        internal Func<DateTime> Now = delegate { return DateTime.Now; };

        internal CompanionRoutine(string directory)
        {
            path = Path.Combine(directory, "routine.json");
            if (!File.Exists(path)) return;
            try
            {
                using (FileStream input = File.OpenRead(path))
                {
                    if (input.Length > 8192) throw new InvalidDataException();
                    data = (RoutineData)new DataContractJsonSerializer(typeof(RoutineData)).ReadObject(input);
                }
                if (data == null || data.Schema != 1 || data.Start < 0 || data.Start >= 1440 || data.End < 0 || data.End >= 1440 || data.Hours && data.Start == data.End)
                    throw new InvalidDataException();
            }
            catch (Exception) { unreadable = true; }
        }

        internal bool QuietNow
        {
            get
            {
                lock (gate)
                {
                    if (unreadable) return true;
                    if (!data.Hours) return false;
                    DateTime now = Now();
                    int minute = now.Hour * 60 + now.Minute;
                    return data.Start < data.End ? minute >= data.Start && minute < data.End : minute >= data.Start || minute < data.End;
                }
            }
        }
        internal bool LowResource { get { lock (gate) return unreadable || data.LowResource; } }
        internal bool Paused { get { return LowResource || QuietNow; } }
        internal string Status
        {
            get
            {
                lock (gate)
                {
                    if (unreadable) return "Autonomous activity is paused: routine.json is invalid or from a newer version. Preserve it and restore a compatible routine.json.previous backup while the app is closed.";
                    return "Quiet hours: " + (data.Hours ? Clock(data.Start) + "-" + Clock(data.End) + " (this PC's local time)." : "off.") + (QuietNow ? " Autonomous interactions are paused now." : "") + (data.LowResource ? " Performance mode is on." : "");
                }
            }
        }
        private static string Clock(int minute) { return (minute / 60).ToString("00", CultureInfo.InvariantCulture) + ":" + (minute % 60).ToString("00", CultureInfo.InvariantCulture); }
        private static bool Minute(string text, out int minute)
        {
            DateTime value;
            bool valid = DateTime.TryParseExact(text, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out value);
            minute = valid ? value.Hour * 60 + value.Minute : 0;
            return valid;
        }
        internal string Command(string text, bool performance)
        {
            string command = (text ?? "").Trim().ToLowerInvariant();
            bool resource = performance && (command == "/performance on" || command == "/performance off");
            if (!resource && command != "/quiet" && !command.StartsWith("/quiet ", StringComparison.Ordinal)) return null;
            lock (gate)
            {
                if (unreadable || command == "/quiet") return Status;
                RoutineData next = new RoutineData { Schema = 1, Hours = data.Hours, Start = data.Start, End = data.End, LowResource = data.LowResource };
                if (resource) next.LowResource = command == "/performance on";
                else if (command == "/quiet hours off") next.Hours = false;
                else if (command.StartsWith("/quiet hours ", StringComparison.Ordinal))
                {
                    string[] range = command.Substring(13).Split('-');
                    if (range.Length != 2 || !Minute(range[0], out next.Start) || !Minute(range[1], out next.End) || next.Start == next.End)
                        return "Use /quiet hours HH:mm-HH:mm in local 24-hour time, with different start/end times, or /quiet hours off.";
                    next.Hours = true;
                }
                else return "Use /quiet, /quiet hours HH:mm-HH:mm or /quiet hours off.";
                string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    using (FileStream output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    {
                        new DataContractJsonSerializer(typeof(RoutineData)).WriteObject(output, next);
                        output.Flush(true);
                    }
                    if (File.Exists(path)) File.Replace(temporary, path, path + ".previous");
                    else File.Move(temporary, path);
                    data = next;
                    return Status;
                }
                catch (Exception error) { return "Routine settings were not changed: " + error.GetType().Name + "."; }
                finally { try { if (File.Exists(temporary)) File.Delete(temporary); } catch (IOException) { } catch (UnauthorizedAccessException) { } }
            }
        }
    }
    [DataContract] internal sealed class RoutineData
    {
        [DataMember(IsRequired = true)] public int Schema;
        [DataMember] public bool Hours;
        [DataMember] public int Start;
        [DataMember] public int End;
        [DataMember] public bool LowResource;
    }
}
