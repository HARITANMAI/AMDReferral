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
	}

	public bool GetGrounded()
	{

		return m_Grounded;
	}

	private void FixedUpdate()
	{
		//Hook's Law
	}
}
