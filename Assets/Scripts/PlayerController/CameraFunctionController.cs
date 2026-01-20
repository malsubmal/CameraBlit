using System;
using Cysharp.Threading.Tasks;
using GameFlow;
using Unity.Cinemachine;
using UnityEngine;
using ZeroMessenger;

namespace PlayerController
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
            
            //send message notice just capture photo
            MessageBroker<NoticePhotoCapture>.Default.Publish(new NoticePhotoCapture(gamePhotoData, _photoRenderTexture));

            return new(texture2D, gamePhotoData);
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

[Serializable]
public struct PhotoCaptureResult
{
    private Texture2D _photoTexture;
    private GamePhotoData _photoData;

    public PhotoCaptureResult(Texture2D photoTexture, GamePhotoData photoData)
    {
        _photoTexture = photoTexture;
        _photoData = photoData;
    }

    public Texture2D PhotoTexture => _photoTexture;
    public GamePhotoData PhotoData => _photoData;
}