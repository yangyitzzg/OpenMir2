﻿using System.Collections;
using System.Runtime.CompilerServices;
using GameSrv.Actor;
using GameSrv.Castle;
using GameSrv.Event;
using GameSrv.GameCommand;
using GameSrv.Guild;
using GameSrv.Items;
using GameSrv.Magic;
using GameSrv.Maps;
using GameSrv.Npc;
using GameSrv.RobotPlay;
using GameSrv.Script;
using SystemModule.Consts;
using SystemModule.Data;
using SystemModule.Enums;
using SystemModule.Packets.ClientPackets;
using SystemModule.Packets.ServerPackets;

namespace GameSrv.Player
{
    public partial class PlayObject
    {
        /// <summary>
        /// 性别
        /// </summary>
        public PlayGender Gender;
        /// <summary>
        /// 人物的头发
        /// </summary>
        public byte Hair;
        /// <summary>
        /// 人物的职业 (0:战士 1：法师 2:道士)
        /// </summary>
        public PlayJob Job;
        /// <summary>
        /// 登录帐号名
        /// </summary>
        public string UserAccount;
        /// <summary>
        /// 人物IP地址
        /// </summary>
        public string LoginIpAddr = string.Empty;
        public string LoginIpLocal = string.Empty;
        /// <summary>
        /// 账号过期
        /// </summary>
        public bool AccountExpired;
        /// <summary>
        /// 账号游戏点数检查时间
        /// </summary>
        public int AccountExpiredTick;
        public long ExpireTime;
        public int ExpireCount;
        public int QueryExpireTick;
        /// <summary>
        /// 权限等级
        /// </summary>
        public byte Permission;
        /// <summary>
        /// 人物的幸运值
        /// </summary>
        public byte Luck;
        /// <summary>
        /// 人物身上最多可带金币数
        /// </summary>
        public int GoldMax;
        /// <summary>
        /// 允许行会传送
        /// </summary>
        public bool AllowGuildReCall = false;
        /// <summary>
        /// 允许私聊
        /// </summary>
        public bool HearWhisper;
        /// <summary>
        /// 允许群聊
        /// </summary>
        public bool BanShout;
        /// <summary>
        /// 拒绝行会聊天
        /// </summary>
        public bool BanGuildChat;
        /// <summary>
        /// 是否允许交易
        /// </summary>
        internal bool AllowDeal;
        /// <summary>
        /// 检查重叠人物使用
        /// </summary>
        public bool BoDuplication;
        /// <summary>
        /// 检查重叠人物间隔
        /// </summary>
        public int DupStartTick = 0;
        /// <summary>
        /// 是否用了神水
        /// </summary>
        protected bool UserUnLockDurg;
        /// <summary>
        /// 允许组队
        /// </summary>
        public bool AllowGroup;
        /// <summary>
        /// 允许加入行会
        /// </summary>        
        internal bool AllowGuild;
        /// <summary>
        /// 交易对象
        /// </summary>
        protected PlayObject DealCreat;
        /// <summary>
        /// 正在交易
        /// </summary>
        protected bool Dealing;
        /// <summary>
        /// 交易最后操作时间
        /// </summary>
        internal int DealLastTick = 0;
        /// <summary>
        /// 交易的金币数量
        /// </summary>
        public int DealGolds;
        /// <summary>
        /// 确认交易标志
        /// </summary>
        public bool DealSuccess = false;
        /// <summary>
        /// 回城地图
        /// </summary>
        public string HomeMap;
        /// <summary>
        /// 回城座标X
        /// </summary>
        public short HomeX = 0;
        /// <summary>
        /// 回城座标Y
        /// </summary>
        public short HomeY = 0;
        /// <summary>
        /// 记忆使用间隔
        /// </summary>
        public int GroupRcallTick;
        public short GroupRcallTime;
        /// <summary>
        /// 行会传送
        /// </summary>
        public bool GuildMove = false;
        public CommandMessage ClientMsg;
        /// <summary>
        /// 在行会占争地图中死亡次数
        /// </summary>
        public ushort FightZoneDieCount;
        /// <summary>
        /// 祈祷
        /// </summary>
        protected bool IsSpirit = false;
        /// <summary>
        /// 野蛮冲撞间隔
        /// </summary>
        public int DoMotaeboTick = 0;
        protected bool CrsHitkill = false;
        public bool MBo43Kill = false;
        protected bool RedUseHalfMoon;
        protected bool UseThrusting;
        protected bool UseHalfMoon;
        /// <summary>
        /// 魔法技能
        /// </summary>
        protected UserMagic[] MagicArr;
        /// <summary>
        /// 攻杀剑法
        /// </summary>
        protected bool PowerHit;
        /// <summary>
        /// 烈火剑法
        /// </summary>
        protected bool FireHitSkill;
        /// <summary>
        /// 烈火剑法
        /// </summary>
        protected bool TwinHitSkill;
        /// <summary>
        /// 额外攻击伤害(攻杀)
        /// </summary>
        internal ushort HitPlus;
        /// <summary>
        /// 双倍攻击伤害(烈火专用)
        /// </summary>
        internal ushort HitDouble;
        protected int LatestFireHitTick = 0;
        protected int LatestTwinHitTick = 0;
        /// <summary>
        /// 交易列表
        /// </summary>
        public IList<UserItem> DealItemList;
        /// <summary>
        /// 仓库物品列表
        /// </summary>
        internal readonly IList<UserItem> StorageItemList;
        /// <summary>
        /// 可见事件列表
        /// </summary>
        internal readonly IList<MapEvent> VisibleEvents;
        /// <summary>
        /// 可见物品列表
        /// </summary>
        protected readonly IList<VisibleMapItem> VisibleItems;
        /// <summary>
        /// 禁止私聊人员列表
        /// </summary>
        public IList<string> LockWhisperList;
        /// <summary>
        /// 力量物品(影响力量的物品)
        /// </summary>
        public bool BoPowerItem = false;
        public bool AllowGroupReCall;
        public int HungerStatus = 0;
        public int BonusPoint = 0;
        public byte BtB2;
        /// <summary>
        /// 人物攻击变色标志
        /// </summary>
        public bool PvpFlag;
        /// <summary>
        /// 减PK值时间`
        /// </summary>
        private int DecPkPointTick;
        /// <summary>
        /// 人物的PK值
        /// </summary>
        public int PkPoint;
        /// <summary>
        /// 人物攻击变色时间长度
        /// </summary>
        public int PvpNameColorTick;
        protected bool NameColorChanged;
        /// <summary>
        /// 是否在开行会战
        /// </summary>
        public bool InGuildWarArea;
        public GuildInfo MyGuild;
        public short GuildRankNo;
        public string GuildRankName = string.Empty;
        public string ScriptLable = string.Empty;
        public int SayMsgCount = 0;
        public int SayMsgTick;
        public bool DisableSayMsg;
        public int DisableSayMsgTick;
        public int CheckDupObjTick;
        public int DiscountForNightTick;
        /// <summary>
        /// 是否在安全区域
        /// </summary>
        private bool IsSafeArea;
        /// <summary>
        /// 喊话消息间隔
        /// </summary>
        protected int ShoutMsgTick;
        protected byte AttackSkillCount;
        protected byte AttackSkillPointCount;
        protected bool SmashSet = false;
        protected bool HwanDevilSet = false;
        protected bool PuritySet = false;
        protected bool MundaneSet = false;
        protected bool NokChiSet = false;
        protected bool TaoBuSet = false;
        protected bool FiveStringSet = false;
        public byte ValNpcType;
        public byte ValType;
        public byte ValLabel;
        /// <summary>
        /// 复活戒指使用间隔时间
        /// </summary>
        public int RevivalTick = 0;
        /// <summary>
        /// 掉物品
        /// </summary>
        public bool NoDropItem = false;
        /// <summary>
        /// 探测项链使用间隔
        /// </summary>
        public int ProbeTick;
        /// <summary>
        /// 传送戒指使用间隔
        /// </summary>
        public int TeleportTick;
        protected int DecHungerPointTick;
        /// <summary>
        /// 气血石
        /// </summary>
        protected int AutoAddHpmpMode = 0;
        public int CheckHpmpTick = 0;
        public int KickOffLineTick = 0;
        /// <summary>
        /// 挂机
        /// </summary>
        public bool OffLineFlag = false;
        /// <summary>
        /// 挂机字符
        /// </summary>
        public string OffLineLeaveWord = string.Empty;
        /// <summary>
        /// Socket Handle
        /// </summary>
        public int SocketId = 0;
        /// <summary>
        /// 人物连接到游戏网关SOCKETID
        /// </summary>
        public ushort SocketIdx = 0;
        /// <summary>
        /// 人物所在网关号
        /// </summary>
        public int GateIdx = 0;
        public int SoftVersionDate = 0;
        /// <summary>
        /// 登录时间戳
        /// </summary>
        public long LogonTime;
        /// <summary>
        /// 战领沙城时间
        /// </summary>
        public int LogonTick;
        /// <summary>
        /// 是否进入游戏完成
        /// </summary>
        public bool BoReadyRun;
        /// <summary>
        /// 移动间隔
        /// </summary>
        protected int MapMoveTick;
        /// <summary>
        /// 人物当前付费模式
        /// 1:试玩
        /// 2:付费
        /// 3:测试
        /// </summary>
        public byte PayMent;
        public byte PayMode = 0;
        /// <summary>
        /// 当前会话ID
        /// </summary>
        public int SessionId = 0;
        /// <summary>
        /// 全局会话信息
        /// </summary>
        public PlayerSession SessInfo;
        public int LoadTick = 0;
        /// <summary>
        /// 人物当前所在服务器序号
        /// </summary>
        public byte ServerIndex = 0;
        /// <summary>
        /// 超时关闭链接
        /// </summary>
        public bool BoEmergencyClose;
        /// <summary>
        /// 掉线标志
        /// </summary>
        public bool BoSoftClose;
        /// <summary>
        /// 断线标志(@kick 命令)
        /// </summary>
        public bool BoKickFlag;
        /// <summary>
        /// 是否重连
        /// </summary>
        public bool BoReconnection;
        public bool RcdSaved;
        public bool SwitchData;
        public bool SwitchDataOk = false;
        public string SwitchDataTempFile = string.Empty;
        public int WriteChgDataErrCount;
        public string SwitchMapName = string.Empty;
        public short SwitchMapX = 0;
        public short SwitchMapY = 0;
        public bool SwitchDataSended;
        public int ChgDataWritedTick = 0;
        /// <summary>
        /// 心灵启示
        /// </summary>
        public bool AbilSeeHealGauge;
        /// <summary>
        /// 攻击间隔
        /// </summary>
        public int HitIntervalTime;
        /// <summary>
        /// 魔法间隔
        /// </summary>
        public int MagicHitIntervalTime;
        /// <summary>
        /// 走路间隔
        /// </summary>
        public int RunIntervalTime;
        /// <summary>
        /// 走路间隔
        /// </summary>
        public int WalkIntervalTime;
        /// <summary>
        /// 换方向间隔
        /// </summary>
        public int TurnIntervalTime;
        /// <summary>
        /// 组合操作间隔
        /// </summary>
        public int ActionIntervalTime;
        /// <summary>
        /// 移动刺杀间隔
        /// </summary>
        public int RunLongHitIntervalTime;
        /// <summary>
        /// 跑位攻击间隔
        /// </summary>        
        public int RunHitIntervalTime;
        /// <summary>
        /// 走位攻击间隔
        /// </summary>        
        public int WalkHitIntervalTime;
        /// <summary>
        /// 跑位魔法间隔
        /// </summary>        
        public int RunMagicIntervalTime;
        /// <summary>
        /// 魔法攻击时间
        /// </summary>        
        public int MagicAttackTick;
        /// <summary>
        /// 魔法攻击间隔时间
        /// </summary>
        public int MagicAttackInterval;
        /// <summary>
        /// 人物跑动时间
        /// </summary>
        public int MoveTick;
        /// <summary>
        /// 人物攻击计数
        /// </summary>
        public int AttackCount;
        /// <summary>
        /// 人物攻击计数
        /// </summary>
        public int AttackCountA;
        /// <summary>
        /// 魔法攻击计数
        /// </summary>
        public int MagicAttackCount;
        /// <summary>
        /// 人物跑计数
        /// </summary>
        public int MoveCount;
        /// <summary>
        /// 超速计数
        /// </summary>
        public int OverSpeedCount;
        /// <summary>
        /// 复活戒指
        /// </summary>
        internal bool Revival = false;
        /// <summary>
        /// 传送戒指
        /// </summary>
        public bool Teleport = false;
        /// <summary>
        /// 麻痹戒指
        /// </summary>
        internal bool Paralysis = false;
        /// <summary>
        /// 火焰戒指
        /// </summary>
        private bool FlameRing = false;
        /// <summary>
        /// 治愈戒指
        /// </summary>
        private bool RecoveryRing;
        /// <summary>
        /// 未知戒指
        /// </summary>
        protected bool AngryRing = false;
        /// <summary>
        /// 护身戒指
        /// </summary>
        internal bool MagicShield = false;
        /// <summary>
        /// 防护身
        /// </summary>
        internal readonly bool UnMagicShield = false;
        /// <summary>
        /// 活力戒指
        /// </summary>
        private bool MuscleRing = false;
        /// <summary>
        /// 探测项链
        /// </summary>
        public bool ProbeNecklace = false;
        /// <summary>
        /// 防复活
        /// </summary>
        internal readonly bool UnRevival = false;
        /// <summary>
        /// 记忆全套
        /// </summary>
        public bool RecallSuite;
        /// <summary>
        /// 魔血一套
        /// </summary>
        protected int MoXieSuite;
        /// <summary>
        /// 虹魔一套
        /// </summary>
        internal int SuckupEnemyHealthRate;
        internal double SuckupEnemyHealth;
        public double BodyLuck;
        public int BodyLuckLevel;
        public bool DieInFight3Zone;
        public string GotoNpcLabel = string.Empty;
        public bool TakeDlgItem = false;
        public int DlgItemIndex = 0;
        public int DelayCall;
        public int DelayCallTick = 0;
        public bool IsDelayCall;
        public int DelayCallNpc;
        public string DelayCallLabel = string.Empty;
        public ScriptInfo MScript;
        public int LastNpc = 0;
        /// <summary>
        /// 职业属性点
        /// </summary>
        public NakedAbility BonusAbil;
        /// <summary>
        /// 玩家的变量P
        /// </summary>
        public int[] MNVal;
        /// <summary>
        /// 玩家的变量M
        /// </summary>
        public int[] MNMval;
        /// <summary>
        /// 玩家的变量D
        /// </summary>
        public int[] MDyVal;
        /// <summary>
        /// 玩家的变量
        /// </summary>
        public string[] MNSval;
        /// <summary>
        /// 人物变量  N
        /// </summary>
        public int[] MNInteger;
        /// <summary>
        /// 人物变量  S
        /// </summary>
        public string[] MSString;
        /// <summary>
        /// 服务器变量 W 0-20 个人服务器数值变量，不可保存，不可操作
        /// </summary>
        public string[] MServerStrVal;
        /// <summary>
        /// E 0-20 个人服务器字符串变量，不可保存，不可操作
        /// </summary>
        public int[] MServerIntVal;
        /// <summary>
        /// 技能表
        /// </summary>
        public readonly IList<UserMagic> MagicList;
        /// <summary>
        /// 组队长
        /// </summary>
        public int GroupOwner;
        /// <summary>
        /// 组成员
        /// </summary>
        public IList<PlayObject> GroupMembers;
        public string PlayDiceLabel = string.Empty;
        public bool IsTimeRecall;
        public int TimeRecallTick = 0;
        public string TimeRecallMoveMap = string.Empty;
        public short TimeRecallMoveX;
        public short TimeRecallMoveY;
        /// <summary>
        /// 减少勋章持久间隔
        /// </summary>
        protected int DecLightItemDrugTick;
        /// <summary>
        /// 保存人物数据时间间隔
        /// </summary>
        public int SaveRcdTick;
        public byte Bright;
        public bool IsNewHuman;
        private bool IsSendNotice;
        private int WaitLoginNoticeOkTick;
        public bool LoginNoticeOk;
        /// <summary>
        /// 试玩模式
        /// </summary>
        public bool TryPlayMode;
        public int ShowLineNoticeTick;
        public int ShowLineNoticeIdx;
        public int SoftVersionDateEx;
        /// <summary>
        /// 可点击脚本标签字典
        /// </summary>
        private readonly Hashtable CanJmpScriptLableMap;
        public int ScriptGotoCount = 0;
        /// <summary>
        /// 用于处理 @back 脚本命令
        /// </summary>
        public string ScriptCurrLable = string.Empty;
        /// <summary>
        /// 用于处理 @back 脚本命令
        /// </summary>        
        public string ScriptGoBackLable = string.Empty;
        /// <summary>
        /// 转身间隔
        /// </summary>
        public int TurnTick;
        public int OldIdent = 0;
        public byte MBtOldDir = 0;
        /// <summary>
        /// 第一个操作
        /// </summary>
        public bool IsFirstAction = false;
        /// <summary>
        /// 二次操作之间间隔时间
        /// </summary>        
        public int ActionTick;
        /// <summary>
        /// 配偶名称
        /// </summary>
        public string DearName;
        public PlayObject DearHuman;
        /// <summary>
        /// 是否允许夫妻传送
        /// </summary>
        public bool CanDearRecall;
        public bool CanMasterRecall;
        /// <summary>
        /// 夫妻传送时间
        /// </summary>
        public int DearRecallTick;
        public int MasterRecallTick;
        /// <summary>
        /// 师徒名称
        /// </summary>
        public string MasterName;
        public PlayObject MasterHuman;
        public IList<PlayObject> MasterList;
        public bool IsMaster = false;
        /// <summary>
        /// 对面玩家
        /// </summary>
        public int PoseBaseObject = 0;
        /// <summary>
        /// 声望点
        /// </summary>
        public byte CreditPoint = 0;
        /// <summary>
        /// 离婚次数
        /// </summary>        
        public byte MarryCount = 0;
        /// <summary>
        /// 转生等级
        /// </summary>
        public byte ReLevel = 0;
        public byte ReColorIdx;
        public int ReColorTick = 0;
        /// <summary>
        /// 杀怪经验倍数
        /// </summary>
        public int MNKillMonExpMultiple;
        /// <summary>
        /// 处理消息循环时间控制
        /// </summary>        
        public int GetMessageTick = 0;
        public bool IsSetStoragePwd;
        public bool IsReConfigPwd;
        public bool IsCheckOldPwd;
        public bool IsUnLockPwd;
        public bool IsUnLockStoragePwd;
        /// <summary>
        /// 锁密码
        /// </summary>
        public bool IsPasswordLocked;
        public byte PwdFailCount;
        /// <summary>
        /// 是否启用锁登录功能
        /// </summary>
        public bool IsLockLogon;
        /// <summary>
        /// 是否打开登录锁
        /// </summary>        
        public bool IsLockLogoned;
        public string MSTempPwd;
        public string StoragePwd;
        public bool IsStartMarry = false;
        public bool IsStartMaster = false;
        public bool IsStartUnMarry = false;
        public bool IsStartUnMaster = false;
        /// <summary>
        /// 禁止发方字(发的文字只能自己看到)
        /// </summary>
        public bool FilterSendMsg;
        /// <summary>
        /// 杀怪经验倍数(此数除以 100 为真正倍数)
        /// </summary>        
        public int KillMonExpRate;
        /// <summary>
        /// 人物攻击力倍数(此数除以 100 为真正倍数)
        /// </summary>        
        public int PowerRate;
        public int KillMonExpRateTime = 0;
        public int PowerRateTime = 0;
        public int ExpRateTick;
        /// <summary>
        /// 技巧项链
        /// </summary>
        private bool FastTrain = false;
        /// <summary>
        /// 是否允许使用物品
        /// </summary>
        public bool BoCanUseItem;
        /// <summary>
        /// 是否允许交易物品
        /// </summary>
        public bool IsCanDeal;
        public bool IsCanDrop;
        public bool IsCanGetBackItem;
        public bool IsCanWalk;
        public bool IsCanRun;
        public bool IsCanHit;
        public bool IsCanSpell;
        public bool IsCanSendMsg;
        /// <summary>
        /// 会员类型
        /// </summary>
        public int MemberType;
        /// <summary>
        /// 会员等级
        /// </summary> 
        public byte MemberLevel;
        /// <summary>
        /// 发祝福语标志
        /// </summary> 
        public bool BoSendMsgFlag;
        public bool BoChangeItemNameFlag;
        /// <summary>
        /// 游戏币
        /// </summary>
        public int GameGold;
        /// <summary>
        /// 是否自动减游戏币
        /// </summary>        
        public bool BoDecGameGold;
        public int DecGameGoldTime;
        public int DecGameGoldTick;
        public int DecGameGold;
        // 一次减点数
        public bool BoIncGameGold;
        // 是否自动加游戏币
        public int IncGameGoldTime;
        public int IncGameGoldTick;
        public int IncGameGold;
        // 一次减点数
        public int GamePoint;
        // 游戏点数
        public int IncGamePointTick;
        public int PayMentPoint;
        public int PayMentPointTick = 0;
        public int DecHpTick = 0;
        public int IncHpTick = 0;
        /// <summary>
        /// PK 死亡掉经验，不够经验就掉等级
        /// </summary>
        private readonly int PkDieLostExp;
        /// <summary>
        /// PK 死亡掉等级
        /// </summary>
        private readonly byte PkDieLostLevel;
        /// <summary>
        /// 私聊对象
        /// </summary>
        public PlayObject WhisperHuman;
        /// <summary>
        /// 清理无效对象间隔
        /// </summary>
        public int ClearInvalidObjTick = 0;
        public short Contribution;
        public string RankLevelName = string.Empty;
        public bool IsFilterAction = false;
        public int AutoGetExpTick;
        public int AutoGetExpTime = 0;
        public int AutoGetExpPoint;
        public Envirnoment AutoGetExpEnvir;
        public bool AutoGetExpInSafeZone = false;
        public readonly Dictionary<string, DynamicVar> DynamicVarMap;
        public short ClientTick;
        /// <summary>
        /// 进入速度测试模式
        /// </summary>
        public bool TestSpeedMode;
        public string RandomNo = string.Empty;
        /// <summary>
        /// 刷新包裹间隔
        /// </summary>
        public int QueryBagItemsTick = 0;
        public bool IsTimeGoto;
        public int TimeGotoTick;
        public string TimeGotoLable;
        public BaseObject TimeGotoNpc;
        /// <summary>
        /// 个人定时器
        /// </summary>
        public int[] AutoTimerTick;
        /// <summary>
        /// 个人定时器 时间间隔
        /// </summary>
        public int[] AutoTimerStatus;
        /// <summary>
        /// 0-攻击力增加 1-魔法增加  2-道术增加(无极真气) 3-攻击速度 4-HP增加(酒气护体)
        /// 5-增加MP上限 6-减攻击力 7-减魔法 8-减道术 9-减HP 10-减MP 11-敏捷 12-增加防
        /// 13-增加魔防 14-增加道术上限(虎骨酒) 15-连击伤害增加(醉八打) 16-内力恢复速度增加(何首养气酒)
        /// 17-内力瞬间恢复增加(何首凝神酒) 18-增加斗转上限(培元酒) 19-不死状态 21-弟子心法激活
        /// 22-移动减速 23-定身(十步一杀)
        /// </summary>
        internal ushort[] ExtraAbil;
        internal byte[] ExtraAbilFlag;
        /// <summary>
        /// 0-攻击力增加 1-魔法增加  2-道术增加(无极真气) 3-攻击速度 4-HP增加(酒气护体)
        /// 5-增加MP上限 6-减攻击力 7-减魔法 8-减道术 9-减HP 10-减MP 11-敏捷 12-增加防
        /// 13-增加魔防 14-增加道术上限(虎骨酒) 15-连击伤害增加(醉八打) 16-内力恢复速度增加(何首养气酒)
        /// 17-内力瞬间恢复增加(何首凝神酒) 18-增加斗转上限(培元酒) 19-不死状态 20-道术+上下限(除魔药剂类) 21-弟子心法激活
        /// 22-移动减速 23-定身(十步一杀)
        /// </summary>
        internal int[] ExtraAbilTimes;
        /// <summary>
        /// 点击NPC时间
        /// </summary>
        public int ClickNpcTime = 0;
        /// <summary>
        /// 是否开通元宝交易服务
        /// </summary>
        public bool SaleDeal;
        /// <summary>
        /// 确认元宝寄售标志
        /// </summary>
        public bool SellOffConfirm = false;
        /// <summary>
        /// 元宝寄售物品列表
        /// </summary>
        private IList<UserItem> SellOffItemList;
        public byte[] QuestUnitOpen;
        public byte[] QuestUnit;
        public byte[] QuestFlag;
        public MarketUser MarketUser;
        public bool FlagReadyToSellCheck = false;

