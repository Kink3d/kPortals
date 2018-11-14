using UnityEngine;

namespace kTools.Portals
{
	[ExecuteInEditMode]
    [AddComponentMenu("kTools/Portals/PortalVolume")]
	[RequireComponent(typeof(BoxCollider))]
	public sealed class PortalVolume : MonoBehaviour 
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
