using UnityEngine;
using UnityEditor;
using kTools.Portals;

namespace kTools.PortalsEditor
{
	public static class BakeEditor 
	{
		// -------------------------------------------------- //
        //                    EDITOR STYLES                   //
        // -------------------------------------------------- //

        internal class Styles
        {
            public static GUIContent generateText = EditorGUIUtility.TrTextContent("Bake", "Bake new data.");
            public static GUIContent cancelText = EditorGUIUtility.TrTextContent("Cancel", "Cancel current bake operation.");
            public static GUIContent clearText = EditorGUIUtility.TrTextContent("Clear", "Clear active data.");
        }

		// -------------------------------------------------- //
        //                   PUBLIC METHODS                   //
        // -------------------------------------------------- //

		/// <summary>
        /// Draw the Bake tools section for IBake. Editor only.
        /// </summary>
        /// <param name="target">Target Object as IBake interface.</param>
		public static void DrawBakeTools(IBake target, string statistics = null)
        {
			// Draw buttons
            EditorGUILayout.BeginHorizontal();
            bool isGenerating = target.bakeState != BakeState.Active && target.bakeState != BakeState.Empty;
            GUI.enabled = !isGenerating;
            if (GUILayout.Button(Styles.generateText))
                target.OnClickBake();
            GUI.enabled = true;
            if (GUILayout.Button(isGenerating ? Styles.cancelText : Styles.clearText))
                target.OnClickCancel();
            EditorGUILayout.EndHorizontal();

			// Draw progress bar
			DrawProgressBar(target);
            EditorGUILayout.Space();

            // Draw statistics panel
            if(!string.IsNullOrEmpty(statistics))
            {
                EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);
                DrawStatistics(statistics);
            }
        }

		// -------------------------------------------------- //
        //                  INTERNAL METHODS                  //
        // -------------------------------------------------- //

		private static void DrawProgressBar(IBake target)
        {
            Rect r = EditorGUILayout.BeginVertical();
			string label;
            string completion = string.Format(" {0}%", ((int)(target.completion * 100)).ToString(), "%");

            switch(target.bakeState)
            {
                case BakeState.Occluders:
                    label = "Processing Occluders" + completion;
                    break;
                case BakeState.Volumes:
                    label = "Generating Volume Data" + completion;
                    break;
                case BakeState.Visibility:
                    label = "Calculating Visibility" + completion;
                    break;
                case BakeState.Active:
                    label = "Active Data";
                    break;
                default:
                    label = "No Data";
                    break;
            }

            EditorGUI.ProgressBar(r, target.completion, label);
            GUILayout.Space(16);
            EditorGUILayout.EndVertical();
        }

        private static void DrawStatistics(string content)
        {
            Rect rect = GUILayoutUtility.GetRect(new GUIContent(content), GUIStyle.none);
            rect.height += 4;
            EditorGUI.DrawRect(rect, Color.grey);
            EditorGUI.TextField(rect, content);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }
	}
}
