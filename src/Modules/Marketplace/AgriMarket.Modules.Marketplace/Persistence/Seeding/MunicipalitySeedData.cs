using AgriMarket.Modules.Marketplace.Entities;
using static AgriMarket.Modules.Marketplace.Persistence.Seeding.CountySeedData;

namespace AgriMarket.Modules.Marketplace.Persistence.Seeding;

internal static class MunicipalitySeedData
{
    public static Municipality[] GetAll() =>
    [
        // Harju maakond (16)
        new() { Id = new("b0000000-0000-0000-0000-000000000001"), Name = "Tallinn", EhakCode = "0784", CountyId = HarjuId },
        new() { Id = new("b0000000-0000-0000-0000-000000000002"), Name = "Anija vald", EhakCode = "0141", CountyId = HarjuId },
        new() { Id = new("b0000000-0000-0000-0000-000000000003"), Name = "Harku vald", EhakCode = "0198", CountyId = HarjuId },
        new() { Id = new("b0000000-0000-0000-0000-000000000004"), Name = "Jõelähtme vald", EhakCode = "0245", CountyId = HarjuId },
        new() { Id = new("b0000000-0000-0000-0000-000000000005"), Name = "Keila linn", EhakCode = "0296", CountyId = HarjuId },
        new() { Id = new("b0000000-0000-0000-0000-000000000006"), Name = "Kiili vald", EhakCode = "0303", CountyId = HarjuId },
        new() { Id = new("b0000000-0000-0000-0000-000000000007"), Name = "Kose vald", EhakCode = "0338", CountyId = HarjuId },
        new() { Id = new("b0000000-0000-0000-0000-000000000008"), Name = "Kuusalu vald", EhakCode = "0353", CountyId = HarjuId },
        new() { Id = new("b0000000-0000-0000-0000-000000000009"), Name = "Loksa linn", EhakCode = "0424", CountyId = HarjuId },
        new() { Id = new("b0000000-0000-0000-0000-00000000000a"), Name = "Lääne-Harju vald", EhakCode = "0431", CountyId = HarjuId },
        new() { Id = new("b0000000-0000-0000-0000-00000000000b"), Name = "Maardu linn", EhakCode = "0446", CountyId = HarjuId },
        new() { Id = new("b0000000-0000-0000-0000-00000000000c"), Name = "Raasiku vald", EhakCode = "0651", CountyId = HarjuId },
        new() { Id = new("b0000000-0000-0000-0000-00000000000d"), Name = "Rae vald", EhakCode = "0653", CountyId = HarjuId },
        new() { Id = new("b0000000-0000-0000-0000-00000000000e"), Name = "Saku vald", EhakCode = "0718", CountyId = HarjuId },
        new() { Id = new("b0000000-0000-0000-0000-00000000000f"), Name = "Saue vald", EhakCode = "0726", CountyId = HarjuId },
        new() { Id = new("b0000000-0000-0000-0000-000000000010"), Name = "Viimsi vald", EhakCode = "0890", CountyId = HarjuId },

        // Hiiu maakond (1)
        new() { Id = new("b0000000-0000-0000-0000-000000000011"), Name = "Hiiumaa vald", EhakCode = "0205", CountyId = HiiuId },

        // Ida-Viru maakond (8)
        new() { Id = new("b0000000-0000-0000-0000-000000000012"), Name = "Alutaguse vald", EhakCode = "0130", CountyId = IdaViruId },
        new() { Id = new("b0000000-0000-0000-0000-000000000013"), Name = "Jõhvi vald", EhakCode = "0251", CountyId = IdaViruId },
        new() { Id = new("b0000000-0000-0000-0000-000000000014"), Name = "Kohtla-Järve linn", EhakCode = "0321", CountyId = IdaViruId },
        new() { Id = new("b0000000-0000-0000-0000-000000000015"), Name = "Lüganuse vald", EhakCode = "0442", CountyId = IdaViruId },
        new() { Id = new("b0000000-0000-0000-0000-000000000016"), Name = "Narva linn", EhakCode = "0511", CountyId = IdaViruId },
        new() { Id = new("b0000000-0000-0000-0000-000000000017"), Name = "Narva-Jõesuu linn", EhakCode = "0514", CountyId = IdaViruId },
        new() { Id = new("b0000000-0000-0000-0000-000000000018"), Name = "Sillamäe linn", EhakCode = "0735", CountyId = IdaViruId },
        new() { Id = new("b0000000-0000-0000-0000-000000000019"), Name = "Toila vald", EhakCode = "0803", CountyId = IdaViruId },

        // Jõgeva maakond (3)
        new() { Id = new("b0000000-0000-0000-0000-00000000001a"), Name = "Jõgeva vald", EhakCode = "0247", CountyId = JogevaId },
        new() { Id = new("b0000000-0000-0000-0000-00000000001b"), Name = "Mustvee vald", EhakCode = "0486", CountyId = JogevaId },
        new() { Id = new("b0000000-0000-0000-0000-00000000001c"), Name = "Põltsamaa vald", EhakCode = "0618", CountyId = JogevaId },

        // Järva maakond (3)
        new() { Id = new("b0000000-0000-0000-0000-00000000001d"), Name = "Järva vald", EhakCode = "0255", CountyId = JarvaId },
        new() { Id = new("b0000000-0000-0000-0000-00000000001e"), Name = "Paide linn", EhakCode = "0567", CountyId = JarvaId },
        new() { Id = new("b0000000-0000-0000-0000-00000000001f"), Name = "Türi vald", EhakCode = "0834", CountyId = JarvaId },

        // Lääne maakond (2)
        new() { Id = new("b0000000-0000-0000-0000-000000000020"), Name = "Haapsalu linn", EhakCode = "0184", CountyId = LaaneId },
        new() { Id = new("b0000000-0000-0000-0000-000000000021"), Name = "Lääne-Nigula vald", EhakCode = "0441", CountyId = LaaneId },

        // Lääne-Viru maakond (8)
        new() { Id = new("b0000000-0000-0000-0000-000000000022"), Name = "Haljala vald", EhakCode = "0191", CountyId = LaaneViruId },
        new() { Id = new("b0000000-0000-0000-0000-000000000023"), Name = "Kadrina vald", EhakCode = "0272", CountyId = LaaneViruId },
        new() { Id = new("b0000000-0000-0000-0000-000000000024"), Name = "Rakvere linn", EhakCode = "0663", CountyId = LaaneViruId },
        new() { Id = new("b0000000-0000-0000-0000-000000000025"), Name = "Rakvere vald", EhakCode = "0661", CountyId = LaaneViruId },
        new() { Id = new("b0000000-0000-0000-0000-000000000026"), Name = "Tapa vald", EhakCode = "0792", CountyId = LaaneViruId },
        new() { Id = new("b0000000-0000-0000-0000-000000000027"), Name = "Vinni vald", EhakCode = "0897", CountyId = LaaneViruId },
        new() { Id = new("b0000000-0000-0000-0000-000000000028"), Name = "Viru-Nigula vald", EhakCode = "0903", CountyId = LaaneViruId },
        new() { Id = new("b0000000-0000-0000-0000-000000000029"), Name = "Väike-Maarja vald", EhakCode = "0928", CountyId = LaaneViruId },

        // Põlva maakond (3)
        new() { Id = new("b0000000-0000-0000-0000-00000000002a"), Name = "Kanepi vald", EhakCode = "0284", CountyId = PolvaId },
        new() { Id = new("b0000000-0000-0000-0000-00000000002b"), Name = "Põlva vald", EhakCode = "0622", CountyId = PolvaId },
        new() { Id = new("b0000000-0000-0000-0000-00000000002c"), Name = "Räpina vald", EhakCode = "0708", CountyId = PolvaId },

        // Pärnu maakond (7)
        new() { Id = new("b0000000-0000-0000-0000-00000000002d"), Name = "Häädemeeste vald", EhakCode = "0214", CountyId = ParnuId },
        new() { Id = new("b0000000-0000-0000-0000-00000000002e"), Name = "Kihnu vald", EhakCode = "0305", CountyId = ParnuId },
        new() { Id = new("b0000000-0000-0000-0000-00000000002f"), Name = "Lääneranna vald", EhakCode = "0430", CountyId = ParnuId },
        new() { Id = new("b0000000-0000-0000-0000-000000000030"), Name = "Põhja-Pärnumaa vald", EhakCode = "0638", CountyId = ParnuId },
        new() { Id = new("b0000000-0000-0000-0000-000000000031"), Name = "Pärnu linn", EhakCode = "0624", CountyId = ParnuId },
        new() { Id = new("b0000000-0000-0000-0000-000000000032"), Name = "Saarde vald", EhakCode = "0712", CountyId = ParnuId },
        new() { Id = new("b0000000-0000-0000-0000-000000000033"), Name = "Tori vald", EhakCode = "0809", CountyId = ParnuId },

        // Rapla maakond (4)
        new() { Id = new("b0000000-0000-0000-0000-000000000034"), Name = "Kehtna vald", EhakCode = "0293", CountyId = RaplaId },
        new() { Id = new("b0000000-0000-0000-0000-000000000035"), Name = "Kohila vald", EhakCode = "0317", CountyId = RaplaId },
        new() { Id = new("b0000000-0000-0000-0000-000000000036"), Name = "Märjamaa vald", EhakCode = "0503", CountyId = RaplaId },
        new() { Id = new("b0000000-0000-0000-0000-000000000037"), Name = "Rapla vald", EhakCode = "0669", CountyId = RaplaId },

        // Saare maakond (2)
        new() { Id = new("b0000000-0000-0000-0000-000000000038"), Name = "Muhu vald", EhakCode = "0478", CountyId = SaareId },
        new() { Id = new("b0000000-0000-0000-0000-000000000039"), Name = "Saaremaa vald", EhakCode = "0714", CountyId = SaareId },

        // Tartu maakond (8)
        new() { Id = new("b0000000-0000-0000-0000-00000000003a"), Name = "Elva vald", EhakCode = "0171", CountyId = TartuId },
        new() { Id = new("b0000000-0000-0000-0000-00000000003b"), Name = "Kambja vald", EhakCode = "0283", CountyId = TartuId },
        new() { Id = new("b0000000-0000-0000-0000-00000000003c"), Name = "Kastre vald", EhakCode = "0291", CountyId = TartuId },
        new() { Id = new("b0000000-0000-0000-0000-00000000003d"), Name = "Luunja vald", EhakCode = "0432", CountyId = TartuId },
        new() { Id = new("b0000000-0000-0000-0000-00000000003e"), Name = "Nõo vald", EhakCode = "0528", CountyId = TartuId },
        new() { Id = new("b0000000-0000-0000-0000-00000000003f"), Name = "Peipsiääre vald", EhakCode = "0586", CountyId = TartuId },
        new() { Id = new("b0000000-0000-0000-0000-000000000040"), Name = "Tartu linn", EhakCode = "0793", CountyId = TartuId },
        new() { Id = new("b0000000-0000-0000-0000-000000000041"), Name = "Tartu vald", EhakCode = "0796", CountyId = TartuId },

        // Valga maakond (3)
        new() { Id = new("b0000000-0000-0000-0000-000000000042"), Name = "Otepää vald", EhakCode = "0557", CountyId = ValgaId },
        new() { Id = new("b0000000-0000-0000-0000-000000000043"), Name = "Tõrva vald", EhakCode = "0824", CountyId = ValgaId },
        new() { Id = new("b0000000-0000-0000-0000-000000000044"), Name = "Valga vald", EhakCode = "0855", CountyId = ValgaId },

        // Viljandi maakond (4)
        new() { Id = new("b0000000-0000-0000-0000-000000000045"), Name = "Mulgi vald", EhakCode = "0480", CountyId = ViljandiId },
        new() { Id = new("b0000000-0000-0000-0000-000000000046"), Name = "Põhja-Sakala vald", EhakCode = "0615", CountyId = ViljandiId },
        new() { Id = new("b0000000-0000-0000-0000-000000000047"), Name = "Viljandi linn", EhakCode = "0899", CountyId = ViljandiId },
        new() { Id = new("b0000000-0000-0000-0000-000000000048"), Name = "Viljandi vald", EhakCode = "0901", CountyId = ViljandiId },

        // Võru maakond (5)
        new() { Id = new("b0000000-0000-0000-0000-000000000049"), Name = "Antsla vald", EhakCode = "0142", CountyId = VoruId },
        new() { Id = new("b0000000-0000-0000-0000-00000000004a"), Name = "Rõuge vald", EhakCode = "0698", CountyId = VoruId },
        new() { Id = new("b0000000-0000-0000-0000-00000000004b"), Name = "Setomaa vald", EhakCode = "0732", CountyId = VoruId },
        new() { Id = new("b0000000-0000-0000-0000-00000000004c"), Name = "Võru linn", EhakCode = "0919", CountyId = VoruId },
        new() { Id = new("b0000000-0000-0000-0000-00000000004d"), Name = "Võru vald", EhakCode = "0917", CountyId = VoruId },
    ];
}
