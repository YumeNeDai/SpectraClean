public abstract class PlayerAbstractState
{
    protected bool _isRootState = false;
    protected PlayerStateMachine _playerStateMachine;
    protected StateFactory _stateFactory;
    private PlayerAbstractState _currentSuperState;
    private PlayerAbstractState _currentSubState;

    public PlayerAbstractState(PlayerStateMachine playerState, StateFactory playerStateFactory)
    {
        _playerStateMachine = playerState;
        _stateFactory = playerStateFactory;
    }

    public abstract void EnterState();

    public abstract void UpdateState();

    public abstract void ExitState();

    public abstract void CheckSwitchState();

    public abstract void InitializeSubState();

    public void UpdateStates()
    {
        UpdateState();
        if (_currentSubState != null)
        {
            _currentSubState.UpdateStates();
        }
    }

    protected void SwitchState(PlayerAbstractState newState)
    {
        ExitState();

        newState.EnterState();

        if (_isRootState)
        {
            _playerStateMachine.CurrentState = newState;
        }

        else if (_currentSuperState != null)
        {
            _currentSuperState.SetSubState(newState);
        }
    }

    protected void SetSuperState(PlayerAbstractState newSuperState)
    {
        _currentSuperState = newSuperState;
    }

    protected void SetSubState(PlayerAbstractState newSubState)
    {
        _currentSubState = newSubState;
        newSubState.SetSuperState(this);
    }

    public void ExitStates()
    {
        ExitState();
        if (_currentSubState != null)
        {
            _currentSubState.ExitStates();
        }
    }

}