using dnlib.DotNet;
using System;

namespace RATsConfigExtractor
{
    internal class Utils
    {
        internal static string? GetName(IField field)
        {
            return field switch
            {
                FieldDef fieldDefinition => fieldDefinition.Name,
                MemberRef memberReference => memberReference.Name,
                _ => throw new ArgumentOutOfRangeException(nameof(field))
            };
        }
    }
}