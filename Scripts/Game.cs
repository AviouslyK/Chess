using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;		
using UnityEngine.UI;
using System.Linq;
using System;
using Random = UnityEngine.Random;

// Todo:
// 1 - change moveplates for white to only be legal moves when in check
// 2 - implement checkmate 
// 3 - Castling 
// 4 - implement draw by repetition
// 5 - pawn promotion
// 6 - undo-ing a move

public class Game : MonoBehaviour
{
    public GameObject chesspiece;

    public struct Move
    {
        public int x; // board x coordinate of move
        public int y; // board y coordinate of move
        public int score; // how good is the move?
        public bool attack; // is there an enemy piece at this position
    }

    private int capturedBlack = 0;
    private int capturedWhite = 0;
    private int turn = 0;
    private bool whiteInCheck = false;
    private bool blackInCheck = false;

    // these are reset to true each turn, to see if based on the 
    // board state castling is possible (pieces in the way basically)
    public bool whiteCastleLeftOK = true;
    public bool blackCastleLeftOK = true;
    public bool whiteCastleRightOK = true;
    public bool blackCastleRightOK = true;

    // These are more permanently set
    // if the rook has moved, only one side is set to true to prevent castling.
    // But if the king moves, or you castle, both are impossible from here on out
    public bool whiteHasCastledL = false;
    public bool blackHasCastledL = false;
    public bool whiteHasCastledR = false;
    public bool blackHasCastledR = false;

    // value of pieces
    public Dictionary<string,int> pieceValue = new Dictionary<string,int>();

    // Positions and team for each chesspiece
    private GameObject[,] positions = new GameObject[9,9]; // holds piece's at each board position
                                                           // 9x9 instead of 8x8 for extra row to 
                                                           // move pieces out of way when calculating 
                                                           // possible moves. 8x8 are physical locations 
    private GameObject[] playerBlack = new GameObject[16]; // holds black's pieces
    private GameObject[] playerWhite = new GameObject[16]; // holds white's pieces

    private string currentPlayer = "white";

    private bool gameOver = false;

    // Start is called before the first frame update
    void Start()
    {   
        pieceValue.Add("white_pawn",1); pieceValue.Add("black_pawn",1);
        pieceValue.Add("white_knight",3); pieceValue.Add("black_knight",3);
        pieceValue.Add("white_bishop",3); pieceValue.Add("black_bishop",3);
        pieceValue.Add("white_rook",5); pieceValue.Add("black_rook",5);
        pieceValue.Add("white_queen",10); pieceValue.Add("black_queen",10);
        pieceValue.Add("white_king",100); pieceValue.Add("black_king",100);
        

        playerWhite = new GameObject[] {
            Create("white_rook",0,0), Create("white_knight",1,0), Create("white_bishop",2,0), Create("white_queen",3,0),
            Create("white_king",4,0), Create("white_bishop",5,0), Create("white_knight",6,0), Create("white_rook",7,0),
            Create("white_pawn",0,1), Create("white_pawn",1,1),   Create("white_pawn",2,1),   Create("white_pawn",3,1),
            Create("white_pawn",4,1), Create("white_pawn",5,1),   Create("white_pawn",6,1),   Create("white_pawn",7,1),
        };
        playerBlack = new GameObject[] {
            Create("black_rook",0,7), Create("black_knight",1,7), Create("black_bishop",2,7), Create("black_queen",3,7),
            Create("black_king",4,7), Create("black_bishop",5,7), Create("black_knight",6,7), Create("black_rook",7,7), 
            Create("black_pawn",0,6), Create("black_pawn",1,6),   Create("black_pawn",2,6),   Create("black_pawn",3,6),
            Create("black_pawn",4,6), Create("black_pawn",5,6),   Create("black_pawn",6,6),   Create("black_pawn",7,6),
        };

        // Set all pieces positions on the position board
        for (int i = 0; i < playerWhite.Length; i++)
        {
            SetPosition(playerWhite[i]);
            SetPosition(playerBlack[i]);
        }
    }

    public GameObject Create(string name, int x, int y)
    {
        GameObject obj = Instantiate(chesspiece, new Vector3(0,0,-1), Quaternion.identity);
        Chessman cm = obj.GetComponent<Chessman>();
        cm.name = name;
        cm.SetXBoard(x);
        cm.SetYBoard(y);
        cm.Activate(); // put on correct sprite
        return obj;
    }


    public void SetPosition(GameObject obj)
    {
        Chessman cm = obj.GetComponent<Chessman>();

        positions[cm.GetXBoard(), cm.GetYBoard()] = obj;
    }

    public void SetPositionEmpty(int x, int y) { positions[x,y] = null; }
    public GameObject GetPosition(int x, int y) { return positions[x,y]; }
    public int GetCaptured(string s)
    {
        if (s == "white") return capturedWhite;
        else return capturedBlack;
    }

    public void CaptureTally(string team)
    {
        if (team == "white") capturedWhite ++;
        else capturedBlack ++;
    }


