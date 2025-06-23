using UnityEngine;

public class _FollowCam: MonoBehaviour
{
    public float CamSpeed, ScrollSpeed, PivotOffsetUp, PivotOffsetFw, PivotOverShoot;
    public float[] ScrollSteps;
    public int CurrentScrollStep;
    public Vector3 FocusPoint;
    public bool FocusY;

    private float h = -Mathf.PI/2, v = 1.0f, Radius;

    void LateUpdate()
    {
        if((GetComponentInParent<_Cursor>().Mode & CursorMode.Locked) == CursorMode.Locked)
        {
            _Player player = transform.parent.GetComponentInParent<_Main>().ThePlayer;
            GameObject playerModel = GetComponentInParent<_Main>().PlayerModel;

            if (FocusY)
                h = (player.transform.position.x > FocusPoint.x ? 0 : Mathf.PI) + Mathf.Atan((player.transform.position.z - FocusPoint.z)/(player.transform.position.x - FocusPoint.x));
            else
                h -= CamSpeed * Input.GetAxis("Mouse X");

            v += CamSpeed * Input.GetAxis("Mouse Y");

            v = v <= -PivotOverShoot ? -PivotOverShoot : v;
            v = v >= Mathf.PI - 0.001f ? Mathf.PI - 0.001f : v;

            if(Input.GetAxis("Mouse ScrollWheel") != 0f)
                CurrentScrollStep = Input.GetAxis("Mouse ScrollWheel") < 0 ? (CurrentScrollStep == ScrollSteps.Length-1 ? CurrentScrollStep : CurrentScrollStep+1) : (CurrentScrollStep == 0 ? 0 : CurrentScrollStep-1);

            Radius = ScrollSteps[CurrentScrollStep];

            Vector3 CameraPivot = new Vector3(-PivotOffsetFw * Mathf.Cos(h) * Mathf.Cos(v), PivotOffsetUp, -PivotOffsetFw*Mathf.Sin(h) * Mathf.Cos(v));
            Vector3 CameraOffset = new Vector3(Mathf.Sin(v) * Mathf.Cos(h), Mathf.Cos(v), Mathf.Sin(v) * Mathf.Sin(h));

            RaycastHit hit;
            Physics.Raycast(new Ray(player.transform.position + CameraPivot, CameraOffset), out hit, Radius, 1 << 8);

            transform.position = player.transform.position + CameraPivot + (hit.distance == 0 ? Radius : hit.distance) * CameraOffset;
            transform.rotation = Quaternion.LookRotation(player.transform.position + CameraPivot - transform.position, v > 0 ? Vector3.up : Vector3.down);

            playerModel.transform.position = player.transform.position + new Vector3(0, 1, 0);
            playerModel.transform.rotation = Quaternion.LookRotation((v > 0 ? 1 : -1) * new Vector3(transform.position.x - playerModel.transform.position.x + PivotOffsetFw*Mathf.Cos(h), 0, transform.position.z - playerModel.transform.position.z + PivotOffsetFw*Mathf.Sin(h)), Vector3.up);
        }
    }
}
