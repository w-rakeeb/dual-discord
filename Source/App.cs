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
 static bool loaded;static string shownPolicy="";static AppShell shell;static UiPreferences preferences;static bool refreshing,busy,closed,missing;static DispatcherTimer timer;static ReleaseInfo availableRelease;static bool updating;static readonly UpdateSchedule updateSchedule=new UpdateSchedule();static WindowLifecycle lifecycle;static InstanceSignals signals;static ManagedSecond managedSecond;static bool policyChecking,exitingForUpdate,installingUpdate;static TaskbarIdentity taskbar;
 [DllImport("dwmapi.dll")]static extern int DwmSetWindowAttribute(IntPtr handle,int key,ref int value,int size);
 [STAThread]static void Main(string[] args){
  try{
   if(args.Contains("--ui-qa")){string output=args.FirstOrDefault(s=>s.StartsWith("--qa-output="));UiQa.Run(Engine.Home,output==null?Path.Combine(Engine.Home,"UI Review"):output.Substring(12));return;}
   if(args.Contains("--exit")){InstanceSignals.Send(2);return;}
   Engine.Init();

   if(args.Contains("--quit-second")){Engine.Quit(true,args.Contains("--force"));return;}
   if(args.Contains("--diagnostics")){var all=Engine.Snapshot();var value=new{Home=Engine.Home,SecondData=Engine.SecondData,MainData=Engine.MainData,Vencord=Engine.Settings.SyncVencord?Engine.MainVencord:Engine.PrivateVencord,Options=Engine.Settings,Main=Engine.GetInstance(false,all),Second=Engine.GetInstance(true,all)};File.WriteAllText(Path.Combine(Engine.Home,"Logs","diagnostics.json"),new JavaScriptSerializer().Serialize(value));return;}
   using(var mutex=new Mutex(false,"Local\\DualDiscordManager2")){bool owned;try{owned=mutex.WaitOne(0);}catch(AbandonedMutexException){owned=true;}if(!owned){if(!InstanceSignals.Send(args.Contains("--second")?1:0))MessageBox.Show("Dual Discord is opening. Try its shortcut again in a moment.","Dual Discord");return;}try{Show(args.Contains("--second"),args.Contains("--startup"));}finally{mutex.ReleaseMutex();}}
  }catch(Exception ex){try{Engine.Log("UI error: "+ex);}catch{}MessageBox.Show(ex is IOException?ex.Message:"Dual Discord couldn't finish opening. Your Discord profiles haven't been changed. Details are in the activity log.","Dual Discord");}
 }
 static void Show(bool launchSecond=false,bool startup=false){
  var app=new Application{ShutdownMode=ShutdownMode.OnMainWindowClose};preferences=UiPreferences.Load(Engine.Home);Theme.Apply(preferences.Theme);Theme.InitStyles();shell=new AppShell(Engine.Home);app.MainWindow=shell;preferences.Restore(shell);shell.Navigate(preferences.Page,false);
  taskbar=new TaskbarIdentity(Engine.Home);lifecycle=new WindowLifecycle(shell,Engine.Home,()=>Engine.Custom);lifecycle.ExitRequested=RequestExit;lifecycle.SecondOpenRequested=async delegate{await Act(true,true);};lifecycle.SecondQuitRequested=async delegate{await Act(true,false);};lifecycle.Restored=CheckOnReopen;signals=new InstanceSignals(shell,lifecycle.Restore,async delegate{await Act(true,true);},RequestExit);app.SessionEnding+=delegate{lifecycle.AllowExit();};Engine.SecondStarted=process=>{if(!Engine.Custom.KeepSecondRunning){if(managedSecond==null)managedSecond=new ManagedSecond();managedSecond.Assign(process);}};
  shell.ExitApp=RequestExit;shell.CheckPolicy=async delegate{await CheckPolicy();await Refresh();};shell.CloseActionChanged=async value=>{string previous=Engine.Custom.CloseAction;try{Engine.Custom.CloseAction=value;Engine.SaveCustom();shell.Toast.Show("Close behavior updated");}catch{Engine.Custom.CloseAction=previous;shell.Toast.Show("Close behavior could not be saved.");}await Refresh();};
  shell.AddShortcut=delegate{try{DesktopShortcut.Create(Engine.Home,Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));shell.Toast.Show("Dualcord shortcut added to your desktop");}catch{shell.Toast.Show("The desktop shortcut could not be created.");}};
  if(launchSecond){shell.ShowActivated=false;shell.Opacity=0;shell.ShowInTaskbar=false;}
  shell.ResetPreferences=async delegate{await ResetPreferences();};
  shell.OpenProfile=async second=>await Act(second,true);shell.QuitProfile=async second=>await Act(second,false);
  shell.OptionChanged=async(name,value)=>await Change(name,value);
  shell.AppearanceChanged=mode=>{preferences.Theme=mode;Theme.Apply(mode);ApplyBackdrop();SaveUi();shell.Toast.Show("Appearance updated");};
  shell.OpenFolder=path=>{try{if(Directory.Exists(path))Process.Start(new ProcessStartInfo(path){UseShellExecute=true});else shell.Toast.Show("This folder hasn't been created yet.");}catch{shell.Toast.Show("The folder couldn't be opened.");}};
  shell.CopyPath=path=>{try{Clipboard.SetText(path);shell.Toast.Show("Path copied");}catch{shell.Toast.Show("The clipboard is busy. Try again.");}};
  shell.ExternalLink=url=>Process.Start(new ProcessStartInfo(url){UseShellExecute=true});shell.Locate=Locate;shell.Detect=async delegate{try{bool found=await Task.Run(()=>Engine.DetectInstallation());shell.Toast.Show(found?"Discord detected and saved":"Discord was not found. Select Discord.exe manually.");Files();await Refresh();}catch(Exception ex){shell.Toast.Show(ex.Message);}};shell.ChooseStorage=async delegate{await ChooseStorage();};shell.Updates.CheckRequested=async delegate{await CheckUpdates();};shell.Updates.InstallRequested=async delegate{await InstallUpdate();};
  shell.Activated+=delegate{CheckOnReopen();};shell.SetOptions(Engine.Settings,false);Files();shell.SourceInitialized+=delegate{ApplyBackdrop();};
  timer=new DispatcherTimer{Interval=TimeSpan.FromSeconds(2)};timer.Tick+=async delegate{await Refresh();if(!policyChecking&&PolicyService.Due)await CheckPolicy();if(!updating&&updateSchedule.TakeCheck(Engine.Custom.AutoCheckUpdates,DateTime.UtcNow))await CheckUpdates();};
  bool initialLoad=true;shell.Loaded+=async delegate{if(!initialLoad)return;initialLoad=false;if(launchSecond){shell.Hide();shell.Opacity=1;}await Refresh();timer.Start();var policy=CheckPolicy();var updates=updateSchedule.TakeCheck(Engine.Custom.AutoCheckUpdates,DateTime.UtcNow)?CheckUpdates():Task.FromResult(0);await Task.WhenAll(policy,updates);if(closed)return;loaded=true;await Refresh();if(launchSecond&&PolicyService.BlockReason.Length==0){await Act(true,true,startup);if(!shell.SecondProfile.Running)lifecycle.Restore();}if(updateSchedule.TakeCheck(Engine.Custom.AutoCheckUpdates,DateTime.UtcNow))await CheckUpdates();};
  shell.Closing+=delegate(object sender,System.ComponentModel.CancelEventArgs e){if(busy||(installingUpdate&&!exitingForUpdate)){e.Cancel=true;shell.Toast.Show("Wait for the current action to finish before closing.");}SaveUi();};shell.Closed+=delegate{closed=true;timer.Stop();signals.Dispose();lifecycle.Dispose();taskbar.Dispose();if(managedSecond!=null)managedSecond.Dispose();Engine.SecondStarted=null;SystemEvents.UserPreferenceChanged-=SystemChanged;};
  SystemEvents.UserPreferenceChanged+=SystemChanged;app.Run(shell);
 }
 static async void CheckOnReopen(){if(!loaded||closed||updating)return;if(updateSchedule.TakeCheck(Engine.Custom.AutoCheckUpdates,DateTime.UtcNow,true))await CheckUpdates();}
 static void RequestExit(){if(busy||installingUpdate){lifecycle.Restore();shell.Toast.Show("Wait for the current action to finish before exiting.");return;}lifecycle.AllowExit();shell.Close();}
 static async Task CheckPolicy(){if(policyChecking||closed)return;policyChecking=true;try{await Task.Run(()=>PolicyService.Check(Engine.Home));if(!closed){shell.SetPolicy(PolicyService.BlockReason,false);if(PolicyService.BlockReason.Length>0&&shownPolicy!=PolicyService.BlockReason){shownPolicy=PolicyService.BlockReason;lifecycle.Restore();shell.Navigate("Profiles",false);}else if(PolicyService.BlockReason.Length==0)shownPolicy="";if(PolicyService.RequiresUpdate&&!updating&&updateSchedule.TakeCheck(true,DateTime.UtcNow))await CheckUpdates();}}finally{policyChecking=false;}}
 static void BindManagedSecond(Instance instance){if(Engine.Custom.KeepSecondRunning)return;if(managedSecond==null)managedSecond=new ManagedSecond();foreach(var item in instance.Processes)try{using(var process=Process.GetProcessById(item.Id)){long ticks=process.StartTime.ToUniversalTime().Ticks;if(item.StartedUtcTicks>0&&Math.Abs(ticks-item.StartedUtcTicks)<=10&&Installation.Physical(process.MainModule.FileName).Equals(Installation.Physical(item.Exe),StringComparison.OrdinalIgnoreCase))managedSecond.Assign(process);}}catch(ArgumentException){}catch(InvalidOperationException){}}
 static async Task ResetPreferences(){
  if(busy||installingUpdate){shell.Toast.Show("Wait for the current action to finish before resetting.");return;}if(MessageBox.Show(shell,"Restore default startup, sharing, appearance and window-behavior preferences? Your accounts, files and profile locations are kept. A settings backup is saved first.","Reset app preferences",MessageBoxButton.YesNo,MessageBoxImage.Question)!=MessageBoxResult.Yes)return;busy=true;try{await Task.Run(()=>Engine.ResetPreferences());preferences.Theme="Dark";Theme.Apply("Dark");shell.SetAppearance("Dark");lifecycle.Apply();ApplyBackdrop();SaveUi();shell.Toast.Show("Default preferences restored; your files were kept");}catch(Exception ex){Engine.Log("Reset failed: "+ex.Message);shell.Toast.Show(ex is IOException?ex.Message:"Preferences could not be reset. Check the activity log.");}finally{busy=false;}await Refresh();
 }
 static void SaveUi(){try{preferences.Save(Engine.Home,shell,shell.Page);}catch{shell.Toast.Show("Window preferences couldn't be saved.");}}
 static void SystemChanged(object sender,UserPreferenceChangedEventArgs e){if(closed)return;shell.Dispatcher.BeginInvoke(new Action(delegate{Theme.Apply(preferences.Theme);ApplyBackdrop();}));}
 static void ApplyBackdrop(){try{var handle=new WindowInteropHelper(shell).Handle;int dark=Theme.Light?0:1;DwmSetWindowAttribute(handle,20,ref dark,4);int corners=2;DwmSetWindowAttribute(handle,33,ref corners,4);int backdrop=Theme.High?1:2;DwmSetWindowAttribute(handle,38,ref backdrop,4);}catch(DllNotFoundException){}}
 static async Task Act(bool second,bool opening,bool startup=false){
  if(busy||closed)return;busy=true;var card=second?shell.SecondProfile:shell.MainProfile;card.Loading(opening);
  foreach(var item in new[]{shell.MainProfile,shell.SecondProfile}){item.Open.IsEnabled=false;item.Quit.IsEnabled=false;}
  try{await Task.Run(delegate{if(opening)Engine.Launch(second,startup);else Engine.Quit(second);});shell.Toast.Show((second?"Second":"Main")+" profile "+(opening?(card.Running?"brought to front":"started"):"closed"));}
  catch(Exception ex){Engine.Log("Profile action failed: "+ex.Message);shell.Toast.Show("Discord couldn't "+(opening?"open":"close")+". Try again, or check the activity log.");}
  finally{card.EndLoading();busy=false;}
  await Refresh();
 }
 static async Task Change(string name,bool value){
  if(busy){shell.Toast.Show("Wait for the current profile action to finish.");await Refresh();return;}
  try {
   if(name=="StartApp"||name=="StartSecond"){
    if(name=="StartApp")Engine.Settings.StartApp=value;else Engine.Settings.StartSecond=value;
    Engine.Save();
   } else if(name=="SyncDiscord"||name=="SyncVencord") {
    await Task.Run(delegate{Engine.SetSharing(name=="SyncDiscord",value);});
   } else {
    if((name.StartsWith("CopyVencord",StringComparison.Ordinal)||name=="CopyQuickCss"||name=="SyncAudio"||name=="SyncGraphics"||name=="CopyThemes"||name=="KeepSecondRunning")&&Engine.GetInstance(true,Engine.Snapshot()).Count>0)
     throw new IOException("Quit the second profile first.");
    string previous=new JavaScriptSerializer().Serialize(Engine.Custom);
    try {
     switch(name){
      case "AutoCheckUpdates":Engine.Custom.AutoCheckUpdates=value;break;
      case "SyncAudio":Engine.Custom.SyncAudio=value;break;
      case "SyncGraphics":Engine.Custom.SyncGraphics=value;break;
      case "CopyVencordPlugins":Engine.Custom.CopyVencordPlugins=value;break;
      case "CopyVencordPluginSettings":Engine.Custom.CopyVencordPluginSettings=value;break;
      case "CopyVencordPreferences":Engine.Custom.CopyVencordPreferences=value;break;
      case "CopyVencordAppearance":Engine.Custom.CopyVencordAppearance=value;break;
      case "CopyQuickCss":Engine.Custom.CopyQuickCss=value;break;
      case "CopyThemes":Engine.Custom.CopyThemes=value;break;
      case "HideTaskbar":Engine.Custom.HideTaskbar=value;break;
      case "HideTray":Engine.Custom.HideTray=value;break;
      case "KeepSecondRunning":if(!BuildProfile.Personal)throw new IOException("Public releases require Dual Discord to stay running.");Engine.Custom.KeepSecondRunning=value;break;
      case "SeparateTray":Engine.Custom.SeparateTray=value;break;
      case "SeparateTaskbar":Engine.Custom.SeparateTaskbar=value;break;
      default:throw new IOException("Unknown option.");
     }
     Engine.SaveCustom();
    } catch {Engine.Custom=new JavaScriptSerializer().Deserialize<Customization>(previous);throw;}
   }
   lifecycle.Apply();shell.Toast.Show("Settings saved");Files();
  } catch(Exception ex) {
   Engine.Log("Setting change failed: "+ex.Message);
   shell.Toast.Show(ex is IOException?ex.Message:"The setting could not be saved. Try again.");
  }
  await Refresh();
 }
 static async Task Refresh(){
  if(refreshing||closed)return;refreshing=true;
  try{
   var values=await Task.Run(delegate{var all=Engine.Snapshot();return new[]{Engine.GetInstance(false,all),Engine.GetInstance(true,all)};});if(closed)return;
   lifecycle.UpdateSecond(values[1].Count>0);BindManagedSecond(values[1]);taskbar.Update(values[1],Engine.Custom.SeparateTaskbar);shell.SetStats(values[0],values[1],Directory.Exists(Engine.SecondData));shell.SetLifecycle(Engine.Custom,values[1].Count>0||busy);shell.SetOptions(Engine.Settings,values[1].Count>0||busy);shell.SetCustomization(Engine.Custom,StoragePath(Engine.ProfileRoot),values[1].Count>0||busy,Engine.Settings.SyncDiscord,Engine.Settings.SyncVencord,Engine.VencordAvailable());
   try{Engine.FindDiscord();missing=false;}catch{missing=true;}shell.Missing(missing);shell.SetInstallation(missing?"Discord was not found. Detect it or select the installed Discord.exe.":"Detected: "+StoragePath(Engine.DiscordRoot));shell.SetPolicy(PolicyService.BlockReason,!PolicyService.Ready);
   if(!busy&&!installingUpdate&&PolicyService.StopManagedSecond&&values[1].Count>0){busy=true;try{await Task.Run(()=>Engine.Quit(true));lifecycle.UpdateSecond(false);Engine.Log("Signed policy stopped managed Dualcord; main Discord preserved");shell.Toast.Show("Dualcord stopped: "+PolicyService.BlockReason);}finally{busy=false;}}
   if(busy)foreach(var card in new[]{shell.MainProfile,shell.SecondProfile}){card.Open.IsEnabled=false;card.Quit.IsEnabled=false;}
  }catch(Exception ex){Engine.Log("Status unavailable: "+ex.Message);if(!closed)shell.Toast.Show("Status is temporarily unavailable.");}
  finally{refreshing=false;}
 }
 sealed class PickerOwner:System.Windows.Forms.IWin32Window{public IntPtr Handle{get{return new WindowInteropHelper(shell).Handle;}}}
 static async Task ChooseStorage(){
  if(busy||updating)return;
  using(var picker=new System.Windows.Forms.FolderBrowserDialog{Description="Choose a new or empty folder for the second Discord profile",ShowNewFolderButton=true,SelectedPath=Path.GetDirectoryName(StoragePath(Engine.ProfileRoot))}){
   if(picker.ShowDialog(new PickerOwner())!=System.Windows.Forms.DialogResult.OK)return;
   busy=true;shell.Toast.Show("Copying and verifying the second profile…");await Refresh();
   try{await Task.Run(()=>Engine.RelocateSecond(picker.SelectedPath));Files();shell.Toast.Show("Profile folder updated. The original copy was kept.");}
   catch(Exception ex){Engine.Log("Profile relocation failed: "+ex.Message);shell.Toast.Show(ex is IOException?ex.Message:"The profile could not be copied. Its original folder is still selected.");}
   finally{busy=false;}await Refresh();
  }
 }
 static void Locate(){
  var dialog=new OpenFileDialog{Title="Locate the installed Discord",Filter="Discord application|Discord.exe",CheckFileExists=true};
  if(Directory.Exists(Engine.DiscordRoot))dialog.InitialDirectory=Engine.DiscordRoot;
  if(dialog.ShowDialog(shell)!=true)return;
  try{Engine.SelectInstallation(dialog.FileName);Files();shell.Missing(false);shell.SetInstallation("Detected: "+StoragePath(Engine.DiscordRoot));shell.Toast.Show("Discord installation saved");}catch(Exception ex){shell.Toast.Show(ex.Message);}
 }
 static async Task CheckUpdates(){
  if(updating)return;updating=true;shell.Updates.State("Checking GitHub…",true);
  try{var release=await Task.Run(()=>UpdateService.GetLatest());if(closed)return;availableRelease=release;updateSchedule.Succeeded(DateTime.UtcNow);Engine.Log("Update check succeeded: installed "+UpdateService.CurrentVersion+", latest "+release.Tag);bool newer=release.Version>UpdateService.ParseVersion(UpdateService.CurrentVersion);shell.Updates.State(newer?"Version "+release.Tag+" is available.":"You're up to date. Installed version: "+UpdateService.CurrentVersion,false,newer);if(newer&&updateSchedule.Announce(release.Tag)){shell.UpdateAlert.ShowRelease(release.Tag);lifecycle.Restore();}}
  catch(Exception ex){updateSchedule.Failed(DateTime.UtcNow,ex is UpdateRateLimitException);Engine.Log("Update check failed: "+ex.Message);if(!closed)shell.Updates.State(ex is IOException?ex.Message:"The update check could not be completed.");}
  finally{updating=false;}
 }
 static async Task InstallUpdate(){
  if(!BuildProfile.CanInstallPublicUpdate){shell.Toast.Show("This personal build keeps your extra options. Install a personal update manually; public packages enforce managed mode.");return;}
  if(busy){shell.Toast.Show("Wait for the current profile action to finish before updating.");return;}
  if(updating||availableRelease==null||availableRelease.Version<=UpdateService.ParseVersion(UpdateService.CurrentVersion))return;updating=true;installingUpdate=true;shell.Updates.State("Downloading and verifying the update…",true);
  try{
   string zip=await Task.Run(()=>UpdateService.Download(availableRelease));if(closed)return;
   string folder=Path.GetDirectoryName(zip),helper=Path.Combine(folder,"Dual Discord Updater.exe");File.Copy(Path.Combine(Engine.Home,"Dual Discord Updater.exe"),helper,false);
   using(var current=Process.GetCurrentProcess()){
    var request=new UpdateRequest{Target=Engine.Home,Package=zip,Sha256=availableRelease.Sha256,Version=availableRelease.Tag,ManagerPid=current.Id,ManagerStartTicks=current.StartTime.ToUniversalTime().Ticks};string file=Path.Combine(folder,"request.json");File.WriteAllText(file,new JavaScriptSerializer().Serialize(request));
    Process.Start(new ProcessStartInfo(helper,"--request \""+file+"\""){UseShellExecute=false,CreateNoWindow=true,WorkingDirectory=folder});
   }
   if(managedSecond!=null)managedSecond.PreserveForRestart();exitingForUpdate=true;lifecycle.AllowExit();shell.Close();
  }catch(Exception ex){if(!closed)shell.Updates.State(ex is IOException?ex.Message:"The update could not be installed. Try a writable app folder.",false,true);}
  finally{updating=false;installingUpdate=false;}
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
