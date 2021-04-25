using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Rule {
    public int[] match;
    public TileBase[] tile;
    public Matrix4x4 matrix;
}

public class Room {
    public List<Vector3Int> cells = new List<Vector3Int>();
    public Vector2Int boundsTopLeft;
    public Vector2Int boundsBottomRight;
    public List<Room> neighbors = new List<Room>();
    public bool hasMoss = false;

    public bool Overlap(Room oth) {
        if (boundsTopLeft.x > oth.boundsBottomRight.x
            && boundsBottomRight.x < oth.boundsTopLeft.x
            && boundsTopLeft.y > oth.boundsBottomRight.y
            && boundsBottomRight.y < oth.boundsTopLeft.y) {
            return false;
        }
        foreach (Vector3Int apt in cells) {
            if (oth.cells.Contains(apt)) return true;
        }
        return false;
    }
    public float SqrDistance(Room oth, out Vector3Int from, out Vector3Int to) {
        float dist = float.PositiveInfinity;
        from = new Vector3Int(); to = new Vector3Int(); // c# 
        foreach (Vector3Int pt in cells) {
            foreach (Vector3Int othpt in oth.cells) {
                Vector2 diff = new Vector2(pt.x - othpt.x, pt.y - othpt.y);
                if (diff.sqrMagnitude < dist) {
                    dist = diff.sqrMagnitude;
                    from = pt; to = othpt;
                }
            }
        }
        return dist;
    }

    public List<Vector3Int> Contour() {
        List<Vector3Int> cont = new List<Vector3Int>();
        foreach (Vector3Int cell in cells) {
            bool border = false;
            for (int x = -1; x < 2 && !border; x++) {
                for (int y = -1; y < 2 && !border; y++) {
                    Vector3Int v = cell + new Vector3Int(x, y, 0);
                    if (!cells.Contains(v)) border = true;
                }
            }
            if (border) {
                cont.Add(cell);
            }
        }
        return cont;
    }
}

public class GroundGeneration : MonoBehaviour {

    public Vector2Int size;
    public TileBase ground_full_0Tile;
    public TileBase ground_full_1Tile;
    public TileBase ground_corridorTile;
    public TileBase ground_corridor_cornerTile;
    public TileBase ground_roomTile;
    public TileBase moss_0Tile;
    public TileBase moss_1Tile;
    public TileBase moss_2Tile;
    public TileBase moss_cornerTile;
    public TileBase moss_corner_innerTile;
    public TileBase ground_mossTile;
    public TileBase darkTile;
    public Tilemap groundtilemap;
    public Tilemap wallstilemap;
    public Tilemap darktilemap;
    public List<Room> rooms = new List<Room>();
    public Room start;

    public List<Vector3Int> Expand(List<Vector3Int> cells) {
        List<Vector3Int> exp = new List<Vector3Int>();
        foreach (Vector3Int cell in cells) {
            exp.Add(cell);
            for (int x = -1; x < 2; x++) {
                for (int y = -1; y < 2; y++) {
                    Vector3Int v = cell + new Vector3Int(x, y, 0);
                    if (!exp.Contains(v)) {
                        exp.Add(v);
                    }
                }
            }
        }
        return exp;
    }

    Room GenRoomRandRect(int size, int n = 8) {
        Room room = new Room();
        for (int i = 0; i < n; i++) {
            int sx = Random.Range(2, size);
            int sy = Random.Range(2, size);
            int dir = Random.Range(0, 4);
            if (dir == 1) {
                int temp = sy;
                sy = sx;
                sx = -temp;
            }
            if (dir == 2) {
                sx *= -1;
                sy *= -1;
            }
            if (dir == 3) {
                int temp = sx;
                sx = sy;
                sy = -temp;
            }
            for (int y = Mathf.Min(0, sy); y < Mathf.Max(1, sy); y++) {
                for (int x = Mathf.Min(0, sx); x < Mathf.Max(1, sx); x++) {
                    Vector3Int pt = new Vector3Int(x, y, 0);
                    if (!room.cells.Contains(pt)) {
                        room.cells.Add(pt);
                    }
                }
            }
        }
        room.boundsTopLeft = new Vector2Int(-size, -size);
        room.boundsBottomRight = new Vector2Int(size, size);
        return room;
    }

