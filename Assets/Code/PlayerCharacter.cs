using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerCharacter : Character
{
    public const int MaxHealth = 6;
    public const float AttackRange = 1f;

    public Sprite Normal, Attack;

    Vector2 spawnPos;
    int health = MaxHealth;

    Vector2 hitPushbackDir = Vector2.zero;
    int hitPushbackFrames = 0;

    public ParticleSystem SpawnParticles;
    public CameraFollow CameraFollow;

    protected override void Start()
    {
        spawnPos = Position;
        GameManager.PlayerUI.Health = health;
        base.Start();
    }

    public override void Damage(Character sender)
    {
        hitPushbackDir = (Position - sender.Position).normalized * .1f;
        hitPushbackFrames = 10;

        health--;
        GameManager.PlayerUI.Health = health;

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

    List<Character> attackList = new List<Character>();

    protected override Vector2 TakeInput()
    {
        //Detect player death from falling
        if (transform.position.y < -2.0f)
        {
            Respawn();
            return Vector2.zero;
        }

        GetComponent<SpriteRenderer>().sprite = Input.GetButton("Attack") ? Attack : Normal;
        if (Input.GetButtonDown("Attack"))
        {
            GameManager.CharacterUpdater.FindAllInRadius(attackList, Position, AttackRange);
            foreach (Character character in attackList)
            {
                if (character == this)
                    continue;
                character.Damage(this);
            }
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

