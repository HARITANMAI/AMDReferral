using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_Handler : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI ui_TankSpeed;
    [SerializeField] TextMeshProUGUI ui_ReloadStatus;
    [SerializeField] TextMeshProUGUI ui_TurretRotation;

    Turret m_Turret;
    Barrel m_Barrle;
    DriveWheel m_DriveWheel;
    Rigidbody m_RB;

    private void Awake()
    {
        m_Turret = GetComponent<Turret>();
        m_Barrle = GetComponent<Barrel>();
        m_DriveWheel = GetComponent<DriveWheel>();
        m_RB = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!m_Barrle.m_CanFire)
        {
            ui_ReloadStatus.text = "Barrel Status: Reloading";
        }
        else
        {
            ui_ReloadStatus.text = "Barrel Status: Ready";
        }

        ui_TankSpeed.text = ($"Tank Velocity: {Mathf.Round(m_RB.velocity.magnitude).ToString()}");

        ui_TurretRotation.text = ($"Turret Rotation: {Mathf.Round(m_Turret.m_TurretAimDirection).ToString()}");
    }
}
