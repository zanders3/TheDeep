using UnityEngine;

public class Torch : MonoBehaviour
{
    public float Speed = 1.0f, Scale = 1.0f, Offset = 0.0f;

    void Update()
    {
        light.range = Mathf.PerlinNoise(Time.realtimeSinceStartup * Speed, 0.0f) * Scale + Offset;
    }
}
