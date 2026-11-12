using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using System.Threading.Tasks;
using System.Text;
using TMPro;
public class Contexte
{
    public Partida partida;
    public FitxaPartida fitxa;
}
public class Partida : MonoBehaviour
{
    private enum estat
    {
        setupPartida,
        cridarJugadors,
        esperantAccioJugadors,
        setupFaseMoviment,
        faseMoviment,
        FinalTorn,
        victoria
    }
    public struct NovaFitxa
    {
        public int jugador;
        public (int x, int y, int z) pos;
        public FitxaPartida fitxa;
        public Quaternion rot;
        public bool potPosar;
    }
    [SerializeField] private estat estatActual = estat.setupFaseMoviment;
    [SerializeField] private int torn = 0;
    //TODO Hauria de ser una llista de jugadors o tenir el jugador usuari i els altres
    // [SerializeField] private Jugador jugador;
    [SerializeField] private List<Jugador> jugadors;
    [SerializeField] private Dictionary<Jugador, int> jugadorsSenseTorn;
    [SerializeField] private int numJugadors;
    [SerializeField] private GameObject prefabJugador;
    private List<NovaFitxa> novesFitxes;
    

    [SerializeField] private GameObject prefabJugadorIA;

    [SerializeField] private GameObject prefabTauler;
    [SerializeField] private Tauler tauler;
    
    public bool ferForça = false;
    [SerializeField] private float mutiplicadorForça = 40f;
    [SerializeField] private float sumadorForça = 40f;

    private float tempsComprovacioMoviment = 0.5f;
    private int maxComprovacionsMoviment = 20; // 40 * 0.5 = 20 segons abans de frenar en sec el moviment
    private int comprovacioMovimentActual = 0;
    private float tempsMoviment = 0f;
    
    [SerializeField] private List<Vector3> posCotxesInicial = new List<Vector3>();
    private int IDGuanyador;

    [SerializeField] private Accions accions;
    [SerializeField] private SubscripcioEvent subEvents;
    [SerializeField] private TextMeshProUGUI textGuia;
    private Contexte contexte;

