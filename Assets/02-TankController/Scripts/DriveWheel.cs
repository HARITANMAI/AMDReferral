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
		m_Acceleration = amount * (float)(m_Data.EngineData.HorsePower/56.5);
	}

	public void Init(TankSO inData)
	{
		m_Data = inData;
		m_NumGroundedWheels = 0;
		foreach(Suspension wheel in m_SuspensionWheels)
		{
			//The suspension's event gets invoked in the suspension script and calls this Handle_Wheel method
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
    }

	private void FixedUpdate()
	{
		if (m_NumGroundedWheels > 0)
		{
			m_Grounded = true;
		}
		else 
		{
			m_Grounded = false;
		}

		if (m_Grounded)
		{
			Debug.Log("m_Grounded in FixedUpdate is true");
            float traction = m_NumGroundedWheels / m_SuspensionWheels.Length;
            float force = m_Acceleration * traction;

            foreach (Suspension wheel in m_SuspensionWheels)
            {
                if (wheel.GetGrounded() == true)
                {
					Vector3 wheelForce = (wheel.transform.forward * force) /  m_NumGroundedWheels;
                    m_RB.AddForceAtPosition(wheelForce, wheel.transform.position, ForceMode.Acceleration);
                }
            }

            //m_RB.AddForce(force * gameObject.transform.parent.forward * 8f, ForceMode.Acceleration);
        }
    }
}