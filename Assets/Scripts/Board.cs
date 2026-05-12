
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Board : MonoBehaviour
{
    [Header("Настройки сетки")]
    public int width = 6;
    public int height = 6;
    public GameObject tilePrefab;
    public int tileSize = 80;

    [Header("Настройки игры")]
    public int tileTypes = 5;
    public float swapDuration = 0.1f;

    private Tile[,] tiles;
    private Tile selectedTile = null;
    private bool isProcessing = false;
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        GenerateBoard();
    }

    void GenerateBoard()
    {
        // Очищаем старые тайлы
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        tiles = new Tile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject newTileObj = Instantiate(tilePrefab, transform);
                RectTransform rect = newTileObj.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(x * tileSize, y * tileSize);
                rect.sizeDelta = new Vector2(tileSize - 4, tileSize - 4);

                Tile tile = newTileObj.GetComponent<Tile>();
                int randomType = Random.Range(0, tileTypes);
                tile.Initialize(x, y, randomType);

                tiles[x, y] = tile;
            }
        }

        RemoveInitialMatches();
    }

    void RemoveInitialMatches()
    {
        bool hasMatches = true;
        int maxAttempts = 50;
        int attempts = 0;

        while (hasMatches && attempts < maxAttempts)
        {
            hasMatches = false;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    List<Tile> matches = GetMatchAt(x, y);
                    if (matches.Count >= 3)
                    {
                        hasMatches = true;
                        int newType = Random.Range(0, tileTypes);
                        tiles[x, y].type = newType;
                        tiles[x, y].UpdateSprite();
                    }
                }
            }
            attempts++;
        }
    }

    public void OnTileClicked(Tile clickedTile)
    {
        if (isProcessing) return;

        if (selectedTile == null)
        {
            selectedTile = clickedTile;
            selectedTile.SetSelected(true);
            return;
        }

        if (selectedTile == clickedTile)
        {
            selectedTile.SetSelected(false);
            selectedTile = null;
            return;
        }

        bool isAdjacent = (Mathf.Abs(selectedTile.x - clickedTile.x) + Mathf.Abs(selectedTile.y - clickedTile.y)) == 1;

        if (!isAdjacent)
        {
            selectedTile.SetSelected(false);
            selectedTile = clickedTile;
            selectedTile.SetSelected(true);
            return;
        }

        StartCoroutine(TrySwap(selectedTile, clickedTile));

        selectedTile.SetSelected(false);
        selectedTile = null;
    }

    IEnumerator TrySwap(Tile tileA, Tile tileB)
    {
        isProcessing = true;

        SwapTiles(tileA, tileB);
        yield return new WaitForSeconds(swapDuration);

        List<Tile> matches = GetAllMatches();

        if (matches.Count > 0)
        {
            if (gameManager != null) gameManager.PlaySuccessSound();
            yield return StartCoroutine(ProcessMatchesAndCascade(matches));
        }
        else
        {
            if (gameManager != null) gameManager.PlayFailSound();
            SwapTiles(tileA, tileB);
            yield return new WaitForSeconds(swapDuration);
        }

        isProcessing = false;

        if (!HasAnyMoves())
        {
            yield return new WaitForSeconds(0.5f);
            ShuffleBoard();
        }
    }

    void SwapTiles(Tile tileA, Tile tileB)
    {
        // Меняем в массиве
        tiles[tileA.x, tileA.y] = tileB;
        tiles[tileB.x, tileB.y] = tileA;

        // Меняем координаты
        int tempX = tileA.x, tempY = tileA.y;
        tileA.x = tileB.x;
        tileA.y = tileB.y;
        tileB.x = tempX;
        tileB.y = tempY;

        // Анимация перемещения
        StartCoroutine(AnimateSwap(tileA, tileB));
    }

    IEnumerator AnimateSwap(Tile tileA, Tile tileB)
    {
        RectTransform rectA = tileA.GetComponent<RectTransform>();
        RectTransform rectB = tileB.GetComponent<RectTransform>();

        Vector2 posA = rectA.anchoredPosition;
        Vector2 posB = rectB.anchoredPosition;

        float elapsed = 0;
        while (elapsed < swapDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / swapDuration;
            rectA.anchoredPosition = Vector2.Lerp(posA, posB, t);
            rectB.anchoredPosition = Vector2.Lerp(posB, posA, t);
            yield return null;
        }

        rectA.anchoredPosition = posB;
        rectB.anchoredPosition = posA;
    }

    List<Tile> GetMatchAt(int x, int y)
    {
        List<Tile> matches = new List<Tile>();
        if (tiles[x, y].type == -1) return matches;

        int currentType = tiles[x, y].type;

        // Горизонталь
        int left = x, right = x;
        while (left > 0 && tiles[left - 1, y].type == currentType) left--;
        while (right < width - 1 && tiles[right + 1, y].type == currentType) right++;

        if (right - left + 1 >= 3)
        {
            for (int i = left; i <= right; i++)
                if (!matches.Contains(tiles[i, y]))
                    matches.Add(tiles[i, y]);
        }

        // Вертикаль
        int down = y, up = y;
        while (down > 0 && tiles[x, down - 1].type == currentType) down--;
        while (up < height - 1 && tiles[x, up + 1].type == currentType) up++;

        if (up - down + 1 >= 3)
        {
            for (int i = down; i <= up; i++)
                if (!matches.Contains(tiles[x, i]))
                    matches.Add(tiles[x, i]);
        }

        return matches;
    }

    List<Tile> GetAllMatches()
    {
        List<Tile> allMatches = new List<Tile>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (tiles[x, y].type != -1)
                {
                    List<Tile> matches = GetMatchAt(x, y);
                    foreach (Tile tile in matches)
                    {
                        if (!allMatches.Contains(tile))
                            allMatches.Add(tile);
                    }
                }
            }
        }

        return allMatches;
    }

    IEnumerator ProcessMatchesAndCascade(List<Tile> matches)
    {
        int points = matches.Count * 10;
        if (gameManager != null) gameManager.AddScore(points);

        foreach (Tile tile in matches)
        {
            tile.type = -1;
            tile.UpdateSprite();
        }

        yield return new WaitForSeconds(0.1f);

        bool changed = true;
        while (changed)
        {
            changed = false;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (tiles[x, y].type == -1)
                    {
                        for (int y2 = y + 1; y2 < height; y2++)
                        {
                            if (tiles[x, y2].type != -1)
                            {
                                Tile temp = tiles[x, y];
                                tiles[x, y] = tiles[x, y2];
                                tiles[x, y2] = temp;

                                tiles[x, y].x = x;
                                tiles[x, y].y = y;
                                tiles[x, y2].x = x;
                                tiles[x, y2].y = y2;

                                StartCoroutine(AnimateMove(tiles[x, y], new Vector2(x * tileSize, y * tileSize)));
                                changed = true;
                                break;
                            }
                        }
                    }
                }
            }

            yield return new WaitForSeconds(0.05f);
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (tiles[x, y].type == -1)
                {
                    int newType = Random.Range(0, tileTypes);
                    tiles[x, y].type = newType;
                    tiles[x, y].UpdateSprite();
                }
            }
        }

        List<Tile> newMatches = GetAllMatches();
        if (newMatches.Count > 0)
        {
            yield return new WaitForSeconds(0.1f);
            yield return StartCoroutine(ProcessMatchesAndCascade(newMatches));
        }
    }

    IEnumerator AnimateMove(Tile tile, Vector2 targetPos)
    {
        RectTransform rect = tile.GetComponent<RectTransform>();
        Vector2 startPos = rect.anchoredPosition;
        float duration = 0.08f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, elapsed / duration);
            yield return null;
        }

        rect.anchoredPosition = targetPos;
    }

    bool HasAnyMoves()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x < width - 1)
                {
                    SwapTest(x, y, x + 1, y);
                    if (GetAllMatches().Count > 0) return true;
                    SwapTest(x + 1, y, x, y);
                }
                if (y < height - 1)
                {
                    SwapTest(x, y, x, y + 1);
                    if (GetAllMatches().Count > 0) return true;
                    SwapTest(x, y + 1, x, y);
                }
            }
        }
        return false;
    }

    void SwapTest(int x1, int y1, int x2, int y2)
    {
        int tempType = tiles[x1, y1].type;
        tiles[x1, y1].type = tiles[x2, y2].type;
        tiles[x2, y2].type = tempType;
    }

    public void ShuffleBoard()
    {
        List<int> allTypes = new List<int>();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                allTypes.Add(tiles[x, y].type);

        allTypes = allTypes.OrderBy(t => Random.Range(0, 100)).ToList();

        int index = 0;
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                tiles[x, y].type = allTypes[index++];

        List<Tile> matches = GetAllMatches();
        if (matches.Count > 0)
        {
            StartCoroutine(ProcessMatchesAndCascade(matches));
        }
    }
}