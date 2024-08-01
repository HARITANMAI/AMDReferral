using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	[SerializeField] private Transform m_SpringArmTarget;
	[SerializeField] private Transform m_CameraMount;
	[SerializeField] private Camera m_Camera;

	private float m_CameraDist = 5f;

	[SerializeField] private float m_YawSensitivity;
	[SerializeField] private float m_PitchSensitivity;
	[SerializeField] private float m_ZoomSensitivity;

	[SerializeField] private float m_MaxDist;
	[SerializeField] private float m_MinDist;

	[SerializeField] private float m_CameraProbeSize;
	[SerializeField] private Vector3 m_TargetOffset;

	public void RotateSpringArm(Vector2 change)
	{
		//Storing the local rotation of spring arm to change it based on the input
		Vector3 angle = m_SpringArmTarget.localEulerAngles;

		//Changing pitch and yaw/ X and Y axes. X axes rotation controls up and down while Y does left and right.
		angle.x -= change.y * m_PitchSensitivity;
		angle.y += change.x * m_YawSensitivity;

		//Clamping in X axis to visually clamp the Y axis camera movement in gameplay 
		angle.x = Mathf.Clamp(angle.x, 0, 90);

		//Assigning the new rotation to the spring arm's rotation
		m_SpringArmTarget.localEulerAngles = angle;
	}

	public void ChangeCameraDistance(float amount)
	{
		//Gets input from the mouse wheel and changes it based on the zoom sensitivity
		m_CameraDist += amount * m_ZoomSensitivity;

		//Ensuring the camera zoom is within the limits
		m_CameraDist = Mathf.Clamp(m_CameraDist, m_MinDist, m_MaxDist);
	}

	private void LateUpdate()
	{
		//Offsetting the camera mount based on the camera mounts forward axis multiplied by the camera distance
		m_CameraMount.position = m_SpringArmTarget.position - m_CameraMount.forward * m_CameraDist;
	}
}