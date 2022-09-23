using UnityEngine;
using System.Collections.Generic;
using Offworld.GameCore;

namespace Offworld.AppCore
{
    public interface IGameRenderHelpers
    {
        GameObject SpawnAsset(AssetType prefabAsset, Vector3 position, GameObject parent);
        GameObject SpawnUIAsset(AssetType prefabAsset, GameObject parent);
        GameObject GetUnit(int unitID);
        bool IsRendererReady();
        void AddCaveToTile(TileServer pTile);
        void SetSupersizeResources(List<ResourceType> resources);
        void SetResourcesVisible(bool visibleState);
        void SetResourceHintsVisible(bool visibleState);
        void SetWorldTextVisible(bool visibleState);
        void SetRegionTextVisible(bool visibleState);
        void SetHQsAnimateWhenPaused(bool animateWhenPaused);
    }
}
