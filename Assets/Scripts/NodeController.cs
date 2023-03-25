using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeController : MonoBehaviour
{
    public GameController parent;
    public PlayerType owner;
    public GameObject textObject;
    public float pop = 0;
    public Color backColor;
    public Color fontColor;
    public float lastGrowth;
    public float lastAttack;
    public float lastChange;
    public float defense;
    public float attack;

    public int moveSpeedStat = -1;
    public int spawnSpeedStat = -1;
    public int attackStat = -1;
    public int defenseStat = -1;

    public float changeTime = 5.0f;
    public float spawnTime = 3;
    public float attackTime = 0.25f;
    public int TargetNode = -1;
    public float radius = 1.0f;
    public int frontier;

    public int id;

    // Start is called before the first frame update
    void Start()
    {
        lastGrowth = -1;
        lastAttack = -1;
        lastChange = Random.Range(0.0f, changeTime) - 1;
    }

    public void SetPopulation(int newPop)
    {
        pop = newPop;
        UpdatePopDisplay();
    }

    public void UpdatePopDisplay(){
        TextMesh t = textObject.GetComponent<TextMesh>();
        //t.text = frontier.ToString();
        //t.text = owner.ToString();
        t.text = Mathf.FloorToInt(pop).ToString();
        t.color = fontColor;
    }

    public void SetOwner(PlayerType newOwner)
    {
        owner = newOwner;

        moveSpeedStat = parent.stats.GetMoveSpeed(owner);
        spawnSpeedStat = parent.stats.GetSpawnSpeed(owner);
        attackStat = parent.stats.GetAttack(owner);
        defenseStat = parent.stats.GetDefense(owner);
        if(owner == PlayerType.Unowned) TargetNode = -1;

        backColor = parent.stats.GetColor(owner);
        fontColor = parent.stats.GetFontColor(owner);

        attackTime = 0.8f / new float[] { 2.0f, 3.0f, 4.0f, 5f, 8f}[moveSpeedStat-1];
        spawnTime = new float[] { 1.7f, 1.4f, 1.0f, 0.75f, 0.60f}[spawnSpeedStat-1] / 0.55f * parent.simScale;
        defense = new float[] { 1.0f, 0.7f, 0.5f, 0.32f, 0.25f}[defenseStat-1];;
        attack = new float[] { 1.2f, 1.5f, 2.4f, 2.8f, 5.5f}[attackStat-1];

        UpdateColor(false);
        if(parent.selected == id) parent.selected = -1;
    }

    void Attack(){
        GameObject bugObj = Instantiate(parent.bugPrefab, this.transform.position, Quaternion.identity);
        BugController bugCon = bugObj.GetComponent<BugController>();
        bugObj.transform.parent = parent.bugFolder.transform;
        bugCon.parent = this.parent;
        bugCon.owner = owner;
        bugCon.home = this.gameObject;
        bugCon.target = parent.nodes[TargetNode].gameObject;
        bugCon.speed = 2.0f / attackTime;
        bugCon.attack = attack;
        bugCon.defense = defense;
        bugObj.transform.position -= (bugObj.transform.position - bugCon.target.transform.position).normalized * parent.nodeRadius;
        pop -= 1;
        if(parent.nodes[TargetNode].owner == owner) lastAttack += attackTime * 0.25f;  
    }

    void FindAllyInTrouble(){
        for(int i = 0; i < 15; i += 1){
            int TargetId = parent.connections[id][Random.Range(0, parent.connections[id].Count)];
            if(parent.nodes[TargetId].owner == owner && pop - parent.nodes[TargetId].pop > 20 && parent.nodes[TargetId].pop < 30 && parent.nodes[TargetId].frontier == 0){
                TargetNode = TargetId;
            }
        }
    }

    void FindEnemy(){
        for(int i = 0; i < 15; i += 1){
            int TargetId = parent.connections[id][Random.Range(0, parent.connections[id].Count)];
            if(parent.nodes[TargetId].owner != owner){
                TargetNode = TargetId;
            }
        }
    }

    void FindFront(){
        for(int i = 0; i < 15; i += 1){
            int TargetId = parent.connections[id][Random.Range(0, parent.connections[id].Count)];
            if(parent.nodes[TargetId].owner == owner && parent.nodes[TargetId].frontier < frontier && parent.nodes[TargetId].pop < 50){
                TargetNode = TargetId;
            }
        }
    }

    void FindFlank(){
        for(int i = 0; i < 15; i += 1){
            int TargetId = parent.connections[id][Random.Range(0, parent.connections[id].Count)];
            if(parent.nodes[TargetId].owner == owner && parent.nodes[TargetId].frontier <= frontier && parent.nodes[TargetId].pop < 50){
                TargetNode = TargetId;
            }
        }
    }


    public void SetTarget(){
        lastChange += Time.deltaTime * parent.gameSpeed;
        if(lastChange > changeTime){
            lastChange -= changeTime;
            changeTime = Random.Range(3.0f, 7.0f) * attackTime * 5;
            if(owner != PlayerType.Unowned && parent.connections[id].Count > 0){
                TargetNode = -1;
                FindAllyInTrouble();
                if(TargetNode == -1) FindEnemy();
                if(TargetNode == -1) FindFront();
                if(TargetNode == -1) FindFlank();
                if(TargetNode == -1) TargetNode = parent.connections[id][Random.Range(0, parent.connections[id].Count)];
            }
            if(Random.Range(0.0f, 1.0f) < 0.1f) TargetNode = -1;
        }
    }

    void UpdateColor(bool select){
        backColor = parent.stats.GetColor(owner);
        if(select) backColor = new Color32((byte) (backColor.r / 2), (byte) (backColor.g / 2), (byte) (backColor.b / 2), 255);
        SpriteRenderer mat = transform.GetComponent<SpriteRenderer>();
        mat.color = backColor;
    }

    void OnMouseDown() {
        if(parent.playerRace == PlayerType.Unowned) parent.playerRace = owner;
        if(parent.playerRace == PlayerType.Unowned) return;
        Debug.Log("Click " + id.ToString());

        if(parent.selected == -1){
            if(owner != parent.playerRace) return;
            parent.selected = id;
            UpdateColor(true);
            Debug.Log("Set selected to " + id.ToString());
        } else {
            if(parent.connections[parent.selected].Contains(id)){
                parent.nodes[parent.selected].TargetNode = id;
                Debug.Log("Attacking " + id.ToString());
                parent.nodes[parent.selected].UpdateColor(false);
                parent.selected = -1;
            } else if(parent.selected == id) {
                TargetNode = -1;
                UpdateColor(false);
                parent.selected = -1;
            } else {
                parent.nodes[parent.selected].UpdateColor(false);
                parent.selected = -1;
                Debug.Log("Invalid target");
            }
        }
    }

    void MakeDecision(){
        if(parent.playerRace != owner){
            SetTarget();
        }

        if(TargetNode != -1){
            lastAttack += Time.deltaTime * parent.gameSpeed;
            if(lastAttack > attackTime){
                lastAttack -= attackTime;
                if(this.pop <= 5){
                    TargetNode = -1;
                }else{
                    Attack();
                }
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(pop > 20 && owner != PlayerType.Unowned){
            lastGrowth += Time.deltaTime * (Mathf.Max(0, 40 - pop) / 20) * parent.gameSpeed;
        }else if(pop > 10 && owner == PlayerType.Unowned){
            lastGrowth += Time.deltaTime * (Mathf.Max(0, 20 - pop) / 10) * parent.gameSpeed;
        }else{
            lastGrowth += Time.deltaTime * parent.gameSpeed;
        }
        if(lastGrowth > spawnTime){
            lastGrowth -= spawnTime;
            pop += 1;
        }

        MakeDecision();

        UpdatePopDisplay();
    }
}
