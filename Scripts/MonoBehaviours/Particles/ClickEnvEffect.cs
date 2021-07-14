using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.MonoBehaviours.Particles
{
public class ClickEnvEffect : MonoBehaviour
{
    ParticleSystem _particleSystem;
    ParticleSystem.MainModule mainModule;
    ParticleSystem.ShapeModule shapeModule;

    [Tooltip("Integer which sets the number of steps for the animation effects, the higher the smoother")]
    [Range(10, 100)]
    [SerializeField] int Smoothness = 10;

    float initialShapeRadius;
    float initialStartSize;
    float totalEffectTime;

    // Start is called before the first frame update
    void Start()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        mainModule = _particleSystem.main;
        shapeModule = _particleSystem.shape;

        totalEffectTime = mainModule.duration;
        initialShapeRadius = shapeModule.radius;
        initialStartSize = mainModule.startSize.constant;

        StartCoroutine(DecreaseShape());
        StartCoroutine(DecreaseStartLife());
    }

    IEnumerator DecreaseShape()
    {
        int i = 1;
        while (shapeModule.radius > 0f)
        {
            shapeModule.radius = initialShapeRadius - initialShapeRadius / Smoothness * i;
            i++;
            yield return new WaitForSeconds(totalEffectTime/Smoothness);
        }

        yield return null;
    }

    IEnumerator DecreaseStartLife()
    {
        int i = 1;
        while (mainModule.startLifetime.constant > 0f)
        {
            mainModule.startSize = initialStartSize - initialStartSize / Smoothness * i;
            i++;
            yield return new WaitForSeconds(totalEffectTime / Smoothness);
        }

        yield return null;
    }
}
    }