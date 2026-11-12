using UnityEngine;
using System.IO;
public class FitxaEdicio : MonoBehaviour
{
    [SerializeField] private Fitxa dadesFitxa;
    public void Inicialitza(Fitxa novaFitxa)
    {
        dadesFitxa = novaFitxa;
        InstanciaObjecte();
    }
    private void InstanciaObjecte()
    {
        while (transform.childCount > 0)
        {
            Transform child = transform.GetChild(0);
            child.SetParent(null);
            Destroy(child.gameObject);
        }
        Instantiate(dadesFitxa.model, transform);
    }

    public void modificaEvent(DadesEfecteFitxa dades, string text)
    {
        // aixo no es guarda en el joc real s'ha de crear fitjers json
        // dadesFitxa.Efecte = dades;
        // dadesFitxa.TextEfecte = text;
        Utils.GuardaDadesFitxa(dadesFitxa.ID, dades, text);
    }
    public string GetTextEvent()
    {
        DadesGuardarFitxa dades = Utils.CarregaDadesFitxa(dadesFitxa.ID);
        if (dades != null) return dades.TextEfecte;
        return "";
    }
}
