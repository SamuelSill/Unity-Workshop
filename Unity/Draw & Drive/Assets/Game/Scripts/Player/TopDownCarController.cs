using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class TopDownCarController : NetworkBehaviour
{
    [Header("Car settings")]
    public float driftFactor = 0.95f;
    public float accelerationFactor = 3.0f;
    public float turnFactor = 5.0f;
    public float maxSpeed = 3;

    private float accelarationInput = 0;
    private float steeringInput = 0;

    private float rotationAngle = 0;

    private float velocityVsUp = 0;

    private Rigidbody2D carRigidbody2D;

    public override void OnNetworkSpawn()
    {
        carRigidbody2D = GetComponent<Rigidbody2D>();

        if (IsOwner)
        {
            if (gameObject.name.Contains("Mobile"))
            {
                var username = gameObject.GetComponent<PlayerOptions>().UserName;
                var player =
                    ServerSession.LobbyPlayers.ContainsKey(username) ?
                    ServerSession.LobbyPlayers[username] :
                    ServerSession.EnemyLobbyPlayers[username];

                var matchingPlayerCar = ServerSession.GameCars.Find(car => car.id.Equals(player.selected_car.id));
                maxSpeed = matchingPlayerCar.speed + player.selected_car.upgrades.speed;
                accelerationFactor = maxSpeed / 2.0f;
                turnFactor = matchingPlayerCar.steering + player.selected_car.upgrades.steering;
            }
            else
            {
                maxSpeed = ServerSession.CurrentCarSpeed;
                accelerationFactor = maxSpeed / 2.0f;
                turnFactor = ServerSession.CurrentCarSteering;
            }
        }
    }
    void FixedUpdate()
    {
        if (!IsOwner)
            return;

        HandleMovmentServerAuth();
    }

    private void HandleMovmentServerAuth() 
    {
        HandleMovmentServerRpc(steeringInput, accelarationInput);
    }

    [ServerRpc(RequireOwnership = false)]
    private void HandleMovmentServerRpc(float steeringInput, float accelarationInput) 
    {
        this.steeringInput = steeringInput;
        this.accelarationInput = accelarationInput;

        ApplyEngineForce();

        killOrthogonalVelocity();

        ApplySteering();
    }

    private void ApplyEngineForce() 
    {
        //Calculate how much "forward" we are going in terms of the direction of our velocity
        velocityVsUp = Vector2.Dot(transform.up, carRigidbody2D.velocity);

        //Limit so we cannot go faster then the max speed in the "forward" direction
        if (velocityVsUp > maxSpeed && accelarationInput > 0)
            return;

        //Limit so we cannot go faster then 50% of the max speed in the "reverse" direction
        if (velocityVsUp < -maxSpeed * 0.5f && accelarationInput < 0)
            return;

        //Limit so we cannot go faster in any direction while accelerating
        if (carRigidbody2D.velocity.sqrMagnitude > maxSpeed * maxSpeed && accelarationInput > 0)
            return;


        //Apply drag if there is no accelerations so the car stops when the player lets go of the accelerator
        if (accelarationInput == 0)
            carRigidbody2D.drag = Mathf.Lerp(carRigidbody2D.drag, 3.0f, Time.fixedDeltaTime * 3);
        else 
            carRigidbody2D.drag = 0;

        //Create force for the engine
        Vector2 engineForceVector = transform.up * accelarationInput * accelerationFactor;

        //Apply force and pushes the car forward
        carRigidbody2D.AddForce(engineForceVector, ForceMode2D.Force);

    }
    private void ApplySteering() 
    {
        //Limit the cars ability to turn when moving slowly
        float minSpeedBeforAllowTurningFactor = (carRigidbody2D.velocity.magnitude / 8);
        minSpeedBeforAllowTurningFactor = Mathf.Clamp01(minSpeedBeforAllowTurningFactor);

        //Update the rotation angle based on input
        rotationAngle -= steeringInput * turnFactor * minSpeedBeforAllowTurningFactor;

        //Apply steering bu rotating the car object
        carRigidbody2D.MoveRotation(rotationAngle);
    }

    private void killOrthogonalVelocity() 
    {
        Vector2 forwardVelocity = transform.up * Vector2.Dot(carRigidbody2D.velocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(carRigidbody2D.velocity, transform.right);

        //Limit car drift
        carRigidbody2D.velocity = forwardVelocity + rightVelocity * driftFactor;

    }

    public void SetInputVector(Vector2 inputVector) 
    {
        steeringInput = inputVector.x;
        accelarationInput = inputVector.y;
    }
}
