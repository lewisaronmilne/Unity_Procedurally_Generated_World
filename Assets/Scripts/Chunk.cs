public class Chunk
{
    public World ParentWorld { get; private set; }
    public Coord WorldCoord { get; private set; }
    public Tile[,] Tiles { get; private set; }

    public int Size { get { return ParentWorld.ChunkSize; } }

    // automatically populates itself with tiles on creation

    public Chunk(World world, Coord coord, int[][][] heightMap)
    {
        this.ParentWorld = world;
        this.WorldCoord = coord;
        Tiles = new Tile[Size, Size];

        for (int y = 0; y < Size; y++)
        {
            for(int x = 0; x < Size; x++)
            {
                Tiles[x, y] = new Tile(this, new Coord(x,y), heightMap[x][y]);
            }
        }
    }
}
