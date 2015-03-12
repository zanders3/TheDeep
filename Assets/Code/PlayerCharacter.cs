using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayerCharacter : Character
{
    public const int MaxHealth = 6;

    Vector2 spawnPos;
    int health = MaxHealth;

    Vector2 hitPushbackDir = Vector2.zero;
    int hitPushbackFrames = 0;

    public ParticleSystem SpawnParticles;
    public CameraFollow CameraFollow;

    public PlayerUI PlayerUI;

    protected override void Start()
    {
        spawnPos = Position;
        base.OnCollision += OnCharCollision;
        PlayerUI.Health = health;
        base.Start();
    }

    void OnCharCollision(Character other)
    {
        hitPushbackDir = (Position - other.Position).normalized * .1f;
        hitPushbackFrames = 10;

        health--;
        PlayerUI.Health = health;

        if (health <= 0)
            Respawn();
    }

    void Respawn()
    {
        SetPosition(spawnPos.x, spawnPos.y);
        SpawnParticles.Play();
        CameraFollow.Reposition();
        health = MaxHealth;
    }

    protected override Vector2 TakeInput()
    {
        //Detect player death from falling
        if (transform.position.y < -2.0f)
        {
            Respawn();
            return Vector2.zero;
        }

        //Take user input
        Vector2 desiredVelocity = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * MaxSpeed;

        if (hitPushbackFrames > 0)
        {
            hitPushbackFrames--;
            AddImpulse(hitPushbackDir);
        }

        return (desiredVelocity - velocity) * MaxForce;
    }
}

