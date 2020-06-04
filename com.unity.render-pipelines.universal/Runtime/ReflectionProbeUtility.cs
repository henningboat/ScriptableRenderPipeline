using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Rendering.LWRP
{
	public static class ReflectionProbeUtility
	{
		#region Static Stuff

		private static CubemapArray _cubemap;
		private static readonly int AIHITReflectionProbeArrayID = Shader.PropertyToID("aihit_REFLECTION_PROBE_ARRAY");
		private static List<ReflectionProbe> _reflectionProbes;
		private static ComputeBuffer _reflectionProbeComputeBuffer;
		private static readonly int ReflectionProbeBoundsBuffer = Shader.PropertyToID("_ReflectionProbeBoundsBuffer");
		private static bool _isDirty;
		private static bool _lastSelectionWasReflectionProbe;

		private static List<List<int>> _perTimePeriodRefelctionProbeIndices;
		private static List<int> _fallbackIndices = new List<int>() { 0 };

#if UNITY_EDITOR
		[InitializeOnLoadMethod]
#endif
		[RuntimeInitializeOnLoadMethod]
		public static void Init()
		{
#if UNITY_EDITOR
			Selection.selectionChanged += () =>
			                              {
				                              if (_lastSelectionWasReflectionProbe)
				                              {
					                              SetDirty();
				                              }

				                              _lastSelectionWasReflectionProbe = Selection.activeObject != null && Selection.activeGameObject.GetComponent<ReflectionProbe>() != null;
			                              };

			SceneView.duringSceneGui += view =>
			                            {
				                            if (_lastSelectionWasReflectionProbe)
				                            {
					                            if (Event.current.type == EventType.MouseDrag)
					                            {
						                            SetDirty();
					                            }
				                            }
			                            };
#endif

			ReflectionProbe.reflectionProbeChanged += (reflectionProbe, e) => SetDirty();
		}

		public static void SetDirty()
		{
			_isDirty = true;
		}

		public static void BuildCubeMap(bool forceUpdate,TimePeriod currentTimePeriod, out List<int> reflectionProbeIndices)
		{
			try
			{
				if (currentTimePeriod == TimePeriod.TAll)
				{
					reflectionProbeIndices = _fallbackIndices;
				}

				int currentTimePeriodIndex = TimePeriodUtility.GetTimePeriodIndex(currentTimePeriod);

				if (_isDirty)
				{
					forceUpdate = true;
				}

				if (_cubemap != null && !forceUpdate)
				{
					reflectionProbeIndices = _perTimePeriodRefelctionProbeIndices[currentTimePeriodIndex];
					return;
				}

				_isDirty = false;
				_reflectionProbes = Object.FindObjectsOfType<ReflectionProbe>().ToList();




				_perTimePeriodRefelctionProbeIndices = new List<List<int>>();

				//we always add the default reflection probe just in case
				int reflectionProbeCount = _reflectionProbes.Count + 1;

				for (int i = 0; i < 4; i++)
				{
					List<int> _indicesForTimePeriod = new List<int>();

					TimePeriod timePeriod = TimePeriodUtility.GetTimePeriodFromIndex(i);
					int timePeriodLayer = TimePeriodUtility.GetTimePeriodLayer(timePeriod);


					List<ReflectionProbe> reflectionProbesForTimePeriod = _reflectionProbes.Where(probe => probe.gameObject.layer == timePeriodLayer).ToList();

					float maxVolume = float.MinValue;
					ReflectionProbe mainRefelctionProbe = null;
					foreach (ReflectionProbe reflectionProbe in reflectionProbesForTimePeriod)
					{
						Vector3 boundsSize = reflectionProbe.bounds.size;
						float volume = boundsSize.x * boundsSize.y * boundsSize.z;
						if (volume > maxVolume)
						{
							mainRefelctionProbe = reflectionProbe;
							maxVolume = volume;
						}
					}

					if (mainRefelctionProbe != null)
					{
						reflectionProbesForTimePeriod.Remove(mainRefelctionProbe);
						reflectionProbesForTimePeriod.Insert(0, mainRefelctionProbe);
					}

					foreach (ReflectionProbe probe in reflectionProbesForTimePeriod)
					{
						_indicesForTimePeriod.Add(_reflectionProbes.IndexOf(probe) + 1);
					}

					if (_indicesForTimePeriod.Count == 0)
					{
						_indicesForTimePeriod.Add(0);
					}

					_perTimePeriodRefelctionProbeIndices.Add(_indicesForTimePeriod);
				}


				if (_cubemap != null)
				{
					if (_cubemap.cubemapCount != reflectionProbeCount)
					{
						Object.DestroyImmediate(_cubemap);
						CreateCubeMapArray(reflectionProbeCount);
					}
				}
				else
				{
					CreateCubeMapArray(reflectionProbeCount);
				}

				for (int i = 0; i < _reflectionProbes.Count; i++)
				{
					Texture texture = _reflectionProbes[i].texture;
					if (texture == null || texture.width != 256)
					{
						continue;
					}

					for (int j = 0; j < 6; j++)
					{
						Graphics.CopyTexture(texture, j, _cubemap, 6 + i * 6 + j);
					}
				}


				ReflectionProbeMap[] _probeMap = new ReflectionProbeMap[reflectionProbeCount];
				_probeMap[0] = new ReflectionProbeMap();
				for (int i = 0; i < _reflectionProbes.Count; i++)
				{
					var probe = _reflectionProbes[i];
					var bounds = probe.bounds;

					TimePeriod timePeriod = TimePeriodUtility.GetTimePeriodFromLayer(probe.gameObject.layer);
					int timePeriodIndex = TimePeriodUtility.GetTimePeriodIndex(timePeriod);

					_probeMap[i + 1] = new ReflectionProbeMap(bounds.center, bounds.extents, probe.boxProjection ? 1 : 0, timePeriodIndex);
				}

				if (_reflectionProbeComputeBuffer != null)
					_reflectionProbeComputeBuffer.Dispose();

				_reflectionProbeComputeBuffer = new ComputeBuffer(reflectionProbeCount, 32, ComputeBufferType.Structured);
				_reflectionProbeComputeBuffer.SetData(_probeMap);

				Shader.SetGlobalBuffer(ReflectionProbeBoundsBuffer, _reflectionProbeComputeBuffer);
				Shader.SetGlobalTexture(AIHITReflectionProbeArrayID, _cubemap);


				reflectionProbeIndices = _perTimePeriodRefelctionProbeIndices[currentTimePeriodIndex];
			}
			catch(Exception e)
			{
				reflectionProbeIndices = _fallbackIndices;
			}
		}


		[StructLayout(LayoutKind.Sequential)]
		private struct ReflectionProbeMap
		{
			private Vector3 _center;
			private Vector3 _extends;
			private float _box;
			private float _timePeriodIndex;

			public ReflectionProbeMap(Vector3 center, Vector3 extends, float box, float timePeriodIndex)
			{
				_center = center;
				_extends = extends;
				_box = box;
				_timePeriodIndex = timePeriodIndex;
			}
		}

		private static void CreateCubeMapArray(int reflectionProbesLength)
		{
			_cubemap = new CubemapArray(256, reflectionProbesLength, DefaultFormat.HDR, TextureCreationFlags.MipChain);
			_cubemap.hideFlags = HideFlags.DontSave;
		}

		#endregion
	}
}
