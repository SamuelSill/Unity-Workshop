using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class CarInputHandler : NetworkBehaviour
{
    TopDownCarController topDownCarController;
    Vector2 inputVector;
    public override void OnNetworkSpawn()
    {
        topDownCarController = GetComponent<TopDownCarController>();
        if (IsHost)
        {
            foreach (string userName in ServerSession.PlayerMobileControls.Keys)
            {
                NetworkManegerUI.AddUserToMobileUsers(userName);
            }
        }
        inputVector = Vector2.zero;
    }
    private float mobileTurnSpeedDecrease = 8f;
    // Update is called once per frame
    void Update()
    {
        //if (ServerSession.PlayerMobileControls.TryGetValue(ServerSession.Username, out Queue<ServerSession.MobileControls> mobileControllsQueue)) {
        if (IsHost && gameObject.name.Contains("Mobile")) {
            string name = NetworkManegerUI.GetCurrentMobileUser();
            //Debug.Log("mobileControls user name : " + name);//+ ", number of users : " + NetworkManegerUI.mobileUserUsername.Count);
            if (ServerSession.PlayerMobileControls.TryGetValue(name, out Queue<ServerSession.MobileControls> mobileControllsQueue))
            {
                if (mobileControllsQueue.TryDequeue(out ServerSession.MobileControls mobileControls))
                {

                    //Debug.Log("mobileControls Z: " + mobileControls.gyro.z);
                    //Debug.Log("mobileControls drive: " + mobileControls.drive);
                    if (mobileControls.drive.Equals("forward"))
                    {
                        inputVector.y = 1;
                        inputVector.x = mobileControls.direction;
                    }
                    else if (mobileControls.drive.Equals("reverse"))
                    {
                        inputVector.y = -1;
                        inputVector.x = -mobileControls.direction;
                    }
                    else {
                        inputVector.y = 0;
                        inputVector.x = mobileControls.direction;
                    }

                    if (TimerStarter.GameStarted) 
                    {
                        topDownCarController.SetInputVector(inputVector);
                    }
                    
                    //Debug.Log("mobile input: " + inputVector);
                }
            }
            return;
        }

        if (!IsOwner || !TimerStarter.GameStarted) {
            return;
        }
        inputVector = Vector2.zero;
        inputVector.x = Input.GetAxis("Horizontal");
        inputVector.y = Input.GetAxis("Vertical");
        if (inputVector.y < 0)
        {
            inputVector.x = -inputVector.x;
        }
        //Debug.Log("normal user input : "+ inputVector);
        topDownCarController.SetInputVector(inputVector);
    }
}
