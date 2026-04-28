using UnityEngine;

public class TileGrid : MonoBehaviour {
    public TileRow[]  rows  { get; private set; }
    public TileCell[] cells { get; private set; }

    public int Size   => cells.Length;       // Tổng số ô (16)
    public int Height => rows.Length;        // Số hàng (4)
    public int Width  => Size / Height;      // Số cột (4)

    private void Awake() {
        rows  = GetComponentsInChildren<TileRow>();
        cells = GetComponentsInChildren<TileCell>();

        // Gán tọa độ (x, y) cho từng ô theo thứ tự
        for (int i = 0; i < cells.Length; i++)
            cells[i].coordinates = new Vector2Int(i % Width, i / Width);
    }

    public TileCell GetCell(Vector2Int coordinates) => GetCell(coordinates.x, coordinates.y);

    // Trả về null nếu tọa độ ngoài lưới
    public TileCell GetCell(int x, int y) =>
        (x >= 0 && x < Width && y >= 0 && y < Height) ? rows[y].cells[x] : null;

    // Lấy ô kề theo hướng (y đảo vì Unity UI tọa độ ngược)
    public TileCell GetAdjacentCell(TileCell cell, Vector2Int direction) {
        Vector2Int coords = cell.coordinates;
        coords.x += direction.x;
        coords.y -= direction.y; // Trừ vì trục Y của UI ngược với Vector2Int
        return GetCell(coords);
    }

    // Tìm ô trống ngẫu nhiên bằng vòng lặp tuyến tính, trả null nếu bàn đầy
    public TileCell GetRandomEmptyCell() {
        int index = Random.Range(0, cells.Length);
        int start = index;
        while (cells[index].Occupied) {
            index = (index + 1) % cells.Length;
            if (index == start) return null; // Đã duyệt hết, bàn đầy
        }
        return cells[index];
    }
}
