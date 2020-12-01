using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

                    // すでにおいてあったらだめ
                    if (!string.IsNullOrEmpty(squares[x][z])) return;

                    squares[x][z] = pieceName.Substring(6, 4);
                    selectedPiece.transform.position = position;
                    phase = PIECE;

                    // 勝利判定
                    if (isQuarto())
                    {
                        Debug.Log("Quarto!");
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

    private bool isQuarto()
    {
        return isVerticalQuarto() || isHorizontalQuarto() || isLeftCrossQuarto() || isRightCrossQuarto();
    }

    private bool isVerticalQuarto()
    {
        for (int i=0; i<4; i++) {
            bool[] result = {true, true, true, true};
            string first = squares[0][i];
            if (string.IsNullOrEmpty(first)) continue;
            bool flag = true;
            for (int j=1; j<4; j++) {
                if (string.IsNullOrEmpty(squares[j][i])){ // 駒なし
                    flag = false;
                    break;
                }
                for (int idx=0; idx<4; idx++) {
                    if (squares[j][i][idx] != first[idx]) {
                        result[idx] = false;
                    }
                }
            }
            if (flag && result.Contains(true)) return true;
        }
        return false;
    }

    private bool isHorizontalQuarto()
    {
        for (int i=0; i<4; i++) {
            bool[] result = {true, true, true, true};
            string first = squares[i][0];
            if (string.IsNullOrEmpty(first)) continue;
            bool flag = true;
            for (int j=1; j<4; j++) {
                if (string.IsNullOrEmpty(squares[i][j])){ // 駒なし
                    flag = false;
                    break;
                }
                for (int idx=0; idx<4; idx++) {
                    if (squares[i][j][idx] != first[idx]) {
                        result[idx] = false;
                    }
                }
            }
            if (flag && result.Contains(true)) return true;
        }
        return false;
    }

    private bool isLeftCrossQuarto()
    {
        bool[] result = {true, true, true, true};
        string first = squares[0][0];
        if (string.IsNullOrEmpty(first)) return false;
        bool flag = true;
        for (int i=1; i<4; i++) {
            if (string.IsNullOrEmpty(squares[i][i])){ // 駒なし
                flag = false;
                break;
            }
            for (int idx=0; idx<4; idx++) {
                if (squares[i][i][idx] != first[idx]) {
                    result[idx] = false;
                }
            }
        }
        if (flag && result.Contains(true)) return true;
        return false;
    }

    private bool isRightCrossQuarto()
    {
        bool[] result = {true, true, true, true};
        string first = squares[0][3];
        if (string.IsNullOrEmpty(first)) return false;
        bool flag = true;
        for (int i=1; i<4; i++) {
            if (string.IsNullOrEmpty(squares[i][3-i])){ // 駒なし
                flag = false;
                break;
            }
            for (int idx=0; idx<4; idx++) {
                if (squares[i][3-i][idx] != first[idx]) {
                    result[idx] = false;
                }
            }
        }
        if (flag && result.Contains(true)) return true;
        return false;
    }
}
