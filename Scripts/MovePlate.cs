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

        // set original location to empty
        controller.GetComponent<Game>().SetPositionEmpty(reference.GetComponent<Chessman>().GetXBoard(), reference.GetComponent<Chessman>().GetYBoard());

        // update reference now that piece is moving
        reference.GetComponent<Chessman>().SetXBoard(matrixX);
        reference.GetComponent<Chessman>().SetYBoard(matrixY);
        reference.GetComponent<Chessman>().SetCoords(); // update global coords based on new board position

        // keep controller in sync with reference
        controller.GetComponent<Game>().SetPosition(reference);

        // start next turn
        controller.GetComponent<Game>().NextTurn();

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
