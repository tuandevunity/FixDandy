using System;
using System.Collections;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : Singleton<GameController>
{
    [SerializeField] GameObject playerPrefab;
    public GameState GameState;
    public MainCamController MainCamController;
    public MapController MapController { get; private set; }
    public PlayerController PlayerController { get; private set; }
    public PlayerMovement PlayerMovement { get; private set; }
    public CamPlayer CamPlayer { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        Application.targetFrameRate = 60;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        SetPosPrefabPlayer(Vector3.zero);
    }
    void SetPosPrefabPlayer(Vector3 posSpawn)
    {
        playerPrefab.transform.position = posSpawn;
    }
    public void LoadingGame(Action complete = null)
    {
        var dataGamePlay = DataManager.GamePlayUtility.Profile;
        StartCoroutine(LoadSceneAsync(() =>
        {
            complete?.Invoke();
            UIManager.ShowPanel<PanelGamePlay>(0);
            //
            UIManager.ShowPanel<PanelSetting>(0);
            SetGameState(GameState.Playing);
        }));
    }
    public void ResetGame()
    {
        ResetMap();
        var dataGamePlay = DataManager.GamePlayUtility.Profile;
        UIManager.HideAllPanel();
        UIManager.ShowPanel<PanelLoadScene>(0).OnShow(StartCoroutine(IELoadScenePlayGame()), complete: () =>
        {
            UIManager.ShowPanel<PanelGamePlay>(0);

            UIManager.ShowPanel<PanelSetting>(0);
            SetGameState(GameState.Playing);
        });
    }
    public void ResetMap()
    {
        var dataGamePlay = DataManager.GamePlayUtility.Profile;
        DataManager.SelfUtility.Profile.Spoon = false;
        dataGamePlay.ResetMap();
    }
    public void InitPlayer(Transform transSpawnPlayer, bool setPosPlayer = true) // Khoi tao nguoi choi
    {
        var spawnPlayer = transSpawnPlayer;
        SetPosPrefabPlayer(setPosPlayer ? transSpawnPlayer.position + transSpawnPlayer.forward * 0.5f : transSpawnPlayer.position);
        if(PlayerController != null) Destroy(PlayerController.gameObject);
        var player = Instantiate(playerPrefab);
        
        CamPlayer = player.GetComponent<CamPlayer>();
        MainCamController.SetFollowAndTimeChangeCam(CamPlayer.GetCamTarget(), 0.5f);
        PlayerController = player.GetComponent<PlayerController>();
        PlayerMovement = player.GetComponent<PlayerMovement>();
        
        PlayerMovement.ResetPlayer(spawnPlayer.eulerAngles.y + 180);
        CamPlayer.SetRotCamFirstGame(spawnPlayer.eulerAngles.y);
    }

    IEnumerator LoadSceneAsync(Action complete = null)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync("Map" + DataManager.GamePlayUtility.Profile.Map);

        while(!operation.isDone)
        {
            yield return null;
        }
        MapController = FindObjectOfType<MapController>();
        InitPlayer(CheckPointCurrent());
        complete?.Invoke();
    }
    IEnumerator IELoadScenePlayGame()
    {
        SceneManager.LoadScene("Map" + DataManager.GamePlayUtility.Profile.Map);
        yield return null;
        MapController = FindObjectOfType<MapController>();
        InitPlayer(CheckPointCurrent());
    }
    Transform CheckPointCurrent(bool increaseCheckPoint = false)
    {
        Debug.Log("lay check point player");
        var dataGamePlay = DataManager.GamePlayUtility.Profile;
        if(increaseCheckPoint) dataGamePlay.IncreseCheckPointInMap();
        return MapController.Checkpoint;
    }

    public void SkipCheckpoint()
    {
        StartCoroutine(IESkipCheckpoint());
    }


    IEnumerator IESkipCheckpoint()
    {
        GameState = GameState.Pause;
        Transform nextCheckpoint = CheckPointCurrent(true); // co the cai nay null
        PlayerMovement.SetPosPlayer(nextCheckpoint.position + nextCheckpoint.forward * 0.5f); // VI TRI
        PlayerMovement.ResetPlayer(PlayerMovement.transform.eulerAngles.y + 180); // GOC XOAY
        var cam = PlayerMovement.GetComponent<CamPlayer>(); // CAM
        cam.SetRotCamFirstGame(PlayerMovement.transform.eulerAngles.y);
        yield return new WaitForSeconds(0.2f);
        GameState = GameState.Playing;
    }

    public void SetGameState(GameState gameState) => GameState = gameState;
}
public enum GameState
{
    None,
    Playing,
    Pause,
}
