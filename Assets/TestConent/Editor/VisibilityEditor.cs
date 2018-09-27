using System;
using UnityEditor;
using UnityEngine;

namespace SimpleTools.Culling.Tests
{
    [CustomEditor(typeof(Visibility))]
    public class VisibilityEditor : Editor
    {
        SerializedProperty m_RayDensityProp;
        SerializedProperty m_FilterAngleProp;
        SerializedProperty m_DrawRaysProp;

        void OnEnable()
        {
            m_RayDensityProp = serializedObject.FindProperty("m_RayDensity");
            m_FilterAngleProp = serializedObject.FindProperty("m_FilterAngle");
            m_DrawRaysProp = serializedObject.FindProperty("m_DrawRays");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_RayDensityProp, new GUIContent("Ray Density"), true);
            EditorGUILayout.PropertyField(m_FilterAngleProp, new GUIContent("Filter Angle"), true);
            EditorGUILayout.PropertyField(m_DrawRaysProp, new GUIContent("Draw Rays"), true);

            EditorGUILayout.Space();
            Visibility visibility = (Visibility)target;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Generate")))
            {
                visibility.OnClickGenerate();
            }
            if (GUILayout.Button(new GUIContent("Clear")))
            {
                visibility.OnClickCancel();
            }
            EditorGUILayout.EndHorizontal();

            if(visibility.displayDebug)
            {
                string boundsDebug = "Bounds: " + visibility.totalBounds.x + "x" + visibility.totalBounds.x + "x" + visibility.totalBounds.x;
                string raysDebug = "Rays: " + visibility.successfulRays + " out of " + visibility.totalRays + " hit (" + (int)(((float)visibility.successfulRays / (float)visibility.totalRays) * 100) + "%)";
                string renderersDebug = "Renderers: " + visibility.successfulRenderers + " out of " + visibility.totalRenderers + " hit (" + (int)(((float)visibility.successfulRenderers / (float)visibility.totalRenderers) * 100) + "%)";
                EditorGUILayout.LabelField(new GUIContent(boundsDebug));
                EditorGUILayout.LabelField(new GUIContent(raysDebug));
                EditorGUILayout.LabelField(new GUIContent(renderersDebug));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
