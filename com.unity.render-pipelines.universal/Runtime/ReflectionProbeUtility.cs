using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine.Experimental.Rendering;

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

		[MenuItem("AIHIT/UpdateReflectionProbes")]
		private static void ForceUpdateReflectionProbes()
		{
			BuildCubeMap(true, out int i);
		}

		[InitializeOnLoadMethod]
		[RuntimeInitializeOnLoadMethod]
		public static void Init()
		{
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

			ReflectionProbe.reflectionProbeChanged += (reflectionProbe, e) => SetDirty();
		}

		public static void SetDirty()
		{
			_isDirty = true;
		}

		public static void BuildCubeMap(bool forceUpdate, out int totalReflectionProbeCount)
		{
			if (_isDirty)
			{
				forceUpdate = true;
			}
			if (_cubemap != null && !forceUpdate)
			{
				totalReflectionProbeCount = _reflectionProbes.Count;
				return;
			}

			_isDirty = false;
			_reflectionProbes = Object.FindObjectsOfType<ReflectionProbe>().ToList();
			totalReflectionProbeCount = _reflectionProbes.Count;

			float maxVolume = float.MinValue;
			ReflectionProbe mainRefelctionProbe=null;
			foreach (ReflectionProbe reflectionProbe in _reflectionProbes)
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
				_reflectionProbes.Remove(mainRefelctionProbe);
				_reflectionProbes.Insert(0, mainRefelctionProbe);
			}
			
			int reflectionProbeCount = _reflectionProbes.Count;

			bool useDefaultReflectionProbe = false;

			if (reflectionProbeCount == 0)
			{
				reflectionProbeCount = 1;
				useDefaultReflectionProbe = true;
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

			if (!useDefaultReflectionProbe)
			{
				for (int i = 0; i < reflectionProbeCount; i++)
				{
					Texture texture = _reflectionProbes[i].texture;
					if (texture == null || texture.width != 256)
					{
						continue;
					}

					for (int j = 0; j < 6; j++)
					{
						Graphics.CopyTexture(texture, j, _cubemap, i * 6 + j);
					}
				}
			}
			else
			{
				for (int j = 0; j < 6; j++)
				{
					Graphics.CopyTexture(ReflectionProbe.defaultTexture, j, _cubemap, j);
				}
			}

			ReflectionProbeMap[] _probeMap = new ReflectionProbeMap[reflectionProbeCount];

			for (int i = 0; i < _reflectionProbes.Count; i++)
			{
				var probe = _reflectionProbes[i];
				var bounds = probe.bounds;
				_probeMap[i] = new ReflectionProbeMap(bounds.center, bounds.extents, probe.boxProjection ? 1 : 0);
			}

			if (_reflectionProbeComputeBuffer != null)
				_reflectionProbeComputeBuffer.Dispose();

			_reflectionProbeComputeBuffer = new ComputeBuffer(reflectionProbeCount, 28, ComputeBufferType.Structured);
			_reflectionProbeComputeBuffer.SetData(_probeMap);

			Shader.SetGlobalBuffer(ReflectionProbeBoundsBuffer, _reflectionProbeComputeBuffer);
			Shader.SetGlobalTexture(AIHITReflectionProbeArrayID, _cubemap);
		}


		[StructLayout(LayoutKind.Sequential)]
		private struct ReflectionProbeMap
		{
			private Vector3 _center;
			private Vector3 _extends;
			private float _box;

			public ReflectionProbeMap(Vector3 center, Vector3 extends, float box)
			{
				_center = center;
				_extends = extends;
				_box = box;
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