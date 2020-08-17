using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour {

    private float m_horizontalInput;
    private float m_verticalInput;
    private float m_steeringAngle;

    public WheelCollider frontDriverWheelC, frontPassengerWheelC;
    public WheelCollider rearDriverWheelC, rearPassengerWheelC;
    public Transform frontDriverT, frontPassengerT;
    public Transform rearDriverT, rearPassengerT;
    public float SteeringAngle = 40;
    public float Power = 500;
    public float brakes = 1;
    public void GetInput()

    {
        m_horizontalInput = Input.GetAxis("Horizontal");
        m_verticalInput = Input.GetAxis("Vertical");
    }

    private void Steer()
    {
        m_steeringAngle = SteeringAngle * m_horizontalInput;
        frontDriverWheelC.steerAngle = m_steeringAngle;
        frontPassengerWheelC.steerAngle = m_steeringAngle;
    }

    private void Accelerate()
    {
        frontDriverWheelC.motorTorque = m_verticalInput * Power;
        frontPassengerWheelC.motorTorque = m_verticalInput * Power;
        //rearDriverWheelC.motorTorque = m_verticalInput * Power;
        //rearPassengerWheelC.motorTorque = m_verticalInput * Power;
    }

    private void UpdateWheelPoses()
    {
        UpdateWheelPose(frontDriverWheelC, frontDriverT);
        UpdateWheelPose(frontPassengerWheelC, frontPassengerT);
        UpdateWheelPose(rearDriverWheelC, rearDriverT);
        UpdateWheelPose(rearPassengerWheelC, rearPassengerT);
    }

    private void UpdateWheelPose(WheelCollider _collider, Transform _transform)
    {
        Vector3 _pos = _transform.position;
        Quaternion _quat = _transform.rotation;

        _collider.GetWorldPose(out _pos, out _quat);

        _transform.position = _pos;
        _transform.rotation = _quat;
    }

    private void FixedUpdate()
    {
        GetInput();
        Steer();
        Accelerate();
        UpdateWheelPoses();

    }


}
