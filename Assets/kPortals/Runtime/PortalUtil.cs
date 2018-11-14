using UnityEngine;

namespace kTools.Portals
{
	public static class PortalUtil 
	{
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

		public static Mesh CreatePrimitiveMesh(PrimitiveType type)
        {
            GameObject gameObject = GameObject.CreatePrimitive(type);
            Mesh mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
            GameObject.Destroy(gameObject);
            return mesh;
        }
	}
}
