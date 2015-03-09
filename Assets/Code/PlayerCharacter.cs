using UnityEngine;

public class PlayerCharacter : Character
{
    Vector2 spawnPos;

    public ParticleSystem SpawnParticles;
    public CameraFollow CameraFollow;

    protected override void Start()
    {
        spawnPos = Position;
        base.Start();
    }

    protected override Vector2 TakeInput()
    {
        //Detect player death from falling
        if (transform.position.y < -2.0f)
        {
            SetPosition(spawnPos.x, spawnPos.y);
            SpawnParticles.Play();
            CameraFollow.Reposition();
            return Vector2.zero;
        }

        //Take user input
        Vector2 desiredVelocity = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * MaxSpeed;

        return (desiredVelocity - velocity) * MaxForce;
    }
}

