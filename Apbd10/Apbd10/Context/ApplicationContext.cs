using Apbd10.Models;
using Microsoft.EntityFrameworkCore;

namespace Apbd10.Context;

public class ApplicationContext : DbContext
{
    protected ApplicationContext()
    {
    }

    public ApplicationContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Medicament> Medicaments { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Prescription> Prescriptions { get; set; }
    public DbSet<PrescriptionMedicament> PrescriptionMedicaments { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PrescriptionMedicament>()
            .HasKey(pm => new { pm.IdPrescription, pm.IdMedicament });

        modelBuilder.Entity<PrescriptionMedicament>()
            .HasOne(pm => pm.Prescription)
            .WithMany(p => p.PrescriptionMedicaments)
            .HasForeignKey(pm => pm.IdPrescription);

        modelBuilder.Entity<PrescriptionMedicament>()
            .HasOne(pm => pm.Medicament)
            .WithMany(m => m.PrescriptionMedicaments)
            .HasForeignKey(pm => pm.IdMedicament);

        // Seed data
        modelBuilder.Entity<Doctor>().HasData(
            new Doctor { IdDoctor = 1, FirstName = "John", LastName = "Doe",Email = "123234@mail"}
        );

        modelBuilder.Entity<Patient>().HasData(
            new Patient { IdPatient = 1, FirstName = "Jane", LastName = "Doe", BirthDate = new DateTime(1990, 1, 1) }
        );

        modelBuilder.Entity<Medicament>().HasData(
            new Medicament { IdMedicament = 1, Name = "Medicament 1", Description = "Description 1",Type = "type1"},
            new Medicament { IdMedicament = 2, Name = "Medicament 2", Description = "Description 2" ,Type = "type1"},
            new Medicament { IdMedicament = 3, Name = "Medicament 3", Description = "Description 3" ,Type = "type2"}
        );

        modelBuilder.Entity<Prescription>().HasData(
            new Prescription { IdPrescription = 1, Date = new DateTime(2023, 6, 10), DueDate = new DateTime(2023, 7, 10), IdPatient = 1, IdDoctor = 1 }
        );

        modelBuilder.Entity<PrescriptionMedicament>().HasData(
            new PrescriptionMedicament { IdPrescription = 1, IdMedicament = 1, Dose = 3 ,Details = "Details1"},
            new PrescriptionMedicament { IdPrescription = 1, IdMedicament = 2, Dose = 2 ,Details = "Details2"}
        );
    }
}