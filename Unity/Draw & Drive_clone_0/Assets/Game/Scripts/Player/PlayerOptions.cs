using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerOptions : NetworkBehaviour
{
    [SerializeField] private List<Vector3> spawnPositionList;
    private PlayerCustomisation playerCustomisation;
    public static int PositionNetworkSpawned { get; private set; }
    
    public override void OnNetworkSpawn()
    {
        if (IsServer) {
            PositionNetworkSpawned = 0;
        }
        //transform.position = spawnPositionList[((int)OwnerClientId) % spawnPositionList.Count];
        if (IsOwner) {
            int isRight = 0;
            if (ServerSession.CurrentTeam.Equals("right")) {
                isRight = 1;
            }
            changePlayerPositionServerRpc((int)OwnerClientId % 3 + 3 * isRight);
        }
        playerCustomisation = GetComponent<PlayerCustomisation>();
        playerCustomisation.currentColor = (PlayerCustomisation.moduleColors)(OwnerClientId % 3);
        
    }
    [ServerRpc(RequireOwnership = false)]
    private void changePlayerPositionServerRpc(int index) {
        transform.position = spawnPositionList[index % spawnPositionList.Count];
        changePlayerPositionClientRpc(index);
        PositionNetworkSpawned++;
    }
    [ClientRpc]
    private void changePlayerPositionClientRpc(int index)
    {
        transform.position = spawnPositionList[index % spawnPositionList.Count];
        PositionNetworkSpawned++;
    }
    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}
