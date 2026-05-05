using System;
using System.Collections.Generic;

namespace Ideogram.Client.Constants;

public static class IdeogramStyleTypes
{
    public const string Auto = "AUTO";
    public const string General = "GENERAL";
    public const string Realistic = "REALISTIC";
    public const string Design = "DESIGN";
    public const string Fiction = "FICTION";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Auto,
        General,
        Realistic,
        Design,
        Fiction
    };
}
