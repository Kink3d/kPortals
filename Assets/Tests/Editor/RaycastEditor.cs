using UnityEngine;
using UnityEditor;
using kTools.Portals;
using kTools.Portals.Tests;

namespace kTools.PortalsEditor.Tests
{
	[CustomEditor(typeof(Raycast))]
	public class RaycastEditor : Editor
	{
		// -------------------------------------------------- //
        //                    EDITOR STYLES                   //
        // -------------------------------------------------- //

        internal class Styles
        {
            public static GUIContent propertiesText = EditorGUIUtility.TrTextContent("Properties");
			public static GUIContent rayDensityText = EditorGUIUtility.TrTextContent("Ray Density");
            public static GUIContent bakeToolsText = EditorGUIUtility.TrTextContent("Bake Tools");
        }

        // -------------------------------------------------- //
        //                   PRIVATE FIELDS                   //
        // -------------------------------------------------- //
		
		SerializedProperty m_RayDensityProp;

		// -------------------------------------------------- //
        //                   PUBLIC METHODS                   //
        // -------------------------------------------------- //

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.LabelField(Styles.propertiesText, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_RayDensityProp, Styles.rayDensityText);
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
            m_RayDensityProp = serializedObject.FindProperty("m_RayDensity");
        }
	}
}