        public PlayObject()
        {
            Race = ActorRace.Play;
            Hair = 0;
            Job = PlayJob.Warrior;
            HomeMap = "0";
            DealGolds = 0;
            DealItemList = new List<UserItem>();
            StorageItemList = new List<UserItem>();
            LockWhisperList = new List<string>();
            BoEmergencyClose = false;
            SwitchData = false;
            BoReconnection = false;
            BoKickFlag = false;
            BoSoftClose = false;
            BoReadyRun = false;
            SaveRcdTick = HUtil32.GetTickCount();
            DecHungerPointTick = HUtil32.GetTickCount();
            GroupRcallTick = HUtil32.GetTickCount();
            WantRefMsg = true;
            RcdSaved = false;
            DieInFight3Zone = false;
            DelayCall = 0;
            IsDelayCall = false;
            DelayCallNpc = 0;
            MScript = null;
            IsTimeRecall = false;
            TimeRecallMoveX = 0;
            TimeRecallMoveY = 0;
            RunTick = HUtil32.GetTickCount();
            RunTime = 250;
            SearchTime = 1000;
            SearchTick = HUtil32.GetTickCount();
            AllowGroup = false;
            AllowGuild = false;
            ViewRange = 12;
            InGuildWarArea = false;
            IsNewHuman = false;
            LoginNoticeOk = false;
            AttatckMode = 0;
            TryPlayMode = false;
            BonusAbil = new NakedAbility();
            AccountExpired = false;
            IsSendNotice = false;
            CheckDupObjTick = HUtil32.GetTickCount();
            DiscountForNightTick = HUtil32.GetTickCount();
            IsSafeArea = false;
            MagicAttackTick = HUtil32.GetTickCount();
            MagicAttackInterval = 0;
            AttackTick = HUtil32.GetTickCount();
            MoveTick = HUtil32.GetTickCount();
            TurnTick = HUtil32.GetTickCount();
            ActionTick = HUtil32.GetTickCount();
            AttackCount = 0;
            AttackCountA = 0;
            MagicAttackCount = 0;
            MoveCount = 0;
            OverSpeedCount = 0;
            SayMsgTick = HUtil32.GetTickCount();
            DisableSayMsg = false;
            DisableSayMsgTick = HUtil32.GetTickCount();
            LogonTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            LogonTick = HUtil32.GetTickCount();
            SwitchData = false;
            SwitchDataSended = false;
            WriteChgDataErrCount = 0;
            ShowLineNoticeTick = HUtil32.GetTickCount();
            ShowLineNoticeIdx = 0;
            SoftVersionDateEx = 0;
            CanJmpScriptLableMap = new Hashtable(StringComparer.OrdinalIgnoreCase);
            MagicList = new List<UserMagic>();
            MNKillMonExpMultiple = 1;
            KillMonExpRate = 100;
            ExpRateTick = HUtil32.GetTickCount();
            PowerRate = 100;
            IsSetStoragePwd = false;
            IsReConfigPwd = false;
            IsCheckOldPwd = false;
            IsUnLockPwd = false;
            IsUnLockStoragePwd = false;
            IsPasswordLocked = false;
            PvpFlag = false;
            // 锁仓库
            PwdFailCount = 0;
            FilterSendMsg = false;
            IsCanDeal = true;
            IsCanDrop = true;
            IsCanGetBackItem = true;
            IsCanWalk = true;
            IsCanRun = true;
            IsCanHit = true;
            IsCanSpell = true;
            BoCanUseItem = true;
            MemberType = 0;
            MemberLevel = 0;
            GameGold = 0;
            BoDecGameGold = false;
            DecGameGold = 1;
            DecGameGoldTick = HUtil32.GetTickCount();
            DecGameGoldTime = 60 * 1000;
            DecLightItemDrugTick = HUtil32.GetTickCount();
            BoIncGameGold = false;
            IncGameGold = 1;
            IncGameGoldTick = HUtil32.GetTickCount();
            IncGameGoldTime = 60 * 1000;
            GamePoint = 0;
            IncGamePointTick = HUtil32.GetTickCount();
            PayMentPoint = 0;
            DearHuman = null;
            MasterHuman = null;
            MasterList = new List<PlayObject>();
            BoSendMsgFlag = false;
            BoChangeItemNameFlag = false;
            CanMasterRecall = false;
            CanDearRecall = false;
            DearRecallTick = HUtil32.GetTickCount();
            MasterRecallTick = HUtil32.GetTickCount();
            ReColorIdx = 0;
            WhisperHuman = null;
            OnHorse = false;
            Contribution = 0;
            HitPlus = 0;
            HitDouble = 0;
            RankLevelName = Settings.RankLevelName;
            FixedHideMode = true;
            MNVal = new int[100];
            MNMval = new int[100];
            MDyVal = new int[100];
            MNSval = new string[100];
            MNInteger = new int[100];
            MSString = new string[100];
            MServerStrVal = new string[100];
            MServerIntVal = new int[100];
            ExtraAbil = new ushort[7];
            ExtraAbilTimes = new int[7];
            ExtraAbilFlag = new byte[7];
            HearWhisper = true;
            BanShout = true;
            BanGuildChat = true;
            AllowDeal = true;
            AutoGetExpTick = HUtil32.GetTickCount();
            DecPkPointTick = HUtil32.GetTickCount();
            AutoGetExpPoint = 0;
            AutoGetExpEnvir = null;
            HitIntervalTime = M2Share.Config.HitIntervalTime;// 攻击间隔
            MagicHitIntervalTime = M2Share.Config.MagicHitIntervalTime;// 魔法间隔
            RunIntervalTime = M2Share.Config.RunIntervalTime;// 走路间隔
            WalkIntervalTime = M2Share.Config.WalkIntervalTime;// 走路间隔
            TurnIntervalTime = M2Share.Config.TurnIntervalTime;// 换方向间隔
            ActionIntervalTime = M2Share.Config.ActionIntervalTime;// 组合操作间隔
            RunLongHitIntervalTime = M2Share.Config.RunLongHitIntervalTime;// 组合操作间隔
            RunHitIntervalTime = M2Share.Config.RunHitIntervalTime;// 组合操作间隔
            WalkHitIntervalTime = M2Share.Config.WalkHitIntervalTime;// 组合操作间隔
            RunMagicIntervalTime = M2Share.Config.RunMagicIntervalTime;// 跑位魔法间隔
            DynamicVarMap = new Dictionary<string, DynamicVar>(StringComparer.OrdinalIgnoreCase);
            SessInfo = null;
            TestSpeedMode = false;
            IsLockLogon = true;
            IsLockLogoned = false;
            IsTimeGoto = false;
            TimeGotoTick = HUtil32.GetTickCount();
            TimeGotoLable = "";
            TimeGotoNpc = null;
            AutoTimerTick = new int[20];
            AutoTimerStatus = new int[20];
            CellType = CellType.Play;
            QueryExpireTick = 60 * 1000;
            AccountExpiredTick = HUtil32.GetTickCount();
            GoldMax = M2Share.Config.HumanMaxGold;
            QuestUnitOpen = new byte[128];
            QuestUnit = new byte[128];
            QuestFlag = new byte[128];
            MagicArr = new UserMagic[50];
            SlaveList = new List<BaseObject>();
            GroupMembers = new List<PlayObject>();
            VisibleEvents = new List<MapEvent>();
            VisibleItems = new List<VisibleMapItem>();
            ItemList = new List<UserItem>(Grobal2.MaxBagItem);
            MapMoveTick = HUtil32.GetTickCount();
            RandomNo = M2Share.RandomNumber.Random(999999).ToString();
        }

        public override void Initialize()
        {
            base.Initialize();
            for (int i = 0; i < MagicList.Count; i++)
            {
                if (MagicList[i].Level >= 4)
                {
                    MagicList[i].Level = 0;
                }
            }
            AddBodyLuck(0);
        }

        private void SendNotice()
        {
            //todo 优化
            List<string> loadList = new List<string>();
            M2Share.NoticeMgr.GetNoticeMsg("Notice", loadList);
            string sNoticeMsg = string.Empty;
            if (loadList.Count > 0)
            {
                for (int i = 0; i < loadList.Count; i++)
                {
                    sNoticeMsg = sNoticeMsg + loadList[i] + "\x20\x1B";
                }
            }
            SendDefMessage(Messages.SM_SENDNOTICE, 2000, 0, 0, 0, sNoticeMsg.Replace("/r/n/r/n ", ""));
        }

