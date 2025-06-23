using UnityEngine;
using System.Linq;
using System;

public class _WorldSpaceMouseInteractions : MonoBehaviour
{
    enum TileZone { S, W, N, E, SE, SW, NW, NE, C };
    
    public Material HighlightMaterial;

    private GameObject HighLightObject;
    private Coord tileSelected;
    private TileZone zoneSelected;
    private Vector3 tileClickLocation;

    void Start()
    {
        tileSelected = null;
    }

    void Update()
    {
        Destroy(HighLightObject);
        WhichTiles();
    }

    private void WhichTiles()
    {
        if(tileSelected == null)
        {
            RaycastHit hit;
            if(Physics.Raycast(Camera.main.ScreenPointToRay(GetComponentInParent<_Cursor>().PositionToUse), out hit, 100f, 1 << 8))
            {
                if(hit.triangleIndex*3 < hit.transform.GetComponent<MeshCollider>().sharedMesh.GetTriangles(0).Count()) // check if ray hit grass
                {
                    tileClickLocation = hit.point;
                    Vector3 hitPointScaled = new Vector3(hit.point.x/GetComponent<_WorldSpace>().TileSize, hit.point.y/GetComponent<_WorldSpace>().TileHeight, hit.point.z/GetComponent<_WorldSpace>().TileSize);
                    Coord tileHit = new Coord(Mathf.FloorToInt(hitPointScaled.x), Mathf.FloorToInt(hitPointScaled.z));
                    Vector2 whereOnTileHit = new Vector2(hitPointScaled.x - tileHit.x, hitPointScaled.z - tileHit.y);
                    TileZone zoneHit = TileZoneTeller(whereOnTileHit, 0); // Centres only

                    if(Input.GetMouseButtonDown(0))
                    {
                        tileSelected = tileHit;
                        zoneSelected = zoneHit;
                        GetComponentInParent<_Cursor>().Mode |= CursorMode.HeightEdit;
                        GetComponentInParent<_Main>().TheCam.FocusY = true;
                        GetComponentInParent<_Main>().TheCam.FocusPoint = tileClickLocation;

                        WhichTiles(); // back to top 
                    }
                    else
                    {
                        HighlightTile(tileHit, zoneHit, 0);
                    }
                }
            }
        }
        else
        {
            if(Input.GetMouseButtonUp(0))
            {
                tileSelected = null;
                GetComponentInParent<_Cursor>().Mode &= ~CursorMode.HeightEdit;
                GetComponentInParent<_Main>().TheCam.FocusY = false;
                WhichTiles(); // back to top 
            }
            else
            {
                HighlightTile(tileSelected, zoneSelected, 0);

                Ray ray = Camera.main.ScreenPointToRay(GetComponentInParent<_Cursor>().PositionToUse);
                float rayFlatDistance = Mathf.Sqrt(Mathf.Pow(tileClickLocation.x - ray.origin.x, 2) + Mathf.Pow(tileClickLocation.z - ray.origin.z, 2));
                float rayDirectionFlatProportion = Mathf.Sqrt(Mathf.Pow(ray.direction.x, 2) + Mathf.Pow(ray.direction.z, 2));
                int height = Mathf.RoundToInt((ray.origin.y + (rayFlatDistance / rayDirectionFlatProportion) * ray.direction.y) / GetComponent<_WorldSpace>().TileHeight);

                ChangeTileHandler(GetComponent<_WorldSpace>().ChildWorld.GetTile(tileSelected), height, zoneSelected);
            }
        }
    }

    private void ChangeTileHandler(Tile tileToEdit, int height, TileZone tileZone)
    {
        Tile oldTile = GetComponentInParent<_Main>().TheWorldSpace.ChildWorld.GetTile(tileToEdit.WorldCoord);

        //todo validate height here 

        if(tileZone == TileZone.C)
        {
            tileToEdit.h1 = height;
            tileToEdit.h2 = height;
            tileToEdit.h3 = height;
            tileToEdit.h4 = height;
        }
        else if ((tileZone == TileZone.SE)||(tileZone == TileZone.SW)||(tileZone == TileZone.NW)||(tileZone == TileZone.NE))
        {
            int cornerNum = ((int) tileZone) - 4;
            int[] heights = new int[] { tileToEdit.h1, tileToEdit.h2,  tileToEdit.h3, tileToEdit.h4 };

            heights[cornerNum] = height;
            heights[Misc.Mod(cornerNum + 1, 4)] = heights[cornerNum] + Math.Sign(heights[Misc.Mod(cornerNum + 1, 4)] - heights[cornerNum]);
            heights[Misc.Mod(cornerNum - 1, 4)] = heights[cornerNum] + Math.Sign(heights[Misc.Mod(cornerNum - 1, 4)] - heights[cornerNum]);
            heights[Misc.Mod(cornerNum + 2, 4)] = heights[Misc.Mod(cornerNum + 1, 4)] + Math.Sign(heights[Misc.Mod(cornerNum + 2, 4)] - heights[Misc.Mod(cornerNum + 1, 4)]);
            heights[Misc.Mod(cornerNum - 2, 4)] = heights[Misc.Mod(cornerNum - 1, 4)] + Math.Sign(heights[Misc.Mod(cornerNum - 2, 4)] - heights[Misc.Mod(cornerNum - 1, 4)]);

            tileToEdit.h1 = heights[0];
            tileToEdit.h2 = heights[1];
            tileToEdit.h3 = heights[2];
            tileToEdit.h4 = heights[3];
        }

        GetComponentInParent<_Main>().TheWorldSpace.ReplaceTile(tileToEdit);
    }

