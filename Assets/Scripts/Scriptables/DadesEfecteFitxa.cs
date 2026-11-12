using UnityEngine;

[System.Serializable]
public class DadesEfecteFitxa
{
    public string iniciador;
    public AccioData accio;
}
[System.Serializable]
public class AccioData
{
    public string accio;
    public string actuador;
    public string selector;
}
[System.Serializable]
public class DadesGuardarFitxa
{
    public DadesEfecteFitxa Efecte;
    public string TextEfecte;
}