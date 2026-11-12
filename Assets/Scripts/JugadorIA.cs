using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using TMPro;
public class JugadorIA : Jugador
{
    private readonly Vector2[] direccionsPosibles = new Vector2[]
{
    new Vector2(0, 1),   // 0: Amunt
    new Vector2(0, -1),  // 1: Avall
    new Vector2(1, 0),   // 2: Dreta
    new Vector2(-1, 0)   // 3: Esquerra
};
    public AccioIA millorAccio;
    private Transform objectiu;
    [SerializeField] private float mutiplicadorForça = 40f;
    [SerializeField] private float sumadorForça = 40f;
    [SerializeField] private Vector3 posObjectiu;
    public struct AccioIA {
    public int Direccio;
    public int força;
    public float distanciaAlObjectiu;
    }
    public override void Inicialitza(int i, Accions accions, TextMeshProUGUI text)
    {
        ID = i;
        inventari = Instantiate(prefabInventari, transform).GetComponent<InventariPartida>();
        inventari.Inicialitza(ID, true, accions);
    }
    public override void RobaFitxes()
    {
    }
    public override void AgafaFitxa(FitxaPartida fitxa)
    {
        inventari.PushFitxa(fitxa);
        inventari.Remena();
    }
    public override async Task CalculaSeguentMoviment(bool torn0)
    {
        await Task.Yield();
        if (torn0)
        {
            fitxaPosar = inventari.TreuFitxaAleatoria();
            return;
        }
        if (cotxe.teObjecte) posObjectiu = tauler.PosTaulerAMon(tauler.posicionsInicials[ID]);
        else posObjectiu = objectiu.position;
        CalculaFitxaPosar(); // calculem la posicio de la seguent fitxa
        creaFitxaSimular(); // posem la fitxa en el tauler per simular amb ella
        provaCamins(); // simulem diferents moviments amb la fitxa posada
        DestrueixFitxaSimular(); // destruim la fitxa un cop acabada la simulacio
    }
    private void provaCamins()
    {
        tauler.congelaCotxes(false);
        cotxe.modeSimulacio = true;
        Vector3 posOriginal = cotxe.transform.position;
        Quaternion rotacioOriginal = cotxe.transform.rotation;
        millorAccio.distanciaAlObjectiu = float.MaxValue;
        millorAccio.Direccio = 0; // assegurar que dongui un valor si totes les sortides no son posibles
        millorAccio.força = 1;
        Physics.simulationMode = SimulationMode.Script;
        // simula els resultats de fer el moviment en cada una de les direccions i forçes posibles
        for (int dir = 0; dir < 4; dir++)
        {
            for (int f = 1; f < cotxe.velMax; f++)
            {
                Vector2 dirVector = direccionsPosibles[dir];
                // prova força amb les 4 direccions {(0,1),(0,-1),(1,0)(-1,0)} i totes les forçes
                cotxe.AplicaForça(dirVector, f * mutiplicadorForça + sumadorForça);

                for (int i = 0; i < 300; i++) // 300 passos x 0.02s = 6 segons de futur
                {
                    Physics.Simulate(Time.fixedDeltaTime);
                    if (cotxe.HaParat()) break;
                }
                Vector3 posFinal = cotxe.transform.position;
                AccioIA accioActual = AvaluaResultat(posFinal, dir, f);

                //Debug.Log($"Simulant Dir: {dir}, Força: {f} -> Distancia: {accioActual.distanciaAlObjectiu} Pos {posFinal}");
                if (accioActual.distanciaAlObjectiu < millorAccio.distanciaAlObjectiu)
                {
                    millorAccio = accioActual;
                }
                // retornar el tauler a com estava per la seguent simulació
                tauler.centraCotxes(); // centra Tauler congela en acabar
                tauler.congelaCotxes(false);
            }
        }
        tauler.congelaCotxes(true);
        Physics.simulationMode = SimulationMode.FixedUpdate;
        cotxe.modeSimulacio = false;
    }
    private AccioIA AvaluaResultat(Vector3 novaPos, int dir, int força)
    {
        AccioIA accio = new AccioIA();
        accio.Direccio = dir;
        accio.força = força;
        int layerIndex = LayerMask.NameToLayer("Fitxa");
        int layerMask = 1 << layerIndex;
        RaycastHit hit;
        Vector3 origenRaycast = novaPos + Vector3.up * 1f;
        if (Physics.Raycast(origenRaycast, Vector3.down, out hit, 10f, layerMask))
        {
            if(Utils.EsFitxa(hit.transform.gameObject) != null)
            {
                accio.distanciaAlObjectiu = Vector3.Distance(novaPos, posObjectiu);
            }
            else
            {
                accio.distanciaAlObjectiu = float.MaxValue;
            }
        }
        else
        {
            accio.distanciaAlObjectiu = float.MaxValue;
        }
        return accio;
    }

