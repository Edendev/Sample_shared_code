using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Interfaces
{
    public interface IDestroyable
    {
        void OnDestroyed(GameObject destroyer);
    }
}