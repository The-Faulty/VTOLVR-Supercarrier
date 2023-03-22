using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class ShooterHandler : MonoBehaviour
{
  Animator anim;

  public Transform idlePoint;
  public Transform alignPoint;
  public Transform playerTarget;
  public Transform gameTarget;
  public Transform agent;

  public CrewNav navAgent;

  [Header("Nav Points")]
  public List<GameObject> Catapults = new List<GameObject>();

  public Text indicator;

  private VehicleMaster Vehicle;
  private ModuleEngine[] Engines;
  private CatapultHook catHook;
  private CarrierCatapult playerCat;

  private bool isIdle = true;
  private bool bar = false;
  private bool wings = false;
  private bool engines = false;
  private bool isWalking = false;

  private enum PlayerState
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

  PlayerState state;

  private void OnEnable()
  {
    anim = GetComponent<Animator>();
    Debug.Log("HandlerAnim: " + anim);
    indicator = transform.Find("DeckCrewMesh/Canvas/Text").GetComponent<Text>();
    foreach (Transform cat in transform.parent.parent.parent.Find("NavPoints"))
    {
      Debug.Log(cat.gameObject);
      Catapults.Add(cat.gameObject);
    }
    state = PlayerState.None;
  }

  [ContextMenu("Update")]
  // Update is called once per frame
  void Update()
  {
    Vector3 lookPos;
    Quaternion rotation;
    switch (state)
    {
      case (PlayerState.Taxi):
        if (navAgent.remainingDistance < .3)
        {
          lookPos = playerTarget.transform.position - agent.transform.position;
          lookPos.y = 0;
          rotation = Quaternion.LookRotation(lookPos);
          agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, rotation, Time.deltaTime * 2);
          //do alignment stuff
          Align();
        }
        break;
      case (PlayerState.LaunchBar):
        if (catHook.deployed)
        {
          Debug.Log("Hook");
          state = PlayerState.Hook;
          anim.SetBool("bar", false);

          indicator.text = "Forward";
          anim.SetBool("forward", true);
        }
        break;
      case (PlayerState.Hook):
        //align closer
        break;
      case (PlayerState.Wings):
        if (!Vehicle.wingFolder.deployed)
        {
          indicator.text = "";
          anim.SetBool("wings", false);
          navAgent.SetDestination(idlePoint.localPosition);
          state = PlayerState.LaunchReady;
        }
        break;
      case (PlayerState.LaunchReady):
        if (navAgent.remainingDistance < .3)
        {
          indicator.text = "Engines";
          anim.SetBool("runup", true);
          state = PlayerState.Runup;
        }
        break;
      case (PlayerState.Runup):
        lookPos = playerTarget.transform.position - agent.transform.position;
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
          state = PlayerState.Launch;
          anim.SetBool("runup", false);
          anim.SetBool("launch", true);
          indicator.text = "Launch";
        }
        break;
    }
  }

  //******make close and far for launch bar deployment
  void Align()
  {
    float relativeAngle = Vector2.SignedAngle(new Vector2(gameTarget.forward.x, gameTarget.forward.z), new Vector2((gameTarget.position - playerTarget.position).x, (gameTarget.position - playerTarget.position).z));
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
    if ((gameTarget.transform.position - playerTarget.transform.position).sqrMagnitude < 1.5 && Vector3.Dot(playerTarget.transform.forward, gameTarget.forward) > 0.5f)
    {
      indicator.text = "Bar";
      //Debug.Log("Bar");
      anim.SetBool("left", false);
      anim.SetBool("right", false);
      anim.SetBool("forward", false);
      anim.SetBool("bar", true);
      state = PlayerState.LaunchBar;
    }
  }

  void forward()
  {
    indicator.text = "Forward";
    anim.SetBool("left", false);
    anim.SetBool("right", false);
    anim.SetBool("forward", true);
  }

  void right()
  {
    indicator.text = "Right";
    //Debug.Log("right");
    anim.SetBool("left", false);
    anim.SetBool("right", true);
    anim.SetBool("forward", false);
  }

  void left()
  {
    indicator.text = "Left";
    //Debug.Log("left");
    anim.SetBool("left", true);
    anim.SetBool("right", false);
    anim.SetBool("forward", false);
  }

  [ContextMenu("Trigger Align")]
  void AlignTrigger()
  {
    Debug.Log("Align trigger");
    //AlignButton.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Stop Align";
    navAgent.SetDestination(alignPoint.localPosition);
    state = PlayerState.Taxi;
    isIdle = !isIdle;
    anim.SetBool("left", false);
    anim.SetBool("right", false);
    anim.SetBool("forward", false);
    anim.SetBool("bar", false);
    anim.SetBool("runup", false);
    anim.SetBool("launch", false);
  }

  public void startAlign(CarrierCatapult cat, GameObject vehicle)
  {
    Debug.Log("Start Align");
    GameObject catNavPoints = Catapults[0]; //Catapults[cat.catapultDesignation - 1]; .getComponentsInChildren?

    alignPoint = catNavPoints.transform.Find("AlignPoint").transform;
    idlePoint = catNavPoints.transform.Find("IdlePoint").transform;

    playerCat = cat;
    Vehicle = vehicle.GetComponent<VehicleMaster>();
    Engines = Vehicle.engines;
    catHook = Vehicle.GetComponentInChildren<CatapultHook>();
    catHook.OnHooked.AddListener(onHook);

    playerTarget = catHook.hookForcePointTransform;
    gameTarget = playerCat.catapultTransform;

    Debug.Log(playerTarget);
    Debug.Log(gameTarget);

    AlignTrigger();
  }

  void onHook()
  {
    indicator.text = "Wings";
    state = PlayerState.Wings;
  }
}
