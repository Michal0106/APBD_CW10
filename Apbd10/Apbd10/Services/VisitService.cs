using Apbd10.Context;
using Apbd10.Dto;
using Apbd10.Models;
using Apbd10.Services;
using Microsoft.EntityFrameworkCore;

public class VisitService : IVisitService
{
    private readonly ApplicationContext _applicationContext;

    public VisitService(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async Task<List<Prescription>> AddPrescription(NewPrescriptionDto newPrescriptionDto)
    {
        if (await PatientDoesntExist(newPrescriptionDto))
        {
            await AddPatient(newPrescriptionDto);
        }

        if (Over10Medicaments(newPrescriptionDto))
        {
            throw new InvalidOperationException("Cannot add more than 10 medications to a prescription.");
        }

        if (DatesNotCorrect(newPrescriptionDto))
        {
            throw new InvalidOperationException("Due date cannot be earlier than prescription date.");
        }

        var prescription = new Prescription()
        {
            Date = newPrescriptionDto.Date,
            DueDate = newPrescriptionDto.DueDate,
            IdPatient = newPrescriptionDto.PatientDto.IdPatient,
            IdDoctor = newPrescriptionDto.DoctorDto.IdDoctor
        };

        using (var transaction = await _applicationContext.Database.BeginTransactionAsync())
        {
            try
            {
                await _applicationContext.Prescriptions.AddAsync(prescription);
                await _applicationContext.SaveChangesAsync();
                
                await AddPrescriptionMedicaments(prescription.IdPrescription, newPrescriptionDto.MedicamentDtos);
                await _applicationContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        return await _applicationContext.Prescriptions.ToListAsync();
    }

    public async Task<PatientInfoDto> GetPatientInfo(int patientId)
    {
        var patient = await _applicationContext.Patients
            .Include(p => p.Prescriptions)
                .ThenInclude(pr => pr.PrescriptionMedicaments)
                    .ThenInclude(prm => prm.Medicament)
            .Include(p => p.Prescriptions)
                .ThenInclude(pr => pr.Doctor)
            .FirstOrDefaultAsync(p => p.IdPatient == patientId);

        if (patient == null)
        {
            return null;
        }

        var patientInfoDto = new PatientInfoDto()
        {
            IdPatient = patient.IdPatient,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            BirthDate = patient.BirthDate,
            Prescriptions = patient.Prescriptions
                .OrderBy(pr => pr.DueDate)
                .Select(pr => new PrescriptionDto()
                {
                    IdPrescription = pr.IdPrescription,
                    Date = pr.Date,
                    DueDate = pr.DueDate,
                    Medicaments = pr.PrescriptionMedicaments.Select(prm => new MedicamentDto()
                    {
                        IdMedicament = prm.IdMedicament,
                        Name = prm.Medicament.Name,
                        Dose = prm.Dose,
                        Description = prm.Medicament.Description
                    }).ToList(),
                    Doctor = new DoctorDto()
                    {
                        IdDoctor = pr.IdDoctor,
                        FirstName = pr.Doctor.FirstName,
                        LastName = pr.Doctor.LastName
                    }
                }).ToList()
        };

        return patientInfoDto;
    }

    public async Task<bool> PatientDoesntExist(NewPrescriptionDto newPrescriptionDto)
    {
        var patientDto = newPrescriptionDto.PatientDto;
        var patient = await _applicationContext.Patients.FirstOrDefaultAsync(p => p.IdPatient == patientDto.IdPatient);
        return patient == null;
    }

    public async Task<bool> MedicamentDoesntExist(MedicamentDto medicamentDto)
    {
        var findMedicament = await _applicationContext.Medicaments.FirstOrDefaultAsync(p => p.IdMedicament == medicamentDto.IdMedicament);
        return findMedicament == null;
    }

    public bool Over10Medicaments(NewPrescriptionDto newPrescriptionDto)
    {
        return newPrescriptionDto.MedicamentDtos.Count > 10;
    }

    public async Task AddPatient(NewPrescriptionDto newPrescriptionDto)
    {
        var patientDto = newPrescriptionDto.PatientDto;

        var newPatient = new Patient()
        {
            IdPatient = patientDto.IdPatient,
            FirstName = patientDto.FirstName,
            LastName = patientDto.LastName,
            BirthDate = patientDto.BirthDate
        };

        await _applicationContext.Patients.AddAsync(newPatient);
        await _applicationContext.SaveChangesAsync();
    }

    public bool DatesNotCorrect(NewPrescriptionDto newPrescriptionDto)
    {
        return newPrescriptionDto.DueDate < newPrescriptionDto.Date;
    }

    public async Task AddPrescriptionMedicaments(int prescriptionId, ICollection<MedicamentDto> medicamentDtos)
    {
        foreach (var medicamentDto in medicamentDtos)
        {
            if (await MedicamentDoesntExist(medicamentDto))
            {
                throw new InvalidOperationException($"Medicament with ID {medicamentDto.IdMedicament} does not exist.");
            }

            var prescriptionMedicament = new PrescriptionMedicament
            {
                IdMedicament = medicamentDto.IdMedicament,
                IdPrescription = prescriptionId,
                Dose = medicamentDto.Dose
            };

            await _applicationContext.PrescriptionMedicaments.AddAsync(prescriptionMedicament);
        }
    }
}
