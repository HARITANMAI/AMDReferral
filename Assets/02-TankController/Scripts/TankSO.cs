using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TankData", menuName = "DataObject/Tank/TankData", order = 0)]
public class TankSO : ScriptableObject
{
	public BarrelSO BarrelData;
	public TurretSO TurretData;
	public ShellSO ShellData;
	public EngineSO EngineData;
	public SuspensionSO SuspensionData;
	public float Health;
	public float Mass_Tons;
}
