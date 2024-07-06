using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShooterHandler : MonoBehaviour
{
  Animator anim;

  public Transform idlePoint;
  public Transform alignPoint;
  public Transform hookPoint;
  public Transform planeCOM;
  public Transform hookTarget;
  public Transform agent;

  public CrewManager Manager;

  public CrewNav navAgent;

  [Header("Nav Points")]
  public List<GameObject> Catapults = new List<GameObject>();
  public int designation = 4;

  private VehicleMaster Vehicle;
  private ModuleEngine[] Engines;
  private CatapultHook catHook;
  private CarrierCatapult playerCat;

  private bool isIdle = true;

  private enum AlignmentState
  {
    None,
    Taxi,
    LaunchBar,
    Hook,
    Wings,
    LaunchReady,
    Runup,
    Launch
  }

  AlignmentState state;

  private void OnEnable()
  {
    anim = GetComponent<Animator>();
    Log("HandlerAnim: " + anim);
    foreach (Transform cat in transform.parent.parent.parent.Find("NavPoints"))
    {
      Log(cat.gameObject);
      Catapults.Add(cat.gameObject);
    }
    state = AlignmentState.None;
    //navAgent.SetDestination(Catapults[0].transform.GetChild(1).localPosition);
    Manager.StartAlignment += startAlign;
    Log("ShooterHandler Enable Finish");
  }

  [ContextMenu("Update")]
  // Update is called once per frame
  void Update()
  {
    Vector3 lookPos;
    Quaternion rotation;
    switch (state)
    {
      case (AlignmentState.Taxi):
        if (navAgent.remainingDistance < .3)
        {
          lookPos = hookPoint.transform.position - agent.transform.position;
          lookPos.y = 0;
          rotation = Quaternion.LookRotation(lookPos);
          agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, rotation, Time.deltaTime * 2);
          //do alignment stuff
          Align();

          if ((hookTarget.transform.position - hookPoint.transform.position).sqrMagnitude < 1.5 && Vector3.Dot(hookPoint.transform.forward, hookTarget.forward) > 0.5f)
          {
            //Log("Bar");
            anim.SetBool("left", false);
            anim.SetBool("right", false);
            anim.SetBool("forward", false);
            anim.SetBool("bar", true);
            state = AlignmentState.LaunchBar;
          }
        }
        break;
      case (AlignmentState.LaunchBar):
        if (catHook.deployed)
        {
          Log("Hook");
          state = AlignmentState.Hook;
          anim.SetBool("bar", false);

          anim.SetBool("forward", true);
        }
        break;
      case (AlignmentState.Hook):
        Align();
        if ((hookTarget.transform.position - hookPoint.transform.position).sqrMagnitude < 0.36f && Vector3.Dot(hookPoint.transform.forward, hookTarget.forward) > 0.5f)
        {
          anim.SetBool("left", false);
          anim.SetBool("right", false);
          anim.SetBool("forward", false);
          anim.SetBool("wings", true);
          state = AlignmentState.Wings;
        }
        break;
      case (AlignmentState.Wings):
        if (!Vehicle.wingFolder.deployed)
        {
          anim.SetBool("wings", false);
          navAgent.SetDestination(idlePoint.localPosition);
          state = AlignmentState.LaunchReady;
        }
        break;
      case (AlignmentState.LaunchReady):
        if (navAgent.remainingDistance < .3)
        {
          anim.SetBool("runup", true);
          state = AlignmentState.Runup;
        }
        break;
      case (AlignmentState.Runup):
        lookPos = hookPoint.transform.position - agent.transform.position;
        lookPos.y = 0;
        rotation = Quaternion.LookRotation(lookPos);
        agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, rotation, Time.deltaTime * 2);

        bool flag = true;
        foreach (ModuleEngine engine in Engines)
        {
          if (engine.finalThrottle < .66) flag = false;
        }
        if (flag)
        {
          state = AlignmentState.Launch;
          anim.SetBool("runup", false);
          anim.SetBool("launch", true);
        }
        break;
    }
  }

  //******make close and far for launch bar deployment
  void Align()
  {
    float relativeAngle = Vector2.SignedAngle(new Vector2(hookTarget.forward.x, hookTarget.forward.z), new Vector2((hookTarget.position - planeCOM.position).x, (hookTarget.position - planeCOM.position).z));
    if (relativeAngle > 5)
    {
      left();
    }
    else if (relativeAngle < -5)
    {
      right();
    }
    else
    {
      forward();
    }
  }

  void forward()
  {
    anim.SetBool("left", false);
    anim.SetBool("right", false);
    anim.SetBool("forward", true);
  }

  void right()
  {
    //Log("right");
    anim.SetBool("left", false);
    anim.SetBool("right", true);
    anim.SetBool("forward", false);
  }

  void left()
  {
    //Log("left");
    anim.SetBool("left", true);
    anim.SetBool("right", false);
    anim.SetBool("forward", false);
  }

  [ContextMenu("Trigger Align")]
  void AlignTrigger()
  {
    Log("Align trigger");
    //AlignButton.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Stop Align";
    navAgent.SetDestination(alignPoint.localPosition);
    state = AlignmentState.Taxi;
    isIdle = !isIdle;
    anim.SetBool("left", false);
    anim.SetBool("right", false);
    anim.SetBool("forward", false);
    anim.SetBool("bar", false);
    anim.SetBool("runup", false);
    anim.SetBool("launch", false);
  }

  public void startAlign(CrewManager.VehicleInQueue v)
  {
    if (v.catNumber == designation)
    {
      Log("Start Align");
      GameObject catNavPoints = Catapults[v.catapult.catapultDesignation - 1]; //.getComponentsInChildren?

      alignPoint = catNavPoints.transform.Find("AlignPoint").transform;
      idlePoint = catNavPoints.transform.Find("IdlePoint").transform;

      playerCat = v.catapult;
      Vehicle = v.vehicle.GetComponent<VehicleMaster>();
      Engines = Vehicle.engines;
      catHook = Vehicle.GetComponentInChildren<CatapultHook>();
      catHook.OnHooked.AddListener(onHook);

      hookPoint = catHook.hookForcePointTransform;
      hookTarget = playerCat.catapultTransform;
      planeCOM = Vehicle.GetComponentInChildren<GearAnimator>().dragComponent.transform;

      Log(hookPoint);
      Log(hookTarget);

      AlignTrigger();
    }
  }

  void onHook()
  {
    state = AlignmentState.Wings;
  }

  private void Log(object text)
  {
    Debug.Log("ShooterHandler: " + text);
  }
}
