namespace Medical.DTOs;

public class PrescriptionRequest
{
    public string PatientFirstName { get; set; }
    public string PatientLastName { get; set; }
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }
    public int IdDoctor { get; set; }
    public List<int> MedicamentIds { get; set; }
}