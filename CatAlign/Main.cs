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

namespace CatAlign
{
  public class Main : VTOLMOD
  {
    //public CarrierCatapult playerCat;
    private bool patched = false;

    public static GameObject AlignIndicator;
    public static GameObject AlignIndicatorUI;

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
      //VTOLAPI.SceneLoaded += SceneLoaded;
      base.ModLoaded();
    }

    private IEnumerator loadIndicatorAsync() //thank you NotBDArmory github
    {
      AssetBundleCreateRequest a = AssetBundle.LoadFromFileAsync(ModFolder + "/alignmentindicator.assets");
      yield return a;
      AssetBundle bundle = a.assetBundle;
      AssetBundleRequest handler = bundle.LoadAssetAsync("AlignmentIndicator.prefab");
      yield return handler;
      if (handler.asset == null)
      {
        Debug.Log("Couldn't find alignment indicator");
      }
      AlignIndicator = Instantiate(handler.asset as GameObject);
      DontDestroyOnLoad(AlignIndicator);
      Debug.Log("Alignment Indicator loaded");
      bundle.Unload(false);
      yield break;
    }

    public static void setPlayerCat(CarrierCatapult pc)
    {
      Debug.Log("setPlayerCat has been called");
      GameObject currentVehicle = VTOLAPI.GetPlayersVehicleGameObject();

      Debug.Log(currentVehicle);
      Debug.Log(currentVehicle.name);
      if (currentVehicle.name.Equals("SEVTF(Clone)"))
      {
        Transform hookForcePt = currentVehicle.transform.Find("LandingGear").transform.Find("hookForcePt").transform;
        Debug.Log("Help me im dying");
        Debug.Log(AlignIndicator);
        if (currentVehicle.transform.Find(AlignIndicator.name) == null)
        {
          Debug.Log("Alignment Indicator not found in vehicle");
          AlignIndicator.transform.parent = currentVehicle.transform;
          AlignIndicator.transform.localPosition = new Vector3(0.125f, 0.575f, 6.125f);
          AlignIndicator.transform.localEulerAngles = new Vector3(25, 0, 0);
          AlignIndicatorUI = AlignIndicator.transform.Find("UI").gameObject;
          AlignIndicatorUI.AddComponent<UIHandler>();
        }
        Debug.Log("Oh god, please, it hurts");
        AlignIndicator.SetActive(true);

        RectTransform backgroundUI = (RectTransform)AlignIndicatorUI.transform.Find("Background").transform;
        RectTransform playerUI = (RectTransform)backgroundUI.transform.Find("playerUI").transform;

        UIHandler handler = AlignIndicatorUI.GetComponent<UIHandler>();
        handler.characterTarget = hookForcePt;
        handler.gameTarget = pc.transform;

        if (handler.characterTarget == null || handler.gameTarget == null)
        {
          Debug.Log("Could not assign target element(s)");
        }

        handler.backgroundUI = backgroundUI;
        handler.playerUI = playerUI;

        if (handler.backgroundUI == null || handler.playerUI == null)
        {
          Debug.Log("Could not assign UI element(s)");
        }

        handler.angleDisplay = (Text)AlignIndicatorUI.transform.Find("Info Panel/Angle Text/Angle Display").gameObject.GetComponent<Text>();
        handler.alignDisplay = (Text)AlignIndicatorUI.transform.Find("Info Panel/Align Text/Align Display").gameObject.GetComponent<Text>();
        handler.moveDisplay = (Text)AlignIndicatorUI.transform.Find("Info Panel/Move Text/Move Display").gameObject.GetComponent<Text>();
        if (handler.moveDisplay == null || handler.alignDisplay == null || handler.alignDisplay == null)
        {
          Debug.Log("Could not assign text element(s)");
        }
      }
      else
      {
        Debug.Log("Wrong plane bozo");
      }
    }

    public static void afterHook()
    {
      Debug.Log("afterHook");
      AlignIndicator.SetActive(false);
    }
  }
}