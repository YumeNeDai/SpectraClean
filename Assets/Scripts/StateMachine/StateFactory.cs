public class StateFactory
{
    PlayerStateMachine _playerStateMachine;

    public StateFactory(PlayerStateMachine state)
    {
        _playerStateMachine = state;
    }

    public PlayerAbstractState Idle()
    {
        return new PlayerIdleState(_playerStateMachine, this);
    }

    public PlayerAbstractState Walk()
    {
        return new PlayerWalkState(_playerStateMachine, this);
    }

    public PlayerAbstractState Attack()
    {
        return new PlayerAttackState(_playerStateMachine, this);
    }

    public PlayerAbstractState Grounded()
    {
        return new PlayerGroundedState(_playerStateMachine, this);
    }

    public PlayerAbstractState Falling()
    {
        return new PlayerFallingState(_playerStateMachine, this);
    }
}