using HarmonyLib;
using UnityEngine;
using VTOLVRSupercarrier;
using System.Collections;
using System;

/*[HarmonyPatch(typeof(CarrierCatapult), nameof(CarrierCatapult.Hook))]
public class ExtendCatapultLaunch
{
  /*[HarmonyPrefix]
  //public static void Prefix(ref float ___launchTime)
  public static void Prefix(CarrierCatapult __instance)
  {
    Log("Extending launch time");
    Traverse traverse = Traverse.Create(__instance);
    if ((float)traverse.Field("launchTime").GetValue() < 10)
    {
      Log("Launch time under 10 seconds");
      traverse.Field("launchTime").SetValue(2f);
    }
  }
}*/

//[HarmonyPatch(typeof(AirportManager), nameof(AirportManager.PlayerRequestTakeoff))]
[HarmonyPatch(typeof(AICarrierSpawn), nameof(AICarrierSpawn.RegisterPlayerTakeoffRequest))]
public class AICarrierSpawnPatch
{
  public static void Postfix(AICarrierSpawn __instance, CarrierCatapult __result)
  {
    Debug.Log("AICarrierSpawnPatch: Assigning catapult");
    if (__result != null)
    {
      Debug.Log("AICarrierSpawnPatch: Calling setPlayerCatapult");
      Main.setPlayerCat(__instance, __result);
    }
    else
    {
      Debug.Log("AICarrierSpawnPatch: No catapult returned");
    }
  }
}

//this is kinda cursed but I can't do anything about it
[HarmonyPatch(typeof(CarrierCatapult), nameof(CarrierCatapult.Hook))]
public class CarrierCatapultPatch
{
  private static bool Prefix(CarrierCatapult __instance, CatapultHook hook, ref CatapultHook ___hook, ref Transform ___planeHookTransform, ref Rigidbody ___planeRb, ref FlightInfo ___flightInfo)
  {
    Log("Attempting to hook");
    Log("Traverse created");
    Log(__instance.hooked);
    if (!__instance.hooked)
    {
      __instance.hooked = true;
      ___hook = hook;
      ___planeHookTransform = hook.hookForcePointTransform;
      ___planeRb = hook.rb;
      ___flightInfo = hook.rb.GetComponentInChildren<FlightInfo>();
      if ((bool)___flightInfo)
      {
        ___flightInfo.PauseGCalculations();
      }
      __instance.StartCoroutine(CatapultRoutine(__instance));
      Log("I replaced the hook function");
    }
    return false;
  }

  private static IEnumerator CatapultRoutine(CarrierCatapult __instance)
  {
    Traverse traverse = Traverse.Create(__instance);
    Log("CarrierCatapultNew: Begin catapult routine");
    traverse.Field("catapultReady").SetValue(false);
    yield return new WaitForFixedUpdate();

    __instance.audioSource.PlayOneShot(__instance.latchSound);
    traverse.Field("catPosition").SetValue(traverse.Field("readyPos").GetValue());
    traverse.Field("catapultTransform").Property("position").SetValue(__instance.WorldPos(traverse.Field("readyPos").GetValue<Vector3>())); //catapultTransform.position = __instance.WorldPos(readyPos);
    __instance.CreateJoint();
    FloatingOrigin.instance.AddRigidbody((Rigidbody)traverse.Field("catRb").GetValue());
    Log("CarrierCatapultNew: Begin ReadyRoutine");
    yield return __instance.StartCoroutine(__instance.ReadyRoutine());

    Log("Ready Routine Over, Waiting: " + Time.time);
    yield return CatWait(__instance, Time.time);
    Log("Wait Over: " + Time.time);

    if ((bool)traverse.Field("hook").GetValue<CatapultHook>())
    {
      Log("CarrierCatapultNew: begin LaunchRoutine");
      __instance.audioSource.PlayOneShot(__instance.launchStartSound);
      yield return __instance.LaunchRoutine();
    }
    __instance.DestroyJoint();
    yield return new WaitForSeconds(1f);
    Log("CarrierCatapultNew: begin ReturnRoutine");
    yield return __instance.ReturnRoutine();
    traverse.Field("catapultReady").SetValue(true);
  }

