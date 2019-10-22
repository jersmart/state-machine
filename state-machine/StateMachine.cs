using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace jersmart
{
    public class StateMachine<TState, TTrigger> : IStateMachine<TState, TTrigger>
    {
        public TState CurrentState { get; private set; }

        Dictionary<Tuple<TState, TTrigger>, TState> _transitions = new Dictionary<Tuple<TState, TTrigger>, TState>();
        Dictionary<Tuple<TState, TTrigger>, Delegate> _transitionDelegates = new Dictionary<Tuple<TState, TTrigger>, Delegate>();
        Dictionary<Tuple<TState, TTrigger>, List<Type>> _transitionDelegateTypes = new Dictionary<Tuple<TState, TTrigger>, List<Type>>();
        Dictionary<TState, List<Action>> _stateEntryDelegates = new Dictionary<TState, List<Action>>();
        Dictionary<TState, List<Action>> _stateExitDelegates = new Dictionary<TState, List<Action>>();
        Dictionary<TState, List<Action>> _stateActions = new Dictionary<TState, List<Action>>();

        public StateMachine(TState initialState)
        {
            CurrentState = initialState;
        }

        public IStateMachine<TState, TTrigger> AddTransition(TState currentState, TTrigger trigger, TState futureState)
        {
            var key = new Tuple<TState, TTrigger>(currentState, trigger);
            _transitions.Add(key, futureState);

            return this;
        }

        public IStateMachine<TState, TTrigger> AddTransition(TState currentState, TTrigger trigger, TState futureState, Action action)
        {
            var key = new Tuple<TState, TTrigger>(currentState, trigger);
            _transitions.Add(key, futureState);
            AddDelegate(key, action);
            return this;
        }

        public IStateMachine<TState, TTrigger> AddTransition<TActionParam>(TState currentState, TTrigger trigger, TState futureState, Action<TActionParam> action)
        {
            var key = new Tuple<TState, TTrigger>(currentState, trigger);
            _transitions.Add(key, futureState);
            AddDelegate(key, action);
            return this;
        }

        public IStateMachine<TState, TTrigger> OnEntry(TState state, Action action)
        {
            if (!_stateEntryDelegates.ContainsKey(state))
                _stateEntryDelegates[state] = new List<Action>();

            _stateEntryDelegates[state].Add(action);

            return this;
        }

        public IStateMachine<TState, TTrigger> OnExit(TState state, Action action)
        {
            if (!_stateExitDelegates.ContainsKey(state))
                _stateExitDelegates[state] = new List<Action>();

            _stateExitDelegates[state].Add(action);

            return this;
        }

        public IStateMachine<TState, TTrigger> AddAction(TState state, Action action) {
            if (!_stateActions.ContainsKey(state))
                _stateActions[state] = new List<Action>();

            _stateActions[state].Add(action);

            return this;
        }

        public void Act() {
            foreach(var action in _stateActions[CurrentState])
            {
                action.Invoke();
            }
        }

        private void AddDelegate(Tuple<TState, TTrigger> key, Action action)
        {
            _transitionDelegates.Add(key, action);
        }

        private void AddDelegate<TActionParam>(Tuple<TState, TTrigger> key, Action<TActionParam> action)
        {
            _transitionDelegates.Add(key, action);
            var types = new List<Type>();
            types.Add(typeof(TActionParam));
            _transitionDelegateTypes.Add(key, types);
        }    

        public void Transition(TTrigger trigger, params object[] args)
        {
            var key = new Tuple<TState, TTrigger>(CurrentState, trigger);

            if (!_transitions.ContainsKey(key))
                throw new Exception("The trigger " + trigger.ToString() + " is not valid in the current state: " + CurrentState.ToString());

            // Confirm parameter args are correct
            if (_transitionDelegateTypes.ContainsKey(key))
            {
                var parameterTypes = _transitionDelegateTypes[key];
                var parameterTypesMatch = true;

                if (parameterTypes.Count != args.Count())
                    parameterTypesMatch = false;
                else
                {
                    for (int i = 0; i < parameterTypes.Count; i++)
                    {
                        if (parameterTypes[i] != args[i].GetType())
                        {
                            parameterTypesMatch = false;
                            break;
                        }
                    }
                }

                if (!parameterTypesMatch)
                {
                    var sb = new StringBuilder();
                    sb.Append("This transition requires parameters of the following types: ");
                    foreach (var parameterType in parameterTypes)
                    {
                        sb.Append(parameterType.ToString());
                        sb.Append(", ");
                    }
                    sb.Remove(sb.Length - 2, 2);

                    throw new ArgumentException(sb.ToString());

                }
            }

            // Call OnExit Delegate
            if (_stateExitDelegates.ContainsKey(CurrentState))
            {
                foreach(var exitDelegate in _stateExitDelegates[CurrentState])
                    exitDelegate.Invoke();
            }

            // Call Transition Delegate
            if (_transitionDelegates.ContainsKey(key))
            {
                var _delegate = _transitionDelegates[key];
                _delegate.DynamicInvoke(args);
            }

            // Transition State
            CurrentState = _transitions[key];

            // Call OnEntry Delegate
            if (_stateEntryDelegates.ContainsKey(CurrentState))
            {
                foreach (var entryDelegate in _stateEntryDelegates[CurrentState])
                    entryDelegate.Invoke();
            }
        }
    }
}
