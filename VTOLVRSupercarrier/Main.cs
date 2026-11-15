using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModLoader.Framework;
using ModLoader.Framework.Attributes;
using System.Reflection;
using System.IO;

namespace VTOLVRSupercarrier
{
  [ItemId("the_faulty-supercarrier")]
  public class Main : VtolMod
  {
    public static bool DEBUG = false;
    
    private readonly List<GameObject> spawnedCrew = new List<GameObject>();

    public void Awake()
    {
      StartCoroutine(LoadCarrierCrew());
    }

    private IEnumerator LoadCarrierCrew()
    {
      string modFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      string bundlePath = Path.Combine(modFolder, "carriercrew.assets");
      AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);
      yield return bundleRequest;
      AssetBundle bundle = bundleRequest.assetBundle;
      if (!bundle)
      {
        Log("Couldn't load carrier crew asset bundle from " + bundlePath);
        yield break;
      }

      AssetBundleRequest prefabRequest = bundle.LoadAssetAsync<GameObject>("carriercrew.prefab");
      yield return prefabRequest;
      GameObject prefab = prefabRequest.asset as GameObject;
      bundle.Unload(false);
      if (!prefab)
      {
        Log("Couldn't find carriercrew.prefab in " + bundlePath);
        yield break;
      }

      // Keep crew callbacks inactive until the clone has a carrier parent.
      GameObject staging = new GameObject("CarrierCrew Staging");
      staging.SetActive(false);
      staging.transform.SetParent(transform, false);

      WaitForSecondsRealtime scanInterval = new WaitForSecondsRealtime(0.5f);
      while (true)
      {
        TargetManager targets = TargetManager.instance;
        if (FlightSceneManager.instance && targets && targets.alliedUnits != null)
        {
          foreach (Actor actor in targets.alliedUnits)
          {
            if (!actor || actor.iconType != UnitIconManager.MapIconTypes.Carrier ||
                !actor.GetComponent<AICarrierSpawn>() || actor.transform.Find("CarrierCrew"))
              continue;

            GameObject crew = Instantiate(prefab, staging.transform, false);
            crew.SetActive(false);
            crew.name = "CarrierCrew";
            crew.transform.SetParent(actor.transform, false);
            crew.transform.localPosition = new Vector3(0, 23.98f, 0);
            crew.transform.localRotation = Quaternion.identity;
            spawnedCrew.Add(crew);
            crew.SetActive(true);
            Log("Added carrier crew to " + actor);
          }
        }
        spawnedCrew.RemoveAll(crew => !crew);
        yield return scanInterval;
      }
    }

    public override void UnLoad()
    {
      StopAllCoroutines();
      foreach (GameObject crew in spawnedCrew)
        Destroy(crew);
      spawnedCrew.Clear();
    }
    //Override the VTOLMOD.Log function because it doesn't work with static methods
    private static void Log(object text)
    {
      Debug.Log("VTOLVR-Supercarrier: " + text);
    }
  }
}
