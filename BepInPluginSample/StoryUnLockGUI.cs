using BepInEx.Configuration;
using COM3D2.LillyUtill;
using MaidStatus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COM3D2.StoryUnLock.Plugin
{
    class StoryUnLockGUI : MyGUI
    {
        public static ConfigEntry<bool> btnLock;
        public static ConfigEntry<bool> statusAuto;
        public static ConfigEntry<bool> newMaid;
        public static ConfigEntry<bool> movMaid;

        /*
        public override void Awake()
        {
            base.Awake();

            
        }

        public override bool Equals(object other)
        {
            return base.Equals(other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }

        public override void OnEnable()
        {
            base.OnEnable();
        }

        public override void OnGUI()
        {
            base.OnGUI();
        }

        public override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            base.OnSceneLoaded(scene, mode);
        }
*/



        public override void Start()
        {
            base.Start();

            btnLock = config.Bind("GUI", "btn Lock", false);
            statusAuto = config.Bind("AddStockMaid", "_SetMaidStatusOnOff", true);
            newMaid = config.Bind("AddStockMaid", "newMaid", true);
            movMaid = config.Bind("AddStockMaid", "movMaid", true);

            try
            {
                PersonalNames = PersonalUtill.GetPersonalData().Select((x) => x.uniqueName).ToArray();
            }
            catch (Exception e)
            {
                PersonalNames = Personal.GetAllDatas(true).Select((x) => x.uniqueName).ToArray();
                StoryUnLock.myLog.LogError("StoryUnLockGUI.Start", PersonalNames.Length, e.ToString());
            }

            ContractNames = new string[] { "Trainee", "Exclusive", "Free", "Random" };


        }
        /*
        public override string ToString()
        {
            return base.ToString();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void WindowFunction(int id)
        {
            base.WindowFunction(id);
        }
        */
        public override void WindowFunctionBody(int id)
        {
            GUILayout.Label("etc");
            if (GUILayout.Button("Maid Personal cnt")) StoryUnLockUtill.MaidPersonalCnt();

            GUILayout.Label("maid select");
            // 여기는 출력된 메이드들 이름만 가져옴
            // seleted 가 이름 위치 번호만 가져온건데
            seleted = MaidActivePatch.SelectionGrid3(seleted);

            if (GUILayout.Button("Maid Setting")) StoryUnLockUtill.SetMaidStatusAll(seleted);

            if (GUILayout.Button("Maid cheat " + statusAuto.Value)) statusAuto.Value = !statusAuto.Value;

            GUILayout.Label("메이드 에딧 종료시 이벤트");
            if (GUILayout.Button("New Maid " + newMaid.Value)) newMaid.Value = !newMaid.Value;
            if (GUILayout.Button("Mov Maid " + movMaid.Value)) movMaid.Value = !movMaid.Value;


            //base.WindowFunctionBody(id);
            GUILayout.Label("All Maid Setting");

            GUI.enabled = btnLock.Value;
            if (GUILayout.Button("Work Setting")) StoryUnLockUtill.SetWorkAll();
            if (GUILayout.Button("Scenario Setting")) StoryUnLockUtill.SetScenarioDataAll();
            if (GUILayout.Button("EmpireLife Setting")) StoryUnLockUtill.SetEmpireLifeModeDataAll();
            if (GUILayout.Button("JobClass Setting")) StoryUnLockUtill.SetMaidJobClassAll();
            if (GUILayout.Button("YotogiClass Setting")) StoryUnLockUtill.SetMaidYotogiClassAll();
            if (GUILayout.Button("Maid skill Setting")) StoryUnLockUtill.SetMaidSkillAll();
            GUI.enabled = true;
            if (GUILayout.Button("MaidStatus Setting")) StoryUnLockUtill.SetMaidStatusAll();

            GUILayout.Label("All Maid Flag Remove");
            //if (GUILayout.Button("Remove ErrFlag")) StoryUnLockUtill.RemoveErrFlagAll();
            if (GUILayout.Button("Remove Flag")) StoryUnLockUtill.RemoveFlagAll();
            if (GUILayout.Button("Remove EventEndFlag")) StoryUnLockUtill.RemoveEventEndFlagAll();

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

        public static bool rndPersonal = true;
        public static bool rndContract = false;
        public static int selGridPersonal = 0;
        public static int selGridContract = 0;

        public static string[] PersonalNames;//= new string[] { "radio1", "radio2", "radio3" };
        public static string[] ContractNames;//= new string[] { "radio1", "radio2", "radio3" };
        private int seleted;
    }

}
