namespace Fintech.Enums;

public enum AccountProfileType
{
    StandardIndividual, // PF Padrão
    PremiumIndividual,  // PF Premium
    Corporate,          // PJ
    SmallBusiness,      // MEI/Pequena Empresa
    Salary,             // Conta Salário
    PaymentProvider,    // Conta de Pagamento
    Institutional,       // Institucional (Bancos/Corretoras)
    Business
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

public enum InvestmentType
{
    Selection, // Seleção D.AI
    CDB,       // Certificado de Depósito Bancário
    LCI,       // Letra de Crédito Imobiliário
    LCA,       // Letra de Crédito do Agronegócio
    Stock,     // Ações
    Crypto     // Criptomoedas
}

public enum GoalStatus
{
    InProgress,
    Completed,
    Paused
}

