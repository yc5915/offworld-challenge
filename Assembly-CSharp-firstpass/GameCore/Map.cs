using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Offworld.SystemCore;

namespace Offworld.GameCore
{

    public class MapClient
    {
        protected bool mbDirtyTiles = false;
        public virtual bool isDirtyTiles()
        {
            return mbDirtyTiles;
        }

        protected int miMapLatitude = 0;

        protected List<TileClient> maTiles = new List<TileClient>();
        protected List<TerrainInfo> maTerrainInfos = new List<TerrainInfo>();
        protected List<int> maiTerrainPlateauThicknesses = new List<int>();
        protected int[,] maAdjacentTileIDs = null; //[tileID, direction]
        protected List<int> [,] maRingTileIDs = null; //[tileID, radius]
        protected int[,] maStartingResources = null;
        protected bool[] maIsGeothermal = null;

        protected GameClient mGame = null;
        protected Infos mInfo = null;
        protected GameSettings mSettings = null;

        protected bool mbTerrainIsSet = false;
        protected bool mbHasResourceInfo = false;

        protected string mzMapName = "";
        protected string mzRequiredMod = "";

        public MapClient(GameClient pGame)
        {
            mGame = pGame;
        }

        public virtual void initClient(GameClient pGame)
        {
            mGame = pGame;
            mInfo = pGame.infos();
            mSettings = pGame.gameSettings();

            if (!mbTerrainIsSet)
            {
                SetTerrain();
            }
        }

        public virtual void initClient(Infos pInfo, GameSettings pSettings)
        {
            mInfo = pInfo;
            mSettings = pSettings;

            if (!mbTerrainIsSet)
            {
                SetTerrain();
            }
        }

        protected virtual void SetTerrain()
        {
            maStartingResources = new int[numTerrains(), (int)Globals.Infos.resourcesNum()];
            maIsGeothermal = new bool[numTerrains()];

            TerrainType DEFAULT_TERRAIN = mInfo.Globals.DEFAULT_TERRAIN;

            for (int i = 0; i < numTerrains(); i++)
            {
                maTerrainInfos.Add(new TerrainInfo(DEFAULT_TERRAIN, HeightType.NONE, WindType.NONE, IceType.NONE));
            }

            mbTerrainIsSet = true;
        }

        public virtual void initClientTiles()
        {
            maTiles = new List<TileClient>();

            for (int i = 0; i < numTiles(); i++)
            {
                maTiles.Add(Globals.Factory.createTileClient(mGame));
            }
        }

        //**********************************//
        //**** Variable fetch functions ****//
        //**********************************//

        public void setMapLatitude(LatitudeType eNewValue, GameServer mGame)
        {
            
            int iRange = infos().latitude(eNewValue).miRange;
            int iValue = 0;
            if (mGame != null)
            {
                iValue = mGame.random().Next(iRange);
            }
            else
            {
                iValue = (int)(Random.value * iRange);
            }

            setMapLatitude(infos().latitude(eNewValue).miBase + iValue);
        }

        public void setMapLatitude(int i)
        {
            miMapLatitude = i;
        }

        public virtual int getMapLatitude()
        {
            return miMapLatitude;
        }

        public virtual int getLatitude(int iY)
        {
            int iMapLatitude = getMapLatitude();

            iMapLatitude += ((iY - (getTerrainLength() / 2)) * 30) / (getTerrainLength() / 2);

            return Utils.clamp(iMapLatitude, 0, 180);
        }

        public virtual void setHasResourceInfo(bool pEnabled)
        {
            mbHasResourceInfo = pEnabled;
        }

        public virtual bool getHasResourceInfo()
        {
            return mbHasResourceInfo;
        }

        public virtual void setGeothermal (int terrainID, bool value)
        {
            if (tileIDfromTerrainInfoID(terrainID) < 0)
                return;

            maIsGeothermal[terrainID] = value;
        }

        public virtual bool isGeothermal (int terrainID)
        {
            if (tileIDfromTerrainInfoID(terrainID) < 0)
                return false;

            return maIsGeothermal[terrainID];
        }

