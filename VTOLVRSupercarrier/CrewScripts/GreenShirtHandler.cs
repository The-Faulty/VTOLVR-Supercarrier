using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static VTOLVRSupercarrier.CrewScripts.CatapultCrewManager;

namespace VTOLVRSupercarrier.CrewScripts
{
  class GreenShirtHandler : DeckCrew
  {
    public Transform idlePoint;
    public Transform mainPoint;
    public Transform alignPoint;
    public Transform landingPoint;

    private bool isIdle = true;

    public override void OnEnable()
    {
      base.OnEnable();

      idlePoint = catapultManager.navPoints.greenIdlePoint;
      alignPoint = catapultManager.navPoints.greenAlignPoint;
      mainPoint = catapultManager.navPoints.greenMainPoint;
      landingPoint = catapultManager.navPoints.greenLandPoint;
      //logger.Log("Enable Finish");
    }

    protected override void OnTaxi()
    {
      ResetAnimVars();
      isIdle = !isIdle;
      LookAt(catapultManager.hookPoint.transform);
    }
    protected override void OnHook()
    {
      StartCoroutine(OnHookRoutine());
    }
    private IEnumerator OnHookRoutine()
    {
      yield return new WaitForSeconds(3f);
      navAgent.SetDestination(alignPoint.localPosition);
      while (catapultManager.state == AlignmentState.Hook)
      {
        if (navAgent.remainingDistance < 0.3f)
        {
          anim.SetBool("bar", true);
          LookAt(catapultManager.hookPoint.transform);
        }
        yield return new WaitForFixedUpdate();
      }
      anim.SetBool("bar", false);
      navAgent.SetDestination(mainPoint.localPosition);
    }
    protected override void OnLaunchReady()
    {
      LookAt(catapultManager.hookPoint.transform);
    }
    protected override void OnRunup()
    {
      LookAt(catapultManager.hookPoint.transform);
    }
    protected override void OnLaunch()
    {
      //play launch animation
    }
    protected override void Reset()
    {
      ResetAnimVars();
      StopAllCoroutines();
      navAgent.SetDestination(idlePoint.localPosition);
      logger.Log("reset");
    }
    protected override void OnLanding()
    {
      navAgent.SetDestination(landingPoint.localPosition);
    }
    private void ResetAnimVars()
    {
      anim.SetBool("bar", false);
      anim.SetBool("launch", false);
    }
  }
}
