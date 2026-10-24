using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using Forms=System.Windows.Forms;
namespace DualDiscord {
 public sealed class WindowLifecycle:IDisposable {
  readonly Window window;readonly Forms.NotifyIcon tray;readonly System.Drawing.Icon icon;readonly Func<Customization> settings;bool exitRequested,disposed;
  public Action ExitRequested,SecondOpenRequested,SecondQuitRequested;
  public WindowLifecycle(Window owner,string home,Func<Customization> options){window=owner;settings=options;icon=new System.Drawing.Icon(Path.Combine(home,"Assets","Dual Discord.ico"));tray=new Forms.NotifyIcon{Text="Dual Discord",Icon=icon};
   var menu=new Forms.ContextMenuStrip();menu.Items.Add("Open Dual Discord",null,delegate{Restore();});menu.Items.Add("Open second Discord",null,delegate{if(SecondOpenRequested!=null)SecondOpenRequested();});menu.Items.Add("Quit second Discord",null,delegate{if(SecondQuitRequested!=null)SecondQuitRequested();});menu.Items.Add(new Forms.ToolStripSeparator());menu.Items.Add("Exit Dual Discord",null,delegate{if(ExitRequested!=null)ExitRequested();});tray.ContextMenuStrip=menu;tray.DoubleClick+=delegate{Restore();};window.Closing+=Closing;Apply();
   owner.SourceInitialized+=delegate{HwndSource.FromHwnd(new WindowInteropHelper(owner).Handle).AddHook(WindowsMessage);};
  }
  IntPtr WindowsMessage(IntPtr handle,int msg,IntPtr w,IntPtr l,ref bool handled){if(msg==0x11)exitRequested=true;return IntPtr.Zero;}
  void Closing(object sender,CancelEventArgs e){string action=LifecycleRules.CloseAction(settings().CloseAction,exitRequested);if(action=="Exit")return;e.Cancel=true;if(action=="Minimize")window.WindowState=WindowState.Minimized;else{window.Hide();if(tray.Visible){tray.BalloonTipTitle="Dual Discord is still running";tray.BalloonTipText="Use the tray menu to reopen or exit. Your profile settings control whether Discord stays running.";tray.ShowBalloonTip(2500);}}}
  public void Apply(){if(disposed)return;window.ShowInTaskbar=!settings().HideTaskbar;tray.Visible=!settings().HideTray;}
  public void Restore(){if(disposed)return;Theme.Resources["KeyboardFocusVisibility"]=Visibility.Collapsed;window.Show();window.WindowState=WindowState.Normal;window.ShowInTaskbar=!settings().HideTaskbar;bool top=window.Topmost;window.Topmost=true;window.Activate();window.Topmost=top;}
  public void AllowExit(){exitRequested=true;}
  public void Dispose(){if(disposed)return;disposed=true;window.Closing-=Closing;tray.Visible=false;if(tray.ContextMenuStrip!=null)tray.ContextMenuStrip.Dispose();tray.Dispose();icon.Dispose();}
 }
}
