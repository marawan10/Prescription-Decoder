namespace PrescriptionDecoder.Domain.Entities
{
    public class Medicine
    {
        public string Drug { get; set; }
        public string Dose { get; set; }
        public string Freq { get; set; }
        public string Notes { get; set; }
        public int Confidence { get; set; } // 0-100
        public bool RequiresManualReview { get; set; }
    }
}