    public bool PositionOnBoard(int x, int y)
    {
        if (x < 0 || y < 0 || x >= 8 || y >= 8) // chessboard is 8x8
            return false;
        else 
            return true;
    }

    // getters and setters 
    public string GetCurrentPlayer(){ return currentPlayer; }
    public bool IsGameOver(){ return gameOver; }
    public int GetTurn(){ return turn/2; } // how many turns have been played (each player going counts as 1)
    public int GetValue(string p) { return pieceValue[p]; }
    public bool IsWhiteInCheck() { return whiteInCheck; }
    public bool IsBlackInCheck() { return blackInCheck; }

    public void SetWhiteCheck(bool check) { whiteInCheck = check; }
    public void SetBlackCheck(bool check) { blackInCheck = check; }

    // helpers
    public GameObject FindPiece(string piece_name)
    {   
        if (!pieceValue.ContainsKey(piece_name))
            Debug.Log("YOU'RE TRYING TO FIND A PIECE THAT DOESNT EXIST!");

        for (int i = 0; i < playerBlack.Length; i++)
            if (playerBlack[i].name == piece_name)
                return playerBlack[i];
        
        for (int i = 0; i < playerWhite.Length; i++)
            if (playerWhite[i].name == piece_name)
                return playerWhite[i];
        
        return playerWhite[0]; // should never get here
    }

    public void NextTurn()
    {   
        turn ++;
        if (currentPlayer == "white") currentPlayer = "black";
        else currentPlayer = "white";
    }

    public void Update()
    {
        if (gameOver == true && Input.GetMouseButtonDown(0))
        {
            GameObject.FindGameObjectWithTag("CheckText").GetComponent<Text>().enabled = false;
            gameOver = false;
            SceneManager.LoadScene("Game");
        }

        whiteCastleLeftOK  = true;
        blackCastleLeftOK  = true;
        whiteCastleRightOK = true;
        blackCastleRightOK = true;

        // update castling 
        if (this.whiteHasCastledL) whiteCastleLeftOK  = false;
        if (this.whiteHasCastledR) whiteCastleRightOK  = false;

        if (this.blackHasCastledL) blackCastleLeftOK  = false;
        if (this.blackHasCastledR) blackCastleRightOK  = false;

        if (whiteCastleLeftOK)
        {
            if (this.GetPosition(1,0) != null || this.GetPosition(2,0) != null || this.GetPosition(3,0) != null) whiteCastleLeftOK = false;
        }
        if (whiteCastleRightOK)
        {
            if (this.GetPosition(5,0) != null || this.GetPosition(6,0) != null) whiteCastleRightOK = false;
        }

        if (blackCastleLeftOK)
        {
            if (this.GetPosition(1,7) != null || this.GetPosition(2,7) != null || this.GetPosition(3,7) != null) blackCastleLeftOK = false;
        }
        if (blackCastleRightOK)
        {
            if (this.GetPosition(5,7) != null || this.GetPosition(6,7) != null) blackCastleRightOK = false;
        }

        // check if anyone is in check
        Check();
        if (IsBlackInCheck() || IsWhiteInCheck())
            GameObject.FindGameObjectWithTag("CheckText").GetComponent<Text>().enabled = true;
        else
            GameObject.FindGameObjectWithTag("CheckText").GetComponent<Text>().enabled = false;

        if (currentPlayer == "black") // cpu's turn
        {   
            Debug.Log("whiteCastleLeftOK = " + this.whiteCastleLeftOK + " , whiteCastleRightOK = " + whiteCastleRightOK);
            Debug.Log("blackCastleLeftOK = " + this.blackCastleLeftOK + " , blackCastleRightOK = " + blackCastleRightOK);
            Invoke("makeMove", 2); // wait 3 seconds then move
            if (gameOver == true && Input.GetMouseButtonDown(0))
            {
                GameObject.FindGameObjectWithTag("CheckText").GetComponent<Text>().enabled = false;
                gameOver = false;
                SceneManager.LoadScene("Game");
            }
            NextTurn();
        }

        // Check if White is in CheckMate!
        if (currentPlayer == "white" && IsWhiteInCheck())
        {
            if (checkWhiteCheckmate()) this.Winner("black");
        }
    }

    // count White's legal moves, to see if they're in checkmate
    public bool checkWhiteCheckmate()
    {

        int[] scores = new int[playerWhite.Length]; // to quickly find best score and it's index(piece)
        for (int i = 0; i < playerWhite.Length; i++) // piece loop
        {
            // default score to -1, in case of no legal moves
            int bestScore = -1;
            scores[i] = -1; 

            //Debug.Log("Considering " + playerBlack[i].GetComponent<Chessman>().name + "'s Legal Moves...");
            int x = playerWhite[i].GetComponent<Chessman>().GetXBoard();
            int y = playerWhite[i].GetComponent<Chessman>().GetYBoard();
            if (this.PositionOnBoard(x,y) == false) continue;

            Move[] legals = GetLegalMoves(playerWhite[i].GetComponent<Chessman>().name, x, y);
        
            for (int j = 0; j < legals.Length; j++) { // Find Piece i's best move
                if (legals[j].score > bestScore) {
                    scores[i] = legals[j].score;
                    bestScore  = legals[j].score;
                }          
            }
            
        }
        
        // If No legal moves, then white is in checkmate
        if (scores.Max() == -1 && IsBlackInCheck()) return true;
        return false;
    }

