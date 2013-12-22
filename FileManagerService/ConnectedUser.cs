using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagerService
{
    public class ConnectedUser
    {
        public User User { get; set; }
        public string CurrentDirectory { get; set; }
        public string SessionId { get; set; }
        public IClientNotification CallbackChannel { get; set; }
    }
}
