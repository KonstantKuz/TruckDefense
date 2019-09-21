﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Singleton;
public class InputHandler : MonoCached
{
    [SerializeField]
    private GameObject moveControllerPrefab, fireAreaPrefab, fwdBoost, bcwdBoost, prkngBrake;
    
    public LayerMask damageableMask;

    private Slider moveController;
    private RectTransform fireArea;
    private Button forwardBoost, backwardBoost, parkingBrake, restart;
    private Camera mainCamera;

    public Player player { get; set; }
    public float steeringForce { get; private set; }
    public bool doubleTap { get; private set; }
    
    public static bool GetKeyDown(KeyCode key)
    {
        return Input.GetKeyDown(key);
    }

    public void CreateControlsUI()
    {
        GameObject contr = Instantiate(moveControllerPrefab, GameObject.Find("UI").transform.GetChild(0).transform);
        moveController = contr.GetComponentInChildren<Slider>();
        GameObject fireAr = Instantiate(fireAreaPrefab, moveController.transform.parent, GameObject.Find("UI").transform.GetChild(0).transform);
        fireArea = fireAr.GetComponent<RectTransform>();

        GameObject fwdbst = Instantiate(fwdBoost, GameObject.Find("UI").transform.GetChild(0).transform);
        forwardBoost = fwdBoost.GetComponent<Button>();
        GameObject bcwdbst = Instantiate(bcwdBoost, GameObject.Find("UI").transform.GetChild(0).transform);
        backwardBoost = bcwdbst.GetComponent<Button>();

        GameObject prkBrk = Instantiate(prkngBrake, GameObject.Find("UI").transform.GetChild(0).transform);
        parkingBrake = prkBrk.GetComponent<Button>();
    }
    public void FindControlsUI()
    {
        moveController = GameObject.Find("MoveCntr").GetComponent<Slider>();
        fireArea = GameObject.Find("Panel").GetComponent<RectTransform>();
        forwardBoost = GameObject.Find("ForwardBoost").GetComponent<Button>();
        backwardBoost = GameObject.Find("BackwardBoost").GetComponent<Button>();
        parkingBrake = GameObject.Find("ParkingBrake").GetComponent<Button>();
        //restart = GameObject.Find("Restart").GetComponent<Button>();

        forwardBoost.onClick.AddListener(() => player.ForwardBoost());
        backwardBoost.onClick.AddListener(() => player.BackwardBoost());
        parkingBrake.onClick.AddListener(() => StopPlayer());
        //restart.onClick.AddListener(() => Restart());
    }
    public void FindCamera()
    {
        mainCamera = Camera.main;
    }
    private void Restart()
    {
        UnityEngine.SceneManagement.SceneManager.UnloadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
    private void StopPlayer()
    {
        steeringForce = 0;
        moveController.value = 0;
        player.StopPlayerTruck();
    }
    
    public void UpdateInputs()
    {

        #region unityAndroid
#if UNITY_ANDROID
        steeringForce = moveController.value;
        player.MovePlayerTruck(steeringForce);

        foreach (Touch touch in Input.touches)
        {
            Vector2 localTouchPosition = fireArea.InverseTransformPoint(touch.position);
            if (fireArea.rect.Contains(localTouchPosition))
            {
                if(touch.tapCount == 1)
                {
                    SetUpPlayersTarget(touch, GameEnums.TrackingGroup.FirstTrackingGroup);
                }
                if (touch.tapCount == 2)
                {
                    SetUpPlayersTarget(touch, GameEnums.TrackingGroup.SecondTrackingGroup);
                }
            }

        }
#endif
        #endregion 
        #region unityEditor
#if UNITY_EDITOR
        steeringForce = Input.GetAxis("Vertical");
        Vector2 localMousePosition = fireArea.InverseTransformPoint(Input.mousePosition);
        if (fireArea.rect.Contains(localMousePosition))
        {
            if (Input.GetMouseButton(1))
            {
                SetUpPlayersTarget(Input.mousePosition, GameEnums.TrackingGroup.FirstTrackingGroup);
            }
            if (Input.GetMouseButton(0))
            {
                SetUpPlayersTarget(Input.mousePosition, GameEnums.TrackingGroup.SecondTrackingGroup);
            }
        }
        if(Input.GetKeyDown(KeyCode.F))
        {
            player.ForwardBoost();
        }
        if(Input.GetKeyDown(KeyCode.V))
        {
            player.BackwardBoost();
        }
        if(Input.GetKeyDown(KeyCode.Space))
        {
            player.StopPlayerTruck();
        }

#endif
        #endregion
    }
    void SetUpPlayersTarget(Vector3 mousePosition, GameEnums.TrackingGroup trackingGroup)
    {
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, damageableMask))
        {
            player.SetUpTargets(hit.rigidbody, trackingGroup);
        }
    }

    void SetUpPlayersTarget(Touch touch, GameEnums.TrackingGroup trackingGroup)
    {
        Ray ray = mainCamera.ScreenPointToRay(touch.position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, damageableMask))
        {
            player.SetUpTargets(hit.rigidbody, trackingGroup);
        }
    }
   
    public bool PlayGame()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }
}