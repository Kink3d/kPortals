using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimpleTools.Culling.Tests
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(SimpleCulling))]
	public class ConeAngle : MonoBehaviour 
	{

		// ----------------------------------------------------------------------------------------------------//
		//                                           PUBLIC FIELDS                                             //
		// ----------------------------------------------------------------------------------------------------//

		public Transform raySource;
		public float maxAngle;

		// ----------------------------------------------------------------------------------------------------//
		//                                               TEST                                                  //
		// ----------------------------------------------------------------------------------------------------//

		// --------------------------------------------------
		// Runtime Data

		[SerializeField]
		private TestData[] m_TestData;

		[SerializeField]
		private MeshRenderer[] m_StaticRenderers;
		
		[SerializeField]
		private MeshRenderer[] m_PassedRenderers;

		// --------------------------------------------------
		// Test Execution

		private void Update()
		{
			m_StaticRenderers = Utils.GetStaticRenderers();
			m_TestData = new TestData[m_StaticRenderers.Length];
			for(int i = 0; i < m_TestData.Length; i++)
				m_TestData[i] = new TestData(new Vector3[3] { m_StaticRenderers[i].bounds.center, raySource.position, raySource.position + raySource.forward}); 

			m_PassedRenderers = Utils.FilterRenderersByConeAngle(m_StaticRenderers, raySource.position, raySource.forward, maxAngle);
		}

		// --------------------------------------------------
		// Data Structures

		public struct TestData
		{
			public TestData(Vector3[] points)
			{
				this.points = points;
			}

			public Vector3[] points;
		}

		// ----------------------------------------------------------------------------------------------------//
		//                                              DEBUG                                                  //
		// ----------------------------------------------------------------------------------------------------//

		[ExecuteInEditMode]
        private void OnDrawGizmos()
        {
			DrawVectorDebug();
            DrawPassedRenderersDebug();
			DrawConeDebug();
        }

		private void DrawVectorDebug()
		{
			if(m_TestData == null)
				return;

			for(int i = 0; i < m_TestData.Length; i++)
			{
				Gizmos.color = EditorColors.debugBlackWire;
				Gizmos.DrawLine(m_TestData[i].points[1], m_TestData[i].points[2]);
				Gizmos.color = EditorColors.debugBlueWire;
				Gizmos.DrawLine(m_TestData[i].points[1], m_TestData[i].points[0]);
			}
		}

		private void DrawConeDebug()
		{
			Gizmos.color = EditorColors.debugWhiteWire;
			DebugExtension.DebugCone(raySource.position, Vector3.Scale(raySource.forward, new Vector3(10,10,10)), EditorColors.debugBlackWire, maxAngle, 0);
		}

		private void DrawPassedRenderersDebug()
		{
			if(m_PassedRenderers == null)
				return;
					
			for(int i = 0; i < m_StaticRenderers.Length; i++)
			{
				bool isPassed = m_PassedRenderers.Contains(m_StaticRenderers[i]);

				Transform transform = m_StaticRenderers[i].transform;
				Gizmos.color = isPassed ? EditorColors.debugBlueFill : EditorColors.debugBlackFill;
				Mesh mesh = m_StaticRenderers[i].GetComponent<MeshFilter>().sharedMesh;
				Gizmos.DrawMesh(mesh, transform.position, transform.rotation, transform.lossyScale);
				Gizmos.color = isPassed ? EditorColors.debugBlueWire : EditorColors.debugBlackFill;
				Gizmos.DrawWireMesh(mesh, transform.position, transform.rotation, transform.lossyScale);
			}
		}
	}
}
