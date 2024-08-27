using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace VTOLVRSupercarrier
{
  public class CarrierLogger
  {
    private string origin;
    public CarrierLogger (object origin)
    {
      this.origin = nameof(origin);
    }
    public void Log(object message)
    {
      Debug.Log(origin + ": " + message);
    }
  }
}