    public override Vector2 GetDirDesicio()
    {
        int dir = millorAccio.Direccio;
    
        if (dir >= 0 && dir < direccionsPosibles.Length)
        {
            return direccionsPosibles[dir];
        }
        return Vector2.up;
    }
    public override int GetForcaDesicio()
    {
        return millorAccio.força;
    } 
    public override FitxaPartida GetFitxaPosar()
    {
        return inventari.TreuFitxaAleatoria();
    }

    public override void SetObjectiu(Transform tr)
    {
        objectiu = tr;
    }
    // crea i posa la fitxa en el tauler per poder fer simulacions amb la fitxa
    private void creaFitxaSimular()
    {
        FitxaPartida fitxa = inventari.GetFitxaAleatoria();
        if (fitxa != null)
        {
            fitxa.gameObject.SetActive(true);
            fitxa.SetColisio(true);
            fitxa.transform.position = tauler.PosTaulerAMon(posFitxaPosar);
        }
        fitxaPosar = fitxa;
    }
    
    public override Quaternion GetRotFitxaPosar()
    {
        return Quaternion.identity;
    }
    private void CalculaFitxaPosar()
    {
        (int x,int y, int z) casellaCotxe = tauler.MonAPosTaulerCotxe(cotxe.transform.position);
        List<(int x, int y, int z)> casellesPosibles = CasellesPosibles(casellaCotxe);
        (int x, int y, int z) millorCasella = (int.MaxValue, int.MaxValue,int.MaxValue);
        if (casellesPosibles != null)
        {
            foreach (var casella in casellesPosibles)
            {
                if (Vector3.Distance(tauler.PosTaulerAMon(casella), posObjectiu) < Vector3.Distance(tauler.PosTaulerAMon(millorCasella), posObjectiu))
                {
                    millorCasella = casella;
                }
            }
            posFitxaPosar = millorCasella;
        }
    }
    // retorna totes les posicions on posar fitxes en una creu (altura com a eix) o en un 3x3 de la posicio actual del cotxe
    private List<(int x, int y, int z)> CasellesPosibles((int x,int y,int z) casellaCotxe)
    {
        List<(int x, int y, int z)> res = new List<(int x, int y, int z)>();
        for (int x = -1; x <= 1; x++)
        {
            
                (int x, int y, int z) casella = casellaCotxe;
                casella.x += x;
                if (tauler.potPosarFitxa(casella))
                {
                    res.Add(casella);
                }
        }
        for (int y = -1; y <= 1; y++)
        {
            (int x, int y, int z) casella = casellaCotxe;
            casella.y += y;
            if (tauler.potPosarFitxa(casella))
            {
                res.Add(casella);
            }
        }
        if (res.Count == 0) // si no hi ha ampliar a 3x3
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        (int x, int y, int z) casella = casellaCotxe;
                        casella.x += x;
                        casella.y += y;
                        casella.z += z;
                        if (tauler.potPosarFitxa(casella))
                        {
                            res.Add(casella);
                        }
                    }
                }
            }
        }
        return res;
    }
}
