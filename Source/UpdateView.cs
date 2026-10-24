using System;
using System.Windows;
using System.Windows.Controls;

namespace DualDiscord {
 public sealed class UpdateView:Card {
  public Action CheckRequested,InstallRequested;
  readonly TextBlock status=Ui.Text("Check GitHub for a newer stable release.",Theme.BodySize,"text.secondary");
  readonly Button check,install;
  public UpdateView(){var panel=new StackPanel();panel.Children.Add(Ui.Text("Dual Discord "+UpdateService.CurrentVersion,Theme.TitleSize,"text.primary",true));status.Margin=new Thickness(0,Theme.MarkerInset,0,Theme.Gap);panel.Children.Add(status);var actions=new WrapPanel();check=Ui.Button("Check for updates",false,delegate{if(CheckRequested!=null)CheckRequested();});install=Ui.Button("Download and install",true,delegate{if(InstallRequested!=null)InstallRequested();});install.Margin=new Thickness(Theme.MarkerInset,0,0,0);install.Visibility=Visibility.Collapsed;actions.Children.Add(check);actions.Children.Add(install);panel.Children.Add(actions);var hint=Ui.Text("Updates come from w-rakeeb/dual-discord. Your profiles and settings are kept. Only this manager restarts.",Theme.NoteSize,"text.muted");hint.Margin=new Thickness(0,Theme.Gap,0,0);panel.Children.Add(hint);Child=panel;}
  public void State(string message,bool busy=false,bool available=false){status.Text=message;check.IsEnabled=install.IsEnabled=!busy;check.Content=busy?"Please wait…":"Check for updates";install.Visibility=available?Visibility.Visible:Visibility.Collapsed;}
 }
}
