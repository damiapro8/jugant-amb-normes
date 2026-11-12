using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Threading.Tasks;
using System.Text;
using TMPro;

public class JugadorPersona : Jugador
{
    private enum estat
    {
        esperantFitxaJugador,
        posantFitxaJugador,
        rotantFitxaJugador,
        esperantMovCotxe,
        llest
    }
    [SerializeField] private estat estatActual = estat.llest;
    private bool tornInicial = true;
    private bool movimentLlest = false;
    private int capaActual = 0;

    private Vector3 VecRotarFitxa;
    private Vector3 puntIniciArrosegar = Vector3.zero;
    [SerializeField] private float llindarArrosegar = 2.0f;

    [SerializeField] private List<FitxaPartida> llistaSubstituibles;
    [SerializeField] private TextMeshProUGUI textGuia;
    private FitxaPartida fitxaSubstituida;
    
    [SerializeField] private controladorCamara contCam;

    [SerializeField] private AudioSource audioE1;
    [SerializeField] private AudioSource audioEMax;
    [SerializeField] private AudioClip E1;
    [SerializeField] private AudioClip EMax;
    [SerializeField] private AudioClip Click;

    void Update()
    {
        if (estatActual == estat.posantFitxaJugador)
        {
            MoureVistaPrevia();
        }
    }
    public override void Inicialitza(int i, Accions accions, TextMeshProUGUI text)
    {
        ID = i;
        inventari = Instantiate(prefabInventari, transform).GetComponent<InventariPartida>();
        inventari.Inicialitza(ID, false, accions);
        contCam = Camera.main.transform.parent.GetComponent<controladorCamara>();
        llistaSubstituibles = new List<FitxaPartida>();
        textGuia = text;
    }

    public override void RobaFitxes()
    {
        inventari.OmpleMa();
    }

    public override async Task CalculaSeguentMoviment(bool torn0)
    {
        movimentLlest = false;
        tornInicial = torn0;
        if (teFitxes())
        {
            contCam.TrancisioModeInicial();
            estatActual = estat.esperantFitxaJugador;
            textGuia.text = "Selecciona una Fitxa\nClick esquerra per seleccionar";
            if (!tornInicial)
            {
                mostraFitxesSubstituibles(true);
            }
        }
        else
        {
            contCam.Transicio(cotxe.transform.position, true, true, false, false, false);
            Debug.Log("no et queden fitxes");
            fitxaPosar = null;
            posFitxaPosar = (0,0,0);
            textGuia.text = "Mou el cotxe\nFletxes/AWSD per elegir la direcció, Presiona mes vegades la mateixa direcció per augmentar velocitat, Enter per confirmar";
            estatActual = estat.esperantMovCotxe;
        }
        while (!movimentLlest)
        {
            await Task.Yield();
        }
    }
    private Vector3 GetPosCentrada(Vector3 posicio)
    {
        float centreX = Mathf.FloorToInt(posicio.x) + tauler.midaCella / 2f;
        float centreZ = Mathf.FloorToInt(posicio.z) + tauler.midaCella / 2f;
        return new (centreX, capaActual, centreZ);
    }
    // enganxa la fitxa seleccionada al ratoli i a la cella del tauler centrat
    private void MoureVistaPrevia()
    {
        Vector3 posMonRatoli = Utils.GetPosRatoliCapa(capaActual);
        fitxaPosar.transform.position = posMonRatoli;
        (int x, int y, int z) posTaulerRatoli = tauler.MonAPosTauler(posMonRatoli, capaActual);
        bool potPosarFitxa = tauler.potPosarFitxa(posTaulerRatoli);
        if (potPosarFitxa && !inventari.bossaVuida())
        {
            fitxaPosar.transform.position = GetPosCentrada(posMonRatoli);
        }
        else if (llistaSubstituibles != null && llistaSubstituibles.Contains(tauler.GetFitxa(posTaulerRatoli)))
        {
            // hi ha una fitxa, mirem si esta dins de la llista de fitxes per substituir
            fitxaPosar.transform.position = GetPosCentrada(posMonRatoli);
        }
    }

