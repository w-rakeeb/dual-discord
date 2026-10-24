using System;
using System.IO;
using System.Web.Script.Serialization;
namespace DualDiscord {
 public sealed class Customization {
  public string SecondRoot{get;set;}
  public string CloseAction{get;set;} public bool HideTaskbar{get;set;} public bool HideTray{get;set;} public bool KeepSecondRunning{get;set;}
  public bool AutoCheckUpdates{get;set;}
  public bool SyncAudio{get;set;}
  public bool SyncGraphics{get;set;}
  public bool CopyThemes{get;set;}
  public Customization(){CloseAction="Tray";KeepSecondRunning=true;AutoCheckUpdates=true;SyncAudio=true;SyncGraphics=true;}
  public static Customization Load(string file){if(!File.Exists(file))return new Customization();var value=new JavaScriptSerializer().Deserialize<Customization>(File.ReadAllText(file));if(value==null)throw new IOException("Customization settings are unreadable.");return value;}
  public static void Save(string file,Customization value){string parent=Path.GetDirectoryName(file);Directory.CreateDirectory(parent);string temp=file+".tmp";File.WriteAllText(temp,new JavaScriptSerializer().Serialize(value));if(File.Exists(file))File.Replace(temp,file,file+".previous");else File.Move(temp,file);}
 }
}
