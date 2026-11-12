using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class GrupEfecte
{
    public string grup;
    public Color color = Color.white;
    public List<Efecte> efectes;
}
[CreateAssetMenu(fileName = "LlistaEvents", menuName = "Scriptable Objects/CreaEvent")]
public class LlistaEvents : ScriptableObject
{
    public List<GrupEfecte> grupEfectes;
}
