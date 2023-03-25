using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BugController : MonoBehaviour
{
    public GameObject home;
    public PlayerType owner;
    public GameObject target;
    public GameController parent;
    public float speed = 3;
    public float attack;
    public float defense;
    public Color backColor;

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject otherObj = other.gameObject;
        BugController otherCon = otherObj.GetComponent<BugController>();
        if(otherCon == null) return;
        if(owner == otherCon.owner) return;
        
        if(attack / defense > otherCon.attack / otherCon.defense){
            attack -= otherCon.attack * defense;
            Destroy(otherObj);
        } else {
            otherCon.attack -= attack / otherCon.defense;
            Destroy(this.gameObject);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        backColor = parent.stats.GetColor(owner);

        SpriteRenderer mat = transform.GetComponent<SpriteRenderer>();
        mat.color = backColor;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        this.transform.position -= (this.transform.position - target.transform.position).normalized * speed * Time.deltaTime * parent.gameSpeed;
        if(Vector3.Distance(transform.position, target.transform.position) < parent.nodeRadius){
            NodeController n = target.GetComponent<NodeController>();
            NodeController homeCon = home.GetComponent<NodeController>();
            if(homeCon.owner == n.owner){
                n.pop += 1;
            }else{
                n.pop -= attack * n.defense;
                if(n.pop < 0){
                    //Debug.Log(homeCon.owner);
                    n.SetOwner(homeCon.owner);
                    n.pop = 1;
                    n.TargetNode = -1;
                    parent.setFrontiers();
                }
            }
            Destroy(this.gameObject);
        }
    }
}
