public class Tile
{
    public Chunk ParentChunk { get; private set; }
    public Coord ChunkCoord { get; private set; }

    public int h1, h2, h3, h4;

    public World ParentWorld { get { return ParentChunk.ParentWorld; } }
    public Coord WorldCoord { get { return new Coord(ParentChunk.WorldCoord.x*ParentWorld.ChunkSize+ChunkCoord.x, ParentChunk.WorldCoord.y*ParentWorld.ChunkSize+ChunkCoord.y); } }

    // a tile has 4 corners with independent heights
    // it has a location on the chunk, which has a loction in the world, which can be converted to a combined world coordinate

    public Tile(Chunk chunk, Coord chunkCoord, int[] heights)
    {
        this.ParentChunk = chunk;
        this.ChunkCoord = chunkCoord;

        if(heights.Length == 4)
        {
            this.h1 = heights[0];
            this.h2 = heights[1];
            this.h3 = heights[2];
            this.h4 = heights[3];
        } 
        else
        {
            this.h1 = 0;
            this.h2 = 0;
            this.h3 = 0;
            this.h4 = 0;
        }
    }
}