        public virtual void setStartingResourceLevel(int terrainID, int pResourceType, int pResourceLevel)
        {
            if (tileIDfromTerrainInfoID(terrainID) >= 0)
            {
                maStartingResources[terrainID, pResourceType] = pResourceLevel;
            }
        }

        public virtual int getStartingResourceLevel(int terrainID, int pResourceType)
        {
            return maStartingResources[terrainID, pResourceType];
        }

        public virtual void setMapName(string mapName)
        {
            mzMapName = mapName;
        }

        public virtual string getMapName()
        {
            return mzMapName;
        }

        public virtual void setRequiredMod(string modName)
        {
            mzRequiredMod = modName;
        }

        public virtual string getRequiredMod()
        {
            return mzRequiredMod;
        }

        public virtual List<TerrainInfo> terrainInfoAll()
        {
            return maTerrainInfos;
        }
        public virtual List<TileClient> tileClientAll()
        {
            return maTiles;
        }

        public virtual List<int> tileRingIds(int iCenterTileID, int iRadius)
        {
            if((iRadius >= 0) && (iRadius < maRingTileIDs.GetLength(1))) //use cached value
            {
                return maRingTileIDs[iCenterTileID, iRadius];
            }
            else //calculate new value
            {
                using (new UnityProfileScope("Map.calculateTileRingIds"))
                {
                    return calculateTileRingIds(iCenterTileID, iRadius);
                }
            }
        }

        public virtual IEnumerable<TileClient> tileClientAdjacentRadial(TileClient startTile)
        {
            Queue<int> checkTileIDs = new Queue<int>();
            HashSet<int> visitedTiles = new HashSet<int>();
            checkTileIDs.Enqueue(startTile.getID());
            visitedTiles.Add(startTile.getID());

            while (checkTileIDs.Count > 0)
            {
                int tileID = checkTileIDs.Dequeue();
                TileClient tile = tileClient(tileID);

                yield return tile;

                for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                {
                    TileClient adjacentTile = tileClientAdjacent(tile, eLoopDirection);

                    if (adjacentTile != null && !visitedTiles.Contains(adjacentTile.getID()))
                    {
                        checkTileIDs.Enqueue(adjacentTile.getID());
                        visitedTiles.Add(adjacentTile.getID());
                    }
                }
            }
        }

        public virtual Infos infos()
        {
            return mInfo;
        }
        public virtual int getSeed()
        {
            return mSettings.miSeed;
        }
        public virtual TerrainClassType getTerrainClass()
        {
            return mSettings.meTerrainClass;
        }
        public virtual InfoTerrainClass infoTerrainClass(TerrainClassType pType)
        {
            return mInfo.terrainClass(pType);
        }
        public virtual MapSizeType getMapSize()
        {
            return mSettings.meMapSize;
        }
        public virtual LocationType getLocation()
        {
            return mSettings.meLocation;
        }

        public virtual int numTiles()
        {
            return mSettings.miWidth * mSettings.miHeight;
        }
        public virtual int getMapWidth()
        {
            return mSettings.miWidth;
        }
        public virtual int getMapLength()
        {
            return mSettings.miHeight;
        }
        public virtual int getMapEdgeTilePadding()
        {
            return mSettings.miEdgeTilePadding;
        }

        public virtual int numTerrains()
        {
            return getTerrainWidth() * getTerrainLength();
        }
        public virtual int getTerrainWidth()
        {
            return mSettings.miWidth + 2 * mSettings.miEdgeTilePadding;
        }
        public virtual int getTerrainLength() // previously "Height" not "Length"
        {
            return mSettings.miHeight + 2 * mSettings.miEdgeTilePadding;
        }


        //***********************************//
        //**** Coordinate math functions ****//
        //***********************************//

        public virtual int getTileX(int iID)
        {
            if ((iID < 0) || (iID >= numTiles()))
            {
                return -1;
            }
            return iID % mSettings.miWidth;
        }

        public virtual int getTileY(int iID)
        {
            if ((iID < 0) || (iID >= numTiles()))
            {
                return -1;
            }
            return iID / mSettings.miWidth;
        }

