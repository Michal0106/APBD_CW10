using Apbd10.Dto;
using Apbd10.Models;

namespace Apbd10.Services;

public interface IVisitService
{
    Task<PatientInfoDto> GetPatientInfo(int patientId);
    Task<List<Prescription>> AddPrescription(NewPrescriptionDto newPrescriptionDto);
    Task<bool> MedicamentDoesntExist(MedicamentDto medicamentDto);
    bool Over10Medicaments(NewPrescriptionDto newPrescriptionDto);
    bool DatesNotCorrect(NewPrescriptionDto newPrescriptionDto);
}