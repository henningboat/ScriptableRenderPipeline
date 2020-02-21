using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Rendering.LWRP
{
	public static class AmbientLightUtility
	{
		#region Static Stuff

		private static readonly int SkyAmbientColorID = Shader.PropertyToID("__SkyAmbientColor");
		private static readonly int EquatorAmbientColorID = Shader.PropertyToID("__EquatorAmbientColor");
		private static readonly int GroundAmbientColorID = Shader.PropertyToID("__GroundAmbientColor");
		private static AmbientLightSetting _currentAmbientColor = new AmbientLightSetting();
		private static AmbientLightSetting _default = new AmbientLightSetting();

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