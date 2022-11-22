using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Generic.StateMachine
{
    public abstract class State
    {
        public virtual void Enter() { }
        public virtual void HandleAxisInput() { }
        public virtual void HandleLogic() { }
        public virtual void HandlePhysics() { }
        public virtual void HandleLateUpdate() { }
        public virtual void Exit() { }
    }
}
