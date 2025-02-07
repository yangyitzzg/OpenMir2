﻿using GameSrv.Player;
using SystemModule.Enums;
using SystemModule.Packets.ClientPackets;

namespace GameSrv.GameCommand.Commands {
    /// <summary>
    /// 删除指定玩家技能
    /// </summary>
    [Command("DelSkill", "删除指定玩家技能", "人物名称 技能名称", 10)]
    public class DelSkillCommand : GameCommand {
        [ExecuteCommand]
        public void Execute(string[] @params, PlayObject playObject) {
            if (@params == null) {
                return;
            }
            string sHumanName = @params.Length > 0 ? @params[0] : "";
            string sSkillName = @params.Length > 1 ? @params[1] : "";
            string herostr = @params.Length > 2 ? @params[2] : "";
            bool boDelAll;
            UserMagic userMagic;
            if (string.IsNullOrEmpty(sHumanName) || (string.IsNullOrEmpty(sSkillName))) {
                playObject.SysMsg(Command.CommandHelp, MsgColor.Red, MsgType.Hint);
                return;
            }
            if (string.Compare(sSkillName, "All", StringComparison.OrdinalIgnoreCase) == 0) {
                boDelAll = true;
            }
            else {
                boDelAll = false;
            }
            PlayObject mPlayObject = M2Share.WorldEngine.GetPlayObject(sHumanName);
            if (mPlayObject == null) {
                playObject.SysMsg(string.Format(CommandHelp.NowNotOnLineOrOnOtherServer, sHumanName), MsgColor.Red, MsgType.Hint);
                return;
            }
            for (int i = mPlayObject.MagicList.Count - 1; i >= 0; i--) {
                if (mPlayObject.MagicList.Count <= 0) {
                    break;
                }
                userMagic = mPlayObject.MagicList[i];
                if (userMagic != null) {
                    if (boDelAll) {
                        mPlayObject.MagicList.RemoveAt(i);
                    }
                    else {
                        if (string.Compare(userMagic.Magic.Desc, sSkillName, StringComparison.OrdinalIgnoreCase) == 0) {
                            mPlayObject.SendDelMagic(userMagic);
                            mPlayObject.MagicList.RemoveAt(i);
                            mPlayObject.SysMsg($"技能{sSkillName}已删除。", MsgColor.Green, MsgType.Hint);
                            playObject.SysMsg($"{sHumanName}的技能{sSkillName}已删除。", MsgColor.Green, MsgType.Hint);
                            break;
                        }
                    }
                }
            }
            mPlayObject.RecalcAbilitys();
        }
    }
}