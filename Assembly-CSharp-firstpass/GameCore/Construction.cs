using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO;
using System.Xml;
using Offworld.GameCore.Text;
using Offworld.SystemCore;
using System.Linq;

namespace Offworld.GameCore
{
    public class ConstructionClient : TextHelpers
    {
    	protected GameClient mGame = null;
    	GameClient gameClient()
    	{
    		return mGame;
    	}
    	protected virtual Infos infos()
    	{
    		return gameClient().infos();
    	}

        public enum ConstructionDirtyType
        {
            FIRST,

            miConstruction,
            miDamage,
            mbWasDamaged,
            meOwner,
            maiResourceCost,

            NUM_TYPES
        }

        protected BitMask mDirtyBits = new BitMask();
        bool isDirty(ConstructionDirtyType eType)
        {
            return mDirtyBits.GetBit((int)eType);
        }
        public virtual bool isAnyDirty()
        {
            return !(mDirtyBits.IsEmpty());
        }

        protected int miID = -1;
        protected int miTileID = -1;
        protected int miConstruction = 0;
        protected int miDamage = 0;

        protected bool mbWasDamaged = false;

        protected PlayerType meOwner = PlayerType.NONE;
        protected BuildingType meType = BuildingType.NONE;

        protected List<int> maiResourceCost = new List<int>();

        public ConstructionClient(GameClient pGame)
        {
            mGame = pGame;
        }

        protected virtual void SerializeClient(object stream, bool bAll)
        {
            SimplifyIO.Data(stream, ref mDirtyBits, "DirtyBits");

            if (isDirty(ConstructionDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref miID, "ID");
            }
            if (isDirty(ConstructionDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref miTileID, "TileID");
            }
            if (isDirty(ConstructionDirtyType.miConstruction) || bAll)
            {
                SimplifyIO.Data(stream, ref miConstruction, "Construction");
            }
            if (isDirty(ConstructionDirtyType.miDamage) || bAll)
            {
                SimplifyIO.Data(stream, ref miDamage, "Damage");
            }

            if (isDirty(ConstructionDirtyType.mbWasDamaged) || bAll)
            {
                SimplifyIO.Data(stream, ref mbWasDamaged, "WasDamage");
            }

            if (isDirty(ConstructionDirtyType.meOwner) || bAll)
            {
                SimplifyIO.Data(stream, ref meOwner, "Owner");
            }
            if (isDirty(ConstructionDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref meType, "Type");
            }

            if (isDirty(ConstructionDirtyType.maiResourceCost) || bAll)
            {
                SimplifyIO.Data(stream, ref maiResourceCost, (int)infos().resourcesNum(), "ResourceCost_");
            }
        }

        public virtual void writeClientValues(BinaryWriter stream, bool bAll)
        {
            SerializeClient(stream, bAll);
        }

        public virtual void readClientValues(BinaryReader stream, bool bAll)
        {
            SerializeClient(stream, bAll);
        }

        public virtual int getRepairResourceCost(ResourceType eResource)
        {
            return ((ownerClient().getBuildingResourceCost(getType(), eResource) * getDamage()) / 100) / 2;
        }

        public virtual int getRepairMoneyCost(bool bIgnoreStockpile = false)
        {
            int iCost = 0;

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                if (bIgnoreStockpile)
                {
                    iCost += ownerClient().getIgnoreStockpileCost(eLoopResource, getRepairResourceCost(eLoopResource));
                }
                else
                {
                    iCost += ownerClient().getNeededResourceCost(eLoopResource, getRepairResourceCost(eLoopResource));
                }
            }

            return iCost;
        }

        public virtual bool canAbandon()
        {
            if (infos().rulesSet(mGame.getRulesSet()).mbDisableScrapping)
            {
                return false;
            }

            if (!(tileClient().isOwnerReal()))
            {
                return false;
            }

            if (isDamaged())
            {
                return false;
            }

            if (isWasDamaged())
            {
                return false;
            }

            if (tileClient().isVirus())
            {
                return false;
            }
            
            if (gameClient().auctionTileClient() == tileClient())
            {
                return false;
            }

            return true;
        }

        public virtual bool canRepair(bool bTestMoney)
        {
            if (!isDamaged())
            {
                return false;
            }

            if (!(tileClient().isOwnerReal()))
            {
                return false;
            }

            if (tileClient().getUnitMissionCount(getOwner(), MissionType.REPAIR) > 0)
            {
                return false;
            }

            if (bTestMoney)
            {
                if (ownerClient().getMoney() < getRepairMoneyCost())
                {
                    return false;
                }
            }

            return true;
        }

        protected bool isFrozen()
        {
            return tileClient().isFrozen();
        }

    	public virtual int getID()
    	{
    		return miID;
    	}

