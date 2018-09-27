using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimpleTools.Culling.Tests
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(SimpleCulling))]
	public class Occlusion : MonoBehaviour 
	{
		// ----------------------------------------------------------------------------------------------------//
		//                                           PUBLIC FIELDS                                             //
		// ----------------------------------------------------------------------------------------------------//

		public Transform raySource;
		public float maxDisatnce = 10;

		// ----------------------------------------------------------------------------------------------------//
		//                                               TEST                                                  //
		// ----------------------------------------------------------------------------------------------------//

		// --------------------------------------------------
		// Runtime Data

		[SerializeField]
		private TestData[] m_TestData;

		[SerializeField]
		private MeshRenderer[] m_PassedRenderers;

		private SimpleCulling m_SimpleCulling;
		public SimpleCulling simpleCulling
		{
			get
			{
				if(m_SimpleCulling == null)
					m_SimpleCulling = GetComponent<SimpleCulling>();
				return m_SimpleCulling;
			}
		}

		// --------------------------------------------------
		// Test Execution

		private void Update()
		{
			MeshRenderer[] staticRenderers = Utils.GetStaticRenderers();
			m_TestData = new TestData[staticRenderers.Length];

			for(int i = 0; i < m_TestData.Length; i++)
			{
				bool isHit = Utils.CheckAABBIntersection(raySource.position, raySource.forward, staticRenderers[i].bounds);
				if(isHit)
					isHit = Utils.CheckOcclusion(simpleCulling.occluders, staticRenderers[i], raySource.position, raySource.forward);

				Vector3 rayVector = raySource.position + Vector3.Scale(raySource.forward, new Vector3(maxDisatnce, maxDisatnce, maxDisatnce));
				Vector3[] rayData = new Vector3[2] {raySource.position, rayVector};
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
