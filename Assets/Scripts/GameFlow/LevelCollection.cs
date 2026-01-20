using System.Collections.Generic;
using System.Threading;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using BaseGameEntity;
using Level;
using UnityEngine;
using Utils;

namespace GameFlow
{
    [CreateAssetMenu(menuName = "Create LevelCollection", fileName = "LevelCollection", order = 0)]
    public class LevelCollection : ScriptableObject, ITargetLoader<LevelInstance>
    {
        [SerializeField] private SerializedDictionary<SerializableId, LevelConfig> _levelDictionary;
        [SerializeField] private SerializedDictionary<TargetSubjectIdentifier, GameObject> _subjectDictionary;
        public bool TryGetLevel(SerializableId id, out LevelConfig levelConfig)
        {
            levelConfig = default;
            var noLevel = _levelDictionary.IsNullOrEmpty() || !_levelDictionary.TryGetValue(id, out levelConfig);
            return !noLevel;
        }

        public async UniTask<LevelInstance> LoadTargetSubject(TargetSubjectIdentifier id, CancellationToken cancellationToken)
        {
            GameObject prefab = null;
            var noSubject = _subjectDictionary.IsNullOrEmpty() || !_subjectDictionary.TryGetValue(id, out prefab);

            if (noSubject || prefab == null)
                return default;

            var instantiateOperation = await InstantiateAsync(prefab);

            var leverInstance = new LevelInstance(instantiateOperation[0]);

            return leverInstance;
        }
    }
}