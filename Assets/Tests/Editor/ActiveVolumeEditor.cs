using System;
using UnityEditor;
using UnityEngine;

namespace SimpleTools.Culling.Tests
{
    [CustomEditor(typeof(ActiveVolume))]
    public class ActiveVolumeEditor : Editor
    {
        SerializedProperty m_TargetProp;
        SerializedProperty m_VolumeDensityProp;

        void OnEnable()
        {
            m_TargetProp = serializedObject.FindProperty("m_Target");
            m_VolumeDensityProp = serializedObject.FindProperty("m_VolumeDensity");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_TargetProp, new GUIContent("Target"), true);
            EditorGUILayout.PropertyField(m_VolumeDensityProp, new GUIContent("Volume Density"), true);

            EditorGUILayout.Space();
            ActiveVolume activeVolume = (ActiveVolume)target;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Generate")))
            {
                activeVolume.OnClickGenerate();
            }
            if (GUILayout.Button(new GUIContent("Clear")))
            {
                activeVolume.OnClickCancel();
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
