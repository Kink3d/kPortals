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
        private MeshRenderer[] m_StaticRenderers;

        [SerializeField]
        private MeshRenderer[] m_PassedRenderers;

        [SerializeField]
        private VolumeDebugData m_DebugData;

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
            m_StaticRenderers = Utils.GetStaticRenderers();
            Bounds bounds = Utils.GetSceneBounds(m_StaticRenderers);
            m_VolumeData = Utils.BuildHierarchicalVolumeGrid(bounds, 0);
            m_PassedRenderers = Utils.BuildVisibilityForVolume(bounds, m_RayDensity, m_StaticRenderers, out m_DebugData, m_FilterAngle);

            totalBounds = bounds.size;
            totalRays = m_DebugData.rays.Length;
            successfulRays = m_DebugData.rays.Where(s => s.pass).ToList().Count;
            totalRenderers = m_StaticRenderers.Length;
            successfulRenderers = m_PassedRenderers.Length;
            displayDebug = true;

            UnityEditor.SceneView.RepaintAll();
        }

        [ExecuteInEditMode]
        public void OnClickCancel()
        {
            m_VolumeData = null;
            m_StaticRenderers = null;
            m_DebugData.rays = null;
            displayDebug = false;
            UnityEditor.SceneView.RepaintAll();
        }

        // ----------------------------------------------------------------------------------------------------//
        //                                              DEBUG                                                  //
        // ----------------------------------------------------------------------------------------------------//

        [ExecuteInEditMode]
        private void OnDrawGizmos()
        {
            DrawHierarchicalVolumeDebug();
            DebugUtils.DrawRendererDebug(m_StaticRenderers, m_PassedRenderers);
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

        private void DrawRayDebug()
        {
            if (m_DebugData.rays == null || m_DrawRays == false)
                return;

            for (int i = 0; i < m_DebugData.rays.Length; i++)
            {
                bool pass = m_DebugData.rays[i].pass;
                Gizmos.color = pass ? EditorColors.ray[0] : EditorColors.ray[1];
                Gizmos.DrawLine(m_DebugData.rays[i].points[0], m_DebugData.rays[i].points[1]);
            }
        }
    }
}
