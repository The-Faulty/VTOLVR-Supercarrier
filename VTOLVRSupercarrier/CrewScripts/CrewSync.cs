using VTNetworking;

namespace VTOLVRSupercarrier.CrewScripts
{
  class CrewSync : VTNetSyncRPCOnly
  {
    CatapultCrewManager _crewManager;

    public override void OnNetInitialized()
    {
      _crewManager = GetComponent<CatapultCrewManager>();
      if (!isMine)
        return;
      _crewManager.OnStateChanged += OnStateChanged;
      _crewManager.OnStartAlign += OnStartAlign;
    }

    void OnStateChanged(AlignmentState state)
    {
      SendRPC("RPC_CarrierCrewSyncState", state);
    }

    void OnStartAlign(VehicleMaster vm)
    {
      SendRPC("RPC_CarrierCrewSyncAlign", vm);
    }

    [VTRPC]
    public void RPC_CarrierCrewSyncState(AlignmentState state)
    {
      _crewManager.SyncState(state);
      /*switch (state) 
      {
        case AlignmentState.None:
          _crewManager.ResetTrigger();
          break;
        case AlignmentState.Landing:
          _crewManager.LandingTrigger();
          break;
        case AlignmentState.Taxi:
        case AlignmentState.LaunchBar:
        case AlignmentState.Wings:
        case AlignmentState.Hook:
        case AlignmentState.LaunchReady:
        case AlignmentState.Runup:
        case AlignmentState.Launch:
          _crewManager.SyncState(state);
          break;
      }*/
    }
    [VTRPC]
    public void RPC_CarrierCrewSyncAlign(VehicleMaster vm)
    {
      _crewManager.StartAlign(vm);
    }
  }
}
