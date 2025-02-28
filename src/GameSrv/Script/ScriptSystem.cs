﻿using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using GameSrv.Npc;
using NLog;
using SystemModule.Common;

namespace GameSrv.Script {
    public partial class ScriptSystem {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<string, string> CallScriptDict = new Dictionary<string, string>();
        private readonly char[] TextSpitConst = new[] { ' ', '\t' };

        public void LoadScript(NormNpc NPC, string sPatch, string sScritpName) {
            if (string.IsNullOrEmpty(sPatch)) {
                sPatch = ScriptConst.sNpc_def;
            }
            LoadScriptFile(NPC, sPatch, sScritpName, false); ;
        }

        private static bool LoadScriptCallScript(string sFileName, string sLabel, StringList List) {
            var result = false;
            if (File.Exists(sFileName)) {
                var callStrList = new StringList();
                callStrList.LoadFromFile(sFileName);
                sLabel = '[' + sLabel + ']';
                var findLab = false;
                for (var i = 0; i < callStrList.Count; i++)
                {
                    var sLine = callStrList[i].Trim();
                    if (!string.IsNullOrEmpty(sLine))
                    {
                        if (!findLab) {
                            if (sLine[0] == '[' && string.Compare(sLine, sLabel, StringComparison.OrdinalIgnoreCase) == 0) {
                                findLab = true;
                                continue;
                            }
                        }
                        if (sLine[0] != '{') {
                            if (sLine[0] == '}') {
                                result = true;
                                break;
                            }
                            List.Add(sLine);
                        }
                    }
                }
                callStrList.Dispose();
            }
            return result;
        }

        private static int GetScriptCallCount(string sText) {
            return RegexCallCount().Count(sText);
        }

        private static string GetCallScriptPath(string path) {
            var sCallScriptFile = path;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                if (sCallScriptFile.StartsWith("\\\\")) {
                    sCallScriptFile = sCallScriptFile.Remove(0, 2);
                }
                else if (sCallScriptFile.StartsWith("\\")) {
                    sCallScriptFile = sCallScriptFile.Remove(0, 1);
                }
            }
            else {
                if (sCallScriptFile.StartsWith("\\\\")) {
                    sCallScriptFile = sCallScriptFile.Remove(0, 2);
                }
                else if (sCallScriptFile.StartsWith("\\")) {
                    sCallScriptFile = sCallScriptFile.Remove(0, 1);
                }
                sCallScriptFile = sCallScriptFile.Replace("\\", "/");
            }
            return sCallScriptFile;
        }

        private void LoadCallScript(ref StringList LoadList, ref bool success)
        {
            var callCount = GetScriptCallCount(LoadList.Text);
            if (callCount <= 0) {
                success = true;
                return;
            }
            var sLable = string.Empty;
            var callList = new StringList(1024);
            for (var i = 0; i < LoadList.Count; i++)
            {
                var sLine = LoadList[i].Trim();
                if (!string.IsNullOrEmpty(sLine) && sLine[0] == '#' && HUtil32.CompareLStr(sLine, "#CALL"))
                {
                    sLine = HUtil32.ArrestStringEx(sLine, "[", "]", ref sLable);
                    var sCallScriptFile = GetCallScriptPath(sLable.Trim());
                    var sLabName = sLine.Trim();
                    var sFileName = M2Share.GetEnvirFilePath("QuestDiary", sCallScriptFile);
                    if (CallScriptDict.ContainsKey(sFileName))
                    {
                        callCount--;
                        callList[i] = "#ACT";
                        callList.InsertText(i + 1, "goto " + sLabName);
                        break;
                    }
                    if (LoadScriptCallScript(sFileName, sLabName, callList))
                    {
                        callCount--;
                        if (!CallScriptDict.ContainsKey(sLabName))
                        {
                            CallScriptDict.Add(sFileName, sLabName);
                        }
                    }
                    else
                    {
                        _logger.Error("script error, load fail: " + sCallScriptFile + sLabName);
                    }
                }
                else
                {
                    callList.AppendText(sLine);
                }
            }
            LoadList = callList;
            success = callCount <= 0;
        }

        private string LoadScriptDefineInfo(StringList stringList, ICollection<DefineInfo> List) {
            var result = string.Empty;
            var defFile = string.Empty;
            var defineName = string.Empty;
            var defText = string.Empty;
            for (var i = 0; i < stringList.Count; i++) {
                var line = stringList[i].Trim();
                if (!string.IsNullOrEmpty(line) && line[0] == '#') {
                    if (HUtil32.CompareLStr(line, "#SETHOME"))
                    {
                        result = HUtil32.GetValidStr3(line, ref defFile, TextSpitConst).Trim();
                        stringList[i] = "";
                        continue;
                    }
                    if (HUtil32.CompareLStr(line, "#DEFINE")) {
                        line = HUtil32.GetValidStr3(line, ref defFile, TextSpitConst);
                        line = HUtil32.GetValidStr3(line, ref defineName, TextSpitConst);
                        line = HUtil32.GetValidStr3(line, ref defText, TextSpitConst);
                        var DefineInfo = new DefineInfo { Name = defineName.ToUpper(), Text = defText };
                        List.Add(DefineInfo);
                        stringList[i] = "";
                        continue;
                    }
                    if (HUtil32.CompareLStr(line, "#INCLUDE"))
                    {
                        var definesFile = HUtil32.GetValidStr3(line, ref defFile, TextSpitConst).Trim();
                        definesFile = M2Share.GetEnvirFilePath("Defines", definesFile);
                        if (File.Exists(definesFile))
                        {
                            using var LoadStrList = new StringList();
                            LoadStrList.LoadFromFile(definesFile);
                            result = LoadScriptDefineInfo(LoadStrList, List);
                        }
                        else
                        {
                            _logger.Error("script error, load fail: " + definesFile);
                        }
                        stringList[i] = "";
                        continue;
                    }
                }
            }
            return result;
        }

        private static ScriptInfo LoadMakeNewScript(NormNpc NPC)
        {
            var scriptInfo = new ScriptInfo
            {
                IsQuest = false,
                RecordList = new Dictionary<string, SayingRecord>(StringComparer.OrdinalIgnoreCase)
            };
            NPC.m_ScriptList.Add(scriptInfo);
            return scriptInfo;
        }

