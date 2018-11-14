using System.Linq;
using UnityEngine;
using marijnz.EditorCoroutines;

namespace kTools.Portals.Tests
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
            MeshRenderer[] staticRenderers = Utils.GetStaticRenderers();
			Bounds bounds = Utils.GetSceneBounds(staticRenderers);

            EditorCoroutines.StartCoroutine(Utils.BuildHierarchicalVolumeGrid(bounds, m_VolumeDensity, value => m_VolumeData = value, this), this);
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
                
			Utils.GetActiveVolumeAtPosition(m_VolumeData, m_Target.position, out m_ActiveVolume);
		}

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if(m_VolumeData == null || m_ActiveVolume == null || m_Target == null)
                return;

            DebugUtils.DrawHierarchicalVolumeGrid(m_VolumeData, m_ActiveVolume);
			DebugUtils.DrawSphere(m_Target.position, 0.25f);
        }
#endif

    }
}
