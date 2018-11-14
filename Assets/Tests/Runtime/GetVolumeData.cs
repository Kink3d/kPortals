using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace kTools.Portals.Tests
{
	[ExecuteInEditMode]
    [AddComponentMenu("kTools/Tests/Portals/GetVolumeData")]
	public class GetVolumeData : MonoBehaviour, IBake
	{
		[SerializeField] private VolumeMode m_VolumeMode;
		[SerializeField] private int m_Subdivisions;
		[SerializeField] private SerializableVolume[] m_SerializableVolumes;

		[SerializeField] private BakeState m_BakeState;
		public BakeState bakeState
		{
			get { return m_BakeState; }
		}

		[SerializeField] private float m_Completion;
		public float completion
		{
			get { return m_Completion; }
		}

#if UNITY_EDITOR
		public void OnClickBake()
		{
			m_Completion = 0.0f;
			m_BakeState = BakeState.Volumes;
			m_SerializableVolumes = PortalUtil.GetVolumeData(m_VolumeMode, m_Subdivisions);
			m_BakeState = BakeState.Active;
			m_Completion = 1.0f;
			UnityEditor.SceneView.RepaintAll();
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
		}

		public void OnClickCancel()
		{
			m_SerializableVolumes = null;
			m_BakeState = BakeState.Empty;
			m_Completion = 0.0f;
			UnityEditor.SceneView.RepaintAll();
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
		}

        private void OnDrawGizmos()
        {
			if(m_SerializableVolumes == null)
				return;

			foreach(SerializableVolume volume in PortalUtil.FilterVolumeDataNoChildren(m_SerializableVolumes))
				PortalDebugUtil.DrawCube(volume.positionWS, volume.rotationWS, volume.scaleWS, PortalDebugColors.volume);
        }
#endif
	}
}