        public virtual int tileIDGrid(int iX, int iY)
        {
            if ((iX < 0) || (iX >= mSettings.miWidth) ||
                (iY < 0) || (iY >= mSettings.miHeight))
            {
                return -1;
            }

            return iX + (iY * mSettings.miWidth);
        }

        public virtual int tileIDfromTerrainInfoID(int iIndex)
        {
            int iX = (iIndex % getTerrainWidth()) - getMapEdgeTilePadding();
            int iY = (iIndex / getTerrainWidth()) - getMapEdgeTilePadding();
            return tileIDGrid(iX, iY);
        }

        public virtual int terrainInfoIDfromTileID(int iIndex)
        {
            int iX = (iIndex % getMapWidth()) + getMapEdgeTilePadding();
            int iY = (iIndex / getMapWidth()) + getMapEdgeTilePadding();
            return terrainInfoIDGrid(iX, iY);
        }

        public virtual int getTerrainInfoX(int iID)
        {
            if ((iID < 0) || (iID >= numTerrains()))
            {
                return -1;
            }
            return iID % getTerrainWidth();
        }

        public virtual int getTerrainInfoY(int iID)
        {
            if ((iID < 0) || (iID >= numTerrains()))
            {
                return -1;
            }
            return iID / getTerrainWidth();
        }

        public virtual int terrainInfoIDGrid(int iX, int iY)
        {
            if ((iX < 0) || (iX >= getTerrainWidth()) ||
                (iY < 0) || (iY >= getTerrainLength()))
            {
                return -1;
            }

            return iX + (iY * getTerrainWidth());
        }

        public virtual int terrainInfoIDRange(int iID, int iDX, int iDY, int iRange)
        {
            int iX = getTerrainInfoX(iID);
            int iY = getTerrainInfoY(iID);

            int iNewX = iX + iDX;
            int iNewY = iY + iDY;

            if (Utils.stepDistance(iX, iY, iNewX, iNewY) > iRange)
            {
                return -1;
            }
            else
            {
                return terrainInfoIDGrid(iNewX, iNewY);
            }
        }

        //*********************************//
        //**** Array get/set functions ****//
        //*********************************//

        public virtual List<TileClient> getTileArray()
        {
            return maTiles;
        }

        public virtual void setTileArray(List<TileClient> paTiles)
        {
            maTiles = paTiles;
        }

        public virtual List<TerrainInfo> getTerrainArray()
        {
            return maTerrainInfos;
        }

        public virtual void setTerrainArray(List<TerrainInfo> paTerrains)
        {
            maTerrainInfos = paTerrains;
            mbTerrainIsSet = true;
        }

        public virtual List<int> getThicknessArray()
        {
            return maiTerrainPlateauThicknesses;
        }

        public virtual void setThicknessArray(List<int> paiThicknesses)
        {
            maiTerrainPlateauThicknesses = paiThicknesses;
        }

        //********************************//
        //**** Object fetch functions ****//
        //********************************//

        public virtual TileClient tileClient(int iIndex)
        {
            if ((iIndex < 0) || (iIndex >= numTiles()))
            {
                return null;
            }

            return maTiles[iIndex];
        }

        public virtual TileClient tileClientGrid(int iX, int iY)
        {
            return tileClient(tileIDGrid(iX, iY));
        }

        public virtual TileClient tileClientAdjacent(TileClient tile, DirectionType eDirection)
        {
            int adjacentTileID = maAdjacentTileIDs[tile.getID(), (int)eDirection];
            return tileClient(adjacentTileID);
        }

        public virtual int[,] tileAdjacentIds()
        {
            return maAdjacentTileIDs;
        }

        public virtual TerrainInfo terrainInfo(int iIndex)
        {
            if ((iIndex < 0) || (iIndex >= numTerrains()))
            {
                return null;
            }

            return maTerrainInfos[iIndex];
        }

        public virtual TerrainInfo terrainInfoGrid(int iX, int iY)
        {
            return terrainInfo(terrainInfoIDGrid(iX, iY));
        }

        public virtual int terrainInfoAdjacent(int iX, int iY, DirectionType eDirection)
        {
            return terrainInfoIDGrid(iX + Utils.directionOffsetX(eDirection, iY), iY + Utils.directionOffsetY(eDirection));
        }

