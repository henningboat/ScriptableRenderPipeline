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
			
			
			cameraCullingMask &= ~InsideRiftCullingMask;
			cameraCullingMask |= OutsideRiftCullingMask;
			
			mainCameraPerspective = new CameraPerspective(cameraTransformPosition, cameraTransformRotation, cameraCullingMask, true);

			if (_currentRiftController == null)
			{
				additionalCameraPerspective = null;
			}
			else
			{
				if (_currentRiftController.OverwriteCameraTransformation)
				{
					cameraTransformPosition = _currentRiftController.CameraPosition;
					cameraTransformRotation = _currentRiftController.CameraRotation;
				}

				if (_currentRiftController.OverwriteCullingMask)
				{
					cameraCullingMask = _currentRiftController.CameraCullingMask;
				}
				
				
				cameraCullingMask &= ~OutsideRiftCullingMask;
				cameraCullingMask |= InsideRiftCullingMask;
				additionalCameraPerspective = new CameraPerspective(cameraTransformPosition, cameraTransformRotation, cameraCullingMask, false);
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