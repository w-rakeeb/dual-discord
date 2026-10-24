using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using Microsoft.Win32;
using DualDiscord;

static class App {
 static AppShell shell;static UiPreferences preferences;static bool refreshing,busy,closed,missing;static DispatcherTimer timer;static ReleaseInfo availableRelease;static bool updating;
 [DllImport("dwmapi.dll")]static extern int DwmSetWindowAttribute(IntPtr handle,int key,ref int value,int size);
 [STAThread]static void Main(string[] args){
  try{
   if(args.Contains("--ui-qa")){string output=args.FirstOrDefault(s=>s.StartsWith("--qa-output="));UiQa.Run(Engine.Home,output==null?Path.Combine(Engine.Home,"UI Review"):output.Substring(12));return;}
   Engine.Init();
   if(args.Contains("--second")){Engine.Launch(true,args.Contains("--startup"));return;}
   if(args.Contains("--quit-second")){Engine.Quit(true,args.Contains("--force"));return;}
   if(args.Contains("--diagnostics")){var all=Engine.Snapshot();var value=new{Home=Engine.Home,SecondData=Engine.SecondData,MainData=Engine.MainData,Vencord=Engine.Settings.SyncVencord?Engine.MainVencord:Engine.PrivateVencord,Options=Engine.Settings,Main=Engine.GetInstance(false,all),Second=Engine.GetInstance(true,all)};File.WriteAllText(Path.Combine(Engine.Home,"Logs","diagnostics.json"),new JavaScriptSerializer().Serialize(value));return;}
   using(var mutex=new Mutex(false,"Local\\DualDiscordManager2")){bool owned;try{owned=mutex.WaitOne(0);}catch(AbandonedMutexException){owned=true;}if(!owned){MessageBox.Show("Dual Discord is already open.","Dual Discord");return;}try{Show();}finally{mutex.ReleaseMutex();}}
  }catch(Exception ex){try{Engine.Log("UI error: "+ex);}catch{}MessageBox.Show("Dual Discord couldn't finish opening. Your Discord profiles haven't been changed. Details are in the activity log.","Dual Discord");}
 }
 static void Show(){
  var app=new Application{ShutdownMode=ShutdownMode.OnMainWindowClose};preferences=UiPreferences.Load(Engine.Home);Theme.Apply(preferences.Theme);Theme.InitStyles();shell=new AppShell(Engine.Home);app.MainWindow=shell;preferences.Restore(shell);shell.Navigate(preferences.Page,false);
  shell.OpenProfile=async second=>await Act(second,true);shell.QuitProfile=async second=>await Act(second,false);
  shell.OptionChanged=async(name,value)=>await Change(name,value);
  shell.AppearanceChanged=mode=>{preferences.Theme=mode;Theme.Apply(mode);ApplyBackdrop();SaveUi();shell.Toast.Show("Appearance updated");};
  shell.OpenFolder=path=>{try{if(Directory.Exists(path))Process.Start(new ProcessStartInfo(path){UseShellExecute=true});else shell.Toast.Show("This folder hasn't been created yet.");}catch{shell.Toast.Show("The folder couldn't be opened.");}};
  shell.CopyPath=path=>{try{Clipboard.SetText(path);shell.Toast.Show("Path copied");}catch{shell.Toast.Show("The clipboard is busy. Try again.");}};
  shell.ExternalLink=url=>Process.Start(new ProcessStartInfo(url){UseShellExecute=true});shell.Locate=Locate;shell.Detect=async delegate{try{bool found=await Task.Run(()=>Engine.DetectInstallation());shell.Toast.Show(found?"Discord detected and saved":"Discord was not found. Select Discord.exe manually.");Files();await Refresh();}catch(Exception ex){shell.Toast.Show(ex.Message);}};shell.Updates.CheckRequested=async delegate{await CheckUpdates();};shell.Updates.InstallRequested=async delegate{await InstallUpdate();};
  shell.SetOptions(Engine.Settings,false);Files();shell.SourceInitialized+=delegate{ApplyBackdrop();};
  timer=new DispatcherTimer{Interval=TimeSpan.FromSeconds(2)};timer.Tick+=async delegate{await Refresh();};
  shell.Loaded+=async delegate{await Refresh();timer.Start();};
  shell.Closing+=delegate{SaveUi();};shell.Closed+=delegate{closed=true;timer.Stop();SystemEvents.UserPreferenceChanged-=SystemChanged;};
  SystemEvents.UserPreferenceChanged+=SystemChanged;app.Run(shell);
 }
 static void SaveUi(){try{preferences.Save(Engine.Home,shell,shell.Page);}catch{shell.Toast.Show("Window preferences couldn't be saved.");}}
 static void SystemChanged(object sender,UserPreferenceChangedEventArgs e){if(closed)return;shell.Dispatcher.BeginInvoke(new Action(delegate{Theme.Apply(preferences.Theme);ApplyBackdrop();}));}
 static void ApplyBackdrop(){try{var handle=new WindowInteropHelper(shell).Handle;int dark=Theme.Light?0:1;DwmSetWindowAttribute(handle,20,ref dark,4);int corners=2;DwmSetWindowAttribute(handle,33,ref corners,4);int backdrop=Theme.High?1:2;DwmSetWindowAttribute(handle,38,ref backdrop,4);}catch(DllNotFoundException){}}
 static async Task Act(bool second,bool opening){
  if(busy)return;busy=true;var card=second?shell.SecondProfile:shell.MainProfile;card.Loading(opening);
  foreach(var item in new[]{shell.MainProfile,shell.SecondProfile}){item.Open.IsEnabled=false;item.Quit.IsEnabled=false;}
  try{await Task.Run(delegate{if(opening)Engine.Launch(second);else Engine.Quit(second);});shell.Toast.Show((second?"Second":"Main")+" profile "+(opening?(card.Running?"brought to front":"started"):"closed"));}
  catch(Exception ex){Engine.Log("Profile action failed: "+ex.Message);shell.Toast.Show("Discord couldn't "+(opening?"open":"close")+". Try again, or check the activity log.");}
  finally{card.EndLoading();busy=false;}
  await Refresh();
 }
 static async Task Change(string name,bool value){
  try{if(name=="StartApp"){Engine.Settings.StartApp=value;Engine.Save();}else if(name=="StartSecond"){Engine.Settings.StartSecond=value;Engine.Save();}else await Task.Run(delegate{Engine.SetSharing(name=="SyncDiscord",value);});shell.Toast.Show("Settings saved");Files();}
  catch(Exception ex){Engine.Log("Setting change failed: "+ex.Message);shell.Toast.Show("The setting couldn't be changed. Quit the second profile and try again.");}
  await Refresh();
 }
 static async Task Refresh(){
  if(refreshing||closed)return;refreshing=true;
  try{
   var values=await Task.Run(delegate{var all=Engine.Snapshot();return new[]{Engine.GetInstance(false,all),Engine.GetInstance(true,all)};});if(closed)return;
   shell.SetStats(values[0],values[1],Directory.Exists(Engine.SecondData));shell.SetOptions(Engine.Settings,values[1].Count>0||busy);
   try{Engine.FindDiscord();missing=false;}catch{missing=true;}shell.Missing(missing);shell.SetInstallation(missing?"Discord was not found. Detect it or select the installed Discord.exe.":"Detected: "+StoragePath(Engine.DiscordRoot));
   if(busy)foreach(var card in new[]{shell.MainProfile,shell.SecondProfile}){card.Open.IsEnabled=false;card.Quit.IsEnabled=false;}
  }catch(Exception ex){Engine.Log("Status unavailable: "+ex.Message);if(!closed)shell.Toast.Show("Status is temporarily unavailable.");}
  finally{refreshing=false;}
 }
 static void Locate(){
  var dialog=new OpenFileDialog{Title="Locate the installed Discord",Filter="Discord application|Discord.exe",CheckFileExists=true};
  if(Directory.Exists(Engine.DiscordRoot))dialog.InitialDirectory=Engine.DiscordRoot;
  if(dialog.ShowDialog(shell)!=true)return;
  try{Engine.SelectInstallation(dialog.FileName);Files();shell.Missing(false);shell.SetInstallation("Detected: "+StoragePath(Engine.DiscordRoot));shell.Toast.Show("Discord installation saved");}catch(Exception ex){shell.Toast.Show(ex.Message);}
 }
 static async Task CheckUpdates(){
  if(updating)return;updating=true;shell.Updates.State("Checking GitHub…",true);
  try{var release=await Task.Run(()=>UpdateService.GetLatest());if(closed)return;availableRelease=release;bool newer=release.Version>UpdateService.ParseVersion(UpdateService.CurrentVersion);shell.Updates.State(newer?"Version "+release.Tag+" is available.":"You're up to date. Installed version: "+UpdateService.CurrentVersion,false,newer);}
  catch(Exception ex){if(!closed)shell.Updates.State(ex is IOException?ex.Message:"The update check could not be completed.");}
  finally{updating=false;}
 }
 static async Task InstallUpdate(){
  if(updating||availableRelease==null||availableRelease.Version<=UpdateService.ParseVersion(UpdateService.CurrentVersion))return;updating=true;shell.Updates.State("Downloading and verifying the update…",true);
  try{
   string zip=await Task.Run(()=>UpdateService.Download(availableRelease));if(closed)return;
   string folder=Path.GetDirectoryName(zip),helper=Path.Combine(folder,"Dual Discord Updater.exe");File.Copy(Path.Combine(Engine.Home,"Dual Discord Updater.exe"),helper,false);
   using(var current=Process.GetCurrentProcess()){
    var request=new UpdateRequest{Target=Engine.Home,Package=zip,Sha256=availableRelease.Sha256,Version=availableRelease.Tag,ManagerPid=current.Id,ManagerStartTicks=current.StartTime.ToUniversalTime().Ticks};string file=Path.Combine(folder,"request.json");File.WriteAllText(file,new JavaScriptSerializer().Serialize(request));
    Process.Start(new ProcessStartInfo(helper,"--request \""+file+"\""){UseShellExecute=false,CreateNoWindow=true,WorkingDirectory=folder});
   }
   shell.Close();
  }catch(Exception ex){if(!closed)shell.Updates.State(ex is IOException?ex.Message:"The update could not be installed. Try a writable app folder.",false,true);}
  finally{updating=false;}
 }
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern Microsoft.Win32.SafeHandles.SafeFileHandle CreateFile(string path,uint access,uint share,IntPtr security,uint creation,uint flags,IntPtr template);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern uint GetFinalPathNameByHandle(Microsoft.Win32.SafeHandles.SafeFileHandle handle,System.Text.StringBuilder path,uint length,uint flags);
 static string StoragePath(string path){
  try{using(var handle=CreateFile(path,0,7,IntPtr.Zero,3,0x02000000,IntPtr.Zero)){
   if(handle.IsInvalid)return path;var buffer=new System.Text.StringBuilder(32768);uint length=GetFinalPathNameByHandle(handle,buffer,(uint)buffer.Capacity,0);
   if(length==0||length>=buffer.Capacity)return path;string resolved=buffer.ToString();return resolved.StartsWith("\\\\?\\")?resolved.Substring(4):resolved;
  }}catch{return path;}
 }
 static void Files(){shell.SetFiles(new[]{
  new KeyValuePair<string,string>("Main profile data",Engine.MainData),new KeyValuePair<string,string>("Second profile data",Engine.SecondData),
  new KeyValuePair<string,string>("Activity logs",Path.Combine(Engine.Home,"Logs")),new KeyValuePair<string,string>("App and source files",Engine.Home),
  new KeyValuePair<string,string>("Installed Discord",Engine.DiscordRoot),new KeyValuePair<string,string>("Main Vencord data",Engine.MainVencord),
  new KeyValuePair<string,string>("Second Vencord data",Engine.Settings.SyncVencord?Engine.MainVencord:Engine.PrivateVencord)}.Select(item=>new KeyValuePair<string,string>(item.Key,StoragePath(item.Value))));}
}
