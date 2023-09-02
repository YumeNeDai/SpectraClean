using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerGroundedState : PlayerAbstractState, IRootState
{

    public PlayerGroundedState(PlayerStateMachine playerState, StateFactory playerStateFactory)
        :base(playerState, playerStateFactory)
    {
        _isRootState = true;
        InitializeSubState();
    }

    public override void EnterState() 
    {
        // reset the fall timeout timer
        _playerStateMachine.FallTimeoutDelta = _playerStateMachine.FallTimeout;
    }

    public override void UpdateState() 
    {
        VerticalVelocityFixed();
        CheckSwitchState();
    }

    public override void ExitState() { }

    public override void CheckSwitchState() 
    {
        if (!_playerStateMachine.Grounded)
        {
            SwitchState(_stateFactory.Falling());
        }

        if (_playerStateMachine.Input.IsAttacking) 
        {
            SwitchState(_stateFactory.Attack());
        }
    }

    public override void InitializeSubState()
    {
        SetSubState(_stateFactory.Walk());
  
    }

    private void VerticalVelocityFixed()
    {
        if (_playerStateMachine.VerticalVelocity < 2f)
        {
            _playerStateMachine.VerticalVelocity = -2f;
        }
        
    }

    public void HandleGravity()
    {

    }
}
