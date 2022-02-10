using BepInEx.Configuration;
using COM3D2.LillyUtill;
using FacilityFlag;
using MaidStatus;
using MaidStatus.CsvData;
using PlayerStatus;
using Schedule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wf;
using Yotogis;

namespace COM3D2.StoryUnLock.Plugin
{
    class StoryUnLockUtill
    {
        internal static void init(ConfigFile config)
        {
            _SetMaidStatusOnOff = config.Bind(
            "EasyUtill",
            "_SetMaidStatusOnOff",
            true
            );
        }

        static bool isSetAllWorkRun = false;

        internal static void SetWorkAll()
        {

            if (isSetAllWorkRun)
                return;

            Task.Factory.StartNew(() =>
            {
                isSetAllWorkRun = true;
                StoryUnLock.myLog.LogDarkBlue("ScheduleAPIPatch.SetAllWork. start");

                ReadOnlyDictionary<int, NightWorkState> night_works_state_dic = GameMain.Instance.CharacterMgr.status.night_works_state_dic;
                StoryUnLock.myLog.LogMessage("ScheduleAPIPatch.SetAllWork.night_works_state_dic:" + night_works_state_dic.Count);

                foreach (var item in night_works_state_dic)
                {
                    NightWorkState nightWorkState = item.Value;
                    nightWorkState.finish = true;
                }

                StoryUnLock.myLog.LogMessage("ScheduleAPIPatch.SetAllWork.YotogiData:" + ScheduleCSVData.YotogiData.Values.Count);
                foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList())
                {
                    StoryUnLock.myLog.LogMessage(".SetAllWork.Yotogi:" + maid.status.fullNameEnStyle, ScheduleCSVData.YotogiData.Values.Count);
                    if (maid.status.heroineType == HeroineType.Sub)
                        continue;


                    if (maid.boNPC || maid.boMAN)
                        continue;


                    foreach (ScheduleCSVData.Yotogi yotogi in ScheduleCSVData.YotogiData.Values)
                    {

                        if (DailyMgr.IsLegacy)
                        {
                            maid.status.OldStatus.SetFlag("_PlayedNightWorkId" + yotogi.id, 1);
                        }
                        else
                        {
                            maid.status.SetFlag("_PlayedNightWorkId" + yotogi.id, 1);
                        }
                        if (yotogi.condFlag1.Count > 0)
                        {
                            for (int n = 0; n < yotogi.condFlag1.Count; n++)
                            {
                                maid.status.SetFlag(yotogi.condFlag1[n], 1);
                            }
                        }
                        if (yotogi.condFlag0.Count > 0)
                        {
                            for (int num = 0; num < yotogi.condFlag0.Count; num++)
                            {
                                maid.status.SetFlag(yotogi.condFlag0[num], 0);
                            }
                        }
                    }
                    if (DailyMgr.IsLegacy)
                    {
                        maid.status.OldStatus.SetFlag("_PlayedNightWorkVip", 1);
                    }
                    else
                    {
                        maid.status.SetFlag("_PlayedNightWorkVip", 1);
                    }
                }

                StoryUnLock.myLog.LogDarkBlue("ScheduleAPIPatch.SetAllWork. end");
                isSetAllWorkRun = false;
            });

        }


        static bool isRunSetScenarioDataAll = false;