    [SerializeField] private AudioSource audioServo;
    [SerializeField] private AudioClip Servo;
    private void Awake()
    {
        accions = new Accions();
        subEvents = new SubscripcioEvent();
        contexte = new Contexte();
        contexte.partida = this;
    }
    void Update()
    {
        switch (estatActual)
        {
            case estat.setupPartida:
            // instanciar el tauler
            torn = 0; // el torn 0 inicialitza variables i es el torn on es posa la fitxa inicial
            tauler = Instantiate(prefabTauler, transform).GetComponent<Tauler>();
            tauler.transform.position = tauler.GetOffsetPosTauler(transform.position);
            tauler.inicialitza(numJugadors);
            // instanciar el jugador: modificar en posar mes jugadors.
            jugadors = new List<Jugador>();
            jugadorsSenseTorn = new Dictionary<Jugador, int>();
            jugadors.Add(Instantiate(prefabJugador, tauler.transform).GetComponent<Jugador>());
            jugadors[0].transform.position -= new Vector3(0,0,1) * 2f;
            jugadors[0].Inicialitza(0, accions, textGuia);
            jugadors[0].SetTauler(tauler);
            for (int i = 1; i < numJugadors; i++)
            {
                jugadors.Add(Instantiate(prefabJugadorIA, tauler.transform).GetComponent<Jugador>());
                jugadors[i].transform.position -= new Vector3(0,0,1) * 2f;
                jugadors[i].Inicialitza(i, accions, textGuia);
                jugadors[i].SetObjectiu(tauler.getModelObjecte());
                jugadors[i].SetTauler(tauler);
            }
            estatActual = estat.cridarJugadors;
            novesFitxes = new List<NovaFitxa>();
            break;
            case estat.cridarJugadors:
            CridarJugadors();
            estatActual = estat.esperantAccioJugadors;
            break;
            case estat.setupFaseMoviment:
            if (novesFitxes != null) novesFitxes.Clear();
            if (torn == 0)
            {
                
                int i = 0;
                foreach (Jugador jugador in jugadors)
                {
                    
                    posCotxesInicial.Add(tauler.PosTaulerAMon(tauler.posicionsInicials[i]));
                    tauler.cotxes.Add(Instantiate(jugador.prefabCotxe, posCotxesInicial[jugador.ID], Quaternion.identity).GetComponent<Cotxe>());
                    Cotxe cotxejugador = tauler.cotxes[i];
                    jugador.SetCotxe(cotxejugador);
                    
                    cotxejugador.Inicialitza(jugador.ID, posCotxesInicial[jugador.ID], contexte, subEvents);
                    // Instanciar les fitxes
                    NovaFitxa novaFitxa;
                    novaFitxa.pos = Utils.VecATupla(tauler.posicionsInicials[jugador.ID]);
                    novaFitxa.fitxa = jugador.GetFitxaPosar(); 
                    novaFitxa.rot = jugador.GetRotFitxaPosar();
                    novaFitxa.fitxa.setJugador(jugador.ID);
                    novaFitxa.jugador = jugador.ID;
                    novaFitxa.potPosar = true;
                    novaFitxa.potPosar = !colisioFitxes(novaFitxa); // no hauria de fer falta perque es torn inicial
                    novesFitxes.Add(novaFitxa);
                    i++;
                }
            }
            else
            {
                foreach (Jugador jugador in jugadors)
                {
                    if (!jugadorsSenseTorn.ContainsKey(jugador))
                    {
                        NovaFitxa novaFitxa;
                        novaFitxa.fitxa = jugador.GetFitxaPosar();
                        if (novaFitxa.fitxa != null) // prevenir que es posi un fitxa vuida (sobretot per les IA si no tenen lloc posible)
                        {
                            novaFitxa.pos = jugador.GetPosFitxaPosar();
                            novaFitxa.rot = jugador.GetRotFitxaPosar();
                            novaFitxa.fitxa.setJugador(jugador.ID);
                            novaFitxa.jugador = jugador.ID;
                            novaFitxa.potPosar = true;
                            novaFitxa.potPosar = !colisioFitxes(novaFitxa);
                            novesFitxes.Add(novaFitxa);
                        }
                        else
                        {
                            novaFitxa.potPosar = false;
                        }
                    }
                }
            }
            // instanciar les fitxes que no tenen colisió
            foreach (var novaFitxa in novesFitxes)
            {
                if (novaFitxa.potPosar)
                {
                    AssignaFitxa(novaFitxa);
                }
                else
                {
                    if (novaFitxa.fitxa != null) jugadors[novaFitxa.jugador].AgafaFitxa(novaFitxa.fitxa); // tornar la fitxa que ha fet colisió
                }
            }
            tauler.centraCotxes();
            tauler.congelaCotxes(false);
            if (torn != 0) 
            {
                audioServo.PlayOneShot(Servo);
                ferForça = true;
            }
            estatActual = estat.faseMoviment;
            tempsMoviment = 0f;
            break;
            case estat.faseMoviment:
            // esperem a que els cotxes es parin de moures en fixed update
            break;
            case estat.FinalTorn:
            cridarEnAcabarTorn();
            tauler.congelaCotxes(true);
            foreach (Jugador jugador in jugadors)
            {
                jugador.RobaFitxes();
                if (jugadorsSenseTorn.ContainsKey(jugador))
                {
                    if (jugadorsSenseTorn[jugador] <= 0)
                    {
                        jugadorsSenseTorn.Remove(jugador);
                    }
                    else
                    {
                        jugadorsSenseTorn[jugador]--;
                    }
                }
            }
            estatActual = estat.cridarJugadors;
            torn++;
            break;
            case estat.victoria:
            textGuia.text = $"JUGADOR {jugadors[IDGuanyador].nom} HA GUANYAT !!!";
            Instantiate(jugadors[IDGuanyador].prefabCotxe, tauler.PosTaulerAMon(tauler.posObjecte), Quaternion.identity).GetComponent<Cotxe>().Inicialitza(IDGuanyador, posCotxesInicial[IDGuanyador], contexte, subEvents);
            break;
            default:
            break;
        }
    }
    // fer coses amb fisiques sol anar millor fer amb fixedUpdate
    void FixedUpdate()
    {
        if (ferForça)
        {
            // demanar dir i força a jugadors
            foreach (Jugador jugador in jugadors)
            {
                if (!jugadorsSenseTorn.ContainsKey(jugador))
                {
                    tauler.cotxes[jugador.ID].AplicaForça(jugador.GetDirDesicio(), jugador.GetForcaDesicio() * mutiplicadorForça + sumadorForça);
                }
            }
            ferForça = false;
            estatActual = estat.faseMoviment;
        }
        else if (estatActual == estat.faseMoviment)
        {
            tempsMoviment += Time.deltaTime;
            if (tempsMoviment > tempsComprovacioMoviment)
            {
                comprovacioMovimentActual += 1;
                // esperar a que tots els jugadors parin o es caiguin
                bool totsquiets = true;
                foreach (Jugador jugador in jugadors)
                {
                    if (!tauler.cotxes[jugador.ID].HaParat())
                    {
                        if (tauler.cotxes[jugador.ID].transform.position.y >= tauler.yplaEliminar)
                        {
                            // el jugador no ha parat ni ha caigut
                            totsquiets = false;
                            break;
                        }
                    }
                }
                if (totsquiets || comprovacioMovimentActual > maxComprovacionsMoviment)
                {
                    Vector3 posSeguent = Vector3.zero;
                    foreach (Jugador jugador in jugadors)
                    {
                        Vector3 posCotxe = tauler.cotxes[jugador.ID].gameObject.transform.position;
                        FitxaPartida fitxaDeSota = Utils.getFitxaDeSota(posCotxe);
                        if (fitxaDeSota == null)
                        {
                            // el cotxe no te fitxa a sota = a caigut del tauler
                            EliminaCotxe(tauler.cotxes[jugador.ID]);
                            posSeguent = posCotxesInicial[jugador.ID];
                        }
                        else
                        {
                            posSeguent = Utils.GetPosSeguentCotxe(posCotxe, tauler, tauler.cotxes[jugador.ID].offsetCotxe);
                            bool posinicial = tauler.GuardaPosCotxe(jugador.ID, posSeguent);
                            if (posinicial && tauler.cotxes[jugador.ID].teObjecte)
                            {
                                // ha portat l'objecte a l'inici -> jugador ha guanyat la partida
                                estatActual = estat.victoria;
                                IDGuanyador = jugador.ID;
                                tempsMoviment = 0f;
                                return;
                            }
                        }
                        tauler.GuardaPosCotxe(jugador.ID, posSeguent);
                    }
                    comprovacioMovimentActual = 0;
                    estatActual = estat.FinalTorn;
                    tauler.centraCotxes();
                }
                tempsMoviment = 0f;
            }
        }
    }

