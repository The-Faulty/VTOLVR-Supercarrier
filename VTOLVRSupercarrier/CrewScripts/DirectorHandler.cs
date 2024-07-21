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

    private bool isIdle = true;

    public override void OnEnable()
    {
      base.OnEnable();

      alignPoint = catapultManager.navPoints.directorAlignPoint.transform;
      mainPoint = catapultManager.navPoints.directorMainPoint.transform;
      Log("DirectorHandler Enable Finish");
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
          Align();
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
          Align();
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
      navAgent.SetDestination(mainPoint.localPosition);
    }


    //******make close and far for launch bar deployment
    void Align()
    {
      float relativeAngle = Vector2.SignedAngle(new Vector2(catapultManager.hookTarget.forward.x, catapultManager.hookTarget.forward.z), new Vector2((catapultManager.hookTarget.position - catapultManager.planeCOM.position).x, (catapultManager.hookTarget.position - catapultManager.planeCOM.position).z));
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
      Debug.Log("ShooterHandler: " + text);
    }
  }
}
