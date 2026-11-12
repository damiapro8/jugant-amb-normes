using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class InventariEdicio : MonoBehaviour
{
    [SerializeField] private InventariJugador inventari;
    [SerializeField] private int columnes = 4;
    [SerializeField] GameObject prefabFitxa;
    [SerializeField] private float midaAmpladaTaula = 10f;
    [SerializeField] private float midaAlçadabossa = 4f;
    [SerializeField] private int limitBossa = 8;

    private List<GameObject> fitxesInstanciades = new List<GameObject>();

    void Start()
    {
        MostraBossa();
    }

    public void MostraBossa()
    {
        float espaiX = midaAmpladaTaula / columnes;
        int filesTotals = limitBossa / columnes;
        float espaiY = midaAlçadabossa / filesTotals;
        var bossaActual = inventari.Bosses[0];

        for (int i = 0; i < limitBossa; i++)
        {
            int fila = i / columnes;
            int columna = i % columnes;

            // Calculem la posició local (podeu ajustar el signe de la Y si voleu que creixi cap a dalt o cap a baix)
            float posX = columna * espaiX;
            float posY = -fila * espaiY; // Negatiu per a que la segona fila quedi a sota

            Vector3 posicioLocal = new Vector3(posX, 0, posY);

            Transform pare = transform;
            GameObject novaFitxa = Instantiate(prefabFitxa, pare);
            novaFitxa.transform.localPosition = posicioLocal;
            FitxaEdicio componentEdicio = novaFitxa.GetComponent<FitxaEdicio>();
            componentEdicio.Inicialitza(bossaActual.fitxes[i]);
            fitxesInstanciades.Add(novaFitxa);
        }
    }
}
