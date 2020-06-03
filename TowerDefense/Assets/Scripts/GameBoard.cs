using System.Collections.Generic;
using UnityEngine;
using System;

public class GameBoard : MonoBehaviour
{
    [SerializeField] private Transform ground = default;
    [SerializeField] private GameTile tilePrefab = default;
    [SerializeField] private Texture2D gridTexture = default;

    private Vector2Int size;
    private GameTile[] tiles;

    private Queue<GameTile> searchFrontier = new Queue<GameTile>();

    private GameTileContentFactory contentFactory;

    private bool showPaths;
    private bool showGrid;

    private List<GameTile> spawnPoints = new List<GameTile>();

    private List<GameTileContent> updatingContent = new List<GameTileContent>();

    public bool ShowPaths
    {
        get => showPaths;
        set
        {
            showPaths = value;
            if(showPaths)
            {
                foreach(GameTile tile in tiles)
                {
                    tile.ShowPath();
                }
            }
            else
            {
                foreach(GameTile tile in tiles)
                {
                    tile.HidePath();
                }
            }
        }
    }

    public bool ShowGrid
    {
        get => showGrid;
        set
        {
            showGrid = value;
            Material material = ground.GetComponent<MeshRenderer>().material;
            if(showGrid)
            {
                material.mainTexture = gridTexture;
                material.SetTextureScale("_MainTex", size);
            }
            else
            {
                material.mainTexture = null;
            }
        }
    }

    public int SpawnPointCount => spawnPoints.Count;

    public void Initialize(Vector2Int size, GameTileContentFactory contentFactory)
    {
        this.size = size;
        this.contentFactory = contentFactory;
        ground.localScale = new Vector3(size.x, size.y, 1f);

        // get center
        var offset = new Vector2((size.x - 1) * 0.5f, (size.y - 1) * 0.5f);

        tiles = new GameTile[size.x * size.y];
        for (int i = 0, y = 0; y < size.y; y++)
        {
            for (var x = 0; x < size.x; x++, i++)
            {
                GameTile tile = tiles[i] = Instantiate(tilePrefab);
                tile.transform.SetParent(transform, false);
                tile.transform.localPosition = new Vector3(x - offset.x, 0f, y - offset.y);

                if (x > 0)
                {
                    GameTile.MakeEastWestneighbors(tile, tiles[i - 1]);
                }

                if (y > 0)
                {
                    GameTile.MakeNorthSouthNeighbors(tile, tiles[i - size.x]);
                }

                tile.IsAlternative = (x & 1) == 0;
                if((y & 1) == 0)
                {
                    tile.IsAlternative = !tile.IsAlternative;
                }

                tile.Content = contentFactory.Get(GameTileContentType.Empty);
            }
        }

        //FindPaths();
        ToggleDestination(tiles[tiles.Length / 2]);
        ToggleSpawnPoint(tiles[0]);
    }

    public void GameUpdate()
    {
        for(int i = 0;i < updatingContent.Count;i++)
        {
            updatingContent[i].GameUpdate();
        }
    }

    public GameTile GetTile(Ray ray)
    {
        if(Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, 1))
        {
            int x = (int)(hit.point.x + size.x * 0.5f);
            int y = (int)(hit.point.z + size.y * 0.5f);

            if(x >= 0 && x < size.x && y >= 0 && y < size.y)
            {
                return tiles[x + y * size.x];
            }
        }

        return null;
    }

    public GameTile GetSpawnPoint(int index)
    {
        if(index < 0 || index >= spawnPoints.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        return spawnPoints[index];
    }

    public void ToggleDestination(GameTile tile)
    {
        if(tile.Content.Type == GameTileContentType.Destination)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Empty);
            if(!FindPaths())
            {
                tile.Content = contentFactory.Get(GameTileContentType.Destination);
                FindPaths();
            }
        }
        else if(tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Destination);
            FindPaths();
        }
    }

    public void ToggleWall(GameTile tile)
    {
        if (tile.Content.Type == GameTileContentType.Wall)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Empty);
            FindPaths();
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Wall);
            if (!FindPaths())
            {
                tile.Content = contentFactory.Get(GameTileContentType.Empty);
                FindPaths();
            }
        }
    }

    public void ToggleSpawnPoint(GameTile tile)
    {
        if(tile.Content.Type == GameTileContentType.SpawnPoint)
        {
            if (spawnPoints.Count > 1)
            {
                spawnPoints.Remove(tile);
                tile.Content = contentFactory.Get(GameTileContentType.Empty);
            }
        }
        else if(tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = contentFactory.Get(GameTileContentType.SpawnPoint);
            spawnPoints.Add(tile);
        }
    }

    public void ToggleTower(GameTile tile)
    {
        if (tile.Content.Type == GameTileContentType.Tower)
        {
            updatingContent.Remove(tile.Content);
            tile.Content = contentFactory.Get(GameTileContentType.Empty);
            FindPaths();
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Tower);
            if (!FindPaths())
            {
                tile.Content = contentFactory.Get(GameTileContentType.Empty);
                FindPaths();
            }
            else
            {
                updatingContent.Add(tile.Content);
            }
        }
        else if(tile.Content.Type == GameTileContentType.Wall)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Tower);
            updatingContent.Add(tile.Content);
        }
    }


    private bool FindPaths()
    {
        foreach (var tile in tiles)
        {
            if (tile.Content.Type == GameTileContentType.Destination)
            {
                tile.BecameDestination();
                searchFrontier.Enqueue(tile);
            }
            else
            {
                tile.ClearPath();
            }
        }

        // non-reachable
        if(searchFrontier.Count == 0)
        {
            return false;
        }

        //tiles[tiles.Length/2].BecameDestination();
        //searchFrontier.Enqueue(tiles[tiles.Length / 2]);

        while (searchFrontier.Count > 0)
        {
            var tileSearch = searchFrontier.Dequeue();
            if (tileSearch != null)
            {
                if (tileSearch.IsAlternative)
                {
                    searchFrontier.Enqueue(tileSearch.GrowPathNorth());
                    searchFrontier.Enqueue(tileSearch.GrowPathSouth());
                    searchFrontier.Enqueue(tileSearch.GrowPathEast());
                    searchFrontier.Enqueue(tileSearch.GrowPathWest());
                }
                else
                {
                    searchFrontier.Enqueue(tileSearch.GrowPathWest());
                    searchFrontier.Enqueue(tileSearch.GrowPathEast());
                    searchFrontier.Enqueue(tileSearch.GrowPathSouth());
                    searchFrontier.Enqueue(tileSearch.GrowPathNorth());
                }
            }
        }

        foreach(GameTile tile in tiles)
        {
            if(!tile.HasPath)
            {
                return false;
            }
        }

        if (showPaths)
        {
            foreach (GameTile gameTile in tiles)
            {
                gameTile.ShowPath();
            }
        }

        return true;
    }
}
