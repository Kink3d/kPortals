using UnityEngine;
using UnityEditor;
using kTools.Portals;
using kTools.Portals.Tests;

namespace kTools.PortalsEditor.Tests
{
	[CustomEditor(typeof(GetVolumeData))]
	public class GetVolumeDataEditor : Editor
	{
		// -------------------------------------------------- //
        //                    EDITOR STYLES                   //
        // -------------------------------------------------- //

        internal class Styles
        {
            public static GUIContent propertiesText = EditorGUIUtility.TrTextContent("Properties");
			public static GUIContent modeText = EditorGUIUtility.TrTextContent("Mode");
			public static GUIContent bakeToolsText = EditorGUIUtility.TrTextContent("Bake Tools");
        }

        // -------------------------------------------------- //
        //                   PRIVATE FIELDS                   //
        // -------------------------------------------------- //
		
		SerializedProperty m_VolumeModeProp;

		// -------------------------------------------------- //
        //                   PUBLIC METHODS                   //
        // -------------------------------------------------- //

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.LabelField(Styles.propertiesText, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_VolumeModeProp, Styles.modeText);
            EditorGUILayout.Space();
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
        }
	}
}
