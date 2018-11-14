using UnityEngine;

namespace kTools.Portals
{
	// -------------------------------------------------- //
	//                        ENUM                        //
	// -------------------------------------------------- //

	public enum BakeState { Empty, Occluders, Volumes, Occlusion, Active }

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

		void OnClickBake();
		void OnClickCancel();
	}
}
