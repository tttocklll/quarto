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
    private bool turn = YOU; // 先手を設定
    private bool isComWorking = false;
    Text turnText;

    // misc
    private string selectedName;
    private string pieceName;
    private List<string> alreadyPut = new List<string>();
    private List<string> remainingPieces = new List<string>();
    private bool isEnd = false;
    private bool ALPHA_BETA = true; // true ? alpha-beta : minimax
    private const int DEPTH = 3; // 探索の深さ
    private const int MAX_VAL = 1000;
    private const int MIN_VAL = -1000;

    // stopwatch
    private System.Diagnostics.Stopwatch sw;
    private int sumCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        camera_object = GameObject.Find("Main Camera").GetComponent<Camera>();
        turnText = GameObject.Find("Text").GetComponent<Text>();
        turnText.text = turn == YOU ? "TURN: YOU" : "TURN: COM";
        turnText.color = turn == YOU ? Color.red : Color.white;
        sw = new System.Diagnostics.Stopwatch();
        InitializeArray();
    }

    // Update is called once per frame
    void Update()
    {
        if (isEnd) return;

        // 対ランダム用

        // if (turn == YOU)
        // {
        //     if (phase == PIECE)
        //     {
        //         pieceName = randPiece();
        //         selectedPiece = GameObject.Find(pieceName);
        //         Debug.LogFormat("YOU PIECE: {0}", pieceName);
        //         alreadyPut.Add(pieceName);
        //         remainingPieces.Remove(pieceName);

        //         Vector3 position = selectedPiece.transform.position;
        //         position.x = -11;
        //         position.z = 0;
        //         selectedPiece.transform.position = position;
        //         phase = PUT;
        //         turn = !turn;
        //         turnText.text = "TURN: COM";
        //         turnText.color = Color.white;
        //     }
        //     else if (phase == PUT)
        //     {
        //         var (nextX, nextZ) = randPut();

        //         Debug.LogFormat("YOU PUT: ({0}, {1})", nextX, nextZ);

        //         // phase == PUT
        //         squares[nextX][nextZ] = pieceName.Substring(6, 4);
        //         Vector3 position = selectedPiece.transform.position;
        //         position.x = nextX * 2 - 3;
        //         position.z = nextZ * 2 - 3;
        //         selectedPiece.transform.position = position;
        //         phase = PIECE;

        //         // displayBoard();

        //         // 勝利判定
        //         if (isQuarto())
        //         {
        //             GameObject quartoText = Instantiate(QuartoText);
        //             turnText.text = "YOU WIN!";
        //             isEnd = true;
        //             Debug.Log((float)sw.ElapsedMilliseconds / sumCount);
        //             return;
        //         } else if (remainingPieces.Count <= 0) {
        //             GameObject quartoText = Instantiate(QuartoText);
        //             quartoText.GetComponent<TextMesh>().text = "DRAW";
        //             quartoText.GetComponent<TextMesh>().color = Color.blue;
        //             turnText.text = "DRAW";
        //             turnText.color = Color.blue;
        //             isEnd = true;
        //             return;
        //         }
        //     }
        // }

        // 対人用

        if (Input.GetMouseButtonDown(0) && turn == YOU)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
                selectedObj = hit.collider.gameObject;
                selectedName = selectedObj.name;
                if (phase == PIECE && selectedName.Substring(0, 5) == "Piece") { // 相手への駒選択
                    // 盤面上のものを触ってしまった場合なにもしない
                    if (alreadyPut.Contains(selectedName)) return;

                    selectedPiece = selectedObj;
                    pieceName = selectedName;
                    // Debug.LogFormat("YOU PIECE: {0}", selectedName);
                    alreadyPut.Add(pieceName);
                    remainingPieces.Remove(pieceName);

                    Vector3 position = selectedObj.transform.position;
                    position.x = -11;
                    position.z = 0;
                    selectedObj.transform.position = position;
                    phase = PUT;
                    turn = !turn;
                    turnText.text = "TURN: COM";
                    turnText.color = Color.white;
                } else if (phase == PUT && selectedName.Substring(0, 5) == "Cylin") { // 駒を盤面に置く
                    Vector3 position = selectedObj.transform.position;
                    position.y = selectedPiece.transform.position.y;
                    int x = Mathf.RoundToInt((position.x + 3) / 2);
                    int z = Mathf.RoundToInt((position.z + 3) / 2);

                    // すでにおいてあったらだめ
                    if (!string.IsNullOrEmpty(squares[x][z])) return;

                    // Debug.LogFormat("YOU PUT: ({0}, {1})", x, z);
                    squares[x][z] = pieceName.Substring(6, 4);
                    selectedPiece.transform.position = position;
                    phase = PIECE;

                    // displayBoard();

                    // 勝利判定
                    if (isQuarto())
                    {
                        GameObject quartoText = Instantiate(QuartoText);
                        turnText.text = "YOU WIN!";
                        isEnd = true;
                        Debug.Log((float)sw.ElapsedMilliseconds / sumCount);
                        return;
                    } else if (remainingPieces.Count <= 0) {
                        GameObject quartoText = Instantiate(QuartoText);
                        quartoText.GetComponent<TextMesh>().text = "DRAW";
                        quartoText.GetComponent<TextMesh>().color = Color.blue;
                        turnText.text = "DRAW";
                        turnText.color = Color.blue;
                        isEnd = true;
                        return;
                    }
                }
            }
        }
        // main of AI
        else if (turn == COM && !isComWorking)
        {
            Vector3 position;
            if (phase == PIECE) {
                isComWorking = true;
                selectedPiece = GameObject.Find("Piece_0000");
                pieceName = "Piece_0000";
                alreadyPut.Add("Piece_0000");
                remainingPieces.Remove("Piece_0000");

                position = selectedPiece.transform.position;
                position.x = -11;
                position.z = 0;
                selectedPiece.transform.position = position;
                phase = PUT;
                turn = !turn;
                turnText.text = "TURN: YOU";
                turnText.color = Color.red;
                isComWorking = false;
                return;
            }
            sw.Start();
            sumCount++;
            isComWorking = true;
            int nextX, nextZ, eval;
            string nextPiece;
            (eval, nextX, nextZ, nextPiece) = negaMax(0, pieceName, remainingPieces, MIN_VAL, MAX_VAL);
            if (eval == -1000) {
                (nextX, nextZ) = randPut();
                nextPiece = randPiece();
            }
            Debug.LogFormat("COM PUT: ({0}, {1})", nextX, nextZ);
            Debug.LogFormat("COM PIECE: {0}", nextPiece);
            Debug.LogFormat("COM EVAL: {0}", eval);

            // phase == PUT
            squares[nextX][nextZ] = pieceName.Substring(6, 4);
            position = selectedPiece.transform.position;
            position.x = nextX * 2 - 3;
            position.z = nextZ * 2 - 3;
            selectedPiece.transform.position = position;
            // displayBoard();
            if (isQuarto())
            {
                sw.Stop();
                GameObject quartoText = Instantiate(QuartoText);
                turnText.text = "COM WIN!";
                turnText.color = Color.green;
                isEnd = true;
                Debug.Log((float)sw.ElapsedMilliseconds / sumCount);
                return;
            }else if (remainingPieces.Count <= 0) {
                GameObject quartoText = Instantiate(QuartoText);
                quartoText.GetComponent<TextMesh>().text = "DRAW";
                quartoText.GetComponent<TextMesh>().color = Color.blue;
                turnText.text = "DRAW";
                turnText.color = Color.blue;
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
            turnText.text = "TURN: YOU";
            turnText.color = Color.red;
            isComWorking = false;
            sw.Stop();
            Debug.Log(sw.ElapsedMilliseconds + "ms");
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
    private (int, int, int, string) negaMax(int depth, string nextPiece, List<string> curRemainingPieces, int alpha, int beta) {
        List<(int, int)> nextMoves;
        int maxVal, curVal;
        if (depth == DEPTH) { // 深さ制限
            // 合法手を生成
            nextMoves = getNextMoves(nextPiece);
            maxVal = MIN_VAL;
            foreach ((int x, int z) move in nextMoves) {
                // 手を打つ
                squares[move.x][move.z] = nextPiece.Substring(6, 4);
                curVal = getHeuristicVal();
                // 手を戻す
                squares[move.x][move.z] = "";

                if (maxVal < curVal) maxVal = curVal;
                if (maxVal >= beta) break;
            }
            return (maxVal, 0, 0, "");
        }

        // 合法手を生成
        nextMoves = getNextMoves(nextPiece);

        maxVal = MIN_VAL;
        (int x, int z) maxMove = (0, 0);
        string maxPiece = "";
        foreach ((int x, int z) move in nextMoves) {
            // 手を打つ
            squares[move.x][move.z] = nextPiece.Substring(6, 4);
            if (isQuarto()) {
                squares[move.x][move.z] = "";
                return (MAX_VAL, move.x, move.z, nextPiece);
            }

            // 次の駒を選んで次へ
            foreach (string piece in curRemainingPieces.ToArray()) { // 削除・追加時のエラー回避
                curRemainingPieces.Remove(piece);
                (curVal, _, _, _) = negaMax(depth + 1, piece, curRemainingPieces, -beta, -System.Math.Max(alpha, maxVal));
                curVal *= -1;
                curRemainingPieces.Add(piece);
                if (maxVal < curVal) {
                    maxVal = curVal;
                    maxMove = move;
                    maxPiece = piece;
                    // Debug.LogFormat("update: maxPiece={0}, move=({2}, {3}), eval={1}", maxPiece, maxVal, move.x, move.z);
                }
                if (maxVal >= beta && ALPHA_BETA) break;
            }

            // 手を戻す
            squares[move.x][move.z] = "";
            if (maxVal >= beta && ALPHA_BETA) break;
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
        // state[i] := 1ラインに同種の駒がi個あり、かつ異種がない
        // state[3]があるなら、あと一つ空いているところに同種を置けばQuarto!
        int[] state = {0, 0, 0, 0, 0};
        int[] result = {0, 0, 0, 0};
        string first = "";
        // squaresの状態から評価
        // vertical
        for (int i=0; i<4; i++) {
            result = FillArray(result, 0);
            first = "";
            for (int j=0; j<4; j++) {
                if (string.IsNullOrEmpty(squares[j][i])) continue;
                if (string.IsNullOrEmpty(first)){
                    first = squares[j][i];
                    result = FillArray(result, 1);
                    continue;
                }
                for (int idx = 0; idx < 4; idx++) {
                    if (squares[j][i][idx] == first[idx]) result[idx]++;
                    else result[idx] = -100;
                }
            }
            for (int idx=0; idx<4; idx++) {
                if (result[idx] >= 0) {
                    state[result[idx]]++;
                }
            }
        }
        // horizontal
        for (int i=0; i<4; i++) {
            result = FillArray(result, 0);
            first = "";
            for (int j=0; j<4; j++) {
                if (string.IsNullOrEmpty(squares[i][j])) continue;
                if (string.IsNullOrEmpty(first)){
                    first = squares[i][j];
                    result = FillArray(result, 1);
                    continue;
                }
                for (int idx = 0; idx < 4; idx++) {
                    if (squares[i][j][idx] == first[idx]) result[idx]++;
                    else result[idx] = -100;
                }
            }
            for (int idx=0; idx<4; idx++) {
                if (result[idx] >= 0) {
                    state[result[idx]]++;
                }
            }
        }
        // left diagnosis
        result = FillArray(result, 0);
        first = "";
        for (int i=0; i<4; i++) {
            if (string.IsNullOrEmpty(squares[i][i])) continue;
            if (string.IsNullOrEmpty(first)) {
                first = squares[i][i];
                result = FillArray(result, 1);
                continue;
            }
            for (int idx = 0; idx < 4; idx++) {
                if (squares[i][i][idx] == first[idx]) result[idx]++;
                else result[idx] = -100;
            }
        }
        for (int idx=0; idx<4; idx++) {
            if (result[idx] >= 0) {
                state[result[idx]]++;
            }
        }
        // right diagnosis
        result = FillArray(result, 0);
        first = "";
        for (int i=0; i<4; i++) {
            if (string.IsNullOrEmpty(squares[i][3-i])) continue;
            if (string.IsNullOrEmpty(first)) {
                first = squares[i][3-i];
                result = FillArray(result, 1);
                continue;
            }
            for (int idx = 0; idx < 4; idx++) {
                if (squares[i][3-i][idx] == first[idx]) result[idx]++;
                else result[idx] = -100;
            }
        }
        for (int idx=0; idx<4; idx++) {
            if (result[idx] >= 0) {
                state[result[idx]]++;
            }
        }
        // return (state[4] > 0) ? MAX_VAL : state[3];
        // return (state[4] > 0) ? MAX_VAL : state[3] * 2 - state[2];
        return (state[4] > 0) ? MAX_VAL : state[3] * 10 + state[2] * 3;
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

    // misc
    private void displayBoard()
    {
        var temp = new List<string>();
        for (int i=0; i<4; i++) {
            temp.Add(string.Join(" | ", squares[i]));
        }
        Debug.Log(string.Join("\n", temp));
    }


    private int[] FillArray(int[] array, int fillItem) {
        for (int i=0; i<array.Length; i++) {
            array[i] = fillItem;
        }
        return array;
    }

    // random
    private string randPiece() {
        if (remainingPieces.Count <= 0) return "";
        int curRnd;
        string pieceName;
        while (true) {
            curRnd = Random.Range(0, 16);
            pieceName = "Piece_" + System.Convert.ToString(curRnd, 2).PadLeft(4, '0');
            if (!alreadyPut.Contains(pieceName)) {
                alreadyPut.Add(pieceName);
                return pieceName;
            }
        }
    }

    private (int, int) randPut() {
        int x, z;
        while (true) {
            x = Random.Range(0, 4);
            z = Random.Range(0, 4);
            if (string.IsNullOrEmpty(squares[x][z])) {
                return (x, z);
            }
        }
    }
}
