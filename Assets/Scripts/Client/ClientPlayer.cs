using UnityEngine;

public struct TickedPlayerState{
    public int Tick;
    public PlayerState playerState;
}

public class ClientPlayer : MonoBehaviour
{
    CharacterController2D characterController;
    public Vector3 MovementVelocity { private get; set; } = Vector3.zero;
    public Vector3 RotationVelocity;

    public Vector3 PrvsPosition;
    public Quaternion PrvsRotation;

    void Awake()
    {
        characterController = GetComponent<CharacterController2D>();
        PrvsPosition = transform.position;
        PrvsRotation = transform.rotation;
    }

    public void Simulate(float deltaTime)
    {
        characterController.move(MovementVelocity * deltaTime);
        transform.Rotate(RotationVelocity * deltaTime);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public Quaternion GetRotation()
    {
        return transform.rotation;
    }

    public void ApplyPosition(Vector3 position)
    {
        PrvsPosition = position;
        transform.position = position;
    }

    public void ApplyRotation(Quaternion rotation)
    {
        PrvsRotation = rotation;
        transform.rotation = rotation;
    }

    public void Reset()
    {
        PrvsPosition = transform.position;
        PrvsRotation = transform.rotation;
    }

    public void Destroy()
    {
        GameObject.Destroy(gameObject);
    }
}
