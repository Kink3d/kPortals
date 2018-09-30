using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimpleTools.Culling
{
	#if UNITY_EDITOR

    public static class DebugUtils
    {
        public static void DrawRenderers(MeshRenderer[] allRenderers, MeshRenderer[] passedRenderers)
        {
            if (allRenderers == null || passedRenderers == null)
                return;

            for (int i = 0; i < allRenderers.Length; i++)
            {
                bool isPassed = passedRenderers.Contains(allRenderers[i]);
                Transform transform = allRenderers[i].transform;
                Mesh mesh = allRenderers[i].GetComponent<MeshFilter>().sharedMesh;

                Gizmos.color = isPassed ? DebugColors.occludeePass[0] : DebugColors.occludeeFail[0];
                Gizmos.DrawWireMesh(mesh, transform.position, transform.rotation, transform.lossyScale);
                Gizmos.color = isPassed ? DebugColors.occludeePass[1] : DebugColors.occludeeFail[1];
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
				Gizmos.color = DebugColors.occluder[0];
				Gizmos.DrawWireMesh(occluders[i].collider.sharedMesh, transform.position, transform.rotation, transform.lossyScale);
				Gizmos.color = DebugColors.occluder[1];
				Gizmos.DrawMesh(occluders[i].collider.sharedMesh, transform.position, transform.rotation, transform.lossyScale);
			}
		}

        public static void DrawRay(Vector3 position, Vector3 direction, bool pass = true)
        {
            Gizmos.color = pass ? DebugColors.ray[0] : DebugColors.ray[1];
            Gizmos.DrawLine(position, position + Vector3.Scale(direction, new Vector3(10, 10, 10)));
        }

        public static void DrawCone(Vector3 position, Vector3 direction, float angle)
        {
            DebugExtension.DebugCone(position, Vector3.Scale(direction, new Vector3(10, 10, 10)), DebugColors.visualiser[0], angle, 0);
        }

		public static void DrawSphere(Vector3 position, float radius)
		{
			Gizmos.color = DebugColors.ray[0];
			Gizmos.DrawSphere(position, radius);
		}

		public static void DrawHierarchicalVolumeGrid(VolumeData data, VolumeData activeVolume = null)
		{
			if(data == null)
				return;

			DrawHierarchicalVolumeGridRecursive(data, activeVolume);
		}

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
                Gizmos.color = isActive ? DebugColors.volumeActive[0] : DebugColors.volume[0];
                Gizmos.DrawWireCube(data.bounds.center, data.bounds.size);
				Gizmos.color = isActive ? DebugColors.volumeActive[1] : DebugColors.volume[1];
                Gizmos.DrawCube(data.bounds.center, data.bounds.size);
            }
        }
    }

	public static class DebugColors
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
