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
        Vector2 inputVector = Vector2.zero;
        //if (ServerSession.PlayerMobileControls.TryGetValue(ServerSession.Username, out Queue<ServerSession.MobileControls> mobileControllsQueue)) {
        if (IsHost && gameObject.name.Contains("Mobile")) {
            
            string name = NetworkManegerUI.GetCurrentMobileUser();
            Debug.Log("mobileControls user name : " + name);//+ ", number of users : " + NetworkManegerUI.mobileUserUsername.Count);
            if (ServerSession.PlayerMobileControls.TryGetValue(name, out Queue<ServerSession.MobileControls> mobileControllsQueue))
            {
                if (mobileControllsQueue.TryDequeue(out ServerSession.MobileControls mobileControls))
                {
                    
                    Debug.Log("mobileControls Z: " + mobileControls.gyro.z);
                    /*
                    if (mobileControls.drive.Equals("forword")) {
                        inputVector.x = 1;
                    }
                    else if (mobileControls.drive.Equals("backwords"))
                    {
                        inputVector.x = -1;
                    }
                    float steerValue = mobileControls.gyro.z % 10;
                    float minValue = -10;
                    float maxValue = 10;
                    inputVector.y = Mathf.Round(((2 * (steerValue - minValue)) / (maxValue - minValue)) - 1);
                    topDownCarController.SetInputVector(inputVector);
                    */
                }
            }
            return;
        }

        if (!IsOwner || !TimerStarter.GameStarted)
            return;

        inputVector.x = Input.GetAxis("Horizontal");
        inputVector.y = Input.GetAxis("Vertical");

        topDownCarController.SetInputVector(inputVector);
    }
}
