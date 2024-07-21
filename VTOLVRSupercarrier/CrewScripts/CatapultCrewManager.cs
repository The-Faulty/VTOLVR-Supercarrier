using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace VTOLVRSupercarrier.CrewScripts
{
  public class CatapultCrewManager : MonoBehaviour
  {
    public Transform hookPoint;
    public Transform planeCOM;
    public Transform hookTarget;
    public VehicleMaster vehicle;
    public ModuleEngine[] engines;
    public CatapultHook catHook;

    public BoxCollider AlignTrigger;

    public CrewManager crewManager;
    public NavPoints navPoints;
    public int designation;

    public event Action OnTaxi, OnLaunchBar, OnWings, OnHook, OnLaunchReady, OnRunup, OnLaunch, Reset;

    public enum AlignmentState
    {
      None,
      Taxi,
      LaunchBar,
      Wings,
      Hook,
      LaunchReady,
      Runup,
      Launch
    }

    private AlignmentState _alignmentState = AlignmentState.None;
    public AlignmentState state
    {
      get => _alignmentState;
      set
      {
        _alignmentState = value;
        Log("New state: " + value);
      }
    }

    void OnEnable()
    {
      designation = 4;
      navPoints = GetComponentInChildren<NavPoints>();
      CarrierCatapultManager catapultManager = GetComponentInParent<CarrierCatapultManager>();
      foreach (CarrierCatapult cat in catapultManager.catapults)
      {
        Log(cat.catapultDesignation);
        if (cat.catapultDesignation == designation)
        {
          hookTarget = cat.catapultTransform;
          break;
        }
      }
      state = AlignmentState.None;
    }

    private void OnTriggerEnter(Collider other)
    {
      VehicleMaster vm = other.gameObject.GetComponentInParent<VehicleMaster>();
      if (vm && vehicle == null && state == AlignmentState.None)
      {
        ManagedCatapult mc = vm.gameObject.AddComponent<ManagedCatapult>();
        mc.manager = this;
        StartAlign(other.gameObject.GetComponentInParent<VehicleMaster>());
      }
    }

    private void OnTriggerExit(Collider other)
    {
      ManagedCatapult mc = other.gameObject.GetComponentInParent<ManagedCatapult>();
      if (mc && mc.manager == this)
      {
        Destroy(other.gameObject.GetComponentInParent<ManagedCatapult>());
      }
    }

    public void StartAlign(VehicleMaster v)
    {
      Log("Start Align");

      vehicle = v;
      engines = vehicle.engines;
      catHook = vehicle.GetComponentInChildren<CatapultHook>();
      //catHook.OnHooked.AddListener(OnHookListener);

      hookPoint = catHook.hookForcePointTransform;
      planeCOM = vehicle.GetComponentInChildren<CenterOfMass>().transform;

      Log(hookPoint);
      Log(hookTarget);

      state = AlignmentState.Taxi;
      OnTaxi?.Invoke();
      StartCoroutine(AlignRoutine());
    }

    private IEnumerator AlignRoutine()
    {
      while (state != AlignmentState.None)
      {
        switch (state)
        {
          case AlignmentState.Taxi:
            if ((navPoints.preHookAlignPoint.position - hookPoint.transform.position).sqrMagnitude < 0.36f && Vector3.Dot(hookPoint.transform.forward, navPoints.preHookAlignPoint.forward) > 0.5f)
            {
              state = AlignmentState.LaunchBar;
              OnLaunchBar?.Invoke();
            }
            break;
          case AlignmentState.LaunchBar:
            if (catHook.deployed)
            {
              if (vehicle.wingFolder)
              {
                state = AlignmentState.Wings;
                OnWings?.Invoke();
              }
              else
              {
                state = AlignmentState.Hook;
                OnHook?.Invoke();
              }
            }
            break;
          case AlignmentState.Wings:
            if (!vehicle.wingFolder.deployed)
            {
              state = AlignmentState.Hook;
              OnHook?.Invoke();
            }
            break;
          case AlignmentState.Hook:
            if ((hookTarget.transform.position - hookPoint.transform.position).sqrMagnitude < 0.36f && Vector3.Dot(hookPoint.transform.forward, hookTarget.forward) > 0.5f)
            {
              state = AlignmentState.LaunchReady;
              OnLaunchReady?.Invoke();
            }
            break;
          case AlignmentState.LaunchReady:
            // wait for aircraft to move forward
            yield return new WaitForSeconds(6);
            state = AlignmentState.Runup;
            OnRunup?.Invoke();
            break;
          case AlignmentState.Runup:
            bool flag = true;
            foreach (ModuleEngine engine in engines)
            {
              if (engine.finalThrottle < .66) flag = false;
            }
            if (flag)
            {
              state = AlignmentState.Launch;
              OnLaunch?.Invoke();
            }
            break;
          case AlignmentState.Launch:
            yield return new WaitForSeconds(10);
            state = AlignmentState.None;
            vehicle = null;
            Reset?.Invoke();
            break;
        }
        yield return new WaitForFixedUpdate();
      }
    }

    private void Log(object text)
    {
      Debug.Log("CatapultCrewManager: " + text);
    }
  }
}