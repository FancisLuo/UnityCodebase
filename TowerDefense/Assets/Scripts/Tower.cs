using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : GameTileContent
{
    [SerializeField, Range(1.5f, 10.5f)]
    private float targetingRange = 1.5f;

    private const int enemyLayerMask = 1 << 9;

    private TargetPoint target;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void GameUpdate()
    {
        Debug.Log("searching for target...");
        if(TrackTarget() || AcquireTarget())
        {
            //Debug.Log("Acquire target!");
            Debug.Log("Locked on target!");
        }
    }

    private bool AcquireTarget()
    {
        Vector3 a = transform.localPosition;
        Vector3 b = a;
        b.y += 3f;

        Collider[] targets = Physics.OverlapCapsule(/*transform.localPosition*/a, b, targetingRange, enemyLayerMask);
        if(targets != null && targets.Length > 0)
        {
            target = targets[0].GetComponent<TargetPoint>();

            Debug.Assert(target != null, "Targeted non-enemy!", targets[0]);
            return true;
        }

        target = null;
        return false;
    }

    private bool TrackTarget()
    {
        if(target == null)
        {
            return false;
        }

        //Vector3 a = transform.localPosition;
        //Vector3 b = target.Position;
        //if(Vector3.Distance(a, b) > (targetingRange + 0.125f * target.Enemy.Scale))
        Vector3 a = transform.localPosition;
        Vector3 b = target.Position;
        float x = a.x - b.x;
        float z = a.z - b.z;
        float r = targetingRange + 0.125f * target.Enemy.Scale;
        if(x * x + z * z > r * r)
        {
            target = null;
            return false;
        }

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 position = transform.localPosition;
        position.y += 0.01f;
        Gizmos.DrawWireSphere(position, targetingRange);

        if(target != null)
        {
            Gizmos.DrawLine(position, target.Position);
        }
    }
}
