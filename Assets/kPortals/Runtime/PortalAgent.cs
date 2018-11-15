using UnityEngine;

namespace kTools.Portals
{
    [AddComponentMenu("kTools/Portals/Portal Agent")]
	public class PortalAgent : MonoBehaviour 
	{
		private int m_ActiveVolumeID = -1;
		public int activeVolumeID
		{
			get { return m_ActiveVolumeID; }
			set { m_ActiveVolumeID = value; }
		}

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
