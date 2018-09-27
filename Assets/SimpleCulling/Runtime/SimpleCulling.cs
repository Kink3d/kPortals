using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimpleTools.Culling
{
	public class SimpleCulling : MonoBehaviour 
	{
		public enum DebugMode { None, Occluders, Volumes }

		// ----------------------------------------------------------------------------------------------------//
		//                                           PUBLIC FIELDS                                             //
		// ----------------------------------------------------------------------------------------------------//

		private string m_OccluderTag = "Occluder"; // TD - Expose and add option for layer

		[SerializeField]
        private int m_VolumeDensity = 4;

		[SerializeField]
        private DebugMode m_DebugMode = DebugMode.None;

		// ----------------------------------------------------------------------------------------------------//
		//                                              RUNTIME                                                //
		// ----------------------------------------------------------------------------------------------------//

		// --------------------------------------------------
		// Runtime Data

		[SerializeField]
		private bool m_GenerateVolumes = true;
		[SerializeField]
		private bool m_GenerateOccluders = true;

		[SerializeField]
		private VolumeData m_VolumeData;

		[SerializeField]
		private OccluderData[] m_Occluders;

		// ----------------------------------------------------------------------------------------------------//
		//                                              EDITOR                                                 //
		// ----------------------------------------------------------------------------------------------------//

#if UNITY_EDITOR

		// --------------------------------------------------
		// Editor State

		private bool m_IsGenerating;
		public bool isGenerating { get { return m_IsGenerating; } }

		// --------------------------------------------------
		// Editor Data
        
		private MeshRenderer[] m_StaticRenderers;

		// --------------------------------------------------
		// Interface

		[ExecuteInEditMode]
        public void OnClickGenerate()
        {
            m_IsGenerating = true;
            m_StaticRenderers = Utils.GetStaticRenderers();

			if(m_GenerateOccluders)
			{
				ClearOccluderProxyGeometry();
                m_Occluders = Utils.BuildOccluderProxyGeometry(transform, m_StaticRenderers, m_OccluderTag);
			}
			if(m_GenerateVolumes)
			{
				ClearHierarchicalVolumeGrid();
                Bounds bounds = Utils.GetSceneBounds(m_StaticRenderers);
				m_VolumeData = Utils.BuildHierarchicalVolumeGrid(bounds, m_VolumeDensity);
			}
            m_IsGenerating = false;
        }

        [ExecuteInEditMode]
        public void OnClickCancel()
        {
            ClearOccluderProxyGeometry();
			ClearHierarchicalVolumeGrid();
        }

		// --------------------------------------------------
		// Occluder Proxy Geometry

		[ExecuteInEditMode]
		private void ClearOccluderProxyGeometry()
		{
			m_Occluders = null;
			Transform container = transform.Find(Utils.occluderContainerName);
			if(container != null)
			{
				DestroyImmediate(container.gameObject);
			}
		}

		// --------------------------------------------------
		// Hirarchical Volume Grid

		[ExecuteInEditMode]
		private void ClearHierarchicalVolumeGrid()
		{
			m_VolumeData = null;
		}

		// ----------------------------------------------------------------------------------------------------//
		//                                              DEBUG                                                  //
		// ----------------------------------------------------------------------------------------------------//

		[ExecuteInEditMode]
        private void OnDrawGizmos()
        {
            if (m_DebugMode == DebugMode.Occluders)
            {
				DrawOccluderDebug();
            }
			if (m_DebugMode == DebugMode.Volumes)
            {
				DrawHierarchicalVolumeDebug();
            }
        }

		private void DrawOccluderDebug()
		{
			if(m_Occluders == null)
				return;
					
			for(int i = 0; i < m_Occluders.Length; i++)
			{
				Transform transform = m_Occluders[i].collider.transform;
				Gizmos.color = EditorColors.occluderFill;
				Gizmos.DrawMesh(m_Occluders[i].collider.sharedMesh, transform.position, transform.rotation, transform.lossyScale);
				Gizmos.color = EditorColors.occluderWire;
				Gizmos.DrawWireMesh(m_Occluders[i].collider.sharedMesh, transform.position, transform.rotation, transform.lossyScale);
			}
		}

		private void DrawHierarchicalVolumeDebug()
		{
			if(m_VolumeData == null)
				return;

			IterateHierarchicalVolumeDebug(m_VolumeData);
		}

        public static void IterateHierarchicalVolumeDebug(VolumeData data)
        {
            if (data.children != null && data.children.Length > 0)
            {
                for (int i = 0; i < data.children.Length; i++)
                    IterateHierarchicalVolumeDebug(data.children[i]);
            }
            else
            {
                Gizmos.color = EditorColors.volumeFill;
                Gizmos.DrawCube(data.bounds.center, data.bounds.size);
                Gizmos.color = EditorColors.volumeWire;
                Gizmos.DrawWireCube(data.bounds.center, data.bounds.size);
            }
        }

#endif

    }
}