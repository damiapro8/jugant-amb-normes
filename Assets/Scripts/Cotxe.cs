using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cotxe : MonoBehaviour
{
    public int comptador;
    public int jugador;
    public float velMax = 4f;
    public Vector3 velActual;
    [SerializeField] private GameObject model;
    [SerializeField] private Transform modelObjecte;
    private Rigidbody rb;
    public bool teObjecte = false;
    [SerializeField] private bool comprovaColisions = false;
    [SerializeField] private LayerMask capaObjecte;
    [SerializeField] private LayerMask capaCotxe;
    [SerializeField] private LayerMask capaFitxa;
    [SerializeField] private float llintarEliminar;
    [SerializeField] private List<FitxaPartida> LlistaColisions;
    private Vector3 posInicial;
    public bool modeSimulacio = false;
    private SubscripcioEvent subscripcio;
    private Contexte contexte;
    public float offsetCotxe;
    void Start()
    {
        offsetCotxe = CalculaOffset();
    }
    private float CalculaOffset()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        float offsetFonsCotxe = 0f;
        if (colliders.Length > 0)
        {
            float puntMesBaixMon = float.MaxValue; // punt més baix en coordenades mon
            foreach (Collider col in colliders)
            {
                if (col.bounds.min.y < puntMesBaixMon)
                {
                    puntMesBaixMon = col.bounds.min.y;
                }
            }
            offsetFonsCotxe = transform.position.y - puntMesBaixMon;
        }
        return offsetFonsCotxe;
    }
    void FixedUpdate()
    {
        if (comprovaColisions)
        {
            velActual = rb.linearVelocity;
        }
    }
    public void Inicialitza(int idjugador, Vector3 posinici, Contexte cont, SubscripcioEvent subs)
    {
        Instantiate(model, transform);
        rb = GetComponent<Rigidbody>();
        teObjecte = false;
        jugador = idjugador;
        comptador = 0;
        comprovaColisions = false;
        posInicial = posinici;
        LlistaColisions = new List<FitxaPartida>();
        contexte = cont;
        subscripcio = subs;
    }
    // Gira el cotxe cap a la direccio (pla 2d) i li dona una força
    public void AplicaForça(Vector2 dir, float força)
    {
        comprovaColisions=true;
        Vector3 direccio = new Vector3(dir.x, 0f, dir.y);
        transform.rotation = Quaternion.LookRotation(direccio, Vector3.up);
        rb.AddForce(direccio * força, ForceMode.Impulse);
        LlistaColisions.Clear();
        LlistaColisions.Add(Utils.getFitxaDeSota(transform.position));
    }

    public void ResetPos(Vector3 pos)
    {
        this.transform.position = pos;
        this.transform.rotation = Quaternion.identity;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        comprovaColisions = false;
        Physics.SyncTransforms();
    }
    public void TornaAInici()
    {
        ResetPos(posInicial);
    }

    public bool HaParat()
    {
        if (rb.linearVelocity.magnitude < 0.001f)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return true;
        }
        return false;
    }

    private void OnTriggerEnter(Collider obj)
    {
        if (!comprovaColisions || modeSimulacio) return;
        if (((1 << obj.gameObject.layer) & capaObjecte) != 0)
        {
            Transform model = obj.transform.BuscaTag("Objecte");
            if (model != null) agafaObjecte(model);
        }
    }

    private void OnCollisionEnter(Collision obj)
    {
        if (!comprovaColisions || modeSimulacio) return;
        if (((1 << obj.gameObject.layer) & capaCotxe) != 0)
        {
            GameObject rival = obj.gameObject;
            Cotxe cotxeRival = rival.transform.BuscaComponentRecursiu<Cotxe>();
            if (velActual.magnitude - cotxeRival.velActual.magnitude > llintarEliminar)
            {
                // en cas de xoc li robem l'objecte
                if (cotxeRival.teObjecte) 
                {
                    Transform objecte = cotxeRival.retornaObjecte();
                    agafaObjecte(objecte);
                }
                cotxeRival.TornaAInici();
                rb.linearVelocity = velActual;
            }
        }
        else if (((1 << obj.gameObject.layer) & capaFitxa) != 0)
        {
            FitxaPartida fitxa = Utils.EsFitxa(obj.gameObject);
            if (fitxa!=null)
            {
                if (!LlistaColisions.Contains(fitxa))
                {
                    LlistaColisions.Add(fitxa);
                }
                if (subscripcio.subscripcions.ContainsKey("EnXocar") && subscripcio.subscripcions["EnXocar"].Contains(fitxa))
                {
                    subscripcio.ExecutaUnic("EnXocar", fitxa, contexte);
                }
            }
        }
    }
    private void agafaObjecte(Transform model)
    {
        teObjecte = true;
        modelObjecte = model;
        modelObjecte.SetParent(this.transform);
        modelObjecte.localPosition = new Vector3(0, 0.5f, 0);
        modelObjecte.localRotation = Quaternion.identity;
    }
    public Transform retornaObjecte()
    {
        teObjecte = false;
        return modelObjecte;
    }
    public List<FitxaPartida> GetColisions()
    {
        if (LlistaColisions.Count > 0)
            return LlistaColisions;
        else
            return null;
    }
    public void Congela(bool congela)
    {
        rb.isKinematic = congela;
    }
    public void EliminaFitxaDeColisions(FitxaPartida fitxa)
    {
        LlistaColisions.Remove(fitxa);
    }
    public void SetSubscripcions(SubscripcioEvent subs)
    {
        subscripcio = subs;
    }
}
