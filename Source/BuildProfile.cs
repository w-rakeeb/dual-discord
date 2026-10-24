namespace DualDiscord {
 public static class BuildProfile {
  public static bool Personal {get{
#if PERSONAL
   return true;
#else
   return false;
#endif
  }}
  public static bool CanInstallPublicUpdate {get{return !Personal;}}
  public static string Label {get{return Personal?"Personal build":"Public build";}}
 }
}
