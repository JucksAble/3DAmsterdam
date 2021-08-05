﻿using Netherlands3D.Cameras;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Interface
{
    public class Compass : MonoBehaviour
    {
        private Vector3 direction = Vector3.zero;

        [SerializeField]
        private bool counterRotate = false;

        private float lastClick = 0;
        private float clickModifierTimer = 0.2f;

        void Update()
        {
            direction.z = CameraModeChanger.Instance.ActiveCamera.transform.eulerAngles.y;
            if (counterRotate)
                direction.z *= -1.0f;

            transform.localEulerAngles = direction;
        }

        public void ResetCameraToNorth()
        {
            print("Reset camera to north");
            if ((Time.realtimeSinceStartup - lastClick) < clickModifierTimer)
            {
                CameraModeChanger.Instance.CurrentCameraControls.ResetNorth(true);
            }
            else{
                CameraModeChanger.Instance.CurrentCameraControls.ResetNorth(false);
            }

            lastClick = Time.realtimeSinceStartup;
        }
    }
}