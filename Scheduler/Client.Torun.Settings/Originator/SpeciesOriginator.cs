using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Client.Torun.Settings.Enumeration;
using Client.Torun.Settings.Utility;
using Client.Torun.Shared.DTOs;
using Scheduler.Shared.Classes;
using Scheduler.Shared.CustomExceptions;
using Scheduler.Shared.Extensions;
using Scheduler.Shared.Interfaces;

namespace Client.Torun.Settings.Originator
{

    public class SpeciesOriginator : ISpeciesOriginator
    {
        // UWAGA: Interwał dyżurowy jest zdefiniowany na sztywno

        private static readonly HttpClient HttpClient = new HttpClient(new HttpClientHandler()
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip
        })
        {
            BaseAddress = new Uri("http://localhost:51301"),
            Timeout = new TimeSpan(0, 0, 30), // UWAGA! TimeOut nie jest na razie obsłużony
            DefaultRequestHeaders =
            {
                Accept = { new MediaTypeWithQualityHeaderValue("application/json")}, 
                AcceptEncoding = { new StringWithQualityHeaderValue("gzip")},

            }
        };

        private static string _auth;
        private SpinLock _spinLock = new SpinLock();
        private readonly DateTime _date;
        private int _numberOfDoctorsOnDuty;
        private int _populationSize;
        private int _totalNumberOfDoctors;
        private List<string> _doctorIds = new List<string>();
        private List<DayOffDto> _daysOff = new List<DayOffDto>();
        private List<DayOffOnLastDayPreviousMonthDto> _daysOffOnLastDayOfPreviousMonth = new List<DayOffOnLastDayPreviousMonthDto>();

        private List<DutyRequirementForMonthDto> _dutyRequirementsForMonth =
            new List<DutyRequirementForMonthDto>();

        private readonly List<Individual> _individuals = new List<Individual>();
        private List<DateTime> _publicHolidaysForMonth = new List<DateTime>();
        private List<string> _doctorsOnDutyLastDayOfPreviousMonth = new List<string>();
        private List<DoctorDto> _doctorDtos = new List<DoctorDto>();
        


        private int AllowedMaxDaysOffOnSingleDay => _totalNumberOfDoctors - _numberOfDoctorsOnDuty;

        private int AllowedMaxDaysOffOnSeveralConsequtiveDays => _totalNumberOfDoctors - (_numberOfDoctorsOnDuty * 2); // 2 = interwał dyżurowy

        private List<IGrouping<int, DateTime>> DaysOffGrouppedByDayNumber
        {
            get { return _daysOff.Select(d => d.Date).GroupBy(d => d.Day).ToList(); }
        }
        
