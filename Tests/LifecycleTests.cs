using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using DualDiscord;
static class LifecycleTests {
 static int failed,total;static void Assert(bool v){if(!v)throw new Exception("Assertion failed");}
 static void Check(string name,Action test){total++;try{test();Console.WriteLine("PASS "+name);}catch(Exception e){failed++;Console.WriteLine("FAIL "+name+": "+e.Message);}}
 static void Reject(Action action){try{action();}catch(IOException){return;}throw new Exception("Expected rejection");}
 static string Envelope(RSACryptoServiceProvider key,PolicyDocument doc){byte[] bytes=Encoding.UTF8.GetBytes(new JavaScriptSerializer().Serialize(doc));return new JavaScriptSerializer().Serialize(new PolicyEnvelope{Payload=Convert.ToBase64String(bytes),Signature=Convert.ToBase64String(key.SignData(bytes,CryptoConfig.MapNameToOID("SHA256")))});}
 public static int Run(){
  Check("Close defaults to tray; second lifetime follows build",()=>{var settings=new Customization();Assert(settings.CloseAction=="Tray"&&settings.KeepSecondRunning==BuildProfile.Personal&&!settings.HideTaskbar&&!settings.HideTray);});
  Check("Old customization JSON inherits new lifecycle defaults",()=>{var settings=new JavaScriptSerializer().Deserialize<Customization>("{\"AutoCheckUpdates\":false}");Assert(settings.CloseAction=="Tray"&&settings.KeepSecondRunning==BuildProfile.Personal&&!settings.AutoCheckUpdates);});
  Check("Exit bypasses tray behavior only when explicitly requested",()=>{Assert(LifecycleRules.CloseAction("Tray",false)=="Tray"&&LifecycleRules.CloseAction("Tray",true)=="Exit"&&LifecycleRules.CloseAction("Minimize",false)=="Minimize"&&LifecycleRules.CloseAction("invalid",false)=="Tray");});
  Check("Lifecycle settings persist independently of account data",()=>{string file=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Lifecycle-"+Guid.NewGuid().ToString("N"),"customization.json");Customization.Save(file,new Customization{CloseAction="Exit",HideTaskbar=true,HideTray=true,KeepSecondRunning=false});var value=Customization.Load(file);Assert(value.CloseAction=="Exit"&&value.HideTaskbar&&value.HideTray&&!value.KeepSecondRunning);});
  using(var key=new RSACryptoServiceProvider(2048)){key.PersistKeyInCsp=false;var doc=new PolicyDocument{Revision=5,MinimumVersion="2.3.0",Message="Please update"};string json=Envelope(key,doc),publicKey=key.ToXmlString(false);
   Check("A signed enable policy permits the required version",()=>{var verified=AppPolicy.Verify(json,publicKey,0);Assert(verified.Revision==5&&AppPolicy.BlockReason(verified,"2.3.0")=="");});
   Check("Mandatory version blocks older launcher builds",()=>Assert(AppPolicy.BlockReason(AppPolicy.Verify(json,publicKey,0),"2.2.0").Length>0));
   Check("Verified restrictions stop managed public profiles but preserve personal lifetime",()=>{var verified=AppPolicy.Verify(json,publicKey,0);Assert(AppPolicy.StopManagedSecond(verified,"2.2.0",false)&&!AppPolicy.StopManagedSecond(verified,"2.2.0",true)&&!AppPolicy.StopManagedSecond(verified,"2.3.0",false)&&!AppPolicy.StopManagedSecond(null,"2.2.0",false));});
   Check("First public launch needs verified availability; cache and personal offline use remain",()=>{Assert(AppPolicy.AvailabilityReason(null,"2.6.0",true,false).Length>0&&AppPolicy.AvailabilityReason(null,"2.6.0",true,true)==""&&AppPolicy.AvailabilityReason(doc,"2.6.0",true,false)=="");});
   Check("Signed disable policy blocks launches even on newest version",()=>{doc.Disabled=true;doc.Message="This utility is temporarily unavailable.";var value=AppPolicy.Verify(Envelope(key,doc),publicKey,0);Assert(AppPolicy.BlockReason(value,"99.0.0")==doc.Message);doc.Disabled=false;});
   Check("Unsigned policy is rejected",()=>Reject(()=>AppPolicy.Verify("{\"Payload\":\"e30=\",\"Signature\":\"\"}",publicKey,0)));
   Check("Policy signed by another key is rejected",()=>{using(var other=new RSACryptoServiceProvider(2048)){other.PersistKeyInCsp=false;Reject(()=>AppPolicy.Verify(json,other.ToXmlString(false),0));}});
   Check("Replayed older policy revision is rejected",()=>Reject(()=>AppPolicy.Verify(json,publicKey,6)));
   Check("Tampered policy content is rejected",()=>{var env=new JavaScriptSerializer().Deserialize<PolicyEnvelope>(json);env.Payload=Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"Revision\":999}"));Reject(()=>AppPolicy.Verify(new JavaScriptSerializer().Serialize(env),publicKey,0));});
   Check("Unsupported minimum version is rejected",()=>{doc.MinimumVersion="latest";Reject(()=>AppPolicy.Verify(Envelope(key,doc),publicKey,0));});
  }
  Check("Offline availability uses a verified saved policy",()=>{string home=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Offline-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Path.Combine(home,"Settings"));string signed=File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"..","policy","runtime.json"));File.WriteAllText(Path.Combine(home,"Settings","policy-cache.json"),signed);PolicyService.Check(home,()=>{throw new IOException("offline");});Assert(PolicyService.Ready&&PolicyService.BlockReason==""&&PolicyService.Status.Contains("saved policy"));});
  Check("Availability network wait does not block UI status reads",()=>{string home=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Responsive-"+Guid.NewGuid().ToString("N"));using(var entered=new System.Threading.ManualResetEvent(false))using(var release=new System.Threading.ManualResetEvent(false)){var task=System.Threading.Tasks.Task.Run(()=>PolicyService.Check(home,()=>{entered.Set();release.WaitOne(2000);throw new IOException("offline");}));Assert(entered.WaitOne(2000));var clock=System.Diagnostics.Stopwatch.StartNew();bool ready=PolicyService.Ready;string reason=PolicyService.BlockReason;clock.Stop();release.Set();task.Wait();Assert(clock.ElapsedMilliseconds<300&&ready&&reason=="");}});
  Check("Launching refreshes stale policy but reuses a fresh verified policy",()=>{string home=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"LaunchFreshness-"+Guid.NewGuid().ToString("N"));string signed=File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"..","policy","runtime.json"));PolicyService.Check(home,()=>signed);bool checkedAgain=false;PolicyService.EnsureAllowed(home,()=>{checkedAgain=true;PolicyService.Check(home,()=>signed);});Assert(!checkedAgain);typeof(PolicyService).GetField("next",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static).SetValue(null,DateTime.MinValue);PolicyService.EnsureAllowed(home,()=>{checkedAgain=true;PolicyService.Check(home,()=>signed);});Assert(checkedAgain);});
  Console.WriteLine("LIFECYCLE "+(total-failed)+" passed, "+failed+" failed");return failed;
 }
}
