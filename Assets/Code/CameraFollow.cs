using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Character Target;
    private Vector3 offset, velocity;

    void Start()
    {
        if (Target != null)
            offset = transform.position - Target.transform.position;
    }

    void FixedUpdate()
    {
        if (Target == null)
            return;

        Vector3 targetPos = new Vector3(Target.transform.position.x, Target.Height, Target.transform.position.z);

        transform.position += ((targetPos + offset) - transform.position) * 0.1f;
        transform.LookAt(targetPos);
    }
}