﻿using GameSrv.Actor;
using Spectre.Console;
using SystemModule.Core.Common;
using SystemModule.Data;
using SystemModule.Enums;

namespace GameSrv.Monster
{
    public class MonsterObject : AnimalObject
    {
        private int m_dwThinkTick;
        private bool m_boDupMode;

        public MonsterObject() : base()
        {
            m_boDupMode = false;
            m_dwThinkTick = HUtil32.GetTickCount();
            ViewRange = 5;
            RunTime = 250;
            SearchTime = 3000 + M2Share.RandomNumber.Random(2000);
            SearchTick = HUtil32.GetTickCount();
        }

        protected BaseObject MakeClone(string sMonName, BaseObject OldMon)
        {
            AnimalObject ElfMon = (AnimalObject)M2Share.WorldEngine.RegenMonsterByName(Envir.MapName, CurrX, CurrY, sMonName);
            if (ElfMon != null)
            {
                if (OldMon.TargetCret == null)
                {
                    OldMon.TargetCret = OldMon.Master.TargetCret == null ? OldMon.Master.LastHiter : OldMon.Master.TargetCret;
                }
                ElfMon.Master = OldMon.Master;
                ElfMon.MasterRoyaltyTick = OldMon.MasterRoyaltyTick;
                ElfMon.SlaveMakeLevel = OldMon.SlaveMakeLevel;
                ElfMon.SlaveExpLevel = OldMon.SlaveExpLevel;
                ElfMon.RecalcAbilitys();
                ElfMon.RefNameColor();
                ElfMon.Abil = OldMon.Abil;
                ElfMon.StatusTimeArr = OldMon.StatusTimeArr;
                ElfMon.TargetCret = OldMon.TargetCret;
                ElfMon.TargetFocusTick = OldMon.TargetFocusTick;
                ElfMon.LastHiter = OldMon.LastHiter;
                ElfMon.LastHiterTick = OldMon.LastHiterTick;
                ElfMon.Dir = OldMon.Dir;
                ElfMon.IsSlave = true;
                if (OldMon.Master != null)
                {
                    OldMon.Master.SlaveList.Add(ElfMon);
                }
                return ElfMon;
            }
            return null;
        }

