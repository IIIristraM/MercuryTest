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
        //соответствующий пользователь файловой системы
        public User User { get; set; }

        //текущий рабочий каталог
        public string CurrentDirectory { get; set; }

        //идентификатор сессии 
        public string SessionId { get; set; }

        //объект для оповещения клиента
        public IClientNotification CallbackChannel { get; set; }
    }
}
