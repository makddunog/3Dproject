using Fusion;
using UnityEngine;

public struct PlayerInputData : INetworkInput
{
    public Vector2 move;
    public Vector2 look;
    public NetworkButtons buttons;
}

public enum PlayerButtons
{
    Jump = 0,
    Run = 1
}