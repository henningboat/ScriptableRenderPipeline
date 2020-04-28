namespace UnityEngine.Rendering.LWRP
{
	public static class RiftPerspectiveManager
	{
		private static IRiftPerspectiveController _currentRiftController;

		public static void GetPerspectives(Camera camera, out CameraPerspective mainCameraPerspective, out CameraPerspective additionalCameraPerspective)
		{
			if ((_currentRiftController as Object) == null)
			{
				_currentRiftController = null;
			}

			Transform cameraTransform = camera.transform;
			int cameraCullingMask = camera.cullingMask;

			Vector3 cameraTransformPosition = cameraTransform.position;
			Quaternion cameraTransformRotation = cameraTransform.rotation;
			float nearClipPlane = camera.nearClipPlane;

			cameraCullingMask &= ~InsideRiftCullingMask;
			cameraCullingMask |= OutsideRiftCullingMask;
			
			mainCameraPerspective = new CameraPerspective(cameraTransformPosition, cameraTransformRotation, cameraCullingMask, true, nearClipPlane);

			if (_currentRiftController == null)
			{
				additionalCameraPerspective = null;
			}
			else
			{
				if (_currentRiftController.OverwriteCameraTransformation)
				{
					_currentRiftController.GetTransformation(camera, out cameraTransformPosition, out cameraTransformRotation, out nearClipPlane);
				}

				if (_currentRiftController.OverwriteCullingMask)
				{
					cameraCullingMask = _currentRiftController.GetCullingMask(camera);
				}
				
				
				cameraCullingMask &= ~OutsideRiftCullingMask;
				cameraCullingMask |= InsideRiftCullingMask;
				additionalCameraPerspective = new CameraPerspective(cameraTransformPosition, cameraTransformRotation, cameraCullingMask, false, nearClipPlane);
			}
		}

		public static readonly int OutsideRiftCullingMask = LayerMask.GetMask("OutsideRift");
		public static readonly int InsideRiftCullingMask = LayerMask.GetMask("InsideRift");

		public static void SetRiftPerspective(IRiftPerspectiveController riftPerspectiveController)
		{
			_currentRiftController = riftPerspectiveController;
		}

		public static void RemoveCameraRiftController(IRiftPerspectiveController riftPerspectiveController)
		{
			if (_currentRiftController == riftPerspectiveController)
			{
				_currentRiftController = null;
			}
		}
	}
}