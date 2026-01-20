using BlitRendererFeature.CommonPayload;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZeroMessenger;

public class TestBlitRequest : MonoBehaviour
{
    [ContextMenu("Test Blit Request")]
    private async void RequestBlit()
    {
        var req = new BlitPhotoRequest(null);
        var handle = req.FinishHandle;
        MessageBroker<BlitPhotoRequest>.Default.Publish(req, this.GetCancellationTokenOnDestroy());
        await handle.Task;
        Debug.Log("Finish Blit");
    }
}