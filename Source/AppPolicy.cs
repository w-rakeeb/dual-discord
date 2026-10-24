using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
namespace DualDiscord {
 public static class LifecycleRules {
  public static string CloseAction(string requested,bool explicitExit){return explicitExit?"Exit":requested=="Exit"||requested=="Minimize"?requested:"Tray";}
 }
 public sealed class PolicyEnvelope {public string Payload{get;set;}public string Signature{get;set;}}
 public sealed class PolicyDocument {public long Revision{get;set;}public string MinimumVersion{get;set;}public bool Disabled{get;set;}public string Message{get;set;}}
 public static class AppPolicy {
  public static PolicyDocument Verify(string json,string key,long minimumRevision){
   try {
    if(String.IsNullOrWhiteSpace(json)||json.Length>65536)throw new IOException("Invalid app policy.");
    var serializer=new JavaScriptSerializer{MaxJsonLength=65536};var envelope=serializer.Deserialize<PolicyEnvelope>(json);
    if(envelope==null||String.IsNullOrEmpty(envelope.Payload)||String.IsNullOrEmpty(envelope.Signature))throw new IOException("Unsigned app policy.");
    byte[] payload=Convert.FromBase64String(envelope.Payload),signature=Convert.FromBase64String(envelope.Signature);
    using(var rsa=new RSACryptoServiceProvider()){rsa.PersistKeyInCsp=false;rsa.FromXmlString(key);if(!rsa.VerifyData(payload,CryptoConfig.MapNameToOID("SHA256"),signature))throw new IOException("App policy signature is invalid.");}
    var value=serializer.Deserialize<PolicyDocument>(Encoding.UTF8.GetString(payload));
    if(value==null||value.Revision<1||value.Revision<minimumRevision||(value.Message??"").Length>512)throw new IOException("App policy revision or message is invalid.");
    UpdateService.ParseVersion(value.MinimumVersion);return value;
   }catch(IOException){throw;}catch(Exception ex){if(ex is FormatException||ex is ArgumentException||ex is CryptographicException||ex is InvalidOperationException)throw new IOException("App policy could not be verified.",ex);throw;}
  }
  public static string BlockReason(PolicyDocument doc,string current){
   if(doc==null)return "";
   if(doc.Disabled)return String.IsNullOrWhiteSpace(doc.Message)?"Dual Discord is currently unavailable. Check About for updates and release information.":doc.Message;
   if(UpdateService.ParseVersion(current)<UpdateService.ParseVersion(doc.MinimumVersion))return "Update required: install Dual Discord "+doc.MinimumVersion+" or later. "+(doc.Message??"");
   return "";
  }
 }
}
