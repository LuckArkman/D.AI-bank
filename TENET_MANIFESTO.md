# Tenet Architecture Manifesto

## 1. Core Imutável (Tenet Core)
- **Financeiro**: Ledger de dupla entrada (Double-Entry Ledger) agnóstico a moeda.
- **Identidade**: Gestão de identidades (Principals) e permissões.
- **Event Engine**: Barramento de eventos (Outbox/Inbox) para consistência eventual.
- **Infra**: Observabilidade, Auditoria e Telemetria.
- **Universal**: Jamais contém `if (country == "BR")`.

## 2. Regulatory Packs
- **KYC/AML**: Fluxos de verificação específicos por jurisdição.
- **Fiscal**: Motores de cálculo de impostos regionais.
- **Reporting**: Adaptadores para órgãos reguladores (BACEN, SEC, EBA).
- **Limits**: Motores de limites baseados em perfil e país.

## 3. Multimodalidade
- **Modos Ativos**: Bank, Payment Inst, Crypto Exchange, Web3 Wallet.
- **Interação**: Multi-interface (Web/API/Mobile) e Multi-custódia.

## 4. Institutional Profile (The "Client Tenet")
- Define o território de operação.
- Associa os produtos e modos regulatórios permitidos.
- Configura parceiros locais de liquidação.
