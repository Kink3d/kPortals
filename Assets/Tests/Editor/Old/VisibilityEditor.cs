using UnityEditor;
using UnityEngine;

namespace kTools.PortalsOld.Tests
{
    [CustomEditor(typeof(Visibility))]
    public class VisibilityEditor : Editor
    {
        Visibility m_ActualTarget;

        SerializedProperty m_RayDensityProp;
        SerializedProperty m_FilterAngleProp;

        void OnEnable()
        {
            m_ActualTarget = (Visibility)target;
            m_RayDensityProp = serializedObject.FindProperty("m_RayDensity");
            m_FilterAngleProp = serializedObject.FindProperty("m_FilterAngle");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_RayDensityProp, new GUIContent("Ray Density"), true);
            EditorGUILayout.PropertyField(m_FilterAngleProp, new GUIContent("Filter Angle"), true);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Generate")))
                m_ActualTarget.OnClickGenerate();
            if (GUILayout.Button(new GUIContent("Clear")))
                m_ActualTarget.OnClickCancel();
            EditorGUILayout.EndHorizontal();

            if(m_ActualTarget.displayDebug)
            {
                var rayHitPercentage = (int)(((float)m_ActualTarget.successfulRays / (float)m_ActualTarget.totalRays) * 100);
                var rendererHitPercentage = (int)(((float)m_ActualTarget.successfulRenderers / (float)m_ActualTarget.totalRenderers) * 100);
                string boundsDebug = string.Format("Bounds: {0}x{0}x{0}", m_ActualTarget.totalBounds.x);
                string raysDebug = string.Format("Rays: {0} out of {1} hit ({2}%)", m_ActualTarget.successfulRays, m_ActualTarget.totalRenderers, rayHitPercentage);
                string renderersDebug = string.Format("Renderers: {0} out of {1} hit ({2}%)", m_ActualTarget.successfulRenderers, m_ActualTarget.totalRays, rendererHitPercentage);
                EditorGUILayout.LabelField(new GUIContent(boundsDebug));
                EditorGUILayout.LabelField(new GUIContent(raysDebug));
                EditorGUILayout.LabelField(new GUIContent(renderersDebug));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
