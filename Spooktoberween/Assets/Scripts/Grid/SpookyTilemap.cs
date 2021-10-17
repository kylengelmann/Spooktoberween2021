using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

[ExecuteInEditMode]
public class SpookyTilemap : MonoBehaviour
{
#if UNITY_EDITOR
    [System.NonSerialized] Tilemap tilemap;

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

        Dictionary<Sprite, GameObject> spriteGeoDict = new Dictionary<Sprite, GameObject>();
        foreach(SpriteGeoMapEntry mapEntry in spriteGeoMap)
        {
            GameObject geo = mapEntry.geo;
            if(geo)
            {
                foreach(Sprite sprite in mapEntry.sprites)
                {
                    if(spriteGeoDict.ContainsKey(sprite))
                    {
                        Debug.LogWarning("Sprite " + sprite.name + " is in the sprite geo map for tilemap " + tilemap.name + " multiple times, only the first instance will be used");
                    }
                    else
                    {
                        spriteGeoDict.Add(sprite, geo);
                    }
                }
            }
        }

        bool bMadeSceneDirty = false;

        GameObject geoParent = GameObject.Find(geoParentName);
        GameObject tilemapGeoParent;
        string tilemapGeoParentName = tilemap.name + "_" + geoParentName;
        if (geoParent)
        {
            Transform tilemapGeoParentTransform = geoParent.transform.Find(tilemapGeoParentName);
            if(tilemapGeoParentTransform)
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
        else
        {
            geoParent = new GameObject(geoParentName);
            tilemapGeoParent = new GameObject(tilemapGeoParentName);
            tilemapGeoParent.transform.SetParent(geoParent.transform);
            bMadeSceneDirty = true;
        }

        Matrix4x4 cellToWorldGeo = transform.localToWorldMatrix * cellToLocalGeo;
        foreach(Vector3Int cellPos in tilemap.cellBounds.allPositionsWithin)
        {
            if(tilemap.HasTile(cellPos))
            {
                Sprite tileSprite = tilemap.GetSprite(cellPos);

                GameObject tileGeo;
                if (spriteGeoDict.TryGetValue(tileSprite, out tileGeo))
                {
                    Vector3 cellPosFloat = new Vector3(cellPos.x + .5f, cellPos.y + .5f, 0f);
                    Vector3 tileGeoPos = cellToWorldGeo.MultiplyPoint3x4(cellPosFloat);
                    GameObject tileGeoInstance = Instantiate(tileGeo, tileGeoPos, tilemap.transform.rotation);
                    tileGeoInstance.transform.SetParent(tilemapGeoParent.transform, true);
                    tileGeoInstance.name += cellPos.x + "_" + cellPos.y;

                    bMadeSceneDirty = true;
                }
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
