using UnityEngine;

public class RabbitEvents : MonoBehaviour
{
    public void TriggerVegiePour()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.TriggerVegiePour();
        }
    }
}
