using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif

[ExecuteInEditMode]
public class SpookyTilemap : MonoBehaviour
{
    [System.NonSerialized] Tilemap tilemap;

    Dictionary<Vector3Int, SpookyTileGeo> geoInstances;

    readonly Matrix4x4 cellToLocalWorld = new Matrix4x4(new Vector4(.5f, -.5f, 0f, 0f),
                                                        new Vector4(.25f, .25f, 0f, 0f),
                                                        new Vector4(0f, 0f, 1f, 0f),
                                                        new Vector4(0f, 0f, 0f, 1f)).transpose;

    Matrix4x4 cellToWorld;
    Matrix4x4 worldToCell;

    Vector3Int lastPlayerCell;

    bool bForceFullVisibilityUpdate;

    private void Awake()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif
        tilemap = GetComponent<Tilemap>();
    }

    private void Start()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif

        cellToWorld = tilemap.gameObject.transform.localToWorldMatrix * cellToLocalWorld;
        worldToCell = cellToWorld.inverse;

        SpookyPlayer player = SpookyGameManager.gameManager.player;
        if(player)
        {
            lastPlayerCell = tilemap.WorldToCell(player.transform.position);

            bForceFullVisibilityUpdate = true;
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif

        SpookyPlayer player = SpookyGameManager.gameManager.player;
        if (!player) return;

        Vector3 playerPos = player.transform.position;

        Vector3 playerCellPos = worldToCell.MultiplyPoint(playerPos);
        playerCellPos.z = 0;

        if(bForceFullVisibilityUpdate)
        {
            foreach (Vector3Int cellPos in tilemap.cellBounds.allPositionsWithin)
            {
                if (tilemap.HasTile(cellPos))
                {
                    SpookyTile tile = tilemap.GetTile<SpookyTile>(cellPos);
                    if (tile)
                    {
                        tile.UpdateVisibility(cellPos, playerCellPos, tilemap);
                    }
                }
            }

            bForceFullVisibilityUpdate = false;
        }
        else
        {
            Vector3Int roundedPlayerPos = new Vector3Int(Mathf.FloorToInt(playerCellPos.x), Mathf.FloorToInt(playerCellPos.y), 0);

            int xUpdateMin = Mathf.Min(roundedPlayerPos.x, lastPlayerCell.x);
            int xUpdateMax = Mathf.Max(roundedPlayerPos.x, lastPlayerCell.x);

            int yUpdateMin = Mathf.Min(roundedPlayerPos.y, lastPlayerCell.y);
            int yUpdateMax = Mathf.Max(roundedPlayerPos.y, lastPlayerCell.y);

            BoundsInt cellBounds = tilemap.cellBounds;

            BoundsInt xBounds = new BoundsInt(xUpdateMin, cellBounds.yMin, 0, xUpdateMax - xUpdateMin + 1, cellBounds.size.y, 1);
            BoundsInt yBounds = new BoundsInt(cellBounds.xMin, yUpdateMin, 0, cellBounds.size.x, yUpdateMax - yUpdateMin + 1, 1);

            lastPlayerCell = roundedPlayerPos;

            int numUpdateCells = xBounds.size.x*xBounds.size.y + yBounds.size.x * yBounds.size.y - xBounds.size.y * yBounds.size.x;

            foreach (Vector3Int cellPos in xBounds.allPositionsWithin)
            {
                if (tilemap.HasTile(cellPos))
                {
                    SpookyTile tile = tilemap.GetTile<SpookyTile>(cellPos);
                    if(tile)
                    {
                        tile.UpdateVisibility(cellPos, playerCellPos, tilemap);
                    }
                }
            }

            foreach(Vector3Int cellPos in yBounds.allPositionsWithin)
            {
                if(xBounds.Contains(cellPos)) continue;

                if (tilemap.HasTile(cellPos))
                {
                    SpookyTile tile = tilemap.GetTile<SpookyTile>(cellPos);
                    if (tile)
                    {
                        tile.UpdateVisibility(cellPos, playerCellPos, tilemap);
                    }
                }
            }
        }
    }

