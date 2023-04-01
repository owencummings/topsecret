using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class GameManager : NetworkBehaviour
{

    // A class for spawning & maintaining the higher-level objects for a session

    public GameObject gridManagerPrefab;
    public GameObject playerPrefab;
    public GameObject planePrefab;
    public GameObject playerCinemachine;

    [ServerRpc(RequireOwnership = false)]
    // TODO: make game  pre-spawn player prefabs, and then change ownership based on clientID upon connecting
    void SpawnPlayerServerRpc(ServerRpcParams serverRpcParams = default){
        if (IsServer){
            // For continuity, should assume control/reconnect existing characer
            GameObject player = Instantiate(playerPrefab, new Vector3(0, 20, 0), Quaternion.identity);
            player.name = "Player" + serverRpcParams.Receive.SenderClientId.ToString();
            player.GetComponent<NetworkObject>().SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);
        }
    }

    public override void OnNetworkSpawn()
    {
        Physics.gravity = new Vector3(0, -20.0f, 0);
        Instantiate(planePrefab, Vector3.zero, Quaternion.identity);
        if (IsServer){
            //GameObject gridManager = Instantiate(gridManagerPrefab, Vector3.zero, Quaternion.identity);
            //gridManager.GetComponent<NetworkObject>().Spawn();
        }

        if (IsClient){
            // For continuity, should assume control/reconnect existing characer
            if(IsServer){
                GameObject spawnedPlayer = Instantiate(playerPrefab, new Vector3(0, 20, 0), Quaternion.identity);
                spawnedPlayer.name = "Player" + NetworkManager.Singleton.LocalClientId.ToString();
                spawnedPlayer.GetComponent<NetworkObject>().Spawn();
                playerCinemachine.GetComponent<PlayerFollow>().SetTarget(spawnedPlayer);
            } else {
                SpawnPlayerServerRpc();
            }
        }
    }

}
