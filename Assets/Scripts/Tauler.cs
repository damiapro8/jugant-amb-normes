using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tauler : MonoBehaviour
{
    public int maxX;
    public int maxY;
    public int maxZ;
    public int minZ;
    public celaTauler[,,] tauler;
    public float midaCella = 1f;
    public float yplaEliminar = -1f;
    public Vector3[] posicionsInicials;
    public List<Cotxe> cotxes;
    public Vector3[] posCotxes;
    public Vector3 posObjecte;
    [SerializeField] private GameObject prefabObjecte;
    [SerializeField] private GameObject objecte;
    [SerializeField] private GameObject baseObjecte;
    [SerializeField] private LayerMask mascara;
    
    // private void OnDrawGizmos()
    // {
    //     if (tauler == null) return;

    //     for (int x = 0; x < tauler.GetLength(0); x++)
    //     {
    //         for (int y = 0; y < tauler.GetLength(1); y++)
    //         {
    //             for (int z = minZ; z < tauler.GetLength(2); z++)
    //             {
    //                 Vector3 posicioMón = PosTaulerAMon((x,y,z));
    //                 Vector3 midaCapsa = Vector3.one * (midaCella * 0.9f);

    //                 if (tauler[x, y, z] != null && tauler[x, y, z].aire)
    //                 {
    //                     Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
    //                     Gizmos.DrawCube(posicioMón, midaCapsa);
    //                 }
    //                 else
    //                 {
    //                     Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
    //                     Gizmos.DrawWireCube(posicioMón, midaCapsa);
    //                 }
    //             }
    //         }
    //     }
    // }

    // assigna totes les caselles del tauler una cela vuida menys aquella cela esta ocupada per qualsevol objecte
    public void inicialitza(int numJugadors)
    {
        tauler = new celaTauler[maxX,maxY,maxZ];
        for (int x = 0; x < tauler.GetLength(0); x++)
        {
            for (int y = 0; y < tauler.GetLength(1); y++)
            {
                for (int z = minZ; z < tauler.GetLength(2); z++)
                {
                    tauler[x,y,z] = new celaTauler();
                    // mirem si la posicio del tauler hi ha algo
                    Vector3 posicioMón = PosTaulerAMon((x,y,z));
                    Vector3 meitatMidaCapsa = Vector3.one * (midaCella * 0.45f);
                    Collider[] colliders = Physics.OverlapBox(posicioMón, meitatMidaCapsa, transform.rotation, mascara);
                    if (colliders.Length > 0) 
                    {   
                        FitxaPartida fitxa = Utils.EsFitxa(colliders[0].gameObject);
                        if (fitxa != null)
                        {
                            tauler[x,y,z].SetFitxa(fitxa);
                            fitxa.gameObject.GetComponent<BoxCollider>().enabled = false;
                        }
                        else
                        {
                            tauler[x,y,z].aire = true;
                        }
                    }
                    else if (posObjecte == new Vector3(x,y,z))
                    {
                        // asignar la posicio del objecte
                        objecte = Instantiate(prefabObjecte, PosTaulerAMon(posObjecte), transform.rotation);
                        FitxaPartida baseObj = Instantiate(baseObjecte, PosTaulerAMon(posObjecte), transform.rotation).GetComponent<FitxaPartida>();
                        baseObj.gameObject.GetComponent<BoxCollider>().enabled = false;
                        tauler[x,y,z].SetFitxa(baseObj); // mirar si fa falta modificar el model o tenir base com a predeterminat
                    }
                }
            }
        }
        posCotxes = new Vector3[numJugadors];
        for (int i = 0; i < numJugadors; i++)
        {
            posCotxes[i] = posicionsInicials[i];
        }
    }

    
    // rep coordenades mon i les transforma a cordenades tauler capa tauler per saber la altura
    public (int x, int y, int z) MonAPosTauler(Vector3 posMon, int CapaTauler)
    {
        int x = Mathf.FloorToInt((posMon - transform.position).x / midaCella);
        int y = Mathf.FloorToInt((posMon - transform.position).z / midaCella);
        return (x,y,CapaTauler);
    }
    // rep coordenades mon i les transforma a cordenades tauler
    public (int x, int y, int z) MonAPosTauler(Vector3 posMon)
    {
        int x = Mathf.FloorToInt((posMon - transform.position).x / midaCella);
        int y = Mathf.FloorToInt((posMon - transform.position).z / midaCella);
        int z = Mathf.FloorToInt((posMon - transform.position).y / midaCella);
        return (x,y,z);
    }
    // els cotxes tenen el punt de referencia massa baix -> sumar 1 en altura
    public (int x, int y, int z) MonAPosTaulerCotxe(Vector3 posMon)
    {
        int x = Mathf.FloorToInt((posMon - transform.position).x / midaCella);
        int y = Mathf.FloorToInt((posMon - transform.position).z / midaCella);
        int z = Mathf.FloorToInt((posMon - transform.position).y / midaCella) + 1;
        return (x,y,z);
    }
    // rep coordenades tauler i les transforma a cordenades mon
    public Vector3 PosTaulerAMon(Vector3 posTauler)
    {
        float x = posTauler.x * midaCella + transform.position.x;
        float z = posTauler.y * midaCella + transform.position.z;
        float y = posTauler.z * midaCella + transform.position.y;
        Vector3 offset = new Vector3(midaCella * 0.5f, 0, midaCella * 0.5f);
        return new Vector3(x,y,z) + offset;
    }
    public Vector3 PosTaulerAMon((int x, int y, int z) posTauler)
    {
        float x = posTauler.x * midaCella + transform.position.x;
        float z = posTauler.y * midaCella + transform.position.z;
        float y = posTauler.z * midaCella + transform.position.y;
        Vector3 offset = new Vector3(midaCella * 0.5f, 0, midaCella * 0.5f);
        return new Vector3(x,y,z) + offset;
    }
    // retorna la posicio que posa el centre del tauler en pos si s'instancia
    public Vector3 GetOffsetPosTauler(Vector3 pos)
    {
        float x = pos.x - (midaCella * maxX / 2.0f);
        float y = pos.y;
        float z = pos.z - (midaCella * maxY / 2.0f);
        return new Vector3(x,y,z);
    }

    // transforma la posicio en tauler i la guarda, retorna un bolea si esta en la posicio inicial
    public bool GuardaPosCotxe(int jugador, Vector3 pos)
    {
        (int x, int y, int z) posCotxe = MonAPosTaulerCotxe(pos);
        posCotxes[jugador].x = posCotxe.x;
        posCotxes[jugador].y = posCotxe.y;
        posCotxes[jugador].z = posCotxe.z;
        if (posicionsInicials[jugador] == posCotxes[jugador])
        {
            return true;
        }
        return false;
    }

    public void retornaCotxesAPos()
    {
        foreach (var cotxe in cotxes)
        {
            cotxe.ResetPos(PosTaulerAMon(posCotxes[cotxe.jugador]));
        }
    }
    // rep posicions de tauler decideix si es pot posar
    // utilitzaar en fitxes compostes
    // public bool potPosarFitxes(List<Vector3> posicions)
    // {
    //     foreach (var p in posicions)
    //     {
    //         int x = (int)p.x;
    //         int y = (int)p.y;
    //         int z = (int)p.z;

    //         if (x < 0 || x >= maxX || y < 0 || y >= maxY || z < minZ || z >= maxZ) return false;
    //         if (!tauler[x,y,z].EstaVuida()) return false;
    //     }
    //     return true;
    // }

    // rep posicions de tauler decideix si es pot posar
    public bool posDinsTauler((int x, int y, int z) pos)
    {
        return !(pos.x < 0 || pos.x >= maxX || pos.y < 0 || pos.y >= maxY || pos.z < minZ || pos.z >= maxZ);
    }
    public bool potPosarFitxa((int x, int y, int z) pos)
    {

        if (!posDinsTauler(pos)) return false;
        if (!tauler[pos.x,pos.y,pos.z].EstaVuida()) return false;
        return true;
    }
    // dibuixa una representacio del tauler
    // void OnDrawGizmos()
    // {
    //     if(midaCella <= 0 || maxX <= 0|| maxY <= 0|| maxZ <= 0) return;
    //     Vector3 origen = transform.position;
    //     for (int z = minZ; z < maxZ; z++)
    //     {
    //         if(z%2 == 0)
    //         {
    //             Gizmos.color = Color.red;
    //         }
    //         else
    //         {
    //             Gizmos.color = Color.yellow;
    //         }
    //         for (int i = 0; i <= maxX; i++)
    //         {
    //             Vector3 inici = origen + new Vector3(0, z, i*midaCella);
    //             Vector3 fi = origen + new Vector3(maxY * midaCella, z, i*midaCella);
    //             Gizmos.DrawLine(inici, fi);
    //         }
    //         for (int i = 0; i <= maxY; i++)
    //         {
    //             Vector3 inici = origen + new Vector3(i*midaCella, z, 0);
    //             Vector3 fi = origen + new Vector3(i*midaCella, z, maxX * midaCella);
    //             Gizmos.DrawLine(inici, fi);
    //         }
    //     }
    // }
    public bool SetFitxa(FitxaPartida fitxa, (int x, int y, int z) p)
    {
        if (tauler[p.x,p.y,p.z].EstaVuida())
        {
            tauler[p.x,p.y,p.z].SetFitxa(fitxa);
            return true;
        }
        return false;
    }
    public FitxaPartida GetFitxa((int x, int y, int z) p)
    {
        if (posDinsTauler(p) && !tauler[p.x,p.y,p.z].EstaVuida())
        {
            return tauler[p.x,p.y,p.z].fitxa;
        }
        return null;
    }
    // treu la fitxa de la cela i la retorna
    public FitxaPartida treuFitxa((int x, int y, int z) p)
    {
        if (!tauler[p.x,p.y,p.z].EstaVuida())
        {
            return tauler[p.x,p.y,p.z].TreuFitxa();
        }
        return null;
    }

    public void retornaObjecte(Transform obj)
    {
        obj.SetParent(objecte.transform);
        obj.localPosition = new Vector3(0f, 0f, 0f);
        obj.localRotation = Quaternion.identity;
    }

    public Transform getModelObjecte()
    {
        return objecte.transform.GetChild(0);
    }
    // dona una casella vuida per evitar errors
    public (int x, int y, int z) casellaVuida()
    {
        for (int x = 0; x < tauler.GetLength(0); x++)
        {
            for (int y = 0; y < tauler.GetLength(1); y++)
            {
                for (int z = minZ; z < tauler.GetLength(2); z++)
                {
                    if (tauler[x,y,z].EstaVuida()) return (x,y,z);
                }
            }
        }
        return (0,0,0);
    }

    public void congelaCotxes(bool congela)
    {
        foreach (var cotxe in cotxes)
        {
            cotxe.Congela(congela);
        }
    }
    public void centraCotxes()
    {
        congelaCotxes(false); //ens assegurem de no modificar el rigidbody en kinematic
        foreach (Cotxe cotxe in cotxes)
        {
            Vector3 posCotxe = PosTaulerAMon(Utils.VecATupla(posCotxes[cotxe.jugador]));
            Vector3 posNova = Utils.GetPosSeguentCotxe(posCotxe, this, cotxe.offsetCotxe);
            cotxe.ResetPos(posNova); 
            //Debug.Log($"postauler: {posCotxes[cotxe.jugador]}, posCotxe: {posCotxe}, amb raycast: {posNova}");
        }
        congelaCotxes(true);
    }
}

public class celaTauler
{
    public FitxaPartida fitxa;
    public bool aire = false;
    // cambia la fitxa de la cela
    public void SetFitxa(FitxaPartida fitxaPartida)
    {
        this.fitxa = fitxaPartida;
    }
    // treu la fitxa i la retorna
    public FitxaPartida TreuFitxa()
    {
        FitxaPartida f = fitxa;
        fitxa = null;
        return f;
    }
    // comprova si la cela esta ocupada
    public bool EstaVuida()
    {
        return fitxa == null && !aire;
    }
}