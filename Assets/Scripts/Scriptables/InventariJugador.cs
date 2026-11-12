using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "InventariJugador", menuName = "Scriptable Objects/InventariJugador")]
public class InventariJugador : ScriptableObject
{
    public List<Fitxa> Fitxes;
    public List<Bossa> Bosses;
}
