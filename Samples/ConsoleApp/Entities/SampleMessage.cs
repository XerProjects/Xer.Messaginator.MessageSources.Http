using System;

namespace ConsoleApp.HttpMessageSource.Entities
{
    public class SampleMessage
    {
        public int Id { get; set; }
        public string Message { get; set; }

        public SampleMessage() { Id = GetHashCode(); }
    }
}