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

namespace CatAlign
{
  public class Main : VTOLMOD
  {
    //public CarrierCatapult playerCat;
    private bool patched = false;

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
      AssetBundleCreateRequest a = AssetBundle.LoadFromFileAsync(ModFolder + "/align.assets");
      yield return a;
      AssetBundle bundle = a.assetBundle;
      AssetBundleRequest handler = bundle.LoadAssetAsync("AlignmentIndicator.prefab");
      yield return handler;
      if (handler.asset == null)
      {
        Debug.Log("Couldn't find alignment indicator");
      }
      //0.13, 0.575, 6.125
      //25,0,0
      yield break;
    }

    //This method is called every frame by Unity. Here you'll probably put most of your code
    void Update()
    {
    }

    //This method is like update but it's framerate independent. This means it gets called at a set time interval instead of every frame. This is useful for physics calculations
    void FixedUpdate()
    {

    }

    public static void setPlayerCat(CarrierCatapult pc)
    {
      Debug.Log("setPlayerCat has been called");
      GameObject currentVehicle = VTOLAPI.GetPlayersVehicleGameObject();
      Transform hookForcePt = currentVehicle.transform.Find("LandingGear").transform.Find("hookForcePt").transform;
      GameObject alignIndicatorUI = currentVehicle.transform.Find("sevtf_layer_2/WingLeftPart/wingLeft/HP_5/AlignmentIndicator(Clone)/UI").gameObject;

      /*foreach (Transform child in alignIndicatorUI.GetComponentInChildren<Transform>())
      {
        Debug.Log(child);
        foreach (Transform c in child.GetComponentInChildren<Transform>())
        {
          Debug.Log(c);
          foreach (Transform ch in c.GetComponentInChildren<Transform>())
          {
            Debug.Log(ch);
            foreach (Transform chi in ch.GetComponentInChildren<Transform>())
            {
              Debug.Log(chi);
            }
          }
        }
      }*/

      RectTransform backgroundUI = (RectTransform)alignIndicatorUI.transform.Find("Background").transform;
      RectTransform playerUI = (RectTransform)backgroundUI.transform.Find("playerUI").transform;

      UIHandler handler = alignIndicatorUI.AddComponent<UIHandler>();
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

      handler.angleDisplay = (Text)alignIndicatorUI.transform.Find("Info Panel/Angle Text/Angle Display").gameObject.GetComponent<Text>();
      handler.alignDisplay = (Text)alignIndicatorUI.transform.Find("Info Panel/Align Text/Align Display").gameObject.GetComponent<Text>();
      handler.moveDisplay = (Text)alignIndicatorUI.transform.Find("Info Panel/Move Text/Move Display").gameObject.GetComponent<Text>();
      if (handler.moveDisplay == null || handler.alignDisplay == null || handler.alignDisplay == null)
      {
        Debug.Log("Could not assign text element(s)");
      }
    }

    public static void afterHook()
    {
      Debug.Log("afterHook");
      GameObject currentVehicle = VTOLAPI.GetPlayersVehicleGameObject();
      GameObject alignIndicatorUI = currentVehicle.transform.Find("sevtf_layer_2/WingLeftPart/wingLeft/HP_5/AlignmentIndicator(Clone)/UI").gameObject;
      if (alignIndicatorUI.GetComponent<UIHandler>() != null)
      {
        Debug.Log("destroying");
        Destroy(alignIndicatorUI.GetComponent<UIHandler>());
      }
    }
  }
}