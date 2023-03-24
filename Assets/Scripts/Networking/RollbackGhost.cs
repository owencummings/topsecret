using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomNetcode;
using UnityEngine.SceneManagement;

public class RollbackGhost : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private PlayerState playerState;
    public GameObject ghost;
    private IStateful ogStateful;
    private IStateful ghostStateful;

    void CreateGhost(){
        GameObject ghost = new GameObject("Ghost");

        if (rb) {
            ghost.AddComponent<Rigidbody>(rb);
        }
        if (capsuleCollider) {
            ghost.AddComponent<CapsuleCollider>(capsuleCollider);
        }
        if (playerState) {
            ogStateful = playerState;
            ghostStateful = ghost.AddComponent<PlayerState>(playerState);
        }
        SceneManager.MoveGameObjectToScene(ghost, SceneManager.GetSceneByName("ParallelGameScene"));
        ghost.SetActive(false);
        RollbackManager.Instance.Subscribe(gameObject.GetInstanceID(), ogStateful, ghostStateful, gameObject, ghost);
    }


    void Awake(){
        rb = this.GetComponent<Rigidbody>();
        capsuleCollider = this.GetComponent<CapsuleCollider>();
        playerState = this.GetComponent<PlayerState>();
    }

    void Start()
    {
        CreateGhost();
    }

    void OnDestroy(){
        RollbackManager.Instance.Unsubscribe(this.gameObject.GetInstanceID());
        Destroy(ghost);
    }

}
