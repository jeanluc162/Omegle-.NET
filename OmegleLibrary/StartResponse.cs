using System;

namespace OmegleLibrary
{
    internal class StartResponse
    {
        public EventsResponse events {get;set;}
        public String clientID {get;set;}
        public StatusResponse statusInfo {get;set;}
    }
}