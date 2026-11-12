using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;

public static class Utils
{
    public static FitxaPartida EsFitxa(GameObject obj)
    {
        FitxaPartida fitxa = obj.transform.GetComponent<FitxaPartida>();
        if (fitxa != null)
        {
            // tenim la fitxa
            return fitxa;
        }
        Transform pareActual = obj.transform.parent;
        while (pareActual != null)
        {
            fitxa = pareActual.GetComponent<FitxaPartida>();
            if (fitxa != null)
            {
                // tenim la fitxa
                return fitxa;
            }
            pareActual = pareActual.parent;
        }
        return null;
    }
    public static FitxaEdicio EsFitxaEdicio(GameObject obj)
    {
        FitxaEdicio fitxa = obj.transform.GetComponent<FitxaEdicio>();
        if (fitxa != null)
        {
            // tenim la fitxa
            return fitxa;
        }
        Transform pareActual = obj.transform.parent;
        while (pareActual != null)
        {
            fitxa = pareActual.GetComponent<FitxaEdicio>();
            if (fitxa != null)
            {
                // tenim la fitxa
                return fitxa;
            }
            pareActual = pareActual.parent;
        }
        return null;
    }
    public static T BuscaComponentRecursiu<T>(this Transform parent) where T : Component
    {
        T component = parent.GetComponent<T>();
        if (component != null) return component;

        foreach (Transform child in parent)
        {
            T result = child.BuscaComponentRecursiu<T>();
            if (result != null) return result;
        }
        return null;
    }
    public static Transform BuscaTag(this Transform parent, string tag)
    {
        // 1. Mirem si l'objecte actual té el Tag
        if (parent.CompareTag(tag))
        {
            return parent;
        }

        // 2. Busquem en els fills recursivament
        foreach (Transform child in parent)
        {
            Transform result = child.BuscaTag(tag);
            if (result != null) return result;
        }

        // 3. Si hem recorregut tota la branca i no hi és
        return null;
    }
    // retorna la posicio del ratoli en coordenades mon relativa a un pla de y=capa
    // retrona vec0 si el raycast no dona a la capa
    public static Vector3 GetPosRatoliCapa(float capa)
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane groundplane = new(Vector3.up, Vector3.zero + new Vector3(0,capa, 0));
        if(groundplane.Raycast(ray,out float distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    } 
    // retorna la fitxa més propera (raycast) que el ratoli apunta.
    public static FitxaPartida getFitxaSeleccionada()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out hit))
        {
            return EsFitxa(hit.transform.gameObject);
        }
        return null;
    }

    public static FitxaEdicio getFitxaEdicioSeleccionada()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out hit))
        {
            return EsFitxaEdicio(hit.transform.gameObject);
        }
        return null;
    }

    public static (int x, int y, int z) VecATupla(Vector3 vec)
    {
        (int x, int y, int z) tupla;
        tupla.x = (int)vec.x;
        tupla.y = (int)vec.y;
        tupla.z = (int)vec.z;
        return tupla;
    }
    public static Vector3 TuplaAVec((int x, int y, int z) tupla)
    {
        Vector3 vec = new Vector3(tupla.x,tupla.y,tupla.z);
        return vec;
    }
    public static FitxaPartida getFitxaDeSota(Vector3 posObj)
    {
        RaycastHit hit;
        Ray ray = new Ray(posObj, Vector3.down);
        if (Physics.Raycast(ray, out hit))
        {
            return EsFitxa(hit.transform.gameObject);
        }
        return null;
    }
    public static void GuardaDadesFitxa(int id, DadesEfecteFitxa efecte, string text)
    {
        string ruta = Path.Combine(Application.persistentDataPath, $"fitxa_{id}.json");
        DadesGuardarFitxa dadesGuardar = new DadesGuardarFitxa {Efecte = efecte, TextEfecte = text};

        string json = JsonUtility.ToJson(dadesGuardar, true); 
        File.WriteAllText(ruta, json);
    }

    public static DadesGuardarFitxa CarregaDadesFitxa(int id)
    {
        string ruta = Path.Combine(Application.persistentDataPath, $"fitxa_{id}.json");

        if (File.Exists(ruta))
        {
            string json = File.ReadAllText(ruta);
            return JsonUtility.FromJson<DadesGuardarFitxa>(json);
        }
        return null;
    }
    public static Vector3 GetPosCentrada(Vector3 posicio, Tauler tauler)
    {
        float centreX = Mathf.FloorToInt(posicio.x) + tauler.midaCella / 2f;
        float centreZ = Mathf.FloorToInt(posicio.z) + tauler.midaCella / 2f;
        float centreY = Mathf.FloorToInt(posicio.y) + tauler.midaCella / 2f;
        return new (centreX, centreY, centreZ);
    }
    public static Vector3 GetPosSeguentCotxe(Vector3 posCotxe, Tauler tauler, float offsetCotxe)
    {
        int mascara = LayerMask.GetMask("Fitxa");
        int CapaCotxe = Mathf.FloorToInt(posCotxe.y + 0.15f); // ofset per evitar clipping a la casella inferior
        Vector3 posRaig = GetPosCentrada(posCotxe, tauler);
        posRaig.y += 0.6f;
        RaycastHit hit;
        Ray ray = new Ray(posRaig, Vector3.down);
        Vector3 posSeguentCotxe = new Vector3(0f,-100f,0f);
        if (Physics.Raycast(ray, out hit, 100f, mascara))
        {
            posSeguentCotxe = hit.point + new Vector3(0f,offsetCotxe + 0.005f,0f); // punt més alt de la fitxa + ofset de cotxe
        }
        return posSeguentCotxe;
    }
}
