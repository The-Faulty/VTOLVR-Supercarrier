using System;
using UnityEditor;
using Harmony;
using UnityEngine;
using VTOLVRSuperCarrier;
using System.Reflection;

[HarmonyPatch(typeof(CarrierCatapult), nameof(CarrierCatapult.Hook))]
public class ExtendCatapultLaunch
{
  /*[HarmonyPrefix]
  //public static void Prefix(ref float ___launchTime)
  public static void Prefix(CarrierCatapult __instance)
  {
    Debug.Log("Extending launch time");
    Traverse traverse = Traverse.Create(__instance);
    if ((float)traverse.Field("launchTime").GetValue() < 10)
    {
      Debug.Log("Launch time under 10 seconds");
      traverse.Field("launchTime").SetValue(2f);
    }
  }*/
}

//[HarmonyPatch(typeof(AirportManager), nameof(AirportManager.PlayerRequestTakeoff))]
[HarmonyPatch(typeof(AICarrierSpawn), nameof(AICarrierSpawn.RegisterPlayerTakeoffRequest))]
public class AssignCatapult
{
  public static void Postfix(ref CarrierCatapult __result)
  {
    Debug.Log("Assigning catapult");
    //Traverse traverse = Traverse.Create(__instance);
    //if ((bool)traverse.Field("isCarrier").GetValue())
    if (__result != null)
    {
      Debug.Log("Calling setPlayerCatapult");
      Main.setPlayerCat(__result);
      //Main.setPlayerCat((CarrierCatapult)traverse.Field("carrierCatapult").GetValue());
    } else
    {
      Debug.Log("false");
    }
  }
}

//********Postfix cat launch to remove alignment script
/*[HarmonyPatch(typeof(CarrierCatapult), nameof(CarrierCatapult.Hook))]
public class afterHooked
{
  public static void Postfix()
  {
    Debug.Log("After Hooked");
    Main.afterHook();
  }
}*/

[HarmonyPatch(typeof(CarrierCatapult), nameof(CarrierCatapult.LaunchRoutine))]
//Do stuff

/*[HarmonyPatch(typeof(RotationToggle), nameof(RotationToggle.Toggle))]
public class wingToggle
{
  public static void Prefix()
  {

  }
}*/

[HarmonyPatch(typeof(CarrierCatapult), nameof(CarrierCatapult.EndLaunch))] 
public class endLaunch
{
  public static void Prefix()
  {
    //Reset the shooter/crew
  }
}