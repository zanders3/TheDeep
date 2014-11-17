using UnityEngine;

public class PlayerCharacter : Character
{
    protected override Vector2 TakeInput()
    {
        Vector2 desiredVelocity = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * MaxSpeed;

        return (desiredVelocity - velocity) * MaxForce;
    }
}

