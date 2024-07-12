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
    Wings,
    Hook,
    LaunchReady,
    Runup,
    Launch
  }

  AlignmentState state;

  private void OnEnable()
  {
    anim = GetComponent<Animator>();
    foreach (Transform cat in transform.parent.parent.parent.Find("NavPoints"))
    {
      Log(cat.gameObject);
      Catapults.Add(cat.gameObject);
    }
    state = AlignmentState.None;
    Manager.StartAlignment += startAlign;
    Log("ShooterHandler Enable Finish");
  }

  [ContextMenu("Update")]
  // Update is called once per frame
  void Update()
  {
    switch (state)
    {
      case AlignmentState.Taxi:
        if (navAgent.remainingDistance < .3)
        {
          lookAt(hookPoint.transform);
          //do alignment stuff
          anim.SetBool("align", true);
          Align();

          if ((hookTarget.transform.position - hookPoint.transform.position).sqrMagnitude < 1.5 && Vector3.Dot(hookPoint.transform.forward, hookTarget.forward) > 0.5f)
          {
            //add stop animation here
            anim.SetBool("align", false);
            anim.SetBool("bar", true);
            state = AlignmentState.LaunchBar;
            Log("Launch bar");
          }
        }
        break;
      case AlignmentState.LaunchBar:
        if (catHook.deployed)
        {
          state = AlignmentState.Wings;
          anim.SetBool("bar", false);
          anim.SetBool("wings", true);
          Log("Wings");
        }
        break;
      case AlignmentState.Wings:
        if (!Vehicle.wingFolder.deployed)
        {
          anim.SetBool("wings", false);
          lookAt(idlePoint);
          if (waitForSeconds(2f))
          {
            navAgent.SetDestination(idlePoint.localPosition);
            state = AlignmentState.Hook;
            Log("Hook");
            Log((hookPoint.transform.position - agent.transform.position).normalized);
            Log(hookPoint.transform.position);
          }
        }
        break;
      case AlignmentState.Hook:
        if (navAgent.remainingDistance < .3f)
        {
          lookAt(hookPoint.transform);
          anim.SetBool("align", true);
          Align();
          if ((hookTarget.transform.position - hookPoint.transform.position).sqrMagnitude < 0.36f && Vector3.Dot(hookPoint.transform.forward, hookTarget.forward) > 0.5f)
          {
            anim.SetBool("align", false);
            state = AlignmentState.LaunchReady;
            Log("Launch Ready");
            Log((hookPoint.transform.position - agent.transform.position).normalized);
            Log(hookPoint.transform.position);
          }
        }
        break;
      case AlignmentState.LaunchReady:
        lookAt(hookPoint.transform);
        if (waitForSeconds(6f))
        {
          anim.SetBool("runup", true);
          state = AlignmentState.Runup;
          Log("Run up");
          Log((hookPoint.transform.position - agent.transform.position).normalized);
          Log(hookPoint.transform.position);
        }   
        break;
      case AlignmentState.Runup:
        //lookAt(hookPoint.transform);
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
          Log("Launch");
        }
        break;
      case AlignmentState.Launch:
        if (waitForSeconds(10f))
        {
          anim.SetBool("launch", false);
          state = AlignmentState.None;
          Log("Launch Complete");
        }
        break;
    }
  }

  //******make close and far for launch bar deployment
  void Align()
  {
    float relativeAngle = Vector2.SignedAngle(new Vector2(hookTarget.forward.x, hookTarget.forward.z), new Vector2((hookTarget.position - planeCOM.position).x, (hookTarget.position - planeCOM.position).z));
    if (relativeAngle > 2.5f)
    {
      //indicator.text = "Left";
      anim.SetFloat("alignBlend", -1, 0.3f, Time.deltaTime);
    }
    else if (relativeAngle < -2.5f)
    {
      //indicator.text = "Right";
      anim.SetFloat("alignBlend", 1, 0.3f, Time.deltaTime);
    }
    else
    {
      //indicator.text = "Forward";
      anim.SetFloat("alignBlend", 0, 0.3f, Time.deltaTime);
    }
  }

  void lookAt(Transform t)
	{
		Vector3 lookPos;
		Quaternion rotation;
		lookPos = t.position - agent.transform.position;
		lookPos.y = 0;
		rotation = Quaternion.LookRotation(lookPos);
		//Debug.Log(rotation);
		agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, rotation, Time.deltaTime * 2);
	}

  [ContextMenu("Trigger Align")]
  void AlignTrigger()
  {
    Log("Align trigger");
    //AlignButton.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Stop Align";
    navAgent.SetDestination(alignPoint.localPosition);
    state = AlignmentState.Taxi;
    isIdle = !isIdle;
    anim.SetBool("align", false);
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

  private float startTime = 0;
  private float wait = 0;
  private bool waiting = false;
  private bool waitForSeconds(float waitTime)
  {
    if (waiting)
    {
      if (Time.time - startTime > wait)
      {
        waiting = false;
        return true;
      }
      return false;
    }
    startTime = Time.time;
    wait = waitTime;
    waiting = true;
    return false;
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
