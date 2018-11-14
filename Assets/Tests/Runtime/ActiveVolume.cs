using System.Linq;
using UnityEngine;
using marijnz.EditorCoroutines;

namespace kTools.PortalsOld.Tests
{
	[ExecuteInEditMode]
    [AddComponentMenu("kTools/Tests/Portals/ActiveVolume")]
	public class ActiveVolume : MonoBehaviour 
	{
        [SerializeField] private Transform m_Target;
		[SerializeField] private int m_VolumeDensity;

        [SerializeField] private VolumeData m_VolumeData;
		[SerializeField] private VolumeData m_ActiveVolume;
		
        public void OnClickGenerate()
        {
#if UNITY_EDITOR
			m_VolumeData = null;
            MeshRenderer[] staticRenderers = PortalUtils.GetStaticRenderers();
			Bounds bounds = PortalUtils.GetSceneBounds(staticRenderers);

            EditorCoroutines.StartCoroutine(PortalUtils.BuildHierarchicalVolumeGrid(bounds, m_VolumeDensity, value => m_VolumeData = value, this), this);
            UnityEditor.SceneView.RepaintAll();
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
#endif
        }

        public void OnClickCancel()
        {
#if UNITY_EDITOR
            m_VolumeData = null;
            UnityEditor.SceneView.RepaintAll();
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
#endif
        }

        private void Update()
		{
			if(m_VolumeData == null || m_Target == null)
                return;
                
			PortalUtils.GetActiveVolumeAtPosition(m_VolumeData, m_Target.position, out m_ActiveVolume);
		}

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if(m_VolumeData == null || m_ActiveVolume == null || m_Target == null)
                return;

            PortalDebugUtils.DrawHierarchicalVolumeGrid(m_VolumeData, m_ActiveVolume);
			PortalDebugUtils.DrawSphere(m_Target.position, 0.25f);
        }
#endif

    }
}
