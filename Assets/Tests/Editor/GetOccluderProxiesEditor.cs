using UnityEngine;
using UnityEditor;
using kTools.Portals;
using kTools.Portals.Tests;

namespace kTools.PortalsEditor.Tests
{
	[CustomEditor(typeof(GetOccluderProxies))]
	public class GetOccluderProxiesEditor : Editor
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
