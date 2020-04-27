namespace UnityEngine.Rendering.LWRP
{
	public interface IRiftPerspectiveController
	{
		bool OverwriteCameraTransformation { get; }
		bool OverwriteCullingMask { get; }
		Vector3 CameraPosition { get; }
		Quaternion CameraRotation { get; }
		int CameraCullingMask { get; }
	}
}