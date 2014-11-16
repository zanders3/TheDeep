using UnityEngine;

public abstract class Character : MonoBehaviour
{
    public Map Map;
    public float MaxSpeed = 1.0f, MaxForce = 1.0f;

    Vector2 velocity;

    protected abstract Vector2 TakeInput();

    void Update()
    {
        Vector2 steering = TakeInput();

    }
}
