using System;

namespace sanitizzazioneLPG.Persistence;

/// <summary>
/// classe usata per indicare che sono stati trovati due elementi con lo stesso id
/// </summary>
public class PersExcDupl : PersExc
{
    public PersExcDupl()
    {
    }

    public PersExcDupl(string message) : base(message)
    {
    }

    public PersExcDupl(string message, Exception inner) : base(message, inner)
    {
    }
}
