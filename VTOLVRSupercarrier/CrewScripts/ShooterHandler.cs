using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static VTOLVRSupercarrier.CrewScripts.CatapultCrewManager;

namespace VTOLVRSupercarrier.CrewScripts
{
  public class ShooterHandler : DeckCrew
  {
    public Transform idlePoint;
    public Transform mainPoint;

    private bool isIdle = true;

    public override void OnEnable()
    {
      base.OnEnable();

      mainPoint = catapultManager.navPoints.shooterMainPoint;
      idlePoint = catapultManager.navPoints.shooterIdlePoint;
      //Log("Enable Finish");
      //Log(GetComponentInChildren<SkinnedMeshRenderer>().material.shader);
    }

    [ContextMenu("OnTaxi")]
    protected override void OnTaxi()
    {
      navAgent.SetDestination(mainPoint.localPosition);
      isIdle = false;
      ResetAnimVars();
      StartCoroutine(OnTaxiRoutine());
    }
    private IEnumerator OnTaxiRoutine()
    {
      while (navAgent.remainingDistance > 0.3f)
      {
        yield return new WaitForFixedUpdate();
      }
      LookAt(catapultManager.navPoints.preHookAlignPoint);
    }

    protected override void OnLaunchReady()
    {
      LookAt(catapultManager.hookPoint.transform);
    }

    protected override void OnRunup()
    {
      LookAt(catapultManager.hookPoint.transform);
      anim.SetBool("runup", true);
    }

    protected override void OnLaunch()
    {
      anim.SetBool("runup", false);
      anim.SetBool("launch", true);
    }

    protected override void Reset()
    {
      ResetAnimVars();
      StopAllCoroutines();
      navAgent.SetDestination(idlePoint.localPosition);
      Log("reset");
      isIdle = true;
    }

    private void ResetAnimVars()
    {
      anim.SetBool("runup", false);
      anim.SetBool("launch", false);
    }

    private void Log(object text)
    {
      Debug.Log("ShooterHandler: " + text);
    }
  }
}