using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.MonoBehaviours.Managers;

namespace Game.Generic.BehaviourTree
{
    public class LeafNode : BehaviourTreeNode
    {
        public delegate BehaviourTreeStatus Tick();
        public Tick ProcessMethod;
        
        public delegate void OnReset();
        public OnReset ResetMethods;

        public LeafNode() : base() { }
        public LeafNode(string nodeName, Tick processMethod) : base (nodeName)
        {
            ProcessMethod = processMethod;
        }

        public override BehaviourTreeStatus Process()
        {
#if EDITOR_DEBUG_MODE
            if (GameManager.GameDebug.GetShowBehaviourTreeDebug())
            {
                if (GameManager.GameDebug.GetBehaviourTreeDebugType().HasFlag(DebugManager.BehaviourTreeDebugType.Leafs))
                {
                    Debug.Log("Leaf: " + NodeName);
                }
            }
#endif
            return ProcessMethod != null ? ProcessMethod() : BehaviourTreeStatus.FAILURE;
        }

        public override void Reset()
        {
            base.Reset();
            if (ResetMethods != null) ResetMethods?.Invoke();
        }

        public virtual void AddResetMethod(OnReset method)
        {
            ResetMethods += method;
        }

        public virtual void RemoveResetMethod(OnReset method)
        {
            ResetMethods -= method;
        }
    }
}
