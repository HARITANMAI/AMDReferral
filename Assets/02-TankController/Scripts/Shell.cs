using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{
    Rigidbody m_RB;

    private void Awake()
    {
        m_RB = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        m_RB.AddForce(-gameObject.transform.up * 9.8f);
    }
}
