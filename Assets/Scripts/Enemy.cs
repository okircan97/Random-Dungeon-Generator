using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{   
    /////////////////////////////////////////
    //////////////// FIELDS /////////////////
    /////////////////////////////////////////

    // Position of the mobs.
    Vector2 enemyPos;                       
    // Things that the mobs could collide with.
    LayerMask obstacleMask;           
    // Layer mask that the mobs use while following the player.      
    LayerMask walkableMask;                 
    // Coordinates that the enemies could move to.
    List<Vector2> availableMovements = new List<Vector2>();  
    // The size of the hitbox of the collider.
    Vector2 hitbox = Vector2.one * 0.8f;    
    // This float represents the speed of the enemies.
    [SerializeField] float speed;           
    // A bool to check whether the enemy is moving or not.
    bool isMoving;                          
    // Player instance.
    Player player;                          
    // The distance that the enemy will move till it notices the player.
    public float alertRange;                 
    List<Node> nodeList = new List<Node>();


    /////////////////////////////////////////
    ///////////// START & UPDATE ////////////
    /////////////////////////////////////////
    void Start()
    {   
        // Initialize the fields.
        player = FindObjectOfType<Player>();
        obstacleMask = LayerMask.GetMask("Wall", "Enemy", "Player");
        obstacleMask = LayerMask.GetMask("Wall", "Enemy");
        enemyPos = transform.position;
        // Call the HandleEnemybehavior(), which will decide
        // what the enemy should do.
        StartCoroutine(HandleEnemyBehavior());
    }

    // void Update()
    // {   
    //     // If the enemy is not currently moving, make it move.
    //     if(!isMoving){
    //         Patrol();
    //     }
    // }



    /////////////////////////////////////////
    //////////////// METHODS ////////////////
    /////////////////////////////////////////

    // This method is to make the enemy move, using the SmoothMove()
    // coroutine.
    void Patrol(){
        // Clear the list, availableMovements.
        availableMovements.Clear();

        // Collider(s) that the enemies could hit.
        Collider2D hitTop    = Physics2D.OverlapBox(enemyPos + Vector2.up, hitbox, 0, obstacleMask);
        Collider2D hitRight  = Physics2D.OverlapBox(enemyPos + Vector2.right, hitbox, 0, obstacleMask);
        Collider2D hitButtom = Physics2D.OverlapBox(enemyPos + Vector2.down, hitbox, 0, obstacleMask);
        Collider2D hitLeft   = Physics2D.OverlapBox(enemyPos + Vector2.left, hitbox, 0, obstacleMask);

        // Add the available positions to the availableMovements.
        if(!hitTop){availableMovements.Add(Vector2.up);}
        if(!hitRight){availableMovements.Add(Vector2.right); }
        if(!hitButtom){availableMovements.Add(Vector2.down);}
        if(!hitLeft){availableMovements.Add(Vector2.left);}

        // If there are available positions, get a random number and
        // move the mob accordingly.
        if(availableMovements.Count > 0){
            int randomNumber = Random.Range(0, availableMovements.Count);
            enemyPos += availableMovements[randomNumber];
        }

        // Move the enemy.
        StartCoroutine(SmoothMove());
    }


    // This coroutine is to make the enemy move from current position 
    // to a new position.
    IEnumerator SmoothMove(){
        isMoving = true;
        // Move till the distance between the enemy and target position is smaller than 0.01f.
        while(Vector2.Distance(transform.position, enemyPos) > 0.01f){
            transform.position = Vector2.MoveTowards(transform.position, enemyPos, speed * Time.deltaTime);
            yield return null;
        }
        
        // Move the enemy to the exact position of the target position.
        transform.position = enemyPos;
        
        // Before starting to move again, wait for some time.
        yield return new WaitForSeconds(Random.Range(1,5));
        isMoving = false;
    }


    // This method is to handle the behavior of the enemy.
    // The enemy could patrol, attack or follow the player.
    IEnumerator HandleEnemyBehavior(){
        while(true){
            yield return new WaitForSeconds(0.1f);
            if(!isMoving){
                // The distance between the enemy and the player.
                float distance = Vector2.Distance(transform.position, player.transform.position);
                // If the distance between the enemy and the player is smaller
                // than the "alertRange", do one of the following:
                if(distance <= alertRange){
                    // If the distance between them is smaller than 1.1f,
                    // attack the player.
                    if(distance <= 1.1f){
                        Attack();
                        yield return new WaitForSeconds(0.5f);
                    }
                    // If not, follow the player.
                    // First, find where to move next.
                    Vector2 newPos = FindNextStep(transform.position, player.transform.position);
                    // If the enemy is not where it should be, it should move.
                    if(newPos != enemyPos){
                        enemyPos = newPos;
                        StartCoroutine(SmoothMove());
                    }
                    // Else, keep patrolling.
                    else{
                        Patrol();
                    }
                }
                // If the distance is bigger than the alert range, keep patrolling.
                Patrol();
            }
        }
    }

    // ----------------------------------------------------------
    // NOTE: Pathfinding algorithm is not working as it should
    //       and I don't like how the instructor handles it
    //       either way. I didn't follow the 24th lesson carefully. 
    //       If I decide to use this, it would make sense to use
    //       a* or djikstra.
    // ----------------------------------------------------------
    // This method is to find where the enemy should move next.
    // First param is the enemy position, and the second one
    // is the player's position.
    Vector2 FindNextStep(Vector2 enemyPos, Vector2 playerPos){
        // The current index, it's initially 0.
        int currentIndex = 0;
        // The current position of the enemy.
        Vector2 currentPos = enemyPos;
        // Clear the nodes list.
        nodeList.Clear();
        // Create a new node at the current pos (it's the parent node).
        Node node = new Node(enemyPos, enemyPos);
        // Add the node to the node list.
        nodeList.Add(node);
        // Till the enemy reaches the player and the current node index is
        // smaller than 1000 and there are still available nodes:
        while(currentPos != playerPos && currentIndex < 1000 && nodeList.Count > 0){
            // Check the adjacent nodes and add reachable ones to the list.
            CheckAdjNode(currentPos + Vector2.up,    currentPos);
            CheckAdjNode(currentPos + Vector2.right, currentPos);
            CheckAdjNode(currentPos + Vector2.down,  currentPos);
            CheckAdjNode(currentPos + Vector2.left,  currentPos);

            // Get to the next node.
            currentIndex++;
            if(currentIndex < nodeList.Count){
                currentPos = nodeList[currentIndex].position;
            }
        }
        // If the enemy reaches the player:
        if(currentPos == playerPos){
            // Reverse the nodes list.
            nodeList.Reverse();
            for(int i = 0; i < nodeList.Count; i++){
                // If where the enemy is, is the node list's ith element:
                if(currentPos == nodeList[i].position){
                    // If the parent of the node is the starting position, 
                    // enemy's first position, return the current position.
                    if(nodeList[i].parent == enemyPos){
                        return currentPos;
                    }
                    // If not, keep going back on the list (tree like structure).
                    currentPos = nodeList[i].parent;
                }
                // If not, go back to the parent and keep checking.
            }
        }
        // Return the new enemy position to move. 
        return enemyPos;
    }



    // This method is to check a node adjacent to the enemy.
    void CheckAdjNode(Vector2 position, Vector2 parent){
        // Collider to check if there's anything that the enemy could collide with.
        Collider2D hit = Physics2D.OverlapBox(position,hitbox,0,walkableMask);
        // If there's not, add that adj node to the node list.
        if(!hit){
            nodeList.Add(new Node(position, parent));
        }
    }



    // This method is to make the enemy attack.
    void Attack(){
        int roll = Random.Range(0, 100);
        if(roll > 50){
            int damage = Random.Range(0,100);
            Debug.Log("Enemy hit" + damage + "to the player!");
        }
        else{
            Debug.Log("The enemy missed!");
        }
    }
}
