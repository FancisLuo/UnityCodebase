using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private Vector2Int boardSize = new Vector2Int(11, 11);
    [SerializeField] private GameBoard board = default;
    [SerializeField] private GameTileContentFactory tileContentFactory = default;
    [SerializeField] private EnemyFactory enemyFactory = default;
    [SerializeField, Range(0.1f, 10f)] private float spawnSpeed = 1.0f;

    private Ray touchRay => Camera.main.ScreenPointToRay(Input.mousePosition);

    private float spawnProgress;

    private EnemyCollection enemies = new EnemyCollection();

    private void Awake()
    {
        OnValidate();

        board.Initialize(boardSize, tileContentFactory);
        board.ShowGrid = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            HandleTouch();
        }
        else if(Input.GetMouseButtonDown(1))
        {
            HandleAlternativeTouch();
        }

        if(Input.GetKeyDown(KeyCode.V))
        {
            board.ShowPaths = !board.ShowPaths;
        }

        if(Input.GetKeyDown(KeyCode.G))
        {
            board.ShowGrid = !board.ShowGrid;
        }

        spawnProgress += spawnSpeed * Time.deltaTime;
        while(spawnProgress >= 1f)
        {
            spawnProgress -= 1f;
            SpawnEnemy();
        }

        enemies.GameUpdate();
        Physics.SyncTransforms();
        board.GameUpdate();
    }

    private void HandleTouch()
    {
        GameTile tile = board.GetTile(touchRay);
        if(tile != null)
        {
            //tile.Content = tileContentFactory.Get(GameTileContentType.Destination);
            //board.ToggleDestination(tile);
            if (Input.GetKey(KeyCode.LeftShift))
            {
                board.ToggleTower(tile);
            }
            else
            {
                board.ToggleWall(tile);
            }
        }
    }

    private void  HandleAlternativeTouch()
    {
        GameTile tile = board.GetTile(touchRay);
        if(tile != null)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                board.ToggleDestination(tile);
            }
            else
            {
                board.ToggleSpawnPoint(tile);
            }
        }
    }

    private void OnValidate()
    {
        if(boardSize.x <= 2)
        {
            boardSize.x = 2;
        }

        if(boardSize.y <= 2)
        {
            boardSize.y = 2;
        }
    }

    private void SpawnEnemy()
    {
        GameTile spawnPoint = board.GetSpawnPoint(Random.Range(0, board.SpawnPointCount));
        Enemy enemy = enemyFactory.Get();
        enemy.SpawnOn(spawnPoint);

        enemies.Add(enemy);
    }
}
