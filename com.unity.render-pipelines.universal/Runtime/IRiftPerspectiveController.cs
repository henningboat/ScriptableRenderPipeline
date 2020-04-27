namespace UnityEngine.Rendering.LWRP
{
	public interface IRiftPerspectiveController
	{
		bool OverwriteCameraTransformation { get; }
		bool OverwriteCullingMask { get; }
		void GetTransformation(Camera camera, out Vector3 position, out Quaternion rotation);
		int GetCullingMask(Camera camera);
	}
}