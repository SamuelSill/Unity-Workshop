using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerOptions : NetworkBehaviour
{
    [SerializeField] private List<Vector3> spawnPositionList;
    private PlayerCustomisation playerCustomisation;
    public override void OnNetworkSpawn()
    {

        transform.position = spawnPositionList[((int)OwnerClientId) % spawnPositionList.Count];
        playerCustomisation = GetComponent<PlayerCustomisation>();
        playerCustomisation.currentColor = (PlayerCustomisation.moduleColors)(OwnerClientId % 3);
    }
    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}
