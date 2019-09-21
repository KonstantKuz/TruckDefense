﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using StateMachine;

public static class EnemyStateDictionary
{
    public static Dictionary<GameEnums.EnemyFollowType, State<Enemy>> stateDictionary { get; private set; }

    public static void CreateStateDictionary()
    {
        stateDictionary = new Dictionary<GameEnums.EnemyFollowType, State<Enemy>>(5);
        stateDictionary.Add(GameEnums.EnemyFollowType.PathFollow, PathFollowState.Instance);
        stateDictionary.Add(GameEnums.EnemyFollowType.PlayerFollow, PlayerFollowState.Instance);
        stateDictionary.Add(GameEnums.EnemyFollowType.SingleTest, SingleTestState.Instance);
    }
}
[RequireComponent(typeof(Truck))]
[DisallowMultipleComponent]
[SelectionBase]
public class Enemy : MonoCached, INeedTarget
{
    
    public GameEnums.EnemyFollowType followType;

    [SerializeField]
    private Transform sensorsPosition;

    [SerializeField]
    private float sensorsLength = 5f, sideSensorsOffset = 1f, sensorsAngle = 15f;

    public Truck truck { get; set; }

    //для объезда препятствий
    private Vector3 sensorsStartPos, sensorsRSidePos, sensorsLSidePos, sideSensorsOffsetVec;

    private bool avoiding = false;

    public List<Node> path { get; set; }
    public Node currentNode { get; set; }
    public int targetIndex { get; set; }
    
    //для стрельбы и преследования игрока
    public TargetData targetData { get; private set; }

    public StateMachine<Enemy> enemyState { get; private set; }

    #region UnityCallbacks

    private void Awake()
    {
        truck = GetComponent<Truck>();
        EnemyStateDictionary.CreateStateDictionary();
        enemyState = new StateMachine<Enemy>(this);
        truck.SetUpTruck();

    }

    private void OnEnable()
    {
        allTicks.Add(this);
        enemyState.ChangeState(EnemyStateDictionary.stateDictionary[followType]);
    }
    private void OnDisable()
    {
        allTicks.Remove(this);
    }

    private void Start()
    {
        sideSensorsOffsetVec = sensorsPosition.right * sideSensorsOffset;
        sensorsStartPos = sensorsPosition.position;
        sensorsRSidePos = sensorsStartPos + sideSensorsOffsetVec;
        sensorsLSidePos = sensorsStartPos - sideSensorsOffsetVec;
    }

    #endregion

    #region My

    public void InjectNewTargetData(Rigidbody targetRigidbody)
    {
        targetData = new TargetData(targetRigidbody);

        for (int i = 0; i < truck.firePoint.FirstTrackingGroupGuns.Count; i++)
        {
            truck.firePoint.FirstTrackingGroupGuns[i].SetTargetData(targetData);
        }
    }

    public override void OnTick()
    {
        enemyState.UpdateMachine();
    }
    
    public void ReTracePath(List<Node> newPath)
    {
        path = newPath;
        targetIndex = 0;
        if(PathCheck())
        {
            currentNode = path[targetIndex];
        }
    }
    
    public bool PathCheck()
    {
        return (!ReferenceEquals(path, null) && path.Count > 0);
    }

    public float AvoidForce()
    {

        sensorsStartPos = truck._transform.TransformPoint(sensorsPosition.localPosition);
        sensorsRSidePos = sensorsStartPos + sideSensorsOffsetVec;
        sensorsLSidePos = sensorsStartPos - sideSensorsOffsetVec;
        float avoidForce = 0;
        RaycastHit hit;

        for (int i = 0; i < 3; i++)
        {
            if (Physics.Raycast(sensorsRSidePos, Quaternion.AngleAxis(sensorsAngle/2+ i * sensorsAngle, sensorsPosition.up) * sensorsPosition.forward, out hit, sensorsLength))
            {
                avoiding = true;
                Debug.DrawLine(sensorsRSidePos, hit.point, Color.red);
                avoidForce -= 0.2f;
            }
            else { avoiding = false; }

            if (Physics.Raycast(sensorsLSidePos, Quaternion.AngleAxis(-sensorsAngle/2+i * -sensorsAngle, sensorsPosition.up) * sensorsPosition.forward, out hit, sensorsLength))
            {
                avoiding = true;
                Debug.DrawLine(sensorsLSidePos, hit.point, Color.red);
                avoidForce += 0.2f;
            }
            else { avoiding = false; }
        }

        return avoidForce;

    }

    private void OnDrawGizmos()
    {
        if (path != null)
        {
            for (int i = 0; i < path.Count; i++)
            {

                Gizmos.color = Color.green;
                if (path[i].worldPosition == currentNode.worldPosition)
                {
                    Gizmos.color = Color.red;
                }
                Gizmos.DrawSphere(path[i].worldPosition, 0.7f);
            }
        }
    }
    #endregion
}
