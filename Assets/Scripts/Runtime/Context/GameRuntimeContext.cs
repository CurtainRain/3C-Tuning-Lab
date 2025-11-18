using System;
using System.Collections;
using Runtime.Const.Enums;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏运行时环境
/// </summary>
public class GameRuntimeContext : MonoBehaviour
{

    // c-mark:服务和handler太多了 考虑直接全部编程接口 这样只有new的地方要考虑初始化 其他地方直接使用接口
    public static GameRuntimeContext Instance;

    // 基础服务
    public StorageService storageService;
    public Output3CDataByCSVService output3CDataByCSVService;
    public IOPlayerInputDataRecordService iOPlayerInputDataService;

    // 游戏模式
    public GameRunningModeSwitcher gameRunningModeSwitcher;


    // 输入相关
    public IInputProvider inputProvider;
    public SystemInputHandler systemInputHandler;
    public PlayerInputHandler playerInputHandler;
    public RecordPlaybackInputHandler recordPlaybackInputHandler;

    [Header("首地图")]
    [SerializeField] private SceneAsset firstScene;

    private Coroutine _recordLoop;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if(Instance){
            throw new Exception("GameRuntimeContext 只能有一个实例");
        }

        if(!firstScene){
            throw new Exception("GameRuntimeContext 未设置首地图");
        }

        Instance = this;

        storageService = new StorageService(Application.persistentDataPath);
        output3CDataByCSVService = new Output3CDataByCSVService();
        output3CDataByCSVService.Init(this);
        iOPlayerInputDataService = new IOPlayerInputDataRecordService();
        iOPlayerInputDataService.Init(this);

        inputProvider = new KeyboardMouseInputProvider();
        inputProvider.Init();
        systemInputHandler = new SystemInputHandler();
        systemInputHandler.Init(inputProvider);
        playerInputHandler = new PlayerInputHandler();
        playerInputHandler.Init(this);
        recordPlaybackInputHandler = new RecordPlaybackInputHandler();
        recordPlaybackInputHandler.Init(this);

        gameRunningModeSwitcher = new GameRunningModeSwitcher();
        gameRunningModeSwitcher.Init(systemInputHandler);

        _recordLoop = StartCoroutine(RecordLoop());

        Debug.Log("GameRuntimeContext: 初始化完成, 切换到首地图");
        SceneManager.LoadScene(firstScene.name, LoadSceneMode.Additive);
    }

    void Update()
    {
        systemInputHandler.Tick();
        playerInputHandler.Tick();
        recordPlaybackInputHandler.Tick();
    }

    private IEnumerator RecordLoop()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();  // 等所有 LateUpdate 结束
            OnFrameEnd();
        }
    }

    void OnFrameEnd(){
        recordPlaybackInputHandler.OnFrameEnd();
    }

    void OnDestroy()
    {
        if(_recordLoop != null){
            StopCoroutine(_recordLoop);
            _recordLoop = null;
        }

        if(gameRunningModeSwitcher != null)
        {
            gameRunningModeSwitcher.Destroy();
            gameRunningModeSwitcher = null;
        }

        if(systemInputHandler != null){
            systemInputHandler.Destroy();
            systemInputHandler = null;
        }

        if(recordPlaybackInputHandler != null){
            recordPlaybackInputHandler.Destroy();
            recordPlaybackInputHandler = null;
        }

        if(playerInputHandler != null){
            playerInputHandler.Destroy();
            playerInputHandler = null;
        }

        if(inputProvider != null){
            inputProvider.Destroy();
            inputProvider = null;
        }

        if(iOPlayerInputDataService != null){
            iOPlayerInputDataService.Destroy();
            iOPlayerInputDataService = null;
        }

        if(output3CDataByCSVService != null){
            output3CDataByCSVService.Destroy();
            output3CDataByCSVService = null;
        }

        storageService = null;
        Instance = null;
    }

    public PlayerInputData GetInputData(){
        if(gameRunningModeSwitcher.currentRunningMode == GameRunningMode.RecordPlaybackMode){
            return recordPlaybackInputHandler.GetInputData();
        }else{
            return playerInputHandler.GetInputData();
        }
    }
}
