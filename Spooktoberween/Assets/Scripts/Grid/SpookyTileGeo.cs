using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SpookyTileGeo : MonoBehaviour
{
    public Tilemap tilemap;
    public Vector3Int tilePos;

    new Renderer renderer;
    void Start()
    {
        if(!tilemap)
        {
            Debug.LogError("Null tilemap");
            return;
        }

        SpookyTile tile = tilemap.GetTile<SpookyTile>(tilePos);
        if(tile)
        {
            tile.SetGeoInstance(tilePos, this);
        }

        renderer = GetComponent<Renderer>();
    }

    public void SetRenderNormals(bool bNewRenderNormals)
    {
        if(renderer)
        {
            renderer.renderingLayerMask = bNewRenderNormals ? (renderer.renderingLayerMask | 1) : (renderer.renderingLayerMask & ~(uint)1 );
        }
    }
}
