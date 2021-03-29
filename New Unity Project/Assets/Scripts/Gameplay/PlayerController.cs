using UnityEngine;
using Mirror;
using System;

public static class PlayerControllerSettings
{
    public static float MoveSpeed = 10.0f;
    public static float TurnSpeed = 5.0f;
    public static float CrouchSpeed = 5.0f;
    public static float Gravity = 10.0f;
    public static float JumpForce = 5.0f;
}

public class PlayerController : NetworkBehaviour
{
    [SyncVar] public string alias;
    [SyncVar] public GameObject controlledPawn;

    private void Update()
    {
        if (!hasAuthority) { return; }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            if (controlledPawn != null) return;
            CmdRequestToSpawn();
        }
        if (controlledPawn == null) return;
        Pawn controlledPawnRef = controlledPawn.GetComponent<Pawn>();
    }

    public void NotifyGameManagerOfDeath()
    {
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().WhenPlayerDies(this);
    }

    [Command] void CmdRequestToSpawn()
    {
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().SpawnPlayerForController(this);
    }

    //[Command]
    void CmdShootGun(bool hasHitTarget, Vector3 targetPosition)
    {
        if (controlledPawn == null) return;
        controlledPawn.GetComponent<Pawn>().Shoot(hasHitTarget, targetPosition);
    }

    //[Command]
    void CmdReloadGun()
    {
        if (controlledPawn == null) return;
        if (controlledPawn.GetComponent<Pawn>().bulletsInMag == Pawn.bulletsPerMag) return;

        controlledPawn.GetComponent<Pawn>().reloading = true;
        controlledPawn.GetComponent<Pawn>().Reload();
        Invoke("SetPawnGunReloadFalse", controlledPawn.GetComponent<Pawn>().reloadAnimDuration);
    }
}
