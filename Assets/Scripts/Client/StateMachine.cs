using System.Collections.Generic;

public class StateMachine<T>
{
    State currentState = null;
    Dictionary<T, State> states = new Dictionary<T, State>();

    public delegate void StateFunc();

    public void Add(T id, StateFunc enter, StateFunc update, StateFunc leave)
    {
        states.Add(id, new State(id, enter, update, leave));
    }

    public T CurrentState()
    {
        return currentState.Id;
    }

    public void Update()
    {
        if (currentState.Update != null)
            currentState.Update();
    }

    public void Shutdown()
    {
        if (currentState != null && currentState.Leave != null)
            currentState.Leave();
        currentState = null;
    }

    public void SwitchTo(T state)
    {
        var newState = states[state];

        if (currentState != null && currentState.Leave != null)
            currentState.Leave();
        if (newState.Enter != null)
            newState.Enter();
        currentState = newState;

    }

    class State
    {
        public State(T id, StateFunc enter, StateFunc update, StateFunc leave)
        {
            Id = id;
            Enter = enter;
            Update = update;
            Leave = leave;
        }
        public T Id;
        public StateFunc Enter;
        public StateFunc Update;
        public StateFunc Leave;
    }
}
