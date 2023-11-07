using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{    public Board board {  get; private set; }
    public Vector3Int position { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public TetrominoData data { get; private set; }
    public int rotationIndex {  get; private set; }

    [SerializeField] float stepDelay = 1f;
    [SerializeField] float lockDelay = 0.5f;

    public float stepTime;
    private float lockTime;

    int touchSensitivityHorizontal = 8;
    int touchSensitivityVertical = 4;
    Vector2 previousUnitPosition = Vector2.zero;
    Vector2 direction = Vector2.zero;
    bool moved;

    AudioManager audioManager;

    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
    }
    public void Init(Board board, Vector3Int position, TetrominoData data)
    {
        this.board = board;
        this.position = position;
        this.data = data;

        stepTime = Time.time + stepDelay;
        lockTime = 0f;

        if(this.cells == null )
        {
            cells = new Vector3Int[data.cells.Length];
        }

        for ( int i = 0; i < data.cells.Length; i++ )
        {
            this.cells[i] = (Vector3Int)data.cells[i];
        }
    }
    private void Update()
    {
        board.Clear(this);

        lockTime += Time.deltaTime;
        if(!PauseMenu.isPaused) 
        {
            //Rotación
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Rotate(-1);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                Rotate(1);
            }

            //Movimiento ortogonal
            if (Input.GetKeyDown(KeyCode.A))
            {
                Move(Vector2Int.left);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                Move(Vector2Int.right);
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                Move(Vector2Int.down);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                HardDrop();
            }

            if (Time.time >= stepTime)
            {
                Step(); ;
            }

            if(Input.touchCount > 0)
            {
                Touch currentTouch = Input.GetTouch(0);
                if(currentTouch.phase == TouchPhase.Began)
                {
                    previousUnitPosition = new Vector2(currentTouch.position.x, currentTouch.position.y);
                }
                else if(currentTouch.phase == TouchPhase.Moved)
                {
                    Vector2 touchDeltaPosition = currentTouch.deltaPosition;
                    direction = touchDeltaPosition.normalized;

                    if (Mathf.Abs(currentTouch.position.x - previousUnitPosition.x) >= touchSensitivityHorizontal 
                        && direction.x < 0 && currentTouch.deltaPosition.y > -10 && currentTouch.deltaPosition.y < 10)
                    {
                        //Move Left
                        Move(Vector2Int.left);
                        previousUnitPosition = currentTouch.position;
                        moved = true;
                    }

                    else if(Mathf.Abs(currentTouch.position.x - previousUnitPosition.x) >= touchSensitivityVertical
                        && direction.y > 0 && currentTouch.deltaPosition.y > -10 && currentTouch.deltaPosition.y < 10)
                    {
                        //Move Right
                        Move(Vector2Int.right);
                        previousUnitPosition = currentTouch.position;
                        moved = true;
                    }

                    else if (Mathf.Abs(currentTouch.position.y - previousUnitPosition.y) >= touchSensitivityHorizontal
                        && direction.x < 0 && currentTouch.deltaPosition.x > -10 && currentTouch.deltaPosition.x < 10)
                    {
                        //Move Down
                        Move(Vector2Int.down);
                        previousUnitPosition = currentTouch.position;
                        moved = true;
                    }
                }
                else if(currentTouch.phase == TouchPhase.Ended)
                {
                    if(!moved && currentTouch.position.x > Screen.width / 4)
                    {
                        Rotate(1);
                    }
                    moved = false;
                }
            }
        }

        board.Set(this);

        UpdateFallingSpeed();
    }

    private void Step()
    {
        stepTime = Time.time + stepDelay;

        Move(Vector2Int.down);

        if(lockTime >= lockDelay)
        {
            Lock();
        }

    }
    private void UpdateFallingSpeed()
    {
        stepDelay = 1f - ((float)board.level * 0.1f);
    }

    private void Lock()
    {
        audioManager.PlaySFX(audioManager.pieceLockSound);
        board.Set(this);
        board.ClearLine();
        board.SpawnPiece();
    }

    private void HardDrop()
    {
        while(Move(Vector2Int.down))
        {
            continue;
        }
        Lock();
    }

    public bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = board.IsValidPosition(this, newPosition);

        if (valid)
        {
            position = newPosition;
            this.lockTime = 0f;
        }

        return valid;
    }

    private void Rotate(int direction)
    {
        int originalRotation = rotationIndex;
        rotationIndex += Wrap(rotationIndex + direction, 0, 4);

        ApplyRotationMatrix(direction);

        if(!TestWallKicks(rotationIndex, direction))
        {
            rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }

    void ApplyRotationMatrix(int direction)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3 cell = cells[i];

            int x = 0;
            int y = 0;

            switch (data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                    break;

                default:
                    x = Mathf.RoundToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                    break;
            }

            cells[i] = new Vector3Int(x, y, 0);

        }
    }

    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);
        for(int i = 0; i < data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = data.wallKicks[wallKickIndex, i];
            if(Move(translation))
            {
                return true;
            }
        }

        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;

        if( rotationDirection < 0 )
        {
            wallKickIndex--;
        }
        return Wrap(wallKickIndex, 0, data.wallKicks.GetLength(0));
    }

    private int Wrap(int input, int min, int max)
    {
        if(input < min)
        {
            return max - (min - input) % (max - min);
        }
        else
        {
            return min + (input - min) % (max - min);
        }
    }
}
