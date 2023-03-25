using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public class GameController : MonoBehaviour
{
    [System.NonSerialized]
    public float maxLineLength = 15;
    [System.NonSerialized]
    public float maxSimLength = 300;
    [System.NonSerialized]
    public int nodeAttempts = 100;
    public float simWidth = 33;
    [System.NonSerialized]
    public float simHeight = 15;
    [System.NonSerialized]
    public float minNodeDist = 8.0f;
    public int numPlayers = 5;

    public GameObject BoxPrefab;
    [System.NonSerialized]
    public List<GameObject> Boxes;
    public GameObject scoreBoard;
    public GameObject nodePrefab;
    public GameObject nodeFolder;
    public GameObject bugPrefab;
    public GameObject bugFolder;
    public GameObject linePrefab;
    public GameObject lineFolder;
    private List<GameObject> lines;
    public float nodeRadius = 1.0f;
    [System.NonSerialized]
    public Dictionary<int, List<int>> connections;
    [System.NonSerialized]
    public List<NodeController> nodes;

    public GameObject timer1;
    public GameObject timer2;
    private float timer1last;
    private float timerctr1;
    private float timerctr2;
    
    private float lastStatChange = 0;
    private float statChangeTime = 30.0f;

    public float gameSpeed = 1.0f;

    [System.NonSerialized]
    public float resetTime = 0;
    private bool over = false;

    public float simScale = 1.0f;

    private List<int> wins;

    public Stats stats = new Stats();
    public StatsController StatsDisplay;
    public GameObject camera;

    [System.NonSerialized]
    public PlayerType playerRace = PlayerType.Unowned;
    [System.NonSerialized]
    public int selected = -1;

    void DrawLine(Vector3 start, Vector3 end)
    {
        GameObject myLine = Instantiate(linePrefab, new Vector3(0,0,0), Quaternion.identity);
        myLine.transform.parent = lineFolder.transform;
        myLine.transform.position = start;
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        //lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
        //Color color = new Color(0.13f, 0.13f, 0.13f);
        Color color = new Color(0.87f, 0.87f, 0.87f);
        lr.SetColors(color, color);
        lr.SetWidth(0.2f, 0.2f);
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        lines.Add(myLine);
        //GameObject.Destroy(myLine, duration);
    }

    bool checkIntersections(int n1, int n2){
        int k = 0;
        for(int i = 0; i < nodes.Count; i += 1){
            foreach(int j in connections[i]){
                if(j < i) continue;
                if(i == n1 || i == n2 || j == n1 || j == n2) continue;
                Vector3 start1 = nodes[n1].transform.position;
                Vector3 end1 = nodes[n2].transform.position;

                Vector3 start2 = nodes[i].transform.position;
                Vector3 end2 = nodes[j].transform.position;

                if(doIntersect(start1, end1, start2, end2)){
                    return true;
                }
            }
        }
        return false;
        //Debug.Log(k.ToString());
    }

    void CreateGraph(){
        nodes = new List<NodeController>();
        connections = new Dictionary<int, List<int>>();
        lines = new List<GameObject>();

        for(int i = 0; i < nodeAttempts; i += 1){
            float x = UnityEngine.Random.Range(-simWidth, simWidth);
            float y = UnityEngine.Random.Range(-simHeight, simHeight);
            
            Vector3 newPos = new Vector3(x, y, 0);

            bool nearby = false;
            foreach(NodeController n in nodes){
                float dist = Vector3.Distance(n.transform.position, newPos);
                if(dist < minNodeDist) nearby = true;
            } 
            if(!nearby){
                GameObject newNode = Instantiate(nodePrefab, newPos, Quaternion.identity);
                NodeController newNodeCon = newNode.GetComponent<NodeController>();
                newNodeCon.parent = this;

                newNode.transform.parent = nodeFolder.transform;
                newNode.transform.localScale = new Vector3(1f,1f,1f);

                nodes.Add(newNodeCon);
                connections.Add(nodes.Count - 1, new List<int>());
            }
        }


        for(int i = 0; i < nodes.Count; i += 1){
            NodeController n1 = nodes[i];

            for(int j = 0; j < nodes.Count; j += 1){
                NodeController n2 = nodes[j];

                if(j == i) continue;

                float dist = Vector3.Distance(n1.transform.position, n2.transform.position);
                if(dist < maxLineLength && !connections[i].Contains(j) && !checkIntersections(i, j)){
                    connections[i].Add(j);
                    connections[j].Add(i);
                    DrawLine(n1.transform.position, n2.transform.position);
                }
            } 
        }

        //Prim Jarnik
        int V = nodes.Count;
        int[] parentNode = new int[V];
        float[] distanceTo = new float[V];
        bool[] inSpanningTree = new bool[V];
        for (int i = 0; i < V; i++) {
            distanceTo[i] = int.MaxValue;
            inSpanningTree[i] = false;
        }

        distanceTo[0] = 0;
        parentNode[0] = -1;

        for (int count = 0; count < V - 1; count++) {
            float min = int.MaxValue;
            int closestNode = -1;
 
            for (int v = 0; v < V; v++){
                if (inSpanningTree[v] == false && distanceTo[v] < min) {
                    min = distanceTo[v];
                    closestNode = v;
                }
            }
                
            inSpanningTree[closestNode] = true;

            for (int v = 0; v < V; v++){
                float dist = Vector3.Distance(nodes[v].transform.position, nodes[closestNode].transform.position);
                if (inSpanningTree[v] == false && dist < distanceTo[v]) {
                    parentNode[v] = closestNode;
                    distanceTo[v] = dist;
                }
            }
        }

        for (int i = 1; i < V; i++) {
            if(!connections[i].Contains(parentNode[i]) && !checkIntersections(i, parentNode[i])){
                connections[i].Add(parentNode[i]);
                connections[parentNode[i]].Add(i);
                DrawLine(nodes[i].transform.position, nodes[parentNode[i]].transform.position);
            }
        }
    }

    void SetScoreBoard(bool setup){
        if(setup){
            for(int i = 0; i < stats.GetNum(); i += 1){
                GameObject newBox = Instantiate(BoxPrefab, new Vector3(0.0f, 0.0f, 0), Quaternion.identity);
                newBox.transform.parent = scoreBoard.transform;
                Boxes.Add(newBox);
            }
        }

         for(int i = 0; i < stats.GetNum(); i += 1){
                GameObject newBox = Boxes[i];
                float bw = Screen.width / ((float) stats.GetNum());
                newBox.GetComponent<RectTransform>().sizeDelta = new Vector2(bw, bw * 96f / 467f);
                newBox.GetComponent<RectTransform>().anchoredPosition = new Vector2((i * bw), 0.0f);
        
                Image sr = newBox.GetComponent<Image>();
                PlayerType pt = (PlayerType) i;
                sr.color = stats.GetColor(pt);

                Text tm = newBox.transform.GetChild(0).GetComponent<Text>();
                tm.color = stats.GetFontColor(pt);
            }

        for(int i = 0; i < stats.GetNum(); i += 1){
            Text t = Boxes[i].transform.GetChild(0).GetComponent<Text>();
            
            if(i == stats.GetNum() - 1){
                t.text = (wins[i].ToString().PadRight(2) + " Draws").Replace (' ', '\u00A0');;
            }else{
                t.text = (wins[i].ToString().PadRight(2) + " " + ((PlayerType) i).ToString()).Replace (' ', '\u00A0');;
            }
        }
    }

    void Start()
    {
        Boxes = new List<GameObject>();
        wins = new List<int>();
        for(int i = 0; i < stats.GetNum(); i += 1){
            wins.Add(0);
        }
        StatsDisplay.parent = this;
        startSim();

        SetScoreBoard(true);
        StatsDisplay.ChooseAndUpdate();
        timer1.GetComponent<Text>().color = new Color(0.87f, 0.87f, 0.87f);
    }

    void startSim(){
        simScale = UnityEngine.Random.Range(0.7f, 1.2f);
        simWidth = simScale * 38;
        simHeight = simScale * 18;
        nodeAttempts = (int) (100 * simScale * simScale);
        camera.transform.position = new Vector3(2.6f * simWidth / 38f, -0.7f * simWidth / 38f, -10);
        camera.GetComponent<Camera>().orthographicSize = 25.5f * simScale;


        CreateGraph();

        for(int i = 0; i < nodes.Count; i += 1){
            NodeController n1 = nodes[i];
            n1.SetOwner(PlayerType.Unowned);
            n1.SetPopulation(UnityEngine.Random.Range(3, 5));
            n1.spawnTime = UnityEngine.Random.Range(6f, 9f); 
            n1.TargetNode = -1; 
            n1.id = i;
        }

        List<PlayerType> added = new List<PlayerType>();
        int attempts = 0;
        while(added.Count < numPlayers && attempts < 50){
            attempts += 1;
            int r = UnityEngine.Random.Range(0, nodes.Count);
            if(nodes[r].owner != PlayerType.Unowned) continue;
    
            PlayerType newType = (PlayerType) UnityEngine.Random.Range(0, stats.GetNum()-1);
            if(added.Contains(newType)) continue;
            nodes[r].SetOwner(newType);
            nodes[r].SetPopulation((int) (UnityEngine.Random.Range(25, 30) * simScale));
            //nodes[r].spawnTime = Random.Range(1.0f, 1.5f);
            //nodes[r].attackTime = 0.5f;

            if(connections[r].Count > 0) nodes[r].TargetNode = connections[r][UnityEngine.Random.Range(0, connections[r].Count)];

            added.Add(newType);
        }

        setFrontiers();
        StatsDisplay.ChooseAndUpdate();
    }

    void resetSim(){
        Destroy(lineFolder);
        Destroy(bugFolder);
        Destroy(nodeFolder);

        lineFolder = new GameObject();
        lineFolder.name = "Lines";
        bugFolder = new GameObject();
        bugFolder.name = "Bugs";
        nodeFolder = new GameObject();
        nodeFolder.name = "Nodes";

        playerRace = PlayerType.Unowned;

        startSim();
    }

    public void setFrontiers(){
        foreach(NodeController n in nodes){
            n.frontier = -1;
        }

        for(int i = 0; i < nodes.Count; i += 1){
            if(connections[i].Count == 0) continue;
            foreach(int j in connections[i]){
                if(nodes[i].owner != nodes[j].owner){
                    nodes[i].frontier = 0;
                    nodes[j].frontier = 0;
                }
            }
        }

        for(int k = 0; k < 10; k += 1){
            for(int i = 0; i < nodes.Count; i += 1){
                if(nodes[i].frontier != -1) continue;
                foreach(int j in connections[i]){
                    if(nodes[i].frontier != -1 && nodes[i].frontier <= nodes[j].frontier) continue;
                    if(nodes[j].frontier != -1) nodes[i].frontier = nodes[j].frontier + 1;
                }
            }
        }
    }

    void Update(){
        timerctr2 += Time.deltaTime * gameSpeed;
        timer2.GetComponent<Text>().text = "UnFixed Time: " + ((int)timerctr1).ToString();
    }

    void FixedUpdate()
    {
        timerctr1 += Time.deltaTime;// * gameSpeed;
        timer1.GetComponent<Text>().text = "Time: " + ((int)timerctr2).ToString();
        //SetScoreBoard(false);
        

        lastStatChange += Time.deltaTime * gameSpeed;
        if(lastStatChange > statChangeTime){
            lastStatChange -= statChangeTime;
            StatsDisplay.ChooseAndUpdate();
        }

        bool same = true;
        bool canReach = false;
        PlayerType first = PlayerType.Unowned;
        foreach(NodeController n in nodes){
            if(first == PlayerType.Unowned && n.owner != PlayerType.Unowned) first = n.owner;
            if(n.owner != first && n.owner != PlayerType.Unowned) same = false;
            if(n.frontier != -1) canReach = true;
        }

        if((same || !canReach) && !over) {
            over = true;
            resetTime = (maxSimLength * simScale) - 3;
            StatsDisplay.ChooseAndUpdate();
            lastStatChange = -3.0f;
        }

        resetTime += Time.deltaTime * gameSpeed;
        if(resetTime > maxSimLength * simScale){
            timerctr1 = 0;
            timerctr2 = 0;
            if(same){
                wins[(int) first] += 1;
            }else{
                wins[stats.GetNum()-1] += 1;
            }
            for(int i = 0; i < nodes.Count; i += 1){
                nodes[i].SetTarget();
                nodes[i].lastAttack = 0;
            }
            SetScoreBoard(false);


            resetTime -= maxSimLength * simScale;
            resetSim();
            
            over = false;
        }
    }

    bool onSegment(Vector3 p, Vector3 q, Vector3 r)
    {
        if (q.x < Mathf.Max(p.x, r.x) && q.x > Mathf.Min(p.x, r.x) &&
            q.y < Mathf.Max(p.y, r.y) && q.y > Mathf.Min(p.y, r.y))
        return true;
    
        return false;
    }
    
    // To find orientation of ordered triplet (p, q, r).
    // The function returns following values
    // 0 --> p, q and r are collinear
    // 1 --> Clockwise
    // 2 --> Counterclockwise
    int orientation(Vector3 p, Vector3 q, Vector3 r)
    {
        // See https://www.geeksforgeeks.org/orientation-3-ordered-points/
        // for details of below formula.
        float val = (q.y - p.y) * (r.x - q.x) -
                (q.x - p.x) * (r.y - q.y);
    
        if (val == 0) return 0;  // collinear
    
        return (val > 0)? 1: 2; // clock or counterclock wise
    }
    
    // The main function that returns true if line segment 'p1q1'
    // and 'p2q2' intersect.
    bool doIntersect(Vector3 p1, Vector3 q1, Vector3 p2, Vector3 q2)
    {
        // Find the four orientations needed for general and
        // special cases
        int o1 = orientation(p1, q1, p2);
        int o2 = orientation(p1, q1, q2);
        int o3 = orientation(p2, q2, p1);
        int o4 = orientation(p2, q2, q1);
    
        // General case
        if (o1 != o2 && o3 != o4)
            return true;
    
        // Special Cases
        // p1, q1 and p2 are collinear and p2 lies on segment p1q1
        if (o1 == 0 && onSegment(p1, p2, q1)) return true;
    
        // p1, q1 and q2 are collinear and q2 lies on segment p1q1
        if (o2 == 0 && onSegment(p1, q2, q1)) return true;
    
        // p2, q2 and p1 are collinear and p1 lies on segment p2q2
        if (o3 == 0 && onSegment(p2, p1, q2)) return true;
    
        // p2, q2 and q1 are collinear and q1 lies on segment p2q2
        if (o4 == 0 && onSegment(p2, q1, q2)) return true;
    
        return false; // Doesn't fall in any of the above cases
    }
}


