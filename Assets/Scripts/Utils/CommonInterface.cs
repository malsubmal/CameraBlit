using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Pool;

namespace Utils
{
    public interface IDefaultable<T>
    {
        public bool IsDefault();
        public void SetDefault();
    }
}
