using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
namespace DualDiscord {
 // Copies configuration, never plugin binaries, cloud authentication or Discord profiles.
 public static class VencordSharing {
  const int Limit=4*1024*1024;
  static readonly string[] General={"notifyAboutUpdates","autoUpdate","autoUpdateNotification","frameless","transparent","winCtrlQ","disableMinSize","winNativeTitleBar","nativeTitleBar","windowsMaterial","notifications","uiElements"};
  static JavaScriptSerializer Serializer(){return new JavaScriptSerializer{MaxJsonLength=Limit,RecursionLimit=64};}
  public static bool Selected(Customization o){return o.CopyVencordPlugins||o.CopyVencordPluginSettings||o.CopyVencordPreferences||o.CopyVencordAppearance||o.CopyQuickCss;}
  public static void Apply(string source,string target,Customization o){
   if(!Selected(o))return;source=Installation.Physical(source);target=Installation.Physical(target);
   if(source.Equals(target,StringComparison.OrdinalIgnoreCase)||Installation.IsUnder(source,target)||Installation.IsUnder(target,source))throw new IOException("Choose separate Vencord storage before copying settings.");
   string from=Path.Combine(source,"settings"),to=Path.Combine(target,"settings"),backups=Path.Combine(target,"Backups");
   CheckPath(source,from);CheckPath(target,to);CheckPath(target,backups);
   string input=Path.Combine(from,"settings.json"),output=Path.Combine(to,"settings.json");CheckPath(source,input);CheckPath(target,output);
   var changes=new Dictionary<string,byte[]>();
   if(File.Exists(input)){
    var main=Read(input);var second=File.Exists(output)?Read(output):new Dictionary<string,object>();string original=Serializer().Serialize(second);
    if(o.CopyVencordPreferences)foreach(string key in General)if(main.ContainsKey(key))SetSafe(key,main[key],second);
    if(o.CopyVencordAppearance)foreach(string key in new[]{"enabledThemes","themeLinks"})if(main.ContainsKey(key))SetSafe(key,main[key],second);
    if(o.CopyQuickCss&&main.ContainsKey("useQuickCss"))SetSafe("useQuickCss",main["useQuickCss"],second);
    object raw;if((o.CopyVencordPlugins||o.CopyVencordPluginSettings)&&main.TryGetValue("plugins",out raw)){
     var plugins=raw as Dictionary<string,object>;if(plugins==null)throw new IOException("Main Vencord plugin settings are unreadable.");
     var destination=Dictionary(second,"plugins");
     foreach(var entry in plugins){var plugin=entry.Value as Dictionary<string,object>;if(plugin==null||!SafeKey(entry.Key))continue;var current=Dictionary(destination,entry.Key);
      foreach(var option in plugin){if(option.Key=="enabled"){if(o.CopyVencordPlugins&&option.Value is bool)current["enabled"]=option.Value;}else if(o.CopyVencordPluginSettings)SetSafe(option.Key,option.Value,current);}
     }
    }
    string updated=Serializer().Serialize(second);if(updated!=original)changes[output]=Encoding.UTF8.GetBytes(updated);
   }
   if(o.CopyQuickCss){string css=Path.Combine(from,"quickCss.css"),destination=Path.Combine(to,"quickCss.css");CheckPath(source,css);CheckPath(target,destination);if(File.Exists(css)){Bound(css);if(File.Exists(destination))Bound(destination);byte[] bytes=File.ReadAllBytes(css);if(!File.Exists(destination)||!bytes.SequenceEqual(File.ReadAllBytes(destination)))changes[destination]=bytes;}}
   if(changes.Count==0)return;
   // Validate and back up the entire batch before committing any replacement.
   Directory.CreateDirectory(to);CheckPath(target,to);Directory.CreateDirectory(backups);CheckPath(target,backups);
   string backup=Path.Combine(backups,"Settings-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")+"-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(backup);
   var originals=new Dictionary<string,string>();foreach(string file in changes.Keys){CheckPath(target,file);if(File.Exists(file)){Bound(file);string saved=Path.Combine(backup,Path.GetFileName(file));File.Copy(file,saved);originals[file]=saved;}}
   var written=new List<string>();try{foreach(var change in changes){Commit(change.Key,change.Value);written.Add(change.Key);}}
   catch{foreach(string file in written){string saved;if(originals.TryGetValue(file,out saved))Commit(file,File.ReadAllBytes(saved));else if(File.Exists(file))File.Move(file,Path.Combine(backup,"Failed-new-"+Path.GetFileName(file)));}throw;}
  }
  static void CheckPath(string root,string path){string relative=path.Substring(root.Length).TrimStart('\\');string current=root;foreach(string part in relative.Split('\\')){current=Path.Combine(current,part);if((File.Exists(current)||Directory.Exists(current))&&(File.GetAttributes(current)&FileAttributes.ReparsePoint)!=0)throw new IOException("Vencord copy cannot use linked settings or backup files.");}}
  static void Bound(string file){if(new FileInfo(file).Length>Limit)throw new IOException("Vencord settings or QuickCSS exceed the 4 MB copy limit.");}
  static Dictionary<string,object> Read(string file){Bound(file);try{var value=Serializer().Deserialize<Dictionary<string,object>>(File.ReadAllText(file));if(value==null)throw new ArgumentException();return value;}catch(ArgumentException){throw new IOException("Vencord settings could not be read. The second profile was kept.");}catch(InvalidOperationException){throw new IOException("Vencord settings could not be read. The second profile was kept.");}}
  static Dictionary<string,object> Dictionary(Dictionary<string,object> parent,string key){object value;var result=parent.TryGetValue(key,out value)?value as Dictionary<string,object>:null;if(result==null){result=new Dictionary<string,object>();parent[key]=result;}return result;}
  static bool SafeKey(string key){string normalized=new string(key.Where(Char.IsLetterOrDigit).ToArray()).ToLowerInvariant();return normalized!="auth"&&normalized!="authorization"&&normalized!="authentication"&&!new[]{"token","secret","password","apikey","webhook","credential","cloud","cookie","sessionid"}.Any(normalized.Contains);}
  static void SetSafe(string key,object value,Dictionary<string,object> target){
   if(!SafeKey(key)||value==null)return;
   var map=value as Dictionary<string,object>;if(map!=null){var child=Dictionary(target,key);foreach(var pair in map)SetSafe(pair.Key,pair.Value,child);return;}
   if(!(value is string)&&value is IEnumerable){var list=new List<object>();foreach(object item in (IEnumerable)value){var holder=new Dictionary<string,object>();SetSafe("item",item,holder);if(holder.ContainsKey("item"))list.Add(holder["item"]);}target[key]=list.ToArray();return;}
   target[key]=value;
  }
  static void Commit(string file,byte[] bytes){string temp=file+".tmp-"+Guid.NewGuid().ToString("N");using(var stream=new FileStream(temp,FileMode.CreateNew,FileAccess.Write,FileShare.None))stream.Write(bytes,0,bytes.Length);if(File.Exists(file))File.Replace(temp,file,null);else File.Move(temp,file);}
 }
}
