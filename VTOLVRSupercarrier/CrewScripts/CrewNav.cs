using System.Collections;
using UnityEngine;

namespace VTOLVRSupercarrier.CrewScripts
{
  public class CrewNav : MonoBehaviour
  {
    public Animator anim;

    public float remainingDistance;
    public float JogSpeed = 2.5f;
    public float WalkSpeed = 1.5f;
    public float BackupSpeed = 1f;

    private bool isMoving = false;

    void OnEnable()
    {
      Log("Walk Speed - " + WalkSpeed);
      Log("Jog Speed - " + JogSpeed);
      WalkSpeed = 1.5f;
      JogSpeed = 2.5f;
      BackupSpeed = 1f;
      anim = GetComponentInChildren<Animator>();
      Debug.Log(anim);
    }

    public void SetDestination(Vector3 pos)
    {
      StartCoroutine(MoveToAsync(pos));
    }

    private IEnumerator MoveToAsync(Vector3 pos)
    {
      Log(transform.gameObject);
      Log(transform);
      Log("Move to" + pos);

      Vector3 lookPos;
      Quaternion rotation;
      Vector3 startPos = transform.localPosition;
      float distance = Vector3.Distance(startPos, pos);
      remainingDistance = distance;


      while (remainingDistance > 0.00001f)
      {
        lookPos = pos - transform.localPosition;
        lookPos.y = 0;
        rotation = Quaternion.LookRotation(lookPos);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, rotation, Time.deltaTime * 2);
        transform.localPosition = Vector3.Lerp(startPos, pos, 1 - (remainingDistance / distance));
        remainingDistance -= anim.GetFloat("walkBlend") * Time.deltaTime;
        if (remainingDistance > 2)
        {
          anim.SetFloat("walkBlend", JogSpeed, 0.4f, Time.deltaTime);
        }
        else if (remainingDistance > 0.2f)
        {
          anim.SetFloat("walkBlend", WalkSpeed, 0.2f, Time.deltaTime);
        }
        else
        {
          anim.SetFloat("walkBlend", 0f, 0.2f, Time.deltaTime);
          if (anim.GetFloat("walkBlend") < 0.000001f)
          {
            anim.SetFloat("walkBlend", 0f);
            transform.localPosition = pos;
            remainingDistance = 0;
          }
        }
        yield return new WaitForFixedUpdate();
      }
      anim.SetFloat("walkBlend", 0f);
      transform.localPosition = pos;
      remainingDistance = 0;
      Log("Finished Moving");
    }


    public void SetMovingDesitination(GameObject target, bool backwards = false)
    {
      StartCoroutine(MoveToMovingAsync(target, backwards));
    }

    private IEnumerator MoveToMovingAsync(GameObject target, bool backwards)
    {
      Vector3 pos = target.transform.position;
      Log(transform.gameObject);
      Log("Move to" + pos);

      Vector3 lookPos;
      Quaternion rotation;
      Vector3 startPos = transform.localPosition;
      float distance = Vector3.Distance(startPos, pos);
      remainingDistance = distance;

      anim.SetBool("idle", false);
      if (backwards)
      {
        anim.SetBool("backup", true);
      }
      else
      {
        anim.SetBool("walk", true);
      }

      while (remainingDistance > 0)
      {
        pos = target.transform.position;
        lookPos = transform.localPosition - pos;
        lookPos.y = 0;
        rotation = Quaternion.LookRotation(lookPos);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, rotation, Time.deltaTime * 2);
        transform.localPosition = Vector3.Lerp(startPos, pos, 1 - (remainingDistance / distance));
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
}