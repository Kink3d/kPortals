using UnityEngine;
using UnityEditor;
using kTools.Portals;
using kTools.Portals.Tests;

namespace kTools.PortalsEditor.Tests
{
	[CustomEditor(typeof(GetOccluderData))]
	public class GetOccluderDataEditor : Editor
	{
		// -------------------------------------------------- //
        //                   PUBLIC METHODS                   //
        // -------------------------------------------------- //

        public override void OnInspectorGUI()
		{
			BakeEditor.DrawBakeTools(target as IBake);
		}
	}
}
