using AlvTime.Business.TimeRegistration;
using AlvTime.Persistence.Repositories;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.IO;



namespace SkattefunnRydder
{

    public class SkattefunnEntry: CreateTimeEntryDto
    {
        public int UserId { get; set; } 
    }
    public class SkattefunnProgram
    {
        private TimeRegistrationStorage _timeRegistrationStorage;
        public SkattefunnProgram(TimeRegistrationStorage timeRegistrationStorage)
        {
            _timeRegistrationStorage = timeRegistrationStorage;
        }

        public async Task RunSkattefunnProgram()
        {
            var timeEntries = new List<SkattefunnEntry>();


            using (var reader = new StreamReader(@"C:\Users\leno\source\repos\alvtime\packages\api\SkattefunnRydder\data.csv"))
                { 
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split('\t');
                    if (values.Length == 4)
                    {  
                        string format = "M/d/yyyy h:mm";
                        DateTime parsedDate = new DateTime();
                        IFormatProvider provider = CultureInfo.CurrentCulture;
                        try
                        {
                            parsedDate = DateTime.ParseExact(values[3], format, provider);
                        }
                        catch (FormatException ex)
                        {
                            Console.WriteLine(ex.Message);
                        }


                        timeEntries.Add(new SkattefunnEntry { 
                            Date = parsedDate,
                            TaskId = int.Parse(values[0]),
                            UserId =  int.Parse(values[2]) ,
                            Value  = decimal.Parse(values[1])

                        });
                    }
                    }
                }

            foreach (var timeEntry in timeEntries)
            {
                if(timeEntry.TaskId == 199)
                    continue;
                var oldRowCriterias = new TimeEntryQuerySearch
                {
                    UserId = timeEntry.UserId,
                    FromDateInclusive = timeEntry.Date.Date,
                    ToDateInclusive = timeEntry.Date.Date,
                    TaskId = timeEntry.TaskId
                };
                var uvaerRowCriterias = new TimeEntryQuerySearch
                {
                    UserId = timeEntry.UserId,
                    FromDateInclusive = timeEntry.Date.Date,
                    ToDateInclusive = timeEntry.Date.Date,
                    TaskId = 199
                };
                

                var oldRow = await _timeRegistrationStorage.GetTimeEntry(oldRowCriterias);
                var uvaerRow = await _timeRegistrationStorage.GetTimeEntry(uvaerRowCriterias);
                if (uvaerRow == null)
                {
                    var newRow = new SkattefunnEntry
                    {
                        UserId = timeEntry.UserId,
                        Date = timeEntry.Date,
                        Value = timeEntry.Value,
                        TaskId = 199
                    };
                    await _timeRegistrationStorage.CreateTimeEntry(newRow, timeEntry.UserId);
                }
                else
                {
                    var newRow = new SkattefunnEntry
                    {
                        UserId = timeEntry.UserId,
                        Date = timeEntry.Date,
                        Value = timeEntry.Value + uvaerRow.Value,
                        TaskId = 199
                    };
                    await _timeRegistrationStorage.UpdateTimeEntry(newRow, timeEntry.UserId);
                }

                var oldRowUpdate = new SkattefunnEntry
                {
                    UserId = timeEntry.UserId,
                    Date = oldRow.Date,
                    Value = oldRow.Value - timeEntry.Value,
                    TaskId = oldRow.TaskId
                };
                await _timeRegistrationStorage.UpdateTimeEntry(oldRowUpdate, timeEntry.UserId);
            }
        }
    }
}
