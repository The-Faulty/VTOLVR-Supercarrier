using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace VTOLVRSupercarrier.CrewScripts
{
  public abstract class DeckCrew : MonoBehaviour
  {
    public CrewNav navAgent;
    [Header("Nav Points")]
    public List<GameObject> Catapults = new List<GameObject>();

    public CrewManager Manager;
    public CatapultCrewManager catapultManager;

    protected Animator anim;

    public virtual void OnEnable()
    {
      anim = GetComponentInChildren<Animator>();
      anim.Play("Idle", 0, Random.Range(0, 1f));
      catapultManager.OnTaxi += OnTaxi;
      catapultManager.OnLaunchBar += OnLaunchBar;
      catapultManager.OnWings += OnWings;
      catapultManager.OnHook += OnHook;
      catapultManager.OnLaunchReady += OnLaunchReady;
      catapultManager.OnRunup += OnRunup;
      catapultManager.OnLaunch += OnLaunch;
      catapultManager.Reset += Reset;
    }

    protected void LookAt(Transform t)
    {
      StartCoroutine(LookAtRoutine(t));
    }

    private IEnumerator LookAtRoutine(Transform t)
    {
      Vector3 lookPos = t.position - navAgent.transform.position;
      Quaternion rotation = Quaternion.LookRotation(lookPos);
      float angle;
      while (navAgent.transform.rotation != rotation)
      {
        angle = Quaternion.Angle(navAgent.transform.rotation, rotation);
        if (angle < 1f)
        {
          navAgent.transform.rotation = rotation;
          break;
        }
        lookPos = t.position - navAgent.transform.position;
        lookPos.y = 0;
        rotation = Quaternion.LookRotation(lookPos);
        navAgent.transform.rotation = Quaternion.Slerp(navAgent.transform.rotation, rotation, Time.fixedDeltaTime * 2);
        yield return new WaitForFixedUpdate();
      }
    }

    protected float startTime = 0;
    protected float wait = 0;
    protected bool waiting = false;
    protected bool WaitForSeconds(float waitTime)
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
    
    protected virtual void OnTaxi()
    {
      return;
    }
    protected virtual void OnLaunchBar()
    {
      return;
    }
    protected virtual void OnWings()
    {
      return;
    }
    protected virtual void OnHook()
    {
      return;
    }
    protected virtual void OnLaunchReady()
    {
      return;
    }
    protected virtual void OnRunup()
    {
      return;
    }
    protected virtual void OnLaunch()
    {
      return;
    }
    protected virtual void Reset()
    {
      return;
    }
    protected virtual void OnLanding()
    {
      return;
    }
  }
}