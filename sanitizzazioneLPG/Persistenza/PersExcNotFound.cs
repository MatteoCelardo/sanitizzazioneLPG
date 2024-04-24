using System;

namespace sanitizzazioneLPG.Persistenza;

/// <summary>
/// Classe usata se un elemento cercato nella persistenza non è presente
/// </summary>
public class PersExcNotFound : PersExc
{
    public PersExcNotFound()
    {
    }

    public PersExcNotFound(string message) : base(message)
    {
    }

    public PersExcNotFound(string message, Exception inner) : base(message, inner)
    {
    }
}
