using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace kTools.Portals
{
	public static class PortalCoreUtil
	{
		// -------------------------------------------------- //
        //                   PRIVATE FIELDS                   //
        // -------------------------------------------------- //

        private static Mesh m_Cube;
        public static Mesh cube
        {
            get
            {
                if(m_Cube == null)
                    m_Cube = CreatePrimitiveMesh(PrimitiveType.Cube);
                return m_Cube;
            }
        }

        // -------------------------------------------------- //
        //                   PUBLIC METHODS                   //
        // -------------------------------------------------- //

        /// <summary>
        /// Safely destroy an Object.
        /// </summary>
        /// <param name="obj">Object to destroy.</param>
        public static void Destroy(UnityEngine.Object obj)
        {
            #if UNITY_EDITOR
                UnityEngine.Object.DestroyImmediate(obj);
            #else
                UnityEngine.Destroy(obj);
            #endif
        }

        /// <summary>
        /// Add a value to the List if it is not already present.
        /// </summary>
        /// <param name="value">Value to add.</param>
        public static void AddIfUnique<T>(this List<T> list, T value)
        {
            if (!list.Contains(value))
				list.Add(value);
        }

        /// <summary>
        /// Try to the Value of a VisibilityData as MeshRenderer[] from a List.
        /// </summary>
        /// <param name="volume">SerializableVolume to use as Key.</param>
		/// <param name="renderers">MeshRenderer[] returns if true.</param>
		public static bool TryGetRemderers(this List<VisbilityData> list, SerializableVolume volume, out MeshRenderer[] renderers)
		{
			for(int i = 0; i < list.Count; i++)
			{
				if(list[i].volume.volumeID == volume.volumeID)
				{
					renderers = list[i].renderers;
					return true;
				}
			}
			renderers = null;
			return false;
		}

		// -------------------------------------------------- //
        //                  INTERNAL METHODS                  //
        // -------------------------------------------------- //

        private static Mesh CreatePrimitiveMesh(PrimitiveType type)
        {
            // Get a Mesh of a Unity primitive type
            var gameObject = GameObject.CreatePrimitive(type);
            var mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
            Destroy(gameObject);
            return mesh;
        }
	}
}