        public virtual int getTileID()
        {
            return miTileID;
        }
        public virtual TileClient tileClient()
        {
            return gameClient().tileClient(getTileID());
        }
        public virtual int getX()
        {
            return tileClient().getX();
        }
        public virtual int getY()
        {
            return tileClient().getY();
        }
        public virtual Vector3 getWorldPosition()
        {
            return tileClient().getWorldPosition();
        }

        public virtual int getConstruction()
        {
            return miConstruction;
        }

        public virtual int getDamage()
        {
            return miDamage;
        }
        public virtual bool isDamaged()
        {
            return (getDamage() > 0);
        }

        public virtual bool isWasDamaged()
        {
            return mbWasDamaged;
        }

        public virtual PlayerType getOwner()
        {
            return meOwner;
        }
        public virtual PlayerClient ownerClient()
        {
            return gameClient().playerClient(getOwner());
        }
        public virtual TeamType getTeam()
        {
            return ownerClient().getTeam();
        }

        public virtual BuildingType getType()
        {
            return meType;
        }
        public virtual BuildingType getVisibleType(TeamType eTeam)
        {
            BuildingType eVisibleType = BuildingType.NONE;

            if (!(tileClient().showCorrectBuilding(eTeam, true)))
            {
                eVisibleType = tileClient().getVisibleBuilding();
            }

            if (eVisibleType == BuildingType.NONE)
            {
                eVisibleType = getType();
            }

            return eVisibleType;
        }
        public virtual BuildingClassType getClass()
        {
            return infos().building(getType()).meClass;
        }

        public virtual int getRate()
        {
            int iRate = Constants.CONSTRUCTION_RATE;

            if (tileClient().isDouble())
            {
                iRate *= 2;
            }

            if (tileClient().isHalf())
            {
                iRate /= 2;
            }

            iRate *= Math.Max(0, (infos().terrain(tileClient().getTerrain()).maiConstructionModifier[(int)gameClient().getLocation()] + 100));
            iRate /= 100;

            {
                EventStateType eEventStateGame = gameClient().getEventStateGameActive();

                if (eEventStateGame != EventStateType.NONE)
                {
                    iRate *= Math.Max(0, (infos().eventState(eEventStateGame).miConstructionModifier + 100));
                    iRate /= 100;
                }
            }

            {
                EventStateType eEventStateLevel = gameClient().getEventStateLevel();

                if (eEventStateLevel != EventStateType.NONE)
                {
                    iRate *= Math.Max(0, (infos().eventState(eEventStateLevel).miConstructionModifier + 100));
                    iRate /= 100;
                }
            }

            return Math.Max(1, iRate);
        }

