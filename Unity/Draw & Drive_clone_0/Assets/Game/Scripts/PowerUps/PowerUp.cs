using System.Collections;
using Unity.Netcode;
using UnityEngine;

public abstract class PowerUp : NetworkBehaviour
{
    private NotificationMessage NotificationMessage;
   
    private float baseDuration;
    private string message;
    private Sprite sprite;
    public override void OnNetworkSpawn()
    {
        GameObject NotificationPanle = GameObject.FindWithTag("Notification Panle");
        NotificationMessage = NotificationPanle.GetComponent<NotificationMessage>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StartCoroutine(speedEffect(collision));
        }
    }
    
    protected abstract void PlayParticalEffect();
    protected abstract void ApplyPowerUpEffectOnPlayer(Collider2D player);
    protected abstract void RemovePowerUpEffectOnPlayer(Collider2D player);
    protected abstract float GetDuration();
    protected abstract string GetMessage();
    private Sprite GetSprite()
    {
        return transform.Find("Bubble").GetComponent<SpriteRenderer>().sprite;
    }
    private void SendNotification() {
        NotificationMessage.SetUp(message, baseDuration, sprite);
    }
    private void RemoveNotification()
    {
        NotificationMessage.Remove(message);
    }
    private IEnumerator speedEffect(Collider2D player)
    {
        baseDuration = GetDuration();
        message = GetMessage();
        sprite = GetSprite();

        PlayParticalEffect();

        ApplyPowerUpEffectOnPlayer(player);

        SendNotification();

        // Hide this object
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        // Hide all his childern
        Transform[] objectChildren = gameObject.GetComponentsInChildren<Transform>();
        for (int i = 0; i < objectChildren.Length; i++)
        {
            objectChildren[i].GetComponent<SpriteRenderer>().enabled = false;
        }

        yield return new WaitForSeconds(baseDuration);

        RemovePowerUpEffectOnPlayer(player);

        RemoveNotification();

        DespawnTimerServerRpc(gameObject.GetComponent<NetworkObject>());
    }
    [ServerRpc(RequireOwnership = false)]
    private void DespawnTimerServerRpc(NetworkObjectReference newTimer)
    {
        newTimer.TryGet(out NetworkObject newTimerNetworkObject);
        if (newTimerNetworkObject != null)
        {
            newTimerNetworkObject.Despawn();
        }

    }

}
