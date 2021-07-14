using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Game.Interfaces;
using Game.Generic.PathFinding;
using Game.PropertyAttributes;
using Game.MonoBehaviours.Managers;

namespace Game.MonoBehaviours
{
    public class WorldNode : MonoBehaviour, INode
    {
        NavMeshPath pathToCheck;

        #region MonoBehaviour

        private void Awake()
        {
            pathToCheck = new NavMeshPath();
        }

        private void Start()
        {
            // Add node to GameManager
            GameManager.AddNode(this);
        }

        private void OnDisable()
        {
            // Remove entity from game manager
            GameManager.RemoveNode(this);

            // Recalculate neighbours from neighbours
            foreach (INode node in Neighbours)
            {
                if (node.Neighbours != null)
                {
                    WorldNode neighbourEntityRef = node.WorldObject().GetComponent<WorldNode>();
                    List<INode> toAdd = new List<INode>();
                    foreach (INode neighbourNode in Neighbours)
                    {
                        if (neighbourNode.WorldObject() != node.WorldObject())
                            neighbourEntityRef.AddNeighbour(neighbourNode.WorldObject());
                    }
                    neighbourEntityRef.RemoveNeighbour(gameObject);
                }
            }
        }

        /// <summary>
        /// Time-consuming operation to recalculate all neighbours
        /// </summary>
        public void RecalculateNeighbours()
        {
            // Erase neigbhours
            neighboursList.Clear();
            neighboursList.TrimExcess();

            INode[] allNodes = FindObjectsOfType<MonoBehaviour>().OfType<INode>().ToArray();
            List<INode> orderedNodesCloseToNode = new List<INode>(allNodes);

            // Sort destroyable entities by distsance to this entity
            orderedNodesCloseToNode.Sort(delegate (INode a, INode b)
            {
                return (transform.position - a.WorldObject().transform.position).magnitude.CompareTo((transform.position - b.WorldObject().transform.position).magnitude);
            });

            // Get all closest accesible nodes and add them as a neighbour of this node neigihbours
            foreach (INode closestNode in orderedNodesCloseToNode)
            {
                // Check if path is available
                if (closestNode.WorldObject() != this)
                {
                    if (PathFinding.CheckPathToTarget(gameObject, closestNode.WorldObject().gameObject, out pathToCheck))
                    {
                        AddNeighbour(closestNode.WorldObject());
                    }
                }
            }
        }

        #endregion

        #region INode

        /// <summary>
        /// Assign the neighbours that will be used for the path finding
        /// </summary>
        [TypeConstraint(typeof(INode))]
        [SerializeField] List<GameObject> neighboursList;

        public IEnumerable<INode> Neighbours
        {
            get
            {
                List<INode> nodes = new List<INode>();
                foreach (GameObject go in neighboursList)
                {
                    if (go != null)
                    {
                        nodes.Add(go.GetComponent<INode>());
                    }
                }

                return nodes;
            }
        }

        public IEnumerable<INode> GetEnumerator()
        {
            return GetEnumerator();
        }

        public float CostTo(INode neighbour) => (neighbour.WorldObject().transform.position - transform.position).magnitude;

        public float EstimatedCostTo(INode goal)
        {
            WorldNode goalEntity = (WorldNode)goal;

            return (goalEntity.transform.position - transform.position).magnitude;
        }

        public GameObject WorldObject() => gameObject;

        #endregion

        public void AddNeighbour(GameObject toAdd)
        {
            INode node = toAdd.GetComponent<INode>();

            if (node == null)
                return;

            if (neighboursList.Find(x => x == toAdd) != null)
                return;

            neighboursList.Add(toAdd);
        }
        public void RemoveNeighbour(GameObject toRemove)
        {
            INode node = toRemove.GetComponent<INode>();

            if (node == null)
                return;

            if (neighboursList.Find(x => x == toRemove) == null)
                return;

            neighboursList.Remove(toRemove);
        }


#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            // Show neighbour connections
            if (neighboursList != null)
            {
                foreach (GameObject neighbour in neighboursList)
                {
                    if (neighbour != null)
                        Debug.DrawLine(transform.position, neighbour.transform.position, Color.cyan);
                }
            }
        }

#endif
    }
}

