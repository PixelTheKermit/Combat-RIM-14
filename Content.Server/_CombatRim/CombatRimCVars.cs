using Robust.Shared.Configuration;

namespace Content.Server._CombatRim;

[CVarDefs]
public sealed class CombatRimCVars
{
    /// <summary>
    /// What is the main entity used for buying and selling goods?
    /// </summary>
    public static readonly CVarDef<string> MainCurrency =
        CVarDef.Create("combatrim.economy.main_currency", "SpaceCash", CVar.SERVERONLY);

    public static readonly CVarDef<bool> DoEcoEvents =
        CVarDef.Create("combatrim.economy.events.enabled", true, CVar.SERVERONLY);

    /// <summary>
    /// What should be the name of the bank announcer?
    /// </summary>
    public static readonly CVarDef<string> BankAnnouncer =
        CVarDef.Create("combatrim.economy.events.announcer", "Bank of the Death Sector", CVar.SERVERONLY);

    /// <summary>
    /// Min interval of the economic events.
    /// </summary>
    public static readonly CVarDef<int> EcoEventMinInterval =
        CVarDef.Create("combatrim.economy.events.min_interval", 10, CVar.SERVERONLY);

    /// <summary>
    /// Max interval of the economic events.
    /// </summary>
    public static readonly CVarDef<int> EcoEventMaxInterval =
        CVarDef.Create("combatrim.economy.events.max_interval", 45, CVar.SERVERONLY);

    /// <summary>
    /// The "border" of the sector, measured by tiles and value is multiplied by 128.
    /// </summary>
    public static readonly CVarDef<int> SectorBorderDist =
        CVarDef.Create("combatrim.border.damage_dist", 32, CVar.SERVERONLY);
}
