using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.MonoBehaviours.Managers;

namespace Game.Generic.BehaviourTree
{
    public class SequenceNode : BehaviourTreeNode
    {        
        BehaviourTree dependency = null; 

        public SequenceNode(string nodeName) : base(nodeName) { }
        public SequenceNode(string nodeName, BehaviourTree _dependency) : base(nodeName)
        {
            dependency = _dependency;
        }

        public override BehaviourTreeStatus Process()
        {
            if (childNodes.Count == 0) return BehaviourTreeStatus.SUCCESS;

            if (dependency != null)
            {
                BehaviourTreeStatus dependencyStatus = dependency.Process();
                if (dependencyStatus == BehaviourTreeStatus.FAILURE)
                {
#if EDITOR_DEBUG_MODE
                    if (GameManager.GameDebug.GetShowBehaviourTreeDebug())
                    {
                        if (GameManager.GameDebug.GetBehaviourTreeDebugType().HasFlag(DebugManager.BehaviourTreeDebugType.Dependencies))
                        {
                            Debug.Log("Dependency tree for " + NodeName + " FAILED");
                            dependency.PrintTree();
                        }
                    }
#endif
                    Reset();
                    return BehaviourTreeStatus.FAILURE;
                }
                else if (dependencyStatus == BehaviourTreeStatus.RUNING) return BehaviourTreeStatus.RUNING;
            }

            BehaviourTreeStatus childStatus = childNodes[currentChildIndex].Process();
            if (childStatus == BehaviourTreeStatus.RUNING) return BehaviourTreeStatus.RUNING;
            if (childStatus == BehaviourTreeStatus.FAILURE)
            {
#if EDITOR_DEBUG_MODE
                if (GameManager.GameDebug.GetShowBehaviourTreeDebug())
                {
                    if (GameManager.GameDebug.GetBehaviourTreeDebugType().HasFlag(DebugManager.BehaviourTreeDebugType.Sequences))
                    {
                        Debug.Log(NodeName + " FAILED");
                    }
                }
#endif
                Reset();
                return BehaviourTreeStatus.FAILURE;
            }

            currentChildIndex++;

            if (currentChildIndex >= childNodes.Count)
            {
#if EDITOR_DEBUG_MODE
                if (GameManager.GameDebug.GetShowBehaviourTreeDebug())
                {
                    if (GameManager.GameDebug.GetBehaviourTreeDebugType().HasFlag(DebugManager.BehaviourTreeDebugType.Sequences))
                    {
                        Debug.Log(NodeName + " SUCCEDED");
                    }
                }
#endif
                Reset();
                return BehaviourTreeStatus.SUCCESS;
            }
            return BehaviourTreeStatus.RUNING;
        }
    }
}
