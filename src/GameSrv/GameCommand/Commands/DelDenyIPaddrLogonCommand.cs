﻿using GameSrv.Player;
using SystemModule.Enums;

namespace GameSrv.GameCommand.Commands {
    [Command("DelDenyIPaddrLogon", "", "IP地址", 10)]
    public class DelDenyIPaddrLogonCommand : GameCommand {
        [ExecuteCommand]
        public void Execute(string[] @params, PlayObject playObject) {
            if (@params == null) {
                return;
            }
            string sIPaddr = @params.Length > 0 ? @params[0] : "";
            if (string.IsNullOrEmpty(sIPaddr)) {
                playObject.SysMsg(Command.CommandHelp, MsgColor.Red, MsgType.Hint);
                return;
            }
            bool boDelete = false;
            try {
                for (int i = M2Share.DenyIPAddrList.Count - 1; i >= 0; i--) {
                    if (M2Share.DenyIPAddrList.Count <= 0) {
                        break;
                    }
                    //if ((sIPaddr).CompareTo((Settings.g_DenyIPAddrList[i])) == 0)
                    //{
                    //    //if (((int)Settings.g_DenyIPAddrList[i]) != 0)
                    //    //{
                    //    //    M2Share.SaveDenyIPAddrList();
                    //    //}
                    //    Settings.g_DenyIPAddrList.RemoveAt(i);
                    //    PlayObject.SysMsg(sIPaddr + "已从禁止登录IP列表中删除。", TMsgColor.c_Green, TMsgType.t_Hint);
                    //    boDelete = true;
                    //    break;
                    //}
                }
            }
            finally {
            }
            if (!boDelete) {
                playObject.SysMsg(sIPaddr + "没有被禁止登录。", MsgColor.Green, MsgType.Hint);
            }
        }
    }
}