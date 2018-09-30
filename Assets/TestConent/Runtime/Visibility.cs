using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using marijnz.EditorCoroutines;

namespace SimpleTools.Culling.Tests
{
    [ExecuteInEditMode]
    public class Visibility : MonoBehaviour
    {

#if UNITY_EDITOR

        // ----------------------------------------------------------------------------------------------------//
        //                                           PUBLIC FIELDS                                             //
        // ----------------------------------------------------------------------------------------------------//

        [SerializeField]
        private int m_RayDensity;

        [SerializeField]
        private int m_FilterAngle;

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
            EditorCoroutines.StartCoroutine(Generate(), this);
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

        private IEnumerator Generate()
		{
            m_StaticRenderers = Utils.GetStaticRenderers();
            Bounds bounds = Utils.GetSceneBounds(m_StaticRenderers);

            yield return EditorCoroutines.StartCoroutine(Utils.BuildHierarchicalVolumeGrid(bounds, 0, value => m_VolumeData = value, this), this);
            yield return EditorCoroutines.StartCoroutine(Utils.BuildVisibilityForVolume(bounds, m_RayDensity, m_StaticRenderers, value => m_PassedRenderers = value, value => m_DebugData = value, this, m_FilterAngle), this);

            totalBounds = bounds.size;
            totalRays = m_DebugData.rays.Length;
            successfulRays = m_DebugData.rays.Where(s => s.pass).ToList().Count;
            totalRenderers = m_StaticRenderers.Length;
            successfulRenderers = m_PassedRenderers.Length;
            displayDebug = true;
            UnityEditor.SceneView.RepaintAll();
        }

        // ----------------------------------------------------------------------------------------------------//
        //                                              DEBUG                                                  //
        // ----------------------------------------------------------------------------------------------------//

        [ExecuteInEditMode]
        private void OnDrawGizmos()
        {
            DebugUtils.DrawHierarchicalVolumeGrid(m_VolumeData);
            DebugUtils.DrawRenderers(m_StaticRenderers, m_PassedRenderers);

            if(m_DebugData.rays == null)
                return;

            foreach(RayDebugData ray in m_DebugData.rays)
            {
                DebugUtils.DrawSphere(ray.position, 0.1f);
                DebugUtils.DrawRay(ray.position, ray.direction, ray.pass);
            }
        }

#endif

    }
}
