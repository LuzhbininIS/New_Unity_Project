using Mirror;
using System.Linq;
using UnityEngine;

public class RoundSystem : NetworkBehaviour
{
	[SerializeField] private Animator animator = null;

	private NetworkManagerAll room;
	private NetworkManagerAll Room
	{
		get
		{
			if (room != null) { return room; }
			return room = NetworkManager.singleton as NetworkManagerAll;
		}
	}

	public void CountdownEnded()
	{
		animator.enabled = false;
	}

	public override void OnStartServer()
	{
		NetworkManagerAll.OnServerStopped += CleanUpServer;
		NetworkManagerAll.OnServerReaded += CheckToStartRound;
	}

	[ServerCallback]
	private void OnDestroy()
	{
		CleanUpServer();
	}

	[Server]
	private void CleanUpServer()
	{
		NetworkManagerAll.OnServerStopped -= CleanUpServer;
		NetworkManagerAll.OnServerReaded -= CheckToStartRound;
	}

	[ServerCallback]
	public void StartRound()
	{
		RpcStartRound();
	}

	[Server]
	private void CheckToStartRound(NetworkConnection conn)
	{
		if (Room.GamePlayers.Count(x => x.connectionToClient.isReady) != Room.GamePlayers.Count) { return; }

		animator.enabled = true;

		RpcStartCountdown();
	}

	[ClientRpc]
	private void RpcStartCountdown()
	{
		animator.enabled = true;
	}

	[ClientRpc]
	private void RpcStartRound()
	{
		//InputManager.Remove(ActionMapNames.Player);
	}
}
