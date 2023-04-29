using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chessman : MonoBehaviour
{
    // References
    public GameObject controller;
    public GameObject movePlate; // where you can move your piece

    // Positions
    private int xBoard = -1;
    private int yBoard = -1;

    private string player; // black or white player

    // References for all the sprites that the chesspiece can be
    public Sprite black_queen, black_king, black_bishop, black_knight, black_rook, black_pawn;
    public Sprite white_queen, white_king, white_bishop, white_knight, white_rook, white_pawn;

    // called when chesspiece is created
    public void Activate()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");

        // convert unity coords to chessboard coords
        SetCoords();

        // set up Sprite image
        switch (this.name)
        {
            case "black_queen" : this.GetComponent<SpriteRenderer>().sprite = black_queen; player = "black"; break;
            case "black_king"  : this.GetComponent<SpriteRenderer>().sprite = black_king; player = "black"; break;
            case "black_bishop": this.GetComponent<SpriteRenderer>().sprite = black_bishop; player = "black"; break;
            case "black_knight": this.GetComponent<SpriteRenderer>().sprite = black_knight; player = "black"; break;
            case "black_rook"  : this.GetComponent<SpriteRenderer>().sprite = black_rook; player = "black"; break;
            case "black_pawn"  : this.GetComponent<SpriteRenderer>().sprite = black_pawn; player = "black"; break;

            case "white_queen" : this.GetComponent<SpriteRenderer>().sprite = white_queen; player = "white"; break;
            case "white_king"  : this.GetComponent<SpriteRenderer>().sprite = white_king; player = "white"; break;
            case "white_bishop": this.GetComponent<SpriteRenderer>().sprite = white_bishop; player = "white"; break;
            case "white_knight": this.GetComponent<SpriteRenderer>().sprite = white_knight; player = "white"; break;
            case "white_rook"  : this.GetComponent<SpriteRenderer>().sprite = white_rook; player = "white"; break;
            case "white_pawn"  : this.GetComponent<SpriteRenderer>().sprite = white_pawn; player = "white"; break;
        }
    }

    // keeps track of global coords, based on board coords
    public void SetCoords(){ 
        float x = xBoard;
        float y = yBoard;

        // found through trial and error
        x *= 1.25f;
        y *= 1.25f;

        x += -4.4f;
        y += -4.4f;

        this.transform.position = new Vector3(x,y,-1.0f); // set z = -1 so in front of board
    }

    // Getters and Setters
    public string GetPlayer() {return player; }
    public int GetXBoard() { return xBoard; }
    public int GetYBoard() { return yBoard; }

    public void SetXBoard(int x) { xBoard = x; }
    public void SetYBoard(int y) { yBoard = y; }

    // When you click on a piece
    public void OnMouseUp()
    {
        if (!controller.GetComponent<Game>().IsGameOver() && this.name.Contains("white"))
        {
            DestroyMovePlates();
            InitiateMovePlates();
        }
    }

    public void DestroyMovePlates()
    {   
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate"); // all moveplates that currently exist
        for (int i = 0; i < movePlates.Length; i++) {
            Destroy(movePlates[i]);
        }
    }

    public void InitiateMovePlates()
    {
        switch (this.name)
        {   
            case "black_king":
            case "white_king":
                SurroundMovePlate();
                CastleMovePlate(xBoard, -2, yBoard, 0, "white_king");
                CastleMovePlate(xBoard,  2, yBoard, 0, "white_king");
                break;


            case "black_queen":
            case "white_queen":
                LineMovePlate(1,0, "white_queen");
                LineMovePlate(0,1, "white_queen");
                LineMovePlate(1,1, "white_queen");
                LineMovePlate(-1,0, "white_queen");
                LineMovePlate(0,-1, "white_queen");
                LineMovePlate(-1,-1, "white_queen");
                LineMovePlate(-1,1, "white_queen");
                LineMovePlate(1,-1, "white_queen");
                break;
            
            case "black_knight":
            case "white_knight":
                LMovePlate();
                break;
            
            case "black_bishop":
            case "white_bishop":
                LineMovePlate(1,1, "white_bishop");   
                LineMovePlate(1,-1, "white_bishop");   
                LineMovePlate(-1,1, "white_bishop");   
                LineMovePlate(-1,-1, "white_bishop");    
                break;

            case "black_rook":
            case "white_rook":
                LineMovePlate(1,0, "white_rook");
                LineMovePlate(0,1, "white_rook");
                LineMovePlate(-1,0, "white_rook");
                LineMovePlate(0,-1, "white_rook");
                break;
            
            case "black_pawn":
                if (yBoard == 6) 
                {
                    PawnMovePlate(xBoard, yBoard - 1);
                    PawnMovePlate(xBoard, yBoard - 2);
                }
                else PawnMovePlate(xBoard, yBoard - 1);
                break;
            case "white_pawn":
                if (yBoard == 1)
                {
                    PawnMovePlate(xBoard, yBoard + 1);
                    PawnMovePlate(xBoard, yBoard + 2);
                }
                else PawnMovePlate(xBoard, yBoard + 1);
                break;
        }
    }

    public void LineMovePlate(int xIncrement, int yIncrement, string piece)
    {
        Game sc = controller.GetComponent<Game>();

        int x = xBoard + xIncrement;
        int y = yBoard + yIncrement;

        // while position is on board, and no piece already there
        while(sc.PositionOnBoard(x,y) && sc.GetPosition(x,y) == null)
        {
            MovePlateSpawn(x,y, piece);
            x += xIncrement;
            y += yIncrement;
        }

        // place attack moveplate if on board and on opponents piece
        if (sc.PositionOnBoard(x,y) && sc.GetPosition(x,y).GetComponent<Chessman>().player != player)
        {
            MovePlateAttackSpawn(x,y, piece); 
        }
    }

    public void LMovePlate()
    {
        PointMovePlate(xBoard + 1, yBoard + 2, "white_knight");
        PointMovePlate(xBoard - 1, yBoard + 2, "white_knight");
        PointMovePlate(xBoard + 2, yBoard + 1, "white_knight");
        PointMovePlate(xBoard + 2, yBoard - 1, "white_knight");
        PointMovePlate(xBoard + 1, yBoard - 2, "white_knight");
        PointMovePlate(xBoard - 1, yBoard - 2, "white_knight");
        PointMovePlate(xBoard - 2, yBoard + 1, "white_knight");
        PointMovePlate(xBoard - 2, yBoard - 1, "white_knight");

    }

    public void SurroundMovePlate()
    {   
        PointMovePlate(xBoard + 0, yBoard + 1, "white_king");
        PointMovePlate(xBoard + 0, yBoard - 1, "white_king");
        PointMovePlate(xBoard - 1, yBoard - 1, "white_king");
        PointMovePlate(xBoard - 1, yBoard + 0, "white_king");
        PointMovePlate(xBoard - 1, yBoard + 1, "white_king");
        PointMovePlate(xBoard + 1, yBoard - 1, "white_king");
        PointMovePlate(xBoard + 1, yBoard + 0, "white_king");
        PointMovePlate(xBoard + 1, yBoard + 1, "white_king");
    }

    public void CastleMovePlate(int x, int xInc, int y, int yInc, string piece)
    {
        Game sc = controller.GetComponent<Game>();
        
        if (xInc == -2 && sc.whiteCastleLeftOK) MovePlateSpawn(x + xInc, y + yInc, piece);  
        if (xInc == 2 && sc.whiteCastleRightOK) MovePlateSpawn(x + xInc, y + yInc, piece);  

    }

    public void PointMovePlate(int x, int y, string piece)
    {
        Game sc = controller.GetComponent<Game>();
        if (sc.PositionOnBoard(x,y))
        {
            // Check if there's a piece there, and create appropriate moveplate
            GameObject cp = sc.GetPosition(x,y);
            if (cp == null) MovePlateSpawn(x,y, piece);  
            else if (cp.GetComponent<Chessman>().player != player) MovePlateAttackSpawn(x,y, piece);
        }
    }

    public void PawnMovePlate(int x, int y)
    {
        Game sc = controller.GetComponent<Game>();
        if (sc.PositionOnBoard(x,y))
        {
            if (sc.GetPosition(x,y) == null) MovePlateSpawn(x,y, "white_pawn"); 

            if (sc.PositionOnBoard(x+1,y) && sc.GetPosition(x+1,y) != null && sc.GetPosition(x+1,y).GetComponent<Chessman>().player != player) 
            {
                MovePlateAttackSpawn(x + 1, y, "white_pawn");
            }

            if (sc.PositionOnBoard(x-1,y) && sc.GetPosition(x-1,y) != null && sc.GetPosition(x-1,y).GetComponent<Chessman>().player != player) 
            {
                MovePlateAttackSpawn(x - 1, y, "white_pawn");
            }
        }
    }


    // Helper function. Will see if the white king is in check after this move
    // Is used to prevent the creation of movePlates for illegal check moves
    public bool isLegalMove(int x, int y, string piece, bool isAttack)
    {   
        Game sc = controller.GetComponent<Game>(); // so we can use Game class functions

        if (!isAttack)
        {
            bool check_status = sc.IsWhiteInCheck(); // save so we can reset after testing moves // TODO need to save black check status as well?

            GameObject thiscp = sc.FindPiece(piece);  // save piece's original position for when we undo the move later

            int ogx = thiscp.GetComponent<Chessman>().GetXBoard(); 
            int ogy = thiscp.GetComponent<Chessman>().GetYBoard();

            // make the move
            thiscp.GetComponent<Chessman>().SetXBoard(x);
            thiscp.GetComponent<Chessman>().SetYBoard(y);
            sc.SetPosition(thiscp);
            sc.SetPositionEmpty(ogx,ogy);
            sc.Check();
            if (sc.IsWhiteInCheck()) 
            {
                // undo the move
                thiscp.GetComponent<Chessman>().SetXBoard(ogx);
                thiscp.GetComponent<Chessman>().SetYBoard(ogy);
                sc.SetPosition(thiscp);
                sc.SetPositionEmpty(x,y);

                return false;
            } 

            // undo move
            thiscp.GetComponent<Chessman>().SetXBoard(ogx);
            thiscp.GetComponent<Chessman>().SetYBoard(ogy);
            sc.SetPosition(thiscp);
            sc.SetPositionEmpty(x,y);

            return true;
            
        } else // attacking move
        {
            GameObject cp = sc.GetPosition(x, y); // piece we're 'capturing'
            cp.GetComponent<Chessman>().SetXBoard(8); // pretend like we captured
            cp.GetComponent<Chessman>().SetYBoard(8); // so move off board
            sc.SetPosition(cp); // update new position 
            sc.SetPositionEmpty(x, y); // set old position as empty

            // save piece's original position for when we undo the move later
            GameObject thiscp = sc.FindPiece(piece); // piece being moved
            int ogx = thiscp.GetComponent<Chessman>().GetXBoard(); 
            int ogy = thiscp.GetComponent<Chessman>().GetYBoard();
            // make the move
            thiscp.GetComponent<Chessman>().SetXBoard(x);
            thiscp.GetComponent<Chessman>().SetYBoard(y);
            sc.SetPosition(thiscp);
            sc.SetPositionEmpty(ogx,ogy);
            sc.Check(); // update, are we in check?
            if (sc.IsWhiteInCheck()) 
            {   
                // undo the move
                thiscp.GetComponent<Chessman>().SetXBoard(ogx);
                thiscp.GetComponent<Chessman>().SetYBoard(ogy);
                sc.SetPosition(thiscp);
                cp.GetComponent<Chessman>().SetXBoard(x);
                cp.GetComponent<Chessman>().SetYBoard(y);
                sc.SetPosition(cp);
                sc.SetPositionEmpty(8,8); // from when we moved the 'captured' piece off board

                return false;
            }
            // Now undo the move
            thiscp.GetComponent<Chessman>().SetXBoard(ogx);
            thiscp.GetComponent<Chessman>().SetYBoard(ogy);
            sc.SetPosition(thiscp);
            cp.GetComponent<Chessman>().SetXBoard(x);
            cp.GetComponent<Chessman>().SetYBoard(y);
            sc.SetPosition(cp);
            sc.SetPositionEmpty(8,8); // from when we moved the 'captured' piece off board

            return true; // legal move
        }

    }

    public void MovePlateSpawn(int matrixX, int matrixY, string piece)
    {
        if (isLegalMove(matrixX, matrixY, piece, false))
        {
            float x = matrixX;
            float y = matrixY;
            
            // found through trial and error
            x *= 1.25f;
            y *= 1.25f;

            x += -4.4f;
            y += -4.4f;

            GameObject mp = Instantiate(movePlate, new Vector3(x,y,-3.0f), Quaternion.identity); // for displaying on screen in unity
            MovePlate mpScript = mp.GetComponent<MovePlate>();
            mpScript.SetReference(gameObject);
            mpScript.SetCoords(matrixX,matrixY); // for us to keep track
        }
    }

    public void MovePlateAttackSpawn(int matrixX, int matrixY, string piece)
    {
        // First check if legal move - will White King be in check after this move
        if (isLegalMove(matrixX, matrixY, piece, true))
        {
            float x = matrixX;
            float y = matrixY;
            
            // found through trial and error
            x *= 1.25f;
            y *= 1.25f;

            x += -4.4f;
            y += -4.4f;

            GameObject mp = Instantiate(movePlate, new Vector3(x,y,-3.0f), Quaternion.identity); // for displaying on screen in unity
            MovePlate mpScript = mp.GetComponent<MovePlate>();
            mpScript.attack = true;
            mpScript.SetReference(gameObject);
            mpScript.SetCoords(matrixX,matrixY); // for us to keep track
        }
    }

   
}


