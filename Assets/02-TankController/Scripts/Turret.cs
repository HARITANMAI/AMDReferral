using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Turret : MonoBehaviour
{
	[SerializeField] private Transform m_CameraMount;
	[SerializeField] private Transform m_Turret;
	[SerializeField] private Transform m_Barrel;

	private TankSO m_Data;
	private bool m_RotationDirty;
	private Coroutine m_CRAimingTurret;

	private void Awake()
	{
		m_RotationDirty = false;
	}

	public void Init(TankSO inData)
	{
		m_Data = inData;
	}

	public void SetRotationDirty()
	{
		//This function gets called on mouse's input and starts the aimign coroutine if it hasn't been started already
		if(m_CRAimingTurret == null)
        {
            m_RotationDirty = true;
            //m_CRAimingTurret = StartCoroutine(C_AimTurret());
		}
    }

	private IEnumerator C_AimTurret()
	{
		//Debug.Log("The Aim Turret coroutine started.");
		while (m_RotationDirty)
		{
			//Projecting the camera's forward vector onto the plane of turret to remove its Y axis and constrain its rotation to horizontal movement
			Vector3 projectedVec = Vector3.ProjectOnPlane(m_CameraMount.forward, transform.up);

			//Making a quaternion that follows in the direction of inputted vector while rotating along with the tank's main body
			Quaternion targetRot = Quaternion.LookRotation(projectedVec, transform.up);

			//Rotating the turret towards the target rotation using 'RotateTowards' to have constant rotation speed
			m_Turret.rotation = Quaternion.RotateTowards(m_Turret.rotation, targetRot, Time.fixedDeltaTime * m_Data.TurretData.TurretTraverseSpeed);

            Debug.DrawLine(transform.position, transform.position + projectedVec * 20f, Color.red);

			//Checking if the current rotation reaches near target rotation to stop the coroutine
            if (Quaternion.Angle(m_Turret.rotation, targetRot) <= 0.1f)
			{
				m_RotationDirty = false;
				StopCoroutine(C_AimTurret());
				m_CRAimingTurret = null;
				yield break;
			}

			yield return null;
		}
	}
}
