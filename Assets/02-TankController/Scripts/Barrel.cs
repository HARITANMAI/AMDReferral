using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrel : MonoBehaviour
{
	[SerializeField] private TankSO m_Data;
	[SerializeField] private Shell m_ShellPrefab;
	[SerializeField] private Transform m_ShellPosition;
	[SerializeField] private ShellSO[] m_AmmoTypes;
	[SerializeField] private int[] m_AmmoCounts;
	private int m_SelectedShell;

	private float m_CurrentDispersion;
	private bool m_CanFire;

	public void Init(TankSO inData)
	{
		m_Data = inData;
		m_CanFire = true;
	}

    public void Fire()
	{
		if (m_CanFire)
		{
            Shell shell = Instantiate(m_ShellPrefab, m_ShellPosition.transform.position, transform.rotation);

			//Getting the velocity from SO and multiplying it with the forward direction of the shell position
            Vector3 shellForce = m_Data.ShellData.Velocity * m_ShellPosition.transform.forward;
            Rigidbody shellRb = shell.GetComponent<Rigidbody>();
            shellRb.AddForce(shellForce, ForceMode.Impulse);

			//Adding tiny recoil force to the tank
			Rigidbody tankRb = GetComponent<Rigidbody>();
			tankRb.AddForce(-shellForce * 0.01f, ForceMode.Impulse);

			StartCoroutine(C_Reload());
        }
	}

	IEnumerator C_Reload()
	{
		m_CanFire = false;
        yield return new WaitForSeconds(m_Data.BarrelData.ReloadTime);
        m_CanFire = true;
        StopCoroutine(C_Reload());
	}
}
