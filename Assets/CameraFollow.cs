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

    void Update()
    {
        if (Target == null)
            return;

        transform.position = Vector3.MoveTowards(transform.position, Target.position + offset, Time.deltaTime * 3.0f);
        transform.LookAt(Target);
    }
}