using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class SubscripcioEvent
{
    public Dictionary<string, List<FitxaPartida>> subscripcions = new Dictionary<string, List<FitxaPartida>>();


    public void Subscriu(string iniciador, FitxaPartida fitxa)
    {
        Debug.Log($"S'ha subscrit una fitxa {iniciador}");
        if (!subscripcions.ContainsKey(iniciador))
        {
            subscripcions[iniciador] = new List<FitxaPartida>();
        }

        // 2. Afegim la fitxa a la llista (evitant duplicats)
        if (!subscripcions[iniciador].Contains(fitxa))
        {
            subscripcions[iniciador].Add(fitxa);
        }
    }

    public void Executa(string iniciador, Contexte contexte)
    {
        if (subscripcions.ContainsKey(iniciador))
        {
            foreach (FitxaPartida fitxa in subscripcions[iniciador].ToList())
            {
                contexte.fitxa = fitxa;
                fitxa.ExecutaEvent(contexte);
            }
        }
    }
    public void ExecutaUnic(string iniciador, FitxaPartida fitxa, Contexte contexte)
    {
        if (subscripcions.ContainsKey(iniciador))
        {
            if (fitxa != null)
            {
                contexte.fitxa = fitxa;
                fitxa.ExecutaEvent(contexte);
            }
        }
    }

    public void Desubscriu(string iniciador, FitxaPartida fitxa)
    {
        if (subscripcions.ContainsKey(iniciador))
        {
            subscripcions[iniciador].Remove(fitxa);
        }

        if (subscripcions[iniciador].Count == 0)
        {
            subscripcions.Remove(iniciador);
        }
    }

    public void DesubscriuTot(FitxaPartida fitxa)
{
    var claus = subscripcions.Keys.ToList();

    foreach (var clau in claus)
    {
        subscripcions[clau].RemoveAll(f => f == fitxa);

        if (subscripcions[clau].Count == 0)
        {
            subscripcions.Remove(clau);
        }
    }
}
}