        public virtual int getResourceCost(ResourceType eIndex)
        {
            return maiResourceCost[(int)eIndex];
        }
    }

    public class ConstructionServer : ConstructionClient
    {
        GameServer gameServer()
    	{
            return (GameServer)mGame;
    	}

        protected void makeDirty(ConstructionDirtyType eType)
        {
            mDirtyBits.SetBit((int)eType, true);
        }
        protected void makeAllDirty()
        {
            for (ConstructionDirtyType eLoopType = 0; eLoopType < ConstructionDirtyType.NUM_TYPES; eLoopType++)
            {
                makeDirty(eLoopType);
            }
        }
        public void clearDirty()
        {
            mDirtyBits.Clear();
        }

        public ConstructionServer(GameClient pGame)
            : base(pGame)
        {
        }

        public virtual void init(GameServer pGame, int iID, PlayerType eOwner, BuildingType eType, TileServer pTile, bool bSpend)
    	{
    		mGame = pGame;

    		miID = iID;
            miTileID = pTile.getID();

            meOwner = eOwner;
            meType = eType;

            maiResourceCost.Clear();
            for (ResourceType eResource = 0; eResource < infos().resourcesNum(); eResource++)
            {
                maiResourceCost.Add(ownerServer().getBuildingResourceCost(getType(), eResource));
            }

            tileServer().addConstruction(this);

            tileServer().realOwnerServer().changeRealConstructionCount(getType(), 1);

            tileServer().doVisibleBuilding();

            if (bSpend)
            {
                for (ResourceType eResource = 0; eResource < infos().resourcesNum(); eResource++)
                {
                    int iCost = ownerServer().getBuildingResourceCost(getType(), eResource);
                    if (iCost > 0)
                    {
                        ownerServer().spend(eResource, iCost);
                        gameServer().statsServer().changeStat(StatsType.RESOURCE, (int)ResourceStatType.CONSTRUCTION_CONSUMED, getOwner(), (int)eResource, (iCost * Constants.RESOURCE_MULTIPLIER));
                    }
                }
            }

            makeAllDirty();
        }

        public virtual void kill(bool bRefund)
        {
            tileServer().removeConstruction(this);

            tileServer().realOwnerServer().changeRealConstructionCount(getType(), -1);

            if (bRefund)
            {
                for (ResourceType eResource = 0; eResource < infos().resourcesNum(); eResource++)
                {
                    int iCost = getResourceCost(eResource);
                    if (iCost > 0)
                    {
                        ownerServer().changeWholeResourceStockpile(eResource, iCost, false);
                        gameServer().statsServer().changeStat(StatsType.RESOURCE, (int)ResourceStatType.CONSTRUCTION_CONSUMED, getOwner(), (int)eResource, -(iCost * Constants.RESOURCE_MULTIPLIER));
                    }
                }
            }
        }

        public virtual void abandon()
        {
            if (!canAbandon())
            {
                return;
            }

            ownerServer().killConstruction(this, true);
        }

        public virtual void repair()
        {
            if (!canRepair(true))
            {
                return;
            }

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                int iCost = getRepairResourceCost(eLoopResource);
                if (iCost > ownerServer().getWholeResourceStockpile(eLoopResource, true))
                {
                    ownerServer().trade(eLoopResource, (iCost - ownerServer().getWholeResourceStockpile(eLoopResource, true)), false);
                }
                ownerServer().changeWholeResourceStockpile(eLoopResource, -(iCost), true);
            }

            {
                HQServer pHQ = ownerServer().findClosestHQServer(tileServer());

                if (pHQ != null)
                {
                    UnitServer pUnit = ownerServer().createUnit(infos().Globals.REPAIR_UNIT, BuildingType.NONE, pHQ.tileServer());

                    if (pUnit != null)
                    {
                        pUnit.setMissionInfo(MissionType.REPAIR, getTileID(), false);
                    }
                }
            }
        }

        public virtual TileServer tileServer()
        {
            return (TileServer)tileClient();
        }

        public virtual bool incrementConstruction()
        {
            if (isFrozen())
            {
                return false;
            }

            if (isDamaged())
            {
                return false;
            }

            miConstruction += getRate();

            makeDirty(ConstructionDirtyType.miConstruction);

            if (getConstruction() < infos().building(getType()).miConstructionThreshold * 10)
            {
                return false;
            }

            finishConstruction();

            return true;
        }
        public virtual void startRepairConstruction()
        {
            miConstruction = Math.Max(0, ((infos().building(getType()).miConstructionThreshold * (100 - getDamage())) / 100)) * 10;

            setDamage(0);

            makeDirty(ConstructionDirtyType.miConstruction);
        }

        public virtual void finishConstruction()
        {
            ownerServer().killConstruction(this, false);

            bool classWasConstructed = gameServer().hasBuildingClassBeenConstructed(getType());
            ownerServer().createBuilding(getType(), tileServer(), !isWasDamaged(), this);
            bool classNowConstructed = gameServer().hasBuildingClassBeenConstructed(getType());

            bool bFirst = classNowConstructed && !classWasConstructed;

            gameServer().gameEventsServer().AddBuildingConstructed(getOwner(), getType(), tileServer().getID(), bFirst);

            if (bFirst && !(ownerServer().isHumanTeam()) && tileClient().isOwnerReal())
            {
                TextType eText = infos().character(ownerServer().getCharacter()).maeQuipBuildingClassFirst[(int)(infos().building(getType()).meClass)];

                if (eText != TextType.NONE)
                {
                    ownerServer().AI_quip(PlayerType.NONE, eText);
                }
            }
        }

        public virtual void setDamage(int iNewValue)
        {
            if (getDamage() != iNewValue)
            {
                miDamage = iNewValue;

                if (getDamage() > 0)
                {
                    makeWasDamaged();
                }

                makeDirty(ConstructionDirtyType.miDamage);
            }
        }

        public virtual void makeWasDamaged()
        {
            if (!isWasDamaged())
            {
                mbWasDamaged = true;

                makeDirty(ConstructionDirtyType.mbWasDamaged);
            }
        }

        public virtual PlayerServer ownerServer()
        {
            return (PlayerServer)ownerClient();
        }
        public virtual void setOwner(PlayerType eNewValue)
        {
            PlayerType eOldValue = getOwner();

            if (eOldValue != eNewValue)
            {
                if (eOldValue != PlayerType.NONE)
                {
                    ownerServer().removeConstruction(this);
                }

                meOwner = eNewValue;

                if (eNewValue != PlayerType.NONE)
                {
                    ownerServer().addConstruction(getID());
                }

                makeDirty(ConstructionDirtyType.meOwner);
            }
        }

        public virtual void setResourceCost(ResourceType eIndex, int iNewValue)
        {
            if (getResourceCost(eIndex) != iNewValue)
            {
                maiResourceCost[(int)eIndex] = iNewValue;

                makeDirty(ConstructionDirtyType.maiResourceCost);
            }
        }
    }
}