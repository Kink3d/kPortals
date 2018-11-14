using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace kTools.Portals
{
#if UNITY_EDITOR
    public static class PortalDebugUtils
    {
        // -------------------------------------------------- //
        //                    PUBLIC METHODS                  //
        // -------------------------------------------------- //

        public static void DrawRenderers(MeshRenderer[] allRenderers, MeshRenderer[] passedRenderers)
        {
            if (allRenderers == null || passedRenderers == null)
                return;

            for (int i = 0; i < allRenderers.Length; i++)
            {
                bool isPassed = passedRenderers.Contains(allRenderers[i]);
                Transform transform = allRenderers[i].transform;
                Mesh mesh = allRenderers[i].GetComponent<MeshFilter>().sharedMesh;

                Gizmos.color = isPassed ? DebugColorsOld.occludeePass[0] : DebugColorsOld.occludeeFail[0];
                Gizmos.DrawWireMesh(mesh, transform.position, transform.rotation, transform.lossyScale);
                Gizmos.color = isPassed ? DebugColorsOld.occludeePass[1] : DebugColorsOld.occludeeFail[1];
                Gizmos.DrawMesh(mesh, transform.position, transform.rotation, transform.lossyScale);
            }
        }

		public static void DrawOccluders(OccluderData[] occluders)
		{
			if(occluders == null)
				return;
					
			for(int i = 0; i < occluders.Length; i++)
			{
				Transform transform = occluders[i].collider.transform;
				Gizmos.color = DebugColorsOld.occluder[0];
				Gizmos.DrawWireMesh(occluders[i].collider.sharedMesh, transform.position, transform.rotation, transform.lossyScale);
				Gizmos.color = DebugColorsOld.occluder[1];
				Gizmos.DrawMesh(occluders[i].collider.sharedMesh, transform.position, transform.rotation, transform.lossyScale);
			}
		}

        public static void DrawRay(Vector3 position, Vector3 direction, bool pass = true)
        {
            Gizmos.color = pass ? DebugColorsOld.ray[0] : DebugColorsOld.ray[1];
            Gizmos.DrawLine(position, position + Vector3.Scale(direction, new Vector3(10, 10, 10)));
        }

        public static void DrawCone(Vector3 position, Vector3 direction, float angle)
        {
            DebugExtension.DebugCone(position, Vector3.Scale(direction, new Vector3(10, 10, 10)), DebugColorsOld.visualiser[0], angle, 0);
        }

		public static void DrawSphere(Vector3 position, float radius)
		{
			Gizmos.color = DebugColorsOld.ray[0];
			Gizmos.DrawSphere(position, radius);
		}

		public static void DrawHierarchicalVolumeGrid(VolumeData data, VolumeData activeVolume = null)
		{
			if(data == null)
				return;

			DrawHierarchicalVolumeGridRecursive(data, activeVolume);
		}

        // -------------------------------------------------- //
        //                  INTERNAL METHODS                  //
        // -------------------------------------------------- //

        private static void DrawHierarchicalVolumeGridRecursive(VolumeData data, VolumeData activeVolume)
        {
            if (data.children != null && data.children.Length > 0)
            {
                for (int i = 0; i < data.children.Length; i++)
                    DrawHierarchicalVolumeGridRecursive(data.children[i], activeVolume);
            }
            else
            {
				bool isActive = data == activeVolume;
                Gizmos.color = isActive ? DebugColorsOld.volumeActive[0] : DebugColorsOld.volume[0];
                Gizmos.DrawWireCube(data.bounds.center, data.bounds.size);
				Gizmos.color = isActive ? DebugColorsOld.volumeActive[1] : DebugColorsOld.volume[1];
                Gizmos.DrawCube(data.bounds.center, data.bounds.size);
            }
        }
    }

    // -------------------------------------------------- //
    //                   DEBUG RESOURCES                  //
    // -------------------------------------------------- //

    public static class DebugColors
    {
        public static DebugColor volume = new DebugColor(new Vector4(0.43f, 0.81f, 0.96f, 1.0f), new Vector4(0.43f, 0.81f, 0.96f, 0.5f));
        public static DebugColor occluder = new DebugColor(new Vector4(0.99f, 0.82f, 0.64f, 1.0f), new Vector4(0.99f, 0.82f, 0.64f, 0.5f));
    }

    public struct DebugColor
    {
        public DebugColor(Vector4 wire, Vector4 fill)
        {
            m_Wire = wire;
            m_Fill = fill;
        }

        private Vector4 m_Wire;
        public Vector4 wire 
        { 
            get { return m_Wire; }
        }

        private Vector4 m_Fill;
        public Vector4 fill 
        { 
            get { return m_Fill; }
        }
    }

	public static class DebugColorsOld
	{
        public static Color[] ray = new Color[2] { new Color(0f, 1f, 0f, 1f), new Color(0f, 1f, 0f, 0.1f) };
        public static Color[] visualiser = new Color[2] { new Color(0.5f, 0.5f, 0.5f, 1f), new Color(0.5f, 0.5f, 0.5f, 0.5f) };
        public static Color[] occludeePass = new Color[2] { new Color(1f, 1f, 1f, 1f), new Color(1f, 1f, 1f, 0.5f) };
        public static Color[] occludeeFail = new Color[2] { new Color(0f, 0f, 0f, 1f), new Color(0f, 0f, 0f, 0.5f) };
		public static Color[] occluder = new Color[2] { new Color(1f, 0f, 1f, 1f), new Color(1f, 0f, 1f, 0.5f) };
		public static Color[] volume = new Color[2] { new Color(0f, 1f, 1f, 0.25f), new Color(0f, 1f, 1f, 0.1f) };
		public static Color[] volumeActive = new Color[2] { new Color(0f, 1f, 1f, 1f), new Color(0f, 1f, 1f, 0.5f) };
	}
#endif
}
