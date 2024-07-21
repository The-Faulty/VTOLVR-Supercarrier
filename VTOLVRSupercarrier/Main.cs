using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using ModLoader.Framework;
using ModLoader.Framework.Attributes;
using System.Reflection;
using UnityEngine.SceneManagement;
using VTOLAPI;
using Mod_Loader.Classes;
using System.IO;
using VTOLVRSupercarrier.CrewScripts;

namespace VTOLVRSupercarrier
{
  [ItemId("the_faulty-supercarrier")]
  public class Main : VtolMod
  {
    private bool patched = false;
    private bool isLoaded = false;
    private string ModFolder;

    List<Actor> Carriers = new List<Actor>();

    public static GameObject CarrierCrew;

    public void Awake()
    {
      ModFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      StartCoroutine(LoadIndicatorAsync());
      VTAPI.SceneLoaded += SceneChanged;
      VTAPI.MissionReloaded += MissionReloaded;
      SceneManager.sceneUnloaded += SceneUnload;
    }

    private IEnumerator LoadIndicatorAsync() //thank you NotBDArmory github
    {
      Log("Loading Asset Bundle");
      AssetBundleCreateRequest a = AssetBundle.LoadFromFileAsync(ModFolder + "/carriercrew.assets");
      yield return a;
      AssetBundle bundle = a.assetBundle;
      AssetBundleRequest handler = bundle.LoadAssetAsync("carriercrew.prefab");
      yield return handler;
      if (handler.asset == null)
      {
        Log("Couldn't find carrier crew");
      }
      Log("Asset Bundle Loaded");
      CarrierCrew = Instantiate(handler.asset as GameObject);
      CarrierCrew.name = "CarrierCrew";

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
          //clone.transform.GetComponentInChildren<CrewManager>().carrier = actor.gameObject.GetComponent<AICarrierSpawn>();
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
    

    /*public static void setPlayerCat(AICarrierSpawn instance, CarrierCatapult cat, GameObject vehicle = null)
    {
      //(might be fixed) Need logic here to check which carrier the player requested from and then assign to specific deck crew
      GameObject localCarrier = instance.gameObject;
      Log("Player takeoff request on " + localCarrier);

      CrewManager manager = localCarrier.GetComponentInChildren<CrewManager>();

      manager.takeoffRequest(cat, vehicle);
      Log("setPlayerCat Over");
    }*/

    public override void UnLoad()
    {
      // Unload
    }
    //Override the VTOLMOD.Log function because it doesn't work with static methods
    private static void Log(object text)
    {
      Debug.Log("VTOLVR-Supercarrier: " + text);
    }

  }
}