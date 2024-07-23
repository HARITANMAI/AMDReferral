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

	}

	public void ChangeCameraDistance(float amount)
	{

	}

	private void LateUpdate()
	{

	}
}