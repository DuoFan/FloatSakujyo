using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace GameExtension
{
    public interface IState<T>
    {
        void Enter(T entity);
        void Execute(T entity);
        void Exit(T entity);
    }

    public interface IStateMachine<T> 
    {
        T Entity { get; }
        IState<T> CurrentState { get; set; }
        IState<T> PreviousState { get; set; }
    }

    public static class StateMachineExtension
    {
        public static void ChangeState<T>(this IStateMachine<T> machine, IState<T> newState)
        {
            if(machine.CurrentState != null)
            {
                if (newState.GetType().Equals(machine.CurrentState.GetType()))
                {
                    return;
                }
            }

            machine.PreviousState = machine.CurrentState;
            machine.CurrentState?.Exit(machine.Entity);
            machine.CurrentState = newState;
            machine.CurrentState.Enter(machine.Entity);
        }
        public static void RevertToPreviousState<T>(this IStateMachine<T> machine)
        {
            machine.CurrentState?.Exit(machine.Entity);
            machine.CurrentState = machine.PreviousState;
            machine.CurrentState?.Enter(machine.Entity);
            machine.PreviousState = null;
        }

        public static void ExitState<T>(this IStateMachine<T> machine)
        {
            machine.CurrentState?.Exit(machine.Entity);
            if (machine != null)
            {
                machine.CurrentState = null;
            }
        }

        public static void ClearState<T>(this IStateMachine<T> machine)
        {
            if (machine != null)
            {
                machine.CurrentState?.Exit(machine.Entity);
                machine.CurrentState = null;
                machine.PreviousState = null;
            }
        }
    }
}

