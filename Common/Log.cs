using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public Log(string mensaje, Action action, Status status = Status.Ok)
        {
            this.Mensaje = mensaje;
            this.Action = action;
            this.Date = DateTime.Now;
            this.Status = status;
        }

        public override string ToString()
        {
            return "Fecha: " + this.Date.ToString() + "; Action: " + (Action)this.Action + "; Status: " + (Status)this.Status+ "; Mensaje: " + this.Mensaje;
        }
    }
}
