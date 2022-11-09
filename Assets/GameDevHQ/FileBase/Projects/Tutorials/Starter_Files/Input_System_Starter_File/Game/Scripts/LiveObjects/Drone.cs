using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Game.Scripts.UI;
using Game.Scripts.PlayerNM;

namespace Game.Scripts.LiveObjects
{
    public class Drone : MonoBehaviour
    {
        private enum Tilt
        {
            NoTilt, Forward, Back, Left, Right
        }

        [SerializeField]
        private Rigidbody _rigidbody;
        [SerializeField]
        private float _speed = 5f;
        private bool _inFlightMode = false;
        [SerializeField]
        private Animator _propAnim;
        [SerializeField]
        private CinemachineVirtualCamera _droneCam;
        [SerializeField]
        private InteractableZone _interactableZone;


        public static event Action OnEnterFlightMode;
        public static event Action onExitFlightmode;

        private Player player;

        private void Awake()
        {
            player = FindObjectOfType<Player>();
        }

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += EnterFlightMode;
        }

        private void EnterFlightMode(InteractableZone zone)
        {
            if (_inFlightMode != true && zone.GetZoneID() == 4) // drone Scene
            {
                _propAnim.SetTrigger("StartProps");
                _droneCam.Priority = 11;
                _inFlightMode = true;
                OnEnterFlightMode?.Invoke();
                UIManager.Instance.DroneView(true);
                _interactableZone.CompleteTask(4);
            }
        }

        private void ExitFlightMode()
        {
            _droneCam.Priority = 9;
            _inFlightMode = false;
            UIManager.Instance.DroneView(false);
        }

        Vector2 inputDirection = Vector2.zero;
        private void Update()
        {
            if (_inFlightMode)
            {
                inputDirection = player.Controls.Drone.Movement.ReadValue<Vector2>();
                CalculateTilt();
                CalculateMovementUpdate();

                if (player.Controls.Drone.Exit.IsPressed())
                {
                    _inFlightMode = false;
                    onExitFlightmode?.Invoke();
                    ExitFlightMode();
                }
            }
        }

        private void FixedUpdate()
        {
            _rigidbody.AddForce(transform.up * (9.81f), ForceMode.Acceleration);
            if (_inFlightMode)
                CalculateMovementFixedUpdate();
        }

        private void CalculateMovementUpdate()
        {

            if (inputDirection.x < 0)
            {
                var tempRot = transform.localRotation.eulerAngles;
                tempRot.y -= _speed / 3;
                transform.localRotation = Quaternion.Euler(tempRot);
            }
            if (inputDirection.x > 0)
            {
                var tempRot = transform.localRotation.eulerAngles;
                tempRot.y += _speed / 3;
                transform.localRotation = Quaternion.Euler(tempRot);
            }
        }

        private void CalculateMovementFixedUpdate()
        {

            if (player.Controls.Drone.Thrust.ReadValue<float>() > 0)
            {
                _rigidbody.AddForce(transform.up * _speed, ForceMode.Acceleration);
            }
            if (player.Controls.Drone.Thrust.ReadValue<float>() < 0)
            {
                _rigidbody.AddForce(-transform.up * _speed, ForceMode.Acceleration);
            }
        }

        private void CalculateTilt()
        {
            if (inputDirection.x < 0)
                transform.rotation = Quaternion.Euler(00, transform.localRotation.eulerAngles.y, 30);
            else if (inputDirection.x > 0)
                transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, -30);
            else if (inputDirection.y > 0)
                transform.rotation = Quaternion.Euler(30, transform.localRotation.eulerAngles.y, 0);
            else if (inputDirection.y < 0)
                transform.rotation = Quaternion.Euler(-30, transform.localRotation.eulerAngles.y, 0);
            else
                transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0);
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= EnterFlightMode;
        }
    }
}
