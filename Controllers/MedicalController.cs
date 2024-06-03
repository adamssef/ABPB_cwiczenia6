using Microsoft.AspNetCore.Mvc;
using Medical.Context;
using Medical.DTOs;
using Medical.Models;
using Microsoft.EntityFrameworkCore;

namespace Medical.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicalController : ControllerBase
    {
        private readonly MedicalContext _dbContext;

        public MedicalController(MedicalContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult GetMedicaments()
        {
            var medicaments = _dbContext.Medicaments;
            return Ok(medicaments);
        }

        [HttpPost]
        [Route("dodaj-recepte")]
        public IActionResult AddPrescription([FromBody] PrescriptionRequest request)
        {
            if (request.MedicamentIds.Count > 10)
            {
                return BadRequest("Recepta może obejmować maksymalnie 10 leków.");
            }

            if (request.DueDate < request.Date)
            {
                return BadRequest("DueDate musi być większa lub równa Date.");
            }

            var patient = _dbContext.Patients.FirstOrDefault(p =>
                p.FirstName == request.PatientFirstName && p.LastName == request.PatientLastName);
            if (patient == null)
            {
                patient = new Patient
                {
                    FirstName = request.PatientFirstName,
                    LastName = request.PatientLastName
                };
                _dbContext.Patients.Add(patient);
                _dbContext.SaveChanges();
            }

            var medicaments = _dbContext.Medicaments.Where(m => request.MedicamentIds.Contains(m.IdMedicament))
                .ToList();
            if (medicaments.Count != request.MedicamentIds.Count)
            {
                return BadRequest("Jeden lub więcej leków nie istnieje.");
            }

            var prescription = new Prescription
            {
                Date = DateOnly.FromDateTime(request.Date),
                DueDate = DateOnly.FromDateTime(request.DueDate),
                IdPatient = patient.IdPatient,
                IdDoctor = request.IdDoctor
            };

            _dbContext.Prescriptions.Add(prescription);
            _dbContext.SaveChanges();

            foreach (var medicamentId in request.MedicamentIds)
            {
                var prescriptionMedicament = new PrescriptionMedicament
                {
                    IdMedicament = medicamentId,
                    IdPrescription = prescription.IdPrescription
                };
                _dbContext.PrescriptionMedicaments.Add(prescriptionMedicament);
            }

            _dbContext.SaveChanges();

            return Ok("Recepta została pomyślnie dodana.");
        }

        [HttpGet]
        [Route("pacjent/{id}")]
        public IActionResult GetPatient(int id)
        {
            var patient = _dbContext.Patients
                .Include(p => p.Prescriptions)
                    .ThenInclude(pr => pr.PrescriptionMedicaments)
                        .ThenInclude(pm => pm.IdMedicamentNavigation)
                .Include(p => p.Prescriptions)
                    .ThenInclude(pr => pr.IdDoctorNavigation)
                .FirstOrDefault(p => p.IdPatient == id);

            if (patient == null)
            {
                return NotFound("Pacjent nie istnieje.");
            }

            var response = new PatientResponse
            {
                IdPatient = patient.IdPatient,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                Birthdate = patient.Birthdate,
                Prescriptions = patient.Prescriptions
                    .OrderBy(pr => pr.DueDate)
                    .Select(pr => new PrescriptionResponse
                    {
                        IdPrescription = pr.IdPrescription,
                        Date = pr.Date.HasValue ? pr.Date.Value : (DateOnly?)null,
                        DueDate = pr.DueDate.HasValue ? pr.DueDate.Value : (DateOnly?)null,
                        Doctor = pr.IdDoctorNavigation != null ? new DoctorResponse
                        {
                            IdDoctor = pr.IdDoctorNavigation.IdDoctor,
                            FirstName = pr.IdDoctorNavigation.FirstName,
                            LastName = pr.IdDoctorNavigation.LastName,
                            Email = pr.IdDoctorNavigation.Email
                        } : null,
                        Medicaments = pr.PrescriptionMedicaments.Select(pm => new MedicamentResponse
                        {
                            IdMedicament = pm.IdMedicamentNavigation.IdMedicament,
                            Name = pm.IdMedicamentNavigation.Name,
                            Description = pm.IdMedicamentNavigation.Description,
                            Type = pm.IdMedicamentNavigation.Type,
                            Dose = pm.Dose ?? 0
                        }).ToList()
                    }).ToList()
            };

            return Ok(response);
        }
    }
}
