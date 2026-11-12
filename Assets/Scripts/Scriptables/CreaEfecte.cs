using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NouEfecte", menuName = "Scriptable Objects/Crea Efecte")]
public class Efecte : ScriptableObject
{
    public string nomEfecte;
    public int CP;
    public List<Efecte> efectesRelacionats;

}