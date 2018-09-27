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
		//                                              RUNTIME                                                //
		// ----------------------------------------------------------------------------------------------------//

		// --------------------------------------------------
		// Public Fields

		public string occluderTag = "Occluder"; // TD - Add option for layer

		[SerializeField]
        int m_VolumeDensity = 4;

		[SerializeField]
        DebugMode m_DebugMode = DebugMode.None;

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
			List<MeshRenderer> occluderRenderers = m_StaticRenderers.Where(s => s.gameObject.tag == occluderTag).ToList();
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
				IterateHierarchicalVolumeGrid(count, ref data);
			}
			m_VolumeData = data;
		}

		[ExecuteInEditMode]
		private void IterateHierarchicalVolumeGrid(int count, ref VolumeData data)
		{
			count++;
			VolumeData[] childData = new VolumeData[8];
			Vector3 half = new Vector3(0.5f, 0.5f, 0.5f);
			for(int i = 0; i < childData.Length; i++)
			{
				Vector3 size = new Vector3(data.bounds.size.x * 0.5f, data.bounds.size.y * 0.5f, data.bounds.size.z * 0.5f);
				float signX = (float)(i + 1) % 2 == 0 ? 1 : -1;  
				float signY = i == 2 || i == 3 || i == 6 || i == 7 ? 1 : -1; 
				float signZ = i == 4 || i == 5 || i == 6 || i == 7 ? 1 : -1;//(float)(i + 1) * 0.5f > 4 ? 1 : -1;
				Vector3 position = data.bounds.center + new Vector3(signX * size.x * 0.5f, signY * size.y * 0.5f, signZ * size.z * 0.5f);
				Bounds bounds = new Bounds(position, size);
				childData[i] = new VolumeData(bounds, null, null);

				if(count < m_VolumeDensity)
				{
					IterateHierarchicalVolumeGrid(count, ref childData[i]);
				}
			}
			data.children = childData;
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
				DrawVolumeDebug();
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

		private void DrawVolumeDebug()
		{
			if(m_VolumeData == null)
				return;

			IterateVolumeDebug(m_VolumeData);
		}

		private void IterateVolumeDebug(VolumeData data)
		{
			if(data.children != null && data.children.Length > 0)
			{
				for(int i = 0; i < data.children.Length; i++)
					IterateVolumeDebug(data.children[i]);
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