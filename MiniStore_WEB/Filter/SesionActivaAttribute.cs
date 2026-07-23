using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProgramacionAvanzadaWebProyecto.Filter
{
    public class SesionActivaAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var autenticado = context.HttpContext.Session.GetString("Autenticado");

            if (autenticado != "1")
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            base.OnActionExecuting(context);
        }

    }
}