  private static IEnumerator CatWait(CarrierCatapult __instance, float start)
  {
    Traverse traverse = Traverse.Create(__instance);
    while (Time.time - start <= 3)
    {
      traverse.Field("catPosition").SetValue(traverse.Field("launchStartPos").GetValue());
      traverse.Field("catRb").GetValue<Rigidbody>().MovePosition(__instance.WorldPos(traverse.Field("catPosition").GetValue<Vector3>()));
      yield return new WaitForFixedUpdate();
    }
  }

  private static void Log(object text)
  {
    Debug.Log("CarrierCatapultPatch: " + text);
  }
}

//Register AI Takeoff Request
/*[HarmonyPatch(typeof(AIPilot), nameof(AIPilot.TakeOffCarrier))]
class AITakeOffPatch
{
  public static void Prefix(AIPilot __instance, ref AICarrierSpawn ___carrier, ref int ___spawnIdx)
  {
    if (___carrier.usesCatapults)
    {
      CarrierCatapult catapult = ___carrier.spawnPoints[___spawnIdx].catapult;
      GameObject vehicle = __instance.gameObject;
      Main.setPlayerCat(___carrier, catapult, vehicle);
    }
  }
}

//https://gist.github.com/pardeike/c873b95e983e4814a8f6eb522329aee5 Make the ai plane stop just before the catapult to lower hook
/*[HarmonyPatch(typeof(AIPilot), nameof(AIPilot.CTO_TaxiToCatapult))]
class AIPilotCarrierTaxiPatch
{
  class SimpleEnumerator : IEnumerable
  {
    public IEnumerator enumerator;
    public Action prefixAction, postfixAction;
    public Action<object> preItemAction, postItemAction;
    public Func<object, object> itemAction;
    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    public IEnumerator GetEnumerator()
    {
      prefixAction();
      while (enumerator.MoveNext())
      {
        var item = enumerator.Current;
        preItemAction(item);
        yield return itemAction(item);
        postItemAction(item);
      }
      postfixAction();
    }
  }

  static void Postfix(ref IEnumerator __result)
  {
    Action prefixAction = () => { Console.WriteLine("--> beginning"); };
    Action postfixAction = () => { Console.WriteLine("--> ending"); };
    Action<object> preItemAction = (item) => { Console.WriteLine($"--> before {item}"); };
    Action<object> postItemAction = (item) => { Console.WriteLine($"--> after {item}"); };
    Func<object, object> itemAction = (item) =>
    {
      var newItem = item + "+";
      Console.WriteLine($"--> item {item} => {newItem}");
      return newItem;
    };
    var myEnumerator = new SimpleEnumerator()
    {
      enumerator = __result,
      prefixAction = prefixAction,
      postfixAction = postfixAction,
      preItemAction = preItemAction,
      postItemAction = postItemAction,
      itemAction = itemAction
    };
    __result = myEnumerator.GetEnumerator();
  }
}*/

//********Postfix cat launch to remove alignment script
/*[HarmonyPatch(typeof(CarrierCatapult), nameof(CarrierCatapult.Hook))]
public class afterHooked
{
  public static void Postfix()
  {
    Log("After Hooked");
    Main.afterHook();
  }
}*/

//[HarmonyPatch(typeof(CarrierCatapult), nameof(CarrierCatapult.LaunchRoutine))]
//Do stuff

/*[HarmonyPatch(typeof(RotationToggle), nameof(RotationToggle.Toggle))]
public class wingToggle
{
  public static void Prefix()
  {

  }
}

[HarmonyPatch(typeof(CarrierCatapult), nameof(CarrierCatapult.EndLaunch))]
public class endLaunch
{
  public static void Prefix()
  {
    //Reset the shooter/crew
  }
}*/