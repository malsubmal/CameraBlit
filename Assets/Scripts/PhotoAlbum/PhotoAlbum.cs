using System;
using System.Collections.Generic;
using System.Threading;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using ImageProcess;
using ResourceLoad;
using UnityEngine;
using Utils;

namespace PhotoAlbum
{
    [CreateAssetMenu(fileName = "PhotoAlbumConfig", menuName = "Create PhotoAlbumConfig")]
    public class PhotoAlbum : ScriptableObject
    {
        [SerializeField] private SerializedDictionary<SerializableId, Texture2D> _photoTextureDictionary;
        private MiniResourceDirectory _miniResourceDirectory; //turn to interface, maybe
        
        public UniTask<Texture2D> LoadPhoto(SerializableId photoId)
        {
            if (!_photoTextureDictionary.ContainsKey(photoId))
                return UniTask.FromResult<Texture2D>(null);
            
            return UniTask.FromResult(_photoTextureDictionary[photoId]);
        }

        public async UniTask<Sprite> LoadPhotoAsSprite(SerializableId photoId)
        {
            var texture = await LoadPhoto(photoId);
            
            Rect rect = new Rect(0.0f, 0.0f, texture.width, texture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f); 
            float pixelsPerUnit = 100.0f; 

            // Create the new Sprite from the Texture2D
            return Sprite.Create(texture, rect, pivot, pixelsPerUnit);

        }
        
        public UniTask SavePhoto(SerializableId photoId, Texture2D texture)
        {
            _photoTextureDictionary.TryAdd(photoId, texture);
            return UniTask.CompletedTask;
        }
    }
}