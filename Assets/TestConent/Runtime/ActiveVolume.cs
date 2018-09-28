using System.Linq;
using UnityEngine;
using marijnz.EditorCoroutines;

namespace SimpleTools.Culling.Tests
{
	[ExecuteInEditMode]
	public class ActiveVolume : MonoBehaviour 
	{
		// ----------------------------------------------------------------------------------------------------//
        //                                           PUBLIC FIELDS                                             //
        // ----------------------------------------------------------------------------------------------------//

        [SerializeField]
		private Transform m_Target;

		[SerializeField]
		public int m_VolumeDensity;

        // ----------------------------------------------------------------------------------------------------//
        //                                               TEST                                                  //
        // ----------------------------------------------------------------------------------------------------//

        // --------------------------------------------------
        // Runtime Data

        [SerializeField]
        private VolumeData m_VolumeData;

		[SerializeField]
		private VolumeData m_ActiveVolume;

        // --------------------------------------------------
        // Test Execution
		
		[ExecuteInEditMode]
        public void OnClickGenerate()
        {
			m_VolumeData = null;
            MeshRenderer[] staticRenderers = Utils.GetStaticRenderers();
			Bounds bounds = Utils.GetSceneBounds(staticRenderers);
            EditorCoroutines.StartCoroutine(Utils.BuildHierarchicalVolumeGrid(bounds, m_VolumeDensity, value => m_VolumeData = value, this), this);

            UnityEditor.SceneView.RepaintAll();
        }

        [ExecuteInEditMode]
        public void OnClickCancel()
        {
            m_VolumeData = null;
            UnityEditor.SceneView.RepaintAll();
        }

		private void Update()
		{
			if(m_VolumeData != null && m_Target != null)
				Utils.GetActiveVolumeAtPosition(m_VolumeData, m_Target.position, out m_ActiveVolume);
		}

        // ----------------------------------------------------------------------------------------------------//
        //                                              DEBUG                                                  //
        // ----------------------------------------------------------------------------------------------------//

        [ExecuteInEditMode]
        private void OnDrawGizmos()
        {
            DebugUtils.DrawHierarchicalVolumeGrid(m_VolumeData, m_ActiveVolume);
			DebugUtils.DrawSphere(m_Target.position, 0.25f);
        }
	}
}
