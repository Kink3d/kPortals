using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using marijnz.EditorCoroutines;

namespace SimpleTools.Culling.Tests
{
	[ExecuteInEditMode]
	public class Occluders : MonoBehaviour 
	{

#if UNITY_EDITOR

        // ----------------------------------------------------------------------------------------------------//
        //                                               TEST                                                  //
        // ----------------------------------------------------------------------------------------------------//

        // --------------------------------------------------
        // Runtime Data

        [SerializeField]
        private MeshRenderer[] m_StaticRenderers;

		[SerializeField]
		private OccluderData[] m_Occluders;

		// --------------------------------------------------
        // Test Execution

        [ExecuteInEditMode]
        public void OnClickGenerate()
        {
            EditorCoroutines.StartCoroutine(Generate(), this);
        }

        [ExecuteInEditMode]
        public void OnClickCancel()
        {
			EditorCoroutines.StopAllCoroutines(this);
            ClearOccluderData();
            m_Occluders = null;
            m_StaticRenderers = null;
            UnityEditor.SceneView.RepaintAll();
        }

        private IEnumerator Generate()
		{
            m_StaticRenderers = Utils.GetStaticRenderers();
            yield return EditorCoroutines.StartCoroutine(Utils.BuildOccluderProxyGeometry(transform, m_StaticRenderers, value => m_Occluders = value, this, "Occluder"), this);
            UnityEditor.SceneView.RepaintAll();
        }

		[ExecuteInEditMode]
		private void ClearOccluderData()
		{
			m_Occluders = null;
			Transform container = transform.Find(Utils.occluderContainerName);
			if(container != null)
			{
				DestroyImmediate(container.gameObject);
			}
		}

		// ----------------------------------------------------------------------------------------------------//
		//                                              DEBUG                                                  //
		// ----------------------------------------------------------------------------------------------------//

		[ExecuteInEditMode]
        private void OnDrawGizmos()
        {
            DebugUtils.DrawOccluders(m_Occluders);
        }

#endif

    }
}
