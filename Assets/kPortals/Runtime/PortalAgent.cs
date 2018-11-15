using UnityEngine;

namespace kTools.Portals
{
    [AddComponentMenu("kTools/Portals/Portal Agent")]
	public class PortalAgent : MonoBehaviour 
	{
		// -------------------------------------------------- //
        //                  INTERNAL METHODS                  //
        // -------------------------------------------------- //
		
		private void OnEnable()
		{
			if(PortalSystem.Instance)
				PortalSystem.Instance.RegisterAgent(this);
		}

		private void OnDisable()
		{
			if(PortalSystem.Instance)
				PortalSystem.Instance.UnregisterAgent(this);
		}
	}
}
