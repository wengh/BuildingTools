using UnityEngine;

public class FlyCamera : MonoBehaviour
{

    /*
    EXTENDED FLYCAM
        Desi Quintans (CowfaceGames.com), 17 August 2012.
        Based on FlyThrough.js by Slin (http://wiki.unity3d.com/index.php/FlyThrough), 17 May 2011.
 
    LICENSE
        Free as in speech, and free as in beer.
 
    FEATURES
        WASD/Arrows:    Movement
                  Q:    Climb
                  E:    Drop
                      Shift:    Move faster
                    Control:    Move slower
                        End:    Toggle cursor locking to screen (you can also press Ctrl+P to toggle play mode on and off).
    */

    public float cameraSensitivity = 90;
    public float normalMoveSpeed = 15;
    public float slowMoveFactor = 0.25f;
    public float fastMoveFactor = 5;
    public float maxFoV = 60;
    public float zoomFactor = 0.4f;

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;
    private float zoom = 1;

    void Update()
    {
        rotationX += Input.GetAxis("Mouse X") * zoom * cameraSensitivity * Time.unscaledDeltaTime;
        rotationY += Input.GetAxis("Mouse Y") * zoom * cameraSensitivity * Time.unscaledDeltaTime;
        rotationY = Mathf.Clamp(rotationY, -90, 90);

        transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
        transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);

        int forward = (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0);
        int right = (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0);
        int up = (Input.GetKey(KeyCode.Space) ? 1 : 0) - (Input.GetKey(KeyCode.LeftAlt) ? 1 : 0);

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            transform.position += transform.forward * (normalMoveSpeed * fastMoveFactor) * forward * Time.unscaledDeltaTime;
            transform.position += transform.right * (normalMoveSpeed * fastMoveFactor) * right * Time.unscaledDeltaTime;
            transform.position += transform.up * (normalMoveSpeed * fastMoveFactor) * up * Time.unscaledDeltaTime;
        }
        else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            transform.position += transform.forward * (normalMoveSpeed * slowMoveFactor) * forward * Time.unscaledDeltaTime;
            transform.position += transform.right * (normalMoveSpeed * slowMoveFactor) * right * Time.unscaledDeltaTime;
            transform.position += transform.up * (normalMoveSpeed * slowMoveFactor) * up * Time.unscaledDeltaTime;
        }
        else
        {
            transform.position += transform.forward * normalMoveSpeed * forward * Time.unscaledDeltaTime;
            transform.position += transform.right * normalMoveSpeed * right * Time.unscaledDeltaTime;
            transform.position += transform.up * normalMoveSpeed * up * Time.unscaledDeltaTime;
        }

        if (Input.GetMouseButton(0)) zoom *= 1 - zoomFactor * Time.unscaledDeltaTime;
        if (Input.GetMouseButton(1)) zoom = Mathf.Min(1, zoom / (1 - zoomFactor * Time.unscaledDeltaTime));

        Camera.main.fieldOfView = maxFoV * zoom;
    }
}