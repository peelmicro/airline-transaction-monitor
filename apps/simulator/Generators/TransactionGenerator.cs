using Bogus;

namespace Simulator.Generators;

/// <summary>
/// Generates realistic airline transactions using Bogus.
/// Produces weighted distributions for error rates, realistic PANs (masked),
/// sensible amounts, and airline-appropriate currencies.
/// </summary>
public class TransactionGenerator
{
    private static readonly Dictionary<string, (string Currency, string[] Airports)> AirlineConfig = new()
    {
        ["Ryanair"] = ("EUR", ["DUB", "STN", "MAD", "BGY", "CIA"]),
        ["Iberia"] = ("EUR", ["MAD", "BCN", "LHR", "JFK", "MIA"]),
        ["BritishAirways"] = ("GBP", ["LHR", "LGW", "JFK", "CDG", "FRA"]),
        ["EasyJet"] = ("GBP", ["LGW", "LTN", "CDG", "AMS", "BCN"]),
        ["AmericanAirlines"] = ("USD", ["DFW", "MIA", "JFK", "LAX", "ORD"]),
        ["DeltaAirLines"] = ("USD", ["ATL", "JFK", "LAX", "MSP", "DTW"])
    };

    private static readonly string[] AcquirerCodes = ["ElavonUS", "Worldpay", "ElavonEU", "Adyen", "Barclays", "Santander"];
    private static readonly string[] CardBrandCodes = ["Visa", "Mastercard", "Amex", "UnionPay", "JCB"];
    private static readonly string[] Statuses = ["authorized", "captured", "declined", "refunded", "failed"];
    private static readonly string[] ErrorStatuses = ["declined", "failed"];
    private static readonly string[] SuccessStatuses = ["authorized", "captured"];

    public List<GeneratedTransaction> Generate(string airlineCode, int count, double errorRate)
    {
        var faker = new Faker();
        var config = AirlineConfig.GetValueOrDefault(airlineCode, ("EUR", ["LHR", "CDG", "FRA"]));
        var transactions = new List<GeneratedTransaction>(count);

        for (var i = 0; i < count; i++)
        {
            var isError = faker.Random.Double() < errorRate;
            var status = isError
                ? faker.PickRandom(ErrorStatuses)
                : faker.PickRandom(SuccessStatuses);

            var pan = faker.Finance.CreditCardNumber();
            var maskedCard = $"****-****-****-{pan[^4..]}";

            var airports = config.Airports;
            var origin = faker.PickRandom(airports);
            var destination = faker.PickRandom(airports.Where(a => a != origin).ToArray());

            transactions.Add(new GeneratedTransaction
            {
                AirlineCode = airlineCode,
                MaskedCard = maskedCard,
                CardBrandCode = faker.PickRandom(CardBrandCodes),
                Amount = faker.Random.Int(500, 150000), // 5.00 to 1500.00 in minor units
                CurrencyCode = config.Currency,
                AcquirerCode = faker.PickRandom(AcquirerCodes),
                Status = status,
                TransactionDate = DateTime.UtcNow.AddSeconds(-faker.Random.Int(0, 60)),
                FlightNumber = $"{airlineCode[..2].ToUpper()}{faker.Random.Int(100, 9999)}",
                OriginAirport = origin,
                DestinationAirport = destination,
                PassengerReference = $"PNR-{faker.Random.AlphaNumeric(6).ToUpper()}"
            });
        }

        return transactions;
    }
}

public class GeneratedTransaction
{
    public string AirlineCode { get; set; } = string.Empty;
    public string MaskedCard { get; set; } = string.Empty;
    public string CardBrandCode { get; set; } = string.Empty;
    public int Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string AcquirerCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public string OriginAirport { get; set; } = string.Empty;
    public string DestinationAirport { get; set; } = string.Empty;
    public string PassengerReference { get; set; } = string.Empty;
}