        public virtual int terrainInfoIDAdjacent(int iID, DirectionType eDirection)
        {
            return terrainInfoAdjacent(getTerrainInfoX(iID), getTerrainInfoY(iID), eDirection);
        }

        public virtual GameSettings settings()
        {
            return mSettings;
        }

        //*********************************//
        //**** Miscellaneous functions ****//
        //*********************************//

        public virtual void UpdateAdjacency()
        {
            using (new UnityProfileScope("MapClient.UpdateAdjacencyCache"))
            {
                maAdjacentTileIDs = new int[numTiles(), (int)DirectionType.NUM_TYPES];
                for (int i = 0; i < maAdjacentTileIDs.GetLength(0); i++)
                {
                    int centerX = getTileX(i);
                    int centerY = getTileY(i);
                    for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
                    {
                        int newX = centerX + Utils.directionOffsetX(eDirection, centerY);
                        int newY = centerY + Utils.directionOffsetY(eDirection);
                        maAdjacentTileIDs[i, (int)eDirection] = tileIDGrid(newX, newY);
                    }
                }
            }

            using (new UnityProfileScope("MapClient.UpdateRingCache"))
            {
                int cMAX_CACHE_RADIUS = 8; //this covers all of the base game search radii
                maRingTileIDs = new List<int>[numTiles(), cMAX_CACHE_RADIUS];
                for(int iCenterTileID=0; iCenterTileID<numTiles(); iCenterTileID++)
                {
                    for(int iRadius=0; iRadius<cMAX_CACHE_RADIUS; iRadius++)
                    {
                        maRingTileIDs[iCenterTileID, iRadius] = calculateTileRingIds(iCenterTileID, iRadius);
                    }
                }
            }
        }

        protected virtual List<int> calculateTileRingIds(int iCenterTileID, int iRadius)
        {
            //using (new UnityProfileScope("Map.calculateTileRingIds"))
            {
                List<int> results = new List<int>(6 * Mathf.Max(iRadius, 0));
                if(iRadius == 0) //trivial case
                {
                    results.Add(iCenterTileID);
                }
                else //traverse ring
                {
                    //move West to find starting point on ring
                    int tileX = getTileX(iCenterTileID) - iRadius;
                    int tileY = getTileY(iCenterTileID);

                    //traverse ring clockwise starting NE
                    for(int i=0; i<6; i++)
                    {
                        int direction = ((int)DirectionType.NE + i) % 6;
                        for(int j=0; j<iRadius; j++)
                        {
                            tileX += ((tileY & 1) == 0) ? Utils.DIRECTION_OFFSET_X_EVEN[direction] : Utils.DIRECTION_OFFSET_X_ODD[direction];
                            tileY += Utils.DIRECTION_OFFSET_Y[direction];
                            int tileID = tileIDGrid(tileX, tileY);
                            if(tileID >= 0)
                            {
                                results.Add(tileID);
                            }
                        }
                    }
                }

                return results;
            }
        }

        public virtual float getPlateauThickness(HeightType eHeight) // previously getTerrainThickness
        {
            if ((int)eHeight < 0 || (int)eHeight >= (int)infos().heightsNum())
            {
                Debug.LogError("Tried to request thickness of height " + ((int)eHeight).ToString());
                return 1f;
            }

            return (float)maiTerrainPlateauThicknesses[(int)eHeight] / (float)Constants.TERRAIN_THICKNESS_MULTIPLIER;
        }

        public virtual float getPlateauHeight(HeightType eHeight)
        {
            if (eHeight == 0)
            {
                return -10f;
            }
            else
            {
                float result = 0f;
                for (HeightType eLoopHeight = 0; eLoopHeight < eHeight; eLoopHeight++)
                {
                    result += getPlateauThickness(eLoopHeight) * 5f;
                }
                return result;
            }
        }

