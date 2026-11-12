using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InventariPartida : MonoBehaviour
{
    [SerializeField] private List<FitxaPartida> ma = new List<FitxaPartida>();
    [SerializeField] private int limitMa = 3;
    [SerializeField] private Stack<FitxaPartida> bossa = new Stack<FitxaPartida>();
    [SerializeField] private int limitBossa;
    [SerializeField] GameObject prefabFitxa;
    
    [SerializeField] private Bossa dadesBossa;
    
    [SerializeField] private float midaAmpladaMa = 10f;    // amplada total de la zona on posar les fitxes a la mà
    [SerializeField] private float midaAlçadaMa = 2f;      // altura total de la zona on posar les fitxes a la mà
    

    public void Inicialitza(int numjugador, bool jugadorIA, Accions accions)
    {
        ma.Clear();
        bossa.Clear();
        InicialitzaBossa(numjugador, accions);
        if (!jugadorIA) InicialitzaMa();
    }
    // inicialitza la bossa amb limitBossa fitxes
    private void InicialitzaBossa(int numjugador, Accions accions)
    {
        foreach (Fitxa fitxa in dadesBossa.fitxes)
        {
            // instanciar fitxa i assignar dades
            FitxaPartida fitxaPosar = Instantiate(prefabFitxa).GetComponent<FitxaPartida>();
            fitxaPosar.Setup(fitxa, numjugador, accions);
            fitxaPosar.gameObject.SetActive(false);
            fitxaPosar.SetColisio(false);
            bossa.Push(fitxaPosar);
        }
        Remena();
    }
    // Agafa tantes fitxes com Limit ma
    private void InicialitzaMa()
    {
        for (int i = 0; i < limitMa; i++)
        {
            if (bossa.Count > 0)
            {
                FitxaPartida fitxa = bossa.Pop();
                float midaCasella = midaAmpladaMa / limitMa;
                Vector3 posFitxa = new (transform.position.x + i * midaCasella + midaCasella/2, transform.position.y , transform.position.z + midaAlçadaMa / 2);
                fitxa.transform.position = posFitxa;
                fitxa.SetColisio(false);
                fitxa.gameObject.SetActive(true);
                ma.Add(fitxa);
            }
        }
    }
    // aleatoritza la bossa de fitxes
    public void Remena()
    {
        List<FitxaPartida> llistaRemenar = new List<FitxaPartida>(bossa);
        for (int i = 0; i < llistaRemenar.Count; i++) {
            FitxaPartida temp = llistaRemenar[i];
            int randomIndex = Random.Range(i, llistaRemenar.Count);
            llistaRemenar[i] = llistaRemenar[randomIndex];
            llistaRemenar[randomIndex] = temp;
        }
        bossa.Clear();
        foreach (var fitxa in llistaRemenar)
        {
            bossa.Push(fitxa);
        }
    }
    // retorna la fitxa de la ma i l'elimina
    public FitxaPartida TreuFitxa(int num)
    {
        FitxaPartida fitxa = ma[num];
        ma[num] = null;
        return fitxa;
    }
    // diu si la fitxa pertany a la ma del jugador
    public int FitxaEsDeMa(FitxaPartida fitxa)
    {
        int index = 0;
        foreach(FitxaPartida fitxaMa in ma)
        {
            if (fitxaMa == fitxa)
            {
                return index;
            }
            index++;
        }
        return -1;
    }
    public void PushFitxa(FitxaPartida fitxa)
    {
        bossa.Push(fitxa);
    }
    // roba una fitxa i la posa en una posicio de la mà
    public void RobaFitxa()
    {
        if (bossa.Count > 0)
        {
            for (int i = 0; i < limitMa; i++)
            {
                if (ma[i] == null)
                {
                    FitxaPartida fitxa = bossa.Pop();
                    ma[i] = fitxa;
                    float midaCasella = midaAmpladaMa / limitMa;
                    Vector3 posFitxa = new (transform.position.x + i * midaCasella + midaCasella/2, transform.position.y , transform.position.z + midaAlçadaMa / 2);
                    fitxa.transform.position = posFitxa;
                    fitxa.gameObject.SetActive(true);
                    return;
                }
            }
        }   
    }
    // roba fitxes fins tenir la ma plena o no tenir fitxes en la bossa
    public void OmpleMa()
    {
        for (int i = 0; i < limitMa; i++)
        {
            if (ma[i] == null && bossa.Count > 0)
            {
                FitxaPartida fitxa = bossa.Pop();
                ma[i] = fitxa;
                float midaCasella = midaAmpladaMa / limitMa;
                Vector3 posFitxa = new (transform.position.x + i * midaCasella + midaCasella/2, transform.position.y , transform.position.z + midaAlçadaMa / 2);
                fitxa.transform.position = posFitxa;
                fitxa.gameObject.SetActive(true);
            }
        }
    }

    // posa la fitxa en un espai vuit de la ma i la retorna a lloc visualment
    public void AgafaFitxa(FitxaPartida fitxa)
    {
        for (int i = 0; i < limitMa; i++)
        {
            if (ma[i] == null || ma[i] == fitxa)
            {
                ma[i] = fitxa;
                float midaCasella = midaAmpladaMa / limitMa;
                Vector3 posFitxa = new (transform.position.x + i * midaCasella + midaCasella/2, transform.position.y , transform.position.z + midaAlçadaMa / 2);
                fitxa.transform.position = posFitxa;
                fitxa.gameObject.SetActive(true);
                fitxa.SetColisio(true);
                return;
            }
        }
        // si no hi ha espai en la ma la barregem dins la bossa
        bossa.Push(fitxa);
        Remena();
    }

    public FitxaPartida GetFitxaAleatoria()
    {
        if (bossa.Count != 0) return bossa.Peek();
        return null;
    }
    public FitxaPartida TreuFitxaAleatoria()
    {
        if (bossa.Count != 0) return bossa.Pop();
        return null;
    }
    public bool inventariVuit()
    {
        if (bossa.Count == 0)
        {
            for (int i = 0; i < limitMa; i++)
            {
                if (ma[i] != null) return false;
            }
            return true;
        }
        return false;
    }

    public bool bossaVuida()
    {
        return bossa.Count <= 0;
    }
    // dibuixa una referencia
    void OnDrawGizmos()
    {
        Vector3 origen = transform.position;
        Gizmos.color = Color.green;
        for (int i = 0; i <= 1; i++)
        {
            Vector3 inici = origen + new Vector3(0, 0, i*midaAlçadaMa);
            Vector3 fi = origen + new Vector3(midaAmpladaMa, 0, i*midaAlçadaMa);
            Gizmos.DrawLine(inici, fi);
        }
        for (int i = 0; i <= limitMa; i++)
        {
            Vector3 inici = origen + new Vector3(i*(midaAmpladaMa/limitMa), 0, 0);
            Vector3 fi = origen + new Vector3(i*(midaAmpladaMa/limitMa), 0, midaAlçadaMa);
            Gizmos.DrawLine(inici, fi);
        }
    }
}
