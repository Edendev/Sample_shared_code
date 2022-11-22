using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Generic.StateMachine
{
    public class StateMachine
    {
        Stack<State> states = new Stack<State>();
        int maxCapacity;

        public void Initialize(State initialState, int _maxCapacity = 10)
        {
            maxCapacity = _maxCapacity;
            states.Push(initialState);
            states.Peek().Enter();
        }

        public void ChangeState(State newState)
        {
            if (CurrentState() != null)
            {
                states.Peek().Exit();
                if (states.Count > maxCapacity) ReduceStack();
            }

            states.Push(newState);
            states.Peek().Enter();
        }

        void ReduceStack()
        {
            State firstState = states.Pop();
            states.Clear();
            states.Push(firstState);
        }

        public State CurrentState()
        {
            return states.Peek();
        }

        public State PreviousState()
        {
            if (states.Count < 2) return null;
            return states.Skip(1).First();
        }
    }
}

