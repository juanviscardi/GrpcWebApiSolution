using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

public enum Action
{
    Create,
    Delete,
    Modify,
    Retrive
}

public enum Status
{
    Ok,
    Error
}

namespace Common
{
    public class Log
    {
        public string Mensaje { get; set; }
        public DateTime Date { get; set; }
        public Action Action { get; set; }
        public Status Status { get; set; }
        public string UserName { get; set; }

        public Log(string mensaje, Action action, string userName, Status status = Status.Ok)
        {
            this.Mensaje = mensaje;
            this.Action = action;
            this.Date = DateTime.Now;
            this.Status = status;
            this.UserName = userName;
        }

        public Log(string message)
        {
            List<string> partes = message.Split(ProtocolSpecification.fieldsSeparator).ToList();
            if (partes.Count >= 5)
            {
                DateTime date;
                if (DateTime.TryParse(partes[0], out date))
                    this.Date = date;

                Action action;
                if (Enum.TryParse(partes[1], out action))
                    this.Action = action;

                Status status;
                if (Enum.TryParse(partes[2], out status))
                    this.Status = status;

                this.UserName = partes[3];
                this.Mensaje = partes[4];
            }
        }

        public override string ToString()
        {
            return this.Date.ToString() + ProtocolSpecification.fieldsSeparator +
                    this.Action + ProtocolSpecification.fieldsSeparator +
                    this.Status + ProtocolSpecification.fieldsSeparator +
                    this.UserName + ProtocolSpecification.fieldsSeparator +
                    this.Mensaje;
        }

        public string ToStringListar()
        {
            return "Fecha: " + this.Date.ToString() +
                    "; Action: " + (Action)this.Action +
                    "; Status: " + (Status)this.Status +
                    "; Usuario: " + this.UserName +
                    "; Mensaje: " + this.Mensaje;
        }
    }
}