    private TileZone TileZoneTeller(Vector2 whereOnTileHit, int type)
    {
        // types: 0 all, 1 centre only, 2 edges only, 3 corners only

        if ((type == 0) || (type == 1) || (type == 3))
        {
            float boundaryNum = 0; 
            switch(type)
            {
                case 0: boundaryNum = 0.33f; break;
                case 1: boundaryNum = 0.0f; break;
                case 3: boundaryNum = 0.5f; break;
            }

            if(whereOnTileHit.y > 1-boundaryNum)
            {
                if(whereOnTileHit.x > 1-boundaryNum)
                    return TileZone.NW;
                else if(whereOnTileHit.x < boundaryNum)
                    return TileZone.NE;
                else
                    return TileZone.N;
            }
            else if(whereOnTileHit.y < boundaryNum)
            {
                if(whereOnTileHit.x > 1-boundaryNum)
                    return TileZone.SW;
                else if(whereOnTileHit.x < boundaryNum)
                    return TileZone.SE;
                else
                    return TileZone.S;
            }
            else if(whereOnTileHit.x > 1-boundaryNum)
                return TileZone.W;
            else if(whereOnTileHit.x < boundaryNum)
                return TileZone.E;
            else
                return TileZone.C;
        }
        else
        {
            if(whereOnTileHit.y > whereOnTileHit.x)
            {
                if(whereOnTileHit.y > -whereOnTileHit.x + 1)
                    return TileZone.N;
                else
                    return TileZone.E;
            }
            else
            {
                if(whereOnTileHit.y > -whereOnTileHit.x + 1)
                    return TileZone.W;
                else
                    return TileZone.S;
            }
        }
    }

