﻿using GameSrv.Player;
using SystemModule.Enums;

namespace GameSrv.GameCommand.Commands {
    /// <summary>
    /// 调整指定玩家游戏币
    /// </summary>
    [Command("AddGameGold", "调整指定玩家游戏币", "人物名称  金币数量", 10)]
    public class AddGameGoldCommand : GameCommand {
        [ExecuteCommand]
        public void Execute(string[] @params, PlayObject playObject) {
            if (@params == null) {
                return;
            }
            string sHumName = @params.Length > 0 ? @params[0] : "";
            int nPoint = @params.Length > 1 ? HUtil32.StrToInt(@params[1], 0) : 0;
            if (playObject.Permission < 6) {
                return;
            }
            if (string.IsNullOrEmpty(sHumName) || nPoint <= 0) {
                playObject.SysMsg(Command.CommandHelp, MsgColor.Red, MsgType.Hint);
                return;
            }
            PlayObject mPlayObject = M2Share.WorldEngine.GetPlayObject(sHumName);
            if (mPlayObject != null) {
                if (mPlayObject.GameGold + nPoint < 2000000) {
                    mPlayObject.GameGold += nPoint;
                }
                else {
                    nPoint = 2000000 - mPlayObject.GameGold;
                    mPlayObject.GameGold = 2000000;
                }
                mPlayObject.GoldChanged();
                playObject.SysMsg(sHumName + "的游戏点已增加" + nPoint + '.', MsgColor.Green, MsgType.Hint);
                mPlayObject.SysMsg("游戏点已增加" + nPoint + '.', MsgColor.Green, MsgType.Hint);
            }
            else {
                playObject.SysMsg(string.Format(CommandHelp.NowNotOnLineOrOnOtherServer, sHumName), MsgColor.Red, MsgType.Hint);
            }
        }
    }
}