        private bool LoadScriptFileQuestCondition(string sText, ref QuestConditionInfo QuestConditionInfo) {
            var result = false;
            var sCmd = string.Empty;
            var sParam1 = string.Empty;
            var sParam2 = string.Empty;
            var sParam3 = string.Empty;
            var sParam4 = string.Empty;
            var sParam5 = string.Empty;
            var sParam6 = string.Empty;
            var nCMDCode = 0;
            sText = HUtil32.GetValidStrCap(sText, ref sCmd, TextSpitConst);
            sText = HUtil32.GetValidStrCap(sText, ref sParam1, TextSpitConst);
            sText = HUtil32.GetValidStrCap(sText, ref sParam2, TextSpitConst);
            sText = HUtil32.GetValidStrCap(sText, ref sParam3, TextSpitConst);
            sText = HUtil32.GetValidStrCap(sText, ref sParam4, TextSpitConst);
            sText = HUtil32.GetValidStrCap(sText, ref sParam5, TextSpitConst);
            sText = HUtil32.GetValidStrCap(sText, ref sParam6, TextSpitConst);
            if (sCmd.IndexOf(".", StringComparison.OrdinalIgnoreCase) > -1) //支持脚本变量
            {
                var sActName = string.Empty;
                sCmd = HUtil32.GetValidStrCap(sCmd, ref sActName, '.');
                if (!string.IsNullOrEmpty(sActName)) {
                    QuestConditionInfo.sOpName = sActName;
                    if (".".IndexOf(sCmd, StringComparison.OrdinalIgnoreCase) > -1) {
                        sCmd = HUtil32.GetValidStrCap(sCmd, ref sActName, '.');
                        if (string.Compare(sActName, "H", StringComparison.OrdinalIgnoreCase) == 0) {
                            QuestConditionInfo.sOpHName = "H";
                        }
                    }
                }
            }
            sCmd = sCmd.ToUpper();
            switch (sCmd) {
                case ScriptConst.sCHECK: {
                        nCMDCode = ScriptConst.nCHECK;
                        HUtil32.ArrestStringEx(sParam1, "[", "]", ref sParam1);
                        if (!HUtil32.IsStringNumber(sParam1)) {
                            nCMDCode = 0;
                        }
                        if (!HUtil32.IsStringNumber(sParam2)) {
                            nCMDCode = 0;
                        }
                        goto L001;
                    }
                case ScriptConst.sCHECKOPEN: {
                        nCMDCode = ScriptConst.nCHECKOPEN;
                        HUtil32.ArrestStringEx(sParam1, "[", "]", ref sParam1);
                        if (!HUtil32.IsStringNumber(sParam1)) {
                            nCMDCode = 0;
                        }
                        if (!HUtil32.IsStringNumber(sParam2)) {
                            nCMDCode = 0;
                        }
                        goto L001;
                    }
                case ScriptConst.sCHECKUNIT: {
                        nCMDCode = ScriptConst.nCHECKUNIT;
                        HUtil32.ArrestStringEx(sParam1, "[", "]", ref sParam1);
                        if (!HUtil32.IsStringNumber(sParam1)) {
                            nCMDCode = 0;
                        }
                        if (!HUtil32.IsStringNumber(sParam2)) {
                            nCMDCode = 0;
                        }
                        goto L001;
                    }
                case ScriptConst.sCHECKPKPOINT:
                    nCMDCode = ScriptConst.nCHECKPKPOINT;
                    goto L001;
                case ScriptConst.sCHECKGOLD:
                    nCMDCode = ScriptConst.nCHECKGOLD;
                    goto L001;
                case ScriptConst.sCHECKLEVEL:
                    nCMDCode = ScriptConst.nCHECKLEVEL;
                    goto L001;
                case ScriptConst.sCHECKJOB:
                    nCMDCode = ScriptConst.nCHECKJOB;
                    goto L001;
                case ScriptConst.sRANDOM:
                    nCMDCode = ScriptConst.nRANDOM;
                    goto L001;
                case ScriptConst.sCHECKITEM:
                    nCMDCode = ScriptConst.nCHECKITEM;
                    goto L001;
                case ScriptConst.sGENDER:
                    nCMDCode = ScriptConst.nGENDER;
                    goto L001;
                case ScriptConst.sCHECKBAGGAGE:
                    nCMDCode = ScriptConst.nCHECKBAGGAGE;
                    goto L001;
                case ScriptConst.sCHECKNAMELIST:
                    nCMDCode = ScriptConst.nCHECKNAMELIST;
                    goto L001;
                case ScriptConst.sSC_HASGUILD:
                    nCMDCode = ScriptConst.nSC_HASGUILD;
                    goto L001;
                case ScriptConst.sSC_ISGUILDMASTER:
                    nCMDCode = ScriptConst.nSC_ISGUILDMASTER;
                    goto L001;
                case ScriptConst.sSC_CHECKCASTLEMASTER:
                    nCMDCode = ScriptConst.nSC_CHECKCASTLEMASTER;
                    goto L001;
                case ScriptConst.sSC_ISNEWHUMAN:
                    nCMDCode = ScriptConst.nSC_ISNEWHUMAN;
                    goto L001;
                case ScriptConst.sSC_CHECKMEMBERTYPE:
                    nCMDCode = ScriptConst.nSC_CHECKMEMBERTYPE;
                    goto L001;
                case ScriptConst.sSC_CHECKMEMBERLEVEL:
                    nCMDCode = ScriptConst.nSC_CHECKMEMBERLEVEL;
                    goto L001;
                case ScriptConst.sSC_CHECKGAMEGOLD:
                    nCMDCode = ScriptConst.nSC_CHECKGAMEGOLD;
                    goto L001;
                case ScriptConst.sSC_CHECKGAMEPOINT:
                    nCMDCode = ScriptConst.nSC_CHECKGAMEPOINT;
                    goto L001;
                case ScriptConst.sSC_CHECKNAMELISTPOSITION:
                    nCMDCode = ScriptConst.nSC_CHECKNAMELISTPOSITION;
                    goto L001;
                case ScriptConst.sSC_CHECKGUILDLIST:
                    nCMDCode = ScriptConst.nSC_CHECKGUILDLIST;
                    goto L001;
                case ScriptConst.sSC_CHECKRENEWLEVEL:
                    nCMDCode = ScriptConst.nSC_CHECKRENEWLEVEL;
                    goto L001;
                case ScriptConst.sSC_CHECKSLAVELEVEL:
                    nCMDCode = ScriptConst.nSC_CHECKSLAVELEVEL;
                    goto L001;
                case ScriptConst.sSC_CHECKSLAVENAME:
                    nCMDCode = ScriptConst.nSC_CHECKSLAVENAME;
                    goto L001;
                case ScriptConst.sSC_CHECKCREDITPOINT:
                    nCMDCode = ScriptConst.nSC_CHECKCREDITPOINT;
                    goto L001;
                case ScriptConst.sSC_CHECKOFGUILD:
                    nCMDCode = ScriptConst.nSC_CHECKOFGUILD;
                    goto L001;
                case ScriptConst.sSC_CHECKPAYMENT:
                    nCMDCode = ScriptConst.nSC_CHECKPAYMENT;
                    goto L001;
                case ScriptConst.sSC_CHECKUSEITEM:
                    nCMDCode = ScriptConst.nSC_CHECKUSEITEM;
                    goto L001;
                case ScriptConst.sSC_CHECKBAGSIZE:
                    nCMDCode = ScriptConst.nSC_CHECKBAGSIZE;
                    goto L001;
                case ScriptConst.sSC_CHECKLISTCOUNT:
                    nCMDCode = ScriptConst.nSC_CHECKLISTCOUNT;
                    goto L001;
                case ScriptConst.sSC_CHECKDC:
                    nCMDCode = ScriptConst.nSC_CHECKDC;
                    goto L001;
                case ScriptConst.sSC_CHECKMC:
                    nCMDCode = ScriptConst.nSC_CHECKMC;
                    goto L001;
                case ScriptConst.sSC_CHECKSC:
                    nCMDCode = ScriptConst.nSC_CHECKSC;
                    goto L001;
                case ScriptConst.sSC_CHECKHP:
                    nCMDCode = ScriptConst.nSC_CHECKHP;
                    goto L001;
                case ScriptConst.sSC_CHECKMP:
                    nCMDCode = ScriptConst.nSC_CHECKMP;
                    goto L001;
                case ScriptConst.sSC_CHECKITEMTYPE:
                    nCMDCode = ScriptConst.nSC_CHECKITEMTYPE;
                    goto L001;
                case ScriptConst.sSC_CHECKEXP:
                    nCMDCode = ScriptConst.nSC_CHECKEXP;
                    goto L001;
                case ScriptConst.sSC_CHECKCASTLEGOLD:
                    nCMDCode = ScriptConst.nSC_CHECKCASTLEGOLD;
                    goto L001;
                case ScriptConst.sSC_PASSWORDERRORCOUNT:
                    nCMDCode = ScriptConst.nSC_PASSWORDERRORCOUNT;
                    goto L001;
                case ScriptConst.sSC_ISLOCKPASSWORD:
                    nCMDCode = ScriptConst.nSC_ISLOCKPASSWORD;
                    goto L001;
                case ScriptConst.sSC_ISLOCKSTORAGE:
                    nCMDCode = ScriptConst.nSC_ISLOCKSTORAGE;
                    goto L001;
                case ScriptConst.sSC_CHECKBUILDPOINT:
                    nCMDCode = ScriptConst.nSC_CHECKBUILDPOINT;
                    goto L001;
                case ScriptConst.sSC_CHECKAURAEPOINT:
                    nCMDCode = ScriptConst.nSC_CHECKAURAEPOINT;
                    goto L001;
                case ScriptConst.sSC_CHECKSTABILITYPOINT:
                    nCMDCode = ScriptConst.nSC_CHECKSTABILITYPOINT;
                    goto L001;
                case ScriptConst.sSC_CHECKFLOURISHPOINT:
                    nCMDCode = ScriptConst.nSC_CHECKFLOURISHPOINT;
                    goto L001;
                case ScriptConst.sSC_CHECKCONTRIBUTION:
                    nCMDCode = ScriptConst.nSC_CHECKCONTRIBUTION;
                    goto L001;
                case ScriptConst.sSC_CHECKRANGEMONCOUNT:
                    nCMDCode = ScriptConst.nSC_CHECKRANGEMONCOUNT;
                    goto L001;
                case ScriptConst.sSC_CHECKITEMADDVALUE:
                    nCMDCode = ScriptConst.nSC_CHECKITEMADDVALUE;
                    goto L001;
                case ScriptConst.sSC_CHECKINMAPRANGE:
                    nCMDCode = ScriptConst.nSC_CHECKINMAPRANGE;
                    goto L001;
                case ScriptConst.sSC_CASTLECHANGEDAY:
                    nCMDCode = ScriptConst.nSC_CASTLECHANGEDAY;
                    goto L001;
                case ScriptConst.sSC_CASTLEWARDAY:
                    nCMDCode = ScriptConst.nSC_CASTLEWARDAY;
                    goto L001;
                case ScriptConst.sSC_ONLINELONGMIN:
                    nCMDCode = ScriptConst.nSC_ONLINELONGMIN;
                    goto L001;
                case ScriptConst.sSC_CHECKGUILDCHIEFITEMCOUNT:
                    nCMDCode = ScriptConst.nSC_CHECKGUILDCHIEFITEMCOUNT;
                    goto L001;
                case ScriptConst.sSC_CHECKNAMEDATELIST:
                    nCMDCode = ScriptConst.nSC_CHECKNAMEDATELIST;
                    goto L001;
                case ScriptConst.sSC_CHECKMAPHUMANCOUNT:
                    nCMDCode = ScriptConst.nSC_CHECKMAPHUMANCOUNT;
                    goto L001;
                case ScriptConst.sSC_CHECKMAPMONCOUNT:
                    nCMDCode = ScriptConst.nSC_CHECKMAPMONCOUNT;
                    goto L001;
                case ScriptConst.sSC_CHECKVAR:
                    nCMDCode = ScriptConst.nSC_CHECKVAR;
                    goto L001;
                case ScriptConst.sSC_CHECKSERVERNAME:
                    nCMDCode = ScriptConst.nSC_CHECKSERVERNAME;
                    goto L001;
                case ScriptConst.sSC_ISATTACKGUILD:
                    nCMDCode = ScriptConst.nSC_ISATTACKGUILD;
                    goto L001;
                case ScriptConst.sSC_ISDEFENSEGUILD:
                    nCMDCode = ScriptConst.nSC_ISDEFENSEGUILD;
                    goto L001;
                case ScriptConst.sSC_ISATTACKALLYGUILD:
                    nCMDCode = ScriptConst.nSC_ISATTACKALLYGUILD;
                    goto L001;
                case ScriptConst.sSC_ISDEFENSEALLYGUILD:
                    nCMDCode = ScriptConst.nSC_ISDEFENSEALLYGUILD;
                    goto L001;
                case ScriptConst.sSC_ISCASTLEGUILD:
                    nCMDCode = ScriptConst.nSC_ISCASTLEGUILD;
                    goto L001;
                case ScriptConst.sSC_CHECKCASTLEDOOR:
                    nCMDCode = ScriptConst.nSC_CHECKCASTLEDOOR;
                    goto L001;
                case ScriptConst.sSC_ISSYSOP:
                    nCMDCode = ScriptConst.nSC_ISSYSOP;
                    goto L001;
                case ScriptConst.sSC_ISADMIN:
                    nCMDCode = ScriptConst.nSC_ISADMIN;
                    goto L001;
                case ScriptConst.sSC_CHECKGROUPCOUNT:
                    nCMDCode = ScriptConst.nSC_CHECKGROUPCOUNT;
                    goto L001;
                case ScriptConst.sCHECKACCOUNTLIST:
                    nCMDCode = ScriptConst.nCHECKACCOUNTLIST;
                    goto L001;
                case ScriptConst.sCHECKIPLIST:
                    nCMDCode = ScriptConst.nCHECKIPLIST;
                    goto L001;
                case ScriptConst.sCHECKBBCOUNT:
                    nCMDCode = ScriptConst.nCHECKBBCOUNT;
                    goto L001;
                case ScriptConst.sSC_CHECKDLGITEMTYPE:
                    nCMDCode = ScriptConst.nSC_CHECKDLGITEMTYPE;
                    goto L001;
                case ScriptConst.sSC_CHECKDLGITEMNAME:
                    nCMDCode = ScriptConst.nSC_CHECKDLGITEMNAME;
                    goto L001;
                case ScriptConst.sDAYTIME:
                    nCMDCode = ScriptConst.nDAYTIME;
                    goto L001;
                case ScriptConst.sCHECKITEMW:
                    nCMDCode = ScriptConst.nCHECKITEMW;
                    goto L001;
                case ScriptConst.sISTAKEITEM:
                    nCMDCode = ScriptConst.nISTAKEITEM;
                    goto L001;
                case ScriptConst.sCHECKDURA:
                    nCMDCode = ScriptConst.nCHECKDURA;
                    goto L001;
                case ScriptConst.sCHECKDURAEVA:
                    nCMDCode = ScriptConst.nCHECKDURAEVA;
                    goto L001;
                case ScriptConst.sDAYOFWEEK:
                    nCMDCode = ScriptConst.nDAYOFWEEK;
                    goto L001;
                case ScriptConst.sHOUR:
                    nCMDCode = ScriptConst.nHOUR;
                    goto L001;
                case ScriptConst.sMIN:
                    nCMDCode = ScriptConst.nMIN;
                    goto L001;
                case ScriptConst.sCHECKLUCKYPOINT:
                    nCMDCode = ScriptConst.nCHECKLUCKYPOINT;
                    goto L001;
                case ScriptConst.sCHECKMONMAP:
                    nCMDCode = ScriptConst.nCHECKMONMAP;
                    goto L001;
                case ScriptConst.sCHECKMONAREA:
                    nCMDCode = ScriptConst.nCHECKMONAREA;
                    goto L001;
                case ScriptConst.sCHECKHUM:
                    nCMDCode = ScriptConst.nCHECKHUM;
                    goto L001;
                case ScriptConst.sEQUAL:
                    nCMDCode = ScriptConst.nEQUAL;
                    goto L001;
                case ScriptConst.sLARGE:
                    nCMDCode = ScriptConst.nLARGE;
                    goto L001;
                case ScriptConst.sSMALL:
                    nCMDCode = ScriptConst.nSMALL;
                    goto L001;
                case ScriptConst.sSC_CHECKPOSEDIR:
                    nCMDCode = ScriptConst.nSC_CHECKPOSEDIR;
                    goto L001;
                case ScriptConst.sSC_CHECKPOSELEVEL:
                    nCMDCode = ScriptConst.nSC_CHECKPOSELEVEL;
                    goto L001;
                case ScriptConst.sSC_CHECKPOSEGENDER:
                    nCMDCode = ScriptConst.nSC_CHECKPOSEGENDER;
                    goto L001;
                case ScriptConst.sSC_CHECKLEVELEX:
                    nCMDCode = ScriptConst.nSC_CHECKLEVELEX;
                    goto L001;
                case ScriptConst.sSC_CHECKBONUSPOINT:
                    nCMDCode = ScriptConst.nSC_CHECKBONUSPOINT;
                    goto L001;
                case ScriptConst.sSC_CHECKMARRY:
                    nCMDCode = ScriptConst.nSC_CHECKMARRY;
                    goto L001;
                case ScriptConst.sSC_CHECKPOSEMARRY:
                    nCMDCode = ScriptConst.nSC_CHECKPOSEMARRY;
                    goto L001;
                case ScriptConst.sSC_CHECKMARRYCOUNT:
                    nCMDCode = ScriptConst.nSC_CHECKMARRYCOUNT;
                    goto L001;
                case ScriptConst.sSC_CHECKMASTER:
                    nCMDCode = ScriptConst.nSC_CHECKMASTER;
                    goto L001;
                case ScriptConst.sSC_HAVEMASTER:
                    nCMDCode = ScriptConst.nSC_HAVEMASTER;
                    goto L001;
                case ScriptConst.sSC_CHECKPOSEMASTER:
                    nCMDCode = ScriptConst.nSC_CHECKPOSEMASTER;
                    goto L001;
                case ScriptConst.sSC_POSEHAVEMASTER:
                    nCMDCode = ScriptConst.nSC_POSEHAVEMASTER;
                    goto L001;
                case ScriptConst.sSC_CHECKISMASTER:
                    nCMDCode = ScriptConst.nSC_CHECKISMASTER;
                    goto L001;
                case ScriptConst.sSC_CHECKPOSEISMASTER:
                    nCMDCode = ScriptConst.nSC_CHECKPOSEISMASTER;
                    goto L001;
                case ScriptConst.sSC_CHECKNAMEIPLIST:
                    nCMDCode = ScriptConst.nSC_CHECKNAMEIPLIST;
                    goto L001;
                case ScriptConst.sSC_CHECKACCOUNTIPLIST:
                    nCMDCode = ScriptConst.nSC_CHECKACCOUNTIPLIST;
                    goto L001;
                case ScriptConst.sSC_CHECKSLAVECOUNT:
                    nCMDCode = ScriptConst.nSC_CHECKSLAVECOUNT;
                    goto L001;
                case ScriptConst.sSC_CHECKPOS:
                    nCMDCode = ScriptConst.nSC_CHECKPOS;
                    goto L001;
                case ScriptConst.sSC_CHECKMAP:
                    nCMDCode = ScriptConst.nSC_CHECKMAP;
                    goto L001;
                case ScriptConst.sSC_REVIVESLAVE:
                    nCMDCode = ScriptConst.nSC_REVIVESLAVE;
                    goto L001;
                case ScriptConst.sSC_CHECKMAGICLVL:
                    nCMDCode = ScriptConst.nSC_CHECKMAGICLVL;
                    goto L001;
                case ScriptConst.sSC_CHECKGROUPCLASS:
                    nCMDCode = ScriptConst.nSC_CHECKGROUPCLASS;
                    goto L001;
                case ScriptConst.sSC_ISGROUPMASTER:
                    nCMDCode = ScriptConst.nSC_ISGROUPMASTER;
                    goto L001;
                case ScriptConst.sCheckDiemon:
                    nCMDCode = ScriptConst.nCheckDiemon;
                    goto L001;
                case ScriptConst.scheckkillplaymon:
                    nCMDCode = ScriptConst.ncheckkillplaymon;
                    goto L001;
                case ScriptConst.sSC_CHECKRANDOMNO:
                    nCMDCode = ScriptConst.nSC_CHECKRANDOMNO;
                    goto L001;
                case ScriptConst.sSC_CHECKISONMAP:
                    nCMDCode = ScriptConst.nSC_CHECKISONMAP;
                    goto L001;
                // 检测是否安全区
                case ScriptConst.sSC_CHECKINSAFEZONE:
                    nCMDCode = ScriptConst.nSC_CHECKINSAFEZONE;
                    goto L001;
                case ScriptConst.sSC_KILLBYHUM:
                    nCMDCode = ScriptConst.nSC_KILLBYHUM;
                    goto L001;
                case ScriptConst.sSC_KILLBYMON:
                    nCMDCode = ScriptConst.nSC_KILLBYMON;
                    goto L001;
                // 增加挂机
                case ScriptConst.sSC_OffLine:
                    nCMDCode = ScriptConst.nSC_OffLine;
                    goto L001;
                // 增加脚本特修所有装备命令
                case ScriptConst.sSC_REPAIRALL:
                    nCMDCode = ScriptConst.nSC_REPAIRALL;
                    goto L001;
                // 刷新包裹物品命令
                case ScriptConst.sSC_QUERYBAGITEMS:
                    nCMDCode = ScriptConst.nSC_QUERYBAGITEMS;
                    goto L001;
                case ScriptConst.sSC_SETRANDOMNO:
                    nCMDCode = ScriptConst.nSC_SETRANDOMNO;
                    goto L001;
                case ScriptConst.sSC_DELAYGOTO:
                case "DELAYCALL":
                    nCMDCode = ScriptConst.nSC_DELAYGOTO;
                    goto L001;
                case ScriptConst.sSCHECKDEATHPLAYMON:
                    nCMDCode = ScriptConst.nSCHECKDEATHPLAYMON;
                    goto L001;
                case ScriptConst.sSCHECKKILLMOBNAME:
                    nCMDCode = ScriptConst.nSCHECKDEATHPLAYMON;
                    goto L001;
            }
        L001:
            if (nCMDCode > 0) {
                QuestConditionInfo.CmdCode = nCMDCode;
                if (!string.IsNullOrEmpty(sParam1) && sParam1[0] == '\"') {
                    HUtil32.ArrestStringEx(sParam1, "\"", "\"", ref sParam1);
                }
                if (!string.IsNullOrEmpty(sParam2) && sParam2[0] == '\"') {
                    HUtil32.ArrestStringEx(sParam2, "\"", "\"", ref sParam2);
                }
                if (!string.IsNullOrEmpty(sParam3) && sParam3[0] == '\"') {
                    HUtil32.ArrestStringEx(sParam3, "\"", "\"", ref sParam3);
                }
                if (!string.IsNullOrEmpty(sParam4) && sParam4[0] == '\"') {
                    HUtil32.ArrestStringEx(sParam4, "\"", "\"", ref sParam4);
                }
                if (!string.IsNullOrEmpty(sParam5) && sParam5[0] == '\"') {
                    HUtil32.ArrestStringEx(sParam5, "\"", "\"", ref sParam5);
                }
                if (!string.IsNullOrEmpty(sParam6) && sParam6[0] == '\"') {
                    HUtil32.ArrestStringEx(sParam6, "\"", "\"", ref sParam6);
                }
                QuestConditionInfo.sParam1 = sParam1;
                QuestConditionInfo.sParam2 = sParam2;
                QuestConditionInfo.sParam3 = sParam3;
                QuestConditionInfo.sParam4 = sParam4;
                QuestConditionInfo.sParam5 = sParam5;
                QuestConditionInfo.sParam6 = sParam6;
                if (HUtil32.IsStringNumber(sParam1)) {
                    QuestConditionInfo.nParam1 = HUtil32.StrToInt(sParam1, 0);
                }
                if (HUtil32.IsStringNumber(sParam2)) {
                    QuestConditionInfo.nParam2 = HUtil32.StrToInt(sParam2, 0);
                }
                if (HUtil32.IsStringNumber(sParam3)) {
                    QuestConditionInfo.nParam3 = HUtil32.StrToInt(sParam3, 0);
                }
                if (HUtil32.IsStringNumber(sParam4)) {
                    QuestConditionInfo.nParam4 = HUtil32.StrToInt(sParam4, 0);
                }
                if (HUtil32.IsStringNumber(sParam5)) {
                    QuestConditionInfo.nParam5 = HUtil32.StrToInt(sParam5, 0);
                }
                if (HUtil32.IsStringNumber(sParam6)) {
                    QuestConditionInfo.nParam6 = HUtil32.StrToInt(sParam6, 0);
                }
                result = true;
            }
            return result;
        }

