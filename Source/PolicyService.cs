using System;
using System.IO;
using System.Net;
using System.Text;
namespace DualDiscord {
 public static class PolicyService {
  static readonly object gate=new object();static PolicyDocument current;static bool ready,checking;static DateTime next=DateTime.MinValue;
  public static string Status="Availability has not been checked yet.";
  public static bool Ready {get{lock(gate)return ready;}}
  public static string BlockReason {get{lock(gate)return AppPolicy.AvailabilityReason(current,UpdateService.CurrentVersion,ready,BuildProfile.Personal);}}
  public static bool StopManagedSecond {get{lock(gate)return AppPolicy.StopManagedSecond(current,UpdateService.CurrentVersion,BuildProfile.Personal);}}
  public static bool RequiresUpdate {get{lock(gate)return current!=null&&UpdateService.ParseVersion(current.MinimumVersion)>UpdateService.ParseVersion(UpdateService.CurrentVersion);}}
  public static bool Due {get{lock(gate)return !checking&&DateTime.UtcNow>=next;}}
  public static void Check(string home){Check(home,Download);}
  internal static void Check(string home,Func<string> download){
   PolicyDocument selected;lock(gate){if(checking)return;checking=true;selected=current;}
   string cache=Path.Combine(home,"Settings","policy-cache.json");string cached=null;
   try{if(File.Exists(cache)){cached=File.ReadAllText(cache);selected=AppPolicy.Verify(cached,PolicyKey.PublicXml,selected==null?0:selected.Revision);}}catch{cached=null;}
   try{
    string json=download();var value=AppPolicy.Verify(json,PolicyKey.PublicXml,selected==null?0:selected.Revision);
    if(selected!=null&&selected.Revision==value.Revision&&cached!=null&&json!=cached)throw new IOException("Conflicting policy revision.");
    selected=value;Status="Verified release policy · revision "+value.Revision;
    try{Directory.CreateDirectory(Path.GetDirectoryName(cache));string temp=cache+".tmp";File.WriteAllText(temp,json);if(File.Exists(cache))File.Replace(temp,cache,null);else File.Move(temp,cache);}catch{Status+=" · local cache could not be saved";}
   }catch{Status=selected==null?(BuildProfile.Personal?"Availability service unavailable. Offline use is allowed.":"Availability could not be verified. Connect and check again before launching."):"Availability service unavailable. Using verified saved policy.";}
   lock(gate){current=selected;ready=true;checking=false;next=DateTime.UtcNow.AddMinutes(1);}
  }
  static string Download(){
   ServicePointManager.SecurityProtocol|=SecurityProtocolType.Tls12;
   var request=(HttpWebRequest)WebRequest.Create("https://raw.githubusercontent.com/"+UpdateService.Repository+"/main/policy/runtime.json?check="+(DateTime.UtcNow.Ticks/TimeSpan.TicksPerMinute));request.CachePolicy=new System.Net.Cache.HttpRequestCachePolicy(System.Net.Cache.HttpRequestCacheLevel.NoCacheNoStore);request.UserAgent="DualDiscord/"+UpdateService.CurrentVersion;request.Timeout=8000;request.ReadWriteTimeout=8000;request.AllowAutoRedirect=false;
   using(var response=request.GetResponse())using(var stream=response.GetResponseStream())using(var memory=new MemoryStream()){
    byte[] buffer=new byte[4096];int count;while((count=stream.Read(buffer,0,buffer.Length))>0){memory.Write(buffer,0,count);if(memory.Length>65536)throw new IOException("Policy is too large.");}return Encoding.UTF8.GetString(memory.ToArray());
   }
  }
  public static void EnsureAllowed(string home){EnsureAllowed(home,()=>Check(home));}
  internal static void EnsureAllowed(string home,Action check){if(!Ready||Due)check();if(!Ready)throw new IOException("Availability is still being checked. Try again in a moment.");string reason=BlockReason;if(reason.Length>0)throw new IOException(reason);}
 }
}
