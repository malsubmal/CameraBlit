using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utils;
using ZeroMessenger;

namespace BlitRendererFeature.CommonPayload
{

    [Serializable]
    public struct BlitPhotoRequest : IDefaultable<BlitPhotoRequest>
    {
        private RenderTexture _renderTexture;
        private UniTaskCompletionSource<Texture2D> _noticeFinishBlit;

        public RenderTexture RenderTexture => _renderTexture;
        public UniTaskCompletionSource<Texture2D> FinishHandle => _noticeFinishBlit;

        public BlitPhotoRequest(RenderTexture renderTexture)
        {
            _renderTexture = renderTexture;
            _noticeFinishBlit = new UniTaskCompletionSource<Texture2D>();
        }

        public void SubmitRequest()
        {
            MessageBroker<BlitPhotoRequest>.Default.Publish(this);
        }

        public void SetCompleteHandle(Texture2D tex)
        {
            _noticeFinishBlit.TrySetResult(tex);
        }

        public bool IsDefault()
        {
            return _renderTexture is null && _noticeFinishBlit == default;
        }

        public void SetDefault()
        {
            _renderTexture = null;
            _noticeFinishBlit = default;
        }
    }
}