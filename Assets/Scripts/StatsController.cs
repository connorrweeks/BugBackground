using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class StatsController : MonoBehaviour
{
    public GameController parent;
    Dictionary<string, GameObject> myChildren;
    void Start()
    {
        myChildren = new Dictionary<string, GameObject>();
        foreach (Transform child in transform)
        {
            myChildren.Add(child.gameObject.name, child.gameObject);
        }
    }

    private int currentType = -1;
    public void ChooseAndUpdate(){
        if(currentType == -1) currentType = UnityEngine.Random.Range(0, parent.stats.GetNum()-1);
        int k = 0;
        int m = Enum.GetNames(typeof(PlayerType)).Length - 1;
        while(k < m){
            currentType = (currentType + 1) % m;
            k += 1;
            PlayerType pt = (PlayerType) currentType;
            foreach(NodeController n in parent.nodes){
                if(n.owner == pt){
                    UpdateDisplay(pt);
                    return;
                }
            }
        }
        Debug.Log("No players found!");
        UpdateDisplay(PlayerType.Unowned);
    }

    public void UpdateDisplay(PlayerType p){
        gameObject.GetComponent<Image>().color = parent.stats.GetColor(p);
        myChildren["Text"].GetComponent<Text>().text = p.ToString();
        myChildren["Text"].GetComponent<Text>().color = parent.stats.GetFontColor(p);
        for(int i = 1; i < 6; i += 1){
            myChildren["Move" + i.ToString()].SetActive(i <= parent.stats.GetMoveSpeed(p));
            myChildren["Spawn" + i.ToString()].SetActive(i <= parent.stats.GetSpawnSpeed(p));
            myChildren["Attack" + i.ToString()].SetActive(i <= parent.stats.GetAttack(p));
            myChildren["Defense" + i.ToString()].SetActive(i <= parent.stats.GetDefense(p));
            myChildren["Move" + i.ToString()].GetComponent<Image>().color = parent.stats.GetFontColor(p);
            myChildren["Spawn" + i.ToString()].GetComponent<Image>().color = parent.stats.GetFontColor(p);
            myChildren["Attack" + i.ToString()].GetComponent<Image>().color = parent.stats.GetFontColor(p);
            myChildren["Defense" + i.ToString()].GetComponent<Image>().color = parent.stats.GetFontColor(p);
        }
    }
}
