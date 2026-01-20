using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Unity.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Utils
{
    public interface IDefaultable<T>
    {
        public bool IsDefault();
        public void SetDefault();
    }

    public static class CollectionUtils
    {
        public static bool IsNullOrEmpty<T>(this T collection)
        where T : ICollection
        {
            return collection is null || collection.Count == 0;
        }
    }

    [Serializable]
    public struct SerializableId : IEquatable<SerializableId>, IDefaultable<SerializableId>
    {
        [SerializeField] private int _id;

        public static SerializableId GetId()
        {
            return new SerializableId()
            {
                _id = Random.Range(1, Int32.MaxValue)
            };
        }

        public bool Equals(SerializableId other)
        {
            return _id == other._id;
        }

        public override bool Equals(object obj)
        {
            return obj is SerializableId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _id;
        }

        public bool IsDefault()
        {
            return _id == 0;
        }

        public void SetDefault()
        {
            _id = 0;
        }
    }
}

namespace ResourceLoad
{
    public interface IAsyncResourceLoad
    {
        public UniTask<T> LoadAsset<T>(string pathName) where T : Object;
    }
    
    public class MiniResourceDirectory : IAsyncResourceLoad
    {
        private Dictionary<string, Object> _resourceLoaded;
        
        public async UniTask<T> LoadAsset<T>(string pathName)
            where T : Object
        {
            var resource = await Resources.LoadAsync<T>(pathName) as T;
            _resourceLoaded ??= new();
            _resourceLoaded.TryAdd(pathName, resource);
            return resource;
        }
    }
}


namespace ImageProcess
{

    [Serializable]
    public struct PhotoSizingConfig
    {
        public readonly int Width;
        public readonly int Height;

        public PhotoSizingConfig(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }


    public static class ImageExportUtils
    {
        private static Texture2D _wrapper;
        public static void ReadBackRenderTexture<T>(Texture2D outputTex, NativeArray<T> dataBuf)
            where T : struct
        {
            outputTex.SetPixelData(dataBuf, 0, 0);
        }
        
        public static UniTask<Object> SaveImagePNG(this Texture2D texture, string name)
        {
            var pathName = $"{name}.png";
            File.WriteAllBytes(pathName, texture.EncodeToPNG());
            return Resources.LoadAsync<Texture2D>(pathName).ToUniTask();
        }
        
        private static Texture2D RetrieveTexture(int width = 1920, int height = 1080)
        {
            return _wrapper ??= new Texture2D(width, height, TextureFormat.RGBA32, false);
        }

        public static Texture2D ReadBackRenderTextureAutoCreateTexture2D<T>(NativeArray<T> dataBuf)
            where T : struct
        {
            var tex2D = RetrieveTexture();
            ReadBackRenderTexture(tex2D, dataBuf);
            return tex2D;
        }
    }
}
