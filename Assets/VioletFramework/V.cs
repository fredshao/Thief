using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

public class V : VMonoSingleton<V> {
    private static Audio _vAudio = null;
    private static ResourceSystem _vResource = null;
    private static BundleSystem _vBundle = null;
    private static UnityTicker _vTicker = null;
    private static GridSystem _vGrid = null;
    private static HexagonSystem _vHexagon = null;
    private static MsgSystem _vMsg = null;
    private static ThreadBridge _vThread = null;
    private static ConfigManager _vTable = null;
    private static UISystem _vUI = null;
    private static VEncoder _vEncoder = null;
    private static SystemService _vSystemService = null;

    public static Audio vAudio { get { return _vAudio; } }
    public static ResourceSystem vResource { get { return _vResource; } }
    public static BundleSystem vBundle { get { return _vBundle; } }
    public static UnityTicker vTicker { get { return _vTicker; } }
    public static GridSystem vGrid { get { return _vGrid; } }
    public static HexagonSystem vHexagon { get { return _vHexagon; } }
    public static MsgSystem vMsg { get { return _vMsg; } }
    public static ThreadBridge vThread { get { return _vThread; } }
    public static ConfigManager vTable { get { return _vTable; } }
    public static UISystem vUI { get { return _vUI; } }
    public static VEncoder vEncoder { get { return _vEncoder; } }
    public static SystemService vSystemService { get { return _vSystemService; } }


	public override void Initialize()
    {
        base.Initialize();

        _vAudio = new Audio();
        _vAudio.Initialize();

        _vResource = new ResourceSystem();
        _vResource.Initialize();

        _vBundle = new BundleSystem();
        _vBundle.Initialize();

        _vTicker = new UnityTicker();
        _vTicker.Initialize();

        _vGrid = new GridSystem();
        _vGrid.Initialize();

        _vHexagon = new HexagonSystem();
        _vHexagon.Initialize();

        _vMsg = new MsgSystem();
        _vMsg.Initialize();

        _vThread = new ThreadBridge();
        _vThread.Initialize();

        _vTable = new ConfigManager();
        //_vTable.Initialize();

        _vUI = new UISystem();
        _vUI.Initialize();

        _vEncoder = new VEncoder();
        _vEncoder.Initialize();

        _vSystemService = new SystemService();
        _vSystemService.Initialize();
    }

    private void Update()
    {
        _vAudio.OnUpdate();
        _vResource.OnUpdate();
        _vBundle.OnUpdate();
        _vTicker.OnUpdate();
        _vGrid.OnUpdate();
        _vHexagon.OnUpdate();
        _vMsg.OnUpdate();
        _vThread.OnUpdate();
        _vUI.OnUpdate();
        _vSystemService.OnUpdate();

    }

    private void FixedUpdate()
    {
        _vAudio.OnFixedUpdate();
        _vResource.OnFixedUpdate();
        _vBundle.OnFixedUpdate();
        _vTicker.OnFixedUpdate();
        _vGrid.OnFixedUpdate();
        _vHexagon.OnFixedUpdate();
        _vMsg.OnFixedUpdate();
        _vThread.OnFixedUpdate();
        _vUI.OnFixedUpdate();
        _vSystemService.OnFixedUpdate();
    }



}
