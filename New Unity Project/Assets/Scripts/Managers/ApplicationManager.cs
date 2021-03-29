using UnityEngine;
using Mirror;
using Steamworks;

public class ApplicationManager : MonoBehaviour
{
    private static ApplicationManager _instance;
    public static ApplicationManager Instance() { return _instance; }

    NetworkManagerMultiplayer networkManager;
    [Scene] public string openingScene;

    [SerializeField] GameObject pauseMenuPanel;
    bool isPlaying;
    bool isPaused;
    public bool cursorShouldBeLocked;
    GameObject currentlyShowingPauseMenu;

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    protected Callback<GameOverlayActivated_t> m_GameOverlayActivated;

    private const string HostAddressKey = "HostAddress";

    private bool isHosting;

    private void Awake()
    {
        if (_instance != null) { Destroy(gameObject); }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        networkManager = FindObjectOfType<NetworkManagerMultiplayer>();
        networkManager.OnClientDisconnectEventFired += NetworkManager_OnClientDisconnectEventFired;
        networkManager.OnClientStoppedEventFired += NetworkManager_OnClientStoppedEventFired;

        if (!SteamManager.Initialized) { return; }
        string name = SteamFriends.GetPersonaName();
        Debug.Log(name);

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

        m_GameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
    }

    private void OnGameOverlayActivated(GameOverlayActivated_t pCallback)
    {
        if (pCallback.m_bActive != 0)
        {
            Debug.Log("Steam Overlay has been activated");
        }
        else
        {
            Debug.Log("Steam Overlay has been closed");
        }
    }

    private void NetworkManager_OnClientStoppedEventFired()
    {
        OpenOpeningScene();
    }

    private void NetworkManager_OnClientDisconnectEventFired()
    {
        OpenOpeningScene();
    }

    void OpenOpeningScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(openingScene);
    }

    public void HostGame()
    {
        isPlaying = true;
        isHosting = true;

        networkManager.StartHost();

        //
        
        //
    }

    public void JoinGame(string ipAddress)
    {
        isPlaying = true;
        isHosting = false;

        networkManager.networkAddress = ipAddress;
        networkManager.StartClient();
    }

    public void HostSteamGame()
    {

        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);
    }

    public void QuitApplicationButtonClicked()
    {
        Application.Quit();
    }


    private void Update()
    {
        if (isPlaying)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!isPaused)
                {
                    isPaused = true;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;

                    GameObject uiCanvas = GameObject.FindGameObjectWithTag("GameCanvas");
                    currentlyShowingPauseMenu = Instantiate(pauseMenuPanel, uiCanvas.transform);
                    currentlyShowingPauseMenu.GetComponent<PauseMenuScript>().isHosting = isHosting;
                }
                else
                {
                    isPaused = false;
                    if (cursorShouldBeLocked)
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                    }
                    Destroy(currentlyShowingPauseMenu);
                }
            }
        }
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            //button.SetActive(true);
            return;
        }

        isPlaying = true;
        isHosting = true;

        networkManager.StartHost();

       
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
        string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);

    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);


    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (NetworkServer.active) { return; }

        isPlaying = true;
        isHosting = false;

        string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);

        networkManager.networkAddress = hostAddress;

        networkManager.StartClient();

        //button.SetActive(false);

    }
}
