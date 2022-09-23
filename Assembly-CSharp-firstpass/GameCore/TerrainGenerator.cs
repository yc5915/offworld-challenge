using UnityEngine;
using UnityEngine.Assertions;
using System.Xml;
using System.Collections.Generic;
using System;
using System.Linq;
using Offworld.SystemCore;

namespace Offworld.GameCore
{
    public struct TerrainCraterPreset
    {
        public int width;
        public int weighting;
        public bool[] slopes;
        public bool[] centerTiles;
        public int[] craterChunkIDs;
        public int[] craterChunkDirections;
    }

    public class TerrainGenerator
    {
        public const int RESOURCE_DIE_SIZE = 10000;

        public void readTerrain(GameServer game)
        {
            MapClient map = (MapClient)game.mapClient();
            map.readTerrainXML(game.gameSettings());
        }

        public TerrainCraterPreset[] ReadCraterPresetXML(MapClient map)
        {
            using (new UnityProfileScope("TerrainGenerator.ReadCraterPresetXML"))
            {
                TextAsset textXML = Resources.Load<TextAsset>("Data/crater-presets.xml");
                Assert.IsNotNull(textXML, "Failed to load resource: CraterPresets");
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(textXML.text);

                XmlNodeList nodes = xmlDoc.SelectNodes("CraterPresets/Entry");
                TerrainCraterPreset[] presets = new TerrainCraterPreset[nodes.Count];
                foreach (XmlNode node in nodes)
                {
                    int id = Convert.ToInt32(readString(node, "id"));
                    int width = Convert.ToInt32(readString(node, "width"));
                    int weighting = Convert.ToInt32(readString(node, "weighting"));

                    string centerStr = readString(node, "centerTiles");
                    string slopeStr = readString(node, "slopes");
                    string chunkIDStr = readString(node, "chunkID");
                    string chunkDirStr = readString(node, "chunkDir");

                    int size = centerStr.Length;

                    presets[id] = new TerrainCraterPreset();
                    presets[id].width = width;
                    presets[id].weighting = weighting;

                    presets[id].centerTiles = new bool[size];
                    presets[id].slopes = new bool[size];
                    presets[id].craterChunkIDs = new int[size];
                    presets[id].craterChunkDirections = new int[size];

                    for (int ch = 0; ch < size; ch++)
                    {
                        string centerChar = centerStr.Substring(ch, 1);
                        string slopeChar = slopeStr.Substring(ch, 1);
                        string chunkIDChar = chunkIDStr.Substring(ch * 4, 3);
                        string chunkDirChar = chunkDirStr.Substring(ch * 2, 1);

                        presets[id].centerTiles[ch] = (centerChar == "X");
                        presets[id].slopes[ch] = (slopeChar == "X");
                        presets[id].craterChunkIDs[ch] = Convert.ToInt32(chunkIDChar) - 200;
                        presets[id].craterChunkDirections[ch] = Convert.ToInt32(chunkDirChar) - 1;
                    }
                }
                // hacky fix for different crater options between Ceres and other locations (others are missing the last 3 crater options)
                if (map.getLocation() != LocationType.CERES) {
                    TerrainCraterPreset[] reducedSet = new TerrainCraterPreset[presets.Length - 3];
                    for (int i = 0; i < reducedSet.Length; i++) {
                        reducedSet[i] = presets[i];
                    }
                    presets = reducedSet;
                }
                
                return presets;
            }
        }

        public string readString(XmlNode node, string zName)
        {
            try
            {
                return Infos.FindChild(node, zName).InnerText;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed Infos.readString() reading " + zName);
                Debug.LogException(e);
            }

            return "";
        }

        public void generateRandomTerrain(GameServer game)
        {
            generateRandomTerrain(game, null);
        }

        public int getEquatorDistance(int iLatitude)
        {
            return Math.Abs(iLatitude - Globals.Infos.latitude(LatitudeType.EQUATOR).miBase + Globals.Infos.latitude(LatitudeType.EQUATOR).miRange / 2);
        }

