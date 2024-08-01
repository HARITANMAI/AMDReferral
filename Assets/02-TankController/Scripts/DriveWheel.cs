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
		m_Acceleration = amount;
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
		if (newGrounded)
		{
			m_NumGroundedWheels++;
		}
		if(!newGrounded)
		{
			m_NumGroundedWheels--;
		}
	}

	private void FixedUpdate()
	{
		if(m_Grounded)
		{
			//MOVE LOGIC
            m_RB.AddForce(gameObject.transform.parent.forward * m_Acceleration * 10f, ForceMode.Acceleration);
        }
    }
}