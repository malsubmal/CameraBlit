using System;
using BaseGameEntity;
using UnityEngine;

namespace CameraFunction.CommonPayload
{
    [Serializable]
    public struct NoticePhotoCapturePayload
    {
        private PhotoCaptureResult _photoCaptureResult;

        public NoticePhotoCapturePayload(PhotoCaptureResult captureResult)
        {
            _photoCaptureResult = captureResult;
        }

        public GamePhotoData ImageMeta => _photoCaptureResult.PhotoData;
        public RenderTexture ImageTexture => _photoCaptureResult.PhotoRawTexture;
    }
    
    [Serializable]
    public struct PhotoCaptureResult
    {
        private RenderTexture _renderTexture;
        private Texture2D _texture2D;
        private GamePhotoData _photoData;

        public PhotoCaptureResult(Texture2D photoTexture, GamePhotoData photoData, RenderTexture renderTex)
        {
            _renderTexture = renderTex;
            _texture2D = photoTexture;
            _photoData = photoData;
        }

        
        public RenderTexture PhotoRawTexture => _renderTexture;
        public Texture2D PhotoTexture => _texture2D;
        public GamePhotoData PhotoData => _photoData;
    }
}