using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;

public class NotificationMessage : NetworkBehaviour
{
    [SerializeField]
    private float powerUpTimerSpacing = 15;
    [SerializeField]
    private GameObject powerUpTimerPrefab;
    private NetworkObject newPowerUpTimer;
    private LinkedList<NetworkObject> DespawnList;
    private TextMeshProUGUI textMeshPro;
    private static float currentSpacing = 0;
    public override void OnNetworkSpawn()
    {
        textMeshPro = transform.Find("NotificationMessageLeft").GetComponent<TextMeshProUGUI>();
        if (IsServer)
        {
            DespawnList = new LinkedList<NetworkObject>();
        }
    }
    public void SetUp(string text) {
        string originalText = textMeshPro.text;
        string newText = originalText + text + "\n";
        textMeshPro.SetText(newText);
    }
    public void SetUp(string text, float duration, Sprite powerUpSprite)
    {
        SpawnPowerUpTimer();
        AddVariablesToTimer(duration, powerUpSprite);
        string originalText = textMeshPro.text;
        string newText = originalText + text + "\n";
        textMeshPro.SetText(newText);
        
    }
    [ClientRpc]
    private void UpdateClientTimeClientRpc(int duration) {

        newPowerUpTimer.GetComponent<Timer>().seconds = (int)duration;
    }
    private void AddVariablesToTimer(float duration, Sprite powerUpSprite)
    {
        Image[] timerImage = newPowerUpTimer.GetComponentsInChildren<Image>();
        Timer timer = newPowerUpTimer.GetComponent<Timer>();

        timer.seconds = (int)duration;

        foreach (Image image in timerImage)
        {
            image.sprite = powerUpSprite;
        }
        UpdateClientTimeClientRpc((int)duration);
    }
    [ServerRpc(RequireOwnership = false)]
    private void SpawnTimerServerRpc() {
        GameObject newPowerUpTimerObject = Instantiate(powerUpTimerPrefab, Vector3.zero, Quaternion.identity);
        newPowerUpTimer = newPowerUpTimerObject.GetComponent<NetworkObject>();
        newPowerUpTimer.Spawn();
        DespawnList.AddLast(newPowerUpTimer);
        GetSpawnTimerClientRpc(newPowerUpTimer);
    }
    [ClientRpc]
    private void GetSpawnTimerClientRpc(NetworkObjectReference newTimer)
    {

        newTimer.TryGet(out NetworkObject newTimerNetworkObject);
        if (newTimerNetworkObject != null)
        {
            newPowerUpTimer = newTimerNetworkObject;
        }
    }
    private void SpawnPowerUpTimer()
    {
        if (IsServer)
        {
            SpawnTimerServerRpc();
        }
        Vector3 objectWorldPossition = powerUpTimerPrefab.transform.position;
        objectWorldPossition.y -= currentSpacing;
        currentSpacing += powerUpTimerSpacing;

        newPowerUpTimer.transform.SetParent(transform, false);
        newPowerUpTimer.transform.localPosition = objectWorldPossition;

    }
    [ClientRpc]
    private void MoveUpTimersClientRpc(NetworkObjectReference newTimer) {
        newTimer.TryGet(out NetworkObject newTimerNetworkObject);
        if (newTimerNetworkObject != null)
        {
            newTimerNetworkObject.transform.position += new Vector3(0, powerUpTimerSpacing);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void DespawnTimerServerRpc(NetworkObjectReference newTimer)
    {
        DespawnList.RemoveFirst();
        newTimer.TryGet(out NetworkObject newTimerNetworkObject);
        if (newTimerNetworkObject != null)
        {
            newTimerNetworkObject.Despawn();
        }
        
        if (DespawnList.Count != 0) {
            foreach (NetworkObject Timer in DespawnList) {
                Timer.transform.position += new Vector3(0, powerUpTimerSpacing);
                if (Timer.IsSpawned)
                {
                    MoveUpTimersClientRpc(Timer);
                }
            }
        }
    }
    private void DespawnPowerUpTimer() {
            
            
        if (IsServer)
        {
            DespawnTimerServerRpc(DespawnList.First.Value); 
        }
    }
    [ClientRpc]
    private void UpdateRemovedDataClientRpc(string text)
    {
        currentSpacing -= powerUpTimerSpacing;
        textMeshPro.SetText(text);
    }
    public void Remove(string textToRemove)
    {

        string[] getLines = textMeshPro.text.Split('\n');
        List<string> lines = new(getLines);

        lines.Remove(textToRemove);

        string updatedText = string.Join("\n", lines);

        textMeshPro.SetText(updatedText);
        UpdateRemovedDataClientRpc(updatedText);
        lines.Clear();

        
        DespawnPowerUpTimer();
    }
}
