using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }
    [SerializeField] TetrominoData[] tetrominoes;
    public Piece activePiece { get; private set; }
    public Piece nextPiece { get; private set; }

    Vector2 previewPiecePosition = new Vector2(-7.5f, 9.5f);

    [SerializeField] Vector3Int spawnPosition;

    public Vector2Int boardSize = new Vector2Int (10, 20);

    [SerializeField] int linesCleaned;
    public int level = 0;

    GameObject scoreText;

    AudioManager audioManager;

    [SerializeField] GameObject gameOverPanel;

    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int (-boardSize.x / 2, -boardSize.y / 2);
            return new RectInt(position, boardSize);
        }

    }

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        activePiece = GetComponentInChildren<Piece>();
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
        scoreText = GameObject.FindGameObjectWithTag("ScoreText");

        for (int i = 0; i < tetrominoes.Length; i++)
        {
            tetrominoes[i].Init();
        }
    }

    private void Start()
    {
        SpawnPiece();
    }

    private void Update()
    {
        UpdateLevel();
    }
    public void SpawnPiece()
    {
        int random = Random.Range(0, tetrominoes.Length);
        TetrominoData data = tetrominoes[random];

        activePiece.Init(this, spawnPosition, data);

        if(IsValidPosition(activePiece, spawnPosition))
        {
            Set(activePiece);
        }
        else
        {
            GameOver();
        }

        Set(this.activePiece);
    }

    private void GameOver()
    {
        audioManager.PlaySFX(audioManager.gameOverSound);
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }
    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = Bounds;
        for(int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            if(!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }

            if(tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }
        return true;
    }

    public void ClearLine()
    {
        RectInt bounds = Bounds;
        int row = bounds.yMin;

        while(row < bounds.yMax)
        {
            if (IsLineFull(row))
            {
                LineClear(row);
                
            }
            else
            {
                row++;
            }
        }
    }

    private void LineClear(int row)
    {
        RectInt bounds = Bounds;
        for(int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            tilemap.SetTile(position, null);
        }

        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                tilemap.SetTile(position, above);
            }
            row++;
        }

        audioManager.PlaySFX(audioManager.lineCompletedSound);
        linesCleaned++;
        scoreText.GetComponent<GameScore>().Score += 500;

    }

    private bool IsLineFull(int row)
    {
        RectInt bounds = Bounds;

        for(int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            if(!tilemap.HasTile(position))
            {
                return false;
            }
        }

        return true;
    }

    void UpdateLevel()
    {
        level = linesCleaned / 10;
    }


}

