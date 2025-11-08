using System;
using _RacingGamePrototype.Scripts.World.Surfaces;
using UnityEngine;

namespace _RacingGamePrototype.Scripts.Car
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class CarPhysicsController : MonoBehaviour
    {
        [Header("Car setup")]
        [SerializeField] private Transform centerOfMass;
        [SerializeField] private WheelCollider[] frontWheels;
        [SerializeField] private WheelCollider[] rearWheels;
        [SerializeField] private DriveType driveType = DriveType.RWD;

        [Header("Car settings")]
        [SerializeField] private float motorForce = 1500f;
        [SerializeField] private float brakeForce = 3000f;
        [SerializeField] private float maxSteerAngle = 30f;
        [SerializeField] private float minSteerAngle = 10f;
        [SerializeField] private float steerFadeSpeed = 60f;

        [Header("Physics settings")]
        [SerializeField] private float turnDrag = 0.98f;
        [SerializeField] private float gripRestoreSpeed = 20f;
        [SerializeField] private float wheelGrip = 2.0f;
        [SerializeField] private float driftGChangeThreshold = 0.4f;
        [SerializeField] private float driftRecoveryG = 0.4f;
        [SerializeField] private float aeroDrag = 0.3f;
        [SerializeField] private float rollingResistance = 50f;
        [SerializeField] private SurfaceGripData surfaceGripData;

        private Rigidbody _rb;
        private Vector3 _lastLocalVelocity;
        private float _surfaceGrip = 1f;
        private bool _isDrifting;
        private float _sidewaysG;
        private float _deltaG;
        private float _lastSidewaysG;

        public bool IsBraking { get; private set; }
        public Rigidbody Rigidbody => _rb;

        private enum DriveType { FWD, RWD, AWD }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            if (centerOfMass != null)
                _rb.centerOfMass = centerOfMass.localPosition;
        }

        public void FixedUpdatePhysics(float throttle, float steer)
        {
            UpdateSurfaceGrip();
            UpdateDriftState();
            HandleMotor(throttle);
            ApplyAerodynamicDrag();
            HandleSteering(steer);
            ApplyTurnResistance();
            UpdateGrip(throttle, steer);
        }

        public void ResetPhysics(Vector3 pos, Quaternion rot)
        {
            _rb.isKinematic = true;

            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.position = pos;
            _rb.rotation = rot;
            transform.SetPositionAndRotation(pos, rot);

            _lastLocalVelocity = Vector3.zero;
            _isDrifting = false;

            _rb.isKinematic = false;
        }

        private void HandleMotor(float throttle)
        {
            float currentSpeed = Vector3.Dot(_rb.linearVelocity, transform.forward);
            float motorTorque = 0f;
            float brakeTorque = 0f;
            IsBraking = false;

            const float stopThreshold = 1f;

            if (throttle > 0f)
            {
                if (currentSpeed < -stopThreshold)
                {
                    brakeTorque = brakeForce;
                    IsBraking = true;
                }
                else
                {
                    motorTorque = throttle * motorForce;
                }
            }
            else if (throttle < 0f)
            {
                if (currentSpeed > stopThreshold)
                {
                    brakeTorque = brakeForce;
                    IsBraking = true;
                }
                else
                {
                    motorTorque = throttle * motorForce * 0.6f;
                }
            }
            else
            {
                brakeTorque = 0.05f * brakeForce;
            }

            foreach (var f in frontWheels)
                f.brakeTorque = brakeTorque;
            foreach (var r in rearWheels)
                r.brakeTorque = brakeTorque;

            switch (driveType)
            {
                case DriveType.FWD:
                    foreach (var w in frontWheels)
                        w.motorTorque = motorTorque;
                    foreach (var w in rearWheels)
                        w.motorTorque = 0f;
                    break;
                case DriveType.RWD:
                    foreach (var w in rearWheels)
                        w.motorTorque = motorTorque;
                    foreach (var w in frontWheels)
                        w.motorTorque = 0f;
                    break;
                case DriveType.AWD:
                    float halfTorque = motorTorque * 0.5f;
                    foreach (var w in frontWheels)
                        w.motorTorque = halfTorque;
                    foreach (var w in rearWheels)
                        w.motorTorque = halfTorque;
                    break;
            }
        }

        private void HandleSteering(float steerInput)
        {
            float speed = _rb.linearVelocity.magnitude;
            float speedFactor = Mathf.Clamp01(speed / (steerFadeSpeed / 3.6f));
            float currentMaxSteer = Mathf.Lerp(maxSteerAngle, minSteerAngle, speedFactor);
            float steer = steerInput * currentMaxSteer;

            foreach (var wheel in frontWheels)
                wheel.steerAngle = steer;
        }

        private void UpdateSurfaceGrip()
        {
            if (rearWheels.Length == 0) return;
            if (rearWheels[0].GetGroundHit(out var hit))
                _surfaceGrip = surfaceGripData ? surfaceGripData.GetGripForTag(hit.collider.tag) : 1f;
            else
                _surfaceGrip = 1f;
        }

        private void UpdateGrip(float throttle, float steer)
        {
            float speed = _rb.linearVelocity.magnitude;
            bool coasting = Mathf.Abs(throttle) < 0.1f;
            float restoreSpeedMs = gripRestoreSpeed / 3.6f;

            float frontSide = (wheelGrip + 0.2f) * _surfaceGrip;
            float rearSide = (wheelGrip + 0.1f) * _surfaceGrip;
            float forward = wheelGrip * _surfaceGrip;

            if (coasting)
            {
                _rb.AddTorque(transform.up * (steer * 0.5f), ForceMode.Acceleration);
                frontSide *= 1.3f;
                rearSide *= 0.9f;
                forward *= 0.9f;
            }

            bool burnout = speed <= restoreSpeedMs;
            if (_isDrifting)
            {
                frontSide *= 0.9f;
                rearSide *= 0.5f;
                forward *= 0.8f;
            }

            if (burnout)
            {
                rearSide *= Mathf.Lerp(1f, 0.1f, Mathf.Abs(steer));
                forward *= 0.7f;
            }

            foreach (var f in frontWheels)
            {
                var fr = f.sidewaysFriction; fr.stiffness = frontSide; f.sidewaysFriction = fr;
                var ff = f.forwardFriction; ff.stiffness = forward; f.forwardFriction = ff;
            }

            foreach (var r in rearWheels)
            {
                var fr = r.sidewaysFriction; fr.stiffness = rearSide; r.sidewaysFriction = fr;
                var ff = r.forwardFriction; ff.stiffness = forward; r.forwardFriction = ff;
            }
        }

        private void ApplyTurnResistance()
        {
            if (_rb.linearVelocity.sqrMagnitude < 0.1f)
                return;
            Vector3 localVel = transform.InverseTransformDirection(_rb.linearVelocity);
            localVel.x *= turnDrag;
            _rb.linearVelocity = transform.TransformDirection(localVel);
        }

        private void ApplyAerodynamicDrag()
        {
            float speed = _rb.linearVelocity.magnitude;
            Vector3 dragForce = -_rb.linearVelocity.normalized * (speed * speed * aeroDrag + rollingResistance);
            _rb.AddForce(dragForce, ForceMode.Force);
        }

        private void UpdateDriftState()
        {
            Vector3 localVel = transform.InverseTransformDirection(_rb.linearVelocity);
            float lateralAccel = (localVel.x - _lastLocalVelocity.x) / Time.fixedDeltaTime;
            _lastLocalVelocity = localVel;
            _sidewaysG = Mathf.Abs(lateralAccel) / 9.81f;
            _deltaG = Mathf.Abs((_sidewaysG - _lastSidewaysG) / Time.fixedDeltaTime);
            _lastSidewaysG = _sidewaysG;
            _isDrifting = _deltaG > driftGChangeThreshold;
        }
    }
}
