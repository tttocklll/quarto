using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private int[,,] squares = new int [4, 10, 10];

    // camera
    private Camera camera_object;
    private RaycastHit hit;
    // Start is called before the first frame update
    void Start()
    {
        camera_object = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            // Ray ray = camera_object.ScreenPointToRay(Input.mousePosition);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
                int x = (Mathf.RoundToInt(hit.point.x) + 3) / 2;
                int z = (Mathf.RoundToInt(hit.point.z) + 3) / 2;
                Debug.Log(x);
                Debug.Log(z);
            }
        }
    }
}
