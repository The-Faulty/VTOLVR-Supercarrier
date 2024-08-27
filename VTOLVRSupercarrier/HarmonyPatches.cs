using HarmonyLib;
using UnityEngine;
using VTOLVRSupercarrier;
using System.Collections;
using System;
using VTOLVRSupercarrier.CrewScripts;

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
/*[HarmonyPatch(typeof(AICarrierSpawn), nameof(AICarrierSpawn.RegisterPlayerTakeoffRequest))]
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
}*/

//this is kinda cursed but I can't do anything about it
[HarmonyPatch(typeof(CarrierCatapult), nameof(CarrierCatapult.Hook))]
public class CarrierCatapultPatch
{
  private static bool Prefix(CarrierCatapult __instance, CatapultHook hook, ref CatapultHook ___hook, ref Transform ___planeHookTransform, ref Rigidbody ___planeRb, ref FlightInfo ___flightInfo)
  {
    Log("Attempting to hook");
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
    Log("CarrierCatapultNew: Begin catapult routine");
    __instance.catapultReady = false;
    yield return CarrierCatapult.fixedWait;

    __instance.audioSource.PlayOneShot(__instance.latchSound);
    __instance.catPosition = __instance.readyPos;
    __instance.catapultTransform.position = __instance.WorldPos(__instance.readyPos);
    __instance.CreateJoint();
    FloatingOrigin.instance.AddRigidbody(__instance.catRb);
    //yield return new WaitForSeconds(2f);
    Log("CarrierCatapultNew: Begin ReadyRoutine");
    yield return __instance.StartCoroutine(__instance.ReadyRoutine());

    Log("Ready Routine Over, Waiting: " + Time.time);
    yield return CatWait(__instance, Time.time);
    Log("Wait Over: " + Time.time);

    if (__instance.hook)
    {
      Log("CarrierCatapultNew: begin LaunchRoutine");
      __instance.audioSource.PlayOneShot(__instance.launchStartSound);
      yield return __instance.LaunchRoutine();
    }
    __instance.DestroyJoint();
    yield return new WaitForSeconds(1f);
    Log("CarrierCatapultNew: begin ReturnRoutine");
    yield return __instance.ReturnRoutine();
    __instance.catapultReady = true;
  }

  private static IEnumerator CatWait(CarrierCatapult __instance, float start)
  {
    //Traverse traverse = Traverse.Create(__instance);
    while (Time.time - start <= 3)
    {
      __instance.catPosition = __instance.launchStartPos;
      __instance.catRb.MovePosition(__instance.WorldPos(__instance.catPosition));
      //traverse.Field("catPosition").SetValue(traverse.Field("launchStartPos").GetValue());
      //traverse.Field("catRb").GetValue<Rigidbody>().MovePosition(__instance.WorldPos(traverse.Field("catPosition").GetValue<Vector3>()));
      yield return new WaitForFixedUpdate();
    }
  }

  private static void Log(object text)
  {
    Debug.Log("CarrierCatapultPatch: " + text);
  }
}

// Trying to use AICarrier method instead
/*[HarmonyPatch(typeof(AIPilot), nameof(AIPilot.LandAtAirport))]
public class AILandAtCarrier
{
  public static void Postfix(AIPilot __instance, AirportManager airport)
  {
    if (__instance.commandState == AIPilot.CommandStates.Land && airport.isCarrier)
    {
      // Trigger landing routines on carrier where pilot is landing
      CatapultCrewManager[] crews = airport.gameObject.GetComponentsInChildren<CatapultCrewManager>();
      foreach (var crew in crews)
      {
        crew.LandingTrigger();
      }
    }
  }
}*/

[HarmonyPatch(typeof(AICarrierSpawn), nameof(AICarrierSpawn.BeginLandingMode))]
public class CarrierLandingModePatch
{
  public static void Postfix(AICarrierSpawn __instance)
  {
    CatapultCrewManager[] crews = __instance.GetComponentsInChildren<CatapultCrewManager>();
    foreach (var crew in crews)
    {
      crew.LandingTrigger();
    }
  }
}