    private void HighlightTile(Coord tileHit, TileZone zoneHit, int cornerType)
    {
        Tile tile = GetComponentInParent<_Main>().TheWorldSpace.ChildWorld.GetTile(tileHit);

        MeshBuilder meshBuilder = new MeshBuilder();

        float tileSize = GetComponent<_WorldSpace>().TileSize;
        float tileHeight = GetComponent<_WorldSpace>().TileHeight;

        Vector3 middlePoint;
        if(tile.h1 == tile.h3)
            middlePoint = new Vector3(tileSize/2, tile.h1*(tileHeight), tileSize/2);
        else
            middlePoint = new Vector3(tileSize/2, (tile.h2 + tile.h4)*(tileHeight/2), tileSize/2);

        Vector3[] potentialCorners =
        {
            new Vector3(0, tile.h1*tileHeight + 0.01f, 0),
            new Vector3(tileSize, tile.h2*tileHeight + 0.01f, 0),
            new Vector3(tileSize, tile.h3*tileHeight + 0.01f, tileSize),
            new Vector3(0, tile.h4*tileHeight + 0.01f, tileSize)
        };

        if((int)zoneHit < 8)
        {
            if((int)zoneHit < 4)
            {
                //edges
                int vertCount = meshBuilder.Vertices.Count;

                meshBuilder.Vertices.Add(potentialCorners[Misc.Mod((int)zoneHit, 4)]);
                meshBuilder.Normals.Add(Vector3.up);

                meshBuilder.Vertices.Add(middlePoint);
                meshBuilder.Normals.Add(Vector3.up);

                meshBuilder.Vertices.Add(potentialCorners[Misc.Mod((int)zoneHit + 1, 4)]);
                meshBuilder.Normals.Add(Vector3.up);

                meshBuilder.AddTriangle(vertCount, vertCount + 1, vertCount + 2, 0);
            }
            else
            {
                //corners
                int vertCount = meshBuilder.Vertices.Count;

                Vector3[] v =
                {
                    potentialCorners[Misc.Mod((int)zoneHit, 4)],
                    potentialCorners[Misc.Mod((int)zoneHit - 1, 4)],
                    potentialCorners[Misc.Mod((int)zoneHit + 1, 4)],
                    middlePoint
                };

                // optional small corners
                if(cornerType == 1)
                {
                    // 0.707 is half sqrt 2, it keeps the corner highlighting the same size as the edge highlighting
                    v[1] = v[0] + 0.707f * (potentialCorners[Misc.Mod((int)zoneHit - 1, 4)] - v[0]);
                    v[2] = v[0] + 0.707f * (potentialCorners[Misc.Mod((int)zoneHit + 1, 4)] - v[0]);
                    v[3] = v[0] + 0.707f * (middlePoint - v[0]);
                }

                meshBuilder.Vertices.Add(v[0]);
                meshBuilder.Normals.Add(Vector3.up);

                meshBuilder.Vertices.Add(v[3]);
                meshBuilder.Normals.Add(Vector3.up);

                meshBuilder.Vertices.Add(v[2]);
                meshBuilder.Normals.Add(Vector3.up);

                meshBuilder.AddTriangle(vertCount, vertCount + 1, vertCount + 2, 0);

                meshBuilder.Vertices.Add(v[0]);
                meshBuilder.Normals.Add(Vector3.up);

                meshBuilder.Vertices.Add(v[1]);
                meshBuilder.Normals.Add(Vector3.up);

                meshBuilder.Vertices.Add(v[3]);
                meshBuilder.Normals.Add(Vector3.up);

                meshBuilder.AddTriangle(vertCount + 3, vertCount + 4, vertCount + 5, 0);
            }
        }
        else
        {
            //centre
            int vertCount = meshBuilder.Vertices.Count;

            meshBuilder.Vertices.Add(potentialCorners[0]);
            meshBuilder.Normals.Add(Vector3.up);

            meshBuilder.Vertices.Add(middlePoint);
            meshBuilder.Normals.Add(Vector3.up);

            meshBuilder.Vertices.Add(potentialCorners[1]);
            meshBuilder.Normals.Add(Vector3.up);

            meshBuilder.AddTriangle(vertCount, vertCount + 1, vertCount + 2, 0);

            meshBuilder.Vertices.Add(potentialCorners[1]);
            meshBuilder.Normals.Add(Vector3.up);

            meshBuilder.Vertices.Add(middlePoint);
            meshBuilder.Normals.Add(Vector3.up);

            meshBuilder.Vertices.Add(potentialCorners[2]);
            meshBuilder.Normals.Add(Vector3.up);

            meshBuilder.AddTriangle(vertCount + 3, vertCount + 4, vertCount + 5, 0);

            meshBuilder.Vertices.Add(potentialCorners[2]);
            meshBuilder.Normals.Add(Vector3.up);

            meshBuilder.Vertices.Add(middlePoint);
            meshBuilder.Normals.Add(Vector3.up);

            meshBuilder.Vertices.Add(potentialCorners[3]);
            meshBuilder.Normals.Add(Vector3.up);

            meshBuilder.AddTriangle(vertCount + 6, vertCount + 7, vertCount + 8, 0);

            meshBuilder.Vertices.Add(potentialCorners[3]);
            meshBuilder.Normals.Add(Vector3.up);

            meshBuilder.Vertices.Add(middlePoint);
            meshBuilder.Normals.Add(Vector3.up);

            meshBuilder.Vertices.Add(potentialCorners[0]);
            meshBuilder.Normals.Add(Vector3.up);

            meshBuilder.AddTriangle(vertCount + 9, vertCount + 10, vertCount + 11, 0);
        }

        Mesh mesh = meshBuilder.CreateMesh();
        mesh.RecalculateNormals();

        HighLightObject = new GameObject("Highlight");

        HighLightObject.transform.position = new Vector3(tileHit.x*GetComponent<_WorldSpace>().TileSize, 0, tileHit.y*GetComponent<_WorldSpace>().TileSize);
        HighLightObject.transform.rotation = Quaternion.identity;
        HighLightObject.transform.parent = this.transform.parent;

        HighLightObject.layer = 0;

        HighLightObject.AddComponent<MeshFilter>();
        HighLightObject.GetComponent<MeshFilter>().sharedMesh = mesh;

        HighLightObject.AddComponent<MeshRenderer>();
        HighLightObject.GetComponent<MeshRenderer>().material = HighlightMaterial;
    }
}