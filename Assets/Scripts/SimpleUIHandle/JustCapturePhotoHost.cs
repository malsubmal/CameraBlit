using System;
using Cysharp.Threading.Tasks;
using GameFlow;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZeroMessenger;

namespace SimpleUIHandle
{
    public class JustCapturePhotoHost : MonoBehaviour
    {
        [SerializeField] private GameObject _hostObject;
        [SerializeField] private RawImage _photoImage;
        [SerializeField] private TMP_Text _tagLine;
        [SerializeField] private int _autoOffCanvas = 3;

        private const string Match = "It's a Match! You Win";
        private const string NoMatch = "It's Not a Match";
        
        private IDisposable _disposable;
        private void Awake()
        {
            _disposable = MessageBroker<PhotoMatchStatus>.Default.Subscribe(OnPhotoCapture);
        }

        private void OnDestroy()
        {
            _disposable?.Dispose();
        }

        private async void OnPhotoCapture(PhotoMatchStatus payload)
        {
            var texture = payload.photoTexture;
            var isMatch = payload.IsMatch();
            
            _photoImage.texture = texture;
            _tagLine.text = isMatch ? Match : NoMatch;
            
            _hostObject.SetActive(true);

            if (!isMatch)
            {
                await UniTask.WaitForSeconds(_autoOffCanvas);
                
                if (_hostObject is not null)
                    _hostObject.SetActive(false);
            }
        }
    }
}