    public override void EsperarSeleccioFitxa(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (estatActual == estat.esperantFitxaJugador)
            {
                fitxaPosar = Utils.getFitxaSeleccionada();
                if (fitxaPosar != null)
                {
                    if (!FitxaEsDeMa(fitxaPosar)) fitxaPosar = null;
                    else 
                    {
                        audioE1.PlayOneShot(Click);
                        // si la fitxa es de la ma del jugador cambiem d'estat
                        if (tornInicial) // fitxa inicial
                        {
                            posFitxaPosar = Utils.VecATupla(tauler.posicionsInicials[ID]);
                            fitxaPosar.transform.position = tauler.PosTaulerAMon(posFitxaPosar);
                            estatActual = estat.rotantFitxaJugador;
                            fitxaPosar.MostraCub(true);
                            contCam.Transicio(fitxaPosar.transform.position, false, true, true, true, false);
                            textGuia.text = "Rota la fitxa inicial\nArrosega click esquerra per rotar, Click dret per cancelar, Enter per confirmar";
                        }
                        else
                        {
                            fitxaPosar.SetColisio(false); // treure colisio per evitar empuchar objectes amb la fitxa
                            estatActual = estat.posantFitxaJugador; 
                            contCam.TrancisioModeMirar();
                            textGuia.text = "Posa la fitxa en una casella\nClick esquerra per posar. Espai i Shift per pujar i baixar. Click dret per cancelar";
                        }
                        
                    }
                }
            }
        }
    }
    // torna la fitxa a la ma en fer click dret
    public override void CancelarSeleccioFitxa(InputAction.CallbackContext context)
    {
        if (context.performed && estatActual == estat.posantFitxaJugador)
        {
            AgafaFitxa(fitxaPosar);
            fitxaPosar.SetColisio(true);
            fitxaPosar = null;
            estatActual = estat.esperantFitxaJugador;
            contCam.TrancisioModeInicial();
            
        }
        if (context.performed && estatActual == estat.rotantFitxaJugador)
        {
            if (tornInicial) 
            {
                AgafaFitxa(fitxaPosar);
                fitxaPosar.MostraCub(false);
                fitxaPosar.SetColisio(true);
                estatActual = estat.esperantFitxaJugador;
                textGuia.text = "Selecciona la Fitxa inicial\nClick esquerra per seleccionar";
                contCam.TrancisioModeInicial();
                fitxaPosar = null;
            }
            else 
            {
                if (fitxaSubstituida != null)
                {
                    fitxaSubstituida.gameObject.SetActive(true);
                    fitxaSubstituida.transform.position = tauler.PosTaulerAMon(posFitxaPosar);
                }
                estatActual = estat.posantFitxaJugador;
                textGuia.text = "Posa la fitxa en una casella\nClick esquerra per posar. Espai i Shift per pujar i baixar. Click dret per cancelar";
                contCam.TrancisioModeMirar();
                mostraFitxesSubstituibles(true);
                fitxaPosar.MostraCub(false);
            }
            
            puntIniciArrosegar = Vector3.zero;
        }
    }

    // posa la vista previa de fitxa en el tauler en fer click
    public override void posaFitxa(InputAction.CallbackContext context)
    {
        if (context.performed && estatActual == estat.posantFitxaJugador)
        {
            Vector3 posMonRatoli = Utils.GetPosRatoliCapa(capaActual);
            posFitxaPosar = tauler.MonAPosTauler(posMonRatoli, capaActual);
            bool potPosarFitxa = tauler.potPosarFitxa(posFitxaPosar);
            if (potPosarFitxa && !inventari.bossaVuida()) // posar la fitxa en un lloc vuit
            {
                fitxaPosar.transform.position = GetPosCentrada(posMonRatoli);
                // posFitxaPosar = tauler.MonAPosTauler(fitxaPosar.transform.position);
                mostraFitxesSubstituibles(false);
                fitxaPosar.MostraCub(true);
                estatActual = estat.rotantFitxaJugador;
                textGuia.text = "Rota la fitxa\nArrosega click esquerra per rotar, Click dret per cancelar, Enter per confirmar";
                contCam.Transicio(fitxaPosar.transform.position, false, true, true, true, false);
                audioE1.PlayOneShot(Click);
            }
            else if (llistaSubstituibles != null && llistaSubstituibles.Contains(tauler.GetFitxa(posFitxaPosar))) // substituir una fitxa
            {
                fitxaSubstituida = tauler.GetFitxa(posFitxaPosar);
                // hi ha una fitxa, mirem si esta dins de la llista de fitxes per substituir
                fitxaPosar.transform.position = GetPosCentrada(posMonRatoli);
                fitxaSubstituida.gameObject.SetActive(false);

                mostraFitxesSubstituibles(false);
                fitxaPosar.MostraCub(true);
                estatActual = estat.rotantFitxaJugador;
                textGuia.text = "Rota la fitxa\nArrosega click esquerra per rotar, Click dret per cancelar, Enter per confirmar";
                contCam.Transicio(fitxaPosar.transform.position, false, true, true, true, false);
                audioE1.PlayOneShot(Click);
            }
        }
    }
    public override void canviaCapa(InputAction.CallbackContext context)
    {
        if (context.performed && estatActual == estat.posantFitxaJugador)
        {
            float dir = context.ReadValue<float>();
            if (dir > 0 && capaActual < tauler.maxZ - 1)
            {
                capaActual++;
                contCam.TransportaCam(contCam.transform.position + new Vector3(0f,tauler.midaCella,0f), true, true, true, true, true);
            }
            else if (dir < 0 && capaActual > tauler.minZ)
            {
                capaActual--;
                contCam.TransportaCam(contCam.transform.position + new Vector3(0f,-tauler.midaCella,0f), true, true, true, true, true);
            }
        }
    }
    public override void rotarFitxa(InputAction.CallbackContext context)
    {
        if (estatActual != estat.rotantFitxaJugador) return;
        if (context.started)
        {
            RaycastHit hit;
            
            Vector2 posRatoli = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(posRatoli);
            if (Physics.Raycast(ray, out hit))
            {
                if (Utils.EsFitxa(hit.transform.gameObject) == fitxaPosar)
                {
                    VecRotarFitxa = hit.normal; // vector normal del pla
                    // trobar el pla del obj
                    Plane groundplane = new(VecRotarFitxa, hit.point); // creem un pla corresponent a la cara clickada
                    if(groundplane.Raycast(ray,out float distance))
                    {
                        puntIniciArrosegar =  ray.GetPoint(distance); // hem trovat la posicio del pla que s'ha clickat
                    }
                }
            }
        }
        if (context.canceled)
        {
            if (puntIniciArrosegar != Vector3.zero)
            {
                Vector2 posRatoli = Mouse.current.position.ReadValue();
                Ray ray = Camera.main.ScreenPointToRay(posRatoli);
                Plane groundplane = new(VecRotarFitxa, puntIniciArrosegar);
                Vector3 dir = Vector3.zero;
                if(groundplane.Raycast(ray,out float distance))
                {
                    Vector3 puntfinal = ray.GetPoint(distance);
                    dir = puntfinal - puntIniciArrosegar; // tenim el vector de direccio pero en coordenades mon no de pla
                    if (dir.magnitude > llindarArrosegar)
                    {
                        Quaternion rotPla = Quaternion.LookRotation(VecRotarFitxa); // rotacio amb normal de la cara
                        Vector3 dirPla = Quaternion.Inverse(rotPla) * dir; // tenint en compte la rotacio de la cara cap a dins, agafem la direccio del vector en aquest
                        // dibuixar el pla de la rotacio
                        Debug.DrawRay(puntIniciArrosegar, rotPla * Vector3.right * 2, Color.red, 1f); // Eix X local
                        Debug.DrawRay(puntIniciArrosegar, rotPla * Vector3.up * 2, Color.green, 1f);  // Eix Y local

                        Vector3 eixRotacio = Vector3.zero;

                        if (Mathf.Abs(dirPla.x) > Mathf.Abs(dirPla.y))
                        {
                            eixRotacio = rotPla * Vector3.up;
                            fitxaPosar.transform.Rotate(eixRotacio, 90f* Mathf.Sign(dirPla.x), Space.World);
                        }
                        else
                        {
                            eixRotacio = rotPla * Vector3.right;
                            fitxaPosar.transform.Rotate(eixRotacio, -90f* Mathf.Sign(dirPla.y), Space.World);
                        }
                    }
                }
                puntIniciArrosegar = Vector3.zero;
            }
        }
    }

    public override void ConfirmarPosicio(InputAction.CallbackContext context)
    {
        if (context.performed && estatActual == estat.rotantFitxaJugador)
        {
            fitxaPosar.MostraCub(false);
            fitxaPosar.SetColisio(true);
            rotFitxaPosar = fitxaPosar.transform.rotation;
            if (tornInicial)
            {
                contCam.TrancisioModeInicial();
                posFitxaPosar = tauler.MonAPosTauler(fitxaPosar.transform.position);
                estatActual = estat.llest;
                movimentLlest = true;
                textGuia.text = "";
            }
            else
            {
                contCam.Transicio(cotxe.transform.position, true, true, false, false, false);
                força = 0;
                Direccio = Vector2.zero;
                estatActual = estat.esperantMovCotxe;
                textGuia.text = "Mou el cotxe\nFletxes/AWSD per elegir la direcció, Presiona mes vegades la mateixa direcció per augmentar velocitat, Enter per confirmar";
            }
            tauler.centraCotxes();
        }
    }

    // amb les tecles direccionals selecciona la direccio, multiples cops augmenta la força
    public override void SeleccionaDirCotxe(InputAction.CallbackContext context)
    {
        if (context.performed && estatActual == estat.esperantMovCotxe)
        {
            Vector2 dir = context.ReadValue<Vector2>();
            if (Direccio == Vector2.zero || dir != Direccio)
            {
                Direccio = dir;
                força = 1;
                Vector3 direccio3 = new Vector3(dir.x, 0f, dir.y);
                cotxe.transform.rotation = Quaternion.LookRotation(direccio3, Vector3.up);
                audioE1.PlayOneShot(E1);
            }
            else if (dir == Direccio && força < cotxe.velMax)
            {
                força += 1;
                audioE1.PlayOneShot(E1);
            }
            else
            {
                audioEMax.PlayOneShot(EMax);
            }
        }
    }
    // intro per confirmar la seleccio
    public override void ConfirmarDirCotxe(InputAction.CallbackContext context)
    {
        if (context.performed && estatActual == estat.esperantMovCotxe && força > 0 && Direccio != Vector2.zero)
        {
            estatActual = estat.llest;
            movimentLlest = true;
            contCam.TrancisioModeInicial();
            DestrueixFitxaSimular();
            if (fitxaSubstituida != null) fitxaSubstituida.gameObject.SetActive(true);
            textGuia.text = "";
        }
    }
    
    private void mostraFitxesSubstituibles(bool mostra)
    {
        llistaSubstituibles = cotxe.GetColisions();
        if (llistaSubstituibles != null)
        {
            foreach (FitxaPartida fitxa in llistaSubstituibles)
            {
                if (fitxa != null)
                    fitxa.MostraCub(mostra);
            }
        }
    }

    
}
