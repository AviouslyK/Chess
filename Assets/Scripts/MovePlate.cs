using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlate : MonoBehaviour
{
    public GameObject controller;

    // Move plates need to have reference back to piece that wants to move
    GameObject reference = null; 

    // Board positions
    int matrixX;
    int matrixY;

    // false: empty space, true: attacking enemy piece
    public bool attack = false;

    // activates when MovePlate is created
    public void Start() 
    {   
        if (attack)
        {
            // change color to red
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.0f, 0.0f, 1.0f); // (r,g,b,alpha)
        }
    }

    // activates when you tap the MovePlate
    public void OnMouseUp()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");

        if (attack) // must get rid of existing piece
        {
            // get piece at moveplate's position
            GameObject cp = controller.GetComponent<Game>().GetPosition(matrixX, matrixY);

            // if destroying King, end game
            if (cp.name == "white_king") controller.GetComponent<Game>().Winner("black");
            if (cp.name == "black_king") controller.GetComponent<Game>().Winner("white");

            //Destroy(cp);
            // instead of destroying the piece, move it off the board and shrink it
            string col = cp.GetComponent<Chessman>().GetPlayer();
            if (col == "black")
            {
                if (controller.GetComponent<Game>().GetCaptured(col) < 4)
                {
                    cp.GetComponent<Chessman>().SetXBoard(8);
                    cp.GetComponent<Chessman>().SetYBoard(0 + controller.GetComponent<Game>().GetCaptured(col));
                }
                else 
                {
                    cp.GetComponent<Chessman>().SetXBoard(9);
                    cp.GetComponent<Chessman>().SetYBoard(controller.GetComponent<Game>().GetCaptured(col) - 4);
                }
            }
                cp.GetComponent<Chessman>().SetCoords();
                cp.GetComponent<Chessman>().GetComponent<Transform>().localScale = new Vector3(1.5f,1.5f,1.0f);
                controller.GetComponent<Game>().CaptureTally(col);
            
        }

        // If you're moving the king, you can no longer castle
        if (reference.GetComponent<Chessman>().name.Contains("king")) 
        {
            controller.GetComponent<Game>().whiteHasCastledL = true;
            controller.GetComponent<Game>().whiteHasCastledR = true;
        }

        // If you're moving the left rook, you can no longer castle left
        if (reference.GetComponent<Chessman>().name.Contains("rook") && reference.GetComponent<Chessman>().GetXBoard() == 0) 
        {
            controller.GetComponent<Game>().whiteHasCastledL = true;
        }

        // If you're moving the right rook, you can no longer castle right
        if (reference.GetComponent<Chessman>().name.Contains("rook") && reference.GetComponent<Chessman>().GetXBoard() == 7) 
        {
            controller.GetComponent<Game>().whiteHasCastledR = true;
        }

        // if castling right, also move the rook
        if (reference.GetComponent<Chessman>().name.Contains("king") && controller.GetComponent<Game>().whiteCastleRightOK && matrixX == 6 && matrixY == 0)
        {
            GameObject cp = controller.GetComponent<Game>().GetPosition(matrixX+1, matrixY); // get rook
            controller.GetComponent<Game>().SetPositionEmpty(cp.GetComponent<Chessman>().GetXBoard(), cp.GetComponent<Chessman>().GetYBoard()); // set rooks original position to empty
            cp.GetComponent<Chessman>().SetXBoard(matrixX-1); // move rook
            cp.GetComponent<Chessman>().SetCoords(); // move rook visually, in game
            controller.GetComponent<Game>().SetPosition(cp); // update rook's global position
            controller.GetComponent<Game>().whiteHasCastledL= true; // don't let white castle again
            controller.GetComponent<Game>().whiteHasCastledR= true; // don't let white castle again
        }
        // Castling Left
        if (reference.GetComponent<Chessman>().name.Contains("king") && controller.GetComponent<Game>().whiteCastleLeftOK && matrixX == 2 && matrixY == 0)
        {
            GameObject cp = controller.GetComponent<Game>().GetPosition(matrixX-2, matrixY);
            controller.GetComponent<Game>().SetPositionEmpty(cp.GetComponent<Chessman>().GetXBoard(), cp.GetComponent<Chessman>().GetYBoard()); // set rooks original position to empty
            cp.GetComponent<Chessman>().SetXBoard(matrixX+1);
            cp.GetComponent<Chessman>().SetCoords();
            controller.GetComponent<Game>().SetPosition(cp); // update rook's global position
            controller.GetComponent<Game>().whiteHasCastledL = true;
            controller.GetComponent<Game>().whiteHasCastledR= true; // don't let white castle again
        }
        
        // set original location to empty
        controller.GetComponent<Game>().SetPositionEmpty(reference.GetComponent<Chessman>().GetXBoard(), reference.GetComponent<Chessman>().GetYBoard());

        // update reference now that piece is moving
        reference.GetComponent<Chessman>().SetXBoard(matrixX);
        reference.GetComponent<Chessman>().SetYBoard(matrixY);
        reference.GetComponent<Chessman>().SetCoords(); // update global coords based on new board position

        // keep controller in sync with reference
        controller.GetComponent<Game>().SetPosition(reference);

        // start next turn
        controller.GetComponent<Game>().NextTurn("black");

        // finally, since the piece has moved - destroy the movePlates
        reference.GetComponent<Chessman>().DestroyMovePlates();
    }

    public void SetCoords(int x, int y)
    {
        matrixX = x;
        matrixY = y;
    }

    public void SetReference(GameObject obj) { reference = obj; }

    // so controller or chesspiece can get reference connected to a moveplate
    public GameObject GetReference() { return reference; }

}
