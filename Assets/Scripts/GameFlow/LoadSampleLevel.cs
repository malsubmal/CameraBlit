using GameFlow;
using UnityEngine;
using ZeroMessenger;

public class LoadSampleLevel : MonoBehaviour
{
    private void Awake()
    {
        MessageBroker<RequestLevel>.Default.Publish(new RequestLevel());
    }
}