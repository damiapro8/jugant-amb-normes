using UnityEngine;
using UnityEngine.InputSystem;
public class Edicio : MonoBehaviour
{
    private FitxaEdicio fitxaActual;
    [SerializeField] CanviadorColor textManager;
    [SerializeField] AutocompletatUI autoc;
    public void EsperarSeleccioFitxa(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            FitxaEdicio fitxaClicada = Utils.getFitxaEdicioSeleccionada();
            if (fitxaClicada != null)
            {
                fitxaActual = fitxaClicada;
                textManager.CanviaText(fitxaActual.GetTextEvent());
            }
        }
    }

    public void autocompletar(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            autoc.ExecutarAutocompletat();
        }
    }
    public void modificaEvent(DadesEfecteFitxa dades, string text)
    {
        if (fitxaActual != null)
        {
            fitxaActual.modificaEvent(dades,text);
        }
    }
}
