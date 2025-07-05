using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TrProtocol.Packets;
using TShockAPI;

namespace Fplayer;

[ApiVersion(2, 1)]
public class Plugin : TerrariaPlugin
{
    public override string Name => "Fplayer";
    public override Version Version => new Version(1, 0, 0, 4);
    public override string Author => "少司命";
    public override string Description => "在你的服务器中放置假人！";

    internal static readonly DummyPlayer[] _players = new DummyPlayer[Main.maxNetPlayers];
    internal static int Port = 7777;
    
    // 新的假人列表
    internal static List<DummyPlayer> DummyPlayers = new List<DummyPlayer>();

    public Plugin(Main game) : base(game)
    {
    }

    public override void Initialize()
    {
        Config.Read();
        ServerApi.Hooks.ServerLeave.Register(this, this.OnLeave);
        On.Terraria.Netplay.OpenPort += this.Netplay_OpenPort;
        Commands.ChatCommands.Add(new Command("dummy.client.use", CommandAdapter.Adapter, "dummy"));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ServerApi.Hooks.ServerLeave.Deregister(this, this.OnLeave);
            On.Terraria.Netplay.OpenPort -= this.Netplay_OpenPort;
            Commands.ChatCommands.RemoveAll(c => c.CommandDelegate.Method?.DeclaringType?.Assembly == Assembly.GetExecutingAssembly());
        }
        base.Dispose(disposing);
    }

    private void Netplay_OpenPort(On.Terraria.Netplay.orig_OpenPort orig, int port)
    {
        orig(port);
        foreach (var dummy in Config.Instance.Dummys)
        {
            var ply = new DummyPlayer(new()
            {
                Hair = dummy.Hair,
                HairColor = dummy.HairColor,
                EyeColor = dummy.EyeColor,
                ShirtColor = dummy.ShirtColor,
                ShoeColor = dummy.ShoeColor,
                SkinColor = dummy.SkinColor,
                HairDye = dummy.HairDye,
                Name = dummy.Name,
                SkinVariant = dummy.SkinVariant,
                UnderShirtColor = dummy.UnderShirtColor,
                HideMisc = dummy.HideMisc,
            }, dummy.UUID);
            ply.GameLoop("127.0.0.1", port, TShock.Config.Settings.ServerPassword);
            if (!string.IsNullOrEmpty(dummy.Password))
            {
                ply.ChatText($"/login {dummy.Password}");
            }
            ply.On<LoadPlayer>(p => _players[p.PlayerSlot] = ply);
        }
        Port = port;
    }

    private void OnLeave(LeaveEventArgs args)
    {
        var ply = _players[args.Who];
        ply?.Close();
    }
}
