using UnityEngine;

public class Baddie : Character
{
    public PlayerCharacter Player;
    public const float AttackDistSq = 2.0f * 2.0f;

    Vector2 startPos;

    protected override void Start()
    {
        startPos = Position;
        base.Start();
    }

    protected override Vector2 TakeInput()
    {
        Vector2 playerDir = Player.Position - Position;
        Vector2 desiredVelocity;

        if (playerDir.sqrMagnitude >= AttackDistSq)
            desiredVelocity = (startPos - Position).normalized * MaxSpeed;
        else
            desiredVelocity = playerDir.normalized * MaxSpeed;

        return (desiredVelocity - velocity) * MaxForce;
    }
}