    // CPU make's a move
    public void makeMove()
    {
        int bestMoveX;
        int bestMoveY;

        Move[] moves = new Move[playerBlack.Length];
        int[] scores = new int[playerBlack.Length]; // to quickly find best score and it's index(piece)
        for (int i = 0; i < playerBlack.Length; i++) // piece loop
        {
            // default score to -1, in case of no legal moves
            int bestScore = -1;
            scores[i] = -1; 

            //Debug.Log("Considering " + playerBlack[i].GetComponent<Chessman>().name + "'s Legal Moves...");
            int x = playerBlack[i].GetComponent<Chessman>().GetXBoard();
            int y = playerBlack[i].GetComponent<Chessman>().GetYBoard();
            if (this.PositionOnBoard(x,y) == false) continue;

            Move[] legals = GetLegalMoves(playerBlack[i].GetComponent<Chessman>().name, x, y);
        
            for (int j = 0; j < legals.Length; j++) { // Find Piece i's best move
                if (legals[j].score > bestScore) {
                    moves[i]  = legals[j];
                    scores[i] = legals[j].score;
                    bestScore  = legals[j].score;
                }          
            }
            
        }
        
        // Checkmate case 
        if (scores.Max() == -1 && IsBlackInCheck()) this.Winner("white");
        else { 

            int bestPiece = scores.ToList().IndexOf(scores.Max()); // get best move from all pieces
            bestMoveX = moves[bestPiece].x;
            bestMoveY = moves[bestPiece].y;

            Debug.Log(playerBlack[bestPiece].GetComponent<Chessman>().name + " to " + bestMoveX + ", " + bestMoveY);

            // Put piece in new space 
            bool attacking = false;
            if (this.GetPosition(bestMoveX,bestMoveY) != null && this.GetPosition(bestMoveX,bestMoveY).GetComponent<Chessman>().GetPlayer() == "white") 
                attacking = true;
            if (attacking)
            {
                // get piece at bestMoves's position
                GameObject cp = this.GetPosition(bestMoveX, bestMoveY);

                // Remove piece from game (shrink and put to side)
                string col = cp.GetComponent<Chessman>().GetPlayer();
                if (col == "white")
                {
                    cp.GetComponent<Chessman>().SetXBoard(8);
                    cp.GetComponent<Chessman>().SetYBoard(7 - this.GetCaptured(col));
                }
                else 
                {
                    cp.GetComponent<Chessman>().SetXBoard(8);
                    cp.GetComponent<Chessman>().SetYBoard(0 + this.GetCaptured(col));
                }
                    cp.GetComponent<Chessman>().SetCoords();
                    cp.GetComponent<Chessman>().GetComponent<Transform>().localScale = new Vector3(1.5f,1.5f,1.0f);
                    this.CaptureTally(col);
            }

            // If you moved the king, can no longer castle
            if (playerBlack[bestPiece].GetComponent<Chessman>().name.Contains("king")) 
            {
                blackHasCastledL = true;
                blackHasCastledR = true;
            }

            // If you moved the left rook, you can no longer castle left
            if (playerBlack[bestPiece].GetComponent<Chessman>().name.Contains("rook") && playerBlack[bestPiece].GetComponent<Chessman>().GetXBoard() == 0) 
            {
                blackHasCastledL = true;
            }

            // If you moved the right rook, you can no longer castle right
            if (playerBlack[bestPiece].GetComponent<Chessman>().name.Contains("rook") && playerBlack[bestPiece].GetComponent<Chessman>().GetXBoard() == 7) 
            {
                blackHasCastledR = true;
            }


            // if castling right, also move the rook
            if (playerBlack[bestPiece].GetComponent<Chessman>().name.Contains("King") && this.blackCastleRightOK && bestMoveX == 6 && bestMoveY == 7)
            {
                GameObject cp = this.GetPosition(bestMoveX+1, bestMoveY);
                cp.GetComponent<Chessman>().SetXBoard(bestMoveX-1);
                cp.GetComponent<Chessman>().SetCoords();
                this.blackHasCastledL = true;
                this.blackHasCastledR = true; 
            }
            // Castling Left, also move rook
            if (playerBlack[bestPiece].GetComponent<Chessman>().name.Contains("King") && this.blackCastleLeftOK && bestMoveX == 2 && bestMoveY == 7)
            {
                GameObject cp = this.GetPosition(bestMoveX-2, bestMoveY);
                cp.GetComponent<Chessman>().SetXBoard(bestMoveX+1);
                cp.GetComponent<Chessman>().SetCoords();
                this.blackHasCastledL = true;
                this.blackHasCastledR = true;
            }

            // set original location to empty
            SetPositionEmpty(playerBlack[bestPiece].GetComponent<Chessman>().GetXBoard(),playerBlack[bestPiece].GetComponent<Chessman>().GetYBoard());

            playerBlack[bestPiece].GetComponent<Chessman>().SetXBoard(bestMoveX); // updates piece's properties
            playerBlack[bestPiece].GetComponent<Chessman>().SetYBoard(bestMoveY); // updates piece's properties
            playerBlack[bestPiece].GetComponent<Chessman>().SetCoords(); // actually moves piece in unity
            SetPosition(playerBlack[bestPiece]); // updates game so we know the piece as moved here


            //Debug.Log("Move Score = " + scores[bestPiece]);
        }
    }

