using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    private string[][] squares = new string [4][];

    // camera
    private Camera camera_object;
    private RaycastHit hit;

    // event system
    [SerializeField] EventSystem eventSystem;
    GameObject selectedObj;
    GameObject selectedPiece;
    private const int PIECE = 0;
    private const int PUT = 1;
    int phase = PIECE;
    string selectedName;
    string pieceName;

    // turn
    private const bool CPU = false;
    private const bool YOU = true;
    bool turn = YOU;

    // Start is called before the first frame update
    void Start()
    {
        camera_object = GameObject.Find("Main Camera").GetComponent<Camera>();
        InitializeArray();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
                selectedObj = hit.collider.gameObject;
                selectedName = selectedObj.name;
                if (phase == PIECE && selectedName.Substring(0, 5) == "Piece") { // 相手への駒選択
                    selectedPiece = selectedObj;
                    pieceName = selectedName;
                    Vector3 position = selectedObj.transform.position;
                    position.x = -11;
                    position.z = 0;
                    selectedObj.transform.position = position;
                    phase = PUT;
                    turn = !turn;
                } else if (phase == PUT && selectedName.Substring(0, 5) == "Cylin") { // 駒を盤面に置く
                    Vector3 position = selectedObj.transform.position;
                    position.y = selectedPiece.transform.position.y;
                    int x = Mathf.RoundToInt((position.x + 3) / 2);
                    int z = Mathf.RoundToInt((position.z + 3) / 2);
                    squares[x][z] = pieceName.Substring(6, 4);
                    selectedPiece.transform.position = position;
                    phase = PIECE;
                    for (int i=0; i<4; i++) {
                        Debug.Log(string.Join(",",squares[i]));
                    }
                }
            }
        }
    }

    private void InitializeArray()
    {
        //for文を利用して配列にアクセスする
        for (int i = 0; i < 4;i++)
        {
            squares[i] = new string[4];
        }
    }
}
