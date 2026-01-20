using UnityEngine;
using System;
using Utils;
using Random = UnityEngine.Random;

namespace BaseGameEntity
{
    [Serializable]
    public struct GamePhotoData : IDefaultable<GamePhotoData>
    {
        [SerializeField] private SerializableId _photoId;
        [SerializeField] private CameraState _cameraState;
        [SerializeField] private PhotoDegradationMode _degradationMode;
        [SerializeField] private TargetSubjectIdentifier _targetSubjectIdentifier;

        public GamePhotoData(CameraState camState, TargetSubjectIdentifier targetSubject)
        {
            _photoId = SerializableId.GetId();
            _cameraState = camState;
            _degradationMode = PhotoDegradationMode.None;
            _targetSubjectIdentifier = targetSubject;
        }

        public SerializableId PhotoId => _photoId;
        public CameraState CameraState => _cameraState;
        public PhotoDegradationMode DegradationMode => _degradationMode;
        public TargetSubjectIdentifier TargetSubjectIdentifier => _targetSubjectIdentifier;

        public bool IsDefault()
        {
            return _photoId.IsDefault() && _cameraState.IsDefault() && _targetSubjectIdentifier.IsDefault();
        }

        public void SetDefault()
        {
            _photoId.SetDefault();
            _cameraState.SetDefault();
            _targetSubjectIdentifier.SetDefault();
            _degradationMode = PhotoDegradationMode.None;

        }
    }

    [Serializable]
    public struct TargetSubjectIdentifier : IEquatable<TargetSubjectIdentifier>, IDefaultable<TargetSubjectIdentifier>
    {
        [SerializeField] private SerializableId _id;

        public bool Equals(TargetSubjectIdentifier other)
        {
            return _id.Equals(other._id);
        }

        public override bool Equals(object obj)
        {
            return obj is TargetSubjectIdentifier other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public bool IsDefault()
        {
            return _id.IsDefault();
        }

        public void SetDefault()
        {
            _id.SetDefault();
        }
    }

    [Serializable]
    public struct CameraState : IEquatable<CameraState>, IDefaultable<CameraState>
    {
        [SerializeField] private Vector3 _position;
        [SerializeField] private Quaternion _rotation;
        [SerializeField] private float _zoomValue;

        public CameraState(Vector3 position, Quaternion rotation, float zoom)
        {
            _position = position;
            _rotation = rotation;
            _zoomValue = zoom;
        }

        public float ComputeVariance(CameraState other)
        {
            return (_position - other._position).sqrMagnitude
                   + (_rotation.eulerAngles - other._rotation.eulerAngles).sqrMagnitude
                   + Mathf.Pow((_zoomValue - other._zoomValue),
                       2); //square to give equal importance among  three factors
        }

        public bool Equals(CameraState other)
        {
            return _position.Equals(other._position) && _rotation.Equals(other._rotation) &&
                   _zoomValue.Equals(other._zoomValue);
        }

        public override bool Equals(object obj)
        {
            return obj is CameraState other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_position, _rotation, _zoomValue);
        }

        public bool IsDefault()
        {
            return _position == Vector3.zero && _rotation.eulerAngles == Vector3.zero && _zoomValue == 0;
        }

        public void SetDefault()
        {
            _position = Vector3.zero;
            _rotation.eulerAngles = Vector3.zero;
            _zoomValue = 0;
        }
    }

    [Serializable] //can turn to flag to combine mode
    public enum PhotoDegradationMode
    {
        None = 0,
        Scratched = 1,
        Washed = 2,
        Blurry = 3,
    }

    public static class PhotoComparer
    {
        public static bool IsSimilarTargetSubject(this GamePhotoData thisPhoto, GamePhotoData comparePhoto)
        {
            return comparePhoto.TargetSubjectIdentifier.Equals(thisPhoto.TargetSubjectIdentifier);
        }

        public static bool IsSimilarFrame(this GamePhotoData thisPhoto, GamePhotoData comparePhoto,
            float toleranceRange)
        {
            if (thisPhoto.CameraState.Equals(comparePhoto.CameraState))
                return true;

            return thisPhoto.CameraState.ComputeVariance(comparePhoto.CameraState) < toleranceRange;
        }
    }
}