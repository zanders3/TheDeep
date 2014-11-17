using UnityEngine;

public class Baddie : Character
{
    public PlayerCharacter Player;
    public const float AttackDistSq = 2.0f * 2.0f;

    private Vector2 startPos;

    void Start()
    {
        startPos = Position;
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
