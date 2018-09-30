using System;
using UnityEditor;
using UnityEngine;

namespace SimpleTools.Culling.Tests
{
    [CustomEditor(typeof(Occluders))]
    public class OccludersEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Occluders occluders = (Occluders)target;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Generate")))
            {
                occluders.OnClickGenerate();
            }
            if (GUILayout.Button(new GUIContent("Clear")))
            {
                occluders.OnClickCancel();
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
