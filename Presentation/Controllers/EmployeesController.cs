﻿using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Shared.DataTransferObjects;

namespace Presentation.Controllers
{
    [Route("api/companies/{companyid}/employees")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IServiceManager _service;

        public EmployeesController(IServiceManager service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult GetEmployeesForCompany(Guid companyId)
        {
            var employees = _service.EmployeeService.GetEmployees(companyId, false);
            return Ok(employees);
        }

        [HttpGet("{employeeId:guid}",Name= "GetEmployeeForCompany")]
        public IActionResult GetEmployees(Guid companyId, Guid employeeId)
        {
            var employee = _service.EmployeeService.GetEmployee(companyId, employeeId, false);
            return Ok(employee);
        }

        [HttpPost]
        public IActionResult CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeForCreationDto employee)
        {
            if (employee is null)
                return BadRequest("EmployeeForCreationDto object is null");
            if (!ModelState.IsValid) return UnprocessableEntity(ModelState);
            var employeeToReturn =
                _service.EmployeeService.CreateEmployeeForCompany(companyId, employee, trackChanges: false);
            return CreatedAtRoute("GetEmployeeForCompany", new { companyId, employeeId = employeeToReturn.Id }, employeeToReturn);
        }

        [HttpDelete("{employeeId:guid}")]
        public IActionResult DeleteEmployeeForCompany(Guid companyId, Guid employeeId)
        {
            _service.EmployeeService.DeleteEmployeeForCompany(companyId, employeeId, trackChanges: false);
            return NoContent();
        }

        [HttpPut("{employeeId:guid}")]
        public IActionResult UpdateEmployeeForCompany(Guid companyId, Guid employeeId, [FromBody] EmployeeForUpdateDto employee)
        {
            if (employee is null)
                return BadRequest("EmployeeForUpdateDto object is null");

            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);

            _service.EmployeeService.UpdateEmployeeForCompany(companyId, employeeId, employee, compTrackChanges: false, empTrackChanges: true);
            return NoContent();
        }

        [HttpPatch("{id:guid}")]
        public IActionResult PartiallyUpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody] JsonPatchDocument<EmployeeForUpdateDto> patchDoc)
        {
            if (patchDoc is null)
                return BadRequest("patchDoc object sent from client is null.");
            var result = _service.EmployeeService.GetEmployeeForPatch(companyId, id, compTrackChanges: false,
                empTrackChanges: true);
            patchDoc.ApplyTo(result.employeeToPatch,ModelState);
            TryValidateModel(result.employeeToPatch);
            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);
            _service.EmployeeService.SaveChangesForPatch(result.employeeToPatch, result.employeeEntity);
            return NoContent();
        }
    }
}
