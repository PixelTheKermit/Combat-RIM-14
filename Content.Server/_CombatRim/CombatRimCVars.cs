using Robust.Shared.Configuration;

namespace Content.Server._CombatRim;

[CVarDefs]
public sealed class CombatRimCVars
{
    /// <summary>
    /// What is the main entity used for buying and selling goods?
    /// </summary>
    public static readonly CVarDef<string> MainCurrency =
        CVarDef.Create("economy.main_currency", "SpaceCash", CVar.SERVERONLY);

    /// <summary>
    /// Restocks and inflation events
    /// </summary>
    /// <returns></returns>
    public static readonly CVarDef<bool> DoEcoEvents =
        CVarDef.Create("economy.events_enabled", true, CVar.SERVERONLY);

    /// <summary>
    /// What should be the name of the bank announcer?
    /// </summary>
    public static readonly CVarDef<string> BankAnnouncer =
        CVarDef.Create("economy.events_announcer", "Sector Bank", CVar.SERVERONLY);

    /// <summary>
    /// Min interval of the economic events.
    /// </summary>
    public static readonly CVarDef<int> EcoEventMinInterval =
        CVarDef.Create("economy.events_min_interval", 10, CVar.SERVERONLY);

    /// <summary>
    /// Max interval of the economic events.
    /// </summary>
    public static readonly CVarDef<int> EcoEventMaxInterval =
        CVarDef.Create("economy.events_max_interval", 45, CVar.SERVERONLY);

    /// <summary>
    /// Min interval of the economic events.
    /// </summary>
    public static readonly CVarDef<int> NextRestockMinInterval =
        CVarDef.Create("economy.restock_min_interval", 5, CVar.SERVERONLY);

    /// <summary>
    /// Max interval of the economic events.
    /// </summary>
    public static readonly CVarDef<int> NextRestockMaxInterval =
        CVarDef.Create("economy.restock_max_interval", 15, CVar.SERVERONLY);

    /// <summary>
    /// The "border" of the sector, measured by tiles and value is multiplied by 128.
    /// </summary>
    public static readonly CVarDef<int> SectorBorderDist =
        CVarDef.Create("border.damage_dist", 32, CVar.SERVERONLY);
}
