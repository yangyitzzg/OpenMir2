﻿using GameSrv.Actor;
using GameSrv.Guild;

namespace GameSrv.Castle {
    public class CastleConst {
        /// <summary>
        /// 最大守卫次数
        /// </summary>
        public const int MaxCastleArcher = 12;
        /// <summary>
        /// 最大城堡守卫
        /// </summary>
        public const int MaxCalsteGuard = 4;
        /// <summary>
        /// 沙巴克战役列表
        /// </summary>
        public const string AttackSabukWallList = "AttackSabukWall.txt";
        /// <summary>
        /// 沙巴克配置文件
        /// </summary>
        public const string SabukWFileName = "SabukW.txt";
    }

    public struct AttackerInfo : IDisposable {
        public DateTime AttackDate;
        public string sGuildName;
        public GuildInfo Guild;

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        public void Dispose() {
            //通知垃圾回收器不再调用终结器
            GC.SuppressFinalize(this);
        }
    }

    public struct DefenseUnit {
        public int nMainDoorX;
        public int nMainDoorY;
        public string sMainDoorName;
        public bool boXXX;
        public ushort wMainDoorHP;
        public BaseObject MainDoor;
        public BaseObject LeftWall;
        public BaseObject CenterWall;
        public BaseObject RightWall;
        public BaseObject Archer;
    }

    /// <summary>
    /// 守卫
    /// </summary>
    public struct ArcherUnit {
        public short nX;
        public short nY;
        public string sName;
        public bool nStatus;
        public ushort nHP;
        public BaseObject BaseObject;
    }
}

