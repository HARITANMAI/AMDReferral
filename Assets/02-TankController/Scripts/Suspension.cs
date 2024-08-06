using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Suspension : MonoBehaviour
{
	public event Action<bool> OnGroundedChanged; 

	[SerializeField] private Transform m_Wheel;
	[SerializeField] private Rigidbody m_RB;

	private SuspensionSO m_Data;
	private float m_SpringSize;
	private bool m_Grounded;

	public void Init(SuspensionSO inData)
	{
		m_Data = inData;
		m_Grounded = false;
	}

	public bool GetGrounded()
	{
		//Raycasting towards the -Y axis of the wheel by the length of the spring
		if (Physics.Raycast(m_Wheel.position, m_Wheel.up * -1, 1f))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	private void FixedUpdate()
	{
		//Invoking the public event only when the grounded status of the wheel changes
		bool m_newGrounded = GetGrounded();

        if (m_newGrounded != m_Grounded)
		{
			m_Grounded = m_newGrounded;
            OnGroundedChanged?.Invoke(m_Grounded);
			//Debug.Log("newGrounded if statement is working");
        }
        //Hook's Law for the suspension
    }
}
