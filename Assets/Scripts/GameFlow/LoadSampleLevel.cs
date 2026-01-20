using GameFlow;
using UnityEngine;
using ZeroMessenger;

public class LoadSampleLevel : MonoBehaviour
{
    [ContextMenu("level")]
    private void Awake()
    {
        MessageBroker<RequestLevel>.Default.Publish(new RequestLevel());
    }
}