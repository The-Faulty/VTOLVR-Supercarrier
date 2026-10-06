﻿using System;
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
    private LineRenderer alignLine;
    private TextMesh alignAngleText;
    private WheelsController wheelsController;

    public override void OnEnable()
    {
      base.OnEnable();
      if (logger == null)
      {
        logger = new CarrierLogger("DirectorHandler");
      }
      alignPoint = catapultManager.navPoints.directorAlignPoint;
      mainPoint = catapultManager.navPoints.directorMainPoint;
      landingPoint = catapultManager.navPoints.directorLandPoint;
      //logger.Log("Enable Finish");
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
      LookAt(catapultManager.navPoints.preHookAlignPoint);
    }

    protected override void OnLanding()
    {
      navAgent.SetDestination(landingPoint.localPosition);
      LookAt(mainPoint);
    }

    /**
     * Makes the director steer the players plane in the direction of the target
     */
    void Align(Transform target)
    {      
      Vector3 alignmentOrigin = GetAlignmentPoint();
      if(Main.DEBUG){
        // create a new line from the nosewheel to the catapult shuttle
        // only needs to be run the first time
        if (alignLine == null)
        {
          GameObject lineObject = new GameObject("Director Alignment Line");
          lineObject.transform.SetParent(transform, false);
          alignLine = lineObject.AddComponent<LineRenderer>();
          alignLine.positionCount = 2;
          alignLine.startWidth = 0.05f;
          alignLine.endWidth = 0.05f;
          alignLine.startColor = Color.yellow;
          alignLine.endColor = Color.yellow;
          alignLine.useWorldSpace = true;
          alignLine.material = new Material(Shader.Find("Sprites/Default"));
        }

        alignLine.SetPosition(0, alignmentOrigin);
        alignLine.SetPosition(1, target.position);
      }
      
      Vector3 dirToTarget = target.position - alignmentOrigin;

      // Signed angle around the Y-axis (up) relative to the target's forward vector
      float relativeAngle = Vector3.SignedAngle(dirToTarget, catapultManager.vehicle.transform.forward,  Vector3.up);

      if(Main.DEBUG){
        // add the current detected angle above the shuttle / target
        if (alignAngleText == null)
        {
          GameObject textObject = new GameObject("Director Alignment Angle");
          alignAngleText = textObject.AddComponent<TextMesh>();
          alignAngleText.anchor = TextAnchor.LowerCenter;
          alignAngleText.alignment = TextAlignment.Center;
          alignAngleText.characterSize = 0.15f;
          alignAngleText.fontSize = 48;
          alignAngleText.color = Color.white;
        }

        alignAngleText.text = $"{relativeAngle:F1}°";
        alignAngleText.transform.position = target.position + Vector3.up * 0.5f;
        if (Camera.main != null)
        {
          // make sure that it aligns to the player cam instead of somewhere else
          alignAngleText.transform.rotation = Camera.main.transform.rotation;
        }
      }

      // Angle is positive when target is to the LEFT, negative when to the RIGHT
      if (relativeAngle > 10f)
      {
        // LEFT
        anim.SetFloat("alignBlend", -1f, 0.3f, Time.deltaTime);
        if(Main.DEBUG){
          alignLine.startColor = Color.red;
          alignLine.endColor = Color.red;
        }
      }
      else if (relativeAngle < -10f)
      {
        // RIGHT
        anim.SetFloat("alignBlend", 1f, 0.3f, Time.deltaTime);
        if (Main.DEBUG)
        {

          alignLine.startColor = Color.blue;
          alignLine.endColor = Color.blue;
        }
      }
      else
      {
        // FORWARD
        anim.SetFloat("alignBlend", 0f, 0.3f, Time.deltaTime);
        if (Main.DEBUG)
        {
          alignLine.startColor = Color.green;
          alignLine.endColor = Color.green;
        }
      }
    }

    /**
     * Function for getting the position of the alignment point for the current aircraft.
     * This will try to return the nose wheel but returns the centre of mass if that fails.
     */
    private Vector3 GetAlignmentPoint()
    {
      if (wheelsController == null)
      {
        wheelsController = catapultManager.vehicle.GetComponentInChildren<WheelsController>(true);
      }

      if (wheelsController != null && 
          wheelsController.suspensions != null &&
          wheelsController.steeringSuspensionIndex >= 0 &&
          wheelsController.steeringSuspensionIndex < wheelsController.suspensions.Length)
      {
        RaySpringDamper noseWheel = wheelsController.suspensions[wheelsController.steeringSuspensionIndex];
        if (noseWheel != null)
        {
          return noseWheel.isTouching ? noseWheel.point : noseWheel.transform.position;
        }
      }

      return catapultManager.planeCOM.position;
    }

    private void ResetAnimVars()
    {
      anim.SetBool("align", false);
      anim.SetBool("bar", false);
      anim.SetBool("launch", false);
    }
  }
}