        /// <summary>
        /// 更新自身视野对象（可见对象）
        /// </summary>
        /// <param name="acrotId"></param>
        private void UpdateMonsterVisible(int acrotId)
        {
            bool boIsVisible = false;
            VisibleBaseObject visibleBaseObject;
            var baseObject = M2Share.ActorMgr.Get(acrotId);
            if ((baseObject.Race == ActorRace.Play) || (baseObject.Master != null))// 如果是人物或宝宝则置TRUE
            {
                IsVisibleActive = true;
            }
            for (int i = 0; i < VisibleActors.Count; i++)
            {
                visibleBaseObject = VisibleActors[i];
                if (visibleBaseObject == null)
                {
                    continue;
                }
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

        protected override bool Operate(ProcessMessage processMsg)
        {
            if (processMsg.wIdent == Messages.RM_UPDATEVIEWRANGE)
            {
                UpdateMonsterVisible(processMsg.wParam);
                return true;
            }
            return base.Operate(processMsg);
        }

        private bool Think()
        {
            bool result = false;
            if ((HUtil32.GetTickCount() - m_dwThinkTick) > (3 * 1000))
            {
                m_dwThinkTick = HUtil32.GetTickCount();
                if (Envir.GetXyObjCount(CurrX, CurrY) >= 2)
                {
                    m_boDupMode = true;
                }
                if (!IsProperTarget(TargetCret))
                {
                    TargetCret = null;
                }
            }
            if (m_boDupMode)
            {
                int nOldX = CurrX;
                int nOldY = CurrY;
                WalkTo(M2Share.RandomNumber.RandomByte(8), false);
                if (nOldX != CurrX || nOldY != CurrY)
                {
                    m_boDupMode = false;
                    result = true;
                }
            }
            return result;
        }

        protected virtual bool AttackTarget()
        {
            byte btDir = 0;
            if (TargetCret != null)
            {
                if (GetAttackDir(TargetCret, ref btDir))
                {
                    if (HUtil32.GetTickCount() - AttackTick > NextHitTime)
                    {
                        AttackTick = HUtil32.GetTickCount();
                        TargetFocusTick = HUtil32.GetTickCount();
                        Attack(TargetCret, btDir);
                        BreakHolySeizeMode();
                    }
                    return true;
                }
                else
                {
                    if (TargetCret.Envir == Envir)
                    {
                        SetTargetXy(TargetCret.CurrX, TargetCret.CurrY);
                    }
                    else
                    {
                        DelTargetCreat();
                    }
                }
            }
            return false;
        }

        public override void Run()
        {
            if (CanMove() && !FixedHideMode && !StoneMode)
            {
                if (Think())
                {
                    base.Run();
                    return;
                }
                if (WalkWaitLocked)
                {
                    if ((HUtil32.GetTickCount() - WalkWaitTick) > WalkWait)
                    {
                        WalkWaitLocked = false;
                    }
                }
                if (!WalkWaitLocked && (HUtil32.GetTickCount() - WalkTick) > WalkSpeed)
                {
                    WalkTick = HUtil32.GetTickCount();
                    WalkCount++;
                    if (WalkCount > WalkStep)
                    {
                        WalkCount = 0;
                        WalkWaitLocked = true;
                        WalkWaitTick = HUtil32.GetTickCount();
                    }
                    if (!RunAwayMode)
                    {
                        if (!NoAttackMode)
                        {
                            if (TargetCret != null)
                            {
                                if (AttackTarget())
                                {
                                    base.Run();
                                    return;
                                }
                            }
                            else
                            {
                                TargetX = -1;
                                if (Mission)
                                {
                                    TargetX = MissionX;
                                    TargetY = MissionY;
                                }
                            }
                        }
                        if (Master != null)
                        {
                            short nX = 0;
                            short nY = 0;
                            if (TargetCret == null)
                            {
                                Master.GetBackPosition(ref nX, ref nY);
                                if (Math.Abs(TargetX - nX) > 1 || Math.Abs(TargetY - nY) > 1)
                                {
                                    TargetX = nX;
                                    TargetY = nY;
                                    if (Math.Abs(CurrX - nX) <= 2 && Math.Abs(CurrY - nY) <= 2)
                                    {
                                        if (Envir.GetMovingObject(nX, nY, true) != null)
                                        {
                                            TargetX = CurrX;
                                            TargetY = CurrY;
                                        }
                                    }
                                }
                            }
                            if (!Master.SlaveRelax && (Envir != Master.Envir || Math.Abs(CurrX - Master.CurrX) > 20 || Math.Abs(CurrY - Master.CurrY) > 20)) //离主人视野范围超过20
                            {
                                SpaceMove(Master.Envir.MapName, TargetX, TargetY, 1);//飞到主人身边
                            }
                        }
                    }
                    else
                    {
                        if (RunAwayTime > 0 && (HUtil32.GetTickCount() - RunAwayStart) > RunAwayTime)
                        {
                            RunAwayMode = false;
                            RunAwayTime = 0;
                        }
                    }
                    if (Master != null && Master.SlaveRelax)
                    {
                        base.Run();
                        return;
                    }
                    if (TargetX != -1)
                    {
                        GotoTargetXy();
                    }
                    else
                    {
                        if (TargetCret == null)
                        {
                            Wondering();
                        }
                    }
                }
            }
            base.Run();
        }

        protected bool GetLongAttackDirDis(BaseObject baseObject, int dis, ref byte dir)
        {
            var result = false;
            var nC = 0;
            while (true)
            {
                if (CurrX - nC <= baseObject.CurrX && (CurrX + nC >= baseObject.CurrX) && (CurrY - nC <= baseObject.CurrY) && (CurrY + nC >= baseObject.CurrY) && ((CurrX != baseObject.CurrX) || (CurrY != baseObject.CurrY)))
                {
                    if ((CurrX - nC == baseObject.CurrX) && (CurrY == baseObject.CurrY))
                    {
                        dir = Direction.Left;
                        result = true;
                    }
                    if ((CurrX + nC == baseObject.CurrX) && (CurrY == baseObject.CurrY))
                    {
                        dir = Direction.Right;
                        result = true;
                        break;
                    }
                    if (CurrX == baseObject.CurrX && ((CurrY - nC) == baseObject.CurrY))
                    {
                        dir = Direction.Up;
                        result = true;
                        break;
                    }
                    if (CurrX == baseObject.CurrX && ((CurrY + nC) == baseObject.CurrY))
                    {
                        dir = Direction.Down;
                        result = true;
                        break;
                    }
                    if ((CurrX - nC) == baseObject.CurrX && ((CurrY - nC) == baseObject.CurrY))
                    {
                        dir = Direction.UpLeft;
                        result = true;
                        break;
                    }
                    if ((CurrX + nC) == baseObject.CurrX && ((CurrY - nC) == baseObject.CurrY))
                    {
                        dir = Direction.UpRight;
                        result = true;
                        break;
                    }
                    if ((CurrX - nC) == baseObject.CurrX && ((CurrY + nC) == baseObject.CurrY))
                    {
                        dir = Direction.DownLeft;
                        result = true;
                        break;
                    }
                    if ((CurrX + nC) == baseObject.CurrX && ((CurrY + nC) == baseObject.CurrY))
                    {
                        dir = Direction.DownRight;
                        result = true;
                        break;
                    }
                    dir = 0;
                }
                nC++;
                if (nC > dis)
                {
                    break;
                }
            }
            return result;
        }
    }
}