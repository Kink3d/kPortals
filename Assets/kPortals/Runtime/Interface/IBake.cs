using UnityEngine;

namespace kTools.Portals
{
	// -------------------------------------------------- //
	//                        ENUM                        //
	// -------------------------------------------------- //

	public enum BakeState { Empty, Occluders, Volumes, Visibility, Active }

	public interface IBake
	{
		// -------------------------------------------------- //
        //                      GET / SET                     //
        // -------------------------------------------------- //

		BakeState bakeState { get; }
		float completion { get; }

		// -------------------------------------------------- //
        //                       METHODS                      //
        // -------------------------------------------------- //

#if UNITY_EDITOR
		void OnClickBake();
		void OnClickCancel();
#endif
	}
}
