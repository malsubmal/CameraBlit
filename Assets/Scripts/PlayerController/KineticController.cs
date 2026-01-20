using System;
using UnityEngine;
using Utils;

namespace MovementFunction
{
    [Serializable]
    public class KineticController
    {
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private Vector2 _movementSpeed;
        [SerializeField] private float _turnSpeed;


        public void ProcessKineticInput(PlayerKineticInput playerInput, float deltaTime)
        {
            Turn(playerInput, deltaTime);
            Move(playerInput, deltaTime);
        }
        
        public void Move(PlayerKineticInput playerInput, float deltaTime)
        {
            if (playerInput.MovementVector == Vector2.zero)
            {
                return;
            }
            
            var movementUnit = _movementSpeed * deltaTime;
            var normalizeInput = playerInput.MovementVector.normalized;

            var transform = _characterController.transform;
            
            var forwardDirection = transform.forward;
            var forwardMotion = normalizeInput.x * movementUnit.x;

            var rightDirection = transform.right; //right as positive;
            var sideWayMotion = normalizeInput.y * movementUnit.y;

            var sumMotion = forwardDirection * forwardMotion + sideWayMotion * rightDirection;
            sumMotion.y = 0;
            _characterController.Move(sumMotion);
        }

        public void Turn(PlayerKineticInput playerInput, float deltaTime)
        {
            var rotateUnit = deltaTime * _turnSpeed;
            var clampTurnInput = Mathf.Clamp(playerInput.TurnValue, -1f, 1f);
            _characterController.transform.RotateAround(_characterController.transform.position, Vector3.up, clampTurnInput * rotateUnit);    
        }
    }

    public struct PlayerKineticInput : IDefaultable<PlayerKineticInput>
    {
        private Vector2 _movementVector;
        private float _turnValue;

        public PlayerKineticInput(Vector2 movement, float turn)
        {
            _movementVector = movement;
            _turnValue = turn;
        }

        public Vector2 MovementVector => _movementVector;
        public float TurnValue => _turnValue;
        
        public bool IsDefault()
        {
            return _movementVector.Equals(Vector2.zero) && _turnValue == 0;

        }

        public void SetDefault()
        {
            _movementVector = Vector2.zero;
            _turnValue = 0;
        }
    }

}