    void DrawCorridor(Vector3Int from, Vector3Int to) {
        Vector3Int[] dirToVec = new Vector3Int[4] {
            new Vector3Int(1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, -1, 0)
        };
        int iter = 0;
        List<Vector3Int> corr = new List<Vector3Int>();
        for (; iter < 100; iter++) {
            if (from == to) break;
            float mind = float.PositiveInfinity;
            int seld = 0;
            for (int d = 0; d < 4; d++) {
                float dist = Vector3.Distance(from + dirToVec[d], to);
                if (dist <= mind) {
                    mind = dist; seld = d;
                }
            }

            from = from + dirToVec[seld];
            wallstilemap.SetTile(from, null);
            corr.Add(from);
        }

        List<Rule> rules = new List<Rule>();
        {
            Rule rule = new Rule();
            rule.match = new int[4] { 1, 0, 1, 0 };
            rule.tile = new TileBase[] { ground_corridorTile };
            rule.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 90f), Vector3.one);
            rules.Add(rule);
        }
        {
            Rule rule = new Rule();
            rule.match = new int[4] { 0, 1, 0, 1 };
            rule.tile = new TileBase[] { ground_corridorTile };
            rule.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 0f), Vector3.one);
            rules.Add(rule);
        }
        {
            Rule rule = new Rule();
            rule.match = new int[4] { 1, 1, 0, 0 };
            rule.tile = new TileBase[] { ground_corridor_cornerTile };
            rule.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 90f), Vector3.one);
            rules.Add(rule);
        }
        {
            Rule rule = new Rule();
            rule.match = new int[4] { 1, 0, 0, 1 };
            rule.tile = new TileBase[] { ground_corridor_cornerTile };
            rule.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 0f), Vector3.one);
            rules.Add(rule);
        }
        {
            Rule rule = new Rule();
            rule.match = new int[4] { 0, 1, 1, 0 };
            rule.tile = new TileBase[] { ground_corridor_cornerTile };
            rule.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 180f), Vector3.one);
            rules.Add(rule);
        }
        {
            Rule rule = new Rule();
            rule.match = new int[4] { 0, 0, 1, 1 };
            rule.tile = new TileBase[] { ground_corridor_cornerTile };
            rule.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 270f), Vector3.one);
            rules.Add(rule);
        }


        for (int i = 0; i < corr.Count; i++) {
            foreach (Rule rule in rules) {
                bool found = true;
                for (int d = 0; d < 4; d++) {
                    Vector3Int see = corr[i] + dirToVec[d];
                    if (rule.match[d] == 0) {
                        if (corr.Contains(see)) {
                            found = false;
                            break;
                        }
                    }
                }
                if (found) {
                    groundtilemap.SetTile(corr[i], rule.tile[0]);
                    groundtilemap.SetTransformMatrix(corr[i], rule.matrix);
                    break;
                }
            }
        }
        if (iter >= 100) { print("out of iteration"); }
    }

    void ConnectRooms(List<Room> rooms) {
        Room start = rooms[0];
        List<Room> conn = new List<Room>();
        conn.Add(start);
        int iter = 0;
        for (; iter < rooms.Count - 1; iter += 1) {
            float mind = float.PositiveInfinity;
            Vector3Int from = new Vector3Int(), to = new Vector3Int();
            Room nearest = null;
            Room sel = null;
            foreach (Room rcon in conn) {
                foreach (Room oth in rooms) {
                    if (conn.Contains(oth)) { continue; }
                    Vector3Int tempfrom, tempto;
                    float dist = rcon.SqrDistance(oth, out tempfrom, out tempto);
                    if (dist < mind) {
                        mind = dist;
                        nearest = oth;
                        from = tempfrom;
                        to = tempto;
                        sel = rcon;
                    }
                }
            }
            if (nearest == null) {
                break;
            } else {
                sel.neighbors.Add(nearest);
                nearest.neighbors.Add(sel);
                conn.Add(nearest);

                DrawCorridor(from, to);
            }
        }
    }

    void GroundCorridors(List<Room> rooms, int n) {
        EnemyManagement em = FindObjectOfType<EnemyManagement>();
        Room start = rooms[0];
        {
            Room temp = new Room();
            Vector3Int spawn = new Vector3Int(start.cells[0].x + 4, 0, 0);
            SpawnPoint sp = new SpawnPoint();
            sp.pt = spawn; sp.room = start;
            em.spawnPoints.Add(sp);
            temp.cells.Add(spawn);
            Vector3Int from, to;
            rooms[0].SqrDistance(temp, out from, out to);
            DrawCorridor(from, to);
        }
        /*
        rooms.Remove(start);

        rooms.Sort((a, b) => a.cells[0].y.CompareTo(b.cells[0].y));
        rooms.Reverse();
        for (int i = 0; i < n; i++) {
            Room temp = new Room();
            Vector3Int spawn = new Vector3Int(rooms[i].cells[0].x + 4, 0, 0);
            SpawnPoint sp = new SpawnPoint();
            sp.pt = spawn; sp.room = rooms[i];
            em.spawnPoints.Add(sp);
            temp.cells.Add(spawn);
            Vector3Int from, to;
            rooms[i].SqrDistance(temp, out from, out to);
            DrawCorridor(from, to);
        }
        rooms.Add(start);*/
    }

    void Gen(int n) {
        for (int x = -size.x / 2; x < size.x / 2; x++) {
            for (int y = 0; y < size.y; y++) {
                TileBase tile = Random.Range(0, 2) == 0 ?
                    ground_full_0Tile : ground_full_1Tile;
                wallstilemap.SetTile(new Vector3Int(x, -y, 0), tile);
                darktilemap.SetTile(new Vector3Int(x, -y, 0), darkTile);
                darktilemap.SetColor(new Vector3Int(x, -y, 0), new Color(0, 0, 0, 1));
            }
        }

        start = GenRoomRandRect(8, 12);
        Vector3Int soff = new Vector3Int(0, -9, 0);
        for (int j = 0; j < start.cells.Count; j++) { start.cells[j] += soff; }
        rooms.Add(start); n -= 1;

        for (int i = 0; i < n; i++) {
            Room r = null;
            int iter = 0;
            for (; iter < 500; iter++) {
                r = GenRoomRandRect(Random.Range(4, 8));
                Vector3Int off = new Vector3Int(
                    Random.Range(0, size.x - 10) - (size.x - 20) / 2,
                    -5 - Random.Range(0, size.y - 10), 0);
                for (int j = 0; j < r.cells.Count; j++) { r.cells[j] += off; }

                float mind = float.PositiveInfinity;
                Vector3Int from, to;
                foreach (Room oth in rooms) {
                    float dist = r.SqrDistance(oth, out from, out to);
                    mind = Mathf.Min(mind, dist);
                }
                if (mind < 5 * 5 && mind > 3 * 3) {
                    break; // success
                }
            }
            if (r != null || iter < 100) {
                rooms.Add(r);
            }
        }

        foreach (Room r in rooms) {
            foreach (Vector3Int pt in r.cells) {
                if (wallstilemap.HasTile(pt)) {
                    wallstilemap.SetTile(pt, null);
                    groundtilemap.SetTile(pt, ground_roomTile);
                }
            }
        }

        ConnectRooms(rooms);
        GroundCorridors(rooms, 3);
        /*
        List<Vector3Int> mossy = start.Contour();
        List<Vector3Int> notMossy = new List<Vector3Int>();
        foreach (Vector3Int cell in start.cells) {
            if (mossy.Contains(cell)) {
                notMossy.Add(cell);
            }
        }

        Vector3Int pt = notMossy[Random.Range(0, notMossy.Count-1)];
        */
        FindObjectOfType<PlantManagement>().Spawn(soff + new Vector3Int(0, 1, 0), 0);
        FindObjectOfType<PlantManagement>().Spawn(soff, 2);
    }

    void Start() {
        groundtilemap = GameObject.Find("GroundTM").GetComponent<Tilemap>();
        wallstilemap = GameObject.Find("WallsTM").GetComponent<Tilemap>();
        darktilemap = GameObject.Find("DarkTM").GetComponent<Tilemap>();
        Gen(30);

        CoverWithMoss(start);
    }

    public void UncoverRoom(Room room) {
        List<Vector3Int> exp = Expand(room.cells);
        List<Vector3Int> exp2 = Expand(exp);
        foreach (Vector3Int cell in exp2) {
            darktilemap.SetColor(cell, new Color(0, 0, 0, 0.5f));
        }
        foreach (Vector3Int cell in exp) {
            darktilemap.SetColor(cell, new Color(0, 0, 0, 0.25f));
        }
        foreach (Vector3Int cell in room.cells) {
            darktilemap.SetColor(cell, new Color(0, 0, 0, 0));
        }
    }

    public void CoverWithMoss(Room room) {
        room.hasMoss = true;

        UncoverRoom(room);
        foreach (Room n in room.neighbors) {
            UncoverRoom(n);
        }

        List<Rule> rules = new List<Rule>();
        {
            Rule rule = new Rule();
            rule.match = new int[9] {
                0, 2, 0,
                1, 1, 1,
                0, 0, 0
            };
            rule.tile = new TileBase[] { moss_0Tile, moss_1Tile, moss_2Tile, };
            rule.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 0f), Vector3.one);
            rules.Add(rule);
        }
        {
            Rule rule = new Rule();
            rule.match = new int[9] {
                0, 1, 2,
                0, 1, 1,
                0, 0, 0
            };
            rule.tile = new TileBase[] { moss_cornerTile, };
            rule.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 0f), Vector3.one);
            rules.Add(rule);
        }
        {
            Rule rule = new Rule();
            rule.match = new int[9] {
                0, 0, 2,
                1, 1, 0,
                0, 1, 0
            };
            rule.tile = new TileBase[] { moss_corner_innerTile, };
            rule.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 0f), Vector3.one);
            rules.Add(rule);
        }

        List<Rule> rotatedRules = new List<Rule>();
        foreach (Rule rule in rules) {
            int[] rot = new int[9];
            rule.match.CopyTo(rot, 0);
            for (int i = 0; i < 4; i++) {
                Rule n = new Rule();
                n.match = new int[9];
                rot.CopyTo(n.match, 0);
                n.tile = rule.tile;
                n.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -90f * i), Vector3.one);
                rotatedRules.Add(n);
                rot = new int[9] {
                    rot[2], rot[5], rot[8],
                    rot[1], rot[4], rot[7],
                    rot[0], rot[3], rot[6]
                };
            }
        }
        List<Vector3Int> border = room.Contour();
        foreach (Vector3Int b in border) {
            foreach (Rule rule in rotatedRules) {
                bool found = true;
                for (int j = -1; j < 2; j++) {
                    for (int i = -1; i < 2; i++) { 
                        Vector3Int see = b + new Vector3Int(i, j, 0);
                        int xy = i + 1 + (j + 1) * 3;
                        if (rule.match[xy] == 1) {
                            if (!border.Contains(see)) {
                                found = false;
                                break;
                            }
                        }
                        if (rule.match[xy] == 2) {
                            if (room.cells.Contains(see)) {
                                found = false;
                                break;
                            }
                        }
                    }
                }
                if (found) {
                    TileBase tile = rule.tile[Random.Range(0, rule.tile.Length - 1)];
                    groundtilemap.SetTile(b + new Vector3Int(0, 0, 1), tile);
                    groundtilemap.SetTransformMatrix(b + new Vector3Int(0, 0, 1), rule.matrix);
                    break;
                }
            }
        }

        foreach(Vector3Int cell in room.cells) {
            groundtilemap.SetTile(cell, ground_mossTile);
        }
    }
}
