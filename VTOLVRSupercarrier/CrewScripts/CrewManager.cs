using System;
using System.Collections.Generic;
using UnityEngine;
using VTOLAPI;

namespace VTOLVRSupercarrier.CrewScripts
{
  // this isnt really needed anymore but I'm just going to leave it here
  public class CrewManager : MonoBehaviour
  {
    public enum TaxiState
    {
      None,
      Waiting,
      TaxiToCatapult,
      HoldShort,
      Align
    }
    public struct VehicleInQueue
    {
      public VehicleInQueue(GameObject v, CarrierCatapult c)
      {
        vehicle = v;
        catapult = c;
        catNumber = c.catapultDesignation;
        state = TaxiState.None;
      }
      public GameObject vehicle;
      public CarrierCatapult catapult;
      public int catNumber;
      public TaxiState state;
    }
    public List<VehicleInQueue> vehicleQueue = new List<VehicleInQueue>();

    public event Action<VehicleInQueue> StartAlignment;

    public AICarrierSpawn carrier;

    public void takeoffRequest(CarrierCatapult cat, GameObject vehicle)
    {
      Log("Takeoff request on cat " + cat.catapultDesignation);
      if (vehicle == null)
      {
        vehicle = VTAPI.GetPlayersVehicleGameObject();
      }
      VehicleInQueue data = new VehicleInQueue(vehicle, cat);
      vehicleQueue.Add(data);
      StartAlignment.Invoke(data);
    }
    private void Log(object text)
    {
      Debug.Log("CrewManager: " + text);
    }
  }
}