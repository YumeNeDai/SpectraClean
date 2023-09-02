using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerWalkState : PlayerAbstractState
{
    private float _rotationVelocity;

    public PlayerWalkState(PlayerStateMachine playerState, StateFactory playerStateFactory)
       : base(playerState, playerStateFactory)
    {

    }

    public override void EnterState() { }

    public override void UpdateState() 
    {
        Move();
        CheckSwitchState();
    }

    public override void ExitState() { }

    public override void CheckSwitchState()
    {
        
    }

    public override void InitializeSubState() { }

    private void Move()
    {
        // set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = _playerStateMachine.Input.IsSprinting ? _playerStateMachine.RunningSpeed : _playerStateMachine.MoveSpeed;

        // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

        // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is no input, set the target speed to 0
        if (_playerStateMachine.Input.GetMove == Vector2.zero) targetSpeed = 0.0f;

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_playerStateMachine.Controller.velocity.x, 0.0f, _playerStateMachine.Controller.velocity.z).magnitude;

        float inputMagnitude = _playerStateMachine.Input.IsAnalog ? _playerStateMachine.Input.GetMove.magnitude : 1f;

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed <= targetSpeed - PlayerStateMachine.SpeedOffset ||
            currentHorizontalSpeed >= targetSpeed + PlayerStateMachine.SpeedOffset)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            _playerStateMachine.Speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                Time.deltaTime * _playerStateMachine.SpeedChangeRate);

            // round speed to 3 decimal places
            _playerStateMachine.Speed = Mathf.Round(_playerStateMachine.Speed * 1000f) / 1000f;
        }

        else
        {
            _playerStateMachine.Speed = targetSpeed;
        }

        _playerStateMachine.AnimationBlend = Mathf.Lerp(_playerStateMachine.AnimationBlend, targetSpeed, Time.deltaTime * _playerStateMachine.SpeedChangeRate);
        if (_playerStateMachine.AnimationBlend < 0.01f) _playerStateMachine.AnimationBlend = 0f;

        Vector2 movement = _playerStateMachine.Input.GetMove;
        // normalise input direction
        Vector3 inputDirection = new Vector3(movement.x, 0.0f, movement.y).normalized;

        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is a move input rotate player when the player is moving
        if (movement != Vector2.zero)
        {
            _playerStateMachine.TargetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                              _playerStateMachine.MainCamera.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(_playerStateMachine.transform.eulerAngles.y, _playerStateMachine.TargetRotation, ref _rotationVelocity,
                _playerStateMachine.RotationSmoothTime);

            // rotate to face input direction relative to camera position
            _playerStateMachine.transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, _playerStateMachine.TargetRotation, 0.0f) * Vector3.forward;

        // move the player
        _playerStateMachine.Controller.Move(targetDirection.normalized * (_playerStateMachine.Speed * Time.deltaTime) +
                             new Vector3(0.0f, _playerStateMachine.VerticalVelocity, 0.0f) * Time.deltaTime);

        // update animator if using character
        _playerStateMachine.Animator.SetFloat(_playerStateMachine.AnimIDSpeed, _playerStateMachine.AnimationBlend);
        _playerStateMachine.Animator.SetFloat(_playerStateMachine.AnimIDMotionSpeed, inputMagnitude);
    }

}
