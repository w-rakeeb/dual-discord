using System;
using System.IO;
using System.Diagnostics;
using DualDiscord;
class EngineIntegrationTests {
 static int Main(){string home=AppDomain.CurrentDomain.BaseDirectory;string source=Path.Combine(Path.GetDirectoryName(home.TrimEnd('\\')),"engine-source-"+Guid.NewGuid().ToString("N")),target=source+"-relocated",blocked=source+"-blocked";Directory.CreateDirectory(Path.Combine(source,"discord"));Directory.CreateDirectory(Path.Combine(home,"Logs"));File.WriteAllText(Path.Combine(source,"discord","fixture.txt"),"account-isolation-placeholder");Engine.Custom=new Customization{SecondRoot=source};Engine.Settings=new Options();Engine.Connection=new InstallationSettings{InstallRoot=Path.Combine(home,"fixture-install")};
  using(var client=Process.Start(new ProcessStartInfo(Path.Combine(Engine.DiscordRoot,"Discord.exe"),"--dual-discord-second"){UseShellExecute=false,CreateNoWindow=true})){
   try{System.Threading.Thread.Sleep(500);bool rejected=false;try{Engine.RelocateSecond(blocked);}catch(IOException){rejected=true;}if(!rejected||Directory.Exists(blocked)||Engine.ProfileRoot!=source)throw new Exception("Running-profile guard failed");Console.WriteLine("PASS engine blocks relocation while selected profile runs");}
   finally{if(!client.HasExited)client.Kill();client.WaitForExit();}
  }
  Engine.RelocateSecond(target);if(Engine.ProfileRoot!=target||!File.Exists(Path.Combine(source,"discord","fixture.txt"))||File.ReadAllText(Path.Combine(Engine.SecondData,"fixture.txt"))!="account-isolation-placeholder")throw new Exception("Engine relocation failed");var loaded=Customization.Load(Path.Combine(home,"Settings","customization.json"));if(loaded.SecondRoot!=target)throw new Exception("Persisted root mismatch");Console.WriteLine("PASS engine switches to verified copy and persists the selected location");return 0;
 }
}
