using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace VTOLVRSupercarrier
{
  public class CarrierLogger
  {
    private object origin;
    public CarrierLogger (object origin)
    {
      this.origin = origin;
    }
    public void Log(object message)
    {
      Debug.Log(origin + ": " + message);
    }
  }
}
