using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using Game.Scripts.PlayerNM;

namespace Game.Scripts.LiveObjects
{
    public class Laptop : MonoBehaviour
    {
        [SerializeField]
        private Slider _progressBar;
        [SerializeField]
        private int _hackTime = 5;
        public bool _comepletedHack = false;
        [SerializeField]
        private CinemachineVirtualCamera[] _cameras;
        private int _activeCamera = 0;
        [SerializeField]
        private InteractableZone _interactableZone;

        public static event Action onHackComplete;
        public static event Action onHackEnded;

        private Player player;

        private void Awake()
        {
            player = FindObjectOfType<Player>();
        }

        private void OnEnable()
        {
            InteractableZone.onHoldStarted += InteractableZone_onHoldStarted;
            InteractableZone.onHoldEnded += InteractableZone_onHoldEnded;
            player.Controls.Camera.Switch.performed += Switch_performed;
            player.Controls.Camera.Exit.performed += Exit_performed;
        }

        private void Exit_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (!_comepletedHack) return;
            _comepletedHack = false;
            onHackEnded?.Invoke();
            ResetCameras();
            player.Controls.Default.Enable();
            player.Controls.Camera.Disable();
        }

        private void Switch_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (!_comepletedHack) return;

            var previous = _activeCamera;
            _activeCamera++;


            if (_activeCamera >= _cameras.Length)
                _activeCamera = 0;


            _cameras[_activeCamera].Priority = 11;
            _cameras[previous].Priority = 9;
        }

        private void Update()
        {
            if (_comepletedHack == true)
            {
                player.Controls.Default.Disable();
                player.Controls.Camera.Enable();
            }
            else
            {
                player.Controls.Camera.Disable();
            }
        }

        void ResetCameras()
        {
            foreach (var cam in _cameras)
            {
                cam.Priority = 9;
            }
        }

        private void InteractableZone_onHoldStarted(int zoneID, float currentTime)
        {
            if (zoneID == 3 && _comepletedHack == false) //Hacking terminal
            {
                _progressBar.gameObject.SetActive(true);
                _progressBar.value = currentTime / _hackTime;


                if (currentTime >= 5)
                {

                    _comepletedHack = true;
                    _interactableZone.CompleteTask(3);

                    //hide progress bar
                    _progressBar.gameObject.SetActive(false);
                    _cameras[0].Priority = 11;

                    onHackComplete?.Invoke();
                }

            }
        }

        private void InteractableZone_onHoldEnded(int zoneID, float currentTime)
        {
            if (zoneID == 3) //Hacking terminal
            {
                if (_comepletedHack == true)
                    return;

                //StopAllCoroutines();
                _progressBar.gameObject.SetActive(false);
                _progressBar.value = 0;
                onHackEnded?.Invoke();
            }
        }


        IEnumerator HackingRoutine()
        {
            while (_progressBar.value < 1)
            {
                _progressBar.value += Time.deltaTime / _hackTime;
                yield return new WaitForEndOfFrame();
            }

            //successfully hacked
            _comepletedHack = true;
            _interactableZone.CompleteTask(3);

            //hide progress bar
            _progressBar.gameObject.SetActive(false);

            //enable Vcam1
            _cameras[0].Priority = 11;
        }

        private void OnDisable()
        {
            InteractableZone.onHoldStarted -= InteractableZone_onHoldStarted;
            InteractableZone.onHoldEnded -= InteractableZone_onHoldEnded;
        }
    }

}
