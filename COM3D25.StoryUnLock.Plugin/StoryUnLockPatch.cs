
using FacilityFlag;
using HarmonyLib;
using MaidStatus;
using scoutmode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace COM3D2.StoryUnLock.Plugin
{
    class StoryUnLockPatch
    {
        public static Dictionary<string, DataArray<int, byte>> m_SaveDataMaidScenarioExecuteCountArray;//= new Dictionary<string, DataArray<int, byte>>();
        public static DataArray<int, byte> m_SaveDataScenarioExecuteCountArray;// = new DataArray<int, byte>();

        [HarmonyPatch(typeof(EmpireLifeModeManager), MethodType.Constructor)]
        [HarmonyPostfix]//HarmonyPostfix ,HarmonyPrefix
        public static void Constructor(
            DataArray<int, byte> ___m_SaveDataScenarioExecuteCountArray
            , Dictionary<string, DataArray<int, byte>> ___m_SaveDataMaidScenarioExecuteCountArray
            )
        {
            m_SaveDataScenarioExecuteCountArray = ___m_SaveDataScenarioExecuteCountArray;
            m_SaveDataMaidScenarioExecuteCountArray = ___m_SaveDataMaidScenarioExecuteCountArray;
        }


        public static Maid selectMaid;

        /// <summary>
        /// 메이드 관리에서 모든 버튼 활성화
        /// </summary>
        /// <param name="___select_maid_"></param>
        /// <param name="___button_dic_"></param>
        /// <param name="__instance"></param>
        [HarmonyPostfix, HarmonyPatch(typeof(MaidManagementMain), "OnSelectChara")]
        public static void OnSelectChara(Maid ___select_maid_, Dictionary<string, UIButton> ___button_dic_, MaidManagementMain __instance)
        {
            selectMaid = ___select_maid_;

            foreach (var item in ___button_dic_)
            {
                item.Value.isEnabled = true;
            }
        }

        /// <summary>
        /// 에딧 종료
        /// </summary>
        /// <param name="___m_strScriptArg"></param>
        /// <param name="___m_maid"></param>
        [HarmonyPatch(typeof(SceneEdit), "OnEndScene")]
        [HarmonyPrefix]
        public static void OnEndScene(string ___m_strScriptArg, Maid ___m_maid)
        {
            // MaidManagementMain.OnSelectChara: , A1 , 구 메이드 비서   , Sub , Exclusive
            if (___m_maid.status.heroineType == MaidStatus.HeroineType.Sub)
            {
                return;
            }
            //StoryUnLock.myLog.LogMessage("SceneEdit.OnEndScene");
            if (StoryUnLock.newMaid.Value)
            {
                GameMain.Instance.CMSystem.SetTmpGenericFlag("新規雇用メイド", 1);
            }
            else if (StoryUnLock.movMaid.Value)
            {
                GameMain.Instance.CMSystem.SetTmpGenericFlag("移籍メイド", 1);
            }
        }

        private static bool isNewMaid;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ScoutMainScreenManager), "AddScoutMaid")]
        [HarmonyPatch(typeof(MaidManagementMain), "Employment")]
        public static void NewMaid()
        {
            isNewMaid = true;
        }

        [HarmonyPatch(typeof(SceneEdit), "OnCompleteFadeIn")]
        [HarmonyPostfix]
        public static void OnCompleteFadeIn(Maid ___m_maid) // Maid ___m_maid,SceneEdit __instance
        {
            if (!isNewMaid)
            {
                return;
            }
            if (StoryUnLock.personalRandom.Value)
            {
                SetPersonalRandom(___m_maid);
            }
            if (StoryUnLock.statusAuto.Value)
            {
                //StoryUnLock.myLog.LogMessage("SceneEdit.OnCompleteFadeIn");
                //GameMain.Instance.CharacterMgr.GetMaid(0);
                
                StoryUnLockUtill.SetMaidStatus(___m_maid);
                StoryUnLockUtill.SetMaidYotogiClass(___m_maid);
                StoryUnLockUtill.SetMaidJobClass(___m_maid);
                StoryUnLockUtill.SetMaidSkill(___m_maid);
            }
            isNewMaid = false;
        }

        public static void SetPersonalRandom(Maid maid)
        {
            if (maid is null)
            {
                return;
            }
            var p = Personal.GetAllDatas(true);
            int a = UnityEngine.Random.Range(0, p.Count);
            Personal.Data data = p[a];
            maid.status.SetPersonal(data);
            maid.status.firstName = data.uniqueName;
            return ;
        }
    }
}
