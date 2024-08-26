using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static VTOLVRSupercarrier.CrewScripts.CatapultCrewManager;

namespace VTOLVRSupercarrier.CrewScripts
{
  class DirectorHandler : DeckCrew
  {

    public Transform mainPoint;
    public Transform alignPoint;
    public Transform landingPoint;

    private bool isIdle = true;

    public override void OnEnable()
    {
      base.OnEnable();

      alignPoint = catapultManager.navPoints.directorAlignPoint;
      mainPoint = catapultManager.navPoints.directorMainPoint;
      landingPoint = catapultManager.navPoints.directorLandPoint;
      //Log("Enable Finish");
    }

    protected override void OnTaxi()
    {
      ResetAnimVars();
      navAgent.SetDestination(alignPoint.localPosition);
      isIdle = !isIdle;
      anim.SetBool("align", true);
      LookAt(catapultManager.hookPoint.transform);
      StartCoroutine(OnTaxiRoutine());
    }
    private IEnumerator OnTaxiRoutine()
    {
      while (catapultManager.state == AlignmentState.Taxi)
      {
        if (navAgent.remainingDistance < 0.3f)
        {
          Align(catapultManager.navPoints.preHookAlignPoint);
        }

        yield return new WaitForFixedUpdate();
      }
      anim.SetBool("align", false);
    }
    protected override void OnLaunchBar()
    {
      anim.SetBool("bar", true);
    }
    protected override void OnWings()
    {
      anim.SetBool("bar", false);
      anim.SetBool("wings", true);
    }
    protected override void OnHook()
    {
      anim.SetBool("bar", false);
      anim.SetBool("wings", false);
      StartCoroutine(OnHookRoutine());
    }
    private IEnumerator OnHookRoutine()
    {
      while (anim.IsInTransition(0))
      {
        yield return new WaitForFixedUpdate();
      }
      navAgent.SetDestination(mainPoint.localPosition);
      while (catapultManager.state == AlignmentState.Hook)
      {
        if (navAgent.remainingDistance < 0.3f)
        {
          anim.SetBool("align", true);
          LookAt(catapultManager.hookPoint.transform);
          Align(catapultManager.hookTarget);
        }
        yield return new WaitForFixedUpdate();
      }
      anim.SetBool("align", false);
      LookAt(catapultManager.hookPoint.transform);
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
      navAgent.SetDestination(alignPoint.localPosition);
    }

    protected override void OnLanding()
    {
      navAgent.SetDestination(landingPoint.localPosition);
    }

    void Align(Transform target)
    {
      float relativeAngle = Vector2.SignedAngle(new Vector2(target.forward.x, target.forward.z), new Vector2((target.position - catapultManager.planeCOM.position).x, (target.position - catapultManager.planeCOM.position).z));
      if (relativeAngle > 2.5f)
      {
        anim.SetFloat("alignBlend", -1, 0.3f, Time.deltaTime);
      }
      else if (relativeAngle < -2.5f)
      {
        anim.SetFloat("alignBlend", 1, 0.3f, Time.deltaTime);
      }
      else
      {
        anim.SetFloat("alignBlend", 0, 0.3f, Time.deltaTime);
      }
    }

    private void ResetAnimVars()
    {
      anim.SetBool("align", false);
      anim.SetBool("bar", false);
      anim.SetBool("launch", false);
    }

    private void Log(object text)
    {
      Debug.Log("DirectorHandler: " + text);
    }
  }
}
