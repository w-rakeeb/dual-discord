using System;
using System.IO;
using System.Web.Script.Serialization;
using System.Windows;

namespace DualDiscord {
public sealed class UiPreferences {
 public string Theme {get;set;} public string Page {get;set;}
 public double Width {get;set;} public double Height {get;set;} public double Left {get;set;} public double Top {get;set;}
 public UiPreferences(){Theme="Dark";Page="Profiles";Width=900;Height=620;Left=double.NaN;Top=double.NaN;}
 public static UiPreferences Load(string home){try{var file=Path.Combine(home,"Settings","ui.json");return File.Exists(file)?new JavaScriptSerializer().Deserialize<UiPreferences>(File.ReadAllText(file)):new UiPreferences();}catch{return new UiPreferences();}}
 public void Save(string home,Window window,string page){var bounds=window.RestoreBounds;Width=Math.Max(720,bounds.Width);Height=Math.Max(520,bounds.Height);Left=bounds.Left;Top=bounds.Top;Page=page;Directory.CreateDirectory(Path.Combine(home,"Settings"));File.WriteAllText(Path.Combine(home,"Settings","ui.json"),new JavaScriptSerializer().Serialize(this));}
 public void Restore(Window window){window.Width=Math.Max(720,Math.Min(Width,SystemParameters.VirtualScreenWidth));window.Height=Math.Max(520,Math.Min(Height,SystemParameters.VirtualScreenHeight));if(!double.IsNaN(Left)&&!double.IsNaN(Top)&&Left+Width>SystemParameters.VirtualScreenLeft&&Left<SystemParameters.VirtualScreenLeft+SystemParameters.VirtualScreenWidth&&Top+Height>SystemParameters.VirtualScreenTop&&Top<SystemParameters.VirtualScreenTop+SystemParameters.VirtualScreenHeight){window.WindowStartupLocation=WindowStartupLocation.Manual;window.Left=Left;window.Top=Top;}}
}
}