        private bool LoadScriptFileQuestAction(string sText, ref QuestActionInfo QuestActionInfo) {
            var sCmd = string.Empty;
            var sParam1 = string.Empty;
            var sParam2 = string.Empty;
            var sParam3 = string.Empty;
            var sParam4 = string.Empty;
            var sParam5 = string.Empty;
            var sParam6 = string.Empty;
            int nCMDCode= 0;
            var result = false;
            sText = HUtil32.GetValidStrCap(sText, ref sCmd, TextSpitConst);
            sText = HUtil32.GetValidStrCap(sText, ref sParam1, TextSpitConst);
            sText = HUtil32.GetValidStrCap(sText, ref sParam2, TextSpitConst);
            sText = HUtil32.GetValidStrCap(sText, ref sParam3, TextSpitConst);
            sText = HUtil32.GetValidStrCap(sText, ref sParam4, TextSpitConst);
            sText = HUtil32.GetValidStrCap(sText, ref sParam5, TextSpitConst);
            sText = HUtil32.GetValidStrCap(sText, ref sParam6, TextSpitConst);
            if (sCmd.IndexOf(".", StringComparison.OrdinalIgnoreCase) > -1) //支持脚本变量
            {
                var sActName = string.Empty;
                sCmd = HUtil32.GetValidStrCap(sCmd, ref sActName, '.');
                if (!string.IsNullOrEmpty(sActName)) {
                    QuestActionInfo.sOpName = sActName;
                    if (sCmd.IndexOf(".", StringComparison.OrdinalIgnoreCase) > -1) {
                        sCmd = HUtil32.GetValidStrCap(sCmd, ref sActName, '.');
                        if (string.Compare(sActName, "H", StringComparison.OrdinalIgnoreCase) == 0) {
                            QuestActionInfo.sOpHName = "H";
                        }
                    }
                }
            }
            sCmd = sCmd.ToUpper();
            switch (sCmd) {
                case ScriptConst.sSET: {
                        nCMDCode = ScriptConst.nSET;
                        HUtil32.ArrestStringEx(sParam1, "[", "]", ref sParam1);
                        if (!HUtil32.IsStringNumber(sParam1)) {
                            nCMDCode = 0;
                        }
                        if (!HUtil32.IsStringNumber(sParam2)) {
                            nCMDCode = 0;
                        }
                        break;
                    }
                case ScriptConst.sRESET: {
                        nCMDCode = ScriptConst.nRESET;
                        HUtil32.ArrestStringEx(sParam1, "[", "]", ref sParam1);
                        if (!HUtil32.IsStringNumber(sParam1)) {
                            nCMDCode = 0;
                        }
                        if (!HUtil32.IsStringNumber(sParam2)) {
                            nCMDCode = 0;
                        }
                        break;
                    }
                case ScriptConst.sSETOPEN: {
                        nCMDCode = ScriptConst.nSETOPEN;
                        HUtil32.ArrestStringEx(sParam1, "[", "]", ref sParam1);
                        if (!HUtil32.IsStringNumber(sParam1)) {
                            nCMDCode = 0;
                        }
                        if (!HUtil32.IsStringNumber(sParam2)) {
                            nCMDCode = 0;
                        }
                        break;
                    }
                case ScriptConst.sSETUNIT: {
                        nCMDCode = ScriptConst.nSETUNIT;
                        HUtil32.ArrestStringEx(sParam1, "[", "]", ref sParam1);
                        if (!HUtil32.IsStringNumber(sParam1)) {
                            nCMDCode = 0;
                        }
                        if (!HUtil32.IsStringNumber(sParam2)) {
                            nCMDCode = 0;
                        }
                        break;
                    }
                case ScriptConst.sRESETUNIT: {
                        nCMDCode = ScriptConst.nRESETUNIT;
                        HUtil32.ArrestStringEx(sParam1, "[", "]", ref sParam1);
                        if (!HUtil32.IsStringNumber(sParam1)) {
                            nCMDCode = 0;
                        }
                        if (!HUtil32.IsStringNumber(sParam2)) {
                            nCMDCode = 0;
                        }

                        break;
                    }
                case ScriptConst.sTAKE:
                    nCMDCode = ScriptConst.nTAKE;
                    goto L001;
                case ScriptConst.sSC_GIVE:
                    nCMDCode = ScriptConst.nSC_GIVE;
                    goto L001;
                case ScriptConst.sCLOSE:
                    nCMDCode = ScriptConst.nCLOSE;
                    goto L001;
                case ScriptConst.sBREAK:
                    nCMDCode = ScriptConst.nBREAK;
                    goto L001;
                case ScriptConst.sGOTO:
                    nCMDCode = ScriptConst.nGOTO;
                    goto L001;
                case ScriptConst.sADDNAMELIST:
                    nCMDCode = ScriptConst.nADDNAMELIST;
                    goto L001;
                case ScriptConst.sDELNAMELIST:
                    nCMDCode = ScriptConst.nDELNAMELIST;
                    goto L001;
                case ScriptConst.sADDGUILDLIST:
                    nCMDCode = ScriptConst.nADDGUILDLIST;
                    goto L001;
                case ScriptConst.sDELGUILDLIST:
                    nCMDCode = ScriptConst.nDELGUILDLIST;
                    goto L001;
                case ScriptConst.sSC_LINEMSG:
                    nCMDCode = ScriptConst.nSC_LINEMSG;
                    goto L001;
                case ScriptConst.sADDACCOUNTLIST:
                    nCMDCode = ScriptConst.nADDACCOUNTLIST;
                    goto L001;
                case ScriptConst.sDELACCOUNTLIST:
                    nCMDCode = ScriptConst.nDELACCOUNTLIST;
                    goto L001;
                case ScriptConst.sADDIPLIST:
                    nCMDCode = ScriptConst.nADDIPLIST;
                    goto L001;
                case ScriptConst.sDELIPLIST:
                    nCMDCode = ScriptConst.nDELIPLIST;
                    goto L001;
                case ScriptConst.sSENDMSG:
                    nCMDCode = ScriptConst.nSENDMSG;
                    goto L001;
                case ScriptConst.sCHANGEMODE:
                    nCMDCode = ScriptConst.nSC_CHANGEMODE;
                    goto L001;
                case ScriptConst.sPKPOINT:
                    nCMDCode = ScriptConst.nPKPOINT;
                    goto L001;
                case ScriptConst.sCHANGEXP:
                    nCMDCode = ScriptConst.nCHANGEXP;
                    goto L001;
                case ScriptConst.sSC_RECALLMOB:
                    nCMDCode = ScriptConst.nSC_RECALLMOB;
                    goto L001;
                case ScriptConst.sTAKEW:
                    nCMDCode = ScriptConst.nTAKEW;
                    goto L001;
                case ScriptConst.sTIMERECALL:
                    nCMDCode = ScriptConst.nTIMERECALL;
                    goto L001;
                case ScriptConst.sSC_PARAM1:
                    nCMDCode = ScriptConst.nSC_PARAM1;
                    goto L001;
                case ScriptConst.sSC_PARAM2:
                    nCMDCode = ScriptConst.nSC_PARAM2;
                    goto L001;
                case ScriptConst.sSC_PARAM3:
                    nCMDCode = ScriptConst.nSC_PARAM3;
                    goto L001;
                case ScriptConst.sSC_PARAM4:
                    nCMDCode = ScriptConst.nSC_PARAM4;
                    goto L001;
                case ScriptConst.sSC_EXEACTION:
                    nCMDCode = ScriptConst.nSC_EXEACTION;
                    goto L001;
                case ScriptConst.sMAPMOVE:
                    nCMDCode = ScriptConst.nMAPMOVE;
                    goto L001;
                case ScriptConst.sMAP:
                    nCMDCode = ScriptConst.nMAP;
                    goto L001;
                case ScriptConst.sTAKECHECKITEM:
                    nCMDCode = ScriptConst.nTAKECHECKITEM;
                    goto L001;
                case ScriptConst.sMONGEN:
                    nCMDCode = ScriptConst.nMONGEN;
                    goto L001;
                case ScriptConst.sMONCLEAR:
                    nCMDCode = ScriptConst.nMONCLEAR;
                    goto L001;
                case ScriptConst.sMOV:
                    nCMDCode = ScriptConst.nMOV;
                    goto L001;
                case ScriptConst.sINC:
                    nCMDCode = ScriptConst.nINC;
                    goto L001;
                case ScriptConst.sDEC:
                    nCMDCode = ScriptConst.nDEC;
                    goto L001;
                case ScriptConst.sSUM:
                    nCMDCode = ScriptConst.nSUM;
                    goto L001;
                //变量运算
                //除法
                case ScriptConst.sSC_DIV:
                    nCMDCode = ScriptConst.nSC_DIV;
                    goto L001;
                //除法
                case ScriptConst.sSC_MUL:
                    nCMDCode = ScriptConst.nSC_MUL;
                    goto L001;
                //除法
                case ScriptConst.sSC_PERCENT:
                    nCMDCode = ScriptConst.nSC_PERCENT;
                    goto L001;
                case ScriptConst.sTHROWITEM:
                case ScriptConst.sDROPITEMMAP:
                    nCMDCode = ScriptConst.nTHROWITEM;
                    goto L001;
                case ScriptConst.sBREAKTIMERECALL:
                    nCMDCode = ScriptConst.nBREAKTIMERECALL;
                    goto L001;
                case ScriptConst.sMOVR:
                    nCMDCode = ScriptConst.nMOVR;
                    goto L001;
                case ScriptConst.sEXCHANGEMAP:
                    nCMDCode = ScriptConst.nEXCHANGEMAP;
                    goto L001;
                case ScriptConst.sRECALLMAP:
                    nCMDCode = ScriptConst.nRECALLMAP;
                    goto L001;
                case ScriptConst.sADDBATCH:
                    nCMDCode = ScriptConst.nADDBATCH;
                    goto L001;
                case ScriptConst.sBATCHDELAY:
                    nCMDCode = ScriptConst.nBATCHDELAY;
                    goto L001;
                case ScriptConst.sBATCHMOVE:
                    nCMDCode = ScriptConst.nBATCHMOVE;
                    goto L001;
                case ScriptConst.sPLAYDICE:
                    nCMDCode = ScriptConst.nPLAYDICE;
                    goto L001;
                case ScriptConst.sGOQUEST:
                    nCMDCode = ScriptConst.nGOQUEST;
                    goto L001;
                case ScriptConst.sENDQUEST:
                    nCMDCode = ScriptConst.nENDQUEST;
                    goto L001;
                case ScriptConst.sSC_HAIRCOLOR:
                    nCMDCode = ScriptConst.nSC_HAIRCOLOR;
                    goto L001;
                case ScriptConst.sSC_WEARCOLOR:
                    nCMDCode = ScriptConst.nSC_WEARCOLOR;
                    goto L001;
                case ScriptConst.sSC_HAIRSTYLE:
                    nCMDCode = ScriptConst.nSC_HAIRSTYLE;
                    goto L001;
                case ScriptConst.sSC_MONRECALL:
                    nCMDCode = ScriptConst.nSC_MONRECALL;
                    goto L001;
                case ScriptConst.sSC_HORSECALL:
                    nCMDCode = ScriptConst.nSC_HORSECALL;
                    goto L001;
                case ScriptConst.sSC_HAIRRNDCOL:
                    nCMDCode = ScriptConst.nSC_HAIRRNDCOL;
                    goto L001;
                case ScriptConst.sSC_KILLHORSE:
                    nCMDCode = ScriptConst.nSC_KILLHORSE;
                    goto L001;
                case ScriptConst.sSC_RANDSETDAILYQUEST:
                    nCMDCode = ScriptConst.nSC_RANDSETDAILYQUEST;
                    goto L001;
                case ScriptConst.sSC_CHANGELEVEL:
                    nCMDCode = ScriptConst.nSC_CHANGELEVEL;
                    goto L001;
                case ScriptConst.sSC_MARRY:
                    nCMDCode = ScriptConst.nSC_MARRY;
                    goto L001;
                case ScriptConst.sSC_UNMARRY:
                    nCMDCode = ScriptConst.nSC_UNMARRY;
                    goto L001;
                case ScriptConst.sSC_GETMARRY:
                    nCMDCode = ScriptConst.nSC_GETMARRY;
                    goto L001;
                case ScriptConst.sSC_GETMASTER:
                    nCMDCode = ScriptConst.nSC_GETMASTER;
                    goto L001;
                case ScriptConst.sSC_CLEARSKILL:
                    nCMDCode = ScriptConst.nSC_CLEARSKILL;
                    goto L001;
                case ScriptConst.sSC_DELNOJOBSKILL:
                    nCMDCode = ScriptConst.nSC_DELNOJOBSKILL;
                    goto L001;
                case ScriptConst.sSC_DELSKILL:
                    nCMDCode = ScriptConst.nSC_DELSKILL;
                    goto L001;
                case ScriptConst.sSC_ADDSKILL:
                    nCMDCode = ScriptConst.nSC_ADDSKILL;
                    goto L001;
                case ScriptConst.sSC_SKILLLEVEL:
                    nCMDCode = ScriptConst.nSC_SKILLLEVEL;
                    goto L001;
                case ScriptConst.sSC_CHANGEPKPOINT:
                    nCMDCode = ScriptConst.nSC_CHANGEPKPOINT;
                    goto L001;
                case ScriptConst.sSC_CHANGEEXP:
                    nCMDCode = ScriptConst.nSC_CHANGEEXP;
                    goto L001;
                case ScriptConst.sSC_CHANGEJOB:
                    nCMDCode = ScriptConst.nSC_CHANGEJOB;
                    goto L001;
                case ScriptConst.sSC_MISSION:
                    nCMDCode = ScriptConst.nSC_MISSION;
                    goto L001;
                case ScriptConst.sSC_MOBPLACE:
                    nCMDCode = ScriptConst.nSC_MOBPLACE;
                    goto L001;
                case ScriptConst.sSC_SETMEMBERTYPE:
                    nCMDCode = ScriptConst.nSC_SETMEMBERTYPE;
                    goto L001;
                case ScriptConst.sSC_SETMEMBERLEVEL:
                    nCMDCode = ScriptConst.nSC_SETMEMBERLEVEL;
                    goto L001;
                case ScriptConst.sSC_GAMEGOLD:
                    nCMDCode = ScriptConst.nSC_GAMEGOLD;
                    goto L001;
                case ScriptConst.sSC_GAMEPOINT:
                    nCMDCode = ScriptConst.nSC_GAMEPOINT;
                    goto L001;
                case ScriptConst.sSC_PKZONE:
                    nCMDCode = ScriptConst.nSC_PKZONE;
                    goto L001;
                case ScriptConst.sSC_RESTBONUSPOINT:
                    nCMDCode = ScriptConst.nSC_RESTBONUSPOINT;
                    goto L001;
                case ScriptConst.sSC_TAKECASTLEGOLD:
                    nCMDCode = ScriptConst.nSC_TAKECASTLEGOLD;
                    goto L001;
                case ScriptConst.sSC_HUMANHP:
                    nCMDCode = ScriptConst.nSC_HUMANHP;
                    goto L001;
                case ScriptConst.sSC_HUMANMP:
                    nCMDCode = ScriptConst.nSC_HUMANMP;
                    goto L001;
                case ScriptConst.sSC_BUILDPOINT:
                    nCMDCode = ScriptConst.nSC_BUILDPOINT;
                    goto L001;
                case ScriptConst.sSC_AURAEPOINT:
                    nCMDCode = ScriptConst.nSC_AURAEPOINT;
                    goto L001;
                case ScriptConst.sSC_STABILITYPOINT:
                    nCMDCode = ScriptConst.nSC_STABILITYPOINT;
                    goto L001;
                case ScriptConst.sSC_FLOURISHPOINT:
                    nCMDCode = ScriptConst.nSC_FLOURISHPOINT;
                    goto L001;
                case ScriptConst.sSC_OPENMAGICBOX:
                    nCMDCode = ScriptConst.nSC_OPENMAGICBOX;
                    goto L001;
                case ScriptConst.sSC_SETRANKLEVELNAME:
                    nCMDCode = ScriptConst.nSC_SETRANKLEVELNAME;
                    goto L001;
                case ScriptConst.sSC_GMEXECUTE:
                    nCMDCode = ScriptConst.nSC_GMEXECUTE;
                    goto L001;
                case ScriptConst.sSC_GUILDCHIEFITEMCOUNT:
                    nCMDCode = ScriptConst.nSC_GUILDCHIEFITEMCOUNT;
                    goto L001;
                case ScriptConst.sSC_ADDNAMEDATELIST:
                    nCMDCode = ScriptConst.nSC_ADDNAMEDATELIST;
                    goto L001;
                case ScriptConst.sSC_DELNAMEDATELIST:
                    nCMDCode = ScriptConst.nSC_DELNAMEDATELIST;
                    goto L001;
                case ScriptConst.sSC_MOBFIREBURN:
                    nCMDCode = ScriptConst.nSC_MOBFIREBURN;
                    goto L001;
                case ScriptConst.sSC_MESSAGEBOX:
                    nCMDCode = ScriptConst.nSC_MESSAGEBOX;
                    goto L001;
                case ScriptConst.sSC_SETSCRIPTFLAG:
                    nCMDCode = ScriptConst.nSC_SETSCRIPTFLAG;
                    goto L001;
                case ScriptConst.sSC_SETAUTOGETEXP:
                    nCMDCode = ScriptConst.nSC_SETAUTOGETEXP;
                    goto L001;
                case ScriptConst.sSC_VAR:
                    nCMDCode = ScriptConst.nSC_VAR;
                    goto L001;
                case ScriptConst.sSC_LOADVAR:
                    nCMDCode = ScriptConst.nSC_LOADVAR;
                    goto L001;
                case ScriptConst.sSC_SAVEVAR:
                    nCMDCode = ScriptConst.nSC_SAVEVAR;
                    goto L001;
                case ScriptConst.sSC_CALCVAR:
                    nCMDCode = ScriptConst.nSC_CALCVAR;
                    goto L001;
                case ScriptConst.sSC_AUTOADDGAMEGOLD:
                    nCMDCode = ScriptConst.nSC_AUTOADDGAMEGOLD;
                    goto L001;
                case ScriptConst.sSC_AUTOSUBGAMEGOLD:
                    nCMDCode = ScriptConst.nSC_AUTOSUBGAMEGOLD;
                    goto L001;
                case ScriptConst.sSC_RECALLGROUPMEMBERS:
                    nCMDCode = ScriptConst.nSC_RECALLGROUPMEMBERS;
                    goto L001;
                case ScriptConst.sSC_CLEARNAMELIST:
                    nCMDCode = ScriptConst.nSC_CLEARNAMELIST;
                    goto L001;
                case ScriptConst.sSC_CHANGENAMECOLOR:
                    nCMDCode = ScriptConst.nSC_CHANGENAMECOLOR;
                    goto L001;
                case ScriptConst.sSC_CLEARPASSWORD:
                    nCMDCode = ScriptConst.nSC_CLEARPASSWORD;
                    goto L001;
                case ScriptConst.sSC_RENEWLEVEL:
                    nCMDCode = ScriptConst.nSC_RENEWLEVEL;
                    goto L001;
                case ScriptConst.sSC_KILLMONEXPRATE:
                    nCMDCode = ScriptConst.nSC_KILLMONEXPRATE;
                    goto L001;
                case ScriptConst.sSC_POWERRATE:
                    nCMDCode = ScriptConst.nSC_POWERRATE;
                    goto L001;
                case ScriptConst.sSC_CHANGEPERMISSION:
                    nCMDCode = ScriptConst.nSC_CHANGEPERMISSION;
                    goto L001;
                case ScriptConst.sSC_KILL:
                    nCMDCode = ScriptConst.nSC_KILL;
                    goto L001;
                case ScriptConst.sSC_KICK:
                    nCMDCode = ScriptConst.nSC_KICK;
                    goto L001;
                case ScriptConst.sSC_BONUSPOINT:
                    nCMDCode = ScriptConst.nSC_BONUSPOINT;
                    goto L001;
                case ScriptConst.sSC_RESTRENEWLEVEL:
                    nCMDCode = ScriptConst.nSC_RESTRENEWLEVEL;
                    goto L001;
                case ScriptConst.sSC_DELMARRY:
                    nCMDCode = ScriptConst.nSC_DELMARRY;
                    goto L001;
                case ScriptConst.sSC_DELMASTER:
                    nCMDCode = ScriptConst.nSC_DELMASTER;
                    goto L001;
                case ScriptConst.sSC_MASTER:
                    nCMDCode = ScriptConst.nSC_MASTER;
                    goto L001;
                case ScriptConst.sSC_UNMASTER:
                    nCMDCode = ScriptConst.nSC_UNMASTER;
                    goto L001;
                case ScriptConst.sSC_CREDITPOINT:
                    nCMDCode = ScriptConst.nSC_CREDITPOINT;
                    goto L001;
                case ScriptConst.sSC_CLEARNEEDITEMS:
                    nCMDCode = ScriptConst.nSC_CLEARNEEDITEMS;
                    goto L001;
                case ScriptConst.sSC_CLEARMAKEITEMS:
                    nCMDCode = ScriptConst.nSC_CLEARMAEKITEMS;
                    goto L001;
                case ScriptConst.sSC_SETSENDMSGFLAG:
                    nCMDCode = ScriptConst.nSC_SETSENDMSGFLAG;
                    goto L001;
                case ScriptConst.sSC_UPGRADEITEMS:
                    nCMDCode = ScriptConst.nSC_UPGRADEITEMS;
                    goto L001;
                case ScriptConst.sSC_UPGRADEITEMSEX:
                    nCMDCode = ScriptConst.nSC_UPGRADEITEMSEX;
                    goto L001;
                case ScriptConst.sSC_MONGENEX:
                    nCMDCode = ScriptConst.nSC_MONGENEX;
                    goto L001;
                case ScriptConst.sSC_CLEARMAPMON:
                    nCMDCode = ScriptConst.nSC_CLEARMAPMON;
                    goto L001;
                case ScriptConst.sSC_SETMAPMODE:
                    nCMDCode = ScriptConst.nSC_SETMAPMODE;
                    goto L001;
                case ScriptConst.sSC_KILLSLAVE:
                    nCMDCode = ScriptConst.nSC_KILLSLAVE;
                    goto L001;
                case ScriptConst.sSC_CHANGEGENDER:
                    nCMDCode = ScriptConst.nSC_CHANGEGENDER;
                    goto L001;
                case ScriptConst.sSC_MAPTING:
                    nCMDCode = ScriptConst.nSC_MAPTING;
                    goto L001;
                case ScriptConst.sSC_GUILDRECALL:
                    nCMDCode = ScriptConst.nSC_GUILDRECALL;
                    goto L001;
                case ScriptConst.sSC_GROUPRECALL:
                    nCMDCode = ScriptConst.nSC_GROUPRECALL;
                    goto L001;
                case ScriptConst.sSC_GROUPADDLIST:
                    nCMDCode = ScriptConst.nSC_GROUPADDLIST;
                    goto L001;
                case ScriptConst.sSC_CLEARLIST:
                    nCMDCode = ScriptConst.nSC_CLEARLIST;
                    goto L001;
                case ScriptConst.sSC_GROUPMOVEMAP:
                    nCMDCode = ScriptConst.nSC_GROUPMOVEMAP;
                    goto L001;
                case ScriptConst.sSC_SAVESLAVES:
                    nCMDCode = ScriptConst.nSC_SAVESLAVES;
                    goto L001;
                case ScriptConst.sCHECKUSERDATE:
                    nCMDCode = ScriptConst.nCHECKUSERDATE;
                    goto L001;
                case ScriptConst.sADDUSERDATE:
                    nCMDCode = ScriptConst.nADDUSERDATE;
                    goto L001;
                case ScriptConst.sCheckDiemon:
                    nCMDCode = ScriptConst.nCheckDiemon;
                    goto L001;
                case ScriptConst.scheckkillplaymon:
                    nCMDCode = ScriptConst.ncheckkillplaymon;
                    goto L001;
                case ScriptConst.sSC_CHECKRANDOMNO:
                    nCMDCode = ScriptConst.nSC_CHECKRANDOMNO;
                    goto L001;
                case ScriptConst.sSC_CHECKISONMAP:
                    nCMDCode = ScriptConst.nSC_CHECKISONMAP;
                    goto L001;
                case ScriptConst.sSC_CHECKINSAFEZONE:
                    nCMDCode = ScriptConst.nSC_CHECKINSAFEZONE;
                    goto L001;
                case ScriptConst.sSC_KILLBYHUM:
                    nCMDCode = ScriptConst.nSC_KILLBYHUM;
                    goto L001;
                case ScriptConst.sSC_KILLBYMON:
                    nCMDCode = ScriptConst.nSC_KILLBYMON;
                    goto L001;
                case ScriptConst.sSC_ISHIGH:
                    nCMDCode = ScriptConst.nSC_ISHIGH;
                    goto L001;
                case ScriptConst.sOPENYBDEAL:
                    nCMDCode = ScriptConst.nOPENYBDEAL;
                    goto L001;
                case ScriptConst.sQUERYYBSELL:
                    nCMDCode = ScriptConst.nQUERYYBSELL;
                    goto L001;
                case ScriptConst.sQUERYYBDEAL:
                    nCMDCode = ScriptConst.nQUERYYBDEAL;
                    goto L001;
                case ScriptConst.sDELAYGOTO:
                case ScriptConst.sDELAYCALL:
                    nCMDCode = ScriptConst.nDELAYGOTO;
                    goto L001;
                case ScriptConst.sCLEARDELAYGOTO:
                    nCMDCode = ScriptConst.nCLEARDELAYGOTO;
                    goto L001;
                case ScriptConst.sSC_QUERYVALUE:
                    nCMDCode = ScriptConst.nSC_QUERYVALUE;
                    goto L001;
                case ScriptConst.sSC_QUERYITEMDLG:
                    nCMDCode = ScriptConst.nSC_QUERYITEMDLG;
                    goto L001;
                case ScriptConst.sSC_UPGRADEDLGITEM:
                    nCMDCode = ScriptConst.nSC_UPGRADEDLGITEM;
                    goto L001;
                case ScriptConst.sSC_GETDLGITEMVALUE:
                    nCMDCode = ScriptConst.nSC_GETDLGITEMVALUE;
                    goto L001;
                case ScriptConst.sSC_TAKEDLGITEM:
                    nCMDCode = ScriptConst.nSC_TAKEDLGITEM;
                    goto L001;
            }
        L001:
            if (nCMDCode > 0) {
                QuestActionInfo.nCmdCode = nCMDCode;
                if (!string.IsNullOrEmpty(sParam1) && sParam1[0] == '\"') {
                    HUtil32.ArrestStringEx(sParam1, "\"", "\"", ref sParam1);
                }
                if (!string.IsNullOrEmpty(sParam2) && sParam2[0] == '\"') {
                    HUtil32.ArrestStringEx(sParam2, "\"", "\"", ref sParam2);
                }
                if (!string.IsNullOrEmpty(sParam3) && sParam3[0] == '\"') {
                    HUtil32.ArrestStringEx(sParam3, "\"", "\"", ref sParam3);
                }
                if (!string.IsNullOrEmpty(sParam4) && sParam4[0] == '\"') {
                    HUtil32.ArrestStringEx(sParam4, "\"", "\"", ref sParam4);
                }
                if (!string.IsNullOrEmpty(sParam5) && sParam5[0] == '\"') {
                    HUtil32.ArrestStringEx(sParam5, "\"", "\"", ref sParam5);
                }
                if (!string.IsNullOrEmpty(sParam6) && sParam6[0] == '\"') {
                    HUtil32.ArrestStringEx(sParam6, "\"", "\"", ref sParam6);
                }
                QuestActionInfo.sParam1 = sParam1;
                QuestActionInfo.sParam2 = sParam2;
                QuestActionInfo.sParam3 = sParam3;
                QuestActionInfo.sParam4 = sParam4;
                QuestActionInfo.sParam5 = sParam5;
                QuestActionInfo.sParam6 = sParam6;
                if (HUtil32.IsStringNumber(sParam1)) {
                    QuestActionInfo.nParam1 = HUtil32.StrToInt(sParam1, 0);
                }
                if (HUtil32.IsStringNumber(sParam2)) {
                    QuestActionInfo.nParam2 = HUtil32.StrToInt(sParam2, 1);
                }
                if (HUtil32.IsStringNumber(sParam3)) {
                    QuestActionInfo.nParam3 = HUtil32.StrToInt(sParam3, 1);
                }
                if (HUtil32.IsStringNumber(sParam4)) {
                    QuestActionInfo.nParam4 = HUtil32.StrToInt(sParam4, 0);
                }
                if (HUtil32.IsStringNumber(sParam5)) {
                    QuestActionInfo.nParam5 = HUtil32.StrToInt(sParam5, 0);
                }
                if (HUtil32.IsStringNumber(sParam6)) {
                    QuestActionInfo.nParam6 = HUtil32.StrToInt(sParam6, 0);
                }
                result = true;
            }
            return result;
        }

