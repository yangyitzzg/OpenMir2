﻿using GameSrv.Player;
using SystemModule.Enums;

namespace GameSrv.GameCommand.Commands {
    [Command("TestStatus", "", 10)]
    public class TestStatusCommand : GameCommand {
        [ExecuteCommand]
        public void Execute(string[] @params, PlayObject playObject) {
            if (@params == null) {
                return;
            }
            int nType = @params.Length > 0 ? HUtil32.StrToInt(@params[0], 0) : 0;
            int nTime = @params.Length > 1 ? HUtil32.StrToInt(@params[1], 0) : 0;
            if (playObject.Permission < 6) {
                return;
            }

            //if ((!(nType >= Grobal2.ushort.GetLowerBound(0) && nType<= Grobal2.ushort..Length)) || (nTime < 0))
            //{
            //    this.SysMsg("命令格式: @" + sCmd + " 类型(0..11) 时长", TMsgColor.c_Red, TMsgType.t_Hint);
            //    return;
            //}
            playObject.StatusTimeArr[nType] = (ushort)(nTime * 1000);
            playObject.StatusArrTick[nType] = HUtil32.GetTickCount();
            playObject.CharStatus = playObject.GetCharStatus();
            playObject.StatusChanged();
            playObject.SysMsg(string.Format("状态编号:{0} 时间长度: {1} 秒", nType, nTime), MsgColor.Green, MsgType.Hint);
        }
    }
}