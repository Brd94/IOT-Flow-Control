using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace WEBServer.Client.ViewModel
{
    public interface ILoginViewModel
    {
        public string EmailAddress { get; set; }
        public string Password { get; set; }

        public Task LoginUser();
        
    }
}