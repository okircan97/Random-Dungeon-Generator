using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// -------------------------------------------------------------------
// NOTE: The "RequireComponent" attribute automatically adds 
//       required components as dependencies.
// -------------------------------------------------------------------
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]

// This script is to reload the scene on hit (and trigger) to the ExitDoor. 
// It'll be attached to the ExitDoor game object (it's on prefabs).
public class ExitDoor : MonoBehaviour
{
    /////////////////////////////////////////
    //////////////// FIELDS /////////////////
    /////////////////////////////////////////

    // The box collider of the exit door.
    BoxCollider2D box;


    /////////////////////////////////////////
    ///////////////// RESET /////////////////
    /////////////////////////////////////////

    // ----------------------------------------------------------------------
    // NOTE: Reset is called when the user hits the Reset button in the 
    //       Inspector's context menu or when adding the component the 
    //       first time.
    // ----------------------------------------------------------------------
    void Reset() {
        // Make the rigidbody component kinematic.
        GetComponent<Rigidbody2D>().isKinematic = true; 
        // Get the collider.
        box = GetComponent<BoxCollider2D>();
        // Update the collider's size to 0.1f.
        box.size = Vector2.one * 0.1f;
        // Make collider a trigger.
        box.isTrigger = true;
    }


    /////////////////////////////////////////
    /////////////// COLLUSION ///////////////
    /////////////////////////////////////////

    // Reload the level on trigger (Since the dungeon is randomly generated,
    // it'll actually be a new level).
    private void OnTriggerEnter2D(Collider2D other) {
        if(other.tag == "Player"){
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
