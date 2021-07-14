using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Generic.Entities
{
    public abstract class RemoteControlAction {}
    public class MoveToDestination_RCA : RemoteControlAction
    {
        public Vector3 Destination { get; private set; }
        public MoveToDestination_RCA(Vector3 _destination) => Destination = _destination;
    }
    public class UpdateStartingTransformToCurrent_RCA : RemoteControlAction { }
    public class DisablePatrolWaypoints_RCA : RemoteControlAction { }
    public class EnablePatrolWaypoints_RCA : RemoteControlAction { }
}

