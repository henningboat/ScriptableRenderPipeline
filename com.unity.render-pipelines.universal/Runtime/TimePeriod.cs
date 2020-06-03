using System;

namespace UnityEngine.Rendering.Universal
{
	[Flags]
	public enum TimePeriod
	{
		T1 = 1 << 0,
		T2 = 1 << 1,
		T3 = 1 << 2,
		T4 = 1 << 3,
		TAll = T1 | T2 | T3 | T4,
	}

	public static class TimePeriodUtility
	{
		#region Static Stuff

		private static int TimePeriodLayerMasksT1 { get; }
		private static int TimePeriodLayerMasksT2 { get; }
		private static int TimePeriodLayerMasksT3 { get; }
		private static int TimePeriodLayerMasksT4 { get; }
		private static int AllTimePeriodLayerMasks { get; }
		public static int TimePeriodLayerT1 { get; }
		public static int TimePeriodLayerT2 { get; }
		public static int TimePeriodLayerT3 { get; }
		public static int TimePeriodLayerT4 { get; }

		public static TimePeriod GetTimePeriod(CameraData cameraData)
		{
			int cameraCullingMask = cameraData.camera.cullingMask;
			return GetTimePeriodFromCullingMask(cameraCullingMask);
		}

		public static TimePeriod GetTimePeriodFromCullingMask(int cameraCullingMask)
		{
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

			if (timePeriodMask == TimePeriodLayerMasksT4)
			{
				return TimePeriod.T4;
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
				case TimePeriod.T4:
					return TimePeriodLayerT4;
				case TimePeriod.TAll:
					return -1;
				default:
					throw new ArgumentOutOfRangeException(nameof(timePeriod), timePeriod, null);
			}
		}

		public static int GetTimePeriodIndex(TimePeriod timePeriod)
		{
			switch (timePeriod)
			{
				case TimePeriod.T1:
					return 0;
					break;
				case TimePeriod.T2:
					return 1;
					break;
				case TimePeriod.T3:
					return 2;
					break;
				case TimePeriod.T4:
					return 3;
			}

			throw new ArgumentOutOfRangeException(nameof(timePeriod), timePeriod, null);
		}

		public static TimePeriod GetTimePeriodFromIndex(int index)
		{
			switch (index)
			{
				case 0:
					return TimePeriod.T1;
				case 1:
					return TimePeriod.T2;
				case 2:
					return TimePeriod.T3;
				case 3:
					return TimePeriod.T4;
			}

			throw new ArgumentOutOfRangeException();
		}

		public static TimePeriod GetTimePeriodFromLayer(int gameObjectLayer)
		{
			if (gameObjectLayer == TimePeriodLayerT1)
				return TimePeriod.T1;
			if (gameObjectLayer == TimePeriodLayerT2)
				return TimePeriod.T2;
			if (gameObjectLayer == TimePeriodLayerT3)
				return TimePeriod.T3;
			if (gameObjectLayer == TimePeriodLayerT4)
				return TimePeriod.T4;
			throw new ArgumentNullException();
		}

		static TimePeriodUtility()
		{
			TimePeriodLayerMasksT1 = LayerMask.GetMask("GraphicsT1");
			TimePeriodLayerMasksT2 = LayerMask.GetMask("GraphicsT2");
			TimePeriodLayerMasksT3 = LayerMask.GetMask("GraphicsT3");
			TimePeriodLayerMasksT4 = LayerMask.GetMask("GraphicsT4");
			AllTimePeriodLayerMasks = TimePeriodLayerMasksT1 | TimePeriodLayerMasksT2 | TimePeriodLayerMasksT3 | TimePeriodLayerMasksT4;

			TimePeriodLayerT1 = LayerMask.NameToLayer("GraphicsT1");
			TimePeriodLayerT2 = LayerMask.NameToLayer("GraphicsT2");
			TimePeriodLayerT3 = LayerMask.NameToLayer("GraphicsT3");
			TimePeriodLayerT4 = LayerMask.NameToLayer("GraphicsT4");
		}

		#endregion
	}
}