using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;		
using UnityEngine.UI;
using System.Linq;

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

    // value of pieces
    public Dictionary<string,int> pieceValue = new Dictionary<string,int>();

    // Positions and team for each chesspiece
    private GameObject[,] positions = new GameObject[8,8]; // holds piece's at each board position
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
        if (x < 0 || y < 0 || x >= positions.GetLength(0) || y >= positions.GetLength(1))
            return false;
        else 
            return true;
    }

    // getters and setters 
    public string GetCurrentPlayer(){ return currentPlayer; }
    public bool IsGameOver(){ return gameOver; }
    public int GetTurn(){ return turn/2; } // how many turns have been played (each player going counts as 1)
    public int GetValue(string p) { return pieceValue[p]; }

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
            gameOver = false;
            SceneManager.LoadScene("Game");
        }

        if (currentPlayer == "black") // cpu's turn
        {   
            Invoke("makeMove", 1); // wait 3 seconds then move
            NextTurn();
        }
    }

    // CPU make's a move
    public void makeMove()
    {
        Debug.Log("Making Moves...");

        int bestMoveX;
        int bestMoveY;

        Move[] moves = new Move[playerBlack.Length];
        int[] scores = new int[playerBlack.Length]; // to quickly find best score and it's index(piece)
        for (int i = 0; i < playerBlack.Length; i++) // piece loop
        {
            // mem saver
            if (i > 1) break;

            Debug.Log("Considering " + playerBlack[i].GetComponent<Chessman>().name + "'s Legal Moves...");
            int x = playerBlack[i].GetComponent<Chessman>().GetXBoard();
            int y = playerBlack[i].GetComponent<Chessman>().GetYBoard();
            Move[] legals = GetLegalMoves(playerBlack[i].GetComponent<Chessman>().name, x, y, false);

            // default score to -1, in case of no legal moves
            int minScore = -1;
            scores[i] = -1; 

            for (int j = 0; j < legals.Length; i++) { // Find Piece i's best move
                if (legals[j].score > minScore) {
                    moves[i]  = legals[j];
                    scores[i] = legals[j].score;
                    minScore  = legals[j].score;
                }                    
            }
            
        }
        
        int bestPiece = scores.ToList().IndexOf(scores.Max()); // get best move from all pieces
        bestMoveX = moves[bestPiece].x;
        bestMoveY = moves[bestPiece].y;

        // Put piece in new space 
        bool attacking = false;
        if (this.GetPosition(bestMoveX,bestMoveY) != null && this.GetPosition(bestMoveX,bestMoveY).GetComponent<Chessman>().GetPlayer() == "white") 
            attacking = true;
        if (attacking)
        {
            // get piece at moveplate's position
            GameObject cp = this.GetPosition(bestMoveX, bestMoveY);
            // if destroying King, end game
            if (cp.name == "white_king") this.Winner("black");
            if (cp.name == "black_king") this.Winner("white");

            // Remove piece from game (shrink and put to side)
            string col = cp.GetComponent<Chessman>().GetPlayer();
            if (col == "white")
            {
                cp.GetComponent<Chessman>().SetXBoard(8);
                cp.GetComponent<Chessman>().SetYBoard(8 - this.GetCaptured(col));
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

        // set original location to empty
        SetPositionEmpty(playerBlack[bestPiece].GetComponent<Chessman>().GetXBoard(),playerBlack[bestPiece].GetComponent<Chessman>().GetYBoard());

        playerBlack[bestPiece].GetComponent<Chessman>().SetXBoard(bestMoveX); // updates piece's properties
        playerBlack[bestPiece].GetComponent<Chessman>().SetYBoard(bestMoveY); // updates piece's properties
        playerBlack[bestPiece].GetComponent<Chessman>().SetCoords(); // actually moves piece in unity
        SetPosition(playerBlack[bestPiece]); // updates game so we know the piece as moved here

        Debug.Log("Move Score = " + scores[bestPiece]);
    }

    // Find all legal moves, call with def true to only consider attacking or defending
    // moves (i.e. ignore moves to empty squares)
    public Move[] GetLegalMoves(string piece, int x, int y, bool def) 
    {   
        // use a list becase idk how many moves there will be
        List<Move> legalMoves = new List<Move>();

        Move[] right     = LineMoves(x,y, 1, 0,piece, def);
        Move[] left      = LineMoves(x,y,-1, 0,piece, def);
        Move[] up        = LineMoves(x,y, 0, 1,piece, def);
        Move[] down      = LineMoves(x,y, 0,-1,piece, def);
        Move[] upleft    = LineMoves(x,y,-1, 1,piece, def);
        Move[] upright   = LineMoves(x,y, 1, 1,piece, def);
        Move[] downleft  = LineMoves(x,y,-1,-1,piece, def);
        Move[] downright = LineMoves(x,y, 1,-1,piece, def);

        // Like MovePlate Code
        switch(piece) 
        {
            case "black_queen":
                for (int i = 0; i < downleft.Length; i++)
                    if (downleft[i].score > 0) legalMoves.Add(downleft[i]);
                for (int i = 0; i < upleft.Length; i++)
                    if (upleft[i].score > 0) legalMoves.Add(upleft[i]);
                for (int i = 0; i < downright.Length; i++)
                    if (downright[i].score > 0) legalMoves.Add(downright[i]);
                for (int i = 0; i < upright.Length; i++)
                    if (upright[i].score > 0) legalMoves.Add(upright[i]);       

                for (int i = 0; i < right.Length; i++)
                    if (right[i].score > 0) legalMoves.Add(right[i]);          
                for (int i = 0; i < left.Length; i++)
                    if (left[i].score > 0) legalMoves.Add(left[i]);     
                for (int i = 0; i < up.Length; i++)
                    if (up[i].score > 0) legalMoves.Add(up[i]);    
                for (int i = 0; i < down.Length; i++)
                    if (down[i].score > 0) legalMoves.Add(down[i]);  
                break;   
            
            case "black_king":
                if (PointMove(x,y, 0 ,1,piece, def).score > 0) legalMoves.Add(PointMove(x,y, 0, 1,piece, def));
                if (PointMove(x,y, 0,-1,piece, def).score > 0) legalMoves.Add(PointMove(x,y, 0,-1,piece, def));
                if (PointMove(x,y,-1,-1,piece, def).score > 0) legalMoves.Add(PointMove(x,y,-1,-1,piece, def));
                if (PointMove(x,y,-1, 0,piece, def).score > 0) legalMoves.Add(PointMove(x,y,-1, 0,piece, def));
                if (PointMove(x,y,-1, 1,piece, def).score > 0) legalMoves.Add(PointMove(x,y,-1, 1,piece, def));
                if (PointMove(x,y, 1,-1,piece, def).score > 0) legalMoves.Add(PointMove(x,y, 1,-1,piece, def));
                if (PointMove(x,y, 1, 0,piece, def).score > 0) legalMoves.Add(PointMove(x,y, 1, 0,piece, def));
                if (PointMove(x,y, 1, 1,piece, def).score > 0) legalMoves.Add(PointMove(x,y, 1, 1,piece, def));
                break;

            case "black_bishop":
                for (int i = 0; i < downleft.Length; i++)
                    if (downleft[i].score > 0) legalMoves.Add(downleft[i]);
                for (int i = 0; i < upleft.Length; i++)
                    if (upleft[i].score > 0) legalMoves.Add(upleft[i]);
                for (int i = 0; i < downright.Length; i++)
                    if (downright[i].score > 0) legalMoves.Add(downright[i]);
                for (int i = 0; i < upright.Length; i++)
                    if (upright[i].score > 0) legalMoves.Add(upright[i]);                
                break;
            
            case "black_knight": // only add legal moves, i.e. score is positive
                if (PointMove(x,y,-2,-1,piece, def).score > 0) legalMoves.Add(PointMove(x,y,-2,-1,piece, def));
                if (PointMove(x,y,-2, 1,piece, def).score > 0) legalMoves.Add(PointMove(x,y,-2, 1,piece, def));
                if (PointMove(x,y,-1,-2,piece, def).score > 0) legalMoves.Add(PointMove(x,y,-1,-2,piece, def));
                if (PointMove(x,y,-1, 2,piece, def).score > 0) legalMoves.Add(PointMove(x,y,-1, 2,piece, def));
                if (PointMove(x,y, 1,-2,piece, def).score > 0) legalMoves.Add(PointMove(x,y, 1,-2,piece, def));
                if (PointMove(x,y, 1, 2,piece, def).score > 0) legalMoves.Add(PointMove(x,y, 1, 2,piece, def));
                if (PointMove(x,y, 2,-1,piece, def).score > 0) legalMoves.Add(PointMove(x,y, 2,-1,piece, def));
                if (PointMove(x,y, 2, 1,piece, def).score > 0) legalMoves.Add(PointMove(x,y, 2, 1,piece, def));
                break;

            case "black_pawn":
                if (y == 6) // can jump two squares
                {
                    if (PawnMove(x,y,0,-1,piece, def).score > 0) legalMoves.Add(PawnMove(x,y,0,-1,piece, def));
                    if (PawnMove(x,y,0,-2,piece, def).score > 0) legalMoves.Add(PawnMove(x,y,0,-2,piece, def));
                }
                else 
                {
                    if (PawnMove(x,y,0,-1,piece, def).score > 0) legalMoves.Add(PawnMove(x,y,0,-1,piece, def));
                }
                break;

            case "black_rook":
                for (int i = 0; i < right.Length; i++)
                    if (right[i].score > 0) legalMoves.Add(right[i]);          
                for (int i = 0; i < left.Length; i++)
                    if (left[i].score > 0) legalMoves.Add(left[i]);     
                for (int i = 0; i < up.Length; i++)
                    if (up[i].score > 0) legalMoves.Add(up[i]);    
                for (int i = 0; i < down.Length; i++)
                    if (down[i].score > 0) legalMoves.Add(down[i]);  
                break; 
        }

        // convert to array
        Move[] moves = legalMoves.ToArray();
        return moves;
    }

    public Move[] LineMoves(int x, int y, int xInc, int yInc, string piece, bool def)
    {
        Move[] moves = new Move[28]; // can't be more than 28 legal moves for a piece I think
        
        x += xInc;
        y += yInc;
        int counter = 0;

        // while position is on board, and no piece already there
        while(this.PositionOnBoard(x,y) && this.GetPosition(x,y) == null)
        {
            Move m = new Move();
            m.x = x;
            m.y = y;
            m.attack = false;
            m.score = CalcScore(piece,m);
            if (def) m.score = -1;
            moves[counter] = m;
            counter ++;

            x += xInc;
            y += yInc;
        }

        if (this.PositionOnBoard(x,y) && this.GetPosition(x,y).GetComponent<Chessman>().GetPlayer() != "black")
        {
            Move m = new Move();
            m.x = x;
            m.y = y;
            m.attack = true;
            m.score = CalcScore(piece,m);
            moves[counter] = m;
            counter ++; 
        }

        // set negative scores for rest of moves array that we didn't need to fill
        for (int i = counter; i < moves.Length; i++){
            moves[i].score = -1;
        }

        return moves;
    }

    public Move PawnMove(int x, int y, int xInc, int yInc, string piece, bool def)
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
            if (def) m.score = -1;
            return m; 
        }
        
        // attack diagnoally right
        else if (this.PositionOnBoard(x+1,y) && this.GetPosition(x+1,y) != null && this.GetPosition(x+1,y).GetComponent<Chessman>().GetPlayer() != "black") 
        {
            m.x = x+1;
            m.attack = true;
            m.score = CalcScore(piece,m);
            return m; 
        }

        // attack diagonally left
        else if (this.PositionOnBoard(x-1,y) && this.GetPosition(x-1,y) != null && this.GetPosition(x-1,y).GetComponent<Chessman>().GetPlayer() != "black") 
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

    public Move PointMove(int x, int y, int xInc, int yInc, string piece, bool def)
    {   
        x += xInc;
        y += yInc;
        Move m = new Move();
        m.x = x;
        m.y = y;
        if (this.PositionOnBoard(x,y) && this.GetPosition(x,y) == null) // is position on board and no piece already there? 
        {
            m.attack = false;
            m.score = CalcScore(piece,m);
            if (def) m.score = -1;
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
            Debug.Log("Capture Score = " + score);
        }

        return score;
    }

    /* Too Slow
    public int CountDefense(string piece, Move m)
    {
        int defscore = 0;
        if (piece == "black_king") return defscore; // king can't gaurd anyone

        // second moves show who this piece is defending
        Move[] second_move = GetLegalMoves(piece, m.x, m.y, true);

        // count how many pieces we'd be defending
        for (int i = 0; i < second_move.Length; i++)
        {   
            int x = second_move[i].x;
            int y = second_move[i].y;

            if (this.PositionOnBoard(x,y) && this.GetPosition(x,y) == null)
                defscore += 0;
            else if (this.PositionOnBoard(x,y) && this.GetPosition(x,y).GetComponent<Chessman>().GetPlayer() == "black")
            {
                defscore += 3*GetValue(this.GetPosition(x,y).GetComponent<Chessman>().name);
            }
        }

        defscore -= GetValue(piece);

        return defscore;
    }
    */

    public void Winner(string playerWinner)
    {
        gameOver = true;

        // remove pieces with position of board

        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().enabled = true;
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().text = playerWinner + " is the winner";
        GameObject.FindGameObjectWithTag("RestartText").GetComponent<Text>().enabled = true;
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


     // counts how many enemey pieces are attacking the x,y square
    public int CountAttackers(int x, int y, string piece)
    {   
        int attackers = 0;

        // is enemy pawn diagonally in front?
        if (piece.Substring(0, 5) == "white")
        {
            attackers += FoeHere(x,y,-1, 1,"black_pawn");
            attackers += FoeHere(x,y, 1, 1,"black_pawn");

            // is enemy bishop attacking?
            attackers += FoeHere(x,y, 1, 1,"black_bishop");
            attackers += FoeHere(x,y, 1,-1,"black_bishop");
            attackers += FoeHere(x,y,-1, 1,"black_bishop");
            attackers += FoeHere(x,y,-1,-1,"black_bishop");

            // is enemy rook attacking?
            attackers += FoeHere(x,y, 1, 0,"black_rook");
            attackers += FoeHere(x,y, 0, 1,"black_rook");
            attackers += FoeHere(x,y,-1, 0,"black_rook");
            attackers += FoeHere(x,y, 0,-1,"black_rook");
        }
        if (piece.Substring(0, 5) == "black")
        {
            // is enemy pawn diagonally in front?
            attackers += FoeHere(x,y,-1,-1,"white_pawn");
            attackers += FoeHere(x,y, 1,-1,"white_pawn");

            // is enemy bishop attacking?
            attackers += FoeHere(x,y, 1, 1,"white_bishop");
            attackers += FoeHere(x,y, 1,-1,"white_bishop");
            attackers += FoeHere(x,y,-1, 1,"white_bishop");
            attackers += FoeHere(x,y,-1,-1,"white_bishop");

            // is enemy rook attacking?
            attackers += FoeHere(x,y, 1, 0,"white_rook");
            attackers += FoeHere(x,y, 0, 1,"white_rook");
            attackers += FoeHere(x,y,-1, 0,"white_rook");
            attackers += FoeHere(x,y, 0,-1,"white_rook");
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
            else if (this.GetPosition(x,y).GetComponent<Chessman>().name.Substring(0, 5) == piece.Substring(0, 5))  // same color piece
                return true;
            else 
                return false;
        }
        else if (piece.Substring(6, 4) == "bish" || piece.Substring(6, 4) == "rook" || piece.Substring(6, 4) == "quee") 
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
            else if (this.GetPosition(x,y).GetComponent<Chessman>().name.Substring(0, 5) == piece.Substring(0, 5)) // same color piece
                return true;
            else 
                return false;
        }
        else
            return false;
    }


    // return true if there is an enemy piece at this square
    public int FoeHere(int x, int y, int xIncrement, int yIncrement, string piece)
    {   
        x += xIncrement;
        y += yIncrement;

            if (piece == "white_pawn" || piece == "black_pawn") 
            {
                if (!this.PositionOnBoard(x,y)) // not valid square
                    return 0;
                else if (this.GetPosition(x,y) == null) // empty square
                    return 0;
                else if (piece.Substring(0, 5) == "white" && this.GetPosition(x,y).GetComponent<Chessman>().GetPlayer() == "black") 
                    return GetValue(this.GetPosition(x,y).GetComponent<Chessman>().name);
                else if (piece.Substring(0, 5) == "black" && this.GetPosition(x,y).GetComponent<Chessman>().GetPlayer() == "white") 
                    return GetValue(this.GetPosition(x,y).GetComponent<Chessman>().name);
                else 
                    return 0;
            }

            else if (piece.Substring(6, 4) == "bish" || piece.Substring(6, 4) == "rook" || piece.Substring(6, 4) == "quee") 
            {
                while(this.PositionOnBoard(x,y) && this.GetPosition(x,y) == null) // go through diagonal until you hit a piece or go off the board
                {   
                    x += xIncrement;
                    y += yIncrement;
                }
                if (!this.PositionOnBoard(x,y)) // not valid square
                    return 0;
                else if (this.GetPosition(x,y) == null) // empty square
                    return 0;
                else if (piece.Substring(0, 5) == "white" && this.GetPosition(x,y).GetComponent<Chessman>().GetPlayer() == "black") 
                    return GetValue(this.GetPosition(x,y).GetComponent<Chessman>().name);
                else if (piece.Substring(0, 5) == "black" && this.GetPosition(x,y).GetComponent<Chessman>().GetPlayer() == "white") 
                    return GetValue(this.GetPosition(x,y).GetComponent<Chessman>().name);
                else 
                    return 0;
            }

            else
                return 0;
    }
}
