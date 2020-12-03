﻿using System.Collections;
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

    // prefabs
    public GameObject QuartoText;

    // event system
    [SerializeField] EventSystem eventSystem;
    GameObject selectedObj;
    GameObject selectedPiece;
    private const int PIECE = 0;
    private const int PUT = 1;
    private int phase = PIECE;

    // turn
    private const bool COM = false;
    private const bool YOU = true;
    private bool turn = YOU;
    private bool isComWorking = false;

    // misc
    private string selectedName;
    private string pieceName;
    private List<string> alreadyPut = new List<string>(); // いらないかも
    private List<string> remainingPieces = new List<string>();
    private bool isEnd = false;
    private const int DEPTH = 3;

    // Start is called before the first frame update
    void Start()
    {
        camera_object = GameObject.Find("Main Camera").GetComponent<Camera>();
        InitializeArray();
    }

    // Update is called once per frame
    void Update()
    {
        if (isEnd) return;
        if (Input.GetMouseButtonDown(0) && turn == YOU) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
                selectedObj = hit.collider.gameObject;
                selectedName = selectedObj.name;
                if (phase == PIECE && selectedName.Substring(0, 5) == "Piece") { // 相手への駒選択
                    // 盤面上のものを触ってしまった場合なにもしない
                    if (alreadyPut.Contains(selectedName)) return;

                    selectedPiece = selectedObj;
                    pieceName = selectedName;
                    alreadyPut.Add(pieceName);
                    remainingPieces.Remove(pieceName);

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

                    displayBoard();

                    // 勝利判定
                    if (isQuarto())
                    {
                        GameObject quartoText = Instantiate(QuartoText);
                        isEnd = true;
                        return;
                    }
                }
            }
        } else if (turn == COM && !isComWorking) {
            isComWorking = true;
            int nextX, nextZ;
            string nextPiece;
            (_, nextX, nextZ, nextPiece) = negaMax(DEPTH, pieceName, remainingPieces);
            Debug.LogFormat("x = {0}, z = {1}, piece = {2}", nextX, nextZ, nextPiece);

            // phase == PUT
            squares[nextX][nextZ] = pieceName.Substring(6, 4);
            Vector3 position = selectedPiece.transform.position;
            position.x = nextX * 2 - 3;
            position.z = nextZ * 2 - 3;
            selectedPiece.transform.position = position;
            displayBoard();
            if (isQuarto())
            {
                GameObject quartoText = Instantiate(QuartoText);
                isEnd = true;
                return;
            }
            phase = PIECE;

            // phase == PIECE
            selectedPiece = GameObject.Find(nextPiece);
            pieceName = nextPiece;
            alreadyPut.Add(nextPiece);
            remainingPieces.Remove(nextPiece);

            position = selectedPiece.transform.position;
            position.x = -11;
            position.z = 0;
            selectedPiece.transform.position = position;
            phase = PUT;
            turn = !turn;
            isComWorking = false;
        }
    }

    // 初期化
    private void InitializeArray()
    {
        //for文を利用して配列にアクセスする
        for (int i = 0; i < 4;i++)
        {
            squares[i] = new string[4];
        }

        for (int i = 0; i < 16;i++)
        {
            remainingPieces.Add("Piece_" + System.Convert.ToString(i, 2).PadLeft(4, '0'));
        }
    }

    // nega max 法により最適手を求める
    private (int, int, int, string) negaMax(int limit, string nextPiece, List<string> curRemainingPieces) {
        if (limit <= 0) { // 深さ制限
            return (getHeuristicVal(), 0, 0, "");
        }

        // 合法手を生成
        List<(int, int)> nextMoves = getNextMoves(nextPiece);

        int maxVal = -1000000000;
        int curVal;
        (int x, int z) maxMove = (0, 0);
        string maxPiece = "";
        foreach ((int x, int z) move in nextMoves) {
            // 手を打つ
            squares[move.x][move.z] = nextPiece.Substring(6, 4);

            // 次の駒を選んで次へ
            foreach (string piece in curRemainingPieces.ToArray()) { // 削除・追加時のエラー回避
                curRemainingPieces.Remove(piece);
                (curVal, _, _, _) = negaMax(limit - 1, piece, curRemainingPieces);
                curVal *= -1;
                if (maxVal < curVal) {
                    maxVal = curVal;
                    maxMove = move;
                    maxPiece = piece;
                }
                curRemainingPieces.Add(piece);
            }

            // 手を戻す
            squares[move.x][move.z] = "";
        }
        return (maxVal, maxMove.x, maxMove.z, maxPiece);
    }

    private List<(int, int)> getNextMoves(string nextPiece) {
        List<(int, int)> nextMoves = new List<(int, int)>();
        for (int i=0; i<4; i++) {
            for (int j=0; j<4; j++) {
                if (!string.IsNullOrEmpty(squares[i][j])) continue;
                    nextMoves.Add((i, j));
            }
        }
        return nextMoves;
    }

    private int getHeuristicVal(){
        // squaresの状態から評価
        return Random.Range(0, 10000);
    }

    // 勝利判定
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

    private void displayBoard() {
        var temp = new List<string>();
        for (int i=0; i<4; i++) {
            temp.Add(string.Join(" | ", squares[i]));
        }
        Debug.Log(string.Join("\n", temp));
    }
}
