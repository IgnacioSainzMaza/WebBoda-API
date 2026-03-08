using System;
using System.Collections.Generic;
using System.Text;

namespace WebBoda.Domain.Enums
{
    /// <summary>
    /// Representa la opción de autobús de vuelta elegida por el invitado.
    /// Se almacena como int en base de datos (EF Core lo convierte automáticamente).
    /// </summary>
    public enum TipoAutobusVuelta
    {
        NoCogeAutobus = 0,
        PrimerServicio = 1,
        SegundoServicio = 2
    }

}