    // Find all legal moves, call with def true to only consider attacking or defending
    // moves (i.e. ignore moves to empty squares)
    public Move[] GetLegalMoves(string piece, int x, int y) 
    {   
        // use a list becase idk how many moves there will be
        List<Move> legalMoves = new List<Move>();

        Move[] right     = LineMoves(x,y, 1, 0,piece);
        Move[] left      = LineMoves(x,y,-1, 0,piece);
        Move[] up        = LineMoves(x,y, 0, 1,piece);
        Move[] down      = LineMoves(x,y, 0,-1,piece);
        Move[] upleft    = LineMoves(x,y,-1, 1,piece);
        Move[] upright   = LineMoves(x,y, 1, 1,piece);
        Move[] downleft  = LineMoves(x,y,-1,-1,piece);
        Move[] downright = LineMoves(x,y, 1,-1,piece);

        // Like MovePlate Code
        switch(piece) 
        {
            case "black_queen":
            case "white_queen":
                legalMoves.AddRange(downleft);
                legalMoves.AddRange(upleft);
                legalMoves.AddRange(downright);
                legalMoves.AddRange(upright);      
                legalMoves.AddRange(right);
                legalMoves.AddRange(left);
                legalMoves.AddRange(up);
                legalMoves.AddRange(down); 
                break;   
            
            case "black_king":
            case "white_king":
                if (PointMove(x,   y+1, piece).score > 0) legalMoves.Add(PointMove(x,   y+1, piece));
                if (PointMove(x,   y-1, piece).score > 0) legalMoves.Add(PointMove(x,   y-1, piece));
                if (PointMove(x-1, y-1, piece).score > 0) legalMoves.Add(PointMove(x-1, y-1, piece));
                if (PointMove(x-1, y,   piece).score > 0) legalMoves.Add(PointMove(x-1, y,   piece));
                if (PointMove(x-1, y+1, piece).score > 0) legalMoves.Add(PointMove(x-1, y+1, piece));
                if (PointMove(x+1, y-1, piece).score > 0) legalMoves.Add(PointMove(x+1, y-1, piece));
                if (PointMove(x+1, y,   piece).score > 0) legalMoves.Add(PointMove(x+1, y,   piece));
                if (PointMove(x+1, y+1, piece).score > 0) legalMoves.Add(PointMove(x+1, y+1, piece));
                // two castling cases
                if (this.blackCastleRightOK && PointMove(x+2, y, piece).score > 0) legalMoves.Add(PointMove(x+2, y, piece));
                if (this.blackCastleRightOK && PointMove(x-2, y, piece).score > 0) legalMoves.Add(PointMove(x-2, y, piece));
                break;

            case "black_bishop":
            case "white_bishop":
                legalMoves.AddRange(downleft);
                legalMoves.AddRange(upleft);
                legalMoves.AddRange(downright);
                legalMoves.AddRange(upright);            
                break;
            
            case "black_knight": 
            case "white_knight": // only add legal moves, i.e. score is positive
                if (PointMove(x-2,y-1,piece).score > 0) legalMoves.Add(PointMove(x-2,y-1,piece));
                if (PointMove(x-2,y+1,piece).score > 0) legalMoves.Add(PointMove(x-2,y+1,piece));
                if (PointMove(x-1,y-2,piece).score > 0) legalMoves.Add(PointMove(x-1,y-2,piece));
                if (PointMove(x-1,y+2,piece).score > 0) legalMoves.Add(PointMove(x-1,y+2,piece));
                if (PointMove(x+1,y-2,piece).score > 0) legalMoves.Add(PointMove(x+1,y-2,piece));
                if (PointMove(x+1,y+2,piece).score > 0) legalMoves.Add(PointMove(x+1,y+2,piece));
                if (PointMove(x+2,y-1,piece).score > 0) legalMoves.Add(PointMove(x+2,y-1,piece));
                if (PointMove(x+2,y+1,piece).score > 0) legalMoves.Add(PointMove(x+2,y+1,piece));
                break;

            case "black_pawn":
                if (y == 6) // can jump two squares
                {
                    if (PawnMove(x,y,0,-1,piece).score > 0) legalMoves.Add(PawnMove(x,y,0,-1,piece));
                    if (PawnMove(x,y,0,-2,piece).score > 0) legalMoves.Add(PawnMove(x,y,0,-2,piece));
                }
                else 
                {
                    if (PawnMove(x,y,0,-1,piece).score > 0) legalMoves.Add(PawnMove(x,y,0,-1,piece));
                }
                break;

            case "white_pawn":
                if (y == 1) // can jump two squares
                {
                    if (PawnMove(x,y,0,1,piece).score > 0) legalMoves.Add(PawnMove(x,y,0,1,piece));
                    if (PawnMove(x,y,0,2,piece).score > 0) legalMoves.Add(PawnMove(x,y,0,2,piece));
                }
                else 
                {
                    if (PawnMove(x,y,0,1,piece).score > 0) legalMoves.Add(PawnMove(x,y,0,1,piece));
                }
                break;

            case "black_rook":
            case "white_rook":
                legalMoves.AddRange(right);
                legalMoves.AddRange(left);
                legalMoves.AddRange(up);
                legalMoves.AddRange(down); 
                break; 
        }

         
        bool check_status = IsBlackInCheck(); // save so we can reset after testing moves // TODO need to save black check status as well?
        if (piece.Contains("white")) check_status = IsWhiteInCheck(); 
        // check to make sure each move doesn't put us in, or keep us in check
        foreach (Move m in legalMoves.ToList()) // add .ToList to list so we can delete elements while looping (makes a copy)
        {  
            bool attacking = false;
            if (piece.Contains("black") && this.GetPosition(m.x,m.y) != null && this.GetPosition(m.x,m.y).GetComponent<Chessman>().GetPlayer() == "white") 
                attacking = true;
            if (piece.Contains("white") && this.GetPosition(m.x,m.y) != null && this.GetPosition(m.x,m.y).GetComponent<Chessman>().GetPlayer() == "black") 
                attacking = true;
            
            if (attacking)
            {
                GameObject cp = this.GetPosition(m.x, m.y); // piece we're 'capturing'
                cp.GetComponent<Chessman>().SetXBoard(8); // pretend like we captured
                cp.GetComponent<Chessman>().SetYBoard(8); // so move off board
                this.SetPosition(cp); // update new position 
                this.SetPositionEmpty(m.x, m.y); // set old position as empty

                // save piece's original position for when we undo the move later
                GameObject thiscp = FindPiece(piece); // piece being moved
                int ogx = thiscp.GetComponent<Chessman>().GetXBoard(); 
                int ogy = thiscp.GetComponent<Chessman>().GetYBoard();
                // make the move
                thiscp.GetComponent<Chessman>().SetXBoard(m.x);
                thiscp.GetComponent<Chessman>().SetYBoard(m.y);
                this.SetPosition(thiscp);
                this.SetPositionEmpty(ogx,ogy);
                Check(); // update, are we in check?
                if (piece.Contains("black") && IsBlackInCheck()) legalMoves.Remove(m); // if so, this ain't a legal move
                if (piece.Contains("white") && IsWhiteInCheck()) legalMoves.Remove(m); // if so, this ain't a legal move
                
                // Now undo the move
                thiscp.GetComponent<Chessman>().SetXBoard(ogx);
                thiscp.GetComponent<Chessman>().SetYBoard(ogy);
                this.SetPosition(thiscp);
                cp.GetComponent<Chessman>().SetXBoard(m.x);
                cp.GetComponent<Chessman>().SetYBoard(m.y);
                this.SetPosition(cp);
                this.SetPositionEmpty(8,8); // from when we moved the 'captured' piece off board
            }
            else // if not attacking, a bit simpler
            {
                // save piece's original position for when we undo the move later
                GameObject thiscp = FindPiece(piece);
                int ogx = thiscp.GetComponent<Chessman>().GetXBoard(); 
                int ogy = thiscp.GetComponent<Chessman>().GetYBoard();
                // make the move
                thiscp.GetComponent<Chessman>().SetXBoard(m.x);
                thiscp.GetComponent<Chessman>().SetYBoard(m.y);
                this.SetPosition(thiscp);
                this.SetPositionEmpty(ogx,ogy);
                Check(); // update, are we in check?
                if (piece.Contains("black") && IsBlackInCheck()) legalMoves.Remove(m); // if so, this ain't a legal move
                if (piece.Contains("white") && IsWhiteInCheck()) legalMoves.Remove(m); // if so, this ain't a legal move

                thiscp.GetComponent<Chessman>().SetXBoard(ogx);
                thiscp.GetComponent<Chessman>().SetYBoard(ogy);
                this.SetPosition(thiscp);
                this.SetPositionEmpty(m.x,m.y);
            }

            // reset check status
            if (piece.Contains("black")) SetBlackCheck(check_status);
            if (piece.Contains("white")) SetBlackCheck(check_status);
        }

        // convert to array
        return legalMoves.ToArray();;
    }

