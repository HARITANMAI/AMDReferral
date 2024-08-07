using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ExampleSuspension : MonoBehaviour
{
    Rigidbody m_RB;
    [SerializeField][Range (0, 100)]float m_SpringSize;
    [SerializeField][Range(0, 100)] float m_SuspensionStrength;
    [SerializeField][Range(0, 100)] float m_SuspensionDamper;

    private void Awake()
    {
        m_RB = GetComponent<Rigidbody>();
        m_SpringSize = 5f;
        m_SuspensionDamper = 10f;
        m_SuspensionStrength = 2f;
    }

    private void FixedUpdate()
    {
        //Hook's Law for the suspension
        //The direction the spring should offset from
        Vector3 SpringOffsetDirection = Vector3.down;
        //Vector3 localDir = SpringOffsetDirection;

        //Coverting local to world
        Vector3 localDir = transform.InverseTransformDirection(SpringOffsetDirection);

        Vector3 worldVel = m_RB.GetPointVelocity(transform.position);

        //Finding vector from this location to spring base position
        Vector3 springVec = transform.position - transform.parent.position;

        //Difference bw initial spring length and the current spring length
        float susOffset = m_SpringSize - Vector3.Dot(springVec, localDir);

        float susVel = Vector3.Dot(localDir, worldVel);

        float susforce = (susOffset * m_SuspensionStrength) - (susVel * m_SuspensionDamper);

        m_RB.AddForce(localDir * (susforce / m_RB.mass));
    }
}
