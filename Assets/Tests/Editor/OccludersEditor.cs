using UnityEditor;
using UnityEngine;

namespace kTools.Portals.Tests
{
    [CustomEditor(typeof(Occluders))]
    public class OccludersEditor : Editor
    {
        Occluders m_ActualTarget;

        void OnEnable()
        {
            m_ActualTarget = (Occluders)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

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
