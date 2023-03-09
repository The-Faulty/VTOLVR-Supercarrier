using System;
using UnityEditor;
using HarmonyLib;
using UnityEngine;
using CatAlign;
using System.Reflection;

[HarmonyPatch(typeof(CarrierCatapult), nameof(CarrierCatapult.Hook))]
public class ExtendCatapultLaunch
{
  [HarmonyPrefix]
  //public static void Prefix(ref float ___launchTime)
  public static void Prefix(CarrierCatapult __instance)
  {
    Debug.Log("Extending launch time");
    //___launchTime = 7;
    Traverse traverse = Traverse.Create(__instance);
    if ((int)traverse.Field("launchTime").GetValue() < 10)
    {
      Debug.Log("Launch time under 10 seconds");
      traverse.Field("launchTime").SetValue(10);
    }
  }
}

[HarmonyPatch(typeof(AirportManager), nameof(AirportManager.PlayerRequestTakeoff))]
public class AssignCatapult
{
  public static void Postfix(AirportManager __instance)
  {
    Debug.Log("Assigning catapult");
    Traverse traverse = Traverse.Create(__instance);
    if ((bool)traverse.Field("isCarrier").GetValue())
    {
      Debug.Log("Calling setPlayerCatapult");
      Main.setPlayerCat((CarrierCatapult)traverse.Field("carrierCatapult").GetValue());
    }
  }
}

//typeof(CarrierCatapult).GetMethod("EndLaunch", BindingFlags.NonPublic).Name
//********Postfix cat launch to remove alignment script
[HarmonyPatch(typeof(CarrierCatapult), nameof(CarrierCatapult.Hook))]
public class afterHooked
{
  public static void Postfix()
  {
    Debug.Log("After Hooked");
    Main.afterHook();
  }
}