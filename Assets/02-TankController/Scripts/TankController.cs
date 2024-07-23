using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class TankController : MonoBehaviour
{
	private AM_02Tank m_ActionMap;
	[SerializeField] private TankSO m_Data;
	[SerializeField] private Rigidbody m_RB;
	[SerializeField] private CameraController m_CameraController;
	[SerializeField] private Turret m_TurretController;
	[SerializeField] private DriveWheel[] m_DriveWheels;
	private int m_NumSuspensionsGrounded;

	private float m_InAccelerate;

	private float m_InSteer;
	private bool m_IsSteering;
	private Coroutine m_CRSteer;

	private bool m_IsFiring;
	private Coroutine m_CRFire;

	private void Awake()
	{
		m_ActionMap = new AM_02Tank();
		m_RB = GetComponent<Rigidbody>();
		m_CameraController = GetComponent<CameraController>();
		m_TurretController = GetComponent<Turret>();
		m_NumSuspensionsGrounded = 0;
		foreach (DriveWheel wheel in m_DriveWheels)
		{
			wheel.Init(m_Data);
			wheel.OnGroundedChanged += Handle_SuspensionGroundedChanged;
		}
		m_TurretController.Init(m_Data);
	}

	private void OnEnable()
	{
		m_ActionMap.Enable();

		m_ActionMap.Default.Accelerate.performed += Handle_AcceleratePerformed;
		m_ActionMap.Default.Accelerate.canceled += Handle_AccelerateCanceled;
		m_ActionMap.Default.Steer.performed += Handle_SteerPerformed;
		m_ActionMap.Default.Steer.canceled += Handle_SteerCanceled;
		m_ActionMap.Default.Fire.performed += Handle_FirePerformed;
		m_ActionMap.Default.Fire.canceled += Handle_FireCanceled;
		m_ActionMap.Default.Aim.performed += Handle_AimPerformed;
		m_ActionMap.Default.Zoom.performed += Handle_ZoomPerformed;
	}
	private void OnDisable()
	{
		m_ActionMap.Disable();

		m_ActionMap.Default.Accelerate.performed -= Handle_AcceleratePerformed;
		m_ActionMap.Default.Accelerate.canceled -= Handle_AccelerateCanceled;
		m_ActionMap.Default.Steer.performed -= Handle_SteerPerformed;
		m_ActionMap.Default.Steer.canceled -= Handle_SteerCanceled;
		m_ActionMap.Default.Fire.performed -= Handle_FirePerformed;
		m_ActionMap.Default.Fire.canceled -= Handle_FireCanceled;
		m_ActionMap.Default.Aim.performed -= Handle_AimPerformed;
		m_ActionMap.Default.Zoom.performed -= Handle_ZoomPerformed;
	}

	private void Handle_AcceleratePerformed(InputAction.CallbackContext context)
	{
		m_InAccelerate = context.ReadValue<float>();
		foreach (DriveWheel wheel in m_DriveWheels)
		{
			wheel.SetAcceleration(m_InAccelerate);
		}
		m_TurretController.SetRotationDirty();
	}
	private void Handle_AccelerateCanceled(InputAction.CallbackContext context)
	{
		m_InAccelerate = context.ReadValue<float>();
		foreach (DriveWheel wheel in m_DriveWheels)
		{
			wheel.SetAcceleration(m_InAccelerate);
		}
		m_TurretController.SetRotationDirty();
	}

	private void Handle_SteerPerformed(InputAction.CallbackContext context)
	{
		m_InSteer = context.ReadValue<float>();

		if (m_IsSteering) return;

		m_IsSteering = true;

		m_CRSteer = StartCoroutine(C_SteerUpdate());
	}
	private void Handle_SteerCanceled(InputAction.CallbackContext context)
	{
		m_InSteer = context.ReadValue<float>();

		if (!m_IsSteering) return;

		m_IsSteering = false;

		StopCoroutine(m_CRSteer);
	}
	private IEnumerator C_SteerUpdate()
	{
		while (m_IsSteering)
		{

			yield return null;
		}
	}

	private void Handle_FirePerformed(InputAction.CallbackContext context)
	{
		if (m_IsFiring) return;

		m_IsFiring = true;

		m_CRFire = StartCoroutine(C_FireUpdate());
	}
	private void Handle_FireCanceled(InputAction.CallbackContext context)
	{
		if (!m_IsFiring) return;

		m_IsFiring = false;

		StopCoroutine(m_CRFire);
	}
	private IEnumerator C_FireUpdate()
	{
		while (m_IsFiring)
		{
			yield return null;
		}
	}

	private void Handle_AimPerformed(InputAction.CallbackContext context)
	{
		m_CameraController.RotateSpringArm(context.ReadValue<Vector2>());
		m_TurretController.SetRotationDirty();
	}

	private void Handle_ZoomPerformed(InputAction.CallbackContext context)
	{
		m_CameraController.ChangeCameraDistance(context.ReadValue<float>());
		m_TurretController.SetRotationDirty();
	}

	private void Handle_SuspensionGroundedChanged(bool newGrounded)
	{

	}
}