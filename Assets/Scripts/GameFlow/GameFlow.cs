using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using BaseGameEntity;
using CameraFunction.CommonPayload;
using UnityEngine;
using Level;
using Utils;
using ZeroMessenger;

namespace GameFlow
{
    [Serializable]
    public class GameFlowController : MonoBehaviour
    {
        [SerializeField] private LevelLoader _levelLoader;
        [SerializeField] private PlayerLevelProgressTracker _playerLevelProgressTracker;
        [SerializeField] private SubjectHost _subjectHost;
        [SerializeField] private LevelPhotoQuestController _levelPhotoQuestController;
        private IDisposable _disposable;
        private void Awake()
        {
            _disposable = MessageBroker<RequestLevel>.Default.Subscribe(OnLevelRequested);
        }

        private void OnDestroy()
        {
            _disposable?.Dispose();
        }

        public async void InitializeLevel(SerializableId levelId)
        {
            
            var levelConfig = _levelLoader.GetLevelConfig(levelId);

            if (levelConfig.IsDefault())
            {
                Debug.LogError("Fail to init level");
                return;
            }

            var (isCanceled, subject) = await _levelLoader.LoadLevel(levelConfig, this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow();

            if (isCanceled)
            {
                Debug.LogError("Error Loading Level");
                return;
            }
            
            _subjectHost.HostSubject(subject);
            
            _playerLevelProgressTracker.SetFinishAllLevelPhotoCallback(OnFinishAllLevelPhoto);
            
            _playerLevelProgressTracker.InitializeLevelProgress(levelConfig);
            
            _levelPhotoQuestController.SetCallbackOnMatch(OnPhotoMatch);
            
            _levelPhotoQuestController.InitLevelPhoto(levelConfig.LevelPhoto);
            
            MessageBroker<FinishInitialization>.Default.Publish(new(levelConfig));
        }
        
        private void OnPhotoMatch(GamePhotoData levelPhoto, GamePhotoData capturePhoto)
        {
            _playerLevelProgressTracker.UpdateProgress(levelPhoto, PhotoCaptureProgressStatus.FinishCapture);
        }

        private void OnFinishAllLevelPhoto()
        {
            MessageBroker<EndGameNotice>.Default.Publish(new());
        }

        private void OnLevelRequested(RequestLevel req)
        {
            Debug.Log("req");
            InitializeLevel(new SerializableId());
        }
    }

    [Serializable]
    public struct RequestLevel
    {
        public SerializableId Levelid;
    }

    [Serializable]
    public struct PhotoMatchStatus
    {
        public GamePhotoData capturePhoto;
        public GamePhotoData matchPhoto;

        public RenderTexture photoTexture;

        public bool IsMatch() => !matchPhoto.IsDefault();
    }
    
    [Serializable]
    public struct EndGameNotice {}

    [Serializable]
    public struct FinishInitialization
    {
        private LevelConfig _levelConfig;


        public FinishInitialization(LevelConfig levelConfig)
        {
            _levelConfig = levelConfig;
        }

        public LevelConfig LevelConfig => _levelConfig;
    }


    [Serializable]
    public class LevelPhotoQuestController : IDisposable
    {
        [SerializeField] private float _toleranceValue;
        
        private IDisposable _messageSubscription;
        private GamePhotoData[] _levelPhoto;
        
        private Action<GamePhotoData, GamePhotoData> _onMatchPhoto;

        public void SetCallbackOnMatch(Action<GamePhotoData, GamePhotoData> callback)
        {
            _onMatchPhoto = callback;
        }
        
        ~LevelPhotoQuestController()
        {
            _messageSubscription?.Dispose();
        }
        
