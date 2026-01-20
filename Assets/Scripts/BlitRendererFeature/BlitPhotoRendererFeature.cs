using System;
using System.Collections.Generic;
using BlitRendererFeature.CommonPayload;
using Cysharp.Threading.Tasks;
using ImageProcess;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using Utils;
using ZeroMessenger;

namespace BlitRendererFeature
{


    public class BlitPhotoRendererFeature : ScriptableRendererFeature
    {
        [SerializeField] private BlitPhotoRendererFeatureSettings settings;
        private BlitPhotoRendererFeaturePass _scriptablePass;
        private IDisposable _disposable;

        /// <inheritdoc/>
        public override void Create()
        {
            //unsubscribe blit request
            _disposable?.Dispose();

            settings.BlitPhotoRequests.Clear();

            //re-subscribe blit request
            _disposable = MessageBroker<BlitPhotoRequest>.Default.Subscribe(x =>
            {
                Debug.Log("Receive Blit Request");
                settings.BlitPhotoRequests.Enqueue(x);
            });

            //pass static setting
            _scriptablePass = new BlitPhotoRendererFeaturePass(settings)
            {
                renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType != CameraType.Game) return;
            if (settings.BlitPhotoRequests.IsNullOrEmpty()) return;
            renderer.EnqueuePass(_scriptablePass);
        }

        [Serializable]
        public class BlitPhotoRendererFeatureSettings
        {
            private Queue<BlitPhotoRequest> _blitPhotoRequests;

            public Queue<BlitPhotoRequest> BlitPhotoRequests
            {
                get
                {
                    _blitPhotoRequests ??= new();
                    return _blitPhotoRequests;
                }
            }
        }

        class BlitPhotoRendererFeaturePass : ScriptableRenderPass
        {
            readonly BlitPhotoRendererFeatureSettings _settings;

            public BlitPhotoRendererFeaturePass(BlitPhotoRendererFeatureSettings settings)
            {
                requiresIntermediateTexture = true;
                _settings = settings;
            }

            private class PassData
            {

            }

            private class ReadbackPassData
            {
                public TextureHandle TextureHandle;
                public BlitPhotoRequest BlitRequest;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                const string passName = "ReadbackPass Blit Photo";

                BlitPhotoRequest blitPhotoRequest = new();
                var hasRequest = !_settings.BlitPhotoRequests.IsNullOrEmpty() &&
                                 _settings.BlitPhotoRequests.TryDequeue(out blitPhotoRequest);

                if (!hasRequest || blitPhotoRequest.IsDefault())
                {
                    Debug.LogError("Empty blit request queue");
                    return;
                }

                var resourceData = frameData.Get<UniversalResourceData>();

                var cameraRenderTex = resourceData.activeColorTexture;
                if (!cameraRenderTex.IsValid())
                {
                    Debug.LogError("Invalid Camera Render Tex");
                    return;
                }

                var destTexAlloc = RTHandles.Alloc(blitPhotoRequest.RenderTexture);
                var destRenderTex = renderGraph.ImportTexture(destTexAlloc);

                renderGraph.AddBlitPass(cameraRenderTex, destRenderTex, Vector2.one, Vector2.zero);

                using (var builder = renderGraph.AddUnsafePass(passName, out ReadbackPassData passData))
                {
                    builder.AllowPassCulling(false);
                    passData.TextureHandle = destRenderTex;
                    passData.BlitRequest = blitPhotoRequest;
                    builder.UseTexture(passData.TextureHandle);

                    builder.SetRenderFunc(static (ReadbackPassData data, UnsafeGraphContext ctx) =>
                    {
                        ctx.cmd.RequestAsyncReadback(data.TextureHandle, (request) =>
                        {
                            var result = request.GetData<Byte>();
                            var tex = ImageExportUtils.ReadBackRenderTextureAutoCreateTexture2D(result);
                            data.BlitRequest.SetCompleteHandle(tex);
                            result.Dispose();
                        });
                    });
                }
            }

        }
    }
}