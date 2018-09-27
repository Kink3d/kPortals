using System;
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

		private VolumeData m_VolumeData;

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

		private static string occluderContainerName = "OccluderProxies";
		private MeshRenderer[] m_StaticRenderers;
		private OccluderData[] m_Occluders;

		// --------------------------------------------------
		// Interface

		[ExecuteInEditMode]
        public void OnClickGenerate()
        {
			m_StaticRenderers = GetStaticRenderers();

			ClearOccluderProxyGeometry();
            BuildOccluderProxyGeometry();

			ClearHierarchicalVolumeGrid();
			BuildHierarchicalVolumeGrid();
        }

        [ExecuteInEditMode]
        public void OnClickCancel()
        {
            ClearOccluderProxyGeometry();
			ClearHierarchicalVolumeGrid();
        }

		// --------------------------------------------------
		// Static Renderers

		[ExecuteInEditMode]
		private MeshRenderer[] GetStaticRenderers()
		{
			return FindObjectsOfType<MeshRenderer>().Where(s => s.gameObject.isStatic).ToArray();
		}

		// --------------------------------------------------
		// Occluder Proxy Geometry

		[ExecuteInEditMode]
		private void BuildOccluderProxyGeometry()
		{
			m_IsGenerating = true;

			Transform container = Utils.NewObject(occluderContainerName, transform).transform;
			List<MeshRenderer> occluderRenderers = m_StaticRenderers.Where(s => s.gameObject.tag == m_OccluderTag).ToList();
			m_Occluders = new OccluderData[occluderRenderers.Count];
			for(int i = 0; i < m_Occluders.Length; i++)
			{
				GameObject occluderObj = occluderRenderers[i].gameObject;
				Transform occluderTransform = occluderObj.transform;
				GameObject proxyObj = Utils.NewObject(occluderObj.name, container, occluderTransform.position,occluderTransform.rotation, occluderTransform.lossyScale);
				MeshCollider proxyCollider = proxyObj.AddComponent<MeshCollider>();
				proxyCollider.sharedMesh = occluderRenderers[i].GetComponent<MeshFilter>().sharedMesh;
				proxyCollider.enabled = false;
				m_Occluders[i] = new OccluderData(proxyCollider);
			}

			m_IsGenerating = false;
		}

		[ExecuteInEditMode]
		private void ClearOccluderProxyGeometry()
		{
			m_Occluders = null;
			Transform container = transform.Find(occluderContainerName);
			if(container != null)
			{
				DestroyImmediate(container.gameObject);
			}
		}

		// --------------------------------------------------
		// Hirarchical Volume Grid

		[ExecuteInEditMode]
		private void BuildHierarchicalVolumeGrid()
		{
			VolumeData data = new VolumeData(GetSceneBounds(), null, null);
			int count = 0;
			if(count < m_VolumeDensity)
			{
				Utils.IterateHierarchicalVolumeGrid(count, m_VolumeDensity, ref data);
			}
			m_VolumeData = data;
		}

		[ExecuteInEditMode]
		private void ClearHierarchicalVolumeGrid()
		{
			m_VolumeData = null;
		}

		[ExecuteInEditMode]
		private Bounds GetSceneBounds()
		{
			Bounds sceneBounds = new Bounds(Vector3.zero, Vector3.zero);
			for(int i = 0; i < m_StaticRenderers.Length; i++)
			{
				sceneBounds.Encapsulate(m_StaticRenderers[i].bounds);
			}
			float maxSize = Mathf.Max(Mathf.Max(sceneBounds.size.x, sceneBounds.size.y), sceneBounds.size.z);
			sceneBounds.size = new Vector3(maxSize, maxSize, maxSize);
			return sceneBounds;
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

			Utils.IterateHierarchicalVolumeDebug(m_VolumeData);
		}

#endif
		
	}
}