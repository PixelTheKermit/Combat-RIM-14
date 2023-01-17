using System.Linq;
using Content.Server.Administration;
using Content.Server.Climbing;
using Content.Server.GameTicking;
using Content.Server.Medical.CryopodSpawn;
using Content.Server.Mind.Components;
using Content.Server.Spawners.EntitySystems;
using Content.Server.Station.Systems;
using Content.Shared.ActionBlocker;
using Content.Shared.Bed.Sleep;
using Content.Shared.Destructible;
using Content.Shared.DragDrop;
using Content.Shared.Examine;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Preferences;
using Content.Shared.StatusEffect;
using Content.Shared.Verbs;
using Robust.Server.Containers;
using Robust.Shared.Containers;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Server.Medical.Cryopod;

public sealed class CryopodSpawnSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly ClimbSystem _climb = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly StationJobsSystem _jobsSystem = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly QuickDialogSystem _quickDialog = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;

    private EntityUid? _storageMap;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CryopodSpawnComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<CryopodSpawnComponent, GetVerbsEvent<AlternativeVerb>>(AddAlternativeVerbs);
        SubscribeLocalEvent<CryopodSpawnComponent, SuicideEvent>(OnSuicide);
        SubscribeLocalEvent<CryopodSpawnComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<CryopodSpawnComponent, DestructionEventArgs>((e,c,_) => EjectBody(e, c));
        SubscribeLocalEvent<CryopodSpawnComponent, DragDropEvent>(OnDragDrop, before: new []{typeof(ClimbSystem)});
        SubscribeLocalEvent<PlayerSpawningEvent>(OnSpawning, before: new [] {typeof(SpawnPointSystem)});
    }

    private EntityUid GetStorageMap()
    {
        if (Deleted(_storageMap))
        {
            var map = _mapManager.CreateMap();
            _storageMap = _mapManager.GetMapEntityId(map);
            _mapManager.SetMapPaused(map, true);
        }

        return _storageMap.Value;
    }

    private void OnInit(EntityUid uid, CryopodSpawnComponent component, ComponentInit args)
    {
        component.BodyContainer = _container.EnsureContainer<ContainerSlot>(uid, "body_container");
    }

    private void OnSpawning(PlayerSpawningEvent args)
    {
        if (args.SpawnResult != null)
            return;

        var validPods = EntityQuery<CryopodSpawnComponent>().Where(c => c.DoSpawns && _stationSystem.GetOwningStation(c.Owner) == args.Station).ToArray();
        if (!validPods.Any())
            return;
        _random.Shuffle(validPods);

        var pod = validPods.First();
        var xform = Transform(pod.Owner);

        // l o n g expression.
        args.SpawnResult = FindExistingBody(args.HumanoidCharacterProfile, xform.Coordinates)
                           ?? _stationSpawning.SpawnPlayerMob(xform.Coordinates, args.Job, args.HumanoidCharacterProfile, args.Station);

        _audio.PlayPvs(pod.ArrivalSound, pod.Owner);
        if (IsOccupied(pod))
            EjectBody(pod.Owner, pod);
        InsertBody(args.SpawnResult.Value, pod, true);
        var duration = _random.NextFloat(pod.InitialSleepDurationRange.X, pod.InitialSleepDurationRange.Y);
        _statusEffects.TryAddStatusEffect<SleepingComponent>(args.SpawnResult.Value, "ForcedSleep", TimeSpan.FromSeconds(duration), false);
    }

    private EntityUid? FindExistingBody(HumanoidCharacterProfile? profile, EntityCoordinates coords)
    {
        if (profile is null)
            return null;

        foreach (var ent in Transform(GetStorageMap()).ChildEntities)
        {
            if (Name(ent) != profile.Name)
                continue;

            Transform(ent).Coordinates = coords;
            return ent;
        }

        return null;
    }

    private void OnDragDrop(EntityUid uid, CryopodSpawnComponent component, DragDropEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = InsertBody(args.Dragged, component, false);
    }

    private void AddAlternativeVerbs(EntityUid uid, CryopodSpawnComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        // Eject verb
        if (IsOccupied(component))
        {
            AlternativeVerb verb = new()
            {
                Act = () => EjectBody(uid, component),
                Category = VerbCategory.Eject,
                Text = Loc.GetString("medical-scanner-verb-noun-occupant")
            };
            args.Verbs.Add(verb);
        }

        // Self-insert verb
        if (!IsOccupied(component) &&
            _actionBlocker.CanMove(args.User))
        {
            AlternativeVerb verb = new()
            {
                Act = () => InsertBody(args.User, component, false),
                Category = VerbCategory.Insert,
                Text = Loc.GetString("medical-scanner-verb-enter")
            };
            args.Verbs.Add(verb);
        }
    }

    private void OnSuicide(EntityUid uid, CryopodSpawnComponent component, SuicideEvent args)
    {
        if (args.Handled)
            return;

        if (args.Victim != component.BodyContainer.ContainedEntity)
            return;

        QueueDel(args.Victim);
        _audio.PlayPvs(component.LeaveSound, uid);
        args.SetHandled(SuicideKind.Special);
    }

    private void OnExamine(EntityUid uid, CryopodSpawnComponent component, ExaminedEvent args)
    {
        var message = component.BodyContainer.ContainedEntity == null
            ? "cryopod-examine-empty"
            : "cryopod-examine-occupied";

        args.PushMarkup(Loc.GetString(message));
    }

    public bool InsertBody(EntityUid? toInsert, CryopodSpawnComponent component, bool force)
    {
        if (toInsert == null)
            return false;

        if (IsOccupied(component) && !force)
            return false;

        if (!HasComp<MobThresholdsComponent>(toInsert.Value))
            return false;

        if (TryComp<MindComponent>(toInsert, out var mind))
        {
            var session = mind.Mind?.Session;
            if (session is not null && session.Status == SessionStatus.Disconnected)
            {
                CryoStoreBody(toInsert.Value, component);
                return true; // goodbye.
            }
        }

        var success = component.BodyContainer.Insert(toInsert.Value, EntityManager);

        if (success && mind?.Mind?.Session != null)
        {
            _quickDialog.OpenDialog(mind.Mind.Session, "Enter cryogenic storage?", () =>
            {
                CryoStoreBody(toInsert.Value, component);
            });
        }

        return success;
    }

    private void CryoStoreBody(EntityUid body, CryopodSpawnComponent pod)
    {
        if (!TryComp(body, out MindComponent? mind) || mind.Mind is null)
        {
            QueueDel(body);
            return;
        }

        var jobProto = mind.Mind.CurrentJob?.Prototype;
        if (jobProto is not null)
        {
            var station = _stationSystem.GetOwningStation(pod.Owner);

            if (station is null)
                return; // Whuh?!

            _jobsSystem.TryAdjustJobSlot(station.Value, jobProto, 1, false, true);
        }

        _gameTicker.OnGhostAttempt(mind.Mind, false);
        var storage = GetStorageMap();
        var xform = Transform(body);
        xform.Coordinates = new EntityCoordinates(storage, Vector2.Zero);
    }

    public bool EjectBody(EntityUid pod, CryopodSpawnComponent component)
    {
        if (!IsOccupied(component))
            return false;

        var toEject = component.BodyContainer.ContainedEntity;
        if (toEject == null)
            return false;

        component.BodyContainer.Remove(toEject.Value);
        _climb.ForciblySetClimbing(toEject.Value, pod);

        return true;
    }

    public bool IsOccupied(CryopodSpawnComponent component)
    {
        return component.BodyContainer.ContainedEntity != null;
    }
}

