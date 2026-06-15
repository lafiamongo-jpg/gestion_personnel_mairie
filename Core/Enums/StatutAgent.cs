namespace GestionPersonnelMairie.Core.Enums;

public static class StatutAgent
{
    public const string Actif = "Actif";
    public const string EnConge = "En congé";
    public const string Suspendu = "Suspendu";
    public const string Archive = "Archivé";

    public static readonly string[] All = [Actif, EnConge, Suspendu, Archive];
}
