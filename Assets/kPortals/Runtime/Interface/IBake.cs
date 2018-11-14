using UnityEngine;

namespace kTools.Portals
{
	public enum BakeState { Empty, Occluders, Volumes, Occlusion, Active }

	public interface IBake
	{
		BakeState bakeState { get; }
		float completion { get; }

		void OnClickBake();
		void OnClickCancel();
	}
}
