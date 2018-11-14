using UnityEngine;

namespace kTools.Portals
{
	[ExecuteInEditMode]
    [AddComponentMenu("kTools/Portals/PortalVolume")]
	public sealed class PortalVolume : MonoBehaviour 
	{
		// -------------------------------------------------- //
        //                       GIZMOS                       //
        // -------------------------------------------------- //

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
			Gizmos.DrawIcon(transform.position, "kTools/Portals/PortalVolume icon.png", true);
			PortalDebugUtils.DrawDebugCube(transform.position, transform.rotation, transform.lossyScale, PortalDebugColors.volume);
        }
#endif
	}
}
