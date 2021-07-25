using FacilityFlag;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

    }
}
