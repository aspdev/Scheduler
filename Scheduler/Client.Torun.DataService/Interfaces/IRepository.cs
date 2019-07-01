using Client.Torun.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Client.Torun.DataService.Interfaces
{
    public interface IRepository
    {
        Task<int> GetNumberOfDoctorsOnDuty();
        Task<int> GetTotalNumberOfDoctors();
        Task<List<Guid>> GetDoctorIds();
        Task<List<Tuple<Guid, DateTime>>> GetDaysOffForMonth(DateTime date);
        List<string> GetPublicHolidays(int year, int month);
        Task<List<KeyValuePair<Guid, Tuple<int, int, int>>>> GetDutyRequirementsForMonth(int year, int month);
        Task<List<Guid>> GetDoctorsOnDutyLastDayOfPreviousMonth(int year, int month);
        Task<List<Tuple<Guid, DateTime>>> GetDaysOffOnLastDayOfPreviousMonth(int year, int currentMonth);
        Task<List<DoctorDto>> GetDoctorDtos();
    }
}
