﻿using UnityEngine;
public class UpdateManager : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 120;
    }

    private void Update()
    {
        for (int i = 0; i < MonoCached.allTicks.Count; i++)
        {
            MonoCached.allTicks[i].Tick();
        }
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < MonoCached.allFixedTicks.Count; i++)
        {
            MonoCached.allFixedTicks[i].FixedTick();
        }
    }

    private void LateUpdate()
    {
        for (int i = 0; i < MonoCached.allLateTicks.Count; i++)
        {
            MonoCached.allLateTicks[i].LateTick();
        }
    }
}