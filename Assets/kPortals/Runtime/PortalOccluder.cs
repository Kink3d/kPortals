using UnityEngine;

namespace kTools.Portals
{
	[ExecuteInEditMode]
    [AddComponentMenu("kTools/Portals/PortalOccluder")]
	public sealed class PortalOccluder : MonoBehaviour 
	{
		// -------------------------------------------------- //
        //                   PUBLIC METHODS                   //
        // -------------------------------------------------- //

		/// <summary>
        /// Converts this Monobehaviour to a SerializableOccluder struct for storage.
        /// </summary>
		public SerializableOccluder Serialize()
		{
			return new SerializableOccluder()
			{
				positionWS = transform.position,
				rotationWS = transform.rotation,
				scaleWS = transform.lossyScale,
				mesh = PortalUtil.cube
			};
		}

		// -------------------------------------------------- //
        //                       GIZMOS                       //
        // -------------------------------------------------- //

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
			Gizmos.DrawIcon(transform.position, "kTools/Portals/PortalVolume icon.png", true);
			PortalDebugUtils.DrawDebugCube(transform.position, transform.rotation, transform.lossyScale, PortalDebugColors.occluder);
        }
#endif
	}
}
