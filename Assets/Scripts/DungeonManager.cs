using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// ----------------------------------------------------------------------
// NOTE: An enum is a special "class" that represents a group of constants
// ----------------------------------------------------------------------
public enum DungeonType {Caverns, Rooms, WindingHalls};

// This script is to create the dungeon. The floors, walls,
// exit door, items...
public class DungeonManager : MonoBehaviour
{
    /////////////////////////////////////////
    //////////////// FIELDS /////////////////
    /////////////////////////////////////////

    // With this DungeonType object, we'll choose the dungeon type that'll
    // be created.
    public DungeonType dungeonType;

    // The prefabs. DungeonMaker object will create the Tilespawner objects, 
    // which will use the wall and floor prefabs to create the dungeon.
    // Also, the DungeonMaker object will use the exitDoorPrefab.
    public GameObject floorPrefab, wallPrefab, tileSpawnerPrefab, exitDoorPrefab;

    // Items inside those game object lists will be instantiated randomly.
    public GameObject[] randomItems, randomEnemies;

    // Those game objects represent the items inside the randomItems and 
    // randomEnemies.
    GameObject goRandomItem, goRandomEnemy;

    // This list contains the rounded edges prefabs, in respect to their 
    // bit values. Check the PDF for more info.
    // NOTE: Their sorting layer is also wall. So, don't forget to make their
    //       layer equal to 1, so that they appear in front of the walls.
    public GameObject[] roundedEdges;

    // This game object represents the round edge prefabs inside the list above.
    GameObject goRoundedEdge;

    // Those ints are the spawn percentage of the random items and enemies.
    [Range(0, 100)] public int itemSpawnPercent, enemySpawnPercent;

    // Those values represents the boundries of the map.
    public float minX, minY, maxX, maxY;

    // This list will include the positions that's been passed with
    // the RandomWalker method. Later, the positions here will be
    // used by the TileSpawner to instantiate floors and walls.
    List<Vector3> floorList = new List<Vector3>();

    // This int represents the total floor value.
    [Range(100, 1500)] public int totalFloorValue;

    // A bool to check if the pos exists in the floor list.
    bool existsInFloorList;

    // Game objects, representing the TileSpawner and the ExitDoor.
    GameObject goTileSpawner, goExitDoor;

    // Two LayerMasks, one to check the floors, and one to check the walls.
    LayerMask floorMask, wallMask;

    // The size of the hitbox of the collider.
    Vector2 hitbox = Vector2.one * 0.8f;


    /////////////////////////////////////////
    ///////////// START & UPDATE ////////////
    /////////////////////////////////////////
    void Start() {
        // Initialize the floorMask and the wallMask.
        floorMask = LayerMask.GetMask("Floor");
        wallMask  = LayerMask.GetMask("Wall");
        // Generate the dungeon according to the dungeon type.
        switch(dungeonType){
            case DungeonType.Caverns:
                RandomWalker();
                break;
            case DungeonType.Rooms:
                RoomWalker();
                break;
            case DungeonType.WindingHalls:
                WindingWalker();
                break;
        }
    }

