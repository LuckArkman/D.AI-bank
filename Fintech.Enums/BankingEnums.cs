namespace Fintech.Enums;

public enum AccountProfileType
{
    StandardIndividual, // PF Padrão
    PremiumIndividual,  // PF Premium
    Corporate,          // PJ
    SmallBusiness,      // MEI/Pequena Empresa
    Salary,             // Conta Salário
    PaymentProvider,    // Conta de Pagamento
    Institutional       // Institucional (Bancos/Corretoras)
}

public enum CardType
{
    Debit,
    Credit,
    Hybrid
}

public enum CardStatus
{
    Active,
    Inactive,
    Blocked,
    Cancelled,
    Expired
}

public enum LoanStatus
{
    Pending,
    Approved,
    Active,
    Paid,
    Overdue,
    Rejected
}
