using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Кнопки")]
    public Button shuffleButton;

    private Board board;

    void Start()
    {
        board = FindObjectOfType<Board>();

        if (shuffleButton != null)
            shuffleButton.onClick.AddListener(ShuffleBoard);
    }

    void ShuffleBoard()
    {
        if (board != null)
            board.ShuffleBoard();
    }
}