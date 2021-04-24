using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room {
    public List<Vector3Int> cells = new List<Vector3Int>();
    public Vector2Int boundsTopLeft;
    public Vector2Int boundsBottomRight;
    public List<Room> neighbors = new List<Room>();
    public bool hasMoss = false;

    public bool Overlap (Room oth) {
        if (boundsTopLeft.x > oth.boundsBottomRight.x
            && boundsBottomRight.x < oth.boundsTopLeft.x
            && boundsTopLeft.y > oth.boundsBottomRight.y
            && boundsBottomRight.y < oth.boundsTopLeft.y)
        {
            return false;
        }
        foreach (Vector3Int apt in cells) {
            if (oth.cells.Contains(apt)) return true;
        }
        return false;
    }
    public float SqrDistance (Room oth, out Vector3Int from, out Vector3Int to) {
        float dist = float.PositiveInfinity;
        from = new Vector3Int(); to = new Vector3Int(); // c# 
        foreach (Vector3Int pt in cells) {
            foreach (Vector3Int othpt in oth.cells) {
                Vector2 diff = new Vector2(pt.x-othpt.x, pt.y-othpt.y);
                if (diff.sqrMagnitude < dist) {
                    dist = diff.sqrMagnitude;
                    from = pt; to = othpt;
                }
            }
        }
        return dist;
    }

    public List<Vector3Int> Contour () {
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
    public TileBase groundTile;
    public TileBase corridorTile;
    public TileBase roomTile;
    public TileBase mossTile;
    public Tilemap groundtilemap;
    public Tilemap wallstilemap;
    public List<Room> rooms = new List<Room>();
    public Room start;

    Room GenRoomRandRect(int size, int n=8) {
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

    void DrawCorridor (Vector3Int from, Vector3Int to) {
        Vector3Int[] dirToVec = new Vector3Int[4] {
            new Vector3Int(1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, -1, 0)
        };
        int iter = 0;
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
            groundtilemap.SetTile(from, corridorTile);
        }
        if (iter >= 100) { print("out of iteration"); }
    }

    void ConnectRooms (List<Room> rooms) {
        Room start = rooms[0];
        List<Room> conn = new List<Room>();
        conn.Add(start);
        int iter = 0;
        for (; iter < rooms.Count-1; iter+= 1) {
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

    void GroundCorridors (List<Room> rooms, int n) {
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
        rooms.Remove(start);

        rooms.Sort((a, b) => a.cells[0].y.CompareTo(b.cells[0].y));
        rooms.Reverse();
        for (int i=0; i<n; i++) {
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
        rooms.Add(start);
    }

    void Gen (int n) {
        for (int x = -size.x/2; x < size.x/2; x++) {
            for (int y = 0; y < size.y; y++) {
                wallstilemap.SetTile(new Vector3Int(x, -y, 0), groundTile);
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
                if (mind < 5*5 && mind > 3*3) {
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
                    groundtilemap.SetTile(pt, roomTile);
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
        Gen(24);

        CoverWithMoss(start);
    }

    public void CoverWithMoss (Room room) {
        room.hasMoss = true;
        List<Vector3Int> border = room.Contour();
        foreach (Vector3Int b in border) {
            groundtilemap.SetTile(b + new Vector3Int(0, 0, 1), mossTile);
        }
    }
}