public enum PlayerType
{
    Bees,
    //Wasps,
    Ants,
    Termites,
    Beetles,
    Mantises,
    Roaches,
    Crickets,
    //Centipedes,
    Spiders,
    //Fleas,
    //Ticks,
    //Flies,
    Unowned
}


public class Stats
{    
    List<int> moveStats = new List<int>();
    List<int> spawnStats = new List<int>();
    List<int> attackStats = new List<int>();
    List<int> defenseStats = new List<int>();
    List<Color> teamColors = new List<Color>();

    List<int> statShuffle = new List<int>();
    public Stats()
    {
        
        AddTeam(3,3,4,2,new Color32(255, 215, 0, 255)); //Add Bees
        //AddTeam(2,2,4,4,new Color32(220, 20, 60, 255)); //Add Wasps
        AddTeam(3,5,1,3,new Color32(71, 155, 120, 255)); //Add Ants
        AddTeam(4,5,2,1,new Color32(30, 144, 255, 255)); //Add Termites
        AddTeam(1,3,3,5,new Color32(139, 0, 139, 255)); //Add Beetles
        AddTeam(3,1,5,3,new Color32(255, 105, 0, 255)); //Add Mantises
        AddTeam(3,4,1,4,new Color32(192, 192, 192, 255)); //Add Roaches
        AddTeam(5,3,3,1,new Color32(0, 180, 20, 255)); //Add Crickets
        //AddTeam(5,1,5,1,new Color32(178, 34, 34, 255)); //Add Centipedes
        AddTeam(1,2,5,4,new Color32(50, 50, 50, 255)); //Add Spiders
        //AddTeam(5,5,1,1,new Color32(255, 105, 180, 255)); //Add Fleas
        //AddTeam(1,5,5,1,new Color32(240, 128, 128, 255)); //Add Ticks
        //AddTeam(4,3,3,2,new Color32(40, 40, 40, 255)); //Add Flies
        //R.Shuffle(statShuffle);
        //System.Random rng = new System.Random();
        //statShuffle = statShuffle.OrderBy(a => rng.Next()).ToList();
        //for(int i = 0; i < statShuffle.Count; i += 1){
        //    Debug.Log(statShuffle[i]);
        //}
        //teamColors = teamColors.OrderBy(a => rng.Next()).ToList();
        AddTeam(1,1,1,1,new Color32(105, 105, 105, 255)); //Add Unowned
    }