    void Update() {
        // ----------------------------------------------------------------------
        // NOTE: "Application.isEditor", Returns true if the game is being run 
        //       from the Unity editor; false if run from any deployment target.
        // ----------------------------------------------------------------------
        // It'll reload the current scene when backspace is clicked. It is for
        // debugging purposes.
        if(Application.isEditor && Input.GetKeyDown(KeyCode.Backspace)){
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }


    /////////////////////////////////////////
    //////////////// METHODS ////////////////
    /////////////////////////////////////////

    // This method will use the random walker algorithm. It's like a 
    // virtual entity walking around randomly. It'll create Vector3s
    // at random coordinates and they'll be used to create a dungeon
    // cavern.
    void RandomWalker(){
        // Create a Vector3 at 0,0,0; and add it to the floorList as the
        // first element.
        Vector3 currentPos = Vector3.zero;
        floorList.Add(currentPos);

        // While the number of floor tiles is smaller than the totalFloorValue,
        // generate a random position then move according to it.
        while(floorList.Count < totalFloorValue){
            // Get a new position to move.
            currentPos += generateDirection();
            // Check if this vector is visited before.
            existsInFloorList = CheckTheTiles(currentPos);
            // If not visited, add it to the floor list.
            if(!existsInFloorList){
                floorList.Add(currentPos);
            }
        }

        // Create the dungeon.
        StartCoroutine(WaitAndSetStuff());
    }


    // This method will use a modified random walker algorithm. 
    // It'll create Vector3s at random coordinates and they'll
    // be used to create hallways and rooms.
    void RoomWalker(){
        // Create a Vector3 at 0,0,0; and add it to the floorList as the
        // first element.
        Vector3 currentPos = Vector3.zero;
        floorList.Add(currentPos);

        // While the number of floor tiles is smaller than the totalFloorValue,
        // generate a random position then move according to it.
        while(floorList.Count < totalFloorValue){
            // Create a hallway.
            currentPos = CreateHallway(currentPos);
            // Create a room.
            CreateRoom(currentPos);
        }
        // Create the dungeon.
        StartCoroutine(WaitAndSetStuff()); 
    }


    // This method is basically the same with the RoomWalker. The only difference
    // is that, in this one, the rooms will be generated by 50 percent chance.
    void WindingWalker(){
        // Create a Vector3 at 0,0,0; and add it to the floorList as the
        // first element.
        Vector3 currentPos = Vector3.zero;
        floorList.Add(currentPos);

        // While the number of floor tiles is smaller than the totalFloorValue,
        // generate a random position then move according to it.
        while(floorList.Count < totalFloorValue){
            // Create a hallway.
            currentPos = CreateHallway(currentPos);
            // Create a room, by fifty chance.
            int roomRate = 50;
            int randomNumber = Random.Range(0,100);
            if(randomNumber <= roomRate){
                CreateRoom(currentPos);
            }
            
        }
        // Create the dungeon.
        StartCoroutine(WaitAndSetStuff()); 
    }



    // This method is to create hallways.
    Vector3 CreateHallway(Vector3 currentPos){
        // Walk direction.
        Vector3 walkDir = generateDirection();
        // Walk length (the length of the hallway).
        int walkLength = Random.Range(9, 18);

        // Create a random hallway.
        // Check the visited tiles for the hallway.
        for(int i = 0; i < walkLength; i++){
            // Check if this vector is visited before.
            existsInFloorList = CheckTheTiles(currentPos + walkDir);
            // If not visited, add it to the floor list.
            if(!existsInFloorList){
                floorList.Add(currentPos + walkDir);
            }
            currentPos += walkDir;
        }
        return currentPos;
    }



    // This method is to create rooms.
    void CreateRoom(Vector3 currentPos){
        // Create a random room to the end off the hall.
        int roomWidth  = Random.Range(1,5);
        int roomHeight = Random.Range(1,5);
            
        for(int w = -roomWidth; w <= roomWidth; w++){
            for(int h = -roomHeight; h <= roomHeight; h++){
                // A vector corresponding to the curretn x and y coordinates.
                Vector3 offSet = new Vector3(w,h,0);
                // Check if this vector is visited before.
                existsInFloorList = CheckTheTiles(currentPos + offSet);
                // If not visited, add it to the floor list.
                if(!existsInFloorList){
                    floorList.Add(currentPos + offSet);
                }
            }
        }
    }



    // This method is to generate a random direction.
    Vector3 generateDirection(){
        switch(Random.Range(1, 5)){
            case 1:
                return Vector3.up;
            case 2:
                return Vector3.right;
            case 3:
                return Vector3.down;
            case 4:
                return Vector3.left;
            }
            return Vector3.zero;
    }

    // This method is to check the current floor tiles and see if
    // the random walker passed them before.
    bool CheckTheTiles(Vector3 currentPos){
        bool existsInFloorList = false;
        // Check if the currentPos is already in the floorList.
        for(int i = 0; i < floorList.Count; i++){
            if(Vector3.Equals(currentPos, floorList[i])){
                existsInFloorList = true;
                break;
            }
        }
        return existsInFloorList;
    }


    // This coroutine is to wait for all the tilespawners to done  
    // and destroy themselves; then create necessary objects.
    IEnumerator WaitAndSetStuff(){

        // Instantiate TileSpawner objects to the positions inside the
        // list floorList.
        for(int i = 0; i < floorList.Count; i++){
            goTileSpawner = Instantiate(tileSpawnerPrefab, floorList[i], Quaternion.identity) as GameObject;
        }

        // Wait for the tilespawners to be done (They'll delete themselves
        // when they're done).
        while(FindObjectsOfType<TileSpawner>().Length > 0){
            yield return null;
        }

        // Set the door.
        SetExitDoor();

        // Set the random items and enemies.
        // ----------------------------------------------------------------------
        // NOTE: The instructor wanted to check the walls as well. That's why he
        //       added and extracted an extra 2 to the X and Y values. I do not
        //       think that it is necessary. Actually, an even better way would
        //       be using the floorList that we've created, instead of the 
        //       coordinates. I might update the code later.
        // ----------------------------------------------------------------------
        // In order to instantiate random objects, the coordinates
        // between minX - maxX, and minY - maxY will be checked.
        for(float x = minX - 2; x < maxX + 2; x++){
            for(float y = minY - 2; y < maxY + 2; y++){
                // Create a hit box that'll check for collusion with the floorMask,
                // will only check for the floor.
                Collider2D hitFloor = Physics2D.OverlapBox(new Vector2(x,y), hitbox, 0, floorMask);
                // If there's a floor:
                if(hitFloor){
                    // If the floor pos is not crashing with the exit door position:
                    if(!Vector2.Equals(hitFloor.transform, floorList[floorList.Count - 1])){
                        // Colliders to check the four side of the dungeonManager object.
                        Collider2D hitTop    = Physics2D.OverlapBox(new Vector2(x, y+1), hitbox, 0, wallMask);
                        Collider2D hitRight  = Physics2D.OverlapBox(new Vector2(x+1, y), hitbox, 0, wallMask);
                        Collider2D hitButtom = Physics2D.OverlapBox(new Vector2(x, y-1), hitbox, 0, wallMask);
                        Collider2D hitLeft   = Physics2D.OverlapBox(new Vector2(x-1, y), hitbox, 0, wallMask);
                        // If there's a wall in any of the four sides, and no walls opposite together,
                        // we MIGHT initiate a random object.
                        if((hitTop || hitRight || hitButtom || hitLeft) && !(hitTop && hitButtom) && !(hitRight && hitLeft)){
                            // Call SetRandomItem(Collider2D) to instantiate an item randomly.
                            SetRandomItem(hitFloor);
                        }
                        // If there're no walls around, we MIGHT initiate a random enemy.
                        if(!hitTop && !hitRight && !hitButtom && !hitLeft){
                            // Call SetRandomEnemy(Collider2D) to instantiate an enemy randomly.
                            SetRandomEnemy(hitFloor);
                        }
                    }
                }
                // Set the rounded edges aroung the walls.
                RoundEdges(x, y);
            }
        }
    }

    // This method is to create random enemies.
    void SetRandomEnemy(Collider2D hitFloor){
        // A random number between 1 and 100. If it's small or equal to the enemySpawnPercent,
        // an enemy will be instantiated.
        int randNum = Random.Range(1, 101);
        if(randNum <= enemySpawnPercent){
            // A random index number to choose an item from the list randomEnemies.
            int randIndex = Random.Range(0, randomEnemies.Length);
            goRandomEnemy = Instantiate(randomEnemies[randIndex], hitFloor.transform.position, Quaternion.identity) as GameObject;
            goRandomEnemy.name = randomEnemies[randIndex].name;
            // It'll be a child of the floor tile.
            goRandomEnemy.transform.SetParent(hitFloor.transform);
        }
    }

    // This method is to create random items.
    void SetRandomItem(Collider2D hitFloor){
        // A random number between 1 and 100. If it's small or equal to the itemSpawnPercent,
        // an item will be instantiated.
        int randNum = Random.Range(0, 101);
        if(randNum <= itemSpawnPercent){
            // A random index number to choose an item from the list randomItems.
            int randIndex = Random.Range(0, randomItems.Length);
            goRandomItem = Instantiate(randomItems[randIndex], hitFloor.transform.position, Quaternion.identity) as GameObject;
            goRandomItem.name = randomItems[randIndex].name;
            // It'll be a child of the floor tile.
            goRandomItem.transform.SetParent(hitFloor.transform);
        }
    }



    // This method is to create the exit door.
    void SetExitDoor(){
        // The door will be instantiated to the position of the last
        // element inside the floorList.
        Vector3 doorPos = floorList[floorList.Count - 1];

        // Initialize goExitDoor as a clone of the exitDoorPrefab and instantiate it.
        goExitDoor = Instantiate(exitDoorPrefab, doorPos, Quaternion.identity) as GameObject; 

        // Change goExitDoor's name with exitDoorPrefab's name, so that they're identical.
        goExitDoor.name = exitDoorPrefab.name;

        // Put goExitDoor inside where dungeonManager is located ("Environment" game object). 
        goExitDoor.transform.SetParent(transform);
    }

    // This method is to round the edges of the walls.
    void RoundEdges(float x, float y){
        // A collider to detect the walls.
        Collider2D hitWall = Physics2D.OverlapBox(new Vector2(x,y), hitbox, 0, wallMask);
        // If there's a wall colliding with the dungeonManager objcet:
        if(hitWall){
            // Colliders to check the four side of the dungeonManager object.
            Collider2D hitTop    = Physics2D.OverlapBox(new Vector2(x, y+1), hitbox, 0, wallMask);
            Collider2D hitRight  = Physics2D.OverlapBox(new Vector2(x+1, y), hitbox, 0, wallMask);
            Collider2D hitButtom = Physics2D.OverlapBox(new Vector2(x, y-1), hitbox, 0, wallMask);
            Collider2D hitLeft   = Physics2D.OverlapBox(new Vector2(x-1, y), hitbox, 0, wallMask);
            // ----------------------------------------------------------------------
            // NOTE: Four sides of a wall represents four different bits. 
            //       0        0        0        0
            //       left     button   right    up
            //       Floors are 1, walls are 0.
            // ----------------------------------------------------------------------
            // This int is to keep the track of the walls around the dungeonManager object.
            int bitValue = 0;
            if(!hitTop)    {bitValue += 1;}
            if(!hitRight)  {bitValue += 2;}
            if(!hitButtom) {bitValue += 4;}
            if(!hitLeft)   {bitValue += 8;}
            // It there are floor(s) around the wall, we'll instantiate the rounded edges
            // in respect to their bit value. In the tileset array, the rounded edges are 
            // in order according to the bit representing them. Since array starts from 0,
            // we've to substract 1 from the bitValue to access to the corresponding 
            // rounded edge.
            if(bitValue > 0){
                goRoundedEdge = Instantiate(roundedEdges[bitValue-1], new Vector2(x,y), Quaternion.identity) as GameObject;
                goRoundedEdge.name = roundedEdges[bitValue-1].name;
                // It'll be a child of the wall.
                goRoundedEdge.transform.SetParent(hitWall.transform); 
            }
        }
    }
}
