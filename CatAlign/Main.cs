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
      }
      //VTOLAPI.SceneLoaded += SceneLoaded;
      base.ModLoaded();
    }

    //This method is called every frame by Unity. Here you'll probably put most of your code
    void Update()
    {
    }

    //This method is like update but it's framerate independent. This means it gets called at a set time interval instead of every frame. This is useful for physics calculations
    void FixedUpdate()
    {

    }

    //This function is called every time a scene is loaded. this behaviour is defined in Awake().
    /*private void SceneLoaded(VTOLScenes scene)
    {
      //If you want something to happen in only one (or more) scenes, this is where you define it.

      //For example, lets say you're making a mod which only does something in the ready room and the loading scene. This is how your code could look:
      switch (scene)
      {
        case VTOLScenes.ReadyRoom:
          //Add your ready room code here
          break;
        case VTOLScenes.LoadingScene:
          //Add your loading scene code here
          break;
      }
      //GameObject currentVehicle = VTOLAPI.GetPlayersVehicleGameObject();
      //Transform hookForcePt = currentVehicle.transform.Find("LandingGear").transform.Find("hookForcePt").transform;
    }*/

    public static void setPlayerCat(CarrierCatapult pc)
    {
      Debug.Log("setPlayerCat has been called");
      GameObject currentVehicle = VTOLAPI.GetPlayersVehicleGameObject();
      Debug.Log("1");
      Transform hookForcePt = currentVehicle.transform.Find("LandingGear").transform.Find("hookForcePt").transform;
      Debug.Log("2");
      GameObject alignIndicatorUI = currentVehicle.transform.Find("sevtf_layer_2/WingLeftPart/wingLeft/HP_5/AlignmentIndicator(Clone)/UI").gameObject;
      Debug.Log("3");

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
      Debug.Log("3");
      RectTransform playerUI = (RectTransform)backgroundUI.transform.Find("playerUI").transform;
      Debug.Log("3");

      Debug.Log(hookForcePt);
      Debug.Log(pc);
      Debug.Log(pc.transform);
      UIHandler handler = alignIndicatorUI.AddComponent<UIHandler>();
      Debug.Log("3");
      handler.characterTarget = hookForcePt;
      Debug.Log("3");
      handler.gameTarget = pc.transform;

      Debug.Log("4");
      if (handler.characterTarget == null || handler.gameTarget == null)
      {
        Debug.Log("Could not assign target element(s)");
      }

      handler.backgroundUI = backgroundUI;
      handler.playerUI = playerUI;

      Debug.Log("5");
      if (handler.backgroundUI == null || handler.playerUI == null)
      {
        Debug.Log("Could not assign UI element(s)");
      }

      handler.angleDisplay = (Text)alignIndicatorUI.transform.Find("Info Panel").transform.Find("Angle Text").transform.Find("Angle Display").gameObject.GetComponent<Text>();
      handler.alignDisplay = (Text)alignIndicatorUI.transform.Find("Info Panel").transform.Find("Align Text").transform.Find("Align Display").gameObject.GetComponent<Text>();
      handler.moveDisplay = (Text)alignIndicatorUI.transform.Find("Info Panel").transform.Find("Move Text").transform.Find("Move Display").gameObject.GetComponent<Text>();
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