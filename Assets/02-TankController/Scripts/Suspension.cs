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
	private Vector3 m_SuspensionForce;
	private bool m_Grounded;

	public float minLength;
	public float maxLength;
	private float restLength;

    public void Init(SuspensionSO inData)
	{
		m_Data = inData;
		m_Grounded = false;
		Debug.Log("The init in suspesion script is runninThe init in suspesion script is runninThe init in suspesion script " +
			"is runninThe init in suspesion script is runninThe init in suspesion script is runninThe init in suspesion script is running");
	}

	public bool GetGrounded()
	{
		//Raycasting towards the -Y axis of the wheel by the length of the spring
		if (Physics.Raycast(m_Wheel.position, m_Wheel.up * -1f, 1f))
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
        }

		WheelSuspension();
    }

	void WheelSuspension()
	{
        if (Physics.Raycast(m_Wheel.transform.position, -m_Wheel.up, out RaycastHit hit, m_SpringSize + (m_Data.WheelDiameter / 2)))
        {
			float springLength = hit.distance - (m_Data.WheelDiameter / 2);

			float springForce = m_Data.SuspensionStrength * (restLength - springLength);

			m_SuspensionForce = springForce * m_Wheel.transform.up;

			m_RB.AddForceAtPosition(m_SuspensionForce, hit.point);
        }
    }

    void TankSuspension()
    {
        //Tank Suspension
        Vector3 SpringOffsetDirection = -m_Wheel.up;

        //Coverting from local to world
        Vector3 localDir = transform.InverseTransformDirection(SpringOffsetDirection);

        Vector3 worldVel = m_RB.GetPointVelocity(m_Wheel.transform.position);

        //Finding vector from this location to spring base position
        Vector3 springVec = transform.position - transform.parent.position;

        m_SpringSize = 0.8f;
        //Difference bw initial spring length and the current spring length
        float susOffset = m_SpringSize - Vector3.Dot(springVec, localDir);

        float susVel = Vector3.Dot(localDir, worldVel);

        float susforce = (susOffset * 5f) - (susVel * 2f);

        m_RB.AddForceAtPosition(localDir * (susforce / m_RB.mass), m_Wheel.transform.position);
        //m_RB.AddForce(localDir * (susforce / m_RB.mass))

        //transform.localPosition = -susOffset * transform.position;
    }
}
