namespace PrescriptionDecoder.Domain.Entities
{
    public class Prescription
    {
        public string DoctorName { get; set; }
        public string Specialist { get; set; }
        public string Notes { get; set; }
        public List<Medicine> Medicines { get; set; } = new List<Medicine>();
    }
}
