using System.IO;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace ImageProcess
{
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