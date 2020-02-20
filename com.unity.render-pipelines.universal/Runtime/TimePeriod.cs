using System;

namespace UnityEngine.Rendering.Universal
{
	[Flags]
	public enum TimePeriod
	{
		T1 = 1 << 0,
		T2 = 1 << 1,
		T3 = 1 << 2,
		TAll = T1 | T2 | T3,
	}

	public static class TimePeriodUtility
	{
		#region Static Stuff

		private static int TimePeriodLayerMasksT1 { get; }
		private static int TimePeriodLayerMasksT2 { get; }
		private static int TimePeriodLayerMasksT3 { get; }
		private static int AllTimePeriodLayerMasks { get; }
		public static int TimePeriodLayerT1 { get; }
		public static int TimePeriodLayerT2 { get; }
		public static int TimePeriodLayerT3 { get; }

		public static TimePeriod GetTimePeriod(CameraData cameraData)
		{
			int cameraCullingMask = cameraData.camera.cullingMask;

			int timePeriodMask = cameraCullingMask & AllTimePeriodLayerMasks;

			if (timePeriodMask == TimePeriodLayerMasksT1)
			{
				return TimePeriod.T1;
			}

			if (timePeriodMask == TimePeriodLayerMasksT2)
			{
				return TimePeriod.T2;
			}

			if (timePeriodMask == TimePeriodLayerMasksT3)
			{
				return TimePeriod.T3;
			}

			return TimePeriod.TAll;
		}

		public static int GetTimePeriodLayer(TimePeriod timePeriod)
		{
			switch (timePeriod)
			{
				case TimePeriod.T1:
					return TimePeriodLayerT1;
				case TimePeriod.T2:
					return TimePeriodLayerT2;
				case TimePeriod.T3:
					return TimePeriodLayerT3;
				case TimePeriod.TAll:
					return -1;
				default:
					throw new ArgumentOutOfRangeException(nameof(timePeriod), timePeriod, null);
			}
		}

		static TimePeriodUtility()
		{
			TimePeriodLayerMasksT1 = LayerMask.GetMask("GraphicsT1");
			TimePeriodLayerMasksT2 = LayerMask.GetMask("GraphicsT2");
			TimePeriodLayerMasksT3 = LayerMask.GetMask("GraphicsT3");
			AllTimePeriodLayerMasks = TimePeriodLayerMasksT1 | TimePeriodLayerMasksT2 | TimePeriodLayerMasksT3;

			TimePeriodLayerT1 = LayerMask.NameToLayer("GraphicsT1");
			TimePeriodLayerT2 = LayerMask.NameToLayer("GraphicsT2");
			TimePeriodLayerT3 = LayerMask.NameToLayer("GraphicsT3");
		}

		#endregion
	}
}