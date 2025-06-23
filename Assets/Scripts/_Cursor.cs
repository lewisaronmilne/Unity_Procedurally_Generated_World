using UnityEngine;
using System.Collections;
using System;

[Flags]
public enum CursorMode { Locked = 0x01, HeightEdit = 0x02 };

public class _Cursor : MonoBehaviour
{ 
    public float Sensitivity;
    public CursorMode Mode { get { return currentMode; }  set { PrevMode = Mode; currentMode = value; } }
    public CursorMode PrevMode { get; private set; }
    public UnityEngine.UI.Image Crosshairs, Cursor, HeightEditCursor;
    public Vector3 PositionToUse;

    private System.DateTime timeSinceSwitch;
    private CursorMode currentMode;

    void Update()
    {
        if((Mode & CursorMode.Locked) == CursorMode.Locked)
        {
            Crosshairs.enabled = true;
            Cursor.enabled = false;

            if(System.DateTime.Now > timeSinceSwitch.AddSeconds(5))
                Cursor.transform.position = Crosshairs.transform.position;

            PositionToUse = Crosshairs.transform.position;
        }
        else
        {
            Crosshairs.enabled = false;
            Cursor.enabled = true;

            timeSinceSwitch = System.DateTime.Now;

            Vector3 cursorPosition;

            if((Mode & CursorMode.HeightEdit) == CursorMode.HeightEdit)
                cursorPosition = Cursor.transform.position + new Vector3(0, Sensitivity*Input.GetAxis("Mouse Y"));
            else
                cursorPosition = Cursor.transform.position + new Vector3(Sensitivity*Input.GetAxis("Mouse X"), Sensitivity*Input.GetAxis("Mouse Y"));

            cursorPosition.x = cursorPosition.x < 0 ? 0 : cursorPosition.x;
            cursorPosition.x = cursorPosition.x > Screen.width ? Screen.width : cursorPosition.x;
            cursorPosition.y = cursorPosition.y < 0 ? 0 : cursorPosition.y;
            cursorPosition.y = cursorPosition.y > Screen.height ? Screen.height : cursorPosition.y;

            Cursor.transform.position = cursorPosition;

            PositionToUse = cursorPosition;
        }

        if ((Mode & CursorMode.HeightEdit) == CursorMode.HeightEdit)
        {
            Crosshairs.enabled = false;
            Cursor.enabled = false;
            HeightEditCursor.enabled = true;
            
            HeightEditCursor.transform.position = PositionToUse;
        }
        else
        {
            HeightEditCursor.enabled = false;
        }
    }
}
