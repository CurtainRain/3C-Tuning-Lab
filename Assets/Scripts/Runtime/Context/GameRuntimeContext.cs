using System;
using System.Collections;
using Runtime.Const.Enums;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏运行时环境
/// </summary>
public class GameRuntimeContext : MonoBehaviour
{
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
    [SerializeField] private string _firstSceneName;

    [Header("3C参数")]
    [Tooltip("摄像机参数")] public CameraController3CParams cameraController3CParams;
    [Tooltip("角色参数")] public CharacterController3CParams characterController3CParams;

    private Coroutine _recordLoop;
    private string _bootstrapSceneName;  // 记录引导场景名称

    void Awake()
    {
        // 在 DontDestroyOnLoad 之前记录当前场景名称
        _bootstrapSceneName = gameObject.scene.name;

        DontDestroyOnLoad(gameObject);

        if(Instance){
            throw new Exception("GameRuntimeContext 只能有一个实例");
        }

        if(string.IsNullOrEmpty(_firstSceneName)){
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
        StartCoroutine(LoadGameSceneAsync());
    }

    /// <summary>
    /// 异步加载游戏场景并卸载引导场景
    /// </summary>
    private IEnumerator LoadGameSceneAsync()
    {
        // 使用 Additive 模式加载游戏场景
        yield return SceneManager.LoadSceneAsync(_firstSceneName, LoadSceneMode.Additive);

        // 设置新场景为激活场景
        Scene newScene = SceneManager.GetSceneByName(_firstSceneName);
        if (newScene.IsValid())
        {
            SceneManager.SetActiveScene(newScene);
            Debug.Log($"GameRuntimeContext: 已激活场景 {_firstSceneName}");
        }

        // 等待一帧，让Unity完成场景激活
        yield return null;

        // 切换了场景 需要更新全局光照
        DynamicGI.UpdateEnvironment();

        // 卸载引导场景
        if (!string.IsNullOrEmpty(_bootstrapSceneName) && _bootstrapSceneName != "DontDestroyOnLoad")
        {
            yield return SceneManager.UnloadSceneAsync(_bootstrapSceneName);
            Debug.Log($"GameRuntimeContext: 已卸载引导场景 {_bootstrapSceneName}");
        }
    }

    void FixedUpdate()
    {
        systemInputHandler.FixedUpdate();
        gameRunningModeSwitcher.FixedUpdate();

        playerInputHandler.FixedUpdate();
        recordPlaybackInputHandler.FixedUpdate();
    }

    void Update()
    {
        systemInputHandler.Update();
        playerInputHandler.Update();
    }

    void LateUpdate()
    {
    }

    private IEnumerator RecordLoop()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            OnFrameEnd();
        }
    }

    void OnFrameEnd(){
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