        public void RunNotice()
        {
            const string sExceptionMsg = "[Exception] PlayObject::RunNotice";
            if (BoEmergencyClose || BoKickFlag || BoSoftClose)
            {
                if (BoKickFlag)
                {
                    SendDefMessage(Messages.SM_OUTOFCONNECTION, 0, 0, 0, 0);
                }
                MakeGhost();
            }
            else
            {
                try
                {
                    if (!IsSendNotice)
                    {
                        SendNotice();
                        IsSendNotice = true;
                        WaitLoginNoticeOkTick = HUtil32.GetTickCount();
                    }
                    else
                    {
                        if ((HUtil32.GetTickCount() - WaitLoginNoticeOkTick) > 10 * 1000)
                        {
                            BoEmergencyClose = true;
                        }
                        ProcessMessage msg = default;
                        while (GetMessage(ref msg))
                        {
                            if (msg.wIdent == Messages.CM_LOGINNOTICEOK)
                            {
                                LoginNoticeOk = true;
                                ClientTick = (short)msg.nParam1;
                                SysMsg(ClientTick.ToString(), MsgColor.Red, MsgType.Notice);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    M2Share.Logger.Error(sExceptionMsg);
                }
            }
        }

        /// <summary>
        /// 发送登录消息
        /// </summary>
        private void SendLogon()
        {
            MessageBodyWL messageBodyWl = default;
            ClientMsg = Messages.MakeMessage(Messages.SM_LOGON, ActorId, CurrX, CurrY, HUtil32.MakeWord(Dir, Light));
            messageBodyWl.Param1 = GetFeatureToLong();
            messageBodyWl.Param2 = CharStatus;
            if (AllowGroup)
            {
                messageBodyWl.Tag1 = HUtil32.MakeLong(HUtil32.MakeWord(1, 0), GetFeatureEx());
            }
            else
            {
                messageBodyWl.Tag1 = 0;
            }
            messageBodyWl.Tag2 = 0;
            SendSocket(ClientMsg, EDCode.EncodePacket(messageBodyWl));
            int nRecog = GetFeatureToLong();
            SendDefMessage(Messages.SM_FEATURECHANGED, ActorId, HUtil32.LoWord(nRecog), HUtil32.HiWord(nRecog), GetFeatureEx());
            SendDefMessage(Messages.SM_ATTACKMODE, (byte)AttatckMode, 0, 0, 0);
        }

        /// <summary>
        /// 玩家登录
        /// </summary>
        public void UserLogon()
        {
            string sIPaddr = "127.0.0.1";
            const string sExceptionMsg = "[Exception] PlayObject::UserLogon";
            const string sCheckIPaddrFail = "登录IP地址不匹配!!!";
            try
            {
                if (M2Share.Config.TestServer)
                {
                    if (Abil.Level < M2Share.Config.TestLevel)
                    {
                        Abil.Level = (byte)M2Share.Config.TestLevel;
                    }
                    if (Gold < M2Share.Config.TestGold)
                    {
                        Gold = M2Share.Config.TestGold;
                    }
                }
                if (M2Share.Config.TestServer || M2Share.Config.ServiceMode)
                {
                    PayMent = 3;
                }
                MapMoveTick = HUtil32.GetTickCount();
                LogonTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                LogonTick = HUtil32.GetTickCount();
                Initialize();
                SendPriorityMsg(Messages.RM_LOGON, 0, 0, 0, 0, "", MessagePriority.High);
                if (Abil.Level <= 7)
                {
                    if (GetRangeHumanCount() >= 80)
                    {
                        MapRandomMove(Envir.MapName, 0);
                    }
                }
                if (DieInFight3Zone)
                {
                    MapRandomMove(Envir.MapName, 0);
                }
                if (M2Share.WorldEngine.GetHumPermission(ChrName, ref sIPaddr, ref Permission))
                {
                    if (M2Share.Config.PermissionSystem)
                    {
                        if (!M2Share.CompareIPaddr(LoginIpAddr, sIPaddr))
                        {
                            SysMsg(sCheckIPaddrFail, MsgColor.Red, MsgType.Hint);
                            BoEmergencyClose = true;
                        }
                    }
                }
                GetStartPoint();
                for (int i = MagicList.Count - 1; i >= 0; i--)
                {
                    CheckSeeHealGauge(MagicList[i]);
                }
                UserItem userItem;
                if (IsNewHuman)
                {
                    userItem = new UserItem();
                    if (M2Share.WorldEngine.CopyToUserItemFromName(M2Share.Config.Candle, ref userItem))
                    {
                        ItemList.Add(userItem);
                    }
                    else
                    {
                        Dispose(userItem);
                    }
                    userItem = new UserItem();
                    if (M2Share.WorldEngine.CopyToUserItemFromName(M2Share.Config.BasicDrug, ref userItem))
                    {
                        ItemList.Add(userItem);
                    }
                    else
                    {
                        Dispose(userItem);
                    }
                    userItem = new UserItem();
                    if (M2Share.WorldEngine.CopyToUserItemFromName(M2Share.Config.WoodenSword, ref userItem))
                    {
                        ItemList.Add(userItem);
                    }
                    else
                    {
                        Dispose(userItem);
                    }
                    userItem = new UserItem();
                    string sItem = Gender == PlayGender.Man
                        ? M2Share.Config.ClothsMan
                        : M2Share.Config.ClothsWoman;
                    if (M2Share.WorldEngine.CopyToUserItemFromName(sItem, ref userItem))
                    {
                        ItemList.Add(userItem);
                    }
                    else
                    {
                        Dispose(userItem);
                    }
                }
                // 检查背包中的物品是否合法
                for (int i = ItemList.Count - 1; i >= 0; i--)
                {
                    if (!string.IsNullOrEmpty(M2Share.WorldEngine.GetStdItemName(ItemList[i].Index))) continue;
                    Dispose(ItemList[i]);
                    ItemList.RemoveAt(i);
                }
                // 检查人物身上的物品是否符合使用规则
                if (M2Share.Config.CheckUserItemPlace)
                {
                    for (int i = 0; i < UseItems.Length; i++)
                    {
                        if (UseItems[i] == null || UseItems[i].Index <= 0) continue;
                        StdItem stdItem = M2Share.WorldEngine.GetStdItem(UseItems[i].Index);
                        if (stdItem != null)
                        {
                            if (!M2Share.CheckUserItems(i, stdItem))
                            {
                                if (!AddItemToBag(UseItems[i]))
                                {
                                    ItemList.Insert(0, UseItems[i]);
                                }
                                UseItems[i].Index = 0;
                            }
                        }
                        else
                        {
                            UseItems[i].Index = 0;
                        }
                    }
                }
                // 检查背包中是否有复制品
                for (int i = ItemList.Count - 1; i >= 0; i--)
                {
                    string sItemName = M2Share.WorldEngine.GetStdItemName(ItemList[i].Index);
                    for (int j = i - 1; j >= 0; j--)
                    {
                        UserItem userItem1 = ItemList[j];
                        if (M2Share.WorldEngine.GetStdItemName(userItem1.Index) == sItemName && ItemList[i].MakeIndex == userItem1.MakeIndex)
                        {
                            ItemList.RemoveAt(j);
                            break;
                        }
                    }
                }
                for (int i = 0; i < StatusArrTick.Length; i++)
                {
                    if (StatusTimeArr[i] > 0)
                    {
                        StatusArrTick[i] = HUtil32.GetTickCount();
                    }
                }
                CharStatus = GetCharStatus();
                RecalcLevelAbilitys();
                RecalcAbilitys();
                Abil.MaxExp = GetLevelExp(Abil.Level);// 登录重新取得升级所需经验值
                WAbil.Exp = Abil.Exp;
                WAbil.MaxExp = Abil.MaxExp;
                if (BtB2 == 0)
                {
                    PkPoint = 0;
                    BtB2++;
                }
                if (Gold > M2Share.Config.HumanMaxGold * 2 && M2Share.Config.HumanMaxGold > 0)
                {
                    Gold = M2Share.Config.HumanMaxGold * 2;
                }
                if (!TryPlayMode)
                {
                    if (SoftVersionDate < M2Share.Config.SoftVersionDate)//登录版本号验证
                    {
                        SysMsg(Settings.ClientSoftVersionError, MsgColor.Red, MsgType.Hint);
                        SysMsg(Settings.DownLoadNewClientSoft, MsgColor.Red, MsgType.Hint);
                        SysMsg(Settings.ForceDisConnect, MsgColor.Red, MsgType.Hint);
                        BoEmergencyClose = true;
                        return;
                    }
                    if (SoftVersionDateEx == 0 && M2Share.Config.boOldClientShowHiLevel)
                    {
                        SysMsg(Settings.ClientSoftVersionTooOld, MsgColor.Blue, MsgType.Hint);
                        SysMsg(Settings.DownLoadAndUseNewClient, MsgColor.Red, MsgType.Hint);
                        if (!M2Share.Config.CanOldClientLogon)
                        {
                            SysMsg(Settings.ClientSoftVersionError, MsgColor.Red, MsgType.Hint);
                            SysMsg(Settings.DownLoadNewClientSoft, MsgColor.Red, MsgType.Hint);
                            SysMsg(Settings.ForceDisConnect, MsgColor.Red, MsgType.Hint);
                            BoEmergencyClose = true;
                            return;
                        }
                    }
                    switch (AttatckMode)
                    {
                        case AttackMode.HAM_ALL:// [攻击模式: 全体攻击]
                            SysMsg(Settings.AttackModeOfAll, MsgColor.Green, MsgType.Hint);
                            break;
                        case AttackMode.HAM_PEACE:// [攻击模式: 和平攻击]
                            SysMsg(Settings.AttackModeOfPeaceful, MsgColor.Green, MsgType.Hint);
                            break;
                        case AttackMode.HAM_DEAR:// [攻击模式: 和平攻击]
                            SysMsg(Settings.AttackModeOfDear, MsgColor.Green, MsgType.Hint);
                            break;
                        case AttackMode.HAM_MASTER:// [攻击模式: 和平攻击]
                            SysMsg(Settings.AttackModeOfMaster, MsgColor.Green, MsgType.Hint);
                            break;
                        case AttackMode.HAM_GROUP:// [攻击模式: 编组攻击]
                            SysMsg(Settings.AttackModeOfGroup, MsgColor.Green, MsgType.Hint);
                            break;
                        case AttackMode.HAM_GUILD:// [攻击模式: 行会攻击]
                            SysMsg(Settings.AttackModeOfGuild, MsgColor.Green, MsgType.Hint);
                            break;
                        case AttackMode.HAM_PKATTACK:// [攻击模式: 红名攻击]
                            SysMsg(Settings.AttackModeOfRedWhite, MsgColor.Green, MsgType.Hint);
                            break;
                    }
                    SysMsg(Settings.StartChangeAttackModeHelp, MsgColor.Green, MsgType.Hint);// 使用组合快捷键 CTRL-H 更改攻击...
                    if (M2Share.Config.TestServer)
                    {
                        SysMsg(Settings.StartNoticeMsg, MsgColor.Green, MsgType.Hint);// 欢迎进入本服务器进行游戏...
                    }
                    if (M2Share.WorldEngine.PlayObjectCount > M2Share.Config.TestUserLimit)
                    {
                        if (Permission < 2)
                        {
                            SysMsg(Settings.OnlineUserFull, MsgColor.Red, MsgType.Hint);
                            SysMsg(Settings.ForceDisConnect, MsgColor.Red, MsgType.Hint);
                            BoEmergencyClose = true;
                        }
                    }
                }
                Bright = M2Share.GameTime;
                SendPriorityMsg(Messages.RM_ABILITY, 0, 0, 0, 0, "", MessagePriority.High);
                SendPriorityMsg(Messages.RM_SUBABILITY, 0, 0, 0, 0, "", MessagePriority.High);
                SendPriorityMsg(Messages.RM_ADJUST_BONUS, 0, 0, 0, 0);
                SendPriorityMsg(Messages.RM_DAYCHANGING, 0, 0, 0, 0);
                SendPriorityMsg(Messages.RM_SENDUSEITEMS, 0, 0, 0, 0, "", MessagePriority.High);
                SendPriorityMsg(Messages.RM_SENDMYMAGIC, 0, 0, 0, 0, "", MessagePriority.High);
                MyGuild = M2Share.GuildMgr.MemberOfGuild(ChrName);
                if (MyGuild != null)
                {
                    GuildRankName = MyGuild.GetRankName(this, ref GuildRankNo);
                    for (int i = MyGuild.GuildWarList.Count - 1; i >= 0; i--)
                    {
                        SysMsg(MyGuild.GuildWarList[i] + " 正在与本行会进行行会战.", MsgColor.Green, MsgType.Hint);
                    }
                }
                RefShowName();
                if (PayMent == 1)
                {
                    if (!TryPlayMode)
                    {
                        SysMsg(Settings.YouNowIsTryPlayMode, MsgColor.Red, MsgType.Hint);
                    }
                    GoldMax = M2Share.Config.HumanTryModeMaxGold;
                    if (Abil.Level > M2Share.Config.TryModeLevel)
                    {
                        SysMsg("测试状态可以使用到第 " + M2Share.Config.TryModeLevel, MsgColor.Red, MsgType.Hint);
                        SysMsg("链接中断，请到以下地址获得收费相关信息。(https://www.mir2.com)", MsgColor.Red, MsgType.Hint);
                        BoEmergencyClose = true;
                    }
                }
                if (PayMent == 3 && !TryPlayMode)
                {
                    SysMsg(Settings.NowIsFreePlayMode, MsgColor.Green, MsgType.Hint);
                }
                if (M2Share.Config.VentureServer)
                {
                    SysMsg("当前服务器运行于冒险模式.", MsgColor.Green, MsgType.Hint);
                }
                if (MagicArr[MagicConst.SKILL_ERGUM] != null && !UseThrusting)
                {
                    UseThrusting = true;
                    SendSocket("+LNG");
                }
                if (Envir.Flag.boNORECONNECT)
                {
                    MapRandomMove(Envir.Flag.sNoReConnectMap, 0);
                }
                if (CheckDenyLogon())// 如果人物在禁止登录列表里则直接掉线而不执行下面内容
                {
                    return;
                }
                if (M2Share.ManageNPC != null)
                {
                    M2Share.ManageNPC.GotoLable(this, "@Login", false);
                }
                FixedHideMode = false;
                if (!string.IsNullOrEmpty(DearName))
                {
                    CheckMarry();
                }
                CheckMaster();
                FilterSendMsg = M2Share.GetDisableSendMsgList(ChrName);
                // 密码保护系统
                if (M2Share.Config.PasswordLockSystem)
                {
                    if (IsPasswordLocked)
                    {
                        IsCanGetBackItem = !M2Share.Config.LockGetBackItemAction;
                    }
                    if (M2Share.Config.LockHumanLogin && IsLockLogon && IsPasswordLocked)
                    {
                        IsCanDeal = !M2Share.Config.LockDealAction;
                        IsCanDrop = !M2Share.Config.LockDropAction;
                        BoCanUseItem = !M2Share.Config.LockUserItemAction;
                        IsCanWalk = !M2Share.Config.LockWalkAction;
                        IsCanRun = !M2Share.Config.LockRunAction;
                        IsCanHit = !M2Share.Config.LockHitAction;
                        IsCanSpell = !M2Share.Config.LockSpellAction;
                        IsCanSendMsg = !M2Share.Config.LockSendMsgAction;
                        ObMode = M2Share.Config.LockInObModeAction;
                        AdminMode = M2Share.Config.LockInObModeAction;
                        SysMsg(Settings.ActionIsLockedMsg + " 开锁命令: @" + CommandMgr.GameCommands.LockLogon.CmdName, MsgColor.Red, MsgType.Hint);
                        SendMsg(M2Share.ManageNPC, Messages.RM_MENU_OK, 0, ActorId, 0, 0, Settings.ActionIsLockedMsg + "\\ \\" + "密码命令: @" + CommandMgr.GameCommands.PasswordLock.CmdName);
                    }
                    if (!IsPasswordLocked)
                    {
                        SysMsg(Format(Settings.PasswordNotSetMsg, CommandMgr.GameCommands.PasswordLock.CmdName), MsgColor.Red, MsgType.Hint);
                    }
                    if (!IsLockLogon && IsPasswordLocked)
                    {
                        SysMsg(Format(Settings.NotPasswordProtectMode, CommandMgr.GameCommands.LockLogon.CmdName), MsgColor.Red, MsgType.Hint);
                    }
                    SysMsg(Settings.ActionIsLockedMsg + " 开锁命令: @" + CommandMgr.GameCommands.Unlock.CmdName, MsgColor.Red, MsgType.Hint);
                    SendMsg(M2Share.ManageNPC, Messages.RM_MENU_OK, 0, ActorId, 0, 0, Settings.ActionIsLockedMsg + "\\ \\" + "开锁命令: @" + CommandMgr.GameCommands.Unlock.CmdName + '\\' + "加锁命令: @" + CommandMgr.GameCommands.Lock.CmdName + '\\' + "设置密码命令: @" + CommandMgr.GameCommands.SetPassword.CmdName + '\\' + "修改密码命令: @" + CommandMgr.GameCommands.ChgPassword.CmdName);
                }
                // 重置泡点方面计时
                IncGamePointTick = HUtil32.GetTickCount();
                IncGameGoldTick = HUtil32.GetTickCount();
                AutoGetExpTick = HUtil32.GetTickCount();
                GetSellOffGlod();// 检查是否有元宝寄售交易结束还没得到元宝
            }
            catch (Exception e)
            {
                M2Share.Logger.Error(sExceptionMsg);
                M2Share.Logger.Error(e.StackTrace);
            }
            // ReadAllBook();
        }

        /// <summary>
        /// 使用祝福油
        /// </summary>
        /// <returns></returns>
        private bool WeaptonMakeLuck()
        {
            if (UseItems[ItemLocation.Weapon] == null || UseItems[ItemLocation.Weapon].Index <= 0)
            {
                return false;
            }
            int nRand = 0;
            StdItem stdItem = M2Share.WorldEngine.GetStdItem(UseItems[ItemLocation.Weapon].Index);
            if (stdItem != null)
            {
                nRand = Math.Abs(HUtil32.HiByte(stdItem.DC) - HUtil32.LoByte(stdItem.DC)) / 5;
            }
            if (M2Share.RandomNumber.Random(M2Share.Config.WeaponMakeUnLuckRate) == 1)
            {
                MakeWeaponUnlock();
            }
            else
            {
                bool boMakeLuck = false;
                if (UseItems[ItemLocation.Weapon].Desc[4] > 0)
                {
                    UseItems[ItemLocation.Weapon].Desc[4] -= 1;
                    SysMsg(Settings.WeaptonMakeLuck, MsgColor.Green, MsgType.Hint);
                    boMakeLuck = true;
                }
                else if (UseItems[ItemLocation.Weapon].Desc[3] < M2Share.Config.WeaponMakeLuckPoint1)
                {
                    UseItems[ItemLocation.Weapon].Desc[3]++;
                    SysMsg(Settings.WeaptonMakeLuck, MsgColor.Green, MsgType.Hint);
                    boMakeLuck = true;
                }
                else if (UseItems[ItemLocation.Weapon].Desc[3] < M2Share.Config.WeaponMakeLuckPoint2 && M2Share.RandomNumber.Random(nRand + M2Share.Config.WeaponMakeLuckPoint2Rate) == 1)
                {
                    UseItems[ItemLocation.Weapon].Desc[3]++;
                    SysMsg(Settings.WeaptonMakeLuck, MsgColor.Green, MsgType.Hint);
                    boMakeLuck = true;
                }
                else if (UseItems[ItemLocation.Weapon].Desc[3] < M2Share.Config.WeaponMakeLuckPoint3 && M2Share.RandomNumber.Random(nRand * M2Share.Config.WeaponMakeLuckPoint3Rate) == 1)
                {
                    UseItems[ItemLocation.Weapon].Desc[3]++;
                    SysMsg(Settings.WeaptonMakeLuck, MsgColor.Green, MsgType.Hint);
                    boMakeLuck = true;
                }
                if (Race == ActorRace.Play)
                {
                    RecalcAbilitys();
                    SendMsg(Messages.RM_ABILITY, 0, 0, 0, 0);
                    SendMsg(Messages.RM_SUBABILITY, 0, 0, 0, 0);
                }
                if (!boMakeLuck)
                {
                    SysMsg(Settings.WeaptonNotMakeLuck, MsgColor.Green, MsgType.Hint);
                }
            }
            return true;
        }

        /// <summary>
        /// 修复武器
        /// </summary>
        /// <returns></returns>
        private bool RepairWeapon()
        {
            if (UseItems[ItemLocation.Weapon] == null)
            {
                return false;
            }
            UserItem userItem = UseItems[ItemLocation.Weapon];
            if (userItem.Index <= 0 || userItem.DuraMax <= userItem.Dura)
            {
                return false;
            }
            userItem.DuraMax -= (ushort)((userItem.DuraMax - userItem.Dura) / M2Share.Config.RepairItemDecDura);
            ushort nDura = (ushort)HUtil32._MIN(5000, userItem.DuraMax - userItem.Dura);
            if (nDura <= 0) return false;
            userItem.Dura += nDura;
            SendMsg(Messages.RM_DURACHANGE, 1, userItem.Dura, userItem.DuraMax, 0);
            SysMsg(Settings.WeaponRepairSuccess, MsgColor.Green, MsgType.Hint);
            return true;
        }

        /// <summary>
        /// 特修武器
        /// </summary>
        /// <returns></returns>
        private bool SuperRepairWeapon()
        {
            if (UseItems[ItemLocation.Weapon] == null || UseItems[ItemLocation.Weapon].Index <= 0)
            {
                return false;
            }
            UseItems[ItemLocation.Weapon].Dura = UseItems[ItemLocation.Weapon].DuraMax;
            SendMsg(Messages.RM_DURACHANGE, 1, UseItems[ItemLocation.Weapon].Dura, UseItems[ItemLocation.Weapon].DuraMax, 0);
            SysMsg(Settings.WeaponRepairSuccess, MsgColor.Green, MsgType.Hint);
            return true;
        }

        private void MakeWeaponUnlock()
        {
            if (UseItems[ItemLocation.Weapon] == null)
            {
                return;
            }
            if (UseItems[ItemLocation.Weapon].Index <= 0)
            {
                return;
            }
            if (UseItems[ItemLocation.Weapon].Desc[3] > 0)
            {
                UseItems[ItemLocation.Weapon].Desc[3] -= 1;
                SysMsg(Settings.TheWeaponIsCursed, MsgColor.Red, MsgType.Hint);
            }
            else
            {
                if (UseItems[ItemLocation.Weapon].Desc[4] < 10)
                {
                    UseItems[ItemLocation.Weapon].Desc[4]++;
                    SysMsg(Settings.TheWeaponIsCursed, MsgColor.Red, MsgType.Hint);
                }
            }
            RecalcAbilitys();
            SendMsg(Messages.RM_ABILITY, 0, 0, 0, 0);
            SendMsg(Messages.RM_SUBABILITY, 0, 0, 0, 0);
        }

        /// <summary>
        /// 角色杀死目标触发
        /// </summary>
        private void KillTargetTrigger(int actorId)
        {
            var killObject = M2Share.ActorMgr.Get(actorId);
            if (killObject == null)
            {
                return;
            }
            if (M2Share.FunctionNPC != null)
            {
                M2Share.FunctionNPC.GotoLable(this, "@PlayKillMob", false);
            }
            int monsterExp = CalcGetExp(WAbil.Level, killObject.FightExp);
            if (!M2Share.Config.VentureServer)
            {
                if (IsRobot && ExpHitter != null && ExpHitter.Race == ActorRace.Play)
                {
                    ((RobotPlayer)ExpHitter).GainExp(monsterExp);
                }
                else
                {
                    GainExp(monsterExp);
                }
            }
            // 是否执行任务脚本
            if (Envir.IsCheapStuff())// 地图是否有任务脚本
            {
                Merchant QuestNPC;
                if (GroupOwner != 0)
                {
                    PlayObject groupOwnerPlay = (PlayObject)M2Share.ActorMgr.Get(GroupOwner);
                    for (int i = 0; i < groupOwnerPlay.GroupMembers.Count; i++)
                    {
                        PlayObject groupHuman = groupOwnerPlay.GroupMembers[i];
                        bool tCheck;
                        if (!groupHuman.Death && Envir == groupHuman.Envir && Math.Abs(CurrX - groupHuman.CurrX) <= 12 && Math.Abs(CurrX - groupHuman.CurrX) <= 12 && this == groupHuman)
                        {
                            tCheck = false;
                        }
                        else
                        {
                            tCheck = true;
                        }
                        QuestNPC = Envir.GetQuestNpc(groupHuman, ChrName, "", tCheck);
                        if (QuestNPC != null)
                        {
                            QuestNPC.Click(groupHuman);
                        }
                    }
                }
                QuestNPC = Envir.GetQuestNpc(this, ChrName, "", false);
                if (QuestNPC != null)
                {
                    QuestNPC.Click(this);
                }
            }
            try
            {
                bool boPK = false;
                if (!M2Share.Config.VentureServer && !Envir.Flag.FightZone && !Envir.Flag.Fight3Zone)
                {
                    if (PvpLevel() < 2)
                    {
                        if ((killObject.Race == ActorRace.Play) || (killObject.Race == ActorRace.NPC))//允许NPC杀死人物
                        {
                            boPK = true;
                        }
                        if (killObject.Master != null && killObject.Master.Race == ActorRace.Play)
                        {
                            killObject = killObject.Master;
                            boPK = true;
                        }
                    }
                }
                if (boPK && Race == ActorRace.Play && killObject.Race == ActorRace.Play)
                {
                    bool guildwarkill = false;
                    PlayObject targetObject = ((PlayObject)killObject);
                    if (MyGuild != null && targetObject.MyGuild != null)
                    {
                        if (GetGuildRelation(this, targetObject) == 2)
                        {
                            guildwarkill = true;
                        }
                    }
                    else
                    {
                        UserCastle Castle = M2Share.CastleMgr.InCastleWarArea(this);
                        if ((Castle != null && Castle.UnderWar) || (InGuildWarArea))
                        {
                            guildwarkill = true;
                        }
                    }
                    if (!guildwarkill)
                    {
                        if ((M2Share.Config.IsKillHumanWinLevel || M2Share.Config.IsKillHumanWinExp || Envir.Flag.boPKWINLEVEL || Envir.Flag.boPKWINEXP))
                        {
                            PvpDie(targetObject);
                        }
                        else
                        {
                            if (!IsGoodKilling(this))
                            {
                                targetObject.IncPkPoint(M2Share.Config.KillHumanAddPKPoint);
                                targetObject.SysMsg(Settings.YouMurderedMsg, MsgColor.Red, MsgType.Hint);
                                SysMsg(Format(Settings.YouKilledByMsg, targetObject.ChrName), MsgColor.Red, MsgType.Hint);
                                targetObject.AddBodyLuck(-M2Share.Config.KillHumanDecLuckPoint);
                                if (PvpLevel() < 1)
                                {
                                    if (M2Share.RandomNumber.Random(5) == 0)
                                    {
                                        targetObject.MakeWeaponUnlock();
                                    }
                                }
                            }
                            else
                            {
                                targetObject.SysMsg(Settings.YouprotectedByLawOfDefense, MsgColor.Green, MsgType.Hint);
                            }
                        }
                        if (killObject.Race == ActorRace.Play)// 检查攻击人是否用了着经验或等级装备
                        {
                            if (targetObject.PkDieLostExp > 0)
                            {
                                if (Abil.Exp >= targetObject.PkDieLostExp)
                                {
                                    Abil.Exp -= targetObject.PkDieLostExp;
                                }
                                else
                                {
                                    Abil.Exp = 0;
                                }
                            }
                            if (targetObject.PkDieLostLevel > 0)
                            {
                                if (Abil.Level >= targetObject.PkDieLostLevel)
                                {
                                    Abil.Level -= targetObject.PkDieLostLevel;
                                }
                                else
                                {
                                    Abil.Level = 0;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                M2Share.Logger.Error(ex);
            }
            if (!Envir.Flag.FightZone && !Envir.Flag.Fight3Zone && killObject.Race == ActorRace.Play)
            {
                BaseObject AttackBaseObject = killObject;
                if (killObject.Master != null)
                {
                    AttackBaseObject = killObject.Master;
                }
                if (!NoItem || !Envir.Flag.NoDropItem)//允许设置 m_boNoItem 后人物死亡不掉物品
                {
                    if (AttackBaseObject != null)
                    {
                        if (M2Share.Config.KillByHumanDropUseItem && AttackBaseObject.Race == ActorRace.Play || M2Share.Config.KillByMonstDropUseItem && AttackBaseObject.Race != ActorRace.Play)
                        {
                            killObject.DropUseItems(0);
                        }
                    }
                    else
                    {
                        killObject.DropUseItems(0);
                    }
                    if (M2Share.Config.DieScatterBag)
                    {
                        killObject.ScatterBagItems(0);
                    }
                    if (M2Share.Config.DieDropGold)
                    {
                        killObject.ScatterGolds(0);
                    }
                }
                AddBodyLuck(-(50 - (50 - WAbil.Level * 5)));
            }
            if ((M2Share.FunctionNPC != null) && (Envir != null) && Envir.Flag.boKILLFUNC)
            {
                if (killObject.Race != ActorRace.Play) //怪杀死玩家
                {
                    if (ExpHitter != null)
                    {
                        if (ExpHitter.Race == ActorRace.Play)
                        {
                            M2Share.FunctionNPC.GotoLable(ExpHitter as PlayObject, "@KillPlayMon" + Envir.Flag.nKILLFUNCNO, false);
                        }
                        if (ExpHitter.Master != null)
                        {
                            M2Share.FunctionNPC.GotoLable(ExpHitter.Master as PlayObject, "@KillPlayMon" + Envir.Flag.nKILLFUNCNO, false);
                        }
                    }
                    else
                    {
                        if (LastHiter != null)
                        {
                            if (LastHiter.Race == ActorRace.Play)
                            {
                                M2Share.FunctionNPC.GotoLable(LastHiter as PlayObject, "@KillPlayMon" + Envir.Flag.nKILLFUNCNO, false);
                            }
                            if (LastHiter.Master != null)
                            {
                                M2Share.FunctionNPC.GotoLable(LastHiter.Master as PlayObject, "@KillPlayMon" + Envir.Flag.nKILLFUNCNO, false);
                            }
                        }
                    }
                }
                else
                {
                    if ((LastHiter != null) && (LastHiter.Race == ActorRace.Play))
                    {
                        M2Share.FunctionNPC.GotoLable(LastHiter as PlayObject, "@KillPlay" + Envir.Flag.nKILLFUNCNO, false);
                    }
                }
            }
        }

        public override bool IsProperTarget(BaseObject baseObject)
        {
            if (baseObject == null || baseObject.ActorId == this.ActorId)
            {
                return false;
            }
            var result = IsAttackTarget(baseObject);//先检查攻击模式
            if (result)
            {
                if (baseObject.Race == ActorRace.Play)
                {
                    return IsProtectTarget(baseObject);
                }
                if ((baseObject.Master != null) && (baseObject.Race != ActorRace.Play))
                {
                    if (baseObject.Master == this)
                    {
                        if (AttatckMode != AttackMode.HAM_ALL)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        result = base.IsAttackTarget(baseObject);//是否可以攻击对方召唤者
                        if (IsProtectTarget(baseObject))
                        {
                            result = false;
                        }
                    }
                }
                if (result)
                {
                    return result;
                }
                return base.IsAttackTarget(baseObject);
            }
            return false;
        }

        public override void Die()
        {
            if (SuperMan)
            {
                return;
            }
            IncSpell = 0;
            IncHealth = 0;
            IncHealing = 0;
            string tStr;
            if (GroupOwner != 0)
            {
                PlayObject groupOwnerPlay = (PlayObject)M2Share.ActorMgr.Get(GroupOwner);
                groupOwnerPlay.DelMember(this);// 人物死亡立即退组，以防止组队刷经验
            }
            if (LastHiter != null)
            {
                if (LastHiter.Race == ActorRace.Play)
                {
                    tStr = LastHiter.ChrName;
                }
                else
                {
                    tStr = '#' + LastHiter.ChrName;
                }
            }
            else
            {
                tStr = "####";
            }
            M2Share.EventSource.AddEventLog(GameEventLogType.PlayDie, MapName + "\t" + CurrX + "\t" + CurrY + "\t" + ChrName + "\t" + "FZ-" + HUtil32.BoolToIntStr(Envir.Flag.FightZone) + "_F3-" + HUtil32.BoolToIntStr(Envir.Flag.Fight3Zone) + "\t" + '0' + "\t" + '1' + "\t" + tStr);
            base.Die();
            SendSelfDelayMsg(Messages.RM_MASTERDIEMUTINY, 0, 0, 0, 0, "", 1000);
        }

        protected override void KickException()
        {
            MapName = M2Share.Config.HomeMap;
            CurrX = M2Share.Config.HomeX;
            CurrY = M2Share.Config.HomeY;
            BoEmergencyClose = true;
            SendSelfDelayMsg(Messages.RM_MASTERDIEMUTINY, 0, 0, 0, 0, "", 1000);
        }

        public override void RecalcAbilitys()
        {
            base.RecalcAbilitys();
            Luck = 0;
            Luck = (byte)(Luck + AddAbil.Luck);
            Luck = (byte)(Luck - AddAbil.UnLuck);
            if (Race == ActorRace.Play)
            {
                bool mhRing = false;
                bool mhBracelet = false;
                bool mhNecklace = false;
                RecoveryRing = false;
                AngryRing = false;
                MagicShield = false;
                MoXieSuite = 0;
                SuckupEnemyHealthRate = 0;
                SuckupEnemyHealth = 0;
                FastTrain = false;
                bool[] spiritArr = new bool[4] { false, false, false, false };
                bool[] cghi = new bool[4] { false, false, false, false };
                bool shRing = false;
                bool shBracelet = false;
                bool shNecklace = false;
                bool hpRing = false;
                bool hpBracelet = false;
                bool mpRing = false;
                bool mpBracelet = false;
                bool hpmpRing = false;
                bool hpmpBracelet = false;
                bool hppNecklace = false;
                bool hppBracelet = false;
                bool hppRing = false;
                bool choWeapon = false;
                bool choNecklace = false;
                bool choRing = false;
                bool choHelmet = false;
                bool choBracelet = false;
                bool psetNecklace = false;
                bool psetBracelet = false;
                bool psetRing = false;
                bool hsetNecklace = false;
                bool hsetBracelet = false;
                bool hsetRing = false;
                bool ysetNecklace = false;
                bool ysetBracelet = false;
                bool ysetRing = false;
                bool bonesetWeapon = false;
                bool bonesetHelmet = false;
                bool bonesetDress = false;
                bool bugsetNecklace = false;
                bool bugsetRing = false;
                bool bugsetBracelet = false;
                bool ptsetBelt = false;
                bool ptsetBoots = false;
                bool ptsetNecklace = false;
                bool ptsetBracelet = false;
                bool ptsetRing = false;
                bool kssetBelt = false;
                bool kssetBoots = false;
                bool kssetNecklace = false;
                bool kssetBracelet = false;
                bool kssetRing = false;
                bool rubysetBelt = false;
                bool rubysetBoots = false;
                bool rubysetNecklace = false;
                bool rubysetBracelet = false;
                bool rubysetRing = false;
                bool strongPtsetBelt = false;
                bool strongPtsetBoots = false;
                bool strongPtsetNecklace = false;
                bool strongPtsetBracelet = false;
                bool strongPtsetRing = false;
                bool strongKssetBelt = false;
                bool strongKssetBoots = false;
                bool strongKssetNecklace = false;
                bool strongKssetBracelet = false;
                bool strongKssetRing = false;
                bool strongRubysetBelt = false;
                bool strongRubysetBoots = false;
                bool strongRubysetNecklace = false;
                bool strongRubysetBracelet = false;
                bool strongRubysetRing = false;
                bool dragonsetRingLeft = false;
                bool dragonsetRingRight = false;
                bool dragonsetBraceletLeft = false;
                bool dragonsetBraceletRight = false;
                bool dragonsetNecklace = false;
                bool dragonsetDress = false;
                bool dragonsetHelmet = false;
                bool dragonsetWeapon = false;
                bool dragonsetBoots = false;
                bool dragonsetBelt = false;
                bool dsetWingdress = false;
                for (int i = 0; i < UseItems.Length; i++)
                {
                    if (UseItems[i] != null && (UseItems[i].Index > 0))
                    {
                        StdItem stdItem;
                        if (UseItems[i].Dura == 0)
                        {
                            stdItem = M2Share.WorldEngine.GetStdItem(UseItems[i].Index);
                            if (stdItem != null)
                            {
                                if ((i == ItemLocation.Weapon) || (i == ItemLocation.RighThand))
                                {
                                    WAbil.HandWeight = (byte)(WAbil.HandWeight + stdItem.Weight);
                                }
                                else
                                {
                                    WAbil.WearWeight = (byte)(WAbil.WearWeight + stdItem.Weight);
                                }
                            }
                            continue;
                        }
                        stdItem = M2Share.WorldEngine.GetStdItem(UseItems[i].Index);
                        ApplyItemParameters(UseItems[i], stdItem, ref AddAbil);
                        ApplyItemParametersEx(UseItems[i], ref WAbil);
                        if (stdItem != null)
                        {
                            if ((i == ItemLocation.Weapon) || (i == ItemLocation.RighThand))
                            {
                                WAbil.HandWeight = (byte)(WAbil.HandWeight + stdItem.Weight);
                            }
                            else
                            {
                                WAbil.WearWeight = (byte)(WAbil.WearWeight + stdItem.Weight);
                            }
                            switch (i)
                            {
                                case ItemLocation.Weapon:
                                case ItemLocation.ArmRingl:
                                case ItemLocation.ArmRingr:
                                    {
                                        if ((stdItem.SpecialPwr <= -1) && (stdItem.SpecialPwr >= -50))
                                        {
                                            AddAbil.UndeadPower = (byte)(AddAbil.UndeadPower + (-stdItem.SpecialPwr));
                                        }
                                        if ((stdItem.SpecialPwr <= -51) && (stdItem.SpecialPwr >= -100))
                                        {
                                            AddAbil.UndeadPower = (byte)(AddAbil.UndeadPower + (stdItem.SpecialPwr + 50));
                                        }
                                        switch (stdItem.Shape)
                                        {
                                            case ItemShapeConst.CCHO_WEAPON:
                                                choWeapon = true;
                                                break;
                                            case ItemShapeConst.BONESET_WEAPON_SHAPE when (stdItem.StdMode == 6):
                                                bonesetWeapon = true;
                                                break;
                                            case DragonConst.DRAGON_WEAPON_SHAPE:
                                                dragonsetWeapon = true;
                                                break;
                                        }
                                        break;
                                    }
                                case ItemLocation.Necklace:
                                    {
                                        switch (stdItem.Shape)
                                        {
                                            case ItemShapeConst.NECTLACE_FASTTRAINING_ITEM:
                                                FastTrain = true;
                                                break;
                                            case ItemShapeConst.NECTLACE_SEARCH_ITEM:
                                                ProbeNecklace = true;
                                                break;
                                            case ItemShapeConst.NECKLACE_GI_ITEM:
                                                cghi[1] = true;
                                                break;
                                            case ItemShapeConst.NECKLACE_OF_MANATOHEALTH:
                                                mhNecklace = true;
                                                MoXieSuite = MoXieSuite + stdItem.AniCount;
                                                break;
                                            case ItemShapeConst.NECKLACE_OF_SUCKHEALTH:
                                                shNecklace = true;
                                                SuckupEnemyHealthRate = SuckupEnemyHealthRate + stdItem.AniCount;
                                                break;
                                            case ItemShapeConst.NECKLACE_OF_HPPUP:
                                                hppNecklace = true;
                                                break;
                                            case ItemShapeConst.CCHO_NECKLACE:
                                                choNecklace = true;
                                                break;
                                            case ItemShapeConst.PSET_NECKLACE_SHAPE:
                                                psetNecklace = true;
                                                break;
                                            case ItemShapeConst.HSET_NECKLACE_SHAPE:
                                                hsetNecklace = true;
                                                break;
                                            case ItemShapeConst.YSET_NECKLACE_SHAPE:
                                                ysetNecklace = true;
                                                break;
                                            case ItemShapeConst.BUGSET_NECKLACE_SHAPE:
                                                bugsetNecklace = true;
                                                break;
                                            case ItemShapeConst.PTSET_NECKLACE_SHAPE:
                                                ptsetNecklace = true;
                                                break;
                                            case ItemShapeConst.KSSET_NECKLACE_SHAPE:
                                                kssetNecklace = true;
                                                break;
                                            case ItemShapeConst.RUBYSET_NECKLACE_SHAPE:
                                                rubysetNecklace = true;
                                                break;
                                            case ItemShapeConst.STRONG_PTSET_NECKLACE_SHAPE:
                                                strongPtsetNecklace = true;
                                                break;
                                            case ItemShapeConst.STRONG_KSSET_NECKLACE_SHAPE:
                                                strongKssetNecklace = true;
                                                break;
                                            case ItemShapeConst.STRONG_RUBYSET_NECKLACE_SHAPE:
                                                strongRubysetNecklace = true;
                                                break;
                                            case DragonConst.DRAGON_NECKLACE_SHAPE:
                                                dragonsetNecklace = true;
                                                break;
                                        }
                                        break;
                                    }
                                case ItemLocation.Ringr:
                                case ItemLocation.Ringl:
                                    {
                                        switch (stdItem.Shape)
                                        {
                                            case ItemShapeConst.RING_TRANSPARENT_ITEM:
                                                StatusTimeArr[PoisonState.STATETRANSPARENT] = 60000;
                                                HideMode = true;
                                                break;
                                            case ItemShapeConst.RING_SPACEMOVE_ITEM:
                                                Teleport = true;
                                                break;
                                            case ItemShapeConst.RING_MAKESTONE_ITEM:
                                                Paralysis = true;
                                                break;
                                            case ItemShapeConst.RING_REVIVAL_ITEM:
                                                Revival = true;
                                                break;
                                            case ItemShapeConst.RING_FIREBALL_ITEM:
                                                FlameRing = true;
                                                break;
                                            case ItemShapeConst.RING_HEALING_ITEM:
                                                RecoveryRing = true;
                                                break;
                                            case ItemShapeConst.RING_ANGERENERGY_ITEM:
                                                AngryRing = true;
                                                break;
                                            case ItemShapeConst.RING_MAGICSHIELD_ITEM:
                                                MagicShield = true;
                                                break;
                                            case ItemShapeConst.RING_SUPERSTRENGTH_ITEM:
                                                MuscleRing = true;
                                                break;
                                            case ItemShapeConst.RING_CHUN_ITEM:
                                                cghi[0] = true;
                                                break;
                                            case ItemShapeConst.RING_OF_MANATOHEALTH:
                                                mhRing = true;
                                                MoXieSuite = MoXieSuite + stdItem.AniCount;
                                                break;
                                            case ItemShapeConst.RING_OF_SUCKHEALTH:
                                                shRing = true;
                                                SuckupEnemyHealthRate = SuckupEnemyHealthRate + stdItem.AniCount;
                                                break;
                                            case ItemShapeConst.RING_OF_HPUP:
                                                hpRing = true;
                                                break;
                                            case ItemShapeConst.RING_OF_MPUP:
                                                mpRing = true;
                                                break;
                                            case ItemShapeConst.RING_OF_HPMPUP:
                                                hpmpRing = true;
                                                break;
                                            case ItemShapeConst.RING_OH_HPPUP:
                                                hppRing = true;
                                                break;
                                            case ItemShapeConst.CCHO_RING:
                                                choRing = true;
                                                break;
                                            case ItemShapeConst.PSET_RING_SHAPE:
                                                psetRing = true;
                                                break;
                                            case ItemShapeConst.HSET_RING_SHAPE:
                                                hsetRing = true;
                                                break;
                                            case ItemShapeConst.YSET_RING_SHAPE:
                                                ysetRing = true;
                                                break;
                                            case ItemShapeConst.BUGSET_RING_SHAPE:
                                                bugsetRing = true;
                                                break;
                                            case ItemShapeConst.PTSET_RING_SHAPE:
                                                ptsetRing = true;
                                                break;
                                            case ItemShapeConst.KSSET_RING_SHAPE:
                                                kssetRing = true;
                                                break;
                                            case ItemShapeConst.RUBYSET_RING_SHAPE:
                                                rubysetRing = true;
                                                break;
                                            case ItemShapeConst.STRONG_PTSET_RING_SHAPE:
                                                strongPtsetRing = true;
                                                break;
                                            case ItemShapeConst.STRONG_KSSET_RING_SHAPE:
                                                strongKssetRing = true;
                                                break;
                                            case ItemShapeConst.STRONG_RUBYSET_RING_SHAPE:
                                                strongRubysetRing = true;
                                                break;
                                            case DragonConst.DRAGON_RING_SHAPE:
                                                {
                                                    if ((i == ItemLocation.Ringl))
                                                    {
                                                        dragonsetRingLeft = true;
                                                    }
                                                    if ((i == ItemLocation.Ringr))
                                                    {
                                                        dragonsetRingRight = true;
                                                    }
                                                    break;
                                                }
                                        }
                                        break;
                                    }
                            }
                            switch (i)
                            {
                                case ItemLocation.ArmRingl:
                                case ItemLocation.ArmRingr:
                                    {
                                        switch (stdItem.Shape)
                                        {
                                            case ItemShapeConst.ARMRING_HAP_ITEM:
                                                cghi[2] = true;
                                                break;
                                            case ItemShapeConst.BRACELET_OF_MANATOHEALTH:
                                                mhBracelet = true;
                                                MoXieSuite = MoXieSuite + stdItem.AniCount;
                                                break;
                                            case ItemShapeConst.BRACELET_OF_SUCKHEALTH:
                                                shBracelet = true;
                                                SuckupEnemyHealthRate = SuckupEnemyHealthRate + stdItem.AniCount;
                                                break;
                                            case ItemShapeConst.BRACELET_OF_HPUP:
                                                hpBracelet = true;
                                                break;
                                            case ItemShapeConst.BRACELET_OF_MPUP:
                                                mpBracelet = true;
                                                break;
                                            case ItemShapeConst.BRACELET_OF_HPMPUP:
                                                hpmpBracelet = true;
                                                break;
                                            case ItemShapeConst.BRACELET_OF_HPPUP:
                                                hppBracelet = true;
                                                break;
                                            case ItemShapeConst.CCHO_BRACELET:
                                                choBracelet = true;
                                                break;
                                            case ItemShapeConst.PSET_BRACELET_SHAPE:
                                                psetBracelet = true;
                                                break;
                                            case ItemShapeConst.HSET_BRACELET_SHAPE:
                                                hsetBracelet = true;
                                                break;
                                            case ItemShapeConst.YSET_BRACELET_SHAPE:
                                                ysetBracelet = true;
                                                break;
                                            case ItemShapeConst.BUGSET_BRACELET_SHAPE:
                                                bugsetBracelet = true;
                                                break;
                                            case ItemShapeConst.PTSET_BRACELET_SHAPE:
                                                ptsetBracelet = true;
                                                break;
                                            case ItemShapeConst.KSSET_BRACELET_SHAPE:
                                                kssetBracelet = true;
                                                break;
                                            case ItemShapeConst.RUBYSET_BRACELET_SHAPE:
                                                rubysetBracelet = true;
                                                break;
                                            case ItemShapeConst.STRONG_PTSET_BRACELET_SHAPE:
                                                strongPtsetBracelet = true;
                                                break;
                                            case ItemShapeConst.STRONG_KSSET_BRACELET_SHAPE:
                                                strongKssetBracelet = true;
                                                break;
                                            case ItemShapeConst.STRONG_RUBYSET_BRACELET_SHAPE:
                                                strongRubysetBracelet = true;
                                                break;
                                            case DragonConst.DRAGON_BRACELET_SHAPE:
                                                {
                                                    if ((i == ItemLocation.ArmRingl))
                                                    {
                                                        dragonsetBraceletLeft = true;
                                                    }
                                                    if ((i == ItemLocation.ArmRingr))
                                                    {
                                                        dragonsetBraceletRight = true;
                                                    }
                                                    break;
                                                }
                                        }
                                        break;
                                    }
                                case ItemLocation.Helmet:
                                    {
                                        switch (stdItem.Shape)
                                        {
                                            case ItemShapeConst.HELMET_IL_ITEM:
                                                cghi[3] = true;
                                                break;
                                            case ItemShapeConst.CCHO_HELMET:
                                                choHelmet = true;
                                                break;
                                            case ItemShapeConst.BONESET_HELMET_SHAPE:
                                                bonesetHelmet = true;
                                                break;
                                            case DragonConst.DRAGON_HELMET_SHAPE:
                                                dragonsetHelmet = true;
                                                break;
                                        }
                                        break;
                                    }
                                case ItemLocation.Dress:
                                    {
                                        switch (stdItem.Shape)
                                        {
                                            case ItemShapeConst.DRESS_SHAPE_WING:
                                                dsetWingdress = true;
                                                break;
                                            case ItemShapeConst.BONESET_DRESS_SHAPE:
                                                bonesetDress = true;
                                                break;
                                            case DragonConst.DRAGON_DRESS_SHAPE:
                                                dragonsetDress = true;
                                                break;
                                        }
                                        break;
                                    }
                                case ItemLocation.Belt:
                                    {
                                        switch (stdItem.Shape)
                                        {
                                            case ItemShapeConst.PTSET_BELT_SHAPE:
                                                ptsetBelt = true;
                                                break;
                                            case ItemShapeConst.KSSET_BELT_SHAPE:
                                                kssetBelt = true;
                                                break;
                                            case ItemShapeConst.RUBYSET_BELT_SHAPE:
                                                rubysetBelt = true;
                                                break;
                                            case ItemShapeConst.STRONG_PTSET_BELT_SHAPE:
                                                strongPtsetBelt = true;
                                                break;
                                            case ItemShapeConst.STRONG_KSSET_BELT_SHAPE:
                                                strongKssetBelt = true;
                                                break;
                                            case ItemShapeConst.STRONG_RUBYSET_BELT_SHAPE:
                                                strongRubysetBelt = true;
                                                break;
                                            case DragonConst.DRAGON_BELT_SHAPE:
                                                dragonsetBelt = true;
                                                break;
                                        }
                                        break;
                                    }
                                case ItemLocation.Boots:
                                    {
                                        switch (stdItem.Shape)
                                        {
                                            case ItemShapeConst.PTSET_BOOTS_SHAPE:
                                                ptsetBoots = true;
                                                break;
                                            case ItemShapeConst.KSSET_BOOTS_SHAPE:
                                                kssetBoots = true;
                                                break;
                                            case ItemShapeConst.RUBYSET_BOOTS_SHAPE:
                                                rubysetBoots = true;
                                                break;
                                            case ItemShapeConst.STRONG_PTSET_BOOTS_SHAPE:
                                                strongPtsetBoots = true;
                                                break;
                                            case ItemShapeConst.STRONG_KSSET_BOOTS_SHAPE:
                                                strongKssetBoots = true;
                                                break;
                                            case ItemShapeConst.STRONG_RUBYSET_BOOTS_SHAPE:
                                                strongRubysetBoots = true;
                                                break;
                                            case DragonConst.DRAGON_BOOTS_SHAPE:
                                                dragonsetBoots = true;
                                                break;
                                        }
                                        break;
                                    }
                                case ItemLocation.Charm:
                                    {
                                        if ((stdItem.StdMode == 53) && (stdItem.Shape == ItemShapeConst.SHAPE_OF_LUCKYLADLE))
                                        {
                                            AddAbil.Luck = (byte)(HUtil32._MIN(255, AddAbil.Luck + 1));
                                        }
                                        break;
                                    }
                            }

                            switch (stdItem.StdMode)
                            {
                                case ItemShapeConst.SpiritItem1:
                                    spiritArr[0] = true;
                                    break;
                                case ItemShapeConst.SpiritItem2:
                                    spiritArr[1] = true;
                                    break;
                                case ItemShapeConst.SpiritItem3:
                                    spiritArr[2] = true;
                                    break;
                                case ItemShapeConst.SpiritItem4:
                                    spiritArr[3] = true;
                                    break;
                            }
                        }
                    }
                }
                if (cghi[0] && cghi[1] && cghi[2] && cghi[3]) //记忆套装
                {
                    RecallSuite = true;
                }
                if (mhNecklace && mhBracelet && mhRing)
                {
                    MoXieSuite = MoXieSuite + 50;
                }
                if (shNecklace && shBracelet && shRing)
                {
                    AddAbil.HIT = (ushort)(AddAbil.HIT + 2);
                }
                if (hpBracelet && hpRing)
                {
                    AddAbil.HP = (ushort)(AddAbil.HP + 50);
                }
                if (mpBracelet && mpRing)
                {
                    AddAbil.MP = (ushort)(AddAbil.MP + 50);
                }
                if (hpmpBracelet && hpmpRing)
                {
                    AddAbil.HP = (ushort)(AddAbil.HP + 30);
                    AddAbil.MP = (ushort)(AddAbil.MP + 30);
                }
                if (hppNecklace && hppBracelet && hppRing)
                {
                    AddAbil.HP = (ushort)(AddAbil.HP + ((WAbil.MaxHP * 30) / 100));
                    AddAbil.AC = (ushort)(AddAbil.AC + HUtil32.MakeWord(2, 2));
                }
                if (choWeapon && choNecklace && choRing && choHelmet && choBracelet)
                {
                    AddAbil.HitSpeed = (ushort)(AddAbil.HitSpeed + 4);
                    AddAbil.DC = (ushort)(AddAbil.DC + HUtil32.MakeWord(2, 5));
                }
                if (psetBracelet && psetRing)
                {
                    AddAbil.HitSpeed = (ushort)(AddAbil.HitSpeed + 2);
                    if (psetNecklace)
                    {
                        AddAbil.DC = (ushort)(AddAbil.DC + HUtil32.MakeWord(1, 3));
                    }
                }
                if (hsetBracelet && hsetRing)
                {
                    WAbil.MaxWeight = (ushort)(WAbil.MaxWeight + 20);
                    WAbil.MaxWearWeight = (byte)(HUtil32._MIN(255, WAbil.MaxWearWeight + 5));
                    if (hsetNecklace)
                    {
                        AddAbil.MC = (ushort)(AddAbil.MC + HUtil32.MakeWord(1, 2));
                    }
                }
                if (ysetBracelet && ysetRing)
                {
                    AddAbil.UndeadPower = (byte)(AddAbil.UndeadPower + 3);
                    if (ysetNecklace)
                    {
                        AddAbil.SC = (ushort)(AddAbil.SC + HUtil32.MakeWord(1, 2));
                    }
                }
                if (bonesetWeapon && bonesetHelmet && bonesetDress)
                {
                    AddAbil.AC = (ushort)(AddAbil.AC + HUtil32.MakeWord(0, 2));
                    AddAbil.MC = (ushort)(AddAbil.MC + HUtil32.MakeWord(0, 1));
                    AddAbil.SC = (ushort)(AddAbil.SC + HUtil32.MakeWord(0, 1));
                }
                if (bugsetNecklace && bugsetRing && bugsetBracelet)
                {
                    AddAbil.DC = (ushort)(AddAbil.DC + HUtil32.MakeWord(0, 1));
                    AddAbil.MC = (ushort)(AddAbil.MC + HUtil32.MakeWord(0, 1));
                    AddAbil.SC = (ushort)(AddAbil.SC + HUtil32.MakeWord(0, 1));
                    AddAbil.AntiMagic = (ushort)(AddAbil.AntiMagic + 1);
                    AddAbil.AntiPoison = (ushort)(AddAbil.AntiPoison + 1);
                }
                if (ptsetBelt && ptsetBoots && ptsetNecklace && ptsetBracelet && ptsetRing)
                {
                    AddAbil.DC = (ushort)(AddAbil.DC + HUtil32.MakeWord(0, 2));
                    AddAbil.AC = (ushort)(AddAbil.AC + HUtil32.MakeWord(0, 2));
                    WAbil.MaxHandWeight = (byte)(HUtil32._MIN(255, WAbil.MaxHandWeight + 1));
                    WAbil.MaxWearWeight = (byte)(HUtil32._MIN(255, WAbil.MaxWearWeight + 2));
                }
                if (kssetBelt && kssetBoots && kssetNecklace && kssetBracelet && kssetRing)
                {
                    AddAbil.SC = (ushort)(AddAbil.SC + HUtil32.MakeWord(0, 2));
                    AddAbil.AC = (ushort)(AddAbil.AC + HUtil32.MakeWord(0, 1));
                    AddAbil.MAC = (ushort)(AddAbil.MAC + HUtil32.MakeWord(0, 1));
                    AddAbil.SPEED = (ushort)(AddAbil.SPEED + 1);
                    WAbil.MaxHandWeight = (byte)(HUtil32._MIN(255, WAbil.MaxHandWeight + 1));
                    WAbil.MaxWearWeight = (byte)(HUtil32._MIN(255, WAbil.MaxWearWeight + 2));
                }
                if (rubysetBelt && rubysetBoots && rubysetNecklace && rubysetBracelet && rubysetRing)
                {
                    AddAbil.MC = (ushort)(AddAbil.MC + HUtil32.MakeWord(0, 2));
                    AddAbil.MAC = (ushort)(AddAbil.MAC + HUtil32.MakeWord(0, 2));
                    WAbil.MaxHandWeight = (byte)(HUtil32._MIN(255, WAbil.MaxHandWeight + 1));
                    WAbil.MaxWearWeight = (byte)(HUtil32._MIN(255, WAbil.MaxWearWeight + 2));
                }
                if (strongPtsetBelt && strongPtsetBoots && strongPtsetNecklace && strongPtsetBracelet && strongPtsetRing)
                {
                    AddAbil.DC = (ushort)(AddAbil.DC + HUtil32.MakeWord(0, 3));
                    AddAbil.HP = (ushort)(AddAbil.HP + 30);
                    AddAbil.HitSpeed = (ushort)(AddAbil.HitSpeed + 2);
                    WAbil.MaxWearWeight = (byte)(HUtil32._MIN(255, WAbil.MaxWearWeight + 2));
                }
                if (strongKssetBelt && strongKssetBoots && strongKssetNecklace && strongKssetBracelet && strongKssetRing)
                {
                    AddAbil.SC = (ushort)(AddAbil.SC + HUtil32.MakeWord(0, 2));
                    AddAbil.HP = (ushort)(AddAbil.HP + 15);
                    AddAbil.MP = (ushort)(AddAbil.MP + 20);
                    AddAbil.UndeadPower = (byte)(AddAbil.UndeadPower + 1);
                    AddAbil.HIT = (ushort)(AddAbil.HIT + 1);
                    AddAbil.SPEED = (ushort)(AddAbil.SPEED + 1);
                    WAbil.MaxWearWeight = (byte)(HUtil32._MIN(255, WAbil.MaxWearWeight + 2));
                }
                if (strongRubysetBelt && strongRubysetBoots && strongRubysetNecklace && strongRubysetBracelet && strongRubysetRing)
                {
                    AddAbil.MC = (ushort)(AddAbil.MC + HUtil32.MakeWord(0, 2));
                    AddAbil.MP = (ushort)(AddAbil.MP + 40);
                    AddAbil.SPEED = (ushort)(AddAbil.SPEED + 2);
                    WAbil.MaxWearWeight = (byte)(HUtil32._MIN(255, WAbil.MaxWearWeight + 2));
                }
                if (dragonsetRingLeft && dragonsetRingRight && dragonsetBraceletLeft && dragonsetBraceletRight && dragonsetNecklace && dragonsetDress && dragonsetHelmet && dragonsetWeapon && dragonsetBoots && dragonsetBelt)
                {
                    AddAbil.AC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(AddAbil.AC) + 1), (ushort)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.AC) + 4));
                    AddAbil.MAC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(AddAbil.MAC) + 1), (ushort)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.MAC) + 4));
                    AddAbil.Luck = (byte)(HUtil32._MIN(255, AddAbil.Luck + 2));
                    AddAbil.HitSpeed = (ushort)(AddAbil.HitSpeed + 2);
                    AddAbil.AntiMagic = (ushort)(AddAbil.AntiMagic + 6);
                    AddAbil.AntiPoison = (ushort)(AddAbil.AntiPoison + 6);
                    WAbil.MaxHandWeight = (byte)(HUtil32._MIN(255, WAbil.MaxHandWeight + 34));
                    WAbil.MaxWearWeight = (byte)(HUtil32._MIN(255, WAbil.MaxWearWeight + 27));
                    WAbil.MaxWeight = (ushort)(WAbil.MaxWeight + 120);
                    WAbil.MaxHP = (ushort)(WAbil.MaxHP + 70);
                    WAbil.MaxMP = (ushort)(WAbil.MaxMP + 80);
                    AddAbil.SPEED = (ushort)(AddAbil.SPEED + 1);
                    AddAbil.DC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(AddAbil.DC) + 1), (ushort)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.DC) + 4));
                    AddAbil.MC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(AddAbil.MC) + 1), (ushort)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.MC) + 3));
                    AddAbil.SC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(AddAbil.SC) + 1), (ushort)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.SC) + 3));
                }
                else
                {
                    if (dragonsetDress && dragonsetHelmet && dragonsetWeapon && dragonsetBoots && dragonsetBelt)
                    {
                        WAbil.MaxHandWeight = (byte)(HUtil32._MIN(255, WAbil.MaxHandWeight + 34));
                        WAbil.MaxWeight = (ushort)(WAbil.MaxWeight + 50);
                        AddAbil.SPEED = (ushort)(AddAbil.SPEED + 1);
                        AddAbil.DC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(AddAbil.DC) + 1), (ushort)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.DC) + 4));
                        AddAbil.MC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(AddAbil.MC) + 1), (ushort)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.MC) + 3));
                        AddAbil.SC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(AddAbil.SC) + 1), (ushort)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.SC) + 3));
                    }
                    else if (dragonsetDress && dragonsetBoots && dragonsetBelt)
                    {
                        WAbil.MaxHandWeight = (byte)(HUtil32._MIN(255, WAbil.MaxHandWeight + 17));
                        WAbil.MaxWeight = (ushort)(WAbil.MaxWeight + 30);
                        AddAbil.DC = HUtil32.MakeWord(HUtil32.LoByte(AddAbil.DC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.DC) + 1));
                        AddAbil.MC = HUtil32.MakeWord(HUtil32.LoByte(AddAbil.MC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.MC) + 1));
                        AddAbil.SC = HUtil32.MakeWord(HUtil32.LoByte(AddAbil.SC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.SC) + 1));
                    }
                    else if (dragonsetDress && dragonsetHelmet && dragonsetWeapon)
                    {
                        AddAbil.DC = HUtil32.MakeWord(HUtil32.LoByte(AddAbil.DC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.DC) + 2));
                        AddAbil.MC = HUtil32.MakeWord(HUtil32.LoByte(AddAbil.MC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.MC) + 1));
                        AddAbil.SC = HUtil32.MakeWord(HUtil32.LoByte(AddAbil.SC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.SC) + 1));
                        AddAbil.SPEED = (ushort)(AddAbil.SPEED + 1);
                    }
                    if (dragonsetRingLeft && dragonsetRingRight && dragonsetBraceletLeft && dragonsetBraceletRight && dragonsetNecklace)
                    {
                        WAbil.MaxWearWeight = (byte)(HUtil32._MIN(255, WAbil.MaxWearWeight + 27));
                        WAbil.MaxWeight = (ushort)(WAbil.MaxWeight + 50);
                        AddAbil.AC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(AddAbil.AC) + 1), (ushort)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.AC) + 3));
                        AddAbil.MAC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(AddAbil.MAC) + 1), (ushort)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.MAC) + 3));
                    }
                    else if ((dragonsetRingLeft || dragonsetRingRight) && dragonsetBraceletLeft && dragonsetBraceletRight && dragonsetNecklace)
                    {
                        WAbil.MaxWearWeight = (byte)(HUtil32._MIN(255, WAbil.MaxWearWeight + 17));
                        WAbil.MaxWeight = (ushort)(WAbil.MaxWeight + 30);
                        AddAbil.AC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(AddAbil.AC) + 1), (ushort)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.AC) + 1));
                        AddAbil.MAC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(AddAbil.MAC) + 1), (ushort)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.MAC) + 1));
                    }
                    else if (dragonsetRingLeft && dragonsetRingRight && (dragonsetBraceletLeft || dragonsetBraceletRight) && dragonsetNecklace)
                    {
                        WAbil.MaxWearWeight = (byte)(HUtil32._MIN(255, WAbil.MaxWearWeight + 17));
                        WAbil.MaxWeight = (ushort)(WAbil.MaxWeight + 30);
                        AddAbil.AC = HUtil32.MakeWord(HUtil32.LoByte(AddAbil.AC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.AC) + 2));
                        AddAbil.MAC = HUtil32.MakeWord(HUtil32.LoByte(AddAbil.MAC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.MAC) + 2));
                    }
                    else if ((dragonsetRingLeft || dragonsetRingRight) && (dragonsetBraceletLeft || dragonsetBraceletRight) && dragonsetNecklace)
                    {
                        WAbil.MaxWearWeight = (byte)(HUtil32._MIN(255, WAbil.MaxWearWeight + 17));
                        WAbil.MaxWeight = (ushort)(WAbil.MaxWeight + 30);
                        AddAbil.AC = HUtil32.MakeWord(HUtil32.LoByte(AddAbil.AC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.AC) + 1));
                        AddAbil.MAC = HUtil32.MakeWord(HUtil32.LoByte(AddAbil.MAC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.MAC) + 1));
                    }
                    else
                    {
                        if (dragonsetBraceletLeft && dragonsetBraceletRight)
                        {
                            AddAbil.AC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(AddAbil.AC) + 1), (ushort)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.AC)));
                            AddAbil.MAC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(AddAbil.MAC) + 1), (ushort)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.MAC)));
                        }
                        if (dragonsetRingLeft && dragonsetRingRight)
                        {
                            AddAbil.AC = HUtil32.MakeWord(HUtil32.LoByte(AddAbil.AC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.AC) + 1));
                            AddAbil.MAC = HUtil32.MakeWord(HUtil32.LoByte(AddAbil.MAC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.MAC) + 1));
                        }
                    }
                }
                if (dsetWingdress && (Abil.Level >= 20))
                {
                    switch (Abil.Level)
                    {
                        case < 40:
                            AddAbil.DC = (ushort)(AddAbil.DC + HUtil32.MakeWord(0, 1));
                            AddAbil.MC = (ushort)(AddAbil.MC + HUtil32.MakeWord(0, 2));
                            AddAbil.SC = (ushort)(AddAbil.SC + HUtil32.MakeWord(0, 2));
                            AddAbil.AC = (ushort)(AddAbil.AC + HUtil32.MakeWord(2, 3));
                            AddAbil.MAC = (ushort)(AddAbil.MAC + HUtil32.MakeWord(0, 2));
                            break;
                        case < 50:
                            AddAbil.DC = (ushort)(AddAbil.DC + HUtil32.MakeWord(0, 3));
                            AddAbil.MC = (ushort)(AddAbil.MC + HUtil32.MakeWord(0, 4));
                            AddAbil.SC = (ushort)(AddAbil.SC + HUtil32.MakeWord(0, 4));
                            AddAbil.AC = (ushort)(AddAbil.AC + HUtil32.MakeWord(5, 5));
                            AddAbil.MAC = (ushort)(AddAbil.MAC + HUtil32.MakeWord(1, 2));
                            break;
                        default:
                            AddAbil.DC = (ushort)(AddAbil.DC + HUtil32.MakeWord(0, 5));
                            AddAbil.MC = (ushort)(AddAbil.MC + HUtil32.MakeWord(0, 6));
                            AddAbil.SC = (ushort)(AddAbil.SC + HUtil32.MakeWord(0, 6));
                            AddAbil.AC = (ushort)(AddAbil.AC + HUtil32.MakeWord(9, 7));
                            AddAbil.MAC = (ushort)(AddAbil.MAC + HUtil32.MakeWord(2, 4));
                            break;
                    }
                }
                WAbil.Weight = RecalcBagWeight();

                if (FlameRing)
                {
                    AddItemSkill(Settings.AM_FIREBALL);
                }
                else
                {
                    DelItemSkill(Settings.AM_FIREBALL);
                }
                if (RecoveryRing)
                {
                    AddItemSkill(Settings.AM_HEALING);
                }
                else
                {
                    DelItemSkill(Settings.AM_HEALING);
                }
                if (MuscleRing)
                {
                    WAbil.MaxWeight = (ushort)(WAbil.MaxWeight * 2);
                    WAbil.MaxWearWeight = (byte)HUtil32._MIN(255, WAbil.MaxWearWeight * 2);
                    if ((WAbil.MaxHandWeight * 2 > 255))
                    {
                        WAbil.MaxHandWeight = 255;
                    }
                    else
                    {
                        WAbil.MaxHandWeight = (byte)(WAbil.MaxHandWeight * 2);
                    }
                }
                if (MoXieSuite > 0) //魔血套装
                {
                    if (MoXieSuite >= WAbil.MaxMP)
                    {
                        MoXieSuite = WAbil.MaxMP - 1;
                    }
                    WAbil.MaxMP = (ushort)(WAbil.MaxMP - MoXieSuite);
                    WAbil.MaxHP = (ushort)(WAbil.MaxHP + MoXieSuite);
                    if ((Race == ActorRace.Play) && (WAbil.HP > WAbil.MaxHP))
                    {
                        WAbil.HP = WAbil.MaxHP;
                    }
                }
                if (spiritArr[0] && spiritArr[2] && spiritArr[3] && spiritArr[4]) //祈祷套装
                {
                    IsSpirit = true;
                }
                if ((Race == ActorRace.Play) && (WAbil.HP > WAbil.MaxHP) && (!mhNecklace && !mhBracelet && !mhRing))
                {
                    WAbil.HP = WAbil.MaxHP;
                }
                if ((Race == ActorRace.Play) && (WAbil.MP > WAbil.MaxMP))
                {
                    WAbil.MP = WAbil.MaxMP;
                }
                if (ExtraAbil[AbilConst.EABIL_DCUP] > 0)
                {
                    WAbil.DC = HUtil32.MakeWord(HUtil32.LoByte(WAbil.DC), (ushort)(HUtil32.HiByte(WAbil.DC) + ExtraAbil[AbilConst.EABIL_DCUP]));
                }
                if (ExtraAbil[AbilConst.EABIL_MCUP] > 0)
                {
                    WAbil.MC = HUtil32.MakeWord(HUtil32.LoByte(WAbil.MC), (ushort)(HUtil32.HiByte(WAbil.MC) + ExtraAbil[AbilConst.EABIL_MCUP]));
                }
                if (ExtraAbil[AbilConst.EABIL_SCUP] > 0)
                {
                    WAbil.SC = HUtil32.MakeWord(HUtil32.LoByte(WAbil.SC), (ushort)(HUtil32.HiByte(WAbil.SC) + ExtraAbil[AbilConst.EABIL_SCUP]));
                }
                if (ExtraAbil[AbilConst.EABIL_HITSPEEDUP] > 0)
                {
                    HitSpeed = (ushort)(HitSpeed + ExtraAbil[AbilConst.EABIL_HITSPEEDUP]);
                }
                if (ExtraAbil[AbilConst.EABIL_HPUP] > 0)
                {
                    WAbil.MaxHP = (ushort)(WAbil.MaxHP + ExtraAbil[AbilConst.EABIL_HPUP]);
                }
                if (ExtraAbil[AbilConst.EABIL_MPUP] > 0)
                {
                    WAbil.MaxMP = (ushort)(WAbil.MaxMP + ExtraAbil[AbilConst.EABIL_MPUP]);
                }
                if (ExtraAbil[AbilConst.EABIL_PWRRATE] > 0)
                {
                    WAbil.DC = HUtil32.MakeWord((ushort)((HUtil32.LoByte(WAbil.DC) * ExtraAbil[AbilConst.EABIL_PWRRATE]) / 100), (ushort)((HUtil32.HiByte(WAbil.DC) * ExtraAbil[AbilConst.EABIL_PWRRATE]) / 100));
                    WAbil.MC = HUtil32.MakeWord((ushort)((HUtil32.LoByte(WAbil.MC) * ExtraAbil[AbilConst.EABIL_PWRRATE]) / 100), (ushort)((HUtil32.HiByte(WAbil.MC) * ExtraAbil[AbilConst.EABIL_PWRRATE]) / 100));
                    WAbil.SC = HUtil32.MakeWord((ushort)((HUtil32.LoByte(WAbil.SC) * ExtraAbil[AbilConst.EABIL_PWRRATE]) / 100), (ushort)((HUtil32.HiByte(WAbil.SC) * ExtraAbil[AbilConst.EABIL_PWRRATE]) / 100));
                }
                if (Race == ActorRace.Play)
                {
                    bool fastmoveflag = UseItems[ItemLocation.Boots] != null && UseItems[ItemLocation.Boots].Dura > 0 && UseItems[ItemLocation.Boots].Index == Settings.INDEX_MIRBOOTS;
                    if (fastmoveflag)
                    {
                        StatusTimeArr[PoisonState.FASTMOVE] = 60000;
                    }
                    else
                    {
                        StatusTimeArr[PoisonState.FASTMOVE] = 0;
                    }
                    //if ((Abil.Level >= EfftypeConst.EFFECTIVE_HIGHLEVEL))
                    //{
                    //    if (BoHighLevelEffect)
                    //    {
                    //        StatusTimeArr[Grobal2.STATE_50LEVELEFFECT] = 60000;
                    //    }
                    //    else
                    //    {
                    //        StatusTimeArr[Grobal2.STATE_50LEVELEFFECT] = 0;
                    //    }
                    //}
                    //else
                    //{
                    //    StatusTimeArr[Grobal2.STATE_50LEVELEFFECT] = 0;
                    //}
                    CharStatus = GetCharStatus();
                    StatusChanged();
                    SendUpdateMsg(Messages.RM_CHARSTATUSCHANGED, HitSpeed, CharStatus, 0, 0, "");
                }
                RecalcAdjusBonus();

                byte oldlight = Light;
                Light = GetMyLight();
                if (oldlight != Light)
                {
                    SendRefMsg(Messages.RM_CHANGELIGHT, 0, 0, 0, 0, "");
                }
                if (IsSpirit)
                {
                    SendSelfDelayMsg(Messages.RM_SPIRITSUITE, 0, 0, 0, 0, "", 500);
                }
                SpeedPoint = (byte)(SpeedPoint + AddAbil.SPEED);
                HitPoint = (byte)(HitPoint + AddAbil.HIT);
                AntiPoison = (byte)(AntiPoison + AddAbil.AntiPoison);
                PoisonRecover = (ushort)(PoisonRecover + AddAbil.PoisonRecover);
                HealthRecover = (ushort)(HealthRecover + AddAbil.HealthRecover);
                SpellRecover = (ushort)(SpellRecover + AddAbil.SpellRecover);
                AntiMagic = (ushort)(AntiMagic + AddAbil.AntiMagic);
                Luck = (byte)(Luck + AddAbil.Luck);
                Luck = (byte)(Luck - AddAbil.UnLuck);
                HitSpeed = AddAbil.HitSpeed;
                WAbil.MaxHP = (ushort)(Abil.MaxHP + AddAbil.HP);
                WAbil.MaxMP = (ushort)(Abil.MaxMP + AddAbil.MP);
                WAbil.AC = HUtil32.MakeWord((byte)HUtil32._MIN(255, HUtil32.LoByte(AddAbil.AC) + HUtil32.LoByte(Abil.AC)), (byte)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.AC) + HUtil32.HiByte(Abil.AC)));
                WAbil.MAC = HUtil32.MakeWord((byte)HUtil32._MIN(255, HUtil32.LoByte(AddAbil.MAC) + HUtil32.LoByte(Abil.MAC)), (byte)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.MAC) + HUtil32.HiByte(Abil.MAC)));
                WAbil.DC = HUtil32.MakeWord((byte)HUtil32._MIN(255, HUtil32.LoByte(AddAbil.DC) + HUtil32.LoByte(Abil.DC)), (byte)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.DC) + HUtil32.HiByte(Abil.DC)));
                WAbil.MC = HUtil32.MakeWord((byte)HUtil32._MIN(255, HUtil32.LoByte(AddAbil.MC) + HUtil32.LoByte(Abil.MC)), (byte)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.MC) + HUtil32.HiByte(Abil.MC)));
                WAbil.SC = HUtil32.MakeWord((byte)HUtil32._MIN(255, HUtil32.LoByte(AddAbil.SC) + HUtil32.LoByte(Abil.SC)), (byte)HUtil32._MIN(255, HUtil32.HiByte(AddAbil.SC) + HUtil32.HiByte(Abil.SC)));
            }
        }

        public override int GetAttackPower(int nBasePower, int nPower)
        {
            if (nPower < 0)
            {
                nPower = 0;
            }
            int result = 0;
            if (Luck > 0)
            {
                if (M2Share.RandomNumber.Random(10 - HUtil32._MIN(9, Luck)) == 0)
                {
                    result = nBasePower + nPower;
                }
                else
                {
                    result = nBasePower + M2Share.RandomNumber.Random(nPower + 1);
                }
            }
            else
            {
                result = nBasePower + M2Share.RandomNumber.Random(nPower + 1);
                if (Luck <= 0)
                {
                    if (M2Share.RandomNumber.Random(10 - HUtil32._MAX(0, -Luck)) == 0)
                    {
                        result = nBasePower;
                    }
                }
            }
            result = HUtil32.Round(result * (PowerRate / 100.0));
            if (BoPowerItem)
            {
                result = HUtil32.Round(PowerItem * (double)result);
            }
            if (AutoChangeColor)
            {
                result = result * AutoChangeIdx + 1;
            }
            if (FixColor)
            {
                result = result * FixColorIdx + 1;
            }
            return result;
        }

        public override void StruckDamage(int nDamage)
        {
            base.StruckDamage(nDamage);
            ushort nDam = (ushort)(M2Share.RandomNumber.Random(10) + 5);
            if (StatusTimeArr[PoisonState.DAMAGEARMOR] > 0)
            {
                nDam = (ushort)HUtil32.Round(nDam * (M2Share.Config.PosionDamagarmor / 10.0)); // 1.2
            }
            bool boRecalcAbi = false;
            ushort nDura;
            int nOldDura;
            if (UseItems[ItemLocation.Dress] != null && UseItems[ItemLocation.Dress].Index > 0)
            {
                nDura = UseItems[ItemLocation.Dress].Dura;
                nOldDura = HUtil32.Round(nDura / 1000.0);
                nDura -= nDam;
                if (nDura <= 0)
                {
                    SendDelItems(UseItems[ItemLocation.Dress]);
                    StdItem stdItem = M2Share.WorldEngine.GetStdItem(UseItems[ItemLocation.Dress].Index);
                    if (stdItem.NeedIdentify == 1)
                    {
                        M2Share.EventSource.AddEventLog(3, MapName + "\t" + CurrX + "\t" + CurrY + "\t" +
                                                           ChrName + "\t" + stdItem.Name + "\t" +
                                                           UseItems[ItemLocation.Dress].MakeIndex + "\t"
                                                           + HUtil32.BoolToIntStr(Race == ActorRace.Play) +
                                                           "\t" + '0');
                    }
                    UseItems[ItemLocation.Dress].Index = 0;
                    FeatureChanged();
                    UseItems[ItemLocation.Dress].Index = 0;
                    UseItems[ItemLocation.Dress].Dura = 0;
                    boRecalcAbi = true;
                }
                else
                {
                    UseItems[ItemLocation.Dress].Dura = nDura;
                }
                if (nOldDura != HUtil32.Round(nDura / 1000.0))
                {
                    SendMsg(Messages.RM_DURACHANGE, ItemLocation.Dress, nDura, UseItems[ItemLocation.Dress].DuraMax, 0);
                }
            }

            for (int i = 0; i < UseItems.Length; i++)
            {
                if ((UseItems[i] != null) && (UseItems[i].Index > 0) && (M2Share.RandomNumber.Random(8) == 0))
                {
                    nDura = UseItems[i].Dura;
                    nOldDura = HUtil32.Round(nDura / 1000.0);
                    nDura -= nDam;
                    if (nDura <= 0)
                    {
                        SendDelItems(UseItems[i]);
                        StdItem stdItem = M2Share.WorldEngine.GetStdItem(UseItems[i].Index);
                        if (stdItem.NeedIdentify == 1)
                        {
                            M2Share.EventSource.AddEventLog(3, MapName + "\t" + CurrX + "\t" + CurrY + "\t" + ChrName + "\t" + stdItem.Name + "\t" +
                                                   UseItems[i].MakeIndex + "\t" + HUtil32.BoolToIntStr(Race == ActorRace.Play) + "\t" + '0');
                        }
                        UseItems[i].Index = 0;
                        FeatureChanged();
                        UseItems[i].Index = 0;
                        UseItems[i].Dura = 0;
                        boRecalcAbi = true;
                    }
                    else
                    {
                        UseItems[i].Dura = nDura;
                    }
                    if (nOldDura != HUtil32.Round(nDura / 1000.0))
                    {
                        SendMsg(Messages.RM_DURACHANGE, i, nDura, UseItems[i].DuraMax, 0);
                    }
                }
            }
            if (boRecalcAbi)
            {
                RecalcAbilitys();
                SendMsg(Messages.RM_ABILITY, 0, 0, 0, 0);
                SendMsg(Messages.RM_SUBABILITY, 0, 0, 0, 0);
            }
        }

        /// <summary>
        /// 更新玩家自身可见的玩家和怪物
        /// </summary>
        /// <param name="baseObject"></param>
        protected override void UpdateVisibleGay(BaseObject baseObject)
        {
            bool boIsVisible = false;
            VisibleBaseObject visibleBaseObject;
            if (baseObject.Race == ActorRace.Play || baseObject.Master != null)
            {
                IsVisibleActive = true;// 如果是人物或宝宝则置TRUE
            }
            for (int i = 0; i < VisibleActors.Count; i++)
            {
                visibleBaseObject = VisibleActors[i];
                if (visibleBaseObject.BaseObject == baseObject)
                {
                    visibleBaseObject.VisibleFlag = VisibleFlag.Invisible;
                    boIsVisible = true;
                    break;
                }
            }
            if (boIsVisible)
            {
                return;
            }
            visibleBaseObject = new VisibleBaseObject
            {
                VisibleFlag = VisibleFlag.Show,
                BaseObject = baseObject
            };
            VisibleActors.Add(visibleBaseObject);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SearchViewRange()
        {
            for (int i = VisibleItems.Count - 1; i >= 0; i--)
            {
                VisibleItems[i].VisibleFlag = VisibleFlag.Hidden;
            }
            for (int i = VisibleEvents.Count - 1; i >= 0; i--)
            {
                VisibleEvents[i].VisibleFlag = VisibleFlag.Hidden;
            }
            for (int i = VisibleActors.Count - 1; i >= 0; i--)
            {
                VisibleActors[i].VisibleFlag = VisibleFlag.Hidden;
            }
            short nStartX = (short)(CurrX - ViewRange);
            short nEndX = (short)(CurrX + ViewRange);
            short nStartY = (short)(CurrY - ViewRange);
            short nEndY = (short)(CurrY + ViewRange);
            try
            {
                //todo 需要要优化整个方法
                for (short nX = nStartX; nX <= nEndX; nX++)
                {
                    for (short nY = nStartY; nY <= nEndY; nY++)
                    {
                        ref MapCellInfo cellInfo = ref Envir.GetCellInfo(nX, nY, out bool cellSuccess);
                        if (cellSuccess && cellInfo.IsAvailable)
                        {
                            var nIdx = 0;
                            while (true)
                            {
                                if (cellInfo.Count <= nIdx)
                                {
                                    break;
                                }
                                CellObject cellObject = cellInfo.ObjList[nIdx];
                                switch (cellObject.CellType)
                                {
                                    case CellType.Play:
                                    case CellType.Monster:
                                    case CellType.Merchant:
                                        //if ((HUtil32.GetTickCount() - cellObject.AddTime) >= 60 * 1000)
                                        //{
                                        //    cellInfo.Remove(nIdx);
                                        //    if (cellInfo.Count > 0)
                                        //    {
                                        //        continue;
                                        //    }
                                        //    cellInfo.Clear();
                                        //    break;
                                        //}
                                        BaseObject baseObject = M2Share.ActorMgr.Get(cellObject.CellObjId);
                                        if (baseObject != null && !baseObject.Invisible)
                                        {
                                            if (!baseObject.Ghost && !baseObject.FixedHideMode && !baseObject.ObMode)
                                            {
                                                if (Race < ActorRace.Animal || Master != null || WantRefMsg || baseObject.Master != null && Math.Abs(baseObject.CurrX - CurrX) <= 3 && Math.Abs(baseObject.CurrY - CurrY) <= 3 || baseObject.Race == ActorRace.Play)
                                                {
                                                    UpdateVisibleGay(baseObject);//更新自己的视野对象
                                                    if (baseObject.CellType == CellType.Monster && !ObMode && !FixedHideMode) //进入附近怪物视野
                                                    {
                                                        if (Math.Abs(baseObject.CurrX - CurrX) <= (ViewRange - baseObject.ViewRange) && Math.Abs(baseObject.CurrY - CurrY) <= (ViewRange - baseObject.ViewRange))
                                                        {
                                                            M2Share.ActorMgr.SendMessage(baseObject.ActorId, Messages.RM_UPDATEVIEWRANGE, this.ActorId, 0, 0, 0, "");// 发送消息更新对方的视野
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    case CellType.Item:
                                        if ((HUtil32.GetTickCount() - cellObject.AddTime) > M2Share.Config.ClearDropOnFloorItemTime)// 60 * 60 * 1000
                                        {
                                            cellInfo.Remove(nIdx);
                                            if (cellInfo.Count > 0)
                                            {
                                                continue;
                                            }
                                            cellInfo.Clear();
                                            break;
                                        }
                                        MapItem mapItem = M2Share.CellObjectMgr.Get<MapItem>(cellObject.CellObjId);
                                        if (mapItem == null)
                                        {
                                            continue;
                                        }
                                        UpdateVisibleItem(nX, nY, mapItem);
                                        if (mapItem.OfBaseObject > 0 || mapItem.DropBaseObject > 0)
                                        {
                                            if ((HUtil32.GetTickCount() - mapItem.CanPickUpTick) > M2Share.Config.FloorItemCanPickUpTime)// 2 * 60 * 1000
                                            {
                                                mapItem.OfBaseObject = 0;
                                                mapItem.DropBaseObject = 0;
                                            }
                                            else
                                            {
                                                if (M2Share.ActorMgr.Get(mapItem.OfBaseObject) != null)
                                                {
                                                    if (M2Share.ActorMgr.Get(mapItem.OfBaseObject).Ghost)
                                                    {
                                                        mapItem.OfBaseObject = 0;
                                                    }
                                                }
                                                if (M2Share.ActorMgr.Get(mapItem.DropBaseObject) != null)
                                                {
                                                    if (M2Share.ActorMgr.Get(mapItem.DropBaseObject).Ghost)
                                                    {
                                                        mapItem.DropBaseObject = 0;
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    case CellType.Event:
                                        MapEvent mapEvent = M2Share.CellObjectMgr.Get<MapEvent>(cellObject.CellObjId);
                                        if (mapEvent == null)
                                        {
                                            continue;
                                        }
                                        if (mapEvent.Visible)
                                        {
                                            UpdateVisibleEvent(nX, nY, mapEvent);
                                        }
                                        break;
                                }
                                nIdx++;
                            }
                        }
                    }
                }
                int n18 = 0;
                while (true)
                {
                    if (VisibleActors.Count <= n18)
                    {
                        break;
                    }
                    VisibleBaseObject visibleBaseObject = VisibleActors[n18];
                    if (visibleBaseObject.VisibleFlag == VisibleFlag.Hidden)
                    {
                        BaseObject baseObject = visibleBaseObject.BaseObject;
                        if (!baseObject.FixedHideMode && !baseObject.Ghost)//防止人物退出时发送重复的消息占用带宽，人物进入隐身模式时人物不消失问题
                        {
                            SendMsg(baseObject, Messages.RM_DISAPPEAR, 0, 0, 0, 0);
                        }
                        VisibleActors.RemoveAt(n18);
                        Dispose(visibleBaseObject);
                        continue;
                    }
                    if (visibleBaseObject.VisibleFlag == VisibleFlag.Show)
                    {
                        BaseObject baseObject = visibleBaseObject.BaseObject;
                        if (baseObject != this)
                        {
                            if (baseObject.Death)
                            {
                                if (baseObject.Skeleton)
                                {
                                    SendMsg(baseObject, Messages.RM_SKELETON, baseObject.Dir, baseObject.CurrX, baseObject.CurrY, 0);
                                }
                                else
                                {
                                    SendMsg(baseObject, Messages.RM_DEATH, baseObject.Dir, baseObject.CurrX, baseObject.CurrY, 0);
                                }
                            }
                            else
                            {
                                SendMsg(baseObject, Messages.RM_TURN, baseObject.Dir, baseObject.CurrX, baseObject.CurrY, 0, baseObject.GetShowName());
                            }
                        }
                    }
                    n18++;
                }

                int I = 0;
                while (true)
                {
                    if (VisibleItems.Count <= I)
                    {
                        break;
                    }
                    VisibleMapItem visibleMapItem = VisibleItems[I];
                    if (visibleMapItem.VisibleFlag == VisibleFlag.Hidden)
                    {
                        SendMsg(Messages.RM_ITEMHIDE, 0, visibleMapItem.MapItem.ItemId, visibleMapItem.nX, visibleMapItem.nY);
                        VisibleItems.RemoveAt(I);
                        Dispose(visibleMapItem);
                        continue;
                    }
                    if (visibleMapItem.VisibleFlag == VisibleFlag.Show)
                    {
                        SendMsg(Messages.RM_ITEMSHOW, visibleMapItem.wLooks, visibleMapItem.MapItem.ItemId, visibleMapItem.nX, visibleMapItem.nY, visibleMapItem.sName);
                    }
                    I++;
                }
                I = 0;
                while (true)
                {
                    if (VisibleEvents.Count <= I)
                    {
                        break;
                    }
                    MapEvent mapEvent = VisibleEvents[I];
                    if (mapEvent.VisibleFlag == VisibleFlag.Hidden)
                    {
                        SendMsg(Messages.RM_HIDEEVENT, 0, mapEvent.Id, mapEvent.nX, mapEvent.nY);
                        VisibleEvents.RemoveAt(I);
                        continue;
                    }
                    if (mapEvent.VisibleFlag == VisibleFlag.Show)
                    {
                        SendMsg(Messages.RM_SHOWEVENT, (short)mapEvent.EventType, mapEvent.Id, HUtil32.MakeLong(mapEvent.nX, (short)mapEvent.EventParam), mapEvent.nY);
                    }
                    I++;
                }
            }
            catch (Exception e)
            {
                M2Share.Logger.Error(e.StackTrace);
                KickException();
            }
        }

        /// <summary>
        /// 显示玩家名字
        /// </summary>
        /// <returns></returns>
        public override string GetShowName()
        {
            string result = string.Empty;
            string sChrName = string.Empty;
            string sGuildName = string.Empty;
            string sDearName = string.Empty;
            string sMasterName = string.Empty;
            const string sExceptionMsg = "[Exception] PlayObject::GetShowName";
            try
            {
                if (MyGuild != null)
                {
                    UserCastle castle = M2Share.CastleMgr.IsCastleMember(this);
                    if (castle != null)
                    {
                        sGuildName = Settings.CastleGuildName.Replace("%castlename", castle.sName);
                        sGuildName = sGuildName.Replace("%guildname", MyGuild.GuildName);
                        sGuildName = sGuildName.Replace("%rankname", GuildRankName);
                    }
                    else
                    {
                        castle = M2Share.CastleMgr.InCastleWarArea(this);// 01/25 多城堡
                        if (M2Share.Config.ShowGuildName || castle != null && castle.UnderWar || InGuildWarArea)
                        {
                            sGuildName = Settings.NoCastleGuildName.Replace("%guildname", MyGuild.GuildName);
                            sGuildName = sGuildName.Replace("%rankname", GuildRankName);
                        }
                    }
                }
                if (!M2Share.Config.ShowRankLevelName)
                {
                    if (ReLevel > 0)
                    {
                        switch (Job)
                        {
                            case PlayJob.Warrior:
                                sChrName = Settings.WarrReNewName.Replace("%chrname", ChrName);
                                break;
                            case PlayJob.Wizard:
                                sChrName = Settings.WizardReNewName.Replace("%chrname", ChrName);
                                break;
                            case PlayJob.Taoist:
                                sChrName = Settings.TaosReNewName.Replace("%chrname", ChrName);
                                break;
                        }
                    }
                    else
                    {
                        sChrName = ChrName;
                    }
                }
                else
                {
                    sChrName = Format(RankLevelName, ChrName);
                }
                if (!string.IsNullOrEmpty(MasterName))
                {
                    if (IsMaster)
                    {
                        sMasterName = Format(Settings.MasterName, MasterName);
                    }
                    else
                    {
                        sMasterName = Format(Settings.NoMasterName, MasterName);
                    }
                }
                if (!string.IsNullOrEmpty(DearName))
                {
                    if (Gender == PlayGender.Man)
                    {
                        sDearName = Format(Settings.ManDearName, DearName);
                    }
                    else
                    {
                        sDearName = Format(Settings.WoManDearName, DearName);
                    }
                }
                string sShowName = Settings.HumanShowName.Replace("%chrname", sChrName);
                sShowName = sShowName.Replace("%guildname", sGuildName);
                sShowName = sShowName.Replace("%dearname", sDearName);
                sShowName = sShowName.Replace("%mastername", sMasterName);
                result = sShowName;
            }
            catch (Exception e)
            {
                M2Share.Logger.Error(sExceptionMsg);
                M2Share.Logger.Error(e.Message);
            }
            return result;
        }

        /// <summary>
        /// 计算角色外形代码
        /// </summary>
        /// <returns></returns>
        public override int GetFeature(BaseObject baseObject)
        {
            byte nDress = 0;
            StdItem stdItem;
            if (UseItems[ItemLocation.Dress] != null && UseItems[ItemLocation.Dress].Index > 0) // 衣服
            {
                stdItem = M2Share.WorldEngine.GetStdItem(UseItems[ItemLocation.Dress].Index);
                if (stdItem != null)
                {
                    nDress = (byte)(stdItem.Shape * 2);
                }
            }
            PlayGender playGender = Gender;
            nDress += (byte)playGender;
            byte nWeapon = (byte)playGender;
            if (UseItems[ItemLocation.Weapon] != null && UseItems[ItemLocation.Weapon].Index > 0) // 武器
            {
                stdItem = M2Share.WorldEngine.GetStdItem(UseItems[ItemLocation.Weapon].Index);
                if (stdItem != null)
                {
                    nWeapon += (byte)(stdItem.Shape * 2);
                }
            }
            byte nHair = (byte)(Hair * 2 + (byte)playGender);
            return M2Share.MakeHumanFeature(0, nDress, nWeapon, nHair);
        }

        public override void MakeGhost()
        {
            const string sExceptionMsg = "[Exception] PlayObject::MakeGhost";
            try
            {
                if (M2Share.HighLevelHuman == ActorId)
                {
                    M2Share.HighLevelHuman = 0;
                }
                if (M2Share.HighPKPointHuman == ActorId)
                {
                    M2Share.HighPKPointHuman = 0;
                }
                if (M2Share.HighDCHuman == ActorId)
                {
                    M2Share.HighDCHuman = 0;
                }
                if (M2Share.HighMCHuman == ActorId)
                {
                    M2Share.HighMCHuman = 0;
                }
                if (M2Share.HighSCHuman == ActorId)
                {
                    M2Share.HighSCHuman = 0;
                }
                if (M2Share.HighOnlineHuman == ActorId)
                {
                    M2Share.HighOnlineHuman = 0;
                }
                // 人物下线后通知配偶，并把对方的相关记录清空
                string sSayMsg;
                if (DearHuman != null)
                {
                    if (Gender == PlayGender.Man)
                    {
                        sSayMsg = Settings.ManLongOutDearOnlineMsg.Replace("%d", DearName);
                        sSayMsg = sSayMsg.Replace("%s", ChrName);
                        sSayMsg = sSayMsg.Replace("%m", Envir.MapDesc);
                        sSayMsg = sSayMsg.Replace("%x", CurrX.ToString());
                        sSayMsg = sSayMsg.Replace("%y", CurrY.ToString());
                        DearHuman.SysMsg(sSayMsg, MsgColor.Red, MsgType.Hint);
                    }
                    else
                    {
                        sSayMsg = Settings.WoManLongOutDearOnlineMsg.Replace("%d", DearName);
                        sSayMsg = sSayMsg.Replace("%s", ChrName);
                        sSayMsg = sSayMsg.Replace("%m", Envir.MapDesc);
                        sSayMsg = sSayMsg.Replace("%x", CurrX.ToString());
                        sSayMsg = sSayMsg.Replace("%y", CurrY.ToString());
                        DearHuman.SysMsg(sSayMsg, MsgColor.Red, MsgType.Hint);
                    }
                    DearHuman.DearHuman = null;
                    DearHuman = null;
                }
                if (MasterHuman != null || MasterList.Count > 0)
                {
                    if (IsMaster)
                    {
                        for (int i = MasterList.Count - 1; i >= 0; i--)
                        {
                            PlayObject human = MasterList[i];
                            sSayMsg = Settings.MasterLongOutMasterListOnlineMsg.Replace("%s", ChrName);
                            sSayMsg = sSayMsg.Replace("%m", Envir.MapDesc);
                            sSayMsg = sSayMsg.Replace("%x", CurrX.ToString());
                            sSayMsg = sSayMsg.Replace("%y", CurrY.ToString());
                            human.SysMsg(sSayMsg, MsgColor.Red, MsgType.Hint);
                            human.MasterHuman = null;
                        }
                    }
                    else
                    {
                        if (MasterHuman == null)
                        {
                            return;
                        }
                        sSayMsg = Settings.MasterListLongOutMasterOnlineMsg.Replace("%d", MasterName);
                        sSayMsg = sSayMsg.Replace("%s", ChrName);
                        sSayMsg = sSayMsg.Replace("%m", Envir.MapDesc);
                        sSayMsg = sSayMsg.Replace("%x", CurrX.ToString());
                        sSayMsg = sSayMsg.Replace("%y", CurrY.ToString());
                        MasterHuman.SysMsg(sSayMsg, MsgColor.Red, MsgType.Hint);
                        // 如果为大徒弟则将对方的记录清空
                        if (MasterHuman.MasterName == ChrName)
                        {
                            MasterHuman.MasterHuman = null;
                        }
                        for (int i = 0; i < MasterHuman.MasterList.Count; i++)
                        {
                            if (MasterHuman.MasterList[i] == this)
                            {
                                MasterHuman.MasterList.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                M2Share.Logger.Error(sExceptionMsg);
                M2Share.Logger.Error(e.Message);
            }
            base.MakeGhost();
            if (Ghost)
            {
                SendSelfDelayMsg(Messages.RM_MASTERDIEGHOST, 0, 0, 0, 0, "", 1000);
            }
        }

        internal override void ScatterBagItems(int itemOfCreat)
        {
            const int dropWide = 2;
            if (AngryRing || NoDropItem || Envir.Flag.NoDropItem)
            {
                return;// 不死戒指
            }
            const string sExceptionMsg = "[Exception] PlayObject::ScatterBagItems";
            try
            {
                if (ItemList.Count > 0)
                {
                    IList<DeleteItem> delList = new List<DeleteItem>();
                    bool boDropall = M2Share.Config.DieRedScatterBagAll && PvpLevel() >= 2;
                    for (int i = ItemList.Count - 1; i >= 0; i--)
                    {
                        if (boDropall || M2Share.RandomNumber.Random(M2Share.Config.DieScatterBagRate) == 0)
                        {
                            if (DropItemDown(ItemList[i], dropWide, true, itemOfCreat, ActorId))
                            {
                                delList.Add(new DeleteItem()
                                {
                                    ItemName = M2Share.WorldEngine.GetStdItemName(ItemList[i].Index),
                                    MakeIndex = ItemList[i].MakeIndex
                                });
                                Dispose(ItemList[i]);
                                ItemList.RemoveAt(i);
                            }
                        }
                    }
                    if (delList.Count > 0)
                    {
                        int objectId = HUtil32.Sequence();
                        M2Share.ActorMgr.AddOhter(objectId, delList);
                        SendMsg(Messages.RM_SENDDELITEMLIST, 0, objectId, 0, 0);
                    }
                }
            }
            catch
            {
                M2Share.Logger.Error(sExceptionMsg);
            }
        }

        protected override byte GetNameColor()
        {
            byte pvpLevel = PvpLevel();
            if (pvpLevel == 0)
            {
                return base.GetNameColor();
            }
            return pvpLevel >= 2 ? M2Share.Config.PKLevel2NameColor : M2Share.Config.PKLevel1NameColor;
        }

        protected override byte GetChrColor(BaseObject baseObject)
        {
            if (baseObject.Race == ActorRace.Play)
            {
                byte result = baseObject.NameColor;
                PlayObject targetObject = (PlayObject)baseObject;
                if (targetObject.PvpLevel() < 2)
                {
                    if (targetObject.PvpFlag)
                    {
                        result = M2Share.Config.PKFlagNameColor;
                    }
                    int n10 = GetGuildRelation(this, targetObject);
                    switch (n10)
                    {
                        case 1:
                        case 3:
                            result = M2Share.Config.AllyAndGuildNameColor;
                            break;
                        case 2:
                            result = M2Share.Config.WarGuildNameColor;
                            break;
                    }
                    if (targetObject.Envir.Flag.Fight3Zone)
                    {
                        result = MyGuild == targetObject.MyGuild ? M2Share.Config.AllyAndGuildNameColor : M2Share.Config.WarGuildNameColor;
                    }
                }
                UserCastle castle = M2Share.CastleMgr.InCastleWarArea(targetObject);
                if ((castle != null) && castle.UnderWar && InGuildWarArea && targetObject.InGuildWarArea)
                {
                    result = M2Share.Config.InFreePKAreaNameColor;
                    GuildWarArea = true;
                    if (MyGuild == null)
                    {
                        return result;
                    }
                    if (castle.IsMasterGuild(MyGuild))
                    {
                        if ((MyGuild == targetObject.MyGuild) || MyGuild.IsAllyGuild(targetObject.MyGuild))
                        {
                            result = M2Share.Config.AllyAndGuildNameColor;
                        }
                        else
                        {
                            if (castle.IsAttackGuild(targetObject.MyGuild))
                            {
                                result = M2Share.Config.WarGuildNameColor;
                            }
                        }
                    }
                    else
                    {
                        if (castle.IsAttackGuild(MyGuild))
                        {
                            if ((MyGuild == targetObject.MyGuild) || MyGuild.IsAllyGuild(targetObject.MyGuild))
                            {
                                result = M2Share.Config.AllyAndGuildNameColor;
                            }
                            else
                            {
                                if (castle.IsMember(targetObject))
                                {
                                    result = M2Share.Config.WarGuildNameColor;
                                }
                            }
                        }
                    }
                }
                return result;
            }
            return base.GetChrColor(baseObject);
        }

        protected override void RecalcHitSpeed()
        {
            HitPlus = 0;
            HitDouble = 0;
            NakedAbility bonusTick;
            switch (Job)
            {
                case PlayJob.Warrior:
                    bonusTick = M2Share.Config.BonusAbilofWarr;
                    HitPoint = (byte)(Settings.DEFHIT + BonusAbil.Hit / bonusTick.Hit);
                    SpeedPoint = (byte)(Settings.DEFSPEED + BonusAbil.Speed / bonusTick.Speed);
                    break;
                case PlayJob.Wizard:
                    bonusTick = M2Share.Config.BonusAbilofWizard;
                    HitPoint = (byte)(Settings.DEFHIT + BonusAbil.Hit / bonusTick.Hit);
                    SpeedPoint = (byte)(Settings.DEFSPEED + BonusAbil.Speed / bonusTick.Speed);
                    break;
                case PlayJob.Taoist:
                    bonusTick = M2Share.Config.BonusAbilofTaos;
                    SpeedPoint = (byte)(Settings.DEFSPEED + BonusAbil.Speed / bonusTick.Speed + 3);
                    break;
            }
            for (int i = 0; i < MagicList.Count; i++)
            {
                UserMagic userMagic = MagicList[i];
                MagicArr[userMagic.MagIdx] = userMagic;
                switch (userMagic.MagIdx)
                {
                    case MagicConst.SKILL_ONESWORD: // 基本剑法
                        if (userMagic.Level > 0)
                        {
                            HitPoint = (byte)(HitPoint + HUtil32.Round(9 / 3.0 * userMagic.Level));
                        }
                        break;
                    case MagicConst.SKILL_ILKWANG: // 精神力战法
                        if (userMagic.Level > 0)
                        {
                            HitPoint = (byte)(HitPoint + HUtil32.Round(8 / 3.0 * userMagic.Level));
                        }
                        break;
                    case MagicConst.SKILL_YEDO: // 攻杀剑法
                        if (userMagic.Level > 0)
                        {
                            HitPoint = (byte)(HitPoint + HUtil32.Round(3 / 3.0 * userMagic.Level));
                        }
                        HitPlus = (byte)(Settings.DEFHIT + userMagic.Level);
                        AttackSkillCount = (byte)(7 - userMagic.Level);
                        AttackSkillPointCount = M2Share.RandomNumber.RandomByte(AttackSkillCount);
                        break;
                    case MagicConst.SKILL_FIRESWORD: // 烈火剑法
                        HitDouble = (byte)(4 + userMagic.Level * 4);
                        break;
                }
            }
        }

        /// <summary>
        /// 切换地图
        /// </summary>
        internal bool EnterAnotherMap(Envirnoment envir, short nDMapX, short nDMapY)
        {
            bool result = false;
            const string sExceptionMsg = "[Exception] BaseObject::EnterAnotherMap";
            try
            {
                if (Abil.Level < envir.EnterLevel)
                {
                    SysMsg($"需要 {envir.Flag.RequestLevel - 1} 级以上才能进入 {envir.MapDesc}", MsgColor.Red, MsgType.Hint);
                    return false;
                }
                if (envir.QuestNpc != null)
                {
                    envir.QuestNpc.Click(this);
                }
                if (envir.Flag.NeedSetonFlag >= 0)
                {
                    if (GetQuestFalgStatus(envir.Flag.NeedSetonFlag) != envir.Flag.NeedOnOff)
                    {
                        return false;
                    }
                }
                if (!envir.CellValid(nDMapX, nDMapY))
                {
                    return false;
                }
                UserCastle castle = M2Share.CastleMgr.IsCastlePalaceEnvir(envir);
                if ((castle != null))
                {
                    if (!castle.CheckInPalace(CurrX, CurrY))
                    {
                        return false;
                    }
                }
                if (envir.Flag.NoHorse)
                {
                    OnHorse = false;
                }
                Envirnoment oldEnvir = Envir;
                short nOldX = CurrX;
                short nOldY = CurrY;
                DisappearA();
                VisibleHumanList.Clear();
                for (int i = 0; i < VisibleItems.Count; i++)
                {
                    VisibleItems[i] = null;
                }
                VisibleItems.Clear();
                VisibleEvents.Clear();
                for (int i = 0; i < VisibleActors.Count; i++)
                {
                    VisibleActors[i] = null;
                }
                VisibleActors.Clear();
                SendMsg(Messages.RM_CLEAROBJECTS, 0, 0, 0, 0);
                Envir = envir;
                MapName = envir.MapName;
                MapFileName = envir.MapFileName;
                CurrX = nDMapX;
                CurrY = nDMapY;
                SendMsg(Messages.RM_CHANGEMAP, 0, 0, 0, 0, envir.MapFileName);
                if (AddToMap())
                {
                    //MapMoveTick = HUtil32.GetTickCount();
                    SpaceMoved = true;
                    result = true;
                }
                else
                {
                    Envir = oldEnvir;
                    CurrX = nOldX;
                    CurrY = nOldY;
                    Envir.AddToMap(CurrX, CurrY, CellType, this.ActorId, this);
                }
                OnEnvirnomentChanged();
                // 复位泡点，及金币，时间
                IncGamePointTick = HUtil32.GetTickCount();
                IncGameGoldTick = HUtil32.GetTickCount();
                AutoGetExpTick = HUtil32.GetTickCount();
                if (Envir.Flag.Fight3Zone && (Envir.Flag.Fight3Zone != oldEnvir.Flag.Fight3Zone))
                {
                    RefShowName();
                }
            }
            catch
            {
                M2Share.Logger.Error(sExceptionMsg);
            }
            return result;
        }

        private void WinLottery()
        {
            int nGold = 0;
            int nWinLevel = 0;
            int nRate = M2Share.RandomNumber.Random(M2Share.Config.WinLotteryRate);
            if (nRate >= M2Share.Config.WinLottery6Min && nRate <= M2Share.Config.WinLottery6Max)
            {
                if (M2Share.Config.WinLotteryCount < M2Share.Config.NoWinLotteryCount)
                {
                    nGold = M2Share.Config.WinLottery6Gold;
                    nWinLevel = 6;
                    M2Share.Config.WinLotteryLevel6++;
                }
            }
            else if (nRate >= M2Share.Config.WinLottery5Min && nRate <= M2Share.Config.WinLottery5Max)
            {
                if (M2Share.Config.WinLotteryCount < M2Share.Config.NoWinLotteryCount)
                {
                    nGold = M2Share.Config.WinLottery5Gold;
                    nWinLevel = 5;
                    M2Share.Config.WinLotteryLevel5++;
                }
            }
            else if (nRate >= M2Share.Config.WinLottery4Min && nRate <= M2Share.Config.WinLottery4Max)
            {
                if (M2Share.Config.WinLotteryCount < M2Share.Config.NoWinLotteryCount)
                {
                    nGold = M2Share.Config.WinLottery4Gold;
                    nWinLevel = 4;
                    M2Share.Config.WinLotteryLevel4++;
                }
            }
            else if (nRate >= M2Share.Config.WinLottery3Min && nRate <= M2Share.Config.WinLottery3Max)
            {
                if (M2Share.Config.WinLotteryCount < M2Share.Config.NoWinLotteryCount)
                {
                    nGold = M2Share.Config.WinLottery3Gold;
                    nWinLevel = 3;
                    M2Share.Config.WinLotteryLevel3++;
                }
            }
            else if (nRate >= M2Share.Config.WinLottery2Min && nRate <= M2Share.Config.WinLottery2Max)
            {
                if (M2Share.Config.WinLotteryCount < M2Share.Config.NoWinLotteryCount)
                {
                    nGold = M2Share.Config.WinLottery2Gold;
                    nWinLevel = 2;
                    M2Share.Config.WinLotteryLevel2++;
                }
            }
            else if (M2Share.Config.WinLottery1Min + M2Share.Config.WinLottery1Max == nRate)
            {
                if (M2Share.Config.WinLotteryCount < M2Share.Config.NoWinLotteryCount)
                {
                    nGold = M2Share.Config.WinLottery1Gold;
                    nWinLevel = 1;
                    M2Share.Config.WinLotteryLevel1++;
                }
            }
            if (nGold > 0)
            {
                switch (nWinLevel)
                {
                    case 1:
                        SysMsg(Settings.WinLottery1Msg, MsgColor.Green, MsgType.Hint);
                        break;
                    case 2:
                        SysMsg(Settings.WinLottery2Msg, MsgColor.Green, MsgType.Hint);
                        break;
                    case 3:
                        SysMsg(Settings.WinLottery3Msg, MsgColor.Green, MsgType.Hint);
                        break;
                    case 4:
                        SysMsg(Settings.WinLottery4Msg, MsgColor.Green, MsgType.Hint);
                        break;
                    case 5:
                        SysMsg(Settings.WinLottery5Msg, MsgColor.Green, MsgType.Hint);
                        break;
                    case 6:
                        SysMsg(Settings.WinLottery6Msg, MsgColor.Green, MsgType.Hint);
                        break;
                }
                if (IncGold(nGold))
                {
                    GoldChanged();
                }
                else
                {
                    DropGoldDown(nGold, true, 0, 0);
                }
            }
            else
            {
                M2Share.Config.NoWinLotteryCount += 500;
                SysMsg(Settings.NotWinLotteryMsg, MsgColor.Red, MsgType.Hint);
            }
        }

        internal void AddItemSkill(int nIndex)
        {
            MagicInfo magic = null;
            switch (nIndex)
            {
                case 1:
                    magic = M2Share.WorldEngine.FindMagic(M2Share.Config.FireBallSkill);
                    break;
                case 2:
                    magic = M2Share.WorldEngine.FindMagic(M2Share.Config.HealSkill);
                    break;
            }
            if (magic != null)
            {
                if (!IsTrainingSkill(magic.MagicId))
                {
                    UserMagic userMagic = new UserMagic
                    {
                        Magic = magic,
                        MagIdx = magic.MagicId,
                        Key = (char)0,
                        Level = 1,
                        TranPoint = 0
                    };
                    MagicList.Add(userMagic);
                    SendAddMagic(userMagic);
                }
            }
        }

        public void SetExpiredTime(int expiredTime)
        {
            if (Abil.Level > Settings.ExpErienceLevel)
            {
                ExpireTime = HUtil32.GetTickCount() + (60 * 1000);
                ExpireCount = expiredTime;
            }
        }

        private void CheckExpiredTime()
        {
            ExpireCount--;
            switch (ExpireCount)
            {
                case 30:
                    SysMsg("您的账号游戏时间即将到期，您将在[30:00]分钟后断开服务器。", MsgColor.Blue, MsgType.System);
                    break;
                case > 0 and <= 10:
                    SysMsg($"您的账号游戏时间即将到期，您将在[{ExpireCount}:00]分钟后断开服务器。", MsgColor.Blue, MsgType.System);
                    break;
                case <= 0:
                    ExpireTime = 0;
                    ExpireCount = 0;
                    AccountExpired = true;
                    SysMsg("您的账号游戏时间已到期，访问(https://mir2.sdo.com)购买充值，所有游戏大区均可账号共享游戏时间。", MsgColor.Blue, MsgType.System);
                    break;
            }
        }

        public int GetQuestFalgStatus(int nFlag)
        {
            int result = 0;
            nFlag -= 1;
            if (nFlag < 0)
            {
                return result;
            }
            int n10 = nFlag / 8;
            int n14 = nFlag % 8;
            if ((n10 - QuestFlag.Length) < 0)
            {
                if (((128 >> n14) & QuestFlag[n10]) != 0)
                {
                    result = 1;
                }
                else
                {
                    result = 0;
                }
            }
            return result;
        }

        public void SetQuestFlagStatus(int nFlag, int nValue)
        {
            nFlag -= 1;
            if (nFlag < 0)
            {
                return;
            }
            int n10 = nFlag / 8;
            int n14 = nFlag % 8;
            if ((n10 - QuestFlag.Length) < 0)
            {
                byte bt15 = QuestFlag[n10];
                if (nValue == 0)
                {
                    QuestFlag[n10] = (byte)((~(128 >> n14)) & bt15);
                }
                else
                {
                    QuestFlag[n10] = (byte)((128 >> n14) | bt15);
                }
            }
        }

        public int GetQuestUnitOpenStatus(int nFlag)
        {
            int result = 0;
            nFlag -= 1;
            if (nFlag < 0)
            {
                return result;
            }
            int n10 = nFlag / 8;
            int n14 = nFlag % 8;
            if ((n10 - QuestUnitOpen.Length) < 0)
            {
                if (((128 >> n14) & QuestUnitOpen[n10]) != 0)
                {
                    result = 1;
                }
                else
                {
                    result = 0;
                }
            }
            return result;
        }

        public void SetQuestUnitOpenStatus(int nFlag, int nValue)
        {
            nFlag -= 1;
            if (nFlag < 0)
            {
                return;
            }
            int n10 = nFlag / 8;
            int n14 = nFlag % 8;
            if ((n10 - QuestUnitOpen.Length) < 0)
            {
                byte bt15 = QuestUnitOpen[n10];
                if (nValue == 0)
                {
                    QuestUnitOpen[n10] = (byte)((~(128 >> n14)) & bt15);
                }
                else
                {
                    QuestUnitOpen[n10] = (byte)((128 >> n14) | bt15);
                }
            }
        }

        public int GetQuestUnitStatus(int nFlag)
        {
            int result = 0;
            nFlag -= 1;
            if (nFlag < 0)
            {
                return result;
            }
            int n10 = nFlag / 8;
            int n14 = nFlag % 8;
            if ((n10 - QuestUnit.Length) < 0)
            {
                if (((128 >> n14) & QuestUnit[n10]) != 0)
                {
                    result = 1;
                }
                else
                {
                    result = 0;
                }
            }
            return result;
        }

        public void SetQuestUnitStatus(int nFlag, int nValue)
        {
            nFlag -= 1;
            if (nFlag < 0)
            {
                return;
            }
            int n10 = nFlag / 8;
            int n14 = nFlag % 8;
            if ((n10 - QuestUnit.Length) < 0)
            {
                byte bt15 = QuestUnit[n10];
                if (nValue == 0)
                {
                    QuestUnit[n10] = (byte)((~(128 >> n14)) & bt15);
                }
                else
                {
                    QuestUnit[n10] = (byte)((128 >> n14) | bt15);
                }
            }
        }

        /// <summary>
        /// 检查武器是否升级
        /// </summary>
        private void CheckWeaponUpgrade()
        {
            if (UseItems[ItemLocation.Weapon] != null && UseItems[ItemLocation.Weapon].Desc[ItemAttr.WeaponUpgrade] > 0)
            {
                UserItem useItems = new UserItem(UseItems[ItemLocation.Weapon]);
                CheckWeaponUpgradeStatus(ref UseItems[ItemLocation.Weapon]);
                StdItem StdItem = M2Share.WorldEngine.GetStdItem(useItems.Index);
                if (UseItems[ItemLocation.Weapon].Index == 0)
                {
                    SysMsg(Settings.TheWeaponBroke, MsgColor.Red, MsgType.Hint);
                    SendDelItems(useItems);
                    SendRefMsg(Messages.RM_BREAKWEAPON, 0, 0, 0, 0, "");
                    if (StdItem?.NeedIdentify == 1)
                    {
                        M2Share.EventSource.AddEventLog(21, MapName + "\t" + CurrX + "\t" + CurrY + "\t" + ChrName + "\t" + StdItem?.Name + "\t" + useItems.MakeIndex + "\t" + '1' + "\t" + '0');
                    }
                    FeatureChanged();
                }
                else
                {
                    SysMsg(Settings.TheWeaponRefineSuccessfull, MsgColor.Red, MsgType.Hint);
                    SendUpdateItem(UseItems[ItemLocation.Weapon]);
                    if (StdItem.NeedIdentify == 1)
                    {
                        M2Share.EventSource.AddEventLog(20, MapName + "\t" + CurrX + "\t" + CurrY + "\t" + ChrName + "\t" + StdItem.Name + "\t" + useItems.MakeIndex + "\t" + '1' + "\t" + '0');
                    }
                    RecalcAbilitys();
                    SendMsg(Messages.RM_ABILITY, 0, 0, 0, 0);
                    SendMsg(Messages.RM_SUBABILITY, 0, 0, 0, 0);
                }
            }
        }

        /// <summary>
        /// 检查武器升级状态
        /// </summary>
        private static void CheckWeaponUpgradeStatus(ref UserItem userItem)
        {
            if ((userItem.Desc[0] + userItem.Desc[1] + userItem.Desc[2]) < M2Share.Config.UpgradeWeaponMaxPoint)
            {
                if (userItem.Desc[ItemAttr.WeaponUpgrade] == 1)
                {
                    userItem.Index = 0;
                }
                if (HUtil32.RangeInDefined(userItem.Desc[ItemAttr.WeaponUpgrade], 10, 13))
                {
                    userItem.Desc[0] = (byte)(userItem.Desc[0] + userItem.Desc[ItemAttr.WeaponUpgrade] - 9);
                }
                if (HUtil32.RangeInDefined(userItem.Desc[ItemAttr.WeaponUpgrade], 20, 23))
                {
                    userItem.Desc[1] = (byte)(userItem.Desc[1] + userItem.Desc[ItemAttr.WeaponUpgrade] - 19);
                }
                if (HUtil32.RangeInDefined(userItem.Desc[ItemAttr.WeaponUpgrade], 30, 33))
                {
                    userItem.Desc[2] = (byte)(userItem.Desc[2] + userItem.Desc[ItemAttr.WeaponUpgrade] - 29);
                }
            }
            else
            {
                userItem.Index = 0;
            }
            userItem.Desc[ItemAttr.WeaponUpgrade] = 0;
        }

        private UserMagic GetMagicInfo(int nMagicId)
        {
            for (int i = 0; i < MagicList.Count; i++)
            {
                UserMagic userMagic = MagicList[i];
                if (userMagic.Magic.MagicId == nMagicId)
                {
                    return userMagic;
                }
            }
            return null;
        }

        private bool IsProperIsFriend(BaseObject cret)
        {
            bool result = false;
            if (cret.Race == ActorRace.Play)
            {
                switch (AttatckMode)
                {
                    case AttackMode.HAM_ALL:
                        result = true;
                        break;
                    case AttackMode.HAM_PEACE:
                        result = true;
                        break;
                    case AttackMode.HAM_DEAR:
                        if ((this == cret) || (cret == DearHuman))
                        {
                            result = true;
                        }
                        break;
                    case AttackMode.HAM_MASTER:
                        if (this == cret)
                        {
                            result = true;
                        }
                        else if (IsMaster)
                        {
                            for (int i = 0; i < MasterList.Count; i++)
                            {
                                if (MasterList[i] == cret)
                                {
                                    result = true;
                                    break;
                                }
                            }
                        }
                        else if (((PlayObject)cret).IsMaster)
                        {
                            for (int i = 0; i < ((PlayObject)cret).MasterList.Count; i++)
                            {
                                if (((PlayObject)cret).MasterList[i] == this)
                                {
                                    result = true;
                                    break;
                                }
                            }
                        }
                        break;
                    case AttackMode.HAM_GROUP:
                        if (cret == this)
                        {
                            result = true;
                        }
                        if (IsGroupMember(cret))
                        {
                            result = true;
                        }
                        break;
                    case AttackMode.HAM_GUILD:
                        if (cret == this)
                        {
                            result = true;
                        }
                        if (MyGuild != null)
                        {
                            if (MyGuild.IsMember(cret.ChrName))
                            {
                                result = true;
                            }
                            if (GuildWarArea && (((PlayObject)cret).MyGuild != null))
                            {
                                if (MyGuild.IsAllyGuild(((PlayObject)cret).MyGuild))
                                {
                                    result = true;
                                }
                            }
                        }
                        break;
                    case AttackMode.HAM_PKATTACK:
                        if (cret == this)
                        {
                            result = true;
                        }
                        if (PvpLevel() >= 2)
                        {
                            if (((PlayObject)cret).PvpLevel() < 2)
                            {
                                result = true;
                            }
                        }
                        else
                        {
                            if (((PlayObject)cret).PvpLevel() >= 2)
                            {
                                result = true;
                            }
                        }
                        break;
                }
            }
            return result;
        }

        internal void AddBodyLuck(double dLuck)
        {
            if ((dLuck > 0) && (BodyLuck < 5 * Settings.BODYLUCKUNIT))
            {
                BodyLuck = BodyLuck + dLuck;
            }
            if ((dLuck < 0) && (BodyLuck > -(5 * Settings.BODYLUCKUNIT)))
            {
                BodyLuck = BodyLuck + dLuck;
            }
            int n = Convert.ToInt32(BodyLuck / Settings.BODYLUCKUNIT);
            if (n > 5)
            {
                n = 5;
            }
            if (n < -10)
            {
                n = -10;
            }
            BodyLuckLevel = n;
        }

        public void SetPkFlag(BaseObject baseObject)
        {
            if (baseObject.Race == ActorRace.Play)
            {
                PlayObject targetObject = (PlayObject)baseObject;
                if ((PvpLevel() < 2) && (targetObject.PvpLevel() < 2) && (!Envir.Flag.FightZone) && (!Envir.Flag.Fight3Zone) && !PvpFlag)
                {
                    targetObject.PvpNameColorTick = HUtil32.GetTickCount();
                    if (!targetObject.PvpFlag)
                    {
                        targetObject.PvpFlag = true;
                        targetObject.RefNameColor();
                    }
                }
            }
        }

        public void ChangePkStatus(bool boWarFlag)
        {
            if (InGuildWarArea != boWarFlag)
            {
                InGuildWarArea = boWarFlag;
                NameColorChanged = true;
            }
        }

        public byte PvpLevel()
        {
            return (byte)(PkPoint / 100);
        }

        internal void CheckPkStatus()
        {
            if (PvpFlag && ((HUtil32.GetTickCount() - PvpNameColorTick) > M2Share.Config.dwPKFlagTime)) // 60 * 1000
            {
                PvpFlag = false;
                RefNameColor();
            }
        }

        public void IncPkPoint(int nPoint)
        {
            byte oldPvpLevel = PvpLevel();
            PkPoint += nPoint;
            if (PvpLevel() != oldPvpLevel)
            {
                if (PvpLevel() <= 2)
                {
                    RefNameColor();
                }
            }
        }

        private void DecPkPoint(int nPoint)
        {
            byte pvpLevel = PvpLevel();
            PkPoint -= nPoint;
            if (PkPoint < 0)
            {
                PkPoint = 0;
            }
            if ((PvpLevel() != pvpLevel) && (pvpLevel > 0) && (pvpLevel <= 2))
            {
                RefNameColor();
            }
        }

        public void TrainSkill(UserMagic userMagic, int nTranPoint)
        {
            if (FastTrain)
            {
                nTranPoint = nTranPoint * 3;
            }
            userMagic.TranPoint += nTranPoint;
        }

        public bool IsGuildMaster()
        {
            return (MyGuild != null) && (GuildRankNo == 1);
        }

        internal void ApplyItemParameters(UserItem uitem, StdItem item, ref AddAbility aabil)
        {
            if (item != null)
            {
                ClientItem clientItem = new ClientItem();
                ItemSystem.GetUpgradeStdItem(item, uitem, ref clientItem);
                ApplyItemParametersByJob(uitem, ref clientItem);
                switch (item.StdMode)
                {
                    case 5:
                    case 6:
                        aabil.HIT = (ushort)(aabil.HIT + HUtil32.HiByte(clientItem.Item.AC));
                        aabil.HitSpeed = (ushort)(aabil.HitSpeed + ItemSystem.RealAttackSpeed(HUtil32.HiByte(clientItem.Item.MAC)));
                        aabil.Luck = (byte)(aabil.Luck + HUtil32.LoByte(clientItem.Item.AC));
                        aabil.UnLuck = (byte)(aabil.UnLuck + HUtil32.LoByte(clientItem.Item.MAC));
                        aabil.Slowdown = (byte)(aabil.Slowdown + clientItem.Item.Slowdown);
                        aabil.Poison = (byte)(aabil.Poison + clientItem.Item.Tox);
                        if (clientItem.Item.SpecialPwr >= 1 && clientItem.Item.SpecialPwr <= 10)
                        {
                            aabil.WeaponStrong = (byte)clientItem.Item.SpecialPwr;
                        }
                        break;
                    case 10:
                    case 11:
                        aabil.AC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(aabil.AC) + HUtil32.LoByte(clientItem.Item.AC)), (ushort)(HUtil32.HiByte(aabil.AC) + HUtil32.HiByte(clientItem.Item.AC)));
                        aabil.MAC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(aabil.MAC) + HUtil32.LoByte(clientItem.Item.MAC)), (ushort)(HUtil32.HiByte(aabil.MAC) + HUtil32.HiByte(clientItem.Item.MAC)));
                        aabil.SPEED = (ushort)(aabil.SPEED + clientItem.Item.Agility);
                        aabil.AntiMagic = (ushort)(aabil.AntiMagic + clientItem.Item.MgAvoid);
                        aabil.AntiPoison = (ushort)(aabil.AntiPoison + clientItem.Item.ToxAvoid);
                        aabil.HP = (ushort)(aabil.HP + clientItem.Item.HpAdd);
                        aabil.MP = (ushort)(aabil.MP + clientItem.Item.MpAdd);
                        if (clientItem.Item.EffType1 > 0)
                        {
                            switch (clientItem.Item.EffType1)
                            {
                                case EfftypeConst.EFFTYPE_HP_MP_ADD:
                                    if ((aabil.HealthRecover + clientItem.Item.EffRate1 > 65000))
                                    {
                                        aabil.HealthRecover = 65000;
                                    }
                                    else
                                    {
                                        aabil.HealthRecover = (ushort)(aabil.HealthRecover + clientItem.Item.EffRate1);
                                    }
                                    if ((aabil.SpellRecover + clientItem.Item.EffValue1 > 65000))
                                    {
                                        aabil.SpellRecover = 65000;
                                    }
                                    else
                                    {
                                        aabil.SpellRecover = (ushort)(aabil.SpellRecover + clientItem.Item.EffValue1);
                                    }
                                    break;
                            }
                        }
                        if (clientItem.Item.EffType2 > 0)
                        {
                            switch (clientItem.Item.EffType2)
                            {
                                case EfftypeConst.EFFTYPE_HP_MP_ADD:
                                    if ((aabil.HealthRecover + clientItem.Item.EffRate2 > 65000))
                                    {
                                        aabil.HealthRecover = 65000;
                                    }
                                    else
                                    {
                                        aabil.HealthRecover = (ushort)(aabil.HealthRecover + clientItem.Item.EffRate2);
                                    }
                                    if ((aabil.SpellRecover + clientItem.Item.EffValue2 > 65000))
                                    {
                                        aabil.SpellRecover = 65000;
                                    }
                                    else
                                    {
                                        aabil.SpellRecover = (ushort)(aabil.SpellRecover + clientItem.Item.EffValue2);
                                    }
                                    break;
                            }
                        }
                        if (clientItem.Item.EffType1 == EfftypeConst.EFFTYPE_LUCK_ADD)
                        {
                            if (aabil.Luck + clientItem.Item.EffValue1 > 255)
                            {
                                aabil.Luck = byte.MaxValue;
                            }
                            else
                            {
                                aabil.Luck = (byte)(aabil.Luck + clientItem.Item.EffValue1);
                            }
                        }
                        else if (clientItem.Item.EffType2 == EfftypeConst.EFFTYPE_LUCK_ADD)
                        {
                            if (aabil.Luck + clientItem.Item.EffValue2 > 255)
                            {
                                aabil.Luck = byte.MaxValue;
                            }
                            else
                            {
                                aabil.Luck = (byte)(aabil.Luck + clientItem.Item.EffValue2);
                            }
                        }
                        break;
                    case 15:
                        aabil.AC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(aabil.AC) + HUtil32.LoByte(clientItem.Item.AC)), (ushort)(HUtil32.HiByte(aabil.AC) + HUtil32.HiByte(clientItem.Item.AC)));
                        aabil.MAC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(aabil.MAC) + HUtil32.LoByte(clientItem.Item.MAC)), (ushort)(HUtil32.HiByte(aabil.MAC) + HUtil32.HiByte(clientItem.Item.MAC)));
                        aabil.HIT = (ushort)(aabil.HIT + clientItem.Item.Accurate);
                        aabil.AntiMagic = (ushort)(aabil.AntiMagic + clientItem.Item.MgAvoid);
                        aabil.AntiPoison = (ushort)(aabil.AntiPoison + clientItem.Item.ToxAvoid);
                        break;
                    case 19:
                        aabil.AntiMagic = (ushort)(aabil.AntiMagic + HUtil32.HiByte(clientItem.Item.AC));
                        aabil.UnLuck = (byte)(aabil.UnLuck + HUtil32.LoByte(clientItem.Item.MAC));
                        aabil.Luck = (byte)(aabil.Luck + HUtil32.HiByte(clientItem.Item.MAC));
                        aabil.HitSpeed = (ushort)(aabil.HitSpeed + clientItem.Item.AtkSpd);
                        aabil.HIT = (ushort)(aabil.HIT + clientItem.Item.Accurate);
                        aabil.Slowdown = (byte)(aabil.Slowdown + clientItem.Item.Slowdown);
                        aabil.Poison = (byte)(aabil.Poison + clientItem.Item.Tox);
                        break;
                    case 20:
                        aabil.HIT = (ushort)(aabil.HIT + HUtil32.HiByte(clientItem.Item.AC));
                        aabil.SPEED = (ushort)(aabil.SPEED + HUtil32.HiByte(clientItem.Item.MAC));
                        aabil.HitSpeed = (ushort)(aabil.HitSpeed + clientItem.Item.AtkSpd);
                        aabil.AntiMagic = (ushort)(aabil.AntiMagic + clientItem.Item.MgAvoid);
                        aabil.Slowdown = (byte)(aabil.Slowdown + clientItem.Item.Slowdown);
                        aabil.Poison = (byte)(aabil.Poison + clientItem.Item.Tox);
                        break;
                    case 21:
                        aabil.HealthRecover = (ushort)(aabil.HealthRecover + HUtil32.HiByte(clientItem.Item.AC));
                        aabil.SpellRecover = (ushort)(aabil.SpellRecover + HUtil32.HiByte(clientItem.Item.MAC));
                        aabil.HitSpeed = (ushort)(aabil.HitSpeed + HUtil32.LoByte(clientItem.Item.AC));
                        aabil.HitSpeed = (ushort)(aabil.HitSpeed - HUtil32.LoByte(clientItem.Item.MAC));
                        aabil.HitSpeed = (ushort)(aabil.HitSpeed + clientItem.Item.AtkSpd);
                        aabil.HIT = (ushort)(aabil.HIT + clientItem.Item.Accurate);
                        aabil.AntiMagic = (ushort)(aabil.AntiMagic + clientItem.Item.MgAvoid);
                        aabil.Slowdown = (byte)(aabil.Slowdown + clientItem.Item.Slowdown);
                        aabil.Poison = (byte)(aabil.Poison + clientItem.Item.Tox);
                        break;
                    case 22:
                        aabil.AC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(aabil.AC) + HUtil32.LoByte(clientItem.Item.AC)), (ushort)(HUtil32.HiByte(aabil.AC) + HUtil32.HiByte(clientItem.Item.AC)));
                        aabil.MAC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(aabil.MAC) + HUtil32.LoByte(clientItem.Item.MAC)), (ushort)(HUtil32.HiByte(aabil.MAC) + HUtil32.HiByte(clientItem.Item.MAC)));
                        aabil.HitSpeed = (ushort)(aabil.HitSpeed + clientItem.Item.AtkSpd);
                        aabil.Slowdown = (byte)(aabil.Slowdown + clientItem.Item.Slowdown);
                        aabil.Poison = (byte)(aabil.Poison + clientItem.Item.Tox);
                        aabil.HIT = (ushort)(aabil.HIT + clientItem.Item.Accurate);
                        aabil.HP = (ushort)(aabil.HP + clientItem.Item.HpAdd);
                        break;
                    case 23:
                        aabil.AntiPoison = (ushort)(aabil.AntiPoison + HUtil32.HiByte(clientItem.Item.AC));
                        aabil.PoisonRecover = (ushort)(aabil.PoisonRecover + HUtil32.HiByte(clientItem.Item.MAC));
                        aabil.HitSpeed = (ushort)(aabil.HitSpeed + HUtil32.LoByte(clientItem.Item.AC));
                        aabil.HitSpeed = (ushort)(aabil.HitSpeed - HUtil32.LoByte(clientItem.Item.MAC));
                        aabil.HitSpeed = (ushort)(aabil.HitSpeed + clientItem.Item.AtkSpd);
                        aabil.Slowdown = (byte)(aabil.Slowdown + clientItem.Item.Slowdown);
                        aabil.Poison = (byte)(aabil.Poison + clientItem.Item.Tox);
                        break;
                    case 24:
                    case 26:
                        if (clientItem.Item.SpecialPwr >= 1 && clientItem.Item.SpecialPwr <= 10)
                        {
                            aabil.WeaponStrong = (byte)clientItem.Item.SpecialPwr;
                        }
                        switch (item.StdMode)
                        {
                            case 24:
                                aabil.HIT = (ushort)(aabil.HIT + HUtil32.HiByte(clientItem.Item.AC));
                                aabil.SPEED = (ushort)(aabil.SPEED + HUtil32.HiByte(clientItem.Item.MAC));
                                break;
                            case 26:
                                aabil.AC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(aabil.AC) + HUtil32.LoByte(clientItem.Item.AC)), (ushort)(HUtil32.HiByte(aabil.AC) + HUtil32.HiByte(clientItem.Item.AC)));
                                aabil.MAC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(aabil.MAC) + HUtil32.LoByte(clientItem.Item.MAC)), (ushort)(HUtil32.HiByte(aabil.MAC) + HUtil32.HiByte(clientItem.Item.MAC)));
                                aabil.HIT = (ushort)(aabil.HIT + clientItem.Item.Accurate);
                                aabil.SPEED = (ushort)(aabil.SPEED + clientItem.Item.Agility);
                                aabil.MP = (ushort)(aabil.MP + clientItem.Item.MpAdd);
                                break;
                        }
                        break;
                    case 52:
                        aabil.AC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(aabil.AC) + HUtil32.LoByte(clientItem.Item.AC)), (ushort)(HUtil32.HiByte(aabil.AC) + HUtil32.HiByte(clientItem.Item.AC)));
                        aabil.MAC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(aabil.MAC) + HUtil32.LoByte(clientItem.Item.MAC)), (ushort)(HUtil32.HiByte(aabil.MAC) + HUtil32.HiByte(clientItem.Item.MAC)));
                        aabil.SPEED = (ushort)(aabil.SPEED + clientItem.Item.Agility);
                        break;
                    case 54:
                        aabil.AC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(aabil.AC) + HUtil32.LoByte(clientItem.Item.AC)), (ushort)(HUtil32.HiByte(aabil.AC) + HUtil32.HiByte(clientItem.Item.AC)));
                        aabil.MAC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(aabil.MAC) + HUtil32.LoByte(clientItem.Item.MAC)), (ushort)(HUtil32.HiByte(aabil.MAC) + HUtil32.HiByte(clientItem.Item.MAC)));
                        aabil.HIT = (ushort)(aabil.HIT + clientItem.Item.Accurate);
                        aabil.SPEED = (ushort)(aabil.SPEED + clientItem.Item.Agility);
                        aabil.AntiPoison = (ushort)(aabil.AntiPoison + clientItem.Item.ToxAvoid);
                        break;
                    case 53:
                        aabil.HP = (ushort)(aabil.HP + clientItem.Item.HpAdd);
                        aabil.MP = (ushort)(aabil.MP + clientItem.Item.MpAdd);
                        break;
                    default:
                        aabil.AC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(aabil.AC) + HUtil32.LoByte(clientItem.Item.AC)), (ushort)(HUtil32.HiByte(aabil.AC) + HUtil32.HiByte(clientItem.Item.AC)));
                        aabil.MAC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(aabil.MAC) + HUtil32.LoByte(clientItem.Item.MAC)), (ushort)(HUtil32.HiByte(aabil.MAC) + HUtil32.HiByte(clientItem.Item.MAC)));
                        break;
                }
                aabil.DC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(aabil.DC) + HUtil32.LoByte(clientItem.Item.DC)), (ushort)HUtil32._MIN(255, HUtil32.HiByte(aabil.DC) + HUtil32.HiByte(clientItem.Item.DC)));
                aabil.MC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(aabil.MC) + HUtil32.LoByte(clientItem.Item.MC)), (ushort)HUtil32._MIN(255, HUtil32.HiByte(aabil.MC) + HUtil32.HiByte(clientItem.Item.MC)));
                aabil.SC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(aabil.SC) + HUtil32.LoByte(clientItem.Item.SC)), (ushort)HUtil32._MIN(255, HUtil32.HiByte(aabil.SC) + HUtil32.HiByte(clientItem.Item.SC)));
            }
        }

        internal static void ApplyItemParametersEx(UserItem uitem, ref Ability aWabil)
        {
            StdItem item = M2Share.WorldEngine.GetStdItem(uitem.Index);
            if (item != null)
            {
                ClientItem clientItem = new ClientItem();
                ItemSystem.GetUpgradeStdItem(item, uitem, ref clientItem);
                switch (item.StdMode)
                {
                    case 52:
                        if (clientItem.Item.EffType1 > 0)
                        {
                            switch (clientItem.Item.EffType1)
                            {
                                case EfftypeConst.EFFTYPE_TWOHAND_WEHIGHT_ADD:
                                    if ((aWabil.MaxHandWeight + clientItem.Item.EffValue1 > 255))
                                    {
                                        aWabil.MaxHandWeight = byte.MaxValue;
                                    }
                                    else
                                    {
                                        aWabil.MaxHandWeight = (byte)(aWabil.MaxHandWeight + clientItem.Item.EffValue1);
                                    }
                                    break;
                                case EfftypeConst.EFFTYPE_EQUIP_WHEIGHT_ADD:
                                    if ((aWabil.MaxWearWeight + clientItem.Item.EffValue1 > 255))
                                    {
                                        aWabil.MaxWearWeight = byte.MaxValue;
                                    }
                                    else
                                    {
                                        aWabil.MaxWearWeight = (byte)(aWabil.MaxWearWeight + clientItem.Item.EffValue1);
                                    }
                                    break;
                            }
                        }
                        if (clientItem.Item.EffType2 > 0)
                        {
                            switch (clientItem.Item.EffType2)
                            {
                                case EfftypeConst.EFFTYPE_TWOHAND_WEHIGHT_ADD:
                                    if ((aWabil.MaxHandWeight + clientItem.Item.EffValue2 > 255))
                                    {
                                        aWabil.MaxHandWeight = byte.MaxValue;
                                    }
                                    else
                                    {
                                        aWabil.MaxHandWeight = (byte)(aWabil.MaxHandWeight + clientItem.Item.EffValue2);
                                    }
                                    break;
                                case EfftypeConst.EFFTYPE_EQUIP_WHEIGHT_ADD:
                                    if ((aWabil.MaxWearWeight + clientItem.Item.EffValue2 > 255))
                                    {
                                        aWabil.MaxWearWeight = byte.MaxValue;
                                    }
                                    else
                                    {
                                        aWabil.MaxWearWeight = (byte)(aWabil.MaxWearWeight + clientItem.Item.EffValue2);
                                    }
                                    break;
                            }
                        }
                        break;
                    case 54:
                        if (clientItem.Item.EffType1 > 0)
                        {
                            switch (clientItem.Item.EffType1)
                            {
                                case EfftypeConst.EFFTYPE_BAG_WHIGHT_ADD:
                                    if ((aWabil.MaxWeight + clientItem.Item.EffValue1 > 65000))
                                    {
                                        aWabil.MaxWeight = 65000;
                                    }
                                    else
                                    {
                                        aWabil.MaxWeight = (ushort)(aWabil.MaxWeight + clientItem.Item.EffValue1);
                                    }
                                    break;
                            }
                        }
                        if (clientItem.Item.EffType2 > 0)
                        {
                            switch (clientItem.Item.EffType2)
                            {
                                case EfftypeConst.EFFTYPE_BAG_WHIGHT_ADD:
                                    if ((aWabil.MaxWeight + clientItem.Item.EffValue2 > 65000))
                                    {
                                        aWabil.MaxWeight = 65000;
                                    }
                                    else
                                    {
                                        aWabil.MaxWeight = (ushort)(aWabil.MaxWeight + clientItem.Item.EffValue2);
                                    }
                                    break;
                            }
                        }
                        break;
                }
            }
        }

        protected void ChangeItemByJob(ref ClientItem citem, int level)
        {
            if ((citem.Item.StdMode == 22) && (citem.Item.Shape == DragonConst.DRAGON_RING_SHAPE))
            {
                switch (Job)
                {
                    case PlayJob.Warrior:
                        citem.Item.DC = HUtil32.MakeWord(HUtil32.LoByte(citem.Item.DC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(citem.Item.DC) + 4));
                        citem.Item.MC = 0;
                        citem.Item.SC = 0;
                        break;
                    case PlayJob.Wizard:
                        citem.Item.DC = 0;
                        citem.Item.SC = 0;
                        break;
                    case PlayJob.Taoist:
                        citem.Item.MC = 0;
                        break;
                }
            }
            else if ((citem.Item.StdMode == 26) && (citem.Item.Shape == DragonConst.DRAGON_BRACELET_SHAPE))
            {
                switch (Job)
                {
                    case PlayJob.Warrior:
                        citem.Item.DC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(citem.Item.DC) + 1), (ushort)HUtil32._MIN(255, HUtil32.HiByte(citem.Item.DC) + 2));
                        citem.Item.MC = 0;
                        citem.Item.SC = 0;
                        citem.Item.AC = HUtil32.MakeWord(HUtil32.LoByte(citem.Item.AC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(citem.Item.AC) + 1));
                        break;
                    case PlayJob.Wizard:
                        citem.Item.DC = 0;
                        citem.Item.SC = 0;
                        citem.Item.AC = HUtil32.MakeWord(HUtil32.LoByte(citem.Item.AC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(citem.Item.AC) + 1));
                        break;
                    case PlayJob.Taoist:
                        citem.Item.MC = 0;
                        break;
                }
            }
            else if ((citem.Item.StdMode == 19) && (citem.Item.Shape == DragonConst.DRAGON_NECKLACE_SHAPE))
            {
                switch (Job)
                {
                    case PlayJob.Warrior:
                        citem.Item.MC = 0;
                        citem.Item.SC = 0;
                        break;
                    case PlayJob.Wizard:
                        citem.Item.DC = 0;
                        citem.Item.SC = 0;
                        break;
                    case PlayJob.Taoist:
                        citem.Item.DC = 0;
                        citem.Item.MC = 0;
                        break;
                }
            }
            else if (((citem.Item.StdMode == 10) || (citem.Item.StdMode == 11)) && (citem.Item.Shape == DragonConst.DRAGON_DRESS_SHAPE))
            {
                switch (Job)
                {
                    case PlayJob.Warrior:
                        citem.Item.MC = 0;
                        citem.Item.SC = 0;
                        break;
                    case PlayJob.Wizard:
                        citem.Item.DC = 0;
                        citem.Item.SC = 0;
                        break;
                    case PlayJob.Taoist:
                        citem.Item.DC = 0;
                        citem.Item.MC = 0;
                        break;
                }
            }
            else if ((citem.Item.StdMode == 15) && (citem.Item.Shape == DragonConst.DRAGON_HELMET_SHAPE))
            {
                switch (Job)
                {
                    case PlayJob.Warrior:
                        citem.Item.MC = 0;
                        citem.Item.SC = 0;
                        break;
                    case PlayJob.Wizard:
                        citem.Item.DC = 0;
                        citem.Item.SC = 0;
                        break;
                    case PlayJob.Taoist:
                        citem.Item.DC = 0;
                        citem.Item.MC = 0;
                        break;
                }
            }
            else if (((citem.Item.StdMode == 5) || (citem.Item.StdMode == 6)) && (citem.Item.Shape == DragonConst.DRAGON_WEAPON_SHAPE))
            {
                switch (Job)
                {
                    case PlayJob.Warrior:
                        citem.Item.DC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(citem.Item.DC) + 1), (ushort)HUtil32._MIN(255, HUtil32.HiByte(citem.Item.DC) + 28));
                        citem.Item.MC = 0;
                        citem.Item.SC = 0;
                        citem.Item.AC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(citem.Item.AC) - 2), HUtil32.HiByte(citem.Item.AC));
                        break;
                    case PlayJob.Wizard:
                        citem.Item.SC = 0;
                        if (HUtil32.HiByte(citem.Item.MAC) > 12)
                        {
                            citem.Item.MAC = HUtil32.MakeWord(HUtil32.LoByte(citem.Item.MAC), (ushort)(HUtil32.HiByte(citem.Item.MAC) - 12));
                        }
                        else
                        {
                            citem.Item.MAC = HUtil32.MakeWord(HUtil32.LoByte(citem.Item.MAC), 0);
                        }
                        break;
                    case PlayJob.Taoist:
                        citem.Item.DC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(citem.Item.DC) + 2), (ushort)HUtil32._MIN(255, HUtil32.HiByte(citem.Item.DC) + 10));
                        citem.Item.MC = 0;
                        citem.Item.AC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(citem.Item.AC) - 2), HUtil32.HiByte(citem.Item.AC));
                        break;
                }
            }
            else if ((citem.Item.StdMode == 53))
            {
                if ((citem.Item.Shape == ShapeConst.LOLLIPOP_SHAPE))
                {
                    switch (Job)
                    {
                        case PlayJob.Warrior:
                            citem.Item.DC = HUtil32.MakeWord(HUtil32.LoByte(citem.Item.DC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(citem.Item.DC) + 2));
                            citem.Item.MC = 0;
                            citem.Item.SC = 0;
                            break;
                        case PlayJob.Wizard:
                            citem.Item.DC = 0;
                            citem.Item.MC = HUtil32.MakeWord(HUtil32.LoByte(citem.Item.MC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(citem.Item.MC) + 2));
                            citem.Item.SC = 0;
                            break;
                        case PlayJob.Taoist:
                            citem.Item.DC = 0;
                            citem.Item.MC = 0;
                            citem.Item.SC = HUtil32.MakeWord(HUtil32.LoByte(citem.Item.SC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(citem.Item.SC) + 2));
                            break;
                    }
                }
                else if ((citem.Item.Shape == ShapeConst.GOLDMEDAL_SHAPE) || (citem.Item.Shape == ShapeConst.SILVERMEDAL_SHAPE) || (citem.Item.Shape == ShapeConst.BRONZEMEDAL_SHAPE))
                {
                    switch (Job)
                    {
                        case PlayJob.Warrior:
                            citem.Item.DC = HUtil32.MakeWord(HUtil32.LoByte(citem.Item.DC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(citem.Item.DC)));
                            citem.Item.MC = 0;
                            citem.Item.SC = 0;
                            break;
                        case PlayJob.Wizard:
                            citem.Item.DC = 0;
                            citem.Item.MC = HUtil32.MakeWord(HUtil32.LoByte(citem.Item.MC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(citem.Item.MC)));
                            citem.Item.SC = 0;
                            break;
                        case PlayJob.Taoist:
                            citem.Item.DC = 0;
                            citem.Item.MC = 0;
                            citem.Item.SC = HUtil32.MakeWord(HUtil32.LoByte(citem.Item.SC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(citem.Item.SC)));
                            break;
                    }
                }
            }
        }

        private void ApplyItemParametersByJob(UserItem uitem, ref ClientItem std)
        {
            StdItem item = M2Share.WorldEngine.GetStdItem(uitem.Index);
            if (item != null)
            {
                if ((item.StdMode == 22) && (item.Shape == DragonConst.DRAGON_RING_SHAPE))
                {
                    switch (Job)
                    {
                        case PlayJob.Warrior:
                            std.Item.DC = HUtil32.MakeWord(HUtil32.LoByte(item.DC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(item.DC) + 4));
                            std.Item.MC = 0;
                            std.Item.SC = 0;
                            break;
                        case PlayJob.Wizard:
                            std.Item.DC = 0;
                            std.Item.SC = 0;
                            break;
                        case PlayJob.Taoist:
                            std.Item.MC = 0;
                            break;
                    }
                }
                else if ((item.StdMode == 26) && (item.Shape == DragonConst.DRAGON_BRACELET_SHAPE))
                {
                    switch (Job)
                    {
                        case PlayJob.Warrior:
                            std.Item.DC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(item.DC) + 1), (ushort)HUtil32._MIN(255, HUtil32.HiByte(item.DC) + 2));
                            std.Item.MC = 0;
                            std.Item.SC = 0;
                            std.Item.AC = HUtil32.MakeWord(HUtil32.LoByte(item.AC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(item.AC) + 1));
                            break;
                        case PlayJob.Wizard:
                            std.Item.DC = 0;
                            std.Item.SC = 0;
                            std.Item.AC = HUtil32.MakeWord(HUtil32.LoByte(item.AC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(item.AC) + 1));
                            break;
                        case PlayJob.Taoist:
                            std.Item.MC = 0;
                            break;
                    }
                }
                else if ((item.StdMode == 19) && (item.Shape == DragonConst.DRAGON_NECKLACE_SHAPE))
                {
                    switch (Job)
                    {
                        case PlayJob.Warrior:
                            std.Item.MC = 0;
                            std.Item.SC = 0;
                            break;
                        case PlayJob.Wizard:
                            std.Item.DC = 0;
                            std.Item.SC = 0;
                            break;
                        case PlayJob.Taoist:
                            std.Item.DC = 0;
                            std.Item.MC = 0;
                            break;
                    }
                }
                else if (((item.StdMode == 10) || (item.StdMode == 11)) && (item.Shape == DragonConst.DRAGON_DRESS_SHAPE))
                {
                    switch (Job)
                    {
                        case PlayJob.Warrior:
                            std.Item.MC = 0;
                            std.Item.SC = 0;
                            break;
                        case PlayJob.Wizard:
                            std.Item.DC = 0;
                            std.Item.SC = 0;
                            break;
                        case PlayJob.Taoist:
                            std.Item.DC = 0;
                            std.Item.MC = 0;
                            break;
                    }
                }
                else if ((item.StdMode == 15) && (item.Shape == DragonConst.DRAGON_HELMET_SHAPE))
                {
                    switch (Job)
                    {
                        case PlayJob.Warrior:
                            std.Item.MC = 0;
                            std.Item.SC = 0;
                            break;
                        case PlayJob.Wizard:
                            std.Item.DC = 0;
                            std.Item.SC = 0;
                            break;
                        case PlayJob.Taoist:
                            std.Item.DC = 0;
                            std.Item.MC = 0;
                            break;
                    }
                }
                else if (((item.StdMode == 5) || (item.StdMode == 6)) && (item.Shape == DragonConst.DRAGON_WEAPON_SHAPE))
                {
                    switch (Job)
                    {
                        case PlayJob.Warrior:
                            std.Item.DC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(item.DC) + 1), (ushort)HUtil32._MIN(255, HUtil32.HiByte(item.DC) + 28));
                            std.Item.MC = 0;
                            std.Item.SC = 0;
                            std.Item.AC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(item.AC) - 2), HUtil32.HiByte(item.AC));
                            break;
                        case PlayJob.Wizard:
                            std.Item.SC = 0;
                            if (HUtil32.HiByte(item.MAC) > 12)
                            {
                                std.Item.MAC = HUtil32.MakeWord(HUtil32.LoByte(item.MAC), (ushort)(HUtil32.HiByte(item.MAC) - 12));
                            }
                            else
                            {
                                std.Item.MAC = HUtil32.MakeWord(HUtil32.LoByte(item.MAC), 0);
                            }
                            break;
                        case PlayJob.Taoist:
                            std.Item.DC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(item.DC) + 2), (ushort)HUtil32._MIN(255, HUtil32.HiByte(item.DC) + 10));
                            std.Item.MC = 0;
                            std.Item.AC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(item.AC) - 2), HUtil32.HiByte(item.AC));
                            break;
                    }
                }
                else if ((item.StdMode == 53))
                {
                    if ((item.Shape == ShapeConst.LOLLIPOP_SHAPE))
                    {
                        switch (Job)
                        {
                            case PlayJob.Warrior:
                                std.Item.DC = HUtil32.MakeWord(HUtil32.LoByte(item.DC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(item.DC) + 2));
                                std.Item.MC = 0;
                                std.Item.SC = 0;
                                break;
                            case PlayJob.Wizard:
                                std.Item.DC = 0;
                                std.Item.MC = HUtil32.MakeWord(HUtil32.LoByte(item.MC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(item.MC) + 2));
                                std.Item.SC = 0;
                                break;
                            case PlayJob.Taoist:
                                std.Item.DC = 0;
                                std.Item.MC = 0;
                                std.Item.SC = HUtil32.MakeWord(HUtil32.LoByte(item.SC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(item.SC) + 2));
                                break;
                        }
                    }
                    else if ((item.Shape == ShapeConst.GOLDMEDAL_SHAPE) || (item.Shape == ShapeConst.SILVERMEDAL_SHAPE) || (item.Shape == ShapeConst.BRONZEMEDAL_SHAPE))
                    {
                        switch (Job)
                        {
                            case PlayJob.Warrior:
                                std.Item.DC = HUtil32.MakeWord(HUtil32.LoByte(item.DC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(item.DC)));
                                std.Item.MC = 0;
                                std.Item.SC = 0;
                                break;
                            case PlayJob.Wizard:
                                std.Item.DC = 0;
                                std.Item.MC = HUtil32.MakeWord(HUtil32.LoByte(item.MC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(item.MC)));
                                std.Item.SC = 0;
                                break;
                            case PlayJob.Taoist:
                                std.Item.DC = 0;
                                std.Item.MC = 0;
                                std.Item.SC = HUtil32.MakeWord(HUtil32.LoByte(item.SC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(item.SC)));
                                break;
                        }
                    }
                }
                if (((item.StdMode == 10) || (item.StdMode == 11)) && (item.Shape == ItemShapeConst.DRESS_SHAPE_PBKING))
                {
                    switch (Job)
                    {
                        case PlayJob.Warrior:
                            std.Item.DC = HUtil32.MakeWord(HUtil32.LoByte(item.DC), (ushort)HUtil32._MIN(255, HUtil32.HiByte(item.DC) + 2));
                            std.Item.MC = 0;
                            std.Item.SC = 0;
                            std.Item.AC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(item.AC) + 2), (ushort)HUtil32._MIN(255, HUtil32.HiByte(item.AC) + 4));
                            std.Item.MpAdd = item.MpAdd + 30;
                            break;
                        case PlayJob.Wizard:
                            std.Item.DC = 0;
                            std.Item.SC = 0;
                            std.Item.MAC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(item.MAC) + 1), (ushort)HUtil32._MIN(255, HUtil32.HiByte(item.MAC) + 2));
                            std.Item.HpAdd = item.HpAdd + 30;
                            break;
                        case PlayJob.Taoist:
                            std.Item.DC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(item.DC) + 1), (ushort)HUtil32._MIN(255, HUtil32.HiByte(item.DC)));
                            std.Item.MC = 0;
                            std.Item.AC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(item.AC) + 1), (ushort)HUtil32._MIN(255, HUtil32.HiByte(item.AC)));
                            std.Item.MAC = HUtil32.MakeWord((ushort)(HUtil32.LoByte(item.MAC) + 1), (ushort)HUtil32._MIN(255, HUtil32.HiByte(item.MAC)));
                            std.Item.HpAdd = item.HpAdd + 20;
                            std.Item.MpAdd = item.MpAdd + 10;
                            break;
                    }
                }
            }
        }

        private void DeleteNameSkill(string sSkillName)
        {
            for (int i = 0; i < MagicList.Count; i++)
            {
                UserMagic userMagic = MagicList[i];
                if (userMagic.Magic.MagicName == sSkillName)
                {
                    SendDelMagic(userMagic);
                    MagicList.RemoveAt(i);
                    break;
                }
            }
        }

        private void DelItemSkill(int nIndex)
        {
            if (Race != ActorRace.Play)
            {
                return;
            }
            switch (nIndex)
            {
                case 1:
                    if (Job != PlayJob.Wizard)
                    {
                        DeleteNameSkill(M2Share.Config.FireBallSkill);
                    }
                    break;
                case 2:
                    if (Job != PlayJob.Taoist)
                    {
                        DeleteNameSkill(M2Share.Config.HealSkill);
                    }
                    break;
            }
        }

        private void DelMember(PlayObject baseObject)
        {
            if (GroupOwner != baseObject.ActorId)
            {
                for (int i = 0; i < GroupMembers.Count; i++)
                {
                    if (GroupMembers[i] == baseObject)
                    {
                        baseObject.LeaveGroup();
                        GroupMembers.RemoveAt(i);
                        break;
                    }
                }
            }
            else
            {
                for (int i = GroupMembers.Count - 1; i >= 0; i--)
                {
                    GroupMembers[i].LeaveGroup();
                    GroupMembers.RemoveAt(i);
                }
            }
            if (!this.CancelGroup())
            {
                this.SendDefMessage(Messages.SM_GROUPCANCEL, 0, 0, 0, 0);
            }
            else
            {
                this.SendGroupMembers();
            }
        }

        private bool IsGroupMember(BaseObject target)
        {
            bool result = false;
            if (GroupOwner == 0)
            {
                return false;
            }
            PlayObject groupOwnerPlay = (PlayObject)M2Share.ActorMgr.Get(GroupOwner);
            for (int i = 0; i < groupOwnerPlay.GroupMembers.Count; i++)
            {
                if (groupOwnerPlay.GroupMembers[i] == target)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        private void LeaveGroup()
        {
            const string sExitGroupMsg = "{0} 已经退出了本组.";
            SendGroupText(Format(sExitGroupMsg, ChrName));
            GroupOwner = 0;
            SendMsg(Messages.RM_GROUPCANCEL, 0, 0, 0, 0);
        }

        public void SendGroupText(string sMsg)
        {
            sMsg = M2Share.Config.GroupMsgPreFix + sMsg;
            if (GroupOwner != 0)
            {
                PlayObject groupOwnerPlay = (PlayObject)M2Share.ActorMgr.Get(GroupOwner);
                for (int i = 0; i < groupOwnerPlay.GroupMembers.Count; i++)
                {
                    groupOwnerPlay.GroupMembers[i].SendMsg(this, Messages.RM_GROUPMESSAGE, 0, M2Share.Config.GroupMsgFColor, M2Share.Config.GroupMsgBColor, 0, sMsg);
                }
            }
        }

        public bool CheckMagicLevelUp(UserMagic userMagic)
        {
            bool result = false;
            int nLevel;
            if ((userMagic.Level < 4) && (userMagic.Magic.TrainLv >= userMagic.Level))
            {
                nLevel = userMagic.Level;
            }
            else
            {
                nLevel = 0;
            }
            if ((userMagic.Magic.TrainLv > userMagic.Level) && (userMagic.Magic.MaxTrain[nLevel] <= userMagic.TranPoint))
            {
                if (userMagic.Magic.TrainLv > userMagic.Level)
                {
                    userMagic.TranPoint -= userMagic.Magic.MaxTrain[nLevel];
                    userMagic.Level++;
                    UpdateDelayMsg(Messages.RM_MAGIC_LVEXP, 0, userMagic.Magic.MagicId, userMagic.Level, userMagic.TranPoint, "", 800);
                    CheckSeeHealGauge(userMagic);
                }
                else
                {
                    userMagic.TranPoint = userMagic.Magic.MaxTrain[nLevel];
                }
                result = true;
            }
            return result;
        }

        /// <summary>
        /// 心灵启示
        /// </summary>
        protected void CheckSeeHealGauge(UserMagic magic)
        {
            if (magic.Magic.MagicId == 28)
            {
                if (magic.Level >= 2)
                {
                    AbilSeeHealGauge = true;
                }
            }
        }

        public void HasLevelUp(int nLevel)
        {
            Abil.MaxExp = GetLevelExp(Abil.Level);
            RecalcLevelAbilitys();
            RecalcAbilitys();
            SendMsg(Messages.RM_LEVELUP, 0, Abil.Exp, 0, 0);
            if (M2Share.FunctionNPC != null)
            {
                M2Share.FunctionNPC.GotoLable(this, "@LevelUp", false);
            }
        }

        public void RecalcLevelAbilitys()
        {
            int n;
            byte nLevel = Abil.Level;
            switch (this.Job)
            {
                case PlayJob.Taoist:
                    Abil.MaxHP = (ushort)HUtil32._MIN(ushort.MaxValue, 14 + HUtil32.Round((nLevel / (double)M2Share.Config.nLevelValueOfTaosHP + M2Share.Config.nLevelValueOfTaosHPRate) * nLevel));
                    Abil.MaxMP = (ushort)HUtil32._MIN(ushort.MaxValue, 13 + HUtil32.Round(nLevel / (double)M2Share.Config.nLevelValueOfTaosMP * 2.2 * nLevel));
                    Abil.MaxWeight = (ushort)(50 + HUtil32.Round(nLevel / 4.0 * nLevel));
                    Abil.MaxWearWeight = (byte)HUtil32._MIN(byte.MaxValue, (15 + HUtil32.Round(nLevel / 50.0 * nLevel)));
                    if ((12 + HUtil32.Round(Abil.Level / 13.0 * Abil.Level)) > 255)
                    {
                        Abil.MaxHandWeight = byte.MaxValue;
                    }
                    else
                    {
                        Abil.MaxHandWeight = (byte)(12 + HUtil32.Round(nLevel / 42.0 * nLevel));
                    }
                    n = nLevel / 7;
                    Abil.DC = HUtil32.MakeWord((ushort)HUtil32._MAX(n - 1, 0), (ushort)HUtil32._MAX(1, n));
                    Abil.MC = 0;
                    Abil.SC = HUtil32.MakeWord((ushort)HUtil32._MAX(n - 1, 0), (ushort)HUtil32._MAX(1, n));
                    Abil.AC = 0;
                    n = HUtil32.Round(nLevel / 6.0);
                    Abil.MAC = HUtil32.MakeWord((ushort)(n / 2), (ushort)(n + 1));
                    break;
                case PlayJob.Wizard:
                    Abil.MaxHP = (ushort)HUtil32._MIN(ushort.MaxValue, 14 + HUtil32.Round(((nLevel / (double)M2Share.Config.nLevelValueOfWizardHP) + M2Share.Config.nLevelValueOfWizardHPRate) * nLevel));
                    Abil.MaxMP = (ushort)HUtil32._MIN(ushort.MaxValue, 13 + HUtil32.Round(((nLevel / (double)5) + 2) * 2.2 * nLevel));
                    Abil.MaxWeight = (ushort)(50 + HUtil32.Round(nLevel / 5.0 * nLevel));
                    Abil.MaxWearWeight = (byte)HUtil32._MIN(byte.MaxValue, 15 + HUtil32.Round(nLevel / 100.0 * nLevel));
                    Abil.MaxHandWeight = (byte)(12 + HUtil32.Round(nLevel / 90.0 * nLevel));
                    n = nLevel / 7;
                    Abil.DC = HUtil32.MakeWord((ushort)HUtil32._MAX(n - 1, 0), (ushort)HUtil32._MAX(1, n));
                    Abil.MC = HUtil32.MakeWord((ushort)HUtil32._MAX(n - 1, 0), (ushort)HUtil32._MAX(1, n));
                    Abil.SC = 0;
                    Abil.AC = 0;
                    Abil.MAC = 0;
                    break;
                case PlayJob.Warrior:
                    Abil.MaxHP = (ushort)HUtil32._MIN(ushort.MaxValue, 14 + HUtil32.Round(((nLevel / (double)M2Share.Config.nLevelValueOfWarrHP) + M2Share.Config.nLevelValueOfWarrHPRate + (nLevel / (double)20)) * nLevel));
                    Abil.MaxMP = (ushort)HUtil32._MIN(ushort.MaxValue, 11 + HUtil32.Round(nLevel * 3.5));
                    Abil.MaxWeight = (ushort)(50 + HUtil32.Round(nLevel / 3.0 * nLevel));
                    Abil.MaxWearWeight = (byte)HUtil32._MIN(byte.MaxValue, (15 + HUtil32.Round(nLevel / 20.0 * nLevel)));
                    Abil.MaxHandWeight = (byte)(12 + HUtil32.Round(nLevel / 13.0 * nLevel));
                    Abil.DC = HUtil32.MakeWord((ushort)HUtil32._MAX(nLevel / 5 - 1, 1), (ushort)HUtil32._MAX(1, nLevel / 5));
                    Abil.SC = 0;
                    Abil.MC = 0;
                    Abil.AC = HUtil32.MakeWord(0, (ushort)(nLevel / 7));
                    Abil.MAC = 0;
                    break;
                case PlayJob.None:
                    break;
            }
            if (Abil.HP > Abil.MaxHP)
            {
                Abil.HP = Abil.MaxHP;
            }
            if (Abil.MP > Abil.MaxMP)
            {
                Abil.MP = Abil.MaxMP;
            }
        }

        /// <summary>
        /// 无极真气
        /// </summary>
        /// <returns></returns>
        public void AttPowerUp(int nPower, int nTime)
        {
            this.ExtraAbil[0] = (ushort)nPower;
            this.ExtraAbilTimes[0] = HUtil32.GetTickCount() + nTime * 1000;
            SysMsg(Format(Settings.AttPowerUpTime, nTime / 60, nTime % 60), MsgColor.Green, MsgType.Hint);
            RecalcAbilitys();
            SendMsg(Messages.RM_ABILITY, 0, 0, 0, 0);
        }

        public UserItem CheckItemCount(string sItemName, ref int nCount)
        {
            UserItem result = null;
            nCount = 0;
            for (int i = 0; i < UseItems.Length; i++)
            {
                if (UseItems[i] == null)
                {
                    continue;
                }
                string sName = M2Share.WorldEngine.GetStdItemName(UseItems[i].Index);
                if (string.Compare(sName, sItemName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    result = UseItems[i];
                    nCount++;
                }
            }
            return result;
        }

        /// <summary>
        /// 减少复活戒指持久
        /// </summary>
        internal void ItemDamageRevivalRing()
        {
            for (int i = 0; i < UseItems.Length; i++)
            {
                if (UseItems[i] != null && UseItems[i].Index > 0)
                {
                    StdItem pSItem = M2Share.WorldEngine.GetStdItem(UseItems[i].Index);
                    if (pSItem != null)
                    {
                        if (M2Share.ItemDamageRevivalMap.Contains(pSItem.Shape) || (((i == ItemLocation.Weapon) || (i == ItemLocation.RighThand)) && M2Share.ItemDamageRevivalMap.Contains(pSItem.AniCount)))
                        {
                            ushort nDura = UseItems[i].Dura;
                            ushort tDura = (ushort)HUtil32.Round(nDura / 1000.0);
                            nDura -= 1000;
                            if (nDura <= 0)
                            {
                                nDura = 0;
                                UseItems[i].Dura = nDura;
                                if (Race == ActorRace.Play)
                                {
                                    this.SendDelItems(UseItems[i]);
                                }
                                UseItems[i].Index = 0;
                                RecalcAbilitys();
                            }
                            else
                            {
                                UseItems[i].Dura = nDura;
                            }
                            if (tDura != HUtil32.Round(nDura / 1000.0)) // 1.03
                            {
                                SendMsg(Messages.RM_DURACHANGE, i, nDura, UseItems[i].DuraMax, 0);
                            }
                        }
                    }
                }
            }
        }

        private static bool IsGoodKilling(PlayObject cert)
        {
            return cert.PvpFlag;
        }

        internal UserMagic GetAttackMagic(int magicId)
        {
            return MagicArr[magicId];
        }

        internal byte GetMyLight()
        {
            byte currentLight = 0;
            if (Abil.Level >= EfftypeConst.EFFECTIVE_HIGHLEVEL)
            {
                currentLight = 1;
            }
            for (byte i = ItemLocation.Dress; i <= ItemLocation.Charm; i++)
            {
                if (UseItems[i] == null)
                {
                    continue;
                }
                if ((UseItems[i].Index > 0) && (UseItems[i].Dura > 0))
                {
                    StdItem stdItem = M2Share.WorldEngine.GetStdItem(UseItems[i].Index);
                    if (stdItem != null)
                    {
                        if (currentLight < stdItem.Light)
                        {
                            currentLight = stdItem.Light;
                        }
                    }
                }
            }
            return currentLight;
        }

        /// <summary>
        /// 更新可见物品列表
        /// </summary>
        protected void UpdateVisibleItem(short wX, short wY, MapItem MapItem)
        {
            VisibleMapItem visibleMapItem = null;
            bool boIsVisible = false;
            for (int i = 0; i < VisibleItems.Count; i++)
            {
                visibleMapItem = VisibleItems[i];
                if (visibleMapItem.MapItem == MapItem)
                {
                    visibleMapItem.VisibleFlag = VisibleFlag.Invisible;
                    boIsVisible = true;
                    break;
                }
            }
            if (boIsVisible)
            {
                return;
            }
            visibleMapItem ??= new VisibleMapItem
            {
                VisibleFlag = VisibleFlag.Show,
                nX = wX,
                nY = wY,
                MapItem = MapItem,
                sName = MapItem.Name,
                wLooks = MapItem.Looks
            };
            VisibleItems.Add(visibleMapItem);
        }

        protected void UpdateVisibleEvent(short wX, short wY, MapEvent MapEvent)
        {
            bool boIsVisible = false;
            for (int i = 0; i < VisibleEvents.Count; i++)
            {
                MapEvent mapEvent = VisibleEvents[i];
                if (mapEvent == MapEvent)
                {
                    mapEvent.VisibleFlag = VisibleFlag.Invisible;
                    boIsVisible = true;
                    break;
                }
            }
            if (boIsVisible)
            {
                return;
            }
            MapEvent.VisibleFlag = VisibleFlag.Show;
            MapEvent.nX = wX;
            MapEvent.nY = wY;
            VisibleEvents.Add(MapEvent);
        }
    }
}