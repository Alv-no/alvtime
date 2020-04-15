using AlvTimeWebApi.Dto;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests
{
    public class ExampleUT
    {
        [Fact]
        public void Test1_Passes()
        {
            var list = new List<CreateTimeEntryDto>();

            var dto = new CreateTimeEntryDto
            {
                Date = DateTime.UtcNow,
                TaskId = 3,
                Value = 3
            };

            list.Add(dto);
        }
    }
}
