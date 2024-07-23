using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShellData", menuName = "DataObject/Tank/ShellData")]
public class ShellSO : ScriptableObject
{
	public float Damage;
	public float Velocity;
}
