namespace Raqmiyat.Framework.Model
{
    public class DBBatchPaymentParams
    {
        public string? RefenceNbr { get; set; }
        public decimal SrlNbr { get; set; }
        public string? Payment_Type { get; set; }
        public string? Interbank_Settlement_Date { get; set; }
        public string? EndToEnd_Identification { get; set; }
        public string? Transaction_Identification { get; set; }
        public string? UETR { get; set; }
        public string? Local_Instrument_Code { get; set; }
        public string? Category_Purpose_Code { get; set; }
        public string? Active_Currency { get; set; }
        public decimal Interbank_Settlement_Amount { get; set; }
        public string? Charge_Bearer { get; set; }
        public string? AccptanceDateTime { get; set; }
        public string? Debtor_Name { get; set; }
        public string? Debtor_Identification { get; set; }
        public string? Debtor_Identification_Code { get; set; }
        public string? Debtor_Issuer { get; set; }
        public string? Debtor_IBAN { get; set; }
        public string? Debtor_Account_Type { get; set; }
        public string? Debtor_Identity_Type { get; set; }
        public string? Debtor_Identity_Number { get; set; }
        public string? Debtor_BirthDate { get; set; }
        public string? Debtor_CityOfBirth { get; set; }
        public string? Debtor_CountryOfBirth { get; set; }
        public string? Debtor_Economic_Activity_Code { get; set; }
        public string? Debtor_Trade_License_Number { get; set; }
        public string? Debtor_Emirates_Code { get; set; }
        public string? Issuer_Type_Code { get; set; }
        public string? Debtor_Institution_Identification { get; set; }
        public string? Creditor_Institution_Identification { get; set; }
        public string? Creditor_Name { get; set; }
        public string? Creditor_IBAN { get; set; }
        public string? Creditor_Identification_Code { get; set; }
        public string? Purpose_of_payment { get; set; }
        public string? Remittance_Information { get; set; }
        public string? Debtor_Acc_Not_IBAN { get; set; }
        public string? Creditor_Acc_Not_IBAN { get; set; }
        public string? Creditor_Card_Type_Code { get; set; }
        public string? Debtor_Card_Type_Code { get; set; }
    }
}