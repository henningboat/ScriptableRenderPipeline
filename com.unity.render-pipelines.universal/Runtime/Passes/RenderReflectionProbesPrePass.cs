using System.Linq;
using Unity.Collections;

namespace UnityEngine.Rendering.Universal.Internal
{
	public class RenderReflectionProbesPrePass : ScriptableRenderPass
	{
		private string _profilerTag = "Render Reflection Probe pre pass";
		private ComputeBuffer _reflectionProbeAABBComputeBuffer;
		private Mesh _cubeMesh;
		private Material _material;
		private RenderTargetHandle _reflectionProbeMap;
		private RenderTexture _reflectionProbeMapTexture;

		private static readonly int _ReflectionProbeBoundsBufferID = Shader.PropertyToID("_ReflectionProbeBoundsBuffer");
		private ComputeBuffer _computeBuffer;

		public RenderReflectionProbesPrePass(RenderPassEvent evt, Mesh cubeMesh)
		{
			renderPassEvent = evt;
			_reflectionProbeMap.Init("_ScreenSpaceReflectionProbeMap");
			_cubeMesh = cubeMesh;
		}

		private void CreateMaterial()
		{
			_material = new Material(Shader.Find("Hidden/ReflectionProbePrepass"));
		}

		public void Setup()
		{
		}

		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescripor)
		{
			_reflectionProbeMapTexture = RenderTexture.GetTemporary(cameraTextureDescripor.width,
				cameraTextureDescripor.height, 0, RenderTextureFormat.R8);
			ConfigureTarget(new RenderTargetIdentifier(_reflectionProbeMapTexture));
			ConfigureClear(ClearFlag.All, Color.black);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			NativeArray<VisibleReflectionProbe> reflectionProbes = renderingData.cullResults.visibleReflectionProbes;
			var cmd = CommandBufferPool.Get(_profilerTag);

			if (_cubeMesh != null && reflectionProbes.Length > 1)
			{
				if (_material == null)
				{
					CreateMaterial();
				}

				if (_reflectionProbeAABBComputeBuffer != null)
				{
					_reflectionProbeAABBComputeBuffer.Dispose();
				}

				int oneMinusReflectionProbeCount = reflectionProbes.Length - 1;

				_reflectionProbeAABBComputeBuffer = new ComputeBuffer(oneMinusReflectionProbeCount * 2, 3 * 4, ComputeBufferType.Structured);

				Vector3[] reflectionProbeCenterAndSizeData = new Vector3[oneMinusReflectionProbeCount * 2];

				int mainReflectionProbeIndex = 0;
				for (int i = 0; i < reflectionProbes.Length; i++)
				{
					if (reflectionProbes[i].bounds.size.x > 1000)
					{
						mainReflectionProbeIndex = i;
					}
				}


				int index = 0;
				for (int i = 0; i < reflectionProbes.Length; i++)
				{
					if (i != mainReflectionProbeIndex)
					{
						Bounds bounds = reflectionProbes[i].bounds;
						reflectionProbeCenterAndSizeData[index * 2] = bounds.center;
						reflectionProbeCenterAndSizeData[index * 2 + 1] = bounds.size;
						index++;
					}
				}

				_reflectionProbeAABBComputeBuffer.SetData(reflectionProbeCenterAndSizeData);

				//from https://forum.unity.com/threads/reconstructing-world-pos-from-depth-imprecision.228936/
				Matrix4x4 viewMat = renderingData.cameraData.camera.worldToCameraMatrix;
				Matrix4x4 projMat = GL.GetGPUProjectionMatrix( renderingData.cameraData.camera.projectionMatrix, false );
				Matrix4x4 viewProjMat = (projMat * viewMat);          
				cmd.SetGlobalMatrix("_ViewProjInv", viewProjMat.inverse);

				cmd.SetGlobalBuffer(_ReflectionProbeBoundsBufferID, _reflectionProbeAABBComputeBuffer);

				cmd.DrawMeshInstancedProcedural(_cubeMesh, 0, _material, 0, oneMinusReflectionProbeCount);
			}

			cmd.SetGlobalTexture(_reflectionProbeMap.id, _reflectionProbeMapTexture);
			context.ExecuteCommandBuffer(cmd);



			CommandBufferPool.Release(cmd);
		}


		/// <inheritdoc/>
		public override void FrameCleanup(CommandBuffer cmd)
		{
			_reflectionProbeMapTexture.Release();
		}
	}
}
