using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriveWheel : MonoBehaviour
{
	public event Action<bool> OnGroundedChanged;

	[SerializeField] private Rigidbody m_RB;
	[SerializeField] private TankSO m_Data;
	[SerializeField] private Suspension[] m_SuspensionWheels;
	private int m_NumGroundedWheels;
	private bool m_Grounded;
	private float m_Acceleration;

	public void SetAcceleration(float amount)
	{
        // Power = HorsePower * 745.7 or Power = Force * Velocity, Converting HorsePower into Power
        float power = m_Data.EngineData.HorsePower * 745.7f;

        //(Weight = Mass * Gravity) -> (Mass = Weight / Gravity)
        float mass = (float)(m_Data.Mass_Tons * 1000);

        //Clamping Velocity
        float velocity = Mathf.Clamp(m_RB.velocity.magnitude, 1f, m_Data.Max_Speed);

        //(Accel = Force / Mass) -> (Accel = Power / (Mass * Velocity))
        m_Acceleration = amount * (power / (mass * velocity));
	}

	public void Init(TankSO inData)
	{
		m_Data = inData;
		m_NumGroundedWheels = 0;
		foreach(Suspension wheel in m_SuspensionWheels)
		{
			//The suspension's event gets invoked in the suspension script and passes a boolean value into this function
			wheel.OnGroundedChanged += Handle_WheelGroundedChanged;
		}
	}

	//Handle_WheelGroundChanged is the function which gets called by invoking the OnGroundChanged event through the Suspension script
	private void Handle_WheelGroundedChanged(bool newGrounded)
	{
        //Add or remove number of wheels
        if (newGrounded == true)
		{
            m_NumGroundedWheels++;
            Debug.Log($"Handle_WheelGroundedChanged added: {m_NumGroundedWheels}");
		}
		else
		{
            m_NumGroundedWheels--;
            Debug.Log($"Handle_WheelGroundedChanged subtracted: {m_NumGroundedWheels}");
        }

        //Update the tank's grounded status
        if (m_NumGroundedWheels > 0)
        {
            m_Grounded = true;
        }
        else
        {
            m_Grounded = false;
        }
    }

    private void FixedUpdate()
    {
        if (m_Grounded)
        {
            float traction = m_NumGroundedWheels / m_SuspensionWheels.Length;
            float force = m_Acceleration * traction;
            float speed = Vector3.Dot(m_RB.velocity, transform.forward);
            Debug.Log($"Current speed is: {speed}");

            if (speed < m_Data.Max_Speed)
			{
                foreach (Suspension wheel in m_SuspensionWheels)
                {
                    if (wheel.GetGrounded())
                    {
                        //Getting force per wheel
                        Vector3 wheelForce = (wheel.transform.forward * force) / m_NumGroundedWheels;
                        m_RB.AddForceAtPosition(wheelForce, wheel.transform.position, ForceMode.Acceleration);
                    }
                }
            }
            else
            {
                m_RB.velocity = transform.forward * 10.56f;
            }
        }
    }
}