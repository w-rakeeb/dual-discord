using System;
using System.Windows;
using System.Windows.Controls;
namespace DualDiscord {
 public sealed class UpdateNotice:Card {
  readonly TextBlock title=Ui.Text("An update is available",Theme.TitleSize,"text.primary",true);
  public Action Install;
  public UpdateNotice(){MaxWidth=480;HorizontalAlignment=HorizontalAlignment.Right;VerticalAlignment=VerticalAlignment.Top;Margin=new Thickness(Theme.Padding,Theme.CaptionHeight+Theme.Gap,Theme.Padding,Theme.Padding);Visibility=Visibility.Collapsed;
   var panel=new StackPanel();panel.Children.Add(title);var description=Ui.Text("Install the latest version. Your accounts stay signed in and Discord keeps running.",Theme.BodySize,"text.secondary");description.Margin=new Thickness(0,Theme.MarkerInset,0,Theme.Gap);panel.Children.Add(description);
   var actions=new WrapPanel{HorizontalAlignment=HorizontalAlignment.Right};var later=Ui.Button("Later",false,delegate{Visibility=Visibility.Collapsed;});later.Margin=new Thickness(0,0,Theme.MarkerInset,0);actions.Children.Add(later);actions.Children.Add(Ui.Button("View update",true,delegate{Visibility=Visibility.Collapsed;if(Install!=null)Install();}));panel.Children.Add(actions);Child=panel;
  }
  public void ShowRelease(string version){title.Text="Dual Discord "+version+" is available";Visibility=Visibility.Visible;}
 }
}
