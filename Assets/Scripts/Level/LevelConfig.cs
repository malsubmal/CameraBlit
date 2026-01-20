using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Level;
using PhotoAlbum;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace Level
{
    [Serializable]
    public struct LevelConfig : ILevelConfig, IDefaultable<LevelConfig>
    {
        [SerializeField] private TargetSubjectIdentifier _targetSubjectIdentifier;
        [SerializeField] private GamePhotoData[] _levelPhoto;
        public TargetSubjectIdentifier TargetSubjectIdentifier => _targetSubjectIdentifier;
        public GamePhotoData[] LevelPhoto => _levelPhoto;
        public bool IsDefault()
        {
            return _targetSubjectIdentifier.IsDefault() && _levelPhoto.IsNullOrEmpty();
        }

        public void SetDefault()
        {
            _targetSubjectIdentifier.SetDefault();
            _levelPhoto = null;
        }
        
    }

    [Serializable]
    public struct LevelInstance : ITargetSubject
    {
        private GameObject _subjectInstance;
        
        public LevelInstance(GameObject subject)
        {
            _subjectInstance = subject;
        }

        public Transform SubjectTransform => _subjectInstance != null ? _subjectInstance.transform : null;
        
    }

    public interface ILevelConfig
    {
        public TargetSubjectIdentifier TargetSubjectIdentifier { get; }
        public GamePhotoData[] LevelPhoto { get; }
    }
}

public interface ITargetLoader<T>
where T : ITargetSubject
{
    public UniTask<T> LoadTargetSubject(TargetSubjectIdentifier id, CancellationToken cancellationToken);
}
    
public interface ITargetSubject
{
    public Transform SubjectTransform { get; }
}