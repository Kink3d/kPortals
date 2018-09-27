using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleTools.Culling.Tests
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(SimpleCulling))]
	public class AABBIntersection : MonoBehaviour 
	{
		// ----------------------------------------------------------------------------------------------------//
		//                                           PUBLIC FIELDS                                             //
		// ----------------------------------------------------------------------------------------------------//

		public Transform raySource;

		// ----------------------------------------------------------------------------------------------------//
		//                                               TEST                                                  //
		// ----------------------------------------------------------------------------------------------------//

		// --------------------------------------------------
		// Runtime Data

		private TestData[] m_TestData;
		private MeshRenderer[] m_PassedRenderers;

		// --------------------------------------------------
		// Test Execution

		private void Update()
		{
			MeshRenderer[] staticRenderers = Utils.GetStaticRenderers();
			m_TestData = new TestData[staticRenderers.Length];

			for(int i = 0; i < m_TestData.Length; i++)
			{
				bool isHit = Utils.CheckAABBIntersection(raySource.position, raySource.forward, staticRenderers[i].bounds);
				Vector3[] rayData = new Vector3[2] {raySource.position, raySource.position + Vector3.Scale(raySource.forward, new Vector3(10, 10, 10))};
				m_TestData[i] = new TestData(rayData, staticRenderers[i], isHit);
			}
		}

		// --------------------------------------------------
		// Data Structures

		public struct TestData
		{
			public TestData(Vector3[] ray, MeshRenderer renderer, bool pass)
			{
				this.ray = ray;
				this.renderer = renderer;
				this.pass = pass;
			}

			public Vector3[] ray;
			public MeshRenderer renderer;
			public bool pass;
		}

		// ----------------------------------------------------------------------------------------------------//
		//                                              DEBUG                                                  //
		// ----------------------------------------------------------------------------------------------------//

		[ExecuteInEditMode]
        private void OnDrawGizmos()
        {
			DrawIntersectionDebug();
        }

		private void DrawIntersectionDebug()
		{
			if(m_TestData == null)
				return;

			for(int i = 0; i < m_TestData.Length; i++)
			{
				Gizmos.color = EditorColors.debugBlackWire;
				Gizmos.DrawLine(m_TestData[i].ray[0], m_TestData[i].ray[1]);

				Transform transform = m_TestData[i].renderer.transform;
				Gizmos.color = m_TestData[i].pass ? EditorColors.debugBlueFill : EditorColors.debugBlackFill;
				Mesh mesh = m_TestData[i].renderer.GetComponent<MeshFilter>().sharedMesh;
				Gizmos.DrawMesh(mesh, transform.position, transform.rotation, transform.lossyScale);
				Gizmos.color = m_TestData[i].pass ? EditorColors.debugBlueWire : EditorColors.debugBlackWire;
				Gizmos.DrawWireMesh(mesh, transform.position, transform.rotation, transform.lossyScale);
			}
		}
	}
}
