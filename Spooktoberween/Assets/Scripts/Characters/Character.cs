using UnityEngine;

public abstract class Character : MonoBehaviour
{
    public EFaceDirection currentFaceDirection {get; protected set;}

    public abstract Vector2 GetVelocity();
}
