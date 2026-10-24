using System;
using System.Collections.Generic;
using System.IO;
namespace DualDiscord {
 public static class PreferenceSharing {
  static readonly string[] Audio={"audioSubsystem","useLegacyAudioDevice","offloadAdmControls","asyncVideoInputDeviceInit"};
  static readonly string[] Graphics={"BACKGROUND_COLOR","enableHardwareAcceleration","enableH264MFElectron","openH264Enabled"};
  public static void Apply(IDictionary<string,object> source,IDictionary<string,object> target,Customization options){if(options.SyncAudio)Copy(Audio,source,target);if(options.SyncGraphics)Copy(Graphics,source,target);}
  static void Copy(string[] keys,IDictionary<string,object> source,IDictionary<string,object> target){foreach(string key in keys)if(source.ContainsKey(key))target[key]=source[key];}
  public static void CopyThemes(string source,string target){
   if(!Directory.Exists(source))return;
   if((File.GetAttributes(source)&FileAttributes.ReparsePoint)!=0)throw new IOException("Theme folder links cannot be copied.");
   Directory.CreateDirectory(target);if((File.GetAttributes(target)&FileAttributes.ReparsePoint)!=0)throw new IOException("Choose separate Vencord storage before copying themes.");
   string backupRoot=Path.Combine(target,"Backups");if(Directory.Exists(backupRoot)&&(File.GetAttributes(backupRoot)&FileAttributes.ReparsePoint)!=0)throw new IOException("Theme backups cannot use a linked folder.");
   if(Installation.Physical(source).Equals(Installation.Physical(target),StringComparison.OrdinalIgnoreCase))throw new IOException("Choose separate Vencord storage before copying themes.");
   var files=Directory.GetFiles(source,"*.css");long total=0;foreach(string file in files){var info=new FileInfo(file);if((info.Attributes&FileAttributes.ReparsePoint)!=0)throw new IOException("Theme links cannot be copied.");total+=info.Length;}if(total>16*1024*1024)throw new IOException("The CSS themes exceed the 16 MB copy limit.");
   string backup=Path.Combine(target,"Backups",DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")+"-"+Guid.NewGuid().ToString("N"));
   foreach(string file in files){string destination=Path.Combine(target,Path.GetFileName(file));if(File.Exists(destination)){if((File.GetAttributes(destination)&FileAttributes.ReparsePoint)!=0)throw new IOException("Theme links cannot be replaced.");Directory.CreateDirectory(backup);File.Copy(destination,Path.Combine(backup,Path.GetFileName(file)),false);}File.Copy(file,destination,true);}
  }
 }
}
