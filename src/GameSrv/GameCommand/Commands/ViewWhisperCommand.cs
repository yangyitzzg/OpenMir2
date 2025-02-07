﻿using GameSrv.Player;
using SystemModule.Enums;

namespace GameSrv.GameCommand.Commands {
    /// <summary>
    /// 监听指定玩家私聊信息
    /// </summary>
    [Command("ViewWhisper", "监听指定玩家私聊信息", CommandHelp.GameCommandViewWhisperHelpMsg, 10)]
    public class ViewWhisperCommand : GameCommand {
        [ExecuteCommand]
        public void Execute(string[] @params, PlayObject playObject) {
            if (@params == null) {
                return;
            }
            string sChrName = @params.Length > 0 ? @params[0] : "";
            if (string.IsNullOrEmpty(sChrName) || !string.IsNullOrEmpty(sChrName) && sChrName[1] == '?') {
                playObject.SysMsg(Command.CommandHelp, MsgColor.Red, MsgType.Hint);
                return;
            }
            PlayObject mPlayObject = M2Share.WorldEngine.GetPlayObject(sChrName);
            if (mPlayObject != null) {
                if (mPlayObject.WhisperHuman == playObject) {
                    mPlayObject.WhisperHuman = null;
                    playObject.SysMsg(string.Format(CommandHelp.GameCommandViewWhisperMsg1, sChrName), MsgColor.Green, MsgType.Hint);
                }
                else {
                    mPlayObject.WhisperHuman = playObject;
                    playObject.SysMsg(string.Format(CommandHelp.GameCommandViewWhisperMsg2, sChrName), MsgColor.Green, MsgType.Hint);
                }
            }
            else {
                playObject.SysMsg(string.Format(CommandHelp.NowNotOnLineOrOnOtherServer, sChrName), MsgColor.Red, MsgType.Hint);
            }
        }
    }
}