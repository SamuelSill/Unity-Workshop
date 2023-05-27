using Unity.Netcode;
using UnityEngine;

public class PUChangeColor : PowerUp
{
    [SerializeField]
    private GameObject changeColorPickUpEffect;
    [SerializeField]
    private float duration = 10f;

    private PlayerCustomisation.moduleColors oldColor;

    [ClientRpc]
    private void setRandonColorClientRpc(int randomNumber, NetworkObjectReference customisationReference) {
        PlayerCustomisation.moduleColors color1, color2;

        if (!customisationReference.TryGet(out NetworkObject customisationObject))
        {
            return;
        }
        PlayerCustomisation controllerSettings = customisationObject.GetComponent<PlayerCustomisation>();
        oldColor = controllerSettings.currentColor;
        if (oldColor == PlayerCustomisation.moduleColors.blue)
        {
            color1 = PlayerCustomisation.moduleColors.red;
            color2 = PlayerCustomisation.moduleColors.green;
        }
        else if (oldColor == PlayerCustomisation.moduleColors.red)
        {
            color1 = PlayerCustomisation.moduleColors.blue;
            color2 = PlayerCustomisation.moduleColors.green;
        }
        else
        {
            color1 = PlayerCustomisation.moduleColors.red;
            color2 = PlayerCustomisation.moduleColors.blue;
        }
        if (randomNumber == 1)
        {
            controllerSettings.currentColor = color1;
        }
        else
        {
            controllerSettings.currentColor = color2;
        }
    }

    private void setToDiffrentColor(NetworkObject playerCustomisation)
    {
        if (IsServer)
        {         
            int randomNumber = Random.Range(1, 3);
            setRandonColorClientRpc(randomNumber, playerCustomisation);
        }
    }
    [ClientRpc]
    private void ReturnToOldColorClientRpc(PlayerCustomisation.moduleColors old, NetworkObjectReference objectThatChanged) {
        objectThatChanged.TryGet(out NetworkObject newTimerNetworkObject);
        if (newTimerNetworkObject != null)
        {
            //Debug.Log("ganna chage color");
            newTimerNetworkObject.GetComponent<PlayerCustomisation>().currentColor = old;
        }
    }
    private void ReturnToOldColor(PlayerCustomisation playerCustomisationSetttings)
    {
        //Debug.Log("ganna chage color");
        playerCustomisationSetttings.currentColor = oldColor;
        ReturnToOldColorClientRpc(oldColor, playerCustomisationSetttings.GetComponent<NetworkObject>());
    }
    protected override float GetDuration()
    {
        return duration;
    }
    protected override void PlayParticalEffect()
    {
        Instantiate(changeColorPickUpEffect, transform.position, transform.rotation);
    }
    protected override void ApplyPowerUpEffectOnPlayer(Collider2D player)
    {
        PlayerCustomisation controllerSettings = player.GetComponent<PlayerCustomisation>();
        setToDiffrentColor(controllerSettings.GetComponent<NetworkObject>());
    }
    protected override void RemovePowerUpEffectOnPlayer(Collider2D player)
    {
        PlayerCustomisation controllerSettings = player.GetComponent<PlayerCustomisation>();
        ReturnToOldColor(controllerSettings);
    }
    protected override string GetMessage()
    {
        return "Color Change";
    }
}
