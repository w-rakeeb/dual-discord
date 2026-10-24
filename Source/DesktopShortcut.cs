using System;
using System.IO;
using System.Runtime.InteropServices;
namespace DualDiscord {
 public static class DesktopShortcut {
  public static string Create(string home,string desktop){
   Directory.CreateDirectory(desktop);string path=Path.Combine(desktop,"Second Discord.lnk");
   object instance=Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell")),link=null;
   try{dynamic shell=instance;link=shell.CreateShortcut(path);dynamic shortcut=link;shortcut.TargetPath=Path.Combine(home,"Dual Discord.exe");shortcut.Arguments="--second";shortcut.WorkingDirectory=home;shortcut.IconLocation=Path.Combine(home,"Assets","Dual Discord.ico")+",0";shortcut.Description="Open your separate second Discord profile";shortcut.Save();return path;}
   finally{if(link!=null)Marshal.FinalReleaseComObject(link);Marshal.FinalReleaseComObject(instance);}
  }
 }
}