#if UNITY_EDITOR
    [SerializeField] GameObject generatedGeoPrefab;

    [System.Serializable]
    public struct SpriteGeoMapEntry
    {
        public GameObject geo;
        public List<Sprite> sprites;
    }

    [SerializeField, SpookySpriteGeoMapEntry] List<SpriteGeoMapEntry> spriteGeoMap = new List<SpriteGeoMapEntry>();

    readonly Matrix4x4 cellToLocalGeo = new Matrix4x4(new Vector4(.5f, -.5f, 0f, 0f),
                                                      new Vector4(.25f, .25f, 0f, 0f),
                                                      new Vector4(.4375f, .4375f, 0f, 10f),
                                                      new Vector4(0f, 0f, 0f, 1f)).transpose;

    const string geoParentName = "GeneratedGeo";

    public void GenerateGeo()
    {
        if(!tilemap)
        {
            Debug.LogError("No tilemap component on gameobject " + gameObject.name + ", cannot generate geo");
            return;
        }

        //Dictionary<Sprite, SpriteGeoMapEntry> spriteGeoDict = new Dictionary<Sprite, SpriteGeoMapEntry>();
        //foreach(SpriteGeoMapEntry mapEntry in spriteGeoMap)
        //{
        //    GameObject geo = mapEntry.geo;
        //    if(geo)
        //    {
        //        foreach(Sprite sprite in mapEntry.sprites)
        //        {
        //            if(spriteGeoDict.ContainsKey(sprite))
        //            {
        //                Debug.LogWarning("Sprite " + sprite.name + " is in the sprite geo map for tilemap " + tilemap.name + " multiple times, only the first instance will be used");
        //            }
        //            else
        //            {
        //                spriteGeoDict.Add(sprite, mapEntry);
        //            }
        //        }
        //    }
        //}

        bool bMadeSceneDirty = false;

        GameObject geoParent = GameObject.Find(geoParentName);
        GameObject tilemapGeoParent;
        string tilemapGeoParentName = tilemap.name + "_" + geoParentName;
        if (geoParent)
        {
            Transform tilemapGeoParentTransform = geoParent.transform.Find(tilemapGeoParentName);
            if(tilemapGeoParentTransform && tilemapGeoParentTransform.gameObject)
            {
                tilemapGeoParent = tilemapGeoParentTransform.gameObject;
                for(int i = tilemapGeoParentTransform.childCount - 1; i >= 0; --i)
                {
                    DestroyImmediate(tilemapGeoParentTransform.GetChild(i).gameObject);
                    bMadeSceneDirty = true;
                }
            }
            else
            {
                tilemapGeoParent = new GameObject(tilemapGeoParentName);
                tilemapGeoParent.transform.SetParent(geoParent.transform);
                bMadeSceneDirty = true;
            }
        }
        else if(generatedGeoPrefab)
        {
            geoParent = Instantiate(generatedGeoPrefab);
            geoParent.name = geoParentName;
            tilemapGeoParent = new GameObject(tilemapGeoParentName);
            tilemapGeoParent.transform.SetParent(geoParent.transform);
            bMadeSceneDirty = true;
        }
        else
        {
            Debug.LogError("SpookyTilemap on " + gameObject.name + " has no generated geo prefab");
            return;
        }

        Matrix4x4 cellToWorldGeo = transform.localToWorldMatrix * cellToLocalGeo;
        foreach(Vector3Int cellPos in tilemap.cellBounds.allPositionsWithin)
        {
            if(tilemap.HasTile(cellPos))
            {
                SpookyTile tile = tilemap.GetTile<SpookyTile>(cellPos);
                if(tile && tile.geoPrefab)
                {
                    Vector3 cellPosFloat = new Vector3(cellPos.x + .5f, cellPos.y + .5f, 0f);
                    Vector3 tileGeoPos = cellToWorldGeo.MultiplyPoint3x4(cellPosFloat);
                    GameObject tileGeoInstance = (GameObject)PrefabUtility.InstantiatePrefab(tile.geoPrefab, gameObject.scene);
                    tileGeoInstance.transform.position = tileGeoPos;
                    tileGeoInstance.transform.rotation = tilemap.transform.rotation;
                    tileGeoInstance.transform.SetParent(tilemapGeoParent.transform, true);
                    tileGeoInstance.name += cellPos.x + "_" + cellPos.y;

                    SpookyTileGeo newGeo = tileGeoInstance.AddComponent<SpookyTileGeo>();
                    if (newGeo)
                    {
                        newGeo.tilePos = cellPos;
                        newGeo.tilemap = tilemap;
                    }

                    bMadeSceneDirty = true;
                }

                //Sprite tileSprite = tilemap.GetSprite(cellPos);

                //SpriteGeoMapEntry tileGeoEntry;
                //if (spriteGeoDict.TryGetValue(tileSprite, out tileGeoEntry))
                //{
                //    Vector3 cellPosFloat = new Vector3(cellPos.x + .5f, cellPos.y + .5f, 0f);
                //    Vector3 tileGeoPos = cellToWorldGeo.MultiplyPoint3x4(cellPosFloat);
                //    GameObject tileGeoInstance = (GameObject)PrefabUtility.InstantiatePrefab(tileGeoEntry.geo, gameObject.scene); 
                //    tileGeoInstance.transform.position = tileGeoPos;
                //    tileGeoInstance.transform.rotation = tilemap.transform.rotation;
                //    tileGeoInstance.transform.SetParent(tilemapGeoParent.transform, true);
                //    tileGeoInstance.name += cellPos.x + "_" + cellPos.y;

                //    SpookyTileGeo newGeo = tileGeoInstance.GetComponent<SpookyTileGeo>();
                //    if(newGeo)
                //    {
                //        newGeo.tilePos = new Vector2Int(cellPos.x, cellPos.y);
                //        newGeo.tilemap = this;
                //    }

                //    bMadeSceneDirty = true;
                //}
            }
        }

        if(bMadeSceneDirty) EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    public void OnValidate()
    {
        if(!tilemap)
        {
            tilemap = GetComponent<Tilemap>();
            if(!tilemap)
            {
                Debug.LogWarning("No tilemap component on gameobject " + gameObject.name);
            }
        }
    }
#endif // UNITY_EDITOR
}
