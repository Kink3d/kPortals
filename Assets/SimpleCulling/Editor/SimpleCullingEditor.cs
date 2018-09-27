using System;
using UnityEditor;
using UnityEngine;

namespace SimpleTools.Culling
{
    [CustomEditor(typeof(SimpleCulling))]
    public class SimpleCullingSystemEditor : Editor
    {
        internal class Styles
        {
			// Groups
			public static GUIContent bakeSettingsText = EditorGUIUtility.TrTextContent("Bake Settings");
            public static GUIContent debugSettingsText = EditorGUIUtility.TrTextContent("Debug Settings");

			// Bake Settings
            public static GUIContent volumeDensityText = EditorGUIUtility.TrTextContent("Volume Density", "...");

            // Debug Settings
            public static GUIContent debugModeText = EditorGUIUtility.TrTextContent("Debug Mode", string.Format("...", Environment.NewLine));

            // Bake Tools
            public static GUIContent generateText = EditorGUIUtility.TrTextContent("Generate", "...");
            public static GUIContent cancelText = EditorGUIUtility.TrTextContent("Cancel", "...");
            public static GUIContent clearText = EditorGUIUtility.TrTextContent("Clear", "...");
        }

		bool m_BakeSettingsFoldout = false;
        bool m_DebugSettingsFoldout = false;

		SerializedProperty m_VolumeDensityProp;
        SerializedProperty m_DebugModeProp;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

			DrawBakeSettings();
            DrawDebugSettings();
            DrawBakeTools();

            serializedObject.ApplyModifiedProperties();
        }

        void OnEnable()
        {
            m_DebugModeProp = serializedObject.FindProperty("m_DebugMode");
			m_VolumeDensityProp = serializedObject.FindProperty("m_VolumeDensity");
        }

		void DrawBakeSettings()
        {
            m_BakeSettingsFoldout = EditorGUILayout.Foldout(m_BakeSettingsFoldout, Styles.bakeSettingsText, true);
            if (m_BakeSettingsFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_VolumeDensityProp, Styles.volumeDensityText, true);
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        }

        void DrawDebugSettings()
        {
            m_DebugSettingsFoldout = EditorGUILayout.Foldout(m_DebugSettingsFoldout, Styles.debugSettingsText, true);
            if (m_DebugSettingsFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_DebugModeProp, Styles.debugModeText);
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        }

        void DrawBakeTools()
        {
            EditorGUILayout.Space();
            SimpleCulling simpleCulling = (SimpleCulling)target;

			// --------------------------------------------------
			// Buttons

            EditorGUILayout.BeginHorizontal();
            GUI.enabled = !simpleCulling.isGenerating;
            if (GUILayout.Button(Styles.generateText))
            {
                simpleCulling.OnClickGenerate();
            }
            GUI.enabled = true;
            if (GUILayout.Button(simpleCulling.isGenerating ? Styles.cancelText : Styles.clearText))
            {
                simpleCulling.OnClickCancel();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}