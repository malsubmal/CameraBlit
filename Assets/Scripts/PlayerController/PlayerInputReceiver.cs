using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PlayerController
{
    public class PlayerInputReceiver : MonoBehaviour
    {
        [Header("Input Config")] 
        [SerializeField] private bool _isReverseHorizontal = true;
        [SerializeField] private float _thresholdTakePhoto = 0.5f;
        
        [SerializeField] private KineticController _kineticController;
        [SerializeField] private CameraFunctionController _cameraFunctionController;
        [SerializeField] private PlayerKineticInput _playerKineticInput;
        
        private PlayerKineticInput _queueInput;
        //private CameraFunctionInput _cameraInput;
        
        private void Update()
        {
            var forwardInput = Input.GetAxis("Vertical");
            var sideWayInput = Input.GetAxis("Horizontal");
            var turnInput = Input.GetAxis("Mouse ScrollWheel");
            var photoInput = Input.GetAxis("Fire1");
            
            RecordCameraInput(photoInput);
            RecordKineticInput(forwardInput, _isReverseHorizontal ? -sideWayInput : sideWayInput, turnInput);
        }

        private void RecordKineticInput(float forward, float sideway, float turn)
        {
            _queueInput = new PlayerKineticInput(new Vector2(forward, sideway), turn);
        }

        private void RecordCameraInput(float photoInput)
        {
            if (photoInput < _thresholdTakePhoto) return;
               
            _cameraFunctionController.CapturePhoto().Forget();
        }

        private void FixedUpdate()
        {
            _kineticController.ProcessKineticInput(_queueInput, Time.fixedDeltaTime);
            _queueInput.SetDefault();
        }
    }
}