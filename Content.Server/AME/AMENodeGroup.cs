using System.Linq;
using Content.Server.Administration.Systems;
using Content.Server.AME.Components;
using Content.Server.Chat.Managers;
using Content.Server.Explosion.EntitySystems;
using Content.Server.NodeContainer.NodeGroups;
using Content.Server.NodeContainer.Nodes;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;

namespace Content.Server.AME
{
    /// <summary>
    /// Node group class for handling the Antimatter Engine's console and parts.
    /// </summary>
    [NodeGroup(NodeGroupID.AMEngine)]
    public sealed class AMENodeGroup : BaseNodeGroup
    {
        /// <summary>
        /// The AME controller which is currently in control of this node group.
        /// This could be tracked a few different ways, but this is most convenient,
        /// since any part connected to the node group can easily find the master.
        /// </summary>
        [ViewVariables]
        private AMEControllerComponent? _masterController;

        [Dependency] private readonly IChatManager _chat = default!;
        [Dependency] private readonly IEntityManager _entMan = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IRobustRandom _random = default!;

        public AMEControllerComponent? MasterController => _masterController;

        private readonly List<AMEShieldComponent> _cores = new();

        public int CoreCount => _cores.Count;

        public override void LoadNodes(List<Node> groupNodes)
        {
            base.LoadNodes(groupNodes);

            MapGridComponent? grid = null;

            foreach (var node in groupNodes)
            {
                var nodeOwner = node.Owner;
                if (_entMan.TryGetComponent(nodeOwner, out AMEShieldComponent? shield))
                {
                    var xform = _entMan.GetComponent<TransformComponent>(nodeOwner);
                    if (xform.GridUid != grid?.Owner && !_mapManager.TryGetGrid(xform.GridUid, out grid))
                        continue;

                    if (grid == null)
                        continue;

                    var nodeNeighbors = grid.GetCellsInSquareArea(xform.Coordinates, 1)
                        .Where(entity => entity != nodeOwner && _entMan.HasComponent<AMEShieldComponent>(entity));

                    if (nodeNeighbors.Count() >= 8)
                    {
                        _cores.Add(shield);
                        shield.SetCore();
                        // Core visuals will be updated later.
                    }
                    else
                    {
                        shield.UnsetCore();
                    }
                }
            }

            // Separate to ensure core count is correctly updated.
            foreach (var node in groupNodes)
            {
                var nodeOwner = node.Owner;
                if (_entMan.TryGetComponent(nodeOwner, out AMEControllerComponent? controller))
                {
                    if (_masterController == null)
                    {
                        // Has to be the first one, as otherwise IsMasterController will return true on them all for this first update.
                        _masterController = controller;
                    }
                    controller.OnAMENodeGroupUpdate();
                }
            }

            UpdateCoreVisuals();
        }

        public void UpdateCoreVisuals()
        {
            var injectionAmount = 0;
            var injecting = false;

            if (_masterController != null)
            {
                injectionAmount = _masterController.InjectionAmount;
                injecting = _masterController.Injecting;
            }

            var injectionStrength = CoreCount > 0 ? injectionAmount / CoreCount : 0;

            foreach (AMEShieldComponent core in _cores)
            {
                core.UpdateCoreVisuals(injectionStrength, injecting);
            }
        }

        public float InjectFuel(int fuel, out bool overloading)
        {
            overloading = false;
            if(fuel > 0 && CoreCount > 0)
            {
                var safeFuelLimit = CoreCount * 2;
                if (fuel > safeFuelLimit)
                {
                    // The AME is being overloaded.
                    // Note about these maths: I would assume the general idea here is to make larger engines less safe to overload.
                    // In other words, yes, those are supposed to be CoreCount, not safeFuelLimit.
                    var instability = 0;
                    var overloadVsSizeResult = fuel - CoreCount;

                    // fuel > safeFuelLimit: Slow damage. Can safely run at this level for burst periods if the engine is small and someone is keeping an eye on it.
                    if (_random.Prob(0.5f))
                        instability = 1;
                    // overloadVsSizeResult > 5:
                    if (overloadVsSizeResult > 5)
                        instability = 5;
                    // overloadVsSizeResult > 10: This will explode in at most 5 injections.
                    if (overloadVsSizeResult > 10)
                        instability = 20;

                    // Apply calculated instability
                    if (instability != 0)
                    {
                        overloading = true;
                        var integrityCheck = 100;
                        foreach(AMEShieldComponent core in _cores)
                        {
                            var oldIntegrity = core.CoreIntegrity;
                            core.CoreIntegrity -= instability;

                            if (oldIntegrity > 95
                                && core.CoreIntegrity <= 95
                                && core.CoreIntegrity < integrityCheck)
                                integrityCheck = core.CoreIntegrity;
                        }

                        // Admin alert
                        if (integrityCheck != 100 && _masterController != null)
                            _chat.SendAdminAlert($"AME overloading: {_entMan.ToPrettyString(_masterController.Owner)}");
                    }
                }
                // Note the float conversions. The maths will completely fail if not done using floats.
                // Oh, and don't ever stuff the result of this in an int. Seriously.
                return (((float) fuel) / CoreCount) * fuel * 2000;
            }
            return 0;
        }

        public int GetTotalStability()
        {
            if(CoreCount < 1) { return 100; }
            var stability = 0;

            foreach(AMEShieldComponent core in _cores)
            {
                stability += core.CoreIntegrity;
            }

            stability = stability / CoreCount;

            return stability;
        }

        public void ExplodeCores()
        {
            if(_cores.Count < 1 || MasterController == null) { return; }

            float radius = 0;

            /*
             * todo: add an exact to the shielding and make this find the core closest to the controller
             * so they chain explode, after helpers have been added to make it not cancer
            */

            foreach (AMEShieldComponent core in _cores)
            {
                radius += MasterController.InjectionAmount;
            }

            radius *= 2;
            radius = Math.Min(radius, 8);
            EntitySystem.Get<ExplosionSystem>().TriggerExplosive(MasterController.Owner, radius: radius, delete: false);
        }
    }
}
