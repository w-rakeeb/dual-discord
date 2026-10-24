using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.Script.Serialization;
using Microsoft.Win32.SafeHandles;

namespace DualDiscord {
 public sealed class InstallationSettings {public string InstallRoot{get;set;}}
 public static class Installation {
  public static string FindExecutable(string selected){
   if(String.IsNullOrWhiteSpace(selected))return null;
   string folder=Path.GetFullPath(selected);
   if(File.Exists(folder)){if(!Path.GetFileName(folder).Equals("Discord.exe",StringComparison.OrdinalIgnoreCase))return null;folder=Path.GetDirectoryName(folder);}
   if(Path.GetFileName(folder).StartsWith("app-",StringComparison.OrdinalIgnoreCase))folder=Path.GetDirectoryName(folder);
   if(!Directory.Exists(folder))return null;
   var versions=Directory.GetDirectories(folder,"app-*").Select(p=>new{Path=p,Version=VersionOf(Path.GetFileName(p).Substring(4))}).Where(p=>Complete(p.Path)).OrderByDescending(p=>p.Version).ToArray();
   if(versions.Length>0)return Path.Combine(versions[0].Path,"Discord.exe");
   string flat=Path.Combine(folder,"Discord.exe");return Complete(folder)?flat:null;
  }
  static bool Complete(string folder){string resources=Path.Combine(folder,"resources","app.asar");return File.Exists(Path.Combine(folder,"Discord.exe"))&&File.Exists(resources)&&new FileInfo(resources).Length>0;}
  static Version VersionOf(string text){Version v;return Version.TryParse(text,out v)?v:new Version(0,0);}
  public static string RootOf(string exe){string folder=Path.GetDirectoryName(Path.GetFullPath(exe));return Path.GetFileName(folder).StartsWith("app-",StringComparison.OrdinalIgnoreCase)?Path.GetDirectoryName(folder):folder;}
  public static string Discover(string standard,IEnumerable<string> running){foreach(string candidate in new[]{standard}.Concat(running??new string[0]).Where(p=>!String.IsNullOrWhiteSpace(p)).Distinct(StringComparer.OrdinalIgnoreCase))try{string exe=FindExecutable(candidate);if(exe!=null)return exe;}catch(IOException){}catch(UnauthorizedAccessException){}catch(ArgumentException){}return null;}
  public static InstallationSettings Load(string file){if(!File.Exists(file))return new InstallationSettings();var value=new JavaScriptSerializer().Deserialize<InstallationSettings>(File.ReadAllText(file));if(value==null)throw new IOException("Installation settings are unreadable.");return value;}
  public static void PrepareSecond(string root,string privateVencord){Directory.CreateDirectory(Path.Combine(root,"discord"));Directory.CreateDirectory(privateVencord);}
  public static void Save(string file,InstallationSettings settings){Directory.CreateDirectory(Path.GetDirectoryName(file));string temp=file+".tmp";File.WriteAllText(temp,new JavaScriptSerializer().Serialize(settings),new UTF8Encoding(false));if(File.Exists(file))File.Replace(temp,file,file+".previous");else File.Move(temp,file);}
  [DllImport("kernel32.dll",CharSet=CharSet.Unicode)]static extern SafeFileHandle CreateFile(string path,uint access,uint share,IntPtr security,uint mode,uint flags,IntPtr template);
  [DllImport("kernel32.dll",CharSet=CharSet.Unicode)]static extern uint GetFinalPathNameByHandle(SafeFileHandle handle,StringBuilder value,uint length,uint flags);
  public static string Physical(string path){string full=Path.GetFullPath(path).TrimEnd('\\');using(var handle=CreateFile(full,0,7,IntPtr.Zero,3,0x02000000,IntPtr.Zero)){if(handle.IsInvalid)return full;var result=new StringBuilder(32768);uint n=GetFinalPathNameByHandle(handle,result,(uint)result.Capacity,0);if(n==0||n>=result.Capacity)return full;string value=result.ToString();return value.StartsWith("\\\\?\\")?value.Substring(4).TrimEnd('\\'):value.TrimEnd('\\');}}
  public static bool IsUnder(string path,string root){return !String.IsNullOrWhiteSpace(path)&&Physical(path).StartsWith(Physical(root)+"\\",StringComparison.OrdinalIgnoreCase);}
 }
}
