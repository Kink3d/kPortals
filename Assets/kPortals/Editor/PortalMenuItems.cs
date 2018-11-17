using UnityEngine;
using UnityEditor;
using kTools.Portals;

namespace kTools.PortalsEditor
{
    public class DecalMenuItems
    {
        // Create a new PortalSystem object from Hierarchy window
        [MenuItem("GameObject/kTools/Portals/Portal System", false, 10)]
        static void CreatePortalSystemObject(MenuCommand menuCommand)
        {
            GameObject go = new GameObject();
            go.name = "PortalSystem";
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            go.AddComponent<PortalSystem>();
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

        // Create a new PortalVolume object from Hierarchy window
        [MenuItem("GameObject/kTools/Portals/Portal Volume", false, 10)]
        static void CreatePortalVolumeObject(MenuCommand menuCommand)
        {
            GameObject go = new GameObject();
            go.name = "PortalVolume";
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            go.AddComponent<PortalVolume>();
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

		// Create a new PortalOccluder object from Hierarchy window
        [MenuItem("GameObject/kTools/Portals/Portal Occluder", false, 10)]
        static void CreatePortalOccluderObject(MenuCommand menuCommand)
        {
            GameObject go = new GameObject();
            go.name = "PortalOccluder";
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            go.AddComponent<PortalOccluder>();
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
    }
}
