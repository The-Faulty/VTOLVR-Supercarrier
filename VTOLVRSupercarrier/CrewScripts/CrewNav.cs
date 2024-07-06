using System.Collections;
using UnityEngine;

public class CrewNav : MonoBehaviour
{
  public Transform CharacterTransform;

  public Animator anim;

  public float remainingDistance;
  public float JogSpeed = 2.5f;
  public float WalkSpeed = 1.5f;
  public float BackupSpeed = 1f;

  private bool isMoving = false;

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
    BackupSpeed = 1f;
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
    float distance = Vector3.Distance(startPos, pos);
    remainingDistance = distance;

    anim.SetBool("jog", true);
    anim.SetBool("idle", false);
    while (remainingDistance > 0)
    {
      lookPos = pos - CharacterTransform.localPosition;
      lookPos.y = 0;
      rotation = Quaternion.LookRotation(lookPos);
      CharacterTransform.localRotation = Quaternion.Slerp(CharacterTransform.localRotation, rotation, Time.deltaTime * 2);
      CharacterTransform.localPosition = Vector3.Lerp(startPos, pos, 1 - (remainingDistance / distance));
      if (remainingDistance > 2)
      {
        remainingDistance -= JogSpeed * Time.deltaTime;
      } else
      {
        anim.SetBool("jog", false);
        anim.SetBool("walk", true);
        remainingDistance -= WalkSpeed * Time.deltaTime;
      }
      yield return new WaitForFixedUpdate();
    }
    anim.SetBool("walk", false);
    anim.SetBool("idle", true);
  }

  public void SetMovingDesitination(GameObject target, bool backwards = false)
  {
    StartCoroutine(MoveToMovingAsync(target, backwards));
  }

  private IEnumerator MoveToMovingAsync(GameObject target, bool backwards)
  {
    Vector3 pos = target.transform.position;
    Log(CharacterTransform.gameObject);
    Log("Move to" + pos);

    Vector3 lookPos;
    Quaternion rotation;
    Vector3 startPos = CharacterTransform.localPosition;
    float distance = Vector3.Distance(startPos, pos);
    remainingDistance = distance;
    
    anim.SetBool("idle", false);
    if (backwards)
    {
      anim.SetBool("backup", true);
    } else
    {
      anim.SetBool("walk", true);
    }

    while (remainingDistance > 0)
    {
      pos = target.transform.position;
      lookPos = CharacterTransform.localPosition - pos;
      lookPos.y = 0;
      rotation = Quaternion.LookRotation(lookPos);
      CharacterTransform.localRotation = Quaternion.Slerp(CharacterTransform.localRotation, rotation, Time.deltaTime * 2);
      CharacterTransform.localPosition = Vector3.Lerp(startPos, pos, 1 - (remainingDistance / distance));
      remainingDistance -= BackupSpeed * Time.deltaTime;
      yield return new WaitForFixedUpdate();
    }
    anim.SetBool("walk", false);
    anim.SetBool("backup", false);
    anim.SetBool("idle", true);
  }

  private void Log(object text)
  {
    Debug.Log("CrewNav: " + text);
  }
}
