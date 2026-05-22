using AgriMarket.Modules.Marketplace.Entities;

namespace AgriMarket.Modules.Marketplace.Persistence.Seeding;

internal static class CountySeedData
{
    public static readonly Guid HarjuId = new("a0000000-0000-0000-0000-000000000001");
    public static readonly Guid HiiuId = new("a0000000-0000-0000-0000-000000000002");
    public static readonly Guid IdaViruId = new("a0000000-0000-0000-0000-000000000003");
    public static readonly Guid JogevaId = new("a0000000-0000-0000-0000-000000000004");
    public static readonly Guid JarvaId = new("a0000000-0000-0000-0000-000000000005");
    public static readonly Guid LaaneId = new("a0000000-0000-0000-0000-000000000006");
    public static readonly Guid LaaneViruId = new("a0000000-0000-0000-0000-000000000007");
    public static readonly Guid PolvaId = new("a0000000-0000-0000-0000-000000000008");
    public static readonly Guid ParnuId = new("a0000000-0000-0000-0000-000000000009");
    public static readonly Guid RaplaId = new("a0000000-0000-0000-0000-00000000000a");
    public static readonly Guid SaareId = new("a0000000-0000-0000-0000-00000000000b");
    public static readonly Guid TartuId = new("a0000000-0000-0000-0000-00000000000c");
    public static readonly Guid ValgaId = new("a0000000-0000-0000-0000-00000000000d");
    public static readonly Guid ViljandiId = new("a0000000-0000-0000-0000-00000000000e");
    public static readonly Guid VoruId = new("a0000000-0000-0000-0000-00000000000f");

    public static County[] GetAll() =>
    [
        new() { Id = HarjuId, Name = "Harju maakond", EhakCode = "0037" },
        new() { Id = HiiuId, Name = "Hiiu maakond", EhakCode = "0039" },
        new() { Id = IdaViruId, Name = "Ida-Viru maakond", EhakCode = "0044" },
        new() { Id = JogevaId, Name = "Jõgeva maakond", EhakCode = "0049" },
        new() { Id = JarvaId, Name = "Järva maakond", EhakCode = "0051" },
        new() { Id = LaaneId, Name = "Lääne maakond", EhakCode = "0056" },
        new() { Id = LaaneViruId, Name = "Lääne-Viru maakond", EhakCode = "0059" },
        new() { Id = PolvaId, Name = "Põlva maakond", EhakCode = "0063" },
        new() { Id = ParnuId, Name = "Pärnu maakond", EhakCode = "0067" },
        new() { Id = RaplaId, Name = "Rapla maakond", EhakCode = "0070" },
        new() { Id = SaareId, Name = "Saare maakond", EhakCode = "0074" },
        new() { Id = TartuId, Name = "Tartu maakond", EhakCode = "0078" },
        new() { Id = ValgaId, Name = "Valga maakond", EhakCode = "0082" },
        new() { Id = ViljandiId, Name = "Viljandi maakond", EhakCode = "0084" },
        new() { Id = VoruId, Name = "Võru maakond", EhakCode = "0086" },
    ];
}
