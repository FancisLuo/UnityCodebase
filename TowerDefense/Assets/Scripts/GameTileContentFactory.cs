using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.SceneManagement;

[CreateAssetMenu]
public class GameTileContentFactory : /*ScriptableObject*/GameObjectFactory
{
    [SerializeField] private GameTileContent destinationPrefab = default;
    [SerializeField] private GameTileContent emptyPrefab = default;
    [SerializeField] private GameTileContent wallPrefab = default;
    [SerializeField] private GameTileContent spawnPointPrefab = default;
    [SerializeField] private /*GameTileContent*/Tower towerPrefab = default;

    public void Reclaim(GameTileContent content)
    {
        Debug.Assert(content.OriginFactory == this, "Wrong factory reclaimed!");
        Destroy(content.gameObject);
    }

    private GameTileContent Get(GameTileContent prefab)
    {
        //GameTileContent instance = Instantiate(prefab);
        //instance.OriginFactory = this;
        //MoveToFactoryScene(instance.gameObject);

        GameTileContent instance = CreateGameObjectInstance<GameTileContent>(prefab);
        instance.OriginFactory = this;
        return instance;
    }

    public GameTileContent Get(GameTileContentType type)
    {
        switch(type)
        {
            case GameTileContentType.Destination:
                return Get(destinationPrefab);

            case GameTileContentType.Empty:
                return Get(emptyPrefab);

            case GameTileContentType.Wall:
                return Get(wallPrefab);

            case GameTileContentType.SpawnPoint:
                return Get(spawnPointPrefab);

            case GameTileContentType.Tower:
                return Get(towerPrefab);
        }

        return null;
    }

    /*
     * Move to GameObjectFactory
     */
    //private void MoveToFactoryScene(GameObject o)
    //{
    //    if(!contentScene.isLoaded)
    //    {
    //        if(Application.isEditor)
    //        {
    //            contentScene = SceneManager.GetSceneByName(name);
    //            if(!contentScene.isLoaded)
    //            {
    //                contentScene = SceneManager.CreateScene(name);
    //            }
    //        }
    //        else
    //        {
    //            contentScene = SceneManager.CreateScene(name);
    //        }
    //    }

    //    SceneManager.MoveGameObjectToScene(o, contentScene);
    //}
}
