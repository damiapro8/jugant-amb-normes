using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading.Tasks;
using System.Text;
using TMPro;
public class Jugador : MonoBehaviour
{
    public int ID;
    public string nom;
    [SerializeField] protected InventariPartida inventari;
    public GameObject prefabCotxe;
    [SerializeField] protected Cotxe cotxe;
    public GameObject prefabInventari;
    public Vector2 Direccio = Vector2.zero;
    public int força = 0;
    protected FitxaPartida fitxaPosar;
    protected (int x, int y, int z) posFitxaPosar;
    protected Quaternion rotFitxaPosar;
    protected Tauler tauler;

    public virtual void Inicialitza(int i, Accions accions, TextMeshProUGUI text)
    {
    }
    // diu si la fitxa pertany a la ma del jugador i la treu d'allà
    public bool FitxaEsDeMa(FitxaPartida fitxa)
    {
        return inventari.FitxaEsDeMa(fitxa) >= 0;
    }
    // roba fitxes fins tenir la ma plena
    public virtual void RobaFitxes()
    {
    }
    public virtual void AgafaFitxa(FitxaPartida fitxa)
    {
        fitxa.SetColisio(true);
        fitxa.gameObject.SetActive(true);
        inventari.AgafaFitxa(fitxa);
    }
    public FitxaPartida TreuFitxa(FitxaPartida fitxa)
    {
        int pos = inventari.FitxaEsDeMa(fitxa);
        return inventari.TreuFitxa(pos);
    }
    public void SetCotxe(Cotxe c)
    {
        cotxe = c;
    }
    public virtual Vector2 GetDirDesicio()
    {
        return Direccio;
    }
    public virtual int GetForcaDesicio()
    {
        return força;
    }
    public virtual void SetObjectiu(Transform tr)
    {
    }
    public virtual FitxaPartida GetFitxaPosar()
    {
        return TreuFitxa(fitxaPosar);
    }
    public virtual (int x,int y,int z) GetPosFitxaPosar()
    {
        return posFitxaPosar;
    }
    public virtual Quaternion GetRotFitxaPosar()
    {
        return rotFitxaPosar;
    }
    public void SetTauler(Tauler reftauler)
    {
        tauler = reftauler;
    }
    public virtual async Task CalculaSeguentMoviment(bool torn0)
    {
        await Task.Yield();
    }
    public bool teFitxes()
    {
        return !inventari.inventariVuit();
    }
    protected void DestrueixFitxaSimular()
    {
        if (fitxaPosar != null)
        {
            fitxaPosar.gameObject.SetActive(false);
            fitxaPosar.SetColisio(false);
        }
    }

    //inputs de jugador persona:
    public virtual void EsperarSeleccioFitxa(InputAction.CallbackContext context) {}
    public virtual void CancelarSeleccioFitxa(InputAction.CallbackContext context){}
    public virtual void posaFitxa(InputAction.CallbackContext context) {}
    public virtual void canviaCapa(InputAction.CallbackContext context) {}
    public virtual void rotarFitxa(InputAction.CallbackContext context) {}
    public virtual void ConfirmarPosicio(InputAction.CallbackContext context) {}
    public virtual void SeleccionaDirCotxe(InputAction.CallbackContext context) {}
    public virtual void ConfirmarDirCotxe(InputAction.CallbackContext context) {}
}