[HarmonyPatch(typeof(AICarrierSpawn), nameof(AICarrierSpawn.CheckLandingMode))]
public class CarrierCheckLandingModePatch
{
  public static void Postfix(AICarrierSpawn __instance)
  {
    //bool isInQueue = __instance.landingMode;
    CatapultCrewManager[] crews = __instance.GetComponentsInChildren<CatapultCrewManager>();
    foreach (var crew in crews)
    {
      if (__instance.landingMode)
      {
        crew.LandingTrigger();
        continue;
      }
      crew.ResetTrigger();
    }
  }
}


[HarmonyPatch(typeof(OpticalLandingSystem), nameof(OpticalLandingSystem.Update))]
public class OLSUpdatePatch
{
  public static bool Prefix(OpticalLandingSystem __instance)
  {
    Transform transform = __instance.targetTransform;
    if (__instance.usePlayer && FlightSceneManager.instance.playerActor)
    {
      if (!__instance.playerHook && FlightSceneManager.instance && FlightSceneManager.instance.playerActor)
      {
        __instance.playerHook = FlightSceneManager.instance.playerActor.GetComponentInChildren<Tailhook>(true);
      }
      if (__instance.playerHook)
      {
        transform = __instance.playerHook.hookPointTf;
      }
      else
      {
        transform = FlightSceneManager.instance.playerActor.transform;
      }
    }
    if (!__instance.wavingOff)
    {
      if (transform)
      {
        Vector3 vector = transform.position - __instance.sensorTransform.position;
        float sqrMagnitude = vector.sqrMagnitude;
        if (sqrMagnitude >= __instance.maxSqrDist || sqrMagnitude <= __instance.minSqrDist || Vector3.Dot(__instance.sensorTransform.forward, vector.normalized) <= __instance.minDot)
        {
          __instance.displayObject.SetActive(false);
          return false;
        }
        __instance.displayObject.SetActive(true);
        Vector3 vector2 = __instance.sensorTransform.InverseTransformPoint(transform.position);
        vector2.x = 0f;
        int num = Mathf.RoundToInt(Mathf.Clamp(VectorUtils.SignedAngle(Vector3.forward, vector2, -Vector3.up) / __instance.maxOffset, -1f, 1f) * (float)(__instance.flareCount / 2));
        int num2 = num + __instance.flareCount / 2;
        if (num2 < 0)
        {
          num2 = 0;
        }
        else if (num2 >= __instance.flareCount)
        {
          num2 = __instance.flareCount - 1;
        }
        for (int i = 0; i < __instance.flareCount; i++)
        {
          if (i == num2)
          {
            __instance.ballFlares[i].SetActive(true);
          }
          else
          {
            __instance.ballFlares[i].SetActive(false);
          }
        }
        if (__instance.datumObject)
        {
          __instance.datumObject.SetActive(num2 != __instance.flareCount - 1);
        }
        if (__instance.usePlayer && __instance.playerHook && __instance.playerHook.isDeployed)
        {
          OpticalLandingSystem.OLSData olsdata = new OpticalLandingSystem.OLSData
          {
            ball = num
          };
          __instance.playerHook.SendOLSData(olsdata);
          CatapultCrewManager[] crews = __instance.GetComponentInParent<AICarrierSpawn>().GetComponentsInChildren<CatapultCrewManager>();
          foreach (var crew in crews)
          {
            crew.LandingTrigger(__instance.playerHook.GetComponentInParent<VehicleMaster>());
          }
          return false;
        }
      }
      else
      {
        __instance.displayObject.SetActive(false);
        CatapultCrewManager[] crews = __instance.GetComponentInParent<AICarrierSpawn>().GetComponentsInChildren<CatapultCrewManager>();
        foreach (var crew in crews)
        {
          crew.ResetTrigger();
        }
      }
    }
    return false;
  }
}

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