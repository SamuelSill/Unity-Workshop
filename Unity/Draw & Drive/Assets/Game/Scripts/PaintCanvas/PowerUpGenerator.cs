using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PowerUpGenerator : NetworkBehaviour
{
    public int maxPowerUps = 5;

    [SerializeField]
    private List<GameObject> powerUpsList;
    [SerializeField]
    private float difficultyScale = 0.5f;
    private float boundsPadding = 0.2f;
    private float wait_time;
    private bool newWait;
    private Bounds textureBounds;
    public int powerUpCounter;

    public override void OnNetworkSpawn()
    {
        powerUpCounter = 0;
        textureBounds = GetComponent<Renderer>().bounds;
        newWait = true;
    }
    private void FixedUpdate()
    {
        if (IsServer)
        {
            waitToSpawn();
        }
    }
    private void waitToSpawn()
    {
        if (newWait)
        {
            wait_time = (Random.Range(10, 31) * difficultyScale);
            newWait = false;
        }
        if (wait_time > 0)
        {
            wait_time -= Time.deltaTime;
            return;
        }
        else
        {
            spawnPowerUpObject();
            newWait = true;
        }

    }
    private void spawnPowerUpObject() {
        if (powerUpCounter < maxPowerUps)
        {
            if (!IsServer)
            { return; }
            int powerUpIndex = Random.Range(0, powerUpsList.Count);
            GameObject chosenPowerUp = powerUpsList[powerUpIndex];
            Vector3 randomPosition = getRandomPossition();
            GameObject go = Instantiate(chosenPowerUp, randomPosition, Quaternion.identity);
            powerUpCounter++;
            go.GetComponent<PowerUp>().generator = this;
            go.GetComponent<NetworkObject>().Spawn();
        }
    }

    private Vector3 getRandomPossition() {
        Vector3 powerUpPosition = new Vector3(
                Random.Range(textureBounds.min.x + boundsPadding, textureBounds.max.x - boundsPadding),
                Random.Range(textureBounds.min.y + boundsPadding, textureBounds.max.y - boundsPadding), 0);
        return powerUpPosition;
    }

}
