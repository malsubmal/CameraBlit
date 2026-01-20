using System;
using BlitRendererFeature.CommonPayload;
using Cysharp.Threading.Tasks;
using BaseGameEntity;
using CameraFunction.CommonPayload;
using Unity.Cinemachine;
using UnityEngine;
using ZeroMessenger;
using CameraState = BaseGameEntity.CameraState;

namespace CameraFunction
{
    [Serializable]
    public class CameraFunctionController
    {
        [SerializeField] private CinemachineCamera _camera;
        [SerializeField] private RenderTexture _photoRenderTexture;
        
        private TargetSubjectIdentifier _currentSubjectIdentifier;
        
        
        public void SetSubject(TargetSubjectIdentifier subject)
        {
            _currentSubjectIdentifier = subject;
        }
        
        public async UniTask<PhotoCaptureResult> CapturePhoto()
        {
            var gamePhotoData = new GamePhotoData(CaptureCameraState(), _currentSubjectIdentifier);
            
            //request capture the texture
            var texture2D = await RequestBlitAsync(_photoRenderTexture);
            
            
            var captureResult = new PhotoCaptureResult(texture2D, gamePhotoData, _photoRenderTexture);
            
            //send message notice just capture photo
            MessageBroker<NoticePhotoCapturePayload>.Default.Publish(new NoticePhotoCapturePayload(captureResult));

            return captureResult;
        }

        private async UniTask<Texture2D> RequestBlitAsync(RenderTexture renderTexture)
        {
            var blitRequest = new BlitPhotoRequest(renderTexture);

            blitRequest.SubmitRequest();

            var textureBlit = await blitRequest.FinishHandle.Task;

            return textureBlit;
        }
        
        private CameraState CaptureCameraState()
        {
            var transform = _camera.transform;
            return new CameraState(transform.position, transform.rotation, _camera.Lens.FieldOfView);
        }
    }
}
