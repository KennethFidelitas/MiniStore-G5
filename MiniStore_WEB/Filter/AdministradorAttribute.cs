using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProgramacionAvanzadaWebProyecto.Filter
{
    public class AdministradorAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;

            if (session.GetString("Autenticado") != "1")
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            var esAdministrador =
                session.GetInt32("ConsecutivoRol") == 2 ||
                string.Equals(
                    session.GetString("NombreRol"),
                    "Administrador",
                    StringComparison.OrdinalIgnoreCase);

            if (!esAdministrador)
            {
                context.Result = new RedirectToActionResult(
                    "AccesoDenegado",
                    "Account",
                    null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
