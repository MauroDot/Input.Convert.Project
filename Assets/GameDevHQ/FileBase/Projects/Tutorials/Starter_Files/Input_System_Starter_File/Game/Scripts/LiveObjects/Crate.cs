using Cinemachine;
using Game.Scripts.LiveObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.LiveObjects
{
    public class Crate : MonoBehaviour
    {
        [SerializeField] private float _punchDelay;
        [SerializeField] private GameObject _wholeCrate, _brokenCrate;
        [SerializeField] private Rigidbody[] _pieces;
        [SerializeField] private BoxCollider _crateCollider;
        [SerializeField] private InteractableZone _interactableZone;
        private bool _isReadyToBreak = false;

        [SerializeField] private List<Rigidbody> _brakeOff = new List<Rigidbody>();

        private void OnEnable()
        {
            //InteractableZone.onZoneInteractionComplete += InteractableZone_onZoneInteractionComplete;
            InteractableZone.onHoldEndedButCooler += InteractableZone_onHoldEnded;
        }
        private float currentCoolDown = .2f;
        private float cooldown = .2f;
        private float MaxTimeSpentHolding = 2f;
        private void InteractableZone_onHoldEnded(float curHoldTime)
        {
            if (!_interactableZone._inZone) return;
            if (_interactableZone.GetZoneID() != 6) return;
            if (currentCoolDown < cooldown) return;
            if (_isReadyToBreak == false && _brakeOff.Count > 0)
            {
                _wholeCrate.SetActive(false);
                _brokenCrate.SetActive(true);
                _isReadyToBreak = true;
            }

            if (_isReadyToBreak && _interactableZone.GetZoneID() == 6) //Crate zone            
            {
                Debug.Log("Pressed E key.");
                if (_brakeOff.Count > 0)
                {
                    float breakPower = Mathf.Clamp(curHoldTime / MaxTimeSpentHolding, 0, 1);
                    BreakPart(breakPower);
                    StartCoroutine(PunchDelay());
                }
                else if (_brakeOff.Count == 0)
                {
                    _isReadyToBreak = false;
                    _crateCollider.enabled = false;
                    _interactableZone.CompleteTask(6);
                    Debug.Log("Completely Busted");
                }
            }

            currentCoolDown = 0;
        }

        private void Start()
        {
            _brakeOff.AddRange(_pieces);

        }

        private void Update()
        {
            currentCoolDown += Time.deltaTime;
        }

        public void BreakPart(float power)
        {
            int breakPower = (int)Mathf.Lerp(1, _brakeOff.Count, power);

            Debug.Log(power + " " + breakPower);

            int count = breakPower;

            for (int i = 0; i < count; i++)
            {
                int whichPieceGetBlownOff = Random.Range(0, _brakeOff.Count);
                _brakeOff[whichPieceGetBlownOff].constraints = RigidbodyConstraints.None;
                _brakeOff[whichPieceGetBlownOff].AddForce(new Vector3(1f, 1f, 1f), ForceMode.Force);
                _brakeOff.Remove(_brakeOff[whichPieceGetBlownOff]);
            }

        }

        IEnumerator PunchDelay()
        {
            float delayTimer = 0;
            while (delayTimer < _punchDelay)
            {
                yield return new WaitForEndOfFrame();
                delayTimer += Time.deltaTime;
            }

            _interactableZone.ResetAction(6);
        }

        private void OnDisable()
        {
            InteractableZone.onHoldEndedButCooler += InteractableZone_onHoldEnded;
        }
    }
}
