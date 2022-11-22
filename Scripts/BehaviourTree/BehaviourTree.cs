using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Generic.BehaviourTree
{
    public class BehaviourTree : BehaviourTreeNode
    {
        public BehaviourTree(string nodeName) : base(nodeName) { }

        public override BehaviourTreeStatus Process()
        {
            if (childNodes.Count == 0) return BehaviourTreeStatus.SUCCESS;
            return childNodes[currentChildIndex].Process();
        }

        struct NodeLevel
        {
            public int level;
            public BehaviourTreeNode node;
        }

        public void PrintTree()
        {
            Stack<NodeLevel> nodes = new Stack<NodeLevel>();
            BehaviourTreeNode currentNode = this;
            nodes.Push(new NodeLevel { level = 0, node = currentNode });
            string output = "";
            while(nodes.Count > 0)
            {
                NodeLevel nextNode = nodes.Pop();
                output += new string('-', nextNode.level) + nextNode.node.NodeName + "\n";
                for (int i = nextNode.node.GetChildNodes().Count - 1; i >= 0; i--)
                {
                    nodes.Push(new NodeLevel { level = nextNode.level + 1, node = nextNode.node.GetChildNodes()[i] });
                }
            }
            Debug.Log(output);
        }

        public override void AddChild(BehaviourTreeNode node)
        {
            // Only one child is allowed for the root node
            if (childNodes.Count > 0) return;
            base.AddChild(node);
        }
    }
}
