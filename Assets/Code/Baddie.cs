using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Baddie : Character
{
    public PlayerCharacter Player;
    public const float AttackDistSq = 2f * 2f;
    public const int NumIdleFrames = 80;

    public Sprite AttackSprite, NormalSprite;

    Vector2 startPos;
    int baddieIdleFrames = 0;

    protected override void Start()
    {
        startPos = Position;
        base.OnCollision = OnCharCollision;
        GetComponent<SpriteRenderer>().sprite = NormalSprite;
        base.Start();
    }

    void OnCharCollision(Character other)
    {
        if (other == Player)
        {
            Player.Damage(this);
            GetComponent<SpriteRenderer>().sprite = AttackSprite;
            baddieIdleFrames = NumIdleFrames;
        }
    }

    public override void Damage(Character sender)
    {
        GameObject.Destroy(gameObject);
    }

    protected override Vector2 TakeInput()
    {
        if (baddieIdleFrames > 0)
        {
            baddieIdleFrames--;
            if (baddieIdleFrames <= 0)
                GetComponent<SpriteRenderer>().sprite = NormalSprite;

            return -velocity;
        }

        if (transform.position.y < 0.0f)
            GameObject.Destroy(gameObject);

        Vector2 playerDir = Player.Position - Position;
        Vector2 desiredVelocity;

        Vector2 distFromStart = (startPos - Position);
        float distFromStartLen = distFromStart.sqrMagnitude;
        if (playerDir.sqrMagnitude >= AttackDistSq)
        {
            if (distFromStartLen < 1f)
                desiredVelocity = Vector2.zero;
            else
                desiredVelocity = distFromStart.normalized * MaxSpeed;
        }
        else
            desiredVelocity = playerDir.normalized * MaxSpeed;

        return (desiredVelocity - velocity) * MaxForce;
    }
}
