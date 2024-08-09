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

	private float minLength;
	private float maxLength;
	private float springLength;
	private float lastLength;

    public void Init(SuspensionSO inData)
	{
		m_Data = inData;
		m_Grounded = false;

		minLength = m_Data.RestLength - m_Data.SpringTravel;
		maxLength = m_Data.RestLength + m_Data.SpringTravel;
	}

	public bool GetGrounded()
	{
		//Raycasting towards the -Y axis of the wheel by the length of the spring
		return Physics.Raycast(m_Wheel.position, -m_Wheel.up, m_Data.WheelDiameter/2);
    }

	private void FixedUpdate()
	{
		//Invoking the public event only when the grounded status of the wheel changes
		bool m_newGrounded = GetGrounded();

        if (m_newGrounded != m_Grounded)
		{
			m_Grounded = m_newGrounded;
            OnGroundedChanged?.Invoke(m_Grounded);
        }

        WheelSuspension();
    }

	void WheelSuspension()
	{
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, maxLength))
        {
            lastLength = springLength;
            springLength = hit.distance;
            springLength = Mathf.Clamp(springLength, minLength, maxLength);

            float springVelocity = (lastLength - springLength) / Time.fixedDeltaTime;

            float springForce = m_Data.SuspensionStrength * (m_Data.RestLength - springLength);	//F = K * X
            float damperForce = m_Data.SuspensionDamper * springVelocity;

            Vector3 m_SuspensionForce = (springForce + damperForce) * m_Wheel.transform.up;

            m_RB.AddForceAtPosition(m_SuspensionForce, hit.point);
			m_Wheel.transform.localPosition = transform.up * springLength;

			Debug.Log($"Spring Length is: {springLength}");
			Debug.Log($"Spring Strength is: {springForce}");
            Debug.Log($"Damper Strength is: {damperForce}");
        }
    }

    private void OnDrawGizmos()
    {
		Gizmos.DrawSphere(m_Wheel.transform.position, 0.3f);
    }
}
