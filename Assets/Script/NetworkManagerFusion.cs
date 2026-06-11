using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManagerFusion : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkManagerFusion Instance;

    public NetworkPrefabRef playerPrefab;

    private NetworkRunner runner;
    private NetworkSceneManagerDefault sceneManager;

    private const string VillageRoomName = "VillageRoom";

    private const string VillageSceneName = "SampleScene";
    private const string HouseSceneName = "HouseScene";

    private bool started = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    async void Start()
    {
        await StartVillageSession();
    }

    private async Task StartVillageSession()
    {
        if (started)
        {
            return;
        }

        started = true;

        Debug.Log("[Fusion] Village Session 시작 준비");

        runner = GetComponent<NetworkRunner>();

        if (runner == null)
        {
            runner = gameObject.AddComponent<NetworkRunner>();
        }

        sceneManager = GetComponent<NetworkSceneManagerDefault>();

        if (sceneManager == null)
        {
            sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        runner.RemoveCallbacks(this);
        runner.AddCallbacks(this);
        runner.ProvideInput = true;

        Debug.Log("[Fusion] StartGame 호출");

        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = VillageRoomName,
            SceneManager = sceneManager
        });

        if (result.Ok)
        {
            Debug.Log("[Fusion] StartGame 성공: " + VillageRoomName);
        }
        else
        {
            Debug.LogError("[Fusion] StartGame 실패: " + result.ShutdownReason);
        }
    }

    public async void EnterHouseRoom()
    {
        Debug.Log("[Scene] HouseScene으로 이동");

        KeepPlayerAlive();

        if (SceneManager.GetActiveScene().name != HouseSceneName)
        {
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(HouseSceneName);

            while (!loadOperation.isDone)
            {
                await Task.Yield();
            }
        }

        await MovePlayerToSpawnPoint();
    }

    public async void EnterVillageRoom()
    {
        Debug.Log("[Scene] SampleScene으로 이동");

        KeepPlayerAlive();

        if (SceneManager.GetActiveScene().name != VillageSceneName)
        {
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(VillageSceneName);

            while (!loadOperation.isDone)
            {
                await Task.Yield();
            }
        }

        await MovePlayerToSpawnPoint();
    }

    private GameObject FindLocalPlayer()
    {
        NetworkObject[] networkObjects = FindObjectsOfType<NetworkObject>(true);

        foreach (NetworkObject obj in networkObjects)
        {
            if (obj.HasStateAuthority && obj.GetComponent<CharacterController>() != null)
            {
                Debug.Log("[FindLocalPlayer] StateAuthority 플레이어 찾음: " + obj.name);
                return obj.gameObject;
            }
        }

        foreach (NetworkObject obj in networkObjects)
        {
            if (obj.HasInputAuthority && obj.GetComponent<CharacterController>() != null)
            {
                Debug.Log("[FindLocalPlayer] InputAuthority 플레이어 찾음: " + obj.name);
                return obj.gameObject;
            }
        }

        Camera[] cameras = FindObjectsOfType<Camera>(true);

        foreach (Camera cam in cameras)
        {
            if (cam.enabled && cam.gameObject.activeInHierarchy)
            {
                Transform current = cam.transform;

                while (current.parent != null)
                {
                    current = current.parent;
                }

                if (current.GetComponent<CharacterController>() != null)
                {
                    Debug.Log("[FindLocalPlayer] 활성 카메라 기준 플레이어 찾음: " + current.name);
                    return current.gameObject;
                }
            }
        }

        Debug.LogWarning("[FindLocalPlayer] 내 플레이어를 찾지 못했습니다.");
        return null;
    }

    private void KeepPlayerAlive()
    {
        GameObject player = FindLocalPlayer();

        if (player == null)
        {
            Debug.LogWarning("[Scene] 씬 이동 전 플레이어를 찾지 못했습니다.");
            return;
        }

        DontDestroyOnLoad(player);
        Debug.Log("[Scene] 플레이어를 씬 이동 후에도 유지합니다: " + player.name);
    }

    private async Task MovePlayerToSpawnPoint()
    {
        await Task.Delay(1500);

        GameObject player = FindLocalPlayer();

        if (player == null)
        {
            Debug.LogWarning("[Scene] 플레이어 오브젝트를 찾을 수 없습니다.");
            return;
        }

        Debug.Log("[Scene] 이동 전 플레이어 위치: " + player.transform.position);
        Debug.Log("[Scene] 이동할 플레이어: " + player.name);

        CharacterController controller = player.GetComponent<CharacterController>();

        if (controller != null)
        {
            controller.enabled = false;
        }

        string currentSceneName = SceneManager.GetActiveScene().name;

        Vector3 targetPosition;
        Quaternion targetRotation;

        if (currentSceneName == HouseSceneName)
        {
            // HouseScene 강제 스폰 위치
            // 지금 전지현씨가 보여준 침대 위치 기준으로 Y만 올린 좌표
            targetPosition = new Vector3(-3.1f, 1.2f, 2.4f);
            targetRotation = Quaternion.Euler(0f, 180f, 0f);

            Debug.Log("[Scene] HouseScene 고정 좌표로 이동 시도");
        }
        else
        {
            // SampleScene 복귀 위치
            targetPosition = new Vector3(0f, 1.2f, 0f);
            targetRotation = Quaternion.Euler(0f, 0f, 0f);

            GameObject spawnPoint = null;

            try
            {
                spawnPoint = GameObject.FindWithTag("SpawnPoint");
            }
            catch
            {
                Debug.LogWarning("[Scene] SpawnPoint 태그를 찾을 수 없습니다.");
            }

            if (spawnPoint != null)
            {
                targetPosition = spawnPoint.transform.position;
                targetRotation = spawnPoint.transform.rotation;
            }

            Debug.Log("[Scene] SampleScene 좌표로 이동 시도");
        }

        player.transform.SetPositionAndRotation(targetPosition, targetRotation);

        if (controller != null)
        {
            controller.enabled = true;
        }

        MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour script in scripts)
        {
            if (script != null)
            {
                script.enabled = true;
            }
        }

        Camera[] allCameras = FindObjectsOfType<Camera>(true);

        foreach (Camera cam in allCameras)
        {
            cam.enabled = false;
        }

        AudioListener[] allListeners = FindObjectsOfType<AudioListener>(true);

        foreach (AudioListener listener in allListeners)
        {
            listener.enabled = false;
        }

        Camera[] playerCameras = player.GetComponentsInChildren<Camera>(true);

        foreach (Camera cam in playerCameras)
        {
            cam.gameObject.SetActive(true);
            cam.enabled = true;
            cam.tag = "MainCamera";
        }

        AudioListener[] playerListeners = player.GetComponentsInChildren<AudioListener>(true);

        foreach (AudioListener listener in playerListeners)
        {
            listener.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("[Scene] 이동 후 플레이어 위치: " + player.transform.position);
        Debug.Log("[Scene] 플레이어 위치 이동 + 컨트롤 재활성화 완료");
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("[Fusion] Player Joined: " + player);

        // Shared Mode에서는 각자 자기 플레이어만 직접 Spawn
        if (player != runner.LocalPlayer)
        {
            return;
        }

        Vector3 spawnPosition = Vector3.zero;
        Quaternion spawnRotation = Quaternion.identity;

        GameObject spawnPoint = null;

        try
        {
            spawnPoint = GameObject.FindWithTag("SpawnPoint");
        }
        catch
        {
            Debug.LogWarning("[Fusion] SpawnPoint 태그가 없어서 Vector3.zero에 스폰합니다.");
        }

        if (spawnPoint != null)
        {
            spawnPosition = spawnPoint.transform.position;
            spawnRotation = spawnPoint.transform.rotation;
        }
        else
        {
            spawnPosition = new Vector3(0f, 1.2f, 0f);
            spawnRotation = Quaternion.Euler(0f, 0f, 0f);
        }

        if (playerPrefab.Equals(default(NetworkPrefabRef)))
        {
            Debug.LogError("[Fusion] Player Prefab이 비어 있습니다. NetworkManager의 Player Prefab에 Ghost1을 넣어주세요.");
            return;
        }

        runner.Spawn(
            playerPrefab,
            spawnPosition,
            spawnRotation,
            player
        );

        Debug.Log("[Fusion] Local Player Spawn 완료");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("[Fusion] Player Left: " + player);
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        PlayerInputData data = new PlayerInputData();

        data.move = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        data.look = new Vector2(
            Input.GetAxisRaw("Mouse X"),
            Input.GetAxisRaw("Mouse Y")
        );

        NetworkButtons buttons = default;

        buttons.Set(PlayerButtons.Jump, Input.GetKey(KeyCode.Space));
        buttons.Set(PlayerButtons.Run, Input.GetKey(KeyCode.LeftShift));

        data.buttons = buttons;

        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("[Fusion] Shutdown: " + shutdownReason);
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("[Fusion] Connected To Server");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log("[Fusion] Disconnected From Server: " + reason);
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.LogWarning("[Fusion] Connect Failed: " + reason);
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("[Fusion] Scene Load Done");
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        Debug.Log("[Fusion] Scene Load Start");
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
}