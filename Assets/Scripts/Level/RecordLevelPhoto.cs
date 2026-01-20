using System;
using ImageProcess;
using PlayerController;
using UnityEngine;
using Random = UnityEngine.Random;

public class RecordLevelPhoto : MonoBehaviour
{
    [SerializeField] private GameLevelRecording _gameLevelRecording;
    [SerializeField] private PhotoAlbum.PhotoAlbum _photoAlbum;
    [SerializeField] private CameraFunctionController _cameraFunctionController;
    [SerializeField] private TargetSubjectIdentifier _subject;
    [SerializeField] private string _saveImagePath;
    [SerializeField] private Texture2D _immediateTexture;
    [ContextMenu("Capture Frame")]
    
    private async void CaptureCurrentFrame()
    {
        _cameraFunctionController.SetSubject(_subject);
        var captureResult = await _cameraFunctionController.CapturePhoto();
        Debug.Log("HEHE");
        _immediateTexture = captureResult.PhotoTexture;
        var id = Random.Range(0, 100);
        var exportedTex = await _immediateTexture.SaveImagePNG($"{_saveImagePath}/{id}") as Texture2D;
        _gameLevelRecording = new GameLevelRecording()
        {
            gamePhotoData = captureResult.PhotoData,
            texture = exportedTex
        };
    }
    
    
    [Serializable]
    public struct GameLevelRecording
    {
        [SerializeField] public GamePhotoData gamePhotoData;
        [SerializeField] public Texture2D texture;
    }
}