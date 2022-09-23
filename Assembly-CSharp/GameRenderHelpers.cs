using System.Collections.Generic;
using Offworld.AppCore;
using Offworld.GameCore;
using UnityEngine;
public class GameRenderHelpers : IGameRenderHelpers
{
    public GameObject SpawnAsset(AssetType prefabAsset, Vector3 position, GameObject parent) => null;
    public GameObject SpawnUIAsset(AssetType prefabAsset, GameObject parent) => null;
    public GameObject GetUnit(int unitID) => null;
    public bool IsRendererReady() => false;
    public void AddCaveToTile(TileServer pTile) { }
    public void SetSupersizeResources(List<ResourceType> resources) { }
    public void SetResourcesVisible(bool visibleState) { }
    public void SetResourceHintsVisible(bool visibleState) { }
    public void SetWorldTextVisible(bool visibleState) { }
    public void SetRegionTextVisible(bool visibleState) { }
    public void SetHQsAnimateWhenPaused(bool animateWhenPaused) { }
}
