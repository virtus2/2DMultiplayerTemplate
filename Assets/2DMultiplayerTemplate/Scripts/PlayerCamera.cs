using Unity.Cinemachine;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private CinemachineCamera playerCamera;

    public void SetFollowTarget(GameObject go)
    {
        playerCamera.Follow = go.transform;
    }
}
