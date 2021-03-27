using UnityEngine;
using Mirror;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;

public class NetworkManagerAll: NetworkManager
{
	[Header("Menu")]
	[SerializeField] private int minPlayers = 2;
	[Scene] [SerializeField] private string menuScene = string.Empty;

	[Header("Maps")]
	[SerializeField] private int numberOfRounds = 2;
	[SerializeField] private MapSet mapSet = null;

	[Header("Room")]
	[SerializeField] private NetworkRoomPlayerLobby roomPlayerPrefab = null;

	[Header("Game")]
	[SerializeField] private NetworkGamePlayerLobby gamePlayerPrefab = null;
	[SerializeField] private GameObject playerSpawnSystem = null;
	[SerializeField] private GameObject roundSystem = null;

	private MapHandler mapHandler;

	public static event Action OnClientConnected;
	public static event Action OnClientDisconnected;
	public static event Action<NetworkConnection> OnServerReaded;
	public static event Action OnServerStopped;

	private const string mapPrefix = "Scene_Map";

	public List<NetworkRoomPlayerLobby> RoomPlayers { get; } = new List<NetworkRoomPlayerLobby>();

	public List<NetworkGamePlayerLobby> GamePlayers { get; } = new List<NetworkGamePlayerLobby>();

	// Spectàtors lits here

	public override void OnStartServer()
	{
		spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();
	}

	public override void OnStartClient()
	{
		var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");

		foreach (var prefab in spawnablePrefabs)
		{
			ClientScene.RegisterPrefab(prefab);
		}
	}

	public override void OnClientConnect(NetworkConnection conn)
	{
		base.OnClientConnect(conn);

		OnClientConnected?.Invoke();
	}

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		base.OnClientDisconnect(conn);

		OnClientDisconnected?.Invoke();
	}

	public override void OnServerConnect(NetworkConnection conn)
	{
		if (numPlayers >= maxConnections)
		{
			conn.Disconnect();
			return;
		}

		if (SceneManager.GetActiveScene().path != menuScene)
		{
			conn.Disconnect();
			return;
		}
	}

	public override void OnServerAddPlayer(NetworkConnection conn)
	{
		if (SceneManager.GetActiveScene().path == menuScene)
		{
			bool isLeader = RoomPlayers.Count == 0;

			NetworkRoomPlayerLobby roomPlayerInstance = Instantiate(roomPlayerPrefab);

			roomPlayerInstance.IsLeader = isLeader;

			NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);

		}
	}

	public override void OnServerDisconnect(NetworkConnection conn)
	{
		if (conn.identity != null)
		{
			var player = conn.identity.GetComponent<NetworkRoomPlayerLobby>();

			RoomPlayers.Remove(player);

			NotifyPlayersOfReadyState();
		}

		base.OnServerDisconnect(conn);
	}

	public override void OnStopServer()
	{
		OnServerStopped?.Invoke();

		RoomPlayers.Clear();
		GamePlayers.Clear();
	}

	public void NotifyPlayersOfReadyState()
	{
		foreach (var player in RoomPlayers)
		{
			player.HandleReadyToStart(IsReadyToStart());
		}
	}

	private bool IsReadyToStart()
	{
		if (numPlayers < minPlayers) { return false; }

		foreach (var player in RoomPlayers)
		{
			if (!player.IsReady) { return false; }

		}

		return true;
	}

	public void StartGame()
	{
		if(SceneManager.GetActiveScene().path == menuScene)
		{
			if (!IsReadyToStart()) { return; }

			mapHandler = new MapHandler(mapSet, numberOfRounds);

			ServerChangeScene(mapHandler.NextMap());
		}
	}

	public void GoToNextMap()
	{
		if (mapHandler.IsComplete)
		{
			// Final scores, announce winner

			StopHost();

			return;
		}

		ServerChangeScene(mapHandler.NextMap());
	}

	public override void ServerChangeScene(string newSceneName)
	{
		// From menu to game
		if (SceneManager.GetActiveScene().path == menuScene && Path.GetFileName(newSceneName).StartsWith(mapPrefix))
		{
			for (int i = RoomPlayers.Count - 1; i >= 0; i--)
			{
				var conn = RoomPlayers[i].connectionToClient;
				var gamePlayerInstance = Instantiate(gamePlayerPrefab);
				gamePlayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);
				NetworkServer.Destroy(conn.identity.gameObject);

				NetworkServer.ReplacePlayerForConnection(conn, gamePlayerInstance.gameObject, true);
			}
		}

		base.ServerChangeScene(newSceneName);
	}

	public override void OnServerSceneChanged(string sceneName)
	{
		if (Path.GetFileName(sceneName).StartsWith(mapPrefix))
		{
			GameObject playerSpawnSystemInstance = Instantiate(playerSpawnSystem);
			NetworkServer.Spawn(playerSpawnSystemInstance);

			GameObject roundSystemInstance = Instantiate(roundSystem);
			NetworkServer.Spawn(roundSystemInstance);
		}
	}

	public override void OnServerReady(NetworkConnection conn)
	{
		base.OnServerReady(conn);

		OnServerReaded?.Invoke(conn);
	}
}
