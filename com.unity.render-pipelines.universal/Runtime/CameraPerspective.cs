namespace UnityEngine.Rendering.LWRP
{
	public class CameraPerspective
	{
		public readonly Vector3 CameraPosition;
		public readonly Quaternion CameraRotation;
		public readonly int CullingMask;
		public readonly bool IsMainPass;
		public readonly float NearClipPlane;

		public CameraPerspective(Vector3 cameraPosition, Quaternion cameraRotation, int cullingMask, bool isMainPass, float nearClipPlane)
		{
			CameraPosition = cameraPosition;
			CameraRotation = cameraRotation;
			CullingMask = cullingMask;
			IsMainPass = isMainPass;
			NearClipPlane = nearClipPlane;
		}
	}
}