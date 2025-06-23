using UnityEngine;
using UnityEngine.UI;

public class _Main : MonoBehaviour
{
    public _Player ThePlayer;
    public _FollowCam TheCam;
    public _WorldSpace TheWorldSpace;
    public Text TheShoutText;
    public GameObject PlayerModel;

    private Coord PrevPlayerCoord;

    private void Start()
    {
        GetComponent<_Cursor>().Mode = CursorMode.Locked;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // move player to correct spawn height to avoid falling at beginning
        ThePlayer.transform.position = new Vector3(0f, TheWorldSpace.ChildWorld.GetChunk(new Coord(0, 0)).Tiles[0, 0].h1*TheWorldSpace.TileHeight+ThePlayer.GetComponent<Collider>().bounds.size.y, 0f);
    }

    void Update()
    {
        Coord playerCoord = new Coord(Mathf.FloorToInt(ThePlayer.transform.position.x/TheWorldSpace.ChunkSize), Mathf.FloorToInt(ThePlayer.transform.position.z/TheWorldSpace.ChunkSize));

        // get/create the chunks around the player position
        if(playerCoord != PrevPlayerCoord)
        {
            PrevPlayerCoord = playerCoord;

            for(int y = -TheWorldSpace.ChunkSpawnRadius; y < TheWorldSpace.ChunkSpawnRadius; y++)
            {
                for(int x = -TheWorldSpace.ChunkSpawnRadius; x < TheWorldSpace.ChunkSpawnRadius; x++)
                {
                    TheWorldSpace.PutChunk(new Coord(playerCoord.x + x, playerCoord.y + y));
                }
            }
        }

        if(Input.GetKeyDown("l"))
        {
            GetComponent<_Cursor>().Mode ^= CursorMode.Locked;
        }

        if(Input.GetKeyDown("space"))
        {
            print("!!Test!!");
        }
    }

    public void ShoutText(string inText)
    {
        TheShoutText.text = inText;
    }
}