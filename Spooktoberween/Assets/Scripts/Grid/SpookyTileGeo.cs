using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SpookyTileGeo : MonoBehaviour
{
    public Tilemap tilemap;
    public Vector3Int tilePos;

    List<Renderer> renderers = new List<Renderer>();
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

        Renderer myRenderer = GetComponent<Renderer>();
        if(myRenderer) renderers.Add(myRenderer);

        foreach(Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            if(renderer) renderers.Add(renderer);
        }
    }

    public void SetRenderNormals(bool bNewRenderNormals)
    {
        foreach(Renderer renderer in renderers)
        {
            renderer.renderingLayerMask = bNewRenderNormals ? (renderer.renderingLayerMask | 1) : (renderer.renderingLayerMask & ~(uint)1 );
        }
    }
}
