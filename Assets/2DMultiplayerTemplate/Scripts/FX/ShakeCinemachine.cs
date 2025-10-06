using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class ShakeCinemachine : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private CinemachineImpulseSource impulseSource;

    public void StartShake()
    {
        impulseSource.GenerateImpulse();
    }
}