using System;
using System.Collections.Generic;
using UnityEngine;

// Code from https://www.toptal.com/unity/unity-ai-development-finite-state-machine-tutorial
namespace AI.FSM
{
    public class FiniteStateMachine : MonoBehaviour
    {
        [SerializeField] private State initialState;
        private Dictionary<Type, Component> cachedComponents;
        
        public State currentState { get; set; }

        private void Awake()
        {
            currentState = initialState;
            cachedComponents = new Dictionary<Type, Component>();
        }

        private void Update()
        {
            currentState.Execute(this);
        }
        
        public new T GetComponent<T>() where T : Component
        {
            if (cachedComponents.ContainsKey(typeof(T)))
                return cachedComponents[typeof(T)] as T;

            var component = base.GetComponent<T>();
            
            if (component != null)
            {
                cachedComponents.Add(typeof(T), component);
            }
            
            return component;
        }
    }
}