        public virtual void randomizeTerrainThicknesses()
        {
            maiTerrainPlateauThicknesses = new List<int>();

            maiTerrainPlateauThicknesses.Add(Constants.TERRAIN_THICKNESS_MULTIPLIER);

            float[] sources = new float[3] { 0.70f, 0.98f, 1.2f };
            List<int> values = new List<int>();

            for (int i = 1; i < (int)infos().heightsNum(); i++)
            {
                if (values.Count == 0)
                {
                    values.Add(0);
                    values.Add(1);
                    values.Add(2);
                }

                int rand;
                GameServer gGame = (GameServer)mGame;
                if (gGame != null)
                    rand = gGame.random().Next(values.Count);
                else
                    rand = Mathf.FloorToInt(Mathf.Clamp(Random.value * (float)values.Count, 0f, (float)(values.Count - 1)));

                maiTerrainPlateauThicknesses.Add((int)(sources[values[rand]] * Constants.TERRAIN_THICKNESS_MULTIPLIER));
                values.RemoveAt(rand);
            }
        }

        public virtual void writeTerrainXML(string fullFilePath, int[] craterGroupIDs = null)
        {
            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.Indent = true;
            xmlSettings.NewLineOnAttributes = true;

            using (XmlWriter writer = XmlWriter.Create(fullFilePath, xmlSettings))
            {
                writer.WriteStartDocument();

                writer.WriteStartElement("Root");

                writer.WriteAttributeString("MapName", mzMapName.ToString());

                writer.WriteAttributeString("UsesTypes", true.ToString());

                // required attributes
                writer.WriteAttributeString("MapWidth", mSettings.miWidth.ToString());
                writer.WriteAttributeString("MapLength", mSettings.miHeight.ToString());
                writer.WriteAttributeString("MapEdgeTilePadding", mSettings.miEdgeTilePadding.ToString());

                // optional attributes
                if (getTerrainClass() != TerrainClassType.NONE)
                    writer.WriteAttributeString("MapClass", infos().terrainClass(getTerrainClass()).mzType);
                if (getMapSize() != MapSizeType.NONE)
                    writer.WriteAttributeString("MapSizeType", infos().mapSize(getMapSize()).mzType);

                writer.WriteAttributeString("RequiredMod", mzRequiredMod.ToString());

                writer.WriteAttributeString("LocationType", infos().location(getLocation()).mzType);

                writer.WriteAttributeString("HasResourceInfo", mbHasResourceInfo.ToString());


                writer.WriteAttributeString("MapLatitude", getMapLatitude().ToString());

                for (int i = 0; i < numTerrains(); i++)
                {
                    writer.WriteStartElement("tInfo");
                    writer.WriteAttributeString("ID", i.ToString());

                    writer.WriteElementString("Terrain", infos().terrain(terrainInfo(i).Terrain).mzType);
                    writer.WriteElementString("Height", infos().height(terrainInfo(i).Height).mzType);

                    if (terrainInfo(i).IsCrater > 0)
                    {
                        writer.WriteElementString("IsCrater", terrainInfo(i).IsCrater.ToString());
                    }

                    if (terrainInfo(i).CraterChunkID >= 0)
                    {
                        writer.WriteElementString("CraterID", terrainInfo(i).CraterChunkID.ToString());
                        writer.WriteElementString("CraterDir", ((int)terrainInfo(i).CraterChunkDir).ToString());
                    }

                    if (terrainInfo(i).Ice != IceType.NONE)
                    {
                        writer.WriteElementString("IceType", infos().ice(terrainInfo(i).Ice).mzType);
                    }

                    if (craterGroupIDs != null)
                    {
                        if (craterGroupIDs[i] >= 0)
                        {
                            writer.WriteElementString("CraterGroupID", craterGroupIDs[i].ToString());
                        }
                    }

                    int iTileID = tileIDfromTerrainInfoID(i);
                    if (iTileID >= 0)
                    {
                        if ((mGame == null) ? maIsGeothermal[i] : mGame.tileClient(iTileID).isGeothermal())
                        {
                            writer.WriteStartElement("IsGeothermal");
                            writer.WriteValue(2);
                            writer.WriteEndElement();
                        }
                        else
                        {
                            for (ResourceType eLoopResource = 0; eLoopResource < Globals.Infos.resourcesNum(); eLoopResource++)
                            {
                                ResourceLevelType eResourceLevel = (mGame == null) ? (ResourceLevelType)maStartingResources[i, (int)eLoopResource] : mGame.tileClient(iTileID).getResourceLevel(eLoopResource, false);
                                if (eResourceLevel > ResourceLevelType.NONE)
                                {
                                    writer.WriteStartElement("Resource");
                                    writer.WriteAttributeString("Type", infos().resource(eLoopResource).mzType);
                                    writer.WriteAttributeString("Level", infos().resourceLevel(eResourceLevel).mzType);
                                    writer.WriteValue(1);
                                    writer.WriteEndElement();
                                }
                            }
                        }

                    }

                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                writer.WriteEndDocument();
            }
        }

        public static XmlDocument openTerrainXML(GameSettings pSettings, Infos infos)
        {
            using (new UnityProfileScope("Map.openTerrainXML"))
            {
                try
                {
                    XmlDocument xmlDoc = null;
                    if (File.Exists(pSettings.mzMap)) //try loading loose file
                    {
                        if (Path.GetExtension(pSettings.mzMap) == ".mapz") //gzip compressed map
                        {
                            xmlDoc = new XmlDocument();
                            using (FileStream source = FileUtilities.OpenRead(pSettings.mzMap))
                            using (MemoryStream destination = new MemoryStream())
                            {
                                using (new UnityProfileScope("Map.DecompressGZip"))
                                {
                                    CompressionUtilities.GZipDecompress(source, destination);
                                }

                                destination.Position = 0; //start at beginning of uncompressed file

                                using (new UnityProfileScope("Map.LoadXml"))
                                {
                                    xmlDoc.Load(destination);
                                }
                            }
                        }
                        else
                        {
                            xmlDoc = new XmlDocument();
                            xmlDoc.Load(pSettings.mzMap);
                        }
                    }
                    else //otherwise, load moddable file
                    {
                        xmlDoc = infos.getModdableXML(pSettings.mzMap, ".map");
                    }

                    if (xmlDoc == null)
                    {
                        Debug.LogWarning("Failed to load map resource: " + pSettings.mzMap);
                        return null;
                    }

                    loadSettingsFromXML(xmlDoc, pSettings, infos);
                    return xmlDoc;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Failed to load map resource: " + pSettings.mzMap);
                    Debug.LogException(ex);
                    return null;
                }
            }
        }

        public static void loadSettingsFromXML(XmlDocument xmlDoc, GameSettings pSettings, Infos infos)
        {
            using (new UnityProfileScope("Map.loadSettingsFromXML"))
            {
                XmlNode rootNode = xmlDoc["Root"];

                XmlNode usesTypes = rootNode.Attributes.GetNamedItem("UsesTypes");
                bool bUsesTypes = (usesTypes != null) ? bool.Parse(usesTypes.Value) : false;

                XmlNode mapWidthAttribute = rootNode.Attributes.GetNamedItem("MapWidth");
                XmlNode mapLengthAttribute = rootNode.Attributes.GetNamedItem("MapLength");
                XmlNode mapEdgeTilePaddingAttribute = rootNode.Attributes.GetNamedItem("MapEdgeTilePadding");
                XmlNode mapClassAttribute = rootNode.Attributes.GetNamedItem("MapClass");
                XmlNode mapSizeTypeAttribute = rootNode.Attributes.GetNamedItem("MapSizeType");
                XmlNode locationTypeAttribute = rootNode.Attributes.GetNamedItem("LocationType");
                XmlNode mapNameAttribute = rootNode.Attributes.GetNamedItem("MapName");

                // required attributes
                pSettings.miWidth = int.Parse(mapWidthAttribute.Value);
                pSettings.miHeight = int.Parse(mapLengthAttribute.Value);
                pSettings.miEdgeTilePadding = int.Parse(mapEdgeTilePaddingAttribute.Value);
                pSettings.mzMapName = mapNameAttribute != null ? mapNameAttribute.Value : "";

                // optional attributes
                if (bUsesTypes)
                {
                    pSettings.meTerrainClass = infos.getType<TerrainClassType>(mapClassAttribute.Value);
                    pSettings.meMapSize = infos.getType<MapSizeType>(mapSizeTypeAttribute.Value);
                    pSettings.meLocation = infos.getType<LocationType>(locationTypeAttribute.Value);
                }
                else
                {
                    pSettings.meTerrainClass = (mapClassAttribute != null) ? (TerrainClassType)int.Parse(mapClassAttribute.Value) : 0;
                    pSettings.meMapSize = (mapSizeTypeAttribute != null) ? (MapSizeType)int.Parse(mapSizeTypeAttribute.Value) : infos.Globals.DEFAULT_MAPSIZE;
                    pSettings.meLocation = (locationTypeAttribute != null) ? (LocationType)int.Parse(locationTypeAttribute.Value) : LocationType.MARS;
                }
            }
        }

        public virtual int[] readTerrainXML(GameSettings pSettings) // the returned int array is info on which tiles are anchors for crater groups
        {
            using (new UnityProfileScope("Map.readTerrainXML"))
            {
                XmlDocument xmlDoc = openTerrainXML(pSettings, infos());
                return readTerrainXML(xmlDoc, pSettings);
            }
        }

        public virtual int[] readTerrainXML(XmlDocument xmlDoc, GameSettings pSettings)
        {
            using (new UnityProfileScope("Map.readTerrainXML2"))
            {
                loadSettingsFromXML(xmlDoc, pSettings, infos());
                mbTerrainIsSet = false;
                initClient(infos(), pSettings);

                int[] craterGroups;

                {
                    XmlNode rootNode = xmlDoc["Root"];
                    XmlNode mapName = rootNode.Attributes.GetNamedItem("MapName");
                    XmlNode mapLatitudeAttribute = rootNode.Attributes.GetNamedItem("MapLatitude");
                    XmlNode mapHasResourceInfo = rootNode.Attributes.GetNamedItem("HasResourceInfo");
                    XmlNode mapRequiredMod = rootNode.Attributes.GetNamedItem("RequiredMod");

                    if (mapName != null)
                        mzMapName = mapName.Value;
                    if (mapLatitudeAttribute != null)
                        miMapLatitude = int.Parse(mapLatitudeAttribute.Value);
                    if (mapHasResourceInfo != null)
                        mbHasResourceInfo = bool.Parse(mapHasResourceInfo.Value);
                    if (mapRequiredMod != null)
                        mzRequiredMod = mapRequiredMod.Value;

                    XmlNode usesTypes = rootNode.Attributes.GetNamedItem("UsesTypes");
                    bool bUsesTypes = (usesTypes != null) ? bool.Parse(usesTypes.Value) : false;

                    craterGroups = new int[numTerrains()];
                    for (int i = 0; i < craterGroups.Length; i++)
                    {
                        craterGroups[i] = -1;
                    }

                    XmlNodeList nodes = xmlDoc.SelectNodes("Root/tInfo");
                    foreach (XmlNode node in nodes)
                    {
                        int id = int.Parse(node.Attributes.GetNamedItem("ID").Value);

                        TerrainType eTerrain = TerrainType.NONE;
                        if (bUsesTypes)
                        {
                            eTerrain = infos().readType<TerrainType>(node, "Terrain");
                        }
                        else
                        {
                            int terrainInt = 0;
                            infos().readInt(node, "Terrain", ref terrainInt);
                            eTerrain = (TerrainType)terrainInt;
                        }

                        HeightType eHeight = HeightType.NONE;
                        if (bUsesTypes)
                        {
                            eHeight = infos().readType<HeightType>(node, "Height");
                        }
                        else
                        {
                            int heightInt = 0;
                            infos().readInt(node, "Height", ref heightInt);
                            eHeight = (HeightType)heightInt;
                        }

                        TerrainInfo tInfo = terrainInfo(id);

                        foreach(XmlNode childNode in node.ChildNodes)
                        {
                            if (childNode.Name == "IsCrater")
                            {
                                int whyisthisaref = 0;
                                infos().readInt(node, "IsCrater", ref whyisthisaref);
                                tInfo.IsCrater = whyisthisaref;
                            }
                            else if (childNode.Name == "CraterID")
                            {
                                int whyisthisaref = 0;
                                infos().readInt(node, "CraterID", ref whyisthisaref);
                                tInfo.CraterChunkID = whyisthisaref;
                            }
                            else if (childNode.Name == "CraterDir")
                            {
                                int whyisthisaref = 0;
                                infos().readInt(node, "CraterDir", ref whyisthisaref);
                                tInfo.CraterChunkDir = (DirectionType)whyisthisaref;
                            }
                            else if (childNode.Name == "CraterGroupID")
                            {
                                int whyisthisaref = 0;
                                infos().readInt(node, "CraterGroupID", ref whyisthisaref);
                                craterGroups[id] = whyisthisaref;
                            }
                            else if (childNode.Name == "IceType")
                            {
                                if (bUsesTypes)
                                {
                                    tInfo.Ice = infos().readType<IceType>(node, "IceType");
                                }
                                else
                                {
                                    int whyisthisaref = 0;
                                    infos().readInt(node, "IceType", ref whyisthisaref);
                                    tInfo.Ice = (IceType)whyisthisaref;
                                }
                            }
                            else if (childNode.Name == "IsGeothermal")
                            {
                                maIsGeothermal[id] = true;
                            }
                            else if (childNode.Name == "Resource")
                            {
                                if (tileIDfromTerrainInfoID(id) >= 0)
                                {
                                    if (bUsesTypes)
                                    {
                                        XmlNode resourceType = childNode.Attributes.GetNamedItem("Type");
                                        ResourceType eResource = infos().getType<ResourceType>(resourceType.Value);
                                        XmlNode resourceLevel = childNode.Attributes.GetNamedItem("Level");
                                        ResourceLevelType eResourceLevel = infos().getType<ResourceLevelType>(resourceLevel.Value);
                                        maStartingResources[id, (int)eResource] = (int)eResourceLevel;
                                    }
                                    else
                                    {
                                        XmlNode resourceType = childNode.Attributes.GetNamedItem("Type");
                                        int peType = int.Parse(resourceType.Value);
                                        XmlNode resourceLevel = childNode.Attributes.GetNamedItem("Level");
                                        int peLevel = int.Parse(resourceLevel.Value);
                                        maStartingResources[id, peType] = peLevel;
                                    }
                                }
                            }
                        }
                        tInfo.Terrain = eTerrain;
                        tInfo.Height = eHeight;
                    }
                }
                //        string craterLog = "CraterGroups ";
                //        for (int i = 0; i < craterGroups.Length; i++)
                //        {
                //            if (craterGroups[i] >= 0)
                //            craterLog += craterGroups[i].ToString() + " ";
                //        }
                //        Debug.Log(craterLog);
                return craterGroups;
            }
        }
    }

