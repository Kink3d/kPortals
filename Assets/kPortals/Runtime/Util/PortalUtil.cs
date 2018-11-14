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
                colliders[i] = CreateOccluderProxy(occluders[i]);
            return colliders;
        }
        
        /// <summary>
        /// Get SerializableVolume data based on Volume mode. Editor only.
        /// </summary>
        /// <param name="mode">Mode for generating Volumes.</param>
        public static SerializableVolume[] GetVolumeData(VolumeMode mode, int autoSubdivisions = 0)
        {
            switch(mode)
            {
                case VolumeMode.Auto:
                    return GetVolumeDataAuto(autoSubdivisions);
                case VolumeMode.Manual:
                    return GetVolumeDataManual();
                case VolumeMode.Hybrid:
                    return GetVolumeDataAuto(autoSubdivisions).Concat(GetVolumeDataManual()).ToArray();
                default:
                    Debug.LogError("Not a valid Volume mode!");
                    return null;
            }
        }

        /// <summary>
        /// Filter SerializableVolume data to return only data with no parent. Editor only.
        /// </summary>
        /// <param name="serializableVolumes">Data to filter.</param>
        public static SerializableVolume[] FilterVolumeDataNoParent(SerializableVolume[] serializableVolumes)
        {
            return serializableVolumes.Where(s => s.parentID == -1).ToArray();
        }

        /// <summary>
        /// Filter SerializableVolume data to return only data with no children. Editor only.
        /// </summary>
        /// <param name="serializableVolumes">Data to filter.</param>
        public static SerializableVolume[] FilterVolumeDataNoChildren(SerializableVolume[] serializableVolumes)
        {
            return serializableVolumes.Where(s => s.childIDs == null).ToArray();
        }
#endif

        // -------------------------------------------------- //
        //                  INTERNAL METHODS                  //
        // -------------------------------------------------- //

        // --------------------------------------------------
        // GEOMETRY

        private static Mesh CreatePrimitiveMesh(PrimitiveType type)
        {
            // Get a Mesh of a Unity primitive type
            var gameObject = GameObject.CreatePrimitive(type);
            var mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
            Destroy(gameObject);
            return mesh;
        }

#if UNITY_EDITOR
        // --------------------------------------------------
        // SCENE DATA
        private static Bounds GetSceneBounds()
        {
            // Encapsulate all static occludees in Bounds
            var occludeeObjects = GetStaticOccludeeRenderers();
            var sceneBounds = new Bounds(Vector3.zero, Vector3.zero);
            for (int i = 0; i < occludeeObjects.Length; i++)
                sceneBounds.Encapsulate(occludeeObjects[i].bounds);

            // Return cubic bounds from max
            float maxSize = Mathf.Max(Mathf.Max(sceneBounds.size.x, sceneBounds.size.y), sceneBounds.size.z);
            sceneBounds.size = new Vector3(maxSize, maxSize, maxSize);
            return sceneBounds;
        }

        private static MeshRenderer[] GetStaticOccluderRenderers()
        {
            // Get all renderers in scene with correct static flags
            var occluderFlag = (int)StaticEditorFlags.OccluderStatic;
            return UnityEngine.Object.FindObjectsOfType<MeshRenderer>().Where(
                s => (occluderFlag & (int)UnityEditor.GameObjectUtility.GetStaticEditorFlags(s.gameObject)) == occluderFlag).ToArray();
        }

        private static MeshRenderer[] GetStaticOccludeeRenderers()
        {
            // Get all renderers in scene with correct static flags
            var occludeeFlag = (int)StaticEditorFlags.OccludeeStatic;
            return UnityEngine.Object.FindObjectsOfType<MeshRenderer>().Where(
                s => (occludeeFlag & (int)UnityEditor.GameObjectUtility.GetStaticEditorFlags(s.gameObject)) == occludeeFlag).ToArray();
        }

        // --------------------------------------------------
        // OCCLUDER DATA

        private static SerializableOccluder[] GetStaticOccluderData()
		{
            // Get all Occluders and Serialize
			var staticOccluderObjects = GetStaticOccluderRenderers();
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

        // --------------------------------------------------
        // OCCLUDER PROXY

        private static MeshCollider CreateOccluderProxy(SerializableOccluder occluder)
        {
            // Initialize proxy object
            var go = new GameObject("OccluderProxy", typeof(MeshCollider));
            var transform = go.transform;
            var collider = go.GetComponent<MeshCollider>();

            // Set occluder data
            transform.position = occluder.positionWS;
            transform.rotation = occluder.rotationWS;
            transform.localScale = occluder.scaleWS;
            collider.sharedMesh = occluder.mesh;
            return collider;
        }

        // --------------------------------------------------
        // VOLUME DATA

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

        private static SerializableVolume[] GetVolumeDataAuto(int volumeSubdivisions)
        {
            // Create initial volume
            var sceneBounds = GetSceneBounds();
            var volumeData = new VolumeData()
            {
                positionWS = sceneBounds.center,
                scaleWS = sceneBounds.size
            };
            
            // Recursively generate hierarchy
            int currentDepth = 0;
            if(currentDepth < volumeSubdivisions)
                GetVolumeDataAutoRecursive(currentDepth + 1, volumeSubdivisions, ref volumeData);

            // Serialize
            return volumeData.Serialize();
        }
        
        private static void GetVolumeDataAutoRecursive(int currentDepth, int volumeSubdivisions, ref VolumeData parentVolume)
        {
            parentVolume.children = new VolumeData[8];
            for(int i = 0; i < parentVolume.children.Length; i++)
			{
                // Get offset values from index 
                // TODO
				// - Math this
				var signX = (float)(i + 1) % 2 == 0 ? 1 : -1;  
				var signY = i == 2 || i == 3 || i == 6 || i == 7 ? 1 : -1;
				var signZ = i == 4 || i == 5 || i == 6 || i == 7 ? 1 : -1; //(float)(i + 1) * 0.5f > 4 ? 1 : -1;

                // Calculate transformations
                var scale = new Vector3(parentVolume.scaleWS.x * 0.5f, parentVolume.scaleWS.y * 0.5f, parentVolume.scaleWS.z * 0.5f);
                var position = parentVolume.positionWS + new Vector3(signX * scale.x * 0.5f, signY * scale.y * 0.5f, signZ * scale.z * 0.5f);

                // Create new child VolumeData
                parentVolume.children[i] = new VolumeData()
                {
                    positionWS = position,
                    scaleWS = scale
                };

                // Continue down hierarchy
                if(currentDepth < volumeSubdivisions)
                    GetVolumeDataAutoRecursive(currentDepth + 1, volumeSubdivisions, ref parentVolume.children[i]);
            }
        }
#endif
	}
}
