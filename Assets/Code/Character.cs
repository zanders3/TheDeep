using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public abstract class Character : MonoBehaviour
{
    public System.Action<Character> OnCollision = null;

    public const float Radius = 0.4f;

    public GameManager GameManager;
    public float MaxSpeed = 1.0f, MaxForce = 1.0f;

    float height, heightVelocity = 0.0f;
    Quaternion targetRotation = Quaternion.identity;
    bool hadContact = false;
    
    protected Vector2 velocity;
    Vector2 impulse = Vector2.zero;

    public Vector2 Position
    {
        get { return new Vector2(transform.position.x, transform.position.z); }
    }

    protected abstract Vector2 TakeInput();

    protected virtual void Start()
    {
        SetPosition(transform.position.x, transform.position.z);
    }

    public virtual void Damage(Character sender) {}

    protected void SetPosition(float x, float y)
    {
        height = GetHeight(new Vector2(x, y), GameManager.Map.Get(Mathf.FloorToInt(x), Mathf.FloorToInt(y)));
        heightVelocity = 0.0f;
        hadContact = false;

        targetRotation = Quaternion.identity;
        transform.rotation = Quaternion.identity;

        transform.position = new Vector3(x, height, y);
        velocity = Vector2.zero;
    }

    static float GetHeight(Vector2 pos, TileInfo tile)
    {
        float x = pos.x % 1.0f, y = pos.y % 1.0f;
        switch (tile.Type)
        {
            default:
                return tile.Height == 0.0f ? -100.0f : tile.Height;
            case Tile.RampW:
                return (tile.Height-1) + x;
            case Tile.RampE:
                return (tile.Height-1) + (1.0f-x);
            case Tile.RampN:
                return (tile.Height-1) + y;
            case Tile.RampS:
                return (tile.Height-1) + (1.0f-y);
        }
    }

    bool CannotTraverse(float currentHeight, float x, float y)
    {
        float targetHeight = GetHeight(new Vector2(x, y), GameManager.Map.Get(Mathf.FloorToInt(x), Mathf.FloorToInt(y)));
        return targetHeight > currentHeight + 0.5f;
    }

    float UpdateCollision(ref Vector2 velocity, ref Vector2 pos)
    {
        int x = Mathf.FloorToInt(pos.x), y = Mathf.FloorToInt(pos.y);

        TileInfo tile = GameManager.Map.Get(x, y);

        bool l = CannotTraverse(height, pos.x-0.5f, pos.y);
        bool r = CannotTraverse(height, pos.x+0.5f, pos.y);
        bool d = CannotTraverse(height, pos.x, pos.y+0.5f);
        bool u = CannotTraverse(height, pos.x, pos.y-0.5f);

        float minX = x + (l ? 0.2f : -1.0f);
        float maxX = x + (r ? 0.8f :  2.0f);
        float minY = y + (u ? 0.2f : -1.0f);
        float maxY = y + (d ? 0.8f :  2.0f);

        if (pos.x <= minX)
        {
            velocity.x -= pos.x - minX;
        }
        if (pos.x >= maxX)
        {
            velocity.x -= pos.x - maxX;
        }
        if (pos.y <= minY)
        {
            velocity.y -= pos.y - minY;
        }
        if (pos.y >= maxY)
        {
            velocity.y -= pos.y - maxY;
        }

        return GetHeight(pos, tile) + 0.3f;
    }

    public void AddImpulse(Vector2 impulse)
    {
        this.impulse += impulse;
    }

    public void DoUpdate()
    {
        //Calculate position and steering
        Vector2 steering = Vector2.ClampMagnitude(TakeInput(), MaxForce);
        Vector2 position = new Vector3(transform.position.x, transform.position.z);

        velocity += steering;
        velocity = Vector2.ClampMagnitude(velocity, MaxSpeed);

        float targetHeight = UpdateCollision(ref velocity, ref position);
        velocity += impulse;
        impulse = Vector2.zero;
        position += velocity;

        //Apply gravity
        heightVelocity += -9.0f * Time.fixedDeltaTime;

        //Apply fallen off edge jump force
        if (hadContact && height > targetHeight + 0.8f)
        {
            heightVelocity += 3.0f;
            hadContact = false;
        }

        //Apply walking jump force
        float p = targetHeight - height;
        if (Mathf.Abs(p) < 0.01f && heightVelocity <= 0.0f && velocity.magnitude > 0.01f)
        {
            heightVelocity += 1.0f;
        }

        //Apply surface contact force and impulse
        if (p > 0.0f)
        {
            if (heightVelocity < 0.0f)
                heightVelocity = 0.0f;
            height += p * 0.2f;

            if (Mathf.Abs(p) < 0.01f)
                hadContact = true;
        }

        //Apply height velocity
        height += heightVelocity * Time.fixedDeltaTime;

        transform.position = new Vector3(position.x, height, position.y);

        //Calculate new rotation based upon velocity direction
        if (velocity.magnitude > 0.01f)
        {
            float targetAngle = Mathf.Atan2(velocity.y, -velocity.x) * Mathf.Rad2Deg;
            //Avoid 45 <-> 135 angles (to avoid not facing the camera)
            if (targetAngle > 45f && targetAngle < 135f)
                targetAngle = targetAngle > 80f ? 135f : 45f;
            else if (targetAngle < -45f && targetAngle > -135f)
                targetAngle = targetAngle > -80f ? -45f : -135f;

            targetRotation = Quaternion.AngleAxis(targetAngle, Vector3.up);
        }

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            0.2f
        );
    }
}
