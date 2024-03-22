using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Otus.Teaching.PromoCodeFactory.Core.Abstractions.Repositories;
using Otus.Teaching.PromoCodeFactory.Core.Domain.Administration;
using Otus.Teaching.PromoCodeFactory.WebHost.Models;

namespace Otus.Teaching.PromoCodeFactory.WebHost.Controllers
{
    /// <summary>
    /// Сотрудники
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class EmployeesController
        : ControllerBase
    {
        private readonly IRepository<Employee> _employeeRepository;

        public EmployeesController(IRepository<Employee> employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }
        
        /// <summary>
        /// Получить данные всех сотрудников
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<EmployeeShortResponse>> GetEmployeesAsync()
        {
            var employees = await _employeeRepository.GetAllAsync();

            var employeesModelList = employees.Select(x => 
                new EmployeeShortResponse()
                    {
                        Id = x.Id,
                        Email = x.Email,
                        FullName = x.FullName,
                    }).ToList();

            return employeesModelList;
        }
        
        /// <summary>
        /// Получить данные сотрудника по Id
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<EmployeeResponse>> GetEmployeeByIdAsync(Guid id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);

            if (employee == null)
                return NotFound();
            
            var employeeModel = new EmployeeResponse()
            {
                Id = employee.Id,
                Email = employee.Email,
                Roles = employee.Roles.Select(x => new RoleItemResponse()
                {
                    Name = x.Name,
                    Description = x.Description
                }).ToList(),
                FullName = employee.FullName,
                AppliedPromocodesCount = employee.AppliedPromocodesCount
            };

            return employeeModel;
        }

        [HttpPost]
        public async Task<ActionResult<EmployeeModel>> CreateEmployee(EmployeeModel model)
        {
            model.Id = Guid.NewGuid();
            var employee = new Employee()
            {
                Email = model.Email,
                LastName = model.LastName,
                FirstName = model.FirstName,
                Roles = model.Roles.Select(x => new Role()
                {
                    Name = x.Name,
                    Description = x.Description
                }).ToList()??new List<Role>()
            };
            var newEmp =  await _employeeRepository.CreateAsync(employee);
            var employeeModel = new EmployeeResponse()
            {
                Id = newEmp.Id,
                Email = newEmp.Email,
                Roles = newEmp.Roles.Select(x => new RoleItemResponse()
                {
                    Name = x.Name,
                    Description = x.Description
                }).ToList(),
                FullName = newEmp.FullName,
                AppliedPromocodesCount = newEmp.AppliedPromocodesCount
            };

            return Ok(employeeModel);
        }

        [HttpDelete("deleteEmp/{id}")]
        public async Task<ActionResult> DeleteEmployee(Guid id)
        {
            var employees = await _employeeRepository.GetAllAsync();
            var emp = employees.FirstOrDefault(x => x.Id == id);
            if (emp == null) return BadRequest();
            await _employeeRepository.DeleteAsync(emp);
            return Ok();
        }

        [HttpPost("update/{id}")]
        public async Task<ActionResult> UpdateEmployee(Guid id,EmployeeModel model)
        {
            var emp = await _employeeRepository.GetByIdAsync(id);
            if (emp == null) return BadRequest();
            emp.Email = model.Email;
            emp.Roles = model.Roles.Select(x => new Role()
            {
                Name = x.Name,
                Description = x.Description,
                Id = x.Id
            }).ToList();
            emp.LastName = model.LastName;
            emp.FirstName = model.FirstName;
            var response = await _employeeRepository.UpdateAsync(emp);
            return Ok(response);
        }
    }
}