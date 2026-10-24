using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Media;
namespace DualDiscord {
 // Render bundled SVG geometry directly so UI logos remain sharp at every DPI.
 public static class LogoAssets {
  static readonly System.Collections.Generic.Dictionary<string,ImageSource> Cache=new System.Collections.Generic.Dictionary<string,ImageSource>(StringComparer.OrdinalIgnoreCase);
  public static ImageSource Load(string file){string svg=Path.ChangeExtension(file,"svg");ImageSource image;if(Cache.TryGetValue(svg,out image))return image;
   var xml=new XmlDocument();xml.XmlResolver=null;xml.Load(svg);var path=xml.SelectSingleNode("//*[local-name()='path']");var geometry=Geometry.Parse("F1 "+path.Attributes["d"].Value);var drawing=new DrawingGroup();
   if(Path.GetFileName(svg).Equals("Discord.svg",StringComparison.OrdinalIgnoreCase))drawing.Children.Add(new GeometryDrawing((Brush)new BrushConverter().ConvertFromString("#5865F2"),null,geometry));
   else {
    var rect=xml.SelectSingleNode("//*[local-name()='rect']");drawing.Children.Add(new GeometryDrawing(BrushOf(rect,"fill"),null,new RectangleGeometry(new System.Windows.Rect(Number(rect,"x"),Number(rect,"y"),Number(rect,"width"),Number(rect,"height")),Number(rect,"rx"),Number(rect,"rx"))));
    foreach(XmlNode layer in xml.SelectNodes("//*[local-name()='use']")){double[] values=Regex.Matches(layer.Attributes["transform"].Value,@"[-\d.]+").Cast<Match>().Select(m=>Double.Parse(m.Value,CultureInfo.InvariantCulture)).ToArray();var group=new DrawingGroup();var transform=new TransformGroup();transform.Children.Add(new ScaleTransform(values[2],values[2]));transform.Children.Add(new TranslateTransform(values[0],values[1]));group.Transform=transform;Pen pen=layer.Attributes["stroke"]==null?null:new Pen(BrushOf(layer,"stroke"),Number(layer,"stroke-width")){LineJoin=PenLineJoin.Round};group.Children.Add(new GeometryDrawing(BrushOf(layer,"fill"),pen,geometry));drawing.Children.Add(group);}
   }
   image=new DrawingImage(drawing);image.Freeze();Cache[svg]=image;return image;
  }
  static Brush BrushOf(XmlNode node,string name){return (Brush)new BrushConverter().ConvertFromString(node.Attributes[name].Value);}
  static double Number(XmlNode node,string name){return Double.Parse(node.Attributes[name].Value,CultureInfo.InvariantCulture);}
 }
}
