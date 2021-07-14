using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.MonoBehaviours.Particles
{
    public class MouseOverTargetEffect : MonoBehaviour
    {
        ParticleSystem _particleSystem;
        ParticleSystem.MainModule mainModule;
        ParticleSystem.ShapeModule shapeModule;

        void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            mainModule = _particleSystem.main;
            shapeModule = _particleSystem.shape;
        }

        public void SetRadius(float radius)
        {
            shapeModule.radius = radius;
            mainModule.startSize = radius/5f;
        }
    }
}