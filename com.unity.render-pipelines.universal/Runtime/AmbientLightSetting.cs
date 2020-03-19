using System;
using Sirenix.OdinInspector;

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
		private static readonly int FogColorMatrixID = Shader.PropertyToID("_FogColorMatrix");
		private static readonly int WorldToFogMatrixID = Shader.PropertyToID("_WorldToFogMatrix");

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
			#region Static Stuff

			private static readonly int SkyAmbientColorID = Shader.PropertyToID("__SkyAmbientColor");
			private static readonly int EquatorAmbientColorID = Shader.PropertyToID("__EquatorAmbientColor");
			private static readonly int GroundAmbientColorID = Shader.PropertyToID("__GroundAmbientColor");

			#endregion

			#region Serialize Fields

			[BoxGroup("Light")] [SerializeField]
			private Color _skyAmbientColor;
			[BoxGroup("Light")] [SerializeField]
			private Color _equatorAmbientColor;
			[BoxGroup("Light")] [SerializeField]
			private Color _groundAmbientColor;
			[BoxGroup("Fog")] [SerializeField]
			private Color _forwardFogColor = Color.clear;
			[BoxGroup("Fog")] [SerializeField]
			private float _forwardFogDistance = 10;
			[BoxGroup("Fog")] [SerializeField]
			private bool _heightFog;
			[BoxGroup("Fog")] [SerializeField] [ShowIf("_heightFog")]
			private Color _heightFogColor = Color.clear;
			[BoxGroup("Fog")] [SerializeField] [ShowIf("_heightFog")]
			private float _heightFogStartHeight = 0;
			[BoxGroup("Fog")] [SerializeField] [ShowIf("_heightFog")]
			private float _heightFogDistance = 10;

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

			#region Public methods

			public void Apply(Camera camera)
			{
				Shader.SetGlobalColor(SkyAmbientColorID, _skyAmbientColor);
				Shader.SetGlobalColor(EquatorAmbientColorID, _equatorAmbientColor);
				Shader.SetGlobalColor(GroundAmbientColorID, _groundAmbientColor);

				Matrix4x4 colorMatrix = new Matrix4x4();

				colorMatrix[0, 0] = 0;
				colorMatrix[1, 0] = 0;
				colorMatrix[2, 0] = 0;
				colorMatrix[3, 0] = 0;

				colorMatrix[0, 1] = _forwardFogColor.r;
				colorMatrix[1, 1] = _forwardFogColor.g;
				colorMatrix[2, 1] = _forwardFogColor.b;
				colorMatrix[3, 1] = _forwardFogColor.a;

				colorMatrix[0, 2] = _heightFogColor.r;
				colorMatrix[1, 2] = _heightFogColor.g;
				colorMatrix[2, 2] = _heightFogColor.b;
				colorMatrix[3, 2] = _heightFogColor.a;

				Color mixColor = Color.Lerp(_forwardFogColor, _heightFogColor, 0.5f);

				colorMatrix[0, 3] = mixColor.r;
				colorMatrix[1, 3] = mixColor.g;
				colorMatrix[2, 3] = mixColor.b;
				colorMatrix[3, 3] = mixColor.a;

				Shader.SetGlobalMatrix(FogColorMatrixID, colorMatrix);
				Vector3 camForward = camera.transform.forward;
				Vector3 camForwardFlat = new Vector3(camForward.x, 0, camForward.z).normalized;
				Quaternion heightFogRotation = Quaternion.LookRotation(camForwardFlat);
				Vector3 fogPosition = camera.transform.position;
				if (_heightFog)
				{
					fogPosition.y = _heightFogStartHeight;
				}
				else
				{
					fogPosition.y = 1000000;
				}

				Vector3 fogScale = new Vector3(1, _heightFogDistance, _forwardFogDistance);

				Shader.SetGlobalMatrix(WorldToFogMatrixID, Matrix4x4.TRS(fogPosition, heightFogRotation, fogScale).inverse);
			}

			[OnInspectorGUI]
			private void Update()
			{
				_heightFogDistance = Mathf.Min(Mathf.Max(0.001f, _heightFogDistance), 10000);
				_forwardFogDistance = Mathf.Min(Mathf.Max(0.001f, _forwardFogDistance), 10000);
			}
			
			#endregion
		}

		#endregion
	}
}