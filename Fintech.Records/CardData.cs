namespace Fintech.Records;

public record CardData(
    string HolderName,
    string CardNumber,
    string ExpirationDate,
    string Cvv
);