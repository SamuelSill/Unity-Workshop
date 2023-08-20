using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlayerOptions : NetworkBehaviour
{
    public string UserName {get; set;}
    static Dictionary<ulong, int> playerLeftLocations;
    static Dictionary<ulong, int> playerRightLocations;

    [SerializeField] private List<Vector3> spawnPositionList;
    private PlayerCustomisation playerCustomisation;

    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            if (!gameObject.name.Contains("Mobile") && IsOwner)
            {
                playerLeftLocations = new Dictionary<ulong, int>();
                playerRightLocations = new Dictionary<ulong, int>();
            }

            if (gameObject.name.Contains("Mobile"))
            {
                GenerateLocationServerRpc(NetworkObjectId, ServerSession.UserTeam(UserName));
            }
            else if (IsOwner)
            {
                GenerateLocationServerRpc(NetworkObjectId, ServerSession.CurrentTeam);
            }
        }
        else
        {
            if (IsOwner)
            {
                GenerateLocationServerRpc(NetworkObjectId, ServerSession.CurrentTeam);
            }
            else
            {
                GetLocationServerRpc(NetworkObjectId);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void GenerateLocationServerRpc(ulong objectNetId, string carTeam)
    {
        var playerLocations = carTeam.Equals("left") ? playerLeftLocations : playerRightLocations;
        var matchingStartIndex = carTeam.Equals("left") ? 0 : 3;

        playerLocations.Add(objectNetId, playerLocations.Count);
        int index = matchingStartIndex + playerLocations[objectNetId];

        // Update position
        transform.position = spawnPositionList[index];
        ChangePlayerPositionClientRpc(index);

        // Update color
        ChangePlayerColorClientRpc(index);
    }

    [ServerRpc(RequireOwnership = false)]
    void GetLocationServerRpc(ulong objectNetId)
    {
        string carTeam = playerLeftLocations.ContainsKey(objectNetId) ? "left" : "right";
        var playerLocations = carTeam.Equals("left") ? playerLeftLocations : playerRightLocations;
        var matchingStartIndex = carTeam.Equals("left") ? 0 : 3;

        int index = matchingStartIndex + playerLocations[objectNetId];

        // Update position
        transform.position = spawnPositionList[index];
        ChangePlayerPositionClientRpc(index);

        // Update color
        ChangePlayerColorClientRpc(index);
    }

    [ClientRpc]
    private void ChangePlayerColorClientRpc(int index)
    {
        playerCustomisation = GetComponent<PlayerCustomisation>();
        playerCustomisation.currentColor = (CarColor)(index % 3);
    }

    [ClientRpc]
    private void ChangePlayerPositionClientRpc(int index)
    {
        transform.position = spawnPositionList[index];
    }
}