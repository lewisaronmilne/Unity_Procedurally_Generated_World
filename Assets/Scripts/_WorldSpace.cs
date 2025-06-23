using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class _WorldSpace : MonoBehaviour
{
    public float TileSize, TileHeight;
    public int ChunkSize, MaxChunks, ChunksToCull, ChunkSpawnRadius, TileTextureWidth, TileTextureHeight, MaxHeight, MinHeight;
    public Texture2D TileTextureSheet;

    public World ChildWorld;

    private Meshes TheMeshes;
    private TileMaterials TheTileMaterials;
    private Dictionary<Coord,Chunk> OnScreenChunks;

    void Start()
    {
        ChildWorld = new World(ChunkSize);
        TheMeshes = new Meshes();
        TheTileMaterials = new TileMaterials(this);
        OnScreenChunks = new Dictionary<Coord, Chunk>();
    }

    // get chunk from data and add it to the sreen
    public void PutChunk(Coord coord)
    {
        Chunk chunk = ChildWorld.GetChunk(coord);

        // only put chunk on screen if it's not on screen already
        if (!OnScreenChunks.ContainsKey(chunk.WorldCoord))
        {
            //if too many chunk on screen, get rid of some of them
            OnScreenChunks.Add(chunk.WorldCoord, chunk);
            if(OnScreenChunks.Count > MaxChunks)
                CullChunks();

            MeshBuilder meshBuilder = new MeshBuilder();
            int min = 0;

            for(int y = 0; y < chunk.Size; y++)
                for(int x = 0; x < chunk.Size; x++)
                    BuildTile(meshBuilder, chunk.Tiles[x, y], new Vector3(x, -min, y));

            Mesh mesh = meshBuilder.CreateMesh();
            mesh.RecalculateNormals();

            GameObject tileInWorld = new GameObject(coord.x + ", " + coord.y);

            tileInWorld.transform.position = new Vector3(coord.x*ChunkSize*TileSize, min*TileHeight, coord.y*ChunkSize*TileSize);
            tileInWorld.transform.rotation = Quaternion.identity;
            tileInWorld.transform.parent = this.transform;

            tileInWorld.layer = 8;

            tileInWorld.AddComponent<MeshFilter>();
            tileInWorld.GetComponent<MeshFilter>().sharedMesh = mesh;

            Material[] TileMats =
            {
                TheTileMaterials.Get(new Coord(0, 0)),
                TheTileMaterials.Get(new Coord(1, 0))
            };
            tileInWorld.AddComponent<MeshRenderer>();
            tileInWorld.GetComponent<MeshRenderer>().materials = TileMats;

            tileInWorld.AddComponent<MeshCollider>();
            tileInWorld.GetComponent<MeshCollider>().sharedMesh = mesh;

            // keep track of the actual game instance data so you can remove it later
            List<GameObject> meshes = new List<GameObject>();
            meshes.Add(tileInWorld);
            TheMeshes.Put(coord, meshes);
        }
    }

    public void ReplaceChunk(Chunk chunk)
    {
        ChildWorld.ReplaceChunk(chunk);
        RemoveChunk(chunk.WorldCoord);
        RemoveChunk(new Coord(chunk.WorldCoord.x - 1, chunk.WorldCoord.y));
        RemoveChunk(new Coord(chunk.WorldCoord.x, chunk.WorldCoord.y -1));
        PutChunk(chunk.WorldCoord);
        PutChunk(new Coord(chunk.WorldCoord.x - 1, chunk.WorldCoord.y));
        PutChunk(new Coord(chunk.WorldCoord.x, chunk.WorldCoord.y -1));
    }

    public void ReplaceTile(Tile tile)
    {
        Chunk chunk = ChildWorld.GetChunk(tile.ParentChunk.WorldCoord);
        chunk.Tiles[tile.ChunkCoord.x, tile.ChunkCoord.y] = tile;
        ReplaceChunk(chunk);
    }

    // remove chunk from screen
    private void RemoveChunk(Coord coord)
    {
        if(OnScreenChunks.ContainsKey(coord))
        {
            OnScreenChunks.Remove(coord);
            TheMeshes.Remove(new Coord(coord.x, coord.y));
        }
    }

    // remove furthest on screen chunks from the screen
    private void CullChunks()
    {
        _Player player = transform.GetComponentInParent<_Main>().ThePlayer;
        Coord playerChunkCoord = new Coord(Mathf.FloorToInt(player.transform.position.x/ChunkSize), Mathf.FloorToInt(player.transform.position.z/ChunkSize));

        Chunk[] onScreenChunksArr = new Chunk[OnScreenChunks.Count];
        OnScreenChunks.Values.CopyTo(onScreenChunksArr, 0);

        // sort chunks by distance to player
        Array.Sort(onScreenChunksArr, delegate(Chunk a, Chunk b)
        {
            float aDist = Coord.DistanceBetween(playerChunkCoord, a.WorldCoord);
            float bDist = Coord.DistanceBetween(playerChunkCoord, b.WorldCoord);
            if(aDist == bDist)
                return 0;
            else
                return aDist < bDist ? -1 : 1;
        });

        for (int i = onScreenChunksArr.Length-ChunksToCull-1; i < onScreenChunksArr.Length; i++)
        {
            RemoveChunk(onScreenChunksArr[i].WorldCoord);
        }
    }

    // add tile to screen
    private void BuildTile(MeshBuilder meshBuilder, Tile tile, Vector3 offset)
    {
        int h1 = tile.h1;
        int h2 = tile.h2;
        int h3 = tile.h3;
        int h4 = tile.h4;

        // building top of tile
        // change Tile orientation if it would cause an odd looking tile
        if(h1 == h3)
        {
            int vertCount = meshBuilder.Vertices.Count;

            meshBuilder.Vertices.Add(new Vector3(0, h1 * TileHeight, 0) + offset);
            meshBuilder.Normals.Add(Vector3.up);
            meshBuilder.UVs.Add(new Vector2(0, 0));

            meshBuilder.Vertices.Add(new Vector3(0, h4 * TileHeight, TileSize) + offset);
            meshBuilder.Normals.Add(Vector3.up);
            meshBuilder.UVs.Add(new Vector2(0, 1));

            meshBuilder.Vertices.Add(new Vector3(TileSize, h3 * TileHeight, TileSize) + offset);
            meshBuilder.Normals.Add(Vector3.up);
            meshBuilder.UVs.Add(new Vector2(1, 1));

            meshBuilder.AddTriangle(vertCount, vertCount + 1, vertCount + 2, 0);

            meshBuilder.Vertices.Add(new Vector3(0, h1 * TileHeight, 0) + offset);
            meshBuilder.Normals.Add(Vector3.up);
            meshBuilder.UVs.Add(new Vector2(0, 0));

            meshBuilder.Vertices.Add(new Vector3(TileSize, h3 * TileHeight, TileSize) + offset);
            meshBuilder.Normals.Add(Vector3.up);
            meshBuilder.UVs.Add(new Vector2(1, 1));

            meshBuilder.Vertices.Add(new Vector3(TileSize, h2 * TileHeight, 0) + offset);
            meshBuilder.Normals.Add(Vector3.up);
            meshBuilder.UVs.Add(new Vector2(1, 0));

            meshBuilder.AddTriangle(vertCount + 3, vertCount + 4, vertCount + 5, 0);
        }
        else
        {
            int vertCount = meshBuilder.Vertices.Count;

            meshBuilder.Vertices.Add(new Vector3(0, h1 * TileHeight, 0) + offset);
            meshBuilder.Normals.Add(Vector3.up);
            meshBuilder.UVs.Add(new Vector2(0, 0));

            meshBuilder.Vertices.Add(new Vector3(0, h4 * TileHeight, TileSize) + offset);
            meshBuilder.Normals.Add(Vector3.up);
            meshBuilder.UVs.Add(new Vector2(0, 1));

            meshBuilder.Vertices.Add(new Vector3(TileSize, h2 * TileHeight, 0) + offset);
            meshBuilder.Normals.Add(Vector3.up);
            meshBuilder.UVs.Add(new Vector2(1, 0));

            meshBuilder.AddTriangle(vertCount, vertCount + 1, vertCount + 2, 0);

            meshBuilder.Vertices.Add(new Vector3(0, h4 * TileHeight, TileSize) + offset);
            meshBuilder.Normals.Add(Vector3.up);
            meshBuilder.UVs.Add(new Vector2(0, 1));

            meshBuilder.Vertices.Add(new Vector3(TileSize, h3 * TileHeight, TileSize) + offset);
            meshBuilder.Normals.Add(Vector3.up);
            meshBuilder.UVs.Add(new Vector2(1, 1));

            meshBuilder.Vertices.Add(new Vector3(TileSize, h2 * TileHeight, 0) + offset);
            meshBuilder.Normals.Add(Vector3.up);
            meshBuilder.UVs.Add(new Vector2(1, 0));

            meshBuilder.AddTriangle(vertCount + 3, vertCount + 4, vertCount + 5, 0);
        }

        // building north face of tile
        // only built on north and east side of the tile, and only if the tile either side of the edge misallign
        Tile northTile = ChildWorld.GetTile(new Coord(tile.WorldCoord.x, tile.WorldCoord.y + 1));

        int n1 = h4;
        int n2 = northTile.h1;
        int n3 = h3;
        int n4 = northTile.h2;

        if(n1 != n2)
        {
            int vertCount = meshBuilder.Vertices.Count;

            meshBuilder.Vertices.Add(new Vector3(0, n1 * TileHeight, TileSize) + offset);
            meshBuilder.Normals.Add(Vector3.up);
            meshBuilder.UVs.Add(new Vector2(0, n1));

            meshBuilder.Vertices.Add(new Vector3(0, n2 * TileHeight, TileSize) + offset);
            meshBuilder.Normals.Add(Vector3.up);
            meshBuilder.UVs.Add(new Vector2(0, n2));

            meshBuilder.Vertices.Add(new Vector3(TileSize, n4 * TileHeight, TileSize) + offset);
            meshBuilder.Normals.Add(Vector3.up);
            meshBuilder.UVs.Add(new Vector2(1, n4));

            meshBuilder.AddTriangle(vertCount, vertCount + 1, vertCount + 2, 1);
        }

        if(n3 != n4)
        {
            int vertCount = meshBuilder.Vertices.Count;

            meshBuilder.Vertices.Add(new Vector3(0, n1 * TileHeight, TileSize) + offset);
            meshBuilder.Normals.Add(Vector3.up);
            meshBuilder.UVs.Add(new Vector2(0, n1));

            meshBuilder.Vertices.Add(new Vector3(TileSize, n4 * TileHeight, TileSize) + offset);
            meshBuilder.Normals.Add(Vector3.up);
            meshBuilder.UVs.Add(new Vector2(1, n4));

            meshBuilder.Vertices.Add(new Vector3(TileSize, n3 * TileHeight, TileSize) + offset);
            meshBuilder.Normals.Add(Vector3.up);
            meshBuilder.UVs.Add(new Vector2(1, n3));

            meshBuilder.AddTriangle(vertCount, vertCount + 1, vertCount + 2, 1);
        }

        // building east face of tile
        Tile eastTile = ChildWorld.GetTile(new Coord(tile.WorldCoord.x + 1, tile.WorldCoord.y));

        int e1 = eastTile.h1;
        int e2 = h2;
        int e3 = eastTile.h4;
        int e4 = h3;

        if(e1 != e2)
        {
            int vertCount = meshBuilder.Vertices.Count;

            meshBuilder.Vertices.Add(new Vector3(TileSize, e1 * TileHeight, 0) + offset);
            meshBuilder.Normals.Add(Vector3.up);
            meshBuilder.UVs.Add(new Vector2(0, e1));

            meshBuilder.Vertices.Add(new Vector3(TileSize, e2 * TileHeight, 0) + offset);
            meshBuilder.Normals.Add(Vector3.up);
            meshBuilder.UVs.Add(new Vector2(0, e2));

            meshBuilder.Vertices.Add(new Vector3(TileSize, e4 * TileHeight, TileSize) + offset);
            meshBuilder.Normals.Add(Vector3.up);
            meshBuilder.UVs.Add(new Vector2(1, e4));

            meshBuilder.AddTriangle(vertCount, vertCount + 1, vertCount + 2, 1);
        }

        if(e3 != e4)
        {
            int vertCount = meshBuilder.Vertices.Count;

            meshBuilder.Vertices.Add(new Vector3(TileSize, e1 * TileHeight, 0) + offset);
            meshBuilder.Normals.Add(Vector3.up);
            meshBuilder.UVs.Add(new Vector2(0, e1));

            meshBuilder.Vertices.Add(new Vector3(TileSize, e4 * TileHeight, TileSize) + offset);
            meshBuilder.Normals.Add(Vector3.up);
            meshBuilder.UVs.Add(new Vector2(1, e4));

            meshBuilder.Vertices.Add(new Vector3(TileSize, e3 * TileHeight, TileSize) + offset);
            meshBuilder.Normals.Add(Vector3.up);
            meshBuilder.UVs.Add(new Vector2(1, e3));

            meshBuilder.AddTriangle(vertCount, vertCount + 1, vertCount + 2, 1);
        }
    }
}