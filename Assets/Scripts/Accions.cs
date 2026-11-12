using UnityEngine;

public class Accions
{
    public void Executa(AccioData accio, Contexte contexte)
    {
        GameObject actuador = TradueixActuador(accio, contexte);
        if (actuador != null) ExecutaAccio(accio.accio, actuador, contexte);
    }

    private GameObject TradueixActuador(AccioData accio, Contexte contexte)
    {
        switch (accio.actuador)
        {
            case "Cotxe":
                switch (accio.selector)
                {
                    case "Usuari":
                    return contexte.partida.GetCotxe(contexte.fitxa.propietari).gameObject;
                    case "AmbObjecte":
                    Cotxe cotxe = contexte.partida.GetCotxeAmbObjecte();
                    if (cotxe != null) return cotxe.gameObject;
                    return null;
                    default:
                    break;
                }
            break;
            case "Fitxa":
                switch (accio.selector)
                {
                    case "Aquesta":
                    return contexte.fitxa.gameObject;
                    default:
                    break;
                }
            break;
            default:
            break;
        }
        return null;
    }
    private void ExecutaAccio(string accio, GameObject actuador, Contexte contexte)
    {
        switch (accio)
        {
            case "Destrueix": 
                Cotxe cotxeObj = Utils.BuscaComponentRecursiu<Cotxe>(actuador.transform);
                if (cotxeObj != null) contexte.partida.EliminaCotxe(cotxeObj);
                FitxaPartida fitxa = Utils.EsFitxa(actuador);
                if (fitxa != null) contexte.partida.EliminaFitxa(fitxa);
            break;
            default:
            break;
        }
    }
}
