using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace UnityEngine.Rendering.LWRP
{
	public static class AmbientLightUtility
	{
		#region Static Stuff

		private static AmbientLightSetting _currentAmbientColor = GetDefault();
		private static AmbientLightSetting _default = GetDefault();
#if UNITY_EDITOR
		private static AmbientLightSetting.PerTimePeriodSetting _editorNoLightingSetting = new AmbientLightSetting.PerTimePeriodSetting(Color.black, Color.black, Color.black);
#endif
		private static AmbientLightSetting GetDefault()
		{
			return new AmbientLightSetting(
				new AmbientLightSetting.PerTimePeriodSetting(
					new Color(0.374f, 0.443f, 0.434f),
					new Color(0.623f, 0.449f, 0.179f),
					new Color(0.538f, 0.259f, 0.16f)),
				new AmbientLightSetting.PerTimePeriodSetting(
					new Color(0.004f, 0.008f, 0.012f),
					new Color(0.004f, 0.008f, 0.01f),
					new Color(0f, 0f, 0f)),
				new AmbientLightSetting.PerTimePeriodSetting(
					new Color(0.038f, 0.024f, 0.01f, 0f),
					new Color(0.038f, 0.015f, 0.003f, 0f),
					new Color(0f, 0f, 0f, 0f)),
				new AmbientLightSetting.PerTimePeriodSetting(
					new Color(0.004f, 0.008f, 0.012f),
					new Color(0.004f, 0.008f, 0.01f),
					new Color(0f, 0f, 0f)));
		}

		public static void SetAmbientLightSetting(AmbientLightSetting ambientLightSetting)
		{
			_currentAmbientColor = ambientLightSetting;
		}

		public static void RemoveAmbientLightSetting(AmbientLightSetting ambientLightSetting)
		{
			if (_currentAmbientColor == ambientLightSetting)
			{
				_currentAmbientColor = _default;
			}
		}

		public static void ApplyAmbientColor(TimePeriod timePeriod, Camera camera)
		{
			var data = _currentAmbientColor.GetForTimePeriod(timePeriod);

#if UNITY_EDITOR
			if (SceneView.lastActiveSceneView.sceneLighting == false)
			{
				_editorNoLightingSetting.Apply(camera);
			}
			else
#endif
			{
				data.Apply(camera);
			}
		}

		#endregion
	}
}