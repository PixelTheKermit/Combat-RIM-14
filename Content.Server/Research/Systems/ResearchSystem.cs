using System.Linq;
using Content.Shared.Research.Components;
using Content.Shared.Research.Systems;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.Research.Systems
{
    [UsedImplicitly]
    public sealed partial class ResearchSystem : SharedResearchSystem
    {
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            InitializeClient();
            InitializeConsole();
            InitializeSource();
            InitializeServer();
        }

        /// <summary>
        /// Gets a server based on it's unique numeric id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ResearchServerComponent? GetServerById(int id)
        {
            foreach (var server in EntityQuery<ResearchServerComponent>())
            {
                if (server.Id == id)
                    return server;
            }

            return null;
        }

        public string[] GetServerNames(ResearchServerComponent[]? allServers)
        {
            if (allServers == null)
                allServers = EntityQuery<ResearchServerComponent>(true).ToArray();
            var list = new string[allServers.Length];

            for (var i = 0; i < allServers.Length; i++)
            {
                list[i] = allServers[i].ServerName;
            }

            return list;
        }

        public int[] GetServerIds(ResearchServerComponent[]? allServers)
        {
            if (allServers == null)
                allServers = EntityQuery<ResearchServerComponent>(true).ToArray();
            var list = new int[allServers.Length];

            for (var i = 0; i < allServers.Length; i++)
            {
                list[i] = allServers[i].Id;
            }

            return list;
        }

        public List<ResearchServerComponent> GetServersOnGrid(EntityUid grid)
        {
            var allServers = EntityQuery<ResearchServerComponent>(true);
            var list = new List<ResearchServerComponent>();

            foreach (var server in allServers)
            {
                if (Comp<TransformComponent>(server.Owner).GridUid == grid)
                    list.Add(server);
            }

            return list;
        }

        public override void Update(float frameTime)
        {
            foreach (var server in EntityQuery<ResearchServerComponent>())
            {
                if (server.NextUpdateTime > _timing.CurTime)
                    continue;
                server.NextUpdateTime = _timing.CurTime + server.ResearchConsoleUpdateTime;

                UpdateServer(server.Owner, (int) server.ResearchConsoleUpdateTime.TotalSeconds, server);
            }
        }
    }
}
