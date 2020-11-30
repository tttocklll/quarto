using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private int[,,] squares = new int [4, 10, 10];

    // camera
    private Camera camera_object;
    private RaycastHit hit;

    // prefabs
    public GameObject Piece_0000;
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
                Vector3 position = hit.collider.gameObject.transform.position;
                int x = Mathf.RoundToInt((position.x + 3) / 2);
                int z = Mathf.RoundToInt((position.z + 3) / 2);
                Debug.Log(x);
                Debug.Log(z);
                GameObject cur_piece = Instantiate(Piece_0000);
                
                position.y = 1.5F;
                cur_piece.transform.position = position;
            }
        }
    }
}
