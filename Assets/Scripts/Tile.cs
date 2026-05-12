using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerClickHandler
{
    public int x, y;
    public int type;
    public bool isSelected = false;

    private Image image;
    private Board board;

    [Header("Спрайты фруктов")]
    public Sprite[] fruitSprites;

    void Awake()
    {
        image = GetComponent<Image>();
        board = FindObjectOfType<Board>();
    }

    public void Initialize(int newX, int newY, int newType)
    {
        x = newX;
        y = newY;
        type = newType;
        UpdateSprite();
    }

    public void UpdateSprite()
    {
        if (fruitSprites != null && type >= 0 && type < fruitSprites.Length)
        {
            image.sprite = fruitSprites[type];
            image.enabled = true;
        }
        else if (type == -1)
        {
            image.enabled = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (board != null)
            board.OnTileClicked(this);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        image.color = selected ? new Color(1f, 1f, 0.7f) : Color.white;
    }
}