        public SpeciesOriginator(string authorizationToken, DateTime date)
        {
            if (string.IsNullOrEmpty(_auth))
            {
                _auth = authorizationToken;
                HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_auth}");
            }
            else
            {
                if (string.Equals(_auth, authorizationToken, StringComparison.Ordinal) == false)
                {
                    _auth = authorizationToken;
                    HttpClient.DefaultRequestHeaders.Remove("Authorization");
                    HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_auth}");
                }
            }
            
            _date = date;
           
        }

        public async Task SetDataFromCustomApi()
        {

            var first = Task.Run(async () => _numberOfDoctorsOnDuty = await GetNumberOfDoctorsOnDutyAsync());
            var second = Task.Run(async () => _populationSize = await GetPopulationSizeAsync());
            var third = Task.Run(async () => _totalNumberOfDoctors = await GetTotalNumberOfDoctorsAsync());
            var fourth = Task.Run(async () => _doctorIds = await GetDoctorIdsAsync());
            var fifth = Task.Run(async () => _daysOff = await GetDaysOffAsync());
            var sixth = Task.Run(async () => _publicHolidaysForMonth = await GetPublicHolidaysForMonthAsync());
            var seventh = Task.Run(async () => _dutyRequirementsForMonth = await GetDutyRequirementsForMonthAsync());
            var eighth = Task.Run(async () =>
                _doctorsOnDutyLastDayOfPreviousMonth = await GetDoctorsOnDutyLastDayOfPreviousMonthAsync());
            var ninth = Task.Run(async () =>
                _daysOffOnLastDayOfPreviousMonth = await GetDaysOffOnLastDayOfPreviousMonthAsync());
            var tenth = Task.Run(async () =>
                _doctorDtos = await GetDoctorDtos());

            var whenAll = Task.WhenAll(first, second, third, fourth, fifth, sixth, seventh, eighth, ninth, tenth);

            try
            {
                await whenAll;
            }
            catch
            {
                throw whenAll.Exception;
                
            }



        }
        
        public PreGenerationCheckMessenger PerformPreGenerationChecks()
        {
            PreGenerationCheckMessenger messenger = new PreGenerationCheckMessenger();

            // 1. Sprawdzamy czy całkowita liczba lekarzy wystarczy, aby umożliwić ułożenie grafiku z wymaganym interwałem dyżurowym i wymaganą ilością lekarzy na dyżurze
            //    Np. 2 dyżury dziennie z interwałem 2 - musi byc minimum 4 lekarzy
            // Zbyt mała całkowita ilość lekarzy w stosunku do wymaganej ilości lekarzy na dyżurze i przyjętego interwału dyżurowego

            if (!IsTotalDoctorsGreaterOrEqualToNumberOfDoctorsOnDutyTimesDutyInterval())
            {
                string message = "Not enough doctors relative to the duty interval";
                messenger.Messages.Add(message);
            }

            // 2. Sprawdzamy czy danego dnia ilość niemożności nie jest zbyt duża w stosunku do wymaganej ilości dyżurantów
            //    Np. Jest 7 lekarzy a 6 wpisuje niemożność
            foreach (var day in DaysOffGrouppedByDayNumber)
            {
                if (IsTotalDaysOffOnADayGreaterThanAllowedMaxDaysOffOnSingleDay(day.Count()))
                {
                    string message = $"Too many absences on {new DateTime(_date.Year, _date.Month, day.Key).ToShortDateString()}. Cannot assign required number of doctors.";

                    messenger.Messages.Add(message);
                }
            }

            // 3. Czy jeśli WSZYSCY lekarze zgłosili żądanie dyżurowe, to czy ilość wszystkich dyżurów jest większa niż zgłaszana dostępność lekarzy (niepokryte godziny)
            //    Np. Jeśli L1 - 3 dyżury, L2 - 8 dyżurów, etc a suma < niż 60 (bo tyle jest w miesiącu)
            //    Suma żądanej ilości dyżurów jest o {diff} mniejsza niż całkowita wymagana ilość dyżurów w jednostce
            if (_dutyRequirementsForMonth.Count() == _numberOfDoctorsOnDuty)
            {
                int diff = CalculateDifferenceBetweenTotalDutiesAndTotalDutyRequirements();

                if (diff > 0)
                {
                    var dutySingularOrPlural = diff > 1 ? "duties" : "duty";
                    string message = $"The total of duties to assign in the month exceeds the total of duties requested. Cannot assign {diff} doctor {dutySingularOrPlural}.";

                    messenger.Messages.Add(message);
                }
            }


            // 4. A co jeśli nie wszyscy, ale CZĘŚĆ lekarzy zgłosiła żądanie dyżurowe??




            // 5. Sprawdzamy czy nie zabraknie lekarza na dyżurze z powodu układu kilku niemożności w kolejno następujących dniach (od dnia 2 do przedostatniego)
            //    Np. Dopuszczalna ilość niemożności to 7 - 4 (2*2). Jeśli obok sibie będzie 4 + 4 / 4 + 5 pojawi się konflikt z ograniczeniem interwałowym
            var keys = DaysOffGrouppedByDayNumber.Select(d => d.Key).ToList();

            for (int i = 0; i < DaysOffGrouppedByDayNumber.Count - 1; i++)
            {
                if (keys.Contains(DaysOffGrouppedByDayNumber[i].Key + 1))
                {

                    if (DaysOffGrouppedByDayNumber[i].Count() > AllowedMaxDaysOffOnSeveralConsequtiveDays &&
                        DaysOffGrouppedByDayNumber[i + 1].Count() > AllowedMaxDaysOffOnSeveralConsequtiveDays)
                    {
                        string message = $"Adverse arrangement of absences on {new DateTime(_date.Year, _date.Month, DaysOffGrouppedByDayNumber[i].Key).ToShortDateString()} and" +
                                         $" {new DateTime(_date.Year, _date.Month, DaysOffGrouppedByDayNumber[i].Key + 1).ToShortDateString()}";

                        messenger.Messages.Add(message);

                    }

                }

            }

            // 6. Czy nie zabraknie lakarza na dyżurze pierwszego dnia miesiąca z powodu niemożności ostatniego dnia poprzedniego miesiąca i niemożności pierwszego dnia bieżącego miesiąca.
            //    To samo co w punkcie 5 tylko w zakresie 1 dnia bieżącego miesiąca i ostatniego dnia poprzedniego miesiąca 
            var daysOffOnDayOne = DaysOffGrouppedByDayNumber.FirstOrDefault(d => d.Key == 1);
            
            if (daysOffOnDayOne != null && _daysOffOnLastDayOfPreviousMonth.Any())
            {
                if (daysOffOnDayOne.Count() > AllowedMaxDaysOffOnSeveralConsequtiveDays &&
                    _daysOffOnLastDayOfPreviousMonth.Count() > AllowedMaxDaysOffOnSeveralConsequtiveDays)
                {
                    DateTime firstDayOfCurrentMonth = new DateTime(_date.Year, _date.Month, 1);
                    DateTime previousMonthDate = firstDayOfCurrentMonth.AddMonths(-1);

                    string message =
                        $"Too many absences on {firstDayOfCurrentMonth.ToShortDateString()} " +
                        $"relative to {new DateTime(previousMonthDate.Year, previousMonthDate.Month, DateTime.DaysInMonth(previousMonthDate.Year, previousMonthDate.Month))}";

                    messenger.Messages.Add(message);
                }
            }

            return messenger;
        }

        public List<Individual> Initialize()
        {

            SetDaysOff();
            
            SetDuties();
            
            return _individuals;
        }

        public IAllel[,] Repair(IAllel[,] dna)
        {
            List<int> rows = new List<int>();

            // 1. Usuwamy zbędne allele jeśli w kolumnie istnieje więcej niż liczba lekarzy wymaganych na dyżurze
                
            // a) zliczamy allel OnDuty i rząd zapisujemy na liście

            // TODO: Usuwane allele mogą być tymi, których ilość została na sztywno zdefiniowana przez lekarza

            for (int i = 0; i < DateTime.DaysInMonth(_date.Year, _date.Month); i++)
            {
                for (int j = 0; j < _totalNumberOfDoctors; j++)
                {
                    for (int k = i; k <= i; k++)
                    {
                        if (dna[j, k] != null && dna[j, k].Value == Allel.OnDuty.Value)
                        {
                            rows.Add(j);
                        }
                    }
                }

                // b. jeśli w danym dniu jest więcej alleli OnDuty niż powinno być usuwamy zbędne 

                if (rows.Count > _numberOfDoctorsOnDuty)
                {
                    RemoveRedundantAlleles(dna, i, rows);
                }

                rows.Clear();
            }

            // 2. Jeśli w kolumnie jest mniej lekarzy niż powinno być dopełniamy do wymaganej liczby 

            /*
             * Niedostępności:
             * - kiedy ma już przypisany dyżur,
             * - kiedy jest wstawiona niemożność,
             * - kiedy ma dyżur następnego dnia,
             * - kiedy miał dyżur poprzedniego dnia,
             * - kiedy miał dyżur ostatniego dnia w poprzednim miesiącu (przypadek szczególny dla dyżurów 1 dnia)
             *
             */

            List<int> unavailableDoctors = new List<int>();

            for (int i = 0; i < DateTime.DaysInMonth(_date.Year, _date.Month); i++)
            {

                int onDutyCount = 0; // zliczamy ile jest na dyżurze

                for (int j = 0; j < _totalNumberOfDoctors; j++)
                {
                    for (int k = i; k <= i; k++)
                    {
                        if (dna[j, k] != null && dna[j, k].Value == Allel.OnDuty.Value)
                        {
                            onDutyCount++;
                            rows.Add(j);  // niedostępny jeśli ma juz przypisany dużur
                        }

                        if (dna[j, k] != null && dna[j, k].Value == Allel.DayOff.Value)
                        {
                            rows.Add(j); // niedostępny jeśli jest "niemożność"
                        }

                        if (k < (DateTime.DaysInMonth(_date.Year, _date.Month) - 1))
                        {
                            if (dna[j, k + 1] != null && dna[j, k + 1].Value == Allel.OnDuty.Value)
                            {
                                rows.Add(j); // niedostępny jeśli ma dyżur następnego dnia
                            }
                        }

                        if (k > 0)
                        {
                            if (dna[j, k - 1] != null && dna[j, k - 1].Value == Allel.OnDuty.Value)
                            {
                                rows.Add(j); // niedostępny niedostępny jeśli miał dyżur poprzedniego dnia
                            }
                        }

                        if (k == 0)
                        {
                            foreach (var doctor in _doctorsOnDutyLastDayOfPreviousMonth)
                            {
                               int index =  _doctorIds.IndexOf(doctor);
                               rows.Add(index); // niedostępny - przypadek szczególny - dużur poprzedniego dnia w poprzednim miesiącu
                            }
                        }

                        // spawdzamy czy możemy dodać lekarza ze względu na limity, które zdefiniował

                        /*
                         * Ponieważ istnieje korelacja pomiędzy indeksami listy "doctorIds" oraz indeksami wierszy w tablicy,
                         * możliwe jest wyciągniecie GuidId lekarza z listy doctorIds i sprawdzenie jego wymagań dyżurowych
                         */

                        // if(unavailableDoctors.Contains(j) --> nie wykonuj kodu poniżej, wiemy, że już jest niedostępny 

                        if (!unavailableDoctors.Contains(j))
                        {
                            string doctorId = _doctorIds[j];
                            DutyRequirementForMonthDto doctorDutyRequirement =
                                _dutyRequirementsForMonth.FirstOrDefault(d => d.UserId == doctorId);

                            // jeżeli sformułowano wymagania dotyczące ilości dyżurów
                            if (doctorDutyRequirement != null)
                            {
                                var numberOfDutiesInTheRow = dna.GetRow(j)
                                    .Count(a => a != null && a.Value == Allel.OnDuty.Value);

                                if (numberOfDutiesInTheRow >= doctorDutyRequirement.RequiredTotalDutiesInMonth)
                                {
                                    //rows.Add(j); // niesdostępny - wyczerpał zdefiniowany limit
                                    unavailableDoctors.Add(j); // dodajemy do listy niedostępnych
                                }
                                else
                                {

                                    DateTime currentDate = new DateTime(_date.Year, _date.Month, k + 1);

                                    // jeśli dzień jest świętem sprawdzamy czy limit świąt został wyczerpany

                                    if (IsWeekendOrPublicHoliday(currentDate))
                                    {
                                        var doctorDuties = dna.GetRow(j);
                                        int holidayDutiesCount = 0;

                                        // liczymy ile dyżurów ma w święta

                                        for (int d = 0; d < doctorDuties.Length; d++)
                                        {
                                            if (IsWeekendOrPublicHoliday(new DateTime(_date.Year, _date.Month, d + 1)) &&
                                                                         doctorDuties[d] != null &&
                                                                         doctorDuties[d].Value == Allel.OnDuty.Value)
                                            {
                                                holidayDutiesCount++;
                                            }
                                        }

                                        if (holidayDutiesCount >= doctorDutyRequirement.RequiredTotalHolidayDuties)
                                        {
                                            rows.Add(j); // niedostępny w święta - wyczerpał limit świąt
                                        }
                                    }
                                    else //sprawdzamy dni powszednie
                                    {
                                        var doctorDuties = dna.GetRow(j);
                                        int weekdayDutiesCount = 0;

                                        // liczymy ile dyżurów ma w dni powszednie

                                        for (int d = 0; d < doctorDuties.Length; d++)
                                        {
                                            if ((!IsWeekendOrPublicHoliday(new DateTime(_date.Year, _date.Month, d + 1))) &&
                                                doctorDuties[d] != null && doctorDuties[d].Value == Allel.OnDuty.Value)
                                            {
                                                weekdayDutiesCount++;
                                            }
                                        }

                                        if (weekdayDutiesCount >= doctorDutyRequirement.RequiredTotalWeekdayDuties)
                                        {
                                            rows.Add(j); // niedsotępny w dniu powszednie - wyczerpał limit
                                        }
                                    }

                                }


                            }
                        }

                        

                    }
                }

                // tutaj musimy dodać niedostepnych do rows

                var inaccessibleRows = rows.Union(unavailableDoctors).ToList();

               
                if (onDutyCount < _numberOfDoctorsOnDuty) // jeśli na dyżurze jest mniej niż wymagane uzupełniamy
                {
                    InsertMissingAllels(dna, i, onDutyCount, inaccessibleRows);
                }

                rows.Clear();
            }

            return dna;
        }

        public Dictionary<DateTime, Object> CustomFormatSolution(IAllel[,] dna, DateTime date)
        {
            Dictionary<DateTime, Object> formattedSolution = new Dictionary<DateTime, Object>();

            for (int i = 0; i < dna.GetLength(1); i++)
            {
                List<DoctorDto> listOfDoctors = new List<DoctorDto>();

                for (int j = 0; j < dna.GetLength(0); j++)
                {
                    for (int k = i; k <= i; k++)
                    {
                        if (dna[j, k] != null)
                        {
                            var doctorId = _doctorIds[j];
                            var doctorDto = _doctorDtos.FirstOrDefault(d => d.DoctorId == doctorId);
                            listOfDoctors.Add(doctorDto);
                        }


                    }

                }

                int day = i + 1;

                formattedSolution.Add(new DateTime(date.Year, date.Month, day), listOfDoctors);

                
            }

            return formattedSolution;

        }

        private async Task<List<DoctorDto>> GetDoctorDtos()
        {
            string url = "originator/doctordtos";

            using (var response = await HttpClient.GetAsync(url,
                HttpCompletionOption.ResponseHeadersRead))
            {
                await EnsureResponseIsSuccessful(response);

                var stream = await response.Content.ReadAsStreamAsync();

                List<DoctorDto> doctorDtos = stream.ReadAndDeserializeFromJson<List<DoctorDto>>();

                return doctorDtos;
            }


        }       
        private async Task<int> GetNumberOfDoctorsOnDutyAsync()
        {
            string url = "originator/number-of-doctors-on-duty";

            using (var response = await HttpClient.GetAsync(url, 
                HttpCompletionOption.ResponseHeadersRead))
            {
                await EnsureResponseIsSuccessful(response);

                var stream = await response.Content.ReadAsStreamAsync();
                int numberOfDoctors = stream.ReadAndDeserializeFromJson<int>();

                return numberOfDoctors;

            }

            

        }

        private async Task<int> GetPopulationSizeAsync()
        {
            string url = "originator/population-size";
            
            using (var response = await HttpClient.GetAsync(url,
                HttpCompletionOption.ResponseHeadersRead))
            {
                await EnsureResponseIsSuccessful(response);

                var stream = await response.Content.ReadAsStreamAsync();
                int populationSize = stream.ReadAndDeserializeFromJson<int>();

                return populationSize;
            }
            
        }

        private async Task<int> GetTotalNumberOfDoctorsAsync()
        {
            string url = "originator/total-number-of-doctors";
            
            using (var response = await HttpClient.GetAsync(url, 
                HttpCompletionOption.ResponseHeadersRead))
            {
                await EnsureResponseIsSuccessful(response);

                var stream = await response.Content.ReadAsStreamAsync();

                int totalNumberOfDoctors = stream.ReadAndDeserializeFromJson<int>();

                return totalNumberOfDoctors;
            }
            
        }

        private async Task<List<string>> GetDoctorIdsAsync()
        {
            string url = "originator/doctor-ids";
            
            using (var response = await HttpClient.GetAsync(url, 
                HttpCompletionOption.ResponseHeadersRead))
            {
                await EnsureResponseIsSuccessful(response);

                var stream = await response.Content.ReadAsStreamAsync();

                List<string> doctorIds = stream.ReadAndDeserializeFromJson<List<string>>();

                return doctorIds;
            }
        
        }

        private async Task<List<DayOffDto>> GetDaysOffAsync()
        {
            string url = "originator/days-off?dateFilter=" + _date;
            
            using (var response = await HttpClient.GetAsync(url, 
                HttpCompletionOption.ResponseHeadersRead))
            {
                await EnsureResponseIsSuccessful(response);

                var stream = await response.Content.ReadAsStreamAsync();

                List<DayOffDto> daysOff = stream.ReadAndDeserializeFromJson<List<DayOffDto>>();

                return daysOff;
            }
            
        }

        private async Task<List<DateTime>> GetPublicHolidaysForMonthAsync()
        {
            string url = "originator/public-holidays?year=" + _date.Year + "&month=" +
                         _date.Month;
            
            using (var response = await HttpClient.GetAsync(url, 
                HttpCompletionOption.ResponseHeadersRead))
            {
                await EnsureResponseIsSuccessful(response);

                var stream = await response.Content.ReadAsStreamAsync();

                List<DateTime> publicHolidays = stream.ReadAndDeserializeFromJson<List<DateTime>>();

                return publicHolidays;
            }

        }

        private async Task<List<DutyRequirementForMonthDto>> GetDutyRequirementsForMonthAsync()
        {
            string url = "originator/duty-requirements-for-month?year=" + _date.Year +
                         "&month=" + _date.Month;
            
            using (var response = await HttpClient.GetAsync(url, 
                HttpCompletionOption.ResponseHeadersRead))
            {
                await EnsureResponseIsSuccessful(response);

                var stream = await response.Content.ReadAsStreamAsync();

                List<DutyRequirementForMonthDto> dutyRequirements =
                    stream.ReadAndDeserializeFromJson<List<DutyRequirementForMonthDto>>();

                return dutyRequirements;
            }
            
        }

        private async Task<List<string>> GetDoctorsOnDutyLastDayOfPreviousMonthAsync()
        {
            string url = "originator/doctors-on-duty-on-last-day-previous-month?=" +
                         _date.Year + "&month=" + _date.Month;
            
            using (var response = await HttpClient.GetAsync(url, 
                HttpCompletionOption.ResponseHeadersRead))
            {
                await EnsureResponseIsSuccessful(response);

                var stream = await response.Content.ReadAsStreamAsync();

                List<string> doctors = stream.ReadAndDeserializeFromJson<List<string>>();

                return doctors;
            }
            
        }

        private async Task<List<DayOffOnLastDayPreviousMonthDto>> GetDaysOffOnLastDayOfPreviousMonthAsync()
        {
            string url = "originator/days-off-on-last-day-previous-month?=" +
                         _date.Year + "&month=" + _date.Month;
           
            using (var response = await HttpClient.GetAsync(url, 
                HttpCompletionOption.ResponseHeadersRead))
            {
                await EnsureResponseIsSuccessful(response);

                var stream = await response.Content.ReadAsStreamAsync();

                List<DayOffOnLastDayPreviousMonthDto> daysOff = stream.ReadAndDeserializeFromJson<List<DayOffOnLastDayPreviousMonthDto>>();

                return daysOff;
            }
            
        }

        private async Task EnsureResponseIsSuccessful(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorStream = await response.Content.ReadAsStreamAsync();

                var error = await errorStream.ToStringAsync();

                throw new ApiException()
                {
                    Content = error
                };

            }
        }

        private void SetDaysOff()
        {
            Parallel.For(0, _populationSize, (i) =>
            {
                Individual individual = new Individual
                {
                    Dna = new IAllel[_totalNumberOfDoctors, DateTime.DaysInMonth(_date.Year, _date.Month)]
                };

                for (int j = 0; j < _totalNumberOfDoctors; j++)
                {
                    int index = j;
                    var daysOff = _daysOff.FindAll(d => d.UserId == _doctorIds[index]).Select(d => d.Date.Day).ToList();

                    if (daysOff.Count == 0)
                    {
                        continue;
                    }
                    else
                    {
                        for (int k = 0; k < DateTime.DaysInMonth(_date.Year, _date.Month); k++)
                        {
                            if (daysOff.Contains(k + 1))
                            {
                                individual.Dna[j, k] = Allel.DayOff;
                            }
                        }
                    }

                }

                bool lockTaken = false;

                try
                {
                    _spinLock.Enter(ref lockTaken);
                    _individuals.Add(individual);
                }
                finally
                {
                    if (lockTaken)
                    {
                        _spinLock.Exit();
                    }
                }



            });
        }

        private void SetDuties()
        {
            Parallel.ForEach(_individuals, (individual) =>
            {
                List<DutyRequirementForMonthDto> dutyRequirementsForMonth =
                    new List<DutyRequirementForMonthDto>(_dutyRequirementsForMonth);
                List<string> doctorsAvailableForSchedule = new List<string>(_doctorIds);

                for (int i = 0; i < DateTime.DaysInMonth(_date.Year, _date.Month); i++)
                {

                    // 1. Usuwamy z listy doctorsAvailableForSchedule lekarzy, którzy wyczerpali dostępną ilość dyżurów

                    foreach (var doctorId in new List<string>(doctorsAvailableForSchedule))
                    {
                        var doctorDutyRequirements = dutyRequirementsForMonth.FirstOrDefault(dr => dr.UserId == doctorId);

                        if (doctorDutyRequirements != null)
                        {
                            if (doctorDutyRequirements.RequiredTotalDutiesInMonth == 0)
                            {
                                doctorsAvailableForSchedule.Remove(doctorId);
                            }
                        }
                    }

                    // 2. Jeżeli nadal na liście doctorsAvailableForSchedule jest dostępny lekarz kontunuujemy

                    if (doctorsAvailableForSchedule.Count > 0)
                    {
                        // 2. Tworzymy z listy doctorsAvailableForSchedule początkową pulę lekarzy dostępnych danego dnia (doctorsAvailableForDay)   

                        List<string> doctorsAvailableForDay = new List<string>(doctorsAvailableForSchedule);

                        // 3. Usuwamy lekarzy, którzy zgłosili niemożność lub mieli dyżur pooprzedniego dnia 

                        for (int j = i; j < i + 1; j++)
                        {
                            for (int k = 0; k < _totalNumberOfDoctors; k++)
                            {
                                for (int m = j; m <= j; m++)
                                {
                                    if (individual.Dna[k, m] == Allel.DayOff)
                                    {
                                        doctorsAvailableForDay.Remove(_doctorIds[k]);
                                        continue;
                                    }

                                    // poprzedni dzień

                                    if (m > 0)
                                    {
                                        if (individual.Dna[k, m - 1] == Allel.OnDuty)
                                        {
                                            doctorsAvailableForDay.Remove(_doctorIds[k]);
                                            continue;
                                        }
                                    }

                                    // ostatni dzień poprzedniego miesiąca

                                    if (m == 0 && _doctorsOnDutyLastDayOfPreviousMonth.Count > 0)
                                    {
                                        if (_doctorsOnDutyLastDayOfPreviousMonth.Contains(_doctorIds[k]))
                                        {
                                            doctorsAvailableForDay.Remove(_doctorIds[k]);
                                        }
                                    }
                                }
                            }
                        }

                        DateTime currentDate = new DateTime(_date.Year, _date.Month, i + 1);

                        if (doctorsAvailableForDay.Count > 0)
                        {
                            // 4.Jeżeli w danym dniu przypada święto, usuwamy lekarza z listy dostępnych w tym dniu jeśli lekarz wyczerpał limit dostępności w święta(Item3)

                            if (IsWeekendOrPublicHoliday(currentDate))
                            {
                                foreach (var doctorId in new List<string>(doctorsAvailableForDay))
                                {
                                    var doctorDutyRequirements =
                                        dutyRequirementsForMonth.FirstOrDefault(dr => dr.UserId == doctorId);

                                    if (doctorDutyRequirements != null)
                                    {
                                        if (doctorDutyRequirements.RequiredTotalHolidayDuties == 0)
                                        {
                                            doctorsAvailableForDay.Remove(doctorId);
                                        }
                                    }
                                }
                                
                            }



                            // 5. Jeżeli dzień nie jest świąteczny, usuwamy lekarza z listy dostępnych w tym dniu jeśi lekarz wyczerpał limit dostepności w dni robocze (Item2)

                            if (!IsWeekendOrPublicHoliday(currentDate))
                            {

                                foreach (var doctorId in new List<string>(doctorsAvailableForDay))
                                {
                                    var doctorDutyRequirements =
                                        dutyRequirementsForMonth.FirstOrDefault(dr => dr.UserId == doctorId);

                                    if (doctorDutyRequirements != null)
                                    {
                                        if (doctorDutyRequirements.RequiredTotalWeekdayDuties == 0)
                                        {
                                            doctorsAvailableForDay.Remove(doctorId);
                                        }
                                    }
                                }


                            }

                            // 6. Dodajemy lekarzy na dyżur. Odniesieniem jest indeks na liście _doctorsId i wiersze tablicy

                            for (int t = 0; t < _numberOfDoctorsOnDuty; t++)
                            {
                                if (doctorsAvailableForDay.Count > 0)
                                {
                                    var doctor =
                                        doctorsAvailableForDay[
                                            RandomGenerator.IntBetween(0, doctorsAvailableForDay.Count)];

                                    int index = _doctorIds.IndexOf(doctor);
                                    individual.Dna[index, i] = Allel.OnDuty;

                                    var doctorDutyRequirements =
                                        dutyRequirementsForMonth.FirstOrDefault(d => d.UserId == doctor);

                                    if (doctorDutyRequirements != null)
                                    {
                                        int requiredTotalDuties;
                                        int requiredWeekdayDuties;
                                        int requiredHolidayDuties;

                                        if (IsWeekendOrPublicHoliday(currentDate))
                                        {
                                            requiredTotalDuties = doctorDutyRequirements.RequiredTotalDutiesInMonth - 1;
                                            requiredWeekdayDuties = doctorDutyRequirements.RequiredTotalWeekdayDuties;
                                            requiredHolidayDuties = doctorDutyRequirements.RequiredTotalHolidayDuties - 1;
                                        }
                                        else
                                        {
                                            requiredTotalDuties = doctorDutyRequirements.RequiredTotalDutiesInMonth - 1;
                                            requiredWeekdayDuties = doctorDutyRequirements.RequiredTotalWeekdayDuties - 1;
                                            requiredHolidayDuties = doctorDutyRequirements.RequiredTotalHolidayDuties;
                                        }

                                        DutyRequirementForMonthDto updated = new DutyRequirementForMonthDto
                                        {
                                            UserId = doctorDutyRequirements.UserId,
                                            RequiredTotalDutiesInMonth = requiredTotalDuties,
                                            RequiredTotalWeekdayDuties = requiredWeekdayDuties,
                                            RequiredTotalHolidayDuties = requiredHolidayDuties
                                        };

                                        dutyRequirementsForMonth.Remove(doctorDutyRequirements);
                                        dutyRequirementsForMonth.Add(updated);

                                    }

                                    doctorsAvailableForDay.Remove(doctor);

                                }
                                else
                                {
                                    break;
                                }
                            }


                        }


                    }
                    else
                    {
                        // 7. Jeżeli nie ma lekarzy z dostępnymi dniami dyżurowymi w grafiku (doctorsAvailableForScheduler) przerywamy pętlę
                        break;
                    }



                }
            });
        }

        private bool IsWeekendOrPublicHoliday(DateTime currentDate)
        {
            bool isPublicHoliday = _publicHolidaysForMonth.Exists(d => d.Day == currentDate.Day);

            bool isWeekend = currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday;

            return isPublicHoliday || isWeekend;
        }

        private bool IsTotalDoctorsGreaterOrEqualToNumberOfDoctorsOnDutyTimesDutyInterval(int dutyInterval = 2)
        {
            return _totalNumberOfDoctors >= _numberOfDoctorsOnDuty * dutyInterval;
        }

        private bool IsTotalDaysOffOnADayGreaterThanAllowedMaxDaysOffOnSingleDay(int totalDaysOffOnADay)
        {

            return totalDaysOffOnADay > AllowedMaxDaysOffOnSingleDay;

        }

        private int CalculateDifferenceBetweenTotalDutiesAndTotalDutyRequirements()
        {
            int totalDutiesInMonth = DateTime.DaysInMonth(_date.Year, _date.Month);

            int totalDutyRequirements = _dutyRequirementsForMonth.Select(dr => dr.RequiredTotalDutiesInMonth).Sum();

            return totalDutiesInMonth - totalDutyRequirements;
        }

        private void RemoveRedundantAlleles(IAllel[,] dna, int columnIndex, List<int> rowsToChooseFromForRemoval)
        {
            while (rowsToChooseFromForRemoval.Count > _numberOfDoctorsOnDuty)
            {
                int row = rowsToChooseFromForRemoval[RandomGenerator.IntBetween(0, rowsToChooseFromForRemoval.Count)];
                dna[row, columnIndex] = null;
                rowsToChooseFromForRemoval.Remove(row);

            }
        }

        private void InsertMissingAllels(IAllel[,] dna, int columnIndex, int onDutyCount, List<int> inaccessibleRows)
        {
            List<int> availableDoctors = new List<int>();

            for (int i = 0; i < _totalNumberOfDoctors; i++)
            {
                if (inaccessibleRows.Contains(i))
                {
                    continue;
                }

                availableDoctors.Add(i);
            }

            int numberOfMissingAllels = _numberOfDoctorsOnDuty - onDutyCount;

            for (int i = 0; i < numberOfMissingAllels; i++)
            {
                if (availableDoctors.Count > 0)
                {
                    var doctor = availableDoctors[RandomGenerator.IntBetween(0, availableDoctors.Count)];
                    dna[doctor, columnIndex] = Allel.OnDuty;
                    availableDoctors.Remove(doctor);
                }
            }


        }


    }
}
