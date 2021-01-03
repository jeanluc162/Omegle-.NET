using System;

namespace OmegleLibrary
{
    internal class StatusResponse
    {
        public Int32 count {get;set;}
        public Boolean force_unmon {get;set;}
        public Double spyQueueTime {get;set;}
        public Double spyeeQueueTime {get;set;}
        public String[] servers {get;set;}
    }
}