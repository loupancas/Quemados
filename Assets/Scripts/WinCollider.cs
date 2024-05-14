using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out Player p)) return;

        if (!p.HasStateAuthority) return;

        GameManager.Instance.RPC_PlayerWin(p.Runner.LocalPlayer);
    }
}
