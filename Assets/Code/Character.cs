using UnityEngine;

public abstract class Character : MonoBehaviour
{
    public Map Map;
    public float MaxSpeed = 1.0f, MaxForce = 1.0f;

    float walkHeight = 0.0f, walkVelocity = 0.0f;

    public float Height { get; private set; }

    public Vector2 Position
    {
        get { return new Vector2(transform.position.x, transform.position.z); }
    }

    protected Vector2 velocity;

    protected abstract Vector2 TakeInput();

    static float GetHeight(Vector2 pos, TileInfo tile)
    {
        float x = pos.x % 1.0f, y = pos.y % 1.0f;
        switch (tile.Type)
        {
            default:
                return tile.Height;
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
        float targetHeight = GetHeight(new Vector2(x, y), Map.Get(Mathf.FloorToInt(x), Mathf.FloorToInt(y)));
        return targetHeight > currentHeight + 0.8f;
    }

    float UpdateCollision(ref Vector2 velocity, ref Vector2 pos)
    {
        int x = Mathf.FloorToInt(pos.x), y = Mathf.FloorToInt(pos.y);

        TileInfo tile = Map.Get(x, y);
        float height = GetHeight(pos, tile);

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
            velocity.x = pos.x - minX;
            pos.x = minX;
        }
        if (pos.x >= maxX)
        {
            velocity.x = pos.x - maxX;
            pos.x = maxX;
        }
        if (pos.y <= minY)
        {
            velocity.y = pos.y - minY;
            pos.y = minY;
        }
        if (pos.y >= maxY)
        {
            velocity.y = pos.y - maxY;
            pos.y = maxY;
        }

        return GetHeight(pos, tile) + 0.3f;
    }

    void FixedUpdate()
    {
        Vector2 position = new Vector3(transform.position.x, transform.position.z);
        Vector2 steering = Vector2.ClampMagnitude(TakeInput(), MaxForce);

        velocity += steering;
        velocity = Vector2.ClampMagnitude(velocity, MaxSpeed);
        position += velocity;

        float targetHeight = UpdateCollision(ref velocity, ref position);
        Height = targetHeight;

        //Add walk jumping animation to y height
        walkVelocity += -6.0f * Time.deltaTime;
        if (walkHeight <= targetHeight)
        {
            walkHeight = targetHeight;
            walkVelocity += (targetHeight - walkHeight);
            if (walkVelocity < 0.0f)
            {
                if (velocity.magnitude > 0.01f)
                    walkVelocity += 1.0f;
                else
                    walkVelocity = 0.0f;
            }
        }

        walkHeight += walkVelocity * Time.deltaTime;

        transform.position = new Vector3(position.x, walkHeight, position.y);

        //Calculate new rotation based upon velocity direction
        if (velocity.magnitude > 0.01f)
        {
            float targetAngle = Mathf.Atan2(velocity.y, -velocity.x) * Mathf.Rad2Deg;
            //Avoid 45 <-> 135 angles (to avoid not facing the camera)
            if (targetAngle > 45f && targetAngle < 135f)
                targetAngle = targetAngle > 90f ? 135f : 45f;
            else if (targetAngle < -45f && targetAngle > -135f)
                targetAngle = targetAngle > -90f ? -45f : -135f;

            transform.rotation = 
                Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.AngleAxis(targetAngle, Vector3.up),
                    0.2f
                );
        }
    }
}
