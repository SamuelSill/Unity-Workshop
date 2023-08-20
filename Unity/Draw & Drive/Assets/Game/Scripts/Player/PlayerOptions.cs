using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlayerOptions : NetworkBehaviour
{
    public string UserName {get; set;}

    private int highlightBaseDuration = 10;
    private bool decending = true;
    int maxHighlightValue = 255;
    int minxHighlightValue = 100;
    int gapValue = 10;
    SpriteRenderer highlightAura;

    ulong OwnerId;
    private ulong allyMobileID = 2;
    private ulong enemyMobileID = 2;

    [SerializeField] private List<Vector3> spawnPositionList;
    private PlayerCustomisation playerCustomisation;
    public static int PositionNetworkSpawned { get; private set; }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            PositionNetworkSpawned = 0;

        }
        //transform.position = spawnPositionList[((int)OwnerClientId) % spawnPositionList.Count];
        OwnerId = OwnerClientId;
        if (IsOwner)
        {
            int isRight = 0;
            if (ServerSession.CurrentTeam.Equals("right"))
            {
                isRight = 1;
            }

            if (gameObject.name.Contains("Mobile"))
            {
                if (ServerSession.EnemyLobbyPlayers.ContainsKey(UserName))
                {
                    Debug.Log("mobile enemy");
                    OwnerId = enemyMobileID;
                    enemyMobileID--;

                    // On enemy canvas (not the host canvas)
                    isRight = (isRight + 1) % 2;
                }
                else
                {
                    Debug.Log("mobile ally");
                    OwnerId = allyMobileID;
                    allyMobileID--;
                }


            }
            changePlayerPositionServerRpc((int)OwnerId % 3 + 3 * isRight);
        }
        else 
        { 
        
        }
        playerCustomisation = GetComponent<PlayerCustomisation>();
        playerCustomisation.currentColor = (CarColor)(OwnerId % 3);

        if (IsOwner)
        {
            Transform highlightAuraTrans = transform.Find("Hightlight");
            if (highlightAuraTrans != null)
            {
                highlightAura = highlightAuraTrans.GetComponent<SpriteRenderer>();
                highlightAura.gameObject.SetActive(true);
                RunHighlightAffect();
            }
            
        }
    }
    private void Start()
    {
        //Debug.Log("PlayerOptions start");
        if (gameObject.name.Contains("Mobile"))
        {
            //Debug.Log("players enemys: "+ ServerSession.EnemyLobbyPlayers.Count);
            //Debug.Log("players allys: " + ServerSession.LobbyPlayers.Count);
           // if (ServerSession.EnemyLobbyPlayers.TryGetValue(ServerSession.Username, out ServerSession.UserGameStats enemyUser))
            //{
           //     Debug.Log("enemy users: " + enemyUser.username);
           // }
           // if (ServerSession.LobbyPlayers.TryGetValue(ServerSession.Username, out ServerSession.UserGameStats FriendlyUser))
           // {
            //    Debug.Log("Friendly user: " + FriendlyUser.username);
           // }

            int ownerID = 1;
            int isRight = 0;
            if (ServerSession.CurrentTeam.Equals("right"))
            {
                isRight = 1;
            }
            int index = ownerID + (3 * isRight);
            GetMobilePlayerColorServerRpc();
            //changePlayerPositionClientRpc(index);
            //transform.position = spawnPositionList[index % spawnPositionList.Count];
            //PositionNetworkSpawned++;
            //playerCustomisation = GetComponent<PlayerCustomisation>();
            //playerCustomisation.currentColor = (CarColor)(ownerID % 3);
        }

    }

    [ServerRpc(RequireOwnership = false)]
    private void GetMobilePlayerColorServerRpc()
    {
        GetMobilePlayerColorClientRpc(OwnerId);
    }
    [ClientRpc]
    private void GetMobilePlayerColorClientRpc(ulong OwnerId)
    {
        playerCustomisation = GetComponent<PlayerCustomisation>();
        playerCustomisation.currentColor = (CarColor)(OwnerId % 3);
    }

    [ServerRpc(RequireOwnership = false)]
    private void changePlayerPositionServerRpc(int index)
    {
        transform.position = spawnPositionList[index % spawnPositionList.Count];
        changePlayerPositionClientRpc(index);
        PositionNetworkSpawned++;
        Debug.Log("Spawned " + PositionNetworkSpawned);
    }
    [ClientRpc]
    private void changePlayerPositionClientRpc(int index)
    {
        transform.position = spawnPositionList[index % spawnPositionList.Count];
        PositionNetworkSpawned++;
        Debug.Log("Spawned " + PositionNetworkSpawned);
    }
    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
    private IEnumerator RunHighlightAffect()
    {
        if (highlightAura != null)
        {
            if (decending)
            {
                Color newColor = highlightAura.color;
                newColor.b -= gapValue;
                newColor.r -= gapValue;
                newColor.g -= gapValue;
                highlightAura.color = newColor;
                if (highlightAura.color.b <= minxHighlightValue)
                {
                    decending = false;
                }
            }
            else {
                Color newColor = highlightAura.color;
                newColor.b += gapValue;
                highlightAura.color = newColor;
                if (highlightAura.color.b >= maxHighlightValue)
                {
                    decending = true;
                }
            }
        }

        yield return new WaitForSeconds(highlightBaseDuration);
        if (highlightAura != null)
        {
            highlightAura.gameObject.SetActive(false);
        }
    }
}