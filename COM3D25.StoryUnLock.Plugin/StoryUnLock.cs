using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using COM3D2API;
using HarmonyLib;
using LillyUtill.MyMaidActive;
using LillyUtill.MyWindowRect;
using MaidStatus;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace COM3D2.StoryUnLock.Plugin
{
    public class MyAttribute
    {
        public const string PLAGIN_NAME = "StoryUnLock";
        public const string PLAGIN_VERSION = "22.3.5";
        public const string PLAGIN_FULL_NAME = "COM3D2.StoryUnLock.Plugin";
    }

    [BepInPlugin(MyAttribute.PLAGIN_FULL_NAME, MyAttribute.PLAGIN_NAME, MyAttribute.PLAGIN_VERSION)]// 버전 규칙 잇음. 반드시 2~4개의 숫자구성으로 해야함. 미준수시 못읽어들임
    //[BepInPlugin("COM3D2.Sample.Plugin", "COM3D2.Sample.Plugin", "21.6.6")]// 버전 규칙 잇음. 반드시 2~4개의 숫자구성으로 해야함. 미준수시 못읽어들임
    //[BepInProcess("COM3D2x64.exe")]
    public class StoryUnLock : BaseUnityPlugin
    {
        // 단축키 설정파일로 연동
        //private ConfigEntry<BepInEx.Configuration.KeyboardShortcut> ShowCounter;

        Harmony harmony;
        public static ManualLogSource Log;
        public static ConfigFile config;

        public static StoryUnLock sample;

        //public static MyLog myLog = new MyLog(MyAttribute.PLAGIN_NAME);


        public static ConfigEntry<bool> btnLock;
        public static ConfigEntry<bool> personalRandom;
        public static ConfigEntry<bool> statusAuto;
        public static ConfigEntry<bool> newMaid;
        public static ConfigEntry<bool> movMaid;

        public static bool rndPersonal = true;
        public static bool rndContract = false;
        public static int selGridPersonal = 0;
        public static int selGridContract = 0;

        public static string[] PersonalNames;//= new string[] { "radio1", "radio2", "radio3" };
        public static string[] ContractNames;//= new string[] { "radio1", "radio2", "radio3" };
        private int seleted;

        public static WindowRectUtill myWindowRect;

        public StoryUnLock()
        {
            sample = this;
            config = Config;
            Log = Logger;
        }

        /// <summary>
        ///  게임 실행시 한번만 실행됨
        /// </summary>
        public void Awake()
        {
            //StoryUnLock.myLog.LogMessage("Awake");

            StoryUnLockUtill.init(Config);

            // 단축키 기본값 설정
            //ShowCounter = Config.Bind("KeyboardShortcut", "KeyboardShortcut0", new BepInEx.Configuration.KeyboardShortcut(KeyCode.Alpha9, KeyCode.LeftControl));

            // 기어 메뉴 추가. 이 플러그인 기능 자체를 멈추려면 enabled 를 꺽어야함. 그러면 OnEnable(), OnDisable() 이 작동함
            btnLock = config.Bind("GUI", "btn Lock", false);
            personalRandom = config.Bind("AddStockMaid", "personalRandom", true);
            statusAuto = config.Bind("AddStockMaid", "_SetMaidStatusOnOff", false);
            newMaid = config.Bind("AddStockMaid", "newMaid", false);
            movMaid = config.Bind("AddStockMaid", "movMaid", false);

            ContractNames = new string[] { "Trainee", "Exclusive", "Free", "Random" };

            myWindowRect = new WindowRectUtill(Config, MyAttribute.PLAGIN_FULL_NAME, MyAttribute.PLAGIN_NAME, "SU");
        }



        public void OnEnable()
        {
            // StoryUnLock.myLog.LogMessage("OnEnable");

            SceneManager.sceneLoaded += this.OnSceneLoaded;

            // 하모니 패치
            harmony = Harmony.CreateAndPatchAll(typeof(StoryUnLockPatch));

        }


        /*
        */
        /// <summary>
        /// 게임 실행시 한번만 실행됨
        /// </summary>
        public void Start()
        {
            StoryUnLock.Log.LogMessage("Start");

            //SampleGUI.Install(gameObject, Config);

            //StoryUnLockGUI.Install<StoryUnLockGUI>(gameObject, Config, MyAttribute.PLAGIN_FULL_NAME, MyAttribute.PLAGIN_NAME, "SU", Properties.Resources.icon, new BepInEx.Configuration.KeyboardShortcut(KeyCode.Alpha9, KeyCode.LeftControl));

            
            SystemShortcutAPI.AddButton(
                MyAttribute.PLAGIN_FULL_NAME
                , new Action(delegate ()
                { // 기어메뉴 아이콘 클릭시 작동할 기능
                    myWindowRect.IsGUIOn = !myWindowRect.IsGUIOn;
                })
                , MyAttribute.PLAGIN_NAME // 표시될 툴팁 내용                               
                , Properties.Resources.icon);// 표시될 아이콘

            try
            {
                PersonalNames = Personal.GetAllDatas(true).Select((x) => x.uniqueName).ToArray();
            }
            catch (Exception e)
            {
                StoryUnLock.Log.LogError($"StoryUnLockGUI.Start , {PersonalNames.Length} , {e}");
            }
        }
        public static string scene_name = string.Empty;


        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            //Sample.myLog.LogMessage("OnSceneLoaded", scene.name, scene.buildIndex);
            //  scene.buildIndex 는 쓰지 말자 제발
            scene_name = scene.name;
        }
        private void OnGUI()
        {
            if (!myWindowRect.IsGUIOn)
                return;

            //GUI.skin.window = ;

            //myWindowRect.WindowRect = GUILayout.Window(windowId, myWindowRect.WindowRect, WindowFunction, MyAttribute.PLAGIN_NAME + " " + ShowCounter.Value.ToString(), GUI.skin.box);
            // 별도 창을 띄우고 WindowFunction 를 실행함. 이건 스킨 설정 부분인데 따로 공부할것
            myWindowRect.WindowRect = GUILayout.Window(myWindowRect.winNum, myWindowRect.WindowRect, WindowFunction, "", GUI.skin.box);
        }

        private Vector2 scrollPosition;

        private void WindowFunction(int id)
        {
            GUI.enabled = true; // 기능 클릭 가능

            GUILayout.BeginHorizontal();// 가로 정렬
            // 라벨 추가
            GUILayout.Label(myWindowRect.windowName, GUILayout.Height(20));
            // 안쓰는 공간이 생기더라도 다른 기능으로 꽉 채우지 않고 빈공간 만들기
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { myWindowRect.IsOpen = !myWindowRect.IsOpen; }
            if (GUILayout.Button("x", GUILayout.Width(20), GUILayout.Height(20))) { myWindowRect.IsGUIOn = false; }
            GUI.changed = false;

            GUILayout.EndHorizontal();// 가로 정렬 끝

            if (!myWindowRect.IsOpen)
            {

            }
            else
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);

                #region 여기에 내용 작성

                WindowFunctionBody(id);

                #endregion

                GUILayout.EndScrollView();
            }
            GUI.enabled = true;
            GUI.DragWindow(); // 창 드레그 가능하게 해줌. 마지막에만 넣어야함
        }

        private void WindowFunctionBody(int id)
        {
            GUILayout.Label("etc");
            if (GUILayout.Button("Maid Personal cnt")) StoryUnLockUtill.MaidPersonalCnt();

            GUILayout.Label("maid select");
            // 여기는 출력된 메이드들 이름만 가져옴
            // seleted 가 이름 위치 번호만 가져온건데
            seleted = MaidActiveUtill.SelectionGrid(seleted);

            if (GUILayout.Button("All Setting")) StoryUnLockUtill.SetMaidAll(seleted);
            if (GUILayout.Button("Main Story")) StoryUnLockUtill.SetMaidMainStory(seleted);


            GUILayout.Label("메이드 고용시");
            if (GUILayout.Button("Maid personal Random " + personalRandom.Value)) personalRandom.Value = !personalRandom.Value;
            if (GUILayout.Button("Maid cheat " + statusAuto.Value)) statusAuto.Value = !statusAuto.Value;

            GUILayout.Label("메이드 에딧 종료시 이벤트");
            if (GUILayout.Button("New Maid " + newMaid.Value)) newMaid.Value = !newMaid.Value;
            if (GUILayout.Button("Mov Maid " + movMaid.Value)) movMaid.Value = !movMaid.Value;


            //base.WindowFunctionBody(id);
            GUILayout.Label("All Maid Setting");

            if (GUILayout.Button("Set Slave")) StoryUnLockKs.SetSlaveStockMaids();
            if (GUILayout.Button("Set Married")) StoryUnLockKs.SetMarriedStockMaids();
            if (GUILayout.Button("Set PMDモード＿開始")) StoryUnLockKs.SetPMDStockMaids();
            if (GUILayout.Button("Work Setting")) StoryUnLockUtill.SetWorkAll();
            if (GUILayout.Button("EmpireLife Setting")) StoryUnLockUtill.SetEmpireLifeModeDataAll();
            if (GUILayout.Button("YotogiClass Setting")) StoryUnLockUtill.SetMaidYotogiClassAll();
            if (GUILayout.Button("JobClass Setting")) StoryUnLockUtill.SetMaidJobClassAll();
            if (GUILayout.Button("Maid skill Setting")) StoryUnLockUtill.SetMaidSkillAll();
            if (GUILayout.Button("MaidStatus Setting")) StoryUnLockUtill.SetMaidStatusAll();
            GUI.enabled = btnLock.Value;
            if (GUILayout.Button("Scenario Event End")) StoryUnLockUtill.ScenarioPlayAll();
            if (GUILayout.Button("Scenario Event End reset")) StoryUnLockUtill.SetScenarioDataAllReset();

            GUILayout.Label("All Maid Flag Remove");
            //if (GUILayout.Button("Remove ErrFlag")) StoryUnLockUtill.RemoveErrFlagAll();
            if (GUILayout.Button("Remove Flag")) StoryUnLockUtill.RemoveFlagAll();
            if (GUILayout.Button("Remove EventEndFlag")) StoryUnLockUtill.RemoveEventEndFlagAll();
            GUI.enabled = true;

            GUILayout.Label("Player");
            if (GUILayout.Button("FreeMode Flag Setting")) StoryUnLockUtill.SetFreeModeItemEverydayAll();
            if (GUILayout.Button("Yotogi Setting")) StoryUnLockUtill.SetYotogiAll(); // player
            if (GUILayout.Button("PlayerStatus Setting")) StoryUnLockUtill.SetAllPlayerStatus();

            GUILayout.Label("Scene Maid Management");
            //GUI.enabled = SceneManager.GetActiveScene().name == "SceneMaidManagement";
            GUI.enabled = StoryUnLock.scene_name == "SceneMaidManagement";
            if (GUILayout.Button("Select Maid All Setting")) StoryUnLockUtill.SetMaidAll(StoryUnLockPatch.selectMaid);
            //if (GUILayout.Button("선택 메이드 플레그 제거")) FlagUtill.RemoveEventEndFlag(true);
            //if (GUILayout.Button("HeroineType.Original")) StoryUnLockUtill.SetHeroineType(HeroineType.Original);
            //if (GUILayout.Button("HeroineType.Transfer")) StoryUnLockUtill.SetHeroineType(HeroineType.Transfer);
            GUI.enabled = true;

            GUILayout.Label("Maid Add");
            if (GUILayout.Button("Maid add")) StoryUnLockUtill.AddStockMaid();
            if (GUILayout.Button("Maid add * 10")) for (int i = 0; i < 10; i++) { StoryUnLockUtill.AddStockMaid(); }
            if (GUILayout.Button("Maid add * 50")) for (int i = 0; i < 50; i++) { StoryUnLockUtill.AddStockMaid(); }
            GUILayout.Label("Maid Add Personal");
            if (GUILayout.Button("Personal Rand " + rndPersonal)) rndPersonal = !rndPersonal;
            if (!rndPersonal)
            {
                selGridPersonal = GUILayout.SelectionGrid(selGridPersonal, PersonalNames, 3);
            }

            GUILayout.Label("Contract");
            if (GUILayout.Button("Contract Rand " + rndContract + " " + ContractNames[selGridContract])) rndContract = !rndContract;
            if (!rndContract)
            {
                selGridContract = GUILayout.SelectionGrid(selGridContract, ContractNames, 2);
            }
            /**/

        }


        public void OnDisable()
        {
            //StoryUnLock.myLog.LogMessage("OnDisable");

            SceneManager.sceneLoaded -= this.OnSceneLoaded;

            harmony.UnpatchSelf();// ==harmony.UnpatchAll(harmony.Id);
            //harmony.UnpatchAll(); // 정대 사용 금지. 다름 플러그인이 패치한것까지 다 풀려버림
        }
        /*
        public void Pause()
        {
            Sample.myLog.LogMessage("Pause");
        }

        public void Resume()
        {
            Sample.myLog.LogMessage("Resume");
        }
        */
        /*
        /// <summary>
        /// 게임 X 버튼 눌렀을때 반응
        /// </summary>
        public void OnApplicationQuit()
        {
            Sample.myLog.LogMessage("OnApplicationQuit");
        }
        */
    }
}
