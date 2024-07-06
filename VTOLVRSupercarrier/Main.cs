using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Harmony;
using System.Reflection;
using UnityEngine.SceneManagement;

namespace VTOLVRSupercarrier
{
  public class Main : VTOLMOD
  {
    private bool patched = false;
    private bool isLoaded = false;

    List<Actor> Carriers = new List<Actor>();

    public static GameObject CarrierCrew;

    // This method is run once, when the Mod Loader is done initialising this game object
    public override void ModLoaded()
    {
      //This is an event the VTOLAPI calls when the game is done loading a scene
      if (!patched)
      {
        HarmonyInstance harmony = HarmonyInstance.Create("the_faulty.supercarrier");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        patched = true;
        Log("Supercarrier has been patched");
        StartCoroutine(loadIndicatorAsync());
      }

      VTOLAPI.SceneLoaded += SceneChanged;
      VTOLAPI.MissionReloaded += MissionReloaded;
      SceneManager.sceneUnloaded += SceneUnload;
      base.ModLoaded();
    }

    private IEnumerator loadIndicatorAsync() //thank you NotBDArmory github
    {
      AssetBundleCreateRequest a = AssetBundle.LoadFromFileAsync(ModFolder + "/carriercrew.assets");
      yield return a;
      AssetBundle bundle = a.assetBundle;
      AssetBundleRequest handler = bundle.LoadAssetAsync("CarrierCrew.prefab");
      yield return handler;
      if (handler.asset == null)
      {
        Log("Couldn't find carrier crew");
      }
      CarrierCrew = Instantiate(handler.asset as GameObject);
      CarrierCrew.name = "CarrierCrew";
      Transform Shooter = CarrierCrew.transform.Find("Crew/Shooter").transform;
      GameObject ShooterMain = Shooter.transform.Find("DeckCrewLights").gameObject;

      CrewManager crewManager = CarrierCrew.AddComponent<CrewManager>();

      ShooterHandler shooterHandler = ShooterMain.AddComponent<ShooterHandler>();
      shooterHandler.agent = Shooter;
      shooterHandler.Manager = crewManager;

      CrewNav nav = ShooterMain.AddComponent<CrewNav>();
      nav.CharacterTransform = Shooter;
      shooterHandler.navAgent = nav;

      bundle.Unload(false);
      DontDestroyOnLoad(CarrierCrew);
      Log(CarrierCrew);
      Log("Carrier crew loaded");
      CarrierCrew.SetActive(false);
      yield break;
    }

    public IEnumerator loadSupercarrier()
    {
      Log("getCarriers");
      Carriers.Clear();
      TargetManager tm = TargetManager.instance;
      while (tm.alliedUnits.Count < 5) yield return new WaitForSeconds(1f);
      tm.alliedUnits.ForEach(actor =>
      {
        Log(actor);
        if (actor.iconType == UnitIconManager.MapIconTypes.Carrier)
        {
          Log("Carrier found: " + actor);
          Carriers.Add(actor);

          GameObject clone = Instantiate(CarrierCrew);  //Should add crew to every carrier now
          clone.name = "CarrierCrew";
          clone.transform.parent = actor.gameObject.GetComponent<Transform>();
          clone.transform.localPosition = new Vector3(0, 23.98f, 0);
          clone.transform.localEulerAngles = new Vector3(0, 0, 0);
          clone.transform.GetComponentInChildren<CrewManager>().carrier = actor.gameObject.GetComponent<AICarrierSpawn>();
          clone.SetActive(true);
          Log("Added " + clone + " to " + actor);
        }
      });
      Log(Carriers[0].GetComponent<Transform>());
      isLoaded = true;
      yield break;
    }

    private void SceneChanged(VTOLScenes scenes) 
    {
      Log("Scene changed");
      if (scenes == VTOLScenes.Akutan || scenes == VTOLScenes.CustomMapBase || scenes == VTOLScenes.CustomMapBase_OverCloud) // If inside of a scene that you can fly in
      {
        Log("Flight Scene");
        StartCoroutine(loadSupercarrier());
      }
    }
    private void MissionReloaded()
    {
      Log("Mission Reloaded");
      StartCoroutine(loadSupercarrier());
    }
    private void SceneUnload(Scene s)
    {
      if (isLoaded)
      {
        foreach (Actor carrier in Carriers) //Remove each carrier crew when the scene unloads
        {
          Destroy(carrier.transform.Find("CarrierCrew"));
        }
      }
    }
    

    //rename to addTakeoffRequest
    public static void setPlayerCat(AICarrierSpawn instance, CarrierCatapult cat, GameObject vehicle = null)
    {
      //(might be fixed) Need logic here to check which carrier the player requested from and then assign to specific deck crew
      GameObject localCarrier = instance.gameObject;
      Log("Player takeoff request on " + localCarrier);

      CrewManager manager = localCarrier.GetComponentInChildren<CrewManager>();

      manager.takeoffRequest(cat, vehicle);
      Log("setPlayerCat Over");
    }

    /*public static void afterHook()
    {
      Log("afterHook");
      //AlignIndicator.SetActive(false);
    }*/

    //Override the VTOLMOD.Log function because it doesn't work with static methods
    private static new void Log(object text)
    {
      Debug.Log("VTOLVR-Supercarrier: " + text);
    }
  }
}