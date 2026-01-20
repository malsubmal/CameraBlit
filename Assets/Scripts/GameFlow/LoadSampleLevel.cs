using UnityEngine;
using ZeroMessenger;

namespace GameFlow
{
    public class LoadSampleLevel : MonoBehaviour
    {
        private void Awake()
        {
            MessageBroker<RequestLevel>.Default.Publish(new RequestLevel());
        }
    }
}