        internal static void SetScenarioDataAll()
        {

            if (isRunSetScenarioDataAll)
                return;

            Task.Factory.StartNew(() =>
            {
                isRunSetScenarioDataAll = true;

                StoryUnLock.myLog.LogDarkBlue("SetScenarioDataAll. start");

                // 병렬 처리
                foreach (var scenarioData in GameMain.Instance.ScenarioSelectMgr.GetAllScenarioData())
                {

                    // MyLog.LogMessageS(".SetScenarioDataAll:" + scenarioData.ID + " , " + scenarioData.ScenarioScript + " , " + scenarioData.IsPlayable + " , " + scenarioData.Title); ;
                    if (scenarioData.IsPlayable)
                    {
                        foreach (var maid in scenarioData.GetEventMaidList())
                        {
                            if (maid.status.heroineType == HeroineType.Sub)                            
                                continue;

                            if (maid.boNPC || maid.boMAN)
                                continue;

                            bool b = maid.status.GetEventEndFlag(scenarioData.ID);
                            if (!b)
                            {
                                maid.status.SetEventEndFlag(scenarioData.ID, true);
                                if (scenarioData.ScenarioScript.Contains("_Marriage"))
                                {
                                    maid.status.specialRelation = SpecialRelation.Married;
                                    maid.status.relation = Relation.Lover;
                                    maid.status.OldStatus.isMarriage = true;
                                    maid.status.OldStatus.isNewWife = true;
                                }
                            }

                        }
                    }
                    try
                    {
                    }
                    catch (Exception e)
                    {
                        StoryUnLock.myLog.LogError("ScenarioDataUtill.SetScenarioDataAll2 : " + e.ToString());
                    }
                }
                try
                {
                }
                catch (Exception e)
                {
                    StoryUnLock.myLog.LogError("ScenarioDataUtill.SetScenarioDataAll1 : " + e.ToString());
                }

                StoryUnLock.myLog.LogDarkBlue("SetScenarioDataAll. end");

                isRunSetScenarioDataAll = false;
            });
        }



        internal static void MaidPersonalCnt()
        {            
            Dictionary<int, int> d = new Dictionary<int, int>();
            foreach (var item in Personal.GetAllDatas(true))
            {
                d.Add(item.id, 0);
            }

            foreach (var maid in GameMain.Instance.CharacterMgr.GetStockMaidList())
            {
                if (maid.boMAN || maid.boNPC || maid.status.heroineType == HeroineType.Sub)
                {
                    continue;
                }
                d[maid.status.personal.id]++;                
            }

            foreach (var item in Personal.GetAllDatas(true))
            {
                StoryUnLock.myLog.LogMessage("ScenarioDataUtill.MaidPersonalCnt " 
                    , item.id
                    , item.replaceText
                    , item.uniqueName
                    , d[item.id]
                    );
                ;
            }
        }

        static bool isScenarioExecuteCountAllRun = false;

