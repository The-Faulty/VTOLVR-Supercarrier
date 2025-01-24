using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VTOLAPI;

namespace VTOLVRSupercarrier.CrewScripts
{
  public enum AlignmentState
  {
    None,
    Taxi,
    LaunchBar,
    Wings,
    Hook,
    LaunchReady,
    Runup,
    Launch,
    Landing
  }

  public class CatapultCrewManager : MonoBehaviour
  {
    public Transform hookPoint;
    public Transform planeCOM;
    public Transform hookTarget;
    public VehicleMaster vehicle;
    public ModuleEngine[] engines;
    public CatapultHook catHook;

    public BoxCollider AlignTrigger;

    public ManagedCatapult player;
    public NavPoints navPoints;
    public int designation;

    public Runway runway;
    public event Action OnTaxi, OnLaunchBar, OnWings, OnHook, OnLaunchReady, OnRunup, OnLaunch, OnLanding, Reset;
    public event Action<AlignmentState> OnStateChanged;
    public event Action<VehicleMaster> OnStartAlign;

    private AlignmentState _alignmentState = AlignmentState.None;
    private CarrierLogger logger;

    public AlignmentState state
    {
      get => _alignmentState;
      set
      {
        _alignmentState = value;
        OnStateChanged?.Invoke(value);
        logger.Log("New state: " + value);
      }
    }

    private Coroutine routine;

    void OnEnable()
    {
      if (logger == null)
      {
        logger = new CarrierLogger("CatapultCrewManager");
      }
      //designation = 4;
      runway = GetComponentInParent<AICarrierSpawn>().runway;
      navPoints = GetComponentInChildren<NavPoints>();
      state = AlignmentState.None;
      CarrierCatapultManager catapultManager = GetComponentInParent<CarrierCatapultManager>();
      if (catapultManager != null)
      {
        foreach (CarrierCatapult cat in catapultManager.catapults)
        {
          logger.Log(cat.catapultDesignation);
          if (cat.catapultDesignation == designation)
          {
            hookTarget = cat.catapultTransform;
            break;
          }
        }
      }
    }

    private void OnTriggerEnter(Collider other)
    {
      logger.Log("Trigger Enter");
      logger.Log(state);
      logger.Log(vehicle);
      VehicleMaster vm = other.gameObject.GetComponentInParent<VehicleMaster>();
      VehicleMaster localPlayer = VTAPI.GetPlayersVehicleGameObject().GetComponent<VehicleMaster>();
      if (vm && vm == localPlayer && vehicle == null && state == AlignmentState.None)
      {
        player = vm.gameObject.AddComponent<ManagedCatapult>();
        player.manager = this;
        OnStartAlign?.Invoke(vm);
        StartAlign(vm);
      }
    }

    private void OnTriggerExit(Collider other)
    {
      ManagedCatapult mc = other.gameObject.GetComponentInParent<ManagedCatapult>();
      if (mc && mc == player && mc.manager == this)
      {
        Destroy(player);
        ResetTrigger();
      }
    }

    public void StartAlign(VehicleMaster v)
    {
      logger.Log("Start Align");

      vehicle = v;
      engines = vehicle.engines;
      catHook = vehicle.GetComponentInChildren<CatapultHook>();
      //catHook.OnHooked.AddListener(OnHookListener);

      hookPoint = catHook.hookForcePointTransform;
      planeCOM = vehicle.GetComponentInChildren<CenterOfMass>().transform;

      logger.Log(hookPoint);
      logger.Log(hookTarget);

      state = AlignmentState.Taxi;
      OnTaxi?.Invoke();
      routine = StartCoroutine(AlignRoutine());
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
            if (((hookTarget.transform.position - hookPoint.transform.position).sqrMagnitude < 0.36f && Vector3.Dot(hookPoint.transform.forward, hookTarget.forward) > 0.5f) || vehicle.GetComponentInChildren<CatapultHook>().hooked)
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
            logger.Log("reset");
            Reset?.Invoke();
            break;
        }
        yield return new WaitForFixedUpdate();
      }
    }

    public void LandingTrigger(VehicleMaster vm = null)
    {
      if (designation > 2 && state == AlignmentState.None)
      {
        logger.Log("landing");
        OnLanding?.Invoke();
        state = AlignmentState.Landing;
        if (vm)
        {
          routine = StartCoroutine(LandingRoutine(vm));
        }
      }
    }

    private IEnumerator LandingRoutine(VehicleMaster vm)
    {
      Actor carrierActor = GetComponentInParent<Actor>();
      while (state == AlignmentState.Landing && vm.flightInfo.isLanded == false && vm.pilotIsDead == false)
      {
        yield return new WaitForSeconds(1);
      }
      yield return new WaitForSeconds(4);
      while (!runway.IsRunwayClear(carrierActor))
      {
        yield return new WaitForSeconds(2);
      }
      ResetTrigger();
    }

    public void ResetTrigger()
    {
      logger.Log("reset");
      player = null;
      if (routine != null)
      {
        StopCoroutine(routine);
      }
      state = AlignmentState.None;
      Reset?.Invoke();
    }

    public void SyncState(AlignmentState serverState)
    {
      if (serverState == state || state == AlignmentState.None)
        return;

      switch (serverState)
      {
        case AlignmentState.None:
          ResetTrigger();
          break;
        case AlignmentState.Taxi:
          // Currently needs to be invoked by collider to properly start alignment sequence
          // OnTaxi?.Invoke();
          break;
        case AlignmentState.LaunchBar:
          OnLaunchBar?.Invoke();
          break;
        case AlignmentState.Wings:
          OnWings?.Invoke();
          break;
        case AlignmentState.Hook:
          OnHook?.Invoke();
          break;
        case AlignmentState.LaunchReady:
          OnLaunchReady?.Invoke();
          break;
        case AlignmentState.Runup:
          OnRunup?.Invoke();
          break;
        case AlignmentState.Launch:
          OnLaunch?.Invoke();
          break;
        case AlignmentState.Landing:
          LandingTrigger();
          break;
      }
    }
  }
}