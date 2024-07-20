using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;//Fileを扱うため

public class SaveData
{
    //フォルダ
    static string windowsPlayerDataPath = Application.dataPath + "/../" + "/SaveData";
    static string androidDataPath = Application.persistentDataPath + "/SaveData";
    //ファイル
    static string gameDataName = "/GameData.json";
    static string systemDataName = "/SystemData.json";

    /////////////////////////////////////////////////
    public static UnityEvent onImpossibleDevice = new UnityEvent();//セーブ不可能な端末のとき
    public static UnityEvent onComplateSave = new UnityEvent();//セーブが完了したとき

    static string thisDeviceDataPath = null;
    static bool savePossible = false;//セーブができるデバイスか

    public static void BootCheck()
    {
        Debug.Log(Application.platform);
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            thisDeviceDataPath = windowsPlayerDataPath;
            savePossible = true;
            CheckFile(thisDeviceDataPath);
        }
        else if(Application.platform == RuntimePlatform.Android)
        {
            thisDeviceDataPath = androidDataPath;
            savePossible = true;
            CheckFile(thisDeviceDataPath);
        }
        else
        {
            savePossible = false;
            onImpossibleDevice.Invoke();
            Debug.Log("セーブ対象外デバイスです");
        }
    }

    static void CheckFile(string dataPath)
    {
        //フォルダがなかったら作成する
        if (!Directory.Exists(dataPath))
        {
            Directory.CreateDirectory(dataPath);
            Debug.Log("セーブデータフォルダを作成");
        }
    }

    ///////////////////////////////////////////////////////////////////////
    #region System

    public class SystemSaveData//設定
    {
        [SerializeField]
        public static bool firstBoot = false;//最初の起動
        [SerializeField]
        public static bool firstTutorial = false;//最初のチュートリアル起動
        [SerializeField]
        public static string applicationVersion = Application.version;//アプリのバージョン
        [SerializeField]
        public static bool fullScreen = true;//フルスクリーン
        [SerializeField]
        public static int screenSize = 2;//解像度変更
        [SerializeField]
        public static float bgmVolume = 0.7f;//BGMの音量
        [SerializeField]
        public static float seVolume = 1f;//効果音の音量

        public static void Save()
        {
            if (savePossible)
            {
                SystemData systemData = new SystemData();
                systemData.firstBoot = firstBoot;
                systemData.firstTutorial = firstTutorial;
                systemData.applicationVersion = applicationVersion;
                systemData.fullScreen = fullScreen;
                systemData.screenSize = screenSize;
                systemData.bgmVolume = bgmVolume;
                systemData.seVolume = seVolume;
                string json = JsonUtility.ToJson(systemData);//Jsonに変換
                File.WriteAllText(thisDeviceDataPath + systemDataName, json);//書き込み
                Debug.Log("セッティングデータ　セーブ完了");
            }
        }

        public static void Load()
        {
            if (savePossible)
            {
                string path = thisDeviceDataPath + systemDataName;//パスの設定
                string json;
                if (File.Exists(path))//ファイルがあれば
                {
                    json = File.ReadAllText(path);//読み込み
                    SystemData systemData = JsonUtility.FromJson<SystemData>(json);//変換
                    firstBoot = systemData.firstBoot;
                    firstTutorial = systemData.firstTutorial;
                    applicationVersion = systemData.applicationVersion;
                    fullScreen = systemData.fullScreen;
                    screenSize = systemData.screenSize;
                    bgmVolume = systemData.bgmVolume;
                    seVolume = systemData.seVolume;
                    Debug.Log("設定データ　ロード完了");
                }
                else
                {
                    Save();
                    Debug.Log("設定データ　セーブデータ 作成");
                }
            }
        }
    }

    class SystemData
    {
        public bool firstBoot;
        public bool firstTutorial;
        public string applicationVersion;
        public bool fullScreen;
        public int screenSize;
        public float bgmVolume;
        public float seVolume;
    }
    #endregion

    #region System

    public class GameSaveData//設定
    {
        [SerializeField]
        public static string playerName = "";
        [SerializeField]
        public static int playerSkin = 0;

        public static void Save()
        {
            if (savePossible)
            {
                GameData gameData = new GameData();
                gameData.playerName = playerName;
                gameData.playerSkin = playerSkin;
                string json = JsonUtility.ToJson(gameData);//Jsonに変換
                File.WriteAllText(thisDeviceDataPath + gameDataName, json);//書き込み
                Debug.Log("セッティングデータ　セーブ完了");
            }
        }

        public static void Load()
        {
            if (savePossible)
            {
                string path = thisDeviceDataPath + gameDataName;//パスの設定
                string json;
                if (File.Exists(path))//ファイルがあれば
                {
                    json = File.ReadAllText(path);//読み込み
                    GameData gameData = JsonUtility.FromJson<GameData>(json);//変換
                    playerName = gameData.playerName;
                    playerSkin = gameData.playerSkin;
                    Debug.Log("設定データ　ロード完了");
                }
                else
                {
                    Save();
                    Debug.Log("設定データ　セーブデータ 作成");
                }
            }
        }
    }

    class GameData
    {
        public string playerName;
        public int playerSkin;
    }
    #endregion

}
