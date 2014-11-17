using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform Target;
    private Vector3 offset, velocity;

    void Start()
    {
        if (Target != null)
            offset = transform.position - Target.position;
    }

    void FixedUpdate()
    {
        if (Target == null)
            return;

        transform.position += ((Target.position + offset) - transform.position) * 0.1f;
        transform.LookAt(Target);
    }
}