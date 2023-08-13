using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is to generate the floors and walls.
public class TileSpawner : MonoBehaviour
{
    /////////////////////////////////////////
    //////////////// FIELDS /////////////////
    /////////////////////////////////////////

    // A dungeon manager object instance.
    DungeonManager dungeonManager;  

    // This game object represents the floor tiles.
    GameObject goFloor;      

    // This game object represents the walls.
    GameObject goWall;      

    // This var is for the environment objects that we'll check.
    LayerMask environmentMask;

    // This vector represents the position that we'll check for floors.       
    Vector2 targetPos;     

    // The size of the hitbox of the collider.
    Vector2 hitbox = Vector2.one * 0.8f;



    /////////////////////////////////////////
    ///////////// START & UPDATE ////////////
    /////////////////////////////////////////

    // On awake, instantiate the floors.
    void Awake() {
        // Initialize the dungeonManager.
        dungeonManager = FindObjectOfType<DungeonManager>();

        // Initialize goFloor as a clone of the floorPrefab and instantiate it.
        goFloor = Instantiate(dungeonManager.floorPrefab, transform.position, Quaternion.identity) as GameObject; 

        // Change goFloor's name with dungeonManager.floorPrefab's name, so that they're identical.
        goFloor.name = dungeonManager.floorPrefab.name;

        // Set dungeonManager as goFloor's parent. 
        goFloor.transform.SetParent(dungeonManager.transform);

        // Update the minX, minY, maxX and maxY values according to the
        // newly instantiated floor.
        if(transform.position.x < dungeonManager.minX){
            dungeonManager.minX = transform.position.x;
        }
        if(transform.position.y < dungeonManager.minY){
            dungeonManager.minY = transform.position.y;
        }
        if(transform.position.x > dungeonManager.maxX){
            dungeonManager.maxX = transform.position.x;
        }
        if(transform.position.y > dungeonManager.maxY){
            dungeonManager.maxY = transform.position.y;
        }
    }


    // On start, instantiate the walls.
    void Start()
    {
        // Initialize the layer mask to the layer's that we'll check.
        environmentMask = LayerMask.GetMask("Wall", "Floor");

        // This nested loop is to check the various positions for walls and floors.
        for(int x = -1; x <= 1; x++ ){
            for(int y = -1; y <= 1; y++){
                // Update the targetPos according to the x and y values. 
                targetPos = new Vector2(transform.position.x + x, transform.position.y + y);

                // "Collider2D hit" is the collider(s) that the tilespawner hit.
                Collider2D hit = Physics2D.OverlapBox(targetPos, hitbox, 0, environmentMask);

                // If there's nothing tilespawner hit, initialize a wall to the targetPos.
                if(!hit){
                    // Initialize goWall as a clone of the wallPrefab and instantiate it.
                    goWall = Instantiate(dungeonManager.wallPrefab, targetPos, Quaternion.identity) as GameObject; 

                    // Change goWall's name with dungeonManager.floorPrefab's name, so that they're identical.
                    goWall.name = dungeonManager.wallPrefab.name;

                    // Set dungeonManager as goWall's parent.
                    goWall.transform.SetParent(dungeonManager.transform);
                }
            }
        }

        // Destroy the game object this script is attached to.
        Destroy(gameObject);
    }


    /////////////////////////////////////////
    //////////////// METHODS ////////////////
    /////////////////////////////////////////

    // Gizmos will be used to visualize the game level. It'll basically create
    // a white box where the TileSpawner object is. It serves no real purpose.
    void OnDrawGizmos() {
        Gizmos.color = Color.white;
        Gizmos.DrawCube(transform.position, Vector3.one);
    }
}
