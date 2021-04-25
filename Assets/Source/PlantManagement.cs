using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Item {
    public string name;
    public Sprite sprite;
    public string description;
    public float range = 0;
    public float damage = 0;
    public float prod = 0;
    public float firerate = 0;
    public float prodrate = 0;
    public float lumPerRoom = 0;
    public float luck = 0;
    public float halfNotMossMalus = 0;
    public float light = 0;
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
    public TMP_Text amtTimer;
    public TMP_Text amtKills;
    Transform winpanel;

    public float reslight = 200;

    public int sel = 0;

    public float[] pcost = new float[3] { 50, 75, 120 };

    float timer;
    public int kills;

    public bool endshown = false;

    public enum State { Idle, Placing, Reward, End }
    public State state;

    void ConstructItemPool () {
        foreach (Sprite sprite in itemSprites) {
            Item item = new Item();
            item.name = sprite.name;
            item.sprite = sprite;
            item.name = item.name.Substring(5, item.name.Length - 5);
            item.name = item.name[0].ToString().ToUpper() + 
                item.name.Substring(1, item.name.Length - 1);
            if (item.name == "Pepper") {
                item.description = "Bonus damage 20%";
                item.damage = 0.20f;
            }
            if (item.name == "Glasses") {
                item.description = "Bonus range 35%";
                item.range = 0.35f;
            }
            if (item.name == "Lightbulb") {
                item.description = "Bonus light production 10%";
                item.prod = 0.10f;
            }
            if (item.name == "Mustaches") {
                item.description = "Allow 1 more Luminenescent per room";
                item.lumPerRoom = 1;
            }
            if (item.name == "Horseshoe") {
                item.description = "+20% effect to all other bonus items";
                item.luck = 0.20f;
            }
            if (item.name == "Hat") {
                item.description = "Bonus fire rate + 15%";
                item.firerate = 0.15f;
            }
            if (item.name == "Machete") {
                item.description = "Half malus when placing a plant in a non mossy room";
                item.halfNotMossMalus = 0.5f;
            }
            if (item.name == "Power") {
                item.description = "+200 Light";
                item.light = 200;
            }
            if (item.name == "Ghost") {
                item.description = "Faster Production 10%";
                item.prodrate = 0.10f;
            }
            if (item.name == "Charcoal") {
                item.description = "Bonus damage and Range 10%";
                item.damage = 0.10f;
                item.range = 0.10f;
            }
            if (item.name == "Slingshot") {
                item.description = "-100 Light, Bonus fire rate 25%";
                item.light = -100;
                item.firerate = 0.25f;
            }
            if (item.name == "Paint") {
                item.description = "+100 Light";
                item.light = 100;
            }
            if (item.name == "Chip") {
                item.description = "Bonus light production and production speed 15%";
                item.prodrate = 0.15f;
                item.prod = 0.15f;
            }
            if (item.name == "Sign") {
                item.description = "Bonus firerate 20% at the cost of 10% damage";
                item.firerate = 0.20f;
                item.damage = -0.10f;
            }
            if (item.name == "Reactor") {
                item.description = "Bonus production 50% at the cost of 400 light";
                item.prod = 0.50f;
                item.light = -400;
            }
            if (item.name == "Map") {
                item.description = "Bonus range 100% at the cost of 20% less fire rate";
                item.range = 1.00f;
                item.firerate = -0.20f;
            }
            if (item.name == "Pen") {
                item.description = "Bonus damage 30%";
                item.damage = 0.30f;
            }
            if (item.name == "Branch") {
                item.description = "+100 light and +20% effect to all other bonus items";
                item.luck = 0.20f;
                item.light = 100;
            }
            itemPool.Add(item);
        }
    }

    public Item Bonuses () {
        Item bonus = new Item();
        foreach (Item item in inventory) {
            bonus.damage += item.damage;
            bonus.range += item.range;
            bonus.prod += item.prod;
            bonus.firerate += item.firerate;
            bonus.prodrate += item.prodrate;
            bonus.lumPerRoom += item.lumPerRoom;
            bonus.luck += item.luck;
            bonus.halfNotMossMalus += item.halfNotMossMalus;
            bonus.light += item.light;
        }
        bonus.damage += bonus.luck + 1;
        bonus.range += bonus.luck + 1;
        bonus.prod += bonus.luck + 1;
        bonus.firerate += bonus.luck + 1;
        bonus.prodrate += bonus.luck + 1;
        bonus.lumPerRoom += bonus.luck + 1;
        bonus.halfNotMossMalus += bonus.luck + 1;
        bonus.light += bonus.luck + 1;
        return bonus;
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
        amtKills = GameObject.Find("AmtKills").GetComponent<TMP_Text>();
        amtTimer = GameObject.Find("AmtTime").GetComponent<TMP_Text>();
        rew0 = rewardPanel.transform.Find("Rew0Panel").Find("Rew0").GetComponent<Image>();
        rew1 = rewardPanel.transform.Find("Rew1Panel").Find("Rew1").GetComponent<Image>();
        rew0Name = rewardPanel.transform.Find("Rew0Panel").Find("Rew0Name").GetComponent<TMP_Text>();
        rew1Name = rewardPanel.transform.Find("Rew1Panel").Find("Rew1Name").GetComponent<TMP_Text>();
        rew0Description = rewardPanel.transform.Find("Rew0Panel").Find("Rew0Description").GetComponent<TMP_Text>();
        rew1Description = rewardPanel.transform.Find("Rew1Panel").Find("Rew1Description").GetComponent<TMP_Text>();
        winpanel = GameObject.Find("WinPanel").transform;
        winpanel.gameObject.SetActive(false);
        invetoryFather = GameObject.Find("Inventory");
        GameObject obj = new GameObject();
        prefabIndicator = obj.AddComponent<SpriteRenderer>();
        ConstructItemPool();

        timer = Time.time;

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

        amtTimer.text = (Time.time - timer).ToString("N0");
        amtKills.text = kills.ToString();

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

                int lumNum = 0;
                foreach (PlantLogic pl in plants) {
                    if (rhover.cells.Contains(pl.cell)
                        && pl.production > 0 && sel == 2) { lumNum++; }
                }
                bool hasLum = lumNum >= 1 * Bonuses().lumPerRoom;
                if (hasLum) {
                    labelCost.gameObject.SetActive(true);
                    labelCost.text = "Cave has another Luminescent";
                }

                if (!tile && !ontop && adjToMossy && !hasLum) {
                    prefabIndicator.gameObject.SetActive(true);
                    prefabIndicator.sprite = plantsPrefabs[sel].GetComponent<SpriteRenderer>().sprite;
                    float moss = rhover.hasMoss ? 1 : 2 + (1 - 1 * Bonuses().halfNotMossMalus);
                    float cost = pcost[sel] * moss;

                    if (reslight >= cost) {
                        amtCost.gameObject.SetActive(true);
                        labelCost.gameObject.SetActive(true);
                        labelCost.text = rhover.hasMoss ? "Cost" : "Double cost, cave is not mossy";
                        amtCost.text = cost.ToString("N0");

                        if (Input.GetMouseButtonDown(0)) {
                            state = State.Idle;
                            reslight -= cost;

                            Spawn(mpInt, sel);
                            if (!rhover.hasMoss && sel == 2) {
                                gg.CoverWithMoss(rhover);
                                state = State.Reward;
                                choice = PickItems(2);
                            }
                        }
                    } else {
                        labelCost.gameObject.SetActive(true);
                        labelCost.text = "Not enough light (If there's no moss it costs x2)";
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
            income += (pl.production * Bonuses().prod) 
                / (pl.prodCooldown * Bonuses().prodrate);
        }
        amtIncome.text = income.ToString("N1");

        amtLight.text = reslight.ToString("N0");

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

        if (!endshown) {
            if (inventory.Count > 15) {
                winpanel.gameObject.SetActive(true);
                state = State.End;
                endshown = true;
                winpanel.Find("Why").GetComponent<TMP_Text>().text = "You collected 15 items!";
                winpanel.Find("Name").GetComponent<TMP_Text>().text = "You Win!";
            }
            if (plants.Count == 0 && reslight < 50) {
                winpanel.gameObject.SetActive(true);
                print("you lose");
                state = State.End;
                endshown = true;
                winpanel.Find("Why").GetComponent<TMP_Text>().text = "You don't have enough light to rebuild!";
                winpanel.Find("Name").GetComponent<TMP_Text>().text = "You Lose!";
            }
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
        reslight += choice[val].light;
    }

    public void UIPressWin (int val) {
        if (val == 0) {
            state = State.Idle;
            winpanel.gameObject.SetActive(false);
        }
        if (val == 1) {
            SceneManager.LoadScene("Menu");
        }
    }
}
