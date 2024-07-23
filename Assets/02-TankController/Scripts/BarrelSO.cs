using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BarrelData", menuName = "DataObject/Tank/BarrelData")]
public class BarrelSO : ScriptableObject
{
	public float Dispersion;
	public float AimingTime;
	public float MaxRange;
	public float ReloadTime;

}
