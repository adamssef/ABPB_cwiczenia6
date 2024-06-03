namespace Medical.DTOs;

public class PatientResponse
{
    public int IdPatient { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? Birthdate { get; set; }
    public List<PrescriptionResponse> Prescriptions { get; set; }
}