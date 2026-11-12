using UnityEngine;
using TMPro;
public enum TipusToken
{
    Iniciador,
    Selector,
    Actuador,
    Accio,
    Condicional,
    Operacio,
    Simbol,
    Identificador
}

public class Token
{
    public TipusToken tipus;
    public string text;
    public Token(TipusToken tipus, string valor) {
        this.tipus = tipus;
        this.text = valor;
    }
}
public class ParsejadorEvent : MonoBehaviour
{
    private ParsejadorSintaxi pSintaxi;
    public LlistaEvents configuracioTokens;
    public TMP_InputField inputField;
    public string textDeProva;
    [SerializeField] private Edicio editor;
    void Start()
    {
        pSintaxi = new ParsejadorSintaxi();
        pSintaxi.Inicialitza(configuracioTokens);
    }

    public void OnBotoGuardarClick()
    {
        if (inputField == null)
        {
            Debug.LogError("<color=red><b>❌ ERROR:</b> No s'ha assignat l'InputField a l'Inspector de Unity!</color>");
            return;
        }
        string textEscrit = inputField.text;

        if (string.IsNullOrWhiteSpace(textEscrit))
        {
            editor.modificaEvent(null, ""); // eliminem el efecte escrit
        }
        DadesEfecteFitxa dades = pSintaxi.ParsejarSintaxi(textEscrit);
        if (dades != null) // s'ha parsejat amb exit
        {
            editor.modificaEvent(dades, textEscrit);
        }
    }
}
