using System;
using UnityEditor;
using UnityEngine;

namespace kTools.Portals
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
            public static GUIContent volumeModeText = EditorGUIUtility.TrTextContent("Generation Mode", string.Format("Manual: Use user-defined volumes for visibility.{0}Automatic: Generate a grid of volumes for visibility.", Environment.NewLine));
            public static GUIContent manualVolumesText = EditorGUIUtility.TrTextContent("Manual Volumes", "List of user defined volumes used for visibility.");
            public static GUIContent volumeDensityText = EditorGUIUtility.TrTextContent("Volume Density", "Amount of subdivisions to use when generating automatic volume grid.");
            public static GUIContent rayDensityText = EditorGUIUtility.TrTextContent("Ray Density", "Amount of rays to use per cubic unit. Lower values result in faster bake time but less visibility accuracy.");
            public static GUIContent filterAngleText = EditorGUIUtility.TrTextContent("Filter Angle", "Objects outside this angle from a ray is discarded. Lower values result in faster bake time but less visibility accuracy.");
            public static GUIContent occluderTagText = EditorGUIUtility.TrTextContent("Occluder Tag", "Tag used for filtering occluder objects.");

            // Debug Settings
            public static GUIContent debugModeText = EditorGUIUtility.TrTextContent("Debug Mode", string.Format("...", Environment.NewLine));

            // Bake Tools
            public static GUIContent generateText = EditorGUIUtility.TrTextContent("Generate", "Generate new culling data.");
            public static GUIContent cancelText = EditorGUIUtility.TrTextContent("Cancel", "Cancel current bake operation.");
            public static GUIContent clearText = EditorGUIUtility.TrTextContent("Clear", "Clear active culling data.");
        }

		bool m_BakeSettingsFoldout = false;
        bool m_DebugSettingsFoldout = false;

        SerializedProperty m_VolumeModeProp;
        SerializedProperty m_ManualVolumesProp;
		SerializedProperty m_VolumeDensityProp;
        SerializedProperty m_RayDensityProp;
        SerializedProperty m_FilterAngleProp;
        SerializedProperty m_OccluderTagProp;
        SerializedProperty m_DebugModeProp;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

			DrawBakeSettings();
            DrawDebugSettings();
            DrawBakeTools();
            DrawProgressBar();

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            m_VolumeModeProp = serializedObject.FindProperty("m_VolumeMode");
            m_ManualVolumesProp = serializedObject.FindProperty("m_ManualVolumes");
            m_VolumeDensityProp = serializedObject.FindProperty("m_VolumeDensity");
            m_RayDensityProp = serializedObject.FindProperty("m_RayDensity");
            m_FilterAngleProp = serializedObject.FindProperty("m_FilterAngle");
            m_OccluderTagProp = serializedObject.FindProperty("m_OccluderTag");
            m_DebugModeProp = serializedObject.FindProperty("m_DebugMode");
        }

		private void DrawBakeSettings()
        {
            m_BakeSettingsFoldout = EditorGUILayout.Foldout(m_BakeSettingsFoldout, Styles.bakeSettingsText, true);
            if (m_BakeSettingsFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_VolumeModeProp, Styles.volumeModeText);
                if (m_VolumeModeProp.enumValueIndex == (int)SimpleCulling.VolumeMode.Manual)
                    EditorGUILayout.PropertyField(m_ManualVolumesProp, Styles.manualVolumesText, true);
                else if (m_VolumeModeProp.enumValueIndex == (int)SimpleCulling.VolumeMode.Automatic)
                    EditorGUILayout.PropertyField(m_VolumeDensityProp, Styles.volumeDensityText, true);
                EditorGUILayout.PropertyField(m_RayDensityProp, Styles.rayDensityText, true);
                EditorGUILayout.PropertyField(m_FilterAngleProp, Styles.filterAngleText, true);
                EditorGUILayout.PropertyField(m_OccluderTagProp, Styles.occluderTagText, true);
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        }

        private void DrawDebugSettings()
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

        private void DrawBakeTools()
        {
            EditorGUILayout.Space();
            SimpleCulling simpleCulling = (SimpleCulling)target;

			// --------------------------------------------------
			// Buttons

            EditorGUILayout.BeginHorizontal();
            bool isGenerating = simpleCulling.bakeState != SimpleCulling.BakeState.Active && simpleCulling.bakeState != SimpleCulling.BakeState.Empty;
            GUI.enabled = !isGenerating;
            if (GUILayout.Button(Styles.generateText))
            {
                simpleCulling.OnClickGenerate();
            }
            GUI.enabled = true;
            if (GUILayout.Button(isGenerating ? Styles.cancelText : Styles.clearText))
            {
                simpleCulling.OnClickCancel();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawProgressBar()
        {
            SimpleCulling culling = (SimpleCulling)target;

            Rect r = EditorGUILayout.BeginVertical();
			string label;
            string completion = string.Format(" {0}%", ((int)(culling.completion * 100)).ToString(), "%");

            switch(culling.bakeState)
            {
                case SimpleCulling.BakeState.Occluders:
                    label = "Processing Occluders" + completion;
                    break;
                case SimpleCulling.BakeState.Volumes:
                    label = "Generating Volumes" + completion;
                    break;
                case SimpleCulling.BakeState.Occlusion:
                    label = "Baking Occlusion" + completion;
                    break;
                case SimpleCulling.BakeState.Active:
                    label = "Active Culling Data";
                    break;
                default:
                    label = "No Culling Data";
                    break;
            }

            EditorGUI.ProgressBar(r, culling.completion, label);
            GUILayout.Space(16);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
    }
}