        private static string GetScriptCrossPath(string path)
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? path : path.Replace("\\", "/");
        }

        /// <summary>
        /// 加载NPC脚本
        /// </summary>
        /// <returns></returns>
        public void LoadScriptFile(NormNpc NPC, string sPatch, string sScritpName, bool boFlag) {
            var command = string.Empty;
            var questName = string.Empty;
            var s44 = string.Empty;
            var slabName = string.Empty;
            var boDefine = false;
            ScriptInfo Script = null;
            SayingRecord SayingRecord = null;
            SayingProcedure SayingProcedure = null;
            var scriptType = 0;
            var questCount = 0;
            var sScritpFileName = M2Share.GetEnvirFilePath(sPatch, GetScriptCrossPath($"{sScritpName}.txt"));
            if (File.Exists(sScritpFileName)) {
                CallScriptDict.Clear();
                var stringList = new StringList();
                stringList.LoadFromFile(sScritpFileName);
                var success = false;
                while (!success) {
                    LoadCallScript(ref stringList, ref success);
                }
                IList<DefineInfo> defineList = new List<DefineInfo>();
                var defline = LoadScriptDefineInfo(stringList, defineList);
                var defineInfo = new DefineInfo { Name = "@HOME" };
                if (string.IsNullOrEmpty(defline)) {
                    defline = "@main";
                }
                defineInfo.Text = defline;
                defineList.Add(defineInfo);
                int n24;
                // 常量处理
                for (var i = 0; i < stringList.Count; i++) {
                    var line = stringList[i].Trim();
                    if (!string.IsNullOrEmpty(line)) {
                        if (line[0] == '[') {
                            boDefine = false;
                        }
                        else {
                            if (line[0] == '#' && (HUtil32.CompareLStr(line, "#IF") || HUtil32.CompareLStr(line, "#ACT") || HUtil32.CompareLStr(line, "#ELSEACT"))) {
                                boDefine = true;
                            }
                            else {
                                if (boDefine) {
                                    // 将Define 好的常量换成指定值
                                    for (var n20 = 0; n20 < defineList.Count; n20++) {
                                        defineInfo = defineList[n20];
                                        var n1C = 0;
                                        while (true) {
                                            n24 = line.ToUpper().IndexOf(defineInfo.Name, StringComparison.OrdinalIgnoreCase);
                                            if (n24 <= 0) {
                                                break;
                                            }
                                            var s58 = line[..n24];
                                            var s5C = line[(defineInfo.Name.Length + n24)..];
                                            line = s58 + defineInfo.Text + s5C;
                                            stringList[i] = line;
                                            n1C++;
                                            if (n1C >= 10) {
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                // 释放常量定义内容
                for (var i = 0; i < defineList.Count; i++) {
                    defineList[i] = null;
                }
                defineList.Clear();
                var nQuestIdx = 0;
                for (var i = 0; i < stringList.Count; i++) {
                    var line = stringList[i].Trim();
                    if (string.IsNullOrEmpty(line) || line[0] == ';' || line[0] == '/') {
                        continue;
                    }
                    if (scriptType == 0 && boFlag) {
                        if (line.StartsWith("%"))// 物品价格倍率
                        {
                            line = line[1..];
                            var nPriceRate = HUtil32.StrToInt(line, -1);
                            if (nPriceRate >= 55) {
                                ((Merchant)NPC).PriceRate = nPriceRate;
                            }
                            continue;
                        }
                        if (line.StartsWith("+"))// 物品交易类型
                        {
                            line = line[1..];
                            var nItemType = HUtil32.StrToInt(line, -1);
                            if (nItemType >= 0) {
                                ((Merchant)NPC).ItemTypeList.Add(nItemType);
                            }
                            continue;
                        }
                        if (line.StartsWith("("))// 增加处理NPC可执行命令设置
                        {
                            HUtil32.ArrestStringEx(line, "(", ")", ref line);
                            if (!string.IsNullOrEmpty(line)) {
                                while (!string.IsNullOrEmpty(line)) {
                                    line = HUtil32.GetValidStr3(line, ref command, HUtil32.Separator);
                                    if (command.Equals(ScriptConst.sBUY, StringComparison.OrdinalIgnoreCase)) {
                                        ((Merchant)NPC).IsBuy = true;
                                        continue;
                                    }
                                    if (command.Equals(ScriptConst.sSELL, StringComparison.OrdinalIgnoreCase)) {
                                        ((Merchant)NPC).IsSell = true;
                                        continue;
                                    }
                                    if (command.Equals(ScriptConst.sMAKEDURG, StringComparison.OrdinalIgnoreCase)) {
                                        ((Merchant)NPC).IsMakeDrug = true;
                                        continue;
                                    }
                                    if (command.Equals(ScriptConst.sPRICES, StringComparison.OrdinalIgnoreCase)) {
                                        ((Merchant)NPC).IsPrices = true;
                                        continue;
                                    }
                                    if (command.Equals(ScriptConst.sSTORAGE, StringComparison.OrdinalIgnoreCase)) {
                                        ((Merchant)NPC).IsStorage = true;
                                        continue;
                                    }
                                    if (command.Equals(ScriptConst.sGETBACK, StringComparison.OrdinalIgnoreCase)) {
                                        ((Merchant)NPC).IsGetback = true;
                                        continue;
                                    }
                                    if (command.Equals(ScriptConst.sUPGRADENOW, StringComparison.OrdinalIgnoreCase)) {
                                        ((Merchant)NPC).IsUpgradenow = true;
                                        continue;
                                    }
                                    if (command.Equals(ScriptConst.sGETBACKUPGNOW, StringComparison.OrdinalIgnoreCase)) {
                                        ((Merchant)NPC).IsGetBackupgnow = true;
                                        continue;
                                    }
                                    if (command.Equals(ScriptConst.sREPAIR, StringComparison.OrdinalIgnoreCase)) {
                                        ((Merchant)NPC).IsRepair = true;
                                        continue;
                                    }
                                    if (command.Equals(ScriptConst.SuperRepair, StringComparison.OrdinalIgnoreCase)) {
                                        ((Merchant)NPC).IsSupRepair = true;
                                        continue;
                                    }
                                    if (command.Equals(ScriptConst.sSL_SENDMSG, StringComparison.OrdinalIgnoreCase)) {
                                        ((Merchant)NPC).IsSendMsg = true;
                                        continue;
                                    }
                                    if (command.Equals(ScriptConst.UseItemName, StringComparison.OrdinalIgnoreCase)) {
                                        ((Merchant)NPC).IsUseItemName = true;
                                        continue;
                                    }
                                    if (command.Equals(ScriptConst.sOFFLINEMSG, StringComparison.OrdinalIgnoreCase)) {
                                        ((Merchant)NPC).IsOffLineMsg = true;
                                        continue;
                                    }
                                    if (string.Compare(command, ScriptConst.sybdeal, StringComparison.OrdinalIgnoreCase) == 0) {
                                        ((Merchant)NPC).IsYbDeal = true;
                                        continue;
                                    }
                                }
                            }
                            continue;
                        }
                        // 增加处理NPC可执行命令设置
                    }
                    string s38;
                    if (line.StartsWith("{")) {
                        if (HUtil32.CompareLStr(line, "{Quest")) {
                            s38 = HUtil32.GetValidStr3(line, ref questName, new[] { ' ', '}', '\t' });
                            HUtil32.GetValidStr3(s38, ref questName, new[] { ' ', '}', '\t' });
                            questCount = HUtil32.StrToInt(questName, 0);
                            Script = LoadMakeNewScript(NPC);
                            Script.QuestCount = questCount;
                            questCount++;
                        }
                        if (HUtil32.CompareLStr(line, "{~Quest")) {
                            continue;
                        }
                    }
                    if (scriptType == 1 && Script != null && line.StartsWith("#")) {
                        s38 = HUtil32.GetValidStr3(line, ref questName, new[] { '=', ' ', '\t' });
                        Script.IsQuest = true;
                        if (HUtil32.CompareLStr(line, "#IF")) {
                            var questFlag = string.Empty;
                            HUtil32.ArrestStringEx(line, "[", "]", ref questFlag);
                            Script.QuestInfo[nQuestIdx].wFlag = HUtil32.StrToInt16(questFlag, 0);
                            HUtil32.GetValidStr3(s38, ref s44, new[] { '=', ' ', '\t' });
                            n24 = HUtil32.StrToInt(s44, 0);
                            if (n24 != 0) {
                                n24 = 1;
                            }
                            Script.QuestInfo[nQuestIdx].btValue = (byte)n24;
                        }
                        if (HUtil32.CompareLStr(line, "#RAND")) {
                            Script.QuestInfo[nQuestIdx].nRandRage = HUtil32.StrToInt(s44, 0);
                        }
                        continue;
                    }
                    if (line.StartsWith("[")) {
                        scriptType = 10;
                        if (Script == null) {
                            Script = LoadMakeNewScript(NPC);
                            Script.QuestCount = questCount;
                        }
                        if (line.Equals("[goods]", StringComparison.OrdinalIgnoreCase)) {
                            scriptType = 20;
                            NPC.ProcessRefillIndex = M2Share.CurrentMerchantIndex;
                            M2Share.CurrentMerchantIndex++;
                            continue;
                        }
                        line = HUtil32.ArrestStringEx(line, "[", "]", ref slabName);
                        SayingRecord = new SayingRecord { sLabel = slabName };
                        line = HUtil32.GetValidStrCap(line, ref slabName, TextSpitConst);
                        if (slabName.Equals("TRUE", StringComparison.OrdinalIgnoreCase)) {
                            SayingRecord.boExtJmp = true;
                        }
                        else {
                            SayingRecord.boExtJmp = false;
                        }
                        SayingProcedure = new SayingProcedure();
                        SayingRecord.ProcedureList.Add(SayingProcedure);
                        if (Script.RecordList.ContainsKey(SayingRecord.sLabel)) {
                            SayingRecord.sLabel += M2Share.RandomNumber.GetRandomNumber(1, 200);
                        }
                        Script.RecordList.Add(SayingRecord.sLabel, SayingRecord);
                        continue;
                    }
                    if (Script != null && SayingRecord != null) {
                        if (line[0] == '#' && scriptType >= 10 && scriptType < 20) {
                            if (line.Equals("#IF", StringComparison.OrdinalIgnoreCase)) {
                                if (SayingProcedure.ConditionList.Count > 0 || !string.IsNullOrEmpty(SayingProcedure.sSayMsg)) {
                                    SayingProcedure = new SayingProcedure();
                                    SayingRecord.ProcedureList.Add(SayingProcedure);
                                }
                                scriptType = 11;
                                continue;
                            }
                            if (line.Equals("#ACT", StringComparison.OrdinalIgnoreCase)) {
                                scriptType = 12;
                                continue;
                            }
                            if (line.Equals("#SAY", StringComparison.OrdinalIgnoreCase)) {
                                scriptType = 10;
                                continue;
                            }
                            if (line.Equals("#ELSEACT", StringComparison.OrdinalIgnoreCase)) {
                                scriptType = 13;
                                continue;
                            }
                            if (line.Equals("#ELSESAY", StringComparison.OrdinalIgnoreCase)) {
                                scriptType = 14;
                            }
                            continue;
                        }
                        switch (scriptType) {
                            case 10:
                                SayingProcedure.sSayMsg += line;
                                break;
                            case 11: {
                                    var questConditionInfo = new QuestConditionInfo();
                                    if (LoadScriptFileQuestCondition(line.Trim(), ref questConditionInfo)) {
                                        SayingProcedure.ConditionList.Add(questConditionInfo);
                                    }
                                    else {
                                        _logger.Error("脚本错误: " + line + " 第:" + i + " 行: " + sScritpFileName);
                                    }
                                    break;
                                }
                            case 12: {
                                    var questActionInfo = new QuestActionInfo();
                                    if (LoadScriptFileQuestAction(line.Trim(), ref questActionInfo)) {
                                        SayingProcedure.ActionList.Add(questActionInfo);
                                    }
                                    else {
                                        _logger.Error("脚本错误: " + line + " 第:" + i + " 行: " + sScritpFileName);
                                    }
                                    break;
                                }
                            case 13: {
                                    var questActionInfo = new QuestActionInfo();
                                    if (LoadScriptFileQuestAction(line.Trim(), ref questActionInfo)) {
                                        SayingProcedure.ElseActionList.Add(questActionInfo);
                                    }
                                    else {
                                        _logger.Error("脚本错误: " + line + " 第:" + i + " 行: " + sScritpFileName);
                                    }
                                    break;
                                }
                            case 14:
                                SayingProcedure.sElseSayMsg += line;
                                break;
                        }
                    }
                    if (scriptType == 20 && boFlag) {
                        var sItemName = string.Empty;
                        var sItemCount = string.Empty;
                        var sItemRefillTime = string.Empty;
                        line = HUtil32.GetValidStrCap(line, ref sItemName, TextSpitConst);
                        line = HUtil32.GetValidStrCap(line, ref sItemCount, TextSpitConst);
                        line = HUtil32.GetValidStrCap(line, ref sItemRefillTime, TextSpitConst);
                        if (!string.IsNullOrEmpty(sItemName) && !string.IsNullOrEmpty(sItemRefillTime)) {
                            if (sItemName[0] == '\"') {
                                HUtil32.ArrestStringEx(sItemName, "\"", "\"", ref sItemName);
                            }
                            var goods = new Goods {
                                ItemName = sItemName,
                                Count = HUtil32.StrToInt(sItemCount, 0),
                                RefillTime = HUtil32.StrToInt(sItemRefillTime, 0),
                                RefillTick = 0
                            };
                            if (M2Share.CanSellItem(sItemName)) {
                                ((Merchant)NPC).RefillGoodsList.Add(goods);
                            }
                        }
                    }
                }
                stringList.Dispose();
            }
            else {
                _logger.Error("Script file not found: " + sScritpFileName);
            }
        }
       
        /// <summary>
        /// 格式化标签
        /// </summary>
        /// <param name="sLabel"></param>
        /// <param name="boChange"></param>
        /// <returns></returns>
        private static string FormatLabelStr(string sLabel, ref bool boChange) {
            var result = sLabel;
            if (sLabel.IndexOf(")", StringComparison.OrdinalIgnoreCase) > -1) {
                HUtil32.GetValidStr3(sLabel, ref result, '(');
                boChange = true;
            }
            return result;
        }

        /// <summary>
        /// 初始化脚本处理方法
        /// </summary>
        /// <returns></returns>
        protected string InitializeProcedure(string sMsg) {
            var nC = 0;
            var sCmd = string.Empty;
            var tempstr = sMsg;
            while (true) {
                if (tempstr.IndexOf(">", StringComparison.OrdinalIgnoreCase) < -1) {
                    break;
                }
                tempstr = HUtil32.ArrestStringEx(tempstr, "<", ">", ref sCmd);
                if (!string.IsNullOrEmpty(sCmd)) {
                    if (sCmd[0] == '$') {
                        InitializeVariable(sCmd, ref sMsg);
                    }
                }
                else {
                    break;
                }
                nC++;
                if (nC > 100) {
                    break;
                }
            }
            return sMsg;
        }

        /// <summary>
        /// 初始化NPC显示内容
        /// </summary>
        /// <param name="sMsg">标签内容</param>
        /// <param name="StringList">新标签列表</param>
        /// <param name="OldStringList">老标签列表</param>
        /// <param name="ScriptNameList">标签列表</param>
        /// <returns></returns>
        private string InitializeSayMsg(string sMsg, List<string> StringList, IList<string> OldStringList, IList<string> ScriptNameList) {
            var nC = 0;
            var s10 = string.Empty;
            var tempstr = sMsg;
            string sLabel;
            var sname = string.Empty;
            int nIdx;
            var nChangeIndex = 1;
            var nNotIdx = -1;
            var boChange = false;
            var boAddResetLabel = false;
            while (true) {
                if (string.IsNullOrEmpty(tempstr)) {
                    break;
                }
                if (tempstr.IndexOf('>') <= 0) {
                    break;
                }
                tempstr = HUtil32.ArrestStringEx(tempstr, "<", ">", ref s10);
                if (!string.IsNullOrEmpty(s10)) {
                    if (s10.IndexOf("/", StringComparison.OrdinalIgnoreCase) > 0) {
                        sLabel = HUtil32.GetValidStr3(s10, ref sname, '/');
                        if (string.Compare(sLabel, "@close", StringComparison.OrdinalIgnoreCase) == 0) {
                            continue;
                        }
                        if (string.Compare(sLabel, "@Exit", StringComparison.OrdinalIgnoreCase) == 0) {
                            continue;
                        }
                        if (string.Compare(sLabel, "@main", StringComparison.OrdinalIgnoreCase) == 0) {
                            continue;
                        }
                        if (HUtil32.CompareLStr(sLabel, "FCOLOR", 6)) {
                            continue;
                        }
                        if (HUtil32.CompareLStr(sLabel, "@Move(", 6)) {
                            continue;
                        }
                        if (HUtil32.CompareLStr(sLabel, "~@", 2)) {
                            continue;
                        }
                        if (HUtil32.CompareLStr(sLabel, "@@", 2)) {
                            if (!boAddResetLabel) {
                                boAddResetLabel = true;
                                sMsg = ScriptConst.RESETLABEL + sMsg;
                            }
                            continue;
                        }
                        nIdx = ScriptNameList.IndexOf(FormatLabelStr(sLabel, ref boChange));
                        if (nIdx == -1) {
                            nIdx = nNotIdx;
                            nNotIdx -= 1;
                        }
                        else if (boChange) {
                            nIdx = nChangeIndex * 100000 + nIdx;
                            nChangeIndex++;
                        }
                        OldStringList.Add(sLabel);
                        try {
                            if (sLabel.Length >= 2 && sLabel[1] == '@' && sLabel[0] == '@') {
                                sLabel = "@@" + nIdx;
                            }
                            else {
                                sLabel = "@" + nIdx;
                            }
                        }
                        finally {
                            StringList.Add(sLabel);
                        }
                        sMsg = sMsg.Replace("<" + s10 + ">", "<" + sname + "/" + sLabel + ">");
                    }
                    else if (s10[0] == '$') {
                        //游戏变量处理
                        InitializeVariable(s10, ref sMsg);
                    }
                }
                else {
                    break;
                }
                nC++;
                if (nC >= 100) {
                    break;
                }
            }
            return sMsg;
        }

        /// <summary>
        /// 初始化全局变量脚本
        /// </summary>
        private static void InitializeVariable(string sLabel, ref string sMsg) {
            var s14 = string.Empty;
            var sLabel2 = sLabel.ToUpper();
            if (sLabel2 == ScriptConst.sVAR_SERVERNAME) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_SERVERNAME);
            }
            else if (sLabel2 == ScriptConst.sVAR_SERVERIP) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_SERVERIP);
            }
            else if (sLabel2 == ScriptConst.sVAR_WEBSITE) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_WEBSITE);
            }
            else if (sLabel2 == ScriptConst.sVAR_BBSSITE) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_BBSSITE);
            }
            else if (sLabel2 == ScriptConst.sVAR_CLIENTDOWNLOAD) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CLIENTDOWNLOAD);
            }
            else if (sLabel2 == ScriptConst.sVAR_QQ) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_QQ);
            }
            else if (sLabel2 == ScriptConst.sVAR_PHONE) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_PHONE);
            }
            else if (sLabel2 == ScriptConst.sVAR_BANKACCOUNT0) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_BANKACCOUNT0);
            }
            else if (sLabel2 == ScriptConst.sVAR_BANKACCOUNT1) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_BANKACCOUNT1);
            }
            else if (sLabel2 == ScriptConst.sVAR_BANKACCOUNT2) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_BANKACCOUNT2);
            }
            else if (sLabel2 == ScriptConst.sVAR_BANKACCOUNT3) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_BANKACCOUNT3);
            }
            else if (sLabel2 == ScriptConst.sVAR_BANKACCOUNT4) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_BANKACCOUNT4);
            }
            else if (sLabel2 == ScriptConst.sVAR_BANKACCOUNT5) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_BANKACCOUNT5);
            }
            else if (sLabel2 == ScriptConst.sVAR_BANKACCOUNT6) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_BANKACCOUNT6);
            }
            else if (sLabel2 == ScriptConst.sVAR_BANKACCOUNT7) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_BANKACCOUNT7);
            }
            else if (sLabel2 == ScriptConst.sVAR_BANKACCOUNT8) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_BANKACCOUNT8);
            }
            else if (sLabel2 == ScriptConst.sVAR_BANKACCOUNT9) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_BANKACCOUNT9);
            }
            else if (sLabel2 == ScriptConst.sVAR_GAMEGOLDNAME) {
                //sMsg = sMsg.Replace("<" + sLabel + ">", Grobal2.sSTRING_GAMEGOLD);
            }
            else if (sLabel2 == ScriptConst.sVAR_GAMEPOINTNAME) {
                // sMsg = sMsg.Replace("<" + sLabel + ">", Grobal2.sSTRING_GAMEPOINT);
            }
            else if (sLabel2 == ScriptConst.sVAR_USERCOUNT) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_USERCOUNT);
            }
            else if (sLabel2 == ScriptConst.sVAR_DATETIME) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_DATETIME);
            }
            else if (sLabel2 == ScriptConst.sVAR_USERNAME) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_USERNAME);
            }
            else if (sLabel2 == ScriptConst.sVAR_FBMAPNAME) { //副本
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_FBMAPNAME);
            }
            else if (sLabel2 == ScriptConst.sVAR_FBMAP) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_FBMAP);
            }
            else if (sLabel2 == ScriptConst.sVAR_ACCOUNT) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_ACCOUNT);
            }
            else if (sLabel2 == ScriptConst.sVAR_ASSEMBLEITEMNAME) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_ASSEMBLEITEMNAME);
            }
            else if (sLabel2 == ScriptConst.sVAR_MAPNAME) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_MAPNAME);
            }
            else if (sLabel2 == ScriptConst.sVAR_GUILDNAME) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_GUILDNAME);
            }
            else if (sLabel2 == ScriptConst.sVAR_RANKNAME) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_RANKNAME);
            }
            else if (sLabel2 == ScriptConst.sVAR_LEVEL) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_LEVEL);
            }
            else if (sLabel2 == ScriptConst.sVAR_HP) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_HP);
            }
            else if (sLabel2 == ScriptConst.sVAR_MAXHP) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_MAXHP);
            }
            else if (sLabel2 == ScriptConst.sVAR_MP) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_MP);
            }
            else if (sLabel2 == ScriptConst.sVAR_MAXMP) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_MAXMP);
            }
            else if (sLabel2 == ScriptConst.sVAR_AC) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_AC);
            }
            else if (sLabel2 == ScriptConst.sVAR_MAXAC) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_MAXAC);
            }
            else if (sLabel2 == ScriptConst.sVAR_MAC) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_MAC);
            }
            else if (sLabel2 == ScriptConst.sVAR_MAXMAC) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_MAXMAC);
            }
            else if (sLabel2 == ScriptConst.sVAR_DC) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_DC);
            }
            else if (sLabel2 == ScriptConst.sVAR_MAXDC) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_MAXDC);
            }
            else if (sLabel2 == ScriptConst.sVAR_MC) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_MC);
            }
            else if (sLabel2 == ScriptConst.sVAR_MAXMC) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_MAXMC);
            }
            else if (sLabel2 == ScriptConst.sVAR_SC) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_SC);
            }
            else if (sLabel2 == ScriptConst.sVAR_MAXSC) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_MAXSC);
            }
            else if (sLabel2 == ScriptConst.sVAR_EXP) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_EXP);
            }
            else if (sLabel2 == ScriptConst.sVAR_MAXEXP) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_MAXEXP);
            }
            else if (sLabel2 == ScriptConst.sVAR_PKPOINT) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_PKPOINT);
            }
            else if (sLabel2 == ScriptConst.sVAR_CREDITPOINT) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CREDITPOINT);
            }
            else if (sLabel2 == ScriptConst.sVAR_GOLDCOUNT) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_GOLDCOUNT);
            }
            else if (sLabel2 == ScriptConst.sVAR_GAMEGOLD) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_GAMEGOLD);
            }
            else if (sLabel2 == ScriptConst.sVAR_GAMEPOINT) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_GAMEPOINT);
            }
            else if (sLabel2 == ScriptConst.sVAR_LOGINTIME) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_LOGINTIME);
            }
            else if (sLabel2 == ScriptConst.sVAR_LOGINLONG) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_LOGINLONG);
            }
            else if (sLabel2 == ScriptConst.sVAR_DRESS) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_DRESS);
            }
            else if (sLabel2 == ScriptConst.sVAR_WEAPON) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_WEAPON);
            }
            else if (sLabel2 == ScriptConst.sVAR_RIGHTHAND) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_RIGHTHAND);
            }
            else if (sLabel2 == ScriptConst.sVAR_HELMET) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_HELMET);
            }
            else if (sLabel2 == ScriptConst.sVAR_NECKLACE) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_NECKLACE);
            }
            else if (sLabel2 == ScriptConst.sVAR_RING_R) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_RING_R);
            }
            else if (sLabel2 == ScriptConst.sVAR_RING_L) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_RING_L);
            }
            else if (sLabel2 == ScriptConst.sVAR_ARMRING_R) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_ARMRING_R);
            }
            else if (sLabel2 == ScriptConst.sVAR_ARMRING_L) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_ARMRING_L);
            }
            else if (sLabel2 == ScriptConst.sVAR_BUJUK) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_BUJUK);
            }
            else if (sLabel2 == ScriptConst.sVAR_BELT) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_BELT);
            }
            else if (sLabel2 == ScriptConst.sVAR_BOOTS) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_BOOTS);
            }
            else if (sLabel2 == ScriptConst.sVAR_CHARM) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CHARM);
            }
            //=======================================没有用到的==============================
            else if (sLabel2 == ScriptConst.sVAR_HOUSE) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_HOUSE);
            }
            else if (sLabel2 == ScriptConst.sVAR_CIMELIA) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CIMELIA);
            }
            //=======================================没有用到的==============================
            else if (sLabel2 == ScriptConst.sVAR_IPADDR) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_IPADDR);
            }
            else if (sLabel2 == ScriptConst.sVAR_IPLOCAL) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_IPLOCAL);
            }
            else if (sLabel2 == ScriptConst.sVAR_GUILDBUILDPOINT) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_GUILDBUILDPOINT);
            }
            else if (sLabel2 == ScriptConst.sVAR_GUILDAURAEPOINT) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_GUILDAURAEPOINT);
            }
            else if (sLabel2 == ScriptConst.sVAR_GUILDSTABILITYPOINT) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_GUILDSTABILITYPOINT);
            }
            else if (sLabel2 == ScriptConst.sVAR_GUILDFLOURISHPOINT) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_GUILDFLOURISHPOINT);
            }
            //=================================没用用到的====================================
            else if (sLabel2 == ScriptConst.sVAR_GUILDMONEYCOUNT) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_GUILDMONEYCOUNT);
            }
            //=================================没用用到的结束====================================
            else if (sLabel2 == ScriptConst.sVAR_REQUESTCASTLEWARITEM) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_REQUESTCASTLEWARITEM);
            }
            else if (sLabel2 == ScriptConst.sVAR_REQUESTCASTLEWARDAY) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_REQUESTCASTLEWARDAY);
            }
            else if (sLabel2 == ScriptConst.sVAR_REQUESTBUILDGUILDITEM) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_REQUESTBUILDGUILDITEM);
            }
            else if (sLabel2 == ScriptConst.sVAR_OWNERGUILD) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_OWNERGUILD);
            }
            else if (sLabel2 == ScriptConst.sVAR_CASTLENAME) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CASTLENAME);
            }
            else if (sLabel2 == ScriptConst.sVAR_LORD) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_LORD);
            }
            else if (sLabel2 == ScriptConst.sVAR_GUILDWARFEE) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_GUILDWARFEE);
            }
            else if (sLabel2 == ScriptConst.sVAR_BUILDGUILDFEE) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_BUILDGUILDFEE);
            }
            else if (sLabel2 == ScriptConst.sVAR_CASTLEWARDATE) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CASTLEWARDATE);
            }
            else if (sLabel2 == ScriptConst.sVAR_LISTOFWAR) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_LISTOFWAR);
            }
            else if (sLabel2 == ScriptConst.sVAR_CASTLECHANGEDATE) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CASTLECHANGEDATE);
            }
            else if (sLabel2 == ScriptConst.sVAR_CASTLEWARLASTDATE) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CASTLEWARLASTDATE);
            }
            else if (sLabel2 == ScriptConst.sVAR_CASTLEGETDAYS) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CASTLEGETDAYS);
            }
            else if (sLabel2 == ScriptConst.sVAR_CMD_DATE) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CMD_DATE);
            }
            //===================================没用用到的======================================
            else if (sLabel2 == ScriptConst.sVAR_CMD_PRVMSG) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CMD_PRVMSG);
            }
            //===================================没用用到的结束======================================
            else if (sLabel2 == ScriptConst.sVAR_CMD_ALLOWMSG) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CMD_ALLOWMSG);
            }
            else if (sLabel2 == ScriptConst.sVAR_CMD_LETSHOUT) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CMD_LETSHOUT);
            }
            else if (sLabel2 == ScriptConst.sVAR_CMD_LETTRADE) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CMD_LETTRADE);
            }
            else if (sLabel2 == ScriptConst.sVAR_CMD_LETGuild) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CMD_LETGuild);
            }
            else if (sLabel2 == ScriptConst.sVAR_CMD_ENDGUILD) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CMD_ENDGUILD);
            }
            else if (sLabel2 == ScriptConst.sVAR_CMD_BANGUILDCHAT) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CMD_BANGUILDCHAT);
            }
            else if (sLabel2 == ScriptConst.sVAR_CMD_AUTHALLY) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CMD_AUTHALLY);
            }
            else if (sLabel2 == ScriptConst.sVAR_CMD_AUTH) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CMD_AUTH);
            }
            else if (sLabel2 == ScriptConst.sVAR_CMD_AUTHCANCEL) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CMD_AUTHCANCEL);
            }
            else if (sLabel2 == ScriptConst.sVAR_CMD_USERMOVE) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CMD_USERMOVE);
            }
            else if (sLabel2 == ScriptConst.sVAR_CMD_SEARCHING) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CMD_SEARCHING);
            }
            else if (sLabel2 == ScriptConst.sVAR_CMD_ALLOWGROUPCALL) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CMD_ALLOWGROUPCALL);
            }
            else if (sLabel2 == ScriptConst.sVAR_CMD_GROUPRECALLL) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CMD_GROUPRECALLL);
            }
            #region 没有使用的
            //===========================================没有使用的========================================
            else if (sLabel2 == ScriptConst.sVAR_CMD_ALLOWGUILDRECALL) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CMD_ALLOWGUILDRECALL);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_CMD_ALLOWGUILDRECALL, SctiptDef.sVAR_CMD_ALLOWGUILDRECALL);
            }
            else if (sLabel2 == ScriptConst.sVAR_CMD_GUILDRECALLL) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CMD_GUILDRECALLL);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_CMD_GUILDRECALLL, SctiptDef.sVAR_CMD_GUILDRECALLL);
            }
            else if (sLabel2 == ScriptConst.sVAR_CMD_DEAR) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CMD_DEAR);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_CMD_DEAR, SctiptDef.sVAR_CMD_DEAR);
            }
            else if (sLabel2 == ScriptConst.sVAR_CMD_ALLOWDEARRCALL) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CMD_ALLOWDEARRCALL);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_CMD_ALLOWDEARRCALL, SctiptDef.sVAR_CMD_ALLOWDEARRCALL);
            }
            else if (sLabel2 == ScriptConst.sVAR_CMD_DEARRECALL) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CMD_DEARRECALL);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_CMD_DEARRECALL, SctiptDef.sVAR_CMD_DEARRECALL);
            }
            else if (sLabel2 == ScriptConst.sVAR_CMD_MASTER) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CMD_MASTER);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_CMD_MASTER, SctiptDef.sVAR_CMD_MASTER);
            }
            else if (sLabel2 == ScriptConst.sVAR_CMD_ALLOWMASTERRECALL) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CMD_ALLOWMASTERRECALL);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_CMD_ALLOWMASTERRECALL, SctiptDef.sVAR_CMD_ALLOWMASTERRECALL);
            }
            else if (sLabel2 == ScriptConst.sVAR_CMD_MASTERECALL) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CMD_MASTERECALL);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_CMD_MASTERECALL, SctiptDef.sVAR_CMD_MASTERECALL);
            }
            else if (sLabel2 == ScriptConst.sVAR_CMD_TAKEONHORSE) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CMD_TAKEONHORSE);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_CMD_TAKEONHORSE, SctiptDef.sVAR_CMD_TAKEONHORSE);
            }
            else if (sLabel2 == ScriptConst.sVAR_CMD_TAKEOFHORSE) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CMD_TAKEOFHORSE);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_CMD_TAKEOFHORSE, SctiptDef.sVAR_CMD_TAKEOFHORSE);
            }
            else if (sLabel2 == ScriptConst.sVAR_CMD_ALLSYSMSG) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CMD_ALLSYSMSG);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_CMD_ALLSYSMSG, SctiptDef.sVAR_CMD_ALLSYSMSG);
            }
            else if (sLabel2 == ScriptConst.sVAR_CMD_MEMBERFUNCTION) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CMD_MEMBERFUNCTION);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_CMD_MEMBERFUNCTION, SctiptDef.sVAR_CMD_MEMBERFUNCTION);
            }
            else if (sLabel2 == ScriptConst.sVAR_CMD_MEMBERFUNCTIONEX) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CMD_MEMBERFUNCTIONEX);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_CMD_MEMBERFUNCTIONEX, SctiptDef.sVAR_CMD_MEMBERFUNCTIONEX);
            }
            else if (sLabel2 == ScriptConst.sVAR_CASTLEGOLD) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CASTLEGOLD);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_CASTLEGOLD, SctiptDef.sVAR_CASTLEGOLD);
            }
            else if (sLabel2 == ScriptConst.sVAR_TODAYINCOME) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_TODAYINCOME);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_TODAYINCOME, SctiptDef.sVAR_TODAYINCOME);
            }
            else if (sLabel2 == ScriptConst.sVAR_CASTLEDOORSTATE) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CASTLEDOORSTATE);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_CASTLEDOORSTATE, SctiptDef.sVAR_CASTLEDOORSTATE);
            }
            else if (sLabel2 == ScriptConst.sVAR_REPAIRDOORGOLD) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_REPAIRDOORGOLD);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_REPAIRDOORGOLD, SctiptDef.sVAR_REPAIRDOORGOLD);
            }
            else if (sLabel2 == ScriptConst.sVAR_REPAIRWALLGOLD) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_REPAIRWALLGOLD);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_REPAIRWALLGOLD, SctiptDef.sVAR_REPAIRWALLGOLD);
            }
            else if (sLabel2 == ScriptConst.sVAR_GUARDFEE) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_GUARDFEE);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_GUARDFEE, SctiptDef.sVAR_GUARDFEE);
            }
            else if (sLabel2 == ScriptConst.sVAR_ARCHERFEE) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_ARCHERFEE);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_ARCHERFEE, SctiptDef.sVAR_ARCHERFEE);
            }
            else if (sLabel2 == ScriptConst.sVAR_GUARDRULE) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_GUARDRULE);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_GUARDRULE, SctiptDef.sVAR_GUARDRULE);
            }
            else if (sLabel2 == ScriptConst.sVAR_STORAGE2STATE) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_STORAGE2STATE);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_STORAGE2STATE, SctiptDef.sVAR_STORAGE2STATE);
            }
            else if (sLabel2 == ScriptConst.sVAR_STORAGE3STATE) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_STORAGE3STATE);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_STORAGE3STATE, SctiptDef.sVAR_STORAGE3STATE);
            }
            else if (sLabel2 == ScriptConst.sVAR_STORAGE4STATE) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_STORAGE4STATE);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_STORAGE4STATE, SctiptDef.sVAR_STORAGE4STATE);
            }
            else if (sLabel2 == ScriptConst.sVAR_STORAGE5STATE) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_STORAGE5STATE);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_STORAGE5STATE, SctiptDef.sVAR_STORAGE5STATE);
            }
            else if (sLabel2 == ScriptConst.sVAR_SELFNAME) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_SELFNAME);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_SELFNAME, SctiptDef.sVAR_SELFNAME);
            }
            else if (sLabel2 == ScriptConst.sVAR_POSENAME) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_POSENAME);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_POSENAME, SctiptDef.sVAR_POSENAME);
            }
            else if (sLabel2 == ScriptConst.sVAR_GAMEDIAMOND) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_GAMEDIAMOND);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_GAMEDIAMOND, SctiptDef.sVAR_GAMEDIAMOND);
            }
            else if (sLabel2 == ScriptConst.sVAR_GAMEGIRD) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_GAMEGIRD);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_GAMEGIRD, SctiptDef.sVAR_GAMEGIRD);
            }
            else if (sLabel2 == ScriptConst.sVAR_CMD_ALLOWFIREND) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CMD_ALLOWFIREND);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_CMD_ALLOWFIREND, SctiptDef.sVAR_CMD_ALLOWFIREND);
            }
            else if (sLabel2 == ScriptConst.sVAR_EFFIGYSTATE) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_EFFIGYSTATE);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_EFFIGYSTATE, SctiptDef.sVAR_EFFIGYSTATE);
            }
            else if (sLabel2 == ScriptConst.sVAR_EFFIGYOFFSET) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_EFFIGYOFFSET);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_EFFIGYOFFSET, SctiptDef.sVAR_EFFIGYOFFSET);
            }
            else if (sLabel2 == ScriptConst.sVAR_YEAR) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_YEAR);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_YEAR, SctiptDef.sVAR_YEAR);
            }
            else if (sLabel2 == ScriptConst.sVAR_MONTH) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_MONTH);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_MONTH, SctiptDef.sVAR_MONTH);
            }
            else if (sLabel2 == ScriptConst.sVAR_DAY) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_DAY);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_DAY, SctiptDef.sVAR_DAY);
            }
            else if (sLabel2 == ScriptConst.sVAR_HOUR) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_HOUR);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_HOUR, SctiptDef.sVAR_HOUR);
            }
            else if (sLabel2 == ScriptConst.sVAR_MINUTE) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_MINUTE);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_MINUTE, SctiptDef.sVAR_MINUTE);
            }
            else if (sLabel2 == ScriptConst.sVAR_SEC) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_SEC);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_SEC, SctiptDef.sVAR_SEC);
            }
            else if (sLabel2 == ScriptConst.sVAR_MAP) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_MAP);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_MAP, SctiptDef.sVAR_MAP);
            }
            else if (sLabel2 == ScriptConst.sVAR_X) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_X);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_X, SctiptDef.sVAR_X);
            }
            else if (sLabel2 == ScriptConst.sVAR_Y) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_Y);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_Y, SctiptDef.sVAR_Y);
            }
            else if (sLabel2 == ScriptConst.sVAR_UNMASTER_FORCE) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_UNMASTER_FORCE);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_UNMASTER_FORCE, SctiptDef.sVAR_UNMASTER_FORCE);
            }
            else if (sLabel2 == ScriptConst.sVAR_USERGOLDCOUNT) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_USERGOLDCOUNT);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_USERGOLDCOUNT, SctiptDef.sVAR_USERGOLDCOUNT);
            }
            else if (sLabel2 == ScriptConst.sVAR_MAXGOLDCOUNT) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_MAXGOLDCOUNT);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_MAXGOLDCOUNT, SctiptDef.sVAR_MAXGOLDCOUNT);
            }
            else if (sLabel2 == ScriptConst.sVAR_STORAGEGOLDCOUNT) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_STORAGEGOLDCOUNT);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_STORAGEGOLDCOUNT, SctiptDef.sVAR_STORAGEGOLDCOUNT);
            }
            else if (sLabel2 == ScriptConst.sVAR_BINDGOLDCOUNT) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_BINDGOLDCOUNT);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_BINDGOLDCOUNT, SctiptDef.sVAR_BINDGOLDCOUNT);
            }
            else if (sLabel2 == ScriptConst.sVAR_UPGRADEWEAPONFEE) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_UPGRADEWEAPONFEE);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_UPGRADEWEAPONFEE, SctiptDef.sVAR_UPGRADEWEAPONFEE);
            }
            else if (sLabel2 == ScriptConst.sVAR_USERWEAPON) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_USERWEAPON);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_USERWEAPON, SctiptDef.sVAR_USERWEAPON);
            }
            else if (sLabel2 == ScriptConst.sVAR_CMD_STARTQUEST) {
                sMsg = sMsg.Replace("<" + sLabel + ">", ScriptConst.tVAR_CMD_STARTQUEST);
                //SctiptDef.g_GrobalManage.Add(SctiptDef.nVAR_CMD_STARTQUEST, SctiptDef.sVAR_CMD_STARTQUEST);
            }
            //===========================================没有使用的========================================
            #endregion
            else if (HUtil32.CompareLStr(sLabel2, ScriptConst.sVAR_TEAM)) {
                s14 = sLabel2.Substring(ScriptConst.sVAR_TEAM.Length + 1 - 1, 1);
                if (!string.IsNullOrEmpty(s14)) {
                    sMsg = sMsg.Replace("<" + sLabel + ">", string.Format(ScriptConst.tVAR_TEAM, s14));
                }
                else {
                    sMsg = sMsg.Replace("<" + sLabel + ">", "????");
                }
            }
            else if (HUtil32.CompareLStr(sLabel2, ScriptConst.sVAR_HUMAN)) {
                HUtil32.ArrestStringEx(sLabel, "(", ")", ref s14);
                if (!string.IsNullOrEmpty(s14)) {
                    sMsg = sMsg.Replace("<" + sLabel + ">", string.Format(ScriptConst.tVAR_HUMAN, s14));
                }
                else {
                    sMsg = sMsg.Replace("<" + sLabel + ">", "????");
                }
            }
            else if (HUtil32.CompareLStr(sLabel2, ScriptConst.sVAR_GUILD)) {
                HUtil32.ArrestStringEx(sLabel, "(", ")", ref s14);
                if (!string.IsNullOrEmpty(s14)) {
                    sMsg = sMsg.Replace("<" + sLabel + ">", string.Format(ScriptConst.tVAR_GUILD, s14));
                }
                else {
                    sMsg = sMsg.Replace("<" + sLabel + ">", "????");
                }
            }
            else if (HUtil32.CompareLStr(sLabel2, ScriptConst.sVAR_GLOBAL)) {
                HUtil32.ArrestStringEx(sLabel, "(", ")", ref s14);
                if (!string.IsNullOrEmpty(s14)) {
                    sMsg = sMsg.Replace("<" + sLabel + ">", string.Format(ScriptConst.tVAR_GLOBAL, s14));
                }
                else {
                    sMsg = sMsg.Replace("<" + sLabel + ">", "????");
                }
            }
            else if (HUtil32.CompareLStr(sLabel2, ScriptConst.sVAR_STR)) {
                //'欢迎使用个人银行储蓄，目前完全免费，请多利用。\ \<您的个人银行存款有/@-1>：<$46><｜/@-2><$125/G18>\ \<您的包裹里以携带有/AUTOCOLOR=249>：<$GOLDCOUNT><｜/@-2><$GOLDCOUNTX>\ \ \<存入金币/@@InPutInteger1>      <取出金币/@@InPutInteger2>      <返 回/@Main>'
                HUtil32.ArrestStringEx(sLabel, "(", ")", ref s14);
                if (!string.IsNullOrEmpty(s14)) {
                    sMsg = sMsg.Replace("<" + sLabel + ">", string.Format(ScriptConst.tVAR_STR, s14));
                }
                else {
                    sMsg = sMsg.Replace("<" + sLabel + ">", "????");
                }
            }
            else if (HUtil32.CompareLStr(sLabel2, ScriptConst.sVAR_MISSIONARITHMOMETER)) {
                HUtil32.ArrestStringEx(sLabel, "(", ")", ref s14);
                if (!string.IsNullOrEmpty(s14)) {
                    sMsg = sMsg.Replace("<" + sLabel + ">", string.Format(ScriptConst.tVAR_MISSIONARITHMOMETER, s14));
                }
                else {
                    sMsg = sMsg.Replace("<" + sLabel + ">", "????");
                }
            }
            else {
                sMsg = sMsg.Replace("<" + sLabel + ">", "????");
            }
        }

        [GeneratedRegex("#CALL", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.RightToLeft, "zh-CN")]
        private static partial Regex RegexCallCount();
    }
}