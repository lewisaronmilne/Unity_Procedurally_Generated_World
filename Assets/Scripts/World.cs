using System;
using System.Collections.Generic;

// the data structure here goes, A World contains many Chunks, which each contain the same ammount of Tiles 

public class World
{
    public int ChunkSize;
    public WorldGen Gen;

    // only one chunk exists at any coord
    private Dictionary<Coord, Chunk> Chunks = new Dictionary<Coord, Chunk>();

    public World(int chunkSize)
    {
        this.ChunkSize = chunkSize;
        Gen = new WorldGen();
    }
    
    // if already exists return what it found, if not then make one and return it
    public Chunk GetChunk(Coord coord)
    {
        Chunk chunk;
        if(Chunks.TryGetValue(coord, out chunk))
            return chunk;
        else
        {
            int[][][] heightMap = new int[ChunkSize][][];
            for(int x = 0; x < ChunkSize; x++)
            {
                heightMap[x] = new int[ChunkSize][];
                for(int y = 0; y < ChunkSize; y++)
                {
                    heightMap[x][y] = Gen.HeightAt(new Coord(coord.x*ChunkSize+x, coord.y*ChunkSize+y));
                }
            }

            chunk = new Chunk(this, coord, heightMap);
            Chunks.Add(coord, chunk);
            return chunk;
        }
    }

    public void ReplaceChunk(Chunk chunk)
    {
        if(Chunks.ContainsKey(chunk.WorldCoord))
            Chunks[chunk.WorldCoord] = chunk;
        else
            Chunks.Add(chunk.WorldCoord, chunk);
    }

    // will create a new chunk if one does not exist at the location that the tile it is looking for is at
    public Tile GetTile(Coord worldCoord)
    {
        Coord chunkCoord = new Coord((int)Math.Floor(worldCoord.x/(float)ChunkSize), (int)Math.Floor(worldCoord.y/(float)ChunkSize));
        Chunk chunk = GetChunk(chunkCoord);
        return chunk.Tiles[worldCoord.x - chunkCoord.x*ChunkSize, worldCoord.y - chunkCoord.y*ChunkSize];
    }
}