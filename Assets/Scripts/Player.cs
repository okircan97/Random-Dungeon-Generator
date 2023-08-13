using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // ----------------------------------------------------------------------
    // ---------------------------- FIELDS ----------------------------------
    // ----------------------------------------------------------------------

    // This var represents the transform component of the player object.
    // (which is inside its child: "Graphics"). It'll be used to turn the
    // player to the relevant position, according to the input.
    Transform graphics_trans;  

    // This var represents the initial localScale.x value of the object.
    float flipX;

    // This var represents the position that we wanted the player to move to.
    Vector2 targetPos;

    // The size of the hitbox of the collider.
    Vector2 hitbox = Vector2.one * 0.8f;

    // This var represents the layers that the player could collide with.
    LayerMask obstacles;

    // This bool is to check if the player is moving.
    bool isMoving;

    // This float represents the speed of the player.
    [SerializeField] float speed;



    // ----------------------------------------------------------------------
    // ----------------------- START & UPDATE -------------------------------
    // ----------------------------------------------------------------------
    void Start(){
        // GetComponentInChildren, makes a DFS in the object and returns a 
        // component (the first one encountered, obviously) with the desired type.
        graphics_trans = GetComponentInChildren<SpriteRenderer>().transform;

        // Initialize the flipX to the localScale.x of the object.
        flipX = graphics_trans.localScale.x;

        // Initialize the obstacles.
        obstacles = LayerMask.GetMask("Wall", "Enemy");
    }

    void Update(){
        Move();
    }



    // ----------------------------------------------------------------------
    // --------------------- FUNCTIONS & COROUTINES -------------------------
    // ----------------------------------------------------------------------

    // This method is to make the player move, using the coroutine: SmoothMove().
    void Move(){
        // ----------------------------------------------------------------------
        // NOTE: GetAxis is smoothed based on the “sensitivity” setting so that value 
        //       gradually changes from 0 to 1, or 0 to -1. Whereas GetAxisRaw will only 
        //       ever return 0, -1, or 1 exactly.
        // ----------------------------------------------------------------------
        // NOTE: System.Math is usually faster than Mathf even with the necessary double to float 
        //       conversion. So, if you don't mind putting (float) everywhere and you're looking  
        //       for the optimal performances go for Math. Then again if you're really after extreme  
        //       performances, some operations like Sin and Cos are better handled with home made 
        //       precomputed results.
        // ----------------------------------------------------------------------
        // NOTE: The reason why "System.Math.Sign" is used is to get the raw inputs for both the
        //       keyboard and the controller. If not, the controller will keep returning floating 
        //       values.
        // ----------------------------------------------------------------------
        // The following piece of code is to get the inputs of "Horizontal" and "Vertical".
        // Their key inputs can be checked from Unity -> Edit -> Project Settings -> Input Manager.
        float horizontal = System.Math.Sign(Input.GetAxisRaw("Horizontal"));
        float vertical   = System.Math.Sign(Input.GetAxisRaw("Vertical"));

        // If a direction button is clicked, hence the absolute value of horizontal or the vertical is
        // became greater than 0:
        if((Mathf.Abs(horizontal) > 0) || (Mathf.Abs(vertical) > 0)){
            // If the horizontal value is changed:
            // Turn the player object to the relevant position. Reason why I multiplicated 
            // horizontal with flipX, is to ensure that the flipping still works even if  
            // I decide to rescale the player's transform at any time.
            if(Mathf.Abs(horizontal) > 0){
                graphics_trans.localScale = new Vector2(flipX * horizontal, graphics_trans.localScale.y);
            }

            // If the player is not moving, get the target positions.
            if(!isMoving){
                if(Mathf.Abs(horizontal) > 0){
                    targetPos = new Vector2(transform.position.x + horizontal, 
                                            transform.position.y);
                }
                else if(Mathf.Abs(vertical) > 0){
                    targetPos = new Vector2(transform.position.x, 
                                            transform.position.y + vertical);
                }

                // ----------------------------------------------------------------------
                // NOTE: Physics2D.OverlapBox, checks if a Collider falls within a box area,
                //       and returns it.
                //       OverlapBox(Vector2 center of the box, 
                //                  Vector2 size of the box, 
                //                  float angle of the box,
                //                  ContactFilter2D contactFilter to filter the results (layers, z depth etc.)
                // ----------------------------------------------------------------------
                // The following piece of code is to handle the collusion.
                // ----------------------------------------------------------------------
                // "Collider2D hit" is the collider that the player hit.
                Collider2D hit = Physics2D.OverlapBox(targetPos, hitbox, 0, obstacles);
                // If the player doesn't hit any colliders, call the SmoothMove coroutine to move.
                if(!hit){
                    StartCoroutine(SmoothMove());
                }
            }

        }
    }
    


    // ----------------------------------------------------------------------
    // NOTE: Vector2.Distance(Vector2 a, Vector2 b) returns the distance between a and b.
    // ----------------------------------------------------------------------
    // NOTE: Vector2.MoveTowards(Vector2 current, Vector2 target, float maxDistanceDelta)
    //       Moves an object from "current", to the target. "maxDistanceDelta" can be thought
    //       as the speed. 
    // ----------------------------------------------------------------------
    // This coroutine will be used inside the Move() method.
    IEnumerator SmoothMove(){
        isMoving = true;
        // Move till the distance between the player and target position is smaller than 0.01f.
        while(Vector2.Distance(transform.position, targetPos) > 0.01f){
            transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }
        // Move the player to the exact position of the target position.
        transform.position = targetPos;
        isMoving = false;
    }
}
