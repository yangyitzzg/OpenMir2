﻿using GameSrv.Player;
using SystemModule.Enums;

namespace GameSrv.GameCommand.Commands {
    [Command("ShowDenyAccountLogon", "", 10)]
    public class ShowDenyAccountLogonCommand : GameCommand {
        [ExecuteCommand]
        public void Execute(PlayObject playObject) {
            if (playObject.Permission < 6) {
                return;
            }
            try {
                if (M2Share.DenyAccountList.Count <= 0) {
                    playObject.SysMsg("禁止登录帐号列表为空。", MsgColor.Green, MsgType.Hint);
                    return;
                }
                for (int i = 0; i < M2Share.DenyAccountList.Count; i++) {
                    //PlayObject.SysMsg(Settings.g_DenyAccountList[i], TMsgColor.c_Green, TMsgType.t_Hint);
                }
            }
            finally {
            }
        }
    }
}