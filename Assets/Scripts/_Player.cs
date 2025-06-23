using UnityEngine;

public class _Player : MonoBehaviour
{
    public float Speed;

    void FixedUpdate()
    {
        if((GetComponentInParent<_Cursor>().Mode & CursorMode.Locked) == CursorMode.Locked)
        {
            float inputH = Input.GetAxis("Horizontal");
            float inputV = Input.GetAxis("Vertical");

            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;

            if((inputH == 0) && (inputV == 0))
            {
                GetComponent<Rigidbody>().linearVelocity = GetComponent<Rigidbody>().linearVelocity.magnitude < 0.1f ? Vector3.zero : GetComponent<Rigidbody>().linearVelocity * 0.95f;
                GetComponent<Rigidbody>().angularVelocity = GetComponent<Rigidbody>().angularVelocity.magnitude < 0.1f ? Vector3.zero : GetComponent<Rigidbody>().angularVelocity * 0.95f;
            }
            else
            {
                // direct forward movement away from camera
                float playerAngle = GetComponentInParent<_Main>().PlayerModel.transform.rotation.eulerAngles.y;

                float axisY = -Mathf.Cos(playerAngle*Mathf.PI/180);
                float axisX = -Mathf.Sin(playerAngle*Mathf.PI/180);

                Vector3 movement = new Vector3(axisY*inputH + axisX*inputV, 0.0f, -axisX*inputH + axisY*inputV);

                GetComponent<Rigidbody>().AddForce(movement * Speed);
            }  
        }
        else
        {
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
    }
}
