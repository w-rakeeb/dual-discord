using System;
using System.IO;
using System.Runtime.InteropServices;
namespace DualDiscord {
 public static class DesktopShortcut {
  public static string Create(string home,string desktop){
   Branding.Ensure(home);Directory.CreateDirectory(desktop);string path=Path.Combine(desktop,"Dualcord.lnk");
   object instance=Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell")),link=null;
   try{dynamic shell=instance;link=shell.CreateShortcut(path);dynamic shortcut=link;shortcut.TargetPath=Path.Combine(home,"Dual Discord.exe");shortcut.Arguments="--second";shortcut.WorkingDirectory=home;shortcut.IconLocation=Path.Combine(home,"Assets","Dualcord.ico")+",0";shortcut.Description="Open your separate second Discord profile";shortcut.Save();TaskbarIdentity.SetShortcut(path);string legacy=Path.Combine(desktop,"Second Discord.lnk");if(File.Exists(legacy)){object oldLink=shell.CreateShortcut(legacy);try{dynamic old=oldLink;if(((string)old.TargetPath).Equals(Path.Combine(home,"Dual Discord.exe"),StringComparison.OrdinalIgnoreCase)&&(string)old.Arguments=="--second"){string backup=Path.Combine(home,"Backups","Shortcuts-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(backup);File.Move(legacy,Path.Combine(backup,"Second Discord.lnk"));}}finally{Marshal.FinalReleaseComObject(oldLink);}}return path;}
   finally{if(link!=null)Marshal.FinalReleaseComObject(link);Marshal.FinalReleaseComObject(instance);}
  }
 }
}
