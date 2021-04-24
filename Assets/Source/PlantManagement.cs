using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using UnityEngine.UI;

public class Item {
    public string name;
    public Sprite sprite;
    public string description;
}

public class PlantManagement : MonoBehaviour {
    public GameObject itemPrefab;
    public List<Sprite> itemSprites;
    public List<Item> itemPool = new List<Item>();
    public List<Item> inventory = new List<Item>();
    public List<Item> choice = new List<Item>();

    GroundGeneration gg;

    public List<GameObject> plantsPrefabs = new List<GameObject>();

    GameObject tileIndicator;
    SpriteRenderer prefabIndicator;
    GameObject plantsFather;
    GameObject invetoryFather;
    public List<PlantLogic> plants = new List<PlantLogic>();

    public TMP_Text amtLight;
    public TMP_Text amtIncome;
    public TMP_Text amtCost;
    public TMP_Text labelCost;
    public GameObject rewardPanel;
    public Image rew0;
    public Image rew1;
    public TMP_Text rew0Name;
    public TMP_Text rew1Name;
    public TMP_Text rew0Description;
    public TMP_Text rew1Description;

    public int reslight = 200;

    public int sel = 0;

    public float[] pcost = new float[3] { 50, 75, 120 };

    public enum State { Idle, Placing, Reward }
    public State state;

    void ConstructItemPool () {
        foreach (Sprite sprite in itemSprites) {
            Item item = new Item();
            item.name = sprite.name;
            item.sprite = sprite;
            if (item.name == "item_ghost") {
                item.description = "faster fast";
            }
            itemPool.Add(item);
        }
    }

    Item PickItem (List<Item> items) {
        if (items.Count == 0) return null;
        else return items[Random.Range(0, items.Count-1)];
    }

    List<Item> PickItems (int n) {
        List<Item> items = new List<Item>();
        List<Item> temp = new List<Item>();
        foreach (Item i in itemPool) temp.Add(i);
        for (int i = 0; i < n; i++) {
            Item item = PickItem(temp);
            items.Add(item);
            temp.Remove(item);
        }
        return items;
    }

    void Start() {
        gg = GameObject.Find("Grid").GetComponent<GroundGeneration>();
        plantsFather = GameObject.Find("Plants");
        tileIndicator = GameObject.Find("TileIndicator");
        amtLight = GameObject.Find("AmtResource").GetComponent<TMP_Text>();
        amtIncome = GameObject.Find("AmtIncome").GetComponent<TMP_Text>();
        amtCost = GameObject.Find("AmtCost").GetComponent<TMP_Text>();
        labelCost = GameObject.Find("LabelCost").GetComponent<TMP_Text>();
        rewardPanel = GameObject.Find("ChoosePanel");
        rew0 = rewardPanel.transform.Find("Rew0Panel").Find("Rew0").GetComponent<Image>();
        rew1 = rewardPanel.transform.Find("Rew1Panel").Find("Rew1").GetComponent<Image>();
        rew0Name = rewardPanel.transform.Find("Rew0Panel").Find("Rew0Name").GetComponent<TMP_Text>();
        rew1Name = rewardPanel.transform.Find("Rew1Panel").Find("Rew1Name").GetComponent<TMP_Text>();
        rew0Description = rewardPanel.transform.Find("Rew0Panel").Find("Rew0Description").GetComponent<TMP_Text>();
        rew1Description = rewardPanel.transform.Find("Rew1Panel").Find("Rew1Description").GetComponent<TMP_Text>();
        invetoryFather = GameObject.Find("Inventory");
        GameObject obj = new GameObject();
        prefabIndicator = obj.AddComponent<SpriteRenderer>();
        ConstructItemPool();

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

        foreach (Transform deadman in invetoryFather.transform) { 
            Destroy(deadman.gameObject); 
        }
        Vector3 off = new Vector3();
        foreach (Item item in inventory) {
            GameObject obj = Instantiate(itemPrefab, 
                invetoryFather.transform.position + off, Quaternion.identity);
            obj.transform.SetParent(invetoryFather.transform);
            obj.GetComponent<Image>().sprite = item.sprite;
            off += new Vector3(32+10, 0, 0);
        }

        amtCost.gameObject.SetActive(false);
        labelCost.gameObject.SetActive(false);
        rewardPanel.SetActive(false);

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
                            state = State.Idle;
                            reslight -= (int)cost;

                            Spawn(mpInt, sel);
                            if (!rhover.hasMoss && sel == 2) {
                                gg.CoverWithMoss(rhover);
                                state = State.Reward;
                                choice = PickItems(2);
                            }
                        }
                    } else {
                        labelCost.gameObject.SetActive(true);
                        labelCost.text = "Not enough light!";
                    }
                }
            }
        } else if (state == State.Reward) {
            rewardPanel.SetActive(true);
            rew0.sprite = choice[0].sprite;
            rew1.sprite = choice[1].sprite;
            rew0Name.text = choice[0].name;
            rew1Name.text = choice[1].name;
            rew0Description.text = choice[0].description;
            rew1Description.text = choice[1].description;
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

    public void UIPressPick (int val) {
        state = State.Idle;
        inventory.Add(choice[val]);
        itemPool.Remove(choice[val]);
    }
}
