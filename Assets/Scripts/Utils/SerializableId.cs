using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Utils
{
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