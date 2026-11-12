using UnityEngine;
using System.Collections;
using System.Collections.Generic;
enum Model
{
    Basica,
    Cub
}
public class FitxaPartida : MonoBehaviour
{
    [SerializeField] private Fitxa dadesFitxa;
    [SerializeField] private DadesGuardarFitxa dadesEfecte;
    public int comptador;
    public int propietari;
    [SerializeField] private GameObject cubVisualitzarPrefab;
    private GameObject cubVisualitzar;
    private Accions accions;
    void Start()
    {
        InstanciaObjecte();
    }
    public void Setup(Fitxa novesDades, int numjugador, Accions acs)
    {
        dadesFitxa = novesDades;
        comptador = 0;
        propietari = numjugador;
        accions = acs;
        InstanciaObjecte();
        dadesEfecte = Utils.CarregaDadesFitxa(dadesFitxa.ID);
    }
    public void Setup(FitxaPartida fitxa, Accions acs)
    {
        dadesFitxa = fitxa.dadesFitxa;
        comptador = 0;
        propietari = fitxa.propietari;
        accions = acs;
        InstanciaObjecte();
        dadesEfecte = Utils.CarregaDadesFitxa(dadesFitxa.ID);
    }
    public void setJugador(int jugador)
    {
        propietari = jugador;
    }
    public int getJugador(int jugador)
    {
        return propietari;
    }
    // borra el model antic i instancia el nou, es posible que la borra recursiva dongui warnings en unity
    public void InstanciaObjecte()
    {
        while (transform.childCount > 0)
        {
            Transform child = transform.GetChild(0);
            child.SetParent(null);
            Destroy(child.gameObject);
        }
        Instantiate(dadesFitxa.model, transform);
        cubVisualitzar = Instantiate(cubVisualitzarPrefab, transform);
        cubVisualitzar.SetActive(false);
    }
    public void MostraCub(bool mostra)
    {
        cubVisualitzar.SetActive(mostra);
        this.GetComponent<BoxCollider>().enabled = mostra;
    }
    
    public void SetColisio(bool actiu)
    {
        if (transform.GetChild(0).GetComponent<BoxCollider>() == null) return;
        transform.GetChild(0).GetComponent<BoxCollider>().enabled = actiu;
    }

    public void ExecutaEvent(Contexte contexte)
    {
        if (dadesEfecte != null && dadesEfecte.Efecte != null)
        {
            accions.Executa(dadesEfecte.Efecte.accio, contexte);
        }
    }
    public string GetIniciador()
    {
        if (dadesEfecte != null && dadesEfecte.Efecte != null )
        {
            return dadesEfecte.Efecte.iniciador;
        }
        return "";
    }

    public void SetAccions(Accions acs)
    {
        accions = acs;
    }
}
