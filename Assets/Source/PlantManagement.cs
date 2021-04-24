using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class PlantManagement : MonoBehaviour {
    GroundGeneration gg;

    public List<GameObject> plantsPrefabs = new List<GameObject>();

    GameObject tileIndicator;
    SpriteRenderer prefabIndicator;
    GameObject plantsFather;
    public List<PlantLogic> plants = new List<PlantLogic>();

    public TMP_Text amtLight;
    public TMP_Text amtIncome;
    public TMP_Text amtCost;
    public TMP_Text labelCost;

    public int reslight = 200;

    public int sel = 0;

    public float[] pcost = new float[3] { 50, 75, 120 };

    public enum State { Idle, Placing }
    public State state;

    void Start() {
        gg = GameObject.Find("Grid").GetComponent<GroundGeneration>();
        plantsFather = GameObject.Find("Plants");
        tileIndicator = GameObject.Find("TileIndicator");
        amtLight = GameObject.Find("AmtResource").GetComponent<TMP_Text>();
        amtIncome = GameObject.Find("AmtIncome").GetComponent<TMP_Text>();
        amtCost = GameObject.Find("AmtCost").GetComponent<TMP_Text>();
        labelCost = GameObject.Find("LabelCost").GetComponent<TMP_Text>();
        GameObject obj = new GameObject();
        prefabIndicator = obj.AddComponent<SpriteRenderer>();

        state = State.Idle;
    }

    public PlantLogic PlantFromVec(Vector3Int pos) {
        foreach (PlantLogic pl in plants) {
            if (pl.cell == pos) { return pl; }
        }
        return null;
    }

    public void Spawn(Vector3Int cell, int s) {
        Vector3 pos = new Vector3(cell.x, cell.y) * 0.25f
            + new Vector3(0.25f / 2, 0.25f / 2, -1);
        GameObject obj = Instantiate(plantsPrefabs[s], pos, Quaternion.identity);
        obj.transform.SetParent(plantsFather.transform);
        PlantLogic pl = obj.GetComponent<PlantLogic>();
        pl.cell = cell;
        plants.Add(pl);
    }

    void Update() {
        Vector3 mp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int mpInt = new Vector3Int(Mathf.FloorToInt(mp.x * 4),
            Mathf.FloorToInt(mp.y * 4), 0);
        tileIndicator.SetActive(false);
        prefabIndicator.gameObject.SetActive(false);
        tileIndicator.transform.position =
            new Vector3(mpInt.x, mpInt.y) * 0.25f
            + new Vector3(0.25f / 2, 0.25f / 2, -1);
        prefabIndicator.transform.position = tileIndicator.transform.position;
        Room rhover = null;
        foreach (Room room in gg.rooms) {
            if (room.cells.Contains(mpInt)) {
                rhover = room;
            }
        }

        amtCost.gameObject.SetActive(false);
        labelCost.gameObject.SetActive(false);

        if (state == State.Idle) {
        } else if (state == State.Placing) {
            if (rhover != null) {
                tileIndicator.SetActive(true);

                Vector3 pos = tileIndicator.transform.position;
                TileBase tile = gg.groundtilemap.GetTile(mpInt + new Vector3Int(0, 0, 1));
                bool ontop = PlantFromVec(mpInt) != null;
                bool adjToMossy = rhover.hasMoss;
                foreach(Room neighbor in rhover.neighbors) {
                    if (neighbor.hasMoss) adjToMossy = true;
                }

                bool hasLum = false;
                foreach (PlantLogic pl in plants) {
                    if (rhover.cells.Contains(pl.cell) 
                        && pl.production > 0 && sel == 2) 
                    { hasLum = true; }
                }

                if (tile) {
                    labelCost.gameObject.SetActive(true);
                    labelCost.text = "Invalid Position";
                }

                if (ontop) {
                    labelCost.gameObject.SetActive(true);
                    labelCost.text = "On top of other plant";
                }

                if (!adjToMossy) {
                    labelCost.gameObject.SetActive(true);
                    labelCost.text = "Not in of near a mossy cave";
                }

                if (hasLum) {
                    labelCost.gameObject.SetActive(true);
                    labelCost.text = "Cave has another Luminescent";
                }

                if (!tile && !ontop && adjToMossy && !hasLum) {
                    prefabIndicator.gameObject.SetActive(true);
                    prefabIndicator.sprite = plantsPrefabs[sel].GetComponent<SpriteRenderer>().sprite;
                    int moss = rhover.hasMoss ? 1 : 2;
                    float cost = pcost[sel] * moss;
                    if (reslight >= cost) {
                        amtCost.gameObject.SetActive(true);
                        labelCost.gameObject.SetActive(true);
                        labelCost.text = rhover.hasMoss ? "Cost" : "Double cost, cave is not mossy";
                        amtCost.text = cost.ToString("N0");

                        if (Input.GetMouseButtonDown(0)) {
                            reslight -= (int)cost;
                            state = State.Idle;

                            Spawn(mpInt, sel);
                            if (!rhover.hasMoss && sel == 2) {
                                gg.CoverWithMoss(rhover);
                            }
                        }
                    }
                }
            }
        }

        float income = 0;
        foreach (PlantLogic pl in plants) {
            income += pl.production / pl.prodCooldown;
        }
        amtIncome.text = income.ToString("N0");

        amtLight.text = reslight.ToString();

        List<PlantLogic> delList = new List<PlantLogic>();
        foreach (PlantLogic pl in plants) {
            if (pl.hp <= 0) {
                delList.Add(pl);
            }
        }
        foreach (PlantLogic pl in delList) {
            plants.Remove(pl);
            Destroy(pl.gameObject);
        }

        if (plants.Count == 0 && reslight < 50) {
            print("you lose");
        }
    }

    public void UIPressPlant(int val) {
        state = State.Placing;
        sel = val;
    }
}