    public Move[] LineMoves(int x, int y, int xInc, int yInc, string piece)
    {
        List<Move> moves = new List<Move>();
        
        x += xInc;
        y += yInc;

        // while position is on board, and no piece already there
        while(this.PositionOnBoard(x,y) && this.GetPosition(x,y) == null)
        {
            Move m = new Move();
            m.x = x;
            m.y = y;
            m.attack = false;
            m.score = CalcScore(piece,m);
            moves.Add(m);

            x += xInc;
            y += yInc;
        }

        if (this.PositionOnBoard(x,y) && this.GetPosition(x,y).GetComponent<Chessman>().GetPlayer() == "white")
        {
            Move m = new Move();
            m.x = x;
            m.y = y;
            m.attack = true;
            m.score = CalcScore(piece,m);
            moves.Add(m);
        }

        return moves.ToArray();;
    }

    public Move PawnMove(int x, int y, int xInc, int yInc, string piece)
    {   
        x += xInc;
        y += yInc;
        Move m = new Move();
        m.x = x;
        m.y = y;
        if (this.PositionOnBoard(x,y) == false)
        {
            m.score = -1;
            return m;
        }

        else if (this.GetPosition(x,y) == null) // empty space
        {
            m.attack = false;
            m.score = CalcScore(piece,m);
            if (Math.Abs(yInc) == 2 && this.GetPosition(x,y+1) != null) m.score = -1; // if pawn moving two spaces, can't jump over a piece
            return m; 
        }
        
        // attack diagnoally right (not if jumping two spaces though)
        else if (Math.Abs(yInc) != 2 && this.PositionOnBoard(x+1,y) && this.GetPosition(x+1,y) != null && this.GetPosition(x+1,y).GetComponent<Chessman>().GetPlayer() != "black") 
        {
            m.x = x+1;
            m.attack = true;
            m.score = CalcScore(piece,m);
            return m; 
        }

        // attack diagonally left (not if jumping two spaces though)
        else if (Math.Abs(yInc) != 2 && this.PositionOnBoard(x-1,y) && this.GetPosition(x-1,y) != null && this.GetPosition(x-1,y).GetComponent<Chessman>().GetPlayer() != "black") 
        {
            m.x = x-1;
            m.attack = true;
            m.score = CalcScore(piece,m);
            return m; 
        }
        else
        {
            m.score = -1;
            return m;
        }
    }

