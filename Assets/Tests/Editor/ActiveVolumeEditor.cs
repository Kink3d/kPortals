using UnityEditor;
using UnityEngine;

namespace kTools.Portals.Tests
{
    [CustomEditor(typeof(ActiveVolume))]
    public class ActiveVolumeEditor : Editor
    {
        ActiveVolume m_ActualTarget;

        SerializedProperty m_TargetProp;
        SerializedProperty m_VolumeDensityProp;

        void OnEnable()
        {
            m_ActualTarget = (ActiveVolume)target;
            m_TargetProp = serializedObject.FindProperty("m_Target");
            m_VolumeDensityProp = serializedObject.FindProperty("m_VolumeDensity");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_TargetProp, new GUIContent("Target"), true);
            EditorGUILayout.PropertyField(m_VolumeDensityProp, new GUIContent("Volume Density"), true);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Generate")))
                m_ActualTarget.OnClickGenerate();
            if (GUILayout.Button(new GUIContent("Clear")))
                m_ActualTarget.OnClickCancel();
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
