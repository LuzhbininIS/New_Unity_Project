using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Old_Player : NetworkBehaviour
{
    [SerializeField]
    private Vector3 movement = new Vector3();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    [Client]
    // Update is called once per frame
    void Update()
    {
        if (!hasAuthority) { return; }

        if (!Input.GetKeyDown(KeyCode.Space)) { return; }

        //transform.Translate(movement);

        CmdMove();
    }

    [Command]
    private void CmdMove()
	{
        // Validate logic here
        // Логика сервера (определение игрока, выбор соответствующих команд)

        RpcMove();
    }

    [ClientRpc]
    private void RpcMove()
	{
        // Реакция клиента на решение сервера
        transform.Translate(movement);

    }
}