    private async void CridarJugadors()
    {
        foreach (Jugador jugador in jugadors)
        {
            if (!jugadorsSenseTorn.ContainsKey(jugador))
            {
                await jugador.CalculaSeguentMoviment(torn == 0);
            }
        }
        estatActual = estat.setupFaseMoviment; // si arriba aqui els jugadors ja han decidit la seva accio
    }
    public void EliminaCotxe(Cotxe cotxeEliminat)
    {
        cotxeEliminat.ResetPos(Utils.GetPosSeguentCotxe(posCotxesInicial[cotxeEliminat.jugador], tauler, cotxeEliminat.offsetCotxe));
        if (cotxeEliminat.teObjecte)
        {
            //retornar l'objecte a la posicio inicial
            Transform model = cotxeEliminat.retornaObjecte();
            tauler.retornaObjecte(model);
        }
        // fer que perdi el torn
        if (jugadorsSenseTorn.ContainsKey(jugadors[cotxeEliminat.jugador]))
        {
            jugadorsSenseTorn[jugadors[cotxeEliminat.jugador]] += 1;
        }
        else
        {
            jugadorsSenseTorn[jugadors[cotxeEliminat.jugador]] = 1;
        }
        
    }
    // assigna la fitxa al tauler
    private void AssignaFitxa(NovaFitxa novaFitxa)
    {
        novaFitxa.fitxa.gameObject.SetActive(true);
        novaFitxa.fitxa.SetColisio(true);
        FitxaPartida fitxa = Instantiate(novaFitxa.fitxa, tauler.PosTaulerAMon(novaFitxa.pos), novaFitxa.rot);
        fitxa.Setup(novaFitxa.fitxa, accions);
        fitxa.MostraCub(false);
        fitxa.SetAccions(accions);
        string iniciadorFitxa = fitxa.GetIniciador();
        if (iniciadorFitxa != "")
        {
            subEvents.Subscriu(iniciadorFitxa, fitxa);
        }
        if (!tauler.SetFitxa(fitxa, novaFitxa.pos)) // la fitxa no s'ha pogut posar -> ja hi ha una fitxa alla -> substituir fitxa
        {
            jugadors[novaFitxa.jugador].AgafaFitxa(TreuRefsFitxa(novaFitxa.pos));
            tauler.SetFitxa(fitxa, novaFitxa.pos);
        }
        Destroy(novaFitxa.fitxa.gameObject);
    }
    private bool colisioFitxes(NovaFitxa fitxa)
    {
        bool colisio = false;
        for (int i = 0; i < novesFitxes.Count; i++)
        {
            if (novesFitxes[i].pos == fitxa.pos && fitxa.jugador != novesFitxes[i].jugador)
            {
                var f = novesFitxes[i];
                f.potPosar = false;
                novesFitxes[i] = f;
                colisio = true;
            }
        }
        return colisio;
    }

    public Cotxe GetCotxe(int jugador)
    {
        return tauler.cotxes[jugador];
    }

    private void cridarEnAcabarTorn()
    {
        subEvents.Executa("EnAcabarTorn", contexte);
    }

    public FitxaPartida TreuRefsFitxa(FitxaPartida fitxa)
    {
        subEvents.DesubscriuTot(fitxa);
        foreach (var cotxe in tauler.cotxes)
        {
            cotxe.EliminaFitxaDeColisions(fitxa);
        }
        return tauler.treuFitxa(tauler.MonAPosTauler(fitxa.transform.position));
    }

    public FitxaPartida TreuRefsFitxa((int x, int y, int z) posFitxa)
    {
        FitxaPartida fitxa = tauler.treuFitxa(posFitxa);
        subEvents.DesubscriuTot(fitxa);
        foreach (var cotxe in tauler.cotxes)
        {
            cotxe.EliminaFitxaDeColisions(fitxa);
        }
        return fitxa;
    }
    public void EliminaFitxa(FitxaPartida fitxa)
    {
        
        Destroy(TreuRefsFitxa(fitxa).gameObject);
    }

    public Cotxe GetCotxeAmbObjecte()
    {
        foreach (Cotxe cotxe in tauler.cotxes)
        {
            if (cotxe.teObjecte)
            {
                return cotxe;
            }
        }
        return null;
    }
}
