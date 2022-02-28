using LillyUtill.MyPersonal;
using MaidStatus;
using MaidStatus.Old;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COM3D2.StoryUnLock.Plugin
{
    public class StoryUnLockKs
    {
        public static void SetPMDStockMaids()
        {
            // NTR_0002.ks
            foreach (var maid in GameMain.Instance.CharacterMgr.GetStockMaidList())
            {
                SetPMDStockMaid(maid);
            }
        }

        public static void SetPMDStockMaid(Maid maid)
        {
            if (GameUty.IsExistFile(maid.status.personal.replaceText + "_pmd_op_0005.ks"))
            {
                //StoryUnLock.Log.LogMessage($"Set PMD { maid.status.fullNameEnStyle }");
                maid.status.SetFlag("PMDモード進行フラグ１", 1);
                maid.status.SetEventEndFlag(5650, false);
                maid.status.SetFlag("PMDモード進行フラグ２", 1);
                maid.status.SetEventEndFlag(5670, false);
                maid.status.SetFlag("PMDモード＿イベントLV1開放", 1);
                //maid.status.SetFlag("PMDモード＿開始", 1);
                maid.status.SetFlag("PMDモードED通過", 1);

                scenarioDataEventEndFlag(maid,5640);
                scenarioDataEventEndFlag(maid,5650);
                scenarioDataEventEndFlag(maid,5660);
                scenarioDataEventEndFlag(maid,5670);
                scenarioDataEventEndFlag(maid,5680);
            }
            else
            {
                //StoryUnLock.Log.LogMessage($"No PMD { maid.status.fullNameEnStyle }");
            }
        }

        private static void scenarioDataEventEndFlag(Maid maid,int no)
        {
            var scenarioData = GameMain.Instance.ScenarioSelectMgr.GetScenarioData(no);
            scenarioData.GetEventMaidList().Remove(maid);
            maid.status.SetEventEndFlag(no, true);
        }

        public static void SetSlaveStockMaids()
        {
            // NTR_0002.ks
            foreach (var maid in GameMain.Instance.CharacterMgr.GetStockMaidList())
            {
                SetSlaveStockMaid(maid);
            }
        }

        public static void SetSlaveStockMaid(Maid maid)
        {
            if (GameUty.IsExistFile(maid.status.personal.replaceText + "_slave_0002.ks"))
            {
                StoryUnLock.Log.LogMessage($"Set Slave { maid.status.fullNameEnStyle }");
                if (GameMain.Instance.ScriptMgr.compatibilityMode || GameMain.Instance.ScriptMgr.tjsLegacyMode)
                    maid.status.OldStatus.relation = MaidStatus.Old.Relation.Slave;
                else
                    maid.status.additionalRelation = AdditionalRelation.Slave;
            }
            else
            {
                StoryUnLock.Log.LogMessage($"No Slave { maid.status.fullNameEnStyle }");
            }
        }

        public static void SetNTRStockMaids()
        {
            // NTR_0002.ks
            foreach (var maid in GameMain.Instance.CharacterMgr.GetStockMaidList())
            {
                if (GameUty.IsExistFile(maid.status.personal.replaceText + "_NTR_0006.ks"))
                {
                    StoryUnLock.Log.LogMessage($"Set NTR { maid.status.fullNameEnStyle }");
                    
                }
                else
                {
                    StoryUnLock.Log.LogMessage($"No NTR { maid.status.fullNameEnStyle }");
                }
            }
        }

        public static void SetMarriedStockMaids()
        {
            foreach (var maid in GameMain.Instance.CharacterMgr.GetStockMaidList())
            {
                SetMarriedStockMaid(maid);
            }
        }

        public static void SetMarriedStockMaid(Maid maid)
        {
            if (GameUty.IsExistFile(maid.status.personal.replaceText + "_marriage_0006.ks"))
            {
                if (GameMain.Instance.ScriptMgr.compatibilityMode || GameMain.Instance.ScriptMgr.tjsLegacyMode)
                    StoryUnLock.Log.LogMessage($"No Married  compatibilityMode or tjsLegacyMode");
                else
                {
                    StoryUnLock.Log.LogMessage($"SetMarried { maid.status.fullNameEnStyle }");
                    maid.status.SetFlag("GP02＿プロポーズ済", 1);
                    maid.status.specialRelation = SpecialRelation.Married;
                }
            }
            else
            {
                StoryUnLock.Log.LogMessage($"No Married { maid.status.fullNameEnStyle }");
            }
        }

        public static void EvalScriptStockMaid(string script)
        {
            foreach (var maid in GameMain.Instance.CharacterMgr.GetStockMaidList())
            {
                GameMain.Instance.ScriptMgr.EvalScript(script + ";");
            }
        }


        public static void SetMaidCondition(Maid maid, string a)
        {
            if (GameMain.Instance.ScriptMgr.compatibilityMode || GameMain.Instance.ScriptMgr.tjsLegacyMode)
            {
                if (a == "緊張")
                {
                    maid.status.OldStatus.relation = MaidStatus.Old.Relation.Tonus;
                }
                else if (a == "お近づき")
                {
                    maid.status.OldStatus.relation = MaidStatus.Old.Relation.Contact;
                }
                else if (a == "信頼")
                {
                    maid.status.OldStatus.relation = MaidStatus.Old.Relation.Trust;
                }
                else if (a == "恋人")
                {
                    maid.status.OldStatus.relation = MaidStatus.Old.Relation.Lover;
                }
                else if (a == "愛奴")
                {
                    maid.status.OldStatus.relation = MaidStatus.Old.Relation.Slave;
                }
                else if (a == "酔い")
                {
                    maid.status.OldStatus.condition = Condition.Drunk;
                }
                else if (a == "お仕置き")
                {
                    maid.status.OldStatus.condition = Condition.Osioki;
                }
                else if (a == "酔い解除" || a == "お仕置き解除")
                {
                    maid.status.OldStatus.condition = Condition.Null;
                }
            }
            else if (a == "お近づき")
            {
                maid.status.relation = MaidStatus.Relation.Contact;
            }
            else if (a == "信頼")
            {
                maid.status.relation = MaidStatus.Relation.Trust;
            }
            else if (a == "恋人")
            {
                maid.status.relation = MaidStatus.Relation.Lover;
            }
            else if (a == "警戒")
            {
                maid.status.additionalRelation = AdditionalRelation.Vigilance;
            }
            else if (a == "恋人+")
            {
                maid.status.additionalRelation = AdditionalRelation.LoverPlus;
            }
            else if (a == "愛奴")
            {
                maid.status.additionalRelation = AdditionalRelation.Slave;
            }
            else if (a == "Null")
            {
                maid.status.additionalRelation = AdditionalRelation.Null;
            }
            else if (a == "嫁")
            {
                maid.status.specialRelation = SpecialRelation.Married;
            }
            else if (a == "特殊関係解除")
            {
                maid.status.specialRelation = SpecialRelation.Null;
            }
        }
    }
}
