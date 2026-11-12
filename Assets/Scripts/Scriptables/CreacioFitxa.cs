using UnityEngine;

[CreateAssetMenu(fileName = "Fitxa", menuName = "Scriptable Objects/Fitxa")]
public class Fitxa : ScriptableObject
{
    public int ID;
    public GameObject model;
    public int CP;
    public int tipusFitxa;
}