        public void generateRandomTerrain(GameServer game, MapClient map)
        {
            if (map == null)
            {
                map = game.mapServer();
            }

            Fractal fractalHeight = new Fractal();
            fractalHeight.init(map.getSeed(), map.getTerrainWidth(), map.getTerrainLength(), map.infoTerrainClass(map.getTerrainClass()).miHeightVariance, map.infos().mapSize(map.getMapSize()).miCoefficient);

            {
                TerrainType TERRAIN_CANYON = map.infos().getType<TerrainType>("TERRAIN_CANYON");

                int iPercent = map.infos().terrainClass(map.getTerrainClass()).maiTerrainPercent[(int)TERRAIN_CANYON];

                for (int iX = 0; iX < map.getTerrainWidth(); iX++)
                {
                    for (int iY = 0; iY < map.getTerrainLength(); iY++)
                    {
                        TerrainInfo pLoopTerrain = map.terrainInfoGrid(iX, iY);

                        int iValue = fractalHeight.getTileValue(iX, iY);
                        int heightOffset = map.infos().terrainClass(map.getTerrainClass()).miHeightOffset;

                        //Fix to prevent huge amounts of the map from being unusable: heights lower than the minimum are mirrored to go back upward.
                        int canyonHeight = fractalHeight.getValueByPercent(iPercent);
                        if (iValue + heightOffset < canyonHeight)
                        {
                            iValue = (2 * canyonHeight) - (iValue + 12);
                        }

                        if (iValue + heightOffset < canyonHeight)
                        {
                            pLoopTerrain.Height = (HeightType)0;
                        }
                        else
                        {
                            pLoopTerrain.Height = (HeightType)Utils.clamp(((iValue + heightOffset - (canyonHeight / 2)) * map.infos().heights().Count) / (100 - iPercent), 1, map.infos().heights().Count - 1);
                        }
                    }
                }
            }

            {
                TerrainType TERRAIN_SAND = map.infos().getType<TerrainType>("TERRAIN_SAND");

                if (!(map.infos().terrain(TERRAIN_SAND).mabLocationInvalid[(int)(map.getLocation())]))
                {
                    int iPercent = map.infos().terrainClass(map.getTerrainClass()).maiTerrainPercent[(int)TERRAIN_SAND];

                    Fractal fractal = new Fractal();
                    fractal.init(map.getSeed() + 1, map.getTerrainWidth(), map.getTerrainLength(), 10, map.infos().mapSize(map.getMapSize()).miCoefficient);

                    for (int iX = 0; iX < map.getTerrainWidth(); iX++)
                    {
                        for (int iY = 0; iY < map.getTerrainLength(); iY++)
                        {
                            TerrainInfo pLoopTerrain = map.terrainInfoGrid(iX, iY);
                            
                            int iValue = fractal.getTileValue(iX, iY);

                            if (iValue < fractal.getValueByPercent(iPercent))
                            {
                                pLoopTerrain.Terrain = TERRAIN_SAND;
                            }
                        }
                    }
                }
            }

            {
                TerrainType TERRAIN_NORMAL_LIGHT = map.infos().getType<TerrainType>("TERRAIN_NORMAL_LIGHT");

                if (!(map.infos().terrain(TERRAIN_NORMAL_LIGHT).mabLocationInvalid[(int)(map.getLocation())]))
                {
                    Fractal fractal = new Fractal();
                    fractal.init(map.getSeed() + 1, map.getTerrainWidth(), map.getTerrainLength(), 10, map.infos().mapSize(map.getMapSize()).miCoefficient);

                    for (int iX = 0; iX < map.getTerrainWidth(); iX++)
                    {
                        int iPercent = getEquatorDistance(iX);
                        for (int iY = 0; iY < map.getTerrainLength(); iY++)
                        {
                            TerrainInfo pLoopTerrain = map.terrainInfoGrid(iX, iY);
                            
                            int iValue = fractal.getTileValue(iX, iY);

                            if (iValue < fractal.getValueByPercent(iPercent))
                            {
                                pLoopTerrain.Terrain = TERRAIN_NORMAL_LIGHT;
                            }
                        }
                    }
                }
            }

            {
                TerrainType TERRAIN_VOLCANIC = map.infos().getType<TerrainType>("TERRAIN_VOLCANIC");

                if (!(map.infos().terrain(TERRAIN_VOLCANIC).mabLocationInvalid[(int)(map.getLocation())]))
                {
                    int iPercent = map.infos().terrainClass(map.getTerrainClass()).maiTerrainPercent[(int)TERRAIN_VOLCANIC];

                    Fractal fractal = new Fractal();
                    fractal.init(map.getSeed() + 2, map.getTerrainWidth(), map.getTerrainLength(), 10, map.infos().mapSize(map.getMapSize()).miCoefficient);

                    for (int iX = 0; iX < map.getTerrainWidth(); iX++)
                    {
                        for (int iY = 0; iY < map.getTerrainLength(); iY++)
                        {
                            TerrainInfo pLoopTerrain = map.terrainInfoGrid(iX, iY);

                            int iValue = fractal.getTileValue(iX, iY);

                            if (iValue > fractal.getValueByPercent(100 - iPercent))
                            {
                                pLoopTerrain.Terrain = TERRAIN_VOLCANIC;
                            }
                        }
                    }
                }
            }

            {
                TerrainType TERRAIN_ROCKY = map.infos().getType<TerrainType>("TERRAIN_ROCKY");

                if (!(map.infos().terrain(TERRAIN_ROCKY).mabLocationInvalid[(int)(map.getLocation())]))
                {
                    int iPercent = map.infos().terrainClass(map.getTerrainClass()).maiTerrainPercent[(int)TERRAIN_ROCKY];

                    Fractal fractal = new Fractal();
                    fractal.init(map.getSeed() + 3, map.getTerrainWidth(), map.getTerrainLength(), 10, map.infos().mapSize(map.getMapSize()).miCoefficient);

                    for (int iX = 0; iX < map.getTerrainWidth(); iX++)
                    {
                        for (int iY = 0; iY < map.getTerrainLength(); iY++)
                        {
                            TerrainInfo pLoopTerrain = map.terrainInfoGrid(iX, iY);

                            int iValue = fractal.getTileValue(iX, iY);

                            if ((iValue > fractal.getValueByPercent(50 - (iPercent / 2))) && (iValue < fractal.getValueByPercent(50 + (iPercent / 2))))
                            {
                                pLoopTerrain.Terrain = TERRAIN_ROCKY;
                            }
                        }
                    }
                }
            }

            {
                TerrainType TERRAIN_RIVERBED = map.infos().getType<TerrainType>("TERRAIN_RIVERBED");

                if (!(map.infos().terrain(TERRAIN_RIVERBED).mabLocationInvalid[(int)(map.getLocation())]))
                {
                    createFlow(game, map, fractalHeight, TERRAIN_RIVERBED);
                }
            }

            {
                TerrainType TERRAIN_LAVA_FLOW = map.infos().getType<TerrainType>("TERRAIN_LAVA_FLOW");

                if (!(map.infos().terrain(TERRAIN_LAVA_FLOW).mabLocationInvalid[(int)(map.getLocation())]))
                {
                    createFlow(game, map, fractalHeight, TERRAIN_LAVA_FLOW);
                }
            }

            {
                TerrainType TERRAIN_LAKEBED = map.infos().getType<TerrainType>("TERRAIN_LAKEBED");

                if (!(map.infos().terrain(TERRAIN_LAKEBED).mabLocationInvalid[(int)(map.getLocation())]))
                {
                    int iPercent = map.infos().terrainClass(map.getTerrainClass()).maiTerrainPercent[(int)TERRAIN_LAKEBED];

                    Fractal fractal = new Fractal();
                    fractal.init(map.getSeed() + 4, map.getTerrainWidth(), map.getTerrainLength(), 10, map.infos().mapSize(map.getMapSize()).miCoefficient);

                    for (int iX = 0; iX < map.getTerrainWidth(); iX++)
                    {
                        for (int iY = 0; iY < map.getTerrainLength(); iY++)
                        {
                            TerrainInfo pLoopTerrain = map.terrainInfoGrid(iX, iY);

                            int iValue = fractal.getTileValue(iX, iY);

                            if ((iValue > fractal.getValueByPercent((int)(pLoopTerrain.Height) * 20)) && (iValue < fractal.getValueByPercent(((int)(pLoopTerrain.Height) * 20) + iPercent)))
                            {
                                pLoopTerrain.Terrain = TERRAIN_LAKEBED;
                            }
                        }
                    }
                }
            }

            {
                TerrainType TERRAIN_CLAY = map.infos().getType<TerrainType>("TERRAIN_CLAY", false);

                if (!(map.infos().terrain(TERRAIN_CLAY).mabLocationInvalid[(int)(map.getLocation())]))
                {
                    int iPercent = map.infos().terrainClass(map.getTerrainClass()).maiTerrainPercent[(int)TERRAIN_CLAY];

                    Fractal fractal = new Fractal();
                    fractal.init(map.getSeed() + 12, map.getTerrainWidth(), map.getTerrainLength(), 10, map.infos().mapSize(map.getMapSize()).miCoefficient);

                    for (int iX = 0; iX < map.getTerrainWidth(); iX++)
                    {
                        for (int iY = 0; iY < map.getTerrainLength(); iY++)
                        {
                            TerrainInfo pLoopTerrain = map.terrainInfoGrid(iX, iY);

                            int iValue = fractal.getTileValue(iX, iY);

                            if ((iValue > fractal.getValueByPercent((int)(pLoopTerrain.Height) * 20)) && (iValue < fractal.getValueByPercent(((int)(pLoopTerrain.Height) * 20) + iPercent)))
                            {
                                pLoopTerrain.Terrain = TERRAIN_CLAY;
                            }
                        }
                    }
                }
            }

            {
                TerrainType TERRAIN_SALTS = map.infos().getType<TerrainType>("TERRAIN_SALTS", false);

                if (!(map.infos().terrain(TERRAIN_SALTS).mabLocationInvalid[(int)(map.getLocation())]))
                {
                    int iPercent = map.infos().terrainClass(map.getTerrainClass()).maiTerrainPercent[(int)TERRAIN_SALTS];

                    Fractal fractal = new Fractal();
                    fractal.init(map.getSeed() + 11, map.getTerrainWidth(), map.getTerrainLength(), 5, map.infos().mapSize(map.getMapSize()).miCoefficient);

                    for (int iX = 0; iX < map.getTerrainWidth(); iX++)
                    {
                        for (int iY = 0; iY < map.getTerrainLength(); iY++)
                        {
                            TerrainInfo pLoopTerrain = map.terrainInfoGrid(iX, iY);

                            int iValue = fractal.getTileValue(iX, iY);

                            if (iValue < fractal.getValueByPercent(iPercent))
                            {
                                pLoopTerrain.Terrain = TERRAIN_SALTS;
                            }
                        }
                    }
                }
            }

            {
                TerrainType TERRAIN_CRATER = map.infos().getType<TerrainType>("TERRAIN_CRATER");

                if (!(map.infos().terrain(TERRAIN_CRATER).mabLocationInvalid[(int)(map.getLocation())]))
                {
                    int iTilesPer = map.infos().terrainClass(map.getTerrainClass()).maiTilesPerTerrain[(int)TERRAIN_CRATER];

                    int iNumCraters = (map.numTiles() / Math.Max(10, iTilesPer));

                    for (int i = 0; i < iNumCraters; i++)
                    {
                        int iCenterID;
                        if (game != null)
                        {
                            iCenterID = game.random().Next(map.numTerrains());
                        }
                        else
                        {
                            iCenterID = Mathf.FloorToInt((UnityEngine.Random.value * (float)map.numTerrains()) - 0.001f);
                        }

                        TerrainInfo pCenterTerrain = map.terrainInfo(iCenterID);

                        if (pCenterTerrain.Height > (HeightType)1)
                        {
                            HeightType eNewHeight = (HeightType)(Math.Max(1, ((int)(pCenterTerrain.Height) - 1)));

                            int iRange;
                            if (game != null)
                            {
                                iRange = game.random().Next(map.infos().terrainClass(map.getTerrainClass()).miCraterWidthRoll);
                            }
                            else
                            {
                                iRange = Mathf.FloorToInt((UnityEngine.Random.value * (float)map.infos().terrainClass(map.getTerrainClass()).miCraterWidthRoll) - 0.0001f);
                            }
                            iRange += map.infos().terrainClass(map.getTerrainClass()).miCraterWidthBase;

                            for (int iDX = -(iRange); iDX <= iRange; iDX++)
                            {
                                for (int iDY = -(iRange); iDY <= iRange; iDY++)
                                {
                                    TerrainInfo pLoopTerrain = map.terrainInfo(map.terrainInfoIDRange(iCenterID, iDX, iDY, iRange));

                                    if (pLoopTerrain != null)
                                    {
                                        pLoopTerrain.Terrain = TERRAIN_CRATER;
                                        pLoopTerrain.Height = eNewHeight;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            {
                TerrainType TERRAIN_CRACK = map.infos().getType<TerrainType>("TERRAIN_CRACK");

                if (!(map.infos().terrain(TERRAIN_CRACK).mabLocationInvalid[(int)(map.getLocation())]))
                {
                    int iRoll = map.infos().terrainClass(map.getTerrainClass()).maiTerrainRoll[(int)TERRAIN_CRACK];
                    if (iRoll > 0)
                    {
                        Fractal fractal = new Fractal();
                        fractal.init(map.getSeed() + 5, map.getTerrainWidth(), map.getTerrainLength(), 10, map.infos().mapSize(map.getMapSize()).miCoefficient);

                        for (int i = 0; i < map.numTerrains(); i++)
                        {
                            TerrainInfo pLoopTerrain = map.terrainInfo(i);

                            int iTileRoll = ((game != null) ? game.random().Next(iRoll) : Mathf.FloorToInt(UnityEngine.Random.value * iRoll - 0.0001f));

                            if (iTileRoll == 0)
                            {
                                pLoopTerrain.Terrain = TERRAIN_CRACK;

                                for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
                                {
                                    bool bFlipCoin = ((game != null) ? game.random().Next(2) == 0 : UnityEngine.Random.value < 0.5f);

                                    if (bFlipCoin)
                                    {
                                        TerrainInfo pAdjacentTerrain = map.terrainInfo(map.terrainInfoIDAdjacent(i, eDirection));

                                        if (pAdjacentTerrain != null)
                                        {
                                            pAdjacentTerrain.Terrain = TERRAIN_CRACK;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            {
                TerrainType TERRAIN_CAVE = map.infos().getType<TerrainType>("TERRAIN_CAVE", false);

                if (!(map.infos().terrain(TERRAIN_CAVE).mabLocationInvalid[(int)(map.getLocation())]))
                {
                    int iRoll = map.infos().terrainClass(map.getTerrainClass()).maiTerrainRoll[(int)TERRAIN_CAVE];
                    if (iRoll > 0)
                    {
                        Fractal fractal = new Fractal();
                        fractal.init(map.getSeed() + 13, map.getTerrainWidth(), map.getTerrainLength(), 10, map.infos().mapSize(map.getMapSize()).miCoefficient);

                        for (int i = 0; i < map.numTerrains(); i++)
                        {
                            TerrainInfo pLoopTerrain = map.terrainInfo(i);

                            int iTileRoll = ((game != null) ? game.random().Next(iRoll) : Mathf.FloorToInt(UnityEngine.Random.value * iRoll - 0.0001f));

                            if (iTileRoll == 0)
                            {
                                pLoopTerrain.Terrain = TERRAIN_CAVE;
                                if (game != null)
                                    game.addCaveTile(map.tileIDfromTerrainInfoID(i));

                                for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
                                {
                                    bool bFlipCoin = ((game != null) ? game.random().Next(3) == 0 : UnityEngine.Random.value < 0.33f);

                                    if (bFlipCoin)
                                    {
                                        int adjacentID = map.terrainInfoIDAdjacent(i, eDirection);
                                        TerrainInfo pAdjacentTerrain = map.terrainInfo(adjacentID);

                                        if (pAdjacentTerrain != null)
                                        {
                                            pAdjacentTerrain.Terrain = TERRAIN_CAVE;
                                            if (game != null)
                                                game.addCaveTile(map.tileIDfromTerrainInfoID(adjacentID));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            {
                TerrainType TERRAIN_BASALT = map.infos().getType<TerrainType>("TERRAIN_BASALT", false);

                if (!(map.infos().terrain(TERRAIN_BASALT).mabLocationInvalid[(int)(map.getLocation())]))
                {
                    int iRoll = map.infos().terrainClass(map.getTerrainClass()).maiTerrainRoll[(int)TERRAIN_BASALT];
                    if (iRoll > 0)
                    {
                        Fractal fractal = new Fractal();
                        fractal.init(map.getSeed() + 14, map.getTerrainWidth(), map.getTerrainLength(), 10, map.infos().mapSize(map.getMapSize()).miCoefficient);

                        List<ResourceType> terrainResources = new List<ResourceType>();
                        for (ResourceType eLoopResource = 0; eLoopResource < map.infos().resourcesNum(); eLoopResource++)
                        {
                            if (map.infos().terrain(TERRAIN_BASALT).maiResourceRate[(int)eLoopResource] > 0)
                                terrainResources.Add(eLoopResource);
                        }

                        for (int i = 0; i < map.numTerrains(); i++)
                        {
                            TerrainInfo pLoopTerrain = map.terrainInfo(i);

                            int iTileRoll = ((game != null) ? game.random().Next(iRoll) : Mathf.FloorToInt(UnityEngine.Random.value * iRoll - 0.0001f));

                            if (iTileRoll == 0)
                            {
                                pLoopTerrain.Terrain = TERRAIN_BASALT;
                                if (game != null)
                                {
                                    foreach (ResourceType eLoopResource in terrainResources)
                                        game.addResourceTileToList(map.tileIDfromTerrainInfoID(i), eLoopResource);
                                }

                                for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
                                {
                                    bool bFlipCoin = ((game != null) ? game.random().Next(3) != 0 : UnityEngine.Random.value < 0.66f);

                                    if (bFlipCoin)
                                    {
                                        int adjacentID = map.terrainInfoIDAdjacent(i, eDirection);
                                        TerrainInfo pAdjacentTerrain = map.terrainInfo(adjacentID);

                                        if (pAdjacentTerrain != null)
                                        {
                                            pAdjacentTerrain.Terrain = TERRAIN_BASALT;
                                            if (game != null)
                                            {
                                                foreach (ResourceType eLoopResource in terrainResources)
                                                    game.addResourceTileToList(map.tileIDfromTerrainInfoID(i), eLoopResource);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            {
                TerrainType TERRAIN_METHANE = map.infos().getType<TerrainType>("TERRAIN_METHANE", false);
                int iRoll = map.infos().terrainClass(map.getTerrainClass()).maiTerrainRoll[(int)TERRAIN_METHANE];
                if (iRoll > 0)
                {
                    new Fractal().init(map.getSeed() + 14, map.getTerrainWidth(), map.getTerrainLength(), 10, map.infos().mapSize(map.getMapSize()).miCoefficient);
                    List<ResourceType> terrainResources = new List<ResourceType>();
                    for (ResourceType eLoopResource = 0; eLoopResource < map.infos().resourcesNum(); eLoopResource++)
                    {
                        if (map.infos().terrain(TERRAIN_METHANE).maiResourceRate[(int)eLoopResource] > 0)
                            terrainResources.Add(eLoopResource);
                    }

                    for (int index = 0; index < map.numTerrains(); ++index)
                    {
                        TerrainInfo terrainInfo1 = map.terrainInfo(index);

                        int iTileRoll = ((game != null) ? game.random().Next(iRoll) : Mathf.FloorToInt(UnityEngine.Random.value * iRoll - 0.0001f));
                        if (iTileRoll == 0)
                        {
                            terrainInfo1.Terrain = TERRAIN_METHANE;
                            if (game != null)
                            {
                                foreach (ResourceType eLoopResource in terrainResources)
                                    game.addResourceTileToList(map.tileIDfromTerrainInfoID(index), eLoopResource);
                            }

                            for (DirectionType eDirection = DirectionType.NW; eDirection < DirectionType.NUM_TYPES; ++eDirection)
                            {
                                if (game == null ? (double)UnityEngine.Random.value < 0.66f : game.random().Next(3) != 0)
                                {
                                    int adjacentID = map.terrainInfoIDAdjacent(index, eDirection);
                                    TerrainInfo pAdjacentTerrain = map.terrainInfo(adjacentID);
                                    if (pAdjacentTerrain != null)
                                    {
                                        pAdjacentTerrain.Terrain = TERRAIN_METHANE;
                                        if (game != null)
                                        {
                                            foreach (ResourceType eLoopResource in terrainResources)
                                                game.addResourceTileToList(map.tileIDfromTerrainInfoID(index), eLoopResource);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            {
                TerrainType TERRAIN_HILLS = map.infos().getType<TerrainType>("TERRAIN_HILLS");

                if (!(map.infos().terrain(TERRAIN_HILLS).mabLocationInvalid[(int)(map.getLocation())]))
                {
                    int iPercent = map.infos().terrainClass(map.getTerrainClass()).maiTerrainPercent[(int)TERRAIN_HILLS];

                    Fractal fractal = new Fractal();
                    fractal.init(map.getSeed() + 6, map.getTerrainWidth(), map.getTerrainLength(), 10, map.infos().mapSize(map.getMapSize()).miCoefficient);

                    for (int iX = 0; iX < map.getTerrainWidth(); iX++)
                    {
                        for (int iY = 0; iY < map.getTerrainLength(); iY++)
                        {
                            TerrainInfo pLoopTerrain = map.terrainInfoGrid(iX, iY);

                            int iValue = fractal.getTileValue(iX, iY);

                            if (((iValue > fractal.getValueByPercent(20)) && (iValue < fractal.getValueByPercent(20 + (iPercent / 2)))) ||
                                ((iValue > fractal.getValueByPercent(70)) && (iValue < fractal.getValueByPercent(70 + (iPercent / 2)))))
                            {
                                pLoopTerrain.Terrain = TERRAIN_HILLS;
                            }
                        }
                    }

                    // Each hills tile has a 50% chance to expand into two of the tiles next to it, once.
                    // This effect does not chain, and is intended to make the hills clumps generally more substantial
                    bool[] addHills = new bool[map.numTerrains()];
                    int hillRoll = 0;
                    int loopID = -1;
                    for (int iX = 0; iX < map.numTerrains(); iX++)
                    {
                        if (map.terrainInfo(iX).Terrain == TERRAIN_HILLS)
                        {
                            if (game != null)
                            {
                                hillRoll = game.random().Next(4);
                            }
                            else
                            {
                                hillRoll = Mathf.FloorToInt(UnityEngine.Random.value * 3.999f);
                            }

                            if (hillRoll == 2)
                            {
                                loopID = map.terrainInfoIDAdjacent(iX, (DirectionType)0);
                                if (loopID >= 0)
                                {
                                    addHills[loopID] = true;
                                }
                                loopID = map.terrainInfoIDAdjacent(iX, (DirectionType)1);
                                if (loopID >= 0)
                                {
                                    addHills[loopID] = true;
                                }
                            }
                            else if (hillRoll == 3)
                            {
                                loopID = map.terrainInfoIDAdjacent(iX, (DirectionType)1);
                                if (loopID >= 0)
                                {
                                    addHills[loopID] = true;
                                }
                                loopID = map.terrainInfoIDAdjacent(iX, (DirectionType)2);
                                if (loopID >= 0)
                                {
                                    addHills[loopID] = true;
                                }
                            }
                        }
                    }
                    for (int iX = 0; iX < map.numTerrains(); iX++)
                    {
                        if (addHills[iX])
                        {
                            map.terrainInfo(iX).Terrain = TERRAIN_HILLS;
                        }
                    }
                }
            }

            flattenTerrain(game);

            ScrambleTileHeights(map, game);

            if (map.getLocation() == LocationType.EUROPA) {
                int striationRoll = 0;
                if (game != null)
                {
                    striationRoll = game.random().Next(map.infoTerrainClass(map.getTerrainClass()).miStriationRolls);
                }
                else
                {
                    striationRoll = Mathf.FloorToInt(UnityEngine.Random.value * map.infoTerrainClass(map.getTerrainClass()).miStriationRolls * 0.999f);
                }
                if (striationRoll == 0)
                    StriationGenerate(game, map);
            }
        }

        void createFlow(GameServer pGame, MapClient pMap, Fractal pFractalHeight, TerrainType eTerrain)
        {
            int iTilesPer = pMap.infos().terrainClass(pMap.getTerrainClass()).maiTilesPerTerrain[(int)eTerrain];

            int iNumRivers = (pMap.numTiles() / Math.Max(10, iTilesPer));

            for (int i = 0; i < iNumRivers; i++)
            {

                int iRiverID;
                if (pGame != null)
                {
                    iRiverID = pGame.random().Next(pMap.numTerrains());
                }
                else
                {
                    iRiverID = Mathf.FloorToInt((UnityEngine.Random.value * (float)pMap.numTerrains()) - 0.001f);
                }

                if (pMap.terrainInfo(iRiverID).Height > (HeightType)0)
                {
                    DirectionType eFirstDirection = DirectionType.NONE;

                    {
                        int iBestValue = int.MaxValue;

                        for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
                        {
                            TerrainInfo pAdjacentTerrain = pMap.terrainInfo(pMap.terrainInfoIDAdjacent(iRiverID, eDirection));

                            if (pAdjacentTerrain != null)
                            {
                                int iValue = pFractalHeight.getTileValue(pMap.getTerrainInfoX(iRiverID), pMap.getTerrainInfoY(iRiverID));
                                if (iValue < iBestValue)
                                {
                                    eFirstDirection = eDirection;
                                    iBestValue = iValue;
                                }
                            }
                        }
                    }


                    while (true)
                    {
                        pMap.terrainInfo(iRiverID).Terrain = eTerrain;

                        //int iBestTerrainID = -1;
                        DirectionType eBestDirection = DirectionType.NONE;
                        int iBestValue = int.MaxValue;

                        for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
                        {
                            if (eDirection != Utils.directionOpposite(eFirstDirection))
                            {
                                int iAdjacentTerrainID = pMap.terrainInfoIDAdjacent(iRiverID, eDirection);

                                if (iAdjacentTerrainID == -1)
                                {
                                    //iBestTerrainID = iAdjacentTerrainID;
                                    eBestDirection = DirectionType.NONE;
                                    iBestValue = 0;
                                }
                                else if (pMap.terrainInfo(iAdjacentTerrainID).Terrain != eTerrain)
                                {
                                    int iValue = pFractalHeight.getTileValue(pMap.getTerrainInfoX(iAdjacentTerrainID), pMap.getTerrainInfoY(iAdjacentTerrainID));
                                    if (iValue < iBestValue)
                                    {
                                        //iBestTerrainID = iAdjacentTerrainID;
                                        eBestDirection = eDirection;
                                        iBestValue = iValue;
                                    }
                                }
                            }
                        }

                        //if (iBestTerrainID == -1)
                        if (eBestDirection == DirectionType.NONE)
                        {
                            break;
                        }
                        else
                        {
                            int iDirectionOffset;
                            if (pGame != null)
                            {
                                iDirectionOffset = ((pGame.random().Next(7) + 4) / 5) - 1; //this results in going forward 5 out of 7 times, and 1 out of 7 each for left or right
                            }
                            else
                            {
                                iDirectionOffset = ((Mathf.FloorToInt(UnityEngine.Random.value * 6.999f) + 4) / 5) - 1;
                            }
                            eBestDirection = Utils.wrapDirection(eBestDirection, iDirectionOffset);
                        }

                        //iRiverID = iBestTerrainID;
                        iRiverID = pMap.terrainInfoIDAdjacent(iRiverID, eBestDirection);
                    }
                }
            }
        }

        public void flattenTerrain(GameServer pGame)
        {
            if (pGame == null)
            {
                return;
            }

            TerrainType TERRAIN_NORMAL = pGame.infos().getType<TerrainType>("TERRAIN_NORMAL");

            ModuleType START_MODULE = pGame.infos().location(pGame.getLocation()).meStartModule;
            int COLONY_FLAT_RANGE = (pGame.infos().Globals.COLONY_FLAT_RANGE + 2);

            TileServer pBestTile = null;
            int iBestValue = 0;

            if (pBestTile == null)
            {
                foreach (TileServer pLoopTile in pGame.tileServerAll())
                {
                    if (pLoopTile.usable() && (pLoopTile.getHeight() > (HeightType)0))
                    {
                        if (pLoopTile.canHaveModule(START_MODULE))
                        {
                            int iValue = pGame.random().Next(1000);

                            iValue += (Math.Abs(Math.Min(pLoopTile.getX(), pGame.getMapWidth() - pLoopTile.getX()) + Math.Min(pLoopTile.getY(), pGame.getMapHeight() - pLoopTile.getY())) * 100);

                            if (iValue > iBestValue)
                            {
                                pBestTile = pLoopTile;
                                iBestValue = iValue;
                            }
                        }
                    }
                }
            }

            if (pBestTile == null)
            {
                foreach (TileServer pLoopTile in pGame.tileServerAll())
                {
                    if (pLoopTile.usable() && (pLoopTile.getHeight() > (HeightType)0))
                    {
                        int iValue = pGame.random().Next(1000);

                        iValue += (Math.Abs(Math.Min(pLoopTile.getX(), pGame.getMapWidth() - pLoopTile.getX()) + Math.Min(pLoopTile.getY(), pGame.getMapHeight() - pLoopTile.getY())) * 100);

                        if (iValue > iBestValue)
                        {
                            pBestTile = pLoopTile;
                            iBestValue = iValue;
                        }
                    }
                }
            }

            if (pBestTile != null)
            {
                foreach (TileServer pRangeTile in pGame.tileServerRangeIterator(pBestTile, COLONY_FLAT_RANGE))
                {
                    if (!(pRangeTile.usable()))
                    {
                        pRangeTile.getTerrainInfo().Terrain = TERRAIN_NORMAL;
                    }

                    pRangeTile.getTerrainInfo().Height = pBestTile.getHeight();
                }
            }
        }

        public void generateIce(GameServer pGame)
        {
            generateIce(pGame.mapClient(), pGame);
        }

        bool validIceTile(MapClient pMap, int iID, IceType eIce)
        {
            if (pMap.infos().terrain(pMap.terrainInfo(iID).Terrain).maabIceInvalid[(int)pMap.getLocation()][(int)eIce])
            {
                return false;
            }

            if (pMap.getLocation() != LocationType.EUROPA)
            {
                for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
                {
                    TerrainInfo pAdjacentTerrain = pMap.terrainInfo(pMap.terrainInfoIDAdjacent(iID, eDirection));

                    if (pAdjacentTerrain != null)
                    {
                        if (pMap.infos().terrain(pAdjacentTerrain.Terrain).mbNoAdjacentIce)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public void generateIce(MapClient pMap, GameServer pGame)
        {
            // pGame can be a null value due to the Map Editor, which does not create or maintain a GameServer object; it cannot be assumed to exist
            // pGame is only passed for possible usage of its procedural number generation in such cases where it does exist e.g. actual game matches

            for (IceType eLoopIce = 0; eLoopIce < pMap.infos().icesNum(); eLoopIce++)
            {
                Fractal fractalLatitude = new Fractal();
                fractalLatitude.init(pMap.getSeed() + 7, pMap.getTerrainWidth(), pMap.getTerrainLength(), 20, pMap.infos().mapSize(pMap.getMapSize()).miCoefficient);

                Fractal fractalAppearance = new Fractal();
                fractalAppearance.init(pMap.getSeed() + 9, pMap.getTerrainWidth(), pMap.getTerrainLength(), 40, pMap.infos().mapSize(pMap.getMapSize()).miCoefficient);

                int iCount = 0;

                for (int iX = 0; iX < pMap.getTerrainWidth(); iX++)
                {
                    for (int iY = 0; iY < pMap.getTerrainLength(); iY++)
                    {
                        // cannot assume pGame exists -- check before using it!
                        int randomNum = 0;
                        if (pGame != null)
                        {
                            randomNum = pGame.random().Next(100);
                        }
                        else
                        {
                            randomNum = Mathf.FloorToInt(UnityEngine.Random.value * 99.99f);
                        }

                        if (randomNum < pMap.infos().ice(eLoopIce).maiLocationAppearanceProb[(int)pMap.getLocation()])
                        {
                            int iPercent = pMap.infos().ice(eLoopIce).maiLocationAppearancePercent[(int)pMap.getLocation()];

                            iPercent *= Math.Max(0, (pMap.infos().latitude(pMap.settings().meLatitude).miIceModifier + 100));
                            iPercent /= 100;

                            int iAppearanceValue = fractalAppearance.getTileValue(iX, iY);
                            if (iAppearanceValue <= fractalAppearance.getValueByPercent(iPercent))
                            {
                                int iTileID = pMap.terrainInfoIDGrid(iX, iY);
                                TerrainInfo pLoopTerrain = pMap.terrainInfo(iTileID);

                                if (validIceTile(pMap, iTileID, eLoopIce))
                                {
                                    if (pLoopTerrain.Ice == IceType.NONE)
                                    {
                                        if (pMap.infos().ice(eLoopIce).maiLocationLatitudeMin[(int)pMap.getLocation()] != -1)
                                        {
                                            int iMapLatitude = pMap.getLatitude(iY);

                                            iMapLatitude += (int)(pLoopTerrain.Height) * 5;

                                            int iLatitudeValue = fractalLatitude.getTileValue(iX, iY);

                                            if (iLatitudeValue > fractalLatitude.getValueByPercent(75))
                                            {
                                                iMapLatitude += 12;
                                            }
                                            else if (iLatitudeValue > fractalLatitude.getValueByPercent(50))
                                            {
                                                iMapLatitude += 8;
                                            }
                                            else if (iLatitudeValue > fractalLatitude.getValueByPercent(25))
                                            {
                                                iMapLatitude += 4;
                                            }

                                            if (iMapLatitude > pMap.infos().ice(eLoopIce).maiLocationLatitudeMin[(int)pMap.getLocation()])
                                            {
                                                pLoopTerrain.Ice = eLoopIce;
                                                iCount++;
                                            }
                                        }
                                    }

                                    if (pLoopTerrain.Ice == IceType.NONE)
                                    {
                                        if (pMap.infos().ice(eLoopIce).maiLocationLatitudeMax[(int)pMap.getLocation()] != -1)
                                        {
                                            int iMapLatitude = pMap.getLatitude(iY);

                                            iMapLatitude -= (int)(pLoopTerrain.Height) * 5;

                                            int iLatitudeValue = fractalLatitude.getTileValue(iX, iY);

                                            if (iLatitudeValue > fractalLatitude.getValueByPercent(75))
                                            {
                                                iMapLatitude -= 12;
                                            }
                                            else if (iLatitudeValue > fractalLatitude.getValueByPercent(50))
                                            {
                                                iMapLatitude -= 8;
                                            }
                                            else if (iLatitudeValue > fractalLatitude.getValueByPercent(25))
                                            {
                                                iMapLatitude -= 4;
                                            }

                                            if (iMapLatitude < pMap.infos().ice(eLoopIce).maiLocationLatitudeMax[(int)pMap.getLocation()])
                                            {
                                                pLoopTerrain.Ice = eLoopIce;
                                                iCount++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < pMap.numTerrains(); i++)
                {
                    TerrainInfo pLoopTerrain = pMap.terrainInfo(i);

                    if (pLoopTerrain.Ice == eLoopIce)
                    {
                        bool bIceFound = false;

                        for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
                        {
                            TerrainInfo pAdjacentTerrain = pMap.terrainInfo(pMap.terrainInfoIDAdjacent(i, eDirection));

                            if ((pAdjacentTerrain == null) || (pAdjacentTerrain.Ice == eLoopIce))
                            {
                                if ((pAdjacentTerrain == null) || (pMap.infos().terrain(pAdjacentTerrain.Terrain).mbUsable))
                                {
                                    bIceFound = true;
                                    break;
                                }
                            }
                        }

                        if (!bIceFound)
                        {
                            if (!(pMap.infos().terrain(pLoopTerrain.Terrain).mbUsable) || (iCount > ((pMap.numTerrains() * 5) / 100)))
                            {
                                pLoopTerrain.Ice = IceType.NONE;
                                iCount--;
                            }
                            else
                            {
                                TerrainInfo pBestTerrain = null;
                                int iBestValue = 0;

                                for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
                                {
                                    int iAdjacentID = pMap.terrainInfoIDAdjacent(i, eDirection);
                                    TerrainInfo pAdjacentTerrain = pMap.terrainInfo(iAdjacentID);

                                    if (pAdjacentTerrain != null)
                                    {
                                        if (pAdjacentTerrain.Ice == IceType.NONE)
                                        {
                                            if (pMap.infos().terrain(pAdjacentTerrain.Terrain).mbUsable)
                                            {
                                                if (validIceTile(pMap, iAdjacentID, eLoopIce))
                                                {
                                                    int iValue = ((pGame != null) ? pGame.random().Next(100) : Mathf.FloorToInt(UnityEngine.Random.value * 100f)) + 1;
                                                    if (iValue > iBestValue)
                                                    {
                                                        pBestTerrain = pAdjacentTerrain;
                                                        iBestValue = iValue;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                if (pBestTerrain != null)
                                {
                                    pBestTerrain.Ice = eLoopIce;
                                    iCount++;
                                }
                                else
                                {
                                    pLoopTerrain.Ice = IceType.NONE;
                                    iCount--;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void generateWind(GameServer pGame)
        {
            MapServer pMap = pGame.mapServer();

            for (int i = 0; i < pMap.numTerrains(); i++)
            {
                TileClient pLoopTile = pMap.tileClient(pMap.tileIDfromTerrainInfoID(i));

                if (pLoopTile != null)
                {
                    if (pMap.infos().location(pGame.getLocation()).mbNoWind)
                    {
                        pLoopTile.getTerrainInfo().Wind = WindType.NONE;
                        continue;
                    }
                    int iCount = 0;

                    if (pLoopTile.usable())
                    {
                        for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
                        {
                            TerrainInfo pAdjacentTerrain = pMap.terrainInfo(pMap.terrainInfoIDAdjacent(i, eDirection));

                            if (pAdjacentTerrain != null)
                            {
                                iCount += Math.Max(0, (pLoopTile.getHeight() - pAdjacentTerrain.Height));
                            }
                        }
                    }

                    int iMaxWind = pMap.infos().winds().Count - 1;
                    pLoopTile.getTerrainInfo().Wind = ((WindType)Utils.clamp(iCount, 0, iMaxWind));
                    if(iCount + 3 >= iMaxWind)
                    {
                        pGame.addWindTileToList(pLoopTile.getID());
                    }
                }
            }
        }

        public static int getResourceProb(ResourceType eResource, GameClient pGame, TileClient pTile, bool bSabotage)
        {
            if (pTile.noResources())
            {
                return 0;
            }

            if (!(pGame.isResourceValid(eResource)))
            {
                return 0;
            }

            if (bSabotage)
            {
                if (pGame.getResourceRateCount(eResource) == 0)
                {
                    return 0;
                }
            }

            int iProb = 0;

            iProb += pGame.infos().resource(eResource).maiLocationAppearanceProb[(int)(pGame.getLocation())];
            iProb += (pGame.infos().terrain(pTile.getTerrain()).maaiResourceProb[(int)(pGame.getLocation())][(int)eResource] / ((pTile.getIce() != IceType.NONE && pGame.getLocation() != LocationType.EUROPA) ? 3 : 1));

            if (bSabotage)
            {
                foreach (TileClient pAdjacentTile in pGame.tileClientAdjacentAll(pTile))
                {
                    iProb += pGame.infos().resourceLevel(pAdjacentTile.getResourceLevel(eResource, false)).miAdjacentProb;
                }
            }

            if (pTile.getIce() != IceType.NONE)
            {
                iProb *= Math.Max(0, (pGame.infos().ice(pTile.getIce()).maaiResourceProbModifier[(int)eResource][(int)pGame.getLocation()] + 100));
                iProb /= 100;
            }

            return iProb;
        }

        public static List<ResourceType> addResource(GameServer pGame, TileServer pTile, bool bSabotage, bool bAnnounce = false)
        {
            int iCount = 1;
            List<ResourceType> aeResourcesAdded = new List<ResourceType>();

            while (true)
            {
                bool bAdded = false;

                for (ResourceType eLoopResource = 0; eLoopResource < pGame.infos().resourcesNum(); eLoopResource++)
                {
                    int iProb = getResourceProb(eLoopResource, pGame, pTile, bSabotage);

                    if (iProb > 0)
                    {
                        iProb *= iCount;
                        iProb += (iCount / 5);

                        if (!bSabotage)
                        {
                            iProb *= Math.Max(0, (pGame.infos().resourcePresence(pGame.getResourcePresence()).miModifier + 100));
                            iProb /= 100;
                        }

                        if (pGame.random().Next(RESOURCE_DIE_SIZE) < iProb)
                        {
                            bool bValid = true;

                            if (!bSabotage)
                            {
                                foreach (InfoResource pAvoidResource in pGame.infos().resources())
                                {
                                    int iRange = pGame.infos().resource(eLoopResource).maiAvoidRange[pAvoidResource.miType];
                                    if (iRange > 0)
                                    {
                                        foreach (TileServer pRangeTile in pGame.tileServerRangeIterator(pTile, iRange))
                                        {
                                            if (pRangeTile.getResourceLevel(pAvoidResource.meType, false) > ResourceLevelType.NONE)
                                            {
                                                bValid = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            if (bValid)
                            {
                                pTile.createResource(eLoopResource);
                                pGame.addResourceTileToList(pTile, eLoopResource);

                                bAdded = true;
                                aeResourcesAdded.Add(eLoopResource);
                            }
                        }
                    }
                }

                if (!bSabotage || bAdded)
                {
                    if (bAnnounce && bAdded)
                        return aeResourcesAdded;
                    return null;
                }
                else
                {
                    iCount++;
                }
            }            
        }

        public void loadResources(GameServer game)
        {
            using (new UnityProfileScope("TerrainGenerator.loadResources"))
            {
                foreach (TileServer pLoopTile in game.tileServerAll())
                {
                    if (game.mapClient().isGeothermal(pLoopTile.getTerrainID()))
                    {
                        pLoopTile.setGeothermal(true);
                        game.addGeothermalTileToList(pLoopTile.getID());
                    }
                    else
                    {
                        for (ResourceType eLoopResource = 0; eLoopResource < game.infos().resourcesNum(); eLoopResource++)
                        {
                            ResourceLevelType eResourceLevel = (ResourceLevelType)game.mapClient().getStartingResourceLevel(pLoopTile.getTerrainID(), (int)eLoopResource);
                            if (eResourceLevel > ResourceLevelType.NONE)
                            {
                                pLoopTile.setResourceLevel(eLoopResource, eResourceLevel);
                                game.addResourceTileToList(pLoopTile, eLoopResource);
                            }
                        }
                    }
                }
            }
        }

        public void generateResources(GameServer game)
        {
            using (new UnityProfileScope("TerrainGenerator.generateResources"))
            {
                TileClient pColonyTile = game.startModuleTileClient();
                int iMaxDist = Utils.maxStepDistance(game);

                foreach (TileServer pLoopTile in game.tileServerAll())
                {
                    if (!(pLoopTile.noResources()) && pLoopTile.isEmpty() && !(pLoopTile.hasResources()))
                    {
                        if ((pColonyTile == null) || (Utils.stepDistanceTile(pLoopTile, pColonyTile) >= (iMaxDist / 10)))
                        {
                            int iGeothermalProb = ((game.infos().location(game.getLocation()).mbNoGeothermal) ? 0 : game.infos().terrain(pLoopTile.getTerrain()).maiGeothermalProb[(int)game.getLocation()]);

                            iGeothermalProb *= Math.Max(0, (game.infos().resourcePresence(game.getResourcePresence()).miModifier + 100));
                            iGeothermalProb /= 100;

                            if (game.random().Next(RESOURCE_DIE_SIZE) < iGeothermalProb)
                            {
                                pLoopTile.setGeothermal(true);
                                game.addGeothermalTileToList(pLoopTile.getID());
                            }
                            else
                            {
                                addResource(game, pLoopTile, false);
                            }
                        }
                    }
                }

                for (ResourceLevelType eLoopResourceLevel = (game.infos().resourceLevelsNum() - 1); eLoopResourceLevel > ResourceLevelType.NONE; eLoopResourceLevel--)
                {
                    foreach (TileServer pLoopTile in game.tileServerAll())
                    {
                        if (!(pLoopTile.noResources()) && pLoopTile.isEmpty() && !(pLoopTile.isGeothermal()))
                        {
                            for (ResourceType eLoopResource = 0; eLoopResource < game.infos().resourcesNum(); eLoopResource++)
                            {
                                if (game.infos().resource(eLoopResource).mabNoSpread[(int)game.getLocation()])
                                    continue;
                                ResourceLevelType eBestValue = ResourceLevelType.NONE;

                                for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
                                {
                                    TileClient pAdjacentTile = game.tileServerAdjacent(pLoopTile, eDirection);
                                    if (pAdjacentTile != null)
                                    {
                                        ResourceLevelType eValue = pAdjacentTile.getResourceLevel(eLoopResource, false);

                                        if (eValue > eBestValue)
                                        {
                                            eBestValue = eValue;
                                        }
                                    }
                                }

                                if (eBestValue == eLoopResourceLevel)
                                {
                                    int iRoll = game.random().Next(100);

                                    for (ResourceLevelType eSpreadResourceLevel = 0; eSpreadResourceLevel < game.infos().resourceLevelsNum(); eSpreadResourceLevel++)
                                    {
                                        if (!(game.infos().resourceLevel(eSpreadResourceLevel).mabLocationInvalid[(int)(game.getLocation())]))
                                        {
                                            if (iRoll < game.infos().resourceLevel(eBestValue).maiSpreadRoll[(int)eSpreadResourceLevel])
                                            {
                                                if (eSpreadResourceLevel > pLoopTile.getResourceLevel(eLoopResource, false))
                                                {
                                                    pLoopTile.setResourceLevel(eLoopResource, eSpreadResourceLevel);
                                                    game.addResourceTileToList(pLoopTile, eLoopResource);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                int iLocationRateModifier = game.getLocation() == LocationType.EUROPA ? 3 : 1; //hard-coded reduction in resource min for Europa, should probably move to InfoLocation
                for (ResourceType eLoopResource = 0; eLoopResource < game.infos().resourcesNum(); eLoopResource++)
                {
                    if (game.isResourceValid(eLoopResource))
                    {
                        int iMinResourceRate = game.infos().resource(eLoopResource).miMinRate;

                        for (PlayerType eLoopPlayer = 0; eLoopPlayer < game.getNumPlayers(); eLoopPlayer++)
                        {
                            PlayerServer pLoopPlayer = game.playerServer(eLoopPlayer);

                            int iMaxRate = 0;

                            for (HQType eLoopHQ = 0; eLoopHQ < game.infos().HQsNum(); eLoopHQ++)
                            {
                                if (pLoopPlayer.canFound(eLoopHQ))
                                {
                                    iMaxRate = Math.Max(iMaxRate, game.infos().resource(eLoopResource).maiHQMinRate[(int)eLoopHQ]) / iLocationRateModifier;
                                }
                            }

                            iMinResourceRate += iMaxRate;
                        }

                        if (game.isCampaign())
                        {
                            iMinResourceRate *= 6;
                            iMinResourceRate /= 5;
                        }

                        if (game.gameSettings().mzMap.Length > 0)
                        {
                            iMinResourceRate *= 6;
                            iMinResourceRate /= 5;
                        }

                        iMinResourceRate *= Math.Max(0, (game.infos().resourceMinimum(game.getResourceMinimum()).miModifier + 100));
                        iMinResourceRate /= 100;

                        if (iMinResourceRate > 0)
                        {
                            bool bValid = true;

                            if (bValid)
                            {
                                if (game.infos().resource(eLoopResource).maiLocationAppearanceProb[(int)(game.getLocation())] == 0)
                                {
                                    bValid = false;
                                }
                            }

                            if (bValid)
                            {
                                for (TerrainType eLoopTerrain = 0; eLoopTerrain < game.infos().terrainsNum(); eLoopTerrain++)
                                {
                                    int iRate = game.infos().terrain(eLoopTerrain).maiResourceRate[(int)eLoopResource];
                                    if (iRate > 0)
                                    {
                                        if ((iRate * game.getTerrainCount(eLoopTerrain)) >= ((Constants.RESOURCE_MULTIPLIER * iMinResourceRate) / 100))
                                        {
                                            bValid = false;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (bValid)
                            {
                                for (IceType eLoopIce = 0; eLoopIce < game.infos().icesNum(); eLoopIce++)
                                {
                                    int iIgnoreCount = game.infos().ice(eLoopIce).maiIgnoreMinimum[(int)eLoopResource];

                                    if (game.gameSettings().mzMap.Length > 0)
                                    {
                                        iIgnoreCount *= 5;
                                        iIgnoreCount /= 4;
                                    }

                                    if (iIgnoreCount > 0)
                                    {
                                        if (game.getIceCount(eLoopIce) >= (iIgnoreCount * (int)(game.getNumPlayers())))
                                        {
                                            bValid = false;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (bValid)
                            {
                                int iResourceRateNeeded = (iMinResourceRate - game.getResourceRateCount(eLoopResource));

                                while (iResourceRateNeeded > 0)
                                {
                                    TileServer pBestTile = null;
                                    int iBestValue = 0;

                                    foreach (TileServer pLoopTile in game.tileServerAll())
                                    {
                                        if (!(pLoopTile.noResources()) && pLoopTile.isEmpty() && !(pLoopTile.isGeothermal()) && !(pLoopTile.hasResources()))
                                        {
                                            if ((pColonyTile == null) || (Utils.stepDistanceTile(pLoopTile, pColonyTile) >= (iMaxDist / 10)))
                                            {
                                                int iValue = game.infos().resource(eLoopResource).maiLocationAppearanceProb[(int)(game.getLocation())];

                                                iValue += getResourceProb(eLoopResource, game, pLoopTile, false);

                                                iValue *= 10;
                                                iValue += game.random().Next(1000);

                                                if (iValue > iBestValue)
                                                {
                                                    pBestTile = pLoopTile;
                                                    iBestValue = iValue;
                                                }
                                            }
                                        }
                                    }

                                    if (pBestTile != null)
                                    {
                                        ResourceLevelType eHighestResourceLevel;

                                        if (game.getLocation() == LocationType.EUROPA)
                                            eHighestResourceLevel = (ResourceLevelType)2; //fill out resource min with lows on Europa, probably shouldn't be hard-coded
                                        else
                                            eHighestResourceLevel = game.getHighestResourceLevel();

                                        if ((int)eHighestResourceLevel > 2)
                                        {
                                            if (game.random().Next(2) == 0)
                                            {
                                                eHighestResourceLevel--;
                                            }
                                        }

                                        pBestTile.setResourceLevel(eLoopResource, eHighestResourceLevel);
                                        game.addResourceTileToList(pBestTile, eLoopResource);

                                        iResourceRateNeeded -= game.infos().resourceLevel(eHighestResourceLevel).miRateMultiplier;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        void StriationGenerate(GameServer game, MapClient map) {
            float[] striationValues = new float[map.getTerrainWidth() * map.getTerrainLength()];

            StriationCirclesGrid(14f, 5f, 19f, 0.9f, ref striationValues, map.getTerrainWidth(), map.getTerrainLength(), game);

            TerrainType TERRAIN_VOLCANIC = map.infos().getType<TerrainType>("TERRAIN_VOLCANIC");

            for (int i = 0; i < striationValues.Length; i++) {
                if (striationValues[i] < 0f) {
                    map.terrainInfo(i).Terrain = TERRAIN_VOLCANIC;
                }
            }
        }

        void StriationCirclesGrid(float distPerCircle, float minSize, float maxSize, float wanderDist, ref float[] values, int width, int length, GameServer game) {
            int xCount = Mathf.CeilToInt(width / distPerCircle);
            int yCount = Mathf.CeilToInt(length / distPerCircle);

            int xPos = 0;
            int yPos = 0;
            float radius = 1f;

            int x, y;

            float xRand, yRand, radRand;
            
            for (x = 0; x <= xCount; x++) {
                for (y = 0; y <= yCount; y++) {
                    xRand = (game != null) ? (game.random().Next(2000) - 1000) / 1000f : UnityEngine.Random.value - 0.5f;
                    yRand = (game != null) ? (game.random().Next(2000) - 1000) / 1000f : UnityEngine.Random.value - 0.5f;
                    radRand = (game != null) ? game.random().Next(1000) / 1000f : UnityEngine.Random.value;

                    xPos = Mathf.RoundToInt(x * distPerCircle + (xRand * distPerCircle * wanderDist));
                    yPos = Mathf.RoundToInt(y * distPerCircle + (yRand * distPerCircle * wanderDist));
                    radius = Mathf.Lerp(minSize, maxSize, radRand);

                    StriationPlaceCircle(xPos, yPos, radius, values, width, length, game);
                }
            }
        }

        void StriationPlaceCircle(int x, int y, float radius, float[] values, int width, int length, GameServer game) {
            float angle = 0f;

            float angleOffset = ((game != null) ? game.random().Next(1000) / 1000f : UnityEngine.Random.value) * Mathf.PI * 2f;
            float fadeAngle = ((game != null) ? game.random().Next(1000) / 1000f : UnityEngine.Random.value) * Mathf.PI * 2f;
            float edgeMaxWidth = ((game != null) ? game.random().Next(1000) / 1000f : UnityEngine.Random.value);

            float adjustRadius;
            float fadeValue = 0f;

            edgeMaxWidth = Mathf.Lerp(0.48f, 1.2f, edgeMaxWidth * edgeMaxWidth);

            float fadeX = Mathf.Sin(fadeAngle);
            float fadeY = Mathf.Cos(fadeAngle);

            for (int xPos = 0; xPos < width; xPos++) {
                for (int yPos = 0; yPos < length; yPos++) {
                    angle = Mathf.Atan2(yPos - y, xPos - x);
                    adjustRadius = radius + (Mathf.Sin((angle + angleOffset) * 5f) * radius * 0.02f) + (Mathf.Sin((angle + angleOffset) * 12f) * radius * 0.01f);

                    fadeValue = Mathf.Clamp01(((fadeX * (xPos - x)) + (fadeY * (yPos - y))) / radius + 0.2f);

                    if (Mathf.Abs(StriationTileDistance(x, y, xPos, yPos) - adjustRadius) < edgeMaxWidth * fadeValue) {
                        StriationSetEdgeValue(xPos, yPos, values, width);
                    }
                }
            }
        }

        float StriationTileDistance(int xA, int yA, int xB, int yB) {
            return Vector3.Distance(new Vector3(xA, yA), new Vector3(xB, yB));
        }

        float StriationGetValue(int x, int y, float[] values, int width) {
            return values[x + y * width];
        }

        void StriationSetEdgeValue(int x, int y, float[] values, int width) {
            values[x + (y * width)] = -1f;
        }

        public void postProcessTileHeights(GameServer game)
        {
            postProcessTileHeights(game.mapServer());
        }

        public bool[] postProcessTileHeights(MapClient map)
        {
            bool[] isEdge = new bool[map.numTerrains()];

            AddEdgeBufferTiles(map, isEdge);
            FillEdgeGaps(map, isEdge);

            TerrainType TERRAIN_CANYON = map.infos().getType<TerrainType>("TERRAIN_CANYON");
            TerrainType TERRAIN_SLOPE = map.infos().getType<TerrainType>("TERRAIN_SLOPE");

            for (int iX = 0; iX < map.getTerrainWidth(); iX++)
            {
                for (int iY = 0; iY < map.getTerrainLength(); iY++)
                {
                    TerrainInfo pLoopTerrain = map.terrainInfoGrid(iX, iY);

                    if (pLoopTerrain.Height == (HeightType)0 && ((map.getLocation() != LocationType.EUROPA) || (pLoopTerrain.Terrain != TERRAIN_SLOPE)) )
                    {
                        pLoopTerrain.Terrain = TERRAIN_CANYON;
                    }
                }
            }

            return isEdge;
        }

        void ScrambleTileHeights (MapClient map, GameServer game)
        {
            HeightType[] scrambledHeights = new HeightType[map.numTerrains()];
            for (int i = 0; i < map.numTerrains(); i++)
            {
                if (map.terrainInfo(i).Height != (HeightType)0)
                {
                    int grabDirection = 0;
                    if (game != null)
                        grabDirection = game.random().Next(12);
                    else
                        grabDirection = Mathf.FloorToInt(UnityEngine.Random.value * 12f);

                    grabDirection = (grabDirection < 6) ? grabDirection : -1; // values above the maximum number of directions result in no change

                    int targetID = i;
                    if (grabDirection > -1)
                        targetID = map.terrainInfoIDAdjacent(i, (DirectionType)grabDirection);
                    if (targetID < 0)
                        targetID = i;

                    scrambledHeights[i] = map.terrainInfo(targetID).Height;
                }
                else
                    scrambledHeights[i] = map.terrainInfo(i).Height;
            }

            for (int i = 0; i < map.numTerrains(); i++)
            {
                map.terrainInfo(i).Height = scrambledHeights[i];
            }
        }

        // Passes over the map once for each height level to ensure no tile is more than one height level away from its neighbors.
        void AddEdgeBufferTiles(MapClient map, bool[] isEdge)
        {
            for (HeightType h = (HeightType)(map.infos().heights().Count); h >= 0; h--)
            {
                for (int i = 0; i < map.numTerrains(); i++)
                {
                    TerrainInfo mInfo = map.terrainInfo(i);

                    //if (heights[i] == h)
                    if (mInfo.Height == h)
                    {
                        for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
                        {
                            int adjID = map.terrainInfoIDAdjacent(i, eDirection);
                            TerrainInfo adjInfo = map.terrainInfo(adjID);
                            if (adjInfo != null)
                            {
                                if (adjInfo.Height < h)
                                {
                                    adjInfo.Height = h - 1;

                                    isEdge[adjID] = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        void AddEdgeBufferTiles(MapClient map, bool[] isEdge, int tileID, List<int> recheckTiles)
        {
            TerrainInfo mInfo = map.terrainInfo(tileID);
            for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
            {
                int adjID = map.terrainInfoIDAdjacent(tileID, eDirection);
                TerrainInfo adjInfo = map.terrainInfo(adjID);
                if (adjInfo != null)
                {
                    if (adjInfo.Height < mInfo.Height)
                    {
                        adjInfo.Height = mInfo.Height - 1;
                        recheckTiles.Add(adjID);

                        isEdge[adjID] = true;
                        AddEdgeBufferTiles(map, isEdge, adjID, recheckTiles);
                    }
                }
            }
        }

        // Passes over each tile to ensure that the gap between two raised tiles is at least two tiles wide, so plateau edge meshes will fit.
        // If not, the gap tile is raised to match the tiles above it.
        void FillEdgeGaps(MapClient map, bool[] isEdge)
        {
            List<int> recheckTiles = new List<int>();

            for (int i = 0; i < map.numTerrains(); i++)
            {
                if (isEdge[i])
                {
                    CheckGapFillAtIndex(map, isEdge, i, recheckTiles);
                }
            }

            while (recheckTiles.Count > 0)
            {
                int toCheck = recheckTiles[0];
                recheckTiles.RemoveAt(0);
                CheckGapFillAtIndex(map, isEdge, toCheck, recheckTiles);
            }

            TerrainType TERRAIN_SLOPE = map.infos().getType<TerrainType>("TERRAIN_SLOPE");
            for (int i = 0; i < map.numTerrains(); i++)
            {
                if (isEdge[i])
                {
                    TerrainInfo mInfo = map.terrainInfo(i);
                    mInfo.Terrain = TERRAIN_SLOPE;
                }
            }
        }

        void CheckGapFillAtIndex(MapClient map, bool[] isEdge, int id, List<int> recheckTiles)
        {
            int numAbove = 0;
            int numGaps = 0;
            int numValid = 0;
            bool startOnGap = false;
            bool lastWasAbove = false;

            HeightType aboveHeight = 0;

            TerrainInfo mInfo = map.terrainInfo(id);

            bool firstAdjacent = true;
            for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
            {
                int adjID = map.terrainInfoIDAdjacent(id, eDirection);
                TerrainInfo adjInfo = map.terrainInfo(adjID);
                if (adjInfo != null)
                {
                    if (adjInfo.Height > mInfo.Height)
                    {
                        numAbove++;
                        lastWasAbove = true;
                        aboveHeight = adjInfo.Height;
                    }
                    else
                    {
                        if (firstAdjacent)
                        {
                            startOnGap = true;
                        }

                        if (lastWasAbove)
                        {
                            numGaps++;
                        }

                        lastWasAbove = false;
                    }
                    firstAdjacent = false;
                    numValid++;
                }
                else
                {
                    if (firstAdjacent)
                    {
                        startOnGap = true;
                    }

                    if (lastWasAbove)
                    {
                        numGaps++;
                    }

                    lastWasAbove = false;
                }
            }

            if (lastWasAbove && startOnGap)
            {
                numGaps++;
            }

            if (numGaps > 1 || numAbove >= numValid - 1)
            {
                // tile is surrounded or has too many gaps, so fill it in -- thus it is no longer an edge tile itself
                isEdge[id] = false;
                mInfo.Height = aboveHeight;

                // FIX
                //game.tile(id).getTerrainInfo().meHeight = aboveHeight;

                for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
                {
                    int adjID = map.terrainInfoIDAdjacent(id, eDirection);
                    TerrainInfo adjInfo = map.terrainInfo(adjID);
                    if (adjInfo != null)
                    {
                        if (adjInfo.Height < mInfo.Height)
                        {
                            // add Edge marking to lower tiles and add them to the list to recheck
                            adjInfo.Height = mInfo.Height - 1;
                            isEdge[adjID] = true;
                            recheckTiles.Add(adjID);
                            AddEdgeBufferTiles(map, isEdge, adjID, recheckTiles);
                        }
                    }
                }
            }
        }

        public void PlaceCraterPrefabs(GameServer game)
        {
            MapClient map = game.mapClient();

            if (Globals.Infos.location(map.getLocation()).mbNoCraters)
                return;

            TerrainType TERRAIN_CRATER = map.infos().getType<TerrainType>("TERRAIN_CRATER");
            int iTilesPer = map.infos().terrainClass(map.getTerrainClass()).maiTilesPerTerrain[(int)TERRAIN_CRATER];

            TerrainCraterPreset[] presets = ReadCraterPresetXML(map);

            switch(game.getLocation())
            {
                case LocationType.MARS:
                    presets = presets.Take(presets.Length - 3).ToArray();
                    break;
                case LocationType.EUROPA:
                    presets = presets.Take(presets.Length - 3).Where(x => x.width < 5).ToArray();
                    break;
            }

            int[] appearanceWeighting = new int[presets.Length];
            RebuildWeighting(presets, ref appearanceWeighting);

            int iNumCraterAttempts = (map.numTiles() / Math.Max(10, iTilesPer));
            iNumCraterAttempts *= 6; // compensating for decreased chance of placement compared to simple height-drop method -- this may need tweaking
            if (game.getLocation() == LocationType.CERES)
            {
                iNumCraterAttempts *= 12;
            }

            while (iNumCraterAttempts > 0)
            {
                iNumCraterAttempts--;

                int randomRoll = game.random().Next(appearanceWeighting[presets.Length - 1]);
                int iPresetID = 0;
                while (iPresetID < presets.Length - 1 && randomRoll > appearanceWeighting[iPresetID])
                {
                    iPresetID++;
                }

                int iCraterBorder = 5; // hardcoded -- move to statics class
                int iCraterX = game.random().Next(iCraterBorder, map.getTerrainWidth() - (iCraterBorder + presets[iPresetID].width));
                int iCraterY = game.random().Next(iCraterBorder, map.getTerrainLength() - (iCraterBorder + (presets[iPresetID].centerTiles.Length / presets[iPresetID].width)));

                bool bValidPlacement = true;

                bValidPlacement = CheckCraterPlacement(map, iPresetID, iCraterX, iCraterY, presets);

                if (bValidPlacement)
                {
                    PlaceCrater(map, iPresetID, iCraterX, iCraterY, presets);
                    presets[iPresetID].weighting -= 3000;
                    if (presets[iPresetID].weighting <= 0)
                        presets[iPresetID].weighting = 2000;
                    RebuildWeighting(presets, ref appearanceWeighting);
                }
            }
        }

        private void RebuildWeighting(TerrainCraterPreset[] presets, ref int[] array)
        {
            array[0] = presets[0].weighting;
            for (int i = 1; i < presets.Length; i++)
            {
                array[i] = presets[i].weighting + array[i - 1];
            }
        }

        public bool CheckCraterPlacement(MapClient map, int iPresetID, int iCraterX, int iCraterY, TerrainCraterPreset[] presets)
        {
            TerrainType TERRAIN_SLOPE = map.infos().getType<TerrainType>("TERRAIN_SLOPE");
            bool bValidPlacement = true;

            int iPresetTileID = 0;
            while (bValidPlacement && iPresetTileID < presets[iPresetID].centerTiles.Length)
            {
                if (presets[iPresetID].slopes[iPresetTileID])
                {
                    int iPX = iPresetTileID % presets[iPresetID].width;
                    int iPY = iPresetTileID / presets[iPresetID].width;

                    int iTX = iCraterX + iPX - ((iCraterY % 2) * (iPY % 2));
                    int iTY = iCraterY + iPY;

                    TerrainInfo info = map.terrainInfoGrid(iTX, iTY);
                    if (info == null)
                    {
                        bValidPlacement = false;
                    }
                    else
                    {
                        bValidPlacement = ((info.Terrain != TERRAIN_SLOPE) && (info.Height > (HeightType)0));
                    }
                }
                iPresetTileID++;
            }
            return bValidPlacement;
        }

        public int[] PlaceCrater(MapClient map, int iPresetID, int iCraterX, int iCraterY, TerrainCraterPreset[] presets)
        {
            TerrainType TERRAIN_SLOPE = map.infos().getType<TerrainType>("TERRAIN_SLOPE");
            TerrainType TERRAIN_CRATER = map.infos().getType<TerrainType>("TERRAIN_CRATER");
            TerrainType TERRAIN_CANYON = map.infos().getType<TerrainType>("TERRAIN_CANYON");

            List<int> terrainIDs = new List<int>();

            for (int iPresetTileID = 0; iPresetTileID < presets[iPresetID].centerTiles.Length; iPresetTileID++)
            {
                int iPX = iPresetTileID % presets[iPresetID].width;
                int iPY = iPresetTileID / presets[iPresetID].width;

                int iTX = iCraterX + iPX - ((iCraterY % 2) * (iPY % 2));
                int iTY = iCraterY + iPY;

                int infoID = map.terrainInfoIDGrid(iTX, iTY);
                TerrainInfo info = map.terrainInfo(infoID);

                if (presets[iPresetID].centerTiles[iPresetTileID])
                {
                    info.IsCrater += 1;
                    if (info.Terrain != TERRAIN_SLOPE && info.Terrain != TERRAIN_CANYON)
                    {
                        info.Terrain = TERRAIN_CRATER;
                    }
                }
                if (presets[iPresetID].slopes[iPresetTileID])
                {
                    info.Terrain = TERRAIN_SLOPE;
                    terrainIDs.Add(infoID);
                }
                if (presets[iPresetID].craterChunkIDs[iPresetTileID] >= 0)
                {
                    info.CraterChunkID = presets[iPresetID].craterChunkIDs[iPresetTileID];
                    info.CraterChunkDir = (DirectionType)presets[iPresetID].craterChunkDirections[iPresetTileID];
                }
            }

            return terrainIDs.ToArray();
        }

        public void RemoveCrater(MapClient map, int iPresetID, int iCraterX, int iCraterY, TerrainCraterPreset[] presets)
        {
            TerrainType TERRAIN_CRATER = map.infos().getType<TerrainType>("TERRAIN_CRATER");

            for (int iPresetTileID = 0; iPresetTileID < presets[iPresetID].centerTiles.Length; iPresetTileID++)
            {
                int iPX = iPresetTileID % presets[iPresetID].width;
                int iPY = iPresetTileID / presets[iPresetID].width;

                int iTX = iCraterX + iPX - ((iCraterY % 2) * (iPY % 2));
                int iTY = iCraterY + iPY;

                int infoID = map.terrainInfoIDGrid(iTX, iTY);
                TerrainInfo info = map.terrainInfo(infoID);

                if (presets[iPresetID].centerTiles[iPresetTileID])
                {
                    info.IsCrater -= 1;
                }
                if (presets[iPresetID].slopes[iPresetTileID])
                {
                    info.Terrain = TERRAIN_CRATER;
                }
                if (presets[iPresetID].craterChunkIDs[iPresetTileID] >= 0)
                {
                    info.CraterChunkID = -1;
                }
            }
        }
    }
}