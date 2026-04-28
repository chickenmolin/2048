using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBoard : MonoBehaviour {
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private TileState[] tileStates; // Mảng trạng thái: 2, 4, 8, 16...

    private TileGrid grid;
    private List<Tile> tiles;
    private bool waiting; // Khóa input khi đang chờ animation

    public void ClearBoard() {
        foreach (var cell in grid.cells) cell.tile = null;
        foreach (var tile in tiles) Destroy(tile.gameObject);
        tiles.Clear();
    }

    // Tạo tile mới (số 2) tại ô trống ngẫu nhiên
    public void CreateTile() {
        Tile tile = Instantiate(tilePrefab, grid.transform);
        tile.SetState(tileStates[0]);
        tile.Spawn(grid.GetRandomEmptyCell());
        tiles.Add(tile);
    }

    private void Update() {
        if (waiting) return; // Chờ animation xong mới nhận input tiếp

        // Mỗi hướng truyền tham số duyệt khác nhau để xử lý đúng thứ tự
        if      (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            Move(Vector2Int.up,    0,            1,  1,             1);
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            Move(Vector2Int.left,  1,            1,  0,             1);
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            Move(Vector2Int.down,  0,            1,  grid.Height-2, -1);
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            Move(Vector2Int.right, grid.Width-2, -1, 0,             1);
    }

    // Duyệt toàn bộ ô theo thứ tự phù hợp hướng di chuyển
    private void Move(Vector2Int direction, int startX, int incrementX, int startY, int incrementY) {
        bool changed = false;
        for (int x = startX; x >= 0 && x < grid.Width; x += incrementX)
            for (int y = startY; y >= 0 && y < grid.Height; y += incrementY) {
                TileCell cell = grid.GetCell(x, y);
                if (cell.Occupied)
                    changed |= MoveTile(cell.tile, direction);
            }
        if (changed) StartCoroutine(WaitForChanges());
    }

    // Đẩy tile đi xa nhất có thể, merge nếu gặp tile cùng số
    private bool MoveTile(Tile tile, Vector2Int direction) {
        TileCell newCell = null;
        TileCell adjacent = grid.GetAdjacentCell(tile.cell, direction);
        while (adjacent != null) {
            if (adjacent.Occupied) {
                if (CanMerge(tile, adjacent.tile)) { MergeTiles(tile, adjacent.tile); return true; }
                break;
            }
            newCell = adjacent;
            adjacent = grid.GetAdjacentCell(adjacent, direction);
        }
        if (newCell != null) { tile.MoveTo(newCell); return true; }
        return false;
    }

    // Merge khi cùng state và tile đích chưa bị khóa
    private bool CanMerge(Tile a, Tile b) => a.state == b.state && !b.locked;

    private void MergeTiles(Tile a, Tile b) {
        tiles.Remove(a);
        a.Merge(b.cell); // a trượt vào rồi tự hủy

        // Nâng state của b lên 1 bậc (VD: 2+2=4)
        TileState newState = tileStates[Mathf.Clamp(IndexOf(b.state) + 1, 0, tileStates.Length - 1)];
        b.SetState(newState);
        GameManager.Instance.IncreaseScore(newState.number);
    }

    private int IndexOf(TileState state) {
        for (int i = 0; i < tileStates.Length; i++)
            if (state == tileStates[i]) return i;
        return -1;
    }

    // Chờ 0.1s cho animation → mở khóa → tạo tile mới → kiểm tra thua
    private IEnumerator WaitForChanges() {
        waiting = true;
        yield return new WaitForSeconds(0.1f);
        waiting = false;
        foreach (var tile in tiles) tile.locked = false;
        if (tiles.Count != grid.Size) CreateTile();
        if (CheckForGameOver()) GameManager.Instance.GameOver();
    }

    // Thua khi bàn đầy và không còn cặp tile nào có thể merge
    public bool CheckForGameOver() {
        if (tiles.Count != grid.Size) return false;
        foreach (var tile in tiles) {
            if ((grid.GetAdjacentCell(tile.cell, Vector2Int.up)    != null && CanMerge(tile, grid.GetAdjacentCell(tile.cell, Vector2Int.up).tile))    ||
                (grid.GetAdjacentCell(tile.cell, Vector2Int.down)  != null && CanMerge(tile, grid.GetAdjacentCell(tile.cell, Vector2Int.down).tile))  ||
                (grid.GetAdjacentCell(tile.cell, Vector2Int.left)  != null && CanMerge(tile, grid.GetAdjacentCell(tile.cell, Vector2Int.left).tile))  ||
                (grid.GetAdjacentCell(tile.cell, Vector2Int.right) != null && CanMerge(tile, grid.GetAdjacentCell(tile.cell, Vector2Int.right).tile)))
                return false;
        }
        return true;
    }
}
