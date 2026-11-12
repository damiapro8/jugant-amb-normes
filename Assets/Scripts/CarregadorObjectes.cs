using UnityEngine;

public class CarregadorObjectes : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod]
    void Awake()
    {
        Resources.LoadAll<Efecte>("");
        Resources.LoadAll<LlistaEvents>("");
    }
}