    public Move PointMove(int x, int y, string piece)
    {   
        Move m = new Move();
        m.x = x;
        m.y = y;
        if (this.PositionOnBoard(x,y) && this.GetPosition(x,y) == null) // is position on board and no piece already there? 
        {
            m.attack = false;
            m.score = CalcScore(piece,m);
            return m;
        }
        else if (this.PositionOnBoard(x,y) && this.GetPosition(x,y).GetComponent<Chessman>().GetPlayer() != "black") 
        {
            m.attack = true;
            m.score = CalcScore(piece,m);
            return m;
        }
        else // give illegal moves negative score
        {
            m.score = -1;
            return m;
        }
    }
    public int CalcScore(string piece, Move m) // piece that's moving, where it's going x,y,
    {
        int score = Random.Range(0,100);
        
        // attacking moves are good - general
        if (m.attack == true) score += Random.Range(0,100);

        // is position on board and no piece already there? 
        if (this.PositionOnBoard(m.x,m.y) && this.GetPosition(m.x,m.y) == null) 
        {
            // todo add map for each piece, favoring certain squares
            if (this.GetTurn() < 10) 
                if (m.x >=2 && m.y >=2) // center is good
                    score += Random.Range(0,45); 
        }
        
        // Being defended is good
        score += Random.Range(0,10*CountDefenders(m.x, m.y, piece));

        // Being attacked is bad
        score -= Random.Range(0,10*CountAttackers(m.x, m.y, piece));

        // Capturing Pieces is good
        if (m.attack == true) 
        {   
            int enemy_value = GetValue(this.GetPosition(m.x,m.y).GetComponent<Chessman>().name);
            int my_value = GetValue(piece);
            score += 60*(enemy_value/my_value);
            //Debug.Log("Capture Score = " + score);
        }

        return score;
    }

    public void Winner(string playerWinner)
    {
        gameOver = true;

        // remove pieces with position of board

        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().enabled = true;
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().text = playerWinner + " is the winner";
        GameObject.FindGameObjectWithTag("RestartText").GetComponent<Text>().enabled = true;
    }
    
    // Find out if either king is in check
    public void Check()
    {   
        // kings are 4th element in the player's piece arrays
        if (CountAttackers(playerBlack[4].GetComponent<Chessman>().GetXBoard(), playerBlack[4].GetComponent<Chessman>().GetYBoard(), playerBlack[4].GetComponent<Chessman>().name) > 0)
            SetBlackCheck(true);
        else
            SetBlackCheck(false);
        if (CountAttackers(playerWhite[4].GetComponent<Chessman>().GetXBoard(), playerWhite[4].GetComponent<Chessman>().GetYBoard(), playerWhite[4].GetComponent<Chessman>().name) > 0)
            SetWhiteCheck(true);
        else 
            SetWhiteCheck(false);
    }

