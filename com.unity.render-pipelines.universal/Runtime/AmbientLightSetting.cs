using System;

namespace UnityEngine.Rendering.Universal
{
	[Serializable]
	public class AmbientLightSetting
	{
		#region Static Stuff

		private static PerTimePeriodSetting _default = new PerTimePeriodSetting();

		#endregion

		#region Serialize Fields

		[SerializeField] private PerTimePeriodSetting _t1 = new PerTimePeriodSetting();
		[SerializeField] private PerTimePeriodSetting _t2 = new PerTimePeriodSetting();
		[SerializeField] private PerTimePeriodSetting _t3 = new PerTimePeriodSetting();
		[SerializeField] private PerTimePeriodSetting _t4 = new PerTimePeriodSetting();

        #endregion

        #region Constructors

        public AmbientLightSetting(PerTimePeriodSetting t1, PerTimePeriodSetting t2, PerTimePeriodSetting t3, PerTimePeriodSetting t4)
		{
			_t1 = t1;
			_t2 = t2;
			_t3 = t3;
			_t4 = t4;
		}

		#endregion

		#region Public methods

		public PerTimePeriodSetting GetForTimePeriod(TimePeriod timePeriod)
		{
			switch (timePeriod)
			{
				case TimePeriod.T1:
					return _t1;
				case TimePeriod.T2:
					return _t2;
				case TimePeriod.T3:
					return _t3;
				case TimePeriod.T4:
					return _t4;
			}

			return _default;
		}

		#endregion

		#region Nested type: PerTimePeriodSetting

		[Serializable]
		public class PerTimePeriodSetting
		{
			#region Serialize Fields

			[SerializeField] private Color _skyAmbientColor;
			[SerializeField] private Color _equatorAmbientColor;
			[SerializeField] private Color _groundAmbientColor;

			#endregion

			#region Properties

			public Color EquatorAmbientColor => _equatorAmbientColor;
			public Color GroundAmbientColor => _groundAmbientColor;
			public Color SkyAmbientColor => _skyAmbientColor;

			#endregion

			#region Constructors

			public PerTimePeriodSetting()
			{
			}

			public PerTimePeriodSetting(Color skyAmbientColor, Color equatorAmbientColor, Color groundAmbientColor)
			{
				_skyAmbientColor = skyAmbientColor;
				_equatorAmbientColor = equatorAmbientColor;
				_groundAmbientColor = groundAmbientColor;
			}

			#endregion
		}

		#endregion
	}
}