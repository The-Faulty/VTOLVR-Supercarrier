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
      _crewManager.OnLanding += Landing;
      _crewManager.Reset += Reset;
    }

    public void Landing()
    {
      SendRPC("RPC_CarrierCrewSyncLanding", 1);
    }

    public void Reset()
    {
      SendRPC("RPC_CarrierCrewSyncLanding", 0);
    }

    [VTRPC]
    public void RPC_CarrierCrewSyncLanding(int state)
    {
      if (state == 1)
      {
        _crewManager.LandingTrigger();
        return;
      }
      _crewManager.ResetTrigger();
    }
  }
}
