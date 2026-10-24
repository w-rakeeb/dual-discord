using System;
using System.IO;
using System.Diagnostics;
using DualDiscord;
class EngineIntegrationTests {
 static int Main(){string home=AppDomain.CurrentDomain.BaseDirectory;string source=Path.Combine(Path.GetDirectoryName(home.TrimEnd('\\')),"engine-source-"+Guid.NewGuid().ToString("N")),target=source+"-relocated",blocked=source+"-blocked";Directory.CreateDirectory(Path.Combine(source,"discord"));Directory.CreateDirectory(Path.Combine(home,"Logs"));File.WriteAllText(Path.Combine(source,"discord","fixture.txt"),"account-isolation-placeholder");Engine.Custom=new Customization{SecondRoot=source};Engine.Settings=new Options();Engine.Connection=new InstallationSettings{InstallRoot=Path.Combine(home,"fixture-install")};
  using(var client=Process.Start(new ProcessStartInfo(Path.Combine(Engine.DiscordRoot,"Discord.exe"),"--dual-discord-second"){UseShellExecute=false,CreateNoWindow=true})){
   try{System.Threading.Thread.Sleep(500);bool rejected=false;try{Engine.RelocateSecond(blocked);}catch(IOException){rejected=true;}if(!rejected||Directory.Exists(blocked)||Engine.ProfileRoot!=source)throw new Exception("Running-profile guard failed");Console.WriteLine("PASS engine blocks relocation while selected profile runs");Engine.Launch(true);var running=Engine.GetInstance(true,Engine.Snapshot());if(running.Roots.Count!=1||running.Roots[0].Id!=client.Id)throw new Exception("Warm launch replaced the profile process");Console.WriteLine("PASS warm launch reuses existing profile without requiring another executable start");}
   finally{if(!client.HasExited)client.Kill();client.WaitForExit();}
  }
  Engine.RelocateSecond(target);if(Engine.ProfileRoot!=target||!File.Exists(Path.Combine(source,"discord","fixture.txt"))||File.ReadAllText(Path.Combine(Engine.SecondData,"fixture.txt"))!="account-isolation-placeholder")throw new Exception("Engine relocation failed");var loaded=Customization.Load(Path.Combine(home,"Settings","customization.json"));if(loaded.SecondRoot!=target)throw new Exception("Persisted root mismatch");Console.WriteLine("PASS engine switches to verified copy and persists the selected location");
  using(var child=Process.Start(new ProcessStartInfo(Path.Combine(Engine.DiscordRoot,"Discord.exe")){UseShellExecute=false,CreateNoWindow=true}))using(var unrelated=Process.Start(new ProcessStartInfo(Path.Combine(Engine.DiscordRoot,"Discord.exe")){UseShellExecute=false,CreateNoWindow=true})){
   try{using(var job=new ManagedSecond()){job.Assign(child);}if(!child.WaitForExit(3000)||unrelated.HasExited)throw new Exception("Managed lifetime isolation failed");Console.WriteLine("PASS managed exit closes its child and preserves unrelated processes");}finally{if(!child.HasExited)child.Kill();if(!unrelated.HasExited)unrelated.Kill();}
  }
  using(var child=Process.Start(new ProcessStartInfo(Path.Combine(Engine.DiscordRoot,"Discord.exe")){UseShellExecute=false,CreateNoWindow=true})){
   try{using(var job=new ManagedSecond()){job.Assign(child);job.PreserveForRestart();}if(child.HasExited)throw new Exception("Update handoff lost child");Console.WriteLine("PASS updater handoff preserves managed second process");}finally{if(!child.HasExited)child.Kill();}
  }
  string shortcut=DesktopShortcut.Create(home,Path.Combine(home,"FixtureDesktop"));if(!File.Exists(shortcut))throw new Exception("Shortcut was not created");object shell=Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));try{dynamic link=((dynamic)shell).CreateShortcut(shortcut);if((string)link.Arguments!="--second"||(string)link.TargetPath!=Path.Combine(home,"Dual Discord.exe"))throw new Exception("Shortcut target mismatch");System.Runtime.InteropServices.Marshal.FinalReleaseComObject(link);}finally{System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);}Console.WriteLine("PASS second-profile shortcut has correct target and launch argument");return 0;
 }
}
