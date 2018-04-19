using UnityEngine;
using XInputDotNetPure;

public class TireRotation : MonoBehaviour
{
    PlayerIndex player;
    GamePadState contState;

    [SerializeField] WheelCollider wheelCollider;

    private void Awake()
    {
        player = PlayerIndex.One;
    }

    private void Update()
    {
        contState = GamePad.GetState(player);

        if(contState.IsConnected)
            RotateTires();
    }

    void RotateTires()
    {
        transform.Rotate(wheelCollider.rpm / 60 * 360 * Time.deltaTime, 0, 0);
    }
}
