using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;//File����������

public class SaveData
{
    //�t�H���_
    static string windowsPlayerDataPath = Application.dataPath + "/../" + "/SaveData";
    static string androidDataPath = Application.persistentDataPath + "/SaveData";
    //�t�@�C��
    static string gameDataName = "/GameData.json";
    static string systemDataName = "/SystemData.json";

    /////////////////////////////////////////////////
    public static UnityEvent onImpossibleDevice = new UnityEvent();//�Z�[�u�s�\�Ȓ[���̂Ƃ�
    public static UnityEvent onComplateSave = new UnityEvent();//�Z�[�u�����������Ƃ�

    static string thisDeviceDataPath = null;
    static bool savePossible = false;//�Z�[�u���ł���f�o�C�X��

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
            Debug.Log("�Z�[�u�ΏۊO�f�o�C�X�ł�");
        }
    }

    static void CheckFile(string dataPath)
    {
        //�t�H���_���Ȃ�������쐬����
        if (!Directory.Exists(dataPath))
        {
            Directory.CreateDirectory(dataPath);
            Debug.Log("�Z�[�u�f�[�^�t�H���_���쐬");
        }
    }

    ///////////////////////////////////////////////////////////////////////
    #region System

    public class SystemSaveData//�ݒ�
    {
        [SerializeField]
        public static bool firstBoot = false;//�ŏ��̋N��
        [SerializeField]
        public static bool firstTutorial = false;//�ŏ��̃`���[�g���A���N��
        [SerializeField]
        public static string applicationVersion = Application.version;//�A�v���̃o�[�W����
        [SerializeField]
        public static bool fullScreen = true;//�t���X�N���[��
        [SerializeField]
        public static int screenSize = 2;//�𑜓x�ύX
        [SerializeField]
        public static float bgmVolume = 0.7f;//BGM�̉���
        [SerializeField]
        public static float seVolume = 1f;//���ʉ��̉���

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
                string json = JsonUtility.ToJson(systemData);//Json�ɕϊ�
                File.WriteAllText(thisDeviceDataPath + systemDataName, json);//��������
                Debug.Log("�Z�b�e�B���O�f�[�^�@�Z�[�u����");
            }
        }

        public static void Load()
        {
            if (savePossible)
            {
                string path = thisDeviceDataPath + systemDataName;//�p�X�̐ݒ�
                string json;
                if (File.Exists(path))//�t�@�C���������
                {
                    json = File.ReadAllText(path);//�ǂݍ���
                    SystemData systemData = JsonUtility.FromJson<SystemData>(json);//�ϊ�
                    firstBoot = systemData.firstBoot;
                    firstTutorial = systemData.firstTutorial;
                    applicationVersion = systemData.applicationVersion;
                    fullScreen = systemData.fullScreen;
                    screenSize = systemData.screenSize;
                    bgmVolume = systemData.bgmVolume;
                    seVolume = systemData.seVolume;
                    Debug.Log("�ݒ�f�[�^�@���[�h����");
                }
                else
                {
                    Save();
                    Debug.Log("�ݒ�f�[�^�@�Z�[�u�f�[�^ �쐬");
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

    public class GameSaveData//�ݒ�
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
                string json = JsonUtility.ToJson(gameData);//Json�ɕϊ�
                File.WriteAllText(thisDeviceDataPath + gameDataName, json);//��������
                Debug.Log("�Z�b�e�B���O�f�[�^�@�Z�[�u����");
            }
        }

        public static void Load()
        {
            if (savePossible)
            {
                string path = thisDeviceDataPath + gameDataName;//�p�X�̐ݒ�
                string json;
                if (File.Exists(path))//�t�@�C���������
                {
                    json = File.ReadAllText(path);//�ǂݍ���
                    GameData gameData = JsonUtility.FromJson<GameData>(json);//�ϊ�
                    playerName = gameData.playerName;
                    playerSkin = gameData.playerSkin;
                    Debug.Log("�ݒ�f�[�^�@���[�h����");
                }
                else
                {
                    Save();
                    Debug.Log("�ݒ�f�[�^�@�Z�[�u�f�[�^ �쐬");
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
