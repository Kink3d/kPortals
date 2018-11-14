using System.Collections;
using UnityEngine;
using marijnz.EditorCoroutines;

namespace kTools.Portals.Tests
{
	[ExecuteInEditMode]
    [AddComponentMenu("kTools/Tests/Portals/Occluders")]
	public class Occluders : MonoBehaviour 
	{
        [SerializeField] private MeshRenderer[] m_StaticRenderers;
		[SerializeField] private OccluderData[] m_Occluders;
        
        public void OnClickGenerate()
        {
#if UNITY_EDITOR
            EditorCoroutines.StartCoroutine(Generate(), this);
#endif
        }

        public void OnClickCancel()
        {
#if UNITY_EDITOR
			EditorCoroutines.StopAllCoroutines(this);
            m_Occluders = null;
            m_StaticRenderers = null;
			Transform container = transform.Find(PortalUtils.occluderContainerName);
			if(container != null)
				DestroyImmediate(container.gameObject);
            UnityEditor.SceneView.RepaintAll();
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
#endif
        }

#if UNITY_EDITOR
        private IEnumerator Generate()
		{
            m_StaticRenderers = PortalUtils.GetStaticRenderers();
            yield return EditorCoroutines.StartCoroutine(PortalUtils.BuildOccluderProxyGeometry(transform, m_StaticRenderers, value => m_Occluders = value, this, "Occluder"), this);
            UnityEditor.SceneView.RepaintAll();
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        }

        private void OnDrawGizmos()
        {
            if(m_Occluders == null)
                return;

            PortalDebugUtils.DrawOccluders(m_Occluders);
        }
#endif

    }
}
