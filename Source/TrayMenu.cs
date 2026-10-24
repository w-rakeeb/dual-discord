using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Forms=System.Windows.Forms;
namespace DualDiscord {
 public static class TrayMenu {
  public static Forms.ContextMenuStrip Create(){
   var menu=new Forms.ContextMenuStrip{ShowImageMargin=false,ShowCheckMargin=false,Padding=new Forms.Padding(6),Renderer=new TrayMenuRenderer(),Font=new Font("Segoe UI",10f),DropShadowEnabled=true};
   menu.Opening+=delegate{Apply(menu);};return menu;
  }
  public static void Apply(Forms.ContextMenuStrip menu){menu.BackColor=ColorFor("bg.surface");menu.ForeColor=ColorFor("text.primary");foreach(Forms.ToolStripItem item in menu.Items){item.ForeColor=menu.ForeColor;if(!(item is Forms.ToolStripSeparator)){item.AutoSize=false;item.Size=new Size(232,32);}}}
  public static Color ColorFor(string token){
   var brush=Theme.Resources[token] as System.Windows.Media.SolidColorBrush;
   var color=brush==null?(System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(Theme.Palette[token][Theme.Light?1:0]):brush.Color;
   return Color.FromArgb(color.A,color.R,color.G,color.B);
  }
 }
 public sealed class TrayMenuRenderer:Forms.ToolStripProfessionalRenderer {
  protected override void OnRenderToolStripBackground(Forms.ToolStripRenderEventArgs e){using(var brush=new SolidBrush(TrayMenu.ColorFor("bg.surface")))e.Graphics.FillRectangle(brush,e.AffectedBounds);}
  protected override void OnRenderMenuItemBackground(Forms.ToolStripItemRenderEventArgs e){
   if(!e.Item.Selected||!e.Item.Enabled)return;var r=new Rectangle(0,0,e.Item.Width-1,e.Item.Height-1);
   using(var path=Rounded(r,6))using(var brush=new SolidBrush(TrayMenu.ColorFor(Theme.High?"accent.default":"bg.surfaceHover"))){e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;e.Graphics.FillPath(brush,path);}
  }
  protected override void OnRenderItemText(Forms.ToolStripItemTextRenderEventArgs e){e.TextColor=TrayMenu.ColorFor(!e.Item.Enabled?"text.disabled":Theme.High&&e.Item.Selected?"accent.onAccent":"text.primary");base.OnRenderItemText(e);}
  protected override void OnRenderToolStripBorder(Forms.ToolStripRenderEventArgs e){using(var pen=new Pen(TrayMenu.ColorFor("border.subtle")))e.Graphics.DrawRectangle(pen,0,0,e.ToolStrip.Width-1,e.ToolStrip.Height-1);}
  protected override void OnRenderSeparator(Forms.ToolStripSeparatorRenderEventArgs e){using(var pen=new Pen(TrayMenu.ColorFor("border.subtle")))e.Graphics.DrawLine(pen,8,e.Item.Height/2,e.Item.Width-8,e.Item.Height/2);}
  static GraphicsPath Rounded(Rectangle r,int radius){var path=new GraphicsPath();int d=radius*2;path.AddArc(r.X,r.Y,d,d,180,90);path.AddArc(r.Right-d,r.Y,d,d,270,90);path.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);path.AddArc(r.X,r.Bottom-d,d,d,90,90);path.CloseFigure();return path;}
 }
}
