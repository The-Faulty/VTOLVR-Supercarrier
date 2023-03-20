using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Harmony;
using System.Reflection;
using System.IO;
using UnityEngine.SceneManagement;

namespace VTOLVRSuperCarrier
{
  public class Main : VTOLMOD
  {
    //public CarrierCatapult playerCat;
    private bool patched = false;

    private static List<Actor> Carriers = new List<Actor>();

    public static GameObject CarrierCrew;
    private static ShooterHandler shooterHandler;

    // This method is run once, when the Mod Loader is done initialising this game object
    public override void ModLoaded()
    {
      //This is an event the VTOLAPI calls when the game is done loading a scene
      if (!patched)
      {
        HarmonyInstance harmony = HarmonyInstance.Create("the_faulty.align");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        patched = true;
        Debug.Log("Align has been patched");
        StartCoroutine(loadIndicatorAsync());
      }
      VTOLAPI.SceneLoaded += SceneChanged;
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
        Debug.Log("Couldn't find carrier crew");
      }
      CarrierCrew = Instantiate(handler.asset as GameObject);
      bundle.Unload(false);
      DontDestroyOnLoad(CarrierCrew);
      Debug.Log(CarrierCrew);
      Debug.Log("Carrier crew loaded");
      yield break;
    }

    public IEnumerator getCarriers()
    {
      Debug.Log("getCarriers");
      TargetManager tm = TargetManager.instance;
      Debug.Log(tm);
      Debug.Log(tm.allActors);
      Debug.Log(tm.alliedUnits);
      while (tm.allActors.Count < 5) yield return null;
      tm.allActors.ForEach(actor =>
      {
        Debug.Log("Supercarrier: " + actor);
        if (actor.iconType == UnitIconManager.MapIconTypes.Carrier)
        {
          Carriers.Add(actor);
          Debug.Log("Carrier found: " + actor);
        }
      });
      Debug.Log(Carriers[0].GetComponent<Transform>());
      CarrierCrew.transform.parent = Carriers[0].GetComponent<Transform>();
      CarrierCrew.transform.localPosition = new Vector3(0, 23.98f, 0);
      CarrierCrew.transform.localEulerAngles = new Vector3(0, 0, 0);
      CarrierCrew.SetActive(true);
      yield break;
    }

    private void SceneChanged(VTOLScenes scenes) 
    {
      //Carriers = null;
      if (scenes == VTOLScenes.Akutan || scenes == VTOLScenes.CustomMapBase || scenes == VTOLScenes.CustomMapBase_OverCloud) // If inside of a scene that you can fly in
      {
        Debug.Log("Flight Scene");
        StartCoroutine(getCarriers());
         //append this to work for all carriers in scene

      }
    }

    public static void setPlayerCat(CarrierCatapult cat)
    {
      //Need logic here to check which carrier the player requested from and then assign to specific deck crew
      Debug.Log("setPlayerCat");
      Transform Shooter = CarrierCrew.transform.Find("Crew/Shooter").transform;
      GameObject ShooterMain = Shooter.transform.Find("DeckCrewLights").gameObject;

      shooterHandler = ShooterMain.AddComponent<ShooterHandler>();
      shooterHandler.agent = Shooter;

      CrewNav nav = ShooterMain.AddComponent<CrewNav>();
      nav.CharacterTransform = Shooter;

      shooterHandler.navAgent = nav;
      shooterHandler.startAlign(cat, VTOLAPI.GetPlayersVehicleGameObject());
      Debug.Log("setPlayerCat Over");
    }

    public static void afterHook()
    {
      Debug.Log("afterHook");
      //AlignIndicator.SetActive(false);
    }
  }
}