     // counts how many friendly pieces are defending the square x,y
    public int CountDefenders(int x, int y, string piece)
    {   
        int defenders = 0;

       
        if (piece.Substring(0, 5) == "white")
        {
             // is friendly pawn diagonally behind?
            if (FriendHere(x,y,-1,-1,"white_pawn")) defenders++;
            if (FriendHere(x,y, 1,-1,"white_pawn")) defenders++;

            // is friendly bishop guarding?
            if (FriendHere(x,y, 1, 1,"white_bishop")) defenders++;
            if (FriendHere(x,y, 1,-1,"white_bishop")) defenders++;
            if (FriendHere(x,y,-1, 1,"white_bishop")) defenders++;
            if (FriendHere(x,y,-1,-1,"white_bishop")) defenders++;

            // is friendly rook guarding?
            if (FriendHere(x,y, 1, 0,"white_rook")) defenders++;
            if (FriendHere(x,y, 0, 1,"white_rook")) defenders++;
            if (FriendHere(x,y,-1, 0,"white_rook")) defenders++;
            if (FriendHere(x,y, 0,-1,"white_rook")) defenders++;
        }
        if (piece.Substring(0, 5) == "black")
        {
            // is friendly pawn diagonally behind?
            if (FriendHere(x,y,-1, 1,"black_pawn")) defenders++;
            if (FriendHere(x,y, 1, 1,"black_pawn")) defenders++;

            // is friendly bishop guarding?
            if (FriendHere(x,y, 1, 1,"black_bishop")) defenders++;
            if (FriendHere(x,y, 1,-1,"black_bishop")) defenders++;
            if (FriendHere(x,y,-1, 1,"black_bishop")) defenders++;
            if (FriendHere(x,y,-1,-1,"black_bishop")) defenders++;

            // is friendly rook guarding?
            if (FriendHere(x,y, 1, 0,"black_rook")) defenders++;
            if (FriendHere(x,y, 0, 1,"black_rook")) defenders++;
            if (FriendHere(x,y,-1, 0,"black_rook")) defenders++;
            if (FriendHere(x,y, 0,-1,"black_rook")) defenders++;
        }
        return defenders;
    }


     // counts how many enemy pieces are attacking the x,y square
    public int CountAttackers(int x, int y, string piece)
    {   
        int attackers = 0;

        
        if (piece.Contains("white"))
        {
            // is enemy pawn diagonally in front?
            attackers += FoeHere(x,y,-1, 1,"black_pawn");
            attackers += FoeHere(x,y, 1, 1,"black_pawn");

            // is enemy bishop attacking?
            attackers += FoeHere(x,y, 1, 1,"black_bishop");
            attackers += FoeHere(x,y, 1,-1,"black_bishop");
            attackers += FoeHere(x,y,-1, 1,"black_bishop");
            attackers += FoeHere(x,y,-1,-1,"black_bishop");

            // is enemy knight attacking?
            attackers += FoeHere(x,y, 1, 2,"black_knight");
            attackers += FoeHere(x,y,-1, 2,"black_knight");
            attackers += FoeHere(x,y, 2, 1,"black_knight");
            attackers += FoeHere(x,y, 2,-1,"black_knight");
            attackers += FoeHere(x,y, 1,-2,"black_knight");
            attackers += FoeHere(x,y,-1,-2,"black_knight");
            attackers += FoeHere(x,y,-2, 1,"black_knight");
            attackers += FoeHere(x,y,-2,-1,"black_knight");

            // is enemy rook attacking?
            attackers += FoeHere(x,y, 1, 0,"black_rook");
            attackers += FoeHere(x,y, 0, 1,"black_rook");
            attackers += FoeHere(x,y,-1, 0,"black_rook");
            attackers += FoeHere(x,y, 0,-1,"black_rook");

            // is enemy queen attacking?
            attackers += FoeHere(x,y, 1, 0,"black_queen");
            attackers += FoeHere(x,y, 0, 1,"black_queen");
            attackers += FoeHere(x,y,-1, 0,"black_queen");
            attackers += FoeHere(x,y, 0,-1,"black_queen");
            attackers += FoeHere(x,y, 1, 1,"black_queen");
            attackers += FoeHere(x,y, 1,-1,"black_queen");
            attackers += FoeHere(x,y,-1, 1,"black_queen");
            attackers += FoeHere(x,y,-1,-1,"black_queen");

            // Keep track of Kings as well, to make sure a King never attacks a King 
            attackers += FoeHere(x,y, 1, 0,"black_king");
            attackers += FoeHere(x,y, 0, 1,"black_king");
            attackers += FoeHere(x,y,-1, 0,"black_king");
            attackers += FoeHere(x,y, 0,-1,"black_king");
        }
        if (piece.Contains("black"))
        {
            // is enemy pawn diagonally in front?
            attackers += FoeHere(x,y,-1,-1,"white_pawn");
            attackers += FoeHere(x,y, 1,-1,"white_pawn");
            
            // is enemy bishop attacking?
            attackers += FoeHere(x,y, 1, 1,"white_bishop");
            attackers += FoeHere(x,y, 1,-1,"white_bishop");
            attackers += FoeHere(x,y,-1, 1,"white_bishop");
            attackers += FoeHere(x,y,-1,-1,"white_bishop");

            // is enemy knight attacking?
            attackers += FoeHere(x,y, 1, 2,"white_knight");
            attackers += FoeHere(x,y,-1, 2,"white_knight");
            attackers += FoeHere(x,y, 2, 1,"white_knight");
            attackers += FoeHere(x,y, 2,-1,"white_knight");
            attackers += FoeHere(x,y, 1,-2,"white_knight");
            attackers += FoeHere(x,y,-1,-2,"white_knight");
            attackers += FoeHere(x,y,-2, 1,"white_knight");
            attackers += FoeHere(x,y,-2,-1,"white_knight");

            // is enemy rook attacking?
            attackers += FoeHere(x,y, 1, 0,"white_rook");
            attackers += FoeHere(x,y, 0, 1,"white_rook");
            attackers += FoeHere(x,y,-1, 0,"white_rook");
            attackers += FoeHere(x,y, 0,-1,"white_rook");

            // is enemy queen attacking?
            attackers += FoeHere(x,y, 1, 0,"white_queen");
            attackers += FoeHere(x,y, 0, 1,"white_queen");
            attackers += FoeHere(x,y,-1, 0,"white_queen");
            attackers += FoeHere(x,y, 0,-1,"white_queen");
            attackers += FoeHere(x,y, 1, 1,"white_queen");
            attackers += FoeHere(x,y, 1,-1,"white_queen");
            attackers += FoeHere(x,y,-1, 1,"white_queen");
            attackers += FoeHere(x,y,-1,-1,"white_queen");

            // Keep track of Kings as well, to make sure a King never attacks a King 
            attackers += FoeHere(x,y, 1, 0,"white_king");
            attackers += FoeHere(x,y, 0, 1,"white_king");
            attackers += FoeHere(x,y,-1, 0,"white_king");
            attackers += FoeHere(x,y, 0,-1,"white_king");
        }    

        return attackers;
    }

