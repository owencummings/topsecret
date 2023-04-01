/*
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class RollbackNetcode : NetworkBehaviour
{
    private const int StateBufferSize = 64;
    private readonly Queue<GameState> _stateBuffer = new Queue<GameState>(StateBufferSize);
    private int _currentStateIndex = 0;

    private struct GameState
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    private void StoreCurrentState()
    {
        if (_stateBuffer.Count >= StateBufferSize)
        {
            _stateBuffer.Dequeue();
        }

        _stateBuffer.Enqueue(new GameState
        {
            position = transform.position,
            rotation = transform.rotation,
        });

        _currentStateIndex++;
    }

    private void RollbackToState(int index)
    {
        if (index < 0 || index >= _stateBuffer.Count)
        {
            Debug.LogError("Invalid state index for rollback.");
            return;
        }

        GameState state = _stateBuffer.ToArray()[index];
        transform.position = state.position;
        transform.rotation = state.rotation;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            NetworkTickSystem.Instance.OnNetworkTick += OnNetworkTick;
            NetworkManager.Singleton.OnNamedMessageReceived += OnNamedMessageReceived;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            NetworkTickSystem.Instance.OnNetworkTick -= OnNetworkTick;
            NetworkManager.Singleton.OnNamedMessageReceived -= OnNamedMessageReceived;
        }
        base.OnNetworkDespawn();
    }

    private void OnNetworkTick()
    {
        StoreCurrentState();

        // Send input data to the server
        if (IsOwner)
        {
            // Collect input data
            Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            // Send input data to the server
            InputMessage inputMessage = new InputMessage
            {
                senderClientId = OwnerClientId,
                input = input
            };

            CustomMessagingManager.SendNamedMessage("PlayerInput", NetworkManager.Singleton.ServerClientId, inputMessage);
        }
    }

    private void OnNamedMessageReceived(ulong clientId, string name, FastBufferReader reader)
    {
        if (name == "PlayerInput")
        {
            if (IsServer && clientId != OwnerClientId)
            {
                return;
            }

            // Read input data from the message
            InputMessage inputMessage = reader.ReadValue<InputMessage>();

            // Perform movement logic based on the received input
            ProcessInput(inputMessage.input);

            // If not the server, perform rollback logic
            if (!IsServer)
            {
                int rollbackIndex = _currentStateIndex - StateBufferSize / 2;
                RollbackToState(rollbackIndex);

                // Re-simulate forward with the corrected input data
                GameState[] states = _stateBuffer.ToArray();
                for (int i = rollbackIndex + 1; i < _currentStateIndex; i++)
                {
                    ProcessInput(states[i].input);
                }
            }
        }
    }

    private void ProcessInput(Vector3 input)
    {
        // Implement your movement logic here based on the input
    }

    private struct InputMessage : INetworkSerializable
    {
        public ulong senderClientId;
        public Vector3 input;

        public void Serialize(FastBufferWriter writer)
        {
            writer.WriteValue(senderClientId);
            writer.WriteValue(input);
        }

        public void Deserialize(FastBufferReader reader)
        {
            senderClientId = reader.ReadValue<ulong>();
            input = reader.ReadValue<Vector3>();
        }
    }
}
*/