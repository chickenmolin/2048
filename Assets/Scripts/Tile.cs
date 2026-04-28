using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour {
    public TileState state { get; private set; } // Trạng thái: số, màu sắc
    public TileCell cell  { get; private set; }  // Ô đang chiếm
    public bool locked    { get; set; }           // Khóa để không merge 2 lần/lượt

    private Image background;
    private TextMeshProUGUI text;

    // Cập nhật màu nền, màu chữ, số hiển thị
    public void SetState(TileState state) {
        this.state        = state;
        background.color  = state.backgroundColor;
        text.color        = state.textColor;
        text.text         = state.number.ToString();
    }

    // Xuất hiện tại ô, gán liên kết 2 chiều Tile ↔ Cell
    public void Spawn(TileCell cell) {
        if (this.cell != null) this.cell.tile = null; // Hủy liên kết ô cũ
        this.cell      = cell;
        this.cell.tile = this;
        transform.position = cell.transform.position;
    }

    // Di chuyển đến ô mới có animation
    public void MoveTo(TileCell cell) {
        if (this.cell != null) this.cell.tile = null;
        this.cell      = cell;
        this.cell.tile = this;
        StartCoroutine(Animate(cell.transform.position, false));
    }

    // Merge vào ô khác: hủy liên kết rồi tự hủy sau animation
    public void Merge(TileCell cell) {
        if (this.cell != null) this.cell.tile = null;
        this.cell          = null;
        cell.tile.locked   = true; // Khóa tile đích để không merge tiếp
        StartCoroutine(Animate(cell.transform.position, true));
    }

    // Lerp vị trí trong 0.1s, nếu merge thì Destroy sau khi đến nơi
    private IEnumerator Animate(Vector3 to, bool merging) {
        float elapsed = 0f, duration = 0.1f;
        Vector3 from = transform.position;
        while (elapsed < duration) {
            transform.position = Vector3.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = to;
        if (merging) Destroy(gameObject);
    }
}
