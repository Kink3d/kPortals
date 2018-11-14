using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace kTools.Portals
{
    public enum VolumeMode { Auto, Manual, Hybrid }

    public static class PortalUtil 
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

#if UNITY_EDITOR
        /// <summary>
        /// Get SerializableOccluder data for all active occluders in the scene. Editor only.
        /// </summary>
        public static SerializableOccluder[] GetOccluderData()
        {
            var staticOccluders = GetStaticOccluderData();
            var customOccluders = GetCustomOccluderData();
            return staticOccluders.Concat(customOccluders).ToArray();
        }

        /// <summary>
        /// Build an array of Occluder proxies from serialized Occluder data. Editor only.
        /// </summary>
        /// <param name="occluders">Serialized data to use.</param>
        public static MeshCollider[] GetOccluderProxies(SerializableOccluder[] occluders)
        {
            var colliders = new MeshCollider[occluders.Length];
            for(int i = 0; i < occluders.Length; i++)
            {
                // Initialize proxy object
                var go = new GameObject("OccluderProxy", typeof(MeshCollider));
                var transform = go.transform;
                var collider = go.GetComponent<MeshCollider>();

                // Set occluder data
                transform.position = occluders[i].positionWS;
                transform.rotation = occluders[i].rotationWS;
                transform.localScale = occluders[i].scaleWS;
                collider.sharedMesh = occluders[i].mesh;
                colliders[i] = collider;
            }
            return colliders;
        }
        
        /// <summary>
        /// Get SerializableVolume data based on Volume mode. Editor only.
        /// </summary>
        /// <param name="mode">Mode for generating Volumes.</param>
        public static SerializableVolume[] GetVolumeData(VolumeMode mode)
        {
            switch(mode)
            {
                case VolumeMode.Auto:
                    return GetVolumeDataAuto();
                case VolumeMode.Manual:
                    return GetVolumeDataManual();
                case VolumeMode.Hybrid:
                    return GetVolumeDataAuto().Concat(GetVolumeDataManual()).ToArray();
                default:
                    Debug.LogError("Not a valid Volume mode!");
                    return null;
            }
        }
#endif

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

#if UNITY_EDITOR
        private static SerializableOccluder[] GetStaticOccluderData()
		{
            // Get all renderers in scene with correct static flags
            var occluderFlag = (int)StaticEditorFlags.OccluderStatic;
			var staticOccluderObjects = UnityEngine.Object.FindObjectsOfType<MeshRenderer>().Where(
                s => (occluderFlag & (int)UnityEditor.GameObjectUtility.GetStaticEditorFlags(s.gameObject)) == occluderFlag).ToArray();

            // Serialize
            var customOccluderData = new SerializableOccluder[staticOccluderObjects.Length];
            for(int i = 0; i < customOccluderData.Length; i++)
                customOccluderData[i] = staticOccluderObjects[i].Serialize();
            return customOccluderData;
		}

        private static SerializableOccluder[] GetCustomOccluderData()
        {
            // Get all PortalOccluders in scene
            var customOccluderObjects = UnityEngine.Object.FindObjectsOfType<PortalOccluder>();

            // Serialize
            var customOccluderData = new SerializableOccluder[customOccluderObjects.Length];
            for(int i = 0; i < customOccluderData.Length; i++)
                customOccluderData[i] = customOccluderObjects[i].Serialize();
            return customOccluderData;
        }

        private static SerializableOccluder Serialize(this MeshRenderer renderer)
        {
            // Serialize a SerializableOccluder from a MeshRenderer
            var transform = renderer.transform;
            var filter = renderer.GetComponent<MeshFilter>();
            return new SerializableOccluder()
            {
                positionWS = transform.position,
                rotationWS = transform.rotation,
                scaleWS = transform.lossyScale,
                mesh = filter.sharedMesh
            };
        }

        private static SerializableVolume[] GetVolumeDataManual()
        {
            // Get all VolumeOccluders in scene
            var manualVolumeObjects = UnityEngine.Object.FindObjectsOfType<PortalVolume>();

			// Serialize
            var manualVolumeData = new SerializableVolume[manualVolumeObjects.Length];
            for(int i = 0; i < manualVolumeData.Length; i++)
                manualVolumeData[i] = manualVolumeObjects[i].Serialize();
            return manualVolumeData;
        }

        private static SerializableVolume[] GetVolumeDataAuto()
        {
            return null;
        }
#endif
	}
}
