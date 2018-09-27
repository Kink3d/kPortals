using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimpleTools.Culling
{
	public class SimpleCulling : MonoBehaviour 
	{
		public enum DebugMode { None, Occluders }

		// ----------------------------------------------------------------------------------------------------//
		//                                              RUNTIME                                                //
		// ----------------------------------------------------------------------------------------------------//

		// --------------------------------------------------
		// Public Fields

		public string occluderTag = "Occluder"; // TD - Add option for layer

		[SerializeField]
        DebugMode m_DebugMode = DebugMode.None;

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
			DeleteOccluderProxyGeometry();
            BuildOccluderProxyGeometry();
        }

        [ExecuteInEditMode]
        public void OnClickCancel()
        {
            DeleteOccluderProxyGeometry();
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
		private void DeleteOccluderProxyGeometry()
		{
			m_Occluders = null;
			Transform container = transform.Find(occluderContainerName);
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
            if (m_DebugMode == DebugMode.Occluders)
            {
				DrawOccluderDebug();
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

#endif
		
	}
}