using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrewNav : MonoBehaviour
{
  public Transform CharacterTransform;

  public Animator anim;

  public float remainingDistance;
  public float MoveSpeed = 1.5f;

  public CrewNav(Transform charT)
  {
    CharacterTransform = charT;
  }

  void OnEnable()
  {
    MoveSpeed = 1.5f;
    anim = GetComponent<Animator>();
    Debug.Log(anim);
  }

  public void SetDestination(Vector3 pos)
  {
    StartCoroutine(MoveToAsync(pos));
  }

  private IEnumerator MoveToAsync(Vector3 pos)
  {
    Debug.Log(CharacterTransform.gameObject);
    Debug.Log(CharacterTransform);
    Debug.Log("Move to" + pos);
    
    Vector3 lookPos;
    Quaternion rotation;
    Vector3 startPos = CharacterTransform.localPosition;
    Debug.Log(startPos);
    float distance = Vector3.Distance(startPos, pos);
    remainingDistance = distance;
    Debug.Log("distance");
    anim.SetBool("walk", true);
    Debug.Log("walk");
    anim.SetBool("idle", false);
    while (remainingDistance > 0)
    {
      lookPos = pos - CharacterTransform.localPosition;
      lookPos.y = 0;
      rotation = Quaternion.LookRotation(lookPos);
      CharacterTransform.localRotation = Quaternion.Slerp(CharacterTransform.localRotation, rotation, Time.deltaTime * 2);
      CharacterTransform.localPosition = Vector3.Lerp(startPos, pos, 1 - (remainingDistance / distance));
      remainingDistance -= MoveSpeed * Time.deltaTime;
      yield return null;
    }
    anim.SetBool("walk", false);
    anim.SetBool("idle", true);
  }
}
