using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform Target;
    private Vector3 offset, velocity;

    void Start()
    {
        if (Target != null)
            offset = transform.position - Target.transform.position;
    }

    float[] targetYPos = null;
    int currentIdx = 0;

    void FixedUpdate()
    {
        if (Target == null)
            return;
        if (targetYPos == null)
        {
            targetYPos = new float[40];
            for (int i = 0; i<targetYPos.Length; i++)
                targetYPos[i] = Target.transform.position.y;
        }

        Vector3 targetPos = Target.transform.position;
        targetYPos[currentIdx] = targetPos.y;
        currentIdx = (currentIdx + 1) % targetYPos.Length;

        targetPos.y = targetYPos[0];
        for (int i = 1; i<targetYPos.Length; i++)
            targetPos.y = Mathf.Min(targetYPos[i], targetPos.y);

        transform.position += ((targetPos + offset) - transform.position) * 0.1f;
        transform.LookAt(targetPos);
    }
}