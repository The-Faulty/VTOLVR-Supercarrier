using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
  public Transform characterTarget;
  public Transform gameTarget;

  public RectTransform playerUI;
  public RectTransform backgroundUI;

  public Text angleDisplay;
  public Text alignDisplay;
  public Text moveDisplay;

  public bool isAssigned;

  Vector2 backgroundSize;
  Vector2 relativeDistance;

  int maxDistance = 10;
  float worldUIConversion;
  float uiOffset;

  float relativeAngle;

  bool isAligned = false;


  void Start()
  {
    backgroundSize = new Vector2(backgroundUI.rect.width, backgroundUI.rect.height);
    uiOffset = backgroundSize.y - playerUI.rect.height;
    worldUIConversion = backgroundSize.y / maxDistance;
    
    print(worldUIConversion);
  }

  void Update()
  {
    if (!gameTarget || !characterTarget)
    {
      Debug.Log("Alignment has no target!");
      return;
    }
    if (isAssigned)
    {
      relativeAngle = Vector2.SignedAngle(new Vector2(gameTarget.forward.x, gameTarget.forward.z), new Vector2((gameTarget.position - characterTarget.position).x, (gameTarget.position - characterTarget.position).z));
      angleDisplay.text = relativeAngle.ToString();

      worldToUI(characterTarget, gameTarget);
      howMove();
      checkAlign();
      //print((characterTarget.position - gameTarget.position).sqrMagnitude);
      //print(Vector3.Dot(characterTarget.forward, gameTarget.forward));
    }
  }

  //Unity has y and z flipped from standard convention
  void worldToUI(Transform player, Transform target)
  {
    relativeDistance = new Vector2(player.position.x - target.position.x, player.position.z - (target.position.z));

    playerUIsetPosition(Mathf.Sin(Mathf.Deg2Rad * relativeAngle) * relativeDistance.magnitude, Mathf.Cos(Mathf.Deg2Rad * relativeAngle) * relativeDistance.magnitude);
  }

  void playerUIsetPosition(float x, float y)
  {
    x *= worldUIConversion;
    y = uiOffset - (y * worldUIConversion);

    //check collisions
    if (y < playerUI.rect.height) y = playerUI.rect.height;
    if (x < -(backgroundSize.x / 2) + playerUI.rect.width / 2) x = -(backgroundSize.x / 2) + (playerUI.rect.width / 2);
    if (x > (backgroundSize.x / 2) - playerUI.rect.width / 2) x = (backgroundSize.x / 2) - (playerUI.rect.width / 2);

    playerUI.anchoredPosition = new Vector2(x, y);
    playerUI.eulerAngles = new Vector3(0, 0, Vector3.SignedAngle(characterTarget.forward, gameTarget.forward, Vector3.up));
  }

  void checkAlign()
  {
    if (Vector3.Dot(characterTarget.forward, gameTarget.forward) > 0.5 && !isAligned)
    {
      isAligned = true;
      alignDisplay.text = "true";
    }
    else if (Vector3.Dot(characterTarget.forward, gameTarget.forward) < 0.5 && isAligned)
    {
      isAligned = false;
      alignDisplay.text = "false";
    }
  }

  void howMove()
  {
    if (relativeAngle > 10)
    {
      moveDisplay.text = "left";
    }
    else if (relativeAngle < -10)
    {
      moveDisplay.text = "right";
    }
    else
    {
      moveDisplay.text = "forward";
    }
  }
}
