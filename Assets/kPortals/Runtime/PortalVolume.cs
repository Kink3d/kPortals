using UnityEngine;

namespace kTools.Portals
{
	[ExecuteInEditMode]
    [AddComponentMenu("kTools/Portals/Portal Volume")]
	public sealed class PortalVolume : MonoBehaviour 
	{
		// -------------------------------------------------- //
        //                   PUBLIC METHODS                   //
        // -------------------------------------------------- //

		/// <summary>
        /// Converts this Monobehaviour to a SerializableVolume struct for storage.
        /// </summary>
		public SerializableVolume Serialize(int index)
		{
			return new SerializableVolume()
			{
				positionWS = transform.position,
				rotationWS = transform.rotation,
				scaleWS = transform.lossyScale,
				volumeID = index,
				parentID = -1,
				childIDs = null
			};
		}

		// -------------------------------------------------- //
        //                       GIZMOS                       //
        // -------------------------------------------------- //

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
			Gizmos.DrawIcon(transform.position, "kTools/Portals/PortalVolume icon.png", true);
			PortalDebugUtil.DrawCube(transform.position, transform.rotation, transform.lossyScale, PortalDebugColors.volume);
        }
#endif
	}
}
