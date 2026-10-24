using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using Microsoft.Win32;
using DualDiscord;

public sealed class Options {
    public bool StartApp { get; set; }
    public bool StartSecond { get; set; }
    public bool SyncDiscord { get; set; }
    public bool SyncVencord { get; set; }
}
public sealed class Proc {
    public int Id, Parent; public string Command, Exe;
}
public sealed class Instance {
    public bool Second; public int Count; public long Memory; public List<Proc> Processes = new List<Proc>(); public List<Proc> Roots = new List<Proc>();
}
public static class Engine {
    public static readonly string Home = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
    public static readonly string ProfileRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Discord Duo", "Second Account");
    public static readonly string SecondData = Path.Combine(ProfileRoot, "discord");
    public static readonly string MainData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "discord");
    public static readonly string MainVencord = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Vencord");
    public static readonly string PrivateVencord = Path.Combine(Home, "Data", "Vencord");
    public static readonly string StandardDiscordRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Discord");
    public static InstallationSettings Connection = new InstallationSettings();
    public static string DiscordRoot { get { return String.IsNullOrEmpty(Connection.InstallRoot) ? StandardDiscordRoot : Connection.InstallRoot; } }
    static readonly JavaScriptSerializer Json = new JavaScriptSerializer { MaxJsonLength = 10000000 };
    public static Options Settings;
    static readonly string[] SyncKeys = { "audioSubsystem", "useLegacyAudioDevice", "BACKGROUND_COLOR", "offloadAdmControls", "enableHardwareAcceleration", "asyncVideoInputDeviceInit", "enableH264MFElectron", "openH264Enabled" };
    delegate bool EnumCallback(IntPtr window, IntPtr data);
    [DllImport("user32.dll")] static extern bool EnumWindows(EnumCallback callback, IntPtr data);
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr window, out uint pid);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern int GetWindowText(IntPtr window, StringBuilder title, int count);
    [DllImport("user32.dll")] static extern bool PostMessage(IntPtr window, uint message, IntPtr w, IntPtr l);
    public static void Init() {
        foreach (string folder in new[] { "Settings", "Logs", "Data", "Backups" }) Directory.CreateDirectory(Path.Combine(Home, folder));
        string file = Path.Combine(Home, "Settings", "app.json");
        Settings = File.Exists(file) ? Json.Deserialize<Options>(File.ReadAllText(file)) : new Options();
        if (Settings == null) throw new Exception("App settings are unreadable.");
        string installationFile=Path.Combine(Home,"Settings","installation.json");
        Connection=Installation.Load(installationFile);
        if(Installation.FindExecutable(Connection.InstallRoot)==null)DetectInstallation();
        Installation.PrepareSecond(ProfileRoot,PrivateVencord);
        if (!File.Exists(file)) Save(false);
    }
    public static void CopyFolder(string source, string target) {
        if (!Directory.Exists(source)) return;
        Directory.CreateDirectory(target);
        foreach (string file in Directory.GetFiles(source)) File.Copy(file, Path.Combine(target, Path.GetFileName(file)), true);
        foreach (string folder in Directory.GetDirectories(source)) {
            if ((File.GetAttributes(folder) & FileAttributes.ReparsePoint) != 0) continue;
            CopyFolder(folder, Path.Combine(target, Path.GetFileName(folder)));
        }
    }
    public static void Save(bool applyStartup = true) {
        AtomicWrite(Path.Combine(Home, "Settings", "app.json"), Json.Serialize(Settings));
        if(applyStartup) using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run")) {
            if (Settings.StartApp) key.SetValue("Dual Discord", "\"" + Path.Combine(Home, "Dual Discord.exe") + "\""); else key.DeleteValue("Dual Discord", false);
            if (Settings.StartSecond) key.SetValue("Dual Discord Second", "\"" + Path.Combine(Home, "Dual Discord.exe") + "\" --second --startup"); else key.DeleteValue("Dual Discord Second", false);
        }
    }
    static void AtomicWrite(string file, string text) {
        Directory.CreateDirectory(Path.GetDirectoryName(file)); string temp = file + ".tmp";
        // Discord can encrypt its settings file with EFS. Keep that file and its
        // encryption metadata instead of replacing it with an unencrypted inode.
        if (File.Exists(file) && (File.GetAttributes(file) & FileAttributes.Encrypted) != 0) {
            File.WriteAllText(file, text, new UTF8Encoding(false)); return;
        }
        File.WriteAllText(temp, text, new UTF8Encoding(false));
        if (File.Exists(file)) File.Replace(temp, file, null); else File.Move(temp, file);
    }
    public static string FindDiscord() {
        string exe=Installation.FindExecutable(DiscordRoot);
        if(exe==null)throw new IOException("Discord was not found. Use Detect or Locate Discord in Settings.");
        return exe;
    }
    public static bool DetectInstallation(){
        string selected=Installation.Discover(StandardDiscordRoot,Snapshot().Select(p=>p.Exe));
        if(selected==null)return false;
        SelectInstallation(selected);return true;
    }
    public static void SelectInstallation(string exe){
        string found=Installation.FindExecutable(exe);if(found==null)throw new IOException("Select the installed stable Discord.exe.");
        string root=Installation.RootOf(found);
        if(!Installation.Physical(root).Equals(Installation.Physical(DiscordRoot),StringComparison.OrdinalIgnoreCase)){
            var all=Snapshot();if(GetInstance(false,all).Count>0||GetInstance(true,all).Count>0)throw new IOException("Close both Discord profiles before choosing a different installation.");
        }
        var value=new InstallationSettings{InstallRoot=root};Installation.Save(Path.Combine(Home,"Settings","installation.json"),value);Connection=value;
    }
    public static bool VencordAvailable(){try{string file=Path.Combine(Path.GetDirectoryName(FindDiscord()),"resources","app.asar");return File.Exists(file)&&new FileInfo(file).Length<65536&&File.ReadAllText(file).IndexOf("vencord",StringComparison.OrdinalIgnoreCase)>=0&&Directory.Exists(MainVencord);}catch{return false;}}
    static Version VersionOf(string value) { Version version; return Version.TryParse(value, out version) ? version : new Version(0, 0); }
    public static List<Proc> Snapshot() {
        var result = new List<Proc>();
        using (var query = new ManagementObjectSearcher("SELECT ProcessId, ParentProcessId, CommandLine, ExecutablePath FROM Win32_Process WHERE Name='Discord.exe'")) {
            foreach (ManagementObject row in query.Get()) using (row) result.Add(new Proc { Id = Convert.ToInt32(row["ProcessId"]), Parent = Convert.ToInt32(row["ParentProcessId"]), Command = Convert.ToString(row["CommandLine"]), Exe = Convert.ToString(row["ExecutablePath"]) });
        }
        return result;
    }
    static bool PathMatches(string command, string path) {
        int pos = command.IndexOf(path, StringComparison.OrdinalIgnoreCase);
        if (pos < 0) return false;
        int end = pos + path.Length;
        return end == command.Length || command[end] == '"' || command[end] == ' ' || command[end] == '\\';
    }
    public static Instance GetInstance(bool second, List<Proc> all) {
        string path = second ? SecondData : MainData;
        var result = new Instance { Second = second };
        var rootIds = new HashSet<int>(all.Where(p => PathMatches(p.Command, path) && p.Command.Contains("--type=")).Select(p => p.Parent));
        if (second) foreach (var p in all.Where(p => p.Command.Contains("--dual-discord-second") && !p.Command.Contains("--type="))) rootIds.Add(p.Id);
        result.Roots = all.Where(p => rootIds.Contains(p.Id) && !p.Command.Contains("--type=") && Installation.IsUnder(p.Exe,DiscordRoot)).ToList();
        var ids = new HashSet<int>(result.Roots.Select(p => p.Id));
        bool changed = true;
        while (changed) { changed = false; foreach (var p in all) if (ids.Contains(p.Parent) && ids.Add(p.Id)) changed = true; }
        foreach (var p in all.Where(p => PathMatches(p.Command, path))) ids.Add(p.Id);
        result.Processes = all.Where(p => ids.Contains(p.Id)).ToList(); result.Count = result.Processes.Count;
        foreach (var p in result.Processes) try { using (var proc = Process.GetProcessById(p.Id)) result.Memory += proc.WorkingSet64; } catch (ArgumentException) { }
        return result;
    }
    public static void Launch(bool second, bool startup = false) {
        if (second && startup && GetInstance(true, Snapshot()).Count > 0) return;
        if (second && GetInstance(true, Snapshot()).Count == 0) {
            if (Settings.SyncDiscord) SyncFromMain();
            SetClosePolicy();
        }
        string exe = FindDiscord(); var info = new ProcessStartInfo(exe) { UseShellExecute = false, WorkingDirectory = Path.GetDirectoryName(exe) };
        if (second) {
            info.EnvironmentVariables["DISCORD_USER_DATA_DIR"] = ProfileRoot;
            info.EnvironmentVariables["VENCORD_USER_DATA_DIR"] = Settings.SyncVencord ? MainVencord : PrivateVencord;
            info.Arguments = "--dual-discord-second" + (startup ? " --start-minimized --start-inactive" : "");
        } else { info.EnvironmentVariables.Remove("DISCORD_USER_DATA_DIR"); info.EnvironmentVariables.Remove("VENCORD_USER_DATA_DIR"); }
        Process.Start(info); Log(second ? "Opened second Discord" : "Opened main Discord");
    }
    public static void SetClosePolicy() {
        string file = Path.Combine(SecondData, "settings.json");
        var values = File.Exists(file) ? Json.Deserialize<Dictionary<string, object>>(File.ReadAllText(file)) : new Dictionary<string, object>();
        values["MINIMIZE_TO_TRAY"] = true; values["START_MINIMIZED"] = false; values["OPEN_ON_STARTUP"] = false;
        AtomicWrite(file, Json.Serialize(values));
    }
    public static void SyncFromMain() {
        string source = Path.Combine(MainData, "settings.json"), target = Path.Combine(SecondData, "settings.json");
        if (!File.Exists(source)) return;
        var main = Json.Deserialize<Dictionary<string, object>>(File.ReadAllText(source));
        var second = File.Exists(target) ? Json.Deserialize<Dictionary<string, object>>(File.ReadAllText(target)) : new Dictionary<string, object>();
        foreach (string key in SyncKeys) if (main.ContainsKey(key)) second[key] = main[key];
        AtomicWrite(target, Json.Serialize(second));
        Log("Copied supported local Discord preferences from main to second");
    }
    public static string Quit(bool second, bool force = false) {
        var target = GetInstance(second, Snapshot());
        if (target.Count == 0) return "Already closed";
        var handles = new List<Process>();
        foreach (var item in target.Processes) try {
            var process = Process.GetProcessById(item.Id);
            if (!process.MainModule.FileName.Equals(item.Exe, StringComparison.OrdinalIgnoreCase)) { process.Dispose(); continue; }
            handles.Add(process);
        } catch (ArgumentException) { }
        try {
            if (!force) foreach (var root in target.Roots) EnumWindows(delegate(IntPtr window, IntPtr data) {
                uint pid; GetWindowThreadProcessId(window, out pid); if (pid != root.Id) return true;
                var title = new StringBuilder(512); GetWindowText(window, title, title.Capacity);
                if (title.ToString().EndsWith(" - Discord", StringComparison.OrdinalIgnoreCase)) PostMessage(window, 0x0010, IntPtr.Zero, IntPtr.Zero);
                return true;
            }, IntPtr.Zero);
            var clock = Stopwatch.StartNew();
            while (!force && clock.ElapsedMilliseconds < 2000 && handles.Any(p => !p.HasExited)) Thread.Sleep(100);
            bool forced = false;
            foreach (var root in target.Roots) {
                var process = handles.FirstOrDefault(p => p.Id == root.Id);
                if (process != null && !process.HasExited) { process.Kill(); forced = true; }
            }
            foreach (var process in handles) if (!process.HasExited) { try { process.Kill(); forced = true; } catch (InvalidOperationException) { } }
            foreach (var process in handles) process.WaitForExit(3000);
            if (GetInstance(second, Snapshot()).Count != 0) throw new Exception("Some selected Discord processes remain. Try Quit again.");
            Log((second ? "Second" : "Main") + " Discord fully exited" + (forced ? " (targeted cleanup was needed)" : ""));
            return "Fully closed · 0 processes";
        } finally { foreach (var process in handles) process.Dispose(); }
    }
    public static void SetSharing(bool discord, bool enabled) {
        if (GetInstance(true, Snapshot()).Count > 0) throw new Exception("Quit the second Discord before changing sharing.");
        if (discord) Settings.SyncDiscord = enabled; else Settings.SyncVencord = enabled;
        Save(); Log((discord ? "Discord preference sync" : "Vencord sharing") + (enabled ? " enabled" : " disabled"));
    }
    public static void Log(string text) { File.AppendAllText(Path.Combine(Home, "Logs", "activity.log"), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + text + Environment.NewLine); }
}