    public class MapServer : MapClient
    {
        public virtual void makeDirtyTiles()
        {
            mbDirtyTiles = true;
        }
        public virtual void clearDirtyTiles()
        {
            mbDirtyTiles = false;
        }

        public MapServer(GameClient pGame)
            : base(pGame)
        {
        }

        public virtual void init(GameServer pGame)
        {
            using (new UnityProfileScope("MapServer.init"))
            {
                mGame = pGame;
                mInfo = pGame.infos();

                TerrainType DEFAULT_TERRAIN;
                if (mGame != null)
                {
                    DEFAULT_TERRAIN = mGame.infos().Globals.DEFAULT_TERRAIN;
                    mSettings = mGame.gameSettings();
                }
                else
                {
                    DEFAULT_TERRAIN = mInfo.Globals.DEFAULT_TERRAIN;
                }

                for (int i = 0; i < numTerrains(); i++)
                {
                    maTerrainInfos.Add(new TerrainInfo(DEFAULT_TERRAIN, HeightType.NONE, WindType.NONE, IceType.NONE));
                }
            }
        }

        public virtual void initTiles()
        {
            using (new UnityProfileScope("MapServer.initTiles"))
            {
                for (int i = 0; i < numTiles(); i++)
                {
                    TileServer pLoopTile = Globals.Factory.createTileServer(mGame);
                    pLoopTile.init((GameServer)mGame, i);
                    maTiles.Add(pLoopTile);
                }

                makeDirtyTiles();
            }
        }

        public virtual TileServer tileServer(int iIndex)
        {
            return (TileServer)tileClient(iIndex);
        }
    }
}