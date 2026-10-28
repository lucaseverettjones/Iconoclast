using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightPulsate : MonoBehaviour
{
    public Light point_light;
    public float speed = 5f;
    public float range = 10f;

    private const float intensityMin = 1.0f;

    void Start()
    {
        point_light = GetComponent<Light>();
    }

    void Update()
    {
        point_light.intensity = intensityMin + Mathf.PingPong(Time.time * speed, range - intensityMin);
    }
}
