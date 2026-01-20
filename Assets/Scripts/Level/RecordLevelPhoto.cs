using System;
using BaseGameEntity;
using CameraFunction;
using ImageProcess;
using UnityEngine;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
public class RecordLevelPhoto : MonoBehaviour
{
    [SerializeField] private GameLevelRecording _gameLevelRecording;
    [SerializeField] private PhotoAlbum.PhotoAlbum _photoAlbum;
    [SerializeField] private CameraFunctionController _cameraFunctionController;
    [SerializeField] private TargetSubjectIdentifier _subject;
    [SerializeField] private string _saveImagePath;
    
    [ContextMenu("Capture Frame")]
    private async void CaptureCurrentFrame()
    {
        _cameraFunctionController.SetSubject(_subject);
        var captureResult = await _cameraFunctionController.CapturePhoto();
        var immediateTexture = captureResult.PhotoTexture;
        var id = Random.Range(0, 100);
        var exportedTex = await immediateTexture.SaveImagePNG($"{_saveImagePath}/{id}") as Texture2D;
        _gameLevelRecording = new GameLevelRecording()
        {
            gamePhotoData = captureResult.PhotoData,
            texture = exportedTex
        };
    }
    
    
    [Serializable]
    struct GameLevelRecording
    {
        [SerializeField] public GamePhotoData gamePhotoData;
        [SerializeField] public Texture2D texture;
    }
}
#endif