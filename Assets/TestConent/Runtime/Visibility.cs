using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimpleTools.Culling.Tests
{
    [ExecuteInEditMode]
    public class Visibility : MonoBehaviour
    {
        // ----------------------------------------------------------------------------------------------------//
        //                                           PUBLIC FIELDS                                             //
        // ----------------------------------------------------------------------------------------------------//

        [SerializeField]
        private int m_RayDensity;

        [SerializeField]
        private int m_FilterAngle;

        [SerializeField]
        private bool m_DrawRays;

        // ----------------------------------------------------------------------------------------------------//
        //                                               TEST                                                  //
        // ----------------------------------------------------------------------------------------------------//

        // --------------------------------------------------
        // Runtime Data

        [SerializeField]
        private VolumeData m_VolumeData;

        [SerializeField]
        private TestData[] m_TestData;

        [SerializeField]
        private MeshRenderer[] m_StaticRenderers;

        [SerializeField]
        private MeshRenderer[] m_PassedRenderers;

        // --------------------------------------------------
        // Debug Data

        public bool displayDebug;
        public Vector3 totalBounds;
        public int totalRays;
        public int successfulRays;
        public int totalRenderers;
        public int successfulRenderers;

        // --------------------------------------------------
        // Test Execution

        [ExecuteInEditMode]
        public void OnClickGenerate()
        {
            m_VolumeData = null;
            m_StaticRenderers = Utils.GetStaticRenderers();
            Bounds bounds = Utils.GetSceneBounds(m_StaticRenderers);
            m_VolumeData = Utils.BuildHierarchicalVolumeGrid(bounds, 0);

            List<MeshRenderer> passedRenderers = new List<MeshRenderer>();
            int rayCount = (int)(m_RayDensity * bounds.size.x * bounds.size.y * bounds.size.z);
            m_TestData = new TestData[rayCount];

            successfulRays = 0;

            for (int r = 0; r < rayCount; r++)
            {
                Vector3 rayPosition = Utils.RandomPointWithinBounds(m_VolumeData.bounds);
                Vector3 rayDirection = Utils.RandomSphericalDistributionVector();

                bool rayHasHit = false;

                MeshRenderer[] filteredRenderers = Utils.FilterRenderersByConeAngle(m_StaticRenderers, rayPosition, rayDirection, m_FilterAngle);
                for (int i = 0; i < filteredRenderers.Length; i++)
                {
                    bool isHit = Utils.CheckAABBIntersection(rayPosition, rayDirection, filteredRenderers[i].bounds);
                    if(isHit)
                    {
                        rayHasHit = true;
                        successfulRays++;
                        isHit = !passedRenderers.Contains(filteredRenderers[i]);
                        if(isHit)
                            passedRenderers.Add(filteredRenderers[i]);
                    }
                }

                totalBounds = bounds.size;
                totalRays = rayCount;
                totalRenderers = m_StaticRenderers.Length;
                successfulRenderers = passedRenderers.Count;

                Vector3 rayEnd = Vector3.Scale(rayDirection, new Vector3(10, 10, 10));
                m_TestData[r] = new TestData(new Vector3[2] { rayPosition, rayEnd }, rayHasHit);
            }

            m_PassedRenderers = passedRenderers.ToArray();
            displayDebug = true;

            UnityEditor.SceneView.RepaintAll();
        }

        [ExecuteInEditMode]
        public void OnClickCancel()
        {
            m_VolumeData = null;
            m_StaticRenderers = null;
            m_TestData = null;
            displayDebug = false;
            UnityEditor.SceneView.RepaintAll();
        }

        // --------------------------------------------------
        // Data Structures

        [Serializable]
        public struct TestData
        {
            public TestData(Vector3[] ray, bool pass)
            {
                this.ray = ray;
                this.pass = pass;
            }

            public Vector3[] ray;
            public bool pass;
        }

        // ----------------------------------------------------------------------------------------------------//
        //                                              DEBUG                                                  //
        // ----------------------------------------------------------------------------------------------------//

        [ExecuteInEditMode]
        private void OnDrawGizmos()
        {
            DrawHierarchicalVolumeDebug();
            DrawRendererDebug();
            DrawRayDebug();
        }

        private void DrawHierarchicalVolumeDebug()
        {
            if (m_VolumeData == null)
                return;

            Gizmos.color = EditorColors.volumeFill;
            Gizmos.DrawCube(m_VolumeData.bounds.center, m_VolumeData.bounds.size);
            Gizmos.color = EditorColors.volumeWire;
            Gizmos.DrawWireCube(m_VolumeData.bounds.center, m_VolumeData.bounds.size);
        }

        private void DrawRendererDebug()
        {
            if (m_StaticRenderers == null)
                return;

            for (int i = 0; i < m_StaticRenderers.Length; i++)
            {
                bool pass = m_PassedRenderers.Contains(m_StaticRenderers[i]);
                Transform transform = m_StaticRenderers[i].transform;
                Mesh mesh = m_StaticRenderers[i].GetComponent<MeshFilter>().sharedMesh;
                
                Gizmos.color = pass ? EditorColors.occludeePass[0] : EditorColors.occludeeFail[0];
                Gizmos.DrawWireMesh(mesh, transform.position, transform.rotation, transform.lossyScale);

                Gizmos.color = pass ? EditorColors.occludeePass[1] : EditorColors.occludeeFail[1];
                Gizmos.DrawMesh(mesh, transform.position, transform.rotation, transform.lossyScale);
            }
        }

        private void DrawRayDebug()
        {
            if (m_TestData == null || m_DrawRays == false)
                return;

            for (int i = 0; i < m_TestData.Length; i++)
            {
                bool pass = m_TestData[i].pass;
                Gizmos.color = pass ? EditorColors.occludeePass[0] : EditorColors.occludeeFail[1];
                Gizmos.DrawLine(m_TestData[i].ray[0], m_TestData[i].ray[1]);
            }
        }
    }
}
