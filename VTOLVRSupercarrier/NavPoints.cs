using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace VTOLVRSupercarrier
{
  public class NavPoints : MonoBehaviour
  {
    public Transform shooterIdlePoint;
    public Transform shooterMainPoint;
    public Transform shooterLandPoint;

    public Transform directorMainPoint;
    public Transform directorAlignPoint;
    public Transform directorLandPoint;

    public Transform greenIdlePoint;
    public Transform greenMainPoint;
    public Transform greenAlignPoint;
    public Transform greenLandPoint;

    public Transform preHookAlignPoint;
  }
}