using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriveWheel : MonoBehaviour
{
	public event Action<bool> OnGroundedChanged;

	[SerializeField] private Rigidbody m_RB;
	[SerializeField] private TankSO m_Data;
    [SerializeField] private SuspensionSO m_SusData;
    [SerializeField] private Suspension[] m_SuspensionWheels;
	private int m_NumGroundedWheels;
	private bool m_Grounded;
	private float m_Acceleration;

	public void SetAcceleration(float amount)
	{
        // Power = HorsePower * 745.7 or Power = Force * Velocity, Converting HorsePower into Power
        float power = m_Data.EngineData.HorsePower * 745.7f;

        //'Weight = Mass * Gravity' ------> 'Mass = Weight / Gravity'
        float mass = (float)(m_Data.Mass_Tons * 1000);

        //Using constant Velocity to have stable acceleration
        float velocity = 2f;

        //'Acceleration = Force / Mass' -------> 'Acceleration = Power / (Mass * Velocity)'
        m_Acceleration = amount * (power / (mass * velocity));
        //Debug.Log($"Accerlation is: {m_Acceleration}");
	}

	public void Init(TankSO inData)
	{
		m_Data = inData;
		m_NumGroundedWheels = 0;
		foreach(Suspension wheel in m_SuspensionWheels)
		{
            wheel.Init(m_SusData); 
            wheel.OnGroundedChanged += Handle_WheelGroundedChanged;
            //The suspension's event gets invoked in the suspension script and passes a boolean value into this function
        }
    }

	//Handle_WheelGroundChanged is the function which gets called by invoking the OnGroundChanged event through the Suspension script
	private void Handle_WheelGroundedChanged(bool newGrounded)
	{
        //Add or remove number of wheels
        if (newGrounded == true)
		{
            m_NumGroundedWheels++;
            //Debug.Log($"Handle_WheelGroundedChanged added: {m_NumGroundedWheels}");
		}
		else
		{
            m_NumGroundedWheels--;
            //Debug.Log($"Handle_WheelGroundedChanged subtracted: {m_NumGroundedWheels}");
        }

        //Update the tank's grounded status
        m_Grounded = m_NumGroundedWheels > 0;
    }

    private void FixedUpdate()
    {
        if (m_Grounded)
        {
            float speed = Vector3.Dot(m_RB.velocity, transform.forward);
            float speed2 = m_RB.velocity.magnitude;
            Debug.Log($"Tank velocity in forward: {speed}");
            //Debug.Log($"Overall tank's velocity: {speed2}");

            float traction = m_NumGroundedWheels / m_SuspensionWheels.Length;
            float force = m_Acceleration * traction;

            if (speed < m_Data.Max_Speed && speed > -m_Data.Max_Speed)
			{
                foreach (Suspension wheel in m_SuspensionWheels)
                {
                    if (wheel.GetGrounded())
                    {
                        //Getting force per wheel
                        Vector3 wheelForce = (wheel.transform.forward * force) / m_NumGroundedWheels;
                        m_RB.AddForceAtPosition(wheelForce, wheel.transform.position, ForceMode.Acceleration);
                        
                        //Fixing the tank drift bug at high speeds
                        //Getting the velocity of the each wheel
                        Vector3 velocity = m_RB.GetPointVelocity(wheel.transform.position) / m_NumGroundedWheels;
                        Vector3 lateralVelocity = Vector3.ProjectOnPlane(velocity, wheel.transform.forward);

                        //Applying opposite lateral movement force to the tank to reduce drifting
                        m_RB.AddForceAtPosition(-lateralVelocity * 0.5f, wheel.transform.position, ForceMode.Acceleration);
                    }
                }
            }

            if (force == 0)
            {
                //Applies force in the opposite direction of tank's movement, making it break when theres no input
                m_RB.AddForce(-m_RB.velocity * 0.5f);
            }
        }
    }
}