using UnityEngine;
using UnityEditor;
using kTools.Portals;
using kTools.Portals.Tests;

namespace kTools.PortalsEditor.Tests
{
	[CustomEditor(typeof(Visibility))]
	public class VisibilityEditor : Editor
	{
		// -------------------------------------------------- //
        //                    EDITOR STYLES                   //
        // -------------------------------------------------- //

        internal class Styles
        {
            public static GUIContent volumesSettingsText = EditorGUIUtility.TrTextContent("Volume Settings");
			public static GUIContent modeText = EditorGUIUtility.TrTextContent("Mode");
            public static GUIContent subdivisionsText = EditorGUIUtility.TrTextContent("Subdivisions");
            public static GUIContent bakeSettingsText = EditorGUIUtility.TrTextContent("Bake Settings");
            public static GUIContent rayDensityText = EditorGUIUtility.TrTextContent("Ray Count");
            public static GUIContent coneAngleText = EditorGUIUtility.TrTextContent("Cone Angle");
			public static GUIContent bakeToolsText = EditorGUIUtility.TrTextContent("Bake Tools");
        }

        // -------------------------------------------------- //
        //                   PRIVATE FIELDS                   //
        // -------------------------------------------------- //
		
		SerializedProperty m_VolumeModeProp;
        SerializedProperty m_SubdivisionsProp;
        SerializedProperty m_RayCountProp;
        SerializedProperty m_ConeAngleProp;

		// -------------------------------------------------- //
        //                   PUBLIC METHODS                   //
        // -------------------------------------------------- //

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawVolumeSettings();
            DrawBakeSettings();
            
			EditorGUILayout.LabelField(Styles.bakeToolsText, EditorStyles.boldLabel);
            BakeEditor.DrawBakeTools(target as IBake);
            serializedObject.ApplyModifiedProperties();
        }
		
		// -------------------------------------------------- //
        //                   PRIVATE METHODS                  //
        // -------------------------------------------------- //

        private void OnEnable()
        {
            m_VolumeModeProp = serializedObject.FindProperty("m_VolumeMode");
            m_SubdivisionsProp = serializedObject.FindProperty("m_Subdivisions");
            m_RayCountProp = serializedObject.FindProperty("m_RayCount");
            m_ConeAngleProp = serializedObject.FindProperty("m_ConeAngle");
        }

        private void DrawVolumeSettings()
        {
            EditorGUILayout.LabelField(Styles.volumesSettingsText, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_VolumeModeProp, Styles.modeText);
            if(m_VolumeModeProp.enumValueIndex != (int)VolumeMode.Manual)
                EditorGUILayout.PropertyField(m_SubdivisionsProp, Styles.subdivisionsText);
            EditorGUILayout.Space();
        }

        private void DrawBakeSettings()
        {
            EditorGUILayout.LabelField(Styles.bakeSettingsText, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_RayCountProp, Styles.rayDensityText);
            EditorGUILayout.PropertyField(m_ConeAngleProp, Styles.coneAngleText);
            EditorGUILayout.Space();
        }
	}
}
