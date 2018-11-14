using UnityEngine;

namespace kTools.Portals
{
	[ExecuteInEditMode]
    [AddComponentMenu("kTools/Portals/PortalOccluder")]
	[RequireComponent(typeof(BoxCollider))]
	public sealed class PortalOccluder : MonoBehaviour 
	{
		// -------------------------------------------------- //
        //                   PRIVATE FIELDS                   //
        // -------------------------------------------------- //
		
		private BoxCollider m_BoxCollider;
		public BoxCollider boxCollider
		{
			get 
			{
				if(m_BoxCollider == null)
					m_BoxCollider = GetComponent<BoxCollider>();
				return m_BoxCollider;
			}
		}

		// -------------------------------------------------- //
        //                ENGINE LOOP METHODS                 //
        // -------------------------------------------------- //

		private void OnEnable()
		{
#if UNITY_EDITOR
            // Collapse BoxCollider UI as user shouldnt edit it
			UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(boxCollider, false);
#endif
		}

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
