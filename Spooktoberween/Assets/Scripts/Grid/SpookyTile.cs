using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewSpookyTile", menuName = "2D/Tiles/SpookyTile", order = 0)]
public class SpookyTile : Tile
{
    public enum EWallHideBehavior
    {
        None,
        X,
        Y,
        XOrY,
        XAndY
    }

    public Sprite VisibleSprite;
    public Sprite HiddenSprite;
    public EWallHideBehavior hideBehavior;
    public Vector2 hidePivot;
    public GameObject geoPrefab;

    public struct PerCellData
    {
        public bool bIsVisible;
        public SpookyTileGeo geoInstance;
    }

    Dictionary<Vector3Int, PerCellData> cellData = new Dictionary<Vector3Int, PerCellData>();

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);

        PerCellData data;
        if(cellData.TryGetValue(position, out data))
        tileData.sprite = data.bIsVisible ? VisibleSprite : HiddenSprite;
    }

    public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
    {
        if(!cellData.ContainsKey(position)) cellData.Add(position, new PerCellData(){bIsVisible = true});
        return base.StartUp(position, tilemap, go);
    }

    public void UpdateVisibility(Vector3Int position, Vector3 playerTilePos, Tilemap tilemap)
    {
        if (hideBehavior == EWallHideBehavior.None) return;

        PerCellData data;
        if (cellData.TryGetValue(position, out data))
        {

            bool bHideX = playerTilePos.x > (position.x + hidePivot.x);
            bool bHideY = playerTilePos.y > (position.y + hidePivot.y);

            bool bWasVisible = data.bIsVisible;

            switch (hideBehavior)
            {
                case EWallHideBehavior.X:
                    data.bIsVisible = !bHideX;
                    break;
                case EWallHideBehavior.Y:
                    data.bIsVisible = !bHideY;
                    break;
                case EWallHideBehavior.XOrY:
                    data.bIsVisible = !(bHideX | bHideY);
                    break;
                case EWallHideBehavior.XAndY:
                    data.bIsVisible = !(bHideX & bHideY);
                    break;
            }

            if(bWasVisible != data.bIsVisible) 
            {
                cellData[position] = data;

                if(data.geoInstance)
                {
                    data.geoInstance.SetRenderNormals(data.bIsVisible);
                }

                tilemap.RefreshTile(position);
            }
        }
    }

    public void SetGeoInstance(Vector3Int position, SpookyTileGeo geo)
    {
        PerCellData data;
        if(cellData.TryGetValue(position, out data))
        {
            data.geoInstance = geo;
            cellData.Add(position, data);
        }
        else
        {
            data.bIsVisible = true;
            data.geoInstance = geo;
            cellData.Add(position, data);
        }
    }
}
