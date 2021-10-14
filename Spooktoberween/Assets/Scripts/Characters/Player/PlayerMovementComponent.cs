using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementComponent : MonoBehaviour
{
    public CharacterController characterController {get; private set;}

    public PlayerMovementParams movementParams = new PlayerMovementParams() {movementSpeed = 3f, movementAcceleration = 7f, brakingAcceleration = 10f};

    public Vector2 velocity {get; protected set;}

    Vector2 movementInput;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        UpdateVelocity(deltaTime);
        UpdateLocation(deltaTime);
    }

    void UpdateVelocity(float deltaTime)
    {
        Vector2 goalVeclocity = movementInput * movementParams.movementSpeed;
        float goalSpeed = goalVeclocity.magnitude;
        Vector2 goalDirection = goalVeclocity / (goalSpeed + Mathf.Epsilon);

        float parallelSpeed = Vector2.Dot(velocity, goalDirection);

        Vector2 perpendicularVelocity = velocity - (parallelSpeed * goalDirection);
        float perpendicularSpeed = perpendicularVelocity.magnitude;
        Vector2 perpendicularDirection = perpendicularVelocity / (perpendicularSpeed + Mathf.Epsilon);

        float parallelDeltaSpeed = 0f;
        if(parallelSpeed > goalSpeed)
        {
            float speedDiff = goalSpeed - parallelSpeed;
            parallelDeltaSpeed = Mathf.Max(speedDiff, -movementParams.brakingAcceleration * deltaTime);
        }
        else if(parallelSpeed < goalSpeed)
        {
            float speedDiff = goalSpeed - parallelSpeed;
            parallelDeltaSpeed = Mathf.Min(speedDiff, (parallelSpeed < 0f ? movementParams.brakingAcceleration : movementParams.movementAcceleration) * deltaTime);
        }

        parallelSpeed += parallelDeltaSpeed;

        if(perpendicularSpeed > 0f)
        {
            perpendicularSpeed = Mathf.Max(0f, perpendicularSpeed - movementParams.brakingAcceleration * deltaTime);
        }

        velocity = goalDirection * parallelSpeed + perpendicularDirection * perpendicularSpeed;
    }

    void UpdateLocation(float deltaTime)
    {
        Vector2 locationDelta = velocity * deltaTime;
        characterController.Move(new Vector3(locationDelta.x, 0f, locationDelta.y));
    }

    public void SetMovmentInput(Vector2 newMovementInput)
    {
        movementInput = newMovementInput.sqrMagnitude > 1f ? newMovementInput.normalized : newMovementInput;
    }

    [System.Serializable]
    public struct PlayerMovementParams
    {
        public float movementSpeed;
        public float movementAcceleration;
        public float brakingAcceleration;
    }
}