    void AddTeam(int move, int spawn, int attack, int defense, Color c){
        moveStats.Add(move);
        spawnStats.Add(spawn);
        attackStats.Add(attack);
        defenseStats.Add(defense);
        teamColors.Add(c);
        statShuffle.Add(statShuffle.Count);
    }

    public int GetNum(){
        return moveStats.Count;
    }

    public Color GetFontColor(PlayerType p){
        p = (PlayerType) statShuffle[(int) p];
        //if(p == PlayerType.Wasps || p == PlayerType.Beetles || p == PlayerType.Spiders || p == PlayerType.Flies){
        if(p == PlayerType.Beetles || p == PlayerType.Spiders){
            return new Color(0.96f, 0.96f, 0.96f, 1.0f);
        //}else if(p == PlayerType.Centipedes){
        //    return new Color32(255, 215, 0, 255);
        }else{
            return new Color(0.13f, 0.13f, 0.13f, 1.0f);
        }
    }

    public Color GetColor(PlayerType p){
        p = (PlayerType) statShuffle[(int) p];
        return teamColors[(int) p];
    }

    public int GetMoveSpeed(PlayerType p){
        p = (PlayerType) statShuffle[(int) p];
        return moveStats[(int) p];
    }

    public int GetSpawnSpeed(PlayerType p){
        p = (PlayerType) statShuffle[(int) p];
        return spawnStats[(int) p];
    }

    public int GetAttack(PlayerType p){
        p = (PlayerType) statShuffle[(int) p];
        return attackStats[(int) p];
    }

    public int GetDefense(PlayerType p){
        p = (PlayerType) statShuffle[(int) p];
        return defenseStats[(int) p];
    }
}

