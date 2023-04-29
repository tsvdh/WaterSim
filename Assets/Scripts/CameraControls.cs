using UnityEngine;

public class CameraControls : MonoBehaviour
{

    public float moveSpeed;
    public float lookSpeed;

    private Vector2 _prevMousePos;

    // Update is called once per frame
    private void Update()
    {
        float posChange = Time.deltaTime * moveSpeed;
        Transform newTransform = this.transform;
        
        if (Input.GetKey(KeyCode.W))
        {
            newTransform.position += newTransform.forward * posChange;
        }
        if (Input.GetKey(KeyCode.S))
        {
            newTransform.position -= newTransform.forward * posChange;
        }
        if (Input.GetKey(KeyCode.D))
        {
            newTransform.position += newTransform.right * posChange;
        }
        if (Input.GetKey(KeyCode.A))
        {
            newTransform.position -= newTransform.right * posChange;
        }
        
        if (Input.GetKey(KeyCode.Space))
        {
            newTransform.position += newTransform.up * posChange;
        }
        if (Input.GetKey(KeyCode.C))
        {
            newTransform.position -= newTransform.up * posChange;
        }

        Vector2 curMousePos = Input.mousePosition;
        
        if (Input.GetMouseButton(2))
        {
            Vector2 dirChange = curMousePos - _prevMousePos;
            dirChange *= Time.deltaTime * lookSpeed;
            
            Vector3 newAngles = newTransform.eulerAngles;
            newAngles.y += dirChange.x;
            newAngles.x += dirChange.y;
            newTransform.eulerAngles = newAngles;
        }

        _prevMousePos = curMousePos;
    }
}
