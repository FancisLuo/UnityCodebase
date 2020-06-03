using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private Transform model = default;

    private EnemyFactory originFactory;

    private GameTile tileFrom, tileTo;
    private Vector3 positionFrom, positionTo;
    private float progress, progressFactor;

    private Direction direction;
    private DirectionChange directionChange;
    private float directionAngleFrom, directionAngleTo;

    private float speed;
    private float pathOffset;

    public float Scale { get; private set; }

    public EnemyFactory OriginFactory
    {
        get => originFactory;
        set
        {
            Debug.Assert(originFactory == null, "Redefined origin factory!");
            originFactory = value;
        }
    }

    public void Initialize(float scale, float speed, float pathOffset)
    {
        Scale = scale;
        model.localScale = new Vector3(scale, scale, scale);
        this.speed = speed;
        this.pathOffset = pathOffset;
    }

    public void SpawnOn(GameTile tile)
    {
        Debug.Assert(tile != null);
        Debug.Assert(tile.NextTileOnPath != null, "Nowhere to go!", this);
        //transform.localPosition = tile.transform.localPosition;

        tileFrom = tile;
        tileTo = tile.NextTileOnPath;

        //positionFrom = tileFrom.transform.localPosition;
        ////positionTo = tileTo.transform.localPosition;
        //positionTo = tileFrom.ExitPoint;
        //transform.localRotation = tileFrom.PathDirection.GetRotation();

        progress = 0f;

        PrepareIntro();
    }

    public bool GameUpdate()
    {
        // shift position data when progress complete
        progress += Time.deltaTime * progressFactor;
        while(progress >= 1f)
        {
            //tileFrom = tileTo;
            //tileTo = tileTo.NextTileOnPath;

            if(tileTo == null)
            {
                OriginFactory.Reclaim(this);
                return false;
            }

            //positionFrom = positionTo;
            ////positionTo = tileTo.transform.localPosition;
            //positionTo = tileFrom.ExitPoint;
            //transform.localRotation = tileFrom.PathDirection.GetRotation();

            progress = (progress - 1f) / progressFactor;
            PrepareNextState();
            progress *= progressFactor;
        }

        //transform.localPosition += Vector3.forward * Time.deltaTime;
        // Move by progress        
        //transform.localPosition = Vector3.LerpUnclamped(positionFrom, positionTo, progress);

        //if(directionChange != DirectionChange.None)
        //{
        //    float angle = Mathf.LerpUnclamped(directionAngleFrom, directionAngleTo, progress);
        //    transform.localRotation = Quaternion.Euler(0f, angle, 0f);
        //}
        if (directionChange == DirectionChange.None)
        {
            transform.localPosition = Vector3.LerpUnclamped(positionFrom, positionTo, progress);
        }
        else
        {
            float angle = Mathf.LerpUnclamped(directionAngleFrom, directionAngleTo, progress);
            transform.localRotation = Quaternion.Euler(0f, angle, 0f);
        }

        return true;
    }

    private void PrepareIntro()
    {
        positionFrom = tileFrom.transform.localPosition;
        positionTo = tileFrom.ExitPoint;
        direction = tileFrom.PathDirection;
        directionChange = DirectionChange.None;
        directionAngleFrom = directionAngleTo = direction.GetAngle();
        transform.localPosition = new Vector3(pathOffset, 0f);
        transform.localRotation = tileFrom.PathDirection.GetRotation();

        progressFactor = 2f * speed;
    }

    private void PrepareOutro()
    {
        positionTo = tileFrom.transform.localPosition;
        directionChange = DirectionChange.None;
        directionAngleTo = direction.GetAngle();
        model.localPosition = new Vector3(pathOffset, 0f)/*Vector3.zero*/;
        transform.localRotation = direction.GetRotation();
        progressFactor = 2f * speed;
    }

    private void PrepareNextState()
    {
        tileFrom = tileTo;
        tileTo = tileTo.NextTileOnPath;
        positionFrom = positionTo;
        if(tileTo == null)
        {
            PrepareOutro();
            return;
        }

        positionTo = tileFrom.ExitPoint;
        directionChange = direction.GetDirectionChangeTo(tileFrom.PathDirection);
        direction = tileFrom.PathDirection;
        directionAngleFrom = directionAngleTo;

        switch(directionChange)
        {
            case DirectionChange.None:
                PrepareForward();
                break;

            case DirectionChange.TurnRight:
                PrepareTurnRight();
                break;

            case DirectionChange.TurnLeft:
                PrepareTurnLeft();
                break;

            default:
                PrepareTurnAround();
                break;
        }
    }

    private void PrepareForward()
    {
        transform.localRotation = direction.GetRotation();
        directionAngleTo = direction.GetAngle();

        model.localPosition = new Vector3(pathOffset, 0f)/*Vector3.zero*/;
        progressFactor = speed;
    }

    private void PrepareTurnRight()
    {
        directionAngleTo = directionAngleFrom + 90f;

        model.localPosition = new Vector3(pathOffset - 0.5f, 0f);
        transform.localPosition = positionFrom + direction.GetHalfVector();
        progressFactor = speed / (Mathf.PI * /*0.25f*/0.5f * (0.5f - pathOffset));
    }

    private void PrepareTurnLeft()
    {
        directionAngleTo = directionAngleFrom - 90f;

        model.localPosition = new Vector3(pathOffset + 0.5f, 0f);
        transform.localPosition = positionFrom + direction.GetHalfVector();
        progressFactor = speed / (Mathf.PI * /*0.25f*/0.5f * (0.5f + pathOffset));
    }

    private void PrepareTurnAround()
    {
        directionAngleTo = directionAngleFrom + /*180f*/(pathOffset < 0f ? 180f : -180f);

        model.localPosition = new Vector3(pathOffset, 0f)/*Vector3.zero*/;
        transform.localPosition = positionFrom;
        progressFactor = /*2f*/speed / (Mathf.PI * Mathf.Max(Mathf.Abs(pathOffset), 0.2f));
    }
}
