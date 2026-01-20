using UnityEngine;

namespace Level
{
    public class SubjectHost : MonoBehaviour
    {
        [SerializeField] private Transform _hostTransform; //
        private ITargetSubject _targetSubject;

        public void HostSubject(ITargetSubject targetSubject)
        {
            //take transform
            targetSubject.SubjectTransform.SetParent(_hostTransform);
            targetSubject.SubjectTransform.localScale = Vector3.one;
        }

    }
}