using UnityEngine;
using UnityEngine.InputSystem;

namespace StarterAssets
{
    public class StarterAssetsInputs : MonoBehaviour
    {
        [Header("Character Input Values")]
        [SerializeField] private Vector2 move;
        [SerializeField] private Vector2 rotation;
        [SerializeField] private bool attack;
        [SerializeField] private bool sprint;

        [Header("Movement Settings")]
        [SerializeField] private bool analogMovement;

        public Vector2 GetMove { get => move; }
        public Vector2 GetRotation { get => rotation; }
        public bool IsAttacking { get => attack; }
        public bool IsSprinting { get => sprint; }
        public bool IsAnalog { get => analogMovement; }

        public void OnMove(InputAction.CallbackContext value)
        {
            MoveInput(value.ReadValue<Vector2>());
        }

        public void OnRotation(InputAction.CallbackContext value)
        {
            RotationInput(value.ReadValue<Vector2>());
        }

        public void OnAttack(InputAction.CallbackContext value)
        {
            AttackInput(value.action.triggered);
        }

        public void OnSprint(InputAction.CallbackContext value)
        {
            SprintInput(value.action.ReadValue<float>() == 1);
        }

        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        }

        public void RotationInput(Vector2 newRotationDirection)
        {
            rotation = newRotationDirection;
        }

        private void AttackInput(bool newJumpState)
        {
            attack = newJumpState;
        }

        public void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }
    }

}