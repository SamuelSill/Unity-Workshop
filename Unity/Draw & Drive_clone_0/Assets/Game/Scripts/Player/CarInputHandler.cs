using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class CarInputHandler : NetworkBehaviour
{
    TopDownCarController topDownCarController;

    public override void OnNetworkSpawn()
    {
        topDownCarController = GetComponent<TopDownCarController>();
    }

    // Update is called once per frame
    void Update()
    {

        if (!IsOwner)
            return;

        Vector2 inputVector = Vector2.zero;

        inputVector.x = Input.GetAxis("Horizontal");
        inputVector.y = Input.GetAxis("Vertical");

        topDownCarController.SetInputVector(inputVector);
    }
}
