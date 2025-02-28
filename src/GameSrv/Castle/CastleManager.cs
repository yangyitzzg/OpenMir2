﻿using GameSrv.Actor;
using GameSrv.GameCommand;
using GameSrv.Maps;
using GameSrv.Player;
using NLog;
using SystemModule.Common;

namespace GameSrv.Castle {
    public class CastleManager {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IList<UserCastle> _castleList;

        public CastleManager() {
            _castleList = new List<UserCastle>();
        }

        public UserCastle Find(string sCastleName) {
            for (int i = 0; i < _castleList.Count; i++) {
                if (string.Compare(_castleList[i].sName, sCastleName, StringComparison.OrdinalIgnoreCase) == 0) {
                    return _castleList[i];
                }
            }
            return null;
        }

        /// <summary>
        /// 是否沙巴克攻城战役区域
        /// </summary>
        /// <param name="BaseObject"></param>
        /// <returns></returns>
        public UserCastle InCastleWarArea(BaseObject BaseObject) {
            for (int i = 0; i < _castleList.Count; i++) {
                if (_castleList[i].InCastleWarArea(BaseObject.Envir, BaseObject.CurrX, BaseObject.CurrY)) {
                    return _castleList[i];
                }
            }
            return null;
        }

        public UserCastle InCastleWarArea(Envirnoment Envir, int nX, int nY) {
            for (int i = 0; i < _castleList.Count; i++) {
                if (_castleList[i].InCastleWarArea(Envir, nX, nY)) {
                    return _castleList[i];
                }
            }
            return null;
        }

        public void Initialize() {
            UserCastle castle;
            if (_castleList.Count <= 0) {
                castle = new UserCastle(M2Share.Config.CastleDir);
                castle.Initialize();
                castle.ConfigDir = "0";
                castle.EnvirList.Add("0151");
                castle.EnvirList.Add("0152");
                castle.EnvirList.Add("0153");
                castle.EnvirList.Add("0154");
                castle.EnvirList.Add("0155");
                castle.EnvirList.Add("0156");
                _castleList.Add(castle);
                Save();
                return;
            }
            for (int i = 0; i < _castleList.Count; i++) {
                castle = _castleList[i];
                castle.Initialize();
            }
            _logger.Debug("城堡城初始完成...");
        }

        // 城堡皇宫所在地图
        public UserCastle IsCastlePalaceEnvir(Envirnoment Envir) {
            for (int i = 0; i < _castleList.Count; i++) {
                if (_castleList[i].PalaceEnvir == Envir) {
                    return _castleList[i];
                }
            }
            return null;
        }

        // 城堡所在地图
        public UserCastle IsCastleEnvir(Envirnoment envir) {
            for (int i = 0; i < _castleList.Count; i++) {
                if (_castleList[i].CastleEnvir == envir) {
                    return _castleList[i];
                }
            }
            return null;
        }

        public UserCastle IsCastleMember(PlayObject playObject) {
            for (int i = 0; i < _castleList.Count; i++) {
                if (_castleList[i].IsMember(playObject)) {
                    return _castleList[i];
                }
            }
            return null;
        }

        public void Run() {
            for (int i = 0; i < _castleList.Count; i++) {
                _castleList[i].Run();
            }
        }

        public void GetCastleGoldInfo(IList<string> List) {
            for (int i = 0; i < _castleList.Count; i++) {
                UserCastle castle = _castleList[i];
                List.Add(string.Format(CommandHelp.GameCommandSbkGoldShowMsg, castle.sName, castle.TotalGold, castle.TodayIncome));
            }
        }

        public void Save() {
            SaveCastleList();
            for (int i = 0; i < _castleList.Count; i++) {
                UserCastle castle = _castleList[i];
                castle.Save();
            }
        }

        public void LoadCastleList() {
            string castleFile = Path.Combine(M2Share.BasePath, M2Share.Config.CastleFile);
            if (File.Exists(castleFile)) {
                using StringList loadList = new StringList();
                loadList.LoadFromFile(castleFile);
                for (int i = 0; i < loadList.Count; i++) {
                    string sCastleDir = loadList[i].Trim();
                    if (!string.IsNullOrEmpty(sCastleDir)) {
                        UserCastle castle = new UserCastle(sCastleDir);
                        _castleList.Add(castle);
                    }
                }
                _logger.Info($"已读取 [{_castleList.Count}] 个城堡信息...");
            }
            else {
                _logger.Error("城堡列表文件未找到!!!");
            }
        }

        private void SaveCastleList() {
            string castleDirPath = Path.Combine(M2Share.BasePath, M2Share.Config.CastleDir);
            if (!Directory.Exists(castleDirPath)) {
                Directory.CreateDirectory(castleDirPath);
            }
            using StringList loadList = new StringList(_castleList.Count);
            for (int i = 0; i < _castleList.Count; i++) {
                loadList.Add(i.ToString());
            }
            string savePath = Path.Combine(M2Share.BasePath, M2Share.Config.CastleFile);
            loadList.SaveToFile(savePath);
        }

        public UserCastle GetCastle(int nIndex) {
            if (nIndex >= 0 && nIndex < _castleList.Count) {
                return _castleList[nIndex];
            }
            return null;
        }

        public void GetCastleNameList(IList<string> List) {
            for (int i = 0; i < _castleList.Count; i++) {
                List.Add(_castleList[i].sName);
            }
        }

        public void IncRateGold(int nGold) {
            for (int i = 0; i < _castleList.Count; i++) {
                _castleList[i].IncRateGold(nGold);
            }
        }
    }
}