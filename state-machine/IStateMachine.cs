using System;

namespace jersmart
{
    public interface IStateMachine<TState, TTrigger>
    {
        TState CurrentState { get; }

        IStateMachine<TState, TTrigger> AddTransition(TState currentState, TTrigger trigger, TState futureState);
        IStateMachine<TState, TTrigger> AddTransition(TState currentState, TTrigger trigger, TState futureState, Action action);
        IStateMachine<TState, TTrigger> AddTransition<TActionParam>(TState currentState, TTrigger trigger, TState futureState, Action<TActionParam> action);
        IStateMachine<TState, TTrigger> OnEntry(TState state, Action action);
        IStateMachine<TState, TTrigger> OnExit(TState state, Action action);
        IStateMachine<TState, TTrigger> AddAction(TState state, Action action);
        void Act();
        void Transition(TTrigger trigger, params object[] args);
    }
}