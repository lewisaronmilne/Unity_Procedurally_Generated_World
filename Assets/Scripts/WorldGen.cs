using System;
using UnityEngine;

// the world/terrain generation logic

public class WorldGen
{
    private Noise TheNoise;

    public WorldGen()
    {
        TheNoise = new Noise();
        TheNoise.addChannel(new Channel("height", Algorithm.Perlin3d, 20.0f, NoiseStyle.Linear, 0.0f, 20.0f, Edge.Smooth).setFractal(2, 10, 0.05f));
    }

    public int[] HeightAt(Coord coord)
    {
        int[] heights = new int[]
        {
            Mathf.RoundToInt(TheNoise.getNoise(new Vector3(coord.x, 0, coord.y), "height")),
            Mathf.RoundToInt(TheNoise.getNoise(new Vector3(coord.x+1, 0, coord.y), "height")),
            Mathf.RoundToInt(TheNoise.getNoise(new Vector3(coord.x+1, 0, coord.y+1), "height")),
            Mathf.RoundToInt(TheNoise.getNoise(new Vector3(coord.x, 0, coord.y+1), "height"))
        };

        int[] perm = Misc.GetSortArray(heights);

        foreach(int i in perm)
        {
            heights[Misc.Mod(i+1, 4)] = heights[i] + Math.Sign(heights[Misc.Mod(i+1, 4)] - heights[i]);
            heights[Misc.Mod(i-1, 4)] = heights[i] + Math.Sign(heights[Misc.Mod(i-1, 4)] - heights[i]);
        }

        return heights;
    }
}