    // return true if there is a friendly piece at this square
    public bool FriendHere(int x, int y, int xIncrement, int yIncrement, string piece)
    {   
        x += xIncrement;
        y += yIncrement;

        if (piece == "white_pawn" || piece == "black_pawn") 
        {
            if (!this.PositionOnBoard(x,y)) // not valid square
                return false;
            else if (this.GetPosition(x,y) == null) // empty square
                return false;
            else if (piece == this.GetPosition(x,y).GetComponent<Chessman>().name) // the friendly piece "piece", is here at x,y
                return true;
            else 
                return false;
        }
        else if (piece.Contains("bishop") || piece.Contains("rook") || piece.Contains("queen")) 
        {
            while(this.PositionOnBoard(x,y) && this.GetPosition(x,y) == null) // go through diagonal until you hit a piece or go off the board
            {   
                x += xIncrement;
                y += yIncrement;
            }
            if (!this.PositionOnBoard(x,y)) // not valid square
                return false;
            else if (this.GetPosition(x,y) == null) // empty square
                return false;
            else if (piece == this.GetPosition(x,y).GetComponent<Chessman>().name) // the friendly piece "piece", is here at x,y
                return true;
            else 
                return false;
        }
        else
            return false;
    }


    // return value of enemy piece at this square
    public int FoeHere(int x, int y, int xIncrement, int yIncrement, string piece)
    {   
        x += xIncrement;
        y += yIncrement;

            if (piece.Contains("pawn") || piece.Contains("knight") || piece.Contains("king")) 
            {
                if (!this.PositionOnBoard(x,y)) // not valid square
                    return 0;
                else if (this.GetPosition(x,y) == null) // empty square
                    return 0;
                else if (piece == this.GetPosition(x,y).GetComponent<Chessman>().name)
                    return GetValue(this.GetPosition(x,y).GetComponent<Chessman>().name);
                else 
                    return 0;
            }

            else if (piece.Contains("bishop") || piece.Contains("rook") || piece.Contains("queen")) 
            {
                while(this.PositionOnBoard(x,y) && this.GetPosition(x,y) == null) // go through diagonal until you hit a piece or go off the board
                {   
                    x += xIncrement;
                    y += yIncrement;
                    //Debug.Log("x = " + x + ", y = " + y);
                }
                if (!this.PositionOnBoard(x,y)) // not valid square
                    return 0;
                else if (this.GetPosition(x,y) == null) // empty square
                    return 0;
                else if (piece == this.GetPosition(x,y).GetComponent<Chessman>().name) 
                    return GetValue(this.GetPosition(x,y).GetComponent<Chessman>().name);
                else 
                    return 0;
            }

            else
                return 0;
    }
}
