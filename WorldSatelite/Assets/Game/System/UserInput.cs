using System;
using System.Collections;
using System.Globalization;
using Cinemachine;
using Game.Orbit;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Game.System
{
    [RequireComponent(typeof(CinemachineCameraOffset))]
    [RequireComponent(typeof(CinemachineVirtualCamera))]
    public class UserInput : Timer
    {
        public static UserInput Instance;
        public Transform target = default;
        public CinemachineCameraOffset offset;

        [Header("Zooming")]
        public float minZoom = -9000;
        public float maxZoom = -30000;
        public int zoomingSpeed = 500;

        [Header("Time settings")]
        public Text currentValue;
        public Slider slider;
        public Text timeValue;
        
        [Header("Satellite data")]
        public GameObject satPanel;
        public Text satName;
        public Text satNumber;
        public Text satLaunch;

        //time
        private DateTime earthTime;
        
        //camera settings
        private bool freeLookActive;
        private bool hasFocus;
        private bool inWindow;
        private float currentSliderValue = 0;

        //selection
        public SatelliteOrbit selectedObject = null;
        
        private void Awake()
        {
            if(!Instance)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError("World instance already set!");
            }
        }
        
        private void Start()
        {
            offset = GetComponent<CinemachineCameraOffset>();
            CinemachineCore.GetInputAxis = GetInputAxis;
            earthTime = DateTime.Now;
            Instance.satPanel.SetActive(false);
            StartCoroutine(CheckTime());
        }

        private void Update()
        {
            OnWindowPosition();
            if (inWindow)
            {
                freeLookActive = Input.GetMouseButton(1); // 0 = left mouse btn or 1 = right
                Scroll();
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    SetSelection();
                }
                if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
                {
                    slider.value++;
                }
                if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
                {
                    slider.value--;
                }
            }
            if(freeLookActive)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }

        private void UpdateText()
        {
            earthTime = earthTime.AddMinutes(SatelliteSpeedFactor * currentSliderValue);
            timeValue.text = earthTime.ToLongTimeString()+" "+earthTime.ToShortDateString();
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (currentSliderValue == slider.value) return;
            currentValue.text = slider.value.ToString(CultureInfo.InvariantCulture);
            currentSliderValue = slider.value;
        }

        private float GetInputAxis(string axisName)
        {
            if(inWindow)
            {
                if(freeLookActive)
                {
                    return Input.GetAxis(axisName == "Mouse Y" ? "Mouse Y" : "Mouse X");
                }
            }
            return 0;
        }

        private void Scroll()
        {
            var abs = offset.m_Offset.z + Input.mouseScrollDelta.y * zoomingSpeed;
            if (abs < maxZoom)
            {
                offset.m_Offset.z = maxZoom;
                return;
            }
            if (abs > minZoom)
            {
                offset.m_Offset.z = minZoom;
                return;
            }
            offset.m_Offset.z += Input.mouseScrollDelta.y * zoomingSpeed;
        }

        private void OnWindowPosition()
        {
            var screenRect = new Rect(0, 0, Screen.width, Screen.height);
            inWindow = screenRect.Contains(Input.mousePosition) && hasFocus;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            this.hasFocus = hasFocus;
        }

        private void RescaleSat(bool defaultScale = true)
        {
            if (selectedObject != null)
            {
                selectedObject.transform.localScale = defaultScale ? Vector3.one : new Vector3(10, 10, 10);
            }
        }
        
        public static int GetScaleValue()
        {
            return (int)Instance.currentSliderValue;
        }

        public static void SetSelection(SatelliteOrbit obj)
        {
            if (Instance.selectedObject != null)
            {
                Instance.RescaleSat();
                Instance.selectedObject.HideOrbit();
            }
            Instance.selectedObject = obj;
            Instance.RescaleSat(false);
            obj.ShowOrbit();
            Instance.satPanel.SetActive(true);
            Instance.satName.text = obj.tle.Name;
            Instance.satNumber.text = obj.tle.NoradNumber;
            Instance.satLaunch.text = obj.tle.Epoch;
        }
        
        private static void SetSelection()
        {
            Instance.RescaleSat();
            Instance.selectedObject = null;
            Instance.satPanel.SetActive(false);
        }

        protected override void DoWork()
        {
            UpdateText();
        }
    }
}