        /// <summary>
        /// 피들러 참고. 이숫자 대체 어디서 들고오는거야
        /// </summary>
        public static void SetEmpireLifeModeDataAll()
        {

            if (!isScenarioExecuteCountAllRun)
            {
                Task.Factory.StartNew(() =>
                {
                    StoryUnLock.myLog.LogDarkBlue("SetScenarioExecuteCountAll. start");
                    isScenarioExecuteCountAllRun = true;

                    foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList())
                    {
                        if (maid.status.heroineType == HeroineType.Sub)
                            continue;

                        if (maid.boNPC || maid.boMAN)
                            continue;

                        foreach (var data in EmpireLifeModeData.GetAllDatas(true))
                        {
                            //try
                            //{
                            int cnt = GameMain.Instance.LifeModeMgr.GetScenarioExecuteCount(data.ID);
                            if (cnt >= 255)
                                continue;

                            IncrementMaidScenarioExecuteCount(data.ID, maid);


                        }
                    }

                    isScenarioExecuteCountAllRun = false;
                    StoryUnLock.myLog.LogDarkBlue("SetScenarioExecuteCountAll. end");
                });
            }
        }

        public static void IncrementMaidScenarioExecuteCount(int eventID, Maid maid)
        {
            if (maid == null)
            {
                return;
            }
            //NDebug.AssertNull(maid);
            //string guid = maid.status.guid;
            DataArray<int, byte> dataArray = CreateMaidDataArray(maid);
            byte b = dataArray.Get(eventID, false);
            b = 255;
            //if (b < 255)
            //{
            //    b += 1;
            //}
            dataArray.Add(eventID, b, true);
            StoryUnLockPatch.m_SaveDataScenarioExecuteCountArray.Add(eventID, b, true);
        }

        public static DataArray<int, byte> CreateMaidDataArray(Maid maid)
        {
            NDebug.AssertNull(maid);
            string guid = maid.status.guid;
            if (StoryUnLockPatch.m_SaveDataMaidScenarioExecuteCountArray.ContainsKey(guid))
            {
                return StoryUnLockPatch.m_SaveDataMaidScenarioExecuteCountArray[guid];
            }
            DataArray<int, byte> dataArray = new DataArray<int, byte>();
            StoryUnLockPatch.m_SaveDataMaidScenarioExecuteCountArray.Add(guid, dataArray);
            /*
            Debug.LogFormat("メイド「{0}」の情報を作成しました。", new object[]
            {
            maid.status.fullNameJpStyle
            });*/
            return dataArray;
        }

        public static void SetMaidJobClassAll()
        {
            StoryUnLock.myLog.LogDarkBlue("SetMaidJobClassAll. start");


            foreach (var maid in GameMain.Instance.CharacterMgr.GetStockMaidList())
            {
                if (maid.status.heroineType == HeroineType.Sub)
                    continue;

                if (maid.boNPC || maid.boMAN)
                    continue;

                SetMaidJobClass(maid);
            }

            StoryUnLock.myLog.LogDarkBlue("SetMaidJobClassAll. end");
        }


        public static void SetMaidJobClass(Maid maid)
        {

            #region JobClass

            JobClassSystem jobClassSystem = maid.status.jobClass;
            List<JobClass.Data> learnPossibleClassDatas = jobClassSystem.GetLearnPossibleClassDatas(true, AbstractClassData.ClassType.Share | AbstractClassData.ClassType.New | AbstractClassData.ClassType.Old);


            foreach (JobClass.Data data in learnPossibleClassDatas)
                jobClassSystem.Add(data, true, true);

            var jobClassSystems = jobClassSystem.GetAllDatas().Values;// old 데이터 포함

            SetExpMax(jobClassSystems.Select(x => x.expSystem));

            #endregion
        }

        public static void SetExpMax(IEnumerable<SimpleExperienceSystem> expSystems)
        {
            //int c = 0;
            foreach (var expSystem in expSystems)
            {
                expSystem.SetLevel(expSystem.GetMaxLevel());
                //   c++;
            }
            //MyLog.LogMessage("SetExpMax : " + c);
        }

        public static void SetMaidYotogiClassAll()
        {
            StoryUnLock.myLog.LogDarkBlue("SetMaidYotogiClassAll. start");

            foreach (var maid in GameMain.Instance.CharacterMgr.GetStockMaidList())
            {
                if (maid.status.heroineType == HeroineType.Sub || maid.boNPC || maid.boMAN)
                    continue;

                SetMaidYotogiClass(maid);
            }

            StoryUnLock.myLog.LogDarkBlue("SetMaidYotogiClassAll. start");
        }

        public static void SetMaidYotogiClass(Maid maid)
        {
            StoryUnLock.myLog.LogDarkBlue("SetMaidYotogiClass. start");

            #region YotogiClass

            YotogiClassSystem yotogiClassSystem = maid.status.yotogiClass;
            List<YotogiClass.Data> learnPossibleYotogiClassDatas = yotogiClassSystem.GetLearnPossibleClassDatas(true, AbstractClassData.ClassType.Share | AbstractClassData.ClassType.New | AbstractClassData.ClassType.Old);

            StoryUnLock.myLog.LogMessage("SetMaidStatus.YotogiClass learn", maid.status.fullNameEnStyle, learnPossibleYotogiClassDatas.Count);
            foreach (YotogiClass.Data data in learnPossibleYotogiClassDatas)
                maid.status.yotogiClass.Add(data, true, true);

            var yotogiClassSystems = yotogiClassSystem.GetAllDatas().Values;
            StoryUnLock.myLog.LogMessage("SetMaidStatus.YotogiClass expSystem", maid.status.fullNameEnStyle, yotogiClassSystems.Count);
            SetExpMax(yotogiClassSystems.Select(x => x.expSystem));

            #endregion

            StoryUnLock.myLog.LogDarkBlue("SetMaidYotogiClass. end");
        }

        public static void SetMaidSkillAll()
        {
            StoryUnLock.myLog.LogDarkBlue("SetMaidSkillAll. start");

            foreach (var maid in GameMain.Instance.CharacterMgr.GetStockMaidList())
            {
                if (maid.status.heroineType == HeroineType.Sub || maid.boNPC || maid.boMAN)
                    continue;

                SetMaidSkill(maid);
            }

            StoryUnLock.myLog.LogDarkBlue("SetMaidSkillAll. end");
        }

        public static void SetMaidSkill(Maid maid)
        {
            #region 스킬 영역

            List<Skill.Data> learnPossibleSkills = Skill.GetLearnPossibleSkills(maid.status);

            foreach (Skill.Data data in learnPossibleSkills)
                maid.status.yotogiSkill.Add(data);

            SetExpMax(maid.status.yotogiSkill.datas.GetValueArray().Select(x => x.expSystem));


            List<Skill.Old.Data> learnPossibleOldSkills = Skill.Old.GetLearnPossibleSkills(maid.status);

            foreach (Skill.Old.Data data in learnPossibleOldSkills)
                maid.status.yotogiSkill.Add(data);

            SetExpMax(maid.status.yotogiSkill.oldDatas.GetValueArray().Select(x => x.expSystem));

            #endregion
        }

        public static void SetMaidStatusAll()
        {
            StoryUnLock.myLog.LogDarkBlue("MaidStatusUtill.SetMaidStatusAll. start");

            foreach (var maid in GameMain.Instance.CharacterMgr.GetStockMaidList())
            {

            }

            StoryUnLock.myLog.LogDarkBlue("MaidStatusUtill.SetMaidStatusAll. end");
        }

        internal static void SetMaidStatusAll(int seleted)
        {
            Maid maid = MaidActivePatch.GetMaid(seleted);

            if (maid==null||maid.status.heroineType == HeroineType.Sub || maid.boNPC || maid.boMAN)
                return;

            SetMaidStatus(maid);
        }

        public static void SetMaidStatus(Maid maid)
        {
            if (maid == null)
            {
                StoryUnLock.myLog.LogFatal("MaidStatusUtill.SetMaidStatus:null");
                return;
            }
            StoryUnLock.myLog.LogMessage("SetMaidStatus : " + maid.status.fullNameEnStyle);

            maid.status.employmentDay = 1;// 고용기간

            maid.status.baseAppealPoint = 9999;
            maid.status.baseCare = 9999;
            maid.status.baseCharm = 9999;
            maid.status.baseCooking = 9999;
            maid.status.baseDance = 9999;
            maid.status.baseElegance = 9999;
            maid.status.baseHentai = 9999;
            maid.status.baseHousi = 9999;
            maid.status.baseInyoku = 9999;
            maid.status.baseLovely = 9999;
            maid.status.baseMaxHp = 9999;
            maid.status.baseMaxMind = 9999;
            maid.status.baseMaxReason = 9999;
            maid.status.baseMvalue = 9999;
            maid.status.baseReception = 9999;
            maid.status.baseTeachRate = 9999;
            maid.status.baseVocal = 9999;

            maid.status.studyRate = 0;   // 습득율
            maid.status.likability = 999;// 호감도

            if (maid.boNPC)
            {

            }

            //if (true)
            //{
            //    //maid.status.contract = Contract.;// 적용 방식 고민 필요
            //}

            //maid.status.specialRelation = SpecialRelation.Married;// 되는건가?
            maid.status.additionalRelation = AdditionalRelation.Slave;// 되는건가?

            //maid.status.heroineType = HeroineType.Original;// 기본, 엑스트라 , 이전 // 사용 금지.일반 메이드를 엑스트라로 하면 꼬인다. 반대도 마찬가지
            maid.status.relation = Relation.Lover;// 호감도
            maid.status.seikeiken = Seikeiken.Yes_Yes;// 


            // 특징
            StoryUnLock.myLog.LogMessage("SetMaidStatus.AddFeature: " + maid.status.fullNameEnStyle);
            foreach (Feature.Data data in Feature.GetAllDatas(true))
                maid.status.AddFeature(data);


            // 성벽
            StoryUnLock.myLog.LogMessage("SetMaidStatus.AddPropensity: " + maid.status.fullNameEnStyle);
            foreach (Propensity.Data data in Propensity.GetAllDatas(true))
                maid.status.AddPropensity(data);


            StoryUnLock.myLog.LogMessage("SetMaidStatus.WorkData max : " + maid.status.fullNameEnStyle, maid.status.workDatas.Count);
            foreach (WorkData workData in maid.status.workDatas.GetValueArray())
            {
                workData.level = 10;
                workData.playCount = 9999U;
            }


        }

        /*
        public static void RemoveErrFlagAll()
        {
            ReadCSVAll();
            foreach (var maid in GameMain.Instance.CharacterMgr.GetStockMaidList())
            {
                foreach (var item in eventEndFlags)
                {
                    maid.status.RemoveEventEndFlag(item.Key);
                }
                foreach (var item in flags)
                {
                    maid.status.RemoveFlag(item.Key);
                }
                foreach (var item in oldflags)
                {
                    maid.status.OldStatus.RemoveFlag(item.Key);
                }
            }

        }
        public static Dictionary<string, int> oldflags = new Dictionary<string, int>();
        public static Dictionary<string, int> flags = new Dictionary<string, int>();
        public static Dictionary<int, bool> eventEndFlags = new Dictionary<int, bool>();

        private static void ReadCSVAll()
        {
            ReadCSV("oldflags", ref oldflags);
            ReadCSV("flags", ref flags);
            ReadCSV("eventEndFlags", ref eventEndFlags);
        }

        public static void ReadCSV(string file, ref Dictionary<string, int> list)
        {
            string filePath = Path.GetDirectoryName(StoryUnLock.config.ConfigFilePath) + "/" + file + ".csv";
            if (!File.Exists(filePath))
            {
                StoryUnLock.myLog.LogMessage("ReadCSV make : " + filePath);

                using StreamWriter sw = File.CreateText(filePath);
                sw.WriteLine("# flag name,int value");
                sw.WriteLine("// キャラクターパックEXメイド,0");
                return;
            }
            list.Clear();
            using StreamReader sr = new StreamReader(filePath);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                StoryUnLock.myLog.LogDebug("ReadCSV read : " + line);
                if (line.StartsWith("//") || line.StartsWith("#"))
                {
                    continue;
                }
                var values = line.Split(',');
                if (int.TryParse(values[1], out int j))
                //if (list.ContainsKey(values[0]))
                //{
                //    list[values[0]] = j;
                //}
                //else
                {
                    list.Add(values[0], j);
                }
                else
                    StoryUnLock.myLog.LogWarning("ReadCSV error : " + line);
            }
        }

        public static void ReadCSV(string file, ref Dictionary<int, bool> list)
        {
            string filePath = Path.GetDirectoryName(Lilly.customFile.ConfigFilePath) + "/" + file + ".csv";
            if (!File.Exists(filePath))
            {
                StoryUnLock.myLog.LogMessage("ReadCSV make : " + filePath);

                using StreamWriter sw = File.CreateText(filePath);
                sw.WriteLine("# flag id,bool value");
                sw.WriteLine("// flag id,bool value");
                sw.WriteLine("// 1," + false);
                sw.WriteLine("// 2," + true);
                return;
            }
            list.Clear();
            using StreamReader sr = new StreamReader(filePath);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                StoryUnLock.myLog.LogDebug("ReadCSV read : " + line);
                if (line.StartsWith("//") || line.StartsWith("#"))
                {
                    continue;
                }
                var values = line.Split(',');
                if (int.TryParse(values[0], out int j) && bool.TryParse(values[1], out bool k))
                //if (list.ContainsKey(j))
                //{
                //    list[j] = k;
                //}
                //else
                {
                    list.Add(j, k);
                }

                else
                    StoryUnLock.myLog.LogWarning("ReadCSV error : " + line);
            }
        }
        */

        public static void RemoveFlagAll()
        {
            foreach (var maid in GameMain.Instance.CharacterMgr.GetStockMaidList())
            {
                StoryUnLock.myLog.LogMessage("RemoveEventEndFlagAll:" + maid.status.fullNameEnStyle); ;
                maid.status.RemoveFlagAll();
            }
        }

        public static void RemoveEventEndFlagAll()
        {
            foreach (var maid in GameMain.Instance.CharacterMgr.GetStockMaidList())
            {
                StoryUnLock.myLog.LogMessage("RemoveEventEndFlagAll:" + maid.status.fullNameEnStyle); ;
                maid.status.RemoveEventEndFlagAll();
            }
        }

        internal static void SetFreeModeItemEverydayAll()
        {
            StoryUnLock.myLog.LogDarkBlue("ScenarioDataUtill.SetScenarioAll. start");

            SetEveryday(FreeModeItemEveryday.ScnearioType.Nitijyou);
            StoryUnLock.myLog.LogInfo("ScenarioDataUtill.SetScenarioAll. Nitijyou end");

            SetEveryday(FreeModeItemEveryday.ScnearioType.Story);
            StoryUnLock.myLog.LogInfo("ScenarioDataUtill.SetScenarioAll. Story emd");

            StoryUnLock.myLog.LogDarkBlue("ScenarioDataUtill.SetScenarioAll. end");
        }

        private static void SetEveryday(FreeModeItemEveryday.ScnearioType type)
        {
            string fileName = string.Empty;
            string fixingFlagText;
            if (type == FreeModeItemEveryday.ScnearioType.Nitijyou)
            {
                fileName = "recollection_normal2.nei";
                fixingFlagText = "シーン鑑賞_一般_フラグ_";
            }
            else
            {
                if (type != FreeModeItemEveryday.ScnearioType.Story)
                {
                    return;
                }
                fileName = "recollection_story.nei";
                fixingFlagText = "シーン鑑賞_メイン_フラグ_";
            }
            SetEverydaySub(type, fileName, AbstractFreeModeItem.GameMode.COM3D, fixingFlagText);
            if (GameUty.IsEnabledCompatibilityMode && type == FreeModeItemEveryday.ScnearioType.Nitijyou)
            {
                SetEverydaySub(type, fileName, AbstractFreeModeItem.GameMode.CM3D2, fixingFlagText);
            }
        }

        private static void SetEverydaySub(FreeModeItemEveryday.ScnearioType type, string fileName, AbstractFreeModeItem.GameMode gameMode, string fixingFlagText)
        {
            AFileBase afileBase;
            if (gameMode == AbstractFreeModeItem.GameMode.CM3D2)
            {
                afileBase = GameUty.FileSystemOld.FileOpen(fileName);
            }
            else
            {
                if (gameMode != AbstractFreeModeItem.GameMode.COM3D)
                {
                    return;
                }
                afileBase = GameUty.FileSystem.FileOpen(fileName);
            }
            using (afileBase)
            {
                using (CsvParser csvParser = new CsvParser())
                {
                    bool condition = csvParser.Open(afileBase);
                    NDebug.Assert(condition, fileName + "\nopen failed.");
                    for (int i = 1; i < csvParser.max_cell_y; i++)
                    {
                        if (csvParser.IsCellToExistData(0, i))
                        {
                            int cellAsInteger = csvParser.GetCellAsInteger(0, i);

                            int num = 1;
                            if (gameMode != AbstractFreeModeItem.GameMode.CM3D2 || type != FreeModeItemEveryday.ScnearioType.Nitijyou)
                            {
                                string name = csvParser.GetCellAsString(num++, i);
                                string call_file_name = csvParser.GetCellAsString(num++, i);
                                string check_flag_name = csvParser.GetCellAsString(num++, i);
                                if (gameMode == AbstractFreeModeItem.GameMode.COM3D)
                                {
                                    bool netorare = (csvParser.GetCellAsString(num++, i) == "○");
                                }
                                string info_text = csvParser.GetCellAsString(num++, i);
                                List<string> list = new List<string>();
                                for (int j = 0; j < 9; j++)
                                {
                                    if (csvParser.IsCellToExistData(num, i))
                                    {
                                        list.Add(csvParser.GetCellAsString(num, i));
                                    }
                                    num++;
                                }
                                int subHerionID = csvParser.GetCellAsInteger(num++, i);
                                while (csvParser.IsCellToExistData(num, 0))
                                {
                                    if (csvParser.GetCellAsString(num, i) == "○")
                                    {
                                        string cellAsString = csvParser.GetCellAsString(num, 0);
                                        //Personal.Data data = Personal.GetData(cellAsString);
                                    }
                                    num++;
                                }

                                if (GameMain.Instance.CharacterMgr.status.GetFlag(fixingFlagText + check_flag_name) == 0)
                                {
                                    StoryUnLock.myLog.LogMessage("SetEverydaySub.Flag"
                                    , check_flag_name
                                    , call_file_name
                                    , cellAsInteger
                                    , name
                                    , info_text
                                    );
                                    GameMain.Instance.CharacterMgr.status.SetFlag(fixingFlagText + check_flag_name, 1);
                                }

                            }
                        }

                    }
                }
            }
        }

        internal static void SetYotogiAll()
        {
            StoryUnLock.myLog.LogDarkBlue("SetAllYotogi START");

            foreach (var item in ScheduleCSVData.YotogiData)
            {
                ScheduleCSVData.Yotogi yotogi = item.Value;
                StoryUnLock.myLog.LogMessage("SetScenarioAll.YotogiData", item.Key, yotogi.yotogiType);
                if (yotogi.condManVisibleFlag1.Count > 0)
                {
                    for (int j = 0; j < yotogi.condManVisibleFlag1.Count; j++)
                    {
                        if (GameMain.Instance.CharacterMgr.status.GetFlag(yotogi.condManVisibleFlag1[j]) == 0)
                        {
                            StoryUnLock.myLog.LogMessage("SetScenarioAll.yotogi." + yotogi.condManVisibleFlag1[j]);
                            GameMain.Instance.CharacterMgr.status.SetFlag(yotogi.condManVisibleFlag1[j], 1);
                        }
                    }
                }
            }

            StoryUnLock.myLog.LogDarkBlue("SetAllYotogi END"           );
        }

        internal static void SetAllPlayerStatus()
        {
            StoryUnLock.myLog.LogDarkBlue("SetAllPlayerStatus st");

            PlayerStatus.Status status = GameMain.Instance.CharacterMgr.status;
            status.casinoCoin = 999999L;
            status.clubGauge = 100;
            status.clubGrade = 5;
            status.money = 9999999999L;

            ScheduleCSVData.vipFullOpenDay = 0;

            try
            {
                foreach (Trophy.Data item in Trophy.GetAllDatas(false))
                {
                    if (GameMain.Instance.CharacterMgr.status.IsHaveTrophy(item.id))
                    {
                        continue;
                    }

                    StoryUnLock.myLog.LogMessage("Trophy"
                    , item.id
                    , item.name
                    , item.type
                    , item.rarity
                    , item.maidPoint
                    , item.infoText
                    , item.bonusText
                    );
                    GameMain.Instance.CharacterMgr.status.AddHaveTrophy(item.id);
                }
            }
            catch (Exception e)
            {
                StoryUnLock.myLog.LogError("Trophy:" + e.ToString());
            }


            StoryUnLock.myLog.LogDarkBlue("SetAllPlayerStatus ed");
        }

        public static void SetMaidAll(Maid maid)
        {
            StoryUnLock.myLog.LogMessage(
                "CheatUtill.SetMaidAll st"
                );

            SetMaidStatus(maid);
            SetMaidYotogiClass(maid);
            SetMaidJobClass(maid);
            SetMaidSkill(maid);

            StoryUnLock.myLog.LogMessage(
            "CheatUtill.SetMaidAll end"
            );
        }

        internal static void SetHeroineType(HeroineType transfer)
        {
            StoryUnLock.myLog.LogMessage(
            "CheatUtill.SetHeroineType"
            );
            StoryUnLockPatch.selectMaid.status.heroineType = transfer;
        }



        public static ConfigEntry<bool> _SetMaidStatusOnOff;

        public static void AddStockMaid()
        {
            Maid maid = GameMain.Instance.CharacterMgr.AddStockMaid();
            PresetUtill.SetMaidRandPreset2(maid);

            if (maid == null)
            {
                StoryUnLock.myLog.LogFatal("maid null");
            }

            if (StoryUnLockGUI.rndPersonal)
            {
                StoryUnLockGUI.selGridPersonal = PersonalUtill.SetPersonalRandom(maid);
            }
            else
            {
                PersonalUtill.SetPersonal(maid, StoryUnLockGUI.selGridPersonal);
            }
            
            switch (StoryUnLockGUI.selGridContract)
            {
                case 0:
                    break;
                case 1:
                    maid.status.contract = Contract.Exclusive;
                    break;
                case 2:
                    maid.status.contract = Contract.Free;
                    break;
                case 3:
                    maid.status.contract = MyUtill.RandomEnum(Contract.Trainee);
                    break;
            }
            /**/
            if (_SetMaidStatusOnOff.Value)
                SetMaidAll(maid);

#if PresetUtill
            PresetUtill.RandPreset(maid);
#endif

        }


    }
}
