using UnityEngine;
using System.Collections.Generic;
public class ParsejadorSintaxi
{
    public LlistaEvents configuracioTokens;
    [SerializeField] private List<Efecte> llistaAccions;
    private Gramatica gramatica;
    void Start()
    {
        
    }
    public void Inicialitza(LlistaEvents events)
    {
        configuracioTokens = events;

        if (events == null)
        {
            return;
        }
        // Passem la referència de l'ScriptableObject al motor de regles
        TerminalRule.ConfiguracioDades = configuracioTokens;

        gramatica = new Gramatica();
        gramatica.DefinirGramatica();
    }
    public DadesEfecteFitxa ParsejarSintaxi(string textAAvaluar)
    {
        DadesEfecteFitxa dadesFinals = null;

        TerminalRule.InicialitzarSessioParseig();

        List<Token> tokens = Tokenitzar(textAAvaluar);

        ParseResult resultat = gramatica.ReglaFinalDeLaCarta.Parse(tokens, 0);

        if (resultat.Success && resultat.NextIndex == tokens.Count)
        {
            dadesFinals = ConstruirDadesEfecte(tokens);
        }
        return dadesFinals;
    }

    private DadesEfecteFitxa ConstruirDadesEfecte(List<Token> tokens)
    {
        DadesEfecteFitxa dades = new DadesEfecteFitxa();
        dades.accio = new AccioData();

        bool jaHemTrobatLAccio = false;

        for (int i = 0; i < tokens.Count; i++)
        {
            Token t = tokens[i];

            // Mirem si la TerminalRule va validar amb èxit un "Efecte" d'ScriptableObject en aquesta posició
            if (TerminalRule.EfectesParsejatsPerIndex.TryGetValue(i, out Efecte efecteTrobat))
            {
                if (TerminalRule.TipusParsejatsPerIndex.TryGetValue(i, out TipusToken tipusReal))
                {
                    switch (tipusReal)
                    {
                        case TipusToken.Iniciador:
                            dades.iniciador = efecteTrobat.nomEfecte;
                            break;

                        case TipusToken.Accio:
                            dades.accio.accio = efecteTrobat.nomEfecte;
                            jaHemTrobatLAccio = true;
                            break;

                        case TipusToken.Actuador:
                            if (jaHemTrobatLAccio)
                            {
                                dades.accio.actuador = efecteTrobat.nomEfecte;
                            }
                            break;

                        case TipusToken.Selector:
                            dades.accio.selector = efecteTrobat.nomEfecte;
                            break;
                    }
                }
            }
        }
        return dades;
    }

    public static List<Token> Tokenitzar(string input)
    {
        List<Token> llistaTokens = new List<Token>();

        string textSanejat = input
            .Replace("(", " ( ")
            .Replace(")", " ) ")
            .Replace(".", " . ")
            .Replace(",", " , ");

        string[] paraules = textSanejat.Split(new char[] { ' ', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

        foreach (string paraula in paraules)
        {
            if (paraula == "(" || paraula == ")" || paraula == "." || paraula == ",")
            {
                llistaTokens.Add(new Token(TipusToken.Simbol, paraula));
            }
            else
            {
                // Tot el que no sigui un símbol es cataloga com a Identificador genèric
                llistaTokens.Add(new Token(TipusToken.Identificador, paraula));
            }
        }

        return llistaTokens;
    }

    public List<string> ObtenirSuggerimentsContextuals(string textActual, out string prefixFiltre)
    {
        prefixFiltre = "";
        TerminalRule.InicialitzarSessioParseig();
        TerminalRule.SuggerimentsAcumulats.Clear();

        List<Token> tokens = Tokenitzar(textActual);
        
        bool acabaEnEspai = textActual.Length > 0 && char.IsWhiteSpace(textActual[textActual.Length - 1]);
        
        int indexObjectiu = 0;
        if (tokens.Count == 0)
        {
            indexObjectiu = 0;
        }
        else if (acabaEnEspai)
        {
            indexObjectiu = tokens.Count;
        }
        else
        {
            indexObjectiu = tokens.Count - 1;
            prefixFiltre = tokens[tokens.Count - 1].text;
        }

        TerminalRule.IndexSuggeriment = indexObjectiu;

        if (gramatica != null && gramatica.ReglaFinalDeLaCarta != null)
        {
            gramatica.ReglaFinalDeLaCarta.Parse(tokens, 0);
        }

        List<string> filtrats = new List<string>();
        foreach (string sug in TerminalRule.SuggerimentsAcumulats)
        {
            if (string.IsNullOrEmpty(prefixFiltre) || sug.StartsWith(prefixFiltre, System.StringComparison.OrdinalIgnoreCase))
            {
                filtrats.Add(sug);
            }
        }

        return filtrats;
    }
}
