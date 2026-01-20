using System;
using GameFlow;
using Level;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using ZeroMessenger;

namespace SimpleUIHandle
{
    public class LevelPhotoHost : MonoBehaviour
    {
        [SerializeField] private Image _levelPhoto;
        [SerializeField] private PhotoAlbum.PhotoAlbum _photoAlbum;

        private IDisposable _disposable;
        private void Awake()
        {
            _disposable = MessageBroker<FinishInitialization>.Default.Subscribe(OnReceiveLevelInit);
        }

        private void OnDestroy()
        {
            _disposable?.Dispose();
        }

        private void OnReceiveLevelInit(FinishInitialization payload)
            => InitLevelPhotoUI(payload.LevelConfig);

        public async void InitLevelPhotoUI(LevelConfig levelConfig)
        {
            var levelPhotos = levelConfig.LevelPhoto;

            if (levelPhotos.IsNullOrEmpty())
            {
                Debug.LogError("Level Photo IsNullOrEmpty");
                return;
            }

            var levelPhotoData = levelPhotos[0];

            var spr = await _photoAlbum.LoadPhotoAsSprite(levelPhotoData.PhotoId);
            
            _levelPhoto.sprite = spr;
        }
    }
}