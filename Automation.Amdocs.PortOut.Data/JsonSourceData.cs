using System;

namespace Automation.Amdocs.PortOut.Data
{
    [Serializable]
    public class JsonSourceData
    {
        public string CaseNumber { get; set; }
        public string CustomerNumberUfix { get; set; }
        public string CustomerNumberHistory { get; set; }
        public string CTN { get; set; }
        public string ContractType { get; set; }
        public string PortOutBlocking { get; set; }
        public string PortOutBlockingCode { get; set; }
        public string CloseCaseNotes { get; set; }
        public string Flash { get; set; }
        public string ProposedDate { get; set; }
        public string Pending { get; set; }
        public string PendingCease { get; set; }
        public string DaysBetweenProposedDateAndToday { get; set; }
        public JsonSourceData()
        {
            CaseNumber = string.Empty;
            
            CustomerNumberUfix = string.Empty;
            CustomerNumberHistory = string.Empty;
            CTN = string.Empty;
            ContractType = string.Empty;
            PortOutBlocking = string.Empty;
            PortOutBlockingCode = string.Empty;
            CloseCaseNotes = string.Empty;
            Flash = string.Empty;
            ProposedDate = string.Empty;
            Pending = string.Empty;
            PendingCease = string.Empty;
            DaysBetweenProposedDateAndToday = string.Empty;
        }
    }
}