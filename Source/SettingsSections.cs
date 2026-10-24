using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
namespace DualDiscord {
 // Native tab semantics keep keyboard navigation, selection and screen-reader roles.
 public sealed class SettingsSections:TabControl {
  readonly Dictionary<string,TabItem> sections=new Dictionary<string,TabItem>();
  public SettingsSections(){Style=Theme.Style("SettingsTabs");Ui.Name(this,"Settings sections");}
  public void Add(string name,string glyph,FrameworkElement content){
   var item=new TabItem{Content=content,Style=Theme.Style("SettingsTab")};Ui.Name(item,name+" settings");
   var header=new StackPanel{Orientation=Orientation.Horizontal,VerticalAlignment=VerticalAlignment.Center};
   var icon=Ui.Glyph(glyph,14);icon.Margin=new Thickness(0,0,Theme.MarkerInset,0);icon.SetBinding(TextBlock.ForegroundProperty,new Binding("Foreground"){Source=item});header.Children.Add(icon);
   var text=Ui.Text(name);text.VerticalAlignment=VerticalAlignment.Center;text.SetBinding(TextBlock.ForegroundProperty,new Binding("Foreground"){Source=item});header.Children.Add(text);item.Header=header;
   sections.Add(name,item);Items.Add(item);if(Items.Count==1)SelectedIndex=0;
  }
  public void Select(string name){TabItem item;if(!sections.TryGetValue(name,out item))throw new ArgumentException("Unknown Settings section.","name");SelectedItem=item;}
  public string SelectedSection {get{foreach(var pair in sections)if(pair.Value==SelectedItem)return pair.Key;return "";}}
 }
}
