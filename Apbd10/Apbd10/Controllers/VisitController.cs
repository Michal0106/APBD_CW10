using Apbd10.Context;
using Apbd10.Dto;
using Apbd10.Models;
using Apbd10.Services;
using Microsoft.AspNetCore.Mvc;

namespace Apbd10.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VisitController : ControllerBase
{
    private readonly IVisitService _visitService;

    public VisitController(IVisitService visitService)
    {
        _visitService = visitService;
    }

    [HttpPost]
    public async Task<IActionResult> AddPrescription(NewPrescriptionDto newPrescriptionDto)
    {
        try
        {
            var prescriptions = await _visitService.AddPrescription(newPrescriptionDto);
            return Ok(prescriptions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPatientInfo(int id)
    {
        var patientInfo = await _visitService.GetPatientInfo(id);
        if (patientInfo == null)
        {
            return NotFound();
        }
        return Ok(patientInfo);
    }
}