        public void InitLevelPhoto(GamePhotoData[] gamePhotoData)
        {
            _messageSubscription?.Dispose();

            _levelPhoto = gamePhotoData;
            
            _messageSubscription = MessageBroker<NoticePhotoCapturePayload>.Default.Subscribe(this, (capture, controller) =>
            {
                var isMatch = false;

                foreach (var photoData in _levelPhoto)
                {
                    isMatch |= capture.ImageMeta.IsSimilarFrame(photoData, _toleranceValue) &&
                               capture.ImageMeta.IsSimilarTargetSubject(photoData);

                    if (!isMatch) continue;
                    
                    OnMatchPhoto(photoData, capture.ImageMeta, capture.ImageTexture);
                }

                if (!isMatch)
                    OnPhotoZeroMatch(capture.ImageMeta, capture.ImageTexture);
            });
        }

        private void OnPhotoZeroMatch(GamePhotoData capturePhoto, RenderTexture renderTexture)
        {
            MessageBroker<PhotoMatchStatus>.Default.Publish(new()
            {
                capturePhoto = capturePhoto,
                photoTexture = renderTexture
            });
        }

        private void OnMatchPhoto(GamePhotoData levelPhoto, GamePhotoData capturePhoto, RenderTexture renderTexture)
        {
            _onMatchPhoto?.Invoke(levelPhoto, capturePhoto);
            MessageBroker<PhotoMatchStatus>.Default.Publish(new()
            {
                matchPhoto = levelPhoto,
                capturePhoto = capturePhoto,
                photoTexture = renderTexture
            });
        }

        public void Dispose()
        {
            _messageSubscription?.Dispose();
        }
    }

    [Serializable]
    public class PlayerLevelProgressTracker
    {
        private Dictionary<GamePhotoData, PhotoCaptureProgressStatus> _levelPhotoCaptureProgress;
        private Action _onFinishCaptureAllLevelPhoto;

        public void SetFinishAllLevelPhotoCallback(Action callback) => _onFinishCaptureAllLevelPhoto = callback;

        public void InitializeLevelProgress(LevelConfig levelConfig)
        {
            _levelPhotoCaptureProgress ??= new();
            foreach (var gamePhotoData in levelConfig.LevelPhoto)
            {
                _levelPhotoCaptureProgress.TryAdd(gamePhotoData, PhotoCaptureProgressStatus.NotCapture);
            }
        }

        public void UpdateProgress(GamePhotoData levelPhoto, PhotoCaptureProgressStatus captureStatus)
        {
            if (_levelPhotoCaptureProgress.IsNullOrEmpty() || !_levelPhotoCaptureProgress.ContainsKey(levelPhoto))
            {
                Debug.LogError("Status Log is Null or Empty OR not contain game photo");
                return;
            }

            _levelPhotoCaptureProgress[levelPhoto] = captureStatus;

            OnPhotoProgressUpdated();
        }

        private void OnPhotoProgressUpdated()
        {
            if (_levelPhotoCaptureProgress.IsNullOrEmpty())
            {
                Debug.LogError("Level Photo Progress is null or empty");
                return;
            }

            var finishCapture = true;
            
            foreach (var photoCaptureProgressStatus in _levelPhotoCaptureProgress.Values)
            {
                finishCapture &= (photoCaptureProgressStatus == PhotoCaptureProgressStatus.FinishCapture);
            }
            
            if (finishCapture)
                _onFinishCaptureAllLevelPhoto?.Invoke();
        }
    }

    [Serializable]
    public struct FinishCaptureAllPhotoNotice {}
    

    [Serializable]
    public enum PhotoCaptureProgressStatus : byte //per photo status
    {
        NotCapture = 0,
        FinishCapture = 1,
    }    
    
    [Serializable]
    public class LevelLoader
    {
        [SerializeField] private LevelCollection _levelCollection;

        public LevelConfig GetLevelConfig(SerializableId levelId)
        {
            if (!_levelCollection)
            {
                Debug.LogError("Null Level Collection SOS");
                return default;
            }
            
            var hasLevel = _levelCollection.TryGetLevel(levelId, out var level);
            return level;
        }

        public async UniTask<LevelInstance> LoadLevel(LevelConfig levelConfig, CancellationToken token)
        {
            var (isCanceled, subject) = await _levelCollection.LoadTargetSubject(levelConfig.TargetSubjectIdentifier, token).SuppressCancellationThrow();

            if (isCanceled)
                return default;

            return subject;
        }

    }
}