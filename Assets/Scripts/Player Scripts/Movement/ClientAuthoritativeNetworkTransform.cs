using Unity.Netcode.Components;
using UnityEngine;

// Owner-authoritative NetworkTransform: the client controlling a player simulates its own
// physics/movement (Playermovement, gated by IsOwner) and this replicates that transform to
// everyone else, instead of expecting the server to own it.
[DisallowMultipleComponent]
public class ClientAuthoritativeNetworkTransform : NetworkTransform
{
    protected override bool OnIsServerAuthoritative() => false;
}
