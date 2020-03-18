using UnityEngine.Rendering.Universal;

namespace UnityEngine.Rendering.LWRP
{
	public static class AmbientLightUtility
	{
		#region Static Stuff

		private static readonly int SkyAmbientColorID = Shader.PropertyToID("__SkyAmbientColor");
		private static readonly int EquatorAmbientColorID = Shader.PropertyToID("__EquatorAmbientColor");
		private static readonly int GroundAmbientColorID = Shader.PropertyToID("__GroundAmbientColor");
		private static AmbientLightSetting _currentAmbientColor = GetDefault();
		private static AmbientLightSetting _default = GetDefault();

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

		public static void ApplyAmbientColor(TimePeriod timePeriod)
		{
			var data = _currentAmbientColor.GetForTimePeriod(timePeriod);
			Shader.SetGlobalColor(SkyAmbientColorID, data.SkyAmbientColor);
			Shader.SetGlobalColor(EquatorAmbientColorID, data.EquatorAmbientColor);
			Shader.SetGlobalColor(GroundAmbientColorID, data.GroundAmbientColor);
		}

		#endregion
	}
}