﻿using GameSrv.Player;
using SystemModule.Enums;

namespace GameSrv.GameCommand.Commands
{
    /// <summary>
    /// 密码锁定
    /// </summary>
    [Command("PasswordLock", "锁定登录", "")]
    public class PasswordLockCommand : GameCommand
    {
        [ExecuteCommand]
        public void Execute(string[] @params, PlayObject playObject)
        {
            if (@params == null || @params.Length <= 0)
            {
                return;
            }
            if (!M2Share.Config.PasswordLockSystem)
            {
                playObject.SysMsg(Settings.NoPasswordLockSystemMsg, MsgColor.Red, MsgType.Hint);
                return;
            }
            if (string.IsNullOrEmpty(playObject.StoragePwd))
            {
                playObject.SendMsg(Messages.RM_PASSWORD, 0, 0, 0, 0);
                playObject.IsSetStoragePwd = true;
                playObject.SysMsg(Settings.SetPasswordMsg, MsgColor.Green, MsgType.Hint);
                return;
            }
            if (playObject.PwdFailCount > 3)
            {
                playObject.SysMsg(Settings.StoragePasswordLockedMsg, MsgColor.Red, MsgType.Hint);
                playObject.IsPasswordLocked = true;
                return;
            }
            if (!string.IsNullOrEmpty(playObject.StoragePwd))
            {
                playObject.SendMsg(Messages.RM_PASSWORD, 0, 0, 0, 0);
                playObject.IsCheckOldPwd = true;
                playObject.SysMsg(Settings.PleaseInputOldPasswordMsg, MsgColor.Green, MsgType.Hint);
            }
        }
    }
}