using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Linq;

public class controladorCamara : MonoBehaviour
{
    // variables comunes
    Camera cam;
    public float tempsMoure = 5f;
    public bool moure = false;
    public bool zoom = false;
    public bool girar = false;


    // variables moure
    public float velocitatMoure = 1f;
    [SerializeField] private Vector3 puntIniciArrosegar = Vector3.zero;
    [SerializeField] private Vector3 puntActualArrosegar = Vector3.zero;
    private Vector3 novaPosicio;
    [SerializeField] private Vector3 posMirar;
    [SerializeField] private Vector3 posInicial;

    // variables rotar
    public float velocitatGirar = 4f;
    private Quaternion novaRotació;
    [SerializeField] private Vector3 posRatoliIniciGirar = Vector3.zero;
    [SerializeField] private Vector3 posRatoliActualGirar = Vector3.zero;
    [SerializeField] private Quaternion rotInicial;

    // variables zoom
    private Vector3 nouZoom;
    public float velocitatZoom = 2f;
    public float distanciaMaxZoom = 100f;
    public float distanciaMinZoom = 10f;
    [SerializeField] private Vector3 zoomInicial;
    [SerializeField] private float zoomCotxe;

    void Start()
    {
        cam = Camera.main;
        cam.transform.LookAt(this.transform);
        novaPosicio = transform.position;
        novaRotació = transform.rotation;
        nouZoom = cam.transform.localPosition;
        zoomInicial = nouZoom;
        posInicial = transform.position;
        rotInicial = transform.rotation;
    }
    void Update()
    {
        MouCamera();
    }
    public void EnMantenirClickDret(InputAction.CallbackContext context)
    {
        if (!girar) return;
        Vector2 delta = context.ReadValue<Vector2>();
        novaRotació *= Quaternion.Euler(Vector3.up * delta.x*velocitatGirar);
    }
    public void EnMantenirClick(InputAction.CallbackContext context)
    {
        if (!moure) return;
        if (context.started)
        {
            // es comença a clickar el boto (potser mantenir)
            Ray raigIniciArrosegar = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            Plane plaIniciArrosegar = new Plane(Vector3.up, Vector3.zero);
            float distancia;
            if (plaIniciArrosegar.Raycast(raigIniciArrosegar, out distancia))
            {
                puntIniciArrosegar = raigIniciArrosegar.GetPoint(distancia);
            }
        }
        else if (context.performed)
        {
            // s'esta mantenint el boto
            Ray raigActualArrosegar = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            Plane plaActualArrosegar = new Plane(Vector3.up, Vector3.zero);
            float distancia;
            if (plaActualArrosegar.Raycast(raigActualArrosegar, out distancia))
            {
                puntActualArrosegar = raigActualArrosegar.GetPoint(distancia);
                novaPosicio = transform.position + (puntIniciArrosegar - puntActualArrosegar) * velocitatMoure;
            }
        }
        // else if (context.canceled) {} fer servir si es vol fer algo en deixar anar
    }
    public void TrancisioModeMirar()
    {
        novaPosicio = posMirar;
        moure = true;
        zoom = true;
        girar = true;
    }

    public void TrancisioModeInicial()
    {
        novaPosicio = posInicial;
        novaRotació = rotInicial;
        nouZoom = zoomInicial;
        moure = true;
        zoom = true;
        girar = true;
    }
    public void Transicio(Vector3 pos, bool potmoure, bool potzoom, bool potgirar, bool mantenirRotacio, bool mantenirZoom)
    {
        novaPosicio = pos;
        if (!mantenirRotacio) novaRotació = rotInicial;
        if (!mantenirZoom) nouZoom = zoomInicial + Vector3.Normalize(nouZoom) * zoomCotxe;
        moure = potmoure;
        zoom = potzoom;
        girar = potgirar;
    }
    public void TransportaCam(Vector3 pos, bool potmoure, bool potzoom, bool potgirar, bool mantenirRotacio, bool mantenirZoom)
    {
        novaPosicio = pos;
        transform.position = pos;
        if (!mantenirRotacio)
        {
            transform.rotation = rotInicial;
            novaRotació = rotInicial;
        } 
        if (!mantenirZoom)
        { 
            nouZoom = zoomInicial + Vector3.Normalize(nouZoom) * zoomCotxe;
            cam.transform.localPosition = zoomInicial + Vector3.Normalize(nouZoom) * zoomCotxe;
        }
        moure = potmoure;
        zoom = potzoom;
        girar = potgirar;
    }

    private void MouCamera()
    {
        transform.position = Vector3.Lerp(transform.position, novaPosicio, tempsMoure * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, novaRotació, Time.deltaTime * tempsMoure);
        if (Mouse.current.scroll.ReadValue().y != 0 && zoom)
        {
            Vector3 zoom = nouZoom + (Mouse.current.scroll.ReadValue().y * -0.1f) * Vector3.Normalize(nouZoom) * velocitatZoom;
            float distCentre = Vector3.Distance(zoom, this.transform.position);
            if (distCentre <= distanciaMaxZoom && distCentre >= distanciaMinZoom) nouZoom = zoom;
        }
        cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, nouZoom, Time.deltaTime * tempsMoure);
    }
}
