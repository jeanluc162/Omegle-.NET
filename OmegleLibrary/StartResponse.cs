using System;
using System.Collections.Generic;

namespace OmegleLibrary
{
    internal class StartResponse
    {
        public List<List<String>> events {get;set;}
        public String clientID {get;set;}
        public StatusResponse statusInfo {get;set;}
    }
}