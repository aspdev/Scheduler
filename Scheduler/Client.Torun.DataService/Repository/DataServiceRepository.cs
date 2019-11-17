using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Client.Torun.DataService.Interfaces;
using Client.Torun.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Client.Torun.DataService.Repository
{
    public class DataServiceRepository : IRepository
    {
        private readonly DataServiceContext _context;
        

        public DataServiceRepository(DataServiceContext context)
        {
            _context = context;
        }

        public async Task<int> GetNumberOfDoctorsOnDuty()
        {
            return await _context.AdminSettings.Select(s => s.NumberOfDoctorsOnDuty).FirstAsync();

        }

        public async Task<int> GetTotalNumberOfDoctors()
        {
            return await _context.Users.CountAsync();
        }

        public async Task<List<Guid>> GetDoctorIds()
        {
            return await _context.Users.Select(u => u.UserId).ToListAsync();
        }

        public async Task<List<Tuple<Guid, DateTime>>> GetDaysOffForMonth(DateTime date)
        {
            return await _context.DaysOff
                .Where(d => d.Date.Year == date.Year && d.Date.Month == date.Month)
                .Select(d => new Tuple<Guid, DateTime>(d.UserId, d.Date))
                .ToListAsync();
        }

        public List<string> GetPublicHolidays(int year, int month)
        {
            List<DateTime> publicHolidays = new List<DateTime>()
            {
                new DateTime(year, 1, 1), // Nowy Rok
                new DateTime(year, 1, 6), // Trzech Króli
                new DateTime(year, 5, 1), // Święto Pracy
                new DateTime(year, 5, 3), // Święto Konstytucji
                new DateTime(year, 8, 15), // Święto Wojska Polskiego
                new DateTime(year, 11, 1), // Dzień Wszystkich Świętych
                new DateTime(year, 11, 11), // Dzień Niepodległości
                new DateTime(year, 12, 25), // Boże Narodzenie
                new DateTime(year, 12, 26) // 2 dzień Świąt Bożego Narodzenia
            };

            DateTime easterMondayDate = CalculateEasterDate(year).AddDays(1);
            DateTime corpusChristiDate = CalculateEasterDate(year).AddDays(60);

            publicHolidays.Add(easterMondayDate);
            publicHolidays.Add(corpusChristiDate);

            return publicHolidays.Where(h => h.Year == year && h.Month == month)
                .Select(h => h.ToShortDateString()).ToList();
        }

        public async Task<List<KeyValuePair<Guid, Tuple<int, int, int>>>> GetDutyRequirementsForMonth(int year, int month)
        {
            var dutyRequirements = await _context.DutyRequirements
                .Where(dr => dr.Date.Year == year && dr.Date.Month == month)
                .Select(dr => new KeyValuePair<Guid, Tuple<int, int, int>>(dr.UserId,
                    new Tuple<int, int, int>(dr.RequiredTotalDutiesInMonth, dr.TotalWeekdayDuties,
                        dr.TotalHolidayDuties)))
                .ToListAsync();

            return dutyRequirements;
        }

        public async Task<List<Guid>> GetDoctorsOnDutyLastDayOfPreviousMonth(int year, int month)
        {
            DateTime previousMonth = new DateTime(year, month, 1).AddMonths(-1);

            var doctors = await _context.Duties
                .Where(d => d.Date.Year == previousMonth.Year &&
                            d.Date.Month == previousMonth.Month && 
                            d.Date.Day == DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month))
                .Select(d => d.UserId).ToListAsync();

            return doctors;
        }

        public async Task<List<Tuple<Guid, DateTime>>> GetDaysOffOnLastDayOfPreviousMonth(int year, int currentMonth)
        {
            DateTime previousMonthDate = new DateTime(year, currentMonth, 1).AddMonths(-1);
            int lastDay = DateTime.DaysInMonth(previousMonthDate.Year, previousMonthDate.Month);

            return await _context.DaysOff
                .Where(d => d.Date.Year == previousMonthDate.Year && d.Date.Month == previousMonthDate.Month && d.Date.Day == lastDay)
                .Select(d => new Tuple<Guid, DateTime>(d.UserId, d.Date))
                .ToListAsync();
        }

        public async Task<List<DoctorDto>> GetDoctorDtos()
        {
            var doctorDtos = await _context.Users.Select(u => 
                new DoctorDto() { DoctorId = u.UserId.ToString(), Email = u.Email, Name = u.FirstName + " " + u.LastName }).ToListAsync();

            return doctorDtos; 
        }



        private DateTime CalculateEasterDate(int year)
        {
            int day = 0;
            int month = 0;

            int g = year % 19;
            int c = year / 100;
            int h = (c - (int)(c / 4) - (int)((8 * c + 13) / 25) + 19 * g + 15) % 30;
            int i = h - (int)(h / 28) * (1 - (int)(h / 28) * (int)(29 / (h + 1)) * (int)((21 - g) / 11));

            day = i - ((year + (int)(year / 4) + i + 2 - c + (int)(c / 4)) % 7) + 28;
            month = 3;

            if (day > 31)
            {
                month++;
                day -= 31;
            }

            return new DateTime(year, month, day);
        }

        
    }
}
