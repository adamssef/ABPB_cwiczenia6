using Medical.DTOs;

namespace Medical.DTOs;

public class PrescriptionResponse
{
    public int IdPrescription { get; set; }
    public DateOnly? Date { get; set; }
    public DateOnly? DueDate { get; set; }
    public DoctorResponse? Doctor { get; set; }
    public List<MedicamentResponse> Medicaments { get; set; }
}