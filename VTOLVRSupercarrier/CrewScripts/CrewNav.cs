using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrewNav : MonoBehaviour
{
  public Transform CharacterTransform;

  public Animator anim;

  public float remainingDistance;
  public float JogSpeed = 3f;
  public float WalkSpeed = 1.5f;


  public CrewNav(Transform charT)
  {
    CharacterTransform = charT;
  }

  void OnEnable()
  {
    Log("Walk Speed - " + WalkSpeed);
    Log("Jog Speed - " + JogSpeed);
    WalkSpeed = 1.5f;
    JogSpeed = 3f;
    anim = GetComponent<Animator>();
    Debug.Log(anim);
  }

  public void SetDestination(Vector3 pos)
  {
    StartCoroutine(MoveToAsync(pos));
  }

  private IEnumerator MoveToAsync(Vector3 pos)
  {
    Log(CharacterTransform.gameObject);
    Log(CharacterTransform);
    Log("Move to" + pos);
    
    Vector3 lookPos;
    Quaternion rotation;
    Vector3 startPos = CharacterTransform.localPosition;
    Log(startPos);
    float distance = Vector3.Distance(startPos, pos);
    remainingDistance = distance;

    anim.SetBool("walk", true);
    anim.SetBool("idle", false);
    while (remainingDistance > 0)
    {
      lookPos = pos - CharacterTransform.localPosition;
      lookPos.y = 0;
      rotation = Quaternion.LookRotation(lookPos);
      CharacterTransform.localRotation = Quaternion.Slerp(CharacterTransform.localRotation, rotation, Time.deltaTime * 2);
      CharacterTransform.localPosition = Vector3.Lerp(startPos, pos, 1 - (remainingDistance / distance));
      if (remainingDistance > 5)
      {
        remainingDistance -= JogSpeed * Time.deltaTime;
      } else
      {
        remainingDistance -= WalkSpeed * Time.deltaTime;
      }
      yield return new WaitForFixedUpdate();
    }
    anim.SetBool("walk", false);
    anim.SetBool("idle", true);
  }

  private void Log(object text)
  {
    Debug.Log("CrewNav: " + text);
  }
}
